module DEPUtils

open System
open System.IO
open System.Reflection
open System.Xml

open Extract.DataEntry
open Extract.Utilities.FSharp

open SqliteIssueDetector

let divideQuery (queryInfo: QueryInfo) =
  let fixedQuery = sprintf "<root>%s</root>" queryInfo.queryText
  let doc = XmlDocument()
  doc.LoadXml fixedQuery
  doc.SelectNodes("""//SQL[not(@Connection='FAMDB')]""")
  |> Seq.cast<XmlNode>
  |> Seq.map(fun q -> {queryInfo with queryText = q.InnerText})

// Differentiate data entry controls that support queries from other objects
let (|QueryComboBox|QueryTableColumn|QueryTableRow|QueryTextBox|QueryButton|QueryCheckBox|NotQuery|) (control: obj) =
  match control with
  | :? DataEntryComboBox as control -> QueryComboBox control
  | :? DataEntryTableColumn as control -> QueryTableColumn control
  | :? DataEntryTableRow as control -> QueryTableRow control
  | :? DataEntryTextBox as control -> QueryTextBox control
  | :? DataEntryButton as control -> QueryButton control
  | :? DataEntryCheckBox as control -> QueryCheckBox control
  | _ -> NotQuery control

/// Check DLL to look for problem SQL nodes in AutoUpdateQuery and ValidationQuery properties of fields
let checkAssembly (assemblyPath: string) =
  AssemblyBinder.init()

  try
    let dataEntryControlHostType = typeof<DataEntryControlHost>
    let depAssembly = Assembly.LoadFrom assemblyPath

    let depTypes =
      depAssembly.GetTypes()
      |> Seq.filter dataEntryControlHostType.IsAssignableFrom
      |> Seq.toList

    // There is no common interface or base class that can be used to tell data entry controls apart from other fields
    // so just get all fields and their names
    let controls =
      depTypes
      |> Seq.collect (fun depType ->
        printfn "Checking DEP %s from %s" (depType.Name) assemblyPath
        let dep = Activator.CreateInstance depType
        depType.GetFields(BindingFlags.Instance ||| BindingFlags.NonPublic ||| BindingFlags.Public)
        |> Seq.map (fun fieldInfo -> fieldInfo.Name, fieldInfo.GetValue dep |> box)
      )
      |> Seq.toList

    let queries = 
      controls
      |> Seq.collect (fun (fieldName, control) ->
        let vq = { sourceID = ControlName fieldName; queryType = "ValidationQuery"; queryText = "" }
        let auq = { vq with queryType = "AutoUpdateQuery" }
        match control with
        | QueryComboBox control -> [{ vq with queryText = control.ValidationQuery}; {auq with queryText = control.AutoUpdateQuery}]
        | QueryTableColumn control -> [{ vq with queryText = control.ValidationQuery}; {auq with queryText = control.AutoUpdateQuery}]
        | QueryTableRow control -> [{ vq with queryText = control.ValidationQuery}; {auq with queryText = control.AutoUpdateQuery}]
        | QueryTextBox control -> [{ vq with queryText = control.ValidationQuery}; {auq with queryText = control.AutoUpdateQuery}]
        | QueryButton control -> [{ vq with queryText = control.ValidationQuery}; {auq with queryText = control.AutoUpdateQuery}]
        | QueryCheckBox control -> [{ vq with queryText = control.ValidationQuery}; {auq with queryText = control.AutoUpdateQuery}]
        | NotQuery _ -> []
      )
      |> Seq.filter (fun queryInfo -> not (isNull queryInfo.queryText))
      |> Seq.collect divideQuery
      |> Seq.toList

    FileInfo.getDEPInfoFromQueryInfo assemblyPath queries

  with e ->
    { FileInfo.empty with
        path = assemblyPath
        warningsAndErrors = [Failure (sprintf "Failed to load %s" assemblyPath, e)] }

/// Recursively check dir for DLLs to look for problem SQL nodes in AutoUpdateQuery and ValidationQuery properties of fields
let checkAssembliesInDir (assemblyDir: string) =
  System.IO.Directory.GetFiles (assemblyDir, "*.dll", System.IO.SearchOption.AllDirectories)
  |> Seq.filter (fun path -> (Path.GetFileName path).StartsWith("Extract.DataEntry.DEP", StringComparison.OrdinalIgnoreCase))
  |> Seq.map checkAssembly
  |> Seq.toList

/// Check config file to look for SQLCE connections and problem SQL nodes in AutoUpdateQuery and ValidationQuery properties
let checkConfigFile (configPath: string) =
  try
    printfn "Checking %s" configPath
    let doc = XmlDocument()
    doc.Load configPath
    let queries =
      doc.SelectNodes("""//Member[Property/@name='AutoUpdateQuery']|//Member[Property/@name='ValidationQuery']""")
      |> Seq.cast<XmlNode>
      |> Seq.collect(fun memberNode ->
        let nameAttr = memberNode.Attributes.ItemOf("name")
        let name = if nameAttr |> isNull then "" else nameAttr.Value
        let queryInfo = { sourceID = ControlName name; queryType = ""; queryText = "" }
        memberNode.SelectNodes("""Property[@name='AutoUpdateQuery' or @name='ValidationQuery']""")
        |> Seq.cast<XmlNode>
        |> Seq.map(fun queryNode ->
          let queryType = queryNode.Attributes.ItemOf("name").Value
          {queryInfo with queryText = queryNode.InnerXml; queryType = queryType})
      )
      |> Seq.collect divideQuery
      |> Seq.toList

    let info = FileInfo.getDEPInfoFromQueryInfo configPath queries

    let sqlCeErrors =
      doc.SelectNodes("""//DatabaseConnection/*""")
      |> Seq.cast<XmlNode>
      |> Seq.map(fun node -> node.InnerText)
      |> Seq.filter(fun v -> v |> Regex.isMatch """(?inx) System\.Data\.SqlServerCe""")
      |> Seq.map(fun v -> Error2 ("SQL Compact Database connection", v))
      |> Seq.toList

    let sdfFileErrors =
      doc.SelectNodes("""//value|//DatabaseConnection/*""")
      |> Seq.cast<XmlNode>
      |> Seq.map(fun node -> node.InnerText)
      |> Seq.filter(fun v -> v.EndsWith(".sdf", StringComparison.OrdinalIgnoreCase))
      |> Seq.map(fun v -> Error2 ("SQL Compact Database reference", v))
      |> Seq.toList

    {info with warningsAndErrors = info.warningsAndErrors @ sdfFileErrors @ sqlCeErrors}

  with e ->
    { FileInfo.empty with
        path = configPath
        warningsAndErrors = [Failure (sprintf "Failed to load %s" configPath, e)] }

/// Recursively check dir for config files to look for SQLCE connections and problem SQL nodes in AutoUpdateQuery and ValidationQuery properties
let checkConfigFilesInDir (configDir: string) =
  System.IO.Directory.GetFiles (configDir, "*.config", System.IO.SearchOption.AllDirectories)
  |> Seq.map checkConfigFile
  |> Seq.toList
