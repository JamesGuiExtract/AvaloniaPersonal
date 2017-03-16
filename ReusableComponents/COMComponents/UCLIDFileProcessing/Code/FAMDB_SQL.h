#pragma once

#include <FAMUtilsConstants.h>

#include <string>

using namespace std;

// User ID and Machine ID query constants
static const string gstrUSER_ID_VAR = "<UserID>";
static const string gstrMACHINE_ID_VAR = "<MachineID>";

// Create Table SQL statements
static const string gstrCREATE_ACTION_TABLE = "CREATE TABLE [dbo].[Action] "
	"([ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Action] PRIMARY KEY CLUSTERED, " 
	"[ASCName] [nvarchar](50) NOT NULL, "
	"[Description] [nvarchar](255) NULL, "
	"[WorkflowID] [INT] NULL, "
	"CONSTRAINT [IX_Action] UNIQUE NONCLUSTERED ([ASCName], [WorkflowID]))";

static const string gstrCREATE_LOCK_TABLE = 
	"CREATE TABLE [dbo].[LockTable]([LockName] [nvarchar](50) NOT NULL CONSTRAINT [PK_LockTable] PRIMARY KEY CLUSTERED,"
	"[UPI] [nvarchar](512), "
	"[LockTime] datetime NOT NULL CONSTRAINT [DF_LockTable_LockTime]  DEFAULT (GETUTCDATE()))";

static const string gstrCREATE_DB_INFO_TABLE = 
	"CREATE TABLE [dbo].[DBInfo]([ID] int IDENTITY(1,1) NOT NULL, "
	"[Name] [nvarchar](50) NOT NULL CONSTRAINT [PK_DB_INFO] PRIMARY KEY CLUSTERED, "
	"[Value] [nvarchar](max))";

static const string gstrCREATE_ACTION_STATE_TABLE = "CREATE TABLE [dbo].[ActionState]([Code] [nvarchar](1) NOT NULL "
	"CONSTRAINT [PK_ActionState] PRIMARY KEY CLUSTERED,"
	"[Meaning] [nvarchar](255) NULL)";

static const string gstrCREATE_FAM_FILE_TABLE = "CREATE TABLE [dbo].[FAMFile]("
	"[ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_File] PRIMARY KEY CLUSTERED,"
	"[FileName] [nvarchar](255) NULL,"
	"[FileSize] [bigint] NOT NULL CONSTRAINT [DF_FAMFile_FileSize]  DEFAULT ((0)),"
	"[Pages] [int] NOT NULL CONSTRAINT [DF_FAMFile_Pages]  DEFAULT ((0)),"
	"[Priority] [int] NOT NULL CONSTRAINT [DF_FAMFile_Priority] DEFAULT((3)))";

static const string gstrCREATE_QUEUE_EVENT_CODE_TABLE = "CREATE TABLE [dbo].[QueueEventCode]("
	"[Code] [nvarchar](1) NOT NULL CONSTRAINT [PK_QueueEventCode] PRIMARY KEY CLUSTERED ,"
	"[Description] [nvarchar](255) NULL)";

static const string gstrCREATE_ACTION_STATISTICS_TABLE = "CREATE TABLE [dbo].[ActionStatistics]("
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

static const string gstrCREATE_ACTION_STATISTICS_DELTA_TABLE = "CREATE TABLE [dbo].[ActionStatisticsDelta]("
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

static const string gstrCREATE_FILE_ACTION_STATE_TRANSITION_TABLE  ="CREATE TABLE [dbo].[FileActionStateTransition]("
	"[ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_FileActionStateTransition] PRIMARY KEY CLUSTERED,"
	"[FileID] [int] NULL,"
	"[ActionID] [int] NULL,"
	"[ASC_From] [nvarchar](1) NULL,"
	"[ASC_To] [nvarchar](1) NULL,"
	"[DateTimeStamp] [datetime] NULL,"
	"[MachineID] [int] NULL, "
	"[FAMUserID] [int] NULL, "
	"[Exception] [ntext] NULL, "
	"[Comment] [nvarchar](50) NULL, "
	"[QueueID] [int] NULL)";

static const string gstrCREATE_QUEUE_EVENT_TABLE = "CREATE TABLE [dbo].[QueueEvent]("
	"[ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_QueueEvent] PRIMARY KEY CLUSTERED,"
	"[FileID] [int] NULL,"
	"[ActionID] [int] NULL,"
	"[DateTimeStamp] [datetime] NULL,"
	"[QueueEventCode] [nvarchar](1) NULL,"
	"[FileModifyTime] [datetime] NULL,"
	"[FileSizeInBytes] [bigint] NULL,"
	"[MachineID] [int] NULL, "
	"[FAMUserID] [int] NULL)";

static const string gstrCREATE_LOGIN_TABLE = "CREATE TABLE [dbo].[Login]("
	"[ID] [int] IDENTITY(1,1) NOT NULL, "
	"[UserName] [nvarchar](50) NOT NULL, "
	"[Password] [nvarchar](128) NOT NULL DEFAULT(''), "
	"CONSTRAINT [PK_LoginID] PRIMARY KEY CLUSTERED ( [ID] ASC ))";

static const string gstrCREATE_MACHINE_TABLE = "CREATE TABLE [dbo].[Machine]("
	"[ID] [int] IDENTITY(1,1) NOT NULL, "
	"[MachineName] [nvarchar](50) NULL, "
	"CONSTRAINT [PK_Machine] PRIMARY KEY CLUSTERED ([ID] ASC), "
	"CONSTRAINT [IX_MachineName] UNIQUE NONCLUSTERED ([MachineName]))";

static const string gstrCREATE_FAM_USER_TABLE = "CREATE TABLE [dbo].[FAMUser]("
	"[ID] [int] IDENTITY(1,1) NOT NULL, "
	"[UserName] [nvarchar](50) NULL, "
	"CONSTRAINT [PK_FAMUser] PRIMARY KEY CLUSTERED ([ID] ASC), "
	"CONSTRAINT [IX_UserName] UNIQUE NONCLUSTERED ([UserName] ASC))";

static const string gstrCREATE_FAM_FILE_ACTION_COMMENT_TABLE = "CREATE TABLE [dbo].[FileActionComment] ("
	"[ID] [int] IDENTITY(1,1) NOT NULL, "
	"[UserName] [nvarchar](50) NULL, "
	"[FileID] [int] NULL, "
	"[ActionID] [int] NULL, "
	"[Comment] [ntext] NULL, "
	"[DateTimeStamp] [datetime] NOT NULL CONSTRAINT [DF_FileActionComment_DateTimeStamp] DEFAULT((GETDATE())), "
	"CONSTRAINT [PK_FAMFileActionComment] PRIMARY KEY CLUSTERED ([ID] ASC))";

static const string gstrCREATE_FAM_SKIPPED_FILE_TABLE = "CREATE TABLE [dbo].[SkippedFile] ("
	"[ID] [int] IDENTITY(1,1) NOT NULL, "
	"[UserName] [nvarchar](50) NULL, "
	"[FileID] [int] NULL, "
	"[ActionID] [int] NULL, "
	"[DateTimeStamp] [datetime] NOT NULL CONSTRAINT [DF_SkippedFile_DateTimeStamp] DEFAULT((GETDATE())), "
	"[TimeSinceSkipped] AS (DATEDIFF(second,[DateTimeStamp],GETDATE())), " // Computed column for time skipped
	"[FAMSessionID] [int] NULL, "
	"CONSTRAINT [PK_FAMSkippedFile] PRIMARY KEY CLUSTERED ([ID] ASC))";

static const string gstrCREATE_FAM_TAG_TABLE = "CREATE TABLE [dbo].[Tag] ("
	"[ID] [int] IDENTITY(1,1) NOT NULL, "
	"[TagName] [nvarchar](100) NOT NULL, "
	"[TagDescription] [nvarchar](255) NULL, "
	"CONSTRAINT [PK_FAMTag] PRIMARY KEY CLUSTERED ([ID] ASC), "
	"CONSTRAINT [IX_TagName] UNIQUE NONCLUSTERED ([TagName] ASC))";

static const string gstrCREATE_FAM_FILE_TAG_TABLE = "CREATE TABLE [dbo].[FileTag] ("
	"[FileID] [int] NOT NULL, "
	"[TagID] [int] NOT NULL)";

static const string gstrCREATE_ACTIVE_FAM_TABLE = 
	"CREATE TABLE [dbo].[ActiveFAM]([ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_ActiveFAM] PRIMARY KEY CLUSTERED, "
	"[LastPingTime] datetime NOT NULL CONSTRAINT [DF_ActiveFAM_LastPingTime]  DEFAULT (GETUTCDATE()),"
	"[FAMSessionID] [int] NOT NULL)";

static const string gstrCREATE_LOCKED_FILE_TABLE = 
	"CREATE TABLE [dbo].[LockedFile]([FileID] [int] NOT NULL,"
	"[ActionID] [int] NOT NULL, "
	"[StatusBeforeLock] [nvarchar](1) NOT NULL, "
	"[ActiveFAMID] [int] NOT NULL, "
	"CONSTRAINT [PK_LockedFile] PRIMARY KEY CLUSTERED ([FileID], [ActionID], [ActiveFAMID]))";

