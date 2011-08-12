#pragma once

#include <FAMUtilsConstants.h>

#include <string>

using namespace std;

// User ID and Machine ID query constants
static const string gstrUSER_ID_VAR = "<UserID>";
static const string gstrMACHINE_ID_VAR = "<MachineID>";

// Create Table SQL statements
static const string gstrCREATE_ACTION_TABLE = "CREATE TABLE [Action] ([ID] [int] IDENTITY(1,1) NOT NULL "
	"CONSTRAINT [PK_Action] PRIMARY KEY CLUSTERED, " 
	"[ASCName] [nvarchar](50) NOT NULL,	[Description] [nvarchar](255) NULL)";

static const string gstrCREATE_LOCK_TABLE = 
	"CREATE TABLE [LockTable]([LockName] [nvarchar](50) NOT NULL CONSTRAINT [PK_LockTable] PRIMARY KEY CLUSTERED,"
	"[UPI] [nvarchar](512), "
	"[LockTime] datetime NOT NULL CONSTRAINT [DF_LockTable_LockTime]  DEFAULT (GETDATE()))";

static const string gstrCREATE_DB_INFO_TABLE = 
	"CREATE TABLE [DBInfo]([ID] int IDENTITY(1,1) NOT NULL, "
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

// The ProcessingFAM table is now the ActiveFAM table, but this definition needs to remain for the
// schema update process.
static const string gstrCREATE_PROCESSING_FAM_TABLE = 
	"CREATE TABLE [ProcessingFAM]([ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_ProcessingFAM] PRIMARY KEY CLUSTERED, "
	"[ActionID] [int] NOT NULL, "
	"[UPI] [nvarchar](450), "
	"[LastPingTime] datetime NOT NULL CONSTRAINT [DF_ProcessingFAM_LastPingTime]  DEFAULT (GETDATE()))";

static const string gstrCREATE_ACTIVE_FAM_TABLE = 
	"CREATE TABLE [ActiveFAM]([ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_ActiveFAM] PRIMARY KEY CLUSTERED, "
	"[ActionID] [int] NOT NULL, "
	"[UPI] [nvarchar](450), "
	"[LastPingTime] datetime NOT NULL CONSTRAINT [DF_ActiveFAM_LastPingTime]  DEFAULT (GETDATE()),"
	"[Queuing] [bit] NOT NULL,"
	"[Processing] [bit] NOT NULL)";

static const string gstrCREATE_LOCKED_FILE_TABLE = 
	"CREATE TABLE [LockedFile]([FileID] [int] NOT NULL,"
	"[ActionID] [int] NOT NULL, "
	"[UPIID] [int] , "
	"[StatusBeforeLock] [nvarchar](1) NOT NULL, "
	"CONSTRAINT [PK_LockedFile] PRIMARY KEY CLUSTERED ([FileID], [ActionID], [UPIID]))";

static const string gstrCREATE_USER_CREATED_COUNTER_TABLE =
	"CREATE TABLE [UserCreatedCounter] ("
	"[CounterName] [nvarchar](50) NOT NULL CONSTRAINT [PK_UserCreatedCounter] PRIMARY KEY CLUSTERED,"
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

static const string gstrCREATE_SOURCE_DOC_CHANGE_HISTORY =
	"CREATE TABLE [SourceDocChangeHistory]( "
	"[ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_SourceDocChangeHistory] PRIMARY KEY CLUSTERED, "
	"[FileID] [int] NOT NULL, "
	"[FromFileName]  [nvarchar](255) NULL,"
	"[ToFileName]  [nvarchar](255) NULL,"
	"[TimeStamp] [DateTime] NOT NULL, "
	"[FAMUserID] int NOT NULL, "
	"[MachineID] int NOT NULL) ";

static const string gstrCREATE_DOC_TAG_HISTORY_TABLE =
	"CREATE TABLE [DocTagHistory]( "
	"[ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_DocTagHistory] PRIMARY KEY CLUSTERED, "
	"[FileID] [int] NOT NULL, "
	"[TagID] [int] NOT NULL, "
	"[Tagged] [bit] NOT NULL,"
	"[TimeStamp] [DateTime] NOT NULL, "
	"[FAMUserID] int NOT NULL, "
	"[MachineID] int NOT NULL) ";

