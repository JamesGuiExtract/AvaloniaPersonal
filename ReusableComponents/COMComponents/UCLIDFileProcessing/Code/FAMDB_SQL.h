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
	"[MainSequence] [BIT] NULL, "
	"[Guid] uniqueidentifier NOT NULL DEFAULT newid(),"
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
	"[Priority] [int] NOT NULL CONSTRAINT [DF_FAMFile_Priority] DEFAULT((3)), "
	"[AddedDateTime] [datetime] NOT NULL CONSTRAINT [DF_FAMFile_AddedDateTime] DEFAULT(GETDATE())"
	")";

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
	"[Guid] uniqueidentifier NOT NULL DEFAULT newid(),"
	"CONSTRAINT [PK_LoginID] PRIMARY KEY CLUSTERED ( [ID] ASC ))";

static const string gstrCREATE_MACHINE_TABLE = "CREATE TABLE [dbo].[Machine]("
	"[ID] [int] IDENTITY(1,1) NOT NULL, "
	"[MachineName] [nvarchar](50) NULL, "
	"CONSTRAINT [PK_Machine] PRIMARY KEY CLUSTERED ([ID] ASC), "
	"CONSTRAINT [IX_MachineName] UNIQUE NONCLUSTERED ([MachineName]))";

static const string gstrCREATE_FAM_USER_TABLE = "CREATE TABLE [dbo].[FAMUser]("
	"[ID] [int] IDENTITY(1,1) NOT NULL, "
	"[UserName] [nvarchar](50) NULL, "
	"[FullUserName] [nvarchar](128) NULL,"
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
	"[Guid] uniqueidentifier NOT NULL DEFAULT newid(),"
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
	"[ActionName] NVARCHAR(50) NOT NULL, "
	"CONSTRAINT [PK_LockedFile] PRIMARY KEY CLUSTERED ([FileID], [ActionName]))";

static const string gstrCREATE_USER_CREATED_COUNTER_TABLE =
	"CREATE TABLE [dbo].[UserCreatedCounter] ("
	"[CounterName] [nvarchar](50) NOT NULL CONSTRAINT [PK_UserCreatedCounter] PRIMARY KEY CLUSTERED,"
	"[Value] [bigint] NOT NULL CONSTRAINT [DF_UserCreatedCounter_Value] DEFAULT((0)), "
	"[Guid] uniqueidentifier NOT NULL DEFAULT newid())";

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
	"CONSTRAINT [PK_FileActionStatus] PRIMARY KEY NONCLUSTERED "
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
	"[AttributeQuery] [nvarchar](256) NOT NULL, "
	"[Guid] uniqueidentifier NOT NULL DEFAULT newid())";

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
	"[Blocking] [bit] NOT NULL DEFAULT 1,"
	"[WorkflowName] NVARCHAR(100) NULL, "
	"[Guid] uniqueidentifier NOT NULL DEFAULT newid())";

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
	"[Guid] uniqueidentifier NOT NULL DEFAULT newid(), "
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
	" [ActionID] [int] NULL,"
	" [TaskClassID] [int] NOT NULL, "
	" [FileID] [int] NOT NULL, "
	" [DateTimeStamp] [datetime] NULL, "
	" [Duration] [float] NULL, "
	" [OverheadTime] [float] NULL, "
	" [ActivityTime] [float] NULL)";

static const string gstrCREATE_FILE_TASK_SESSION_CACHE =
	"CREATE TABLE [dbo].[FileTaskSessionCache] ( "
	"[ID] BIGINT IDENTITY(1, 1) NOT NULL CONSTRAINT [PK_FileTaskSessionCache] PRIMARY KEY CLUSTERED, "
	// NOTE: When attribute data has been updated, AutoDeleteWithActiveFAMID will be set to NULL to
	// prevent deletion so subsequent sessions can use it.
	"[AutoDeleteWithActiveFAMID] INT NULL, "
	"[FileTaskSessionID] INT NOT NULL, "
	"[Page] INT NOT NULL, "
	"[ImageData] VARBINARY(MAX) NULL, "
	"[USSData] NVARCHAR(MAX) NULL, "
	"[WordZoneData] NVARCHAR(MAX) NULL, "
	"[AttributeData] NVARCHAR(MAX) NULL, "
	"[AttributeDataModifiedTime] DATETIME NULL, "
	"[Exception] NVARCHAR(MAX) NULL)";

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
	"  LastUpdatedTime datetime NOT NULL, "
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
	"	[OutputFilePathInitializationFunction] NVARCHAR(255) NULL, "
	"	[LoadBalanceWeight] INT NOT NULL CONSTRAINT [DF_Workflow_LoadBalanceWeight] DEFAULT(1), "
	"	[EditActionID] INT, "
	"	[PostEditActionID] INT, "
	"   [Guid] uniqueidentifier NOT NULL DEFAULT newid(),"
	"	CONSTRAINT [IX_WorkflowName] UNIQUE NONCLUSTERED ([Name]))";

static const string gstrCREATE_WORKFLOWFILE =
	"CREATE TABLE dbo.[WorkflowFile]( "
	"	[WorkflowID] INT NOT NULL, "
	"	[FileID] INT NOT NULL, "
	"	[Deleted] BIT NOT NULL DEFAULT(0), "
	"	[AddedDateTime] [datetime] NOT NULL CONSTRAINT [DF_WorkflowFile_AddedDateTime] DEFAULT(GETDATE()), "
	"	CONSTRAINT [PK_WorkflowFile] PRIMARY KEY CLUSTERED ([WorkflowID], [FileID]));";

static const string gstrCREATE_WORKFLOWCHANGE =
	"CREATE TABLE [dbo].[WorkflowChange]( "
	"	[ID]               INT IDENTITY(1, 1) NOT NULL, "
	"	[WorkflowChangeDate]   DATETIMEOFFSET "
	"		CONSTRAINT [DF_WorkflowChange_WorkflowChangeDate] DEFAULT (sysdatetimeoffset()) NOT NULL, "
	"	[DestWorkflowID]   INT NOT NULL, "
	"	CONSTRAINT [PK_WorkflowChange] PRIMARY KEY CLUSTERED([ID] ASC)); ";

static const string gstrCREATE_WORKFLOWCHANGEFILE =
"CREATE TABLE [dbo].[WorkflowChangeFile]( "
	"   [ID]			   INT IDENTITY(1, 1) NOT NULL,"
	"	[FileID]           INT NOT NULL, "
	"	[WorkflowChangeID] INT NOT NULL, "
	"	[SourceActionID]   INT NULL, "
	"	[DestActionID]     INT NULL, "
	"	[SourceWorkflowID] INT NULL, "
	"	[DestWorkflowID]   INT NOT NULL, "
	"	CONSTRAINT [PK_WorkflowChangeFile] PRIMARY KEY CLUSTERED([ID] ASC));";

static const string gstrCREATE_MLMODEL =
"CREATE TABLE [dbo].[MLModel]( "
	"	[ID]   INT IDENTITY(1, 1) NOT NULL, "
	"	[Name] NVARCHAR(255) NOT NULL, "
	"   [Guid] uniqueidentifier NOT NULL DEFAULT newid(),"
	"   CONSTRAINT [IX_MLModelName] UNIQUE NONCLUSTERED ([Name] ASC), "
	"   CONSTRAINT [PK_MLModel] PRIMARY KEY CLUSTERED ([ID] ASC));";

static const string gstrCREATE_MLDATA =
"CREATE TABLE [dbo].[MLData]( "
	"   [ID]		     INT IDENTITY(1, 1) NOT NULL,"
	"	[MLModelID]      INT NOT NULL, "
	"	[FileID]         INT NOT NULL, "
	"   [IsTrainingData] BIT NOT NULL DEFAULT 1, "
	"   [DateTimeStamp]  DATETIME NOT NULL, "
	"	[Data]           NVARCHAR(MAX) NOT NULL, "
	"   [CanBeDeleted]	 BIT NOT NULL DEFAULT 0, "
	"   CONSTRAINT [PK_MLData] PRIMARY KEY NONCLUSTERED ([ID] ASC), "
	"	CONSTRAINT [IX_MLDataDateTimeStamp] UNIQUE CLUSTERED ([DateTimeStamp] ASC, [ID] ASC));";

static const string gstrCREATE_WEB_APP_CONFIG =
	"CREATE TABLE [dbo].[WebAppConfig]( "
	"	[ID] INT IDENTITY(1, 1) NOT NULL CONSTRAINT [PK_WebAppConfig] PRIMARY KEY CLUSTERED, "
	"	[Type] NVARCHAR(100) NOT NULL, "
	"	[WorkflowID] INT NOT NULL, "
	"	[Settings] NTEXT, "
	"   [Guid] uniqueidentifier NOT NULL DEFAULT newid(),"
	"	CONSTRAINT[IX_WEB_APP_TYPE] UNIQUE NONCLUSTERED ([Type], [WorkflowID]))";

static const string gstrCREATE_DASHBOARD_TABLE =
	"CREATE TABLE [dbo].[Dashboard]( "
	"	[DashboardName] [nvarchar](100) NOT NULL CONSTRAINT [PK_Dashboard] PRIMARY KEY CLUSTERED, \r\n"
	"	[Definition] [xml] NOT NULL, \r\n"
	"   [FAMUserID] INT NOT NULL, \r\n"
	"   [LastImportedDate] DATETIME NOT NULL, \r\n "
	"   [UseExtractedData] BIT DEFAULT 0, \r\n"
	"   [ExtractedDataDefinition] [xml] NULL,"
	"   [Guid] uniqueidentifier NOT NULL DEFAULT newid()"
	")";

static const string gstrADD_DASHBOARD_FAMUSER_FK =
	"ALTER TABLE dbo.[Dashboard] "
	"WITH CHECK ADD CONSTRAINT [FK_Dashboard_FAMUser] FOREIGN KEY([FAMUserID]) "
	"REFERENCES [FAMUser]([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

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
	"[IX_Skipped_File] ON [SkippedFile]([FileID], [ActionID], [UserName]) INCLUDE ([FAMSessionID])";

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

static const string gstrCREATE_INPUT_EVENT_FAMUSER_WITH_TIMESTAMP_INDEX = "CREATE NONCLUSTERED INDEX "
	"IX_InputEvent_FAMUser_With_TimeStamp ON [dbo].[InputEvent]([FAMUserID]) INCLUDE([TimeStamp])";

static const string gstrCREATE_FILE_ACTION_STATUS_ALL_INDEX = 
	"CREATE UNIQUE NONCLUSTERED INDEX "
	"[IX_FileActionStatus_All] ON [dbo].[FileActionStatus] "
	"([ActionID] ASC, [ActionStatus] ASC, [Priority] DESC, [FileID] ASC)";

static const string gstrCREATE_ACTIONSTATUS_ACTIONID_PRIORITY_FILE_INDEX =
	"CREATE UNIQUE CLUSTERED INDEX[IX_ActionStatusActionIDPriorityFileID] ON[dbo].[FileActionStatus] "
	"("
	"	[ActionStatus] ASC,"
	"	[ActionID] ASC,"
	"	[Priority] DESC,"
	"	[FileID] ASC"
	")";

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

static const string gstrCREATE_PAGINATION_DESTFILE_INDEX =
	"CREATE NONCLUSTERED INDEX[IX_Pagination_DestFileID] "
	"ON[dbo].[Pagination]([DestFileID] ASC) "; 

static const string gstrCREATE_PAGINATION_SOURCEFILE_INDEX = 
	"CREATE NONCLUSTERED INDEX[IX_Pagination_SourceFileID] "
	"ON[dbo].[Pagination]([SourceFileID] ASC) ";

static const string gstrCREATE_FILE_TASK_SESSION_ACTION_INDEX =
	"CREATE NONCLUSTERED INDEX IX_FileTaskSession_Action \r\n"
	"ON [dbo].[FileTaskSession]([ActionID]) \r\n"
	"INCLUDE ([ID], [FileID], [DateTimeStamp]) ";

static const string gstrCREATE_WORKFLOWCHANGEFILE_INDEX =
"CREATE UNIQUE NONCLUSTERED INDEX [IX_WorkflowChangeFileUnique] "
	"ON [dbo].[WorkflowChangeFile] ([FileID] ASC, [WorkflowChangeID] ASC, [SourceActionID] ASC)";

static const string gstrCREATE_FAMSESSION_ID_FAMUSERID_INDEX =
"IF EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_FAMSession_ID_FAMUserID' AND object_id = OBJECT_ID('FAMSession')) \r\n"
"BEGIN \r\n"
"	DROP INDEX [IX_FAMSession_ID_FAMUserID] ON [dbo].[FAMSession] \r\n"
"END \r\n"
"CREATE NONCLUSTERED INDEX [IX_FAMSession_ID_FAMUserID] ON [dbo].[FAMSession] ( \r\n"
"	[ID] ASC \r\n"
"	, [FAMUserID] ASC \r\n"
") \r\n";

static const string gstrCREATE_FILETASKSESSION_DATETIMESTAMP_WITH_INCLUDES_INDEX =
"IF EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_FileTaskSession_DateTimeStamp_withIncludes ' AND object_id = OBJECT_ID('FileTaskSession')) \r\n"
"BEGIN \r\n"
"	DROP INDEX [IX_FileTaskSession_DateTimeStamp_withIncludes ] ON [dbo].[FileTaskSession] \r\n"
"END \r\n"
"CREATE NONCLUSTERED INDEX IX_FileTaskSession_DateTimeStamp_withIncludes ON[dbo].[FileTaskSession]([DateTimeStamp]) \r\n"
"INCLUDE( \r\n"
"	[FAMSessionID] \r\n"
"	, [TaskClassID] \r\n"
"	, [ActivityTime] \r\n"
") \r\n";

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

static const string gstrADD_WORKFLOW_EDITACTION_FK =
	"ALTER TABLE [dbo].[Workflow]  "
	" WITH CHECK ADD CONSTRAINT [FK_Workflow_EditAction] FOREIGN KEY([EditActionID]) "
	" REFERENCES [dbo].[Action] ([ID])"
	" ON UPDATE NO ACTION "  // Anything except NO ACTION leads to errors about cascading
	" ON DELETE NO ACTION";  // updates/deletes due to multiple FKs to Action table.

static const string gstrADD_WORKFLOW_POSTEDITACTION_FK =
	"ALTER TABLE [dbo].[Workflow]  "
	" WITH CHECK ADD CONSTRAINT [FK_Workflow_PostEditAction] FOREIGN KEY([PostEditActionID]) "
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

