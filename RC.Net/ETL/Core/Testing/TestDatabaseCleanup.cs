using Extract.FileActionManager.Database.Test;
using Extract.SqlDatabase;
using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using static System.DateTime;

namespace Extract.ETL.Test
{

    [Category("TestDatabaseCleanup")]
    [TestFixture]
    public class TestDatabaseCleanup
    {
        static readonly string _DATABASE = "Resources.AccuracyDemo_LabDE.bak";

        static CancellationToken _noCancel = new(false);
        static CancellationToken _cancel = new(true);

        /// <summary>
        /// Manages test FAM DBs
        /// </summary>
        static FAMTestDBManager<TestAccuracyServices> _testDbManager;

        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testDbManager = new FAMTestDBManager<TestAccuracyServices>();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [OneTimeTearDown]
        public static void FinalCleanup()
        {
            if (_testDbManager != null)
            {
                _testDbManager.Dispose();
                _testDbManager = null;
            }
        }

        /// <summary>
        /// Test that if no Database configuration is set that the call to Process throws an exception
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("ETL")]
        public static void TestNoDBProcess()
        {
            var databaseCleanup = new DatabaseCleanup();
            Assert.Throws<ExtractException>(() => databaseCleanup.Process(_noCancel), "Process should throw an exception");
        }

        /// <summary>
        /// Test the use of the IDatabase.Process command for DataCaptureAccuracy Service
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("ETL")]
        public static void TestDatabaseCleanupProcess()
        {
            string testDBName = "Test_DatabaseCleanup";
            try
            {
                // This is only used to initialize the database used for calculating the stats
                var fileProcessingDb = _testDbManager.GetDatabase(_DATABASE, testDBName);
        
                // Create DataCaptureAccuracy object using the initialized database
                DatabaseCleanup databaseCleanup = CreateTestDatabaseCleanup(fileProcessingDb.DatabaseServer, fileProcessingDb.DatabaseName);
        
        
                using var connection = new ExtractRoleConnection(databaseCleanup.DatabaseServer, databaseCleanup.DatabaseName);
                connection.Open();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT COUNT(ID) FROM [FileActionStateTransition]";

                // no records should be produced by this
                Assert.Throws<ExtractException>(() => databaseCleanup.Process(_cancel));        
                Assert.AreEqual(cmd.ExecuteScalar() as Int32?, 15);
        
                // Process using the settings
                databaseCleanup.Process(_noCancel);

                // After processing all records should be removed as they are all older than 50 days.
                Assert.AreEqual(cmd.ExecuteScalar() as Int32?, 0);

                // Run again - There should be no changes
                Assert.AreEqual(cmd.ExecuteScalar() as Int32?, 0);
            }
            finally
            {
                _testDbManager.RemoveDatabase(testDBName);
            }
        }

        /// <summary>
        /// Test the use of the IDatabase.Process command for DataCaptureAccuracy Service
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("ETL")]
        public static void TestDatabaseCleanupProcessMaxFiles1()
        {
            string testDBName = "Test_DatabaseCleanup";
            try
            {
                // This is only used to initialize the database used for calculating the stats
                var fileProcessingDb = _testDbManager.GetDatabase(_DATABASE, testDBName);

                // Create DataCaptureAccuracy object using the initialized database
                DatabaseCleanup databaseCleanup = CreateTestDatabaseCleanup(fileProcessingDb.DatabaseServer, fileProcessingDb.DatabaseName);
                databaseCleanup.MaxFilesToSelect = 1;

                using var connection = new ExtractRoleConnection(databaseCleanup.DatabaseServer, databaseCleanup.DatabaseName);
                connection.Open();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT COUNT(ID) FROM [FileActionStateTransition]";

                // The database has 15 records by default.
                Assert.AreEqual(cmd.ExecuteScalar() as Int32?, 15);

                // Process using the settings
                databaseCleanup.Process(_noCancel);

                // Get the data from the database after processing
                // The singular file being deleted has 5 records in the fast table, but it is one file. That is why the fast entries go down to 10.
                Assert.AreEqual(cmd.ExecuteScalar() as Int32?, 10);
            }
            finally
            {
                _testDbManager.RemoveDatabase(testDBName);
            }
        }

        /// <summary>
        /// Tests that the DataCaptureAccuracy object can save settings to a string and then 
        /// be created with that string
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("ETL")]
        public static void TestDatabaseCleanupGetSettingsAndLoad()
        {
            DatabaseCleanup databaseCleanup = new()
            {
                DatabaseName = "Database",
                DatabaseServer = "Server",
                PurgeRecordsOlderThanDays = 50,
                MaxFilesToSelect = 100,
                Description = "Test Description"
            };

            string settings = databaseCleanup.ToJson();

            var newDatabaseCleanupSettings = DatabaseService.FromJson(settings);

            Assert.IsTrue(string.IsNullOrEmpty(newDatabaseCleanupSettings.DatabaseName));
            Assert.IsTrue(string.IsNullOrEmpty(newDatabaseCleanupSettings.DatabaseServer));
            Assert.AreEqual(databaseCleanup.Description, newDatabaseCleanupSettings.Description);

            DatabaseCleanup databaseCleanupFromDatabase = newDatabaseCleanupSettings as DatabaseCleanup;

            Assert.IsNotNull(databaseCleanupFromDatabase);

            Assert.AreEqual(databaseCleanup.PurgeRecordsOlderThanDays, databaseCleanupFromDatabase.PurgeRecordsOlderThanDays);
        }

        static DatabaseCleanup CreateTestDatabaseCleanup(string databaseServer, string databaseName)
        {
            DatabaseCleanup databaseCleanup = new()
            {
                DatabaseServiceID = 1,
                DatabaseName = databaseName,
                DatabaseServer = databaseServer,
                PurgeRecordsOlderThanDays = 50,
                MaxFilesToSelect = 10000,
            };
            databaseCleanup.UpdateDatabaseServiceSettings();
            return databaseCleanup;
        }
    }
}
