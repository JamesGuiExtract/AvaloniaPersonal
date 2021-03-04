module CrossValidate

open Microsoft.FSharp.Quotations
open Microsoft.ML
open Swensen.Unquote
open System
open System.IO
open Utils

let crossValidateBinaryClassifier (trainingPipelinePath: string) (trainingDataPath: string) numberOfFolds numberOfRepetitions outputDataFolder =
  let numberOfRepetitions = numberOfRepetitions |> Option.defaultValue 1
  let mlContext = MLContext()
  let trainingPipelineExpr: Expr<unit -> IEstimator<ITransformer>> = FsPickler.ofBinFile trainingPipelinePath
  let buildTrainingPipeline = trainingPipelineExpr |> eval
  let trainingPipeline = buildTrainingPipeline ()

  let trainingDataView = trainingDataPath |> mlContext.Data.LoadFromBinary
  outputDataFolder |> Directory.CreateDirectory |> ignore
  let rng = new Random()
  [1..numberOfRepetitions]
  |> Seq.collect (fun rep ->
    mlContext.BinaryClassification.CrossValidate(trainingDataView, trainingPipeline, numberOfFolds = numberOfFolds, seed = Nullable (rng.Next()))
  )
  |> Seq.iteri (fun i result ->
    let outputDataFile = Path.Combine (outputDataFolder, sprintf "ScoredHoldOutSet%02d.bin" (i + 1))
    printfn "Writing scored hold-out set %d to %s" (i+1) outputDataFile
    use stream = new FileStream(outputDataFile, FileMode.Create, FileAccess.Write)
    mlContext.Data.SaveAsBinary (result.ScoredHoldOutSet, stream)
  )
