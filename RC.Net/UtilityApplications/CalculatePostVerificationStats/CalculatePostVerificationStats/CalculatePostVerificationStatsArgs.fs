module CalculatePostVerificationStats.Args

open Argu

type CalculatePostVerificationStatsArgs =
    | [<Mandatory; AltCommandLine("-d")>] Database of serverName: string * dbName: string
    | [<Mandatory; AltCommandLine("-i")>] Input_Action of name: string
    | Limit_To_First of number: string
    | [<Mandatory; AltCommandLine("-e")>] Expected_Attribute_Set of name: string
    | [<Mandatory; AltCommandLine("-f")>] Found_Attribute_Set of name: string
    | [<AltCommandLine("-o")>] Output_File of filename: string
    | Data_Query of xpath: string
    | Flagged_Query of xpath: string
    | [<AltCommandLine("-p")>] Parallel_Degree of number: int

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Database _ -> "DatabaseServer databaseName"
            | Input_Action _ -> "Files completed in this action will be analyzed"
            | Limit_To_First _ -> "Optional, whether to use only the first x files completed in the action"
            | Expected_Attribute_Set _ -> "Attribute set name of the expected VOAs"
            | Found_Attribute_Set _ -> "Attribute set name of the found VOAs"
            | Output_File _ -> "Path to CSV to write results to"
            | Data_Query _ -> "XPath query to select attributes to compare"
            | Flagged_Query _ -> "XPath query to select attributes that flag a document for verification"
            | Parallel_Degree _ -> "Number of files to process at once"
