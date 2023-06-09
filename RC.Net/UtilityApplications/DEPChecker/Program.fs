﻿open System
open System.IO

open Extract.Licensing
open Extract.Utilities.FSharp

open SqliteIssueDetector

let usage error =
  let usageMessage = """USAGE: DEPChecker.exe [--help] [--verbose|-v] [<directory>|<assembly>|<configFile>|<batchFile>]"""
    
  if error then
    failwith (sprintf "Invalid args\r\n%s" usageMessage)
  else
    printfn "%s" usageMessage

let padResults (results: (string * string) list) =
  let maxPrefix =
    results
    |> Seq.map (fst >> String.length)
    |> Seq.max

  results
  |> Seq.map (fun (p, r) ->
    let padding =
      if p.Length = 0 then ""
      else String.replicate (maxPrefix - p.Length + 1) " "
    sprintf "%s%s%s" p padding r
  )

let getPrefix = function
| Warning _ -> "  Warning: "
| Error _ | Error2 _  -> "  Error:   "
| Failure _ -> "  Failure: "

let printResults verbose (fileInfos: FileInfo seq) =
  fileInfos
  |> Seq.iter (fun (fileInfo: FileInfo) ->
    if not (fileInfo.warningsAndErrors |> Seq.isEmpty) then
      printfn ""
      printfn "%s:" fileInfo.path
      let results =
        fileInfo.warningsAndErrors
        |> List.map (fun (message: Message) ->
          match message with
          | Error (description, queryInfo)
          | Warning (description, queryInfo) ->
              let sourceInfo =
                match queryInfo.sourceID with
                | ControlName controlName -> sprintf "%s%s.%s:" (getPrefix message) controlName queryInfo.queryType
                | LineNumber lineNum -> sprintf "%sline %d:" (getPrefix message) lineNum
              sourceInfo, sprintf "%s%s" description (if verbose then sprintf ": %s" queryInfo.queryText else "")
          | Error2 (description, value) ->
              sprintf "%s%s:" (getPrefix message) description,
              value
          | Failure (description, e) ->
              sprintf "%s%s:" (getPrefix message) description,
              sprintf "%O" e
        )
        |> List.distinct // If not printing query text there could be duplicates, since query = SQL node
        |> padResults
      printfn "%s" (results |> String.concat Environment.NewLine)
  )

[<EntryPoint; STAThread>]
let main argv =
  try
    if argv.Length = 0 || argv |> Seq.filter (Regex.isMatch """(?inx)^([-/][h?]|--help)$""") |> (not << Seq.isEmpty) then
      usage false
    else
      let verboseFlag, argv = argv |> Array.partition (Regex.isMatch """(?inx)^(-v|--verbose)$""")
      let verbose = verboseFlag |> (not << Seq.isEmpty)
      LicenseUtilities.LoadLicenseFilesFromFolder(0, MapLabel())

      let path = Path.GetFullPath (argv.[0].TrimEnd '"')
      if File.Exists path && path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) then
        DEPUtils.checkAssembly path |> Seq.singleton |> printResults verbose
      elif File.Exists path && path.EndsWith(".config", StringComparison.OrdinalIgnoreCase) then
        DEPUtils.checkConfigFile path |> Seq.singleton |> printResults verbose
      elif File.Exists path && path.EndsWith(".bat", StringComparison.OrdinalIgnoreCase) then
        BatchUtils.checkBatchFile path |> Seq.singleton |> printResults verbose
      elif Directory.Exists path then
        [ yield! DEPUtils.checkAssembliesInDir path
          yield! DEPUtils.checkConfigFilesInDir path
          yield! BatchUtils.checkBatchFilesInDir path ]
        |> printResults verbose
      else
        usage true
    0
  with e ->
    printfn "%O" e
    1
