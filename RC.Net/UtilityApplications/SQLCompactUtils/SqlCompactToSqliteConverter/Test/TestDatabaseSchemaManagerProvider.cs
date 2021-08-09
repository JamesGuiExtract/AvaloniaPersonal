using Extract.Database;
using Extract.FileActionManager.Database;
using Extract.Testing.Utilities;
using NUnit.Framework;
using System.IO;

namespace Extract.Utilities.SqlCompactToSqliteConverter.Test
{
    // Tests in this class must be run serially unless [FixtureLifeCycle(LifeCycle.InstancePerTestCase)] is used (upgrade nunit)
    [TestFixture]
    [Category("SqlCompactToSqliteConverter")]
    public class TestDatabaseSchemaManagerProvider
    {
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
        }

        private TemporaryFile _temporaryDatabaseFile;
        private string _databaseFile;
        private DatabaseSchemaManagerProvider _schemaManagerProvider;
        
        // Per-test setup
        [SetUp]
        public void Init()
        {
            _temporaryDatabaseFile = new(".sdf", false);
            _databaseFile = _temporaryDatabaseFile.FileName;
            File.Delete(_databaseFile);

            _schemaManagerProvider = new();
        }

        // Per-test cleanup
        [TearDown]
        public void TearDown()
        {
            _temporaryDatabaseFile.Dispose();
        }

        /// Confirm that the correct IDatabaseSchemaManager is returned from a database with a Settings table
        [Test]
        [Category("Automated")]
        public void GetSqlCompactSchemaUpdater_ShouldReturnConfiguredSchemaManager()
        {
            // Create a compact database with a schema manager defined in the settings table
            using (SqlCompactDatabaseWithSettingsTable database = new(_databaseFile))
            {
                database.CreateDatabase();

                // Add name of a schema manager to the settings table
                database.Settings.InsertOnSubmit(new Settings
                {
                    Name = "DatabaseSchemaManager",
                    Value = typeof(SchemaManagerMock).FullName
                });

                database.SubmitChanges();
            }

            IDatabaseSchemaManager schemaManager = _schemaManagerProvider.GetSqlCompactSchemaManager(_databaseFile);

            Assert.NotNull(schemaManager);
            Assert.AreEqual(typeof(SchemaManagerMock), schemaManager.GetType());
        }

        /// Confirm that null is returned from a database with a Settings table but no schema manager defined
        [Test]
        [Category("Automated")]
        public void GetSqlCompactSchemaUpdater_ShouldReturnNullIfSettingsTableIsEmpty()
        {
            // Create a compact database with an empty settings table
            using (SqlCompactDatabaseWithSettingsTable database = new(_databaseFile))
            {
                database.CreateDatabase();
            }

            IDatabaseSchemaManager schemaManager = _schemaManagerProvider.GetSqlCompactSchemaManager(_databaseFile);

            Assert.IsNull(schemaManager);
        }

        /// Confirm that null is returned from a database without a Settings table
        [Test]
        [Category("Automated")]
        public void GetSqlCompactSchemaUpdater_ShouldReturnNullIfThereIsNoSettingsTable()
        {
            // Create a compact database without any special tables
            using (SqlCompactDatabaseWithNoSpecialTables database = new(_databaseFile))
            {
                database.CreateDatabase();
            }

            IDatabaseSchemaManager schemaManager = _schemaManagerProvider.GetSqlCompactSchemaManager(_databaseFile);

            Assert.IsNull(schemaManager);
        }

        /// Confirm that the correct IDatabaseSchemaManager is returned from a database with an FPSFile table
        [Test]
        [Category("Automated")]
        public void GetSqlCompactSchemaUpdater_ShouldReturnFAMServiceSchemaManager()
        {
            // Create a compact database but don't configure a schema manager
            using (SqlCompactDatabaseWithFPSFileTable database = new(_databaseFile))
            {
                database.CreateDatabase();
            }

            IDatabaseSchemaManager schemaManager = _schemaManagerProvider.GetSqlCompactSchemaManager(_databaseFile);

            Assert.NotNull(schemaManager);
            Assert.AreEqual(typeof(FAMServiceDatabaseManager), schemaManager.GetType());
        }
    }
}
