// DataEntry_DB_SQL.h - Constants for DB SQL queries that are DataEntry Specific

#pragma once

#include <string>

using namespace std;

//--------------------------------------------------------------------------------------------------
// DataEntryData
//--------------------------------------------------------------------------------------------------

// DataEntry data table name
static const string gstrDATA_ENTRY_DATA = "DataEntryData";

// Create Table SQL statements
static const string gstrCREATE_DATAENTRY_DATA = 
	"CREATE TABLE [dbo].[DataEntryData]( "
	" [ID] [int] IDENTITY(1,1) NOT NULL "
	" CONSTRAINT [PK_DataEntryData] PRIMARY KEY CLUSTERED, "
	" [FileID] [int] NULL, "
	" [UserID] [int] NULL, "
	" [ActionID] [int] NULL, "
	" [MachineID] [int] NULL, "
	" [DateTimeStamp] [datetime] NULL, "
	" [Duration] [float] NULL, "
	" [TotalDuration] [float] NULL)";

// Query to add DataEntryData - FAMFile foreign key
static const string gstrADD_FK_DATAENTRY_FAMFILE =
	"ALTER TABLE [DataEntryData]  "
	" WITH CHECK ADD CONSTRAINT [FK_DataEntryData_FAMFile] FOREIGN KEY([FileID]) "
	" REFERENCES [FAMFile] ([ID]) "
	" ON UPDATE CASCADE "
	" ON DELETE CASCADE";

// Query to add DataEntryData - FAMUser foreign key
static const string gstrADD_FK_DATAENTRYDATA_FAMUSER = 
	"ALTER TABLE [dbo].[DataEntryData]  "
	" WITH CHECK ADD CONSTRAINT [FK_DataEntryData_FAMUser] FOREIGN KEY([UserID]) "
	" REFERENCES [dbo].[FAMUser] ([ID])";

// Query to add DataEntryData - Action foreign key
static const string gstrADD_FK_DATAENTRYDATA_ACTION = 
	"ALTER TABLE [dbo].[DataEntryData]  "
	" WITH CHECK ADD CONSTRAINT [FK_DataEntryData_Action] FOREIGN KEY([ActionID]) "
	" REFERENCES [dbo].[Action] ([ID])"
	" ON UPDATE CASCADE "
	" ON DELETE CASCADE";

// Query to add DataEntryData - Machine foreign key
static const string gstrADD_FK_DATAENTRYDATA_MACHINE =
	"ALTER TABLE [dbo].[DataEntryData]  "
	" WITH CHECK ADD CONSTRAINT [FK_DataEntryData_Machine] FOREIGN KEY([MachineID])"
	" REFERENCES [dbo].[Machine] ([ID])";

// Query to add index of FileID and DateTimeStamp fields
static const string gstrCREATE_FILEID_DATETIMESTAMP_INDEX = 
	"CREATE NONCLUSTERED INDEX [IX_FileID_DateTimeStamp] ON [dbo].[DataEntryData] "
	"( [FileID] ASC, [DateTimeStamp] ASC )";

// Query for inserting record into DataEntryData table - this query requires the
// completion of the VALUES Clause
static const string gstrINSERT_DATAENTRY_DATA_RCD = 
	"INSERT INTO [dbo].[DataEntryData] "
	" ([FileID]"
    "  ,[UserID]"
	"  ,[ActionID]"
    "  ,[MachineID]"
    "  ,[DateTimeStamp]"
    "  ,[Duration]"
	"  ,[TotalDuration]) "
	"  VALUES ";

// Deletes all records in the DataEntryData table that 
// have the given file ID, and the specified Verified value.
// This should be run before adding a new record if
// history is not being kept.
// Requires the <FileID> be replaced with the appropriate
// values.
static const string gstrDELETE_PREVIOUS_STATUS_FOR_FILEID = 
	"DELETE FROM DataEntryData "
	"WHERE FileID = <FileID>";

//--------------------------------------------------------------------------------------------------
// DataEntryCounterDefinition
//--------------------------------------------------------------------------------------------------

// DataEntry counter definition table name
static const string gstrDATAENTRY_DATA_COUNTER_DEFINITION = "DataEntryCounterDefinition";

