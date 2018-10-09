using LearningMachineTrainer;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AccuracyData = Extract.Utilities.Union<Accord.Statistics.Analysis.GeneralConfusionMatrix, Accord.Statistics.Analysis.ConfusionMatrix>;

namespace Extract.AttributeFinder
{
    [CLSCompliant(false)]
    public static class SharedTrainingMethods
    {
        /// <summary>
        /// Handles cleanup, checking for cancellation request and exceptions
        /// </summary>
        /// <param name="learningMachine"></param>
        /// <param name="task"></param>
        /// <param name="stopwatch"></param>
        /// <param name="statusUpdates"></param>
        /// <param name="cancellationToken"></param>
        /// <returns><c>true</c> if main task was successful, <c>false</c> if there was an exception or if cancellation was requested</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Exception RunCleanup(
            Task<(AccuracyData, AccuracyData)> task,
            ILearningMachineModel learningMachine,
            bool testOnly,
            Stopwatch stopwatch,
            ConcurrentQueue<StatusArgs> statusUpdates,
            CancellationToken cancellationToken)
        {
            try
            {
                Exception exception = null;
                stopwatch.Stop();
                var elapsedTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss", CultureInfo.CurrentCulture);

                if (cancellationToken.IsCancellationRequested)
                {
                    statusUpdates.Enqueue(
                        new StatusArgs
                        {
                            StatusMessage = "Canceled. Time elapsed: " + elapsedTime,
                            ReplaceLastStatus = true
                        });
                }
                else if (task.Exception == null)
                {
                    var completedMessage = testOnly ? "Testing Complete" : "Training Complete";
                    statusUpdates.Enqueue(
                        new StatusArgs { StatusMessage = completedMessage + ". Time elapsed: " + elapsedTime + "\r\n" });

                    void writeAccuracyData(AccuracyData accuracyData, SerializableConfusionMatrix confusionMatrix) => accuracyData.Match(
                        gcm =>
                        {
                            statusUpdates.Enqueue(new StatusArgs
                            {
                                StatusMessage = "  Number of samples: {0:N0}",
                                Int32Value = gcm.Samples
                            });
                            statusUpdates.Enqueue(new StatusArgs
                            {
                                StatusMessage = "  Overall agreement: {0:N4}\r\n  Chance agreement: {1:N4}",
                                DoubleValues = new[] { gcm.OverallAgreement, gcm.ChanceAgreement }
                            });
                            if (confusionMatrix != null)
                            {
                                var negativeClasses = string.Join(", ", confusionMatrix.NegativeClassIndexes().Select(i => confusionMatrix.Labels[i]));
                                statusUpdates.Enqueue(new StatusArgs
                                {
                                    StatusMessage = "  F1 Score (micro avg): {0:N4}" +
                                        "\r\n  Precision (micro avg): {1:N4}" +
                                        "\r\n  Recall (micro avg): {2:N4}" +
                                        "\r\n  Negative class: " + negativeClasses,
                                    DoubleValues = new[] { confusionMatrix.FScoreMicroAverage(), confusionMatrix.PrecisionMicroAverage(), confusionMatrix.RecallMicroAverage() }
                                });
                            }
                        },
                        cm =>
                        {
                            var positiveCategoryCodes = learningMachine.Encoder.AnswerCodeToName.Count - 1;
                            ExtractException.Assert("ELI41410", "Internal logic exception: There should be exactly one positive category in order to use a confusion matrix",
                                positiveCategoryCodes == 1);
                            string positiveCategory = learningMachine.Encoder.AnswerCodeToName[positiveCategoryCodes];

                            statusUpdates.Enqueue(new StatusArgs
                            {
                                StatusMessage = "  Number of samples: {0:N0}",
                                Int32Value = cm.Samples
                            });
                            statusUpdates.Enqueue(new StatusArgs
                            {
                                StatusMessage = "  F1 Score: {0:N4}" +
                                    "\r\n  Precision: {1:N4}" +
                                    "\r\n  Recall: {2:N4}" +
                                    "\r\n  Positive class: " + positiveCategory,
                                DoubleValues = new[] { cm.FScore, cm.Precision, cm.Recall }
                            });
                        });

                    var trainingAccuracyData = task.Result.Item1;
                    var testingAccuracyData = task.Result.Item2;

                    var trainingAccuracyData2 = learningMachine.AccuracyData?.train;
                    var testingAccuracyData2 = learningMachine.AccuracyData?.test;

                    // Training data may not be present (if training % was 0)
                    if (trainingAccuracyData != null)
                    {
                        statusUpdates.Enqueue(new StatusArgs { StatusMessage = "Training Set Accuracy:" });
                        writeAccuracyData(trainingAccuracyData, trainingAccuracyData2);
                    }

                    if (testingAccuracyData != null)
                    {
                        statusUpdates.Enqueue(new StatusArgs {StatusMessage = "Testing Set Accuracy:"});
                        writeAccuracyData(testingAccuracyData, testingAccuracyData2);
                    }
                }
                else
                {
                    statusUpdates.Enqueue(new StatusArgs
                    {
                        StatusMessage = "Error occurred. Time elapsed: " + elapsedTime
                    });

                    var firstException = task.Exception.InnerExceptions.FirstOrDefault();
                    if (firstException != null)
                    {
                        exception = firstException.AsExtract("ELI40378");
                    }
                }

                // Mark end of session
                statusUpdates.Enqueue(new StatusArgs
                {
                    TaskName = "__END_OF_SESSION__", // Keep from matching previous status
                    StatusMessage = "...\r\n" // In case these logs turn into YAML files, use YAML EOF
                });

                return exception;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
    }
}
