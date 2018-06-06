﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading;
using Extract.FileActionManager.Database.Test;
using Extract.Testing.Utilities;
using NUnit.Framework;

namespace Extract.ETL.Test
{
    // Define RedactionAccuracyList as a list of tuples for the data in the ReportingRedactionAccuracy table
    using RedactionAccuracyList =
        List<(Int64 FoundAttributeSetForFileID,
            Int64 ExpectedAttributeSetForFileID,
            Int32 FileID,
            Int32 Page,
            string Attribute,
            Int64 Expected,
            Int64 Found,
            Int64 Correct,
            Int64 FalsePositives,
            Int64 OverRedacted,
            Int64 UnderRedacted,
            Int64 Missed,
            DateTime FoundDateTimeStamp,
            Int32 FoundFAMUserID,
            Int32 FoundActionID,
            DateTime ExpectedDateTimeStamp,
            Int32 ExpectedFAMUserID,
            Int32 ExpectedActionID)>;

    [Category("TestRedactionAccuracyServices")]
    [TestFixture]
    public class TestRedactionAccuracyServices
    {
        #region Constants

        static readonly string _DATABASE = "Resources.RedactionDemo_IDShield_first.bak";
        static readonly string _RERUN_DATABASE = "Resources.RedactionDemo_IDShield_second.bak";

 
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
            (1 ,  11,  1 ,  1,   "SSN", 2,   2,   2,   0,   0,   0,   0, DateTime.Parse("1/8/2018 3:08:57 PM"), 1, 1, DateTime.Parse("1/8/2018 3:17:46 PM"), 1, 3),
            (2 ,  12,  2 ,  1,   "SSN", 2,   3,   2,   0,   0,   0,   0, DateTime.Parse("1/8/2018 3:10:11 PM"), 1, 1, DateTime.Parse("1/8/2018 3:18:03 PM"), 1, 3),
            (3 ,  13,  3 ,  1,   "SSN", 2,   2,   1,   0,   0,   1,   1, DateTime.Parse("1/8/2018 3:11:28 PM"), 1, 1, DateTime.Parse("1/8/2018 3:18:49 PM"), 1, 3),
            (4 ,  14,  4 ,  1,   "SSN", 3,   2,   2,   0,   0,   0,   1, DateTime.Parse("1/8/2018 3:12:17 PM"), 1, 1, DateTime.Parse("1/8/2018 3:19:15 PM"), 1, 3),
            (5 ,  15,  5 ,  1,   "SSN", 1,   1,   1,   0,   0,   0,   0, DateTime.Parse("1/8/2018 3:12:56 PM"), 1, 1, DateTime.Parse("1/8/2018 3:19:50 PM"), 1, 3),
            (6 ,  16,  6 ,  1,   "SSN", 2,   2,   1,   0,   0,   1,   1, DateTime.Parse("1/8/2018 3:13:23 PM"), 1, 1, DateTime.Parse("1/8/2018 3:21:47 PM"), 1, 3),
            (7 ,  17,  7 ,  1,   "SSN", 1,   1,   1,   0,   0,   0,   0, DateTime.Parse("1/8/2018 3:13:40 PM"), 1, 1, DateTime.Parse("1/8/2018 3:21:50 PM"), 1, 3),
            (8 ,  18,  8 ,  1,   "SSN", 1,   1,   1,   0,   0,   0,   0, DateTime.Parse("1/8/2018 3:14:02 PM"), 1, 1, DateTime.Parse("1/8/2018 3:21:52 PM"), 1, 3),
            (9 ,  19,  9 ,  1,   "SSN", 2,   3,   1,   0,   0,   2,   1, DateTime.Parse("1/8/2018 3:14:55 PM"), 1, 1, DateTime.Parse("1/8/2018 3:22:02 PM"), 1, 3),
            (10,  20,  10,  1,   "SSN", 2,   2,   2,   0,   0,   0,   0, DateTime.Parse("1/8/2018 3:15:26 PM"), 1, 1, DateTime.Parse("1/8/2018 3:22:05 PM"), 1, 3)
        };                                                             

