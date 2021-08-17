using Extract.FileActionManager.Database.Test;
using Extract.SqlDatabase;
using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Extract.ETL.Test
{

    // Define RedactionAccuracyList as a list of tuples for the data in the ReportingRedactionAccuracy table
    using RedactionAccuracyList = List<RedactionAccuracyItem>;
    using I = RedactionAccuracyItem;

    struct RedactionAccuracyItem
    {
        public RedactionAccuracyItem(long foundAttributeSetForFileID,
            long expectedAttributeSetForFileID,
            int fileID,
            int page,
            string attribute,
            long expected,
            long found,
            long correct,
            long falsePositives,
            long overRedacted,
            long underRedacted,
            long missed,
            DateTime foundDateTimeStamp,
            int foundFAMUserID,
            int foundActionID,
            DateTime expectedDateTimeStamp,
            int expectedFAMUserID,
            int expectedActionID)
        {
            FoundAttributeSetForFileId = foundAttributeSetForFileID;
            ExpectedAttributeSetForFileID = expectedAttributeSetForFileID;
            FileID = fileID;
            Page = page;
            Attribute = attribute;
            Expected = expected;
            Found = found;
            Correct = correct;
            FalsePositives = falsePositives;
            OverRedacted = overRedacted;
            UnderRedacted = underRedacted;
            Missed = missed;
            FoundDateTimeStamp = foundDateTimeStamp;
            FoundFAMUserID = foundFAMUserID;
            FoundActionID = foundActionID;
            ExpectedDateTimeStamp = expectedDateTimeStamp;
            ExpectedFAMUserID = expectedFAMUserID;
            ExpectedActionID = expectedActionID;
        }

        public RedactionAccuracyItem(long foundAttributeSetForFileID,
            long expectedAttributeSetForFileID,
            int fileID,
            int page,
            string attribute,
            long expected,
            long found,
            long correct,
            long falsePositives,
            long overRedacted,
            long underRedacted,
            long missed,
            string foundDateTimeStamp,
            int foundFAMUserID,
            int foundActionID,
            string expectedDateTimeStamp,
            int expectedFAMUserID,
            int expectedActionID)
        {
            FoundAttributeSetForFileId = foundAttributeSetForFileID;
            ExpectedAttributeSetForFileID = expectedAttributeSetForFileID;
            FileID = fileID;
            Page = page;
            Attribute = attribute;
            Expected = expected;
            Found = found;
            Correct = correct;
            FalsePositives = falsePositives;
            OverRedacted = overRedacted;
            UnderRedacted = underRedacted;
            Missed = missed;
            FoundDateTimeStamp = DateTime.Parse(foundDateTimeStamp, CultureInfo.CurrentCulture);
            FoundFAMUserID = foundFAMUserID;
            FoundActionID = foundActionID;
            ExpectedDateTimeStamp = DateTime.Parse(expectedDateTimeStamp, CultureInfo.CurrentCulture);
            ExpectedFAMUserID = expectedFAMUserID;
            ExpectedActionID = expectedActionID;
        }

        internal long FoundAttributeSetForFileId;
        internal long ExpectedAttributeSetForFileID;
        internal int FileID;
        internal int Page;
        internal string Attribute;
        internal long Expected;
        internal long Found;
        internal long Correct;
        internal long FalsePositives;
        internal long OverRedacted;
        internal long UnderRedacted;
        internal long Missed;
        internal DateTime FoundDateTimeStamp;
        internal int FoundFAMUserID;
        internal int FoundActionID;
        internal DateTime ExpectedDateTimeStamp;
        internal int ExpectedFAMUserID;
        internal int ExpectedActionID;
    }

    [Category("TestRedactionAccuracyServices")]
    [TestFixture]
    public class TestRedactionAccuracyServices
    {
        #region Constants

        static readonly string _DATABASE = "Resources.FirstRun_IDShield.bak";
        static readonly string _RERUN_DATABASE = "Resources.SecondRun_IDShield.bak";

        #endregion

        #region Fields

        /// <summary>
        /// Manages test FAM DBs
        /// </summary>
        static FAMTestDBManager<TestAccuracyServices> _testDbManager;

        static CancellationToken _noCancel = new CancellationToken(false);
        static CancellationToken _cancel = new CancellationToken(true);

        /// <summary>
        /// List of the expected contents of the ReportingRedactionAccuracy table after the first run
        /// </summary>
        static RedactionAccuracyList _FIRST_RUN_EXPECTED_RESULTS = new RedactionAccuracyList
        {
            new I(1 ,  3,  1 ,  1,   "SSN", 2,   2,   2,   0,   0,   0,   0, "2019-10-11 09:06:53 AM", 1, 1, "2019-10-11 09:09:13 AM", 1, 3),
            new I(2 ,  4,  2 ,  1,   "SSN", 1,   0,   0,   0,   0,   0,   1, "2019-10-11 09:07:25 AM", 1, 1, "2019-10-11 09:09:32 AM", 1, 3)
        };

        /// <summary>
        /// List of the expected contents of the ReportingRedactionAccuracy table after a rerun of the file from the first run
        /// </summary>
        static RedactionAccuracyList _RERUN_FILE_EXPECTED_RESULTS = new RedactionAccuracyList
        {//this is not correct - 
            new I(1 ,  5,  1 ,  1,   "SSN", 2,   2,   2,   0,   0,   0,   0, "2019-10-11 09:06:53 AM", 1, 1, "2019-10-11 02:51:41 PM", 1, 3),
            new I(2 ,  6,  2 ,  1,   "SSN", 2,   0,   0,   0,   0,   0,   2, "2019-10-11 09:07:25 AM", 1, 1, "2019-10-11 02:52:01 PM", 1, 3)
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

        #endregion Overhead

        #region Unit Tests

        /// <summary>
        /// Test that if no Database configuration is set that the call to Process throws an exception
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("ETL")]
        public static void TestNoDBProcess()
        {
            var redactionAccuracy = new RedactionAccuracy();
            Assert.Throws<ExtractException>(() => redactionAccuracy.Process(_noCancel), "Process should throw an exception");
        }

        /// <summary>
        /// Test the use of the IDatabase.Process command for RedactionAccuracy Service
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("ETL")]
        public static void TestRedactionAccuracyServiceStatus()
        {
            string testDBName = "RedactionAccuracyServiceStatus_Test";
            try
            {
                // This is only used to initialize the database used for calculating the stats
                var fileProcessingDb = _testDbManager.GetDatabase(_DATABASE, testDBName);

                // Create DataCaptureAccuracy object using the initialized database
                RedactionAccuracy redactionAccuracy = CreateTestRedactionAccuracy(fileProcessingDb.DatabaseServer,
                    fileProcessingDb.DatabaseName);

                redactionAccuracy.RefreshStatus();
                var status = redactionAccuracy.Status as RedactionAccuracy.RedactionAccuracyStatus;

                Assert.AreEqual(0, status.LastFileTaskSessionIDProcessed, "LastFileTaskSessionIDProcessed is 0");

                // Process using the settings
                redactionAccuracy.Process(_noCancel);

                // Status should have been updated by the process
                status = redactionAccuracy.Status as RedactionAccuracy.RedactionAccuracyStatus;

                Assert.AreEqual(8, status.LastFileTaskSessionIDProcessed, "LastFileTaskSessionIDProcessed is 8");

                // Refresh from the Database to make sure the database was updated properly
                redactionAccuracy.RefreshStatus();
                status = redactionAccuracy.Status as RedactionAccuracy.RedactionAccuracyStatus;

                Assert.AreEqual(8, status.LastFileTaskSessionIDProcessed, "LastFileTaskSessionIDProcessed is 8");

                var result = TestServiceRecordInDatabase(redactionAccuracy);
                Assert.AreNotEqual(DBNull.Value, result["Status"], "Status should not be null");
                Assert.AreNotEqual(DBNull.Value, result["LastFileTaskSessionIDProcessed"], "LastFileTaskSessionIDProcessed should not be null");

                fileProcessingDb.Clear(true);

                result = TestServiceRecordInDatabase(redactionAccuracy);
                Assert.AreEqual(DBNull.Value, result["Status"], "Status should not be null");
                Assert.AreEqual(DBNull.Value, result["LastFileTaskSessionIDProcessed"], "LastFileTaskSessionIDProcessed should not be null");

                redactionAccuracy.RefreshStatus();
                status = redactionAccuracy.Status as RedactionAccuracy.RedactionAccuracyStatus;

                Assert.AreEqual(0, status.LastFileTaskSessionIDProcessed, "LastFileTaskSessionIDProcessed should be 0");
            }
            finally
            {
                _testDbManager.RemoveDatabase(testDBName);
            }
        }

        private static IDataRecord TestServiceRecordInDatabase(RedactionAccuracy redactionAccuracy)
        {
            using var connection = new ExtractRoleConnection(redactionAccuracy.DatabaseServer, redactionAccuracy.DatabaseName);
            connection.Open();
            using var statusCmd = connection.CreateCommand();

            statusCmd.CommandText = "SELECT Status, LastFileTaskSessionIDProcessed FROM DatabaseService WHERE ID = @DatabaseServiceID ";
            statusCmd.Parameters.AddWithValue("@DatabaseServiceID", redactionAccuracy.DatabaseServiceID);
            var result = statusCmd.ExecuteReader().Cast<IDataRecord>().SingleOrDefault();
            Assert.AreNotEqual(null, result, "A single record should be returned");
            return result;

        }

        /// <summary>
        /// Test the use of the IDatabase.Process command for RedactionAccuracy Service
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("ETL")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "<Pending>")]
        public static void TestRedactionAccuracyServiceProcessNoExistingData()
        {
            string testDBName = "RedactionAccuracyService_Test";
            try
            {
                // This is only used to initialize the database used for calculating the stats
                var fileProcessingDb = _testDbManager.GetDatabase(_DATABASE, testDBName);

                // Create DataCaptureAccuracy object using the initialized database
                RedactionAccuracy redactionAccuracy = CreateTestRedactionAccuracy(fileProcessingDb.DatabaseServer,
                    fileProcessingDb.DatabaseName);


                // with the _cancel token there should be no results
                Assert.Throws<ExtractException>(() => redactionAccuracy.Process(_cancel));

                using var connection = new ExtractRoleConnection(redactionAccuracy.DatabaseServer, redactionAccuracy.DatabaseName);
                connection.Open();

                using SqlCommand cmd = connection.CreateCommand();

                cmd.CommandText = "Select Count(ID) from ReportingRedactionAccuracy";

                Assert.AreEqual(cmd.ExecuteScalar() as Int32?, 0);

                // Process using the settings
                redactionAccuracy.Process(_noCancel);

                // Get the data from the database after processing
                cmd.CommandText =
                    $"SELECT * FROM ReportingRedactionAccuracy WHERE DatabaseServiceID = {redactionAccuracy.DatabaseServiceID}";

                CheckResults(cmd.ExecuteReader(), _FIRST_RUN_EXPECTED_RESULTS);

                // Run again - There should be no changes
                redactionAccuracy.Process(_noCancel);

                CheckResults(cmd.ExecuteReader(), _FIRST_RUN_EXPECTED_RESULTS);
            }
            finally
            {
                _testDbManager.RemoveDatabase(testDBName);
            }
        }

        /// <summary>
        /// Test the use of the IDatabase.Process command for DataCaptureAccuracy Service with newer data that previous run
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("ETL")]
        public static void TestRedactionAccuracyServiceProcessExistingData()
        {
            string testDBName = "RedactionAccuracyService_Test";
            try
            {
                // This is only used to initialize the database used for calculating the stats
                var fileProcessingDb = _testDbManager.GetDatabase(_RERUN_DATABASE, testDBName);

                // Create DataCaptureAccuracy object using the initialized database
                RedactionAccuracy redactionAccuracy = CreateTestRedactionAccuracy(fileProcessingDb.DatabaseServer,
                    fileProcessingDb.DatabaseName);

                // Process using the settings
                redactionAccuracy.Process(_noCancel);

                using var connection = new ExtractRoleConnection(redactionAccuracy.DatabaseServer, redactionAccuracy.DatabaseName);
                connection.Open();

                using SqlCommand cmd = connection.CreateCommand();

                // Get the data from the database after processing
                cmd.CommandText = string.Format(CultureInfo.InvariantCulture, @"
                        SELECT * FROM ReportingRedactionAccuracy WHERE DatabaseServiceID = {0}"
                    , redactionAccuracy.DatabaseServiceID);

                CheckResults(cmd.ExecuteReader(), _RERUN_FILE_EXPECTED_RESULTS);

                // Run again - There should be no changes
                redactionAccuracy.Process(_noCancel);

                CheckResults(cmd.ExecuteReader(), _RERUN_FILE_EXPECTED_RESULTS);
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
        public static void TestRedactionAccuracyGetSettingsAndLoad()
        {
            RedactionAccuracy redactionAccuracy = new RedactionAccuracy();
            redactionAccuracy.DatabaseName = "Database";
            redactionAccuracy.DatabaseServer = "Server";
            redactionAccuracy.FoundAttributeSetName = "DataFoundByRules";
            redactionAccuracy.ExpectedAttributeSetName = "DataSavedByOperator";
            redactionAccuracy.XPathOfSensitiveAttributes = "XPath to ignore test";

            redactionAccuracy.Description = "Test Description";

            string settings = redactionAccuracy.ToJson();

            var newDCAFromSettings = DatabaseService.FromJson(settings);

            Assert.IsTrue(string.IsNullOrEmpty(newDCAFromSettings.DatabaseName));
            Assert.IsTrue(string.IsNullOrEmpty(newDCAFromSettings.DatabaseServer));
            Assert.AreEqual(redactionAccuracy.Description, newDCAFromSettings.Description);

            RedactionAccuracy ra = newDCAFromSettings as RedactionAccuracy;

            Assert.IsNotNull(ra);

            Assert.AreEqual(redactionAccuracy.FoundAttributeSetName, ra.FoundAttributeSetName);
            Assert.AreEqual(redactionAccuracy.ExpectedAttributeSetName, ra.ExpectedAttributeSetName);
            Assert.AreEqual(redactionAccuracy.XPathOfSensitiveAttributes, ra.XPathOfSensitiveAttributes);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Compares what is in the foundResults with expected
        /// </summary>
        /// <param name="foundResults">The SqlDataReader for the results generated</param>
        /// <param name="expected">The Expected results</param>
        static void CheckResults(SqlDataReader foundResults, RedactionAccuracyList expected)
        {
            // Convert reader to IEnummerable 
            var results = foundResults.Cast<IDataRecord>();

            // Compare the found results to the expected
            var formattedResults = results.Select(r => new I(
                   foundAttributeSetForFileID: r.GetInt64(r.GetOrdinal("FoundAttributeSetForFileID")),
                   expectedAttributeSetForFileID: r.GetInt64(r.GetOrdinal("ExpectedAttributeSetForFileID")),
                   fileID: r.GetInt32(r.GetOrdinal("FileID")),
                   page: r.GetInt32(r.GetOrdinal("Page")),
                   attribute: r.GetString(r.GetOrdinal("Attribute")),
                   expected: r.GetInt64(r.GetOrdinal("Expected")),
                   found: r.GetInt64(r.GetOrdinal("Found")),
                   correct: r.GetInt64(r.GetOrdinal("Correct")),
                   falsePositives: r.GetInt64(r.GetOrdinal("FalsePositives")),
                   overRedacted: r.GetInt64(r.GetOrdinal("OverRedacted")),
                   underRedacted: r.GetInt64(r.GetOrdinal("UnderRedacted")),
                   missed: r.GetInt64(r.GetOrdinal("Missed")),
                   foundDateTimeStamp: r.GetDateTime(r.GetOrdinal("FoundDateTimeStamp")),
                   foundFAMUserID: r.GetInt32(r.GetOrdinal("FoundFAMUserID")),
                   foundActionID: r.GetInt32(r.GetOrdinal("FoundActionID")),
                   expectedDateTimeStamp: r.GetDateTime(r.GetOrdinal("ExpectedDateTimeStamp")),
                   expectedFAMUserID: r.GetInt32(r.GetOrdinal("ExpectedFAMUserID")),
                   expectedActionID: r.GetInt32(r.GetOrdinal("ExpectedActionID")))).ToList();


            Assert.That(formattedResults.Count() == expected.Count,
                string.Format(CultureInfo.InvariantCulture, "Found {0} and expected {1} ",
                formattedResults.Count(), expected.Count));

            Assert.That(formattedResults
                    .OrderBy(a => a.Attribute)
                    .ThenBy(a => a.FileID)
                .SequenceEqual(expected
                    .OrderBy(r => r.Attribute)
                    .ThenBy(r => r.FileID)),
                "Compare the actual data with the expected");

            foundResults.Close();
        }

        /// <summary>
        /// Creates a RedactionAccuracy service object for use in testing, the settings are set to the DatabaseServiceID = 1
        /// and the database gets updated to contain the objects settings in the DatabaseService table
        /// </summary>
        /// <param name="databaseServer">Database server to use</param>
        /// <param name="databaseName">Database name to use</param>
        /// <returns>A new <see cref="RedactionAccuracy"/> object that is associated with DatabaseServiceID 1 in the database</returns>
        static RedactionAccuracy CreateTestRedactionAccuracy(string databaseServer, string databaseName)
        {
            RedactionAccuracy accuracy = new RedactionAccuracy
            {
                DatabaseServiceID = 1,
                DatabaseName = databaseName,
                DatabaseServer = databaseServer,
                FoundAttributeSetName = "DataFoundByRules",
                ExpectedAttributeSetName = "DataSavedByOperator",
            };
            accuracy.UpdateDatabaseServiceSettings();
            return accuracy;
        }

        #endregion
    }
}
