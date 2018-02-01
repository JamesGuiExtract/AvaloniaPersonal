using Accord.Math;
using Accord.Neuro;
using Accord.Statistics;
using AForge.Neuro;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// Classifier that uses an Activation Network
    /// </summary>
    [Serializable]
    // Don't rename because it could break serialization
    [Obfuscation(Feature = "renaming", Exclude = true)]
    public class NeuralNetworkClassifier : ITrainableClassifier, IIncrementallyTrainableClassifier
    {

        #region Constants

        private static readonly int _WINDOW_SIZE = 20;

        #endregion Constants

        #region Private Fields

        /// <summary>
        /// The underlying classifier
        /// </summary>
        private ActivationNetwork _classifier;

        /// <summary>
        /// Vector of feature means. These values will be subtracted from input feature vectors.
        /// </summary>
        private double[] _featureMean;

        /// <summary>
        /// Vector of scaling factors that feature vectors will be divided by to standardize them.
        /// Calculated before training by computing the standard deviation and adding a small positive
        /// quantity to guard against division by zero.
        /// </summary>
        private double[] _featureScaleFactor;

        /// <summary>
        /// The feature vector length that this classifier requires
        /// </summary>
        private int _featureVectorLength;

        /// <summary>
        /// Backing field for <see cref="HiddenLayers"/>
        /// </summary>
        private int[] _hiddenLayers;
        private int _maxTrainingIterations;
        private int _numberOfCandidateNetworksToBuild;
        private double _sigmoidAlpha;
        private bool _useCrossValidationSets;
        private int _numberOfClasses;
        private bool _isTrained;
        private DateTime _lastTrainedOn;

        #endregion Private Fields

        #region Properties

        /// <summary>
        /// The number and size of hidden layers in the network
        /// </summary>
        public IEnumerable<int> HiddenLayers
        {
            get
            {
                return _hiddenLayers;
            }
            set
            {
                _hiddenLayers = value.ToArray();
            }
        }

        /// <summary>
        /// The maximum training iterations that will be run. Might be partially ignored
        /// if <see cref="UseCrossValidationSets"/> is <see langword="true"/>
        /// </summary>
        public int MaxTrainingIterations
        {
            get
            {
                return _maxTrainingIterations;
            }
            set
            {
                if (value != _maxTrainingIterations)
                {
                    _maxTrainingIterations = value;
                }
            }
        }

        /// <summary>
        /// The number of candidate networks that will be built in order to select the best
        /// </summary>
        public int NumberOfCandidateNetworksToBuild
        {
            get
            {
                return _numberOfCandidateNetworksToBuild;
            }
            set
            {
                if (value != _numberOfCandidateNetworksToBuild)
                {
                    _numberOfCandidateNetworksToBuild = value;
                }
            }
        }
        
        /// <summary>
        /// The sigmoid activation function alpha value (steepness)
        /// </summary>
        public double SigmoidAlpha
        {
            get
            {
                return _sigmoidAlpha;
            }
            set
            {
                if (value != _sigmoidAlpha)
                {
                    _sigmoidAlpha = value;
                }
            }
        }

        /// <summary>
        /// Whether to use cross validation sets to determine when to stop training
        /// </summary>
        public bool UseCrossValidationSets
        {
            get
            {
                return _useCrossValidationSets;
            }
            set
            {
                if (value != _useCrossValidationSets)
                {
                    _useCrossValidationSets = value;
                }
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Create a new instance with default properties
        /// </summary>
        public NeuralNetworkClassifier()
        {
            HiddenLayers = new[] { 25 };
            MaxTrainingIterations = 500;
            NumberOfCandidateNetworksToBuild = 5;
            SigmoidAlpha = 2.0;
            UseCrossValidationSets = true;
        }

        #endregion Constructors

        #region ITrainableClassifier

        /// <summary>
        /// The number of classes that this classifier can recognize
        /// </summary>
        public int NumberOfClasses
        {
            get
            {
                return _numberOfClasses;
            }
            private set
            {
                if (value != _numberOfClasses)
                {
                    _numberOfClasses = value;
                }
            }
        }

        /// <summary>
        /// Whether this classifier has been trained and is ready to compute answers
        /// </summary>
        /// <returns>Whether this classifier has been trained and is ready to compute answers</returns>
        public bool IsTrained
        {
            get
            {
                return _isTrained;
            }
            private set
            {
                if (value != _isTrained)
                {
                    _isTrained = value;
                }
            }
        }

        /// <summary>
        /// The <see cref="DateTime"/> that this classifier was last trained
        /// </summary>
        public DateTime LastTrainedOn
        {
            get
            {
                return _lastTrainedOn;
            }
            private set
            {
                if (value != _lastTrainedOn)
                {
                    _lastTrainedOn = value;
                }
            }
        }

        /// <summary>
        /// Trains the classifier to recognize classifications
        /// </summary>
        /// <param name="inputs">The input feature vectors</param>
        /// <param name="outputs">The classes for each input</param>
        /// <param name="randomGenerator">Optional random number generator to use for randomness</param>
        /// <remarks>This method will modify the input arrays (standardize the features)</remarks>
        public void TrainClassifier(double[][] inputs, int[] outputs, Random randomGenerator=null)
        {
            try
            {
                TrainClassifier(inputs, outputs, randomGenerator, _ => { }, CancellationToken.None);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39867");
            }
        }

        /// <summary>
        /// Trains the classifier to recognize classifications
        /// </summary>
        /// <param name="inputs">The input feature vectors</param>
        /// <param name="outputs">The classes for each input</param>
        /// <param name="randomGenerator">Random number generator to use for randomness</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        /// <remarks>This method will modify the input arrays (standardize the features)</remarks>
        public void TrainClassifier(double[][] inputs, int[] outputs, Random randomGenerator, Action<StatusArgs> updateStatus,
            CancellationToken cancellationToken)
        {
            try
            {
                // Indent sub-status messages
                Action<StatusArgs> updateStatus2 = args =>
                    {
                        args.Indent++;
                        updateStatus(args);
                    };

                ExtractException.Assert("ELI39717", "No inputs given", inputs != null && inputs.Length > 0);
                ExtractException.Assert("ELI39718", "Inputs and outputs are different lengths", inputs.Length == outputs.Length);

                // If a random number generator was specified, then specify the random number generator
                // for neuron initialization so that results are reproducible
                if (randomGenerator != null)
                {
                    AForge.Neuro.Neuron.RandGenerator = new AForge.ThreadSafeRandom(randomGenerator.Next());
                }

                _featureVectorLength = inputs[0].Length;
                ExtractException.Assert("ELI39719", "Inputs are different lengths",
                    inputs.All(vector => vector.Length == _featureVectorLength));

                NumberOfClasses = outputs.Max() + 1;

                (_featureMean, _featureScaleFactor) = LearningMachine.StandardizeFeatures(inputs);

                // Expand output into one-hot vectors
                double[][] expandedOutputs = Accord.Statistics.Tools.Expand(outputs, NumberOfClasses, negative: -1.0, positive: 1.0);

                int[] layers = HiddenLayers.Concat(new int[] { NumberOfClasses }).ToArray();

                // Run training algorithm
                if (!UseCrossValidationSets)
                {
                    updateStatus(new StatusArgs { StatusMessage = "Training classifier:" });
                    _classifier = TrainClassifier(inputs, expandedOutputs, layers, updateStatus2, cancellationToken);
                }
                else
                {
                    updateStatus(new StatusArgs { StatusMessage = "Building classifier:" });

                    int numberOfNetworks = Math.Max(1, NumberOfCandidateNetworksToBuild);
                    double lowestError = double.MaxValue;
                    for (int i = 0; i < numberOfNetworks; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        updateStatus2(new StatusArgs
                        {
                            TaskName = "TrainCandidateNet",
                            ReplaceLastStatus = true,
                            StatusMessage = string.Format(CultureInfo.CurrentCulture,
                                "Training candidate classifier {0}", i + 1)
                        });

                        // Split data into training and validation sets by getting random subsets of each
                        // category. This is to ensure at least one example of each class exists.
                        // Compute indexes for the two sets of data
                        List<int> trainIdx, cvIdx;
                        LearningMachine.GetIndexesOfSubsetsByCategory(outputs, 0.8, out trainIdx, out cvIdx, randomGenerator);

                        double[][] trainInputs = inputs.Submatrix(trainIdx);
                        double[][] trainOutputs = expandedOutputs.Submatrix(trainIdx);

                        double[][] cvInputs = inputs.Submatrix(cvIdx);
                        double[][] cvOutputs = expandedOutputs.Submatrix(cvIdx);

                        // Train the classifier
                        ActivationNetwork ann;
                        double cvError = TrainClassifier(trainInputs, trainOutputs, cvInputs, cvOutputs, layers, out ann,
                            updateStatus2, cancellationToken);
                        if (cvError < lowestError)
                        {
                            _classifier = ann;
                        }
                    }
                }

                IsTrained = true;
                LastTrainedOn = DateTime.Now;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39722");
            }
        }

        /// <summary>
        /// Computes answer code and score for the input feature vector
        /// </summary>
        /// <remarks>Answer score will always be null</remarks>
        /// <param name="inputs">The feature vector</param>
        /// <param name="standardizeInputs">Whether to apply zero-center and normalize the input</param>
        /// <returns>The answer code and score</returns>
        public (int answerCode, double? score) ComputeAnswer(double[] inputs, bool standardizeInputs = true)
        {
            try
            {
                ExtractException.Assert("ELI39736", "This classifier has not been trained", IsTrained);

                // Scale inputs
                if (standardizeInputs
                    && _featureMean != null
                    && _featureScaleFactor != null)
                {
                    inputs = inputs.Subtract(_featureMean).ElementwiseDivide(_featureScaleFactor);
                }

                double[] responses = _classifier.Compute(inputs);

                // Return index of highest value neuron in the output layer
                responses.Max(out int imax);

                return (imax, null);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39727");
            }
        }

        /// <summary>
        /// Whether this instance has the same configured properties as another
        /// </summary>
        /// <param name="otherClassifier">The <see cref="ITrainableClassifier"/> to compare with this instance</param>
        /// <returns><see langword="true"/> if the configurations are the same, else <see langword="false"/></returns>
        public bool IsConfigurationEqualTo(ITrainableClassifier otherClassifier)
        {
            try
            {
                if (Object.ReferenceEquals(this, otherClassifier))
                {
                    return true;
                }

                var other = otherClassifier as NeuralNetworkClassifier;
                if (other == null
                    || !other.HiddenLayers.SequenceEqual(HiddenLayers)
                    || other.MaxTrainingIterations != MaxTrainingIterations
                    || other.NumberOfCandidateNetworksToBuild != NumberOfCandidateNetworksToBuild
                    || other.SigmoidAlpha != SigmoidAlpha
                    || other.UseCrossValidationSets != UseCrossValidationSets)
                {
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39822");
            }
        }

        /// <summary>
        /// Clear training information
        /// </summary>
        public void Clear()
        {
            LastTrainedOn = DateTime.MinValue;
            IsTrained = false;
            _classifier = null;
        }

        /// <summary>
        /// Pretty prints this object with supplied <see cref="System.CodeDom.Compiler.IndentedTextWriter"/>
        /// </summary>
        /// <param name="writer">The <see cref="System.CodeDom.Compiler.IndentedTextWriter"/> to use</param>
        public void PrettyPrint(System.CodeDom.Compiler.IndentedTextWriter writer)
        {
            try
            {
                var oldIndent = writer.Indent;
                writer.Indent++;
                if (IsTrained)
                {
                    writer.WriteLine("LastTrainedOn: {0:s}", LastTrainedOn);
                }
                else
                {
                    writer.WriteLine("LastTrainedOn: Never");
                }
                writer.WriteLine("Hidden Layers: {0}", string.Join(", ", HiddenLayers));
                writer.WriteLine("SigmoidAlpha: {0}", SigmoidAlpha);
                writer.WriteLine("NumberOfCandidateNetworksToBuild: {0}", NumberOfCandidateNetworksToBuild);
                writer.WriteLine("UseCrossValidationSets: {0}", UseCrossValidationSets);
                writer.WriteLine("MaxTrainingIterations: {0}", MaxTrainingIterations);
                writer.Indent = oldIndent;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI40069");
            }
        }

        #endregion ITrainableClassifier

        #region IIncrementallyTrainableClassifier

        /// <summary>
        /// Trains the classifier to recognize classifications
        /// </summary>
        /// <param name="input">The input feature vectors</param>
        /// <param name="output">The classes for each input</param>
        /// <param name="randomGenerator">Random number generator to use for randomness</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        public void TrainClassifier(double[] input, int output, int? numberOfClasses = null, Random randomGenerator = null)
        {
            try
            {
                ExtractException.Assert("ELI44707", "No input given", input != null && input.Length > 0);

                // If a random number generator was specified, then specify the random number generator
                // for neuron initialization so that results are reproducible
                if (randomGenerator != null)
                {
                    AForge.Neuro.Neuron.RandGenerator = new AForge.ThreadSafeRandom(randomGenerator.Next());
                }

                _featureVectorLength = input.Length;

                ExtractException.Assert("ELI44708", "Number of classes has not been set!", NumberOfClasses != 0 || numberOfClasses.HasValue);
                int[] layers = null;

                if (_classifier == null)
                {
                    if (NumberOfClasses == 0)
                    {
                        NumberOfClasses = numberOfClasses.Value;
                    }
                    layers = HiddenLayers.Concat(new int[] { NumberOfClasses }).ToArray();
                }


                // Standardize input
                if (_featureMean != null && _featureScaleFactor != null && _featureScaleFactor.All(n => n != 0))
                {
                    input = input.Subtract(_featureMean).ElementwiseDivide(_featureScaleFactor);
                }

                // Expand output into one-hot vector
                double[] expandedOutput = new double[NumberOfClasses];
                for (int i = 0; i < expandedOutput.Length; i++)
                {
                    expandedOutput[i] = -1;
                }
                expandedOutput[output] = 1;

                _classifier = TrainClassifier(input, expandedOutput, _classifier, layers);

                IsTrained = true;
                LastTrainedOn = DateTime.Now;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39722");
            }
        }

        #endregion IIncrementallyTrainableClassifier


        #region Private Methods

        /// <summary>
        /// Trains a classifier by running the training algorithm <see cref="MaxTrainingIterations"/> times.
        /// </summary>
        /// <param name="trainInputs">Feature vectors to train with</param>
        /// <param name="trainOutputs">Classes (category codes) for each training input</param>
        /// <param name="layers">Sizes of hidden and output layers</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        /// <returns>The resulting network</returns>
        private ActivationNetwork TrainClassifier(double[][] trainInputs, double[][] trainOutputs, int[] layers,
            Action<StatusArgs> updateStatus, CancellationToken cancellationToken)
        {
            var ann = new ActivationNetwork(new BipolarSigmoidFunction(SigmoidAlpha), _featureVectorLength, layers);
            var initializer = new NguyenWidrow(ann);
            initializer.Randomize();
            var teacher = new Accord.Neuro.Learning.ParallelResilientBackpropagationLearning(ann);
            int sampleSize = trainOutputs.Length;
            for (int i = 1; i <= MaxTrainingIterations; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                double error = teacher.RunEpoch(trainInputs, trainOutputs) / sampleSize;
                updateStatus(new StatusArgs
                {
                    TaskName = "RunEpoch",
                    ReplaceLastStatus = true,
                    StatusMessage = string.Format(CultureInfo.CurrentCulture,
                        "Training iteration: {0} Training error: {1:N4}", i, error)
                });
            }

            return ann;
        }

        /// <summary>
        /// Trains a classifier using a cross-validation set to stop before <see cref="MaxTrainingIterations"/> are reached.
        /// Assumes that <see cref="MaxTrainingIterations"/> is at least <see cref="_WINDOW_SIZE"/>
        /// </summary>
        /// <param name="trainInputs">Feature vectors to train with</param>
        /// <param name="trainOutputs">Classes (category codes) for each training input</param>
        /// <param name="cvInputs">Cross-validation set; feature vectors to check training progress against</param>
        /// <param name="cvOutputs">Array of classes (category codes) for each cross-validation input</param>
        /// <param name="layers">Sizes of hidden and output layers</param>
        /// <param name="trainedNetwork">The resulting network</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        /// <returns>The cross-validation error value of the resulting network</returns>
        private double TrainClassifier(double[][] trainInputs, double[][] trainOutputs,
            double[][] cvInputs, double[][] cvOutputs, int[] layers, out ActivationNetwork trainedNetwork,
            Action<StatusArgs> updateStatus, CancellationToken cancellationToken)
        {
            ExtractException.Assert("ELI39725", "MaxTrainingIterations must be at least " + _WINDOW_SIZE,
                MaxTrainingIterations >= _WINDOW_SIZE);

            var ann = new ActivationNetwork(new BipolarSigmoidFunction(SigmoidAlpha), _featureVectorLength, layers);
            var initializer = new NguyenWidrow(ann);
            initializer.Randomize();
            var teacher = new Accord.Neuro.Learning.ParallelResilientBackpropagationLearning(ann);

            var history = new Queue<Tuple<TemporaryFile, double>>(_WINDOW_SIZE);
            List<TemporaryFile> files = null;
            int trainSize = trainOutputs.Length;
            int cvSize = cvOutputs.Length;

            try
            {
                for (int i = 1; i <= MaxTrainingIterations; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    double trainError = teacher.RunEpoch(trainInputs, trainOutputs) / trainSize; ;
                    double cvError = teacher.ComputeError(cvInputs, cvOutputs) / cvSize;
                    updateStatus(new StatusArgs
                    {
                        TaskName = "RunEpoch",
                        ReplaceLastStatus = true,
                        StatusMessage = string.Format(CultureInfo.CurrentCulture,
                            "Training iteration: {0} Training error: {1:N4} Validation error: {2:N4}", i, trainError, cvError)
                    });

                    var savedNN = new TemporaryFile(false);
                    ann.Save(savedNN.FileName);
                    history.Enqueue(Tuple.Create(savedNN, cvError));

                    if (i >= _WINDOW_SIZE)
                    {
                        // Stop training if the results are not changing
                        // https://extract.atlassian.net/browse/ISSUE-14873
                        if (history.All(t => t.Item2 == history.Peek().Item2))
                        {
                            break;
                        }

                        var avgCVLast = history.Skip(_WINDOW_SIZE / 2).Average(t => t.Item2);
                        var avgCVPrevLast = history.Take(_WINDOW_SIZE / 2).Average(t => t.Item2);

                        // Break if CV error is trending upward
                        if (avgCVLast > avgCVPrevLast)
                        {
                            break;
                        }

                        // Throw away oldest saved NN
                        history.Dequeue();
                    }
                }

                // Retrieve the best NN
                files = history.Select(t => t.Item1).ToList();
                var errors = history.Select(t => t.Item2).ToArray();
                double lowestError = errors.Min(out int iMin);
                var bestFile = files[iMin];

                trainedNetwork = (ActivationNetwork)Network.Load(bestFile.FileName);
                return lowestError;
            }
            finally
            {
                files?.ClearAndDispose();
            }
        }

        /// <summary>
        /// Trains a classifier by running the training algorithm <see cref="MaxTrainingIterations"/> times.
        /// </summary>
        /// <param name="trainInput">Feature vector to train with</param>
        /// <param name="trainOutput">Class (category code) for the training input</param>
        /// <param name="ann">Neural net to be trained (if null then a new network will be created)</param>
        /// <param name="layers">Sizes of hidden and output layers (ignored if <see paramref="ann"/> is non-null)</param>
        /// <returns>The resulting network</returns>
        private ActivationNetwork TrainClassifier(double[] trainInput, double[] trainOutput, ActivationNetwork ann, int[] layers)
        {
            if (ann == null)
            {
                ann = new ActivationNetwork(new BipolarSigmoidFunction(SigmoidAlpha), _featureVectorLength, layers);
                var initializer = new NguyenWidrow(ann);
                initializer.Randomize();
            }
            var teacher = new Accord.Neuro.Learning.ParallelResilientBackpropagationLearning(ann);
            for (int i = 1; i <= MaxTrainingIterations; i++)
            {
                teacher.Run(trainInput, trainOutput);
            }

            return ann;
        }

        #endregion Private Methods
    }
}