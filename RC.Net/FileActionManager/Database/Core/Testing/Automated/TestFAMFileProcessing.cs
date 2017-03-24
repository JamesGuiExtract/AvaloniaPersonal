using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using UCLID_FILEPROCESSORSLib;

namespace Extract.FileActionManager.Database.Test
{
    /// <summary>
    /// Testing class for the workflow management related methods in IFileProcessingDB.
    /// </summary>
    [Category("TestFAMFileProcessing")]
    [TestFixture]
    public class TestFAMFileProcessing
    {
        #region Constants

        static readonly string _LABDE_EMPTY_DB = "Resources.Demo_LabDE_Empty";
        static readonly string _LABDE_TEST_FILE1 = "Resources.TestImage001.tif";
        static readonly string _LABDE_TEST_FILE2 = "Resources.TestImage002.tif";
        static readonly string _LABDE_TEST_FILE3 = "Resources.TestImage003.tif";

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
            string actionName = "A01_ExtractData";

            try
            {
                string testFileName = _testFiles.GetFile(_LABDE_TEST_FILE1);
                var fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                int actionId = fileProcessingDb.GetActionID(actionName);

                fileProcessingDb.RecordFAMSessionStart("Test.fps", actionName, true, false);

                var fileRecord = fileProcessingDb.AddFile(
                    testFileName, actionName, -1, EFilePriority.kPriorityNormal, false, false, EActionStatus.kActionPending, true,
                    out bool t1, out EActionStatus t2);
                int fileId = fileRecord.FileID;

                fileProcessingDb.SetFileStatusToPending(fileId, actionName, false);

                Assert.That(fileProcessingDb.GetStats(actionId, false).NumDocumentsPending == 1);

                fileProcessingDb.RemoveFile(testFileName, actionName);

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
            string actionName = "A01_ExtractData";

            try
            {
                string testFileName = _testFiles.GetFile(_LABDE_TEST_FILE1);
                var fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                int workflowId = fileProcessingDb.AddWorkflow("Workflow1", EWorkflowType.kUndefined);
                fileProcessingDb.SetWorkflowActions(workflowId, new[] { actionName }.ToVariantVector());
                int actionId = fileProcessingDb.GetActionID(actionName);

                fileProcessingDb.RecordFAMSessionStart("Test.fps", actionName, true, false);

                // Should not be able to queue a file without setting a workflow
                bool alreadyExists = false;
                EActionStatus previousStatus = EActionStatus.kActionUnattempted;
                Assert.Throws<COMException>(() => fileProcessingDb.AddFile(
                    testFileName, actionName, -1, EFilePriority.kPriorityNormal, false, false, EActionStatus.kActionPending, true,
                    out alreadyExists, out previousStatus));

                // Should not be able to change workflow during an active FAM Session.
                Assert.Throws<COMException>(() => fileProcessingDb.ActiveWorkflow = testDbName);

                fileProcessingDb.RecordFAMSessionStop();

                // Set active workflow which will correspond to a different action ID.
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                int actionIdWorkflow1 = fileProcessingDb.GetActionID(actionName);
                fileProcessingDb.RecordFAMSessionStart("Test.fps", actionName, true, true);

                // We should now be able to add a file now that we have a workflow set. (Even though
                // the workflow ID is not specified, active workflow should be assumed)
                var fileRecord = fileProcessingDb.AddFile(
                    testFileName, actionName, -1, EFilePriority.kPriorityNormal, false, false, EActionStatus.kActionPending, true,
                    out alreadyExists, out previousStatus);
                int fileId = fileRecord.FileID;

                fileProcessingDb.RecordFAMSessionStop();

                workflowId = fileProcessingDb.AddWorkflow("Workflow2", EWorkflowType.kUndefined);
                fileProcessingDb.SetWorkflowActions(workflowId, new[] { actionName }.ToVariantVector());
                fileProcessingDb.ActiveWorkflow = "Workflow2";
                int actionIdWorkflow2 = fileProcessingDb.GetActionID(actionName);

                fileProcessingDb.RecordFAMSessionStart("Test.fps", actionName, false, true);

                fileProcessingDb.SetFileStatusToPending(fileId, actionName, false);

                // The actionID associated with the workflow should have a file queued, but not the "NULL" workflow action.
                Assert.That(fileProcessingDb.GetStats(actionId, false).NumDocumentsPending == 0);
                Assert.That(fileProcessingDb.GetStats(actionIdWorkflow1, false).NumDocumentsPending == 1);
                Assert.That(fileProcessingDb.GetStats(actionIdWorkflow2, false).NumDocumentsPending == 1);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(actionName, false).NumDocumentsPending == 2); // The file will be counted separately in each workflow

                // Test that after removing the file there are no longer any files pending on the workflow's action.
                fileProcessingDb.RemoveFile(testFileName, actionName);

                Assert.That(fileProcessingDb.GetStats(actionId, false).NumDocumentsPending == 0);
                Assert.That(fileProcessingDb.GetStats(actionIdWorkflow1, false).NumDocumentsPending == 1);
                Assert.That(fileProcessingDb.GetStats(actionIdWorkflow2, false).NumDocumentsPending == 0);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows(actionName, false).NumDocumentsPending == 1);
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

