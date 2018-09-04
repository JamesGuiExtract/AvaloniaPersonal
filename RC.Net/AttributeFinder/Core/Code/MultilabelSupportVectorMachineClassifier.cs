using Accord.MachineLearning.VectorMachines;
using LearningMachineTrainer;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;

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
        private bool _useClassProportionsForComplexityWeights;

        #endregion Fields

        #region Properties

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
