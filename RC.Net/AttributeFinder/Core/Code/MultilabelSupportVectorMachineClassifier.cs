using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Math;
using Accord.Statistics.Kernels;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// One vs many SVM
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multilabel")]
    [CLSCompliant(false)]
    [Serializable]
    // Don't rename because it could break serialization
    [Obfuscation(Feature = "renaming", Exclude = true)]
    public class MultilabelSupportVectorMachineClassifier : SupportVectorMachineClassifier, IDisposable
    {
        #region Constants

        const int _CURRENT_VERSION = 2;

        #endregion Constants

        #region Fields

        [OptionalField(VersionAdded = 2)]
        private int _version = _CURRENT_VERSION;

        // Backing fields for properties
        private bool _calibrateMachineToProduceProbabilities;
        private bool _useClassProportionsForComplexityWeights;

        #endregion Fields

        #region Properties

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

        /// <summary>
        /// This was used as a parameter to the SMO training algorithm but it is no longer used.
        /// The value of this parameter would not affect run-time behavior of a classifier
        /// </summary>
        [Obsolete("This feature has been deprecated. Use explicit WeightRatio property instead.")]
        public bool UseClassProportionsForComplexityWeights
        {
            get
            {
                return _useClassProportionsForComplexityWeights;
            }
            set
            {
                if (value != _useClassProportionsForComplexityWeights)
                {
                    _useClassProportionsForComplexityWeights = value;
                }
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Creates a new instance with default values
        /// </summary>
        public MultilabelSupportVectorMachineClassifier() : base()
        {
            CalibrateMachineToProduceProbabilities = false;
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Trains the classifier to be able to predict classes for inputs
        /// </summary>
        /// <param name="inputs">Array of feature vectors</param>
        /// <param name="outputs">Array of classes (category codes) for each input</param>
        /// <param name="complexity">Complexity value to use for training</param>
        /// <param name="choosingComplexity">Whether this method is being called as part of figuring out what Complexity parameter is the best
        /// (and thus no need to calibrate the machine for probabilities, e.g.)</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        protected override void TrainClassifier(double[][] inputs, int[] outputs, double complexity, bool choosingComplexity,
            Action<StatusArgs> updateStatus, CancellationToken cancellationToken)
        {
            // Build classifier
            IKernel kernel = new Linear();
            var classifier = new MultilabelSupportVectorMachine(FeatureVectorLength, kernel, NumberOfClasses);

            // Train classifier
            var teacher = new MultilabelSupportVectorLearning(classifier, inputs, outputs)
            {
                Algorithm = (svm, classInputs, classOutputs, positiveClassIndex, _) =>
                {
                    var f = new SequentialMinimalOptimization(svm, classInputs, classOutputs)
                    {
                        Complexity = complexity,
                        Compact = (kernel is Linear) && CreateCompactMachine
                    };

                    // Only set WeightRatio if there is a specifed weight ratio that should always be applied,
                    // or one that should be conditionally applied and it is true that the positive class of this
                    // machine is the designated overall negative class. (e.g., LearningMachineDataEncoder._NOT_FIRST_PAGE_CATEGORY_CODE)
                    // NOTE: The positive class is compared because the function is only passed a valid
                    // positive class index (the negative class index param is just the positive index with the sign changed)
                    if (PositiveToNegativeWeightRatio.HasValue
                        && (!ConditionallyApplyWeightRatio || positiveClassIndex == 0))
                    {
                        f.WeightRatio = PositiveToNegativeWeightRatio.Value;
                    }

                    if (TrainingAlgorithmCacheSize.HasValue)
                    {
                        f.CacheSize = TrainingAlgorithmCacheSize.Value;
                    }

                    return f;
                }
            };
            teacher.SubproblemFinished +=
                delegate
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    updateStatus(new StatusArgs { StatusMessage = "Sub-problems finished: {0:N0}", Int32Value = 1 });
                };

            var error = teacher.Run(true);
            updateStatus(new StatusArgs { StatusMessage = "Training error: {0:N4}", DoubleValues = new[] { error } });

            double likelihood = 0;
            if (!choosingComplexity && CalibrateMachineToProduceProbabilities)
            {
                updateStatus(new StatusArgs { StatusMessage = "Calibrating..." });
                Parallel.For(0, classifier.Machines.Length, i =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var machine = classifier.Machines[i];
                    var outputsForMachine = outputs.Apply(y => y == i ? 1 : -1);
                    var calibration = new ProbabilisticOutputCalibration(machine, inputs, outputsForMachine);
                    likelihood += calibration.Run() / inputs.Length;
                });
                updateStatus(new StatusArgs { StatusMessage = "Calibrated. Average log-likelihood: {0:N4}",
                    DoubleValues = new[] { likelihood / classifier.Machines.Length },
                    ReplaceLastStatus = true });
            }
            Classifier = classifier;
        }

        /// <summary>
        /// Computes answer code and score for the input feature vector
        /// </summary>
        /// <remarks>Answer score will be null unless <see cref="CalibrateMachineToProduceProbabilities"/>
        /// was <see langword="true"/> when this instance was trained</remarks>
        /// <param name="inputs">The feature vector</param>
        /// <returns>The answer code and score</returns>
        public override (int answerCode, double? score) ComputeAnswer(double[] inputs)
        {
            try
            {
                ExtractException.Assert("ELI39741", "This classifier has not been trained", IsTrained);

                var classifier = (MultilabelSupportVectorMachine)Classifier;

                // Scale inputs
                inputs = inputs.Subtract(FeatureMean).ElementwiseDivide(FeatureScaleFactor);

                classifier.Compute(inputs, out double[] responses);

                double? max = responses.Max(out int imax);

                // Only return score if classifier is probabilistic
                if (!classifier.IsProbabilistic)
                {
                    max = null;
                }
                return (imax, max);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39742");
            }
        }

        /// <summary>
        /// Whether this instance has the same configured properties as another
        /// </summary>
        /// <param name="otherClassifier">The <see cref="ITrainableClassifier"/> to compare with this instance</param>
        /// <returns><see langword="true"/> if the configurations are the same, else <see langword="false"/></returns>
        public override bool IsConfigurationEqualTo(ITrainableClassifier otherClassifier)
        {
            try
            {
                if (Object.ReferenceEquals(this, otherClassifier))
                {
                    return true;
                }

                var other = otherClassifier as MultilabelSupportVectorMachineClassifier;
                if (other == null
                    || !base.IsConfigurationEqualTo(other)
                    || other.CalibrateMachineToProduceProbabilities != CalibrateMachineToProduceProbabilities
                   )
                {
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39823");
            }
        }

        /// <summary>
        /// Pretty prints this object with supplied <see cref="System.CodeDom.Compiler.IndentedTextWriter"/>
        /// </summary>
        /// <param name="writer">The <see cref="System.CodeDom.Compiler.IndentedTextWriter"/> to use</param>
        public override void PrettyPrint(System.CodeDom.Compiler.IndentedTextWriter writer)
        {
            try
            {
                base.PrettyPrint(writer);
                var oldIndent = writer.Indent;
                writer.Indent++;
                writer.WriteLine("CalibrateMachineToProduceProbabilities: {0}", CalibrateMachineToProduceProbabilities);
                writer.Indent = oldIndent;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI40070");
            }
        }

        #endregion Overrides

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="MultilabelSupportVectorMachineClassifier"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="MultilabelSupportVectorMachineClassifier"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="MultilabelSupportVectorMachineClassifier"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ((MultilabelSupportVectorMachine)Classifier).Dispose();
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members

        #region Private Methods

        /// <summary>
        /// Called when deserializing
        /// </summary>
        /// <param name="context">The context.</param>
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            // Set non-serialized fields

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
            ExtractException.Assert("ELI43530", "Cannot load newer MultilabelSupportVectorMachineClassifier",
                _version <= _CURRENT_VERSION,
                "Current version", _CURRENT_VERSION,
                "Version to load", _version);

            // Set obsolete fields
            _useClassProportionsForComplexityWeights = false;

            _version = _CURRENT_VERSION;
        }

        #endregion Private Methods

    }
}
