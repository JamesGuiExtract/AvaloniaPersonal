// Attribute_DB_SQL.h - Constants for DB SQL queries that are Attribute Specific

#pragma once

#include <string>

// 10.3 table names
static const string gstrATTRIBUTE_SET_NAME = "AttributeSetName";
static const string gstrATTRIBUTE_SET_FOR_FILE = "AttributeSetForFile";
static const string gstrATTRIBUTE_NAME = "AttributeName";
static const string gstrATTRIBUTE_TYPE = "AttributeType";
static const string gstrATTRIBUTE_INSTANCE_TYPE = "AttributeInstanceType";
static const string gstrATTRIBUTE = "Attribute";
static const string gstrRASTER_ZONE = "RasterZone";

// 10.3 table additions

static const std::string gstrCREATE_ATTRIBUTE_SET_NAME_TABLE_v1 = 
	"CREATE TABLE [dbo].[AttributeSetName] "
	"([ID] [bigint] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_AttributeSetName] PRIMARY KEY CLUSTERED, "
	"[Description] [nvarchar](255) NULL CONSTRAINT [Attribute_Set_Name_Description_Unique] UNIQUE(Description))";

static const std::string gstrCREATE_ATTRIBUTE_SET_FOR_FILE_TABLE_v1 = 
	"CREATE TABLE [dbo].[AttributeSetForFile] "
	"([ID] [bigint] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_AttributeSetForFile] PRIMARY KEY CLUSTERED, "
	"[FileTaskSessionID] [int] NOT NULL, "							// foreign key, FileTaskSession.ID
	"[AttributeSetNameID] [bigint] NOT NULL)";							// foreign key, AttributeSetName.ID
	
static const std::string gstrCREATE_ATTRIBUTE_NAME_TABLE_v1 = 
	"CREATE TABLE [dbo].[AttributeName] "
	"([ID] [bigint] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_AttributeName] PRIMARY KEY CLUSTERED, "
	"[Name] [nvarchar](255) NULL CONSTRAINT [Attribute_Name_Name_Unique] UNIQUE(Name))";

static const std::string gstrCREATE_ATTRIBUTE_TYPE_TABLE_v1 = 
	"CREATE TABLE [dbo].[AttributeType] "
	"([ID] [bigint] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_AttributeType] PRIMARY KEY CLUSTERED, "
	"[Type] [nvarchar](255) NULL CONSTRAINT [Attribute_Type_Type_Unique] UNIQUE(Type))";

static const std::string gstrCREATE_ATTRIBUTE_INSTANCE_TYPE_v1 = 
	"CREATE TABLE [dbo].[AttributeInstanceType] "
	"([AttributeID] [bigint] NOT NULL, "
	"[AttributeTypeID] [bigint] NOT NULL, "
	"CONSTRAINT [PK_AttributeInstanceType] PRIMARY KEY CLUSTERED ([AttributeID] ASC, [AttributeTypeID] ASC))";

static const std::string gstrCREATE_ATTRIBUTE_TABLE_v1 = 
	"CREATE TABLE [dbo].[Attribute] "
	"([ID] [bigint] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Attribute] PRIMARY KEY CLUSTERED, "
	"[AttributeSetForFileID] [bigint] NOT NULL, "		//FK, AttributeSetForFile.ID 
	"[AttributeNameID] [bigint] NOT NULL, "				//FK, AttributeName.ID
	"[Value] [nvarchar](max) NOT NULL, "
	"[ParentAttributeID] [bigint],	"					//FK, Atribute.ID, null allowed
	"[GUID] [uniqueidentifier] NOT NULL)";
	
static const std::string gstrCREATE_RASTER_ZONE_TABLE_v1 = 
	"CREATE TABLE [dbo].[RasterZone] "
	"([ID] [bigint] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_RasterZone] PRIMARY KEY CLUSTERED, "
	"[AttributeID] [bigint] NOT NULL, "				//FK, Attribute.ID
	"[Top] [int] NOT NULL, "
	"[Left] [int] NOT NULL, "
	"[Bottom] [int] NOT NULL, "
	"[Right] [int] NOT NULL, "
	"[StartX] [int] NOT NULL, "
	"[StartY] [int] NOT NULL, "
	"[EndX] [int] NOT NULL, "
	"[EndY] [int] NOT NULL, "
	"[PageNumber] [int] NOT NULL, "
	"[Height] [int] NOT NULL)";

