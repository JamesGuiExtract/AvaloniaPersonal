using System;
using System.IO;
using Extract.FileActionManager.Database.Test;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using UCLID_FILEPROCESSINGLib;
using UCLID_FILEPROCESSORSLib;

namespace Extract.FileActionManager.FileProcessors.Test
{
    /// <summary>
    /// Provides unit test cases for the COM class SetActionStatusFileProcessor in FileProcessors/>.
    /// </summary>
    [TestFixture]
    [Category("TestSetActionStatusFileProcessor")]
    public class TestSetActionStatusFileProcessor
    {
        #region Fields

        /// <summary>
        /// Manages test FAM DBs.
        /// </summary>
        static FAMTestDBManager<TestSetActionStatusFileProcessor> _testDbManager;

        #endregion Fields

        #region Constants

        static readonly string _FIRST_ACTION = "first";

        static readonly string _SECOND_ACTION = "second";

        static readonly string _ALL_WORKFLOWS = "<All workflows>";

        #endregion Constants

        #region Overhead

        /// <summary>
        /// Initializes the test fixture.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testDbManager = new FAMTestDBManager<TestSetActionStatusFileProcessor>();

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
        /// Test Set file action status with no workflows sets a different action to pending
        /// </summary>
        [Test, Category("Automated")]
        public static void TestSetFileActionStatusNoWorkflow()
        {
            using (TemporaryFile tmpFile1 = new TemporaryFile(false))
            {
                string testDBName = "Test_SFA_NoWorkflow";
                IFileProcessingDB fileProcessingDb;
                CreateTestDatabase(testDBName, out fileProcessingDb);

                try
                {
                    int firstAction = fileProcessingDb.GetActionID(_FIRST_ACTION);
                    int secondAction = fileProcessingDb.GetActionID(_SECOND_ACTION);

                    // Queue and process a file
                    fileProcessingDb.AddFile(tmpFile1.FileName, _FIRST_ACTION, -1, EFilePriority.kPriorityNormal,
                        false, false, EActionStatus.kActionPending, true, out bool alreadyExists, out EActionStatus previousStatus);

                    var setStatusTaskConfig = new SetActionStatusFileProcessor();
                    setStatusTaskConfig.ActionName = _SECOND_ACTION;
                    setStatusTaskConfig.ActionStatus = (int)EActionStatus.kActionPending;
                    var setStatusTask = (IFileProcessingTask)setStatusTaskConfig;

                    using (var famSession = new FAMProcessingSession(
                        fileProcessingDb, _FIRST_ACTION, "", setStatusTask))
                    {
                        famSession.WaitForProcessingToComplete();
                    }

                    // Ensure file has processed 
                    Assert.That(fileProcessingDb.GetStats(firstAction, false).NumDocumentsPending == 0);
                    Assert.That(fileProcessingDb.GetStats(firstAction, false).NumDocumentsComplete == 1);
                    Assert.That(fileProcessingDb.GetStats(secondAction, false).NumDocumentsPending == 1);
                }
                finally
                {
                    fileProcessingDb.CloseAllDBConnections();
                    fileProcessingDb = null;
                }
            }
        }

