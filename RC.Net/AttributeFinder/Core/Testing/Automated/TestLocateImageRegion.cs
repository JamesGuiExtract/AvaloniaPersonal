using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;
using SpatialString = UCLID_RASTERANDOCRMGMTLib.SpatialString;


namespace Extract.AttributeFinder.Test
{
    /// <summary>
    /// Test cases for LocateImageRegion rule object. This is not meant to be comprehensive at this time but to include
    /// test cases for new features.
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("LocateImageRegion")]
    public class TestLocateImageRegion
    {
        #region Fields
        /// <summary>
        /// Manages the test files used by this test
        /// </summary>
        static TestFileManager<TestLocateImageRegion> _testFiles;

        const string _A418_TIF_FILE = "Resources.A418.tif";
        const string _A418_USS_FILE = "Resources.A418.tif.uss";
        const string _TEST_01_RSD_FILE = "Resources.LIR_Test_01_OneToOne.rsd";
        const string _TEST_02_RSD_FILE = "Resources.LIR_Test_02_MoreLinesThanZones.rsd";
        const string _TEST_03_RSD_FILE = "Resources.LIR_Test_03_MoreZonesThanLines.rsd";
        const string _TEST_04_RSD_FILE = "Resources.LIR_Test_04_MoreZonesThanLines.rsd";
        const string _TEST_05_RSD_FILE = "Resources.LIR_Test_05_One-To-One_ModifiedLines.rsd";
        const string _TEST_06_RSD_FILE = "Resources.LIR_Test_06_MultipleZonesOneChar.rsd";

        #endregion Fields

        #region Setup and Teardown
        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testFiles = new TestFileManager<TestLocateImageRegion>();
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
        /// Tests the behavior of running a locate image region on a hybrid string that has
        /// the same number of lines as raster zones (two of each)
        /// </summary>
        [Test, Category("LocateImageRegion")]        
        public static void Test01()
        {
            var attributes = FindWasHybridAndWasNotHybridWithRuleset(_A418_TIF_FILE, _TEST_01_RSD_FILE);
            var wasHybrid = attributes.Item1;

            SpatialString value = wasHybrid.Value;
            Assert.That(value.GetMode() == ESpatialStringMode.kSpatialMode);

            IUnknownVector zones = value.GetOCRImageRasterZones();
            Assert.AreEqual(2, zones.Size());

            // Check that the value returned by the LIR has zones that correspond to individual lines
            // (is not just a bounding box divided into two)
            var sumOfAreas = zones.ToIEnumerable<RasterZone>().Sum(z => z.Area);
            var boundingZone = new RasterZone();
            var boundingBox = boundingZone.GetBoundsFromMultipleRasterZones(zones, (SpatialPageInfo) value.SpatialPageInfos.GetValue(1));
            boundingZone.CreateFromLongRectangle(boundingBox, 1);
            Assert.Greater(boundingZone.Area, sumOfAreas * 2);

            // Check to see that each non-whitespace char is spatial
            for (int i = 0; i < value.Size; ++i)
            {
                var subString = value.GetSubString(i, i);
                Assert.That(string.IsNullOrWhiteSpace(subString.String) || subString.HasSpatialInfo());
            }
        }

        /// <summary>
        /// Tests the behavior of running a locate image region on a hybrid string that has
        /// one more line than raster zones (three lines and two zones)
        /// </summary>
        [Test, Category("LocateImageRegion")]        
        public static void Test02()
        {
            var attributes = FindWasHybridAndWasNotHybridWithRuleset(_A418_TIF_FILE, _TEST_02_RSD_FILE);
            var wasHybrid = attributes.Item1;

            SpatialString value = wasHybrid.Value;
            Assert.That(value.GetMode() == ESpatialStringMode.kSpatialMode);

            // GetOCRImageRasterZones makes one zone per line but the last line in this string is non-spatial
            // so there will still be only two zones
            IUnknownVector zones = value.GetOCRImageRasterZones();
            Assert.AreEqual(2, zones.Size());

            // Check that the value returned by the LIR has zones that correspond to individual lines
            // (is not just a bounding box divided into three)
            var sumOfAreas = zones.ToIEnumerable<RasterZone>().Sum(z => z.Area);
            var boundingZone = new RasterZone();
            var boundingBox = boundingZone.GetBoundsFromMultipleRasterZones(zones, (SpatialPageInfo) value.SpatialPageInfos.GetValue(1));
            boundingZone.CreateFromLongRectangle(boundingBox, 1);
            Assert.Greater(boundingZone.Area, sumOfAreas * 2);

            // The last two chars, the extra line, have been added without spatial information
            var subString = value.GetSubString(value.Size - 3, value.Size - 1);
            Assert.That(!string.IsNullOrWhiteSpace(subString.String) && !subString.HasSpatialInfo());

            // Check to see that every other non-whitespace char is spatial
            for (int i = 0; i < value.Size - 2; ++i)
            {
                subString = value.GetSubString(i, i);
                Assert.That(string.IsNullOrWhiteSpace(subString.String) || subString.HasSpatialInfo());
            }
        }

        /// <summary>
        /// Tests the behavior of running a locate image region on a hybrid string that has
        /// more raster zones than lines (the first line removed after conversion to hybrid)
        /// In this case the string will be padded with \r\n^ so that the second raster zone
        /// can be associated with a line
        /// </summary>
        [Test, Category("LocateImageRegion")]        
        public static void Test03()
        {
            var attributes = FindWasHybridAndWasNotHybridWithRuleset(_A418_TIF_FILE, _TEST_03_RSD_FILE);
            var wasHybrid = attributes.Item1;
            var wasNotHybrid = attributes.Item2;

            SpatialString value = wasHybrid.Value;
            Assert.That(value.GetMode() == ESpatialStringMode.kSpatialMode);

            IUnknownVector wasHybridZones = value.GetOCRImageRasterZones();
            Assert.AreEqual(2, wasHybridZones.Size());

            // Check that the zones returned by the LIR operating on a hybrid string completely
            // cover the zones from the never-hybrid result
            for (int i = 0; i < wasHybridZones.Size(); ++i)
            {
                var wasHybridZone = (RasterZone) wasHybridZones.At(i);
                var wasNotHybridZone = (RasterZone) wasNotHybrid.Value.GetOCRImageRasterZones().At(i);
                Assert.AreEqual(wasHybridZone.Area, wasHybridZone.GetAreaOverlappingWith(wasNotHybridZone));
            }

            // Check to see that each non-whitespace char is spatial
            for (int i = 0; i < value.Size; ++i)
            {
                var subString = value.GetSubString(i, i);
                Assert.That(string.IsNullOrWhiteSpace(subString.String) || subString.HasSpatialInfo());
            }
        }

        /// <summary>
        /// Tests the behavior of running a locate image region on a hybrid string that has
        /// more raster zones than lines (the second line removed after conversion to hybrid)
        /// In this case the two raster zones get the characters of a single word distributed
        /// between them which results in a larger spatial area that encloses the original
        /// two lines.
        /// </summary>
        [Test, Category("LocateImageRegion")]        
        public static void Test04()
        {
            var attributes = FindWasHybridAndWasNotHybridWithRuleset(_A418_TIF_FILE, _TEST_04_RSD_FILE);
            var wasHybrid = attributes.Item1;
            var wasNotHybrid = attributes.Item2;

            SpatialString value = wasHybrid.Value;
            Assert.That(value.GetMode() == ESpatialStringMode.kSpatialMode);

            IUnknownVector wasHybridZones = value.GetOCRImageRasterZones();
            Assert.AreEqual(2, wasHybridZones.Size());

            // Check that the zones returned by the LIR operating on a hybrid string completely
            // cover the zones from the never-hybrid result
            for (int i = 0; i < wasHybridZones.Size(); ++i)
            {
                var wasHybridZone = (RasterZone) wasHybridZones.At(i);
                var wasNotHybridZone = (RasterZone) wasNotHybrid.Value.GetOCRImageRasterZones().At(i);
                Assert.AreEqual(wasHybridZone.Area, wasHybridZone.GetAreaOverlappingWith(wasNotHybridZone));
            }

            // Check to see that each non-whitespace char is spatial
            for (int i = 0; i < value.Size; ++i)
            {
                var subString = value.GetSubString(i, i);
                Assert.That(string.IsNullOrWhiteSpace(subString.String) || subString.HasSpatialInfo());
            }
        }

        /// <summary>
        /// Tests the behavior of running a locate image region on a hybrid string that has
        /// many lines and just as many raster zones. In this test the shorter lines have been
        /// modified to have three times their original number of characters. This won't affect
        /// how the raster zones are assigned to lines, though.
        /// </summary>
        [Test, Category("LocateImageRegion")]        
        public static void Test05()
        {
            var attributes = FindWasHybridAndWasNotHybridWithRuleset(_A418_TIF_FILE, _TEST_05_RSD_FILE);
            var wasHybrid = attributes.Item1;
            var wasNotHybrid = attributes.Item2;

            SpatialString value = wasHybrid.Value;
            Assert.That(value.GetMode() == ESpatialStringMode.kSpatialMode);

            IUnknownVector zones = value.GetOCRImageRasterZones();
            Assert.AreEqual(6, zones.Size());

            // Check that the value returned by the LIR has zones that correspond to individual lines
            // (is not just a bounding box divided into two)
            var sumOfAreas = zones.ToIEnumerable<RasterZone>().Sum(z => z.Area);
            var boundingZone = new RasterZone();
            var boundingBox = boundingZone.GetBoundsFromMultipleRasterZones(zones, (SpatialPageInfo) value.SpatialPageInfos.GetValue(1));
            boundingZone.CreateFromLongRectangle(boundingBox, 1);
            Assert.Greater(boundingZone.Area, sumOfAreas * 2);

            // Check to see that each non-whitespace char is spatial
            for (int i = 0; i < value.Size; ++i)
            {
                var subString = value.GetSubString(i, i);
                Assert.That(string.IsNullOrWhiteSpace(subString.String) || subString.HasSpatialInfo());
            }

            // Check to see if the sum of areas for the wasNotHybrid result is the same
            var sumOfAreas2 = wasNotHybrid.Value.GetOCRImageRasterZones().ToIEnumerable<RasterZone>().Sum(z => z.Area);
            Assert.AreEqual(sumOfAreas, sumOfAreas2);
        }

        /// <summary>
        /// Tests the behavior of running a locate image region on a hybrid string that has
        /// more raster zones than characters (all text replaced with a single char)
        /// In this case the first of the six raster zones get the existing character and
        /// the rest get assigned padding characters
        /// </summary>
        [Test, Category("LocateImageRegion")]        
        public static void Test06()
        {
            var attributes = FindWasHybridAndWasNotHybridWithRuleset(_A418_TIF_FILE, _TEST_06_RSD_FILE);
            var wasHybrid = attributes.Item1;

            SpatialString value = wasHybrid.Value;
            Assert.That(value.GetMode() == ESpatialStringMode.kSpatialMode);

            IUnknownVector zones = value.GetOCRImageRasterZones();
            Assert.AreEqual(6, zones.Size());

            // Check that the value returned by the LIR has been padded to be 6 + 5 * 2 (newlines) = 16 chars
            Assert.AreEqual(16, value.Size);

            // Check to see that each non-whitespace char is spatial
            for (int i = 0; i < value.Size; ++i)
            {
                var subString = value.GetSubString(i, i);
                Assert.That(string.IsNullOrWhiteSpace(subString.String) || subString.HasSpatialInfo());
            }
        }

        /// <summary>
        /// Gets the two values found by a testing ruleset.
        /// </summary>
        /// <param name="sourceDocName">Name of the source document.</param>
        /// <param name="rsdFilename">The RSD filename.</param>
        /// <returns>The two values found running the given ruleset on the given source document</returns>
        static Tuple<IAttribute, IAttribute> FindWasHybridAndWasNotHybridWithRuleset(string sourceDocName, string rsdFilename)
        {
            _testFiles.GetFile(sourceDocName);
            string ussPath = _testFiles.GetFile(sourceDocName + ".uss");
            string rsdPath = _testFiles.GetFile(rsdFilename);

            SpatialString ss = new SpatialString();
            ss.LoadFrom(ussPath, false);

            AFDocument doc = new AFDocument();
            doc.Text = ss;

            RuleSet ruleSet = new RuleSet();
            ruleSet.LoadFrom(rsdPath, false);
            var attributes = ruleSet.ExecuteRulesOnText(doc,
                pvecAttributeNames: null, bstrAlternateComponentDataDir: null, pProgressStatus: null);

            ExtractException.Assert("ELI41625", "Expected to find exactly two attributes", attributes.Size() == 2);

            return Tuple.Create((IAttribute) attributes.At(0), (IAttribute) attributes.At(1));
        }

        #endregion Private Functions

    }
}
