module CalculatePostVerificationStats.Database

open System
open System.Data
open System.Data.SqlClient

open Extract.AttributeFinder
open Extract.SqlDatabase
open Extract.Utilities.FSharp.Utils
open UCLID_COMUTILSLib

type DatabaseInfo =
    { Server: string
      Name: string
      VerifyActions: string list
      ExpAttributeSet: string
      FndAttributeSet: string }

    static member empty =
        { Server = ""
          Name = ""
          VerifyActions = []
          FndAttributeSet = ""
          ExpAttributeSet = "" }

type IDocumentSource =
    /// Get all files that are complete in one of the configured actions
    abstract member getFilesCompleteForActions: unit -> ResizeArray<string>
    /// Get the first (earliest transition to the complete status) n files that are complete in one of the configured actions
    abstract member getFirstFilesCompleteForActions: limit: int -> ResizeArray<string>

type IAttributeSource =
    abstract member getFndAttributes: fileName: string -> (IUnknownVector * DateTime) option
    abstract member getExpAttributes: fileName: string -> (IUnknownVector * DateTime) option

type Database(db: DatabaseInfo) =

    let getDBConnection db =
        new ExtractRoleConnection(SqlUtil.CreateConnectionString(db.Server, db.Name))

    let enumerate (reader: IDataReader) : 'a seq =
        seq {
            while reader.Read() do
                yield reader :?> 'a
        }

    let getValues db query getter =
        seq {
            use con = getDBConnection db
            con.Open()
            use cmd = con.CreateCommand(CommandTimeout = 0)
            cmd.CommandText <- query
            use reader = cmd.ExecuteReader()

            for row in reader |> enumerate do
                yield getter row
        }

    let escape (s: string) = s.Replace("'", "''")

    let escapeAll (strings: #seq<string>) =
        strings |> Seq.map escape |> String.concat "', '"

    let filesForActionsQuery statuses actions =
        let statuses = escapeAll statuses
        let actions = escapeAll actions

        sprintf
            """
            select distinct FileName from FAMFile with (NOLOCK)
            join FileActionStatus with (NOLOCK) on FAMFile.ID = FileActionStatus.FileID
            join Action on FileActionStatus.ActionID = Action.ID
            where FileActionStatus.ActionStatus in ('%s')
            and ASCName in ('%s')
            """
            statuses
            actions

    let firstFilesForActionsQuery statuses actions limit =
        let statuses = escapeAll statuses
        let actions = escapeAll actions

        sprintf
            """
            ;with FirstCompleteFiles as
            (select top(%d) FileActionStatus.FileID, min(DateTimeStamp) as FirstComplete
            from FileActionStatus with (NOLOCK)
			join FileActionStateTransition with (NOLOCK)
                on FileActionStatus.ActionID = FileActionStateTransition.ActionID
                and FileActionStatus.FileID = FileActionStateTransition.FileID
            join Action on FileActionStatus.ActionID = Action.ID
            where ASCName in ('%s')
            and FileActionStatus.ActionStatus in ('%s')
            and ASC_To in ('%s')
            group by FileActionStatus.FileID
            order by FirstComplete asc)

			select FAMFile.FileName from FAMFile with (NOLOCK)
            join FirstCompleteFiles on FAMFile.ID = FirstCompleteFiles.FileID
            """
            limit
            actions
            statuses
            statuses

    let filesCompleteForActionsQuery (actions: string list) = filesForActionsQuery [ "C" ] actions

    let getAttributes db attributeSetName fileName =
        let getAttributesQuery =
            sprintf
                """
                select AttributeSetForFile.VOA, DateTimeStamp
                from AttributeSetForFile with (NOLOCK)
                join AttributeSetName on AttributeSetForFile.AttributeSetNameID = AttributeSetName.ID
                join FileTaskSession FTS with (NOLOCK) on AttributeSetForFile.FileTaskSessionID = FTS.ID
                join FAMFile with (NOLOCK) on FTS.FileID = FAMFile.ID
                where FileName = '%s'
                and Description = '%s'
                and DateTimeStamp =
                (
                    select max(DateTimeStamp)
                    from FileTaskSession with (NOLOCK)
                    join AttributeSetForFile with (NOLOCK) on AttributeSetForFile.FileTaskSessionID = FileTaskSession.ID
                    where FileID = FTS.FileID
                    and AttributeSetForFile.AttributeSetNameID = AttributeSetName.ID
                )
                """
                (escape fileName)
                (escape attributeSetName)

        retry {
            return
                getValues db getAttributesQuery (fun (reader: SqlDataReader) ->
                    let voa =
                        reader.GetStream 0
                        |> AttributeMethods.GetVectorOfAttributesFromSqlBinary

                    let date = reader.GetDateTime 1
                    voa, date)
                |> Seq.tryHead
        }

    let filesIncompleteForActionsQuery (actions: string list) =
        filesForActionsQuery [ "P"; "R"; "F" ] actions

    interface IDocumentSource with
        member _.getFilesCompleteForActions() =
            getValues db (filesCompleteForActionsQuery db.VerifyActions) (fun row -> row.GetString 0)
            |> ResizeArray

        member _.getFirstFilesCompleteForActions(limit) =
            let query = firstFilesForActionsQuery [ "C" ] db.VerifyActions limit

            getValues db query (fun row -> row.GetString 0)
            |> ResizeArray

    interface IAttributeSource with
        member _.getFndAttributes(fileName: string) =
            getAttributes db db.FndAttributeSet fileName

        member _.getExpAttributes(fileName: string) =
            getAttributes db db.ExpAttributeSet fileName
