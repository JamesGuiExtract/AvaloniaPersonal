module FamDB

open System
open System.Data.SqlClient
open UCLID_FILEPROCESSINGLib
open AttributeDbMgrComponentsLib

let exists serverName (dbName: string) =
  let connectionString = sprintf "server=%s;Trusted_Connection=yes" serverName
  use connection = new SqlConnection(connectionString)
  use command = new SqlCommand("SELECT db_id(@databaseName)", connection)
  command.Parameters.Add(SqlParameter("databaseName", dbName)) |> ignore
  connection.Open()
  command.ExecuteScalar() <> (DBNull.Value :> obj)

let createDB serverName dbName =
  if exists serverName dbName
  then failwithf "Database '%s' already exists on server '%s'" dbName serverName

  let famDB = FileProcessingDBClass()
  famDB.DatabaseServer <- serverName
  famDB.CreateNewDB(dbName, "a")
  famDB

let connectDB serverName dbName =
  let famDB = FileProcessingDBClass()
  famDB.DatabaseServer <- serverName
  famDB.DatabaseName <- dbName
  famDB

let addAction actionName (famDB: FileProcessingDBClass) =
  famDB.DefineNewAction actionName |> ignore

let addMetadataField metadataName (famDB: FileProcessingDBClass) =
  famDB.AddMetadataField metadataName |> ignore

let addAttributeSet attributeSetName (famDB: FileProcessingDBClass) =
  let attrDBMgr = AttributeDBMgrClass()
  attrDBMgr.FAMDB <- famDB
  attrDBMgr.CreateNewAttributeSetName attributeSetName |> ignore