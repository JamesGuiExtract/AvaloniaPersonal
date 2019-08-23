open Argu
open CreateFAMArgs

open System.Runtime.InteropServices
open Extract

let run argv =
  try
    let parser = ArgumentParser.Create<CreateFAMArgs>(programName = "CreateFAM.exe")
    let results = parser.ParseCommandLine(inputs = argv, raiseOnUsage = true)
    let serverName, dbName = results.GetResult Database
    let actionName = results.GetResult Action_Name
    let famPath = results.GetResult FAM_Path
    let expandAttributes = 
      match results.TryGetResult <@ Expand_Attributes @> with
      | Some value -> value
      | _ -> false
    let keepRunning = 
      match results.TryGetResult <@ Keep_Running @> with
      | Some value -> value
      | _ -> false
    let forceQueuing = 
      match results.TryGetResult <@ Force_Queueing @> with
      | Some value -> value
      | _ -> false

    printf "Creating FAM object..."
    let fam = Fam.createFam serverName dbName actionName keepRunning
    printfn "Done."

    match results.TryGetResult <@ Workflow_Name @> with
    | Some name ->
      fam.ActiveWorkflow <- name
    | _ -> ()

    printf "Adding tasks..."
    // Add folder queuer, if needed
    match results.TryGetResult <@ Image_Folders @> with
    | Some paths ->
        paths
        |> Seq.iter (fun folder ->
          fam |> Fam.addQueuer (Fam.createFolderQueuer folder true keepRunning) ("queue " + folder) forceQueuing
        )
    | _ -> ()

    // Add file queuer, if needed
    match results.TryGetResult <@ Image_Lists @> with
    | Some paths ->
        paths
        |> Seq.iter (fun file ->
          fam |> Fam.addQueuer (Fam.createFileListQueuer file) ("queue " + file) forceQueuing
        )
    | _ -> ()

    // Process expected VOA before storing, if needed
    match results.TryGetResult <@ Process_Expected_Rules_Path @> with
    | Some (rulesPath, voaPath) ->
      fam |> Fam.addTask (Fam.createExecuteRules rulesPath (Some voaPath)) (sprintf "run %s with %s" rulesPath voaPath)
    | _ -> ()

    match (results.TryGetResult <@ Expected_Attribute_Set @>, results.TryGetResult <@ Expected_Attribute_Path @>) with
    | (Some name, Some path) ->
      fam |> Fam.addTask (Fam.createStoreAttributes path name expandAttributes) (sprintf "store %s to %s" path name)
    | _ -> ()

    // Backup found VOA before running rules, if needed
    match results.TryGetResult <@ Backup_Found_To_Path @> with
    | Some (sourcePath, destPath) ->
      fam |> Fam.addTask (Fam.createCopyIfMissingTask sourcePath destPath) (sprintf "copy %s to %s if not already there" sourcePath destPath)
    | _ -> ()

    // Run finding rules, if needed
    match results.TryGetResult <@ Rules_Paths @> with
    | Some paths ->
        paths
        |> Seq.iter (fun rulesPath ->
          fam |> Fam.addTask (Fam.createExecuteRules rulesPath None) (sprintf "run %s" rulesPath)
        )
    | _ -> ()

    // Process found VOA before storing, if needed
    match results.TryGetResult <@ Process_Found_Rules_Path @> with
    | Some (rulesPath, voaPath) ->
      fam |> Fam.addTask (Fam.createExecuteRules rulesPath (Some voaPath)) (sprintf "run %s with %s" rulesPath voaPath)
    | _ -> ()

    let foundAttributesInfo = (results.TryGetResult <@ Found_Attribute_Set @>, results.TryGetResult <@ Found_Attribute_Path @>)
    match foundAttributesInfo with
    | (Some name, Some path) ->
      fam |> Fam.addTask (Fam.createStoreAttributes path name expandAttributes) (sprintf "store %s to %s" path name)
    | _ -> ()

    let metadataSetters = results.GetResults <@ Set_Metadata_Field @>
    metadataSetters
    |> Seq.iter (fun (name, value) ->
      fam |> Fam.addTask (Fam.createSetMetadataField name value) (sprintf "set metadata field %s to %s" name value)
    )

    match results.TryGetResult <@ Set_Action_Status @> with
    | Some actionName ->
      fam |> Fam.addTask (Fam.createSetToPending actionName) (sprintf "set <SourceDocName> to pending for %s" actionName)
    | _ -> ()

    printfn "Done."

    printf "Saving FAM to %s..." famPath
    fam |> Fam.saveTo famPath
    printfn "Done."

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