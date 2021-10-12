using Extract.Database;
using Extract.Testing.Utilities;
using Extract.Utilities;
using LinqToDB;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Extract.LabResultsCustomComponents.Test
{
    [TestFixture]
    [Category("OrderMapperSqliteDatabaseManager")]
    public class TestOrderMapperSqliteDatabaseManager
    {
        OrderMapperSqliteDatabaseManager _manager;
        TestFileManager<TestOrderMapperSqliteDatabaseManager> _testFiles;
        string _currentOrderMappingDB;

        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
        }

        [SetUp]
        public void PerTestSetup()
        {
            _testFiles = new();
            _currentOrderMappingDB = _testFiles.GetFile("Resources.OrderMappingDB.sqlite");
            _manager = new OrderMapperSqliteDatabaseManager(_currentOrderMappingDB);
        }

        [TearDown]
        public void Teardown()
        {
            _testFiles.Dispose();
        }

        /// Confirm that an exception is thrown when attempting an invalid upgrade from current to current
        [Test, Category("Automated")]
        public void UpdateToLatestSchema_ThrowsExceptionWhenSchemaIsCurrent()
        {
            var ex = Assert.ThrowsAsync<ExtractException>(async () => await _manager.UpdateToLatestSchema());
            StringAssert.AreEqualIgnoringCase("Database version must be less than the current schema version", ex.Message);
        }

        /// Confirm that an exception is thrown when attempting an invalid upgrade from newer to current
        [Test, Category("Automated")]
        public void UpdateToLatestSchema_ThrowsExceptionWhenSchemaIsNewer()
        {
            using var db = new DatabaseWithSettingsTable(_currentOrderMappingDB);
            db.Update(new Settings
            {
                Name = OrderMapperSqliteDatabaseManager.OrderMapperSchemaVersionKey,
                Value = UtilityMethods.FormatInvariant($"{OrderMapperSqliteDatabaseManager.CurrentSchemaVersion + 1}")
            });

            // Confirm the schema has been updated
            Assert.AreEqual(OrderMapperSqliteDatabaseManager.CurrentSchemaVersion + 1, _manager.GetSchemaVersion());

            // Confirm the correct exception is thrown
            var ex = Assert.ThrowsAsync<ExtractException>(async () => await _manager.UpdateToLatestSchema());
            StringAssert.AreEqualIgnoringCase("Database version must be less than the current schema version", ex.Message);
        }

        /// Confirm that an exception is thrown when attempting an invalid upgrade from an unsupported, old version
        [Test, Category("Automated")]
        public void UpdateToLatestSchema_ThrowsExceptionWhenSchemaIsTooOld([Range(-1, 2)] int schemaVersion)
        {
            using var db = new DatabaseWithSettingsTable(_currentOrderMappingDB);
            db.Update(new Settings
            {
                Name = OrderMapperSqliteDatabaseManager.OrderMapperSchemaVersionKey,
                Value = UtilityMethods.FormatInvariant($"{schemaVersion}")
            });

            // Confirm the schema has been updated
            Assert.AreEqual(schemaVersion, _manager.GetSchemaVersion());

            // Confirm the correct exception is thrown
            var ex = Assert.ThrowsAsync<ExtractException>(async () => await _manager.UpdateToLatestSchema());
            StringAssert.AreEqualIgnoringCase(
                UtilityMethods.FormatInvariant($"Unsupported schema version: {schemaVersion}"),
                ex.Message);
        }


        /// Confirm that GetSchemaVersion returns the schema from the database
        [Test, Category("Automated")]
        public void GetSchemaVersion()
        {
            Assert.AreEqual(OrderMapperSqliteDatabaseManager.CurrentSchemaVersion, _manager.GetSchemaVersion());

            // Change the schema version
            using var db = new DatabaseWithSettingsTable(_currentOrderMappingDB);
            db.Update(new Settings { Name = OrderMapperSqliteDatabaseManager.OrderMapperSchemaVersionKey, Value = "1" });

            // Confirm the schema has been updated
            Assert.AreEqual(1, _manager.GetSchemaVersion());
        }

        /// Confirm that SetDatabase allows the manager to connect to a database file
        [Test, Category("Automated")]
        public void SetDatabase()
        {
            OrderMapperSqliteDatabaseManager manager = new();
            manager.SetDatabase(_currentOrderMappingDB);

            AssertCurrentSchema(manager, _currentOrderMappingDB);
        }

        /// Confirm that a backup database can be created
        [Test, Category("Automated")]
        public void BackupDatabase()
        {
            string backupPath = _manager.BackupDatabase();
            try
            {
                StringAssert.AreNotEqualIgnoringCase(Path.GetFullPath(_currentOrderMappingDB), Path.GetFullPath(backupPath));
                AssertCurrentSchema(new(backupPath), backupPath);
            }
            finally
            {
                File.Delete(backupPath);
            }
        }

        /// Confirm the settings are returned correctly
        [Test, Category("Automated")]
        public void GetSettings()
        {
            var expectedSettings = new List<KeyValuePair<string, string>>
            {
                new("DatabaseSchemaManager", "Extract.LabResultsCustomComponents.OrderMapperSqliteDatabaseManager"),
                new("FKBVersion", "16.1.0.58"),
                new("OrderMapperSchemaVersion", "3"),
            };
            var actualSettings = _manager.GetSettings().ToList();
            CollectionAssert.AreEquivalent(expectedSettings, actualSettings);
        }

        /// Confirm that the create method hasn't been implemented, yet
        [Test, Category("Automated")]
        public void CreateDatabase()
        {
            Assert.Throws<NotImplementedException>(() => _manager.CreateDatabase());
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
            using var db = new DatabaseWithSettingsTable(_currentOrderMappingDB);
            db.Update(new Settings { Name = OrderMapperSqliteDatabaseManager.OrderMapperSchemaVersionKey, Value = "2" });

            Assert.IsTrue(_manager.IsUpdateRequired);
        }



        // Check that the managed db has the correct schema
        // TODO: This could be made more thorough...
        // TODO: Update this after any schema changes
        private static void AssertCurrentSchema(OrderMapperSqliteDatabaseManager manager, string databaseFile)
        {
            Assert.AreEqual(OrderMapperSqliteDatabaseManager.CurrentSchemaVersion, manager.GetSchemaVersion());

            var expectedTableNames = new[]
            {
                "AlternateTestName",
                "AlternateTestNameSource",
                "AlternateTestNameStatus",
                "ComponentToESComponentMap",
                "DisabledESComponentAKA",
                "Flag",
                "Gender",
                "LabAddresses",
                "LabOrder",
                "LabOrderTest",
                "LabTest",
                "OrderDerivedFromESOrder",
                "Physician",
                "Settings",
                "SmartTag",
                "Unit"
            };

            using var db = new DatabaseWithSettingsTable(databaseFile);
            var schema = db.DataProvider.GetSchemaProvider().GetSchema(db);
            var actualTableNames = schema.Tables.Select(table => table.TableName).ToList();

            CollectionAssert.AreEquivalent(expectedTableNames, actualTableNames);
        }
    }
}