module Extract.AttributeFinder.MLNet.ClassifyCandidates.Learn

open Configuration
open Extract
open Extract.Utilities.FSharp.AFUtils
open Extract.Utilities.FSharp
open Microsoft.ML
open System
open System.IO
open System.Threading
open UCLID_AFCORELib
open UCLID_FILEPROCESSINGLib
open UCLID_AFUTILSLib
open Extract.AttributeFinder

module FsPickler =
  open MBrace.FsPickler
  let serializer = BinarySerializer();

  let toBinFile (fileName: string) (x: 'a) =
      use stream = new FileStream(fileName, FileMode.Create, FileAccess.Write)
      serializer.Serialize(stream, x)

  let ofBinFile<'a> (fileName: string): 'a = 
    use stream = new FileStream(fileName, FileMode.Open, FileAccess.Read)
    serializer.Deserialize<'a>(stream)

type FamDB(config: Config) =
  let fileProcessingDB = FileProcessingDBClass()
  let wfID, isConnected =
    let connected =
      try
        fileProcessingDB.GetLastConnectionStringConfiguredThisProcess () |> ignore // this will throw if server is empty
        fileProcessingDB.ConnectLastUsedDBThisProcess ()
        true
      with _ ->
        match config.devConfig with
        | Some devConfig ->
          fileProcessingDB.DatabaseServer <- devConfig.dbServer
          fileProcessingDB.DatabaseName <- devConfig.dbName
          fileProcessingDB.ActiveWorkflow <- devConfig.workflowName
          true
        | None -> false
    if connected then
      try
        let wfID = fileProcessingDB.GetWorkflowID (fileProcessingDB.ActiveWorkflow)
        fileProcessingDB.RecordFAMSessionStart ("ML.Net", config.dataCollectionAction, false, true)
        fileProcessingDB.RegisterActiveFAM ()
        wfID, true
      with _ -> 0, false
    else
      0, false

  let checkoutFiles action max =
    retry {
      let files =
        fileProcessingDB.GetFilesToProcess (action, max, false, "")
        |> Seq.ofUV
        |> ResizeArray<_>
      if files.Count = 0 then
        let actionID = fileProcessingDB.GetActionID action
        let stats = fileProcessingDB.GetStats (actionID, false)
        if stats.NumDocumentsPending = 0
        then return files
      else
        return files
    }

  let checkinFile action (fileRecord : #IFileRecord) =
    fileProcessingDB.SetFileStatusToPending (fileRecord.FileID, action, false)

  let checkinFiles action =
    checkinFile action |> Seq.iter

  let completeFile action (fileRecord : #IFileRecord) =
    fileProcessingDB.NotifyFileProcessed (fileRecord.FileID, action, wfID, false)

  let completeFiles action =
    completeFile action |> Seq.iter

  let setPendingFileID actionName fileID =
    fileProcessingDB.SetFileStatusToPending (fileID, actionName, false)

  let getFileID fileName =
   fileProcessingDB.GetFileID fileName

  let setPendingFileName actionName fileName =
    getFileID fileName |> setPendingFileID actionName

  let skipFile actionName (fileRecord : #IFileRecord) =
    fileProcessingDB.SetFileStatusToSkipped (fileRecord.FileID, actionName, false, false)

  let tryCatchAndReleaseFile actionName =
    let pending = checkoutFiles actionName 1 
    if pending.Count = 1 then
      pending |> checkinFiles actionName
      true
    else false
  
  let tryGetAtLeastXFiles actionName minFiles maxFiles =
    let fileRecords = checkoutFiles actionName maxFiles
    if fileRecords.Count >= minFiles then
      true, fileRecords
    else
      fileRecords |> checkinFiles actionName
      fileRecords.Clear ()
      false, fileRecords

  let getNumberProcessing actionName =
    let actionID = fileProcessingDB.GetActionID actionName
    let stats = fileProcessingDB.GetStats (actionID, true)
    let numDocumentsNotProcessing = stats.NumDocumentsPending + stats.NumDocumentsComplete + stats.NumDocumentsFailed + stats.NumDocumentsSkipped
    let numDocumentsProcessing = stats.NumDocuments - numDocumentsNotProcessing
    numDocumentsProcessing

  with
    member _.IsConnected = isConnected
    member _.DB = fileProcessingDB
    member _.WFID = wfID
    member _.CheckoutFiles = checkoutFiles
    member _.CheckinFiles = checkinFiles
    member _.CompleteFiles = completeFiles
    member _.CompleteFile = completeFile
    member _.SetPendingFileName = setPendingFileName
    member _.SkipFile = skipFile
    member _.TryCatchAndReleaseFile = tryCatchAndReleaseFile
    member _.TryGetAtLeastXFiles = tryGetAtLeastXFiles
    member _.GetNumberProcessing = getNumberProcessing

    interface IDisposable with
      member _.Dispose() = 
        try
          fileProcessingDB.UnregisterActiveFAM ()
          fileProcessingDB.RecordFAMSessionStop ()
        with _ -> ()
(******************************************************************************************************)

[<AutoOpen>]
module DataCollector = 
  // Use found and expected data as inputs to the config.convertToMLExamples function
  let createTrainingData (config: Config) (doc: AFDocument): AFDocument =
    let sdn = doc.Text.SourceDocName
    config.modelConfigs
    |> Seq.iter (fun modelConfig ->
      let dataFile = sdn + modelConfig.trainingDataExt
      if modelConfig.useExistingData && File.Exists dataFile then ()
      else
        let afUtil = AFUtilityClass()
        let exp = loadVoa afUtil (sdn + modelConfig.expExt) |> ExpAttributes
        let fnd =
          match modelConfig.candidateMode with
          | ReadFromDocOnly -> doc.Attribute.SubAttributes |> FndAttributes
          | ReadFromDocWriteToExt candidateExt ->
              let attrr = doc.Attribute.SubAttributes
              AttributeMethods.SaveAttributes (attrr, sdn + candidateExt)
              attrr |> FndAttributes
          | ReadFromExt candidateExt -> loadVoa afUtil (sdn + candidateExt) |> FndAttributes
        modelConfig.converter.ConvertToJsonFile exp fnd dataFile
    )
    doc

  let prepareFileForTraining (fpdb: FamDB) (config: Config) (doc: AFDocument) =
    let sourceDocName = doc.Text.SourceDocName
    if not (String.IsNullOrWhiteSpace sourceDocName) then
      config.dataCollectionLogger.Log (Information, sprintf "Collecting data for %s" sourceDocName)
      createTrainingData config doc |> ignore
      fpdb.SetPendingFileName config.newTrainingDataAction sourceDocName
(******************************************************************************************************)

type ReleaseAfterUsingLock(lockFile) =
  let mutex = new Mutex(false, lockFile |> Utils.hashString)
  let mutable isMutexAcquired = false
  let mutable fileLock = None
  let acquire (timeToWait : int) =
    let retry = RetryBuilder (Math.Max (1, timeToWait / 500), 500)
    if mutex |> Utils.acquireMutex timeToWait then
      isMutexAcquired <- true
      try
        fileLock <-
          retry {
            return Some (new FileStream(lockFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Delete))
          }
      with
      | _ ->
        isMutexAcquired <- false
        mutex.ReleaseMutex ()
    isMutexAcquired

  let release () =
    if isMutexAcquired then
      try mutex.ReleaseMutex () with _ -> ()
      match fileLock with
      | Some fileStream ->
        try File.Delete lockFile with _ -> ()
        fileStream.Dispose ()
      | None -> ()
      isMutexAcquired <- false
  with
    member x.Acquire (timeToWait) = acquire timeToWait
    member x.Release () = release ()

    interface IDisposable with
      member this.Dispose () = release (); mutex.Dispose ()
(******************************************************************************************************)

[<AutoOpen>]
module LearnOrTest =
  let binFolder = Extract.Utilities.FileSystemMethods.CommonComponentsPath
  let mlNetQueueExe = Path.Combine(binFolder, "MLNetQueue.exe")

  type TrainingData = {
    newTrainingFiles: ResizeArray<IFileRecord>
    oldTrainingFiles: ResizeArray<IFileRecord>
    bootstrapTrainingFiles: ResizeArray<string>
  }

  module TrainingData =
    let empty() = {
      newTrainingFiles = ResizeArray<IFileRecord>()
      oldTrainingFiles = ResizeArray<IFileRecord>()
      bootstrapTrainingFiles = ResizeArray<string>()
    }

    let total data =
      data.newTrainingFiles.Count + data.oldTrainingFiles.Count + data.bootstrapTrainingFiles.Count
  (****************************************************************************************************)

  let private checkDBQueue (logger: Logger) (fpdb: FamDB) (config: Config) = (
    logger.Log (Information, sprintf "Checking %s queue for new training data" config.newTrainingDataAction)
    match fpdb.TryGetAtLeastXFiles config.newTrainingDataAction config.minNewFilesBeforeRetrain config.maxFilesToTrainOn with
    | false, _ ->
      logger.Log (Information, "Insufficent new training data found")
      None
    | true, files ->
      let data = TrainingData.empty()
      logger.Log (Information, sprintf "%d new files for training" files.Count)
      data.newTrainingFiles.AddRange files
      if data.newTrainingFiles.Count < config.maxFilesToTrainOn then
        logger.Log (Information, sprintf "Checking %s queue for old training data" config.oldTrainingDataAction)
        match fpdb.TryGetAtLeastXFiles config.oldTrainingDataAction 1 Int32.MaxValue with
        | false, _ ->
          logger.Log (Information, "No old training data found")
        | true, oldFiles ->
          let numberOfOldFilesToUse = config.maxFilesToTrainOn - data.newTrainingFiles.Count
          let numberOfExtras = Math.Max (0, oldFiles.Count - numberOfOldFilesToUse)
          logger.Log (Information, sprintf "Using %d old files for training, setting %d old files to complete" (oldFiles.Count - numberOfExtras) numberOfExtras)
          oldFiles
          |> Seq.indexed
          |> Seq.iter (fun (i, fileRecord) ->
            if i < numberOfExtras then
              fpdb.CompleteFile config.oldTrainingDataAction fileRecord
            else
              data.oldTrainingFiles.Add fileRecord
          )
      Some data
  )
  (****************************************************************************************************)

  // Collect files matching any trainingDataExt and reduce files to base names so that name + <modelConfig>trainingDataExt will exist
  // for at least one of each file
  // (Ideally these files would be named systematically so that there is maximum overlap of the base names between models)
  let private getBootstrapFiles (logger: Logger) (config: Config) (data: TrainingData) = (
    if data.newTrainingFiles.Count + data.oldTrainingFiles.Count < config.maxFilesToTrainOn then
      logger.Log (Information, sprintf "Checking for 'bootstrap' training data in %s" config.bootstrapTrainingDataDir)
      let bsFiles =
        if Directory.Exists config.bootstrapTrainingDataDir then
          let filters =
            config.modelConfigs
            |> Seq.map (fun config -> config.trainingDataExt.ToLowerInvariant())
            |> Seq.distinct
            |> Seq.sortByDescending String.length
            |> Seq.toList
          Directory.GetFiles (config.bootstrapTrainingDataDir, "*.*")
          |> Seq.choose (fun fname ->
            let lowCaseName = fname.ToLowerInvariant()
            filters
            |> Seq.tryFind lowCaseName.EndsWith
            |> Option.map (fun longestMatch -> fname.Remove (fname.Length - longestMatch.Length))
          )
          |> Seq.distinct
          |> Seq.toArray
        else [||]
      match bsFiles.Length with
      | 0 ->
        logger.Log (Information, "No bootstrap training data found")
      | _ ->
        let numberOfBootstrapFilesToUse = config.maxFilesToTrainOn - data.newTrainingFiles.Count - data.oldTrainingFiles.Count
        let numberOfExtras = Math.Max (0, bsFiles.Length - numberOfBootstrapFilesToUse)
        logger.Log (Information, sprintf "Using %d bootstrap files for training" (bsFiles.Length - numberOfExtras))
        data.bootstrapTrainingFiles.AddRange(bsFiles |> Seq.skip numberOfExtras)
    data
  )
  (****************************************************************************************************)

  type Mode = | LearnMode | ValidationMode | TestMode
  type ExceptionType = | ExceptionObject of exn | ExceptionPrintout of string seq

  // Create and throw an exception
  let handleFailure (logger: Logger) mode (modelName: string) error = (
    let msg = match mode with | LearnMode -> "Failed to learn!" | ValidationMode -> "Failed to cross validate!" | TestMode -> "Failed to complete test!"
    let uex =
      match error with
      | ExceptionPrintout errors ->
        let uex = ExtractException("ELI51585", msg)
        errors
        |> Seq.iter (fun (error : string) ->
          logger.Log (Utils.Exception, error)
          uex.AddDebugData ("Error", error)
        )
        uex
      | ExceptionObject ex -> ExtractException("ELI51586", msg, ex)
    uex.AddDebugData ("Model", modelName)
    raise uex
  )
  (******************************************************************************************************)

  let deleteOldLogDirsIfRequired parentDir pattern maxFolders =
    if Directory.Exists parentDir then
      let matchingFolders =
        parentDir
        |> getFoldersStartingWithPattern pattern
        |> Array.sort
      let extra = matchingFolders.Length - maxFolders
      if extra > 0 then
        matchingFolders
        |> Seq.take extra
        |> Seq.iter (fun dir -> try Directory.Delete(dir, true) with _ -> ())
  (******************************************************************************************************)

  let tryToLearn (fpdb: FamDB) (config: Config) = (
    let logger = config.learningLogger
    let handleFailure = handleFailure logger
    let sessionLogDir =
      let prefix = "LearnAndTest_"
      let name = prefix + timeStampForFile ()
      let pattern = prefix + timeStampForFilePat
      match config.maxLogSubDirs with
      | Some maxDirs -> deleteOldLogDirsIfRequired config.mainLogDir pattern maxDirs
      | None -> ()
      Path.Combine (config.mainLogDir, name)

    let checkQueue () = (
      let checkForTrainingData =
        if config.trainWheneverMinNewFilesAreAvailable then
          true
        elif fpdb.IsConnected then
          if fpdb.TryCatchAndReleaseFile config.dataCollectionAction then false // There are still files to collect data for so don't check the queue
          else fpdb.GetNumberProcessing config.dataCollectionAction = 1 // This is the last file processing
        else
          false

      match checkForTrainingData, fpdb.IsConnected with
      | true, true ->
        checkDBQueue logger fpdb config
        |> Option.map (getBootstrapFiles logger config)
      | true, false when config.minNewFilesBeforeRetrain = 0 ->
        TrainingData.empty()
        |> getBootstrapFiles logger config |> Some
      | _ -> None
    )
    (******************************************************************************************************)

    let crossValidate (modelDefPath: string) trainingDataView (modelConfig: ModelConfig) = (
      match modelConfig.evaluateCrossValidationResults with
      | None -> ()
      | Some evaluateCVResults ->
        let cvDir = Path.Combine (modelConfig.modelFolder, modelConfig.modelName + "_Validation")
        try
          Directory.CreateDirectory cvDir |> ignore
          let logPath = Path.Combine (sessionLogDir, sprintf "%s-Validation.txt" modelConfig.modelName)
          let args =
            sprintf """validate --model-definition-path "%s" --training-data-path "%s" --classifier-type Binary --folds 5 --output-data-folder "%s" """
                    modelDefPath trainingDataView cvDir
          let exitCode, _output, error = runProcLogToFile mlNetQueueExe args None logPath config.trainingPriorityClass
          if exitCode <> 0 then handleFailure ValidationMode modelConfig.modelName (ExceptionPrintout error)

          let logger = Logger(logPath)
          evaluateCVResults logger (MLContext()) cvDir
        finally
          try
            Directory.Delete (cvDir, true)
          with e ->
            ExtractException.Log ("ELI51591", e)
    )

    let learn trainingDataView (modelConfig: ModelConfig) = (
      Directory.CreateDirectory modelConfig.modelFolder |> ignore
      let modelDefPath = sprintf """%s\MLModelDefinition_%s.bin""" modelConfig.modelFolder modelConfig.modelName
      modelConfig.buildTrainingPipeline |> FsPickler.toBinFile modelDefPath

      // Cross validate if there is a supplied evaluation function
      crossValidate modelDefPath trainingDataView modelConfig

      let modelPath = sprintf """%s\.temp.MLModel_%s.zip""" modelConfig.modelFolder modelConfig.modelName
      let logPath = Path.Combine (sessionLogDir, sprintf "%s-Learn.txt" modelConfig.modelName)
      let args = sprintf """fit --model-definition-path "%s" --training-data-path "%s" --output-model-path "%s" """ modelDefPath trainingDataView modelPath
      let exitCode, _output, error = runProcLogToFile mlNetQueueExe args None logPath config.trainingPriorityClass
      if exitCode <> 0 then handleFailure LearnMode modelConfig.modelName (ExceptionPrintout error)

      let destModelPath = sprintf """%s\MLModel_%s.zip""" modelConfig.modelFolder modelConfig.modelName

      if File.Exists destModelPath then
        retry { return File.Delete destModelPath }

      retry { return File.Move (modelPath, destModelPath) }
    )
    (******************************************************************************************************)

    // Run the prediction and delegate evaluating the results to the caller via modelConfig.evaluateTestResults
    let test testingDataViewPath (modelConfig: ModelConfig) = (
      match modelConfig.evaluateTestResults with
      | None -> ()
      | Some computeTestResults ->
        let modelPath = sprintf """%s\MLModel_%s.zip""" modelConfig.modelFolder modelConfig.modelName
        use outputDataFile = new TempFile()
        let outputDataPath = outputDataFile.FileName
        try
          Predict.predictWithMLNetQueue modelPath { InputDataFile = testingDataViewPath; OutputDataFile = outputDataPath }
          let mlContext = MLContext()
          let dataView = mlContext.Data.LoadFromBinary outputDataPath
          try
            let logPath = Path.Combine (sessionLogDir, sprintf "%s-Test.txt" modelConfig.modelName)
            let logger = Logger(logPath)
            computeTestResults logger mlContext dataView
          finally
            match dataView with | :? IDisposable as d -> d.Dispose() | _ -> ()
        with | e -> handleFailure TestMode modelConfig.modelName (ExceptionObject e)
    )
    (******************************************************************************************************)
  
    let createDataView images (modelConfig: ModelConfig) dataViewPath = (
      let mlContext = MLContext()
      let dataView =
        images
        |> Seq.map (fun imageName -> imageName + modelConfig.trainingDataExt)
        |> modelConfig.converter.ConvertJsonFilesToDataView mlContext
      use stream = new FileStream(dataViewPath, FileMode.Create, FileAccess.Write)
      mlContext.Data.SaveAsBinary(dataView, stream)
    )
    (******************************************************************************************************)

    let learnAndTest (data: TrainingData) (modelConfig: ModelConfig) = (
      let getNames = Seq.map (fun (fileRecord : IFileRecord) -> fileRecord.Name)

      // First test existing model on the new data
      let testingFiles = data.newTrainingFiles |> getNames |> ResizeArray<_>
      if testingFiles.Count > 0 then
        try
          let testingDataViewPath = sprintf """%s\TestingData_%s.bin""" modelConfig.modelFolder modelConfig.modelName
          createDataView testingFiles modelConfig testingDataViewPath
          logger.Log (Information,  sprintf "Testing previously trained model (%s) with %d new files" modelConfig.modelName testingFiles.Count)
          test testingDataViewPath modelConfig
          logger.Log (Information,  "Done testing model")
        with | :? ExtractException as e -> e.Log()

      let trainingFileNames =
        seq {
          yield! data.bootstrapTrainingFiles
          yield! data.oldTrainingFiles |> getNames
          yield! data.newTrainingFiles |> getNames
        }
        |> Seq.distinct
        |> ResizeArray<_>
      if trainingFileNames.Count < config.minFilesToTrainOn then
        logger.Log (Information,  "Setting old training data back to pending")
        fpdb.CheckinFiles config.oldTrainingDataAction (upcast data.oldTrainingFiles)

        logger.Log (Information,  "Setting new training data back to pending")
        fpdb.CheckinFiles config.newTrainingDataAction (upcast data.newTrainingFiles)
        false
      else
        logger.Log (Information, sprintf "Beginning training model (%s) with %d distinct files" modelConfig.modelName trainingFileNames.Count)

        let trainingDataViewPath = sprintf """%s\TrainingData_%s.bin""" modelConfig.modelFolder modelConfig.modelName
        createDataView trainingFileNames modelConfig trainingDataViewPath

        try
          logger.Log (Information,  sprintf "Training model (%s)" modelConfig.modelName)
          learn trainingDataViewPath modelConfig
          logger.Log (Information,  sprintf "Done training model (%s)" modelConfig.modelName)
          true

        finally
          logger.Log (Information,  "Converting newly-processed training data to old training data")
          fpdb.CheckinFiles config.oldTrainingDataAction (upcast data.newTrainingFiles)
          fpdb.CompleteFiles config.newTrainingDataAction (upcast data.newTrainingFiles)

          logger.Log (Information,  "Setting old training data back to pending")
          fpdb.CheckinFiles config.oldTrainingDataAction (upcast data.oldTrainingFiles)
          logger.Log (Information,  "Done setting old training data back to pending")
    )
    (******************************************************************************************************)

    config.lockFileDir |> Directory.CreateDirectory |> ignore
    use checkingQueueLock = new ReleaseAfterUsingLock (Path.Combine (config.lockFileDir, "checkingQueue.lock"))
    if checkingQueueLock.Acquire 60000 then
      use doingTrainingLock = new ReleaseAfterUsingLock (Path.Combine (config.lockFileDir, "doingTraining.lock"))
      if doingTrainingLock.Acquire 0 then
        let data = checkQueue ()
        checkingQueueLock.Release ()
        match data with
        | Some data when data |> TrainingData.total > 0 ->
          config.modelConfigs
          |> List.map (learnAndTest data)
          |> List.exists id // Return true if any model was successfully trained
        | _ -> false
      else false
    else
      logger.Log (Utils.Exception,  "Could not acquire lock to check the queue")
      false
  )
(******************************************************************************************************)

let createTrainingData config doc =
  use fpdb = new FamDB(config)
  prepareFileForTraining fpdb config doc
  doc
(******************************************************************************************************)

let createTrainingDataAndTryToLearn config doc =
  use fpdb = new FamDB(config)
  prepareFileForTraining fpdb config doc
  while tryToLearn fpdb config do ()
  doc
(******************************************************************************************************)

let reLearn config doc =
  use fpdb = new FamDB(config)
  let config = { config with minNewFilesBeforeRetrain = 0; trainWheneverMinNewFilesAreAvailable = true }
  tryToLearn fpdb config |> ignore
  doc
(******************************************************************************************************)

let tryToLearn config doc =
  use fpdb = new FamDB(config)
  tryToLearn fpdb config |> ignore
  doc
(******************************************************************************************************)