static const string gstrADD_WORKFLOWFILE_WORKFLOW_FK =
	"ALTER TABLE dbo.[WorkflowFile] "
	"WITH CHECK ADD CONSTRAINT [FK_WorkflowFile_Workflow] FOREIGN KEY([WorkflowID]) "
	"REFERENCES [Workflow]([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_WORKFLOWFILE_FAMFILE_FK =
	"ALTER TABLE dbo.[WorkflowFile] "
	"WITH CHECK ADD CONSTRAINT [FK_WorkflowFile_File] FOREIGN KEY([FileID]) "
	"REFERENCES [FAMFile]([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_WORKFLOWCHANGE_WORKFLOW_FK = 
	"ALTER TABLE dbo.[WorkflowChange] "
	"ADD CONSTRAINT [FK_WorkflowChange_WorkflowDest] FOREIGN KEY([DestWorkflowID]) "
	"REFERENCES dbo.[Workflow]([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_WORKFLOWCHANGEFILE_FAMFILE_FK = 
	"ALTER TABLE dbo.[WorkFlowChangeFile] "
	"ADD CONSTRAINT [FK_WorkflowChangeFile_FAMFile] FOREIGN KEY([FileID]) "
	"REFERENCES[dbo].[FAMFile]([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_WORKFLOWCHANGEFILE_WORKFLOWCHANGE_FK =
	"ALTER TABLE dbo.[WorkFlowChangeFile] "
	"ADD CONSTRAINT [FK_WorkflowChangeFile_WorkflowChange] FOREIGN KEY([WorkflowChangeID]) "
	"REFERENCES[dbo].[WorkflowChange]([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_FILE_HANDLER_WORKFLOW_FK =
	"ALTER TABLE dbo.[FileHandler] "
	"WITH CHECK ADD CONSTRAINT [FK_FileHandler_Workflow] FOREIGN KEY([WorkflowName]) "
	"REFERENCES [dbo].[Workflow]([Name]) "
	"ON UPDATE CASCADE "
	"ON DELETE SET NULL";

static const string gstrADD_MLDATA_MLMODEL_FK =
	"ALTER TABLE [MLData]  "
	"WITH CHECK ADD CONSTRAINT [FK_MLData_MLModel] FOREIGN KEY([MLModelID]) "
	"REFERENCES [MLModel] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_MLDATA_FAMFILE_FK =
	"ALTER TABLE [MLData]  "
	"WITH CHECK ADD CONSTRAINT [FK_MLData_FAMFile] FOREIGN KEY([FileID]) "
	"REFERENCES [FAMFile] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_WEB_APP_CONFIG_WORKFLOW_FK =
	"ALTER TABLE dbo.[WebAppConfig] "
	"WITH CHECK ADD CONSTRAINT[FK_WebAppConfig_Workflow] FOREIGN KEY([WorkflowID]) "
	"REFERENCES[Workflow]([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

// ActiveFAM FK with cascade deletes ensures cached data gets cleaned up if a session is lost.
static const string gstrADD_FILE_TASK_SESSION_CACHE_ACTIVEFAM_FK =
	"ALTER TABLE dbo.[FileTaskSessionCache] "
	"ADD CONSTRAINT [FK_FileTaskSessionCache_ActiveFAM] FOREIGN KEY ([AutoDeleteWithActiveFAMID]) "
	"REFERENCES [ActiveFAM]([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_DB_PROCEXECUTOR_ROLE =
	"IF DATABASE_PRINCIPAL_ID('db_procexecutor') IS NULL \r\n"
	"BEGIN\r\n"
	"	CREATE ROLE db_procexecutor \r\n"
	"	GRANT EXECUTE TO db_procexecutor \r\n"
	"END\r\n";

// Queries to add triggers
static const string gstrCREATE_ACTION_ON_DELETE_TRIGGER =
	"CREATE TRIGGER dbo.ActionOnDeleteTrigger \r\n"
	"ON  dbo.Action \r\n"
	"AFTER DELETE \r\n"
	"AS \r\n"
	"BEGIN \r\n"
	"-- SET NOCOUNT ON added to prevent extra result sets from \r\n"
	"-- interfering with SELECT statements. \r\n"
	"SET NOCOUNT ON; \r\n"
	" \r\n"
	"--Insert statements for trigger here \r\n"
	"DELETE FROM WorkflowChangeFile \r\n"
	"FROM WorkflowChangeFile w INNER JOIN deleted d ON W.SourceActionID = D.ID OR W.DestActionID = D.ID \r\n"
	"\r\n"
	"--Update the FileTaskSession table \r\n"
	"UPDATE [dbo].[FileTaskSession] \r\n"
	"SET [ActionID] = NULL  \r\n"
	"FROM[dbo].[FileTaskSession] INNER JOIN deleted d ON[FileTaskSession].ActionID = D.ID \r\n"
	"END";

static const string gstrCREATE_WORKFLOW_ON_DELETE_TRIGGER =
	"CREATE TRIGGER dbo.WorkflowOnDeleteTrigger \r\n"
	"ON  dbo.Workflow \r\n"
	"AFTER DELETE \r\n"
	"AS \r\n"
	"BEGIN \r\n"
	"-- SET NOCOUNT ON added to prevent extra result sets from \r\n"
	"-- interfering with SELECT statements. \r\n"
	"SET NOCOUNT ON; \r\n"
	" \r\n"
	"--Insert statements for trigger here \r\n"
	"DELETE FROM WorkflowChange \r\n"
	"FROM WorkflowChange W INNER JOIN deleted D ON W.DestWorkflowID = D.ID \r\n"
	" \r\n"
	"DELETE FROM WorkflowChangeFile \r\n"
	"FROM WorkflowChangeFile W INNER JOIN deleted D ON W.DestWorkflowID = D.ID OR w.SourceWorkflowID = D.ID \r\n"
	"END";


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
	"FROM         FileActionStatus WITH (NOLOCK) INNER JOIN "
	"                      FAMFile WITH (NOLOCK) ON FileActionStatus.FileID = FAMFile.ID LEFT JOIN "
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
"	[WorkflowID][int] NULL, \r\n"
"	[ASC_From] [nvarchar](1) NOT NULL \r\n"
"); \r\n"
"DECLARE @LoadBalanceRoundSize INT \r\n"
"SET NOCOUNT ON \r\n"
"BEGIN TRY \r\n"
// Calculate the load the total size of a each round of processing to be used when load balancing
// is active. The initial number of files grabbed should be a multiple of this to ensure when the
// round is sequenced, that there is a statistically appropriate chance which files get left out of
// the files returned.
"	SELECT @LoadBalanceRoundSize = CASE WHEN 1 = <LoadBalance> \r\n"
"			THEN ((<MaxFiles> + SUM(LoadBalanceWeight) - 1) / SUM(LoadBalanceWeight)) * SUM(LoadBalanceWeight) \r\n"
"			ELSE <MaxFiles> END \r\n"
"		FROM Workflow \r\n"
"	INNER JOIN Action ON WorkflowID = Workflow.ID AND ASCName = '<ActionName>' \r\n"
//	Iteration will generated a sequence of numbers 1 to 10 (The max weight of any given workflow)
"	;WITH Iteration(Num) AS \r\n"
"	( \r\n"
"		SELECT 1 \r\n"
"		UNION ALL \r\n"
"		SELECT Num + 1 FROM Iteration WHERE Num <= 10 \r\n"
"	), \r\n"
// Load balancing between workflows works by creating "rounds" of processing where for each round
// the number of files processed for a workflow will equal its weight. WeightedWorkflowList produces
// a result representing such a round where each row represents a file to be processed by the
// respective workflow. For each row returned, a random number will be assigned that will be
// consistent for across multiple references (based on the fact that RAND will generate the same
// value for all calls in a query).
"	WeightedWorkflowList(WorkflowID, LoadBalanceWeight, Random) AS \r\n"
"	( \r\n"
"		SELECT ID, \r\n"
"			LoadBalanceWeight, \r\n"
//	RAND has a very poor distribution for similar seeds. Gymnastics here is to a produce well
//  distributed seed for RAND
"			RAND(CHECKSUM(HASHBYTES('MD5', CAST(ROW_NUMBER() OVER(ORDER BY(SELECT 1)) ^ CHECKSUM(RAND()) AS VARCHAR)))) \r\n"
"		FROM Iteration CROSS JOIN Workflow \r\n"
"		WHERE Num <= Workflow.LoadBalanceWeight \r\n"
"	), \r\n"
// In order to account for cases where the FPS is not grabbing the same number of files at a time
// as the total number of files in a round (especially the case where only 1 file is being grabbed
// at a time), the order of files processed in a round should be randomized between separate calls
// to GFTP.
"   LoadBalancing(WorkflowID, OverallSequence, WorkflowSequence) AS \r\n"
"   ( \r\n"
"   	SELECT WorkflowID, \r\n"
"   			ROW_NUMBER() OVER (ORDER BY Random), \r\n"
"   			ROW_NUMBER() OVER (PARTITION BY WorkflowID ORDER BY Random) \r\n"
"   		FROM WeightedWorkflowList \r\n"
"   			WHERE 1 = <LoadBalance> \r\n"
"   ), \r\n"
// Selected files is the overall domain of files available with an added FileRepitition column used
// to weed out duplicate files when processing <All workflows>.
"	SelectedFiles AS \r\n"
"	( \r\n"
"		SELECT ID, FileName, FileSize, Pages, Priority, ActionStatus, ActionId,\r\n"
"			ROW_NUMBER() OVER (PARTITION BY ID ORDER BY Priority DESC, ID ASC) AS FileRepetition \r\n"
"		FROM ( <SelectFilesToProcessQuery> ) T \r\n"
"	), \r\n"
// Limited files will restrict the selected files to only those files to be returned. This includes
// excluding locked or processing files, limiting to <MaxFiles>, and removing duplicate file IDs when
// processing on <all workflows> by excluding cases where FileRepetition is > 1.
"	LimitedFiles AS \r\n"
"	( \r\n"
"		SELECT TOP(@LoadBalanceRoundSize) SelectedFiles.*, WorkflowID, \r\n"
//				WorkflowRound is in which round of processing this file should be processed.
"				(ROW_NUMBER() OVER(PARTITION BY WorkflowID ORDER BY Priority DESC, SelectedFiles.ID ASC) - 1) / LoadBalanceWeight AS WorkflowRound, \r\n"
//				WorkflowSequence is the position in a round specific to files of the same workflow
"				(ROW_NUMBER() OVER(PARTITION BY WorkflowID ORDER BY Priority DESC, SelectedFiles.ID ASC) - 1) % LoadBalanceWeight + 1 AS WorkflowSequence \r\n"
"			FROM SelectedFiles \r\n"
"			INNER JOIN Action ON SelectedFiles.ActionID = Action.ID \r\n"
"			LEFT JOIN Workflow ON 1 = <LoadBalance> AND Action.WorkflowID = Workflow.ID \r\n"
"			LEFT JOIN LockedFile ON SelectedFiles.ID = LockedFile.FileID AND LockedFile.ActionName = '<ActionName>' \r\n"
"			WHERE SelectedFiles.ActionStatus <> 'R' AND LockedFile.FileID IS NULL AND FileRepetition = 1 \r\n"
//			WorkflowRound and WorkflowSequence will determine the order when load balancing is active,
//			otherwise they will be NULL and Priority and FileID will determine the order.
"			ORDER BY WorkflowRound ASC, WorkflowSequence ASC, Priority DESC, SelectedFiles.ID ASC \r\n"
"	), \r\n"
// Sequenced files will apply randomized sequencing using the LoadBalancing expression above if load
// balancing is active.
"	SequencedFiles AS \r\n"
"	( \r\n"
"		SELECT TOP(<MaxFiles>) LimitedFiles.* \r\n"
"			FROM LimitedFiles \r\n"
"			LEFT JOIN LoadBalancing ON LimitedFiles.WorkflowID = LoadBalancing.WorkflowID \r\n"
"				AND LimitedFiles.WorkflowSequence = LoadBalancing.WorkflowSequence \r\n"
"		ORDER BY WorkflowRound ASC, OverallSequence ASC, Priority DESC, LimitedFiles.ID ASC \r\n"
"	) \r\n"
"	UPDATE FileActionStatus Set ActionStatus = 'R' \r\n"
"	OUTPUT SequencedFiles.ID, "
"		   SequencedFiles.FileName, "
"		   SequencedFiles.FileSize, "
"		   SequencedFiles.Pages, "
"		   SequencedFiles.Priority, "
"		   SequencedFiles.ActionId, "
"		   COALESCE(SequencedFiles.WorkflowID, -1) AS WorkflowID, "
"		   deleted.ActionStatus "
"		INTO @OutputTableVar \r\n"
"	FROM  \r\n"
"		SequencedFiles \r\n"
"	INNER JOIN FileActionStatus on FileActionStatus.FileID = SequencedFiles.ID AND FileActionStatus.ActionID = SequencedFiles.ActionID \r\n"
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
"	INSERT INTO LockedFile(FileID,ActionID,ActiveFAMID,StatusBeforeLock,ActionName) \r\n"
"		SELECT [@OutputTableVar].ID, ActionID, <ActiveFAMID> AS ActiveFAMID, ASC_From AS StatusBeforeLock, ASCName FROM @OutputTableVar \r\n"
"			INNER JOIN [Action] ON [ActionID] = [Action].[ID]; \r\n"
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
const string gstrSTANDARD_TOTAL_WORKFLOW_FILES_QUERY = "SELECT COUNT(*) AS " + gstrTOTAL_FILECOUNT_FIELD +
	" FROM [WorkflowFile] WHERE [WorkflowID] = <WorkflowID>";
const string gstrSTANDARD_TOTAL_FAMFILE_QUERY_ORACLE = "SELECT COUNT(*) AS \"" + 
	gstrTOTAL_FILECOUNT_FIELD + "\" FROM \"FAMFile\"";
// Queries for all currently enabled features.
const string gstrGET_ENABLED_FEATURES_QUERY = "SELECT [FeatureName], [AdminOnly] FROM [" +
	gstrDB_FEATURE + "] WHERE [Enabled] = 1";


