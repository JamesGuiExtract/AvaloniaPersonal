using Extract.FileActionManager.Database.Test;
using Extract.SqlDatabase;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading;
using UCLID_FILEPROCESSINGLib;
using static Extract.ETL.DocumentVerificationRates;

namespace Extract.ETL.Test
{
    // Define VerificationRates as a list of tuples for the data in the ReportingVerificationRates table
    using VerificationRatesList =
        List<(Int32 DatabaseServiceID,
            Int32 FileID,
            Int32 ActionID,
            string TaskClassGuid,
            Int32 LastFileTaskSessionID,
            Double Duration,
            Double OverheadTime,
            Double ActivityTime)>;

    /// <summary>
    /// Class to test the AttributeExpander
    /// </summary>
    [Category("TestDocumentVerificationRatesService")]
    [TestFixture]
    public class TestDocumentVerificationRatesService
    {

        #region Constants

        static readonly string _TEST_FILE1 = "Resources.TestImage001.tif";
        static readonly string _TEST_FILE2 = "Resources.TestImage002.tif";

        static readonly string _ACTION_A = "ActionA";
        static readonly string _ACTION_B = "ActionB";

        #endregion

        #region Fields

        static CancellationToken _noCancel = new CancellationToken(false);
        static CancellationToken _cancel = new CancellationToken(true);

        /// <summary>
        /// Manages test FAM DBs
        /// </summary>
        static FAMTestDBManager<TestDocumentVerificationRatesService> _testDbManager;

        /// <summary>
        /// Managers test files
        /// </summary>
        static TestFileManager<TestDocumentVerificationRatesService> _testFileManager;

        static VerificationRatesList _PROCESS1_EXPECTED = new VerificationRatesList
        {
            (1, 1, 1, "", 1, 10.0, 1.0, 10.0)
        };

        static VerificationRatesList _PROCESS2_EXPECTED = new VerificationRatesList
        {
            (1, 1, 1, "", 1, 10.0, 1.0, 10.0),
            (1, 2, 1, "", 2, 20.0, 2.0, 20.0)
        };

        static VerificationRatesList _PROCESS4_EXPECTED = new VerificationRatesList
        {
            (1, 1, 1, "", 1, 10.0, 1.0, 10.0),
            (1, 2, 1, "", 3, 50.0, 5.0, 50.0)
        };

        #endregion

        #region Overhead

        /// <summary>
        /// Initializes the test fixture.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testDbManager = new FAMTestDBManager<TestDocumentVerificationRatesService>();
            _testFileManager = new TestFileManager<TestDocumentVerificationRatesService>();
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
            if (_testFileManager != null)
            {
                _testFileManager.Dispose();
                _testFileManager = null;
            }
        }

        #endregion Overhead

        #region Unit tests

