﻿using Accord.MachineLearning.VectorMachines;
using Accord.Math;
using Accord.Statistics;
using Accord.Statistics.Analysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// Base class for support vector machine classifiers
    /// </summary>
    [CLSCompliant(false)]
    [Serializable]
    // Don't rename because it could break serialization
    [Obfuscation(Feature = "renaming", Exclude = true)]
    public abstract class SupportVectorMachineClassifier : ITrainableClassifier
    {
        #region Fields

        /// <summary>
        /// Vector of feature means. These values will be subtracted from input feature vectors.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected double[] FeatureMean;

        /// <summary>
        /// Vector of scaling factors that feature vectors will be divided by to standardize them.
        /// Calculated before training by computing the standard deviation and adding a small positive
        /// quantity to guard against division by zero.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected double[] FeatureScaleFactor;

        // Backing fields for properties
        private int _featureVectorLength;
        private ISupportVectorMachine _classifier;
        private double _complexity;
        private bool _automaticallyChooseComplexityValue;
        private int _numberOfClasses;
        private bool _isTrained;
        private DateTime _lastTrainedOn;

        #endregion Fields

        #region Properties

        /// <summary>
        /// The feature vector length that this classifier requires
        /// </summary>
        protected int FeatureVectorLength
        {
            get
            {
                return _featureVectorLength;
            }
            set
            {
                if (value != _featureVectorLength)
                {
                    _featureVectorLength = value;
                }
            }
        }

        /// <summary>
        /// The underlying classifier
        /// </summary>
        protected ISupportVectorMachine Classifier
        {
            get
            {
                return _classifier;
            }
            set
            {
                if (value != _classifier)
                {
                    _classifier = value;
                }
            }
        }

        /// <summary>
        /// The complexity (error cost) value to use for training
        /// </summary>
        public double Complexity
        {
            get
            {
                return _complexity;
            }
            set
            {
                if (value != _complexity)
                {
                    _complexity = value;
                }
            }
        }

        /// <summary>
        /// Whether to automatically choose a complexity value based on cross-validation sets
        /// </summary>
        public bool AutomaticallyChooseComplexityValue
        {
            get
            {
                return _automaticallyChooseComplexityValue;
            }
            set
            {
                if (value != _automaticallyChooseComplexityValue)
                {
                    _automaticallyChooseComplexityValue = value;
                }
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Create an instance with default values
        /// </summary>
        protected SupportVectorMachineClassifier()
        {
            AutomaticallyChooseComplexityValue = true;
            Complexity = 1;
        }

        #endregion Constructors

        #region ITrainableClassifier

        /// <summary>
        /// The number of classes that this classifier can recognize
        /// </summary>
        public int NumberOfClasses
        {
            get
            {
                return _numberOfClasses;
            }
            private set
            {
                if (value != _numberOfClasses)
                {
                    _numberOfClasses = value;
                }
            }
        }


        /// <summary>
        /// Whether this classifier has been trained and is ready to compute answers
        /// </summary>
        /// <returns>Whether this classifier has been trained and is ready to compute answers</returns>
        public bool IsTrained
        {
            get
            {
                return _isTrained;
            }
            private set
            {
                if (value != _isTrained)
                {
                    _isTrained = value;
                }
            }
        }

        /// <summary>
        /// The <see cref="DateTime"/> that this classifier was last trained
        /// </summary>
        public DateTime LastTrainedOn
        {
            get
            {
                return _lastTrainedOn;
            }
            private set
            {
                if (value != _lastTrainedOn)
                {
                    _lastTrainedOn = value;
                }
            }
        }

        /// <summary>
        /// Trains the classifier to recognize classifications
        /// </summary>
        /// <param name="inputs">The input feature vectors</param>
        /// <param name="outputs">The classes for each input</param>
        /// <param name="randomGenerator">Optional random number generator to use for randomness</param>
        public void TrainClassifier(double[][] inputs, int[] outputs, Random randomGenerator=null)
        {
            try
            {
                TrainClassifier(inputs, outputs, randomGenerator, _ => { }, CancellationToken.None);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39866");
            }
        }

        /// <summary>
        /// Trains the classifier to recognize classifications
        /// </summary>
        /// <param name="inputs">The input feature vectors</param>
        /// <param name="outputs">The classes for each input</param>
        /// <param name="randomGenerator">Random number generator to use for randomness</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        public void TrainClassifier(double[][] inputs, int[] outputs, Random randomGenerator, Action<StatusArgs> updateStatus,
            CancellationToken cancellationToken)
        {
            try
            {
                ExtractException.Assert("ELI40262", "No inputs given", inputs != null && inputs.Length > 0);
                ExtractException.Assert("ELI40263", "Inputs and outputs are different lengths", inputs.Length == outputs.Length);

                // Indent sub-status messages
                Action<StatusArgs> updateStatus2 = args =>
                    {
                        args.Indent++;
                        updateStatus(args);
                    };

                FeatureVectorLength = inputs[0].Length;
                ExtractException.Assert("ELI40264", "Inputs are different lengths",
                    inputs.All(vector => vector.Length == FeatureVectorLength));

                NumberOfClasses = outputs.Max() + 1;

                // Calculate standardization values
                FeatureMean = inputs.Mean();
                FeatureScaleFactor = inputs.StandardDeviation(FeatureMean);

                // Prevent divide by zero
                if (FeatureScaleFactor.Any(factor => factor == 0))
                {
                    FeatureScaleFactor.ApplyInPlace(factor => factor + 0.0001);
                }

                // Standardize input
                inputs = inputs.Subtract(FeatureMean).ElementwiseDivide(FeatureScaleFactor, inPlace: true);

                // Run training algorithm against subsets to pick a good Complexity value
                if (AutomaticallyChooseComplexityValue)
                {
                    // Split data into training and validation sets by getting random subsets of each
                    // category. This is to ensure at least one example of each class exists.
                    // Compute indexes for the two sets of data
                    List<int> trainIdx, cvIdx;
                    LearningMachine.GetIndexesOfSubsetsByCategory(outputs, 0.8, out trainIdx, out cvIdx, randomGenerator);

                    double[][] trainInputs = inputs.Submatrix(trainIdx);
                    int[] trainOutputs = outputs.Submatrix(trainIdx);

                    double[][] cvInputs = inputs.Submatrix(cvIdx);
                    int[] cvOutputs = outputs.Submatrix(cvIdx);

                    var complexitiesTried = new Dictionary<double, double>();
                    Func<double, bool, double> search = (start, fineTune) =>
                    {
                        double bestScore = 0;
                        var previousBestComplexities = new List<double>();
                        var bestComplexities = new List<double>();
                        int i = 1;
                        int decreasingRun = 0;
                        double startE = 0, endE = 0;
                        double midE = Math.Round(Math.Log(start, 2));
                        double step = 1;
                        if (fineTune)
                        {
                            startE = midE - 1;
                            endE = midE + 1;
                            step = 0.25;
                        }
                        else
                        {
                            startE = midE - 10;
                            endE = midE + 10;
                        }
                        for (double exp = startE; exp <= endE; i++, exp += step)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            double complexity = Math.Pow(2, exp);

                            updateStatus2(new StatusArgs
                            {
                                TaskName = "ChoosingComplexity",
                                StatusMessage = String.Format(CultureInfo.CurrentCulture,
                                fineTune
                                    ? "Fine-tuning complexity value: Iteration {0}, C={1:N6}"
                                    : "Choosing complexity value: Iteration {0}, C={1:N6}",
                                i,
                                complexity)
                            });
                            try
                            {
                                double score = 0;
                                bool alreadyDone = complexitiesTried.TryGetValue(complexity, out score);
                                if (!alreadyDone)
                                {
                                    TrainClassifier(trainInputs, trainOutputs, complexity, _ => { }, cancellationToken);
                                    score = GetAccuracyScore(cvInputs, cvOutputs);
                                    complexitiesTried[complexity] = score;
                                }

                                updateStatus2(new StatusArgs
                                {
                                    TaskName = "ChoosingComplexity",
                                    ReplaceLastStatus = true,
                                    StatusMessage = String.Format(CultureInfo.CurrentCulture,
                                    fineTune
                                        ? alreadyDone ? "Fine-tuning complexity value: Iteration {0}, C={1:N6}, CV accuracy={2:N4} (already done)"
                                                      : "Fine-tuning complexity value: Iteration {0}, C={1:N6}, CV accuracy={2:N4}"
                                        : "Choosing complexity value: Iteration {0}, C={1:N6}, CV accuracy={2:N4}",
                                    i,
                                    complexity,
                                    score)
                                });
                                if (score >= bestScore)
                                {
                                    if (score > bestScore)
                                    {
                                        bestScore = score;
                                        previousBestComplexities = bestComplexities;
                                        bestComplexities = new List<double>();
                                    }
                                    bestComplexities.Add(complexity);
                                    decreasingRun = 0;
                                }
                                else
                                {
                                    ++decreasingRun;
                                    if (!fineTune && decreasingRun == 3)
                                    {
                                        break;
                                    }
                                }
                            }
                            catch (AggregateException ae)
                            {
                                ae.Handle(ex => ex is Accord.ConvergenceException);

                                updateStatus2(new StatusArgs
                                {
                                    TaskName = "ChoosingComplexity",
                                    ReplaceLastStatus = true,
                                    StatusMessage = String.Format(CultureInfo.CurrentCulture,
                                    "Choosing complexity value: Iteration {0}, C={1:N6}, Unable to attain convergence!",
                                    i,
                                    complexity)
                                });

                                // https://extract.atlassian.net/browse/ISSUE-14113
                                // Handle convergence exception as a signal that the complexity value
                                // being tried is too high
                                if (complexity >= start && bestComplexities.Any())
                                {
                                    // Use previous list of bests if there is only one current best (too close to non-convergence)
                                    if (previousBestComplexities.Any() && bestComplexities.Count == 1)
                                    {
                                        bestComplexities = previousBestComplexities;
                                    }
                                    break;
                                }
                            }
                        }

                        // Pick the middle value for complexities, using lower value if there is an even number of bests
                        // use '1' if there are no bests
                        double chosen = bestComplexities.Count == 0
                            ? 1
                            : bestComplexities[(bestComplexities.Count - 1) / 2];

                        return chosen;
                    };

                    // Pick best region
                    Complexity = search(1, false);

                    // Fine-tune
                    Complexity = search(Complexity, true);

                    updateStatus2(new StatusArgs
                    {
                        StatusMessage = String.Format(CultureInfo.CurrentCulture,
                        "Complexity chosen: C={0}",
                        Complexity)
                    });
                }

                bool success = false;
                do
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        // Train classifier
                        updateStatus(new StatusArgs { StatusMessage = "Training classifier:" });
                        TrainClassifier(inputs, outputs, Complexity, updateStatus2, cancellationToken);

                        success = IsTrained = true;
                        LastTrainedOn = DateTime.Now;
                    }
                    // https://extract.atlassian.net/browse/ISSUE-14113
                    // Handle convergence exception as a signal that the complexity value being tried
                    // is too high
                    catch (AggregateException ae)
                    {
                        ae.Handle(ex => ex is Accord.ConvergenceException);

                        updateStatus2(new StatusArgs
                        {
                            StatusMessage = String.Format(CultureInfo.CurrentCulture,
                            "Unable to attain convergence: C={0:N6}",
                            Complexity)
                        });

                        // Try lower complexity value
                        Complexity *= 0.75;
                        updateStatus2(new StatusArgs
                        {
                            StatusMessage = String.Format(CultureInfo.CurrentCulture,
                            "Trying again with lower Complexity value: C={0:N6}",
                            Complexity)
                        });
                    }
                }
                while (!success);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI40265");
            }
        }

        /// <summary>
        /// Computes answer code and score for the input feature vector
        /// </summary>
        /// <param name="inputs">The feature vector</param>
        /// <returns>The answer code and score</returns>
        public abstract Tuple<int, double?> ComputeAnswer(double[] inputs);

        /// <summary>
        /// Whether this instance has the same configured properties as another
        /// </summary>
        /// <param name="otherClassifier">The <see cref="ITrainableClassifier"/> to compare with this instance</param>
        /// <returns><see langword="true"/> if the configurations are the same, else <see langword="false"/></returns>
        public virtual bool IsConfigurationEqualTo(ITrainableClassifier otherClassifier)
        {
            try
            {
                if (Object.ReferenceEquals(this, otherClassifier))
                {
                    return true;
                }

                var other = otherClassifier as SupportVectorMachineClassifier;
                if (other == null
                    || other.GetType() != GetType()
                    || other.AutomaticallyChooseComplexityValue != AutomaticallyChooseComplexityValue
                    || other.Complexity != Complexity)
                {
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39821");
            }
        }

        /// <summary>
        /// Clear training information
        /// </summary>
        public void Clear()
        {
            LastTrainedOn = DateTime.MinValue;
            IsTrained = false;
            Classifier = null;
        }

        /// <summary>
        /// Pretty prints this object with supplied <see cref="System.CodeDom.Compiler.IndentedTextWriter"/>
        /// </summary>
        /// <param name="writer">The <see cref="System.CodeDom.Compiler.IndentedTextWriter"/> to use</param>
        public virtual void PrettyPrint(System.CodeDom.Compiler.IndentedTextWriter writer)
        {
            try
            {
                var oldIndent = writer.Indent;
                writer.Indent++;
                if (IsTrained)
                {
                    writer.WriteLine("LastTrainedOn: {0:s}", LastTrainedOn);
                }
                else
                {
                    writer.WriteLine("LastTrainedOn: Never");
                }
                writer.WriteLine("AutomaticallyChooseComplexityValue: {0}", AutomaticallyChooseComplexityValue);
                writer.WriteLine("Complexity: {0}", Complexity);
                writer.Indent = oldIndent;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI40068");
            }
        }

        #endregion ITrainableClassifier

        #region Protected Methods

        /// <summary>
        /// Trains the classifier to be able to predict classes for inputs
        /// </summary>
        /// <param name="inputs">Array of feature vectors</param>
        /// <param name="outputs">Array of classes (category codes) for each input</param>
        /// <param name="complexity">Complexity value to use for training</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        protected abstract void TrainClassifier(double[][] inputs, int[] outputs, double complexity,
            Action<StatusArgs> updateStatus, CancellationToken cancellationToken);

        #endregion Protected Methods

        #region Private Methods

        /// <summary>
        /// Computes the accuracy or F1 score of the <see cref="Classifier"/>
        /// </summary>
        /// <param name="inputs">The feature vectors</param>
        /// <param name="outputs">The expected results</param>
        /// <returns>The F1 score if there are two classes else the overall agreement</returns>
        private double GetAccuracyScore(double[][] inputs, int[] outputs)
        {
            try
            {
                int[] predictions = inputs.Apply(x =>
                {
                    double _;
                    return Classifier.Compute(x, out _);
                });

                if (NumberOfClasses == 2)
                {
                    var cm = new ConfusionMatrix(predictions, outputs);
                    return cm.FScore;
                }
                else
                {
                    var gc = new GeneralConfusionMatrix(NumberOfClasses, predictions, outputs);
                    return gc.OverallAgreement;
                }
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI40374");
            }
        }

        #endregion Private Methods
    }
}
