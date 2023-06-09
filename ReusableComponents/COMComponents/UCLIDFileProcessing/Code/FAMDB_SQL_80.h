// FAMDB_SQL_80.h - Constants for DB SQL queries as of schema version 23, which corresponds to
// FLEX/IDS 8.0

#pragma once

#include <FAMUtilsConstants.h>

#include <string>

using namespace std;

// Create Table SQL statements
static const string gstrCREATE_ACTION_TABLE_80 =
	"CREATE TABLE [dbo].[Action] ([ID] [int] IDENTITY(1,1) NOT NULL "
	"CONSTRAINT [PK_Action] PRIMARY KEY CLUSTERED, " 
	"[ASCName] [nvarchar](50) NOT NULL,	[Description] [nvarchar](255) NULL)";

static const string gstrCREATE_LOCK_TABLE_80 = 
	"CREATE TABLE [dbo].[LockTable]([LockID] [int] NOT NULL CONSTRAINT [PK_LockTable] PRIMARY KEY CLUSTERED,"
	"[UPI] [nvarchar](512), "
	"[LockTime] datetime NOT NULL CONSTRAINT [DF_LockTable_LockTime]  DEFAULT (GETDATE()))";

static const string gstrCREATE_DB_INFO_TABLE_80 = 
	"CREATE TABLE [dbo].[DBInfo]("
	"[Name] [nvarchar](50) NOT NULL PRIMARY KEY CLUSTERED, "
	"[Value] [nvarchar](max))";

static const string gstrCREATE_ACTION_STATE_TABLE_80 =
	"CREATE TABLE [dbo].[ActionState]([Code] [nvarchar](1) NOT NULL "
	"CONSTRAINT [PK_ActionState] PRIMARY KEY CLUSTERED,"
	"[Meaning] [nvarchar](255) NULL)";

static const string gstrCREATE_FAM_FILE_TABLE_80 = "CREATE TABLE [dbo].[FAMFile]("
	"[ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_File] PRIMARY KEY CLUSTERED,"
	"[FileName] [nvarchar](255) NULL,"
	"[FileSize] [bigint] NOT NULL CONSTRAINT [DF_FAMFile_FileSize]  DEFAULT ((0)),"
	"[Pages] [int] NOT NULL CONSTRAINT [DF_FAMFile_Pages]  DEFAULT ((0)),"
	"[Priority] [int] NOT NULL CONSTRAINT [DF_FAMFile_Priority] DEFAULT((3)))";

static const string gstrCREATE_QUEUE_EVENT_CODE_TABLE_80 = "CREATE TABLE [dbo].[QueueEventCode]("
	"[Code] [nvarchar](1) NOT NULL CONSTRAINT [PK_QueueEventCode] PRIMARY KEY CLUSTERED ,"
	"[Description] [nvarchar](255) NULL)";

static const string gstrCREATE_ACTION_STATISTICS_TABLE_80 = "CREATE TABLE [dbo].[ActionStatistics]("
	"[ActionID] [int] NOT NULL CONSTRAINT [PK_Statistics] PRIMARY KEY CLUSTERED,"
	"[NumDocuments] [int] NOT NULL CONSTRAINT [DF_Statistics_TotalDocuments]  DEFAULT ((0)),"
	"[NumDocumentsComplete] [int] NOT NULL CONSTRAINT [DF_Statistics_ProcessedDocuments]  DEFAULT ((0)),"
	"[NumDocumentsFailed] [int] NOT NULL CONSTRAINT [DF_ActionStatistics_NumDocumentsFailed]  DEFAULT ((0)),"
	"[NumDocumentsSkipped] [int] NOT NULL CONSTRAINT [DF_ActionStatistics_NumDocumentsSkipped] DEFAULT ((0)),"
	"[NumPages] [int] NOT NULL CONSTRAINT [DF_ActionStatistics_NumPages]  DEFAULT ((0)),"
	"[NumPagesComplete] [int] NOT NULL CONSTRAINT [DF_ActionStatistics_NumPagesComplete]  DEFAULT ((0)),"
	"[NumPagesFailed] [int] NOT NULL CONSTRAINT [DF_ActionStatistics_NumPagesFailed]  DEFAULT ((0)),"
	"[NumPagesSkipped] [int] NOT NULL CONSTRAINT [DF_ActionStatistics_NumPagesSkipped]  DEFAULT ((0)),"
	"[NumBytes] [bigint] NOT NULL CONSTRAINT [DF_ActionStatistics_NumBytes]  DEFAULT ((0)),"
	"[NumBytesComplete] [bigint] NOT NULL CONSTRAINT [DF_ActionStatistics_NumBytesComplete]  DEFAULT ((0)),"
	"[NumBytesFailed] [bigint] NOT NULL CONSTRAINT [DF_ActionStatistics_NumBytesFailed]  DEFAULT ((0)),"
	"[NumBytesSkipped] [bigint] NOT NULL CONSTRAINT [DF_ActionStatistics_NumBytesSkipped]  DEFAULT ((0)))";

