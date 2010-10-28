#pragma once

#include <FAMUtilsConstants.h>

#include <string>

using namespace std;

// Create Table SQL statements
static const string gstrCREATE_ACTION_TABLE = "CREATE TABLE [Action] ([ID] [int] IDENTITY(1,1) NOT NULL "
	"CONSTRAINT [PK_Action] PRIMARY KEY CLUSTERED, " 
	"[ASCName] [nvarchar](50) NOT NULL,	[Description] [nvarchar](255) NULL)";

static const string gstrCREATE_LOCK_TABLE = 
	"CREATE TABLE [LockTable]([LockID] [int] NOT NULL CONSTRAINT [PK_LockTable] PRIMARY KEY CLUSTERED,"
	"[UPI] [nvarchar](512), "
	"[LockTime] datetime NOT NULL CONSTRAINT [DF_LockTable_LockTime]  DEFAULT (GETDATE()))";

static const string gstrCREATE_DB_INFO_TABLE = 
	"CREATE TABLE [DBInfo]("
	"[Name] [nvarchar](50) NOT NULL PRIMARY KEY CLUSTERED, "
	"[Value] [nvarchar](max))";

static const string gstrCREATE_ACTION_STATE_TABLE = "CREATE TABLE [ActionState]([Code] [nvarchar](1) NOT NULL "
	"CONSTRAINT [PK_ActionState] PRIMARY KEY CLUSTERED,"
	"[Meaning] [nvarchar](255) NULL)";

static const string gstrCREATE_FAM_FILE_TABLE = "CREATE TABLE [FAMFile]("
	"[ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_File] PRIMARY KEY CLUSTERED,"
	"[FileName] [nvarchar](255) NULL,"
	"[FileSize] [bigint] NOT NULL CONSTRAINT [DF_FAMFile_FileSize]  DEFAULT ((0)),"
	"[Pages] [int] NOT NULL CONSTRAINT [DF_FAMFile_Pages]  DEFAULT ((0)),"
	"[Priority] [int] NOT NULL CONSTRAINT [DF_FAMFile_Priority] DEFAULT((3)))";

static const string gstrCREATE_QUEUE_EVENT_CODE_TABLE = "CREATE TABLE [QueueEventCode]("
	"[Code] [nvarchar](1) NOT NULL CONSTRAINT [PK_QueueEventCode] PRIMARY KEY CLUSTERED ,"
	"[Description] [nvarchar](255) NULL)";

static const string gstrCREATE_ACTION_STATISTICS_TABLE = "CREATE TABLE [ActionStatistics]("
	"[ActionID] [int] NOT NULL CONSTRAINT [PK_Statistics] PRIMARY KEY CLUSTERED,"
	"[LastUpdateTimeStamp] [datetime] NULL,"
	"[NumDocuments] [int] NOT NULL CONSTRAINT [DF_ActionStatistics_TotalDocuments]  DEFAULT ((0)),"
	"[NumDocumentsPending] [int] NOT NULL CONSTRAINT [DF_ActionStatistics_TotalDocumentsPending]  DEFAULT ((0)),"
	"[NumDocumentsComplete] [int] NOT NULL CONSTRAINT [DF_ActionStatistics_ProcessedDocuments]  DEFAULT ((0)),"
	"[NumDocumentsFailed] [int] NOT NULL CONSTRAINT [DF_ActionStatistics_NumDocumentsFailed]  DEFAULT ((0)),"
	"[NumDocumentsSkipped] [int] NOT NULL CONSTRAINT [DF_ActionStatistics_NumDocumentsSkipped] DEFAULT ((0)),"
	"[NumPages] [int] NOT NULL CONSTRAINT [DF_ActionStatistics_NumPages]  DEFAULT ((0)),"
	"[NumPagesPending] [int] NOT NULL CONSTRAINT [DF_ActionStatistics_NumPagesPending]  DEFAULT ((0)),"
	"[NumPagesComplete] [int] NOT NULL CONSTRAINT [DF_ActionStatistics_NumPagesComplete]  DEFAULT ((0)),"
	"[NumPagesFailed] [int] NOT NULL CONSTRAINT [DF_ActionStatistics_NumPagesFailed]  DEFAULT ((0)),"
	"[NumPagesSkipped] [int] NOT NULL CONSTRAINT [DF_ActionStatistics_NumPagesSkipped]  DEFAULT ((0)),"
	"[NumBytes] [bigint] NOT NULL CONSTRAINT [DF_ActionStatistics_NumBytes]  DEFAULT ((0)),"
	"[NumBytesPending] [bigint] NOT NULL CONSTRAINT [DF_ActionStatistics_NumBytesPending]  DEFAULT ((0)),"
	"[NumBytesComplete] [bigint] NOT NULL CONSTRAINT [DF_ActionStatistics_NumBytesComplete]  DEFAULT ((0)),"
	"[NumBytesFailed] [bigint] NOT NULL CONSTRAINT [DF_ActionStatistics_NumBytesFailed]  DEFAULT ((0)),"
	"[NumBytesSkipped] [bigint] NOT NULL CONSTRAINT [DF_ActionStatistics_NumBytesSkipped]  DEFAULT ((0)))";

