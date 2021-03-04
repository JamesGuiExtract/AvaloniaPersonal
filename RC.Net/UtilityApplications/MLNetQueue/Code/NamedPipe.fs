module NamedPipe

open Extract.Utilities.FSharp
open Newtonsoft.Json
open System
open System.IO
open System.IO.Pipes
open System.Text
open System.Threading

let readMessage (pipeStream: NamedPipeServerStream): 'a option =
  let responseBuilder = StringBuilder()
  let messageBuffer = Array.zeroCreate 16
  try
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
          json |> JsonConvert.DeserializeObject<'a> |> Some
      else
        read()
    read()
  with | :? IOException -> None
(************************************************************************************************************************)

let writeMessage (pipeStream: NamedPipeServerStream) (message: 'a) =
  let messageBytes =
    message
    |> JsonConvert.SerializeObject
    |> Encoding.UTF8.GetBytes

  pipeStream.Write(messageBytes, 0, messageBytes.Length);
  pipeStream.WaitForPipeDrain();
(************************************************************************************************************************)

type WaitResult = | WaitSuccess | TimedOut | ErroredOut of exn

let waitForConnection (pipeStream: NamedPipeServerStream) =
  // Wait up to a minute for a connection and then quit
  use timeoutTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes 1.)
  use connectedEvent = new AutoResetEvent(false)
  let mutable waitForConnectionException: Exception = null

  try
    pipeStream.BeginWaitForConnection((fun asyncResult ->
      try
        pipeStream.EndWaitForConnection asyncResult
        connectedEvent.Set() |> ignore
      with
      | e ->
        waitForConnectionException <- e
    ), null) |> ignore

    if WaitHandle.WaitAny([|connectedEvent; timeoutTokenSource.Token.WaitHandle|]) = 1 then
      try
        pipeStream.Dispose()
      with | _ -> ()
      TimedOut
    elif not (isNull waitForConnectionException) then
      try
        pipeStream.Dispose()
      with | _ -> ()
      raise waitForConnectionException
    else
      WaitSuccess
  with | e -> ErroredOut e

let listenForRequests (pipeName: string) (dispatch: 'TRequest -> 'TResult) =
  // Keep track of whether all tasks have completed or not
  let tasks = System.Collections.Concurrent.ConcurrentDictionary<_,_>()

  // Read a request from the stream on the thread pool and return immediately
  let handleConnection pipeStream =
    async {
      match readMessage pipeStream with
      | Some request ->
        try
          try
            tasks.TryAdd(request, None) |> ignore
            try
              let res = dispatch request
              writeMessage pipeStream (Result.Ok res)
            finally
              tasks.TryRemove request |> ignore
          with | e ->
            try
              // TODO: Serialize the whole exn?
              writeMessage pipeStream (Result.Error e.Message)
            with | _ -> ()
        finally
          pipeStream.Dispose()
      | None -> ()
    }
    |> Async.Start

  let rec loop () =
    let pipeStream = new NamedPipeServerStream(pipeName, PipeDirection.InOut, -1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
    match waitForConnection pipeStream with
    | WaitSuccess -> handleConnection pipeStream; loop ()
    | TimedOut when not tasks.IsEmpty -> loop () // If tasks are still running then keep waiting
    | TimedOut | ErroredOut _ -> ()
  loop ()
(************************************************************************************************************************)

