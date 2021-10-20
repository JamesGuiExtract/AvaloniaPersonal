module AssemblyBinder

open System
open System.IO
open System.Reflection

open Extract.Utilities
open Extract.Utilities.FSharp

let redirectAssemblies (assemblyNames: AssemblyName seq) = 
  let shortNameToAssemblyName =
    assemblyNames
    |> Seq.map (fun name -> name.Name, name)
    |> Map.ofSeq
  AppDomain.CurrentDomain.add_AssemblyResolve
    (fun _sender eventArgs ->
      let shortName = AssemblyName(eventArgs.Name).Name
      match shortNameToAssemblyName |> Map.tryFind shortName with
      | Some assemblyName ->
        try
          Assembly.Load assemblyName
        with _ ->
          Unchecked.defaultof<Assembly>
      | None -> Unchecked.defaultof<Assembly>)
    
// Setup binding redirects if not already done
let init =
  let esDir = FileSystemMethods.CommonComponentsPath
  let monitor = Object()
  let mutable initialized = false
  let extractToken = (typeof<Extract.ExtractException>.Assembly).GetName().GetPublicKeyToken()

  fun () ->
    if not initialized then
      lock monitor (fun () ->
        if not initialized then
          Directory.EnumerateFiles(esDir, "*.dll")
          |> Seq.append (Directory.EnumerateFiles(esDir, "*.exe"))
          |> Seq.choose (fun assemblyPath ->
            try
              let name = AssemblyName.GetAssemblyName assemblyPath
              if name.GetPublicKeyToken() = extractToken then
                Some name
              else
                None
            with _ -> None
          )
          |> redirectAssemblies
          initialized <- true
      )

