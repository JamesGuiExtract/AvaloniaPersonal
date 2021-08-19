using Extract.Database;
using Extract.FileActionManager.Database.SqliteModels.Version8;
using Extract.Testing.Utilities;
using Extract.Utilities;
using LinqToDB;
using LinqToDB.Data;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

using CurrentFAMServiceDB = Extract.FileActionManager.Database.SqliteModels.Version8.FAMServiceDB;
using CurrentFAMServiceDBSettings = Extract.FileActionManager.Database.SqliteModels.Version8.FAMServiceDBSettings;
using CurrentFPSFile = Extract.FileActionManager.Database.SqliteModels.Version8.FPSFile;

namespace Extract.FileActionManager.Database.Test
{
    [TestFixture]
    [Category("FAMServiceSqliteDatabaseManager")]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class TestFAMServiceSqliteDatabaseManager
    {
        TemporaryFile _currentServiceDBFile;
        FAMServiceSqliteDatabaseManager _manager;

        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
        }

        [SetUp]
        public void PerTestSetup()
        {
            _currentServiceDBFile = new(".sqlite", false);
            File.Delete(_currentServiceDBFile.FileName);
            _manager = new FAMServiceSqliteDatabaseManager(_currentServiceDBFile.FileName);
            _manager.CreateDatabase();
        }

        [TearDown]
        public void Teardown()
        {
            _currentServiceDBFile.Dispose();
        }

        /// Confirm that the database has been created with the correct tables and settings
        [Test]
        [Category("Automated")]
        public void TestCreateDatabase()
        {
            AssertCurrentSchema(new(_currentServiceDBFile.FileName));
        }

        /// Confirm that an exception is thrown when attempting an invalid upgrade from current to current
        [Test]
        [Category("Automated")]
        public void UpdateToLatestSchema_ThrowsExceptionWhenSchemaIsCurrent()
        {
            var ex = Assert.ThrowsAsync<ExtractException>(async () => await _manager.UpdateToLatestSchema());
            StringAssert.AreEqualIgnoringCase("Database version must be less than the current schema version", ex.Message);
        }

        /// Confirm that an exception is thrown when attempting an invalid upgrade from newer to current
        [Test]
        [Category("Automated")]
        public void UpdateToLatestSchema_ThrowsExceptionWhenSchemaIsNewer()
        {
            using var db = new CurrentFAMServiceDB(SqliteMethods.BuildConnectionOptions(_currentServiceDBFile.FileName));
            db.Update(new Settings {
                Name = CurrentFAMServiceDBSettings.ServiceDBSchemaVersionKey,
                Value = UtilityMethods.FormatInvariant($"{CurrentFAMServiceDB.SchemaVersion + 1}") });

            // Confirm the schema has been updated
            Assert.AreEqual(CurrentFAMServiceDB.SchemaVersion + 1, _manager.GetSchemaVersion());

            // Confirm the correct exception is thrown
            var ex = Assert.ThrowsAsync<ExtractException>(async () => await _manager.UpdateToLatestSchema());
            StringAssert.AreEqualIgnoringCase("Database version must be less than the current schema version", ex.Message);
        }

        /// Confirm that an exception is thrown when attempting an invalid upgrade from an unsupported, old version
        [Test]
        [Category("Automated")]
        public void UpdateToLatestSchema_ThrowsExceptionWhenSchemaIsTooOld([Range(-1, 7)] int schemaVersion)
        {
            using var db = new CurrentFAMServiceDB(SqliteMethods.BuildConnectionOptions(_currentServiceDBFile.FileName));
            db.Update(new Settings {
                Name = CurrentFAMServiceDBSettings.ServiceDBSchemaVersionKey,
                Value = UtilityMethods.FormatInvariant($"{schemaVersion}") });

            // Confirm the schema has been updated
            Assert.AreEqual(schemaVersion, _manager.GetSchemaVersion());

            // Confirm the correct exception is thrown
            var ex = Assert.ThrowsAsync<ExtractException>(async () => await _manager.UpdateToLatestSchema());
            StringAssert.AreEqualIgnoringCase(
                UtilityMethods.FormatInvariant($"Unsupported schema version: {schemaVersion}"),
                ex.Message);
        }

        /// Confirm that SetDatabase allows the manager to connect to a database file
        [Test]
        [Category("Automated")]
        public void TestSetDatabase()
        {
            FAMServiceSqliteDatabaseManager manager = new();
            manager.SetDatabase(_currentServiceDBFile.FileName);

            AssertCurrentSchema(manager);
        }

        /// Confirm that a backup database can be created
        [Test]
        [Category("Automated")]
        public void TestBackupDatabase()
        {
            string backupPath = _manager.BackupDatabase();
            try
            {
                StringAssert.AreNotEqualIgnoringCase(Path.GetFullPath(_currentServiceDBFile.FileName), Path.GetFullPath(backupPath));
                AssertCurrentSchema(new(backupPath));
            }
            finally
            {
                File.Delete(backupPath);
            }
        }