        /// <summary>
        /// Test Set file action status with no workflows defined. 
        /// Sets original action to failed if the destination file does not exist in the database
        /// and the ReportErrorWhenFileNotQueued property is true
        /// </summary>
        [Test, Category("Automated")]
        public static void TestSetFileActionStatusDestinationNotInDBError()
        {
            using (TemporaryFile tmpFile1 = new TemporaryFile(false))
            {
                // The destination file needs to exist so create it before processing
                using (StreamWriter sw = File.CreateText(tmpFile1.FileName + ".test"))
                {
                    sw.WriteLine("Test");
                    sw.Flush();
                    sw.Close();
                };

                string testDBName = "Test_SFA_DestinationNotInDBError";
                IFileProcessingDB fileProcessingDb;
                CreateTestDatabase(testDBName, out fileProcessingDb);

                try
                {
                    // Get the action id's
                    int firstAction = fileProcessingDb.GetActionID(_FIRST_ACTION);
                    int secondAction = fileProcessingDb.GetActionID(_SECOND_ACTION);

                    // Queue file
                    fileProcessingDb.AddFile(tmpFile1.FileName, _FIRST_ACTION, -1, EFilePriority.kPriorityNormal,
                        false, false, EActionStatus.kActionPending, true, out bool alreadyExists, out EActionStatus previousStatus);

                    // Set up SetActionStatus
                    var setStatusTaskConfig = new SetActionStatusFileProcessor();
                    setStatusTaskConfig.ActionName = _SECOND_ACTION;
                    setStatusTaskConfig.ActionStatus = (int)EActionStatus.kActionPending;
                    setStatusTaskConfig.DocumentName = "<SourceDocName>.test";
                    setStatusTaskConfig.ReportErrorWhenFileNotQueued = true;
                    var setStatusTask = (IFileProcessingTask)setStatusTaskConfig;

                    // Press the file
                    using (var famSession = new FAMProcessingSession(
                        fileProcessingDb, _FIRST_ACTION, "", setStatusTask))
                    {
                        famSession.WaitForProcessingToComplete();
                    }

                    // Check that the file has failed to processes
                    Assert.That(fileProcessingDb.GetStats(firstAction, false).NumDocumentsPending == 0);
                    Assert.That(fileProcessingDb.GetStats(firstAction, false).NumDocumentsFailed == 1);
                    Assert.That(fileProcessingDb.GetStats(firstAction, false).NumDocumentsComplete == 0);
                    Assert.That(fileProcessingDb.GetStats(secondAction, false).NumDocumentsPending == 0);
                }
                finally
                {
                    try
                    {
                        fileProcessingDb.CloseAllDBConnections();
                        fileProcessingDb = null;
	
	                    // Delete the .test files
	                    File.Delete(tmpFile1.FileName + ".test");
	                }
                    catch (Exception ex)
                    {
                        ex.ExtractLog("ELI42164");
                    }
                }
            }
        }

        /// <summary>
        /// Test Set file action status with no workflows defined. 
        /// Sets original action to completed if the destination file does not exist in the database
        /// and the ReportErrorWhenFileNotQueued property is false
        /// </summary>
        [Test, Category("Automated")]
        public static void TestSetFileActionStatusDestinationNotInDBNoError()
        {
            using (TemporaryFile tmpFile1 = new TemporaryFile(false))
            {
                // The destination file needs to exist so create it before processing
                using (StreamWriter sw = File.CreateText(tmpFile1.FileName + ".test"))
                {
                    sw.WriteLine("Test");
                    sw.Flush();
                    sw.Close();
                };

                string testDBName = "Test_SFA_DestinationNotInDBNoError";
                IFileProcessingDB fileProcessingDb;
                CreateTestDatabase(testDBName, out fileProcessingDb);

                try
                {
                    // Get Action Id's
                    int firstAction = fileProcessingDb.GetActionID(_FIRST_ACTION);
                    int secondAction = fileProcessingDb.GetActionID(_SECOND_ACTION);

                    // Queue a file
                    fileProcessingDb.AddFile(tmpFile1.FileName, _FIRST_ACTION, -1, EFilePriority.kPriorityNormal,
                        false, false, EActionStatus.kActionPending, true, out bool alreadyExists, out EActionStatus previousStatus);

                    // Setup Set Action Status
                    var setStatusTaskConfig = new SetActionStatusFileProcessor();
                    setStatusTaskConfig.ActionName = _SECOND_ACTION;
                    setStatusTaskConfig.ActionStatus = (int)EActionStatus.kActionPending;
                    setStatusTaskConfig.DocumentName = "<SourceDocName>.test";
                    setStatusTaskConfig.ReportErrorWhenFileNotQueued = false;
                    var setStatusTask = (IFileProcessingTask)setStatusTaskConfig;

                    // Process the file
                    using (var famSession = new FAMProcessingSession(
                        fileProcessingDb, _FIRST_ACTION, "", setStatusTask))
                    {
                        famSession.WaitForProcessingToComplete();
                    }

                    // Check the file has been processed and the second action has been set to pending
                    Assert.That(fileProcessingDb.GetStats(firstAction, false).NumDocumentsPending == 0);
                    Assert.That(fileProcessingDb.GetStats(firstAction, false).NumDocumentsFailed == 0);
                    Assert.That(fileProcessingDb.GetStats(firstAction, false).NumDocumentsComplete == 1);
                    Assert.That(fileProcessingDb.GetStats(secondAction, false).NumDocumentsPending == 1);
                }
                finally
                {
                    try
                    {
                        fileProcessingDb.CloseAllDBConnections();
                        fileProcessingDb = null;
	
                        // Delete the .test files
                        File.Delete(tmpFile1.FileName + ".test");
	                }
                    catch (Exception ex)
                    {
                        ex.ExtractLog("ELI42167");
                    }
                }
            }
        }

