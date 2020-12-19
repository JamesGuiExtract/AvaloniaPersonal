namespace Extract.FileActionManager.Utilities.FAMServiceManager

type Msg =
  | OpenServicesRequest
  | OpenServicesSuccess
  | SetNewServiceName of string
  | SetNewServiceDisplayName of string
  | ManualRefreshRequest
  | RefreshSuccess of FAMService list
  | ManualRefreshSuccess of FAMService list
  | InstallRequest of string * string
  | InstallSuccess of string
  | RemoveRequest of string
  | RemoveSuccess of string
  | ForceKillRequest of FAMService
  | ForceKillSuccess of string * int list * int list
  | LaunchConfigEditorRequest of string
  | LaunchConfigEditorSuccess of string
  | Failure of exn * exn

  | ServiceMsg of string * ServiceMsg
  | SetStartModeSuccess of string
  | SetStartNameAndPasswordSuccess of string * bool
  | StartServiceSuccess of string
  | StopServiceSuccess of string
  | StopServiceFailure of string * exn
  | RestartServiceSuccess of string
  | RestartServiceFailure of string * exn

  | RefreshIfNeeded
  | SetShowDetails of ShowHideDetails
  | ToggleShowDetails
  | SetSelectedService of string option
  | DialogMsg of NamePasswordDialog.Msg
  | ClearStatus

and ServiceMsg =
  | SetStartModeRequest of FAMService.StartMode
  | EditStartNameAndPassword
  | StartServiceRequest
  | RestartServiceRequest
  | StopServiceRequest
  | ParentMsg of Msg

type CmdMsg =
  | OpenServices
  | Install of string * string
  | Remove of string
  | ForceKill of FAMService
  | LaunchConfigEditor of string
  | Refresh
  | ManualRefresh
  | SetStartMode of string * FAMService.StartMode
  | SetStartNameAndPassword of string * string * string
  | StartService of string
  | StopService of string
  | RestartService of string