static const string gstrCREATE_ACTION_STATISTICS_DELTA_TABLE = "CREATE TABLE [ActionStatisticsDelta]("
	"[ID] [bigint] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_ActionStatisticsDelta] PRIMARY KEY CLUSTERED,"
	"[ActionID] [int] NOT NULL,"
	"[NumDocuments] [int] NOT NULL CONSTRAINT [DF_ActionStatisticsDelta_TotalDocuments]  DEFAULT ((0)),"
	"[NumDocumentsPending] [int] NOT NULL CONSTRAINT [DF_ActionStatisticsDelta_TotalDocumentsPending]  DEFAULT ((0)),"
	"[NumDocumentsComplete] [int] NOT NULL CONSTRAINT [DF_ActionStatisticsDelta_ProcessedDocuments]  DEFAULT ((0)),"
	"[NumDocumentsFailed] [int] NOT NULL CONSTRAINT [DF_ActionStatisticsDelta_NumDocumentsFailed]  DEFAULT ((0)),"
	"[NumDocumentsSkipped] [int] NOT NULL CONSTRAINT [DF_ActionStatisticsDelta_NumDocumentsSkipped] DEFAULT ((0)),"
	"[NumPages] [int] NOT NULL CONSTRAINT [DF_ActionStatisticsDelta_NumPages]  DEFAULT ((0)),"
	"[NumPagesPending] [int] NOT NULL CONSTRAINT [DF_ActionStatisticsDelta_NumPagesPending]  DEFAULT ((0)),"
	"[NumPagesComplete] [int] NOT NULL CONSTRAINT [DF_ActionStatisticsDelta_NumPagesComplete]  DEFAULT ((0)),"
	"[NumPagesFailed] [int] NOT NULL CONSTRAINT [DF_ActionStatisticsDelta_NumPagesFailed]  DEFAULT ((0)),"
	"[NumPagesSkipped] [int] NOT NULL CONSTRAINT [DF_ActionStatisticsDelta_NumPagesSkipped]  DEFAULT ((0)),"
	"[NumBytes] [bigint] NOT NULL CONSTRAINT [DF_ActionStatisticsDelta_NumBytes]  DEFAULT ((0)),"
	"[NumBytesPending] [bigint] NOT NULL CONSTRAINT [DF_ActionStatisticsDelta_NumBytesPending]  DEFAULT ((0)),"
	"[NumBytesComplete] [bigint] NOT NULL CONSTRAINT [DF_ActionStatisticsDelta_NumBytesComplete]  DEFAULT ((0)),"
	"[NumBytesFailed] [bigint] NOT NULL CONSTRAINT [DF_ActionStatisticsDelta_NumBytesFailed]  DEFAULT ((0)),"
	"[NumBytesSkipped] [bigint] NOT NULL CONSTRAINT [DF_ActionStatisticsDelta_NumBytesSkipped]  DEFAULT ((0)))";

static const string gstrCREATE_FILE_ACTION_STATE_TRANSITION_TABLE  ="CREATE TABLE [FileActionStateTransition]("
	"[ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_FileActionStateTransition] PRIMARY KEY CLUSTERED,"
	"[FileID] [int] NULL,"
	"[ActionID] [int] NULL,"
	"[ASC_From] [nvarchar](1) NULL,"
	"[ASC_To] [nvarchar](1) NULL,"
	"[DateTimeStamp] [datetime] NULL,"
	"[MachineID] [int] NULL, "
	"[FAMUserID] [int] NULL, "
	"[Exception] [ntext] NULL,"
	"[Comment] [nvarchar](50) NULL)";

static const string gstrCREATE_QUEUE_EVENT_TABLE = "CREATE TABLE [QueueEvent]("
	"[ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_QueueEvent] PRIMARY KEY CLUSTERED,"
	"[FileID] [int] NULL,"
	"[ActionID] [int] NULL,"
	"[DateTimeStamp] [datetime] NULL,"
	"[QueueEventCode] [nvarchar](1) NULL,"
	"[FileModifyTime] [datetime] NULL,"
	"[FileSizeInBytes] [bigint] NULL,"
	"[MachineID] [int] NULL, "
	"[FAMUserID] [int] NULL)";

static const string gstrCREATE_LOGIN_TABLE = "CREATE TABLE [Login]("
	"[ID] [int] IDENTITY(1,1) NOT NULL, "
	"[UserName] [nvarchar](50) NOT NULL, "
	"[Password] [nvarchar](128) NOT NULL DEFAULT(''), "
	"CONSTRAINT [PK_LoginID] PRIMARY KEY CLUSTERED ( [ID] ASC ))";

static const string gstrCREATE_MACHINE_TABLE = "CREATE TABLE [Machine]("
	"[ID] [int] IDENTITY(1,1) NOT NULL, "
	"[MachineName] [nvarchar](50) NULL, "
	"CONSTRAINT [PK_Machine] PRIMARY KEY CLUSTERED ([ID] ASC), "
	"CONSTRAINT [IX_MachineName] UNIQUE NONCLUSTERED ([MachineName]))";

