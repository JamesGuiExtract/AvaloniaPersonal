using Accord.MachineLearning.VectorMachines;
using Accord.Math;
using Accord.Statistics;
using Accord.Statistics.Analysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// Different accuracy measures used to score a classifier
    /// </summary>
    public enum MachineScoreType
    {
        OverallAgreementOrF1 = 0,
        Precision = 1,
        Recall = 2,
    }

    /// <summary>
    /// Base class for support vector machine classifiers
    /// </summary>
    [CLSCompliant(false)]
    [Serializable]
    // Don't rename because it could break serialization
    [Obfuscation(Feature = "renaming", Exclude = true)]
    public abstract class SupportVectorMachineClassifier : ITrainableClassifier
    {
        #region Constants

        const int _CURRENT_VERSION = 2;

        #endregion Constants

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

        [OptionalField(VersionAdded = 2)]
        private int _version = _CURRENT_VERSION;

        [OptionalField(VersionAdded = 2)]
        private int? _smoCacheSize;

        [OptionalField(VersionAdded = 2)]
        private double? _positiveToNegativeWeightRatio;

        [OptionalField(VersionAdded = 2)]
        private bool _conditionallyApplyWeightRatio;

        [OptionalField(VersionAdded = 2)]
        private MachineScoreType _scoreTypeToUseForComplexityChoosingAlgorithm;

        /// <summary>
        /// Store setting for SMO algorithm as a property even though not a serialized one
        /// in case it causes a problem and has to be overridden
        /// </summary>
        [NonSerialized]
        private bool _createCompactMachine = true;

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

        /// <summary>
        /// What portion of the Complexity parameter will be applied to the positive class during training
        /// </summary>
        public double? PositiveToNegativeWeightRatio
        {
            get
            {
                return _positiveToNegativeWeightRatio;
            }
            set
            {
                _positiveToNegativeWeightRatio = value;
            }
        }

        /// <summary>
        /// Whether the <see cref="PositiveToNegativeWeightRatio"/> value should only be applied when the
        /// positive class == 0 (the ID of the overall designated Negative class name)
        /// </summary>
        public bool ConditionallyApplyWeightRatio
        {
            get
            {
                return _conditionallyApplyWeightRatio;
            }
            set
            {
                _conditionallyApplyWeightRatio = value;
            }
        }

        /// <summary>
        /// What type of accuracy score will be used to compare training results
        /// </summary>
        public MachineScoreType ScoreTypeToUseForComplexityChoosingAlgorithm
        {
            get
            {
                return _scoreTypeToUseForComplexityChoosingAlgorithm;
            }
            set
            {
                _scoreTypeToUseForComplexityChoosingAlgorithm = value;
            }
        }

        /// <summary>
        /// Whether to create a compact machine (if false then the machine will retain all support vectors and take up ~ 100 times the memory)
        /// </summary>
        public bool CreateCompactMachine
        {
            get
            {
                return _createCompactMachine;
            }
            set
            {
                _createCompactMachine = value;
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
        /// Size of the cache used by the Sequential Minimum Optimization algorithm
        /// </summary>
        /// <remarks>
        /// Larger cache means faster training but more memory usage.
        /// If <c>null</c> then the size will be the same as the size of the input vector (training set size)
        /// </remarks>
        public int? TrainingAlgorithmCacheSize
        {
            get
            {
                return _smoCacheSize;
            }
            set
            {
                _smoCacheSize = value;
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

                string accuracyType;
                if (ScoreTypeToUseForComplexityChoosingAlgorithm == MachineScoreType.Precision)
                {
                    accuracyType = "Precision";
                }
                else if (ScoreTypeToUseForComplexityChoosingAlgorithm == MachineScoreType.Recall)
                {
                    accuracyType = "Recall";
                }
                else
                {
                    accuracyType = NumberOfClasses == 2 ? "F1-Score" : "Accuracy";
                }

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
                    // NOTE: Chnaged to be 50/50 sets because arguably this will give more accurate results (and it is faster)
                    LearningMachine.GetIndexesOfSubsetsByCategory(outputs, 0.5, out List<int> trainIdx, out List<int> cvIdx, randomGenerator);

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
                        int equalRun = 0;
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
                                i, complexity)
                            });
                            try
                            {
                                double score = 0;
                                bool alreadyDone = complexitiesTried.TryGetValue(complexity, out score);
                                if (!alreadyDone)
                                {
                                    TrainClassifier(trainInputs, trainOutputs, complexity, choosingComplexity: true, updateStatus: _ => { }, cancellationToken: cancellationToken);
                                    score = GetAccuracyScore(cvInputs, cvOutputs, ScoreTypeToUseForComplexityChoosingAlgorithm);
                                    complexitiesTried[complexity] = score;
                                }

                                updateStatus2(new StatusArgs
                                {
                                    TaskName = "ChoosingComplexity",
                                    ReplaceLastStatus = true,
                                    StatusMessage = String.Format(CultureInfo.CurrentCulture,
                                    fineTune
                                        ? alreadyDone ? "Fine-tuning complexity value: Iteration {0}, C={1:N6}, CV {2}={3:N4} (already done)"
                                                      : "Fine-tuning complexity value: Iteration {0}, C={1:N6}, CV {2}={3:N4}"
                                        : "Choosing complexity value: Iteration {0}, C={1:N6}, CV {2}={3:N4}",
                                    i, complexity, accuracyType, score)
                                });
                                if (score >= bestScore)
                                {
                                    if (score > bestScore)
                                    {
                                        bestScore = score;
                                        previousBestComplexities = bestComplexities;
                                        bestComplexities = new List<double>();
                                        equalRun = 0;
                                    }
                                    else
                                    {
                                        ++equalRun;
                                        // https://extract.atlassian.net/browse/ISSUE-14727
                                        if (!fineTune && equalRun == 3 && complexity > 1)
                                        {
                                            break;
                                        }
                                    }
                                    bestComplexities.Add(complexity);
                                    decreasingRun = 0;
                                }
                                else
                                {
                                    ++decreasingRun;
                                    equalRun = 0;
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
                                if (bestComplexities.Any())
                                {
                                    // Use previous list of bests if there is only one current best (too close to non-convergence)
                                    if (decreasingRun == 0 && previousBestComplexities.Any() && bestComplexities.Count == 1)
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
                        TrainClassifier(inputs, outputs, Complexity, choosingComplexity: false, updateStatus: updateStatus2, cancellationToken: cancellationToken);

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
        public abstract (int answerCode, double? score) ComputeAnswer(double[] inputs);

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
                    || other.Complexity != Complexity
                    || other.PositiveToNegativeWeightRatio != PositiveToNegativeWeightRatio
                    || other.TrainingAlgorithmCacheSize != TrainingAlgorithmCacheSize
                    || other.ScoreTypeToUseForComplexityChoosingAlgorithm != ScoreTypeToUseForComplexityChoosingAlgorithm
                    || other.ConditionallyApplyWeightRatio != ConditionallyApplyWeightRatio)
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
                writer.WriteLine("ScoreUsedForComplexityChoosing: {0}", ScoreTypeToUseForComplexityChoosingAlgorithm.ToString());
                writer.WriteLine("Complexity: {0}", Complexity);
                writer.WriteLine("WeightRatio: {0}", PositiveToNegativeWeightRatio);
                writer.WriteLine("ConditionallyApplyWeightRatio: {0}", ConditionallyApplyWeightRatio);
                writer.WriteLine("TrainingAlgorithmCacheSize: {0}", TrainingAlgorithmCacheSize);
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
        /// <param name="choosingComplexity">Whether this method is being called as part of figuring out what Complexity parameter is the best
        /// (and thus no need to calibrate the machine for probabilities, e.g.)</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        protected abstract void TrainClassifier(double[][] inputs, int[] outputs, double complexity, bool choosingComplexity,
            Action<StatusArgs> updateStatus, CancellationToken cancellationToken);

        #endregion Protected Methods

        #region Private Methods

        /// <summary>
        /// Computes the accuracy, precision, recall or F1 score of the <see cref="Classifier"/>
        /// </summary>
        /// <param name="inputs">The feature vectors</param>
        /// <param name="outputs">The expected results</param>
        /// <param name="scoreType">The type of measure to return as accuracy</param>
        /// <returns>The specified score type if there are two classes else the overall agreement</returns>
        private double GetAccuracyScore(double[][] inputs, int[] outputs, MachineScoreType scoreType = MachineScoreType.OverallAgreementOrF1)
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
                    if (scoreType == MachineScoreType.Precision)
                    {
                        return cm.Precision;
                    }
                    else if (scoreType == MachineScoreType.Recall)
                    {
                        return cm.Recall;
                    }
                    else
                    {
                        return cm.FScore;
                    }
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

        /// <summary>
        /// Called when deserializing
        /// </summary>
        /// <param name="context">The context.</param>
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            // Set non-serialized fields
            _createCompactMachine = true;
            _positiveToNegativeWeightRatio = null;
            _smoCacheSize = null;

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
            ExtractException.Assert("ELI43525", "Cannot load newer SupportVectorMachineClassifier",
                _version <= _CURRENT_VERSION,
                "Current version", _CURRENT_VERSION,
                "Version to load", _version);

            _version = _CURRENT_VERSION;
        }

        #endregion Private Methods
    }
}