const string gstrGET_WORK_ITEM_TO_PROCESS = 
"		DECLARE @OutputTableVar table (\r\n"
"		[ID] [int] NOT NULL,\r\n"
"		[WorkItemGroupID] [int] NOT NULL,\r\n"
"		[ActionID] [int] NOT NULL,\r\n"
"		[Status] [nchar](1) NOT NULL,\r\n"
"		[Input] [text] NULL,\r\n"
"		[Output] [text] NULL,\r\n"
"		[FAMSessionID] [int] NULL,\r\n"	
"		[FileName] [nvarchar](255) NULL,\r\n"
"		[StringizedException] [nvarchar](MAX) NULL,\r\n"
"		[BinaryOutput] [varbinary](MAX) NULL,\r\n"
"		[BinaryInput] [varbinary](MAX) NULL,\r\n"
"		[FileID] [int] NULL,\r\n"
"		[WorkGroupFAMSessionID] [int] NULL,\r\n"
"		[Priority] [int] NULL,\r\n"
"		[RunningTaskDescription] [nvarchar](256) NULL\r\n"
"	);\r\n"
"	SET NOCOUNT ON\r\n"
"	BEGIN TRY\r\n"
"		UPDATE [dbo].WorkItem Set Status = 'R', FAMSessionID = <FAMSessionID>\r\n"
"		OUTPUT DELETED.ID, DELETED.WorkItemGroupID, WorkItemGroup.ActionID, INSERTED.Status, DELETED.[Input],\r\n"
"			DELETED.[Output], INSERTED.FAMSessionID, FAMFile.FileName, DELETED.StringizedException, NULL,\r\n"
"			DELETED.BinaryInput, FAMFile.ID, WorkItemGroup.FAMSessionID, FileActionStatus.Priority,\r\n"
"			WorkItemGroup.RunningTaskDescription INTO @OutputTableVar\r\n"
"		FROM WorkItem INNER JOIN WorkItemGroup ON WorkItemGroup.ID = WorkItem.WorkItemGroupID\r\n"
"		INNER JOIN FAMFile WITH (NOLOCK) ON FAMFile.ID = WorkItemGroup.FileID\r\n"
"		INNER JOIN FileActionStatus WITH (NOLOCK) ON FAMFile.ID = FileActionStatus.FileID\r\n"
"		WHERE FileActionStatus.ActionID IN (<ActionIDs>)\r\n"
"	    AND WorkItem.ID IN (\r\n"
"			SELECT TOP(<MaxWorkItems>) WorkItem.ID\r\n"
"			FROM WorkItem WITH (ROWLOCK, UPDLOCK, READPAST) \r\n"
"			INNER JOIN WorkItemGroup ON WorkItem.WorkItemGroupID = WorkItemGroup.ID\r\n"
"			INNER JOIN FAMFile WITH (NOLOCK) ON FAMFile.ID = WorkItemGroup.FileID\r\n"
"			INNER JOIN FileActionStatus WITH (NOLOCK) ON FAMFile.ID = FileActionStatus.FileID\r\n"
"			INNER JOIN ActiveFAM WITH (NOLOCK) ON ActiveFAM.FAMSessionID = WorkItemGroup.FAMSessionID\r\n"
"			WHERE [Status] = 'P'\r\n"
"				AND FileActionStatus.ActionID IN (<ActionIDs>)\r\n"
"				AND WorkItemGroup.ActionID IN (<ActionIDs>)\r\n"
"				AND ('<GroupFAMSessionID>' = '' OR WorkItemGroup.FAMSessionID = '<GroupFAMSessionID>')\r\n"
"				AND ActiveFAM.LastPingTime >= DATEADD(SECOND, -90, GetUTCDate())\r\n"
"				AND FileActionStatus.Priority >= <MinPriority>\r\n"
"			ORDER BY FileActionStatus.Priority DESC, FAMFile.ID ASC\r\n"
"		)\r\n"
"		SET NOCOUNT OFF\r\n"
"	END TRY\r\n"
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
	"FROM [WorkItem] WITH (NOLOCK) INNER JOIN WorkItemGroup WITH (NOLOCK) ON WorkItem.WorkItemGroupID = WorkItemGroup.ID "
	"INNER JOIN FAMFile WITH (NOLOCK) ON WorkItemGroup.FileID = FAMFile.ID "
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
	"  ,[RunningTaskDescription] "
	"FROM [WorkItem] WITH (NOLOCK) INNER JOIN WorkItemGroup WITH (NOLOCK) ON WorkItem.WorkItemGroupID = WorkItemGroup.ID "
	"INNER JOIN FAMFile WITH (NOLOCK) ON WorkItemGroup.FileID = FAMFile.ID "
	"WHERE WorkItemGroupID = <WorkItemGroupID> "
	"AND [WorkItem].[Status] = 'F' ";

const string gstrGET_WORK_ITEM_GROUP_ID =
	"WITH workItemTotals (ID, CountOfWorkItems ,NumberOfWorkItems)  "
	"AS ( "
	"	SELECT WorkItemGroupID AS ID "
	"		,COUNT(WorkItem.ID) AS CountOfWorkItems "
	"		,NumberOfWorkItems "
	"	FROM WorkItemGroup "
	"	INNER JOIN WorkItem WITH (NOLOCK) ON WorkItemGroup.ID = WorkItem.WorkItemGroupID "
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
	"  ,[FileID] "
	"  ,[ActionID])"
	"  OUTPUT INSERTED.ID"
	"  VALUES (<FAMSessionID>, (SELECT [ID] FROM [TaskClass] WHERE [GUID] = '<TaskClassGuid>'), <FileID>, <ActionID>)";

static const string gstrUPDATE_FILETASKSESSION_DATA = 
	"UPDATE [dbo].[FileTaskSession] SET "
	"		[DateTimeStamp] = GETDATE(), "
	"		[Duration] = <Duration>, "
	"		[OverheadTime] = <OverheadTime>, "
	"       [ActivityTime] = <ActivityTime> "
	"	WHERE [ID] = <FileTaskSessionID>";

static const string gstrINSERT_TASKCLASS_STORE_RETRIEVE_ATTRIBUTES = 
	"INSERT INTO [TaskClass] ([GUID], [Name]) VALUES \r\n"
	"	('B25D64C0-6FF6-4E0B-83D4-0D5DFEB68006', 'Core: Store/Retrieve attributes in DB') \r\n";

static const string gstrINSERT_PAGINATION_TASK_CLASS =
	"INSERT INTO [TaskClass] ([GUID], [Name]) VALUES \r\n"
	"	('DF414AD2-742A-4ED7-AD20-C1A1C4993175', 'Pagination: Verify') \r\n";

static const string gstrSPLIT_MULTI_PAGE_DOCUMENT_TASK_CLASS =
	"INSERT INTO [TaskClass] ([GUID], [Name]) VALUES \r\n"
	"	('EF1279E8-4EC2-4CBF-9DE5-E107D97916C0', 'Core: Split multi-page document') \r\n";

static const string gstrINSERT_TASKCLASS_DOCUMENT_API =
	"INSERT INTO [TaskClass] ([GUID], [Name]) VALUES \r\n"
	"	('49C8149D-38D9-4EAF-A46B-CF16EBF0882F', 'Core: Document API') \r\n";

static const string gstrINSERT_TASKCLASS_WEB_VERIFICATION =
	"INSERT INTO [TaskClass] ([GUID], [Name]) VALUES \r\n"
	"	('FD7867BD-815B-47B5-BAF4-243B8C44AABB', 'Core: Web verification') \r\n";

static const string gstrINSERT_AUTO_PAGINATE_TASK_CLASS =
	"INSERT INTO [TaskClass] ([GUID], [Name]) VALUES \r\n"
	"	('8ECBCC95-7371-459F-8A84-A2AFF7769800', 'Pagination: Auto-Paginate') \r\n";

static const string gstrINSERT_RTF_DIVIDE_BATCHES_TASK_CLASS =
	"INSERT INTO [TaskClass] ([GUID], [Name]) VALUES \r\n"
	"	('5F37ABA6-7D18-4AB9-9ABE-79CE0F49C903', 'RTF: Divide batches') \r\n";

static const string gstrINSERT_RTF_UPDATE_BATCHES_TASK_CLASS =
	"INSERT INTO [TaskClass] ([GUID], [Name]) VALUES \r\n"
	"	('4FF8821E-D98A-4B45-AD1A-5E7F62621581', 'RTF: Update batches') \r\n";

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
	"INSERT INTO [Pagination] ([SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) \r\n"
	"	SELECT [NewPaginations].[SourceFileID], \r\n"
	"		[NewPaginations].[SourcePage], \r\n"
	"		<DestFileID> AS [DestFileID], \r\n"
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
	"FROM            ActiveFAM WITH (NOLOCK) INNER JOIN \r\n"
	"                         FAMSession WITH (NOLOCK) ON ActiveFAM.FAMSessionID = FAMSession.ID INNER JOIN \r\n"
	"                         FileTaskSession WITH (NOLOCK) ON FAMSession.ID = FileTaskSession.FAMSessionID INNER JOIN \r\n"
	"                         Pagination WITH (NOLOCK) ON FileTaskSession.ID = Pagination.FileTaskSessionID \r\n"
	"WHERE        (Pagination.DestFileID = <FileID>) \r\n";

static const string gstrALTER_PAGINATION_ALLOW_NULL_DESTFILE = 
	"ALTER TABLE [dbo].[Pagination] ALTER COLUMN [DestFileID] INT NULL";

static const string gstrALTER_PAGINATION_ALLOW_NULL_DESTPAGE = 
	"ALTER TABLE [dbo].[Pagination] ALTER COLUMN [DestPage] INT NULL";

static const string gstrALTER_SECURE_COUNTER_VALUE_LAST_UPDATED_TIME =
"ALTER TABLE dbo.SecureCounterValueChange ALTER COLUMN LastUpdatedTime datetimeoffset";

static const string gstrGET_WORKFLOW_STATUS =
// SET NOCOUNT ON is needed to prevent "operation is not allowed when the object is closed"
// errors using this query.
"SET NOCOUNT ON \r\n"
"BEGIN TRY \r\n"

"DECLARE @workflowFileIDs TABLE (FileID INT) \r\n"
"DECLARE @workflowStatuses TABLE \r\n"
"	(FileID INT, P INT, R INT, S INT, F INT, C INT, \r\n"
"	 EndStatus NVARCHAR(1), WorkflowStatus NVARCHAR(1)) \r\n"

"INSERT INTO @workflowFileIDs (FileID) \r\n"
"	SELECT DISTINCT [FileID] \r\n"
"		FROM [WorkflowFile] \r\n"
"			WHERE [WorkflowID] = <WorkflowID> \r\n"
"			AND [DELETED] = 0 \r\n"
"			AND (<FileID> < 1 OR <FileID> = [FileID]) \r\n"

"DECLARE @fileID INT \r\n"
"DECLARE @endStatus NVARCHAR(1) \r\n"
"DECLARE fileCursor CURSOR FOR SELECT [FileID] FROM @workflowFileIDs \r\n"
"OPEN fileCursor \r\n"

// For each file, populate status counts (P, R, S, F, C) + end action status into @workflowStatuses
"FETCH NEXT FROM fileCursor INTO @fileID \r\n"
"WHILE @@FETCH_STATUS = 0 \r\n"
"BEGIN \r\n"
// https://extract.atlassian.net/browse/ISSUE-15647
// Ensure that @endStatus is updated even if the file does not have a status in the end action.
"	SELECT @endStatus = COALESCE(MAX([ActionStatus]), 'U') \r\n" 
"		FROM [FileActionStatus] WITH (NOLOCK) \r\n"
"		WHERE [FileID] = @fileID AND [ActionID] = <EndActionID>; \r\n"

"	INSERT INTO @workflowStatuses (FileID, P, R, S, F, C, EndStatus) \r\n"
"	SELECT @fileID, *, @endStatus FROM \r\n"
"	( \r\n"
"		SELECT [FileID], [ActionStatus] FROM [FileActionStatus] WITH (NOLOCK) \r\n"
"			WHERE [FileID] = @fileID AND [ActionID] IN (<ActionIDs>) \r\n"
"	) AS T \r\n"
"	PIVOT \r\n"
"	( \r\n"
"		COUNT( [FileID]) \r\n"
"		FOR [ActionStatus] IN(P, R, S, F, C) \r\n"
"	) AS PVT \r\n"

"FETCH NEXT FROM fileCursor INTO @fileID \r\n"
"END \r\n"

"CLOSE fileCursor \r\n"
"DEALLOCATE fileCursor \r\n"

// Processing = ("End" action <> Failed/Skipped) and any other action is either pending or processing.
"UPDATE @workflowStatuses \r\n"
"SET [WorkflowStatus] = 'R' \r\n"
"WHERE ([EndStatus] IS NULL OR \r\n"
"	([EndStatus] <> 'F' AND [EndStatus] <> 'S')) \r\n"
"AND( [P] > 0 OR [R] > 0) \r\n"

// Complete = ("End" action = Complete) and (no action is pending or processing)
"UPDATE @workflowStatuses \r\n"
"SET [WorkflowStatus] = 'C' \r\n"
"WHERE [WorkflowStatus] IS NULL \r\n"
"AND [EndStatus] = 'C' \r\n"

// Failed = ("End" action <> Complete) and (any action is failed)
"UPDATE @workflowStatuses \r\n"
"SET [WorkflowStatus] = 'F' \r\n"
"WHERE [WorkflowStatus] IS NULL \r\n"
"AND [F] > 0 \r\n"

" IF 1 = <ReturnFileStatuses> BEGIN \r\n"
" SELECT \r\n"
"		[FileID], \r\n"
"		COALESCE([WorkflowStatus], 'U') AS [Status] \r\n"
"	FROM @workflowStatuses \r\n"
"	ORDER BY FileID \r\n"
"END \r\n"
"ELSE BEGIN \r\n"
"SELECT \r\n"
"		COALESCE([WorkflowStatus], 'U') AS [Status], \r\n"
"		COUNT(*) AS [Count] \r\n"
"	FROM @workflowStatuses \r\n"
"	GROUP BY[WorkflowStatus] \r\n"
"END \r\n"

// Ensure NOCOUNT is set back to off
"SET NOCOUNT OFF \r\n"
"END TRY \r\n"
"BEGIN CATCH \r\n"
"SET NOCOUNT OFF \r\n"
"END CATCH";

// Helper queries for Moving workflows
static const string gstrDROP_TEMP_FILESELECTION_PROC =
"IF OBJECT_ID('tempdb..#FileSelectionProc') IS NOT NULL DROP PROCEDURE #FileSelectionProc";

static const string gstrCREATE_TEMP_FILESELECTION_PROC =
"CREATE PROCEDURE #fileSelectionProc AS													 \r\n"
"BEGIN																					 \r\n"
"																						 \r\n"
"<SelectionQuery>																		 \r\n"
"																						 \r\n"
"END																					     ";

static const string gstrCREATE_TEMP_SELECTEDFILESTOMOVE = 
"if OBJECT_ID('tempdb..#SelectedFilesToMove') is not null DROP TABLE #SelectedFilesToMove\r\n"
"																						 \r\n"
"CREATE TABLE #SelectedFilesToMove(														 \r\n"
"	ID INT																				 \r\n"
")																						 \r\n"
"																						 \r\n"
"INSERT INTO #SelectedFilesToMove EXEC #fileSelectionProc								 \r\n"
"DROP PROCEDURE #fileSelectionProc															 ";

static const string gstrGET_ATTRIBUTE_VALUE =
	"; WITH TargetAttributeSet AS \r\n"
	"(\r\n"
	"	SELECT [AttributeSetForFile].[ID], \r\n"
	"	ROW_NUMBER() OVER(PARTITION BY [AttributeSetNameID], [FileID] ORDER BY [AttributeSetForFile].[ID] DESC) AS [InstanceID] \r\n"
	"	FROM [AttributeSetForFile] WITH (NOLOCK) \r\n"
	"	INNER JOIN [FileTaskSession] WITH (NOLOCK) ON [FileTaskSessionID] = [FileTaskSession].[ID] \r\n"
	"	INNER JOIN [AttributeSetName] ON [AttributeSetNameID] = [AttributeSetName].[ID] \r\n"
	"	INNER JOIN [FAMFile] WITH (NOLOCK) ON [FileID] = [FAMFile].[ID] \r\n"
	"	WHERE [AttributeSetName].[Description] = '<AttributeSetName>' \r\n"
	"	AND [FileName] = '<SourceDocName>' \r\n"
	"), \r\n"
	"AttributeHierarchy AS \r\n" 
	"(\r\n"
	"	SELECT \r\n"
	"		CAST(NULL AS BIGINT) AS [ID], \r\n"
	"		CAST(NULL AS NVARCHAR(MAX)) AS [Value], \r\n"
	"		'<AttributePath>' AS [RemainingPath] \r\n"
	"	UNION ALL \r\n"
	"	SELECT [Attribute].[ID] AS [ID], [Attribute].[Value], \r\n"
	"		CASE WHEN(CHARINDEX('/', [RemainingPath]) = 0) \r\n"
	"			THEN '' \r\n"
	"			ELSE SUBSTRING([RemainingPath], CHARINDEX('/', [RemainingPath]) + 1, 1000) \r\n"
	"		END AS [RemainingPath] \r\n"
	"	FROM [AttributeHierarchy] \r\n"
	"	INNER JOIN [Attribute] WITH (NOLOCK) ON [AttributeHierarchy].[ID] IS NULL OR [Attribute].[ParentAttributeID] = [AttributeHierarchy].[ID] \r\n"
	"	INNER JOIN [AttributeName] ON [AttributeNameID] = [AttributeName].[ID] \r\n"
	"	INNER JOIN [TargetAttributeSet] ON [Attribute].[AttributeSetForFileID] = [TargetAttributeSet].[ID] \r\n"
	"	WHERE [TargetAttributeSet].[InstanceID] = 1 \r\n"
	"	AND [AttributeName].[Name] = LEFT([RemainingPath], \r\n"
	"		CASE WHEN(CHARINDEX('/', [RemainingPath]) = 0) \r\n"
	"			THEN LEN([RemainingPath]) \r\n"
	"			ELSE CHARINDEX('/', [RemainingPath]) - 1 \r\n"
	"		END) \r\n"
	") \r\n"
	"SELECT [Value] FROM [AttributeHierarchy] WHERE LEN([RemainingPath]) = 0";

