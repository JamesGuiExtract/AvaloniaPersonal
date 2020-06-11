using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Database.Test
{
    /// <summary>
    /// Testing class for database admin related methods in IFileProcessingDB.
    /// </summary>
    [Category("TestFAMDBAdmin")]
    [TestFixture]
    public class TestFAMDBAdmin
    {
        #region Constants

        static readonly string _LABDE_EMPTY_DB = "Resources.Demo_LabDE_Empty";
        static readonly string _LABDE_TEST_FILE1 = "Resources.TestImage001.tif";
        static readonly string _LABDE_TEST_FILE2 = "Resources.TestImage002.tif";
        static readonly string _LABDE_TEST_FILE3 = "Resources.TestImage003.tif";

        static readonly string _LABDE_ACTION1 = "A01_ExtractData";
        static readonly string _LABDE_ACTION2 = "A02_Verify";
        static readonly string _LABDE_ACTION3 = "A03_QA";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Manages test files.
        /// </summary>
        static TestFileManager<TestFAMFileProcessing> _testFiles;

        /// <summary>
        /// Manages test FAM DBs.
        /// </summary>
        static FAMTestDBManager<TestFAMFileProcessing> _testDbManager;

        #endregion Fields

        #region Overhead

        /// <summary>
        /// Initializes the test fixture.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestFAMFileProcessing>();
            _testDbManager = new FAMTestDBManager<TestFAMFileProcessing>();
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

        /// <summary>
        /// Tests the LoginUser method.
        /// </summary>
        [Test, Category("Automated")]
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login")]
        public static void LoginUser()
        {
            string testDbName = "Test_LoginUser";

            try
            {
                var fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                // Good password
                fileProcessingDb.LoginUser("admin", "a");

                // Bad password
                Assert.Throws<COMException>(() => fileProcessingDb.LoginUser("admin", "BadPassword"));

                // Unknown user
                Assert.Throws<COMException>(() => fileProcessingDb.LoginUser("UnknownUser", "a"));

                // User without password set
                fileProcessingDb.AddLoginUser("UserWithNoPassword");
                Assert.Throws<COMException>(() => fileProcessingDb.LoginUser("UserWithNoPassword", ""));
            }
            finally
            {
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        [Test, Category("Automated")]
        public static void OnetimePassword()
        {
            string testDbName = "Test_OnetimePassword";

            try
            {
                var fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);
                var fileProcessingDb2 = new FileProcessingDB();
                fileProcessingDb2.DuplicateConnection(fileProcessingDb);
                var fileProcessingDb3 = new FileProcessingDB();
                fileProcessingDb3.DuplicateConnection(fileProcessingDb);

                // One-time passwords should only be able to be created when logged in as admin
                Assert.Throws<COMException>(() => fileProcessingDb.GetOneTimePassword());

                fileProcessingDb.LoginUser("admin", "a");
                string onetimePassword = fileProcessingDb.GetOneTimePassword();

                // One-time passwords need to be used against "<Admin>"
                Assert.Throws<COMException>(() => fileProcessingDb2.LoginUser("Admin", onetimePassword));
                Assert.Throws<COMException>(() => fileProcessingDb2.LoginUser("<Admin>", "a"));
                Assert.IsFalse(fileProcessingDb2.LoggedInAsAdmin);

                fileProcessingDb2.LoginUser("<Admin>", onetimePassword);
                Assert.IsTrue(fileProcessingDb2.LoggedInAsAdmin);

                // One-time passwords should only be able to be used once.
                Assert.Throws<COMException>(() => fileProcessingDb3.LoginUser("<Admin>", onetimePassword));

                // However, another password should be able to be genereatd.
                onetimePassword = fileProcessingDb.GetOneTimePassword();
                fileProcessingDb3.LoginUser("<Admin>", onetimePassword);
                Assert.IsTrue(fileProcessingDb3.LoggedInAsAdmin);

                // Untested:
                // - Password is limited to use by same user/machine
                // - Password expires in 1 minute.
            }
            finally
            {
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        /// <summary>
        /// Tests the ModifyActionStatusForSelection method in a database without workflows
        /// </summary>
        [Test, Category("Automated")]
        public static void ModifyActionStatusForSelection()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestFAMFileProcessing>();
            _testDbManager = new FAMTestDBManager<TestFAMFileProcessing>();

            string testDbName = "Test_ModifyActionStatusForSelection";

            try
            {
                string testFileName1 = _testFiles.GetFile(_LABDE_TEST_FILE1);
                string testFileName2 = _testFiles.GetFile(_LABDE_TEST_FILE2);
                string testFileName3 = _testFiles.GetFile(_LABDE_TEST_FILE3);
                FileProcessingDB fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                int actionId1 = fileProcessingDb.GetActionID(_LABDE_ACTION1);
                int actionId2 = fileProcessingDb.GetActionID(_LABDE_ACTION2);
                int actionId3 = fileProcessingDb.GetActionID(_LABDE_ACTION3);

                // Initial statuses by File ID
                //            |  P  |  R  |  S  |  C  |  F 
                //   Action 1                     1,2           
                //   Action 2   1                  2
                //   Action 3   3                        2

                fileProcessingDb.AddFile(testFileName1, _LABDE_ACTION1, -1, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionCompleted, false, out bool alreadyExists, out EActionStatus previousStatus);
                fileProcessingDb.AddFile(testFileName1, _LABDE_ACTION2, -1, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);
                fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION1, -1, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionCompleted, false, out alreadyExists, out previousStatus);
                fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION2, -1, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionCompleted, false, out alreadyExists, out previousStatus);
                fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION3, -1, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionFailed, false, out alreadyExists, out previousStatus);
                fileProcessingDb.AddFile(testFileName3, _LABDE_ACTION3, -1, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);

                Assert.That(fileProcessingDb.GetStats(actionId1, false).NumDocuments == 2);
                Assert.That(fileProcessingDb.GetStats(actionId1, false).NumDocumentsComplete == 2);
                Assert.That(fileProcessingDb.GetStats(actionId2, false).NumDocuments == 2);
                Assert.That(fileProcessingDb.GetStats(actionId2, false).NumDocumentsPending == 1);
                Assert.That(fileProcessingDb.GetStats(actionId2, false).NumDocumentsComplete == 1);
                Assert.That(fileProcessingDb.GetStats(actionId3, false).NumDocuments == 2);
                Assert.That(fileProcessingDb.GetStats(actionId3, false).NumDocumentsPending == 1);
                Assert.That(fileProcessingDb.GetStats(actionId3, false).NumDocumentsFailed == 1);

                var fileSelector = new FAMFileSelector();
                fileSelector.AddQueryCondition("SELECT [FAMFile].[ID] FROM [FAMFile] " +
                    "   INNER JOIN [FileActionStatus] ON [FileID] = [FAMFile].[ID] AND [ActionStatus] = 'F'");
                fileProcessingDb.ModifyActionStatusForSelection(fileSelector,
                    _LABDE_ACTION3, EActionStatus.kActionPending, "");

                // Updated statuses
                //            |  P  |  R  |  S  |  C  |  F 
                //   Action 1                     1,2           
                //   Action 2   1                  2
                //   Action 3  2,3                

                Assert.That(fileProcessingDb.GetStats(actionId3, false).NumDocuments == 2);
                Assert.That(fileProcessingDb.GetStats(actionId3, false).NumDocumentsPending == 2);
                Assert.That(fileProcessingDb.GetStats(actionId3, false).NumDocumentsFailed == 0);

                fileSelector.Reset();
                fileSelector.AddQueryCondition(
                    "SELECT [FAMFile].[ID] FROM [FAMFile] WHERE [ID] = 2 OR [ID] = 3");
                fileProcessingDb.ModifyActionStatusForSelection(fileSelector,
                    _LABDE_ACTION2, EActionStatus.kActionSkipped, "");

                // Updated statuses
                //            |  P  |  R  |  S  |  C  |  F 
                //   Action 1                     1,2           
                //   Action 2   1           2,3       
                //   Action 3  2,3      

                Assert.That(fileProcessingDb.GetStats(actionId1, false).NumDocuments == 2);
                Assert.That(fileProcessingDb.GetStats(actionId1, false).NumDocumentsComplete == 2);
                Assert.That(fileProcessingDb.GetStats(actionId2, false).NumDocuments == 3);
                Assert.That(fileProcessingDb.GetStats(actionId2, false).NumDocumentsPending == 1);
                Assert.That(fileProcessingDb.GetStats(actionId2, false).NumDocumentsSkipped == 2);
                Assert.That(fileProcessingDb.GetStats(actionId3, false).NumDocuments == 2);
                Assert.That(fileProcessingDb.GetStats(actionId3, false).NumDocumentsPending == 2);
            }
            finally
            {
                _testFiles.RemoveFile(_LABDE_TEST_FILE1);
                _testFiles.RemoveFile(_LABDE_TEST_FILE2);
                _testFiles.RemoveFile(_LABDE_TEST_FILE3);
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        /// <summary>
        /// Tests the ModifyActionStatusForSelection method in a database with workflows
        /// </summary>
        [Test, Category("Automated")]
        public static void ModifyActionStatusForSelectionWithWorkflows()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestFAMFileProcessing>();
            _testDbManager = new FAMTestDBManager<TestFAMFileProcessing>();

            string testDbName = "Test_ModifyActionStatusForSelectionWithWorkflows";

            try
            {
                string testFileName1 = _testFiles.GetFile(_LABDE_TEST_FILE1);
                string testFileName2 = _testFiles.GetFile(_LABDE_TEST_FILE2);
                string testFileName3 = _testFiles.GetFile(_LABDE_TEST_FILE3);
                FileProcessingDB fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                // Initial statuses by File ID
                //            |  P  |  R  |  S  |  C  |  F 
                // Workflow 1 ----------------------------
                //   Action 1   1                  2
                //   Action 2   2
                // Workflow 2 ----------------------------
                //   Action 1                     1,2           
                //   Action 2   1                  2
                //   Action 3   3                        2

                int workflowID1 = fileProcessingDb.AddWorkflow(
                    "Workflow1", EWorkflowType.kUndefined, _LABDE_ACTION1, _LABDE_ACTION2);

                fileProcessingDb.AddFile(testFileName1, _LABDE_ACTION1, workflowID1, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionPending, false, out bool alreadyExists, out EActionStatus previousStatus);
                fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION1, workflowID1, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionCompleted, false, out alreadyExists, out previousStatus);
                fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION2, workflowID1, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);

                int workflowID2 = fileProcessingDb.AddWorkflow(
                    "Workflow2", EWorkflowType.kUndefined, _LABDE_ACTION1, _LABDE_ACTION2, _LABDE_ACTION3);

                fileProcessingDb.AddFile(testFileName1, _LABDE_ACTION1, workflowID2, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionCompleted, false, out alreadyExists, out previousStatus);
                fileProcessingDb.AddFile(testFileName1, _LABDE_ACTION2, workflowID2, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);
                fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION1, workflowID2, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionCompleted, false, out alreadyExists, out previousStatus);
                fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION2, workflowID2, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionCompleted, false, out alreadyExists, out previousStatus);
                fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION3, workflowID2, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionFailed, false, out alreadyExists, out previousStatus);
                fileProcessingDb.AddFile(testFileName3, _LABDE_ACTION3, workflowID2, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsPending == 1);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsComplete == 3);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION2, false).NumDocumentsPending == 2);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION2, false).NumDocumentsComplete == 1);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION3, false).NumDocumentsPending == 1);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION3, false).NumDocumentsFailed == 1);

                fileProcessingDb.ActiveWorkflow = "Workflow1";
                int actionId1 = fileProcessingDb.GetActionID(_LABDE_ACTION1);
                int actionId2 = fileProcessingDb.GetActionID(_LABDE_ACTION2);
                Assert.That(fileProcessingDb.GetFileCount(false) == 2);
                Assert.That(fileProcessingDb.GetStats(actionId1, false).NumDocuments == 2);

                var fileSelector = new FAMFileSelector();
                fileSelector.AddQueryCondition("SELECT [FAMFile].[ID] FROM [FAMFile]");
                fileProcessingDb.ModifyActionStatusForSelection(fileSelector,
                    _LABDE_ACTION1, EActionStatus.kActionUnattempted, "");

                // Statuses after ModifyActionStatusForSelection in Workflow 1
                //            |  P  |  R  |  S  |  C  |  F 
                // Workflow 1 ----------------------------   
                //   Action 1 
                //   Action 2   2
                // Workflow 2 ----------------------------   
                //   Action 1                     1,2           
                //   Action 2   1                  2
                //   Action 3   3                        2

                Assert.That(fileProcessingDb.GetFileCount(false) == 2);
                Assert.That(fileProcessingDb.GetStats(actionId1, false).NumDocuments == 0);
                Assert.That(fileProcessingDb.GetStats(actionId1, false).NumDocumentsPending == 0);
                Assert.That(fileProcessingDb.GetStats(actionId1, false).NumDocumentsComplete == 0);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsPending == 0);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsComplete == 2);

                fileProcessingDb.ActiveWorkflow = "Workflow2";
                actionId1 = fileProcessingDb.GetActionID(_LABDE_ACTION1);
                actionId2 = fileProcessingDb.GetActionID(_LABDE_ACTION2);
                int actionId3 = fileProcessingDb.GetActionID(_LABDE_ACTION3);

                Assert.That(fileProcessingDb.GetStats(actionId1, false).NumDocuments == 2);
                Assert.That(fileProcessingDb.GetStats(actionId1, false).NumDocumentsPending == 0);
                Assert.That(fileProcessingDb.GetStats(actionId1, false).NumDocumentsComplete == 2);
                Assert.That(fileProcessingDb.GetStats(actionId2, false).NumDocuments == 2);
                Assert.That(fileProcessingDb.GetStats(actionId2, false).NumDocumentsPending == 1);
                Assert.That(fileProcessingDb.GetStats(actionId2, false).NumDocumentsComplete == 1);
                Assert.That(fileProcessingDb.GetStats(actionId3, false).NumDocuments == 2);
                Assert.That(fileProcessingDb.GetStats(actionId3, false).NumDocumentsPending == 1);
                Assert.That(fileProcessingDb.GetStats(actionId3, false).NumDocumentsFailed == 1);

                Assert.That(fileProcessingDb.GetFileCount(false) == 3);

                fileSelector.Reset();
                fileSelector.AddQueryCondition("SELECT [FAMFile].[ID] FROM [FAMFile] " +
                    "   INNER JOIN [FileActionStatus] ON [FileID] = [FAMFile].[ID] AND [ActionStatus] = 'F'");
                fileProcessingDb.ModifyActionStatusForSelection(fileSelector,
                    _LABDE_ACTION3, EActionStatus.kActionPending, "");

                // Statuses after ModifyActionStatusForSelection in Workflow 2
                //            |  P  |  R  |  S  |  C  |  F 
                // Workflow 1 ----------------------------  
                //   Action 1 
                //   Action 2   2
                // Workflow 2 ---------------------------- 
                //   Action 1                     1,2           
                //   Action 2   1                  2
                //   Action 3  2,3                

                Assert.That(fileProcessingDb.GetStats(actionId3, false).NumDocuments == 2);
                Assert.That(fileProcessingDb.GetStats(actionId3, false).NumDocumentsPending == 2);
                Assert.That(fileProcessingDb.GetStats(actionId3, false).NumDocumentsFailed == 0);

                fileProcessingDb.ActiveWorkflow = "";

                fileSelector.Reset();
                fileSelector.AddQueryCondition("SELECT [FAMFile].[ID] FROM [FAMFile] WHERE [ID] = 2 OR [ID] = 3");
                fileProcessingDb.ModifyActionStatusForSelection(fileSelector,
                    _LABDE_ACTION2, EActionStatus.kActionSkipped, "");

                // Statuses after ModifyActionStatusForSelection for all workflows
                //            |  P  |  R  |  S  |  C  |  F 
                // Workflow 1 ----------------------------  
                //   Action 1 
                //   Action 2                2
                // Workflow 2 ----------------------------
                //   Action 1                     1,2           
                //   Action 2   1           2,3       
                //   Action 3  2,3      

                Assert.That(fileProcessingDb.GetFileCount(false) == 3);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocuments == 2);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsComplete == 2);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION2, false).NumDocuments == 4);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION2, false).NumDocumentsPending == 1);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION2, false).NumDocumentsSkipped == 3);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION3, false).NumDocuments == 2);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION3, false).NumDocumentsPending == 2);

                fileProcessingDb.ActiveWorkflow = "Workflow1";
                actionId1 = fileProcessingDb.GetActionID(_LABDE_ACTION1);
                actionId2 = fileProcessingDb.GetActionID(_LABDE_ACTION2);
                Assert.That(fileProcessingDb.GetFileCount(false) == 2);
                Assert.That(fileProcessingDb.GetStats(actionId1, false).NumDocuments == 0);
                Assert.That(fileProcessingDb.GetStats(actionId2, false).NumDocuments == 1);
                Assert.That(fileProcessingDb.GetStats(actionId2, false).NumDocumentsSkipped == 1);

                fileProcessingDb.ActiveWorkflow = "Workflow2";
                actionId1 = fileProcessingDb.GetActionID(_LABDE_ACTION1);
                actionId2 = fileProcessingDb.GetActionID(_LABDE_ACTION2);
                Assert.That(fileProcessingDb.GetFileCount(false) == 3);
                Assert.That(fileProcessingDb.GetStats(actionId1, false).NumDocuments == 2);
                Assert.That(fileProcessingDb.GetStats(actionId1, false).NumDocumentsComplete == 2);
                Assert.That(fileProcessingDb.GetStats(actionId2, false).NumDocuments == 3);
                Assert.That(fileProcessingDb.GetStats(actionId2, false).NumDocumentsPending == 1);
                Assert.That(fileProcessingDb.GetStats(actionId2, false).NumDocumentsSkipped == 2);
                Assert.That(fileProcessingDb.GetStats(actionId3, false).NumDocuments == 2);
                Assert.That(fileProcessingDb.GetStats(actionId3, false).NumDocumentsPending == 2);
            }
            finally
            {
                _testFiles.RemoveFile(_LABDE_TEST_FILE1);
                _testFiles.RemoveFile(_LABDE_TEST_FILE2);
                _testFiles.RemoveFile(_LABDE_TEST_FILE3);
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        /// <summary>
        /// Tests the SetStatusForAllFiles method in a database without workflows
        /// </summary>
        [Test, Category("Automated")]
        public static void SetStatusForAllFiles()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestFAMFileProcessing>();
            _testDbManager = new FAMTestDBManager<TestFAMFileProcessing>();

            string testDbName = "Test_SetStatusForAllFiles";

            try
            {
                string testFileName1 = _testFiles.GetFile(_LABDE_TEST_FILE1);
                string testFileName2 = _testFiles.GetFile(_LABDE_TEST_FILE2);
                string testFileName3 = _testFiles.GetFile(_LABDE_TEST_FILE3);
                FileProcessingDB fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                int actionId1 = fileProcessingDb.GetActionID(_LABDE_ACTION1);
                int actionId2 = fileProcessingDb.GetActionID(_LABDE_ACTION2);
                int actionId3 = fileProcessingDb.GetActionID(_LABDE_ACTION3);

                // Initial statuses by File ID
                //            |  P  |  R  |  S  |  C  |  F 
                //   Action 1                     1,2           
                //   Action 2   1                  2
                //   Action 3   3                        2

                fileProcessingDb.AddFile(testFileName1, _LABDE_ACTION1, -1, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionCompleted, false, out bool alreadyExists, out EActionStatus previousStatus);
                fileProcessingDb.AddFile(testFileName1, _LABDE_ACTION2, -1, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);
                fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION1, -1, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionCompleted, false, out alreadyExists, out previousStatus);
                fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION2, -1, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionCompleted, false, out alreadyExists, out previousStatus);
                fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION3, -1, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionFailed, false, out alreadyExists, out previousStatus);
                fileProcessingDb.AddFile(testFileName3, _LABDE_ACTION3, -1, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);

                Assert.That(fileProcessingDb.GetStats(actionId1, false).NumDocuments == 2);
                Assert.That(fileProcessingDb.GetStats(actionId1, false).NumDocumentsComplete == 2);
                Assert.That(fileProcessingDb.GetStats(actionId2, false).NumDocuments == 2);
                Assert.That(fileProcessingDb.GetStats(actionId2, false).NumDocumentsPending == 1);
                Assert.That(fileProcessingDb.GetStats(actionId2, false).NumDocumentsComplete == 1);
                Assert.That(fileProcessingDb.GetStats(actionId3, false).NumDocuments == 2);
                Assert.That(fileProcessingDb.GetStats(actionId3, false).NumDocumentsPending == 1);
                Assert.That(fileProcessingDb.GetStats(actionId3, false).NumDocumentsFailed == 1);

                fileProcessingDb.SetStatusForAllFiles(_LABDE_ACTION2, EActionStatus.kActionPending);

                // Updated statuses
                //            |  P  |  R  |  S  |  C  |  F 
                //   Action 1                     1,2           
                //   Action 2  1,2,3
                //   Action 3   3                        2

                Assert.That(fileProcessingDb.GetStats(actionId2, false).NumDocuments == 3);
                Assert.That(fileProcessingDb.GetStats(actionId2, false).NumDocumentsPending == 3);

                fileProcessingDb.SetStatusForAllFiles(_LABDE_ACTION1, EActionStatus.kActionSkipped);

                // Updated statuses
                //            |  P  |  R  |  S  |  C  |  F 
                //   Action 1              1,2,3
                //   Action 2  1,2,3
                //   Action 3   3         

                Assert.That(fileProcessingDb.GetStats(actionId1, false).NumDocuments == 3);
                Assert.That(fileProcessingDb.GetStats(actionId1, false).NumDocumentsSkipped == 3);
            }
            finally
            {
                _testFiles.RemoveFile(_LABDE_TEST_FILE1);
                _testFiles.RemoveFile(_LABDE_TEST_FILE2);
                _testFiles.RemoveFile(_LABDE_TEST_FILE3);
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        /// <summary>
        /// Tests the SetStatusForAllFiles method in a database with workflows
        /// </summary>
        [Test, Category("Automated")]
        public static void SetStatusForAllFilesWithWorkflows()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestFAMFileProcessing>();
            _testDbManager = new FAMTestDBManager<TestFAMFileProcessing>();

            string testDbName = "Test_SetStatusForAllFilesWithWorkflows";

            try
            {
                string testFileName1 = _testFiles.GetFile(_LABDE_TEST_FILE1);
                string testFileName2 = _testFiles.GetFile(_LABDE_TEST_FILE2);
                string testFileName3 = _testFiles.GetFile(_LABDE_TEST_FILE3);
                FileProcessingDB fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                // Initial statuses by File ID
                //            |  P  |  R  |  S  |  C  |  F 
                // Workflow 1 ----------------------------
                //   Action 1   1                  2
                //   Action 2   2
                // Workflow 2 ----------------------------
                //   Action 1                     1,2           
                //   Action 2   1                  2
                //   Action 3   3                        2

                int workflowID1 = fileProcessingDb.AddWorkflow(
                    "Workflow1", EWorkflowType.kUndefined, _LABDE_ACTION1, _LABDE_ACTION2);

                fileProcessingDb.AddFile(testFileName1, _LABDE_ACTION1, workflowID1, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionPending, false, out bool alreadyExists, out EActionStatus previousStatus);
                fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION1, workflowID1, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionCompleted, false, out alreadyExists, out previousStatus);
                fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION2, workflowID1, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);

                int workflowID2 = fileProcessingDb.AddWorkflow(
                    "Workflow2", EWorkflowType.kUndefined, _LABDE_ACTION1, _LABDE_ACTION2, _LABDE_ACTION3);

                fileProcessingDb.AddFile(testFileName1, _LABDE_ACTION1, workflowID2, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionCompleted, false, out alreadyExists, out previousStatus);
                fileProcessingDb.AddFile(testFileName1, _LABDE_ACTION2, workflowID2, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);
                fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION1, workflowID2, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionCompleted, false, out alreadyExists, out previousStatus);
                fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION2, workflowID2, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionCompleted, false, out alreadyExists, out previousStatus);
                fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION3, workflowID2, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionFailed, false, out alreadyExists, out previousStatus);
                fileProcessingDb.AddFile(testFileName3, _LABDE_ACTION3, workflowID2, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsPending == 1);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsComplete == 3);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION2, false).NumDocumentsPending == 2);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION2, false).NumDocumentsComplete == 1);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION3, false).NumDocumentsPending == 1);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION3, false).NumDocumentsFailed == 1);

                fileProcessingDb.ActiveWorkflow = "Workflow1";
                int actionId1 = fileProcessingDb.GetActionID(_LABDE_ACTION1);
                int actionId2 = fileProcessingDb.GetActionID(_LABDE_ACTION2);
                Assert.That(fileProcessingDb.GetFileCount(false) == 2);
                Assert.That(fileProcessingDb.GetStats(actionId1, false).NumDocuments == 2);

                fileProcessingDb.SetStatusForAllFiles(_LABDE_ACTION1, EActionStatus.kActionUnattempted);

                // Statuses after SetStatusForAllFiles in Workflow 1
                //            |  P  |  R  |  S  |  C  |  F 
                // Workflow 1 ----------------------------   
                //   Action 1 
                //   Action 2   2
                // Workflow 2 ----------------------------   
                //   Action 1                     1,2           
                //   Action 2   1                  2
                //   Action 3   3                        2

                Assert.That(fileProcessingDb.GetFileCount(false) == 2);
                Assert.That(fileProcessingDb.GetStats(actionId1, false).NumDocuments == 0);
                Assert.That(fileProcessingDb.GetStats(actionId1, false).NumDocumentsPending == 0);
                Assert.That(fileProcessingDb.GetStats(actionId1, false).NumDocumentsComplete == 0);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsPending == 0);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsComplete == 2);

                fileProcessingDb.ActiveWorkflow = "Workflow2";
                actionId1 = fileProcessingDb.GetActionID(_LABDE_ACTION1);
                actionId2 = fileProcessingDb.GetActionID(_LABDE_ACTION2);
                int actionId3 = fileProcessingDb.GetActionID(_LABDE_ACTION3);

                Assert.That(fileProcessingDb.GetStats(actionId1, false).NumDocuments == 2);
                Assert.That(fileProcessingDb.GetStats(actionId1, false).NumDocumentsPending == 0);
                Assert.That(fileProcessingDb.GetStats(actionId1, false).NumDocumentsComplete == 2);
                Assert.That(fileProcessingDb.GetStats(actionId2, false).NumDocuments == 2);
                Assert.That(fileProcessingDb.GetStats(actionId2, false).NumDocumentsPending == 1);
                Assert.That(fileProcessingDb.GetStats(actionId2, false).NumDocumentsComplete == 1);
                Assert.That(fileProcessingDb.GetStats(actionId3, false).NumDocuments == 2);
                Assert.That(fileProcessingDb.GetStats(actionId3, false).NumDocumentsPending == 1);
                Assert.That(fileProcessingDb.GetStats(actionId3, false).NumDocumentsFailed == 1);

                Assert.That(fileProcessingDb.GetFileCount(false) == 3);

                fileProcessingDb.SetStatusForAllFiles(_LABDE_ACTION2, EActionStatus.kActionPending);

                // Statuses after SetStatusForAllFiles in Workflow 2
                //            |  P  |  R  |  S  |  C  |  F 
                // Workflow 1 ----------------------------   
                //   Action 1 
                //   Action 2   2
                // Workflow 2 ----------------------------   
                //   Action 1                     1,2           
                //   Action 2  1,2,3
                //   Action 3   3                        2

                Assert.That(fileProcessingDb.GetStats(actionId2, false).NumDocuments == 3);
                Assert.That(fileProcessingDb.GetStats(actionId2, false).NumDocumentsPending == 3);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION2, false).NumDocuments == 4);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION2, false).NumDocumentsPending == 4);

                fileProcessingDb.ActiveWorkflow = "";

                fileProcessingDb.SetStatusForAllFiles(_LABDE_ACTION1, EActionStatus.kActionSkipped);

                // Statuses after SetStatusForAllFiles in all workflows
                //            |  P  |  R  |  S  |  C  |  F 
                // Workflow 1 ----------------------------
                //   Action 1               1,2
                //   Action 2   2
                // Workflow 2 ----------------------------   
                //   Action 1              1,2,3
                //   Action 2  1,2,3
                //   Action 3   3         

                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocuments == 5);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsSkipped == 5);

                fileProcessingDb.ActiveWorkflow = "Workflow1";
                actionId1 = fileProcessingDb.GetActionID(_LABDE_ACTION1);
                Assert.That(fileProcessingDb.GetFileCount(false) == 2);
                Assert.That(fileProcessingDb.GetStats(actionId1, false).NumDocuments == 2);
                Assert.That(fileProcessingDb.GetStats(actionId1, false).NumDocumentsSkipped == 2);

                fileProcessingDb.ActiveWorkflow = "Workflow2";
                actionId1 = fileProcessingDb.GetActionID(_LABDE_ACTION1);
                Assert.That(fileProcessingDb.GetFileCount(false) == 3);
                Assert.That(fileProcessingDb.GetStats(actionId1, false).NumDocuments == 3);
                Assert.That(fileProcessingDb.GetStats(actionId1, false).NumDocumentsSkipped == 3);
            }
            finally
            {
                _testFiles.RemoveFile(_LABDE_TEST_FILE1);
                _testFiles.RemoveFile(_LABDE_TEST_FILE2);
                _testFiles.RemoveFile(_LABDE_TEST_FILE3);
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        #endregion Test Methods
    }
}
