module Fam

open System.Data.SqlClient
open UCLID_FILEPROCESSINGLib

let loadFrom path =
  let fam = FileProcessingManagerClass()
  fam.LoadFrom(path, false)
  fam

let dropReferencedDB (fam:FileProcessingManagerClass) =
  let serverName = fam.DatabaseServer
  let dbName = fam.DatabaseName
  let connectionString = sprintf "server=%s;Trusted_Connection=yes" serverName

  use connection = new SqlConnection(connectionString)
  connection.Open()

  let dropDB () =
    use command = new SqlCommand(sprintf "DROP DATABASE %s" dbName, connection)
    command.ExecuteNonQuery() |> ignore

  let exists =
    use command = new SqlCommand("SELECT db_id(@databaseName)", connection)
    command.Parameters.Add(SqlParameter("databaseName", dbName)) |> ignore
    command.ExecuteScalar() <> (System.DBNull.Value :> obj)

  if exists then dropDB()
