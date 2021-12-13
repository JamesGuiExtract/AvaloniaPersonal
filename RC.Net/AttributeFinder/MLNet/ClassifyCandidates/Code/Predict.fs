module Extract.AttributeFinder.MLNet.ClassifyCandidates.Predict

open Microsoft.ML
open System
open System.IO
open System.IO.Pipes
open System.Threading

open Extract
open Extract.AttributeFinder
open Extract.Utilities.FSharp
open Extract.Utilities.FSharp.AFUtils
open UCLID_AFCORELib
open UCLID_AFUTILSLib

open Configuration

// Data needed to communicate a prediction request to the server
type ServerRequest = {
  ModelPath: string // Path to the ML.Net model file that the server operates on
  ServerName: string // A prefix to be used for named pipes and events
  ServerCreator: string -> unit // A function to create the server
  TimeToWaitForServerCreation: TimeSpan
  PredictionRequest: DTO.PredictionRequest // Information about what the server needs to do
}

let private pipeExists (pipeName: string) =
  match Directory.GetFiles("""\\.\pipe""", pipeName) with
  | [||] -> false
  | _ -> true
(************************************************************************************************************************)

type PipeStatus = | PipeExists of string | PipeDoesNotExist of string | UnableToGetPipeName
// Compute a unique, deterministic, communication pipe for this model, if it exists, and return the pipe's status
let private getPipeName (request: ServerRequest) =
  try
    let modTime = File.GetLastWriteTimeUtc(request.ModelPath).ToString("yyyy-MM-dd.HH.mm.ss")
    let pipeName = sprintf "%s.%s" request.ServerName (request.ModelPath + modTime |> Utils.hashString)
    if pipeName |> pipeExists then
      PipeExists pipeName
    else
      PipeDoesNotExist pipeName
  with
  | e ->
    let uex = ExtractException("ELI51582", "Application trace: Unable to get pipe name", e) in uex.Log()
    UnableToGetPipeName
(************************************************************************************************************************)

