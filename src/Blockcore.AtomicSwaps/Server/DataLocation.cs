using Blockcore.Networks;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using System.Runtime.InteropServices;

namespace Blockcore.AtomicSwaps.Server
{
	public class DataConfigOptions
	{
		public string DirectoryPath { get; set; }
		public bool UseDefaultPath { get; set; }
	}

	public static class CreteDataLocation
	{
		public static void CreateDataLocation(this IApplicationBuilder app)
		{
			var options = app.ApplicationServices.GetService<IOptions<DataConfigOptions>>();

			if (options.Value.UseDefaultPath)
			{
				options.Value.DirectoryPath = CreateDefaultDataDirectories("AtomicSwaps");
			}
			else if(!string.IsNullOrEmpty(options?.Value.DirectoryPath))
			{
				var directory = Path.Combine(options.Value.DirectoryPath, "AtomicSwaps");
				options.Value.DirectoryPath = Directory.CreateDirectory(directory).FullName;
			}
			else
			{
				options.Value.DirectoryPath = Path.Combine(Directory.GetCurrentDirectory());//, "AtomicSwaps") ;
			}
		}

		private static string CreateDefaultDataDirectories(string appName)
		{
			string directoryPath;

			// Directory paths are different between Windows or Linux/OSX systems.
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				string home = Environment.GetEnvironmentVariable("HOME");
				if (!string.IsNullOrEmpty(home))
				{
					Console.WriteLine("Using HOME environment variable for initializing application data.");
					directoryPath = Path.Combine(home, "." + appName.ToLowerInvariant());
				}
				else
				{
					throw new DirectoryNotFoundException("Could not find HOME directory.");
				}
			}
			else
			{
				string localAppData = Environment.GetEnvironmentVariable("APPDATA");
				if (!string.IsNullOrEmpty(localAppData))
				{
					Console.WriteLine("Using APPDATA environment variable for initializing application data.");
					directoryPath = Path.Combine(localAppData, appName);
				}
				else
				{
					throw new DirectoryNotFoundException("Could not find APPDATA directory.");
				}
			}

			// Create the data directories if they don't exist.
			Directory.CreateDirectory(directoryPath);

			Console.WriteLine("Data directory initialized with path {0}.", directoryPath);
			return directoryPath;
		}

	}
}
