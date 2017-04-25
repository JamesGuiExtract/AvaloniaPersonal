using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
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

        static readonly string _ACTION1 = "Action1";
        static readonly string _ACTION2 = "Action2";
        static readonly string _ACTION3 = "Action3";

        static readonly string _LABDE_ACTION1 = "A01_ExtractData";
        static readonly string _LABDE_ACTION2 = "A02_Verify";
        static readonly string _LABDE_ACTION3 = "A03_QA";
        static readonly string _LABDE_ACTION4 = "A04_SendToEMR";

        static readonly string _CLEANUP_ACTION = "Cleanup";

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
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestFAMFileProcessing>();
            _testDbManager = new FAMTestDBManager<TestFAMFileProcessing>();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestFixtureTearDown]
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

                int workflowId1 = fileProcessingDb.AddWorkflow("Workflow1", EWorkflowType.kUndefined);
                fileProcessingDb.SetWorkflowActions(workflowId1, new[] { _LABDE_ACTION1 }.ToVariantVector());
                int actionId = fileProcessingDb.GetActionID(_LABDE_ACTION1);

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

                int workflowId2 = fileProcessingDb.AddWorkflow("Workflow2", EWorkflowType.kUndefined);
                fileProcessingDb.SetWorkflowActions(workflowId2, new[] { _LABDE_ACTION1 }.ToVariantVector());
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
                int workflowID = fileProcessingDb.AddWorkflow("Workflow1", EWorkflowType.kUndefined);
                fileProcessingDb.SetWorkflowActions(workflowID, new[] { _ACTION1 }.ToVariantVector());
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

                int workflow1ID = fileProcessingDb.AddWorkflow("Workflow1", EWorkflowType.kUndefined);
                fileProcessingDb.SetWorkflowActions(workflow1ID, new[] { _LABDE_ACTION1, _LABDE_ACTION2 }.ToVariantVector());
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                int actionExtract1 = fileProcessingDb.GetActionID(_LABDE_ACTION1);
                int actionVerify1 = fileProcessingDb.GetActionID(_LABDE_ACTION2);

                int workflow2ID = fileProcessingDb.AddWorkflow("Workflow2", EWorkflowType.kUndefined);
                fileProcessingDb.SetWorkflowActions(workflow2ID, new[] { _LABDE_ACTION2, _LABDE_ACTION3 }.ToVariantVector());
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

        /// <summary>
        /// Tests the workflow statistics.
        /// </summary>
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

        /// <summary>
        /// Tests the workflow statistics.
        /// </summary>
        [Test, Category("Automated")]
        public static void GetFilesToProcessAndNotifyWithWorkflows()
        {
            string testDbName = "Test_GetFilesToProcessAndNotifyWithWorkflows";

            try
            {
                string testFileName1 = _testFiles.GetFile(_LABDE_TEST_FILE1);
                string testFileName2 = _testFiles.GetFile(_LABDE_TEST_FILE2);
                var fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                int workflowID1 = fileProcessingDb.AddWorkflow("Workflow1", EWorkflowType.kUndefined);
                fileProcessingDb.SetWorkflowActions(workflowID1,
                    new[] { _LABDE_ACTION1, _LABDE_ACTION2 }.ToVariantVector());
                int workflowID2 = fileProcessingDb.AddWorkflow("Workflow2", EWorkflowType.kUndefined);
                fileProcessingDb.SetWorkflowActions(workflowID2,
                    new[] { _LABDE_ACTION1, _LABDE_ACTION2 }.ToVariantVector());

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
                fileProcessingDb.SetWorkflowActions(workflowID,
                    new[] { _LABDE_ACTION1, _LABDE_ACTION2, _LABDE_ACTION3, _LABDE_ACTION4, _CLEANUP_ACTION }.ToVariantVector());
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
                fileProcessingDb.GetWorkflowStatusAllFiles(
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
                fileProcessingDb.GetWorkflowStatusAllFiles(
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
                fileProcessingDb.GetWorkflowStatusAllFiles(
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
                    fileProcessingDb, _LABDE_ACTION1, "", setStatusTask))
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
                    fileProcessingDb, _LABDE_ACTION1, "", setStatusTask,
                    threadCount: 2, filesToGrabCount: 2))
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
                int workflowID1 = fileProcessingDb.AddWorkflow("Workflow1", EWorkflowType.kUndefined);
                fileProcessingDb.SetWorkflowActions(workflowID1, new[] { _LABDE_ACTION1, _LABDE_ACTION2 }.ToVariantVector());
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                int extractAction1 = fileProcessingDb.GetActionID(_LABDE_ACTION1);

                int workflowID2 = fileProcessingDb.AddWorkflow("Workflow2", EWorkflowType.kUndefined);
                fileProcessingDb.SetWorkflowActions(workflowID2, new[] { _LABDE_ACTION1, _LABDE_ACTION2 }.ToVariantVector());
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
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsPending == 0);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsComplete == 1);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION2, false).NumDocumentsPending == 1);

                Assert.That(fileProcessingDb.GetStats(extractAction2, false).NumDocumentsComplete == 0);
                Assert.That(fileProcessingDb.GetStats(verifyAction2, false).NumDocumentsPending == 0);

                // Queue another file to workflow1, then the original file to workflow2, then test that both
                // processing in a session configured to run on all workflows
                fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION1, workflowID1, EFilePriority.kPriorityNormal,
                    false, false, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);

                fileProcessingDb.ActiveWorkflow = "Workflow2";
                fileProcessingDb.AddFile(testFileName1, _LABDE_ACTION1, workflowID2, EFilePriority.kPriorityNormal,
                    false, false, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);

                Assert.That(fileProcessingDb.GetStats(extractAction1, false).NumDocumentsPending == 1);
                Assert.That(fileProcessingDb.GetStats(extractAction2, false).NumDocumentsPending == 1);

                using (var famSession = new FAMProcessingSession(
                    fileProcessingDb, _LABDE_ACTION1, "", setStatusTask))
                {
                    famSession.WaitForProcessingToComplete();
                }

                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsPending == 0);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).NumDocumentsComplete == 3);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION2, false).NumDocumentsPending == 3);
            }
            finally
            {
                _testFiles.RemoveFile(_LABDE_TEST_FILE1);
                _testFiles.RemoveFile(_LABDE_TEST_FILE2);
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
                    int workflowID = fileProcessingDb.AddWorkflow(workflowName, EWorkflowType.kUndefined);
                    fileProcessingDb.SetWorkflowActions(workflowID, new[] { _LABDE_ACTION1 }.ToVariantVector());

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
                        fileProcessingDb, _LABDE_ACTION1, "", sleepTask,
                        threadCount: 1, filesToGrabCount: 1, keepProcessing: false, docsToProcess: 1))
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

        #endregion Test Methods
    }
}
