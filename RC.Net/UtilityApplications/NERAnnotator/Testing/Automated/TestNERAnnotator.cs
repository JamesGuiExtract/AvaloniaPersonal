using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.UtilityApplications.NERAnnotator.Test
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

            foreach(var dir in _inputFolder.Where(dir => Directory.Exists(dir)))
            {
                Directory.Delete(dir, true);
            }
        }

        // Helper function to build file lists for pagination testing
        // These images are stapled together from Demo_LabDE images
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

                if (i == 1)
                {
                    resourceName = string.Format(CultureInfo.CurrentCulture, baseResourceName, i, ".overlapping.evoa");
                    fileName = string.Format(CultureInfo.CurrentCulture, baseName, i, ".overlapping.evoa");
                    path = Path.Combine(_inputFolder.Last(), (i <= 5 ? "Test" : "Train"), fileName);
                    _testFiles.GetFile(resourceName, path);

                    resourceName = string.Format(CultureInfo.CurrentCulture, baseResourceName, i, ".voa");
                    fileName = string.Format(CultureInfo.CurrentCulture, baseName, i, ".voa");
                    path = Path.Combine(_inputFolder.Last(), (i <= 5 ? "Test" : "Train"), fileName);
                    _testFiles.GetFile(resourceName, path);
                }
            }
        }

        #endregion Overhead

        #region Tests

        // Test OpenNLP format without sentence detection, with a learnable tokenizer
        [Test, Category("NERAnnotator")]
        public static void OpenNlpNoSentLearnableTokenizer()
        {
            SetFiles();
            var settingsFile = Path.Combine(_inputFolder.Last(), "opennlp.no_sent.learnable_tok.annotator");
            _testFiles.GetFile("Resources.opennlp.no_sent.learnable_tok.annotator", settingsFile);
            var settings = Settings.LoadFrom(settingsFile);
            NERAnnotator.Process(settings, _ => { }, CancellationToken.None);
            Directory.SetCurrentDirectory(settings.WorkingDir);

            var expectedFile = _testFiles.GetFile("Resources.opennlp.no_sent.learnable_tok.train.txt");
            var expected = File.ReadAllText(expectedFile);
            var trainingOutputFile = settings.OutputFileBaseName + ".train.txt";
            var trainingOutput = File.ReadAllText(trainingOutputFile);
            Assert.AreEqual(expected, trainingOutput);

            expectedFile = _testFiles.GetFile("Resources.opennlp.no_sent.learnable_tok.test.txt");
            expected = File.ReadAllText(expectedFile);
            var testingOutputFile = settings.OutputFileBaseName + ".test.txt";
            var testingOutput = File.ReadAllText(testingOutputFile);
            Assert.AreEqual(expected, testingOutput);
        }

        // OpenNLP format with sentence detection and learnable tokenizer
        [Test, Category("NERAnnotator")]
        public static void OpenNlpSentLearnableTokenizer()
        {
            SetFiles();
            var settingsFile = Path.Combine(_inputFolder.Last(), "opennlp.sent.learnable_tok.annotator");
            _testFiles.GetFile("Resources.opennlp.sent.learnable_tok.annotator", settingsFile);
            var settings = Settings.LoadFrom(settingsFile);
            NERAnnotator.Process(settings, _ => { }, CancellationToken.None);
            Directory.SetCurrentDirectory(settings.WorkingDir);

            var expectedFile = _testFiles.GetFile("Resources.opennlp.sent.learnable_tok.train.txt");
            var expected = File.ReadAllText(expectedFile);
            var trainingOutputFile = settings.OutputFileBaseName + ".train.txt";
            var trainingOutput = File.ReadAllText(trainingOutputFile);
            Assert.AreEqual(expected, trainingOutput);

            expectedFile = _testFiles.GetFile("Resources.opennlp.sent.learnable_tok.test.txt");
            expected = File.ReadAllText(expectedFile);
            var testingOutputFile = settings.OutputFileBaseName + ".test.txt";
            var testingOutput = File.ReadAllText(testingOutputFile);
            Assert.AreEqual(expected, testingOutput);
        }

        // OpenNLP format with sentence detection and simple tokenizer
        [Test, Category("NERAnnotator")]
        public static void OpenNlpSentSimpleTokenizer()
        {
            SetFiles();
            var settingsFile = Path.Combine(_inputFolder.Last(), "opennlp.sent.simple_tok.annotator");
            _testFiles.GetFile("Resources.opennlp.sent.simple_tok.annotator", settingsFile);
            var settings = Settings.LoadFrom(settingsFile);
            NERAnnotator.Process(settings, _ => { }, CancellationToken.None);
            Directory.SetCurrentDirectory(settings.WorkingDir);

            var expectedFile = _testFiles.GetFile("Resources.opennlp.sent.simple_tok.train.txt");
            var expected = File.ReadAllText(expectedFile);
            var trainingOutputFile = settings.OutputFileBaseName + ".train.txt";
            var trainingOutput = File.ReadAllText(trainingOutputFile);
            Assert.AreEqual(expected, trainingOutput);

            expectedFile = _testFiles.GetFile("Resources.opennlp.sent.simple_tok.test.txt");
            expected = File.ReadAllText(expectedFile);
            var testingOutputFile = settings.OutputFileBaseName + ".test.txt";
            var testingOutput = File.ReadAllText(testingOutputFile);
            Assert.AreEqual(expected, testingOutput);
        }

        // OpenNLP format with whitespace tokenizer
        [Test, Category("NERAnnotator")]
        public static void OpenNlpSentWhiteSpaceTokenizer()
        {
            SetFiles();
            var settingsFile = Path.Combine(_inputFolder.Last(), "opennlp.sent.whitespace_tok.annotator");
            _testFiles.GetFile("Resources.opennlp.sent.whitespace_tok.annotator", settingsFile);
            var settings = Settings.LoadFrom(settingsFile);
            NERAnnotator.Process(settings, _ => { }, CancellationToken.None);
            Directory.SetCurrentDirectory(settings.WorkingDir);

            var expectedFile = _testFiles.GetFile("Resources.opennlp.sent.whitespace_tok.train.txt");
            var expected = File.ReadAllText(expectedFile);
            var trainingOutputFile = settings.OutputFileBaseName + ".train.txt";
            var trainingOutput = File.ReadAllText(trainingOutputFile);
            Assert.AreEqual(expected, trainingOutput);

            expectedFile = _testFiles.GetFile("Resources.opennlp.sent.whitespace_tok.test.txt");
            expected = File.ReadAllText(expectedFile);
            var testingOutputFile = settings.OutputFileBaseName + ".test.txt";
            var testingOutput = File.ReadAllText(testingOutputFile);
            Assert.AreEqual(expected, testingOutput);
        }

        // Verify that all text is in the output
        [Test, Category("NERAnnotator")]
        public static void VerifyThatNoTextIsLost()
        {
            SetFiles();
            var settingsFile = Path.Combine(_inputFolder.Last(), "opennlp.sent.whitespace_tok.annotator");
            _testFiles.GetFile("Resources.opennlp.sent.whitespace_tok.annotator", settingsFile);
            var settings = Settings.LoadFrom(settingsFile);
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
            var settings = Settings.LoadFrom(settingsFile);
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
                    var settings = Settings.LoadFrom(settingsFile);
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
            var settings = Settings.LoadFrom(settingsFile);
            NERAnnotator.Process(settings, _ => { }, CancellationToken.None);
            Directory.SetCurrentDirectory(settings.WorkingDir);

            var expectedFile = _testFiles.GetFile("Resources.opennlp.train_list.train.txt");
            var expected = File.ReadAllText(expectedFile);
            var trainingOutputFile = settings.OutputFileBaseName + ".train.txt";
            var trainingOutput = File.ReadAllText(trainingOutputFile);
            Assert.AreEqual(expected, trainingOutput);

            expectedFile = _testFiles.GetFile("Resources.opennlp.train_list.test.txt");
            expected = File.ReadAllText(expectedFile);
            var testingOutputFile = settings.OutputFileBaseName + ".test.txt";
            var testingOutput = File.ReadAllText(testingOutputFile);
            Assert.AreEqual(expected, testingOutput);
        }

        // With both a training list and a testing list specified
        [Test, Category("NERAnnotator")]
        public static void OpenNlpBothLists()
        {
            SetFiles();
            var settingsFile = Path.Combine(_inputFolder.Last(), "opennlp.both_lists.annotator");
            _testFiles.GetFile("Resources.opennlp.both_lists.annotator", settingsFile);
            var settings = Settings.LoadFrom(settingsFile);
            NERAnnotator.Process(settings, _ => { }, CancellationToken.None);
            Directory.SetCurrentDirectory(settings.WorkingDir);

            var expectedFile = _testFiles.GetFile("Resources.opennlp.both_lists.train.txt");
            var expected = File.ReadAllText(expectedFile);
            var trainingOutputFile = settings.OutputFileBaseName + ".train.txt";
            var trainingOutput = File.ReadAllText(trainingOutputFile);
            Assert.AreEqual(expected, trainingOutput);

            expectedFile = _testFiles.GetFile("Resources.opennlp.both_lists.test.txt");
            expected = File.ReadAllText(expectedFile);
            var testingOutputFile = settings.OutputFileBaseName + ".test.txt";
            var testingOutput = File.ReadAllText(testingOutputFile);
            Assert.AreEqual(expected, testingOutput);
        }

        // Both training and testing dirs are specified (results will be the same as previous test, where both lists are specified)
        [Test, Category("NERAnnotator")]
        public static void OpenNlpBothDirs()
        {
            SetFiles();
            var settingsFile = Path.Combine(_inputFolder.Last(), "opennlp.both_dirs.annotator");
            _testFiles.GetFile("Resources.opennlp.both_dirs.annotator", settingsFile);
            var settings = Settings.LoadFrom(settingsFile);
            NERAnnotator.Process(settings, _ => { }, CancellationToken.None);
            Directory.SetCurrentDirectory(settings.WorkingDir);

            var expectedFile = _testFiles.GetFile("Resources.opennlp.both_lists.train.txt");
            var expected = File.ReadAllText(expectedFile);
            var trainingOutputFile = settings.OutputFileBaseName + ".train.txt";
            var trainingOutput = File.ReadAllText(trainingOutputFile);
            Assert.AreEqual(expected, trainingOutput);

            expectedFile = _testFiles.GetFile("Resources.opennlp.both_lists.test.txt");
            expected = File.ReadAllText(expectedFile);
            var testingOutputFile = settings.OutputFileBaseName + ".test.txt";
            var testingOutput = File.ReadAllText(testingOutputFile);
            Assert.AreEqual(expected, testingOutput);
        }

        // Specify each category explicitly with a different query that matches only one type
        [Test, Category("NERAnnotator")]
        public static void OpenNlpExplicitCategories()
        {
            SetFiles();
            var settingsFile = Path.Combine(_inputFolder.Last(), "opennlp.explicit_categories.annotator");
            _testFiles.GetFile("Resources.opennlp.explicit_categories.annotator", settingsFile);
            var settings = Settings.LoadFrom(settingsFile);
            NERAnnotator.Process(settings, _ => { }, CancellationToken.None);
            Directory.SetCurrentDirectory(settings.WorkingDir);

            var expectedFile = _testFiles.GetFile("Resources.opennlp.sent.learnable_tok.train.txt");
            var expected = File.ReadAllText(expectedFile);
            var trainingOutputFile = settings.OutputFileBaseName + ".train.txt";
            var trainingOutput = File.ReadAllText(trainingOutputFile);
            Assert.AreEqual(expected, trainingOutput);

            expectedFile = _testFiles.GetFile("Resources.opennlp.sent.learnable_tok.test.txt");
            expected = File.ReadAllText(expectedFile);
            var testingOutputFile = settings.OutputFileBaseName + ".test.txt";
            var testingOutput = File.ReadAllText(testingOutputFile);
            Assert.AreEqual(expected, testingOutput);
        }

        // Test that an exception is generated when an output file already exists
        [Test, Category("NERAnnotator")]
        public static void FailIfOutputFileExists()
        {
            SetFiles();
            var settingsFile = Path.Combine(_inputFolder.Last(), "opennlp.explicit_categories.annotator");
            _testFiles.GetFile("Resources.opennlp.explicit_categories.annotator", settingsFile);
            var settings = Settings.LoadFrom(settingsFile);
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
            var settings = Settings.LoadFrom(settingsFile);
            settings.TestingSet = TestingSetType.Specified;
            settings.TestingInput = "";
            NERAnnotator.Process(settings, _ => { }, CancellationToken.None);
            Directory.SetCurrentDirectory(settings.WorkingDir);

            var expectedFile = _testFiles.GetFile("Resources.opennlp.single_output_file.train.txt");
            var expected = File.ReadAllText(expectedFile);
            var trainingOutputFile = settings.OutputFileBaseName + ".train.txt";
            var trainingOutput = File.ReadAllText(trainingOutputFile);
            Assert.AreEqual(expected, trainingOutput);

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
            var settings = Settings.LoadFrom(settingsFile);
            settings.PercentToUseForTestingSet = 0;
            NERAnnotator.Process(settings, _ => { }, CancellationToken.None);
            Directory.SetCurrentDirectory(settings.WorkingDir);

            var expectedFile = _testFiles.GetFile("Resources.opennlp.single_output_file.train.txt");
            var expected = File.ReadAllText(expectedFile);
            var trainingOutputFile = settings.OutputFileBaseName + ".train.txt";
            var trainingOutput = File.ReadAllText(trainingOutputFile);
            Assert.AreEqual(expected, trainingOutput);

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
            var settings = Settings.LoadFrom(settingsFile);
            settings.PercentToUseForTestingSet = 0;
            settings.PercentUninterestingPagesToInclude = 100;
            NERAnnotator.Process(settings, _ => { }, CancellationToken.None);
            Directory.SetCurrentDirectory(settings.WorkingDir);

            var expectedFile = _testFiles.GetFile("Resources.opennlp.single_output_file.train.txt");
            var expected = File.ReadAllText(expectedFile);
            var trainingOutputFile = settings.OutputFileBaseName + ".train.txt";
            var trainingOutput = File.ReadAllText(trainingOutputFile);
            Assert.Greater(trainingOutput.Length, expected.Length);
            Assert.AreEqual(114005, trainingOutput.Length);
        }

        // Verify that output changes for different page-inclusion random seed values
        [Test, Category("NERAnnotator")]
        public static void OutputSomeUninterestingPages()
        {
            SetFiles();
            var settingsFile = Path.Combine(_inputFolder.Last(), "opennlp.explicit_categories.annotator");
            _testFiles.GetFile("Resources.opennlp.explicit_categories.annotator", settingsFile);
            var settings = Settings.LoadFrom(settingsFile);
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
            var settings = Settings.LoadFrom(settingsFile);
            NERAnnotator.Process(settings, _ => { }, CancellationToken.None);

            // Verify tags
            Directory.SetCurrentDirectory(settings.WorkingDir);
            var expectedFile = _testFiles.GetFile("Resources.opennlp.overlapping_expected.train.txt");
            var expected = File.ReadAllText(expectedFile);
            var trainingOutputFile = settings.OutputFileBaseName + ".train.txt";
            var trainingOutput = File.ReadAllText(trainingOutputFile);
            Assert.AreEqual(expected, trainingOutput);

            // Confirm that no (non-whitespace) text is lost
            var uss = new SpatialStringClass();
            var ussPath = Path.GetFullPath(@"Test\Example01.tif.uss");
            uss.LoadFrom(ussPath, false);
            expected = string.Join("", uss.GetPages(false, "")
                                .ToIEnumerable<SpatialString>()
                                // Remove whitespace so that the raw input can be compared to the tokenized input
                                .Select(page => Regex.Replace(page.String, @"\s+", "")));

            // Remove tags and whitespace so that the tokenized output (which has extra whitespace) matches the expected, which has all whitespace removed
            trainingOutput = Regex.Replace(trainingOutput, @"<START:\w+>|<END>|\s+", "");
            Assert.AreEqual(expected, trainingOutput);
        }

        // Test non-default ValueQuery
        [Test, Category("NERAnnotator")]
        public static void ValueQuery()
        {
            SetFiles();
            var settingsFile = Path.Combine(_inputFolder.Last(), "opennlp.flex.annotator");
            _testFiles.GetFile("Resources.opennlp.flex.annotator", settingsFile);
            var settings = Settings.LoadFrom(settingsFile);
            NERAnnotator.Process(settings, _ => { }, CancellationToken.None);

            // Verify tags
            Directory.SetCurrentDirectory(settings.WorkingDir);
            var expectedFile = _testFiles.GetFile("Resources.opennlp.flex.train.txt");
            var expected = File.ReadAllText(expectedFile);
            var trainingOutputFile = settings.OutputFileBaseName + ".train.txt";
            var trainingOutput = File.ReadAllText(trainingOutputFile);
            Assert.AreEqual(expected, trainingOutput);
        }

        #endregion Tests
    }
}