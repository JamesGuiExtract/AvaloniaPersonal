using Extract.SqlDatabase;
using Extract.Testing.Utilities;
using Extract.Utilities;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Database.Test
{
    public enum DatabaseType
    {
        OldSchemaWithRoles,
        OldSchemaNoRoles,
        CreateNewDatabase
    }

    [Category("Automated"), Category("FileProcessingDBSchemaUpdates")]
    [TestFixture]
    public class TestFAMDBSchemaUpdates
    {
        #region Constants

        static readonly string _DB_V194 = "Resources.DBVersion194.bak";
        static readonly string _DB_V201 = "Resources.DBVersion201.bak";
        static readonly string _DB_V205_17 = "Resources.DBVersion205_17.bak";
        static readonly string _DB_V207 = "Resources.DBVersion207.bak";

        #endregion

        #region Fields

        static TestFileManager<TestFAMDBSchemaUpdates> _testFiles;
        static FAMTestDBManager<TestFAMDBSchemaUpdates> _testDbManager;

        #endregion

        #region Overhead

        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testFiles = new();
            _testDbManager = new();
        }

        [OneTimeTearDown]
        public static void FinalCleanup()
        {
            _testFiles?.Dispose();
            _testDbManager?.Dispose();
        }

        #endregion Overhead

        #region Tests

        // Confirm that a new or upgraded version 202 database has the random queue feature
        [Test]
        public static void SchemaVersion202_VerifyGetFilesToProcess(
            [Values] bool upgradeFromPreviousSchema,
            [Values] bool useRandomQueue)
        {
            // Arrange
            int[] firstTenFilesAdded = Enumerable.Range(1, 10).ToArray();
            string dbName = UtilityMethods.FormatInvariant(
                $"Test_SchemaVersion202_{upgradeFromPreviousSchema}_{useRandomQueue}");

            // Act
            using var dbWrapper =
                upgradeFromPreviousSchema
                ? _testDbManager.GetDisposableDatabase(_DB_V201, dbName)
                : _testDbManager.GetDisposableDatabase(dbName);

            foreach (int i in Enumerable.Range(1, 100)) dbWrapper.AddFakeFile(i, false);
            IUnknownVector filesToProcess =
                useRandomQueue
                ? dbWrapper.FileProcessingDB.GetRandomFilesToProcess(dbWrapper.Actions[0], 10, false, "")
                : dbWrapper.FileProcessingDB.GetFilesToProcess(dbWrapper.Actions[0], 10, false, "");

            // Assert
            int[] fileIDsToProcess = filesToProcess
                .ToIEnumerable<IFileRecord>()
                .Select(fileRecord => fileRecord.FileID)
                .ToArray();

            // Make sure schema is at least 202
            Assert.That(dbWrapper.FileProcessingDB.DBSchemaVersion, Is.GreaterThanOrEqualTo(202));

            int expectedSchemaVersion = int.Parse(
                dbWrapper.FileProcessingDB.GetDBInfoSetting("ExpectedSchemaVersion", false)
                , CultureInfo.InvariantCulture);

            // Make sure schema has the correct version number
            Assert.AreEqual(expectedSchemaVersion, dbWrapper.FileProcessingDB.DBSchemaVersion);

            // The correct number of files are returned
            Assert.AreEqual(firstTenFilesAdded.Length, fileIDsToProcess.Length);

            if (useRandomQueue)
            {
                // The files returned are not the first files added to the queue
                CollectionAssert.AreNotEquivalent(firstTenFilesAdded, fileIDsToProcess);
            }
            else
            {
                // The first 10 files added are returned in queue order
                CollectionAssert.AreEqual(firstTenFilesAdded, fileIDsToProcess);
            }
        }

        [Test]
        public static void SchemaVersion203_VerifyApplicationRoles([Values] DatabaseType databaseType)
        {
            string dbName = UtilityMethods.FormatInvariant(
                $"Test_SchemaVersion203_{Enum.GetName(typeof(DatabaseType), databaseType)}");

            using var dbWrapper = databaseType switch
            {
                DatabaseType.OldSchemaWithRoles => _testDbManager.GetDisposableDatabase(_DB_V201, dbName),
                DatabaseType.OldSchemaNoRoles => _testDbManager.GetDisposableDatabase(_DB_V194, dbName),
                DatabaseType.CreateNewDatabase => _testDbManager.GetDisposableDatabase(dbName),
                _ => throw new NotImplementedException()
            };

            SqlAppRoleConnection extractRoleConnection = null;
            try
            {
                Assert.DoesNotThrow(() => extractRoleConnection = new ExtractRoleConnection("(local)", dbName, false)
                    , "Failed to create ExtractRoleConnection");
                Assert.DoesNotThrow(() => extractRoleConnection.Open(), "Failed to open ExtractRoleConnection");

                using var cmd = extractRoleConnection.CreateCommand();
                cmd.CommandText = "SELECT Count(name) FROM sys.database_principals p where type_desc = 'APPLICATION_ROLE' "
                    + $"AND name = '{extractRoleConnection.RoleName}'";
                var result = cmd.ExecuteScalar();
                Assert.AreEqual(1, (int)result, $"Application role '{extractRoleConnection.RoleName}' should exist and be usable");
            }
            finally
            {
                extractRoleConnection?.Dispose();
            }

            using var roleConnection = new NoAppRoleConnection("(local)", dbName);
            roleConnection.Open();

            SqlApplicationRoleTestUtils.CreateApplicationRole(
                roleConnection, "TestRole", "Test-Password2", SqlApplicationRoleTestUtils.AppRoleAccess.NoAccess);
            using (var sqlApplicationRole = new TestAppRoleConnection("(local)", dbName))
            {
                sqlApplicationRole.Role = "TestRole";
                sqlApplicationRole.Password = "Test-Password2";
                sqlApplicationRole.Open();

                using var selectDBInfoCmd = sqlApplicationRole.CreateCommand();
                selectDBInfoCmd.CommandText = "SELECT Count(*) FROM DBInfo";

                Assert.DoesNotThrow(() =>
                {
                    selectDBInfoCmd.ExecuteScalar();
                }, $"Select on DBInfo access should be available for all roles (public)");
            }

            int expectedSchemaVersion = int.Parse(
                dbWrapper.FileProcessingDB.GetDBInfoSetting("ExpectedSchemaVersion", false)
                , CultureInfo.InvariantCulture);

            // Make sure schema has the correct version number
            Assert.AreEqual(expectedSchemaVersion, dbWrapper.FileProcessingDB.DBSchemaVersion);
        }

        [Test]
        public static void SchemaVersion204_VerifyApplicationRoles([Values] DatabaseType databaseType)
        {
            string dbName = UtilityMethods.FormatInvariant(
                $"Test_SchemaVersion204_{Enum.GetName(typeof(DatabaseType), databaseType)}");

            using var dbWrapper = databaseType switch
            {
                DatabaseType.OldSchemaWithRoles => _testDbManager.GetDisposableDatabase(_DB_V201, dbName),
                DatabaseType.OldSchemaNoRoles => _testDbManager.GetDisposableDatabase(_DB_V194, dbName),
                DatabaseType.CreateNewDatabase => _testDbManager.GetDisposableDatabase(dbName),
                _ => throw new NotImplementedException()
            };

            SqlAppRoleConnection reportingRoleConnection = null;
            try
            {
                Assert.DoesNotThrow(() => reportingRoleConnection = new ExtractReportingRoleConnection("(local)", dbName, false)
                    , "Failed to create ExtractReportingRoleConnection");
                Assert.DoesNotThrow(() => reportingRoleConnection.Open(), "Failed to open ExtractReportingConnection");

                using var cmd = reportingRoleConnection.CreateCommand();
                cmd.CommandText = "SELECT Count(name) FROM sys.database_principals p where type_desc = 'APPLICATION_ROLE' "
                    + $"AND name = '{reportingRoleConnection.RoleName}'";
                var result = cmd.ExecuteScalar();
                Assert.AreEqual(1, (int)result, $"Application role '{reportingRoleConnection.RoleName}' should exist and be usable");

                cmd.CommandText = "SELECT Count(CSN) FROM LabDEEncounter";
                Assert.Throws<System.Data.SqlClient.SqlException>(() => cmd.ExecuteScalar()
                    , $"Application role '{reportingRoleConnection.RoleName}' should not be able to select LabDEEncounter");

                cmd.CommandText = "SELECT Count(ID) FROM TaskClass";
                result = cmd.ExecuteScalar();
                Assert.Greater((int)result, 1
                    , $"Application role '{reportingRoleConnection.RoleName}' should be able to select records from most tables");

                cmd.CommandText = "INSERT INTO FAMFile (FileName) VALUES ('Test')";
                Assert.Throws<System.Data.SqlClient.SqlException>(() => cmd.ExecuteScalar()
                    , $"Application role '{reportingRoleConnection.RoleName}' should not be able to add records");
            }
            finally
            {
                reportingRoleConnection?.Dispose();
            }

            int expectedSchemaVersion = int.Parse(
                dbWrapper.FileProcessingDB.GetDBInfoSetting("ExpectedSchemaVersion", false)
                , CultureInfo.InvariantCulture);

            // Make sure schema has the correct version number
            Assert.AreEqual(expectedSchemaVersion, dbWrapper.FileProcessingDB.DBSchemaVersion);
        }

        [Test]
        public static void SchemaVersion205_VerifyApplicationRoles([Values] DatabaseType databaseType)
        {
            string dbName = UtilityMethods.FormatInvariant(
                $"Test_SchemaVersion205_{Enum.GetName(typeof(DatabaseType), databaseType)}");

            using var dbWrapper = databaseType switch
            {
                DatabaseType.OldSchemaWithRoles => _testDbManager.GetDisposableDatabase(_DB_V201, dbName),
                DatabaseType.OldSchemaNoRoles => _testDbManager.GetDisposableDatabase(_DB_V194, dbName),
                DatabaseType.CreateNewDatabase => _testDbManager.GetDisposableDatabase(dbName),
                _ => throw new NotImplementedException()
            };

            SqlAppRoleConnection extractRoleConnection = null;
            try
            {
                Assert.DoesNotThrow(() => extractRoleConnection = new ExtractRoleConnection("(local)", dbName, false)
                    , "Failed to create ExtractRoleConnection");
                Assert.DoesNotThrow(() => extractRoleConnection.Open(), "Failed to open ExtractRoleConnection");

                using var cmd = extractRoleConnection.CreateCommand();
                cmd.CommandText = "SELECT Count(name) FROM sys.database_principals p where type_desc = 'APPLICATION_ROLE' "
                    + $"AND name = 'ExtractSecurityRole'";
                var result = cmd.ExecuteScalar();
                Assert.AreEqual(0, (int)result, $"Application role 'ExtractSecurityRole' should not exist");

                cmd.CommandText = $"ALTER APPLICATION ROLE '{extractRoleConnection.RoleName}' WITH PASSWORD = 'Check2SeeThisIsNotAllowed!'";
                Assert.Throws<System.Data.SqlClient.SqlException>(() => result = cmd.ExecuteScalar(),
                    $"Application role '{extractRoleConnection.RoleName}' should not have alter rights");
            }
            finally
            {
                extractRoleConnection?.Dispose();
            }

            int expectedSchemaVersion = int.Parse(
                dbWrapper.FileProcessingDB.GetDBInfoSetting("ExpectedSchemaVersion", false)
                , CultureInfo.InvariantCulture);

            // Make sure schema has the correct version number
            Assert.AreEqual(expectedSchemaVersion, dbWrapper.FileProcessingDB.DBSchemaVersion);
        }

        // Confirm that a new or upgraded version 205 database has LabDESchemaVersion = 18
        [Test]
        public static void SchemaVersion205_VerifyLabDESchema([Values] bool upgradeFromPreviousSchema)
        {
            // Arrange
            string dbName = UtilityMethods.FormatInvariant(
                $"Test_SchemaVersion205_VerifyLabDESchema_{upgradeFromPreviousSchema}");

            // Act
            var fileProcessingDB =
                upgradeFromPreviousSchema
                ? _testDbManager.GetDatabase(_DB_V205_17, dbName)
                : _testDbManager.GetNewDatabase(dbName);

            // Assert

            // Make sure LabDE schema version is at least 18
            Assert.That(int.Parse(fileProcessingDB.GetDBInfoSetting("LabDESchemaVersion", false), CultureInfo.InvariantCulture),
                Is.GreaterThanOrEqualTo(18));

            // Check for the new indexes
            using var connection = new ExtractRoleConnection(fileProcessingDB.DatabaseServer, fileProcessingDB.DatabaseName);
            connection.Open();
            Assert.That(IndexExists(connection, "dbo.LabDEEncounter", "IX_Encounter_EncounterDateTime"));
            Assert.That(IndexExists(connection, "dbo.LabDEOrder", "IX_Order_EncounterID"));
        }

        [Test]
        public static void SchemaVersion206_VerifyApplicationRoles([Values] bool upgradeFromPreviousSchema)
        {
            // Arrange
            string dbName = UtilityMethods.FormatInvariant($"Test_SchemaVersion206_{upgradeFromPreviousSchema}");

            // Act
            var fileProcessingDB =
                upgradeFromPreviousSchema
                ? _testDbManager.GetDatabase(_DB_V205_17, dbName)
                : _testDbManager.GetNewDatabase(dbName);

            // Assert

            // Make sure schema version is at least 206
            Assert.That(fileProcessingDB.DBSchemaVersion, Is.GreaterThanOrEqualTo(206));

            // Ensure the attribute table is accessible
            using var reportingRoleConnection = new ExtractReportingRoleConnection(fileProcessingDB.DatabaseServer, fileProcessingDB.DatabaseName);
            reportingRoleConnection.Open();
            using var cmd = reportingRoleConnection.CreateCommand();
            cmd.CommandText = "SELECT Count(ID) FROM Attribute";
            cmd.ExecuteScalar();
        }

        // Confirm that a new task class guid for split MIME file task has been added
        [Test]
        public static void SchemaVersion207_VerifyNewTaskClass([Values] bool upgradeFromPreviousSchema)
        {
            // Arrange
            string dbName = UtilityMethods.FormatInvariant($"Test_SchemaVersion207_{upgradeFromPreviousSchema}");

            // Act
            var fileProcessingDB =
                upgradeFromPreviousSchema
                ? _testDbManager.GetDatabase(_DB_V205_17, dbName)
                : _testDbManager.GetNewDatabase(dbName);

            // Assert

            // Make sure schema version is at least 207
            Assert.That(fileProcessingDB.DBSchemaVersion, Is.GreaterThanOrEqualTo(207));

            // Confirm the new task class exists
            using var roleConnection = new ExtractRoleConnection(fileProcessingDB.DatabaseServer, fileProcessingDB.DatabaseName);
            roleConnection.Open();
            using var cmd = roleConnection.CreateCommand();
            cmd.CommandText = "SELECT Name FROM TaskClass WHERE GUID = 'A941CCD2-4BF2-4D3E-8B3F-CA17AE340D73'";
            Assert.AreEqual("Core: Split MIME file", cmd.ExecuteScalar());
        }

        // Confirm that ExpandAttributes service settings are updated by the schema update code
        // The ExpandAttributes ETL update is also indirectly tested by the ETL test project (TestAttributeExpander.TestIssue_16038)
        [Test]
        public static void SchemaVersion208_VerifyExpandAttributesSettings()
        {
            // Arrange
            string dbName = UtilityMethods.FormatInvariant($"Test_SchemaVersion208_VerifyExpandAttributesSettings");

            // Act
            var fileProcessingDB = _testDbManager.GetDatabase(_DB_V207, dbName);

            // Assert

            // Make sure schema version is at least 208
            Assert.That(fileProcessingDB.DBSchemaVersion, Is.GreaterThanOrEqualTo(208));

            // Get json for all the services
            List<JObject> services = GetServices(fileProcessingDB);

            // Confirm that the json for the ExpandAttributes service is correct
            JObject expandAttributesService =
                services.SingleOrDefault(jobject => (string)jobject["$type"] == "Extract.ETL.ExpandAttributes, Extract.ETL");
            Assert.NotNull(expandAttributesService);

            // Check the version
            var version = (int)expandAttributesService["Version"];
            Assert.AreEqual(3, version,
                message: "This test may need updating to handle ETL service updates");

            // Confirm that AttributeSetNameID has been changed to AttributeSetName
            JArray dashboardAttributeFields = (JArray)expandAttributesService["DashboardAttributes"];
            Assert.AreEqual(4, dashboardAttributeFields.Count);
            Assert.IsTrue(dashboardAttributeFields.All(jobject => jobject["AttributeSetNameID"] is null));
            Assert.IsTrue(dashboardAttributeFields.All(jobject => !string.IsNullOrEmpty((string)jobject["AttributeSetName"])));

            // Confirm that the json for the other services was not updated
            List<JObject> otherServices = services
                .Where(jobject => (string)jobject["$type"] != "Extract.ETL.ExpandAttributes, Extract.ETL")
                .ToList();

            Assert.AreEqual(7, otherServices.Count);
            Assert.IsTrue(otherServices.All(jobject => (int)jobject["Version"] == 1),
                message: "This test may need updating to handle ETL service updates");
        }

        #endregion Tests

        #region Utils

        private static bool IndexExists(DbConnection connection, string tableName, string indexName)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = $@"IF (IndexProperty(Object_Id('{tableName}'), '{indexName}', 'IndexID') IS NOT NULL) BEGIN SELECT 1 END";
            return cmd.ExecuteScalar() is int;
        }

        private static List<JObject> GetServices(FileProcessingDB fileProcessingDB)
        {
            List<JObject> servicesJson = new();
            using ExtractRoleConnection roleConnection = new(fileProcessingDB.DatabaseServer, fileProcessingDB.DatabaseName);
            roleConnection.Open();
            using var cmd = roleConnection.CreateCommand();
            cmd.CommandText = "SELECT Settings FROM DatabaseService";
            using var servicesReader = cmd.ExecuteReader();
            while (servicesReader.Read())
            {
                var serviceSettings = servicesReader.GetString(0);
                var jsonObject = JObject.Parse(serviceSettings);
                servicesJson.Add(jsonObject);
            }

            return servicesJson;
        }

        #endregion
    }
}