static const string gstrCREATE_DATABASE_SERVICE_TABLE =
	"CREATE TABLE [dbo].[DatabaseService]( "
	"	[ID][int] IDENTITY(1, 1) NOT NULL CONSTRAINT[PK_DatabaseService] PRIMARY KEY CLUSTERED, "
	"	[Description] NVARCHAR(256) NOT NULL, "
	"	[Settings] NVARCHAR(MAX) NOT NULL, "
	"   [Status] NVARCHAR(MAX) NULL, "
	"   [Enabled] BIT NOT NULL CONSTRAINT [DF_DatabaseServiceEnabled] DEFAULT 1, "
	"	[LastFileTaskSessionIDProcessed] INT NULL, "
	"	[StartTime] DateTime NULL, "
	"	[LastWrite] DateTime NULL, "
	"	[EndTime] DateTime NULL, "
	"	[MachineID] INT NULL, "
	"	[Exception] NVARCHAR(MAX) NULL, "
	"   [ActiveServiceMachineID] INT NULL, "
	"   [NextScheduledRunTime] DateTime NULL, "
	"	[ActiveFAMID] INT NULL, "
	"   [Guid] uniqueidentifier NOT NULL DEFAULT newid()"
	")";

static const string gstrCREATE_DATABASE_SERVICE_UPDATE_TRIGGER =
	"CREATE TRIGGER[dbo].[DatabaseServiceUpdateTrigger] \r\n"
	"	ON[dbo].[DatabaseService] \r\n"
	"AFTER UPDATE \r\n"
	"AS \r\n"
	"BEGIN \r\n"
	"-- SET NOCOUNT ON added to prevent extra result sets from \r\n"
	"-- interfering with SELECT statements. \r\n"
	"	SET NOCOUNT ON; \r\n"
	"	\r\n"
	"	UPDATE DS\r\n"
	"	SET ActiveServiceMachineID = NULL, \r\n"
	"	NextScheduledRunTime = NULL \r\n"
	"	FROM DatabaseService DS \r\n"
	"	INNER JOIN inserted I ON DS.ID = I.ID \r\n"
	"	WHERE I.ActiveFAMID IS NULL \r\n"
	"	END\r\n";

static const string gstrCREATE_DATABASE_SERVICE_DESCRIPTION_INDEX = 
	"CREATE UNIQUE NONCLUSTERED INDEX "
	"[IX_DatabaseService_Description] ON [DatabaseService]([Description])";

static const string gstrADD_DATABASESERVICE_ACTIVEFAM_FK =
	"ALTER TABLE [dbo].[DatabaseService] "
	"	WITH CHECK ADD CONSTRAINT [FK_DatabaseService_ActiveFAM] FOREIGN KEY([ActiveFAMID]) "
	"	REFERENCES [dbo].[ActiveFAM]([ID]) "
	"	ON UPDATE CASCADE "
	"	ON DELETE SET NULL";

static const string gstrADD_DATABASESERVICE_ACTIVE_MACHINE_FK =
"ALTER TABLE [dbo].[DatabaseService] "
"	WITH CHECK ADD CONSTRAINT [FK_DatabaseService_Active_Machine] FOREIGN KEY([ActiveServiceMachineID]) "
"	REFERENCES [dbo].[Machine]([ID]) "
"	ON UPDATE NO ACTION "
"	ON DELETE NO ACTION";

static const string gstrADD_DATABASESERVICE_MACHINE_FK =
"ALTER TABLE [dbo].[DatabaseService] "
"	WITH CHECK ADD CONSTRAINT [FK_DatabaseService_Machine] FOREIGN KEY([MachineID]) "
"	REFERENCES [dbo].[Machine]([ID]) "
"	ON UPDATE CASCADE "
"	ON DELETE SET NULL";

static const string gstrCREATE_REPORTING_VERIFICATION_RATES =
	"CREATE TABLE [dbo].[ReportingVerificationRates]( "
	"   [ID][int] IDENTITY(1, 1) NOT NULL CONSTRAINT[PK_ReportingVerificationRates] PRIMARY KEY NONCLUSTERED, "
	"   [DatabaseServiceID] [INT] NOT NULL, "
	"	[FileID] [int] NOT NULL, "
	"	[ActionID] [int] NULL,"
	"	[TaskClassID] [int] NOT NULL, "
	"   [LastFileTaskSessionID] [int] NOT NULL, "
	"	[Duration] [float] NOT NULL CONSTRAINT [DF_Duration] DEFAULT(0.0), "
	"	[OverheadTime] [float] NOT NULL CONSTRAINT [DF_OverheadTime] DEFAULT(0.0), "
	"	[ActivityTime][float] NOT NULL CONSTRAINT [DF_ActivityTime] DEFAULT(0.0) "
	"   CONSTRAINT [IX_ReportingVerificationRatesFileActionTask] UNIQUE CLUSTERED([FileID],[ActionID],[TaskClassID],[DatabaseServiceID]))";

static const std::string gstrADD_REPORTING_VERIFICATION_RATES_FAMFILE_FK =
	"ALTER TABLE[dbo].[ReportingVerificationRates]  "
	"	WITH CHECK ADD  CONSTRAINT [FK_ReportingVerificationRates_FAMFile] FOREIGN KEY([FileID]) "
	"	REFERENCES[dbo].[FAMFile]([ID]) "
	"	ON UPDATE CASCADE "
	"	ON DELETE CASCADE";

static const std::string gstrADD_REPORTING_VERIFICATION_RATES_DATABASE_SERVICE_FK =
	"ALTER TABLE[dbo].[ReportingVerificationRates]  "
	"	WITH CHECK ADD  CONSTRAINT [FK_ReportingVerificationRates_DatabaseService] FOREIGN KEY([DatabaseServiceID]) "
	"	REFERENCES[dbo].[DatabaseService]([ID]) "
	"	ON UPDATE CASCADE "
	"	ON DELETE CASCADE";

static const std::string gstrADD_REPORTING_VERIFICATION_RATES_ACTION_FK =
	"ALTER TABLE[dbo].[ReportingVerificationRates]  "
	"	WITH CHECK ADD  CONSTRAINT [FK_ReportingVerificationRates_Action] FOREIGN KEY([ActionID]) "
	"	REFERENCES[dbo].[Action]([ID]) "
	"	ON UPDATE CASCADE "
	"	ON DELETE CASCADE";

static const std::string gstrADD_REPORTING_VERIFICATION_RATES_TASK_CLASS_FK =
	"ALTER TABLE[dbo].[ReportingVerificationRates]  "
	"	WITH CHECK ADD  CONSTRAINT [FK_ReportingVerificationRates_TaskClass] FOREIGN KEY([TaskClassID]) "
	"	REFERENCES[dbo].[TaskClass]([ID]) "
	"	ON UPDATE CASCADE "
	"	ON DELETE CASCADE";
static const std::string gstrADD_REPORTING_VERIFICATION_RATES_FILE_TASK_SESSION_FK =
	"ALTER TABLE[dbo].[ReportingVerificationRates]  "
	"	WITH CHECK ADD  CONSTRAINT [FK_ReportingVerificationRates_FileTaskSession] FOREIGN KEY([LastFileTaskSessionID]) "
	"	REFERENCES[dbo].[FileTaskSession]([ID]) "
	"	ON UPDATE CASCADE "
	"	ON DELETE CASCADE";

static const std::string gstrCREATE_PAGINATED_DEST_FILES_VIEW =
	"IF OBJECT_ID('[dbo].[vPaginatedDestFiles]', 'V') IS NULL "
	"	EXECUTE('CREATE VIEW [dbo].[vPaginatedDestFiles] "
	"		AS "
	"		SELECT DISTINCT DestFileID "
	"		FROM            dbo.Pagination "
	"		WHERE(DestFileID NOT IN "
	"		(SELECT        SourceFileID "
	"			FROM            dbo.Pagination "
	"			WHERE(SourceFileID <> DestFileID)))'"
	"	)";

static const std::string gstrCREATE_USERS_WITH_ACTIVE_VIEW =
	"IF OBJECT_ID('[dbo].[vUsersWithActive]', 'V') IS NULL "
	"	EXECUTE('CREATE VIEW [dbo].[vUsersWithActive] "
	"		AS "
	"		SELECT dbo.FAMUser.ID AS FAMUserID "
	"		, dbo.FAMUser.UserName "
	"       , dbo.FAMUser.FullUserName "
	"		, dbo.ActiveFAM.LastPingTime "
	"		, CASE "
	"				WHEN LastPingTime IS NULL THEN 0 "
	"               WHEN DATEDIFF(mi, LastPingTime, GETDATE()) > 5 THEN 0 "
	"				ELSE 1 "
	"			END AS CurrentlyActive "
	"		FROM dbo.ActiveFAM "
	"			INNER JOIN dbo.FAMSession ON dbo.ActiveFAM.FAMSessionID = dbo.FAMSession.ID "
	"			RIGHT OUTER JOIN dbo.FAMUser ON dbo.FAMSession.FAMUserID = dbo.FAMUser.ID '"
	"	)";

static const std::string gstrVIEW_DEFINITION_FOR_FAMUSER_INPUT_EVENTS_TIME =
"			SELECT[FAMSession].[FAMUserID]															"
"				, CAST([FileTaskSession].[DateTimeStamp] AS DATE) AS [InputDate]					"
"				, SUM([FileTaskSession].[ActivityTime] / 60.0) AS           TotalMinutes			"
"			FROM[FAMSession]																		"
"			INNER JOIN[FileTaskSession] ON[FAMSession].[ID] =										"
"			[FileTaskSession].[FAMSessionID]														"
"			inner join TaskClass on FileTaskSession.TaskClassID = TaskClass.ID						"
"			where([TaskClass].GUID IN																"
"				(''FD7867BD-815B-47B5-BAF4-243B8C44AABB'',											"
"				 ''59496DF7-3951-49B7-B063-8C28F4CD843F'',											"
"				 ''AD7F3F3F-20EC-4830-B014-EC118F6D4567'',											"
"				 ''DF414AD2-742A-4ED7-AD20-C1A1C4993175'',											"
"				 ''8ECBCC95-7371-459F-8A84-A2AFF7769800''))											"
"				AND																					"
"					FileTaskSession.DateTimeStamp IS NOT NULL										"
"			GROUP BY[FAMSession].[FAMUserID]														"
"				, CAST([FileTaskSession].[DateTimeStamp] AS DATE)									";

static const std::string gstrCREATE_FAMUSER_INPUT_EVENTS_TIME_VIEW = 
	"IF OBJECT_ID('[dbo].[vFAMUserInputEventsTime]', 'V') IS NULL "
	"	EXECUTE('CREATE VIEW[dbo].[vFAMUserInputEventsTime] "
	"		AS " + gstrVIEW_DEFINITION_FOR_FAMUSER_INPUT_EVENTS_TIME +
	"'	)";

static const std::string gstrALTER_FAMUSER_INPUT_EVENTS_TIME_VIEW =
"IF OBJECT_ID('[dbo].[vFAMUserInputEventsTime]', 'V') IS NOT NULL "
"	EXECUTE('ALTER VIEW[dbo].[vFAMUserInputEventsTime] "
"		AS " + gstrVIEW_DEFINITION_FOR_FAMUSER_INPUT_EVENTS_TIME +
"'	)";

static const std::string gstrCREATE_PAGINATION_DATA_WITH_RANK_VIEW =
	"IF OBJECT_ID('[dbo].[vPaginationDataWithRank]', 'V') IS NULL \r\n"
	"		EXECUTE('CREATE VIEW[dbo].[vPaginationDataWithRank] AS SELECT Pagination.ID PaginationID, \r\n"
	"			FAMSession.FAMUserID, \r\n"
	"			SourceFileID, \r\n"
	"			DestFileID, \r\n"
	"			OriginalFileID, \r\n"
	"			FileTaskSession.DateTimeStamp, \r\n"
	"			FileTaskSession.ActionID, \r\n"
	"			Action.ASCName, \r\n"
	"			Pagination.FileTaskSessionID, \r\n"
	"			RANK() \r\n"
	"			OVER(PARTITION BY SourceFileID, Action.ASCName, Pagination.FileTaskSessionID \r\n"
	"				ORDER BY  FileTaskSession.DateTimeStamp DESC) RankDesc \r\n"
	"				FROM[dbo].[Pagination] \r\n"
	"				INNER JOIN FileTaskSession \r\n"
	"				ON FileTaskSession.ID = Pagination.FileTaskSessionID \r\n"
	"				INNER JOIN Action \r\n"
	"				ON FileTaskSession.ActionID = Action.ID \r\n"
	"				INNER JOIN FAMSession \r\n"
	"				ON FAMSession.ID = FileTaskSession.FAMSessionID \r\n"
	"				WHERE FileTaskSession.ActionID IS NOT NULL \r\n"
	"				AND FileTaskSession.DateTimeStamp IS NOT NULL') \r\n";

