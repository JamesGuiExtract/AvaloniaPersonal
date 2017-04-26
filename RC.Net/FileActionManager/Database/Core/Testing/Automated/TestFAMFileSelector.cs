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

        static readonly int _CURRENT_WORKFLOW = -1;

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

        #region Helper Methods

        /// <summary>
        /// Statuses by file index position
        ///            |  P  |  R  |  S  |  C  |  F 
        ///  Action 1   0,1,4              2,3               
        ///  Action 2                 2           3
        /// High priority: 1
        /// </summary>
        /// <param name="fileProcessingDb">The file processing database.</param>
        /// <returns></returns>
        static string[] AddTestFiles1(this FileProcessingDB fileProcessingDb)
        {
            string testFileName0 = _testFiles.GetFile(_LABDE_TEST_FILE1);
            string testFileName1 = _testFiles.GetFile(_LABDE_TEST_FILE2);
            string testFileName2 = _testFiles.GetFile(_LABDE_TEST_FILE3);
            string testFileName3 = _testFiles.GetFile(_LABDE_TEST_FILE4);
            string testFileName4 = _testFiles.GetFile(_LABDE_TEST_FILE5);

            fileProcessingDb.RecordFAMSessionStart("Test.fps", _LABDE_ACTION1, true, false);

            var fileIDs = new List<string>();

            var fileRecord = fileProcessingDb.AddFile(
                testFileName0, _LABDE_ACTION1, _CURRENT_WORKFLOW, EFilePriority.kPriorityNormal, false, false,
                EActionStatus.kActionPending, true, out bool alreadyExists, out EActionStatus previousStatus);
            fileIDs.Add(fileRecord.FileID.AsString());
            fileRecord = fileProcessingDb.AddFile(
                testFileName1, _LABDE_ACTION1, _CURRENT_WORKFLOW, EFilePriority.kPriorityHigh, false, false,
                EActionStatus.kActionPending, true, out alreadyExists, out previousStatus);
            fileIDs.Add(fileRecord.FileID.AsString());
            fileRecord = fileProcessingDb.AddFile(
                testFileName2, _LABDE_ACTION1, _CURRENT_WORKFLOW, EFilePriority.kPriorityNormal, false, false,
                EActionStatus.kActionCompleted, true, out alreadyExists, out previousStatus);
            fileProcessingDb.NotifyFileSkipped(fileRecord.FileID, _LABDE_ACTION2, -1, false);
            fileIDs.Add(fileRecord.FileID.AsString());
            fileRecord = fileProcessingDb.AddFile(
                testFileName3, _LABDE_ACTION1, _CURRENT_WORKFLOW, EFilePriority.kPriorityNormal, false, false,
                EActionStatus.kActionCompleted, true, out alreadyExists, out previousStatus);
            fileProcessingDb.NotifyFileFailed(fileRecord.FileID, _LABDE_ACTION2, -1,
                new ExtractException("ELI43262", "Simulated failure").ToSerializedHexString(), false);
            fileIDs.Add(fileRecord.FileID.AsString());
            fileRecord = fileProcessingDb.AddFile(
                testFileName4, _LABDE_ACTION1, _CURRENT_WORKFLOW, EFilePriority.kPriorityNormal, false, false,
                EActionStatus.kActionPending, true, out alreadyExists, out previousStatus);
            fileIDs.Add(fileRecord.FileID.AsString());

            fileProcessingDb.RecordFAMSessionStop();

            return fileIDs.ToArray();
        }

        /// <summary>
        /// Statuses by file index position
        ///            |  P  |  R  |  S  |  C  |  F 
        ///  Action 1     0                 1
        ///  Action 2
        ///  Action 3     2
        /// High priority: 1,2
        /// </summary>
        /// <param name="fileProcessingDb">The file processing database.</param>
        /// <returns></returns>
        static string[] AddTestFiles2(this FileProcessingDB fileProcessingDb)
        {
            string testFileName0 = _testFiles.GetFile(_LABDE_TEST_FILE5);
            string testFileName1 = _testFiles.GetFile(_LABDE_TEST_FILE6);
            string testFileName2 = _testFiles.GetFile(_LABDE_TEST_FILE7);

            fileProcessingDb.RecordFAMSessionStart("Test.fps", _LABDE_ACTION1, true, false);

            var fileIDs = new List<string>();

            var fileRecord = fileProcessingDb.AddFile(
                testFileName0, _LABDE_ACTION1, _CURRENT_WORKFLOW, EFilePriority.kPriorityNormal, false, false,
                EActionStatus.kActionPending, true, out bool alreadyExists, out EActionStatus previousStatus);
            fileIDs.Add(fileRecord.FileID.AsString());
            fileRecord = fileProcessingDb.AddFile(
                testFileName1, _LABDE_ACTION1, _CURRENT_WORKFLOW, EFilePriority.kPriorityHigh, false, false,
                EActionStatus.kActionCompleted, true, out alreadyExists, out previousStatus);
            fileIDs.Add(fileRecord.FileID.AsString());
            fileRecord = fileProcessingDb.AddFile(
                testFileName2, _LABDE_ACTION3, _CURRENT_WORKFLOW, EFilePriority.kPriorityHigh, false, false,
                EActionStatus.kActionPending, true, out alreadyExists, out previousStatus);
            fileIDs.Add(fileRecord.FileID.AsString());

            fileProcessingDb.RecordFAMSessionStop();

            return fileIDs.ToArray();
        }

        /// <summary>
        /// Gets the file IDs selected by the specified <see paarmref="fileSelector"/>.
        /// </summary>
        /// <param name="fileSelector">The <see cref="FAMFileSelector"/> to use.</param>
        /// <param name="fileProcessingDb">The <see cref="FileProcessingDB"/> to use.</param>
        /// <returns>And array of file IDs selected (as strings).</returns>
        static string[] GetResults(this FAMFileSelector fileSelector, FileProcessingDB fileProcessingDb)
        {
            string[] resultsArray;

            using (DataTable resultsTable = new DataTable())
            {
                resultsTable.Locale = CultureInfo.CurrentCulture;

                string query = fileSelector.BuildQuery(
                    fileProcessingDb, "[FAMFile].[ID]", "ORDER BY [FAMFile].[ID]");

                Recordset adoRecordset = fileProcessingDb.GetResultsForQuery(query);

                using (OleDbDataAdapter adapter = new System.Data.OleDb.OleDbDataAdapter())
                {
                    adapter.Fill(resultsTable, adoRecordset);
                }

                adoRecordset.Close();

                resultsArray = resultsTable.ToStringArray("\t");
            }

            return resultsArray;
        }

        #endregion Helper Methods

        #region TestMethods

        [Test, Category("TestFAMFileSelector")]
        public static void FAMFileSelector()
        {
            string testDbName = "Test_FAMFileSelector";

            try
            {
                var fileProcessingDb = _testDbManager.GetDatabase(_LABDE_EMPTY_DB, testDbName);
                var fileIDs = fileProcessingDb.AddTestFiles1();
                var fileSelector = new FAMFileSelector();

                Assert.IsTrue(fileSelector.SelectingAllFiles);

                fileSelector.AddActionStatusCondition(fileProcessingDb, _LABDE_ACTION1, EActionStatus.kActionPending);

                Assert.IsFalse(fileSelector.SelectingAllFiles);
                Assert.AreEqual(
                    Invariant($"{fileIDs[0]},{fileIDs[1]},{fileIDs[4]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));

                fileSelector.Reset();
                fileSelector.AddQueryCondition("SELECT FAMFile.ID FROM FAMFile WHERE [Priority] = 5");

                Assert.AreEqual(fileSelector.GetResults(fileProcessingDb).Single(), fileIDs[1]);

                fileSelector.Reset();
                fileProcessingDb.AddFileSet("TestFileSet",
                    new[]
                    {
                        int.Parse(fileIDs[0], CultureInfo.InvariantCulture),
                        int.Parse(fileIDs[2], CultureInfo.InvariantCulture)
                    }
                    .ToVariantVector());
                fileSelector.AddFileSetCondition("TestFileSet");

                Assert.AreEqual(
                    Invariant($"{fileIDs[0]},{fileIDs[2]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));
            }
            finally
            {
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

                int workflowId1 = fileProcessingDb.AddWorkflow("Workflow1", EWorkflowType.kUndefined);
                fileProcessingDb.SetWorkflowActions(workflowId1, new[] { _LABDE_ACTION1, _LABDE_ACTION2 }.ToVariantVector());
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                var fileIDs1 = fileProcessingDb.AddTestFiles1();

                int workflowId2 = fileProcessingDb.AddWorkflow("Workflow2", EWorkflowType.kUndefined);
                fileProcessingDb.SetWorkflowActions(workflowId2, new[] { _LABDE_ACTION1, _LABDE_ACTION3 }.ToVariantVector());
                fileProcessingDb.ActiveWorkflow = "Workflow2";
                var fileIDs2 = fileProcessingDb.AddTestFiles2();

                var fileSelector = new FAMFileSelector();
                fileSelector.AddActionStatusCondition(fileProcessingDb, _LABDE_ACTION1, EActionStatus.kActionPending);

                fileProcessingDb.ActiveWorkflow = "";
                Assert.AreEqual(
                    Invariant($"{fileIDs1[0]},{fileIDs1[1]},{fileIDs2[0]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                Assert.AreEqual(
                    Invariant($"{fileIDs1[0]},{fileIDs1[1]},{fileIDs1[4]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));
                fileProcessingDb.ActiveWorkflow = "Workflow2";
                Assert.AreEqual(fileIDs2[0], fileSelector.GetResults(fileProcessingDb).Single());

                fileSelector.Reset();
                fileSelector.AddQueryCondition("SELECT FAMFile.ID FROM FAMFile WHERE [Priority] = 5");

                fileProcessingDb.ActiveWorkflow = "";
                Assert.AreEqual(
                    Invariant($"{fileIDs1[1]},{fileIDs2[1]},{fileIDs2[2]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                Assert.AreEqual(
                    Invariant($"{fileIDs1[1]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));
                fileProcessingDb.ActiveWorkflow = "Workflow2";
                Assert.AreEqual(
                    Invariant($"{fileIDs2[1]},{fileIDs2[2]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));

                fileSelector.Reset();
                fileProcessingDb.AddFileSet("TestFileSet",
                    new[]
                    {
                        int.Parse(fileIDs1[0], CultureInfo.InvariantCulture),
                        int.Parse(fileIDs2[2], CultureInfo.InvariantCulture)
                    }
                    .ToVariantVector());
                fileSelector.AddFileSetCondition("TestFileSet");

                fileProcessingDb.ActiveWorkflow = "";
                Assert.AreEqual(
                    Invariant($"{fileIDs1[0]},{fileIDs2[2]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                Assert.AreEqual(
                    Invariant($"{fileIDs1[0]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));
                fileProcessingDb.ActiveWorkflow = "Workflow2";
                Assert.AreEqual(
                    Invariant($"{fileIDs2[2]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));
            }
            finally
            {
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
                var fileIDs = fileProcessingDb.AddTestFiles1();

                var fileSelector = new FAMFileSelector();
                fileSelector.AddActionStatusCondition(fileProcessingDb, _LABDE_ACTION1, EActionStatus.kActionPending);
                fileSelector.LimitToSubset(bRandomSubset: true, bTopSubset: false, bUsePercentage: true, nSubsetSize: 66);

                Assert.AreEqual(2, fileSelector.GetResults(fileProcessingDb).Count());

                fileSelector.LimitToSubset(bRandomSubset: false, bTopSubset: true, bUsePercentage: false, nSubsetSize: 2);

                Assert.AreEqual(
                    Invariant($"{fileIDs[0]},{fileIDs[1]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));

                fileSelector.LimitToSubset(bRandomSubset: false, bTopSubset: false, bUsePercentage: false, nSubsetSize: 1);

                Assert.AreEqual(
                    Invariant($"{fileIDs[4]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));
            }
            finally
            {
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

                int workflowId1 = fileProcessingDb.AddWorkflow("Workflow1", EWorkflowType.kUndefined);
                fileProcessingDb.SetWorkflowActions(workflowId1, new[] { _LABDE_ACTION1, _LABDE_ACTION2 }.ToVariantVector());
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                var fileIDs1 = fileProcessingDb.AddTestFiles1();

                int workflowId2 = fileProcessingDb.AddWorkflow("Workflow2", EWorkflowType.kUndefined);
                fileProcessingDb.SetWorkflowActions(workflowId2, new[] { _LABDE_ACTION1, _LABDE_ACTION3 }.ToVariantVector());
                fileProcessingDb.ActiveWorkflow = "Workflow2";
                var fileIDs2 = fileProcessingDb.AddTestFiles2();

                var fileSelector = new FAMFileSelector();
                fileSelector.AddActionStatusCondition(fileProcessingDb, _LABDE_ACTION1, EActionStatus.kActionPending);
                fileSelector.LimitToSubset(bRandomSubset: true, bTopSubset: false, bUsePercentage: true, nSubsetSize: 66);

                fileProcessingDb.ActiveWorkflow = "";
                Assert.AreEqual(2, fileSelector.GetResults(fileProcessingDb).Count());
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                Assert.AreEqual(2, fileSelector.GetResults(fileProcessingDb).Count());
                fileProcessingDb.ActiveWorkflow = "Workflow2";
                Assert.AreEqual(1, fileSelector.GetResults(fileProcessingDb).Count());

                fileSelector.LimitToSubset(bRandomSubset: false, bTopSubset: true, bUsePercentage: false, nSubsetSize: 2);

                fileProcessingDb.ActiveWorkflow = "";
                Assert.AreEqual(
                    Invariant($"{fileIDs1[0]},{fileIDs1[1]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                Assert.AreEqual(
                    Invariant($"{fileIDs1[0]},{fileIDs1[1]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));
                fileProcessingDb.ActiveWorkflow = "Workflow2";
                Assert.AreEqual(
                    Invariant($"{fileIDs2[0]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));

                fileSelector.LimitToSubset(bRandomSubset: false, bTopSubset: false, bUsePercentage: false, nSubsetSize: 1);

                fileProcessingDb.ActiveWorkflow = "";
                Assert.AreEqual(
                    Invariant($"{fileIDs2[0]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));
                fileProcessingDb.ActiveWorkflow = "Workflow1";
                Assert.AreEqual(
                    Invariant($"{fileIDs1[4]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));
                fileProcessingDb.ActiveWorkflow = "Workflow2";
                Assert.AreEqual(
                    Invariant($"{fileIDs2[0]}"),
                    string.Join(",", fileSelector.GetResults(fileProcessingDb)));
            }
            finally
            {
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        #endregion TestMethods
    }
}
