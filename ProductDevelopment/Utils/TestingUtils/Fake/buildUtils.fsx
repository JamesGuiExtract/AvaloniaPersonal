// For editing purposes (if these show up as errors run ..\buildIntellisenseFile.bat)
#if !FAKE
#I "../Fake/.fake/build.fsx"
#load "../Fake/.fake/build.fsx/intellisense.fsx"
#endif

open Fake.Core
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.DotNet

// Set ES_DLL_DIR environment dir if not set so that build references work on dev machine or installed machine
let private defaultESDir =
  if File.exists @"C:\Engineering\Binaries\Debug\ProcessFiles.exe"
  then @"C:\Engineering\Binaries\Debug"
  else @"C:\Program Files (x86)\Extract Systems\CommonComponents"

let esDir = Environment.environVarOrDefault "ES_DLL_DIR" defaultESDir
Environment.setEnvironVar "ES_DLL_DIR" esDir
// Build this solution
Target.create "BuildTestingUtils" (fun _ ->
  let setMsBuildParams (defaults:MSBuild.CliArguments) =
    { defaults with
        Verbosity = Some(Minimal)
        Targets = ["Build"]
        Properties =
            [
                "Optimize", "True"
                "DebugSymbols", "True"
                "Configuration", "Release"
            ]
    }
  let setParams (defaults:DotNet.MSBuildOptions) =
    { defaults with
        MSBuildParams = setMsBuildParams defaults.MSBuildParams
     }

  let sln = __SOURCE_DIRECTORY__ </> @"..\TestingUtils.sln"
  sln |> DotNet.restore id
  sln |> DotNet.msbuild setParams
)
