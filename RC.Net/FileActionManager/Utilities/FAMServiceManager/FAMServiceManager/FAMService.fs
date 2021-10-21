[<AutoOpen>]
module Extract.FileActionManager.Utilities.FAMServiceManager.FAMService

open Extract.FileActionManager.Database
open Extract.Utilities
open Extract.Utilities.FSharp
open Extract.Utilities.SqlCompactToSqliteConverter
open System
open System.IO
open System.Management
 
type FPSData = 
 { ID: int
   FileName: string
   NumberOfInstances: int
   NumberOfFilesToProcess: int }

type ServiceState =
  | Starting
  | Running
  | Stopping
  | Stopped
  | Unknown

[<RequireQualifiedAccess>]
module ServiceState =
  let toString = function
  | Starting -> "Start Pending"
  | Running -> "Running"
  | Stopping -> "Stop Pending"
  | Stopped -> "Stopped"
  | Unknown -> "Unknown"

  let ofString = function
  | "Start Pending" -> Starting
  | "Running" -> Running
  | "Stop Pending" -> Stopping
  | "Stopped" -> Stopped
  | _ -> Unknown

type StartMode =
  | Manual
  | Auto
  | DelayedAuto
  | Disabled
  | Unknown

[<RequireQualifiedAccess>]
module StartMode =
  let toString = function
  | Manual -> "Manual"
  | Auto -> "Auto"
  | DelayedAuto -> "Auto (Delayed)"
  | Disabled -> "Disabled"
  | Unknown -> "Unknown"

  let toPropertyString = function
  | Manual -> "Manual"
  | Auto -> "Automatic"
  | Disabled -> "Disabled"
  | _ -> "Unsupported"

  let ofString = function
  | "Manual" -> Manual
  | "Auto" -> Auto
  | "Auto (Delayed)" -> DelayedAuto
  | "Disabled" -> Disabled
  | _ -> Unknown

  // Convert start mode string returned by sc qc
  // https://docs.microsoft.com/en-us/windows/win32/services/configuring-a-service-using-sc
  let ofStringSC (str: string) =
    match str.ToLowerInvariant().Replace(" ", "") with
    | "demand_start" -> Manual
    | "auto_start" -> Auto
    | "auto_start(delayed)" -> DelayedAuto
    | "disabled" -> Disabled
    | _ -> Unknown

  let allModes =
    [Manual; Auto; DelayedAuto; Disabled]
    |> List.map toString

type FAMService =
  { Name: string
    DisplayName: string
    StartName: string
    StartMode: StartMode
    State: ServiceState
    StateChangeInitiated: bool
    PID: uint32 option
    Settings: Map<string, string>
    FPSData: FPSData list }

