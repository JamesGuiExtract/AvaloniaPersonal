// IDShield_DB_SQL.h - Constants for DB SQL queries that are IDShield Specific

#pragma once

#include <string>

using namespace std;

// IDShield data table name
static const string gstrIDSHIELD_DATA = "IDShieldData";

// Create Table SQL statements
static const string gstrCREATE_IDSHIELD_DATA_V5 = 
	"CREATE TABLE [dbo].[IDShieldData]( "
	" [ID] [int] IDENTITY(1,1) NOT NULL "
	" CONSTRAINT [PK_IDShieldData] PRIMARY KEY CLUSTERED, "
	" [NumHCDataFound] [int] NULL, "
	" [NumMCDataFound] [int] NULL, "
	" [NumLCDataFound] [int] NULL, "
	" [NumCluesFound] [int] NULL, "
	" [TotalManualRedactions] [int] NULL, "
	" [TotalRedactions] [int] NULL, "
	" [NumPagesAutoAdvanced] [int] NULL, "
	" [FileTaskSessionID] [int] NOT NULL)";

static const string gstrADD_IDSHIELDDATA_FILETASKSESSION_FK =
	"ALTER TABLE [dbo].[IDShieldData] "
	"WITH CHECK ADD CONSTRAINT [FK_IDShieldData_FileTaskSession] FOREIGN KEY([FileTaskSessionID])"
	"REFERENCES [dbo].[FileTaskSession] ([ID]) "
	" ON UPDATE CASCADE "
	" ON DELETE CASCADE";

static const string gstrCREATE_IDSHIELDDATA_FILETASKSESSION_INDEX = 
	"CREATE NONCLUSTERED INDEX [IX_FileID_FileTaskSession] ON [dbo].[IDShieldData] "
	"([FileTaskSessionID] ASC)";

static const string gstrINSERT_FILETASKSESSION_DATA_RCD =
	"INSERT INTO [dbo].[FileTaskSession] "
	" ([FAMSessionID]"
	"  ,[TaskClassID]"
	"  ,[FileID]"
    "  ,[DateTimeStamp]"
    "  ,[Duration]"
	"  ,[OverheadTime]) "
	"  OUTPUT INSERTED.ID "
	"  VALUES (<FAMSessionID>, <TaskClassID>, <FileID>, GETDATE(), <Duration>, <OverheadTime>)";

static const string gstrINSERT_IDSHIELD_DATA_RCD = 
	"INSERT INTO [dbo].[IDShieldData] "
	" ([FileTaskSessionID]"
    "  ,[NumHCDataFound]"
    "  ,[NumMCDataFound]"
    "  ,[NumLCDataFound]"
    "  ,[NumCluesFound]"
    "  ,[TotalRedactions]"
    "  ,[TotalManualRedactions]"
	"  ,[NumPagesAutoAdvanced]) "
	"  VALUES (<FileTaskSessionID>, <NumHCDataFound>, <NumMCDataFound>, <NumLCDataFound>, "
	"	<NumCluesFound>, <TotalRedactions>, <TotalManualRedactions>, <NumPagesAutoAdvanced>)";

// Deletes all records in the FileTaskSession table that have the given TaskClassID and FileID. This
// should be run before adding a new record if history is not being kept.
// Requires <TaskClassID> and <FileID> be replaced with the appropriate values.
// (Cascade deletes will delete the corresponding row in IDShieldData)
static const string gstrDELETE_PREVIOUS_STATUS_FOR_FILEID = 
	"DELETE FROM [FileTaskSession]"
	"WHERE [TaskClassID] = <TaskClassID> AND [FileID] = <FileID>";

