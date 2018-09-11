﻿using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;
using SpatialString = UCLID_RASTERANDOCRMGMTLib.SpatialString;


namespace Extract.AttributeFinder.Test
{
    /// <summary>
    /// Test cases for checking that OCR parameters are set by the OCR process and are propagated through various common rule objects
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("OCRParameters")]
    public class TestOCRParameters
    {
        #region Fields
        /// <summary>
        /// Manages the test files used by this test
        /// </summary>
        static TestFileManager<TestOCRParameters> _testFiles;

        const string _A418_TIF_FILE = "Resources.A418.tif";
        const string _DEFAULT_PARAMS_FILE = "Resources.defaultOCRParams.rsd";
        const string _TEST_PROPAGATION_RULESET = "Resources.TestOCRParamsPropagation.rsd";
        const string _CALL_TEST_PROPAGATION_RULESET = "Resources.CallTestOCRParamsPropagation.rsd";
        const string _CALL_TEST_PROPAGATION_BY_PAGE_RULESET = "Resources.CallTestOCRParamsPropagationByPage.rsd";
        const string _CALL_TEST_PROPAGATION_BY_DOC_RULESET = "Resources.CallTestOCRParamsPropagationByDocument.rsd";

        static readonly string _OCR_SINGLE_DOCUMENT_APPLICATION =
            Path.Combine(FileSystemMethods.CommonComponentsPath, "OCRSingleDocument.exe");

        #endregion Fields

        #region Setup and Teardown
        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testFiles = new TestFileManager<TestOCRParameters>();
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
        /// Tests using OCRSingleDocument.exe without parameters
        /// </summary>
        [Test, Category("OCRParameters")]        
        public static void OCRSingleDocument()
        {
            string ussPath = null;

            try
            {
                string imagePath = _testFiles.GetFile(_A418_TIF_FILE);
                ussPath = imagePath + ".uss";
                Assert.That(!File.Exists(ussPath));

                int exitCode = SystemMethods.RunExecutable(
                    _OCR_SINGLE_DOCUMENT_APPLICATION,
                    new[] { imagePath },
                    createNoWindow: true);

                Assert.AreEqual(0, exitCode);
                Assert.That(File.Exists(ussPath));

                var uss = new SpatialStringClass();
                uss.LoadFrom(ussPath, false);
                var ocrParams = ((IHasOCRParameters)uss).OCRParameters;

                Assert.AreEqual(0, ocrParams.Size);
            }
            finally
            {
                if (ussPath != null && File.Exists(ussPath))
                {
                    File.Delete(ussPath);
                }
            }
        }

        /// <summary>
        /// Tests using OCRSingleDocument.exe with parameters
        /// </summary>
        [Test, Category("OCRParameters")]        
        public static void OCRSingleDocumentWithDefaultParams()
        {
            string ussPath = null;

            try
            {
                string imagePath = _testFiles.GetFile(_A418_TIF_FILE);
                string paramsPath = _testFiles.GetFile(_DEFAULT_PARAMS_FILE);
                ussPath = imagePath + ".uss";
                Assert.That(!File.Exists(ussPath));

                int exitCode = SystemMethods.RunExecutable(
                    _OCR_SINGLE_DOCUMENT_APPLICATION,
                    new[] { imagePath, "/params", paramsPath },
                    createNoWindow: true);

                Assert.AreEqual(0, exitCode);
                Assert.That(File.Exists(ussPath));

                var uss = new SpatialStringClass();
                uss.LoadFrom(ussPath, false);
                var ocrParamsFromUss = ((IHasOCRParameters)uss).OCRParameters.ToIEnumerable().ToList();

                var rsd = new RuleSetClass();
                rsd.LoadFrom(paramsPath, false);
                var ocrParamsFromRuleset = ((IHasOCRParameters)rsd).OCRParameters.ToIEnumerable().ToList();

                CollectionAssert.AreEqual(ocrParamsFromRuleset, ocrParamsFromUss);
            }
            finally
            {
                if (ussPath != null && File.Exists(ussPath))
                {
                    File.Delete(ussPath);
                }
            }
        }

        /// <summary>
        /// Tests that OCR parameters are propagated from the input spatial string to a reOCR modifier via the AFDocument
        /// </summary>
        [Test, Category("OCRParameters")]        
        public static void SingleRSDPropagation()
        {
            string ussPath = null;

            try
            {
                string imagePath = _testFiles.GetFile(_A418_TIF_FILE);
                string paramsPath = _testFiles.GetFile(_DEFAULT_PARAMS_FILE);
                string rulesetPath = _testFiles.GetFile(_TEST_PROPAGATION_RULESET);
                ussPath = imagePath + ".uss";
                Assert.That(!File.Exists(ussPath));

                int exitCode = SystemMethods.RunExecutable(
                    _OCR_SINGLE_DOCUMENT_APPLICATION,
                    new[] { imagePath, "/params", paramsPath },
                    createNoWindow: true);

                Assert.AreEqual(0, exitCode);
                Assert.That(File.Exists(ussPath));

                var uss = new SpatialStringClass();
                uss.LoadFrom(ussPath, false);
                var ocrParamsFromUss = ((IHasOCRParameters)uss).OCRParameters.ToIEnumerable().ToList();

                var rsd = new RuleSetClass();
                rsd.LoadFrom(rulesetPath, false);

                var doc = new AFDocumentClass();
                doc.Text = uss;
                var attrr = rsd.ExecuteRulesOnText(doc, null, null, null);

                var attributeParams = attrr.ToIEnumerable<IAttribute>()
                    .Select(a => ((IHasOCRParameters)a.Value).OCRParameters.ToIEnumerable().ToList());

                int i = 0;
                foreach (var p in attributeParams)
                {
                    CollectionAssert.AreEqual(ocrParamsFromUss, p, UtilityMethods.FormatCurrent($"Attribute #{++i} doesn't have the correct parameters"));
                }
            }
            finally
            {
                if (ussPath != null && File.Exists(ussPath))
                {
                    File.Delete(ussPath);
                }
            }
        }

        /// <summary>
        /// Tests that OCR parameters are propagated through a find-from-rsd rule
        /// </summary>
        [Test, Category("OCRParameters")]        
        public static void MultipleRSDPropagation()
        {
            string ussPath = null;

            try
            {
                string imagePath = _testFiles.GetFile(_A418_TIF_FILE);
                string paramsPath = _testFiles.GetFile(_DEFAULT_PARAMS_FILE);
                _testFiles.GetFile(_TEST_PROPAGATION_RULESET);
                string rulesetPath = _testFiles.GetFile(_CALL_TEST_PROPAGATION_RULESET);
                ussPath = imagePath + ".uss";
                Assert.That(!File.Exists(ussPath));

                int exitCode = SystemMethods.RunExecutable(
                    _OCR_SINGLE_DOCUMENT_APPLICATION,
                    new[] { imagePath, "/params", paramsPath },
                    createNoWindow: true);

                Assert.AreEqual(0, exitCode);
                Assert.That(File.Exists(ussPath));

                var uss = new SpatialStringClass();
                uss.LoadFrom(ussPath, false);
                var ocrParamsFromUss = ((IHasOCRParameters)uss).OCRParameters.ToIEnumerable().ToList();

                var rsd = new RuleSetClass();
                rsd.LoadFrom(rulesetPath, false);

                var doc = new AFDocumentClass();
                doc.Text = uss;
                var attrr = rsd.ExecuteRulesOnText(doc, null, null, null);

                var attributeParams = attrr.ToIEnumerable<IAttribute>()
                    .Select(a => ((IHasOCRParameters)a.Value).OCRParameters.ToIEnumerable().ToList());

                int i = 0;
                foreach (var p in attributeParams)
                {
                    CollectionAssert.AreEqual(ocrParamsFromUss, p, UtilityMethods.FormatCurrent($"Attribute #{++i} doesn't have the correct parameters"));
                }
            }
            finally
            {
                if (ussPath != null && File.Exists(ussPath))
                {
                    File.Delete(ussPath);
                }
            }
        }

        /// <summary>
        /// Tests that OCR parameters are propagated through a find-from-rsd rule that is run using the insert-below-document mode
        /// </summary>
        [Test, Category("OCRParameters")]        
        public static void MultipleRSDByDocumentPropagation()
        {
            string ussPath = null;

            try
            {
                string imagePath = _testFiles.GetFile(_A418_TIF_FILE);
                string paramsPath = _testFiles.GetFile(_DEFAULT_PARAMS_FILE);
                _testFiles.GetFile(_TEST_PROPAGATION_RULESET);
                string rulesetPath = _testFiles.GetFile(_CALL_TEST_PROPAGATION_BY_DOC_RULESET);
                ussPath = imagePath + ".uss";
                Assert.That(!File.Exists(ussPath));

                int exitCode = SystemMethods.RunExecutable(
                    _OCR_SINGLE_DOCUMENT_APPLICATION,
                    new[] { imagePath, "/params", paramsPath },
                    createNoWindow: true);

                Assert.AreEqual(0, exitCode);
                Assert.That(File.Exists(ussPath));

                var uss = new SpatialStringClass();
                uss.LoadFrom(ussPath, false);
                var ocrParamsFromUss = ((IHasOCRParameters)uss).OCRParameters.ToIEnumerable().ToList();

                var rsd = new RuleSetClass();
                rsd.LoadFrom(rulesetPath, false);

                var doc = new AFDocumentClass();
                doc.Text = uss;
                var attrr = rsd.ExecuteRulesOnText(doc, null, null, null);

                var attributeParams = attrr.ToIEnumerable<IAttribute>()
                    .SelectMany(a => a.SubAttributes.ToIEnumerable<IAttribute>())
                    .Select(a => ((IHasOCRParameters)a.Value).OCRParameters.ToIEnumerable().ToList());

                int i = 0;
                foreach (var p in attributeParams)
                {
                    CollectionAssert.AreEqual(ocrParamsFromUss, p, UtilityMethods.FormatCurrent($"Attribute #{++i} doesn't have the correct parameters"));
                }
            }
            finally
            {
                if (ussPath != null && File.Exists(ussPath))
                {
                    File.Delete(ussPath);
                }
            }
        }

        /// <summary>
        /// Tests that OCR parameters are propagated through a find-from-rsd rule that is run per-page
        /// </summary>
        [Test, Category("OCRParameters")]        
        public static void MultipleRSDByPagePropagation()
        {
            string ussPath = null;

            try
            {
                string imagePath = _testFiles.GetFile(_A418_TIF_FILE);
                string paramsPath = _testFiles.GetFile(_DEFAULT_PARAMS_FILE);
                _testFiles.GetFile(_TEST_PROPAGATION_RULESET);
                string rulesetPath = _testFiles.GetFile(_CALL_TEST_PROPAGATION_BY_PAGE_RULESET);
                ussPath = imagePath + ".uss";
                Assert.That(!File.Exists(ussPath));

                int exitCode = SystemMethods.RunExecutable(
                    _OCR_SINGLE_DOCUMENT_APPLICATION,
                    new[] { imagePath, "/params", paramsPath },
                    createNoWindow: true);

                Assert.AreEqual(0, exitCode);
                Assert.That(File.Exists(ussPath));

                var uss = new SpatialStringClass();
                uss.LoadFrom(ussPath, false);
                var ocrParamsFromUss = ((IHasOCRParameters)uss).OCRParameters.ToIEnumerable().ToList();

                var rsd = new RuleSetClass();
                rsd.LoadFrom(rulesetPath, false);

                var doc = new AFDocumentClass();
                doc.Text = uss;
                var attrr = rsd.ExecuteRulesOnText(doc, null, null, null);

                var attributeParams = attrr.ToIEnumerable<IAttribute>()
                    .SelectMany(a => a.SubAttributes.ToIEnumerable<IAttribute>())
                    .Select(a => ((IHasOCRParameters)a.Value).OCRParameters.ToIEnumerable().ToList());

                int i = 0;
                foreach (var p in attributeParams)
                {
                    CollectionAssert.AreEqual(ocrParamsFromUss, p, UtilityMethods.FormatCurrent($"Attribute #{++i} doesn't have the correct parameters"));
                }
            }
            finally
            {
                if (ussPath != null && File.Exists(ussPath))
                {
                    File.Delete(ussPath);
                }
            }
        }

        #endregion Public Test Functions
    }
}
