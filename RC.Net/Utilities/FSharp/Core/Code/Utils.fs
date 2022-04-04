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
    if String.IsNullOrEmpty fname then None
    elif File.Exists fname then Some fname
    else None
(******************************************************************************************************)

type MaybeBuilder() =
    member this.Bind(m, f) = Option.bind f m

    member this.Return(x) = Some x

    member this.Zero() = None

    member this.ReturnFrom(m) = m

    member this.TryWith(body, handler) =
        try
            this.ReturnFrom(body ())
        with
        | e -> handler e

    member this.TryFinally(body, compensation) =
        try
            this.ReturnFrom(body ())
        finally
            compensation ()

    member this.Using(disposable: #System.IDisposable, body) =
        let body' = fun () -> body disposable

        this.TryFinally(
            body',
            fun () ->
                match disposable with
                | null -> ()
                | disp -> disp.Dispose()
        )

let maybe = MaybeBuilder()
(******************************************************************************************************)

type RetryBuilder(maxRetries, waitBetweenRetries: int) =
    member x.Return(a) = a // Enable 'return'

    member x.Delay(f) = f // Gets wrapped body and returns it (as it is)
    // so that the body is passed to 'Run'
    member x.Zero() = failwith "Zero" // Support if .. then

    member x.Run(f) = // Gets function created by 'Delay'
        let rec loop n =
            try
                f ()
            with
            | e ->
                if n = 1 then raise e
                Thread.Sleep(waitBetweenRetries)
                loop (n - 1)

        loop maxRetries

let retry = RetryBuilder(50, 500)
(******************************************************************************************************)

type RetryUntilTrueBuilder(maxRetries, waitBetweenRetries: int) =
    member x.Return(a) = a // Enable 'return'

    member x.Delay(f) = f // Gets wrapped body and returns it (as it is)
    // so that the body is passed to 'Run'
    member x.Zero() = false // Support if .. then

    member x.Run(f) = // Gets function created by 'Delay'
        let rec loop n =
            match f (), n with
            | true, _ -> true
            | false, 1 -> false
            | _ ->
                Thread.Sleep waitBetweenRetries
                loop (n - 1)

        loop maxRetries

let retryUntilTrue = RetryUntilTrueBuilder(50, 500)
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
            let other: NoCase = downcast obj
            System.StringComparer.InvariantCultureIgnoreCase.Compare(this.Value, other.Value)

    override this.ToString() = this.Value.ToString()
(******************************************************************************************************)

let memoize f =
    let cache = Dictionary<_, _>()

    fun x ->
        match cache.TryGetValue x with
        | found, res when found -> res
        | _ ->
            let res = f x
            cache.Add(x, res)
            res
(******************************************************************************************************)

module Regex =
    open System.Text.RegularExpressions

    let replace pat rep inp =
        Regex.Replace(input = inp, pattern = pat, replacement = rep)

    let replaceEnd pat rep inp =
        Regex.Replace(input = inp, pattern = pat, replacement = rep, options = RegexOptions.RightToLeft)

    let isMatch pat inp =
        Regex.IsMatch(input = inp, pattern = pat)

    let isMatchRev pat inp =
        Regex.IsMatch(input = inp, pattern = pat, options = RegexOptions.RightToLeft)

    let isMatchStartingAt pat startIdx inp =
        Regex(pat)
            .IsMatch(input = inp, startat = startIdx)

    let countMatches pat inp =
        Regex.Matches(input = inp, pattern = pat).Count

    let split pat inp = Regex.Split(input = inp, pattern = pat)

    let splitRev pat inp =
        Regex.Split(input = inp, pattern = pat, options = RegexOptions.RightToLeft)

    let findAllMatches pat inp =
        Regex.Matches(input = inp, pattern = pat)
        |> Seq.cast<Match>

    let findAllMatchesRev pat inp =
        Regex.Matches(input = inp, pattern = pat, options = RegexOptions.RightToLeft)
        |> Seq.cast<Match>

    let escape = Regex.Escape
(******************************************************************************************************)

module String =
    let toBytes (x: string) = System.Text.Encoding.UTF8.GetBytes x

    let ofBytes (bytes: byte array) =
        System.Text.Encoding.UTF8.GetString bytes

    let toLower (x: string) = x.ToLowerInvariant()

    let containsIgnoreCase needle haystack =
        (haystack |> toLower).Contains(needle |> toLower)

    (****************************************************************************************************)
    let private calculateDistMatrix lev allowInsertedWhitespace stopAt (strOne: string) (strTwo: string) =
        let (distArray: int [,]) =
            Array2D.zeroCreate (strOne.Length + 1) (strTwo.Length + 1)

        for i = 0 to strOne.Length do
            distArray.[i, 0] <- i

        if lev then
            for j = 0 to strTwo.Length do
                distArray.[0, j] <- j

        let mutable stop = false

        for j = 1 to strTwo.Length do
            if stop then
                distArray.[strOne.Length, j] <- strOne.Length
            else
                let currentChar = strTwo.[j - 1]

                let insertionCost =
                    if allowInsertedWhitespace
                       && System.Char.IsWhiteSpace currentChar then
                        0
                    else
                        1

                for i = 1 to strOne.Length do
                    distArray.[i, j] <-
                        if strOne.[i - 1] = currentChar then
                            distArray.[i - 1, j - 1]
                        else
                            List.min (
                                [ distArray.[i - 1, j] + 1
                                  distArray.[i, j - 1] + insertionCost
                                  distArray.[i - 1, j - 1] + 1 ]
                            )

                match stopAt with
                | Some allowedErrors when distArray.[strOne.Length, j] <= allowedErrors -> stop <- true
                | _ -> ()

        distArray

    let levDist strOne strTwo =
        let distArray = calculateDistMatrix true false None strOne strTwo
        distArray.[strOne.Length, strTwo.Length]

    let sellerDist strOne stopAt allowInsertedWhitespace strTwo =
        let distArray =
            calculateDistMatrix false allowInsertedWhitespace (Some stopAt) strOne strTwo

        let lastRow = distArray.[strOne.Length, *]
        Array.min lastRow

    let fuzzyContains needle allowedErrors haystack =
        sellerDist needle allowedErrors true haystack
        <= allowedErrors

    let fuzzyContainsIgnoreCase needle allowedErrors haystack =
        fuzzyContains (needle |> toLower) allowedErrors (haystack |> toLower)

    let fuzzyContainsOneOfIgnoreCase needles allowedErrors haystack =
        let haystack = haystack |> toLower

        needles
        |> List.exists (fun needle -> fuzzyContains (needle |> toLower) allowedErrors haystack)
(******************************************************************************************************)

open Newtonsoft.Json
open System.Linq

module Object =
    let toJson<'a> (x: 'a) =
        JsonConvert.SerializeObject(x, Formatting.Indented)

    let toJsonFile<'a> (fileName: string) (x: 'a) =
        let json = JsonConvert.SerializeObject(x, Formatting.Indented)
        retry { return File.WriteAllText(fileName, json) }

    let ofJson<'a> (json: string) = JsonConvert.DeserializeObject<'a> json

    let ofJsonFile<'a> (fileName: string) : 'a =
        File.ReadAllText fileName
        |> JsonConvert.DeserializeObject<'a>
