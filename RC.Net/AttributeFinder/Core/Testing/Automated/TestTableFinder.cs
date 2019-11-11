using Extract.DataCaptureStats;
using Extract.ETL;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using org.apache.pdfbox.pdmodel;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

using SpatialString = UCLID_RASTERANDOCRMGMTLib.SpatialString;


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
            "Resources.Tabula.getTableCellsWithFeatures.rsd",
            "Resources.Tabula.TableScripts.fsx",
        };

        static string[] _testFilePaths;

        /// <summary>
        /// Manages the test files used by this test
        /// </summary>
        static TestFileManager<TestTableFinder> _testFiles;

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
            foreach (var inputFile in _testFilePaths.Where(x => Regex.IsMatch(x, @"\d.pdf$")))
            {
                var expectedFile = inputFile + ".byPage.byColumn.voa";
                var expectedAttributes = new IUnknownVectorClass();
                expectedAttributes.LoadFrom(expectedFile, false);
                var tables = TabulaUtils.GetTablesAsOneAttributePerPage(inputFile, byRow: false, pdfFile: inputFile);

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
            foreach (var inputFile in _testFilePaths.Where(x => Regex.IsMatch(x, @"\d.pdf$")))
            {
                var expectedFile = inputFile + ".byPage.byRow.voa";
                var expectedAttributes = new IUnknownVectorClass();
                expectedAttributes.LoadFrom(expectedFile, false);
                var tables = TabulaUtils.GetTablesAsOneAttributePerPage(inputFile, byRow: true, pdfFile: inputFile);

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
            foreach (var inputFile in _testFilePaths.Where(x => x.EndsWith(".tif", StringComparison.OrdinalIgnoreCase)))
            {
                var expectedFile = inputFile + ".byPage.byColumn.voa";
                var expectedAttributes = new IUnknownVectorClass();
                expectedAttributes.LoadFrom(expectedFile, false);
                var tables = TabulaUtils.GetTablesAsOneAttributePerPage(inputFile, byRow: false);

                var expectedText = ConcatAttributeText(expectedAttributes);
                var foundText = ConcatAttributeText(tables);
                Assert.AreEqual(expectedText, foundText);

                var spatialResults = GetComparisonResultsSpatial(expectedAttributes, tables);
                Assert.AreEqual("Expected||1||Found||1||Correct||1||", spatialResults);
            }
        }

        /// <summary>
        /// Test getting spatial string version of tables from image-based PDFs, column first ordering of cells
        /// </summary>
        [Test]
        public static void TablesAsOneAttributePerPageFromImageBasedPdfByColumn()
        {
            foreach (var inputFile in _testFilePaths.Where(x => x.EndsWith(".tif.pdf", StringComparison.OrdinalIgnoreCase)))
            {
                var expectedFile = inputFile + ".byPage.byColumn.voa";
                var expectedAttributes = new IUnknownVectorClass();
                expectedAttributes.LoadFrom(expectedFile, false);
                var tables = TabulaUtils.GetTablesAsOneAttributePerPage(inputFile, byRow: false);

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
            var expectedFile = inputFile + ".byPage.byColumn.voa";
            var expectedAttributes = new IUnknownVectorClass();
            expectedAttributes.LoadFrom(expectedFile, false);
            var table = TabulaUtils.GetTablesAsSpatialString(inputFile, 1, byRow: false);

            var expectedText = ConcatAttributeText(expectedAttributes);
            var foundText = table.String;
            Assert.AreEqual(expectedText, foundText);
        }


        /// <summary>
        /// Test using GetTableCellsAsSpatialStrings from a script for use with attribute classification
        /// </summary>
        [Test]
        public static void TablesCellsWithFeatures()
        {
            var inputFile = _testFiles.GetFile("Resources.Tabula.us-005.pdf");
            var expectedFile = inputFile + ".protofeatures.voa";
            var expectedAttributes = new IUnknownVectorClass();
            expectedAttributes.LoadFrom(expectedFile, false);

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