static const string gstrCREATE_USER_CREATED_COUNTER_TABLE =
	"CREATE TABLE [dbo].[UserCreatedCounter] ("
	"[CounterName] [nvarchar](50) NOT NULL CONSTRAINT [PK_UserCreatedCounter] PRIMARY KEY CLUSTERED,"
	"[Value] [bigint] NOT NULL CONSTRAINT [DF_UserCreatedCounter_Value] DEFAULT((0)))";

static const string gstrCREATE_FPS_FILE_TABLE =
	"CREATE TABLE [dbo].[FPSFile] ("
	"[ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_FPSFile] PRIMARY KEY CLUSTERED, "
	"[FPSFileName] [nvarchar](512) NOT NULL)";

static const string gstrCREATE_FAM_SESSION =
	"CREATE TABLE [dbo].[FAMSession] ("
	"[ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_FAMSession] PRIMARY KEY CLUSTERED, "
	"[MachineID] [int] NOT NULL, "
	"[FAMUserID] [int] NOT NULL, "
	"[UPI] [nvarchar](450), "
	"[StartTime] [datetime] NOT NULL CONSTRAINT [DF_FAMSession_StartTime] DEFAULT((GETDATE())), "
	"[StopTime] [datetime], "
	"[FPSFileID] [int], "
	"[ActionID] [int], "
	"[Queuing] [bit],"
	"[Processing] [bit])";

static const string gstrCREATE_INPUT_EVENT =
	"CREATE TABLE [dbo].[InputEvent] ("
	"[ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_InputEvent] PRIMARY KEY CLUSTERED, "
	"[TimeStamp] [DateTime] NOT NULL, "
	"[ActionID] int NOT NULL, "
	"[FAMUserID] int NOT NULL, "
	"[MachineID] int NOT NULL, "
	"[PID] int NOT NULL, "
	"[SecondsWithInputEvents] int NOT NULL)";

static const string gstrCREATE_FILE_ACTION_STATUS = 
	"CREATE TABLE [dbo].[FileActionStatus]( "
	"[ActionID] [int] NOT NULL, "
	"[FileID] [int] NOT NULL, "
	"[ActionStatus] [nvarchar](1) NOT NULL, "
	"[Priority] [int] NOT NULL, \r\n"
	"CONSTRAINT [PK_FileActionStatus] PRIMARY KEY CLUSTERED "
	"( "
	"	[FileID] ASC, "
	"	[ActionID] ASC "
	")) ";

static const string gstrCREATE_SOURCE_DOC_CHANGE_HISTORY =
	"CREATE TABLE [dbo].[SourceDocChangeHistory]( "
	"[ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_SourceDocChangeHistory] PRIMARY KEY CLUSTERED, "
	"[FileID] [int] NOT NULL, "
	"[FromFileName]  [nvarchar](255) NULL,"
	"[ToFileName]  [nvarchar](255) NULL,"
	"[TimeStamp] [DateTime] NOT NULL, "
	"[FAMUserID] int NOT NULL, "
	"[MachineID] int NOT NULL) ";

static const string gstrCREATE_DOC_TAG_HISTORY_TABLE =
	"CREATE TABLE [dbo].[DocTagHistory]( "
	"[ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_DocTagHistory] PRIMARY KEY CLUSTERED, "
	"[FileID] [int] NOT NULL, "
	"[TagID] [int] NOT NULL, "
	"[Tagged] [bit] NOT NULL,"
	"[TimeStamp] [DateTime] NOT NULL, "
	"[FAMUserID] int NOT NULL, "
	"[MachineID] int NOT NULL) ";

static const string gstrCREATE_DB_INFO_CHANGE_HISTORY_TABLE =
	"CREATE TABLE [dbo].[DBInfoChangeHistory]( "
	"[ID] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_DBInfoHistory] PRIMARY KEY CLUSTERED, "
	"[FAMUserID] INT NOT NULL, "
	"[MachineID] INT NOT NULL, "
	"[DBInfoID] INT NOT NULL, "
	"[OldValue] NVARCHAR(MAX), "
	"[NewValue] NVARCHAR(MAX), "
	"[TimeStamp] DATETIME NOT NULL DEFAULT(GETDATE()))";

static const string gstrCREATE_FTP_ACCOUNT  ="CREATE TABLE [dbo].[FTPAccount]("
	"[ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_FTPAccount] PRIMARY KEY CLUSTERED, "
	"[ServerAddress] [nvarchar](128) NULL, "
	"[UserName] [nvarchar](50) NOT NULL, "
	"CONSTRAINT [IX_FTP_ACCOUNT] UNIQUE NONCLUSTERED ([ServerAddress], [UserName]))";

static const string gstrCREATE_FTP_EVENT_HISTORY_TABLE  ="CREATE TABLE [dbo].[FTPEventHistory]("
	"[ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_FTPEventHistory] PRIMARY KEY CLUSTERED,"
	"[FileID] [int] NULL,"
	"[ActionID] [int] NULL,"
	"[DateTimeStamp] [datetime] NOT NULL CONSTRAINT [DF_FTPEventHistory_DateTimeStamp] DEFAULT((GETDATE())), "
	"[QueueOrProcess] [nvarchar](1) NULL,"
	"[FTPAction] [nvarchar](1) NOT NULL,"
	"[FTPAccountID] [int] NOT NULL,"
	"[Arg1] [nvarchar](255) NOT NULL,"
	"[Arg2] [nvarchar](255) NULL,"
	"[MachineID] [int] NULL, "
	"[FAMUserID] [int] NULL, "
	"[Retries] [int] NULL, "
	"[Exception] [ntext] NULL)";

static const string gstrCREATE_QUEUED_ACTION_STATUS_CHANGE_TABLE =
	"CREATE TABLE [dbo].[QueuedActionStatusChange]("
	"[ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_QueuedActionStatusChange] PRIMARY KEY CLUSTERED, "
	"[FileID] [int] NULL, "
	"[ActionID] [int] NULL, "
	"[ASC_To] [nvarchar](1) NOT NULL, "
	"[DateTimeStamp] [datetime] NULL,"
	"[MachineID] int NOT NULL, "
	"[FAMUserID] int NOT NULL, "
	"[ChangeStatus][nvarchar](1) NOT NULL, "
	"[FAMSessionID] int NULL)";

static const string gstrCREATE_FIELD_SEARCH_TABLE =
	"CREATE TABLE [dbo].[FieldSearch]("
	"[ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_FieldSearch] PRIMARY KEY CLUSTERED,"
	"[Enabled] [bit] NOT NULL DEFAULT 1,"
	"[FieldName] [nvarchar](64) NOT NULL CONSTRAINT [IX_FieldSearch_FieldName] UNIQUE,"
	"[AttributeQuery] [nvarchar](256) NOT NULL)";

// Was LaunchApp in versions 114 and 115
static const string gstrCREATE_FILE_HANDLER_TABLE =
	"CREATE TABLE [dbo].[FileHandler]("
	"[ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_LaunchApp] PRIMARY KEY CLUSTERED,"
	"[Enabled] [bit] NOT NULL DEFAULT 1,"
	"[AppName] [nvarchar](64) NOT NULL CONSTRAINT [IX_FileHandler_AppName] UNIQUE,"
	"[IconPath] [nvarchar](260),"
	"[ApplicationPath] [nvarchar](260) NOT NULL,"
	"[Arguments] [ntext],"
	"[AdminOnly] [bit] NOT NULL DEFAULT 0,"
	"[AllowMultipleFiles] [bit] NOT NULL DEFAULT 0,"
	"[SupportsErrorHandling] [bit] NOT NULL DEFAULT 0,"
	"[Blocking] [bit] NOT NULL DEFAULT 1)";

static const string gstrCREATE_FEATURE_TABLE =
	"CREATE TABLE [dbo].[Feature]("
	"[ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Feature] PRIMARY KEY CLUSTERED,"
	"[Enabled] [bit] NOT NULL DEFAULT 1,"
	"[FeatureName] [nvarchar](64) NOT NULL CONSTRAINT [IX_Feature_FeatureName] UNIQUE,"
	"[FeatureDescription] [nvarchar](max),"
	"[AdminOnly] [bit] NOT NULL DEFAULT 1)";

static const string gstrCREATE_WORK_ITEM_GROUP_TABLE =
	"CREATE TABLE [dbo].[WorkItemGroup]("
	"[ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_WorkItemGroup] PRIMARY KEY CLUSTERED,"
	"[FileID] [int] NOT NULL,"
	"[ActionID] [int] NOT NULL,"
	"[StringizedSettings] [nvarchar](MAX) NULL,"
	"[NumberOfWorkItems] [int] NOT NULL, "
	"[RunningTaskDescription] [nvarchar](256) NULL, "
	"[FAMSessionID] [int] NULL)";

static const string gstrCREATE_WORK_ITEM_TABLE =
	"CREATE TABLE [dbo].[WorkItem]("
	"[ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_WorkItem] PRIMARY KEY CLUSTERED ,"
	"[WorkItemGroupID] [int] NOT NULL,"
	"[Status] [nchar](1) NOT NULL,"
	"[Input] [nvarchar](MAX) NULL,"
	"[BinaryInput] [varbinary](MAX) NULL,"
	"[Output] [nvarchar](MAX) NULL,"
	"[BinaryOutput] [varbinary](MAX) NULL,"
	"[Sequence] [int] NOT NULL,"
	"[StringizedException] [nvarchar](MAX) NULL, "
	"[FAMSessionID] [int] NULL)";