[<RequireQualifiedAccess>]
module FAMService =
  let init =
    { Name = "Name"
      DisplayName = "DisplayName"
      StartName = "StartName"
      StartMode = StartMode.Unknown
      State = ServiceState.Unknown
      StateChangeInitiated = false
      PID = None
      Settings = Map.empty
      FPSData = List.empty }

  let private getDatabasePath serviceName = 
    async {
      let programData = FileSystemMethods.CommonApplicationDataPath
      let sqlitePath = Path.Combine(programData, "ESFAMService", serviceName + ".sqlite")
      let! _ = Async.AwaitTask (DatabaseConverter.ConvertDatabaseIfNeeded sqlitePath)
      return sqlitePath
    }
    |> Async.RunSynchronously

  let private getDetails serviceName =
    try
      let databasePath = getDatabasePath serviceName
      let mgr = FAMServiceSqliteDatabaseManager(databasePath)
      let fpsFiles =
        mgr.GetFpsFileData(false)
        |> Seq.mapi (fun i data ->
          { ID = i
            FileName = data.FileName
            NumberOfInstances = data.NumberOfInstances
            NumberOfFilesToProcess = data.NumberOfFilesToProcess })
        |> Seq.toList

      let settings = mgr.GetSettings() |> Seq.map (|KeyValue|) |> Map.ofSeq
      settings, fpsFiles
    with | _ -> Map.empty, []

  let private es_fam_service = "esfamservice.exe" 
  let private getInstalledFamServices() =
    seq {
      use searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Service")
      use results = searcher.Get()
      yield!
        results
        |> Seq.cast<ManagementBaseObject>
        |> Seq.filter (fun info -> (string info.["PathName"]).ToLowerInvariant().Contains(es_fam_service))
    }

  let getFamService name =
    getInstalledFamServices()
    |> Seq.filter (fun info -> string info.["Name"] = name)
    |> Seq.head

  let private installUtilPath = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\installutil.exe"
  let private scPath = @"C:\Windows\System32\sc.exe"
  let private secEditPath = @"C:\Windows\System32\SecEdit.exe"
  let private servicePath = Path.Combine(FileSystemMethods.CommonComponentsPath, es_fam_service)
  let private editorPath = Path.Combine(FileSystemMethods.CommonComponentsPath, "ExtractSqliteEditor.exe")

  let private runCmd cmd =
    let output = ref ""
    let errors = ref ""
    let retVal = SystemMethods.RunExecutable(cmd, output, errors)
    if retVal <> 0
    then failwith (if String.IsNullOrWhiteSpace errors.Value then output.Value else errors.Value)
    !output

  let private runCmdf fmt = 
    Printf.kprintf runCmd fmt

  let private isDelayedStartMode (info: ManagementBaseObject) =
    try
      Convert.ToBoolean(info.["DelayedAutoStart"])
    with
      | :? ManagementException as e when e.Message.Trim().ToLowerInvariant() = "not found" ->
        // NH: On my Windows 8.1 machine "DelayedAutoStart" is missing from the properties so use sc.exe to check
        let serviceName = string info.["Name"]
        let details = runCmdf "%s qc \"%s\"" scPath serviceName
        let startMode =
          details
          |> Regex.findAllMatches """(?xm) ^\s*START_TYPE\s*:\s*\d+\s+ (?'start_mode'[^\r\n]+)"""
          |> Seq.head
          |> (fun m -> m.Groups.["start_mode"].Value.Trim())
          |> StartMode.ofStringSC
        startMode = StartMode.DelayedAuto

  let private getStartMode (info: ManagementBaseObject) =
    let startMode = string info.["StartMode"] |> StartMode.ofString
    match startMode with
    | Auto ->
      let delayed = isDelayedStartMode info
      if delayed then DelayedAuto else Auto
    | _ -> startMode

  let getInstalled() =
    getInstalledFamServices()
    |> Seq.map (fun info ->
      let serviceName = string info.["Name"]
      let settings, fpsFiles = getDetails serviceName
      let res =
        { Name = string info.["Name"]
          DisplayName = string info.["DisplayName"]
          StartName = string info.["StartName"]
          StartMode = getStartMode info
          State = info.["State"] |> string |> ServiceState.ofString
          StateChangeInitiated = false
          PID = match info.["ProcessId"] :?> uint32 with 0u -> None | pid -> Some pid
          Settings = settings
          FPSData = fpsFiles }
      res
    )
    |> Seq.toList

  let install name displayName =
    runCmdf "%s /ServiceName=\"%s\" /DisplayName=\"%s\" \"%s\"" installUtilPath name displayName servicePath |> ignore

  let uninstall name =
    runCmdf "%s /u /ServiceName=\"%s\" \"%s\"" installUtilPath name servicePath |> ignore

  let edit name =
    let dbPath = getDatabasePath name
    runCmdf "\"%s\" \"%s\"" editorPath dbPath |> ignore

  // Returned by ManagementObject.InvokeMethod
  type ReturnValue =
    | Success = 0
    | NotSupported = 1
    | AccessDenied = 2
    | DependentServicesRunning = 3
    | InvalidServiceControl = 4
    | ServiceCannotAcceptControl = 5
    | ServiceNotActive = 6
    | ServiceRequestTimeout = 7
    | UnknownFailure = 8
    | PathNotFound = 9
    | ServiceAlreadyRunning = 10
    | ServiceDatabaseLocked = 11
    | ServiceDependencyDeleted = 12
    | ServiceDependencyFailure = 13
    | ServiceDisabled = 14
    | ServiceLogonFailure = 15
    | ServiceMarkedForDeletion = 16
    | ServiceNoThread = 17
    | StatusCircularDependency = 18
    | StatusDuplicateName = 19
    | StatusInvalidName = 20
    | StatusInvalidParameter = 21
    | StatusInvalidServiceAccount = 22
    | StatusServiceExists = 23
    | ServiceAlreadyPaused = 24
    | ServiceNotFound = 25

  let setProperties method properties (service: ManagementObject) =
    let args = service.GetMethodParameters method
    properties |> Seq.iter (fun (propertyName, value) -> args.[propertyName] <- value)
    let returnVal =
      service.InvokeMethod(method, args, null).["ReturnValue"]
      :?> uint32 |> int |> enum<ReturnValue>
    match returnVal with
    | ReturnValue.Success -> ()
    | _ -> failwithf "Return code: %A" returnVal

  let setAutoMode serviceName delayed =
    let mode = if delayed then "delayed-auto" else "auto"
    runCmdf "%s config \"%s\" start= %s" scPath serviceName mode |> ignore

  let setStartMode name mode =
    let service = getFamService name :?> ManagementObject

    // Determine whether mode can be set with WMI or needs to be set by running sc.exe
    // Need to always use sc.exe if setting to Auto because the delayed property might be set but hidden (e.g., on Windows 8.1)
    match mode with
    | Auto -> setAutoMode name false
    | DelayedAuto -> setAutoMode name true
    | _ -> service |> setProperties "Change" ["StartMode", mode |> StartMode.toPropertyString]

  open System.Security.Principal
  let getSID userName =
    try
      let f = NTAccount(userName)
      let sid = f.Translate typeof<SecurityIdentifier> :?> SecurityIdentifier
      Some sid
    with | :? IdentityNotMappedException -> None

  let getUserName (sid: SecurityIdentifier) =
    (sid.Translate typeof<NTAccount>).Value

  let private getCfgForServiceLogonRight sids =
    sprintf """[Unicode]
Unicode=yes
[Version]
signature="$CHICAGO$"
Revision=1
[Registry Values]
[Profile Description]
Description=Add logon as service right
[Privilege Rights]
SeServiceLogonRight = %s""" (sids |> String.concat ",")

  let private grantLogOnRights sid =
    // Need to get the current SIDs with this right so that we don't remove rights for the other users
    let currentRights =
      use curInfFile = new TemporaryFile(sensitive=false, extension=".inf")
      runCmdf "%s /export /cfg \"%s\"" secEditPath curInfFile.FileName |> ignore
      curInfFile.FileName
      |> File.ReadLines
      |> Seq.tryFind (fun line -> line.StartsWith "SeServiceLogonRight =")
      |> Option.map (fun line -> line.Remove(0, line.IndexOf '=' + 1).Trim().Split ',' |> List.ofArray)
      |> Option.defaultValue []

    let sidString = "*" + sid.ToString()
    if not (currentRights |> Seq.contains sidString) then
      let inf = getCfgForServiceLogonRight (sidString::currentRights)
      use infFile = new TemporaryFile(sensitive=false, extension=".inf")
      File.WriteAllText(infFile.FileName, inf)
      use dbFile = new TemporaryFile(sensitive=false, extension=".sdb")
      File.Delete dbFile.FileName
      runCmdf "%s /import /db \"%s\" /cfg \"%s\"" secEditPath dbFile.FileName infFile.FileName |> ignore
      runCmdf "%s /configure /db \"%s\"" secEditPath dbFile.FileName |> ignore

  let setStartNameAndPassword serviceName startName password =
    let service = getFamService serviceName :?> ManagementObject
    // Ensure the user has the log-on-as-service right
    match getSID startName with
    | Some sid ->
      grantLogOnRights sid
      // Translate back to a username to get full name (e.g., with domain)
      let userName = getUserName sid
      let propsToChange =
        [ yield ("StartName", userName)
          if not (String.IsNullOrEmpty password) then
            yield ("StartPassword", password)
        ]
      service |> setProperties "Change" propsToChange
      let changedPassword = propsToChange.Length = 2
      changedPassword
    | None -> failwithf "Could not resolve %s to a Security Identifier" startName

  let startService name =
    let service = getFamService name :?> ManagementObject
    let returnVal =
      service.InvokeMethod("StartService", null, null).["ReturnValue"]
      :?> uint32 |> int |> enum<ReturnValue>
    match returnVal with
    | ReturnValue.Success -> ()
    | _ -> failwithf "Return code: %A" returnVal

  let stopService name =
    let service = getFamService name :?> ManagementObject
    let returnVal = 
      service.InvokeMethod("StopService", null, null).["ReturnValue"]
      :?> uint32 |> int |> enum<ReturnValue>
    match returnVal with
    | ReturnValue.Success -> ()
    | _ -> failwithf "Return code: %A" returnVal

  let isPendingState service =
    match service.State with
    | Starting | Stopping -> true
    | _ -> false


  open Extract
  open Extract.FileActionManager.Utilities
  open Newtonsoft.Json
  open System.Diagnostics
  open System.IO.Pipes
  open System.Text

  let getSpawnedProcessIDs servicePID: int list =
    let pipeName = sprintf "ESFAMServicePipe_%d" servicePID
    use pipeStream = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut)
    try
      pipeStream.Connect 5000
      pipeStream.ReadMode <- PipeTransmissionMode.Message

      let request =
        RequestMessage.GetSpawnedProcessIDs
        |> JsonConvert.SerializeObject
        |> Encoding.UTF8.GetBytes

      pipeStream.Write(request, 0, request.Length);
      pipeStream.WaitForPipeDrain();

      let readMessage(): 'a =
        let responseBuilder = StringBuilder()
        let messageBuffer = Array.zeroCreate 16
        let rec read () =
          let bytesRead = pipeStream.Read (messageBuffer, 0, messageBuffer.Length)
          Encoding.UTF8.GetString(messageBuffer, 0, bytesRead)
          |> responseBuilder.Append
          |> ignore

          if pipeStream.IsMessageComplete then
            responseBuilder.ToString() |> JsonConvert.DeserializeObject<'a>
          else
            read()
        read()
      readMessage()

    // If service isn't responding then assume no processes have been spawned. E.g., the service can't connect to the File Processing DB
    with | :? TimeoutException -> []

  let logAppTraceForKilledProcess service killedPIDs =
    let servicePID = service.PID |> Option.map int |> Option.defaultValue 0
    let actuallyKilled = killedPIDs |> List.contains servicePID
    let killedKey = sprintf "PID%s" (if killedPIDs.Length = 1 then "" else "s")
    let killedValue = killedPIDs |> List.map string |> String.concat ", "
    let uex =
      if actuallyKilled then
        ExtractException("ELI51484", sprintf "Application trace: Force killed %s" service.Name)
      else
        ExtractException("ELI51485", sprintf "Application trace: Attempted to force kill %s" service.Name)
    uex.AddDebugData(killedKey, killedValue)
    uex.Log()

  let kill service =
    match service.PID with
    | Some pid ->
      let spawnedIDs = getSpawnedProcessIDs pid
      let allPIDs = [int pid; yield! spawnedIDs]
      let results =
        allPIDs
        |> List.map (fun pid -> try Ok (Process.GetProcessById pid) with | e -> Error e )
        |> List.map (fun res ->
          try
            match res with
            | Ok proc -> proc.Kill(); Ok proc.Id
            | Error e -> Error e
          with | e -> Error e
        )
      let pidsKilled = results |> List.choose (function | Ok pid -> Some pid | _ -> None)
      logAppTraceForKilledProcess service pidsKilled
      let pidsNotKilled =
        allPIDs
        |> List.except pidsKilled
        |> List.choose (fun pid -> try Some (Process.GetProcessById pid).Id with | _ -> None)
      pidsKilled, pidsNotKilled
    | None -> failwith "Service is not associated with a process"
