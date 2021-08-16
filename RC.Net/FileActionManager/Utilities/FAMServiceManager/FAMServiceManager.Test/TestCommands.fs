[<NUnit.Framework.Category("FAMServiceManager")>]
module Extract.FileActionManager.Utilities.FAMServiceManager.Test.Commands

open Extract.FileActionManager.Utilities.FAMServiceManager
open Extract.Testing.Utilities
open Extract.Utilities.FSharp
open NUnit.Framework
open Swensen.Unquote
open System
open System.IO
open System.Management
open System.Windows
open Extract.Utilities

[<OneTimeSetUp>]
let Setup () =
  GeneralMethods.TestSetup();


// Helper types/functions
let private removeServiceAndDB name =
  try
    let mgmtObj = FAMService.getFamService name
    if mgmtObj.["State"] <> box "Stopped" then
      stopService name |> Async.RunSynchronously |> ignore
  with | _ -> ()
  try
    remove name
    |> Async.RunSynchronously
    |> ignore
  with | _ -> ()
  try
    let databaseName = sprintf """C:\ProgramData\Extract Systems\ESFAMService\%s.sqlite""" name
    File.Delete databaseName
  with | _ -> ()

type private TempService(name, displayName) =
  interface IDisposable with
    member _.Dispose() = removeServiceAndDB name
  with
    member _.Name = name
    member _.DisplayName = displayName

let private installTempService () =
  let name, displayName = Guid.NewGuid().ToString(), Guid.NewGuid().ToString()
  let msg = 
    install (name, displayName)
    |> Async.RunSynchronously
  match msg with
  | InstallSuccess _ -> new TempService(name, displayName)
  | _ -> failwith "Failed to install temp service"

let private retry = RetryBuilder (10, 500)

let private waitForServiceState (targetState: string) (transitionState: string option) (service: TempService) =
  let mutable state = "Unknown"
  let mutable transitionStateSeen = false

  let transitioning state =
    transitionState |> Option.map (fun s -> s = state) |> Option.defaultValue false

  // Handle service starting, then stopped without seeing running state
  let transitioned state =
    transitionStateSeen && transitionState |> Option.map (fun s -> s <> state) |> Option.defaultValue false

  try
    retry {
      let mgmtObj = FAMService.getFamService service.Name
      state <- string mgmtObj.["State"]
      if state = targetState then
        return ()
      elif transitioned state then
        return ()
      elif transitioning state then
        transitionStateSeen <- true
        failwith "Do retry"
      else
        failwith "Do retry"
    }
  with | _ -> ()
  state

let private runCmd cmd =
  let output = ref ""
  let errors = ref ""
  let retVal = SystemMethods.RunExecutable(cmd, output, errors)
  if retVal <> 0
  then failwith (if String.IsNullOrWhiteSpace errors.Value then output.Value else errors.Value)
  !output

let private runCmdf fmt = 
  Printf.kprintf runCmd fmt

let private isDelayedStart (info: ManagementBaseObject) =
  let serviceName = string info.["Name"]
  let details = runCmdf "sc qc \"%s\"" serviceName
  details |> Regex.isMatch """AUTO_START +\(DELAYED\)"""

// Interactive tests
[<Category("Interactive")>]
module Interactive =

  [<Test; Category("Interactive")>]
  let ``OpenServices runs services msc`` () =
    let msg = openServices () |> Async.RunSynchronously
    
    test <@ msg = Msg.OpenServicesSuccess @>

    // Let the window open
    System.Threading.Thread.Sleep 2000

    test <@ MessageBox.Show("Was services.msc run?",
              "Confirm services.msc",
              MessageBoxButton.YesNo,
              MessageBoxImage.Question,
              MessageBoxResult.Yes) = MessageBoxResult.Yes @>


  [<Test; Category("Interactive")>]
  let ``Edit opens SQLCDBEditor for the DB`` () =
    use service = installTempService ()

    let editTask =
      async {
        let! msg = edit service.Name
        test <@ msg = Msg.LaunchConfigEditorSuccess(service.Name) @>
      } |> Async.StartAsTask

    // Let the window open
    System.Threading.Thread.Sleep 1000

    test <@ MessageBox.Show(sprintf "Was the SQLCDBEditor for %s.sqlite opened?" service.Name,
              "Confirm editor",
              MessageBoxButton.YesNo,
              MessageBoxImage.Question,
              MessageBoxResult.Yes) = MessageBoxResult.Yes @>

    MessageBox.Show("Close the SQLCDBEditor and this dialog to complete the test", "Close editor", MessageBoxButton.OK)
    |> ignore

    editTask |> Async.AwaitTask |> ignore


