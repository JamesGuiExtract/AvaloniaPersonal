module SqliteIssueDetector

open Extract.Utilities.FSharp

type SourceIdentifier =
| ControlName of string
| LineNumber of int

type QueryInfo =
  { sourceID: SourceIdentifier
    queryType: string
    queryText: string }

type Message =
| Warning of string * QueryInfo 
| Error of string * QueryInfo 
| Error2 of string * string 
| Failure of string * exn

type FileInfo =
  { path: string
    warningsAndErrors: Message list }

module FileInfo =
  let empty =
    { path = ""
      warningsAndErrors = [] }

  let getDEPInfoFromQueryInfo (path: string) (queries: QueryInfo list) =
    let badQueryErrors =
      [
        yield!
          queries
          |> List.filter (fun queryInfo -> queryInfo.queryText |> Utils.Regex.isMatch """(?inx) \bLEN\s*\(""")
          |> List.map (fun q -> Error ("Query not compatible with SQLite; change LEN function to LENGTH", q))
        yield!
          queries
          |> List.filter (fun queryInfo -> queryInfo.queryText.Contains("+"))
          |> List.map (fun q -> Error ("Query not compatible with SQLite; change + to || for string concatenation", q))
      ]

    let inefficientQueryWarnings =
      queries
      |> List.filter (fun queryInfo -> queryInfo.queryText |> Utils.Regex.isMatch """(?inx) \bSUBSTRING\s*\(""")
      |> List.map (fun q -> Warning ("Inefficient query; contains SUBSTRING() function", q))

    { empty with
        path = path
        warningsAndErrors = badQueryErrors @ inefficientQueryWarnings }

