using Extract.AttributeFinder;
using Extract.FileActionManager.Database.Test;
using Extract.Imaging.Utilities;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.FileActionManager.FileProcessors.Test
{
    [TestFixture]
    [Category("CombinePagesTask")]
    public class TestCombinePagesTask
    {
        #region Constants

        static readonly string _FAMDB = "Resources.TestSplitMultipageFile.bak";
        static readonly string _ACTION_NAME = "Test";
        static readonly string _TIF_SINGLE_PAGE = "Resources.A418.tif";
        static readonly string _TIF_MULTI_PAGE = "Resources.C413.tif";
        static readonly string _PDF_MULTI_PAGE = "Resources.test3.pdf";
        static readonly string _TIF_50_PAGE = "Resources.0050pages.tif";
        static readonly string _PDF_ROT_270 = "Resources.SUPPAGRE.PDF";

        static readonly Dictionary<string, (string resourceName, int pageCount)> SourceFiles= new()
        {
            {nameof(_TIF_SINGLE_PAGE), (_TIF_SINGLE_PAGE, 1)},
            {nameof(_TIF_MULTI_PAGE), (_TIF_MULTI_PAGE, 5)},
            {nameof(_PDF_MULTI_PAGE), (_PDF_MULTI_PAGE, 4)},
            {nameof(_TIF_50_PAGE), (_TIF_50_PAGE, 50)},
            {nameof(_PDF_ROT_270), (_PDF_ROT_270, 1)}
        };

        const string _ALL_PAGES = "1..";
        const string _FIRST_PAGE = "1";
        const string _ALL_EXCEPT_FIRST_PAGE = "2..";
        const string _FIRST_TWO_PAGES = "..2";
        const string _LAST_3_PAGES = "-3..";
        const string _FIRST_AND_LAST_PAGE = "1,-1";
        const string _SECOND_AND_SECOND_TO_LAST_PAGE = "2,-2";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Manages the test files needed for testing.
        /// </summary>
        static TestFileManager<TestCombinePagesTask> _testFiles;

        /// <summary>
        /// Manages test FAM DBs.
        /// </summary>
        static FAMTestDBManager<TestCombinePagesTask> _testDbManager;

        #endregion Fields

        #region Overhead Methods

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestCombinePagesTask>();
            _testDbManager = new FAMTestDBManager<TestCombinePagesTask>();

            UnlockLeadtools.UnlockPdfSupport(false);
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [OneTimeTearDown]
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

        [Category("Automated")]
        [TestCase(
            new[] { nameof(_TIF_SINGLE_PAGE), nameof(_TIF_MULTI_PAGE) },
            new[] { _FIRST_PAGE, _ALL_PAGES },
            nameof(_TIF_MULTI_PAGE), null, true,
            TestName = "Cover page on SourceDocName",
            ExpectedResult = true)]
        [TestCase(
            new[] { nameof(_PDF_ROT_270), nameof(_TIF_MULTI_PAGE) },
            new[] { _FIRST_PAGE, _ALL_PAGES },
            nameof(_TIF_MULTI_PAGE), null, false,
            TestName = "PDF cover page on SourceDocName (no data)",
            ExpectedResult = true)]
        [TestCase(
            new[] { nameof(_TIF_SINGLE_PAGE), nameof(_TIF_MULTI_PAGE) },
            new[] { _FIRST_PAGE, _ALL_PAGES },
            nameof(_TIF_MULTI_PAGE), ".pdf", true,
            TestName = "Cover page converted to PDF",
            ExpectedResult = true)]
        [TestCase(
            new[] { nameof(_TIF_SINGLE_PAGE), nameof(_PDF_MULTI_PAGE) },
            new[] { _FIRST_PAGE, _ALL_EXCEPT_FIRST_PAGE },
            nameof(_PDF_MULTI_PAGE), ".tif", true,
            TestName = "Replace cover page multi-source to TIF",
            ExpectedResult = true)]
        [TestCase(
            new[] { nameof(_TIF_MULTI_PAGE), nameof(_PDF_MULTI_PAGE), nameof(_TIF_50_PAGE) },
            new[] { _ALL_EXCEPT_FIRST_PAGE, _ALL_EXCEPT_FIRST_PAGE, _ALL_EXCEPT_FIRST_PAGE },
            nameof(_PDF_MULTI_PAGE), ".tif", true,
            TestName = "Remove cover pages and combine to new tif",
            ExpectedResult = true)]
        [TestCase(
            new[] { nameof(_TIF_MULTI_PAGE), nameof(_TIF_MULTI_PAGE) },
            new[] { _ALL_EXCEPT_FIRST_PAGE, _FIRST_PAGE },
            nameof(_TIF_MULTI_PAGE), ".pdf", true,
            TestName = "Move first to last in new pdf",
            ExpectedResult = true)]
        [TestCase(
            new[] { nameof(_PDF_MULTI_PAGE), nameof(_PDF_MULTI_PAGE), nameof(_PDF_MULTI_PAGE), nameof(_PDF_MULTI_PAGE) },
            new[] { _FIRST_TWO_PAGES, _FIRST_AND_LAST_PAGE, _LAST_3_PAGES, _SECOND_AND_SECOND_TO_LAST_PAGE },
            nameof(_PDF_MULTI_PAGE), ".tif", true,
            TestName = "PDF page ranges variations to new tif",
            ExpectedResult = true)]
        [TestCase(
            new[] { nameof(_TIF_SINGLE_PAGE), nameof(_TIF_MULTI_PAGE), nameof(_TIF_MULTI_PAGE) },
            new[] { _FIRST_PAGE, _ALL_PAGES, _FIRST_AND_LAST_PAGE },
            nameof(_TIF_MULTI_PAGE), ".tif", true,
            TestName = "Duplicate pages to new tif",
            ExpectedResult = true)]
        [TestCase(
            new[] { nameof(_TIF_SINGLE_PAGE), nameof(_TIF_MULTI_PAGE), nameof(_TIF_MULTI_PAGE)},
            new[] { _FIRST_PAGE, _ALL_PAGES, _ALL_PAGES },
            nameof(_TIF_MULTI_PAGE), null, true,
            TestName = "Failure: Duplicate SourceDocName pages",
            ExpectedResult = false)]
        [TestCase(
            new[] { nameof(_TIF_SINGLE_PAGE), nameof(_TIF_MULTI_PAGE) },
            new[] { _ALL_EXCEPT_FIRST_PAGE, _ALL_PAGES, },
            nameof(_TIF_MULTI_PAGE), ".tif", true,
            TestName = "Failure: Bad page range",
            ExpectedResult = false)]
        public static bool CombinePagesTask(
            string[] sourceFiles,
            string[] pageSpecs,
            string sourceDocName,
            string newOutputFile,
            bool dataExists)
        {
            string testDbName = _testDbManager.GenerateDatabaseName();
            TestCaseData testCaseData = new ();

            try
            {
                var fileProcessingDb = _testDbManager.GetDatabase(_FAMDB, testDbName);
                var taskConfig = new CombinePagesTask();

                // CombinePagesTask initialize at first with PagesSourceFiles as readable labels
                for (int i = 0; i < sourceFiles.Length; i++)
                {
                    string sourceFile = sourceFiles[i];
                    taskConfig.PageSources.Add(new(sourceFile, pageSpecs[i]));
                }

                // Deploy test files from resources referenced in PageSources
                // Queue sourceDocName to the database
                // Populate testCaseData for output validation after task execution.
                foreach (var pageSource in taskConfig.PageSources)
                {
                    bool isSourceFile = (sourceDocName == pageSource.Document);
                    string sourceFileResource = SourceFiles[pageSource.Document].resourceName;
                    var pageNums = ExpandPageRange(pageSource.Document, pageSource.Pages);

                    foreach (var pageNum in pageNums)
                    {
                        testCaseData.DestToSourcePageMapping[++testCaseData.DestMaxPage] = isSourceFile
                            ? pageNum
                            : null;
                    }

                    if (isSourceFile)
                    {
                        // uss/voa data will only be updated if updating SourceDocName in place.
                        if (newOutputFile == null)
                        {
                            testCaseData.DataExists = dataExists;

                            string ussFileName = _testFiles.GetFile(sourceFileResource + ".uss");
                            string voaFileName = _testFiles.GetFile(sourceFileResource + ".voa");

                            if (dataExists)
                            {
                                testCaseData.SourceUss = new();
                                testCaseData.SourceUss.LoadFrom(ussFileName, false);

                                testCaseData.SourceAttributesByPage = GetAttributeInfoByPage(voaFileName);
                            }
                            else
                            {
                                // ensure uss/voa data don't exist from previous tests
                                File.Delete(ussFileName);
                                File.Delete(voaFileName);
                            }
                        }
                        else
                        {
                            testCaseData.DataExists = false;
                        }

                        int fileId = int.Parse(fileProcessingDb.AddTestFiles(_testFiles,
                            (sourceFileResource, _ACTION_NAME, -1, EActionStatus.kActionPending, EFilePriority.kPriorityNormal))[1]);
                        pageSource.Document = fileProcessingDb.GetFileNameFromFileID(fileId);
                        taskConfig.OutputPath = pageSource.Document;
                        sourceDocName = null;
                    }
                    else
                    {
                        pageSource.Document = _testFiles.GetFile(sourceFileResource);
                    }

                    testCaseData.SourceImages.AddRange(GetImages(pageSource.Document, pageNums));
                }

                if (newOutputFile != null)
                {
                    taskConfig.OutputPath = FileSystemMethods.GetTemporaryFileName(newOutputFile);
                    File.Delete(taskConfig.OutputPath);
                }

                // Execute task
                var task = (IFileProcessingTask)taskConfig;
                using (var famSession = new FAMProcessingSession(fileProcessingDb, _ACTION_NAME, "", task))
                {
                    famSession.WaitForProcessingToComplete();
                }

                if (fileProcessingDb.GetStatsAllWorkflows(_ACTION_NAME, true).NumDocumentsComplete == 0)
                {
                    return false;
                }

                ValidateOutput(taskConfig, testCaseData);

                return true;
            }
            finally
            {
                if (testCaseData.SourceImages != null)
                {
                    CollectionMethods.ClearAndDispose(testCaseData.SourceImages);
                }
                _testDbManager.RemoveDatabase(testDbName);
            }
        }

        #endregion Unit Tests

        #region Private Members

        // Data used to validate test results
        class TestCaseData
        {
            public List<Bitmap> SourceImages { get; set; } = new();
            public Dictionary<int, int?> DestToSourcePageMapping { get; set; } = new();
            public int DestMaxPage { get; set; }
            public bool DataExists { get; set; }
            public SpatialString SourceUss { get; set; } = null;
            public Dictionary<int, List<(string name, string value, string type)>> SourceAttributesByPage { get; set; } = null;
        }

        static Dictionary<int, List<(string name, string value, string type)>> 
            GetAttributeInfoByPage(string dataFileName)
        {
            Dictionary<int, List<(string name, string value, string type)>> attributesByPage = new();

            IUnknownVector sourceVoa = new();
            sourceVoa.LoadFrom(dataFileName, false);

            foreach (var attribute in sourceVoa
                .ToIEnumerable<IAttribute>()
                .SelectMany(attribute => attribute.EnumerateDepthFirst()))
            {
                if (attribute.Value.HasSpatialInfo())
                {
                    int pageNum = attribute.Value.GetFirstPageNumber();

                    var pageAttributes = attributesByPage.GetOrAdd(
                        pageNum, _ => new List<(string name, string value, string type)>());
                    pageAttributes.Add((attribute.Name, attribute.Value.String, attribute.Type));
                }
            }

            return attributesByPage;
        }

        static Bitmap[] GetImages(string documentPath, IEnumerable<int> pageNums = null)
        {
            using IMG document = LoadDocument(documentPath);

            var images = document switch
            {
                TIF tif => ImageUtils.GetPagesAsImages(tif, pageNums),
                PDF pdf => ImageUtils.GetPagesAsImages(pdf, pageNums),
                _ => throw new ExtractException("ELI53966", "Unexpected image type")
            };

            return images;
        }

        static IEnumerable<int> ExpandPageRange(string sourceFile, string pageSpec)
        {
            int pageCount = SourceFiles[sourceFile].pageCount;
            IEnumerable<int> pages = pageSpec switch
            {
                _ when pageSpec == _ALL_PAGES =>
                    Enumerable.Range(1, pageCount),
                _ when pageSpec == _FIRST_PAGE =>
                    Enumerable.Range(1, 1),
                _ when pageSpec == _ALL_EXCEPT_FIRST_PAGE =>
                    Enumerable.Range(2, pageCount - 1),
                _ when pageSpec == _FIRST_TWO_PAGES =>
                    Enumerable.Range(1, 2),
                _ when pageSpec == _LAST_3_PAGES =>
                    Enumerable.Range(pageCount - 2, 3),
                _ when pageSpec == _FIRST_AND_LAST_PAGE =>
                    new int[] { 1, pageCount },
                _ when pageSpec == _SECOND_AND_SECOND_TO_LAST_PAGE =>
                    new int[] { 2, pageCount -1 },
                _ => throw new AssertionException("Unit test logic error in ExpandPageRange")
            };

            return pages;
        }

        static IMG LoadDocument(string documentPath)
        {
            return documentPath switch
            {
                _ when documentPath.EndsWith(".tif", System.StringComparison.OrdinalIgnoreCase) =>
                    new TIF(documentPath),
                _ when documentPath.EndsWith(".pdf", System.StringComparison.OrdinalIgnoreCase) =>
                    new PDF(documentPath),
                _ => throw new AssertionException("Unit test logic error in LoadDocument")
            };
        }

        static void ValidateOutput(CombinePagesTask taskConfig, TestCaseData testCaseData)
        {
            Bitmap[] destImages = null;

            try
            {
                Assert.IsTrue(File.Exists(taskConfig.OutputPath));
                Assert.AreEqual(testCaseData.DataExists, File.Exists(taskConfig.OutputPath + ".uss"));
                Assert.AreEqual(testCaseData.DataExists, File.Exists(taskConfig.OutputPath + ".voa"));
                using var outputDocument = LoadDocument(taskConfig.OutputPath);
                destImages = GetImages(taskConfig.OutputPath);

                // https://extract.atlassian.net/browse/ISSUE-13887
                // ImageMethods.StaplePagesAsNewDocument may alter the pixel format / color depth
                // of input images. This prevents being able to compare that the source images
                // ended up in the correct position in the output document by comparing pixels.
                // For now, at least compare that the expected number of images can be read from
                // the output document.
                Assert.AreEqual(testCaseData.DestMaxPage, destImages.Length);
                //double errors = ImageUtils.ComparePagesAsImages(sourceImages, destImages);
                //Assert.That(errors, Is.LessThan(0.1));

                if (testCaseData.DataExists)
                {
                    SpatialString outputUss = new();
                    outputUss.LoadFrom(taskConfig.OutputPath + ".uss", false);
                    var destAttributesByPage = GetAttributeInfoByPage(taskConfig.OutputPath + ".voa");

                    for (int destPageNum = 1; destPageNum < testCaseData.DestMaxPage; destPageNum++)
                    {
                        if (testCaseData.DestToSourcePageMapping[destPageNum].HasValue)
                        {
                            var sourcePageNum = testCaseData.DestToSourcePageMapping[destPageNum].Value;
                            var sourceString = testCaseData.SourceUss.GetSpecifiedPages(sourcePageNum, sourcePageNum).String;
                            var destString = outputUss.GetSpecifiedPages(destPageNum, destPageNum).String;

                            Assert.AreEqual(sourceString.Trim(), destString.Trim());

                            if (destAttributesByPage.ContainsKey(destPageNum))
                            {
                                Assert.True(
                                    testCaseData.SourceAttributesByPage[sourcePageNum].SequenceEqual(
                                    destAttributesByPage[destPageNum]));
                            }
                        }
                    }
                }
            }
            finally
            {
                if (destImages != null)
                {
                    destImages.ToList().ForEach(image => image.Dispose());
                }
            }
        }

        #endregion Private Members
    }
}
