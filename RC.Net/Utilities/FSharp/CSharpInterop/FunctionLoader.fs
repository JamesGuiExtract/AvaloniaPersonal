namespace Extract.Utilities.FSharp

open System
open System.IO
open System.Text
open System.Threading
open FSharp.Compiler.Interactive.Shell

open Extract
open Extract.Utilities
open System.Runtime.InteropServices

type Transformation<'TData> = 'TData -> 'TData

[<AbstractClass; Sealed>]
type FunctionLoader() =
    static let fsiLock = obj ()

    // Setup function to create an FSI instance
    static let getEvaluationSessionCreator includeDirectories collectible =
        let sbOut = StringBuilder()
        let sbErr = StringBuilder()
        let inStream = new StringReader("")
        let outStream = new StringWriter(sbOut)
        let errStream = new StringWriter(sbErr)
        let fsiConfig = FsiEvaluationSession.GetDefaultConfiguration()

        let argv =
            seq {
                yield "fsi" // Dummy argument required for historical reasons
                yield "--simpleresolution" // Do not use MSBuild for reference resolution
                yield "--quiet" // Don't print to std out
                yield "--noninteractive" // Don't start a thread to watch for user input
                yield "--nowarn:211" // Don't warn about nonexistent #Included directories
                yield "-d:FUNCTION_LOADER" // Allow conditional compilation via #if FUNCTION_LOADER

                yield!
                    (includeDirectories
                     |> Seq.map (sprintf @"-I:""%s"""))
            }
            |> Seq.toArray

        fun () ->
            try
                FsiEvaluationSession.Create(fsiConfig, argv, inStream, outStream, errStream, collectible = collectible)
            with
            | ex -> raise (ExtractException("ELI47006", "Error creating FSI session", ex))

    // Load a script file with fsi and get the specified functions
    // Assumes the functions are in a module with the same name as the file name, minus extension
    static let evaluate path functionNames includeDirectories referenceAssemblies collectible =
        let fullPath = Path.GetFullPath path
        let scriptDir = Path.GetDirectoryName fullPath
        ExtractException.Assert("ELI46985", "Path is not a full path", (path = fullPath), "Path", path)

        let referenceAssemblies =
            if referenceAssemblies |> isNull then
                [||]
            else
                referenceAssemblies

        let includeDirectories =
            if includeDirectories |> isNull then
                [||]
            else
                includeDirectories

        let includeDirectories =
            seq {
                yield scriptDir
                yield FileSystemMethods.CommonComponentsPath
                yield! includeDirectories
            }
            |> Seq.distinct

        let sessionCreator = getEvaluationSessionCreator includeDirectories collectible

        // Prevent deadlock if this thread already has the lock
        ExtractException.Assert(
            "ELI51863",
            "Cannot create a new FSI session inside an FSI session",
            not (Monitor.IsEntered fsiLock)
        )

        // Creating a session and evaluating code isn't threadsafe so use a lock around all FSI evaluation code
        // https://extract.atlassian.net/browse/ISSUE-17681
        lock fsiLock (fun () ->
            let fsi = sessionCreator ()

            // Load the script file
            let initScript =
                let moduleName = Path.GetFileNameWithoutExtension path
                let extension = Path.GetExtension path

                let isSource =
                    String.Equals(".fs", extension, StringComparison.OrdinalIgnoreCase)
                    || String.Equals(".fsx", extension, StringComparison.OrdinalIgnoreCase)

                seq {
                    yield!
                        referenceAssemblies
                        |> Seq.map (sprintf @"#r @""%s""")

                    if isSource then
                        sprintf @"#load @""%s""" fullPath
                    else
                        sprintf @"#r @""%s""" fullPath

                    sprintf "open %s;;" moduleName
                }
                |> String.concat Environment.NewLine

            let res, errs = fsi.EvalInteractionNonThrowing(initScript)

            match res with
            | Choice2Of2 ex ->
                let uex = ExtractException("ELI46925", "Error loading script", ex)
                uex.AddDebugData("Script path", fullPath, false)

                errs
                |> Array.iter (fun errorInfo -> uex.AddDebugData("Error", errorInfo.ToString(), false))

                raise uex
            | _ -> ()

            let typeName = sprintf "%s.%s" typeof<'TData>.Namespace typeof<'TData>.Name

            // Get the functions
            functionNames
            |> Array.map (fun functionName ->
                // Annotate the function name with its full type to avoid value restriction exception
                let getFunctionScript = sprintf "%s : %s -> %s" functionName typeName typeName

                let res, errs = fsi.EvalExpressionNonThrowing getFunctionScript

                match res with
                | Choice1Of2 (Some f) -> f.ReflectionValue :?> Transformation<'TData>
                | _ ->
                    let uex = ExtractException("ELI46954", "Error getting function")
                    uex.AddDebugData("Script path", fullPath, false)
                    uex.AddDebugData("Function name", functionName, false)

                    errs
                    |> Array.iter (fun errorInfo -> uex.AddDebugData("Error", errorInfo.ToString(), false))

                    raise uex))

    /// Load a single function from a source file or assembly
    static member LoadFunction<'TData>
        (
            scriptPath,
            functionName,
            collectible,
            [<Optional>] includeDirectories: string [],
            [<Optional>] referenceAssemblies: string []
        ) : Transformation<'TData> =

        evaluate scriptPath [| functionName |] includeDirectories referenceAssemblies collectible
        |> Array.head

    /// Load specified functions from a source file or assembly
    static member LoadFunctions<'TData>
        (
            scriptPath,
            functionNames,
            collectible,
            [<Optional>] includeDirectories: string [],
            [<Optional>] referenceAssemblies: string []
        ) : Transformation<'TData> [] =

        evaluate scriptPath functionNames includeDirectories referenceAssemblies collectible
