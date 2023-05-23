#nullable enable
using Blockcore.AtomicSwaps.Server.Controllers;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;
using Blockcore.AtomicSwaps.Shared;

namespace Blockcore.AtomicSwaps.Server.Services
{
    public interface IStorageService
    {
        Task<IEnumerable<SwapSession>> Get();
        Task<IEnumerable<SwapSession>> Post(List<string> pubKeys);

        Task<SwapSession?> Get(string session);
        Task Add(SwapSession swap);
        Task Update(SwapSession swap);
        Task Complete(SwapSession swap);
    }

    public class SwapContext : DbContext
    {
        public DbSet<SwapSession> Swaps { get; set; }
        public string DbPath { get; }

        public SwapContext(string path)
        {
            DbPath = path;
        }

        // The following configures EF to create a Sqlite database file in the
        // special "local" folder for your platform.
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Owned<SwapSessionCoin>();
        }
    }

    public class StorageService : IStorageService
    {
        private readonly string dbPath;
        private readonly string dbConnection;

        //private SwapContext swapContext;

        public StorageService(IOptions<DataConfigOptions> options)
        {
            dbPath = Path.Combine(options.Value.DirectoryPath, $"swaps.db");

            using var swapContext = new SwapContext(dbPath);
            swapContext.Database.EnsureCreated();
        }


        //get public swaps
        public async Task<IEnumerable<SwapSession>> Get()
        {
            await using var swapContext = new SwapContext(dbPath);

            var swaps = swapContext.Swaps
                .Where(s => (s.Status == SwapsDataStatus.Available || s.Status == SwapsDataStatus.InProgress) && !s.IsPrivate)
                .OrderByDescending(s => s.Created);

            return swaps.ToList();
        }

        //post pubKeys and get public and private swaps
        public async Task<IEnumerable<SwapSession>> Post(List<string> pubKeys)
        {
            await using var swapContext = new SwapContext(dbPath);

            var swaps = await swapContext.Swaps
                 .Where(s => (s.Status == SwapsDataStatus.Available || s.Status == SwapsDataStatus.InProgress)
                  && (!s.IsPrivate || pubKeys.Contains(s.SwapTaker.SenderPubkey) || pubKeys.Contains(s.SwapMaker.SenderPubkey)))
                 .OrderByDescending(s => s.Created)
                 .ToListAsync();

            return swaps;
        }

        public async Task<SwapSession?> Get(string session)
        {
            await using var swapContext = new SwapContext(dbPath);

            return await swapContext.Swaps.SingleOrDefaultAsync(s => s.SwapSessionId == session);
        }

        public async Task Add(SwapSession swap)
        {
            await using var swapContext = new SwapContext(dbPath);

            swapContext.Swaps.Add(swap);
            await swapContext.SaveChangesAsync();
        }

        public async Task Update(SwapSession swap)
        {
            await using var swapContext = new SwapContext(dbPath);
            swapContext.Update(swap);
            await swapContext.SaveChangesAsync();
        }

        public async Task Complete(SwapSession swap)
        {
            await using var swapContext = new SwapContext(dbPath);
            swap.Status = SwapsDataStatus.Complete;
            swapContext.Update(swap);
            await swapContext.SaveChangesAsync();
        }
    }
}