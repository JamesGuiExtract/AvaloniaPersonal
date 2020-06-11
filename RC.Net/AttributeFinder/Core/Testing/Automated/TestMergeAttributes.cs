using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_AFOUTPUTHANDLERSLib;
using UCLID_RASTERANDOCRMGMTLib;
using System.Diagnostics;
using System;

namespace Extract.AttributeFinder.Test
{
    /// <summary>
    /// Provides test cases for the <see cref="MergeAttributes"/> class.
    /// </summary>
    [TestFixture]
    [Category("MergeAttributes")]
    public class TestMergeAttributes
    {
        #region Constants

        /// <summary>
        /// The name of an embedded resource test VOA file.
        /// </summary>
        const string _TEST_USS_FILE = "Resources.A418.tif.uss";
        const string _TEST_DATA_FILE = "Resources.A418.tif.numbers-in-triplicate.voa";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Manages the test images needed for testing.
        /// </summary>
        static TestFileManager<TestMergeAttributes> _testImages;

        #endregion Fields

        #region Overhead

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testImages = new TestFileManager<TestMergeAttributes>();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [OneTimeTearDown]
        public static void FinalCleanup()
        {
            // Dispose of the test image manager
            if (_testImages != null)
            {
                _testImages.Dispose();
            }
        }

        #endregion Overhead

        #region Tests

        /// <summary>
        /// Test that merging duplicate attributes is not horribly slow
        /// https://extract.atlassian.net/browse/ISSUE-16741
        /// </summary>
        [Test]
        public static void TestMergingManyDuplicates()
        {
            var doc = new AFDocumentClass();
            doc.Text.LoadFrom(_testImages.GetFile(_TEST_USS_FILE), false);

            var attributes = new IUnknownVectorClass();
            attributes.LoadFrom(_testImages.GetFile(_TEST_DATA_FILE), false);
            var tripleSize = attributes.Size();

            var noDuplicates = attributes.ToIEnumerable<IAttribute>().Take(tripleSize / 3).ToIUnknownVector();

            var merger = new MergeAttributesClass
            {
                AttributeQuery = "*",
                CreateMergedRegion = false
            };

            var sw = new Stopwatch();
            sw.Start();
            merger.ProcessOutput(noDuplicates, doc, null);
            sw.Stop();
            var timeToDoNothing = sw.ElapsedMilliseconds;
            Assert.AreEqual(tripleSize / 3, noDuplicates.Size());

            sw.Restart();
            merger.ProcessOutput(attributes, doc, null);
            sw.Stop();
            var timeToMergeDuplicates = sw.ElapsedMilliseconds;
            Assert.AreEqual(tripleSize / 3, attributes.Size());
            Assert.Less(timeToMergeDuplicates, timeToDoNothing * 100, "Full regression likely");
            Assert.Less(timeToMergeDuplicates, timeToDoNothing * 50, "Partial regression likely");
        }

        #endregion Tests
    }
}