// 10.3 table FK additions start here.
static const std::string gstrADD_ATTRIBUTE_SET_FOR_FILE_FILETASKSESSIONID_FK = 
	"ALTER TABLE [dbo].[AttributeSetForFile] "
	"WITH CHECK ADD CONSTRAINT [FK_AttributeSetForFile_FileTaskSessionID] FOREIGN KEY([FileTaskSessionID]) "
	"REFERENCES [FileTaskSession] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const std::string gstrADD_ATTRIBUTE_SET_FOR_FILE_ATTRIBUTESETNAMEID_FK = 
	"ALTER TABLE [AttributeSetForFile] "
	"WITH CHECK ADD CONSTRAINT [FK_AttributeSetForFile_AttributeSetNameID] FOREIGN KEY([AttributeSetNameID]) "
	"REFERENCES [AttributeSetName] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const std::string gstrADD_ATTRIBUTE_INSTANCE_TYPE_ATTRIBUTEID = 
	"ALTER TABLE [AttributeInstanceType] "
	"WITH CHECK ADD CONSTRAINT [FK_Attribute_Instance_Type_AttributeID] FOREIGN KEY([AttributeID]) "
	"REFERENCES [Attribute] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const std::string gstrADD_ATTRIBUTE_INSTANCE_TYPE_ATTRIBUTETYPEID = 
	"ALTER TABLE [AttributeInstanceType] "
	"WITH CHECK ADD CONSTRAINT [FK_Attribute_Instance_Type_AttributeTypeID] FOREIGN KEY([AttributeTypeID]) "
	"REFERENCES [AttributeType] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const std::string gstrADD_ATTRIBUTE_ATTRIBUTE_SET_FILE_FILEID_FK = 
	"ALTER TABLE [Attribute] "
	"WITH CHECK ADD CONSTRAINT [FK_Attribute_AttributeSetForFileID] FOREIGN KEY([AttributeSetForFileID]) "
	"REFERENCES [AttributeSetForFile] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const std::string gstrADD_ATTRIBUTE_ATTRIBUTE_NAMEID_FK = 
	"ALTER TABLE [Attribute] "
	"WITH CHECK ADD CONSTRAINT [FK_Attribute_AttributeNameID] FOREIGN KEY([AttributeNameID]) "
	"REFERENCES [AttributeName] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const std::string gstrADD_ATTRIBUTE_PARENT_ATTRIBUTEID_FK = 
	"ALTER TABLE [Attribute] "
	"WITH CHECK ADD CONSTRAINT [FK_Attribute_ParentAttributeID] FOREIGN KEY([ParentAttributeID]) "
	"REFERENCES [Attribute] ([ID]) "
	"ON UPDATE NO ACTION "
	"ON DELETE NO ACTION";

static const std::string gstrADD_RASTER_ZONE_ATTRIBUTEID_FK = 
	"ALTER TABLE [RasterZone] "
	"WITH CHECK ADD CONSTRAINT [FK_RasterZone_AttributeID] FOREIGN KEY([AttributeID]) "
	"REFERENCES [Attribute] ([ID]) "
	"ON UPDATE CASCADE "
	"ON DELETE CASCADE";

static const string gstrADD_WORKFLOW_OUTPUTATTRIBUTESET_FK_V3 =
	"ALTER TABLE [dbo].[Workflow]  "
	" WITH CHECK ADD CONSTRAINT [FK_Workflow_OutputAttributeSet] FOREIGN KEY([OutputAttributeSetID]) "
	" REFERENCES [dbo].[AttributeSetName] ([ID])"
	" ON UPDATE CASCADE "
	" ON DELETE CASCADE";

static const string gstrADD_WORKFLOW_OUTPUTATTRIBUTESET_FK =
	"ALTER TABLE [dbo].[Workflow]  "
	" WITH CHECK ADD CONSTRAINT [FK_Workflow_OutputAttributeSet] FOREIGN KEY([OutputAttributeSetID]) "
	" REFERENCES [dbo].[AttributeSetName] ([ID])"
	" ON UPDATE CASCADE "
	" ON DELETE SET NULL";

