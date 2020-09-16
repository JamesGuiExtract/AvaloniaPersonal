module Extract.Utilities.FSharp.FunctionLoader

open System

type Transformation<'TData> = 'TData -> 'TData

module Evaluator =
    open System.IO
    open System.Text
    open Microsoft.FSharp.Compiler.Interactive.Shell

    // Load a script file with fsi and get the specified functions
    // Assumes the functions are in a module with the same name as the file name, minus extension
    let evaluate path functionNames includeDirectories collectible =
        let fullPath = Path.GetFullPath path
        let scriptDir = Path.GetDirectoryName fullPath
        Extract.ExtractException.Assert("ELI46985", "Path is not a full path", (path = fullPath), "Path", path);

        let includeDirectories =
          seq {
            yield scriptDir
            yield Extract.Utilities.FileSystemMethods.CommonComponentsPath
            yield! includeDirectories
          }
          |> Seq.distinct

        let fsi =
            let sbOut = StringBuilder()
            let sbErr = StringBuilder()
            let inStream = new StringReader("")
            let outStream = new StringWriter(sbOut)
            let errStream = new StringWriter(sbErr)
            try
                let fsiConfig = FsiEvaluationSession.GetDefaultConfiguration()
                let argv =
                  seq {
                    yield "fsi" // Dummy argument required for historical reasons
                    yield "--simpleresolution" // Do not use MSBuild for reference resolution
                    yield "--quiet" // Don't print to std out
                    yield "--noninteractive" // Don't start a thread to watch for user input
                    yield "--nowarn:211" // Don't warn about nonexistent #Included directories
                    yield "-d:FUNCTION_LOADER" // Allow conditional compilation via #if FUNCTION_LOADER
                    yield! (includeDirectories |> Seq.map (sprintf @"-I:""%s"""))
                  } |> Seq.toArray
                FsiEvaluationSession.Create(fsiConfig, argv, inStream, outStream, errStream, collectible = collectible)
            with
            | ex ->
                let uex = Extract.ExtractException("ELI47006", "Error creating FSI session", ex)
                raise uex

        let moduleName = Path.GetFileNameWithoutExtension path
        let initScript = sprintf """#load @"%s"; open %s;;""" fullPath moduleName
        let res, errs = fsi.EvalInteractionNonThrowing(initScript)
        match res with
        | Choice2Of2 ex ->
            let uex = Extract.ExtractException("ELI46925", "Error loading script", ex)
            uex.AddDebugData("Script path", fullPath, false)
            errs |> Array.iter (fun errorInfo ->
               uex.AddDebugData("Error", errorInfo.ToString (), false))
            raise uex
        | _ -> ()

        let typeName = sprintf "%s.%s" typeof<'TData>.Namespace typeof<'TData>.Name

        functionNames
        |> Array.map (fun functionName ->
            // Annotate the function name with its full type to avoid value restriction exception
            let getFunctionScript = sprintf "%s : %s -> %s" functionName typeName typeName
            let res, errs = fsi.EvalExpressionNonThrowing getFunctionScript
            match res with
            | Choice1Of2 (Some f) -> f.ReflectionValue :?> Transformation<'TData>
            | _ ->
                let uex = Extract.ExtractException("ELI46954", "Error getting function")
                uex.AddDebugData("Script path", fullPath, false)
                uex.AddDebugData("Function name", functionName, false)
                errs |> Array.iter (fun errorInfo ->
                   uex.AddDebugData("Error", errorInfo.ToString (), false))
                raise uex
        )

// Load a single function from a file
// Throws if the script can't be loaded or the function doesn't exist
let LoadFunction<'TData>(scriptPath, functionName, collectible, ([<ParamArray>] includeDirectories : string[])): Transformation<'TData> =
    Evaluator.evaluate scriptPath [| functionName |] includeDirectories collectible
    |> Array.head

// Loads functions from a file
// Throws if the script can't be loaded or any of the functions don't exist
let LoadFunctions<'TData>(scriptPath, functionNames, collectible, [<ParamArray>] includeDirectories): Transformation<'TData>[] =
    let includeDirectories = if includeDirectories = null then [||] else includeDirectories
    Evaluator.evaluate scriptPath functionNames includeDirectories collectible
