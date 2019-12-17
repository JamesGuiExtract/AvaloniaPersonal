using Extract.AttributeFinder.Tabula;
using Extract.DataCaptureStats;
using Extract.ETL;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;
using OCRParam = Extract.Utilities.Union<(int key, int value), (int key, double value), (string key, int value), (string key, double value), (string key, string value)>;


namespace Extract.AttributeFinder.Test
{
    /// <summary>
    /// Provides test cases for the <see cref="TestTableFinder"/> class.
    /// </summary>
    [TestFixture]
    [Category("TableFinder")]
    public class TestTableFinder
    {
        #region Fields

        static readonly string[] _testFileResourceNames = new string[]
        {
            "Resources.Tabula.us-005.pdf",
            "Resources.Tabula.us-005.pdf.byPage.byColumn.voa",
            "Resources.Tabula.us-005.pdf.byPage.byRow.voa",
            "Resources.Tabula.us-005.pdf.protofeatures.voa",
            "Resources.Tabula.us-005.pdf.tif",
            "Resources.Tabula.us-005.pdf.tif.byPage.byColumn.voa",
            "Resources.Tabula.us-005.pdf.tif.pdf",
            "Resources.Tabula.us-005.pdf.tif.pdf.byPage.byColumn.voa",
            "Resources.Tabula.us-005.pdf.uss",
            "Resources.Tabula.us-005.pdf.tif.uss",
            "Resources.Tabula.us-005.pdf.tif.pdf.uss",
            "Resources.Tabula.us-006.pdf",
            "Resources.Tabula.us-006.pdf.byPage.byColumn.voa",
            "Resources.Tabula.us-006.pdf.byPage.byRow.voa",
            "Resources.Tabula.us-006.pdf.tif",
            "Resources.Tabula.us-006.pdf.tif.byPage.byColumn.voa",
            "Resources.Tabula.us-006.pdf.tif.pdf",
            "Resources.Tabula.us-006.pdf.tif.pdf.byPage.byColumn.voa",
            "Resources.Tabula.us-006.pdf.uss",
            "Resources.Tabula.us-006.pdf.tif.uss",
            "Resources.Tabula.us-006.pdf.tif.pdf.uss",
            "Resources.Tabula.MultilineTable.tif",
            "Resources.Tabula.MultilineTable.tif.uss",
            "Resources.Tabula.MultilineTable.tif.byPage.byRow.voa",
            "Resources.Tabula.MultilineTable.tif.byPage.byColumn.voa",
            "Resources.Tabula.getTableCellsWithFeatures.rsd",
            "Resources.Tabula.TableScripts.fsx",
        };

        static readonly string[] _testFileResourceNames_ISSUE_16821 = new string[]
        {
            // Test that low DPI image processes without error
            // https://extract.atlassian.net/browse/ISSUE-16821
            "Resources.Tabula.us-006.lowDPI.tif",
            "Resources.Tabula.us-006.lowDPI.tif.uss",
            "Resources.Tabula.us-006.lowDPI.tif.byPage.byColumn.voa",

            // Test that really large image processes without error
            // https://extract.atlassian.net/browse/ISSUE-16821
            "Resources.Tabula.us-006.hugePage.tif",
            "Resources.Tabula.us-006.hugePage.tif.uss",
            "Resources.Tabula.us-006.hugePage.tif.byPage.byColumn.voa",
        };

        static string[] _testFilePaths;
        static string[] _testFilePaths_ISSUE_16821;

        /// <summary>
        /// Manages the test files used by this test
        /// </summary>
        static TestFileManager<TestTableFinder> _testFiles;

        static readonly ThreadLocal<AFUtility> _afutil = new ThreadLocal<AFUtility>(() => new AFUtilityClass());

        #endregion Fields

        #region Setup and Teardown
        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testFiles = new TestFileManager<TestTableFinder>();
            _testFilePaths = _testFileResourceNames.Select(name => _testFiles.GetFile(name)).ToArray();
            _testFilePaths_ISSUE_16821 = _testFileResourceNames_ISSUE_16821
                .Select(name => _testFiles.GetFile(name)).ToArray();
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
        /// Test getting spatial string version of tables from text-based, column first ordering of cells
        /// </summary>
        [Test]
        public static void TablesAsOneAttributePerPageFromNativelyDigitalPdfByColumn()
        {
            using (var tabulaUtility = new TabulaUtility<SpatialString>(new TabulaTableFinderV1(), new TablesToSpatialString(byRow: false)))
                foreach (var inputFile in _testFilePaths.Where(x => Regex.IsMatch(x, @"\d.pdf$")))
                {
                    var expectedAttributes = _afutil.Value.GetAttributesFromFile(inputFile + ".byPage.byColumn.voa");
                    var tables = tabulaUtility
                        .GetTablesOnSpecifiedPages(inputFile, pdfFile: inputFile)
                        .GetTablesAsOneAttributePerPage();

                    var expectedText = ConcatAttributeText(expectedAttributes);
                    var foundText = ConcatAttributeText(tables);
                    Assert.AreEqual(expectedText, foundText);

                    var spatialResults = GetComparisonResultsSpatial(expectedAttributes, tables);
                    Assert.AreEqual("Expected||1||Found||1||Correct||1||", spatialResults);
                }
        }