static const string gstrCREATE_FILE_ACTION_STATE_TRANSITION_TABLE_80  =
	"CREATE TABLE [dbo].[FileActionStateTransition]("
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

static const string gstrCREATE_QUEUE_EVENT_TABLE_80 = "CREATE TABLE [dbo].[QueueEvent]("
	"[ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_QueueEvent] PRIMARY KEY CLUSTERED,"
	"[FileID] [int] NULL,"
	"[ActionID] [int] NULL,"
	"[DateTimeStamp] [datetime] NULL,"
	"[QueueEventCode] [nvarchar](1) NULL,"
	"[FileModifyTime] [datetime] NULL,"
	"[FileSizeInBytes] [bigint] NULL,"
	"[MachineID] [int] NULL, "
	"[FAMUserID] [int] NULL)";

static const string gstrCREATE_LOGIN_TABLE_80 = "CREATE TABLE [dbo].[Login]("
	"[ID] [int] IDENTITY(1,1) NOT NULL, "
	"[UserName] [nvarchar](50) NOT NULL, "
	"[Password] [nvarchar](128) NOT NULL DEFAULT(''), "
	"CONSTRAINT [PK_LoginID] PRIMARY KEY CLUSTERED ( [ID] ASC ))";

static const string gstrCREATE_MACHINE_TABLE_80 = "CREATE TABLE [dbo].[Machine]("
	"[ID] [int] IDENTITY(1,1) NOT NULL, "
	"[MachineName] [nvarchar](50) NULL, "
	"CONSTRAINT [PK_Machine] PRIMARY KEY CLUSTERED ([ID] ASC), "
	"CONSTRAINT [IX_MachineName] UNIQUE NONCLUSTERED ([MachineName]))";

static const string gstrCREATE_FAM_USER_TABLE_80 = "CREATE TABLE [dbo].[FAMUser]("
	"[ID] [int] IDENTITY(1,1) NOT NULL, "
	"[UserName] [nvarchar](50) NULL, "
	"CONSTRAINT [PK_FAMUser] PRIMARY KEY CLUSTERED ([ID] ASC), "
	"CONSTRAINT [IX_UserName] UNIQUE NONCLUSTERED ([UserName] ASC))";

static const string gstrCREATE_FAM_FILE_ACTION_COMMENT_TABLE_80 =
	"CREATE TABLE [dbo].[FileActionComment] ("
	"[ID] [int] IDENTITY(1,1) NOT NULL, "
	"[UserName] [nvarchar](50) NULL, "
	"[FileID] [int] NULL, "
	"[ActionID] [int] NULL, "
	"[Comment] [ntext] NULL, "
	"[DateTimeStamp] [datetime] NOT NULL CONSTRAINT [DF_FileActionComment_DateTimeStamp] DEFAULT((GETDATE())), "
	"CONSTRAINT [PK_FAMFileActionComment] PRIMARY KEY CLUSTERED ([ID] ASC))";

