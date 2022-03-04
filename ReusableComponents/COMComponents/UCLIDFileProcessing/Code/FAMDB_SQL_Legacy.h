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

// No longer part of the database; definition for schema update use only.
static const string gstrCREATE_ACTIONSTATUS_PRIORITY_FILE_ACTIONID_INDEX =
	"CREATE UNIQUE CLUSTERED INDEX "
	"[IX_ActionStatusPriorityFileIDActionID] ON [dbo].[FileActionStatus] "
	"("
	"	[ActionStatus] ASC,"
	"	[Priority] DESC,"
	"	[FileID] ASC, "
	"	[ActionID] ASC"
	")";

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

// Used for schema version 129-160
static const string gstrCREATE_FILE_TASK_SESSION_V129 =
	"CREATE TABLE [dbo].[FileTaskSession]( "
	" [ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_FileTaskSession] PRIMARY KEY CLUSTERED, "
	" [FAMSessionID] [int] NOT NULL, "
	" [TaskClassID] [int] NOT NULL, "
	" [FileID] [int] NOT NULL, "
	" [DateTimeStamp] [datetime] NULL, "
	" [Duration] [float] NULL, "
	" [OverheadTime] [float] NULL)";

// Used for schema version 161-200
static const string gstrCREATE_FILE_TASK_SESSION_V161 =
	"CREATE TABLE [dbo].[FileTaskSession]( "
	" [ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_FileTaskSession] PRIMARY KEY CLUSTERED, "
	" [FAMSessionID] [int] NOT NULL, "
	" [ActionID] [int] NULL,"
	" [TaskClassID] [int] NOT NULL, "
	" [TaskClassGUID] UNIQUEIDENTIFIER, "
	" [FileID] [int] NOT NULL, "
	" [DateTimeStamp] [datetime] NULL, "
	" [Duration] [float] NULL, "
	" [OverheadTime] [float] NULL, "
	" [ActivityTime] [float] NULL)";

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

// used for schema version 162-199
static const string gstrCREATE_REPORTING_VERIFICATION_RATES_V162 =
"CREATE TABLE [dbo].[ReportingVerificationRates]( "
"   [ID][int] IDENTITY(1, 1) NOT NULL CONSTRAINT[PK_ReportingVerificationRates] PRIMARY KEY NONCLUSTERED, "
"   [DatabaseServiceID] [INT] NOT NULL, "
"	[FileID] [int] NOT NULL, "
"	[ActionID] [int] NULL,"
"	[TaskClassID] [int] NOT NULL, "
"   [LastFileTaskSessionID] [int] NOT NULL, "
"	[Duration] [float] NOT NULL CONSTRAINT [DF_Duration] DEFAULT(0.0), "
"	[OverheadTime] [float] NOT NULL CONSTRAINT [DF_OverheadTime] DEFAULT(0.0), "
"	[ActivityTime] [float] NOT NULL CONSTRAINT [DF_ActivityTime] DEFAULT(0.0) "
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

// used for schema version 169 to 191
static const string gstrCREATE_WORKFLOWFILE_169 =
	"CREATE TABLE dbo.[WorkflowFile]( "
	"	[WorkflowID] INT NOT NULL, "
	"	[FileID] INT NOT NULL, "
	"	[Deleted] BIT NOT NULL DEFAULT(0), "
	"	[AddedDateTime] [datetime] NOT NULL CONSTRAINT [DF_WorkflowFile_AddedDateTime] DEFAULT(GETDATE()), "
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


static const string gstrCREATE_ACTIONSTATUS_ACTIONID_PRIORITY_FILE_INDEX_188 =
"CREATE UNIQUE CLUSTERED INDEX[IX_ActionStatusActionIDPriorityFileID] ON[dbo].[FileActionStatus] "
"("
"	[ActionStatus] ASC,"
"	[ActionID] ASC,"
"	[Priority] DESC,"
"	[FileID] ASC "
")";

static const string gstrCREATE_ACTION_STATISTICS_DELTA_ACTIONID_ID_INDEX =
"CREATE UNIQUE NONCLUSTERED INDEX "
"[IX_ActionStatisticsDeltaActionID_ID] ON [dbo].[ActionStatisticsDelta] "
"([ActionID] ASC, [ID] ASC)";

// used for schema version 102 to 191
static const string gstrCREATE_ACTION_STATISTICS_TABLE_102 = "CREATE TABLE [dbo].[ActionStatistics]("
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

