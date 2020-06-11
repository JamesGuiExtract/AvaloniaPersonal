using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System.Linq;
using System.Threading;
using UCLID_COMUTILSLib;
using ComAttribute = UCLID_AFCORELib.Attribute;

namespace Extract.AttributeFinder.Test
{
    /// <summary>
    /// Test cases for AFUtils methods. This is not meant to be comprehensive at this time but to include
    /// test cases for new features.
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("AttributeMethods")]
    public class TestAttributeMethods
    {
        #region Fields

        /// <summary>
        /// Manages the test images needed for testing.
        /// </summary>
        static TestFileManager<TestAttributeMethods> _testFiles;

        #endregion Fields

        #region Setup and Teardown
        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testFiles = new TestFileManager<TestAttributeMethods>();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [OneTimeTearDown]
        public static void FinalCleanup()
        {
            _testFiles?.Dispose();
        }

        #endregion Setup and Teardown

        #region Public Test Functions

        /// <summary>
        /// Test GetComponentDataFolder2 run in parallel
        /// </summary>
        [Test, Category("AFUtility")]        
        public static void TestGetComponentDataFolder2()
        {
            ThreadLocal<UCLID_AFUTILSLib.AFUtility> afutility = new ThreadLocal<UCLID_AFUTILSLib.AFUtility>(() => new UCLID_AFUTILSLib.AFUtility());
            Enumerable.Range(1, 10000).AsParallel().ForAll(i =>
            {
                var componentDataDir = afutility.Value.GetComponentDataFolder2("Latest", null);
                Assert.NotNull(componentDataDir);
            });
        }

        /// <summary>
        /// Test TryDivideAttributesWithSimpleQuery
        /// </summary>
        [Test, Category("AttributeMethods")]        
        public static void TestTryDivideAttributesWithSimpleQuery()
        {

            var voaPath = _testFiles.GetFile("Resources.A418.tif.voa");
            var voa = new IUnknownVector();
            voa.LoadFrom(voaPath, false);
            var attributes = voa.ToIEnumerable<ComAttribute>();
            bool success = attributes.TryDivideAttributesWithSimpleQuery("PatientInfo", out var patientInfo, out var other);
            Assert.That(success);
            Assert.AreEqual(voa.Size(), patientInfo.Count() + other.Count());
            Assert.AreEqual(1, patientInfo.Count());

            patientInfo = patientInfo.First().SubAttributes.ToIEnumerable<ComAttribute>();
            success = patientInfo
                .TryDivideAttributesWithSimpleQuery("*@Date", out var dates, out other);
            Assert.That(success);
            Assert.AreEqual("12/12/1970", dates.Single().Value.String);

            var subattributes = attributes.SelectMany(a => a.SubAttributes.ToIEnumerable<ComAttribute>()).ToList();
            success = subattributes
                .TryDivideAttributesWithSimpleQuery("CollectionTime|*@Date", out var datesAndTimes, out other);
            Assert.That(success);
            Assert.AreEqual(7, datesAndTimes.Count());

            success = subattributes
                .TryDivideAttributesWithSimpleQuery("CollectionTime@Date", out var none, out other);
            Assert.That(success);
            Assert.AreEqual(0, none.Count());
            Assert.AreEqual(subattributes.Count, other.Count());

            success = subattributes
                .TryDivideAttributesWithSimpleQuery("CollectionTime@", out var some, out other);
            Assert.That(success);
            Assert.AreEqual(3, some.Count());
            Assert.AreEqual(subattributes.Count, 3 + other.Count());

            success = subattributes
                .TryDivideAttributesWithSimpleQuery("CollectionDate@", out none, out other);
            Assert.That(success);
            Assert.AreEqual(0, none.Count());
            Assert.AreEqual(subattributes.Count, other.Count());
        }

        /// <summary>
        /// Test that multiple types are handled correctly
        /// </summary>
        [Test, Category("AttributeMethods")]        
        public static void TestTryDivideAttributesWithMultipleTypes()
        {

            var voaPath = _testFiles.GetFile("Resources.A418.tif.voa");
            var voa = new IUnknownVector();
            voa.LoadFrom(voaPath, false);
            var attributes = voa.ToIEnumerable<ComAttribute>();
            bool success = attributes.TryDivideAttributesWithSimpleQuery("PatientInfo", out var patientInfo, out var other);
            Assert.That(success);
            Assert.AreEqual(voa.Size(), patientInfo.Count() + other.Count());
            Assert.AreEqual(1, patientInfo.Count());

            patientInfo = patientInfo.First().SubAttributes.ToIEnumerable<ComAttribute>();
            success = patientInfo
                .TryDivideAttributesWithSimpleQuery("*@Date", out var dates, out other);
            Assert.That(success);
            dates.Single().AddType("DOB");

            success = patientInfo
                .TryDivideAttributesWithSimpleQuery("*@Date", out dates, out other);
            Assert.That(success);
            Assert.AreEqual(1, dates.Count());

            success = patientInfo
                .TryDivideAttributesWithSimpleQuery("*@DOB", out dates, out other);
            Assert.That(success);
            Assert.AreEqual(1, dates.Count());

            // (This doesn't really test anything because it doesn't result in a type of "Date+DOB+"...)
            dates.Single().AddType("");
            success = patientInfo
                .TryDivideAttributesWithSimpleQuery("DOB@", out dates, out other);
            Assert.That(success);
            Assert.AreEqual(0, dates.Count());
        }

        /// <summary>
        /// Test non-simple queries
        /// </summary>
        [Test, Category("AttributeMethods")]        
        public static void TestTryDivideAttributesWithComplexQuery()
        {

            var voaPath = _testFiles.GetFile("Resources.A418.tif.voa");
            var voa = new IUnknownVector();
            voa.LoadFrom(voaPath, false);
            var attributes = voa.ToIEnumerable<ComAttribute>();

            Assert.That(!attributes.TryDivideAttributesWithSimpleQuery("*/*", out var _, out var _));
            Assert.That(!attributes.TryDivideAttributesWithSimpleQuery("*{*}", out var _, out var _));
        }

        #endregion Public Test Functions

    }
}
