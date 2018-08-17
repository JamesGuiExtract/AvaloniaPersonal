using LearningMachineTrainer;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// Interface for learning machine classifiers
    /// </summary>
    public interface ITrainableClassifier : IClassifierModel
    {
        /// <summary>
        /// Whether this instance has the same configured properties as another
        /// </summary>
        /// <param name="otherClassifier">The <see cref="ITrainableClassifier"/> to compare with this instance</param>
        /// <returns><see langword="true"/> if the configurations are the same, else <see langword="false"/></returns>
        bool IsConfigurationEqualTo(ITrainableClassifier otherClassifier);

        /// <summary>
        /// Clear training information
        /// </summary>
        void Clear();

        /// <summary>
        /// Pretty prints this object with supplied <see cref="System.CodeDom.Compiler.IndentedTextWriter"/>
        /// </summary>
        /// <param name="writer">The <see cref="System.CodeDom.Compiler.IndentedTextWriter"/> to use</param>
        void PrettyPrint(System.CodeDom.Compiler.IndentedTextWriter writer);
    }

    public interface IIncrementallyTrainableClassifier
    {
        void TrainClassifier(double[] input, int output, int? numberOfClasses = null, Random randomGenerator = null);
    }
}
