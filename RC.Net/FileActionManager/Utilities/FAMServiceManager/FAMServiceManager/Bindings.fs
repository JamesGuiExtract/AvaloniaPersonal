[<AutoOpen>]
module Extract.FileActionManager.Utilities.FAMServiceManager.Bindings

open Elmish.WPF
open System
open System.Windows.Controls

let ``num with s if not 1`` n f =
  f n (if n = 1 then "" else "s")

let isMultiLine s =
  not (s |> String.IsNullOrEmpty || s.Split('\r', '\n').Length = 1)

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
          let numToProcess =
            if data.NumberOfFilesToProcess = -1 then
              match s.Settings.TryGetValue "NumberOfFilesToProcessPerFAMInstance" with
              | true, num -> int32 num
              | _ -> data.NumberOfFilesToProcess
            else data.NumberOfFilesToProcess
          (string data.ID), (data.FileName, sprintf "%d instance%s w/ %d file%s to process per instance"
                                            |> ``num with s if not 1`` data.NumberOfInstances
                                            |> ``num with s if not 1`` numToProcess)
        )
      // Combine the lists
      settingsRows @ fileRows),
    getId = fst, // First element of tuple
    bindings = fun () -> [
        "Name" |> Binding.oneWay(fun (_, (_id, (name, _value))) -> name)
        "Value" |> Binding.oneWay(fun (_, (_id, (_name, value))) -> value)
        ])

// Bindings for service rows (grid rows)
let serviceBindings () : Binding<Model * FAMService, ServiceMsg> list = [
  "Name" |> Binding.oneWay(fun (_, s) -> s.Name)
  "DisplayName" |> Binding.oneWay(fun (_, s) -> s.DisplayName)
  "StartMode" |> Binding.twoWay((fun (_, s) -> s.StartMode |> StartMode.toString), StartMode.ofString >> SetStartModeRequest)
  "StartName" |> Binding.oneWay(fun (_, s) -> s.StartName)
  "ToggleStart" |> Binding.cmdIf(fun (_, s) ->
      match s.StateChangeInitiated, s.State with
      | true, _ -> None
      | _, Running -> Some StopServiceRequest
      | _, Stopped -> Some StartServiceRequest
      | _ -> None)
  "StartStop" |> Binding.oneWay(fun (_, s) -> match s.State with | Running -> "Stop" | _ -> "Start")
  "ToggleShowDetails" |> Binding.cmdIf(fun (m, (s: FAMService)) ->
      match m.SelectedService, s.Name with
      | Some n, n' when n = n' -> Some (ToggleShowDetails |> ParentMsg)
      | _ -> None)
  "ShowHideDetails" |> Binding.oneWay(fun (m, s) ->
      match m.SelectedService, s.Name with
      | Some n, n' when n = n' ->
        match m.ShowDetails with
        | HideAll -> "Show details"
        | _ -> "Hide details"
      | _ ->
        match m.ShowDetails with
        | ShowAll -> "Hide details"
        | _ -> "Show details")
  "Restart" |> Binding.cmdIf(fun (_, s) ->
      match s.StateChangeInitiated, s.State with
      | true, _ -> None
      | _, Running -> Some RestartServiceRequest
      | _ -> None)
  "State" |> Binding.oneWay(fun (_, s) -> s.State |> ServiceState.toString)
  "Settings" |> settingsBindings
  "Remove" |> Binding.cmdIf(fun (_, s) ->
      match s.StateChangeInitiated, s.State with
      | true, _ -> None
      | _, Stopped -> Some (s.Name |> RemoveRequest |> ParentMsg)
      | _ -> None)
  "ForceKill" |> Binding.cmdIf(fun (_, s) -> s.PID |> Option.map (fun _ -> s |> ForceKillRequest |> ParentMsg))
  "LaunchConfigEditor" |> Binding.cmd(fun (_, (s: FAMService)) -> s.Name |> LaunchConfigEditorRequest |> ParentMsg)
  "EditStartNameAndPassword" |> Binding.cmd EditStartNameAndPassword
]

// Bindings for the main window
let rootBindings () : Binding<Model, Msg> list = [
  "OpenServices" |> Binding.cmd(fun _ -> OpenServicesRequest)
  "Services" |> Binding.subModelSeq(
    getSubModels = (fun m -> m.Services),
    getId = (fun c -> c.Name),
    toMsg = ServiceMsg,
    bindings = serviceBindings)
  "LastRefresh" |> Binding.oneWay(fun m -> sprintf "Installed services circa %O" m.LastRefresh)
  "Refresh" |> Binding.cmd(fun _ -> ManualRefreshRequest)
  "NewServiceName" |> Binding.twoWay((fun m -> m.NewServiceName), (fun n -> SetNewServiceName n))
  "NewServiceDisplayName" |> Binding.twoWay((fun m -> m.NewServiceDisplayName), (fun n -> SetNewServiceDisplayName n))
  "Status" |> Binding.oneWay(fun m -> m.StatusMsg)
  "ClearStatus" |> Binding.cmdIf(fun m -> if m.StatusMsg |> isMultiLine then Some ClearStatus else None)
  "ShowClearStatus" |> Binding.oneWay(fun m -> m.StatusMsg |> isMultiLine)
  "InstallService" |> Binding.cmdIf(
    exec = (fun m -> InstallRequest (m.NewServiceName, m.NewServiceDisplayName)),
    canExec = (fun m -> not (String.IsNullOrWhiteSpace m.NewServiceName || String.IsNullOrWhiteSpace m.NewServiceDisplayName)))
  "ShowRowDetails" |> Binding.oneWay(fun m ->
      match m.ShowDetails with
      | ShowAll -> DataGridRowDetailsVisibilityMode.Visible
      | HideAll | Unknown -> DataGridRowDetailsVisibilityMode.Collapsed
      | ShowSelected -> DataGridRowDetailsVisibilityMode.VisibleWhenSelected)
  "ShowAllRowDetails" |> Binding.twoWay((fun m -> m.ShowDetails = ShowAll), (fun show -> SetShowDetails <| if show then ShowAll else Unknown))
  "ShowSelectedRowDetails" |> Binding.twoWay((fun m -> m.ShowDetails = ShowSelected), (fun show -> SetShowDetails <| if show then ShowSelected else Unknown))
  "HideAllRowDetails" |> Binding.twoWay((fun m -> m.ShowDetails = HideAll), (fun hide -> SetShowDetails <| if hide then HideAll else Unknown))
  "SelectedService" |> Binding.subModelSelectedItem("Services", (fun m -> m.SelectedService), SetSelectedService)
  "NamePasswordDialogVisible" |> Binding.oneWay(fun m -> match m.Dialog with Some _ -> true | _ -> false)
  "NamePasswordDialog" |> Binding.subModelOpt(
    getSubModel = (fun m -> m.Dialog),
    toBindingModel = snd,
    toMsg = DialogMsg,
    bindings = NamePasswordDialog.bindings)
]

