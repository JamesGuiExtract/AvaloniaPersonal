using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Database.Test
{
    /// <summary>
    /// Testing class for the workflow management related methods in IFileProcessingDB.
    /// </summary>
    [Category("TestFAMWorkflowManagement")]
    [TestFixture]
    public class TestFAMWorkflowManagement
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
        static TestFileManager<TestFAMWorkflowManagement> _testFiles;

        /// <summary>
        /// Manages test FAM DBs.
        /// </summary>
        static FAMTestDBManager<TestFAMWorkflowManagement> _testDbManager;

        #endregion Fields

        #region Overhead

        /// <summary>
        /// Initializes the test fixture.
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestFAMWorkflowManagement>();
            _testDbManager = new FAMTestDBManager<TestFAMWorkflowManagement>();
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
        /// Tests whether a new workflow can be created.
        /// </summary>
        [Test, Category("Automated")]
        public static void CreateWorkflow()
        {
            string testDbName = "Test_CreateWorkflow";

            try
            {
                var fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);
                int id = fileProcessingDb.AddWorkflow("Workflow1", EWorkflowType.kUndefined);
                IWorkflowDefinition workflowDefiniton = fileProcessingDb.GetWorkflowDefinition(id);
                Assert.That(workflowDefiniton.Name == "Workflow1");
            }
            finally
            {
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        /// <summary>
        /// Tests whether a new workflow can be deleted.
        /// </summary>
        [Test, Category("Automated")]
        public static void DeleteWorkflow()
        {
            string testDbName = "Test_DeleteWorkflow";

            try
            {
                var fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);
                int id = fileProcessingDb.AddWorkflow("Workflow1", EWorkflowType.kUndefined);
                IWorkflowDefinition workflowDefiniton = fileProcessingDb.GetWorkflowDefinition(id);
                Assert.That(workflowDefiniton.Name == "Workflow1");

                fileProcessingDb.DeleteWorkflow(id);

                Assert.Throws<COMException>(() => fileProcessingDb.GetWorkflowDefinition(id));
            }
            finally
            {
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        /// <summary>
        /// Tests whether workflow properties can be updated.
        /// </summary>
        [Test, Category("Automated")]
        public static void SetWorkflowProperties()
        {
            string testDbName = "Test_SetWorkflowProperties";

            try
            {
                var fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);
                int id = fileProcessingDb.AddWorkflow("Workflow1", EWorkflowType.kExtraction);

                // Test initial Workflow property values
                WorkflowDefinition outputDefinition = fileProcessingDb.GetWorkflowDefinition(id);
                Assert.That(outputDefinition.Name == "Workflow1");
                Assert.That(outputDefinition.Type == EWorkflowType.kExtraction);
                Assert.That(string.IsNullOrEmpty(outputDefinition.Description));
                Assert.That(string.IsNullOrEmpty(outputDefinition.StartAction));
                Assert.That(string.IsNullOrEmpty(outputDefinition.EndAction));
                Assert.That(string.IsNullOrEmpty(outputDefinition.PostWorkflowAction));
                Assert.That(string.IsNullOrEmpty(outputDefinition.DocumentFolder));
                Assert.That(string.IsNullOrEmpty(outputDefinition.OutputAttributeSet));
                Assert.That(string.IsNullOrEmpty(outputDefinition.OutputFileMetadataField));

                var actionNames = new[] { "A01_ExtractData", "A02_Verify", "Z_AdminAction" }
                    .ToVariantVector();
                fileProcessingDb.SetWorkflowActions(id, actionNames);

                // Update workflow properties.
                WorkflowDefinition workflowDefinition = new WorkflowDefinition();
                workflowDefinition.ID = id;
                workflowDefinition.Name = "Workflow1";
                workflowDefinition.Type = EWorkflowType.kClassification;
                workflowDefinition.Description = "A test of FileProcessingDB.SetWorkflowDefinition.";
                workflowDefinition.StartAction = "A01_ExtractData";
                workflowDefinition.EndAction = "A02_Verify";
                workflowDefinition.PostWorkflowAction = "Z_AdminAction";
                workflowDefinition.DocumentFolder = @"C:\Demo_LabDE\Input";
                workflowDefinition.OutputAttributeSet = "DataFoundByRules";
                workflowDefinition.OutputFileMetadataField = "PatientFirstName";
                fileProcessingDb.SetWorkflowDefinition(workflowDefinition);

                // Test updated workflow properties.
                outputDefinition = fileProcessingDb.GetWorkflowDefinition(id);
                Assert.That(outputDefinition.Name == workflowDefinition.Name);
                Assert.That(outputDefinition.Type == workflowDefinition.Type);
                Assert.That(outputDefinition.Description == workflowDefinition.Description);
                Assert.That(outputDefinition.StartAction == workflowDefinition.StartAction);
                Assert.That(outputDefinition.EndAction == workflowDefinition.EndAction);
                Assert.That(outputDefinition.PostWorkflowAction == workflowDefinition.PostWorkflowAction);
                Assert.That(outputDefinition.DocumentFolder == workflowDefinition.DocumentFolder);
                Assert.That(outputDefinition.OutputAttributeSet == workflowDefinition.OutputAttributeSet);
                Assert.That(outputDefinition.OutputFileMetadataField == workflowDefinition.OutputFileMetadataField);

                // Clear workflow properties
                workflowDefinition.ID = id;
                workflowDefinition.Name = "";
                workflowDefinition.Type = EWorkflowType.kUndefined;
                workflowDefinition.Description = "";
                workflowDefinition.StartAction = "";
                workflowDefinition.EndAction = "";
                workflowDefinition.PostWorkflowAction = "";
                workflowDefinition.DocumentFolder = @"";
                workflowDefinition.OutputAttributeSet = "";
                workflowDefinition.OutputFileMetadataField = "";
                fileProcessingDb.SetWorkflowDefinition(workflowDefinition);

                // Test cleared workflow properties
                outputDefinition = fileProcessingDb.GetWorkflowDefinition(id);
                Assert.That(string.IsNullOrEmpty(outputDefinition.Name));
                Assert.That(outputDefinition.Type == EWorkflowType.kUndefined);
                Assert.That(string.IsNullOrEmpty(outputDefinition.Description));
                Assert.That(string.IsNullOrEmpty(outputDefinition.StartAction));
                Assert.That(string.IsNullOrEmpty(outputDefinition.EndAction));
                Assert.That(string.IsNullOrEmpty(outputDefinition.PostWorkflowAction));
                Assert.That(string.IsNullOrEmpty(outputDefinition.DocumentFolder));
                Assert.That(string.IsNullOrEmpty(outputDefinition.OutputAttributeSet));
                Assert.That(string.IsNullOrEmpty(outputDefinition.OutputFileMetadataField));
            }

            finally
            {
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        /// <summary>
        /// Tests whether workflows are retained after a database clear operation.
        /// </summary>
        [Test, Category("Automated")]
        public static void PreserveWorkflowOnClear()
        {
            string testDbName = "Test_PreserveWorkflowOnClear";

            try
            {
                var fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                // Create and configure workflow
                int id = fileProcessingDb.AddWorkflow("Workflow1", EWorkflowType.kExtraction);

                var actionNames = new[] { "A01_ExtractData", "A02_Verify", "Z_AdminAction" }
                    .ToVariantVector();
                fileProcessingDb.SetWorkflowActions(id, actionNames);

                WorkflowDefinition workflowDefinition = new WorkflowDefinition();
                workflowDefinition.ID = id;
                workflowDefinition.Name = testDbName;
                workflowDefinition.Type = EWorkflowType.kExtraction;
                workflowDefinition.Description =
                    "A test of whether workflows are retained after a database clear operation.";
                workflowDefinition.StartAction = "A01_ExtractData";
                workflowDefinition.EndAction = "A02_Verify";
                workflowDefinition.PostWorkflowAction = "Z_AdminAction";
                workflowDefinition.DocumentFolder = @"C:\Demo_LabDE\Input";
                workflowDefinition.OutputAttributeSet = "DataFoundByRules";
                workflowDefinition.OutputFileMetadataField = "PatientFirstName";
                fileProcessingDb.SetWorkflowDefinition(workflowDefinition);

                // Clear DB (retain user settings).
                fileProcessingDb.Clear(vbRetainUserValues: true);

                // Ensure workflow is still properly configured.
                WorkflowDefinition outputDefinition = fileProcessingDb.GetWorkflowDefinition(id);
                Assert.That(outputDefinition.Name == workflowDefinition.Name);
                Assert.That(outputDefinition.Type == workflowDefinition.Type);
                Assert.That(outputDefinition.Description == workflowDefinition.Description);
                Assert.That(outputDefinition.StartAction == workflowDefinition.StartAction);
                Assert.That(outputDefinition.EndAction == workflowDefinition.EndAction);
                Assert.That(outputDefinition.PostWorkflowAction == workflowDefinition.PostWorkflowAction);
                Assert.That(outputDefinition.DocumentFolder == workflowDefinition.DocumentFolder);
                Assert.That(outputDefinition.OutputAttributeSet == workflowDefinition.OutputAttributeSet);
                Assert.That(outputDefinition.OutputFileMetadataField == workflowDefinition.OutputFileMetadataField);
            }

            finally
            {
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        /// <summary>
        /// Tests adding and removing actions from a workflow.
        /// </summary>
        [Test, Category("Automated")]
        public static void ManageWorkflowActions()
        {
            string testDbName = "Test_ManageWorkflowActions";

            try
            {
                var fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                var allActionNames = new[] // In order of name
                    { "A01_ExtractData", "A02_Verify", "A03_QA", "B01_ViewNonLab", "Z_AdminAction"}; 

                int id = fileProcessingDb.AddWorkflow("Workflow1", EWorkflowType.kUndefined);

                IStrToStrMap workflowActionMap = fileProcessingDb.GetWorkflowActions(id);
                Assert.That(workflowActionMap.Size == 0);

                // Test adding three existing actions to workflow
                var actionNames = new[] { "A01_ExtractData", "A02_Verify", "A03_QA" }; // In order of name
                fileProcessingDb.SetWorkflowActions(id, actionNames.ToVariantVector());

                workflowActionMap = fileProcessingDb.GetWorkflowActions(id);
                var retrievedActions = workflowActionMap.ComToDictionary().Keys.OrderBy(name => name);
                Assert.That(actionNames.SequenceEqual(retrievedActions));

                fileProcessingDb.ActiveWorkflow = "Workflow1";
                retrievedActions = fileProcessingDb.GetActions().ComToDictionary().Keys.OrderBy(name => name);
                Assert.That(actionNames.SequenceEqual(retrievedActions));

                // Ensure that GetAllActions retrieves actions that aren't assigned to this workflow.
                retrievedActions = fileProcessingDb.GetAllActions().ComToDictionary().Keys.OrderBy(name => name);
                Assert.That(allActionNames.SequenceEqual(retrievedActions));

                // Test deleting 2 actions + adding one existing action and one new action to workflow
                fileProcessingDb.DefineNewAction(testDbName);

                actionNames = new[] { "A02_Verify", testDbName, "Z_AdminAction" }; // In order of name
                fileProcessingDb.SetWorkflowActions(id, actionNames.ToVariantVector());

                workflowActionMap = fileProcessingDb.GetWorkflowActions(id);
                retrievedActions = workflowActionMap.ComToDictionary().Keys.OrderBy(name => name);
                Assert.That(actionNames.SequenceEqual(retrievedActions));

                // Confirm that action configuration is preserved if DB is cleared (retaining settings)
                fileProcessingDb.Clear(true);

                workflowActionMap = fileProcessingDb.GetWorkflowActions(id);
                retrievedActions = workflowActionMap.ComToDictionary().Keys.OrderBy(name => name);
                Assert.That(actionNames.SequenceEqual(retrievedActions));

                // Confirm workflow can be deleted even if it has actions.
                fileProcessingDb.DeleteWorkflow(id);
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
        public static void EditActionsWithWorkflows()
        {
            string testDbName = "Test_EditActionsWithWorkflows";

            try
            {
                var fileProcessingDb = _testDbManager.GetNewDatabase(testDbName);

                int actionStart1 = fileProcessingDb.DefineNewAction("Start");

                int workflowID = fileProcessingDb.AddWorkflow("Workflow1", EWorkflowType.kUndefined);

                // Check that GetActionID applies only to current workflow.
                Assert.That(fileProcessingDb.GetActionID("Start") == actionStart1);
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                Assert.Throws<COMException>(() => fileProcessingDb.GetActionID("Start"));

                // Despite active workflow being set, end should not yet be part of it.
                int actionEnd1 = fileProcessingDb.DefineNewAction("End");
                Assert.Throws<COMException>(() => fileProcessingDb.GetActionID("End"));

                var workflowActions = new[] { "Start", "End" }.ToVariantVector();
                fileProcessingDb.SetWorkflowActions(workflowID, workflowActions);

                // Test that a separate action ID was assigned for the workflow
                int actionStart2 = fileProcessingDb.GetActionID("Start");
                Assert.That(actionStart1 != actionStart2);
                int actionEnd2 = fileProcessingDb.GetActionID("End");
                Assert.That(actionEnd1 != actionEnd2);

                // Test renaming an action while an active workflow is set.
                fileProcessingDb.RenameAction("Start", "Begin");
                var newWorkflowActions = fileProcessingDb.GetWorkflowActions(workflowID)
                    .ComToDictionary()
                    .Keys
                    .AsEnumerable<string>()
                    .ToArray();
                Assert.That(newWorkflowActions[0] == "Begin");
                Assert.That(newWorkflowActions[1] == "End");

                // Check that GetActions also returns the correct result per active workflow.
                var actionDictionary = fileProcessingDb.GetActions().ComToDictionary();
                Assert.That(actionDictionary.Count == 2);
                Assert.That(actionDictionary["Begin"] == actionStart2.ToString(CultureInfo.InvariantCulture));
                Assert.That(actionDictionary["End"] == actionEnd2.ToString(CultureInfo.InvariantCulture));

                // Test deleting an action while an active workflow is set.
                fileProcessingDb.DeleteAction("End");

                // Ensure that we can look up action ID and name both with and without a workflow set.
                Assert.That(fileProcessingDb.GetActionID("Begin") == actionStart2);
                Assert.That(fileProcessingDb.GetActionName(actionStart2) == "Begin");
                fileProcessingDb.ActiveWorkflow = "";
                Assert.That(fileProcessingDb.GetActionID("Begin") == actionStart1);
                Assert.That(fileProcessingDb.GetActionName(actionStart1) == "Begin");

                // With no context, GetActions should not report workflow specific actions.
                Assert.That(fileProcessingDb.GetActions().ComToDictionary().Keys.Single() == "Begin");

                // After having deleted actions both with and without the active workflow set, make
                // sure there are no actions left.
                fileProcessingDb.DeleteAction("Begin");
                Assert.That(fileProcessingDb.GetWorkflowActions(workflowID).Size == 0);
                Assert.That(fileProcessingDb.GetActions().Size == 0);
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
        public static void AddRemoveFilesWithWorkflows()
        {
            string testDbName = "Test_AddRemoveFilesWithWorkflows";
            string actionName = "A01_ExtractData";

            try
            {
                string testFileName = _testFiles.GetFile(_LABDE_TEST_FILE1);
                var fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                int id = fileProcessingDb.AddWorkflow("Workflow1", EWorkflowType.kUndefined);
                fileProcessingDb.SetWorkflowActions(id, new[] { actionName }.ToVariantVector());
                int actionId1 = fileProcessingDb.GetActionID(actionName);

                fileProcessingDb.RecordFAMSessionStart("Test.fps", actionName, true, false);

                // Should not be able to queue a file without setting a workflow.
                bool alreadyExists = false;
                EActionStatus previousStatus = EActionStatus.kActionUnattempted;
                Assert.Throws<COMException>(() => fileProcessingDb.AddFile(
                    testFileName, actionName, EFilePriority.kPriorityNormal, false, false, EActionStatus.kActionPending, true,
                    out alreadyExists, out previousStatus));

                // Should not be able to change workflow during an active FAM Session.
                Assert.Throws<COMException>(() => fileProcessingDb.ActiveWorkflow = testDbName);

                fileProcessingDb.RecordFAMSessionStop();

                // Set active workflow which will correspond to a different action ID.
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                int actionId2 = fileProcessingDb.GetActionID(actionName);
                fileProcessingDb.RecordFAMSessionStart("Test.fps", actionName, true, false);

                // We should now be able to add a file now that we have a workflow set.
                fileProcessingDb.AddFile(
                    testFileName, actionName, EFilePriority.kPriorityNormal, false, false, EActionStatus.kActionPending, true,
                    out alreadyExists, out previousStatus);

                // The actionID associated with the workflow should have a file queued, but not the "NULL" workflow action.
                Assert.That(fileProcessingDb.GetStats(actionId1, false).NumDocumentsPending == 0);
                Assert.That(fileProcessingDb.GetStats(actionId2, false).NumDocumentsPending == 1);

                // Test that after removing the file there are no longer any files pending on the workflow's action.
                fileProcessingDb.RemoveFile(testFileName, actionName);
                Assert.That(fileProcessingDb.GetStats(actionId2, false).NumDocumentsPending == 0);
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
                int actionID = fileProcessingDb.GetActionID("Action1");
                fileProcessingDb.RecordFAMSessionStart("Test.fps", "Action1", true, false);

                bool alreadyExists = false;
                EActionStatus previousStatus = EActionStatus.kActionUnattempted;
                var fileRecord = fileProcessingDb.AddFile(testFileName, "Action1",
                    EFilePriority.kPriorityNormal, true, false, EActionStatus.kActionPending, true,
                    out alreadyExists, out previousStatus);
                int fileId = fileRecord.FileID;

                // Without having yet set AutoCreateActions setting to true, AutoCreateActions should
                // fail and we should not be able to queue to an action that does not exist in the
                // workflow (whether or not it exists as whole).
                Assert.Throws<COMException>(() => fileProcessingDb.AutoCreateAction("Action2"));
                Assert.Throws<COMException>(() =>
                    fileProcessingDb.SetStatusForFile(fileId, "Action2", EActionStatus.kActionPending, true, false, out previousStatus));

                // Turn on auto-create actions.
                fileProcessingDb.RecordFAMSessionStop();
                fileProcessingDb.ExecuteCommandQuery("UPDATE [DBInfo] SET [Value] = '1' WHERE [Name] = 'AutoCreateActions'");
                fileProcessingDb.RecordFAMSessionStart("Test.fps", "Action1", true, false);

                // Test creating an using an action that already exists in a different workflow.
                fileProcessingDb.AutoCreateAction("Action2");
                fileProcessingDb.SetStatusForFile(fileId, "Action2", EActionStatus.kActionPending, true, false, out previousStatus);

                // Test creating an using an action that didn't already exist in a different workflow.
                fileProcessingDb.AutoCreateAction("Action3");
                fileProcessingDb.SetStatusForFile(fileId, "Action3", EActionStatus.kActionPending, true, false, out previousStatus);

                // Action 1
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
        public static void TestWorkflowStatistics()
        {
            string testDbName = "Test_TestWorkflowStatistics";

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
                var fileRecord = fileProcessingDb.AddFile(testFileName1, "A01_ExtractData",
                    EFilePriority.kPriorityNormal, true, false, EActionStatus.kActionPending, false,
                    out alreadyExists, out previousStatus);
                int fileId = fileRecord.FileID;
                fileProcessingDb.SetStatusForFile(fileId, "A02_Verify", EActionStatus.kActionCompleted, false, false, out previousStatus);

                fileProcessingDb.RecordFAMSessionStop();
                fileProcessingDb.ActiveWorkflow = "Workflow2";
                fileProcessingDb.RecordFAMSessionStart("Test.fps", "A02_Verify", true, false);

                // Workflow2: 
                // File 1, 2 and 3 to Pending in A02_Verify
                // File 3 to Skipped in A03_QA
                // File 3 to Failed in NewAction
                fileProcessingDb.SetStatusForFile(fileId, "A02_Verify", EActionStatus.kActionPending, false, false, out previousStatus);
                fileRecord = fileProcessingDb.AddFile(testFileName2, "A02_Verify",
                    EFilePriority.kPriorityNormal, true, false, EActionStatus.kActionPending, false,
                    out alreadyExists, out previousStatus);
                fileRecord = fileProcessingDb.AddFile(testFileName3, "A02_Verify",
                    EFilePriority.kPriorityNormal, true, false, EActionStatus.kActionPending, false,
                    out alreadyExists, out previousStatus);
                fileId = fileRecord.FileID;
                fileProcessingDb.SetStatusForFile(fileId, "A03_QA", EActionStatus.kActionSkipped, false, false, out previousStatus);

                fileProcessingDb.ExecuteCommandQuery("UPDATE [DBInfo] SET [Value] = '1' WHERE [Name] = 'AutoCreateActions'");
                int newAction2 = fileProcessingDb.AutoCreateAction("NewAction");
                fileProcessingDb.SetStatusForFile(fileId, "NewAction", EActionStatus.kActionFailed, true, false, out previousStatus);

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
        public static void TestGetFilesToProcess()
        {
            string testDbName = "Test_TestGetFilesToProcess";

            try
            {
                string testFileName1 = _testFiles.GetFile(_LABDE_TEST_FILE1);
                string testFileName2 = _testFiles.GetFile(_LABDE_TEST_FILE2);
                var fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                int workflowID = fileProcessingDb.AddWorkflow("Workflow1", EWorkflowType.kUndefined);
                fileProcessingDb.SetWorkflowActions(workflowID,
                    new[] { "A01_ExtractData", "A02_Verify" }.ToVariantVector());
                workflowID = fileProcessingDb.AddWorkflow("Workflow2", EWorkflowType.kUndefined);
                fileProcessingDb.SetWorkflowActions(workflowID,
                    new[] { "A01_ExtractData", "A02_Verify" }.ToVariantVector());

                // Start processing in Workflow 1
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                fileProcessingDb.RecordFAMSessionStart("Test.fps", "A01_ExtractData", true, false);
                fileProcessingDb.RegisterActiveFAM();

                // In Workflow 1, queue file 1 to pending in A01_ExtractData, and check that
                // GetFilesToProcess grabs it.
                bool alreadyExists = false;
                EActionStatus previousStatus;
                var fileRecord = fileProcessingDb.AddFile(testFileName1, "A01_ExtractData", EFilePriority.kPriorityNormal, false, true,
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
                fileProcessingDb.RecordFAMSessionStart("Test.fps", "A02_Verify", true, false);
                fileProcessingDb.RegisterActiveFAM();

                // Check that though there is a file pending in Workflow1, GetFilesToProcess won't
                // grab it for workflow2.
                Assert.That(fileProcessingDb.GetStatsAllWorkflows("A02_Verify", false).NumDocumentsPending == 1);
                Assert.That(fileProcessingDb.GetFilesToProcess("A02_Verify", 10, false, "").Size() == 0);

                // In Workflow2, queue file1 for A02_Verify, stats now report 2 files pending
                // (really same file in both workflows), then confirm GetFilesToProcess grabs only the file for
                // Workflow2.
                fileProcessingDb.AddFile(testFileName1, "A02_Verify", EFilePriority.kPriorityNormal, false, true,
                    EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);
                Assert.That(fileProcessingDb.GetStatsAllWorkflows("A02_Verify", false).NumDocumentsPending == 2);
                fileToProcessId = fileProcessingDb.GetFilesToProcess("A02_Verify", 10, false, "")
                    .ToIEnumerable<IFileRecord>()
                    .Single()
                    .FileID;
                Assert.That(fileId1 == fileToProcessId);
                fileProcessingDb.NotifyFileProcessed(fileId1, "A02_Verify", false);

                // In Workflow2, queue file 2 for A01_ExtractData
                fileRecord = fileProcessingDb.AddFile(testFileName2, "A01_ExtractData", EFilePriority.kPriorityNormal, false, true,
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
            }
            finally
            {
                _testFiles.RemoveFile(_LABDE_TEST_FILE1);
                _testFiles.RemoveFile(_LABDE_TEST_FILE2);
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        #endregion Test Methods
    }
}
