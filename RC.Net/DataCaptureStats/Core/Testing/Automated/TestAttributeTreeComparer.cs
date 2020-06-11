using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Linq;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_COMUTILSLib;

namespace Extract.DataCaptureStats.Test
{
    /// <summary>
    /// Unit tests for AttributeTreeComparer class
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("AttributeTreeComparer")]
    public class TestAttributeTreeComparer
    {
        #region Fields

        /// <summary>
        /// Manages the test data files
        /// </summary>
        static TestFileManager<TestAttributeTreeComparer> _testFiles;

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

            _testFiles = new TestFileManager<TestAttributeTreeComparer>();
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

        // Ensure extra attribute is counted incorrect
        [Test, Category("AttributeTreeComparer")]
        public static void TestExtra()
        {
            SetFiles("Resources.TestExtra.found.eav", "Resources.TestExtra.expected.eav");
            var expectedResult = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test", 1),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test", 1),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test", 1),
            };

            var result = AttributeTreeComparer.CompareAttributes(_expected, _found)
                .ToArray();

            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        // Ensure that 'correct' subattribute of wrong tree is counted as incorrect
        [Test, Category("AttributeTreeComparer")]
        public static void TestIncorrectSubAttributes()
        {
            SetFiles("Resources.TestIncorrectSub.found.eav", "Resources.TestIncorrectSub.expected.eav");
            var expectedResult = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test", 2),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test", 2),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Test", 2),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Test", 2),
            };

            var result = AttributeTreeComparer.CompareAttributes(_expected, _found)
                .ToArray();

            CollectionAssert.AreEquivalent(expectedResult, result);
        }


        // Ensure Types are taken into account
        [Test, Category("AttributeTreeComparer")]
        public static void TestTypes1()
        {
            SetFiles("Resources.TestTypes1.found.eav", "Resources.TestTypes1.expected.eav");
            var expectedResult = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test@A", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test@B", 1),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test@A", 1),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test@B", 1),
            };

            var result = AttributeTreeComparer.CompareAttributes(_expected, _found)
                .ToArray();

            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        // Ensure Types are taken into account
        [Test, Category("AttributeTreeComparer")]
        public static void TestTypes2()
        {
            SetFiles("Resources.TestTypes1.found.eav", "Resources.TestTypes1.expected.eav");
            foreach (var attribute in _expected.ToIEnumerable<IAttribute>())
            {
                if (attribute.Type.Equals("A", StringComparison.OrdinalIgnoreCase))
                {
                    attribute.Type = "B";
                }
                else
                {
                    attribute.Type = "A";
                }
            }
            var expectedResult = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test@A", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test@B", 1),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test@A", 1),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test@B", 1),
            };

            var result = AttributeTreeComparer.CompareAttributes(_expected, _found)
                .ToArray();

            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        // Out of order test case 1 from ISSUE-5373
        [Test, Category("AttributeTreeComparer")]
        public static void OutOfOrder_Test01()
        {
            SetFiles("Resources.OoO_Test1.1.found.eav", "Resources.OoO.1.expected.eav");
            var expectedResult = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.ContainerOnly, "Test", 0),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component", 52),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component", 49),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component", 2),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Flag", 19),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Flag", 17),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Flag", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range", 43),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range", 40),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range", 2),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range/Max", 43),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range/Max", 40),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range/Max", 2),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range/Min", 43),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range/Min", 40),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range/Min", 2),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Units", 47),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Units", 33),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Units", 4),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Value", 52),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Value", 48),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Value", 3),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Date", 4),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Date", 3),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Name", 4),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Name", 2),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Name", 1),
            };

            var result = AttributeTreeComparer.CompareAttributes(_expected, _found)
                .ToArray();

            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        // Out of order test case 2 from ISSUE-5373
        [Test, Category("AttributeTreeComparer")]
        public static void OutOfOrder_Test02()
        {
            SetFiles("Resources.OoO_Test2.1.found.eav", "Resources.OoO.1.expected.eav");
            var expectedResult = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.ContainerOnly, "Test", 0),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component", 52),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component", 47),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Flag", 19),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Flag", 18),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range", 43),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range", 41),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range/Max", 43),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range/Max", 41),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range/Max", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range/Min", 43),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range/Min", 41),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range/Min", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Units", 47),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Units", 34),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Units", 3),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Value", 52),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Value", 46),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Value", 2),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Date", 4),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Date", 3),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Name", 4),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Name", 3),
            };

            var result = AttributeTreeComparer.CompareAttributes(_expected, _found)
                .ToArray();

            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        // Out of order test case 3 from ISSUE-5373
        [Test, Category("AttributeTreeComparer")]
        public static void OutOfOrder_Test03()
        {
            SetFiles("Resources.OoO_Test3.1.found.eav", "Resources.OoO.1.expected.eav");
            var expectedResult = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.ContainerOnly, "Test", 0),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component", 52),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component", 50),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Flag", 19),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Flag", 18),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range", 43),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range", 41),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range/Max", 43),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range/Max", 41),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range/Max", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range/Min", 43),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range/Min", 41),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range/Min", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Units", 47),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Units", 34),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Units", 3),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Value", 52),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Value", 49),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Value", 2),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Date", 4),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Date", 4),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Name", 4),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Name", 4),
            };

            var result = AttributeTreeComparer.CompareAttributes(_expected, _found)
                .ToArray();

            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        // Out of order test case 4 from ISSUE-5373
        [Test, Category("AttributeTreeComparer")]
        public static void OutOfOrder_Test04()
        {
            SetFiles("Resources.OoO_Test4.1.found.eav", "Resources.OoO.1.expected.eav");
            var expectedResult = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.ContainerOnly, "Test", 0),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component", 52),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component", 35),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Flag", 19),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Flag", 11),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range", 43),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range", 27),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range/Max", 43),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range/Max", 27),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range/Min", 43),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range/Min", 27),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Units", 47),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Units", 23),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Units", 2),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Value", 52),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Value", 35),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Date", 4),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Date", 3),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Name", 4),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Name", 3),
            };

            var result = AttributeTreeComparer.CompareAttributes(_expected, _found)
                .ToArray();

            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        // Out of order test case 5 from ISSUE-5373
        [Test, Category("AttributeTreeComparer")]
        public static void OutOfOrder_Test05()
        {
            SetFiles("Resources.OoO_Test5.1.found.eav", "Resources.OoO.1.expected.eav");
            var expectedResult = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.ContainerOnly, "Test", 0),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component", 52),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component", 49),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Flag", 19),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Flag", 17),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range", 43),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range", 40),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range/Max", 43),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range/Max", 40),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range/Max", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range/Min", 43),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range/Min", 40),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range/Min", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Units", 47),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Units", 33),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Units", 3),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Value", 52),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Value", 48),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Value", 2),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Date", 4),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Date", 3),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Name", 4),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Name", 3),
            };

            var result = AttributeTreeComparer.CompareAttributes(_expected, _found)
                .ToArray();

            CollectionAssert.AreEquivalent(expectedResult, result);
        }


        // Out of order test case 6 from ISSUE-5373
        [Test, Category("AttributeTreeComparer")]
        public static void OutOfOrder_Test06()
        {
            SetFiles("Resources.OoO_Test6.1.found.eav", "Resources.OoO.1.expected.eav");
            var expectedResult = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.ContainerOnly, "Test", 0),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component", 52),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component", 50),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Flag", 19),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Flag", 18),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range", 43),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range", 41),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range/Max", 43),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range/Max", 41),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range/Max", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range/Min", 43),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range/Min", 41),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range/Min", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Units", 47),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Units", 34),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Units", 3),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Value", 52),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Value", 49),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Value", 2),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Date", 4),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Date", 4),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Name", 4),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Name", 4),
            };

            var result = AttributeTreeComparer.CompareAttributes(_expected, _found)
                .ToArray();

            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        // Out of order test case 7 from ISSUE-5373
        [Test, Category("AttributeTreeComparer")]
        public static void OutOfOrder_Test07()
        {
            SetFiles("Resources.OoO_Test7.2.found.eav", "Resources.OoO.2.expected.eav");
            var expectedResult = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.ContainerOnly, "Test", 0),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component", 68),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component", 67),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component", 3),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range", 68),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range", 44),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range", 12),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range/Max", 68),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range/Max", 44),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range/Max", 12),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range/Min", 68),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range/Min", 44),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range/Min", 12),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Units", 68),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Units", 52),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Value", 68),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Value", 55),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Value", 10),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Date", 4),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Date", 4),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Name", 4),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Name", 4),
            };

            var result = AttributeTreeComparer.CompareAttributes(_expected, _found)
                .ToArray();

            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        // Out of order test case 8 from ISSUE-5373
        [Test, Category("AttributeTreeComparer")]
        public static void OutOfOrder_Test08()
        {
            SetFiles("Resources.OoO_Test8.2.found.eav", "Resources.OoO.2.expected.eav");
            var expectedResult = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.ContainerOnly, "Test", 0),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component", 68),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component", 67),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component", 3),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range", 68),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range", 44),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range", 12),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range/Max", 68),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range/Max", 44),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range/Max", 12),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range/Min", 68),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range/Min", 44),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range/Min", 12),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Units", 68),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Units", 52),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Value", 68),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Value", 55),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Value", 10),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Date", 4),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Date", 4),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Name", 4),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Name", 4),
            };

            var result = AttributeTreeComparer.CompareAttributes(_expected, _found)
                .ToArray();

            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        // Out of order test case 9 from ISSUE-5373
        [Test, Category("AttributeTreeComparer")]
        public static void OutOfOrder_Test09()
        {
            SetFiles("Resources.OoO_Test9.3.found.eav", "Resources.OoO.3.expected.eav");
            var expectedResult = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.ContainerOnly, "Test", 0),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component", 8),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component", 8),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Flag", 2),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Flag", 2),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range", 8),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range", 7),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range/Max", 8),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range/Max", 7),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range/Max", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range/Min", 8),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range/Min", 7),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range/Min", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Units", 6),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Units", 6),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Value", 8),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Value", 5),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Value", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Date", 4),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Date", 4),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Name", 4),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Name", 4),
            };

            var result = AttributeTreeComparer.CompareAttributes(_expected, _found)
                .ToArray();

            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        // Out of order test case 10 from ISSUE-5373
        [Test, Category("AttributeTreeComparer")]
        public static void OutOfOrder_Test10()
        {
            SetFiles("Resources.OoO_Test10.3.found.eav", "Resources.OoO.3.expected.eav");
            var expectedResult = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.ContainerOnly, "Test", 0),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component", 8),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component", 8),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Flag", 2),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Flag", 2),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range", 8),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range", 7),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range/Max", 8),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range/Max", 7),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range/Max", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range/Min", 8),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range/Min", 7),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range/Min", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Units", 6),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Units", 6),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Value", 8),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Value", 5),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Value", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Date", 4),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Date", 4),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Name", 4),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Name", 4),
            };

            var result = AttributeTreeComparer.CompareAttributes(_expected, _found)
                .ToArray();

            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        // Out of order test case 11 from ISSUE-5373
        [Test, Category("AttributeTreeComparer")]
        public static void OutOfOrder_Test11()
        {
            SetFiles("Resources.OoO_Test11.3.found.eav", "Resources.OoO.3.expected.eav");
            var expectedResult = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.ContainerOnly, "Test", 0),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component", 8),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component", 4),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Flag", 2),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Flag", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range", 8),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range", 3),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range/Max", 8),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range/Max", 3),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range/Max", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range/Min", 8),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range/Min", 3),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range/Min", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Units", 6),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Units", 4),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Value", 8),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Value", 2),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Value", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Date", 4),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Date", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Name", 4),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Name", 3),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Name", 1),
            };

            var result = AttributeTreeComparer.CompareAttributes(_expected, _found)
                .ToArray();

            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        // Out of order test case 12 from ISSUE-5373
        [Test, Category("AttributeTreeComparer")]
        public static void OutOfOrder_Test12()
        {
            SetFiles("Resources.OoO_Test12.3.found.eav", "Resources.OoO.3.expected.eav");
            var expectedResult = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.ContainerOnly, "Test", 0),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component", 8),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component", 8),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Flag", 2),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Flag", 2),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range", 8),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range", 7),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range/Max", 8),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range/Max", 7),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range/Max", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Range/Min", 8),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Range/Min", 7),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Range/Min", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Units", 6),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Units", 6),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Component/Value", 8),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Component/Value", 5),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Component/Value", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Date", 4),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Date", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Name", 4),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Name", 4),
            };

            var result = AttributeTreeComparer.CompareAttributes(_expected, _found)
                .ToArray();

            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        // Ensure that non-empty ignore xpath works
        [Test, Category("AttributeTreeComparer")]
        public static void TestIgnoreXPath()
        {
            SetFiles("Resources.TestIncorrectSub.found.eav", "Resources.TestIncorrectSub.expected.eav");
            var expectedResult = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test", 2),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test", 2),
            };

            var ignoreXPath = "/*/Test/Test";
            var result = AttributeTreeComparer.CompareAttributes(_expected, _found, ignoreXPath: ignoreXPath)
                .ToArray();
            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        // Ensure that default container xpath works
        // Found uses N/A for top-level value but expected use both N/A and empty string
        [Test, Category("AttributeTreeComparer")]
        public static void TestContainerXPath1()
        {
            SetFiles("Resources.TestContainerXPath1.found.eav", "Resources.TestContainerXPath1.expected.eav");
            var expectedResult = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.ContainerOnly, "Test", 0),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Test", 2),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Test", 2),
            };

            var result = AttributeTreeComparer.CompareAttributes(_expected, _found)
                .ToArray();
            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        // Ensure that a non-default container xpath works
        [Test, Category("AttributeTreeComparer")]
        public static void TestContainerXPath2()
        {
            SetFiles("Resources.TestIncorrectSub.found.eav", "Resources.TestIncorrectSub.expected.eav");
            var expectedResult = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.ContainerOnly, "Test", 0),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/Test", 2),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "Test/Test", 2),
            };

            var containerXPath = "//*[not(text()) or text()='N/A'] | /*/Test";
            var result = AttributeTreeComparer.CompareAttributes(_expected, _found, containerXPath: containerXPath)
                .ToArray();

            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        // Make everything a container
        [Test, Category("AttributeTreeComparer")]
        public static void TestContainerXPath3()
        {
            SetFiles("Resources.TestIncorrectSub.found.eav", "Resources.TestIncorrectSub.expected.eav");
            var expectedResult = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.ContainerOnly, "Test", 0),
                new AccuracyDetail(AccuracyDetailLabel.ContainerOnly, "Test/Test", 0),
            };

            var containerXPath = "//*";
            var result = AttributeTreeComparer.CompareAttributes(_expected, _found, containerXPath: containerXPath)
                .ToArray();

            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        // Ensure that ambiguous container state is handled correctly
        // In this case because the expected PatientName attribute meets the criteria but found version doesn't
        // the result is that the path is not treated as a container-only
        [Test, Category("AttributeTreeComparer")]
        public static void TestContainerXPath4()
        {
            SetFiles("Resources.TestContainerXPath4.found.eav", "Resources.TestContainerXPath4.expected.eav");
            var expectedResult = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.ContainerOnly, "PatientInfo", 0),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "PatientInfo/PatientName", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "PatientInfo/PatientName/First", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "PatientInfo/PatientName/Last", 1),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "PatientInfo/PatientName", 1),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "PatientInfo/PatientName/First", 1),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "PatientInfo/PatientName/Last", 1),
            };

            var result = AttributeTreeComparer.CompareAttributes(_expected, _found)
                .ToArray();
            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        // Show resolution for ambiguous container state with explicit container XPath for PatientName
        [Test, Category("AttributeTreeComparer")]
        public static void TestContainerXPath5()
        {
            SetFiles("Resources.TestContainerXPath4.found.eav", "Resources.TestContainerXPath4.expected.eav");
            var expectedResult = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.ContainerOnly, "PatientInfo", 0),
                new AccuracyDetail(AccuracyDetailLabel.ContainerOnly, "PatientInfo/PatientName", 0),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "PatientInfo/PatientName/First", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "PatientInfo/PatientName/Last", 1),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "PatientInfo/PatientName/First", 1),
                new AccuracyDetail(AccuracyDetailLabel.Correct, "PatientInfo/PatientName/Last", 1),
            };

            var containerXPath = AttributeTreeComparer.DefaultContainerXPath + " | /*/PatientInfo/PatientName";
            var result = AttributeTreeComparer.CompareAttributes(_expected, _found,
                containerXPath: containerXPath)
                .ToArray();
            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        // Test handling of empty attribute via the default ignore pattern (should be treated as if it weren't there)
        [Test, Category("AttributeTreeComparer")]
        public static void TestEmptyExpectedAttribute()
        {
            SetFiles("Resources.TestEmptyExpected.found.eav", "Resources.TestEmptyExpected.expected.eav");
            var expectedResult = new AccuracyDetail[]
            {
                new AccuracyDetail(AccuracyDetailLabel.ContainerOnly, "Test", 0),
                new AccuracyDetail(AccuracyDetailLabel.ContainerOnly, "Test/EmptyWithNonEmptyChild", 0),
                new AccuracyDetail(AccuracyDetailLabel.ContainerOnly, "Test/EmptyWithNonEmptyGrandChild", 0),
                new AccuracyDetail(AccuracyDetailLabel.ContainerOnly, "Test/EmptyWithNonEmptyGrandChild/Empty", 0),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/EmptyWithNonEmptyChild/NonEmpty", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/EmptyWithNonEmptyGrandChild/Empty/NonEmpty", 1),
                new AccuracyDetail(AccuracyDetailLabel.Expected, "Test/NonEmpty", 1),
                new AccuracyDetail(AccuracyDetailLabel.Incorrect, "Test/Empty", 1),
            };

            var result = AttributeTreeComparer.CompareAttributes(_expected, _found).ToArray();
            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        #endregion Tests
    }
}