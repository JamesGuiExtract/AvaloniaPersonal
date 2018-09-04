using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Math;
using Accord.Statistics.Kernels;
using Extract;
using Extract.AttributeFinder;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LearningMachineTrainer
{
    [CLSCompliant(false)]
    public interface ISupportVectorMachineModel : IClassifierModel
    {
        bool AutomaticallyChooseComplexityValue { get; set; }

        ISupportVectorMachine Classifier { get; set; }

        double Complexity { get; set; }

        bool ConditionallyApplyWeightRatio { get; set; }

        int FeatureVectorLength { get; set; }

        double? PositiveToNegativeWeightRatio { get; set; }

        MachineScoreType ScoreTypeToUseForComplexityChoosingAlgorithm { get; set; }

        int? TrainingAlgorithmCacheSize { get; set; }

        int Version { get; set; }

        bool CalibrateMachineToProduceProbabilities { get; set; }
    }

    [CLSCompliant(false)]
    public interface IMulticlassSupportVectorMachineModel : ISupportVectorMachineModel
    { }

    [CLSCompliant(false)]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multilabel")]
    public interface IMultilabelSupportVectorMachineModel : ISupportVectorMachineModel
    { }
    
    [CLSCompliant(false)]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Svm")]
    public static class SvmMethods
    {
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static (int answerCode, double? score) ComputeAnswer(ISupportVectorMachineModel model, double[] inputs)
        {
            try
            {
                double? max = null;
                int imax = model.Classifier.Compute(inputs, out double maxResponse);

                // Return score if classifier is probabilistic
                if (model.Classifier is MultilabelSupportVectorMachine ml && ml.IsProbabilistic
                    || model.Classifier is MulticlassSupportVectorMachine mc && mc.IsProbabilistic)
                {
                    max = maxResponse;
                }

                return (imax, max);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45692");
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
        /// <remarks>This method will modify the input arrays (standardize the features)</remarks>
        public static void TrainClassifier(ISupportVectorMachineModel model, double[][] inputs, int[] outputs,
            Random randomGenerator, Action<StatusArgs> updateStatus, CancellationToken cancellationToken)
        {
            try
            {
                // Standarize inputs and store mean and sigma values
                (model.FeatureMean, model.FeatureScaleFactor) = inputs.Standardize();

                // Indent sub-status messages
                Action<StatusArgs> updateStatus2 = args =>
                    {
                        args.Indent++;
                        updateStatus(args);
                    };

                model.FeatureVectorLength = inputs[0].Length;

                model.NumberOfClasses = outputs.Max() + 1;

                string accuracyType;
                if (model.ScoreTypeToUseForComplexityChoosingAlgorithm == MachineScoreType.Precision)
                {
                    accuracyType = "Precision";
                }
                else if (model.ScoreTypeToUseForComplexityChoosingAlgorithm == MachineScoreType.Recall)
                {
                    accuracyType = "Recall";
                }
                else
                {
                    accuracyType = model.NumberOfClasses == 2 ? "F1-Score" : "Accuracy";
                }

                // Run training algorithm against subsets to pick a good Complexity value
                if (model.AutomaticallyChooseComplexityValue)
                {
                    // Split data into training and validation sets by getting random subsets of each
                    // category. This is to ensure at least one example of each class exists.
                    // Compute indexes for the two sets of data
                    // NOTE: Chnaged to be 50/50 sets because arguably this will give more accurate results (and it is faster)
                    LearningMachineMethods.GetIndexesOfSubsetsByCategory(outputs, 0.5, out List<int> trainIdx, out List<int> cvIdx, randomGenerator);

                    double[][] trainInputs = inputs.Submatrix(trainIdx);
                    int[] trainOutputs = outputs.Submatrix(trainIdx);

                    double[][] cvInputs = inputs.Submatrix(cvIdx);
                    int[] cvOutputs = outputs.Submatrix(cvIdx);

                    var complexitiesTried = new Dictionary<double, double>();
                    double search(double start, bool fineTune)
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
                                    if (model is IMultilabelSupportVectorMachineModel ml)
                                    {
                                        TrainMultilabel(ml, trainInputs, trainOutputs, complexity, choosingComplexity: true,
                                            updateStatus: _ => { }, cancellationToken: cancellationToken);
                                    }
                                    else if (model is IMulticlassSupportVectorMachineModel mc)
                                    {
                                        TrainMulticlass(mc, trainInputs, trainOutputs, complexity, choosingComplexity: true,
                                            updateStatus: _ => { }, cancellationToken: cancellationToken);
                                    }
                                    score = LearningMachineMethods.GetAccuracyScore(model, cvInputs, cvOutputs, false,
                                        model.ScoreTypeToUseForComplexityChoosingAlgorithm);

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
                                bool continueLoop = true;

                                ae.Handle(ex =>
                                {
                                    if (ex is Accord.ConvergenceException)
                                    {
                                        updateStatus2(new StatusArgs
                                        {
                                            TaskName = "ChoosingComplexity",
                                            ReplaceLastStatus = true,
                                            StatusMessage = String.Format(CultureInfo.CurrentCulture,
                                            "Choosing complexity value iteration {0}: C={1:N6}, Unable to attain convergence!",
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
                                            continueLoop = false;
                                        }

                                        // Handled
                                        return true;
                                    }
                                    else if (ex is OutOfMemoryException)
                                    {
                                        updateStatus2(new StatusArgs
                                        {
                                            TaskName = "ChoosingComplexity",
                                            StatusMessage = "Error: Out of memory"
                                        });
                                    }

                                    // Not handled
                                    return false;
                                });

                                if (!continueLoop)
                                {
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
                    }

                    // Pick best region
                    model.Complexity = search(1, false);

                    // Fine-tune
                    model.Complexity = search(model.Complexity, true);

                    updateStatus2(new StatusArgs
                    {
                        StatusMessage = String.Format(CultureInfo.CurrentCulture,
                        "Complexity chosen: C={0}",
                        model.Complexity)
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
                        if (model is IMultilabelSupportVectorMachineModel ml)
                        {
                            TrainMultilabel(ml, inputs, outputs, model.Complexity, choosingComplexity: false,
                                updateStatus: updateStatus2, cancellationToken: cancellationToken);
                        }
                        else if (model is IMulticlassSupportVectorMachineModel mc)
                        {
                            TrainMulticlass(mc, inputs, outputs, model.Complexity, choosingComplexity: false,
                                updateStatus: updateStatus2, cancellationToken: cancellationToken);
                        }

                        success = model.IsTrained = true;
                        model.LastTrainedOn = DateTime.Now;
                    }
                    // https://extract.atlassian.net/browse/ISSUE-14113
                    // Handle convergence exception as a signal that the complexity value being tried
                    // is too high
                    catch (AggregateException ae)
                    {
                        ae.Handle(ex =>
                        {
                            if (ex is Accord.ConvergenceException)
                            {
                                updateStatus2(new StatusArgs
                                {
                                    StatusMessage = String.Format(CultureInfo.CurrentCulture,
                                    "Unable to attain convergence: C={0:N6}",
                                    model.Complexity)
                                });

                                // Try lower complexity value
                                model.Complexity *= 0.75;
                                updateStatus2(new StatusArgs
                                {
                                    StatusMessage = String.Format(CultureInfo.CurrentCulture,
                                    "Trying again with lower Complexity value: C={0:N6}",
                                    model.Complexity)
                                });

                                return true;
                            }
                            // Write out a message (used by MLModelTrainer) but don't handle the exception
                            else if (ex is OutOfMemoryException)
                            {
                                updateStatus2(new StatusArgs
                                {
                                    TaskName = "ChoosingComplexity",
                                    StatusMessage = "Error: Out of memory"
                                });
                            }

                            return false;
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
        /// Trains the classifier to be able to predict classes for inputs
        /// </summary>
        /// <param name="inputs">Array of feature vectors</param>
        /// <param name="outputs">Array of classes (category codes) for each input</param>
        /// <param name="complexity">Complexity value to use for training</param>
        /// <param name="choosingComplexity">Whether this method is being called as part of figuring out what Complexity parameter is the best
        /// (and thus no need to calibrate the machine for probabilities, e.g.)</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        private static void TrainMulticlass(IMulticlassSupportVectorMachineModel model, double[][] inputs, int[] outputs, double complexity, bool choosingComplexity,
            Action<StatusArgs> updateStatus, CancellationToken cancellationToken)
        {
            // Build classifier
            IKernel kernel = new Linear();
            var classifier = new MulticlassSupportVectorMachine(model.FeatureVectorLength, kernel, model.NumberOfClasses);

            // Train classifier
            var teacher = new MulticlassSupportVectorLearning(classifier, inputs, outputs)
            {
                // Accord source for MulticlassSupportVectorLearning indicates that positive class index
                // is the final parameter:
                //     // Transform it into a two-class problem
                //     subOutputs.ApplyInPlace(x => x = (x == i) ? -1 : +1);
                //     // Train the machine on the two-class problem.
                //     var subproblem = configure(machine, subInputs, subOutputs, i, j);
                Algorithm = (svm, classInputs, classOutputs, negativeClassIndex, positiveClassIndex) =>
                {
                    var f = new SequentialMinimalOptimization(svm, classInputs, classOutputs)
                    {
                        Complexity = complexity,
                        Compact = (kernel is Linear)
                    };

                    // Only set WeightRatio if there is a specifed weight ratio that should always be applied,
                    // or one that should be conditionally applied and it is true that the positive class of this
                    // machine is the designated overall negative class. (e.g., LearningMachineDataEncoder._NOT_FIRST_PAGE_CATEGORY_CODE)
                    // NOTE: The positive class is compared because the function is only passed a valid
                    // positive class index (the other parameter is just -posidx)
                    // NOTE: The positive class is compared for consistency, because the other type of SVM (multilabel)
                    // is only passed a valid positive class index (the negative class index param is just the positive index with the sign changed)
                    if (model.PositiveToNegativeWeightRatio.HasValue
                        && (!model.ConditionallyApplyWeightRatio || positiveClassIndex == 0))
                    {
                        f.WeightRatio = model.PositiveToNegativeWeightRatio.Value;
                    }

                    if (model.TrainingAlgorithmCacheSize.HasValue)
                    {
                        f.CacheSize = model.TrainingAlgorithmCacheSize.Value;
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

            double likelihood = 0;
            if (!choosingComplexity && model.CalibrateMachineToProduceProbabilities)
            {
                updateStatus(new StatusArgs { StatusMessage = "Calibrating..." });

                int classes = classifier.Classes;
                int total = (classes * (classes - 1)) / 2;
                var pairs = new (int i, int j)[total];
                for (int i = 0, k = 0; i < classes; i++)
                {
                    for (int j = 0; j < i; j++, k++)
                    {
                        pairs[k] = (i, j);
                    }
                }
                Parallel.For(0, total, k =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var (i, j) = pairs[k];
                    var machine = classifier[i, j];
                    int[] idx = outputs.Find(x => x == i || x == j);
                    double[][] subInputs = inputs.Submatrix(idx);
                    int[] subOutputs = outputs.Submatrix(idx);
                    subOutputs.ApplyInPlace(x => x = (x == i) ? -1 : +1);

                    var calibration = new ProbabilisticOutputCalibration(machine, subInputs, subOutputs);
                    likelihood += calibration.Run() / subInputs.Length;
                });
                updateStatus(new StatusArgs { StatusMessage = "Calibrated. Average log-likelihood: {0:N4}",
                    DoubleValues = new[] { likelihood / classifier.MachinesCount },
                    ReplaceLastStatus = true });
            }

            model.Classifier = classifier;
            updateStatus(new StatusArgs { StatusMessage = "Training error: {0:N4}", DoubleValues = new[] { error } });
        }

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
        private static void TrainMultilabel(IMultilabelSupportVectorMachineModel model, double[][] inputs, int[] outputs, double complexity, bool choosingComplexity,
            Action<StatusArgs> updateStatus, CancellationToken cancellationToken)
        {
            // Build classifier
            IKernel kernel = new Linear();
            var classifier = new MultilabelSupportVectorMachine(model.FeatureVectorLength, kernel, model.NumberOfClasses);

            // Train classifier
            var teacher = new MultilabelSupportVectorLearning(classifier, inputs, outputs)
            {
                // Accord source for MultilabelSupportVectorLearning indicates that positive class index
                // is the penultimate parameter:
                //     // Extract outputs for the given label
                //     int[] subOutputs = outputs.GetColumn(i);
                //     // Train the machine on the two-class problem.
                //     configure(machine, inputs, subOutputs, i, -i).Run(false);
                Algorithm = (svm, classInputs, classOutputs, positiveClassIndex, _) =>
                {
                    var f = new SequentialMinimalOptimization(svm, classInputs, classOutputs)
                    {
                        Complexity = complexity,
                        Compact = (kernel is Linear)
                    };

                    // Only set WeightRatio if there is a specifed weight ratio that should always be applied,
                    // or one that should be conditionally applied and it is true that the positive class of this
                    // machine is the designated overall negative class. (e.g., LearningMachineDataEncoder._NOT_FIRST_PAGE_CATEGORY_CODE)
                    // NOTE: The positive class is compared because the function is only passed a valid
                    // positive class index (the negative class index param is just the positive index with the sign changed)
                    if (model.PositiveToNegativeWeightRatio.HasValue
                        && (!model.ConditionallyApplyWeightRatio || positiveClassIndex == 0))
                    {
                        f.WeightRatio = model.PositiveToNegativeWeightRatio.Value;
                    }

                    if (model.TrainingAlgorithmCacheSize.HasValue)
                    {
                        f.CacheSize = model.TrainingAlgorithmCacheSize.Value;
                    }

                    return f;
                }
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
            if (!choosingComplexity && model.CalibrateMachineToProduceProbabilities)
            {
                updateStatus(new StatusArgs { StatusMessage = "Calibrating..." });
                Parallel.For(0, classifier.Machines.Length, i =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var machine = classifier.Machines[i];
                    var outputsForMachine = outputs.Apply(y => y == i ? 1 : -1);
                    var calibration = new ProbabilisticOutputCalibration(machine, inputs, outputsForMachine);
                    likelihood += calibration.Run() / inputs.Length;
                });
                updateStatus(new StatusArgs { StatusMessage = "Calibrated. Average log-likelihood: {0:N4}",
                    DoubleValues = new[] { likelihood / classifier.Machines.Length },
                    ReplaceLastStatus = true });
            }
            model.Classifier = classifier;
        }
    }
}