static const string gstrCREATE_FAM_SKIPPED_FILE_TABLE_80 =
	"CREATE TABLE [dbo].[SkippedFile] ("
	"[ID] [int] IDENTITY(1,1) NOT NULL, "
	"[UserName] [nvarchar](50) NULL, "
	"[FileID] [int] NULL, "
	"[ActionID] [int] NULL, "
	"[DateTimeStamp] [datetime] NOT NULL CONSTRAINT [DF_SkippedFile_DateTimeStamp] DEFAULT((GETDATE())), "
	"[TimeSinceSkipped] AS (DATEDIFF(second,[DateTimeStamp],GETDATE())), " // Computed column for time skipped
	"[UPIID] [int] NOT NULL DEFAULT(0), "
	"CONSTRAINT [PK_FAMSkippedFile] PRIMARY KEY CLUSTERED ([ID] ASC))";

static const string gstrCREATE_FAM_TAG_TABLE_80 = "CREATE TABLE [dbo].[Tag] ("
	"[ID] [int] IDENTITY(1,1) NOT NULL, "
	"[TagName] [nvarchar](100) NOT NULL, "
	"[TagDescription] [nvarchar](255) NULL, "
	"CONSTRAINT [PK_FAMTag] PRIMARY KEY CLUSTERED ([ID] ASC), "
	"CONSTRAINT [IX_TagName] UNIQUE NONCLUSTERED ([TagName] ASC))";

static const string gstrCREATE_FAM_FILE_TAG_TABLE_80 = "CREATE TABLE [dbo].[FileTag] ("
	"[FileID] [int] NOT NULL, "
	"[TagID] [int] NOT NULL)";

static const string gstrCREATE_PROCESSING_FAM_TABLE_80 = 
	"CREATE TABLE [dbo].[ProcessingFAM]([ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_ProcessingFAM] PRIMARY KEY CLUSTERED,"
	"[UPI] [nvarchar](450), "
	"[LastPingTime] datetime NOT NULL CONSTRAINT [DF_ProcessingFAM_LastPingTime]  DEFAULT (GETDATE()))";

static const string gstrCREATE_LOCKED_FILE_TABLE_80 = 
	"CREATE TABLE [dbo].[LockedFile]([FileID] [int] NOT NULL,"
	"[ActionID] [int] NOT NULL, "
	"[UPIID] [int] , "
	"[StatusBeforeLock] [nvarchar](1) NOT NULL, "
	"CONSTRAINT [PK_LockedFile] PRIMARY KEY CLUSTERED ([FileID], [ActionID], [UPIID]))";

static const string gstrCREATE_USER_CREATED_COUNTER_TABLE_80 =
	"CREATE TABLE [dbo].[UserCreatedCounter] ("
	"[CounterName] [nvarchar](50) NOT NULL CONSTRAINT [PK_UserCreatedConter] PRIMARY KEY CLUSTERED,"
	"[Value] [bigint] NOT NULL CONSTRAINT [DF_UserCreatedCounter_Value] DEFAULT((0)))";

static const string gstrCREATE_FPS_FILE_TABLE_80 =
	"CREATE TABLE [FPSFile] ("
	"[ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_FPSFile] PRIMARY KEY CLUSTERED, "
	"[FPSFileName] [nvarchar](512) NOT NULL)";

static const string gstrCREATE_FAM_SESSION_80 =
	"CREATE TABLE [dbo].[FAMSession] ("
	"[ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_FAMSession] PRIMARY KEY CLUSTERED, "
	"[MachineID] int NOT NULL, "
	"[FAMUserID] int NOT NULL, "
	"[UPI] [nvarchar](450), "
	"[StartTime] datetime NOT NULL CONSTRAINT [DF_FAMSession_StartTime] DEFAULT((GETDATE())), "
	"[StopTime] datetime, "
	"[FPSFileID] int NOT NULL)";

