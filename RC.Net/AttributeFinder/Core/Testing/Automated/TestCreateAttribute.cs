using Extract.AttributeFinder.Rules;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
        const string _A418_TIF_FILE = "Resources.A418.tif";
        const string _A418_VOA_FILE = "Resources.A418.tif.TestCreateAttributes.voa";
        const string _A418_USS_FILE = "Resources.A418.tif.uss";
        const string _A418_EXPECTED_VOA_FILE = "Resources.A418_xpath_3_subattrs_added.voa";

        #endregion Test One File Names

        #region Test Two File Names

        const string _TEST_TWO_RSD_FILE = "Resources.CreateSubattrFlexIndex2.rsd";
        const string _EXAMPLE_02_VOA_FILE = "Resources.Example02.tif.voa";
        const string _EXAMPLE_02_TIF_FILE = "Resources.Example02.tif";
        const string _EXAMPLE_02_USS_FILE = "Resources.Example02.tif.uss";
        const string _EXAMPLE02_EXPECTED_VOA_FILE = "Resources.Example02.tif.test.voa";

        #endregion Test Two File Names

        #region Test Three File Names

        const string _TEST_THREE_RSD_FILE = "Resources.CreateSubAttrIdShield_testImage013.rsd";
        const string _TEST_IMAGE_013_VOA_FILE = "Resources.TestImage013.tif.voa";
        const string _TEST_IMAGE_013_TIF_FILE = "Resources.TestImage013.tif";
        const string _TEST_IMAGE_013_USS_FILE = "Resources.TestImage013.tif.uss";
        const string _TEST_IMAGE_013_EXPECTED_VOA_FILE = "Resources.TestImage013.tif.test.voa";

        #endregion Test Three File Names

        #region Test Four File Names

        // Test four is the same as test 3 expect that it creates top-level attributes
        const string _TEST_FOUR_RSD_FILE = "Resources.CreateTopLevelAttrIdShield_testImage013.rsd";
        const string _TEST_IMAGE_013_EXPECTED_VOA_FILE_FOR_TEST_4 = "Resources.Test4.tif.test.voa";

        #endregion Test Four File Names

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
        [Test, Category("CreateAttribute")]        
        public static void Test1()
        {
            DoTest(_A418_VOA_FILE, 
                   _A418_USS_FILE, 
                   _TEST_ONE_RSD_FILE, 
                   _A418_EXPECTED_VOA_FILE,
                   _A418_TIF_FILE);
        }

        /// <summary>
        /// Tests creating one attribute under each Party node, named either Grantor or Grantee depending
        /// on the ./MappedTo value. 
        /// After the rules are run, the returned IUnknownVector should match the IUnknownVector 
        /// loaded from the reference file.
        /// The original document is Example02.tif, from Demo_FlexIndex/Input.
        /// </summary>
        [Test, Category("CreateAttribute")]
        public static void Test2()
        {
            DoTest(_EXAMPLE_02_VOA_FILE, 
                   _EXAMPLE_02_USS_FILE, 
                   _TEST_TWO_RSD_FILE, 
                   _EXAMPLE02_EXPECTED_VOA_FILE,
                   _EXAMPLE_02_TIF_FILE);
        }

        /// <summary>
        /// Tests creating three attributes under the root, corresponding to three SSN attributes in the document.
        /// After the rules are run, the returned IUnknownVector should match the IUnknownVector 
        /// loaded from the reference file.
        /// The original document is TestImage013.tif, from Demo_IDShield/Input.
        /// </summary>
        [Test, Category("CreateAttribute")]
        public static void Test3()
        {
            DoTest(_TEST_IMAGE_013_VOA_FILE, 
                   _TEST_IMAGE_013_USS_FILE, 
                   _TEST_THREE_RSD_FILE, 
                   _TEST_IMAGE_013_EXPECTED_VOA_FILE,
                   _TEST_IMAGE_013_TIF_FILE); 
        }

        /// <summary>
        /// Tests creating three attributes at the top-level, corresponding to three SSN attributes in the document.
        /// After the rules are run, the returned IUnknownVector should match the IUnknownVector 
        /// loaded from the reference file.
        /// The original document is TestImage013.tif, from Demo_IDShield/Input.
        /// (Like Test3 except creates top-level attributes)
        /// </summary>
        [Test, Category("CreateAttribute")]
        public static void Test4()
        {
            DoTest(_TEST_IMAGE_013_VOA_FILE, 
                   _TEST_IMAGE_013_USS_FILE, 
                   _TEST_FOUR_RSD_FILE, 
                   _TEST_IMAGE_013_EXPECTED_VOA_FILE_FOR_TEST_4,
                   _TEST_IMAGE_013_TIF_FILE); 
        }

        /// <summary>
        /// Tests that 'do not create if...' is ignored when value element is not XPath
        /// https://extract.atlassian.net/browse/ISSUE-16829
        /// </summary>
        [Test, Category("CreateAttribute")]
        public static void EmptyValue_Literal()
        {
            var creator = new CreateAttribute();
            creator.Root = "/*";
            creator.AddSubAttributeComponents(new AttributeNameAndTypeAndValue(
                name: "Test",
                type: "SSN",
                value: "",
                nameContainsXPath: false,
                typeContainsXPath: false,
                valueContainsXPath: false,
                doNotCreateIfNameIsEmpty: true,
                doNotCreateIfTypeIsEmpty: true,
                doNotCreateIfValueIsEmpty: true
                ));

            var attrr = new IUnknownVectorClass();
            creator.ProcessOutput(attrr, new AFDocumentClass(), null);
            Assert.AreEqual(1, attrr.Size());
        }

        /// <summary>
        /// Tests that 'do not create if...' is respected when value element is XPath
        /// </summary>
        [Test, Category("CreateAttribute")]
        public static void EmptyValue_XPath()
        {
            var creator = new CreateAttribute();
            creator.Root = "/*";
            creator.AddSubAttributeComponents(new AttributeNameAndTypeAndValue(
                name: "Test",
                type: "SSN",
                value: "_NON_EXISTENT_ELEMENT_",
                nameContainsXPath: false,
                typeContainsXPath: false,
                valueContainsXPath: true,
                doNotCreateIfNameIsEmpty: true,
                doNotCreateIfTypeIsEmpty: true,
                doNotCreateIfValueIsEmpty: true
                ));

            var attrr = new IUnknownVectorClass();
            creator.ProcessOutput(attrr, new AFDocumentClass(), null);
            Assert.AreEqual(0, attrr.Size());
        }

        /// <summary>
        /// Tests that 'do not create if...' is ignored when type element is not XPath
        /// https://extract.atlassian.net/browse/ISSUE-16829
        /// </summary>
        [Test, Category("CreateAttribute")]
        public static void EmptyType_Literal()
        {
            var creator = new CreateAttribute();
            creator.Root = "/*";
            creator.AddSubAttributeComponents(new AttributeNameAndTypeAndValue(
                name: "Test",
                type: "",
                value: "Val",
                nameContainsXPath: false,
                typeContainsXPath: false,
                valueContainsXPath: false,
                doNotCreateIfNameIsEmpty: true,
                doNotCreateIfTypeIsEmpty: true,
                doNotCreateIfValueIsEmpty: true
                ));

            var attrr = new IUnknownVectorClass();
            creator.ProcessOutput(attrr, new AFDocumentClass(), null);
            Assert.AreEqual(1, attrr.Size());
        }

        /// <summary>
        /// Tests that 'do not create if...' is respected when type element is XPath
        /// </summary>
        [Test, Category("CreateAttribute")]
        public static void EmptyType_XPath()
        {
            var creator = new CreateAttribute();
            creator.Root = "/*";
            creator.AddSubAttributeComponents(new AttributeNameAndTypeAndValue(
                name: "Test",
                type: "_NON_EXISTENT_ELEMENT",
                value: "Val",
                nameContainsXPath: false,
                typeContainsXPath: true,
                valueContainsXPath: false,
                doNotCreateIfNameIsEmpty: true,
                doNotCreateIfTypeIsEmpty: true,
                doNotCreateIfValueIsEmpty: true
                ));

            var attrr = new IUnknownVectorClass();
            creator.ProcessOutput(attrr, new AFDocumentClass(), null);
            Assert.AreEqual(0, attrr.Size());
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
        /// <param name="filenameToApply">filename to set the attributes primary spatial string SourceDocName to.
        /// This is necessary because otherwise the spatial string IsEqualTo() compare will return false always.</param>
        /// <returns>loaded IUknownVector</returns>
        static IUnknownVector LoadVoaFile(string voaFilename, string filenameToApply = "")
        {
            IUnknownVector attributes = new IUnknownVector();

            var filename = _testFiles.GetFile(voaFilename);
            attributes.LoadFrom(filename, bSetDirtyFlagToTrue: false);

            if (!string.IsNullOrWhiteSpace(filenameToApply))
            {
                foreach(var attr in attributes.ToIEnumerable<IAttribute>().SelectMany(a => a.EnumerateDepthFirst()))
                {
                    attr.Value.SourceDocName = filenameToApply;
                }
            }

            return attributes;
        }

        /// <summary>
        /// Loads the uss file.
        /// </summary>
        /// <param name="ussFilename">The uss filename.</param>
        /// <param name="filenameToApply">A filename to set the SourceDocName to. This is necessary 
        /// because AttributeFinderEngine.FindAttributes() accesses the SourceDocName; not setting
        /// this causes an exception when it tries to locate original file.</param>
        /// <returns>loaded SpatialString</returns>
        static SpatialString LoadUssFile(string ussFilename, string filenameToApply)
        {
            SpatialString ss = new SpatialString();

            var filename = _testFiles.GetFile(ussFilename);
            ss.LoadFrom(filename, bSetDirtyFlagToTrue: false);

            ss.SourceDocName = filenameToApply;

            return ss;
        }

        /// <summary>
        /// This is the framework for all of the tests, as they are identical structurally.
        /// </summary>
        /// <param name="sourceVoaFilename">The source voa filename.</param>
        /// <param name="sourceUssFilename">The source uss filename.</param>
        /// <param name="rsdFilename">The RSD filename.</param>
        /// <param name="expectedVoaFilename">The expected voa filename.</param>
        /// <param name="originalTifFilename">Name of the original .tif file</param>
        static void DoTest(string sourceVoaFilename,
                           string sourceUssFilename,
                           string rsdFilename,
                           string expectedVoaFilename,
                           string originalTifFilename)
        {
            // Need the original .tif file as well, or FindAttributes() throws an error.
            var tifFilename = _testFiles.GetFile(originalTifFilename);

            // load the source voa file
            IUnknownVector attributesFromSourceVoa = LoadVoaFile(sourceVoaFilename, filenameToApply: tifFilename);

            AFDocument doc = new AFDocument();
            doc.Attribute.SubAttributes.Append(attributesFromSourceVoa);

            SpatialString ssText = LoadUssFile(sourceUssFilename, filenameToApply: tifFilename);

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

            var expectedVoaAttributes = LoadVoaFile(expectedVoaFilename, filenameToApply: tifFilename);

            bool match = ((IComparableObject)expectedVoaAttributes).IsEqualTo(attributesCreatedFromRules);

            Assert.That(match);
        }

        #endregion Private Functions

    }
}