static const string gstrCREATE_FAM_USER_TABLE = "CREATE TABLE [FAMUser]("
	"[ID] [int] IDENTITY(1,1) NOT NULL, "
	"[UserName] [nvarchar](50) NULL, "
	"CONSTRAINT [PK_FAMUser] PRIMARY KEY CLUSTERED ([ID] ASC), "
	"CONSTRAINT [IX_UserName] UNIQUE NONCLUSTERED ([UserName] ASC))";

static const string gstrCREATE_FAM_FILE_ACTION_COMMENT_TABLE = "CREATE TABLE [FileActionComment] ("
	"[ID] [int] IDENTITY(1,1) NOT NULL, "
	"[UserName] [nvarchar](50) NULL, "
	"[FileID] [int] NULL, "
	"[ActionID] [int] NULL, "
	"[Comment] [ntext] NULL, "
	"[DateTimeStamp] [datetime] NOT NULL CONSTRAINT [DF_FileActionComment_DateTimeStamp] DEFAULT((GETDATE())), "
	"CONSTRAINT [PK_FAMFileActionComment] PRIMARY KEY CLUSTERED ([ID] ASC))";

static const string gstrCREATE_FAM_SKIPPED_FILE_TABLE = "CREATE TABLE [SkippedFile] ("
	"[ID] [int] IDENTITY(1,1) NOT NULL, "
	"[UserName] [nvarchar](50) NULL, "
	"[FileID] [int] NULL, "
	"[ActionID] [int] NULL, "
	"[DateTimeStamp] [datetime] NOT NULL CONSTRAINT [DF_SkippedFile_DateTimeStamp] DEFAULT((GETDATE())), "
	"[TimeSinceSkipped] AS (DATEDIFF(second,[DateTimeStamp],GETDATE())), " // Computed column for time skipped
	"[UPIID] [int] NOT NULL DEFAULT(0), "
	"CONSTRAINT [PK_FAMSkippedFile] PRIMARY KEY CLUSTERED ([ID] ASC))";

static const string gstrCREATE_FAM_TAG_TABLE = "CREATE TABLE [Tag] ("
	"[ID] [int] IDENTITY(1,1) NOT NULL, "
	"[TagName] [nvarchar](100) NOT NULL, "
	"[TagDescription] [nvarchar](255) NULL, "
	"CONSTRAINT [PK_FAMTag] PRIMARY KEY CLUSTERED ([ID] ASC), "
	"CONSTRAINT [IX_TagName] UNIQUE NONCLUSTERED ([TagName] ASC))";

static const string gstrCREATE_FAM_FILE_TAG_TABLE = "CREATE TABLE [FileTag] ("
	"[FileID] [int] NOT NULL, "
	"[TagID] [int] NOT NULL)";

static const string gstrCREATE_PROCESSING_FAM_TABLE = 
	"CREATE TABLE [ProcessingFAM]([ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_ProcessingFAM] PRIMARY KEY CLUSTERED, "
	"[ActionID] [int] NOT NULL, "
	"[UPI] [nvarchar](450), "
	"[LastPingTime] datetime NOT NULL CONSTRAINT [DF_ProcessingFAM_LastPingTime]  DEFAULT (GETDATE()))";

static const string gstrCREATE_LOCKED_FILE_TABLE = 
	"CREATE TABLE [LockedFile]([FileID] [int] NOT NULL,"
	"[ActionID] [int] NOT NULL, "
	"[UPIID] [int] , "
	"[StatusBeforeLock] [nvarchar](1) NOT NULL, "
	"CONSTRAINT [PK_LockedFile] PRIMARY KEY CLUSTERED ([FileID], [ActionID], [UPIID]))";

static const string gstrCREATE_USER_CREATED_COUNTER_TABLE =
	"CREATE TABLE [UserCreatedCounter] ("
	"[CounterName] [nvarchar](50) NOT NULL CONSTRAINT [PK_UserCreatedConter] PRIMARY KEY CLUSTERED,"
	"[Value] [bigint] NOT NULL CONSTRAINT [DF_UserCreatedCounter_Value] DEFAULT((0)))";

static const string gstrCREATE_FPS_FILE_TABLE =
	"CREATE TABLE [FPSFile] ("
	"[ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_FPSFile] PRIMARY KEY CLUSTERED, "
	"[FPSFileName] [nvarchar](512) NOT NULL)";

static const string gstrCREATE_FAM_SESSION =
	"CREATE TABLE [FAMSession] ("
	"[ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_FAMSession] PRIMARY KEY CLUSTERED, "
	"[MachineID] int NOT NULL, "
	"[FAMUserID] int NOT NULL, "
	"[UPI] [nvarchar](450), "
	"[StartTime] datetime NOT NULL CONSTRAINT [DF_FAMSession_StartTime] DEFAULT((GETDATE())), "
	"[StopTime] datetime, "
	"[FPSFileID] int NOT NULL)";

