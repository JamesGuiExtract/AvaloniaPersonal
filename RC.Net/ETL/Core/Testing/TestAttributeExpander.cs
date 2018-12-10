﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Threading;
using Extract.Database;
using Extract.FileActionManager.Database.Test;
using Extract.Testing.Utilities;
using NUnit.Framework;

namespace Extract.ETL.Test
{
    /// <summary>
    /// Class to test the AttributeExpander
    /// </summary>
    [Category("TestAttributeExpander")]
    [TestFixture]
    public class TestAttributeExpander
    {
        #region Constants

        const string _ExpectedSaveAll = "Select * FROM TestExpandAttributesResults";
        const string _ExpectedNoRasterZones = @"
           SELECT  [ID]
              ,[AttributeSetForFileID]
              ,[Name]
              ,[Type]
              ,[Value]
              ,[ParentAttributeID]
              ,[GUID]
              ,NULL AS [Top]
              ,NULL AS [Left]
              ,NULL AS [Bottom]
              ,NULL AS [Right]
              ,NULL AS [StartX]
              ,NULL AS [StartY]
              ,NULL AS [EndX]
              ,NULL AS [EndY]
              ,NULL AS [PageNumber]
              ,NULL AS [Height]
          FROM [TestExpandAttributesResults] ";

        static readonly string _DATABASE = "Resources.ExpandAttributes.bak";
        static readonly string _1000RASTER_ZONE = "Resources.Test1000.bak";

        static readonly string _RESULTS_DATABASE = "Resources.ExpandAttribute_ExpectedResults.bak";
        static readonly string _QUERY_ATTRIBUTE_RESULTS = @"
            SELECT [Attribute].[ID]
                  ,[AttributeSetForFileID]
                  ,[AttributeName].[Name]
	              ,[AttributeType].[Type]
                  ,[Value]
                  ,[ParentAttributeID]
                  ,[GUID]
                  ,[Top]
                  ,[Left]
                  ,[Bottom]
                  ,[Right]
                  ,[StartX]
                  ,[StartY]
                  ,[EndX]
                  ,[EndY]
                  ,[PageNumber]
                  ,[Height]
              FROM [Attribute]
              INNER JOIN [AttributeName] ON [Attribute].AttributeNameID = [AttributeName].[ID]
              INNER JOIN [AttributeInstanceType] ON [Attribute].[ID] = [AttributeInstanceType].[AttributeID]
              INNER JOIN [AttributeType] ON [AttributeType].[ID] = [AttributeInstanceType].[AttributeTypeID]
              LEFT JOIN [RasterZone] ON [Attribute].[ID] = [RasterZone].[AttributeID]";

        #endregion

        #region Fields

        static CancellationToken _noCancel = new CancellationToken(false);
        static CancellationToken _cancel = new CancellationToken(true);

        /// <summary>
        /// Manages test FAM DBs
        /// </summary>
        static FAMTestDBManager<TestAttributeExpander> _testDbManager;

        static TestFileManager<TestAttributeExpander> _testFileManager;

        #endregion

        #region Overhead

        /// <summary>
        /// Initializes the test fixture.
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testDbManager = new FAMTestDBManager<TestAttributeExpander>();
            _testFileManager = new TestFileManager<TestAttributeExpander>();

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

        #region Unit Tests

        /// <summary>
        /// Test Expand Attributes with StoreSpatialInfo=true and StoreEmptyAttributes=false
        /// </summary>
        [Test]
        [Category("Automated"), Category("ETL")]
        [TestCase(true, false, _ExpectedSaveAll + " WHERE [Name] != 'Empty'", TestName = "ExpandAttribute store spatial")]
        [TestCase(false, false, _ExpectedNoRasterZones + " WHERE [Name] != 'Empty'", TestName = "ExpandAttribute no spatial")]
        [TestCase(true, true, _ExpectedSaveAll, TestName = "ExpandAttribute spatial and empty")]
        [TestCase(false, true, _ExpectedNoRasterZones,TestName = "ExpandAttribute no spatial with empty")]
        public static void TestExpandAttributes(bool  storeSpatialInfo, bool storeEmptyAttributes, string expectedQuery)
        {
            string testDBName = "ExpandAttribute_Test";
            string resultDBName = "ExpandAttribute_Results_Test";
            try
            {
                // This is only used to initialize the database used for calculating the stats
                var fileProcessingDb = _testDbManager.GetDatabase(_DATABASE, testDBName);

                // add database that has the results
                addExpectedResultsDB(resultDBName);

                ExpandAttributes expandAttributes = new ExpandAttributes();
                expandAttributes.DatabaseName = fileProcessingDb.DatabaseName;
                expandAttributes.DatabaseServer = fileProcessingDb.DatabaseServer;
                expandAttributes.StoreSpatialInfo = storeSpatialInfo;
                expandAttributes.StoreEmptyAttributes = storeEmptyAttributes;

                processTest(resultDBName, expandAttributes, expectedQuery);
            }
            finally
            {
                _testDbManager.RemoveDatabase(testDBName);
                removeExpectedResultsDB(resultDBName);
            }
        }

