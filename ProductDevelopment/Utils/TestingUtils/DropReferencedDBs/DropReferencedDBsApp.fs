open Argu
open System.IO
open System.Runtime.InteropServices
open Extract

type CLIArguments =
  | [<Mandatory>] Search_Root of path:string
with
  interface IArgParserTemplate with
    member s.Usage =
      match s with
      | Search_Root _ -> "The path to recursively search for FPS files"

let parser = ArgumentParser.Create<CLIArguments>(programName = "DropReferencedDBs.exe")

let run argv =
  try
    let results = parser.ParseCommandLine(inputs = argv, raiseOnUsage = true)
    let root = results.GetResult Search_Root
    let fpsFiles = Directory.GetFiles(root, "*.fps", SearchOption.AllDirectories)
    fpsFiles
    |> Seq.iter (fun file ->
      try
        Fam.loadFrom file |> Fam.dropReferencedDB
      with e ->
        printfn "%s" e.Message
    )
  with
  | :? COMException as uex ->
    let deserialized = ExtractException.FromStringizedByteStream("ELI00000", uex.Message)
    deserialized.Log()
    printfn "ERROR! %s" deserialized.Message
    exit 1
  | e ->
      printfn "%s" e.Message
      exit 1
  0

[<EntryPoint>]
let main argv =
  run argv