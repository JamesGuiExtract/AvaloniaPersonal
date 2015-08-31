// Attribute_DB_SQL.h - Constants for DB SQL queries that are Attribute Specific

#pragma once

#include <string>

// 10.3 table (name) additions
static const string gstrATTRIBUTE_SET_NAME = "AttributeSetName";
static const string gstrATTRIBUTE_SET_FOR_FILE = "AttributeSetForFile";
static const string gstrATTRIBUTE_NAME = "AttributeName";
static const string gstrATTRIBUTE_TYPE = "AttributeType";
static const string gstrATTRIBUTE_INSTANCE_TYPE = "AttributeInstanceType";
static const string gstrATTRIBUTE = "Attribute";
static const string gstrRASTER_ZONE = "RasterZone";

static const std::string gstrCREATE_ATTRIBUTE_SET_NAME_TABLE = 
	"CREATE TABLE [dbo].[AttributeSetName] "
	"([ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_AttributeSetName] PRIMARY KEY CLUSTERED, "
	"[Description] [nvarchar](255) NULL CONSTRAINT [Attribute_Set_Name_Description_Unique] UNIQUE(Description))";

static const std::string gstrCREATE_ATTRIBUTE_SET_FOR_FILE_TABLE = 
	"CREATE TABLE [dbo].[AttributeSetForFile] "
	"([ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_AttributeSetForFile] PRIMARY KEY CLUSTERED, "
	"[FileID] [int] NOT NULL, "						// foreign key, FAMFile.ID
	"[AttributeSetNameID] [int] NOT NULL)";
	
static const std::string gstrCREATE_FILEID_ATTRIBUTE_SET_NAME_ID_INDEX = 
	"CREATE UNIQUE NONCLUSTERED INDEX [IX_FileID_AttributeSetNameID] ON [dbo].[AttributeSetForFile]([FileID] ASC, [AttributeSetNameID] ASC)";

static const std::string gstrCREATE_ATTRIBUTE_NAME_TABLE = 
	"CREATE TABLE [dbo].[AttributeName] "
	"([ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_AttributeName] PRIMARY KEY CLUSTERED, "
	"[Name] [nvarchar](255) NULL CONSTRAINT [Attribute_Name_Name_Unique] UNIQUE(Name))";

static const std::string gstrCREATE_ATTRIBUTE_TYPE_TABLE = 
	"CREATE TABLE [dbo].[AttributeType] "
	"([ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_AttributeType] PRIMARY KEY CLUSTERED, "
	"[Type] [nvarchar](255) NULL CONSTRAINT [Attribute_Type_Type_Unique] UNIQUE(Type))";

static const std::string gstrCREATE_ATTRIBUTE_INSTANCE_TYPE = 
	"CREATE TABLE [dbo].[AttributeInstanceType] "
	"([AttributeID] [int] NOT NULL, "
	"[AttributeTypeID] [int] NOT NULL, "
	"CONSTRAINT [PK_AttributeInstanceType] PRIMARY KEY CLUSTERED ([AttributeID] ASC, [AttributeTypeID] ASC))";

static const std::string gstrCREATE_ATTRIBUTE_TABLE = 
	"CREATE TABLE [dbo].[Attribute] "
	"([ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Attribute] PRIMARY KEY CLUSTERED, "
	"[AttributeSetForFileID] [int] NOT NULL, "		//FK, AttributeSetForFile.ID 
	"[AttributeNameID] [int] NOT NULL, "			//FK, AttributeName.ID 
	"[Value] [nvarchar](max) NOT NULL, "
	"[ParentAttributeID] [int],	"					//FK, Atribute.ID, null allowed
	"[GUID] [uniqueidentifier] NOT NULL)";
	
static const std::string gstrCREATE_ATTRIBUTE_SET_FOR_FILEID_GUID_INDEX =
	"CREATE UNIQUE NONCLUSTERED INDEX [IX_AttributeSetForFileID_GUID] ON [dbo].[Attribute] ([AttributeSetForFileID] ASC, [GUID] ASC)";

static const std::string gstrCREATE_RASTER_ZONE_TABLE = 
	"CREATE TABLE [dbo].[RasterZone] "
	"([ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_RasterZone] PRIMARY KEY CLUSTERED, "
	"[AttributeID] [int] NOT NULL, "				//FK, Attribute.ID
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
static const std::string gstrADD_ATTRIBUTE_SET_FOR_FILEID_FK = 
	"ALTER TABLE [AttributeSetForFile] "
	"WITH CHECK ADD CONSTRAINT [FK_AttributeSetForFile_FileID] FOREIGN KEY([FileID]) "
	"REFERENCES [FAMFILE] ([ID]) "
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
