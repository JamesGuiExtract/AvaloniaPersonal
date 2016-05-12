﻿using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Math;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// One vs many SVM
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multilabel")]
    [CLSCompliant(false)]
    public class MultilabelSupportVectorMachineClassifier : SupportVectorMachineClassifier, IDisposable
    {
        #region Properties

        /// <summary>
        /// Whether or not to produce a probabilistic machine
        /// </summary>
        public bool CalibrateMachineToProduceProbabilities
        {
            get;
            set;
        }

        /// <summary>
        /// Whether to give different weight to errors for some classes based on number of
        /// examples of each class (so that minority classes are not ignored)
        /// </summary>
        public bool UseClassProportionsForComplexityWeights
        {
            get;
            set;
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
        /// <returns>The trained support vector machine</returns>
        protected override void TrainClassifier(double[][] inputs, int[] outputs, double complexity)
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

            teacher.Run();

            if (CalibrateMachineToProduceProbabilities)
            {
                for (int i = 0; i < classifier.Machines.Length; i++)
                {
                    var machine = classifier.Machines[i];
                    var outputsForMachine = outputs.Apply(y => y == i ? 1 : -1);
                    var calibration = new ProbabilisticOutputCalibration(machine, inputs, outputsForMachine);
                    calibration.Run();
                }
            }

            Classifier = classifier;
        }

        /// <summary>
        /// Computes answer code and score for the input feature vector
        /// </summary>
        /// <remarks>Answer score will be null unless <see cref="CalibrateMachineToProduceProbabilities"/>
        /// was <see langref="true"/> when this instance was trained</remarks>
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