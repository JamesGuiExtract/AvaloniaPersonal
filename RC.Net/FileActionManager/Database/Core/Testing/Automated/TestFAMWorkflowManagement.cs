using Extract.Testing.Utilities;
using NUnit.Framework;
using System.Runtime.InteropServices;
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
        #region Fields

        /// <summary>
        /// Manages the test images needed for testing.
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
            // Dispose of the test image manager
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
                var fileProcessingDb = _testDbManager.GetDatabase("Resources.Demo_LabDE_Empty", testDbName);
                int id = fileProcessingDb.AddWorkflow(testDbName, EWorkflowType.kUndefined);
                IWorkflowDefinition workflowDefiniton = fileProcessingDb.GetWorkflowDefinition(id);
                Assert.That(workflowDefiniton.Name == testDbName);
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
                var fileProcessingDb = _testDbManager.GetDatabase("Resources.Demo_LabDE_Empty", testDbName);
                int id = fileProcessingDb.AddWorkflow(testDbName, EWorkflowType.kUndefined);
                IWorkflowDefinition workflowDefiniton = fileProcessingDb.GetWorkflowDefinition(id);
                Assert.That(workflowDefiniton.Name == testDbName);

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
                var fileProcessingDb = _testDbManager.GetDatabase("Resources.Demo_LabDE_Empty", testDbName);
                int id = fileProcessingDb.AddWorkflow(testDbName, EWorkflowType.kExtraction);

                // Test initial Workflow property values
                WorkflowDefinition outputDefinition = fileProcessingDb.GetWorkflowDefinition(id);
                Assert.That(outputDefinition.Name == testDbName);
                Assert.That(outputDefinition.Type == EWorkflowType.kExtraction);
                Assert.That(string.IsNullOrEmpty(outputDefinition.Description));
                Assert.That(string.IsNullOrEmpty(outputDefinition.StartAction));
                Assert.That(string.IsNullOrEmpty(outputDefinition.EndAction));
                Assert.That(string.IsNullOrEmpty(outputDefinition.PostWorkflowAction));
                Assert.That(string.IsNullOrEmpty(outputDefinition.DocumentFolder));
                Assert.That(string.IsNullOrEmpty(outputDefinition.OutputAttributeSet));

                // Update workflow properties.
                WorkflowDefinition workflowDefinition = new WorkflowDefinition();
                workflowDefinition.ID = id;
                workflowDefinition.Name = testDbName;
                workflowDefinition.Type = EWorkflowType.kClassification;
                workflowDefinition.Description = "A test of FileProcessingDB.SetWorkflowDefinition.";
                workflowDefinition.StartAction = "A01_ExtractData";
                workflowDefinition.EndAction = "A02_Verify";
                workflowDefinition.PostWorkflowAction = "Z_AdminAction";
                workflowDefinition.DocumentFolder = @"C:\Demo_LabDE\Input";
                workflowDefinition.OutputAttributeSet = "DataFoundByRules";
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
                var fileProcessingDb = _testDbManager.GetDatabase("Resources.Demo_LabDE_Empty", testDbName);

                // Create and configure workflow
                int id = fileProcessingDb.AddWorkflow(testDbName, EWorkflowType.kExtraction);

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
            }

            finally
            {
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        #endregion Test Methods
    }
}
