using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace SmartVault.Program
{
    partial class Program
    {
        private static SQLiteConnection _connection;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide an account ID as an argument.");
                return;
            }

            InitializeDatabaseConnection();

            WriteEveryThirdFileToFile(args[0]);
            GetAllFileSizes();

            DisposeDatabaseConnection();
        }

        private static void InitializeDatabaseConnection()
        {
            if (_connection != null)
            {
                Console.WriteLine("Database connection is already open.");
                return;
            }

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("SmartVault.Program/appsettings.json").Build();

            var databaseFileName = configuration?["DatabaseFileName"];
            Console.WriteLine($"Checking database: {databaseFileName}");

            if (!File.Exists(databaseFileName))
            {
                Console.WriteLine($"Database file '{databaseFileName}' does not exist. Consider running the data generation tool first.");
                throw new FileNotFoundException($"Database file '{databaseFileName}' not found.");
            }

            _connection = new SQLiteConnection(
                string.Format(
                    configuration?["ConnectionStrings:DefaultConnection"] ?? "", 
                    databaseFileName
                )
            );

            _connection.Open();

            Console.WriteLine($"Connected to database: {configuration?["DatabaseFileName"]}");
        }

        private static void DisposeDatabaseConnection()
        {
            if (_connection != null)
            {
                _connection.Dispose();
                Console.WriteLine("Database connection closed.");
            }
        }

        private static void GetAllFileSizes()
        {
            var query = "SELECT FilePath FROM Document";
            var files = _connection.Query<string>(query).ToList();

            Console.WriteLine($"Calculating total size of all files. Number of files: {files.Count}");

            long totalSize = 0;
            foreach (var filePath in files)
            {
                if (File.Exists(filePath))
                {
                    var fileInfo = new FileInfo(filePath);
                    totalSize += fileInfo.Length;
                }
            }

            Console.WriteLine($"Total size of all files: {totalSize} bytes");
        }

        private static void WriteEveryThirdFileToFile(string accountId)
        {
            var query = @"
                SELECT FilePath 
                FROM (
                    SELECT FilePath, ROW_NUMBER() OVER (PARTITION BY AccountId ORDER BY Id) AS RowNum
                    FROM Document
                    WHERE AccountId = @AccountId
                )
                WHERE RowNum % 3 = 0;
            ";

            var files = _connection.Query(query, new { AccountId = accountId })
                .Where(doc => File.ReadAllText(doc.FilePath).Contains("Smith Property"))
                .ToList();

            if (files.Count == 0)
            {
                Console.WriteLine("No files contain the text 'Smith Property'.");
                return;
            }

            string outputFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"Account_{accountId}_ThirdFiles.txt");
            using (var outputFile = new StreamWriter(outputFilePath))
            {
                foreach (var file in files)
                {
                    string fileContent = File.ReadAllText(file.FilePath);
                    outputFile.WriteLine(fileContent);
                }
            }

            Console.WriteLine($"Contents of every third file containing 'Smith Property' have been written to {outputFilePath}");
        }
    }
}