        /// <summary>
        /// Test that if no Database configuration is set that the call to Process throws an exception
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("ETL")]
        public static void TestNoDBProcess()
        {
            var documentVerificationRates = new DocumentVerificationRates();
            Assert.Throws<ExtractException>(() => documentVerificationRates.Process(_noCancel), "Process should throw an exception");
        }

        [Test]
        [Category("Automated")]
        [Category("ETL")]
        public static void TestDocumentVerificationRatesServiceSerialization()
        {
            DocumentVerificationRates verificationRates = new DocumentVerificationRates();
            verificationRates.DatabaseServer = "TestService";
            verificationRates.DatabaseName = "TestDatabase";
            verificationRates.DatabaseServiceID = 1;
            verificationRates.Description = "Test description";
            verificationRates.Enabled = !verificationRates.Enabled;
            verificationRates.Schedule = new ScheduledEvent();
            verificationRates.Schedule.Start = new DateTime(2018, 1, 1);
            verificationRates.Schedule.RecurrenceUnit = DateTimeUnit.Minute;

            string settings = verificationRates.ToJson();

            DocumentVerificationRates testRates = (DocumentVerificationRates)DocumentVerificationRates.FromJson(settings);

            Assert.That(string.IsNullOrWhiteSpace(testRates.DatabaseServer), "DatabaseServer should not be saved.");
            Assert.That(string.IsNullOrWhiteSpace(testRates.DatabaseName), "DatabaseName should not be saved.");
            Assert.That(testRates.DatabaseServiceID == 0, "DatabaseServiceID should be default of 0.");
            Assert.That(testRates.Enabled == !verificationRates.Enabled, "Enabled should be in the default state.");
            Assert.AreEqual(verificationRates.Description, testRates.Description, "Description serialization worked.");
            Assert.IsNotNull(verificationRates.Schedule, "Schedule was set so should not be null.");
            Assert.AreEqual(verificationRates.Schedule.Start, new DateTime(2018, 1, 1));
            Assert.AreEqual(verificationRates.Schedule.RecurrenceUnit, DateTimeUnit.Minute);
        }

        [Test]
        [Category("Automated")]
        [Category("ETL")]
        public static void TestDocumentVerificationStatusSerialization()
        {
            DocumentVerificationStatus serviceStatus = new DocumentVerificationStatus();

            string settings = serviceStatus.ToJson();

            DocumentVerificationStatus testStatus = (DocumentVerificationStatus)DocumentVerificationStatus.FromJson(settings);

            Assert.AreEqual(serviceStatus.LastFileTaskSessionIDProcessed, testStatus.LastFileTaskSessionIDProcessed,
                "LastFileTaskSessionIDProcessed should be serialized.");
        }

        [Test]
        [Category("Automated")]
        [Category("ETL")]
        [TestCase(Constants.TaskClassPaginationVerification, TestName = "Pagination: Verify")]
        [TestCase(Constants.TaskClassWebVerification, TestName = "Core: Web verification")]
        [TestCase(Constants.TaskClassDataEntryVerification, TestName = "Data Entry: Verify extracted data")]
        [TestCase(Constants.TaskClassRedactionVerification, TestName = "Redaction: Verify sensitive data")]
        public static void TestDocumentVerificationRatesServiceProcess(string taskGuid)
        {
            string testDBName = "Test_DocumentVerificationRatesServiceProcess_" + taskGuid;
            try
            {
                var setupData = SetupDatabase(testDBName);

                int actionID1 = setupData.db.GetActionID(_ACTION_A);

                PerformTestSession(setupData.db, taskGuid, setupData.fileRecord1.FileID, actionID1, 10.0, 1.0, 10.0);

                DocumentVerificationRates rates = new DocumentVerificationRates();
                rates.DatabaseServer = setupData.db.DatabaseServer;
                rates.DatabaseName = setupData.db.DatabaseName;
                rates.DatabaseServiceID = StoreDatabaseServiceRecord(rates);

                // This should not process any results
                Assert.Throws<ExtractException>(() => rates.Process(_cancel));
                CheckResults(rates, taskGuid, new VerificationRatesList());

                rates.Process(_noCancel);

                CheckResults(rates, taskGuid, _PROCESS1_EXPECTED);

                // Test that a StartedFileTaskSession does not affect the results

                setupData.db.RecordFAMSessionStart("Test.fps", _ACTION_A, true, true);
                setupData.db.RegisterActiveFAM();
                int fileTaskSessionID = setupData.db.StartFileTaskSession(taskGuid, setupData.fileRecord2.FileID, actionID1);

                rates.Process(_noCancel);

                CheckResults(rates, taskGuid, _PROCESS1_EXPECTED);

                // Check the status
                var status = rates.Status as DocumentVerificationStatus;

                Assert.AreEqual(status.LastFileTaskSessionIDProcessed, 1, "LastFileTaskSessionIDProcessed is 1.");
                Assert.That(status.SetOfActiveFileTaskIds.Count == 0, "SetOfActiveFileTaskIds is no longer used.");

                setupData.db.EndFileTaskSession(fileTaskSessionID, 20.0, 2.0, 20.0);
                rates.Process(_noCancel);

                status = rates.Status as DocumentVerificationStatus;
                Assert.AreEqual(status.LastFileTaskSessionIDProcessed, 2, "LastFileTaskSessionIDProcessed is 2.");
                Assert.That(status.SetOfActiveFileTaskIds.Count == 0, "SetOfActiveFileTaskIds is no longer used.");

                CheckResults(rates, taskGuid, _PROCESS2_EXPECTED);

                setupData.db.UnregisterActiveFAM();
                setupData.db.RecordFAMSessionStop();

                rates.Process(_noCancel);

                CheckResults(rates, taskGuid, _PROCESS2_EXPECTED);

                PerformTestSession(setupData.db, taskGuid, setupData.fileRecord2.FileID, actionID1, 30.0, 3.0, 30.0);

                rates.Process(_noCancel);

                CheckResults(rates, taskGuid, _PROCESS4_EXPECTED);

                // Test that the status gets reset if the database is cleared with retain settings
                setupData.db.Clear(true);

                using var connection = new ExtractRoleConnection(rates.DatabaseServer, rates.DatabaseName);
                connection.Open();
                var statusCmd = connection.CreateCommand();

                statusCmd.CommandText = "SELECT Status, LastFileTaskSessionIDProcessed FROM DatabaseService WHERE ID = @DatabaseServiceID ";
                statusCmd.Parameters.AddWithValue("@DatabaseServiceID", rates.DatabaseServiceID);
                using var reader = statusCmd.ExecuteReader();
                var result = reader.Cast<IDataRecord>().SingleOrDefault();
                Assert.AreNotEqual(null, result, "A single record was returned");
                Assert.AreEqual(DBNull.Value, result["Status"], "Status is null");
                Assert.AreEqual(DBNull.Value, result["LastFileTaskSessionIDProcessed"], "LastFileTaskSessionIDProcessed is null");

                rates.RefreshStatus();
                status = rates.Status as DocumentVerificationStatus;

                Assert.AreEqual(0, status.LastFileTaskSessionIDProcessed, "LastFileTaskSessionIDProcessed should be 0");
            }
            finally
            {
                _testDbManager.RemoveDatabase(testDBName);
            }
        }

        [Test]
        [Category("Automated")]
        [Category("ETL")]
        [TestCase(Constants.TaskClassPaginationVerification, 10.0,  0.0,  0.0, TestName = "Pagination: Verify 10.0,  0.0,  0.0")]
        [TestCase(Constants.TaskClassPaginationVerification, 10.0,  0.0, 10.0, TestName = "Pagination: Verify 10.0,  0.0, 10.0")]
        [TestCase(Constants.TaskClassPaginationVerification, 10.0, 10.0,  0.0, TestName = "Pagination: Verify 10.0, 10.0,  0.0")]
        [TestCase(Constants.TaskClassPaginationVerification, 10.0, 10.0, 10.0, TestName = "Pagination: Verify 10.0, 10.0, 10.0")]
        [TestCase(Constants.TaskClassPaginationVerification,  0.0, 10.0, 10.0, TestName = "Pagination: Verify  0.0, 10.0, 10.0")]
        [TestCase(Constants.TaskClassWebVerification, 10.0,  0.0,  0.0, TestName = "Core: Web verification 10.0,  0.0,  0.0")]
        [TestCase(Constants.TaskClassWebVerification, 10.0,  0.0, 10.0, TestName = "Core: Web verification 10.0,  0.0, 10.0")]
        [TestCase(Constants.TaskClassWebVerification, 10.0, 10.0,  0.0, TestName = "Core: Web verification 10.0, 10.0,  0.0")]
        [TestCase(Constants.TaskClassWebVerification, 10.0, 10.0, 10.0, TestName = "Core: Web verification 10.0, 10.0, 10.0")]
        [TestCase(Constants.TaskClassWebVerification,  0.0, 10.0, 10.0, TestName = "Core: Web verification  0.0, 10.0, 10.0")]
        [TestCase(Constants.TaskClassDataEntryVerification, 10.0,  0.0,  0.0, TestName = "Data Entry: Verify extracted data 10.0,  0.0,  0.0")]
        [TestCase(Constants.TaskClassDataEntryVerification, 10.0,  0.0, 10.0, TestName = "Data Entry: Verify extracted data 10.0,  0.0, 10.0")]
        [TestCase(Constants.TaskClassDataEntryVerification, 10.0, 10.0,  0.0, TestName = "Data Entry: Verify extracted data 10.0, 10.0,  0.0")]
        [TestCase(Constants.TaskClassDataEntryVerification, 10.0, 10.0, 10.0, TestName = "Data Entry: Verify extracted data 10.0, 10.0, 10.0")]
        [TestCase(Constants.TaskClassDataEntryVerification,  0.0, 10.0, 10.0, TestName = "Data Entry: Verify extracted data  0.0, 10.0, 10.0")]
        [TestCase(Constants.TaskClassRedactionVerification, 10.0,  0.0,  0.0, TestName = "Redaction: Verify sensitive data 10.0,  0.0,  0.0")]
        [TestCase(Constants.TaskClassRedactionVerification, 10.0,  0.0, 10.0, TestName = "Redaction: Verify sensitive data 10.0,  0.0, 10.0")]
        [TestCase(Constants.TaskClassRedactionVerification, 10.0, 10.0,  0.0, TestName = "Redaction: Verify sensitive data 10.0, 10.0,  0.0")]
        [TestCase(Constants.TaskClassRedactionVerification, 10.0, 10.0, 10.0, TestName = "Redaction: Verify sensitive data 10.0, 10.0, 10.0")]
        [TestCase(Constants.TaskClassRedactionVerification,  0.0, 10.0, 10.0, TestName = "Redaction: Verify sensitive data  0.0, 10.0, 10.0")]
        [CLSCompliant(false)]
        // Added for https://extract.atlassian.net/browse/ISSUE-16928 
        public static void TestDocumentVerificationRatesZeroTimes(string taskGuid, double duration,
            double overheadTime, double activityTime)
        {
            string testDBName = "Test_DocumentVerificationRatesZeroTimes_" + taskGuid;
            try
            {
                var setupData = SetupDatabase(testDBName);

                int actionID1 = setupData.db.GetActionID(_ACTION_A);

                PerformTestSession(setupData.db, taskGuid, setupData.fileRecord1.FileID, actionID1, duration, overheadTime, activityTime);

                DocumentVerificationRates rates = new DocumentVerificationRates();
                rates.DatabaseServer = setupData.db.DatabaseServer;
                rates.DatabaseName = setupData.db.DatabaseName;
                rates.DatabaseServiceID = StoreDatabaseServiceRecord(rates);

                var expected = duration != 0 ? new VerificationRatesList
                {
                    (1, 1, 1, "", 1, duration, overheadTime, activityTime)
                } : new VerificationRatesList();

                rates.Process(_noCancel);

                CheckResults(rates, taskGuid, expected);
            }
            finally
            {
                _testDbManager.RemoveDatabase(testDBName);
            }
        }

        [Test]
        [Category("Automated")]
        [Category("ETL")]
        [TestCase(Constants.TaskClassPaginationVerification, TestName = "Pagination: Verify")]
        [TestCase(Constants.TaskClassWebVerification, TestName = "Core: Web verification")]
        [TestCase(Constants.TaskClassDataEntryVerification, TestName = "Data Entry: Verify extracted data")]
        [TestCase(Constants.TaskClassRedactionVerification, TestName = "Redaction: Verify sensitive data")]
        // Added for https://extract.atlassian.net/browse/ISSUE-16932
        public static void TestDocumentVerificationRatesNullActionID(string taskGuid)
        {
            string testDBName = "Test_DocumentVerificationRatesNullActionID_" + taskGuid;
            try
            {
                var setupData = SetupDatabase(testDBName);

                int actionIDA = setupData.db.GetActionID(_ACTION_A);
                int actionIDB = setupData.db.GetActionID(_ACTION_B);

                PerformTestSession(setupData.db, taskGuid, setupData.fileRecord1.FileID, actionIDA, 10.0, 0.0, 10.0);
                
                PerformTestSession(setupData.db, taskGuid, setupData.fileRecord1.FileID, actionIDB, 10.0, 0.0, 10.0);

                setupData.db.DeleteAction(_ACTION_B);

                DocumentVerificationRates rates = new DocumentVerificationRates();
                rates.DatabaseServer = setupData.db.DatabaseServer;
                rates.DatabaseName = setupData.db.DatabaseName;
                rates.DatabaseServiceID = StoreDatabaseServiceRecord(rates);

                var expected = new VerificationRatesList
                {
                    (1, 1, 1, "", 1, 10.0, 0.0, 10.0)
                };

                rates.Process(_noCancel);

                CheckResults(rates, taskGuid, expected);
            }
            finally
            {
                _testDbManager.RemoveDatabase(testDBName);
            }
        }


        #endregion

        #region Helper methods

        static IFileProcessingDB CreateTestDatabase(string DBName)
        {
            var fileProcessingDB = _testDbManager.GetNewDatabase(DBName);

            // Create 2 actions
            fileProcessingDB.DefineNewAction(_ACTION_A);
            fileProcessingDB.DefineNewAction(_ACTION_B);

            return fileProcessingDB;
        }

        static Int32 StoreDatabaseServiceRecord(DocumentVerificationRates rates)
        {
            try
            {
                using var connection = new ExtractRoleConnection(rates.DatabaseServer, rates.DatabaseName);
                connection.Open();
                using var cmd = connection.CreateCommand();

                // Database should have one record in the DatabaseService table - not using the settings in the table
                cmd.CommandText = @"INSERT INTO [dbo].[DatabaseService]([Description], [Settings])
                          OUTPUT inserted.ID
                          VALUES (@Description, @Settings )";
                cmd.Parameters.Add("@Description", SqlDbType.NVarChar).Value = rates.Description;
                cmd.Parameters.Add("@Settings", SqlDbType.NVarChar).Value = rates.ToJson();
                return (Int32)cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45488");
            }
        }

        static void CheckResults(DocumentVerificationRates rates, string taskGuid, VerificationRatesList expected)
        {
            // Put the taskClassID in the expected
            var adjustedExpected = expected.Select(e => (
                    DatabaseServiceID: e.DatabaseServiceID,
                    FileID: e.FileID,
                    ActionID: e.ActionID,
                    TaskClassGuid: taskGuid,
                    LastFileTaskSessionID: e.LastFileTaskSessionID,
                    Duration: e.Duration,
                    OverheadTime: e.OverheadTime,
                    ActivityTime: e.ActivityTime
                    )).ToList();

            using var connection = new ExtractRoleConnection(rates.DatabaseServer, rates.DatabaseName);
            connection.Open();
            using var cmd = connection.CreateCommand();


            cmd.CommandText = "SELECT COUNT(ID) FROM [dbo].[ReportingVerificationRates]";

            Assert.AreEqual(cmd.ExecuteScalar() as Int32?, adjustedExpected.Count);

            cmd.CommandText = @"
                    SELECT    [DatabaseServiceID]
                                ,[FileID]
                                ,[ActionID]
                                ,CONVERT(nvarchar(50), TaskClass.GUID) TaskClassGuid
                                ,[LastFileTaskSessionID]
                                ,[Duration]
                                ,[OverheadTime]
                                ,[ActivityTime]  
                    FROM [dbo].[ReportingVerificationRates] INNER JOIN [dbo].[TaskClass] 
                        ON [dbo].[ReportingVerificationRates].TaskClassID = [dbo].[TaskClass].ID";

            using var reader = cmd.ExecuteReader();

            // Convert reader to IEnummerable
            var results = reader.Cast<IDataRecord>();

            var resultsInTuples = results.Select(r => (
                DatabaseServiceID: r.GetInt32(r.GetOrdinal("DatabaseServiceID")),
                FileID: r.GetInt32(r.GetOrdinal("FileID")),
                ActionID: r.GetInt32(r.GetOrdinal("ActionID")),
                TaskClassGuid: r.GetString(r.GetOrdinal("TaskClassGuid")),
                LastFileTaskSessionID: r.GetInt32(r.GetOrdinal("LastFileTaskSessionID")),
                Duration: r.GetDouble(r.GetOrdinal("Duration")),
                OverheadTime: r.GetDouble(r.GetOrdinal("OverheadTime")),
                ActivityTime: r.GetDouble(r.GetOrdinal("ActivityTime"))
            )).ToList();

            Assert.AreEqual(resultsInTuples.Count, adjustedExpected.Count,
                string.Format(CultureInfo.InvariantCulture, "Found {0} and expected {1}",
                resultsInTuples.Count, adjustedExpected.Count));
            Assert.That(resultsInTuples
                    .OrderBy(a => a.DatabaseServiceID)
                    .ThenBy(a => a.FileID)
                .SequenceEqual(adjustedExpected
                    .OrderBy(r => r.DatabaseServiceID)
                    .ThenBy(r => r.FileID)),
                "Compare the actual data with the expected");
        }

        static (IFileProcessingDB db, string fileName1, FileRecord fileRecord1, string fileName2, FileRecord fileRecord2) SetupDatabase(string testDBName)
        {
            var fileProcessingDb = CreateTestDatabase(testDBName);

            string testFileName1 = _testFileManager.GetFile(_TEST_FILE1);
            string testFileName2 = _testFileManager.GetFile(_TEST_FILE2);
            bool alreadyExists = false;
            EActionStatus previousStatus;
            var fileRecord1 = fileProcessingDb.AddFile(testFileName1, _ACTION_A, -1, EFilePriority.kPriorityNormal,
                false, false, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);
            var fileRecord2 = fileProcessingDb.AddFile(testFileName2, _ACTION_A, -1, EFilePriority.kPriorityNormal,
                false, false, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);
            return (fileProcessingDb, testFileName1, fileRecord1, testFileName2, fileRecord2);

        }

        static void PerformTestSession(IFileProcessingDB db, string taskGuid, int fileID, int actionID, double duration,
            double overheadTime, double activityTime)
        {
            db.RecordFAMSessionStart("Test.fps", _ACTION_A, true, true);
            db.RegisterActiveFAM();

            int fileTaskSessionID = db.StartFileTaskSession(taskGuid, fileID, actionID);

            db.EndFileTaskSession(fileTaskSessionID, duration, overheadTime, activityTime);
            db.UnregisterActiveFAM();
            db.RecordFAMSessionStop();
        }

        #endregion
    }
}
