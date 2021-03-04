module GenerateModel

open Extract.Utilities.FSharp
open Microsoft.ML
open Microsoft.ML.AutoML
open System
open System.Diagnostics

let generateBinaryModel (experimentTime: uint32) (trainingDataFile: string) (modelPath: string) =
  let mlContext = MLContext()
  let trainingDataView = trainingDataFile |> mlContext.Data.LoadFromBinary
  try
    let sw = Stopwatch()
    sw.Start()
    let experimentResult =
      mlContext.Auto()
        .CreateBinaryClassificationExperiment(experimentTime)
        .Execute(trainingDataView)
    let model = experimentResult.BestRun.Model
    sw.Stop()
    printfn "Done. Elapsed seconds: %.1f" (float sw.ElapsedMilliseconds / 1000.)
    printf "Saving model to %s..." modelPath
    retry { return mlContext.Model.Save (model, trainingDataView.Schema, modelPath) }
    model
  finally
    match trainingDataView with | :? IDisposable as d -> d.Dispose() | _ -> ()
(************************************************************************************************************************)
