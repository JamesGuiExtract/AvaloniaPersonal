#r "paket:
nuget FSharp.Core 4.5.0.0
nuget Fake.Core.Target
//"

#load "./.fake/build.fsx/intellisense.fsx"

open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core
open System.Text.RegularExpressions

type NoCase(value) =
  member val Value = value

  override this.Equals(that) =
    match that with 
    | :? NoCase as other -> System.StringComparer.InvariantCultureIgnoreCase.Equals(this.Value, other.Value)
    | _ -> false

  override this.GetHashCode() =
    System.StringComparer.InvariantCultureIgnoreCase.GetHashCode(this.Value)

  interface System.IComparable with
    member this.CompareTo obj =
        let other : NoCase = downcast obj
        System.StringComparer.InvariantCultureIgnoreCase.Compare(this.Value, other.Value)

// Properties

// Targets
Target.create "ListTargets" (fun _ ->
  Target.listAvailable()
)

type ProjectPlatform(m : Match) =
  member x.Guid = m.Groups.["guid"].Value
  member x.SlnMode = m.Groups.["slnMode"].Value
  member x.ProjMode = m.Groups.["projMode"].Value
  member x.Platform = Regex.Replace(m.Groups.["platform"].Value, "Any CPU", "AnyCPU")

type ProjectDetails(m : Match) =
  let rest = m.Groups.["rest"].Value
  let deps = Regex.Matches(rest, @"(?inxsm)\bProjectSection\(ProjectDependencies\)\s=\spostProject(?'dep'\s+(?'guid1'\{[-\w]{36}\})\s=\s(?'guid2'\{[-\w]{36}\}))+\s+EndProjectSection\r?$")
  member x.Guid = m.Groups.["guid"].Value
  member x.Name = m.Groups.["name"].Value
  member x.Path = m.Groups.["path"].Value
  member val Platforms : Map<string, ProjectPlatform> = Map.empty with get, set
  member val AltName : string option = None with get, set
  member val Dependencies =
    if deps.Count = 0
    then List.empty
    else
      let depsMatch = deps.[0]
      seq {0 .. depsMatch.Groups.["dep"].Captures.Count - 1}
      |> Seq.map (fun i ->
        let guid1 = depsMatch.Groups.["guid1"].Captures.[i].Value
        let guid2 = depsMatch.Groups.["guid2"].Captures.[i].Value
        if guid1 <> guid2 then failwith "WTF!"
        else guid1)
      |> Seq.toList
    with get, set