static const string gstrCREATE_INPUT_EVENT_80 =
	"CREATE TABLE [dbo].[InputEvent] ("
	"[ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_InputEvent] PRIMARY KEY CLUSTERED, "
	"[TimeStamp] [DateTime] NOT NULL, "
	"[ActionID] int NOT NULL, "
	"[FAMUserID] int NOT NULL, "
	"[MachineID] int NOT NULL, "
	"[PID] int NOT NULL, "
	"[SecondsWithInputEvents] int NOT NULL)";

// Create table indexes SQL
static const string gstrCREATE_FAM_FILE_ID_PRIORITY_INDEX_80 = "CREATE UNIQUE NONCLUSTERED INDEX [IX_Files_PriorityID] "
	"ON [FAMFile]([Priority] DESC, [ID] ASC)";

static const string gstrCREATE_FAM_FILE_INDEX_80 = "CREATE UNIQUE NONCLUSTERED INDEX [IX_Files_FileName] "
	"ON [FAMFile]([FileName] ASC)";

static const string gstrCREATE_QUEUE_EVENT_INDEX_80 = "CREATE NONCLUSTERED INDEX [IX_FileID] "
	"ON [QueueEvent]([FileID])";

static const string gstrCREATE_FILE_ACTION_COMMENT_INDEX_80 = "CREATE UNIQUE NONCLUSTERED INDEX "
	"[IX_File_Action_Comment] ON [FileActionComment]([FileID], [ActionID])";

static const string gstrCREATE_SKIPPED_FILE_INDEX_80 = "CREATE UNIQUE NONCLUSTERED INDEX "
	"[IX_Skipped_File] ON [SkippedFile]([FileID], [ActionID])";

static const string gstrCREATE_SKIPPED_FILE_UPI_INDEX_80 = "CREATE NONCLUSTERED INDEX "
	"[IX_Skipped_File_UPI] ON [SkippedFile]([UPIID])";

static const string gstrCREATE_FILE_TAG_INDEX_80 = "CREATE UNIQUE NONCLUSTERED INDEX "
	"[IX_File_Tag] ON [FileTag]([FileID], [TagID])";

static const string gstrCREATE_PROCESSING_FAM_UPI_INDEX_80 = "CREATE UNIQUE NONCLUSTERED INDEX "
	"[IX_ProcessingFAM_UPI] ON [ProcessingFAM]([UPI])";

static const string gstrCREATE_USER_CREATED_COUNTER_VALUE_INDEX_80 = "CREATE NONCLUSTERED INDEX "
	"[IX_UserCreatedCounter_Value] ON [UserCreatedCounter]([Value])";

static const string gstrCREATE_FPS_FILE_NAME_INDEX_80 = "CREATE NONCLUSTERED INDEX "
	"[IX_FPSFile_FPSFileName] ON [FPSFile]([FPSFileName])";

static const string gstrCREATE_INPUT_EVENT_INDEX_80 = "CREATE UNIQUE NONCLUSTERED INDEX "
	"[IX_Input_Event] ON [InputEvent]([TimeStamp], [ActionID], [MachineID], [FAMUserID], [PID])";

