module CreateFAMArgs
open Argu

type CreateFAMArgs =
  | [<Mandatory>] Database of serverName:string * dbName:string
  | [<Mandatory>] Action_Name of actionName:string
  | [<Mandatory>] FAM_Path of famPath:string
  | Workflow_Name of workflowName:string
  | Image_Folders of imageFolders:string list
  | Image_Lists of files:string list
  | Rules_Paths of rulesPaths:string list
  | Found_Attribute_Set of foundName:string
  | Found_Attribute_Path of foundPath:string
  | Backup_Found_To_Path of sourcePath:string * destPath:string
  | Process_Found_Rules_Path of rulesPath:string * voaPath:string
  | Expected_Attribute_Set of expectedName:string
  | Expected_Attribute_Path of expectedPath:string
  | Process_Expected_Rules_Path of rulesPath:string * voaPath:string
  | Set_Metadata_Field of name:string * value:string
  | Set_Action_Status of actionName:string
  | Expand_Attributes of expandAttributes:bool
  | Keep_Running of keepRunning:bool
  | Force_Queueing of forceQueue:bool
with
  interface IArgParserTemplate with
    member s.Usage =
      match s with
      | Database _ -> "DatabaseServer databaseName"
      | Action_Name _ -> "Action name"
      | FAM_Path _ -> "Path to save FPS file"
      | Workflow_Name _ -> "Workflow name"
      | Image_Folders _ -> "Folders to queue"
      | Image_Lists _ -> "Lists to queue"
      | Rules_Paths _ -> "Rule sets to run"
      | Found_Attribute_Set _ -> "Found attribute set name"
      | Found_Attribute_Path _ -> "Attribute path to store into found set"
      | Backup_Found_To_Path _ -> "Copy file before running rules if destination doesn't already exist"
      | Process_Found_Rules_Path _ -> "Rule set to run against found VOA file before storing"
      | Process_Expected_Rules_Path _ -> "Rule set to run against expected VOA file before storing"
      | Expected_Attribute_Set _ -> "Expected attribute set name"
      | Expected_Attribute_Path _ -> "Attribute path to store into expected set"
      | Set_Metadata_Field _ -> "Set metadata field after other processing is complete. Can occur multiple times"
      | Set_Action_Status _ -> "Set this action's status to pending after processing"
      | Keep_Running _ -> "Whether to keep queuing/processing even after existing files have been queued/processed"
      | Expand_Attributes _ -> "Whether to expand stored attributes into discrete fields"
      | Force_Queueing _ -> "Whether to force already processed files to be queued again"