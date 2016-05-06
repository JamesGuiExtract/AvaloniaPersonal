﻿using System;
using System.Diagnostics.CodeAnalysis;

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
        /// Computes answer code and score for the input feature vector
        /// </summary>
        /// <param name="inputs">The feature vector</param>
        /// <returns>The answer code and score</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        Tuple<int, double?> ComputeAnswer(double[] inputs);

        /// <summary>
        /// The number of classes that this classifier can recognize
        /// </summary>
        int NumberOfClasses { get; }

        /// <summary>
        /// Whether this classifier has been trained and is ready to compute answers
        /// </summary>
        /// <returns>Whether this classifier has been trained and is ready to compute answers</returns>
        bool IsTrained { get; }

        //event StatusUpdateEvent(); // TBD custom event data structure for showing progress status
    }
}