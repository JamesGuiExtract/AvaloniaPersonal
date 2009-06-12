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
	" [IsMostRecentUpdate] [bit] NULL, "
	" [Verified] [bit] NULL, "
	" [UserID] [int] NULL, "
	" [MachineID] [int] NULL, "
	" [DateTimeStamp] [datetime] NULL, "
	" [Duration] [float] NULL, "
	" [NumHCDataFound] [int] NULL, "
	" [NumMCDataFound] [int] NULL, "
	" [NumLCDataFound] [int] NULL, "
	" [NumCluesFound] [int] NULL, "
	" [TotalManualRedactions] [int] NULL, "
	" [TotalRedactions] [int] NULL)";

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
    "  ,[IsMostRecentUpdate]"
    "  ,[Verified]"
    "  ,[UserID]"
    "  ,[MachineID]"
    "  ,[DateTimeStamp]"
    "  ,[Duration]"
    "  ,[NumHCDataFound]"
    "  ,[NumMCDataFound]"
    "  ,[NumLCDataFound]"
    "  ,[NumCluesFound]"
    "  ,[TotalRedactions]"
    "  ,[TotalManualRedactions]) "
	"  VALUES ";

// Updates the [IsMostRecentUpdate] column to 0 
// and requires the FileID to be appended
static const string gstrUPDATE_MOST_RECENT_STATUS =
	"UPDATE IDShieldData "
	"SET [IsMostRecentUpdate] = 0 "
	"WHERE FileID = ";

// Deletes all records in the IDShieldData table that 
// have the given file ID, this should be ran before adding a new record if
// history is not being kept
// Requires the FileID to be appended
static const string gstrDELETE_PREVIOUS_STATUS_FOR_FILEID = 
	"DELETE FROM IDShieldData "
	"WHERE FileID = ";

// Query to generate a recordset with the Project Info data for
// an action name.  
static const string gstrREPORT_PROJECT_INFO_QUERY =
	"DECLARE @TotalDocsInProject int;  "
	"DECLARE @TotalPagesInProject int; "
	"DECLARE @TotalDocsVerified int; "
	"DECLARE @TotalPagesInVerifiedDocs int; "
	"DECLARE @TotalDocsRedacted int; "
	"DECLARE @TotalPagesInRedactedDocs int; "
	"DECLARE @TotalProjectTime float; "
	"DECLARE @TotalDocsVerifiedNotRedacted int; "
	"DECLARE @TotalPagesInVerifiedNotRedacted int; "
	
	";WITH IDShieldDocData (Verified, Redacted, Pages, PagesInRedacted, DocsVerifiedNotRedacted, PagesInVerifiedNotRedacted) "
	"AS "
	"( "
	"	SELECT  COALESCE((CASE Verified WHEN 0 THEN 0 ELSE 1 END), 0) as Verified, "
	"			COALESCE((CASE TotalRedactions WHEN 0 THEN 0 ELSE 1 END), 0) AS Redacted, "
	"			COALESCE((CASE Verified WHEN 0 THEN 0 ELSE FAMFile.Pages END), 0) AS Pages, "
	"			COALESCE((CASE TotalRedactions WHEN 0 THEN 0 ELSE FAMFile.Pages END), 0) AS PagesInRedacted, "
	"			COALESCE((CASE TotalRedactions WHEN 0 THEN "
	"					COALESCE((CASE Verified WHEN 0 THEN 0 ELSE 1 END), 0) ELSE 0 END), 0) AS DocsVerifiedNotRedacted, "
	"			COALESCE((CASE TotalRedactions WHEN 0 THEN "
	"					COALESCE((CASE Verified WHEN 0 THEN 0 ELSE FAMFile.Pages END), 0) ELSE 0 END), 0) AS PagesInVerifiedNotRedacted "
	"	FROM         IDShieldData INNER JOIN "
	"					FAMFile ON IDShieldData.FileID = FAMFile.ID "
	"	WHERE     (IDShieldData.IsMostRecentUpdate = 1) "
	") "
	"SELECT @TotalDocsVerified = SUM(Verified),  @TotalDocsRedacted = SUM(Redacted), "
	"		@TotalPagesInVerifiedDocs = SUM(Pages), @TotalPagesInRedactedDocs = SUM(PagesInRedacted), "
	"		@TotalDocsVerifiedNotRedacted = SUM(DocsVerifiedNotRedacted), "
	"		@TotalPagesInVerifiedNotRedacted = SUM(PagesInVerifiedNotRedacted) "
	"FROM IDShieldDocData "
	
	"SELECT     @TotalDocsInProject = COUNT(ID), @TotalPagesInProject = SUM(Pages) "
	"FROM         FAMFile "

	";WITH ProjectTimes (ProjectStartTime, ProjectEndTime) "
	"AS "
	"( "
	"	SELECT    "
	"			MIN(DATEADD( ms, - (Duration - floor(Duration/86400) * 86400) * 1000, "
	"				DATEADD( d, -(floor(Duration/86400)), DateTimeStamp))) AS ProjectStartTime, "
	"			MAX(DateTimeStamp) AS ProjectEndTime "
	"	FROM         IDShieldData "
	"	WHERE     (IsMostRecentUpdate = 1 AND Verified = 1) "
	") "
	"SELECT @TotalProjectTime = DateDiff(ms, DateAdd(d, "
	"					DateDiff(d, ProjectStartTime, ProjectEndTime), ProjectStartTime), ProjectEndTime)/1000.0 + "
	"					DateDiff(d, ProjectStartTime, ProjectEndTime) * 86400 "
	"FROM ProjectTimes; "

	"SELECT "
	"	COALESCE(@TotalProjectTime, 0.0) as TotalProjectTime, "
	"	COALESCE(@TotalDocsInProject, 0) as TotalDocsInProject, "
	"	COALESCE(@TotalPagesInProject, 0) as TotalPagesInProject, "
	"	COALESCE(@TotalDocsVerified, 0) AS TotalDocsVerified, "
	"	COALESCE(@TotalPagesInVerifiedDocs, 0) as TotalPagesInVerifiedDocs, "
	"	COALESCE(@TotalDocsRedacted, 0) as TotalDocsRedacted, "
	"	COALESCE(@TotalPagesInRedactedDocs, 0) as TotalPagesInRedactedDocs, "
	"	COALESCE(@TotalDocsVerifiedNotRedacted, 0) as TotalDocsVerifiedNotRedacted, "
	"	COALESCE(@TotalPagesInVerifiedNotRedacted, 0) as TotalPagesInVerifiedNotRedacted";
	
