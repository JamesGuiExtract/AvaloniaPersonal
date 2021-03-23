using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
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
                    _LABDE_ACTION3, EActionStatus.kActionPending, "", false);

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
                    _LABDE_ACTION2, EActionStatus.kActionSkipped, "", false);

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
        [Category("Automated")]
        [TestCase(false)]
        [TestCase(true, 1, 1)]
        [TestCase(true, 1, 2)]
        [TestCase(true, 2, 1)]
        [TestCase(true, 2, 2)]
        [TestCase(true, 2, 3)]
        public static void ModifyActionStatusForSelectionWithWorkflows(bool deleteFilesFromWorkflow, int workflowToDeleteFrom = 0, int fileToDelete = 0)
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
                var expectedWorkflowStatuses = new ActionStatus[][]
                {
                    new ActionStatus[]
                    {
                        new ActionStatus{P = new[] { 1 }, C = new[] { 2 } },
                        new ActionStatus{P = new[] { 2 } },
                        new ActionStatus(),
                    },
                    new ActionStatus[]
                    {
                        new ActionStatus{C = new[] { 1, 2 } },
                        new ActionStatus{P = new[] { 1 }, C = new[] { 2 } },
                        new ActionStatus{P = new[] { 3 }, F = new[] { 2 } },
                    }
                };
                ActionStatusCounts[] expectedCountsByAction = expectedWorkflowStatuses.ComputeCountsFromIDsByAction();

                int workflowID1 = fileProcessingDb.AddWorkflow(
                    "Workflow1", EWorkflowType.kUndefined, _LABDE_ACTION1, _LABDE_ACTION2);
                fileProcessingDb.AddFile(testFileName1, _LABDE_ACTION1, workflowID1, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionPending, false, out _, out _);
                fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION1, workflowID1, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionCompleted, false, out _, out _);
                fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION2, workflowID1, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionPending, false, out _, out _);

                int workflowID2 = fileProcessingDb.AddWorkflow(
                    "Workflow2", EWorkflowType.kUndefined, _LABDE_ACTION1, _LABDE_ACTION2, _LABDE_ACTION3);

                fileProcessingDb.AddFile(testFileName1, _LABDE_ACTION1, workflowID2, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionCompleted, false, out _, out _);
                fileProcessingDb.AddFile(testFileName1, _LABDE_ACTION2, workflowID2, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionPending, false, out _, out _);
                fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION1, workflowID2, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionCompleted, false, out _, out _);
                fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION2, workflowID2, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionCompleted, false, out _, out _);
                fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION3, workflowID2, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionFailed, false, out _, out _);
                fileProcessingDb.AddFile(testFileName3, _LABDE_ACTION3, workflowID2, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionPending, false, out _, out _);

                string[] actions = { _LABDE_ACTION1, _LABDE_ACTION2, _LABDE_ACTION3 };
                var actualCountsByAction = actions.Select(a => fileProcessingDb.GetStatsAllWorkflows(a, false)).ComputeCountsFromActionStatisticsByAction();
                CollectionAssert.AreEquivalent(expectedCountsByAction, actualCountsByAction);

                // Separate the expected data. If not deleting then these will be the same as total and empty, respectively
                ActionStatus[][] expectedNonDeletedStatus = expectedWorkflowStatuses;
                ActionStatus[][] expectedDeletedStatus = expectedWorkflowStatuses.KeepOnlySpecifiedFile(0, 0);

                if (deleteFilesFromWorkflow)
                {
                    fileProcessingDb.MarkFileDeleted(fileToDelete, workflowToDeleteFrom);

                    expectedNonDeletedStatus = expectedWorkflowStatuses.RemoveFileFromWorkflow(workflowToDeleteFrom, fileToDelete);
                    var expectedNonDeletedCountsByAction = expectedNonDeletedStatus.ComputeCountsFromIDsByAction();

                    var actualNonDeletedCountsByAction = actions
                        .Select(action => fileProcessingDb.GetStatsAllWorkflows(action, false))
                        .ComputeCountsFromActionStatisticsByAction();
                    CollectionAssert.AreEquivalent(expectedNonDeletedCountsByAction, actualNonDeletedCountsByAction);

                    expectedDeletedStatus = expectedWorkflowStatuses.KeepOnlySpecifiedFile(workflowToDeleteFrom, fileToDelete);
                    var expectedDeletedCountsByAction = expectedDeletedStatus.ComputeCountsFromIDsByAction();

                    var actualDeletedCountsByAction = actions.Select(a => fileProcessingDb.GetDeletedFileStatsAllWorkflows(a, false)).ComputeCountsFromActionStatisticsByAction();
                    CollectionAssert.AreEquivalent(expectedDeletedCountsByAction, actualDeletedCountsByAction);
                }

                // Make workflow 1 active
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                int actionId1 = fileProcessingDb.GetActionID(_LABDE_ACTION1);
                int actionId2 = fileProcessingDb.GetActionID(_LABDE_ACTION2);
                Assert.AreEqual(2, fileProcessingDb.GetFileCount(false));
                Assert.AreEqual(2, fileProcessingDb.GetStats(actionId1, false).NumDocuments + fileProcessingDb.GetDeletedFileStats(actionId1, false).NumDocuments);

                var fileSelector = new FAMFileSelector();
                fileSelector.AddQueryCondition("SELECT [FAMFile].[ID] FROM [FAMFile]");
                fileProcessingDb.ModifyActionStatusForSelection(fileSelector,
                    _LABDE_ACTION1, EActionStatus.kActionUnattempted, "", false);

                // Statuses after ModifyActionStatusForSelection in Workflow 1
                //            |  P  |  R  |  S  |  C  |  F 
                // Workflow 1 ----------------------------   
                //   Action 1 
                //   Action 2   2
                // Workflow 2 ----------------------------   
                //   Action 1                     1,2           
                //   Action 2   1                  2
                //   Action 3   3                        2

                expectedWorkflowStatuses = new ActionStatus[][]
                {
                    new ActionStatus[]
                    {
                        new ActionStatus(),
                        new ActionStatus{P = new[] { 2 } },
                        new ActionStatus(),
                    },
                    new ActionStatus[]
                    {
                        new ActionStatus{C = new[] { 1, 2 } },
                        new ActionStatus{P = new[] { 1 }, C = new[] { 2 } },
                        new ActionStatus{P = new[] { 3 }, F = new[] { 2 } },
                    }
                };

                if (deleteFilesFromWorkflow)
                {
                    expectedNonDeletedStatus = expectedWorkflowStatuses.RemoveFileFromWorkflow(workflowToDeleteFrom, fileToDelete);
                    expectedDeletedStatus = expectedWorkflowStatuses.KeepOnlySpecifiedFile(workflowToDeleteFrom, fileToDelete);
                }
                else
                {
                    expectedNonDeletedStatus = expectedWorkflowStatuses;
                    expectedDeletedStatus = expectedWorkflowStatuses.KeepOnlySpecifiedFile(0, 0);
                }

                Assert.That(fileProcessingDb.GetFileCount(false) == 2);

                // Compute the expected stats for workflow 1
                var expectedNonDeletedStats = expectedNonDeletedStatus[0].ComputeCountsFromIDsByAction();
                var expectedDeletedStats = expectedDeletedStatus[0].ComputeCountsFromIDsByAction();

                var actionIDs = new[] { actionId1, actionId2, 0 };
                var actualNonDeletedStats = actionIDs
                    .Select(actionID => actionID == 0 ? new ActionStatisticsClass() : fileProcessingDb.GetStats(actionID, false))
                    .ToArray()
                    .ComputeCountsFromActionStatisticsByAction();

                var actualDeletedStats = actionIDs
                    .Select(actionID => actionID == 0 ? new ActionStatisticsClass() : fileProcessingDb.GetDeletedFileStats(actionID, false))
                    .ToArray()
                    .ComputeCountsFromActionStatisticsByAction();

                CollectionAssert.AreEquivalent(expectedNonDeletedStats, actualNonDeletedStats);
                CollectionAssert.AreEquivalent(expectedDeletedStats, actualDeletedStats);

                // Compute expected numbers for action 1, all workflows
                var expectedNonDeletedAction1Stats = expectedNonDeletedStatus.ComputeCountsFromIDsByAction()[0];
                var expectedDeletedAction1Stats = expectedDeletedStatus.ComputeCountsFromIDsByAction()[0];
                var expectedCombinedStats = expectedNonDeletedAction1Stats + expectedDeletedAction1Stats;

                // Sanity check, combined numbers match previous expected data for action 1
                Assert.AreEqual(new ActionStatusCounts { C = 2 }, expectedCombinedStats);

                var actualNonDeletedAction1Stats = fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION1, false).ComputeCountsFromActionStatistics();
                Assert.AreEqual(expectedNonDeletedAction1Stats, actualNonDeletedAction1Stats);

                var actualDeletedAction1Stats = fileProcessingDb.GetDeletedFileStatsAllWorkflows(_LABDE_ACTION1, false).ComputeCountsFromActionStatistics();
                Assert.AreEqual(expectedDeletedAction1Stats, actualDeletedAction1Stats);

                // Make workflow 2 active
                fileProcessingDb.ActiveWorkflow = "Workflow2";
                actionId1 = fileProcessingDb.GetActionID(_LABDE_ACTION1);
                actionId2 = fileProcessingDb.GetActionID(_LABDE_ACTION2);
                int actionId3 = fileProcessingDb.GetActionID(_LABDE_ACTION3);

                // Compute the expected stats for workflow 2
                expectedNonDeletedStats = expectedNonDeletedStatus[1].ComputeCountsFromIDsByAction();
                expectedDeletedStats = expectedDeletedStatus[1].ComputeCountsFromIDsByAction();

                actionIDs = new[] { actionId1, actionId2, actionId3 };
                actualNonDeletedStats = actionIDs
                    .Select(actionID => fileProcessingDb.GetStats(actionID, false))
                    .ToArray()
                    .ComputeCountsFromActionStatisticsByAction();

                actualDeletedStats = actionIDs
                    .Select(actionID => fileProcessingDb.GetDeletedFileStats(actionID, false))
                    .ToArray()
                    .ComputeCountsFromActionStatisticsByAction();

                CollectionAssert.AreEquivalent(expectedNonDeletedStats, actualNonDeletedStats);
                CollectionAssert.AreEquivalent(expectedDeletedStats, actualDeletedStats);

                Assert.That(fileProcessingDb.GetFileCount(false) == 3);

                fileSelector.Reset();
                fileSelector.AddQueryCondition("SELECT [FAMFile].[ID] FROM [FAMFile] " +
                    "   INNER JOIN [FileActionStatus] ON [FileID] = [FAMFile].[ID] AND [ActionStatus] = 'F'");
                fileProcessingDb.ModifyActionStatusForSelection(fileSelector,
                    _LABDE_ACTION3, EActionStatus.kActionPending, "", false);

                // Statuses after ModifyActionStatusForSelection in Workflow 2
                //            |  P  |  R  |  S  |  C  |  F 
                // Workflow 1 ----------------------------  
                //   Action 1 
                //   Action 2   2
                // Workflow 2 ---------------------------- 
                //   Action 1                     1,2           
                //   Action 2   1                  2
                //   Action 3  2,3                

                expectedWorkflowStatuses = new ActionStatus[][]
                {
                    new ActionStatus[]
                    {
                        new ActionStatus(),
                        new ActionStatus{P = new[] { 2 } },
                        new ActionStatus(),
                    },
                    new ActionStatus[]
                    {
                        new ActionStatus{C = new[] { 1, 2 } },
                        new ActionStatus{P = new[] { 1 }, C = new[] { 2 } },
                        new ActionStatus{P = new[] { 2, 3 } },
                    }
                };

                if (deleteFilesFromWorkflow)
                {
                    expectedNonDeletedStatus = expectedWorkflowStatuses.RemoveFileFromWorkflow(workflowToDeleteFrom, fileToDelete);
                    expectedDeletedStatus = expectedWorkflowStatuses.KeepOnlySpecifiedFile(workflowToDeleteFrom, fileToDelete);
                }
                else
                {
                    expectedNonDeletedStatus = expectedWorkflowStatuses;
                    expectedDeletedStatus = expectedWorkflowStatuses.KeepOnlySpecifiedFile(0, 0);
                }

                // Compute the expected stats for workflow 2
                expectedNonDeletedStats = expectedNonDeletedStatus[1].ComputeCountsFromIDsByAction();
                expectedDeletedStats = expectedDeletedStatus[1].ComputeCountsFromIDsByAction();

                actionIDs = new[] { actionId1, actionId2, actionId3 };
                actualNonDeletedStats = actionIDs
                    .Select(actionID => fileProcessingDb.GetStats(actionID, false))
                    .ToArray()
                    .ComputeCountsFromActionStatisticsByAction();

                actualDeletedStats = actionIDs
                    .Select(actionID => fileProcessingDb.GetDeletedFileStats(actionID, false))
                    .ToArray()
                    .ComputeCountsFromActionStatisticsByAction();

                CollectionAssert.AreEquivalent(expectedNonDeletedStats, actualNonDeletedStats);
                CollectionAssert.AreEquivalent(expectedDeletedStats, actualDeletedStats);


                // Make all workflows active
                fileProcessingDb.ActiveWorkflow = "";

                fileSelector.Reset();
                fileSelector.AddQueryCondition("SELECT [FAMFile].[ID] FROM [FAMFile] WHERE [ID] = 2 OR [ID] = 3");
                fileProcessingDb.ModifyActionStatusForSelection(fileSelector,
                    _LABDE_ACTION2, EActionStatus.kActionSkipped, "", false);

                // Statuses after ModifyActionStatusForSelection for all workflows
                //            |  P  |  R  |  S  |  C  |  F 
                // Workflow 1 ----------------------------  
                //   Action 1 
                //   Action 2                2
                // Workflow 2 ----------------------------
                //   Action 1                     1,2           
                //   Action 2   1           2,3       
                //   Action 3  2,3      

                expectedWorkflowStatuses = new ActionStatus[][]
                {
                    new ActionStatus[]
                    {
                        new ActionStatus(),
                        new ActionStatus{S = new[] { 2 } },
                        new ActionStatus(),
                    },
                    new ActionStatus[]
                    {
                        new ActionStatus{C = new[] { 1, 2 } },
                        new ActionStatus{P = new[] { 1 }, S = new[] { 2, 3 } },
                        new ActionStatus{P = new[] { 2, 3 } },
                    }
                };

                if (deleteFilesFromWorkflow)
                {
                    expectedNonDeletedStatus = expectedWorkflowStatuses.RemoveFileFromWorkflow(workflowToDeleteFrom, fileToDelete);
                    expectedDeletedStatus = expectedWorkflowStatuses.KeepOnlySpecifiedFile(workflowToDeleteFrom, fileToDelete);
                }
                else
                {
                    expectedNonDeletedStatus = expectedWorkflowStatuses;
                    expectedDeletedStatus = expectedWorkflowStatuses.KeepOnlySpecifiedFile(0, 0);
                }

                Assert.AreEqual(3, fileProcessingDb.GetFileCount(false));

                // Compute the expected stats for all workflows
                expectedNonDeletedStats = expectedNonDeletedStatus.ComputeCountsFromIDsByAction();
                expectedDeletedStats = expectedDeletedStatus.ComputeCountsFromIDsByAction();

                actualNonDeletedStats = actions.Select(a => fileProcessingDb.GetStatsAllWorkflows(a, false)).ComputeCountsFromActionStatisticsByAction();
                CollectionAssert.AreEquivalent(expectedNonDeletedStats, actualNonDeletedStats);

                actualDeletedStats = actions.Select(a => fileProcessingDb.GetDeletedFileStatsAllWorkflows(a, false)).ComputeCountsFromActionStatisticsByAction();
                CollectionAssert.AreEquivalent(expectedDeletedStats, actualDeletedStats);

                // Make workflow 1 active
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                actionId1 = fileProcessingDb.GetActionID(_LABDE_ACTION1);
                actionId2 = fileProcessingDb.GetActionID(_LABDE_ACTION2);

                // Compute the expected stats for workflow 1
                expectedNonDeletedStats = expectedNonDeletedStatus[0].ComputeCountsFromIDsByAction();
                expectedDeletedStats = expectedDeletedStatus[0].ComputeCountsFromIDsByAction();

                actionIDs = new[] { actionId1, actionId2, 0 };
                actualNonDeletedStats = actionIDs
                    .Select(actionID => actionID == 0 ? new ActionStatisticsClass() : fileProcessingDb.GetStats(actionID, false))
                    .ToArray()
                    .ComputeCountsFromActionStatisticsByAction();

                actualDeletedStats = actionIDs
                    .Select(actionID => actionID == 0 ? new ActionStatisticsClass() : fileProcessingDb.GetDeletedFileStats(actionID, false))
                    .ToArray()
                    .ComputeCountsFromActionStatisticsByAction();

                CollectionAssert.AreEquivalent(expectedNonDeletedStats, actualNonDeletedStats);
                CollectionAssert.AreEquivalent(expectedDeletedStats, actualDeletedStats);

                Assert.AreEqual(2, fileProcessingDb.GetFileCount(false));

                // Make workflow 2 active
                fileProcessingDb.ActiveWorkflow = "Workflow2";
                actionId1 = fileProcessingDb.GetActionID(_LABDE_ACTION1);
                actionId2 = fileProcessingDb.GetActionID(_LABDE_ACTION2);

                // Compute the expected stats for workflow 2
                expectedNonDeletedStats = expectedNonDeletedStatus[1].ComputeCountsFromIDsByAction();
                expectedDeletedStats = expectedDeletedStatus[1].ComputeCountsFromIDsByAction();

                actionIDs = new[] { actionId1, actionId2, actionId3 };
                actualNonDeletedStats = actionIDs
                    .Select(actionID => fileProcessingDb.GetStats(actionID, false))
                    .ToArray()
                    .ComputeCountsFromActionStatisticsByAction();

                actualDeletedStats = actionIDs
                    .Select(actionID => fileProcessingDb.GetDeletedFileStats(actionID, false))
                    .ToArray()
                    .ComputeCountsFromActionStatisticsByAction();

                CollectionAssert.AreEquivalent(expectedNonDeletedStats, actualNonDeletedStats);
                CollectionAssert.AreEquivalent(expectedDeletedStats, actualDeletedStats);

                Assert.AreEqual(3, fileProcessingDb.GetFileCount(false));
            }
            finally
            {
                _testFiles.RemoveFile(_LABDE_TEST_FILE1);
                _testFiles.RemoveFile(_LABDE_TEST_FILE2);
                _testFiles.RemoveFile(_LABDE_TEST_FILE3);
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        // <summary>
        /// Tests the ModifyActionStatusForSelection method in a database with workflows when
        /// All Workflows is selected but the target action does not exist for all workflows.
        /// https://extract.atlassian.net/browse/ISSUE-17380
        /// </summary>
        [Test, Category("Automated")]
        public static void ModifyActionStatusAllWorkflowsWithMissingWorkflowAction()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestFAMFileProcessing>();
            _testDbManager = new FAMTestDBManager<TestFAMFileProcessing>();

            string testDbName = "Test_ModifyActionStatusAllWorkflowsWithMissingWorkflowAction";

            try
            {
                string testFileName1 = _testFiles.GetFile(_LABDE_TEST_FILE1);
                string testFileName2 = _testFiles.GetFile(_LABDE_TEST_FILE2);
                string testFileName3 = _testFiles.GetFile(_LABDE_TEST_FILE3);
                FileProcessingDB fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                // Initial statuses by File ID
                //            |  P  |  R  |  S  |  C  |  F 
                // Workflow 1 ----------------------------
                //   Action 1   1,2                 
                //   Action 2   
                // Workflow 2 ----------------------------
                //   Action 1   3
                //   Action 2   
                //   Action 3 

                int workflowID1 = fileProcessingDb.AddWorkflow(
                    "Workflow1", EWorkflowType.kUndefined, _LABDE_ACTION1, _LABDE_ACTION2);

                fileProcessingDb.AddFile(testFileName1, _LABDE_ACTION1, workflowID1, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionPending, false, out bool alreadyExists, out EActionStatus previousStatus);
                fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION1, workflowID1, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);

                int workflowID2 = fileProcessingDb.AddWorkflow(
                    "Workflow2", EWorkflowType.kUndefined, _LABDE_ACTION1, _LABDE_ACTION2, _LABDE_ACTION3);

                fileProcessingDb.AddFile(testFileName3, _LABDE_ACTION1, workflowID2, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);

                fileProcessingDb.ActiveWorkflow = "Workflow1";
                int workflow1Action2 = fileProcessingDb.GetActionID(_LABDE_ACTION2);

                fileProcessingDb.ActiveWorkflow = "Workflow2";
                int workflow2Action2 = fileProcessingDb.GetActionID(_LABDE_ACTION2);
                int workflow2Action3 = fileProcessingDb.GetActionID(_LABDE_ACTION3);

                fileProcessingDb.ActiveWorkflow = "";

                var fileSelector = new FAMFileSelector();
                fileSelector.AddQueryCondition("SELECT [FAMFile].[ID] FROM [FAMFile]");

                fileProcessingDb.ModifyActionStatusForSelection(fileSelector,
                    _LABDE_ACTION2, EActionStatus.kActionPending, "", vbModifyWhenTargetActionMissingForSomeFiles: false);

                // After setting Action 2 to pending for all workflows
                //            |  P  |  R  |  S  |  C  |  F 
                // Workflow 1 ----------------------------
                //   Action 1   1,2                 
                //   Action 2   1,2
                // Workflow 2 ----------------------------
                //   Action 1   3
                //   Action 2   3
                //   Action 3 

                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION2, false).NumDocumentsPending == 3);
                Assert.That(fileProcessingDb.GetStats(workflow1Action2, false).NumDocuments == 2);
                Assert.That(fileProcessingDb.GetStats(workflow2Action2, false).NumDocuments == 1);

                // Action 3 doesn't exist for workflow 1
                Assert.Throws<COMException>(() => fileProcessingDb.ModifyActionStatusForSelection(fileSelector,
                    _LABDE_ACTION3, EActionStatus.kActionPending, "", vbModifyWhenTargetActionMissingForSomeFiles: false));

                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION3, false).NumDocumentsPending == 0);

                // Retry, but allow Action 3 to be set for Workflow 2
                fileProcessingDb.ModifyActionStatusForSelection(fileSelector,
                    _LABDE_ACTION3, EActionStatus.kActionPending, "", vbModifyWhenTargetActionMissingForSomeFiles: true);


                // After setting Action 3 to pending for all workflows
                //            |  P  |  R  |  S  |  C  |  F 
                // Workflow 1 ----------------------------
                //   Action 1   1,2                 
                //   Action 2   1,2
                // Workflow 2 ----------------------------
                //   Action 1   3
                //   Action 2   3
                //   Action 3   3

                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION3, false).NumDocumentsPending == 1);
                Assert.That(fileProcessingDb.GetStats(workflow2Action3, false).NumDocuments == 1);

                fileSelector.Reset();
                fileSelector.AddQueryCondition("SELECT [FAMFile].[ID] FROM [FAMFile] " +
                    "INNER JOIN [WorkflowFile] ON [FileID] = [FAMFile].[ID] " +
                    "WHERE [WorkflowID] = " + workflowID1.ToString(CultureInfo.InvariantCulture));

                // There are no selected files for which Action 3 exists
                Assert.Throws<COMException>(() => fileProcessingDb.ModifyActionStatusForSelection(fileSelector,
                    _LABDE_ACTION3, EActionStatus.kActionSkipped, "", vbModifyWhenTargetActionMissingForSomeFiles: false));

                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION3, false).NumDocumentsSkipped == 0);

                fileProcessingDb.ModifyActionStatusForSelection(fileSelector,
                    _LABDE_ACTION3, EActionStatus.kActionSkipped, "", vbModifyWhenTargetActionMissingForSomeFiles: true);

                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION3, false).NumDocumentsSkipped == 0);

                fileSelector.Reset();
                fileSelector.AddQueryCondition("SELECT [FAMFile].[ID] FROM [FAMFile] " +
                    "INNER JOIN [WorkflowFile] ON [FileID] = [FAMFile].[ID] " +
                    "WHERE [WorkflowID] = " + workflowID2.ToString(CultureInfo.InvariantCulture));

                // No error even though Action 3 doesn't exist in Workflow 1 because no selected files from Workflow 1
                fileProcessingDb.ModifyActionStatusForSelection(fileSelector,
                    _LABDE_ACTION3, EActionStatus.kActionSkipped, "", vbModifyWhenTargetActionMissingForSomeFiles: false);

                // After setting Action 3 to skipped for all files in Workflow 2
                //            |  P  |  R  |  S  |  C  |  F 
                // Workflow 1 ----------------------------
                //   Action 1   1,2                 
                //   Action 2   1,2
                // Workflow 2 ----------------------------
                //   Action 1   3
                //   Action 2   3
                //   Action 3                3

                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION3, false).NumDocumentsSkipped == 1);
                Assert.That(fileProcessingDb.GetStats(workflow2Action3, false).NumDocumentsSkipped == 1);
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

    #region Helper Classes

    // FileIDs for each status
    class ActionStatus
    {
        public int[] P = Array.Empty<int>();
        public int[] R = Array.Empty<int>();
        public int[] S = Array.Empty<int>();
        public int[] C = Array.Empty<int>();
        public int[] F = Array.Empty<int>();

        public ActionStatus() { }

        ActionStatus(int[][] statuses)
        {
            P = statuses[0];
            R = statuses[1];
            S = statuses[2];
            C = statuses[3];
            F = statuses[4];
        }

        int[][] statuses => new int[][] { P, R, S, C, F };

        // Create a copy that has specific file IDs filtered out
        public ActionStatus CopyWithFileFilter(Func<int, bool> fileFilter)
        {
            return new ActionStatus(statuses.Select(status => status.Where(fileFilter).ToArray()).ToArray());
        }
    }

    // Total counts for each status
    class ActionStatusCounts : IEquatable<ActionStatusCounts>
    {
        public int P;
        public int R;
        public int S;
        public int C;
        public int F;

        public ActionStatusCounts() { }

        ActionStatusCounts(int[] counts)
        {
            P = counts[0];
            R = counts[1];
            S = counts[2];
            C = counts[3];
            F = counts[4];
        }

        int[] counts => new int[] { P, R, S, C, F };

        public bool Equals(ActionStatusCounts other)
        {
            return
                P == other.P &&
                R == other.R &&
                S == other.S &&
                C == other.C &&
                F == other.F;
        }

        public static ActionStatusCounts operator +(ActionStatusCounts a, ActionStatusCounts b)
        {
            return new ActionStatusCounts(a.counts.Zip(b.counts, (a, b) => a + b).ToArray());
        }

        public override string ToString()
        {
            return UtilityMethods.FormatInvariant($"P={P},R={R},S={S},C={C},F={F}");
        }
    }

    static class ActionStatusExtensions
    {
        static ActionStatus[][] FilterFileFromWorkflow(ActionStatus[][] statuses, int workflowToDeleteFrom, bool keepFilesInOtherWorkflows, Func<int, bool> fileFilter)
        {
            return statuses
                .Select((workflow, i) => (i + 1) == workflowToDeleteFrom
                    ? workflow.Select(a => a.CopyWithFileFilter(fileFilter)).ToArray()
                    : keepFilesInOtherWorkflows
                        ? workflow
                        : workflow.Select(a => new ActionStatus()).ToArray())
                .ToArray();
        }

        public static ActionStatus[][] RemoveFileFromWorkflow(this ActionStatus[][] statuses, int workflowToDeleteFrom, int fileToDelete)
        {
            return FilterFileFromWorkflow(statuses, workflowToDeleteFrom, true, fileID => fileID != fileToDelete);
        }

        public static ActionStatus[][] KeepOnlySpecifiedFile(this ActionStatus[][] statuses, int workflowToDeleteFrom, int fileToKeep)
        {
            return FilterFileFromWorkflow(statuses, workflowToDeleteFrom, false, fileID => fileID == fileToKeep);
        }

        public static ActionStatusCounts ComputeCountsFromIDs(this ActionStatus action)
        {
            return new ActionStatusCounts
            {
                P = action.P.Length,
                R = action.R.Length,
                S = action.S.Length,
                C = action.C.Length,
                F = action.F.Length
            };
        }

        public static ActionStatusCounts ComputeCountsFromIDs(this ActionStatus[] workflows)
        {
            return workflows.Select(ComputeCountsFromIDs).Aggregate((acc, x) => acc + x);
        }

        public static ActionStatusCounts[] ComputeCountsFromIDsByAction(this ActionStatus[] actions)
        {
            return actions.Select(ComputeCountsFromIDs).ToArray();
        }

        // Require each array to have the same length (each 'workflow' has every action represented)
        public static ActionStatusCounts[] ComputeCountsFromIDsByAction(this ActionStatus[][] workflows)
        {
            int numWorkflows = workflows.Length;
            int numActions = workflows[0].Length;

            foreach (var workflow in workflows)
            {
                Assert.AreEqual(numActions, workflow.Length);
            }

            // Transpose the matrix to sum by action
            ActionStatus[][] actions = new ActionStatus[numActions][];
            for (int i = 0; i < numActions; i++)
            {
                actions[i] = new ActionStatus[numWorkflows];
                for (int j = 0; j < numWorkflows; j++)
                {
                    actions[i][j] = workflows[j][i];
                }
            }
            return actions.Select(ComputeCountsFromIDs).ToArray();
        }

        public static ActionStatusCounts ComputeCountsFromActionStatistics(this ActionStatistics stats)
        {
            var p = stats.NumDocumentsPending;
            var s = stats.NumDocumentsSkipped;
            var c = stats.NumDocumentsComplete;
            var f = stats.NumDocumentsFailed;
            var r = stats.NumDocuments - p - s - c - f;
            return new ActionStatusCounts { P = p, R = r, S = s, C = c, F = f };
        }

        public static ActionStatusCounts[] ComputeCountsFromActionStatisticsByAction(this IEnumerable<ActionStatistics> actions)
        {
            return actions.Select(ComputeCountsFromActionStatistics).ToArray();
        }
    }

    #endregion
}
