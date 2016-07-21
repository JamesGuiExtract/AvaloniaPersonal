using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Math;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;

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
        #region Fields

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
        /// Whether to give different weight to errors for some classes based on number of
        /// examples of each class (so that minority classes are not ignored)
        /// </summary>
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
            UseClassProportionsForComplexityWeights = true;
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Trains the classifier to be able to predict classes for inputs
        /// </summary>
        /// <param name="inputs">Array of feature vectors</param>
        /// <param name="outputs">Array of classes (category codes) for each input</param>
        /// <param name="complexity">Complexity value to use for training</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        protected override void TrainClassifier(double[][] inputs, int[] outputs, double complexity,
            Action<StatusArgs> updateStatus, CancellationToken cancellationToken)
        {
            // Build classifier
            var kernel = new Accord.Statistics.Kernels.Linear();
            var classifier = new MultilabelSupportVectorMachine(FeatureVectorLength, kernel, NumberOfClasses);

            // Train classifier
            var teacher = new MultilabelSupportVectorLearning(classifier, inputs, outputs);
            teacher.Algorithm = (svm, classInputs, classOutputs, i, j) =>
                {
                    var f = new SequentialMinimalOptimization(svm, classInputs, classOutputs);
                    f.Complexity = complexity;
                    f.UseClassProportions = UseClassProportionsForComplexityWeights;
                    return f;
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
            if (CalibrateMachineToProduceProbabilities)
            {
                updateStatus(new StatusArgs { StatusMessage = "Calibrating..." });
                for (int i = 0; i < classifier.Machines.Length; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var machine = classifier.Machines[i];
                    var outputsForMachine = outputs.Apply(y => y == i ? 1 : -1);
                    var calibration = new ProbabilisticOutputCalibration(machine, inputs, outputsForMachine);
                    likelihood += calibration.Run() / inputs.Length;
                }
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
        public override Tuple<int, double?> ComputeAnswer(double[] inputs)
        {
            try
            {
                ExtractException.Assert("ELI39741", "This classifier has not been trained", IsTrained);

                var classifier = (MultilabelSupportVectorMachine)Classifier;

                // Scale inputs
                inputs = inputs.Subtract(FeatureMean).ElementwiseDivide(FeatureScaleFactor);

                double[] responses;
                classifier.Compute(inputs, out responses);

                int imax;
                double? max = responses.Max(out imax);

                // Only return score if classifier is probabilistic
                if (!classifier.IsProbabilistic)
                {
                    max = null;
                }
                return Tuple.Create(imax, max);
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
                    || other.UseClassProportionsForComplexityWeights != UseClassProportionsForComplexityWeights)
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
                writer.WriteLine("UseClassProportionsForComplexityWeights: {0}", UseClassProportionsForComplexityWeights);
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
    }
}