        /// <summary>
        /// Tests that the SetFileActionStatus processed properly within the context of a workflow.
        /// </summary>
        [Test, Category("Automated")]
        public static void TestSetFileActionStatusWithWorkflowsCurrentWorkflow()
        {
            string testDBName = "Test_SFA_WithWorkflows";
            IFileProcessingDB fileProcessingDb;
            CreateTestDatabase(testDBName, out fileProcessingDb);

            try
            {
                using (TemporaryFile tmpFile1 = new TemporaryFile(false))
                using (TemporaryFile tmpFile2 = new TemporaryFile(false))
                {
                    // Create 2 workflows
                    int workflowID1 = fileProcessingDb.AddWorkflow("Workflow1", EWorkflowType.kUndefined,
                        _FIRST_ACTION, _SECOND_ACTION);
                    fileProcessingDb.ActiveWorkflow = "Workflow1";

                    // Get the action ID's for the first workflow
                    int firstAction1 = fileProcessingDb.GetActionID(_FIRST_ACTION);
                    int secondAction1 = fileProcessingDb.GetActionID(_SECOND_ACTION);

                    int workflowID2 = fileProcessingDb.AddWorkflow("Workflow2", EWorkflowType.kUndefined,
                        _FIRST_ACTION, _SECOND_ACTION);
                    fileProcessingDb.ActiveWorkflow = "Workflow2";

                    int firstAction2 = fileProcessingDb.GetActionID(_FIRST_ACTION);
                    int secondAction2 = fileProcessingDb.GetActionID(_SECOND_ACTION);

                    // Queue a file in workflow 1
                    fileProcessingDb.ActiveWorkflow = "Workflow1";
                    bool alreadyExists = false;
                    EActionStatus previousStatus;
                    fileProcessingDb.AddFile(tmpFile1.FileName, _FIRST_ACTION, workflowID1, EFilePriority.kPriorityNormal,
                        false, false, EActionStatus.kActionPending, true, out alreadyExists, out previousStatus);

                    // Setup Set Action Status to set the second action on the current workflow
                    var setStatusTaskConfig = new SetActionStatusFileProcessor();
                    setStatusTaskConfig.ActionName = _SECOND_ACTION;
                    setStatusTaskConfig.ActionStatus = (int)EActionStatus.kActionPending;
                    var setStatusTask = (IFileProcessingTask)setStatusTaskConfig;

                    // Process first action on workflow1
                    using (var famSession = new FAMProcessingSession(
                        fileProcessingDb, _FIRST_ACTION, "Workflow1", setStatusTask))
                    {
                        famSession.WaitForProcessingToComplete();
                    }

                    // Check that the file was processed on action 1 in workflow 1 and set action 2 in workflow 1
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows(_FIRST_ACTION, false).NumDocumentsPending == 0);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows(_FIRST_ACTION, false).NumDocumentsComplete == 1);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows(_SECOND_ACTION, false).NumDocumentsPending == 1);

                    Assert.That(fileProcessingDb.GetStats(firstAction1, false).NumDocumentsComplete == 1);
                    Assert.That(fileProcessingDb.GetStats(secondAction1, false).NumDocumentsPending == 1);

