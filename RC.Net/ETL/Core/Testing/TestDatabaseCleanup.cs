using Extract.FileActionManager.Database.Test;
using Extract.SqlDatabase;
using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UCLID_FILEPROCESSINGLib;
using UCLID_FILEPROCESSORSLib;

namespace Extract.ETL.Test
{

    [Category("TestDatabaseCleanup")]
    [TestFixture]
    public class TestDatabaseCleanup
    {
        struct TableRowCounts
        {
            public int? AttributeSetForFileRowCount { get; set; }
            public int? AttributeTableRowCount { get; set; }
            public int? FileActionStateTransitionTableRowCount { get; set; }
            public int? SourceDocChangeHistoryTableRowCount { get; set; }
            public int? QueueEventTableRowCount { get; set; }
            public int? LabDEOrderTableRowCount { get; set; }
            public int? LabDEEncounterTableRowCount { get; set; }
        };

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

        [Test]
        [Category("Automated")]
        [Category("ETL")]
        public static void TestDatabaseCleanupProcessWithCancel()
        {
            string testDBName = "Test_DatabaseCleanup";
            try
            {
                // This is only used to initialize the database used for calculating the stats
                var fileProcessingDb = _testDbManager.GetDatabase(_DATABASE, testDBName);
        
                // Create DataCaptureAccuracy object using the initialized database
                DatabaseCleanup databaseCleanup = CreateTestDatabaseCleanup(fileProcessingDb.DatabaseServer, fileProcessingDb.DatabaseName);


                var rowCounts = GetTableRowCounts(databaseCleanup.DatabaseName, databaseCleanup.DatabaseServer);

                // no records should be produced by this
                Assert.Throws<ExtractException>(() => databaseCleanup.Process(_cancel));
                var rowCountsProcess = GetTableRowCounts(databaseCleanup.DatabaseName, databaseCleanup.DatabaseServer);

                Assert.IsTrue(rowCounts.Equals(rowCountsProcess));
        
                // Process using the settings
                databaseCleanup.Process(_noCancel);
                rowCountsProcess = GetTableRowCounts(databaseCleanup.DatabaseName, databaseCleanup.DatabaseServer);
                Assert.AreEqual(0, rowCountsProcess.QueueEventTableRowCount);
            }
            finally
            {
                _testDbManager.RemoveDatabase(testDBName);
            }
        }

        [Test]
        [Category("Automated")]
        [Category("ETL")]
        public static void TestDatabaseCleanupProcessMaxRecords1()
        {
            string testDBName = "Test_DatabaseCleanup";
            try
            {
                // This is only used to initialize the database used for calculating the stats
                var fileProcessingDb = _testDbManager.GetDatabase(_DATABASE, testDBName);
                ProcessFile(fileProcessingDb);
                // Create DataCaptureAccuracy object using the initialized database
                DatabaseCleanup databaseCleanup = CreateTestDatabaseCleanup(fileProcessingDb.DatabaseServer, fileProcessingDb.DatabaseName);
                databaseCleanup.MaxDaysToProcessPerRun = 1;
                databaseCleanup.Process(_noCancel);
                var rowCountsProcess = GetTableRowCounts(databaseCleanup.DatabaseName, databaseCleanup.DatabaseServer);

                // These need to be compared against a date, so no easy formula exists for validating these hence the magic numbers.
                Assert.AreEqual(1, rowCountsProcess.QueueEventTableRowCount);
                Assert.AreEqual(5, rowCountsProcess.FileActionStateTransitionTableRowCount);
                Assert.AreEqual(0, rowCountsProcess.SourceDocChangeHistoryTableRowCount);
                Assert.AreEqual(0, rowCountsProcess.LabDEEncounterTableRowCount);
                Assert.AreEqual(0, rowCountsProcess.LabDEOrderTableRowCount);
                Assert.AreEqual(4, rowCountsProcess.AttributeSetForFileRowCount);
                Assert.AreEqual(53, rowCountsProcess.AttributeTableRowCount);
            }
            finally
            {
                _testDbManager.RemoveDatabase(testDBName);
            }
        }