// Add foreign keys SQL
static const string gstrADD_STATISTICS_ACTION_FK_80 = 
	"ALTER TABLE [ActionStatistics]  "
	"WITH CHECK ADD CONSTRAINT [FK_Statistics_Action] FOREIGN KEY([ActionID]) "
	"REFERENCES [Action] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_FILE_ACTION_STATE_TRANSITION_ACTION_FK_80 = 
	"ALTER TABLE [FileActionStateTransition]  "
	"WITH CHECK ADD CONSTRAINT [FK_FileActionStateTransition_Action] FOREIGN KEY([ActionID]) "
	"REFERENCES [Action] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_FILE_ACTION_STATE_TRANSITION_FAM_FILE_FK_80 = 
	"ALTER TABLE [FileActionStateTransition]  "
	"WITH CHECK ADD CONSTRAINT [FK_FileActionStateTransition_FAMFile] FOREIGN KEY([FileID]) "
	"REFERENCES [FAMFile] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_FILE_ACTION_STATE_TRANSITION_MACHINE_FK_80 = 
	"ALTER TABLE [FileActionStateTransition] "
	"WITH CHECK ADD CONSTRAINT [FK_FileActionStateTransition_Machine] FOREIGN KEY([MachineID]) "
	"REFERENCES [Machine] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_FILE_ACTION_STATE_TRANSITION_FAM_USER_FK_80 = 
	"ALTER TABLE [FileActionStateTransition] "
	"WITH CHECK ADD CONSTRAINT [FK_FileActionStateTransition_FAMUser] FOREIGN KEY([FAMUserID]) "
	"REFERENCES [FAMUser] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_FILE_ACTION_STATE_TRANSITION_ACTION_STATE_TO_FK_80 = 
	"ALTER TABLE [dbo].[FileActionStateTransition] "
	"WITH CHECK ADD CONSTRAINT [FK_FileActionStateTransition_ActionState_To] FOREIGN KEY([ASC_To])"
	"REFERENCES [ActionState] ([Code])";

static const string gstrADD_FILE_ACTION_STATE_TRANSITION_ACTION_STATE_FROM_FK_80 = 
	"ALTER TABLE [dbo].[FileActionStateTransition] "
	"WITH CHECK ADD CONSTRAINT [FK_FileActionStateTransition_ActionState_From] FOREIGN KEY([ASC_From])"
	"REFERENCES [ActionState] ([Code])";

static const string gstrADD_QUEUE_EVENT_FAM_FILE_FK_80 = 
	"ALTER TABLE [QueueEvent]  "
	"WITH CHECK ADD CONSTRAINT [FK_QueueEvent_File] FOREIGN KEY([FileID]) "
	"REFERENCES [FAMFile] ([ID])";

static const string gstrADD_QUEUE_EVENT_QUEUE_EVENT_CODE_FK_80 = 
	"ALTER TABLE [QueueEvent]  "
	"WITH CHECK ADD CONSTRAINT [FK_QueueEvent_QueueEventCode] FOREIGN KEY([QueueEventCode]) "
	"REFERENCES [QueueEventCode] ([Code])";

static const string gstrADD_QUEUE_EVENT_MACHINE_FK_80 =
	"ALTER TABLE [QueueEvent] "
	"WITH CHECK ADD CONSTRAINT [FK_QueueEvent_Machine] FOREIGN KEY([MachineID]) "
	"REFERENCES [Machine] ([ID])";

static const string gstrADD_QUEUE_EVENT_FAM_USER_FK_80 =
	"ALTER TABLE [QueueEvent] "
	"WITH CHECK ADD CONSTRAINT [FK_QueueEvent_FAMUser] FOREIGN KEY([FAMUserID]) "
	"REFERENCES [FAMUser] ([ID])";