// 10.3 table indexes are here...
static const std::string gstrCREATE_FILEID_ATTRIBUTE_SET_NAME_ID_INDEX = 
	"CREATE NONCLUSTERED INDEX [IX_FileTaskSessionID_AttributeSetNameID] ON [dbo].[AttributeSetForFile]([FileTaskSessionID] ASC, [AttributeSetNameID] ASC);\n";


// ****************************************************************************
// version 2 update- add a binary column to AttributeSetForFile, to store the complete spatial string for the document.
static const std::string gstrADD_ATTRIBUTE_SET_FOR_FILE_VOA_COLUMN = 
	"ALTER TABLE [dbo].[AttributeSetForFile] ADD [VOA] [varbinary](max) NULL";

// Added AccuracyData table for 10.7
static const std::string gstrREPORTING_REDACTION_ACCURACY_TABLE = "ReportingRedactionAccuracy";
static const std::string gstrREPORTING_DATA_CAPTURE_ACCURACY_TABLE = "ReportingDataCaptureAccuracy";

static const std::string gstrCREATE_REPORTING_REDACTION_ACCURACY_TABLE_V4 =
	"CREATE TABLE [dbo].[ReportingRedactionAccuracy]( "
	"	[ID][bigint] IDENTITY(1, 1) NOT NULL CONSTRAINT [PK_ReportingRedactionAccuracy] PRIMARY KEY CLUSTERED, "
	"   [DatabaseServiceID] [INT] NOT NULL, "
	"	[FoundAttributeSetForFileID][BIGINT] NOT NULL, "
	"	[ExpectedAttributeSetForFileID][BIGINT] NOT NULL, "
	"	[FileID][INT] NOT NULL, "
	"   [Page][INT] NOT NULL, "
	"	[Attribute][nvarchar](MAX) NOT NULL, "
	"	[Expected][bigint] NOT NULL CONSTRAINT DF_ReportingRedactionAccuracyExpected DEFAULT(0), "
	"	[Found][bigint] NOT NULL CONSTRAINT DF_ReportingRedactionAccuracyFound DEFAULT(0), "
	"	[Correct][bigint] NOT NULL CONSTRAINT DF_ReportingRedactionAccuracyCorrect DEFAULT(0), "
	"	[FalsePositives][bigint] NOT NULL CONSTRAINT DF_ReportingRedactionAccuracyFalsePositives DEFAULT(0), "
	"	[OverRedacted][bigint] NOT NULL CONSTRAINT DF_ReportingRedactionAccuracyOverRedacted DEFAULT(0), "
	"	[UnderRedacted][bigint] NOT NULL CONSTRAINT DF_ReportingRedactionAccuracyUnderRedacted DEFAULT(0), "
	"	[Missed][bigint] NOT NULL CONSTRAINT DF_ReportingRedactionAccuracyMissed DEFAULT(0) "
	") ";

static const std::string gstrCREATE_REPORTING_REDACTION_ACCURACY_TABLE =
	"CREATE TABLE [dbo].[ReportingRedactionAccuracy]( "
	"	[ID][bigint] IDENTITY(1, 1) NOT NULL CONSTRAINT [PK_ReportingRedactionAccuracy] PRIMARY KEY CLUSTERED, "
	"   [DatabaseServiceID] [INT] NOT NULL, "
	"	[FoundAttributeSetForFileID][BIGINT] NOT NULL, "
	"   [FoundDateTimeStamp] [DATETIME] NOT NULL, "
	"   [FoundFAMUserID] INT NOT NULL, "
	"   [FoundActionID] INT NULL, "
	"	[ExpectedAttributeSetForFileID][BIGINT] NOT NULL, "
	"   [ExpectedDateTimeStamp] [DATETIME] NOT NULL, "
	"   [ExpectedFAMUserID] INT NOT NULL, "
	"   [ExpectedActionID] INT NULL, "
	"	[FileID][INT] NOT NULL, "
	"   [Page][INT] NOT NULL, "
	"	[Attribute][nvarchar](MAX) NOT NULL, "
	"	[Expected][bigint] NOT NULL CONSTRAINT DF_ReportingRedactionAccuracyExpected DEFAULT(0), "
	"	[Found][bigint] NOT NULL CONSTRAINT DF_ReportingRedactionAccuracyFound DEFAULT(0), "
	"	[Correct][bigint] NOT NULL CONSTRAINT DF_ReportingRedactionAccuracyCorrect DEFAULT(0), "
	"	[FalsePositives][bigint] NOT NULL CONSTRAINT DF_ReportingRedactionAccuracyFalsePositives DEFAULT(0), "
	"	[OverRedacted][bigint] NOT NULL CONSTRAINT DF_ReportingRedactionAccuracyOverRedacted DEFAULT(0), "
	"	[UnderRedacted][bigint] NOT NULL CONSTRAINT DF_ReportingRedactionAccuracyUnderRedacted DEFAULT(0), "
	"	[Missed][bigint] NOT NULL CONSTRAINT DF_ReportingRedactionAccuracyMissed DEFAULT(0) "
	") ";


