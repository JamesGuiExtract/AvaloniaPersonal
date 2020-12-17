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

// Used from schema 116 - 149
// Was LaunchApp in versions 114 and 115
static const string gstrCREATE_FILE_HANDLER_TABLE_V116 =
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

// Used for schema versions 143 - 145
static const string gstrCREATE_WORKFLOW_V143 =
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

// Used for schema versions 146 - 168
static const string gstrCREATE_WORKFLOW_V146 =
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
"	CONSTRAINT [IX_WorkflowName] UNIQUE NONCLUSTERED ([Name]))";

// Used for schema version 129-147
static const string gstrCREATE_FILE_TASK_SESSION_V129 =
	"CREATE TABLE [dbo].[FileTaskSession]( "
	" [ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_FileTaskSession] PRIMARY KEY CLUSTERED, "
	" [FAMSessionID] [int] NOT NULL, "
	" [TaskClassID] [int] NOT NULL, "
	" [FileID] [int] NOT NULL, "
	" [DateTimeStamp] [datetime] NULL, "
	" [Duration] [float] NULL, "
	" [OverheadTime] [float] NULL)";

// used for schema version 148-150
static const string gstrADD_WORKFLOWCHANGEFILE_ACTIONSOURCE_FK_V148 =
"ALTER TABLE dbo.[WorkFlowChangeFile] "
"ADD CONSTRAINT [FK_WorkflowChangeFile_ActionSource] FOREIGN KEY([SourceActionID]) "
"REFERENCES[dbo].[Action]([ID]) "
" ON UPDATE NO ACTION "  // Anything except NO ACTION leads to errors about cascading
" ON DELETE NO ACTION";  // updates/deletes due to multiple FKs to Action table.

// used for schema version 148-150
static const string gstrADD_WORKFLOWCHANGEFILE_ACTIONDESTINATION_FK_V148 =
"ALTER TABLE dbo.[WorkFlowChangeFile] "
"ADD CONSTRAINT [FK_WorkflowChangeFile_ActionDestination] FOREIGN KEY([DestActionID]) "
"REFERENCES[dbo].[Action]([ID]) "
" ON UPDATE NO ACTION "  // Anything except NO ACTION leads to errors about cascading
" ON DELETE NO ACTION";  // updates/deletes due to multiple FKs to Workflow table.

// used for schema version 148-150
static const string gstrADD_WORKFLOWCHANGEFILE_WORKFLOWDEST_FK_V148 =
"ALTER TABLE dbo.[WorkFlowChangeFile] "
"ADD CONSTRAINT [FK_WorkflowChangeFile_WorkflowDest] FOREIGN KEY([DestWorkflowID]) "
"REFERENCES[dbo].[Workflow]([ID]) "
" ON UPDATE NO ACTION "  // Anything except NO ACTION leads to errors about cascading
" ON DELETE NO ACTION";  // updates/deletes due to multiple FKs to Workflow table.

// used for schema version 148-150
static const string gstrADD_WORKFLOWCHANGEFILE_WORKFLOWSOURCE_FK_V148 =
"ALTER TABLE dbo.[WorkFlowChangeFile] "
"ADD CONSTRAINT [FK_WorkflowChangeFile_WorkflowSource] FOREIGN KEY([SourceWorkflowID]) "
"REFERENCES[dbo].[Workflow]([ID]) "
" ON UPDATE NO ACTION "  // Anything except NO ACTION leads to errors about cascading
" ON DELETE NO ACTION";  // updates/deletes due to multiple FKs to Workflow table.

// used for schema version 148-151
static const string gstrCREATE_WORKFLOWCHANGEFILE_V148 =
"CREATE TABLE [dbo].[WorkflowChangeFile]( "
"	[FileID]           INT NOT NULL, "
"	[WorkflowChangeID] INT NOT NULL, "
"	[SourceActionID]   INT NOT NULL, "
"	[DestActionID]     INT NOT NULL, "
"	[SourceWorkflowID] INT NULL, "
"	[DestWorkflowID]   INT NOT NULL, "
"	CONSTRAINT [PK_WorkflowChangeFile] PRIMARY KEY CLUSTERED([FileID] ASC, [WorkflowChangeID] ASC, [SourceActionID] ASC));";