let buildScript slnFile outputName =
  if Shell.testFile outputName
  then Shell.rm outputName

  // Functions
  let output endl fmt =
    Printf.kprintf (fun str ->
      File.writeString true outputName (str + endl))
      fmt

  // let outputfn = sprintf >> outputfn
  let outputfn fmt =
    output "\r\n" fmt

  let outputf fmt =
    output "" fmt

  let slnDir = Path.getDirectory slnFile
  Shell.chdir slnDir
  let sln = File.readAsString slnFile

  // Get projects
  let matches = Regex.Matches(sln, """(?inxsm)^Project\("\{[-\w]{36}\}"\)\s=\s"(?'name'[^"]+)",\s"(?'path'[^"]+proj)",\s"(?'guid'\{[-\w]{36}\})"(?'rest'[\S\s]*?(?=^EndProject\r?$))""")

  let projects =
    matches
    |> Seq.cast
    |> Seq.map (fun (m: Match) -> m.Groups.["guid"].Value, ProjectDetails m)
    |> Map.ofSeq

  // Get configuration platforms for each project GUID
  let platformsText = Regex.Match(sln, @"(?inxsm)GlobalSection\(ProjectConfigurationPlatforms\)\s=\spostSolution[\S\s]+?EndGlobalSection").Value
  let platformMatches = Regex.Matches(platformsText, @"(?inxsm)(?'guid'\{[-\w]{36}\})\.(?'slnMode'Debug|Release)\|(Mixed\sPlatforms|Any\sCPU)\.ActiveCfg\s=\s(?'projMode'Debug|Release)\|(?'platform'[^\r\n]+)")

  platformMatches
  |> Seq.cast
  |> Seq.map ProjectPlatform
  |> Seq.iter (fun platformInfo ->
    if projects.ContainsKey platformInfo.Guid
    then let proj = projects.[platformInfo.Guid]
         proj.Platforms <- proj.Platforms.Add(platformInfo.SlnMode, platformInfo)
  )

  projects
  |> Seq.iter (fun p ->
    let folder = Path.getDirectory p.Value.Path
    let idlFile = !! (folder </> "*.idl") |> Seq.tryHead
    match idlFile with
    | Some file ->
      let idl = File.readAsString file
      let m = Regex.Match(idl, @"(?inxsm)^library\s+(?'library'\w+)")
      if m.Success
      then p.Value.AltName <- Some(m.Groups.["library"].Value)
      else printfn "Couldn't find library name for %s" file
    | None -> ()
  )

  let projectsByName =
    projects
    |> Seq.map (fun kv -> NoCase kv.Value.Name, kv.Value)
    |> Map.ofSeq

  let projectsByAltName =
    projects
    |> Seq.choose (fun kv ->
      match kv.Value.AltName with
      | Some name -> Some(NoCase name, kv.Value)
      | None -> None
    )
    |> Map.ofSeq

  let addDependency =
    fun name dependentOnName ->
      if not(projectsByName.ContainsKey (NoCase name))
      then printfn "Dependency not added (project %s doesn't exist)" name
      elif not(projectsByName.ContainsKey (NoCase dependentOnName))
      then printfn "Dependency not added (project %s doesn't exist)" dependentOnName
      else
        let p = projectsByName.[NoCase name]
        let depGuid = projectsByName.[NoCase dependentOnName].Guid
        if (List.contains depGuid p.Dependencies)
        then printfn "Dependency already exists! %s ==> %s" dependentOnName name
        else p.Dependencies <- (depGuid :: p.Dependencies)
  
  // Dependencies for project references
  projects
  |> Seq.iter (fun kv ->
    let p = kv.Value
    let projFile = p.Path
    let proj = File.readAsString projFile
    let matches = Regex.Matches(proj, @"(?inxsm)<ProjectReference[\S\s]*?<Project>(?'guid'\{[-\w]{36}\})")
    let newRefs = matches
                  |> Seq.cast
                  |> Seq.map (fun (m: Match) -> m.Groups.["guid"].Value.ToUpperInvariant())
                  |> Seq.where (fun guid -> projects.ContainsKey(guid))
                  |> Seq.toList

    p.Dependencies <- List.concat [p.Dependencies; newRefs]
  )

  // Dependencies for COM references
  projects
  |> Seq.iter (fun kv ->
    let p = kv.Value
    let projFile = p.Path
    let proj = File.readAsString projFile
    let matches = Regex.Matches(proj, @"(?inxsm)(<COMReference\sInclude=""|<Reference\sInclude=""Interop\.)(?'fullname'((?'prefix'(UCLID)?)_?)(?'name'[^""]+?)(Lib)?)(\.dll)?""")
    let newRefs = matches
                  |> Seq.cast
                  |> Seq.choose (fun (m: Match) ->
                      let fullName = m.Groups.["fullname"].Value
                      let prefix = m.Groups.["prefix"].Value
                      let name = m.Groups.["name"].Value

                      if projectsByName.ContainsKey(NoCase name)
                      then Some(projectsByName.[NoCase name].Guid)
                      elif projectsByName.ContainsKey(NoCase(prefix + name))
                      then Some(projectsByName.[NoCase(prefix + name)].Guid)
                      elif projectsByAltName.ContainsKey(NoCase fullName)
                      then Some(projectsByAltName.[NoCase fullName].Guid)
                      elif fullName <> "ADODB"
                      then printfn "Project not found: \"%s\"" fullName
                           None
                      else None
                  )
                  |> Seq.toList
    let missing = Seq.except p.Dependencies newRefs |> Seq.toList
    if not missing.IsEmpty
    then
      let missingAsString = String.concat "\n" (missing |> Seq.map (fun guid -> sprintf "\t\t%s = %s" guid guid))
      printfn "Project %s is missing dependencies \n%s" p.Name missingAsString
      p.Dependencies <- List.concat [p.Dependencies; missing]
  )

  // Add missing dependencies
  addDependency "GenerateDotNetLicenseIDFiles" "UCLIDExceptionMgmt"

  outputfn """
#r "paket:
nuget FSharp.Core 4.5.0.0
nuget Fake.Core.Target
nuget Fake.Core.Tasks
nuget Fake.DotNet.MSBuild
//"

#load "./.fake/%s/intellisense.fsx"

open Fake.Core
open Fake.Core.TargetOperators
open Fake.Core.TaskRunner
open Fake.DotNet

let numRetries = 3
let mutable buildMode = "Debug"

let vs2017Path = @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\"
let vs2019Path = @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\"
if System.IO.Directory.Exists vs2019Path
then Environment.setEnvironVar "DevEnvDir" vs2019Path
elif System.IO.Directory.Exists vs2017Path
then Environment.setEnvironVar "DevEnvDir" vs2017Path

// Targets
Target.create "All.Clean" ignore
Target.create "All.Build" ignore

Target.create "ListTargets" (fun _ ->
  Target.listAvailable()
)""" (System.IO.Path.GetFileName(outputName))

  projects
  |> Seq.iter (fun kv ->
    let p = kv.Value
    let projName = p.Name
    printfn "%s" p.Name
    let projPath = Path.getFullName p.Path
    outputfn """
Target.create "%s.Build" (fun _ ->
  buildMode <- Environment.environVarOrDefault "buildMode" "Debug"
  let setParams (defaults : MSBuildParams) =
    { defaults with
        Verbosity = Some(Minimal)
        Targets = ["Build"]
        Properties =
          [
            "Configuration", %s
            "BuildProjectReferences", "False"
          ]
     }

  runWithRetries (fun () -> MSBuild.build setParams @"%s") numRetries
)"""
      projName
      (sprintf @"if buildMode = ""Release"" then ""%s"" else ""%s""" (p.Platforms.["Release"].ProjMode) (p.Platforms.["Debug"].ProjMode))
      projPath

    outputfn """
Target.create "%s.Clean" (fun _ ->
  buildMode <- Environment.environVarOrDefault "buildMode" "Debug"
  let setParams (defaults : MSBuildParams) =
    { defaults with
        Verbosity = Some(Minimal)
        Targets = ["Clean"]
        Properties =
          [
            "Configuration", %s
            "BuildProjectReferences", "False"
          ]
     }

  runWithRetries (fun () -> MSBuild.build setParams @"%s") numRetries
)"""
      projName
      (sprintf @"if buildMode = ""Release"" then ""%s"" else ""%s""" (p.Platforms.["Release"].ProjMode) (p.Platforms.["Debug"].ProjMode))
      projPath
  )

  outputfn "\r\n// Dependencies"

  projects
  |> Seq.map (fun kv -> kv.Value)
  |> Seq.sortBy (fun p -> p.Name)
  |> Seq.iter (fun p ->
    let projName = p.Name
    if not (List.isEmpty p.Dependencies)
    then
      let depListAsString =
        String.concat "; "
          (p.Dependencies
            |> Seq.map (fun depGuid -> sprintf "\"%s.Build\"" projects.[depGuid].Name)
            |> Seq.sort)
      outputfn "\"%s.Build\" <== [%s]" projName depListAsString
  )

  let dependendedOn = projects |> Seq.collect (fun kv -> kv.Value.Dependencies) |> Set.ofSeq
  let babies =
    projects
    |> Seq.where (fun kv -> not (dependendedOn.Contains kv.Key))
    |> Seq.map (fun kv -> kv.Value)
    |> Seq.toList
  
  outputfn ""

  let topLevelListAsString =
    String.concat "; "
      (babies
        |> Seq.map (fun p -> sprintf "\"%s.Build\"" p.Name)
        |> Seq.sort)
  outputfn "\"All.Build\" <== [%s]" topLevelListAsString

  projects
  |> Seq.iter (fun kv ->
    kv.Value.Dependencies
    |> Seq.iter (fun d ->
        let p = projects.[d]
        let cleanDep = kv.Value
        outputfn "\"%s.Clean\" ==> \"%s.Clean\"" cleanDep.Name p.Name
    )
  )
  let independent =
    projects
    |> Seq.where (fun kv -> kv.Value.Dependencies.IsEmpty)
    |> Seq.map (fun kv -> kv.Value)
    |> Seq.toList
  let topLevelListAsString =
    String.concat "; "
      (independent
        |> Seq.map (fun p -> sprintf "\"%s.Clean\"" p.Name)
        |> Seq.sort)
  outputfn "\"All.Clean\" <== [%s]" topLevelListAsString

  outputfn "Target.runOrDefaultWithArguments \"ListTargets\""

