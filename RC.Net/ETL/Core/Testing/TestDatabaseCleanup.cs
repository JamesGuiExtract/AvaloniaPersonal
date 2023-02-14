using Extract.FileActionManager.Database.Test;
using Extract.SqlDatabase;
using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using UCLID_FILEPROCESSINGLib;

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
            public int? ReportingDataCaptureAccuracy { get; set; }
            public int? ReportingRedactionAccuracy { get; set; }
            public int? DashboardAttributeFields { get; set; }
        };

        private static readonly string AttributeSetName = "VOA";

        private static readonly string UpdateTablesDatetimeForFileID = @"
UPDATE
	dbo.QueueEvent
SET
	DateTimeStamp = @DateTimeStamp
WHERE
	FileID = @FileID
;

UPDATE
	dbo.[FileActionStateTransition]
SET
	DateTimeStamp = @DateTimeStamp
WHERE
	FileID = @FileID
;
UPDATE
	dbo.[FileTaskSession]
SET
	DateTimeStamp = @DateTimeStamp
WHERE
	FileID = @FileID";

        static CancellationToken _noCancel = new(false);
        static CancellationToken _cancel = new(true);

        /// <summary>
        /// Manages test FAM DBs
        /// </summary>
        static FAMTestDBManager<TestDatabaseCleanup> _testDbManager;

        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testDbManager = new FAMTestDBManager<TestDatabaseCleanup>();
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
        /// Runs the cleanup service on a blank database.
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("ETL")]
        public static void TestCleanupBlankDB()
        {
            string testDBName = "Test_DatabaseCleanup_BlankDB";

            using var dbWrapper = new OneWorkflow<TestDatabaseCleanup>(_testDbManager, testDBName, false);
            DatabaseCleanup databaseCleanup = CreateTestDatabaseCleanup(dbWrapper);
            databaseCleanup.Process(_noCancel);
        }

        /// <summary>
        /// Attempts to cleanup on record in the database, that should be so new it is not deleted.
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("ETL")]
        public static void TestCleanupOneRecordNoDeletions()
        {
            string testDBName = "Test_DatabaseCleanup_OneRecordNoDeletions";

            using var dbWrapper = new OneWorkflow<TestDatabaseCleanup>(_testDbManager, testDBName, false);
            DatabaseCleanup databaseCleanup = CreateTestDatabaseCleanup(dbWrapper);
            PopulateFakeDatabaseData(dbWrapper, 1, 0, 0);

            databaseCleanup.Process(_noCancel);
            var rowCounts = GetTableRowCounts(testDBName, "(local)");
            Assert.That(rowCounts.QueueEventTableRowCount, Is.EqualTo(1));
        }

        /// <summary>
        /// Attempts to cleanup on record in the database, that should be so new it is not deleted.
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("ETL")]
        public static void TestCleanupAllTables()
        {
            string testDBName = "Test_DatabaseCleanup_AllTables";
            try
            {
                var fileProcessingDb = _testDbManager.GetDatabase("Resources.DatabaseCleanupAllTables.bak", testDBName);
                DatabaseCleanup databaseCleanup = new()
                {
                    DatabaseName = fileProcessingDb.DatabaseName,
                    DatabaseServer = fileProcessingDb.DatabaseServer,
                    PurgeRecordsOlderThanDays = 50,
                    MaxDaysToProcessPerRun = 10,
                };
                databaseCleanup.AddToDatabase(fileProcessingDb.DatabaseServer, fileProcessingDb.DatabaseName);
                databaseCleanup.UpdateDatabaseServiceSettings();
                var originalRowCounts = GetTableRowCounts(testDBName, "(local)");
                Assert.AreEqual(35, originalRowCounts.AttributeSetForFileRowCount);
                Assert.AreEqual(3251, originalRowCounts.AttributeTableRowCount);
                Assert.AreEqual(146, originalRowCounts.FileActionStateTransitionTableRowCount);
                Assert.AreEqual(10, originalRowCounts.LabDEEncounterTableRowCount);
                Assert.AreEqual(72, originalRowCounts.LabDEOrderTableRowCount);
                Assert.AreEqual(52, originalRowCounts.QueueEventTableRowCount);
                Assert.AreEqual(2, originalRowCounts.SourceDocChangeHistoryTableRowCount);
                Assert.AreEqual(339, originalRowCounts.ReportingDataCaptureAccuracy);
                Assert.AreEqual(7, originalRowCounts.ReportingRedactionAccuracy);
                Assert.AreEqual(35, originalRowCounts.DashboardAttributeFields);
                databaseCleanup.Process(_noCancel);

                var newRowCounts = GetTableRowCounts(testDBName, "(local)");
                Assert.AreEqual(0, newRowCounts.AttributeSetForFileRowCount);
                Assert.AreEqual(0, newRowCounts.AttributeTableRowCount);
                Assert.AreEqual(0, newRowCounts.FileActionStateTransitionTableRowCount);
                Assert.AreEqual(0, newRowCounts.LabDEEncounterTableRowCount);
                Assert.AreEqual(0, newRowCounts.LabDEOrderTableRowCount);
                Assert.AreEqual(0, newRowCounts.QueueEventTableRowCount);
                Assert.AreEqual(0, newRowCounts.SourceDocChangeHistoryTableRowCount);
                Assert.AreEqual(0, newRowCounts.ReportingDataCaptureAccuracy);
                Assert.AreEqual(0, newRowCounts.ReportingRedactionAccuracy);
                Assert.AreEqual(0, newRowCounts.DashboardAttributeFields);
            }
            finally
            {
                _testDbManager.RemoveDatabase(testDBName);
            }
        }

        /// <summary>
        /// Deletes records, but updates the file task session table so that some reporting rows remain.
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("ETL")]
        public static void TestCleanupRetainingReportingData()
        {
            string testDBName = "Test_TestCleanupRetainingReportingData";
            try
            {
                var fileProcessingDb = _testDbManager.GetDatabase("Resources.DatabaseCleanupAllTables.bak", testDBName);
                DatabaseCleanup databaseCleanup = new()
                {
                    DatabaseName = fileProcessingDb.DatabaseName,
                    DatabaseServer = fileProcessingDb.DatabaseServer,
                    PurgeRecordsOlderThanDays = 50,
                    MaxDaysToProcessPerRun = 10,
                };
                databaseCleanup.AddToDatabase(fileProcessingDb.DatabaseServer, fileProcessingDb.DatabaseName);
                databaseCleanup.UpdateDatabaseServiceSettings();
                var originalRowCounts = GetTableRowCounts(testDBName, "(local)");
                Assert.AreEqual(35, originalRowCounts.AttributeSetForFileRowCount);
                Assert.AreEqual(3251, originalRowCounts.AttributeTableRowCount);
                Assert.AreEqual(146, originalRowCounts.FileActionStateTransitionTableRowCount);
                Assert.AreEqual(10, originalRowCounts.LabDEEncounterTableRowCount);
                Assert.AreEqual(72, originalRowCounts.LabDEOrderTableRowCount);
                Assert.AreEqual(52, originalRowCounts.QueueEventTableRowCount);
                Assert.AreEqual(2, originalRowCounts.SourceDocChangeHistoryTableRowCount);
                Assert.AreEqual(339, originalRowCounts.ReportingDataCaptureAccuracy);
                Assert.AreEqual(7, originalRowCounts.ReportingRedactionAccuracy);
                Assert.AreEqual(35, originalRowCounts.DashboardAttributeFields);

                // Update files 1 and 2, to retain their reporting rows.
                UpdateDateTimesForFiles(new Dictionary<int, DateTime> { { 1, DateTime.Now.AddDays(-10) }, { 2, DateTime.Now.AddDays(-10) } }, fileProcessingDb);

                databaseCleanup.Process(_noCancel);

                var newRowCounts = GetTableRowCounts(testDBName, "(local)");
                Assert.AreEqual(4, newRowCounts.AttributeSetForFileRowCount);
                Assert.AreEqual(736, newRowCounts.AttributeTableRowCount);
                Assert.AreEqual(17, newRowCounts.FileActionStateTransitionTableRowCount);
                Assert.AreEqual(0, newRowCounts.LabDEEncounterTableRowCount);
                Assert.AreEqual(0, newRowCounts.LabDEOrderTableRowCount);
                Assert.AreEqual(4, newRowCounts.QueueEventTableRowCount);
                Assert.AreEqual(0, newRowCounts.SourceDocChangeHistoryTableRowCount);
                Assert.AreEqual(102, newRowCounts.ReportingDataCaptureAccuracy);
                Assert.AreEqual(2, newRowCounts.ReportingRedactionAccuracy);
                Assert.AreEqual(4, newRowCounts.DashboardAttributeFields);
            }
            finally
            {
                _testDbManager.RemoveDatabase(testDBName);
            }
        }

        /// <summary>
        /// Attempts to run the cleanup service, and ensure that it is cancelled.
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("ETL")]
        public static void TestDatabaseCleanupProcessWithCancel()
        {
            string testDBName = "Test_DatabaseCleanup_Cancel";

            using var dbWrapper = new OneWorkflow<TestDatabaseCleanup>(_testDbManager, testDBName, false);
            DatabaseCleanup databaseCleanup = CreateTestDatabaseCleanup(dbWrapper);
            PopulateFakeDatabaseData(dbWrapper, 1, 0, 0);
            UpdateDateTimesForFiles(new Dictionary<int, DateTime> { { 1, DateTime.Now.AddDays(-365) } }, dbWrapper.FileProcessingDB);

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

        /// <summary>
        /// Test cleaning up on record in the database.
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("ETL")]
        public static void TestCleanupOneRecordOneDeletion()
        {
            string testDBName = "Test_DatabaseCleanup_OneRecordOneDeletion";

            using var dbWrapper = new OneWorkflow<TestDatabaseCleanup>(_testDbManager, testDBName, false);
            DatabaseCleanup databaseCleanup = CreateTestDatabaseCleanup(dbWrapper);
            PopulateFakeDatabaseData(dbWrapper, 1, 0, 0);
            UpdateDateTimesForFiles(new Dictionary<int, DateTime> { { 1, DateTime.Now.AddDays(-365) } }, dbWrapper.FileProcessingDB);
            databaseCleanup.Process(_noCancel);
            var tableRowCounts = GetTableRowCounts(dbWrapper.FileProcessingDB.DatabaseName, dbWrapper.FileProcessingDB.DatabaseServer);
            Assert.AreEqual(0, tableRowCounts.FileActionStateTransitionTableRowCount);
            Assert.AreEqual(0, tableRowCounts.QueueEventTableRowCount);
            Assert.AreEqual(0, tableRowCounts.AttributeSetForFileRowCount);
        }

        /// <summary>
        /// Ensures that cleaned up records are logged.
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("ETL")]
        public static void TestCleanupLogging()
        {
            string testDBName = "Test_DatabaseCleanup_Logging";

            using var dbWrapper = new OneWorkflow<TestDatabaseCleanup>(_testDbManager, testDBName, false);
            DatabaseCleanup databaseCleanup = CreateTestDatabaseCleanup(dbWrapper);
            PopulateFakeDatabaseData(dbWrapper, 1, 0, 0);
            UpdateDateTimesForFiles(new Dictionary<int, DateTime> { { 1, DateTime.Now.AddDays(-365) } }, dbWrapper.FileProcessingDB);
            databaseCleanup.Process(_noCancel);

            Assert.AreEqual(databaseCleanup.RowDeletedFromTables["FileActionStateTransition"], 2);
            Assert.AreEqual(databaseCleanup.RowDeletedFromTables["QueueEvent"], 1);
        }

        /// <summary>
        /// Tests exceptions.
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("ETL")]
        public static void TestExceptions()
        {
            string testDBName = "Test_DatabaseCleanup_Exceptions";

            using var dbWrapper = new OneWorkflow<TestDatabaseCleanup>(_testDbManager, testDBName, false);
            DatabaseCleanup databaseCleanup = CreateTestDatabaseCleanup(dbWrapper);
            PopulateFakeDatabaseData(dbWrapper, 1, 0, 0);
            UpdateDateTimesForFiles(new Dictionary<int, DateTime> { { 1, DateTime.Now.AddDays(-365) } }, dbWrapper.FileProcessingDB);

            using ExtractRoleConnection connection = new(dbWrapper.FileProcessingDB.DatabaseServer, dbWrapper.FileProcessingDB.DatabaseName);
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = $"UPDATE dbo.DatabaseService SET [LastFileTaskSessionIDProcessed] = {int.MaxValue.ToString(CultureInfo.InvariantCulture)}";
            cmd.ExecuteNonQuery();

            Assert.Throws<ExtractException>(() => databaseCleanup.Process(_noCancel));
        }

        /// <summary>
        /// Ensure that only so many days are processed at a time. This is important for customers with very large
        /// databases. Otherwise cleanup could take all night/days.
        /// Also tests the bounds case of MaxDaysToProcess.
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("ETL")]
        public static void TestCleanupMaxDaysToProcess()
        {
            string testDBName = "Test_DatabaseCleanup_MaxDaysToProcess";

            using var dbWrapper = new OneWorkflow<TestDatabaseCleanup>(_testDbManager, testDBName, false);
            DatabaseCleanup databaseCleanup = CreateTestDatabaseCleanup(dbWrapper);
            databaseCleanup.MaxDaysToProcessPerRun = 5;
            databaseCleanup.PurgeRecordsOlderThanDays = 10;
            PopulateFakeDatabaseData(dbWrapper, 5, 0, 0);
            UpdateDateTimesForFiles(new Dictionary<int, DateTime> {
                { 1, DateTime.Now.AddDays(-365) },
                { 2, DateTime.Now.AddDays(-364) },
                { 3, DateTime.Now.AddDays(-361) },
                { 4, DateTime.Now.AddDays(-360) },
                { 5, DateTime.Now.AddDays(-360) },
            }, dbWrapper.FileProcessingDB);

            databaseCleanup.Process(_noCancel);
            var tableRowCounts = GetTableRowCounts(dbWrapper.FileProcessingDB.DatabaseName, dbWrapper.FileProcessingDB.DatabaseServer);
            // The bottom two records are outside of the max days to process, and will not be deleted. The other three records will.
            Assert.AreEqual(4, tableRowCounts.FileActionStateTransitionTableRowCount);
            Assert.AreEqual(2, tableRowCounts.QueueEventTableRowCount);
        }

        /// <summary>
        /// Ensures cleanup runs smoothly with a mixutre of lost sessions.
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("ETL")]
        public static void TestCleanupMultipleLostSessions()
        {
            string testDBName = "Test_DatabaseCleanup_MultipleLostSessions";

            using var dbWrapper = new OneWorkflow<TestDatabaseCleanup>(_testDbManager, testDBName, false);
            DatabaseCleanup databaseCleanup = CreateTestDatabaseCleanup(dbWrapper);
            PopulateFakeDatabaseData(dbWrapper, 20, 10, 0);
            UpdateDateTimesForFiles(new Dictionary<int, DateTime> {
                { 11, DateTime.Now.AddDays(-365) },
                { 12, DateTime.Now.AddDays(-365) },
                { 13, DateTime.Now.AddDays(-365) },
                { 14, DateTime.Now.AddDays(-365) },
                { 15, DateTime.Now.AddDays(-365) },
                { 16, DateTime.Now.AddDays(-365) },
                { 17, DateTime.Now.AddDays(-365) },
                { 18, DateTime.Now.AddDays(-365) },
                { 19, DateTime.Now.AddDays(-365) },
                { 20, DateTime.Now.AddDays(-365) },
            }, dbWrapper.FileProcessingDB);

            databaseCleanup.Process(_noCancel);

            var tableRowCounts = GetTableRowCounts(dbWrapper.FileProcessingDB.DatabaseName, dbWrapper.FileProcessingDB.DatabaseServer);
            // The first 10 records are lost sessions and should not be cleaned up.
            Assert.AreEqual(20, tableRowCounts.FileActionStateTransitionTableRowCount);
            Assert.AreEqual(10, tableRowCounts.QueueEventTableRowCount);
        }

        /// <summary>
        /// Tests running the service multiple times.
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("ETL")]
        public static void TestMultipleRunsOfService()
        {
            string testDBName = "Test_DatabaseCleanup_MultipleRunsOfService";

            using var dbWrapper = new OneWorkflow<TestDatabaseCleanup>(_testDbManager, testDBName, false);
            DatabaseCleanup databaseCleanup = CreateTestDatabaseCleanup(dbWrapper);
            databaseCleanup.MaxDaysToProcessPerRun = 5;
            databaseCleanup.PurgeRecordsOlderThanDays = 10;
            PopulateFakeDatabaseData(dbWrapper, 2, 0, 0);
            UpdateDateTimesForFiles(new Dictionary<int, DateTime> {
                { 1, DateTime.Now.AddDays(-365) },
                { 2, DateTime.Now.AddDays(-365) },
            }, dbWrapper.FileProcessingDB);
            databaseCleanup.Process(_noCancel);
            var tableRowCounts = GetTableRowCounts(dbWrapper.FileProcessingDB.DatabaseName, dbWrapper.FileProcessingDB.DatabaseServer);
            //Two rows should be cleaned up
            Assert.AreEqual(0, tableRowCounts.QueueEventTableRowCount);
            Assert.AreEqual(0, tableRowCounts.FileActionStateTransitionTableRowCount);

            PopulateFakeDatabaseData(dbWrapper, 2, 0, 2);
            UpdateDateTimesForFiles(new Dictionary<int, DateTime> {
                { 3, DateTime.Now.AddDays(-365) },
                { 4, DateTime.Now.AddDays(-365) },
            }, dbWrapper.FileProcessingDB);
            databaseCleanup.Process(_noCancel);

            tableRowCounts = GetTableRowCounts(dbWrapper.FileProcessingDB.DatabaseName, dbWrapper.FileProcessingDB.DatabaseServer);
            //Two rows should be cleaned up
            Assert.AreEqual(0, tableRowCounts.QueueEventTableRowCount);
            Assert.AreEqual(0, tableRowCounts.FileActionStateTransitionTableRowCount);
        }

        /// <summary>
        /// A record with a lost session will have a null date time. This should not delete any records, or throw any exceptions.
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("ETL")]
        public static void TestCleanupOneRecordLostSession()
        {
            string testDBName = "Test_DatabaseCleanup_OneRecordLostSession";

            using var dbWrapper = new OneWorkflow<TestDatabaseCleanup>(_testDbManager, testDBName, false);
            DatabaseCleanup databaseCleanup = CreateTestDatabaseCleanup(dbWrapper);
            PopulateFakeDatabaseData(dbWrapper, 1, 1, 0);
            databaseCleanup.Process(_noCancel);
            var tableRowCounts = GetTableRowCounts(dbWrapper.FileProcessingDB.DatabaseName, dbWrapper.FileProcessingDB.DatabaseServer);
            Assert.AreEqual(2, tableRowCounts.FileActionStateTransitionTableRowCount);
            Assert.AreEqual(1, tableRowCounts.QueueEventTableRowCount);
        }

        /// <summary>
        /// Ensure that settings can be restored from the json.
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

        private static void UpdateDateTimesForFiles(Dictionary<int, DateTime> fileToUpdate, IFileProcessingDB fileProcessingDB)
        {
            using ExtractRoleConnection connection = new(fileProcessingDB.DatabaseServer, fileProcessingDB.DatabaseName);
            connection.Open();
            foreach (var file in fileToUpdate)
            {
                using var cmd = connection.CreateCommand();

                cmd.Parameters.AddWithValue("@DateTimeStamp", file.Value);
                cmd.Parameters.AddWithValue("@FileID", file.Key);
                cmd.CommandText = UpdateTablesDatetimeForFileID;
                cmd.ExecuteNonQuery();
            }
        }

        private static void PopulateFakeDatabaseData(OneWorkflow<TestDatabaseCleanup> dbWrapper, int filesToAdd, int lostSessionsToCreate, int startIndex)
        {
            if (startIndex == 0)
                dbWrapper.CreateAttributeSet(AttributeSetName);


            for (int i = startIndex; i < filesToAdd + startIndex; i++)
            {
                int fileID = i + 1;

                dbWrapper.AddFakeFile(fileID, setAsSkipped: false);
                dbWrapper.AddFakeVOA(fileID, AttributeSetName);

            }

            for (int i = startIndex; i < filesToAdd + startIndex; i++)
            {
                int fileID = i + 1;

                int sessionID = dbWrapper.FileProcessingDB.StartFileTaskSession(Constants.TaskClassWebVerification, fileID, 1);
                if (lostSessionsToCreate > 0)
                {
                    lostSessionsToCreate -= 1;
                }
                else
                {
                    dbWrapper.FileProcessingDB.EndFileTaskSession(sessionID, 0, 0, false);
                }
            }
        }

        static DatabaseCleanup CreateTestDatabaseCleanup(OneWorkflow<TestDatabaseCleanup> dbWrapper)
        {
            DatabaseCleanup databaseCleanup = new()
            {
                DatabaseName = dbWrapper.FileProcessingDB.DatabaseName,
                DatabaseServer = dbWrapper.FileProcessingDB.DatabaseServer,
                PurgeRecordsOlderThanDays = 50,
                MaxDaysToProcessPerRun = 10,
            };
            databaseCleanup.AddToDatabase(dbWrapper.FileProcessingDB.DatabaseServer, dbWrapper.FileProcessingDB.DatabaseName);
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

            cmd.CommandText = "Select COUNT(*) FROM [ReportingRedactionAccuracy]";
            tableRowCounts.ReportingRedactionAccuracy = cmd.ExecuteScalar() as Int32?;

            cmd.CommandText = "Select COUNT(*) FROM [ReportingDataCaptureAccuracy]";
            tableRowCounts.ReportingDataCaptureAccuracy = cmd.ExecuteScalar() as Int32?;

            cmd.CommandText = "Select COUNT(*) FROM [DashboardAttributeFields]";
            tableRowCounts.DashboardAttributeFields = cmd.ExecuteScalar() as Int32?;

            return tableRowCounts;
        }
    }
}
