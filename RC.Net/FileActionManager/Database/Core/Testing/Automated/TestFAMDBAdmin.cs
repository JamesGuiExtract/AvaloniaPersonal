using Extract.SqlDatabase;
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

using static System.FormattableString;

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

        const string _LABDE_ACTION1 = "A01_ExtractData";
        const string _LABDE_ACTION2 = "A02_Verify";
        const string _LABDE_ACTION3 = "A03_QA";

        const string _WORKFLOW1 = "Workflow1";
        const string _WORKFLOW2 = "Workflow2";

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
        public static void ModifyActionStatusForSelection(
            [Values(-1, 0, 1)] int UserId)
        {
            string testDbName = "Test_ModifyActionStatusForSelection";

            try
            {
                var fileProcessingDb = _testDbManager.GetNewDatabase(testDbName);
                int actionId1 = fileProcessingDb.DefineNewAction(_LABDE_ACTION1);
                int actionId2 = fileProcessingDb.DefineNewAction(_LABDE_ACTION2);
                int actionId3 = fileProcessingDb.DefineNewAction(_LABDE_ACTION3);

                // Add another FAMUser
                string insertUser = "INSERT INTO FAMUser (UserName, FullUserName) VALUES('testUser', 'test user')";
                // since this was a new database and user id for the executing user is 1 this users Id will be 2
                fileProcessingDb.ExecuteCommandQuery(insertUser);

                // Initial statuses by File ID
                //            |  P  |  R  |  S  |  C  |  F 
                //   Action 1                     1,2           
                //   Action 2   1                  2
                //   Action 3   3                        2
                SetupDB3FilesNoWorkflowNoUsers( fileProcessingDb);

                // change the status for one of the files for a test user
                fileProcessingDb.SetStatusForFileForUser(2, _LABDE_ACTION2, -1, "TestUser", EActionStatus.kActionCompleted, false, false, out _);

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
                    _LABDE_ACTION3, EActionStatus.kActionPending, false, UserId);
                Assert.AreEqual(1, numModified);

                // Updated statuses
                //            |  P  |  R  |  S  |  C  |  F 
                //   Action 1                     1,2           
                //   Action 2   1                  2
                //   Action 3  2,3                

                Assert.That(fileProcessingDb.GetStats(actionId3, false).NumDocuments == 2);
                Assert.That(fileProcessingDb.GetStats(actionId3, false).NumDocumentsPending == 2);
                Assert.That(fileProcessingDb.GetStats(actionId3, false).NumDocumentsFailed == 0);

                // Check the files
                var expected = GetOriginalExpectedNoWorkflow();
                if (UserId < 1)
                {
                    expected[_LABDE_ACTION3 + "NO"] = new() { { 2, "P" }, { 3, "P" } };
                }
                else
                {
                    expected[_LABDE_ACTION3 + "NO"] = new() { { 3, "P" } };
                    expected[_LABDE_ACTION3 + "1"] = new() { { 2, "P" } };
                }
                var actual = fileProcessingDb.GetActualNoWorkflow();

                Assert.Multiple(() =>
                {
                    foreach (var item in expected)
                    {
                        CollectionAssert.AreEquivalent(item.Value, actual[item.Key],
                            Invariant($"FileActionStatus records should match expected for {UserId}"));
                    }
                });

                fileSelector.Reset();
                fileSelector.AddQueryCondition(
                    "SELECT [FAMFile].[ID] FROM [FAMFile] WHERE [ID] = 2 OR [ID] = 3");
                numModified = fileProcessingDb.ModifyActionStatusForSelection(fileSelector,
                    _LABDE_ACTION2, EActionStatus.kActionSkipped, false, UserId);
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

                if (UserId == -1)
                {
                    expected[_LABDE_ACTION2 + "NO"] = new() { { 1, "P"}, { 3, "S"} };
                    expected[_LABDE_ACTION2 + "2"] = new() { { 2, "S" } };
                }
                else if (UserId == 0)
                {
                    expected[_LABDE_ACTION2 + "NO"] = new () { { 1, "P"}, { 2, "S"}, { 3, "S"} };
                    expected[_LABDE_ACTION2 + "2"] = new();
                }
                else
                {
                    expected[_LABDE_ACTION2 + "2"] = new();
                    expected[_LABDE_ACTION2 + UserId.ToString(CultureInfo.InvariantCulture)] = new() { { 2, "S" }, { 3, "S" } };
                }
                actual = fileProcessingDb.GetActualNoWorkflow();
                Assert.Multiple(() =>
                {
                    foreach (var item in expected)
                    {
                        CollectionAssert.AreEquivalent(item.Value, actual[item.Key],
                            Invariant($"FileActionStatus records should match expected for {_LABDE_ACTION2} and {UserId}"));
                    }
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
                    _WORKFLOW1, EWorkflowType.kUndefined, _LABDE_ACTION1, _LABDE_ACTION2);
                fileProcessingDb.AddFile(testFileName1, _LABDE_ACTION1, workflowID1, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionPending, false, out _, out _);
                fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION1, workflowID1, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionCompleted, false, out _, out _);
                fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION2, workflowID1, EFilePriority.kPriorityNormal,
                    true, false, EActionStatus.kActionPending, false, out _, out _);

                int workflowID2 = fileProcessingDb.AddWorkflow(
                   _WORKFLOW2, EWorkflowType.kUndefined, _LABDE_ACTION1, _LABDE_ACTION2, _LABDE_ACTION3);

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
                fileProcessingDb.ActiveWorkflow = _WORKFLOW1;
                int actionId1 = fileProcessingDb.GetActionID(_LABDE_ACTION1);
                int actionId2 = fileProcessingDb.GetActionID(_LABDE_ACTION2);
                Assert.AreEqual(2, fileProcessingDb.GetFileCount(false));
                Assert.AreEqual(2, fileProcessingDb.GetStats(actionId1, false).NumDocuments);
                Assert.AreEqual(2, fileProcessingDb.GetVisibleFileStats(actionId1, false, false).NumDocuments
                    + fileProcessingDb.GetInvisibleFileStats(actionId1, false).NumDocuments);

                var fileSelector = new FAMFileSelector();
                fileSelector.AddQueryCondition("SELECT [FAMFile].[ID] FROM [FAMFile]");
                int numModified = fileProcessingDb.ModifyActionStatusForSelection(fileSelector,
                    _LABDE_ACTION1, EActionStatus.kActionUnattempted, false, -1);
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
                fileProcessingDb.ActiveWorkflow = _WORKFLOW2;
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
                    _LABDE_ACTION3, EActionStatus.kActionPending, false, -1);
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
                    _LABDE_ACTION2, EActionStatus.kActionSkipped, false, -1);
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
                fileProcessingDb.ActiveWorkflow = _WORKFLOW1;
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
                fileProcessingDb.ActiveWorkflow = _WORKFLOW2;
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
        public static void ModifyActionStatusAllWorkflowsWithMissingWorkflowAction(
            [Values(-1, 0, 1)] int userID)
        {
            string testDbName = "Test_ModifyActionStatusAllWorkflowsWithMissingWorkflowAction";

            try
            {
                FileProcessingDB fileProcessingDb = _testDbManager.GetNewDatabase(testDbName);
                int actionId1 = fileProcessingDb.DefineNewAction(_LABDE_ACTION1);
                int actionId2 = fileProcessingDb.DefineNewAction(_LABDE_ACTION2);
                int actionId3 = fileProcessingDb.DefineNewAction(_LABDE_ACTION3);

                // Add another FAMUser
                string insertUser = "INSERT INTO FAMUser (UserName, FullUserName) VALUES('testUser', 'test user')";
                // since this was a new database and user id for the executing user is 1 this users Id will be 2
                fileProcessingDb.ExecuteCommandQuery(insertUser);

                // Initial statuses by File ID
                //            |  P  |  R  |  S  |  C  |  F 
                // Workflow 1 ----------------------------
                //   Action 1   1,2                 
                //   Action 2   
                // Workflow 2 ----------------------------
                //   Action 1   3
                //   Action 2   
                //   Action 3
                SetupDB3FilesWithWorkflowForModify(fileProcessingDb);

                fileProcessingDb.ActiveWorkflow = _WORKFLOW1;
                int workflow1Action2 = fileProcessingDb.GetActionID(_LABDE_ACTION2);

                fileProcessingDb.ActiveWorkflow = _WORKFLOW2;
                int workflow2Action2 = fileProcessingDb.GetActionID(_LABDE_ACTION2);
                int workflow2Action3 = fileProcessingDb.GetActionID(_LABDE_ACTION3);

                fileProcessingDb.ActiveWorkflow = "";

                var fileSelector = new FAMFileSelector();
                fileSelector.AddQueryCondition("SELECT [FAMFile].[ID] FROM [FAMFile]");

                int numModified = fileProcessingDb.ModifyActionStatusForSelection(fileSelector,
                    _LABDE_ACTION2, EActionStatus.kActionPending, vbModifyWhenTargetActionMissingForSomeFiles: false, userID);
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

                // Check that the files have been updated properly
                var expected = GetOriginalExpectedWorkflowForModify();
                if (userID < 1)
                {
                    expected[_WORKFLOW1 + _LABDE_ACTION2 + "NO"] = new() { { 1, "P" }, { 2, "P" } };
                }
                else
                {
                    expected[_WORKFLOW1 + _LABDE_ACTION2 + userID.ToString(CultureInfo.InvariantCulture)] = 
                        new() { { 1, "P" }, { 2, "P" } };
                    expected[_WORKFLOW2 + _LABDE_ACTION2 + "NO"] = new();
                    expected[_WORKFLOW2 + _LABDE_ACTION2 + userID.ToString(CultureInfo.InvariantCulture)] = new() { { 3, "P"} };
                }
                var actual = fileProcessingDb.GetActualWorkflowModify();
                Assert.Multiple(() =>
                {
                    foreach (var item in expected)
                    {
                        CollectionAssert.AreEquivalent(item.Value, actual[item.Key],
                            Invariant($"FileActionStatus records should match expected for {_LABDE_ACTION2} and {userID.ToString(CultureInfo.InvariantCulture)}"));
                    }
                });

                // Action 3 doesn't exist for workflow 1
                Assert.Throws<COMException>(() => fileProcessingDb.ModifyActionStatusForSelection(fileSelector,
                    _LABDE_ACTION3, EActionStatus.kActionPending, vbModifyWhenTargetActionMissingForSomeFiles: false, userID));

                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION3, false).NumDocumentsPending == 0);
                // Make sure nothing changed
                actual = fileProcessingDb.GetActualWorkflowModify();
                Assert.Multiple(() =>
                {
                    foreach (var item in expected)
                    {
                        CollectionAssert.AreEquivalent(item.Value, actual[item.Key],
                            Invariant($"FileActionStatus records should match expected for {_LABDE_ACTION2} and {userID.ToString(CultureInfo.InvariantCulture)}"));
                    }
                });

                // Retry, but allow Action 3 to be set for Workflow 2
                numModified = fileProcessingDb.ModifyActionStatusForSelection(fileSelector,
                    _LABDE_ACTION3, EActionStatus.kActionPending, vbModifyWhenTargetActionMissingForSomeFiles: true, userID);
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

                if (userID < 1)
                {
                    expected[_WORKFLOW2 + _LABDE_ACTION3 + "NO"] = new() { { 3, "P" } };
                }
                else
                {
                    expected[_WORKFLOW2 + _LABDE_ACTION3 + userID.ToString(CultureInfo.InvariantCulture)] =
                        new() { { 3, "P" } };
                }

                actual = fileProcessingDb.GetActualWorkflowModify();
                Assert.Multiple(() =>
                {
                    foreach (var item in expected)
                    {
                        CollectionAssert.AreEquivalent(item.Value, actual[item.Key],
                            Invariant($"FileActionStatus records should match expected for {_LABDE_ACTION2} and {userID.ToString(CultureInfo.InvariantCulture)}"));
                    }
                });

                var workflowID1 = fileProcessingDb.GetWorkflowID(_WORKFLOW1);
                var workflowID2 = fileProcessingDb.GetWorkflowID(_WORKFLOW2);
                fileSelector.Reset();
                fileSelector.AddQueryCondition("SELECT [FAMFile].[ID] FROM [FAMFile] " +
                    "INNER JOIN [WorkflowFile] ON [FileID] = [FAMFile].[ID] " +
                    "WHERE [WorkflowID] = " + workflowID1.ToString(CultureInfo.InvariantCulture));

                // There are no selected files for which Action 3 exists
                Assert.Throws<COMException>(() => fileProcessingDb.ModifyActionStatusForSelection(fileSelector,
                    _LABDE_ACTION3, EActionStatus.kActionSkipped, vbModifyWhenTargetActionMissingForSomeFiles: false, userID));

                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION3, false).NumDocumentsSkipped == 0);

                numModified = fileProcessingDb.ModifyActionStatusForSelection(fileSelector,
                    _LABDE_ACTION3, EActionStatus.kActionSkipped, vbModifyWhenTargetActionMissingForSomeFiles: true, userID);
                Assert.AreEqual(0, numModified);

                Assert.That(fileProcessingDb.GetStatsAllWorkflows(_LABDE_ACTION3, false).NumDocumentsSkipped == 0);

                fileSelector.Reset();
                fileSelector.AddQueryCondition("SELECT [FAMFile].[ID] FROM [FAMFile] " +
                    "INNER JOIN [WorkflowFile] ON [FileID] = [FAMFile].[ID] " +
                    "WHERE [WorkflowID] = " + workflowID2.ToString(CultureInfo.InvariantCulture));

                // No error even though Action 3 doesn't exist in Workflow 1 because no selected files from Workflow 1
                numModified = fileProcessingDb.ModifyActionStatusForSelection(fileSelector,
                    _LABDE_ACTION3, EActionStatus.kActionSkipped, vbModifyWhenTargetActionMissingForSomeFiles: false, userID);
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

                if (userID < 1)
                {
                    expected[_WORKFLOW2 + _LABDE_ACTION3 + "NO"] = new() { { 3, "S" } };
                }
                else
                {
                    expected[_WORKFLOW2 + _LABDE_ACTION3 + userID.ToString(CultureInfo.InvariantCulture)] =
                        new() { { 3, "S" } };
                }

                actual = fileProcessingDb.GetActualWorkflowModify();
                Assert.Multiple(() =>
                {
                    foreach (var item in expected)
                    {
                        CollectionAssert.AreEquivalent(item.Value, actual[item.Key],
                            Invariant($"FileActionStatus records should match expected for {_LABDE_ACTION2} and {userID.ToString(CultureInfo.InvariantCulture)}"));
                    }
                });

                // Test the case for the Workflow 1 action1 file 1 user 2
                fileSelector.Reset();
                fileSelector.AddQueryCondition("SELECT [FAMFile].[ID] FROM [FAMFile]");
                fileProcessingDb.ActiveWorkflow = _WORKFLOW1;
                numModified = fileProcessingDb.ModifyActionStatusForSelection(fileSelector,
                    _LABDE_ACTION1, EActionStatus.kActionCompleted, vbModifyWhenTargetActionMissingForSomeFiles: false, userID);
                Assert.AreEqual(2, numModified);

                if (userID == -1)
                {
                    expected[_WORKFLOW1 + _LABDE_ACTION1 + "NO"] = new() { { 2, "C" } };
                    expected[_WORKFLOW1 + _LABDE_ACTION1 + "2"] = new() { { 1, "C" } };
                }
                else if (userID == 0)
                {
                    expected[_WORKFLOW1 + _LABDE_ACTION1 + "NO"] = new() { { 1, "C" }, { 2, "C" } };
                    expected[_WORKFLOW1 + _LABDE_ACTION1 + "2"] = new();
                }
                else
                {
                    expected[_WORKFLOW1 + _LABDE_ACTION1 + "NO"] = new();
                    expected[_WORKFLOW1 + _LABDE_ACTION1 + "1"] = new() { { 1, "C" }, { 2, "C" } };
                    expected[_WORKFLOW1 + _LABDE_ACTION1 + "2"] = new();
                }

                actual = fileProcessingDb.GetActualWorkflowModify();
                Assert.Multiple(() =>
                {
                    foreach (var item in expected)
                    {
                        CollectionAssert.AreEquivalent(item.Value, actual[item.Key],
                            Invariant($"FileActionStatus records should match expected for {_LABDE_ACTION1} and {userID.ToString(CultureInfo.InvariantCulture)}"));
                    }
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

        /// <summary>
        /// Tests the SetStatusForAllFiles method in a database without workflows
        /// </summary>
        [Test, Category("Automated")]
        [CLSCompliant(false)]
        [Pairwise]
        public static void SetStatusForAllFiles(
            [Values(1, 2, 3)] int actionID,
            [Values(EActionStatus.kActionPending, 
                EActionStatus.kActionCompleted, 
                EActionStatus.kActionSkipped, 
                EActionStatus.kActionFailed)] EActionStatus newStatus,
            [Values(-1, 0, 1)]int userID)
        {
            string testDbName = "Test_SetStatusForAllFiles";

            try
            {
                FileProcessingDB fileProcessingDb = _testDbManager.GetNewDatabase(testDbName);
                int action1 = fileProcessingDb.DefineNewAction(_LABDE_ACTION1);
                int action2 = fileProcessingDb.DefineNewAction(_LABDE_ACTION2);
                int action3 = fileProcessingDb.DefineNewAction(_LABDE_ACTION3);

                SetupDB3FilesNoWorkflowNoUsers(fileProcessingDb);

                // Add another FAMUser
                string insertUser = "INSERT INTO FAMUser (UserName, FullUserName) VALUES('testUser', 'test user')";
                // since this was a new database and user id for the executing user is 1 this users Id will be 2
                int insertedUserID = 2;
                fileProcessingDb.ExecuteCommandQuery(insertUser);

                // change the status for one of the files for a test user
                fileProcessingDb.SetStatusForFileForUser(1, _LABDE_ACTION1, -1, "TestUser", EActionStatus.kActionCompleted, false, false, out _);

                Dictionary<string, IActionStatistics> expectedStatistics = new()
                {
                    { _LABDE_ACTION1, new ActionStatistics() { NumDocuments = 2, NumDocumentsComplete = 2 } },
                    { _LABDE_ACTION2, new ActionStatistics() { NumDocuments = 2, NumDocumentsPending = 1, NumDocumentsComplete = 1 } },
                    { _LABDE_ACTION3, new ActionStatistics() { NumDocuments = 2, NumDocumentsPending = 1, NumDocumentsFailed = 1 } }
                };

                Dictionary<int, Dictionary<int, string>> expectedActionFiles = new();

                // Initial status of all the files (no user)
                expectedActionFiles[action1] = new() { { 2, "C" } };
                expectedActionFiles[action2] = new() { { 1, "P" }, { 2, "C" } };
                expectedActionFiles[action3] = new() { { 3, "P" }, { 2, "F" } };

                string newStatusString = newStatus.AsStatusString();

                string actionName = fileProcessingDb.GetActionName(actionID);

                var actions = fileProcessingDb.GetActions().ComToDictionary();

                Dictionary<string, IActionStatistics> actualStatistics = new();

                Assert.Multiple(() =>
                {
                    fileProcessingDb.compareStatsFromDB(expectedStatistics, actions, true);

                    if (newStatus == EActionStatus.kActionFailed)
                    {
                        Assert.Throws<COMException>(() => { fileProcessingDb.SetStatusForAllFiles(actionName, newStatus, userID); });
                    }
                    else
                    {
                        fileProcessingDb.SetStatusForAllFiles(actionName, newStatus, userID);

                        expectedStatistics[actionName] = ExpectedStatisticsForSetStatusForAll(newStatus);

                        fileProcessingDb.compareStatsFromDB(expectedStatistics, actions, true);

                        foreach (var action in actions)
                        {
                            // Cases that need to be tested
                            // actionID == 1, if userID == -1 there will be 3 files changed but one will be userID = 2 and the others wil be no user
                            //                if userID == 0 there will be 3 files changed (the one assigned to the added user testUser will not change
                            //                if userID > 0 3 files will change to the specific user
                            int currActionID = int.Parse(action.Value, CultureInfo.CurrentCulture);
                            var actualFilesForAction = fileProcessingDb.GetFilesWithStatusForAction(currActionID, userID);
                            var actualFilesForActionNoUser = fileProcessingDb.GetFilesWithStatusForAction(currActionID, 0);
                            var actualFilesForAction2 = fileProcessingDb.GetFilesWithStatusForAction(currActionID, insertedUserID);
                            Dictionary<int, string> expectedActionUser2;
                            Dictionary<int, string> expectedNoUser;
                            Dictionary<int, string> expected;

                            Dictionary<int, string> AllSet = new() { { 1, newStatusString }, { 2, newStatusString }, { 3, newStatusString } };

                            if (actionID == action1)
                            {
                                if (currActionID == action1)
                                {
                                    expected = (userID == -1) ?
                                        new() { { 2, newStatusString }, { 3, newStatusString } } : AllSet;
                                    expectedNoUser = userID > 0 ? new() : expected; 
                                    expectedActionUser2 = (userID == -1) ? new() { { 1, newStatusString } } : new();
                                }
                                else
                                {
                                    expected = userID > 0 ? new() : expectedActionFiles[currActionID];
                                    expectedNoUser = expectedActionFiles[currActionID];
                                    expectedActionUser2 = new();
                                }
                            }
                            else
                            {
                                if (currActionID == actionID)
                                {
                                    expected = AllSet;
                                    expectedNoUser = userID < 1 ? AllSet : new();
                                    expectedActionUser2 = new();
                                }
                                else
                                {
                                    expected = userID > 0 ? new() : expectedActionFiles[currActionID];
                                    expectedNoUser = expectedActionFiles[currActionID];
                                    expectedActionUser2 = currActionID == action1 ? new() { { 1, "C" } } : new();
                                }
                            }

                            CollectionAssert.AreEquivalent(expected, actualFilesForAction,
                                Invariant($"FileActionStatus records should match expected for {action.Key} and {userID}"));

                            CollectionAssert.AreEquivalent(expectedNoUser, actualFilesForActionNoUser,
                                Invariant($"FileActionStatus records should match expected for {action.Key} and No user"));

                            CollectionAssert.AreEquivalent(expectedActionUser2, actualFilesForAction2,
                                Invariant($"FileActionStatus records should match expected for TestUser"));

                        }
                    }
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

        [Test, Category("Automated")]
        [CLSCompliant(false)]
        [Pairwise]
        public static void SetStatusForAllFilesWithWorkflowsWithUsers(
            [Values(_LABDE_ACTION1, _LABDE_ACTION2, _LABDE_ACTION3)] string actionName,
            [Values(EActionStatus.kActionPending,
                EActionStatus.kActionCompleted,
                EActionStatus.kActionSkipped,
                EActionStatus.kActionFailed,
                EActionStatus.kActionUnattempted)] EActionStatus newStatus,
            [Values(-1, 0, 1)] int userID,
            [Values(_WORKFLOW1, _WORKFLOW2)] string workflow)
        {
            string testDbName = "Test_SetStatusForAllFilesWithWorkflowsAndUsers";

            try
            {
                FileProcessingDB fileProcessingDb = _testDbManager.GetNewDatabase(testDbName);
                fileProcessingDb.DefineNewAction(_LABDE_ACTION1);
                fileProcessingDb.DefineNewAction(_LABDE_ACTION2);
                fileProcessingDb.DefineNewAction(_LABDE_ACTION3);

                SetupDB3FilesWithWorkflowNoUsers(fileProcessingDb);

                // Add another FAMUser
                string insertUser = "INSERT INTO FAMUser (UserName, FullUserName) VALUES('testUser', 'test user')";
                
                fileProcessingDb.ExecuteCommandQuery(insertUser);

                fileProcessingDb.ActiveWorkflow = _WORKFLOW2;

                // Change the status of one of the files in the 2nd workflow to testUser
                EActionStatus prevStatus;
                fileProcessingDb.SetStatusForFileForUser(1, _LABDE_ACTION1, 2, "testUser",
                    EActionStatus.kActionCompleted, false, false, out prevStatus);

                fileProcessingDb.ActiveWorkflow = workflow;

                if (workflow == _WORKFLOW1 && actionName == _LABDE_ACTION3 || newStatus == EActionStatus.kActionFailed)
                {
                    Assert.Throws<COMException>(() => fileProcessingDb.SetStatusForAllFiles(actionName, newStatus, userID),
                        "Should throw COMException");
                }
                else
                {
                    fileProcessingDb.SetStatusForAllFiles(actionName, newStatus, userID);
                    fileProcessingDb.verifyStatisticssForSetStatusAllFilesWorkflowUser(workflow, actionName, newStatus);
                    fileProcessingDb.verifyFASForSetStatusAllFilesWorkflowUser(workflow, actionName, newStatus, userID);
                }
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

 

        #endregion Test Methods

        #region Helper methods
        [CLSCompliant(false)]
        public static bool StatsEqual(this IActionStatistics actual, IActionStatistics expected, bool compareDocCountsOnly )
        {
            bool result = true;
            result &= actual.NumDocuments == expected.NumDocuments;
            result &= actual.NumDocumentsComplete == expected.NumDocumentsComplete;
            result &= actual.NumDocumentsFailed == expected.NumDocumentsFailed;
            result &= actual.NumDocumentsPending == expected.NumDocumentsPending;
            result &= actual.NumDocumentsSkipped == expected.NumDocumentsSkipped;

            if (!compareDocCountsOnly)
            {
                result &= actual.NumBytes == expected.NumBytes;
                result &= actual.NumBytesComplete == expected.NumBytesComplete;
                result &= actual.NumBytesFailed == expected.NumBytesFailed;
                result &= actual.NumBytesPending == expected.NumBytesPending;
                result &= actual.NumBytesSkipped == expected.NumBytesSkipped;

                result &= actual.NumPages == expected.NumPages;
                result &= actual.NumPagesComplete == expected.NumPagesComplete;
                result &= actual.NumPagesFailed == expected.NumPagesFailed;
                result &= actual.NumPagesPending == expected.NumPagesPending;
                result &= actual.NumPagesSkipped == expected.NumPagesSkipped;
            }
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
                _WORKFLOW1, EWorkflowType.kUndefined, _LABDE_ACTION1, _LABDE_ACTION2);

            fileProcessingDb.AddFile(testFileName1, _LABDE_ACTION1, workflowID1, EFilePriority.kPriorityNormal,
                true, false, EActionStatus.kActionPending, false, out bool alreadyExists, out EActionStatus previousStatus);
            fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION1, workflowID1, EFilePriority.kPriorityNormal,
                true, false, EActionStatus.kActionCompleted, false, out alreadyExists, out previousStatus);
            fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION2, workflowID1, EFilePriority.kPriorityNormal,
                true, false, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);

            int workflowID2 = fileProcessingDb.AddWorkflow(
                _WORKFLOW2, EWorkflowType.kUndefined, _LABDE_ACTION1, _LABDE_ACTION2, _LABDE_ACTION3);

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

        // Initial statuses by File ID
        //            |  P  |  R  |  S  |  C  |  F 
        // Workflow 1 ----------------------------
        //   Action 1   1,2                 
        //   Action 2   
        // Workflow 2 ----------------------------
        //   Action 1   3
        //   Action 2   
        //   Action 3 
        // Workflow1-Action1-File1 is for User TestUser
        private static void SetupDB3FilesWithWorkflowForModify(FileProcessingDB fileProcessingDb)
        {
            string testFileName1 = _testFiles.GetFile(_LABDE_TEST_FILE1);
            string testFileName2 = _testFiles.GetFile(_LABDE_TEST_FILE2);
            string testFileName3 = _testFiles.GetFile(_LABDE_TEST_FILE3);

            int workflowID1 = fileProcessingDb.AddWorkflow(
                _WORKFLOW1, EWorkflowType.kUndefined, _LABDE_ACTION1, _LABDE_ACTION2);

            fileProcessingDb.AddFile(testFileName1, _LABDE_ACTION1, workflowID1, EFilePriority.kPriorityNormal,
                true, false, EActionStatus.kActionPending, false, out bool alreadyExists, out EActionStatus previousStatus);
            fileProcessingDb.SetStatusForFileForUser(1, _LABDE_ACTION1, 1, "TestUser", EActionStatus.kActionPending, false, false, out _);

            fileProcessingDb.AddFile(testFileName2, _LABDE_ACTION1, workflowID1, EFilePriority.kPriorityNormal,
                true, false, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);

            int workflowID2 = fileProcessingDb.AddWorkflow(
                _WORKFLOW2, EWorkflowType.kUndefined, _LABDE_ACTION1, _LABDE_ACTION2, _LABDE_ACTION3);

            fileProcessingDb.AddFile(testFileName3, _LABDE_ACTION1, workflowID2, EFilePriority.kPriorityNormal,
                true, false, EActionStatus.kActionPending, false, out alreadyExists, out previousStatus);
        }
        private static Dictionary<string, Dictionary<int, string>> GetOriginalExpectedWorkflowForModify()
        {
            Dictionary<string, Dictionary<int, string>> result = new()
            {
                { _WORKFLOW1 + _LABDE_ACTION1 + "NO", new() { { 2, "P" } } },
                { _WORKFLOW1 + _LABDE_ACTION1 + "1", new() },
                { _WORKFLOW1 + _LABDE_ACTION1 + "2", new() { { 1, "P" } } },
                { _WORKFLOW1 + _LABDE_ACTION2 + "NO", new() },
                { _WORKFLOW1 + _LABDE_ACTION2 + "1", new() },
                { _WORKFLOW1 + _LABDE_ACTION2 + "2", new() },
                { _WORKFLOW2 + _LABDE_ACTION1 + "NO", new() { { 3, "P" } } },
                { _WORKFLOW2 + _LABDE_ACTION1 + "1", new() },
                { _WORKFLOW2 + _LABDE_ACTION1 + "2", new() },
                { _WORKFLOW2 + _LABDE_ACTION2 + "NO", new() { { 3, "P" } } },
                { _WORKFLOW2 + _LABDE_ACTION2 + "1", new() },
                { _WORKFLOW2 + _LABDE_ACTION2 + "2", new() },
                { _WORKFLOW2 + _LABDE_ACTION3 + "NO", new() },
                { _WORKFLOW2 + _LABDE_ACTION3 + "1", new() },
                { _WORKFLOW2 + _LABDE_ACTION3 + "2", new() }
            };
            return result;
        }

        private static Dictionary<string, Dictionary<int, string>> GetActualWorkflowModify(this FileProcessingDB fileProcessingDb)
        {
            string saveCurrentWorkflow = fileProcessingDb.ActiveWorkflow;

            Dictionary<string, Dictionary<int, string>> actual = new();
            fileProcessingDb.ActiveWorkflow = _WORKFLOW1;
            int actionID = fileProcessingDb.GetActionID(_LABDE_ACTION1);
            actual[_WORKFLOW1 + _LABDE_ACTION1 + "NO"] = fileProcessingDb.GetFilesWithStatusForAction(actionID, -1);
            actual[_WORKFLOW1 + _LABDE_ACTION1 + "1"] = fileProcessingDb.GetFilesWithStatusForAction(actionID, 1);
            actual[_WORKFLOW1 + _LABDE_ACTION1 + "2"] = fileProcessingDb.GetFilesWithStatusForAction(actionID, 2);

            actionID = fileProcessingDb.GetActionID(_LABDE_ACTION2);
            actual[_WORKFLOW1 + _LABDE_ACTION2 + "NO"] = fileProcessingDb.GetFilesWithStatusForAction(actionID, -1);
            actual[_WORKFLOW1 + _LABDE_ACTION2 + "1"] = fileProcessingDb.GetFilesWithStatusForAction(actionID, 1);
            actual[_WORKFLOW1 + _LABDE_ACTION2 + "2"] = fileProcessingDb.GetFilesWithStatusForAction(actionID, 2);

            fileProcessingDb.ActiveWorkflow = _WORKFLOW2;
            actionID = fileProcessingDb.GetActionID(_LABDE_ACTION1);
            actual[_WORKFLOW2 + _LABDE_ACTION1 + "NO"] = fileProcessingDb.GetFilesWithStatusForAction(actionID, -1);
            actual[_WORKFLOW2 + _LABDE_ACTION1 + "1"] = fileProcessingDb.GetFilesWithStatusForAction(actionID, 1);
            actual[_WORKFLOW2 + _LABDE_ACTION1 + "2"] = fileProcessingDb.GetFilesWithStatusForAction(actionID, 2);
            
            actionID = fileProcessingDb.GetActionID(_LABDE_ACTION2);
            actual[_WORKFLOW2 + _LABDE_ACTION2 + "NO"] = fileProcessingDb.GetFilesWithStatusForAction(actionID, -1);
            actual[_WORKFLOW2 + _LABDE_ACTION2 + "1"] = fileProcessingDb.GetFilesWithStatusForAction(actionID, 1);
            actual[_WORKFLOW2 + _LABDE_ACTION2 + "2"] = fileProcessingDb.GetFilesWithStatusForAction(actionID, 2);

            actionID = fileProcessingDb.GetActionID(_LABDE_ACTION3);
            actual[_WORKFLOW2 + _LABDE_ACTION3 + "NO"] = fileProcessingDb.GetFilesWithStatusForAction(actionID, -1);
            actual[_WORKFLOW2 + _LABDE_ACTION3 + "1"] = fileProcessingDb.GetFilesWithStatusForAction(actionID, 1);
            actual[_WORKFLOW2 + _LABDE_ACTION3 + "2"] = fileProcessingDb.GetFilesWithStatusForAction(actionID, 2);

            fileProcessingDb.ActiveWorkflow = saveCurrentWorkflow;

            return actual;
        }

        private static void compareStatsFromDB(this FileProcessingDB fileProcessingDb, 
            Dictionary<string, IActionStatistics> expected, Dictionary<string, string> actions, bool compareDocCountsOnly)
        {
            Dictionary<string, IActionStatistics> actualStatistics = new();
            foreach (var action in actions)
            {
                actualStatistics.Add(action.Key, fileProcessingDb.GetStats(Int32.Parse(action.Value, CultureInfo.CurrentCulture), false));
            }
            
            foreach (var action in actualStatistics)
            {
                Assert.That(action.Value.StatsEqual(expected[action.Key], compareDocCountsOnly), Invariant($"Actual status should equal the expected for action {action.Key}"));
            }
        }

        private static Dictionary<int, string> GetFilesWithStatusForAction(this FileProcessingDB fileProcessingDB, int actionID, int userID)
        {
            string sql;
            if (userID > 0)
            {
                sql = Invariant($"SELECT FileID, ActionStatus FROM FileActionStatus WHERE ActionID = {actionID} AND UserID = {userID}");
            }
            else
            {
                sql = Invariant($"SELECT FileID, ActionStatus FROM FileActionStatus WHERE ActionID = {actionID} AND UserID is NULL ");
            }

            return fileProcessingDB.GetResultsForQuery(sql)
            .AsDataTable()
            .AsEnumerable()
            .ToDictionary(row => row.Field<int>("FileID"), row => row.Field<string>("ActionStatus"));
        }

        private static Dictionary<string, Dictionary<int, string>> GetOriginalExpectedNoWorkflow()
        {
            Dictionary<string, Dictionary<int, string>> original = new()
            {
                { _LABDE_ACTION1 + "NO", new() { { 1, "C" }, { 2, "C" } } },
                { _LABDE_ACTION1 + "1", new() },
                { _LABDE_ACTION1 + "2", new() },
                { _LABDE_ACTION2 + "NO", new() { { 1, "P" } } },
                { _LABDE_ACTION2 + "1", new() },
                { _LABDE_ACTION2 + "2", new() { { 2, "C" } } },
                { _LABDE_ACTION3 + "NO", new() { { 2, "F" }, { 3, "P" } } },
                { _LABDE_ACTION3 + "1", new() },
                { _LABDE_ACTION3 + "2", new() }
            };
            return original;
        }

        private static Dictionary<string, Dictionary<int, string>> GetActualNoWorkflow(this FileProcessingDB fileProcessingDb)
        {
            Dictionary<string, Dictionary<int, string>> actual = new();
            int actionID = fileProcessingDb.GetActionID(_LABDE_ACTION1);
            actual[_LABDE_ACTION1 + "NO"] = fileProcessingDb.GetFilesWithStatusForAction(actionID, -1);
            actual[_LABDE_ACTION1 + "1"] = fileProcessingDb.GetFilesWithStatusForAction(actionID, 1);
            actual[_LABDE_ACTION1 + "2"] = fileProcessingDb.GetFilesWithStatusForAction(actionID, 2);

            actionID = fileProcessingDb.GetActionID(_LABDE_ACTION2);
            actual[_LABDE_ACTION2 + "NO"] = fileProcessingDb.GetFilesWithStatusForAction(actionID, -1);
            actual[_LABDE_ACTION2 + "1"] = fileProcessingDb.GetFilesWithStatusForAction(actionID, 1);
            actual[_LABDE_ACTION2 + "2"] = fileProcessingDb.GetFilesWithStatusForAction(actionID, 2);

            actionID = fileProcessingDb.GetActionID(_LABDE_ACTION3);
            actual[_LABDE_ACTION3 + "NO"] = fileProcessingDb.GetFilesWithStatusForAction(actionID, -1);
            actual[_LABDE_ACTION3 + "1"] = fileProcessingDb.GetFilesWithStatusForAction(actionID, 1);
            actual[_LABDE_ACTION3 + "2"] = fileProcessingDb.GetFilesWithStatusForAction(actionID, 2);

            return actual;
        }

        private static void verifyStatisticssForSetStatusAllFilesWorkflowUser(
            this FileProcessingDB fileProcessingDB, string workflow, string actionName, EActionStatus newStatus)
        {
            Dictionary<string, ActionStatistics> expectedStatistics = new()
            {
                { _WORKFLOW1 + _LABDE_ACTION1, new() { NumDocuments = 2, NumDocumentsPending = 1, NumDocumentsComplete = 1 } },
                { _WORKFLOW1 + _LABDE_ACTION2, new() { NumDocuments = 1, NumDocumentsPending = 1 } },
                { _WORKFLOW2 + _LABDE_ACTION1, new() { NumDocuments = 2, NumDocumentsComplete = 2 } },
                { _WORKFLOW2 + _LABDE_ACTION2, new() { NumDocuments = 2, NumDocumentsPending = 1, NumDocumentsComplete = 1 } },
                { _WORKFLOW2 + _LABDE_ACTION3, new() { NumDocuments = 2, NumDocumentsPending = 1, NumDocumentsFailed = 1 } }
            };

            int newStatValue = (workflow == _WORKFLOW1) ? 2 : 3;
            expectedStatistics[workflow + actionName] = newStatus switch
            {
                EActionStatus.kActionPending => new() { NumDocuments = newStatValue, NumDocumentsPending = newStatValue },
                EActionStatus.kActionSkipped => new() { NumDocuments = newStatValue, NumDocumentsSkipped = newStatValue },
                EActionStatus.kActionCompleted => new() { NumDocuments = newStatValue, NumDocumentsComplete = newStatValue },
                EActionStatus.kActionFailed => expectedStatistics[workflow + actionName], // no change
                EActionStatus.kActionUnattempted => new(),
                _ => throw new ArgumentOutOfRangeException(nameof(newStatus), Invariant($"Not an expected value for {newStatus.AsStatusString()}"))
            };

            Dictionary<string, ActionStatistics> actualStatistics = new();
            fileProcessingDB.ActiveWorkflow = _WORKFLOW1;

            int actionID = fileProcessingDB.GetActionID(_LABDE_ACTION1);
            actualStatistics[_WORKFLOW1 + _LABDE_ACTION1] = fileProcessingDB.GetStats(actionID, false);

            actionID = fileProcessingDB.GetActionID(_LABDE_ACTION2);
            actualStatistics[_WORKFLOW1 + _LABDE_ACTION2] = fileProcessingDB.GetStats(actionID, false);

            fileProcessingDB.ActiveWorkflow = _WORKFLOW2;

            actionID = fileProcessingDB.GetActionID(_LABDE_ACTION1);
            actualStatistics[_WORKFLOW2 + _LABDE_ACTION1] = fileProcessingDB.GetStats(actionID, false);

            actionID = fileProcessingDB.GetActionID(_LABDE_ACTION2);
            actualStatistics[_WORKFLOW2 + _LABDE_ACTION2] = fileProcessingDB.GetStats(actionID, false);

            actionID = fileProcessingDB.GetActionID(_LABDE_ACTION3);
            actualStatistics[_WORKFLOW2 + _LABDE_ACTION3] = fileProcessingDB.GetStats(actionID, false);

            Assert.Multiple(() =>
            {
                foreach (var action in actualStatistics)
                {
                    Assert.That(action.Value.StatsEqual(expectedStatistics[action.Key], true),
                        Invariant($"Actual status should equal the expected for action {action.Key} for {workflow} and {actionName}"));
                }

            });
        }

        private static void verifyFASForSetStatusAllFilesWorkflowUser(
            this FileProcessingDB fileProcessingDB, string workflow, string actionName, EActionStatus newStatus, int userID)
        {
            string newStatusString = newStatus.AsStatusString();
            string CurrKey = workflow + actionName + userID.ToString(CultureInfo.InvariantCulture);
            Dictionary<string, Dictionary<int, string>> expected = new()
            {
                { _WORKFLOW1 + _LABDE_ACTION1 + "NO", new() { { 1, "P" }, { 2, "C" } } },
                { _WORKFLOW1 + _LABDE_ACTION1 + "1", new() },
                { _WORKFLOW1 + _LABDE_ACTION2 + "NO", new() { { 2, "P" } } },
                { _WORKFLOW1 + _LABDE_ACTION2 + "1", new() },

                { _WORKFLOW2 + _LABDE_ACTION1 + "NO", new() { { 2, "C" } } },
                { _WORKFLOW2 + _LABDE_ACTION1 + "1", new() },
                { _WORKFLOW2 + _LABDE_ACTION1 + "2", new() { { 1, "C" } } },
                { _WORKFLOW2 + _LABDE_ACTION2 + "NO", new() { { 1, "P" }, { 2, "C" } } },
                { _WORKFLOW2 + _LABDE_ACTION2 + "1", new() },
                { _WORKFLOW2 + _LABDE_ACTION2 + "2", new() },
                { _WORKFLOW2 + _LABDE_ACTION3 + "NO", new() { { 2, "F" }, { 3, "P" } } },
                { _WORKFLOW2 + _LABDE_ACTION3 + "1", new() },
                { _WORKFLOW2 + _LABDE_ACTION3 + "2", new() }
            };

            if (newStatus == EActionStatus.kActionUnattempted)
            {
                expected[workflow + actionName + "NO"] = new();
                expected[workflow + actionName + "1"] = new();
                if (workflow == _WORKFLOW2)
                {
                    expected[workflow + actionName + "2"] = new();
                }
            }
            else if (userID == -1 && workflow == _WORKFLOW2 && actionName == _LABDE_ACTION1)
            {
                expected[workflow + actionName + "NO"] = new() { { 2, newStatusString }, { 3, newStatusString } };
                expected[workflow + actionName + "2"] = new() { { 1, newStatusString } };
            }
            else if (userID == 0 && workflow == _WORKFLOW2 && actionName == _LABDE_ACTION1)
            {
                expected[workflow + actionName + "NO"] = new() { { 1, newStatusString }, { 2, newStatusString }, { 3, newStatusString } };
                expected[workflow + actionName + "2"] = new();
            }
            else if (workflow == _WORKFLOW2)
            {
                if (userID > 0)
                {
                    expected[workflow + actionName + "NO"] = new();
                    expected[workflow + actionName + "2"] = new();
                    expected[workflow + actionName + userID.ToString(CultureInfo.InvariantCulture)] = new() { { 1, newStatusString }, { 2, newStatusString }, { 3, newStatusString } };
                }
                else
                {
                    expected[workflow + actionName + "NO"] = new() { { 1, newStatusString }, { 2, newStatusString }, { 3, newStatusString } };
                }
            }
            else if (userID > 0)
            {
                expected[workflow + actionName + "NO"] = new();
                expected[workflow + actionName + userID.ToString(CultureInfo.InvariantCulture)] = new()
                {
                    { 1, newStatusString },
                    { 2, newStatusString }
                };
            }
            else
            {
                expected[workflow + actionName + "NO"] = new()
                {
                    { 1, newStatusString },
                    { 2, newStatusString }
                };
            }

            Dictionary<string, Dictionary<int, string>> actual = new();

            fileProcessingDB.ActiveWorkflow = _WORKFLOW1;
            int actionID = fileProcessingDB.GetActionID(_LABDE_ACTION1);
            actual[_WORKFLOW1 + _LABDE_ACTION1 + "NO"] = fileProcessingDB.GetFilesWithStatusForAction(actionID, -1);
            actual[_WORKFLOW1 + _LABDE_ACTION1 + "1"] = fileProcessingDB.GetFilesWithStatusForAction(actionID, 1);

            actionID = fileProcessingDB.GetActionID(_LABDE_ACTION2);
            actual[_WORKFLOW1 + _LABDE_ACTION2 + "NO"] = fileProcessingDB.GetFilesWithStatusForAction(actionID, -1);
            actual[_WORKFLOW1 + _LABDE_ACTION2 + "1"] = fileProcessingDB.GetFilesWithStatusForAction(actionID, 1);

            fileProcessingDB.ActiveWorkflow = _WORKFLOW2;
            actionID = fileProcessingDB.GetActionID(_LABDE_ACTION1);
            actual[_WORKFLOW2 + _LABDE_ACTION1 + "NO"] = fileProcessingDB.GetFilesWithStatusForAction(actionID, -1);
            actual[_WORKFLOW2 + _LABDE_ACTION1 + "1"] = fileProcessingDB.GetFilesWithStatusForAction(actionID, 1);
            actual[_WORKFLOW2 + _LABDE_ACTION1 + "2"] = fileProcessingDB.GetFilesWithStatusForAction(actionID, 2);

            actionID = fileProcessingDB.GetActionID(_LABDE_ACTION2);
            actual[_WORKFLOW2 + _LABDE_ACTION2 + "NO"] = fileProcessingDB.GetFilesWithStatusForAction(actionID, -1);
            actual[_WORKFLOW2 + _LABDE_ACTION2 + "1"] = fileProcessingDB.GetFilesWithStatusForAction(actionID, 1);
            actual[_WORKFLOW2 + _LABDE_ACTION2 + "2"] = fileProcessingDB.GetFilesWithStatusForAction(actionID, 2);

            actionID = fileProcessingDB.GetActionID(_LABDE_ACTION3);
            actual[_WORKFLOW2 + _LABDE_ACTION3 + "NO"] = fileProcessingDB.GetFilesWithStatusForAction(actionID, -1);
            actual[_WORKFLOW2 + _LABDE_ACTION3 + "1"] = fileProcessingDB.GetFilesWithStatusForAction(actionID, 1);
            actual[_WORKFLOW2 + _LABDE_ACTION3 + "2"] = fileProcessingDB.GetFilesWithStatusForAction(actionID, 2);

            // compare the 2

            Assert.Multiple(() =>
            {
                foreach (var item in expected)
                {
                    CollectionAssert.AreEquivalent(item.Value, actual[item.Key],
                        Invariant($"FileActionStatus records should match expected for {workflow}, {actionName} and  {userID}"));
                }
            });
        }

        /// <summary>
        /// This is used to get the status expected after a call to SetStatusForAllFiles
        /// NOTE: Assumes there are 3 files in the database with ids 1, 2, 3
        /// </summary>
        /// <param name="numberOfFiles">Total number of files in the database</param>
        /// <param name="newStatus">Status all the files will be set to </param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static IActionStatistics ExpectedStatisticsForSetStatusForAll( EActionStatus newStatus) => newStatus switch
        {
            EActionStatus.kActionPending => new ActionStatistics() { NumDocuments = 3, NumDocumentsPending = 3 },
            EActionStatus.kActionFailed => new ActionStatistics() { NumDocuments = 3, NumDocumentsFailed = 3},
            EActionStatus.kActionSkipped => new ActionStatistics() { NumDocuments = 3, NumDocumentsSkipped = 3 },
            EActionStatus.kActionCompleted => new ActionStatistics() { NumDocuments = 3, NumDocumentsComplete = 3},

            _ => throw new ArgumentOutOfRangeException(nameof(newStatus), Invariant($"Not an expected value for {newStatus.AsStatusString()}"))
        };

        private static string AsStatusString(this EActionStatus status) => status switch
        {
            EActionStatus.kActionPending => "P",
            EActionStatus.kActionFailed => "F",
            EActionStatus.kActionSkipped => "S",
            EActionStatus.kActionCompleted => "C",
            _ => "U"
        };

        #endregion
    }
}