        /// <summary>
        /// List of the expected contents of the ReportingRedactionAccuracy table after a rerun of the file from the first run
        /// </summary>
        static RedactionAccuracyList _RERUN_FILE_EXPECTED_RESULTS = new RedactionAccuracyList
        {
            (1 ,  21,  1 ,  1,   "SSN", 2,   2,   2,   0,   0,   0,   0, DateTime.Parse("1/8/2018 3:08:57 PM"), 1, 1, DateTime.Parse("1/8/2018 3:57:08 PM"), 1, 3),
            (2 ,  22,  2 ,  1,   "SSN", 2,   3,   2,   0,   0,   0,   0, DateTime.Parse("1/8/2018 3:10:11 PM"), 1, 1, DateTime.Parse("1/8/2018 3:57:13 PM"), 1, 3),
            (3 ,  23,  3 ,  1,   "SSN", 2,   2,   1,   0,   0,   1,   1, DateTime.Parse("1/8/2018 3:11:28 PM"), 1, 1, DateTime.Parse("1/8/2018 3:57:21 PM"), 1, 3),
            (4 ,  24,  4 ,  1,   "SSN", 3,   2,   2,   0,   0,   0,   1, DateTime.Parse("1/8/2018 3:12:17 PM"), 1, 1, DateTime.Parse("1/8/2018 3:57:25 PM"), 1, 3),
            (5 ,  25,  5 ,  1,   "SSN", 1,   1,   1,   0,   0,   0,   0, DateTime.Parse("1/8/2018 3:12:56 PM"), 1, 1, DateTime.Parse("1/8/2018 3:57:27 PM"), 1, 3),
            (6 ,  26,  6 ,  1,   "SSN", 2,   2,   0,   0,   0,   2,   2, DateTime.Parse("1/8/2018 3:13:23 PM"), 1, 1, DateTime.Parse("1/8/2018 3:57:45 PM"), 1, 3),
            (7 ,  27,  7 ,  1,   "SSN", 1,   1,   1,   0,   0,   0,   0, DateTime.Parse("1/8/2018 3:13:40 PM"), 1, 1, DateTime.Parse("1/8/2018 3:57:48 PM"), 1, 3),
            (8 ,  28,  8 ,  1,   "SSN", 1,   1,   1,   0,   0,   0,   0, DateTime.Parse("1/8/2018 3:14:02 PM"), 1, 1, DateTime.Parse("1/8/2018 3:57:50 PM"), 1, 3),
            (9 ,  29,  9 ,  1,   "SSN", 2,   3,   1,   0,   0,   2,   1, DateTime.Parse("1/8/2018 3:14:55 PM"), 1, 1, DateTime.Parse("1/8/2018 3:57:54 PM"), 1, 3),
            (10,  30,  10,  1,   "SSN", 2,   2,   2,   0,   0,   0,   0, DateTime.Parse("1/8/2018 3:15:26 PM"), 1, 1, DateTime.Parse("1/8/2018 3:57:58 PM"), 1, 3)
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

            _testDbManager = new FAMTestDBManager<TestAccuracyServices>();
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
        }

        #endregion Overhead

        #region Unit Tests

        /// <summary>
        /// Test the use of the IDatabase.Process command for RedactionAccuracy Service
        /// </summary>
        [Test, Category("Automated")]
        public static void TestRedactionAccuracyServiceProcessNoExistingData()
        {
            string testDBName = "RedactionAccuracyService_Test";
            try
            {
                // This is only used to initialize the database used for calculating the stats
                var fileProcessingDb = _testDbManager.GetDatabase(_DATABASE, testDBName);

                // Create DataCaptureAccuracy object using the initialized database
                RedactionAccuracy redactionAccuracy = new RedactionAccuracy();
                redactionAccuracy.DatabaseName = fileProcessingDb.DatabaseName;
                redactionAccuracy.DatabaseServer = fileProcessingDb.DatabaseServer;

                redactionAccuracy.ExpectedAttributeSetName = "DataSavedByOperator";
                redactionAccuracy.FoundAttributeSetName = "DataFoundByRules";

                // Save the record in the DatabaseService table 
                // Build the connection string from the settings
                SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder();
                sqlConnectionBuild.DataSource = redactionAccuracy.DatabaseServer;
                sqlConnectionBuild.InitialCatalog = redactionAccuracy.DatabaseName;

                sqlConnectionBuild.IntegratedSecurity = true;
                sqlConnectionBuild.NetworkLibrary = "dbmssocn";
                sqlConnectionBuild.MultipleActiveResultSets = true;

                using (var connection = new SqlConnection(sqlConnectionBuild.ConnectionString))
                {
                    connection.Open();
                    SqlCommand cmd = connection.CreateCommand();

                    // Database should have one record in the DatabaseService table - not using the settings in the table
                    cmd.CommandText = @"SELECT TOP (1) [ID] FROM [dbo].[DatabaseService]";
        
                    redactionAccuracy.DatabaseServiceID = (int)cmd.ExecuteScalar();
                    //redactionAccuracy.XPathOfSensitiveAttributes = "";

                    // with the _cancel token there should be no results
                    redactionAccuracy.Process(_cancel);

                    cmd.CommandText = "Select Count(ID) from ReportingRedactionAccuracy";

                    Assert.AreEqual(cmd.ExecuteScalar() as Int32?, 0);

                    // Process using the settings
                    redactionAccuracy.Process(_noCancel);

                    // Get the data from the database after processing
                    cmd.CommandText = string.Format(CultureInfo.InvariantCulture, @"
                        SELECT * FROM ReportingRedactionAccuracy WHERE DatabaseServiceID = {0}"
                        , redactionAccuracy.DatabaseServiceID);

                    CheckResults(cmd.ExecuteReader(), _FIRST_RUN_EXPECTED_RESULTS);

                    // Run again - There should be no changes
                    redactionAccuracy.Process(_noCancel);

                    CheckResults(cmd.ExecuteReader(), _FIRST_RUN_EXPECTED_RESULTS);
                }
            }
            finally
            {
                _testDbManager.RemoveDatabase(testDBName);
            }
        }

