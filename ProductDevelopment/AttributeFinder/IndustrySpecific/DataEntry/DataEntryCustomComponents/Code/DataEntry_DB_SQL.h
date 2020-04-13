// DataEntry_DB_SQL.h - Constants for DB SQL queries that are DataEntry Specific

#pragma once

#include <string>

using namespace std;

//--------------------------------------------------------------------------------------------------
// DataEntryData
//--------------------------------------------------------------------------------------------------

// DataEntry data table name
static const string gstrDATA_ENTRY_DATA = "DataEntryData";

// Create Table SQL statements
static const string gstrCREATE_DATAENTRY_DATA_V1 = 
	"CREATE TABLE [dbo].[DataEntryData]( "
	" [ID] [int] IDENTITY(1,1) NOT NULL "
	" CONSTRAINT [PK_DataEntryData] PRIMARY KEY CLUSTERED, "
	" [FileID] [int] NULL, "
	" [UserID] [int] NULL, "
	" [ActionID] [int] NULL, "
	" [MachineID] [int] NULL, "
	" [DateTimeStamp] [datetime] NULL, "
	" [Duration] [float] NULL, "
	" [OverheadTime] [float] NULL)";

// Query to add DataEntryData - FAMFile foreign key
static const string gstrADD_FK_DATAENTRY_FAMFILE_V1 =
	"ALTER TABLE [DataEntryData]  "
	" WITH CHECK ADD CONSTRAINT [FK_DataEntryData_FAMFile] FOREIGN KEY([FileID]) "
	" REFERENCES [FAMFile] ([ID]) "
	" ON UPDATE CASCADE "
	" ON DELETE CASCADE";

// Query to add DataEntryData - FAMUser foreign key
static const string gstrADD_FK_DATAENTRYDATA_FAMUSER_V1 = 
	"ALTER TABLE [dbo].[DataEntryData]  "
	" WITH CHECK ADD CONSTRAINT [FK_DataEntryData_FAMUser] FOREIGN KEY([UserID]) "
	" REFERENCES [dbo].[FAMUser] ([ID])";

// Query to add DataEntryData - Action foreign key
static const string gstrADD_FK_DATAENTRYDATA_ACTION_V1 = 
	"ALTER TABLE [dbo].[DataEntryData]  "
	" WITH CHECK ADD CONSTRAINT [FK_DataEntryData_Action] FOREIGN KEY([ActionID]) "
	" REFERENCES [dbo].[Action] ([ID])"
	" ON UPDATE CASCADE "
	" ON DELETE CASCADE";

// Query to add DataEntryData - Machine foreign key
static const string gstrADD_FK_DATAENTRYDATA_MACHINE_V1 =
	"ALTER TABLE [dbo].[DataEntryData]  "
	" WITH CHECK ADD CONSTRAINT [FK_DataEntryData_Machine] FOREIGN KEY([MachineID])"
	" REFERENCES [dbo].[Machine] ([ID])";

// Query to add index of FileID and DateTimeStamp fields
static const string gstrCREATE_FILEID_DATETIMESTAMP_INDEX_V1 = 
	"CREATE NONCLUSTERED INDEX [IX_FileID_DateTimeStamp] ON [dbo].[DataEntryData] "
	"( [FileID] ASC, [DateTimeStamp] ASC )";

//--------------------------------------------------------------------------------------------------
// DataEntryCounterDefinition
//--------------------------------------------------------------------------------------------------

// DataEntry counter definition table name
static const string gstrDATAENTRY_DATA_COUNTER_DEFINITION = "DataEntryCounterDefinition";

// Create Table SQL statements
static const string gstrCREATE_DATAENTRY_COUNTER_DEFINITION = 
	"CREATE TABLE [dbo].[DataEntryCounterDefinition]( "
	" [ID] INT IDENTITY(1,1) NOT NULL "
	" CONSTRAINT [PK_DataEntryCounterDefinition] PRIMARY KEY CLUSTERED, "
	" [Name] NVARCHAR(50) NOT NULL, "
	" [AttributeQuery] NVARCHAR(255) NOT NULL, "
	" [RecordOnLoad] BIT NOT NULL, "
	" [RecordOnSave] BIT NOT NULL)";

//--------------------------------------------------------------------------------------------------
// DataEntryCounterType
//--------------------------------------------------------------------------------------------------

// DataEntry counter type table name
static const string gstrDATAENTRY_DATA_COUNTER_TYPE = "DataEntryCounterType";

