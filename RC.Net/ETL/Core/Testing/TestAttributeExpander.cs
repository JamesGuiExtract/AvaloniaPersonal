using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Extract.FileActionManager.Database.Test;
using Extract.Testing.Utilities;
using NUnit.Framework;
using Extract.Database;
using System.Security.AccessControl;
using System.IO;

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

        static readonly string _ExpectedSaveAll = "Select * FROM TestExpandAttributesResults";
        static readonly string _ExpectedNoRasterZones = @"
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

        /// <summary>
        /// Manages test FAM DBs
        /// </summary>
        static FAMTestDBManager<TestAttributeExpander> _testDbManager;

        static TestFileManager<TestAttributeExpander> _testFileManager;

        /// <summary>
        /// List containing the names of the RasterZone fields
        /// </summary>
        static List<string> RasterZoneFields = new List<string>()
        {
              "Top"
              ,"Left"
              ,"Bottom"
              ,"Right"
              ,"StartX"
              ,"StartY"
              ,"EndX"
              ,"EndY"
              ,"PageNumber"
              ,"Height"        
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
        }

        #endregion Overhead

        #region Unit Tests

        /// <summary>
        /// Test Expand Attributes with StoreSpatialInfo=true and StoreEmptyAttributes=false
        /// </summary>
        [Test, Category("Automated")]
        public void TestExpandAttributes()
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
                expandAttributes.StoreSpatialInfo = true;
                expandAttributes.StoreEmptyAttributes = false;

                processTest(resultDBName, expandAttributes, _ExpectedSaveAll + " WHERE [Name] != 'Empty'");
            }
            finally
            {
                _testDbManager.RemoveDatabase(testDBName);
                removeExpectedResultsDB(resultDBName);
            }
        }

        /// <summary>
        /// Test Expand Attributes with StoreSpatialInfo=false and StoreEmptyAttributes=false
        /// </summary>
        [Test, Category("Automated")]
        public void TestExpandAttributesNoSpatial()
        {
            string testDBName = "ExpandAttributeNoSpatial_Test";
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
                expandAttributes.StoreSpatialInfo = false;
                expandAttributes.StoreEmptyAttributes = false;

                processTest(resultDBName, expandAttributes, _ExpectedNoRasterZones + " WHERE [Name] != 'Empty'");
            }
            finally
            {
                _testDbManager.RemoveDatabase(testDBName);
                removeExpectedResultsDB(resultDBName);
            }
        }

        /// <summary>
        /// Test Expand Attributes with StoreSpatialInfo=true and StoreEmptyAttributes=true
        /// </summary>
        [Test, Category("Automated")]
        public void TestExpandAttributesWithEmpty()
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
                expandAttributes.StoreSpatialInfo = true;
                expandAttributes.StoreEmptyAttributes = true; ;

                processTest(resultDBName, expandAttributes, _ExpectedSaveAll);
            }
            finally
            {
                _testDbManager.RemoveDatabase(testDBName);
                removeExpectedResultsDB(resultDBName);
            }
        }

        /// <summary>
        /// Test Expand Attributes with StoreSpatialInfo=false and StoreEmptyAttributes=true
        /// </summary>
        [Test, Category("Automated")]
        public void TestExpandAttributesNoSpatialWithEmpty()
        {
            string testDBName = "ExpandAttributeNoSpatial_Test";
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
                expandAttributes.StoreSpatialInfo = false;
                expandAttributes.StoreEmptyAttributes = true;

                processTest(resultDBName, expandAttributes, _ExpectedNoRasterZones);
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
        [Test, Category("Automated")]
        public void TestExpandAttributesSerialization()
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

        #endregion

        #region Helper methods

        void processTest(string resultsDbName, ExpandAttributes expandAttributes, string expectedResultsSQL)
        {
            // Build the connection string from the settings
            SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder();
            sqlConnectionBuild.DataSource = expandAttributes.DatabaseServer;
            sqlConnectionBuild.InitialCatalog = expandAttributes.DatabaseName;

            sqlConnectionBuild.IntegratedSecurity = true;
            sqlConnectionBuild.NetworkLibrary = "dbmssocn";
            sqlConnectionBuild.MultipleActiveResultSets = true;

            using (var connection = new SqlConnection(sqlConnectionBuild.ConnectionString))
            {
                connection.Open();
                SqlCommand cmd = connection.CreateCommand();

                // Database should have one record in the DatabaseService table - not using the settings in the table
                cmd.CommandText = @"SELECT TOP (1) [ID] FROM [dbo].[DatabaseService]";

                expandAttributes.DatabaseServiceID = (int)cmd.ExecuteScalar();

                // Process using the settings
                expandAttributes.Process();

                // Get the results
                cmd.CommandText = _QUERY_ATTRIBUTE_RESULTS;
                var results = cmd.ExecuteReader().Cast<IDataRecord>().ToList();


                sqlConnectionBuild.InitialCatalog = resultsDbName;
                using (var expectedResultsConnection = new SqlConnection(sqlConnectionBuild.ConnectionString))
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

        bool isEqual(IDataRecord a, IDataRecord b)
        {
            if (a.FieldCount != b.FieldCount || a.FieldCount != 17)
            {
                return false;
            }

            bool returnValue = true;

            for (int i = 0; returnValue &&  i < a.FieldCount; i++)
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
                returnValue = returnValue && a[i].Equals( b[i]);
            }

            return returnValue;
        }

        void addExpectedResultsDB(string dbName)
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

        void removeExpectedResultsDB(string dbName)
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

        #endregion
    }
}
