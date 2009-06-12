// ConvertSQL.h : Header file for the SQL Queries needed to convert the database
//

#pragma once

#include <string>

using namespace std;

// Query to select the FAST records in the 5.0 db to be transfered - this has the ASCName for the action
// instead of the ID since the new database may have a different id for that action name
static const string gstrSELECT_FAST_RECORDS_FOR_TRANSFER_FROM_5_0 = 
	"SELECT     FileActionStateTransition.ID, FileActionStateTransition.FileID, "
	"			FileActionStateTransition.ASC_From, FileActionStateTransition.ASC_To, "
	"           FileActionStateTransition.TS_Transition, FileActionStateTransition.MachineName, "
	"			FileActionStateTransition.UserName, FileActionStateTransition.Exception, "
	"			FileActionStateTransition.Comment, Action.ASCName "
	"FROM       FileActionStateTransition INNER JOIN "
	"				Action ON FileActionStateTransition.ActionID = Action.ID";

// Query to select the ActionStatistics records in 5.0 db to be transfered. The ActionID is 
// Converted to the ASCName so it can be looked up in the new database
static const string gstrSELECT_ACTIONSTATISTICS_FOR_TRANSFER_FROM_5_0 = 
	"SELECT     ActionStatistics.NumDocuments, ActionStatistics.NumDocumentsComplete, "
	"			ActionStatistics.NumDocumentsFailed, ActionStatistics.NumPages, "
	"           ActionStatistics.NumPagesComplete, ActionStatistics.NumPagesFailed, "
	"			ActionStatistics.NumBytes, ActionStatistics.NumBytesComplete, "
	"           ActionStatistics.NumBytesFailed, Action.ASCName "
	"FROM       ActionStatistics INNER JOIN "
    "                  Action ON ActionStatistics.ActionID = Action.ID";