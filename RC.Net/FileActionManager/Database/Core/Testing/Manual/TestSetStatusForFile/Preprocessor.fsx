module Preprocessor

#I @"C:\Engineering\Binaries\Debug"

#r "Extract.FileActionManager.Database.Test.dll"
#r "Extract.Testing.Utilities.dll"
#r "Extract.Utilities.dll"
#r "Interop.UCLID_AFCORELib.dll"
#r "Interop.UCLID_COMUtilsLib.dll"
#r "Interop.UCLID_FILEPROCESSINGLib.dll"
#r "Interop.UCLID_RASTERANDOCRMGMTLib.dll"

open System
open System.Diagnostics
open System.IO

open Extract.FileActionManager.Database.Test
open Extract.Utilities
open UCLID_AFCORELib
open UCLID_FILEPROCESSINGLib

/// Create DB with workflow and two actions and start a session
type FamDB(dbMgr: FAMTestDBManager<_>, dbName: string) =
  let fpDB = dbMgr.GetNewDatabase dbName
  let action1 = "Action1"
  let action2 = "Action2"

  // Setup DB
  do
    fpDB.DefineNewAction action1 |> ignore
    fpDB.DefineNewAction action2 |> ignore
    fpDB.AddWorkflow("Workflow1", EWorkflowType.kUndefined, action1, action2) |> ignore
    fpDB.AddWorkflow("Workflow2", EWorkflowType.kUndefined, action1, action2) |> ignore
    fpDB.ActiveWorkflow <- "Workflow1"
    fpDB.RecordFAMSessionStart("Test.fps", action1, true, true)
    fpDB.RegisterActiveFAM()

  with
    member _.SetActionState actionName actionState fileID =
      fpDB.SetStatusForFile (fileID, actionName, -1, actionState, false, false) |> ignore

    member _.AddFakeFile (fileNumber: int) =
      let fileName = Path.Combine(Path.GetTempPath(), sprintf "%03d" fileNumber)
      fpDB.AddFileNoQueue(fileName, 0L, 0, EFilePriority.kPriorityNormal, 1) |> ignore

    member _.Action1 = action1
    member _.Action2 = action2

    interface IDisposable with
      member _.Dispose() = 
        try
          fpDB.UnregisterActiveFAM()
          fpDB.RecordFAMSessionStop()
        with _ -> ()
        try
          dbMgr.RemoveDatabase dbName
        with _ -> ()
(******************************************************************************************************)

/// Measure the time it takes to set action statuses using multiple threads at once (for different files)
let runTest (doc: AFDocument): AFDocument =
  use dbManager = new FAMTestDBManager<obj>()
  let dbName = "Test_MeasureAddingFakeFilesAndSettingActionStatus"
  use fpdb = new FamDB(dbManager, dbName)
  let loglines = ResizeArray<_>()
  let log line = loglines.Add line
  let files = [|1..10000|]
  let sw = Stopwatch()

  for i in files do fpdb.AddFakeFile i
  CollectionMethods.Shuffle files

  sw.Start()

  files
  |> Array.Parallel.iter (fun fileID ->
    let actions = [|fpdb.Action1; fpdb.Action1; fpdb.Action2; fpdb.Action2|]
    let statuses = [|
      EActionStatus.kActionCompleted
      EActionStatus.kActionFailed
      EActionStatus.kActionPending
      EActionStatus.kActionProcessing
      EActionStatus.kActionSkipped
      EActionStatus.kActionUnattempted |]

    CollectionMethods.Shuffle statuses
    CollectionMethods.Shuffle actions

    actions
    |> Seq.iteri (fun i action ->
      fileID |> fpdb.SetActionState action statuses.[i]
    )
  )

  let setTime = sw.Elapsed.TotalSeconds
  log (sprintf "Test time: %.0f s" setTime)

  doc.Text.CreateNonSpatialString(loglines |> String.concat Environment.NewLine, "dummy")
  doc
