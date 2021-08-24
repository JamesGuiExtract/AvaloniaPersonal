module DEPUtils

open System
open System.IO
open System.Reflection
open System.Xml

open Extract.DataEntry
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
let private init =
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

type QueryInfo =
  { controlName: string
    queryType: string
    queryText: string }

type Message =
| Warning of string * QueryInfo 
| Error of string * QueryInfo 
| Error2 of string * string 
| Failure of string * exn

type DEPInfo =
  { path: string
    warningsAndErrors: Message list }

module DEPInfo =
  let empty =
    { path = ""
      warningsAndErrors = [] }

  let getDEPInfoFromQueryInfo (path: string) (queries: QueryInfo list) =
    let badQueryErrors =
      queries
      |> List.filter (fun queryInfo -> queryInfo.queryText |> Utils.Regex.isMatch """(?inx) \bLEN\s*\(""")
      |> List.map (fun q -> Error ("Query not compatible with SQLite; change LEN function to LENGTH", q))

    let inefficientQueryWarnings =
      queries
      |> List.filter (fun queryInfo -> queryInfo.queryText |> Utils.Regex.isMatch """(?inx) \bSUBSTRING\s*\(""")
      |> List.map (fun q -> Warning ("Inefficient query; contains SUBSTRING() function", q))

    { empty with
        path = path
        warningsAndErrors = badQueryErrors @ inefficientQueryWarnings }

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
  init()

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
        let vq = { controlName = fieldName; queryType = "ValidationQuery"; queryText = "" }
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

    DEPInfo.getDEPInfoFromQueryInfo assemblyPath queries

  with e ->
    { DEPInfo.empty with
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
        let queryInfo = { controlName = name; queryType = ""; queryText = "" }
        memberNode.SelectNodes("""Property[@name='AutoUpdateQuery' or @name='ValidationQuery']""")
        |> Seq.cast<XmlNode>
        |> Seq.map(fun queryNode ->
          let queryType = queryNode.Attributes.ItemOf("name").Value
          {queryInfo with queryText = queryNode.InnerXml; queryType = queryType})
      )
      |> Seq.collect divideQuery
      |> Seq.toList

    let info = DEPInfo.getDEPInfoFromQueryInfo configPath queries

    let sqlCeErrors =
      doc.SelectNodes("""//value""")
      |> Seq.cast<XmlNode>
      |> Seq.map(fun node -> node.InnerText)
      |> Seq.filter(fun v -> v |> Regex.isMatch """(?inx) System\.Data\.SqlServerCe""")
      |> Seq.map(fun v -> Error2 ("SQL Compact Database connection", v))
      |> Seq.toList

    let sdfFileErrors =
      doc.SelectNodes("""//value""")
      |> Seq.cast<XmlNode>
      |> Seq.map(fun node -> node.InnerText)
      |> Seq.filter(fun v -> v.EndsWith(".sdf", StringComparison.OrdinalIgnoreCase))
      |> Seq.map(fun v -> Error2 ("SQL Compact Database reference", v))
      |> Seq.toList

    {info with warningsAndErrors = info.warningsAndErrors @ sdfFileErrors @ sqlCeErrors}

  with e ->
    { DEPInfo.empty with
        path = configPath
        warningsAndErrors = [Failure (sprintf "Failed to load %s" configPath, e)] }

/// Recursively check dir for config files to look for SQLCE connections and problem SQL nodes in AutoUpdateQuery and ValidationQuery properties
let checkConfigFilesInDir (configDir: string) =
  System.IO.Directory.GetFiles (configDir, "*.config", System.IO.SearchOption.AllDirectories)
  |> Seq.map checkConfigFile
  |> Seq.toList