static const string gstrCREATE_ACTION_STATISTICS_DELTA_TABLE_102 = "CREATE TABLE [dbo].[ActionStatisticsDelta]("
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

// used for schema versions 190 to 191
static const string gstrCREATE_WORKFLOWFILE_FILEID_WORKFLOWID_DELETED_INDEX =
"IF EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_Workflowfile_FileID_WorkflowID_Deleted' AND object_id = OBJECT_ID('WorkflowFile')) \r\n"
"BEGIN \r\n"
"	DROP INDEX [IX_Workflowfile_FileID_WorkflowID_Deleted] ON [dbo].[WorkflowFile] \r\n"
"END \r\n"
"CREATE NONCLUSTERED INDEX [IX_Workflowfile_FileID_WorkflowID_Deleted] ON[dbo].[WorkflowFile]\r\n"
"(																							\r\n"
"	[FileID] ASC,																			\r\n"
"	[WorkflowID] ASC,																		\r\n"
"	[Deleted] ASC																			\r\n"
")";

static const std::string gstrVIEW_DEFINITION_FOR_FAMUSER_INPUT_EVENTS_TIME_V166 =
"			SELECT[FAMSession].[FAMUserID]															"
"				, CAST([FileTaskSession].[DateTimeStamp] AS DATE) AS [InputDate]					"
"				, SUM([FileTaskSession].[ActivityTime] / 60.0) AS           TotalMinutes			"
"			FROM[FAMSession] WITH (NOLOCK)																		"
"			INNER JOIN[FileTaskSession] WITH (NOLOCK) ON[FAMSession].[ID] =										"
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

static const std::string gstrCREATE_FAMUSER_INPUT_EVENTS_TIME_VIEW_V166 =
"IF OBJECT_ID('[dbo].[vFAMUserInputEventsTime]', 'V') IS NULL "
"	EXECUTE('CREATE VIEW[dbo].[vFAMUserInputEventsTime] "
"		AS " + gstrVIEW_DEFINITION_FOR_FAMUSER_INPUT_EVENTS_TIME_V166 +
"'	)";

static const std::string gstrALTER_FAMUSER_INPUT_EVENTS_TIME_VIEW_V174 =
"IF OBJECT_ID('[dbo].[vFAMUserInputEventsTime]', 'V') IS NOT NULL "
"	EXECUTE('ALTER VIEW[dbo].[vFAMUserInputEventsTime] "
"		AS " + gstrVIEW_DEFINITION_FOR_FAMUSER_INPUT_EVENTS_TIME_V166 +
"'	)";


static const string gstrCREATE_FAMUSER_INPUT_EVENTS_TIME_WITH_FILEID_VIEW_V184 =
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


