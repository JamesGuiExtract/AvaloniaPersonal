using Extract.Testing.Utilities;
using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System;

namespace Extract.AttributeFinder.Test
{
    /// <summary>
    /// Unit tests for implementations of ITrainableClassifier
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("TrainableClassifier")]
    public class TestTrainableClassifier
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
        static TestFileManager<TestTrainableClassifier> _testFiles;
        static string[] _ussFiles;
        static string[] _voaFiles;
        static string[] _eavFiles;
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

            _testFiles = new TestFileManager<TestTrainableClassifier>();
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
        }

        #endregion Overhead

        #region Tests

        // Test multi-class SVM with pagination problem
        [Test, Category("TrainableClassifier")]
        public static void PaginationMulticlassSVM()
        {
            Tuple<double[][], int[]> results = GetPaginationData();
            double[][] inputs = results.Item1;
            int[] outputs = results.Item2;
            var classifier = new MulticlassSupportVectorMachineClassifier{Complexity=-1, AutomaticallyChooseComplexityValue=true};
            Assert.That(!classifier.IsTrained);
            Assert.Throws<ExtractException>(() => classifier.ComputeAnswer(inputs[0]));
            classifier.TrainClassifier(inputs, outputs, new System.Random(0));
            Assert.That(classifier.IsTrained);
            // Complexity value will have been chosen to be a non-negative number
            Assert.Greater(classifier.Complexity, 0);
            double f1Score = LearningMachine.GetAccuracyScore(classifier, inputs, outputs);
            Assert.AreEqual(1.0, f1Score);
        }

        // Test multi-class SVM without automatically chosen complexity value
        [Test, Category("TrainableClassifier")]
        public static void PaginationMulticlassSVMNoAutoC()
        {
            Tuple<double[][], int[]> results = GetPaginationData();
            double[][] inputs = results.Item1;
            int[] outputs = results.Item2;

            // Use a very low complexity to lower the accuracy
            var classifier = new MulticlassSupportVectorMachineClassifier { Complexity = 0.0001, AutomaticallyChooseComplexityValue = false };
            classifier.TrainClassifier(inputs, outputs, new System.Random(0));
            double f1Score = LearningMachine.GetAccuracyScore(classifier, inputs, outputs);

            // Missed one example
            Assert.AreEqual(14.0/15.0, f1Score);
        }

        // Test multi-label SVM with pagination problem
        [Test, Category("TrainableClassifier")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multilabel")]
        public static void PaginationMultilabelSVM()
        {
            Tuple<double[][], int[]> results = GetPaginationData();
            double[][] inputs = results.Item1;
            int[] outputs = results.Item2;

            var classifier = new MultilabelSupportVectorMachineClassifier { Complexity = -1, AutomaticallyChooseComplexityValue = true };
            Assert.That(!classifier.IsTrained);
            Assert.Throws<ExtractException>(() => classifier.ComputeAnswer(inputs[0]));
            classifier.TrainClassifier(inputs, outputs, new System.Random(0));
            Assert.That(classifier.IsTrained);

            // Complexity value will have been chosen to be a non-negative number
            Assert.Greater(classifier.Complexity, 0);
            double f1Score = LearningMachine.GetAccuracyScore(classifier, inputs, outputs);
            Assert.AreEqual(1.0, f1Score);
        }

        // Test multi-label SVM calibrated to produce probabilities
        [Test, Category("TrainableClassifier")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multilabel")]
        public static void PaginationMultilabelSVMProbabilities()
        {
            Tuple<double[][], int[]> results = GetPaginationData();
            double[][] inputs = results.Item1;
            int[] outputs = results.Item2;
            var classifier = new MultilabelSupportVectorMachineClassifier { CalibrateMachineToProduceProbabilities = true };
            classifier.TrainClassifier(inputs, outputs, new System.Random(0));
            double f1Score = LearningMachine.GetAccuracyScore(classifier, inputs, outputs);
            Assert.AreEqual(1.0, f1Score);

            // Example has good probability score
            Assert.Greater(classifier.ComputeAnswer(inputs[9]).Item2, 0.9);
        }

        // Test multi-label SVM without unknown category
        [Test, Category("TrainableClassifier")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multilabel")]
        public static void DocClassificationMultilabelSVMProbabilitiesWithoutOneExample()
        {
            Tuple<double[][], int[]> results = GetDocumentCategorizationData();
            double[][] inputs = results.Item1;
            int[] outputs = results.Item2;
            var classifier = new MultilabelSupportVectorMachineClassifier { CalibrateMachineToProduceProbabilities = true };

            // Train without one category
            classifier.TrainClassifier(inputs.Take(9).ToArray(), outputs.Take(9).ToArray(), new System.Random(0));
            double f1Score = LearningMachine.GetAccuracyScore(classifier, inputs, outputs);

            // Gets one wrong
            Assert.AreEqual(0.9, f1Score);

            // Has low probability
            Assert.Less(classifier.ComputeAnswer(inputs[9]).Item2, 0.3);
        }

        // Test neural net with pagination problem
        [Test, Category("TrainableClassifier")]
        public static void PaginationNeuralNet()
        {
            Tuple<double[][], int[]> results = GetPaginationData();
            double[][] inputs = results.Item1;
            int[] outputs = results.Item2;
            var classifier = new NeuralNetworkClassifier();
            Assert.That(!classifier.IsTrained);
            Assert.Throws<ExtractException>(() => classifier.ComputeAnswer(inputs[0]));
            classifier.TrainClassifier(inputs, outputs, new System.Random(0));
            Assert.That(classifier.IsTrained);
            double f1Score = LearningMachine.GetAccuracyScore(classifier, inputs, outputs);
            Assert.AreEqual(1.0, f1Score);
        }

        // Test neural net with pagination problem and shingle feature
        [Test, Category("TrainableClassifier")]
        public static void PaginationNeuralNetWithAutoBow()
        {
            Tuple<double[][], int[]> results = GetPaginationData(true);
            double[][] inputs = results.Item1;
            int[] outputs = results.Item2;
            var classifier = new NeuralNetworkClassifier();
            Assert.That(!classifier.IsTrained);
            Assert.Throws<ExtractException>(() => classifier.ComputeAnswer(inputs[0]));
            classifier.TrainClassifier(inputs, outputs, new System.Random(0));
            Assert.That(classifier.IsTrained);
            double f1Score = LearningMachine.GetAccuracyScore(classifier, inputs, outputs);

            // Network performance hurt by extra features
            Assert.Greater(f1Score, 0.92);
            Assert.Less(f1Score, 0.93);
        }

        // Test multi-class SVM with doc classification problem
        [Test, Category("TrainableClassifier")]
        public static void DocClassificationMulticlassSVM()
        {
            Tuple<double[][], int[]> results = GetDocumentCategorizationData();
            double[][] inputs = results.Item1;
            int[] outputs = results.Item2;
            var classifier = new MulticlassSupportVectorMachineClassifier();
            classifier.TrainClassifier(inputs, outputs, new System.Random(0));
            double accuracy = LearningMachine.GetAccuracyScore(classifier, inputs, outputs);
            Assert.AreEqual(1.0, accuracy);
        }

        // Test multi-label SVM with doc classification problem
        [Test, Category("TrainableClassifier")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multilabel")]
        public static void DocClassificationMultilabelSVM()
        {
            Tuple<double[][], int[]> results = GetDocumentCategorizationData();
            double[][] inputs = results.Item1;
            int[] outputs = results.Item2;
            var classifier = new MultilabelSupportVectorMachineClassifier();
            classifier.TrainClassifier(inputs, outputs, new System.Random(0));
            double accuracy = LearningMachine.GetAccuracyScore(classifier, inputs, outputs);
            Assert.AreEqual(1.0, accuracy);
        }

        // Test neural net with doc classification problem
        [Test, Category("TrainableClassifier")]
        public static void DocClassificationNeuralNet()
        {
            Tuple<double[][], int[]> results = GetDocumentCategorizationData();
            double[][] inputs = results.Item1;
            int[] outputs = results.Item2;
            var classifier = new NeuralNetworkClassifier();
            classifier.TrainClassifier(inputs, outputs, new System.Random(0));
            double accuracy = LearningMachine.GetAccuracyScore(classifier, inputs, outputs);
            Assert.AreEqual(1.0, accuracy);
        }

        // Test neural net without using cv set
        [Test, Category("TrainableClassifier")]
        public static void DocClassificationNeuralNetNoCV()
        {
            Tuple<double[][], int[]> results = GetDocumentCategorizationData();
            double[][] inputs = results.Item1;
            int[] outputs = results.Item2;
            var classifier = new NeuralNetworkClassifier { UseCrossValidationSets = false, MaxTrainingIterations = 10};
            classifier.TrainClassifier(inputs, outputs, new System.Random(0));
            double accuracy = LearningMachine.GetAccuracyScore(classifier, inputs, outputs);
            Assert.AreEqual(0.8, accuracy);
        }

        #endregion Tests

        #region Helper Methods

        private static Tuple<double[][], int[]> GetPaginationData(bool withShingles=false)
        {
            SetPaginationFiles();
            SpatialStringFeatureVectorizer autoBow = null;
            if (withShingles)
            {
                autoBow = new SpatialStringFeatureVectorizer("", 5, 2000);
            }
            var encoder = new LearningMachineDataEncoder(LearningMachineUsage.Pagination, autoBow, attributeFilter:"*@Feature");
            encoder.ComputeEncodings(_ussFiles, _voaFiles, _eavFiles);
            return encoder.GetFeatureVectorAndAnswerCollections(_ussFiles, _voaFiles, _eavFiles);
        }

        // Helper method to build file lists for pagination testing
        // These images are stapled together from Demo_LabDE images
        private static void SetPaginationFiles()
        {
            _ussFiles = new string[7];
            _voaFiles = new string[7];
            _eavFiles = new string[7];
            for (int i = 0; i < _ussFiles.Length; i++)
            {
                var baseName = "Resources.LearningMachine.Pagination.Pagination_{0:D3}.tif{1}";
                _testFiles.GetFile(string.Format(CultureInfo.CurrentCulture, baseName, i+1, ""));
                _ussFiles[i] = _testFiles.GetFile(string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".uss"));
                _voaFiles[i] = _testFiles.GetFile(string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".protofeatures.voa"));
                _eavFiles[i] = _testFiles.GetFile(string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".eav"));
            }
        }


        private static Tuple<double[][], int[]> GetDocumentCategorizationData()
        {
            SetDocumentCategorizationFiles();
            var autoBow = new SpatialStringFeatureVectorizer("", 5, 2000);
            var encoder = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization, autoBow);
            encoder.ComputeEncodings(_ussFiles, null, _categories);
            return encoder.GetFeatureVectorAndAnswerCollections(_ussFiles, null, _categories);
        }

        // Helper method to create file lists for document categorization testing
        // These files are from Demo_FlexIndex
        private static void SetDocumentCategorizationFiles()
        {
            _ussFiles = new string[10];
            _voaFiles = new string[10];
            for (int i = 0; i < _ussFiles.Length; i++)
            {
                var baseName = "Resources.LearningMachine.DocumentCategorization.Example{0:D2}.tif{1}";
                _testFiles.GetFile(string.Format(CultureInfo.CurrentCulture, baseName, i+1, ""));
                _ussFiles[i] = _testFiles.GetFile(string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".uss"));
                _voaFiles[i] = _testFiles.GetFile(string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".voa"));
            }
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
        }

        #endregion Helper Methods
    }
}