                fileProcessingDb.DefineNewAction("Action1");
                fileProcessingDb.RecordFAMSessionStart("Test.fps", "Action1", true, false);

                bool alreadyExists = false;
                EActionStatus previousStatus = EActionStatus.kActionUnattempted;
                var fileRecord = fileProcessingDb.AddFile(testFileName, "Action1", -1,
                    EFilePriority.kPriorityNormal, true, false, EActionStatus.kActionPending, true,
                    out alreadyExists, out previousStatus);
                int fileId = fileRecord.FileID;

                // Without having yet set AutoCreateActions setting to true, AutoCreateActions should
                // fail and we should not be able to queue to an action that does not exist
                Assert.Throws<COMException>(() => fileProcessingDb.AutoCreateAction("Action2"));
                Assert.Throws<COMException>(() =>
                    fileProcessingDb.SetStatusForFile(fileId, "Action2", -1, EActionStatus.kActionPending, true, false, out previousStatus));

                // Turn on auto-create actions.
                fileProcessingDb.RecordFAMSessionStop();
                fileProcessingDb.ExecuteCommandQuery("UPDATE [DBInfo] SET [Value] = '1' WHERE [Name] = 'AutoCreateActions'");
                fileProcessingDb.RecordFAMSessionStart("Test.fps", "Action1", true, false);

                fileProcessingDb.AutoCreateAction("Action2");
                fileProcessingDb.SetStatusForFile(fileId, "Action2", -1, EActionStatus.kActionPending, true, false, out previousStatus);

                int actionID = fileProcessingDb.GetActionID("Action1");
                Assert.That(fileProcessingDb.GetStats(actionID, false).NumDocumentsPending == 1);

                actionID = fileProcessingDb.GetActionID("Action2");
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

                fileProcessingDb.DefineNewAction("Action1");
                fileProcessingDb.DefineNewAction("Action2");
                int workflowID = fileProcessingDb.AddWorkflow("Workflow1", EWorkflowType.kUndefined);
                fileProcessingDb.SetWorkflowActions(workflowID, new[] { "Action1" }.ToVariantVector());
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                fileProcessingDb.RecordFAMSessionStart("Test.fps", "Action1", true, false);

                bool alreadyExists = false;
                EActionStatus previousStatus = EActionStatus.kActionUnattempted;
                var fileRecord = fileProcessingDb.AddFile(testFileName, "Action1", workflowID,
                    EFilePriority.kPriorityNormal, true, false, EActionStatus.kActionPending, true,
                    out alreadyExists, out previousStatus);
                int fileId = fileRecord.FileID;

                // Without having yet set AutoCreateActions setting to true, AutoCreateActions should
                // fail and we should not be able to queue to an action that does not exist in the
                // workflow (whether or not it exists as whole).
                Assert.Throws<COMException>(() => fileProcessingDb.AutoCreateAction("Action2"));
                Assert.Throws<COMException>(() =>
                    fileProcessingDb.SetStatusForFile(fileId, "Action2", workflowID, EActionStatus.kActionPending, true, false, out previousStatus));

                // Turn on auto-create actions.
                fileProcessingDb.RecordFAMSessionStop();
                fileProcessingDb.ExecuteCommandQuery("UPDATE [DBInfo] SET [Value] = '1' WHERE [Name] = 'AutoCreateActions'");
                fileProcessingDb.RecordFAMSessionStart("Test.fps", "Action1", true, false);