        /// <summary>
        /// Test getting spatial string version of tables from text-based PDFs, row first ordering of cells
        /// </summary>
        [Test]
        public static void TablesAsOneAttributePerPageFromNativelyDigitalPdfByRow()
        {
            using (var tabulaUtility = TabulaUtils.CreateTabulaUtility(new TablesToSpatialString(byRow: true)))
                foreach (var inputFile in _testFilePaths.Where(x => Regex.IsMatch(x, @"\d.pdf$")))
                {
                    var expectedAttributes = _afutil.Value.GetAttributesFromFile(inputFile + ".byPage.byRow.voa");
                    var tables = tabulaUtility
                        .GetTablesOnSpecifiedPages(inputFile, pdfFile: inputFile)
                        .GetTablesAsOneAttributePerPage();

                    var expectedText = ConcatAttributeText(expectedAttributes);
                    var foundText = ConcatAttributeText(tables);
                    Assert.AreEqual(expectedText, foundText);

                    var spatialResults = GetComparisonResultsSpatial(expectedAttributes, tables);
                    Assert.AreEqual("Expected||1||Found||1||Correct||1||", spatialResults);
                }
        }

        /// <summary>
        /// Test getting spatial string version of tables from TIFs, column first ordering of cells
        /// </summary>
        [Test]
        public static void TablesAsOneAttributePerPageFromTifImagesByColumn()
        {
            using (var tabulaUtility = TabulaUtils.CreateTabulaUtility(new TablesToSpatialString(byRow: false)))
                foreach (var inputFile in _testFilePaths
                    .Where(x => x.EndsWith(".tif", StringComparison.OrdinalIgnoreCase))
                    .Where(x => _testFilePaths.Any(y => y.Equals(x + ".byPage.byColumn.voa", StringComparison.OrdinalIgnoreCase))))
                {
                    var expectedAttributes = _afutil.Value.GetAttributesFromFile(inputFile + ".byPage.byColumn.voa");
                    var tables = tabulaUtility
                        .GetTablesOnSpecifiedPages(inputFile)
                        .GetTablesAsOneAttributePerPage();

                    var expectedText = ConcatAttributeText(expectedAttributes);
                    var foundText = ConcatAttributeText(tables);
                    Assert.AreEqual(expectedText, foundText);

                    var spatialResults = GetComparisonResultsSpatial(expectedAttributes, tables);
                    Assert.AreEqual("Expected||1||Found||1||Correct||1||", spatialResults);
                }
        }

        /// <summary>
        /// Test getting spatial string version of tables from TIFs, row first ordering of cells
        /// </summary>
        [Test]
        public static void TablesAsOneAttributePerPageFromTifImagesByRow()
        {
            using (var tabulaUtility = TabulaUtils.CreateTabulaUtility(new TablesToSpatialString(byRow: true)))
                foreach (var inputFile in _testFilePaths
                    .Where(x => x.EndsWith(".tif", StringComparison.OrdinalIgnoreCase))
                    .Where(x => _testFilePaths.Any(y => y.Equals(x + ".byPage.byRow.voa", StringComparison.OrdinalIgnoreCase))))
                {
                    var expectedAttributes = _afutil.Value.GetAttributesFromFile(inputFile + ".byPage.byRow.voa");
                    var tables = tabulaUtility
                        .GetTablesOnSpecifiedPages(inputFile)
                        .GetTablesAsOneAttributePerPage();

                    var expectedText = ConcatAttributeText(expectedAttributes);
                    var foundText = ConcatAttributeText(tables);
                    Assert.AreEqual(expectedText, foundText);

                    var spatialResults = GetComparisonResultsSpatial(expectedAttributes, tables);
                    Assert.AreEqual("Expected||1||Found||1||Correct||1||", spatialResults);
                }
        }