static const string gstrCREATE_INPUT_EVENT =
	"CREATE TABLE [InputEvent] ("
	"[ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_InputEvent] PRIMARY KEY CLUSTERED, "
	"[TimeStamp] [DateTime] NOT NULL, "
	"[ActionID] int NOT NULL, "
	"[FAMUserID] int NOT NULL, "
	"[MachineID] int NOT NULL, "
	"[PID] int NOT NULL, "
	"[SecondsWithInputEvents] int NOT NULL)";

static const string gstrCREATE_FILE_ACTION_STATUS = 
	"CREATE TABLE [FileActionStatus]( "
	"[ActionID] [int] NOT NULL, "
	"[FileID] [int] NOT NULL, "
	"[ActionStatus] [nvarchar](1) NOT NULL, "
	"CONSTRAINT [PK_FileActionStatus] PRIMARY KEY CLUSTERED "
	"( "
	"	[ActionID] ASC, "
	"	[FileID] ASC "
	")) ";

// Create table indexes SQL
static const string gstrCREATE_FAM_FILE_ID_PRIORITY_INDEX = "CREATE UNIQUE NONCLUSTERED INDEX [IX_Files_PriorityID] "
	"ON [FAMFile]([Priority] DESC, [ID] ASC)";

static const string gstrCREATE_FAM_FILE_INDEX = "CREATE UNIQUE NONCLUSTERED INDEX [IX_Files_FileName] "
	"ON [FAMFile]([FileName] ASC)";

static const string gstrCREATE_QUEUE_EVENT_INDEX = "CREATE NONCLUSTERED INDEX [IX_FileID] "
	"ON [QueueEvent]([FileID])";

static const string gstrCREATE_FILE_ACTION_COMMENT_INDEX = "CREATE UNIQUE NONCLUSTERED INDEX "
	"[IX_File_Action_Comment] ON [FileActionComment]([FileID], [ActionID])";

static const string gstrCREATE_SKIPPED_FILE_INDEX = "CREATE UNIQUE NONCLUSTERED INDEX "
	"[IX_Skipped_File] ON [SkippedFile]([FileID], [ActionID])";

static const string gstrCREATE_SKIPPED_FILE_UPI_INDEX = "CREATE NONCLUSTERED INDEX "
	"[IX_Skipped_File_UPI] ON [SkippedFile]([UPIID])";

static const string gstrCREATE_FILE_TAG_INDEX = "CREATE UNIQUE NONCLUSTERED INDEX "
	"[IX_File_Tag] ON [FileTag]([FileID], [TagID])";

static const string gstrCREATE_PROCESSING_FAM_UPI_INDEX = "CREATE UNIQUE NONCLUSTERED INDEX "
	"[IX_ProcessingFAM_UPI] ON [ProcessingFAM]([UPI])";

static const string gstrCREATE_USER_CREATED_COUNTER_VALUE_INDEX = "CREATE NONCLUSTERED INDEX "
	"[IX_UserCreatedCounter_Value] ON [UserCreatedCounter]([Value])";

static const string gstrCREATE_FPS_FILE_NAME_INDEX = "CREATE NONCLUSTERED INDEX "
	"[IX_FPSFile_FPSFileName] ON [FPSFile]([FPSFileName])";

static const string gstrCREATE_INPUT_EVENT_INDEX = "CREATE UNIQUE NONCLUSTERED INDEX "
	"[IX_Input_Event] ON [InputEvent]([TimeStamp], [ActionID], [MachineID], [FAMUserID], [PID])";

static const string gstrCREATE_FILE_ACTION_STATUS_ACTION_ACTIONSTATUS_INDEX = 
	"CREATE NONCLUSTERED INDEX "
	"[IX_FileActionStatus_ActionID_ActionStatus] ON [dbo].[FileActionStatus] "
	"([ActionID] ASC, [ActionStatus] ASC)";

static const string gstrCREATE_ACTION_STATISTICS_DELTA_ACTIONID_ID_INDEX =
	"CREATE UNIQUE NONCLUSTERED INDEX "
	"[IX_ActionStatisticsDeltaActionID_ID] ON [dbo].[ActionStatisticsDelta] "
	"([ActionID] ASC, [ID] ASC)";

	// Add foreign keys SQL
