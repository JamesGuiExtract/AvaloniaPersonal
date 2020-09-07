module FAMServiceManager.Program

open System
open Elmish
open Elmish.WPF
open Extract.Utilities

module App =

  open FAMService

  type Model =
    { Services: FAMService list
      NewServiceName: string
      NewServiceDisplayName: string
      StatusMsg: string }

  type Msg =
    | OpenServicesRequest
    | OpenServicesSuccess
    | OpenServicesFailure of exn
    | LoadSuccess of FAMService list
    | LoadFailure of exn
    | SetNewServiceName of string
    | SetNewServiceDisplayName of string
    | RefreshRequest
    | RefreshSuccess of FAMService list
    | RefreshFailure of exn
    | InstallRequest of (string * string)
    | InstallSuccess of string
    | InstallFailure of exn
    | RemoveRequest of string
    | RemoveSuccess of string
    | RemoveFailure of exn
    | LaunchConfigEditorRequest of string
    | LaunchConfigEditorSuccess of string
    | LaunchConfigEditorFailure of exn

  type CmdMsg =
    | OpenServices
    | Install of (string * string)
    | Remove of string
    | LaunchConfigEditor of string
    | Refresh
    | Load

  let init () =
    { Services = []
      NewServiceName = ""
      NewServiceDisplayName = ""
      StatusMsg = "" },
    [Refresh]

  let update msg m =
    match msg with
    | OpenServicesRequest -> { m with StatusMsg = "Opening services.msc..." }, [OpenServices]
    | OpenServicesSuccess -> { m with StatusMsg = sprintf "Opened services.msc at %O" DateTimeOffset.Now }, []
    | OpenServicesFailure ex -> { m with StatusMsg = sprintf "Failed to open services.msc with exception %s: %s" (ex.GetType().Name) ex.Message }, []

    | RefreshRequest -> { m with StatusMsg = sprintf "Refreshing..." }, [Refresh]
    | RefreshSuccess services -> { m with Services = services; StatusMsg = sprintf "Installed services circa %O" DateTimeOffset.Now }, []
    | RefreshFailure ex | LoadFailure ex -> { m with StatusMsg = sprintf "Failed to load services with exception %s: %s" (ex.GetType().Name) ex.Message }, []

    | LoadSuccess services -> { m with Services = services }, []

    | SetNewServiceName name -> { m with NewServiceName = name }, []
    | SetNewServiceDisplayName name -> { m with NewServiceDisplayName = name }, []

    | InstallRequest (name, displayName) ->
        { m with NewServiceName = ""; NewServiceDisplayName = ""; StatusMsg = sprintf "Installing %s..." name }, [Install (name, displayName)]
    | InstallSuccess name -> { m with StatusMsg = sprintf "Successfully installed %s at %O" name DateTimeOffset.Now }, [Load]
    | InstallFailure ex -> { m with StatusMsg = sprintf "Installation failed with exception %s: %s" (ex.GetType().Name) ex.Message }, [Load]

    | RemoveRequest name -> { m with StatusMsg = sprintf "Removing %s..." name }, [Remove name]
    | RemoveSuccess name -> { m with StatusMsg = sprintf "Successfully removed %s at %O" name DateTimeOffset.Now }, [Load]
    | RemoveFailure ex -> { m with StatusMsg = sprintf "Remove failed with exception %s: %s" (ex.GetType().Name) ex.Message }, [Load]

    | LaunchConfigEditorRequest name -> { m with StatusMsg = sprintf "Editing %s with external util..." name }, [LaunchConfigEditor name]
    | LaunchConfigEditorSuccess name -> { m with StatusMsg = sprintf "Successfully edited %s at %O. Refreshing..." name DateTimeOffset.Now }, [Refresh]
    | LaunchConfigEditorFailure ex -> { m with StatusMsg = sprintf "Editing failed with exception %s: %s" (ex.GetType().Name) ex.Message }, []


  let openServices () =

    let openServices () =
      let cmd = "services.msc"
      let retVal = SystemMethods.RunExecutable(cmd, "", Int32.MaxValue, createNoWindow = false, startAndReturnImmediately = true)
      if retVal <> 0 then failwithf "Failed to run %s" cmd

    async {
      do openServices ()
      return OpenServicesSuccess
    }

  let install (serviceName, serviceDisplayName) =
    async {
      do! Async.SwitchToThreadPool ()
      do FAMService.install serviceName serviceDisplayName
      return InstallSuccess serviceName
    }

  let remove serviceName =
    async {
      do! Async.SwitchToThreadPool ()
      do FAMService.uninstall serviceName
      return RemoveSuccess serviceName
    }

  let edit serviceName =
    async {
      do! Async.SwitchToThreadPool ()
      do FAMService.edit serviceName
      return LaunchConfigEditorSuccess serviceName
    }

  let load () =
    async {
      do! Async.SwitchToThreadPool ()
      return FAMService.getInstalled ()
    }
    
  let toCmd = function
    | OpenServices -> Cmd.OfAsync.either openServices () id OpenServicesFailure
    | Install serviceName -> Cmd.OfAsync.either install serviceName id InstallFailure
    | Remove serviceName -> Cmd.OfAsync.either remove serviceName id RemoveFailure
    | LaunchConfigEditor serviceName -> Cmd.OfAsync.either edit serviceName id LaunchConfigEditorFailure
    | Refresh -> Cmd.OfAsync.either load () RefreshSuccess RefreshFailure
    | Load -> Cmd.OfAsync.either load () LoadSuccess LoadFailure


