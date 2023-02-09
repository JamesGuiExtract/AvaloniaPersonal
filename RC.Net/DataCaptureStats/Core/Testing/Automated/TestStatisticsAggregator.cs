using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UCLID_AFUTILSLib;
using UCLID_COMUTILSLib;

namespace Extract.DataCaptureStats.Test
{
    /// <summary>
    /// Unit tests for StatisticsAggregator class
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("StatisticsAggregator")]
    public class TestStatisticsAggregator
    {
        #region Fields

        /// <summary>
        /// Manages the test data files
        /// </summary>
        static TestFileManager<TestStatisticsAggregator> _testFiles;

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

            _testFiles = new TestFileManager<TestStatisticsAggregator>();
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

        // Helper function to build file lists for pagination testing
        // These images are stapled together from Demo_LabDE images
        private static void SetFiles(string foundName, string expectedName)
        {
            _expected = _afUtility.GetAttributesFromFile(_testFiles.GetFile(expectedName));
            _found = _afUtility.GetAttributesFromFile(_testFiles.GetFile(foundName));
        }

        // Test first batch of cases from ISSUE-5373, out of order test cases
        [Test, Category("StatisticsAggregator")]
        public static void TestBatch1FromIssue5373()
        {
            var expectedResult = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.ContainerOnly, "Test", 0),
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

            var result = StatisticsAggregator.AggregateStatistics(perFileResults).ToArray();

            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        // Test second batch of cases from ISSUE-5373, out of order test cases
        [Test, Category("StatisticsAggregator")]
        public static void TestBatch2FromIssue5373()
        {
            var expectedResult = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.ContainerOnly, "Test", 0),
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

            var result = StatisticsAggregator.AggregateStatistics(perFileResults).ToArray();

            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        // Non-container-only and container-only can be output for a particular path
        [Test, Category("StatisticsAggregator")]
        public static void TestContainerXPath()
        {
            var expectedResult = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.ContainerOnly, "Test", 0),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test", 2),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test", 1),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test", 1),
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

            var result = StatisticsAggregator.AggregateStatistics(perFileResults).ToArray();

            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        /// <summary>
        /// Attribute paths should be aggregated in a way that ignores case
        /// because the <see cref="StatisticsSummarizer"/> ignores case and will ignore some of the values 
        /// </summary>
        [Test, Category("StatisticsAggregator")]
        public static void TestPathsOfDifferentCase()
        {
            var input = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.Expected, "PONumber", 2),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "PONumber", 1),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "PONumber", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "POnumber", 4),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "POnumber", 3),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "POnumber", 1),
            };

            var expectedOutput = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.Expected, "PONumber", 6),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "PONumber", 4),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "PONumber", 2),
            };

            var actualOutput = StatisticsAggregator.AggregateStatistics(input).ToArray();

            CollectionAssert.AreEquivalent(expectedOutput, actualOutput);
        }

        #endregion Tests
    }
}