static const string gstrADD_STATISTICS_ACTION_FK = 
	"ALTER TABLE [ActionStatistics]  "
	"WITH CHECK ADD CONSTRAINT [FK_Statistics_Action] FOREIGN KEY([ActionID]) "
	"REFERENCES [Action] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_FILE_ACTION_STATE_TRANSITION_ACTION_FK = 
	"ALTER TABLE [FileActionStateTransition]  "
	"WITH CHECK ADD CONSTRAINT [FK_FileActionStateTransition_Action] FOREIGN KEY([ActionID]) "
	"REFERENCES [Action] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_FILE_ACTION_STATE_TRANSITION_FAM_FILE_FK = 
	"ALTER TABLE [FileActionStateTransition]  "
	"WITH CHECK ADD CONSTRAINT [FK_FileActionStateTransition_FAMFile] FOREIGN KEY([FileID]) "
	"REFERENCES [FAMFile] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_FILE_ACTION_STATE_TRANSITION_MACHINE_FK = 
	"ALTER TABLE [FileActionStateTransition] "
	"WITH CHECK ADD CONSTRAINT [FK_FileActionStateTransition_Machine] FOREIGN KEY([MachineID]) "
	"REFERENCES [Machine] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_FILE_ACTION_STATE_TRANSITION_FAM_USER_FK = 
	"ALTER TABLE [FileActionStateTransition] "
	"WITH CHECK ADD CONSTRAINT [FK_FileActionStateTransition_FAMUser] FOREIGN KEY([FAMUserID]) "
	"REFERENCES [FAMUser] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_FILE_ACTION_STATE_TRANSITION_ACTION_STATE_TO_FK = 
	"ALTER TABLE [dbo].[FileActionStateTransition] "
	"WITH CHECK ADD CONSTRAINT [FK_FileActionStateTransition_ActionState_To] FOREIGN KEY([ASC_To])"
	"REFERENCES [ActionState] ([Code])";

static const string gstrADD_FILE_ACTION_STATE_TRANSITION_ACTION_STATE_FROM_FK = 
	"ALTER TABLE [dbo].[FileActionStateTransition] "
	"WITH CHECK ADD CONSTRAINT [FK_FileActionStateTransition_ActionState_From] FOREIGN KEY([ASC_From])"
	"REFERENCES [ActionState] ([Code])";

static const string gstrADD_QUEUE_EVENT_FAM_FILE_FK = 
	"ALTER TABLE [QueueEvent]  "
	"WITH CHECK ADD CONSTRAINT [FK_QueueEvent_File] FOREIGN KEY([FileID]) "
	"REFERENCES [FAMFile] ([ID])";

static const string gstrADD_QUEUE_EVENT_QUEUE_EVENT_CODE_FK = 
	"ALTER TABLE [QueueEvent]  "
	"WITH CHECK ADD CONSTRAINT [FK_QueueEvent_QueueEventCode] FOREIGN KEY([QueueEventCode]) "
	"REFERENCES [QueueEventCode] ([Code])";

static const string gstrADD_QUEUE_EVENT_MACHINE_FK =
	"ALTER TABLE [QueueEvent] "
	"WITH CHECK ADD CONSTRAINT [FK_QueueEvent_Machine] FOREIGN KEY([MachineID]) "
	"REFERENCES [Machine] ([ID])";

static const string gstrADD_QUEUE_EVENT_FAM_USER_FK =
	"ALTER TABLE [QueueEvent] "
	"WITH CHECK ADD CONSTRAINT [FK_QueueEvent_FAMUser] FOREIGN KEY([FAMUserID]) "
	"REFERENCES [FAMUser] ([ID])";

static const string gstrADD_QUEUE_EVENT_ACTION_FK =
	"ALTER TABLE [QueueEvent] "
	"WITH CHECK ADD CONSTRAINT [FK_QueueEvent_Action] FOREIGN KEY([ActionID]) "
	"REFERENCES [Action] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_FILE_ACTION_COMMENT_FAM_FILE_FK =
	"ALTER TABLE [FileActionComment] "
	"WITH CHECK ADD CONSTRAINT [FK_FileActionComment_FAMFILE] FOREIGN KEY([FileID]) "
	"REFERENCES [FAMFile] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_FILE_ACTION_COMMENT_ACTION_FK = 
	"ALTER TABLE [FileActionComment]  "
	"WITH CHECK ADD CONSTRAINT [FK_FileActionComment_Action] FOREIGN KEY([ActionID]) "
	"REFERENCES [Action] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_SKIPPED_FILE_FAM_FILE_FK =
	"ALTER TABLE [SkippedFile] "
	"WITH CHECK ADD CONSTRAINT [FK_SkippedFile_FAMFILE] FOREIGN KEY([FileID]) "
	"REFERENCES [FAMFile] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_SKIPPED_FILE_ACTION_FK = 
	"ALTER TABLE [SkippedFile]  "
	"WITH CHECK ADD CONSTRAINT [FK_SkippedFile_Action] FOREIGN KEY([ActionID]) "
	"REFERENCES [Action] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_FILE_TAG_FAM_FILE_FK =
	"ALTER TABLE [FileTag] "
	"WITH CHECK ADD CONSTRAINT [FK_FileTag_FamFile] FOREIGN KEY([FileID]) "
	"REFERENCES [FAMFile] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_FILE_TAG_TAG_ID_FK =
	"ALTER TABLE [FileTag] "
	"WITH CHECK ADD CONSTRAINT [FK_FileTag_Tag] FOREIGN KEY([TagID]) "
	"REFERENCES [Tag] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_LOCKED_FILE_ACTION_FK = 
	"ALTER TABLE [dbo].[LockedFile]  "
	"WITH CHECK ADD  CONSTRAINT [FK_LockedFile_Action] FOREIGN KEY([ActionID])"
	"REFERENCES [dbo].[Action] ([ID])"
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_LOCKED_FILE_ACTION_STATE_FK =
	"ALTER TABLE [dbo].[LockedFile]  "
	"WITH CHECK ADD  CONSTRAINT [FK_LockedFile_ActionState] FOREIGN KEY([StatusBeforeLock])"
	"REFERENCES [dbo].[ActionState] ([Code])"
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_LOCKED_FILE_FAMFILE_FK = 
	"ALTER TABLE [dbo].[LockedFile]  "
	"WITH CHECK ADD  CONSTRAINT [FK_LockedFile_FAMFile] FOREIGN KEY([FileID])"
	"REFERENCES [dbo].[FAMFile] ([ID])"
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_LOCKED_FILE_PROCESSINGFAM_FK =
	"ALTER TABLE [dbo].[LockedFile]  "
	"WITH CHECK ADD  CONSTRAINT [FK_LockedFile_ProcessingFAM] FOREIGN KEY([UPIID])"
	"REFERENCES [dbo].[ProcessingFAM] ([ID])"
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

