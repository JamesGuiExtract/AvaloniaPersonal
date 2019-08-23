open Argu
open CreateFAMDBArgs
open Extract
open System.Runtime.InteropServices

let run argv =
  try
    let parser = ArgumentParser.Create<CreateFAMDBArgs>(programName = "CreateFAMDB.exe")
    let results = parser.ParseCommandLine(inputs = argv, raiseOnUsage = true)
    let serverName, dbName = results.GetResult Database
    let famDB =
      if FamDB.exists serverName dbName
      then
        printfn "Existing DB %s found on %s" dbName serverName
        FamDB.connectDB serverName dbName
      else
        printfn "Creating DB %s on %s" dbName serverName
        FamDB.createDB serverName dbName

    match results.TryGetResult <@ Action_Names @> with
    | Some names ->
        names
        |> Seq.iter (fun name ->
          try
            famDB |> FamDB.addAction name
          with
          | :? COMException as uex ->
            let deserialized = ExtractException.FromStringizedByteStream("ELI00000", uex.Message)
            deserialized.Log()
            printfn "ERROR! %s" deserialized.Message
        )
    | _ -> ()

    match results.TryGetResult <@ Attribute_Sets @> with
    | Some names ->
        names
        |> Seq.iter (fun name ->
          try
            famDB |> FamDB.addAttributeSet name
          with
          | :? COMException as uex ->
            let deserialized = ExtractException.FromStringizedByteStream("ELI00000", uex.Message)
            deserialized.Log()
            printfn "ERROR! %s" deserialized.Message
        )
    | _ -> ()

    match results.TryGetResult <@ Metadata_Fields @> with
    | Some names ->
        names
        |> Seq.iter (fun name ->
          try
            famDB |> FamDB.addMetadataField name
          with
          | :? COMException as uex ->
            let deserialized = ExtractException.FromStringizedByteStream("ELI00000", uex.Message)
            deserialized.Log()
            printfn "ERROR! %s" deserialized.Message
        )
    | _ -> ()

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