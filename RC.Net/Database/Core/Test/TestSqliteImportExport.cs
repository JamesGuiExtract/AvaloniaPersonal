using Extract.Database.Sqlite;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;

namespace Extract.Database.Test
{
    [TestFixture, Category("SqliteImportExport"), Category("Automated")]
    public class TestSqliteImportExport
    {
        const string _SIMPLE_AKA_DATA = "Resources.SimpleAKAData.csv";
        const string _COMPLEX_AKA_DATA = "Resources.ComplexAKAData.csv";

        static TestFileManager<TestSqliteImportExport> _testFileManager;

        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testFileManager = new();
        }

        [OneTimeTearDown]
        public static void FinalCleanup()
        {
            _testFileManager.Dispose();
        }

        /// Confirm that importing works when using append (auto-increment ID column is ignored)
        [Test]
        public void ImportDataIntoEmptyTable_Append()
        {
            // Arrange
            using var databaseFile = new TemporaryFile(".sqlite", false);
            using var connection = CreateDatabase(databaseFile.FileName);
            string csvFile = _testFileManager.GetFile(_SIMPLE_AKA_DATA);

            // Act
            var res = ImportTable.ImportFromFile(new("AlternateTestName", csvFile, false, replaceDataRows: false), connection);

            // Assert
            Assert.That(res.Item1, Is.EqualTo(0)); // Zero failed rows

            var expectedData = new[]
            {
                new [] {"A Name","A test code","A","1" },
                new [] {"Another Name","A different test code","P","2" }
            };

            List<string[]> actualData = GetDataFromAKATable(connection);

            CollectionAssert.AreEquivalent(expectedData, actualData);
        }

        /// Confirm that importing works when using replace (auto-increment ID column values are imported)
        [Test]
        public void ImportDataIntoEmptyTable_Replace()
        {
            // Arrange
            using var databaseFile = new TemporaryFile(".sqlite", false);
            using var connection = CreateDatabase(databaseFile.FileName);
            string csvFile = _testFileManager.GetFile(_SIMPLE_AKA_DATA);

            // Act
            var res = ImportTable.ImportFromFile(new("AlternateTestName", csvFile, false, replaceDataRows: true), connection);

            // Assert
            Assert.That(res.Item1, Is.EqualTo(0)); // Zero failed rows

            var expectedData = new[]
            {
                new [] {"A Name","A test code","A","2" },
                new [] {"Another Name","A different test code","P","1" }
            };

            List<string[]> actualData = GetDataFromAKATable(connection);

            CollectionAssert.AreEquivalent(expectedData, actualData);
        }

        /// Confirm that exporting works
        [Test]
        public void ExportData()
        {
            // Arrange
            using var databaseFile = new TemporaryFile(".sqlite", false);
            using var connection = CreateDatabase(databaseFile.FileName);
            string insertQuery =
                @"insert into AlternateTestName values ('A Name','A test code','A','2');
                  insert into AlternateTestName values ('Another Name','A different test code','P','1');";
            using var command = new SQLiteCommand(insertQuery, connection);
            command.ExecuteNonQuery();

            using var outputFile = new TemporaryFile(".csv", false);

            // Act
            ExportTable.ExportToFile(new("select * from AlternateTestName", outputFile.FileName), connection);

            // Assert
            var expectedData = new[]
            {
                "A Name,A test code,A,2",
                "Another Name,A different test code,P,1"
            };
            var actualData = File.ReadAllLines(outputFile.FileName);

            CollectionAssert.AreEquivalent(expectedData, actualData);
        }

        /// Confirm that importing then exporting data that has special characters and newlines works
        // https://extract.atlassian.net/browse/ISSUE-17963
        // https://extract.atlassian.net/browse/ISSUE-17965
        [Test]
        public void ImportThenExportComplexData()
        {
            // Arrange
            using var databaseFile = new TemporaryFile(".sqlite", false);
            using var connection = CreateDatabase(databaseFile.FileName);
            string csvFile = _testFileManager.GetFile(_COMPLEX_AKA_DATA);
            using var outputFile = new TemporaryFile(".csv", false);

            // Act
            ImportTable.ImportFromFile(new("AlternateTestName", csvFile, false, replaceDataRows: true), connection);
            ExportTable.ExportToFile(new("select * from AlternateTestName", outputFile.FileName), connection);

            // Assert

            // This matches the input except for extra spaces around commas and unnecessary quotes
            var expectedData = new[]
            {
                "\"A Name, with comma\",\"\"\"Test\"\" code\",\",\",5",
                "\"A multi-",
                "line name, with comma\",Doesn't need quotes but it works,A,9999999999"
            };

            var actualData = File.ReadAllLines(outputFile.FileName);

            Assert.That(actualData, Is.EquivalentTo(expectedData));
        }

