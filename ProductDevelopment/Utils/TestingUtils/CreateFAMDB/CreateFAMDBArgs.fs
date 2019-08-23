module CreateFAMDBArgs

open Argu

type CreateFAMDBArgs =
  | [<Mandatory>] Database of serverName:string * dbName:string
  | Action_Names of actionName:string list
  | Attribute_Sets of name:string list
  | Metadata_Fields of name:string list

with
  interface IArgParserTemplate with
    member s.Usage =
      match s with
      | Database _ -> "DatabaseServer databaseName"
      | Action_Names _ -> "Action name"
      | Attribute_Sets _ -> "Attribute set names"
      | Metadata_Fields _ -> "Metadata field names"