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

static const std::string gstrCREATE_ATTRIBUTE_SET_NAME_TABLE_v1 = 
	"CREATE TABLE [dbo].[AttributeSetName] "
	"([ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_AttributeSetName] PRIMARY KEY CLUSTERED, "
	"[Description] [nvarchar](255) NULL CONSTRAINT [Attribute_Set_Name_Description_Unique] UNIQUE(Description))";

static const std::string gstrCREATE_ATTRIBUTE_SET_FOR_FILE_TABLE_v1 = 
	"CREATE TABLE [dbo].[AttributeSetForFile] "
	"([ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_AttributeSetForFile] PRIMARY KEY CLUSTERED, "
	"[FileID] [int] NOT NULL, "						// foreign key, FAMFile.ID
	"[AttributeSetNameID] [int] NOT NULL)";
	
static const std::string gstrCREATE_ATTRIBUTE_NAME_TABLE_v1 = 
	"CREATE TABLE [dbo].[AttributeName] "
	"([ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_AttributeName] PRIMARY KEY CLUSTERED, "
	"[Name] [nvarchar](255) NULL CONSTRAINT [Attribute_Name_Name_Unique] UNIQUE(Name))";

static const std::string gstrCREATE_ATTRIBUTE_TYPE_TABLE_v1 = 
	"CREATE TABLE [dbo].[AttributeType] "
	"([ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_AttributeType] PRIMARY KEY CLUSTERED, "
	"[Type] [nvarchar](255) NULL CONSTRAINT [Attribute_Type_Type_Unique] UNIQUE(Type))";

static const std::string gstrCREATE_ATTRIBUTE_INSTANCE_TYPE_v1 = 
	"CREATE TABLE [dbo].[AttributeInstanceType] "
	"([AttributeID] [int] NOT NULL, "
	"[AttributeTypeID] [int] NOT NULL, "
	"CONSTRAINT [PK_AttributeInstanceType] PRIMARY KEY CLUSTERED ([AttributeID] ASC, [AttributeTypeID] ASC))";

static const std::string gstrCREATE_ATTRIBUTE_TABLE_v1 = 
	"CREATE TABLE [dbo].[Attribute] "
	"([ID] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Attribute] PRIMARY KEY CLUSTERED, "
	"[AttributeSetForFileID] [int] NOT NULL, "		//FK, AttributeSetForFile.ID 
	"[AttributeNameID] [int] NOT NULL, "			//FK, AttributeName.ID 
	"[Value] [nvarchar](max) NOT NULL, "
	"[ParentAttributeID] [int],	"					//FK, Atribute.ID, null allowed
	"[GUID] [uniqueidentifier] NOT NULL)";
	
static const std::string gstrCREATE_RASTER_ZONE_TABLE_v1 = 
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
	"CREATE UNIQUE NONCLUSTERED INDEX [IX_FileID_AttributeSetNameID] ON [dbo].[AttributeSetForFile]([FileID] ASC, [AttributeSetNameID] ASC)";

// NOTE: Originally this index was created, then we discovered that sometimes 
// the GUID would not be unique, so _v2 drops this constraint and then never
// restores it.
/*
The same attribute will be a sub-attribute of multiple attributes and so the same GUID 
can be in the same AttributeSetForFile (or voa file) therefore we can't have the unique 
constraint on the (AttributeSetForFileID, GUID).
The situation is with the metadata saved for each session of IDShield verification, the 
same _IDAndRevision attribute under multiple attributes. When saved in the database it 
would be a case where the there would be multiple Attributes with the same GUID and 
AttributeSetForFileID and will have different ParentAttributeID.
*/
static const std::string gstrCREATE_ATTRIBUTE_SET_FOR_FILEID_GUID_INDEX =
	"CREATE UNIQUE NONCLUSTERED INDEX [IX_AttributeSetForFileID_GUID] ON [dbo].[Attribute] ([AttributeSetForFileID] ASC, [GUID] ASC)";

// All version 2 alterations are here. Version 2 changes many of the PK and FK 
// columns from int to bigint. The exception to this rule is that any key referencing
// a FileID will be int, because that is the type of FAMFile.ID.
// Ordered for execution - basically put all the DROP CONSTRAINT operations together, then all the ALTER COLUMN
// operations, finally redefine all the PK and FK constraints (which again have dependencies). 
// This prevents failures due to FK dependencies.