static const std::string gstrADD_REPORTING_REDACTION_ACCURACY_ATTRIBUTE_SET_FOR_FILE_EXPECTED_FK =
	"ALTER TABLE[dbo].[ReportingRedactionAccuracy]  "
	"	WITH CHECK ADD  CONSTRAINT [FK_ReportingRedactionAccuracy_AttributeSetForFile_Expected] FOREIGN KEY([ExpectedAttributeSetForFileID]) "
	"	REFERENCES[dbo].[AttributeSetForFile]([ID]) "
	"	ON UPDATE NO ACTION "
	"	ON DELETE NO ACTION";

static const std::string gstrADD_REPORTING_REDACTION_ATTRIBUTE_SET_FOR_FILE_FOUND_FK =
	"ALTER TABLE[dbo].[ReportingRedactionAccuracy]  "
	"	WITH CHECK ADD  CONSTRAINT [FK_ReportingRedactionAccuracy_AttributeSetForFile_Found] FOREIGN KEY([FoundAttributeSetForFileID]) "
	"	REFERENCES[dbo].[AttributeSetForFile]([ID]) "
	"	ON UPDATE NO ACTION "
	"	ON DELETE NO ACTION";

static const std::string gstrADD_REPORTING_REDACTION_FAMFILE_FK =
	"ALTER TABLE[dbo].[ReportingRedactionAccuracy]  "
	"	WITH CHECK ADD  CONSTRAINT [FK_ReportingRedactionAccuracy_FAMFile] FOREIGN KEY([FileID]) "
	"	REFERENCES[dbo].[FAMFile]([ID]) "
	"	ON UPDATE CASCADE "
	"	ON DELETE CASCADE"; 

static const std::string gstrADD_REPORTING_REDACTION_DATABASE_SERVICE_FK =
	"ALTER TABLE[dbo].[ReportingRedactionAccuracy]  "
	"	WITH CHECK ADD  CONSTRAINT [FK_ReportingRedactionAccuracy_DatabaseService] FOREIGN KEY([DatabaseServiceID]) "
	"	REFERENCES[dbo].[DatabaseService]([ID]) "
	"	ON UPDATE CASCADE "
	"	ON DELETE CASCADE";

static const std::string gstrCREATE_REPORTING_REDACTION_FILEID_DATABASE_SERVICE_IX =
	"CREATE NONCLUSTERED INDEX[IX_ReportingRedactionAccuracy_FileID_DatabaseServiceID] ON[dbo].[ReportingRedactionAccuracy] "
	"( "
	"	[FileID] ASC, "
	"	[DatabaseServiceID] ASC "
	") ";
	
static const std::string gstrCREATE_REPORTING_DATA_CAPTURE_ACCURACY_TABLE_V4 =
"CREATE TABLE [dbo].[ReportingDataCaptureAccuracy]( "
"	[ID][bigint] IDENTITY(1, 1) NOT NULL CONSTRAINT [PK_ReportingDataCaptureAccuracy] PRIMARY KEY CLUSTERED, "
"   [DatabaseServiceID] [INT] NOT NULL, "
"	[FoundAttributeSetForFileID][BIGINT] NOT NULL, "
"	[ExpectedAttributeSetForFileID][BIGINT] NOT NULL, "
"	[FileID][INT] NOT NULL, "
"	[Attribute][nvarchar](MAX) NOT NULL, "
"	[Correct][bigint] NOT NULL CONSTRAINT DF_ReportingDataCaptureAccuracyCorrect DEFAULT(0), "
"	[Expected][bigint] NOT NULL CONSTRAINT DF_ReportingDataCaptureAccuracyExpected DEFAULT(0), "
"	[Incorrect][bigint] NOT NULL CONSTRAINT DF_ReportingDataCaptureAccuracyIncorrect DEFAULT(0) "
") ";