(******************************************************************************************************)

module FsPickler =
    open MBrace.FsPickler
    let serializer = BinarySerializer()

    let toMemoryStream (x: 'a) =
        let stream = new MemoryStream()
        serializer.Serialize(stream, x)
        stream

    let toBytes (x: 'a) =
        use stream = x |> toMemoryStream
        stream.ToArray()

    let ofStream<'a> (stream: Stream) : 'a = serializer.Deserialize<'a>(stream)

    let ofBytes<'a> (bytes: byte array) : 'a =
        use stream = new MemoryStream(bytes)
        ofStream stream
(******************************************************************************************************)

[<AutoOpen>]
module Ex =
    open System.Reflection

    type Ex =
        /// Modify the exception, preserve the stacktrace and add the current stack, then throw
        /// This puts the origin point of the exception on top of the stacktrace.
        static member inline throwPreserve ex =
            let preserveStackTrace =
                typeof<Exception>.GetMethod
                    ("InternalPreserveStackTrace", BindingFlags.Instance ||| BindingFlags.NonPublic)

            (ex, null)
            |> preserveStackTrace.Invoke // alters the exn, preserves its stacktrace
            |> ignore

            raise ex
(******************************************************************************************************)

// Break a sequence into batches of the specified size
// http://www.fssnip.net/1o/title/Break-sequence-into-nelement-subsequences
module Seq =
    let groupsOfAtMost (size: int) (s: seq<'v>) : seq<list<'v>> =
        seq {
            let en = s.GetEnumerator()
            let more = ref true

            while more.Value do
                let group =
                    [ let i = ref 0

                      while i.Value < size && en.MoveNext() do
                          yield en.Current
                          i.Value <- i.Value + 1 ]

                if List.isEmpty group then
                    more.Value <- false
                else
                    yield group
        }
(******************************************************************************************************)

module Array =
    let shuffle (rng: System.Random option) (lst: #IList<'a>) =
        let Swap i j =
            let item = lst.[i]
            lst.[i] <- lst.[j]
            lst.[j] <- item

        let rng =
            match rng with
            | Some r -> r
            | None -> System.Random()

        let ln = lst.Count

        [ 0 .. (ln - 2) ]
        |> Seq.iter (fun i -> Swap i (rng.Next(i, ln)))

        lst

    module SlightlyParallel =
        let opts =
            System.Threading.Tasks.ParallelOptions(MaxDegreeOfParallelism = Environment.ProcessorCount * 2)

        let map f (a: 'a array) =
            let len = a.Length
            let result = Array.zeroCreate len

            System.Threading.Tasks.Parallel.For(0, len, opts, (fun i -> result.[i] <- f a.[i]))
            |> ignore

            result
(******************************************************************************************************)

module ResizeArray =
    module SlightlyParallel =
        let opts =
            System.Threading.Tasks.ParallelOptions(MaxDegreeOfParallelism = Environment.ProcessorCount * 2)

        let map f (a: ResizeArray<'a>) : ResizeArray<'b> =
            let len = a.Count
            let result = ResizeArray<_>(len)
            result.AddRange(Enumerable.Repeat(Unchecked.defaultof<'b>, len))

            System.Threading.Tasks.Parallel.For(0, len, opts, (fun i -> result.[i] <- f a.[i]))
            |> ignore

            result
(******************************************************************************************************)

let private makePattern =
    Regex.escape
    >> Regex.replace """\d""" """\d"""
    >> Regex.replace """\\\+""" "[+-]"
(******************************************************************************************************)

let private getTimeInfoForCurrentTime () =
    let utcNow = DateTime.UtcNow
    let localNow = utcNow.ToLocalTime()

    let zoneOffset =
        let zone = TimeZoneInfo.Local.GetUtcOffset localNow

        let prefix =
            match (zone < TimeSpan.Zero) with
            | true -> "-"
            | _ -> "+"

        prefix + zone.ToString "hhmm"

    utcNow, localNow, zoneOffset

open System.Globalization

let dayStampForFile () =
    let utcNow, localNow, zoneOffset = getTimeInfoForCurrentTime ()
    let utcString = utcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
    let localString = localNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
    sprintf "%s%s=%s" utcString zoneOffset localString
(******************************************************************************************************)

let dayStampForFilePat = dayStampForFile () |> makePattern
(******************************************************************************************************)

let timeStampForFile () =
    let utcNow, localNow, zoneOffset = getTimeInfoForCurrentTime ()
    let utcString = utcNow.ToString("yyyy-MM-dd.HH.mm.ss", CultureInfo.InvariantCulture)

    let localString =
        localNow.ToString("yyyy-MM-dd.HH.mm.ss", CultureInfo.InvariantCulture)

    sprintf "%s%s=%s" utcString zoneOffset localString

let timeStampForFilePat = timeStampForFile () |> makePattern
(******************************************************************************************************)

// Return folder names that start with the supplied regex pattern
let getFoldersStartingWithPattern pattern parentFolderPath =
    parentFolderPath
    |> Directory.GetDirectories
    |> Array.filter (fun dir ->
        Path.GetFileName dir
        |> Regex.isMatch ("^" + pattern))
(******************************************************************************************************)

// Return file names that start with the supplied regex pattern
let getFilesStartingWithPattern pattern folderPath =
    folderPath
    |> Directory.GetFiles
    |> Array.filter (fun file ->
        Path.GetFileName file
        |> Regex.isMatch ("^" + pattern))
(******************************************************************************************************)

let timeStamp () =
    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
(******************************************************************************************************)

let processIDGen =
    let processID = Process.GetCurrentProcess().Id
    let machineName = Environment.MachineName
    fun () -> sprintf "%s.%05d" machineName processID
(******************************************************************************************************)

let threadIDGen =
    let processID = processIDGen ()
    fun () -> sprintf "%s.%03d" processID Thread.CurrentThread.ManagedThreadId
(******************************************************************************************************)

let logLinePrefixGen = fun () -> sprintf "%s %s" (timeStamp ()) (threadIDGen ())
(******************************************************************************************************)

let logFileSuffixGen =
    let processID = processIDGen ()
    fun () -> sprintf "%s.%s" (dayStampForFile ()) processID
(******************************************************************************************************)

type LogMessageType =
    | Information
    | Exception

type LogMsg = LogMessage of (LogMessageType * string)

type DeleteLogFileInfo =
    { SuffixPattern: string
      MaxFiles: int }

type Logger(logFile: string, ?logFileSuffixGen, ?logLinePrefixGen, ?maxLogFiles) =
    let mutable enabled = true

    let baseName, logFile =
        match logFileSuffixGen with
        | Some f ->
            let dir = Path.GetDirectoryName logFile
            let basename = Path.GetFileNameWithoutExtension logFile

            let extension =
                if Path.HasExtension logFile then
                    Path.GetExtension logFile
                else
                    ""

            basename, Path.Combine(dir, sprintf "%s_%s%s" basename (f ()) extension)
        | None -> "", logFile

    let deleteOldLogFilesIfRequired logDir =
        match maxLogFiles with
        | Some info ->
            let matchingFiles =
                logDir
                |> getFilesStartingWithPattern (sprintf "%s_%s" baseName info.SuffixPattern)
                |> Array.sort

            let extra = matchingFiles.Length - info.MaxFiles

            if extra > 0 then
                matchingFiles
                |> Seq.take extra
                |> Seq.iter (fun file ->
                    try
                        File.Delete file
                    with
                    | _ -> ())
        | _ -> ()

    let inner =
        MailboxProcessor.Start (fun inbox ->
            let rec loop () =
                async {
                    let! msg = inbox.Receive()

                    match msg with
                    | LogMessage (_msgType, line) ->
                        let logDir = logFile |> Path.GetDirectoryName
                        logDir |> Directory.CreateDirectory |> ignore

                        if not (File.Exists logFile) then
                            deleteOldLogFilesIfRequired logDir

                        File.AppendAllLines(logFile, [ line ])

                    return! loop ()
                }

            loop ())

    let log msgType msg =
        if enabled then
            let line =
                match logLinePrefixGen with
                | Some f -> f () + ": " + msg
                | None -> msg

            inner.Post(LogMessage(msgType, line))

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

    match startDir with
    | Some d -> procStartInfo.WorkingDirectory <- d
    | _ -> ()

    let logger = Logger(logFile)
    let outputs = List<string>()
    let errors = List<string>()

    let outputHandler (_sender: obj) (args: DataReceivedEventArgs) =
        if args.Data <> null then
            outputs.Add args.Data
            logger.Log(Information, args.Data)

    let errorHandler (_sender: obj) (args: DataReceivedEventArgs) =
        if args.Data <> null then
            errors.Add args.Data
            logger.Log(Exception, args.Data)

    let p = new Process(StartInfo = procStartInfo)
    p.OutputDataReceived.AddHandler(DataReceivedEventHandler outputHandler)
    p.ErrorDataReceived.AddHandler(DataReceivedEventHandler errorHandler)

    let started =
        try
            p.Start()
        with
        | ex ->
            ex.Data.Add("exeName", exeName)
            reraise ()

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

    match startDir with
    | Some d -> procStartInfo.WorkingDirectory <- d
    | _ -> ()

    let outputs = List<string>()
    let errors = List<string>()
    let outputHandler f (_sender: obj) (args: DataReceivedEventArgs) = f args.Data
    let p = new Process(StartInfo = procStartInfo)
    p.OutputDataReceived.AddHandler(DataReceivedEventHandler(outputHandler outputs.Add))
    p.ErrorDataReceived.AddHandler(DataReceivedEventHandler(outputHandler errors.Add))

    let started =
        try
            p.Start()
        with
        | ex ->
            ex.Data.Add("exeName", exeName)
            reraise ()

    if not started then
        failwithf "Failed to start process %s" exeName

    p.BeginOutputReadLine()
    p.BeginErrorReadLine()
    p.WaitForExit()
    let cleanOut l = l |> Seq.filter (fun l -> l <> null)
    p.ExitCode, cleanOut outputs, cleanOut errors
(******************************************************************************************************)

let startProc exeName args startDir =
    let procStartInfo =
        ProcessStartInfo(
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            UseShellExecute = false,
            CreateNoWindow = true,
            FileName = exeName,
            Arguments = args
        )

    match startDir with
    | Some d -> procStartInfo.WorkingDirectory <- d
    | _ -> ()

    let p = new Process(StartInfo = procStartInfo)

    let started =
        try
            p.Start()
        with
        | ex ->
            ex.Data.Add("exeName", exeName)
            reraise ()

    if not started then
        failwithf "Failed to start process %s" exeName
(******************************************************************************************************)

let private createTempFile extension =
    retry {
        let fname =
            let basename = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())

            match extension with
            | Some ext -> basename + ext
            | None -> basename + ".tmp"

        if File.Exists fname then
            failwith "This should almost never happen"
        else
            File.WriteAllText(fname, "")
            return fname
    }

type TempFile(?extension: string) =
    let fname = createTempFile extension

    interface IDisposable with
        member __.Dispose() =
            try
                retry { return File.Delete fname }
            with
            | ex -> debugRaise ex
    with
        member __.FileName = fname
(******************************************************************************************************)
