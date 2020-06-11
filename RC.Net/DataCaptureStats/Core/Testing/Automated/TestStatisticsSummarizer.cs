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

        #endregion Tests
    }
}