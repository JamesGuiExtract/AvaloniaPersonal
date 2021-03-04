module Arguments

open Argu

type ClassifierType = | Binary | Multiclass

// Args for training a model
type FitModelArgs =
  | [<Mandatory>] Model_Definition_Path of modelDefinitionPath:string
  | [<Mandatory>] Training_Data_Path of dataPath:string
  | [<Mandatory>] Output_Model_Path of modelPath:string
with
  interface IArgParserTemplate with
    member s.Usage =
      match s with
        | Model_Definition_Path _ -> "Path to the serialized training pipeline to be fit to the training data"
        | Training_Data_Path _ -> "Path to serialized IDataView file containing the training data"
        | Output_Model_Path _ -> "Path to save the fitted model to"

// Args for training a model using cross validation
type CrossValidateArgs =
  | [<Mandatory>] Model_Definition_Path of modelDefinitionPath:string
  | [<Mandatory>] Classifier_Type of classifierType:ClassifierType
  | [<Mandatory>] Training_Data_Path of dataPath:string
  | [<Mandatory>] Folds of numberOfFolds:int
  | Repetitions of numberOfRepetitions:int
  | [<Mandatory>] Output_Data_Folder of outputDataPath:string
with
  interface IArgParserTemplate with
    member s.Usage =
      match s with
        | Model_Definition_Path _ -> "Path to the serialized training pipeline to be fit to the training data"
        | Classifier_Type _ -> "Binary or Multiclass"
        | Training_Data_Path _ -> "Path to serialized IDataView file containing the training data"
        | Folds _ -> "Number of folds to divide the data into (more folds means more but smaller test sets)"
        | Repetitions _ -> "Optional number of times to repeat the cross validation (concatenate the results)"
        | Output_Data_Folder _ -> "Folder to save the scored IDataView results for the hold-out sets"

// Args for training a model using AutoML (not fully implemented)
and GenerateModelArgs =
  | [<Mandatory>] Experiment_Type of experimentType:ClassifierType
  | [<Mandatory>] Experiment_Time of experimentTime:uint32
  | [<Mandatory>] Training_Data_Path of dataPath:string
  | [<Mandatory>] Output_Model_Path of modelPath:string
with
  interface IArgParserTemplate with
    member s.Usage =
      match s with
        | Experiment_Type _ -> "Binary or Multiclass"
        | Experiment_Time _ -> "Time in seconds to run the experiment"
        | Training_Data_Path _ -> "Path to serialized IDataView file containing the training data"
        | Output_Model_Path _ -> "Path to save the fitted model to"

// Args for prediction (transforming a data view) using a previously trained model
and PredictionArgs =
  | [<Mandatory>] Model_Path of modelPath:string
  | [<Mandatory>] Input_Data_Path of inputDataPath:string
  | [<Mandatory>] Output_Data_Path of outputDataPath:string
with
  interface IArgParserTemplate with
    member s.Usage =
      match s with
        | Model_Path _ -> "Path to the fitted (trained) model file"
        | Input_Data_Path _ -> "Path to serialized IDataView file containing the input data"
        | Output_Data_Path _ -> "Path to save the output IDataView containing the predictions from the model"

// Args for setting up a server to listen for prediction (transform) requests
and ListenArgs =
  | [<Mandatory>] Model_Path of modelPath:string
  | [<Mandatory>] Pipe_Name of pipeName:string
with
  interface IArgParserTemplate with
    member s.Usage =
      match s with
        | Model_Path _ -> "Path to the fitted (trained) model file"
        | Pipe_Name _ -> "Pipe to create and listen for prediction requests on"

// Top-level args
and [<RequireSubcommand>] MLNetQueueArgs =
  | [<CliPrefix(CliPrefix.None)>] Fit of ParseResults<FitModelArgs>
  | [<CliPrefix(CliPrefix.None)>] Validate of ParseResults<CrossValidateArgs>
  | [<CliPrefix(CliPrefix.None)>] Generate of ParseResults<GenerateModelArgs>
  | [<CliPrefix(CliPrefix.None)>] Predict of ParseResults<PredictionArgs>
  | [<CliPrefix(CliPrefix.None)>] Listen of ParseResults<ListenArgs>
with
  interface IArgParserTemplate with
    member s.Usage =
      match s with
        | Fit _ -> "Fit model to data"
        | Validate _ -> "Fit and test a model using cross validation"
        | Generate _ -> "Create a model using auto-ml"
        | Predict _ -> "Predict using fitted model"
        | Listen _ -> "Listen for prediction requests"
