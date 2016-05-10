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
        static string _inputFolder;
        static string _csvPath;
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
            if (Directory.Exists(_inputFolder))
            {
                Directory.Delete(_inputFolder, true);
            }

            // Delete tmp csv
            if (File.Exists(_csvPath))
            {
                File.Delete(_csvPath);
            }
        }

        #endregion Overhead

        #region Tests

        // Test GetIndexesOfSubsetsByCategory to make sure it is randomly selecting appropriately sized subsets
        [Test, Category("LearningMachine")]
        public static void GetIndexesOfSubsetsByCategoryOneCategory()
        {
            foreach (int size in new[] { 1, 1000})
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
            foreach (int size in new[] { 1, 1000})
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
            var lm = new LearningMachine();
            var inputConfig = new InputConfiguration
              {
                  InputPath = _inputFolder,
                  InputPathType = InputType.Folder,
                  AttributesPath = "",
                  AnswerPath = "$FileOf($DirOf(<SourceDocName>))",
                  TrainingSetPercentage = 80
              };
            var results = lm.TrainMachine(inputConfig);
            Assert.Greater(results.Item1, 0.99);
            Assert.Greater(results.Item2, 0.99);

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
                Classifier = new NeuralNetworkClassifier { UseCrossValidationSets = false},
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.Pagination)
            };
            var results = lm.TrainMachine(new InputConfiguration
              {
                  InputPath = _inputFolder,
                  InputPathType = InputType.Folder,
                  AttributesPath = "<SourceDocName>.protofeatures.voa",
                  AnswerPath = "<SourceDocName>.eav",
                  TrainingSetPercentage = 50
              });
            Assert.Greater(results.Item1, 0.90);
            Assert.Greater(results.Item2, 0.6);
        }

        [Test, Category("LearningMachine")]
        public static void TrainMachinePaginationFromList()
        {
            SetPaginationFiles();
            string[] listContents = Directory.GetFiles(_inputFolder, "*.tif", SearchOption.AllDirectories);
            _csvPath = Path.GetTempFileName();
            File.WriteAllLines(_csvPath, listContents);

            var lm = new LearningMachine
            {
                Classifier = new NeuralNetworkClassifier { UseCrossValidationSets = false},
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.Pagination)
            };
            var results = lm.TrainMachine(new InputConfiguration
              {
                  InputPath = _csvPath,
                  InputPathType = InputType.TextFileOrCsv,
                  AttributesPath = "<SourceDocName>.protofeatures.voa",
                  AnswerPath = "<SourceDocName>.eav",
                  TrainingSetPercentage = 50
              });
            Assert.Greater(results.Item1, 0.90);
            Assert.Greater(results.Item2, 0.6);
        }

        [Test, Category("LearningMachine")]
        public static void TrainMachineFromCsv()
        {
            SetDocumentCategorizationFiles();
            string[] csvContents = Directory.GetFiles(_inputFolder, "*.tif", SearchOption.AllDirectories)
                .Select(imagePath => string.Join(",", imagePath, Path.GetFileName(Path.GetDirectoryName(imagePath)))).ToArray();
            _csvPath = Path.GetTempFileName();
            File.WriteAllLines(_csvPath, csvContents);

            var lm = new LearningMachine();
            var results = lm.TrainMachine(new InputConfiguration
              {
                  InputPath = _csvPath,
                  InputPathType = InputType.TextFileOrCsv,
                  AttributesPath = "",
                  AnswerPath = "",
                  TrainingSetPercentage = 80
              });
            Assert.Greater(results.Item1, 0.99);
            Assert.Greater(results.Item2, 0.99);
        }

        [Test, Category("LearningMachine")]
        public static void TrainMachineFromCsvWithHeader()
        {
            SetDocumentCategorizationFiles();
            string[] csvContents = Directory.GetFiles(_inputFolder, "*.tif", SearchOption.AllDirectories)
                .Select(imagePath => string.Join(",", imagePath, Path.GetFileName(Path.GetDirectoryName(imagePath)))).ToArray();
            csvContents = new[] { "Image Paths, <Category>" }.Concat(csvContents).ToArray();
            _csvPath = Path.GetTempFileName();
            File.WriteAllLines(_csvPath, csvContents);

            var lm = new LearningMachine();
            var results = lm.TrainMachine(new InputConfiguration
              {
                  InputPath = _csvPath,
                  InputPathType = InputType.TextFileOrCsv,
                  AttributesPath = "",
                  AnswerPath = "",
                  TrainingSetPercentage = 80
              });
            Assert.Greater(results.Item1, 0.99);
            Assert.Greater(results.Item2, 0.99);
        }

        [Test, Category("LearningMachine")]
        public static void TrainMachineFromCsvWithQuotes()
        {
            SetDocumentCategorizationFiles();
            string[][] csvTempContents = Directory.GetFiles(_inputFolder, "*.tif", SearchOption.AllDirectories)
                .Select(imagePath => new [] {imagePath, Path.GetFileName(Path.GetDirectoryName(imagePath))}).ToArray();

            csvTempContents[2][0] = "\"" + csvTempContents[2][0] + "\"";
            csvTempContents[3][1] = "\"Abstract of \"\"Support\"\" Judgment\"";

            string[] csvContents = csvTempContents.Select(x => string.Join(",", x)).ToArray();
            _csvPath = Path.GetTempFileName();
            File.WriteAllLines(_csvPath, csvContents);

            var lm = new LearningMachine();
            var results = lm.TrainMachine(new InputConfiguration
              {
                  InputPath = _csvPath,
                  InputPathType = InputType.TextFileOrCsv,
                  AttributesPath = "",
                  AnswerPath = "",
                  TrainingSetPercentage = 80
              });
            Assert.Greater(results.Item1, 0.99);
            Assert.Greater(results.Item2, 0.99);
        }

        // Test that the Document/Pages attributes are correctly created
        [Test, Category("LearningMachine")]
        public static void CreateExpectedPaginationValues()
        {
            SetPaginationFiles();
            var lm = new LearningMachine
            {
                Classifier = new NeuralNetworkClassifier { UseCrossValidationSets = false},
                Encoder = new LearningMachineDataEncoder(LearningMachineUsage.Pagination)
            };
            var inputConfig = new InputConfiguration
              {
                  InputPath = _inputFolder,
                  InputPathType = InputType.Folder,
                  AttributesPath = "<SourceDocName>.protofeatures.voa",
                  AnswerPath = "<SourceDocName>.eav",
                  TrainingSetPercentage = 50
              };
            lm.TrainMachine(inputConfig);

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
                var fileName = Path.ChangeExtension(eavFiles[i], "voa");
                fakeExpectedData.SaveTo(fileName, false, typeof(AttributeStorageManagerClass).GUID.ToString("B"));
                evoaFiles[i] = fileName;
            }
            var results = lm.Encoder.GetFeatureVectorAndAnswerCollections(ussFiles, voaFiles, evoaFiles);
            double [][] inputs = results.Item1;
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

        [Test, Category("Extended")]
        public static void ZTrainMachineFromLargeFolder()
        {
            var lm = new LearningMachine();
            var results = lm.TrainMachine(new InputConfiguration
              {
                  InputPath = @"K:\Common\Engineering\Sample Files\AtPac\CA - Amador\Set003\Images",
                  InputPathType = InputType.Folder,
                  AttributesPath = "",
                  AnswerPath = "$FileOf($DirOf(<SourceDocName>))",
                  TrainingSetPercentage = 80
              });
            Assert.Greater(results.Item1, 0.99);
            Assert.Greater(results.Item2, 0.93);
        }

        #endregion Tests

        #region Helper Methods

        // Helper method to build folder structure for pagination testing
        // These images are stapled together from Demo_LabDE images
        private static void SetPaginationFiles()
        {
            _inputFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_inputFolder);

            for (int i = 0; i < 7; i++)
            {
                var baseName = "Resources.LearningMachine.Pagination.Pagination_{0:D3}.tif{1}";

                string resourceName = string.Format(CultureInfo.CurrentCulture, baseName, i+1, "");
                string path = Path.Combine(_inputFolder, resourceName);
                _testFiles.GetFile(resourceName, path);

                resourceName = string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".uss");
                path = Path.Combine(_inputFolder, resourceName);
                _testFiles.GetFile(resourceName, path);

                resourceName = string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".protofeatures.voa");
                path = Path.Combine(_inputFolder, resourceName);
                _testFiles.GetFile(resourceName, path);

                resourceName = string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".eav");
                path = Path.Combine(_inputFolder, resourceName);
                _testFiles.GetFile(resourceName, path);
            }
        }


        // Helper method to create file lists for document categorization testing
        // These files are from Demo_FlexIndex
        private static void SetDocumentCategorizationFiles()
        {
            _inputFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_inputFolder);

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
                string folder = Path.Combine(_inputFolder, category);
                Directory.CreateDirectory(folder);
            }

            for (int i = 0; i < _categories.Length; i++)
            {
                var baseName = "Resources.LearningMachine.DocumentCategorization.Example{0:D2}.tif{1}";
                string resourceName = string.Format(CultureInfo.CurrentCulture, baseName, i+1, "");
                string path = Path.Combine(_inputFolder, _categories[i], resourceName);
                _testFiles.GetFile(resourceName, path);

                resourceName = string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".uss");
                path = Path.Combine(_inputFolder, _categories[i], resourceName);
                _testFiles.GetFile(resourceName, path);

                resourceName = string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".voa");
                path = Path.Combine(_inputFolder, _categories[i], resourceName);
                _testFiles.GetFile(resourceName, path);
            }
        }

        #endregion Helper Methods
    }
}
