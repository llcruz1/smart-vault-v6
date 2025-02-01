using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SmartVault.Library;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace SmartVault.DataGeneration
{
    partial class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("SmartVault.DataGeneration/appsettings.json").Build();

            SQLiteConnection.CreateFile(configuration["DatabaseFileName"]);
            File.WriteAllText("TestDoc.txt", $"This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}");

            using var connection = new SQLiteConnection(string.Format(configuration?["ConnectionStrings:DefaultConnection"] ?? "", configuration?["DatabaseFileName"]));
            connection.Open();
            
            using var transaction = connection.BeginTransaction();

            Console.WriteLine("Starting the database insert operations...");
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var files = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "BusinessObjectSchema"));
            foreach (var file in files)
            {
                var serializer = new XmlSerializer(typeof(BusinessObject));
                var businessObject = serializer.Deserialize(new StreamReader(file)) as BusinessObject;
                connection.Execute(businessObject?.Script, transaction);
            }

            var users = new List<object>();
            var accounts = new List<object>();
            var documents = new List<object>();
            var documentNumber = 0;

            for (int i = 0; i < 100; i++)
            {
                var randomDayIterator = RandomDay().GetEnumerator();
                randomDayIterator.MoveNext();

                users.Add(new { Id = i, FirstName = $"FName{i}", LastName = $"LName{i}", DateOfBirth = randomDayIterator.Current.ToString("yyyy-MM-dd"), AccountId = i, Username = $"UserName-{i}", Password = "e10adc3949ba59abbe56e057f20f883e" });
                accounts.Add(new { Id = i, Name = $"Account{i}" });

                for (int d = 0; d < 10000; d++, documentNumber++)
                {
                    var documentPath = new FileInfo("TestDoc.txt").FullName;
                    documents.Add(new { Id = documentNumber, Name = $"Document{i}-{d}.txt", FilePath = documentPath, Length = new FileInfo(documentPath).Length, AccountId = i });
                }
            }

            connection.Execute("INSERT INTO User (Id, FirstName, LastName, DateOfBirth, AccountId, Username, Password) VALUES (@Id, @FirstName, @LastName, @DateOfBirth, @AccountId, @Username, @Password)", users, transaction);
            connection.Execute("INSERT INTO Account (Id, Name) VALUES (@Id, @Name)", accounts, transaction);
            connection.Execute("INSERT INTO Document (Id, Name, FilePath, Length, AccountId) VALUES (@Id, @Name, @FilePath, @Length, @AccountId)", documents, transaction);

            transaction.Commit();

            stopwatch.Stop();
            Console.WriteLine($"Time taken for inserts: {stopwatch.Elapsed.TotalSeconds} seconds");

            var accountData = connection.Query("SELECT COUNT(*) FROM Account;");
            Console.WriteLine($"AccountCount: {JsonConvert.SerializeObject(accountData)}");
            var documentData = connection.Query("SELECT COUNT(*) FROM Document;");
            Console.WriteLine($"DocumentCount: {JsonConvert.SerializeObject(documentData)}");
            var userData = connection.Query("SELECT COUNT(*) FROM User;");
            Console.WriteLine($"UserCount: {JsonConvert.SerializeObject(userData)}");
        }

        static IEnumerable<DateTime> RandomDay()
        {
            DateTime start = new DateTime(1985, 1, 1);
            Random gen = new Random();
            int range = (DateTime.Today - start).Days;
            while (true)
                yield return start.AddDays(gen.Next(range));
        }
    }
}
