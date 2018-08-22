using Extract.AttributeFinder;
using Extract.FileActionManager.Database.Test;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System.Globalization;
using System.IO;
using System.Linq;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using UCLID_RASTERANDOCRMGMTLib;
using static System.FormattableString;

namespace Extract.FileActionManager.FileProcessors.Test
{
    [TestFixture]
    [Category("ValidateXmlTask")]
    public class TestSplitMultiPageDocument
    {
        #region Constants

        static readonly string _FAMDB = "Resources.TestSplitMultipageFile.bak";
        static readonly string _ACTION_NAME = "Test";
        //static readonly string _TEST_TIF_SINGLE_PAGE = "Resources.A418.tif";
        static readonly string _TEST_TIF_MULTI_PAGE = "Resources.C413.tif";
        //static readonly string _TEST_PDF_MULTI_PAGE = "Resources.test3.pdf";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Manages the test files needed for testing.
        /// </summary>
        static TestFileManager<TestSplitMultiPageDocument> _testFiles;

        /// <summary>
        /// Manages test FAM DBs.
        /// </summary>
        static FAMTestDBManager<TestSplitMultiPageDocument> _testDbManager;

        #endregion Fields

        #region Overhead Methods

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestSplitMultiPageDocument>();
            _testDbManager = new FAMTestDBManager<TestSplitMultiPageDocument>();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestFixtureTearDown]
        public static void FinalCleanup()
        {
            // Dispose of the test image manager
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

        #endregion Overhead Methods

        #region Unit Tests

        [Test, Category("Automated")]
        public static void TestSplitTif()
        {
            string testDbName = "SplitMultiPageDocument_TestSplitTif";

            try
            {
                var fileProcessingDb = _testDbManager.GetDatabase(_FAMDB, testDbName);

                string testFile = _testFiles.GetFile(_TEST_TIF_MULTI_PAGE);
                _testFiles.GetFile(_TEST_TIF_MULTI_PAGE + ".uss");
                _testFiles.GetFile(_TEST_TIF_MULTI_PAGE + ".voa");
                var fileID = fileProcessingDb.AddTestFiles<TestSplitMultiPageDocument>(_testFiles,
                    (_TEST_TIF_MULTI_PAGE, _ACTION_NAME, -1, EActionStatus.kActionPending, EFilePriority.kPriorityNormal));

                var taskConfig = new SplitMultipageDocumentTask();
                taskConfig.VOAPath = "<SourceDocName>.voa";
                taskConfig.OutputPath = 
                    @"$DirOf(<SourceDocName>)\Split_$FileOf(<SourceDocName>)\$InsertBeforeExt($FileOf(<SourceDocName>),.<PageNumber>)";
                var task = (IFileProcessingTask)taskConfig;

                using (var famSession = new FAMProcessingSession(fileProcessingDb, _ACTION_NAME, "", task))
                {
                    famSession.WaitForProcessingToComplete();
                }

                string outputDir = Path.Combine(Path.GetDirectoryName(testFile),
                    "Split_" + Path.GetFileName(testFile));
                Assert.AreEqual(true, Directory.Exists(outputDir), Invariant($"Output directory missing: {outputDir}"));

                int spatialAttributeTotal = 0;
                string voaFile = testFile + ".voa";
                var voaData = new IUnknownVector();
                voaData.LoadFrom(voaFile, false);
                foreach (var attribute in voaData.EnumerateDepthFirst())
                {
                    if (attribute.Value.HasSpatialInfo())
                    {
                        spatialAttributeTotal += attribute.Value.GetPages(false, "").ToIEnumerable<SpatialString>().Count();
                    }
                }

                System.Diagnostics.Trace.WriteLine(Invariant($"Total spatial attributes for {Path.GetFileName(testFile)}: {spatialAttributeTotal}"));

                for (int page = 1; page <= 5; page++)
                {
                    string outputPageFile = Invariant(
                        $"{outputDir}\\{Path.GetFileNameWithoutExtension(testFile)}.{page:D4}{Path.GetExtension(testFile)}");

                    int pageSpatialAttributeTotal = TestPage(outputPageFile);
                    System.Diagnostics.Trace.WriteLine(Invariant($"Spatial attributes for page {page}: {pageSpatialAttributeTotal}"));
                    spatialAttributeTotal -= pageSpatialAttributeTotal;
                }

                Assert.AreEqual(0, spatialAttributeTotal, Invariant($"Not all attributes accounted for: {spatialAttributeTotal}"));
            }
            finally
            {
                _testFiles.RemoveFile(_TEST_TIF_MULTI_PAGE);
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        [Test, Category("Interactive")]
        public static void TestSplitTifTwice()
        {
            string testDbName = "SplitMultiPageDocument_TestSplitTifTwice";

            try
            {
                var fileProcessingDb = _testDbManager.GetDatabase(_FAMDB, testDbName);

                string testFile = _testFiles.GetFile(_TEST_TIF_MULTI_PAGE);
                _testFiles.GetFile(_TEST_TIF_MULTI_PAGE + ".uss");
                _testFiles.GetFile(_TEST_TIF_MULTI_PAGE + ".voa");
                var fileIDStrings = fileProcessingDb.AddTestFiles<TestSplitMultiPageDocument>(_testFiles,
                    (_TEST_TIF_MULTI_PAGE, _ACTION_NAME, -1, EActionStatus.kActionPending, EFilePriority.kPriorityNormal));
                int fileId = int.Parse(fileIDStrings[1], CultureInfo.InvariantCulture);

                var taskConfig = new SplitMultipageDocumentTask();
                taskConfig.VOAPath = "<SourceDocName>.voa";
                taskConfig.OutputPath =
                    @"$DirOf(<SourceDocName>)\Split_$FileOf(<SourceDocName>)\$InsertBeforeExt($FileOf(<SourceDocName>),.<PageNumber>)";
                var task = (IFileProcessingTask)taskConfig;

                using (var famSession = new FAMProcessingSession(fileProcessingDb, _ACTION_NAME, "", task))
                {

                    System.Diagnostics.Trace.WriteLine($"Processed {famSession.WaitForProcessingToComplete()} file(s)");
                    //Assert.AreEqual(1, famSession.WaitForProcessingToComplete());
                }

                // Now process it again and ensure there are no errors for duplicate entries in pagination table.
                fileProcessingDb.SetFileStatusToPending(fileId, _ACTION_NAME, false);
                using (var famSession = new FAMProcessingSession(fileProcessingDb, _ACTION_NAME, "", task))
                {
                    //Assert.AreEqual(1, famSession.WaitForProcessingToComplete());
                    System.Diagnostics.Trace.WriteLine($"Processed {famSession.WaitForProcessingToComplete()} file(s)");
                }

                UtilityMethods.ShowMessageBox("Test", "Test", false);
            }
            finally
            {
                _testFiles.RemoveFile(_TEST_TIF_MULTI_PAGE);
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outputPageFile">The test file.</param>
        /// <returns></returns>
        static int TestPage(string outputPageFile)
        {
            int spatialAttributeTotal = 0;

            Assert.AreEqual(true, File.Exists(outputPageFile), Invariant($"Output page missing: {outputPageFile}"));
            string ussFile = outputPageFile + ".uss";
            var ussData = new SpatialString();
            ussData.LoadFrom(ussFile, false);
            Assert.AreEqual(1, ussData.GetFirstPageNumber(), "Unexpected page in uss");
            Assert.AreEqual(1, ussData.GetLastPageNumber(), "Unexpected page in uss");

            string voaFile = outputPageFile + ".voa";
            var voaData = new IUnknownVector();
            voaData.LoadFrom(voaFile, false);
            foreach (var attribute in voaData.EnumerateDepthFirst())
            {
                if (attribute.Value.HasSpatialInfo())
                {
                    Assert.AreEqual(1, attribute.Value.GetFirstPageNumber(), "Spatial attribute on wrong page");
                    spatialAttributeTotal++;
                }
            }

            return spatialAttributeTotal;
        }

        #endregion Unit Tests
    }
}
