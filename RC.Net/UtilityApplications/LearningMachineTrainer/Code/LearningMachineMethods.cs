using Accord.Math;
using Accord.Statistics;
using Accord.Statistics.Analysis;
using Extract;
using Extract.AttributeFinder;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.VisualBasic.FileIO;
using AccuracyData = Extract.Utilities.Union<Accord.Statistics.Analysis.GeneralConfusionMatrix, Accord.Statistics.Analysis.ConfusionMatrix>;
using FormatException = System.FormatException;

namespace LearningMachineTrainer
{
    [CLSCompliant(false)]
    public interface ILearningMachineModel
    {
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        List<string> AnswerCodeToName { get; set; }

        Dictionary<string, int> AnswerNameToCode { get; }

        string NegativeClassName { get; }

        IClassifierModel Classifier { get; set; }

        ILearningMachineDataEncoderModel Encoder { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        (SerializableConfusionMatrix train, SerializableConfusionMatrix test)? AccuracyData { get; set; }

        string TrainingLog { get; set; }

        int RandomNumberSeed { get; }

        bool UseUnknownCategory { get; }

        double UnknownCategoryCutoff { get; }

        bool TranslateUnknownCategory { get; }

        string TranslateUnknownCategoryTo { get; }
    }

    public interface IClassifierModel
    {
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        double[] FeatureMean { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        double[] FeatureScaleFactor { get; set; }

        int NumberOfClasses { get; set; }

        bool IsTrained { get; set; }

        DateTime LastTrainedOn { get; set; }
    }

    public interface ILearningMachineDataEncoderModel
    {
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        List<string> AnswerCodeToName { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        Dictionary<string, int> AnswerNameToCode { get; set; }

        string NegativeClassName { get; set; }
    }

    [CLSCompliant(false)]
    public static class LearningMachineMethods
    {
        #region Constants

        // CSV loading/saving is done serially so avoid overhead of updating status every time through the loops
        public static readonly int UpdateFrequency = 64;

        #endregion Constants

        #region fields

        /// <summary>
        /// Thread static random number generator used by Shuffle methods
        /// </summary>
        private static readonly ThreadLocal<Random> _shuffleRandom = new ThreadLocal<Random>(() => new Random());

        #endregion fields

        #region Public Methods

        /// <summary>
        /// Optionally trains and then tests the machine using CSV data
        /// </summary>
        /// <param name="model">The <see cref="ILearningMachineModel"/> to train and test</param>
        /// <param name="testOnly">Whether to only test, not train and test</param>
        /// <param name="csvOutputFileBaseName">The base name of the CSVs containing training/testing data.
        /// ".test.csv" and ".train.csv" will be added to the basename.</param>
        /// <param name="updateCsvWithPredictions">Whether to rewrite the CSVs with prediction and probability columns</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        /// <returns>Tuple of training set accuracy score and testing set accuracy score</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static (AccuracyData trainingSet, AccuracyData testingSet) TrainAndTestWithCsvData(
            this ILearningMachineModel model,
            bool testOnly,
            string csvOutputFileBaseName,
            bool updateCsvWithPredictions,
            Action<StatusArgs> updateStatus,
            CancellationToken cancellationToken)
        {
            try
            {
                var trainingCsv = csvOutputFileBaseName + ".train.csv";
                var testingCsv = csvOutputFileBaseName + ".test.csv";
                return TrainAndTestWithCsvData(model, testOnly, trainingCsv, testingCsv, updateCsvWithPredictions, updateStatus, cancellationToken);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45699");
            }
        }