static const string gstrCREATE_METADATA_FIELD_TABLE =
	"CREATE TABLE [dbo].[MetadataField] ("
	"[ID] INT NOT NULL IDENTITY(1,1) CONSTRAINT [PK_MetadataField] PRIMARY KEY CLUSTERED, "
	"[Name] NVARCHAR(50) NOT NULL,"
	"CONSTRAINT [IX_MetadataFieldName] UNIQUE NONCLUSTERED ([Name] ASC))";

static const string gstrCREATE_FILE_METADATA_FIELD_VALUE_TABLE =
	"CREATE TABLE [dbo].[FileMetadataFieldValue] ("
	"[ID] INT NOT NULL IDENTITY(1,1) CONSTRAINT [PK_FileMetadataFieldValue] PRIMARY KEY CLUSTERED, "
	"[FileID] INT NOT NULL, "
	"[MetadataFieldID] INT NOT NULL, "
	"[Value] NVARCHAR(400))";

static const string gstrCREATE_TASK_CLASS = 
	"CREATE TABLE [dbo].[TaskClass]( "
	" [ID] [INT] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_TaskClass] PRIMARY KEY CLUSTERED, "
	" [GUID] [UNIQUEIDENTIFIER] NOT NULL, "
	" [Name] [NVARCHAR](400) NOT NULL "
	"CONSTRAINT [IX_TaskClass_GUID] UNIQUE NONCLUSTERED ([GUID] ASC), "
	"CONSTRAINT [IX_TaskClass_Name] UNIQUE NONCLUSTERED ([Name] ASC))";

static const string gstrCREATE_FILE_TASK_SESSION = 
	"CREATE TABLE [dbo].[FileTaskSession]( "
	" [ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_FileTaskSession] PRIMARY KEY CLUSTERED, "
	" [FAMSessionID] [int] NOT NULL, "
	" [TaskClassID] [int] NOT NULL, "
	" [FileID] [int] NOT NULL, "
	" [DateTimeStamp] [datetime] NULL, "
	" [Duration] [float] NULL, "
	" [OverheadTime] [float] NULL)";

static const string gstrCREATE_SECURE_COUNTER =
	"CREATE TABLE dbo.[SecureCounter] ( "
	"   [ID] int NOT NULL CONSTRAINT [PK_SecureCounter] PRIMARY KEY CLUSTERED, "
	"   [CounterName] nvarchar(100) NOT NULL, "
	"   [SecureCounterValue] nvarchar(max) NOT NULL, "
	"	[AlertLevel] int NOT NULL CONSTRAINT [DF_SecureCounter_AlertLevel] DEFAULT(0), "
	"	[AlertMultiple] int NOT NULL CONSTRAINT [DF_SecureCounter_AlertMultiple] DEFAULT(0))";

static const string gstrCREATE_SECURE_COUNTER_VALUE_CHANGE = 
	"CREATE TABLE dbo.SecureCounterValueChange ( "
	"  ID int IDENTITY(1,1)  CONSTRAINT PK_SecureCounterValueChange PRIMARY KEY CLUSTERED, "
	"  CounterID int NOT NULL, "
	"  FromValue int NOT NULL, "
	"  ToValue int NOT NULL, "
	"  LastUpdatedTime datetimeoffset NOT NULL, "
	"  LastUpdatedByFAMSessionID int NULL, "
	"  MinFAMFileCount bigint NOT NULL, "
	"  HashValue bigint NOT NULL, "
	"  Comment nvarchar(max)) ";

static const string gstrCREATE_PAGINATION =
	"CREATE TABLE [dbo].[Pagination] ( "
	"	[ID] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Pagination] PRIMARY KEY CLUSTERED, "
	"	[SourceFileID] INT NOT NULL, "
	"	[SourcePage] INT NOT NULL, "
	"	[DestFileID] INT NULL, "
	"	[DestPage] INT NULL, "
	"	[OriginalFileID] INT NOT NULL, "
	"	[OriginalPage] INT NOT NULL, "
	"	[FileTaskSessionID] INT NOT NULL)";

static const string gstrCREATE_WORKFLOW_TYPE =
	"CREATE TABLE [dbo].[WorkflowType]( "
	"	[Code] NVARCHAR(1) NOT NULL CONSTRAINT [PK_WorkflowType] PRIMARY KEY CLUSTERED, "
	"	[Meaning] NVARCHAR(100) NOT NULL)";

static const string gstrCREATE_WORKFLOW =
	"CREATE TABLE [dbo].[Workflow]( "
	"	[ID] INT IDENTITY(1, 1) NOT NULL CONSTRAINT [PK_Workflow] PRIMARY KEY CLUSTERED, "
	"	[Name] NVARCHAR(100), "
	"	[WorkflowTypeCode] NVARCHAR(1), "
	"	[Description] NVARCHAR(MAX), "
	"	[StartActionID] INT, "
	"	[EndActionID] INT, "
	"	[PostWorkflowActionID] INT, "
	"	[DocumentFolder] NVARCHAR(255), "
	"	[OutputAttributeSetID] BIGINT, "
	"	[OutputFileMetadataFieldID] INT, "
	"	CONSTRAINT [IX_WorkflowName] UNIQUE NONCLUSTERED ([Name]))";

// Create table indexes SQL
static const string gstrCREATE_DB_INFO_ID_INDEX = "CREATE UNIQUE NONCLUSTERED INDEX [IX_DBInfo_ID] "
	"ON [DBInfo]([ID])";

static const string gstrCREATE_FAM_FILE_INDEX = "CREATE UNIQUE NONCLUSTERED INDEX [IX_Files_FileName] "
	"ON [FAMFile]([FileName] ASC)";

static const string gstrCREATE_QUEUE_EVENT_INDEX = "CREATE NONCLUSTERED INDEX [IX_FileID] "
	"ON [QueueEvent]([FileID])";

static const string gstrCREATE_FILE_ACTION_COMMENT_INDEX = "CREATE UNIQUE NONCLUSTERED INDEX "
	"[IX_File_Action_Comment] ON [FileActionComment]([FileID], [ActionID])";

static const string gstrCREATE_SKIPPED_FILE_INDEX = "CREATE UNIQUE NONCLUSTERED INDEX "
	"[IX_Skipped_File] ON [SkippedFile]([FileID], [ActionID])";

static const string gstrCREATE_SKIPPED_FILE_FAM_SESSION_INDEX = "CREATE NONCLUSTERED INDEX "
	"[IX_Skipped_File_FAMSession] ON [SkippedFile]([FAMSessionID])";

static const string gstrCREATE_FILE_TAG_INDEX = "CREATE UNIQUE NONCLUSTERED INDEX "
	"[IX_File_Tag] ON [FileTag]([FileID], [TagID])";

static const string gstrCREATE_ACTIVE_FAM_SESSION_INDEX = "CREATE UNIQUE NONCLUSTERED INDEX "
	"[IX_ActiveFAM_FAMSession] ON [ActiveFAM]([FAMSessionID])";

static const string gstrCREATE_USER_CREATED_COUNTER_VALUE_INDEX = "CREATE NONCLUSTERED INDEX "
	"[IX_UserCreatedCounter_Value] ON [UserCreatedCounter]([Value])";

static const string gstrCREATE_FPS_FILE_NAME_INDEX = "CREATE NONCLUSTERED INDEX "
	"[IX_FPSFile_FPSFileName] ON [FPSFile]([FPSFileName])";

static const string gstrCREATE_INPUT_EVENT_INDEX = "CREATE UNIQUE NONCLUSTERED INDEX "
	"[IX_Input_Event] ON [InputEvent]([TimeStamp], [ActionID], [MachineID], [FAMUserID], [PID])";

static const string gstrCREATE_FILE_ACTION_STATUS_ALL_INDEX = 
	"CREATE UNIQUE NONCLUSTERED INDEX "
	"[IX_FileActionStatus_All] ON [dbo].[FileActionStatus] "
	"([ActionID] ASC, [ActionStatus] ASC, [Priority] DESC, [FileID] ASC)";

static const string gstrCREATE_ACTION_STATISTICS_DELTA_ACTIONID_ID_INDEX =
	"CREATE UNIQUE NONCLUSTERED INDEX "
	"[IX_ActionStatisticsDeltaActionID_ID] ON [dbo].[ActionStatisticsDelta] "
	"([ActionID] ASC, [ID] ASC)";

static const string gstrCREATE_QUEUED_ACTION_STATUS_CHANGE_INDEX =
	"CREATE NONCLUSTERED INDEX "
	"[IX_QueuedActionStatusChange] ON [QueuedActionStatusChange]([ChangeStatus], [ActionID], [FileID])";

static const string gstrCREATE_WORK_ITEM_GROUP_FAM_SESSION_INDEX =
	"CREATE NONCLUSTERED INDEX "
	"[IX_WorkItemGroup_FAMSession] ON [WorkItemGroup]([FAMSessionID])";

static const string gstrCREATE_WORK_ITEM_FAM_SESSION_INDEX =
	"CREATE NONCLUSTERED INDEX "
	"[IX_WorkItem_FAMSession] ON [WorkItem]([FAMSessionID])";

static const string gstrCREATE_WORK_ITEM_STATUS_INDEX =
	"CREATE NONCLUSTERED INDEX "
	"[IX_WorkItemStatus] ON [WorkItem]([Status])";

static const string gstrCREATE_WORK_ITEM_ID_STATUS_INDEX = 
	"CREATE NONCLUSTERED INDEX [IX_WorkItemStatusID] ON [dbo].[WorkItem]"
	"([ID] ASC,	[Status] ASC)";

