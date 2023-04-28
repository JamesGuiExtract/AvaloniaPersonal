using Extract.Database.Sqlite;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extract.Database.Test
{
    [TestFixture, Category("SqliteConnection"), Category("Automated")]
    public class TestSqliteConnection
    {
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
        }

        [OneTimeTearDown]
        public static void FinalCleanup()
        {
        }

        /// <summary>
        /// Confirm that connecting to a non-existent file does not create the file
        /// </summary>
        [Test]
        public void ConnectToMissingFile()
        {
            // Arrange
            using var databaseFile = new TemporaryFile(".sqlite", false);
            File.Delete(databaseFile.FileName);

            // Act
            string connectionString = SqliteMethods.BuildConnectionString(databaseFile.FileName);

            // Assert
            using SQLiteConnection sqlConnection = new(connectionString);

            Assert.Throws<SQLiteException>(sqlConnection.Open);

            FileAssert.DoesNotExist(databaseFile.FileName);
        }

        /// <summary>
        /// Confirm that connecting to an existing file works
        /// </summary>
        [Test]
        public void ConnectToExistingFile()
        {
            // Arrange
            using var databaseFile = new TemporaryFile(".sqlite", false);

            // Act
            string connectionString = SqliteMethods.BuildConnectionString(databaseFile.FileName);

            // Assert (Confirm that the connection string is usable)
            using SQLiteConnection sqlConnection = new(connectionString);

            sqlConnection.Open();

            string createTableQuery = @"CREATE TABLE TestTable (Name TEXT, Value TEXT)";
            using SQLiteCommand command = new(createTableQuery, sqlConnection);
            command.ExecuteNonQuery();
        }
    }
}