static const string gstrCREATE_DB_INFO_CHANGE_HISTORY_TABLE =
	"CREATE TABLE [DBInfoChangeHistory]( "
	"[ID] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_DBInfoHistory] PRIMARY KEY CLUSTERED, "
	"[FAMUserID] INT NOT NULL, "
	"[MachineID] INT NOT NULL, "
	"[DBInfoID] INT NOT NULL, "
	"[OldValue] NVARCHAR(MAX), "
	"[NewValue] NVARCHAR(MAX), "
	"[TimeStamp] DATETIME NOT NULL DEFAULT(GETDATE()))";

// Create table indexes SQL
static const string gstrCREATE_DB_INFO_ID_INDEX = "CREATE UNIQUE NONCLUSTERED INDEX [IX_DBInfo_ID] "
	"ON [DBInfo]([ID])";

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

static const string gstrCREATE_ACTIVE_FAM_UPI_INDEX = "CREATE UNIQUE NONCLUSTERED INDEX "
	"[IX_ActiveFAM_UPI] ON [ActiveFAM]([UPI])";

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

// The ProcessingFAM table is now the ActiveFAM table, but this definition needs to remain for the
// schema update process.
static const string gstrADD_LOCKED_FILE_PROCESSINGFAM_FK =
	"ALTER TABLE [dbo].[LockedFile]  "
	"WITH CHECK ADD  CONSTRAINT [FK_LockedFile_ProcessingFAM] FOREIGN KEY([UPIID])"
	"REFERENCES [dbo].[ProcessingFAM] ([ID])"
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_LOCKED_FILE_ACTIVEFAM_FK =
	"ALTER TABLE [dbo].[LockedFile]  "
	"WITH CHECK ADD  CONSTRAINT [FK_LockedFile_ActiveFAM] FOREIGN KEY([UPIID])"
	"REFERENCES [dbo].[ActiveFAM] ([ID])"
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

// The ProcessingFAM table is now the ActiveFAM table, but this definition needs to remain for the
// schema update process.
static const string gstrADD_ACTION_PROCESSINGFAM_FK =
	"ALTER TABLE [dbo].[ProcessingFAM]  "
	"WITH CHECK ADD  CONSTRAINT [FK_ProcessingFAM_Action] FOREIGN KEY([ActionID])"
	"REFERENCES [dbo].[Action] ([ID])";

// Do not want ON UPDATE CASCADE or ON DELETE CASCADE because if
// there are records in the ActiveFAM table there is a FAM processing or Records that need
// to be reverted.
static const string gstrADD_ACTION_ACTIVEFAM_FK =
	"ALTER TABLE [dbo].[ActiveFAM]  "
	"WITH CHECK ADD  CONSTRAINT [FK_ActiveFAM_Action] FOREIGN KEY([ActionID])"
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

