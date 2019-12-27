using Extract.FileActionManager.Database.Test;
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
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testDbManager = new FAMTestDBManager<TestDocumentVerificationRatesService>();
            _testFileManager = new TestFileManager<TestDocumentVerificationRatesService>();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestFixtureTearDown]
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
            string testDBName = taskGuid + "_Test";
            try
            {
                var fileProcessingDb = CreateTestDatabase(testDBName);

                fileProcessingDb.RecordFAMSessionStart("Test.fps", _ACTION_A, true, true);
                fileProcessingDb.RegisterActiveFAM();

                string testfileName1 = _testFileManager.GetFile(_TEST_FILE1);
                string testfileName2 = _testFileManager.GetFile(_TEST_FILE2);
                bool alreadyExists = false;
                EActionStatus previousStatus;
                var fileRecord1 = fileProcessingDb.AddFile(testfileName1, _ACTION_A, -1, EFilePriority.kPriorityNormal,
                    false, false, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);
                var fileRecord2 = fileProcessingDb.AddFile(testfileName2, _ACTION_A, -1, EFilePriority.kPriorityNormal,
                    false, false, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);

                int actionID1 = fileProcessingDb.GetActionID(_ACTION_A);

                int fileTaskSessionID = fileProcessingDb.StartFileTaskSession(taskGuid, fileRecord1.FileID, actionID1);

                fileProcessingDb.EndFileTaskSession(fileTaskSessionID, 10.0, 1.0, 10.0);
                fileProcessingDb.UnregisterActiveFAM();
                fileProcessingDb.RecordFAMSessionStop();

                fileProcessingDb.RecordFAMSessionStart("Test.fps", _ACTION_A, true, true);
                fileProcessingDb.RegisterActiveFAM();

                fileTaskSessionID = fileProcessingDb.StartFileTaskSession(taskGuid, fileRecord2.FileID, actionID1);

                DocumentVerificationRates rates = new DocumentVerificationRates();
                rates.DatabaseServer = fileProcessingDb.DatabaseServer;
                rates.DatabaseName = fileProcessingDb.DatabaseName;
                rates.DatabaseServiceID = StoreDatabaseServiceRecord(rates);

                // This should not process any results
                Assert.Throws<ExtractException>(() => rates.Process(_cancel));
                CheckResults(rates, taskGuid, new VerificationRatesList());

                rates.Process(_noCancel);

                CheckResults(rates, taskGuid, _PROCESS1_EXPECTED);

                // Check the status
                var status = rates.Status as DocumentVerificationStatus;

                Assert.AreEqual(status.LastFileTaskSessionIDProcessed, 1, "LastFileTaskSessionIDProcessed is 1.");
                Assert.That(status.SetOfActiveFileTaskIds.Count == 0, "SetOfActiveFileTaskIds is no longer used.");

                fileProcessingDb.EndFileTaskSession(fileTaskSessionID, 20.0, 2.0, 20.0);
                rates.Process(_noCancel);

                status = rates.Status as DocumentVerificationStatus;
                Assert.AreEqual(status.LastFileTaskSessionIDProcessed, 2, "LastFileTaskSessionIDProcessed is 2.");
                Assert.That(status.SetOfActiveFileTaskIds.Count == 0, "SetOfActiveFileTaskIds is no longer used.");

                CheckResults(rates, taskGuid, _PROCESS2_EXPECTED);

                fileProcessingDb.UnregisterActiveFAM();
                fileProcessingDb.RecordFAMSessionStop();

                rates.Process(_noCancel);

                CheckResults(rates, taskGuid, _PROCESS2_EXPECTED);

                fileProcessingDb.RecordFAMSessionStart("Test.fps", _ACTION_A, true, true);
                fileProcessingDb.RegisterActiveFAM();

                fileTaskSessionID = fileProcessingDb.StartFileTaskSession(taskGuid, fileRecord2.FileID, actionID1);

                fileProcessingDb.EndFileTaskSession(fileTaskSessionID, 30.0, 3.0, 30.0);
                fileProcessingDb.UnregisterActiveFAM();
                fileProcessingDb.RecordFAMSessionStop();

                rates.Process(_noCancel);

                CheckResults(rates, taskGuid, _PROCESS4_EXPECTED);

                // Test that the status gets reset if the database is cleared with retain settings
                fileProcessingDb.Clear(true);

                using (var connection = GetConnection(rates))
                {
                    connection.Open();
                    using (var statusCmd = connection.CreateCommand())
                    {
                        statusCmd.CommandText = "SELECT Status, LastFileTaskSessionIDProcessed FROM DatabaseService WHERE ID = @DatabaseServiceID ";
                        statusCmd.Parameters.AddWithValue("@DatabaseServiceID", rates.DatabaseServiceID);
                        var result = statusCmd.ExecuteReader().Cast<IDataRecord>().SingleOrDefault();
                        Assert.AreNotEqual(null, result, "A single record was returned");
                        Assert.AreEqual(DBNull.Value, result["Status"], "Status is null");
                        Assert.AreEqual(DBNull.Value, result["LastFileTaskSessionIDProcessed"], "LastFileTaskSessionIDProcessed is null");
                    }
                }

                rates.RefreshStatus();
                status = rates.Status as DocumentVerificationStatus;

                Assert.AreEqual(0, status.LastFileTaskSessionIDProcessed, "LastFileTaskSessionIDProcessed should be 0");
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

            return fileProcessingDB;
        }

        static Int32 StoreDatabaseServiceRecord(DocumentVerificationRates rates)
        {
            try
            {
                using (var connection = GetConnection(rates))
                {
                    connection.Open();
                    SqlCommand cmd = connection.CreateCommand();

                    // Database should have one record in the DatabaseService table - not using the settings in the table
                    cmd.CommandText = @"INSERT INTO [dbo].[DatabaseService]([Description], [Settings])
                          OUTPUT inserted.ID
                          VALUES (@Description, @Settings )";
                    cmd.Parameters.Add("@Description", SqlDbType.NVarChar).Value = rates.Description;
                    cmd.Parameters.Add("@Settings", SqlDbType.NVarChar).Value = rates.ToJson();
                    return (Int32)cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45488");
            }
        }

        static SqlConnection GetConnection(DocumentVerificationRates rates)
        {
            // Build the connection string from the settings
            SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder();
            sqlConnectionBuild.DataSource = rates.DatabaseServer;
            sqlConnectionBuild.InitialCatalog = rates.DatabaseName;

            sqlConnectionBuild.IntegratedSecurity = true;
            sqlConnectionBuild.NetworkLibrary = "dbmssocn";
            sqlConnectionBuild.MultipleActiveResultSets = true;
            return new SqlConnection(sqlConnectionBuild.ConnectionString);
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
            using (var connection = GetConnection(rates))
            {
                connection.Open();

                var cmd = connection.CreateCommand();

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

                using (var reader = cmd.ExecuteReader())
                {
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
            }
        }

        #endregion
    }
}
