module Extract.Utilities.FSharp.FunctionLoader

open System

type Transformation<'TData> = 'TData -> 'TData

module Evaluator =
    open System.IO
    open System.Text
    open Extract.Utilities
    open Microsoft.FSharp.Compiler.Interactive.Shell

    // Load a script file with fsi and get the specified functions
    // Assumes the functions are in a module with the same name as the file name, minus extension
    let evaluate path functionNames =
        let fullPath = Path.GetFullPath path
        Extract.ExtractException.Assert("ELI46985", "Path is not a full path", (path = fullPath), "Path", path);

        let fsi =
            let sbOut = StringBuilder()
            let sbErr = StringBuilder()
            let inStream = new StringReader("")
            let outStream = new StringWriter(sbOut)
            let errStream = new StringWriter(sbErr)
            try
                let fsiConfig = FsiEvaluationSession.GetDefaultConfiguration()
                let fsiPath = Path.Combine(FileSystemMethods.CommonComponentsPath, "fsi.exe")
                let argv = [| fsiPath |]
                FsiEvaluationSession.Create(fsiConfig, argv, inStream, outStream, errStream, collectible = true)
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
            uex.AddDebugData("Errors", sprintf "%A" errs, false)
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
                uex.AddDebugData("Errors", sprintf "%A" errs, false)
                raise uex
        )

// Load a single function from a file
// Throws if the script can't be loaded or the function doesn't exist
let LoadFunction<'TData>(scriptPath, functionName): Transformation<'TData> =
    Evaluator.evaluate scriptPath [| functionName |]
    |> Array.head

// Loads functions from a file
// Throws if the script can't be loaded or any of the functions don't exist
let LoadFunctions<'TData>(scriptPath, ([<ParamArray>] functionNames)): Transformation<'TData>[] =
    Evaluator.evaluate scriptPath functionNames