// Do not want ON UPDATE CASCADE or ON DELETE CASCADE because if
// there are records in the ProcessingFAM table there is a FAM processing or Records that need
// to be reverted.
static const string gstrADD_ACTION_PROCESSINGFAM_FK =
	"ALTER TABLE [dbo].[ProcessingFAM]  "
	"WITH CHECK ADD  CONSTRAINT [FK_ProcessingFAM_Action] FOREIGN KEY([ActionID])"
	"REFERENCES [dbo].[Action] ([ID])";

static const string gstrADD_FAM_SESSION_MACHINE_FK =
	"ALTER TABLE [dbo].[FAMSession] "
	"WITH CHECK ADD CONSTRAINT [FK_FAMSession_Machine] FOREIGN KEY([MachineID]) "
	"REFERENCES [dbo].[Machine]([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_FAM_SESSION_FAMUSER_FK =
	"ALTER TABLE [dbo].[FAMSession] "
	"WITH CHECK ADD CONSTRAINT [FK_FAMSession_User] FOREIGN KEY([FAMUserID]) "
	"REFERENCES [dbo].[FAMUser]([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_FAM_SESSION_FPSFILE_FK =
	"ALTER TABLE [dbo].[FAMSession] "
	"WITH CHECK ADD CONSTRAINT [FK_FAMSession_FPSFile] FOREIGN KEY([FPSFileID]) "
	"REFERENCES [dbo].[FPSFile]([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_INPUT_EVENT_ACTION_FK =
	"ALTER TABLE [dbo].[InputEvent] "
	"WITH CHECK ADD CONSTRAINT [FK_InputEvent_Action] FOREIGN KEY([ActionID]) "
	"REFERENCES [dbo].[Action]([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_INPUT_EVENT_MACHINE_FK =
	"ALTER TABLE [dbo].[InputEvent] "
	"WITH CHECK ADD CONSTRAINT [FK_InputEvent_Machine] FOREIGN KEY([MachineID]) "
	"REFERENCES [dbo].[Machine]([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_INPUT_EVENT_FAMUSER_FK =
	"ALTER TABLE [dbo].[InputEvent] "
	"WITH CHECK ADD CONSTRAINT [FK_InputEvent_User] FOREIGN KEY([FAMUserID]) "
	"REFERENCES [dbo].[FAMUser]([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_FILE_ACTION_STATUS_ACTION_FK = 
	"ALTER TABLE [dbo].[FileActionStatus]  "
	"WITH CHECK ADD  CONSTRAINT [FK_FileActionStatus_Action] FOREIGN KEY([ActionID]) "
	"REFERENCES [dbo].[Action] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_FILE_ACTION_STATUS_FAMFILE_FK = 
	"ALTER TABLE [dbo].[FileActionStatus]  "
	"WITH CHECK ADD  CONSTRAINT [FK_FileActionStatus_FAMFile] FOREIGN KEY([FileID]) "
	"REFERENCES [dbo].[FAMFile] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_FILE_ACTION_STATUS_ACTION_STATUS_FK = 
	"ALTER TABLE [dbo].[FileActionStatus]  "
	"WITH CHECK ADD  CONSTRAINT [FK_FileActionStatus_ActionStatus] FOREIGN KEY([ActionStatus]) "
	"REFERENCES [dbo].[ActionState] ([Code]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_ACTION_STATISTICS_DELTA_ACTION_FK = 
	"ALTER TABLE [dbo].[ActionStatisticsDelta] "
	"WITH CHECK ADD CONSTRAINT [FK_ActionStatisticsDelta_Action] FOREIGN KEY([ActionID]) "
	"REFERENCES [dbo].[Action] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

// Query for obtaining the current db lock record with the time it has been locked
static const string gstrDB_LOCK_QUERY = 
	"SELECT LockID, UPI, LockTime, DATEDIFF(second, LockTime, GETDATE()) AS TimeLocked "
	"FROM LockTable";

// Query for deleting all locks from the lock table
static const string gstrDELETE_DB_LOCK = "DELETE FROM LockTable";

