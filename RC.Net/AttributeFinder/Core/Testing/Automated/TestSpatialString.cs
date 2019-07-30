using Extract.Testing.Utilities;
using NUnit.Framework;
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
        static TestFileManager<TestSpatialPageInfo> _testFiles;

        const string _EXAMPLE05_TIF_FILE = "Resources.Example05.tif";
        const string _BLANK_PAGE_GCV_FILE = "Resources.BlankPage.tif.gcv.uss";

        #endregion Fields

        #region Setup and Teardown
        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testFiles = new TestFileManager<TestSpatialPageInfo>();
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

        #endregion Public Test Functions
    }
}