static const string gstrADD_SOURCE_DOC_CHANGE_HISTORY_FAMFILE_FK = 
	"ALTER TABLE [dbo].[SourceDocChangeHistory]  "
	"WITH CHECK ADD  CONSTRAINT [FK_SourceDocChangeHistory_FAMFile] FOREIGN KEY([FileID]) "
	"REFERENCES [dbo].[FAMFile] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_SOURCE_DOC_CHANGE_HISTORY_FAMUSER_FK =
	"ALTER TABLE [dbo].[SourceDocChangeHistory] "
	"WITH CHECK ADD CONSTRAINT [FK_SourceDocChangeHistory_User] FOREIGN KEY([FAMUserID]) "
	"REFERENCES [dbo].[FAMUser]([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_SOURCE_DOC_CHANGE_HISTORY_MACHINE_FK =
	"ALTER TABLE [dbo].[SourceDocChangeHistory] "
	"WITH CHECK ADD CONSTRAINT [FK_SourceDocChangeHistory_Machine] FOREIGN KEY([MachineID]) "
	"REFERENCES [dbo].[Machine]([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_DOC_TAG_HISTORY_FAMFILE_FK = 
	"ALTER TABLE [dbo].[DocTagHistory]  "
	"WITH CHECK ADD CONSTRAINT [FK_DocTagHistory_FAMFile] FOREIGN KEY([FileID]) "
	"REFERENCES [dbo].[FAMFile] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_DOC_TAG_HISTORY_TAG_FK = 
	"ALTER TABLE [dbo].[DocTagHistory]  "
	"WITH CHECK ADD CONSTRAINT [FK_DocTagHistory_Tag] FOREIGN KEY([TagID]) "
	"REFERENCES [dbo].[Tag] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_DOC_TAG_HISTORY_FAMUSER_FK = 
	"ALTER TABLE [dbo].[DocTagHistory]  "
	"WITH CHECK ADD CONSTRAINT [FK_DocTagHistory_FAMUser] FOREIGN KEY([FAMUserID]) "
	"REFERENCES [dbo].[FAMUser] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_DOC_TAG_HISTORY_MACHINE_FK = 
	"ALTER TABLE [dbo].[DocTagHistory]  "
	"WITH CHECK ADD CONSTRAINT [FK_DocTagHistory_Machine] FOREIGN KEY([MachineID]) "
	"REFERENCES [dbo].[Machine] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_DB_INFO_HISTORY_MACHINE_FK =
	"ALTER TABLE [dbo].[DBInfoChangeHistory] "
	"WITH CHECK ADD CONSTRAINT [FK_DBInfoChangeHistory_Machine] FOREIGN KEY([MachineID]) "
	"REFERENCES [dbo].[Machine]([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_DB_INFO_HISTORY_FAMUSER_FK =
	"ALTER TABLE [dbo].[DBInfoChangeHistory] "
	"WITH CHECK ADD CONSTRAINT [FK_DBInfoChangeHistory_User] FOREIGN KEY([FAMUserID]) "
	"REFERENCES [dbo].[FAMUser]([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_DB_INFO_HISTORY_DB_INFO_FK =
	"ALTER TABLE [dbo].[DBInfoChangeHistory] "
	"WITH CHECK ADD CONSTRAINT [FK_DBInfoChageHistory_DBInfo] FOREIGN KEY([DBinfoID]) "
	"REFERENCES [dbo].[DBInfo]([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

// Query for obtaining the current db lock record with the time it has been locked
static const string gstrDB_LOCK_NAME_VAL = "<LockName>";
static const string gstrDB_LOCK_QUERY = 
	"SELECT LockName, UPI, LockTime, DATEDIFF(second, LockTime, GETDATE()) AS TimeLocked "
	"FROM LockTable WHERE LockName = '" + gstrDB_LOCK_NAME_VAL + "'";

// Query for deleting specific locks from the lock table
static const string gstrDELETE_DB_LOCK = "DELETE FROM LockTable WHERE [LockName] = '"
	+ gstrDB_LOCK_NAME_VAL + "'";

// Query to shrink the current database
static const string gstrSHRINK_DATABASE = "DBCC SHRINKDATABASE (0)";

// Constant to be replaced in the DBInfo Setting query
static const string gstrSETTING_NAME = "<SettingName>";

// Query for looking for a specific setting
// To use run replaceVariable to replace <SettingName>
static const string gstrDBINFO_SETTING_QUERY = 
	"SELECT [Name], [Value] FROM DBInfo WHERE [Name] = '" + gstrSETTING_NAME + "'";

// Query for getting all DB info settings
static const string gstrDBINFO_GET_SETTINGS_QUERY =
	"SELECT [Name], [Value] FROM DBInfo";

// Constant to be replaced in the DBInfo Setting query
static const string gstrSETTING_VALUE = "<SettingValue>";

// Query for updating the DB info settings
static const string gstrDBINFO_UPDATE_SETTINGS_QUERY =
	"UPDATE DBInfo SET [Value] = '" + gstrSETTING_VALUE + "' WHERE [Name] = '"
	+ gstrSETTING_NAME + "' AND [Value] <> '" + gstrSETTING_VALUE + "'";

