using AttributeDbMgrComponentsLib;
using Extract.AttributeFinder;
using Extract.FileActionManager.Database.Test;
using Extract.FileActionManager.FileProcessors;
using Extract.Testing.Utilities;
using Extract.Utilities;
using Extract.Utilities.FSharp;
using Extract.Utilities.FSharp.NERAnnotation;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.UtilityApplications.NERAnnotation.Test
{
    /// <summary>
    /// Unit tests for NERAnnotator class
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("NERAnnotator")]
    public class TestNERAnnotator
    {
        #region Fields

        /// <summary>
        /// Manages the test data files
        /// </summary>
        static TestFileManager<TestNERAnnotator> _testFiles;
        static List<string> _inputFolder = new List<string>();

        /// <summary>
        /// Manages test FAM DBs.
        /// </summary>
        static FAMTestDBManager<TestNERAnnotator> _testDbManager;

        static readonly string _DB_NAME = "_TestNERAnnotator_2DB1BD2B-2352-4F4D-AA62-AB215603B1C3";
        static readonly string _ATTRIBUTE_SET_NAME = "Expected";
        static readonly string _STORE_ATTRIBUTE_GUID = typeof(StoreAttributesInDBTask).GUID.ToString();
        static readonly string _MODEL_NAME = "Test";

        static readonly string _GET_MLDATA =
            @"SELECT Data FROM MLData
            JOIN MLModel ON MLData.MLModelID = MLModel.ID
                WHERE Name = @Name
                AND IsTrainingData = @IsTrainingData";

        #endregion Fields

        #region Overhead

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [TestFixtureSetUp]

        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testFiles = new TestFileManager<TestNERAnnotator>();
            _testDbManager = new FAMTestDBManager<TestNERAnnotator>();
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

            // This class is haunted; The first temp folder keeps existing after it has been deleted so to
            // safe, remove them from the list so as not to attempt to delete them more than once if you run
            // test twice (they do go away after closing nunit).
            for (int i = _inputFolder.Count; i > 0;)
            {
                Directory.Delete(_inputFolder[--i], true);
                _inputFolder.RemoveAt(i);
            }

            if (_testDbManager != null)
            {
                _testDbManager.Dispose();
                _testDbManager = null;
            }
        }
        #endregion Overhead

        #region Helper Functions

        // Helper function to put resource test files into folders
        // These images are from Demo_FlexIndex
        private static void SetFiles()
        {
            _inputFolder.Add(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
            Directory.CreateDirectory(_inputFolder.Last());
            Directory.CreateDirectory(Path.Combine(_inputFolder.Last(), "Train"));
            Directory.CreateDirectory(Path.Combine(_inputFolder.Last(), "Test"));

            var tokenFile = Path.Combine(_inputFolder.Last(), "en-token.nlp.etf");
            _testFiles.GetFile("Resources.en-token.nlp.etf", tokenFile);
            var sentenceFile = Path.Combine(_inputFolder.Last(), "en-sent.nlp.etf");
            _testFiles.GetFile("Resources.en-sent.nlp.etf", sentenceFile);
            var trainList = Path.Combine(_inputFolder.Last(), "train.txt");
            _testFiles.GetFile("Resources.train.txt", trainList);
            var testList = Path.Combine(_inputFolder.Last(), "test.txt");
            _testFiles.GetFile("Resources.test.txt", testList);
            var overlappingExpectedList = Path.Combine(_inputFolder.Last(), "overlapping_expected.txt");
            _testFiles.GetFile("Resources.overlapping_expected.txt", overlappingExpectedList);
            var scriptFile = Path.Combine(_inputFolder.Last(), "NERUtils.fsx");
            _testFiles.GetFile("Resources.NERUtils.fsx", scriptFile);

            for (int i = 1; i <= 10; i++)
            {
                var baseResourceName = "Resources.Example{0:D2}.tif{1}";
                var baseName = "Example{0:D2}.tif{1}";

                string resourceName = string.Format(CultureInfo.CurrentCulture, baseResourceName, i, "");
                string fileName = string.Format(CultureInfo.CurrentCulture, baseName, i, "");
                string path = Path.Combine(_inputFolder.Last(), (i <= 5 ? "Test" : "Train"), fileName);
                _testFiles.GetFile(resourceName, path);

                resourceName = string.Format(CultureInfo.CurrentCulture, baseResourceName, i, ".uss");
                fileName = string.Format(CultureInfo.CurrentCulture, baseName, i, ".uss");
                path = Path.Combine(_inputFolder.Last(), (i <= 5 ? "Test" : "Train"), fileName);
                _testFiles.GetFile(resourceName, path);

                resourceName = string.Format(CultureInfo.CurrentCulture, baseResourceName, i, ".evoa");
                fileName = string.Format(CultureInfo.CurrentCulture, baseName, i, ".evoa");
                path = Path.Combine(_inputFolder.Last(), (i <= 5 ? "Test" : "Train"), fileName);
                _testFiles.GetFile(resourceName, path);

                resourceName = string.Format(CultureInfo.CurrentCulture, baseResourceName, i, ".overlapping.evoa");
                if (_testFiles.ResourceExists(resourceName))
                {
                    fileName = string.Format(CultureInfo.CurrentCulture, baseName, i, ".overlapping.evoa");
                    path = Path.Combine(_inputFolder.Last(), (i <= 5 ? "Test" : "Train"), fileName);
                    _testFiles.GetFile(resourceName, path);
                }

                resourceName = string.Format(CultureInfo.CurrentCulture, baseResourceName, i, ".voa");
                if (_testFiles.ResourceExists(resourceName))
                {
                    fileName = string.Format(CultureInfo.CurrentCulture, baseName, i, ".voa");
                    path = Path.Combine(_inputFolder.Last(), (i <= 5 ? "Test" : "Train"), fileName);
                    _testFiles.GetFile(resourceName, path);
                }
            }
        }

        private static void CreateDatabase()
        {
            var fileProcessingDB = _testDbManager.GetNewDatabase(_DB_NAME);
            fileProcessingDB.DefineNewAction("a");
            fileProcessingDB.DefineNewMLModel(_MODEL_NAME);
            var attributeDBMgr = new AttributeDBMgr
            {
                FAMDB = fileProcessingDB
            };
            attributeDBMgr.CreateNewAttributeSetName(_ATTRIBUTE_SET_NAME);
            var afutility = new UCLID_AFUTILSLib.AFUtility();
            fileProcessingDB.RecordFAMSessionStart("DUMMY", "a", true, true);

            var files = Directory.GetFiles(_inputFolder.Last() + "\\Train", "*.tif");
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                var rec = fileProcessingDB.AddFile(file, "a", -1, EFilePriority.kPriorityNormal, false, false, EActionStatus.kActionPending, false,
                    out var _, out var _);
                var voaData = afutility.GetAttributesFromFile(file + ".evoa");
                int fileTaskSessionID = fileProcessingDB.StartFileTaskSession(_STORE_ATTRIBUTE_GUID, rec.FileID, rec.ActionID);
                attributeDBMgr.CreateNewAttributeSetForFile(fileTaskSessionID, _ATTRIBUTE_SET_NAME, voaData, false, true, true,
                    closeConnection: i == (files.Length - 1));
            }

            fileProcessingDB.RecordFAMSessionStop();
            fileProcessingDB.CloseAllDBConnections();
        }

        private static void CompareExpectedFileToFoundText(string expectedFile, string output)
        {
            var expected = File.ReadAllText(expectedFile);
            Assert.AreEqual(expected, output);
        }

        private static void CompareExpectedFileToFoundFile(string expectedFile, string outputFile)
        {
            var output = File.ReadAllText(outputFile);
            CompareExpectedFileToFoundText(expectedFile, output);
        }

        #endregion Helper Functions

        #region Tests

        // Test OpenNLP format without sentence detection, with a learnable tokenizer
        [Test, Category("NERAnnotator")]
        public static void OpenNlpNoSentLearnableTokenizer()
        {
            SetFiles();
            var settingsFile = Path.Combine(_inputFolder.Last(), "opennlp.no_sent.learnable_tok.annotator");
            _testFiles.GetFile("Resources.opennlp.no_sent.learnable_tok.annotator", settingsFile);
            var settings = NERAnnotatorSettings.LoadFrom(settingsFile);
            NERAnnotator.Process(settings, _ => { }, CancellationToken.None);
            Directory.SetCurrentDirectory(settings.WorkingDir);

            var expectedFile = _testFiles.GetFile("Resources.opennlp.no_sent.learnable_tok.train.txt");
            var trainingOutputFile = settings.OutputFileBaseName + ".train.txt";
            CompareExpectedFileToFoundFile(expectedFile, trainingOutputFile);

            expectedFile = _testFiles.GetFile("Resources.opennlp.no_sent.learnable_tok.test.txt");
            var testingOutputFile = settings.OutputFileBaseName + ".test.txt";
            CompareExpectedFileToFoundFile(expectedFile, testingOutputFile);
        }

        // OpenNLP format with sentence detection and learnable tokenizer
        [Test, Category("NERAnnotator")]
        public static void OpenNlpSentLearnableTokenizer()
        {
            SetFiles();
            var settingsFile = Path.Combine(_inputFolder.Last(), "opennlp.sent.learnable_tok.annotator");
            _testFiles.GetFile("Resources.opennlp.sent.learnable_tok.annotator", settingsFile);
            var settings = NERAnnotatorSettings.LoadFrom(settingsFile);
            NERAnnotator.Process(settings, _ => { }, CancellationToken.None);
            Directory.SetCurrentDirectory(settings.WorkingDir);

            var expectedFile = _testFiles.GetFile("Resources.opennlp.sent.learnable_tok.train.txt");
            var trainingOutputFile = settings.OutputFileBaseName + ".train.txt";
            CompareExpectedFileToFoundFile(expectedFile, trainingOutputFile);

            expectedFile = _testFiles.GetFile("Resources.opennlp.sent.learnable_tok.test.txt");
            var testingOutputFile = settings.OutputFileBaseName + ".test.txt";
            CompareExpectedFileToFoundFile(expectedFile, testingOutputFile);
        }

        // OpenNLP format with sentence detection and simple tokenizer
        [Test, Category("NERAnnotator")]
        public static void OpenNlpSentSimpleTokenizer()
        {
            SetFiles();
            var settingsFile = Path.Combine(_inputFolder.Last(), "opennlp.sent.simple_tok.annotator");
            _testFiles.GetFile("Resources.opennlp.sent.simple_tok.annotator", settingsFile);
            var settings = NERAnnotatorSettings.LoadFrom(settingsFile);
            NERAnnotator.Process(settings, _ => { }, CancellationToken.None);
            Directory.SetCurrentDirectory(settings.WorkingDir);

            var expectedFile = _testFiles.GetFile("Resources.opennlp.sent.simple_tok.train.txt");
            var trainingOutputFile = settings.OutputFileBaseName + ".train.txt";
            CompareExpectedFileToFoundFile(expectedFile, trainingOutputFile);

            expectedFile = _testFiles.GetFile("Resources.opennlp.sent.simple_tok.test.txt");
            var testingOutputFile = settings.OutputFileBaseName + ".test.txt";
            CompareExpectedFileToFoundFile(expectedFile, testingOutputFile);
        }

        // OpenNLP format with whitespace tokenizer
        [Test, Category("NERAnnotator")]
        public static void OpenNlpSentWhiteSpaceTokenizer()
        {
            SetFiles();
            var settingsFile = Path.Combine(_inputFolder.Last(), "opennlp.sent.whitespace_tok.annotator");
            _testFiles.GetFile("Resources.opennlp.sent.whitespace_tok.annotator", settingsFile);
            var settings = NERAnnotatorSettings.LoadFrom(settingsFile);
            NERAnnotator.Process(settings, _ => { }, CancellationToken.None);
            Directory.SetCurrentDirectory(settings.WorkingDir);

            var expectedFile = _testFiles.GetFile("Resources.opennlp.sent.whitespace_tok.train.txt");
            var trainingOutputFile = settings.OutputFileBaseName + ".train.txt";
            CompareExpectedFileToFoundFile(expectedFile, trainingOutputFile);

            expectedFile = _testFiles.GetFile("Resources.opennlp.sent.whitespace_tok.test.txt");
            var testingOutputFile = settings.OutputFileBaseName + ".test.txt";
            CompareExpectedFileToFoundFile(expectedFile, testingOutputFile);
        }

        // Verify that all text is in the output
        [Test, Category("NERAnnotator")]
        public static void VerifyThatNoTextIsLost()
        {
            SetFiles();
            var settingsFile = Path.Combine(_inputFolder.Last(), "opennlp.sent.whitespace_tok.annotator");
            _testFiles.GetFile("Resources.opennlp.sent.whitespace_tok.annotator", settingsFile);
            var settings = NERAnnotatorSettings.LoadFrom(settingsFile);
            settings.PercentToUseForTestingSet = 0;
            settings.PercentUninterestingPagesToInclude = 100;
            settings.SplitIntoSentences = false;
            NERAnnotator.Process(settings, _ => { }, CancellationToken.None);

            Directory.SetCurrentDirectory(settings.WorkingDir);
            var uss = new SpatialStringClass();
            var expected = string.Join("\r\n\r\n",
                Directory.GetFiles(settings.TrainingInput, "*.uss", SearchOption.AllDirectories)
                .Select(ussPath =>
                {
                    uss.LoadFrom(ussPath, false);
                    return string.Join("\r\n", uss.GetPages(false, "").ToIEnumerable<SpatialString>()
                        .Select(page => Regex.Replace(page.String, @"[\r\n]+", " ").TrimEnd()));
                }));
            var trainingOutputFile = settings.OutputFileBaseName + ".train.txt";
            // Remove tags and trailing space
            var trainingOutput = Regex.Replace(File.ReadAllText(trainingOutputFile), @"<START:\w+>\x20?|\x20?<END>|[\r\n]+\z", "");
            Assert.AreEqual(expected, trainingOutput);
        }

        // Test that specifying a different random seed for set splitting changes the output
        [Test, Category("NERAnnotator")]
        public static void OpenNlpDifferentTestSetRandomSeed()
        {
            SetFiles();
            var settingsFile = Path.Combine(_inputFolder.Last(), "opennlp.sent.whitespace_tok.annotator");
            _testFiles.GetFile("Resources.opennlp.sent.whitespace_tok.annotator", settingsFile);
            var settings = NERAnnotatorSettings.LoadFrom(settingsFile);
            settings.RandomSeedForSetDivision = 1;
            NERAnnotator.Process(settings, _ => { }, CancellationToken.None);
            Directory.SetCurrentDirectory(settings.WorkingDir);

            var expectedFile = _testFiles.GetFile("Resources.opennlp.sent.whitespace_tok.train.txt");
            var expected = File.ReadAllText(expectedFile);
            var trainingOutputFile = settings.OutputFileBaseName + ".train.txt";
            var trainingOutput = File.ReadAllText(trainingOutputFile);
            CollectionAssert.AreNotEqual(expected, trainingOutput);
        }

        // Test that specifying no random seed for set splitting changes the output
        [Test, Category("NERAnnotator")]
        public static void OpenNlpNoTestSetRandomSeed()
        {
            var passed = false;
            var tried = 0;
            var maxTries = 3;
            do
            {
                tried++;
                try
                {
                    SetFiles();
                    var settingsFile = Path.Combine(_inputFolder.Last(), "opennlp.sent.whitespace_tok.annotator");
                    _testFiles.GetFile("Resources.opennlp.sent.whitespace_tok.annotator", settingsFile);
                    var settings = NERAnnotatorSettings.LoadFrom(settingsFile);
                    settings.RandomSeedForSetDivision = null;
                    NERAnnotator.Process(settings, _ => { }, CancellationToken.None);
                    Directory.SetCurrentDirectory(settings.WorkingDir);

                    var expectedFile = _testFiles.GetFile("Resources.opennlp.sent.whitespace_tok.train.txt");
                    var expected = File.ReadAllText(expectedFile);
                    var trainingOutputFile = settings.OutputFileBaseName + ".train.txt";
                    var trainingOutput = File.ReadAllText(trainingOutputFile);
                    Assert.AreNotEqual(expected, trainingOutput);
                    passed = true;
                }
                catch
                {
                    if (tried >= maxTries)
                    {
                        throw;
                    }
                }
            }
            while (!passed && tried < maxTries);
        }

        // OpenNLP format with an image list as input instead of a directory
        [Test, Category("NERAnnotator")]
        public static void OpenNlpTrainList()
        {
            SetFiles();
            var settingsFile = Path.Combine(_inputFolder.Last(), "opennlp.train_list.annotator");
            _testFiles.GetFile("Resources.opennlp.train_list.annotator", settingsFile);
            var settings = NERAnnotatorSettings.LoadFrom(settingsFile);
            NERAnnotator.Process(settings, _ => { }, CancellationToken.None);
            Directory.SetCurrentDirectory(settings.WorkingDir);

            var expectedFile = _testFiles.GetFile("Resources.opennlp.train_list.train.txt");
            var trainingOutputFile = settings.OutputFileBaseName + ".train.txt";
            CompareExpectedFileToFoundFile(expectedFile, trainingOutputFile);

            expectedFile = _testFiles.GetFile("Resources.opennlp.train_list.test.txt");
            var testingOutputFile = settings.OutputFileBaseName + ".test.txt";
            CompareExpectedFileToFoundFile(expectedFile, testingOutputFile);
        }

        // With both a training list and a testing list specified
        [Test, Category("NERAnnotator")]
        public static void OpenNlpBothLists()
        {
            SetFiles();
            var settingsFile = Path.Combine(_inputFolder.Last(), "opennlp.both_lists.annotator");
            _testFiles.GetFile("Resources.opennlp.both_lists.annotator", settingsFile);
            var settings = NERAnnotatorSettings.LoadFrom(settingsFile);
            NERAnnotator.Process(settings, _ => { }, CancellationToken.None);
            Directory.SetCurrentDirectory(settings.WorkingDir);

            var expectedFile = _testFiles.GetFile("Resources.opennlp.both_lists.train.txt");
            var trainingOutputFile = settings.OutputFileBaseName + ".train.txt";
            CompareExpectedFileToFoundFile(expectedFile, trainingOutputFile);

            expectedFile = _testFiles.GetFile("Resources.opennlp.both_lists.test.txt");
            var testingOutputFile = settings.OutputFileBaseName + ".test.txt";
            CompareExpectedFileToFoundFile(expectedFile, testingOutputFile);
        }

        // Both training and testing dirs are specified (results will be the same as previous test, where both lists are specified)
        [Test, Category("NERAnnotator")]
        public static void OpenNlpBothDirs()
        {
            SetFiles();
            var settingsFile = Path.Combine(_inputFolder.Last(), "opennlp.both_dirs.annotator");
            _testFiles.GetFile("Resources.opennlp.both_dirs.annotator", settingsFile);
            var settings = NERAnnotatorSettings.LoadFrom(settingsFile);
            NERAnnotator.Process(settings, _ => { }, CancellationToken.None);
            Directory.SetCurrentDirectory(settings.WorkingDir);

            var expectedFile = _testFiles.GetFile("Resources.opennlp.both_lists.train.txt");
            var trainingOutputFile = settings.OutputFileBaseName + ".train.txt";
            CompareExpectedFileToFoundFile(expectedFile, trainingOutputFile);

            expectedFile = _testFiles.GetFile("Resources.opennlp.both_lists.test.txt");
            var testingOutputFile = settings.OutputFileBaseName + ".test.txt";
            CompareExpectedFileToFoundFile(expectedFile, testingOutputFile);
        }

        // Specify each category explicitly with a different query that matches only one type
        [Test, Category("NERAnnotator")]
        public static void OpenNlpExplicitCategories()
        {
            SetFiles();
            var settingsFile = Path.Combine(_inputFolder.Last(), "opennlp.explicit_categories.annotator");
            _testFiles.GetFile("Resources.opennlp.explicit_categories.annotator", settingsFile);
            var settings = NERAnnotatorSettings.LoadFrom(settingsFile);
            NERAnnotator.Process(settings, _ => { }, CancellationToken.None);
            Directory.SetCurrentDirectory(settings.WorkingDir);

            var expectedFile = _testFiles.GetFile("Resources.opennlp.sent.learnable_tok.train.txt");
            var trainingOutputFile = settings.OutputFileBaseName + ".train.txt";
            CompareExpectedFileToFoundFile(expectedFile, trainingOutputFile);

            expectedFile = _testFiles.GetFile("Resources.opennlp.sent.learnable_tok.test.txt");
            var testingOutputFile = settings.OutputFileBaseName + ".test.txt";
            CompareExpectedFileToFoundFile(expectedFile, testingOutputFile);
        }

        // Test that an exception is generated when an output file already exists
        [Test, Category("NERAnnotator")]
        public static void FailIfOutputFileExists()
        {
            SetFiles();
            var settingsFile = Path.Combine(_inputFolder.Last(), "opennlp.explicit_categories.annotator");
            _testFiles.GetFile("Resources.opennlp.explicit_categories.annotator", settingsFile);
            var settings = NERAnnotatorSettings.LoadFrom(settingsFile);
            NERAnnotator.Process(settings, _ => { }, CancellationToken.None);

            // Now that the training and testing output files exist, the process will not run
            Assert.Throws<ExtractException>(() => NERAnnotator.Process(settings, _ => { }, CancellationToken.None));

            // With the training output deleted the process still won't run
            Directory.SetCurrentDirectory(settings.WorkingDir);
            var trainingOutputFile = settings.OutputFileBaseName + ".train.txt";
            File.Delete(trainingOutputFile);

            Assert.Throws<ExtractException>(() => NERAnnotator.Process(settings, _ => { }, CancellationToken.None));

            // With both files deleted it will run
            var testingOutputFile = settings.OutputFileBaseName + ".test.txt";
            File.Delete(testingOutputFile);

            Assert.DoesNotThrow(() => NERAnnotator.Process(settings, _ => { }, CancellationToken.None));

            // Run again without FailIfOutputFileExists and verify that the output file is appended to
            settings.FailIfOutputFileExists = false;
            var testingOutput = File.ReadAllText(testingOutputFile);
            var testingOutputSize = testingOutput.Length;
            Assert.DoesNotThrow(() => NERAnnotator.Process(settings, _ => { }, CancellationToken.None));
            testingOutput = File.ReadAllText(testingOutputFile);

            Assert.AreEqual(testingOutputSize * 2, testingOutput.Length);
        }

        // Verify that no testing file need be created (all pages can be put into a single output file)
        [Test, Category("NERAnnotator")]
        public static void SingleOutputFileAllowed()
        {
            SetFiles();
            var settingsFile = Path.Combine(_inputFolder.Last(), "opennlp.explicit_categories.annotator");
            _testFiles.GetFile("Resources.opennlp.explicit_categories.annotator", settingsFile);
            var settings = NERAnnotatorSettings.LoadFrom(settingsFile);
            settings.TestingSet = TestingSetType.Specified;
            settings.TestingInput = "";
            NERAnnotator.Process(settings, _ => { }, CancellationToken.None);
            Directory.SetCurrentDirectory(settings.WorkingDir);

            var expectedFile = _testFiles.GetFile("Resources.opennlp.single_output_file.train.txt");
            var trainingOutputFile = settings.OutputFileBaseName + ".train.txt";
            CompareExpectedFileToFoundFile(expectedFile, trainingOutputFile);

            var testingOutputFile = settings.OutputFileBaseName + ".test.txt";
            Assert.IsFalse(File.Exists(testingOutputFile));
        }

        // Verify that setting the % to use for testing set to zero works the same as explicitly specifying no testing set name
        [Test, Category("NERAnnotator")]
        public static void SingleOutputFileAllowed2()
        {
            SetFiles();
            var settingsFile = Path.Combine(_inputFolder.Last(), "opennlp.explicit_categories.annotator");
            _testFiles.GetFile("Resources.opennlp.explicit_categories.annotator", settingsFile);
            var settings = NERAnnotatorSettings.LoadFrom(settingsFile);
            settings.PercentToUseForTestingSet = 0;
            NERAnnotator.Process(settings, _ => { }, CancellationToken.None);
            Directory.SetCurrentDirectory(settings.WorkingDir);

            var expectedFile = _testFiles.GetFile("Resources.opennlp.single_output_file.train.txt");
            var trainingOutputFile = settings.OutputFileBaseName + ".train.txt";
            CompareExpectedFileToFoundFile(expectedFile, trainingOutputFile);

            var testingOutputFile = settings.OutputFileBaseName + ".test.txt";
            Assert.IsFalse(File.Exists(testingOutputFile));
        }

        // Verify that output is larger if 100 percent of 'uninteresting' pages are output
        [Test, Category("NERAnnotator")]
        public static void OutputAllUninterestingPages()
        {
            SetFiles();
            var settingsFile = Path.Combine(_inputFolder.Last(), "opennlp.explicit_categories.annotator");
            _testFiles.GetFile("Resources.opennlp.explicit_categories.annotator", settingsFile);
            var settings = NERAnnotatorSettings.LoadFrom(settingsFile);
            settings.PercentToUseForTestingSet = 0;
            settings.PercentUninterestingPagesToInclude = 100;
            NERAnnotator.Process(settings, _ => { }, CancellationToken.None);
            Directory.SetCurrentDirectory(settings.WorkingDir);

            var expectedFile = _testFiles.GetFile("Resources.opennlp.single_output_file.train.txt");
            var expected = File.ReadAllText(expectedFile);
            var trainingOutputFile = settings.OutputFileBaseName + ".train.txt";
            var trainingOutput = File.ReadAllText(trainingOutputFile);
            Assert.Greater(trainingOutput.Length, expected.Length);
            Assert.AreEqual(113939, trainingOutput.Length);
        }

        // Verify that output changes for different page-inclusion random seed values
        [Test, Category("NERAnnotator")]
        public static void OutputSomeUninterestingPages()
        {
            SetFiles();
            var settingsFile = Path.Combine(_inputFolder.Last(), "opennlp.explicit_categories.annotator");
            _testFiles.GetFile("Resources.opennlp.explicit_categories.annotator", settingsFile);
            var settings = NERAnnotatorSettings.LoadFrom(settingsFile);
            settings.PercentToUseForTestingSet = 0;
            settings.PercentUninterestingPagesToInclude = 50;
            NERAnnotator.Process(settings, _ => { }, CancellationToken.None);
            Directory.SetCurrentDirectory(settings.WorkingDir);

            // Ensure that more pages are included than when 0% uninteresting pages are included
            var expectedFile = _testFiles.GetFile("Resources.opennlp.single_output_file.train.txt");
            var expected = File.ReadAllText(expectedFile);
            var trainingOutputFile = settings.OutputFileBaseName + ".train.txt";
            var trainingOutput = File.ReadAllText(trainingOutputFile);
            Assert.Greater(trainingOutput.Length, expected.Length);

            // Ensure that less than 100% have been output
            Assert.Less(trainingOutput.Length, 114005);

            File.Delete(trainingOutputFile);
            settings.RandomSeedForPageInclusion = 1;
            NERAnnotator.Process(settings, _ => { }, CancellationToken.None);
            var trainingOutput2 = File.ReadAllText(trainingOutputFile);
            Assert.AreNotEqual(trainingOutput, trainingOutput2);

            var tried = 0;
            var maxTries = 3;
            var passed = false;
            do
            {
                try
                {
                    tried++;
                    File.Delete(trainingOutputFile);
                    settings.RandomSeedForPageInclusion = null;
                    NERAnnotator.Process(settings, _ => { }, CancellationToken.None);
                    var trainingOutput3 = File.ReadAllText(trainingOutputFile);
                    CollectionAssert.AreNotEqual(trainingOutput, trainingOutput3);
                    CollectionAssert.AreNotEqual(trainingOutput2, trainingOutput3);
                    passed = true;
                }
                catch
                {
                    if (tried >= maxTries)
                    {
                        throw;
                    }
                }
            }
            while (!passed && tried < maxTries);
        }

        // Test behavior of overlapping attributes/entities
        [Test, Category("NERAnnotator")]
        public static void OverlappingEntities()
        {
            SetFiles();
            var settingsFile = Path.Combine(_inputFolder.Last(), "opennlp.overlapping_expected.annotator");
            _testFiles.GetFile("Resources.opennlp.overlapping_expected.annotator", settingsFile);
            var settings = NERAnnotatorSettings.LoadFrom(settingsFile);
            NERAnnotator.Process(settings, _ => { }, CancellationToken.None);

            // Verify tags
            Directory.SetCurrentDirectory(settings.WorkingDir);
            var expectedFile = _testFiles.GetFile("Resources.opennlp.overlapping_expected.train.txt");
            var trainingOutputFile = settings.OutputFileBaseName + ".train.txt";
            CompareExpectedFileToFoundFile(expectedFile, trainingOutputFile);

            // Confirm that no (non-whitespace) text is lost
            var uss = new SpatialStringClass();
            var ussPath = Path.GetFullPath(@"Test\Example01.tif.uss");
            uss.LoadFrom(ussPath, false);
            var expected = string.Join("", uss.GetPages(false, "")
                                .ToIEnumerable<SpatialString>()
                                // Remove whitespace so that the raw input can be compared to the tokenized input
                                .Select(page => Regex.Replace(page.String, @"\s+", "")));

            // Remove tags and whitespace so that the tokenized output (which has extra whitespace) matches the expected, which has all whitespace removed
            var trainingOutput = Regex.Replace(File.ReadAllText(trainingOutputFile), @"<START:\w+>|<END>|\s+", "");
            Assert.AreEqual(expected, trainingOutput);
        }

        // Test non-default ValueQuery
        [Test, Category("NERAnnotator")]
        public static void ValueQuery()
        {
            SetFiles();
            var settingsFile = Path.Combine(_inputFolder.Last(), "opennlp.flex.annotator");
            _testFiles.GetFile("Resources.opennlp.flex.annotator", settingsFile);
            var settings = NERAnnotatorSettings.LoadFrom(settingsFile);
            NERAnnotator.Process(settings, _ => { }, CancellationToken.None);

            // Verify tags
            Directory.SetCurrentDirectory(settings.WorkingDir);
            var expectedFile = _testFiles.GetFile("Resources.opennlp.flex.train.txt");
            var trainingOutputFile = settings.OutputFileBaseName + ".train.txt";
            CompareExpectedFileToFoundFile(expectedFile, trainingOutputFile);
        }

        // Test Database mode
        [Test, Category("NERAnnotator")]
        public static void DatabaseMode()
        {
            try
            {
                SetFiles();
                CreateDatabase();
                var settingsFile = Path.Combine(_inputFolder.Last(), "opennlp.train_list.annotator");
                _testFiles.GetFile("Resources.opennlp.train_list.annotator", settingsFile);
                var settings = NERAnnotatorSettings.LoadFrom(settingsFile);
                settings.UseDatabase = true;
                settings.DatabaseServer = "(local)";
                settings.DatabaseName = _DB_NAME;
                settings.FirstIDToProcess = 0;
                settings.LastIDToProcess = 10;
                settings.AttributeSetName = _ATTRIBUTE_SET_NAME;
                settings.ModelName = _MODEL_NAME;
                NERAnnotator.Process(settings, _ => { }, CancellationToken.None);

                // Verify tags
                var expectedFile = _testFiles.GetFile("Resources.opennlp.train_list.train.txt");

                // Build the connection string from the settings
                SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder
                {
                    DataSource = "(local)",
                    InitialCatalog = _DB_NAME,
                    IntegratedSecurity = true,
                    NetworkLibrary = "dbmssocn"
                };

                string trainingOutput = null;
                using (var connection = new SqlConnection(sqlConnectionBuild.ConnectionString))
                {
                    connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = _GET_MLDATA;
                        cmd.Parameters.AddWithValue("@Name", _MODEL_NAME);
                        cmd.Parameters.AddWithValue("@IsTrainingData", true);

                        var lines = new List<string>();
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lines.Add(reader.GetString(0));
                            }

                            trainingOutput = string.Join("\r\n", lines) + "\r\n";
                            reader.Close();
                        }
                    }
                    connection.Close();
                }
                CompareExpectedFileToFoundText(expectedFile, trainingOutput);
            }
            finally
            {
                _testDbManager.RemoveDatabase(_DB_NAME);
            }
        }

        // While tweaking the code that determines token inclusion I accidentally caused this name to get truncated
        // so I've added a test to make sure it is found correctly (first + middle initial--last name doesn't OCR)
        [Test, Category("NERAnnotator")]
        public static void IncludeAllOfEntity()
        {
            SetFiles();
            var serializedSettings = @"
TypesVoaFunction: <SourceDocName>.voa
PercentUninterestingPagesToInclude: 0
RandomSeedForPageInclusion: 0
Format: OpenNLP
SplitIntoSentences: true
SentenceDetectionModelPath: en-sent.nlp.etf
TokenizerType: WhitespaceTokenizer
EntityDefinitions:
- Category: '@Type'
  CategoryIsXPath: true
  RootQuery: /*/HCData|/*/MCData|/*/LCData|/*/Manual
";
            var settings = NERAnnotatorSettings.Load(serializedSettings);
            settings.WorkingDir = _inputFolder.Last();

            var ussFile = Path.Combine(_inputFolder.Last(), "Test", "Example02.tif.uss");
            var uss = new SpatialStringClass();
            uss.LoadFrom(ussFile, false);

            var voaFile = Path.Combine(_inputFolder.Last(), "Test", "Example02.tif.voa");
            var voa = new IUnknownVectorClass();
            voa.LoadFrom(voaFile, false);

            EntitiesAndPage empty(EntitiesAndPage x) => new EntitiesAndPage(entities: ListModule.Empty<Entity>(), page: x.Page);
            var labeled = NERAnnotator.GetTokensForPage(settings, uss, 7, voa, _=>_, _=>_, empty, _=>_)
                .Where(t => !String.IsNullOrWhiteSpace(t.Label))
                .ToList();
            Assert.AreEqual(2, labeled.Count);
            Assert.AreEqual("John", labeled[0].Token);
            Assert.AreEqual("D", labeled[1].Token);
        }

        // There was a mistake in the code that was causing single-character tokens to be dropped
        // Fixing this caused extra garbage lines to get included sometimes. This tests that at least some
        // extra lines are avoided by other means now
        [Test, Category("NERAnnotator")]
        public static void AvoidExtraEntities()
        {
            SetFiles();
            var serializedSettings = @"
TypesVoaFunction: <SourceDocName>.voa
PercentUninterestingPagesToInclude: 0
RandomSeedForPageInclusion: 0
Format: OpenNLP
SplitIntoSentences: true
SentenceDetectionModelPath: en-sent.nlp.etf
TokenizerType: WhitespaceTokenizer
EntityDefinitions:
- Category: '@Type'
  CategoryIsXPath: true
  RootQuery: /*/HCData|/*/MCData|/*/LCData|/*/Manual
";
            var settings = NERAnnotatorSettings.Load(serializedSettings);
            settings.WorkingDir = _inputFolder.Last();

            var ussFile = Path.Combine(_inputFolder.Last(), "Test", "Example05.tif.uss");
            var uss = new SpatialStringClass();
            uss.LoadFrom(ussFile, false);

            var voaFile = Path.Combine(_inputFolder.Last(), "Test", "Example05.tif.voa");
            var voa = new IUnknownVectorClass();
            voa.LoadFrom(voaFile, false);

            EntitiesAndPage empty(EntitiesAndPage x) =>
                new EntitiesAndPage(entities: ListModule.Empty<Entity>(), page: x.Page);
            var labeled = NERAnnotator.GetTokensForPage(settings, uss, 2, voa, _=>_, _=>_, empty, _=>_)
                .Where(t => !String.IsNullOrWhiteSpace(t.Label))
                .ToList();
            Assert.AreEqual(3, labeled.Count);
            Assert.AreEqual("i", labeled[0].Token);
            Assert.AreEqual("--Kityc-e,(jese,", labeled[1].Token);
            Assert.AreEqual("4,4-0,:ez,", labeled[2].Token);
        }

        // There was a mistake in the code that was causing single-character tokens to be dropped
        // Fixing this caused extra garbage lines to get included sometimes. This tests that at least some
        // extra lines are avoided by other means now
        [Test, Category("NERAnnotator")]
        public static void AvoidExtraEntities2()
        {
            SetFiles();
            var serializedSettings = @"
TypesVoaFunction: <SourceDocName>.voa
PercentUninterestingPagesToInclude: 0
RandomSeedForPageInclusion: 0
Format: OpenNLP
SplitIntoSentences: true
SentenceDetectionModelPath: en-sent.nlp.etf
TokenizerType: WhitespaceTokenizer
EntityDefinitions:
- Category: '@Type'
  CategoryIsXPath: true
  RootQuery: /*/HCData|/*/MCData|/*/LCData|/*/Manual
";
            var settings = NERAnnotatorSettings.Load(serializedSettings);
            settings.WorkingDir = _inputFolder.Last();

            var ussFile = Path.Combine(_inputFolder.Last(), "Train", "Example08.tif.uss");
            var uss = new SpatialStringClass();
            uss.LoadFrom(ussFile, false);

            var voaFile = Path.Combine(_inputFolder.Last(), "Train", "Example08.tif.voa");
            var voa = new IUnknownVectorClass();
            voa.LoadFrom(voaFile, false);

            EntitiesAndPage empty(EntitiesAndPage x) =>
                new EntitiesAndPage(entities: ListModule.Empty<Entity>(), page: x.Page);

            var labeled = NERAnnotator.GetTokensForPage(settings, uss, 1, voa, _=>_, _=>_, empty, _=>_)
                .Where(t => !String.IsNullOrWhiteSpace(t.Label))
                .ToList();

            // After fixing mistake, part of the "WITNESS" text above this signature was being included
            Assert.AreEqual(2, labeled.Count);
            Assert.AreEqual("1.-74-.,4.Li", labeled[0].Token);
            Assert.AreEqual("7-jf-,0-71-1", labeled[1].Token);
        }

        // Tests that the 'limit to finishable' function could be used to adjust the zones to avoid garbage lines
        [Test, Category("NERAnnotator")]
        public static void AvoidExtraEntitiesWithFunctions()
        {
            SetFiles();
            var serializedSettings = @"
TypesVoaFunction: <SourceDocName>.voa
PercentUninterestingPagesToInclude: 0
RandomSeedForPageInclusion: 0
Format: OpenNLP
SplitIntoSentences: true
SentenceDetectionModelPath: en-sent.nlp.etf
TokenizerType: WhitespaceTokenizer
EntityDefinitions:
- Category: '@Type'
  CategoryIsXPath: true
  RootQuery: /*/HCData|/*/MCData|/*/LCData|/*/Manual
";
            var settings = NERAnnotatorSettings.Load(serializedSettings);
            settings.WorkingDir = _inputFolder.Last();

            var ussFile = Path.Combine(_inputFolder.Last(), "Test", "Example05.tif.uss");
            var uss = new SpatialStringClass();
            uss.LoadFrom(ussFile, false);

            var voaFile = Path.Combine(_inputFolder.Last(), "Test", "Example05.tif.voa");
            var voa = new IUnknownVectorClass();
            voa.LoadFrom(voaFile, false);

            EntitiesAndPage empty(EntitiesAndPage x) =>
                new EntitiesAndPage(entities: ListModule.Empty<Entity>(), page: x.Page);

            var labeled = NERAnnotator.GetTokensForPage(settings, uss, 2, voa,
                preprocess: _=>_,
                setExpectedValuesFromDefinitions: _=>_,
                resolveToPage: empty,
                limitToFinishable: _=>_)
                .Where(t => !String.IsNullOrWhiteSpace(t.Label))
                .ToList();
            Assert.Greater(labeled.Count, 2);

            // Function to filter and modify entities to only have high confidence (or at least several chars-long) zones
            EntitiesAndPage highConf(EntitiesAndPage x) =>
                new EntitiesAndPage(
                    entities: x.Entities
                    .Where(e => e.SpatialString.Value != null)
                    .Select(e =>
                    {
                        var s = e.SpatialString.Value;
                        var lines = s.GetLines()
                        .ToIEnumerable<SpatialString>()
                        .Where(line =>
                        {
                            var str = line.String;
                            if (str.Where(c => !Char.IsWhiteSpace(c)).Count() > 4)
                            {
                                return true;
                            }

                            int _ = 0, avg = 0;
                            line.GetCharConfidence(ref _, ref _, ref avg);
                            return avg > 60;
                        })
                        .ToList();
                        if (lines.Count == 0)
                        {
                            return null;
                        }

                        s.CreateFromSpatialStrings(lines.ToIUnknownVector(), false);

                        return new Entity(expectedValue: e.ExpectedValue,
                            zones: s.GetOCRImageRasterZones().ToIEnumerable<RasterZone>().ToFSharpList(),
                            valueComponents: e.ValueComponents,
                            spatialString: FSharpOption<SpatialString>.None,
                            category: e.Category);
                    })
                    .Where(e => e != null)
                    .ToFSharpList()
                    , page: x.Page);

            labeled = NERAnnotator.GetTokensForPage(settings, uss, 2, voa,
                preprocess: _=>_,
                setExpectedValuesFromDefinitions: _=>_,
                resolveToPage: empty,
                limitToFinishable: highConf)
                .Where(t => !String.IsNullOrWhiteSpace(t.Label))
                .ToList();
            Assert.AreEqual(2, labeled.Count);
            Assert.AreEqual("--Kityc-e,(jese,", labeled[0].Token);
            Assert.AreEqual("4,4-0,:ez,", labeled[1].Token);
        }

        // Tests that the preprocess function is applied
        [Test, Category("NERAnnotator")]
        public static void Preprocess()
        {
            SetFiles();
            var serializedSettings = @"
TypesVoaFunction: <SourceDocName>.voa
PercentUninterestingPagesToInclude: 100
RandomSeedForPageInclusion: 0
Format: OpenNLP
SplitIntoSentences: true
SentenceDetectionModelPath: en-sent.nlp.etf
TokenizerType: WhitespaceTokenizer
EntityDefinitions:
- Category: '@Type'
  CategoryIsXPath: true
  RootQuery: /*/HCData|/*/MCData|/*/LCData|/*/Manual
";
            var settings = NERAnnotatorSettings.Load(serializedSettings);
            settings.WorkingDir = _inputFolder.Last();

            var ussFile = Path.Combine(_inputFolder.Last(), "Train", "Example06.tif.uss");
            var uss = new SpatialStringClass();
            uss.LoadFrom(ussFile, false);

            var voa = new IUnknownVectorClass();

            AFDocument sortText(AFDocument doc)
            {
                var lines = doc.Text.GetLines();
                if (lines.Size() > 0)
                {
                    var sorter = new SpatiallyCompareStringsClass();
                    lines.Sort(sorter);
                    doc.Text.CreateFromSpatialStrings(lines, true);
                }
                return doc;
            }

            EntitiesAndPage empty(EntitiesAndPage x) =>
                new EntitiesAndPage(entities: ListModule.Empty<Entity>(), page: x.Page);

            var tokensWithoutSort = NERAnnotator.GetTokensForPage(settings, uss, 2, voa,
                preprocess: _ => _,
                setExpectedValuesFromDefinitions: _ => _,
                resolveToPage: empty,
                limitToFinishable: _ => _)
                .ToList();

            var tokensWithSort = NERAnnotator.GetTokensForPage(settings, uss, 2, voa,
                preprocess: sortText,
                setExpectedValuesFromDefinitions: _ => _,
                resolveToPage: empty,
                limitToFinishable: _ => _)
                .ToList();

            Assert.AreEqual(tokensWithSort.Count, tokensWithoutSort.Count);

            int differentAt = -1;
            for (int i = 0; i < tokensWithoutSort.Count; i++)
            {
                if (!tokensWithoutSort[i].Equals(tokensWithSort[i]))
                {
                    differentAt = i;
                    break;
                }
            }
            Assert.Greater(differentAt, 0);

            var sortedTokensText = String.Join(" ", tokensWithSort.Select(t => t.Token));
            var unsortedTokensText = String.Join(" ", tokensWithoutSort.Select(t => t.Token));

            Assert.That(Regex.IsMatch(sortedTokensText, "My Commission Expires.*Notary Public"));
            Assert.That(Regex.IsMatch(unsortedTokensText, "Notary Public.*My Commission Expires"));
        }

        // Tests that the preprocess function specified in the settings is applied
        [Test, Category("NERAnnotator")]
        public static void PreprocessFromScript()
        {
            SetFiles();
            var serializedSettings = @"
TypesVoaFunction: <SourceDocName>.evoa
PercentUninterestingPagesToInclude: 100
RandomSeedForPageInclusion: 0
Format: OpenNLP
SplitIntoSentences: true
SentenceDetectionModelPath: en-sent.nlp.etf
TokenizerType: WhitespaceTokenizer
EntityDefinitions:
- Category: '@Type'
  CategoryIsXPath: true
  RootQuery: /*/HCData|/*/MCData|/*/LCData|/*/Manual
RunPreprocessingFunction: false
PreprocessingScript: NERUtils.fsx
PreprocessingFunctionName: AFDoc.sortText
";
            var settings = NERAnnotatorSettings.Load(serializedSettings);
            settings.WorkingDir = _inputFolder.Last();

            var ussFile = Path.Combine(_inputFolder.Last(), "Train", "Example06.tif.uss");

            var recordsWithoutSortOption = NERAnnotator.GetRecordsForPages(settings, ussFile, 2);
            Assert.That(FSharpOption<(string, string)>.get_IsSome(recordsWithoutSortOption));
            var recordsWithoutSort = recordsWithoutSortOption.Value.data;

            settings.RunPreprocessingFunction = true;
            var recordsWithSortOption = NERAnnotator.GetRecordsForPages(settings, ussFile, 2);
            Assert.That(FSharpOption<(string, string)>.get_IsSome(recordsWithSortOption));
            var recordsWithSort = recordsWithSortOption.Value.data;

            Assert.That(Regex.IsMatch(recordsWithSort, "My Commission Expires.*Notary Public"));
            Assert.That(Regex.IsMatch(recordsWithoutSort, "Notary Public.*My Commission Expires"));
        }

        // Tests that the character replace function is applied
        [Test, Category("NERAnnotator")]
        public static void CharacterReplace()
        {
            SetFiles();
            var serializedSettings = @"
TypesVoaFunction: <SourceDocName>.evoa
PercentUninterestingPagesToInclude: 100
RandomSeedForPageInclusion: 0
Format: OpenNLP
SplitIntoSentences: true
SentenceDetectionModelPath: en-sent.nlp.etf
TokenizerType: WhitespaceTokenizer
EntityDefinitions:
- Category: '@Type'
  CategoryIsXPath: true
  RootQuery: /*/HCData|/*/MCData|/*/LCData|/*/Manual
RunCharacterReplacingFunction: false
CharacterReplacingScript: NERUtils.fsx
CharacterReplacingFunctionName: CharReplacement.makeDatesGeneric
";
            var settings = NERAnnotatorSettings.Load(serializedSettings);
            settings.WorkingDir = _inputFolder.Last();

            var ussFile = Path.Combine(_inputFolder.Last(), "Train", "Example06.tif.uss");

            // Check for numbers
            var records = NERAnnotator.GetRecordsForPages(settings, ussFile, 1, 2);
            Assert.That(FSharpOption<(string, string)>.get_IsSome(records));
            Assert.That(Regex.IsMatch(records.Value.data, "[0-8]"));
            Assert.AreEqual(13, Regex.Matches(records.Value.data, "9").Count);

            // Check that numbers have been replaced with 9s
            settings.RunCharacterReplacingFunction = true;
            records = NERAnnotator.GetRecordsForPages(settings, ussFile, 1, 2);
            Assert.That(FSharpOption<(string, string)>.get_IsSome(records));
            Assert.That(!Regex.IsMatch(records.Value.data, "[0-8]"));
            Assert.AreEqual(231, Regex.Matches(records.Value.data, "9").Count);
        }

        // Tests that non-spatial entities are found
        [Test, Category("NERAnnotator")]
        public static void NonSpatialExpected()
        {
            SetFiles();
            var serializedSettings = @"
TypesVoaFunction: <SourceDocName>.voa
PercentUninterestingPagesToInclude: 100
RandomSeedForPageInclusion: 0
Format: OpenNLP
SplitIntoSentences: true
SentenceDetectionModelPath: en-sent.nlp.etf
TokenizerType: WhitespaceTokenizer
EntityDefinitions:
- Category: PatientName
  CategoryIsXPath: false
  RootQuery: /*/HCData|/*/MCData|/*/LCData|/*/Manual
  ValueQuery: First|Middle|Last
RunEntityFilteringFunctions: true
EntityFilteringScript: NERUtils.fsx
";
            var settings = NERAnnotatorSettings.Load(serializedSettings);
            settings.WorkingDir = _inputFolder.Last();

            var ussFile = Path.Combine(_inputFolder.Last(), "Train", "Example07.tif.uss");
            var voaFile = Path.Combine(_inputFolder.Last(), "Train", "Example07.tif.voa");

            // Ensure preconditions--expected file has no spatial attributes
            var voa = new IUnknownVectorClass();
            voa.LoadFrom(voaFile, false);
            Assert.That(voa.Size() == 1 && !((IAttribute)voa.At(0)).EnumerateDepthFirst().Any(a => a.Value.HasSpatialInfo()));

            // Check for numbers
            var records = NERAnnotator.GetRecordsForPages(settings, ussFile, 1);
            Assert.That(FSharpOption<(string, string)>.get_IsSome(records));
            Assert.That(Regex.IsMatch(records.Value.data, "<START:PatientName> JOHN D DOE <END>"));
        }

        #endregion Tests
    }
}