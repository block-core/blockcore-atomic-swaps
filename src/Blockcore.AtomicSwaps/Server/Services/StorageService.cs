using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blockcore.AtomicSwaps.Shared;
using Blockcore.Configuration;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;

namespace Blockcore.AtomicSwaps.Server.Services
{
	public interface IStorageService
	{
		Task<IEnumerable<SwapsData>> Get();
		Task<IEnumerable<SwapsData>> Get(string session);
		Task Add(SwapsData swap);
		Task Complete(SwapsData swap);
	}

	public class SwapsData
	{
		public string Session { get; set; }
		public int Version { get; set; }
		public string Data { get; set; }
		public bool Active { get; set; } = true;
	}

	public class StorageService : IStorageService
	{
		private readonly string dbPath;
		private readonly string dbConnection;

		public StorageService(IOptions<DataConfigOptions> options)
        {
			this.dbPath = Path.Combine(options.Value.DirectoryPath, $"swaps.db");
			this.dbConnection = "Data Source=" + this.dbPath;

			if (!File.Exists(this.dbPath))
			{
				this.Setup();
			}
		}

		protected SqliteConnection GetDbConnection()
		{
			return new SqliteConnection(this.dbConnection);
		}

		public void Setup()
		{
			using var connection = this.GetDbConnection();

			var table = connection.Query<string>("SELECT name FROM sqlite_master WHERE type='table' AND name = 'Swaps';");
			var tableName = table.FirstOrDefault();
			if (!string.IsNullOrEmpty(tableName) && tableName == "Swaps")
				return;

			connection.Execute("Create Table Swaps (" +
			                   "Session VARCHAR(100) NOT NULL," +
							   "Version INTEGER NOT NULL," +
			                   "Active BOOLEAN NOT NULL," +
							   "Data VARCHAR(5000) NULL);");
		}

		public async Task<IEnumerable<SwapsData>> Get()
		{
			await using var connection = this.GetDbConnection();

			return await connection.QueryAsync<SwapsData>("SELECT * FROM Swaps;");
		}

		public async Task<IEnumerable<SwapsData>> Get(string session)
		{
			await using var connection = this.GetDbConnection();

			return await connection.QueryAsync<SwapsData>("SELECT * FROM Swaps WHERE Session = '@Session';", session);
		}

		public async Task Add(SwapsData swap)
		{
			await using var connection = this.GetDbConnection();

			var swaps = await this.Get(swap.Session);
			var lastSwap = swaps.MaxBy(o => o.Version);

			// increment the version
			if (lastSwap != null)
				swap.Version = lastSwap.Version + 1;

			// mark all previous versions as complete
			await this.Complete(swap);

			swap.Active = true;

			// create new version
			await connection.ExecuteAsync("INSERT INTO Swaps (Session, Version, Data)" +
			                              "VALUES (@Session, @Version, @Data, @Active);", swap);
		}

		public async Task Complete(SwapsData swap)
		{
			await using var connection = this.GetDbConnection();

			await connection.ExecuteAsync("Update Swaps Set Active = 0 Where Session = @Session", swap);
		}
	}
}