// Query for updating the DB info settings and storing the change history
static const string gstrDBINFO_UPDATE_SETTINGS_QUERY_STORE_HISTORY =
	"DECLARE @ChangeHistory TABLE (UserID INT, MachineID INT, DBInfoID INT, "
	"OldValue NVARCHAR(MAX), NewValue NVARCHAR(MAX)); UPDATE [DBInfo] SET [Value] = '"
	+ gstrSETTING_VALUE + "' OUTPUT " + gstrUSER_ID_VAR + ", "
	+ gstrMACHINE_ID_VAR + ", INSERTED.[ID] AS DBInfoID, "
	"DELETED.[Value] AS OldValue, INSERTED.[Value] AS NewValue INTO "
	"@ChangeHistory (UserID, MachineID, DBInfoID, OldValue, NewValue) "
	"WHERE [Name] = '" + gstrSETTING_NAME + "' AND [Value] <> '" + gstrSETTING_VALUE
	+ "'; INSERT INTO [DBInfoChangeHistory] ([FAMUserID], [MachineID], [DBInfoID], "
	"[OldValue], [NewValue]) SELECT * FROM @ChangeHistory;";

// Query to set the last DB info changed time
static const string gstrUPDATE_DB_INFO_LAST_CHANGE_TIME =
	"UPDATE [DBInfo] SET [Value] = CONVERT(NVARCHAR(MAX), GETDATE(), 21) WHERE [Name] = '"
	+ gstrLAST_DB_INFO_CHANGE + "'";

// Query to delete old input event records from the InputEvent table
static const string gstrDELETE_OLD_INPUT_EVENT_RECORDS =
	"DELETE FROM InputEvent WHERE DATEDIFF(d, GETDATE(), [TimeStamp]) > (SELECT COALESCE("
	"(SELECT CAST([Value] AS int) FROM [DBInfo] WHERE [Name] = '"
	+ gstrINPUT_EVENT_HISTORY_SIZE + "'), 30))";

// Query to use to calclulate and insert new ActionStatistics records for the ActionIDs when the id
// to recreate is determined by the <ActionIDWhereClause> which needs to be substituted in.
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
	"<ActionIDWhereClause> ";

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
	"  FROM [ActionStatisticsDelta] "
	"  WHERE  ID <= <LastDeltaID> AND ActionStatisticsDelta.ActionID = <ActionIDToUpdate> "
	"  GROUP BY ActionID) as Changes "
	"  WHERE ActionStatistics.ActionID = <ActionIDToUpdate>";

// Query used to get the files to process and add the appropriate items to LockedFile and FAST 
// Variables that need to be replaced:
//		<SelectFilesToProcessQuery> - The complete query that will select the files to process
//		<ActionID> - The ID of the action being processed
//		<UserID> - ID for the files are being processed under
//		<MachineID> - ID for the machine processing the files
//		<UPIID> - UPIID of the processing FAM
static const string gstrGET_FILES_TO_PROCESS_QUERY = 
	"DECLARE @OutputTableVar table ( \r\n"
	"	[ID] [int] NOT NULL, \r\n"
	"	[FileName] [nvarchar](255) NULL, \r\n"
	"	[FileSize] [bigint] NOT NULL, \r\n"
	"	[Pages] [int] NOT NULL, \r\n"
	"	[Priority] [int] NOT NULL, \r\n"
	"	[ASC_From] [nvarchar](1) NOT NULL \r\n"
	"); \r\n"
	"SET NOCOUNT ON \r\n"
	"BEGIN TRY \r\n"
	"	UPDATE FileActionStatus Set ActionStatus = 'R'  \r\n"
	"	OUTPUT ATABLE.ID, ATABLE.FileName, ATABLE.FileSize, ATABLE.Pages, ATABLE.Priority, deleted.ActionStatus INTO @OutputTableVar \r\n"
	"	FROM  \r\n"
	"	( \r\n"
	"		<SelectFilesToProcessQuery> ) AS ATABLE  \r\n"
	"	INNER JOIN FileActionStatus on FileActionStatus.FileID = ATABLE.ID AND FileActionStatus.ActionID = <ActionID>;  \r\n"
	"	INSERT INTO FileActionStateTransition (FileID, ActionID,  ASC_From, ASC_To,  \r\n"
	"		DateTimeStamp, FAMUserID, MachineID, Exception, Comment) \r\n"
	"	SELECT id, <ActionID> as ActionID, ASC_From, 'R' as ASC_To, GETDATE() AS DateTimeStamp,  \r\n"
	"		<UserID> as UserID, <MachineID> as MachineID, '' as Exception, '' as Comment FROM @OutputTableVar; \r\n"
	"	INSERT INTO LockedFile(FileID,ActionID,UPIID,StatusBeforeLock) \r\n"
	"		SELECT ID, <ActionID> as ActionID, <UPIID> AS UPIID, ASC_From AS StatusBeforeLock FROM @OutputTableVar; \r\n"
	"	SET NOCOUNT OFF \r\n"
	"END TRY \r\n"
	"BEGIN CATCH"
	"\r\n"
	// Ensure NOCOUNT is set to OFF
	"SET NOCOUNT OFF\r\n"
	"\r\n"
	// Get the error message, severity and state
	"	DECLARE @ErrorMessage NVARCHAR(4000);\r\n"
	"	DECLARE @ErrorSeverity INT;\r\n"
	"	DECLARE @ErrorState INT;\r\n"
	"\r\n"
	"SELECT \r\n"
	"	@ErrorMessage = ERROR_MESSAGE(),\r\n"
	"	@ErrorSeverity = ERROR_SEVERITY(),\r\n"
	"	@ErrorState = ERROR_STATE();\r\n"
	"\r\n"
	// Check for state of 0 (cannot raise error with state 0, set to 1)
	"IF @ErrorState = 0\r\n"
	"	SELECT @ErrorState = 1\r\n"
	"\r\n"
	// Raise the error so that it will be caught at the outer scope
	"RAISERROR (@ErrorMessage,\r\n"
	"	@ErrorSeverity,\r\n"
	"	@ErrorState\r\n"
	");\r\n"
	"\r\n"
	"END CATCH\r\n"
	"SELECT * FROM @OutputTableVar ";