static const std::string gstrCREATE_PROCESSING_DATA_VIEW =
	"IF OBJECT_ID('[dbo].[vProcessingData]', 'V') IS NULL \r\n"
	"		EXECUTE(' \r\n"
	"			CREATE VIEW[dbo].[vProcessingData] \r\n"
	"			AS \r\n"
	"			WITH Combined(FileID, \r\n"
	"				ASCName, \r\n"
	"				ActionID, \r\n"
	"				ASC_From, \r\n"
	"				ASC_To, \r\n"
	"				DateTimeStamp, \r\n"
	"				TheRow, \r\n"
	"				WorkflowName) \r\n"
	"				AS(SELECT FileID, \r\n"
	"					ACTION.ASCName, \r\n"
	"					ActionID, \r\n"
	"					ASC_From, \r\n"
	"					ASC_To, \r\n"
	"					DateTimeStamp, \r\n"
	"					ROW_NUMBER() OVER(PARTITION BY FileID, \r\n"
	"						ActionID ORDER BY FileID DESC,  \r\n"
	"						ActionID ASC, \r\n"
	"						DateTimeStamp DESC) AS TheRow, \r\n"
	"					COALESCE(Workflow.Name, '''') WorkflowName \r\n"
	"					FROM FileActionStateTransition \r\n"
	"					INNER JOIN ACTION ON(FileActionStateTransition.ActionID = ACTION.ID) \r\n"
	"					LEFT JOIN Workflow ON[Workflow].ID = [Action].WorkflowID \r\n"
	"					WHERE(ASC_From = ''P'' \r\n"
	"						OR ASC_From = ''R'') \r\n"
	"					AND(ASC_To = ''R'' \r\n"
	"						OR ASC_To = ''C'' \r\n"
	"						OR ASC_To = ''F'')) \r\n"
	"				SELECT f1.WorkflowName, \r\n"
	"				f1.ASCName AS ASCName, \r\n"
	"				f1.ActionID, \r\n"
	"				DATEDIFF(Second, f1.DateTimeStamp, MAX(f2.DateTimeStamp)) AS TotalTime, \r\n"
	"				f1.DateTimeStamp, \r\n"
	"				COUNT(f1.FileID) AS FileCount, \r\n"
	"				Sum(FAMFile.Pages) as Pages \r\n"
	"				FROM Combined AS f1 \r\n"
	"				INNER JOIN Combined AS f2 ON((F1.TheRow = f2.TheRow + 1) \r\n"
	"					AND f1.FileID = f2.FileID \r\n"
	"					AND f1.ASCName = f2.ASCName \r\n"
	"					AND f1.ASC_To = f2.ASC_From) \r\n"
	"				INNER JOIN FAMFile ON f1.FileID = FAMFile.ID \r\n"
	"				WHERE f1.DateTimeStamp < f2.DateTimeStamp \r\n"
	"				GROUP BY f1.WorkflowName, \r\n"
	"				f1.ActionID, \r\n"
	"				f1.ASCName, \r\n"
	"				f1.ASC_From, \r\n"
	"				f1.ASC_to, \r\n"
	"				f1.DateTimeStamp, \r\n"
	"				f2.ASCName, \r\n"
	"				f2.ASC_From, \r\n"
	"				f2.ASC_to') ";

static const string gstr_CLEAR_DATABASE_SERVICE_STATUS_FIELDS =
	"UPDATE[dbo].[DatabaseService] \r\n"
	"SET\r\n"
		"[Status] = NULL,\r\n"
		"[LastFileTaskSessionIDProcessed] = NULL,\r\n"
		"[StartTime] = NULL,\r\n"
		"[LastWrite] = NULL,\r\n"
		"[EndTime] = NULL,\r\n"
		"[MachineID] = NULL,\r\n"
		"[Exception] = NULL,\r\n"
		"[ActiveServiceMachineID] = NULL,\r\n"
		"[NextScheduledRunTime] = NULL,\r\n"
		"[ActiveFAMID] = NULL;\r\n";

static const string gstrGET_OR_CREATE_FILE_TASK_SESSION_CACHE_ROW =
"DECLARE @rowID TABLE ([ID] BIGINT)\r\n"
// SET NOCOUNT ON is needed to prevent "operation is not allowed when the object is closed"
"SET NOCOUNT ON \r\n"
"IF EXISTS(SELECT[ID] FROM [FileTaskSession] WHERE [ID] = <FileTaskSessionID> AND [DateTimeStamp] IS NULL)\r\n"
"BEGIN\r\n"
"	INSERT INTO @rowID\r\n"
"		SELECT [ID]\r\n"
"		FROM [FileTaskSessionCache]\r\n"
"		WHERE [FileTaskSessionID] = <FileTaskSessionID> AND [Page] = <Page>\r\n"
"	IF NOT EXISTS(SELECT * FROM @rowID) AND <CrucialUpdate> = 0\r\n"
"	BEGIN\r\n"
"	INSERT INTO dbo.[FileTaskSessionCache] ([AutoDeleteWithActiveFAMID], [FileTaskSessionID], [Page])\r\n"
"		OUTPUT INSERTED.ID INTO @rowID\r\n"
"		SELECT [ActiveFAM].[ID], <FileTaskSessionID>, <Page>\r\n"
"		FROM [ActiveFAM]\r\n"
"			INNER JOIN [FAMSession] ON [ActiveFAM].[FAMSessionID] = [FAMSession].[ID]\r\n"
"			INNER JOIN [FileTaskSession] ON [FileTaskSession].[FAMSessionID] = [FAMSession].[ID]\r\n"
"		WHERE [FileTaskSession].[ID] = <FileTaskSessionID>\r\n"
"	END\r\n"
"END\r\n"
"SELECT COALESCE(MIN([ID]), -1) AS [ID] FROM @rowID";

static const string gstrCREATE_FILE_TASK_SESSION_CACHE_ROWS =
// SET NOCOUNT ON is needed to prevent "operation is not allowed when the object is closed"
"SET NOCOUNT ON\r\n"
";WITH AllPages(Page) AS(\r\n"
"	SELECT 1\r\n"
"	UNION ALL\r\n"
"	SELECT Page + 1 FROM AllPages WHERE Page < <PageCount>\r\n"
")\r\n"
"INSERT INTO [FileTaskSessionCache] ([AutoDeleteWithActiveFAMID], [FileTaskSessionID], [Page])\r\n"
"	SELECT [ActiveFAM].[ID], <FileTaskSessionID>, [AllPages].[Page]\r\n"
"		FROM [AllPages]\r\n"
"		LEFT JOIN [FileTaskSessionCache] ON [FileTaskSessionCache].[FileTaskSessionID] = <FileTaskSessionID>\r\n"
"			AND [FileTaskSessionCache].[Page] = [AllPages].[Page]\r\n"
"		INNER JOIN [FileTaskSession] ON <FileTaskSessionID> = [FileTaskSession].[ID]\r\n"
"		INNER JOIN [ActiveFAM] ON [FileTaskSession].[FAMSessionID] = [ActiveFAM].[FAMSessionID]\r\n"
"		WHERE [FileTaskSessionCache].[Page] IS NULL\r\n"
"		OPTION (MAXRECURSION 0);\r\n" // Max recursion on CTEs (AllPages in this case) is 100 by default; turn off limit
"SELECT COUNT(*) AS [CacheRowCount] FROM [FileTaskSessionCache] WHERE [FileTaskSessionID] = <FileTaskSessionID>";

static const string gstrGET_FILE_TASK_SESSION_CACHE_ROWS =
	"SELECT [Page] FROM [FileTaskSessionCache] \r\n"
	"	WHERE [FileTaskSessionID] = <FileTaskSessionID>";

static const string gstrGET_FILE_TASK_SESSION_CACHE_DATA_BY_PAGE =
	"SELECT <FieldList> FROM [FileTaskSessionCache] \r\n"
	"	WHERE [FileTaskSessionID] = <FileTaskSessionID> AND [Page] = <Page>";

static const string gstrGET_FILE_TASK_SESSION_CACHE_DATA =
	"SELECT <FieldList> FROM [FileTaskSessionCache] \r\n"
	"	WHERE [FileTaskSessionID] = <FileTaskSessionID>";

static const string gstrGET_FILE_TASK_SESSION_CACHE_DATA_BY_ID =
	"SELECT <FieldList>, GetDate() FROM [FileTaskSessionCache] \r\n"
	"	WHERE [ID] = <ID>";

static const string gstrMARK_TASK_SESSION_ATTRIBUTE_DATA_UNMODIFIED =
	"UPDATE [FileTaskSessionCache] \r\n"
	"	SET [AttributeDataModifiedTime] = NULL, [FileTaskSessionCache].[AutoDeleteWithActiveFAMID] = [ActiveFAM].[ID] \r\n"
	"	FROM [FileTaskSessionCache] \r\n"
	"	INNER JOIN [FileTaskSession] ON [FileTaskSessionID] = [FileTaskSession].[ID] \r\n"
	"	INNER JOIN [FAMSession] ON [FileTaskSession].[FAMSessionID] = [FAMSession].[ID] \r\n"
	"	INNER JOIN [ActiveFAM] ON [ActiveFAM].[FAMSessionID] = [FAMSession].[ID] \r\n"
	"	WHERE [FileTaskSessionID] = <FileTaskSessionID> \r\n"
	"		AND [AttributeDataModifiedTime] IS NOT NULL \r\n";

static const string gstrDISCARD_OLD_CACHE_DATA = "DELETE [FileTaskSessionCache] \r\n"
	"	FROM [FileTaskSessionCache] T \r\n"
	"	INNER JOIN [FileTaskSession] ON [FileTaskSessionID] = [FileTaskSession].[ID] \r\n"
	"	INNER JOIN [FAMSession] ON [FileTaskSession].[FAMSessionID] = [FAMSession].[ID] \r\n"
	"	WHERE [FileID] = <FileID> \r\n"
	"		AND (<ActionID> < 0 OR [FileTaskSession].[ActionID] = <ActionID>) \r\n"
	"		AND (<ExceptFileTaskSessionID> < 0 OR [FileTaskSessionID] <> <ExceptFileTaskSessionID>)";

static const string gstrGET_UNCOMMITTED_ATTRIBUTE_DATA =
	// SET NOCOUNT ON is needed to prevent "operation is not allowed when the object is closed"
	"SET NOCOUNT ON \r\n"
	";WITH [UncommittedSessions]([FileTaskSessionID], [FullUserName], [CacheOrder]) AS \r\n"
	"( \r\n"
	"	SELECT [FileTaskSessionID], COALESCE([FullUserName], [UserName]), \r\n"
	"			ROW_NUMBER() OVER(ORDER BY [AttributeDataModifiedTime] DESC) AS [CacheOrder] \r\n"
	"		FROM [FileTaskSessionCache]\r\n"
	"		INNER JOIN [FileTaskSession] ON [FileTaskSessionID] = [FileTaskSession].[ID] \r\n"
	"		INNER JOIN [FAMSession] ON [FAMSessionID] = [FAMSession].[ID] \r\n"
	"		INNER JOIN [FAMUser] ON [FAMUserID] = [FAMUser].[ID] \r\n"
	"		WHERE [FileID] = <FileID> \r\n"
	"			AND (<ActionID> < 0 OR [FileTaskSession].[ActionID] = <ActionID>) \r\n"
	"			AND [AttributeDataModifiedTime] IS NOT NULL \r\n"
	") \r\n"
	"SELECT [FullUserName], [AttributeDataModifiedTime], [Page], [AttributeData] \r\n"
	"	FROM [FileTaskSessionCache]\r\n"
	"	INNER JOIN [UncommittedSessions] ON [FileTaskSessionCache].[FileTaskSessionID] = [UncommittedSessions].[FileTaskSessionID] \r\n"
	"		AND [UncommittedSessions].[CacheOrder] = 1 \r\n"
	"	WHERE [AttributeDataModifiedTime] IS NOT NULL \r\n"
	"	AND(LEN('<ExceptIfMoreRecentAttributeSetName>') = 0 OR NOT EXISTS \r\n"
	"	( \r\n"
	"		SELECT * FROM [AttributeSetForFile] \r\n"
	"		INNER JOIN [FileTaskSession] ON [FileTaskSessionID] = [FileTaskSession].[ID] \r\n"
	"		INNER JOIN [AttributeSetName] ON [AttributeSetForFile].[AttributeSetNameID] = [AttributeSetName].[ID] \r\n"
	"		WHERE [FileID] = <FileID> \r\n"
	"		AND [AttributeSetName].[Description] = '<ExceptIfMoreRecentAttributeSetName>' \r\n"
	"		AND [FileTaskSession].[DateTimeStamp] > [AttributeDataModifiedTime] \r\n"
	"	)) \r\n";

static const string gstrADD_GUID_COLUMNS =
  " IF COL_LENGTH('dbo.Action', 'GUID') IS NULL BEGIN ALTER TABLE dbo.Action ADD [Guid] uniqueidentifier NOT NULL DEFAULT newid() END \r\n"
  " IF COL_LENGTH('dbo.AttributeSetName', 'GUID') IS NULL BEGIN ALTER TABLE dbo.AttributeSetName ADD[Guid] uniqueidentifier NOT NULL DEFAULT newid() END \r\n"
  " IF COL_LENGTH('dbo.Dashboard', 'GUID') IS NULL BEGIN ALTER TABLE dbo.Dashboard ADD[Guid] uniqueidentifier NOT NULL DEFAULT newid() END \r\n"
  " IF COL_LENGTH('dbo.DatabaseService', 'GUID') IS NULL BEGIN ALTER TABLE dbo.DatabaseService ADD[Guid] uniqueidentifier NOT NULL DEFAULT newid() END \r\n"
  " IF COL_LENGTH('dbo.FAMUser', 'GUID') IS NULL BEGIN ALTER TABLE dbo.FAMUser ADD[Guid] uniqueidentifier NOT NULL DEFAULT newid() END \r\n"
  " IF COL_LENGTH('dbo.FileHandler', 'GUID') IS NULL BEGIN ALTER TABLE dbo.FileHandler  ADD[Guid] uniqueidentifier NOT NULL DEFAULT newid() END \r\n"
  " IF COL_LENGTH('dbo.Workflow', 'GUID') IS NULL BEGIN ALTER TABLE dbo.Workflow ADD[Guid] uniqueidentifier NOT NULL DEFAULT newid() END \r\n"
  " IF COL_LENGTH('dbo.Login', 'GUID') IS NULL BEGIN ALTER TABLE dbo.Login ADD[Guid] uniqueidentifier NOT NULL DEFAULT newid() END \r\n"
  " IF COL_LENGTH('dbo.MetadataField', 'GUID') IS NULL BEGIN ALTER TABLE dbo.MetadataField ADD[Guid] uniqueidentifier NOT NULL DEFAULT newid() END \r\n"
  " IF COL_LENGTH('dbo.MLModel', 'GUID') IS NULL BEGIN ALTER TABLE dbo.MLModel ADD[Guid] uniqueidentifier NOT NULL DEFAULT newid() END \r\n"
  " IF COL_LENGTH('dbo.Tag', 'GUID') IS NULL BEGIN ALTER TABLE dbo.Tag ADD[Guid] uniqueidentifier NOT NULL DEFAULT newid() END \r\n"
  " IF COL_LENGTH('dbo.WebAppConfig', 'GUID') IS NULL BEGIN ALTER TABLE dbo.WebAppConfig ADD[Guid] uniqueidentifier NOT NULL DEFAULT newid() END \r\n"
  " IF COL_LENGTH('dbo.AttributeName', 'GUID') IS NULL BEGIN ALTER TABLE dbo.AttributeName  ADD[Guid] uniqueidentifier NOT NULL DEFAULT newid() END \r\n"
  " IF COL_LENGTH('dbo.FieldSearch', 'GUID') IS NULL BEGIN ALTER TABLE dbo.FieldSearch  ADD[Guid] uniqueidentifier NOT NULL DEFAULT newid() END \r\n"
  " IF COL_LENGTH('dbo.UserCreatedCounter', 'GUID') IS NULL BEGIN ALTER TABLE dbo.UserCreatedCounter ADD[Guid] uniqueidentifier NOT NULL DEFAULT newid() END \r\n"
  " IF(Exists(SELECT * FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'LabDEEncounter')) BEGIN IF COL_LENGTH('dbo.LabDEEncounter', 'GUID') IS NULL BEGIN ALTER TABLE dbo.LabDEEncounter ADD[Guid] uniqueidentifier NOT NULL DEFAULT newid() END END \r\n"
  " IF(Exists(SELECT * FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'LabDEOrder')) BEGIN IF COL_LENGTH('dbo.LabDEOrder', 'GUID') IS NULL BEGIN ALTER TABLE dbo.LabDEOrder ADD[Guid] uniqueidentifier NOT NULL DEFAULT newid() END END \r\n"
  " IF(Exists(SELECT * FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'LabDEPatient')) BEGIN IF COL_LENGTH('dbo.LabDEPatient', 'GUID') IS NULL BEGIN ALTER TABLE dbo.LabDEPatient ADD[Guid] uniqueidentifier NOT NULL DEFAULT newid() END END \r\n"
  " IF(Exists(SELECT * FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'LabDEProvider')) BEGIN IF COL_LENGTH('dbo.LabDEProvider', 'GUID') IS NULL BEGIN ALTER TABLE dbo.LabDEProvider  ADD[Guid] uniqueidentifier NOT NULL DEFAULT newid() END END \r\n"
  " IF(Exists(SELECT * FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'DataEntryCounterDefinition')) BEGIN IF COL_LENGTH('dbo.DataEntryCounterDefinition', 'GUID') IS NULL BEGIN ALTER TABLE dbo.DataEntryCounterDefinition  ADD[Guid] uniqueidentifier NOT NULL DEFAULT newid() END END \r\n";