// Check for the named pipe associated with this model and if it doesn't exist then start a server for it
let private ensureServerIsRunning (request: ServerRequest) =

  // Number of times to check-for/try-to-create the service after failing
  // to either compute the pipe name, acquire mutex, or start the service
  let maxRetryTimes = 10

  let checkForPipeCreatedEvent pipeName =
    match EventWaitHandle.TryOpenExisting("""Local\""" + pipeName) with
    | true, pipeCreatedEvent ->
        pipeCreatedEvent.WaitOne request.TimeToWaitForServerCreation
    | _ -> false

  let startServer pipeName =
    // Create an event that the server will signal when it is ready to accept connections
    if checkForPipeCreatedEvent pipeName then
      true
    else
      use pipeCreatedEvent = new EventWaitHandle(false, EventResetMode.ManualReset, """Local\""" + pipeName)
      try
        request.ServerCreator pipeName
      with e ->
        let uex = ExtractException("ELI51752", "Could not start server!", e)
        raise uex

      // Wait for pipe to be created
      pipeCreatedEvent.WaitOne request.TimeToWaitForServerCreation

  // Try up to 10 times to find the named pipe for the model (or create a new server)
  // This is meant to allow for transient errors, e.g., when a model file is being updated/replaced
  let rec loop = function
  | tries when tries = maxRetryTimes -> failwith "Could not create named pipe"
  | tries ->
    // The pipe name is a hash of the model path and the last write time for the model file
    // so recompute it each time through the loop in case the model has been modified
    match getPipeName request with
    | PipeExists pipeName -> pipeName
    | PipeDoesNotExist pipeName ->
      use mutex = new Mutex(false, sprintf "%s.CreationMutex" pipeName)
      if mutex |> Utils.acquireMutex (int request.TimeToWaitForServerCreation.TotalMilliseconds) then
        // Check to see if the pipe now exists
        if pipeExists pipeName then
          mutex.ReleaseMutex()
          pipeName
        else
          let success =
            try
              startServer pipeName
            finally
              mutex.ReleaseMutex()
          if success then
            pipeName
          else
            loop (tries + 1)
      else loop (tries + 1) // Could not acquire the mutex
    | UnableToGetPipeName -> loop (tries + 1) // Model file might be temporarily missing so try again
  loop 0
(************************************************************************************************************************)

// Send request for prediction to the listening server (start a server if needed)
let private sendRequest (request: ServerRequest) =
  let pipeName = ensureServerIsRunning request
  use pipeStream = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut)
  try
    pipeStream.Connect 5000
    pipeStream.ReadMode <- PipeTransmissionMode.Message
    NamedPipe.writeMessage pipeStream request.PredictionRequest
    NamedPipe.tryReadMessage pipeStream
  with | _ -> None
(************************************************************************************************************************)

/// Try up to 5 times to send the request without communication failures
/// If the request results in an Error message then fail immediately
let sendRequestToServer (request: ServerRequest) =
  let rec loop tries =
    match tries with
    | 5 -> failwith "Could not communicate with server"
    | _ ->
      match sendRequest request with
      | Some (Result.Ok _) -> ()
      | Some (Result.Error pickledEx) ->
          let bytes = Convert.FromBase64String pickledEx
          let ex = bytes |> FsPickler.ofBytes
          Ex.throwPreserve ex
      | None -> loop (tries + 1)
  loop 0
(************************************************************************************************************************)

/// Send request to MLNetQueue.exe
let predictWithMLNetQueue (modelPath: string) (request: DTO.PredictionRequest) =
  // Function to start a server instance
  let serverCreator pipeName =
    let binFolder = Extract.Utilities.FileSystemMethods.CommonComponentsPath
    let serverPath = Path.Combine (binFolder, "MLNetQueue.exe")
    let args = sprintf """listen --model-path "%s" --pipe-name "%s" """ modelPath pipeName
    Utils.startProc serverPath args None

  let serverRequest = {
    ModelPath = modelPath
    ServerName = "Extract.MLNetQueue"
    ServerCreator = serverCreator
    TimeToWaitForServerCreation = TimeSpan.FromMinutes 5.
    PredictionRequest = request
  }

  sendRequestToServer serverRequest
(************************************************************************************************************************)

/// Classify candidate attributes for a document using an MLNetQueue server to do the work
let predict (modelConfig: ModelConfig) (doc: AFDocument) =
  let afUtil = AFUtilityClass()
  let sdn = doc.Text.SourceDocName
  let modelPath = sprintf """%s\MLModel_%s.zip""" modelConfig.modelFolder modelConfig.modelName
  let candidateVoa =
    match modelConfig.candidateMode with
    | ReadFromDocOnly -> doc.Attribute.SubAttributes
    | ReadFromDocWriteToExt candidateExt ->
        let attrr = doc.Attribute.SubAttributes
        AttributeMethods.SaveAttributes (attrr, sdn + candidateExt)
        attrr
    | ReadFromExt candidateExt -> loadVoa afUtil (sdn + candidateExt)

  if candidateVoa.Size() = 0 then
    doc.Attribute.SubAttributes <- candidateVoa
  else
    let candidateVoa = FndAttributes candidateVoa
    let mlContext = MLContext()
    use inputDataFile = new TempFile()
    let inputDataPath = inputDataFile.FileName
    do
      let dataView = modelConfig.converter.ConvertToDataView mlContext None candidateVoa
      use fileStream = new FileStream(inputDataPath, FileMode.OpenOrCreate)
      mlContext.Data.SaveAsBinary(dataView, fileStream)

    use outputDataFile = new TempFile()
    let outputDataPath = outputDataFile.FileName

    predictWithMLNetQueue modelPath { InputDataFile = inputDataPath; OutputDataFile = outputDataPath }

    let dataView = mlContext.Data.LoadFromBinary outputDataPath
    try
      let (FndAttributes voa) = modelConfig.converter.ConvertFromDataView mlContext candidateVoa dataView
      doc.Attribute.SubAttributes <- voa
    finally
      match dataView with | :? IDisposable as d -> d.Dispose() | _ -> ()
  doc
(************************************************************************************************************************)
