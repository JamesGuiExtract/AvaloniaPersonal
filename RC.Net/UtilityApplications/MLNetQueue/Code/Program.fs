open Argu
open Arguments
open FitModel
open GenerateModel
open Predict
open CrossValidate
open Extract.Utilities.FSharp

[<EntryPoint>]
let main argv =
  try
    let parser = ArgumentParser.Create<MLNetQueueArgs>(programName = "MLNetQueue.exe")
    let results = parser.ParseCommandLine(inputs = argv, raiseOnUsage = true)

    match results.GetSubCommand() with
    | Fit cmdArgs ->
        let trainingPipelinePath = cmdArgs.GetResult FitModelArgs.Model_Definition_Path
        let outputModelPath = cmdArgs.GetResult FitModelArgs.Output_Model_Path
        let trainingDataPath = cmdArgs.GetResult FitModelArgs.Training_Data_Path

        printfn "%s" (String.replicate 80 "-")
        printfn "%s" (timeStamp())
        fitModel trainingPipelinePath trainingDataPath outputModelPath |> ignore

    | Validate cmdArgs ->
        let trainingPipelinePath = cmdArgs.GetResult CrossValidateArgs.Model_Definition_Path
        let classifierType = cmdArgs.GetResult CrossValidateArgs.Classifier_Type
        let trainingDataPath = cmdArgs.GetResult CrossValidateArgs.Training_Data_Path
        let numberOfFolds = cmdArgs.GetResult CrossValidateArgs.Folds
        let numberOfRepetitions = cmdArgs.TryGetResult CrossValidateArgs.Repetitions
        let outputDataFolder = cmdArgs.GetResult CrossValidateArgs.Output_Data_Folder

        printfn "%s" (String.replicate 80 "-")
        printfn "%s" (timeStamp())
        match classifierType with
        | Binary -> crossValidateBinaryClassifier trainingPipelinePath trainingDataPath numberOfFolds numberOfRepetitions outputDataFolder
        | Multiclass -> failwith "Unsupported classifier type"

    | Generate cmdArgs ->
        let experimentType = cmdArgs.GetResult GenerateModelArgs.Experiment_Type
        let experimentTime = cmdArgs.GetResult GenerateModelArgs.Experiment_Time
        let outputModelPath = cmdArgs.GetResult GenerateModelArgs.Output_Model_Path
        let trainingDataPath = cmdArgs.GetResult GenerateModelArgs.Training_Data_Path

        printfn "%s" (String.replicate 80 "-")
        printfn "%s" (timeStamp())
        match experimentType with
        | Binary -> generateBinaryModel experimentTime trainingDataPath outputModelPath |> ignore
        | Multiclass -> failwith "Unsupported experiment type"

    | Predict cmdArgs ->
        let modelPath = cmdArgs.GetResult PredictionArgs.Model_Path
        let inputDataPath = cmdArgs.GetResult PredictionArgs.Input_Data_Path
        let outputDataPath = cmdArgs.GetResult PredictionArgs.Output_Data_Path
        predictWithModelFile inputDataPath modelPath outputDataPath

    | Listen cmdArgs ->
        let modelPath = cmdArgs.GetResult ListenArgs.Model_Path
        let pipeName = cmdArgs.GetResult ListenArgs.Pipe_Name
        listenForNamedPipeRequests pipeName modelPath
    0
  with
    | e ->
      eprintfn "%O" e
      exit 1
