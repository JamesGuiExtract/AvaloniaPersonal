using Accord.MachineLearning.VectorMachines;
using Accord.Math;
using LearningMachineTrainer;
using System;
using System.Reflection;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// One vs one SVM classifier
    /// </summary>
    [CLSCompliant(false)]
    [Serializable]
    // Don't rename because it could break serialization
    [Obfuscation(Feature = "renaming", Exclude = true)]
    public class MulticlassSupportVectorMachineClassifier : SupportVectorMachineClassifier, IMulticlassSupportVectorMachineModel, IDisposable
    {
        #region Overrides

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
                ((MulticlassSupportVectorMachine)Classifier)?.Dispose();
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members
    }
}