using System;
using System.Collections.Generic;
using System.IO;
using Moq;
using Xunit;

namespace SmartVault.Program.Tests
{
    public class ProgramTest
    {
        [Fact]
        public void WriteEveryThirdFileToFile_ShouldWriteCorrectFiles()
        {
            // Arrange
            var mockDatabaseService = new Mock<IDatabaseService>();
            mockDatabaseService.Setup(service => service.GetEveryThirdFilePathContainingText(It.IsAny<string>(), "Smith Property"))
                .Returns(new List<string> { "file1.txt", "file2.txt", "file3.txt" });

            foreach (var file in new List<string> { "file1.txt", "file2.txt", "file3.txt" })
            {
                File.WriteAllText(file, "Smith Property");
            }

            // Act
            Program.WriteEveryThirdFileToFile("1", mockDatabaseService.Object);

            // Assert
            string outputFilePath = Path.Combine(AppContext.BaseDirectory, "Account_1_ThirdFiles.txt");
            Assert.True(File.Exists(outputFilePath), "Output file should be created");

            string outputFileContent = File.ReadAllText(outputFilePath);
            Assert.Contains("Smith Property", outputFileContent);

            // Clean up
            foreach (var file in new List<string> { "file1.txt", "file2.txt", "file3.txt" })
            {
                File.Delete(file);
            }
            File.Delete(outputFilePath);
        }

        [Fact]
        public void GetAllFileSizes_ShouldReturnCorrectTotalSize()
        {
            // Arrange
            var mockDatabaseService = new Mock<IDatabaseService>();

            // Mock database returning file paths
            var testFiles = new List<string> { "file1.txt", "file2.txt" };
            mockDatabaseService.Setup(service => service.GetAllFilePaths()).Returns(testFiles);

            // Create test files with known sizes
            File.WriteAllText("file1.txt", new string('a', 100)); // 100 bytes
            File.WriteAllText("file2.txt", new string('b', 200)); // 200 bytes

            // Act
            long totalSize = Program.GetAllFileSizes(mockDatabaseService.Object);

            // Assert
            Assert.Equal(300, totalSize);

            // Cleanup
            File.Delete("file1.txt");
            File.Delete("file2.txt");
        }
    }
}