        /// <summary>
        /// Test that very large page processes without error
        /// </summary>
        [Test]
        public static void HugePage()
        {
            using (var tabulaUtility = TabulaUtils.CreateTabulaUtility(new TablesToSpatialString(byRow: false)))
                foreach (var inputFile in _testFilePaths_ISSUE_16821
                    .Where(x => x.EndsWith(".hugePage.tif", StringComparison.OrdinalIgnoreCase)))
                {
                    var expectedAttributes = _afutil.Value.GetAttributesFromFile(inputFile + ".byPage.byColumn.voa");
                    var tables = tabulaUtility
                        .GetTablesOnSpecifiedPages(inputFile)
                        .GetTablesAsOneAttributePerPage();

                    var expectedText = ConcatAttributeText(expectedAttributes);
                    var foundText = ConcatAttributeText(tables);
                    Assert.AreEqual(expectedText, foundText);

                    var spatialResults = GetComparisonResultsSpatial(expectedAttributes, tables);
                    Assert.AreEqual("Expected||1||Found||1||Correct||1||", spatialResults);
                }
        }

        /// <summary>
        /// Test that OCRParameters can be overridden
        /// </summary>
        [Test]
        public static void OverrideDefaultOCRParameters()
        {
            foreach (var inputFile in _testFilePaths_ISSUE_16821
                .Where(x => x.EndsWith(".hugePage.tif", StringComparison.OrdinalIgnoreCase)))
            {
                // Override TabulaUtils default max image size with Nuance defaults
                var ocrParams = new List<OCRParam>
                {
                    new OCRParam(("Kernel.Img.Max.Pix.X", 8400)),
                    new OCRParam(("Kernel.Img.Max.Pix.Y", 8400)),
                }
                .ToOCRParameters();
                var paramsContainer = (IHasOCRParameters)new AFDocumentClass();
                paramsContainer.OCRParameters = ocrParams;
                using (var tmpFile = new TemporaryFile(false))
                {
                    Assert.Throws<ExtractException>(() =>
                        TabulaUtils.CreateTextPdf(inputFile, tmpFile.FileName, paramsContainer));
                }

                // Sanity check: Using larger max size works
                ocrParams = new List<OCRParam>
                {
                    new OCRParam(("Kernel.Img.Max.Pix.X", 32000)),
                    new OCRParam(("Kernel.Img.Max.Pix.Y", 32000)),
                }
                .ToOCRParameters();
                paramsContainer.OCRParameters = ocrParams;
                using (var tmpFile = new TemporaryFile(false))
                {
                    TabulaUtils.CreateTextPdf(inputFile, tmpFile.FileName, paramsContainer);
                }
            }
        }

        /// <summary>
        /// Test that low DPI image processes without error
        /// </summary>
        [Test]
        public static void LowDpi()
        {
            using (var tabulaUtility = TabulaUtils.CreateTabulaUtility(new TablesToSpatialString(byRow: false)))
                foreach (var inputFile in _testFilePaths_ISSUE_16821
                    .Where(x => x.EndsWith(".lowDPI.tif", StringComparison.OrdinalIgnoreCase)))
                {
                    var expectedAttributes = _afutil.Value.GetAttributesFromFile(inputFile + ".byPage.byColumn.voa");
                    var tables = tabulaUtility
                        .GetTablesOnSpecifiedPages(inputFile)
                        .GetTablesAsOneAttributePerPage();

                    var expectedText = ConcatAttributeText(expectedAttributes);
                    var foundText = ConcatAttributeText(tables);
                    Assert.AreEqual(expectedText, foundText);

                    // No tables are actually found
                    var spatialResults = GetComparisonResultsSpatial(expectedAttributes, tables);
                    Assert.AreEqual("Expected||0||", spatialResults);
                }
        }

        /// <summary>
        /// Test getting spatial string version of tables from image-based PDFs, column first ordering of cells
        /// </summary>
        [Test]
        public static void TablesAsOneAttributePerPageFromImageBasedPdfByColumn()
        {
            using (var tabulaUtility = TabulaUtils.CreateTabulaUtility(new TablesToSpatialString(byRow: false)))
                foreach (var inputFile in _testFilePaths.Where(x => x.EndsWith(".tif.pdf", StringComparison.OrdinalIgnoreCase)))
                {
                    var expectedAttributes = _afutil.Value.GetAttributesFromFile(inputFile + ".byPage.byColumn.voa");
                    var tables = tabulaUtility
                        .GetTablesOnSpecifiedPages(inputFile)
                        .GetTablesAsOneAttributePerPage();

                    var expectedText = ConcatAttributeText(expectedAttributes);
                    var foundText = ConcatAttributeText(tables);
                    Assert.AreEqual(expectedText, foundText);

                    var spatialResults = GetComparisonResultsSpatial(expectedAttributes, tables);
                    Assert.AreEqual("Expected||1||Found||1||Correct||1||", spatialResults);
                }
        }