// Query to shrink the current database
static const string gstrSHRINK_DATABASE = "DBCC SHRINKDATABASE (0)";

// Constant to be replaced in the DBInfo Setting query
static const string gstrSETTING_NAME = "<SettingName>";

// Query for looking for a specific setting
// To use run replaceVariable to replace <SettingName>
static const string gstrDBINFO_SETTING_QUERY = 
	"SELECT [Name], [Value] FROM DBInfo WHERE [Name] = '" + gstrSETTING_NAME + "'";

// Query to delete old input event records from the InputEvent table
static const string gstrDELETE_OLD_INPUT_EVENT_RECORDS =
	"DELETE FROM InputEvent WHERE DATEDIFF(d, GETDATE(), [TimeStamp]) > (SELECT COALESCE("
	"(SELECT CAST([Value] AS int) FROM [DBInfo] WHERE [Name] = '"
	+ gstrINPUT_EVENT_HISTORY_SIZE + "'), 30))";

// Query to use to calclulate and insert a new ActionStatistics records for the ActionID = <ActionIDToRecreate>
// the <ActionIDToRecreate> needs to be replaced with the action id to recreate.
// NOTE: This query will throw and exception if the ActionStatistics record for that action id 
//		 already exists.
static const string gstrRECREATE_ACTION_STATISTICS_FOR_ACTION = 
	"INSERT INTO ActionStatistics  "
	"SELECT 	ActionID, "
	"	GetDate() AS LastupdateTimeStamp, "
	"	NumDocuments,  "
	"	NumDocumentsPending,  "
	"	NumDocumentsComplete,  "
	"	NumDocumentsFailed, "
	"	NumDocumentsSkipped, "
	"	NumPages, "
	"	NumPagesPending, "
	"	NumPagesComplete, "
	"	NumPagesFailed, "
	"	NumPagesSkipped, "
	"	NumBytes, "
	"	NumBytesPending, "
	"	NumBytesComplete, "
	"	NumBytesFailed, "
	"	NumBytesSkipped "
	"FROM "
	"(Select ActionID,  "
	"	SUM(TotalDocuments) AS NumDocuments,  "
	"	SUM(DocsPending) AS [NumDocumentsPending], "
	"	SUM(DocsCompleted) AS [NumDocumentsComplete],  "
	"	SUM(DocsFailed) AS [NumDocumentsFailed],  "
	"	SUM(DocsSkipped) AS [NumDocumentsSkipped],  "
	"	SUM(TotalPages) AS [NumPages], "
	"	SUM(PagesPending) AS [NumPagesPending], "
	"	SUM(PagesCompleted) AS [NumPagesComplete], "
	"	SUM(PagesFailed) AS [NumPagesFailed],  "
	"	SUM(PagesSkipped) AS [NumPagesSkipped], "
	"	SUM(TotalSize) AS [NumBytes],	 "
	"	SUM(BytesPending) AS [NumBytesPending], "
	"	SUM(BytesCompleted) AS [NumBytesComplete],  "
	"	SUM(BytesFailed) AS [NumBytesFailed], "
	"	SUM(BytesSkipped) AS [NumBytesSkipped] "
	"	 "
	"From "
	"(SELECT     Action.ID as ActionID, FileActionStatus.ActionStatus, SUM(COALESCE(FAMFile.FileSize,0)) AS TotalSize, SUM(COALESCE(FAMFile.Pages,0)) AS TotalPages,  "
	"                      SUM(CASE WHEN FileActionStatus.FileID IS NULL THEN 0 ELSE 1 END) AS TotalDocuments, SUM(CASE WHEN ActionStatus = 'C' THEN 1 ELSE 0 END) AS DocsCompleted,  "
	"                      SUM(CASE WHEN ActionStatus = 'F' THEN 1 ELSE 0 END) AS DocsFailed, SUM(CASE WHEN ActionStatus = 'S' THEN 1 ELSE 0 END) AS DocsSkipped,  "
	"                      SUM(CASE WHEN ActionStatus = 'C' THEN COALESCE(FAMFile.Pages, 0) ELSE 0 END) AS PagesCompleted,  "
	"                      SUM(CASE WHEN ActionStatus = 'F' THEN COALESCE(FAMFile.Pages, 0) ELSE 0 END) AS PagesFailed,  "
	"                      SUM(CASE WHEN ActionStatus = 'S' THEN COALESCE(FAMFile.Pages, 0) ELSE 0 END) AS PagesSkipped,  "
	"                      SUM(CASE WHEN ActionStatus = 'C' THEN COALESCE(FAMFile.FileSize, 0) ELSE 0 END) AS BytesCompleted,  "
	"                      SUM(CASE WHEN ActionStatus = 'F' THEN COALESCE(FAMFile.FileSize, 0) ELSE 0 END) AS BytesFailed,  "
	"                      SUM(CASE WHEN ActionStatus = 'S' THEN COALESCE(FAMFile.FileSize, 0) ELSE 0 END) AS BytesSkipped,  "
	"                      SUM(CASE WHEN ActionStatus = 'P' THEN 1 ELSE 0 END) AS DocsPending, "
	"                      SUM(CASE WHEN ActionStatus = 'P' THEN COALESCE(FAMFile.Pages, 0) ELSE 0 END) AS PagesPending, "
	"                      SUM(CASE WHEN ActionStatus = 'P' THEN COALESCE(FAMFile.FileSize, 0) ELSE 0 END) AS BytesPending, "
	"                      Action.ID AS ASAction "
	"FROM         FileActionStatus INNER JOIN "
	"                      FAMFile ON FileActionStatus.FileID = FAMFile.ID LEFT JOIN "
	"                      ActionStatistics ON ActionStatistics.ActionID = FileActionStatus.ActionID RIGHT JOIN Action ON Action.ID = FileActionStatus.ActionID "
	"WHERE     (ActionStatistics.ActionID IS NULL)  "
	"GROUP BY Action.ID, FileActionStatus.ActionStatus, ActionStatistics.ActionID) as totals "
	"GROUP BY ActionID) as NewStats "
	"where ActionID = <ActionIDToRecreate> ";

