using Extract.Testing.Utilities;
using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

using SpatialString = UCLID_RASTERANDOCRMGMTLib.SpatialString;


namespace Extract.AttributeFinder.Test
{
    /// <summary>
    /// Provides test cases for the <see cref="TestCreateAttribute"/> class.
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("CreateAttribute")]
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    [SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
    public class TestCreateAttribute
    {
        #region Fields
        /// <summary>
        /// Manages the test files used by this test
        /// </summary>
        static TestFileManager<TestCreateAttribute> _testFiles;

        #region Test One File Names

        const string _TEST_ONE_RSD_FILE = "Resources.CreateAttribute_3SubAttrs.rsd";
        const string _A418_VOA_FILE = "Resources.A418.tif.TestCreateAttributes.voa";
        const string _A418_USS_FILE = "Resources.A418.tif.TestCreateAttributes.uss";
        const string _A418_EXPECTED_VOA_FILE = "Resources.A418_xpath_3_subattrs_added.voa";

        #endregion Test One File Names

        #region Test Two File Names

        const string _TEST_TWO_RSD_FILE = "Resources.CreateSubattrFlexIndex2.rsd";
        const string _EXAMPLE_02_VOA_FILE = "Resources.Example02.tif.voa";
        const string _EXAMPLE_02_USS_FILE = "Resources.Example02.tif.uss";
        const string _EXAMPLE02_EXPECTED_VOA_FILE = "Resources.Example02.tif.test.voa";

        #endregion Test Two File Names

        #region Test Three File Names

        const string _TEST_THREE_RSD_FILE = "Resources.CreateSubAttrIdShield_testImage013.rsd";
        const string _TEST_IMAGE_013_VOA_FILE = "Resources.TestImage013.tif.voa";
        const string _TEST_IMAGE_013_USS_FILE = "Resources.TestImage013.tif.uss";
        const string _TEST_IMAGE_013_EXPECTED_VOA_FILE = "Resources.TestImage013.tif.test.voa";

        #endregion Test Three File Names
        #endregion Fields

        #region Setup and Teardown
        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testFiles = new TestFileManager<TestCreateAttribute>();
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
        /// Tests creating three attributes under each /Test/Component node. After the rules are run, the 
        /// returned IUnknownVector should match the IUnknownVector loaded from the reference file.
        /// The original document is A418.tif, from Demo_LabDE/Input.
        /// </summary>
        [Test, Category("Interactive")]        
        public static void Test1()
        {
            DoTest(_A418_VOA_FILE, 
                   _A418_USS_FILE, 
                   _TEST_ONE_RSD_FILE, 
                   _A418_EXPECTED_VOA_FILE);
        }

        /// <summary>
        /// Tests creating one attribute under each Party node, named either Grantor or Grantee depending
        /// on the ./MappedTo value. 
        /// After the rules are run, the returned IUnknownVector should match the IUnknownVector 
        /// loaded from the reference file.
        /// The original document is Example02.tif, from Demo_FlexIndex/Input.
        /// </summary>
        [Test, Category("Interactive")]
        public static void Test2()
        {
            DoTest(_EXAMPLE_02_VOA_FILE, 
                   _EXAMPLE_02_USS_FILE, 
                   _TEST_TWO_RSD_FILE, 
                   _EXAMPLE02_EXPECTED_VOA_FILE);
        }

        /// <summary>
        /// Tests creating three attributes under the root, corresponding to three SSN attributes in the document.
        /// After the rules are run, the returned IUnknownVector should match the IUnknownVector 
        /// loaded from the reference file.
        /// The original document is TestImage013.tif, from Demo_IDShield/Input.
        /// </summary>
        [Test, Category("Interactive")]
        public static void Test3()
        {
            DoTest(_TEST_IMAGE_013_VOA_FILE, 
                   _TEST_IMAGE_013_USS_FILE, 
                   _TEST_THREE_RSD_FILE, 
                   _TEST_IMAGE_013_EXPECTED_VOA_FILE);
        }

        #endregion Public Test Functions

        #region Private Functions

        /// <summary>
        /// Gets the specified rule set from the resource.
        /// </summary>
        /// <param name="rsdFileName">Name of the RSD file.</param>
        /// <returns></returns>
        static RuleSet GetRuleSet(string rsdFileName)
        {
            var ruleSet = new RuleSet();
            ruleSet.LoadFrom(_testFiles.GetFile(rsdFileName), bSetDirtyFlagToTrue: false);
            return ruleSet;
        }

        /// <summary>
        /// Loads the voa file that is an embedded resource.
        /// </summary>
        /// <param name="voaFilename">The voa filename.</param>
        /// <param name="loadFromResource">true by default, when true loads from resource, when 
        /// false, uses filename directly with translating it from the resource.</param>
        /// <returns>loaded IUknownVector</returns>
        static IUnknownVector LoadVoaFile(string voaFilename, bool loadFromResource = true)
        {
            IUnknownVector attributes = new IUnknownVector();

            var filename = loadFromResource ? _testFiles.GetFile(voaFilename) : voaFilename;
            attributes.LoadFrom(filename, bSetDirtyFlagToTrue: false);

            return attributes;
        }

        /// <summary>
        /// Loads the uss file.
        /// </summary>
        /// <param name="ussFilename">The uss filename.</param>
        /// <returns>loaded SpatialString</returns>
        static SpatialString LoadUssFile(string ussFilename)
        {
            SpatialString ss = new SpatialString();

            var filename = _testFiles.GetFile(ussFilename);
            ss.LoadFrom(filename, bSetDirtyFlagToTrue: false);

            return ss;
        }

        /// <summary>
        /// This is the framework for all of the tests, as they are identical structurally.
        /// </summary>
        /// <param name="sourceVoaFilename">The source voa filename.</param>
        /// <param name="sourceUssFilename">The source uss filename.</param>
        /// <param name="rsdFilename">The RSD filename.</param>
        /// <param name="expectedVoaFilename">The expected voa filename.</param>
        static void DoTest(string sourceVoaFilename,
                           string sourceUssFilename,
                           string rsdFilename,
                           string expectedVoaFilename)
        {
            // load the source voa file
            IUnknownVector attributesFromSourceVoa = LoadVoaFile(sourceVoaFilename);

            AFDocument doc = new AFDocument();
            doc.Attribute.SubAttributes.Append(attributesFromSourceVoa);

            SpatialString ssText = LoadUssFile(sourceUssFilename);
            doc.Text = ssText;

            // run the rsd file
            RuleSet ruleSet = GetRuleSet(rsdFilename);

            AttributeFinderEngine afe = new AttributeFinderEngine();
            var attributesCreatedFromRules = afe.FindAttributes(doc,
                                                                strSrcDocFileName: "",          // no file, use AFDocText
                                                                nNumOfPagesToRecognize: -1,
                                                                varRuleSet: ruleSet,
                                                                pvecAttributeNames: null,       // process all attribute names
                                                                vbUseAFDocText: true,
                                                                bstrAlternateComponentDataDir: "",
                                                                pProgressStatus: null);

            var expectedVoaAttributes = LoadVoaFile(expectedVoaFilename);
            bool match = ((IComparableObject)expectedVoaAttributes).IsEqualTo(attributesCreatedFromRules);

            Assert.That(match);
        }

        #endregion Private Functions

    }
}
