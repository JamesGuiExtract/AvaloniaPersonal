using Extract.Testing.Utilities;
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
    /// Test cases for FKB propagation through rulesets.
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("FKB")]
    public class TestFKB
    {
        #region Fields
        /// <summary>
        /// Manages the test files used by this test
        /// </summary>
        static TestFileManager<TestFKB> _testFiles;

        const string _A418_TIF_FILE = "Resources.A418.tif";
        const string _A418_USS_FILE = "Resources.A418.tif.uss";
        const string _TEST_01_MASTER_RSD_FILE = "Resources.FKBTest.Master.rsd";
        const string _TEST_01_HELPER_RSD_FILE1 = "Resources.FKBTest.second.rsd";
        const string _TEST_01_HELPER_RSD_FILE2 = "Resources.FKBTest.third.rsd";

        #endregion Fields

        #region Setup and Teardown
        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testFiles = new TestFileManager<TestFKB>();
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
        /// Tests that the FKB specified in the ruleset ("Nonexistent") is propagated to the second and third rulesets
        /// </summary>
        /// <see>https://extract.atlassian.net/browse/ISSUE-14772</see>
        [Test, Category("FKB")]
        public static void Test01_MainFKB_Is_Propagated()
        {
            _testFiles.GetFile(_A418_TIF_FILE);
            string ussPath = _testFiles.GetFile(_A418_USS_FILE);
            string rsdPath = _testFiles.GetFile(_TEST_01_MASTER_RSD_FILE);
            string rsdDir = Path.GetDirectoryName(rsdPath);
            _testFiles.GetFile(_TEST_01_HELPER_RSD_FILE1, Path.Combine(rsdDir, "second.rsd"));
            _testFiles.GetFile(_TEST_01_HELPER_RSD_FILE2, Path.Combine(rsdDir, "third.rsd"));

            SpatialString ss = new SpatialString();
            ss.LoadFrom(ussPath, false);

            AFDocument doc = new AFDocument() { Text = ss };
            RuleSet ruleSet = new RuleSet();
            ruleSet.LoadFrom(rsdPath, false);

            Assert.AreEqual("Nonexistent", ruleSet.FKBVersion);

            var attributes = ruleSet.ExecuteRulesOnText(doc,
                pvecAttributeNames: null, bstrAlternateComponentDataDir: null, pProgressStatus: null);

            // Since a bogus FKB is specified, an exception will be logged and nothing found if the FKB is propagated.
            // Else, if something is wrong, the latest FKB will be used and an attribute will be found.
            Assert.AreEqual(0, attributes.Size());

            // Check that using Latest FKB does result in a found attribute
            ruleSet.FKBVersion = "Latest";
            attributes = ruleSet.ExecuteRulesOnText(doc,
                pvecAttributeNames: null, bstrAlternateComponentDataDir: null, pProgressStatus: null);
            Assert.AreEqual(1, attributes.Size());
        }

        /// <summary>
        /// Tests that the FKB specified in the ruleset ("Latest") is overridden by the specified
        /// AlternateComponentData dir ("C:\NonexistentDir") in the FFRSD rules in the third ruleset
        /// </summary>
        /// <see>https://extract.atlassian.net/browse/ISSUE-14772</see>
        [Test, Category("FKB")]
        public static void Test01_AlternateComponentDataDir_Is_Propagated()
        {
            _testFiles.GetFile(_A418_TIF_FILE);
            string ussPath = _testFiles.GetFile(_A418_USS_FILE);
            string rsdPath = _testFiles.GetFile(_TEST_01_MASTER_RSD_FILE);
            string rsdDir = Path.GetDirectoryName(rsdPath);
            _testFiles.GetFile(_TEST_01_HELPER_RSD_FILE1, Path.Combine(rsdDir, "second.rsd"));
            _testFiles.GetFile(_TEST_01_HELPER_RSD_FILE2, Path.Combine(rsdDir, "third.rsd"));

            SpatialString ss = new SpatialString();
            ss.LoadFrom(ussPath, false);

            AFDocument doc = new AFDocument() { Text = ss };
            RuleSet ruleSet = new RuleSet();
            ruleSet.LoadFrom(rsdPath, false);
            ruleSet.FKBVersion = "Latest";

            var attributes = ruleSet.ExecuteRulesOnText(doc,
                pvecAttributeNames: null, bstrAlternateComponentDataDir: null, pProgressStatus: null);
            Assert.AreEqual(1, attributes.Size());

            attributes = ruleSet.ExecuteRulesOnText(doc,
                pvecAttributeNames: null, bstrAlternateComponentDataDir: @"C:\NonexistentDir", pProgressStatus: null);

            // Since a bogus dir is specified, an exception will be logged and nothing found if the FKB is propagated.
            // Else, if something is wrong, the latest FKB will be used and an attribute will be found.
            Assert.AreEqual(0, attributes.Size());
        }

        #endregion
    }
}
