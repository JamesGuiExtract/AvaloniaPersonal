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

// 10.3 table indexes are here...
static const std::string gstrCREATE_FILEID_ATTRIBUTE_SET_NAME_ID_INDEX = 
	"CREATE NONCLUSTERED INDEX [IX_FileTaskSessionID_AttributeSetNameID] ON [dbo].[AttributeSetForFile]([FileTaskSessionID] ASC, [AttributeSetNameID] ASC);\n";


// ****************************************************************************
// version 2 update- add a binary column to AttributeSetForFile, to store the complete spatial string for the document.
static const std::string gstrADD_ATTRIBUTE_SET_FOR_FILE_VOA_COLUMN = 
	"ALTER TABLE [dbo].[AttributeSetForFile] ADD [VOA] [varbinary](max) NULL";
	