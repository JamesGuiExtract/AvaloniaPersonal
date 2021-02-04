[<AutoOpen>]
module Extract.Utilities.FSharp.Utils

open System
open System.Collections.Generic
open System.Diagnostics
open System.IO
open System.Threading

let debugFail<'a> : 'a =
  #if DEBUG
  failwith "Should never happen"
  #else
  Unchecked.defaultof<'a>
  #endif

let debugRaise<'a> ex : 'a =
  #if DEBUG
  raise ex
  #else
  Unchecked.defaultof<'a>
  #endif

let fileExists fname =
  if String.IsNullOrEmpty fname
  then None
  elif File.Exists fname
  then Some fname
  else None
(******************************************************************************************************)

type MaybeBuilder() =
  member this.Bind(m, f) = Option.bind f m

  member this.Return(x) = Some x

  member this.Zero() = None

  member this.ReturnFrom(m) = m

  member this.TryWith(body, handler) =
    try this.ReturnFrom(body()) with e -> handler e

  member this.TryFinally(body, compensation) =
    try this.ReturnFrom(body()) finally compensation() 

  member this.Using(disposable:#System.IDisposable, body) =
    let body' = fun () -> body disposable
    this.TryFinally(body', fun () -> 
      match disposable with 
      | null -> () 
      | disp -> disp.Dispose())

let maybe = MaybeBuilder ()
(******************************************************************************************************)

type RetryBuilder (maxRetries, waitBetweenRetries : int) = 
  member x.Return(a) = a               // Enable 'return'

  member x.Delay(f) = f                // Gets wrapped body and returns it (as it is)
                                       // so that the body is passed to 'Run'
  member x.Zero() = failwith "Zero"    // Support if .. then 

  member x.Run(f) =                    // Gets function created by 'Delay'
    let rec loop n  = 
      try f ()
      with e ->
        if n = 1 then raise e
        Thread.Sleep (waitBetweenRetries)
        loop (n-1)
    loop maxRetries

let retry = RetryBuilder (50, 500)
(******************************************************************************************************)

type NoCase(value) =
  member val Value = value

  override this.Equals(that) =
    match that with 
    | :? NoCase as other -> System.StringComparer.InvariantCultureIgnoreCase.Equals(this.Value, other.Value)
    | _ -> false

  override this.GetHashCode() =
    System.StringComparer.InvariantCultureIgnoreCase.GetHashCode(this.Value)

  interface System.IComparable with
    member this.CompareTo obj =
        let other : NoCase = downcast obj
        System.StringComparer.InvariantCultureIgnoreCase.Compare(this.Value, other.Value)
  
  override this.ToString() =
    this.Value.ToString()
(******************************************************************************************************)

let memoize f =
  let cache = Dictionary<_, _>()
  fun x ->
    match cache.TryGetValue x with
    | found, res when found -> res
    | _ ->
      let res = f x
      cache.Add (x, res)
      res
(******************************************************************************************************)

module Regex =
  open System.Text.RegularExpressions

  let replace pat rep inp =
    Regex.Replace(input=inp, pattern=pat, replacement=rep)

  let replaceEnd pat rep inp =
    Regex.Replace(input=inp, pattern=pat, replacement=rep, options=RegexOptions.RightToLeft)

  let isMatch pat inp =
    Regex.IsMatch(input=inp, pattern=pat)

  let isMatchRev pat inp =
    Regex.IsMatch(input=inp, pattern=pat, options=RegexOptions.RightToLeft)

  let isMatchStartingAt pat startIdx inp =
    Regex(pat).IsMatch(input=inp, startat=startIdx)

  let countMatches pat inp =
    Regex.Matches(input=inp, pattern=pat).Count

  let split pat inp =
    Regex.Split (input=inp, pattern=pat)

  let splitRev pat inp =
    Regex.Split (input=inp, pattern=pat, options=RegexOptions.RightToLeft)

  let findAllMatches pat inp =
    Regex.Matches (input=inp, pattern=pat)
    |> Seq.cast<Match>

  let findAllMatchesRev pat inp =
    Regex.Matches (input=inp, pattern=pat, options=RegexOptions.RightToLeft)
    |> Seq.cast<Match>

  let escape = Regex.Escape
(******************************************************************************************************)

module String =
  let toBytes (x: string) = System.Text.Encoding.UTF8.GetBytes x
  let ofBytes (bytes: byte array) = System.Text.Encoding.UTF8.GetString bytes
  let toLower (x: string) = x.ToLowerInvariant ()

  let containsIgnoreCase needle haystack =
    (haystack |> toLower).Contains (needle |> toLower)

  (****************************************************************************************************)
  let private calculateDistMatrix lev allowInsertedWhitespace stopAt (strOne : string) (strTwo : string) =
    let (distArray : int[,]) = Array2D.zeroCreate (strOne.Length + 1) (strTwo.Length + 1)
 
    for i = 0 to strOne.Length do distArray.[i, 0] <- i
    if lev then
      for j = 0 to strTwo.Length do distArray.[0, j] <- j

    let mutable stop = false
    for j = 1 to strTwo.Length do
      if stop then
        distArray.[strOne.Length, j] <- strOne.Length
      else
        let currentChar = strTwo.[j - 1]
        let insertionCost =
          if allowInsertedWhitespace && System.Char.IsWhiteSpace currentChar
          then 0 else 1
        for i = 1 to strOne.Length do
          distArray.[i, j] <-
            if strOne.[i - 1] = currentChar then
              distArray.[i - 1, j - 1]
            else
              List.min ([
                distArray.[i-1, j] + 1
                distArray.[i, j-1] + insertionCost
                distArray.[i-1, j-1] + 1
              ])
        match stopAt with
        | Some allowedErrors when distArray.[strOne.Length, j] <= allowedErrors ->
            stop <- true
        | _ -> ()
    distArray

  let levDist strOne strTwo =
    let distArray = calculateDistMatrix true false None strOne strTwo
    distArray.[strOne.Length, strTwo.Length]

  let sellerDist strOne stopAt allowInsertedWhitespace strTwo =
    let distArray = calculateDistMatrix false allowInsertedWhitespace (Some stopAt) strOne strTwo
    let lastRow = distArray.[strOne.Length, *]
    Array.min lastRow

  let fuzzyContains needle allowedErrors haystack =
    sellerDist needle allowedErrors true haystack <= allowedErrors

  let fuzzyContainsIgnoreCase needle allowedErrors haystack =
    fuzzyContains (needle |> toLower) allowedErrors (haystack |> toLower)

  let fuzzyContainsOneOfIgnoreCase needles allowedErrors haystack =
    let haystack = haystack |> toLower
    needles
    |> List.exists (fun needle ->
      fuzzyContains (needle |> toLower) allowedErrors haystack
    )
(******************************************************************************************************)

open Newtonsoft.Json

module Object =
  let toJson<'a> (x: 'a) = 
    JsonConvert.SerializeObject (x, Formatting.Indented)

  let toJsonFile<'a> (fileName: string) (x: 'a) = 
    let json = JsonConvert.SerializeObject (x, Formatting.Indented)
    retry { return File.WriteAllText (fileName, json) }

  let ofJson<'a> (json: string) =
    JsonConvert.DeserializeObject<'a> json

  let ofJsonFile<'a> (fileName: string): 'a = 
    File.ReadAllText fileName
    |> JsonConvert.DeserializeObject<'a>
