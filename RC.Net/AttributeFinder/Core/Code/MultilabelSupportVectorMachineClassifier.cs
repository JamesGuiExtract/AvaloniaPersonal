using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Math;
using Accord.Statistics.Kernels;
using LearningMachineTrainer;
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
    public class MultilabelSupportVectorMachineClassifier : SupportVectorMachineClassifier, IMultilabelSupportVectorMachineModel, IDisposable
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
        /// Computes answer code and score for the input feature vector
        /// </summary>
        /// <remarks>Answer score will be null unless <see cref="CalibrateMachineToProduceProbabilities"/>
        /// was <see langword="true"/> when this instance was trained</remarks>
        /// <param name="inputs">The feature vector</param>
        /// <param name="standardizeInputs">Whether to apply zero-center and normalize the input</param>
        /// <returns>The answer code and score</returns>
        public override (int answerCode, double? score) ComputeAnswer(double[] inputs, bool standardizeInputs = true)
        {
            try
            {
                ExtractException.Assert("ELI39741", "This classifier has not been trained", IsTrained);

                var classifier = (MultilabelSupportVectorMachine)Classifier;

                // Scale inputs
                if (standardizeInputs
                    && FeatureMean != null
                    && FeatureScaleFactor != null)
                {
                    inputs = inputs.Subtract(FeatureMean).ElementwiseDivide(FeatureScaleFactor);
                }

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
                ((MultilabelSupportVectorMachine)Classifier)?.Dispose();
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
