using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SmartVault.Program
{
    public partial class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide an account ID as an argument.");
                return;
            }

            var serviceProvider = ConfigureServices();
            var databaseService = serviceProvider.GetRequiredService<IDatabaseService>();

            WriteEveryThirdFileToFile(args[0], databaseService);
            GetAllFileSizes(databaseService);
        }

        private static ServiceProvider ConfigureServices()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json").Build();

            return new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .AddSingleton<IDatabaseService, DatabaseService>()
                .BuildServiceProvider();
        }

        public static long GetAllFileSizes(IDatabaseService databaseService)
        {
            var files = databaseService.GetAllFilePaths();

            Console.WriteLine($"Calculating total size of all files. Number of files: {files.Count}");

            long totalSize = 0;
            foreach (var filePath in files)
            {
                if (File.Exists(filePath))
                {
                    totalSize += new FileInfo(filePath).Length;
                }
            }

            Console.WriteLine($"Total size of all files: {totalSize} bytes");

            return totalSize;
        }

        public static void WriteEveryThirdFileToFile(string accountId, IDatabaseService databaseService)
        {
            var files = databaseService.GetEveryThirdFilePathContainingText(accountId, "Smith Property");

            if (files.Count == 0)
            {
                Console.WriteLine("No files contain the text 'Smith Property'.");
                return;
            }

            string outputFilePath = Path.Combine(AppContext.BaseDirectory, $"Account_{accountId}_ThirdFiles.txt");
            using (var outputFile = new StreamWriter(outputFilePath))
            {
                foreach (var filePath in files)
                {
                    outputFile.WriteLine(File.ReadAllText(filePath));
                }
            }

            Console.WriteLine($"Contents of every third file containing 'Smith Property' have been written to {outputFilePath}");
        }
    }
}
