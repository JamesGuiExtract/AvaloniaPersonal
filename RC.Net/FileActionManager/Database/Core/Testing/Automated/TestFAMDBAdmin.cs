﻿using Extract.SqlDatabase;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Database.Test
{
    /// <summary>
    /// Testing class for database admin related methods in IFileProcessingDB.
    /// </summary>
    [Category("TestFAMDBAdmin")]
    [TestFixture]
    public static class TestFAMDBAdmin
    {
        #region Constants

        static readonly string _DB_V194 = "Resources.DBVersion194.bak";
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

                // However, another password should be able to be generated.
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
            string testDbName = "Test_ModifyActionStatusForSelection";

            try
            {
                var fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);
                var actionId1 = fileProcessingDb.GetActionID(_LABDE_ACTION1);
                var actionId2 = fileProcessingDb.GetActionID(_LABDE_ACTION2);
                var actionId3 = fileProcessingDb.GetActionID(_LABDE_ACTION3);

                // Initial statuses by File ID
                //            |  P  |  R  |  S  |  C  |  F 
                //   Action 1                     1,2           
                //   Action 2   1                  2
                //   Action 3   3                        2
                SetupDB3FilesNoWorkflowNoUsers( fileProcessingDb);

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
                int numModified = fileProcessingDb.ModifyActionStatusForSelection(fileSelector,
                    _LABDE_ACTION3, EActionStatus.kActionPending, "", false);
                Assert.AreEqual(1, numModified);

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
                numModified = fileProcessingDb.ModifyActionStatusForSelection(fileSelector,
                    _LABDE_ACTION2, EActionStatus.kActionSkipped, "", false);
                Assert.AreEqual(2, numModified);

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
                CollectionAssert.AreEqual(expectedCountsByAction, actualCountsByAction);

                // Separate the expected data. If not deleting then these will be the same as total and empty, respectively
                ActionStatus[][] expectedVisibleStatus = expectedWorkflowStatuses;
                ActionStatus[][] expectedInvisibleStatus = expectedWorkflowStatuses.KeepOnlySpecifiedFile(0, 0);

                if (deleteFilesFromWorkflow)
                {
                    fileProcessingDb.MarkFileDeleted(fileToDelete, workflowToDeleteFrom);

                    expectedVisibleStatus = expectedWorkflowStatuses.RemoveFileFromWorkflow(workflowToDeleteFrom, fileToDelete);
                    var expectedVisibleCountsByAction = expectedVisibleStatus.ComputeCountsFromIDsByAction();

                    var actualVisibleCountsByAction = actions
                        .Select(action => fileProcessingDb.GetVisibleFileStatsAllWorkflows(action, false))
                        .ComputeCountsFromActionStatisticsByAction();
                    CollectionAssert.AreEqual(expectedVisibleCountsByAction, actualVisibleCountsByAction);

                    expectedInvisibleStatus = expectedWorkflowStatuses.KeepOnlySpecifiedFile(workflowToDeleteFrom, fileToDelete);
                    var expectedInvisibleCountsByAction = expectedInvisibleStatus.ComputeCountsFromIDsByAction();

                    var actualInvisibleCountsByAction = actions.Select(a => fileProcessingDb.GetInvisibleFileStatsAllWorkflows(a, false)).ComputeCountsFromActionStatisticsByAction();
                    CollectionAssert.AreEqual(expectedInvisibleCountsByAction, actualInvisibleCountsByAction);
                }

                // Make workflow 1 active
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                int actionId1 = fileProcessingDb.GetActionID(_LABDE_ACTION1);
                int actionId2 = fileProcessingDb.GetActionID(_LABDE_ACTION2);
                Assert.AreEqual(2, fileProcessingDb.GetFileCount(false));
                Assert.AreEqual(2, fileProcessingDb.GetStats(actionId1, false).NumDocuments);
                Assert.AreEqual(2, fileProcessingDb.GetVisibleFileStats(actionId1, false, false).NumDocuments
                    + fileProcessingDb.GetInvisibleFileStats(actionId1, false).NumDocuments);

                var fileSelector = new FAMFileSelector();
                fileSelector.AddQueryCondition("SELECT [FAMFile].[ID] FROM [FAMFile]");
                int numModified = fileProcessingDb.ModifyActionStatusForSelection(fileSelector,
                    _LABDE_ACTION1, EActionStatus.kActionUnattempted, "", false);
                Assert.AreEqual(2, numModified);

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
                    expectedVisibleStatus = expectedWorkflowStatuses.RemoveFileFromWorkflow(workflowToDeleteFrom, fileToDelete);
                    expectedInvisibleStatus = expectedWorkflowStatuses.KeepOnlySpecifiedFile(workflowToDeleteFrom, fileToDelete);
                }
                else
                {
                    expectedVisibleStatus = expectedWorkflowStatuses;
                    expectedInvisibleStatus = expectedWorkflowStatuses.KeepOnlySpecifiedFile(0, 0);
                }

                Assert.That(fileProcessingDb.GetFileCount(false) == 2);

                // Compute the expected stats for workflow 1
                var expectedVisibleStats = expectedVisibleStatus[0].ComputeCountsFromIDsByAction();
                var expectedInvisibleStats = expectedInvisibleStatus[0].ComputeCountsFromIDsByAction();

                var actionIDs = new[] { actionId1, actionId2, 0 };
                var actualVisibleStats = actionIDs
                    .Select(actionID => actionID == 0 ? new ActionStatisticsClass() : fileProcessingDb.GetVisibleFileStats(actionID, false, false))
                    .ToArray()
                    .ComputeCountsFromActionStatisticsByAction();

                var actualInvisibleStats = actionIDs
                    .Select(actionID => actionID == 0 ? new ActionStatisticsClass() : fileProcessingDb.GetInvisibleFileStats(actionID, false))
                    .ToArray()
                    .ComputeCountsFromActionStatisticsByAction();

                CollectionAssert.AreEqual(expectedVisibleStats, actualVisibleStats);
                CollectionAssert.AreEqual(expectedInvisibleStats, actualInvisibleStats);

                // Compute expected numbers for action 1, all workflows
                var expectedVisibleAction1Stats = expectedVisibleStatus.ComputeCountsFromIDsByAction()[0];
                var expectedInvisibleAction1Stats = expectedInvisibleStatus.ComputeCountsFromIDsByAction()[0];
                var expectedCombinedStats = expectedVisibleAction1Stats + expectedInvisibleAction1Stats;

                // Sanity check, combined numbers match previous expected data for action 1
                Assert.AreEqual(new ActionStatusCounts { C = 2 }, expectedCombinedStats);

                var actualVisibleAction1Stats = fileProcessingDb.GetVisibleFileStatsAllWorkflows(_LABDE_ACTION1, false).ComputeCountsFromActionStatistics();
                Assert.AreEqual(expectedVisibleAction1Stats, actualVisibleAction1Stats);

                var actualInvisibleAction1Stats = fileProcessingDb.GetInvisibleFileStatsAllWorkflows(_LABDE_ACTION1, false).ComputeCountsFromActionStatistics();
                Assert.AreEqual(expectedInvisibleAction1Stats, actualInvisibleAction1Stats);

                // Make workflow 2 active
                fileProcessingDb.ActiveWorkflow = "Workflow2";
                actionId1 = fileProcessingDb.GetActionID(_LABDE_ACTION1);
                actionId2 = fileProcessingDb.GetActionID(_LABDE_ACTION2);
                int actionId3 = fileProcessingDb.GetActionID(_LABDE_ACTION3);

                // Compute the expected stats for workflow 2
                expectedVisibleStats = expectedVisibleStatus[1].ComputeCountsFromIDsByAction();
                expectedInvisibleStats = expectedInvisibleStatus[1].ComputeCountsFromIDsByAction();

                actionIDs = new[] { actionId1, actionId2, actionId3 };
                actualVisibleStats = actionIDs
                    .Select(actionID => fileProcessingDb.GetVisibleFileStats(actionID, false, false))
                    .ToArray()
                    .ComputeCountsFromActionStatisticsByAction();

                actualInvisibleStats = actionIDs
                    .Select(actionID => fileProcessingDb.GetInvisibleFileStats(actionID, false))
                    .ToArray()
                    .ComputeCountsFromActionStatisticsByAction();

                CollectionAssert.AreEqual(expectedVisibleStats, actualVisibleStats);
                CollectionAssert.AreEqual(expectedInvisibleStats, actualInvisibleStats);

                Assert.That(fileProcessingDb.GetFileCount(false) == 3);

                fileSelector.Reset();
                fileSelector.AddQueryCondition("SELECT [FAMFile].[ID] FROM [FAMFile] " +
                    "   INNER JOIN [FileActionStatus] ON [FileID] = [FAMFile].[ID] AND [ActionStatus] = 'F'");
                numModified = fileProcessingDb.ModifyActionStatusForSelection(fileSelector,
                    _LABDE_ACTION3, EActionStatus.kActionPending, "", false);
                Assert.AreEqual(1, numModified);

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
                    expectedVisibleStatus = expectedWorkflowStatuses.RemoveFileFromWorkflow(workflowToDeleteFrom, fileToDelete);
                    expectedInvisibleStatus = expectedWorkflowStatuses.KeepOnlySpecifiedFile(workflowToDeleteFrom, fileToDelete);
                }
                else
                {
                    expectedVisibleStatus = expectedWorkflowStatuses;
                    expectedInvisibleStatus = expectedWorkflowStatuses.KeepOnlySpecifiedFile(0, 0);
                }

                // Compute the expected stats for workflow 2
                expectedVisibleStats = expectedVisibleStatus[1].ComputeCountsFromIDsByAction();
                expectedInvisibleStats = expectedInvisibleStatus[1].ComputeCountsFromIDsByAction();

                actionIDs = new[] { actionId1, actionId2, actionId3 };
                actualVisibleStats = actionIDs
                    .Select(actionID => fileProcessingDb.GetVisibleFileStats(actionID, false, false))
                    .ToArray()
                    .ComputeCountsFromActionStatisticsByAction();

                actualInvisibleStats = actionIDs
                    .Select(actionID => fileProcessingDb.GetInvisibleFileStats(actionID, false))
                    .ToArray()
                    .ComputeCountsFromActionStatisticsByAction();

                CollectionAssert.AreEqual(expectedVisibleStats, actualVisibleStats);
                CollectionAssert.AreEqual(expectedInvisibleStats, actualInvisibleStats);


                // Make all workflows active
                fileProcessingDb.ActiveWorkflow = "";

                fileSelector.Reset();
                fileSelector.AddQueryCondition("SELECT [FAMFile].[ID] FROM [FAMFile] WHERE [ID] = 2 OR [ID] = 3");
                numModified = fileProcessingDb.ModifyActionStatusForSelection(fileSelector,
                    _LABDE_ACTION2, EActionStatus.kActionSkipped, "", false);
                Assert.AreEqual(3, numModified);

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
                    expectedVisibleStatus = expectedWorkflowStatuses.RemoveFileFromWorkflow(workflowToDeleteFrom, fileToDelete);
                    expectedInvisibleStatus = expectedWorkflowStatuses.KeepOnlySpecifiedFile(workflowToDeleteFrom, fileToDelete);
                }
                else
                {
                    expectedVisibleStatus = expectedWorkflowStatuses;
                    expectedInvisibleStatus = expectedWorkflowStatuses.KeepOnlySpecifiedFile(0, 0);
                }

                Assert.AreEqual(3, fileProcessingDb.GetFileCount(false));

                // Compute the expected stats for all workflows
                expectedVisibleStats = expectedVisibleStatus.ComputeCountsFromIDsByAction();
                expectedInvisibleStats = expectedInvisibleStatus.ComputeCountsFromIDsByAction();

                actualVisibleStats = actions.Select(a => fileProcessingDb.GetVisibleFileStatsAllWorkflows(a, false)).ComputeCountsFromActionStatisticsByAction();
                CollectionAssert.AreEqual(expectedVisibleStats, actualVisibleStats);

                actualInvisibleStats = actions.Select(a => fileProcessingDb.GetInvisibleFileStatsAllWorkflows(a, false)).ComputeCountsFromActionStatisticsByAction();
                CollectionAssert.AreEqual(expectedInvisibleStats, actualInvisibleStats);

                // Make workflow 1 active
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                actionId1 = fileProcessingDb.GetActionID(_LABDE_ACTION1);
                actionId2 = fileProcessingDb.GetActionID(_LABDE_ACTION2);

                // Compute the expected stats for workflow 1
                expectedVisibleStats = expectedVisibleStatus[0].ComputeCountsFromIDsByAction();
                expectedInvisibleStats = expectedInvisibleStatus[0].ComputeCountsFromIDsByAction();

                actionIDs = new[] { actionId1, actionId2, 0 };
                actualVisibleStats = actionIDs
                    .Select(actionID => actionID == 0 ? new ActionStatisticsClass() : fileProcessingDb.GetVisibleFileStats(actionID, false, false))
                    .ToArray()
                    .ComputeCountsFromActionStatisticsByAction();

                actualInvisibleStats = actionIDs
                    .Select(actionID => actionID == 0 ? new ActionStatisticsClass() : fileProcessingDb.GetInvisibleFileStats(actionID, false))
                    .ToArray()
                    .ComputeCountsFromActionStatisticsByAction();

                CollectionAssert.AreEqual(expectedVisibleStats, actualVisibleStats);
                CollectionAssert.AreEqual(expectedInvisibleStats, actualInvisibleStats);

                Assert.AreEqual(2, fileProcessingDb.GetFileCount(false));

                // Make workflow 2 active
                fileProcessingDb.ActiveWorkflow = "Workflow2";
                actionId1 = fileProcessingDb.GetActionID(_LABDE_ACTION1);
                actionId2 = fileProcessingDb.GetActionID(_LABDE_ACTION2);

                // Compute the expected stats for workflow 2
                expectedVisibleStats = expectedVisibleStatus[1].ComputeCountsFromIDsByAction();
                expectedInvisibleStats = expectedInvisibleStatus[1].ComputeCountsFromIDsByAction();

                actionIDs = new[] { actionId1, actionId2, actionId3 };
                actualVisibleStats = actionIDs
                    .Select(actionID => fileProcessingDb.GetVisibleFileStats(actionID, false, false))
                    .ToArray()
                    .ComputeCountsFromActionStatisticsByAction();

                actualInvisibleStats = actionIDs
                    .Select(actionID => fileProcessingDb.GetInvisibleFileStats(actionID, false))
                    .ToArray()
                    .ComputeCountsFromActionStatisticsByAction();

                CollectionAssert.AreEqual(expectedVisibleStats, actualVisibleStats);
                CollectionAssert.AreEqual(expectedInvisibleStats, actualInvisibleStats);

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

                int numModified = fileProcessingDb.ModifyActionStatusForSelection(fileSelector,
                    _LABDE_ACTION2, EActionStatus.kActionPending, "", vbModifyWhenTargetActionMissingForSomeFiles: false);
                Assert.AreEqual(3, numModified);

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
                numModified = fileProcessingDb.ModifyActionStatusForSelection(fileSelector,
                    _LABDE_ACTION3, EActionStatus.kActionPending, "", vbModifyWhenTargetActionMissingForSomeFiles: true);
                Assert.AreEqual(1, numModified);


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

                numModified = fileProcessingDb.ModifyActionStatusForSelection(fileSelector,
                    _LABDE_ACTION3, EActionStatus.kActionSkipped, "", vbModifyWhenTargetActionMissingForSomeFiles: true);
                Assert.AreEqual(0, numModified);

                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION3, false).NumDocumentsSkipped == 0);

                fileSelector.Reset();
                fileSelector.AddQueryCondition("SELECT [FAMFile].[ID] FROM [FAMFile] " +
                    "INNER JOIN [WorkflowFile] ON [FileID] = [FAMFile].[ID] " +
                    "WHERE [WorkflowID] = " + workflowID2.ToString(CultureInfo.InvariantCulture));

                // No error even though Action 3 doesn't exist in Workflow 1 because no selected files from Workflow 1
                numModified = fileProcessingDb.ModifyActionStatusForSelection(fileSelector,
                    _LABDE_ACTION3, EActionStatus.kActionSkipped, "", vbModifyWhenTargetActionMissingForSomeFiles: false);
                Assert.AreEqual(1, numModified);

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
            string testDbName = "Test_SetStatusForAllFilesWithWorkflows";

            try
            {
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
                SetupDB3FilesWithWorkflowNoUsers(fileProcessingDb);

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

        /// https://extract.atlassian.net/browse/ISSUE-17904
        [Test, Category("Automated")]
        public static void AutoCorrectionOfDatabaseID()
        {
            string dbName = "Test_AutoCorrectionOfDatabaseID";

            using var dbWrapper = _testDbManager.GetDisposableDatabase(_DB_V194, dbName);

            string encryptedDatabaseID = dbWrapper.FileProcessingDB.GetDBInfoSetting("DatabaseID", true);

            // This will trigger an attempt to automatically repair the database ID which will be considered invalid
            // because it was restored. The role passwords need to stay in sync with the updated database ID.
            Assert.IsFalse(dbWrapper.FileProcessingDB.HasCounterCorruption);

            Assert.AreNotEqual(
                encryptedDatabaseID
                , dbWrapper.FileProcessingDB.GetDBInfoSetting("DatabaseID", true));

            SqlAppRoleConnection extractRoleConnection = null;
            try
            {
                Assert.DoesNotThrow(() => extractRoleConnection = new ExtractRoleConnection("(local)", dbName, false)
                    , "Failed to create ExtractRoleConnection");
                Assert.DoesNotThrow(() => extractRoleConnection.Open(), "Failed to open ExtractRoleConnection");
            }
            finally
            {
                extractRoleConnection?.Dispose();
            }
        }

        [Test, Category("Automated")]
        [TestCase(1, false, Description = "No workflow Action 1 to action 4")]
        [TestCase(2, false, Description = "No workflow Action 2 to action 4")]
        [TestCase(3, false, Description = "No workflow Action 3 to action 4")]
        [TestCase(5, true, Description = "Workflow 1 Action 5 to workflow 1 action 10")]        
        [TestCase(6, true, Description = "Workflow 1 Action 6 to workflow 1 action 10")]
        [TestCase(7, true, Ignore = "(fails)Workflow 2 Action 7 to workflow 1 action 10 https://extract.atlassian.net/browse/ISSUE-18169")]
        [TestCase(8, true, Ignore = "(fails)Workflow 2 Action 8 to workflow 1 action 10 https://extract.atlassian.net/browse/ISSUE-18169")]
        [TestCase(9, true, Ignore = "(fails)Workflow 2 Action 9 to workflow 1 action 10 https://extract.atlassian.net/browse/ISSUE-18169")]
        public static void CopyActionStatusFromAction(int actionToCopy, bool workflow)
        {
            string testDbName = "Test_CopyActionStatusFromAction";

            try
            {
                var famDB = _testDbManager.GetNewDatabase(testDbName);
                famDB.DefineNewAction(_LABDE_ACTION1);
                famDB.DefineNewAction(_LABDE_ACTION2);
                famDB.DefineNewAction(_LABDE_ACTION3); 
                int copyToActionID = famDB.DefineNewAction("CopyToAction");

                if (!workflow)
                {
                    SetupDB3FilesNoWorkflowNoUsers(famDB);
                }
                else
                {
                    SetupDB3FilesWithWorkflowNoUsers(famDB);

                    // add CopyToAction to workflow 1
                    var workflow1Actions = new[] { _LABDE_ACTION1, _LABDE_ACTION2, "CopyToAction" }
                        .Select(name =>
                        {
                            var actionInfo = new VariantVector();
                            actionInfo.PushBack(name);
                            actionInfo.PushBack(true);
                            return actionInfo;
                        })
                        .ToIUnknownVector();
                    famDB.SetWorkflowActions(1, workflow1Actions);

                    famDB.ActiveWorkflow = "Workflow1";
                    copyToActionID = famDB.GetActionID("CopyToAction");
                }

                List<IActionStatistics> expectedStatistics = new();
                for (int actionID = 1; actionID < copyToActionID; actionID++)
                {
                    expectedStatistics.Add(famDB.GetStats(actionID, false));
                }
                expectedStatistics.Add(famDB.GetStats(actionToCopy, false));
                
                famDB.CopyActionStatusFromAction(actionToCopy, copyToActionID);

                // Check that the status updated correctly
                List<IActionStatistics> actualStats = new();
                for (int actionID = 1; actionID <= copyToActionID; actionID++)
                {
                    actualStats.Add(famDB.GetStats(actionID, true));
                }

                Assert.Multiple(() =>
                {
                    for (int i = 0; i < expectedStatistics.Count; i++)
                    {
                        Assert.That(actualStats[i].StatsEqual(expectedStatistics[i]), $"Actual status should equal the expected for action {i + 1}");
                    }

                    // Check that the file statuses are as expected
                    string queryActionCopiedID =
                        @$"SELECT [ActionStatus],[FileID] FROM [FileActionStatus]  
                       WHERE ActionID = {actionToCopy} 
                       Order by ActionStatus asc, FileID asc";

                    var fileStatusForActionCopied = famDB.GetResultsForQuery(queryActionCopiedID)
                        .AsDataTable()
                        .AsEnumerable()
                        .ToList();

                    string queryActionCopyToActionID =
                           @$"SELECT [ActionStatus],[FileID] FROM [FileActionStatus]  
                          WHERE ActionID = {copyToActionID} 
                          Order by ActionStatus asc, FileID asc";
                    var fileStatusForCopyToActionID = famDB.GetResultsForQuery(queryActionCopyToActionID)
                        .AsDataTable()
                        .AsEnumerable()
                        .ToList();

                    Assert.That(fileStatusForCopyToActionID
                        .Select(r => new { status = r.Field<string>("ActionStatus"), fileID = r.Field<int>("FileID") })
                        .SequenceEqual(fileStatusForActionCopied
                        .Select(r => new { status = r.Field<string>("ActionStatus"), fileID = r.Field<int>("FileID") }))
                        , $"File status should be the same as for the action {actionToCopy}");
                });
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

        #region Helper methods
        [CLSCompliant(false)]
        public static bool StatsEqual(this IActionStatistics actual, IActionStatistics expected )
        {
            bool result = actual.NumBytes == expected.NumBytes;
            result &= actual.NumBytesComplete == expected.NumBytesComplete;
            result &= actual.NumBytesFailed == expected.NumBytesFailed;
            result &= actual.NumBytesPending == expected.NumBytesPending;
            result &= actual.NumBytesSkipped == expected.NumBytesSkipped;
            result &= actual.NumDocuments == expected.NumDocuments;
            result &= actual.NumDocumentsComplete == expected.NumDocumentsComplete;
            result &= actual.NumDocumentsFailed == expected.NumDocumentsFailed;
            result &= actual.NumDocumentsPending == expected.NumDocumentsPending;
            result &= actual.NumDocumentsSkipped == expected.NumDocumentsSkipped;
            result &= actual.NumPages == expected.NumPages;
            result &= actual.NumPagesComplete == expected.NumPagesComplete;
            result &= actual.NumPagesFailed == expected.NumPagesFailed;
            result &= actual.NumPagesPending == expected.NumPagesPending;
            result &= actual.NumPagesSkipped == expected.NumPagesSkipped;
            return result;
        }

        // Initial statuses by File ID
        //            |  P  |  R  |  S  |  C  |  F 
        //   Action 1                     1,2           
        //   Action 2   1                  2
        //   Action 3   3                        2
        private static void SetupDB3FilesNoWorkflowNoUsers(FileProcessingDB fileProcessingDb)
        {
            string testFileName1 = _testFiles.GetFile(_LABDE_TEST_FILE1);
            string testFileName2 = _testFiles.GetFile(_LABDE_TEST_FILE2);
            string testFileName3 = _testFiles.GetFile(_LABDE_TEST_FILE3);


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
        }

        // Initial statuses by File ID
        //            |  P  |  R  |  S  |  C  |  F 
        // Workflow 1 ----------------------------
        //   Action 1   1                  2
        //   Action 2   2
        // Workflow 2 ----------------------------
        //   Action 1                     1,2           
        //   Action 2   1                  2
        //   Action 3   3                        2
        private static void SetupDB3FilesWithWorkflowNoUsers(FileProcessingDB fileProcessingDb)
        {
            string testFileName1 = _testFiles.GetFile(_LABDE_TEST_FILE1);
            string testFileName2 = _testFiles.GetFile(_LABDE_TEST_FILE2);
            string testFileName3 = _testFiles.GetFile(_LABDE_TEST_FILE3);

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
        }

        #endregion
    }
}
