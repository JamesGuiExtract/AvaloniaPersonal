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
using static System.DateTime;

namespace Extract.ETL.Test
{
    // Define DataEntryAccuracyList as a list of tuples for the data in the ReportingDataCaptureAccuracy table
    using DataEntryAccuracyList =
        List<(Int64 FoundAttributeSetForFileID,
            Int64 ExpectedAttributeSetForFileID,
            string Attribute,
            Int64 Correct,
            Int64 Expected,
            Int64 Incorrect,
            Int32 FileID,
            DateTime FoundDateTimeStamp,
            Int32 FoundFAMUserID,
            Int32 FoundActionID,
            DateTime ExpectedDateTimeStamp,
            Int32 ExpectedFAMUserID,
            Int32 ExpectedActionID)>;

    [Category("TestAccuracyServices")]
    [TestFixture]
    public class TestAccuracyServices
    {
        #region Constants

        static readonly string _DATABASE = "Resources.AccuracyDemo_LabDE.bak";
        static readonly string _RERUN_DATABASE = "Resources.AccuracyDemo_LabDE-rerun1.bak";

        static readonly string _XPATH_CONTAINERS = @"
                    /*//*[not(text()) or text()='N/A']
                  | /*/PatientInfo/Name | /*/PhysicianInfo/*";

        static readonly string _XPATH_IGNORE = @"
                    /*//*[not(.//text())]
                  | /*/LabInfo
                  | /*/ClueOnFirstPage
                  | /*/DeptCode
                  | /*/ResultStatus
                  | /*/Test/LabInfo
                  | /*/Test/EpicCode
                  | /*/Test/Component/OriginalName
                  | /*/Filename
                  | /*/MessageSequenceNumberFile
                  | /*/OperatorComments
                  | /*/PatientInfo/MR_Number
                  | /*/PatientInfo/Name/Title
                  | /*/PatientInfo/Gender
                  | /*/PatientInfo/Name/Suffix
                  | /*/ResultDate 
                  | /*/ResultTime
                  | /*/Test/Comment
                  | /*/Test/Component/Comment
                  | /*/Test/Component/TestCode
                  | /*/Test/Component/Status
                  | /*/Test/OrderCode
                  | /*/Test/OrderNumber
                  | /*/Test/OrderStatus
                  | (/*/Test/CollectionTime | /*/Test/ResultTime)[text()='00:00']";

        static readonly CultureInfo _CULTURE = CultureInfo.CurrentCulture;

        #endregion

        #region Fields

        static CancellationToken _noCancel = new CancellationToken(false);
        static CancellationToken _cancel = new CancellationToken(true);

        /// <summary>
        /// Manages test FAM DBs
        /// </summary>
        static FAMTestDBManager<TestAccuracyServices> _testDbManager;

