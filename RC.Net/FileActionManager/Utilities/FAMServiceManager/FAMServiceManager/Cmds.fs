[<AutoOpen>]
module Extract.FileActionManager.Utilities.FAMServiceManager.Cmds

open Extract
open Extract.Utilities
open System

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

let kill service =
  async {
    do! Async.SwitchToThreadPool ()
    let pidsKilled, pidsNotKilled = FAMService.kill service
    return ForceKillSuccess (service.Name, pidsKilled, pidsNotKilled)
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

let setStartMode (name, mode) =
  async {
    do! Async.SwitchToThreadPool ()
    do FAMService.setStartMode name mode
    return name
  }

let setStartNameAndPassword (serviceName, userName, password) =
  async {
    do! Async.SwitchToThreadPool ()
    let changedPassword = FAMService.setStartNameAndPassword serviceName userName password
    return serviceName, changedPassword
  }

let startService name =
  async {
    do! Async.SwitchToThreadPool ()
    do FAMService.startService name
    return name
  }
  
let stopService name =
  async {
    do! Async.SwitchToThreadPool ()
    do FAMService.stopService name
    return name
  }
  
let handleExn (msg: string) (ex: exn) =
  let uex = ExtractException("ELI51478", msg, ex)
  uex.Log()
  Failure (uex, ex)

let handleExnf fmt = 
  Printf.kprintf handleExn fmt