// Queries for tagging/untagging files and toggling tags
static const string gstrTAG_FILE_ID_VAR = "<FileID>";
static const string gstrTAG_ID_VAR = "<TagID>";
static const string gstrUPDATE_DOC_TAG_HISTORY_VAR = "<UpdateDocTagHistory>";

// Updates the DocTagHistory table if gstrUPDATE_DOC_TAG_HISTORY_VAR is 1
// NOTE: This query ends with " END" to ensure it is nested withing whichever query
// uses it. Therefore, any query using it must include "BEGIN".
#define UPDATE_DOC_TAG_HISTORY_QUERY(TagAdded) \
	" IF (1 = " + gstrUPDATE_DOC_TAG_HISTORY_VAR + ")" \
	+ " INSERT INTO [DocTagHistory]  ([FileID], [TagID], " \
	"[Tagged], [TimeStamp], [FAMUserID], [MachineID]) VALUES " \
	"(" + gstrTAG_FILE_ID_VAR + ", " + gstrTAG_ID_VAR + ", " + TagAdded \
	+ ", GetDate(), " + gstrUSER_ID_VAR + ", " + gstrMACHINE_ID_VAR + ") END"; 

// Insertion query for adding a tag to a file (NOTE, this query
// will attempt to add the tag even if it already exists, if it
// already exists, this will cause a duplicate record exception
// due to the unique index on the key/tag pair)
static const string gstrADD_TAG_QUERY = 
	"BEGIN INSERT INTO [FileTag] ([FileID], [TagID]) VALUES("
	+ gstrTAG_FILE_ID_VAR + ", " + gstrTAG_ID_VAR + ")"
	+ UPDATE_DOC_TAG_HISTORY_QUERY("1");

// Adds a tag to a file if it is not already tagged
static const string gstrTAG_FILE_QUERY =
	"IF NOT EXISTS (SELECT [FileID] FROM [FileTag] WHERE [FileID] = "
	+ gstrTAG_FILE_ID_VAR + " AND [TagID] = "
	+ gstrTAG_ID_VAR + ") " + gstrADD_TAG_QUERY;

// Removes a tag from a file.
static const string gstrUNTAG_FILE_QUERY =
	"BEGIN DELETE FROM [FileTag] WHERE [FileID] = "
	+ gstrTAG_FILE_ID_VAR + " AND [TagID] = "
	+ gstrTAG_ID_VAR
	+ UPDATE_DOC_TAG_HISTORY_QUERY("0");

// Adds a tag to a file if the file isn't already tagged, otherwise
// removes the tag.
static const string gstrTOGGLE_TAG_FOR_FILE_QUERY =
	gstrTAG_FILE_QUERY + " ELSE " + gstrUNTAG_FILE_QUERY;
