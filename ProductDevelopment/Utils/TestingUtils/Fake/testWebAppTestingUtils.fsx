#r "paket:
nuget Argu
nuget FSharp.Core 4.5.0.0
nuget FSharp.Data
nuget Fake.Core.Target
nuget Fake.DotNet.CLI
nuget Fake.DotNet.MSBuild
nuget Fake.DotNet.NuGet
nuget Fake.IO.FileSystem
nuget Fake.IO.Zip
nuget Fake.Tools.Git
nuget System.Data.SqlClient
//"

#if !FAKE
#load "./.fake/testWebAppTestingUtils.fsx/intellisense.fsx"
#endif

#load "buildUtils.fsx"
#load "../WebAppTestingUtils/WebAppTestingUtils.fsx"

open Fake.Core

// Targets
Target.create "ListTargets" (fun _ ->
  Target.listAvailable()
)

Target.runOrDefaultWithArguments "ListTargets"
