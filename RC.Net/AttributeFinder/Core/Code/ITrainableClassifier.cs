using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// Interface for learning machine classifiers
    /// </summary>
    public interface ITrainableClassifier
    {
        /// <summary>
        /// Trains the classifier to recognize classifications
        /// </summary>
        /// <param name="inputs">The input feature vectors</param>
        /// <param name="outputs">The classes for each input</param>
        /// <param name="randomGenerator">Optional random number generator to use for randomness</param>
        void TrainClassifier(double[][] inputs, int[] outputs, Random randomGenerator=null);

        /// <summary>
        /// Trains the classifier to recognize classifications
        /// </summary>
        /// <param name="inputs">The input feature vectors</param>
        /// <param name="outputs">The classes for each input</param>
        /// <param name="randomGenerator">Random number generator to use for randomness</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        void TrainClassifier(double[][] inputs, int[] outputs, Random randomGenerator, Action<StatusArgs> updateStatus,
            CancellationToken cancellationToken);

        /// <summary>
        /// Computes answer code and score for the input feature vector
        /// </summary>
        /// <param name="inputs">The feature vector</param>
        /// <returns>The answer code and score</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        (int answerCode, double? score) ComputeAnswer(double[] inputs);

        /// <summary>
        /// The number of classes that this classifier can recognize
        /// </summary>
        int NumberOfClasses { get; }

        /// <summary>
        /// Whether this classifier has been trained and is ready to compute answers
        /// </summary>
        /// <returns>Whether this classifier has been trained and is ready to compute answers</returns>
        bool IsTrained { get; }

        /// <summary>
        /// The <see cref="DateTime"/> that this classifier was last trained
        /// </summary>
        DateTime LastTrainedOn { get; }

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
}