// Create Table SQL statements
static const string gstrCREATE_DATAENTRY_COUNTER_TYPE = 
	"CREATE TABLE [dbo].[DataEntryCounterType]( "
	" [Type] NVARCHAR(1) NOT NULL "
	" CONSTRAINT [PK_DataEntryCounterType] PRIMARY KEY CLUSTERED, "
	" [Description] NVARCHAR(255) NOT NULL)";

// Populate the DataEntryCounterType table.
static const string gstrPOPULATE_DATAENTRY_COUNTER_TYPES = 
	"INSERT INTO [dbo].[DataEntryCounterType] VALUES "
	"('L', 'OnLoad'); "
	"INSERT INTO [dbo].[DataEntryCounterType] VALUES "
	"('S', 'OnSave')";

//--------------------------------------------------------------------------------------------------
// DataEntryCounterValue
//--------------------------------------------------------------------------------------------------

// DataEntry counter value table name
static const string gstrDATAENTRY_DATA_COUNTER_VALUE = "DataEntryCounterValue";

// Create Table SQL statements
static const string gstrCREATE_DATAENTRY_COUNTER_VALUE = 
	"CREATE TABLE [dbo].[DataEntryCounterValue]( "
	" [InstanceID] INT NOT NULL, "
	" [CounterID] INT NOT NULL, "
	" [Type] NVARCHAR(1) NOT NULL, "
	" [Value] INT NULL)";

// Query to add DataEntryCounterValue - Instance foreign key
static const string gstrADD_FK_DATAENTRY_COUNTER_VALUE_INSTANCE_V1 = 
	"ALTER TABLE [dbo].[DataEntryCounterValue]  "
	" WITH CHECK ADD CONSTRAINT [FK_DataEntryCounterValue_Instance] FOREIGN KEY([InstanceID]) "
	" REFERENCES [dbo].[DataEntryData] ([ID])"
	" ON UPDATE CASCADE "
	" ON DELETE CASCADE";

static const string gstrADD_FK_DATAENTRY_COUNTER_VALUE_INSTANCE_V4 = 
	"ALTER TABLE [dbo].[DataEntryCounterValue]  "
	" WITH CHECK ADD CONSTRAINT [FK_DataEntryCounterValue_Instance] FOREIGN KEY([InstanceID]) "
	" REFERENCES [dbo].[FileTaskSession] ([ID])"
	" ON UPDATE CASCADE "
	" ON DELETE CASCADE";

// Query to add DataEntryCounterValue - ID foreign key
static const string gstrADD_FK_DATAENTRY_COUNTER_VALUE_ID = 
	"ALTER TABLE [dbo].[DataEntryCounterValue]  "
	" WITH CHECK ADD CONSTRAINT [FK_DataEntryCounterValue_ID] FOREIGN KEY([CounterID]) "
	" REFERENCES [dbo].[DataEntryCounterDefinition] ([ID])";
// Updates/deletes will already be cascaded from DataEntryData table

// Query to add DataEntryCounterValue - Type foreign key
static const string gstrADD_FK_DATAENTRY_COUNTER_VALUE_TYPE = 
	"ALTER TABLE [dbo].[DataEntryCounterValue]  "
	" WITH CHECK ADD CONSTRAINT [FK_DataEntryCounterValue_Type] FOREIGN KEY([Type]) "
	" REFERENCES [dbo].[DataEntryCounterType] ([Type])"
	" ON UPDATE CASCADE "
	" ON DELETE CASCADE";

// Query to record counts into the DataEntryCounterValue table. Requires the completion of the
// VALUES clause.
static const string gstrINSERT_DATAENTRY_COUNTER_VALUE =
	"INSERT INTO [dbo].[DataEntryCounterValue] VALUES"; 