                    Assert.That(fileProcessingDb.GetStats(firstAction2, false).NumDocumentsComplete == 0);
                    Assert.That(fileProcessingDb.GetStats(secondAction2, false).NumDocumentsPending == 0);
                }
            }
            finally
            {
                fileProcessingDb.CloseAllDBConnections();
                fileProcessingDb = null;
            }
        }

        /// <summary>
        /// Tests that the SetFileActionStatus processed properly within the context of a workflow.
        /// This processes all workflows for action1 and sets action 2 for the "current" workflow 
        /// </summary>
        [Test, Category("Automated")]
        public static void TestSetFileActionStatusWithAllWorkflowsCurrentWorkflow()
        {
            string testDBName = "Test_SFA_WithWorkflowsAll_Current";
            IFileProcessingDB fileProcessingDb;
            CreateTestDatabase(testDBName, out fileProcessingDb);

            try
            {
                using (TemporaryFile tmpFile1 = new TemporaryFile(false))
                using (TemporaryFile tmpFile2 = new TemporaryFile(false))
                {

                    // Create 2 workflows
                    int workflowID1 = fileProcessingDb.AddWorkflow("Workflow1", EWorkflowType.kUndefined,
                        _FIRST_ACTION, _SECOND_ACTION);
                    fileProcessingDb.ActiveWorkflow = "Workflow1";

                    int firstAction1 = fileProcessingDb.GetActionID(_FIRST_ACTION);
                    int secondAction1 = fileProcessingDb.GetActionID(_SECOND_ACTION);

                    int workflowID2 = fileProcessingDb.AddWorkflow("Workflow2", EWorkflowType.kUndefined,
                        _FIRST_ACTION, _SECOND_ACTION);
                    fileProcessingDb.ActiveWorkflow = "Workflow2";

                    int firstAction2 = fileProcessingDb.GetActionID(_FIRST_ACTION);
                    int secondAction2 = fileProcessingDb.GetActionID(_SECOND_ACTION);

                    // Queue a file in workflow 1
                    fileProcessingDb.ActiveWorkflow = "Workflow1";
                    bool alreadyExists = false;
                    EActionStatus previousStatus;
                    fileProcessingDb.AddFile(tmpFile1.FileName, _FIRST_ACTION, workflowID1, EFilePriority.kPriorityNormal,
                        false, false, EActionStatus.kActionPending, true, out alreadyExists, out previousStatus);

                    // Queue a file in workflow 2
                    fileProcessingDb.ActiveWorkflow = "Workflow2";
                    fileProcessingDb.AddFile(tmpFile2.FileName, _FIRST_ACTION, workflowID2, EFilePriority.kPriorityNormal,
                        false, false, EActionStatus.kActionPending, true, out alreadyExists, out previousStatus);

                    // Check that both files were queued
                    Assert.That(fileProcessingDb.GetStats(firstAction1, false).NumDocumentsPending == 1);
                    Assert.That(fileProcessingDb.GetStats(firstAction2, false).NumDocumentsPending == 1);

                    // Setup SetActionStatus to set action 2 of current workflow to pending
                    var setStatusTaskConfig = new SetActionStatusFileProcessor();
                    setStatusTaskConfig.ActionName = _SECOND_ACTION;
                    setStatusTaskConfig.ActionStatus = (int)EActionStatus.kActionPending;
                    var setStatusTask = (IFileProcessingTask)setStatusTaskConfig;

                    // Process all workflows
                    using (var famSession = new FAMProcessingSession(
                        fileProcessingDb, _FIRST_ACTION, _ALL_WORKFLOWS, setStatusTask))
                    {
                        famSession.WaitForProcessingToComplete();
                    }

                    // Check that the files were processed for all workflows for action 1 and action 2 was set to pending for each workflow 
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows(_FIRST_ACTION, false).NumDocumentsPending == 0);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows(_FIRST_ACTION, false).NumDocumentsComplete == 2);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows(_SECOND_ACTION, false).NumDocumentsPending == 2);

                    Assert.That(fileProcessingDb.GetStats(firstAction1, false).NumDocumentsComplete == 1);
                    Assert.That(fileProcessingDb.GetStats(firstAction2, false).NumDocumentsComplete == 1);
                    Assert.That(fileProcessingDb.GetStats(secondAction1, false).NumDocumentsPending == 1);
                    Assert.That(fileProcessingDb.GetStats(secondAction2, false).NumDocumentsPending == 1);
                }
            }
            finally
            {
                fileProcessingDb.CloseAllDBConnections();
                fileProcessingDb = null;
            }
        }

        /// <summary>
        /// Tests that the SetFileActionStatus processed properly within the context of a workflow.
        /// This queues a file on 2 workflows for action1 and sets a second action to pending for a 3rd workflow
        /// </summary>
        [Test, Category("Automated")]
        public static void TestSetFileActionStatusWithAllWorkflowsSpecifiedWorkflow()
        {
            string testDBName = "Test_SFA_WithWorkflowsAll_Specified";
            IFileProcessingDB fileProcessingDb;
            CreateTestDatabase(testDBName, out fileProcessingDb);

            try
            {
                // Needed temporary file to queue
                using (TemporaryFile tmpFile1 = new TemporaryFile(false))
                using (TemporaryFile tmpFile2 = new TemporaryFile(false))
                {
                    // Create 3 workflows
                    int workflowID1 = fileProcessingDb.AddWorkflow("Workflow1", EWorkflowType.kUndefined,
                        _FIRST_ACTION, _SECOND_ACTION);
                    fileProcessingDb.ActiveWorkflow = "Workflow1";

                    int firstAction1 = fileProcessingDb.GetActionID(_FIRST_ACTION);
                    int secondAction1 = fileProcessingDb.GetActionID(_SECOND_ACTION);

                    int workflowID2 = fileProcessingDb.AddWorkflow("Workflow2", EWorkflowType.kUndefined,
                        _FIRST_ACTION, _SECOND_ACTION);
                    fileProcessingDb.ActiveWorkflow = "Workflow2";

                    int firstAction2 = fileProcessingDb.GetActionID(_FIRST_ACTION);
                    int secondAction2 = fileProcessingDb.GetActionID(_SECOND_ACTION);

                    int workflowID3 = fileProcessingDb.AddWorkflow("Workflow3", EWorkflowType.kUndefined,
                        _SECOND_ACTION);

                    fileProcessingDb.ActiveWorkflow = "Workflow3";
                    int secondAction3 = fileProcessingDb.GetActionID(_SECOND_ACTION);

                    // Queue a file in workflow 1
                    fileProcessingDb.ActiveWorkflow = "Workflow1";
                    bool alreadyExists = false;
                    EActionStatus previousStatus;
                    fileProcessingDb.AddFile(tmpFile1.FileName, _FIRST_ACTION, workflowID1, EFilePriority.kPriorityNormal,
                        false, false, EActionStatus.kActionPending, true, out alreadyExists, out previousStatus);

                    // Queue a file in workflow2
                    fileProcessingDb.ActiveWorkflow = "Workflow2";
                    fileProcessingDb.AddFile(tmpFile2.FileName, _FIRST_ACTION, workflowID2, EFilePriority.kPriorityNormal,
                        false, false, EActionStatus.kActionPending, true, out alreadyExists, out previousStatus);

                    // Set up SetActionStatus to set Second action on workflow3 to pending
                    var setStatusTaskConfig = new SetActionStatusFileProcessor();
                    setStatusTaskConfig.ActionName = _SECOND_ACTION;
                    setStatusTaskConfig.Workflow = "Workflow3";
                    setStatusTaskConfig.ActionStatus = (int)EActionStatus.kActionPending;
                    var setStatusTask = (IFileProcessingTask)setStatusTaskConfig;

                    // Process files for all workflows
                    using (var famSession = new FAMProcessingSession(
                        fileProcessingDb, _FIRST_ACTION, _ALL_WORKFLOWS, setStatusTask))
                    {
                        famSession.WaitForProcessingToComplete();
                    }

                    // Check that all files were processed and the secondAction has only be set to pending on workflow3
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows(_FIRST_ACTION, false).NumDocumentsPending == 0);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows(_FIRST_ACTION, false).NumDocumentsComplete == 2);
                    Assert.That(fileProcessingDb.GetStatsAllWorkflows(_SECOND_ACTION, false).NumDocumentsPending == 2);

                    Assert.That(fileProcessingDb.GetStats(firstAction1, false).NumDocumentsComplete == 1);
                    Assert.That(fileProcessingDb.GetStats(firstAction2, false).NumDocumentsComplete == 1);
                    Assert.That(fileProcessingDb.GetStats(secondAction1, false).NumDocumentsPending == 0);
                    Assert.That(fileProcessingDb.GetStats(secondAction2, false).NumDocumentsPending == 0);
                    Assert.That(fileProcessingDb.GetStats(secondAction3, false).NumDocumentsPending == 2);
                }
            }
            finally
            {
                fileProcessingDb.CloseAllDBConnections();
                fileProcessingDb = null;
            }
        
        }

        #endregion Test Methods

        #region Helper Methods

        static void CreateTestDatabase(string DBName, out IFileProcessingDB fileProcessingDB)
        {
            fileProcessingDB = _testDbManager.GetNewDatabase(DBName);

            // Create 2 actions
            fileProcessingDB.DefineNewAction(_FIRST_ACTION);
            fileProcessingDB.DefineNewAction(_SECOND_ACTION);
        }

        #endregion Helper Methods
    }
}
