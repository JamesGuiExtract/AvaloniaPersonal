using AttributeDbMgrComponentsLib;
using Extract.Database;
using Extract.SqlDatabase;
using Extract.Testing.Utilities;
using NUnit.Framework;
using System.Collections.Generic;
using UCLID_COMUTILSLib;
using UCLID_DATAENTRYCUSTOMCOMPONENTSLib;
using UCLID_FILEPROCESSINGLib;
using UCLID_REDACTIONCUSTOMCOMPONENTSLib;

namespace Extract.FileActionManager.Database.Test
{
    /// <summary>
    /// Testing class for methods used for file processing in IFileProcessingDB.
    /// </summary>
    [Category("TestMisc")]
    [TestFixture]
    public class TestMisc
    {
        #region Constants

        static readonly int _CURRENT_WORKFLOW = -1;
        static readonly string _ATTRIBUTE_SET_NAME = "Expected";
        static readonly string _COUNTER_NAME = "MCData";

        // This is the "default" password for "ExtractRole" used when there is no valid DatabaseID available
        static readonly string _DEFAULT_PW = "02afde95fb8fbb25.9fF"; 

        static readonly string _TEST_IMAGE3 = "Resources.TestImage003.tif";
        static readonly string _TEST_IMAGE3_VOA = "Resources.TestImage003.tif.voa";


        #endregion Constants

        #region Fields

        /// <summary>
        /// Manages test files.
        /// </summary>
        static TestFileManager<TestMisc> _testFiles;

        /// <summary>
        /// Manages test FAM DBs.
        /// </summary>
        static FAMTestDBManager<TestMisc> _testDbManager;

        #endregion Fields

        #region Overhead

        /// <summary>
        /// Initializes the test fixture.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestMisc>();
            _testDbManager = new FAMTestDBManager<TestMisc>();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [OneTimeTearDown]
        public static void FinalCleanup()
        {
            if (_testFiles != null)
            {
                _testFiles.Dispose();
                _testFiles = null;
            }

            if (_testDbManager != null)
            {
                _testDbManager.Dispose();
                _testDbManager = null;
            }
        }

        #endregion Overhead

        #region Test Methods

        /// The test with a missing databse ID is to test code to prevent potential hangs obtaining
        /// app role connections (https://extract.atlassian.net/browse/ISSUE-17912).
        [Test, Category("Automated")]
        [TestCase(false, TestName = "IProductSpecificDBMgrs with valid database ID")]
        [TestCase(true, TestName = "IProductSpecificDBMgrs with missing database ID")]
        public static void ProductManagers(bool testWithMissingDatabaseID)
        {
            string testDbName = _testDbManager.GenerateDatabaseName();
            using var dbWrapper = new OneWorkflow<TestMisc>(_testDbManager, testDbName, false);

            using NoAppRoleConnection connection = new("(local)", testDbName);
            connection.Open();

            dbWrapper.FileProcessingDB.SetDBInfoSetting("EnableDataEntryCounters", "1", true, true);
            int countersAdded = dbWrapper.FileProcessingDB.ExecuteCommandQuery(
                $@"INSERT INTO [DataEntryCounterDefinition] ([Name], [AttributeQuery], [RecordOnLoad], [RecordOnSave])
                    VALUES ('{_COUNTER_NAME}', '{_COUNTER_NAME}', 1, 1)");
            Assert.AreEqual(1, countersAdded);

            if (testWithMissingDatabaseID)
            {
                dbWrapper.FileProcessingDB.SetDBInfoSetting("DatabaseID", "", true, true);
                Assert.AreEqual("0", dbWrapper.FileProcessingDB.GetDBInfoSetting("DatabaseHash", false));

                // Sync the password for ExtractRole with the password that would be used for an invalid database ID
                DBMethods.ExecuteDBQuery(connection, $"ALTER APPLICATION ROLE ExtractRole WITH PASSWORD = '{_DEFAULT_PW}'");
            }

            string imageFileName = _testFiles.GetFile(_TEST_IMAGE3);
            string voaFileName = _testFiles.GetFile(_TEST_IMAGE3_VOA);
            var fileRecord = dbWrapper.FileProcessingDB.AddFile(
                imageFileName, dbWrapper.Action1, _CURRENT_WORKFLOW, EFilePriority.kPriorityNormal, false, false,
                EActionStatus.kActionPending, true, out var _, out EActionStatus t2);

            IUnknownVector attributes = new();
            attributes.LoadFrom(voaFileName, false);

            int actionId = dbWrapper.FileProcessingDB.GetActionID(dbWrapper.Action1);
            int sessionId = dbWrapper.FileProcessingDB.StartFileTaskSession(
                Constants.TaskClassStoreRetrieveAttributes, fileRecord.FileID, actionId);

            DataEntryProductDBMgr dataEntryMgr = new();
            dataEntryMgr.Initialize(dbWrapper.FileProcessingDB);
            dataEntryMgr.RecordCounterValues(true, sessionId, attributes);

            AttributeDBMgr attributeMgr = new();
            attributeMgr.FAMDB = dbWrapper.FileProcessingDB;
            attributeMgr.CreateNewAttributeSetName(_ATTRIBUTE_SET_NAME);
            attributeMgr.CreateNewAttributeSetForFile(sessionId, _ATTRIBUTE_SET_NAME, attributes, true, true, true, true);

            IDShieldProductDBMgr idShieldMgr = new();
            idShieldMgr.Initialize(dbWrapper.FileProcessingDB);
            idShieldMgr.AddIDShieldData(sessionId, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, false);

            new List<string>(new[] { "DataEntryCounterValue", "AttributeSetForFile", "IDShieldData" })
                .ForEach(table =>
                {
                    using var cmd = DBMethods.CreateDBCommand(connection, $"SELECT COUNT(*) FROM [{table}]", null);
                    Assert.AreEqual(1, cmd.ExecuteScalar(), $"Expected 1 {table} record.");
                });

            if (testWithMissingDatabaseID)
            {
                // Confirm we're still operating without a valid database hash, but that we've been authenticated as
                // "ExtractRole" for the above calls.
                Assert.AreEqual("0", dbWrapper.FileProcessingDB.GetDBInfoSetting("DatabaseHash", false));
                dbWrapper.FileProcessingDB.ExecuteCommandQuery($@"UPDATE [DBInfo] SET [Value] = USER_NAME() WHERE [Name] = 'EmailUsername'");
                dbWrapper.FileProcessingDB.CloseAllDBConnections();
                Assert.AreEqual("ExtractRole", dbWrapper.FileProcessingDB.GetDBInfoSetting("EmailUsername", false));
            }
        }

        #endregion Test Methods
    }
}