static const std::string gstrDROP_CONSTRAINTS = 
"ALTER TABLE [dbo].[AttributeInstanceType] DROP CONSTRAINT [PK_AttributeInstanceType];\n"
"ALTER TABLE [dbo].[AttributeInstanceType] DROP CONSTRAINT [FK_Attribute_Instance_Type_AttributeID];\n"
"ALTER TABLE [dbo].[AttributeInstanceType] DROP CONSTRAINT [FK_Attribute_Instance_Type_AttributeTypeID];\n"
"ALTER TABLE [dbo].[RasterZone] DROP CONSTRAINT [PK_RasterZone];\n"
"ALTER TABLE [dbo].[RasterZone] DROP CONSTRAINT [FK_RasterZone_AttributeID];\n"
"ALTER TABLE [dbo].[Attribute] DROP CONSTRAINT [FK_Attribute_ParentAttributeID];\n"
"ALTER TABLE [dbo].[Attribute] DROP CONSTRAINT [PK_Attribute];\n"
"ALTER TABLE [dbo].[Attribute] DROP CONSTRAINT [FK_Attribute_AttributeSetForFileID];\n"
"ALTER TABLE [dbo].[Attribute] DROP CONSTRAINT [FK_Attribute_AttributeNameID];\n"
"DROP INDEX [IX_AttributeSetForFileID_GUID] ON [dbo].[Attribute];\n"
"ALTER TABLE [dbo].[AttributeSetName] DROP CONSTRAINT [PK_AttributeSetName];\n"
"ALTER TABLE [dbo].[AttributeSetForFile] DROP CONSTRAINT [PK_AttributeSetForFile];\n"
"DROP INDEX [IX_FileID_AttributeSetNameID] ON [dbo].[AttributeSetForFile];\n"
"ALTER TABLE [dbo].[AttributeName] DROP CONSTRAINT [PK_AttributeName];\n"
"ALTER TABLE [dbo].[AttributeType] DROP CONSTRAINT [PK_AttributeType];";

// now redefine all column types **********************************************
static const std::string gstrREDEFINE_COLUMN_TYPES = 
"ALTER TABLE [dbo].[AttributeSetName] ALTER COLUMN ID bigint;\n"
"ALTER TABLE [dbo].[AttributeSetForFile] ALTER COLUMN [ID] bigint;\n"
"ALTER TABLE [dbo].[AttributeSetForFile] ALTER COLUMN [AttributeSetNameID] bigint NOT NULL;\n"
"ALTER TABLE [dbo].[AttributeName] ALTER COLUMN ID bigint;\n"
"ALTER TABLE [dbo].[AttributeType] ALTER COLUMN ID bigint;\n"
"ALTER TABLE [dbo].[AttributeInstanceType] ALTER COLUMN [AttributeID] bigint NOT NULL;\n"
"ALTER TABLE [dbo].[AttributeInstanceType] ALTER COLUMN [AttributeTypeID] bigint NOT NULL;\n"
"ALTER TABLE [dbo].[Attribute] ALTER COLUMN [ID] bigint;\n"
"ALTER TABLE [dbo].[Attribute] ALTER COLUMN [AttributeSetForFileID] bigint NOT NULL;\n"
"ALTER TABLE [dbo].[Attribute] ALTER COLUMN [AttributeNameID] bigint NOT NULL;\n"
"ALTER TABLE [dbo].[Attribute] ALTER COLUMN [ParentAttributeID] bigint;\n"
"ALTER TABLE [dbo].[RasterZone] ALTER COLUMN [ID] bigint;\n"
"ALTER TABLE [dbo].[RasterZone] ALTER COLUMN [AttributeID] bigint NOT NULL;";

