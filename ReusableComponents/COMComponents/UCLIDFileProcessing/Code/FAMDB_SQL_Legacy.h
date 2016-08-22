#pragma once

// This file contains const SQL query definitions that are no longer used in the current schema, but
// need to be used as part of the schema update process.

#include <string>


// The ProcessingFAM table is now the ActiveFAM table, but this definition needs to remain for the
// schema update process.
static const std::string gstrCREATE_PROCESSING_FAM_TABLE_V101 = 
	"CREATE TABLE [dbo].[ProcessingFAM]([ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_ProcessingFAM] PRIMARY KEY CLUSTERED, "
	"[ActionID] [int] NOT NULL, "
	"[UPI] [nvarchar](450), "
	"[LastPingTime] datetime NOT NULL CONSTRAINT [DF_ProcessingFAM_LastPingTime]  DEFAULT (GETDATE()))";

// For use in updating from version 127 and before
static const string gstrCREATE_ACTIVE_FAM_TABLE_V110 = 
	"CREATE TABLE [dbo].[ActiveFAM]([ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_ActiveFAM] PRIMARY KEY CLUSTERED, "
	"[ActionID] [int] NOT NULL, "
	"[UPI] [nvarchar](450), "
	"[LastPingTime] datetime NOT NULL CONSTRAINT [DF_ActiveFAM_LastPingTime]  DEFAULT (GETUTCDATE()),"
	"[Queuing] [bit] NOT NULL,"
	"[Processing] [bit] NOT NULL)";

// For use in updating from version 127 and before
static const std::string gstrCREATE_QUEUED_ACTION_STATUS_CHANGE_TABLE_V113 =
	"CREATE TABLE [dbo].[QueuedActionStatusChange]("
	"[ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_QueuedActionStatusChange] PRIMARY KEY CLUSTERED, "
	"[FileID] [int] NULL, "
	"[ActionID] [int] NULL, "
	"[ASC_To] [nvarchar](1) NOT NULL, "
	"[DateTimeStamp] [datetime] NULL,"
	"[MachineID] int NOT NULL, "
	"[FAMUserID] int NOT NULL, "
	"[UPI] [nvarchar](450) NULL, "	
	"[ChangeStatus][nvarchar](1) NOT NULL)";

// FileHandlers table used to be named LaunchApp; LaunchApp table definition needs to be kept
// for the schema update process.
static const std::string gstrCREATE_LAUNCH_APP_TABLE_V114 =
	"CREATE TABLE [dbo].[LaunchApp]("
	"[ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_LaunchApp] PRIMARY KEY CLUSTERED,"
	"[Enabled] [bit] NOT NULL DEFAULT 1,"
	"[AppName] [nvarchar](64) NOT NULL UNIQUE,"
	"[IconPath] [nvarchar](260),"
	"[ApplicationPath] [nvarchar](260) NOT NULL,"
	"[Arguments] [ntext],"
	"[AdminOnly] [bit] NOT NULL DEFAULT 0,"
	"[AllowMultipleFiles] [bit] NOT NULL DEFAULT 0,"
	"[SupportsErrorHandling] [bit] NOT NULL DEFAULT 0,"
	"[Blocking] [bit] NOT NULL DEFAULT 1)";

// For use in updating from version 125 and before
static const std::string gstrCREATE_WORK_ITEM_GROUP_TABLE_V118 =
	"CREATE TABLE [dbo].[WorkItemGroup]("
	"[ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_WorkItemGroup] PRIMARY KEY CLUSTERED,"
	"[FileID] [int] NOT NULL,"
	"[ActionID] [int] NOT NULL,"
	"[StringizedSettings] [nvarchar](MAX) NULL,"
	"[UPI] [nvarchar](450) NULL, "
	"[NumberOfWorkItems] [int] NOT NULL)";

// For use in updating from version 127 and before
static const std::string gstrCREATE_ACTIVE_FAM_UPI_INDEX_V110 = "CREATE UNIQUE NONCLUSTERED INDEX "
	"[IX_ActiveFAM_UPI] ON [ActiveFAM]([UPI])";

// No longer part of the database; definition for schema update use only.
static const std::string gstrCREATE_FILE_ACTION_STATUS_ACTION_ACTIONSTATUS_INDEX_V101 = 
	"CREATE NONCLUSTERED INDEX "
	"[IX_FileActionStatus_ActionID_ActionStatus] ON [dbo].[FileActionStatus] "
	"([ActionID] ASC, [ActionStatus] ASC)";

// For use in updating from version 127 and before
static const std::string gstrCREATE_WORK_ITEM_GROUP_UPI_INDEX_V118 =
	"CREATE NONCLUSTERED INDEX "
	"[IX_WorkItemGroupUPI] ON [WorkItemGroup]([UPI])";

