﻿using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

using ComAttribute = UCLID_AFCORELib.Attribute;

namespace Extract.AttributeFinder.Test
{
    /// <summary>
    /// Class to test the RunMode interface of RuleSet defined in AFCore
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("RuleSetRunMode")]
    class TestRuleSetRunMode
    {
        #region Constants

        /// <summary>
        /// The embedded resource rsd file used as a base for running rules with differing settings
        /// </summary>
        static readonly string _RUN_MODE_TEST_RSD_FILE = "Resources.RunModeTest.rsd";

        /// <summary>
        /// The embedded resource rsd file that is saved with an older version to test defaults when 
        /// loading 
        /// </summary>
        static readonly string _RUN_MODE_LOAD_TEST_FROM_OLD_RSD_FILE = "Resources.RunModeLoadTestFromOld.rsd";

        /// <summary>
        /// The embedded resource uss file to run rules against.
        /// </summary>
        static readonly string _TEST_IMAGE_USS_FILE = "Resources.image3-4-6-7.tif.uss";

        /// <summary>
        /// The embedded resource voa expected output for RunMode = kRunPerDocument
        /// </summary>
        static readonly string _BY_DOC_VOA = "Resources.ByDoc.voa";

        /// <summary>
        /// The embedded resource voa expected output for 
        ///     RunMode = kRunPerDocument
        ///     InsertAttributesUnderParent = true
        ///     InsertParentName = "Document"
        ///     InsertParentValue = "&lt;SourceDocName&gt;"
        /// </summary>
        static readonly string _BY_DOC_UNDER_PARENT_SOURCE_DOC_NAME_VOA = "Resources.ByDocUnderParentSourceDocName.voa";

        /// <summary>
        /// The embedded resource voa expected output for 
        ///     RunMode = kRunPerDocument
        ///     InsertAttributesUnderParent = true
        ///     InsertParentName = "Document"
        ///     InsertParentValue = "&lt;PageContent&gt;"
        /// </summary>
        static readonly string _BY_DOC_UNDER_PARENT_PAGE_CONTENT_VOA = "Resources.ByDocUnderParentPageContent.voa";

        /// <summary>
        /// The embedded resource voa expected output for 
        ///     RunMode = kRunPerPage
        ///     InsertAttributesUnderParent = true
        ///     InsertParentName = "Page"
        ///     InsertParentValue = "&lt;PageNumber&gt;"
        ///     
        /// Also used for all of the test with RunMode = kPassInputVOAToOutput
        /// </summary>
        static readonly string _BY_PAGE_UNDER_PARENT_PAGE_NUMBER_VOA = "Resources.ByPageUnderParentPageNumber.voa";

        /// <summary>
        /// The embedded resource voa expected output for 
        ///     RunMode = kRunPerPage
        ///     InsertAttributesUnderParent = true
        ///     InsertParentName = "Page"
        ///     InsertParentValue = "&lt;PageContent&gt;"
        /// </summary>
        static readonly string _BY_PAGE_UNDER_PARENT_PAGE_CONTENT_VOA = "Resources.ByPageUnderParentPageContent.voa";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Manages the resource files for testing
        /// </summary>
        static TestFileManager<TestRuleSetRunMode> _testFiles;

        #endregion Fields

        #region Overhead

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestRuleSetRunMode>();

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

        #endregion Overhead

        #region Tests

        [Test, Category("RuleSetRunMode")]
        public static void Test01_RunModeDefaultSettings()
        {
            RuleSet rules = new RuleSet();
            IRunMode runMode = (IRunMode)rules;

            // Verify defaults
            TestDefaultRunModeSettings(runMode, "Default after create ");
        }

        [Test, Category("RuleSetRunMode")]
        public static void Test02_RunModeDefaultSettingsAfterLoadOfOldVersion()
        {
            // Load older version RuleSet
            RuleSet rules = GetLoadTestFromOldRuleSet();
            IRunMode runMode = (IRunMode)rules;
            
            TestDefaultRunModeSettings(runMode, "Default after load of old version ");
        }

        [Test, Category("RuleSetRunMode")]
        public static void Test03_RunModeChangeSettings()
        {
            RuleSet rules = new RuleSet();
            IRunMode runMode = (IRunMode)rules;

            ChangeAllSettingsFromDefault(runMode);

            TestChangedSettings(runMode, "setting failed.");
        }

        [Test, Category("RuleSetRunMode")]
        public static void Test04_RunModeChangeSettingsWithSaveAndLoad()
        {
            RuleSet rules = new RuleSet();
            IRunMode runMode = (IRunMode)rules;

            using (TemporaryFile tempRSD = new TemporaryFile(".rsd", false))
            {
                // Change settings from default
                ChangeAllSettingsFromDefault(runMode);

                // Save the changed settings to a file
                rules.SaveTo(tempRSD.FileName, true);

                // Create new object for loading
                rules = new RuleSet();
                runMode = (IRunMode)rules;

                // Load saved settings from file
                rules.LoadFrom(tempRSD.FileName, false);
                TestChangedSettings(runMode, "setting after save and load failed.");
            }
        }

        [Test, Category("RuleSetRunMode")]
        public static void Test05_RunModeByDocument()
        {
            RuleSet rules = GetBaseRuleSet();
            IRunMode runMode = (IRunMode)rules;

            runMode.RunMode = ERuleSetRunMode.kRunPerDocument;

            // Load the expected data for this test
            IUnknownVector expected = new IUnknownVector();
            expected.LoadFrom(_testFiles.GetFile(_BY_DOC_VOA), false);

            // Run the rules
            IUnknownVector results = 
                RunRules(rules, GetSourceDocNameFromVector(expected));
            
            // Verify the results
            Assert.That(IsEqual(results, expected), "Results do not match expected.");        }

        [Test, Category("RuleSetRunMode")]
        public static void Test06_RunModeByDocumentUnderParentSourceDocName()
        {
            RuleSet rules = GetBaseRuleSet();
            IRunMode runMode = (IRunMode) rules;
            runMode.RunMode = ERuleSetRunMode.kRunPerDocument;
            runMode.InsertAttributesUnderParent = true;
            runMode.InsertParentName = "Document";
            runMode.InsertParentValue = "<SourceDocName>";

            // Load the expected data for this test
            IUnknownVector expected = new IUnknownVector();
            expected.LoadFrom(_testFiles.GetFile(_BY_DOC_UNDER_PARENT_SOURCE_DOC_NAME_VOA), false);

            // Run the rules
            IUnknownVector results =
                RunRules(rules, GetSourceDocNameFromVector(expected));

            // Verify the results
            Assert.That(IsEqual(results, expected), "Results do not match expected.");
        }

        [Test, Category("RuleSetRunMode")]
        public static void Test07_RunModeByDocumentUnderParentPageContent()
        {
            RuleSet rules = GetBaseRuleSet();
            IRunMode runMode = (IRunMode)rules;
            runMode.RunMode = ERuleSetRunMode.kRunPerDocument;
            runMode.InsertAttributesUnderParent = true;
            runMode.InsertParentName = "Document";
            runMode.InsertParentValue = "<PageContent>";

            // Load the expected data for this test
            IUnknownVector expected = new IUnknownVector();
            expected.LoadFrom(_testFiles.GetFile(_BY_DOC_UNDER_PARENT_PAGE_CONTENT_VOA), false);

            // Run the rules
            IUnknownVector results =
                RunRules(rules, GetSourceDocNameFromVector(expected));

            // Verify the results
            Assert.That(IsEqual(results, expected), "Results do not match expected.");
        }

        [Test, Category("RuleSetRunMode")]
        public static void Test08_RunModeByPagePageNumber()
        {
            RuleSet rules = GetBaseRuleSet();
            IRunMode runMode = (IRunMode)rules;
            runMode.RunMode = ERuleSetRunMode.kRunPerPage;
            runMode.InsertAttributesUnderParent = true;
            runMode.InsertParentName = "Page";
            runMode.InsertParentValue = "<PageNumber>";

            // Load the expected data for this test
            IUnknownVector expected = new IUnknownVector();
            expected.LoadFrom(_testFiles.GetFile(_BY_PAGE_UNDER_PARENT_PAGE_NUMBER_VOA), false);

            // Run the rules
            IUnknownVector results =
                RunRules(rules, GetSourceDocNameFromVector(expected));

            // Verify the results
            Assert.That(IsEqual(results, expected), "Results do not match expected.");
        }

        [Test, Category("RuleSetRunMode")]
        public static void Test09_RunModeByPagePageContent()
        {
            RuleSet rules = GetBaseRuleSet();
            IRunMode runMode = (IRunMode)rules;
            runMode.RunMode = ERuleSetRunMode.kRunPerPage;
            runMode.InsertAttributesUnderParent = true;
            runMode.InsertParentName = "Page";
            runMode.InsertParentValue = "<PageContent>";

            // Load the expected data for this test
            IUnknownVector expected = new IUnknownVector();
            expected.LoadFrom(_testFiles.GetFile(_BY_PAGE_UNDER_PARENT_PAGE_CONTENT_VOA), false);

            // Run the rules
            IUnknownVector results =
                RunRules(rules, GetSourceDocNameFromVector(expected));

            // Verify the results
            Assert.That(IsEqual(results, expected), "Results do not match expected.");
        }

        [Test, Category("RuleSetRunMode")]
        public static void Test10_RunModePassInputVOAtoOutput()
        {
            RuleSet rules = GetBaseRuleSet();
            IRunMode runMode = (IRunMode)rules;
            runMode.RunMode = ERuleSetRunMode.kPassInputVOAToOutput;

            // Load the expected data for this test
            IUnknownVector expected = new IUnknownVector();
            expected.LoadFrom(_testFiles.GetFile(_BY_PAGE_UNDER_PARENT_PAGE_NUMBER_VOA), false);

            // Run the rules
            IUnknownVector results =
                RunRules(rules, GetSourceDocNameFromVector(expected), expected);

            // Verify the results
            Assert.That(IsEqual(results, expected, true), "Results do not match expected.");
        }

        [Test, Category("RuleSetRunMode")]
        public static void Test11_RunModePassInputVOAtoOutputUnderParentSourceDocName()
        {
            RuleSet rules = GetBaseRuleSet();
            IRunMode runMode = (IRunMode)rules;
            runMode.RunMode = ERuleSetRunMode.kPassInputVOAToOutput;
            runMode.InsertAttributesUnderParent = true;
            runMode.InsertParentName = "Document";
            runMode.InsertParentValue = "<SourceDocName>";

            // Load the expected data for this test
            IUnknownVector expected = new IUnknownVector();
            expected.LoadFrom(_testFiles.GetFile(_BY_PAGE_UNDER_PARENT_PAGE_NUMBER_VOA), false);

            // Get the sourceDocName for the expected results
            string sourceDocName = GetSourceDocNameFromVector(expected);

            // Run the rules
            IUnknownVector results =
                RunRules(rules, sourceDocName, expected);

            // Results should have only one top level attribute
            Assert.That(results.Size() == 1, "To many top level attributes in results");

            // Create the expected parent attribute
            AttributeCreator attributeCreator = new AttributeCreator(sourceDocName);
            ComAttribute parent = attributeCreator.Create(runMode.InsertParentName, sourceDocName);

            // Get the result parent value
            ComAttribute resultParent = (ComAttribute) results.At(0);

            // Get the sub attributes from the result parent for later comparison
            IUnknownVector resultsSubAttribute = resultParent.SubAttributes;

            // Set the result parent sub attributes to null to compare the result parent to 
            // expected parent
            resultParent.SubAttributes = null;

            // Get the IComparableObject interface for the parents
            IComparableObject p1 = (IComparableObject) parent;
            IComparableObject p2 = (IComparableObject) resultParent;

            // Compare the parents - this does not compare the InstanceGUID
            Assert.That(p1.IsEqualTo(p2), "Parents don't match.");

            // Compare the results sub attributes to expected - InstanceGUIDs should all match
            Assert.That(IsEqual(resultsSubAttribute, expected, true), "Output is does not match expected.");
        }

        [Test, Category("RuleSetRunMode")]
        public static void Test12_RunModePassInputVOAtoOutputDeepCopy()
        {
            RuleSet rules = GetBaseRuleSet();
            IRunMode runMode = (IRunMode)rules;
            runMode.RunMode = ERuleSetRunMode.kPassInputVOAToOutput;
            runMode.DeepCopyInput = true;

            // Load the expected data for this test
            IUnknownVector expected = new IUnknownVector();
            expected.LoadFrom(_testFiles.GetFile(_BY_PAGE_UNDER_PARENT_PAGE_NUMBER_VOA), false);

            // Run the rules
            IUnknownVector results =
                RunRules(rules, GetSourceDocNameFromVector(expected), expected);

            // Verify the results
            Assert.That(IsEqual(results, expected, false), "Results do not match expected.");
        }

        [Test, Category("RuleSetRunMode")]
        public static void Test13_RunModePassInputVOAtoOutputUnderParentSourceDocNameDeepCopy()
        {
            RuleSet rules = GetBaseRuleSet();
            IRunMode runMode = (IRunMode)rules;
            runMode.RunMode = ERuleSetRunMode.kPassInputVOAToOutput;
            runMode.DeepCopyInput = true;
            runMode.InsertAttributesUnderParent = true;
            runMode.InsertParentName = "Document";
            runMode.InsertParentValue = "<SourceDocName>";

            // Load the expected data for this test
            IUnknownVector expected = new IUnknownVector();
            expected.LoadFrom(_testFiles.GetFile(_BY_PAGE_UNDER_PARENT_PAGE_NUMBER_VOA), false);

            // Get the sourceDocName for the expected results
            string sourceDocName = GetSourceDocNameFromVector(expected);

            // Run the rules
            IUnknownVector results =
                RunRules(rules, sourceDocName, expected);

            // Results should have only one top level attribute
            Assert.That(results.Size() == 1, "To many top level attributes in results");

            // Create the expected parent attribute
            AttributeCreator attributeCreator = new AttributeCreator(sourceDocName);
            ComAttribute parent = attributeCreator.Create(runMode.InsertParentName, sourceDocName);

            // Get the result parent
            ComAttribute resultParent = (ComAttribute)results.At(0);

            // Get the sub attributes from the result parent for later comparison
            IUnknownVector resultsSubAttribute = resultParent.SubAttributes;
            
            // Set the result parent sub attributes to null to compare the result parent to 
            // expected parent
            resultParent.SubAttributes = null;

            // Get the IComparableObject interface for the parents
            IComparableObject p1 = (IComparableObject)parent;
            IComparableObject p2 = (IComparableObject)resultParent;

            // Compare the parents - this does not compare the InstanceGUID
            Assert.That(p1.IsEqualTo(p2), "Parents don't match.");

            // Compare the results sub attributes to expected - InstanceGUIDs should not match
            Assert.That(IsEqual(resultsSubAttribute, expected, false), "Results do not match expected.");
        }

        #endregion Tests

        #region Private methods
        
        static RuleSet GetBaseRuleSet()
        {
            var baseRuleSet = new RuleSet();
            baseRuleSet.LoadFrom(_testFiles.GetFile(_RUN_MODE_TEST_RSD_FILE), false);
            return baseRuleSet;
        }

        static RuleSet GetLoadTestFromOldRuleSet()
        {
            var loadTestRuleSet = new RuleSet();
            loadTestRuleSet.LoadFrom(_testFiles.GetFile(_RUN_MODE_LOAD_TEST_FROM_OLD_RSD_FILE), false);
            return loadTestRuleSet;
        }

        static AFDocument GetTestInput()
        {
            SpatialString docText = new SpatialString();
            docText.LoadFrom(_testFiles.GetFile(_TEST_IMAGE_USS_FILE), false);

            AFDocument doc = new AFDocument();
            doc.Text = docText;
            return doc;
        }

        static IUnknownVector RunRules(RuleSet rules, string sourceDocName, 
            IUnknownVector attributesToPassIn = null)
        {
            StrToObjectMap attributes = rules.AttributeNameToInfoMap;
            AFDocument afDoc = GetTestInput();

            // Set the SourceDocName for the AFDocument text to the expected sourceDocName
            afDoc.Text.SourceDocName = sourceDocName;

            if (attributesToPassIn != null)
            {
                // Clone with the InstanceGUID
                ICloneIdentifiableObject ipClone = (ICloneIdentifiableObject)attributesToPassIn;
                afDoc.Attribute.SubAttributes = (IUnknownVector) ipClone.CloneIdentifiableObject();
            }

            // Run the rules
            return rules.ExecuteRulesOnText(afDoc, attributes.GetKeys(), null, null);
        }

        static string GetSourceDocNameFromVector(IUnknownVector expected)
        {
            string sourceDocName = "";
            
            // the expected is null nothing to do
            if (expected == null)
            {
                return sourceDocName;
            }
            
            // Search the results for the first attribute with Source doc name set
            for (int i = 0; i < expected.Size(); i++)
            {
                ComAttribute a = (ComAttribute)expected.At(i);
                if (a.Value.HasSpatialInfo())
                {
                    sourceDocName = a.Value.SourceDocName;
                }
                else
                {
                    // Call recursively on subattributes
                    sourceDocName = GetSourceDocNameFromVector(a.SubAttributes);
                }
                if (sourceDocName != "")
                {
                    return sourceDocName;
                }
            }
            return sourceDocName;
        }

        static bool IsEqual(IUnknownVector a1, IUnknownVector a2, bool guidsMatch = false)
        {
            // Iterate through one vector and search the other if found remove from second vector
            for (int i1 = 0; i1 < a1.Size(); i1++)
            {
                IComparableObject a = (IComparableObject)a1.At(i1);
                bool found = false;
                for (int i2 = 0; i2 < a2.Size(); i2++)
                {
                    IComparableObject b = (IComparableObject)a2.At(i2);
                    if (a.IsEqualTo(b) && !(guidsMatch ^ ((IIdentifiableObject)a).InstanceGUID ==
                        ((IIdentifiableObject)b).InstanceGUID))
                    {
                        a2.Remove(i2);
                        found = true;
                        break;
                    }
                }

                // if the element in a1 was not found in a2 return false
                if (!found)
                {
                    return false;
                }
            }
            return a2.Size() == 0;
        }

        static void TestDefaultRunModeSettings(IRunMode runMode, string messagePrefix)
        {
            Assert.That(runMode.RunMode == ERuleSetRunMode.kRunPerDocument,
                messagePrefix + "RunMode should be kRunPerDocument.");

            Assert.That(runMode.InsertAttributesUnderParent == false,
                messagePrefix + "InsertAttributesUnderParent should be false.");

            Assert.That(runMode.InsertParentName == "",
                messagePrefix + "InsertParentName should be empty.");

            Assert.That(runMode.InsertParentValue == "",
                messagePrefix + "InsertParentValue should be empty.");

            Assert.That(runMode.DeepCopyInput == false,
                messagePrefix + "DeepCopyInput should be false.");
        }

        static void ChangeAllSettingsFromDefault(IRunMode runMode)
        {
            // These settings should be set to something other than the default
            runMode.RunMode = ERuleSetRunMode.kPassInputVOAToOutput;
            runMode.InsertAttributesUnderParent = true;
            runMode.InsertParentName = "Page";
            runMode.InsertParentValue = "<PageNumber>";
            runMode.DeepCopyInput = true;
        }

        static void TestChangedSettings(IRunMode runMode, string messageSuffix)
        {
            Assert.That(runMode.RunMode == ERuleSetRunMode.kPassInputVOAToOutput,
                "RunMode " + messageSuffix);

            Assert.That(runMode.InsertAttributesUnderParent == true,
                "InsertAttributeUnderParent" + messageSuffix);

            Assert.That(runMode.InsertParentName == "Page",
                "InsertParentName" + messageSuffix);

            Assert.That(runMode.InsertParentValue == "<PageNumber>",
                "InsertParentValue" + messageSuffix);

            Assert.That(runMode.DeepCopyInput == true,
                "DeepCopyInput" + messageSuffix);
        }

        #endregion Private methods
    }
}