static string gstrCREATE_USER_COUNTS_STORED_PROCEDURE_V187_V199 =
"IF(																																																 \r\n"
"	 EXISTS(													 \r\n"
"		SELECT 1												 \r\n"
"		FROM Information_schema.Routines						 \r\n"
"		WHERE Specific_schema = 'dbo'							 \r\n"
"		AND specific_name = 'sp_UserCounts' \r\n"
"		AND Routine_Type = 'Procedure'							 \r\n"
"	)															 \r\n"
")																 \r\n"
"BEGIN															 \r\n"
"	DROP PROCEDURE [dbo].[sp_UserCounts]			\r\n"
"END															 \r\n"
"   															 \r\n"
"EXEC ('														 \r\n"
"	CREATE PROCEDURE [dbo].[sp_UserCounts] ( \r\n"
"		-- Add the parameters for the function here \r\n"
"		@InclusionActions NVARCHAR(MAX) \r\n"
"		,@ExclusionTags NVARCHAR(MAX) \r\n"
"		,@WorkflowName NVARCHAR(100) \r\n"
"		,@ReportingPeriod_Min DATETIME \r\n"
"		,@ReportingPeriod_Max DATETIME \r\n"
"		) \r\n"
"	AS \r\n"
"	BEGIN \r\n"
"		-- Declare a table of action IDs that should be included in the report. \r\n"
"		DECLARE @includedActions TABLE (ID INT) \r\n"
"	 \r\n"
"		INSERT INTO @includedActions \r\n"
"		SELECT [Action].[ID] \r\n"
"		FROM [Action] \r\n"
"		LEFT JOIN [Workflow] ON [Workflow].ID = [Action].WorkflowID \r\n"
"		WHERE ( \r\n"
"				@InclusionActions IS NULL \r\n"
"				OR @InclusionActions = '''' \r\n"
"				OR [ASCName] IN ( \r\n"
"					SELECT ItemValue \r\n"
"					FROM [dbo].[fn_TableFromCommaSeparatedList](@InclusionActions) \r\n"
"					) \r\n"
"				) \r\n"
"			AND ( \r\n"
"				[Workflow].[Name] = @WorkflowName \r\n"
"				OR @WorkflowName = '''' \r\n"
"				OR @WorkflowName IS NULL \r\n"
"				) \r\n"
"	 \r\n"
"		-- Declare a table of tag IDs to be excluded from the report. \r\n"
"		DECLARE @excludedTags TABLE (ID INT) \r\n"
"	 \r\n"
"		INSERT INTO @excludedTags \r\n"
"		SELECT [ID] \r\n"
"		FROM [Tag] \r\n"
"		WHERE [TagName] IN ( \r\n"
"				SELECT ItemValue \r\n"
"				FROM [dbo].[fn_TableFromCommaSeparatedList](@ExclusionTags) \r\n"
"				) \r\n"
"	 \r\n"
"		-- Declare a local table to hold the count data \r\n"
"		DECLARE @countTable TABLE ( \r\n"
"			FileTaskSessionID INT \r\n"
"			,FileID INT \r\n"
"			,FAMUserID INT \r\n"
"			,TotalDuration FLOAT \r\n"
"			,DocCount INT \r\n"
"			,PageCount INT \r\n"
"			,OrderCount INT \r\n"
"			,TestCount INT \r\n"
"			) \r\n"
"	 \r\n"
"		-- For each unique FileID in DataEntryData, select the most recent entry (from which \r\n"
"		-- counts will be obtained), and aggregate the total time spent on each file (per user). \r\n"
"		INSERT INTO @countTable ( \r\n"
"			FileTaskSessionID \r\n"
"			,FileID \r\n"
"			,FAMUserID \r\n"
"			,TotalDuration \r\n"
"			) \r\n"
"		SELECT ( \r\n"
"				-- For each file, find the last DataEntryData row for which there were saved counts to use \r\n"
"				-- as the counts for the file. If no such row can be found, just use the last row for the \r\n"
"				-- file, but only if that row is from the current user. \r\n"
"				SELECT ID \r\n"
"				FROM ( \r\n"
"					SELECT MAX([FileTaskSession].[ID]) AS ID \r\n"
"					FROM [FileTaskSession] \r\n"
"					INNER JOIN FAMSession ON FAMSession.ID = FileTaskSession.FAMSessionID \r\n"
"					INNER JOIN [DataEntryCounterValue] ON [DataEntryCounterValue].[InstanceID] = [FileTaskSession].[ID] \r\n"
"					INNER JOIN [DataEntryCounterDefinition] ON [DataEntryCounterDefinition].[ID] = [DataEntryCounterValue].[CounterID] \r\n"
"					INNER JOIN TaskClass ON FileTaskSession.TaskClassID = TaskClass.ID \r\n"
"					LEFT JOIN [Action] ON [FileTaskSession].ActionID = [Action].ID \r\n"
"					LEFT JOIN Workflow ON Workflow.ID = [Action].WorkflowID \r\n"
"					WHERE [FileTaskSession].[FileID] = [OuterFileTaskSession].FileID \r\n"
"						-- Limit the selection to the included actions and tags \r\n"
"						AND [FileTaskSession].[ID] IN ( \r\n"
"							SELECT [ID] \r\n"
"							FROM [FileTaskSession] \r\n"
"							LEFT JOIN [FileTag] ON [FileTaskSession].[FileID] = [FileTag].[FileID] \r\n"
"								AND [FileTag].[TagID] IN ( \r\n"
"									SELECT [ID] \r\n"
"									FROM @excludedTags \r\n"
"									) \r\n"
"							WHERE [FileTaskSession].[FileID] = [OuterFileTaskSession].[FileID] \r\n"
"								AND [TagID] IS NULL \r\n"
"							) \r\n"
"						AND [FileTaskSession].[ActionID] IN ( \r\n"
"							SELECT [ID] \r\n"
"							FROM @includedActions \r\n"
"							) \r\n"
"						-- Limit the selection to DataEntryData rows where counts were saved. \r\n"
"						AND ( \r\n"
"							[DataEntryCounterDefinition].[Name] = ''NumOrders'' \r\n"
"							OR [DataEntryCounterDefinition].[Name] = ''NumTests'' \r\n"
"							) \r\n"
"						AND [Type] = ''S'' \r\n"
"						AND Value IS NOT NULL \r\n"
"						AND TaskClass.GUID = ''59496DF7-3951-49B7-B063-8C28F4CD843F'' \r\n"
"						AND ( \r\n"
"							[Workflow].[Name] = @WorkflowName \r\n"
"							OR @WorkflowName = '''' \r\n"
"							OR @WorkflowName IS NULL \r\n"
"							) \r\n"
"						-- Do not limit by time here. That way, if counts for a file are saved multiple times, \r\n"
"						-- but the last time comes outside of the time range, the counts are not included \r\n"
"						-- based on the initial save. (Because in this case, the counts would also be included \r\n"
"						-- in a report for the following period, thus, double-counting) \r\n"
"					) AS FileTaskSessionID \r\n"
"				WHERE OuterFAMSession.[FAMUserID] IN ( \r\n"
"						SELECT FAMUserID \r\n"
"						FROM FileTaskSession \r\n"
"						INNER JOIN FAMSession ON FAMSession.ID = FileTaskSession.FAMSessionID \r\n"
"						LEFT JOIN [Action] ON [FileTaskSession].ActionID = [Action].ID \r\n"
"						LEFT JOIN Workflow ON Workflow.ID = [Action].WorkflowID \r\n"
"						WHERE FileTaskSession.ID = FileTaskSessionID.ID \r\n"
"							AND ( \r\n"
"								[Workflow].[Name] = @WorkflowName \r\n"
"								OR @WorkflowName = '''' \r\n"
"								OR @WorkflowName IS NULL \r\n"
"								) \r\n"
"						) \r\n"
"					AND ( \r\n"
"						SELECT [DateTimeStamp] \r\n"
"						FROM FileTaskSession \r\n"
"						LEFT JOIN [Action] ON [FileTaskSession].ActionID = [Action].ID \r\n"
"						LEFT JOIN Workflow ON Workflow.ID = [Action].WorkflowID \r\n"
"						WHERE FileTaskSession.ID = FileTaskSessionID.ID \r\n"
"							AND ( \r\n"
"								[Workflow].[Name] = @WorkflowName \r\n"
"								OR @WorkflowName = '''' \r\n"
"								OR @WorkflowName IS NULL \r\n"
"								) \r\n"
"						) BETWEEN @ReportingPeriod_Min \r\n"
"						AND @ReportingPeriod_Max \r\n"
"				) \r\n"
"			,[OuterFileTaskSession].[FileID] \r\n"
"			,OuterFAMSession.[FAMUserID] \r\n"
"			,SUM(ActivityTime) + SUM(OverheadTime) \r\n"
"		FROM FileTaskSession AS [OuterFileTaskSession] \r\n"
"		INNER JOIN FAMSession AS OuterFAMSession ON [OuterFileTaskSession].FAMSessionID = OuterFAMSession.ID \r\n"
"		INNER JOIN TaskClass ON [OuterFileTaskSession].TaskClassID = TaskClass.ID \r\n"
"		LEFT JOIN [Action] ON [OuterFileTaskSession].ActionID = [Action].ID \r\n"
"		LEFT JOIN Workflow ON Workflow.ID = [Action].WorkflowID \r\n"
"		-- Limit the selection to the included actions and tags \r\n"
"		WHERE [OuterFileTaskSession].[ID] IN ( \r\n"
"				SELECT [ID] \r\n"
"				FROM FileTaskSession \r\n"
"				LEFT JOIN [FileTag] ON FileTaskSession.[FileID] = [FileTag].[FileID] \r\n"
"					AND [FileTag].[TagID] IN ( \r\n"
"						SELECT [ID] \r\n"
"						FROM @excludedTags \r\n"
"						) \r\n"
"				WHERE FileTaskSession.[FileID] = [OuterFileTaskSession].[FileID] \r\n"
"					AND [TagID] IS NULL \r\n"
"				) \r\n"
"			AND [OuterFileTaskSession].[ActionID] IN ( \r\n"
"				SELECT [ID] \r\n"
"				FROM @includedActions \r\n"
"				) \r\n"
"			AND [DateTimeStamp] BETWEEN @ReportingPeriod_Min \r\n"
"				AND @ReportingPeriod_Max \r\n"
"			AND TaskClass.GUID = ''59496DF7-3951-49B7-B063-8C28F4CD843F'' \r\n"
"			AND ( \r\n"
"				[Workflow].[Name] = @WorkflowName \r\n"
"				OR @WorkflowName = '''' \r\n"
"				OR @WorkflowName IS NULL \r\n"
"				) \r\n"
"		GROUP BY [OuterFileTaskSession].[FileID] \r\n"
"			,OuterFAMSession.[FAMUserID] \r\n"
"	 \r\n"
"		-- Populate the counts using the DataEntryCounterValue table based on the DataEntryID \r\n"
"		-- determined to be operative for each file. \r\n"
"		UPDATE @countTable \r\n"
"		SET [@countTable].OrderCount = COALESCE([DataEntryCounterValue].Value, 0) \r\n"
"		FROM @countTable \r\n"
"		INNER JOIN [DataEntryCounterValue] ON [@countTable].FileTaskSessionID = [DataEntryCounterValue].InstanceID \r\n"
"		INNER JOIN [DataEntryCounterDefinition] ON [DataEntryCounterDefinition].ID = [DataEntryCounterValue].CounterID \r\n"
"		WHERE [DataEntryCounterDefinition].Name = ''NumOrders'' \r\n"
"			AND [DataEntryCounterValue].InstanceID = [@countTable].FileTaskSessionID \r\n"
"			AND [DataEntryCounterValue].[Type] = ''S'' \r\n"
"	 \r\n"
"		UPDATE @countTable \r\n"
"		SET [@countTable].TestCount = COALESCE([DataEntryCounterValue].Value, 0) \r\n"
"		FROM @countTable \r\n"
"		INNER JOIN [DataEntryCounterValue] ON [@countTable].FileTaskSessionID = [DataEntryCounterValue].InstanceID \r\n"
"		INNER JOIN [DataEntryCounterDefinition] ON [DataEntryCounterDefinition].ID = [DataEntryCounterValue].CounterID \r\n"
"		WHERE [DataEntryCounterDefinition].Name = ''NumTests'' \r\n"
"			AND [DataEntryCounterValue].InstanceID = [@countTable].FileTaskSessionID \r\n"
"			AND [DataEntryCounterValue].[Type] = ''S'' \r\n"
"	 \r\n"
"		UPDATE @countTable \r\n"
"		SET [DocCount] = 1 \r\n"
"			,[PageCount] = [Pages] \r\n"
"		FROM @countTable \r\n"
"		INNER JOIN [FAMFile] ON [@countTable].[FileID] = [FAMFile].[ID] \r\n"
"		WHERE [OrderCount] IS NOT NULL \r\n"
"	 \r\n"
"		UPDATE @countTable \r\n"
"		SET [OrderCount] = 0 \r\n"
"			,[TestCount] = 0 \r\n"
"			,[DocCount] = 0 \r\n"
"			,[PageCount] = 0 \r\n"
"		FROM @countTable \r\n"
"		WHERE [OrderCount] IS NULL \r\n"
"	 \r\n"
"		-- Return a select query containing the counts, along with the page counts totaled by user \r\n"
"		SELECT UserName \r\n"
"			, FullUserName \r\n"
"			,SUM([DocCount]) AS NumDocs \r\n"
"			,SUM([PageCount]) AS NumPages \r\n"
"			,SUM([OrderCount]) AS NumOrders \r\n"
"			,SUM([TestCount]) AS NumTests \r\n"
"			,SUM([TotalDuration]) AS TotalTime \r\n"
"			,(SUM([DocCount]) / (SUM([TotalDuration]) / 3600.0)) AS DocsPerHour \r\n"
"			,(SUM([PageCount]) / (SUM([TotalDuration]) / 3600.0)) AS PagesPerHour \r\n"
"			,(SUM([OrderCount]) / (SUM([TotalDuration]) / 3600.0)) AS OrdersPerHour \r\n"
"			,(SUM([TestCount]) / (SUM([TotalDuration]) / 3600.0)) AS TestPerHour \r\n"
"		FROM @countTable \r\n"
"		INNER JOIN FAMFile ON [@countTable].FileID = [FAMFile].ID \r\n"
"		INNER JOIN [FAMUser] ON [@countTable].FAMUserID = [FAMUser].ID \r\n"
"		GROUP BY UserName, FullUserName \r\n"
"	END \r\n"
"') \r\n";