// Create Table SQL statements
static const string gstrCREATE_DATAENTRY_COUNTER_DEFINITION = 
	"CREATE TABLE [dbo].[DataEntryCounterDefinition]( "
	" [ID] INT IDENTITY(1,1) NOT NULL "
	" CONSTRAINT [PK_DataEntryCounterDefinition] PRIMARY KEY CLUSTERED, "
	" [Name] NVARCHAR(50) NOT NULL, "
	" [AttributeQuery] NVARCHAR(255) NOT NULL, "
	" [RecordOnLoad] BIT NOT NULL, "
	" [RecordOnSave] BIT NOT NULL)";

//--------------------------------------------------------------------------------------------------
// DataEntryCounterType
//--------------------------------------------------------------------------------------------------

// DataEntry counter type table name
static const string gstrDATAENTRY_DATA_COUNTER_TYPE = "DataEntryCounterType";

// Create Table SQL statements
static const string gstrCREATE_DATAENTRY_COUNTER_TYPE = 
	"CREATE TABLE [dbo].[DataEntryCounterType]( "
	" [Type] NVARCHAR(1) NOT NULL "
	" CONSTRAINT [PK_DataEntryCounterType] PRIMARY KEY CLUSTERED, "
	" [Description] NVARCHAR(255) NOT NULL)";

// Populate the DataEntryCounterType table.
static const string gstrPOPULATE_DATAENTRY_COUNTER_TYPES = 
	"INSERT INTO [dbo].[DataEntryCounterType] VALUES "
	"('L', 'OnLoad'); "
	"INSERT INTO [dbo].[DataEntryCounterType] VALUES "
	"('S', 'OnSave')";

//--------------------------------------------------------------------------------------------------
// DataEntryCounterValue
//--------------------------------------------------------------------------------------------------

// DataEntry counter value table name
static const string gstrDATAENTRY_DATA_COUNTER_VALUE = "DataEntryCounterValue";

// Create Table SQL statements
static const string gstrCREATE_DATAENTRY_COUNTER_VALUE = 
	"CREATE TABLE [dbo].[DataEntryCounterValue]( "
	" [InstanceID] INT NOT NULL, "
	" [CounterID] INT NOT NULL, "
	" [Type] NVARCHAR(1) NOT NULL, "
	" [Value] INT NULL)";

// Query to add DataEntryCounterValue - Instance foreign key
static const string gstrADD_FK_DATAENTRY_COUNTER_VALUE_INSTANCE = 
	"ALTER TABLE [dbo].[DataEntryCounterValue]  "
	" WITH CHECK ADD CONSTRAINT [FK_DataEntryCounterValue_Instance] FOREIGN KEY([InstanceID]) "
	" REFERENCES [dbo].[DataEntryData] ([ID])"
	" ON UPDATE CASCADE "
	" ON DELETE CASCADE";

// Query to add DataEntryCounterValue - ID foreign key
static const string gstrADD_FK_DATAENTRY_COUNTER_VALUE_ID = 
	"ALTER TABLE [dbo].[DataEntryCounterValue]  "
	" WITH CHECK ADD CONSTRAINT [FK_DataEntryCounterValue_ID] FOREIGN KEY([CounterID]) "
	" REFERENCES [dbo].[DataEntryCounterDefinition] ([ID])";
// Updates/deletes will already be cascaded from DataEntryData table

// Query to add DataEntryCounterValue - Type foreign key
static const string gstrADD_FK_DATAENTRY_COUNTER_VALUE_TYPE = 
	"ALTER TABLE [dbo].[DataEntryCounterValue]  "
	" WITH CHECK ADD CONSTRAINT [FK_DataEntryCounterValue_Type] FOREIGN KEY([Type]) "
	" REFERENCES [dbo].[DataEntryCounterType] ([Type])"
	" ON UPDATE CASCADE "
	" ON DELETE CASCADE";

// Query to record counts into the DataEntryCounterValue table. Requires the completion of the
// VALUES clause.
static const string gstrINSERT_DATAENTRY_COUNTER_VALUE =
	"INSERT INTO [dbo].[DataEntryCounterValue] VALUES"; 