// Text to replace with the Action column name in the Project info query string
//static const string gstrACTION_COLUMN_NAME_REPLACE_VAR = "<ACTION_COLUMN_NAME>";
static const string gstrVERIFIER_QUERY = 
	"SELECT	UserID, "
	"	SUM(TotalPagesInVerifiedDocs) AS TotalPagesInVerifiedDocs, "
	"	SUM(TotalDocsVerified) AS TotalDocsVerified, UserName, "
	"	SUM(Redacted) AS TotalDocsRedacted, "
	"	SUM(RedactedPages) AS TotalPagesInRedactedDocs, "
	"	SUM(TotalRedactions) AS TotalRedactions, "
	"	SUM(TotalManualRedactions) AS TotalManualRedactions, "
	"	SUM(DATEDIFF(d, MinTime, MaxTime) * 86400.0 + "
	"			DATEDIFF(ms, DATEADD(d, DATEDIFF(d, MinTime, MaxTime), MinTime), MaxTime)/ 1000.0) AS TotalProjectTime, "
	"	SUM(ScreenTime) AS TotalScreenTime "
	"FROM    (	SELECT  IDShieldData.UserID, "
	"					SUM(FAMFile.Pages) AS TotalPagesInVerifiedDocs, "
	"					COUNT(IDShieldData.FileID) AS TotalDocsVerified, "
    "					FAMUser.UserName, "
	"					SUM(CASE WHEN (IDShieldData.TotalRedactions > 0) THEN 1 ELSE 0 END) AS Redacted, "
    "					SUM(CASE WHEN (IDShieldData.TotalRedactions > 0) THEN FAMFile.Pages ELSE 0 END) AS RedactedPages, "
    "					SUM(IDShieldData.TotalRedactions) AS TotalRedactions, "
	"					SUM(IDShieldData.TotalManualRedactions) AS TotalManualRedactions, "
    "					MAX(IDShieldData.DateTimeStamp) AS MaxTime, "
	"					MIN(DATEADD( ms, - (Duration - floor(Duration/86400) * 86400) * 1000, "
	"							DATEADD( d, -(floor(Duration/86400)), DateTimeStamp))) AS MinTime, "
	"					SUM(IDShieldData.Duration) AS ScreenTime "
    "			FROM      IDShieldData INNER JOIN "
	"					FAMFile ON IDShieldData.FileID = FAMFile.ID INNER JOIN "
	"					FAMUser ON IDShieldData.UserID = FAMUser.ID "
	"			WHERE   (IDShieldData.IsMostRecentUpdate = 1 AND Verified = 1) "
    "			GROUP BY IDShieldData.UserID, FAMUser.UserName)  AS ByDayAndUser " 
	"GROUP BY UserID, UserName";
