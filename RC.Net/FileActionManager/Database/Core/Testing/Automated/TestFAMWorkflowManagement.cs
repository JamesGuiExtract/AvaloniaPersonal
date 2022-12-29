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
    [Category("TestFAMWorkflowManagement")]
    [TestFixture]
    public class TestFAMWorkflowManagement
    {
        #region Constants

        static readonly string _LABDE_EMPTY_DB = "Resources.Demo_LabDE_Empty";

        static readonly string _LABDE_TEST_FILE1 = "Resources.TestImage001.tif";
        static readonly string _LABDE_TEST_FILE2 = "Resources.TestImage002.tif";

        static readonly string _ACTION_A = "ActionA";
        static readonly string _ACTION_B = "ActionB";
        static readonly string _ACTION_C = "ActionC";
        static readonly string _LABDE_ACTION1 = "A01_ExtractData";
        static readonly string _LABDE_ACTION2 = "A02_Verify";
        static readonly string _LABDE_ACTION3 = "A03_QA";
        static readonly string _LABDE_ACTION4 = "B01_ViewNonLab";
        static readonly string _LABDE_ACTION5 = "Z_AdminAction";

        static readonly string _ALL_WORKFLOWS = "<All workflows>";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Manages test FAM DBs.
        /// </summary>
        static FAMTestDBManager<TestFAMWorkflowManagement> _testDbManager;

        /// <summary>
        /// Manages test files.
        /// </summary>
        static TestFileManager<TestFAMFileProcessing> _testFiles;

        #endregion Fields

        #region Overhead

        /// <summary>
        /// Initializes the test fixture.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testDbManager = new FAMTestDBManager<TestFAMWorkflowManagement>();
            _testFiles = new TestFileManager<TestFAMFileProcessing>();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [OneTimeTearDown]
        public static void FinalCleanup()
        {
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

                Assert.AreEqual(workflowDefiniton.Name, "Workflow1");
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
        /// Tests GetWorkflowID and GetWorkflows
        /// </summary>
        [Test, Category("Automated")]
        public static void GetWorkflowsAndIds()
        {
            string testDbName = "Test_GetWorkflowsAndIds";

            try
            {
                var fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                int id1 = fileProcessingDb.AddWorkflow("Workflow1", EWorkflowType.kUndefined);
                IWorkflowDefinition workflowDefiniton1 = fileProcessingDb.GetWorkflowDefinition(id1);

                Assert.AreEqual(id1, workflowDefiniton1.ID);
                Assert.AreEqual(id1, fileProcessingDb.GetWorkflowID("Workflow1"));
                Assert.AreEqual(id1, fileProcessingDb.GetWorkflowID("workflow1"));

                int id2 = fileProcessingDb.AddWorkflow("Workflow2", EWorkflowType.kUndefined);
                IWorkflowDefinition workflowDefiniton2 = fileProcessingDb.GetWorkflowDefinition(id2);

                Assert.AreEqual(id2, workflowDefiniton2.ID);
                Assert.AreEqual(id2, fileProcessingDb.GetWorkflowID("Workflow2"));
                Assert.AreEqual(id2, fileProcessingDb.GetWorkflowID("WoRkFlOw2"));

                Assert.AreEqual(-1, fileProcessingDb.GetWorkflowID(""));
                fileProcessingDb.ActiveWorkflow = "Workflow2";
                Assert.AreEqual(id2, fileProcessingDb.GetWorkflowID(""));

                var workflows = fileProcessingDb.GetWorkflows();
                Assert.AreEqual(workflows.Size, 2);
                Assert.AreEqual(id1.AsString(), workflows.GetValue("Workflow1"));
                Assert.AreEqual(id2.AsString(), workflows.GetValue("WORKFLOW2"));
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
                int id = fileProcessingDb.AddWorkflow("Workflow1", EWorkflowType.kExtraction,
                    _LABDE_ACTION1, _LABDE_ACTION2, _LABDE_ACTION5);

                // Test initial Workflow property values
                WorkflowDefinition outputDefinition = fileProcessingDb.GetWorkflowDefinition(id);
                Assert.That(outputDefinition.Name == "Workflow1");
                Assert.That(outputDefinition.Type == EWorkflowType.kExtraction);
                Assert.That(string.IsNullOrEmpty(outputDefinition.Description));
                Assert.AreEqual(1, outputDefinition.LoadBalanceWeight);

                // Update workflow properties.
                WorkflowDefinition workflowDefinition = new WorkflowDefinition();
                workflowDefinition.ID = id;
                workflowDefinition.Name = "Workflow1";
                workflowDefinition.Type = EWorkflowType.kClassification;
                workflowDefinition.Description = "A test of FileProcessingDB.SetWorkflowDefinition.";
                workflowDefinition.LoadBalanceWeight = 2;
                fileProcessingDb.SetWorkflowDefinition(workflowDefinition);

                // Test updated workflow properties.
                outputDefinition = fileProcessingDb.GetWorkflowDefinition(id);
                Assert.That(outputDefinition.Name == workflowDefinition.Name);
                Assert.That(outputDefinition.Type == workflowDefinition.Type);
                Assert.That(outputDefinition.Description == workflowDefinition.Description);
                Assert.That(outputDefinition.LoadBalanceWeight == workflowDefinition.LoadBalanceWeight);

                // Clear workflow properties
                workflowDefinition.ID = id;
                workflowDefinition.Name = "";
                workflowDefinition.Type = EWorkflowType.kUndefined;
                workflowDefinition.Description = "";
                workflowDefinition.LoadBalanceWeight = 1;
                fileProcessingDb.SetWorkflowDefinition(workflowDefinition);

                // Test cleared workflow properties
                outputDefinition = fileProcessingDb.GetWorkflowDefinition(id);
                Assert.That(string.IsNullOrEmpty(outputDefinition.Name));
                Assert.That(outputDefinition.Type == EWorkflowType.kUndefined);
                Assert.That(string.IsNullOrEmpty(outputDefinition.Description));
                Assert.AreEqual(1, outputDefinition.LoadBalanceWeight);
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
                int id = fileProcessingDb.AddWorkflow(
                    "Workflow1", EWorkflowType.kExtraction, _LABDE_ACTION1, _LABDE_ACTION2, _LABDE_ACTION5);

                WorkflowDefinition workflowDefinition = new WorkflowDefinition();
                workflowDefinition.ID = id;
                workflowDefinition.Name = testDbName;
                workflowDefinition.Type = EWorkflowType.kExtraction;
                workflowDefinition.Description =
                    "A test of whether workflows are retained after a database clear operation.";
                workflowDefinition.LoadBalanceWeight = 2;
                fileProcessingDb.SetWorkflowDefinition(workflowDefinition);

                // Clear DB (retain user settings).
                fileProcessingDb.Clear(vbRetainUserValues: true);

                // Ensure workflow is still properly configured.
                WorkflowDefinition outputDefinition = fileProcessingDb.GetWorkflowDefinition(id);
                Assert.That(outputDefinition.Name == workflowDefinition.Name);
                Assert.That(outputDefinition.Type == workflowDefinition.Type);
                Assert.That(outputDefinition.Description == workflowDefinition.Description);
                Assert.That(outputDefinition.LoadBalanceWeight == workflowDefinition.LoadBalanceWeight);
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
                    { _LABDE_ACTION1, _LABDE_ACTION2, _LABDE_ACTION3, _LABDE_ACTION4, _LABDE_ACTION5};

                int id = fileProcessingDb.AddWorkflow("Workflow1", EWorkflowType.kUndefined);

                IUnknownVector workflowActions = fileProcessingDb.GetWorkflowActions(id);
                Assert.That(workflowActions.Size() == 0);

                // Test adding three existing actions to workflow
                var actionNames =
                    new[] { _LABDE_ACTION1, _LABDE_ACTION2, _LABDE_ACTION3 }; // In order of name
                workflowActions = actionNames
                    .Select(name =>
                     {
                         var actionInfo = new VariantVector();
                         actionInfo.PushBack(name);
                         actionInfo.PushBack(true);
                         return actionInfo;
                     })
                    .ToIUnknownVector();

                fileProcessingDb.SetWorkflowActions(id, workflowActions);

                workflowActions = fileProcessingDb.GetWorkflowActions(id);
                var retrievedActions = workflowActions
                    .ToIEnumerable<IVariantVector>()
                    .Select(actionInfo => (string)actionInfo[1])
                    .OrderBy(name => name);
                Assert.That(actionNames.SequenceEqual(retrievedActions));

                // Test that the GetWorkflowNameFromActionID works 
                var returnedWorkflowSet = workflowActions
                    .ToIEnumerable<IVariantVector>()
                    .Select(actionInfo => fileProcessingDb.GetWorkflowNameFromActionID((int)actionInfo[0]))
                    .Distinct();

                Assert.That(returnedWorkflowSet.Single() == "Workflow1");

                fileProcessingDb.ActiveWorkflow = "Workflow1";
                retrievedActions = fileProcessingDb.GetActions().ComToDictionary().Keys.OrderBy(name => name);
                Assert.That(actionNames.SequenceEqual(retrievedActions));

                // Ensure that GetAllActions retrieves actions that aren't assigned to this workflow.
                retrievedActions = fileProcessingDb.GetAllActions().ComToDictionary().Keys.OrderBy(name => name);
                Assert.That(allActionNames.SequenceEqual(retrievedActions));

                // Test deleting 2 actions + adding one existing action and one new action to workflow
                fileProcessingDb.DefineNewAction(testDbName);

                actionNames =
                    new[] { _LABDE_ACTION2, testDbName, _LABDE_ACTION5 }; // In order of name
                workflowActions = actionNames
                    .Select(name =>
                    {
                        var actionInfo = new VariantVector();
                        actionInfo.PushBack(name);
                        actionInfo.PushBack(true);
                        return actionInfo;
                    })
                    .ToIUnknownVector();

                fileProcessingDb.SetWorkflowActions(id, workflowActions);

                workflowActions = fileProcessingDb.GetWorkflowActions(id);
                retrievedActions = workflowActions
                    .ToIEnumerable<IVariantVector>()
                    .Select(actionInfo => (string)actionInfo[1])
                    .OrderBy(name => name);
                Assert.That(actionNames.SequenceEqual(retrievedActions));

                // Confirm that action configuration is preserved if DB is cleared (retaining settings)
                fileProcessingDb.Clear(true);

                workflowActions = fileProcessingDb.GetWorkflowActions(id);
                retrievedActions = workflowActions
                    .ToIEnumerable<IVariantVector>()
                    .Select(actionInfo => (string)actionInfo[1])
                    .OrderBy(name => name);
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

                var workflowActions = new[] { "Start", "End" }
                    .Select(name =>
                    {
                        var actionInfo = new VariantVector();
                        actionInfo.PushBack(name);
                        actionInfo.PushBack(true);
                        return actionInfo;
                    })
                    .ToIUnknownVector();
                fileProcessingDb.SetWorkflowActions(workflowID, workflowActions);

                // Test that a separate action ID was assigned for the workflow
                int actionStart2 = fileProcessingDb.GetActionID("Start");
                Assert.That(actionStart1 != actionStart2);
                int actionEnd2 = fileProcessingDb.GetActionID("End");
                Assert.That(actionEnd1 != actionEnd2);

                // Test renaming an action while an active workflow is set.
                fileProcessingDb.RenameAction("Start", "Begin");
                var newWorkflowActions = fileProcessingDb.GetWorkflowActions(workflowID)
                    .ToIEnumerable<IVariantVector>()
                    .Select(actionInfo => (string)actionInfo[1])
                    .OrderBy(name => name)
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
                Assert.That(fileProcessingDb.GetWorkflowActions(workflowID).Size() == 0);
                Assert.That(fileProcessingDb.GetActions().Size == 0);
            }
            finally
            {
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        /// <summary>
        /// Tests Move files to different workflow
        /// </summary>
        [Test, Category("Automated")]
        public static void MoveWorkflow()
        {
            string testDbName = "Test_MoveWorkflow";
            IFileProcessingDB fileProcessingDb = CreateTestDatabase(testDbName);
            try
            {
                fileProcessingDb.RecordFAMSessionStart("Test.fps", _ACTION_A, true, false);
                
                string testfileName1 = _testFiles.GetFile(_LABDE_TEST_FILE1);
                bool alreadyExists = false;
                EActionStatus previousStatus;
                var fileRecord1 = fileProcessingDb.AddFile(testfileName1, _ACTION_A, -1, EFilePriority.kPriorityNormal,
                    false, false, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);

                int actionA_NoWorkflow_ID = fileProcessingDb.GetActionID(_ACTION_A);
                int actionB_NoWorkflow_ID = fileProcessingDb.GetActionID(_ACTION_B);

                fileProcessingDb.StartFileTaskSession(Constants.TaskClassDataEntryVerification, fileRecord1.FileID, actionA_NoWorkflow_ID);

                fileProcessingDb.RecordFAMSessionStop();

                var setStatusTaskConfig = new SetActionStatusFileProcessor();
                setStatusTaskConfig.ActionName = _ACTION_B;
                setStatusTaskConfig.ActionStatus = (int)EActionStatus.kActionPending;
                var setStatusTask = (IFileProcessingTask)setStatusTaskConfig;

                using (var famSession = new FAMProcessingSession(
                    fileProcessingDb, _ACTION_A, _ALL_WORKFLOWS, setStatusTask))
                {
                    famSession.WaitForProcessingToComplete();
                }

                var originalAS_ActionA = fileProcessingDb.GetStats(actionA_NoWorkflow_ID, false);
                var originalAS_ActionB = fileProcessingDb.GetStats(actionB_NoWorkflow_ID, false);

                int workflow1ID = fileProcessingDb.AddWorkflow(
                    "Workflow1", EWorkflowType.kUndefined, _ACTION_A, _ACTION_B, _ACTION_C);

                int count = 
                    fileProcessingDb.MoveFilesToWorkflowFromQuery("SELECT ID FROM FAMFILE", -1, workflow1ID);
                Assert.AreEqual(1, count);

                fileProcessingDb.ActiveWorkflow = "Workflow1";
                int actionA_Workflow1_ID = fileProcessingDb.GetActionID(_ACTION_A);
                int actionB_Workflow1_ID = fileProcessingDb.GetActionID(_ACTION_B);

 
                ActionStatistics blankAS = new ActionStatistics();

                var afterMoveOriginalActionA = fileProcessingDb.GetStats(actionA_NoWorkflow_ID, false);
                var afterMoveOriginalActionB = fileProcessingDb.GetStats(actionB_NoWorkflow_ID, false);

                // Original actions should now be blank
                Assert.That(StatsAreEqual(afterMoveOriginalActionA, blankAS));
                Assert.That(StatsAreEqual(afterMoveOriginalActionB, blankAS));

                var afterMoveWorkflow1ActionA = fileProcessingDb.GetStats(actionA_Workflow1_ID, false);
                var afterMoveWorkflow1ActionB = fileProcessingDb.GetStats(actionB_Workflow1_ID, false);

                Assert.That(StatsAreEqual(originalAS_ActionA, afterMoveWorkflow1ActionA));
                Assert.That(StatsAreEqual(originalAS_ActionB, afterMoveWorkflow1ActionB));

                // This should return 1 record with file ID of 
                var testRecordset = fileProcessingDb.GetResultsForQuery("SELECT * FROM WorkflowFile");
                Assert.That(testRecordset.RecordCount == 1, "There should be 1 record in WorkflowFile table");

                testRecordset.Filter = "FileID = " + fileRecord1.FileID.AsString() + " AND WorkflowID = " + workflow1ID.AsString();
                Assert.That(testRecordset.RecordCount == 1);

                // Actions are different should throw and exception
                int workflow2ID = fileProcessingDb.AddWorkflow(
                    "Workflow2", EWorkflowType.kUndefined, _ACTION_B, _ACTION_C);

                Assert.That(
                    Assert.Throws<COMException>(() =>
                        { fileProcessingDb.MoveFilesToWorkflowFromQuery("SELECT ID FROM FAMFILE", workflow1ID, workflow2ID); })
                        .AsExtract("TEST").Message == "Destination workflow is missing actions in the source workflow."
                );

                // Add another FAMSession
                fileProcessingDb.RecordFAMSessionStart("Test.fps", _ACTION_B, false, true);

                string testfileName2 = _testFiles.GetFile(_LABDE_TEST_FILE2);
                var fileRecord2 = fileProcessingDb.AddFile(testfileName2, _ACTION_B, -1, EFilePriority.kPriorityNormal,
                    false, false, EActionStatus.kActionSkipped, false, out alreadyExists, out previousStatus);

                fileProcessingDb.StartFileTaskSession(Constants.TaskClassDataEntryVerification, fileRecord2.FileID, actionB_Workflow1_ID);

                fileProcessingDb.RecordFAMSessionStop();

                var afterAddSkippedAS_ActionB = fileProcessingDb.GetStats(actionB_Workflow1_ID, false);

                int workflow3ID = fileProcessingDb.AddWorkflow(
                    "Workflow3", EWorkflowType.kUndefined, _ACTION_A, _ACTION_B, _ACTION_C);

                count =
                    fileProcessingDb.MoveFilesToWorkflowFromQuery("SELECT ID FROM FAMFILE", workflow1ID, workflow3ID);
                Assert.AreEqual(2, count);

                var statsForA1_W1_AfterMoveToW3 = fileProcessingDb.GetStats(actionA_Workflow1_ID, false);
                var statsForA2_W1_AfterMoveToW3 = fileProcessingDb.GetStats(actionB_Workflow1_ID, false);

                Assert.That(StatsAreEqual(statsForA1_W1_AfterMoveToW3, blankAS));
                Assert.That(StatsAreEqual(statsForA2_W1_AfterMoveToW3, blankAS));

                fileProcessingDb.ActiveWorkflow = "Workflow3";
                int actionA_Workflow3_ID = fileProcessingDb.GetActionID(_ACTION_A);
                int actionB_Workflow3_ID = fileProcessingDb.GetActionID(_ACTION_B);


                var statsForActionA_W3 = fileProcessingDb.GetStats(actionA_Workflow3_ID, false);
                var statsForActionB_W3 = fileProcessingDb.GetStats(actionB_Workflow3_ID, false);

                Assert.That(StatsAreEqual(originalAS_ActionA, statsForActionA_W3));
                Assert.That(StatsAreEqual(afterAddSkippedAS_ActionB, statsForActionB_W3));


                // Check records in workflow table
                testRecordset = fileProcessingDb.GetResultsForQuery("SELECT * FROM WorkflowFile");
                Assert.That(testRecordset.RecordCount == 2, "There should be 2 records in WorkflowFile table");

                testRecordset.Filter = "FileID = " + fileRecord1.FileID.AsString() + " AND WorkflowID = " + workflow3ID.AsString();
                Assert.That(testRecordset.RecordCount == 1);

                testRecordset.Filter = "FileID = " + fileRecord2.FileID.AsString() + " AND WorkflowID = " + workflow3ID.AsString();
                Assert.That(testRecordset.RecordCount == 1);

                // Check records in FileActionStatus table
                testRecordset = fileProcessingDb.GetResultsForQuery("SELECT * FROM FileActionStatus");
                Assert.That(testRecordset.RecordCount == 3, "There should be 3 records in FileActionStatus table");

                testRecordset.Filter = "ActionID = " + actionA_Workflow3_ID.AsString() + 
                    " AND FileID = " + fileRecord1.FileID.AsString() +" AND ActionStatus = 'C'";
                Assert.That(testRecordset.RecordCount == 1);

                testRecordset.Filter = "ActionID = " + actionB_Workflow3_ID.AsString() +
                    " AND FileID = " + fileRecord1.FileID.AsString() + " AND ActionStatus = 'P'";
                Assert.That(testRecordset.RecordCount == 1);

                testRecordset.Filter = "ActionID = " + actionB_Workflow3_ID.AsString() +
                    " AND FileID = " + fileRecord2.FileID.AsString() + " AND ActionStatus = 'S'";
                Assert.That(testRecordset.RecordCount == 1);

                // Check records in FileActionStateTransition table
                testRecordset = fileProcessingDb.GetResultsForQuery("SELECT * FROM FileActionStateTransition");
                Assert.That(testRecordset.RecordCount == 3, "There should be 3 records in FileActionStateTransition table");

                testRecordset.Filter = "ActionID = " + actionA_Workflow3_ID.AsString() + " AND ASC_From = 'P' AND ASC_To = 'R'";
                Assert.That(testRecordset.RecordCount == 1);

                testRecordset.Filter = "ActionID = " + actionA_Workflow3_ID.AsString() + " AND ASC_From = 'R' AND ASC_To = 'C'";
                Assert.That(testRecordset.RecordCount == 1);

                testRecordset.Filter = "ActionID = " + actionB_Workflow3_ID.AsString() + " AND ASC_From = 'U' AND ASC_To = 'P'";
                Assert.That(testRecordset.RecordCount == 1);

                // Check records in QueueEvent table
                testRecordset = fileProcessingDb.GetResultsForQuery("SELECT * FROM QueueEvent");
                Assert.That(testRecordset.RecordCount == 2, "There should be 2 records in QueueEvent table");

                testRecordset.Filter = "ActionID = " + actionA_Workflow3_ID.AsString() + " AND FileID = " + fileRecord1.FileID.AsString();
                Assert.That(testRecordset.RecordCount == 1);

                testRecordset.Filter = "ActionID = " + actionB_Workflow3_ID.AsString() + " AND FileID = " + fileRecord2.FileID.AsString();
                Assert.That(testRecordset.RecordCount == 1);

                // Check the FAMSession table records
                testRecordset = fileProcessingDb.GetResultsForQuery("SELECT * FROM FAMSession");
                Assert.That(testRecordset.RecordCount == 3, "There should be 3 records in FAMSession table");

                testRecordset.Filter = "ActionID = " + actionA_NoWorkflow_ID.AsString();
                Assert.That(testRecordset.RecordCount == 2);

                testRecordset.Filter = "ActionID = " + actionB_NoWorkflow_ID.AsString();
                Assert.That(testRecordset.RecordCount == 1);

                // Check the FileTaskSession records
                testRecordset = fileProcessingDb.GetResultsForQuery("SELECT * FROM FileTaskSession");
                Assert.That(testRecordset.RecordCount == 2, "There should be 2 records in FileTaskSession table");

                testRecordset.Filter = "ActionID = " + actionA_Workflow3_ID.AsString() + " AND FileID = " + fileRecord1.FileID.AsString();
                Assert.That(testRecordset.RecordCount == 1);

                testRecordset.Filter = "ActionID = " + actionB_Workflow3_ID.AsString() + " AND FileID = " + fileRecord2.FileID.AsString();
                Assert.That(testRecordset.RecordCount == 1);

                testRecordset.Filter = "ActionID = " + actionB_Workflow3_ID.AsString() + " AND FileID = " + fileRecord2.FileID.AsString();
                Assert.That(testRecordset.RecordCount == 1);
            }
            finally
            {
                fileProcessingDb.CloseAllDBConnections();
                fileProcessingDb = null;
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        /// <summary>
        /// Tests Move files to different workflow
        /// </summary>
        [Test, Category("Automated")]
        public static void MoveWorkflow_UnattemptedAndWithAction()
        {
            string testDbName = "Test_MoveWorkflow_UnattemptedAndWithAction";
            IFileProcessingDB fileProcessingDb = CreateTestDatabase(testDbName);
            try
            {
                string testfileName1 = _testFiles.GetFile(_LABDE_TEST_FILE1);
                string testfileName2 = _testFiles.GetFile(_LABDE_TEST_FILE2);

                bool alreadyExists = false;
                EActionStatus previousStatus;

                // 1 file on action and one file unattempted
                var fileRecord1 = fileProcessingDb.AddFile(testfileName1, _ACTION_A, -1, EFilePriority.kPriorityNormal,
                    false, false, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);
                var fileRecord2 = fileProcessingDb.AddFile(testfileName2, _ACTION_A, -1, EFilePriority.kPriorityNormal,
                    false, false, EActionStatus.kActionUnattempted, false, out alreadyExists, out previousStatus);
                var workflow1ID = fileProcessingDb.AddWorkflow("Workflow1", EWorkflowType.kUndefined, _ACTION_A, _ACTION_B, _ACTION_C);
                var workflow2ID = fileProcessingDb.AddWorkflow("Workflow2", EWorkflowType.kUndefined, _ACTION_A, _ACTION_B, _ACTION_C);

                fileProcessingDb.ActiveWorkflow = "Workflow1";
                var actionA_Workflow1_ID = fileProcessingDb.GetActionID(_ACTION_A);

                //var statsW1_1 = fileProcessingDb.GetStats(actionA_Workflow1_ID, false);

                fileProcessingDb.ActiveWorkflow = "Workflow2";
                var actionA_Workflow2_ID = fileProcessingDb.GetActionID(_ACTION_A);

                var statsW2_1 = fileProcessingDb.GetStats(actionA_Workflow2_ID, false);

                int count =
                    fileProcessingDb.MoveFilesToWorkflowFromQuery(
                    "SELECT ID FROM FAMFILE WHERE ID = " + fileRecord1.FileID.AsString(), -1, workflow1ID);
                Assert.AreEqual(1, count);

                fileProcessingDb.ActiveWorkflow = "Workflow1";
                var statsW1_2 = fileProcessingDb.GetStats(actionA_Workflow1_ID, false);
                Assert.That(statsW1_2.NumDocuments == 1 && statsW1_2.NumDocumentsPending == 1,
                    "One pending document for Workflow1");

                fileProcessingDb.ActiveWorkflow = "Workflow2";
                var statsW2_2 = fileProcessingDb.GetStats(actionA_Workflow2_ID, false);
                Assert.That(StatsAreEqual(statsW2_1, statsW2_2), "Workflow2 is unchanged after moving file to Workflow1");

                count =
                    fileProcessingDb.MoveFilesToWorkflowFromQuery("SELECT ID FROM FAMFILE", -1, workflow2ID);
                Assert.AreEqual(1, count);

                fileProcessingDb.ActiveWorkflow = "Workflow1";
                var statsW1_3 = fileProcessingDb.GetStats(actionA_Workflow1_ID, false);
                Assert.That(StatsAreEqual(statsW1_2, statsW1_3), "Workflow1 is unchanged after moving file to Workflow2");

                fileProcessingDb.ActiveWorkflow = "Workflow2";
                var statsW2_3 = fileProcessingDb.GetStats(actionA_Workflow2_ID, false);
                Assert.That(statsW2_3.NumDocuments == 1 && statsW2_3.NumDocumentsPending == 0,
                    "One document in Workflow2 that is not pending");

                // Check records in workflow table
                var testRecordset = fileProcessingDb.GetResultsForQuery("SELECT * FROM WorkflowFile");
                Assert.That(testRecordset.RecordCount == 2, "There should be 2 records in WorkflowFile table");

                testRecordset.Filter = "FileID = " + fileRecord1.FileID.AsString() + " AND WorkflowID = " + workflow1ID.AsString();
                Assert.That(testRecordset.RecordCount == 1, "One document for Workflow1");

                testRecordset.Filter = "FileID = " + fileRecord2.FileID.AsString() + " AND WorkflowID = " + workflow2ID.AsString();
                Assert.That(testRecordset.RecordCount == 1, "One document for Workflow2");
            }
            finally
            {
                fileProcessingDb.CloseAllDBConnections();
                fileProcessingDb = null;
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        /// Tests Move files to different workflow
        /// </summary>
        [Test, Category("Automated")]
        public static void MoveWorkflow_Unattempted()
        {
            string testDbName = "Test_MoveWorkflow_Unattempted";
            IFileProcessingDB fileProcessingDb = CreateTestDatabase(testDbName);
            try
            {
                string testfileName1 = _testFiles.GetFile(_LABDE_TEST_FILE1);
                string testfileName2 = _testFiles.GetFile(_LABDE_TEST_FILE2);

                bool alreadyExists = false;
                EActionStatus previousStatus;

                // add 2 unattempted files
                var fileRecord1 = fileProcessingDb.AddFile(testfileName1, _ACTION_A, -1, EFilePriority.kPriorityNormal,
                    false, false, EActionStatus.kActionUnattempted, false, out alreadyExists, out previousStatus);
                fileProcessingDb.AddFile(testfileName2, _ACTION_A, -1, EFilePriority.kPriorityNormal,
                    false, false, EActionStatus.kActionUnattempted, false, out alreadyExists, out previousStatus);
                var workflow1ID = fileProcessingDb.AddWorkflow("Workflow1", EWorkflowType.kUndefined, _ACTION_A, _ACTION_B, _ACTION_C);
                var workflow2ID = fileProcessingDb.AddWorkflow("Workflow2", EWorkflowType.kUndefined, _ACTION_A, _ACTION_B, _ACTION_C);

                fileProcessingDb.ActiveWorkflow = "Workflow1";
                var actionA_Workflow1_ID = fileProcessingDb.GetActionID(_ACTION_A);

                //var statsW1_1 = fileProcessingDb.GetStats(actionA_Workflow1_ID, false);

                fileProcessingDb.ActiveWorkflow = "Workflow2";
                var actionA_Workflow2_ID = fileProcessingDb.GetActionID(_ACTION_A);

                var statsW2_1 = fileProcessingDb.GetStats(actionA_Workflow2_ID, false);

                int count =
                    fileProcessingDb.MoveFilesToWorkflowFromQuery(
                        "SELECT ID FROM FAMFILE WHERE ID = " + fileRecord1.FileID.AsString(), -1, workflow1ID);
                Assert.AreEqual(1, count);

                fileProcessingDb.ActiveWorkflow = "Workflow1";
                var statsW1_2 = fileProcessingDb.GetStats(actionA_Workflow1_ID, false);
                Assert.That(statsW1_2.NumDocuments == 1, "One document in Workflow1");

                fileProcessingDb.ActiveWorkflow = "Workflow2";
                var statsW2_2 = fileProcessingDb.GetStats(actionA_Workflow2_ID, false);
                Assert.That(StatsAreEqual(statsW2_1, statsW2_2), "Workflow2 is unchanged after moving file to Workflow1");

                count = 
                    fileProcessingDb.MoveFilesToWorkflowFromQuery("SELECT ID FROM FAMFILE", -1, workflow2ID);
                Assert.AreEqual(1, count);

                fileProcessingDb.ActiveWorkflow = "Workflow1";
                var statsW1_3 = fileProcessingDb.GetStats(actionA_Workflow1_ID, false);
                Assert.That(StatsAreEqual(statsW1_2, statsW1_3), "Workflow1 is unchanged after moving file to Workflow2");

                fileProcessingDb.ActiveWorkflow = "Workflow2";
                var statsW2_3 = fileProcessingDb.GetStats(actionA_Workflow2_ID, false);
                Assert.That(statsW2_3.NumDocuments == 1, "There should be 1 record for Workflow2");

                count =
                    fileProcessingDb.MoveFilesToWorkflowFromQuery("SELECT ID FROM FAMFILE", workflow1ID, workflow2ID);
                Assert.AreEqual(1, count);

                ActionStatistics blankAS = new ActionStatistics();

                fileProcessingDb.ActiveWorkflow = "Workflow1";
                var statsW1_4 = fileProcessingDb.GetStats(actionA_Workflow1_ID, false);
                Assert.That(StatsAreEqual(blankAS, statsW1_4), "Workflow1 is not empty after moving file to Workflow2");

                fileProcessingDb.ActiveWorkflow = "Workflow2";
                var statsW2_4 = fileProcessingDb.GetStats(actionA_Workflow2_ID, false);
                Assert.That(statsW2_4.NumDocuments == 2, "There should be 2 records for Workflow2");

                // Check records in workflow table
                var testRecordset = fileProcessingDb.GetResultsForQuery("SELECT * FROM WorkflowFile");
                Assert.That(testRecordset.RecordCount == 2, "There should be 2 records in WorkflowFile table");

                testRecordset.Filter = "FileID = " + fileRecord1.FileID.AsString() + " AND WorkflowID = " + workflow1ID.AsString();
                Assert.That(testRecordset.RecordCount == 0, "WorkflowFile table contains 0 records for Workflow1");
            }
            finally
            {
                fileProcessingDb.CloseAllDBConnections();
                fileProcessingDb = null;
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        #endregion Test Methods

        #region Helper Methods

        static IFileProcessingDB CreateTestDatabase(string DBName)
        {
            var fileProcessingDB = _testDbManager.GetNewDatabase(DBName);

            // Create 2 actions
            fileProcessingDB.DefineNewAction(_ACTION_A);
            fileProcessingDB.DefineNewAction(_ACTION_B);
            fileProcessingDB.DefineNewAction(_ACTION_C);

            return fileProcessingDB;
        }

        /// <summary>
        /// Compares 2 ActionStatistics records for equality
        /// </summary>
        /// <param name="s1">First ActionStatistic to compare</param>
        /// <param name="s2">Second ActionStatistic to compare</param>
        /// <returns>True if s1 and s2 are equal, false otherwise</returns>
        static bool StatsAreEqual(ActionStatistics s1, ActionStatistics s2)
        {
            bool retValue = true;
            retValue = retValue && s1.NumBytes == s2.NumBytes;
            retValue = retValue && s1.NumBytesComplete == s2.NumBytesComplete;
            retValue = retValue && s1.NumBytesFailed == s2.NumBytesFailed;
            retValue = retValue && s1.NumBytesPending == s2.NumBytesPending;
            retValue = retValue && s1.NumBytesSkipped == s2.NumBytesSkipped;
            retValue = retValue && s1.NumDocuments == s2.NumDocuments;
            retValue = retValue && s1.NumDocumentsComplete == s2.NumDocumentsComplete;
            retValue = retValue && s1.NumDocumentsFailed == s2.NumDocumentsFailed;
            retValue = retValue && s1.NumDocumentsPending == s2.NumDocumentsPending;
            retValue = retValue && s1.NumDocumentsSkipped == s2.NumDocumentsSkipped;
            retValue = retValue && s1.NumPages == s2.NumPages;
            retValue = retValue && s1.NumPagesComplete == s2.NumPagesComplete;
            retValue = retValue && s1.NumPagesFailed == s2.NumPagesFailed;
            retValue = retValue && s1.NumPagesPending == s2.NumPagesPending;
            retValue = retValue && s1.NumPagesSkipped == s2.NumPagesSkipped;

            return retValue;
        }

        #endregion Helper Methods
    }
}