module Bindings =

  open App
  open FAMService

  // Bindings for settings rows (grid row details)
  let settingsBindings bindingName =
    bindingName |> Binding.subModelSeq(
      getSubModels = (fun (_, s) ->
        // Select the important settings and format into id*(name*value) tuples
        let settingsRows =
          [
            "DatabaseServer", "Database server"
            "DatabaseName", "Database name"
            "DependentServices", "Dependent services"
            "NumberOfFilesToProcessPerFAMInstance", "Number of files to process per FAM instance"
            "SleepTimeOnStart", "Sleep time on start"
          ]
          |> List.choose (fun (k, readable) -> match s.Settings.TryGetValue k with | true, v -> Some (k, (readable, v)) | _ -> None)
        // Format FPS files as id*(name*value) too
        let fileRows =
          s.FPSData
          |> List.map (fun data ->
            (string data.ID), (data.FileName, sprintf "%d/%d instances/files to process" data.NumberOfInstances data.NumberOfFilesToProcess)
          )
        // Combine the lists
        settingsRows @ fileRows),
      getId = fst, // First element of tuple
      bindings = fun () -> [
          "Name" |> Binding.oneWay(fun (_, (_id, (name, _value))) -> name)
          "Value" |> Binding.oneWay(fun (_, (_id, (_name, value))) -> value)
          ])

  // Bindings for service rows (grid rows)
  let serviceBindings () : Binding<Model * FAMService, Msg> list = [
    "Name" |> Binding.oneWay(fun (_, s) -> s.Name)
    "DisplayName" |> Binding.oneWay(fun (_, s) -> s.DisplayName)
    "LogonUserName" |> Binding.oneWay(fun (_, s) -> s.LogonUserName)
    "StartMode" |> Binding.oneWay(fun (_, s) -> s.StartMode)
    "State" |> Binding.oneWay(fun (_, s) -> if s.Started then "started" else "stopped")
    "Settings" |> settingsBindings
    "Remove" |> Binding.cmd(fun (_, s) -> s.Name |> RemoveRequest)
    "LaunchConfigEditor" |> Binding.cmd(fun (_, s) -> s.Name |> LaunchConfigEditorRequest)
  ]

  // Bindings for the main window
  let rootBindings () : Binding<Model, Msg> list = [
    "OpenServices" |> Binding.cmd(fun _ -> OpenServicesRequest)
    "Services" |> Binding.subModelSeq(
      getSubModels = (fun m -> m.Services),
      getId = (fun c -> c.Name),
      bindings = serviceBindings)
    "Refresh" |> Binding.cmd(fun _ -> RefreshRequest)
    "NewServiceName" |> Binding.twoWay((fun m -> m.NewServiceName), (fun n -> SetNewServiceName n))
    "NewServiceDisplayName" |> Binding.twoWay((fun m -> m.NewServiceDisplayName), (fun n -> SetNewServiceDisplayName n))
    "Status" |> Binding.oneWay(fun m -> m.StatusMsg)
    "InstallService" |> Binding.cmdIf(
      exec = (fun m -> InstallRequest (m.NewServiceName, m.NewServiceDisplayName)),
      canExec = (fun m -> not (String.IsNullOrWhiteSpace m.NewServiceName || String.IsNullOrWhiteSpace m.NewServiceDisplayName)))
  ]


let mainDesignVm = ViewModel.designInstance (App.init () |> fst) (Bindings.rootBindings ())

let main window =
  Program.mkProgramWpfWithCmdMsg App.init App.update Bindings.rootBindings App.toCmd
  |> Program.runWindowWithConfig
    { ElmConfig.Default with LogConsole = false; Measure = false }
    window
