module CalculatePostVerificationStats.App

open Argu
open System.Diagnostics
open System.IO
open System.Runtime.InteropServices
open System.Text

open Extract
open Extract.Licensing
open Extract.Utilities.FSharp

open Args
open FileSetComparer
open Database


type NonThrowingExiter() =
    interface IExiter with
        member __.Name = "Exiter"

        member __.Exit(msg, code) =
            if code = ErrorCode.HelpText then
                printfn "%s" msg
                exit 0
            else
                printfn "%s" msg
                exit -1

let run
    (documentSourceFactory: DatabaseInfo -> IDocumentSource)
    (comparerFactory: DatabaseInfo -> string -> string -> int -> IFileSetComparer)
    (exiter: IExiter option)
    argv
    =
    try
        let parser =
            match exiter with
            | Some exiter ->
                ArgumentParser.Create<CalculatePostVerificationStatsArgs>(
                    programName = "CalculatePostVerificationStats.exe",
                    errorHandler = exiter
                )
            | None ->
                ArgumentParser.Create<CalculatePostVerificationStatsArgs>(
                    programName = "CalculatePostVerificationStats.exe"
                )

        let results = parser.ParseCommandLine(inputs = argv, raiseOnUsage = true)

        let dbServer, dbName = results.GetResult CalculatePostVerificationStatsArgs.Database
        let inputAction = results.GetResult Input_Action

        let limit =
            results.TryGetResult Limit_To_First
            |> Option.map (fun s ->
                match System.Int32.TryParse s with
                | true, n -> Some n
                | _ -> None)
            |> Option.flatten

        let expectedName = results.GetResult Expected_Attribute_Set
        let foundName = results.GetResult Found_Attribute_Set
        let outputFile = results.TryGetResult Output_File

        let parallelDegree =
            results.TryGetResult Parallel_Degree
            |> Option.defaultValue 8

        let dataQuery =
            results.TryGetResult Data_Query
            |> Option.defaultValue "/*/HCData|/*/MCData|/*/LCData|/*/Manual"

        let flaggedQuery =
            results.TryGetResult Flagged_Query
            |> Option.defaultValue "/*/HCData|/*/MCData|/*/LCData|/*/Clues"

        let dbInfo =
            { Server = dbServer
              Name = dbName
              VerifyActions = [ inputAction ]
              ExpAttributeSet = expectedName
              FndAttributeSet = foundName }

        let db = documentSourceFactory dbInfo

        let imageList =
            match limit with
            | Some number -> db.getFirstFilesCompleteForActions (number)
            | None -> db.getFilesCompleteForActions ()

        let fileComparer = comparerFactory dbInfo dataQuery flaggedQuery parallelDegree

        let sw = Stopwatch()
        sw.Start()

        let result = fileComparer.CompareRedactions(imageList)

        match outputFile with
        | Some file -> File.WriteAllLines(file, result.rulesTable |> Seq.map (String.concat ", "))
        | _ -> ()

        let sb = StringBuilder()

        result.ToString() |> sb.AppendLine |> ignore

        sprintf "Total seconds: %d" (sw.ElapsedMilliseconds / 1000L)
        |> sb.Append
        |> ignore

        sb.ToString(), 0

    with
    | :? COMException as uex ->
        let deserialized =
            ExtractException.FromStringizedByteStream("ELI53668", uex.Message)

        deserialized.Log()
        sprintf "ERROR! %s" deserialized.Message, -1
    | e -> sprintf "%A" e, -1

[<EntryPoint>]
let main argv =
    LicenseUtilities.LoadLicenseFilesFromFolder(0, MapLabel())

    // Setup dependencies
    let dbFactory = Database.Database |> memoize // Reuse the same Database to fulfill both interfaces
    let documentSourceFactory info = dbFactory info :> IDocumentSource

    let comparerFactory info dataQuery flaggedQuery parallelism =
        FileSetComparer(dbFactory info :> IAttributeSource, dataQuery, flaggedQuery, parallelism) :> IFileSetComparer

    let exiter = Some(NonThrowingExiter() :> IExiter)

    // Run the program
    let output, exitCode = run documentSourceFactory comparerFactory exiter argv

    if exitCode = 0 then
        printfn "%s" output
    else
        eprintfn "%s" output

    exitCode
