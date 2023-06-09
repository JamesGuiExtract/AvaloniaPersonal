﻿using Extract.AttributeFinder.Rules;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
    [Category("RuleSetRunMode")]
    public class TestRuleSetRunMode
    {
        #region Constants

        /// <summary>
        /// The embedded resource rsd file used as a base for running rules with differing settings
        /// </summary>
        static readonly string _RUN_MODE_TEST_RSD_FILE = "Resources.RunModeTest.rsd";

        /// <summary>
        /// The embedded resource rsd files used to test running as a splitter
        /// https://extract.atlassian.net/browse/ISSUE-14297
        /// </summary>
        static readonly string _TWO_LAYERS_RSD_FILE = "Resources.TwoLayers.rsd";
        static readonly string _INSERT_UNDER_PARENT_RSD_FILE = "Resources.insertUnderParent.rsd";

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
        /// The embedded resource tif file needed to run rules
        /// </summary>
        static readonly string _TEST_IMAGE_FILE = "Resources.image3-4-6-7.tif";

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
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestRuleSetRunMode>();

        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [OneTimeTearDown]
        public static void FinalCleanup()
        {
            if (_testFiles != null)
            {
                _testFiles.Dispose();
            }
        }

        #endregion Overhead

        #region Tests

        [Test, Category("Automated")]
        public static void Test01_RunModeDefaultSettings()
        {
            RuleSet rules = new RuleSet();
            IRunMode runMode = (IRunMode)rules;

            // Verify defaults
            TestDefaultRunModeSettings(runMode, "Default after create ");
        }

        [Test, Category("Automated")]
        public static void Test02_RunModeDefaultSettingsAfterLoadOfOldVersion()
        {
            // Load older version RuleSet
            RuleSet rules = GetLoadTestFromOldRuleSet();
            IRunMode runMode = (IRunMode)rules;
            
            TestDefaultRunModeSettings(runMode, "Default after load of old version ");
        }

        [Test, Category("Automated")]
        public static void Test03_RunModeChangeSettings()
        {
            RuleSet rules = new RuleSet();
            IRunMode runMode = (IRunMode)rules;

            ChangeAllSettingsFromDefault(runMode);

            TestChangedSettings(runMode, "setting failed.");
        }

        [Test, Category("Automated")]
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

        [Test, Category("Automated")]
        public static void Test05_RunModeByDocument()
        {
            RuleSet rules = GetBaseRuleSet();
            IRunMode runMode = (IRunMode)rules;

            runMode.RunMode = ERuleSetRunMode.kRunPerDocument;

            // Load the expected data for this test
            IUnknownVector expected = new IUnknownVector();
            expected.LoadFrom(_testFiles.GetFile(_BY_DOC_VOA), false);

            // Run the rules
            var sourceDocAndResults = RunRules(rules);
            string sourceDocName = sourceDocAndResults.Item1;
            IUnknownVector results = sourceDocAndResults.Item2;

            // Update source doc name of expected to be temp file location
            UpdateSourceDocName(expected, sourceDocName);
            
            // Verify the results
            Assert.That(IsEqual(results, expected), "Results do not match expected.");
        }

        [Test, Category("Automated")]
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
            var sourceDocAndResults = RunRules(rules);
            string sourceDocName = sourceDocAndResults.Item1;
            IUnknownVector results = sourceDocAndResults.Item2;

            // Update source doc name of expected to be temp file location
            UpdateSourceDocName(expected, sourceDocName);

            // Fix incorrect path in expected top-level node's value
            ((ComAttribute)expected.At(0)).Value.ReplaceAndDowngradeToNonSpatial(sourceDocName);

            // Verify the results
            Assert.That(IsEqual(results, expected), "Results do not match expected.");
        }

        [Test, Category("Automated")]
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
            var sourceDocAndResults = RunRules(rules);
            string sourceDocName = sourceDocAndResults.Item1;
            IUnknownVector results = sourceDocAndResults.Item2;

            // Update source doc name of expected to be temp file location
            UpdateSourceDocName(expected, sourceDocName);

            // Verify the results
            Assert.That(IsEqual(results, expected), "Results do not match expected.");
        }

        [Test, Category("Automated")]
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
            var sourceDocAndResults = RunRules(rules);
            string sourceDocName = sourceDocAndResults.Item1;
            IUnknownVector results = sourceDocAndResults.Item2;

            // Update source doc name of expected to be temp file location
            UpdateSourceDocName(expected, sourceDocName);

            // Verify the results
            Assert.That(IsEqual(results, expected), "Results do not match expected.");
        }

        [Test, Category("Automated")]
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
            var sourceDocAndResults = RunRules(rules);
            string sourceDocName = sourceDocAndResults.Item1;
            IUnknownVector results = sourceDocAndResults.Item2;

            // Update source doc name of expected to be temp file location
            UpdateSourceDocName(expected, sourceDocName);

            // Verify the results
            Assert.That(IsEqual(results, expected), "Results do not match expected.");
        }

        [Test, Category("Automated")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ato")]
        public static void Test10_RunModePassInputVOAtoOutput()
        {
            RuleSet rules = GetBaseRuleSet();
            IRunMode runMode = (IRunMode)rules;
            runMode.RunMode = ERuleSetRunMode.kPassInputVOAToOutput;

            // Load the expected data for this test
            IUnknownVector expected = new IUnknownVector();
            expected.LoadFrom(_testFiles.GetFile(_BY_PAGE_UNDER_PARENT_PAGE_NUMBER_VOA), false);

            // Run the rules
            var sourceDocAndResults = RunRules(rules, expected);
            IUnknownVector results = sourceDocAndResults.Item2;

            // Verify the results
            Assert.That(IsEqual(results, expected, true), "Results do not match expected.");
        }

        [Test, Category("Automated")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ato")]
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

            // Run the rules
            var sourceDocAndResults = RunRules(rules, expected);
            string sourceDocName = sourceDocAndResults.Item1;
            IUnknownVector results = sourceDocAndResults.Item2;

            // Update source doc name of expected to be temp file location
            UpdateSourceDocName(expected, sourceDocName);

            // Update source doc name of results (since they are the expected attributes passed through the ruleset)
            UpdateSourceDocName(results, sourceDocName);

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

        [Test, Category("Automated")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ato")]
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
            var sourceDocAndResults = RunRules(rules, expected);
            string sourceDocName = sourceDocAndResults.Item1;
            IUnknownVector results = sourceDocAndResults.Item2;

            // Update source doc name of expected to be temp file location
            UpdateSourceDocName(expected, sourceDocName);

            // Update source doc name of results (since they are the expected attributes passed through the ruleset)
            UpdateSourceDocName(results, sourceDocName);

            // Verify the results
            Assert.That(IsEqual(results, expected, false), "Results do not match expected.");
        }

        [Test, Category("Automated")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ato")]
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

            // Run the rules
            var sourceDocAndResults = RunRules(rules, expected);
            string sourceDocName = sourceDocAndResults.Item1;
            IUnknownVector results = sourceDocAndResults.Item2;

            // Update source doc name of expected to be temp file location
            UpdateSourceDocName(expected, sourceDocName);

            // Update source doc name of results (since they are the expected attributes passed through the ruleset)
            UpdateSourceDocName(results, sourceDocName);

            // Results should have only one top level attribute
            Assert.That(results.Size() == 1, "Too many top level attributes in results");

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

        /// <summary>
        /// Tests that no exception is thrown when running RSD splitter that uses entire-document mode
        /// w/ insert-under-parent against each page of the original document
        /// https://extract.atlassian.net/browse/ISSUE-14297
        /// </summary>
        [Test, Category("Automated")]
        public static void Test14_RSDSplitterEntireDocumentUnderParent()
        {
            var rules = new RuleSet();
            rules.LoadFrom(_testFiles.GetFile(_TWO_LAYERS_RSD_FILE), false);
            _testFiles.GetFile(_INSERT_UNDER_PARENT_RSD_FILE);

            Assert.DoesNotThrow(() => RunRules(rules, null));
        }

        /// <summary>
        /// Add a subattribute to the AFDocument.Attribute. Used by <see cref="Test15_VerifyPreprocessorRunsForPassInputVOAToOutputMode"/>
        /// </summary>
        [CLSCompliant(false)]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Subattribute")]
        public static AFDocument MakeSubattribute(AFDocument document)
        {
            AttributeClass sub = new();
            sub.Name = "AddedByPreprocessor";
            sub.Value.CreateNonSpatialString("Placeholder", document.Text.SourceDocName);
            document.Attribute.SubAttributes.PushBack(sub);

            return document;
        }

        /// <summary>
        /// Confirm that the global preprocessor runs for kPassInputVOAToOutput mode
        /// </summary>
        /// <remarks>
        /// This also relies on/tests the ability to run functions directly from a compiled assembly with the F# Preprocessor
        /// </remarks>
        [Test, Category("Automated")]
        public static void Test15_VerifyPreprocessorRunsForPassInputVOAToOutputMode()
        {
            // Arrange
            RuleSet rules = GetBaseRuleSet();
            IRunMode runMode = (IRunMode)rules;
            runMode.RunMode = ERuleSetRunMode.kPassInputVOAToOutput;

            string thisAssembly = typeof(TestRuleSetRunMode).Assembly.Location;
            string functionName = nameof(TestRuleSetRunMode) + "." + nameof(MakeSubattribute);
            rules.GlobalDocPreprocessor.Object = new FSharpPreprocessor { ScriptPath = thisAssembly, FunctionName = functionName };

            // Act
            var (_, actualAttributes) = RunRules(rules);

            // Assert
            Assert.AreEqual(1, actualAttributes.Size());
            Assert.AreEqual("AddedByPreprocessor", ((IAttribute)actualAttributes.At(0)).Name);
        }

        /// <summary>
        /// Add Document/DocumentData subattributes to the AFDocument.Attribute. Used by <see cref="Test16_RunOnPaginationDocumentMode"/>
        /// </summary>
        [CLSCompliant(false)]
        public static AFDocument MakePaginationHierarchy(AFDocument document)
        {
            var documents = document.Text.GetPages(false, "")
                .ToIEnumerable<SpatialString>()
                .Select(page =>
                {
                    AttributeClass doc = new();
                    doc.Name = "Document";
                    doc.Value.CreateNonSpatialString("N/A", document.Text.SourceDocName);

                    AttributeClass docData = new();
                    docData.Name = "DocumentData";
                    docData.Value = page;
                    doc.SubAttributes.PushBack(docData);

                    return doc;
                })
                .ToIUnknownVector();

            document.Attribute.SubAttributes = documents;

            return document;
        }

        /// <summary>
        /// Confirm that kRunPerPaginationDocument mode works correctly
        /// </summary>
        [Test, Category("RuleSetRunMode")]
        public static void Test16_RunOnPaginationDocumentMode()
        {
            // Arrange
            RuleSet rules = GetBaseRuleSet();
            IRunMode runMode = (IRunMode)rules;
            runMode.RunMode = ERuleSetRunMode.kRunPerPaginationDocument;

            string thisAssembly = typeof(TestRuleSetRunMode).Assembly.Location;
            string functionName = nameof(TestRuleSetRunMode) + "." + nameof(MakePaginationHierarchy);
            rules.GlobalDocPreprocessor.Object = new FSharpPreprocessor { ScriptPath = thisAssembly, FunctionName = functionName };

            // Act
            var (_, actual) = RunRules(rules);

            // Assert
            IUnknownVectorClass expected = new();
            expected.LoadFrom(_testFiles.GetFile("Resources.RunOnPaginationMode.voa"), false);

            Assert.AreEqual(expected.Size(), actual.Size());

            var expectedFlattened = AttributeMethods.EnumerateDepthFirst(expected)
                .Select(attribute => (attribute.Name, attribute.Value.String))
                .ToList();

            var actualFlattened = AttributeMethods.EnumerateDepthFirst(actual)
                .Select(attribute => (attribute.Name, attribute.Value.String))
                .ToList();

            CollectionAssert.AreEqual(expectedFlattened, actualFlattened);
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
            // Get the image so that page count can be obtained
            string filename = _testFiles.GetFile(_TEST_IMAGE_FILE);

            SpatialString docText = new SpatialString();
            docText.LoadFrom(_testFiles.GetFile(_TEST_IMAGE_USS_FILE, filename + ".uss"), false);

            AFDocument doc = new AFDocument();
            doc.Text = docText;
            return doc;
        }

        static Tuple<string, IUnknownVector> RunRules(RuleSet rules,
            IUnknownVector attributesToPassIn = null)
        {
            StrToObjectMap attributes = rules.AttributeNameToInfoMap;
            AFDocument afDoc = GetTestInput();
            string sourceDocName = afDoc.Text.SourceDocName;

            if (attributesToPassIn != null)
            {
                // Clone with the InstanceGUID
                ICloneIdentifiableObject ipClone = (ICloneIdentifiableObject)attributesToPassIn;
                afDoc.Attribute.SubAttributes = (IUnknownVector) ipClone.CloneIdentifiableObject();
            }

            var progressStatus = new ProgressStatusClass();

            // Run the rules
            var result = rules.ExecuteRulesOnText(afDoc, attributes.GetKeys(), null, progressStatus);

            // Confirm that the progress status calculations were correct
            Assert.AreEqual(progressStatus.NumItemsTotal, progressStatus.NumItemsCompleted);

            return Tuple.Create(sourceDocName, result);
        }

        static void UpdateSourceDocName(IUnknownVector voa, string sourceDocName)
        {
            // the voa is null nothing to do
            if (voa == null)
            {
                return;
            }
            
            // Update source doc name for each attribute
            for (int i = 0; i < voa.Size(); i++)
            {
                ComAttribute a = (ComAttribute)voa.At(i);
                if (!string.IsNullOrEmpty(a.Value.SourceDocName))
                    a.Value.SourceDocName = sourceDocName;

                // Call recursively on subattributes
                UpdateSourceDocName(a.SubAttributes, sourceDocName);
            }
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


        [SuppressMessage("Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength")]
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