                // Test creating an using an action that already exists in a different workflow.
                fileProcessingDb.AutoCreateAction("Action2");
                fileProcessingDb.SetStatusForFile(fileId, "Action2", workflowID, EActionStatus.kActionPending, true, false, out previousStatus);

                // Test creating an using an action that didn't already exist in a different workflow.
                fileProcessingDb.AutoCreateAction("Action3");
                fileProcessingDb.SetStatusForFile(fileId, "Action3", workflowID, EActionStatus.kActionPending, true, false, out previousStatus);

                // Action 1
                int actionID = fileProcessingDb.GetActionID("Action1");
                Assert.That(fileProcessingDb.GetStats(actionID, false).NumDocumentsPending == 1);

                actionID = fileProcessingDb.GetActionID("Action2");
                Assert.That(fileProcessingDb.GetStats(actionID, false).NumDocumentsPending == 1);

                actionID = fileProcessingDb.GetActionID("Action3");
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
                fileProcessingDb.SetWorkflowActions(workflow1ID, new[] { "A01_ExtractData", "A02_Verify" }.ToVariantVector());
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                int actionExtract1 = fileProcessingDb.GetActionID("A01_ExtractData");
                int actionVerify1 = fileProcessingDb.GetActionID("A02_Verify");

                int workflow2ID = fileProcessingDb.AddWorkflow("Workflow2", EWorkflowType.kUndefined);
                fileProcessingDb.SetWorkflowActions(workflow2ID, new[] { "A02_Verify", "A03_QA" }.ToVariantVector());
                fileProcessingDb.ActiveWorkflow = "Workflow2";
                int actionVerify2 = fileProcessingDb.GetActionID("A02_Verify");
                int actionQA2 = fileProcessingDb.GetActionID("A03_QA");

                fileProcessingDb.ActiveWorkflow = "Workflow1";
                fileProcessingDb.RecordFAMSessionStart("ExtractData.fps", "A01_ExtractData", true, false);

                // Workflow1: File 1 to Pending in A01_ExtractData and Complete in A02_Verify
                bool alreadyExists = false;
                EActionStatus previousStatus = EActionStatus.kActionUnattempted;
                var fileRecord = fileProcessingDb.AddFile(testFileName1, "A01_ExtractData", workflow1ID,
                    EFilePriority.kPriorityNormal, true, false, EActionStatus.kActionPending, false,
                    out alreadyExists, out previousStatus);
                int fileId = fileRecord.FileID;
                fileProcessingDb.SetStatusForFile(fileId, "A02_Verify", workflow1ID, EActionStatus.kActionCompleted, false, false, out previousStatus);

                fileProcessingDb.RecordFAMSessionStop();
                fileProcessingDb.ActiveWorkflow = "Workflow2";
                fileProcessingDb.RecordFAMSessionStart("Test.fps", "A02_Verify", true, false);

