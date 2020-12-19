[<AutoOpen>]
module Extract.FileActionManager.Utilities.FAMServiceManager.Update

open Elmish
open Elmish.WPF
open System

let rec update msg m =
  match msg with
  | OpenServicesRequest -> { m with StatusMsg = "Opening services.msc..." }, [OpenServices]
  | OpenServicesSuccess -> { m with StatusMsg = sprintf "Opened services.msc at %O" DateTimeOffset.Now }, []

  | ManualRefreshRequest -> { m with StatusMsg = "Refreshing..." }, [ManualRefresh]
  | ManualRefreshSuccess services -> { m with Services = services; LastRefresh = DateTimeOffset.Now; StatusMsg = "" }, []

  | RefreshSuccess services ->
      let m = { m with Services = services; LastRefresh = DateTimeOffset.Now }
      m, m |> Model.getServicesToRestart |> Seq.map RestartService |> Seq.toList

  | SetNewServiceName name -> { m with NewServiceName = name }, []
  | SetNewServiceDisplayName name -> { m with NewServiceDisplayName = name }, []

  | InstallRequest (name, displayName) ->
      { m with NewServiceName = ""; NewServiceDisplayName = ""; StatusMsg = sprintf "Installing %s..." name }, [Install (name, displayName)]
  | InstallSuccess name -> { m with StatusMsg = sprintf "Installed %s at %O" name DateTimeOffset.Now }, [Refresh]

  | RemoveRequest name ->
      let m =  { m with StatusMsg = sprintf "Removing %s..." name }
               |> Model.mapService name (fun s -> { s with StateChangeInitiated = true })
      m, [Remove name]
  | RemoveSuccess name -> { m with StatusMsg = sprintf "Removed %s at %O" name DateTimeOffset.Now }, [Refresh]

  | ForceKillRequest service -> { m with StatusMsg = sprintf "Killing %s..." service.Name }, [ForceKill service]
  | ForceKillSuccess (name, pidsKilled, pidsNotKilled) ->
      let killed = sprintf "(PID%s %s)" (if pidsKilled.Length = 1 then "" else "s:") (pidsKilled |> List.map string |> String.concat ", ")
      let notKilled = if pidsNotKilled |> List.isEmpty then "" else sprintf ". Could not kill: %s" (pidsNotKilled |> List.map string |> String.concat ", ")
      { m with StatusMsg = sprintf "Killed %s %s at %O%s" name killed DateTimeOffset.Now notKilled }, [Refresh]

  | LaunchConfigEditorRequest name -> { m with StatusMsg = sprintf "Editing %s with external util..." name }, [LaunchConfigEditor name]
  | LaunchConfigEditorSuccess name -> { m with StatusMsg = sprintf "Edited %s at %O." name DateTimeOffset.Now }, [Refresh]

  | DialogMsg (NamePasswordDialog.Submit (name, startName, startPassword)) ->
      let m = { m with Dialog = None; StatusMsg = (sprintf "Submitting account change for %s..." name) }
              |> Model.mapService name (fun s -> { s with StartName = startName })
      m, [SetStartNameAndPassword (name, startName, startPassword)]
  | DialogMsg NamePasswordDialog.Cancel -> { m with Dialog = None; StatusMsg = "Canceled edit" }, []
  | DialogMsg msg' ->
      match m.Dialog with
      | Some m' -> { m with Dialog = NamePasswordDialog.update msg' m' |> Some }, []
      | _ -> m, []

  | ServiceMsg (name, msg) ->
    match msg with
    | ParentMsg msg -> update msg m
    | EditStartNameAndPassword ->
        let userName = m.Services |> Seq.tryFind (fun s -> s.Name = name) |> Option.map (fun s -> s.StartName) |> Option.defaultValue ""
        { m with
            Dialog = Some { NamePasswordDialog.init with ServiceName = name; StartName = userName}
            StatusMsg = (sprintf "Editing account for %s..." name)
        }, []
    | SetStartModeRequest mode -> let m = m |> Model.mapService name (fun s -> { s with StartMode = mode }) in m, [SetStartMode (name, mode)]
    | StartServiceRequest -> let m = m |> Model.mapService name (fun s -> { s with StateChangeInitiated = true }) in m, [StartService name]
    | RestartServiceRequest ->
      let m = { m with ServicesToRestart = m.ServicesToRestart |> Set.add name }
              |> Model.mapService name (fun s -> { s with StateChangeInitiated = true })
      m, [StopService name]
    | StopServiceRequest -> let m = m |> Model.mapService name (fun s -> { s with StateChangeInitiated = true }) in m, [StopService name]

  | SetStartModeSuccess name -> {m with StatusMsg = sprintf "Set start mode for %s" name}, [Refresh]
  | SetStartNameAndPasswordSuccess (name, changedPassword) -> {m with StatusMsg = sprintf "Set user name%s for %s" (if changedPassword then " and password" else "") name}, [Refresh]

  // After a start/stop/restart success the service is probably still in the process of starting/stopping
  // The refresh will ensure that the current state is shown and will trigger frequent subsequent refreshes (via timerTick/RefreshIfNeeded)
  | StartServiceSuccess name -> { m with StatusMsg = sprintf "Started %s at %O." name DateTimeOffset.Now }, [Refresh]
  | StopServiceSuccess name -> { m with StatusMsg = sprintf "Stopped %s at %O." name DateTimeOffset.Now }, [Refresh]
  | RestartServiceSuccess name ->
    { m with
        ServicesToRestart = m.ServicesToRestart |> Set.remove name;
        StatusMsg = sprintf "Restarted %s at %O." name DateTimeOffset.Now }, [Refresh]

    // Handle these failures specially to remove the name of the failed service from ServicesToRestart 
  | RestartServiceFailure (name, ex) ->
    { m with
        ServicesToRestart = m.ServicesToRestart |> Set.remove name
        StatusMsg = sprintf "Failed to restart %s %s: %s" name (ex.GetType().Name) ex.Message }, [Refresh]
  | StopServiceFailure (name, ex) ->
    { m with
        ServicesToRestart = m.ServicesToRestart |> Set.remove name
        StatusMsg = sprintf "Failed to stop %s %s: %s" name (ex.GetType().Name) ex.Message }, [Refresh]
  | RefreshIfNeeded -> m, [if m |> Model.isUpdatePending || DateTimeOffset.Now - m.LastRefresh > TimeSpan.FromMinutes 5. then Refresh]
  | SetSelectedService maybeName -> { m with SelectedService = maybeName }, []
  | SetShowDetails mode -> (match mode with Unknown -> m | _ -> { m with ShowDetails = mode }), []
  | ToggleShowDetails -> { m with ShowDetails = match m.ShowDetails with | HideAll -> ShowSelected | _ -> HideAll }, []
  | ClearStatus -> { m with StatusMsg = "" }, []
  | Failure (uex, realEx) -> { m with StatusMsg = sprintf "%s %s: %s" uex.Message (realEx.GetType().Name) realEx.Message }, [Refresh]