        /// <summary>
        /// List of the expected contents of the ReportingDataCaptureAccuracy table after the first run
        /// </summary>
        static DataEntryAccuracyList _FIRST_RUN_EXPECTED_RESULTS = new DataEntryAccuracyList
        {

            (2, 4, "PatientInfo/DOB",                            1, 1, 0,  1, Parse("1/3/2018 10:40:03 AM", _CULTURE), 3, 1, Parse("1/3/2018 10:44:24 AM", _CULTURE), 3, 4),
            (2, 4, "PatientInfo/Name/First",                     1, 1, 0,  1, Parse("1/3/2018 10:40:03 AM", _CULTURE), 3, 1, Parse("1/3/2018 10:44:24 AM", _CULTURE), 3, 4),
            (2, 4, "PatientInfo/Name/Last",                      1, 1, 0,  1, Parse("1/3/2018 10:40:03 AM", _CULTURE), 3, 1, Parse("1/3/2018 10:44:24 AM", _CULTURE), 3, 4),
            (2, 4, "PhysicianInfo/OrderingPhysicianName/Code",   0, 1, 0,  1, Parse("1/3/2018 10:40:03 AM", _CULTURE), 3, 1, Parse("1/3/2018 10:44:24 AM", _CULTURE), 3, 4),
            (2, 4, "PhysicianInfo/OrderingPhysicianName/First",  1, 1, 0,  1, Parse("1/3/2018 10:40:03 AM", _CULTURE), 3, 1, Parse("1/3/2018 10:44:24 AM", _CULTURE), 3, 4),
            (2, 4, "PhysicianInfo/OrderingPhysicianName/Last",   1, 1, 0,  1, Parse("1/3/2018 10:40:03 AM", _CULTURE), 3, 1, Parse("1/3/2018 10:44:24 AM", _CULTURE), 3, 4),
            (2, 4, "PhysicianInfo/OrderingPhysicianName/Middle", 1, 1, 0,  1, Parse("1/3/2018 10:40:03 AM", _CULTURE), 3, 1, Parse("1/3/2018 10:44:24 AM", _CULTURE), 3, 4),
            (2, 4, "Test/CollectionDate",                        1, 1, 0,  1, Parse("1/3/2018 10:40:03 AM", _CULTURE), 3, 1, Parse("1/3/2018 10:44:24 AM", _CULTURE), 3, 4),
            (2, 4, "Test/CollectionTime",                        0, 1, 0,  1, Parse("1/3/2018 10:40:03 AM", _CULTURE), 3, 1, Parse("1/3/2018 10:44:24 AM", _CULTURE), 3, 4),
            (2, 4, "Test/Component",                             3, 3, 0,  1, Parse("1/3/2018 10:40:03 AM", _CULTURE), 3, 1, Parse("1/3/2018 10:44:24 AM", _CULTURE), 3, 4),
            (2, 4, "Test/Component/Range",                       1, 1, 0,  1, Parse("1/3/2018 10:40:03 AM", _CULTURE), 3, 1, Parse("1/3/2018 10:44:24 AM", _CULTURE), 3, 4),
            (2, 4, "Test/Component/Units",                       3, 3, 0,  1, Parse("1/3/2018 10:40:03 AM", _CULTURE), 3, 1, Parse("1/3/2018 10:44:24 AM", _CULTURE), 3, 4),
            (2, 4, "Test/Component/Value",                       3, 3, 0,  1, Parse("1/3/2018 10:40:03 AM", _CULTURE), 3, 1, Parse("1/3/2018 10:44:24 AM", _CULTURE), 3, 4),
            (2, 4, "Test/Name",                                  1, 1, 0,  1, Parse("1/3/2018 10:40:03 AM", _CULTURE), 3, 1, Parse("1/3/2018 10:44:24 AM", _CULTURE), 3, 4)
        };