// used for schema version 148-152
static const string gstrADD_FILE_TASK_SESSION_ACTION_FK_V148 =
"ALTER TABLE [dbo].[FileTaskSession]  "
"WITH CHECK ADD  CONSTRAINT [FK_FileTaskSession_Action] FOREIGN KEY([ActionID])"
"REFERENCES [dbo].[Action] ([ID])";

// used for schema version 160-161
static const string gstrCREATE_REPORTING_VERIFICATION_RATES_V160 =
"CREATE TABLE [dbo].[ReportingVerificationRates]( "
"   [ID][int] IDENTITY(1, 1) NOT NULL CONSTRAINT[PK_ReportingVerificationRates] PRIMARY KEY NONCLUSTERED, "
"   [DatabaseServiceID] [INT] NOT NULL, "
"	[FileID] [int] NOT NULL, "
"	[ActionID] [int] NULL,"
"	[TaskClassID] [int] NOT NULL, "
"   [LastFileTaskSessionID] [int] NOT NULL, "
"	[Duration] [float] NOT NULL CONSTRAINT [DF_Duration] DEFAULT(0.0), "
"	[OverheadTime] [float] NOT NULL CONSTRAINT [DF_OverheadTime] DEFAULT(0.0), "
"	[ActiveMinutes][float] NOT NULL CONSTRAINT [DF_ActiveMinutes] DEFAULT(0.0) "
"   CONSTRAINT [IX_ReportingVerificationRatesFileActionTask] UNIQUE CLUSTERED([FileID],[ActionID],[TaskClassID],[DatabaseServiceID]))";

// used for schema version 159 to 162
static const string gstrCREATE_DATABASE_SERVICE_TABLE_159 =
"CREATE TABLE [dbo].[DatabaseService]( "
"	[ID][int] IDENTITY(1, 1) NOT NULL CONSTRAINT[PK_DatabaseService] PRIMARY KEY CLUSTERED, "
"	[Description] NVARCHAR(MAX) NULL, "
"	[Settings] NVARCHAR(MAX) NOT NULL, "
"   [Status] NVARCHAR(MAX) NULL) ";

// used for schema version 164 to 166
static const string gstrCREATE_DASHBOARD_TABLE_V164 =
"CREATE TABLE [dbo].[Dashboard]( "
"	[DashboardName] [nvarchar](100) NOT NULL CONSTRAINT [PK_Dashboard] PRIMARY KEY CLUSTERED, "
"	[Definition] [xml] NOT NULL )";

// used for schema version 145 to 168
static const string gstrCREATE_WORKFLOWFILE_V145 =
"CREATE TABLE dbo.[WorkflowFile]( "
"	[WorkflowID] INT NOT NULL, "
"	[FileID] INT NOT NULL, "
"	CONSTRAINT [PK_WorkflowFile] PRIMARY KEY CLUSTERED ([WorkflowID], [FileID]));";

static const std::string gstrCREATE_FAMUSER_INPUT_EVENTS_TIME_VIEW_LEGACY_166 =
"IF OBJECT_ID('[dbo].[vFAMUserInputEventsTimeLegacy]', 'V') IS NULL "
"	EXECUTE('CREATE VIEW[dbo].[vFAMUserInputEventsTimeLegacy] "
"		AS "
"		SELECT        FAMUserID, CAST(TimeStamp AS DATE) AS InputDate, COUNT(ID) AS TotalMinutes "
"		FROM            dbo.InputEvent "
"		GROUP BY FAMUserID, CAST(TimeStamp AS DATE)'"
"	)";

static const std::string gstrCREATE_DATABASE_MIGRATION_WIZARD_REPORTING_181 =
" CREATE TABLE[dbo].[ReportingDatabaseMigrationWizard] "
" ( "
" [ID] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY "
" , [Classification] NVARCHAR(128) NOT NULL DEFAULT 'Information' "
" , [TableName] NVARCHAR(128) NOT NULL "
" , [Message] NVARCHAR(512) NOT NULL "
" , [DateTime] DATETIME NOT NULL DEFAULT GETDATE() "
" )";

static const string gstrCREATE_FILE_ACTION_STATUS_112_187 =
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

static const string gstrCREATE_SKIPPED_FILE_FAM_SESSION_INDEX_128_187 = "CREATE NONCLUSTERED INDEX "
"[IX_Skipped_File_FAMSession] ON [SkippedFile]([FAMSessionID])";