let toCmd = function
  | OpenServices -> Cmd.OfAsync.either openServices () id (handleExn "Failed to open services.msc")
  | Install (name, displayName) -> Cmd.OfAsync.either install (name, displayName) id (handleExnf "Failed to install service: %s" name)
  | SetStartMode (name, mode) -> Cmd.OfAsync.either setStartMode (name, mode) SetStartModeSuccess (handleExnf "Failed to set start mode for %s" name)
  | SetStartNameAndPassword (name, startName, password) ->
      Cmd.OfAsync.either setStartNameAndPassword (name, startName, password) SetStartNameAndPasswordSuccess (handleExnf "Failed to set name/password for %s" name)
  | StartService name -> Cmd.OfAsync.either startService name StartServiceSuccess (handleExnf "Failed to start %s" name)
  | RestartService name -> Cmd.OfAsync.either startService name RestartServiceSuccess (fun ex -> RestartServiceFailure (name, ex))
  | StopService name -> Cmd.OfAsync.either stopService name StopServiceSuccess (fun ex -> StopServiceFailure (name, ex))
  | Remove name -> Cmd.OfAsync.either remove name id (handleExnf "Failed to remove service %s" name)
  | ForceKill service -> Cmd.OfAsync.either kill service id (handleExnf "Failed to kill service %s" service.Name)
  | LaunchConfigEditor name -> Cmd.OfAsync.either edit name id (handleExnf "Failed to launch editor for %s" name)
  | Refresh -> Cmd.OfAsync.either load () RefreshSuccess (handleExn "Failed to refresh services")
  | ManualRefresh -> Cmd.OfAsync.either load () ManualRefreshSuccess (handleExn "Failed to refresh services")
