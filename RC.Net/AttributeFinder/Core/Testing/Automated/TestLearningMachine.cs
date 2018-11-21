using AttributeDbMgrComponentsLib;
using Extract.FileActionManager.Database.Test;
using Extract.FileActionManager.FileProcessors;
using Extract.Testing.Utilities;
using Extract.Utilities;
using LearningMachineTrainer;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using UCLID_RASTERANDOCRMGMTLib;
using ComAttribute = UCLID_AFCORELib.Attribute;

namespace Extract.AttributeFinder.Test
{
    [Flags]
    public enum Features
    {
        Attribute = 1,
        SpatialString = 2,
        Both = Attribute | SpatialString
    }
    [Flags]
    [SuppressMessage("Microsoft.Naming", "CA1714:FlagsEnumsShouldHavePluralNames")]
    public enum MissingData
    {
        None = 0,
        SomeImages = 1,
        SomeUSSFiles = 2,
        SomeVOAFiles = 4,
        SomeEAVFiles = 8,
        AllOfSpecified = 16,
        All = AllOfSpecified | SomeImages | SomeUSSFiles | SomeVOAFiles,
        AllVOAFiles = AllOfSpecified | SomeVOAFiles,
        AllEAVFiles = AllOfSpecified | SomeEAVFiles,
        VOA_EAVFiles = SomeVOAFiles | SomeEAVFiles,
        AllV_EFiles = AllOfSpecified | VOA_EAVFiles,
    }
    public enum FeatureRule
    {
        None = 0,
        IfMissing = 1,
        Always = 2
    }