Target.create "AFCoreTestBuild.Build" (fun _ ->
  Shell.chdir __SOURCE_DIRECTORY__
  let slnFile = @"C:\Engineering\ProductDevelopment\AttributeFinder\AFCore\AFCoreTest\Code\AFCoreTest.sln"
  let outputName = Path.getFullName "buildAFCoreTest.fsx"
  buildScript slnFile outputName
)

Target.create "LearningMachineTrainerBuild.Build" (fun _ ->
  Shell.chdir __SOURCE_DIRECTORY__
  let slnFile = @"C:\Engineering\RC.Net\UtilityApplications\LearningMachineTrainer\Code\LearningMachineTrainer.sln"

  let outputName = Path.getFullName "buildLearningMachineTrainer.fsx"
  buildScript slnFile outputName
)

Target.create "UtilsBuild.Build" (fun _ ->
  Shell.chdir __SOURCE_DIRECTORY__
  let slnFile = @"C:\Engineering\ProductDevelopment\Utils\UCLIDUtilApps\Code\Utils.sln"

  let outputName = Path.getFullName "buildUtils.fsx"
  buildScript slnFile outputName
)

Target.create "DashboardsBuild.Build" (fun _ ->
  Shell.chdir __SOURCE_DIRECTORY__
  let slnFile = @"C:\Engineering\RC.Net\Dashboards\Dashboards.sln"

  let outputName = Path.getFullName "buildDashboards.fsx"
  buildScript slnFile outputName
)

Target.create "All.Build" ignore

open Fake.Core.TargetOperators

"AFCoreTestBuild.Build"
  ==> "All.Build"

"LearningMachineTrainerBuild.Build"
  ==> "All.Build"

"UtilsBuild.Build"
  ==> "All.Build"

"DashboardsBuild.Build"
  ==> "All.Build"

Target.runOrDefaultWithArguments "ListTargets"
