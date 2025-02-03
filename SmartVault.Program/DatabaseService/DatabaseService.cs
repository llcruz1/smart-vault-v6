using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace SmartVault.Program
{
    public class DatabaseService : IDatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(IConfiguration configuration)
        {
            var databaseFileName = configuration["DatabaseFileName"];
            if (string.IsNullOrWhiteSpace(databaseFileName) || !File.Exists(databaseFileName))
            {
                throw new FileNotFoundException($"Database file '{databaseFileName}' not found.");
            }

            _connectionString = string.Format(
                configuration["ConnectionStrings:DefaultConnection"] ?? "",
                databaseFileName
            );
        }

        private SQLiteConnection CreateConnection()
        {
            var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            return connection;
        }

        public List<string> GetAllFilePaths()
        {
            using var connection = CreateConnection();
            return connection.Query<string>("SELECT FilePath FROM Document").ToList();
        }

        public List<string> GetEveryThirdFilePathContainingText(string accountId, string searchText)
        {
            using var connection = CreateConnection();
            var query = @"
                SELECT FilePath 
                FROM (
                    SELECT FilePath, ROW_NUMBER() OVER (PARTITION BY AccountId ORDER BY Id) AS RowNum
                    FROM Document
                    WHERE AccountId = @AccountId
                )
                WHERE RowNum % 3 = 0;
            ";

            return connection.Query<string>(query, new { AccountId = accountId })
                .Where(filePath => File.Exists(filePath) && File.ReadAllText(filePath).Contains(searchText))
                .ToList();
        }
    }
}