        /// <summary>
        /// Tests the serialization of the ExpandAttribute object
        /// </summary>
        [Test]
        [Category("Automated"), Category("ETL")]
        public static void TestExpandAttributesSerialization()
        {
            ExpandAttributes expandAttributes = new ExpandAttributes();
            expandAttributes.DatabaseServer = "server";
            expandAttributes.DatabaseName = "DBName";
            expandAttributes.Description = "Description";
            expandAttributes.StoreEmptyAttributes = true;
            expandAttributes.StoreSpatialInfo = false;
            expandAttributes.DatabaseServiceID = 1;
            expandAttributes.Enabled = !expandAttributes.Enabled;

            string settings = expandAttributes.ToJson();

            ExpandAttributes test = (ExpandAttributes)ExpandAttributes.FromJson(settings);

            Assert.That(string.IsNullOrWhiteSpace(test.DatabaseServer), "DatabaseServer should not be saved.");
            Assert.That(string.IsNullOrWhiteSpace(test.DatabaseName), "DatabaseName should not be saved.");
            Assert.AreEqual(expandAttributes.Description, test.Description, "Description serialization worked.");
            Assert.AreEqual(expandAttributes.StoreEmptyAttributes, test.StoreEmptyAttributes, "StoreEmptyAttributes serialization worked.");
            Assert.AreEqual(expandAttributes.StoreSpatialInfo, test.StoreSpatialInfo, "StoreSpatialInfo serialization worked.");
            Assert.That(test.DatabaseServiceID == 0, "Default DatabaseServiceID is zero.");
            Assert.That(test.Enabled == !expandAttributes.Enabled);
        }

        [Test]
        [Category("Automated"), Category("ETL")]
        public static void TestStatus()
        {
            string testDBName = "TestStatus_Test";
            try
            {
                // This is only used to initialize the database used for calculating the stats
                var fileProcessingDb = _testDbManager.GetDatabase(_DATABASE, testDBName);


                ExpandAttributes expandAttributes = new ExpandAttributes();
                expandAttributes.DatabaseName = fileProcessingDb.DatabaseName;
                expandAttributes.DatabaseServer = fileProcessingDb.DatabaseServer;
                expandAttributes.StoreSpatialInfo = true;
                expandAttributes.StoreEmptyAttributes = false;

                expandAttributes.AddToDatabase("(local)", testDBName);
                var status = expandAttributes.Status as ExpandAttributes.ExpandAttributesStatus;
                string jsonStatus = status.ToJson();

                expandAttributes.RefreshStatus();

                Assert.AreEqual(jsonStatus, expandAttributes.Status.ToJson(), "Refreshed status should be the same as previous status");

                expandAttributes.DashboardAttributes.Add(new ExpandAttributes.DashboardAttributeField()
                {
                    DashboardAttributeName = "DocumentType",
                    AttributeSetNameID = 1, // DataFoundByRules 
                    PathForAttributeInAttributeSet = "DocumentType"
                });

                expandAttributes.UpdateDatabaseServiceSettings();

                expandAttributes.Process(_noCancel);

                status = expandAttributes.Status as ExpandAttributes.ExpandAttributesStatus;
                Assert.AreEqual(2, status.LastFileTaskSessionIDProcessed, "All File task sessions should be processed");

                var testValue = new ExpandAttributes.DashboardAttributeField()
                {
                    DashboardAttributeName = "Test",
                    AttributeSetNameID = 1, // DataFoundByRules 
                    PathForAttributeInAttributeSet = "Test"
                };

                expandAttributes.DashboardAttributes.Add(testValue);

                expandAttributes.UpdateDatabaseServiceSettings();

                expandAttributes.RefreshStatus();
                expandAttributes.SaveStatus(expandAttributes.Status);

                status = expandAttributes.Status as ExpandAttributes.ExpandAttributesStatus;

                Assert.AreEqual(-1, status.LastIDProcessedForDashboardAttribute[testValue.ToString()], "Status for new attribute should be -1");

                var itemToRemove = expandAttributes.DashboardAttributes.Single(f => f.DashboardAttributeName == "Test");

                expandAttributes.DashboardAttributes.Remove(itemToRemove);

                expandAttributes.UpdateDatabaseServiceSettings();

                expandAttributes.Process(_noCancel);

                Assert.AreEqual(1, expandAttributes.DashboardAttributes.Count, "Number of DashboardAttributes should be 1");
                status = expandAttributes.Status as ExpandAttributes.ExpandAttributesStatus;
                Assert.AreEqual(1, status.LastIDProcessedForDashboardAttribute.Count, "Number of LastIDProcessedForDashboardAttribute items should be 1");
            }
            finally
            {
                _testDbManager.RemoveDatabase(testDBName);
            }
        }