// Automated Tests
[<Category("Automated")>]
module Automated =

  [<Test>]
  let ``Install installs a new service`` () =
    let name, displayName = Guid.NewGuid().ToString(), Guid.NewGuid().ToString()
    try
      let msg = 
        install (name, displayName)
        |> Async.RunSynchronously
      
      test <@ msg = Msg.InstallSuccess name @>
    finally
      removeServiceAndDB name


  [<Test>]
  let ``SetStartMode changes start mode from Manual to Auto`` () =
    use service = installTempService ()

    do // Confirm initial state
      let mgmtObj = FAMService.getFamService service.Name
      test <@ mgmtObj.["StartMode"] = box "Manual" @>

    // Test transistion
    setStartMode(service.Name, StartMode.Auto) |> Async.RunSynchronously |> ignore
    let mgmtObj = FAMService.getFamService service.Name
    test <@ mgmtObj.["StartMode"] = box "Auto" @>
    test <@ mgmtObj |> isDelayedStart = false @>


  [<Test>]
  let ``SetStartMode changes start mode from Manual to DelayedAuto`` () =
    use service = installTempService ()

    do // Confirm initial state
      let mgmtObj = FAMService.getFamService service.Name
      test <@ mgmtObj.["StartMode"] = box "Manual" @>

    // Test transistion
    setStartMode(service.Name, StartMode.DelayedAuto) |> Async.RunSynchronously |> ignore
    let mgmtObj = FAMService.getFamService service.Name
    test <@ mgmtObj.["StartMode"] = box "Auto" @>
    test <@ mgmtObj |> isDelayedStart = true @>


  [<Test>]
  let ``SetStartMode changes start mode from Manual to Disabled`` () =
    use service = installTempService ()

    do // Confirm initial state
      let mgmtObj = FAMService.getFamService service.Name
      test <@ mgmtObj.["StartMode"] = box "Manual" @>

    // Test transistion
    setStartMode(service.Name, StartMode.Disabled) |> Async.RunSynchronously |> ignore
    let mgmtObj = FAMService.getFamService service.Name
    test <@ mgmtObj.["StartMode"] = box "Disabled" @>


  [<Test>]
  let ``SetStartMode changes start mode from Auto to DelayedAuto`` () =
    use service = installTempService ()

    do // Set and confirm initial state
      setStartMode(service.Name, StartMode.Auto) |> Async.RunSynchronously |> ignore
      let mgmtObj = FAMService.getFamService service.Name
      test <@ mgmtObj.["StartMode"] = box "Auto" @>
      test <@ mgmtObj |> isDelayedStart = false @>

    // Test transistion
    setStartMode(service.Name, StartMode.DelayedAuto) |> Async.RunSynchronously |> ignore
    let mgmtObj = FAMService.getFamService service.Name
    test <@ mgmtObj.["StartMode"] = box "Auto" @>
    test <@ mgmtObj |> isDelayedStart = true @>


  [<Test>]
  let ``SetStartMode changes start mode from Auto to Manual`` () =
    use service = installTempService ()

    do // Set and confirm initial state
      setStartMode(service.Name, StartMode.Auto) |> Async.RunSynchronously |> ignore
      let mgmtObj = FAMService.getFamService service.Name
      test <@ mgmtObj.["StartMode"] = box "Auto" @>

    // Test transistion
    setStartMode(service.Name, StartMode.Manual) |> Async.RunSynchronously |> ignore
    let mgmtObj = FAMService.getFamService service.Name
    test <@ mgmtObj.["StartMode"] = box "Manual" @>


  [<Test>]
  let ``SetStartMode changes start mode from Auto to Disabled`` () =
    use service = installTempService ()

    do // Set and confirm initial state
      setStartMode(service.Name, StartMode.Auto) |> Async.RunSynchronously |> ignore
      let mgmtObj = FAMService.getFamService service.Name
      test <@ mgmtObj.["StartMode"] = box "Auto" @>

    // Test transistion
    setStartMode(service.Name, StartMode.Disabled) |> Async.RunSynchronously |> ignore
    let mgmtObj = FAMService.getFamService service.Name
    test <@ mgmtObj.["StartMode"] = box "Disabled" @>


  [<Test>]
  let ``SetStartMode changes start mode from DelayedAuto to Auto`` () =
    use service = installTempService ()

    do // Set and confirm initial state
      setStartMode(service.Name, StartMode.DelayedAuto) |> Async.RunSynchronously |> ignore
      let mgmtObj = FAMService.getFamService service.Name
      test <@ mgmtObj.["StartMode"] = box "Auto" @>
      test <@ mgmtObj |> isDelayedStart = true @>

    // Test transistion
    setStartMode(service.Name, StartMode.Auto) |> Async.RunSynchronously |> ignore
    let mgmtObj = FAMService.getFamService service.Name
    test <@ mgmtObj.["StartMode"] = box "Auto" @>
    test <@ mgmtObj |> isDelayedStart = false @>


  [<Test>]
  let ``SetStartMode changes start mode from DelayedAuto to Manual`` () =
    use service = installTempService ()

    do // Set and confirm initial state
      setStartMode(service.Name, StartMode.DelayedAuto) |> Async.RunSynchronously |> ignore
      let mgmtObj = FAMService.getFamService service.Name
      test <@ mgmtObj.["StartMode"] = box "Auto" @>
      test <@ mgmtObj |> isDelayedStart = true @>

    // Test transistion
    setStartMode(service.Name, StartMode.Manual) |> Async.RunSynchronously |> ignore
    let mgmtObj = FAMService.getFamService service.Name
    test <@ mgmtObj.["StartMode"] = box "Manual" @>


  [<Test>]
  let ``SetStartMode changes start mode from DelayedAuto to Disabled`` () =
    use service = installTempService ()

    do // Set and confirm initial state
      setStartMode(service.Name, StartMode.DelayedAuto) |> Async.RunSynchronously |> ignore
      let mgmtObj = FAMService.getFamService service.Name
      test <@ mgmtObj.["StartMode"] = box "Auto" @>
      test <@ mgmtObj |> isDelayedStart = true @>

    // Test transistion
    setStartMode(service.Name, StartMode.Disabled) |> Async.RunSynchronously |> ignore
    let mgmtObj = FAMService.getFamService service.Name
    test <@ mgmtObj.["StartMode"] = box "Disabled" @>


  // Test latent 'delayed' property
  [<Test>]
  let ``SetStartMode changes start mode from DelayedAuto to Disabled to Auto`` () =
    use service = installTempService ()

    do // Set and confirm initial state
      setStartMode(service.Name, StartMode.DelayedAuto) |> Async.RunSynchronously |> ignore
      let mgmtObj = FAMService.getFamService service.Name
      test <@ mgmtObj.["StartMode"] = box "Auto" @>
      test <@ mgmtObj |> isDelayedStart = true @>

    do // Test first transistion
      setStartMode(service.Name, StartMode.Disabled) |> Async.RunSynchronously |> ignore
      let mgmtObj = FAMService.getFamService service.Name
      test <@ mgmtObj.["StartMode"] = box "Disabled" @>

    do // Test second transistion
      setStartMode(service.Name, StartMode.Auto) |> Async.RunSynchronously |> ignore
      let mgmtObj = FAMService.getFamService service.Name
      test <@ mgmtObj.["StartMode"] = box "Auto" @>
      test <@ mgmtObj |> isDelayedStart = false @>


  [<Test>]
  let ``SetStartNameAndPassword changes start name to current user`` () =
    use service = installTempService ()
    let userName = sprintf """%s\%s""" Environment.UserDomainName Environment.UserName
    setStartNameAndPassword (service.Name, userName, "") |> Async.RunSynchronously |> ignore
    let mgmtObj = FAMService.getFamService service.Name
    test <@ mgmtObj.["StartName"] = box userName @>


  // Hard to test the success of setting the password...
  [<Test>]
  let ``SetStartNameAndPassword with password doesn't generate error`` () =
    use service = installTempService ()
    let userName = sprintf """%s\%s""" Environment.UserDomainName Environment.UserName
    setStartNameAndPassword (service.Name, userName, "abc") |> Async.RunSynchronously |> ignore
    let mgmtObj = FAMService.getFamService service.Name
    test <@ mgmtObj.["StartName"] = box userName @>


  [<Test>]
  let ``StartService starts the service`` () =
    use service = installTempService ()
    startService service.Name |> Async.RunSynchronously |> ignore
    test <@ service |> waitForServiceState "Running" (Some "Start Pending") = "Running" @>


  [<Test>]
  let ``StopService stops the service`` () =
    use service = installTempService ()

    // Confirm running state
    startService service.Name |> Async.RunSynchronously |> ignore
    test <@ service |> waitForServiceState "Running" (Some "Start Pending") = "Running" @>

    // Test stopping
    stopService service.Name |> Async.RunSynchronously |> ignore
    test <@ service |> waitForServiceState "Stopped" None = "Stopped" @>


  [<Test>]
  let ``RemoveService uninstalls the service`` () =
    let getMatchingService name =
      use searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Service")
      use results = searcher.Get()
      results
      |> Seq.cast<ManagementBaseObject>
      |> Seq.filter (fun info -> string info.["Name"] = name)
      |> Seq.toList

    use service = installTempService ()
    test <@ service.Name |> getMatchingService |> List.length = 1 @>

    // Test remove
    remove service.Name |> Async.RunSynchronously |> ignore
    test <@ service.Name |> getMatchingService |> List.length = 0 @>


  [<Test>]
  let ``Kill kills running service`` () =
    use service = installTempService ()

    // Start the service
    startService service.Name |> Async.RunSynchronously |> ignore
    test <@ service |> waitForServiceState "Running" (Some "Start Pending") = "Running" @>

    let serviceModel =
      let mgmtObj = FAMService.getFamService service.Name
      let pid = match mgmtObj.["ProcessId"] :?> uint32 with 0u -> None | pid -> Some pid
      { FAMService.init with
          Name = service.Name
          PID = pid }

    // Test killing
    let result = kill serviceModel |> Async.RunSynchronously
    test <@ match result with
            | ForceKillSuccess(name, pid::_, []) when name = service.Name && Some (uint32 pid) = serviceModel.PID -> true
            | _ -> false @>

    // Wait half a second to make sure the service status has been updated
    // https://extract.atlassian.net/browse/ISSUE-17417
    System.Threading.Thread.Sleep 500

    test <@ (FAMService.getFamService service.Name).["State"] = box "Stopped" @>


  [<Test>]
  let ``Kill kills starting service`` () =
    use service = installTempService ()

    // Start the service but don't wait
    startService service.Name |> Async.RunSynchronously |> ignore

    let serviceModel =
      let mgmtObj = FAMService.getFamService service.Name
      let pid = match mgmtObj.["ProcessId"] :?> uint32 with 0u -> None | pid -> Some pid
      { FAMService.init with
          Name = service.Name
          PID = pid }

    // Test killing
    let result = kill serviceModel |> Async.RunSynchronously
    test <@ match result with
            | ForceKillSuccess(name, [pid], []) when name = service.Name && Some (uint32 pid) = serviceModel.PID -> true
            | _ -> false @>

    // Wait half a second to make sure the service status has been updated
    // https://extract.atlassian.net/browse/ISSUE-17417
    System.Threading.Thread.Sleep 500

    test <@ (FAMService.getFamService service.Name).["State"] = box "Stopped" @>


  [<Test>]
  let ``Load loads installed services`` () =
    use service1 = installTempService ()
    use service2 = installTempService ()

    // Expect at least these two
    let expectedServices = set [service1.Name, service1.DisplayName; service2.Name, service2.DisplayName ]
    
    let installedServices =
      load ()
      |> Async.RunSynchronously
      |> List.map (fun s -> s.Name, s.DisplayName)
      |> set

    test <@ Set.intersect expectedServices installedServices = expectedServices @>


  [<Test>]
  let ``Load loads details from installed services when DB exists but handles non-existent DB as well`` () =
    use service1 = installTempService () 
    use service2 = installTempService ()

    // Remove DB for one service
    let databaseName = sprintf """C:\ProgramData\Extract Systems\ESFAMService\%s.sqlite""" service1.Name
    File.Delete databaseName
    
    let installedServices = load () |> Async.RunSynchronously
    let service1Model = installedServices |> List.find (fun s -> s.Name = service1.Name)
    let service2Model = installedServices |> List.find (fun s -> s.Name = service2.Name)

    // First service has no details
    test <@ (service1Model.FPSData, service1Model.Settings) = ([], Map.empty) @>

    // Second service has settings
    test <@ service2Model.Settings <> Map.empty @>