static const string gstrMETADATA_FIELD_VALUE_INDEX = "CREATE UNIQUE NONCLUSTERED INDEX "
	"[IX_FileMetadataFieldValue] ON [FileMetadataFieldValue]([FileID], [MetadataFieldID])";

static const string gstrMETADATA_FIELD_VALUE_VALUE_INDEX = 
	"CREATE NONCLUSTERED INDEX "
	"[IX_FileMetadataFieldValue_Value] ON [dbo].[FileMetadataFieldValue] ( "
	"[MetadataFieldID] ASC,  [Value] ASC ) INCLUDE ([FileID]) ";

static const string gstrCREATE_FAST_ACTIONID_INDEX = 
	"CREATE NONCLUSTERED INDEX IX_FileActionStateTransition_ActionID "
	"ON [dbo].[FileActionStateTransition] ([ActionID]) "
	"INCLUDE ([ID],[FileID])";

static const string gstrCREATE_FAST_FILEID_ACTIONID_INDEX = 
	"CREATE NONCLUSTERED INDEX IX_FAST_FILEID_ACTIONID "
	"ON [dbo].[FileActionStateTransition] ([FileID],[ActionID]) "
	"INCLUDE ([ID]) ";

static const string gstrCREATE_FILE_TASK_SESSION_DATETIMESTAMP_INDEX = 
	"CREATE NONCLUSTERED INDEX [IX_FileTaskSession_DateTimeStamp] ON [dbo].[FileTaskSession] "
	"([FileID] ASC, [DateTimeStamp] ASC)";

static const string gstrCREATE_FILE_TASK_SESSION_FAMSESSION_INDEX = 
	"CREATE NONCLUSTERED INDEX [IX_FileTaskSession_FAMSession] ON [dbo].[FileTaskSession] "
	"([FAMSessionID] ASC)";

static const string gstrCREATE_PAGINATION_ORIGINALFILE_INDEX =
	"CREATE NONCLUSTERED INDEX [IX_Pagination_OriginalFile] ON "
	"	[dbo].[Pagination] ([OriginalFileID])";

static const string gstrCREATE_PAGINATION_FILETASKSESSION_INDEX = 
	"CREATE NONCLUSTERED INDEX [IX_Pagination_FileTaskSession] ON "
	"	[dbo].[Pagination] ([FileTaskSessionID])";

// Add foreign keys SQL
static const string gstrADD_ACTION_WORKFLOW_FK =
	"ALTER TABLE dbo.[Action] "
	"WITH CHECK ADD CONSTRAINT [FK_Action_Workflow] FOREIGN KEY([WorkflowID]) "
	"REFERENCES [Workflow]([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

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

static const string gstrADD_FILE_ACTION_STATE_TRANSITION_QUEUE_FK = 
	"ALTER TABLE [dbo].[FileActionStateTransition] "
	"WITH CHECK ADD CONSTRAINT [FK_FileActionStateTransition_Queue] FOREIGN KEY([QueueID])"
	"REFERENCES [QueuedActionStatusChange] ([ID])";

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

// https://extract.atlassian.net/browse/ISSUE-13223
// Deletes cannot be cascaded until ActionID is factored out of the table.
static const string gstrADD_SKIPPED_FILE_FAM_SESSION_FK =
	"ALTER TABLE [dbo].[SkippedFile] "
	"WITH CHECK ADD CONSTRAINT [FK_SkippedFile_FAMSession] FOREIGN KEY([FAMSessionID])"
	"REFERENCES [dbo].[FAMSession] ([ID])";

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

static const string gstrADD_LOCKED_FILE_ACTIVEFAM_FK =
	"ALTER TABLE [dbo].[LockedFile]  "
	"WITH CHECK ADD  CONSTRAINT [FK_LockedFile_ActiveFAM] FOREIGN KEY([ActiveFAMID])"
	"REFERENCES [dbo].[ActiveFAM] ([ID])"
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_ACTIVEFAM_FAM_SESSION_FK =
	"ALTER TABLE [dbo].[ActiveFAM]  "
	"WITH CHECK ADD  CONSTRAINT [FK_ActiveFAM_FAMSession] FOREIGN KEY([FAMSessionID])"
	"REFERENCES [dbo].[FAMSession] ([ID])";

// For version 135, change the FK so that instead of a cascade delete, it
// sets the ActionID to NULL. This allows deleting Actions, which were otherwise
// prevented when a FileTaskSession referenced a FAMSession that in turn referred to
// an Action.
static const string gstrDROP_FAM_SESSION_ACTION_FK = 
	"ALTER TABLE [dbo].[FAMSession]  DROP CONSTRAINT [FK_FAMSession_Action]";

static const string gstrADD_FAM_SESSION_ACTION_FK =
	"ALTER TABLE [dbo].[FAMSession] "
	"WITH CHECK ADD CONSTRAINT [FK_FAMSession_Action] FOREIGN KEY([ActionID])"
	"REFERENCES [dbo].[Action] ([ID])"
	"ON UPDATE CASCADE "
	"ON DELETE SET NULL";

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

static const string gstrADD_FTP_EVENT_HISTORY_FTP_ACCOUNT_FK = 
	"ALTER TABLE [dbo].[FTPEventHistory] "
	"WITH CHECK ADD CONSTRAINT [FK_FTPEventHistory_FTPAccount] FOREIGN KEY([FTPAccountID]) "
	"REFERENCES [dbo].[FTPAccount] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_FTP_EVENT_HISTORY_FAM_FILE_FK = 
	"ALTER TABLE [dbo].[FTPEventHistory] "
	"WITH CHECK ADD CONSTRAINT [FK_FTPEventHistory_FAMFile] FOREIGN KEY([FileID]) "
	"REFERENCES [dbo].[FAMFile] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_FTP_EVENT_HISTORY_ACTION_FK = 
	"ALTER TABLE [dbo].[FTPEventHistory] "
	"WITH CHECK ADD CONSTRAINT [FK_FTPEventHistory_Action] FOREIGN KEY([ActionID]) "
	"REFERENCES [dbo].[Action] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_FTP_EVENT_HISTORY_MACHINE_FK = 
	"ALTER TABLE [dbo].[FTPEventHistory] "
	"WITH CHECK ADD CONSTRAINT [FK_FTPEventHistory_Machine] FOREIGN KEY([MachineID]) "
	"REFERENCES [dbo].[Machine] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_FTP_EVENT_HISTORY_FAM_USER_FK = 
	"ALTER TABLE [dbo].[FTPEventHistory] "
	"WITH CHECK ADD CONSTRAINT [FK_FTPEventHistory_FAMUser] FOREIGN KEY([FAMUserID]) "
	"REFERENCES [dbo].[FAMUser] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_QUEUED_ACTION_STATUS_CHANGE_FAMFILE_FK =
	"ALTER TABLE [dbo].[QueuedActionStatusChange] "
	"WITH CHECK ADD CONSTRAINT [FK_QueuedActionStatusChange_FAMFile] FOREIGN KEY([FileID]) "
	"REFERENCES [dbo].[FAMFile] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_QUEUED_ACTION_STATUS_CHANGE_ACTION_FK =
	"ALTER TABLE [dbo].[QueuedActionStatusChange] "
	"WITH CHECK ADD CONSTRAINT [FK_QueuedActionStatusChange_Action] FOREIGN KEY([ActionID]) "
	"REFERENCES [dbo].[Action] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_QUEUED_ACTION_STATUS_CHANGE_MACHINE_FK =
	"ALTER TABLE [dbo].[QueuedActionStatusChange] "
	"WITH CHECK ADD CONSTRAINT [FK_QueuedActionStatusChange_Machine] FOREIGN KEY([MachineID]) "
	"REFERENCES [dbo].[Machine] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_QUEUED_ACTION_STATUS_CHANGE_USER_FK =
	"ALTER TABLE [dbo].[QueuedActionStatusChange] "
	"WITH CHECK ADD CONSTRAINT [FK_QueuedActionStatusChange_FAMUser] FOREIGN KEY([FAMUserID]) "
	"REFERENCES [dbo].[FAMUser] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

// https://extract.atlassian.net/browse/ISSUE-13223
// Deletes cannot be cascaded until ActionID is factored out of the table.
static const string gstrADD_QUEUED_ACTION_STATUS_CHANGE_FAM_SESSION_FK =
	"ALTER TABLE [dbo].[QueuedActionStatusChange] "
	"WITH CHECK ADD CONSTRAINT [FK_QueuedActionStatusChange_FAMSession] FOREIGN KEY([FAMSessionID]) "
	"REFERENCES [dbo].[FAMSession] ([ID])";

static const string gstrADD_WORK_ITEM_GROUP_FAMFILE_FK = 
	"ALTER TABLE [dbo].[WorkItemGroup]  "
	"WITH CHECK ADD  CONSTRAINT [FK_WorkItemGroup_FAMFile] FOREIGN KEY([FileID])"
	"REFERENCES [dbo].[FAMFile] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_WORK_ITEM_GROUP_ACTION_FK = 
	"ALTER TABLE [dbo].[WorkItemGroup]  "
	"WITH CHECK ADD  CONSTRAINT [FK_WorkItemGroup_Action] FOREIGN KEY([ActionID])"
	"REFERENCES [dbo].[Action] ([ID])"
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