        [Test]
        [Category("Automated"), Category("ETL")]
        public static void Test1000RasterZones()
        {
            string testDBName = "Test1000RasterZones_Test";
            try
            {
                // This is only used to initialize the database used for calculating the stats
                var fileProcessingDb = _testDbManager.GetDatabase(_1000RASTER_ZONE, testDBName);

                ExpandAttributes expandAttributes = new ExpandAttributes();
                expandAttributes.DatabaseName = fileProcessingDb.DatabaseName;
                expandAttributes.DatabaseServer = fileProcessingDb.DatabaseServer;
                expandAttributes.StoreSpatialInfo = true;
                expandAttributes.StoreEmptyAttributes = false;

                expandAttributes.AddToDatabase("(local)", testDBName);

                Assert.DoesNotThrow(()=>expandAttributes.Process(_noCancel), "Test that 1000 raster zone spatialString expands");

                // There should 1320 RasterZones added
                using (var connection = NewSqlConnection(expandAttributes.DatabaseServer, expandAttributes.DatabaseName))
                {
                    connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT COUNT(ID) NumberOfRasterZones FROM RasterZone";
                        var result = cmd.ExecuteScalar();
                        Assert.AreEqual(1320, (int)result, "There should be 1320 raster zones added");
                    }
                }
            }
            finally
            {
                _testDbManager.RemoveDatabase(testDBName);
            }
        }

        #endregion

        #region Helper methods

        static void processTest(string resultsDbName, ExpandAttributes expandAttributes, string expectedResultsSQL)
        {
            using (var connection = NewSqlConnection(expandAttributes.DatabaseServer, expandAttributes.DatabaseName))
            {
                connection.Open();
                SqlCommand cmd = connection.CreateCommand();

                // Database should have one record in the DatabaseService table - not using the settings in the table
                cmd.CommandText = @"SELECT TOP (1) [ID] FROM [dbo].[DatabaseService]";

                expandAttributes.DatabaseServiceID = (int)cmd.ExecuteScalar();
                expandAttributes.UpdateDatabaseServiceSettings();

                Assert.Throws<ExtractException>(() => expandAttributes.Process(_cancel));
                cmd.CommandText = "SELECT COUNT(ID) FROM Attribute ";
                Assert.AreEqual(cmd.ExecuteScalar() as Int32?, 0);

                // Process using the settings
                expandAttributes.Process(_noCancel);

                // Get the results
                cmd.CommandText = _QUERY_ATTRIBUTE_RESULTS;
                var results = cmd.ExecuteReader().Cast<IDataRecord>().ToList();

                using (var expectedResultsConnection = NewSqlConnection(expandAttributes.DatabaseServer, resultsDbName))
                {
                    expectedResultsConnection.Open();
                    var expectedCmd = expectedResultsConnection.CreateCommand();
                    expectedCmd.CommandText = expectedResultsSQL;

                    var expected = expectedCmd.ExecuteReader().Cast<IDataRecord>().ToList();

                    bool matches = results.Count == expected.Count;
                    for (int i = 0; matches && i < results.Count && i < expected.Count; i++)
                    {
                        matches = matches && isEqual(results[i], expected[i]);
                    }

                    Assert.That(matches, "Verify that the results match expected");
                }
            }
        }

        static bool isEqual(IDataRecord a, IDataRecord b)
        {
            if (a.FieldCount != b.FieldCount || a.FieldCount != 17)
            {
                return false;
            }

            bool returnValue = true;

            for (int i = 0; returnValue && i < a.FieldCount; i++)
            {
                if (a.IsDBNull(i))
                {
                    returnValue = returnValue && b.IsDBNull(i);
                    continue;
                }
                if (b.IsDBNull(i))
                {
                    returnValue = returnValue && a.IsDBNull(i);
                    continue;
                }
                returnValue = returnValue && a[i].Equals(b[i]);
            }

            return returnValue;
        }

        static void addExpectedResultsDB(string dbName)
        {
            // Add the database with expected results
            var expectedBackupDB = _testFileManager.GetFile(_RESULTS_DATABASE);

            // In most cases SQL server will not have access to the file; giving access to
            // all users will allow it access.
            FileSecurity fSecurity = File.GetAccessControl(expectedBackupDB);
            fSecurity.AddAccessRule(new FileSystemAccessRule(
                @".\users", FileSystemRights.FullControl, AccessControlType.Allow));
            File.SetAccessControl(expectedBackupDB, fSecurity);

            DBMethods.RestoreDatabaseToLocalServer(expectedBackupDB, dbName);
            _testFileManager.RemoveFile(_RESULTS_DATABASE);

        }

        static void removeExpectedResultsDB(string dbName)
        {
            try
            {
                DBMethods.DropLocalDB(dbName);
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI45443");
            }
        }

        static SqlConnection NewSqlConnection(string databaseServer, string databaseName)
        {
            // Build the connection string from the settings
            SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder();
            sqlConnectionBuild.DataSource = databaseServer;
            sqlConnectionBuild.InitialCatalog = databaseName;

            sqlConnectionBuild.IntegratedSecurity = true;
            sqlConnectionBuild.NetworkLibrary = "dbmssocn";
            sqlConnectionBuild.MultipleActiveResultSets = true;
            return new SqlConnection(sqlConnectionBuild.ConnectionString);
        }

        #endregion
    }
}
