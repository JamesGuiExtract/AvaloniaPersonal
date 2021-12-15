using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;


namespace Extract.AttributeFinder.Test
{
    /// <summary>
    /// Test cases for spatial string methods
    /// work correctly
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("SpatialString")]
    public class TestSpatialString
    {
        #region Fields
        /// <summary>
        /// Manages the test files used by this test
        /// </summary>
        static TestFileManager<TestSpatialString> _testFiles;

        const string _EXAMPLE05_TIF_FILE = "Resources.Example05.tif";
        const string _BLANK_PAGE_IMAGE = "Resources.BlankPage.tif";
        const string _BLANK_PAGE_GCV_FILE = "Resources.BlankPage.tif.gcv.uss";
        const string _REARRANGED_LETTERS_GCV_FILE = "Resources.RearrangedLetters.gcv.uss";

        #endregion Fields

        #region Setup and Teardown
        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testFiles = new TestFileManager<TestSpatialString>();
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
            }
        }

        #endregion Setup and Teardown

        #region Public Test Functions

        /// <summary>
        /// Rules should handle non-spatial strings better
        /// https://extract.atlassian.net/browse/ISSUE-15615
        /// </summary>
        [Test, Category("SpatialString")]        
        public static void GetPagesOnNonemptyNonSpatialString()
        {
            string imagePath = _testFiles.GetFile(_EXAMPLE05_TIF_FILE);
            var ss = new SpatialStringClass();
            ss.CreateNonSpatialString("N/A", imagePath);

            // Confirm that GetPages doesn't throw an exception
            var pages = ss.GetPages(true, " ");

            // Confirm that there are the expected number of pages
            Assert.AreEqual(3, pages.Size());
        }

        /// <summary>
        /// GCV OCR - All Strings Should be Spatial error when processing certain pdf
        /// https://extract.atlassian.net/browse/ISSUE-16436
        /// </summary>
        [Test, Category("SpatialString")]        
        public static void LoadFromArchiveWithBlankPages()
        {
            string ussPath = _testFiles.GetFile(_BLANK_PAGE_GCV_FILE);
            var ss = new SpatialStringClass();

            // Confirm that LoadFrom doesn't throw an exception
            ss.LoadFrom(ussPath, false);

            // Confirm that there are the expected number of pages
            Assert.AreEqual(2, ss.GetPages(false, "").Size());
        }

        /// <summary>
        /// Test that loading pages from file works the same as loading whole file and then getting pages
        /// </summary>
        [Test, Category("SpatialString")]        
        public static void LoadPagesFromFileEmptyPageIncluded()
        {
            // Get the image so that this test works when the original source doc is not available
            // https://extract.atlassian.net/browse/ISSUE-17062
            string sdn = _testFiles.GetFile(_BLANK_PAGE_IMAGE);
            string ussPath = _testFiles.GetFile(_BLANK_PAGE_GCV_FILE, sdn + ".uss");
            var ss = new SpatialStringClass();

            ss.LoadFrom(ussPath, false);
            var pagesFromMem = ss.GetPages(true, "").ToIEnumerable<IComparableObject>().ToList();
            var pagesFromFile = ss.LoadPagesFromFile(ussPath).ToIEnumerable<IComparableObject>().ToList();

            Assert.AreEqual(3, pagesFromMem.Count);
            Assert.AreEqual(3, pagesFromFile.Count);
            for (int i = 0; i < 3; i++)
            {
                Assert.That(pagesFromMem[i].IsEqualTo(pagesFromFile[i]));
            }
        }

        /// <summary>
        /// Test that loading pages from file works the same as loading whole file and then getting pages
        /// Page 2 is in an archive, and get pages filters out that page, therefore we need to also filter out
        /// archived pages in LoadPagesFromFile because it does not filter out archived pages.
        /// </summary>
        [Test, Category("SpatialString")]        
        public static void LoadPagesFromFile()
        {
            string ussPath = _testFiles.GetFile(_BLANK_PAGE_GCV_FILE);
            var ss = new SpatialStringClass();

            ss.LoadFrom(ussPath, false);
            var pagesFromMem = ss.GetPages(false, "")
                .ToIEnumerable<IComparableObject>()
                .ToList();
            var pagesFromFile = ss.LoadPagesFromFile(ussPath)
                .ToIEnumerable<ISpatialString>()
                .Where(page => page.HasSpatialInfo())
                .Cast<IComparableObject>()
                .ToList();
            Assert.AreEqual(2, pagesFromMem.Count);
            Assert.AreEqual(2, pagesFromFile.Count);
            for (int i = 0; i < 2; i++)
            {
                Assert.That(pagesFromMem[i].IsEqualTo(pagesFromFile[i]));
            }
        }

        /// <summary>
        /// Tests that loading from one file simultaneously from many threads doesn't cause any problems
        /// </summary>
        [Test, Category("SpatialString")]        
        public static void LoadSameFileManyTimesAtOnce()
        {
            string ussPath = _testFiles.GetFile(_BLANK_PAGE_GCV_FILE);
            List<(int size, int page)> sizeForEveryAttempt = Enumerable.Range(1, 200)
                .AsParallel()
                .Select(i =>
                {
                    var ss = new SpatialStringClass();
                    var page = i % 2 == 0 ? 1 : 3;
                    ss.LoadPageFromFile(ussPath, page);
                    return (ss.Size, page);
                })
                .ToList();

            var pageOnes = sizeForEveryAttempt.Where(x => x.page == 1).ToList();
            var pageThrees = sizeForEveryAttempt.Where(x => x.page == 3).ToList();

            // Confirm that there are the expected sizes for each attempt
            Assert.AreEqual(1828, pageOnes[0].size);
            Assert.That(pageOnes.TrueForAll(p => p.size == pageOnes[0].size));

            Assert.AreEqual(295, pageThrees[0].size);
            Assert.That(pageThrees.TrueForAll(p => p.size == pageThrees[0].size));
        }

        /// <summary>
        /// CreateFromSpatialStrings creates bad spatial string when given an empty vector
        /// https://extract.atlassian.net/browse/ISSUE-17006
        /// </summary>
        [Test, Category("SpatialString")]
        public static void CreateFromSpatialStringsWithEmptyVector()
        {
            var ss = new SpatialStringClass();
            var pages = new IUnknownVectorClass();
            ss.CreateFromSpatialStrings(pages, false);

            // Confirm that the result is non-spatial
            Assert.AreEqual(ESpatialStringMode.kNonSpatialMode, ss.GetMode());
        }

        /// <summary>
        /// CreateFromSpatialStrings creates bad spatial string when given an empty vector
        /// https://extract.atlassian.net/browse/ISSUE-17558
        /// </summary>
        [Test, Category("SpatialString")]
        public static void GetLinesWorksWithCarriageReturns()
        {
            var ss = new SpatialStringClass();
            ss.CreateNonSpatialString("Line1\r\nLine2\rLine3", "placeholder");

            // Confirm that all three lines are returned by GetLines
            var expectedLines = new[] { "Line1", "Line2", "Line3" };
            var lines = ss.GetLines()
                .ToIEnumerable<SpatialString>()
                .Select(line => line.String)
                .ToArray();
            CollectionAssert.AreEqual(expectedLines, lines);
        }

        // Confirm that average char width works correctly on a string from a problem GCV file
        // https://extract.atlassian.net/browse/ISSUE-17886
        [Test, Category("SpatialString")]
        public static void CheckAverageCharWidth()
        {
            string ussFile = _testFiles.GetFile(_REARRANGED_LETTERS_GCV_FILE);
            var ss = new SpatialStringClass();
            ss.LoadFrom(ussFile, false);
            int avgWidth = ss.GetAverageCharWidth();

            Assert.That(avgWidth, Is.GreaterThan(0));
        }

        #endregion Public Test Functions
    }
}