static const std::string gstrCREATE_REPORTING_DATA_CAPTURE_ACCURACY_TABLE =
"CREATE TABLE [dbo].[ReportingDataCaptureAccuracy]( "
"	[ID][bigint] IDENTITY(1, 1) NOT NULL CONSTRAINT [PK_ReportingDataCaptureAccuracy] PRIMARY KEY CLUSTERED, "
"   [DatabaseServiceID] [INT] NOT NULL, "
"	[FoundAttributeSetForFileID][BIGINT] NOT NULL, "
"   [FoundDateTimeStamp] [DATETIME] NOT NULL, "
"   [FoundFAMUserID] INT NOT NULL, "
"   [FoundActionID] INT NULL, "
"	[ExpectedAttributeSetForFileID][BIGINT] NOT NULL, "
"   [ExpectedDateTimeStamp] [DATETIME] NOT NULL, "
"   [ExpectedFAMUserID] INT NOT NULL, "
"   [ExpectedActionID] INT NULL, "
"	[FileID][INT] NOT NULL, "
"	[Attribute][nvarchar](MAX) NOT NULL, "
"	[Correct][bigint] NOT NULL CONSTRAINT DF_ReportingDataCaptureAccuracyCorrect DEFAULT(0), "
"	[Expected][bigint] NOT NULL CONSTRAINT DF_ReportingDataCaptureAccuracyExpected DEFAULT(0), "
"	[Incorrect][bigint] NOT NULL CONSTRAINT DF_ReportingDataCaptureAccuracyIncorrect DEFAULT(0) "
") ";


static const std::string gstrADD_REPORTING_DATA_CAPTURE_ACCURACY_ATTRIBUTE_SET_FOR_FILE_EXPECTED_FK =
"ALTER TABLE[dbo].[ReportingDataCaptureAccuracy]  "
"	WITH CHECK ADD  CONSTRAINT [FK_ReportingDataCaptureAccuracy_AttributeSetForFile_Expected] FOREIGN KEY([ExpectedAttributeSetForFileID]) "
"	REFERENCES[dbo].[AttributeSetForFile]([ID]) "
"	ON UPDATE NO ACTION "
"	ON DELETE NO ACTION";

static const std::string gstrADD_REPORTING_DATA_CAPTURE_ATTRIBUTE_SET_FOR_FILE_FOUND_FK =
"ALTER TABLE[dbo].[ReportingDataCaptureAccuracy]  "
"	WITH CHECK ADD  CONSTRAINT [FK_ReportingDataCaptureAccuracy_AttributeSetForFile_Found] FOREIGN KEY([FoundAttributeSetForFileID]) "
"	REFERENCES[dbo].[AttributeSetForFile]([ID]) "
"	ON UPDATE NO ACTION "
"	ON DELETE NO ACTION";

static const std::string gstrADD_REPORTING_DATA_CAPTURE_FAMFILE_FK =
"ALTER TABLE[dbo].[ReportingDataCaptureAccuracy]  "
"	WITH CHECK ADD  CONSTRAINT [FK_ReportingDataCaptureAccuracy_FAMFile] FOREIGN KEY([FileID]) "
"	REFERENCES[dbo].[FAMFile]([ID]) "
"	ON UPDATE CASCADE "
"	ON DELETE CASCADE";

static const std::string gstrADD_REPORTING_DATA_CAPTURE_DATABASE_SERVICE_FK =
"ALTER TABLE[dbo].[ReportingDataCaptureAccuracy]  "
"	WITH CHECK ADD  CONSTRAINT [FK_ReportingDataCaptureAccuracy_DatabaseService] FOREIGN KEY([DatabaseServiceID]) "
"	REFERENCES[dbo].[DatabaseService]([ID]) "
"	ON UPDATE CASCADE "
"	ON DELETE CASCADE";

