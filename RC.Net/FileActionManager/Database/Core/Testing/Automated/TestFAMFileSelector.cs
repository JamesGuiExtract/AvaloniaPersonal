using ADODB;
using Extract.Database;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.Linq;
using UCLID_FILEPROCESSINGLib;

using static System.FormattableString;

namespace Extract.FileActionManager.Database.Test
{
    [Category("TestFAMFileSelector")]
    [TestFixture]
    public static class TestFAMFileSelector
    {
        #region Constants

        static readonly string _LABDE_EMPTY_DB = "Resources.Demo_LabDE_Empty";
        static readonly string _LABDE_TEST_FILE1 = "Resources.TestImage001.tif";
        static readonly string _LABDE_TEST_FILE2 = "Resources.TestImage002.tif";
        static readonly string _LABDE_TEST_FILE3 = "Resources.TestImage003.tif";
        static readonly string _LABDE_TEST_FILE4 = "Resources.TestImage004.tif";
        static readonly string _LABDE_TEST_FILE5 = "Resources.TestImage005.tif";
        static readonly string _LABDE_TEST_FILE6 = "Resources.TestImage006.tif";
        static readonly string _LABDE_TEST_FILE7 = "Resources.TestImage007.tif";

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

        #region TestMethods

        [Test, Category("TestFAMFileSelector")]
        public static void FAMFileSelector()
        {
            string testDbName = "Test_FAMFileSelector";

            try
            {
                var fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                /// Statuses by file index position
                ///            |  P  |  R  |  S  |  C  |  F 
                ///  Action 1   1,2,5              3,4               
                ///  Action 2                 3           4
                /// High priority: 2
                (string fileName, string actionName, int workflowID, EActionStatus actionStatus, EFilePriority priority)[] testFiles =
                {
                    (_LABDE_TEST_FILE1, _LABDE_ACTION1, -1, EActionStatus.kActionPending, EFilePriority.kPriorityNormal),
                    (_LABDE_TEST_FILE2, _LABDE_ACTION1, -1, EActionStatus.kActionPending, EFilePriority.kPriorityHigh),
                    (_LABDE_TEST_FILE3, _LABDE_ACTION1, -1, EActionStatus.kActionCompleted, EFilePriority.kPriorityNormal),
                    (_LABDE_TEST_FILE3, _LABDE_ACTION2, -1, EActionStatus.kActionSkipped, EFilePriority.kPriorityNormal),
                    (_LABDE_TEST_FILE4, _LABDE_ACTION1, -1, EActionStatus.kActionCompleted, EFilePriority.kPriorityNormal),
                    (_LABDE_TEST_FILE4, _LABDE_ACTION2, -1, EActionStatus.kActionFailed, EFilePriority.kPriorityNormal),
                    (_LABDE_TEST_FILE5, _LABDE_ACTION1, -1, EActionStatus.kActionPending, EFilePriority.kPriorityNormal),
                };
                var fileIDs = fileProcessingDb.AddTestFiles(_testFiles, testFiles);
                var fileSelector = new FAMFileSelector();

                Assert.IsTrue(fileSelector.SelectingAllFiles);

                fileSelector.AddActionStatusCondition(fileProcessingDb, _LABDE_ACTION1, EActionStatus.kActionPending);

                Assert.IsFalse(fileSelector.SelectingAllFiles);
                Assert.AreEqual(
                    Invariant($"{fileIDs[1]},{fileIDs[2]},{fileIDs[5]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));

                fileSelector.Reset();
                fileSelector.AddQueryCondition("SELECT FAMFile.ID FROM FAMFile WHERE [Priority] = 5");

                Assert.AreEqual(fileSelector.GetResults(fileProcessingDb).Single(), fileIDs[2]);

                fileSelector.Reset();
                fileProcessingDb.AddFileSet("TestFileSet",
                    new[]
                    {
                        int.Parse(fileIDs[1], CultureInfo.InvariantCulture),
                        int.Parse(fileIDs[3], CultureInfo.InvariantCulture)
                    }
                    .ToVariantVector());
                fileSelector.AddFileSetCondition("TestFileSet");

                Assert.AreEqual(
                    Invariant($"{fileIDs[1]},{fileIDs[3]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));
            }
            finally
            {
                _testFiles.RemoveFile(_LABDE_TEST_FILE1);
                _testFiles.RemoveFile(_LABDE_TEST_FILE2);
                _testFiles.RemoveFile(_LABDE_TEST_FILE3);
                _testFiles.RemoveFile(_LABDE_TEST_FILE4);
                _testFiles.RemoveFile(_LABDE_TEST_FILE5);
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        [Test, Category("TestFAMFileSelector")]
        public static void FAMFileSelectorWithWorkflows()
        {
            string testDbName = "Test_FAMFileSelectorWithWorkflows";

            try
            {
                var fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                int workflowID1 = fileProcessingDb.AddWorkflow(
                    "Workflow1", EWorkflowType.kUndefined, _LABDE_ACTION1, _LABDE_ACTION2);
                int workflowID2 = fileProcessingDb.AddWorkflow(
                    "Workflow2", EWorkflowType.kUndefined, _LABDE_ACTION1, _LABDE_ACTION3);

                /// Statuses by file index position
                ///  Workflow1 |  P  |  R  |  S  |  C  |  F 
                ///  Action 1   1,2,5              3,4               
                ///  Action 2                 3           4
                /// High priority: 2
                ///  Workflow2 |  P  |  R  |  S  |  C  |  F 
                ///  Action 1     5                 6
                ///  Action 2
                ///  Action 3     7
                /// High priority: 6,7
                (string fileName, string actionName, int workflowID, EActionStatus actionStatus, EFilePriority priority)[] testFiles =
                {
                    (_LABDE_TEST_FILE1, _LABDE_ACTION1, workflowID1, EActionStatus.kActionPending, EFilePriority.kPriorityNormal),
                    (_LABDE_TEST_FILE2, _LABDE_ACTION1, workflowID1, EActionStatus.kActionPending, EFilePriority.kPriorityHigh),
                    (_LABDE_TEST_FILE3, _LABDE_ACTION1, workflowID1, EActionStatus.kActionCompleted, EFilePriority.kPriorityNormal),
                    (_LABDE_TEST_FILE3, _LABDE_ACTION2, workflowID1, EActionStatus.kActionSkipped, EFilePriority.kPriorityNormal),
                    (_LABDE_TEST_FILE4, _LABDE_ACTION1, workflowID1, EActionStatus.kActionCompleted, EFilePriority.kPriorityNormal),
                    (_LABDE_TEST_FILE4, _LABDE_ACTION2, workflowID1, EActionStatus.kActionFailed, EFilePriority.kPriorityNormal),
                    (_LABDE_TEST_FILE5, _LABDE_ACTION1, workflowID1, EActionStatus.kActionPending, EFilePriority.kPriorityNormal),
                    (_LABDE_TEST_FILE5, _LABDE_ACTION1, workflowID2, EActionStatus.kActionPending, EFilePriority.kPriorityNormal),
                    (_LABDE_TEST_FILE6, _LABDE_ACTION1, workflowID2, EActionStatus.kActionCompleted, EFilePriority.kPriorityHigh),
                    (_LABDE_TEST_FILE7, _LABDE_ACTION3, workflowID2, EActionStatus.kActionPending, EFilePriority.kPriorityHigh),
                };
                var fileIDs = fileProcessingDb.AddTestFiles(_testFiles, testFiles);

                var fileSelector = new FAMFileSelector();
                fileSelector.AddActionStatusCondition(fileProcessingDb, _LABDE_ACTION1, EActionStatus.kActionPending);

                fileProcessingDb.ActiveWorkflow = "";
                Assert.AreEqual(
                    Invariant($"{fileIDs[1]},{fileIDs[2]},{fileIDs[5]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                Assert.AreEqual(
                    Invariant($"{fileIDs[1]},{fileIDs[2]},{fileIDs[5]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));
                fileProcessingDb.ActiveWorkflow = "Workflow2";
                Assert.AreEqual(fileIDs[5], fileSelector.GetResults(fileProcessingDb).Single());

                fileSelector.Reset();
                fileSelector.AddQueryCondition("SELECT FAMFile.ID FROM FAMFile WHERE [Priority] = 5");

                fileProcessingDb.ActiveWorkflow = "";
                Assert.AreEqual(
                    Invariant($"{fileIDs[2]},{fileIDs[6]},{fileIDs[7]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                Assert.AreEqual(
                    Invariant($"{fileIDs[2]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));
                fileProcessingDb.ActiveWorkflow = "Workflow2";
                Assert.AreEqual(
                    Invariant($"{fileIDs[6]},{fileIDs[7]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));

                fileSelector.Reset();
                fileProcessingDb.AddFileSet("TestFileSet",
                    new[]
                    {
                        int.Parse(fileIDs[1], CultureInfo.InvariantCulture),
                        int.Parse(fileIDs[7], CultureInfo.InvariantCulture)
                    }
                    .ToVariantVector());
                fileSelector.AddFileSetCondition("TestFileSet");

                fileProcessingDb.ActiveWorkflow = "";
                Assert.AreEqual(
                    Invariant($"{fileIDs[1]},{fileIDs[7]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                Assert.AreEqual(
                    Invariant($"{fileIDs[1]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));
                fileProcessingDb.ActiveWorkflow = "Workflow2";
                Assert.AreEqual(
                    Invariant($"{fileIDs[7]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));
            }
            finally
            {
                _testFiles.RemoveFile(_LABDE_TEST_FILE1);
                _testFiles.RemoveFile(_LABDE_TEST_FILE2);
                _testFiles.RemoveFile(_LABDE_TEST_FILE3);
                _testFiles.RemoveFile(_LABDE_TEST_FILE4);
                _testFiles.RemoveFile(_LABDE_TEST_FILE5);
                _testFiles.RemoveFile(_LABDE_TEST_FILE6);
                _testFiles.RemoveFile(_LABDE_TEST_FILE7);
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        [Test, Category("TestFAMFileSelector")]
        public static void FAMFileSelectorFilters()
        {
            string testDbName = "Test_FAMFileSelectorFilters";

            try
            {
                var fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                /// Statuses by file index position
                ///            |  P  |  R  |  S  |  C  |  F 
                ///  Action 1   1,2,5              3,4               
                ///  Action 2                 3           4
                /// High priority: 2
                (string fileName, string actionName, int workflowID, EActionStatus actionStatus, EFilePriority priority)[] testFiles =
                {
                    (_LABDE_TEST_FILE1, _LABDE_ACTION1, -1, EActionStatus.kActionPending, EFilePriority.kPriorityNormal),
                    (_LABDE_TEST_FILE2, _LABDE_ACTION1, -1, EActionStatus.kActionPending, EFilePriority.kPriorityHigh),
                    (_LABDE_TEST_FILE3, _LABDE_ACTION1, -1, EActionStatus.kActionCompleted, EFilePriority.kPriorityNormal),
                    (_LABDE_TEST_FILE3, _LABDE_ACTION2, -1, EActionStatus.kActionSkipped, EFilePriority.kPriorityNormal),
                    (_LABDE_TEST_FILE4, _LABDE_ACTION1, -1, EActionStatus.kActionCompleted, EFilePriority.kPriorityNormal),
                    (_LABDE_TEST_FILE4, _LABDE_ACTION2, -1, EActionStatus.kActionFailed, EFilePriority.kPriorityNormal),
                    (_LABDE_TEST_FILE5, _LABDE_ACTION1, -1, EActionStatus.kActionPending, EFilePriority.kPriorityNormal),
                };
                var fileIDs = fileProcessingDb.AddTestFiles(_testFiles, testFiles);

                var fileSelector = new FAMFileSelector();
                fileSelector.AddActionStatusCondition(fileProcessingDb, _LABDE_ACTION1, EActionStatus.kActionPending);
                fileSelector.LimitToSubset(bRandomSubset: true, bTopSubset: false, bUsePercentage: true, nSubsetSize: 66, nOffset: -1);

                Assert.AreEqual(2, fileSelector.GetResults(fileProcessingDb).Count());

                fileSelector.LimitToSubset(bRandomSubset: false, bTopSubset: true, bUsePercentage: false, nSubsetSize: 2, nOffset: -1);

                Assert.AreEqual(
                    Invariant($"{fileIDs[1]},{fileIDs[2]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));

                fileSelector.LimitToSubset(bRandomSubset: false, bTopSubset: false, bUsePercentage: false, nSubsetSize: 1, nOffset: -1);

                Assert.AreEqual(
                    Invariant($"{fileIDs[5]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));
            }
            finally
            {
                _testFiles.RemoveFile(_LABDE_TEST_FILE1);
                _testFiles.RemoveFile(_LABDE_TEST_FILE2);
                _testFiles.RemoveFile(_LABDE_TEST_FILE3);
                _testFiles.RemoveFile(_LABDE_TEST_FILE4);
                _testFiles.RemoveFile(_LABDE_TEST_FILE5);
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        [Test, Category("TestFAMFileSelector")]
        public static void FAMFileSelectorFiltersWithWorkflows()
        {
            string testDbName = "Test_FAMFileSelectorFiltersWithWorkflows";

            try
            {
                var fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);

                int workflowID1 = fileProcessingDb.AddWorkflow(
                    "Workflow1", EWorkflowType.kUndefined, _LABDE_ACTION1, _LABDE_ACTION2);
                int workflowID2 = fileProcessingDb.AddWorkflow(
                    "Workflow2", EWorkflowType.kUndefined, _LABDE_ACTION1, _LABDE_ACTION3);

                /// Statuses by file index position
                ///  Workflow1 |  P  |  R  |  S  |  C  |  F 
                ///  Action 1   1,2,5              3,4               
                ///  Action 2                 3           4
                /// High priority: 2
                ///  Workflow2 |  P  |  R  |  S  |  C  |  F 
                ///  Action 1     5                 6
                ///  Action 2
                ///  Action 3     7
                /// High priority: 6,7
                (string fileName, string actionName, int workflowID, EActionStatus actionStatus, EFilePriority priority)[] testFiles =
                {
                    (_LABDE_TEST_FILE1, _LABDE_ACTION1, workflowID1, EActionStatus.kActionPending, EFilePriority.kPriorityNormal),
                    (_LABDE_TEST_FILE2, _LABDE_ACTION1, workflowID1, EActionStatus.kActionPending, EFilePriority.kPriorityHigh),
                    (_LABDE_TEST_FILE3, _LABDE_ACTION1, workflowID1, EActionStatus.kActionCompleted, EFilePriority.kPriorityNormal),
                    (_LABDE_TEST_FILE3, _LABDE_ACTION2, workflowID1, EActionStatus.kActionSkipped, EFilePriority.kPriorityNormal),
                    (_LABDE_TEST_FILE4, _LABDE_ACTION1, workflowID1, EActionStatus.kActionCompleted, EFilePriority.kPriorityNormal),
                    (_LABDE_TEST_FILE4, _LABDE_ACTION2, workflowID1, EActionStatus.kActionFailed, EFilePriority.kPriorityNormal),
                    (_LABDE_TEST_FILE5, _LABDE_ACTION1, workflowID1, EActionStatus.kActionPending, EFilePriority.kPriorityNormal),
                    (_LABDE_TEST_FILE5, _LABDE_ACTION1, workflowID2, EActionStatus.kActionPending, EFilePriority.kPriorityNormal),
                    (_LABDE_TEST_FILE6, _LABDE_ACTION1, workflowID2, EActionStatus.kActionCompleted, EFilePriority.kPriorityHigh),
                    (_LABDE_TEST_FILE7, _LABDE_ACTION3, workflowID2, EActionStatus.kActionPending, EFilePriority.kPriorityHigh),
                };
                var fileIDs = fileProcessingDb.AddTestFiles(_testFiles, testFiles);

                var fileSelector = new FAMFileSelector();
                fileSelector.AddActionStatusCondition(fileProcessingDb, _LABDE_ACTION1, EActionStatus.kActionPending);
                fileSelector.LimitToSubset(bRandomSubset: true, bTopSubset: false, bUsePercentage: true, nSubsetSize: 66, nOffset: -1);

                fileProcessingDb.ActiveWorkflow = "";
                Assert.AreEqual(2, fileSelector.GetResults(fileProcessingDb).Count());
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                Assert.AreEqual(2, fileSelector.GetResults(fileProcessingDb).Count());
                fileProcessingDb.ActiveWorkflow = "Workflow2";
                Assert.AreEqual(1, fileSelector.GetResults(fileProcessingDb).Count());

                fileSelector.LimitToSubset(bRandomSubset: false, bTopSubset: true, bUsePercentage: false, nSubsetSize: 2, nOffset: -1);

                fileProcessingDb.ActiveWorkflow = "";
                Assert.AreEqual(
                    Invariant($"{fileIDs[1]},{fileIDs[2]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                Assert.AreEqual(
                    Invariant($"{fileIDs[1]},{fileIDs[2]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));
                fileProcessingDb.ActiveWorkflow = "Workflow2";
                Assert.AreEqual(
                    Invariant($"{fileIDs[5]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));

                fileSelector.LimitToSubset(bRandomSubset: false, bTopSubset: false, bUsePercentage: false, nSubsetSize: 1, nOffset: -1);

                fileProcessingDb.ActiveWorkflow = "";
                Assert.AreEqual(
                    Invariant($"{fileIDs[5]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                Assert.AreEqual(
                    Invariant($"{fileIDs[5]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));
                fileProcessingDb.ActiveWorkflow = "Workflow2";
                Assert.AreEqual(
                    Invariant($"{fileIDs[5]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));
            }
            finally
            {
                _testFiles.RemoveFile(_LABDE_TEST_FILE1);
                _testFiles.RemoveFile(_LABDE_TEST_FILE2);
                _testFiles.RemoveFile(_LABDE_TEST_FILE3);
                _testFiles.RemoveFile(_LABDE_TEST_FILE4);
                _testFiles.RemoveFile(_LABDE_TEST_FILE5);
                _testFiles.RemoveFile(_LABDE_TEST_FILE6);
                _testFiles.RemoveFile(_LABDE_TEST_FILE7);
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        #endregion TestMethods
    }
}