        /// <summary>
        /// Test getting spatial string version of tables from one page of a PDF
        /// </summary>
        [Test]
        public static void TablesFromSinglePage()
        {
            var inputFile = _testFiles.GetFile("Resources.Tabula.us-005.pdf");
            var expectedAttributes = _afutil.Value.GetAttributesFromFile(inputFile + ".byPage.byColumn.voa");
            using (var tabulaUtility = TabulaUtils.CreateTabulaUtility(new TablesToSpatialString(byRow: false)))
            {
                var table = tabulaUtility
                    .GetTablesOnSpecifiedPages(inputFile, pageNumbers: new[] { 1 }, pdfFile: inputFile)
                    .SelectMany(x => x)
                    .GetTablesAsSpatialString();

                var expectedText = ConcatAttributeText(expectedAttributes);
                var foundText = table.String;
                Assert.AreEqual(expectedText, foundText);
            }
        }

        /// <summary>
        /// Test that there is an exception when getting tables from non-existent page
        /// </summary>
        [Test]
        public static void TablesFromMissingPage()
        {
            var inputFile = _testFiles.GetFile("Resources.Tabula.us-005.pdf");
            using (var tabulaUtility = TabulaUtils.CreateTabulaUtility(new TablesToSpatialString(byRow: false)))
                Assert.Throws<ExtractException>(() => tabulaUtility.GetTablesOnSpecifiedPages(inputFile, pageNumbers: new[] { 100 }));
        }

        /// <summary>
        /// Test using GetTableCellsAsSpatialStrings from a script for use with attribute classification
        /// </summary>
        [Test]
        public static void TablesCellsWithFeatures()
        {
            var inputFile = _testFiles.GetFile("Resources.Tabula.us-005.pdf");
            var expectedAttributes = _afutil.Value.GetAttributesFromFile(inputFile + ".protofeatures.voa");

            var inputSpatialStringFile = _testFiles.GetFile("Resources.Tabula.us-005.pdf.uss");
            var doc = new AFDocumentClass();
            doc.Text.LoadFrom(inputSpatialStringFile, false);

            var rulesFile = _testFiles.GetFile("Resources.Tabula.getTableCellsWithFeatures.rsd");
            var rules = new RuleSetClass();
            rules.LoadFrom(rulesFile, false);

            // Ensure script name isn't mangled
            var baseDir = Path.GetDirectoryName(rulesFile);
            _testFiles.GetFile("Resources.Tabula.TableScripts.fsx", baseDir + @"\TableScripts.fsx");

            var foundAttributes = rules.ExecuteRulesOnText(doc, null, null, null);

            var results = GetComparisonResultsText(expectedAttributes, foundAttributes);
            var expected =
                "Expected|Cell|10||"
                + "Expected|Cell/CellClass@Feature|10||"
                + "Expected|Cell/RowHeader@Feature+Tokenize|5||"
                + "Expected|Cell/PrevCellInRowClass@Feature|5||"
                + "Expected|Cell/ColHeader@Feature+Tokenize|8||"
                + "Expected|Cell/PrevCellInColClass@Feature|8||"
                + "Expected|Cell/StubHeader@Feature+Tokenize|4||"
                + "Correct|Cell|10||"
                + "Correct|Cell/RowHeader@Feature+Tokenize|5||"
                + "Correct|Cell/ColHeader@Feature+Tokenize|8||"
                + "Correct|Cell/StubHeader@Feature+Tokenize|4||"
                + "Correct|Cell/CellClass@Feature|10||"
                + "Correct|Cell/PrevCellInRowClass@Feature|5||"
                + "Correct|Cell/PrevCellInColClass@Feature|8||";

            Assert.AreEqual(expected, results);
        }

        #endregion Public Test Functions

        #region Helper Methods

        private static object ConcatAttributeText(IUnknownVector attributes)
        {
            return string.Join("\r\n\r\n",
                attributes
                .ToIEnumerable<IAttribute>()
                .Select(a => a.Value.String));
        }

        private static string GetComparisonResultsSpatial(IUnknownVector expectedAttributes, IUnknownVector foundAttributes)
        {
            return String.Join("", IDShieldAttributeComparer
                .CompareAttributes(expectedAttributes, foundAttributes, "/*/*[text()]", CancellationToken.None)
                .SelectMany(pair => pair.Value)
                .AggregateStatistics()
                .Select(x => x.ToString()));
        }

        private static string GetComparisonResultsText(IUnknownVector expectedAttributes, IUnknownVector foundAttributes)
        {
            return String.Join("", AttributeTreeComparer
                .CompareAttributes(expectedAttributes, foundAttributes)
                .AggregateStatistics()
                .Select(x => x.ToString()));
        }

        #endregion Helper Methods
    }
}