// https://extract.atlassian.net/browse/ISSUE-13223
// Deletes cannot be cascaded until ActionID is factored out of the table.
static const string gstrADD_WORK_ITEM_GROUP_FAM_SESSION_FK = 
	"ALTER TABLE [dbo].[WorkItemGroup]  "
	"WITH CHECK ADD  CONSTRAINT [FK_WorkItemGroup_FAMSession] FOREIGN KEY([FAMSessionID])"
	"REFERENCES [dbo].[FAMSession] ([ID])";

static const string gstrADD_WORK_ITEM__WORK_ITEM_GROUP_FK =
	"ALTER TABLE [dbo].[WorkItem]  "
	"WITH CHECK ADD  CONSTRAINT [FK_WorkItem_WorkItemGroup] FOREIGN KEY([WorkItemGroupID])"
	"REFERENCES [dbo].[WorkItemGroup] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

// https://extract.atlassian.net/browse/ISSUE-13223
// Deletes cannot be cascaded until ActionID is factored out of WorkItemGroup
static const string gstrADD_WORK_ITEM_FAM_SESSION_FK =
	"ALTER TABLE [dbo].[WorkItem]  "
	"WITH CHECK ADD  CONSTRAINT [FK_WorkItem_FAMSession] FOREIGN KEY([FAMSessionID])"
	"REFERENCES [dbo].[FAMSession] ([ID]) ";

static const string gstrADD_METADATA_FIELD_VALUE_FAMFILE_FK =
	"ALTER TABLE [FileMetadataFieldValue] "
	"WITH CHECK ADD CONSTRAINT [FK_FileMetadataFieldValue_FAMFile] FOREIGN KEY([FileID]) "
	"REFERENCES [FAMFile] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_METADATA_FIELD_VALUE_METADATA_FIELD_FK =
	"ALTER TABLE [FileMetadataFieldValue] "
	"WITH CHECK ADD CONSTRAINT [FK_FileMetadataFieldValue_MetadataField] FOREIGN KEY([MetadataFieldID]) "
	"REFERENCES [MetadataField] ([ID])  "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_FILE_TASK_SESSION_FAM_SESSION_FK =
	"ALTER TABLE [dbo].[FileTaskSession]  "
	"WITH CHECK ADD  CONSTRAINT [FK_FileTaskSession_FAMSession] FOREIGN KEY([FAMSessionID])"
	"REFERENCES [dbo].[FAMSession] ([ID])";

static const string gstrADD_FILE_TASK_SESSION_TASK_CLASS_FK =
	"ALTER TABLE [dbo].[FileTaskSession]  "
	"WITH CHECK ADD  CONSTRAINT [FK_FileTaskSession_TaskClass] FOREIGN KEY([TaskClassID])"
	"REFERENCES [dbo].[TaskClass] ([ID])";

static const string gstrADD_FILE_TASK_SESSION_FAMFILE_FK =
	"ALTER TABLE [dbo].[FileTaskSession]  "
	"WITH CHECK ADD  CONSTRAINT [FK_FileTaskSession_FAMFile] FOREIGN KEY([FileID])"
	"REFERENCES [dbo].[FAMFile] ([ID])";

static const string gstrADD_SECURE_COUNTER_VALUE_CHANGE_SECURE_COUNTER_FK =
	"ALTER TABLE [dbo].SecureCounterValueChange "
	"WITH CHECK ADD CONSTRAINT FK_SecureCounterValueChange_SecureCounter FOREIGN KEY([CounterID]) "
	"REFERENCES dbo.[SecureCounter] ([ID])"
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_SECURE_COUNTER_VALUE_CHANGE_FAM_SESSION_FK =
	"ALTER TABLE [dbo].SecureCounterValueChange "
	"WITH NOCHECK ADD CONSTRAINT FK_SecureCounterValueChange_FAMSession FOREIGN KEY (LastUpdatedByFAMSessionID) "
	"REFERENCES dbo.[FAMSession] (ID)";

static const string gstrADD_PAGINATION_SOURCEFILE_FAMFILE_FK =
	"ALTER TABLE [dbo].[Pagination] "
	"WITH CHECK ADD CONSTRAINT [FK_Pagination_SourceFile_FAMFile] "
	"FOREIGN KEY (SourceFileID) REFERENCES dbo.[FAMFile] ([ID])";

static const string gstrADD_PAGINATION_DESTFILE_FAMFILE_FK =
	"ALTER TABLE [dbo].[Pagination] "
	"WITH CHECK ADD CONSTRAINT [FK_Pagination_DestFile_FAMFile] "
	"FOREIGN KEY (DestFileID) REFERENCES dbo.[FAMFile] ([ID])";

static const string gstrADD_PAGINATION_ORIGINALFILE_FAMFILE_FK =
	"ALTER TABLE [dbo].[Pagination] "
	"WITH CHECK ADD CONSTRAINT [FK_Pagination_OriginalFile_FAMFile] "
	"FOREIGN KEY (OriginalFileID) REFERENCES dbo.[FAMFile] ([ID])";

static const string gstrADD_PAGINATION_FILETASKSESSION_FK = 
	"ALTER TABLE [dbo].[Pagination]  "
	" WITH CHECK ADD CONSTRAINT [FK_Pagination_FileTaskSession] FOREIGN KEY([FileTaskSessionID]) "
	" REFERENCES [dbo].[FileTaskSession] ([ID])"
	" ON UPDATE CASCADE "
	" ON DELETE CASCADE ";

static const string gstrADD_WORKFLOW_WORKFLOWTYPE_FK =
	"ALTER TABLE [dbo].[Workflow]  "
	" WITH CHECK ADD CONSTRAINT [FK_Workflow_WorkflowType] FOREIGN KEY([WorkflowTypeCode]) "
	" REFERENCES [dbo].[WorkflowType] ([Code])"
	" ON UPDATE CASCADE "
	" ON DELETE CASCADE";

static const string gstrADD_WORKFLOW_STARTACTION_FK =
	"ALTER TABLE [dbo].[Workflow]  "
	" WITH CHECK ADD CONSTRAINT [FK_Workflow_StartAction] FOREIGN KEY([StartActionID]) "
	" REFERENCES [dbo].[Action] ([ID])"
	" ON UPDATE NO ACTION "  // Anything except NO ACTION leads to errors about cascading
	" ON DELETE NO ACTION";  // updates/deletes due to multiple FKs to Action table.

static const string gstrADD_WORKFLOW_ENDACTION_FK =
	"ALTER TABLE [dbo].[Workflow]  "
	" WITH CHECK ADD CONSTRAINT [FK_Workflow_EndAction] FOREIGN KEY([EndActionID]) "
	" REFERENCES [dbo].[Action] ([ID])"
	" ON UPDATE NO ACTION "  // Anything except NO ACTION leads to errors about cascading
	" ON DELETE NO ACTION";  // updates/deletes due to multiple FKs to Action table.

static const string gstrADD_WORKFLOW_POSTWORKFLOWACTION_FK =
	"ALTER TABLE [dbo].[Workflow]  "
	" WITH CHECK ADD CONSTRAINT [FK_Workflow_PostWorkflowAction] FOREIGN KEY([PostWorkflowActionID]) "
	" REFERENCES [dbo].[Action] ([ID])"
	" ON UPDATE NO ACTION "  // Anything except NO ACTION leads to errors about cascading
	" ON DELETE NO ACTION";  // updates/deletes due to multiple FKs to Action table.

// NOTE: Foreign key for OutputAttributeSetID is added in AttributeDBMgr

static const string gstrADD_WORKFLOW_OUTPUTFILEMETADATAFIELD_FK =
	"ALTER TABLE [dbo].[Workflow]  "
	" WITH CHECK ADD CONSTRAINT [FK_Workflow_OutputFileMetadataFieldID] FOREIGN KEY([OutputFileMetadataFieldID]) "
	" REFERENCES [dbo].[MetadataField] ([ID])"
	" ON UPDATE CASCADE " 
	" ON DELETE CASCADE"; 

static const string gstrADD_DB_PROCEXECUTOR_ROLE =
	"IF DATABASE_PRINCIPAL_ID('db_procexecutor') IS NULL \r\n"
	"BEGIN\r\n"
	"	CREATE ROLE db_procexecutor \r\n"
	"	GRANT EXECUTE TO db_procexecutor \r\n"
	"END\r\n";

// Query for obtaining the current db lock record with the time it has been locked
static const string gstrDB_LOCK_NAME_VAL = "<LockName>";
static const string gstrDB_LOCK_QUERY = 
	"SELECT LockName, UPI, LockTime, DATEDIFF(second, LockTime, GETUTCDATE()) AS TimeLocked "
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
// https://extract.atlassian.net/browse/ISSUE-13910
// To allow for the ability to query settings on old DB's where the schema may have changed,
// get all columns rather than a hard-coded list.
static const string gstrDBINFO_SETTING_QUERY = 
	"SELECT * FROM DBInfo WHERE [Name] = '" + gstrSETTING_NAME + "'";

// Query for getting all DB info settings
static const string gstrDBINFO_GET_SETTINGS_QUERY =
	"SELECT [Name], [Value] FROM DBInfo";

// Constant to be replaced in the DBInfo Setting query
static const string gstrSETTING_VALUE = "<SettingValue>";

// Query for updating the DB info settings
static const string gstrDBINFO_UPDATE_SETTINGS_QUERY =
	"UPDATE DBInfo SET [Value] = '" + gstrSETTING_VALUE + "' WHERE [Name] = '"
	+ gstrSETTING_NAME + "' AND [Value] <> '" + gstrSETTING_VALUE + "'";