static const string gstrCREATE_DATABASE_MIGRATION_WIZARD_REPORTING = 
	" IF(NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'ReportingDatabaseMigrationWizard')) "
	" BEGIN "
	" CREATE TABLE [dbo].[ReportingDatabaseMigrationWizard] "
	" ( "
	" [ID] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY "
	" , [Classification] NVARCHAR(128) NOT NULL DEFAULT 'Information' "
	" , [TableName] NVARCHAR(128) NOT NULL "
	" , [Message] NVARCHAR(512) NOT NULL "
	" , [DateTime] DATETIME NOT NULL DEFAULT GETDATE() "
	" , [Old_Value] NVARCHAR(MAX)"
	" , [New_Value] NVARCHAR(MAX)"
	" , [Command] NVARCHAR(64)"
	" )"
	" END ";

static const string gstrALTER_DATABASE_MIGRATION_WIZARD_REPORTING =
	" ALTER TABLE dbo.ReportingDatabaseMigrationWizard ADD Old_Value NVARCHAR(MAX);"
	" ALTER TABLE dbo.ReportingDatabaseMigrationWizard ADD New_Value NVARCHAR(MAX);"
	" ALTER TABLE dbo.ReportingDatabaseMigrationWizard ADD Command NVARCHAR(64); ";


static const string gstrCREATE_GET_CLUSTER_NAME_PROCEDURE =
"IF(													\r\n"
"	NOT EXISTS(											\r\n"
"		SELECT 1										\r\n"
"		FROM Information_schema.Routines				\r\n"
"		WHERE Specific_schema = 'dbo'					\r\n"
"		AND specific_name = 'sp_GetClusterName'			\r\n"
"		AND Routine_Type = 'Procedure'					\r\n"
"	)													\r\n"
")														\r\n"
"BEGIN													\r\n"
"   													\r\n"
"	EXEC ('												\r\n"
"	CREATE PROCEDURE[dbo].[sp_GetClusterName]  			\r\n"
"														\r\n"
"	AS													\r\n"
"	BEGIN												\r\n"
"		SET NOCOUNT ON;									\r\n"
"														\r\n"
"		SELECT cluster_name								\r\n"
"		FROM sys.dm_hadr_cluster;						\r\n"
"	END;')												\r\n"
"END;";

static const string gstrADD_METADATAFIELD_UNIQUE_NAME_CONSTRAINT =
"IF(                                                       \r\n"
"	NOT EXISTS(											\r\n"
"		SELECT 1										\r\n"
"		FROM Information_schema.TABLE_CONSTRAINTS		\r\n"
"		WHERE CONSTRAINT_NAME = 'IX_MetadataFieldName'	\r\n"
"	)													\r\n"
")														\r\n"
"BEGIN													\r\n"
"   ALTER TABLE [dbo].[MetadataField]                   \r\n"
"       ADD CONSTRAINT [IX_MetadataFieldName]           \r\n"
"    UNIQUE NONCLUSTERED ([Name] ASC)                   \r\n"
"END;";


static const string gstrCREATE_FAMUSER_INPUT_EVENTS_TIME_WITH_FILEID_VIEW =
"IF OBJECT_ID('[dbo].[vFAMUserInputWithFileID]', 'V') IS NULL \r\n"
"	EXECUTE( \r\n"
"'CREATE VIEW [dbo].[vFAMUserInputWithFileID]                                             \r\n"
"AS \r\n"
"SELECT dbo.FAMSession.FAMUserID \r\n"
"	,CAST(dbo.FileTaskSession.DateTimeStamp AS DATE) AS InputDate \r\n"
"	,SUM(dbo.FileTaskSession.ActivityTime / 60.0) AS TotalMinutes \r\n"
"	,dbo.FileTaskSession.FileID \r\n"
"FROM dbo.FAMSession \r\n"
"INNER JOIN dbo.FileTaskSession ON dbo.FAMSession.ID = dbo.FileTaskSession.FAMSessionID \r\n"
"INNER JOIN dbo.TaskClass ON dbo.FileTaskSession.TaskClassID = dbo.TaskClass.ID \r\n"
"WHERE ( \r\n"
"		dbo.TaskClass.GUID IN ( \r\n"
"			''FD7867BD-815B-47B5-BAF4-243B8C44AABB'' \r\n"
"			,''59496DF7-3951-49B7-B063-8C28F4CD843F'' \r\n"
"			,''AD7F3F3F-20EC-4830-B014-EC118F6D4567'' \r\n"
"			,''DF414AD2-742A-4ED7-AD20-C1A1C4993175'' \r\n"
"			,''8ECBCC95-7371-459F-8A84-A2AFF7769800'' \r\n"
"			) \r\n"
"		) \r\n"
"	AND (dbo.FileTaskSession.DateTimeStamp IS NOT NULL) \r\n"
"GROUP BY dbo.FAMSession.FAMUserID \r\n"
"	,CAST(dbo.FileTaskSession.DateTimeStamp AS DATE) \r\n"
"	,dbo.FileTaskSession.FileID') \r\n";
 
static const string gstrALTER_DATABASE_MIGRATION_WIZARD_REPORTING_COLUMN_SIZES =
	" ALTER TABLE dbo.ReportingDatabaseMigrationWizard "
	" ALTER COLUMN Old_Value NVARCHAR(MAX) "
	" ALTER TABLE dbo.ReportingDatabaseMigrationWizard "
	" ALTER COLUMN New_Value NVARCHAR(MAX) ";

static const string gstrALTER_FAMUSER_REMOVE_GUID =
" DECLARE @ConstraintName nvarchar(200) "
" SELECT @ConstraintName = Name FROM SYS.DEFAULT_CONSTRAINTS "
" WHERE PARENT_OBJECT_ID = OBJECT_ID('FAMUser') "
" AND PARENT_COLUMN_ID = (SELECT column_id FROM sys.columns "
" 	WHERE NAME = N'Guid' "
" 	AND object_id = OBJECT_ID(N'FAMUser')) "
" 	IF @ConstraintName IS NOT NULL "
" 	EXEC('ALTER TABLE FAMUser DROP CONSTRAINT ' + @ConstraintName) "

" 	IF EXISTS(SELECT 1 "
" 		FROM   INFORMATION_SCHEMA.COLUMNS "
" 		WHERE  TABLE_NAME = 'FAMUser' "
" 		AND COLUMN_NAME = 'Guid' "
" 		AND TABLE_SCHEMA = 'DBO') "
" 	BEGIN "
" 	ALTER TABLE FAMUser "
" 	DROP COLUMN[Guid] "
" 	END ";


static const string gstrCREATE_USAGE_FOR_SPECIFIC_USER_SPECIFIC_DAY_PROCEDURE =
"IF(																																	\r\n"
"	NOT EXISTS(																															\r\n"
"		SELECT 1																														\r\n"
"		FROM Information_schema.Routines																								\r\n"
"		WHERE Specific_schema = 'dbo'																									\r\n"
"		AND specific_name = 'sp_UsageForSpecificUserSpecificDay'																		\r\n"
"		AND Routine_Type = 'Procedure'																									\r\n"
"	)																																	\r\n"
")																																		\r\n"
"BEGIN																																	\r\n"
"   																																	\r\n"
"	EXEC ('																																\r\n"
"		CREATE PROCEDURE dbo.sp_UsageForSpecificUserSpecificDay																			\r\n"
"		@StartHour int,																													\r\n"
"		@EndHour int,																													\r\n"
"		@ReportDate DateTime,																											\r\n"
"		@UserName NVARCHAR(50)																											\r\n"
"		AS																																\r\n"
"		BEGIN																															\r\n"
"		-- SET NOCOUNT ON added to prevent extra result sets from																		\r\n"
"		-- interfering with SELECT statements.																							\r\n"
"		SET NOCOUNT ON;																													\r\n"
"																																		\r\n"
"		--Drop the temporary tables used for computation if they exist																	\r\n"
"		IF OBJECT_ID(N''#tempTable'', N''U'') IS NOT NULL																				\r\n"
"		DROP TABLE #tempTable																											\r\n"
"																																		\r\n"
"		-- Declare the temp table that will contain the hour data																		\r\n"
"		CREATE TABLE #tempTable(_hour INT NOT NULL, _0 BIT DEFAULT((0)), _1 BIT DEFAULT((0)),											\r\n"
"			_2 BIT DEFAULT((0)), _3 BIT DEFAULT((0)), _4 BIT DEFAULT((0)), _5 BIT DEFAULT((0)),											\r\n"
"			_6 BIT DEFAULT((0)), _7 BIT DEFAULT((0)), _8 BIT DEFAULT((0)), _9 BIT DEFAULT((0)),											\r\n"
"			_10 BIT DEFAULT((0)), _11 BIT DEFAULT((0)), _12 BIT DEFAULT((0)), _13 BIT DEFAULT((0)),										\r\n"
"			_14 BIT DEFAULT((0)), _15 BIT DEFAULT((0)), _16 BIT DEFAULT((0)), _17 BIT DEFAULT((0)),										\r\n"
"			_18 BIT DEFAULT((0)), _19 BIT DEFAULT((0)), _20 BIT DEFAULT((0)), _21 BIT DEFAULT((0)),										\r\n"
"			_22 BIT DEFAULT((0)), _23 BIT DEFAULT((0)), _24 BIT DEFAULT((0)), _25 BIT DEFAULT((0)),										\r\n"
"			_26 BIT DEFAULT((0)), _27 BIT DEFAULT((0)), _28 BIT DEFAULT((0)), _29 BIT DEFAULT((0)),										\r\n"
"			_30 BIT DEFAULT((0)), _31 BIT DEFAULT((0)), _32 BIT DEFAULT((0)), _33 BIT DEFAULT((0)),										\r\n"
"			_34 BIT DEFAULT((0)), _35 BIT DEFAULT((0)), _36 BIT DEFAULT((0)), _37 BIT DEFAULT((0)),										\r\n"
"			_38 BIT DEFAULT((0)), _39 BIT DEFAULT((0)), _40 BIT DEFAULT((0)), _41 BIT DEFAULT((0)),										\r\n"
"			_42 BIT DEFAULT((0)), _43 BIT DEFAULT((0)), _44 BIT DEFAULT((0)), _45 BIT DEFAULT((0)),										\r\n"
"			_46 BIT DEFAULT((0)), _47 BIT DEFAULT((0)), _48 BIT DEFAULT((0)), _49 BIT DEFAULT((0)),										\r\n"
"			_50 BIT DEFAULT((0)), _51 BIT DEFAULT((0)), _52 BIT DEFAULT((0)), _53 BIT DEFAULT((0)),										\r\n"
"			_54 BIT DEFAULT((0)), _55 BIT DEFAULT((0)), _56 BIT DEFAULT((0)), _57 BIT DEFAULT((0)),										\r\n"
"			_58 BIT DEFAULT((0)), _59 BIT DEFAULT((0)), TotalActiveCount AS CAST([_0] AS INT) + CAST([_1] AS INT) + CAST([_2] AS INT)	\r\n"
"			+ CAST([_3] AS INT) + CAST([_4] AS INT) + CAST([_5] AS INT) + CAST([_6] AS INT) + CAST([_7] AS INT) + CAST([_8] AS INT)		\r\n"
"			+ CAST([_9] AS INT) + CAST([_10] AS INT) + CAST([_11] AS INT) + CAST([_12] AS INT) + CAST([_13] AS INT)						\r\n"
"			+ CAST([_14] AS INT) + CAST([_15] AS INT) + CAST([_16] AS INT) + CAST([_17] AS INT) + CAST([_18] AS INT)					\r\n"
"			+ CAST([_19] AS INT) + CAST([_20] AS INT) + CAST([_21] AS INT) + CAST([_22] AS INT) + CAST([_23] AS INT)					\r\n"
"			+ CAST([_24] AS INT) + CAST([_25] AS INT) + CAST([_26] AS INT) + CAST([_27] AS INT) + CAST([_28] AS INT)					\r\n"
"			+ CAST([_29] AS INT) + CAST([_30] AS INT) + CAST([_31] AS INT) + CAST([_32] AS INT) + CAST([_33] AS INT)					\r\n"
"			+ CAST([_34] AS INT) + CAST([_35] AS INT) + CAST([_36] AS INT) + CAST([_37] AS INT) + CAST([_38] AS INT)					\r\n"
"			+ CAST([_39] AS INT) + CAST([_40] AS INT) + CAST([_41] AS INT) + CAST([_42] AS INT) + CAST([_43] AS INT)					\r\n"
"			+ CAST([_44] AS INT) + CAST([_45] AS INT) + CAST([_46] AS INT) + CAST([_47] AS INT) + CAST([_48] AS INT)					\r\n"
"			+ CAST([_49] AS INT) + CAST([_50] AS INT) + CAST([_51] AS INT) + CAST([_52] AS INT) + CAST([_53] AS INT)					\r\n"
"			+ CAST([_54] AS INT) + CAST([_55] AS INT) + CAST([_56] AS INT) + CAST([_57] AS INT) + CAST([_58] AS INT)					\r\n"
"			+ CAST([_59] AS INT))																										\r\n"
"																																		\r\n"
"			--Insert the hours into the table(this will default all minutes to non - active)											\r\n"
"			--Insert the hours into the table(this will default all minutes to non - active)											\r\n"
"		; WITH HourValues([Hour]) AS																									\r\n"
"		(																																\r\n"
"			--Anchor																													\r\n"
"			SELECT @StartHour AS[Hour]																									\r\n"
"			UNION ALL																													\r\n"
"			SELECT[Hour] + 1 AS[Hour] FROM HourValues WHERE[Hour] < @EndHour															\r\n"
"		)																																\r\n"
"																																		\r\n"
"		INSERT INTO #tempTable(_hour)																									\r\n"
"		SELECT[Hour] FROM HourValues																									\r\n"
"																																		\r\n"
"		-- Select the hoursand minutes from the input event table																		\r\n"
"		DECLARE rowCursor CURSOR FOR																									\r\n"
"		SELECT DATEPART(hour, [TimeStamp]) AS _hour,																					\r\n"
"		DATEPART(minute, [TimeStamp]) AS _minute,																						\r\n"
"		[SecondsWithInputEvents] FROM[InputEvent] INNER JOIN[FAMUser] ON[InputEvent].[FAMUserID] = [FAMUser].[ID]						\r\n"
"		WHERE DATEDIFF(d, [TimeStamp], @ReportDate) = 0																					\r\n"
"		AND[FAMUser].[UserName] = @UserName																								\r\n"
"																																		\r\n"
"		DECLARE @hour INT, @minute INT, @count INT, @sqlExpression NVARCHAR(MAX)														\r\n"
"		OPEN rowCursor																													\r\n"
"		FETCH NEXT FROM rowCursor INTO @hour, @minute, @count																			\r\n"
"		WHILE @@FETCH_STATUS = 0																										\r\n"
"		BEGIN																															\r\n"
"			SELECT @sqlExpression = ''UPDATE #tempTable SET _'' + CAST(@minute as NVARCHAR(mAX)) + 										\r\n"
"				'' = 1 WHERE _hour = '' + CAST(@hour AS NVARCHAR(2));																		\r\n"
"			EXEC(@sqlExpression);																										\r\n"
"			FETCH NEXT FROM rowCursor INTO @hour, @minute, @count																		\r\n"
"		END																																\r\n"
"		CLOSE rowCursor																													\r\n"
"		DEALLOCATE rowCursor																											\r\n"
"																																		\r\n"
"		SELECT* FROM #tempTable																											\r\n"
"																																		\r\n"
"		DROP TABLE #tempTable																											\r\n"
"																																		\r\n"
"		END																																\r\n"
"		')																																\r\n"
"END																																	\r\n";