(******************************************************************************************************)

module Array =
  let shuffle (rng: System.Random option) (lst: #IList<'a>) =
      let Swap i j =
          let item = lst.[i]
          lst.[i] <- lst.[j]
          lst.[j] <- item
      let rng = match rng with | Some r -> r | None -> System.Random ()
      let ln = lst.Count
      [0..(ln - 2)]
      |> Seq.iter (fun i -> Swap i (rng.Next(i, ln)))
      lst

  module SlightlyParallel =
    let opts = System.Threading.Tasks.ParallelOptions(MaxDegreeOfParallelism = Environment.ProcessorCount)

    let map f (a: 'a array) =
      let len = a.Length
      let result = Array.zeroCreate len
      System.Threading.Tasks.Parallel.For (0, len, opts, (fun i ->
        result.[i] <- f a.[i]
      )) |> ignore
      result

(******************************************************************************************************)

let dayStampForFile () = 
  let zone = TimeZoneInfo.Local.GetUtcOffset DateTime.Now
  let prefix = match (zone<TimeSpan.Zero) with | true -> "-" | _ -> "+"
  System.DateTime.UtcNow.ToString("yyyy-MM-dd") + prefix + zone.ToString("hhss");
(******************************************************************************************************)

let timeStampForFile () = 
  let zone = TimeZoneInfo.Local.GetUtcOffset DateTime.Now
  let prefix = match (zone<TimeSpan.Zero) with | true -> "-" | _ -> "+"
  System.DateTime.UtcNow.ToString("yyyy-MM-dd.HH.mm.ss") + prefix + zone.ToString("hhss");
(******************************************************************************************************)

let timeStamp () = 
  let zone = TimeZoneInfo.Local.GetUtcOffset DateTime.Now
  let prefix = match (zone<TimeSpan.Zero) with | true -> "-" | _ -> "+"
  System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + prefix + zone.ToString("hhss");
(******************************************************************************************************)

let processIDGen =
  let processID = Process.GetCurrentProcess().Id
  let machineName = Environment.MachineName
  fun () -> sprintf "%s.%d" machineName processID 
(******************************************************************************************************)

let threadIDGen =
  let processID = processIDGen ()
  fun () -> sprintf "%s.%d" processID Thread.CurrentThread.ManagedThreadId
(******************************************************************************************************)

let logLinePrefixGen =
  fun () -> sprintf "%s %s" (timeStamp ()) (threadIDGen ())
(******************************************************************************************************)

let logFileSuffixGen =
  let processID = processIDGen ()
  fun () -> sprintf "%s.%s" (dayStampForFile ()) processID
(******************************************************************************************************)

type LogMessageType = 
  | Information
  | Exception

type LogMsg =
  | LogMessage of (LogMessageType * string)
  
type Logger(logFile: string, logFileSuffixGen, exeName: string, logLinePrefixGen) =
  let mutable enabled = true
  let inner =
    MailboxProcessor.Start(fun inbox ->
      let rec loop () =
        async {
          let! msg = inbox.Receive()
          match msg with
          | LogMessage (msgType, line) ->
            let logFile =
              match logFileSuffixGen with
              | Some f ->
                if Path.HasExtension logFile
                then
                  let dir = Path.GetDirectoryName logFile
                  let basename = Path.GetFileNameWithoutExtension logFile
                  let extension = Path.GetExtension logFile
                  Path.Combine (dir, sprintf "%s_%s%s" basename (f ()) extension)
                else
                  sprintf "%s_%s" logFile (f ())
              | None -> logFile

            logFile |> Path.GetDirectoryName |> Directory.CreateDirectory |> ignore
            File.AppendAllLines (logFile, [line])

          return! loop ()
        }
      loop ())
  let log msgType msg =
    if enabled
    then
      let line =
        match logLinePrefixGen with
        | Some f -> f () + ": " + msg
        | None -> msg
      inner.Post (LogMessage (msgType, line))
  with
    member x.Log(msgType, msg) = log msgType msg
    member x.Enable() = enabled <- true
    member x.Disable() = enabled <- false
(******************************************************************************************************)

let runProcLogToFile exeName args startDir logFile priority = 
  let procStartInfo = 
    ProcessStartInfo(
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      UseShellExecute = false,
      CreateNoWindow = true,
      FileName = exeName,
      Arguments = args
    )
  match startDir with | Some d -> procStartInfo.WorkingDirectory <- d | _ -> ()

  let logger = Logger (logFile, None, exeName, None)
  let outputs = List<string>()
  let errors = List<string>()
  let outputHandler (_sender:obj) (args:DataReceivedEventArgs) =
    if args.Data <> null
    then
      outputs.Add args.Data
      logger.Log (Information, args.Data)
  let errorHandler (_sender:obj) (args:DataReceivedEventArgs) =
    if args.Data <> null
    then
      errors.Add args.Data
      logger.Log (Exception, args.Data)

  let p = new Process(StartInfo = procStartInfo)
  p.OutputDataReceived.AddHandler(DataReceivedEventHandler outputHandler)
  p.ErrorDataReceived.AddHandler(DataReceivedEventHandler errorHandler)
  let started = 
    try
      p.Start()
    with | ex ->
      ex.Data.Add("exeName", exeName)
      reraise()
  if not started then
    failwithf "Failed to start process %s" exeName
  p.PriorityClass <- priority
  p.BeginOutputReadLine()
  p.BeginErrorReadLine()
  p.WaitForExit()
  p.ExitCode, outputs, errors
(******************************************************************************************************)

let runProc exeName args startDir = 
  let procStartInfo = 
    ProcessStartInfo(
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      UseShellExecute = false,
      CreateNoWindow = true,
      FileName = exeName,
      Arguments = args
    )
  match startDir with | Some d -> procStartInfo.WorkingDirectory <- d | _ -> ()

  let outputs = List<string>()
  let errors = List<string>()
  let outputHandler f (_sender:obj) (args:DataReceivedEventArgs) = f args.Data
  let p = new Process(StartInfo = procStartInfo)
  p.OutputDataReceived.AddHandler(DataReceivedEventHandler (outputHandler outputs.Add))
  p.ErrorDataReceived.AddHandler(DataReceivedEventHandler (outputHandler errors.Add))
  let started = 
    try
      p.Start()
    with | ex ->
      ex.Data.Add("exeName", exeName)
      reraise()
  if not started then
    failwithf "Failed to start process %s" exeName
  p.BeginOutputReadLine()
  p.BeginErrorReadLine()
  p.WaitForExit()
  let cleanOut l = l |> Seq.filter (fun l -> l <> null)
  p.ExitCode, cleanOut outputs, cleanOut errors
(******************************************************************************************************)

let private createTempFile extension =
  retry {
    let fname =
      let basename = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())
      match extension with
      | Some ext -> basename + ext
      | None -> basename + ".tmp"

    if File.Exists fname
    then
      failwith "This should almost never happen"
    else
      File.WriteAllText (fname, "")
      return fname
  }

type TempFile(?extension: string) =
  let fname = createTempFile extension
  interface IDisposable with
    member __.Dispose() =
      try
        File.Delete fname
      with | ex -> debugRaise ex
  with member __.FileName = fname
(******************************************************************************************************)