        /// Confirm that exporting then importing data that has special characters and newlines results in identical data
        /// in the database after the round trip
        // https://extract.atlassian.net/browse/ISSUE-17963
        // https://extract.atlassian.net/browse/ISSUE-17965
        [Test]
        public void RoundtripComplexData_Replace()
        {
            // Arrange
            using var databaseFile = new TemporaryFile(".sqlite", false);
            using var connection = CreateDatabase(databaseFile.FileName);
            string insertQuery =
                @"insert into AlternateTestName values ('A Name, with comma', '""Test"" code', ',', 5);
                  insert into AlternateTestName values ('A multi-' || char(13) || char(10) || 'line name, with comma', '%WBC', 'A', 9999999999)";
            using var command = new SQLiteCommand(insertQuery, connection);
            command.ExecuteNonQuery();

            // Verify the inserted data
            var expectedData = new[]
            {
                new [] { "A Name, with comma", "\"Test\" code", "," ,"5" },
                new [] { "A multi-\r\nline name, with comma", "%WBC", "A", "9999999999" }
            };
            List<string[]> actualData = GetDataFromAKATable(connection);
            Assume.That(actualData, Is.EquivalentTo(expectedData));

            using var outputFile = new TemporaryFile(".csv", false);

            // Act
            ExportTable.ExportToFile(new("select * from AlternateTestName", outputFile.FileName), connection);
            ImportTable.ImportFromFile(new("AlternateTestName", outputFile.FileName, false, replaceDataRows: true), connection);

            // Assert
            actualData = GetDataFromAKATable(connection);
            Assert.That(actualData, Is.EquivalentTo(expectedData));
        }

        /// Confirm that exporting then importing data that has special characters and newlines
        /// in append mode results in doubled data in the database after the round trip
        // https://extract.atlassian.net/browse/ISSUE-17963
        // https://extract.atlassian.net/browse/ISSUE-17965
        [Test]
        public void RoundtripComplexData_Append()
        {
            // Arrange
            using var databaseFile = new TemporaryFile(".sqlite", false);
            using var connection = CreateDatabase(databaseFile.FileName);
            string insertQuery =
                @"insert into AlternateTestName values ('A Name, with comma', '""Test"" code', ',', 5);
                  insert into AlternateTestName values ('A multi-' || char(13) || char(10) || 'line name, with comma', '%WBC', 'A', 9999999999)";
            using var command = new SQLiteCommand(insertQuery, connection);
            command.ExecuteNonQuery();

            // Verify the inserted data
            var expectedDataPrecondition = new[]
            {
                new [] { "A Name, with comma", "\"Test\" code", "," ,"5" },
                new [] { "A multi-\r\nline name, with comma", "%WBC", "A", "9999999999" }
            };
            List<string[]> actualData = GetDataFromAKATable(connection);
            Assume.That(actualData, Is.EquivalentTo(expectedDataPrecondition));

            using var outputFile = new TemporaryFile(".csv", false);

            // Act
            ExportTable.ExportToFile(new("select * from AlternateTestName", outputFile.FileName), connection);
            ImportTable.ImportFromFile(new("AlternateTestName", outputFile.FileName, false, replaceDataRows: false), connection);

            // Assert
            var expectedDataPostcondition = new[]
            {
                new [] { "A Name, with comma", "\"Test\" code", "," ,"5" },
                new [] { "A multi-\r\nline name, with comma", "%WBC", "A", "9999999999" },
                new [] { "A Name, with comma", "\"Test\" code", "," ,"10000000000" },
                new [] { "A multi-\r\nline name, with comma", "%WBC", "A", "10000000001" }
            };
            actualData = GetDataFromAKATable(connection);
            Assert.That(actualData, Is.EquivalentTo(expectedDataPostcondition));
        }

        #region Helper Methods

        private static List<string[]> GetDataFromAKATable(SQLiteConnection connection)
        {
            using SQLiteCommand command = new("select * from AlternateTestName", connection);
            using SQLiteDataReader reader = command.ExecuteReader();

            var actualData = new List<string[]>();
            while (reader.Read())
            {
                actualData.Add(new[]
                {
                    reader.GetString(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetInt64(3).ToString(CultureInfo.CurrentCulture)
                });
            }

            return actualData;
        }

        private SQLiteConnection CreateDatabase(string path)
        {
            string connectionString = SqliteMethods.BuildConnectionString(path);
            SQLiteConnection sqlConnection = new(connectionString);
            sqlConnection.Open();

            string createTableQuery = @"
                CREATE TABLE AlternateTestName (
                    Name       NVARCHAR (255) NOT NULL COLLATE NOCASE,
                    TestCode   NVARCHAR (255) NOT NULL COLLATE NOCASE,
                    StatusCode NVARCHAR (1)   NOT NULL COLLATE NOCASE,
                    ID         INTEGER        PRIMARY KEY AUTOINCREMENT NOT NULL)";

            using SQLiteCommand command = new(createTableQuery, sqlConnection);
            command.ExecuteNonQuery();

            return sqlConnection;
        }

        #endregion
    }
}