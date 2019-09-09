using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using UCLID_RASTERANDOCRMGMTLib;
using UCLID_COMUTILSLib;
using UCLID_AFUTILSLib;
using UCLID_AFCORELib;
using ComAttribute = UCLID_AFCORELib.Attribute;
using Extract.AttributeFinder.Rules;

namespace Extract.AttributeFinder.Test
{
    /// <summary>
    /// Unit tests for learning machine output handler class
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("LearningMachineOutputHandler")]
    public class TestLearningMachineOutputHandler
    {
        #region Fields

        /// <summary>
        /// Manages the test images needed for testing.
        /// </summary>
        static TestFileManager<TestLearningMachineOutputHandler> _testFiles;

        #endregion Fields

        #region Overhead

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestLearningMachineOutputHandler>();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestFixtureTearDown]
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

        [Test, Category("LearningMachineOutputHandler")]
        public static void TestDocClassifier()
        {
            _testFiles.GetFile("Resources.LearningMachine.DocumentCategorization.Example01.tif");
            var ussPath = _testFiles.GetFile("Resources.LearningMachine.DocumentCategorization.Example01.tif.uss");
            var voaPath = _testFiles.GetFile("Resources.LearningMachine.DocumentCategorization.Example01.tif.voa");
            var lmPath = _testFiles.GetFile("Resources.LearningMachine.DocClassifier.lm");
            var ss = new SpatialString();
            ss.LoadFrom(ussPath, false);
            var voa = new IUnknownVector();
            voa.LoadFrom(voaPath, false);
            var doc = new AFDocument { Text = ss };

            // Test not preserving input attributes
            var lmo = new LearningMachineOutputHandler { SavedMachinePath = lmPath, PreserveInputAttributes = false };
            lmo.ProcessOutput(voa, doc, null);

            Assert.AreEqual(1, voa.Size());
            Assert.AreEqual("Deed of Trust", ((IAttribute)voa.At(0)).Value.String);

            // Test preserving input attributes
            voa.LoadFrom(voaPath, false);
            var previousSize = voa.Size();
            lmo.PreserveInputAttributes = true;
            lmo.ProcessOutput(voa, doc, null);

            Assert.AreEqual(1 + previousSize, voa.Size());
            Assert.AreEqual("Deed of Trust", ((IAttribute)voa.At(previousSize)).Value.String);
        }

        [Test, Category("LearningMachineOutputHandler")]
        public static void TestPagination()
        {
            _testFiles.GetFile("Resources.LearningMachine.Pagination.Pagination_001.tif");
            var ussPath = _testFiles.GetFile("Resources.LearningMachine.Pagination.Pagination_001.tif.uss");
            var voaPath = _testFiles.GetFile("Resources.LearningMachine.Pagination.Pagination_001.tif.protofeatures.voa");
            var lmPath = _testFiles.GetFile("Resources.LearningMachine.Paginator.lm");
            var ss = new SpatialString();
            ss.LoadFrom(ussPath, false);
            var voa = new IUnknownVector();
            voa.LoadFrom(voaPath, false);
            var doc = new AFDocument { Text = ss };

            // Test not preserving input attributes
            Assert.AreEqual(4, voa.Size());
            var lmo = new LearningMachineOutputHandler { SavedMachinePath = lmPath, PreserveInputAttributes = false };
            lmo.ProcessOutput(voa, doc, null);
            Assert.AreEqual(2, voa.Size());
            for (int i = 0; i < voa.Size(); ++i)
            {
                var subattributes = ((IAttribute)voa.At(i)).SubAttributes;
                // There should a Pages and a PaginationConfidence subattribute of each Document attribute
                Assert.AreEqual(2, subattributes.Size());
                Assert.AreEqual(SpecialAttributeNames.Pages, ((IAttribute)subattributes.At(0)).Name);
                Assert.AreEqual(SpecialAttributeNames.PaginationConfidence, ((IAttribute)subattributes.At(1)).Name);
            }

            // Test preserving input attributes
            voa.LoadFrom(voaPath, false);
            var pageOneTwoThreeAttributesCount = voa.ToIEnumerable<IAttribute>().Take(3).Sum(a => a.EnumerateDepthFirst().Count());
            var pageFourAttributesCount = voa.ToIEnumerable<IAttribute>().Skip(3).Take(1).Sum(a => a.EnumerateDepthFirst().Count());
            lmo.PreserveInputAttributes = true;
            lmo.ProcessOutput(voa, doc, null);
            Assert.AreEqual(2, voa.Size());
            // The first doc should have Pages and PaginationConfidence subattributes and three Page subattributes
            Assert.AreEqual(5, ((IAttribute)voa.At(0)).SubAttributes.Size());
            // The enumeration of the first Document attributes includes 1 Doc + 1 Pages + 1 PaginationConfidence + what was there before
            Assert.AreEqual(3 + pageOneTwoThreeAttributesCount, ((IAttribute)voa.At(0)).EnumerateDepthFirst().Count());
            // The second doc should have Pages and PaginationConfidence subattributes and one Page subattribute
            Assert.AreEqual(3, ((IAttribute)voa.At(1)).SubAttributes.Size());
            // The enumeration of the second Document attributes includes 1 Doc + 1 Pages + 1 PaginationConfidence + what was there before
            Assert.AreEqual(3 + pageFourAttributesCount, ((IAttribute)voa.At(1)).EnumerateDepthFirst().Count());
        }

        [Test, Category("LearningMachineOutputHandler")]
        public static void TrainMachineAttributeCategorizationDOB()
        {
            _testFiles.GetFile("Resources.LearningMachine.Pagination.Pagination_003.tif");
            var ussPath = _testFiles.GetFile("Resources.LearningMachine.Pagination.Pagination_003.tif.uss");
            var voaPath = _testFiles.GetFile("Resources.LearningMachine.Pagination.Pagination_003.tif.candidates.voa");
            var lmPath = _testFiles.GetFile("Resources.LearningMachine.AttributeClassifier.lm");
            var ss = new SpatialString();
            ss.LoadFrom(ussPath, false);
            var voa = new IUnknownVector();
            voa.LoadFrom(voaPath, false);
            var doc = new AFDocument { Text = ss };

            // Test not preserving input attributes
            Assert.AreEqual(8, voa.Size());
            var lmo = new LearningMachineOutputHandler { SavedMachinePath = lmPath, PreserveInputAttributes = false };
            lmo.ProcessOutput(voa, doc, null);
            Assert.AreEqual(8, voa.Size());
            Assert.AreEqual(1, ((IAttribute)voa.At(0)).SubAttributes.Size());
            Assert.AreEqual("DOB", ((IAttribute)((IAttribute)voa.At(0)).SubAttributes.At(0)).Value.String);

            // Test preserving input attributes
            voa.LoadFrom(voaPath, false);
            var previousSubattributeSize = ((IAttribute)voa.At(0)).SubAttributes.Size();
            lmo.PreserveInputAttributes = true;
            lmo.ProcessOutput(voa, doc, null);
            Assert.AreEqual(8, voa.Size());
            Assert.AreEqual(1 + previousSubattributeSize, ((IAttribute)voa.At(0)).SubAttributes.Size());
            Assert.AreEqual("DOB", ((IAttribute)((IAttribute)voa.At(0)).SubAttributes.At(previousSubattributeSize)).Value.String);
        }
    }

    #endregion Tests
}
