module TestingUtils

// For editing purposes (if these show up as errors run ..\buildIntellisenseFile.bat)
#if !FAKE
#I "../Fake/.fake/build.fsx"
#load "../Fake/.fake/build.fsx/intellisense.fsx"
#r "netstandard" // fix intellisense issues with 'open System' in Visual Studio
#endif

// Set ES_DLL_DIR environment dir if not set so that build references work on dev machine or installed machine
// Create BuildRuleTestingUtils target
#I "../Fake"
#load @"../Fake/buildUtils.fsx"

#load @"..\CreateFAM\CreateFAMArgs.fs"
#load @"..\CreateFAMDB\CreateFAMDBArgs.fs"

open Fake.Core
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.Tools.Git

let esDir = Environment.environVar "ES_DLL_DIR"

let getServerNameOrDefault defaultName =
  Environment.environVarOrDefault "TESTING_DB_SERVER" defaultName

let getCurrentCommitForDirectory directory =
  CommandHelper.runSimpleGitCommand directory (sprintf """rev-parse --short=12 HEAD""")

let encryptFileAndFail =
  fun fname ->
    if Shell.Exec(esDir </> "EncryptFile.exe", sprintf "\"%s\"" fname) <> 0
    then failwithf "Failed to encrypt file! '%s'" fname

let binDir = __SOURCE_DIRECTORY__ </> @"..\bin\Release"

module FamDB =
  open Argu
  open CreateFAMDBArgs

  // Create DB if it doesn't exist
  let createDB serverName dbName actionName =
    let exeName = "CreateFAMDB.exe"
    let exePath = binDir </> exeName
    let args = [
      Database (serverName, dbName)
      Action_Names [actionName]
    ]
    let parser = ArgumentParser.Create<CreateFAMDBArgs>(programName = "CreateFAMDB.exe")
    let args = parser.PrintCommandLineArgumentsFlat args
    if Shell.Exec(exePath, args) <> 0
    then Trace.log "Create FAM DB failed"

  let dropAllReferencedDBs searchRoot =
    let exePath = binDir </> "DropReferencedDBs.exe"
    let args = sprintf """--search-root "%s" """ searchRoot
    if Shell.Exec(exePath, args) <> 0
    then Trace.log "Drop Referenced DBs failed"


module Fam =
  open CreateFAMArgs
  open Argu

  type CreateFAMParameters = {
    serverName: string
    dbName: string
    actionName: string
    famPath: string
    imageFolders: string list
    imageLists: string list
    rulesPaths: string list
  }
  let defaultParams = {
    serverName = "(local)"
    dbName = "tmp"
    actionName = "a"
    famPath = ""
    imageFolders = []
    imageLists = []
    rulesPaths = []
  }
  let createFAM setParams =
    let exeName = "CreateFAM.exe"
    let exePath = binDir </> exeName
    let p = setParams defaultParams
    let parser = ArgumentParser.Create<CreateFAMArgs>(programName = exeName)
    let mando = [
      Database (p.serverName, p.dbName)
      Action_Name p.actionName
      FAM_Path p.famPath
      Keep_Running true
    ]

    let all = seq {
      yield! mando
      match p.imageFolders with
      | [] -> ()
      | _ -> yield Image_Folders p.imageFolders
      match p.imageLists with
      | [] -> ()
      | _ -> yield Image_Lists p.imageLists
      match p.rulesPaths with
      | [] -> ()
      | _ -> yield Rules_Paths p.rulesPaths
    }

    let args = all |> List.ofSeq |> parser.PrintCommandLineArgumentsFlat
    if Shell.Exec(exePath, args) <> 0
    then failwith "Create FAM failed"

  // Run FPS file and wait for it to finish processing
  let runFAM famPath =
    let exePath = esDir </> "ProcessFiles.exe"
    let args = sprintf """ "%s" /s /fc""" famPath
    printfn "Running FAM with settings from %s..." famPath
    if Shell.Exec(exePath, args) <> 0
    then Trace.log "Run FAM failed!"
    printfn "Done."

module LogProcessStats =
  let run processes seconds statsDir =
    let exePath = esDir </> "LogProcessStats.exe"
    let processesArg = processes |> String.concat ","
    let args = sprintf """ %s %d "%s" /el""" processesArg seconds statsDir
    printfn "Running LogProcessStats for %s..." processesArg
    if Shell.Exec(exePath, args) <> 0
    then Trace.log "Run LogProcessStats failed!"
    printfn "Done."

module Async =
  open System
  open System.Threading
  open System.Threading.Tasks

  let AwaitTaskVoid : (Task -> Async<unit>) = Async.AwaitIAsyncResult >> Async.Ignore

  let DoPeriodicWork (interval: TimeSpan) (token: CancellationToken) (f: unit -> Async<unit>) =
    async {
      while not token.IsCancellationRequested do
        do! f()
        do! Task.Delay(interval, token) |> AwaitTaskVoid
    }