        /// <summary>
        /// Optionally trains and then tests the machine using CSV data
        /// </summary>
        /// <param name="model">The learning machine model to be trained/tested</param>
        /// <param name="testOnly">Whether to only test, not train and test</param>
        /// <param name="updateCsvWithPredictions">Whether to rewrite the CSVs with prediction and probability columns</param>
        /// <param name="trainingCsv">The path of the CSV containing training/testing data for the training set</param>
        /// <param name="testingCsv">The path of the CSV containing testing data for the testing set</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        /// <returns>Tuple of training set accuracy score and testing set accuracy score</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static (AccuracyData trainingSet, AccuracyData testingSet) TrainAndTestWithCsvData(
            this ILearningMachineModel model,
            bool testOnly,
            string trainingCsv,
            string testingCsv,
            bool updateCsvWithPredictions,
            Action<StatusArgs> updateStatus,
            CancellationToken cancellationToken)
        {
            try
            {
                ExtractException.Assert("ELI45517", "Machine is not fully configured", model.IsConfigured());
                ExtractException.Assert("ELI45697", "Machine is not trained", !testOnly || model.Classifier.IsTrained);

                ExtractException.Assert("ELI46422", "No input CSV files found", File.Exists(trainingCsv) || File.Exists(testingCsv));

                AccuracyData trainResult = null;
                AccuracyData testResult = null;

                // Indent sub-status messages
                Action<StatusArgs> updateStatus2 = args =>
                    {
                        args.Indent++;
                        updateStatus(args);
                    };
				// Do training set and testing set in separate blocks to avoid running out of memory
                {
                    var (trainInputs, trainAnswers) = GetDataFromCsv(trainingCsv, updateStatus, cancellationToken);

                    // Check for suspiciously normalized feature vectors
                    if (trainInputs.Any())
                    {
                        double[] mean = trainInputs.Mean();
                        double[] sigma = trainInputs.StandardDeviation(mean);
                        if (mean.All(m => Math.Abs(m) < 0.1)
                            && sigma.All(s => s == 0 || Math.Abs(1 - s) < 0.5))
                        {
                            updateStatus(new StatusArgs
                            {
                                StatusMessage = "WARNING: Feature vectors from \""
                                                + trainingCsv
                                                + "\" appear to already be standardized"
                            });
                        }

                        // If training then init the answer codes otherwise they need to stay the same as when the machine was
                        // trained.
                        // https://extract.atlassian.net/browse/ISSUE-15275
                        if (!testOnly)
                        {
                            model.InitializeAnswerCodeMappings(trainAnswers);
                        }

                        var trainOutputs = trainAnswers.Select(a =>
                                model.AnswerNameToCode.TryGetValue(a, out int code)
                                    ? code
                                    : 0)
                            .ToArray();

                        // Train the classifier
                        if (!testOnly && trainInputs.Any())
                        {
                            var rng = new Random(0);
                            model.Classifier.TrainClassifier(trainInputs, trainOutputs, rng, updateStatus,
                                cancellationToken);
                        }

                        cancellationToken.ThrowIfCancellationRequested();

                        updateStatus(new StatusArgs
                        {
                            StatusMessage = "Calculating accuracy: ..."
                        });

                        // Training mutates the trainInputs array (standardizes the values) in order to save memory
                        // so if training has taken place (testOnly=false) don't standardize the inputs again 
                        trainResult = GetAccuracyScore(model, trainInputs, trainOutputs, testOnly,
                            out (int code, double? score)[] trainPredictionsAndScores);

                        updateStatus(new StatusArgs
                        {
                            StatusMessage = "Calculating accuracy: Done",
                            ReplaceLastStatus = true
                        });

                        if (updateCsvWithPredictions)
                        {
                            updateStatus(new StatusArgs
                            {
                                StatusMessage = "Updating training CSV with predictions:"
                            });

                            UpdateCsv(model, trainingCsv, trainPredictionsAndScores, updateStatus2,
                                cancellationToken);
                        }
                    }
                }


                // Test set
                {
                    var (testInputs, testAnswers) = GetDataFromCsv(testingCsv, updateStatus, cancellationToken);

                    // Check for suspiciously normalized feature vectors
                    if (testInputs.Any())
                    {
                        double[] mean = testInputs.Mean();
                        double[] sigma = testInputs.StandardDeviation(mean);
                        if (mean.All(m => Math.Abs(m) < 0.1)
                            && sigma.All(s => s == 0 || Math.Abs(1 - s) < 0.5))
                        {
                            updateStatus(new StatusArgs
                            {
                                StatusMessage = "WARNING: Feature vectors from \""
                                                + testingCsv
                                                + "\" appear to already be standardized"
                            });
                        }

                        var testOutputs = testAnswers.Select(a =>
                                model.AnswerNameToCode.TryGetValue(a, out int code)
                                    ? code
                                    : 0)
                            .ToArray();

                        updateStatus(new StatusArgs
                        {
                            StatusMessage = "Calculating accuracy: ..."
                        });

                        testResult = GetAccuracyScore(model, testInputs, testOutputs, true,
                            out (int code, double? score)[] testPredictionsAndScores);

                        updateStatus(new StatusArgs
                        {
                            StatusMessage = "Calculating accuracy: Done",
                            ReplaceLastStatus = true
                        });

                        if (updateCsvWithPredictions)
                        {
                            updateStatus(new StatusArgs
                            {
                                StatusMessage = "Updating testing CSV with predictions:"
                            });

                            UpdateCsv(model, testingCsv, testPredictionsAndScores, updateStatus2,
                                cancellationToken);
                        }
                    }
                }

                model.AccuracyData =
                   (train: trainResult is AccuracyData trainData
                       ? new SerializableConfusionMatrix(model.Encoder, trainData)
                       : null,
                    test: testResult is AccuracyData testData
                        ? new SerializableConfusionMatrix(model.Encoder, testData)
                        : null);

                return (trainResult, testResult);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45533");
            }
        }

