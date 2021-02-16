using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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

                // The actionID associated with the workflow should have a file queued, but not the "NULL" workflow action.
                Assert.That(fileProcessingDb.GetStats(actionId, false).NumDocumentsPending == 0);
                Assert.That(fileProcessingDb.GetStats(actionIdWorkflow1, false).NumDocumentsPending == 1);
                Assert.That(fileProcessingDb.GetStats(actionIdWorkflow2, false).NumDocumentsPending == 1);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsPending == 2); // The file will be counted separately in each workflow

                // Test that after removing the file there are no longer any files pending on the workflow's action.
                fileProcessingDb.RemoveFile(testFileName, _LABDE_ACTION1);

                Assert.That(fileProcessingDb.GetStats(actionId, false).NumDocumentsPending == 0);
                Assert.That(fileProcessingDb.GetStats(actionIdWorkflow1, false).NumDocumentsPending == 1);
                Assert.That(fileProcessingDb.GetStats(actionIdWorkflow2, false).NumDocumentsPending == 0);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsPending == 1);
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
                fileProcessingDb.ExecuteCommandQuery("UPDATE [DBInfo] SET [Value] = '1' WHERE [Name] = 'AutoCreateActions'");
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
                fileProcessingDb.ExecuteCommandQuery("UPDATE [DBInfo] SET [Value] = '1' WHERE [Name] = 'AutoCreateActions'");
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

                fileProcessingDb.ExecuteCommandQuery("UPDATE [DBInfo] SET [Value] = '1' WHERE [Name] = 'AutoCreateActions'");
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

            try
            {
                string testFileName1 = _testFiles.GetFile(_LABDE_TEST_FILE1);
                string testFileName2 = _testFiles.GetFile(_LABDE_TEST_FILE2);
                string testFileName3 = _testFiles.GetFile(_LABDE_TEST_FILE3);
                var fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

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

            try
            {
                string testFileName1 = _testFiles.GetFile(_LABDE_TEST_FILE1);
                string testFileName2 = _testFiles.GetFile(_LABDE_TEST_FILE2);
                var fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);
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

            try
            {
                string testFileName1 = _testFiles.GetFile(_LABDE_TEST_FILE1);
                string testFileName2 = _testFiles.GetFile(_LABDE_TEST_FILE2);
                string testFileName3 = _testFiles.GetFile(_LABDE_TEST_FILE3);
                var fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);
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
                var sleepTask  = (IFileProcessingTask)sleepTaskConfig;

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

                    fileProcessingDb.AddFile(tmpFile.FileName, _LABDE_ACTION1, i%3 + 1, EFilePriority.kPriorityNormal, false,
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
                    threadCount: 4, filesToGrab: 1, keepProcessing: false, docsToProcess:300))
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
        /// Tests that a file is considered available for processing even if it is processing in an unrelated action
        /// </summary>
        [Test, Category("Automated")]
        [Parallelizable(ParallelScope.All)]
        [TestCase(false, 1, false, TestName = "GetFilesToProcess: Can get pending file when it is processing in unrelated action; max of 1 with no load balancing")]
        [TestCase(false, 1, true, TestName = "GetFilesToProcess: Can get pending file when it is processing in unrelated action; max of 1 with load balancing")]
        [TestCase(false, 5, false, TestName = "GetFilesToProcess: Can get pending file when it is processing in unrelated action; max of 5 with no load balancing")]
        [TestCase(false, 5, true, TestName = "GetFilesToProcess: Can get pending file when it is processing in unrelated action; max of 5 with load balancing")]
        [TestCase(true, 1, false, TestName = "GetFilesToProcess: Can get skipped file when it is processing in unrelated action; max of 1 with no load balancing")]
        [TestCase(true, 1, true, TestName = "GetFilesToProcess: Can get skipped file when it is processing in unrelated action; max of 1 with load balancing")]
        [TestCase(true, 5, false, TestName = "GetFilesToProcess: Can get skipped file when it is processing in unrelated action; max of 5 with no load balancing")]
        [TestCase(true, 5, true, TestName = "GetFilesToProcess: Can get skipped file when it is processing in unrelated action; max of 5 with load balancing")]
        public static void Test_GetFilesToProcess_Can_Get_File_When_It_Is_Processing_In_Unrelated_Action(
            bool getSkipped, int maxFilesToGet, bool enableLoadBalancing)
        {
            EActionStatus statusToGetFrom = getSkipped ? EActionStatus.kActionSkipped : EActionStatus.kActionPending;
            string testDBName = Guid.NewGuid().ToString();

            try
            {
                using var fpDB = new TwoWorkflows(testDBName, enableLoadBalancing);

                // Add two files and set to pending (or skipped) for action 1 and 2 in workflow 1 and 2
                int testFile1 = fpDB.addFakeFile(1, getSkipped);
                fpDB.addFakeFile(2, getSkipped);

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
        [TestCase(false, 1, false, TestName = "GetFilesToProcess: Do not get pending file for AllWorkflows when it is processing in related action; max of 1 with no load balancing")]
        [TestCase(false, 1, true, TestName = "GetFilesToProcess: Do not get pending file for AllWorkflows when it is processing in related action; max of 1 with load balancing")]
        [TestCase(false, 5, false, TestName = "GetFilesToProcess: Do not get pending file for AllWorkflows when it is processing in related action; max of 5 with no load balancing")]
        [TestCase(false, 5, true, TestName = "GetFilesToProcess: Do not get pending file for AllWorkflows when it is processing in related action; max of 5 with load balancing")]
        [TestCase(true, 1, false, TestName = "GetFilesToProcess: Do not get skipped file for AllWorkflows when it is processing in related action; max of 1 with no load balancing")]
        [TestCase(true, 1, true, TestName = "GetFilesToProcess: Do not get skipped file for AllWorkflows when it is processing in related action; max of 1 with load balancing")]
        [TestCase(true, 5, false, TestName = "GetFilesToProcess: Do not get skipped file for AllWorkflows when it is processing in related action; max of 5 with no load balancing")]
        [TestCase(true, 5, true, TestName = "GetFilesToProcess: Do not get skipped file for AllWorkflows when it is processing in related action; max of 5 with load balancing")]
        public static void GetFilesToProcess_Do_Not_Get_File_For_AllWorkflows_When_It_Is_Processing_In_Related_Action(
            bool getSkipped, int maxFilesToGet, bool enableLoadBalancing)
        {
            EActionStatus statusToGetFrom = getSkipped ? EActionStatus.kActionSkipped : EActionStatus.kActionPending;
            string testDBName = Guid.NewGuid().ToString();

            try
            {
                using var fpDB = new TwoWorkflows(testDBName, enableLoadBalancing);

                // Add two files and set to pending (or skipped) for action 1 and 2 in workflow 1 and 2
                int testFile1 = fpDB.addFakeFile(1, getSkipped);
                fpDB.addFakeFile(2, getSkipped);

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
        [TestCase(false, 1, 1, false, TestName = "GetFilesToProcess: Can get one copy of pending file for AllWorkflows when it is available in two workflows; max 1/1 with no load balancing")]
        [TestCase(false, 1, 1, true, TestName = "GetFilesToProcess: Can get one copy of pending file for AllWorkflows when it is available in two workflows; max 1/1 with load balancing")]
        [TestCase(false, 1, 2, false, TestName = "GetFilesToProcess: Can get one copy of pending file for AllWorkflows when it is available in two workflows; max 1/2 with no load balancing")]
        [TestCase(false, 1, 2, true, TestName = "GetFilesToProcess: Can get one copy of pending file for AllWorkflows when it is available in two workflows; max 1/2 with load balancing")]
        [TestCase(false, 5, 5, false, TestName = "GetFilesToProcess: Can get one copy of pending file for AllWorkflows when it is available in two workflows; max 5/5 with no load balancing")]
        [TestCase(false, 5, 5, true, TestName = "GetFilesToProcess: Can get one copy of pending file for AllWorkflows when it is available in two workflows; max 5/5 with load balancing")]
        [TestCase(false, 5, 10, false, TestName = "GetFilesToProcess: Can get one copy of pending file for AllWorkflows when it is available in two workflows; max 5/10 with no load balancing")]
        [TestCase(false, 5, 10, true, TestName = "GetFilesToProcess: Can get one copy of pending file for AllWorkflows when it is available in two workflows; max 5/10 with load balancing")]
        [TestCase(true, 1, 1, false, TestName = "GetFilesToProcess: Can get one copy of skipped file for AllWorkflows when it is available in two workflows; max 1/1 with no load balancing")]
        [TestCase(true, 1, 1, true, TestName = "GetFilesToProcess: Can get one copy of skipped file for AllWorkflows when it is available in two workflows; max 1/1 with load balancing")]
        [TestCase(true, 1, 2, false, TestName = "GetFilesToProcess: Can get one copy of skipped file for AllWorkflows when it is available in two workflows; max 1/2 with no load balancing")]
        [TestCase(true, 1, 2, true, TestName = "GetFilesToProcess: Can get one copy of skipped file for AllWorkflows when it is available in two workflows; max 1/2 with load balancing")]
        [TestCase(true, 5, 5, false, TestName = "GetFilesToProcess: Can get one copy of skipped file for AllWorkflows when it is available in two workflows; max 5/5 with no load balancing")]
        [TestCase(true, 5, 5, true, TestName = "GetFilesToProcess: Can get one copy of skipped file for AllWorkflows when it is available in two workflows; max 5/5 with load balancing")]
        [TestCase(true, 5, 10, false, TestName = "GetFilesToProcess: Can get one copy of skipped file for AllWorkflows when it is available in two workflows; max 5/10 with no load balancing")]
        [TestCase(true, 5, 10, true, TestName = "GetFilesToProcess: Can get one copy of skipped file for AllWorkflows when it is available in two workflows; max 5/10 with load balancing")]
        public static void GetFilesToProcess_Can_Get_File_For_AllWorkflows_When_It_Is_Available_In_Two_Workflows(
            bool getSkipped, int maxFilesToGet, int filesInDB, bool enableLoadBalancing)
        {
            EActionStatus statusToGetFrom = getSkipped ? EActionStatus.kActionSkipped : EActionStatus.kActionPending;
            string testDBName = Guid.NewGuid().ToString();

            try
            {
                using var fpDB = new TwoWorkflows(testDBName, enableLoadBalancing);

                // Add files and set to pending (or skipped) for action 1 and 2 in workflow 1 and 2
                var fileNumbers = Enumerable.Range(1, filesInDB).ToArray();
                foreach (var fileNumber in fileNumbers)
                {
                    int fileID = fpDB.addFakeFile(fileNumber, getSkipped);
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
                CollectionAssert.AreEquivalent(expectedFiles, filesToProcess);
            }
            catch (COMException cex)
            {
                throw ExtractException.FromStringizedByteStream("ELI51554", cex.Message);
            }
        }

        [Test, Category("Automated")]
        [Parallelizable(ParallelScope.All)]
        [TestCase(1, false, false, TestName = "GetFilesToProcess: Respect workflow and priority; workflow 1, pending files, no load balancing")]
        [TestCase(1, false, true, TestName = "GetFilesToProcess: Respect workflow and priority; workflow 1, pending files, load balancing")]
        [TestCase(1, true, false, TestName = "GetFilesToProcess: Respect workflow and priority; workflow 1, skipped files, no load balancing")]
        [TestCase(1, true, true, TestName = "GetFilesToProcess: Respect workflow and priority; workflow 1, skipped files, load balancing")]
        [TestCase(2, false, false, TestName = "GetFilesToProcess: Respect workflow and priority; workflow 2, pending files, no load balancing")]
        [TestCase(2, false, true, TestName = "GetFilesToProcess: Respect workflow and priority; workflow 2, pending files, load balancing")]
        [TestCase(2, true, false, TestName = "GetFilesToProcess: Respect workflow and priority; workflow 2, skipped files, no load balancing")]
        [TestCase(2, true, true, TestName = "GetFilesToProcess: Respect workflow and priority; workflow 2, skipped files, load balancing")]
        public static void GetFilesToProcess_Respect_Workflow_And_Priority(int workflow, bool getSkipped, bool enableLoadBalancing)
        {
            string testDBName = Guid.NewGuid().ToString();

            try
            {
                using var fpDB = new TwoWorkflows(testDBName, enableLoadBalancing);

                // Add 100 files with normal priority and set to pending (or skipped) for action 1 and 2 in workflow 1 and 2
                for (int i = 1; i <= 100; i++)
                {
                    fpDB.addFakeFile(i, getSkipped);
                }
                // Add 100 files with above normal priority and set to pending (or skipped) for action 1 and 2 in workflow 1 and 2
                for (int i = 101; i <= 200; i++)
                {
                    fpDB.addFakeFile(i, getSkipped, EFilePriority.kPriorityAboveNormal);
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


        #endregion Test Methods

        #region Helper Class

        class TwoWorkflows : IDisposable
        {
            public readonly string action1 = "Action1";
            public readonly string action2 = "Action2";

            public readonly FileProcessingDB wf1 = null;
            public readonly FileProcessingDB wf2 = null;
            public readonly FileProcessingDB wfAll = null;

            readonly FileProcessingDB[] fpDBs = null;
            readonly string testDBName;

            public TwoWorkflows(string testDBName, bool enableLoadBalancing)
            {
                this.testDBName = testDBName;

                // Setup DB
                wf1 = _testDbManager.GetNewDatabase(testDBName);
                wf1.SetDBInfoSetting("EnableLoadBalancing", enableLoadBalancing ? "1" : "0", true, false);
                wf1.DefineNewAction(action1);
                wf1.DefineNewAction(action2);

                int workflow1 = wf1.AddWorkflow("Workflow1", EWorkflowType.kUndefined, action1, action2);
                Assert.AreEqual(1, workflow1);
                int workflow2 = wf1.AddWorkflow("Workflow2", EWorkflowType.kUndefined, action1, action2);
                Assert.AreEqual(2, workflow2);

                // Configure a separate object for each workflow configuration needed
                wf1.ActiveWorkflow = "Workflow1";
                wf2 = new FileProcessingDBClass
                {
                    DatabaseServer = wf1.DatabaseServer,
                    DatabaseName = wf1.DatabaseName,
                    ActiveWorkflow = "Workflow2"
                };
                wfAll = new FileProcessingDBClass
                {
                    DatabaseServer = wf1.DatabaseServer,
                    DatabaseName = wf1.DatabaseName,
                    ActiveWorkflow = ""
                };

                // Start a session for each DB object
                fpDBs = new[] { wf1, wf2, wfAll };
                foreach (var fpDB in fpDBs)
                {
                    fpDB.RecordFAMSessionStart("Test.fps", action1, true, true);
                    fpDB.RegisterActiveFAM();
                }
            }

            public int addFakeFile(int fileNumber, bool setAsSkipped, EFilePriority priority = EFilePriority.kPriorityNormal)
            {
                var fileName = Path.Combine(Path.GetTempPath(), fileNumber.ToString("N3", CultureInfo.InvariantCulture) + ".tif");
                int fileID = wf1.AddFileNoQueue(fileName, 0, 0, priority, 1);
                if (setAsSkipped)
                {
                    wf1.SetFileStatusToSkipped(fileID, action1, false, false);
                    wf1.SetFileStatusToSkipped(fileID, action2, false, false);
                    wf2.SetFileStatusToSkipped(fileID, action1, false, false);
                    wf2.SetFileStatusToSkipped(fileID, action2, false, false);
                }
                else
                {
                    wf1.SetFileStatusToPending(fileID, action1, false);
                    wf1.SetFileStatusToPending(fileID, action2, false);
                    wf2.SetFileStatusToPending(fileID, action1, false);
                    wf2.SetFileStatusToPending(fileID, action2, false);
                }

                return fileID;
            }

            public void startNewSession(FileProcessingDB fpDB)
            {
                fpDB.UnregisterActiveFAM();
                fpDB.RecordFAMSessionStop();
                fpDB.RecordFAMSessionStart("Test.fps", action1, true, true);
                fpDB.RegisterActiveFAM();
            }

            public void Dispose()
            {
                foreach (var fpDB in fpDBs)
                {
                    if (fpDB != null)
                    {
                        try
                        {
                            // Prevent 'files were reverted' log
                            fpDB.SetStatusForAllFiles(action1, EActionStatus.kActionUnattempted);
                            fpDB.SetStatusForAllFiles(action2, EActionStatus.kActionUnattempted);
                            fpDB.UnregisterActiveFAM();
                            fpDB.RecordFAMSessionStop();
                        }
                        catch { }
                    }
                }
                _testDbManager.RemoveDatabase(testDBName);
            }

            public int getTotalProcessing()
            {
                var action1Stats = wfAll.GetStatsAllWorkflows(action1, false);
                var action2Stats = wfAll.GetStatsAllWorkflows(action2, false);
                int totalFilesInDB = action1Stats.NumDocuments + action2Stats.NumDocuments;
                int notProcessing = 0;
                notProcessing += action1Stats.NumDocumentsSkipped;
                notProcessing += action2Stats.NumDocumentsSkipped;
                notProcessing += action1Stats.NumDocumentsPending;
                notProcessing += action2Stats.NumDocumentsPending;
                notProcessing += action1Stats.NumDocumentsComplete;
                notProcessing += action2Stats.NumDocumentsComplete;
                notProcessing += action1Stats.NumDocumentsFailed;
                notProcessing += action2Stats.NumDocumentsFailed;
                return totalFilesInDB - notProcessing;
            }
        }
        #endregion Helper Class
    }
}
