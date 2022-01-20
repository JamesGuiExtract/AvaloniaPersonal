using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using Extract.Database;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Extract.Database.Test
{
    public class TestSqliteImportExport
    {
        const string _SIMPLE_AKA_DATA = "Resources.SimpleAKAData.csv";

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
            var res = ImportTable.ImportFromFile(new("_", "AlternateTestName", csvFile, false, replaceDataRows: false) { ColumnDelimiter = "," }, connection);

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
            var res = ImportTable.ImportFromFile(new("_", "AlternateTestName", csvFile, false, replaceDataRows: true) { ColumnDelimiter = "," }, connection);

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

            // Act
            using var outputFile = new TemporaryFile(".csv", false);
            ExportTable.ExportToFile(new("_", "select * from AlternateTestName", outputFile.FileName, "AlternateTestName", false) { ColumnDelimiter = "," }, connection);

            // Assert
            var expectedData = new[]
            {
                "A Name,A test code,A,2",
                "Another Name,A different test code,P,1"
            };
            var actualData = File.ReadAllLines(outputFile.FileName);

            CollectionAssert.AreEquivalent(expectedData, actualData);
        }

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
    }
}