        /// <summary>
        /// Split data into two subset by computing the indexes of random subsets of each category. At least one example
        /// of each category will be represented in each subset so the subsets may overlap by one.
        /// </summary>
        /// <param name="categories">Category codes for each example in the set of data</param>
        /// <param name="subset1Fraction">The fraction of indexes to be selected for the first subset</param>
        /// <param name="subset1Indexes">The indexes selected for the first subset</param>
        /// <param name="subset2Indexes">The indexes selected for the second subset</param>
        /// <param name="randomGenerator">Optional random number generator used to select the subsets</param>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
        public static void GetIndexesOfSubsetsByCategory<TCategory>(TCategory[] categories, double subset1Fraction,
            out List<int> subset1Indexes, out List<int> subset2Indexes, Random randomGenerator=null)
            where TCategory : IComparable
        {
            try
            {
                ExtractException.Assert("ELI39761",
                    "Fraction must be between 0 and 1",
                    subset1Fraction <= 1 && subset1Fraction >= 0);

                subset1Indexes = new List<int>();
                subset2Indexes = new List<int>();
                foreach(var category in categories.Distinct())
                {
                    // Retrieve the indexes for this category
                    int[] idx = categories.Find(x => x.CompareTo(category) == 0);
                    if (idx.Length > 0)
                    {
                        int subset1Size = Math.Max((int)Math.Round(idx.Length * subset1Fraction), 1);
                        int subset2Size = Math.Max(idx.Length - subset1Size, 1);
                        Shuffle(idx, randomGenerator);
                        var subset1 = idx.Submatrix(0, subset1Size - 1);
                        var subset2 = idx.Submatrix(idx.Length - subset2Size, idx.Length - 1);
                        subset1Indexes.AddRange(subset1);
                        subset2Indexes.AddRange(subset2);
                    }
                }
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39738");
            }
        }

        /// <summary>
        /// Computes the accuracy or F1 score of the classifier
        /// </summary>
        /// <param name="model">The <see cref="IClassifierModel"/> to use to compute answers</param>
        /// <param name="inputs">The feature vectors</param>
        /// <param name="outputs">The expected results</param>
        /// <param name="standardizeInputs">Whether to zero-center and normalize the input (will mutate the input)</param>
        /// <param name="scoreType">The type of score to return</param>
        /// <returns>The specified score type if there are two classes else the overall agreement</returns>
        public static double GetAccuracyScore(IClassifierModel model, double[][] inputs, int[] outputs,
            bool standardizeInputs, MachineScoreType scoreType)
        {
            try
            {
                var result = GetAccuracyScore(model, inputs, outputs, standardizeInputs);
                return result.Match(
                    gc => gc.OverallAgreement,
                    cm =>
                    {
                        switch (scoreType)
                        {
                            case MachineScoreType.Precision:
                                return Double.IsNaN(cm.Precision) ? 0.0 : cm.Precision;
                            case MachineScoreType.Recall:
                                return Double.IsNaN(cm.Recall) ? 0.0 : cm.Recall;
                            default:
                                return Double.IsNaN(cm.FScore) ? 0.0 : cm.FScore;
                        }
                    });
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45691");
            }
        }

