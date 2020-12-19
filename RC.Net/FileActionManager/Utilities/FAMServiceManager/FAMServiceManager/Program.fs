module Extract.FileActionManager.Utilities.FAMServiceManager.Program

open System
open Elmish
open Elmish.WPF

let init () = Model.empty, [Refresh]

let timerTick dispatch =
  let timer = new Timers.Timer(5000.)
  timer.Elapsed.Add (fun _ -> dispatch RefreshIfNeeded)
  timer.Start()

let serviceStartModes = FAMService.StartMode.allModes

let mainDesignVm =
  ViewModel.designInstance
    { Model.empty with
        Services =
          [ FAMService.init
            { FAMService.init with StartName = "UserWithAVeryLongFirst_AndLastName@ExtractSystems.com" } ]
        StatusMsg = "Status..." } (Bindings.rootBindings ())

let main window =
  Program.mkProgramWpfWithCmdMsg init update Bindings.rootBindings toCmd
  |> Program.withSubscription (fun _ -> Cmd.ofSub timerTick)
  |> Program.runWindowWithConfig
    { ElmConfig.Default with
        #if DEBUG
        LogConsole = true
        #endif
        Measure = false }
    window
