using Extract.AttributeFinder.Rules;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;


namespace Extract.AttributeFinder.Test
{
    /// <summary>
    /// Test cases for TranslateToBestMatch rule object. This is not meant to be comprehensive at this time but to include
    /// test cases for new features.
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("TranslateToBestMatch")]
    public class TestTranslateToBestMatch
    {
        #region Fields

        /// <summary>
        /// Manages the test files used by this test
        /// </summary>
        static TestFileManager<TestTranslateToBestMatch> _testFiles;

        #endregion Fields

        #region Setup and Teardown
        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testFiles = new TestFileManager<TestTranslateToBestMatch>();
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

            // Prevent nunit error after tests are run:
            // "Unhandled Exception: NUnit.Engine.NUnitEngineException: Remote test agent exited with non-zero exit code -2146233020"
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        #endregion Setup and Teardown

        #region Public Test Functions

        /// <summary>
        /// Real test cases from MN Rice that successfully translate
        /// </summary>
        [Test, Category("Automated")]        
        public static void TestCorrectTranslations()
        {
            var testDataCSV = _testFiles.GetFile("Resources.TranslateToBestMatch.mn_rice_correct.csv");

            List<(bool match, string input, string output, string groundTruth)> results = ProcessTestCases(testDataCSV);
            var failures = results.Where(t => !t.match).ToList();
            CollectionAssert.AreEqual(Enumerable.Empty<List<(bool, string, string, string)>>(), failures);
        }

        /// <summary>
        /// Real test cases from MN Rice where the input equals the output
        /// </summary>
        [Test, Category("Automated")]        
        public static void TestIdentityTranslations()
        {
            var testDataCSV = _testFiles.GetFile("Resources.TranslateToBestMatch.mn_rice_identity.csv");

            List<(bool match, string input, string output, string groundTruth)> results = ProcessTestCases(testDataCSV);
            var failures = results.Where(t => !t.match).ToList();
            CollectionAssert.AreEqual(Enumerable.Empty<List<(bool, string, string, string)>>(), failures);
        }

        /// <summary>
        /// Real test cases from MN Rice that are not successfully translated (a superset of items that perhaps could be successfully translated)
        /// </summary>
        [Test, Category("Automated")]        
        public static void TestIncorrectTranslations()
        {
            var testDataCSV = _testFiles.GetFile("Resources.TranslateToBestMatch.mn_rice_incorrect.csv");

            List<(bool match, string input, string output, string groundTruth)> results = ProcessTestCases(testDataCSV);
            var successes = results.Where(t => t.match).ToList();

            CollectionAssert.AreEqual(Enumerable.Empty<List<(bool, string, string, string)>>(), successes, "This is actually a good thing!");
        }

        /// <summary>
        /// Artificial test cases based on MN Rice data
        /// </summary>
        [Test, Category("Automated")]        
        public static void TestMultiwordSynonymsSmallToLarge()
        {
            var testDataCSV = _testFiles.GetFile("Resources.TranslateToBestMatch.artificial1.csv");
            var subs = _testFiles.GetFile("Resources.TranslateToBestMatch.artificial_subdivisions1.dat");
            var results = ProcessTestCases(testDataCSV, subs);
            var failures = results.Where(t => !t.match).ToList();

            CollectionAssert.AreEqual(Enumerable.Empty<List<(bool, string, string, string)>>(), failures);
        }

        /// <summary>
        /// Artificial test cases based on MN Rice data
        /// </summary>
        [Test, Category("Automated")]        
        public static void TestMultiwordSynonymsLargeToSmall()
        {
            var testDataCSV = _testFiles.GetFile("Resources.TranslateToBestMatch.artificial2.csv");
            var subs = _testFiles.GetFile("Resources.TranslateToBestMatch.artificial_subdivisions2.dat");
            var results = ProcessTestCases(testDataCSV, subs);
            var failures = results.Where(t => !t.match).ToList();

            CollectionAssert.AreEqual(Enumerable.Empty<List<(bool, string, string, string)>>(), failures);
        }

        /// <summary>
        /// Test that a missing targets file with existing synonyms file fails for the right reason
        /// </summary>
        [Test, Category("Automated")]        
        public static void TestMissingTargetsFile()
        {
            var testDataCSV = _testFiles.GetFile("Resources.TranslateToBestMatch.artificial2.csv");
            var targetsResource = "Resources.TranslateToBestMatch.artificial_subdivisions2.dat";
            var synonymsResource = "Resources.TranslateToBestMatch.synonyms.dat";
            var targets = _testFiles.GetFile(targetsResource);
            var synonyms = _testFiles.GetFile(synonymsResource);

            _testFiles.RemoveFile(targetsResource);
            var ex = Assert.Throws<ExtractException>(() => ProcessTestCases(testDataCSV, targets, synonyms));
            ex = ExtractException.FromStringizedByteStream(ex.EliCode, ex.Message);
            var rootCause = ex.InnerException as ExtractException;
            StringAssert.AreEqualIgnoringCase("Failed to read file", rootCause?.Message);
            StringAssert.AreEqualIgnoringCase(targets, rootCause?.Data["Filename"].ToString());
        }

        /// <summary>
        /// Test that a missing encrypted targets file with existing synonyms file fails for the right reason
        /// </summary>
        [Test, Category("Automated")]        
        public static void TestMissingEncryptedTargetsFile()
        {
            var testDataCSV = _testFiles.GetFile("Resources.TranslateToBestMatch.artificial2.csv");
            var targetsResource = "Resources.TranslateToBestMatch.artificial_subdivisions2.dat";
            var synonymsResource = "Resources.TranslateToBestMatch.synonyms.dat";
            var targets = _testFiles.GetFile(targetsResource) + ".etf";
            var synonyms = _testFiles.GetFile(synonymsResource);

            _testFiles.RemoveFile(targetsResource);
            var ex = Assert.Throws<ExtractException>(() => ProcessTestCases(testDataCSV, targets, synonyms));
            ex = ExtractException.FromStringizedByteStream(ex.EliCode, ex.Message);
            var rootCause = ex.InnerException as ExtractException;
            StringAssert.AreEqualIgnoringCase("Unable to read text file.", rootCause?.Message);
            StringAssert.AreEqualIgnoringCase(targets, rootCause?.Data["File To Read"].ToString());
        }

        /// <summary>
        /// Test that a missing synonyms file with existing targets file fails for the right reason
        /// </summary>
        [Test, Category("Automated")]        
        public static void TestMissingSynonymsFile()
        {
            var testDataCSV = _testFiles.GetFile("Resources.TranslateToBestMatch.artificial2.csv");
            var targetsResource = "Resources.TranslateToBestMatch.artificial_subdivisions2.dat";
            var synonymsResource = "Resources.TranslateToBestMatch.synonyms.dat";
            var targets = _testFiles.GetFile(targetsResource);
            var synonyms = _testFiles.GetFile(synonymsResource);

            _testFiles.RemoveFile(synonymsResource);
            var ex = Assert.Throws<ExtractException>(() => ProcessTestCases(testDataCSV, targets, synonyms));
            ex = ExtractException.FromStringizedByteStream(ex.EliCode, ex.Message);
            var rootCause = ex.InnerException as ExtractException;
            StringAssert.AreEqualIgnoringCase("Failed to read file", rootCause?.Message);
            StringAssert.AreEqualIgnoringCase(synonyms, rootCause?.Data["Filename"].ToString());
        }

        /// <summary>
        /// Test that a missing, encrypted, synonyms file with existing targets file fails for the right reason
        /// </summary>
        [Test, Category("Automated")]        
        public static void TestMissingEncryptedSynonymsFile()
        {
            var testDataCSV = _testFiles.GetFile("Resources.TranslateToBestMatch.artificial2.csv");
            var targetsResource = "Resources.TranslateToBestMatch.artificial_subdivisions2.dat";
            var synonymsResource = "Resources.TranslateToBestMatch.synonyms.dat";
            var targets = _testFiles.GetFile(targetsResource);
            var synonyms = _testFiles.GetFile(synonymsResource) + ".etf";

            _testFiles.RemoveFile(synonymsResource);
            var ex = Assert.Throws<ExtractException>(() => ProcessTestCases(testDataCSV, targets, synonyms));
            ex = ExtractException.FromStringizedByteStream(ex.EliCode, ex.Message);
            var rootCause = ex.InnerException as ExtractException;
            StringAssert.AreEqualIgnoringCase("Unable to read text file.", rootCause?.Message);
            StringAssert.AreEqualIgnoringCase(synonyms, rootCause?.Data["File To Read"].ToString());
        }

        #endregion

        #region Private Methods

        private static List<(bool match, string input, string output, string groundTruth)> ProcessTestCases(string testDataCSV, string subs = null, string syns = null)
        {
            subs = subs ?? _testFiles.GetFile("Resources.TranslateToBestMatch.subdivisions.dat");
            syns = syns ?? _testFiles.GetFile("Resources.TranslateToBestMatch.synonyms.dat");
            var testData = new List<(string input, string output)>();
            using (var csvReader = new Microsoft.VisualBasic.FileIO.TextFieldParser(testDataCSV))
            {
                csvReader.Delimiters = new[] { "," };
                while (!csvReader.EndOfData)
                {
                    var fields = csvReader.ReadFields();
                    testData.Add((input: fields[0], output: fields[1]));
                }
            }

            var ac = new AttributeCreator("");
            var attributes = testData.Select(t => ac.Create("_", t.input)).ToIUnknownVector();
            var translator = new TranslateValueToBestMatch { SourceListPath = subs, SynonymMapPath = syns, UnableToTranslateAction = NoGoodMatchAction.ClearValue };
            translator.ProcessOutput(attributes, new AFDocumentClass(), null);
            return attributes.ToIEnumerable<IAttribute>()
                .Zip(testData, (found, testCase) =>
                    (match: testCase.output == found.Value.String, input: testCase.input, output: found.Value.String, groundTruth: testCase.output))
                .ToList();
        }

        #endregion
    }
}
