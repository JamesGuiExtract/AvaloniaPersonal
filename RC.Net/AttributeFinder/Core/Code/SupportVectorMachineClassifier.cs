using Accord.MachineLearning.VectorMachines;
using LearningMachineTrainer;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// Different accuracy measures used to score a classifier
    /// </summary>
    public enum MachineScoreType
    {
        OverallAgreementOrF1 = 0,
        Precision = 1,
        Recall = 2,
    }

    /// <summary>
    /// Base class for support vector machine classifiers
    /// </summary>
    [CLSCompliant(false)]
    [Serializable]
    // Don't rename because it could break serialization
    [Obfuscation(Feature = "renaming", Exclude = true)]
    public abstract class SupportVectorMachineClassifier : ITrainableClassifier, ISupportVectorMachineModel
    {
        #region Constants

        const int _CURRENT_VERSION = 2;

        #endregion Constants

        #region Fields

        /// <summary>
        /// Vector of feature means. These values will be subtracted from input feature vectors.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected double[] FeatureMean;

        /// <summary>
        /// Vector of scaling factors that feature vectors will be divided by to standardize them.
        /// Calculated before training by computing the standard deviation and adding a small positive
        /// quantity to guard against division by zero.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected double[] FeatureScaleFactor;

        // Backing fields for properties
        private int _featureVectorLength;
        private ISupportVectorMachine _classifier;
        private double _complexity;
        private bool _automaticallyChooseComplexityValue;
        private int _numberOfClasses;
        private bool _isTrained;
        private DateTime _lastTrainedOn;

        [OptionalField(VersionAdded = 2)]
        private int _version = _CURRENT_VERSION;

        [OptionalField(VersionAdded = 2)]
        private int? _smoCacheSize;

        [OptionalField(VersionAdded = 2)]
        private double? _positiveToNegativeWeightRatio;

        [OptionalField(VersionAdded = 2)]
        private bool _conditionallyApplyWeightRatio;

        [OptionalField(VersionAdded = 2)]
        private MachineScoreType _scoreTypeToUseForComplexityChoosingAlgorithm;

        /// <summary>
        /// Store setting for SMO algorithm as a property even though not a serialized one
        /// in case it causes a problem and has to be overridden
        /// </summary>
        [NonSerialized]
        private bool _createCompactMachine = true;

        [OptionalField(VersionAdded = 3)]
        protected bool _calibrateMachineToProduceProbabilities;

        #endregion Fields

        #region Properties

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
        public ISupportVectorMachine Classifier
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

        /// <summary>
        /// The complexity (error cost) value to use for training
        /// </summary>
        public double Complexity
        {
            get
            {
                return _complexity;
            }
            set
            {
                if (value != _complexity)
                {
                    _complexity = value;
                }
            }
        }

        /// <summary>
        /// Whether to automatically choose a complexity value based on cross-validation sets
        /// </summary>
        public bool AutomaticallyChooseComplexityValue
        {
            get
            {
                return _automaticallyChooseComplexityValue;
            }
            set
            {
                if (value != _automaticallyChooseComplexityValue)
                {
                    _automaticallyChooseComplexityValue = value;
                }
            }
        }

        /// <summary>
        /// What portion of the Complexity parameter will be applied to the positive class during training
        /// </summary>
        public double? PositiveToNegativeWeightRatio
        {
            get
            {
                return _positiveToNegativeWeightRatio;
            }
            set
            {
                _positiveToNegativeWeightRatio = value;
            }
        }

        /// <summary>
        /// Whether the <see cref="PositiveToNegativeWeightRatio"/> value should only be applied when the
        /// positive class == 0 (the ID of the overall designated Negative class name)
        /// </summary>
        public bool ConditionallyApplyWeightRatio
        {
            get
            {
                return _conditionallyApplyWeightRatio;
            }
            set
            {
                _conditionallyApplyWeightRatio = value;
            }
        }

        /// <summary>
        /// What type of accuracy score will be used to compare training results
        /// </summary>
        public MachineScoreType ScoreTypeToUseForComplexityChoosingAlgorithm
        {
            get
            {
                return _scoreTypeToUseForComplexityChoosingAlgorithm;
            }
            set
            {
                _scoreTypeToUseForComplexityChoosingAlgorithm = value;
            }
        }

        /// <summary>
        /// Whether to create a compact machine (if false then the machine will retain all support vectors and take up ~ 100 times the memory)
        /// </summary>
        public bool CreateCompactMachine
        {
            get
            {
                return _createCompactMachine;
            }
            set
            {
                _createCompactMachine = value;
            }
        }

        /// <summary>
        /// Whether or not to produce a probabilistic machine
        /// </summary>
        public bool CalibrateMachineToProduceProbabilities
        {
            get
            {
                return _calibrateMachineToProduceProbabilities;
            }
            set
            {
                if (value != _calibrateMachineToProduceProbabilities)
                {
                    _calibrateMachineToProduceProbabilities = value;
                }
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Create an instance with default values
        /// </summary>
        protected SupportVectorMachineClassifier()
        {
            AutomaticallyChooseComplexityValue = true;
            Complexity = 1;
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
        /// Size of the cache used by the Sequential Minimum Optimization algorithm
        /// </summary>
        /// <remarks>
        /// Larger cache means faster training but more memory usage.
        /// If <c>null</c> then the size will be the same as the size of the input vector (training set size)
        /// </remarks>
        public int? TrainingAlgorithmCacheSize
        {
            get
            {
                return _smoCacheSize;
            }
            set
            {
                _smoCacheSize = value;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        double[] IClassifierModel.FeatureMean { get => FeatureMean; set => FeatureMean = value; }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        double[] IClassifierModel.FeatureScaleFactor { get => FeatureScaleFactor; set => FeatureScaleFactor = value; }

        public int Version { get => _version; set => _version = value; }

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
                throw e.AsExtract("ELI39866");
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
                ExtractException.Assert("ELI40262", "No inputs given", inputs != null && inputs.Length > 0);
                ExtractException.Assert("ELI40263", "Inputs and outputs are different lengths", inputs.Length == outputs.Length);

                SvmMethods.TrainClassifier(this, inputs, outputs, randomGenerator, updateStatus, cancellationToken);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI40265");
            }
        }

        /// <summary>
        /// Whether this instance has the same configured properties as another
        /// </summary>
        /// <param name="otherClassifier">The <see cref="ITrainableClassifier"/> to compare with this instance</param>
        /// <returns><see langword="true"/> if the configurations are the same, else <see langword="false"/></returns>
        public virtual bool IsConfigurationEqualTo(ITrainableClassifier otherClassifier)
        {
            try
            {
                if (Object.ReferenceEquals(this, otherClassifier))
                {
                    return true;
                }

                var other = otherClassifier as SupportVectorMachineClassifier;
                if (other == null
                    || other.GetType() != GetType()
                    || other.AutomaticallyChooseComplexityValue != AutomaticallyChooseComplexityValue
                    || other.Complexity != Complexity
                    || other.PositiveToNegativeWeightRatio != PositiveToNegativeWeightRatio
                    || other.TrainingAlgorithmCacheSize != TrainingAlgorithmCacheSize
                    || other.ScoreTypeToUseForComplexityChoosingAlgorithm != ScoreTypeToUseForComplexityChoosingAlgorithm
                    || other.ConditionallyApplyWeightRatio != ConditionallyApplyWeightRatio
                    || other.CalibrateMachineToProduceProbabilities != CalibrateMachineToProduceProbabilities)
                {
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39821");
            }
        }

        /// <summary>
        /// Clear training information
        /// </summary>
        public void Clear()
        {
            LastTrainedOn = DateTime.MinValue;
            IsTrained = false;
            Classifier = null;
        }

        /// <summary>
        /// Pretty prints this object with supplied <see cref="System.CodeDom.Compiler.IndentedTextWriter"/>
        /// </summary>
        /// <param name="writer">The <see cref="System.CodeDom.Compiler.IndentedTextWriter"/> to use</param>
        public virtual void PrettyPrint(System.CodeDom.Compiler.IndentedTextWriter writer)
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
                writer.WriteLine("AutomaticallyChooseComplexityValue: {0}", AutomaticallyChooseComplexityValue);
                writer.WriteLine("ScoreUsedForComplexityChoosing: {0}", ScoreTypeToUseForComplexityChoosingAlgorithm.ToString());
                writer.WriteLine("Complexity: {0}", Complexity);
                writer.WriteLine("WeightRatio: {0}", PositiveToNegativeWeightRatio);
                writer.WriteLine("ConditionallyApplyWeightRatio: {0}", ConditionallyApplyWeightRatio);
                writer.WriteLine("TrainingAlgorithmCacheSize: {0}", TrainingAlgorithmCacheSize);
                writer.WriteLine("CalibrateMachineToProduceProbabilities: {0}", CalibrateMachineToProduceProbabilities);
                writer.Indent = oldIndent;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI40068");
            }
        }

        #endregion ITrainableClassifier

        #region Private Methods

        /// <summary>
        /// Called when deserializing
        /// </summary>
        /// <param name="context">The context.</param>
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            // Set non-serialized fields
            _createCompactMachine = true;
            _positiveToNegativeWeightRatio = null;
            _smoCacheSize = null;

            // Set optional fields
            _version = 1; // _version added with version 2
        }

        /// <summary>
        /// Called when deserialized
        /// </summary>
        /// <param name="context">The context.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            ExtractException.Assert("ELI43525", "Cannot load newer SupportVectorMachineClassifier",
                _version <= _CURRENT_VERSION,
                "Current version", _CURRENT_VERSION,
                "Version to load", _version);

            _version = _CURRENT_VERSION;
        }

        #endregion Private Methods
    }
}