                // Workflow2: 
                // File 1, 2 and 3 to Pending in A02_Verify
                // File 3 to Skipped in A03_QA
                // File 3 to Failed in NewAction
                fileProcessingDb.SetStatusForFile(fileId, "A02_Verify", workflow2ID, EActionStatus.kActionPending, false, false, out previousStatus);
                fileRecord = fileProcessingDb.AddFile(testFileName2, "A02_Verify", workflow2ID,
                    EFilePriority.kPriorityNormal, true, false, EActionStatus.kActionPending, false,
                    out alreadyExists, out previousStatus);
                fileRecord = fileProcessingDb.AddFile(testFileName3, "A02_Verify", workflow2ID,
                    EFilePriority.kPriorityNormal, true, false, EActionStatus.kActionPending, false,
                    out alreadyExists, out previousStatus);
                fileId = fileRecord.FileID;
                fileProcessingDb.SetStatusForFile(fileId, "A03_QA", workflow2ID, EActionStatus.kActionSkipped, false, false, out previousStatus);

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
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows("A01_ExtractData", false).NumDocumentsPending == 1);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows("A01_ExtractData", false).NumPagesPending == 1);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows("A01_ExtractData", false).NumDocuments == 1);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows("A01_ExtractData", false).NumPages == 1);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows("A02_Verify", false).NumDocumentsPending == 3); // File 1 has been counted twice
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows("A02_Verify", false).NumPagesPending == 6);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows("A02_Verify", false).NumDocumentsComplete == 1);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows("A02_Verify", false).NumPagesComplete == 1);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows("A02_Verify", false).NumDocumentsFailed == 0);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows("A02_Verify", false).NumPagesFailed == 0);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows("A02_Verify", false).NumDocuments == 4); // File 1 has been counted twice
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows("A02_Verify", false).NumPages == 7);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows("A03_QA", false).NumDocumentsSkipped == 1);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows("A03_QA", false).NumPagesSkipped == 4);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows("A03_QA", false).NumDocuments == 1);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows("A03_QA", false).NumPages == 4);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows("NewAction", false).NumDocumentsFailed == 1);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows("NewAction", false).NumPagesFailed == 4);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows("NewAction", false).NumDocuments == 1);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows("NewAction", false).NumPages == 4);
                };

                // Check that stats are correct while still in an active workflow and FAMSession.
                checkStats();

                fileProcessingDb.RecordFAMSessionStop();
                fileProcessingDb.ActiveWorkflow = "";

                // Check again after stopping the session and clearing the active workflow.
                checkStats();
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
                int extractAction = fileProcessingDb.GetActionID("A01_ExtractData");
                fileProcessingDb.RecordFAMSessionStart("Test.fps", "A01_ExtractData", true, false);
                fileProcessingDb.RegisterActiveFAM();

                // Queue file 1 and 2 to pending in A01_ExtractData, file 3 to skipped.
                bool alreadyExists = false;
                EActionStatus previousStatus;
                var fileRecord = fileProcessingDb.AddFile(testFileName1, "A01_ExtractData",
                    -1, EFilePriority.kPriorityNormal, false, true,
                    EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);
                int fileId1 = fileRecord.FileID;
                fileRecord = fileProcessingDb.AddFile(testFileName2, "A01_ExtractData",
                    -1, EFilePriority.kPriorityNormal, false, true,
                    EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);
                int fileId2 = fileRecord.FileID;
                fileRecord = fileProcessingDb.AddFile(testFileName3, "A01_ExtractData",
                    -1, EFilePriority.kPriorityNormal, false, true,
                    EActionStatus.kActionSkipped, false, out alreadyExists, out previousStatus);
                int fileId3 = fileRecord.FileID;

                // Check that we get just the pending files
                var files = fileProcessingDb.GetFilesToProcess("A01_ExtractData", 10, false, "")
                    .ToIEnumerable<IFileRecord>()
                    .Select(record => record.FileID);
                Assert.That(files.SequenceEqual(new []{ fileId1, fileId2 }));

                // Simulate one file completing and the other being skipped.
                fileProcessingDb.NotifyFileProcessed(fileId1, "A01_ExtractData", -1, true);
                fileProcessingDb.NotifyFileSkipped(fileId2, "A01_ExtractData", -1, true);

                // Ensure GetFilesToProcess will not grab the skipped files in the same session.
                files = fileProcessingDb.GetFilesToProcess("A01_ExtractData", 10, true, "")
                    .ToIEnumerable<IFileRecord>()
                    .Select(record => record.FileID);
                Assert.That(files.Count() == 0);

                // Start a new FAM session
                fileProcessingDb.UnregisterActiveFAM();
                fileProcessingDb.RecordFAMSessionStop();
                fileProcessingDb.RecordFAMSessionStart("Test.fps", "A01_ExtractData", true, false);
                fileProcessingDb.RegisterActiveFAM();

                // Now we should get the skipped files.
                files = fileProcessingDb.GetFilesToProcess("A01_ExtractData", 10, true, "")
                    .ToIEnumerable<IFileRecord>()
                    .Select(record => record.FileID);
                Assert.That(files.SequenceEqual(new[] { fileId2, fileId3 }));

                var ee = new ExtractException("ELI42152", "Test");
                fileProcessingDb.NotifyFileFailed(fileId2, "A01_ExtractData", -1, ee.AsStringizedByteStream(), true);
                fileProcessingDb.NotifyFileSkipped(fileId3, "A01_ExtractData", -1, true);

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
                    new[] { "A01_ExtractData", "A02_Verify" }.ToVariantVector());
                int workflowID2 = fileProcessingDb.AddWorkflow("Workflow2", EWorkflowType.kUndefined);
                fileProcessingDb.SetWorkflowActions(workflowID2,
                    new[] { "A01_ExtractData", "A02_Verify" }.ToVariantVector());

                // Start processing in Workflow 1
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                int extractActionId1 = fileProcessingDb.GetActionID("A01_ExtractData");
                fileProcessingDb.RecordFAMSessionStart("Test.fps", "A01_ExtractData", true, false);
                fileProcessingDb.RegisterActiveFAM();

                // In Workflow 1, queue file 1 to pending in A01_ExtractData, and check that
                // GetFilesToProcess grabs it.
                bool alreadyExists = false;
                EActionStatus previousStatus;
                var fileRecord = fileProcessingDb.AddFile(testFileName1, "A01_ExtractData",
                    workflowID1, EFilePriority.kPriorityNormal, false, true,
                    EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);
                int fileId1 = fileRecord.FileID;
                int fileToProcessId = fileProcessingDb.GetFilesToProcess("A01_ExtractData", 10, false, "")
                    .ToIEnumerable<IFileRecord>()
                    .Single()
                    .FileID;
                Assert.That(fileId1 == fileToProcessId);

                // Set to pending for both actions in Workflow 1 for later tests.
                fileProcessingDb.SetFileStatusToPending(fileId1, "A01_ExtractData", false);
                fileProcessingDb.SetFileStatusToPending(fileId1, "A02_Verify", false);

                // Start processing in Workflow 2
                fileProcessingDb.UnregisterActiveFAM();
                fileProcessingDb.RecordFAMSessionStop();
                fileProcessingDb.ActiveWorkflow = "Workflow2";
                int extractActionId2 = fileProcessingDb.GetActionID("A01_ExtractData");
                fileProcessingDb.RecordFAMSessionStart("Test.fps", "A02_Verify", true, false);
                fileProcessingDb.RegisterActiveFAM();

                // Check that though there is a file pending in Workflow1, GetFilesToProcess won't
                // grab it for workflow2.
                Assert.That(fileProcessingDb.GetStatsAllWorkflows("A02_Verify", false).NumDocumentsPending == 1);
                Assert.That(fileProcessingDb.GetFilesToProcess("A02_Verify", 10, false, "").Size() == 0);

                // In Workflow2, queue file1 for A02_Verify, stats now report 2 files pending
                // (really same file in both workflows), then confirm GetFilesToProcess grabs only the file for
                // Workflow2.
                fileProcessingDb.AddFile(testFileName1, "A02_Verify", workflowID2, EFilePriority.kPriorityNormal,
                    false, true, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows("A02_Verify", false).NumDocumentsPending == 2);
                fileToProcessId = fileProcessingDb.GetFilesToProcess("A02_Verify", 10, false, "")
                    .ToIEnumerable<IFileRecord>()
                    .Single()
                    .FileID;
                Assert.That(fileId1 == fileToProcessId);
                fileProcessingDb.NotifyFileProcessed(fileId1, "A02_Verify", workflowID2, false);

                // In Workflow2, queue file 2 for A01_ExtractData
                fileRecord = fileProcessingDb.AddFile(testFileName2, "A01_ExtractData", workflowID2,
                    EFilePriority.kPriorityNormal, false, true,
                    EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);
                int fileId2 = fileRecord.FileID;

                // Start processing in A01_ExtractData for all workflows
                fileProcessingDb.UnregisterActiveFAM();
                fileProcessingDb.RecordFAMSessionStop();
                fileProcessingDb.ActiveWorkflow = "";
                fileProcessingDb.RecordFAMSessionStart("Test.fps", "A01_ExtractData", true, false);
                fileProcessingDb.RegisterActiveFAM();

                // Ensure files from both workflows are grabbed for processing.
                var fileIDs = fileProcessingDb.GetFilesToProcess("A01_ExtractData", 10, false, "")
                    .ToIEnumerable<FileRecord>()
                    .Select(r => r.FileID)
                    .ToArray();
                Assert.That(fileIDs[0] == fileId1);
                Assert.That(fileIDs[1] == fileId2);

                // Simulate one file completing and the other failing. Ensure they move the correct statuses
                // in the correct workflows.
                fileProcessingDb.NotifyFileSkipped(fileId1, "A01_ExtractData", workflowID1, true);
                var ee = new ExtractException("ELI42098", "Test");
                fileProcessingDb.NotifyFileFailed(fileId2, "A01_ExtractData", workflowID2, ee.AsStringizedByteStream(), true);

                Assert.That(fileProcessingDb.GetStats(extractActionId1, false).NumDocumentsSkipped == 1);
                Assert.That(fileProcessingDb.GetStats(extractActionId1, false).NumDocumentsFailed == 0);
                Assert.That(fileProcessingDb.GetStats(extractActionId2, false).NumDocumentsSkipped == 0);
                Assert.That(fileProcessingDb.GetStats(extractActionId2, false).NumDocumentsFailed == 1);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows("A01_ExtractData", false).NumDocumentsPending == 0);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows("A01_ExtractData", false).NumDocumentsSkipped == 1);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows("A01_ExtractData", false).NumDocumentsFailed == 1);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows("A02_Verify", false).NumDocumentsComplete == 1);
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
                var fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);
                fileProcessingDb.DefineNewAction("A04_SendToEMR");
                fileProcessingDb.DefineNewAction("Cleanup");

                int workflowID = fileProcessingDb.AddWorkflow("Workflow1", EWorkflowType.kExtraction);
                fileProcessingDb.SetWorkflowActions(workflowID,
                    new[] { "A01_ExtractData", "A02_Verify", "A03_QA", "A04_SendToEMR", "Cleanup" }.ToVariantVector());
                WorkflowDefinition workflow = fileProcessingDb.GetWorkflowDefinition(workflowID);
                workflow.StartAction = "A01_ExtractData";
                workflow.EndAction = "A04_SendToEMR";
                workflow.PostWorkflowAction = "Cleanup";
                fileProcessingDb.SetWorkflowDefinition(workflow);

                fileProcessingDb.ActiveWorkflow = "Workflow1";
                fileProcessingDb.RecordFAMSessionStart("Test.fps", "A01_ExtractData", true, true);
                fileProcessingDb.RegisterActiveFAM();

                bool alreadyExists = false;
                EActionStatus previousStatus;
                var fileRecord = fileProcessingDb.AddFile(testFileName1, "A01_ExtractData", workflowID,
                    EFilePriority.kPriorityNormal, false, true,
                    EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);
                int fileId = fileRecord.FileID;

                Assert.That(fileProcessingDb.GetWorkflowStatus(fileId) == EActionStatus.kActionProcessing);
            }
            finally
            {
                _testFiles.RemoveFile(_LABDE_TEST_FILE1);
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

                int extractAction = fileProcessingDb.GetActionID("A01_ExtractData");
                int verifyAction = fileProcessingDb.GetActionID("A02_Verify");

                // Queue and process a file
                bool alreadyExists = false;
                EActionStatus previousStatus;
                fileProcessingDb.AddFile(testFileName1, "A01_ExtractData", -1, EFilePriority.kPriorityNormal,
                    false, false, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);

                var setStatusTaskConfig = new SetActionStatusFileProcessor();
                setStatusTaskConfig.ActionName = "A02_Verify";
                setStatusTaskConfig.ActionStatus = (int)EActionStatus.kActionPending;
                var setStatusTask = (IFileProcessingTask)setStatusTaskConfig;

                using (var famSession = new FAMProcessingSession(
                    fileProcessingDb, "A01_ExtractData", "", setStatusTask,
                    threadCount: 1, filesToGrabCount: 1, keepProcessing: false))
                {
                    famSession.WaitForProcessingToComplete();
                }

                // Ensure file has processed 
                Assert.That(fileProcessingDb.GetStats(extractAction, false).NumDocumentsPending == 0);
                Assert.That(fileProcessingDb.GetStats(extractAction, false).NumDocumentsComplete == 1);
                Assert.That(fileProcessingDb.GetStats(verifyAction, false).NumDocumentsPending == 1);

                // Queue another 2 more files, test that they can both be processed at once.
                fileProcessingDb.AddFile(testFileName2, "A01_ExtractData", -1, EFilePriority.kPriorityNormal,
                    false, false, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);
                fileProcessingDb.AddFile(testFileName3, "A01_ExtractData", -1, EFilePriority.kPriorityNormal,
                    false, false, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);

                Assert.That(fileProcessingDb.GetStats(extractAction, false).NumDocumentsPending == 2);

                using (var famSession = new FAMProcessingSession(
                    fileProcessingDb, "A01_ExtractData", "", setStatusTask,
                    threadCount: 2, filesToGrabCount: 2, keepProcessing: false))
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
                try
                {
                    _testDbManager.RemoveDatabase(testDbName);
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI42151");
                }
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
                fileProcessingDb.SetWorkflowActions(workflowID1, new[] { "A01_ExtractData", "A02_Verify" }.ToVariantVector());
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                int extractAction1 = fileProcessingDb.GetActionID("A01_ExtractData");

                int workflowID2 = fileProcessingDb.AddWorkflow("Workflow2", EWorkflowType.kUndefined);
                fileProcessingDb.SetWorkflowActions(workflowID2, new[] { "A01_ExtractData", "A02_Verify" }.ToVariantVector());
                fileProcessingDb.ActiveWorkflow = "Workflow2";
                int extractAction2 = fileProcessingDb.GetActionID("A01_ExtractData");
                int verifyAction2 = fileProcessingDb.GetActionID("A02_Verify");

                // Queue and process a file in workflow 1
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                bool alreadyExists = false;
                EActionStatus previousStatus;
                fileProcessingDb.AddFile(testFileName1, "A01_ExtractData", workflowID1, EFilePriority.kPriorityNormal,
                    false, false, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);

                var setStatusTaskConfig = new SetActionStatusFileProcessor();
                setStatusTaskConfig.ActionName = "A02_Verify";
                setStatusTaskConfig.ActionStatus = (int)EActionStatus.kActionPending;
                var setStatusTask = (IFileProcessingTask)setStatusTaskConfig;

                using (var famSession = new FAMProcessingSession(
                    fileProcessingDb, "A01_ExtractData", "Workflow1", setStatusTask,
                    threadCount: 1, filesToGrabCount: 1, keepProcessing: false))
                {
                    famSession.WaitForProcessingToComplete();
                }                

                // Ensure file has processed only in the context of workflow 1 (including where it gets set to pending
                // in the next action)
                Assert.That(fileProcessingDb.GetStatsAllWorkflows("A01_ExtractData", false).NumDocumentsPending == 0);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows("A01_ExtractData", false).NumDocumentsComplete == 1);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows("A02_Verify", false).NumDocumentsPending == 1);

                Assert.That(fileProcessingDb.GetStats(extractAction2, false).NumDocumentsComplete == 0);
                Assert.That(fileProcessingDb.GetStats(verifyAction2, false).NumDocumentsPending == 0);

                // Queue another file to workflow1, then the original file to workflow2, then test that both
                // processing in a session configured to run on all workflows
                fileProcessingDb.AddFile(testFileName2, "A01_ExtractData", workflowID1, EFilePriority.kPriorityNormal,
                    false, false, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);

                fileProcessingDb.ActiveWorkflow = "Workflow2";
                fileProcessingDb.AddFile(testFileName1, "A01_ExtractData", workflowID2, EFilePriority.kPriorityNormal,
                    false, false, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);

                Assert.That(fileProcessingDb.GetStats(extractAction1, false).NumDocumentsPending == 1);
                Assert.That(fileProcessingDb.GetStats(extractAction2, false).NumDocumentsPending == 1);

                using (var famSession = new FAMProcessingSession(
                    fileProcessingDb, "A01_ExtractData", "", setStatusTask,
                    threadCount: 1, filesToGrabCount: 1, keepProcessing: false))
                {
                    famSession.WaitForProcessingToComplete();
                }

                Assert.That(fileProcessingDb.GetStatsAllWorkflows("A01_ExtractData", false).NumDocumentsPending == 0);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows("A01_ExtractData", false).NumDocumentsComplete == 3);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows("A02_Verify", false).NumDocumentsPending == 3);
            }
            finally
            {
                _testFiles.RemoveFile(_LABDE_TEST_FILE1);
                _testFiles.RemoveFile(_LABDE_TEST_FILE2);
                try
                {
                    _testDbManager.RemoveDatabase(testDbName);
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI42130");
                }
            }
        }

        #endregion Test Methods
    }
}
