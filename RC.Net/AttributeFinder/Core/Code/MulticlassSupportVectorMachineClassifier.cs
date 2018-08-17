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
