using Extract.SqlDatabase;
using Extract.Testing.Utilities;
using Extract.Utilities;
using Extract.Web.ApiConfiguration.Services;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using Extract.Web.ApiConfiguration.Models;

namespace Extract.FileActionManager.Database.Test
{
    public enum DatabaseType
    {
        OldSchemaWithRoles,
        OldSchemaNoRoles,
        CreateNewDatabase
    }

    public enum EmailSourceDatabaseType
    {
        OldSchemaWithEmailSource,
        OldSchemaNoEmailSource,
        CreateNewDatabase
    }

    public enum BasicDatabaseType
    {
        OldSchema,
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
        static readonly string _DB_V213 = "Resources.DBVersion213.bak";
        static readonly string _DB_V215 = "Resources.DBVersion215.bak";
        static readonly string _DB_V216_WithWebSettings = "Resources.DBVersion216_WithWebSettings.bak";

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
                ? dbWrapper.FileProcessingDB.GetFilesToProcessAdvanced(dbWrapper.Actions[0], 10, bstrUserName: "",
                    bUseRandomIDForQueueOrder: true, eQueueMode: EQueueType.kPendingAnyUserOrNoUser)
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
                Assert.DoesNotThrow(() => extractRoleConnection = new ExtractRoleConnection("(local)", dbName)
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
                Assert.DoesNotThrow(() => reportingRoleConnection = new ExtractReportingRoleConnection("(local)", dbName)
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
                Assert.DoesNotThrow(() => extractRoleConnection = new ExtractRoleConnection("(local)", dbName)
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

        /// <summary>
        /// Confirm that ExpandAttributes service settings and statuses are updated by the schema update code
        /// https://extract.atlassian.net/browse/ISSUE-18060
        /// https://extract.atlassian.net/browse/ISSUE-18909
        ///
        /// The ExpandAttributes ETL update is also indirectly tested by the ETL test project (TestAttributeExpander.TestIssue_16038)
        /// </summary>
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
            List<(JObject settings, JObject status)> services = GetServices(fileProcessingDB);

            // Confirm that the json for the ExpandAttributes service is correct
            JObject expandAttributesServiceSettings = services
                .Select(pair => pair.settings)
                .SingleOrDefault(jobject => (string)jobject["$type"] == "Extract.ETL.ExpandAttributes, Extract.ETL");
            Assert.NotNull(expandAttributesServiceSettings);

            JObject expandAttributesServiceStatus = services
                .Select(pair => pair.status)
                .SingleOrDefault(jobject =>
                    (string)jobject["$type"] == "Extract.ETL.ExpandAttributes+ExpandAttributesStatus, Extract.ETL");
            Assert.NotNull(expandAttributesServiceStatus);

            Assert.Multiple(() =>
            {
                CheckExpandAttributesServiceSettings(expandAttributesServiceSettings);
                CheckExpandAttributesServiceStatus(expandAttributesServiceStatus);
            });

            // Confirm that the json for the other services was not updated
            List<JObject> otherServices = services
                .Select(pair => pair.settings)
                .Where(jobject => (string)jobject["$type"] != "Extract.ETL.ExpandAttributes, Extract.ETL")
                .ToList();

            Assert.AreEqual(7, otherServices.Count);
            Assert.IsTrue(otherServices.All(jobject => (int)jobject["Version"] == 1),
                message: "This test may need updating to handle ETL service updates");
        }

        private static void CheckExpandAttributesServiceSettings(JObject expandAttributesServiceSettings)
        {
            // Check the version
            var version = (int)expandAttributesServiceSettings["Version"];
            Assert.AreEqual(3, version,
                message: "This test may need updating to handle ETL service updates");

            // Confirm that AttributeSetNameID has been changed to AttributeSetName in the settings
            JArray dashboardAttributeFields = (JArray)expandAttributesServiceSettings["DashboardAttributes"];
            Assert.AreEqual(4, dashboardAttributeFields.Count);
            Assert.IsTrue(dashboardAttributeFields.All(jobject => jobject["AttributeSetNameID"] is null));
            Assert.IsTrue(dashboardAttributeFields.All(jobject => !string.IsNullOrEmpty((string)jobject["AttributeSetName"])));
        }

        private static void CheckExpandAttributesServiceStatus(JObject expandAttributesServiceStatus)
        {
            // Check the version
            var version = (int)expandAttributesServiceStatus["Version"];
            Assert.AreEqual(3, version,
                message: "This test may need updating to handle ETL service status updates");

            // Confirm that AttributeSetNameID has been changed to AttributeSetName in the statuses
            JObject dashboardAttributeFieldStatuses = (JObject)expandAttributesServiceStatus["LastIDProcessedForDashboardAttribute"];
            List<(string name, JToken value)> props = dashboardAttributeFieldStatuses.Properties()
                .Select(prop => (name: prop.Name, value: prop.Value))
                .ToList();
            Assert.AreEqual(5, props.Count);
            Assert.Multiple(() =>
            {
                Assert.AreEqual("$type", props[0].name);

                Assert.AreEqual("DocumentType,\"Data, Found By Rules\",/DocumentType", props[1].name);
                Assert.AreEqual(-1, (int)props[1].value);

                Assert.AreEqual("DocumentType,\"DataBefore\"\"QA\"\"\",//DocumentType", props[2].name);
                Assert.AreEqual(0, (int)props[2].value);

                Assert.AreEqual("DocumentType,\"Data, Found By Rules \"\"Expecteds\"\"\",/*/DocumentType", props[3].name);
                Assert.AreEqual(1, (int)props[3].value);

                Assert.AreEqual("DocumentType,DataAfterLastVerifyOrQA,/root/DocumentType", props[4].name);
                Assert.AreEqual(2, (int)props[4].value);
            });
        }

        [Test]
        public static void SchemaVersion209_UserSpecificQueue([Values] bool upgradeFromPreviousSchema)
        {
            // Arrange
            string dbName = _testDbManager.GenerateDatabaseName();

            // Act
            var fileProcessingDB =
                upgradeFromPreviousSchema
                ? _testDbManager.GetDatabase(_DB_V207, dbName)
                : _testDbManager.GetNewDatabase(dbName);

            // Assert

            Assert.That(fileProcessingDB.DBSchemaVersion, Is.GreaterThanOrEqualTo(209));

            using var roleConnection = new ExtractRoleConnection(fileProcessingDB.DatabaseServer, fileProcessingDB.DatabaseName);
            roleConnection.Open();
            using var cmd = roleConnection.CreateCommand();

            cmd.CommandText = @"SELECT 1
                WHERE COL_LENGTH('dbo.FileActionStatus', 'UserID') IS NOT NULL
                AND object_id('FK_FileActionStatus_FAMUser') IS NOT NULL
                AND object_id('FK_QueuedActionStatusChange_TargetFAMUser') IS NOT NULL";
            Assert.AreEqual(1, cmd.ExecuteScalar());

            cmd.CommandText = @"SELECT COUNT(*) FROM information_schema.parameters
                WHERE SPECIFIC_NAME = 'GetFilesToProcessForAction'
                AND PARAMETER_NAME = '@LimitToUserQueue'";
            Assert.AreEqual(1, cmd.ExecuteScalar());
        }

        [Test]
        public static void SchemaVersion211_UserSpecificQueue([Values] bool upgradeFromPreviousSchema)
        {
            // Arrange
            string dbName = _testDbManager.GenerateDatabaseName();

            // Act
            var fileProcessingDB =
                upgradeFromPreviousSchema
                ? _testDbManager.GetDatabase(_DB_V207, dbName)
                : _testDbManager.GetNewDatabase(dbName);

            // Assert

            Assert.That(fileProcessingDB.DBSchemaVersion, Is.GreaterThanOrEqualTo(211));

            using var roleConnection = new ExtractRoleConnection(fileProcessingDB.DatabaseServer, fileProcessingDB.DatabaseName);
            roleConnection.Open();
            using var cmd = roleConnection.CreateCommand();

            cmd.CommandText = @"SELECT COUNT(*) FROM information_schema.parameters
                WHERE SPECIFIC_NAME = 'GetFilesToProcessForAction'
                AND PARAMETER_NAME = '@IncludeFilesQueuedForOthers'";
            Assert.AreEqual(1, cmd.ExecuteScalar());
        }

        // Confirm that the ExpectedLogin table exists
        [Test]
        public static void SchemaVersion212_ExternalLogin([Values] bool upgradeFromPreviousSchema)
        {
            // Arrange
            string dbName = UtilityMethods.FormatInvariant($"Test_SchemaVersion212_{upgradeFromPreviousSchema}");

            // Act
            var fileProcessingDB =
                upgradeFromPreviousSchema
                ? _testDbManager.GetDatabase(_DB_V207, dbName)
                : _testDbManager.GetNewDatabase(dbName);

            // Assert

            // Make sure schema version is at least 212
            Assert.That(fileProcessingDB.DBSchemaVersion, Is.GreaterThanOrEqualTo(212));

            // Confirm the table exists by writing/reading from it
            fileProcessingDB.LoginUser("admin", "a");
            string expectedUsername = "Ayodeji_Akporobome@PotatoPotahto.com";
            string expectedPassword = "purplish unopposed writing doodle";
            fileProcessingDB.SetExternalLogin("TestLogin", expectedUsername, expectedPassword);
            fileProcessingDB.GetExternalLogin("TestLogin", out var actualUsername, out var actualPassword);

            Assert.AreEqual(expectedUsername, actualUsername);
            Assert.AreEqual(expectedPassword, actualPassword);
        }

        // Confirm that the EmailSource table doesn't have a QueueEventID column
        [Test]
        public static void SchemaVersion214_EmailSourceUpdate([Values] EmailSourceDatabaseType databaseType)
        {
            // Arrange
            string dbName = UtilityMethods.FormatInvariant(
                $"Test_SchemaVersion214{Enum.GetName(typeof(EmailSourceDatabaseType), databaseType)}");

            // Act
            using var dbWrapper = databaseType switch
            {
                EmailSourceDatabaseType.OldSchemaWithEmailSource => _testDbManager.GetDisposableDatabase(_DB_V213, dbName),
                EmailSourceDatabaseType.OldSchemaNoEmailSource => _testDbManager.GetDisposableDatabase(_DB_V194, dbName),
                EmailSourceDatabaseType.CreateNewDatabase => _testDbManager.GetDisposableDatabase(dbName),
                _ => throw new NotImplementedException()
            };

            // Assert

            // Make sure schema version is at least 214
            Assert.That(dbWrapper.FileProcessingDB.DBSchemaVersion, Is.GreaterThanOrEqualTo(214));

            using var connection = new ExtractRoleConnection(dbWrapper.FileProcessingDB.DatabaseServer, dbWrapper.FileProcessingDB.DatabaseName);
            connection.Open();

            // Confirm that the EmailSource table has a OutlookEmailID column but doesn't have a QueueEventID column
            Assert.That(ColumnExists(connection, "dbo.EmailSource", "OutlookEmailID"), Is.True);
            Assert.That(ColumnExists(connection, "dbo.EmailSource", "QueueEventID"), Is.False);
        }

        [Test]
        public static void SchemaVersion215_EmailSourceUpdate([Values] EmailSourceDatabaseType databaseType)
        {
            // Arrange
            string dbName = UtilityMethods.FormatInvariant(
                $"Test_SchemaVersion215{Enum.GetName(typeof(EmailSourceDatabaseType), databaseType)}");

            // Act
            using var dbWrapper = databaseType switch
            {
                EmailSourceDatabaseType.OldSchemaWithEmailSource => _testDbManager.GetDisposableDatabase(_DB_V213, dbName),
                EmailSourceDatabaseType.OldSchemaNoEmailSource => _testDbManager.GetDisposableDatabase(_DB_V194, dbName),
                EmailSourceDatabaseType.CreateNewDatabase => _testDbManager.GetDisposableDatabase(dbName),
                _ => throw new NotImplementedException()
            };

            // Assert

            // Make sure schema version is at least 215
            Assert.That(dbWrapper.FileProcessingDB.DBSchemaVersion, Is.GreaterThanOrEqualTo(215));

            using var connection = new ExtractRoleConnection(dbWrapper.FileProcessingDB.DatabaseServer, dbWrapper.FileProcessingDB.DatabaseName);
            connection.Open();

            // Confirm that the EmailSource table has PendingMoveFromEmailFolder and PendingNotifyFromEmailFolder
            Assert.Multiple(() =>
            {
                Assert.That(ColumnExists(connection, "dbo.EmailSource", "PendingMoveFromEmailFolder"));
                Assert.That(ColumnExists(connection, "dbo.EmailSource", "PendingNotifyFromEmailFolder"));
                Assert.That(IndexExists(connection, "dbo.EmailSource", "IX_EmailSource_PendingMoveFromEmailFolder"));
                Assert.That(IndexExists(connection, "dbo.EmailSource", "IX_EmailSource_PendingNotifyFromEmailFolder"));
            });
        }

        [Test]
        public static void SchemaVersion216_RemoveSkippedFile([Values] bool upgrade)
        {
            // Arrange
            string dbName = UtilityMethods.FormatInvariant($"Test_SchemaVersion216_Upgrade={upgrade}");

            // Act
            using var dbWrapper = upgrade switch
            {
                true => _testDbManager.GetDisposableDatabase(_DB_V215, dbName),
                false => _testDbManager.GetDisposableDatabase(dbName)
            };

            // Assert

            // Make sure schema version is at least 216
            Assert.That(dbWrapper.FileProcessingDB.DBSchemaVersion, Is.GreaterThanOrEqualTo(216));

            using var connection = new ExtractRoleConnection(dbWrapper.FileProcessingDB.DatabaseServer, dbWrapper.FileProcessingDB.DatabaseName);
            connection.Open();

            Assert.Multiple(() =>
            {
                // Confirm that the FileActionStatus table has a FAMSessionID column
                Assert.That(ColumnExists(connection, "dbo.FileActionStatus", "FAMSessionID"));

                // Confirm that the SkippedFile table is gone
                Assert.That(TableExists(connection, "dbo", "SkippedFile"), Is.False);

                if (upgrade)
                {
                    // Confirm that the skipped user was transfered to the fileactionstatus table
                    IActionStatistics userStats = dbWrapper.FileProcessingDB.GetFileStatsForUser("jane_doe", 6, false);
                    Assert.AreEqual(1, userStats.NumDocumentsSkipped);
                }
            });
        }

        [Test]
        public static void SchemaVersion217_AddWebApiConfigurationTable([Values] bool upgrade)
        {
            // Arrange
            string dbName = UtilityMethods.FormatInvariant($"Test_SchemaVersion217_Upgrade={upgrade}");

            // Act
            using var dbWrapper = upgrade switch
            {
                true => _testDbManager.GetDisposableDatabase(_DB_V215, dbName),
                false => _testDbManager.GetDisposableDatabase(dbName)
            };

            // Assert

            // Make sure schema version is at least 217
            Assert.That(dbWrapper.FileProcessingDB.DBSchemaVersion, Is.GreaterThanOrEqualTo(217));

            using var connection = new ExtractRoleConnection(dbWrapper.FileProcessingDB.DatabaseServer, dbWrapper.FileProcessingDB.DatabaseName);
            connection.Open();

            Assert.That(TableExists(connection, "dbo", "WebAPIConfiguration"), Is.True);
        }

        /// <summary>
        /// Confirm that web API configuration settings are copied to the new table when upgrading a database
        /// </summary>
        [Test]
        public static void SchemaVersion218_CopyWebConfigurationsToNewTable()
        {
            // Arrange
            string dbName = UtilityMethods.FormatInvariant($"Test_SchemaVersion218_Upgrade");

            // Act

            // There are two workflows defined that have the Web API actions etc
            // One of the workflows has redaction settings specified as well
            var dbWrapper = _testDbManager.GetDisposableDatabase(_DB_V216_WithWebSettings, dbName);

            // Assert

            // Make sure schema version is at least 218
            Assert.That(dbWrapper.FileProcessingDB.DBSchemaVersion, Is.GreaterThanOrEqualTo(218));

            var configJsonStrings = GetWebApiConfigs(dbWrapper.FileProcessingDB);

            // Both workflows will trigger the creation of a DocumentAPI configuration
            // Only WF2 (with redaction settings) will result in a Redaction configuration
            Assert.AreEqual(3, configJsonStrings.Count);

            // TODO: What are these migrated configurations supposed to be named?
            // Current impl is to name after the workflow and the type of config
            CollectionAssert.AreEquivalent(
                new[]
                {
                    "Workflow: WF1 Type: DocumentAPI",
                    "Workflow: WF2 Type: DocumentAPI",
                    "Workflow: WF2 Type: Redaction"
                },
                configJsonStrings.Keys);

            Assert.Multiple(() =>
            {
                // Confirm that the JSON is correct for each of the copies
                // Compare first expected document API settings JSON
                var docConfig1Exp = @"{
  ""TypeName"": ""DocumentApiWebConfigurationV1"",
  ""DataTransferObject"": {
    ""ConfigurationName"": ""Workflow: WF1 Type: DocumentAPI"",
    ""IsDefault"": true,
    ""WorkflowName"": ""WF1"",
    ""AttributeSet"": ""WF1Attributes"",
    ""ProcessingAction"": ""WF1Process"",
    ""PostProcessingAction"": ""WF1PostProcess"",
    ""DocumentFolder"": ""C:\\A 'folder'\\Another \""name\"""",
    ""StartWorkflowAction"": ""WF1StartAction"",
    ""EndWorkflowAction"": ""WF1EndAction"",
    ""PostWorkflowAction"": ""WF1PostWF"",
    ""OutputFileNameMetadataField"": ""WF1OutputFileMDF"",
    ""OutputFileNameMetadataInitialValueFunction"": ""A function of <SourceDocName> goes here (wf 1)""
  }
}";
                var docConfig1 = configJsonStrings["Workflow: WF1 Type: DocumentAPI"];
                Assert.That(docConfig1, Is.EqualTo(docConfig1Exp.Replace("\r\n", "\n"))); // Rapidjson PrettyWriter uses \n for newlines

                // Compare second expected document API settings JSON
                var docConfig2Exp = @"{
  ""TypeName"": ""DocumentApiWebConfigurationV1"",
  ""DataTransferObject"": {
    ""ConfigurationName"": ""Workflow: WF2 Type: DocumentAPI"",
    ""IsDefault"": true,
    ""WorkflowName"": ""WF2"",
    ""AttributeSet"": ""WF2Attributes"",
    ""ProcessingAction"": ""WF2Process"",
    ""PostProcessingAction"": ""WF2PP"",
    ""DocumentFolder"": ""\\\\mysrv\\My documents"",
    ""StartWorkflowAction"": ""WF2Start"",
    ""EndWorkflowAction"": ""WF2End"",
    ""PostWorkflowAction"": ""WF2PostWF"",
    ""OutputFileNameMetadataField"": ""WF2OutputFileMDF"",
    ""OutputFileNameMetadataInitialValueFunction"": ""<SourceDocName>.redacted""
  }
}";
                var docConfig2 = configJsonStrings["Workflow: WF2 Type: DocumentAPI"];
                Assert.That(docConfig2, Is.EqualTo(docConfig2Exp.Replace("\r\n", "\n"))); // Rapidjson PrettyWriter uses \n for newlines

                // Compare expected redaction settings JSON
                var redactionConfigExp = @"{
  ""TypeName"": ""RedactionWebConfigurationV1"",
  ""DataTransferObject"": {
    ""ConfigurationName"": ""Workflow: WF2 Type: Redaction"",
    ""IsDefault"": true,
    ""WorkflowName"": ""WF2"",
    ""AttributeSet"": ""WF2Attributes"",
    ""ProcessingAction"": ""WF2Process"",
    ""PostProcessingAction"": ""WF2PP"",
    ""ActiveDirectoryGroups"": [],
    ""EnableAllUserPendingQueue"": false,
    ""DocumentTypeFileLocation"": ""D:\\DocTypes.idx"",
    ""RedactionTypes"": [
      ""DLN"",
      ""MinorName""
    ]
  }
}";
                var redactionConfig = configJsonStrings["Workflow: WF2 Type: Redaction"];
                Assert.That(redactionConfig, Is.EqualTo(redactionConfigExp.Replace("\r\n", "\n"))); // Rapidjson PrettyWriter uses \n for newlines

                // Confirm that the json can be retrieved with IFileProcessingDB and deserialized without error
                IConfigurationDatabaseService configService = new ConfigurationDatabaseService(dbWrapper.FileProcessingDB);
                IList<ICommonWebConfiguration> configs = configService.Configurations;
                Assert.That(configs.Count, Is.EqualTo(3));
                CollectionAssert.AreEquivalent(
                    new[]
                    {
                        nameof(DocumentApiConfiguration),
                        nameof(DocumentApiConfiguration),
                        nameof(RedactionWebConfiguration),
                    }, configs.Select(c => c.GetType().Name));
            });
        }

        [Test]
        public static void LabDESchemaVersion19_IndexUpdate([Values] BasicDatabaseType databaseType)
        {
            // Arrange
            string dbName = UtilityMethods.FormatInvariant(
                $"Test_Version19_IndexUpdate{Enum.GetName(typeof(BasicDatabaseType), databaseType)}");

            // Act
            using var dbWrapper = databaseType switch
            {
                BasicDatabaseType.OldSchema => _testDbManager.GetDisposableDatabase(_DB_V215, dbName),
                BasicDatabaseType.CreateNewDatabase => _testDbManager.GetDisposableDatabase(dbName),
                _ => throw new NotImplementedException()
            };

            // Assert

            // Make sure schema version is at least 221
            Assert.That(dbWrapper.FileProcessingDB.DBSchemaVersion, Is.GreaterThanOrEqualTo(221));
            Assert.That(dbWrapper.FileProcessingDB.GetDBInfoSetting("LabDESchemaVersion", true), Is.EqualTo("19"));

            using var connection = new ExtractRoleConnection(dbWrapper.FileProcessingDB.DatabaseServer, dbWrapper.FileProcessingDB.DatabaseName);
            connection.Open();

            Assert.That(IndexExists(connection, "dbo.LabDEOrder", "IX_ORDER_EncounterID"));
            Assert.That(IndexExists(connection, "dbo.LabDEProvider", "IX_LabDEProvider_OtherProviderID"));
            Assert.That(IndexExists(connection, "dbo.FAMSession", "IX_FAMSession_StartTime"));
            Assert.That(IndexExists(connection, "dbo.LabDEProvider", "IX_LabDEProvider_ID"));
            Assert.That(IndexExists(connection, "dbo.LabDEProvider", "IX_LabDEProvider_ProviderName_ProviderType_Inactive"));
            Assert.That(IndexExists(connection, "dbo.LabDEProvider", "IX_LabDEProvider_ProviderType_Inactive"));
        }

        [Test]
        public static void LabDESchemaVersion19_IndexUpdateWithRenamingIndexes()
        {
            // Arrange
            string dbName = "Test_LabDESchemaVersion19_IndexUpdateWithRenamingIndexes";

            // Act
            FileProcessingDB fileProcessingDB = _testDbManager.GetDatabase(_DB_V215, dbName, false);
            try
            {
                using var connection = new NoAppRoleConnection(fileProcessingDB.DatabaseServer, fileProcessingDB.DatabaseName);
                connection.Open();

                // Add some poorly named indexes that need to get renamed.
                var command = connection.CreateCommand();
                command.CommandText = "CREATE INDEX [SillyIndexName] ON [dbo].[LabDEProvider]([OtherProviderID])";
                command.ExecuteNonQuery();

                command.CommandText = "CREATE INDEX [TerribleIndexName] ON [dbo].[LabDEPatient] ([MergedInto]) INCLUDE ([FirstName], [LastName], [DOB])";
                command.ExecuteNonQuery();

                command.CommandText = "CREATE NONCLUSTERED INDEX [CatsAreTheBest] ON [dbo].[FAMSession] ([StartTime]) INCLUDE ([MachineID], [StopTime], [FPSFileID], [ActionID], [Queuing], [Processing])";
                command.ExecuteNonQuery();

                command.CommandText = "CREATE NONCLUSTERED INDEX [WorldDominationPlans] ON [dbo].[LabDEProvider] ([ID])";
                command.ExecuteNonQuery();

                command.CommandText = "CREATE NONCLUSTERED INDEX [HowToHackFBI] ON [dbo].[LabDEProvider] ([FirstName],[MiddleName],[LastName],[ProviderType],[Inactive]) INCLUDE ([ID])";
                command.ExecuteNonQuery();

                command.CommandText = "CREATE NONCLUSTERED INDEX [IsBreadReal] ON [dbo].[LabDEProvider] ([ProviderType],[Inactive]) INCLUDE ([LastName])";
                command.ExecuteNonQuery();

                // Get back on the latest database.
                fileProcessingDB.UpgradeToCurrentSchema(null);

                // Clear the database a few times just to be sure no errors occur.
                fileProcessingDB.Clear(true);
                fileProcessingDB.Clear(false);

                // Assert

                // Make sure schema version is at least 221
                Assert.That(fileProcessingDB.DBSchemaVersion, Is.GreaterThanOrEqualTo(221));
                Assert.That(int.Parse(fileProcessingDB.GetDBInfoSetting("LabDESchemaVersion", true)), Is.GreaterThanOrEqualTo(19));

                // Check that new indexes are created.
                Assert.That(IndexExists(connection, "dbo.LabDEOrder", "IX_ORDER_EncounterID"));
                Assert.That(IndexExists(connection, "dbo.LabDEProvider", "IX_LabDEProvider_OtherProviderID"));
                Assert.That(IndexExists(connection, "dbo.FAMSession", "IX_FAMSession_StartTime"));
                Assert.That(IndexExists(connection, "dbo.LabDEProvider", "IX_LabDEProvider_ID"));
                Assert.That(IndexExists(connection, "dbo.LabDEProvider", "IX_LabDEProvider_ProviderName_ProviderType_Inactive"));
                Assert.That(IndexExists(connection, "dbo.LabDEProvider", "IX_LabDEProvider_ProviderType_Inactive"));

                // Check that my garbage indexes were renamed.
                Assert.That(!IndexExists(connection, "dbo.LabDEPatient", "TerribleIndexName"));
                Assert.That(!IndexExists(connection, "dbo.LabDEProvider", "SillyIndexName"));
                Assert.That(!IndexExists(connection, "dbo.FAMSession", "CatsAreTheBest"));
                Assert.That(!IndexExists(connection, "dbo.LabDEProvider", "WorldDominationPlans"));
                Assert.That(!IndexExists(connection, "dbo.LabDEProvider", "HowToHackFBI"));
                Assert.That(!IndexExists(connection, "dbo.LabDEProvider", "IsBreadReal"));
            }
            finally
            {
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        public static void LabDESchemaVersion18_IndexUpdateWithRenamingIndexes()
        {
            // Arrange
            string dbName = "Test_LabDESchemaVersion18_IndexUpdateWithRenamingIndexes";

            // Act
            FileProcessingDB fileProcessingDB = _testDbManager.GetDatabase(_DB_V215, dbName, false);
            try
            {
                using var connection = new NoAppRoleConnection(fileProcessingDB.DatabaseServer, fileProcessingDB.DatabaseName);
                connection.Open();

                // Add some poorly named indexes that need to get renamed.
                var command = connection.CreateCommand();
                command.CommandText = " CREATE NONCLUSTERED INDEX [TurtlesAreAwesome] ON [dbo].[LabDEOrder] (EncounterID)";
                command.ExecuteNonQuery();

                command.CommandText = " CREATE NONCLUSTERED INDEX [TheCakeIsALie] ON [dbo].[LabDEEncounter] ([EncounterDateTime])";
                command.ExecuteNonQuery();

                // Get back on the latest database.
                fileProcessingDB.UpgradeToCurrentSchema(null);

                // Clear the database a few times just to be sure no errors occur.
                fileProcessingDB.Clear(true);
                fileProcessingDB.Clear(false);

                // Assert

                // Make sure schema version is at least 221
                Assert.That(fileProcessingDB.DBSchemaVersion, Is.GreaterThanOrEqualTo(221));
                Assert.That(int.Parse(fileProcessingDB.GetDBInfoSetting("LabDESchemaVersion", true)), Is.GreaterThan(18));

                // Check that new indexes are created.
                Assert.That(IndexExists(connection, "dbo.LabDEOrder", "IX_Order_EncounterID"));
                Assert.That(IndexExists(connection, "dbo.LabDEEncounter", "IX_Encounter_EncounterDateTime"));

                // Check that my garbage indexes were renamed.
                Assert.That(!IndexExists(connection, "dbo.LabDEOrder", "TurtlesAreAwesome"));
                Assert.That(!IndexExists(connection, "dbo.LabDEEncounter", "TheCakeIsALie"));
            }
            finally
            {
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        #endregion Tests

        #region Utils

        private static bool IndexExists(DbConnection connection, string tableName, string indexName)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = $@"IF (IndexProperty(Object_Id('{tableName}'), '{indexName}', 'IndexID') IS NOT NULL) BEGIN SELECT 1 END";
            return cmd.ExecuteScalar() is int;
        }

        private static bool ColumnExists(DbConnection connection, string tableName, string columnName)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = $@"IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = '{columnName}' AND Object_ID = Object_ID('{tableName}')) BEGIN SELECT 1 END";
            return cmd.ExecuteScalar() is int;
        }

        private static bool TableExists(ExtractRoleConnection connection, string schema, string tableName)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = $@"IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{schema}' AND TABLE_NAME = '{tableName}') BEGIN SELECT 1 END";
            return cmd.ExecuteScalar() is int;
        }

        private static List<(JObject settings, JObject status)> GetServices(FileProcessingDB fileProcessingDB)
        {
            List<(JObject settings, JObject status)> servicesJson = new();
            using ExtractRoleConnection roleConnection = new(fileProcessingDB.DatabaseServer, fileProcessingDB.DatabaseName);
            roleConnection.Open();
            using var cmd = roleConnection.CreateCommand();
            cmd.CommandText = "SELECT Settings, Status FROM DatabaseService";
            using var servicesReader = cmd.ExecuteReader();
            while (servicesReader.Read())
            {
                var serviceSettings = servicesReader.GetString(0);
                var jsonObjectSettings = JObject.Parse(serviceSettings);
                JObject jsonObjectStatus = new();
                if (!servicesReader.IsDBNull(1))
                {
                    jsonObjectStatus = JObject.Parse(servicesReader.GetString(1));
                }
                servicesJson.Add((jsonObjectSettings, jsonObjectStatus));
            }

            return servicesJson;
        }

        private static IDictionary<string, string> GetWebApiConfigs(FileProcessingDB fileProcessingDB)
        {
            Dictionary<string, string> configs = new();
            using ExtractRoleConnection roleConnection = new(fileProcessingDB.DatabaseServer, fileProcessingDB.DatabaseName);
            roleConnection.Open();
            using var cmd = roleConnection.CreateCommand();
            cmd.CommandText = "SELECT [Name], [Settings] FROM [WebAPIConfiguration]";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var name = reader.GetString(0);
                var settings = reader.GetString(1);
                configs.Add(name, settings);
            }

            return configs;
        }

        #endregion
    }
}
