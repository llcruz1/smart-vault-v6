using System;
using System.Data.SQLite;
using System.IO;
using Dapper;
using Xunit;

namespace SmartVault.DataGeneration.Tests
{
    public class DataGenerationTest : IDisposable
    {
        private const string TEST_DB_FILENAME = "testdb_test.sqlite";

        public DataGenerationTest()
        {
            if (File.Exists(TEST_DB_FILENAME))
            {
                File.Delete(TEST_DB_FILENAME);
            }

            CreateAppSetttingsTestFile();
        }

        private static void CreateAppSetttingsTestFile()
        {
            File.WriteAllText("appsettings.json", $@"
            {{
                ""ConnectionStrings"": {{
                    ""DefaultConnection"": ""Data Source={TEST_DB_FILENAME}""
                }},
                ""DatabaseFileName"": ""{TEST_DB_FILENAME}""
            }}");
        }

        public void Dispose()
        {
            if (File.Exists(TEST_DB_FILENAME))
            {
                File.Delete(TEST_DB_FILENAME);
            }
        }

        [Fact]
        public void Main_ShouldCreateDatabaseAndInsertData()
        {
            // Arrange
            Console.WriteLine("Running DataGeneration system test...");

            // Act
            Program.Main(new string[0]);

            // Assert
            // Verify database was created
            Assert.True(File.Exists(TEST_DB_FILENAME), "Database file should be created");

            using var connection = new SQLiteConnection($"Data Source={TEST_DB_FILENAME};");
            connection.Open();

            // Verify tables were created
            int tableCount = connection.QuerySingle<int>("SELECT COUNT(*) FROM sqlite_master WHERE type='table';");
            Assert.True(tableCount > 0, "Tables should be created");

            // Verify record counts
            int userCount = connection.QuerySingle<int>("SELECT COUNT(*) FROM User;");
            int accountCount = connection.QuerySingle<int>("SELECT COUNT(*) FROM Account;");
            int documentCount = connection.QuerySingle<int>("SELECT COUNT(*) FROM Document;");

            Assert.Equal(100, userCount);
            Assert.Equal(100, accountCount);
            Assert.Equal(100 * 10000, documentCount);
        }
    }
}