        /// Confirm that GetSchemaVersion returns the schema from the database
        [Test]
        [Category("Automated")]
        public void TestGetSchemaVersion()
        {
            Assert.AreEqual(CurrentFAMServiceDB.SchemaVersion, _manager.GetSchemaVersion());

            // Change the schema version
            using var db = new CurrentFAMServiceDB(SqliteMethods.BuildConnectionOptions(_currentServiceDBFile.FileName));
            db.Update(new Settings { Name = CurrentFAMServiceDBSettings.ServiceDBSchemaVersionKey, Value = "1" });

            // Confirm the schema has been updated
            Assert.AreEqual(1, _manager.GetSchemaVersion());
        }

        /// Confirm that no update is required for a new database
        [Test, Category("Automated")]
        public void IsUpdateRequired_IsFalseForNewDatabase()
        {
            Assert.IsFalse(_manager.IsUpdateRequired);
        }

        /// Confirm update is required for an old database version
        [Test, Category("Automated")]
        public void IsUpdateRequired_IsTrueWhenVersionIsOld()
        {
            // Change the schema version
            using var db = new CurrentFAMServiceDB(SqliteMethods.BuildConnectionOptions(_currentServiceDBFile.FileName));
            db.Update(new Settings { Name = CurrentFAMServiceDBSettings.ServiceDBSchemaVersionKey, Value = "1" });

            Assert.IsTrue(_manager.IsUpdateRequired);
        }

        // Confirm that GetFpsFileData returns the correct data
        [TestCase(false, TestName = "GetFpsFileData_IncludingZeroInstanceRows")]
        [TestCase(true, TestName = "GetFpsFileData_ExcludingZeroInstanceRows")]
        [Category("Automated")]
        public void GetFpsFileData(bool ignoreZeroRows)
        {
            // On a new DB there are no rows
            var actualSettings = _manager.GetFpsFileData(ignoreZeroRows: ignoreZeroRows);
            CollectionAssert.AreEquivalent(new KeyValuePair<string, string>[0], actualSettings);

            // Add some rows
            var fileRows = new[]
            {
                new CurrentFPSFile{ FileName= "File1", NumberOfInstances = 0, NumberOfFilesToProcess = -1},
                new CurrentFPSFile{ FileName= "File2", NumberOfInstances = 0, NumberOfFilesToProcess = -1},
                new CurrentFPSFile{ FileName= "File3", NumberOfInstances = 5, NumberOfFilesToProcess = 100},
                new CurrentFPSFile{ FileName= "File4", NumberOfInstances = 0, NumberOfFilesToProcess = 10}
            };

            using var db = new CurrentFAMServiceDB(SqliteMethods.BuildConnectionOptions(_currentServiceDBFile.FileName));
            db.BulkCopy(fileRows);

            // Convert the added data to FpsFileTableData
            var expectedData = fileRows
                .Select(f =>
                    new FpsFileTableData(
                        f.FileName,
                        f.NumberOfInstances,
                        f.NumberOfFilesToProcess))
                .ToList();

            // Filter if ignoring zero-instance rows
            if (ignoreZeroRows)
            {
                expectedData.RemoveAll(row => row.NumberOfInstances == 0);
            }

            // Confirm that the expected data is returned
            var actualData = _manager.GetFpsFileData(ignoreZeroRows: ignoreZeroRows);

            CollectionAssert.AreEquivalent(new KeyValuePair<string, string>[0], actualSettings);
        }

        /// The manager class can be created from the value in the settings table
        [Test]
        [Category("Automated")]
        public void ConfirmManagerClassCanBeCreatedFromString()
        {
            using var db = new CurrentFAMServiceDB(SqliteMethods.BuildConnectionOptions(_currentServiceDBFile.FileName));
            var className = db.Settings.Find(DatabaseHelperMethods.DatabaseSchemaManagerKey)?.Value;
            var manager = UtilityMethods.CreateTypeFromTypeName(className) as ISqliteDatabaseManager;

            Assert.AreEqual(typeof(FAMServiceSqliteDatabaseManager), manager.GetType());
        }


        // Check that the managed db has the correct schema
        // TODO: This could be made more thorough...
        // TODO: Update this after any schema changes
        private void AssertCurrentSchema(FAMServiceSqliteDatabaseManager manager)
        {
            Assert.AreEqual(CurrentFAMServiceDB.SchemaVersion, manager.GetSchemaVersion());

            var expectedTableNames = new[] { "Settings", "FPSFile" };

            using var db = new CurrentFAMServiceDB(SqliteMethods.BuildConnectionOptions(_currentServiceDBFile.FileName));
            var schema = db.DataProvider.GetSchemaProvider().GetSchema(db);
            var actualTableNames = schema.Tables.Select(table => table.TableName).ToList();

            CollectionAssert.AreEquivalent(expectedTableNames, actualTableNames);

            var expectedSettings = CurrentFAMServiceDBSettings.DefaultSettings.Select(s => new KeyValuePair<string, string>(s.Name, s.Value)).ToList();
            var actualSettings = manager.GetSettings().ToList();

            CollectionAssert.AreEquivalent(expectedSettings, actualSettings);
        }
    }
}