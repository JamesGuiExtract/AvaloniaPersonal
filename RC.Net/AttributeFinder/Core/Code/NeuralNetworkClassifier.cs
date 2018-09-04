using Accord.Math;
using Accord.Neuro;
using AForge.Neuro;
using LearningMachineTrainer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// Classifier that uses an Activation Network
    /// </summary>
    [Serializable]
    [CLSCompliant(false)]
    // Don't rename because it could break serialization
    [Obfuscation(Feature = "renaming", Exclude = true)]
    public class NeuralNetworkClassifier : INeuralNetModel, ITrainableClassifier, IIncrementallyTrainableClassifier
    {
        #region Constants

        const int _CURRENT_VERSION = 2;

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

        [OptionalField(VersionAdded = 2)]
        private int _version = _CURRENT_VERSION;

        [OptionalField(VersionAdded = 2)]
        private double? _negativeToPositiveWeightRatio;

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

        /// <summary>
        /// The feature vector length that this classifier requires
        /// </summary>
        public int FeatureVectorLength
        {
            get
            {
                return _featureVectorLength;
            }
            set
            {
                if (value != _featureVectorLength)
                {
                    _featureVectorLength = value;
                }
            }
        }

        /// <summary>
        /// The underlying classifier
        /// </summary>
        public ActivationNetwork Classifier
        {
            get
            {
                return _classifier;
            }
            set
            {
                if (value != _classifier)
                {
                    _classifier = value;
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
            set
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
            set
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
            set
            {
                if (value != _lastTrainedOn)
                {
                    _lastTrainedOn = value;
                }
            }
        }

        /// <summary>
        /// Vector of feature means. These values will be subtracted from input feature vectors.
        /// </summary>
        public double[] FeatureMean
        {
            get => _featureMean;
            set => _featureMean = value;
        }

        /// <summary>
        /// Vector of feature means. These values will be subtracted from input feature vectors.
        /// </summary>
        public double[] FeatureScaleFactor
        {
            get => _featureScaleFactor;
            set => _featureScaleFactor = value;
        }

        public double? NegativeToPositiveWeightRatio
        {
            get => _negativeToPositiveWeightRatio;
            set => _negativeToPositiveWeightRatio = value;
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
                ExtractException.Assert("ELI39717", "No inputs given", inputs != null && inputs.Length > 0);
                ExtractException.Assert("ELI39718", "Inputs and outputs are different lengths", inputs.Length == outputs.Length);

                NeuralNetMethods.TrainClassifier(this, inputs, outputs, randomGenerator, updateStatus, cancellationToken);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39722");
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
                    || other.UseCrossValidationSets != UseCrossValidationSets
                    || other.NegativeToPositiveWeightRatio != NegativeToPositiveWeightRatio)
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
                if (NegativeToPositiveWeightRatio.HasValue)
                {
                    writer.WriteLine("WeightRatio: {0}", NegativeToPositiveWeightRatio);
                }
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
                if (FeatureMean != null && FeatureScaleFactor != null && FeatureScaleFactor.All(n => n != 0))
                {
                    input = input.Subtract(FeatureMean).ElementwiseDivide(FeatureScaleFactor);
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

        /// <summary>
        /// Called when deserializing
        /// </summary>
        /// <param name="context">The context.</param>
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            // Set optional fields
            _version = 1; // _version added with version 2

            _negativeToPositiveWeightRatio = null;
        }

        /// <summary>
        /// Called when deserialized
        /// </summary>
        /// <param name="context">The context.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            ExtractException.Assert("ELI46195", "Cannot load newer NeuralNetworkClassifier",
                _version <= _CURRENT_VERSION,
                "Current version", _CURRENT_VERSION,
                "Version to load", _version);

            _version = _CURRENT_VERSION;
        }

        #endregion Private Methods
    }
}