// define PKs and FKs *********************************************************
static const std::string gstrDEFINE_PKS_AND_FKS = 
"ALTER TABLE [dbo].[AttributeSetName] ADD CONSTRAINT [PK_AttributeSetName] PRIMARY KEY CLUSTERED ([ID] ASC);\n"
"ALTER TABLE [dbo].[AttributeSetForFile] ADD CONSTRAINT [PK_AttributeSetForFile] PRIMARY KEY CLUSTERED ([ID] ASC);\n"
"ALTER TABLE [dbo].[AttributeSetForFile] WITH CHECK ADD CONSTRAINT [FK_AttributeSetForFile_AttributeSetNameID] FOREIGN KEY([AttributeSetNameID]) REFERENCES [AttributeSetName] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE;\n"
"CREATE UNIQUE NONCLUSTERED INDEX [IX_FileID_AttributeSetNameID] ON [dbo].[AttributeSetForFile]([FileID] ASC, [AttributeSetNameID] ASC);\n"
"ALTER TABLE [dbo].[AttributeName] ADD CONSTRAINT [PK_AttributeName] PRIMARY KEY CLUSTERED ([ID] ASC);\n"
"ALTER TABLE [dbo].[AttributeType] ADD CONSTRAINT [PK_AttributeType] PRIMARY KEY CLUSTERED ([ID] ASC);\n"
"ALTER TABLE [dbo].[AttributeInstanceType] ADD CONSTRAINT [PK_AttributeInstanceType] PRIMARY KEY CLUSTERED ([AttributeID] ASC, [AttributeTypeID] ASC);\n"
"ALTER TABLE [dbo].[Attribute] ADD CONSTRAINT [PK_Attribute] PRIMARY KEY CLUSTERED ([ID] ASC);\n"
"ALTER TABLE [dbo].[Attribute] WITH CHECK ADD CONSTRAINT [FK_Attribute_AttributeSetForFileID] FOREIGN KEY([AttributeSetForFileID]) REFERENCES [AttributeSetForFile] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE;\n"
"ALTER TABLE [dbo].[Attribute] WITH CHECK ADD CONSTRAINT [FK_Attribute_AttributeNameID] FOREIGN KEY([AttributeNameID]) REFERENCES [AttributeName] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE;\n"
"ALTER TABLE [dbo].[Attribute] WITH CHECK ADD CONSTRAINT [FK_Attribute_ParentAttributeID] FOREIGN KEY([ParentAttributeID]) REFERENCES [Attribute] ([ID]);\n"
"ALTER TABLE [dbo].[AttributeInstanceType] WITH CHECK ADD CONSTRAINT [FK_Attribute_Instance_Type_AttributeID] FOREIGN KEY([AttributeID]) REFERENCES [Attribute] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE;\n"
"ALTER TABLE [dbo].[AttributeInstanceType] WITH CHECK ADD CONSTRAINT [FK_Attribute_Instance_Type_AttributeTypeID] FOREIGN KEY([AttributeTypeID]) REFERENCES [AttributeType] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE;\n"
"ALTER TABLE [dbo].[RasterZone] ADD CONSTRAINT [PK_RasterZone] PRIMARY KEY CLUSTERED ([ID] ASC);\n"
"ALTER TABLE [dbo].[RasterZone] WITH CHECK ADD CONSTRAINT [FK_RasterZone_AttributeID] FOREIGN KEY([AttributeID]) REFERENCES [Attribute] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE;";

// All version 2 tables are redefined here.
// 10.3 table (name) additions

static const std::string gstrCREATE_ATTRIBUTE_SET_NAME_TABLE_v2 = 
	"CREATE TABLE [dbo].[AttributeSetName] "
	"([ID] [bigint] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_AttributeSetName] PRIMARY KEY CLUSTERED, "
	"[Description] [nvarchar](255) NULL CONSTRAINT [Attribute_Set_Name_Description_Unique] UNIQUE(Description))";

static const std::string gstrCREATE_ATTRIBUTE_SET_FOR_FILE_TABLE_v2 = 
	"CREATE TABLE [dbo].[AttributeSetForFile] "
	"([ID] [bigint] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_AttributeSetForFile] PRIMARY KEY CLUSTERED, "
	"[FileID] [int] NOT NULL, "						// foreign key, FAMFile.ID
	"[AttributeSetNameID] [bigint] NOT NULL)";		// foreign key, AttributeSetName.ID
	
static const std::string gstrCREATE_ATTRIBUTE_NAME_TABLE_v2 = 
	"CREATE TABLE [dbo].[AttributeName] "
	"([ID] [bigint] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_AttributeName] PRIMARY KEY CLUSTERED, "
	"[Name] [nvarchar](255) NULL CONSTRAINT [Attribute_Name_Name_Unique] UNIQUE(Name))";

static const std::string gstrCREATE_ATTRIBUTE_TYPE_TABLE_v2 = 
	"CREATE TABLE [dbo].[AttributeType] "
	"([ID] [bigint] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_AttributeType] PRIMARY KEY CLUSTERED, "
	"[Type] [nvarchar](255) NULL CONSTRAINT [Attribute_Type_Type_Unique] UNIQUE(Type))";

static const std::string gstrCREATE_ATTRIBUTE_INSTANCE_TYPE_v2 = 
	"CREATE TABLE [dbo].[AttributeInstanceType] "
	"([AttributeID] [bigint] NOT NULL, "
	"[AttributeTypeID] [bigint] NOT NULL, "
	"CONSTRAINT [PK_AttributeInstanceType] PRIMARY KEY CLUSTERED ([AttributeID] ASC, [AttributeTypeID] ASC))";

static const std::string gstrCREATE_ATTRIBUTE_TABLE_v2 = 
	"CREATE TABLE [dbo].[Attribute] "
	"([ID] [bigint] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Attribute] PRIMARY KEY CLUSTERED, "
	"[AttributeSetForFileID] [bigint] NOT NULL, "		//FK, AttributeSetForFile.ID 
	"[AttributeNameID] [bigint] NOT NULL, "				//FK, AttributeName.ID
	"[Value] [nvarchar](max) NOT NULL, "
	"[ParentAttributeID] [bigint],	"					//FK, Atribute.ID, null allowed
	"[GUID] [uniqueidentifier] NOT NULL)";
	
static const std::string gstrCREATE_RASTER_ZONE_TABLE_v2 = 
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

// 10.3 version 2 table FK additions are the same as version 1, so not re-defined here.