// https://extract.atlassian.net/browse/ISSUE-13226
// Moves all data in DataEntryData to FileTaskSession and updates all foreign keys as necessary.
// The query will attempt to match rows to existing FAMSession rows, though if not successful, new
// "dummy" FAMSession rows will be created to correspond with the DataEntryData rows.
static const string gstrPORT_DATAENTRYDATA_TO_FILETASKSESSION =
"DECLARE @TaskClassID AS INT; \r\n"
"DECLARE @DataEntryDataID AS INT; \r\n"
"DECLARE @FileID AS INT; \r\n"
"DECLARE @UserID AS INT; \r\n"
"DECLARE @ActionID AS INT; \r\n"
"DECLARE @FAMSessionActionID AS INT; \r\n"
"DECLARE @MachineID AS INT; \r\n"
"DECLARE @DateTimeStamp AS DATETIME; \r\n"
"DECLARE @Duration AS FLOAT; \r\n"
"DECLARE @OverheadTime AS FLOAT; \r\n"
"DECLARE @FAMSessionID AS INT; \r\n"
"DECLARE @InstanceID AS INT; \r\n"
"DECLARE @NewCounterValueTable AS TABLE \r\n"
"		([InstanceID] INT, [CounterID] INT, Type NVARCHAR(1), Value INT); \r\n"
" \r\n"
"INSERT INTO [TaskClass] ([GUID], [Name]) VALUES \r\n"
"	('59496DF7-3951-49b7-B063-8C28F4CD843F', 'Data Entry: Verify extracted data') \r\n"
"SELECT @TaskClassID = SCOPE_IDENTITY() \r\n"
" \r\n"
"BEGIN TRY \r\n"
//	-- Iterate all DataEntryData rows
"	DECLARE [DataEntryData_Cursor] CURSOR FOR \r\n"
"	SELECT [ID], [FileID], [UserID], [ActionID], [MachineID], [DateTimeStamp], [Duration], [OverheadTime] \r\n"
"		FROM [DataEntryData] \r\n"
"	OPEN [DataEntryData_Cursor]; \r\n"
"	FETCH NEXT FROM [DataEntryData_Cursor] INTO \r\n"
"		@DataEntryDataID, @FileID, @UserID, @ActionID, @MachineID, @DateTimeStamp, @Duration, @OverheadTime \r\n"
"	WHILE @@FETCH_STATUS = 0 \r\n"
"	BEGIN \r\n"
"		SET @FAMSessionID = NULL \r\n"
"	 \r\n"
//		-- Attempt to find an existing FAM Session that:
//		-- 1) Existed prior to the upgrade (UPI is not null)
//		-- 2) Is a match for the user, machine and date time stamp.
//		-- 3) Is the one and only matching row. If multiple possible sessions are found,
//		-- we won't attempt to pick one; a dummy FAM session will be created instead.
//		-- ADDED a "AND (ActionID IS NULL OR ActionID = @ActionID)" to where clause
//		-- to fix https://extract.atlassian.net/browse/ISSUE-14121
"		SELECT @FAMSessionID = MAX([ID]) \r\n"
"			FROM [FAMSession] \r\n"
"			WHERE [FAMUserID] = @UserID AND [MachineID] = @MachineID \r\n"
"				AND ([UPI] IS NOT NULL) \r\n"
"				AND @DateTimeStamp BETWEEN [StartTime] AND [StopTime] \r\n"
"				AND (ActionID IS NULL OR ActionID = @ActionID) \r\n"
"			GROUP BY [FAMUserID] \r\n"
"			HAVING COUNT(*) = 1 \r\n"
" \r\n"
"		IF @FAMSessionID IS NULL \r\n"
"		BEGIN \r\n"
//			-- If a previously existing FAMSession was not found, see if there is a matching
//			-- "dummy" session we've added as part of the upgrade (UPI is null)
"			SELECT @FAMSessionID = [ID] \r\n"
"				FROM [FAMSession] \r\n"
"				WHERE [FAMUserID] = @UserID AND [MachineID] = @MachineID AND @ActionID = [ActionID] \r\n"
"					AND ([UPI] IS NULL) \r\n"
"					AND @DateTimeStamp BETWEEN [StartTime] AND [StopTime] \r\n"
" \r\n"
"			IF @FAMSessionID IS NULL \r\n"
"			BEGIN \r\n"
//				-- We need to add a new "Dummy" FAM session for the current DataEntryData row.
//				-- The fact that this is a dummy row will be indicated by UPI being NULL.
"				INSERT INTO [FAMSession]  \r\n"
"					([MachineID], [FAMUserID], [StartTime], [StopTime], [ActionID]) \r\n"
"					VALUES \r\n"
"					(@MachineID, @UserID, \r\n"
"						CAST(FLOOR(CAST(@DateTimeStamp AS DECIMAL(12, 5))) AS DATETIME), \r\n"
"						DATEADD(d, 1, CAST(FLOOR(CAST(@DateTimeStamp AS DECIMAL(12, 5))) AS DATETIME)), \r\n"
"						@ActionID) \r\n"
"				SELECT @FAMSessionID = SCOPE_IDENTITY() \r\n"
"			END \r\n"
"		END \r\n"
"		ELSE \r\n"
"		BEGIN \r\n"
"			SELECT @FAMSessionActionID = [ActionID]  \r\n"
"				FROM [FAMSession] \r\n"
"				WHERE [ID] = @FAMSessionID \r\n"
" \r\n"
//			-- If the matching FAMSession row does not have an action ID set, set it to the
//			-- action ID of the DataEntryData row.
"			IF @FAMSessionActionID IS NULL \r\n"
"			BEGIN \r\n"
"				UPDATE [FAMSession] SET [ActionID] = @ActionID WHERE [ID] = @FAMSessionID \r\n"
"			END \r\n"
"			ELSE IF @FAMSessionActionID <> @ActionID \r\n"
//			-- If the matching FAMSession row does have an action ID set, it should match
//			-- the Action ID assigned by a previous DataEntryData row. If it does not,
//			-- something unexpected has happened in this process and it should be aborted.
"			BEGIN \r\n"
"				RAISERROR ('Unexpected error matching DataEntryData to FAMSession', 15, 1) \r\n"
"			END \r\n"
"		END \r\n"
" \r\n"			
"		INSERT INTO [FileTaskSession]  \r\n"
"			([FAMSessionID], [TaskClassID], [FileID], [DateTimeStamp], [Duration], [OverheadTime]) \r\n"
"			VALUES \r\n"
"			(@FAMSessionID, 1, @FileID, @DateTimeStamp, @Duration, @OverheadTime) \r\n"
"		SELECT @InstanceID = SCOPE_IDENTITY() \r\n"
" \r\n"
//		-- The InstanceIDs of DataEntryCounterValue will need to be updated to reflect the row \r\n"
//		-- added to the FileTaskSession table. However, the InstanceIDs cannot be updated while \r\n"
//		-- looping because the new instance ID may be the same as an existing Instance ID in \r\n"
//		-- DataEntryData that hasn't been updated yet. Store the new mappings in a table variable \r\n"
//		-- for now. \r\n"
"		INSERT INTO @NewCounterValueTable ([InstanceID], [CounterID], [Type], [Value]) \r\n"
"			SELECT @InstanceID, [CounterID], [Type], [Value] \r\n"
"				FROM [DataEntryCounterValue] \r\n"
"				WHERE [InstanceID] = @DataEntryDataID \r\n"
" \r\n"
"		FETCH NEXT FROM [DataEntryData_Cursor] INTO \r\n"
"			@DataEntryDataID, @FileID, @UserID, @ActionID, @MachineID, @DateTimeStamp, @Duration, @OverheadTime \r\n"
"	END \r\n"
" \r\n"
//	-- Replace the data in [DataEntryCounterValue] with the new InstanceID mappings. \r\n"
"	DELETE FROM [DataEntryCounterValue] \r\n"
"	INSERT INTO [DataEntryCounterValue] ([InstanceID], [CounterID], [Type], [Value]) \r\n"
"		SELECT [InstanceID], [CounterID], [Type], [Value] \r\n"
"			FROM @NewCounterValueTable \r\n"
" \r\n"
"	CLOSE [DataEntryData_Cursor]; \r\n"
"	DEALLOCATE [DataEntryData_Cursor]; \r\n"
"END TRY \r\n"
"BEGIN CATCH \r\n"
//	-- Without try catch, RAISERROR above doesn't seem to prevent execution from continuing
"	CLOSE [DataEntryData_Cursor]; \r\n"
"	DEALLOCATE [DataEntryData_Cursor]; \r\n"
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
"END CATCH \r\n";

static const string gstrINSERT_DATA_ENTRY_VERIFY_TASK_CLASS =
	"INSERT INTO [TaskClass] ([GUID], [Name]) VALUES \r\n"
	"	('59496DF7-3951-49b7-B063-8C28F4CD843F', 'Data Entry: Verify extracted data') \r\n";

static const string gstrCREATE_DATA_ENTRY_COUNTER_VALUE_INSTANCEID_TYPE_INDEX = 
	"CREATE NONCLUSTERED INDEX [IX_DataEntryCounterValue_InstanceID_Type] ON [dbo].[DataEntryCounterValue] "
	"( [InstanceID] ASC, [Type] ASC	)";