static const std::string gstrCREATE_REPORTING_DATA_CAPTURE_FILEID_DATABASE_SERVICE_IX =
"CREATE NONCLUSTERED INDEX[IX_ReportingDataCaptureAccuracy_FileID_DatabaseServiceID] ON[dbo].[ReportingDataCaptureAccuracy] "
"( "
"	[FileID] ASC, "
"	[DatabaseServiceID] ASC "
") ";

static const string gstrDASHBOARD_ATTRIBUTE_FIELDS_TABLE = "DashboardAttributeFields";

static const string gstrCREATE_DASHBOARD_ATTRIBUTE_FIELDS =
"CREATE TABLE[dbo].[DashboardAttributeFields]( "
"	[AttributeSetForFileID] BIGINT         NOT NULL, "
"	[Name]                  NVARCHAR(255) NOT NULL, "
"	[Value]                 NVARCHAR(MAX) NOT NULL, "
"	CONSTRAINT[PK_DashboardAttributeFields] PRIMARY KEY CLUSTERED([AttributeSetForFileID] ASC, [Name] ASC)) ";

static const string gstrADD_DASHBOARD_ATTRIBUTE_FIELDS_ATTRIBUTESETFORFILE_FK =
"ALTER TABLE[dbo].[DashboardAttributeFields] WITH CHECK "
"ADD CONSTRAINT[FK_DashboardAttributeFields_AttributeSetForFileID] "
"FOREIGN KEY([AttributeSetForFileID]) "
"REFERENCES[dbo].[AttributeSetForFile]([ID]) "
"ON DELETE CASCADE "
"ON UPDATE CASCADE ";

static const std::string gstrCREATE_REPORTING_DATA_CAPTURE_EXPECTED_FAMUSERID_WITH_INCLUDES =
"CREATE NONCLUSTERED INDEX[IX_ReportingDataCaptureAccuracy_ExpectedFAMUserIDWithIncludes] "
"ON[dbo].[ReportingDataCaptureAccuracy]([ExpectedFAMUserID]) "
"INCLUDE([DatabaseServiceID], [ExpectedAttributeSetForFileID], [FileID], [Attribute], [Correct], [Expected], [ExpectedDateTimeStamp]) "; 

static const std::string gstrCREATE_REPORTING_DATA_CAPTURE_FOUND_FAMUSERID_WITH_INCLUDES =
"CREATE NONCLUSTERED INDEX[IX_ReportingDataCaptureAccuracy_FoundFAMUserIDWithIncludes] "
"ON[dbo].[ReportingDataCaptureAccuracy]([FoundFAMUserID]) "
"INCLUDE([DatabaseServiceID], [FoundAttributeSetForFileID], [FileID], [Attribute], [Correct], [Expected], [FoundDateTimeStamp]) ";

static const std::string gstrCREATE_ATTRIBUTESETFORFILE_ATTRIBUTESETID_WITH_NAMEID_VALUE_IX =
"CREATE NONCLUSTERED INDEX[IX_AttributeSetForFile_ID_Value] "
"ON[dbo].[Attribute]([AttributeSetForFileID] ASC) "
"INCLUDE([AttributeNameID], [Value])";


static const std::string gstrCREATE_REPORTING_REDACTION_ACCURACY_EXPECTED_FAMUSERID_WITH_INCLUDES =
"CREATE NONCLUSTERED INDEX[IX_ReportingRedactionAccuracy_ExpectedFAMUserIDWithIncludes] "
"ON[dbo].[ReportingRedactionAccuracy]([ExpectedFAMUserID]) "
"INCLUDE([DatabaseServiceID], [ExpectedAttributeSetForFileID], [FileID], [Attribute], [Correct], [Expected], [ExpectedDateTimeStamp]) ";

static const std::string gstrCREATE_REPORTING_REDACTION_ACCURACY_FOUND_FAMUSERID_WITH_INCLUDES =
"CREATE NONCLUSTERED INDEX[IX_ReportingRedactionAccuracy_FoundFAMUserIDWithIncludes] "
"ON[dbo].[ReportingRedactionAccuracy]([FoundFAMUserID]) "
"INCLUDE([DatabaseServiceID], [FoundAttributeSetForFileID], [FileID], [Attribute], [Correct], [Expected], [FoundDateTimeStamp]) ";