        /// <summary>
        /// List of the expected contents of the ReportingDataCaptureAccuracy table after a rerun of the file from the first run
        /// </summary>
        static DataEntryAccuracyList _RERUN_FILE_EXPECTED_RESULTS = new DataEntryAccuracyList
        {
            (2, 5, "PatientInfo/DOB",                            1, 1, 0, 1, Parse("1/3/2018 10:40:03 AM", _CULTURE), 3, 1, Parse("1/6/2018 10:49:09 AM", _CULTURE), 3, 4),
            (2, 5, "PatientInfo/Name/First",                     1, 1, 0, 1, Parse("1/3/2018 10:40:03 AM", _CULTURE), 3, 1, Parse("1/6/2018 10:49:09 AM", _CULTURE), 3, 4),
            (2, 5, "PatientInfo/Name/Last",                      1, 1, 0, 1, Parse("1/3/2018 10:40:03 AM", _CULTURE), 3, 1, Parse("1/6/2018 10:49:09 AM", _CULTURE), 3, 4),
            (2, 5, "PhysicianInfo/OrderingPhysicianName/Code",   0, 1, 0, 1, Parse("1/3/2018 10:40:03 AM", _CULTURE), 3, 1, Parse("1/6/2018 10:49:09 AM", _CULTURE), 3, 4),
            (2, 5, "PhysicianInfo/OrderingPhysicianName/First",  1, 1, 0, 1, Parse("1/3/2018 10:40:03 AM", _CULTURE), 3, 1, Parse("1/6/2018 10:49:09 AM", _CULTURE), 3, 4),
            (2, 5, "PhysicianInfo/OrderingPhysicianName/Last",   1, 1, 0, 1, Parse("1/3/2018 10:40:03 AM", _CULTURE), 3, 1, Parse("1/6/2018 10:49:09 AM", _CULTURE), 3, 4),
            (2, 5, "PhysicianInfo/OrderingPhysicianName/Middle", 1, 1, 0, 1, Parse("1/3/2018 10:40:03 AM", _CULTURE), 3, 1, Parse("1/6/2018 10:49:09 AM", _CULTURE), 3, 4),
            (2, 5, "Test/CollectionDate",                        1, 1, 0, 1, Parse("1/3/2018 10:40:03 AM", _CULTURE), 3, 1, Parse("1/6/2018 10:49:09 AM", _CULTURE), 3, 4),
            (2, 5, "Test/CollectionTime",                        0, 1, 0, 1, Parse("1/3/2018 10:40:03 AM", _CULTURE), 3, 1, Parse("1/6/2018 10:49:09 AM", _CULTURE), 3, 4),
            (2, 5, "Test/Component",                             1, 1, 2, 1, Parse("1/3/2018 10:40:03 AM", _CULTURE), 3, 1, Parse("1/6/2018 10:49:09 AM", _CULTURE), 3, 4),
            (2, 5, "Test/Component/Range",                       1, 1, 0, 1, Parse("1/3/2018 10:40:03 AM", _CULTURE), 3, 1, Parse("1/6/2018 10:49:09 AM", _CULTURE), 3, 4),
            (2, 5, "Test/Component/Units",                       1, 1, 2, 1, Parse("1/3/2018 10:40:03 AM", _CULTURE), 3, 1, Parse("1/6/2018 10:49:09 AM", _CULTURE), 3, 4),
            (2, 5, "Test/Component/Value",                       0, 1, 3, 1, Parse("1/3/2018 10:40:03 AM", _CULTURE), 3, 1, Parse("1/6/2018 10:49:09 AM", _CULTURE), 3, 4),
            (2, 5, "Test/Name",                                  1, 1, 0, 1, Parse("1/3/2018 10:40:03 AM", _CULTURE), 3, 1, Parse("1/6/2018 10:49:09 AM", _CULTURE), 3, 4)
        }
        ;
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
            var accuracy = new DataCaptureAccuracy();
            Assert.Throws<ExtractException>(() => accuracy.Process(_noCancel), "Process should throw an exception");
        }

