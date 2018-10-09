using Accord.Math;
using Accord.Statistics;
using Accord.Statistics.Analysis;
using Extract;
using Extract.AttributeFinder;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using AccuracyData = Extract.Utilities.Union<Accord.Statistics.Analysis.GeneralConfusionMatrix, Accord.Statistics.Analysis.ConfusionMatrix>;

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
        #region Public Methods

        /// <summary>
        /// Optionally trains and then tests the machine using CSV data
        /// </summary>
        /// <param name="testOnly">Whether to only test, not train and test</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        /// <returns>Tuple of training set accuracy score and testing set accuracy score</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static (AccuracyData trainingSet, AccuracyData testingSet) TrainAndTestWithCsvData(
            this ILearningMachineModel model,
            bool testOnly,
            string csvOutputFileBaseName,
            Action<StatusArgs> updateStatus,
            CancellationToken cancellationToken)
        {
            try
            {
                var trainingCsv = csvOutputFileBaseName + ".train.csv";
                var testingCsv = csvOutputFileBaseName + ".test.csv";
                return TrainAndTestWithCsvData(model, testOnly, trainingCsv, testingCsv, updateStatus, cancellationToken);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45699");
            }
        }

        /// <summary>
        /// Optionally trains and then tests the machine using CSV data
        /// </summary>
        /// <param name="testOnly">Whether to only test, not train and test</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        /// <returns>Tuple of training set accuracy score and testing set accuracy score</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static (AccuracyData trainingSet, AccuracyData testingSet) TrainAndTestWithCsvData(
            this ILearningMachineModel model,
            bool testOnly,
            string trainingCsv,
            string testingCsv,
            Action<StatusArgs> updateStatus,
            CancellationToken cancellationToken)
        {
            try
            {
                ExtractException.Assert("ELI45517", "Machine is not fully configured", model.IsConfigured());
                ExtractException.Assert("ELI45697", "Machine is not trained", !testOnly || model.Classifier.IsTrained);

                (double[][] inputs, List<string> answers) GetDataFromCsv(string path)
                {
                    if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    {
                        return (new double[0][], new List<string>(0));
                    }

                    var row = 0;
                    List<double[]> features = new List<double[]>();
                    List<string> answers = new List<string>();
                    using (var csvReader = new Microsoft.VisualBasic.FileIO.TextFieldParser(path))
                    {
                        csvReader.Delimiters = new[] { "," };
                        csvReader.CommentTokens = new[] { "//", "#" };
                        while (!csvReader.EndOfData)
                        {
                            row++;
                            cancellationToken.ThrowIfCancellationRequested();

                            string[] fields;
                            fields = csvReader.ReadFields();

                            string answer = fields[2];
                            answers.Add(answer);

                            double[] featureVector = fields.Skip(3).Select(s => double.TryParse(s, out var d) ? d : 0).ToArray();
                            features.Add(featureVector);

                            updateStatus(new StatusArgs { StatusMessage = "Getting input data: {0:N0} records", Int32Value = 1 });
                        }
                    }

                    return (features.ToArray(), answers);
                }
                var (trainInputs, trainAnswers) = GetDataFromCsv(trainingCsv);
                var (testInputs, testAnswers) = GetDataFromCsv(testingCsv);

                // Check for suspiciously normalized feature vectors
                // using the largest of the sets
                var featureVectors = trainInputs.Length > testInputs.Length
                    ? trainInputs
                    : testInputs;
                double[] mean = featureVectors.Mean();
                double[] sigma = featureVectors.StandardDeviation(mean);
                if (mean.All(m => Math.Abs(m) < 0.1)
                    && sigma.All(s => s == 0 || Math.Abs(1 - s) < 0.5))
                {
                    updateStatus(new StatusArgs
                    {
                        StatusMessage = "WARNING: Feature vectors from \""
                        + (featureVectors == trainInputs
                            ? trainingCsv
                            : testingCsv)
                        + "\" appear to already be standardized"
                    });
                }

                // If training then init the answer codes otherwise they need to stay the same as when the machine was
                // trained.
                // https://extract.atlassian.net/browse/ISSUE-15275
                if (!testOnly)
                {
                    model.InitializeAnswerCodeMappings(trainAnswers.Concat(testAnswers));
                }

                var trainOutputs = trainAnswers.Select(a => model.AnswerNameToCode[a]).ToArray();
                var testOutputs = testAnswers.Select(a =>
                {
                    if (model.AnswerNameToCode.ContainsKey(a))
                    {
                        return model.AnswerNameToCode[a];
                    }
                    else
                    {
                        return 0;
                    }
                })
                .ToArray();

                // Train the classifier
                if (!testOnly && trainInputs.Any())
                {
                    var rng = new Random(0);
                    model.Classifier.TrainClassifier(trainInputs, trainOutputs, rng, updateStatus, cancellationToken);
                }

                cancellationToken.ThrowIfCancellationRequested();

                // Training mutates the trainInputs array (standardizes the values) in order to save memory
                // so if training has taken place (testOnly=false) don't standardize the inputs again 
                var trainResult = GetAccuracyScore(model, trainInputs, trainOutputs, standardizeInputs: testOnly);
                var testResult = GetAccuracyScore(model, testInputs, testOutputs, true);

                model.AccuracyData =
                    (train: new SerializableConfusionMatrix(model.Encoder, trainResult),
                    test: new SerializableConfusionMatrix(model.Encoder, testResult));

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
        /// <param name="model">The <see cref="ICLassifierModel"/> to use to compute answers</param>
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

                int[] predictions = null;
                switch (model)
                {
                    case INeuralNetModel nn:
                    {
                        (int answerCode, double score)[] predictionsAndScores = inputs.Apply(v => NeuralNetMethods.ComputeAnswer(nn, v));
                        predictions = predictionsAndScores.Select(t => t.answerCode).ToArray();
                        break;
                    }
                    case ISupportVectorMachineModel svm:
                    {
                        (int answerCode, double? score)[] predictionsAndScores = inputs.Apply(v => SvmMethods.ComputeAnswer(svm, v));
                        predictions = predictionsAndScores.Select(t => t.answerCode).ToArray();
                        break;
                    }
                    default:
                        throw new ArgumentException("Unknown IClassifierModel type");
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
                // Scale inputs
                if (standardizeInputs
                    && inputs.Any()
                    && model.Classifier.FeatureMean != null
                    && model.Classifier.FeatureScaleFactor != null)
                {
                    foreach (var v in inputs)
                    {
                        v.Subtract(model.Classifier.FeatureMean, inPlace: true);
                    }
                    inputs.ElementwiseDivide(model.Classifier.FeatureScaleFactor, inPlace: true);
                }

                int numberOfClasses = model.Classifier.NumberOfClasses;
                int[] predictions = null;
                (int answerCode, double? score)[] predictionsAndScores = new (int answerCode, double? score)[inputs.Length];
                switch (model.Classifier)
                {
                    case INeuralNetModel nn:
                    {
                        for (int i = 0; i < inputs.Length; i++)
                        {
                            predictionsAndScores[i] = NeuralNetMethods.ComputeAnswer(nn, inputs[i]);
                        }

                        break;
                    }
                    case ISupportVectorMachineModel svm:
                    {
                        for (int i = 0; i < inputs.Length; i++)
                        {
                            predictionsAndScores[i] = SvmMethods.ComputeAnswer(svm, inputs[i]);
                        }

                        break;
                    }

                    default:
                        throw new ArgumentException("Unknown IClassifierModel type");
                }

                if (model.UseUnknownCategory)
                {
                    bool unknownCategoryUsed = false;
                    predictions = predictionsAndScores.Select(t =>
                    {
                        if (t.score.HasValue
                            && t.score < model.UnknownCategoryCutoff)
                        {
                            if (model.TranslateUnknownCategory
                                && model.Encoder.AnswerNameToCode.TryGetValue(model.TranslateUnknownCategoryTo, out int answerCode))
                            {
                                return answerCode;
                            }

                            // Use value beyond any that the classifier would use for unknown
                            // rather than LearningMachineDataEncoder.UnknownCategoryCode to avoid
                            // misleading 100% accuracy results
                            // https://extract.atlassian.net/browse/ISSUE-13894
                            unknownCategoryUsed = true;
                            return model.Classifier.NumberOfClasses;
                        }
                        else
                        {
                            return t.answerCode;
                        }
                    })
                    .ToArray();

                    if (unknownCategoryUsed)
                    {
                        numberOfClasses++;
                    }
                }
                else
                {
                    predictions = predictionsAndScores.Select(t => t.answerCode).ToArray();
                }

                if (numberOfClasses == 2)
                {
                    var cm = new ConfusionMatrix(predictions, outputs);
                    return new AccuracyData(cm);
                }
                else
                {
                    var gc = new GeneralConfusionMatrix(model.Classifier.NumberOfClasses, outputs, predictions);
                    return new AccuracyData(gc);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45690");
            }
        }

        /// <summary>
        /// Standardizes feature values by subracting the mean and dividing by the standard deviation
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
        /// <param name="model">The <see cref="IClassifierModel"/> to perfom the computation</param>
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
        /// <param name="rng">An instance of <see cref="System.Random"/> to be used
        /// to generate the permutation. If <see langword="null"/> then a thread-local, static instance
        /// will be used.</param>
        private static void Shuffle<T>(T[] array, Random rng)
        {
            try
            {
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

        #endregion Private Methods
    }
}