// Query for inserting a DBInfo setting if it doesn't exist 
static const string gstrDBINFO_INSERT_IF_MISSING_SETTINGS_QUERY =
	"DECLARE @CurrentValue as NVARCHAR(MAX) "
	"SELECT @CurrentValue = Value FROM DBInfo WHERE Name = '" + gstrSETTING_NAME + "' "
	"IF (@CurrentValue IS NULL) BEGIN "
	"	INSERT INTO DBINFO ([Name], [Value]) "
	"	VALUES ('" + gstrSETTING_NAME + "', '" + gstrSETTING_VALUE + "') "
	"END "; 

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

// Insert that adds Enable email Settings to DBInfo, and sets the value
// to true iff email settings are already establised, false if not.
static const string gstrINSERT_EMAIL_ENABLE_SETTINGS_WITH_VALUE = 
"DECLARE @boolText nvarchar(50)\r\n"
"DECLARE @ServerName nvarchar(max)\r\n"
"Set @ServerName =\r\n"
"(\r\n"
"	select Value from DBInfo where Name=\'EmailServer\'\r\n"
")\r\n"
"IF @ServerName=\'\'\r\n"
"BEGIN\r\n"
"	SET @boolText=\'0\'\r\n"
"END\r\n"
"ELSE\r\n"
"BEGIN\r\n"
"	SET @boolText=\'1\'\r\n"
"END\r\n"
"INSERT INTO [DBInfo] (Name, Value) VALUES (\'EmailEnableSettings\', @boolText)";


// Query to delete old input event records from the InputEvent table
static const string gstrDELETE_OLD_INPUT_EVENT_RECORDS =
	"DELETE FROM InputEvent WHERE DATEDIFF(d, GETDATE(), [TimeStamp]) > (SELECT COALESCE("
	"(SELECT CAST([Value] AS int) FROM [DBInfo] WHERE [Name] = '"
	+ gstrINPUT_EVENT_HISTORY_SIZE + "'), 30))";

// Query to use to calculate and insert new ActionStatistics records for the ActionIDs when the id
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

// Query to obtain statistics by aggregating all of the data in ActionStatistics and
// ActionStatisticsDelta.
//		<ActionIDToUpdate> Should be replaced with the ActionID for which stats are needed.
static const string gstrCALCULATE_ACTION_STATISTICS_FOR_ACTION =
	"SELECT MAX([ActionStatistics].[NumDocuments]) + COALESCE(SUM([ActionStatisticsDelta].[NumDocuments]), 0) AS [NumDocuments],"
	"      MAX([ActionStatistics].[NumDocumentsPending]) + COALESCE(SUM([ActionStatisticsDelta].[NumDocumentsPending]), 0) AS [NumDocumentsPending],"
	"      MAX([ActionStatistics].[NumDocumentsComplete]) + COALESCE(SUM([ActionStatisticsDelta].[NumDocumentsComplete]), 0) AS [NumDocumentsComplete],"
	"      MAX([ActionStatistics].[NumDocumentsFailed]) + COALESCE(SUM([ActionStatisticsDelta].[NumDocumentsFailed]), 0) AS [NumDocumentsFailed],"
	"      MAX([ActionStatistics].[NumDocumentsSkipped]) + COALESCE(SUM([ActionStatisticsDelta].[NumDocumentsSkipped]), 0) AS [NumDocumentsSkipped],"
	"      MAX([ActionStatistics].[NumPages]) + COALESCE(SUM([ActionStatisticsDelta].[NumPages]), 0) AS [NumPages],"
	"      MAX([ActionStatistics].[NumPagesPending]) + COALESCE(SUM([ActionStatisticsDelta].[NumPagesPending]), 0) AS [NumPagesPending],"
	"      MAX([ActionStatistics].[NumPagesComplete]) + COALESCE(SUM([ActionStatisticsDelta].[NumPagesComplete]), 0) AS [NumPagesComplete],"
	"      MAX([ActionStatistics].[NumPagesFailed]) + COALESCE(SUM([ActionStatisticsDelta].[NumPagesFailed]), 0) AS [NumPagesFailed],"
	"      MAX([ActionStatistics].[NumPagesSkipped]) + COALESCE(SUM([ActionStatisticsDelta].[NumPagesSkipped]), 0) AS [NumPagesSkipped],"
	"      MAX([ActionStatistics].[NumBytes]) + COALESCE(SUM([ActionStatisticsDelta].[NumBytes]), 0) AS [NumBytes],"
	"      MAX([ActionStatistics].[NumBytesPending]) + COALESCE(SUM([ActionStatisticsDelta].[NumBytesPending]), 0) AS [NumBytesPending],"
	"      MAX([ActionStatistics].[NumBytesComplete]) + COALESCE(SUM([ActionStatisticsDelta].[NumBytesComplete]), 0) AS [NumBytesComplete],"
	"      MAX([ActionStatistics].[NumBytesFailed]) + COALESCE(SUM([ActionStatisticsDelta].[NumBytesFailed]), 0) AS [NumBytesFailed],"
	"      MAX([ActionStatistics].[NumBytesSkipped]) + COALESCE(SUM([ActionStatisticsDelta].[NumBytesSkipped]), 0) AS [NumBytesSkipped]"
	"  FROM [ActionStatistics]"
	"  LEFT JOIN [ActionStatisticsDelta] ON [ActionStatistics].[ActionID] = [ActionStatisticsDelta].[ActionID]"
	"  WHERE [ActionStatistics].[ActionID] = <ActionIDWhereClause> "
	"  GROUP BY [ActionStatistics].[ActionID]";

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
//		<ActionIDs> - A comma delimited list of IDs for the actions being processed
//		<UserID> - ID for the files are being processed under
//		<MachineID> - ID for the machine processing the files
//		<ActiveFAMID> - ID of the active FAM session
//		<RecordFASTEntry> = 1 to record an entry in the FAST table for all file set to processing.
static const string gstrGET_FILES_TO_PROCESS_QUERY = 
	"DECLARE @OutputTableVar table ( \r\n"
	"	[ID] [int] NOT NULL, \r\n"
	"	[FileName] [nvarchar](255) NULL, \r\n"
	"	[FileSize] [bigint] NOT NULL, \r\n"
	"	[Pages] [int] NOT NULL, \r\n"
	"	[Priority] [int] NOT NULL, \r\n"
	"	[ActionID] [int] NOT NULL, \r\n"
	"	[ASC_From] [nvarchar](1) NOT NULL \r\n"
	"); \r\n"
	"SET NOCOUNT ON \r\n"
	"BEGIN TRY \r\n"
	"	UPDATE FileActionStatus Set ActionStatus = 'R'  \r\n"
	"	OUTPUT ATABLE.ID, ATABLE.FileName, ATABLE.FileSize, ATABLE.Pages, ATABLE.Priority, ATABLE.ActionId, deleted.ActionStatus INTO @OutputTableVar \r\n"
	"	FROM  \r\n"
	"	( \r\n"
	"		<SelectFilesToProcessQuery> ) AS ATABLE  \r\n"
	"	INNER JOIN FileActionStatus on FileActionStatus.FileID = ATABLE.ID AND FileActionStatus.ActionID IN (<ActionIDs>)  \r\n"
	"	WHERE ATABLE.ActionStatus <> 'R'; \r\n"
	"	IF (1 = <RecordFASTEntry>) BEGIN"
	//	If a file that is currently unattempted is being moved to processing, first add a FAST table
	//	entry from U->P before adding a record from P -> R
	"		INSERT INTO FileActionStateTransition (FileID, ActionID, ASC_From, ASC_To,  \r\n"
	"			DateTimeStamp, FAMUserID, MachineID, Exception, Comment) \r\n"
	"		SELECT id, ActionID, 'U', 'P' as ASC_To, GETDATE() AS DateTimeStamp,  \r\n"
	"			<UserID> as UserID, <MachineID> as MachineID, '' as Exception, '' as Comment FROM @OutputTableVar \r\n"
	"			WHERE ASC_From = 'U'; \r\n"
	"		INSERT INTO FileActionStateTransition (FileID, ActionID,  ASC_From, ASC_To,  \r\n"
	"			DateTimeStamp, FAMUserID, MachineID, Exception, Comment) \r\n"
	"		SELECT id, ActionID, CASE WHEN ASC_From = 'U' THEN 'P' ELSE ASC_From END, 'R' as ASC_To, GETDATE() AS DateTimeStamp,  \r\n"
	"			<UserID> as UserID, <MachineID> as MachineID, '' as Exception, '' as Comment FROM @OutputTableVar \r\n"
	"	END; \r\n"
	"	INSERT INTO LockedFile(FileID,ActionID,ActiveFAMID,StatusBeforeLock) \r\n"
	"		SELECT ID, ActionID, <ActiveFAMID> AS ActiveFAMID, ASC_From AS StatusBeforeLock FROM @OutputTableVar; \r\n"
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
// NOTE: This query ends with " END" to ensure it is nested within whichever query
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