    /// <summary>
    /// Unit tests for learning machine data class
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("LearningMachine")]
    public class TestLearningMachine
    {
        #region Constants

        static readonly string _STORE_ATTRIBUTE_GUID = typeof(StoreAttributesInDBTask).GUID.ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// Manages the test images needed for testing.
        /// </summary>
        static TestFileManager<TestLearningMachine> _testFiles;
        static FAMTestDBManager<TestLearningMachine> _testDbManager;
        static List<string> _inputFolder = new List<string>();
        static string _csvPath = Path.GetTempFileName();
        static string _savedMachinePath = Path.GetTempFileName();
        static string[] _categories;

        #endregion Fields

        #region Overhead

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestLearningMachine>();
            _testDbManager = new FAMTestDBManager<TestLearningMachine>();
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

            if (_testDbManager != null)
            {
                _testDbManager.Dispose();
            }

            // Delete temp dir
            foreach(var dir in _inputFolder.Where(dir => Directory.Exists(dir)))
            {
                Directory.Delete(dir, true);
            }

            // Delete tmp csv
            if (File.Exists(_csvPath))
            {
                File.Delete(_csvPath);
            }

            // Delete saved machine
            if (File.Exists(_savedMachinePath))
            {
                File.Delete(_savedMachinePath);
            }
        }

        #endregion Overhead

        #region Tests

        // Test GetIndexesOfSubsetsByCategory to make sure it is randomly selecting appropriately sized subsets
        [Test, Category("LearningMachine")]
        public static void GetIndexesOfSubsetsByCategoryOneCategory()
        {
            foreach (int size in new[] { 1, 1000 })
            {
                int[] originalIndexes = new int[size];

                // All one category for this test
                int[] categories = new int[size];
                for (int i = 0; i < size; i++)
                {
                    originalIndexes[i] = i;
                }
                var fractions = new HashSet<double>();
                fractions.Add(1);
                for (double d = 1; d <= 100; d++)
                {
                    for (double n = 1; n < d; n++)
                    {
                        fractions.Add(n / d);
                    }
                }
                var rng = new Random();
                foreach (var subset1Fraction in fractions)
                {
                    int seed = rng.Next();
                    System.Collections.Generic.List<int> subset1Indexes, subset2Indexes;
                    LearningMachineMethods.GetIndexesOfSubsetsByCategory(categories, subset1Fraction, out subset1Indexes, out subset2Indexes, new Random(seed));

                    // Check size of subset1
                    Assert.AreEqual(Math.Max(Math.Round(size * subset1Fraction), 1), subset1Indexes.Count);
                    // Check size of subset2
                    Assert.AreEqual(Math.Max(size - Math.Round(size * subset1Fraction), 1), subset2Indexes.Count);

                    // Subsets should overlap by at most one item
                    Assert.LessOrEqual(subset1Indexes.Intersect(subset2Indexes).Count(), 1);

                    // These next rely on implementation details so may fail if implementation changes

                    // Shuffle a copy of the original indexes
                    var expected = originalIndexes.ToArray();
                    Utilities.CollectionMethods.Shuffle(expected, new Random(seed));

                    // Since the same random generator was used, subset1 indexes should be the same as first of the shuffled indexes
                    CollectionAssert.AreEqual(expected.Take(subset1Indexes.Count), subset1Indexes);

                    // Subset2 indexes should be the same as last of the shuffled indexes
                    CollectionAssert.AreEqual(expected.Skip(size - subset2Indexes.Count), subset2Indexes);
                }
            }
        }

        // Test GetIndexesOfSubsetsByCategory with multiple categories to make sure no items are
        // missing from both subsets and few if any items are present in both subsets with a variety
        // of category sizes
        [Test, Category("LearningMachine")]
        public static void GetIndexesOfSubsetsByCategoryMultipleCategories()
        {
            foreach (int size in new[] { 1, 1000 })
            {
                var numberOfCategories = 10;
                int[] data = new int[size];
                int[] categories = new int[size];
                var rng = new Random();
                for (int i = 0; i < size; i++)
                {
                    data[i] = i;
                    categories[i] = rng.Next(0, numberOfCategories);
                }
                var fractions = new HashSet<double>();
                fractions.Add(1);
                for (double d = 1; d <= 100; d++)
                {
                    for (double n = 1; n < d; n++)
                    {
                        fractions.Add(n / d);
                    }
                }
                foreach (var subset1Fraction in fractions)
                {
                    System.Collections.Generic.List<int> subset1Indexes, subset2Indexes;
                    LearningMachineMethods.GetIndexesOfSubsetsByCategory(categories, subset1Fraction, out subset1Indexes, out subset2Indexes);

                    // Check if counts make sense
                    Assert.GreaterOrEqual(subset1Indexes.Count + subset2Indexes.Count, size);
                    Assert.LessOrEqual(subset1Indexes.Count + subset2Indexes.Count, size + numberOfCategories);

                    // Check to make sure overlap is minimal
                    Assert.LessOrEqual(subset1Indexes.Intersect(subset2Indexes).Count(), numberOfCategories);
                }
            }
        }

        [Test, Category("LearningMachine")]
        public static void TrainMachineDocTypesFromFolder()
        {
            SetDocumentCategorizationFiles();
            var inputConfig = new InputConfiguration
            {
                InputPath = _inputFolder.Last(),
                InputPathType = InputType.Folder,
                AttributesPath = "",
                AnswerPath = "$FileOf($DirOf(<SourceDocName>))",
                TrainingSetPercentage = 80
            };
            var lm = new LearningMachine
            {
                InputConfig = inputConfig,
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization, new SpatialStringFeatureVectorizer(null, 5, 2000)),
                Classifier = new MulticlassSupportVectorMachineClassifier()
            };
            var results = lm.TrainMachine();
            Assert.Greater(results.trainingSet.Match(gcm => gcm.OverallAgreement, cm => cm.Accuracy), 0.99);
            Assert.Greater(results.testingSet.Match(gcm => gcm.OverallAgreement, cm => cm.Accuracy), 0.99);

            // Test SerializableConfusionMatrix
            var trainCM = lm.AccuracyData.Value.train;
            var testCM = lm.AccuracyData.Value.test;
            Assert.AreEqual(results.trainingSet.Match(gcm => gcm.OverallAgreement, cm => cm.Accuracy), trainCM.OverallAgreement());
            Assert.AreEqual(results.testingSet.Match(gcm => gcm.OverallAgreement, cm => cm.Accuracy), testCM.OverallAgreement());

            // Test output
            string[] ussFiles, voaFiles, answers;
            inputConfig.GetInputData(out ussFiles, out voaFiles, out answers);
            for (int i = 0; i < ussFiles.Length; i++)
            {
                var uss = new SpatialStringClass();
                uss.LoadFrom(ussFiles[i], false);
                var voa = new IUnknownVectorClass();
                lm.ComputeAnswer(uss, voa, false);
                Assert.AreEqual(answers[i], ((ComAttribute)voa.At(0)).Value.String);
            }

            // Test preserving input (no input)
            inputConfig.GetInputData(out ussFiles, out voaFiles, out answers);
            for (int i = 0; i < ussFiles.Length; i++)
            {
                var uss = new SpatialStringClass();
                uss.LoadFrom(ussFiles[i], false);
                var voa = new IUnknownVectorClass();
                lm.ComputeAnswer(uss, voa, true);
                Assert.AreEqual(answers[i], ((ComAttribute)voa.At(0)).Value.String);
            }

            // Test preserving input (with input)
            inputConfig.AttributesPath = "<SourceDocName>.voa";
            inputConfig.GetInputData(out ussFiles, out voaFiles, out answers);
            for (int i = 0; i < ussFiles.Length; i++)
            {
                var uss = new SpatialStringClass();
                uss.LoadFrom(ussFiles[i], false);
                var voa = new IUnknownVectorClass();
                voa.LoadFrom(voaFiles[i], false);
                var previousSize = voa.Size();
                lm.ComputeAnswer(uss, voa, true);
                Assert.Less(previousSize, voa.Size());
            }

            // Test not preserving input (with input)
            inputConfig.AttributesPath = "<SourceDocName>.voa";
            inputConfig.GetInputData(out ussFiles, out voaFiles, out answers);
            for (int i = 0; i < ussFiles.Length; i++)
            {
                var uss = new SpatialStringClass();
                uss.LoadFrom(ussFiles[i], false);
                var voa = new IUnknownVectorClass();
                voa.LoadFrom(voaFiles[i], false);
                lm.ComputeAnswer(uss, voa, false);
                Assert.AreEqual(1, voa.Size());
            }
        }

        [Test, Category("LearningMachine")]
        public static void TrainMachineFeatureHashingDocTypesFromFolder()
        {
            SetDocumentCategorizationFiles();
            var inputConfig = new InputConfiguration
            {
                InputPath = _inputFolder.Last(),
                InputPathType = InputType.Folder,
                AttributesPath = "",
                AnswerPath = "$FileOf($DirOf(<SourceDocName>))",
                TrainingSetPercentage = 80
            };
            var lm = new LearningMachine
            {
                InputConfig = inputConfig,
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization,
                    new SpatialStringFeatureVectorizer(null, 5, 2000) { UseFeatureHashing = true }),
                Classifier = new MulticlassSupportVectorMachineClassifier()
            };
            var (trainingSet, testingSet) = lm.TrainMachine();
            Assert.Greater(trainingSet.Match(gcm => gcm.OverallAgreement, cm => cm.Accuracy), 0.99);
            Assert.Greater(testingSet.Match(gcm => gcm.OverallAgreement, cm => cm.Accuracy), 0.99);
        }

        [Test, Category("LearningMachine")]
        public static void TrainMachinePaginationFromFolder()
        {
            SetPaginationFiles();
            var lm = new LearningMachine
            {
                InputConfig = new InputConfiguration
                {
                    InputPath = _inputFolder.Last(),
                    InputPathType = InputType.Folder,
                    AttributesPath = "<SourceDocName>.protofeatures.voa",
                    AnswerPath = "<SourceDocName>.eav",
                    TrainingSetPercentage = 50
                },
                // https://extract.atlassian.net/browse/ISSUE-14479
                // Updated settings because new, TFIDF ordering of features changed the effects of the random initialization
                // (Since using non-@Feature attributes meant patient names, e.g., were being learned as bag-of-word features)
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.Pagination, attributeFilter: "*@Feature"),
                Classifier = new NeuralNetworkClassifier { UseCrossValidationSets = true },
                RandomNumberSeed = 1
            };
            var results = lm.TrainMachine();
            Assert.Greater(results.Item1.Match(_ => Double.NaN, cm => cm.FScore), 0.9);
            Assert.Greater(results.Item2.Match(_ => Double.NaN, cm => cm.FScore), 0.6);

            // Test SerializableConfusionMatrix
            var trainCM = lm.AccuracyData.Value.train;
            var testCM = lm.AccuracyData.Value.test;
            Assert.AreEqual(results.trainingSet.Match(_ => Double.NaN, cm => cm.FScore), trainCM.FScoreMicroAverage());
            Assert.AreEqual(results.trainingSet.Match(_ => Double.NaN, cm => cm.Recall), trainCM.RecallMicroAverage());
            Assert.AreEqual(results.trainingSet.Match(_ => Double.NaN, cm => cm.Precision), trainCM.PrecisionMicroAverage());

            Assert.AreEqual(results.testingSet.Match(_ => Double.NaN, cm => cm.FScore), testCM.FScoreMicroAverage());
            Assert.AreEqual(results.testingSet.Match(_ => Double.NaN, cm => cm.Recall), testCM.RecallMicroAverage());
            Assert.AreEqual(results.testingSet.Match(_ => Double.NaN, cm => cm.Precision), testCM.PrecisionMicroAverage());
        }

        // Test that pagination LMs use unknown category threshold at run time
        // https://extract.atlassian.net/browse/ISSUE-15643
        [Test, Category("LearningMachine")]
        public static void ProbabilityFilterPagination()
        {
            SetPaginationFiles();
            var lm = new LearningMachine
            {
                InputConfig = new InputConfiguration
                {
                    InputPath = _inputFolder.Last(),
                    InputPathType = InputType.Folder,
                    AttributesPath = "<SourceDocName>.protofeatures.voa",
                    AnswerPath = "<SourceDocName>.eav",
                    TrainingSetPercentage = 50
                },
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.Pagination, autoBagOfWords: new SpatialStringFeatureVectorizer("", 5, 2000), attributeFilter: "*@Feature"),
                Classifier = new NeuralNetworkClassifier { UseCrossValidationSets = true }
            };
            lm.TrainMachine();

            lm.InputConfig.GetInputData(out var spatialStringFilePaths, out var attributeFilePaths,
                out var answerFiles);
            var (inputs, answers) = lm.Encoder.GetFeatureVectorAndAnswerCollections(spatialStringFilePaths, attributeFilePaths, answerFiles);

            // Second page of second doc should be a divider but is predicted to be NotFirstPage with a low score
            var (anAnswer, aScore) = lm.Classifier.ComputeAnswer(inputs[3]);
            Assert.AreEqual(lm.Encoder.AnswerNameToCode["NotFirstPage"], anAnswer);
            Assert.AreNotEqual(answers[3], anAnswer);

            // With low score
            Assert.Greater(aScore, 0.5);
            Assert.Less(aScore, 0.6);

            // Confirm that the lm will predict a different value depending on unknown category settings
            var doc = new SpatialStringClass();
            doc.LoadFrom(spatialStringFilePaths[1], false);
            var attrr = new IUnknownVectorClass();
            attrr.LoadFrom(attributeFilePaths[1], false);

            // Confirm default behavior misses the break
            lm.ComputeAnswer(doc, attrr, false);
            Assert.AreEqual(1, attrr.Size());

            // Set threshold and confirm that NotFirstPage is the default Unknown category
            lm.UseUnknownCategory = true;
            lm.UnknownCategoryCutoff = 0.6;
            attrr.LoadFrom(attributeFilePaths[1], false);
            lm.ComputeAnswer(doc, attrr, false);
            Assert.AreEqual(1, attrr.Size());
                
            // Confirm that FirstPage can be used instead, which improves the recall
            lm.TranslateUnknownCategory = true;
            lm.TranslateUnknownCategoryTo = "FirstPage";
            attrr.LoadFrom(attributeFilePaths[1], false);
            lm.ComputeAnswer(doc, attrr, false);
            Assert.AreEqual(2, attrr.Size());
        }

        // Confirm that accuracy can be improved on input that has more cover pages
        // than the training data by using the probability threshold value to reduce false positives
        [Test, Category("LearningMachine")]
        public static void ProbabilityFilterPaginationCoverPages()
        {
            SetPaginationFiles();
            string tempPath = Path.Combine(_inputFolder.Last(), Path.GetRandomFileName());
            var lm = new LearningMachine
            {
                InputConfig = new InputConfiguration
                {
                    InputPath = _inputFolder.Last(),
                    InputPathType = InputType.Folder,
                    AttributesPath = "<SourceDocName>.protofeatures.voa",
                    AnswerPath = "<SourceDocName>.eav",
                    TrainingSetPercentage = 100
                },
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.Pagination, attributeFilter: "*@Feature"),
                Classifier = new NeuralNetworkClassifier { UseCrossValidationSets = true },
                CsvOutputFile = tempPath
            };
            lm.TrainMachine();

            SetPaginationFiles(withCoverPages: true);

            lm.InputConfig.InputPath = _inputFolder.Last();
            lm.InputConfig.TrainingSetPercentage = 0;

            // Result has poor precision (high false positive rate)
            var (_, testResults) = lm.TestMachine();
            testResults.Match(gcm => throw new ArgumentException("Not expecting a general confusion matrix!"),
                cm =>
                {
                    Assert.AreEqual(0.54, Math.Round(cm.FScore, 2));
                    Assert.AreEqual(0.37, Math.Round(cm.Precision, 2));
                    Assert.AreEqual(1, Math.Round(cm.Recall, 2));
                });

            // Fix with the unknown category cutoff feature
            lm.UseUnknownCategory = true;
            lm.UnknownCategoryCutoff = 0.885;
            lm.TranslateUnknownCategory = true;
            lm.TranslateUnknownCategoryTo = "NotFirstPage";
            (_, testResults) = lm.TestMachine();
            testResults.Match(gcm => throw new ArgumentException("Not expecting a general confusion matrix!"),
                cm =>
                {
                    Assert.AreEqual(0.75, Math.Round(cm.Precision, 2));
                    Assert.AreEqual(0.86, Math.Round(cm.Recall, 2));
                    Assert.AreEqual(0.8, Math.Round(cm.FScore, 2));
                });
        }

        // Test that deletion LMs use unknown category threshold at run time
        // https://extract.atlassian.net/browse/ISSUE-15643
        [Test, Category("LearningMachine")]
        public static void ProbabilityFilterDeletion()
        {
            SetPaginationFiles(withCoverPages: true);

            string tempPath = Path.Combine(_inputFolder.Last(), Path.GetRandomFileName());
            var lm = new LearningMachine
            {
                InputConfig = new InputConfiguration
                {
                    InputPath = _inputFolder.Last(),
                    InputPathType = InputType.Folder,
                    AttributesPath = "",
                    AnswerPath = "<SourceDocName>.eav",
                    TrainingSetPercentage = 50
                },
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.Deletion,
                    autoBagOfWords: new SpatialStringFeatureVectorizer("", 5, 2000), attributeFilter: "*@Feature"),
                Classifier = new NeuralNetworkClassifier {UseCrossValidationSets = true},
                CsvOutputFile = tempPath,
                RandomNumberSeed = 1
            };
            lm.ComputeEncodings();
            lm.WriteDataToCsv(_ => { }, CancellationToken.None);
            string trainCSV = tempPath + ".train.csv";
            string testCSV = tempPath + ".test.csv";

            // Train and output the predictions and probabilities back to the CSVs
            lm.TrainAndTestWithCsvData(false, trainCSV, testCSV, true, _ => { }, CancellationToken.None);

            var predictions = GetPredictionsFromCsv(testCSV);

            // Page 2 of Pagination_004.tif is predicted to be deleted but should not be
            var (path, prediction, probability) = predictions[3];
            Assert.AreEqual("Resources.LearningMachine.PaginationWithCoverPages.Pagination_004.tif.uss",
                Path.GetFileName(path));
            Assert.AreEqual("DeletedPage", prediction);

            // With somewhat low score
            Assert.Greater(probability, 0.7);
            Assert.Less(probability, 0.8);

            // Confirm that the lm will predict a different value depending on unknown category settings
            var doc = new SpatialStringClass();
            doc.LoadFrom(path, false);
            var attrr = new IUnknownVectorClass();

            // Confirm default behavior predicts page 2 as deleted
            lm.ComputeAnswer(doc, attrr, false);
            var range = ((IAttribute) attrr.At(0)).Value.String;
            Assert.AreEqual("1-2,4", range);

            // Set threshold and confirm that NotDeletedPage is the default Unknown category
            lm.UseUnknownCategory = true;
            lm.UnknownCategoryCutoff = 0.8;
            lm.ComputeAnswer(doc, attrr, false);
            range = ((IAttribute) attrr.At(0)).Value.String;
            Assert.AreEqual("1,4", range);

            // Confirm that DeletedPage can be used instead
            lm.TranslateUnknownCategory = true;
            lm.TranslateUnknownCategoryTo = "DeletedPage";
            lm.ComputeAnswer(doc, attrr, false);
            range = ((IAttribute) attrr.At(0)).Value.String;
            Assert.AreEqual("1-2,4,9-10", range);
        }

        // Verifies that deleted page ranges are created without error when there are two ranges
        // separated by more than one page
        // https://extract.atlassian.net/browse/ISSUE-15668
        [Test, Category("LearningMachine")]
        public static void DeletionModeMultipleSeparatedRanges()
        {
            SetPaginationFiles(withCoverPages: true);

            string tempPath = Path.Combine(_inputFolder.Last(), Path.GetRandomFileName());
            var lm = new LearningMachine
            {
                InputConfig = new InputConfiguration
                {
                    InputPath = _inputFolder.Last(),
                    InputPathType = InputType.Folder,
                    AttributesPath = "",
                    AnswerPath = "<SourceDocName>.eav",
                    TrainingSetPercentage = 100
                },
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.Deletion,
                    autoBagOfWords: new SpatialStringFeatureVectorizer("", 5, 2000), attributeFilter: "*@Feature"),
                Classifier = new NeuralNetworkClassifier {UseCrossValidationSets = true},
                CsvOutputFile = tempPath
            };
            lm.ComputeEncodings();
            lm.TrainMachine();

            List<string> ranges = new List<string>();
            foreach (var path in Directory.GetFiles(_inputFolder.Last(), "*.uss"))
            {
                var doc = new SpatialStringClass();
                doc.LoadFrom(path, false);
                var attrr = new IUnknownVectorClass();

                lm.ComputeAnswer(doc, attrr, false);
                Assert.That(attrr.Size() == 0 || attrr.Size() == 1);

                if (attrr.Size() == 1)
                {
                    var range = ((IAttribute) attrr.At(0)).Value.String;
                    ranges.Add(range);
                }
            }

            var expected = new List<string>
            {
                "3,5",
                "1,3",
                "1",
                "1,3,9",
                "1,4"
            };
            CollectionAssert.AreEqual(expected, ranges);
        }

        [Test, Category("LearningMachine")]
        public static void TrainMachineFeatureHashingPaginationFromFolder()
        {
            SetPaginationFiles();
            var lm = new LearningMachine
            {
                InputConfig = new InputConfiguration
                {
                    InputPath = _inputFolder.Last(),
                    InputPathType = InputType.Folder,
                    AttributesPath = "<SourceDocName>.protofeatures.voa",
                    AnswerPath = "<SourceDocName>.eav",
                    TrainingSetPercentage = 50
                },
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.Pagination,
                    new SpatialStringFeatureVectorizer(null, 2, 2000) { UseFeatureHashing = true }),
                Classifier = new NeuralNetworkClassifier { UseCrossValidationSets = true, HiddenLayers = new[] { 50 } },
                RandomNumberSeed = 1
            };
            var results = lm.TrainMachine();
            Assert.Greater(results.trainingSet.Match(_ => Double.NaN, cm => cm.FScore), 0.88);
            Assert.Greater(results.testingSet.Match(_ => Double.NaN, cm => cm.FScore), 0.6);
        }

        [Test, Category("LearningMachine")]
        public static void TrainMachinePaginationFromList()
        {
            SetPaginationFiles();
            string[] listContents = Directory.GetFiles(_inputFolder.Last(), "*.tif", SearchOption.AllDirectories);
            File.WriteAllLines(_csvPath, listContents);

            var lm = new LearningMachine
            {
                InputConfig = new InputConfiguration
                {
                    InputPath = _csvPath,
                    InputPathType = InputType.TextFile,
                    AttributesPath = "<SourceDocName>.protofeatures.voa",
                    AnswerPath = "<SourceDocName>.eav",
                    TrainingSetPercentage = 50
                },
                // https://extract.atlassian.net/browse/ISSUE-14479
                // Updated settings because new, TFIDF ordering of features changed the effects of the random initialization
                // (Using non-@Feature attributes meant patient names, e.g., were being learned as bag-of-word features)
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.Pagination, attributeFilter: "*@Feature"),
                Classifier = new NeuralNetworkClassifier { UseCrossValidationSets = true },
                RandomNumberSeed = 1
            };
            var results = lm.TrainMachine();
            Assert.Greater(results.Item1.Match(_ => Double.NaN, cm => cm.FScore), 0.9);
            Assert.Greater(results.Item2.Match(_ => Double.NaN, cm => cm.FScore), 0.6);

            // Test SerializableConfusionMatrix
            var trainCM = lm.AccuracyData.Value.train;
            var testCM = lm.AccuracyData.Value.test;
            Assert.AreEqual(results.trainingSet.Match(_ => Double.NaN, cm => cm.FScore), trainCM.FScoreMicroAverage());
            Assert.AreEqual(results.trainingSet.Match(_ => Double.NaN, cm => cm.Recall), trainCM.RecallMicroAverage());
            Assert.AreEqual(results.trainingSet.Match(_ => Double.NaN, cm => cm.Precision), trainCM.PrecisionMicroAverage());

            Assert.AreEqual(results.testingSet.Match(_ => Double.NaN, cm => cm.FScore), testCM.FScoreMicroAverage());
            Assert.AreEqual(results.testingSet.Match(_ => Double.NaN, cm => cm.Recall), testCM.RecallMicroAverage());
            Assert.AreEqual(results.testingSet.Match(_ => Double.NaN, cm => cm.Precision), testCM.PrecisionMicroAverage());
        }

        [Test, Category("LearningMachine")]
        public static void TrainMachineFromCsv()
        {
            SetDocumentCategorizationFiles();
            string[] csvContents = Directory.GetFiles(_inputFolder.Last(), "*.tif", SearchOption.AllDirectories)
                .Select(imagePath => string.Join(",", imagePath, Path.GetFileName(Path.GetDirectoryName(imagePath)))).ToArray();
            File.WriteAllLines(_csvPath, csvContents);

            var lm = new LearningMachine
            {
                InputConfig = new InputConfiguration
                {
                    InputPath = _csvPath,
                    InputPathType = InputType.Csv,
                    AttributesPath = "",
                    AnswerPath = "",
                    TrainingSetPercentage = 80
                },
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization, new SpatialStringFeatureVectorizer(null, 5, 2000)),
                Classifier = new MulticlassSupportVectorMachineClassifier()
            };
            var results = lm.TrainMachine();
            Assert.Greater(results.Item1.Match(gcm => gcm.OverallAgreement, cm => cm.Accuracy), 0.99);
            Assert.Greater(results.Item2.Match(gcm => gcm.OverallAgreement, cm => cm.Accuracy), 0.99);
        }

        [Test, Category("LearningMachine")]
        public static void TrainMachineAttributeCategorizationDOB()
        {
            SetPaginationFiles();
            var lm = new LearningMachine
            {
                InputConfig = new InputConfiguration
                {
                    InputPath = _inputFolder.Last(),
                    InputPathType = InputType.Folder,
                    AttributesPath = "<SourceDocName>.labeled.voa",
                    TrainingSetPercentage = 80
                },
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.AttributeCategorization, null, "*@Feature"),
                Classifier = new MulticlassSupportVectorMachineClassifier()
            };
            var results = lm.TrainMachine();
            Assert.AreEqual(1.0, results.Item1.Match(_ => Double.NaN, cm => cm.FScore));
            Assert.Greater(results.Item2.Match(_ => Double.NaN, cm => cm.FScore), 0.85);

            // Test SerializableConfusionMatrix
            var trainCM = lm.AccuracyData.Value.train;
            var testCM = lm.AccuracyData.Value.test;
            Assert.AreEqual(results.trainingSet.Match(_ => Double.NaN, cm => cm.FScore), trainCM.FScoreMicroAverage());
            Assert.AreEqual(results.trainingSet.Match(_ => Double.NaN, cm => cm.Recall), trainCM.RecallMicroAverage());
            Assert.AreEqual(results.trainingSet.Match(_ => Double.NaN, cm => cm.Precision), trainCM.PrecisionMicroAverage());

            Assert.AreEqual(results.testingSet.Match(_ => Double.NaN, cm => cm.FScore), testCM.FScoreMicroAverage());
            Assert.AreEqual(results.testingSet.Match(_ => Double.NaN, cm => cm.Recall), testCM.RecallMicroAverage());
            Assert.AreEqual(results.testingSet.Match(_ => Double.NaN, cm => cm.Precision), testCM.PrecisionMicroAverage());
        }

        // This is not really a useful case but make sure it works without exception
        [Test, Category("LearningMachine")]
        public static void TrainMachineAttributeCategorizationFeatureHashingBagOfWords()
        {
            SetPaginationFiles();
            var lm = new LearningMachine
            {
                InputConfig = new InputConfiguration
                {
                    InputPath = _inputFolder.Last(),
                    InputPathType = InputType.Folder,
                    AttributesPath = "<SourceDocName>.labeled.voa",
                    TrainingSetPercentage = 80
                },
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.AttributeCategorization,
                    new SpatialStringFeatureVectorizer(null, 5, 2000) { UseFeatureHashing = true }),
                Classifier = new MulticlassSupportVectorMachineClassifier()
            };
            var results = lm.TrainMachine();
            Assert.AreEqual(1.0, results.trainingSet.Match(_ => Double.NaN, cm => cm.FScore));
            Assert.Greater(results.testingSet.Match(_ => Double.NaN, cm => cm.FScore), 0.85);
        }

        [Test, Category("LearningMachine")]
        public static void TrainMachineAttributeCategorizationDOBAndCollectionDate()
        {
            SetPaginationFiles();
            var lm = new LearningMachine
            {
                InputConfig = new InputConfiguration
                {
                    InputPath = _inputFolder.Last(),
                    InputPathType = InputType.Folder,
                    AttributesPath = "<SourceDocName>.labeled_3_types.voa",
                    TrainingSetPercentage = 80
                },
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.AttributeCategorization, null, "*@Feature"),
                Classifier = new MulticlassSupportVectorMachineClassifier()
            };
            var results = lm.TrainMachine();
            // Note: this number has fallen with the change to use a larger CV set size when picking the value of the Complexity parameter.
            // (Was 20% and now is 50%.) This change was mostly motivated by a desire to increaset the speed of the algorithm but it also may
            // lead to better results. In this case, it doesn't affect the testing set results, only the training set results, which don't really matter.
            Assert.Greater(results.trainingSet.Match(gcm => gcm.OverallAgreement, _ => Double.NaN), 0.7);

            // Test results are between 70% and 80% when there are 'other' dates (neither DOB nor CollectionDate)
            Assert.Greater(results.testingSet.Match(gcm => gcm.OverallAgreement, _ => Double.NaN), 0.7);
            Assert.Less(results.testingSet.Match(gcm => gcm.OverallAgreement, _ => Double.NaN), 0.8);
        }

        [Test, Category("LearningMachine")]
        public static void TrainMachineAttributeCategorizationDOBAndCollectionDate_NoOther()
        {
            SetPaginationFiles();
            var lm = new LearningMachine
            {
                InputConfig = new InputConfiguration
                {
                    InputPath = _inputFolder.Last(),
                    InputPathType = InputType.Folder,
                    AttributesPath = "<SourceDocName>.labeled_2_types.voa",
                    TrainingSetPercentage = 80
                },
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.AttributeCategorization, null, "*@Feature"),
                Classifier = new MulticlassSupportVectorMachineClassifier()
            };
            var results = lm.TrainMachine();
            Assert.AreEqual(1.0, results.Item1.Match(gcm => gcm.OverallAgreement, _ => Double.NaN));

            // Results are better when there are no 'other' dates to worry about
            Assert.AreEqual(1.0, results.Item2.Match(gcm => gcm.OverallAgreement, _ => Double.NaN));
        }


        /// <summary>
        /// Test that a simple list works for attribute categorization input
        /// https://extract.atlassian.net/browse/ISSUE-14430
        /// Learning machine editor: Attribute categorization from a file list requires CSV
        /// </summary>
        [Test, Category("LearningMachine")]
        public static void TrainMachineAttributeCategorizationFromList()
        {
            SetPaginationFiles();
            var files = Directory.GetFiles(_inputFolder.Last(), "*.tif");
            File.WriteAllLines(_csvPath, files);
            var lm = new LearningMachine
            {
                InputConfig = new InputConfiguration
                {
                    InputPath = _csvPath,
                    InputPathType = InputType.TextFile,
                    AttributesPath = "<SourceDocName>.labeled_2_types.voa",
                    TrainingSetPercentage = 80
                },
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.AttributeCategorization, null, "*@Feature"),
                Classifier = new MulticlassSupportVectorMachineClassifier()
            };
            var (trainingSet, testingSet) = lm.TrainMachine();
            Assert.AreEqual(1.0, trainingSet.Match(gcm => gcm.OverallAgreement, _ => Double.NaN));
            Assert.AreEqual(1.0, testingSet.Match(gcm => gcm.OverallAgreement, _ => Double.NaN));
        }

        [Test, Category("LearningMachine")]
        public static void TrainMachineFromCsvWithHeader()
        {
            SetDocumentCategorizationFiles();
            string[] csvContents = Directory.GetFiles(_inputFolder.Last(), "*.tif", SearchOption.AllDirectories)
                .Select(imagePath => string.Join(",", imagePath, Path.GetFileName(Path.GetDirectoryName(imagePath)))).ToArray();
            csvContents = new[] { "Image Paths, <Category>" }.Concat(csvContents).ToArray();
            File.WriteAllLines(_csvPath, csvContents);

            var lm = new LearningMachine
            {
                InputConfig = new InputConfiguration
                {
                    InputPath = _csvPath,
                    InputPathType = InputType.Csv,
                    AttributesPath = "",
                    AnswerPath = "",
                    TrainingSetPercentage = 80
                },
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization, new SpatialStringFeatureVectorizer(null, 5, 2000)),
                Classifier = new MulticlassSupportVectorMachineClassifier()
            };
            var results = lm.TrainMachine();
            Assert.Greater(results.Item1.Match(gcm => gcm.OverallAgreement, cm => cm.Accuracy), 0.99);
            Assert.Greater(results.Item2.Match(gcm => gcm.OverallAgreement, cm => cm.Accuracy), 0.99);
        }

        [Test, Category("LearningMachine")]
        public static void TrainMachineFromCsvWithQuotes()
        {
            SetDocumentCategorizationFiles();
            string[][] csvTempContents = Directory.GetFiles(_inputFolder.Last(), "*.tif", SearchOption.AllDirectories)
                .Select(imagePath => new[] { imagePath, Path.GetFileName(Path.GetDirectoryName(imagePath)) }).ToArray();

            csvTempContents[2][0] = "\"" + csvTempContents[2][0] + "\"";
            csvTempContents[3][1] = "\"Abstract of \"\"Support\"\" Judgment\"";

            string[] csvContents = csvTempContents.Select(x => string.Join(",", x)).ToArray();
            File.WriteAllLines(_csvPath, csvContents);

            var lm = new LearningMachine
            {
                InputConfig = new InputConfiguration
                {
                    InputPath = _csvPath,
                    InputPathType = InputType.Csv,
                    AttributesPath = "",
                    AnswerPath = "",
                    TrainingSetPercentage = 80
                },
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization, new SpatialStringFeatureVectorizer(null, 5, 2000)),
                Classifier = new MulticlassSupportVectorMachineClassifier()
            };
            var results = lm.TrainMachine();
            Assert.Greater(results.Item1.Match(gcm => gcm.OverallAgreement, cm => cm.Accuracy), 0.99);
            Assert.Greater(results.Item2.Match(gcm => gcm.OverallAgreement, cm => cm.Accuracy), 0.99);
        }

        // Test that the Document/Pages attributes are correctly created
        [Test, Category("LearningMachine")]
        public static void CreateExpectedPaginationValues()
        {
            SetPaginationFiles();
            var inputConfig = new InputConfiguration
            {
                InputPath = _inputFolder.Last(),
                InputPathType = InputType.Folder,
                AttributesPath = "<SourceDocName>.protofeatures.voa",
                AnswerPath = "<SourceDocName>.eav",
                TrainingSetPercentage = 50
            };
            var lm = new LearningMachine
            {
                InputConfig = inputConfig,
                Classifier = new NeuralNetworkClassifier { UseCrossValidationSets = false },
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.Pagination)
            };
            lm.TrainMachine();

            // Build answer files from classifier output. This should give 100% accuracy
            // Also keep track of original input attributes and compare against recreated input attributes
            // built from computed attributes
            List<ComAttribute> originalInputAttributes = new List<ComAttribute>();
            List<ComAttribute> computedAttributesPreservedInput = new List<ComAttribute>();
            List<ComAttribute> computedAttributesNoPreservedInput = new List<ComAttribute>();
            string[] ussFiles, voaFiles, eavFiles;
            inputConfig.GetInputData(out ussFiles, out voaFiles, out eavFiles);
            string[] evoaFiles = new string[ussFiles.Length];
            var afutility = new AFUtility();
            for (int i = 0; i < ussFiles.Length; i++)
            {
                var uss = new SpatialStringClass();
                uss.LoadFrom(ussFiles[i], false);
                var voa = new IUnknownVectorClass();
                voa.LoadFrom(voaFiles[i], false);
                originalInputAttributes.AddRange(voa.ToIEnumerable<ComAttribute>());
                lm.ComputeAnswer(uss, voa, true);
                computedAttributesPreservedInput.AddRange(voa.ToIEnumerable<ComAttribute>());
                voa.LoadFrom(voaFiles[i], false);
                lm.ComputeAnswer(uss, voa, false);
                computedAttributesNoPreservedInput.AddRange(voa.ToIEnumerable<ComAttribute>());
                var fileName = Path.ChangeExtension(eavFiles[i], "fake.voa");
                voa.SaveTo(fileName, false, typeof(AttributeStorageManagerClass).GUID.ToString("B"));
                evoaFiles[i] = fileName;
            }
            var results = lm.Encoder.GetFeatureVectorAndAnswerCollections(ussFiles, voaFiles, evoaFiles);
            double[][] inputs = results.Item1;
            int[] outputs = results.Item2;
            Assert.AreEqual(1.0, LearningMachine.GetAccuracyScore(lm.Classifier, inputs, outputs));

            var recreatedInputAttributes = computedAttributesPreservedInput.SelectMany(a => a.Name.Equals("Document", StringComparison.OrdinalIgnoreCase)
                ? afutility.QueryAttributes(a.SubAttributes, "Page", false).ToIEnumerable<ComAttribute>()
                : Enumerable.Repeat(a, 1));
            CollectionAssert.AreEquivalent(originalInputAttributes, recreatedInputAttributes);

            var inputAttributesNotPreserved = computedAttributesNoPreservedInput.SelectMany(a => a.Name.Equals("Document", StringComparison.OrdinalIgnoreCase)
                ? afutility.QueryAttributes(a.SubAttributes, "Page", false).ToIEnumerable<ComAttribute>()
                : Enumerable.Repeat(a, 1));
            CollectionAssert.IsEmpty(inputAttributesNotPreserved);
        }

        // Test comparison method
        [Test, Category("LearningMachine")]
        public static void TestConfigurationEqualTo()
        {
            SetDocumentCategorizationFiles();
            var inputConfig1 = new InputConfiguration
            {
                InputPath = _inputFolder.Last(),
                InputPathType = InputType.Folder,
                AttributesPath = "",
                AnswerPath = "$FileOf($DirOf(<SourceDocName>))",
                TrainingSetPercentage = 80
            };
            var inputConfig2 = new InputConfiguration
            {
                InputPath = _inputFolder.Last(),
                InputPathType = InputType.Folder,
                AttributesPath = "",
                AnswerPath = "$FileOf($DirOf(<SourceDocName>))",
                TrainingSetPercentage = 80
            };
            var lm1 = new LearningMachine
            {
                InputConfig = inputConfig1,
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization, new SpatialStringFeatureVectorizer(null, 5, 2000)),
                Classifier = new MulticlassSupportVectorMachineClassifier { AutomaticallyChooseComplexityValue = false }
            };
            var lm2 = new LearningMachine
            {
                InputConfig = inputConfig2,
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization, new SpatialStringFeatureVectorizer(null, 5, 2000)),
                Classifier = new MulticlassSupportVectorMachineClassifier { AutomaticallyChooseComplexityValue = false }
            };

            Assert.That(lm1.IsConfigurationEqualTo(lm2));

            // Changing InputConfig makes not equal
            lm1.InputConfig.AttributesPath = "Testing";
            Assert.That(!lm1.IsConfigurationEqualTo(lm2));
            lm1.InputConfig.AttributesPath = null;
            Assert.That(lm1.IsConfigurationEqualTo(lm2));

            // Changing Encoder makes not equal
            lm1.Encoder.AutoBagOfWords.Enabled = false;
            Assert.That(!lm1.IsConfigurationEqualTo(lm2));
            lm1.Encoder.AutoBagOfWords.Enabled = true;
            Assert.That(lm1.IsConfigurationEqualTo(lm2));

            // Changing Classifier makes not equal
            lm1.Classifier = new MultilabelSupportVectorMachineClassifier();
            Assert.That(!lm1.IsConfigurationEqualTo(lm2));
            lm1.Classifier = new MulticlassSupportVectorMachineClassifier { AutomaticallyChooseComplexityValue = false };
            Assert.That(lm1.IsConfigurationEqualTo(lm2));

            // Computing features for one machine does not affect configuration equality
            lm1.ComputeEncodings();
            Assert.That(lm1.Encoder.AreEncodingsComputed);
            Assert.That(lm1.IsConfigurationEqualTo(lm2));
            Assert.That(lm2.IsConfigurationEqualTo(lm1));

            // Training one machine does not affect configuration equality
            lm1.TrainMachine();
            Assert.That(lm1.IsConfigurationEqualTo(lm2));
            Assert.That(lm2.IsConfigurationEqualTo(lm1));

            // Test setting values to null
            lm2.InputConfig = null;
            Assert.That(!lm1.IsConfigurationEqualTo(lm2));
            Assert.That(!lm2.IsConfigurationEqualTo(lm1));
            lm2.InputConfig = lm1.InputConfig;
            Assert.That(lm1.IsConfigurationEqualTo(lm2));

            lm2.Encoder = null;
            Assert.That(!lm1.IsConfigurationEqualTo(lm2));
            Assert.That(!lm2.IsConfigurationEqualTo(lm1));
            lm2.Encoder = lm1.Encoder;
            Assert.That(lm1.IsConfigurationEqualTo(lm2));

            lm2.Classifier = null;
            Assert.That(!lm1.IsConfigurationEqualTo(lm2));
            Assert.That(!lm2.IsConfigurationEqualTo(lm1));
            lm2.Classifier = lm1.Classifier;
            Assert.That(lm1.IsConfigurationEqualTo(lm2));
        }

        //// Test shallow clone method
        [Test, Category("LearningMachine")]
        public static void TestShallowClone()
        {
            SetDocumentCategorizationFiles();
            var inputConfig1 = new InputConfiguration
            {
                InputPath = _inputFolder.Last(),
                InputPathType = InputType.Folder,
                AttributesPath = "",
                AnswerPath = "$FileOf($DirOf(<SourceDocName>))",
                TrainingSetPercentage = 80
            };
            var lm1 = new LearningMachine
            {
                InputConfig = inputConfig1,
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization, new SpatialStringFeatureVectorizer(null, 5, 2000)),
                Classifier = new MulticlassSupportVectorMachineClassifier { AutomaticallyChooseComplexityValue = false }
            };
            var lm2 = lm1.ShallowClone();

            // Changing settings of a member changes for both original and clone
            lm1.InputConfig.AttributesPath = "Testing";
            Assert.That(lm1.IsConfigurationEqualTo(lm2));

            lm1.Encoder.AutoBagOfWords.Enabled = false;
            Assert.That(lm1.IsConfigurationEqualTo(lm2));

            ((MulticlassSupportVectorMachineClassifier)lm1.Classifier).AutomaticallyChooseComplexityValue = true;
            Assert.That(lm1.IsConfigurationEqualTo(lm2));

            // Changing a setting on the original won't affect the clone
            lm1.Classifier = null;
            Assert.That(!lm1.IsConfigurationEqualTo(lm2));
            Assert.That(!lm2.IsConfigurationEqualTo(lm1));
        }

        // Test input config equals
        [Test, Category("LearningMachine")]
        public static void TestInputConfigEquals()
        {
            var inputConfig1 = new InputConfiguration
            {
                InputPath = "Dummy",
                InputPathType = InputType.Folder,
                AttributesPath = "",
                AnswerPath = "$FileOf($DirOf(<SourceDocName>))",
                TrainingSetPercentage = 80
            };

            var inputConfig2 = new InputConfiguration
            {
                InputPath = "Dummy",
                InputPathType = InputType.Folder,
                AttributesPath = "",
                AnswerPath = "$FileOf($DirOf(<SourceDocName>))",
                TrainingSetPercentage = 80
            };

            Assert.AreEqual(inputConfig1, inputConfig2);

            inputConfig1.InputPath = "Testing";
            Assert.AreNotEqual(inputConfig1, inputConfig2);
            inputConfig1.InputPath = inputConfig2.InputPath;
            Assert.AreEqual(inputConfig1, inputConfig2);

            inputConfig1.AttributesPath = "Testing";
            Assert.AreNotEqual(inputConfig1, inputConfig2);
            inputConfig1.AttributesPath = inputConfig2.AttributesPath;
            Assert.AreEqual(inputConfig1, inputConfig2);

            inputConfig1.AnswerPath = "Testing";
            Assert.AreNotEqual(inputConfig1, inputConfig2);
            inputConfig1.AnswerPath = inputConfig2.AnswerPath;
            Assert.AreEqual(inputConfig1, inputConfig2);

            inputConfig1.InputPathType = InputType.TextFile;
            Assert.AreNotEqual(inputConfig1, inputConfig2);
            inputConfig1.InputPathType = inputConfig2.InputPathType;
            Assert.AreEqual(inputConfig1, inputConfig2);

            inputConfig1.TrainingSetPercentage = 20;
            Assert.AreNotEqual(inputConfig1, inputConfig2);
            inputConfig1.TrainingSetPercentage = inputConfig2.TrainingSetPercentage;
            Assert.AreEqual(inputConfig1, inputConfig2);

            // Test that null == ""
            inputConfig1.InputPath = inputConfig1.AttributesPath = inputConfig1.AnswerPath = null;
            inputConfig2.InputPath = inputConfig2.AttributesPath = inputConfig2.AnswerPath = "";
            Assert.AreEqual(inputConfig1, inputConfig2);
        }

        // Test serializing multi class svm
        [Test, Category("LearningMachine")]
        public static void TestSerializingMulticlass()
        {
            SetDocumentCategorizationFiles();
            var inputConfig1 = new InputConfiguration
            {
                InputPath = _inputFolder.Last(),
                InputPathType = InputType.Folder,
                AttributesPath = "",
                AnswerPath = "$FileOf($DirOf(<SourceDocName>))",
                TrainingSetPercentage = 80
            };
            var lm1 = new LearningMachine
            {
                InputConfig = inputConfig1,
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization, new SpatialStringFeatureVectorizer(null, 5, 2000)),
                Classifier = new MulticlassSupportVectorMachineClassifier()
            };

            lm1.TrainMachine();
            var savedMachine = new System.IO.MemoryStream();
            lm1.Save(savedMachine);
            savedMachine.Position = 0;
            var lm2 = LearningMachine.Load(savedMachine);

            Assert.That(lm1.IsConfigurationEqualTo(lm2));

            // Test output
            string[] ussFiles, voaFiles, answers;
            lm2.InputConfig.GetInputData(out ussFiles, out voaFiles, out answers);
            for (int i = 0; i < ussFiles.Length; i++)
            {
                var uss = new SpatialStringClass();
                uss.LoadFrom(ussFiles[i], false);
                var voa = new IUnknownVectorClass();
                lm2.ComputeAnswer(uss, voa, false);
                Assert.AreEqual(answers[i], ((ComAttribute)voa.At(0)).Value.String);
            }
        }

        // Test serializing multi label svm
        [Test, Category("LearningMachine")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multilabel")]
        public static void TestSerializingMultilabel()
        {
            SetDocumentCategorizationFiles();
            var inputConfig1 = new InputConfiguration
            {
                InputPath = _inputFolder.Last(),
                InputPathType = InputType.Folder,
                AttributesPath = "",
                AnswerPath = "$FileOf($DirOf(<SourceDocName>))",
                TrainingSetPercentage = 80
            };
            var lm1 = new LearningMachine
            {
                InputConfig = inputConfig1,
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization, new SpatialStringFeatureVectorizer(null, 5, 2000)),
                Classifier = new MultilabelSupportVectorMachineClassifier()
            };

            lm1.TrainMachine();
            var savedMachine = new System.IO.MemoryStream();
            lm1.Save(savedMachine);
            savedMachine.Position = 0;
            var lm2 = LearningMachine.Load(savedMachine);

            Assert.That(lm1.IsConfigurationEqualTo(lm2));

            // Test output
            string[] ussFiles, voaFiles, answers;
            lm2.InputConfig.GetInputData(out ussFiles, out voaFiles, out answers);
            for (int i = 0; i < ussFiles.Length; i++)
            {
                var uss = new SpatialStringClass();
                uss.LoadFrom(ussFiles[i], false);
                var voa = new IUnknownVectorClass();
                lm2.ComputeAnswer(uss, voa, false);
                Assert.AreEqual(answers[i], ((ComAttribute)voa.At(0)).Value.String);
            }
        }

        // Test serializing neural network
        [Test, Category("LearningMachine")]
        public static void TestSerializingNeuralNetToFile()
        {
            SetPaginationFiles();
            string[] listContents = Directory.GetFiles(_inputFolder.Last(), "*.tif", SearchOption.AllDirectories);
            File.WriteAllLines(_csvPath, listContents);

            var lm1 = new LearningMachine
            {
                InputConfig = new InputConfiguration
                {
                    InputPath = _csvPath,
                    InputPathType = InputType.TextFile,
                    AttributesPath = "<SourceDocName>.protofeatures.voa",
                    AnswerPath = "<SourceDocName>.eav",
                    TrainingSetPercentage = 50
                },
                // https://extract.atlassian.net/browse/ISSUE-14479
                // Updated settings because new, TFIDF ordering of features changed the effects of the random initialization
                // (Using non-@Feature attributes meant patient names, e.g., were being learned as bag-of-word features)
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.Pagination, attributeFilter: "*@Feature"),
                Classifier = new NeuralNetworkClassifier { UseCrossValidationSets = true },
                RandomNumberSeed = 1
            };
            lm1.TrainMachine();
            lm1.Save(_savedMachinePath);
            var lm2 = LearningMachine.Load(_savedMachinePath);

            Assert.That(lm1.IsConfigurationEqualTo(lm2));

            // Test output
            var results = lm2.TestMachine();
            Assert.Greater(results.Item1.Match(_ => Double.NaN, cm => cm.FScore), 0.9);
            Assert.Greater(results.Item2.Match(_ => Double.NaN, cm => cm.FScore), 0.6);
        }

        // Test serializing label attributes configuration
        [Test, Category("LearningMachine")]
        public static void TestSerializingLabelAttributesSettings()
        {
            var inputConfig1 = new InputConfiguration
            {
                InputPath = "Dummy",
                InputPathType = InputType.Folder,
                AttributesPath = "",
                AnswerPath = "$FileOf($DirOf(<SourceDocName>))",
                TrainingSetPercentage = 80
            };
            var lm1 = new LearningMachine
            {
                InputConfig = inputConfig1,
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.AttributeCategorization),
                Classifier = new MulticlassSupportVectorMachineClassifier(),
                LabelAttributesSettings = new LabelAttributes
                {
                    AttributesToLabelPath = @"<SourceDocName>.voa",
                    SourceOfLabelsPath = @"$DirOf(<SourceDocName>)\ExpectedRedactions\$FileOf(<SourceDocName>).evoa",
                    DestinationPath = @"<SourceDocName>.labeled.voa"
                }
            };
            lm1.LabelAttributesSettings.CategoryQueryPairs.Add(
                new CategoryQueryPair
                {
                    Category = "DOB",
                    CategoryIsXPath = false,
                    Query = "//@DOB"
                });

            var savedMachine = new System.IO.MemoryStream();
            lm1.Save(savedMachine);
            savedMachine.Position = 0;
            var lm2 = LearningMachine.Load(savedMachine);

            Assert.That(lm1.IsConfigurationEqualTo(lm2));

            lm2.LabelAttributesSettings = new LabelAttributes();
            Assert.False(lm1.IsConfigurationEqualTo(lm2));

            lm2.LabelAttributesSettings = lm1.LabelAttributesSettings.DeepClone();
            Assert.That(lm1.IsConfigurationEqualTo(lm2));

            lm2.LabelAttributesSettings.AttributesToLabelPath = "";
            Assert.False(lm1.IsConfigurationEqualTo(lm2));

            lm2.LabelAttributesSettings = lm1.LabelAttributesSettings.DeepClone();
            lm2.LabelAttributesSettings.SourceOfLabelsPath = "";
            Assert.False(lm1.IsConfigurationEqualTo(lm2));

            lm2.LabelAttributesSettings = lm1.LabelAttributesSettings.DeepClone();
            lm2.LabelAttributesSettings.DestinationPath = "";
            Assert.False(lm1.IsConfigurationEqualTo(lm2));

            lm2.LabelAttributesSettings = lm1.LabelAttributesSettings.DeepClone();
            lm2.LabelAttributesSettings.CategoryQueryPairs.Clear();
            Assert.False(lm1.IsConfigurationEqualTo(lm2));

            lm2.LabelAttributesSettings = lm1.LabelAttributesSettings.DeepClone();
            lm2.LabelAttributesSettings.CategoryQueryPairs.Add(
                lm2.LabelAttributesSettings.CategoryQueryPairs[0].ShallowClone());
            Assert.False(lm1.IsConfigurationEqualTo(lm2));

            lm2.LabelAttributesSettings = lm1.LabelAttributesSettings.DeepClone();
            lm2.LabelAttributesSettings.CategoryQueryPairs[0].Category = "dob";
            Assert.False(lm1.IsConfigurationEqualTo(lm2));

            lm2.LabelAttributesSettings = lm1.LabelAttributesSettings.DeepClone();
            lm2.LabelAttributesSettings.CategoryQueryPairs[0].Query = "*";
            Assert.False(lm1.IsConfigurationEqualTo(lm2));

            lm2.LabelAttributesSettings = lm1.LabelAttributesSettings.DeepClone();
            lm2.LabelAttributesSettings.CategoryQueryPairs[0].CategoryIsXPath = true;
            Assert.False(lm1.IsConfigurationEqualTo(lm2));
        }

        // Confirm that label attributes configuration is not saved when usage
        // is not AttributeCategorization
        [Test, Category("LearningMachine")]
        public static void TestConditionalSerializationOfLabelAttributesSettings()
        {
            var inputConfig1 = new InputConfiguration
            {
                InputPath = "Dummy",
                InputPathType = InputType.Folder,
                AttributesPath = "",
                AnswerPath = "$FileOf($DirOf(<SourceDocName>))",
                TrainingSetPercentage = 80
            };
            var lm1 = new LearningMachine
            {
                InputConfig = inputConfig1,
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization),
                Classifier = new MulticlassSupportVectorMachineClassifier(),
                LabelAttributesSettings = new LabelAttributes
                {
                    AttributesToLabelPath = @"<SourceDocName>.voa",
                    SourceOfLabelsPath = @"$DirOf(<SourceDocName>)\ExpectedRedactions\$FileOf(<SourceDocName>).evoa",
                    DestinationPath = @"<SourceDocName>.labeled.voa"
                }
            };
            lm1.LabelAttributesSettings.CategoryQueryPairs.Add(
                new CategoryQueryPair
                {
                    Category = "DOB",
                    CategoryIsXPath = false,
                    Query = "//@DOB"
                });

            var savedMachine = new System.IO.MemoryStream();
            lm1.Save(savedMachine);
            savedMachine.Position = 0;
            var lm2 = LearningMachine.Load(savedMachine);
            Assert.False(lm1.IsConfigurationEqualTo(lm2));

            lm1.Encoder = new LearningMachineDataEncoder(LearningMachineUsage.Pagination);
            savedMachine = new System.IO.MemoryStream();
            lm1.Save(savedMachine);
            savedMachine.Position = 0;
            lm2 = LearningMachine.Load(savedMachine);
            Assert.False(lm1.IsConfigurationEqualTo(lm2));
        }

        // Test label attributes process
        [Test, Category("LearningMachine")]
        public static void TestLabelAttributesProcess()
        {
            SetPaginationFiles();
            var inputConfig = new InputConfiguration
            {
                InputPath = _inputFolder.Last(),
                InputPathType = InputType.Folder,
                AttributesPath = "",
                AnswerPath = "",
                TrainingSetPercentage = 80
            };
            var labelAttributes = new LabelAttributes
            {
                AttributesToLabelPath = @"<SourceDocName>.candidates.voa",
                SourceOfLabelsPath = @"<SourceDocName>.voa",
                DestinationPath = @"<SourceDocName>.nunit_labeled1.voa"
            };

            labelAttributes.CategoryQueryPairs.Add(new CategoryQueryPair
                {
                    Category = "name()",
                    CategoryIsXPath = true,
                    Query = "/*/Document/DocumentData/PatientInfo/DOB"
                });

            labelAttributes.Process(inputConfig);

            var afutil = new UCLID_AFUTILSLib.AFUtility();
            foreach (var file in Directory.GetFiles(_inputFolder.Last(), "*.nunit_labeled1.voa"))
            {
                var labeledAttributes = afutil.QueryAttributes(afutil.GetAttributesFromFile(file), "*{AttributeType}", false);
                Assert.GreaterOrEqual(labeledAttributes.Size(), 1);

                var xPathContext = new XPathContext(labeledAttributes);
                var labeledAsDOB = xPathContext.FindAllOfType<ComAttribute>("/*/*[AttributeType = 'DOB']");
                Assert.AreEqual(labeledAttributes.Size(), labeledAsDOB.Count());
            }
        }

        // Test more complicated label attributes process
        [Test, Category("LearningMachine")]
        public static void TestLabelAttributesProcess2()
        {
            SetPaginationFiles();
            var inputConfig = new InputConfiguration
            {
                InputPath = _inputFolder.Last(),
                InputPathType = InputType.Folder,
                AttributesPath = "",
                AnswerPath = "",
                TrainingSetPercentage = 80
            };
            var labelAttributes = new LabelAttributes
            {
                AttributesToLabelPath = @"<SourceDocName>.candidates.voa",
                SourceOfLabelsPath = @"<SourceDocName>.voa",
                DestinationPath = @"<SourceDocName>.nunit_labeled2.voa",
                CreateEmptyLabelForNonMatching = true
            };

            labelAttributes.CategoryQueryPairs.Add(new CategoryQueryPair
                {
                    Category = "name()",
                    CategoryIsXPath = true,
                    Query = "/*/Document/DocumentData/PatientInfo/DOB"
                });

            labelAttributes.CategoryQueryPairs.Add(new CategoryQueryPair
                {
                    Category = "name()",
                    CategoryIsXPath = true,
                    Query = "/*/Document/DocumentData/Test/CollectionDate"
                });

            labelAttributes.Process(inputConfig);

            var afutil = new UCLID_AFUTILSLib.AFUtility();
            foreach (var file in Directory.GetFiles(_inputFolder.Last(), "*.nunit_labeled2.voa"))
            {
                var labeledAttributes = afutil.QueryAttributes(afutil.GetAttributesFromFile(file), "*{AttributeType}", false);
                Assert.GreaterOrEqual(labeledAttributes.Size(), 1);

                var xPathContext = new XPathContext(labeledAttributes);
                var labeledAsDOB = xPathContext.FindAllOfType<ComAttribute>("/*/*[AttributeType = 'DOB']").ToList();
                var labeledAsCollectionDate = xPathContext.FindAllOfType<ComAttribute>("/*/*[AttributeType = 'CollectionDate']").ToList();
                Assert.GreaterOrEqual(labeledAsDOB.Count(), 1);
                Assert.GreaterOrEqual(labeledAsCollectionDate.Count(), labeledAsDOB.Count());
                Assert.Greater(labeledAttributes.Size(), labeledAsDOB.Count() + labeledAsCollectionDate.Count());
            }

            var specificFile = Directory.GetFiles(_inputFolder.Last(), "*.nunit_labeled2.voa").First();
            var attributes = afutil.GetAttributesFromFile(specificFile).ToIEnumerable<ComAttribute>().ToList();
            var collectionDateIndexes = attributes.Select((a, i) =>
                a.SubAttributes.ToIEnumerable<ComAttribute>()
                    .FirstOrDefault(sa => sa.Name == "AttributeType")?.Value.String == "CollectionDate"
                    ? i
                    : (int?)null)
                .Where(i => i != null);
            Assert.AreEqual("2,8,17", string.Join(",", collectionDateIndexes));

            var dobIndexes = attributes.Select((a, i) =>
                a.SubAttributes.ToIEnumerable<ComAttribute>()
                    .FirstOrDefault(sa => sa.Name == "AttributeType")?.Value.String == "DOB"
                    ? i
                    : (int?)null)
                .Where(i => i != null);
            Assert.AreEqual("13,16", string.Join(",", dobIndexes));
        }

        // Confirm that a 10.4 machine loads in later version
        [Test, Category("LearningMachine")]
        public static void TestLoading10_4Machine()
        {
            var path = _testFiles.GetFile("Resources.LearningMachine.10.4.lm");
            Assert.DoesNotThrow(() => LearningMachine.Load(path));
        }

        // Confirm that a 10.4 machine saved in a later version is smaller (since it has been compressd)
        // https://extract.atlassian.net/browse/ISSUE-14140
        [Test, Category("LearningMachine")]
        public static void TestSavingCompressedMachine()
        {
            var path = _testFiles.GetFile("Resources.LearningMachine.10.4.lm");
            long uncompressedSize = new FileInfo(path).Length;
            LearningMachine.Load(path).Save(path);
            long compressedSize = new FileInfo(path).Length;

            Assert.Less(compressedSize, uncompressedSize);
        }

        // Test multi-threaded access to a large machine
        // https://extract.atlassian.net/browse/ISSUE-14474
        [Test, Category("LearningMachine")]
        public static void Test100ThreadSimultaneousAccess()
        {
            SetDocumentCategorizationFiles();
            var spatialStrings = Directory.GetFiles(_inputFolder.Last(), "*.tif.uss", SearchOption.AllDirectories)
                .Select(path =>
                {
                    var ss = new SpatialString();
                    ss.LoadFrom(path, false);
                    return ss;
                }).ToList();
            var machinePath = _testFiles.GetFile("Resources.LearningMachine.Large.lm");
            var answers = new ConcurrentBag<KeyValuePair<int, string>>();
            var exceptions = new ConcurrentBag<Exception>();
            Action<object> compute = o =>
            {
                try
                {
                    var attrr = new IUnknownVector();
                    var idx = (int)o % spatialStrings.Count;
                    var ss = spatialStrings[idx];
                    LearningMachine.ComputeAnswer(machinePath, ss, attrr, false);
                    var doctype = ((IAttribute)attrr.At(0)).Value.String;
                    answers.Add(new KeyValuePair<int, string>(idx, doctype));
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            };

            var threads = Enumerable.Repeat<Func<Thread>>(
                () => new Thread(new ParameterizedThreadStart(compute))
                , spatialStrings.Count * 10)
                .Select(f => f())
                .ToList();

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            int i = 0;
            foreach (var thread in threads)
            {
                thread.Start(i++);
            }

            foreach(var thread in threads)
            {
                thread.Join();
            }
            // Finished in less time than possible if loading 100 times
            // (finishes in ~ 2 seconds on my dev machine)
            Assert.Less(stopwatch.ElapsedMilliseconds, 10000);

            // There were no exceptions (e.g., no out-of-memory issues with 100 62MB LMs)
            CollectionAssert.IsEmpty(exceptions);
            Assert.AreEqual(100, answers.Count);

            // The answers aren't all the same
            Assert.Greater(answers.Select(kv => kv.Value).Distinct().Count(), 1);

            // The answer is consistent for each index (each input string)
            foreach (var group in answers.GroupBy(kv => kv.Key))
            {
                Assert.AreEqual(10, group.Count());
                group.Select(kv => kv.Value).Distinct().Single();
            }
        }

        // Test building a classifier for yes/no bubbles using bitmap features
        [Test, Category("LearningMachine")]
        public static void TestBitmapFeature()
        {
            SetBitmapFiles();
            var rulesetPath = _testFiles.GetFile("Resources.LearningMachine.Bitmap.bubble.rsd");
            _testFiles.GetFile("Resources.LearningMachine.Bitmap.bubble.dat");

            // Run the protofeature creation rules
            Parallel.ForEach(Directory.GetFiles(_inputFolder.Last(), "*.tif"), imagePath =>
            {
                var uss = new SpatialStringClass();
                uss.LoadFrom(imagePath + ".uss", false);
                var doc = new AFDocumentClass { Text = uss };
                var ruleset = new RuleSetClass();
                ruleset.LoadFrom(rulesetPath, false);
                ruleset.ExecuteRulesOnText(doc, null, null, null);
            });

            // Create labeled attributes
            var inputConfig = new InputConfiguration
            {
                InputPath = _inputFolder.Last(),
                InputPathType = InputType.Folder,
                AttributesPath = "<SourceDocName>.labeled.voa",
                AnswerPath = "",
                TrainingSetPercentage = 80
            };
            var labelAttributes = new LabelAttributes
            {
                AttributesToLabelPath = @"<SourceDocName>.protofeatures.voa",
                SourceOfLabelsPath = @"<SourceDocName>.evoa",
                DestinationPath = @"<SourceDocName>.labeled.voa"
            };

            labelAttributes.CategoryQueryPairs.Add(new CategoryQueryPair
                {
                    Category = "@Type",
                    CategoryIsXPath = true,
                    Query = "/*/*"
                });

            labelAttributes.Process(inputConfig);

            // Train/test the machine
            var lm = new LearningMachine
            {
                InputConfig = new InputConfiguration
                {
                    InputPath = _inputFolder.Last(),
                    InputPathType = InputType.Folder,
                    AttributesPath = "<SourceDocName>.labeled.voa",
                    TrainingSetPercentage = 80
                },
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.AttributeCategorization, null, "*@Feature"),
                Classifier = new NeuralNetworkClassifier { HiddenLayers = new[] { 50 } }
            };
            var results = lm.TrainMachine();
            Assert.Greater(results.trainingSet.Match(gcm => gcm.OverallAgreement, cm => cm.Accuracy), 0.96);
            Assert.Greater(results.testingSet.Match(gcm => gcm.OverallAgreement, cm => cm.Accuracy), 0.93);
        }

        // https://extract.atlassian.net/browse/ISSUE-14979
        [SuppressMessage("Microsoft.Globalization", "CA1308")]
        [Test, Category("LearningMachine")]
        public static void MultiplyCasedDocType()
        {
            SetDocumentCategorizationFiles();
            var csvContents = Directory.GetFiles(_inputFolder.Last(), "*.tif", SearchOption.AllDirectories)
                .Select(imagePath => string.Join(",", imagePath, Path.GetFileName(Path.GetDirectoryName(imagePath)))).ToList();
            csvContents.Add(csvContents.Last().ToUpperInvariant());
            csvContents.Add(csvContents.Last().ToLowerInvariant());
            File.WriteAllLines(_csvPath, csvContents);

            var lm = new LearningMachine
            {
                InputConfig = new InputConfiguration
                {
                    InputPath = _csvPath,
                    InputPathType = InputType.Csv,
                    AttributesPath = "",
                    AnswerPath = "",
                    TrainingSetPercentage = 80
                },
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization, new SpatialStringFeatureVectorizer(null, 5, 2000)),
                Classifier = new MulticlassSupportVectorMachineClassifier()
            };
            lm.TrainMachine();
        }

        // Write out features to a CSV and train a machine with the CSV
        [Test, Category("LearningMachine")]
        public static void CsvDocumentCategorization()
        {
            SetDocumentCategorizationFiles();
            var inputConfig = new InputConfiguration
            {
                InputPath = _inputFolder.Last(),
                InputPathType = InputType.Folder,
                AttributesPath = "",
                AnswerPath = "$FileOf($DirOf(<SourceDocName>))",
                TrainingSetPercentage = 80
            };
            var lm = new LearningMachine
            {
                InputConfig = inputConfig,
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization, new SpatialStringFeatureVectorizer(null, 5, 2000)),
                Classifier = new MulticlassSupportVectorMachineClassifier(),
                CsvOutputFile = Path.Combine(_inputFolder.Last(), "features")
            };
            lm.ComputeEncodings();
            lm.WriteDataToCsv(_ => { }, CancellationToken.None);

            var trainPath = Path.Combine(_inputFolder.Last(), "features.train.csv");
            var testPath = Path.Combine(_inputFolder.Last(), "features.test.csv");
            Assert.That(File.Exists(trainPath));
            Assert.That(File.Exists(testPath));

            var trainingData = File.ReadAllLines(trainPath);

            // CSV is uss path, index, class name, features
            var trainInputs = trainingData.Select(l => l.Split(',').Skip(3).Select(s => Convert.ToDouble(s, CultureInfo.InvariantCulture)).ToArray()).ToArray();
            var trainOutputs = trainingData.Select(l => l.Split(',').Skip(2).First()).Select(s => lm.Encoder.AnswerNameToCode[s.Unquote()]).ToArray();

            // Data is not standardized so all are integers
            // (Moved this here because now running training will standardize the input array, so as to save memory required to copy the whole thing)
            Assert.That(trainInputs.SelectMany(a => a).Select(d => d % 1).All(d => d == 0));

            lm.Classifier.TrainClassifier(trainInputs, trainOutputs, new Random(0));

            var results = lm.TestMachine();
            Assert.Greater(results.trainingSet.Match(gcm => gcm.OverallAgreement, cm => cm.Accuracy), 0.99);
            Assert.Greater(results.testingSet.Match(gcm => gcm.OverallAgreement, cm => cm.Accuracy), 0.99);

            // Standardize
            lm.StandardizeFeaturesForCsvOutput = true;
            lm.WriteDataToCsv(_ => { }, CancellationToken.None);
            trainingData = File.ReadAllLines(trainPath);
            trainInputs = trainingData.Select(l => l.Split(',').Skip(3).Select(s => Convert.ToDouble(s, CultureInfo.InvariantCulture)).ToArray()).ToArray();

            // At least some are not integers
            Assert.That(trainInputs.SelectMany(a => a).Select(d => d % 1).Any(d => d != 0));
        }

        // Write out features to a CSV and train a machine with the CSV
        [Test, Category("LearningMachine")]
        public static void CsvPagination()
        {
            SetPaginationFiles();
            var lm = new LearningMachine
            {
                InputConfig = new InputConfiguration
                {
                    InputPath = _inputFolder.Last(),
                    InputPathType = InputType.Folder,
                    AttributesPath = "<SourceDocName>.protofeatures.voa",
                    AnswerPath = "<SourceDocName>.eav",
                    TrainingSetPercentage = 50
                },
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.Pagination, attributeFilter: "*@Feature"),
                Classifier = new NeuralNetworkClassifier { UseCrossValidationSets = true },
                RandomNumberSeed = 1,
                CsvOutputFile = Path.Combine(_inputFolder.Last(), "features")
            };
            lm.ComputeEncodings();
            lm.WriteDataToCsv(_ => { }, CancellationToken.None);

            var trainPath = Path.Combine(_inputFolder.Last(), "features.train.csv");
            var testPath = Path.Combine(_inputFolder.Last(), "features.test.csv");
            Assert.That(File.Exists(trainPath));
            Assert.That(File.Exists(testPath));

            var trainingData = File.ReadAllLines(trainPath);

            // CSV is uss path, index, class name, features
            var trainInputs = trainingData.Select(l => l.Split(',').Skip(3).Select(s => Convert.ToDouble(s, CultureInfo.InvariantCulture)).ToArray()).ToArray();
            var trainOutputs = trainingData.Select(l => l.Split(',').Skip(2).First()).Select(s => lm.Encoder.AnswerNameToCode[s.Unquote()]).ToArray();
            lm.Classifier.TrainClassifier(trainInputs, trainOutputs, new Random(1));

            var results = lm.TestMachine();
            Assert.Greater(results.trainingSet.Match(_ => Double.NaN, cm => cm.FScore), 0.9);
            Assert.Greater(results.testingSet.Match(_ => Double.NaN, cm => cm.FScore), 0.6);
        }

        // Write out features to a CSV and train a machine with the CSV
        [Test, Category("LearningMachine")]
        public static void CsvAttributeCategorization()
        {
            SetPaginationFiles();
            var lm = new LearningMachine
            {
                InputConfig = new InputConfiguration
                {
                    InputPath = _inputFolder.Last(),
                    InputPathType = InputType.Folder,
                    AttributesPath = "<SourceDocName>.labeled.voa",
                    TrainingSetPercentage = 80
                },
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.AttributeCategorization, attributeFilter: "*@Feature"),
                Classifier = new MulticlassSupportVectorMachineClassifier(),
                CsvOutputFile = Path.Combine(_inputFolder.Last(), "features")
            };
            lm.ComputeEncodings();
            lm.WriteDataToCsv(_ => { }, CancellationToken.None);

            var trainPath = Path.Combine(_inputFolder.Last(), "features.train.csv");
            var testPath = Path.Combine(_inputFolder.Last(), "features.test.csv");
            Assert.That(File.Exists(trainPath));
            Assert.That(File.Exists(testPath));

            var trainingData = File.ReadAllLines(trainPath);

            // CSV is uss path, index, class name, features
            var trainInputs = trainingData.Select(l => l.Split(',').Skip(3).Select(s => Convert.ToDouble(s, CultureInfo.InvariantCulture)).ToArray()).ToArray();
            var trainOutputs = trainingData.Select(l => l.Split(',').Skip(2).First()).Select(s => lm.Encoder.AnswerNameToCode[s.Unquote()]).ToArray();
            lm.Classifier.TrainClassifier(trainInputs, trainOutputs, new Random(0));

            var results = lm.TestMachine();
            Assert.AreEqual(1.0, results.trainingSet.Match(_ => Double.NaN, cm => cm.FScore));
            Assert.Greater(results.testingSet.Match(_ => Double.NaN, cm => cm.FScore), 0.85);
        }

        [Test, Category("LearningMachine")]
        public static void PrecisionRecallMicroAverage()
        {
            SetDocumentCategorizationFiles();
            string[] csvContents = Directory.GetFiles(_inputFolder.Last(), "*.tif", SearchOption.AllDirectories)
                .Select(imagePath => string.Join(",", imagePath, Path.GetFileName(Path.GetDirectoryName(imagePath)))).ToArray();

            // Create some extra data so that training and testing sets are different
            csvContents = csvContents.Concat(csvContents).ToArray();

            File.WriteAllLines(_csvPath, csvContents);

            var inputConfig = new InputConfiguration
            {
                InputPath = _csvPath,
                InputPathType = InputType.Csv,
                AttributesPath = "",
                AnswerPath = "",
                TrainingSetPercentage = 80
            };
            var lm = new LearningMachine
            {
                InputConfig = inputConfig,
                Encoder = new LearningMachineDataEncoder(
                    LearningMachineUsage.DocumentCategorization,
                    new SpatialStringFeatureVectorizer(null, 5, 20),
                    negativeClassName: "Unknown"),
                Classifier = new MultilabelSupportVectorMachineClassifier { CalibrateMachineToProduceProbabilities = true },
                UseUnknownCategory = true,
                UnknownCategoryCutoff = 0.60,
                TranslateUnknownCategory = true,
                TranslateUnknownCategoryTo = "Unknown"
            };
            var results = lm.TrainMachine();
            Assert.AreEqual(0.9, results.trainingSet.Match(gcm => gcm.OverallAgreement, cm => cm.Accuracy));
            Assert.AreEqual(0.9, results.testingSet.Match(gcm => gcm.OverallAgreement, cm => cm.Accuracy));

            var trainCM = lm.AccuracyData.Value.train;
            var testCM = lm.AccuracyData.Value.test;
            Assert.Greater(trainCM.PrecisionMicroAverage(), 0.99);
            Assert.AreEqual(trainCM.RecallMicroAverage(), 0.9);
            Assert.Greater(trainCM.FScoreMicroAverage(), 0.94);
            Assert.Less(trainCM.FScoreMicroAverage(), 0.95);

            Assert.AreEqual(testCM.RecallMicroAverage(), 0.9);
            Assert.Greater(testCM.FScoreMicroAverage(), 0.94);
            Assert.Less(testCM.FScoreMicroAverage(), 0.95);

            CollectionAssert.AreNotEquivalent(
                trainCM.Data.SelectMany(a => a).ToArray(),
                testCM.Data.SelectMany(a => a).ToArray());
        }

        // Test that an output document that is a blank page doesn't cause an error
        // https://extract.atlassian.net/browse/ISSUE-15615
        [Test, Category("LearningMachine")]
        public static void PaginationWithBlankPages()
        {
            SetPaginationFiles();
            var lm = new LearningMachine
            {
                InputConfig = new InputConfiguration
                {
                    InputPath = _inputFolder.Last(),
                    InputPathType = InputType.Folder,
                    AttributesPath = "<SourceDocName>.protofeatures.voa",
                    AnswerPath = "<SourceDocName>.eav",
                    TrainingSetPercentage = 50
                },
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.Pagination, autoBagOfWords: new SpatialStringFeatureVectorizer("", 5, 2000), attributeFilter: "*@Feature"),
                Classifier = new NeuralNetworkClassifier { UseCrossValidationSets = true }
            };
            lm.TrainMachine();

            lm.InputConfig.GetInputData(out var spatialStringFilePaths, out var attributeFilePaths, out var answerFiles);

            var ss = new SpatialStringClass();
            var voa = new IUnknownVectorClass();
            ss.LoadFrom(spatialStringFilePaths[3], false);

            var pages = ss.GetPages(true, "");
            pages.Remove(0);
            ss.CreateFromSpatialStrings(pages);
            voa.LoadFrom(attributeFilePaths[3], false);

            // No exception is thrown
            lm.ComputeAnswer(ss, voa, true);

            Assert.AreEqual(3, voa.Size());
            var firstDoc = (IAttribute)voa.At(0);

            // Confirm that the first document is a single, empty page
            Assert.AreEqual("Document", firstDoc.Name);
            Assert.AreEqual(ESpatialStringMode.kNonSpatialMode, firstDoc.Value.GetMode());
            Assert.AreEqual("N/A", firstDoc.Value.String);

            var pagesAttribute = (IAttribute)firstDoc.SubAttributes.At(0);
            Assert.AreEqual("Pages", pagesAttribute.Name);
            Assert.AreEqual("1", pagesAttribute.Value.String);
        }

        // Test that if a doc classifier is conigured to use text of page 1 that a blank page doesn't cause an error
        // https://extract.atlassian.net/browse/ISSUE-15615
        [Test, Category("LearningMachine")]
        public static void DocumentClassificationWithBlankPages()
        {
            SetDocumentCategorizationFiles();
            var inputConfig = new InputConfiguration
            {
                InputPath = _inputFolder.Last(),
                InputPathType = InputType.Folder,
                AttributesPath = "",
                AnswerPath = "$FileOf($DirOf(<SourceDocName>))",
                TrainingSetPercentage = 80
            };
            var lm = new LearningMachine
            {
                InputConfig = inputConfig,
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization, new SpatialStringFeatureVectorizer("1", 5, 2000)),
                Classifier = new MulticlassSupportVectorMachineClassifier { CalibrateMachineToProduceProbabilities = true },
                UseUnknownCategory = true,
                UnknownCategoryCutoff = 0.1,
                TranslateUnknownCategory = true,
                TranslateUnknownCategoryTo = "Unknown"
            };
            lm.TrainMachine();

            lm.InputConfig.GetInputData(out var spatialStringFilePaths, out var attributeFilePaths, out var answerFiles);

            var ss = new SpatialStringClass();
            var voa = new IUnknownVectorClass();
            ss.LoadFrom(spatialStringFilePaths[3], false);

            var pages = ss.GetPages(true, "");
            pages.Remove(0);
            ss.CreateFromSpatialStrings(pages);

            // No exception is thrown
            lm.ComputeAnswer(ss, voa, true);

            Assert.AreEqual(1, voa.Size());
            var firstAt = (IAttribute)voa.At(0);

            // Confirm that the doc type was not confidently predicted since the feature vector was all 0s
            Assert.AreEqual("DocumentType", firstAt.Name);
            Assert.AreEqual("Unknown", firstAt.Value.String);
        }

        // Tests using attributes stored in DB and using a feature generator ruleset for pagination classifier training
        [CLSCompliant(false)]
        [Test, Category("LearningMachine")]
        //        usage,                                 useASForExp, featureTypes,           testMissingData,          useFeatureRuleSet,     records, minTrainAcc, minTestAcc
        [TestCase(LearningMachineUsage.DocumentCategorization,  true, Features.SpatialString, MissingData.None,         FeatureRule.None,      20,       1.0,         1.0, TestName = "DocumentCategorization: Not using feature VOAs")]
        [TestCase(LearningMachineUsage.DocumentCategorization, false, Features.SpatialString, MissingData.None,         FeatureRule.None,      20,       1.0,         1.0, TestName = "DocumentCategorization: Use folder structure for expected data")]
        [TestCase(LearningMachineUsage.DocumentCategorization,  true, Features.SpatialString, MissingData.SomeImages,   FeatureRule.None,      14,       1.0,         1.0, TestName = "DocumentCategorization: Missing some images, not using feature VOAs")]
        [TestCase(LearningMachineUsage.DocumentCategorization,  true, Features.SpatialString, MissingData.SomeUSSFiles, FeatureRule.None,      14,       1.0,         1.0, TestName = "DocumentCategorization: Missing some uss files, not using feature VOAs")]
        [TestCase(LearningMachineUsage.DocumentCategorization,  true, Features.SpatialString, MissingData.All,          FeatureRule.None,       0,      null,        null, TestName = "DocumentCategorization: Missing all data")]
        [TestCase(LearningMachineUsage.DocumentCategorization,  true, Features.Attribute,     MissingData.None,         FeatureRule.None,      20,       1.0,         1.0, TestName = "DocumentCategorization: Using only feature VOAs on disk")]
        [TestCase(LearningMachineUsage.DocumentCategorization,  true, Features.Attribute,     MissingData.SomeVOAFiles, FeatureRule.None,      14,       1.0,         1.0, TestName = "DocumentCategorization: Missing some VOAs, using only feature VOAs on disk")]
        [TestCase(LearningMachineUsage.DocumentCategorization,  true, Features.Both,          MissingData.SomeVOAFiles, FeatureRule.None,      14,       1.0,         1.0, TestName = "DocumentCategorization: Missing some VOAs, using feature VOAs on disk")]
        [TestCase(LearningMachineUsage.DocumentCategorization,  true, Features.Both,          MissingData.None,         FeatureRule.None,      20,       1.0,         1.0, TestName = "DocumentCategorization: Using feature VOAs on disk")]
        [TestCase(LearningMachineUsage.DocumentCategorization,  true, Features.Both,          MissingData.SomeImages,   FeatureRule.None,      14,       1.0,         1.0, TestName = "DocumentCategorization: Missing some images, using feature VOAs on disk")]
        [TestCase(LearningMachineUsage.DocumentCategorization,  true, Features.Both,          MissingData.SomeUSSFiles, FeatureRule.None,      14,       1.0,         1.0, TestName = "DocumentCategorization: Missing some uss files, using feature VOAs on disk")]
        [TestCase(LearningMachineUsage.DocumentCategorization,  true, Features.Both,          MissingData.SomeVOAFiles, FeatureRule.IfMissing, 20,       1.0,         1.0, TestName = "DocumentCategorization: Run ruleset if files are missing")]
        [TestCase(LearningMachineUsage.DocumentCategorization,  true, Features.Both,          MissingData.AllVOAFiles,  FeatureRule.Always,    20,       1.0,         1.0, TestName = "DocumentCategorization: Run ruleset for all files")]

        [TestCase(LearningMachineUsage.Pagination,              true, Features.SpatialString, MissingData.None,         FeatureRule.None,      15,       1.0,         0.0, TestName = "Pagination: Not using feature VOAs")]
        [TestCase(LearningMachineUsage.Pagination,             false, Features.SpatialString, MissingData.None,         FeatureRule.None,      15,       1.0,         0.0, TestName = "Pagination: Use eav files")]
        [TestCase(LearningMachineUsage.Pagination,             false, Features.SpatialString, MissingData.SomeEAVFiles, FeatureRule.None,       9,       1.0,         0.0, TestName = "Pagination: Missing some eav files")]
        [TestCase(LearningMachineUsage.Pagination,             false, Features.SpatialString, MissingData.AllEAVFiles,  FeatureRule.None,       0,      null,        null, TestName = "Pagination: Missing all eav files")]
        [TestCase(LearningMachineUsage.Pagination,              true, Features.SpatialString, MissingData.SomeImages,   FeatureRule.None,       9,       1.0,         1.0, TestName = "Pagination: Missing some images, not using feature VOAs")]
        [TestCase(LearningMachineUsage.Pagination,              true, Features.SpatialString, MissingData.SomeUSSFiles, FeatureRule.None,       9,       1.0,         1.0, TestName = "Pagination: Missing uss files, not using feature VOAs")]
        [TestCase(LearningMachineUsage.Pagination,              true, Features.SpatialString, MissingData.All,          FeatureRule.None,       0,      null,        null, TestName = "Pagination: Missing all data")]
        [TestCase(LearningMachineUsage.Pagination,              true, Features.Both,          MissingData.None,         FeatureRule.None,      15,      0.90,         0.0, TestName = "Pagination: Using both spatial string and feature VOAs on disk")]
        [TestCase(LearningMachineUsage.Pagination,              true, Features.Attribute,     MissingData.SomeImages,   FeatureRule.None,       9,       1.0,         1.0, TestName = "Pagination: Missing some images, using feature VOAs on disk")]
        [TestCase(LearningMachineUsage.Pagination,              true, Features.Attribute,     MissingData.None,         FeatureRule.None,      15,       1.0,         0.0, TestName = "Pagination: Using feature VOAs on disk")]
        [TestCase(LearningMachineUsage.Pagination,              true, Features.Attribute,     MissingData.SomeVOAFiles, FeatureRule.IfMissing, 15,      0.92,         0.0, TestName = "Pagination: Run ruleset if files are missing")]
        [TestCase(LearningMachineUsage.Pagination,              true, Features.Attribute,     MissingData.AllVOAFiles,  FeatureRule.Always,    15,      0.85,         0.0, TestName = "Pagination: Run ruleset for all files")]

        [TestCase(LearningMachineUsage.AttributeCategorization, true, Features.SpatialString, MissingData.AllEAVFiles,  FeatureRule.None,      134,      0.0,         0.0, TestName = "AttributeCategorization: Not using attribute features")]
        [TestCase(LearningMachineUsage.AttributeCategorization, true, Features.Attribute,     MissingData.AllEAVFiles,  FeatureRule.None,      134,      1.0,        0.85, TestName = "AttributeCategorization: Using feature VOAs on disk")]
        [TestCase(LearningMachineUsage.AttributeCategorization, true, Features.Both,          MissingData.AllEAVFiles,  FeatureRule.None,      134,      1.0,        0.66, TestName = "AttributeCategorization: Using spatial string features and feature VOAs on disk")]
        [TestCase(LearningMachineUsage.AttributeCategorization, true, Features.Attribute,     MissingData.SomeImages,   FeatureRule.None,       87,      1.0,         1.0, TestName = "AttributeCategorization: Missing some images, using feature VOAs on disk")]
        [TestCase(LearningMachineUsage.AttributeCategorization, true, Features.Attribute,     MissingData.SomeUSSFiles, FeatureRule.None,       87,      1.0,         1.0, TestName = "AttributeCategorization: Missing some uss files, using feature VOAs on disk")]
        [TestCase(LearningMachineUsage.AttributeCategorization, true, Features.Attribute,     MissingData.VOA_EAVFiles, FeatureRule.None,       87,      1.0,         1.0, TestName = "AttributeCategorization: Missing some VOA files, using feature VOAs on disk")]
        [TestCase(LearningMachineUsage.AttributeCategorization, true, Features.Attribute,     MissingData.VOA_EAVFiles, FeatureRule.IfMissing, 134,     0.71,         0.5, TestName = "AttributeCategorization: Run ruleset if files are missing")]
        [TestCase(LearningMachineUsage.AttributeCategorization, true, Features.Attribute,     MissingData.AllV_EFiles,  FeatureRule.Always,    134,     0.71,        0.66, TestName = "AttributeCategorization: Run ruleset for all files")]
        [TestCase(LearningMachineUsage.AttributeCategorization, false, Features.Attribute,    MissingData.SomeVOAFiles, FeatureRule.IfMissing, 134,     0.71,         0.5, TestName = "AttributeCategorization: Don't use attribute set for source of labels")]
        [TestCase(LearningMachineUsage.AttributeCategorization, false, Features.Attribute,    MissingData.VOA_EAVFiles, FeatureRule.IfMissing,  87,      1.0,         1.0, TestName = "AttributeCategorization: Missing some source of labels files")]
        [TestCase(LearningMachineUsage.AttributeCategorization, false, Features.Attribute,    MissingData.AllV_EFiles,  FeatureRule.IfMissing,   0,     null,        null, TestName = "AttributeCategorization: Missing all source of labels files")]
        public static void DBAccess(
            LearningMachineUsage usage,
            bool useASForExp,
            Features featureTypes,
            MissingData testMissingData,
            FeatureRule useFeatureRuleSet,
            int records,
            double? minTrainAcc,
            double? minTestAcc)
        {
            var dbName = "_LMDBAccess_64B05ED2-DD5C-419D-80B0-3BD6E8094F24";
            try
            {
                string featureVoaSuffix = ".voa";
                string eavSuffix = ".voa";
                string sourceOfLabelsSuffix = ".voa";
                string answerPattern = null;
                string featureFilter = "DocumentType";
                string pageRange = null;
                if (usage == LearningMachineUsage.Pagination)
                {
                    SetPaginationFiles();
                    featureVoaSuffix = ".protofeatures.voa";
                    eavSuffix = ".eav";
                    answerPattern = "<SourceDocName>.eav";
                    featureFilter = "*@Feature";
                    pageRange = "3";
                }
                else if (usage == LearningMachineUsage.DocumentCategorization)
                {
                    SetDocumentCategorizationFiles();
                    answerPattern = "$FileOf($DirOf(<SourceDocName>))";
                }
                else if (usage == LearningMachineUsage.AttributeCategorization)
                {
                    SetPaginationFiles();
                    featureVoaSuffix = ".labeled.voa";
                    featureFilter = (featureTypes & Features.Attribute) == Features.Attribute ? "*@Feature" : "*@NO_FEATURES";
                }

                string rulesFile = useFeatureRuleSet == FeatureRule.None
                    ? null
                    : rulesFile = GetTrainingDataRulesFile(usage);

                // Create DB
                var eavAttributeSetName = "EVOA";
                CreateDB(dbName, eavAttributeSetName, eavSuffix);

                // Create LM
                var inputConfig = new InputConfiguration
                {
                    InputPath = _inputFolder.Last(),
                    InputPathType = InputType.Folder,
                    AttributesPath = (featureTypes & Features.Attribute) == Features.Attribute || usage == LearningMachineUsage.AttributeCategorization ? ("<SourceDocName>" + featureVoaSuffix) : "",
                    AnswerPath = answerPattern,
                    TrainingSetPercentage = 80
                };
                var autoBoW = (featureTypes & Features.SpatialString) == Features.SpatialString ? new SpatialStringFeatureVectorizer(pageRange, 5, 10000) { UseFeatureHashing = true } : null;
                var lm = new LearningMachine
                {
                    InputConfig = inputConfig,
                    Encoder = new LearningMachineDataEncoder(usage, autoBoW, featureFilter),
                    Classifier = new MulticlassSupportVectorMachineClassifier()
                };
                if (usage == LearningMachineUsage.AttributeCategorization)
                {
                    lm.Encoder.AttributesToTokenizeFilter = "*@Tokenize";
                    lm.InputConfig.AnswerPath = "<SourceDocName>.NO_EAV";
                    lm.LabelAttributesSettings = new LabelAttributes()
                    {
                        SourceOfLabelsPath = "<SourceDocName>" + sourceOfLabelsSuffix,
                        CreateEmptyLabelForNonMatching = true
                    };
                    lm.LabelAttributesSettings.CategoryQueryPairs.Add(new CategoryQueryPair
                    {
                        Category = "DOB",
                        CategoryIsXPath = false,
                        Query = "/*/Document/DocumentData/PatientInfo/DOB"
                    });
                }
                lm.ComputeEncodings();

                var files = Directory.GetFiles(_inputFolder.Last(), "*.tif", SearchOption.AllDirectories);

                if (useASForExp)
                {
                    // Delete eav files since encodings are computed, unless they are being used for features
                    if ((featureTypes & Features.Attribute) == 0
                        || eavSuffix != featureVoaSuffix
                        || useFeatureRuleSet == FeatureRule.Always)
                    {
                        foreach (var imageName in files)
                        {
                            File.Delete(imageName + eavSuffix);
                        }
                    }
                }

                // If testing missing data then delete some uss and feature voa files
                if (testMissingData != MissingData.None)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        if ((testMissingData & MissingData.AllOfSpecified) == MissingData.AllOfSpecified || i % 4 == 0)
                        {
                            var file = files[i];
                            if ((testMissingData & MissingData.SomeImages) == MissingData.SomeImages)
                            {
                                File.Delete(file);
                            }
                            if ((testMissingData & MissingData.SomeUSSFiles) == MissingData.SomeUSSFiles)
                            {
                                File.Delete(file + ".uss");
                            }
                            if ((testMissingData & MissingData.SomeVOAFiles) == MissingData.SomeVOAFiles)
                            {
                                File.Delete(file + featureVoaSuffix);

                            }
                            if ((testMissingData & MissingData.SomeEAVFiles) == MissingData.SomeEAVFiles)
                            {
                                // EAV files are ignored by attribute categorization usage so delete source of labels files
                                if (usage == LearningMachineUsage.AttributeCategorization)
                                {
                                    File.Delete(file + sourceOfLabelsSuffix);
                                }
                                else
                                {
                                    File.Delete(file + eavSuffix);
                                }
                            }
                        }
                    }
                }

                var (training, testing) = lm.GetDataToWriteToDatabase(CancellationToken.None,
                    "(local)", dbName, eavAttributeSetName,
                    lowestIDToProcess: 0,
                    highestIDToProcess: files.Length,
                    useAttributeSetForExpected: useASForExp,
                    runRuleSetForFeatures: useFeatureRuleSet == FeatureRule.Always,
                    runRuleSetIfFeaturesAreMissing: useFeatureRuleSet == FeatureRule.IfMissing,
                    featureRuleSetName: rulesFile);

                Assert.AreEqual(records, training.Count + testing.Count);

                if (records > 0)
                {
                    TemporaryFile trainingDataFile = new TemporaryFile(false);
                    TemporaryFile testingDataFile = new TemporaryFile(false);

                    File.WriteAllLines(trainingDataFile.FileName, training.Select(record =>
                        string.Join(",", record.Select(s => s.QuoteIfNeeded("\"", ",")))));
                    File.WriteAllLines(testingDataFile.FileName, testing.Select(record =>
                        string.Join(",", record.Select(s => s.QuoteIfNeeded("\"", ",")))));

                    var (trainingSet, testingSet) = lm.TrainAndTestWithCsvData(false, trainingDataFile.FileName, testingDataFile.FileName, false,
                        _ => { }, CancellationToken.None);

                    var trainAcc = trainingSet.Match(gcm => gcm.OverallAgreement, cm => cm.FScore);
                    var testAcc = testingSet.Match(gcm => gcm.OverallAgreement, cm => cm.FScore);

                    // Treat NaN as zero
                    testAcc = double.IsNaN(testAcc) ? 0.0 : testAcc;

                    Assert.GreaterOrEqual(trainAcc, minTrainAcc);
                    Assert.GreaterOrEqual(testAcc, minTestAcc);

                    var classes = lm.Encoder.AnswerCodeToName;
                    if (usage == LearningMachineUsage.Pagination)
                    {
                        CollectionAssert.AreEqual(new[] { "NotFirstPage", "FirstPage" }, classes);
                    }
                    else if (usage == LearningMachineUsage.AttributeCategorization)
                    {
                        CollectionAssert.AreEqual(new[] { "", "DOB" }, classes);
                    }
                    else if (usage == LearningMachineUsage.DocumentCategorization)
                    {
                        // If not expecting files to be skipped then check for all classes to exist
                        var folders = files.Select(p => Path.GetFileName(Path.GetDirectoryName(p))).ToList();
                        if (testMissingData == MissingData.None
                            || (testMissingData & MissingData.SomeVOAFiles) == MissingData.SomeVOAFiles
                                && useFeatureRuleSet != FeatureRule.None)
                        {
                            Assert.AreEqual(folders.Count, folders.Intersect(classes).Count());
                        }
                        else
                        {
                            Assert.Greater(folders.Count, classes.Count());
                        }

                        // "Unknown" class has no folder
                        Assert.AreEqual(1, classes.Except(folders).Count());
                    }
                }
            }
            finally
            {
                _testDbManager.RemoveDatabase(dbName);

                // Reset test files object to avoid complaints about deleted files
                _testFiles.Dispose();
                _testFiles = new TestFileManager<TestLearningMachine>();
            }
        }

        #endregion Tests

        #region Helper Methods

        // Helper method to build folder structure for pagination testing
        // These images are stapled together from Demo_LabDE images
        private static void SetPaginationFiles(bool withCoverPages = false)
        {
            _inputFolder.Add(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
            Directory.CreateDirectory(_inputFolder.Last());

            for (int i = 0; i < 7; i++)
            {
                var baseName = withCoverPages
                    ? "Resources.LearningMachine.PaginationWithCoverPages.Pagination_{0:D3}.tif{1}"
                    : "Resources.LearningMachine.Pagination.Pagination_{0:D3}.tif{1}";

                string resourceName = string.Format(CultureInfo.CurrentCulture, baseName, i+1, "");
                string path = Path.Combine(_inputFolder.Last(), resourceName);
                _testFiles.GetFile(resourceName, path);

                resourceName = string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".uss");
                path = Path.Combine(_inputFolder.Last(), resourceName);
                _testFiles.GetFile(resourceName, path);

                resourceName = string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".protofeatures.voa");
                path = Path.Combine(_inputFolder.Last(), resourceName);
                _testFiles.GetFile(resourceName, path);

                resourceName = string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".eav");
                path = Path.Combine(_inputFolder.Last(), resourceName);
                _testFiles.GetFile(resourceName, path);

                if (!withCoverPages)
                {
                    resourceName = string.Format(CultureInfo.CurrentCulture, baseName, i + 1, ".voa");
                    path = Path.Combine(_inputFolder.Last(), resourceName);
                    _testFiles.GetFile(resourceName, path);

                    resourceName = string.Format(CultureInfo.CurrentCulture, baseName, i + 1, ".candidates.voa");
                    path = Path.Combine(_inputFolder.Last(), resourceName);
                    _testFiles.GetFile(resourceName, path);

                    resourceName = string.Format(CultureInfo.CurrentCulture, baseName, i + 1, ".labeled.voa");
                    path = Path.Combine(_inputFolder.Last(), resourceName);
                    _testFiles.GetFile(resourceName, path);

                    resourceName = string.Format(CultureInfo.CurrentCulture, baseName, i + 1, ".labeled_2_types.voa");
                    path = Path.Combine(_inputFolder.Last(), resourceName);
                    _testFiles.GetFile(resourceName, path);

                    resourceName = string.Format(CultureInfo.CurrentCulture, baseName, i + 1, ".labeled_3_types.voa");
                    path = Path.Combine(_inputFolder.Last(), resourceName);
                    _testFiles.GetFile(resourceName, path);
                }
            }
        }

        // Helper method to build folder structure for bitmap feature testing
        // These images and voas are mocked-up forms from Exact Sciences
        private static void SetBitmapFiles()
        {
            _inputFolder.Add(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
            Directory.CreateDirectory(_inputFolder.Last());

            for (int i = 1; i <= 30; i++)
            {
                var baseName = "Resources.LearningMachine.Bitmap.{0:D3}.tif{1}";

                string resourceName = string.Format(CultureInfo.CurrentCulture, baseName, i, "");
                string path = Path.Combine(_inputFolder.Last(), resourceName);
                _testFiles.GetFile(resourceName, path);

                resourceName = string.Format(CultureInfo.CurrentCulture, baseName, i, ".uss");
                path = Path.Combine(_inputFolder.Last(), resourceName);
                _testFiles.GetFile(resourceName, path);

                resourceName = string.Format(CultureInfo.CurrentCulture, baseName, i, ".evoa");
                path = Path.Combine(_inputFolder.Last(), resourceName);
                _testFiles.GetFile(resourceName, path);
            }
        }

        // Helper method to create file lists for document categorization testing
        // These files are from Demo_FlexIndex
        private static void SetDocumentCategorizationFiles()
        {
            _inputFolder.Add(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
            Directory.CreateDirectory(_inputFolder.Last());

            _categories = new string[]
            {
                "Deed of Trust",
                "Mortgage",
                "Satisfaction of Mortgage",
                "Reconveyance",
                "Grant Deed",
                "Warranty Deed",
                "Quit Claim Deed",
                "Assignment of Deed of Trust",
                "Assignment of Mortgage",
                "Notice of Federal Tax Lien"
            };

            foreach(var category in _categories)
            {
                string folder = Path.Combine(_inputFolder.Last(), category);
                Directory.CreateDirectory(folder);
            }

            for (int i = 0; i < _categories.Length; i++)
            {
                var baseName = "Resources.LearningMachine.DocumentCategorization.Example{0:D2}.tif{1}";
                string resourceName = string.Format(CultureInfo.CurrentCulture, baseName, i+1, "");
                string path = Path.Combine(_inputFolder.Last(), _categories[i], resourceName);
                _testFiles.GetFile(resourceName, path);

                resourceName = string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".uss");
                path = Path.Combine(_inputFolder.Last(), _categories[i], resourceName);
                _testFiles.GetFile(resourceName, path);

                resourceName = string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".voa");
                path = Path.Combine(_inputFolder.Last(), _categories[i], resourceName);
                _testFiles.GetFile(resourceName, path);
            }
        }

        private static List<(string, string, double)> GetPredictionsFromCsv(string path)
        {
            var result = new List<(string, string, double)>();
            using (var csvReader = new Microsoft.VisualBasic.FileIO.TextFieldParser(path))
            {
                csvReader.Delimiters = new[] { "," };

                // Check for header row
                if (!csvReader.EndOfData)
                {
                    var fields = csvReader.ReadFields();
                    
                    int ussPathIndex = Array.IndexOf(fields, "Path");
                    int predictionIndex = Array.IndexOf(fields, "Prediction");
                    int probabilityIndex = Array.IndexOf(fields, "Probability");
                    while (!csvReader.EndOfData && (fields = csvReader.ReadFields()) != null)
                    {
                        result.Add((fields[ussPathIndex], fields[predictionIndex], double.Parse(fields[probabilityIndex],
                            CultureInfo.InvariantCulture)));
                    }
                }
            }

            return result;
        }

        private static string GetTrainingDataRulesFile(LearningMachineUsage usage)
        {
            string rulesFile = null;
            if (usage == LearningMachineUsage.Pagination)
            {
                rulesFile = Path.Combine(_inputFolder.Last(), "createProtofeatures.rsd");
                _testFiles.GetFile("Resources.LearningMachine.PaginationFeatureGenerator.createProtofeatures.rsd", rulesFile);
                _testFiles.GetFile("Resources.LearningMachine.PaginationFeatureGenerator.createProtofeatures.rsd.etf", rulesFile + ".etf");

                rulesFile = Path.Combine(_inputFolder.Last(), "getPageNumber.rsd");
                _testFiles.GetFile("Resources.LearningMachine.PaginationFeatureGenerator.getPageNumber.rsd", rulesFile);
                _testFiles.GetFile("Resources.LearningMachine.PaginationFeatureGenerator.getPageNumber.rsd.etf", rulesFile + ".etf");

                rulesFile = Path.Combine(_inputFolder.Last(), "CreateTrainingData.rsd");
                _testFiles.GetFile("Resources.LearningMachine.PaginationFeatureGenerator.CreateTrainingData.rsd", rulesFile);
            }
            else if (usage == LearningMachineUsage.DocumentCategorization)
            {
                rulesFile = Path.Combine(_inputFolder.Last(), "CreateTrainingData.rsd");
                _testFiles.GetFile("Resources.LearningMachine.DocClassifierFeatureGenerator.CreateTrainingData.rsd", rulesFile);
            }
            else if (usage == LearningMachineUsage.AttributeCategorization)
            {
                rulesFile = Path.Combine(_inputFolder.Last(), "date.dat");
                _testFiles.GetFile("Resources.LearningMachine.CandidateAttributeGenerator.date.dat", rulesFile);

                rulesFile = Path.Combine(_inputFolder.Last(), "CreateTrainingData.rsd");
                _testFiles.GetFile("Resources.LearningMachine.CandidateAttributeGenerator.CreateTrainingData.rsd", rulesFile);
            }
            return rulesFile;
        }

        private static void CreateDB(string dbName, string eavAttributeSetName, string eavSuffix)
        {
            var modelName = "M";
            var fileProcessingDB = _testDbManager.GetNewDatabase(dbName);
            fileProcessingDB.DefineNewAction("a");
            fileProcessingDB.DefineNewMLModel(modelName);
            var attributeDBMgr = new AttributeDBMgr
            {
                FAMDB = fileProcessingDB
            };
            attributeDBMgr.CreateNewAttributeSetName(eavAttributeSetName);
            var afutility = new AFUtility();
            var files = Directory.GetFiles(_inputFolder.Last(), "*.tif", SearchOption.AllDirectories);
            fileProcessingDB.RecordFAMSessionStart("DUMMY", "a", true, true);
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                var rec = fileProcessingDB.AddFile(file, "a", -1, EFilePriority.kPriorityNormal,
                    false, false, EActionStatus.kActionPending, false, out _, out _);

                var path = file + eavSuffix;
                var voaData = afutility.GetAttributesFromFile(path);
                int fileTaskSessionID = fileProcessingDB.StartFileTaskSession(_STORE_ATTRIBUTE_GUID, rec.FileID, rec.ActionID);
                attributeDBMgr.CreateNewAttributeSetForFile(fileTaskSessionID, eavAttributeSetName, voaData, false, true, true,
                    closeConnection: i == (files.Length - 1));

                fileProcessingDB.UpdateFileTaskSession(fileTaskSessionID, 0, 0, 0);
            }
            fileProcessingDB.CloseAllDBConnections();
        }

        #endregion Helper Methods
    }
}
