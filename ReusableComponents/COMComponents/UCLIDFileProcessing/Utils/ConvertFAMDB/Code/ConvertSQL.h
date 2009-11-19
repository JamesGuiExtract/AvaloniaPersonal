// ConvertSQL.h : Header file for the SQL Queries needed to convert the database
//

#pragma once

#include <string>

using namespace std;

// Query to select the FAST records in the 6.0 or 7.0 db to be transfered - this has the ASCName for the action
// instead of the ID since the new database may have a different id for that action name
static const string gstrSELECT_FAST_RECORDS_FOR_TRANSFER_FROM_7_0 = 
	"SELECT     FileActionStateTransition.ID, FileActionStateTransition.FileID, "
	"			FileActionStateTransition.ASC_From, FileActionStateTransition.ASC_To, "
	"           FileActionStateTransition.DateTimeStamp, FileActionStateTransition.MachineID, "
	"			FileActionStateTransition.FamUserID, FileActionStateTransition.Exception, "
	"			FileActionStateTransition.Comment, Action.ASCName "
	"FROM       FileActionStateTransition INNER JOIN "
	"				Action ON FileActionStateTransition.ActionID = Action.ID";

// Query to select the ActionStatistics records in 6.0 or 7.0 db to be transfered. The ActionID is 
// Converted to the ASCName so it can be looked up in the new database
static const string gstrSELECT_ACTIONSTATISTICS_FOR_TRANSFER_FROM_7_0 = 
	"SELECT [ActionID]"
	"	  ,[NumDocuments]"
	"	  ,[NumDocumentsComplete]"
	"	  ,[NumDocumentsFailed]"
	"	  , 0 as [NumDocumentsSkipped]"
	"	  ,[NumPages]"
	"	  ,[NumPagesComplete]"
	"	  ,[NumPagesFailed]"
	"	  , 0 as [NumPagesSkipped]"
	"	  ,[NumBytes]"
	"	  ,[NumBytesComplete]"
	"	  ,[NumBytesFailed]"
	"	  , 0 as [NumBytesSkipped]"
	" FROM       ActionStatistics ";

static const string gstrSELECT_IDSHIELD_DATA_FOR_TRANSFER_FROM_7_0 = 
	"SELECT [FileID]"
	"	  ,[Verified]"
	"	  ,[UserID]"
	"	  ,[MachineID]"
	"	  ,[DateTimeStamp]"
	"	  ,[Duration]"
	"	  ,0 AS [TotalDuration]"
	"	  ,[NumHCDataFound]"
	"	  ,[NumMCDataFound]"
	"	  ,[NumLCDataFound]"
	"	  ,[NumCluesFound]"
	"	  ,[TotalManualRedactions]"
	"	  ,[TotalRedactions]"
	" FROM [IDShieldData]";

// Query to set the value in the Total duration field by adding
static const string gstrUPDATE_IDSHIELD_DATA_DURATION_FOR_8_0 = 
	"UPDATE IDShieldData "
	"SET IDShieldData.TotalDuration = CalculatedTotalDuration "
	"FROM ("
	"		SELECT ID, "
	"				( "
	"					SELECT SUM(Duration) FROM "
	"					( "
	"						SELECT FileID "
	"							, RANK() OVER (PARTITION BY FileID, Verified ORDER BY Verified, DateTimeStamp) AS TheRank "
	"							, Duration "
	"							, Verified "
	"							, DateTimeStamp "
	"							FROM IDShieldData " 
	"					) AS T1 "
	"					WHERE T1.TheRank <= T2.TheRank AND T1.FileID = T2.FileId AND T1.Verified = T2.Verified "
	"				) AS CalculatedTotalDuration "
	"		FROM ( "
	"				SELECT ID, FileID "
	"					, RANK() OVER (PARTITION BY FileID, Verified ORDER BY Verified, DateTimeStamp) AS TheRank "
	"					, Duration "
	"					, Verified "
	"					, DateTimeStamp "
	"				FROM IDShieldData "
	"		) as T2 "
	"	) AS T3 INNER JOIN IDShieldData ON T3.id = IDShieldData.ID ";
				
 