// Variables for gstrRECORD_FTP_EVENT_QUERY
static const string gstrTAG_FTP_SERVERADDRESS_VAR = "<ServerAddress>";
static const string gstrTAG_FTP_USERNAME_VAR = "<UserName>";
static const string gstrTAG_FTP_FILEID_VAR = "<FileID>";
static const string gstrTAG_FTP_ACTIONID_VAR = "<ActionID>";
static const string gstrTAG_FTP_QUEUE_OR_PROCESS_VAR = "<QueueOrProcess>";
static const string gstrTAG_FTP_FTPACTION_VAR = "<FTPAction>";
static const string gstrTAG_FTP_ARG1_VAR = "<Arg1>";
static const string gstrTAG_FTP_ARG2_VAR = "<Arg2>";
static const string gstrTAG_FTP_MACHINEID_VAR = "<MachineID>";
static const string gstrTAG_FTP_USERID_VAR = "<UserID>";
static const string gstrTAG_FTP_RETRIES_VAR = "<Retries>";
static const string gstrTAG_FTP_EXCEPTION_VAR = "<Exception>";

// Logs an FTP event to the FTPEventHistory table.
static const string gstrRECORD_FTP_EVENT_QUERY =
	string("DECLARE @FTPAccountIDTable TABLE(ID INT) \r\n")
	+ "INSERT INTO @FTPAccountIDTable \r\n"
	+ "		SELECT [ID] FROM [dbo].[FTPAccount] \r\n"
	+ "			WHERE [ServerAddress] = '" + gstrTAG_FTP_SERVERADDRESS_VAR + "' \r\n"
	+ "			AND [UserName] = '" + gstrTAG_FTP_USERNAME_VAR + "' \r\n"
	+ "\r\n"
	+ "IF (SELECT COUNT(*) FROM @FTPAccountIDTable) = 0 \r\n"
	+ "		INSERT INTO [dbo].[FTPAccount] ([ServerAddress], [UserName]) \r\n"
	+ "		OUTPUT INSERTED.ID INTO @FTPAccountIDTable \r\n"
	+ "		VALUES ('" + gstrTAG_FTP_SERVERADDRESS_VAR + "', '" + gstrTAG_FTP_USERNAME_VAR + "') \r\n"
	+ "\r\n"
	+ "DECLARE @FTPAccountID INT \r\n"
	+ "SELECT @FTPAccountID = ID FROM @FTPAccountIDTable \r\n"
	+ "\r\n"
	+ "INSERT INTO [dbo].[FTPEventHistory] \r\n"
	+ "		([FileID], [ActionID], [QueueOrProcess], [FTPAction], [FTPAccountID], \r\n"
	+ "		 [Arg1], [Arg2], [MachineID], [FAMUserID], [Retries], [Exception]) \r\n"
	+ "VALUES (" + gstrTAG_FTP_FILEID_VAR + ", " + gstrTAG_FTP_ACTIONID_VAR + ", '"
	+		gstrTAG_FTP_QUEUE_OR_PROCESS_VAR + "', '" + gstrTAG_FTP_FTPACTION_VAR + "', "
	+		"@FTPAccountID, '" + gstrTAG_FTP_ARG1_VAR + "', " + gstrTAG_FTP_ARG2_VAR + ", "
	+		gstrTAG_FTP_MACHINEID_VAR + ", " + gstrTAG_FTP_USERID_VAR + ", "
	+		gstrTAG_FTP_RETRIES_VAR + ", " + gstrTAG_FTP_EXCEPTION_VAR + ")";

// constants for the query to get the total number of files referenced in the database
const string gstrTOTAL_FILECOUNT_FIELD = "FileCount";
// This query executes very fast even on a large DB, but require admin permissions in the database.
const string gstrFAST_TOTAL_FAMFILE_QUERY = "SELECT SUM (row_count) AS " + gstrTOTAL_FILECOUNT_FIELD +
	" FROM sys.dm_db_partition_stats "
	"WHERE object_id=OBJECT_ID('FAMFile') AND (index_id=0 or index_id=1)";
// This query can take some time to run on a large DB, but will work for any database user with read
// permissions.
const string gstrSTANDARD_TOTAL_FAMFILE_QUERY = "SELECT COUNT(*) AS " + gstrTOTAL_FILECOUNT_FIELD +
	" FROM [FAMFile]";
const string gstrSTANDARD_TOTAL_FAMFILE_QUERY_ORACLE = "SELECT COUNT(*) AS \"" + 
	gstrTOTAL_FILECOUNT_FIELD + "\" FROM \"FAMFile\"";
// Queries for all currently enabled features.
const string gstrGET_ENABLED_FEATURES_QUERY = "SELECT [FeatureName], [AdminOnly] FROM [" +
	gstrDB_FEATURE + "] WHERE [Enabled] = 1";


const string gstrGET_WORK_ITEM_TO_PROCESS = 
"		DECLARE @OutputTableVar table ( \r\n"
"		[ID] [int] NOT NULL,\r\n"
"		[WorkItemGroupID] [int] NOT NULL,\r\n"
"		[ActionID] [int] NOT NULL, \r\n"
"		[Status] [nchar](1) NOT NULL,\r\n"
"		[Input] [text] NULL,\r\n"
"		[Output] [text] NULL,\r\n"
"		[FAMSessionID] [int] NULL,\r\n"	
"		[FileName] [nvarchar](255) NULL,\r\n"
"		[StringizedException] [nvarchar](MAX) NULL,\r\n"
"		[BinaryOutput] [varbinary](MAX) NULL,\r\n"
"		[BinaryInput] [varbinary](MAX) NULL, \r\n"
"		[FileID] [int] NULL, \r\n"
"		[WorkGroupFAMSessionID] [int] NULL, \r\n"
"		[Priority] [int] NULL, \r\n"
"		[RunningTaskDescription] [nvarchar](256) NULL \r\n"
"	); \r\n"
"	SET NOCOUNT ON \r\n"
"	BEGIN TRY \r\n"
"		UPDATE [dbo].WorkItem Set Status = 'R', FAMSessionID = <FAMSessionID>  \r\n"
"		OUTPUT DELETED.ID, DELETED.WorkItemGroupID, WorkItemGroup.ActionID, INSERTED.Status, DELETED.[Input], "
"			DELETED.[Output], INSERTED.FAMSessionID, FAMFile.FileName, DELETED.StringizedException, NULL, "
"			DELETED.BinaryInput, FAMFile.ID, WorkItemGroup.FAMSessionID, FileActionStatus.Priority, "
"			WorkItemGroup.RunningTaskDescription INTO @OutputTableVar  \r\n"
"		FROM  WorkItem INNER JOIN WorkItemGroup ON WorkItemGroup.ID = WorkItem.WorkItemGroupID "
"		INNER JOIN FAMFile ON FAMFile.ID = WorkItemGroup.FileID "
"		INNER JOIN FileActionStatus ON FAMFile.ID = FileActionStatus.FileID AND  "
"				FileActionStatus.ActionID = <ActionID> "
"	WHERE WorkItem.ID IN ( "
"		SELECT TOP(<MaxWorkItems>) WorkItem.ID "
"		FROM WorkItem "
"		INNER JOIN WorkItemGroup ON WorkItem.WorkItemGroupID = WorkItemGroup.ID "
"		INNER JOIN FAMFile ON FAMFile.ID = WorkItemGroup.FileID "
"		INNER JOIN FileActionStatus ON FAMFile.ID = FileActionStatus.FileID AND  "
"				FileActionStatus.ActionID = <ActionID> "
"		INNER JOIN ActiveFAM ON ActiveFAM.FAMSessionID = WorkItemGroup.FAMSessionID "
"		WHERE STATUS = 'P' "
"			AND WorkItemGroup.ActionID = <ActionID> "
"			AND ('<GroupFAMSessionID>' = '' OR WorkItemGroup.FAMSessionID = '<GroupFAMSessionID>') "
"			AND ActiveFAM.LastPingTime >= DATEADD(SECOND, -90, GetUTCDate()) "
"			AND FileActionStatus.Priority >= <MinPriority> "
"		ORDER BY FileActionStatus.Priority DESC, FAMFile.ID ASC "
"		) "
"		SET NOCOUNT OFF \r\n"
"	END TRY \r\n"
"	BEGIN CATCH\r\n"

	// Ensure NOCOUNT is set to OFF
"	SET NOCOUNT OFF\r\n"

	// Get the error message, severity and state
"		DECLARE @ErrorMessage NVARCHAR(4000);\r\n"
"		DECLARE @ErrorSeverity INT;\r\n"
"		DECLARE @ErrorState INT;\r\n"

"	SELECT \r\n"
"		@ErrorMessage = ERROR_MESSAGE(),\r\n"
"		@ErrorSeverity = ERROR_SEVERITY(),\r\n"
"		@ErrorState = ERROR_STATE();\r\n"

	// Check for state of 0 (cannot raise error with state 0, set to 1)
"	IF @ErrorState = 0\r\n"
"		SELECT @ErrorState = 1\r\n"

	// Raise the error so that it will be caught at the outer scope
"	RAISERROR (@ErrorMessage,\r\n"
"		@ErrorSeverity,\r\n"
"		@ErrorState\r\n"
"	);\r\n"

"	END CATCH\r\n"
"	SELECT * FROM @OutputTableVar ;\r\n";

const string gstrADD_WORK_ITEM_GROUP_QUERY = 
	"INSERT INTO [dbo].WorkItemGroup (FileID, ActionID, StringizedSettings, FAMSessionID, "
	"NumberOfWorkItems, RunningTaskDescription) "
	"OUTPUT INSERTED.ID ";

const string gstrADD_WORK_ITEM_QUERY =
	"INSERT INTO [dbo].WorkItem (WorkItemGroupID, Status, Input, BinaryInput, Output, FAMSessionID, Sequence)  VALUES ";