static string gstrCREATE_TABLE_FROM_COMMA_SEPARATED_LIST_FUNCTION =
	"IF(																																\r\n"
	"	NOT EXISTS(																														\r\n"
	"		SELECT 1																													\r\n"
	"		FROM Information_schema.Routines																							\r\n"
	"		WHERE Specific_schema = 'dbo'																								\r\n"
	"		AND specific_name = 'fn_TableFromCommaSeparatedList'																		\r\n"
	"		AND Routine_Type = 'Function'																								\r\n"
	"	)																																\r\n"
	")																																	\r\n"
	"BEGIN																																\r\n"
	"EXEC('																																\r\n"
	"	CREATE FUNCTION[dbo].[fn_TableFromCommaSeparatedList]																			\r\n"
	"	(																																\r\n"
	"		@List NVARCHAR(MAX)																											\r\n"
	"	)																																\r\n"
	"	RETURNS																															\r\n"
	"		@TableOfValues TABLE(ItemValue NVARCHAR(max))																				\r\n"
	"	AS																																\r\n"
	"	BEGIN																															\r\n"
	"		-- Fill the table variable with the rows for your result set																\r\n"
	"		DECLARE @table_names xml;																									\r\n"
	"																																	\r\n"
	"		SELECT @table_names =																										\r\n"
	"			CAST('' < XMLRoot > < ItemValue>'' +																					\r\n"
	"			REPLACE(@List, '', '', '' < / ItemValue > < ItemValue>'') + '' < / ItemValue > < / XMLRoot>'' AS XML)					\r\n"
	"																																	\r\n"
	"		INSERT INTO @TableOfValues																									\r\n"
	"		SELECT ltrim(Rtrim(Replace(Replace(a.r.query(''.'').value(''.'', ''NVARCHAR(max)''), char(13), ''''), char(10), '''')))		\r\n"
	"			as ItemValue from @Table_names.nodes('' / XMLRoot / ItemValue'') a(r)													\r\n"
	"																																	\r\n"
	"		DELETE FROM @TableOfValues WHERE ItemValue IS NULL OR ItemValue = ''''														\r\n"
	"																																	\r\n"
	"	RETURN																															\r\n"
	"	END																																\r\n"
	"	')																																\r\n"
	"END";

static string gstrCREATE_USER_COUNTS_STORED_PROCEDURE =
	"IF(																																																 \r\n"
	"	NOT EXISTS(													 \r\n"
	"		SELECT 1												 \r\n"
	"		FROM Information_schema.Routines						 \r\n"
	"		WHERE Specific_schema = 'dbo'							 \r\n"
	"		AND specific_name = 'sp_UserCounts' \r\n"
	"		AND Routine_Type = 'Procedure'							 \r\n"
	"	)															 \r\n"
	")																 \r\n"
	"BEGIN															 \r\n"
	"   															 \r\n"
	"	EXEC ('														 \r\n"
	"		CREATE PROCEDURE [dbo].[sp_UserCounts] ( \r\n"
	"			-- Add the parameters for the function here \r\n"
	"			@InclusionActions NVARCHAR(MAX) \r\n"
	"			,@ExclusionTags NVARCHAR(MAX) \r\n"
	"			,@WorkflowName NVARCHAR(100) \r\n"
	"			,@ReportingPeriod_Min DATETIME \r\n"
	"			,@ReportingPeriod_Max DATETIME \r\n"
	"			) \r\n"
	"		AS \r\n"
	"		BEGIN \r\n"
	"			-- Declare a table of action IDs that should be included in the report. \r\n"
	"			DECLARE @includedActions TABLE (ID INT) \r\n"
	"		 \r\n"
	"			INSERT INTO @includedActions \r\n"
	"			SELECT [Action].[ID] \r\n"
	"			FROM [Action] \r\n"
	"			LEFT JOIN [Workflow] ON [Workflow].ID = [Action].WorkflowID \r\n"
	"			WHERE ( \r\n"
	"					@InclusionActions IS NULL \r\n"
	"					OR @InclusionActions = '''' \r\n"
	"					OR [ASCName] IN ( \r\n"
	"						SELECT ItemValue \r\n"
	"						FROM [dbo].[fn_TableFromCommaSeparatedList](@InclusionActions) \r\n"
	"						) \r\n"
	"					) \r\n"
	"				AND ( \r\n"
	"					[Workflow].[Name] = @WorkflowName \r\n"
	"					OR @WorkflowName = '''' \r\n"
	"					OR @WorkflowName IS NULL \r\n"
	"					) \r\n"
	"		 \r\n"
	"			-- Declare a table of tag IDs to be excluded from the report. \r\n"
	"			DECLARE @excludedTags TABLE (ID INT) \r\n"
	"		 \r\n"
	"			INSERT INTO @excludedTags \r\n"
	"			SELECT [ID] \r\n"
	"			FROM [Tag] \r\n"
	"			WHERE [TagName] IN ( \r\n"
	"					SELECT ItemValue \r\n"
	"					FROM [dbo].[fn_TableFromCommaSeparatedList](@ExclusionTags) \r\n"
	"					) \r\n"
	"		 \r\n"
	"			-- Declare a local table to hold the count data \r\n"
	"			DECLARE @countTable TABLE ( \r\n"
	"				FileTaskSessionID INT \r\n"
	"				,FileID INT \r\n"
	"				,FAMUserID INT \r\n"
	"				,TotalDuration FLOAT \r\n"
	"				,DocCount INT \r\n"
	"				,PageCount INT \r\n"
	"				,OrderCount INT \r\n"
	"				,TestCount INT \r\n"
	"				) \r\n"
	"		 \r\n"
	"			-- For each unique FileID in DataEntryData, select the most recent entry (from which \r\n"
	"			-- counts will be obtained), and aggregate the total time spent on each file (per user). \r\n"
	"			INSERT INTO @countTable ( \r\n"
	"				FileTaskSessionID \r\n"
	"				,FileID \r\n"
	"				,FAMUserID \r\n"
	"				,TotalDuration \r\n"
	"				) \r\n"
	"			SELECT ( \r\n"
	"					-- For each file, find the last DataEntryData row for which there were saved counts to use \r\n"
	"					-- as the counts for the file. If no such row can be found, just use the last row for the \r\n"
	"					-- file, but only if that row is from the current user. \r\n"
	"					SELECT ID \r\n"
	"					FROM ( \r\n"
	"						SELECT MAX([FileTaskSession].[ID]) AS ID \r\n"
	"						FROM [FileTaskSession] \r\n"
	"						INNER JOIN FAMSession ON FAMSession.ID = FileTaskSession.FAMSessionID \r\n"
	"						INNER JOIN [DataEntryCounterValue] ON [DataEntryCounterValue].[InstanceID] = [FileTaskSession].[ID] \r\n"
	"						INNER JOIN [DataEntryCounterDefinition] ON [DataEntryCounterDefinition].[ID] = [DataEntryCounterValue].[CounterID] \r\n"
	"						INNER JOIN TaskClass ON FileTaskSession.TaskClassID = TaskClass.ID \r\n"
	"						LEFT JOIN [Action] ON [FileTaskSession].ActionID = [Action].ID \r\n"
	"						LEFT JOIN Workflow ON Workflow.ID = [Action].WorkflowID \r\n"
	"						WHERE [FileTaskSession].[FileID] = [OuterFileTaskSession].FileID \r\n"
	"							-- Limit the selection to the included actions and tags \r\n"
	"							AND [FileTaskSession].[ID] IN ( \r\n"
	"								SELECT [ID] \r\n"
	"								FROM [FileTaskSession] \r\n"
	"								LEFT JOIN [FileTag] ON [FileTaskSession].[FileID] = [FileTag].[FileID] \r\n"
	"									AND [FileTag].[TagID] IN ( \r\n"
	"										SELECT [ID] \r\n"
	"										FROM @excludedTags \r\n"
	"										) \r\n"
	"								WHERE [FileTaskSession].[FileID] = [OuterFileTaskSession].[FileID] \r\n"
	"									AND [TagID] IS NULL \r\n"
	"								) \r\n"
	"							AND [FileTaskSession].[ActionID] IN ( \r\n"
	"								SELECT [ID] \r\n"
	"								FROM @includedActions \r\n"
	"								) \r\n"
	"							-- Limit the selection to DataEntryData rows where counts were saved. \r\n"
	"							AND ( \r\n"
	"								[DataEntryCounterDefinition].[Name] = ''NumOrders'' \r\n"
	"								OR [DataEntryCounterDefinition].[Name] = ''NumTests'' \r\n"
	"								) \r\n"
	"							AND [Type] = ''S'' \r\n"
	"							AND Value IS NOT NULL \r\n"
	"							AND TaskClass.GUID = ''59496DF7-3951-49B7-B063-8C28F4CD843F'' \r\n"
	"							AND ( \r\n"
	"								[Workflow].[Name] = @WorkflowName \r\n"
	"								OR @WorkflowName = '''' \r\n"
	"								OR @WorkflowName IS NULL \r\n"
	"								) \r\n"
	"							-- Do not limit by time here. That way, if counts for a file are saved multiple times, \r\n"
	"							-- but the last time comes outside of the time range, the counts are not included \r\n"
	"							-- based on the initial save. (Because in this case, the counts would also be included \r\n"
	"							-- in a report for the following period, thus, double-counting) \r\n"
	"						) AS FileTaskSessionID \r\n"
	"					WHERE OuterFAMSession.[FAMUserID] IN ( \r\n"
	"							SELECT FAMUserID \r\n"
	"							FROM FileTaskSession \r\n"
	"							INNER JOIN FAMSession ON FAMSession.ID = FileTaskSession.FAMSessionID \r\n"
	"							LEFT JOIN [Action] ON [FileTaskSession].ActionID = [Action].ID \r\n"
	"							LEFT JOIN Workflow ON Workflow.ID = [Action].WorkflowID \r\n"
	"							WHERE FileTaskSession.ID = FileTaskSessionID.ID \r\n"
	"								AND ( \r\n"
	"									[Workflow].[Name] = @WorkflowName \r\n"
	"									OR @WorkflowName = '''' \r\n"
	"									OR @WorkflowName IS NULL \r\n"
	"									) \r\n"
	"							) \r\n"
	"						AND ( \r\n"
	"							SELECT [DateTimeStamp] \r\n"
	"							FROM FileTaskSession \r\n"
	"							LEFT JOIN [Action] ON [FileTaskSession].ActionID = [Action].ID \r\n"
	"							LEFT JOIN Workflow ON Workflow.ID = [Action].WorkflowID \r\n"
	"							WHERE FileTaskSession.ID = FileTaskSessionID.ID \r\n"
	"								AND ( \r\n"
	"									[Workflow].[Name] = @WorkflowName \r\n"
	"									OR @WorkflowName = '''' \r\n"
	"									OR @WorkflowName IS NULL \r\n"
	"									) \r\n"
	"							) BETWEEN @ReportingPeriod_Min \r\n"
	"							AND @ReportingPeriod_Max \r\n"
	"					) \r\n"
	"				,[OuterFileTaskSession].[FileID] \r\n"
	"				,OuterFAMSession.[FAMUserID] \r\n"
	"				,SUM(ActivityTime) + SUM(OverheadTime) \r\n"
	"			FROM FileTaskSession AS [OuterFileTaskSession] \r\n"
	"			INNER JOIN FAMSession AS OuterFAMSession ON [OuterFileTaskSession].FAMSessionID = OuterFAMSession.ID \r\n"
	"			INNER JOIN TaskClass ON [OuterFileTaskSession].TaskClassID = TaskClass.ID \r\n"
	"			LEFT JOIN [Action] ON [OuterFileTaskSession].ActionID = [Action].ID \r\n"
	"			LEFT JOIN Workflow ON Workflow.ID = [Action].WorkflowID \r\n"
	"			-- Limit the selection to the included actions and tags \r\n"
	"			WHERE [OuterFileTaskSession].[ID] IN ( \r\n"
	"					SELECT [ID] \r\n"
	"					FROM FileTaskSession \r\n"
	"					LEFT JOIN [FileTag] ON FileTaskSession.[FileID] = [FileTag].[FileID] \r\n"
	"						AND [FileTag].[TagID] IN ( \r\n"
	"							SELECT [ID] \r\n"
	"							FROM @excludedTags \r\n"
	"							) \r\n"
	"					WHERE FileTaskSession.[FileID] = [OuterFileTaskSession].[FileID] \r\n"
	"						AND [TagID] IS NULL \r\n"
	"					) \r\n"
	"				AND [OuterFileTaskSession].[ActionID] IN ( \r\n"
	"					SELECT [ID] \r\n"
	"					FROM @includedActions \r\n"
	"					) \r\n"
	"				AND [DateTimeStamp] BETWEEN @ReportingPeriod_Min \r\n"
	"					AND @ReportingPeriod_Max \r\n"
	"				AND TaskClass.GUID = ''59496DF7-3951-49B7-B063-8C28F4CD843F'' \r\n"
	"				AND ( \r\n"
	"					[Workflow].[Name] = @WorkflowName \r\n"
	"					OR @WorkflowName = '''' \r\n"
	"					OR @WorkflowName IS NULL \r\n"
	"					) \r\n"
	"			GROUP BY [OuterFileTaskSession].[FileID] \r\n"
	"				,OuterFAMSession.[FAMUserID] \r\n"
	"		 \r\n"
	"			-- Populate the counts using the DataEntryCounterValue table based on the DataEntryID \r\n"
	"			-- determined to be operative for each file. \r\n"
	"			UPDATE @countTable \r\n"
	"			SET [@countTable].OrderCount = COALESCE([DataEntryCounterValue].Value, 0) \r\n"
	"			FROM @countTable \r\n"
	"			INNER JOIN [DataEntryCounterValue] ON [@countTable].FileTaskSessionID = [DataEntryCounterValue].InstanceID \r\n"
	"			INNER JOIN [DataEntryCounterDefinition] ON [DataEntryCounterDefinition].ID = [DataEntryCounterValue].CounterID \r\n"
	"			WHERE [DataEntryCounterDefinition].Name = ''NumOrders'' \r\n"
	"				AND [DataEntryCounterValue].InstanceID = [@countTable].FileTaskSessionID \r\n"
	"				AND [DataEntryCounterValue].[Type] = ''S'' \r\n"
	"		 \r\n"
	"			UPDATE @countTable \r\n"
	"			SET [@countTable].TestCount = COALESCE([DataEntryCounterValue].Value, 0) \r\n"
	"			FROM @countTable \r\n"
	"			INNER JOIN [DataEntryCounterValue] ON [@countTable].FileTaskSessionID = [DataEntryCounterValue].InstanceID \r\n"
	"			INNER JOIN [DataEntryCounterDefinition] ON [DataEntryCounterDefinition].ID = [DataEntryCounterValue].CounterID \r\n"
	"			WHERE [DataEntryCounterDefinition].Name = ''NumTests'' \r\n"
	"				AND [DataEntryCounterValue].InstanceID = [@countTable].FileTaskSessionID \r\n"
	"				AND [DataEntryCounterValue].[Type] = ''S'' \r\n"
	"		 \r\n"
	"			UPDATE @countTable \r\n"
	"			SET [DocCount] = 1 \r\n"
	"				,[PageCount] = [Pages] \r\n"
	"			FROM @countTable \r\n"
	"			INNER JOIN [FAMFile] ON [@countTable].[FileID] = [FAMFile].[ID] \r\n"
	"			WHERE [OrderCount] IS NOT NULL \r\n"
	"		 \r\n"
	"			UPDATE @countTable \r\n"
	"			SET [OrderCount] = 0 \r\n"
	"				,[TestCount] = 0 \r\n"
	"				,[DocCount] = 0 \r\n"
	"				,[PageCount] = 0 \r\n"
	"			FROM @countTable \r\n"
	"			WHERE [OrderCount] IS NULL \r\n"
	"		 \r\n"
	"			-- Return a select query containing the counts, along with the page counts totaled by user \r\n"
	"			SELECT UserName \r\n"
	"				,SUM([DocCount]) AS NumDocs \r\n"
	"				,SUM([PageCount]) AS NumPages \r\n"
	"				,SUM([OrderCount]) AS NumOrders \r\n"
	"				,SUM([TestCount]) AS NumTests \r\n"
	"				,SUM([TotalDuration]) AS TotalTime \r\n"
	"				,(SUM([DocCount]) / (SUM([TotalDuration]) / 3600.0)) AS DocsPerHour \r\n"
	"				,(SUM([PageCount]) / (SUM([TotalDuration]) / 3600.0)) AS PagesPerHour \r\n"
	"				,(SUM([OrderCount]) / (SUM([TotalDuration]) / 3600.0)) AS OrdersPerHour \r\n"
	"				,(SUM([TestCount]) / (SUM([TotalDuration]) / 3600.0)) AS TestPerHour \r\n"
	"			FROM @countTable \r\n"
	"			INNER JOIN FAMFile ON [@countTable].FileID = [FAMFile].ID \r\n"
	"			INNER JOIN [FAMUser] ON [@countTable].FAMUserID = [FAMUser].ID \r\n"
	"			GROUP BY UserName \r\n"
	"		END \r\n"
	"	') \r\n"
	"END \r\n";


