using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Math;
using Accord.Statistics.Kernels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// One vs one SVM classifier
    /// </summary>
    [CLSCompliant(false)]
    [Serializable]
    // Don't rename because it could break serialization
    [Obfuscation(Feature = "renaming", Exclude = true)]
    public class MulticlassSupportVectorMachineClassifier : SupportVectorMachineClassifier, IDisposable
    {
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
            var classifier = new MulticlassSupportVectorMachine(FeatureVectorLength, kernel, NumberOfClasses);

            // Train classifier
            var teacher = new MulticlassSupportVectorLearning(classifier, inputs, outputs)
            {
                Algorithm = (svm, classInputs, classOutputs, positiveClassIndex, negativeClassIndex) =>
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
                    // positive class index (the other parameter is just -posidx)
                    // NOTE: The positive class is compared for consistency, because the other type of SVM (multilabel)
                    // is only passed a valid positive class index (the negative class index param is just the positive index with the sign changed)
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
                    updateStatus(new StatusArgs { StatusMessage = "Sub-problems finished: {0:N0}", Int32Value = 1 });
                };

            var error = teacher.Run(true, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            updateStatus(new StatusArgs { StatusMessage = "Training error: {0:N4}", DoubleValues = new[] { error } });
            Classifier = classifier;
        }

        /// <summary>
        /// Computes answer code and score for the input feature vector
        /// </summary>
        /// <remarks>Answer score will always be null</remarks>
        /// <param name="inputs">The feature vector</param>
        /// <param name="standardizeInputs">Whether to apply zero-center and normalize the input</param>
        /// <returns>The answer code and score</returns>
        public override (int answerCode, double? score) ComputeAnswer(double[] inputs, bool standardizeInputs = true)
        {
            try
            {
                ExtractException.Assert("ELI39729", "This classifier has not been trained", IsTrained);

                // Scale inputs
                if (standardizeInputs
                    && FeatureMean != null
                    && FeatureScaleFactor != null)
                {
                    inputs = inputs.Subtract(FeatureMean).ElementwiseDivide(FeatureScaleFactor);
                }

                int answer = ((MulticlassSupportVectorMachine)Classifier).Compute(inputs);

                return (answer, null);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39724");
            }
        }

        #endregion Overrides

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="MulticlassSupportVectorMachineClassifier"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="MulticlassSupportVectorMachineClassifier"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="MulticlassSupportVectorMachineClassifier"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ((MulticlassSupportVectorMachine)Classifier).Dispose();
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members
    }
}