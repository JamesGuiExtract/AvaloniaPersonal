using Extract.Database;
using Extract.Database.Sqlite;
using Extract.Testing.Utilities;
using Extract.Utilities.ContextTags.SqliteModels.Version3;
using LinqToDB;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Extract.Utilities.ContextTags.Test
{
    [TestFixture]
    [Category("ContextTagsSqliteDatabaseManager")]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class TestContextTagsSqliteDatabaseManager
    {
        TemporaryFile _currentCustomTagsDBFile;
        ContextTagsSqliteDatabaseManager _manager;

        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
        }

        [SetUp]
        public void PerTestSetup()
        {
            // Create the database in a random subfolder of the temp dir
            _currentCustomTagsDBFile = new(null, "CustomTags.sqlite", null, false);
            File.Delete(_currentCustomTagsDBFile.FileName);
            _manager = new ContextTagsSqliteDatabaseManager(_currentCustomTagsDBFile.FileName);
            _manager.CreateDatabase();
        }

        [TearDown]
        public void Teardown()
        {
            _currentCustomTagsDBFile.Dispose();
        }

        /// Confirm that the database has been created with the correct tables and settings
        [Test, Category("Automated")]
        public void CreateDatabase()
        {
            AssertCurrentSchema(new(_currentCustomTagsDBFile.FileName));
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
            using var db = new CustomTagsDB(SqliteMethods.BuildConnectionOptions(_currentCustomTagsDBFile.FileName));
            db.Update(new Settings
            {
                Name = CustomTagsDBSettings.ContextTagsDBSchemaVersionKey,
                Value = UtilityMethods.FormatInvariant($"{CustomTagsDB.SchemaVersion + 1}")
            });

            // Confirm the schema has been updated
            Assert.AreEqual(CustomTagsDB.SchemaVersion + 1, _manager.GetSchemaVersion());

            // Confirm the correct exception is thrown
            var ex = Assert.ThrowsAsync<ExtractException>(async () => await _manager.UpdateToLatestSchema());
            StringAssert.AreEqualIgnoringCase("Database version must be less than the current schema version", ex.Message);
        }

        /// Confirm that an exception is thrown when attempting an invalid upgrade from an unsupported, old version
        [Test, Category("Automated")]
        public void UpdateToLatestSchema_ThrowsExceptionWhenSchemaIsTooOld([Range(-1, 2)] int schemaVersion)
        {
            using var db = new CustomTagsDB(SqliteMethods.BuildConnectionOptions(_currentCustomTagsDBFile.FileName));
            db.Update(new Settings
            {
                Name = CustomTagsDBSettings.ContextTagsDBSchemaVersionKey,
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

        /// Confirm that SetDatabase allows the manager to connect to a database file
        [Test, Category("Automated")]
        public void SetDatabase()
        {
            ContextTagsSqliteDatabaseManager manager = new();
            manager.SetDatabase(_currentCustomTagsDBFile.FileName);

            AssertCurrentSchema(manager);
        }

        /// Confirm that a backup database can be created
        [Test, Category("Automated")]
        public void BackupDatabase()
        {
            string backupPath = _manager.BackupDatabase();
            try
            {
                StringAssert.AreNotEqualIgnoringCase(Path.GetFullPath(_currentCustomTagsDBFile.FileName), Path.GetFullPath(backupPath));
                AssertCurrentSchema(new(backupPath));
            }
            finally
            {
                File.Delete(backupPath);
            }
        }

        /// Confirm that GetSchemaVersion returns the schema from the database
        [Test, Category("Automated")]
        public void GetSchemaVersion()
        {
            Assert.AreEqual(CustomTagsDB.SchemaVersion, _manager.GetSchemaVersion());

            // Change the schema version
            using var db = new CustomTagsDB(SqliteMethods.BuildConnectionOptions(_currentCustomTagsDBFile.FileName));
            db.Update(new Settings { Name = CustomTagsDBSettings.ContextTagsDBSchemaVersionKey, Value = "1" });

            // Confirm the schema has been updated
            Assert.AreEqual(1, _manager.GetSchemaVersion());
        }

        /// The manager class can be created from the value in the settings table
        [Test, Category("Automated")]
        public void ManagerClassCanBeCreatedFromString()
        {
            using var db = new CustomTagsDB(SqliteMethods.BuildConnectionOptions(_currentCustomTagsDBFile.FileName));
            var className = db.Settings.Find(DatabaseHelperMethods.DatabaseSchemaManagerKey)?.Value;
            var manager = UtilityMethods.CreateTypeFromTypeName(className) as ISqliteDatabaseManager;

            Assert.AreEqual(typeof(ContextTagsSqliteDatabaseManager), manager.GetType());
        }

        /// <summary>
        /// Test that context names can be retrieved, ignoring case and trailing slash for FPSFileDir
        /// </summary>
        [Test, Category("Automated")]
        public void GetContextNameForDirectory()
        {
            using var db = new CustomTagsDB(SqliteMethods.BuildConnectionOptions(_currentCustomTagsDBFile.FileName));
            db.Insert(new Context { Name = "Context1", FPSFileDir = @"\\server\share" });
            db.Insert(new Context { Name = "Context2", FPSFileDir = @"C:\temp" });

            Assert.AreEqual("Context1", _manager.GetContextNameForDirectory(@"\\server\share"));
            Assert.AreEqual("Context1", _manager.GetContextNameForDirectory(@"\\Server\Share"));
            Assert.AreEqual("Context1", _manager.GetContextNameForDirectory(@"\\Server\Share\"));

            Assert.AreEqual("Context2", _manager.GetContextNameForDirectory(@"C:\temp"));
            Assert.AreEqual("Context2", _manager.GetContextNameForDirectory(@"C:\Temp"));
            Assert.AreEqual("Context2", _manager.GetContextNameForDirectory(@"C:\Temp\"));

            Assert.IsNull(_manager.GetContextNameForDirectory(@"C:\Temp\SubDir"));
        }

        /// Check that the correct behavior is implemented for getting tags by context with default values or workflow-specific overrides
        [Test, Category("Automated")]
        public void GetContextTagsByWorkflow()
        {
            using var db = new CustomTagsDB(SqliteMethods.BuildConnectionOptions(_currentCustomTagsDBFile.FileName));

            // Add a context and two tags
            var context1 = new Context { Name = "Context1", FPSFileDir = @"\\server\share" };
            context1.ID = (long)db.InsertWithIdentity(context1);

            var tag1 = new CustomTag { Name = "Tag1" };
            tag1.ID = (long)db.InsertWithIdentity(tag1);

            var tag2 = new CustomTag { Name = "Tag2" };
            tag2.ID = (long)db.InsertWithIdentity(tag2);

            // Set default values
            AddTagValue(db, context1, tag1, "", "C1Default1");
            AddTagValue(db, context1, tag2, "", "C1Default2");

            // Override both values for workflow 1 and one value for workflow 2
            AddTagValue(db, context1, tag1, "WF1", "C1WF1Value1");
            AddTagValue(db, context1, tag2, "WF1", "C1WF1Value2");
            AddTagValue(db, context1, tag1, "WF2", "C1WF2Value1");

            // Add a second context and override one value
            var context2 = new Context { Name = "Context2", FPSFileDir = @"C:\FSPFiles" };
            context2.ID = (long)db.InsertWithIdentity(context2);
            AddTagValue(db, context2, tag2, "WF1", "C2WF1Value2");
            
            Dictionary<string, Dictionary<string, string>> expectedContext1 = new()
            {
                { "", new Dictionary<string, string> { {"Tag1", "C1Default1"}, {"Tag2", "C1Default2"} } },
                { "WF1", new Dictionary<string, string> { {"Tag1", "C1WF1Value1"}, {"Tag2", "C1WF1Value2"} } },
                { "WF2", new Dictionary<string, string> { {"Tag1", "C1WF2Value1"}, {"Tag2", "C1Default2"} } },
            };
            var tagValues1 = _manager.GetContextTagsByWorkflow("Context1");
            CollectionAssert.AreEquivalent(expectedContext1, tagValues1);
            
            // TODO: Should the code be changed to return all defined tags, even if they have missing values?
            // (There is code in the editor, ContextTagsEditorViewRow, to add new TagValue items for all contexts
            // but this only fires for new rows and so doesn't actually ensure that all contexts have all tags defined)
            Dictionary<string, Dictionary<string, string>> expectedContext2 = new()
            {
                { "", new Dictionary<string, string>() },
                { "WF1", new Dictionary<string, string> { {"Tag2", "C2WF1Value2"} } },
            };

            var tagValues2 = _manager.GetContextTagsByWorkflow("Context2");
            CollectionAssert.AreEquivalent(expectedContext2, tagValues2);
        }

        /// Test method used by ContextTagProvider.LoadTagsForPath
        [Test, Category("Automated")]
        public void IsDatabaseEmpty_ReturnsTrueForNewDatabase()
        {
            Assert.IsTrue(_manager.IsDatabaseEmpty());
        }

        /// Test method used by ContextTagProvider.LoadTagsForPath
        [Test, Category("Automated")]
        public void IsDatabaseEmpty_ReturnsFalseIfDatabaseHasContext()
        {
            using var db = new CustomTagsDB(SqliteMethods.BuildConnectionOptions(_currentCustomTagsDBFile.FileName));

            db.Insert(new Context { Name = "Test", FPSFileDir = @"\\server\share" });
            
            Assert.IsFalse(_manager.IsDatabaseEmpty());
        }

        /// Test method used by ContextTagProvider.LoadTagsForPath
        [Test, Category("Automated")]
        public void IsDatabaseEmpty_ReturnsFalseIfDatabaseHasTag()
        {
            using var db = new CustomTagsDB(SqliteMethods.BuildConnectionOptions(_currentCustomTagsDBFile.FileName));

            db.Insert(new CustomTag { Name = "TagName" });
            
            Assert.IsFalse(_manager.IsDatabaseEmpty());
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
            using var db = new CustomTagsDB(SqliteMethods.BuildConnectionOptions(_currentCustomTagsDBFile.FileName));
            db.Update(new Settings { Name = CustomTagsDBSettings.ContextTagsDBSchemaVersionKey, Value = "1" });

            Assert.IsTrue(_manager.IsUpdateRequired);
        }

        #region Private Methods

        // Check that the managed db has the correct schema
        // TODO: This could be made more thorough...
        // TODO: Update this after any schema changes
        private void AssertCurrentSchema(ContextTagsSqliteDatabaseManager manager)
        {
            Assert.AreEqual(CustomTagsDB.SchemaVersion, manager.GetSchemaVersion());

            var expectedTableNames = new[] { "Settings", "Context", "CustomTag", "TagValue" };

            using var db = new CustomTagsDB(SqliteMethods.BuildConnectionOptions(_currentCustomTagsDBFile.FileName));
            var schema = db.DataProvider.GetSchemaProvider().GetSchema(db);
            var actualTableNames = schema.Tables.Select(table => table.TableName).ToList();

            CollectionAssert.AreEquivalent(expectedTableNames, actualTableNames);

            var expectedSettings = CustomTagsDBSettings.DefaultSettings.Select(s => new KeyValuePair<string, string>(s.Name, s.Value)).ToList();
            var actualSettings = manager.GetSettings().ToList();

            CollectionAssert.AreEquivalent(expectedSettings, actualSettings);
        }

        // Simplify adding a tag value
        private static void AddTagValue(
            CustomTagsDB db,
            Context context,
            CustomTag tag,
            string workflow,
            string value)
        {
            db.Insert(new TagValue
            {
                ContextID = (int)context.ID,
                TagID = (int)tag.ID,
                Workflow = workflow,
                Value = value
            });
        }
        #endregion Private Methods
    }
}