static const std::string gstrREPORTING_HIM_STATS_TABLE = "ReportingHIMStats";

static const std::string gstrCREATE_REPORTING_HIM_STATS_V7 =
"CREATE TABLE[dbo].[ReportingHIMStats]( "
"	[PaginationID][int] NOT NULL CONSTRAINT [PK_ReportingHIMStats] PRIMARY KEY CLUSTERED, "
"	[FAMUserID][int] NOT NULL, "
"	[SourceFileID][int] NOT NULL, "
"	[DestFileID][int] NULL, "
"	[OriginalFileID][int] NOT NULL, "
"	[DateProcessed][date] NULL, "
"	[ActionID][int] NULL, "
"	[FileTaskSessionID][int] NOT NULL, "
"	[ActionName][nvarchar](50) NOT NULL "
"	)";

static const std::string gstrCREATE_REPORTING_HIM_STATS =
"CREATE TABLE[dbo].[ReportingHIMStats]( "
"	[ID][int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_ReportingHIMStats] PRIMARY KEY CLUSTERED, "
"	[PaginationID][int] NULL , "
"	[FAMUserID][int] NOT NULL, "
"	[SourceFileID][int] NOT NULL, "
"	[DestFileID][int] NULL, "
"	[OriginalFileID][int] NOT NULL, "
"	[DateProcessed][date] NULL, "
"	[ActionID][int] NULL, "
"	[FileTaskSessionID][int] NOT NULL, "
"	[ActionName][nvarchar](50) NOT NULL "
"	)";

static const std::string gstrCREATE_FAMUSERID_WITH_INCLUDES_INDEX =
"CREATE NONCLUSTERED INDEX [IX_ReportingHIMStats_FAMUserID_With_Includes] "
"ON[dbo].[ReportingHIMStats]([FAMUserID], [DateProcessed]) "
"INCLUDE([SourceFileID], [DestFileID], [OriginalFileID], [ActionName]) ";

static const std::string gstrCREATE_DESTFILE_WITH_INCLUDES_INDEX =
"CREATE NONCLUSTERED INDEX[IX_ReportingHIMStats_DestFileID] ON[dbo].[ReportingHIMStats] "
"([DestFileID]	) INCLUDE([FAMUserID], [SourceFileID], [OriginalFileID], [DateProcessed], [ActionID], [ActionName])";

static const std::string gstrCreate_ReportingDataCaptureAccuracy_FKS =
" ALTER TABLE[dbo].[ReportingDataCaptureAccuracy]"
" WITH CHECK ADD CONSTRAINT FK_ReportingDataCaptureAccuracy_FAMUser_Found FOREIGN KEY([FoundFAMUserID])"
" REFERENCES[dbo].[FAMUser]([ID])"
" ON UPDATE NO ACTION"
" ON DELETE NO ACTION;"
  
" ALTER TABLE[dbo].[ReportingDataCaptureAccuracy]"
" WITH CHECK ADD CONSTRAINT FK_ReportingDataCaptureAccuracy_FAMUser_Expected FOREIGN KEY([ExpectedFAMUserID])"
" REFERENCES[dbo].[FAMUser]([ID])"
" ON UPDATE NO ACTION"
" ON DELETE NO ACTION;"
  
" ALTER TABLE[dbo].[ReportingDataCaptureAccuracy]"
" WITH CHECK ADD CONSTRAINT FK_ReportingDataCaptureAccuracy_Action_Found FOREIGN KEY([FoundActionID])"
" REFERENCES[dbo].[Action]([ID])"
" ON UPDATE NO ACTION"
" ON DELETE NO ACTION;"
  
" ALTER TABLE[dbo].[ReportingDataCaptureAccuracy]"
" WITH CHECK ADD CONSTRAINT FK_ReportingDataCaptureAccuracy_Action_Expected FOREIGN KEY([ExpectedActionID])"
" REFERENCES[dbo].[Action]([ID])"
" ON UPDATE NO ACTION"
" ON DELETE NO ACTION;";

