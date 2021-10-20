module BatchUtils

open System.IO

open Extract.Utilities.FSharp

open SqliteIssueDetector

/// Check .bat file to look for .sdf references and invalid sqlite functions
let checkBatchFile (batPath: string) =
  try
    printfn "Checking %s" batPath
    let lines = File.ReadAllLines batPath
    let queries =
      lines
      |> Seq.mapi (fun i line ->
        { sourceID = LineNumber (i + 1); queryType = ""; queryText = line }
      )
      |> Seq.toList

    let info = FileInfo.getDEPInfoFromQueryInfo batPath queries

    let sdfFileErrors =
      lines
      |> Seq.filter(fun v -> v |> Regex.isMatch """(?inx) \.sdf\b""")
      |> Seq.map(fun v -> Error2 ("SQL Compact Database reference", v))
      |> Seq.toList

    {info with warningsAndErrors = info.warningsAndErrors @ sdfFileErrors}

  with e ->
    { FileInfo.empty with
        path = batPath
        warningsAndErrors = [Failure (sprintf "Failed to load %s" batPath, e)] }

/// Recursively check dir for config files to look for SQLCE connections and problem SQL nodes in AutoUpdateQuery and ValidationQuery properties
let checkBatchFilesInDir (batDir: string) =
  System.IO.Directory.GetFiles (batDir, "*.bat", System.IO.SearchOption.AllDirectories)
  |> Seq.map checkBatchFile
  |> Seq.toList
