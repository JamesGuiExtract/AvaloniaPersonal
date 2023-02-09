using Extract.Testing.Utilities;
using Microsoft.VisualBasic.FileIO;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UCLID_AFUTILSLib;
using UCLID_COMUTILSLib;

namespace Extract.DataCaptureStats.Test
{
    /// <summary>
    /// Unit tests for StatisticsSummarizer class
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("StatisticsSummarizer")]
    public class TestStatisticsSummarizer
    {
        #region Fields

        /// <summary>
        /// Manages the test data files
        /// </summary>
        static TestFileManager<TestStatisticsSummarizer> _testFiles;

        static AFUtility _afUtility;
        static IUnknownVector _expected;
        static IUnknownVector _found;

        #endregion Fields

        #region Overhead

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestStatisticsSummarizer>();
            _afUtility = new AFUtility();
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
            }
        }

        #endregion Overhead

        #region Tests

        // Helper function to create files for testing
        private static void SetFiles(string foundName, string expectedName)
        {
            _expected = _afUtility.GetAttributesFromFile(_testFiles.GetFile(expectedName));
            _found = _afUtility.GetAttributesFromFile(_testFiles.GetFile(foundName));
        }

        // Test first batch of cases from ISSUE-5373, out of order test cases
        [Test, Category("StatisticsSummarizer")]
        public static void TestBatch1FromIssue5373()
        {
            var expectedResult = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test (Summary)", 2674),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test (Summary)", 2194),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test (Summary)", 161),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "(Summary)", 2674),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "(Summary)", 2194),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "(Summary)", 161),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component", 448),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component", 414),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component", 12),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Flag", 114),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Flag", 99),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Flag", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range", 394),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range", 318),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range", 30),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range/Max", 394),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range/Max", 318),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range/Max", 30),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range/Min", 394),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range/Min", 318),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range/Min", 30),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Units", 418),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Units", 295),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Units", 18),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Value", 448),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Value", 385),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Value", 31),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Date", 32),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Date", 20),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Date", 8),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Name", 32),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Name", 27),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Name", 1),
            };

            var setFilesActions = new Action[]
            {
                () => SetFiles("Resources.OoO_Test1.1.found.eav", "Resources.OoO.1.expected.eav"),
                () => SetFiles("Resources.OoO_Test2.1.found.eav", "Resources.OoO.1.expected.eav"),
                () => SetFiles("Resources.OoO_Test3.1.found.eav", "Resources.OoO.1.expected.eav"),
                () => SetFiles("Resources.OoO_Test4.1.found.eav", "Resources.OoO.1.expected.eav"),
                () => SetFiles("Resources.OoO_Test5.1.found.eav", "Resources.OoO.1.expected.eav"),
                () => SetFiles("Resources.OoO_Test6.1.found.eav", "Resources.OoO.1.expected.eav"),
                () => SetFiles("Resources.OoO_Test7.2.found.eav", "Resources.OoO.2.expected.eav"),
                () => SetFiles("Resources.OoO_Test8.2.found.eav", "Resources.OoO.2.expected.eav"),
            };

            var perFileResults = setFilesActions.SelectMany(a =>
            {
                a();
                return AttributeTreeComparer.CompareAttributes(_expected, _found);
            });

            var result = perFileResults
                .AggregateStatistics()
                .SummarizeStatistics()
                .ToArray();

            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        // Test second batch of cases from ISSUE-5373, out of order test cases
        [Test, Category("StatisticsSummarizer")]
        public static void TestBatch2FromIssue5373()
        {
            var expectedResult = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test (Summary)", 224),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test (Summary)", 171),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test (Summary)", 17),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "(Summary)", 224),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "(Summary)", 171),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "(Summary)", 17),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component", 32),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component", 28),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Flag", 8),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Flag", 7),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range", 32),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range", 24),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range", 4),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range/Max", 32),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range/Max", 24),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range/Max", 4),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range/Min", 32),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range/Min", 24),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range/Min", 4),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Units", 24),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Units", 22),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Value", 32),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Value", 17),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Value", 4),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Date", 16),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Date", 10),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Name", 16),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Name", 15),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Name", 1),
            };

            var setFilesActions = new Action[]
            {
                () => SetFiles("Resources.OoO_Test9.3.found.eav", "Resources.OoO.3.expected.eav"),
                () => SetFiles("Resources.OoO_Test10.3.found.eav", "Resources.OoO.3.expected.eav"),
                () => SetFiles("Resources.OoO_Test11.3.found.eav", "Resources.OoO.3.expected.eav"),
                () => SetFiles("Resources.OoO_Test12.3.found.eav", "Resources.OoO.3.expected.eav"),
            };

            var perFileResults = setFilesActions.SelectMany(a =>
            {
                a();
                return AttributeTreeComparer.CompareAttributes(_expected, _found);
            });

            var result = perFileResults
                .AggregateStatistics()
                .SummarizeStatistics()
                .ToArray();

            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        // Non-container-only and container-only result in an exception
        // Ensure the exception contains useful information about the conflict (i.e., the paths)
        [Test, Category("StatisticsSummarizer")]
        public static void TestContainerXPath1()
        {
            SetFiles("Resources.TestContainerXPath1.found.eav", "Resources.TestContainerXPath1.expected.eav");

            var perFileResults = new List<AccuracyDetail>();
            var containerXPath = "/*/Test";
            perFileResults.AddRange(AttributeTreeComparer.CompareAttributes(_expected, _found, containerXPath: containerXPath));

            containerXPath = "";
            perFileResults.AddRange(AttributeTreeComparer.CompareAttributes(_expected, _found, containerXPath: containerXPath));

            var result = perFileResults.AggregateStatistics();

            var ex = Assert.Throws<ExtractException>(() => result.SummarizeStatistics());

            // Statistics Reporter: Helpful container conflict exception info is hidden
            // https://extract.atlassian.net/browse/ISSUE-14408_
            Assert.That(ex.Data.Contains("Conflicting paths"));
            Assert.That((new List<string>()).GetType().IsSerializable);
            Assert.AreEqual("Test", ex.Data["Conflicting paths"].AsString());
        }

        // Non-container-only and container-only result in asterisks
        [Test, Category("StatisticsSummarizer")]
        public static void TestContainerXPath2()
        {
            var expectedResult = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test (Summary) *", 4),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test (Summary) *", 3),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test (Summary) *", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "(Summary)", 4),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "(Summary)", 3),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "(Summary)", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Test", 4),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Test", 3),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Test", 1),
            };

            SetFiles("Resources.TestContainerXPath1.found.eav", "Resources.TestContainerXPath1.expected.eav");

            var perFileResults = new List<AccuracyDetail>();
            var containerXPath = "/*/Test";
            perFileResults.AddRange(AttributeTreeComparer.CompareAttributes(_expected, _found, containerXPath: containerXPath));

            containerXPath = "";
            perFileResults.AddRange(AttributeTreeComparer.CompareAttributes(_expected, _found, containerXPath: containerXPath));

            var result = perFileResults
                .AggregateStatistics()
                .SummarizeStatistics(throwIfContainerOnlyConflict: false)
                .ToArray();

            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        // Test Html formatting
        [Test, Category("StatisticsSummarizer")]
        public static void TestHtmlFormatting()
        {
            var expectedPrefix = @"<table class=""DataCaptureStats"">";

            SetFiles("Resources.MultipleTopLevel.found.eav", "Resources.MultipleTopLevel.expected.eav");
            var result = AttributeTreeComparer.CompareAttributes(_expected, _found,
                AttributeTreeComparer.DefaultIgnoreXPath + " | /*/LabInfo | /*/ResultStatus | /*/Test/LabInfo | /*/Test/EpicCode | /*/Test/Component/OriginalName",
                AttributeTreeComparer.DefaultContainerXPath + " | /*/PatientInfo/Name | /*/PhysicianInfo/*")
                .AggregateStatistics()
                .SummarizeStatistics();
            var group = new GroupStatistics(1, new string[0], new string[0], result);
            var report = group.AccuracyDetailsToHtml();
            Assert.AreEqual(expectedPrefix, report.Substring(0, expectedPrefix.Length));
        }

        /// <summary>
        /// Attribute paths are to be treated in a way that ignores case.
        /// If there are path-label pairs in the input that differ only by
        /// case then that should result in an exception (this would indicate a bug
        /// with the <see cref="StatisticsAggregator"/>)
        /// </summary>
        [Test, Category("StatisticsAggregator")]
        public static void TestPathsOfDifferentCaseForSameLabel()
        {
            var input = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.Expected, "PONumber", 2),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "POnumber", 4),
            };

            Assert.Throws<ExtractException>(() => StatisticsSummarizer.SummarizeStatistics(input));
        }

        /// <summary>
        /// Attribute paths should to be treated in a way that ignores case.
        /// </summary>
        [Test, Category("StatisticsAggregator")]
        public static void TestPathsOfDifferentCaseForDifferentLabel()
        {
            // Arrange
            var input = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.Expected, "PONumber", 2),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "POnumber", 1),
            };

            var expectedOutput = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.Expected, "(Summary)", 2),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "(Summary)", 1),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "(Summary)", 0),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "PONumber", 2),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "POnumber", 1),
            };

            // Act
            List<AccuracyDetail> actualOutput = StatisticsSummarizer.SummarizeStatistics(input).ToList();

            // Assert
            CollectionAssert.AreEquivalent(expectedOutput, actualOutput);
        }

        /// <summary>
        /// Attribute paths should to be treated in a way that ignores case,
        /// including when making a CSV report
        /// </summary>
        [Test, Category("StatisticsAggregator")]
        public static void TestPathsOfDifferentCaseForDifferentLabel_CSV()
        {
            // Arrange
            var input = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.Expected, "PONumber", 2),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "POnumber", 1),
            };

            var expectedHeader = new string[]
            {
                "File count",
                "(Summary).Expected",
                "(Summary).Correct",
                "(Summary).Missing",
                "(Summary).Incorrect",
                "(Summary).% Correct (Recall)",
                "(Summary).Precision",
                "(Summary).F1-Score",
                "(Summary).ROCE (C/I)",
                "(Summary).EiF1 [2E/(1+EF)]",
                "PONumber.Expected",
                "PONumber.Correct",
                "PONumber.Missing",
                "PONumber.Incorrect",
                "PONumber.% Correct (Recall)",
                "PONumber.Precision",
                "PONumber.F1-Score",
                "PONumber.ROCE (C/I)",
                "PONumber.EiF1 [2E/(1+EF)]"
            };

            var expectedValues = new string[]
            {
                "1",
                "2",
                "1",
                "1",
                "0",
                "50.00 %",
                "100.00 %",
                "0.6667",
                "NaN",
                "1.71",
                "2",
                "1",
                "1",
                "0",
                "50.00 %",
                "100.00 %",
                "0.6667",
                "NaN",
                "1.71",
            };

            // Act
            GroupStatistics group = new(
                fileCount: 1,
                groupByNames: Array.Empty<string>(),
                groupByValues: Array.Empty<string>(),
                accuracyDetails: StatisticsSummarizer.SummarizeStatistics(input).ToList());

            string csv = StatisticsSummarizer.AccuracyDetailsToCsv(new[] { group });

            // Assert
            using var csvReader = new TextFieldParser(new MemoryStream(Encoding.Default.GetBytes(csv)));
            csvReader.SetDelimiters(",");
            var header = csvReader.ReadFields();
            var values = csvReader.ReadFields();

            Assert.Multiple(() =>
            {
                CollectionAssert.AreEqual(expectedHeader, header);
                CollectionAssert.AreEqual(expectedValues, values);
            });
        }

        /// <summary>
        /// Attribute paths should to be treated in a way that ignores case,
        /// including when making an HTML table
        /// </summary>
        [Test, Category("StatisticsAggregator")]
        public static void TestPathsOfDifferentCaseForDifferentLabel_Html()
        {
            // Arrange
            var input = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.Expected, "PONumber", 2),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "POnumber", 1),
            };

            string expectedTable = @"<table class=""DataCaptureStats"">
	<caption>

	</caption><thead>
		<tr>
			<th>Path</th><th>Expected</th><th>Correct</th><th>Missing</th><th>Incorrect</th><th>% Correct (Recall)</th><th>Precision</th><th>F1-Score</th><th>ROCE (C/I)</th><th>EiF1 [2E/(1+EF)]</th>
		</tr>
	</thead><tfoot>
		<tr>
			<th>File count</th><td>1</td>
		</tr>
	</tfoot><tr>
		<th>(Summary)</th><td>2</td><td>1</td><td>1</td><td>0</td><td>50.00 %</td><td>100.00 %</td><td>0.6667</td><td>NaN</td><td>1.71</td>
	</tr><tr>
		<th>PONumber</th><td>2</td><td>1</td><td>1</td><td>0</td><td>50.00 %</td><td>100.00 %</td><td>0.6667</td><td>NaN</td><td>1.71</td>
	</tr>
</table>";

            // Act
            GroupStatistics group = new(
                fileCount: 1,
                groupByNames: Array.Empty<string>(),
                groupByValues: Array.Empty<string>(),
                accuracyDetails: StatisticsSummarizer.SummarizeStatistics(input).ToList());
            string table = StatisticsSummarizer.AccuracyDetailsToHtml(group);

            // Assert
            Assert.AreEqual(expectedTable, table);
        }

        #endregion Tests
    }
}