static const std::string gstrAdd_FKS_REPORTINGHIMSTATS =
"ALTER TABLE [dbo].[ReportingHIMStats]"
" WITH CHECK"
" ADD CONSTRAINT[FK_ReportingHIMStats_Pagination]"
" FOREIGN KEY([PaginationID])"
" REFERENCES[dbo].[Pagination]([ID]) ON"
" UPDATE NO ACTION"
" ON DELETE NO ACTION"

" ALTER TABLE[dbo].[ReportingHIMStats]"
" WITH CHECK"
" ADD CONSTRAINT[FK_ReportingHIMStats_FAMUser]"
" FOREIGN KEY([FAMUserID])"
" REFERENCES[dbo].[FAMUser]([ID])"
" ON UPDATE NO ACTION"
" ON DELETE NO ACTION"

" ALTER TABLE[dbo].[ReportingHIMStats]"
" WITH CHECK"
" ADD CONSTRAINT[FK_ReportingHIMStats_FAMFile_SourceFile]"
" FOREIGN KEY([SourceFileID])"
" REFERENCES[dbo].[FAMFile]([ID])"
" ON UPDATE NO ACTION"
" ON DELETE NO ACTION"

" ALTER TABLE[dbo].[ReportingHIMStats]"
" WITH CHECK"
" ADD CONSTRAINT[FK_ReportingHIMStats_FAMFile_DestFile]"
" FOREIGN KEY([DestFileID])"
" REFERENCES[dbo].[FAMFile]([ID])"
" ON UPDATE NO ACTION"
" ON DELETE NO ACTION"

" ALTER TABLE[dbo].[ReportingHIMStats]"
" WITH CHECK"
" ADD CONSTRAINT[FK_ReportingHIMStats_FAMFile_OriginalFile]"
" FOREIGN KEY([OriginalFileID])"
" REFERENCES[dbo].[FAMFile]([ID])"
" ON UPDATE NO ACTION"
" ON DELETE NO ACTION"

" ALTER TABLE[dbo].[ReportingHIMStats]"
" WITH CHECK"
" ADD CONSTRAINT[FK_ReportingHIMStats_FileTaskSession]"
" FOREIGN KEY([FileTaskSessionID])"
" REFERENCES[dbo].[FileTaskSession]([ID])"
" ON UPDATE NO ACTION"
" ON DELETE NO ACTION"

" ALTER TABLE[dbo].ReportingHIMStats "
" WITH CHECK ADD CONSTRAINT FK_ReportingHIMStats_ActionID FOREIGN KEY([ActionID])"
" REFERENCES[dbo].[Action]([ID])"
" ON UPDATE NO ACTION"
" ON DELETE NO ACTION";

static const std::string gstrAdd_FKS_REPORTINGREDACTIONACCURACY =
"ALTER TABLE[dbo].[ReportingRedactionAccuracy]"
" WITH CHECK ADD CONSTRAINT FK_ReportingRedactionAccuracy_FAMUser_Found FOREIGN KEY([FoundFAMUserID])"
" REFERENCES[dbo].[FAMUser]([ID])"
" ON UPDATE NO ACTION"
" ON DELETE NO ACTION"

" ALTER TABLE[dbo].[ReportingRedactionAccuracy]"
" WITH CHECK ADD CONSTRAINT FK_ReportingRedactionAccuracy_FAMUser_Expected FOREIGN KEY([ExpectedFAMUserID])"
" REFERENCES[dbo].[FAMUser]([ID])"
" ON UPDATE NO ACTION"
" ON DELETE NO ACTION"

" ALTER TABLE[dbo].[ReportingRedactionAccuracy]"
" WITH CHECK ADD CONSTRAINT FK_ReportingRedactionAccuracy_Action_Found FOREIGN KEY([FoundActionID])"
" REFERENCES[dbo].[Action]([ID])"
" ON UPDATE NO ACTION"
" ON DELETE NO ACTION"

" ALTER TABLE[dbo].[ReportingRedactionAccuracy]"
" WITH CHECK ADD CONSTRAINT FK_ReportingRedactionAccuracy_Action_Expected FOREIGN KEY([ExpectedActionID])"
" REFERENCES[dbo].[Action]([ID])"
" ON UPDATE NO ACTION"
" ON DELETE NO ACTION";