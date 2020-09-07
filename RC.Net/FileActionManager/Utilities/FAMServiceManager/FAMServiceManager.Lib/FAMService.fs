module FAMService

open System
open System.Collections.Generic
open System.IO
open System.Management
open Extract.Utilities
open Extract.FileActionManager.Database
 
type FPSData = 
 { ID: int
   FileName: string
   NumberOfInstances: int
   NumberOfFilesToProcess: int }

type FAMService =
  { Name: string
    DisplayName: string
    LogonUserName: string
    StartMode: string
    Started: bool
    Settings: IDictionary<string, string>
    FPSData: FPSData list }


[<RequireQualifiedAccess>]
module FAMService =

  let private installUtilPath = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\installutil.exe"
  let private es_fam_service = "esfamservice.exe" 
  let private servicePath = Path.Combine(FileSystemMethods.CommonComponentsPath, es_fam_service)
  let private editorPath = Path.Combine(FileSystemMethods.CommonComponentsPath, "SQLCDBEditor.exe")

  let private getSdfPath serviceName = 
    let programData = FileSystemMethods.CommonApplicationDataPath
    Path.Combine(programData, "ESFAMService", serviceName + ".sdf")

  let private getDetails serviceName =
    let sdfPath = getSdfPath serviceName
    let mgr = FAMServiceDatabaseManager(sdfPath)
    let fpsFiles =
      mgr.GetFpsFileData(false)
      |> Seq.mapi (fun i data ->
        { ID = i
          FileName = data.FileName
          NumberOfInstances = data.NumberOfInstances
          NumberOfFilesToProcess = data.NumberOfFilesToProcess })
      |> Seq.toList

    mgr.Settings, fpsFiles

  let getInstalled() =
    use searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Service")
    use results = searcher.Get()
    results
    |> Seq.cast<ManagementBaseObject>
    |> Seq.filter (fun info -> (string info.["PathName"]).ToLowerInvariant().Contains(es_fam_service))
    |> Seq.map (fun info ->
      let serviceName = string info.["Name"]
      let settings, fpsFiles = getDetails serviceName
      { Name = string info.["Name"]
        DisplayName = string info.["DisplayName"]
        LogonUserName = string info.["StartName"]
        StartMode = string info.["StartMode"]
        Started = Convert.ToBoolean info.["Started"]
        Settings = settings
        FPSData = fpsFiles }
    )
    |> Seq.toList

  let private runCmd cmd =
    let output = ref ""
    let errors = ref ""
    let retVal = SystemMethods.RunExecutable(cmd, output, errors)
    if retVal <> 0
    then failwith (if String.IsNullOrWhiteSpace errors.Value then output.Value else errors.Value)

  let install name displayName =
    let cmd = sprintf "%s /ServiceName=\"%s\" /DisplayName=\"%s\" \"%s\"" installUtilPath name displayName servicePath
    runCmd cmd

  let uninstall name =
    let cmd = sprintf "%s /u /ServiceName=\"%s\" \"%s\"" installUtilPath name servicePath
    runCmd cmd

  let edit name =
    let dbPath = getSdfPath name
    let cmd = sprintf "\"%s\" \"%s\"" editorPath dbPath
    runCmd cmd

  let init =
    { Name = "Name"
      DisplayName = "DisplayName"
      LogonUserName = "StartName"
      StartMode = "StartMode"
      Started = false
      Settings = Dictionary<_,_>()
      FPSData = List.empty }