const string gstrRESET_TIMEDOUT_WORK_ITEM_QUERY =
	"UPDATE dbo.WorkItem SET [Status] = 'P', [FAMSessionID] = NULL "
	"FROM dbo.WorkItem wi LEFT JOIN dbo.ActiveFAM af "
	"ON wi.FAMSessionID = af.FAMSessionID "
	"WHERE [Status] = 'R' AND "
	"	 (af.FAMSessionID IS NULL OR af.LastPingTime < DATEADD(SECOND, -<TimeOutInSeconds>,GetUTCDate()))";

const string gstrGET_WORK_ITEM_FOR_GROUP_IN_RANGE = 
	"SELECT [WorkItem].ID "
    "  ,[WorkItemGroupID] "
    "  ,[Status] "
    "  ,[Input] "
    "  ,[Output] "
	"  ,[WorkItem].[FAMSessionID] "
    "  ,[Sequence] "
	"  ,[stringizedException] "
	"  ,[FileName] "
	"  ,[BinaryOutput] "
	"  ,[BinaryInput] "
	"  ,[FileID] "
	"  ,[WorkItemGroup].[FAMSessionID] as WorkGroupFAMSessionID "
	"  ,[Priority] "
	"  ,[RunningTaskDescription] "
	"FROM [WorkItem] INNER JOIN WorkItemGroup ON WorkItem.WorkItemGroupID = WorkItemGroup.ID "
	"INNER JOIN FAMFile ON WorkItemGroup.FileID = FAMFile.ID "
	"WHERE WorkItemGroupID = <WorkItemGroupID> "
	"AND [Sequence] >= <StartSequence> AND [Sequence] < <EndSequence>";

const string gstrGET_FAILED_WORK_ITEM_FOR_GROUP =
	"SELECT [WorkItem].ID "
    "  ,[WorkItemGroupID] "
    "  ,[Status] "
    "  ,[Input] "
    "  ,[Output] "
	"  ,[WorkItem].[FAMSessionID] "
    "  ,[Sequence] "
	"  ,[stringizedException] "
	"  ,[FileName] "
	"  ,[BinaryOutput] "
	"  ,[BinaryInput] "
	"  ,[FileID] "
	"  ,[WorkItemGroup].[FAMSessionID] as WorkGroupFAMSessionID "
	"  ,[Priority] "
	"FROM [WorkItem] INNER JOIN WorkItemGroup ON WorkItem.WorkItemGroupID = WorkItemGroup.ID "
	"INNER JOIN FAMFile ON WorkItemGroup.FileID = FAMFile.ID "
	"WHERE WorkItemGroupID = <WorkItemGroupID> "
	"AND [WorkItem].[Status] = 'F' ";

const string gstrGET_WORK_ITEM_GROUP_ID =
	"WITH workItemTotals (ID, CountOfWorkItems ,NumberOfWorkItems)  "
	"AS ( "
	"	SELECT WorkItemGroupID AS ID "
	"		,COUNT(WorkItem.ID) AS CountOfWorkItems "
	"		,NumberOfWorkItems "
	"	FROM WorkItemGroup "
	"	INNER JOIN WorkItem ON WorkItemGroup.ID = WorkItem.WorkItemGroupID "
	"	WHERE FileID = <FileID> "
	"		AND ActionID = <ActionID> "
	"		AND StringizedSettings = '<StringizedSettings>' "
	"		AND NumberOfWorkItems = <NumberOfWorkItems> "
	"	GROUP BY WorkItemGroupID "
	"		,NumberOfWorkItems "
	"	) "
	"SELECT ID "
	"FROM workItemTotals "
	"WHERE CountOfWorkItems = NumberOfWorkItems ";

static const string gstrSTART_FILETASKSESSION_DATA = 
	"INSERT INTO [dbo].[FileTaskSession] "
	" ([FAMSessionID]"
	"  ,[TaskClassID]"
	"  ,[FileID])"
	"  OUTPUT INSERTED.ID"
	"  VALUES (<FAMSessionID>, (SELECT [ID] FROM [TaskClass] WHERE [GUID] = '<TaskClassGuid>'), <FileID>)";

static const string gstrUPDATE_FILETASKSESSION_DATA = 
	"UPDATE [dbo].[FileTaskSession] SET "
	"		[DateTimeStamp] = GETDATE(), "
	"		[Duration] = <Duration>, "
	"		[OverheadTime] = <OverheadTime> "
	"	WHERE [ID] = <FileTaskSessionID>";

static const string gstrINSERT_TASKCLASS_STORE_RETRIEVE_ATTRIBUTES = 
	"INSERT INTO [TaskClass] ([GUID], [Name]) VALUES \r\n"
	"	('B25D64C0-6FF6-4E0B-83D4-0D5DFEB68006', 'Core: Store/Retrieve attributes in DB') \r\n";

static const string gstrINSERT_PAGINATION_TASK_CLASS =
	"INSERT INTO [TaskClass] ([GUID], [Name]) VALUES \r\n"
	"	('DF414AD2-742A-4ED7-AD20-C1A1C4993175', 'Core: Paginate files') \r\n";

static const string gstrSELECT_SECURE_COUNTER_WITH_MAX_VALUE_CHANGE = 
	"	SELECT [sc].[ID] "
	"		,[sc].[CounterName] "
	"		,[sc].[SecureCounterValue] "
	"		,[sc].[AlertLevel] "
	"		,[sc].[AlertMultiple] "
	"		,[scvc].[ID] AS [ValueChangedID] "
	"		,[scvc].[FromValue] "
	"		,[scvc].[ToValue] "
	"		,[scvc].[LastUpdatedTime] "
	"		,[scvc].[LastUpdatedByFAMSessionID] "
	"		,[scvc].[MinFAMFileCount] "
	"		,[scvc].[HashValue] "
	"		,[scvc].[Comment] "
	"	FROM dbo.[SecureCounter] [sc] "
	"	LEFT JOIN dbo.[SecureCounterValueChange] [scvc] ON [sc].[ID] = [scvc].[CounterID] "
	"	WHERE ([scvc].[ID] = ( "
	"			SELECT Max([SecureCounterValueChange].[ID]) "
	"			FROM [SecureCounterValueChange] "
	"			WHERE [SecureCounterValueChange].[CounterID] = [SC].[ID] "
	"			) "
	"		OR [scvc].[ID] IS NULL) "
	"		ORDER BY [sc].[ID] ";

static const string gstrSELECT_SINGLE_PAGINATED_PAGE =
	"SELECT [ID] AS [SourceFileID], <SourcePage> AS [SourcePage], <DestPage> AS [DestPage] "
	"	FROM [FAMFile] WHERE [FileName] = '<SourceFileName>'";

static const string gstrINSERT_INTO_PAGINATION =
	"DECLARE @DestFileID INT \r\n"
	"SELECT @DestFileID = [ID] FROM [FAMFile] WHERE [FileName] = '<DestFileName>' \r\n"

	"INSERT INTO [Pagination] ([SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) \r\n"
	"	SELECT [NewPaginations].[SourceFileID], \r\n"
	"		[NewPaginations].[SourcePage], \r\n"
	"		@DestFileID AS [DestFileID], \r\n"
	"		[NewPaginations].[DestPage], \r\n"
	"		COALESCE([OriginalPages].[OriginalFileID], [NewPaginations].[SourceFileID]), \r\n"
	"		COALESCE([OriginalPages].[OriginalPage], [NewPaginations].[SourcePage]), \r\n"
	"		<FAMSessionID> AS [FileTaskSessionID] \r\n"
	"		FROM \r\n"
	"		( \r\n"
	"			<SelectPaginations> \r\n"
	"		) AS [NewPaginations] \r\n"
	"		LEFT JOIN \r\n"
	"		( \r\n"
	"			SELECT [DestFileID], [DestPage], [OriginalFileID], [OriginalPage] \r\n"
	"				FROM [Pagination] \r\n"
	"				GROUP BY [DestFileID], [DestPage], [OriginalFileID], [OriginalPage] \r\n"
	"		) AS [OriginalPages] \r\n"
	"		ON [NewPaginations].[SourceFileID] = [OriginalPages].[DestFileID] \r\n"
	"			AND [NewPaginations].[SourcePage] = [OriginalPages].[DestPage]";

static const string gstrACTIVE_PAGINATION_FILEID = 
	"SELECT        Pagination.DestFileID \r\n"
	"FROM            ActiveFAM INNER JOIN \r\n"
	"                         FAMSession ON ActiveFAM.FAMSessionID = FAMSession.ID INNER JOIN \r\n"
	"                         FileTaskSession ON FAMSession.ID = FileTaskSession.FAMSessionID INNER JOIN \r\n"
	"                         Pagination ON FileTaskSession.ID = Pagination.FileTaskSessionID \r\n"
	"WHERE        (Pagination.DestFileID = <FileID>) \r\n";

static const string gstrALTER_PAGINATION_ALLOW_NULL_DESTFILE = 
	"ALTER TABLE [dbo].[Pagination] ALTER COLUMN [DestFileID] INT NULL";

static const string gstrALTER_PAGINATION_ALLOW_NULL_DESTPAGE = 
	"ALTER TABLE [dbo].[Pagination] ALTER COLUMN [DestPage] INT NULL";

static const string gstrALTER_SECURE_COUNTER_VALUE_LAST_UPDATED_TIME =
"ALTER TABLE dbo.SecureCounterValueChange ALTER COLUMN LastUpdatedTime datetimeoffset";
