using Extract.DataCaptureStats;
using Extract.ETL;
using Extract.Imaging.Forms;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using UCLID_AFCORELib;
using UCLID_AFOUTPUTHANDLERSLib;
using UCLID_AFVALUEFINDERSLib;
using UCLID_AFVALUEMODIFIERSLib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;


namespace Extract.AttributeFinder.Test
{
    /// <summary>
    /// Test cases for spatial string methods
    /// work correctly
    /// </summary>
    [TestFixture]
    [Category("Automated")]
    [Category("RichTextAttributes")]
    public class TestRichTextAttributes
    {
        #region Constants

        const string _EXAMPLE01_RTF_FILE = "Resources.Example01.rtf";

        const string _ALL_THREE_CORRECT =
          "Expected||3||" +
          "Found||3||" +
          "Correct||3||";

        const string _ALL_THREE_UNDERREDACTED =
          "Expected||3||" +
          "Found||3||" +
          "UnderRedacted||3||" +
          "Missed||3||";

        #endregion

        #region Fields

        /// <summary>
        /// Manages the test files used by this test
        /// </summary>
        static TestFileManager<TestRichTextAttributes> _testFiles;

        #endregion Fields

        #region Setup and Teardown
        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testFiles = new TestFileManager<TestRichTextAttributes>();
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

        #region Tests

        /// <summary>
        /// Sanity test: ensure that comparing clones to originals works correctly
        /// </summary>
        [Test]        
        public static void CompareClones()
        {
            string rtfPath = _testFiles.GetFile(_EXAMPLE01_RTF_FILE);

            SpatialString inputText = new SpatialStringClass();
            inputText.LoadFrom(rtfPath, false);
            var doc = new AFDocumentClass { Text = inputText };
            var rule = new RegExprRuleClass { Pattern = @"\b\d{3}-\d{2}-\d{4}\b" };
            var foundAttributes = rule.ParseText(doc, null);
            SetAttributeName(foundAttributes, "Jo");
            var clones = GetClones(foundAttributes);

            string results = GetComparisonResults(clones, foundAttributes);

            Assert.AreEqual(_ALL_THREE_CORRECT, results);
        }

        /// <summary>
        /// Compare selection from viewer with rules
        /// </summary>
        [Test]
        public static void CompareUserRedactionWithRules()
        {
            string rtfPath = _testFiles.GetFile(_EXAMPLE01_RTF_FILE);
            string ssnPattern = @"\b\d{3}-\d{2}-\d{4}\b";

            SpatialString inputText = new SpatialStringClass();
            inputText.LoadFrom(rtfPath, false);
            var doc = new AFDocumentClass { Text = inputText };
            var rule = new RegExprRuleClass { Pattern = ssnPattern };
            var foundAttributes = rule.ParseText(doc, null);
            SetAttributeName(foundAttributes, "Jo");

            RichTextViewer richTextViewer = new RichTextViewer();
            richTextViewer.OpenImage(rtfPath, false);
            var matches = Regex.Matches(richTextViewer.Text, ssnPattern);
            foreach (Match match in matches)
            {
                richTextViewer.Select(match.Index, match.Length);
                richTextViewer.CreateRedaction();
            }
            IUnknownVector expectedAttributes = GetRedactionsFromViewer(richTextViewer, inputText);

            string results = GetComparisonResults(expectedAttributes, foundAttributes);

            Assert.AreEqual(_ALL_THREE_CORRECT, results);
        }

