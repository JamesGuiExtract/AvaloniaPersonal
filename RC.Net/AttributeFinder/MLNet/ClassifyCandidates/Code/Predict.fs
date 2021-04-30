module Extract.AttributeFinder.MLNet.ClassifyCandidates.Predict

open Configuration
open Extract
open Extract.Utilities.FSharp
open Extract.Utilities.FSharp.AFUtils
open Microsoft.ML
open Newtonsoft.Json
open System
open System.IO
open System.IO.Pipes
open System.Text
open System.Threading
open UCLID_AFCORELib
open UCLID_AFUTILSLib
open Extract.AttributeFinder

let private readMessage (pipeStream: NamedPipeClientStream): 'a option =
  let responseBuilder = StringBuilder()
  let messageBuffer = Array.zeroCreate 16
  let rec read () =
    let bytesRead = pipeStream.Read (messageBuffer, 0, messageBuffer.Length)
    Encoding.UTF8.GetString(messageBuffer, 0, bytesRead)
    |> responseBuilder.Append
    |> ignore

    if pipeStream.IsMessageComplete then
      let json = responseBuilder.ToString()
      if String.IsNullOrWhiteSpace json then
        None
      else
        try
          json |> JsonConvert.DeserializeObject<'a> |> Some
        with | _ -> None
    else
      read()
  read()
(************************************************************************************************************************)

let private writeMessage (pipeStream: NamedPipeClientStream) (message: 'a) =
  let messageBytes =
    message
    |> JsonConvert.SerializeObject
    |> Encoding.UTF8.GetBytes

  pipeStream.Write(messageBytes, 0, messageBytes.Length);
  pipeStream.WaitForPipeDrain();
(************************************************************************************************************************)

let private pipeExists (pipeName: string) =
  match Directory.GetFiles("""\\.\pipe""", pipeName) with
  | [||] -> false
  | _ -> true
(************************************************************************************************************************)

type PipeStatus = | PipeExists of string | PipeDoesNotExist of string | UnableToGetPipeName
// Compute a unique, deterministic, communication pipe for this model, if it exists, and return the pipe's status
let private getPipeName modelPath =
  try
    let modTime = File.GetLastWriteTimeUtc(modelPath).ToString("yyyy-MM-dd.HH.mm.ss")
    let pipeName = sprintf "Extract.MLNetQueue.%s" (modelPath + modTime |> Utils.hashString)
    if pipeName |> pipeExists then
      PipeExists pipeName
    else
      PipeDoesNotExist pipeName
  with
  | e ->
    let uex = ExtractException("ELI51582", "Application trace: Unable to get pipe name", e) in uex.Log()
    UnableToGetPipeName
(************************************************************************************************************************)

let retryUntilTrue = RetryUntilTrueBuilder (5, 200)

// Check for the named pipe associated with this model and if it doesn't exist then start a server for it
let private ensureServerIsRunning (modelPath: string) =
  let startServer pipeName =
    // Start a server instance
    let binFolder = Extract.Utilities.FileSystemMethods.CommonComponentsPath
    let serverPath = Path.Combine (binFolder, "MLNetQueue.exe")
    let args = sprintf """listen --model-path "%s" --pipe-name "%s" """ modelPath pipeName
    Utils.startProc serverPath args None
    // Allow time for pipe-creation
    Thread.Sleep 200
    retryUntilTrue { return pipeExists pipeName }

  // Try up to 10 times to find the named pipe for the model (or create a new server)
  // This is meant to allow for transient errors, e.g., when a model file is being updated/replaced
  let rec loop = function
  | 10 -> failwith "Could not create named pipe"
  | tries ->
    // The pipe name is a hash of the model path and the last write time for the model file
    // so recompute it each time through the loop in case the model has been modified
    match getPipeName modelPath with
    | PipeExists pipeName -> pipeName
    | PipeDoesNotExist pipeName ->
      use mutex = new Mutex(false, sprintf "%s.CreationMutex" pipeName)
      if mutex |> Utils.acquireMutex 5000 then
        // Check to see if the pipe now exists
        if pipeExists pipeName then
          mutex.ReleaseMutex()
          pipeName
        else
          try
            let success = startServer pipeName
            mutex.ReleaseMutex()
            if success then pipeName else loop (tries + 1)
          with | _ex -> mutex.ReleaseMutex(); loop (tries + 1)
      else loop (tries + 1) // Could not acquire the mutex
    | UnableToGetPipeName -> loop (tries + 1) // Model file might be temporarily missing so try again
  loop 0
(************************************************************************************************************************)

// Type 'shared' with the server (MLNetQueue) for communicating requests
// TODO: This should probably go in a shared lib or code file but at this time it is such a simple DTO that it easier to
// just repeat the definition
type PredictionRequest = {
  InputDataFile: string
  OutputDataFile: string
}

// Send request for prediction to the listening server (start a server if needed)
let private sendRequest (modelPath: string) (request: PredictionRequest) =
  let pipeName = ensureServerIsRunning modelPath
  use pipeStream = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut)
  try
    pipeStream.Connect 5000
    pipeStream.ReadMode <- PipeTransmissionMode.Message
    writeMessage pipeStream request
    readMessage pipeStream
  with | _ -> None
(************************************************************************************************************************)

/// Try up to 5 times to send the request without communication failures
/// If the request results in an Error message then fail immediately
let predictWithMLNetQueue (modelPath: string) (request: PredictionRequest) =
  let rec loop tries =
    match tries with
    | 5 -> failwith "Could not communicate with server"
    | _ ->
      match sendRequest modelPath request with
      | Some (Result.Ok _) -> ()
 // TODO: try serializing the exception in MLNetQueue.exe and reraising it here.
 // See https://github.com/SwensenSoftware/unquote/commit/95d53254c83c01b5c5d2eeac9a4ce4c9616e6478
 // and https://stackoverflow.com/a/41202215/236255
      | Some (Result.Error msg) -> failwith msg
      | None -> loop (tries + 1)
  loop 0
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