        private static void ProcessFile(FileProcessingDB fileProcessingDb)
        {
            GeneralMethods.TestSetup();
            TestFileManager<TestFAMFileProcessing> _testFiles = new();
            string _LABDE_TEST_FILE1 = "Resources.TestImage001.tif";
            string _LABDE_ACTION1 = "A01_ExtractData";

            try
            {
                string testFileName1 = _testFiles.GetFile(_LABDE_TEST_FILE1);

                fileProcessingDb.AddFile(testFileName1, _LABDE_ACTION1, -1, EFilePriority.kPriorityNormal,
                    false, false, EActionStatus.kActionPending, false, out bool alreadyExists, out EActionStatus previousStatus);

                var sleepTaskConfig = (IFileProcessingTask)new SleepTask
                {
                    SleepTime = 1,
                    TimeUnits = ESleepTimeUnitType.kSleepSeconds
                };

                using var famSession = new FAMProcessingSession(fileProcessingDb, _LABDE_ACTION1, "", sleepTaskConfig);
                famSession.WaitForProcessingToComplete();
            }
            catch (Exception ex)
            {
                var Test = ex;
            }
            finally
            {
                _testFiles.RemoveFile(_LABDE_TEST_FILE1);
            }
        }

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
                MaxDaysToProcessPerRun = 10000,
                Description = "Test Description"
            };

            string settings = databaseCleanup.ToJson();

            var newDatabaseCleanupSettings = (DatabaseCleanup)DatabaseService.FromJson(settings);

            Assert.IsTrue(string.IsNullOrEmpty(newDatabaseCleanupSettings.DatabaseName));
            Assert.IsTrue(string.IsNullOrEmpty(newDatabaseCleanupSettings.DatabaseServer));
            Assert.AreEqual(databaseCleanup.Description, newDatabaseCleanupSettings.Description);
            Assert.AreEqual(databaseCleanup.MaxDaysToProcessPerRun, newDatabaseCleanupSettings.MaxDaysToProcessPerRun);
            Assert.AreEqual(databaseCleanup.PurgeRecordsOlderThanDays, newDatabaseCleanupSettings.PurgeRecordsOlderThanDays);
        }

        static DatabaseCleanup CreateTestDatabaseCleanup(string databaseServer, string databaseName)
        {
            DatabaseCleanup databaseCleanup = new()
            {
                DatabaseServiceID = 1,
                DatabaseName = databaseName,
                DatabaseServer = databaseServer,
                PurgeRecordsOlderThanDays = 50,
                MaxDaysToProcessPerRun = 10,
            };
            databaseCleanup.UpdateDatabaseServiceSettings();
            return databaseCleanup;
        }

        private static TableRowCounts GetTableRowCounts(string databaseName, string databaseServer)
        {
            var tableRowCounts = new TableRowCounts();

            using var connection = new ExtractRoleConnection(databaseServer, databaseName);
            connection.Open();
            using var cmd = connection.CreateCommand();

            cmd.CommandText = "SELECT COUNT(*) FROM [FileActionStateTransition]";
            tableRowCounts.FileActionStateTransitionTableRowCount = cmd.ExecuteScalar() as Int32?;

            cmd.CommandText = "SELECT COUNT(*) FROM [Attribute]";
            tableRowCounts.AttributeTableRowCount = cmd.ExecuteScalar() as Int32?;

            cmd.CommandText = "SELECT COUNT(*) FROM [AttributeSetForFile]";
            tableRowCounts.AttributeSetForFileRowCount = cmd.ExecuteScalar() as Int32?;

            cmd.CommandText = "SELECT COUNT(*) FROM [QueueEvent]";
            tableRowCounts.QueueEventTableRowCount = cmd.ExecuteScalar() as Int32?;

            cmd.CommandText = "SELECT COUNT(*) FROM [SourceDocChangeHistory]";
            tableRowCounts.SourceDocChangeHistoryTableRowCount = cmd.ExecuteScalar() as Int32?;

            cmd.CommandText = "Select COUNT(*) FROM [LabDEEncounter]";
            tableRowCounts.LabDEEncounterTableRowCount = cmd.ExecuteScalar() as Int32?;

            cmd.CommandText = "Select COUNT(*) FROM [LabDEOrder]";
            tableRowCounts.LabDEOrderTableRowCount = cmd.ExecuteScalar() as Int32?;

            return tableRowCounts;
        }
    }
}
