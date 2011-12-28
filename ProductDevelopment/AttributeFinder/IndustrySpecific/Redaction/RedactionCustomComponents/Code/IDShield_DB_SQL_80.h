// IDShield_DB_SQL_80.h - Constants for DB SQL queries that are IDShield Specific
// These are the queries as of schema version 2, which corresponds to IDShield 8.0

#pragma once

#include <string>

using namespace std;

// IDShield data table name
static const string gstrIDSHIELD_DATA_80 = "IDShieldData";

// Create Table SQL statements
static const string gstrCREATE_IDSHIELD_DATA_80 = 
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
	" [TotalRedactions] [int] NULL)";

// Query to add IDShieldData - FAMFile foreign key
static const string gstrADD_FK_IDSHIELD_FAMFILE_80 =
	"ALTER TABLE [IDShieldData]  "
	" WITH CHECK ADD CONSTRAINT [FK_IDShieldData_FAMFile] FOREIGN KEY([FileID]) "
	" REFERENCES [FAMFile] ([ID]) "
	" ON UPDATE CASCADE "
	" ON DELETE CASCADE";

// Query to add IDShieldData - FAMFile foreign key check constraint
static const string gstrADD_CHECK_FK_IDSHIELDDATA_FAMFILE_80 = 
	"ALTER TABLE [dbo].[IDShieldData] CHECK CONSTRAINT [FK_IDShieldData_FAMFile]";

// Query to add IDShieldData - FAMUser foreign key
static const string gstrADD_FK_IDSHIELDDATA_FAMUSER_80 = 
	"ALTER TABLE [dbo].[IDShieldData]  "
	" ADD CONSTRAINT [FK_IDShieldData_FAMUser] FOREIGN KEY([UserID]) "
	" REFERENCES [dbo].[FAMUser] ([ID])";

// Query to add IDShieldData - FAMUser foreign key check constraint
static const string gstrADD_CHECK_FK_IDSHIELDDATA_FAMUSER_80 = 
	"ALTER TABLE [dbo].[IDShieldData] CHECK CONSTRAINT [FK_IDShieldData_FAMUser]";


// Query to add IDShieldData - Machine foreign key
static const string gstrADD_FK_IDSHIELDDATA_MACHINE_80 =
	"ALTER TABLE [dbo].[IDShieldData]  "
	" ADD CONSTRAINT [FK_IDShieldData_Machine] FOREIGN KEY([MachineID])"
	" REFERENCES [dbo].[Machine] ([ID])";

// Query to add IDShieldData - Machine foreign key check constraint
static const string gstrADD_CHECK_FK_IDSHIELDDATA_MACHINE_80 = 
	"ALTER TABLE [dbo].[IDShieldData] CHECK CONSTRAINT [FK_IDShieldData_Machine]";

// Query to add index of FileID and DateTimeStamp fields
static const string gstrCREATE_FILEID_DATETIMESTAMP_INDEX_80 = 
	"CREATE NONCLUSTERED INDEX [IX_FileID_DateTimeStamp] ON [dbo].[IDShieldData] "
	"( [FileID] ASC, [DateTimeStamp] ASC )";