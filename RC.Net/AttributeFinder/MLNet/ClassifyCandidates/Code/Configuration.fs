module Extract.AttributeFinder.MLNet.ClassifyCandidates.Configuration

open Microsoft.FSharp.Quotations
open Microsoft.ML
open System.Diagnostics
open UCLID_COMUTILSLib
open Extract.Utilities.FSharp.Utils

type DevConfig = {
  dbServer: string
  dbName: string
  workflowName: string
}

// One-case discriminated unions to document function params
type SourceDocName = | SourceDocName of string
type FndAttributes = | FndAttributes of IUnknownVector
type ExpAttributes = | ExpAttributes of IUnknownVector

// Interface to hide generic 'TExample and 'TPrediction types from the rest of the framework
type IMLConverter =
  abstract ConvertToDataView: MLContext -> ExpAttributes option -> FndAttributes -> IDataView
  abstract ConvertJsonFilesToDataView: MLContext -> string seq -> IDataView
  abstract ConvertToJsonFile: ExpAttributes -> FndAttributes -> string -> unit
  abstract ConvertFromDataView: MLContext -> FndAttributes -> IDataView -> FndAttributes

type ConvertToMLExamples<'TExample> = ExpAttributes option -> FndAttributes -> 'TExample seq
type ConvertFromPredictions<'TPrediction> = FndAttributes -> 'TPrediction seq -> FndAttributes

// Generic type to handle converting specific example/prediction types to json and IDataView
type MLConverter<'TExample, 'TPrediction
  when 'TExample : not struct
  and 'TPrediction : not struct
  and 'TPrediction : (new : unit -> 'TPrediction)>
  ( convertToMLExamples: ConvertToMLExamples<'TExample>,
    convertFromPredictions: ConvertFromPredictions<'TPrediction> ) =

  interface IMLConverter with
    member _.ConvertToDataView mlContext exp fnd =
      let examples = convertToMLExamples exp fnd
      mlContext.Data.LoadFromEnumerable examples

    member _.ConvertJsonFilesToDataView mlContext files =
      files
      |> Seq.collect Object.ofJsonFile<'TExample seq>
      |> mlContext.Data.LoadFromEnumerable

    member _.ConvertToJsonFile exp fnd outPath =
      let examples = convertToMLExamples (Some exp) fnd
      examples |> Object.toJsonFile outPath

    member _.ConvertFromDataView mlContext fnd dataView =
      let predictions = mlContext.Data.CreateEnumerable<'TPrediction>(dataView, false)
      predictions |> convertFromPredictions fnd

// Controls whether candidates are read/written from/to a VOA file cache
type CandidateMode = | ReadFromDocOnly | ReadFromExt of string | ReadFromDocWriteToExt of string 

type ModelConfig = {
  modelFolder: string
  modelName: string
  expExt: string
  candidateMode: CandidateMode
  trainingDataExt: string
  useExistingData: bool
  converter: IMLConverter
  buildTrainingPipeline: Expr<unit -> IEstimator<ITransformer>>
  evaluateTestResults: (Logger -> MLContext -> IDataView -> unit) option
  evaluateCrossValidationResults: (Logger -> MLContext -> string -> unit) option
}

type Config = {
  mainLogDir: string
  maxLogSubDirs: int option

  modelConfigs: ModelConfig list
  dataCollectionAction: string
  newTrainingDataAction: string
  oldTrainingDataAction: string

  lockFileDir: string
  bootstrapTrainingDataDir: string
  minNewFilesBeforeRetrain: int
  trainWheneverMinNewFilesAreAvailable: bool
  minFilesToTrainOn: int
  maxFilesToTrainOn: int

  trainingPriorityClass: ProcessPriorityClass
  dataCollectionLogger: Logger
  learningLogger: Logger
  devConfig: DevConfig option
}

module Config =
  let private nullLogger = Logger("")
  nullLogger.Disable ()
  let defaults<'TExample, 'TPrediction> = {
    mainLogDir = "" // Path.GetFullPath (__SOURCE_DIRECTORY__ + @"\..\logs")
    maxLogSubDirs = Some 7
    modelConfigs = []
    dataCollectionAction = "CollectTrainingData"
    newTrainingDataAction = "NewTrainingData"
    oldTrainingDataAction = "OldTrainingData"
    lockFileDir = "" // Path.GetFullPath (__SOURCE_DIRECTORY__ + @"\..\ML")
    bootstrapTrainingDataDir = "" // __SOURCE_DIRECTORY__ + @"\..\bootstrap"
    minNewFilesBeforeRetrain = 1
    trainWheneverMinNewFilesAreAvailable = false
    minFilesToTrainOn = 1000
    maxFilesToTrainOn = 10000
    trainingPriorityClass = ProcessPriorityClass.BelowNormal
    dataCollectionLogger = nullLogger
    learningLogger = nullLogger
    devConfig = None
  }