        /// <summary>
        /// Compare overlapping selections from viewer with rules
        /// </summary>
        [Test]
        public static void CompareUserOverlappingRedactionWithRules()
        {
            string rtfPath = _testFiles.GetFile(_EXAMPLE01_RTF_FILE);
            string ssnPattern = @"\b\d{3}-\d{2}-\d{4}\b";

            SpatialString inputText = new SpatialStringClass();
            inputText.LoadFrom(rtfPath, false);
            var doc = new AFDocumentClass { Text = inputText };
            var rule = new RegExprRuleClass { Pattern = ssnPattern };
            var foundAttributes = rule.ParseText(doc, null);
            SetAttributeName(foundAttributes, "Jo");

            RichTextViewer richTextViewer = new RichTextViewer();
            richTextViewer.OpenImage(rtfPath, false);
            var matches = Regex.Matches(richTextViewer.Text, ssnPattern);
            foreach (Match match in matches)
            {
                richTextViewer.Select(match.Index, match.Length * 2 / 3);
                Assert.That(Regex.IsMatch(richTextViewer.SelectedText, @"\A\d{3}-\d{2}-\z"), "This should be the first 2/3 of an SSN");

                richTextViewer.CreateRedaction();

                // Add an overlapping region
                var redaction = richTextViewer.LayerObjects.OfType<Redaction>().Last();
                redaction.Selected = true;
                richTextViewer.Select(match.Index + match.Length / 3 + 1, match.Length * 2 / 3);
                Assert.That(Regex.IsMatch(richTextViewer.SelectedText, @"\A\d{2}-\d{4}\z"), "This should be the last 2/3 of an SSN");
                
                richTextViewer.AppendToRedaction();
                redaction.Selected = false;
            }
            IUnknownVector expectedAttributes = GetRedactionsFromViewer(richTextViewer, inputText);
            Assert.That(
                expectedAttributes
                .ToIEnumerable<IAttribute>()
                .All(attribute => GetOCRImageRasterZones(attribute).Count() > 1),
                "There should be multiple zones for all of these");

            string results = GetComparisonResults(expectedAttributes, foundAttributes);

            Assert.AreEqual(_ALL_THREE_CORRECT, results);
        }


        /// <summary>
        /// Ensure that converting to hybrid doesn't change results
        /// </summary>
        [Test]        
        public static void CompareSpatialModeToHybridMode()
        {
            string rtfPath = _testFiles.GetFile(_EXAMPLE01_RTF_FILE);

            SpatialString inputText = new SpatialStringClass();
            inputText.LoadFrom(rtfPath, false);
            var doc = new AFDocumentClass { Text = inputText };
            var rule = new RegExprRuleClass { Pattern = @"\b\d{3}-\d{2}-\d{4}\b" };
            var foundAttributes = rule.ParseText(doc, null);

            SetAttributeName(foundAttributes, "Jo");
            var hybridClones = GetHybridClones(foundAttributes);

            string results = GetComparisonResults(hybridClones, foundAttributes);

            Assert.AreEqual(_ALL_THREE_CORRECT, results);
        }

        /// <summary>
        /// Verify that OCR raster zones = OriginalImage raster zones
        /// Text files have no rotation or skew so these should always be the same.
        /// </summary>
        [Test]        
        public static void CompareOCRZonesToOriginalZones()
        {
            string rtfPath = _testFiles.GetFile(_EXAMPLE01_RTF_FILE);

            SpatialString inputText = new SpatialStringClass();
            inputText.LoadFrom(rtfPath, false);
            var doc = new AFDocumentClass { Text = inputText };
            var rule = new RegExprRuleClass { Pattern = @"\b\d{3}-\d{2}-\d{4}\b" };
            var foundAttributes = rule.ParseText(doc, null);

            var ocrZones = GetOCRImageRasterZonesForComparing(foundAttributes);
            var originalZones = GetOriginalImageRasterZonesForComparing(foundAttributes);
            Assert.That(ocrZones.Zip(originalZones, (ocr, orig) => ocr.IsEqualTo(orig)).All(x => x));
        }

        /// <summary>
        /// Verify that one zone per letter compares properly to larger zones
        /// This also tests that the get zones methods divide on gaps (e.g., where the SSN is divided by control words)
        /// </summary>
        [Test]        
        public static void CompareManyZonesToOne()
        {
            string rtfPath = _testFiles.GetFile(_EXAMPLE01_RTF_FILE);

            SpatialString inputText = new SpatialStringClass();
            inputText.LoadFrom(rtfPath, false);
            var doc = new AFDocumentClass { Text = inputText };
            var rule = new RegExprRuleClass { Pattern = @"\b\d{3}-\d{2}-\d{4}\b" };
            var foundAttributes = rule.ParseText(doc, null);

            // Verify that at least one attribute has non-sequential letters (spans some special word codes)
            Assert.That(foundAttributes.ToIEnumerable<IAttribute>().Any(attribute => HasNonSequentialLetters(attribute)));

            // Verify that this produces multiple zones (else the area of the non-sequential-letter attribute will considered too large)
            Assert.That(foundAttributes
                .ToIEnumerable<IAttribute>()
                .Any(attribute => GetOCRImageRasterZones(attribute).Count() > 1),
                "There should be multiple zones for at least one of these");

            SetAttributeName(foundAttributes, "Jo");

            var multipleZones = GetAsOneZonePerLetter(foundAttributes);

            string results = GetComparisonResults(multipleZones, foundAttributes);

            Assert.AreEqual(_ALL_THREE_CORRECT, results);
        }

