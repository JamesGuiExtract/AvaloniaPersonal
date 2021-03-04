module Extract.AttributeFinder.MLNet.ClassifyCandidates.Evaluate

open Extract.Utilities.FSharp.Utils
open Microsoft.ML
open Microsoft.ML.Data
open System

let formatBinaryClassificationMetrics (metrics: BinaryClassificationMetrics) =
  [ sprintf "************************************************************"
    sprintf "*    Metrics for binary classification model   "
    sprintf "*-----------------------------------------------------------"
    sprintf "    Accuracy = %.4f" metrics.Accuracy
    sprintf "    AUC = %.4f" metrics.AreaUnderRocCurve
    sprintf "    F1 Score = %.4f" metrics.F1Score
    sprintf "    Negative Precision = %.4f" metrics.NegativePrecision
    sprintf "    Negative Recall = %.4f" metrics.NegativeRecall
    sprintf "    Positive Precision = %.4f" metrics.PositivePrecision
    sprintf "    Positive Recall = %.4f" metrics.PositiveRecall
    sprintf "    %s" (metrics.ConfusionMatrix.GetFormattedConfusionTable())
    sprintf "************************************************************"
  ] |> String.concat Environment.NewLine
(************************************************************************************************************************)

let evaluateBinaryTestResults (logger: Logger) (mlContext: MLContext) (data: IDataView) =
  let testResults = mlContext.BinaryClassification.Evaluate (data, labelColumnName = "Label")
  let msg = formatBinaryClassificationMetrics(testResults)
  logger.Log (Information, msg)
(************************************************************************************************************************)
