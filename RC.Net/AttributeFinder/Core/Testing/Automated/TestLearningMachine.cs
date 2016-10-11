using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using UCLID_RASTERANDOCRMGMTLib;
using UCLID_COMUTILSLib;
using UCLID_AFUTILSLib;
using UCLID_AFCORELib;
using ComAttribute = UCLID_AFCORELib.Attribute;

namespace Extract.AttributeFinder.Test
{
    /// <summary>
    /// Unit tests for learning machine data class
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("LearningMachine")]
    public class TestLearningMachine
    {
        #region Constants

        /// <summary>
        /// The name of an embedded resource folder
        /// </summary>

        #endregion Constants

        #region Fields

        /// <summary>
        /// Manages the test images needed for testing.
        /// </summary>
        static TestFileManager<TestLearningMachine> _testFiles;
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
                    LearningMachine.GetIndexesOfSubsetsByCategory(categories, subset1Fraction, out subset1Indexes, out subset2Indexes, new Random(seed));

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
                    LearningMachine.GetIndexesOfSubsetsByCategory(categories, subset1Fraction, out subset1Indexes, out subset2Indexes);

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
            Assert.Greater(results.Item1.Match(gcm => gcm.OverallAgreement, cm => cm.Accuracy), 0.99);
            Assert.Greater(results.Item2.Match(gcm => gcm.OverallAgreement, cm => cm.Accuracy), 0.99);

            // Test output
            string[] ussFiles, voaFiles, answers;
            inputConfig.GetInputData(out ussFiles, out voaFiles, out answers);
            for (int i = 0; i < ussFiles.Length; i++)
            {
                var uss = new SpatialStringClass();
                uss.LoadFrom(ussFiles[i], false);
                Assert.AreEqual(answers[i], ((ComAttribute)lm.ComputeAnswer(uss, null, false).At(0)).Value.String);
            }

            // Test preserving input (no input)
            inputConfig.GetInputData(out ussFiles, out voaFiles, out answers);
            for (int i = 0; i < ussFiles.Length; i++)
            {
                var uss = new SpatialStringClass();
                uss.LoadFrom(ussFiles[i], false);
                Assert.AreEqual(answers[i], ((ComAttribute)lm.ComputeAnswer(uss, null, true).At(0)).Value.String);
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
                Assert.Less(voa.Size(), lm.ComputeAnswer(uss, voa, true).Size());
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
                Assert.AreEqual(1, lm.ComputeAnswer(uss, voa, false).Size());
            }
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
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.Pagination),
                Classifier = new NeuralNetworkClassifier { UseCrossValidationSets = false },
                RandomNumberSeed = 10
            };
            var results = lm.TrainMachine();
            Assert.Greater(results.Item1.Match(_ => Double.NaN, cm => cm.FScore), 0.90);
            Assert.Greater(results.Item2.Match(_ => Double.NaN, cm => cm.FScore), 0.6);
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
                    InputPathType = InputType.TextFileOrCsv,
                    AttributesPath = "<SourceDocName>.protofeatures.voa",
                    AnswerPath = "<SourceDocName>.eav",
                    TrainingSetPercentage = 50
                },
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.Pagination),
                Classifier = new NeuralNetworkClassifier { UseCrossValidationSets = false },
                RandomNumberSeed = 10
            };
            var results = lm.TrainMachine();
            Assert.Greater(results.Item1.Match(_ => Double.NaN, cm => cm.FScore), 0.90);
            Assert.Greater(results.Item2.Match(_ => Double.NaN, cm => cm.FScore), 0.6);
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
                    InputPathType = InputType.TextFileOrCsv,
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
            Assert.Greater(results.Item1.Match(gcm => gcm.OverallAgreement, _ => Double.NaN), 0.87);

            // Test results are between 70% and 80% when there are 'other' dates (neither DOB nor CollectionDate)
            Assert.Greater(results.Item2.Match(gcm => gcm.OverallAgreement, _ => Double.NaN), 0.7);
            Assert.Less(results.Item2.Match(gcm => gcm.OverallAgreement, _ => Double.NaN), 0.8);
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
                    InputPathType = InputType.TextFileOrCsv,
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
                    InputPathType = InputType.TextFileOrCsv,
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
                var fakeExpectedData = lm.ComputeAnswer(uss, voa, true);
                computedAttributesPreservedInput.AddRange(fakeExpectedData.ToIEnumerable<ComAttribute>());
                fakeExpectedData = lm.ComputeAnswer(uss, voa, false);
                computedAttributesNoPreservedInput.AddRange(fakeExpectedData.ToIEnumerable<ComAttribute>());
                var fileName = Path.ChangeExtension(eavFiles[i], "fake.voa");
                fakeExpectedData.SaveTo(fileName, false, typeof(AttributeStorageManagerClass).GUID.ToString("B"));
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

            inputConfig1.InputPathType = InputType.TextFileOrCsv;
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
                Assert.AreEqual(answers[i], ((ComAttribute)lm2.ComputeAnswer(uss, null, false).At(0)).Value.String);
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
                Assert.AreEqual(answers[i], ((ComAttribute)lm2.ComputeAnswer(uss, null, false).At(0)).Value.String);
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
                    InputPathType = InputType.TextFileOrCsv,
                    AttributesPath = "<SourceDocName>.protofeatures.voa",
                    AnswerPath = "<SourceDocName>.eav",
                    TrainingSetPercentage = 50
                },
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.Pagination),
                Classifier = new NeuralNetworkClassifier { UseCrossValidationSets = false },
                RandomNumberSeed = 10
            };
            lm1.TrainMachine();
            lm1.Save(_savedMachinePath);
            var lm2 = LearningMachine.Load(_savedMachinePath);

            Assert.That(lm1.IsConfigurationEqualTo(lm2));

            // Test output
            var results = lm2.TestMachine();
            Assert.Greater(results.Item1.Match(_ => Double.NaN, cm => cm.FScore), 0.90);
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
                var labeledAsDOB = xPathContext.FindAllOfType<ComAttribute>("/*/*[AttributeType = 'DOB']");
                var labeledAsCollectionDate = xPathContext.FindAllOfType<ComAttribute>("/*/*[AttributeType = 'CollectionDate']");
                Assert.GreaterOrEqual(labeledAsDOB.Count(), 1);
                Assert.GreaterOrEqual(labeledAsCollectionDate.Count(), labeledAsDOB.Count());
                Assert.Greater(labeledAttributes.Size(), labeledAsDOB.Count() + labeledAsCollectionDate.Count());
            }

            var specificFile = Directory.GetFiles(_inputFolder.Last(), "*.nunit_labeled2.voa").First();
            var attributes = afutil.GetAttributesFromFile(specificFile).ToIEnumerable<ComAttribute>();
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

        #endregion Tests

        #region Helper Methods

        // Helper method to build folder structure for pagination testing
        // These images are stapled together from Demo_LabDE images
        private static void SetPaginationFiles()
        {
            _inputFolder.Add(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
            Directory.CreateDirectory(_inputFolder.Last());

            for (int i = 0; i < 7; i++)
            {
                var baseName = "Resources.LearningMachine.Pagination.Pagination_{0:D3}.tif{1}";

                string resourceName = string.Format(CultureInfo.CurrentCulture, baseName, i+1, "");
                string path = Path.Combine(_inputFolder.Last(), resourceName);
                _testFiles.GetFile(resourceName, path);

                resourceName = string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".uss");
                path = Path.Combine(_inputFolder.Last(), resourceName);
                _testFiles.GetFile(resourceName, path);

                resourceName = string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".protofeatures.voa");
                path = Path.Combine(_inputFolder.Last(), resourceName);
                _testFiles.GetFile(resourceName, path);

                resourceName = string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".voa");
                path = Path.Combine(_inputFolder.Last(), resourceName);
                _testFiles.GetFile(resourceName, path);

                resourceName = string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".candidates.voa");
                path = Path.Combine(_inputFolder.Last(), resourceName);
                _testFiles.GetFile(resourceName, path);

                resourceName = string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".labeled.voa");
                path = Path.Combine(_inputFolder.Last(), resourceName);
                _testFiles.GetFile(resourceName, path);

                resourceName = string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".labeled_2_types.voa");
                path = Path.Combine(_inputFolder.Last(), resourceName);
                _testFiles.GetFile(resourceName, path);

                resourceName = string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".labeled_3_types.voa");
                path = Path.Combine(_inputFolder.Last(), resourceName);
                _testFiles.GetFile(resourceName, path);

                resourceName = string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".eav");
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

        #endregion Helper Methods
    }
}