        /// <summary>
        /// Verify that one zone per letter compares properly to larger zones even after doing spatial string replace
        /// This tests that multi-zone attributes are still multi-zone after a replace that alters the end of zone character
        /// </summary>
        [Test]        
        public static void CompareManyZonesToOneAfterReplace()
        {
            string rtfPath = _testFiles.GetFile(_EXAMPLE01_RTF_FILE);

            SpatialString inputText = new SpatialStringClass();
            inputText.LoadFrom(rtfPath, false);
            var doc = new AFDocumentClass { Text = inputText };
            var rule = new RegExprRuleClass { Pattern = @"\b\d{3}-\d{2}-\d{4}\b" };
            var foundAttributes = rule.ParseText(doc, null);
            SetAttributeName(foundAttributes, "Jo");

            // Verify that at least one attribute has non-sequential letters (spans some special word codes)
            Assert.That(foundAttributes.ToIEnumerable<IAttribute>().Any(attribute => HasNonSequentialLetters(attribute)));

            // Verify that this produces multiple zones (else the area of the non-sequential-letter attribute will considered too large)
            Assert.That(foundAttributes
                .ToIEnumerable<IAttribute>()
                .Any(attribute => GetOCRImageRasterZones(attribute).Count() > 1),
                "There should be multiple zones for at least one of these");

            // Change stuff around
            var modifier = new AdvancedReplaceStringClass { StrToBeReplaced = @"\d{2}-(\d{4})", Replacement = "$1", AsRegularExpression = true };
            foreach (var attribute in foundAttributes.ToIEnumerable<IAttribute>())
            {
                modifier.ModifyValue((UCLID_AFCORELib.Attribute)attribute, doc, null);
            }
            // Verify that the previous conditions still hold
            Assert.That(foundAttributes.ToIEnumerable<IAttribute>().Any(attribute => HasNonSequentialLetters(attribute)));
            Assert.That(foundAttributes
                .ToIEnumerable<IAttribute>()
                .Any(attribute => GetOCRImageRasterZones(attribute).Count() > 1),
                "There should be multiple zones for at least one of these");

            var multipleZones = GetAsOneZonePerLetter(foundAttributes);

            string results = GetComparisonResults(multipleZones, foundAttributes);

            Assert.AreEqual(_ALL_THREE_CORRECT, results);
        }

        /// <summary>
        /// Verify that there are multiple zones after building from non-consecutive pieces
        /// Also verify that these are considered under-redactions
        /// </summary>
        [Test]        
        public static void CompareManyZonesToOneAfterRebuild()
        {
            string rtfPath = _testFiles.GetFile(_EXAMPLE01_RTF_FILE);

            SpatialString inputText = new SpatialStringClass();
            inputText.LoadFrom(rtfPath, false);
            var doc = new AFDocumentClass { Text = inputText };
            var rule = new RegExprRuleClass { Pattern = @"\b(?'group1'\d{3})-(?'group2'\d{2})-(?'group3'\d{4})\b", CreateSubAttributesFromNamedMatches = true };
            var foundAttributes = rule.ParseText(doc, null);
            SetAttributeName(foundAttributes, "Jo");

            var clones = GetClones(foundAttributes);

            // Rebuild without middle
            var rebuild = new ModifyAttributeValueOHClass { AttributeQuery = "*", SetAttributeValue = true, AttributeValue = "%group1%-%group3%" };
            rebuild.ProcessOutput(foundAttributes, doc, null);

            // Verify that this produces multiple zones for all
            Assert.That(foundAttributes
                .ToIEnumerable<IAttribute>()
                .All(attribute => GetOCRImageRasterZones(attribute).Count() > 1),
                "There should be multiple zones for all of these");

            string results = GetComparisonResults(clones, foundAttributes);
            Assert.AreEqual(_ALL_THREE_UNDERREDACTED, results);
        }

        #endregion Tests

        #region Helper Methods

        private static string GetComparisonResults(IUnknownVector expectedAttributes, IUnknownVector foundAttributes)
        {
            return String.Join("", IDShieldAttributeComparer
                .CompareAttributes(expectedAttributes, foundAttributes, "/*/*", CancellationToken.None)
                .SelectMany(pair => pair.Value)
                .AggregateStatistics()
                .Select(x => x.ToString()));
        }