// https://extract.atlassian.net/browse/ISSUE-13226
// Moves all data in IDShieldData to FileTaskSession and updates all foreign keys as necessary.
// The query will attempt to match rows to existing FAMSession rows, though if not successful, new
// "dummy" FAMSession rows will be created to correspond with the IDShieldData rows.
static const string gstrPORT_IDSHEIELDDATA_TO_FILETASKSESSION =
"DECLARE @VerifyTaskClassID AS INT; \r\n"
"DECLARE @RedactTaskClassID AS INT; \r\n"
"DECLARE @TaskClassID AS INT; \r\n"
"DECLARE @FileID AS INT; \r\n"
"DECLARE @Verified AS BIT; \r\n"
"DECLARE @UserID AS INT; \r\n"
"DECLARE @MachineID AS INT; \r\n"
"DECLARE @DateTimeStamp AS DATETIME; \r\n"
"DECLARE @Duration AS FLOAT; \r\n"
"DECLARE @OverheadTime AS FLOAT; \r\n"
"DECLARE @FAMSessionID AS INT; \r\n"
"DECLARE @FileTaskSessionID AS INT; \r\n"
" \r\n"
"INSERT INTO [TaskClass] ([GUID], [Name]) VALUES \r\n"
"	('AD7F3F3F-20EC-4830-B014-EC118F6D4567', 'Redaction: Verify sensitive data') \r\n"
"SELECT @VerifyTaskClassID = SCOPE_IDENTITY() \r\n"
" \r\n"
"INSERT INTO [TaskClass] ([GUID], [Name]) VALUES \r\n"
"	('36D14C41-CE3D-4950-AC47-2664563340B1', 'Redaction: Create redacted image') \r\n"
"SELECT @RedactTaskClassID = SCOPE_IDENTITY() \r\n"
" \r\n"
"BEGIN TRY \r\n"
//	-- Iterate all IDShieldData rows 
"	DECLARE [IDShieldData_Cursor] CURSOR FOR \r\n"
"	SELECT [FileID], [Verified], [UserID], [MachineID], [DateTimeStamp], [Duration], [OverheadTime] \r\n"
"		FROM [IDShieldData] FOR UPDATE OF [FileTaskSessionID]\r\n"
"	OPEN [IDShieldData_Cursor]; \r\n"
"	FETCH NEXT FROM [IDShieldData_Cursor] INTO \r\n"
"		@FileID, @Verified, @UserID, @MachineID, @DateTimeStamp, @Duration, @OverheadTime \r\n"
"	WHILE @@FETCH_STATUS = 0 \r\n"
"	BEGIN \r\n"
"		SET @FAMSessionID = NULL \r\n"
" \r\n"
//		-- Attempt to find an existing FAM Session that:
//		-- 1) Existed prior to the upgrade (UPI is not null)
//		-- 2) Is a match for the user, machine and date time stamp.
//		-- 3) Corresponds to the @Verified value for the current row.
//		-- 4) Is the one and only matching row. If multiple possible sessions are found,
//		-- we won't attempt to pick one; a dummy FAM session will be created instead.
"		SELECT @FAMSessionID = MAX([ID]) \r\n"
"			FROM [FAMSession] \r\n"
"			WHERE [FAMUserID] = @UserID AND [MachineID] = @MachineID \r\n"
"				AND ([UPI] IS NOT NULL) \r\n"
"				AND @DateTimeStamp BETWEEN [StartTime] AND [StopTime] \r\n"
"			GROUP BY [FAMUserID] \r\n"
"			HAVING COUNT(*) = 1 \r\n"
" \r\n"
"		IF @FAMSessionID IS NULL \r\n"
"		BEGIN \r\n"
//			-- If a previously existing FAMSession was not found, see if there is a matching
//			-- "dummy" session we've added as part of the upgrade (UPI is null)
"			SELECT @FAMSessionID = [ID] \r\n"
"				FROM [FAMSession] \r\n"
"				WHERE [FAMUserID] = @UserID AND [MachineID] = @MachineID \r\n"
"					AND ([UPI] IS NULL) \r\n"
"					AND @DateTimeStamp BETWEEN [StartTime] AND [StopTime] \r\n"
" \r\n"
"			IF @FAMSessionID IS NULL \r\n"
"			BEGIN \r\n"
//				-- We need to add a new "Dummy" FAM session for the current IDShieldData row.
//				-- The fact that this is a dummy row will be indicated by UPI being NULL.
"				INSERT INTO [FAMSession]  \r\n"
"					([MachineID], [FAMUserID], [StartTime], [StopTime]) \r\n"
"					VALUES \r\n"
"					(@MachineID, @UserID, \r\n"
"						CAST(FLOOR(CAST(@DateTimeStamp AS DECIMAL(12, 5))) AS DATETIME), \r\n"
"						DATEADD(d, 1, CAST(FLOOR(CAST(@DateTimeStamp AS DECIMAL(12, 5))) AS DATETIME))) \r\n"
"				SELECT @FAMSessionID = SCOPE_IDENTITY() \r\n"
"			END \r\n"
"		END \r\n"
" \r\n"
//		-- Set the TaskClassID according to whether the IDShieldData row was for verification \r\n"
"		IF @Verified = 1 \r\n"
"			SET @TaskClassID = @VerifyTaskClassID \r\n"
"		ELSE \r\n"
"			SET @TaskClassID = @RedactTaskClassID \r\n"
" \r\n"
//		-- Add a FileTaskSession row to correspond to the current IDShieldDataRow and link to it.
"		INSERT INTO [FileTaskSession]  \r\n"
"			([FAMSessionID], [TaskClassID], [FileID], [DateTimeStamp], [Duration], [OverheadTime]) \r\n"
"			VALUES \r\n"
"			(@FAMSessionID, @TaskClassID, @FileID, @DateTimeStamp, @Duration, @OverheadTime) \r\n"
"		SELECT @FileTaskSessionID = SCOPE_IDENTITY() \r\n"
" \r\n"		
"		UPDATE [IDShieldData] SET [FileTaskSessionID] = @FileTaskSessionID \r\n"
"			WHERE CURRENT OF [IDShieldData_Cursor] \r\n"
" \r\n"
"		FETCH NEXT FROM [IDShieldData_Cursor] INTO \r\n"
"			@FileID, @Verified, @UserID, @MachineID, @DateTimeStamp, @Duration, @OverheadTime \r\n"
"	END \r\n"
" \r\n"
"	CLOSE [IDShieldData_Cursor]; \r\n"
"	DEALLOCATE [IDShieldData_Cursor]; \r\n"
"END TRY \r\n"
"BEGIN CATCH \r\n"
//	-- Without try catch, RAISERROR above doesn't seem to prevent execution from continuing
"	CLOSE [IDShieldData_Cursor]; \r\n"
"	DEALLOCATE [IDShieldData_Cursor]; \r\n"
" \r\n"
"	DECLARE @ErrorMessage NVARCHAR(4000); \r\n"
"    DECLARE @ErrorSeverity INT; \r\n"
"    DECLARE @ErrorState INT; \r\n"
" \r\n"
"    SELECT  \r\n"
"        @ErrorMessage = ERROR_MESSAGE(), \r\n"
"        @ErrorSeverity = ERROR_SEVERITY(), \r\n"
"        @ErrorState = ERROR_STATE(); \r\n"
" \r\n"
"    RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState) \r\n"
"END CATCH";