using Extract.Database;
using Extract.SqlDatabase;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using UCLID_FILEPROCESSORSLib;

using static System.FormattableString;

namespace Extract.FileActionManager.Database.Test
{
    public enum TestUser
    {
        NoUser = 0,
        CurrentUser = 1,
        AnotherUser = 2
    }

    /// <summary>
    /// Testing class for methods used for file processing in IFileProcessingDB.
    /// </summary>
    [Category("TestFAMFileProcessing")]
    [TestFixture]
    public class TestFAMFileProcessing
    {
        #region Constants

        static readonly int _CURRENT_WORKFLOW = -1;

        static readonly string _LABDE_EMPTY_DB = "Resources.Demo_LabDE_Empty";
        static readonly string _LABDE_TEST_FILE1 = "Resources.TestImage001.tif";
        static readonly string _LABDE_TEST_FILE2 = "Resources.TestImage002.tif";
        static readonly string _LABDE_TEST_FILE3 = "Resources.TestImage003.tif";
        static readonly string _LABDE_TEST_FILE4 = "Resources.TestImage004.tif";
        static readonly string _LABDE_TEST_FILE5 = "Resources.TestImage005.tif";
        static readonly string _LABDE_TEST_FILE6 = "Resources.TestImage006.tif";
        static readonly string _LABDE_TEST_FILE7 = "Resources.TestImage007.tif";
        static readonly string _LABDE_TEST_FILE8 = "Resources.TestImage008.tif";

        static readonly string _ACTION1 = "Action1";
        static readonly string _ACTION2 = "Action2";
        static readonly string _ACTION3 = "Action3";

        static readonly string _LABDE_ACTION1 = "A01_ExtractData";
        static readonly string _LABDE_ACTION2 = "A02_Verify";
        static readonly string _LABDE_ACTION3 = "A03_QA";
        static readonly string _LABDE_ACTION4 = "A04_SendToEMR";
        static readonly string _LABDE_ACTION5 = "Z_AdminAction";

        static readonly string _CLEANUP_ACTION = "Cleanup";