static const string gstrADD_QUEUE_EVENT_ACTION_FK_80 =
	"ALTER TABLE [QueueEvent] "
	"WITH CHECK ADD CONSTRAINT [FK_QueueEvent_Action] FOREIGN KEY([ActionID]) "
	"REFERENCES [Action] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_FILE_ACTION_COMMENT_FAM_FILE_FK_80 =
	"ALTER TABLE [FileActionComment] "
	"WITH CHECK ADD CONSTRAINT [FK_FileActionComment_FAMFILE] FOREIGN KEY([FileID]) "
	"REFERENCES [FAMFile] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_FILE_ACTION_COMMENT_ACTION_FK_80 = 
	"ALTER TABLE [FileActionComment]  "
	"WITH CHECK ADD CONSTRAINT [FK_FileActionComment_Action] FOREIGN KEY([ActionID]) "
	"REFERENCES [Action] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_SKIPPED_FILE_FAM_FILE_FK_80 =
	"ALTER TABLE [SkippedFile] "
	"WITH CHECK ADD CONSTRAINT [FK_SkippedFile_FAMFILE] FOREIGN KEY([FileID]) "
	"REFERENCES [FAMFile] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_SKIPPED_FILE_ACTION_FK_80 = 
	"ALTER TABLE [SkippedFile]  "
	"WITH CHECK ADD CONSTRAINT [FK_SkippedFile_Action] FOREIGN KEY([ActionID]) "
	"REFERENCES [Action] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_FILE_TAG_FAM_FILE_FK_80 =
	"ALTER TABLE [FileTag] "
	"WITH CHECK ADD CONSTRAINT [FK_FileTag_FamFile] FOREIGN KEY([FileID]) "
	"REFERENCES [FAMFile] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_FILE_TAG_TAG_ID_FK_80 =
	"ALTER TABLE [FileTag] "
	"WITH CHECK ADD CONSTRAINT [FK_FileTag_Tag] FOREIGN KEY([TagID]) "
	"REFERENCES [Tag] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_LOCKED_FILE_ACTION_FK_80 = 
	"ALTER TABLE [dbo].[LockedFile]  "
	"WITH CHECK ADD  CONSTRAINT [FK_LockedFile_Action] FOREIGN KEY([ActionID])"
	"REFERENCES [dbo].[Action] ([ID])"
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_LOCKED_FILE_ACTION_STATE_FK_80 =
	"ALTER TABLE [dbo].[LockedFile]  "
	"WITH CHECK ADD  CONSTRAINT [FK_LockedFile_ActionState] FOREIGN KEY([StatusBeforeLock])"
	"REFERENCES [dbo].[ActionState] ([Code])"
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_LOCKED_FILE_FAMFILE_FK_80 = 
	"ALTER TABLE [dbo].[LockedFile]  "
	"WITH CHECK ADD  CONSTRAINT [FK_LockedFile_FAMFile] FOREIGN KEY([FileID])"
	"REFERENCES [dbo].[FAMFile] ([ID])"
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_LOCKED_FILE_PROCESSINGFAM_FK_80 =
	"ALTER TABLE [dbo].[LockedFile]  "
	"WITH CHECK ADD  CONSTRAINT [FK_LockedFile_ProcessingFAM] FOREIGN KEY([UPIID])"
	"REFERENCES [dbo].[ProcessingFAM] ([ID])"
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_FAM_SESSION_MACHINE_FK_80 =
	"ALTER TABLE [dbo].[FAMSession] "
	"WITH CHECK ADD CONSTRAINT [FK_FAMSession_Machine] FOREIGN KEY([MachineID]) "
	"REFERENCES [dbo].[Machine]([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_FAM_SESSION_FAMUSER_FK_80 =
	"ALTER TABLE [dbo].[FAMSession] "
	"WITH CHECK ADD CONSTRAINT [FK_FAMSession_User] FOREIGN KEY([FAMUserID]) "
	"REFERENCES [dbo].[FAMUser]([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_FAM_SESSION_FPSFILE_FK_80 =
	"ALTER TABLE [dbo].[FAMSession] "
	"WITH CHECK ADD CONSTRAINT [FK_FAMSession_FPSFile] FOREIGN KEY([FPSFileID]) "
	"REFERENCES [dbo].[FPSFile]([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_INPUT_EVENT_ACTION_FK_80 =
	"ALTER TABLE [dbo].[InputEvent] "
	"WITH CHECK ADD CONSTRAINT [FK_InputEvent_Action] FOREIGN KEY([ActionID]) "
	"REFERENCES [dbo].[Action]([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_INPUT_EVENT_MACHINE_FK_80 =
	"ALTER TABLE [dbo].[InputEvent] "
	"WITH CHECK ADD CONSTRAINT [FK_InputEvent_Machine] FOREIGN KEY([MachineID]) "
	"REFERENCES [dbo].[Machine]([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_INPUT_EVENT_FAMUSER_FK_80 =
	"ALTER TABLE [dbo].[InputEvent] "
	"WITH CHECK ADD CONSTRAINT [FK_InputEvent_User] FOREIGN KEY([FAMUserID]) "
	"REFERENCES [dbo].[FAMUser]([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";