// For use in updating from version 127 and before
static const std::string gstrCREATE_WORK_ITEM_UPI_INDEX_V118 =
	"CREATE NONCLUSTERED INDEX "
	"[IX_WorkItemUPI] ON [WorkItem]([UPI])";

// The ProcessingFAM table is now the ActiveFAM table, but this definition needs to remain for the
// schema update process.
static const std::string gstrADD_LOCKED_FILE_PROCESSINGFAM_FK_V101 =
	"ALTER TABLE [dbo].[LockedFile]  "
	"WITH CHECK ADD  CONSTRAINT [FK_LockedFile_ProcessingFAM] FOREIGN KEY([UPIID])"
	"REFERENCES [dbo].[ProcessingFAM] ([ID])"
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

// For use in updating from version 127 and before
static const std::string gstrADD_LOCKED_FILE_ACTIVEFAM_FK_V110 =
	"ALTER TABLE [dbo].[LockedFile]  "
	"WITH CHECK ADD  CONSTRAINT [FK_LockedFile_ActiveFAM] FOREIGN KEY([UPIID])"
	"REFERENCES [dbo].[ActiveFAM] ([ID])"
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

// The ProcessingFAM table is now the ActiveFAM table, but this definition needs to remain for the
// schema update process.
static const std::string gstrADD_ACTION_PROCESSINGFAM_FK_V101 =
	"ALTER TABLE [dbo].[ProcessingFAM]  "
	"WITH CHECK ADD  CONSTRAINT [FK_ProcessingFAM_Action] FOREIGN KEY([ActionID])"
	"REFERENCES [dbo].[Action] ([ID])";

// For use in updating from version 127 and before
static const std::string gstrADD_ACTION_ACTIVEFAM_FK_V110 =
	"ALTER TABLE [dbo].[ActiveFAM]  "
	"WITH CHECK ADD  CONSTRAINT [FK_ActiveFAM_Action] FOREIGN KEY([ActionID])"
	"REFERENCES [dbo].[Action] ([ID])";

static const std::string gstrCREATE_WORK_ITEM_TABLE_V118 =
	"CREATE TABLE [dbo].[WorkItem]("
	"[ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_WorkItem] PRIMARY KEY CLUSTERED ,"
	"[WorkItemGroupID] [int] NOT NULL,"
	"[Status] [nchar](1) NOT NULL,"
	"[Input] [nvarchar](MAX) NULL,"
	"[Output] [nvarchar](MAX) NULL,"
	"[UPI] [nvarchar](450) NULL,"
	"[Sequence] [int] NOT NULL,"
	"[StringizedException] [nvarchar](MAX) NULL)";

// This is an old version of the FileActionStatus table that is used only for schema updates.
static const std::string gstrCREATE_FILE_ACTION_STATUS_LEGACY = 
	"CREATE TABLE [dbo].[FileActionStatus]( "
	"[ActionID] [int] NOT NULL, "
	"[FileID] [int] NOT NULL, "
	"[ActionStatus] [nvarchar](1) NOT NULL, "
	"CONSTRAINT [PK_FileActionStatus] PRIMARY KEY CLUSTERED "
	"( "
	"	[ActionID] ASC, "
	"	[FileID] ASC "
	")) ";

// Used for schema versions 130 - 132
static const string gstrCREATE_SECURE_COUNTER_V130 =
	"CREATE TABLE dbo.SecureCounter ( "
	"   ID int NOT NULL CONSTRAINT PK_SecureCounter PRIMARY KEY CLUSTERED, "
	"   CounterName nvarchar(100) NOT NULL, "
	"   SecureCounterValue nvarchar(max) NOT NULL)";

// Used before schema version 128 - 134
static const string gstrADD_FAM_SESSION_ACTION_FK_V128 =
	"ALTER TABLE [dbo].[FAMSession] "
	"WITH CHECK ADD CONSTRAINT [FK_FAMSession_Action] FOREIGN KEY([ActionID])"
	"REFERENCES [dbo].[Action] ([ID])"
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

// Used for schema versions 138 - 140
static const string gstrCREATE_PAGINATION_LEGACY =
	"CREATE TABLE [dbo].[Pagination] ( "
	"	[ID] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Pagination] PRIMARY KEY CLUSTERED, "
	"	[SourceFileID] INT NOT NULL, "
	"	[SourcePage] INT NOT NULL, "
	"	[DestFileID] INT NOT NULL, "
	"	[DestPage] INT NOT NULL, "
	"	[OriginalFileID] INT NOT NULL, "
	"	[OriginalPage] INT NOT NULL, "
	"	[FileTaskSessionID] INT NOT NULL)";

// Used for schema versions 138 - 140
static const string gstrCREATE_PAGINATION_DESTFILE_INDEX_LEGACY =
	"CREATE UNIQUE NONCLUSTERED INDEX [IX_Pagination_DestFile] ON "
	"	[dbo].[Pagination] ([DestFileID], [DestPage])";