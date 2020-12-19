[<NUnit.Framework.Category("FAMServiceManager")>]
module Extract.FileActionManager.Utilities.FAMServiceManager.Test.Update

open Extract.FileActionManager.Utilities.FAMServiceManager
open Extract.Testing.Utilities
open Extract.Utilities.FSharp
open NUnit.Framework
open Swensen.Unquote
open System


[<OneTimeSetUp>]
let Setup () =
  GeneralMethods.TestSetup();

// Helper functions

// Arbitrary threshold to account for time performing the test
let isTimeClose (t: DateTimeOffset) (t': DateTimeOffset) =
  abs((t - t').TotalMilliseconds) < 1000.

// Make the actual time equal to the expected time if they are fairly close to account for time performing the test
let private adjustStatusMsgTimeStampsIfClose (adjustTo: DateTimeOffset) (m: Model, msgs: CmdMsg list) =
  let existingDates =
    m.StatusMsg
    |> Regex.findAllMatches """\d{1,2}/\d{1,2}/\d{4} \d{2}:\d{2}:\d{2} [AP]M [-+]\d{2}:\d{2}"""
    |> Seq.map (fun regexMatch -> DateTimeOffset.TryParse regexMatch.Value)
    |> Seq.filter fst
    |> Seq.map snd

  let adjustedMsg =
    (m.StatusMsg, existingDates)
    ||> Seq.fold (fun msg date ->
      if date |> isTimeClose adjustTo then
        msg.Replace(sprintf "%O" date, sprintf "%O" adjustTo)
      else
        msg
    )
  { m with StatusMsg = adjustedMsg }, msgs

// Make the actual time equal to the expected time if they are fairly close to account for time performing the test
let private adjustLastRefreshIfClose (adjustTo: DateTimeOffset) (m: Model, msgs: CmdMsg list) =
  let adjustedRefreshTime = 
    if abs((m.LastRefresh - adjustTo).TotalSeconds) < 2. then
      adjustTo
    else
      m.LastRefresh
  { m with LastRefresh = adjustedRefreshTime }, msgs

// Start with a modified initialModel to ensure update bases the updated state on the previous state (not Model.empty)
let initialModel =
  { Model.empty with
      Services = [FAMService.init]
      NewServiceName = "New srv"
      StatusMsg = "placeholder msg"
      LastRefresh = DateTimeOffset.Now - TimeSpan.FromDays 1. }


// Automated Tests
[<Category("Automated")>]
module Automated =

  [<Test>]
  let ``InitialState is an empty Model with a Refresh command message`` () =
    let expected = Model.empty, [CmdMsg.Refresh]
    let actual = Program.init()
    test <@ expected = actual @>


  [<Test>]
  let ``OpenServicesRequest sets StatusMsg and returns an OpenServices command message`` () =
    let expected = { initialModel with StatusMsg = "Opening services.msc..." }, [CmdMsg.OpenServices]
    let actual = initialModel |> update Msg.OpenServicesRequest
    test <@ actual = expected @>


  [<Test>]
  let ``OpenServicesSuccess sets StatusMsg`` () =
    let now = DateTimeOffset.Now
    let expected = { initialModel with StatusMsg = sprintf "Opened services.msc at %O" now }, []
    let actual = initialModel |> update Msg.OpenServicesSuccess
    let adjusted = actual |> adjustStatusMsgTimeStampsIfClose now // Make timestamps agree
    test <@ adjusted = expected @>


  [<Test>]
  let ``ManualRefreshRequest sets StatusMsg and returns a ManualRefresh command message`` () =
    let expected = { initialModel with StatusMsg = "Refreshing..." }, [CmdMsg.ManualRefresh]
    let actual = initialModel |> update Msg.ManualRefreshRequest
    test <@ actual = expected @>


  [<Test>]
  let ``ManualRefreshSuccess sets Services, clears StatusMsg and sets LastRefresh to the current time``() =
    let now = DateTimeOffset.Now
    // Start with some status msg and refresh time from a minute ago
    let initialModel =
      { initialModel with
          StatusMsg = "Error occurred"
          LastRefresh = now - TimeSpan.FromMinutes 1.
      }
    let loadedServices = [{FAMService.init with Name = "Service 1"}; {FAMService.init with Name = "Service 2"}]
    let expected =
      { initialModel with
          Services = loadedServices
          LastRefresh = now; StatusMsg = ""
      }, []
    let actual = initialModel |> update (Msg.ManualRefreshSuccess loadedServices)
    let adjusted = actual |> adjustLastRefreshIfClose now // Make timestamps agree
    test <@ adjusted = expected @>


  [<Test>]
  let ``RefreshSuccess behaves like ManualRefreshSuccess except it does not clear StatusMsg and it returns a RestartService command message for any stopped service that is in the ServicesToRestart set`` () =
    let initialModel = { initialModel with ServicesToRestart = set ["Service 3"; "Service 2"] }
    let loadedServices =
      [ {FAMService.init with Name = "Service 1"}
        {FAMService.init with Name = "Service 2"; State = Stopped}
        {FAMService.init with Name = "Service 3"} ]
    let now = DateTimeOffset.Now
    let expected =
      { initialModel with
          Services = loadedServices
          LastRefresh = now }, [CmdMsg.RestartService "Service 2"]
    let actual = initialModel |> update (Msg.RefreshSuccess loadedServices)
    let adjusted = actual |> adjustLastRefreshIfClose now
    test <@ adjusted = expected @>


  [<Test>]
  let ``SetNewServiceName sets NewServiceName`` () =
    let expected = { initialModel with NewServiceName = "Name typed by user" }, []
    let actual = initialModel |> update (Msg.SetNewServiceName "Name typed by user")
    test <@ actual = expected @>


  [<Test>]
  let ``SetNewServiceDisplayName sets NewServiceDisplayName`` () =
    let expected = { initialModel with NewServiceDisplayName = "Display Name typed by user" }, []
    let actual = initialModel |> update (Msg.SetNewServiceDisplayName "Display Name typed by user")
    test <@ actual = expected @>


  [<Test>]
  let ``InstallRequest clears NewServiceName and NewServiceDisplayName, sets StatusMsg and returns an Install command message`` () =
    let name, displayName = "NewService", "New Service"
    let expected =
      { initialModel with
          NewServiceName = ""
          NewServiceDisplayName = ""
          StatusMsg = sprintf "Installing %s..." name
      }, [CmdMsg.Install (name, displayName)]
    let actual = initialModel |> update (Msg.InstallRequest (name, displayName))
    test <@ actual = expected @>


  [<Test>]
  let ``InstallSuccess sets StatusMsg and returns a Refresh command message in order to load the new list of services`` () =
    let now = DateTimeOffset.Now
    let name = "NewService"
    let expected = { initialModel with StatusMsg = sprintf "Installed %s at %O" name now }, [Refresh]
    let actual = initialModel |> update (InstallSuccess name)
    let adjusted = actual |> adjustStatusMsgTimeStampsIfClose now
    test <@ adjusted = expected @>


  [<Test>]
  let ``RemoveRequest sets StatusMsg, updates the FAMService model to prevent double-click of the uninstall button and returns a Remove command message`` () =
    let name = "Service 1"
    let expected =
      { initialModel with
          StatusMsg = sprintf "Removing %s..." name
          Services =
            [ {FAMService.init with Name = "Service 1"; StateChangeInitiated = true}
              {FAMService.init with Name = "Service 2"} ]
      }, [Remove name]
    let initialModel =
      { initialModel with
          Services =
            [ {FAMService.init with Name = "Service 1"}
              {FAMService.init with Name = "Service 2"} ]
      }
    let actual = initialModel |> update (RemoveRequest name)
    test <@ actual = expected @>


  [<Test>]
  let ``RemoveSuccess sets StatusMsg and returns a Refresh command message in order to load the new list of services`` () =
    let now = DateTimeOffset.Now
    let name = "DeletedService"
    let expected = { initialModel with StatusMsg = sprintf "Removed %s at %O" name now }, [Refresh]
    let actual = initialModel |> update (RemoveSuccess name)
    let adjusted = actual |> adjustStatusMsgTimeStampsIfClose now
    test <@ adjusted = expected @>


  [<Test>]
  let ``ForceKillRequest sets StatusMsg and returns a ForceKill command message`` () =
    let service = { FAMService.init with Name = "ServiceToKill" }
    let expected = { initialModel with StatusMsg = sprintf "Killing %s..." service.Name }, [ForceKill service]
    let actual = initialModel |> update (ForceKillRequest service)
    test <@ actual = expected @>


  [<Test>]
  let ``ForceKillSuccess with zero PIDs actually killed and two PIDs attempted but not killed sets StatusMsg and returns a Refresh command message`` () =
    let pidsKilled =  []
    let pidsNotKilled = [5432; 4321]

    let now = DateTimeOffset.Now
    let name = "KilledService"
    let expected =
      { initialModel with
          StatusMsg = sprintf "Killed %s (PIDs: ) at %O. Could not kill: 5432, 4321" name now
      }, [Refresh]
    let actual = initialModel |> update (ForceKillSuccess (name, pidsKilled, pidsNotKilled))
    let adjusted = actual |> adjustStatusMsgTimeStampsIfClose now
    test <@ adjusted = expected @>


  [<Test>]
  let ``ForceKillSuccess with one PID actually killed and two PIDs attempted but not killed sets StatusMsg and returns a Refresh command message`` () =
    let pidsKilled =  [1234]
    let pidsNotKilled = [5432; 4321]

    let now = DateTimeOffset.Now
    let name = "KilledService"
    let expected =
      { initialModel with
          StatusMsg = sprintf "Killed %s (PID 1234) at %O. Could not kill: 5432, 4321" name now
      }, [Refresh]
    let actual = initialModel |> update (ForceKillSuccess (name, pidsKilled, pidsNotKilled))
    let adjusted = actual |> adjustStatusMsgTimeStampsIfClose now
    test <@ adjusted = expected @>


  [<Test>]
  let ``ForceKillSuccess with two PIDs actually killed and zero PIDs attempted but not killed sets StatusMsg and returns a Refresh command message`` () =
    let pidsKilled =  [1234; 2345]
    let pidsNotKilled = []

    let now = DateTimeOffset.Now
    let name = "KilledService"
    let expected =
      { initialModel with
          StatusMsg = sprintf "Killed %s (PIDs: 1234, 2345) at %O" name now
      }, [Refresh]
    let actual = initialModel |> update (ForceKillSuccess (name, pidsKilled, pidsNotKilled))
    let adjusted = actual |> adjustStatusMsgTimeStampsIfClose now
    test <@ adjusted = expected @>


  [<Test>]
  let ``ForceKillSuccess with two PIDs actually killed and one PID attempted but not killed sets StatusMsg and returns a Refresh command message`` () =
    let pidsKilled =  [1234; 2345]
    let pidsNotKilled = [5432]

    let now = DateTimeOffset.Now
    let name = "KilledService"
    let expected =
      { initialModel with
          StatusMsg = sprintf "Killed %s (PIDs: 1234, 2345) at %O. Could not kill: 5432" name now
      }, [Refresh]
    let actual = initialModel |> update (ForceKillSuccess (name, pidsKilled, pidsNotKilled))
    let adjusted = actual |> adjustStatusMsgTimeStampsIfClose now
    test <@ adjusted = expected @>


  [<Test>]
  let ``LaunchConfigEditorRequest sets StatusMsg and returns a LaunchConfigEditor command messsage`` () =
    let name = "ServiceToEdit"
    let expected = { initialModel with StatusMsg = sprintf "Editing %s with external util..." name }, [LaunchConfigEditor name]
    let actual = initialModel |> update (LaunchConfigEditorRequest name)
    test <@ actual = expected @>


  [<Test>]
  let ``LaunchConfigEditorSuccess sets StatusMsg and returns a Refresh command message`` () =
    let now = DateTimeOffset.Now
    let name = "ServiceToEdit"
    let expected = { initialModel with StatusMsg = sprintf "Edited %s at %O." name now }, [Refresh]
    let actual = initialModel |> update (LaunchConfigEditorSuccess name)
    let adjusted = actual |> adjustStatusMsgTimeStampsIfClose now
    test <@ adjusted = expected @>


  [<Test>]
  let ``ServiceMsg: ParentMsg updates the model`` () =
    let name = "ServiceToEdit"
    let expected = { initialModel with StatusMsg = sprintf "Editing %s with external util..." name }, [LaunchConfigEditor name]
    let actual = initialModel |> update (ServiceMsg ("", name |> LaunchConfigEditorRequest |> ParentMsg))
    test <@ actual = expected @>
    

  [<Test>]
  let ``ServiceMsg: SetStartModeRequest updates the FAMService model immediately and returns a SetStartMode command message to update the actual windows service`` () =
    test <@ FAMService.init.StartMode = StartMode.Unknown @>
    let name = "Service 2"
    let mode = Auto
    let expected =
      { initialModel with
          Services =
            [ {FAMService.init with Name = "Service 1"}
              {FAMService.init with Name = "Service 2"; StartMode = mode} ]
      }, [SetStartMode (name, mode)]
    let initialModel =
      { initialModel with
          Services =
            [ {FAMService.init with Name = "Service 1"}
              {FAMService.init with Name = "Service 2"; StartMode = Disabled} ]
      }
    let actual = initialModel |> update (ServiceMsg (name, (SetStartModeRequest mode)))
    test <@ actual = expected @>


  [<Test>]
  let ``ServiceMsg: StartServiceRequest updates the FAMService model to prevent double-click of the start button and returns a StartServiceRequest command message`` () =
    test <@ FAMService.init.StateChangeInitiated = false @>
    let name = "Service 1"
    let expected =
      { initialModel with
          Services =
            [ {FAMService.init with Name = "Service 1"; StateChangeInitiated = true}
              {FAMService.init with Name = "Service 2"} ]
      }, [StartService name]
    let initialModel =
      { initialModel with
          Services =
            [ {FAMService.init with Name = "Service 1"}
              {FAMService.init with Name = "Service 2"} ]
      }
    let actual = initialModel |> update (ServiceMsg (name, StartServiceRequest))
    test <@ actual = expected @>


  [<Test>]
  let ``ServiceMsg: RestartServiceRequest updates the FAMService model to prevent double-click of the restart button and returns a RestartServiceRequest command message`` () =
    test <@ FAMService.init.StateChangeInitiated = false @>
    let name = "Service 2"
    let expected =
      { initialModel with
          Services =
            [ {FAMService.init with Name = "Service 1"}
              {FAMService.init with Name = "Service 2"; StateChangeInitiated = true} ]
          ServicesToRestart = set [name]
      }, [StopService name]
    let initialModel =
      { initialModel with
          Services =
            [ {FAMService.init with Name = "Service 1"}
              {FAMService.init with Name = "Service 2"} ]
      }
    let actual = initialModel |> update (ServiceMsg (name, RestartServiceRequest))
    test <@ actual = expected @>


  [<Test>]
  let ``ServiceMsg: StopServiceRequest updates the FAMService model to prevent double-click of the stop button and returns a StopServiceRequest command message`` () =
    test <@ FAMService.init.StateChangeInitiated = false @>
    let name = "Service 1"
    let expected =
      { initialModel with
          Services =
            [ {FAMService.init with Name = "Service 1"; StateChangeInitiated = true}
              {FAMService.init with Name = "Service 2"} ]
      }, [StopService name]
    let initialModel =
      { initialModel with
          Services =
            [ {FAMService.init with Name = "Service 1"}
              {FAMService.init with Name = "Service 2"} ]
      }
    let actual = initialModel |> update (ServiceMsg (name, StopServiceRequest))
    test <@ actual = expected @>


  [<Test>]
  let ``ServiceMsg: EditStartNameAndPassword sets the Dialog property and StatusMsg using specified service`` () =
    let name = "Service 2"
    let userName = "Herbie"
    let initialModel =
      { initialModel with
          Services =
            [ {FAMService.init with Name = "Service 1"}
              {FAMService.init with Name = "Service 2"; StartName = userName } ]
          StatusMsg = sprintf "Editing account for %s..." name
      }
    let expected =
      { initialModel with
          Dialog = Some { NamePasswordDialog.init with
                            ServiceName = name
                            StartName = userName }
      }, []
    let actual = initialModel |> update (ServiceMsg (name, EditStartNameAndPassword))
    test <@ actual = expected @>


  [<Test>]
  let ``DialogMsg: Cancel sets the Dialog property to None and the StatusMsg to indicate the cancel`` () =
    let name = "Service 1"
    let userName = "Kenten"
    let initialModel =
      { initialModel with
          Services = [ {FAMService.init with Name = "Service 1"} ]
          Dialog = Some { NamePasswordDialog.init with
                            ServiceName = name
                            StartName = userName }
      }
    let expected =
      { initialModel with
          StatusMsg = "Canceled edit"
          Dialog = None }, []
    let actual = initialModel |> update (DialogMsg NamePasswordDialog.Msg.Cancel)
    test <@ actual = expected @>
    

  [<Test>]
  let ``DialogMsg: Submit updates the FAMService model immediately and returns a SetStartNameAndPassword command message to update the actual windows service`` () =
    let name = "Service 1"
    let userName = "Dale"
    let pwd = "******"
    let expected =
      { initialModel with
          Services =
            [ {FAMService.init with Name = "Service 1"; StartName = userName }
              {FAMService.init with Name = "Service 2"} ]
          StatusMsg = sprintf "Submitting account change for Service 1..."
      }, [SetStartNameAndPassword (name, userName, pwd)]
    let initialModel =
      { initialModel with
          Services =
            [ {FAMService.init with Name = "Service 1"}
              {FAMService.init with Name = "Service 2"} ]
      }
    let actual = initialModel |> update (DialogMsg (NamePasswordDialog.Msg.Submit (name, userName, pwd)))
    test <@ actual = expected @>


  [<Test>]
  let ``DialogMsg: SetStartName updates the dialog model`` () =
    let userName = "Dale"
    let expected =
      { initialModel with
          Dialog = Some { NamePasswordDialog.init with ServiceName = "A srvs"; StartName = userName }
      }, []
    let initialModel =
      { initialModel with
          Dialog = Some { NamePasswordDialog.init with ServiceName = "A srvs" }
      }
    let actual = initialModel |> update (DialogMsg (NamePasswordDialog.Msg.SetStartName userName))
    test <@ actual = expected @>


  [<Test>]
  let ``DialogMsg: SetPassword updates the dialog model`` () =
    let password = "123"
    let expected =
      { initialModel with
          Dialog = Some { NamePasswordDialog.init with ServiceName = "A srvs"; Password = password }
      }, []
    let initialModel =
      { initialModel with
          Dialog = Some { NamePasswordDialog.init with ServiceName = "A srvs" }
      }
    let actual = initialModel |> update (DialogMsg (NamePasswordDialog.Msg.SetPassword password))
    test <@ actual = expected @>


  [<Test>]
  let ``DialogMsg: SetPasswordCheck updates the dialog model`` () =
    let password = "123"
    let expected =
      { initialModel with
          Dialog = Some { NamePasswordDialog.init with ServiceName = "A srvs"; PasswordCheck = password }
      }, []
    let initialModel =
      { initialModel with
          Dialog = Some { NamePasswordDialog.init with ServiceName = "A srvs" }
      }
    let actual = initialModel |> update (DialogMsg (NamePasswordDialog.Msg.SetPasswordCheck password))
    test <@ actual = expected @>


  [<Test>]
  let ``SetStartModeSuccess sets StatusMsg and returns a Refresh command message`` () =
    let name = "Service 2"
    let expected = { initialModel with StatusMsg = sprintf "Set start mode for %s" name }, [Refresh]
    let actual = initialModel |> update (SetStartModeSuccess name)
    test <@ actual = expected @>


  [<Test>]
  let ``SetStartNameAndPasswordSuccess when no password given sets StatusMsg and returns a Refresh command message`` () =
    let name = "Service 1"
    let expected = { initialModel with StatusMsg = sprintf "Set user name for %s" name }, [Refresh]
    let actual = initialModel |> update (SetStartNameAndPasswordSuccess (name, false))
    test <@ actual = expected @>


  [<Test>]
  let ``SetStartNameAndPasswordSuccess with a password sets StatusMsg and returns a Refresh command message`` () =
    let name = "Service 2"
    let expected = { initialModel with StatusMsg = sprintf "Set user name and password for %s" name }, [Refresh]
    let actual = initialModel |> update (SetStartNameAndPasswordSuccess (name, true))
    test <@ actual = expected @>


  [<Test>]
  let ``StartServiceSuccess sets StatusMsg and returns a Refresh command message`` () =
    let now = DateTimeOffset.Now
    let name = "Service 1"
    let expected = { initialModel with StatusMsg = sprintf "Started %s at %O." name now }, [Refresh]
    let actual = initialModel |> update (StartServiceSuccess name)
    let adjusted = actual |> adjustStatusMsgTimeStampsIfClose now
    test <@ adjusted = expected @>


  [<Test>]
  let ``StopServiceSuccess sets StatusMsg and returns a Refresh command message`` () =
    let now = DateTimeOffset.Now
    let name = "Service 2"
    let expected = { initialModel with StatusMsg = sprintf "Stopped %s at %O." name now }, [Refresh]
    let actual = initialModel |> update (StopServiceSuccess name)
    let adjusted = actual |> adjustStatusMsgTimeStampsIfClose now
    test <@ adjusted = expected @>


  [<Test>]
  let ``RestartServiceSuccess removes the name from the ServicesToRestart set, sets StatusMsg and returns a Refresh command message`` () =
    let now = DateTimeOffset.Now
    let name = "Service 1"
    let initialModel = { initialModel with ServicesToRestart = set ["Service 1"; "Service 2"] }
    let expected =
      { initialModel with
          StatusMsg = sprintf "Restarted %s at %O." name now
          ServicesToRestart = set ["Service 2"]
      }, [Refresh]
    let actual = initialModel |> update (RestartServiceSuccess name)
    let adjusted = actual |> adjustStatusMsgTimeStampsIfClose now
    test <@ adjusted = expected @>


  [<Test>]
  let ``RestartServiceFailure removes the name from the ServicesToRestart set, sets StatusMsg with the error info and returns a Refresh command message`` () =
    let name = "Service 2"
    let initialModel = { initialModel with ServicesToRestart = set ["Service 1"; "Service 2"] }
    let expected =
      { initialModel with
          StatusMsg = sprintf "Failed to restart Service 2 ExtractException: err"
          ServicesToRestart = set ["Service 1"]
      }, [Refresh]
    let actual = initialModel |> update (RestartServiceFailure (name, Extract.ExtractException("ABC123", "err")))
    test <@ actual = expected @>


  [<Test>]
  let ``StopServiceFailure removes the name from the ServicesToRestart set, sets StatusMsg with the error info and returns a Refresh command message`` () =
    let name = "Service 1"
    let initialModel = { initialModel with ServicesToRestart = set ["Service 1"; "Service 2"] }
    let expected =
      { initialModel with
          StatusMsg = sprintf "Failed to stop Service 1 Exception: failed"
          ServicesToRestart = set ["Service 2"]
      }, [Refresh]
    let actual = initialModel |> update (StopServiceFailure (name, Exception("failed")))
    test <@ actual = expected @>


  [<Test>]
  let ``RefreshIfNeeded when a refresh is not needed does nothing`` () =
    let now = DateTimeOffset.Now
    let initialModel =
      { initialModel with
          LastRefresh = now - TimeSpan.FromMinutes 4.
          Services =
            [ {FAMService.init with Name = "Service 1"; State = Running}
              {FAMService.init with Name = "Service 2"; State = Stopped} ]
      }
    let expected: Model * CmdMsg list = initialModel, []
    let actual = initialModel |> update RefreshIfNeeded
    test <@ actual = expected @>


  [<Test>]
  let ``RefreshIfNeeded when the time since the last refresh is greater than 5 minutes returns a Refresh command message`` () =
    let initialModel = { initialModel with LastRefresh = DateTimeOffset.Now - TimeSpan.FromMinutes 5.01 }
    let expected = initialModel, [Refresh]
    let actual = initialModel |> update RefreshIfNeeded
    test <@ actual = expected @>


  [<Test>]
  let ``RefreshIfNeeded when a service is pending start returns a Refresh command message`` () =
    let initialModel =
      { initialModel with
          LastRefresh = DateTimeOffset.Now
          Services =
            [ {FAMService.init with Name = "Service 1"; State = Starting}
              {FAMService.init with Name = "Service 2"} ]
      }
    let expected = initialModel, [Refresh]
    let actual = initialModel |> update RefreshIfNeeded
    test <@ actual = expected @>


  [<Test>]
  let ``RefreshIfNeeded when a service is pending stop returns a Refresh command message`` () =
    let initialModel =
      { initialModel with
          LastRefresh = DateTimeOffset.Now
          Services =
            [ {FAMService.init with Name = "Service 1"}
              {FAMService.init with Name = "Service 2"; State = Stopping} ]
      }
    let expected = initialModel, [Refresh]
    let actual = initialModel |> update RefreshIfNeeded
    test <@ actual = expected @>


  [<Test>]
  let ``RefreshIfNeeded when ServicesToRestart is non-empty returns a Refresh command message`` () =
    let initialModel =
      { initialModel with
          LastRefresh = DateTimeOffset.Now
          ServicesToRestart = set ["Service 3"; "Service 2"]
      }
    let expected = initialModel, [Refresh]
    let actual = initialModel |> update RefreshIfNeeded
    test <@ actual = expected @>


  [<Test>]
  let ``SetShowDetails changes ShowDetails`` () =
    test <@ initialModel.ShowDetails = HideAll @>
    let expected = { initialModel with ShowDetails = ShowAll }, []
    let actual = initialModel |> update (SetShowDetails ShowAll)
    test <@ actual = expected @>


  [<Test>]
  let ``ToggleShowDetails when ShowDetails is HideAll changes mode to ShowSelected`` () =
    let initialModel = { initialModel with ShowDetails = HideAll }
    let expected = { initialModel with ShowDetails = ShowSelected }, []
    let actual = initialModel |> update ToggleShowDetails
    test <@ actual = expected @>


  [<Test>]
  let ``ToggleShowDetails when ShowDetails is ShowSelected changes mode to HideAll`` () =
    let initialModel = { initialModel with ShowDetails = ShowSelected }
    let expected = { initialModel with ShowDetails = HideAll }, []
    let actual = initialModel |> update ToggleShowDetails
    test <@ actual = expected @>


  [<Test>]
  let ``ToggleShowDetails when ShowDetails is ShowAll changes mode to HideAll`` () =
    let initialModel = { initialModel with ShowDetails = ShowAll }
    let expected = { initialModel with ShowDetails = HideAll }, []
    let actual = initialModel |> update ToggleShowDetails
    test <@ actual = expected @>


  [<Test>]
  let ``SetSelectedService None sets SelectedService to None`` () =
    let initialModel = { initialModel with SelectedService = Some "ServiceName" }
    let expected = { initialModel with SelectedService = None }, []
    let actual = initialModel |> update (SetSelectedService None)
    test <@ actual = expected @>


  [<Test>]
  let ``SetSelectedService (Some name) sets SelectedService to Some name`` () =
    let initialModel = { initialModel with SelectedService = None }
    let expected = { initialModel with SelectedService = Some "ServiceName" }, []
    let actual = initialModel |> update (SetSelectedService (Some "ServiceName"))
    test <@ actual = expected @>


  [<Test>]
  let ``Failure sets StatusMsg and returns a Refresh command message`` () =
    let expected =
      { initialModel with
          StatusMsg = sprintf "Failed to do work. Exception: failed"
      }, [Refresh]
    let actual = initialModel |> update (Msg.Failure (Extract.ExtractException("EFG000", "Failed to do work."), Exception("failed")))
    test <@ actual = expected @>


  [<Test>]
  let ``ClearStatus clears StatusMsg`` () =
    let expected = { initialModel with StatusMsg = "" }, []
    let initialModel = { initialModel with StatusMsg = "some staus" }
    let actual = initialModel |> update ClearStatus
    test <@ actual = expected @>