static string gstrCREATE_GET_FILES_TO_PROCESS_STORED_PROCEDURE =
"IF(																																																 \r\n"
"		EXISTS(													 \r\n"
"		SELECT 1												 \r\n"
"		FROM Information_schema.Routines						 \r\n"
"		WHERE Specific_schema = 'dbo'							 \r\n"
"		AND specific_name = 'GetFilesToProcessForActionID' \r\n"
"		AND Routine_Type = 'Procedure'							 \r\n"
"	)															 \r\n"
")																 \r\n"
"BEGIN															 \r\n"
"	DROP PROCEDURE [dbo].[GetFilesToProcessForActionID]\r\n"
"END   															 \r\n"
"EXEC ('														 \r\n"
"-- =============================================\r\n"
"-- Author:		William Parr\r\n"
"-- Create date: Dec 14, 2020\r\n"
"-- Description:	Gets files to process next \r\n"
"-- =============================================\r\n"
"CREATE PROCEDURE [dbo].[GetFilesToProcessForActionID] @ActionID INT\r\n"
"	,@ActionName NVARCHAR(50)\r\n"
"	,@BatchSize INT\r\n"
"	,@StatusToQueue NVARCHAR(1)\r\n"
"	,@MachineID INT\r\n"
"	,@UserID INT\r\n"
"	,@ActiveFAMID INT\r\n"
"	,@FAMSessionID INT\r\n"
"	,@RecordFASTEntry BIT\r\n"
"	,@SkippedForUser NVARCHAR(50)\r\n"
"AS\r\n"
"BEGIN\r\n"
"	-- SET NOCOUNT ON added to prevent extra result sets from\r\n"
"	-- interfering with SELECT statements.\r\n"
"	SET NOCOUNT ON;\r\n"
"\r\n"
"	BEGIN TRY\r\n"
"		DECLARE @SelectedFiles TABLE (\r\n"
"			[ActionID] [int] NOT NULL\r\n"
"			,[FileID] [int] NOT NULL\r\n"
"			,[ActionStatus] [nvarchar](1) NOT NULL\r\n"
"			,[Priority] [int] NOT NULL\r\n"
"			)\r\n"
"		DECLARE @WorkflowID INT;\r\n"
"		\r\n"
"		SELECT @WorkflowID = COALESCE(WorkflowID, 0) FROM Action WITH (NOLOCK) WHERE ID = @ActionID\r\n"
"		\r\n"
"		IF (@StatusToQueue = ''S'')\r\n"
"		BEGIN\r\n"
"				;\r\n"
"\r\n"
"			WITH FileQueue\r\n"
"			AS (\r\n"
"				SELECT TOP (@BatchSize) FileActionStatus.*\r\n"
"				FROM FileActionStatus WITH (\r\n"
"						UPDLOCK\r\n"
"						,ROWLOCK\r\n"
"						,READPAST\r\n"
"						)\r\n"
"				INNER JOIN SkippedFile WITH (NOLOCK) ON FileActionStatus.ActionID = SkippedFile.ActionID\r\n"
"					AND FileActionStatus.FileID = SkippedFile.FileID\r\n"
"				LEFT JOIN WorkflowFile WITH (NOLOCK) ON WorkflowFile.FileID = FileActionStatus.FileID AND WorkflowFile.WorkflowID = @WorkflowID\r\n"
"				WHERE FileActionStatus.ActionStatus = @StatusToQueue\r\n"
"					AND FileActionStatus.ActionID IN (@ActionID)\r\n"
"					AND (\r\n"
"						@SkippedForUser = ''''\r\n"
"						OR SkippedFile.UserName = @SkippedForUser\r\n"
"						)\r\n"
"					AND (\r\n"
"						SkippedFile.FAMSessionID <> @FAMSessionID\r\n"
"						OR SkippedFile.FAMSessionID IS NULL\r\n"
"						)\r\n"
"					AND (WorkflowFile.WorkflowID IS NULL OR WorkflowFile.Deleted = 0)\r\n"
"				ORDER BY FileActionStatus.ActionStatus\r\n"
"					,FileActionStatus.ActionID\r\n"
"					,FileActionStatus.Priority\r\n"
"					,FileActionStatus.FileID\r\n"
"				)\r\n"
"			UPDATE FileQueue\r\n"
"			SET ActionStatus = ''R''\r\n"
"			OUTPUT Inserted.ActionID\r\n"
"				,Inserted.FileID\r\n"
"				,Inserted.ActionStatus\r\n"
"				,Inserted.Priority\r\n"
"			INTO @SelectedFiles\r\n"
"		END\r\n"
"		ELSE\r\n"
"		BEGIN\r\n"
"				;\r\n"
"\r\n"
"			WITH FileQueue\r\n"
"			AS (\r\n"
"				SELECT TOP (@BatchSize) FileActionStatus.*\r\n"
"				FROM FileActionStatus WITH (\r\n"
"						UPDLOCK\r\n"
"						,ROWLOCK\r\n"
"						,READPAST\r\n"
"						)\r\n"
"						LEFT JOIN WorkflowFile WITH (NOLOCK) ON WorkflowFile.FileID = FileActionStatus.FileID AND WorkflowFile.WorkflowID = @WorkflowID\r\n"
"				WHERE FileActionStatus.ActionStatus = @StatusToQueue\r\n"
"					AND FileActionStatus.ActionID IN (@ActionID)\r\n"
"					AND (WorkflowFile.WorkflowID IS NULL OR WorkflowFile.Deleted = 0)\r\n"
"				ORDER BY ActionStatus\r\n"
"					,FileActionStatus.ActionID\r\n"
"					,FileActionStatus.Priority\r\n"
"					,FileActionStatus.FileID\r\n"
"				)\r\n"
"			UPDATE FileQueue\r\n"
"			SET ActionStatus = ''R''\r\n"
"			OUTPUT Inserted.ActionID\r\n"
"				,Inserted.FileID\r\n"
"				,Inserted.ActionStatus\r\n"
"				,Inserted.Priority\r\n"
"			INTO @SelectedFiles\r\n"
"		END\r\n"
"\r\n"
"		INSERT INTO LockedFile (\r\n"
"			FileID\r\n"
"			,ActionID\r\n"
"			,StatusBeforeLock\r\n"
"			,ActiveFAMID\r\n"
"			,ActionName\r\n"
"			)\r\n"
"		SELECT FileID\r\n"
"			,ActionID\r\n"
"			,@StatusToQueue AS StatusBeforeLck\r\n"
"			,@ActiveFAMID\r\n"
"			,@ActionName\r\n"
"		FROM @SelectedFiles\r\n"
"\r\n"
"		IF 1 = @RecordFASTEntry\r\n"
"		BEGIN\r\n"
"			INSERT INTO FileActionStateTransition (\r\n"
"				FileID\r\n"
"				,ActionID\r\n"
"				,ASC_From\r\n"
"				,ASC_To\r\n"
"				,DateTimeStamp\r\n"
"				,FAMUserId\r\n"
"				,MachineID\r\n"
"				,Exception\r\n"
"				,Comment\r\n"
"				)\r\n"
"			SELECT FileId\r\n"
"				,ActionID\r\n"
"				,@StatusToQueue\r\n"
"				,''R''\r\n"
"				,GETDATE()\r\n"
"				,@Userid\r\n"
"				,@MachineID\r\n"
"				,''''\r\n"
"				,''''\r\n"
"			FROM @SelectedFiles\r\n"
"		END\r\n"
"\r\n"
"		UPDATE [QueuedActionStatusChange]\r\n"
"		SET [ChangeStatus] = ''I''\r\n"
"		WHERE [ChangeStatus] = @StatusToQueue\r\n"
"			AND [ActionID] = @ActionID\r\n"
"			AND [FileID] IN (\r\n"
"				SELECT FileID\r\n"
"				FROM @SelectedFiles\r\n"
"				)\r\n"
"\r\n"
"		INSERT INTO [dbo].[ActionStatisticsDelta] (\r\n"
"			[ActionID]\r\n"
"			,[NumDocuments]\r\n"
"			,[NumDocumentsPending]\r\n"
"			,[NumDocumentsComplete]\r\n"
"			,[NumDocumentsFailed]\r\n"
"			,[NumDocumentsSkipped]\r\n"
"			,[NumPages]\r\n"
"			,[NumPagesPending]\r\n"
"			,[NumPagesComplete]\r\n"
"			,[NumPagesFailed]\r\n"
"			,[NumPagesSkipped]\r\n"
"			,[NumBytes]\r\n"
"			,[NumBytesPending]\r\n"
"			,[NumBytesComplete]\r\n"
"			,[NumBytesFailed]\r\n"
"			,[NumBytesSkipped]\r\n"
"			)\r\n"
"		SELECT Selected.ActionID\r\n"
"			,0\r\n"
"			,CASE @StatusToQueue\r\n"
"				WHEN ''P''\r\n"
"					THEN - COUNT(Selected.FileID)\r\n"
"				ELSE 0\r\n"
"				END\r\n"
"			,0\r\n"
"			,0\r\n"
"			,CASE @StatusToQueue\r\n"
"				WHEN ''S''\r\n"
"					THEN - COUNT(Selected.FileID)\r\n"
"				ELSE 0\r\n"
"				END\r\n"
"			,0\r\n"
"			,CASE @StatusToQueue\r\n"
"				WHEN ''P''\r\n"
"					THEN - SUM(FAMFile.Pages)\r\n"
"				ELSE 0\r\n"
"				END\r\n"
"			,0\r\n"
"			,0\r\n"
"			,CASE @StatusToQueue\r\n"
"				WHEN ''S''\r\n"
"					THEN - SUM(FAMFile.Pages)\r\n"
"				ELSE 0\r\n"
"				END\r\n"
"			,0\r\n"
"			,CASE @StatusToQueue\r\n"
"				WHEN ''P''\r\n"
"					THEN - SUM(FAMFile.FileSize)\r\n"
"				ELSE 0\r\n"
"				END\r\n"
"			,0\r\n"
"			,0\r\n"
"			,CASE @StatusToQueue\r\n"
"				WHEN ''S''\r\n"
"					THEN - SUM(FAMFile.FileSize)\r\n"
"				ELSE 0\r\n"
"				END\r\n"
"		FROM @SelectedFiles Selected\r\n"
"		INNER JOIN FAMFile WITH (NOLOCK) ON FAMFile.ID = Selected.FileID\r\n"
"		GROUP BY Selected.ActionID\r\n"
"	END TRY\r\n"
"\r\n"
"	BEGIN CATCH\r\n"
"		DECLARE @ErrorMessage NVARCHAR(4000);\r\n"
"		DECLARE @ErrorSeverity INT;\r\n"
"		DECLARE @ErrorState INT;\r\n"
"\r\n"
"		SELECT @ErrorMessage = ERROR_MESSAGE()\r\n"
"			,@ErrorSeverity = ERROR_SEVERITY()\r\n"
"			,@ErrorState = ERROR_STATE();\r\n"
"\r\n"
"		IF @ErrorState = 0\r\n"
"			SELECT @ErrorState = 1\r\n"
"\r\n"
"		RAISERROR (\r\n"
"				@ErrorMessage\r\n"
"				,@ErrorSeverity\r\n"
"				,@ErrorState\r\n"
"				);\r\n"
"	END CATCH\r\n"
"\r\n"
"	SELECT FAMFile.ID\r\n"
"		,FileName\r\n"
"		,FileSize\r\n"
"		,Pages\r\n"
"		,Selected.Priority\r\n"
"		,Selected.ActionID\r\n"
"		,WorkflowID\r\n"
"		,@StatusToQueue [ASC_From]\r\n"
"	FROM @SelectedFiles Selected\r\n"
"	INNER JOIN Action  WITH (NOLOCK) ON ActionID = Action.ID\r\n"
"	INNER JOIN FAMFile  WITH (NOLOCK) ON FAMFile.ID = Selected.FileID\r\n"
"END\r\n"
"')";