        private static bool HasNonSequentialLetters(IAttribute attribute)
        {
            var letters = GetLetters(attribute);
            return letters
                .Skip(1)
                .Where((letter, i) => letters[i].Right < letter.Left - 1) // This is a gap of at least two, assuming Right is exclusive, or 1 if it were inclusive
                .Any();
        }

        private static List<Letter> GetLetters(IAttribute attribute)
        {
            return Enumerable.Range(0, attribute.Value.Size)
                .Select(i => attribute.Value.GetOCRImageLetter(i))
                .ToList();
        }

        private static void SetAttributeName(IUnknownVector attributes, string name)
        {
            foreach (var attribute in attributes.ToIEnumerable<IAttribute>())
            {
                attribute.Name = name;
            }
        }

        private static IUnknownVector GetClones(IUnknownVector attributes)
        {
            return (IUnknownVector)((ICopyableObject)attributes).Clone();
        }

        private static IUnknownVector GetHybridClones(IUnknownVector spatialAttributes)
        {
            var cloned = GetClones(spatialAttributes);
            foreach (var attribute in cloned.ToIEnumerable<IAttribute>())
            {
                attribute.Value.DowngradeToHybridMode();
            }

            Assert.That(spatialAttributes.ToIEnumerable<IAttribute>()
                .All(attribute => attribute.Value.GetMode() == ESpatialStringMode.kSpatialMode));

            Assert.That(cloned.ToIEnumerable<IAttribute>()
                .All(attribute => attribute.Value.GetMode() == ESpatialStringMode.kHybridMode));

            return cloned;
        }

        private static IUnknownVector GetAsOneZonePerLetter(IUnknownVector spatialAttributes)
        {
            var cloned = GetClones(spatialAttributes);
            foreach (var attribute in cloned.ToIEnumerable<IAttribute>())
            {
                var letters = GetLetters(attribute);
                var zoneForHeight = GetOCRImageRasterZones(attribute).First();
                var zones = letters
                    .Select(letter => new RasterZoneClass
                    {
                        StartX = letter.Left,
                        EndX = letter.Right,
                        StartY = zoneForHeight.StartY,
                        EndY = zoneForHeight.EndY,
                        Height = zoneForHeight.Height,
                        PageNumber = 1
                    })
                    .ToIUnknownVector();

                attribute.Value.CreateHybridString(zones, "PlaceholderText", attribute.Value.SourceDocName, attribute.Value.SpatialPageInfos);
            }
            return cloned;
        }

        private static List<IComparableObject> GetOCRImageRasterZonesForComparing(IUnknownVector attributes)
        {
            return attributes
                .ToIEnumerable<IAttribute>()
                .SelectMany(attribute => GetOCRImageRasterZones(attribute).Cast<IComparableObject>())
                .ToList();
        }

        private static IEnumerable<IRasterZone> GetOCRImageRasterZones(IAttribute attribute)
        {
            return attribute
                    .Value
                    .GetOCRImageRasterZones()
                    .ToIEnumerable<IRasterZone>();
        }

        private static List<IComparableObject> GetOriginalImageRasterZonesForComparing(IUnknownVector attributes)
        {
            return attributes
                .ToIEnumerable<IAttribute>()
                .SelectMany(attribute => GetOriginalImageRasterZones(attribute).Cast<IComparableObject>())
                .ToList();
        }

        private static IEnumerable<IRasterZone> GetOriginalImageRasterZones(IAttribute attribute)
        {
            return attribute
                    .Value
                    .GetOriginalImageRasterZones()
                    .ToIEnumerable<IRasterZone>();
        }

        private static IUnknownVector GetRedactionsFromViewer(RichTextViewer richTextViewer, SpatialString source)
        {
            return richTextViewer
                .LayerObjects
                .OfType<Redaction>()
                .Select(redaction =>
                {
                    var rasterZones = redaction
                        .GetRasterZones()
                        .Select(x => x.ToComRasterZone())
                        .ToIUnknownVector();
                    var value = new SpatialString();
                    value.CreateHybridString(rasterZones, redaction.Comment, "Unknown", source.SpatialPageInfos);
                    return new AttributeClass { Name = "Qi", Value = value };
                })
                .ToIUnknownVector();
        }

        #endregion Helper Methods
    }
}
