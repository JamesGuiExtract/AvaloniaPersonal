module Extract.AttributeFinder.MLNet.ClassifyCandidates.Evaluate

open Extract.Utilities.FSharp.Utils
open Microsoft.ML
open Microsoft.ML.Data
open System
open System.IO
open System.Collections.Generic

let formatBinaryClassificationMetrics (metrics: BinaryClassificationMetrics) =
  [ sprintf "************************************************************"
    sprintf "*    Metrics for binary classification model   "
    sprintf "*-----------------------------------------------------------"
    sprintf "    Accuracy           = %.4f" metrics.Accuracy
    sprintf "    AUC                = %.4f" metrics.AreaUnderRocCurve
    sprintf "    F1 Score           = %.4f" metrics.F1Score
    sprintf "    Negative Precision = %.4f" metrics.NegativePrecision
    sprintf "    Negative Recall    = %.4f" metrics.NegativeRecall
    sprintf "    Positive Precision = %.4f" metrics.PositivePrecision
    sprintf "    Positive Recall    = %.4f" metrics.PositiveRecall
    sprintf "    %s" (metrics.ConfusionMatrix.GetFormattedConfusionTable())
    sprintf "************************************************************"
  ] |> String.concat Environment.NewLine
(************************************************************************************************************************)

let evaluateBinaryTestResults (logger: Logger) (mlContext: MLContext) (data: IDataView) =
  let testResults = mlContext.BinaryClassification.Evaluate (data, labelColumnName = "Label")
  let msg = formatBinaryClassificationMetrics(testResults)
  logger.Log (Information, msg)
(************************************************************************************************************************)

let standardDeviation (values : #IList<float>) =
  let mean = values |> Seq.average
  let sumOfSquaresOfDifferences = values |> Seq.sumBy (fun v -> pown (v - mean) 2)
  sqrt (sumOfSquaresOfDifferences / float (values.Count - 1))

let marginOfErrorAt95ConfidenceLevel (values: #IList<float>) =
  1.96 * (standardDeviation values) / sqrt(float values.Count);

let formatBinaryCrossValidationMetrics (metrics: #IList<#BinaryClassificationMetrics>) =
  let aggregate (metrics: #seq<float>) =
    let metrics = metrics |> Seq.toArray
    let mean = metrics |> Seq.average
    let std = metrics |> standardDeviation
    let moe = metrics |> marginOfErrorAt95ConfidenceLevel
    mean, std, moe

  [ sprintf "**************************************************************************************************"
    sprintf "*    Metrics for binary classification model (%d folds/repetitions)  " metrics.Count
    sprintf "*-------------------------------------------------------------------------------------------------"
    sprintf "    Accuracy:           mean = %.4f, standard deviation = %.4f, margin of error (95%%) = %.4f" <||| (metrics |> Seq.map (fun m -> m.Accuracy) |> aggregate)
    sprintf "    Area under ROC:     mean = %.4f, standard deviation = %.4f, margin of error (95%%) = %.4f" <||| (metrics |> Seq.map (fun m -> m.AreaUnderRocCurve) |> aggregate)
    sprintf "    F1 Score:           mean = %.4f, standard deviation = %.4f, margin of error (95%%) = %.4f" <||| (metrics |> Seq.map (fun m -> m.F1Score) |> aggregate)
    sprintf "    Negative Precision: mean = %.4f, standard deviation = %.4f, margin of error (95%%) = %.4f" <||| (metrics |> Seq.map (fun m -> m.NegativePrecision) |> aggregate)
    sprintf "    Negative Recall:    mean = %.4f, standard deviation = %.4f, margin of error (95%%) = %.4f" <||| (metrics |> Seq.map (fun m -> m.NegativeRecall) |> aggregate)
    sprintf "    Positive Precision: mean = %.4f, standard deviation = %.4f, margin of error (95%%) = %.4f" <||| (metrics |> Seq.map (fun m -> m.PositivePrecision) |> aggregate)
    sprintf "    Positive Recall:    mean = %.4f, standard deviation = %.4f, margin of error (95%%) = %.4f" <||| (metrics |> Seq.map (fun m -> m.PositiveRecall) |> aggregate)
    sprintf "**************************************************************************************************"
  ] |> String.concat Environment.NewLine
(************************************************************************************************************************)

let evaluateBinaryCrossValidationResults (logger: Logger) (mlContext: MLContext) (dataFolder: string) =
  let testResults =
    Directory.GetFiles(dataFolder, "*.bin")
    |> Array.map (fun file ->
      let data = mlContext.Data.LoadFromBinary(file)
      try
        mlContext.BinaryClassification.Evaluate (data, labelColumnName = "Label")
      finally
        match data with
        | :? IDisposable as d -> d.Dispose ()
        | _ -> ()
    )
  let msg = formatBinaryCrossValidationMetrics(testResults)
  logger.Log (Information, msg)
(************************************************************************************************************************)