        static readonly string _ALL_WORKFLOWS = "<All workflows>";

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
        /// Tests updating the file size.
        /// </summary>
        [Test, Category("Automated")]
        public static void TestUpdatingFileSize()
        {
            string testDbName = "TestUpdatingFileSize";
            int newFileSize = 100;
            try
            {
                string testFileName = _testFiles.GetFile(_LABDE_TEST_FILE1);
                var fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                var fileRecord = fileProcessingDb.AddFile(
                    testFileName, _LABDE_ACTION1, _CURRENT_WORKFLOW, EFilePriority.kPriorityNormal, false, false,
                    EActionStatus.kActionPending, true, out var _, out EActionStatus t2);

                fileProcessingDb.SetFileInformationForFile(fileRecord.FileID, newFileSize, -1);
                var previousPageCount = fileRecord.Pages;

                var updatedFileRecord = fileProcessingDb.GetFileRecord(testFileName, _LABDE_ACTION1);
                Assert.AreEqual(newFileSize, updatedFileRecord.FileSize);
                Assert.AreEqual(previousPageCount, updatedFileRecord.Pages);
            }
            finally
            {
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        /// <summary>
        /// Tests updating the file size.
        /// </summary>
        [Test, Category("Automated")]
        public static void TestUpdatingPageCount()
        {
            string testDbName = "TestUpdatingPageCount";
            int newPageCount = 10000;
            try
            {
                string testFileName = _testFiles.GetFile(_LABDE_TEST_FILE1);
                var fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                var fileRecord = fileProcessingDb.AddFile(
                    testFileName, _LABDE_ACTION1, _CURRENT_WORKFLOW, EFilePriority.kPriorityNormal, false, false,
                    EActionStatus.kActionPending, true, out var _, out EActionStatus t2);

                fileProcessingDb.SetFileInformationForFile(fileRecord.FileID, -1, newPageCount);
                var previousFileSize = fileRecord.FileSize;

                var updatedFileRecord = fileProcessingDb.GetFileRecord(testFileName, _LABDE_ACTION1);
                Assert.AreEqual(newPageCount, updatedFileRecord.Pages);
                Assert.AreEqual(previousFileSize, updatedFileRecord.FileSize);
            }
            finally
            {
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        /// <summary>
        /// Tests updating the file size and page count.
        /// </summary>
        [Test, Category("Automated")]
        public static void TestUpdatingPageCountAndFileSize()
        {
            string testDbName = "TestUpdatingPageCountAndFileSize";
            int newPageCount = 10000;
            int newFileSize = 1000000;
            try
            {
                string testFileName = _testFiles.GetFile(_LABDE_TEST_FILE1);
                var fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                var fileRecord = fileProcessingDb.AddFile(
                    testFileName, _LABDE_ACTION1, _CURRENT_WORKFLOW, EFilePriority.kPriorityNormal, false, false,
                    EActionStatus.kActionPending, true, out var _, out EActionStatus t2);

                fileProcessingDb.SetFileInformationForFile(fileRecord.FileID, newFileSize, newPageCount);

                var updatedFileRecord = fileProcessingDb.GetFileRecord(testFileName, _LABDE_ACTION1);
                Assert.AreEqual(newPageCount, updatedFileRecord.Pages);
                Assert.AreEqual(newFileSize, updatedFileRecord.FileSize);
            }
            finally
            {
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        /// <summary>
        /// Tests adding, renaming and deleting actions both in an out of a workflow.
        /// </summary>
        [Test, Category("Automated")]
        public static void AddRemoveFiles()
        {
            string testDbName = "Test_AddRemoveFiles";

            try
            {
                string testFileName = _testFiles.GetFile(_LABDE_TEST_FILE1);
                var fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                int actionId = fileProcessingDb.GetActionID(_LABDE_ACTION1);

                fileProcessingDb.RecordFAMSessionStart("Test.fps", _LABDE_ACTION1, true, false);

                var fileRecord = fileProcessingDb.AddFile(
                    testFileName, _LABDE_ACTION1, _CURRENT_WORKFLOW, EFilePriority.kPriorityNormal, false, false,
                    EActionStatus.kActionPending, true, out bool t1, out EActionStatus t2);
                int fileId = fileRecord.FileID;

                fileProcessingDb.SetFileStatusToPending(fileId, _LABDE_ACTION1, false);

                Assert.That(fileProcessingDb.GetStats(actionId, false).NumDocumentsPending == 1);

                fileProcessingDb.RemoveFile(testFileName, _LABDE_ACTION1);

                Assert.That(fileProcessingDb.GetStats(actionId, false).NumDocumentsPending == 0);
            }
            finally
            {
                _testFiles.RemoveFile(_LABDE_TEST_FILE1);
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        /// <summary>
        /// Tests adding, renaming and deleting actions both in an out of a workflow.
        /// </summary>
        [Test, Category("Automated")]
        public static void AddRemoveFilesWithWorkflows()
        {
            string testDbName = "Test_AddRemoveFilesWithWorkflows";

            try
            {
                string testFileName = _testFiles.GetFile(_LABDE_TEST_FILE1);
                var fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                int workflowId1 = fileProcessingDb.AddWorkflow(
                    "Workflow1", EWorkflowType.kUndefined, _LABDE_ACTION1);
                int actionId = fileProcessingDb.GetActionID(_LABDE_ACTION1);

                Assert.IsTrue(fileProcessingDb.UsingWorkflows);

                fileProcessingDb.RecordFAMSessionStart("Test.fps", _LABDE_ACTION1, true, false);

                // Should not be able to queue a file without setting a workflow
                bool alreadyExists = false;
                EActionStatus previousStatus = EActionStatus.kActionUnattempted;
                Assert.Throws<COMException>(() => fileProcessingDb.AddFile(
                    testFileName, _LABDE_ACTION1, _CURRENT_WORKFLOW, EFilePriority.kPriorityNormal, false, false,
                    EActionStatus.kActionPending, true, out alreadyExists, out previousStatus));

                // Should not be able to change workflow during an active FAM Session.
                Assert.Throws<COMException>(() => fileProcessingDb.ActiveWorkflow = testDbName);

                fileProcessingDb.RecordFAMSessionStop();

                // Set active workflow which will correspond to a different action ID.
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                int actionIdWorkflow1 = fileProcessingDb.GetActionID(_LABDE_ACTION1);
                fileProcessingDb.RecordFAMSessionStart("Test.fps", _LABDE_ACTION1, true, true);

                // We should now be able to add a file now that we have a workflow set. (Even though
                // the workflow ID is not specified, active workflow should be assumed)
                var fileRecord = fileProcessingDb.AddFile(
                    testFileName, _LABDE_ACTION1, _CURRENT_WORKFLOW, EFilePriority.kPriorityNormal, false, false,
                    EActionStatus.kActionPending, true, out alreadyExists, out previousStatus);
                int fileId = fileRecord.FileID;

                fileProcessingDb.RecordFAMSessionStop();

                // A workflow is now being used.
                Assert.IsTrue(fileProcessingDb.UsingWorkflows);

                int workflowId2 = fileProcessingDb.AddWorkflow(
                    "Workflow2", EWorkflowType.kUndefined, _LABDE_ACTION1);
                fileProcessingDb.ActiveWorkflow = "Workflow2";
                int actionIdWorkflow2 = fileProcessingDb.GetActionID(_LABDE_ACTION1);

                Assert.IsFalse(fileProcessingDb.IsFileInWorkflow(fileId, -1));
                Assert.IsTrue(fileProcessingDb.IsFileInWorkflow(fileId, workflowId1));
                Assert.IsFalse(fileProcessingDb.IsFileInWorkflow(fileId, workflowId2));

                fileProcessingDb.RecordFAMSessionStart("Test.fps", _LABDE_ACTION1, false, true);

                fileProcessingDb.SetFileStatusToPending(fileId, _LABDE_ACTION1, false);

                fileProcessingDb.RecordFAMSessionStop();

                // File was added to workflow automatically
                Assert.IsTrue(fileProcessingDb.IsFileInWorkflow(fileId, workflowId2));

                // The actionID associated with the workflow should have a file queued, but not the "NULL" workflow action.
                Assert.AreEqual(0, fileProcessingDb.GetStats(actionId, false).NumDocumentsPending);
                Assert.AreEqual(1, fileProcessingDb.GetStats(actionIdWorkflow1, false).NumDocumentsPending);
                Assert.AreEqual(1, fileProcessingDb.GetStats(actionIdWorkflow2, false).NumDocumentsPending);
                Assert.AreEqual(2, fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsPending); // The file will be counted separately in each workflow

                // Test that after removing the file there are no longer any files pending on the workflow's action.
                fileProcessingDb.RemoveFile(testFileName, _LABDE_ACTION1);

                Assert.AreEqual(0, fileProcessingDb.GetStats(actionId, false).NumDocumentsPending);
                Assert.AreEqual(1, fileProcessingDb.GetStats(actionIdWorkflow1, false).NumDocumentsPending);
                Assert.AreEqual(0, fileProcessingDb.GetStats(actionIdWorkflow2, false).NumDocumentsPending);
                Assert.AreEqual(1, fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsPending);
            }
            finally
            {
                _testFiles.RemoveFile(_LABDE_TEST_FILE1);
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        /// <summary>
        /// Tests adding, renaming and deleting actions both in an out of a workflow.
        /// </summary>
        [Test, Category("Automated")]
        public static void AutoCreateActions()
        {
            string testDbName = "Test_AutoCreateActions";

            try
            {
                string testFileName = _testFiles.GetFile(_LABDE_TEST_FILE1);
                var fileProcessingDb = _testDbManager.GetNewDatabase(testDbName);

                fileProcessingDb.DefineNewAction(_ACTION1);
                fileProcessingDb.RecordFAMSessionStart("Test.fps", _ACTION1, true, false);

                bool alreadyExists = false;
                EActionStatus previousStatus = EActionStatus.kActionUnattempted;
                var fileRecord = fileProcessingDb.AddFile(testFileName, _ACTION1, _CURRENT_WORKFLOW,
                    EFilePriority.kPriorityNormal, true, false, EActionStatus.kActionPending, true,
                    out alreadyExists, out previousStatus);
                int fileId = fileRecord.FileID;

                // Without having yet set AutoCreateActions setting to true, AutoCreateActions should
                // fail and we should not be able to queue to an action that does not exist
                Assert.Throws<COMException>(() => fileProcessingDb.AutoCreateAction(_ACTION2));
                Assert.Throws<COMException>(() =>
                    fileProcessingDb.SetStatusForFile(fileId, _ACTION2, _CURRENT_WORKFLOW,
                    EActionStatus.kActionPending, true, false, out previousStatus));

                // Turn on auto-create actions.
                fileProcessingDb.RecordFAMSessionStop();
                fileProcessingDb.SetDBInfoSetting("AutoCreateActions", "1", true, true);
                fileProcessingDb.RecordFAMSessionStart("Test.fps", _ACTION1, true, false);

                fileProcessingDb.AutoCreateAction(_ACTION2);
                fileProcessingDb.SetStatusForFile(fileId, _ACTION2, _CURRENT_WORKFLOW,
                    EActionStatus.kActionPending, true, false, out previousStatus);

                int actionID = fileProcessingDb.GetActionID(_ACTION1);
                Assert.That(fileProcessingDb.GetStats(actionID, false).NumDocumentsPending == 1);

                actionID = fileProcessingDb.GetActionID(_ACTION2);
                Assert.That(fileProcessingDb.GetStats(actionID, false).NumDocumentsPending == 1);
            }
            finally
            {
                _testFiles.RemoveFile(_LABDE_TEST_FILE1);
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        /// <summary>
        /// Tests adding, renaming and deleting actions both in an out of a workflow.
        /// </summary>
        [Test, Category("Automated")]
        public static void AutoCreateActionsWithWorkflows()
        {
            string testDbName = "Test_AutoCreateActionsWithWorkflows";

            try
            {
                string testFileName = _testFiles.GetFile(_LABDE_TEST_FILE1);
                var fileProcessingDb = _testDbManager.GetNewDatabase(testDbName);

                fileProcessingDb.DefineNewAction(_ACTION1);
                fileProcessingDb.DefineNewAction(_ACTION2);
                int workflowID = fileProcessingDb.AddWorkflow(
                    "Workflow1", EWorkflowType.kUndefined, _ACTION1);
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                fileProcessingDb.RecordFAMSessionStart("Test.fps", _ACTION1, true, false);

                bool alreadyExists = false;
                EActionStatus previousStatus = EActionStatus.kActionUnattempted;
                var fileRecord = fileProcessingDb.AddFile(testFileName, _ACTION1, workflowID,
                    EFilePriority.kPriorityNormal, true, false, EActionStatus.kActionPending, true,
                    out alreadyExists, out previousStatus);
                int fileId = fileRecord.FileID;

                // Without having yet set AutoCreateActions setting to true, AutoCreateActions should
                // fail and we should not be able to queue to an action that does not exist in the
                // workflow (whether or not it exists as whole).
                Assert.Throws<COMException>(() => fileProcessingDb.AutoCreateAction(_ACTION2));
                Assert.Throws<COMException>(() =>
                    fileProcessingDb.SetStatusForFile(fileId, _ACTION2, workflowID, EActionStatus.kActionPending, true, false, out previousStatus));

                // Turn on auto-create actions.
                fileProcessingDb.RecordFAMSessionStop();
                fileProcessingDb.SetDBInfoSetting("AutoCreateActions", "1", true, true);
                fileProcessingDb.RecordFAMSessionStart("Test.fps", _ACTION1, true, false);

                // Test creating an using an action that already exists in a different workflow.
                fileProcessingDb.AutoCreateAction(_ACTION2);
                fileProcessingDb.SetStatusForFile(fileId, _ACTION2, workflowID, EActionStatus.kActionPending, true, false, out previousStatus);

                // Test creating an using an action that didn't already exist in a different workflow.
                fileProcessingDb.AutoCreateAction(_ACTION3);
                fileProcessingDb.SetStatusForFile(fileId, _ACTION3, workflowID, EActionStatus.kActionPending, true, false, out previousStatus);

                // Action 1
                int actionID = fileProcessingDb.GetActionID(_ACTION1);
                Assert.That(fileProcessingDb.GetStats(actionID, false).NumDocumentsPending == 1);

                actionID = fileProcessingDb.GetActionID(_ACTION2);
                Assert.That(fileProcessingDb.GetStats(actionID, false).NumDocumentsPending == 1);

                actionID = fileProcessingDb.GetActionID(_ACTION3);
                Assert.That(fileProcessingDb.GetStats(actionID, false).NumDocumentsPending == 1);
            }
            finally
            {
                _testFiles.RemoveFile(_LABDE_TEST_FILE1);
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        /// <summary>
        /// Tests the workflow statistics.
        /// </summary>
        [Test, Category("Automated")]
        public static void WorkflowStatistics()
        {
            string testDbName = "Test_WorkflowStatistics";

            try
            {
                string testFileName1 = _testFiles.GetFile(_LABDE_TEST_FILE1);
                string testFileName2 = _testFiles.GetFile(_LABDE_TEST_FILE2);
                string testFileName3 = _testFiles.GetFile(_LABDE_TEST_FILE3);
                var fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                int workflow1ID = fileProcessingDb.AddWorkflow(
                    "Workflow1", EWorkflowType.kUndefined, _LABDE_ACTION1, _LABDE_ACTION2);
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                int actionExtract1 = fileProcessingDb.GetActionID(_LABDE_ACTION1);
                int actionVerify1 = fileProcessingDb.GetActionID(_LABDE_ACTION2);

                int workflow2ID = fileProcessingDb.AddWorkflow(
                    "Workflow2", EWorkflowType.kUndefined, _LABDE_ACTION2, _LABDE_ACTION3);
                fileProcessingDb.ActiveWorkflow = "Workflow2";
                int actionVerify2 = fileProcessingDb.GetActionID(_LABDE_ACTION2);
                int actionQA2 = fileProcessingDb.GetActionID(_LABDE_ACTION3);

                fileProcessingDb.ActiveWorkflow = "Workflow1";
                fileProcessingDb.RecordFAMSessionStart("ExtractData.fps", _LABDE_ACTION1, true, false);

                // Workflow1: File 1 to Pending in _LABDE_ACTION1 and Complete in _LABDE_ACTION2
                bool alreadyExists = false;
                EActionStatus previousStatus = EActionStatus.kActionUnattempted;
                var fileRecord = fileProcessingDb.AddFile(testFileName1, _LABDE_ACTION1, workflow1ID,
                    EFilePriority.kPriorityNormal, true, false, EActionStatus.kActionPending, false,
                    out alreadyExists, out previousStatus);
                int fileId = fileRecord.FileID;
                fileProcessingDb.SetStatusForFile(fileId, _LABDE_ACTION2, workflow1ID, EActionStatus.kActionCompleted, false, false, out previousStatus);

                fileProcessingDb.RecordFAMSessionStop();
                fileProcessingDb.ActiveWorkflow = "Workflow2";
                fileProcessingDb.RecordFAMSessionStart("Test.fps", _LABDE_ACTION2, true, false);

                // Workflow2: 
                // File 1, 2 and 3 to Pending in _LABDE_ACTION2
                // File 3 to Skipped in _LABDE_ACTION3
                // File 3 to Failed in NewAction
                fileProcessingDb.SetStatusForFile(fileId, _LABDE_ACTION2, workflow2ID, EActionStatus.kActionPending, false, false, out previousStatus);
                fileRecord = fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION2, workflow2ID,
                    EFilePriority.kPriorityNormal, true, false, EActionStatus.kActionPending, false,
                    out alreadyExists, out previousStatus);
                fileRecord = fileProcessingDb.AddFile(testFileName3, _LABDE_ACTION2, workflow2ID,
                    EFilePriority.kPriorityNormal, true, false, EActionStatus.kActionPending, false,
                    out alreadyExists, out previousStatus);
                fileId = fileRecord.FileID;
                fileProcessingDb.SetStatusForFile(fileId, _LABDE_ACTION3, workflow2ID, EActionStatus.kActionSkipped, false, false, out previousStatus);

                fileProcessingDb.SetDBInfoSetting("AutoCreateActions", "1", true, true);
                int newAction2 = fileProcessingDb.AutoCreateAction("NewAction");
                fileProcessingDb.SetStatusForFile(fileId, "NewAction", workflow2ID, EActionStatus.kActionFailed, true, false, out previousStatus);

                Action checkStats = () =>
                {
                    // Check Workflow1 stats
                    Assert.That(fileProcessingDb.GetStats(actionExtract1, false).NumDocumentsPending == 1);
                    Assert.That(fileProcessingDb.GetStats(actionExtract1, false).NumPagesPending == 1);
                    Assert.That(fileProcessingDb.GetStats(actionExtract1, false).NumDocuments == 1);
                    Assert.That(fileProcessingDb.GetStats(actionExtract1, false).NumPages == 1);
                    Assert.That(fileProcessingDb.GetStats(actionVerify1, false).NumDocumentsPending == 0);
                    Assert.That(fileProcessingDb.GetStats(actionVerify1, false).NumPagesPending == 0);
                    Assert.That(fileProcessingDb.GetStats(actionVerify1, false).NumDocumentsComplete == 1);
                    Assert.That(fileProcessingDb.GetStats(actionVerify1, false).NumPagesComplete == 1);
                    Assert.That(fileProcessingDb.GetStats(actionVerify1, false).NumDocuments == 1);
                    Assert.That(fileProcessingDb.GetStats(actionVerify1, false).NumPages == 1);

                    // Check Workflow2 stats
                    Assert.That(fileProcessingDb.GetStats(actionVerify2, false).NumDocumentsPending == 3);
                    Assert.That(fileProcessingDb.GetStats(actionVerify2, false).NumPagesPending == 6);
                    Assert.That(fileProcessingDb.GetStats(actionVerify2, false).NumDocumentsFailed == 0);
                    Assert.That(fileProcessingDb.GetStats(actionVerify2, false).NumPagesFailed == 0);
                    Assert.That(fileProcessingDb.GetStats(actionVerify2, false).NumDocuments == 3);
                    Assert.That(fileProcessingDb.GetStats(actionVerify2, false).NumPages == 6);
                    Assert.That(fileProcessingDb.GetStats(actionQA2, false).NumDocumentsSkipped == 1);
                    Assert.That(fileProcessingDb.GetStats(actionQA2, false).NumPagesSkipped == 4);
                    Assert.That(fileProcessingDb.GetStats(actionQA2, false).NumDocuments == 1);
                    Assert.That(fileProcessingDb.GetStats(actionQA2, false).NumPages == 4);
                    Assert.That(fileProcessingDb.GetStats(newAction2, false).NumDocumentsFailed == 1);
                    Assert.That(fileProcessingDb.GetStats(newAction2, false).NumPagesFailed == 4);
                    Assert.That(fileProcessingDb.GetStats(newAction2, false).NumDocuments == 1);
                    Assert.That(fileProcessingDb.GetStats(newAction2, false).NumPages == 4);

                    // Check combined stats for both workflows.
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsPending == 1);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumPagesPending == 1);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocuments == 1);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumPages == 1);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION2, false).NumDocumentsPending == 3); // File 1 has been counted twice
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION2, false).NumPagesPending == 6);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION2, false).NumDocumentsComplete == 1);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION2, false).NumPagesComplete == 1);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION2, false).NumDocumentsFailed == 0);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION2, false).NumPagesFailed == 0);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION2, false).NumDocuments == 4); // File 1 has been counted twice
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION2, false).NumPages == 7);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION3, false).NumDocumentsSkipped == 1);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION3, false).NumPagesSkipped == 4);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION3, false).NumDocuments == 1);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION3, false).NumPages == 4);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows("NewAction", false).NumDocumentsFailed == 1);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows("NewAction", false).NumPagesFailed == 4);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows("NewAction", false).NumDocuments == 1);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows("NewAction", false).NumPages == 4);
                };

                // Check that stats are correct while still in an active workflow and FAMSession.
                checkStats();
                Assert.That(fileProcessingDb.GetFileCount(false) == 3);

                fileProcessingDb.RecordFAMSessionStop();

                // In Workflow1, it should only treat there as being 1 file.
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                Assert.That(fileProcessingDb.GetFileCount(false) == 1);

                fileProcessingDb.ActiveWorkflow = "";

                // Check again after stopping the session and clearing the active workflow.
                checkStats();
                Assert.That(fileProcessingDb.GetFileCount(false) == 3);
            }
            finally
            {
                _testFiles.RemoveFile(_LABDE_TEST_FILE1);
                _testFiles.RemoveFile(_LABDE_TEST_FILE2);
                _testFiles.RemoveFile(_LABDE_TEST_FILE3);
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        [Test, Category("Automated")]
        public static void GetFilesToProcessAndNotify()
        {
            string testDbName = "Test_GetFilesToProcessAndNotify";
            FileProcessingDB fileProcessingDb = null;

            try
            {
                string testFileName1 = _testFiles.GetFile(_LABDE_TEST_FILE1);
                string testFileName2 = _testFiles.GetFile(_LABDE_TEST_FILE2);
                string testFileName3 = _testFiles.GetFile(_LABDE_TEST_FILE3);
                fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                // Start processing
                int extractAction = fileProcessingDb.GetActionID(_LABDE_ACTION1);
                fileProcessingDb.RecordFAMSessionStart("Test.fps", _LABDE_ACTION1, true, false);
                fileProcessingDb.RegisterActiveFAM();

                // Queue file 1 and 2 to pending in _LABDE_ACTION1, file 3 to skipped.
                bool alreadyExists = false;
                EActionStatus previousStatus;
                var fileRecord = fileProcessingDb.AddFile(testFileName1, _LABDE_ACTION1,
                    _CURRENT_WORKFLOW, EFilePriority.kPriorityNormal, false, true,
                    EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);
                int fileId1 = fileRecord.FileID;
                fileRecord = fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION1,
                    _CURRENT_WORKFLOW, EFilePriority.kPriorityNormal, false, true,
                    EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);
                int fileId2 = fileRecord.FileID;
                fileRecord = fileProcessingDb.AddFile(testFileName3, _LABDE_ACTION1,
                    _CURRENT_WORKFLOW, EFilePriority.kPriorityNormal, false, true,
                    EActionStatus.kActionSkipped, false, out alreadyExists, out previousStatus);
                int fileId3 = fileRecord.FileID;

                // Check that we get just the pending files
                var files = fileProcessingDb.GetFilesToProcess(_LABDE_ACTION1, 10, false, "")
                    .ToIEnumerable<IFileRecord>()
                    .Select(record => record.FileID);
                Assert.That(files.SequenceEqual(new[] { fileId1, fileId2 }));

                // Simulate one file completing and the other being skipped.
                fileProcessingDb.NotifyFileProcessed(fileId1, _LABDE_ACTION1, _CURRENT_WORKFLOW, true);
                fileProcessingDb.NotifyFileSkipped(fileId2, _LABDE_ACTION1, _CURRENT_WORKFLOW, true);

                // Ensure GetFilesToProcess will not grab the skipped files in the same session.
                files = fileProcessingDb.GetFilesToProcess(_LABDE_ACTION1, 10, true, "")
                    .ToIEnumerable<IFileRecord>()
                    .Select(record => record.FileID);
                Assert.That(files.Count() == 0);

                // Start a new FAM session
                fileProcessingDb.UnregisterActiveFAM();
                fileProcessingDb.RecordFAMSessionStop();
                fileProcessingDb.RecordFAMSessionStart("Test.fps", _LABDE_ACTION1, true, false);
                fileProcessingDb.RegisterActiveFAM();

                // Now we should get the skipped files.
                files = fileProcessingDb.GetFilesToProcess(_LABDE_ACTION1, 10, true, "")
                    .ToIEnumerable<IFileRecord>()
                    .Select(record => record.FileID);
                Assert.That(files.SequenceEqual(new[] { fileId2, fileId3 }));

                var ee = new ExtractException("ELI42152", "Test");
                fileProcessingDb.NotifyFileFailed(fileId2, _LABDE_ACTION1, _CURRENT_WORKFLOW, ee.AsStringizedByteStream(), true);
                fileProcessingDb.NotifyFileSkipped(fileId3, _LABDE_ACTION1, _CURRENT_WORKFLOW, true);

                Assert.That(fileProcessingDb.GetStats(extractAction, false).NumDocumentsPending == 0);
                Assert.That(fileProcessingDb.GetStats(extractAction, false).NumDocumentsSkipped == 1);
                Assert.That(fileProcessingDb.GetStats(extractAction, false).NumDocumentsFailed == 1);
                Assert.That(fileProcessingDb.GetStats(extractAction, false).NumDocumentsComplete == 1);
            }
            finally
            {
                fileProcessingDb?.UnregisterActiveFAM();
                _testFiles.RemoveFile(_LABDE_TEST_FILE1);
                _testFiles.RemoveFile(_LABDE_TEST_FILE2);
                _testFiles.RemoveFile(_LABDE_TEST_FILE3);
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        [Test, Category("Automated")]
        public static void GetFilesToProcessAndNotifyWithWorkflows()
        {
            string testDbName = "Test_GetFilesToProcessAndNotifyWithWorkflows";
            FileProcessingDB fileProcessingDb = null;

            try
            {
                string testFileName1 = _testFiles.GetFile(_LABDE_TEST_FILE1);
                string testFileName2 = _testFiles.GetFile(_LABDE_TEST_FILE2);
                fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);
                fileProcessingDb.SetDBInfoSetting("EnableLoadBalancing", "0", true, false);

                int workflowID1 = fileProcessingDb.AddWorkflow(
                    "Workflow1", EWorkflowType.kUndefined, _LABDE_ACTION1, _LABDE_ACTION2);
                int workflowID2 = fileProcessingDb.AddWorkflow(
                    "Workflow2", EWorkflowType.kUndefined, _LABDE_ACTION1, _LABDE_ACTION2);

                // Start processing in Workflow 1
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                int extractActionId1 = fileProcessingDb.GetActionID(_LABDE_ACTION1);
                fileProcessingDb.RecordFAMSessionStart("Test.fps", _LABDE_ACTION1, true, false);
                fileProcessingDb.RegisterActiveFAM();

                // In Workflow 1, queue file 1 to pending in _LABDE_ACTION1, and check that
                // GetFilesToProcess grabs it.
                bool alreadyExists = false;
                EActionStatus previousStatus;
                var fileRecord = fileProcessingDb.AddFile(testFileName1, _LABDE_ACTION1,
                    workflowID1, EFilePriority.kPriorityNormal, false, true,
                    EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);
                int fileId1 = fileRecord.FileID;
                int fileToProcessId = fileProcessingDb.GetFilesToProcess(_LABDE_ACTION1, 10, false, "")
                    .ToIEnumerable<IFileRecord>()
                    .Single()
                    .FileID;
                Assert.That(fileId1 == fileToProcessId);

                // Set to pending for both actions in Workflow 1 for later tests.
                fileProcessingDb.SetFileStatusToPending(fileId1, _LABDE_ACTION1, false);
                fileProcessingDb.SetFileStatusToPending(fileId1, _LABDE_ACTION2, false);

                // Start processing in Workflow 2
                fileProcessingDb.UnregisterActiveFAM();
                fileProcessingDb.RecordFAMSessionStop();
                fileProcessingDb.ActiveWorkflow = "Workflow2";
                int extractActionId2 = fileProcessingDb.GetActionID(_LABDE_ACTION1);
                fileProcessingDb.RecordFAMSessionStart("Test.fps", _LABDE_ACTION2, true, false);
                fileProcessingDb.RegisterActiveFAM();

                // Check that though there is a file pending in Workflow1, GetFilesToProcess won't
                // grab it for workflow2.
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION2, false).NumDocumentsPending == 1);
                Assert.That(fileProcessingDb.GetFilesToProcess(_LABDE_ACTION2, 10, false, "").Size() == 0);

                // In Workflow2, queue file1 for _LABDE_ACTION2, stats now report 2 files pending
                // (really same file in both workflows), then confirm GetFilesToProcess grabs only the file for
                // Workflow2.
                fileProcessingDb.AddFile(testFileName1, _LABDE_ACTION2, workflowID2, EFilePriority.kPriorityNormal,
                    false, true, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION2, false).NumDocumentsPending == 2);
                fileToProcessId = fileProcessingDb.GetFilesToProcess(_LABDE_ACTION2, 10, false, "")
                    .ToIEnumerable<IFileRecord>()
                    .Single()
                    .FileID;
                Assert.That(fileId1 == fileToProcessId);
                fileProcessingDb.NotifyFileProcessed(fileId1, _LABDE_ACTION2, workflowID2, false);

                // In Workflow2, queue file 2 for _LABDE_ACTION1
                fileRecord = fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION1, workflowID2,
                    EFilePriority.kPriorityNormal, false, true,
                    EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);
                int fileId2 = fileRecord.FileID;

                // Start processing in _LABDE_ACTION1 for all workflows
                fileProcessingDb.UnregisterActiveFAM();
                fileProcessingDb.RecordFAMSessionStop();
                fileProcessingDb.ActiveWorkflow = "";
                fileProcessingDb.RecordFAMSessionStart("Test.fps", _LABDE_ACTION1, true, false);
                fileProcessingDb.RegisterActiveFAM();

                // Ensure files from both workflows are grabbed for processing.
                var fileIDs = fileProcessingDb.GetFilesToProcess(_LABDE_ACTION1, 10, false, "")
                    .ToIEnumerable<FileRecord>()
                    .Select(r => r.FileID)
                    .ToArray();
                Assert.That(fileIDs[0] == fileId1);
                Assert.That(fileIDs[1] == fileId2);

                // Simulate one file completing and the other failing. Ensure they move the correct statuses
                // in the correct workflows.
                fileProcessingDb.NotifyFileSkipped(fileId1, _LABDE_ACTION1, workflowID1, true);
                var ee = new ExtractException("ELI42098", "Test");
                fileProcessingDb.NotifyFileFailed(fileId2, _LABDE_ACTION1, workflowID2, ee.AsStringizedByteStream(), true);

                Assert.That(fileProcessingDb.GetStats(extractActionId1, false).NumDocumentsSkipped == 1);
                Assert.That(fileProcessingDb.GetStats(extractActionId1, false).NumDocumentsFailed == 0);
                Assert.That(fileProcessingDb.GetStats(extractActionId2, false).NumDocumentsSkipped == 0);
                Assert.That(fileProcessingDb.GetStats(extractActionId2, false).NumDocumentsFailed == 1);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsPending == 0);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsSkipped == 1);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsFailed == 1);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION2, false).NumDocumentsComplete == 1);
            }
            finally
            {
                fileProcessingDb?.UnregisterActiveFAM();
                _testFiles.RemoveFile(_LABDE_TEST_FILE1);
                _testFiles.RemoveFile(_LABDE_TEST_FILE2);
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        /// <summary>
        /// Tests GetWorkflowStatus
        /// </summary>
        [Test, Category("Automated")]
        public static void GetWorkflowStatus()
        {
            string testDbName = "Test_GetWorkflowStatus";
            FileProcessingDB fileProcessingDb = null;

            try
            {
                string testFileName1 = _testFiles.GetFile(_LABDE_TEST_FILE1);
                string testFileName2 = _testFiles.GetFile(_LABDE_TEST_FILE2);
                string testFileName3 = _testFiles.GetFile(_LABDE_TEST_FILE3);
                fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);
                fileProcessingDb.DefineNewAction(_LABDE_ACTION4);
                fileProcessingDb.DefineNewAction(_CLEANUP_ACTION);

                int workflowID = fileProcessingDb.AddWorkflow("Workflow1", EWorkflowType.kExtraction);

                var workflowActions = new Dictionary<string, bool>()
                    {
                        { _LABDE_ACTION1, true },
                        { _LABDE_ACTION2, true },
                        { _LABDE_ACTION3, true },
                        { _LABDE_ACTION4, true },
                        { _LABDE_ACTION5, false },
                        { _CLEANUP_ACTION, false },
                    };
                fileProcessingDb.SetWorkflowActions(workflowID,
                    workflowActions
                        .Select(entry =>
                        {
                            var actionInfo = new VariantVector();
                            actionInfo.PushBack(entry.Key);
                            actionInfo.PushBack(entry.Value);
                            return actionInfo;
                        })
                        .ToIUnknownVector());

                WorkflowDefinition workflow = fileProcessingDb.GetWorkflowDefinition(workflowID);
                workflow.StartAction = _LABDE_ACTION1;
                workflow.EndAction = _LABDE_ACTION4;
                workflow.PostWorkflowAction = _CLEANUP_ACTION;
                fileProcessingDb.SetWorkflowDefinition(workflow);

                fileProcessingDb.ActiveWorkflow = "Workflow1";
                fileProcessingDb.RecordFAMSessionStart("Test.fps", _LABDE_ACTION1, true, true);
                fileProcessingDb.RegisterActiveFAM();

                // File 1 pending in _LABDE_ACTION1
                // File 2 complete in _LABDE_ACTION1 and pending in _LABDE_ACTION2
                // File 3 failed in _LABDE_ACTION1
                bool alreadyExists = false;
                EActionStatus previousStatus;
                var fileRecord = fileProcessingDb.AddFile(testFileName1, _LABDE_ACTION1, workflowID,
                    EFilePriority.kPriorityNormal, false, true,
                    EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);
                int fileId1 = fileRecord.FileID;
                fileRecord = fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION1, workflowID,
                    EFilePriority.kPriorityNormal, false, true,
                    EActionStatus.kActionCompleted, false, out alreadyExists, out previousStatus);
                int fileId2 = fileRecord.FileID;
                fileProcessingDb.SetStatusForFile(fileId2, _LABDE_ACTION2, _CURRENT_WORKFLOW,
                    EActionStatus.kActionPending, false, false, out previousStatus);
                fileRecord = fileProcessingDb.AddFile(testFileName3, _LABDE_ACTION1, workflowID,
                    EFilePriority.kPriorityNormal, false, true,
                    EActionStatus.kActionFailed, false, out alreadyExists, out previousStatus);
                int fileId3 = fileRecord.FileID;

                Assert.That(fileProcessingDb.GetWorkflowStatus(fileId1) == EActionStatus.kActionProcessing);
                Assert.That(fileProcessingDb.GetWorkflowStatus(fileId2) == EActionStatus.kActionProcessing);
                Assert.That(fileProcessingDb.GetWorkflowStatus(fileId3) == EActionStatus.kActionFailed);
                fileProcessingDb.GetAggregateWorkflowStatus(
                    out int unattempted, out int processing, out int completed, out int failed);
                Assert.That(unattempted == 0);
                Assert.That(processing == 2);
                Assert.That(completed == 0);
                Assert.That(failed == 1);

                // File 1 complete in _LABDE_ACTION1 and skipped in _LABDE_ACTION2
                // File 2 complete in _LABDE_ACTION1 and _LABDE_ACTION2, processing in _LABDE_ACTION4
                // File 3 failed in _LABDE_ACTION1 and _LABDE_ACTION4
                fileProcessingDb.SetStatusForFile(fileId1, _LABDE_ACTION1, _CURRENT_WORKFLOW,
                    EActionStatus.kActionCompleted, false, false, out previousStatus);
                fileProcessingDb.SetStatusForFile(fileId1, _LABDE_ACTION2, _CURRENT_WORKFLOW,
                    EActionStatus.kActionSkipped, false, false, out previousStatus);
                fileProcessingDb.SetStatusForFile(fileId2, _LABDE_ACTION2, _CURRENT_WORKFLOW,
                   EActionStatus.kActionCompleted, false, false, out previousStatus);
                fileProcessingDb.SetStatusForFile(fileId2, _LABDE_ACTION4, _CURRENT_WORKFLOW,
                    EActionStatus.kActionProcessing, false, false, out previousStatus);
                fileProcessingDb.SetStatusForFile(fileId3, _LABDE_ACTION4, _CURRENT_WORKFLOW,
                    EActionStatus.kActionFailed, false, false, out previousStatus);

                Assert.That(fileProcessingDb.GetWorkflowStatus(fileId1) == EActionStatus.kActionUnattempted);
                Assert.That(fileProcessingDb.GetWorkflowStatus(fileId2) == EActionStatus.kActionProcessing);
                Assert.That(fileProcessingDb.GetWorkflowStatus(fileId3) == EActionStatus.kActionFailed);
                fileProcessingDb.GetAggregateWorkflowStatus(
                    out unattempted, out processing, out completed, out failed);
                Assert.That(unattempted == 1);
                Assert.That(processing == 1);
                Assert.That(completed == 0);
                Assert.That(failed == 1);

                // File 2 complete in _LABDE_ACTION4, pending in Cleanup.
                fileProcessingDb.SetStatusForFile(fileId2, _LABDE_ACTION4, _CURRENT_WORKFLOW,
                    EActionStatus.kActionCompleted, false, false, out previousStatus);
                fileProcessingDb.SetStatusForFile(fileId2, _CLEANUP_ACTION, _CURRENT_WORKFLOW,
                    EActionStatus.kActionPending, false, false, out previousStatus);

                Assert.That(fileProcessingDb.GetWorkflowStatus(fileId2) == EActionStatus.kActionCompleted);
                fileProcessingDb.GetAggregateWorkflowStatus(
                    out unattempted, out processing, out completed, out failed);
                Assert.That(unattempted == 1);
                Assert.That(processing == 0);
                Assert.That(completed == 1);
                Assert.That(failed == 1);
            }
            finally
            {
                fileProcessingDb?.UnregisterActiveFAM();
                _testFiles.RemoveFile(_LABDE_TEST_FILE1);
                _testFiles.RemoveFile(_LABDE_TEST_FILE2);
                _testFiles.RemoveFile(_LABDE_TEST_FILE3);
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        /// <summary>
        /// Tests that the FileProcessingManager processed properly within the context of a workflow.
        /// </summary>
        [Test, Category("Automated")]
        public static void FileProcessingManager()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestFAMFileProcessing>();
            _testDbManager = new FAMTestDBManager<TestFAMFileProcessing>();

            string testDbName = "Test_FileProcessingManager";

            try
            {
                string testFileName1 = _testFiles.GetFile(_LABDE_TEST_FILE1);
                string testFileName2 = _testFiles.GetFile(_LABDE_TEST_FILE2);
                string testFileName3 = _testFiles.GetFile(_LABDE_TEST_FILE3);
                FileProcessingDB fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                int extractAction = fileProcessingDb.GetActionID(_LABDE_ACTION1);
                int verifyAction = fileProcessingDb.GetActionID(_LABDE_ACTION2);

                // Queue and process a file
                bool alreadyExists = false;
                EActionStatus previousStatus;
                fileProcessingDb.AddFile(testFileName1, _LABDE_ACTION1, _CURRENT_WORKFLOW,
                    EFilePriority.kPriorityNormal, false, false, EActionStatus.kActionPending, false,
                    out alreadyExists, out previousStatus);

                var setStatusTaskConfig = new SetActionStatusFileProcessor();
                setStatusTaskConfig.ActionName = _LABDE_ACTION2;
                setStatusTaskConfig.ActionStatus = (int)EActionStatus.kActionPending;
                var setStatusTask = (IFileProcessingTask)setStatusTaskConfig;

                using (var famSession = new FAMProcessingSession(
                    fileProcessingDb, _LABDE_ACTION1, _ALL_WORKFLOWS, setStatusTask))
                {
                    famSession.WaitForProcessingToComplete();
                }

                // Ensure file has processed 
                Assert.That(fileProcessingDb.GetStats(extractAction, false).NumDocumentsPending == 0);
                Assert.That(fileProcessingDb.GetStats(extractAction, false).NumDocumentsComplete == 1);
                Assert.That(fileProcessingDb.GetStats(verifyAction, false).NumDocumentsPending == 1);

                // Queue another 2 more files, test that they can both be processed at once.
                fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION1, _CURRENT_WORKFLOW,
                    EFilePriority.kPriorityNormal, false, false, EActionStatus.kActionPending, false,
                    out alreadyExists, out previousStatus);
                fileProcessingDb.AddFile(testFileName3, _LABDE_ACTION1, _CURRENT_WORKFLOW,
                    EFilePriority.kPriorityNormal, false, false, EActionStatus.kActionPending, false,
                    out alreadyExists, out previousStatus);

                Assert.That(fileProcessingDb.GetStats(extractAction, false).NumDocumentsPending == 2);

                using (var famSession = new FAMProcessingSession(
                    fileProcessingDb, _LABDE_ACTION1, _ALL_WORKFLOWS, setStatusTask,
                    threadCount: 2, filesToGrab: 2))
                {
                    famSession.WaitForProcessingToComplete();
                }

                Assert.That(fileProcessingDb.GetStats(extractAction, false).NumDocumentsComplete == 3);
                Assert.That(fileProcessingDb.GetStats(verifyAction, false).NumDocumentsPending == 3);
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
        /// Tests that the FileProcessingManager processed properly within the context of a workflow.
        /// </summary>
        [Test, Category("Automated")]
        public static void FileProcessingManagerWithWorkflows()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestFAMFileProcessing>();
            _testDbManager = new FAMTestDBManager<TestFAMFileProcessing>();

            string testDbName = "Test_FileProcessingManagerWithWorkflows";

            try
            {
                string testFileName1 = _testFiles.GetFile(_LABDE_TEST_FILE1);
                string testFileName2 = _testFiles.GetFile(_LABDE_TEST_FILE2);
                FileProcessingDB fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                // Create 2 workflows
                int workflowID1 = fileProcessingDb.AddWorkflow(
                    "Workflow1", EWorkflowType.kUndefined, _LABDE_ACTION1, _LABDE_ACTION2);
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                int extractAction1 = fileProcessingDb.GetActionID(_LABDE_ACTION1);

                int workflowID2 = fileProcessingDb.AddWorkflow(
                    "Workflow2", EWorkflowType.kUndefined, _LABDE_ACTION1, _LABDE_ACTION2);
                fileProcessingDb.ActiveWorkflow = "Workflow2";
                int extractAction2 = fileProcessingDb.GetActionID(_LABDE_ACTION1);
                int verifyAction2 = fileProcessingDb.GetActionID(_LABDE_ACTION2);

                // Queue and process a file in workflow 1
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                bool alreadyExists = false;
                EActionStatus previousStatus;
                fileProcessingDb.AddFile(testFileName1, _LABDE_ACTION1, workflowID1, EFilePriority.kPriorityNormal,
                    false, false, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);

                var setStatusTaskConfig = new SetActionStatusFileProcessor();
                setStatusTaskConfig.ActionName = _LABDE_ACTION2;
                setStatusTaskConfig.ActionStatus = (int)EActionStatus.kActionPending;
                var setStatusTask = (IFileProcessingTask)setStatusTaskConfig;

                using (var famSession = new FAMProcessingSession(
                    fileProcessingDb, _LABDE_ACTION1, "Workflow1", setStatusTask))
                {
                    famSession.WaitForProcessingToComplete();
                }

                // Ensure file has processed only in the context of workflow 1 (including where it gets set to pending
                // in the next action)
                Assert.AreEqual(0, fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsPending);
                Assert.AreEqual(1, fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsComplete);
                Assert.AreEqual(1, fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION2, false).NumDocumentsPending);

                Assert.AreEqual(0, fileProcessingDb.GetStats(extractAction2, false).NumDocumentsComplete);
                Assert.AreEqual(0, fileProcessingDb.GetStats(verifyAction2, false).NumDocumentsPending);

                // Queue another file to workflow1, then the original file to workflow2, then test that both
                // processing in a session configured to run on all workflows
                fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION1, workflowID1, EFilePriority.kPriorityNormal,
                    false, false, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);

                fileProcessingDb.ActiveWorkflow = "Workflow2";
                fileProcessingDb.AddFile(testFileName1, _LABDE_ACTION1, workflowID2, EFilePriority.kPriorityNormal,
                    false, false, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);

                Assert.AreEqual(1, fileProcessingDb.GetStats(extractAction1, false).NumDocumentsPending);
                Assert.AreEqual(1, fileProcessingDb.GetStats(extractAction2, false).NumDocumentsPending);

                using (var famSession = new FAMProcessingSession(
                    fileProcessingDb, _LABDE_ACTION1, _ALL_WORKFLOWS, setStatusTask))
                {
                    famSession.WaitForProcessingToComplete();
                }

                Assert.AreEqual(0, fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsPending);
                Assert.AreEqual(3, fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsComplete);
                Assert.AreEqual(3, fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION2, false).NumDocumentsPending);

                fileProcessingDb.ActiveWorkflow = "";
                fileProcessingDb.SetStatusForAllFiles(_LABDE_ACTION1, EActionStatus.kActionPending);
                Assert.AreEqual(3, fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsPending);

                var sleepTaskConfig = new SleepTask();
                sleepTaskConfig.SleepTime = 1;
                sleepTaskConfig.TimeUnits = ESleepTimeUnitType.kSleepSeconds;
                var sleepTask = (IFileProcessingTask)sleepTaskConfig;

                // Ensure that if the same file is queued in multiple workflows, get files to process handles 
                // simultaneously (which would cause an error). Keep processing needs to be set to true because
                // the check for more files will not return anything while one instance of file 1 is processing
                // and the other is pending.
                using (var famSession = new FAMProcessingSession(
                    fileProcessingDb, _LABDE_ACTION1, _ALL_WORKFLOWS, sleepTask,
                    threadCount: 3, filesToGrab: 3, keepProcessing: true, docsToProcess: 3))
                {
                    System.Threading.Thread.Sleep(500);

                    // Initially, processing should grab only 1 of the two pending instances of file 1.
                    Assert.AreEqual(1, fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsPending);

                    famSession.WaitForProcessingToComplete();
                    // There seem to be a bug with WaitForProcessingToComplete that is causing it to
                    // return before processing is actually complete. Sleep as a work-around.
                    System.Threading.Thread.Sleep(1000);
                }

                Assert.AreEqual(0, fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsPending);
                Assert.AreEqual(3, fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsComplete);
            }
            finally
            {
                _testFiles.RemoveFile(_LABDE_TEST_FILE1);
                _testFiles.RemoveFile(_LABDE_TEST_FILE2);
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        [Test, Category("Automated")]
        public static void FilePriority()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestFAMFileProcessing>();
            _testDbManager = new FAMTestDBManager<TestFAMFileProcessing>();

            string testDbName = "Test_FilePriority";

            try
            {
                FileProcessingDB fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                int actionId = fileProcessingDb.GetActionID(_LABDE_ACTION1);

                // Queue files with varying priorities:
                // Order should be: 3, 4, 6, 1, 7, 5, 2
                (string fileName, string actionName, int workflowID, EActionStatus actionStatus, EFilePriority priority)[] testFiles =
                {
                    (_LABDE_TEST_FILE1, _LABDE_ACTION1, -1, EActionStatus.kActionPending, EFilePriority.kPriorityNormal),
                    (_LABDE_TEST_FILE2, _LABDE_ACTION1, -1, EActionStatus.kActionPending, EFilePriority.kPriorityLow),
                    (_LABDE_TEST_FILE3, _LABDE_ACTION1, -1, EActionStatus.kActionPending, EFilePriority.kPriorityHigh),
                    (_LABDE_TEST_FILE4, _LABDE_ACTION1, -1, EActionStatus.kActionPending, EFilePriority.kPriorityHigh),
                    (_LABDE_TEST_FILE5, _LABDE_ACTION1, -1, EActionStatus.kActionPending, EFilePriority.kPriorityBelowNormal),
                    (_LABDE_TEST_FILE6, _LABDE_ACTION1, -1, EActionStatus.kActionPending, EFilePriority.kPriorityAboveNormal),
                    (_LABDE_TEST_FILE7, _LABDE_ACTION1, -1, EActionStatus.kActionPending, EFilePriority.kPriorityNormal),
                };
                var fileIDs = fileProcessingDb.AddTestFiles(_testFiles, testFiles);

                var fileSelector = new FAMFileSelector();
                fileSelector.AddActionStatusCondition(fileProcessingDb, _LABDE_ACTION1, EActionStatus.kActionCompleted);

                var setStatusTaskConfig = new SetActionStatusFileProcessor();
                setStatusTaskConfig.ActionName = _LABDE_ACTION2;
                setStatusTaskConfig.ActionStatus = (int)EActionStatus.kActionPending;
                var setStatusTask = (IFileProcessingTask)setStatusTaskConfig;

                using (var famSession = new FAMProcessingSession(
                    fileProcessingDb, _LABDE_ACTION1, "", setStatusTask,
                    threadCount: 1, filesToGrab: 1, keepProcessing: false, docsToProcess: 1))
                {
                    famSession.WaitForProcessingToComplete();
                }

                Assert.AreEqual(fileIDs[3], fileSelector.GetResults(fileProcessingDb).Single());

                using (var famSession = new FAMProcessingSession(
                    fileProcessingDb, _LABDE_ACTION1, "", setStatusTask,
                    threadCount: 1, filesToGrab: 2, keepProcessing: false, docsToProcess: 2))
                {
                    famSession.WaitForProcessingToComplete();
                }

                Assert.AreEqual(
                    Invariant($"{fileIDs[3]},{fileIDs[4]},{fileIDs[6]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));

                using (var famSession = new FAMProcessingSession(
                    fileProcessingDb, _LABDE_ACTION1, "", setStatusTask,
                    threadCount: 2, filesToGrab: 25, keepProcessing: false, docsToProcess: 2))
                {
                    famSession.WaitForProcessingToComplete();
                }

                Assert.AreEqual(
                    Invariant($"{fileIDs[1]},{fileIDs[3]},{fileIDs[4]},{fileIDs[6]},{fileIDs[7]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));
                // Ensure extra files grabbed were properly returned to pending.
                Assert.AreEqual(2, fileProcessingDb.GetStats(actionId, false).NumDocumentsPending);

                using (var famSession = new FAMProcessingSession(
                    fileProcessingDb, _LABDE_ACTION1, "", setStatusTask,
                    threadCount: 1, filesToGrab: 25, keepProcessing: false, docsToProcess: 1))
                {
                    famSession.WaitForProcessingToComplete();
                }

                Assert.AreEqual(
                    Invariant($"{fileIDs[1]},{fileIDs[3]},{fileIDs[4]},{fileIDs[5]},{fileIDs[6]},{fileIDs[7]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));
            }
            finally
            {
                _testFiles.RemoveFile(_LABDE_TEST_FILE1);
                _testFiles.RemoveFile(_LABDE_TEST_FILE2);
                _testFiles.RemoveFile(_LABDE_TEST_FILE3);
                _testFiles.RemoveFile(_LABDE_TEST_FILE4);
                _testFiles.RemoveFile(_LABDE_TEST_FILE5);
                _testFiles.RemoveFile(_LABDE_TEST_FILE6);
                _testFiles.RemoveFile(_LABDE_TEST_FILE7);
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        [Test, Category("Automated")]
        public static void FilePriorityWithWorkflows()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestFAMFileProcessing>();
            _testDbManager = new FAMTestDBManager<TestFAMFileProcessing>();

            string testDbName = "Test_FilePriorityWithWorkflows";

            try
            {
                FileProcessingDB fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                // Create 2 workflows
                int workflowID1 = fileProcessingDb.AddWorkflow(
                    "Workflow1", EWorkflowType.kUndefined, _LABDE_ACTION1, _LABDE_ACTION2);
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                int extractAction1 = fileProcessingDb.GetActionID(_LABDE_ACTION1);

                int workflowID2 = fileProcessingDb.AddWorkflow(
                    "Workflow2", EWorkflowType.kUndefined, _LABDE_ACTION1, _LABDE_ACTION2);
                fileProcessingDb.ActiveWorkflow = "Workflow2";
                int extractAction2 = fileProcessingDb.GetActionID(_LABDE_ACTION1);

                // Queue files with varying priorities:
                // 1 = Normal       Workflow 1
                // 2 = Low          Workflow 2
                // 3 = High         Workflow 1,2
                // 4 = High         Workflow 1
                // 5 = BelowNormal  Workflow 2
                // 6 = AboveNormal  Workflow 1,2
                // 7 = Normal       Workflow 2
                // Workflow 1 order: 3, 4, 6, 1
                // Workflow 2 order: 3, 6, 7, 5, 2
                (string fileName, string actionName, int workflowID, EActionStatus actionStatus, EFilePriority priority)[] testFiles =
                {
                    (_LABDE_TEST_FILE1, _LABDE_ACTION1, workflowID1, EActionStatus.kActionPending, EFilePriority.kPriorityNormal),
                    (_LABDE_TEST_FILE2, _LABDE_ACTION1, workflowID2, EActionStatus.kActionPending, EFilePriority.kPriorityLow),
                    (_LABDE_TEST_FILE3, _LABDE_ACTION1, workflowID1, EActionStatus.kActionPending, EFilePriority.kPriorityHigh),
                    (_LABDE_TEST_FILE3, _LABDE_ACTION1, workflowID2, EActionStatus.kActionPending, EFilePriority.kPriorityHigh),
                    (_LABDE_TEST_FILE4, _LABDE_ACTION1, workflowID1, EActionStatus.kActionPending, EFilePriority.kPriorityHigh),
                    (_LABDE_TEST_FILE5, _LABDE_ACTION1, workflowID2, EActionStatus.kActionPending, EFilePriority.kPriorityBelowNormal),
                    (_LABDE_TEST_FILE6, _LABDE_ACTION1, workflowID1, EActionStatus.kActionPending, EFilePriority.kPriorityAboveNormal),
                    (_LABDE_TEST_FILE6, _LABDE_ACTION1, workflowID2, EActionStatus.kActionPending, EFilePriority.kPriorityAboveNormal),
                    (_LABDE_TEST_FILE7, _LABDE_ACTION1, workflowID2, EActionStatus.kActionPending, EFilePriority.kPriorityNormal),
                };
                var fileIDs = fileProcessingDb.AddTestFiles(_testFiles, testFiles);

                var fileSelector = new FAMFileSelector();
                fileSelector.AddActionStatusCondition(fileProcessingDb, _LABDE_ACTION1, EActionStatus.kActionCompleted);

                var setStatusTaskConfig = new SetActionStatusFileProcessor();
                setStatusTaskConfig.ActionName = _LABDE_ACTION2;
                setStatusTaskConfig.ActionStatus = (int)EActionStatus.kActionPending;
                var setStatusTask = (IFileProcessingTask)setStatusTaskConfig;

                using (var famSession = new FAMProcessingSession(
                    fileProcessingDb, _LABDE_ACTION1, "Workflow1", setStatusTask,
                    threadCount: 1, filesToGrab: 1, keepProcessing: false, docsToProcess: 1))
                {
                    famSession.WaitForProcessingToComplete();
                }

                fileProcessingDb.ActiveWorkflow = "Workflow1";
                Assert.AreEqual(fileIDs[3], fileSelector.GetResults(fileProcessingDb).Single());
                Assert.AreEqual(1, fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsComplete);

                using (var famSession = new FAMProcessingSession(
                    fileProcessingDb, _LABDE_ACTION1, "Workflow2", setStatusTask,
                    threadCount: 1, filesToGrab: 1, keepProcessing: false, docsToProcess: 1))
                {
                    famSession.WaitForProcessingToComplete();
                }

                fileProcessingDb.ActiveWorkflow = "Workflow2";
                Assert.AreEqual(fileIDs[3], fileSelector.GetResults(fileProcessingDb).Single());
                // File 3 should now be processed in both workflows.
                Assert.AreEqual(2, fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsComplete);

                using (var famSession = new FAMProcessingSession(
                    fileProcessingDb, _LABDE_ACTION1, "Workflow1", setStatusTask,
                    threadCount: 2, filesToGrab: 25, keepProcessing: false, docsToProcess: 2))
                {
                    famSession.WaitForProcessingToComplete();
                }

                fileProcessingDb.ActiveWorkflow = "Workflow1";
                Assert.AreEqual(
                    Invariant($"{fileIDs[3]},{fileIDs[4]},{fileIDs[6]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));
                // Ensure extra file grabbed was properly returned to pending.
                Assert.AreEqual(1, fileProcessingDb.GetStats(extractAction1, false).NumDocumentsPending);

                using (var famSession = new FAMProcessingSession(
                    fileProcessingDb, _LABDE_ACTION1, "Workflow1", setStatusTask,
                    threadCount: 2, filesToGrab: 25, keepProcessing: false, docsToProcess: 2))
                {
                    famSession.WaitForProcessingToComplete();
                }

                Assert.AreEqual(0, fileProcessingDb.GetStats(extractAction1, false).NumDocumentsPending);
                Assert.AreEqual(4, fileProcessingDb.GetStats(extractAction1, false).NumDocumentsComplete);
                Assert.AreEqual(4, fileProcessingDb.GetStats(extractAction2, false).NumDocumentsPending);
                Assert.AreEqual(1, fileProcessingDb.GetStats(extractAction2, false).NumDocumentsComplete);

                using (var famSession = new FAMProcessingSession(
                    fileProcessingDb, _LABDE_ACTION1, "Workflow2", setStatusTask,
                    threadCount: 1, filesToGrab: 1, keepProcessing: false, docsToProcess: 2))
                {
                    famSession.WaitForProcessingToComplete();
                }

                fileProcessingDb.ActiveWorkflow = "Workflow2";
                Assert.AreEqual(
                   Invariant($"{fileIDs[3]},{fileIDs[6]},{fileIDs[7]}"),
                   string.Join(",", fileSelector.GetResults(fileProcessingDb)));

                using (var famSession = new FAMProcessingSession(
                    fileProcessingDb, _LABDE_ACTION1, "Workflow2", setStatusTask,
                    threadCount: 2, filesToGrab: 1, keepProcessing: false, docsToProcess: 1))
                {
                    famSession.WaitForProcessingToComplete();
                }

                Assert.AreEqual(
                   Invariant($"{fileIDs[3]},{fileIDs[5]},{fileIDs[6]},{fileIDs[7]}"),
                   string.Join(",", fileSelector.GetResults(fileProcessingDb)));

                // 2 files processed in both workflows, 1 file unprocessed in workflow2
                Assert.AreEqual(1, fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsPending);
                Assert.AreEqual(8, fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsComplete);
            }
            finally
            {
                _testFiles.RemoveFile(_LABDE_TEST_FILE1);
                _testFiles.RemoveFile(_LABDE_TEST_FILE2);
                _testFiles.RemoveFile(_LABDE_TEST_FILE3);
                _testFiles.RemoveFile(_LABDE_TEST_FILE4);
                _testFiles.RemoveFile(_LABDE_TEST_FILE5);
                _testFiles.RemoveFile(_LABDE_TEST_FILE6);
                _testFiles.RemoveFile(_LABDE_TEST_FILE7);
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        [Test, Category("Automated")]
        public static void WorkflowLoadBalancing()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestFAMFileProcessing>();
            _testDbManager = new FAMTestDBManager<TestFAMFileProcessing>();

            string testDbName = "Test_WorkflowLoadBalancing";

            try
            {
                FileProcessingDB fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                int workflowID1 = fileProcessingDb.AddWorkflow(
                    "Workflow1", EWorkflowType.kUndefined, _LABDE_ACTION1, _LABDE_ACTION2);
                int workflowID2 = fileProcessingDb.AddWorkflow(
                    "Workflow2", EWorkflowType.kUndefined, _LABDE_ACTION1, _LABDE_ACTION2);

                (string fileName, string actionName, int workflowID, EActionStatus actionStatus, EFilePriority priority)[] testFiles =
                {
                    (_LABDE_TEST_FILE1, _LABDE_ACTION1, workflowID1, EActionStatus.kActionPending, EFilePriority.kPriorityNormal),
                    (_LABDE_TEST_FILE2, _LABDE_ACTION1, workflowID1, EActionStatus.kActionPending, EFilePriority.kPriorityHigh),
                    (_LABDE_TEST_FILE3, _LABDE_ACTION1, workflowID1, EActionStatus.kActionPending, EFilePriority.kPriorityAboveNormal),
                    (_LABDE_TEST_FILE4, _LABDE_ACTION1, workflowID1, EActionStatus.kActionPending, EFilePriority.kPriorityNormal),
                    (_LABDE_TEST_FILE5, _LABDE_ACTION1, workflowID2, EActionStatus.kActionPending, EFilePriority.kPriorityLow),
                    (_LABDE_TEST_FILE6, _LABDE_ACTION1, workflowID2, EActionStatus.kActionPending, EFilePriority.kPriorityLow),
                    (_LABDE_TEST_FILE7, _LABDE_ACTION1, workflowID2, EActionStatus.kActionPending, EFilePriority.kPriorityBelowNormal),
                    (_LABDE_TEST_FILE8, _LABDE_ACTION1, workflowID2, EActionStatus.kActionPending, EFilePriority.kPriorityNormal),
                };
                var fileIDs = fileProcessingDb.AddTestFiles(_testFiles, testFiles);

                var setStatusTaskConfig = new SetActionStatusFileProcessor();
                setStatusTaskConfig.ActionName = _LABDE_ACTION2;
                setStatusTaskConfig.ActionStatus = (int)EActionStatus.kActionPending;
                var setStatusTask = (IFileProcessingTask)setStatusTaskConfig;

                var fileSelector = new FAMFileSelector();
                fileSelector.AddActionStatusCondition(fileProcessingDb, _LABDE_ACTION1, EActionStatus.kActionCompleted);

                // Ensure the 3 files from each workflow are processed with load-balancing in order of priority, then ID
                using (var famSession = new FAMProcessingSession(
                    fileProcessingDb, _LABDE_ACTION1, _ALL_WORKFLOWS, setStatusTask,
                    threadCount: 1, filesToGrab: 6, keepProcessing: false, docsToProcess: 6))
                {
                    famSession.WaitForProcessingToComplete();
                }

                fileProcessingDb.ActiveWorkflow = "Workflow1";
                Assert.AreEqual(
                   Invariant($"{fileIDs[1]},{fileIDs[2]},{fileIDs[3]}"),
                   string.Join(",", fileSelector.GetResults(fileProcessingDb)));

                fileProcessingDb.ActiveWorkflow = "Workflow2";
                Assert.AreEqual(
                   Invariant($"{fileIDs[5]},{fileIDs[7]},{fileIDs[8]}"),
                   string.Join(",", fileSelector.GetResults(fileProcessingDb)));

                fileProcessingDb.ActiveWorkflow = "";
                fileProcessingDb.SetStatusForAllFiles(_LABDE_ACTION1, EActionStatus.kActionPending);

                // Test that changing the weighting affects the number of files processed for
                // each workflow-- in this case, 2 files for workflow 1, 3 for workflow 2.
                // Again, in order of priority, then ID.
                var workflow1 = fileProcessingDb.GetWorkflowDefinition(workflowID1);
                workflow1.LoadBalanceWeight = 2;
                fileProcessingDb.SetWorkflowDefinition(workflow1);

                var workflow2 = fileProcessingDb.GetWorkflowDefinition(workflowID2);
                workflow2.LoadBalanceWeight = 3;
                fileProcessingDb.SetWorkflowDefinition(workflow2);

                using (var famSession = new FAMProcessingSession(
                    fileProcessingDb, _LABDE_ACTION1, _ALL_WORKFLOWS, setStatusTask,
                    threadCount: 1, filesToGrab: 5, keepProcessing: false, docsToProcess: 5))
                {
                    famSession.WaitForProcessingToComplete();
                }

                fileProcessingDb.ActiveWorkflow = "Workflow1";
                Assert.AreEqual(
                   Invariant($"{fileIDs[2]},{fileIDs[3]}"),
                   string.Join(",", fileSelector.GetResults(fileProcessingDb)));

                fileProcessingDb.ActiveWorkflow = "Workflow2";
                Assert.AreEqual(
                   Invariant($"{fileIDs[5]},{fileIDs[7]},{fileIDs[8]}"),
                   string.Join(",", fileSelector.GetResults(fileProcessingDb)));

                // Ensure that if 2 more files are processed it exhausts the files from one of
                // the workflows without error. (In most cases it will grab one file from each, but
                // there is a small chance it will end up taking 2 files from workflow 1).
                using (var famSession = new FAMProcessingSession(
                    fileProcessingDb, _LABDE_ACTION1, _ALL_WORKFLOWS, setStatusTask,
                    threadCount: 1, filesToGrab: 2, keepProcessing: false, docsToProcess: 2))
                {
                    famSession.WaitForProcessingToComplete();
                }

                Assert.AreEqual(7, fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsComplete);

                // Ensure that when trying to process 2 more, it processes the last file without error.
                using (var famSession = new FAMProcessingSession(
                    fileProcessingDb, _LABDE_ACTION1, _ALL_WORKFLOWS, setStatusTask,
                    threadCount: 1, filesToGrab: 2, keepProcessing: false, docsToProcess: 2))
                {
                    famSession.WaitForProcessingToComplete();
                }

                Assert.AreEqual(8, fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsComplete);

                fileProcessingDb.ActiveWorkflow = "";
                fileProcessingDb.SetStatusForAllFiles(_LABDE_ACTION1, EActionStatus.kActionPending);

                // Turn off load-balancing, and ensure files are processed in order of priority then
                // ID regardless of workflow.
                fileProcessingDb.SetDBInfoSetting("EnableLoadBalancing", "0", true, false);
                using (var famSession = new FAMProcessingSession(
                    fileProcessingDb, _LABDE_ACTION1, _ALL_WORKFLOWS, setStatusTask,
                    threadCount: 1, filesToGrab: 5, keepProcessing: false, docsToProcess: 5))
                {
                    famSession.WaitForProcessingToComplete();
                }

                fileProcessingDb.ActiveWorkflow = "Workflow1";
                Assert.AreEqual(
                   Invariant($"{fileIDs[1]},{fileIDs[2]},{fileIDs[3]},{fileIDs[4]}"),
                   string.Join(",", fileSelector.GetResults(fileProcessingDb)));

                fileProcessingDb.ActiveWorkflow = "Workflow2";
                Assert.AreEqual(
                   Invariant($"{fileIDs[8]}"),
                   string.Join(",", fileSelector.GetResults(fileProcessingDb)));
            }
            finally
            {
                _testFiles.RemoveFile(_LABDE_TEST_FILE1);
                _testFiles.RemoveFile(_LABDE_TEST_FILE2);
                _testFiles.RemoveFile(_LABDE_TEST_FILE3);
                _testFiles.RemoveFile(_LABDE_TEST_FILE4);
                _testFiles.RemoveFile(_LABDE_TEST_FILE5);
                _testFiles.RemoveFile(_LABDE_TEST_FILE6);
                _testFiles.RemoveFile(_LABDE_TEST_FILE7);
                _testFiles.RemoveFile(_LABDE_TEST_FILE8);
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        [Test, Category("Automated")]
        public static void WorkflowLoadBalancingRandomDistribution()
        {
            GeneralMethods.TestSetup();

            HashSet<TemporaryFile> tmpFiles = new HashSet<TemporaryFile>();
            _testFiles = new TestFileManager<TestFAMFileProcessing>();
            _testDbManager = new FAMTestDBManager<TestFAMFileProcessing>();

            string testDbName = "Test_WorkflowLoadBalancingRandomDistribution";

            try
            {
                FileProcessingDB fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                int workflowID1 = fileProcessingDb.AddWorkflow(
                    "Workflow1", EWorkflowType.kUndefined, _LABDE_ACTION1, _LABDE_ACTION2);
                int workflowID2 = fileProcessingDb.AddWorkflow(
                    "Workflow2", EWorkflowType.kUndefined, _LABDE_ACTION1, _LABDE_ACTION2);
                int workflowID3 = fileProcessingDb.AddWorkflow(
                    "Workflow3", EWorkflowType.kUndefined, _LABDE_ACTION1, _LABDE_ACTION2);

                int actionID1 = fileProcessingDb.GetActionIDForWorkflow(_LABDE_ACTION1, workflowID1);
                int actionID2 = fileProcessingDb.GetActionIDForWorkflow(_LABDE_ACTION1, workflowID2);
                int actionID3 = fileProcessingDb.GetActionIDForWorkflow(_LABDE_ACTION1, workflowID3);

                var workflow2 = fileProcessingDb.GetWorkflowDefinition(workflowID2);
                workflow2.LoadBalanceWeight = 2;
                fileProcessingDb.SetWorkflowDefinition(workflow2);

                var workflow3 = fileProcessingDb.GetWorkflowDefinition(workflowID3);
                workflow3.LoadBalanceWeight = 3;
                fileProcessingDb.SetWorkflowDefinition(workflow3);

                for (int i = 0; i < 600; i++)
                {
                    var tmpFile = new TemporaryFile(".tif", false);
                    tmpFiles.Add(tmpFile);

                    fileProcessingDb.AddFile(tmpFile.FileName, _LABDE_ACTION1, i % 3 + 1, EFilePriority.kPriorityNormal, false,
                        false, EActionStatus.kActionPending, true, out bool alreadyExists, out EActionStatus previousStatus);
                }

                var setStatusTaskConfig = new SetActionStatusFileProcessor();
                setStatusTaskConfig.ActionName = _LABDE_ACTION2;
                setStatusTaskConfig.ActionStatus = (int)EActionStatus.kActionPending;
                var setStatusTask = (IFileProcessingTask)setStatusTaskConfig;

                // Without load balancing, files should be processed in order of file ID; in this
                // case that means exactly 100 files from each workflow.
                fileProcessingDb.SetDBInfoSetting("EnableLoadBalancing", "0", true, false);
                using (var famSession = new FAMProcessingSession(
                    fileProcessingDb, _LABDE_ACTION1, _ALL_WORKFLOWS, setStatusTask,
                    threadCount: 4, filesToGrab: 1, keepProcessing: false, docsToProcess: 300))
                {
                    famSession.WaitForProcessingToComplete();
                }

                int workflow1Complete = fileProcessingDb.GetStats(actionID1, false).NumDocumentsComplete;
                int workflow2Complete = fileProcessingDb.GetStats(actionID2, false).NumDocumentsComplete;
                int workflow3Complete = fileProcessingDb.GetStats(actionID3, false).NumDocumentsComplete;

                Assert.AreEqual(100, workflow1Complete);
                Assert.AreEqual(100, workflow2Complete);
                Assert.AreEqual(100, workflow3Complete);

                fileProcessingDb.SetStatusForAllFiles(_LABDE_ACTION1, EActionStatus.kActionPending);

                // With load balancing and grabbing only 1 files at a time, the overall distribution
                // should reflect the respective dept weightings, though the exact number of files
                // processed for each will based on random chance.
                fileProcessingDb.SetDBInfoSetting("EnableLoadBalancing", "1", true, false);
                using (var famSession = new FAMProcessingSession(
                    fileProcessingDb, _LABDE_ACTION1, _ALL_WORKFLOWS, setStatusTask,
                    threadCount: 4, filesToGrab: 1, keepProcessing: false, docsToProcess: 300))
                {
                    famSession.WaitForProcessingToComplete();
                }

                workflow1Complete = fileProcessingDb.GetStats(actionID1, false).NumDocumentsComplete;
                workflow2Complete = fileProcessingDb.GetStats(actionID2, false).NumDocumentsComplete;
                workflow3Complete = fileProcessingDb.GetStats(actionID3, false).NumDocumentsComplete;
                System.Diagnostics.Trace.WriteLine(Invariant($"Workflow 1: {workflow1Complete}"));
                System.Diagnostics.Trace.WriteLine(Invariant($"Workflow 2: {workflow2Complete}"));
                System.Diagnostics.Trace.WriteLine(Invariant($"Workflow 3: {workflow3Complete}"));

                var diff1 = Math.Abs(50 - workflow1Complete);
                var diff2 = Math.Abs(100 - workflow2Complete);
                var diff3 = Math.Abs(150 - workflow3Complete);

                // To achieve 99% confidence we are allowing for a wide enough range, the margin of
                // error is 7% for 300 files = 21. Make that for 24 for still some more leeway while
                // still proving different distributions between all workflows.
                Assert.Less(diff1, 24);
                Assert.Less(diff2, 24);
                Assert.Less(diff3, 24);

                fileProcessingDb.SetStatusForAllFiles(_LABDE_ACTION1, EActionStatus.kActionPending);

                // If the same test is repeated, but # of files to grab at a time is equal to the
                // sum of all dept weightings (1 + 2 + 3), this should guarantee that the number of
                // files processed for each dept exactly corresponds to the weightings.
                using (var famSession = new FAMProcessingSession(
                    fileProcessingDb, _LABDE_ACTION1, _ALL_WORKFLOWS, setStatusTask,
                    threadCount: 4, filesToGrab: 6, keepProcessing: false, docsToProcess: 300))
                {
                    famSession.WaitForProcessingToComplete();
                }

                workflow1Complete = fileProcessingDb.GetStats(actionID1, false).NumDocumentsComplete;
                workflow2Complete = fileProcessingDb.GetStats(actionID2, false).NumDocumentsComplete;
                workflow3Complete = fileProcessingDb.GetStats(actionID3, false).NumDocumentsComplete;

                Assert.AreEqual(50, workflow1Complete);
                Assert.AreEqual(100, workflow2Complete);
                Assert.AreEqual(150, workflow3Complete);
            }
            finally
            {
                CollectionMethods.ClearAndDispose(tmpFiles);
                _testDbManager.RemoveDatabase(testDbName);
            }
        }


        /// <summary>
        /// Tests that when the same file is queued to multiple workflows, it will process
        /// separately for each workflow but never in more than one workflow at the same time.
        /// </summary>
        [Test, Category("Automated")]
        public static void SameFileMultipleWorkflows()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestFAMFileProcessing>();
            _testDbManager = new FAMTestDBManager<TestFAMFileProcessing>();

            string testDbName = "Test_FileProcessingManagerWithWorkflows";

            try
            {
                int threadCount = 4;
                var threadReadyEvent = new CountdownEvent(threadCount);
                var startProcessingEvent = new ManualResetEvent(false);

                string testFileName1 = _testFiles.GetFile(_LABDE_TEST_FILE1);
                FileProcessingDB fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                // Create a separate workflow for each thread and queue the same file to each workflow.
                for (int i = 1; i <= threadCount; i++)
                {
                    string workflowName = Invariant($"Workflow{i}");
                    int workflowID = fileProcessingDb.AddWorkflow(
                        workflowName, EWorkflowType.kUndefined, _LABDE_ACTION1);

                    fileProcessingDb.AddFile(testFileName1, _LABDE_ACTION1, workflowID, EFilePriority.kPriorityNormal,
                        false, false, EActionStatus.kActionPending, false, out bool alreadyExists, out EActionStatus previousStatus);
                }

                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsPending == threadCount);

                // Each processing thread will try to process a file in one of the created workflows.
                Action processingThreadAction = () =>
                {
                    var sleepTaskConfig = new SleepTask();
                    sleepTaskConfig.SleepTime = 1;
                    sleepTaskConfig.TimeUnits = ESleepTimeUnitType.kSleepSeconds;
                    var sleepTask = (IFileProcessingTask)sleepTaskConfig;

                    // Wait until all threads are ready before starting processing to help ensure
                    // all workflows will be competing for the file simultaneously.
                    threadReadyEvent.Signal();
                    startProcessingEvent.WaitOne();

                    using (var famSession = new FAMProcessingSession(
                        fileProcessingDb, _LABDE_ACTION1, _ALL_WORKFLOWS, sleepTask,
                        threadCount: 1, filesToGrab: 1, keepProcessing: true, docsToProcess: 1))
                    {
                        int processed = famSession.WaitForProcessingToComplete();
                        Assert.That(processed == 1);
                    }
                };

                // Launch the processing threads
                //Assert.That(Task.Factory.Scheduler.MaximumConcurrencyLevel >= (threadCount + 1));
                var tasks = Enumerable
                    .Range(0, threadCount)
                    .Select(x => Task.Factory.StartNew(processingThreadAction))
                    .ToArray();

                // In order to ensure as much as possible that all processing threads are competing
                // to processing the same file, wait until all processing threads are active before
                // allowing them to commence processing. If all threads haven't started in 1 second
                // it is not likely multithreading is happening to a degree that will allow for a
                // good test.
                Assert.That(threadReadyEvent.Wait(1000));
                startProcessingEvent.Set();

                // Check 10 times per sec to ensure that there is never any more than 1 file
                // processing at a time (that the file is not being simultaneously processed in
                // more than one workflow)
                do
                {
                    var stats = fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false);
                    int processing = stats.NumDocumentsPending + stats.NumDocumentsComplete;

                    Assert.That(processing >= (threadCount - 1));
                }
                while (!Task.WaitAll(tasks, 100));

                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsPending == 0);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsComplete == threadCount);
            }
            finally
            {
                _testFiles.RemoveFile(_LABDE_TEST_FILE1);
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        /// <summary>
        /// Tests adding, renaming and deleting actions both in an out of a workflow.
        /// </summary>
        [Test, Category("Automated")]
        public static void ConnectionRetryProperties()
        {
            string testDbName = "Test_ConnectionRetryProperties";
            FileProcessingDB fileProcessingDb = null;

            try
            {
                fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                // Set connections retry properties to known settings
                fileProcessingDb.SetDBInfoSetting("NumberOfConnectionRetries", "10", true, false);
                fileProcessingDb.SetDBInfoSetting("ConnectionRetryTimeout", "120", true, false);

                int numberOfRetries;
                double retryTimeOut;

                // Get the current number of retries
                fileProcessingDb.GetConnectionRetrySettings(out numberOfRetries, out retryTimeOut);

                Assert.That(numberOfRetries == 10);
                Assert.That(retryTimeOut == 120.0);

                Assert.That(fileProcessingDb.NumberOfConnectionRetries == 10);
                Assert.That(fileProcessingDb.ConnectionRetryTimeout == 120);

                // Change the settings
                // Set connections retry properties to known settings
                fileProcessingDb.SetDBInfoSetting("NumberOfConnectionRetries", "20", true, false);
                fileProcessingDb.SetDBInfoSetting("ConnectionRetryTimeout", "150", true, false);

                // This causes the settings to be reloaded from the DBInfo table
                fileProcessingDb.ResetDBConnection(true, false);

                // Get the current number of retries
                fileProcessingDb.GetConnectionRetrySettings(out numberOfRetries, out retryTimeOut);

                Assert.That(numberOfRetries == 20);
                Assert.That(retryTimeOut == 150.0);

                Assert.That(fileProcessingDb.NumberOfConnectionRetries == 20);
                Assert.That(fileProcessingDb.ConnectionRetryTimeout == 150.0);


                // Set using the properties
                fileProcessingDb.NumberOfConnectionRetries = 0;
                fileProcessingDb.ConnectionRetryTimeout = 0;

                fileProcessingDb.GetConnectionRetrySettings(out numberOfRetries, out retryTimeOut);

                Assert.That(numberOfRetries == 0);
                Assert.That(retryTimeOut == 0);

                Assert.That(fileProcessingDb.NumberOfConnectionRetries == 0);
                Assert.That(fileProcessingDb.ConnectionRetryTimeout == 0);

                // This causes the settings to be reloaded from the DBInfo table
                fileProcessingDb.ResetDBConnection(true, false);

                fileProcessingDb.GetConnectionRetrySettings(out numberOfRetries, out retryTimeOut);

                Assert.That(numberOfRetries == 0);
                Assert.That(retryTimeOut == 0);

                Assert.That(fileProcessingDb.NumberOfConnectionRetries == 0);
                Assert.That(fileProcessingDb.ConnectionRetryTimeout == 0);

                // Open a different instance to see that the setting in the DBInfo table are
                // what they were last set to
                fileProcessingDb = new FileProcessingDB();
                fileProcessingDb.DatabaseName = testDbName;
                fileProcessingDb.DatabaseServer = "(local)";
                fileProcessingDb.ResetDBConnection(true, false);

                fileProcessingDb.GetConnectionRetrySettings(out numberOfRetries, out retryTimeOut);
                Assert.That(numberOfRetries == 20);
                Assert.That(retryTimeOut == 150.0);

                Assert.That(fileProcessingDb.NumberOfConnectionRetries == 20);
                Assert.That(fileProcessingDb.ConnectionRetryTimeout == 150.0);
            }
            finally
            {
                fileProcessingDb?.CloseAllDBConnections();
                fileProcessingDb = null;
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        /// <summary>
        /// Tests addition of new schema elements to track verification session timeouts in the FileTaskSession table
        /// https://extract.atlassian.net/browse/ISSUE-17793
        /// </summary>
        [Test, Category("Automated")]
        public static void VerificationSessionTiming()
        {
            HashSet<TemporaryFile> tmpFiles = new HashSet<TemporaryFile>();
            _testFiles = new TestFileManager<TestFAMFileProcessing>();
            _testDbManager = new FAMTestDBManager<TestFAMFileProcessing>();

            string testDbName = "Test_VerificationSessionTiming";

            try
            {
                DateTime testStartTime = DateTime.Now;

                using var dbWrapper = new OneWorkflow<TestFAMFileProcessing>(_testDbManager, testDbName, false);
                dbWrapper.FileProcessingDB.SetDBInfoSetting("VerificationSessionTimeout", "2", vbSetIfExists: true, vbRecordHistory: true);

                Enumerable.Range(1, 5)
                    .Select(i => dbWrapper.AddFakeFile(i, setAsSkipped: false))
                    .ToList();

                int lostSession = dbWrapper.FileProcessingDB.StartFileTaskSession(Constants.TaskClassWebVerification, 1, 1);

                Task.WaitAll(new[]
                {
                    AddSession(dbWrapper, fileId: 2, duration: 5, overheadTime: 1, activityTime: 4, timedOut: false),
                    AddSession(dbWrapper, fileId: 3, duration: 5, overheadTime: 1, activityTime: 2, timedOut: true),
                    AddSession(dbWrapper, fileId: 4, duration: 3, overheadTime: 1, activityTime: 0, timedOut: true),
                    AddSession(dbWrapper, fileId: 5, duration: 2, overheadTime: 0, activityTime: 1, timedOut: false)
                });

                // StartDateTime, DateTimeStamp, Duration, OverheadTime, ActivityTime, TimedOut, DurationMinusTimeout
                var sessionRows = GetFileTaskSessionData(testDbName);

                Assert.AreEqual(5, sessionRows.Count);
                sessionRows.ForEach(row =>
                {
                    int fileID = (int)row["FileID"];
                    var timingValues = row.Values.Skip(3);
                    switch (fileID)
                    {
                        case 1: Assert.IsTrue(timingValues.SequenceEqual(new object[] { DBNull.Value, DBNull.Value, DBNull.Value, false, DBNull.Value })); break;
                        case 2: Assert.IsTrue(timingValues.SequenceEqual(new object[] { (double)5, (double)1, (double)4, false, (double)5 })); break;
                        case 3: Assert.IsTrue(timingValues.SequenceEqual(new object[] { (double)5, (double)1, (double)2, true, (double)3 })); break;
                        case 4: Assert.IsTrue(timingValues.SequenceEqual(new object[] { (double)3, (double)1, (double)0, true, (double)1 })); break;
                        case 5: Assert.IsTrue(timingValues.SequenceEqual(new object[] { (double)2, (double)0, (double)1, false, (double)2 })); break;
                    }

                    if (row["DateTimeStamp"] is not DBNull)
                    {
                        Assert.Greater((DateTime)row["StartDateTime"], testStartTime);
                        Assert.AreEqual(row["Duration"],
                            Math.Round(((DateTime)row["DateTimeStamp"] - (DateTime)row["StartDateTime"]).TotalSeconds));
                    }
                });
            }
            finally
            {
                CollectionMethods.ClearAndDispose(tmpFiles);
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        /// VerificationSessionTiming helper; starts/stops FileTaskSession for specified file with specified timings.
        static async Task AddSession(OneWorkflow<TestFAMFileProcessing> dbWrapper, int fileId,
            double duration, double overheadTime, double activityTime, bool timedOut)
        {
            int sessionID = dbWrapper.FileProcessingDB.StartFileTaskSession(Constants.TaskClassWebVerification, fileId, 1);
            await Task.Delay((int)(duration * 1000));
            dbWrapper.FileProcessingDB.EndFileTaskSession(sessionID, overheadTime, activityTime, timedOut);
        }

        /// VerificationSessionTiming helper; gets FileTaskSession data to check timings were recorded properly.
        static List<Dictionary<string, object>> GetFileTaskSessionData(string dbName)
        {
            using ExtractRoleConnection dbConnection = new(SqlUtil.CreateConnectionString("(local)", dbName));
            dbConnection.Open();

            using var tableData = DBMethods.ExecuteDBQuery(dbConnection, @"
SELECT [FileID], [StartDateTime], [DateTimeStamp], [Duration], [OverheadTime], [ActivityTime], [TimedOut], [DurationMinusTimeout]
    FROM [FileTaskSession]");

            int columnIndex = 0;
            var columns = tableData.Columns
                .OfType<DataColumn>()
                .Select(c => (columnIndex++, c.ColumnName))
                .ToList();

            return tableData.Rows.OfType<DataRow>()
                .Select(row => Enumerable.Range(0, columns.Count())
                    .ToDictionary(
                        i => columns[i].ColumnName,
                        i => row.ItemArray[i] switch
                        {
                            double d => Math.Round(d),
                            object o => o
                        }))
                .ToList();
        }

        /// <summary>
        /// Tests that a file is considered available for processing even if it is processing in an unrelated action
        /// </summary>
        [Test, Category("Automated")]
        [Parallelizable(ParallelScope.All)]
        public static void Test_GetFilesToProcess_Can_Get_File_When_It_Is_Processing_In_Unrelated_Action(
            [Values] bool getSkipped, [Values(1, 5)] int maxFilesToGet, [Values] bool enableLoadBalancing)
        {
            EActionStatus statusToGetFrom = getSkipped ? EActionStatus.kActionSkipped : EActionStatus.kActionPending;
            string testDBName = "Test_" + Guid.NewGuid().ToString();

            try
            {
                using var fpDB = new TwoWorkflows<TestFAMFileProcessing>(_testDbManager, testDBName, enableLoadBalancing);

                // Add two files and set to pending (or skipped) for action 1 and 2 in workflow 1 and 2
                int testFile1 = fpDB.AddFakeFile(1, getSkipped);
                fpDB.AddFakeFile(2, getSkipped);

                // Restart fam session if processing skipped files or they won't be returned
                if (getSkipped)
                {
                    fpDB.startNewSession(fpDB.wf1);
                }

                // Set file 1 to processing for action 2 in workflow 1
                int action2ID = fpDB.wf1.GetActionID(fpDB.action2);
                fpDB.wf1.SetFileStatusToProcessing(testFile1, action2ID);
                Assert.AreEqual(EActionStatus.kActionProcessing, fpDB.wf1.GetFileStatus(testFile1, fpDB.action2, false));

                // Confirm that file 1 is pending for action 1 in workflow 1
                Assert.AreEqual(statusToGetFrom, fpDB.wf1.GetFileStatus(testFile1, fpDB.action1, false));

                // Confirm that file 1 is returned by GetFilesToProcess even though it is processing in action 2
                var filesToProcess = fpDB.wf1.GetFilesToProcess(fpDB.action1, maxFilesToGet, getSkipped, "")
                    .ToIEnumerable<IFileRecord>()
                    .Select(fileRecord => fileRecord.FileID)
                    .ToList();

                // Confirm that only those files returned plus the previous file are set to Processing
                Assert.AreEqual(1 + filesToProcess.Count, fpDB.getTotalProcessing());

                Assert.Greater(filesToProcess.Count, 0);
                Assert.Contains(testFile1, filesToProcess);
            }
            catch (COMException cex)
            {
                throw ExtractException.FromStringizedByteStream("ELI51551", cex.Message);
            }
        }

        /// <summary>
        /// Tests that a file is not considered available for processing in AllWorkflows mode if it is processing in any workflow for the action
        /// </summary>
        [Test, Category("Automated")]
        [Parallelizable(ParallelScope.All)]
        public static void GetFilesToProcess_Do_Not_Get_File_For_AllWorkflows_When_It_Is_Processing_In_Related_Action(
            [Values] bool getSkipped, [Values(1, 5)] int maxFilesToGet, [Values] bool enableLoadBalancing)
        {
            EActionStatus statusToGetFrom = getSkipped ? EActionStatus.kActionSkipped : EActionStatus.kActionPending;
            string testDBName = "Test_" + Guid.NewGuid().ToString();

            try
            {
                using var fpDB = new TwoWorkflows<TestFAMFileProcessing>(_testDbManager, testDBName, enableLoadBalancing);

                // Add two files and set to pending (or skipped) for action 1 and 2 in workflow 1 and 2
                int testFile1 = fpDB.AddFakeFile(1, getSkipped);
                fpDB.AddFakeFile(2, getSkipped);

                // Set file 1 to processing for action 1 in workflow 1
                fpDB.wf1.SetFileStatusToProcessing(testFile1, fpDB.wf1.GetActionIDForWorkflow(fpDB.action1, 1));
                Assert.AreEqual(EActionStatus.kActionProcessing, fpDB.wf1.GetFileStatus(testFile1, fpDB.action1, false));

                // Confirm that file 1 is pending for action 1 in workflow 2
                Assert.AreEqual(statusToGetFrom, fpDB.wf2.GetFileStatus(testFile1, fpDB.action1, false));

                // Use all workflows to get files
                // Confirm that file 1 is _not_ returned by GetFilesToProcess even though it is in the queue for action 1 in workflow 2
                var filesToProcess = fpDB.wfAll.GetFilesToProcess(fpDB.action1, maxFilesToGet, getSkipped, "")
                    .ToIEnumerable<IFileRecord>()
                    .Select(fileRecord => fileRecord.FileID)
                    .ToList();

                // Confirm that only those files returned plus the previous file are set to Processing
                Assert.AreEqual(1 + filesToProcess.Count, fpDB.getTotalProcessing());

                Assert.Greater(filesToProcess.Count, 0);
                CollectionAssert.DoesNotContain(filesToProcess, testFile1);
            }
            catch (COMException cex)
            {
                throw ExtractException.FromStringizedByteStream("ELI51552", cex.Message);
            }
        }

        /// <summary>
        /// Tests that files in multiple workflows can be gotten for processing in AllWorkflows
        /// </summary>
        [Test, Category("Automated")]
        [CLSCompliant(false)]
        [Parallelizable(ParallelScope.All)]
        public static void GetFilesToProcess_Can_Get_File_For_AllWorkflows_When_It_Is_Available_In_Two_Workflows(
            [Values] bool getSkipped, [Values(1, 5)] int maxFilesToGet, [Values(1, 2, 5, 10)] int filesInDB, [Values] bool enableLoadBalancing)
        {
            // Getting more than one file without using load balancing doesn't work quite right with the final version of GFTP chosen for 11.7.
            Assume.That(maxFilesToGet == 1 || maxFilesToGet > filesInDB || enableLoadBalancing, "This permutation is not supported");

            EActionStatus statusToGetFrom = getSkipped ? EActionStatus.kActionSkipped : EActionStatus.kActionPending;
            string testDBName = "Test_" + Guid.NewGuid().ToString();

            try
            {
                using var fpDB = new TwoWorkflows<TestFAMFileProcessing>(_testDbManager, testDBName, enableLoadBalancing);

                // Add files and set to pending (or skipped) for action 1 and 2 in workflow 1 and 2
                var fileNumbers = Enumerable.Range(1, filesInDB).ToArray();
                foreach (var fileNumber in fileNumbers)
                {
                    int fileID = fpDB.AddFakeFile(fileNumber, getSkipped);
                    Assert.AreEqual(fileNumber, fileID);

                    // Confirm that file is pending for action 1 in workflow 1 and 2
                    Assert.AreEqual(statusToGetFrom, fpDB.wf1.GetFileStatus(fileID, fpDB.action1, false));
                    Assert.AreEqual(statusToGetFrom, fpDB.wf2.GetFileStatus(fileID, fpDB.action1, false));
                }

                // Use all workflows to get files
                var filesToProcess = fpDB.wfAll.GetFilesToProcess(fpDB.action1, maxFilesToGet, getSkipped, "")
                    .ToIEnumerable<IFileRecord>()
                    .Select(fileRecord => fileRecord.FileID)
                    .ToList();

                // Confirm that only those files returned are set to Processing
                Assert.AreEqual(filesToProcess.Count, fpDB.getTotalProcessing());

                // Confirm that all files, up to max, are returned by GetFilesToProcess
                // Ignore ordering
                var expectedFiles = fileNumbers.Take(maxFilesToGet).ToArray();
                CollectionAssert.AreEqual(expectedFiles, filesToProcess);
            }
            catch (COMException cex)
            {
                throw ExtractException.FromStringizedByteStream("ELI51554", cex.Message);
            }
        }

        [Test, Category("Automated")]
        [Parallelizable(ParallelScope.All)]
        public static void GetFilesToProcess_Respect_Workflow_And_Priority(
            [Values(1, 2)] int workflow,
            [Values] bool getSkipped,
            [Values] bool enableLoadBalancing)
        {
            string testDBName = "Test_" + Guid.NewGuid().ToString();

            try
            {
                using var fpDB = new TwoWorkflows<TestFAMFileProcessing>(_testDbManager, testDBName, enableLoadBalancing);

                // Add 100 files with normal priority and set to pending (or skipped) for action 1 and 2 in workflow 1 and 2
                for (int i = 1; i <= 100; i++)
                {
                    fpDB.AddFakeFile(i, getSkipped);
                }
                // Add 100 files with above normal priority and set to pending (or skipped) for action 1 and 2 in workflow 1 and 2
                for (int i = 101; i <= 200; i++)
                {
                    fpDB.AddFakeFile(i, getSkipped, EFilePriority.kPriorityAboveNormal);
                }

                var currentWorkflow = workflow == 1 ? fpDB.wf1 : fpDB.wf2;

                // Restart fam session if processing skipped files or they won't be returned
                if (getSkipped)
                {
                    fpDB.startNewSession(currentWorkflow);
                }

                // Set random 100 files to complete for the current workflow
                var numComplete = 100;
                var allFiles = Enumerable.Range(1, 200).ToList();
                CollectionMethods.Shuffle(allFiles);

                var completeFiles = allFiles.Take(numComplete).ToList();
                foreach (var fileID in completeFiles)
                {
                    currentWorkflow.NotifyFileProcessed(fileID, fpDB.action1, nWorkflowID: workflow, vbAllowQueuedStatusOverride: false);
                }

                var pendingFiles = allFiles.Skip(numComplete).ToList();
                var expectedFiles =
                    pendingFiles
                    .OrderBy(fileID => fileID > 100 ? 1 : 2) // above normal priority first
                    .ThenBy(fileID => fileID)
                    .Take(50)
                    .ToArray();

                // Use current workflow to get 50 files
                var filesToProcess = currentWorkflow.GetFilesToProcess(fpDB.action1, 50, getSkipped, "")
                    .ToIEnumerable<IFileRecord>()
                    .ToArray();

                // Confirm that only those files returned are set to Processing
                Assert.AreEqual(filesToProcess.Count(), fpDB.getTotalProcessing());

                // Confirm that expected files are returned
                var fileIDsToProcess = filesToProcess
                    .Select(fileRecord => fileRecord.FileID)
                    .ToArray();
                CollectionAssert.AreEqual(expectedFiles, fileIDsToProcess);

                // Confirm that the files are for the correct workflow
                var workflows = filesToProcess
                    .Select(fileRecord => fileRecord.WorkflowID)
                    .Distinct()
                    .ToArray();
                CollectionAssert.AreEqual(new[] { workflow }, workflows);
            }
            catch (COMException cex)
            {
                throw ExtractException.FromStringizedByteStream("ELI51555", cex.Message);
            }
        }

        /// <summary>
        /// Gets a random selection of files, sets them to pending in action two, and calls get files to process on them.
        /// </summary>
        [Test, Category("Automated")]
        [Parallelizable(ParallelScope.Self)]
        public static void GetFilesToProcessAfterModifyActionStatusForSelectionWithSubsetsRandom()
        {
            string testDBName = "Test_ModifyActionStatusForSelectionWithSubsetsRandom";
            using var dbWrapper = new OneWorkflow<TestFAMFileProcessing>(_testDbManager, testDBName, false);
            dbWrapper.AddFakeFile(1, false, EFilePriority.kPriorityNormal);
            dbWrapper.AddFakeFile(2, false, EFilePriority.kPriorityNormal);
            dbWrapper.FileProcessingDB.SetStatusForAllFiles("Action2", EActionStatus.kActionUnattempted);
            try
            {
                var fileSelector = new FAMFileSelector();
                fileSelector.LimitToSubset(bRandomSubset: true, bTopSubset: false, bUsePercentage: true, nSubsetSize: 50, nOffset: -1);
                dbWrapper.FileProcessingDB.ModifyActionStatusForSelection(fileSelector, "Action2", EActionStatus.kActionPending, "Action1", true);

                var files = dbWrapper.FileProcessingDB.GetFilesToProcess("Action2", 5, false, string.Empty)
                            .ToIEnumerable<IFileRecord>()
                            .Select(fileRecord => fileRecord.FileID)
                            .ToList();

                Assert.That(files.Count == 1);
            }
            catch (COMException cex)
            {
                throw ExtractException.FromStringizedByteStream("ELI51562", cex.Message);
            }
        }

        /// <summary>
        /// Gets file two via query condition, sets it to pending in action two, and calls get files to process on it.
        /// </summary>
        [Test, Category("Automated")]
        [Parallelizable(ParallelScope.Self)]
        public static void GetFilesToProcessAfterModifyActionStatusForSelectionWithQuerySubset()
        {
            string testDBName = "TestModifyActionStatusForSelectionWithQuerySubSet";
            using var dbWrapper = new OneWorkflow<TestFAMFileProcessing>(_testDbManager, testDBName, false);
            dbWrapper.AddFakeFile(1, false, EFilePriority.kPriorityNormal);
            dbWrapper.AddFakeFile(2, false, EFilePriority.kPriorityNormal);
            dbWrapper.FileProcessingDB.SetStatusForAllFiles("Action2", EActionStatus.kActionUnattempted);
            try
            {

                var fileSelector = new FAMFileSelector();
                fileSelector.AddQueryCondition("SELECT [FAMFile].[ID] FROM [FAMFile] WHERE [ID] = 2");

                dbWrapper.FileProcessingDB.ModifyActionStatusForSelection(fileSelector, "Action2", EActionStatus.kActionPending, "Action1", true);

                var files = dbWrapper.FileProcessingDB.GetFilesToProcess("Action2", 5, false, string.Empty)
                            .ToIEnumerable<IFileRecord>()
                            .Select(fileRecord => fileRecord.FileID)
                            .ToList();

                Assert.That(files.Contains(2) && files.Count == 1);
            }
            catch (COMException cex)
            {
                throw ExtractException.FromStringizedByteStream("ELI51563", cex.Message);
            }
        }

        /// <summary>
        /// Gets the top 50% of files (File one) sets it to pending in action two, and gets it to process.
        /// </summary>
        [Test, Category("Automated")]
        [Parallelizable(ParallelScope.Self)]
        public static void GetFilesToProcessAfterModifyActionStatusSubsetTop()
        {
            string testDBName = "TestGetFilesToProcessAfterModifyActionStatusSubSetTop";
            using var dbWrapper = new OneWorkflow<TestFAMFileProcessing>(_testDbManager, testDBName, false);
            dbWrapper.AddFakeFile(1, false, EFilePriority.kPriorityNormal);
            dbWrapper.AddFakeFile(2, false, EFilePriority.kPriorityNormal);
            dbWrapper.FileProcessingDB.SetStatusForAllFiles("Action2", EActionStatus.kActionUnattempted);
            try
            {
                var fileSelector = new FAMFileSelector();
                // Get file one for the selector
                fileSelector.LimitToSubset(false, true, true, 50, -1);

                // Set file one to pending in action two 
                dbWrapper.FileProcessingDB.ModifyActionStatusForSelection(fileSelector, "Action2", EActionStatus.kActionPending, "Action1", true);

                // Ensure file one is obtained by gftp
                var files = dbWrapper.FileProcessingDB.GetFilesToProcess("Action2", 5, false, string.Empty)
                            .ToIEnumerable<IFileRecord>()
                            .Select(fileRecord => fileRecord.FileID)
                            .ToList();

                Assert.That(files.Contains(1) && files.Count == 1);
            }
            catch (COMException cex)
            {
                throw ExtractException.FromStringizedByteStream("ELI51564", cex.Message);
            }
        }

        /// <summary>
        /// Gets the bottom 50% of files (File two) sets it to pending in action two, and gets it to process.
        /// </summary>
        [Test, Category("Automated")]
        [Parallelizable(ParallelScope.Self)]
        public static void GetFilesToProcessAfterModifyActionStatusSubsetBottom()
        {
            string testDBName = "TestGetFilesToProcessAfterModifyActionStatusSubSetBottom";
            using var dbWrapper = new OneWorkflow<TestFAMFileProcessing>(_testDbManager, testDBName, false);
            dbWrapper.AddFakeFile(1, false, EFilePriority.kPriorityNormal);
            dbWrapper.AddFakeFile(2, false, EFilePriority.kPriorityNormal);
            dbWrapper.FileProcessingDB.SetStatusForAllFiles("Action2", EActionStatus.kActionUnattempted);
            try
            {
                var fileSelector = new FAMFileSelector();
                // Get file two for the selector
                fileSelector.LimitToSubset(false, false, true, 50, -1);

                // Set file two to pending in action two 
                dbWrapper.FileProcessingDB.ModifyActionStatusForSelection(fileSelector, "Action2", EActionStatus.kActionPending, "Action1", true);

                // Ensure file two is obtained by gftp
                var files = dbWrapper.FileProcessingDB.GetFilesToProcess("Action2", 5, false, string.Empty)
                            .ToIEnumerable<IFileRecord>()
                            .Select(fileRecord => fileRecord.FileID)
                            .ToList();

                Assert.That(files.Contains(2) && files.Count == 1);
            }
            catch (COMException cex)
            {
                throw ExtractException.FromStringizedByteStream("ELI51565", cex.Message);
            }
        }

        /// <summary>
        /// Sets files to pending using a simple condition, then calls get files to process on it.
        /// </summary>
        [Test, Category("Automated")]
        [Parallelizable(ParallelScope.Self)]
        public static void GetFilesToProcessAfterModifyActionStatusForSelectionBaseCase()
        {
            string testDBName = "TestModifyActionStatusForSelectionBaseCase";
            using var dbWrapper = new OneWorkflow<TestFAMFileProcessing>(_testDbManager, testDBName, false);
            dbWrapper.AddFakeFile(1, false, EFilePriority.kPriorityNormal);
            dbWrapper.AddFakeFile(2, false, EFilePriority.kPriorityNormal);
            dbWrapper.FileProcessingDB.SetStatusForAllFiles("Action2", EActionStatus.kActionUnattempted);
            try
            {
                var fileSelector = new FAMFileSelector();
                fileSelector.AddActionStatusCondition(dbWrapper.FileProcessingDB, "Action1", EActionStatus.kActionPending);

                dbWrapper.FileProcessingDB.ModifyActionStatusForSelection(fileSelector, "Action2", EActionStatus.kActionPending, "Action1", true);

                var files = dbWrapper.FileProcessingDB.GetFilesToProcess("Action2", 5, false, string.Empty)
                            .ToIEnumerable<IFileRecord>()
                            .Select(fileRecord => fileRecord.FileID)
                            .ToList();
                Assert.That(files.Contains(2) && files.Contains(1) && files.Count == 2);
            }
            catch (COMException cex)
            {
                throw ExtractException.FromStringizedByteStream("ELI51566", cex.Message);
            }
        }

        /// <summary>
        /// Tags the first file, and applies a tag condition. Finally call get files to process on that one file.
        /// </summary>
        [Test, Category("Automated")]
        [Parallelizable(ParallelScope.Self)]
        public static void GetFilesToProcessAfterModifyActionStatusForSelectionTags()
        {
            string testDBName = "TestModifyActionStatusForSelectionTags";
            using var dbWrapper = new OneWorkflow<TestFAMFileProcessing>(_testDbManager, testDBName, false);
            dbWrapper.AddFakeFile(1, false, EFilePriority.kPriorityNormal);
            dbWrapper.AddFakeFile(2, false, EFilePriority.kPriorityNormal);
            dbWrapper.FileProcessingDB.AddTag("Test", "", false);
            dbWrapper.FileProcessingDB.TagFile(1, "Test");
            dbWrapper.FileProcessingDB.SetStatusForAllFiles("Action2", EActionStatus.kActionUnattempted);
            try
            {
                var fileSelector = new FAMFileSelector();
                fileSelector.AddFileTagCondition("Test", TagMatchType.eAnyTag);

                dbWrapper.FileProcessingDB.ModifyActionStatusForSelection(fileSelector, "Action2", EActionStatus.kActionPending, "Action1", true);

                var files = dbWrapper.FileProcessingDB.GetFilesToProcess("Action2", 5, false, string.Empty)
                            .ToIEnumerable<IFileRecord>()
                            .Select(fileRecord => fileRecord.FileID)
                            .ToList();
                Assert.That(files.Contains(1) && files.Count == 1);
            }
            catch (COMException cex)
            {
                throw ExtractException.FromStringizedByteStream("ELI51682", cex.Message);
            }
        }

        [Test, Category("Automated")]
        [Parallelizable(ParallelScope.All)]
        public static void TestGetFilesToProcessAdvanced(
            [Values(0, 1)] int workflowCount, // TODO: Add support for 2+ workflows
            [Values(true)] bool enableLoadBalancing,
            [Values(true)] bool allWorkflows, // TODO: Add support for 2+ workflows
            [Values(false)] bool getSkipped,  // TODO: Add support
            [Values(1, 2)] int priorityCount,
            [Values(1, 10, 100)] int fileCount,
            [Values(1, 5, 100)] int batchSize,
            [Values(false)] bool randomOrder, // TODO: Add support
            [Values] bool limitToUserQueue,
            [Values] bool includeFilesQueuedForOthers)
        {
            string testDBName = _testDbManager.GenerateDatabaseName();

            try
            {
                using var fpDB = new TestDatabase<TestFAMFileProcessing>(_testDbManager, testDBName,
                    workflowCount, actionCount: 2, enableLoadBalancing);

                fpDB.FileProcessingDB.ExecuteCommandQuery(
                    "INSERT INTO [FAMUser] ([UserName], [FullUserName]) VALUES ('User2','User Two')");

                var fileQueueDetails = new Dictionary<int, (FileProcessingDB workflow, EFilePriority priority, TestUser user)>();

                var allFiles = Enumerable.Range(1, fileCount).Select(id =>
                    {
                        var workflow = fpDB.Workflows[id % fpDB.Workflows.Length];
                        // Potential sets of priorities: {3}, {3,4}, {2,3,4}, {2,3,4,5}, {1,2,3,4,5}
                        EFilePriority priority = priorityCount switch
                        {
                            1 => EFilePriority.kPriorityNormal + id % priorityCount,
                            2 => EFilePriority.kPriorityNormal + id % priorityCount,
                            3 => EFilePriority.kPriorityBelowNormal + id % priorityCount,
                            4 => EFilePriority.kPriorityBelowNormal + id % priorityCount,
                            5 => EFilePriority.kPriorityLow + id % priorityCount,
                            _ => throw new ArgumentException("Invalid priority count")
                        };

                        Assert.AreEqual(id, fpDB.AddFakeFile(id, getSkipped, priority, workflow));

                        var user = (TestUser)(id % 3);
                        workflow.SetStatusForFileForUser(id,
                            workflow.GetActiveActionName(),
                            workflow.GetWorkflowID(), GetUserName(user),
                            EActionStatus.kActionPending,
                            vbQueueChangeIfProcessing: false,
                            vbAllowQueuedStatusOverride: false,
                            out var _);

                        fileQueueDetails[id] = (workflow, priority, user);
                        return id;
                    })
                    .ToList();

                var currentSession = allWorkflows ? fpDB.wfAll : fpDB.Workflows[0];

                int workflowId = currentSession.GetWorkflowID();
                string action = currentSession.GetActiveActionName();

                var expectedOrder = allFiles
                    .Where(id => documentQualifiesForUserQueueSettings(
                        fileQueueDetails[id].user, limitToUserQueue, includeFilesQueuedForOthers))
                    .OrderByDescending(id => fileQueueDetails[id].priority)
                    .ThenBy(id => id)
                    .ToArray();

                for (int batchIndex = 1; batchIndex <= fileCount; batchIndex += batchSize)
                {
                    // Use current workflow to get 50 files
                    var filesToProcess = currentSession.GetFilesToProcessAdvanced(
                        action, batchSize, getSkipped, "", randomOrder, limitToUserQueue, includeFilesQueuedForOthers)
                        .ToIEnumerable<IFileRecord>()
                        .Select(fileRecord => fileRecord.FileID)
                        .ToArray();

                    var expectedFiles = expectedOrder
                        .Skip(batchIndex - 1)
                        .Take(batchSize)
                        .ToArray();
                    Assert.True(expectedFiles.SequenceEqual(filesToProcess));
                    Assert.AreEqual(expectedFiles.Length, fpDB.wfAll.GetTotalProcessing());

                    foreach (var fileID in filesToProcess)
                    {
                        int fileWorkflow = fpDB.wfAll.GetWorkflowID(
                            fileQueueDetails[fileID].workflow.ActiveWorkflow);
                        currentSession.NotifyFileProcessed(fileID, action, fileWorkflow, vbAllowQueuedStatusOverride: false);
                    }
                }
            }
            catch (COMException cex)
            {
                throw ExtractException.FromStringizedByteStream("ELI53271", cex.Message);
            }
        }

        [Test, Category("Automated")]
        [Parallelizable(ParallelScope.All)]
        public static void TestUserSpecificQueue(
            [Values(0, 1)] int workflowCount,    // 0: Not using workflows, 1: single workflow
            [Values] bool allWorkflows,
            [Values] TestUser user,
            [Values] bool limitToUserQueue,
            [Values] bool includeFilesQueuedForOthers,
            // When file is already processing, override transition to C:
            // (null == don't override)
            [Values] TestUser? overrideForUser)

        {
            Assume.That(allWorkflows || workflowCount > 0,
                "N/A: Testing a specific workflow when no workflows exist is not meaningful");

            string testDBName = _testDbManager.GenerateDatabaseName();

            try
            {
                using var fpDB = new TestDatabase<TestFAMFileProcessing>(_testDbManager, testDBName,
                    workflowCount, actionCount: 1, enableLoadBalancing: true);

                fpDB.FileProcessingDB.ExecuteCommandQuery(
                    "INSERT INTO [FAMUser] ([UserName], [FullUserName]) VALUES ('User2','User Two')");

                var workflow = fpDB.Workflows[0];
                var session = allWorkflows ? fpDB.wfAll : fpDB.Workflows[0];
                var action = workflow.GetActiveActionName();

                Assert.AreEqual(1, fpDB.AddFakeFile(1, setAsSkipped: false));

                workflow.SetStatusForFileForUser(1,
                    action, workflow.GetWorkflowID(), GetUserName(user),
                    EActionStatus.kActionPending,
                    vbQueueChangeIfProcessing: false,
                    vbAllowQueuedStatusOverride: false,
                    out var _);

                var filesToProcess = session.GetFilesToProcessAdvanced(
                    workflow.GetActiveActionName(),
                    nMaxFiles: 1,
                    bGetSkippedFiles: false,
                    bstrSkippedForUserName: "",
                    bUseRandomIDForQueueOrder: false,
                    limitToUserQueue,
                    includeFilesQueuedForOthers);

                if (documentQualifiesForUserQueueSettings(user, limitToUserQueue, includeFilesQueuedForOthers))
                {
                    Assert.AreEqual(1, filesToProcess.Size());
                    Assert.AreEqual(1, workflow.GetTotalProcessing());

                    workflow.RunInSeparateSession(workflow.ActiveWorkflow, action, famDb =>
                    {
                        famDb.SetStatusForFileForUser(1,
                            action, workflow.GetWorkflowID(),
                            GetUserName(overrideForUser),
                            EActionStatus.kActionPending,
                            vbQueueChangeIfProcessing: overrideForUser != null,
                            vbAllowQueuedStatusOverride: false,
                            out var _);
                        return 0;
                    });

                    session.NotifyFileProcessed(1, action, workflow.GetWorkflowID(), vbAllowQueuedStatusOverride: true);

                    var expectedStatus = overrideForUser switch
                    {
                        null => EActionStatus.kActionCompleted,
                        TestUser.NoUser => EActionStatus.kActionPending,
                        TestUser.CurrentUser => EActionStatus.kActionPending,
                        TestUser.AnotherUser => EActionStatus.kActionPending,
                        _ => throw new ArgumentException("Invalid queueForUser")
                    };
                    Assert.AreEqual(expectedStatus, workflow.GetFileStatus(1, action, false));
                    if (overrideForUser != null)
                    {
                        using var results = workflow.GetQueryResults(
                            "SELECT [UserID] FROM [FileActionStatus] WHERE [FileID] = 1");

                        Assert.AreEqual(1, results.Rows.Count);
                        if (overrideForUser == TestUser.NoUser)
                        {
                            Assert.AreEqual(DBNull.Value, results.Rows[0].ItemArray[0]);
                        }
                        else
                        {
                            Assert.AreEqual(overrideForUser, (TestUser)results.Rows[0].ItemArray[0]);
                        }
                    }
                }
                else
                {
                    Assert.AreEqual(0, filesToProcess.Size());
                }
            }
            catch (COMException cex)
            {
                throw ExtractException.FromStringizedByteStream("ELI53276", cex.Message);
            }
        }

        /// <summary>
        /// Returns the Username associated with the ID's passed
        /// </summary>
        /// <param name="user">Id of user to return</param>
        /// <returns></returns>
        private static string GetUserName(TestUser? user)
        {
            return user switch
            {
                null => "",
                TestUser.NoUser => "",
                TestUser.CurrentUser => Environment.UserName,
                TestUser.AnotherUser => "User2",
                _ => throw new ArgumentException("Invalid TestUser")
            };
        }

        /// <summary>
        /// Helper for TestGetFilesToProcessAdvanced/TestUserSpecificQueue to confirm if files queued
        /// for the specified user should be returned by GetFilesToProcess.
        /// </summary>
        static bool documentQualifiesForUserQueueSettings(TestUser user, bool limitToUserQueue, bool includeFilesQueuedForOthers)
        {
            // Intentionally phrased this logic in a different way than in GetFilesToProcess to better confirm the logic.
            if (limitToUserQueue)
            {
                return user != TestUser.NoUser
                    && (includeFilesQueuedForOthers || user == TestUser.CurrentUser);
            }
            else // !limitToUserQueue
            {
                return includeFilesQueuedForOthers || user == TestUser.NoUser || user == TestUser.CurrentUser;
            }
        }

        /// <summary>
        /// Verify that stats are correctly recorded by SetStatusForFile
        /// </summary>
        [Test, Category("Automated")]
        [Parallelizable(ParallelScope.All)]
        [TestCase(false, TestName = "SetStatusForFile, Visible files")]
        [TestCase(true, TestName = "SetStatusForFile, Invisible files")]
        public static void SetStatusForFile(bool invisibleStats)
        {
            string testDBName = "Test_SetStatusForFile" + Guid.NewGuid().ToString();
            using var dbWrapper = new OneWorkflow<TestFAMFileProcessing>(_testDbManager, testDBName, false);
            var statsComparer = new StatisticsAsserter(dbWrapper.FileProcessingDB, invisibleStats, new[] { dbWrapper.Action1, dbWrapper.Action2 });
            void AssertStats(params ActionStatus[] expectedStatuses)
            {
                statsComparer.AssertStats(expectedStatuses);
            }

            dbWrapper.AddFakeFile(1, false, EFilePriority.kPriorityNormal);
            dbWrapper.AddFakeFile(2, false, EFilePriority.kPriorityNormal);

            // Confirm initial state
            if (invisibleStats)
            {
                // Confirm that after adding the files they will be pending in both actions but won't show up in Invisible stats
                AssertStats(new ActionStatus { },
                            new ActionStatus { });

                // Confirm that after marking the files deleted they will be pending in both actions in Invisible stats
                dbWrapper.FileProcessingDB.MarkFileDeleted(1, 1);
                dbWrapper.FileProcessingDB.MarkFileDeleted(2, 1);
                AssertStats(new ActionStatus { P = new[] { 1, 2 } },
                            new ActionStatus { P = new[] { 1, 2 } });
            }
            else
            {
                AssertStats(new ActionStatus { P = new[] { 1, 2 } },
                            new ActionStatus { P = new[] { 1, 2 } });
            }

            // Set file 1 to unattempted in action 1
            dbWrapper.FileProcessingDB.SetStatusForFile(1, dbWrapper.Action1, 1, EActionStatus.kActionUnattempted, false, false, out var _);
            AssertStats(new ActionStatus { P = new[] { 2 } },
                        new ActionStatus { P = new[] { 1, 2 } });

            // Set file 1 to pending in action 1
            dbWrapper.FileProcessingDB.SetStatusForFile(1, dbWrapper.Action1, 1, EActionStatus.kActionPending, false, false, out var _);
            AssertStats(new ActionStatus { P = new[] { 1, 2 } },
                        new ActionStatus { P = new[] { 1, 2 } });

            // Set file 2 to processing in action 2
            dbWrapper.FileProcessingDB.SetStatusForFile(2, dbWrapper.Action2, 1, EActionStatus.kActionProcessing, false, false, out var _);
            AssertStats(new ActionStatus { P = new[] { 1, 2 } },
                        new ActionStatus { P = new[] { 1 }, R = new[] { 2 } });

            // Set file 2 to completed in action 2
            dbWrapper.FileProcessingDB.SetStatusForFile(2, dbWrapper.Action2, 1, EActionStatus.kActionCompleted, false, false, out var _);
            AssertStats(new ActionStatus { P = new[] { 1, 2 } },
                        new ActionStatus { P = new[] { 1 }, C = new[] { 2 } });

            // Set file 1 to skipped in action 1
            dbWrapper.FileProcessingDB.SetStatusForFile(1, dbWrapper.Action1, 1, EActionStatus.kActionSkipped, false, false, out var _);
            AssertStats(new ActionStatus { P = new[] { 2 }, S = new[] { 1 } },
                        new ActionStatus { P = new[] { 1 }, C = new[] { 2 } });

            // Set file 1 to failed in action 1
            dbWrapper.FileProcessingDB.SetStatusForFile(1, dbWrapper.Action1, 1, EActionStatus.kActionFailed, false, false, out var _);
            AssertStats(new ActionStatus { P = new[] { 2 }, F = new[] { 1 } },
                        new ActionStatus { P = new[] { 1 }, C = new[] { 2 } });
        }

        /// <summary>
        /// Verify that stats are correctly recorded by various SetFileStatusTo... and NotifyFile... methods
        /// </summary>
        [Parallelizable(ParallelScope.All)]
        [Category("Automated")]
        [TestCase(false, TestName = "SetFileStatusTo, Visible files")]
        [TestCase(true, TestName = "SetFileStatusTo, Invisible files")]
        public static void SetFileStatusTo(bool invisibleStats)
        {
            string testDBName = "Test_SetFileStatusTo" + Guid.NewGuid().ToString();
            using var dbWrapper = new OneWorkflow<TestFAMFileProcessing>(_testDbManager, testDBName, false);
            var statsComparer = new StatisticsAsserter(dbWrapper.FileProcessingDB, invisibleStats, new[] { dbWrapper.Action1, dbWrapper.Action2 });
            void AssertStats(params ActionStatus[] expectedStatuses)
            {
                statsComparer.AssertStats(expectedStatuses);
            }

            dbWrapper.AddFakeFile(1, false, EFilePriority.kPriorityNormal);
            dbWrapper.AddFakeFile(2, false, EFilePriority.kPriorityNormal);

            // Confirm initial state
            if (invisibleStats)
            {
                // Confirm that after adding the files they will be pending in both actions but won't show up in Invisible stats
                statsComparer.AssertStats(new ActionStatus { },
                            new ActionStatus { });

                // Confirm that after marking the files deleted they will be pending in both actions in Invisible stats
                dbWrapper.FileProcessingDB.MarkFileDeleted(1, 1);
                dbWrapper.FileProcessingDB.MarkFileDeleted(2, 1);
                AssertStats(new ActionStatus { P = new[] { 1, 2 } },
                            new ActionStatus { P = new[] { 1, 2 } });
            }
            else
            {
                AssertStats(new ActionStatus { P = new[] { 1, 2 } },
                            new ActionStatus { P = new[] { 1, 2 } });
            }


            // Set file 1 to unattempted in action 1
            dbWrapper.FileProcessingDB.SetFileStatusToUnattempted(1, dbWrapper.Action1, false);
            AssertStats(new ActionStatus { P = new[] { 2 } },
                        new ActionStatus { P = new[] { 1, 2 } });

            // Set file 1 to pending in action 1
            dbWrapper.FileProcessingDB.SetFileStatusToPending(1, dbWrapper.Action1, false);
            AssertStats(new ActionStatus { P = new[] { 1, 2 } },
                        new ActionStatus { P = new[] { 1, 2 } });

            // Set file 2 to processing in action 2
            dbWrapper.FileProcessingDB.SetFileStatusToProcessing(2, dbWrapper.FileProcessingDB.GetActionID(dbWrapper.Action2));
            AssertStats(new ActionStatus { P = new[] { 1, 2 } },
                        new ActionStatus { P = new[] { 1 }, R = new[] { 2 } });

            // Set file 2 to completed in action 2
            dbWrapper.FileProcessingDB.NotifyFileProcessed(2, dbWrapper.Action2, 1, false);
            AssertStats(new ActionStatus { P = new[] { 1, 2 } },
                        new ActionStatus { P = new[] { 1 }, C = new[] { 2 } });

            // Set file 1 to skipped in action 1
            dbWrapper.FileProcessingDB.SetFileStatusToSkipped(1, dbWrapper.Action1, false, false);
            AssertStats(new ActionStatus { P = new[] { 2 }, S = new[] { 1 } },
                        new ActionStatus { P = new[] { 1 }, C = new[] { 2 } });

            // Set file 2 to skipped in action 1 with alternate method
            dbWrapper.FileProcessingDB.NotifyFileSkipped(2, dbWrapper.Action1, 1, false);
            AssertStats(new ActionStatus { S = new[] { 1, 2 } },
                        new ActionStatus { P = new[] { 1 }, C = new[] { 2 } });

            // Set file 1 to failed in action 1
            dbWrapper.FileProcessingDB.NotifyFileFailed(1, dbWrapper.Action1, 1, null, false);
            AssertStats(new ActionStatus { S = new[] { 2 }, F = new[] { 1 } },
                        new ActionStatus { P = new[] { 1 }, C = new[] { 2 } });
        }

        [Test, Category("Automated")]
        /// Confirm that a new database has the expected password complexity requirements
        public static void DefaultPasswordComplexityRequirements()
        {
            // Defaults are different if the database is created at Extract (with mapped drive to correct server for \\extract.local\All)
            string expected = SystemMethods.IsExtractInternal()
                ? "1"
                : "8ULD";

            string testDBName = "Test_DefaultPasswordComplexityRequirements";
            using var dbWrapper = new OneWorkflow<TestFAMFileProcessing>(_testDbManager, testDBName, false);
            string actual = dbWrapper.FileProcessingDB.GetDBInfoSetting("PasswordComplexityRequirements", false);

            Assert.AreEqual(expected, actual);
        }

        #endregion Test Methods
    }
}