        /// <summary>
        /// Test the use of the IDatabase.Process command for DataCaptureAccuracy Service with newer data that previous run
        /// </summary>
        [Test, Category("Automated")]
        public static void TestRedactionAccuracyServiceProcessExistingData()
        {
            string testDBName = "RedactionAccuracyService_Test";
            try
            {
                // This is only used to initialize the database used for calculating the stats
                var fileProcessingDb = _testDbManager.GetDatabase(_RERUN_DATABASE, testDBName);

                // Create DataCaptureAccuracy object using the initialized database
                RedactionAccuracy redactionAccuracy = new RedactionAccuracy();
                redactionAccuracy.DatabaseName = fileProcessingDb.DatabaseName;
                redactionAccuracy.DatabaseServer = fileProcessingDb.DatabaseServer;

                redactionAccuracy.FoundAttributeSetName = "DataFoundByRules";
                redactionAccuracy.ExpectedAttributeSetName = "DataSavedByOperator";

                // Build the connection string from the settings
                SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder();
                sqlConnectionBuild.DataSource = redactionAccuracy.DatabaseServer;
                sqlConnectionBuild.InitialCatalog = redactionAccuracy.DatabaseName;

                sqlConnectionBuild.IntegratedSecurity = true;
                sqlConnectionBuild.NetworkLibrary = "dbmssocn";
                sqlConnectionBuild.MultipleActiveResultSets = true;

                using (var connection = new SqlConnection(sqlConnectionBuild.ConnectionString))
                {
                    connection.Open();
                    SqlCommand cmd = connection.CreateCommand();

                    // Database should have one record in the DatabaseService table - not using the settings in the table
                    cmd.CommandText = @"SELECT TOP (1) [ID] FROM [dbo].[DatabaseService]";

                    redactionAccuracy.DatabaseServiceID = (int)cmd.ExecuteScalar();

                    // Process using the settings
                    redactionAccuracy.Process(_noCancel);

                    // Get the data from the database after processing
                    cmd.CommandText = string.Format(CultureInfo.InvariantCulture, @"
                        SELECT * FROM ReportingRedactionAccuracy WHERE DatabaseServiceID = {0}"
                        , redactionAccuracy.DatabaseServiceID);

                    CheckResults(cmd.ExecuteReader(), _RERUN_FILE_EXPECTED_RESULTS);

                    // Run again - There should be no changes
                    redactionAccuracy.Process(_noCancel);

                    CheckResults(cmd.ExecuteReader(), _RERUN_FILE_EXPECTED_RESULTS);
                }
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
        [Test, Category("Automated")]
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

            Assert.IsNullOrEmpty(newDCAFromSettings.DatabaseName);
            Assert.IsNullOrEmpty(newDCAFromSettings.DatabaseServer);
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
            var formatedResults = results.Select(r => (
                   FoundAttributeSetForFileID: r.GetInt64(r.GetOrdinal("FoundAttributeSetForFileID")),
                   ExpectedAttributeSetForFileID: r.GetInt64(r.GetOrdinal("ExpectedAttributeSetForFileID")),
                   FileID: r.GetInt32(r.GetOrdinal("FileID")),
                   Page: r.GetInt32(r.GetOrdinal("Page")),
                   Attribute: r.GetString(r.GetOrdinal("Attribute")),
                   Expected: r.GetInt64(r.GetOrdinal("Expected")),
                   Found: r.GetInt64(r.GetOrdinal("Found")),
                   Correct: r.GetInt64(r.GetOrdinal("Correct")),
                   FalsePositives: r.GetInt64(r.GetOrdinal("FalsePositives")),
                   OverRedacted: r.GetInt64(r.GetOrdinal("OverRedacted")),
                   UnderRedacted: r.GetInt64(r.GetOrdinal("UnderRedacted")),
                   Missed: r.GetInt64(r.GetOrdinal("Missed")),
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

        #endregion
    }
}
