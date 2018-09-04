using Accord.Math;
using Accord.Neuro;
using AForge.Neuro;
using Extract;
using Extract.AttributeFinder;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace LearningMachineTrainer
{
    [CLSCompliant(false)]
    public interface INeuralNetModel : IClassifierModel
    {
        int FeatureVectorLength { get; set; }

        IEnumerable<int> HiddenLayers { get; set; }

        int MaxTrainingIterations { get; set; }

        int NumberOfCandidateNetworksToBuild { get; set; }

        double SigmoidAlpha { get; set; }

        bool UseCrossValidationSets { get; set; }

        ActivationNetwork Classifier { get; set; }

        double? NegativeToPositiveWeightRatio { get; set; }
    }

    [CLSCompliant(false)]
    public static class NeuralNetMethods
    {
        private static readonly int _WINDOW_SIZE = 20;

        public static int ComputeAnswer(INeuralNetModel model, double[] inputs)
        {
            double[] responses = model.Classifier.Compute(inputs);

            // Return index of highest value neuron in the output layer
            responses.Max(out int imax);

            return imax;
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
        public static void TrainClassifier(INeuralNetModel model, double[][] inputs, int[] outputs,
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

                Action<StatusArgs> updateStatus3 = args =>
                    {
                        args.Indent++;
                        updateStatus2(args);
                    };

                // If a random number generator was specified, then specify the random number generator
                // for neuron initialization so that results are reproducible
                if (randomGenerator != null)
                {
                    Neuron.RandGenerator = new AForge.ThreadSafeRandom(randomGenerator.Next());
                }

                model.FeatureVectorLength = inputs[0].Length;
                model.NumberOfClasses = outputs.Max() + 1;

                // Expand output into one-hot vectors
                double[][] expandedOutputs = Accord.Statistics.Tools.Expand(outputs, model.NumberOfClasses, negative: -1.0, positive: 1.0);

                // Apply weight ratio if specified
                if (model.NegativeToPositiveWeightRatio is double ratio)
                {
                    for (int i = 0; i < expandedOutputs.Length; i++)
                    {
                        var val = expandedOutputs[i][0];
                        if (val > 0)
                        {
                            expandedOutputs[i][0] = val * ratio;
                        }
                    }
                }

                int[] layers = model.HiddenLayers.Concat(new int[] { model.NumberOfClasses }).ToArray();

                // Run training algorithm
                if (!model.UseCrossValidationSets)
                {
                    updateStatus(new StatusArgs { StatusMessage = "Training classifier:" });
                    model.Classifier = TrainClassifier(model, inputs, expandedOutputs, layers,
                        updateStatus2, cancellationToken);
                }
                else
                {
                    updateStatus(new StatusArgs { StatusMessage = "Building classifier:" });

                    int numberOfNetworks = Math.Max(1, model.NumberOfCandidateNetworksToBuild);
                    double lowestError = double.MaxValue;
                    for (int i = 0; i < numberOfNetworks; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        updateStatus2(new StatusArgs
                        {
                            TaskName = "TrainCandidateNet",
                            ReplaceLastStatus = true,
                            StatusMessage = string.Format(CultureInfo.CurrentCulture,
                                "Training candidate classifier {0}:", i + 1)
                        });

                        // Split data into training and validation sets by getting random subsets of each
                        // category. This is to ensure at least one example of each class exists.
                        // Compute indexes for the two sets of data
                        LearningMachineMethods.GetIndexesOfSubsetsByCategory(outputs, 0.8, out List<int> trainIdx, out List<int> cvIdx, randomGenerator);

                        double[][] trainInputs = inputs.Submatrix(trainIdx);
                        double[][] trainOutputs = expandedOutputs.Submatrix(trainIdx);

                        double[][] cvInputs = inputs.Submatrix(cvIdx);
                        double[][] cvOutputs = expandedOutputs.Submatrix(cvIdx);

                        // Train the classifier
                        ActivationNetwork ann;
                        double cvError = TrainClassifier(model, trainInputs, trainOutputs, cvInputs, cvOutputs, layers, out ann,
                            updateStatus3, cancellationToken);
                        if (cvError < lowestError)
                        {
                            model.Classifier = ann;
                        }
                    }
                }

                model.IsTrained = true;
                model.LastTrainedOn = DateTime.Now;
            }
            catch (OutOfMemoryException)
            {
                updateStatus(new StatusArgs
                {
                    TaskName = "TrainingClassifier",
                    StatusMessage = "Error: Out of memory"
                });
                throw;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39722");
            }

        }

        /// <summary>
        /// Trains a classifier by running the training algorithm
        /// </summary>
        /// <param name="trainInputs">Feature vectors to train with</param>
        /// <param name="trainOutputs">Classes (category codes) for each training input</param>
        /// <param name="layers">Sizes of hidden and output layers</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        /// <returns>The resulting network</returns>
        private static ActivationNetwork TrainClassifier(INeuralNetModel model, double[][] inputs, double[][] expandedOutputs, int[] layers,
            Action<StatusArgs> updateStatus, CancellationToken cancellationToken)
        {
            var ann = new ActivationNetwork(new BipolarSigmoidFunction(model.SigmoidAlpha), model.FeatureVectorLength, layers);
            var initializer = new NguyenWidrow(ann);
            initializer.Randomize();
            var teacher = new Accord.Neuro.Learning.ParallelResilientBackpropagationLearning(ann);
            int sampleSize = expandedOutputs.Length;
            for (int i = 1; i <= model.MaxTrainingIterations; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                double error = teacher.RunEpoch(inputs, expandedOutputs) / sampleSize;
                updateStatus(new StatusArgs
                {
                    TaskName = "RunEpoch",
                    ReplaceLastStatus = true,
                    StatusMessage = string.Format(CultureInfo.CurrentCulture,
                        "Training iteration: {0} Training error: {1:N4}", i, error)
                });
            }

            return ann;
        }

        /// <summary>
        /// Trains a classifier using a cross-validation set to stop before <see cref="MaxTrainingIterations"/> are reached.
        /// Assumes that <see cref="MaxTrainingIterations"/> is at least <see cref="_WINDOW_SIZE"/>
        /// </summary>
        /// <param name="trainInputs">Feature vectors to train with</param>
        /// <param name="trainOutputs">Classes (category codes) for each training input</param>
        /// <param name="cvInputs">Cross-validation set; feature vectors to check training progress against</param>
        /// <param name="cvOutputs">Array of classes (category codes) for each cross-validation input</param>
        /// <param name="layers">Sizes of hidden and output layers</param>
        /// <param name="trainedNetwork">The resulting network</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        /// <returns>The cross-validation error value of the resulting network</returns>
        private static double TrainClassifier(INeuralNetModel model, double[][] trainInputs, double[][] trainOutputs,
            double[][] cvInputs, double[][] cvOutputs, int[] layers, out ActivationNetwork trainedNetwork,
            Action<StatusArgs> updateStatus, CancellationToken cancellationToken)
        {
            var ann = new ActivationNetwork(new BipolarSigmoidFunction(model.SigmoidAlpha), model.FeatureVectorLength, layers);
            var initializer = new NguyenWidrow(ann);
            initializer.Randomize();
            var teacher = new Accord.Neuro.Learning.ParallelResilientBackpropagationLearning(ann);

            var history = new Queue<Tuple<string, double>>(_WINDOW_SIZE);
            List<string> files = null;
            int trainSize = trainOutputs.Length;
            int cvSize = cvOutputs.Length;

            string tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempFolder);
            try
            {
                for (int i = 1; i <= model.MaxTrainingIterations; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    double trainError = teacher.RunEpoch(trainInputs, trainOutputs) / trainSize; ;
                    double cvError = teacher.ComputeError(cvInputs, cvOutputs) / cvSize;
                    updateStatus(new StatusArgs
                    {
                        TaskName = "RunEpoch",
                        ReplaceLastStatus = true,
                        StatusMessage = string.Format(CultureInfo.CurrentCulture,
                            "Training iteration: {0} Training error: {1:N4} Validation error: {2:N4}", i, trainError, cvError)
                    });

                    string savedNN = Path.Combine(tempFolder, Path.GetRandomFileName());
                    ann.Save(savedNN);
                    history.Enqueue(Tuple.Create(savedNN, cvError));

                    if (i >= _WINDOW_SIZE)
                    {
                        // Stop training if the results are not changing
                        // https://extract.atlassian.net/browse/ISSUE-14873
                        if (history.All(t => t.Item2 == history.Peek().Item2))
                        {
                            break;
                        }

                        var avgCVLast = history.Skip(_WINDOW_SIZE / 2).Average(t => t.Item2);
                        var avgCVPrevLast = history.Take(_WINDOW_SIZE / 2).Average(t => t.Item2);

                        // Break if CV error is trending upward
                        if (avgCVLast > avgCVPrevLast)
                        {
                            break;
                        }

                        // Throw away oldest saved NN
                        var (file, _) = history.Dequeue();
                        File.Delete(file);
                    }
                }

                // Retrieve the best NN
                files = history.Select(t => t.Item1).ToList();
                var errors = history.Select(t => t.Item2).ToArray();
                double lowestError = errors.Min(out int iMin);
                var bestFile = files[iMin];

                trainedNetwork = (ActivationNetwork)Network.Load(bestFile);
                return lowestError;
            }
            finally
            {
                try
                {
                    Directory.Delete(tempFolder, true);
                }
                catch { }
            }
        }
    }
}