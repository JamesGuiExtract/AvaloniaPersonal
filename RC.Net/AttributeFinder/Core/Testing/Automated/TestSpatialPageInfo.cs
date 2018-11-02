using Extract.Imaging;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using UCLID_AFCORELib;
using UCLID_RASTERANDOCRMGMTLib;


namespace Extract.AttributeFinder.Test
{
    /// <summary>
    /// Test cases for checking that spatial page info rotation and merging operations (e.g., from pagination)
    /// work correctly
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("SpatialPageInfo")]
    public class TestSpatialPageInfo
    {
        #region Fields
        /// <summary>
        /// Manages the test files used by this test
        /// </summary>
        static TestFileManager<TestSpatialPageInfo> _testFiles;
        static SynchronousOcrManager _ocrManager;

        const string _EXAMPLE05_TIF_FILE = "Resources.Example05.tif";
        const string _EXAMPLE05_USS_FILE = "Resources.Example05.tif.uss";
        const string _EXAMPLE05_WITH_ROTATED_PAGE_TIF_FILE = "Resources.Example05_WithRotatedPage.tif";
        const string _EXAMPLE05_WITH_ROTATED_PAGE_USS_FILE = "Resources.Example05_WithRotatedPage.tif.uss";

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
            _ocrManager = new SynchronousOcrManager();
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
        /// Checks that a swipe of text that is oriented differently than the primary orientation
        /// of the page is persisted properly
        /// https://extract.atlassian.net/browse/ISSUE-15330
        /// </summary>
        [Test, Category("SpatialPageInfo")]        
        public static void NonPrimaryOrientation()
        {
            string imagePath = _testFiles.GetFile(_EXAMPLE05_TIF_FILE);
            string ussPath = _testFiles.GetFile(_EXAMPLE05_USS_FILE);
            var swipe = new Imaging.RasterZone
            {
                PageNumber = 2,
                StartX = 2203,
                StartY = 1855,
                EndX = 2198,
                EndY = 2241,
                Height = 30
            };
            var val = _ocrManager.GetOcrText(imagePath, swipe, 0.2);
            Assert.AreEqual("TERESA ROSE MARTINEZ", val.String);
            Assert.AreEqual(EOrientation.kRotLeft, val.GetPageInfo(2).Orientation);

            var attr = new AttributeClass
            {
                Name = "Swipe",
                Value = val
            };
            var attrr = new[] { attr }.ToIUnknownVector();

            var primaryString = new SpatialStringClass();
            primaryString.LoadFrom(ussPath, false);
            Assert.AreEqual(EOrientation.kRotNone, primaryString.GetPageInfo(2).Orientation);

            var pageMap = new Dictionary<Tuple<string, int>, List<int>>
            {
                { Tuple.Create(imagePath, 1), new List<int> { 1 } },
                { Tuple.Create(imagePath, 2), new List<int> { 2 } },
                { Tuple.Create(imagePath, 3), new List<int> { 3 } }
            };
            AttributeMethods.TranslateAttributesToNewDocument(
                attrr,
                "Dummy.tif",
                pageMap,
                primaryString.SpatialPageInfos);

            // Confirm that the attribute was modified
            Assert.AreEqual("Dummy.tif", attr.Value.SourceDocName);

            // Confirm that the orientation was left as it was
            Assert.AreEqual(EOrientation.kRotLeft, attr.Value.GetPageInfo(2).Orientation);

            // Check OCRing with the modified attribute
            var zone = (UCLID_RASTERANDOCRMGMTLib.RasterZone)attr.Value.GetOriginalImageRasterZones().At(0);
            var newSwipe = new Imaging.RasterZone(zone);
            var newVal = _ocrManager.GetOcrText(imagePath, newSwipe, 0.2);
            Assert.AreEqual(val.String, newVal.String);
        }

        /// <summary>
        /// Checks that a swipe of text that is oriented differently than the primary orientation
        /// of the page is persisted properly when the page is rotated to match
        /// </summary>
        [Test, Category("SpatialPageInfo")]        
        public static void NonPrimaryOrientationRotated()
        {
            string newImagePath = null;
            try
            {
                string imagePath = _testFiles.GetFile(_EXAMPLE05_TIF_FILE);
                string ussPath = _testFiles.GetFile(_EXAMPLE05_USS_FILE);
                var swipe = new Imaging.RasterZone
                {
                    PageNumber = 2,
                    StartX = 2203,
                    StartY = 1855,
                    EndX = 2198,
                    EndY = 2241,
                    Height = 30
                };
                var val = _ocrManager.GetOcrText(imagePath, swipe, 0.2);
                Assert.AreEqual("TERESA ROSE MARTINEZ", val.String);
                Assert.AreEqual(EOrientation.kRotLeft, val.GetPageInfo(2).Orientation);

                var attr = new AttributeClass
                {
                    Name = "Swipe",
                    Value = val
                };
                var attrr = new[] { attr }.ToIUnknownVector();

                var primaryString = new SpatialStringClass();
                primaryString.LoadFrom(ussPath, false);
                Assert.AreEqual(EOrientation.kRotNone, primaryString.GetPageInfo(2).Orientation);

                var pageMap = new Dictionary<Tuple<string, int>, List<int>>
                {
                    { Tuple.Create(imagePath, 1), new List<int> { 1 } },
                    { Tuple.Create(imagePath, 2), new List<int> { 2 } },
                    { Tuple.Create(imagePath, 3), new List<int> { 3 } }
                };

                // Simulate the original page 2 being rotated left
                var rotationInfo = Array.AsReadOnly(new[] { (imagePath, 2, 270) });

                newImagePath = _testFiles.GetFile(_EXAMPLE05_WITH_ROTATED_PAGE_TIF_FILE);
                var newInfoMap = AttributeMethods.CreateUSSForPaginatedDocument(
                    newImagePath,
                    pageMap,
                    rotationInfo);

                // Primary orientation is now Right because that is the rotation needed to correct
                // the majority of text for the page being rotated left
                Assert.AreEqual(EOrientation.kRotRight, ((SpatialPageInfo)newInfoMap.GetValue(2)).Orientation);

                AttributeMethods.TranslateAttributesToNewDocument(
                    attrr,
                    newImagePath,
                    pageMap,
                    rotationInfo,
                    newInfoMap);

                // Confirm that the attribute was modified
                Assert.AreEqual(newImagePath, attr.Value.SourceDocName);

                // Confirm that the orientation is now None because the page has been rotated to
                // match the orientation of the swipe
                Assert.AreEqual(EOrientation.kRotNone, attr.Value.GetPageInfo(2).Orientation);

                // Check OCRing the rotated image with the modified attribute
                var zone = (UCLID_RASTERANDOCRMGMTLib.RasterZone) attr.Value.GetOriginalImageRasterZones().At(0);
                var newSwipe = new Imaging.RasterZone(zone);
                var newVal = _ocrManager.GetOcrText(newImagePath, newSwipe, 0.2);
                Assert.AreEqual(val.String, newVal.String);
            }
            finally
            {
                if (newImagePath != null)
                {
                    File.Delete(newImagePath + ".uss");
                }
            }
        }

        /// <summary>
        /// Checks that TranslateToNewPageInfo works correctly on a Hybrid mode string
        /// </summary>
        [Test, Category("SpatialPageInfo")]        
        public static void TranslateHybridModeToNewOrientation()
        {
            string imagePath = _testFiles.GetFile(_EXAMPLE05_TIF_FILE);
            string ussPath = _testFiles.GetFile(_EXAMPLE05_USS_FILE);
            var swipe = new Imaging.RasterZone
            {
                PageNumber = 2,
                StartX = 2203,
                StartY = 1855,
                EndX = 2198,
                EndY = 2241,
                Height = 30
            };
            var val = _ocrManager.GetOcrText(imagePath, swipe, 0.2);
            Assert.AreEqual(EOrientation.kRotLeft, val.GetPageInfo(2).Orientation);

            var primaryString = new SpatialStringClass();
            primaryString.LoadFrom(ussPath, false);
            Assert.AreEqual(EOrientation.kRotNone, primaryString.GetPageInfo(2).Orientation);

            var zone = (UCLID_RASTERANDOCRMGMTLib.RasterZone)val.GetOriginalImageRasterZones().At(0);

            // Confirm that translating the hybrid version of the string to spatial page info with 0 orientation
            // gets the same zone as getting the original image zones
            val.DowngradeToHybridMode();
            Assert.AreEqual(ESpatialStringMode.kHybridMode, val.GetMode());
            val.TranslateToNewPageInfo(primaryString.SpatialPageInfos);
            var zones = val.GetOCRImageRasterZones();
            Assert.AreEqual(1, zones.Size());

            var translatedZone = (UCLID_RASTERANDOCRMGMTLib.RasterZone)zones.At(0);
            var translatedWidth = translatedZone.EndY - translatedZone.StartY;
            var originalWidth = zone.EndY - zone.StartY;

            // Width of original image zone = width of translated zone
            Assert.AreEqual(originalWidth, translatedWidth);

            // Height of original image zone = height of the translated zone
            Assert.AreEqual(zone.Height, translatedZone.Height);

            // Area of overlap is close to the area of the smallest zone (So they are in the same place.
            // Difference is due to rounding/padding error)
            var minArea = Math.Min(zone.Area, translatedZone.Area);
            var overlap = translatedZone.GetAreaOverlappingWith(zone);
            Assert.AreEqual(minArea, overlap, delta: 1);
        }

        /// <summary>
        /// Checks that TranslateToNewPageInfo works correctly on a Spatial mode string
        /// </summary>
        [Test, Category("SpatialPageInfo")]        
        public static void TranslateSpatialModeToNewOrientation()
        {
            string imagePath = _testFiles.GetFile(_EXAMPLE05_TIF_FILE);
            string ussPath = _testFiles.GetFile(_EXAMPLE05_USS_FILE);
            var swipe = new Imaging.RasterZone
            {
                PageNumber = 2,
                StartX = 2203,
                StartY = 1855,
                EndX = 2198,
                EndY = 2241,
                Height = 30
            };
            var val = _ocrManager.GetOcrText(imagePath, swipe, 0.2);
            Assert.AreEqual(EOrientation.kRotLeft, val.GetPageInfo(2).Orientation);

            var primaryString = new SpatialStringClass();
            primaryString.LoadFrom(ussPath, false);
            Assert.AreEqual(EOrientation.kRotNone, primaryString.GetPageInfo(2).Orientation);

            var zone = (UCLID_RASTERANDOCRMGMTLib.RasterZone)val.GetOriginalImageRasterZones().At(0);

            // Confirm that translating the string to spatial page info with 0 orientation gets a zone
            // with the roughly the same position and area, although defined differently, as getting original image zones.
            // The zone is defined differently because the translation happens at the character level, before computing the zones,
            // and so this zone is taken to represent vertically stacked characters (very tall zone)
            // rather than text running from top to bottom of the page (wide zone)
            Assert.AreEqual(ESpatialStringMode.kSpatialMode, val.GetMode());
            val.TranslateToNewPageInfo(primaryString.SpatialPageInfos);
            var zones = val.GetOCRImageRasterZones();
            Assert.AreEqual(1, zones.Size());

            var translatedZone = (UCLID_RASTERANDOCRMGMTLib.RasterZone)zones.At(0);
            var translatedWidth = translatedZone.EndX - translatedZone.StartX;
            var originalWidth = zone.EndY - zone.StartY;

            // Width of proper orientation = height of translated zone
            Assert.AreEqual(originalWidth, translatedZone.Height);

            // Height of proper orientation is close to the width of the translated zone
            // (difference is due to rounding/padding error)
            Assert.AreEqual(zone.Height, translatedWidth, delta: 4);

            // Area of overlap is close to the area of the smallest zone (So they are in the same place.
            // Difference is due to rounding/padding error)
            var minArea = Math.Min(zone.Area, translatedZone.Area);
            var overlap = translatedZone.GetAreaOverlappingWith(zone);
            Assert.AreEqual(minArea, overlap, delta: 40);
            Assert.Greater(100.0 * overlap / minArea, 99);
        }

        #endregion Public Test Functions
    }
}
