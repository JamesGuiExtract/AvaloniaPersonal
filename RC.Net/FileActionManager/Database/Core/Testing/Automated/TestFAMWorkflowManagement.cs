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

        #endregion Constants

        #region Fields

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

            _testDbManager = new FAMTestDBManager<TestFAMWorkflowManagement>();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestFixtureTearDown]
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

        #endregion Test Methods
    }
}
