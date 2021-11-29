module Predict

open Microsoft.ML
open System
open System.IO
open Extract.Utilities.FSharp

let predict (mlContext: MLContext) (inputDataFile: string) (model: ITransformer) (outputDataFile: string) =
  let inputs = inputDataFile |> mlContext.Data.LoadFromBinary
  try
    let outputs = inputs |> model.Transform
    use stream = new FileStream(outputDataFile, FileMode.Create, FileAccess.Write)
    mlContext.Data.SaveAsBinary(outputs, stream)
  finally
    match inputs with | :? IDisposable as d -> d.Dispose() | _ -> ()
(************************************************************************************************************************)

let predictWithModelFile (inputDataFile: string) (modelFile: string) (outputDataFile: string) =
  let mlContext = MLContext()
  // Retry in case the training process is overwriting this file
  let model, _modelInputSchema = retry { return mlContext.Model.Load modelFile }
  predict mlContext inputDataFile model outputDataFile
(************************************************************************************************************************)

let retry = RetryBuilder (5, 500)

let listenForNamedPipeRequests pipeName (modelFile: string) =
  let mlContext = MLContext()
  // Load the model lazily so that we can start listening to the pipe immediately
  let model =
    lazy retry {
      let model, _schema = mlContext.Model.Load modelFile
      return model
    }

  let dispatch (request: DTO.PredictionRequest) =
    predict mlContext request.InputDataFile model.Value request.OutputDataFile
    
  NamedPipe.listenForRequests pipeName dispatch
