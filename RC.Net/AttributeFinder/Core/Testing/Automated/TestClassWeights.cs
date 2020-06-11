using Extract.Testing.Utilities;
using LearningMachineTrainer;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Extract.AttributeFinder.Test
{
    /// <summary>
    /// Unit tests for implementations of machine learning class weighting
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("ClassWeights")]
    public class TestClassWeights
    {
        #region Fields

        /// <summary>
        /// Manages the test images needed for testing.
        /// </summary>
        static TestFileManager<TestClassWeights> _testFiles;

        #endregion Fields

        #region Overhead

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestClassWeights>();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [OneTimeTearDown]
        public static void FinalCleanup()
        {
            // Dispose of the test image manager
            if (_testFiles != null)
            {
                _testFiles.Dispose();
            }
        }

        #endregion Overhead

        #region Tests

        // Test one to one weight ratio for Multiclass SVM
        [Test, Category("ClassWeights")]
        public static void MulticlassOneToOneWeightRatio()
        {
            var (lm, train, test) = SetCsvFiles();
            lm.Classifier = new MulticlassSupportVectorMachineClassifier
            {
                AutomaticallyChooseComplexityValue = false,
                Complexity = 0.01,
                PositiveToNegativeWeightRatio = 1,
                TrainingAlgorithmCacheSize = 2000
            };

            var (_, testResult) = lm.TrainAndTestWithCsvData(false, train, test, false, _ => { }, CancellationToken.None);
            testResult.Match(_ => throw new ArgumentException("Not expected"),
                confusionMatrix => Assert.AreEqual(0.81, Math.Round(confusionMatrix.Recall, 2)));
        }

        // Test one tenth to one weight ratio for Multiclass SVM
        // This makes the recall on the test set go up
        [Test, Category("ClassWeights")]
        public static void MulticlassOneTenthToOneWeightRatio()
        {
            var (lm, train, test) = SetCsvFiles();
            lm.Classifier = new MulticlassSupportVectorMachineClassifier
            {
                AutomaticallyChooseComplexityValue = false,
                Complexity = 0.01,
                PositiveToNegativeWeightRatio = 0.1,
                ConditionallyApplyWeightRatio = true,
                TrainingAlgorithmCacheSize = 2000
            };

            var (_, testResult) = lm.TrainAndTestWithCsvData(false, train, test, false, _ => { }, CancellationToken.None);
            testResult.Match(_ => throw new ArgumentException("Not expected"),
                confusionMatrix => Assert.AreEqual(0.88, Math.Round(confusionMatrix.Recall, 2)));
        }

        // Test ten to one weight ratio for Multiclass SVM
        // This makes the recall on the test set go down
        [Test, Category("ClassWeights")]
        public static void MulticlassTenToOneWeightRatio()
        {
            var (lm, train, test) = SetCsvFiles();
            lm.Classifier = new MulticlassSupportVectorMachineClassifier
            {
                AutomaticallyChooseComplexityValue = false,
                Complexity = 0.01,
                PositiveToNegativeWeightRatio = 10,
                ConditionallyApplyWeightRatio = true,
                TrainingAlgorithmCacheSize = 2000
            };

            var (_, testResult) = lm.TrainAndTestWithCsvData(false, train, test, false, _ => { }, CancellationToken.None);
            testResult.Match(_ => throw new ArgumentException("Not expected"),
                confusionMatrix => Assert.AreEqual(0.73, Math.Round(confusionMatrix.Recall, 2)));
        }

        // Test one to one weight ratio for Multilabel SVM
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multilabel")]
        [Test, Category("ClassWeights")]
        public static void MultilabelOneToOneWeightRatio()
        {
            var (lm, train, test) = SetCsvFiles();
            lm.Classifier = new MultilabelSupportVectorMachineClassifier
            {
                AutomaticallyChooseComplexityValue = false,
                Complexity = 0.01,
                PositiveToNegativeWeightRatio = 1,
                TrainingAlgorithmCacheSize = 2000
            };

            var (_, testResult) = lm.TrainAndTestWithCsvData(false, train, test, false, _ => { }, CancellationToken.None);
            testResult.Match(_ => throw new ArgumentException("Not expected"),
                confusionMatrix => Assert.AreEqual(0.81, Math.Round(confusionMatrix.Recall, 2)));
        }

        // Test one tenth to one weight ratio for Multilabel SVM
        // This makes the recall on the test set go up
        [Test, Category("ClassWeights")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multilabel")]
        public static void MultilabelOneTenthToOneWeightRatio()
        {
            var (lm, train, test) = SetCsvFiles();
            lm.Classifier = new MultilabelSupportVectorMachineClassifier
            {
                AutomaticallyChooseComplexityValue = false,
                Complexity = 0.01,
                PositiveToNegativeWeightRatio = 0.1,
                ConditionallyApplyWeightRatio = true,
                TrainingAlgorithmCacheSize = 2000
            };

            var (_, testResult) = lm.TrainAndTestWithCsvData(false, train, test, false, _ => { }, CancellationToken.None);
            testResult.Match(_ => throw new ArgumentException("Not expected"),
                confusionMatrix => Assert.AreEqual(0.85, Math.Round(confusionMatrix.Recall, 2)));
        }

        // Test ten to one weight ratio for Multilabel SVM
        // This makes the recall on the test set go down
        [Test, Category("ClassWeights")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multilabel")]
        public static void MultilabelTenToOneWeightRatio()
        {
            var (lm, train, test) = SetCsvFiles();
            lm.Classifier = new MultilabelSupportVectorMachineClassifier
            {
                AutomaticallyChooseComplexityValue = false,
                Complexity = 0.01,
                PositiveToNegativeWeightRatio = 10,
                ConditionallyApplyWeightRatio = true,
                TrainingAlgorithmCacheSize = 2000
            };

            var (_, testResult) = lm.TrainAndTestWithCsvData(false, train, test, false, _ => { }, CancellationToken.None);
            testResult.Match(_ => throw new ArgumentException("Not expected"),
                confusionMatrix => Assert.AreEqual(0.78, Math.Round(confusionMatrix.Recall, 2)));
        }

        // Test one to one weight ratio for Neural Net
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "NeuralNet")]
        [Test, Category("ClassWeights")]
        public static void NeuralNetOneToOneWeightRatio()
        {
            var (lm, train, test) = SetCsvFiles();
            lm.Classifier = new NeuralNetworkClassifier
            {
                HiddenLayers = new[] { 50 },
                UseCrossValidationSets = false,
                MaxTrainingIterations = 50,
                NegativeToPositiveWeightRatio = 1
            };

            var (_, testResult) = lm.TrainAndTestWithCsvData(false, train, test, false, _ => { }, CancellationToken.None);
            testResult.Match(_ => throw new ArgumentException("Not expected"),
                confusionMatrix => Assert.AreEqual(0.85, Math.Round(confusionMatrix.Recall, 2)));
        }

        // Test one tenth to one weight ratio for Neural Net
        // This makes the recall on the test set go up
        [Test, Category("ClassWeights")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "NeuralNet")]
        public static void NeuralNetOneTenthToOneWeightRatio()
        {
            var (lm, train, test) = SetCsvFiles();
            lm.Classifier = new NeuralNetworkClassifier
            {
                HiddenLayers = new[] { 50 },
                UseCrossValidationSets = false,
                MaxTrainingIterations = 50,
                NegativeToPositiveWeightRatio = .1
            };

            var (_, testResult) = lm.TrainAndTestWithCsvData(false, train, test, false, _ => { }, CancellationToken.None);
            testResult.Match(_ => throw new ArgumentException("Not expected"),
                confusionMatrix => Assert.AreEqual(0.89, Math.Round(confusionMatrix.Recall, 2)));
        }

        // Test ten to one weight ratio for Neural Net
        // This makes the recall on the test set go down
        [Test, Category("ClassWeights")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "NeuralNet")]
        public static void NeuralNetTenToOneWeightRatio()
        {
            var (lm, train, test) = SetCsvFiles();
            lm.Classifier = new NeuralNetworkClassifier
            {
                HiddenLayers = new[] { 50 },
                UseCrossValidationSets = false,
                MaxTrainingIterations = 50,
                NegativeToPositiveWeightRatio = 10
            };

            var (_, testResult) = lm.TrainAndTestWithCsvData(false, train, test, false, _ => { }, CancellationToken.None);
            testResult.Match(_ => throw new ArgumentException("Not expected"),
                confusionMatrix => Assert.AreEqual(0.83, Math.Round(confusionMatrix.Recall, 2)));
        }


        #endregion Tests

        #region Helper Methods

        private static (LearningMachine, string, string) SetCsvFiles()
        {
            var lmResourceName = "Resources.LearningMachine.TestWeightRatio.lm";
            var trainCsvResourceName = "Resources.LearningMachine.TestWeightRatio.train.csv";
            var testCsvResourceName = "Resources.LearningMachine.TestWeightRatio.test.csv";
            var lmName = _testFiles.GetFile(lmResourceName, "");
            var train = _testFiles.GetFile(trainCsvResourceName, "");
            var test = _testFiles.GetFile(testCsvResourceName, "");

            return (LearningMachine.Load(lmName), train, test);
        }

        #endregion Helper Methods
    }
}
