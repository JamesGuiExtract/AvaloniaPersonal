// IDShield_DB_SQL.h - Constants for DB SQL queries that are IDShield Specific

#pragma once

#include <string>

using namespace std;

// IDShield data table name
static const string gstrIDSHIELD_DATA = "IDShieldData";

// Create Table SQL statements
static const string gstrCREATE_IDSHIELD_DATA = 
	"CREATE TABLE [dbo].[IDShieldData]( "
	" [ID] [int] IDENTITY(1,1) NOT NULL "
	" CONSTRAINT [PK_IDShieldData] PRIMARY KEY CLUSTERED, "
	" [FileID] [int] NULL, "
	" [Verified] [bit] NULL, "
	" [UserID] [int] NULL, "
	" [MachineID] [int] NULL, "
	" [DateTimeStamp] [datetime] NULL, "
	" [Duration] [float] NULL, "
	" [TotalDuration] [float] NULL, "
	" [NumHCDataFound] [int] NULL, "
	" [NumMCDataFound] [int] NULL, "
	" [NumLCDataFound] [int] NULL, "
	" [NumCluesFound] [int] NULL, "
	" [TotalManualRedactions] [int] NULL, "
	" [TotalRedactions] [int] NULL, "
	" [NumPagesAutoAdvanced] [int] NULL)";

// Query to add IDShieldData - FAMFile foreign key
static const string gstrADD_FK_IDSHIELD_FAMFILE =
	"ALTER TABLE [IDShieldData]  "
	" WITH CHECK ADD CONSTRAINT [FK_IDShieldData_FAMFile] FOREIGN KEY([FileID]) "
	" REFERENCES [FAMFile] ([ID]) "
	" ON UPDATE CASCADE "
	" ON DELETE CASCADE";

// Query to add IDShieldData - FAMFile foreign key check constraint
static const string gstrADD_CHECK_FK_IDSHIELDDATA_FAMFILE = 
	"ALTER TABLE [dbo].[IDShieldData] CHECK CONSTRAINT [FK_IDShieldData_FAMFile]";

// Query to add IDShieldData - FAMUser foreign key
static const string gstrADD_FK_IDSHIELDDATA_FAMUSER = 
	"ALTER TABLE [dbo].[IDShieldData]  "
	" ADD CONSTRAINT [FK_IDShieldData_FAMUser] FOREIGN KEY([UserID]) "
	" REFERENCES [dbo].[FAMUser] ([ID])";

// Query to add IDShieldData - FAMUser foreign key check constraint
static const string gstrADD_CHECK_FK_IDSHIELDDATA_FAMUSER = 
	"ALTER TABLE [dbo].[IDShieldData] CHECK CONSTRAINT [FK_IDShieldData_FAMUser]";


// Query to add IDShieldData - Machine foreign key
static const string gstrADD_FK_IDSHIELDDATA_MACHINE =
	"ALTER TABLE [dbo].[IDShieldData]  "
	" ADD CONSTRAINT [FK_IDShieldData_Machine] FOREIGN KEY([MachineID])"
	" REFERENCES [dbo].[Machine] ([ID])";

// Query to add IDShieldData - Machine foreign key check constraint
static const string gstrADD_CHECK_FK_IDSHIELDDATA_MACHINE = 
	"ALTER TABLE [dbo].[IDShieldData] CHECK CONSTRAINT [FK_IDShieldData_Machine]";

// Query to add index of FileID and DateTimeStamp fields
static const string gstrCREATE_FILEID_DATETIMESTAMP_INDEX = 
	"CREATE NONCLUSTERED INDEX [IX_FileID_DateTimeStamp] ON [dbo].[IDShieldData] "
	"( [FileID] ASC, [DateTimeStamp] ASC )";

// Query for inserting record into IDShieldData table - this query requires the
// completion of the VALUES Clause
static const string gstrINSERT_IDSHIELD_DATA_RCD = 
	"INSERT INTO [dbo].[IDShieldData] "
	" ([FileID]"
    "  ,[Verified]"
    "  ,[UserID]"
    "  ,[MachineID]"
    "  ,[DateTimeStamp]"
    "  ,[Duration]"
	"  ,[TotalDuration]"
    "  ,[NumHCDataFound]"
    "  ,[NumMCDataFound]"
    "  ,[NumLCDataFound]"
    "  ,[NumCluesFound]"
    "  ,[TotalRedactions]"
    "  ,[TotalManualRedactions]"
	"  ,[NumPagesAutoAdvanced]) "
	"  VALUES ";

// Deletes all records in the IDShieldData table that 
// have the given file ID, and the specified Verified value.
// This should be ran before adding a new record if
// history is not being kept.
// Requires the <FileID>  and <Verified> be replaced with
// the appropriate values
static const string gstrDELETE_PREVIOUS_STATUS_FOR_FILEID = 
	"DELETE FROM IDShieldData "
	"WHERE FileID = <FileID> AND Verified = <Verified>";