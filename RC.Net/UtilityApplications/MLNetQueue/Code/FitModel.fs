module FitModel

open Extract.Utilities.FSharp
open Microsoft.FSharp.Quotations
open Microsoft.ML
open Swensen.Unquote
open System
open System.Diagnostics
open Utils

let fitModel (trainingPipelineFile: string) (trainingDataFile: string) (modelPath: string) =
  let mlContext = MLContext()
  let trainingPipelineExpr: Expr<unit -> IEstimator<ITransformer>> = FsPickler.ofBinFile trainingPipelineFile
  let buildTrainingPipeline = trainingPipelineExpr |> eval
  let trainingPipeline = buildTrainingPipeline ()

  let trainingDataView = trainingDataFile |> mlContext.Data.LoadFromBinary
  try
    let sw = Stopwatch()
    sw.Start()
    let model = trainingPipeline.Fit trainingDataView
    sw.Stop()
    printfn "Done. Elapsed seconds: %.1f" (float sw.ElapsedMilliseconds / 1000.)
    printf "Saving model to %s..." modelPath
    retry { return mlContext.Model.Save (model, trainingDataView.Schema, modelPath) }
    model
  finally
    match trainingDataView with | :? IDisposable as d -> d.Dispose() | _ -> ()
(************************************************************************************************************************)