// Query to use to update the ActionStatistics table from the ActionStatisticsDelta table
// There are to variables that need to be replaced:
//		<LastDeltaID>	Should be replaced with the last record in the ActionStatisticsDelta table 
//						that will be included in the update to the ActionStatistics
//		<ActionIDToUpdate> Should be replaced with the ActionID that is being updated
static const string gstrUPDATE_ACTION_STATISTICS_FOR_ACTION_FROM_DELTA = 
	"UPDATE ActionStatistics "
	"	SET LastUpdateTimeStamp = GETDATE(),  "
	"	[NumDocuments] = ActionStatistics.[NumDocuments] + Changes.[NumDocuments], "
	"	[NumDocumentsPending] = ActionStatistics.[NumDocumentsPending] + Changes.[NumDocumentsPending], "
	"	[NumDocumentsComplete] =  ActionStatistics.[NumDocumentsComplete] + Changes.[NumDocumentsComplete], "
	"    [NumDocumentsFailed] =  ActionStatistics.[NumDocumentsFailed] + Changes.[NumDocumentsFailed], "
	"    [NumDocumentsSkipped] =  ActionStatistics.[NumDocumentsSkipped] + Changes.[NumDocumentsSkipped], "
	"    [NumPages] =  ActionStatistics.[NumPages] + Changes.[NumPages], "
	"    [NumPagesPending] =  ActionStatistics.[NumPagesPending] + Changes.[NumPagesPending], "
	"    [NumPagesComplete] =  ActionStatistics.[NumPagesComplete] + Changes.[NumPagesComplete], "
	"    [NumPagesFailed] =  ActionStatistics.[NumPagesFailed] + Changes.[NumPagesFailed], "
	"    [NumPagesSkipped] =  ActionStatistics.[NumPagesSkipped] + Changes.[NumPagesSkipped], "
	"    [NumBytes] =  ActionStatistics.[NumBytes] + Changes.[NumBytes], "
	"    [NumBytesPending] =  ActionStatistics.[NumBytesPending] + Changes.[NumBytesPending], "
	"    [NumBytesComplete] =  ActionStatistics.[NumBytesComplete] + Changes.[NumBytesComplete], "
	"    [NumBytesFailed] =  ActionStatistics.[NumBytesFailed] + Changes.[NumBytesFailed], "
	"    [NumBytesSkipped] =  ActionStatistics.[NumBytesSkipped] + Changes.[NumBytesSkipped] "
	"       "
	"FROM        "
	"(SELECT [ActionID] "
	"      ,SUM([NumDocuments]) AS [NumDocuments] "
	"      ,SUM([NumDocumentsPending]) AS [NumDocumentsPending] "
	"      ,SUM([NumDocumentsComplete]) AS [NumDocumentsComplete] "
	"      ,SUM([NumDocumentsFailed]) AS [NumDocumentsFailed] "
	"      ,SUM([NumDocumentsSkipped]) AS [NumDocumentsSkipped] "
	"      ,SUM([NumPages]) AS [NumPages] "
	"      ,SUM([NumPagesPending]) AS [NumPagesPending] "
	"      ,SUM([NumPagesComplete]) AS [NumPagesComplete] "
	"      ,SUM([NumPagesFailed]) AS [NumPagesFailed] "
	"      ,SUM([NumPagesSkipped]) AS [NumPagesSkipped] "
	"      ,SUM([NumBytes]) AS [NumBytes] "
	"      ,SUM([NumBytesPending]) AS [NumBytesPending] "
	"      ,SUM([NumBytesComplete]) AS [NumBytesComplete] "
	"      ,SUM([NumBytesFailed]) AS [NumBytesFailed] "
	"      ,SUM([NumBytesSkipped]) AS [NumBytesSkipped] "
	"  FROM [Demo_IDShield].[dbo].[ActionStatisticsDelta] "
	"  WHERE  ID <= <LastDeltaID> "
	"  GROUP BY ActionID) as Changes "
	"  WHERE ActionStatistics.ActionID = <ActionIDToUpdate>";



