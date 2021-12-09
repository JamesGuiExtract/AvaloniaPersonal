using Extract.SqlDatabase;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Linq;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Database.Test
{
    [Category("Automated"), Category("FileProcessingDBSchemaUpdates")]
    [TestFixture]
    public class TestFAMDBSchemaUpdates
    {
        #region Constants

        static readonly string _DB_V194 = "Resources.DBVersion194.bak";
        static readonly string _DB_V201 = "Resources.DBVersion201.bak";

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

        /// Confirm that a new or upgraded version 202 database has the random queue feature
        [Test]
        public static void SchemaVersion202_VerifyGetFilesToProcess(
            [Values] bool upgradeFromPreviousSchema,
            [Values] bool useRandomQueue)
        {
            // Arrange
            int[] firstTenFilesAdded = Enumerable.Range(1, 10).ToArray();
            string dbName = UtilityMethods.FormatInvariant(
                $"Test_SchemaVersion202_{upgradeFromPreviousSchema}_{useRandomQueue}");

            using var dbWrapper =
                upgradeFromPreviousSchema
                ? _testDbManager.GetDisposableDatabase(dbName)
                : _testDbManager.GetDisposableDatabase(_DB_V201, dbName);

            // Act
            foreach (int i in Enumerable.Range(1, 100)) dbWrapper.addFakeFile(i, false);
            IUnknownVector filesToProcess =
                useRandomQueue
                ? dbWrapper.FileProcessingDB.GetRandomFilesToProcess(dbWrapper.Actions[0], 10, false, "")
                : dbWrapper.FileProcessingDB.GetFilesToProcess(dbWrapper.Actions[0], 10, false, "");

            // Assert
            int[] fileIDsToProcess = filesToProcess
                .ToIEnumerable<IFileRecord>()
                .Select(fileRecord => fileRecord.FileID)
                .ToArray();

            int expectedSchemaVersion = int.Parse(dbWrapper.FileProcessingDB.GetDBInfoSetting("ExpectedSchemaVersion", false));

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
        public static void SchemaVersion203_VerifyApplicationRoles(
            [Values] bool upgradeFromPreviousSchema,
            [Values] bool upgradeFromSchemaWithRoles)
        {
            string dbName = UtilityMethods.FormatInvariant(
                $"Test_SchemaVersion203_{upgradeFromPreviousSchema}_{upgradeFromSchemaWithRoles}");

            using var dbWrapper =
                upgradeFromPreviousSchema
                ? upgradeFromSchemaWithRoles
                    ? _testDbManager.GetDisposableDatabase(_DB_V201, dbName)
                    : _testDbManager.GetDisposableDatabase(_DB_V194, dbName)
                : _testDbManager.GetDisposableDatabase(dbName);

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

            int expectedSchemaVersion = int.Parse(dbWrapper.FileProcessingDB.GetDBInfoSetting("ExpectedSchemaVersion", false));

            // Make sure schema has the correct version number
            Assert.AreEqual(expectedSchemaVersion, dbWrapper.FileProcessingDB.DBSchemaVersion);
        }

        #endregion Tests
    }
}