        /// <summary>
        /// Computes predictions and, optionally, probability scores for the inputs
        /// </summary>
        /// <param name="model">The <see cref="IClassifierModel"/> to use to compute answers</param>
        /// <param name="inputs">The feature vectors</param>
        /// <param name="standardizeInputs">Whether to zero-center and normalize the input (will mutate the input)</param>
        public static (int code, double? score)[] GetPredictions(IClassifierModel model, double[][] inputs, bool standardizeInputs)
        {
            try
            {
                // Scale inputs
                if (standardizeInputs
                    && inputs.Any()
                    && model.FeatureMean != null
                    && model.FeatureScaleFactor != null)
                {
                    foreach (var v in inputs)
                    {
                        v.Subtract(model.FeatureMean, inPlace: true);
                    }
                    inputs.ElementwiseDivide(model.FeatureScaleFactor, inPlace: true);
                }

                switch (model)
                {
                    case INeuralNetModel nn:
                    {
                        return inputs.Apply(v => NeuralNetMethods.ComputeAnswer(nn, v));
                    }
                    case ISupportVectorMachineModel svm:
                    {
                        return inputs.Apply(v => SvmMethods.ComputeAnswer(svm, v));
                    }
                    default:
                        throw new ArgumentException("Unknown IClassifierModel type");
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46329");
            }
        }

        /// <summary>
        /// Computes a confusion matrix for the given inputs and outputs
        /// </summary>
        /// <remarks>This overload doesn't use an unknown category cutoff</remarks>
        /// <param name="model">The <see cref="IClassifierModel"/> to use to compute answers</param>
        /// <param name="inputs">The feature vectors</param>
        /// <param name="outputs">The expected results</param>
        /// <param name="standardizeInputs">Whether to zero-center and normalize the input (will mutate the input)</param>
        public static AccuracyData GetAccuracyScore(IClassifierModel model, double[][] inputs, int[] outputs,
            bool standardizeInputs)
        {
            try
            {
                var predictionsAndScores = GetPredictions(model, inputs, standardizeInputs);
                int[] predictions = new int[predictionsAndScores.Length];
                for (int i = 0; i < predictions.Length; i++)
                {
                    predictions[i] = predictionsAndScores[i].code;
                }

                if (model.NumberOfClasses == 2)
                {
                    var cm = new ConfusionMatrix(predictions, outputs);
                    return new AccuracyData(cm);
                }
                else
                {
                    var gc = new GeneralConfusionMatrix(model.NumberOfClasses, outputs, predictions);
                    return new AccuracyData(gc);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45690");
            }
        }

        /// <summary>
        /// Computes a confusion matrix for the given inputs and outputs
        /// </summary>
        /// <remarks>This overload will use the unknown category cutoff if specified in the model</remarks>
        /// <param name="model">The <see cref="ILearningMachineModel"/> to use to compute answers</param>
        /// <param name="inputs">The feature vectors</param>
        /// <param name="outputs">The expected results</param>
        /// <param name="standardizeInputs">Whether to zero-center and normalize the input (will mutate the input)</param>
        public static AccuracyData GetAccuracyScore(ILearningMachineModel model, double[][] inputs, int[] outputs,
            bool standardizeInputs)
        {
            try
            {
                return GetAccuracyScore(model, inputs, outputs, standardizeInputs, out _);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45690");
            }
        }

        /// <summary>
        /// Computes a confusion matrix for the given inputs and outputs
        /// </summary>
        /// <remarks>This overload will use the unknown category cutoff if specified in the model</remarks>
        /// <param name="model">The <see cref="ILearningMachineModel"/> to use to compute answers</param>
        /// <param name="inputs">The feature vectors</param>
        /// <param name="outputs">The expected results</param>
        /// <param name="standardizeInputs">Whether to zero-center and normalize the input (will mutate the input)</param>
        /// <param name="predictionsAndScores">The predicted labels and confidence score for each input</param>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
        public static AccuracyData GetAccuracyScore(ILearningMachineModel model, double[][] inputs, int[] outputs,
            bool standardizeInputs, out (int code, double? score)[] predictionsAndScores)
        {
            try
            {
                predictionsAndScores = GetPredictions(model.Classifier, inputs, standardizeInputs);

                return GetAccuracyScore(model, predictionsAndScores, outputs);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46321");
            }
        }

        /// <summary>
        /// Computes a confusion matrix for the given inputs and outputs
        /// </summary>
        /// <remarks>This overload will use the unknown category cutoff if specified in the model</remarks>
        /// <param name="model">The <see cref="ILearningMachineModel"/> to use to compute answers</param>
        /// <param name="predictionsAndScores">The predicted class and confidence score for each example</param>
        /// <param name="outputs">The expected class for each example</param>
        public static AccuracyData GetAccuracyScore(ILearningMachineModel model, (int code, double? score)[] predictionsAndScores, int[] outputs)
        {
            try
            {
                int numberOfClasses = model.Classifier.NumberOfClasses;
                int[] predictions = new int[predictionsAndScores.Length];
                if (model.UseUnknownCategory)
                {
                    bool unknownCategoryUsed = false;
                    for (int i = 0; i < predictions.Length; i++)
                    {
                        var (code, score) = predictionsAndScores[i];
                        if (score.HasValue
                            && score < model.UnknownCategoryCutoff)
                        {
                            if (model.TranslateUnknownCategory
                                && model.Encoder.AnswerNameToCode.TryGetValue(model.TranslateUnknownCategoryTo, out int answerCode))
                            {
                                predictions[i] = answerCode;
                            }
                            else
                            {
                                // Use value beyond any that the classifier would use for unknown
                                // rather than LearningMachineDataEncoder.UnknownCategoryCode to avoid
                                // misleading 100% accuracy results
                                // https://extract.atlassian.net/browse/ISSUE-13894
                                unknownCategoryUsed = true;
                                predictions[i] = model.Classifier.NumberOfClasses;
                            }
                        }
                        else
                        {
                            predictions[i] = code;
                        }
                    }

                    if (unknownCategoryUsed)
                    {
                        numberOfClasses++;
                    }
                }
                else
                {
                    for (int i = 0; i < predictions.Length; i++)
                    {
                        predictions[i] = predictionsAndScores[i].code;
                    }
                }

                if (numberOfClasses == 2)
                {
                    var cm = new ConfusionMatrix(predictions, outputs);
                    return new AccuracyData(cm);
                }
                else
                {
                    var gc = new GeneralConfusionMatrix(numberOfClasses, outputs, predictions);
                    return new AccuracyData(gc);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46320");
            }
        }

        /// <summary>
        /// Standardizes feature values by subtracting the mean and dividing by the standard deviation
        /// </summary>
        /// <param name="featureVectors">Feature vectors for the training data</param>
        /// <returns>The calculated mean and standard deviation</returns>
        public static (double[] mean, double[] sigma) Standardize(this double[][] featureVectors)
        {
            try
            {
                var mean = featureVectors.Mean();
                var sigma = featureVectors.StandardDeviation(mean);

                // Prevent divide by zero
                if (sigma.Any(factor => factor == 0))
                {
                    sigma.ApplyInPlace(factor => factor + 0.0001);
                }

                // Standardize input
                foreach (var v in featureVectors)
                {
                    v.Subtract(mean, inPlace: true);
                }
                featureVectors.ElementwiseDivide(sigma, inPlace: true);

                return (mean, sigma);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45536");
            }
        }

        /// <summary>
        /// Computes answer code and score for the input feature vector
        /// </summary>
        /// <param name="model">The <see cref="IClassifierModel"/> to perform the computation</param>
        /// <param name="inputs">The feature vector</param>
        /// <param name="standardizeInputs">Whether to apply zero-center and normalize the input</param>
        /// <returns>The answer code and score</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static (int answerCode, double? score) ComputeAnswer(this IClassifierModel model, double[] inputs, bool standardizeInputs = true)
        {
            try
            {
                // Scale inputs
                if (standardizeInputs
                    && model.FeatureMean != null
                    && model.FeatureScaleFactor != null)
                {
                    inputs = inputs.Subtract(model.FeatureMean).ElementwiseDivide(model.FeatureScaleFactor);
                }

                switch (model)
                {
                    case INeuralNetModel nn:
                        return NeuralNetMethods.ComputeAnswer(nn, inputs);
                    case ISupportVectorMachineModel svm:
                        return SvmMethods.ComputeAnswer(svm, inputs);
                    default:
                        throw new ArgumentException("Unknown IClassifierModel type");
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46191");
            }
        }

        /// <summary>
        /// Trains the classifier to recognize classifications
        /// </summary>
        /// <param name="model">The <see cref="IClassifierModel"/> to be trained</param>
        /// <param name="inputs">The input feature vectors</param>
        /// <param name="outputs">The classes for each input</param>
        /// <param name="randomGenerator">Optional random number generator to use for randomness</param>
        public static void TrainClassifier(this IClassifierModel model, double[][] inputs, int[] outputs, Random randomGenerator = null)
        {
            try
            {
                TrainClassifier(model, inputs, outputs, randomGenerator, _ => { }, CancellationToken.None);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46193");
            }
        }

        /// <summary>
        /// Trains the classifier to recognize classifications
        /// </summary>
        /// <param name="model">The <see cref="IClassifierModel"/> to be trained</param>
        /// <param name="inputs">The input feature vectors</param>
        /// <param name="outputs">The classes for each input</param>
        /// <param name="randomGenerator">Random number generator to use for randomness</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        public static void TrainClassifier(this IClassifierModel model, double[][] inputs, int[] outputs,
            Random randomGenerator, Action<StatusArgs> updateStatus, CancellationToken cancellationToken)
        {
            try
            {
                switch (model)
                {
                    case INeuralNetModel nn:
                        NeuralNetMethods.TrainClassifier(nn, inputs, outputs, randomGenerator, updateStatus, cancellationToken);
                        break;
                    case ISupportVectorMachineModel svm:
                        SvmMethods.TrainClassifier(svm, inputs, outputs, randomGenerator, updateStatus, cancellationToken);
                        break;
                    default:
                        throw new ArgumentException("Unknown model type");
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46194");
            }
        }

        #endregion Public Methods

        #region Private Methods

        private static void InitializeAnswerCodeMappings(this ILearningMachineModel model, IEnumerable<string> answers)
        {
            if (model.NegativeClassName == null)
            {
                throw new ArgumentException("NegativeClassName not set!");
            }

            model.AnswerCodeToName = new List<string> { model.NegativeClassName };
            model.AnswerCodeToName.AddRange(answers.Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(s => !s.Equals(model.NegativeClassName, StringComparison.OrdinalIgnoreCase))
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase));

            model.AnswerNameToCode.Clear();
            for (int i = 0; i < model.AnswerCodeToName.Count; i++)
            {
                model.AnswerNameToCode[model.AnswerCodeToName[i]] = i;
            }
        }

        /// <summary>
        /// Knuth shuffle (a.k.a. the Fisher-Yates shuffle) for arrays.
        /// Performs an in-place random permutation of an array.
        /// Adapted from https://www.rosettacode.org/wiki/Knuth_shuffle#C.23
        /// </summary>
        /// <typeparam name="T">The type of the objects in the array</typeparam>
        /// <param name="array">The array to shuffle</param>
        /// <param name="randomNumberGenerator">An instance of <see cref="System.Random"/> to be used
        /// to generate the permutation. If <see langword="null"/> then a thread-local, static instance
        /// will be used.</param>
        private static void Shuffle<T>(T[] array, Random randomNumberGenerator)
        {
            try
            {
                var rng = randomNumberGenerator ?? _shuffleRandom.Value;

                int length = array.Length;

                for (int i = 0; i < length - 1; i++)
                {
                    // Don't select from the entire array length on subsequent loops or the result will be biased
                    int j = rng.Next(i, length);
                    T temp = array[j];
                    array[j] = array[i];
                    array[i] = temp;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39499");
            }
        }

        private static bool IsConfigured(this ILearningMachineModel model)
        {
            return model.Encoder != null && model.Classifier != null;
        }

        /// <summary>
        /// Loads feature vectors and answers from a CSV file
        /// </summary>
        /// <param name="path">The path to the CSV file</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        /// <returns>Tuple of feature vector array and answer array</returns>
        private static (double[][] inputs, List<string> answers) GetDataFromCsv(string path,
            Action<StatusArgs> updateStatus,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return (new double[0][], new List<string>(0));
            }

            List<double[]> features = new List<double[]>();
            List<string> answers = new List<string>();
            using (var csvReader = new TextFieldParser(path))
            {
                csvReader.Delimiters = new[] { "," };
                csvReader.CommentTokens = new[] { "//", "#" };
                int answerIndex = 2;
                bool hasHeader = true;

                // Check for header row
                if (!csvReader.EndOfData)
                {
                    var fields = csvReader.ReadFields();
                    var minRowLength = fields.Length;
                    var featureFields = new HashSet<int>(Enumerable.Range(0, fields.Length));
                    var otherHeaderPattern = new Regex(@"\A[A-Z][a-z]+\z");
                    var featureHeaderPattern = new Regex(@"\Af[\dA-F]+\z");
                    for (int i = 0; i < fields.Length; i++)
                    {
                        var f = fields[i];
                        if (otherHeaderPattern.IsMatch(f))
                        {
                            featureFields.Remove(i);
                        }
                        else if (!featureHeaderPattern.IsMatch(f))
                        {
                            hasHeader = false;
                            break;
                        }
                    }

                    if (hasHeader)
                    {
                        // Variations in column presence and order are accepted if header row is present
                        // Answer header is required, features need to be named f[\dA-F]+, all headers must
                        // be distinct from the others.
                        var fieldSet = new HashSet<string>(fields);

                        if (!fieldSet.Contains("Answer"))
                        {
                            throw new FormatException("CSV header is invalid, must contain an 'Answer' column");
                        }

                        if (fieldSet.Count != fields.Length)
                        {
                            throw new FormatException("CSV header is invalid, must not contain duplicate values");
                        }

                        answerIndex = fields.IndexOf("Answer");

                        if (!csvReader.EndOfData)
                        {
                            fields = csvReader.ReadFields();
                        }
                        else
                        {
                            throw new FormatException("No CSV data records");
                        }
                    }

                    do
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (fields.Length < minRowLength)
                        {
                            throw new FormatException("CSV row doesn't have sufficient fields. Row " +
                                                (answers.Count + 1).ToString(CultureInfo.InvariantCulture));
                        }

                        string answer = fields[answerIndex];
                        answers.Add(answer);

                        var featureSource = hasHeader
                            ? fields.Where((s, i) => featureFields.Contains(i))
                            : fields.Skip(3);
                        double[] featureVector = featureSource
                            .Select(s => double.TryParse(s, out var d) ? d : 0).ToArray();
                        features.Add(featureVector);

                        if (csvReader.EndOfData || answers.Count % UpdateFrequency == 0)
                        {
                            updateStatus(new StatusArgs
                            {
                                StatusMessage = "Getting input data: {0:N0} records",
                                Int32Value = csvReader.EndOfData
                                  ? answers.Count % UpdateFrequency
                                  : UpdateFrequency
                            });
                        }

                    } while (!csvReader.EndOfData && (fields = csvReader.ReadFields()) != null);
                }
            }

            return (features.ToArray(), answers);
        }

        private static void UpdateCsv(ILearningMachineModel model, string path, (int code, double? score)[] predictionsAndScores,
            Action<StatusArgs> updateStatus,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return;
            }

            string tempFile = null;

            try
            {
                var commentPattern = new Regex(@"(?nx)\A\s*(\#|//).*");

                // Save to a temporary file
                tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                using (var outStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var streamWriter = new StreamWriter(outStream))
                {
                    using (var csvReader = new TextFieldParser(path))
                    {
                        csvReader.Delimiters = new[] { "," };
                        csvReader.CommentTokens = new[] { "//", "#" };
                    
                        int answerIndex = 2;
                        int predictionIndex = -1;
                        int probabilityIndex = -1;
                        bool hasHeader = true;
                        int dataIndex = 0;

                        // Copy any comments to output
                        while (!csvReader.EndOfData
                               && commentPattern.IsMatch(csvReader.PeekChars(100)))
                        {
                            streamWriter.WriteLine(csvReader.ReadLine());
                        }

                        // Check for header row
                        if (!csvReader.EndOfData)
                        {
                            string[] fields = csvReader.ReadFields();
                            var minRowLength = fields.Length;
                            var headerPattern = new Regex(@"(?nx)\A( [A-Z][a-z]+ | f[\dA-F]+ )\z");
                            if (fields.Any(f => !headerPattern.IsMatch(f)))
                            {
                                hasHeader = false;
                            }

                            var header = new List<string>(fields);
                            if (hasHeader)
                            {
                                // Variations in column presence and order are accepted if header row is present
                                // Answer header is required, features need to be named f\d+, all headers must
                                // be distinct from the others.
                                var fieldSet = new HashSet<string>(fields);

                                if (!fieldSet.Contains("Answer"))
                                {
                                    throw new FormatException("CSV header is invalid, must contain an 'Answer' column");
                                }

                                if (fieldSet.Count != fields.Length)
                                {
                                    throw new FormatException("CSV header is invalid, must not contain duplicate values");
                                }

                                answerIndex = fields.IndexOf("Answer");

                                if (fieldSet.Contains("Probability"))
                                {
                                    probabilityIndex = fields.IndexOf("Probability");
                                }
                                else
                                {
                                    header.Insert(answerIndex + 1, "Probability");
                                }
                                if (fieldSet.Contains("Prediction"))
                                {
                                    predictionIndex = fields.IndexOf("Prediction");
                                }
                                else
                                {
                                    header.Insert(answerIndex + 1, "Prediction");
                                }
                            }
                            else
                            {
                                header = new List<string>
                                    {"Path", "Index", "Answer", "Prediction", "Probability"};
                                for (int i = 0; i < minRowLength - 3; i++)
                                {
                                    header.Add(string.Format(CultureInfo.InvariantCulture, "f{0:X4}", i));
                                }
                            }

                            // Write header line
                            streamWriter.WriteLine(string.Join(",", header.Select(s => s.QuoteIfNeeded("\"", ","))));

                            if (hasHeader)
                            {
                                if (!csvReader.EndOfData)
                                {
                                    // Copy any comments to output
                                    while (!csvReader.EndOfData
                                           && commentPattern.IsMatch(csvReader.PeekChars(100)))
                                    {
                                        streamWriter.WriteLine(csvReader.ReadLine());
                                    }

                                    fields = csvReader.ReadFields();
                                }
                                else
                                {
                                    throw new FormatException("No CSV data records");
                                }
                            }

                            do
                            {
                                cancellationToken.ThrowIfCancellationRequested();

                                if (dataIndex == predictionsAndScores.Length)
                                {
                                    throw new FormatException("Not enough data rows to update the CSV. CSV path: '" + path + "'");
                                }

                                if (fields.Length < minRowLength)
                                {
                                    throw new FormatException("CSV row doesn't have sufficient fields. Row " +
                                                              (dataIndex + 1).ToString(CultureInfo.InvariantCulture));
                                }

                                var row = new List<string>(fields);
                                var code = predictionsAndScores[dataIndex].code;
                                var predictedClass = model.Encoder.AnswerCodeToName[code];
                                var probString = string.Format(CultureInfo.InvariantCulture, "{0:F4}", predictionsAndScores[dataIndex].score ?? 1);
                                if (probabilityIndex == -1)
                                {
                                    row.Insert(answerIndex + 1, probString);
                                }
                                else
                                {
                                    row[probabilityIndex] = probString;
                                }
                                if (predictionIndex == -1)
                                {
                                    row.Insert(answerIndex + 1, predictedClass);
                                }
                                else
                                {
                                    row[predictionIndex] = predictedClass;
                                }

                                streamWriter.WriteLine(string.Join(",", row.Select(s => s.QuoteIfNeeded("\"", ","))));
                                dataIndex++;

                                // Copy any comments to output
                                while (!csvReader.EndOfData
                                       && commentPattern.IsMatch(csvReader.PeekChars(100)))
                                {
                                    streamWriter.WriteLine(csvReader.ReadLine());
                                }

                                if (csvReader.EndOfData || dataIndex % UpdateFrequency == 0)
                                {
                                    updateStatus(new StatusArgs
                                    {
                                        StatusMessage = "Updating CSV: {0:N0} records",
                                        Int32Value = csvReader.EndOfData
                                            ? dataIndex % UpdateFrequency
                                            : UpdateFrequency
                                    });
                                }

                            } while (!csvReader.EndOfData && (fields = csvReader.ReadFields()) != null);
                        }

                        streamWriter.Flush();
                    }
                }

                File.Copy(tempFile, path, true);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39809");
            }
            finally
            {
                if (tempFile != null)
                {
                    File.Delete(tempFile);
                }
            }
        }

        #endregion Private Methods
    }
}