        /// <summary>
        /// Test the use of the IDatabase.Process command for DataCaptureAccuracy Service
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("ETL")]
        public static void TestDataCaptureAccuracyServiceProcessNoExistingData()
        {
            string testDBName = "Test_DataCaptureAccuracyService";
            try
            {
                // This is only used to initialize the database used for calculating the stats
                var fileProcessingDb = _testDbManager.GetDatabase(_DATABASE, testDBName);

                // Create DataCaptureAccuracy object using the initialized database
                DataCaptureAccuracy dataCaptureAccuracy = CreateTestDataCapture(fileProcessingDb.DatabaseServer, fileProcessingDb.DatabaseName);


                using var connection = new ExtractRoleConnection(dataCaptureAccuracy.DatabaseServer, dataCaptureAccuracy.DatabaseName);
                connection.Open();
                using var cmd = connection.CreateCommand();

                // no records should be produced by this
                Assert.Throws<ExtractException>(() => dataCaptureAccuracy.Process(_cancel));
                cmd.CommandText = "SELECT COUNT(ID) FROM ReportingDataCaptureAccuracy";

                Assert.AreEqual(cmd.ExecuteScalar() as Int32?, 0);

                // Process using the settings
                dataCaptureAccuracy.Process(_noCancel);

                // Get the data from the database after processing
                cmd.CommandText = string.Format(CultureInfo.InvariantCulture, @"
                        SELECT * FROM ReportingDataCaptureAccuracy WHERE DatabaseServiceID = {0}"
                    , dataCaptureAccuracy.DatabaseServiceID);

                CheckResults(cmd.ExecuteReader(), _FIRST_RUN_EXPECTED_RESULTS);

                // Run again - There should be no changes
                dataCaptureAccuracy.Process(_noCancel);

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
        public static void TestDataCaptureAccuracyServiceProcessExistingData()
        {
            string testDBName = "Test_DataCaptureAccuracyService";
            try
            {
                // This is only used to initialize the database used for calculating the stats
                var fileProcessingDb = _testDbManager.GetDatabase(_RERUN_DATABASE, testDBName);

                // Create DataCaptureAccuracy object using the initialized database
                DataCaptureAccuracy dataCaptureAccuracy = CreateTestDataCapture(fileProcessingDb.DatabaseServer, fileProcessingDb.DatabaseName);

                using var connection = new ExtractRoleConnection(dataCaptureAccuracy.DatabaseServer, dataCaptureAccuracy.DatabaseName);
                connection.Open();
                using var cmd = connection.CreateCommand();

                // Process using the settings
                dataCaptureAccuracy.Process(_noCancel);

                // Get the data from the database after processing
                cmd.CommandText = string.Format(CultureInfo.InvariantCulture, @"
                    SELECT * FROM ReportingDataCaptureAccuracy WHERE DatabaseServiceID = {0}"
                    , dataCaptureAccuracy.DatabaseServiceID);

                CheckResults(cmd.ExecuteReader(), _RERUN_FILE_EXPECTED_RESULTS);

                // Run again - There should be no changes
                dataCaptureAccuracy.Process(_noCancel);

                CheckResults(cmd.ExecuteReader(), _RERUN_FILE_EXPECTED_RESULTS);
            }
            finally
            {
                _testDbManager.RemoveDatabase(testDBName);
            }
        }

        /// <summary>
        /// Tests that the status is updated correctly
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("ETL")]
        public static void TestDataCaptureAccuracyStatus()
        {
            string testDBName = "Test_DataCaptureAccuracyServiceStatus";
            try
            {
                // This is only used to initialize the database used for calculating the stats
                var fileProcessingDb = _testDbManager.GetDatabase(_RERUN_DATABASE, testDBName);

                // Create DataCaptureAccuracy object using the initialized database
                DataCaptureAccuracy dataCaptureAccuracy = CreateTestDataCapture(fileProcessingDb.DatabaseServer, fileProcessingDb.DatabaseName);

                dataCaptureAccuracy.RefreshStatus();
                var status = dataCaptureAccuracy.Status as DataCaptureAccuracy.DataCaptureAccuracyStatus;

                Assert.AreEqual(0, status.LastFileTaskSessionIDProcessed, "LastFileTaskSessionIDProcessed should be 0");
                Assert.AreEqual(status.LastFileTaskSessionIDProcessed, dataCaptureAccuracy.LastFileTaskSessionIDProcessed,
                    "LastFileTaskSessionIDProcessed for status and data capture accuracy object should be equal");

                // Process using the settings
                dataCaptureAccuracy.Process(_noCancel);

                dataCaptureAccuracy.RefreshStatus();
                status = dataCaptureAccuracy.Status as DataCaptureAccuracy.DataCaptureAccuracyStatus;

                Assert.AreEqual(9, status.LastFileTaskSessionIDProcessed, "LastFileTaskSessionIDProcessed should be 9");
                Assert.AreEqual(status.LastFileTaskSessionIDProcessed, dataCaptureAccuracy.LastFileTaskSessionIDProcessed,
                    "LastFileTaskSessionIDProcessed for status and data capture accuracy object should be equal");

                // Run again - There should be no changes
                dataCaptureAccuracy.Process(_noCancel);

                dataCaptureAccuracy.RefreshStatus();
                status = dataCaptureAccuracy.Status as DataCaptureAccuracy.DataCaptureAccuracyStatus;

                Assert.AreEqual(9, status.LastFileTaskSessionIDProcessed, "LastFileTaskSessionIDProcessed should be 9");
                Assert.AreEqual(status.LastFileTaskSessionIDProcessed, dataCaptureAccuracy.LastFileTaskSessionIDProcessed,
                    "LastFileTaskSessionIDProcessed for status and data capture accuracy object should be equal");
                
                using var connection = new ExtractRoleConnection(dataCaptureAccuracy.DatabaseServer, dataCaptureAccuracy.DatabaseName);
                connection.Open();
                using (var statusCmd = connection.CreateCommand())
                {
                    statusCmd.CommandText = "SELECT Status, LastFileTaskSessionIDProcessed FROM DatabaseService WHERE ID = @DatabaseServiceID ";
                    statusCmd.Parameters.AddWithValue("@DatabaseServiceID", dataCaptureAccuracy.DatabaseServiceID);
                    var result = statusCmd.ExecuteReader().Cast<IDataRecord>().SingleOrDefault();
                    Assert.AreNotEqual(null, result, "A single record should be returned");
                    Assert.AreNotEqual(DBNull.Value, result["Status"], "Status should not be null");
                    Assert.AreNotEqual(DBNull.Value, result["LastFileTaskSessionIDProcessed"], "LastFileTaskSessionIDProcessed should not be null");
                }
                fileProcessingDb.Clear(true);

                using (var statusCmd = connection.CreateCommand())
                {
                    statusCmd.CommandText = "SELECT Status, LastFileTaskSessionIDProcessed FROM DatabaseService WHERE ID = @DatabaseServiceID ";
                    statusCmd.Parameters.AddWithValue("@DatabaseServiceID", dataCaptureAccuracy.DatabaseServiceID);
                    var result = statusCmd.ExecuteReader().Cast<IDataRecord>().SingleOrDefault();
                    Assert.AreNotEqual(null, result, "A single record should be returned");
                    Assert.AreEqual(DBNull.Value, result["Status"], "Status should not be null");
                    Assert.AreEqual(DBNull.Value, result["LastFileTaskSessionIDProcessed"], "LastFileTaskSessionIDProcessed should not be null");
                }
                connection.Close();

                dataCaptureAccuracy.RefreshStatus();
                status = dataCaptureAccuracy.Status as DataCaptureAccuracy.DataCaptureAccuracyStatus;

                Assert.AreEqual(0, status.LastFileTaskSessionIDProcessed, "LastFileTaskSessionIDProcessed should be 0");
                Assert.AreEqual(status.LastFileTaskSessionIDProcessed, dataCaptureAccuracy.LastFileTaskSessionIDProcessed,
                    "LastFileTaskSessionIDProcessed for status and data capture accuracy object should be equal");
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
        public static void TestDataCaptureAccuracyGetSettingsAndLoad()
        {
            DataCaptureAccuracy dataCaptureAccuracy = new DataCaptureAccuracy();
            dataCaptureAccuracy.DatabaseName = "Database";
            dataCaptureAccuracy.DatabaseServer = "Server";
            dataCaptureAccuracy.FoundAttributeSetName = "DataFoundByRules";
            dataCaptureAccuracy.ExpectedAttributeSetName = "DataSavedByOperator";
            dataCaptureAccuracy.XPathOfAttributesToIgnore = "XPath to ignore test";
            dataCaptureAccuracy.XPathOfContainerOnlyAttributes = "XPath of Container Only test";
            dataCaptureAccuracy.Description = "Test Description";

            string settings = dataCaptureAccuracy.ToJson();

            var newDCAFromSettings = DatabaseService.FromJson(settings);

            Assert.IsTrue(string.IsNullOrEmpty(newDCAFromSettings.DatabaseName));
            Assert.IsTrue(string.IsNullOrEmpty(newDCAFromSettings.DatabaseServer));
            Assert.AreEqual(dataCaptureAccuracy.Description, newDCAFromSettings.Description);

            DataCaptureAccuracy dca = newDCAFromSettings as DataCaptureAccuracy;

            Assert.IsNotNull(dca);

            Assert.AreEqual(dataCaptureAccuracy.FoundAttributeSetName, dca.FoundAttributeSetName);
            Assert.AreEqual(dataCaptureAccuracy.ExpectedAttributeSetName, dca.ExpectedAttributeSetName);
            Assert.AreEqual(dataCaptureAccuracy.XPathOfAttributesToIgnore, dca.XPathOfAttributesToIgnore);
            Assert.AreEqual(dataCaptureAccuracy.XPathOfContainerOnlyAttributes, dca.XPathOfContainerOnlyAttributes);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Compares what is in the foundResults with expected
        /// </summary>
        /// <param name="foundResults">The SqlDataReader for the results generated</param>
        /// <param name="expected">The Expected results</param>
        static void CheckResults(SqlDataReader foundResults, DataEntryAccuracyList expected)
        {
            // Convert reader to IEnummerable 
            var results = foundResults.Cast<IDataRecord>();

            // Compare the found results to the expected
            var formatedResults = results.Select(r => (
                   FoundAttributeSetForFileID: r.GetInt64(r.GetOrdinal("FoundAttributeSetForFileID")),
                   ExpectedAttributeSetForFileID: r.GetInt64(r.GetOrdinal("ExpectedAttributeSetForFileID")),
                   Attribute: r.GetString(r.GetOrdinal("Attribute")),
                   Correct: r.GetInt64(r.GetOrdinal("Correct")),
                   Expected: r.GetInt64(r.GetOrdinal("Expected")),
                   Incorrect: r.GetInt64(r.GetOrdinal("Incorrect")),
                   FileID: r.GetInt32(r.GetOrdinal("FileID")),
                   FoundDateTimeStamp: r.GetDateTime(r.GetOrdinal("FoundDateTimeStamp")),
                   FoundFAMUserID: r.GetInt32(r.GetOrdinal("FoundFAMUserID")),
                   FoundActionID: r.GetInt32(r.GetOrdinal("FoundActionID")),
                   ExpectedDateTimeStamp: r.GetDateTime(r.GetOrdinal("ExpectedDateTimeStamp")),
                   ExpectedFAMUserID: r.GetInt32(r.GetOrdinal("ExpectedFAMUserID")),
                   ExpectedActionID: r.GetInt32(r.GetOrdinal("ExpectedActionID")))).ToList();

            Assert.That(formatedResults.Count() == expected.Count,
                string.Format(CultureInfo.InvariantCulture, "Found {0} and expected {1} ",
                formatedResults.Count(), expected.Count));

            Assert.That(formatedResults
                    .OrderBy(a => a.Attribute)
                    .ThenBy(a => a.FileID)
                .SequenceEqual(expected
                    .OrderBy(r => r.Attribute)
                    .ThenBy(r => r.FileID)),
                "Compare the actual data with the expected");

            foundResults.Close();
        }

        /// <summary>
        /// Creates a DataCaptureAccuracy service object for use in testing, the settings are set to the DatabaseServiceID = 1
        /// and the database gets updated to contain the objects settings in the DatabaseService table
        /// </summary>
        /// <param name="databaseServer">Database server to use</param>
        /// <param name="databaseName">Database name to use</param>
        /// <returns>A new <see cref="DataCaptureAccuracy"/> object that is associated with DatabaseServiceID 1 in the database</returns>
        static DataCaptureAccuracy CreateTestDataCapture(string databaseServer, string databaseName)
        {
            DataCaptureAccuracy accuracy = new DataCaptureAccuracy
            {
                DatabaseServiceID = 1,
                DatabaseName = databaseName,
                DatabaseServer = databaseServer,
                FoundAttributeSetName = "DataFoundByRules",
                ExpectedAttributeSetName = "DataSavedByOperator",
                XPathOfAttributesToIgnore = _XPATH_IGNORE,
                XPathOfContainerOnlyAttributes = _XPATH_CONTAINERS
            };
            accuracy.UpdateDatabaseServiceSettings();
            return accuracy;
        }

        #endregion
    }
}
