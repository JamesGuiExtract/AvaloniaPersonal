// FileProcessingDB.cpp : Implementation internal COM Methods of CFileProcessingDB

#include "stdafx.h"
#include "FileProcessingDB.h"
#include "FAMDB_SQL.h"
#include "FAMDB_SQL_Legacy.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <LicenseMgmt.h>
#include <MiscLeadUtils.h>
#include <UPI.h>
#include <LoginDlg.h>
#include <ByteStreamManipulator.h>
#include <PasswordDlg.h>
#include <ComponentLicenseIDs.h>
#include <FAMUtilsConstants.h>
#include <ChangePasswordDlg.h>
#include <ADOUtils.h>
#include <StopWatch.h>
#include <stringCSIS.h>
#include <EncryptionEngine.h>
#include <StringTokenizer.h>
#include <ValueRestorer.h>
#include <DateUtil.h>
#include <SqlApplicationRole.h>

#include <atlsafe.h>

#include <string>
#include <stack>

using namespace std;
using namespace ADODB;

//-------------------------------------------------------------------------------------------------
// Define constant for the current DB schema version
// This must be updated when the DB schema changes
// !!!ATTENTION!!!
// An UpdateToSchemaVersion method must be added when checking in a new schema version.
// Version 184 First schema that includes all product specific schema regardless of license
//		Also fixes up some missing elements between updating schema and creating
//		All product schemas are also done withing the same transaction.
const long CFileProcessingDB::ms_lFAMDBSchemaVersion = 202;

//-------------------------------------------------------------------------------------------------
// Defined constant for the Request code version
const long glSECURE_COUNTER_REQUEST_VERSION = 2;

// Add item to exception log at this counter decrement frequency
const long gnLOG_FREQUENCY = 1000;

//-------------------------------------------------------------------------------------------------
// Define four UCLID passwords used for encrypting the password
// NOTE: These passwords were not exposed at the header file level because
//		 no user of this class needs to know that these passwords exist
// These passwords are also uses in the FileProcessingDB.cpp
const unsigned long	gulFAMKey1 = 0x78932517;
const unsigned long	gulFAMKey2 = 0x193E2224;
const unsigned long	gulFAMKey3 = 0x20134253;
const unsigned long	gulFAMKey4 = 0x15990323;

// Method defined in FileProcessingDB_Internal
//void addFAMPasswords(ByteStreamManipulator &bsmPassword);
void getFAMPassword(ByteStream& rPasswordBytes);

string buildUpdateSchemaVersionQuery(int nSchemaVersion)
{
	string strQuery = "UPDATE [DBInfo] SET [Value] = '" + asString(nSchemaVersion)
		+ "' WHERE [Name] = '" + gstrFAMDB_SCHEMA_VERSION + "'";

	return strQuery;
}

//-------------------------------------------------------------------------------------------------
// Schema update functions
// 
// NOTE TO IMPLEMENTERS: If pnNumSteps is not null, rather than performing a schema update,
// pnNumSteps should instead be assigned a number of steps corresponding to the number of progress
// steps that should be assigned. Suggested values are:
// 3 = An O(1) operation such as creating a new table.
// 10 = A relatively simple O(n) DB query to run relative to the number of files in the database.
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion101(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 101;

		if (pnNumSteps != __nullptr)
		{
			// This update requires potentially creating a new row in the FileActionStatus table for
			// every row in the FAMFile table and is therefore O(n) relative to the number of files
			// in the DB.
			*pnNumSteps += 10;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		// Drop ProcessingFAM so it can be re-created with the proper columns.
		// No need to transfer data. It will be assumed that all entries are crashed/hung instances.
		vecQueries.push_back("ALTER TABLE [LockedFile] DROP CONSTRAINT [FK_LockedFile_ProcessingFAM]");
		vecQueries.push_back("DROP TABLE [ProcessingFAM]");
		vecQueries.push_back(gstrCREATE_PROCESSING_FAM_TABLE_V101);
		vecQueries.push_back(gstrADD_LOCKED_FILE_PROCESSINGFAM_FK_V101);

		// Create the FileActionStatus table and associated indexes/constraints.
		vecQueries.push_back(gstrCREATE_FILE_ACTION_STATUS_LEGACY);
		vecQueries.push_back(gstrCREATE_FILE_ACTION_STATUS_ACTION_ACTIONSTATUS_INDEX_V101);
		vecQueries.push_back(gstrADD_ACTION_PROCESSINGFAM_FK_V101);
		vecQueries.push_back(gstrADD_FILE_ACTION_STATUS_ACTION_FK);
		vecQueries.push_back(gstrADD_FILE_ACTION_STATUS_FAMFILE_FK);
		vecQueries.push_back(gstrADD_FILE_ACTION_STATUS_ACTION_STATUS_FK);
	
		// Add query to transfer the data from the old FAMFile.ASC columns into the new FileActionStatus
		// table, then drop the FAMFile.ASC columns.
		vecQueries.push_back(
			"DECLARE @dynamic_command NVARCHAR(MAX)\r\n"
			"DECLARE @action_name NVARCHAR(50)\r\n"
			"DECLARE @action_id NVARCHAR(8)\r\n"
			"DECLARE action_cursor CURSOR FOR SELECT [ASCName], [ID] FROM [Action]\r\n"

			"OPEN action_cursor\r\n"

			"BEGIN TRY\r\n"
			"	FETCH NEXT FROM action_cursor INTO @action_name, @action_id\r\n"

			"	WHILE @@FETCH_STATUS = 0\r\n"
			"	BEGIN\r\n"
			"		SET @dynamic_command = 'INSERT INTO [FileActionStatus]\r\n"
			"				([ActionID], [FileID], [ActionStatus])\r\n"
			"			SELECT ' + @action_id + ', [ID], [ASC_' + @action_name + '] FROM [FAMFile]\r\n"
			"				WHERE ASC_' + @action_name + ' != ''U'''\r\n"
			"		EXEC (@dynamic_command)\r\n"

			"		SET @dynamic_command =\r\n"
			"			'ALTER TABLE [FAMFile] DROP CONSTRAINT FK_ASC_' + @action_name\r\n"
			"		EXEC (@dynamic_command)\r\n"

			"		SET @dynamic_command =\r\n"
			"			'DROP INDEX IX_ASC_' + @action_name + ' ON [FAMFile]'\r\n"
			"		EXEC (@dynamic_command)\r\n"

			"		SET @dynamic_command =\r\n"
			"			'ALTER TABLE [FAMFile] DROP CONSTRAINT DF_ASC_' + @action_name\r\n"
			"		EXEC (@dynamic_command)\r\n"

			"		SET @dynamic_command =\r\n"
			"			'ALTER TABLE [FAMFile] DROP COLUMN ASC_' + @action_name\r\n"
			"		EXEC (@dynamic_command)\r\n"

			"		FETCH NEXT FROM action_cursor INTO @action_name, @action_id\r\n"
			"	END\r\n"

			"	CLOSE action_cursor\r\n"
			"	DEALLOCATE action_cursor\r\n"
			"END TRY\r\n"
			"BEGIN CATCH\r\n"
			"	CLOSE action_cursor\r\n"
			"	DEALLOCATE action_cursor\r\n"
	
			"	DECLARE @error_message NVARCHAR(MAX)\r\n"
			"	DECLARE @error_severity INT\r\n"
			"	DECLARE @error_state INT\r\n"
	
			"	SELECT\r\n"
			"		@error_message = ERROR_MESSAGE(),\r\n"
			"		@error_severity = ERROR_SEVERITY(),\r\n"
			"		@error_state = ERROR_STATE()\r\n"
			
			"	IF @error_state = 0\r\n"
			"		SELECT @error_state = 1\r\n"

			"	RAISERROR (@error_message, @error_severity, @error_state)\r\n"
			"END CATCH\r\n");

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI31437");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion102(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 102;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 3;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		// Drop ActionStatistics table so it can be re-created with the proper columns.
		// No need to transfer data; instead, regenerate the stats afterward.
		vecQueries.push_back("DROP Table [ActionStatistics]");
		vecQueries.push_back(gstrCREATE_ACTION_STATISTICS_TABLE_102);

		// https://extract.atlassian.net/browse/ISSUE-12916
		// Was added in 10.1 should have been added at the time the table was changed.
		vecQueries.push_back(gstrADD_STATISTICS_ACTION_FK);

		// Add new ActionStatisticsDelta table.
		vecQueries.push_back(gstrCREATE_ACTION_STATISTICS_DELTA_TABLE_102);
		vecQueries.push_back(gstrCREATE_ACTION_STATISTICS_DELTA_ACTIONID_ID_INDEX);
		vecQueries.push_back(gstrADD_ACTION_STATISTICS_DELTA_ACTION_FK);

		// Add default value for ActionStatisticsUpdateFreqInSeconds.
		vecQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('"
				+ gstrACTION_STATISTICS_UPDATE_FREQ_IN_SECONDS + "', '300')");

		// Regenerate the action statistics for all actions (empty "where" clause)
		string strCreateActionStatsSQL = gstrRECREATE_ACTION_STATISTICS_FOR_ACTION;
		replaceVariable(strCreateActionStatsSQL, "<ActionIDWhereClause>", "");
		vecQueries.push_back(strCreateActionStatsSQL);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI31438");
}
//-------------------------------------------------------------------------------------------------
int  UpdateToSchemaVersion103(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 103;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 3;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		// Add StoreSourceDocChangeHistory table
		vecQueries.push_back(gstrCREATE_SOURCE_DOC_CHANGE_HISTORY);
		vecQueries.push_back(gstrADD_SOURCE_DOC_CHANGE_HISTORY_FAMFILE_FK);
		vecQueries.push_back(gstrADD_SOURCE_DOC_CHANGE_HISTORY_FAMUSER_FK);
		vecQueries.push_back(gstrADD_SOURCE_DOC_CHANGE_HISTORY_MACHINE_FK);
		
		// Add default value for StoreSourceDocChangeHistory.
		vecQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('"
				+ gstrSTORE_SOURCE_DOC_NAME_CHANGE_HISTORY + "', '1')");

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI31527");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion104(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 104;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 3;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		// Add DocTagHistory table
		vecQueries.push_back(gstrCREATE_DOC_TAG_HISTORY_TABLE);
		vecQueries.push_back(gstrADD_DOC_TAG_HISTORY_FAMFILE_FK);
		vecQueries.push_back(gstrADD_DOC_TAG_HISTORY_TAG_FK);
		vecQueries.push_back(gstrADD_DOC_TAG_HISTORY_FAMUSER_FK);
		vecQueries.push_back(gstrADD_DOC_TAG_HISTORY_MACHINE_FK);
		
		// Add default value for StoreDocTagHistory.
		vecQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('"
				+ gstrSTORE_DOC_TAG_HISTORY + "', '1')");

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI31987");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion105(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		const int nNewSchemaVersion = 105;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 3;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		// Add DocTagHistory table
		vecQueries.push_back("ALTER TABLE [DBInfo] ADD ID INT IDENTITY(1,1) NOT NULL");
		vecQueries.push_back(gstrCREATE_DB_INFO_ID_INDEX);
		vecQueries.push_back(gstrCREATE_DB_INFO_CHANGE_HISTORY_TABLE);
		vecQueries.push_back(gstrADD_DB_INFO_HISTORY_FAMUSER_FK);
		vecQueries.push_back(gstrADD_DB_INFO_HISTORY_MACHINE_FK);
		vecQueries.push_back(gstrADD_DB_INFO_HISTORY_DB_INFO_FK);
		
		// Add default value for StoreDBInfoHistory.
		vecQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('"
				+ gstrSTORE_DB_INFO_HISTORY + "', '1')");

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		// Add default value for last DB info change
		vecQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('"
				+ gstrLAST_DB_INFO_CHANGE + "', '" + getSQLServerDateTime(ipConnection) + "')");

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32167");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion106(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		const int nNewSchemaVersion = 106;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 3;
			return nNewSchemaVersion;
		}

		// Drop the LockTable and recreate it with the new schema version
		vector<string> vecQueries;
		vecQueries.push_back("DROP TABLE [LockTable]");
		vecQueries.push_back(gstrCREATE_LOCK_TABLE);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		// Execute the queries
		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32300");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion107(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		const int nNewSchemaVersion = 107;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 3;
			return nNewSchemaVersion;
		}

		// Update the name of the DBInfo setting for SkipAuthenticationOnMachines
		vector<string> vecQueries;
		vecQueries.push_back("UPDATE [DBInfo] SET [Name] = '"
			+ gstrSKIP_AUTHENTICATION_ON_MACHINES
			+ "' WHERE [Name] = 'SkipAuthenticationOnMachines'");

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		// Execute the queries
		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32304");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion108(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		const int nNewSchemaVersion = 108;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 3;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		// Fix the spelling error for the user created counter
		vecQueries.push_back("EXEC sp_rename 'dbo.UserCreatedCounter.PK_UserCreatedConter', "
			"'PK_UserCreatedCounter', 'INDEX'");

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		// Execute the queries
		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32470");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion109(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		const int nNewSchemaVersion = 109;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 3;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		// Add GetFilesToProcessTransactionTimeout setting default if it is not already there
		// [LRCAU #6105] - Add the get files to process transaction timeout if it does not exist
		vecQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) SELECT '"
			+ gstrGET_FILES_TO_PROCESS_TRANSACTION_TIMEOUT + 
			"' AS [Name], '" + asString(gdMINIMUM_TRANSACTION_TIMEOUT, 0)
			+ "' AS [Value] WHERE NOT EXISTS (SELECT [Value] FROM [DBInfo] WHERE [Name] = '"
			+ gstrGET_FILES_TO_PROCESS_TRANSACTION_TIMEOUT + "')");

		// Add the default values for the min and max sleep time between db checks
		vecQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('"
				+ gstrMIN_SLEEP_BETWEEN_DB_CHECKS + "', '"
				+ asString(gnDEFAULT_MIN_SLEEP_TIME_BETWEEN_DB_CHECK) + "')");
		vecQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('"
				+ gstrMAX_SLEEP_BETWEEN_DB_CHECKS + "', '"
				+ asString(gnDEFAULT_MAX_SLEEP_TIME_BETWEEN_DB_CHECK) + "')");

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		// Execute the queries
		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32569");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion110(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 110;

		if (pnNumSteps != __nullptr)
		{
			// This update does not require transferring any data.
			*pnNumSteps += 3;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		// Drop ProcessingFAM so it can be re-created with the proper columns.
		// No need to transfer data. It will be assumed that all entries are crashed/hung instances.
		vecQueries.push_back("ALTER TABLE [LockedFile] DROP CONSTRAINT [FK_LockedFile_ProcessingFAM]");
		vecQueries.push_back("ALTER TABLE [ProcessingFAM] DROP CONSTRAINT [FK_ProcessingFAM_Action]");
		vecQueries.push_back("DROP TABLE [ProcessingFAM]");
		vecQueries.push_back(gstrCREATE_ACTIVE_FAM_TABLE_V110);
		vecQueries.push_back(gstrCREATE_ACTIVE_FAM_UPI_INDEX_V110);
		vecQueries.push_back(gstrADD_LOCKED_FILE_ACTIVEFAM_FK_V110);
		vecQueries.push_back(gstrADD_ACTION_ACTIVEFAM_FK_V110);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33184");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion111(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 111;

		if (pnNumSteps != __nullptr)
		{
			// This update does not require transferring any data.
			*pnNumSteps += 3;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		// Add FTPAccount table
		vecQueries.push_back(gstrCREATE_FTP_ACCOUNT);

		// Add FTPEventHistory table
		vecQueries.push_back(gstrCREATE_FTP_EVENT_HISTORY_TABLE);
		vecQueries.push_back(gstrADD_FTP_EVENT_HISTORY_FTP_ACCOUNT_FK);
		vecQueries.push_back(gstrADD_FTP_EVENT_HISTORY_FAM_FILE_FK);
		vecQueries.push_back(gstrADD_FTP_EVENT_HISTORY_ACTION_FK);
		vecQueries.push_back(gstrADD_FTP_EVENT_HISTORY_MACHINE_FK);
		vecQueries.push_back(gstrADD_FTP_EVENT_HISTORY_FAM_USER_FK);

		// Add default value for StoreFTPEventHistory.
		vecQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('"
			+ gstrSTORE_FTP_EVENT_HISTORY + "', '1')");

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33957");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion112(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 112;

		if (pnNumSteps != __nullptr)
		{
			// This update requires potentially creating a new row in the FileActionStatus table for
			// every row in the FAMFile table and is therefore O(n) relative to the number of files
			// in the DB.
			*pnNumSteps += 10;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back("EXEC sp_rename 'dbo.FileActionStatus', 'FileActionStatus_Old'");
		vecQueries.push_back("ALTER TABLE [FileActionStatus_Old] DROP CONSTRAINT [PK_FileActionStatus]");
		vecQueries.push_back(gstrCREATE_FILE_ACTION_STATUS_112_187);
		vecQueries.push_back("INSERT INTO [FileActionStatus] "
			"([ActionID], [FileID], [ActionStatus], [Priority]) "
			"	SELECT [ActionID], [FileID], [ActionStatus], [FAMFile].[Priority] "
			"		FROM [FileActionStatus_Old] "
			"		INNER JOIN [FAMFile] ON [FileActionStatus_Old].[FileID] = [FAMFile].[ID]");
		vecQueries.push_back("DROP TABLE [FileActionStatus_Old]");
		vecQueries.push_back("DROP INDEX [IX_Files_PriorityID] ON [FAMFile]");
		vecQueries.push_back(gstrCREATE_FILE_ACTION_STATUS_ALL_INDEX);
		vecQueries.push_back(gstrADD_FILE_ACTION_STATUS_ACTION_FK);
		vecQueries.push_back(gstrADD_FILE_ACTION_STATUS_FAMFILE_FK);
		vecQueries.push_back(gstrADD_FILE_ACTION_STATUS_ACTION_STATUS_FK);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI34146");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion113(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 113;

		if (pnNumSteps != __nullptr)
		{
			// This update does not require transferring any data.
			*pnNumSteps += 3;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(gstrCREATE_QUEUED_ACTION_STATUS_CHANGE_TABLE_V113);
		vecQueries.push_back(gstrCREATE_QUEUED_ACTION_STATUS_CHANGE_INDEX);
		vecQueries.push_back(gstrADD_QUEUED_ACTION_STATUS_CHANGE_FAMFILE_FK);
		vecQueries.push_back(gstrADD_QUEUED_ACTION_STATUS_CHANGE_ACTION_FK);
		vecQueries.push_back(gstrADD_QUEUED_ACTION_STATUS_CHANGE_MACHINE_FK);
		vecQueries.push_back(gstrADD_QUEUED_ACTION_STATUS_CHANGE_USER_FK);
		vecQueries.push_back("ALTER TABLE [FileActionStateTransition] ADD [QueueID] INT NULL");
		vecQueries.push_back(gstrADD_FILE_ACTION_STATE_TRANSITION_QUEUE_FK);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI34169");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion114(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 114;

		if (pnNumSteps != __nullptr)
		{
			// This update does not require transferring any data.
			*pnNumSteps += 3;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(gstrCREATE_FIELD_SEARCH_TABLE);
		vecQueries.push_back(gstrCREATE_LAUNCH_APP_TABLE_V114);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI35773");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion115(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 115;

		if (pnNumSteps != __nullptr)
		{
			// This update does not require transferring any data.
			*pnNumSteps += 3;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;
		// Add AlternateComponentDataDir DBInfo value.
		vecQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('"
			+ gstrALTERNATE_COMPONENT_DATA_DIR + "', '')");
		// Add Email settings values.
		// Email setting defaults should be kept in sync with Extract.Utilities.Email.ExtractSmtp
		vecQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('"
			+ gstrEMAIL_SERVER + "', '')");
		vecQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('"
			+ gstrEMAIL_PORT + "', '25')");
		vecQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('"
			+ gstrEMAIL_SENDER_NAME + "', '')");
		vecQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('"
			+ gstrEMAIL_SENDER_ADDRESS + "', '')");
		vecQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('"
			+ gstrEMAIL_SIGNATURE + "', '')");
		vecQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('"
			+ gstrEMAIL_USERNAME + "', '')");
		vecQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('"
			+ gstrEMAIL_PASSWORD + "', '')");
		vecQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('"
			+ gstrEMAIL_TIMEOUT + "', '0')");
		vecQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('"
			+ gstrEMAIL_USE_SSL + "', '0')");

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI35919");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion116(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 116;

		if (pnNumSteps != __nullptr)
		{
			// This update does not require transferring any data.
			*pnNumSteps += 3;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;
		vecQueries.push_back("EXEC sp_rename '" + gstrDB_LAUNCH_APP + "', '" + gstrDB_FILE_HANDLER + "'");
		vecQueries.push_back(gstrCREATE_FEATURE_TABLE);
		vector<string> vecFeatureDefinitionQueries = getFeatureDefinitionQueries(116);
		vecQueries.insert(vecQueries.end(),
			vecFeatureDefinitionQueries.begin(), vecFeatureDefinitionQueries.end());
		
		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);
		
		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36075");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion117(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 117;

		if (pnNumSteps != __nullptr)
		{
			// This update does not require transferring any data.
			*pnNumSteps += 3;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(
			string("INSERT INTO [Feature] "
			"([Enabled], [FeatureName], [FeatureDescription], [AdminOnly]) "
			"VALUES(1, '") + gstrFEATURE_FILE_HANDLER_OPEN_FILE_LOCATION.c_str() + "', "
			"'Allows the containing folder of document to be opened in Windows file explorer.', 1)");
		
		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);
		
		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37136");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion118(_ConnectionPtr ipConnection, long *pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	int nNewSchemaVersion = 118;

	if (pnNumSteps != __nullptr)
	{
		*pnNumSteps += 3;
		return nNewSchemaVersion;
	}

	vector<string> vecQueries;
	vecQueries.push_back(gstrCREATE_WORK_ITEM_GROUP_TABLE_V118);
	vecQueries.push_back(gstrCREATE_WORK_ITEM_TABLE_V118);
	vecQueries.push_back(gstrADD_WORK_ITEM_GROUP_ACTION_FK);
	vecQueries.push_back(gstrADD_WORK_ITEM_GROUP_FAMFILE_FK);
	vecQueries.push_back(gstrADD_WORK_ITEM__WORK_ITEM_GROUP_FK);
	vecQueries.push_back(gstrCREATE_WORK_ITEM_GROUP_UPI_INDEX_V118);
	vecQueries.push_back(gstrCREATE_WORK_ITEM_STATUS_INDEX);
	vecQueries.push_back(gstrCREATE_WORK_ITEM_ID_STATUS_INDEX);
	vecQueries.push_back(gstrCREATE_WORK_ITEM_UPI_INDEX_V118);

	vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

	// Add default value for AllowRestartableProcessing.
	vecQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('"
		+ gstrALLOW_RESTARTABLE_PROCESSING + "', '0')");

	executeVectorOfSQL(ipConnection, vecQueries);

	return nNewSchemaVersion;
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion119(_ConnectionPtr ipConnection, long *pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	int nNewSchemaVersion = 119;

	if (pnNumSteps != __nullptr)
	{
		*pnNumSteps += 3;
		return nNewSchemaVersion;
	}

	vector<string> vecQueries;
	vecQueries.push_back(
		"ALTER TABLE WorkItem ADD [BinaryInput] varbinary(max) NULL, [BinaryOutput] varbinary(max) NULL");
	vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

	executeVectorOfSQL(ipConnection, vecQueries);

	return nNewSchemaVersion;
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion120(_ConnectionPtr ipConnection, long *pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	int nNewSchemaVersion = 120;

	if (pnNumSteps != __nullptr)
	{
		*pnNumSteps += 3;
		return nNewSchemaVersion;
	}

	vector<string> vecQueries;
	vecQueries.push_back(gstrCREATE_METADATA_FIELD_TABLE);
	vecQueries.push_back(gstrCREATE_FILE_METADATA_FIELD_VALUE_TABLE);
	vecQueries.push_back(gstrADD_METADATA_FIELD_VALUE_FAMFILE_FK);
	vecQueries.push_back(gstrADD_METADATA_FIELD_VALUE_METADATA_FIELD_FK);
	vecQueries.push_back(gstrMETADATA_FIELD_VALUE_INDEX);
	vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

	executeVectorOfSQL(ipConnection, vecQueries);

	return nNewSchemaVersion;
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion121(_ConnectionPtr ipConnection, long *pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	int nNewSchemaVersion = 121;

	if (pnNumSteps != __nullptr)
	{
		*pnNumSteps += 3;
		return nNewSchemaVersion;
	}

	vector<string> vecQueries;
	vecQueries.push_back(gstrCREATE_FAST_ACTIONID_INDEX);
	vecQueries.push_back(gstrCREATE_FAST_FILEID_ACTIONID_INDEX);
	vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

	executeVectorOfSQL(ipConnection, vecQueries);

	return nNewSchemaVersion;
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion122(_ConnectionPtr ipConnection, long *pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	int nNewSchemaVersion = 122;

	if (pnNumSteps != __nullptr)
	{
		// This is such a small tweak-- use a single step as opposed to the usual 3.
		*pnNumSteps += 1;
		return nNewSchemaVersion;
	}

	// https://extract.atlassian.net/browse/ISSUE-12493
	// This setting was never adjusted to our knowledge; since it has new importance with the
	// addition of parallelized (work unit) processing, change this settings to 5 minutes if it is
	// currently at the previous default of 60 minutes.
	vector<string> vecQueries;
	vecQueries.push_back("UPDATE [DBInfo] SET [Value] = 5 " 
		"WHERE [Name] = '" + gstrAUTO_REVERT_TIME_OUT_IN_MINUTES + "' AND [Value] = 60");
	vecQueries.push_back("DELETE FROM [DBInfo] WHERE [Name] = 'AutoRevertLockedFiles'");
	vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

	executeVectorOfSQL(ipConnection, vecQueries);

	return nNewSchemaVersion;
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion123(_ConnectionPtr ipConnection, long *pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	int nNewSchemaVersion = 123;

	if (pnNumSteps != __nullptr)
	{
		// This is such a small tweak-- use a single step as opposed to the usual 3.
		*pnNumSteps += 1;
		return nNewSchemaVersion;
	}

	vector<string> vecQueries;
	vecQueries.push_back(
		"ALTER TABLE [Action] ADD CONSTRAINT [Action_ASCName_Unique] UNIQUE (ASCName)"); 
	vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

	executeVectorOfSQL(ipConnection, vecQueries);

	return nNewSchemaVersion;
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion124(_ConnectionPtr ipConnection, long *pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	int nNewSchemaVersion = 124;

	if (pnNumSteps != __nullptr)
	{
		*pnNumSteps += 3;
		return nNewSchemaVersion;
	}

	// https://extract.atlassian.net/browse/ISSUE-12763
	// This schema update actually doesn't do anything in terms of the core FAM DB schema-- it
	// exists only to trigger a schema update in order to prompt the user whether they want to add
	// the newly created LabDE schema elements. That prompt occurs within
	// CLabDEProductDBMgr::UpdateSchemaForFAMDBVersion when the current FAM DB schema version is 123.
	vector<string> vecQueries;
	vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

	executeVectorOfSQL(ipConnection, vecQueries);

	return nNewSchemaVersion;
}

//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion125(_ConnectionPtr ipConnection, long *pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	int nNewSchemaVersion = 125;

	if (pnNumSteps != __nullptr)
	{
		// This is such a small tweak-- use a single step as opposed to the usual 3.
		*pnNumSteps += 1;
		return nNewSchemaVersion;
	}

	vector<string> vecQueries;
	vecQueries.push_back("ALTER TABLE dbo.LockTable DROP CONSTRAINT DF_LockTable_LockTime"); 
	vecQueries.push_back(
		"ALTER TABLE dbo.LockTable ADD CONSTRAINT DF_LockTable_LockTime DEFAULT (getutcdate()) FOR LockTime");
	vecQueries.push_back("ALTER TABLE dbo.ActiveFAM DROP CONSTRAINT DF_ActiveFAM_LastPingTime");
	vecQueries.push_back(
		"ALTER TABLE dbo.ActiveFAM ADD CONSTRAINT DF_ActiveFAM_LastPingTime DEFAULT (getutcdate()) FOR LastPingTime");
	vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

	executeVectorOfSQL(ipConnection, vecQueries);

	return nNewSchemaVersion;
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion126(_ConnectionPtr ipConnection, long *pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	int nNewSchemaVersion = 126;

	if (pnNumSteps != __nullptr)
	{
		// This is such a small tweak-- use a single step as opposed to the usual 3.
		*pnNumSteps += 1;
		return nNewSchemaVersion;
	}

	vector<string> vecQueries;
	vecQueries.push_back("ALTER TABLE dbo.WorkItemGroup ADD RunningTaskDescription nvarchar(256) NULL"); 
	vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

	executeVectorOfSQL(ipConnection, vecQueries);

	return nNewSchemaVersion;
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion127(_ConnectionPtr ipConnection, long *pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	int nNewSchemaVersion = 127;

	if (pnNumSteps != __nullptr)
	{
		// This is such a small tweak-- use a single step as opposed to the usual 3.
		*pnNumSteps += 1;
		return nNewSchemaVersion;
	}

	vector<string> vecQueries;
	vecQueries.push_back("ALTER TABLE dbo.FileMetadataFieldValue ALTER COLUMN Value nvarchar(400) NULL"); 
	vecQueries.push_back(gstrMETADATA_FIELD_VALUE_VALUE_INDEX);

	// Before adding the FK if it doesn't exist should make sure there are no orphaned records
	// https://extract.atlassian.net/browse/ISSUE-13301
	string strDeleteOrphanActionStatistics = 
		"DELETE FROM [dbo].[ActionStatistics] WHERE ActionID NOT IN (SELECT ID FROM [dbo].[Action])";
	vecQueries.push_back(strDeleteOrphanActionStatistics);

	// https://extract.atlassian.net/browse/ISSUE-12916
	// This was added so that an upgrade to 10.1 will fix existing problems if a database was previously
	// upgraded and the FK_Statistics_Action was not added
	string strCheckAndAdd_FK = "IF NOT EXISTS ( SELECT  name FROM sys.foreign_keys "
                "WHERE   name = 'FK_Statistics_Action' ) " + gstrADD_STATISTICS_ACTION_FK;
	vecQueries.push_back(strCheckAndAdd_FK);

	vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

	executeVectorOfSQL(ipConnection, vecQueries);

	return nNewSchemaVersion;
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion128(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 128;

		if (pnNumSteps != __nullptr)
		{
			if (doesTableExist(ipConnection, "WorkItem"))
			{
				long nWorkItemRowCount = 0;
				executeCmdQuery(ipConnection,
					"SELECT Count([ID]) AS [ID] FROM [WorkItem]", false, &nWorkItemRowCount);
				if (nWorkItemRowCount > 0)
				{
					throw UCLIDException("ELI38473",
						"Database cannot be upgraded with incomplete WorkItems");
				}
			}

			*pnNumSteps += 3;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back("DROP INDEX [ActiveFAM].[IX_ActiveFAM_UPI]");
		vecQueries.push_back("ALTER TABLE [ActiveFAM] DROP CONSTRAINT [FK_ActiveFAM_Action]");
		vecQueries.push_back("ALTER TABLE [ActiveFAM] DROP COLUMN [ActionID]");
		vecQueries.push_back("ALTER TABLE [ActiveFAM] DROP COLUMN [UPI]");
		vecQueries.push_back("ALTER TABLE [ActiveFAM] DROP COLUMN [Queuing]");	
		vecQueries.push_back("ALTER TABLE [ActiveFAM] DROP COLUMN [Processing]");
		vecQueries.push_back("ALTER TABLE [ActiveFAM] ADD [FAMSessionID] INT NOT NULL");
		vecQueries.push_back(gstrADD_ACTIVEFAM_FAM_SESSION_FK);
		vecQueries.push_back(gstrCREATE_ACTIVE_FAM_SESSION_INDEX);
		vecQueries.push_back("ALTER TABLE [FAMSession] ADD [ActionID] INT");
		vecQueries.push_back("ALTER TABLE [FAMSession] ADD [Queuing] BIT");
		vecQueries.push_back("ALTER TABLE [FAMSession] ADD [Processing] BIT");
		vecQueries.push_back(gstrADD_FAM_SESSION_ACTION_FK_V128);
		vecQueries.push_back("DROP INDEX [SkippedFile].[IX_Skipped_File_UPI]");

		// Need to drop the unnamed default value constraint on SkippedFile.UPIID.
		// Credit to: https://skuppa.wordpress.com/2010/02/11/working-with-default-constraints/
		vecQueries.push_back(
			"DECLARE @defname VARCHAR(100), @cmd VARCHAR(1000) "
			"SET @defname = "
			"( "
			"	SELECT name "
			"	FROM sysobjects so JOIN sysconstraints sc ON so.id = sc.constid "
			"	WHERE object_name(so.parent_obj) = 'SkippedFile' AND so.xtype = 'D' "
			"	AND sc.colid = (SELECT colid FROM syscolumns WHERE id = object_id('dbo.SkippedFile') AND name = 'UPIID') "
			") "
			"SET @cmd = 'ALTER TABLE [SkippedFile] DROP CONSTRAINT ' + @defname "
			"EXEC(@cmd)");

		vecQueries.push_back("ALTER TABLE [SkippedFile] DROP COLUMN [UPIID]");
		vecQueries.push_back("ALTER TABLE [SkippedFile] ADD [FAMSessionID] INT NULL");
		vecQueries.push_back(gstrADD_SKIPPED_FILE_FAM_SESSION_FK);
		vecQueries.push_back(gstrCREATE_SKIPPED_FILE_FAM_SESSION_INDEX_128_187);
		vecQueries.push_back("ALTER TABLE [QueuedActionStatusChange] DROP COLUMN [UPI]");
		vecQueries.push_back("ALTER TABLE [QueuedActionStatusChange] ADD [FAMSessionID] INT NULL");
		vecQueries.push_back(gstrADD_QUEUED_ACTION_STATUS_CHANGE_FAM_SESSION_FK);
		vecQueries.push_back("DROP INDEX [WorkItemGroup].[IX_WorkItemGroupUPI]");
		vecQueries.push_back("ALTER TABLE [WorkItemGroup] DROP COLUMN [UPI]");
		vecQueries.push_back("ALTER TABLE [WorkItemGroup] ADD [FAMSessionID] INT");
		vecQueries.push_back(gstrADD_WORK_ITEM_GROUP_FAM_SESSION_FK);
		vecQueries.push_back(gstrCREATE_WORK_ITEM_GROUP_FAM_SESSION_INDEX);
		vecQueries.push_back("DROP INDEX [WorkItem].[IX_WorkItemUPI]");
		vecQueries.push_back("ALTER TABLE [WorkItem] DROP COLUMN [UPI]");
		vecQueries.push_back("ALTER TABLE [WorkItem] ADD [FAMSessionID] INT");
		vecQueries.push_back(gstrADD_WORK_ITEM_FAM_SESSION_FK);
		vecQueries.push_back(gstrCREATE_WORK_ITEM_FAM_SESSION_INDEX);
		vecQueries.push_back("ALTER TABLE [LockedFile] DROP CONSTRAINT [PK_LockedFile]");
		vecQueries.push_back("ALTER TABLE [LockedFile] DROP CONSTRAINT [FK_LockedFile_ActiveFAM]");
		vecQueries.push_back("ALTER TABLE [LockedFile] DROP COLUMN [UPIID]");
		vecQueries.push_back("ALTER TABLE [LockedFile] ADD [ActiveFAMID] INT NOT NULL");
		vecQueries.push_back("ALTER TABLE [LockedFile] ADD CONSTRAINT [PK_LockedFile] PRIMARY KEY CLUSTERED ([FileID], [ActionID], [ActiveFAMID])");
		vecQueries.push_back(gstrADD_LOCKED_FILE_ACTIVEFAM_FK);
		vecQueries.push_back("DELETE FROM [DBInfo] WHERE [Name] = 'StoreFAMSessionHistory'");

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI40323");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion129(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 129;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 3;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(gstrCREATE_TASK_CLASS);
		vecQueries.push_back(gstrCREATE_FILE_TASK_SESSION_V129);
		vecQueries.push_back(gstrCREATE_FILE_TASK_SESSION_DATETIMESTAMP_INDEX);
		vecQueries.push_back(gstrCREATE_FILE_TASK_SESSION_FAMSESSION_INDEX);
		vecQueries.push_back(gstrADD_FILE_TASK_SESSION_FAM_SESSION_FK);
		vecQueries.push_back(gstrADD_FILE_TASK_SESSION_TASK_CLASS_FK);
		vecQueries.push_back(gstrADD_FILE_TASK_SESSION_FAMFILE_FK);
		vecQueries.push_back("ALTER TABLE [FAMSession] ALTER COLUMN [FPSFileID] INT NULL");

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38602");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion130(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 130;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 3;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;
	

		vecQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('"
			+ gstrDATABASEID + "', '')");

		vecQueries.push_back(gstrCREATE_SECURE_COUNTER_V130);
		vecQueries.push_back(gstrCREATE_SECURE_COUNTER_VALUE_CHANGE);
		vecQueries.push_back(gstrADD_SECURE_COUNTER_VALUE_CHANGE_SECURE_COUNTER_FK);
		vecQueries.push_back(gstrADD_SECURE_COUNTER_VALUE_CHANGE_FAM_SESSION_FK);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38716");
}

//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion131(_ConnectionPtr ipConnection, 
							 long* pnNumSteps, 
							 IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 131;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 3;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(gstrINSERT_TASKCLASS_STORE_RETRIEVE_ATTRIBUTES);
		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI40358");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion132(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 132;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 3;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		// Drop the SecureCounterValueChange table an SecureCounter table because of issues
		vecQueries.push_back("DELETE FROM DBInfo WHERE [NAME] = '" + gstrDATABASEID + "'");
		vecQueries.push_back("DROP TABLE [dbo].[SecureCounterValueChange]");
		vecQueries.push_back("DROP TABLE [dbo].[SecureCounter]");

		// Create a new DatabaseID and encrypt it
		ByteStream bsDatabaseID;
		createDatabaseID(ipConnection, bsDatabaseID);

		ByteStream bsPW;
		getFAMPassword(bsPW);
		string strDBValue = MapLabel::setMapLabelWithS(bsDatabaseID,bsPW);
		
		vecQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('"
			+ gstrDATABASEID + "', '" + strDBValue + "')");

		vecQueries.push_back(gstrCREATE_SECURE_COUNTER_V130);
		vecQueries.push_back(gstrCREATE_SECURE_COUNTER_VALUE_CHANGE);
		vecQueries.push_back(gstrADD_SECURE_COUNTER_VALUE_CHANGE_SECURE_COUNTER_FK);
		vecQueries.push_back(gstrADD_SECURE_COUNTER_VALUE_CHANGE_FAM_SESSION_FK);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI40359");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion133(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 133;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		// Add license contact settings (for secure counter management screens)
		vecQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('"
			+ gstrSEND_ALERTS_TO_EXTRACT + "', '0')");
		vecQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('"
			+ gstrSEND_ALERTS_TO_SPECIFIED + "', '0')");
		vecQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('"
			+ gstrSPECIFIED_ALERT_RECIPIENTS + "', '')");
		vecQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('"
			+ gstrLICENSE_CONTACT_ORGANIZATION + "', '')");
		vecQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('"
			+ gstrLICENSE_CONTACT_EMAIL + "', '')");
		vecQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('"
			+ gstrLICENSE_CONTACT_PHONE + "', '')");
		
		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI39084");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion134(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 134;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back("ALTER TABLE [SecureCounter] ADD "
			"[AlertLevel] int NOT NULL CONSTRAINT [DF_SecureCounter_AlertLevel] DEFAULT(0)");
		vecQueries.push_back("ALTER TABLE [SecureCounter] ADD "
			"[AlertMultiple] int NOT NULL CONSTRAINT [DF_SecureCounter_AlertMultiple] DEFAULT(0)");

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI39123");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion135(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 135;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(gstrDROP_FAM_SESSION_ACTION_FK);
		vecQueries.push_back(gstrADD_FAM_SESSION_ACTION_FK);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI39184");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion136(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 136;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<_CommandPtr> vecCmds;
		map<string, variant_t> params;

		vecCmds.push_back(buildCmd(ipConnection, gstrINSERT_EMAIL_ENABLE_SETTINGS_WITH_VALUE, params));

		auto cmd = buildCmd(ipConnection, gstADD_UPDATE_DBINFO_SETTING,
			{
				{gstrSETTING_NAME.c_str(), "EmailPossibleInvalidServer"}
				,{gstrSETTING_VALUE.c_str(), "0"}
				,{"@UserID", 0}
				,{"@MachineID", 0}
				,{gstrSAVE_HISTORY.c_str(), 0 }
			});
		vecCmds.push_back(cmd);

		cmd = buildCmd(ipConnection, gstADD_UPDATE_DBINFO_SETTING,
			{
				{gstrSETTING_NAME.c_str(), "EmailPossibleInvalidSenderAddress"}
				,{gstrSETTING_VALUE.c_str(), "0"}
				,{"@UserID", 0}
				,{"@MachineID", 0}
				,{gstrSAVE_HISTORY.c_str(), 0 }
			});
		vecCmds.push_back(cmd);

		vecCmds.push_back(buildCmd(ipConnection, gstrUPDATE_SCHEMA_VERSION_QUERY, { {"@SchemaVersion", asString(nNewSchemaVersion).c_str()} }));

		executeVectorOfCmd(vecCmds);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI39237");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion137(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 137;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back("INSERT INTO [QueueEventCode] ([Code], [Description]) "
			"VALUES('P', 'File was programmatically added without being queued')");

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI39585");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion138(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 138;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 3;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(gstrCREATE_PAGINATION_LEGACY);
		vecQueries.push_back(gstrCREATE_PAGINATION_ORIGINALFILE_INDEX);
		vecQueries.push_back(gstrCREATE_PAGINATION_DESTFILE_INDEX_LEGACY);
		vecQueries.push_back(gstrCREATE_PAGINATION_FILETASKSESSION_INDEX);
		vecQueries.push_back(gstrADD_PAGINATION_SOURCEFILE_FAMFILE_FK);
		vecQueries.push_back(gstrADD_PAGINATION_DESTFILE_FAMFILE_FK);
		vecQueries.push_back(gstrADD_PAGINATION_ORIGINALFILE_FAMFILE_FK);
		vecQueries.push_back(gstrADD_PAGINATION_FILETASKSESSION_FK);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI39680");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion139(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 139;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 3;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(gstrADD_DB_PROCEXECUTOR_ROLE);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI40042");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion140(_ConnectionPtr ipConnection, 
							 long* pnNumSteps, 
							 IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 140;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(gstrINSERT_PAGINATION_TASK_CLASS);
		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI40057");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion141(_ConnectionPtr ipConnection, 
							 long* pnNumSteps, 
							 IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 141;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(gstrALTER_PAGINATION_ALLOW_NULL_DESTFILE);
		vecQueries.push_back(gstrALTER_PAGINATION_ALLOW_NULL_DESTPAGE);
		vecQueries.push_back("DROP INDEX [dbo].[Pagination].[IX_Pagination_DestFile]");
		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI40381");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion142(_ConnectionPtr ipConnection, 
							 long* pnNumSteps, 
							 IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 142;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(gstrALTER_SECURE_COUNTER_VALUE_LAST_UPDATED_TIME);
		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI41817");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion143(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 143;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(gstrCREATE_WORKFLOW_TYPE);
		vecQueries.push_back(gstrCREATE_WORKFLOW_V143);
		vecQueries.push_back(gstrADD_WORKFLOW_WORKFLOWTYPE_FK);
		vecQueries.push_back(gstrADD_WORKFLOW_STARTACTION_FK);
		vecQueries.push_back(gstrADD_WORKFLOW_ENDACTION_FK);
		vecQueries.push_back(gstrADD_WORKFLOW_POSTWORKFLOWACTION_FK);
		vecQueries.push_back(gstrADD_WORKFLOW_OUTPUTFILEMETADATAFIELD_FK);
		// Foreign key for OutputAttributeSetID is added in AttributeDBMgr
		vecQueries.push_back("INSERT INTO [WorkflowType] ([Code], [Meaning]) "
			"VALUES('U', 'Undefined')");
		vecQueries.push_back("INSERT INTO [WorkflowType] ([Code], [Meaning]) "
			"VALUES('R', 'Redaction')");
		vecQueries.push_back("INSERT INTO [WorkflowType] ([Code], [Meaning]) "
			"VALUES('E', 'Extraction')");
		vecQueries.push_back("INSERT INTO [WorkflowType] ([Code], [Meaning]) "
			"VALUES('C', 'Classification')");

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));
		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI41911");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion144(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 144;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back("ALTER TABLE dbo.[Action] ADD [WorkflowID] INT");
		vecQueries.push_back("ALTER TABLE dbo.[Action] DROP CONSTRAINT [Action_ASCName_Unique]");
		vecQueries.push_back("ALTER TABLE dbo.[Action] "
			"ADD CONSTRAINT[IX_Action] UNIQUE([ASCName], [WorkflowID])");
		vecQueries.push_back("ALTER TABLE dbo.[Action] "
			"WITH CHECK ADD CONSTRAINT [FK_Action_Workflow] FOREIGN KEY([WorkflowID]) "
			"REFERENCES [Workflow]([ID]) "
			"ON UPDATE CASCADE "
			"ON DELETE CASCADE");

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));
		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI41989");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion145(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 145;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(gstrCREATE_WORKFLOWFILE_V145);
		vecQueries.push_back(gstrADD_WORKFLOWFILE_WORKFLOW_FK);
		vecQueries.push_back(gstrADD_WORKFLOWFILE_FAMFILE_FK);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));
		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI42168");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion146(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 146;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back("ALTER TABLE dbo.[LockedFile] DROP CONSTRAINT [PK_LockedFile]");
		vecQueries.push_back("ALTER TABLE dbo.[LockedFile] ADD [ActionName] NVARCHAR(50) NOT NULL");
		vecQueries.push_back("ALTER TABLE dbo.[LockedFile] "
			"ADD CONSTRAINT [PK_LockedFile] PRIMARY KEY CLUSTERED ([FileID], [ActionName])");
		vecQueries.push_back("ALTER TABLE dbo.[Workflow]"
			" ADD [OutputFilePathInitializationFunction] NVARCHAR(255) NULL");

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));
		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI42183");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion147(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 147;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back("ALTER TABLE dbo.[Action] ADD [MainSequence] BIT NULL");

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));
		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI43302");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion148(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 148;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 10;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(gstrCREATE_WORKFLOWCHANGE);
		vecQueries.push_back(gstrCREATE_WORKFLOWCHANGEFILE_V148);

		vecQueries.push_back(gstrADD_WORKFLOWCHANGE_WORKFLOW_FK);
		vecQueries.push_back(gstrADD_WORKFLOWCHANGEFILE_FAMFILE_FK);
		vecQueries.push_back(gstrADD_WORKFLOWCHANGEFILE_WORKFLOWCHANGE_FK);
		vecQueries.push_back(gstrADD_WORKFLOWCHANGEFILE_ACTIONSOURCE_FK_V148);
		vecQueries.push_back(gstrADD_WORKFLOWCHANGEFILE_ACTIONDESTINATION_FK_V148);
		vecQueries.push_back(gstrADD_WORKFLOWCHANGEFILE_WORKFLOWDEST_FK_V148);
		vecQueries.push_back(gstrADD_WORKFLOWCHANGEFILE_WORKFLOWSOURCE_FK_V148);

		// Need to update the FileTaskSession to include the ActionID
		vecQueries.push_back("ALTER TABLE dbo.[FileTaskSession] ADD [ActionID] int;");
		vecQueries.push_back(gstrADD_FILE_TASK_SESSION_ACTION_FK_V148);
		vecQueries.push_back(
			"UPDATE       [FileTaskSession] "
			"SET                [ActionID] = [FAMSession].[ActionID] "
			"FROM            [FAMSession] INNER JOIN "
			"[FileTaskSession] ON [FAMSession].[ID] = [FileTaskSession].[FAMSessionID] ");

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));
		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI43412");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion149(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 149;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back("ALTER TABLE dbo.[Workflow] ADD [LoadBalanceWeight] INT NOT NULL DEFAULT(1)");

		vecQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('"
			+ gstrENABLE_LOAD_BALANCING + "', '1')");

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));
		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI43416");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion150(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 150;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back("ALTER TABLE dbo.[FileHandler] ADD [WorkflowName] NVARCHAR(100) NULL");
		vecQueries.push_back(gstrADD_FILE_HANDLER_WORKFLOW_FK);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));
		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI43474");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion151(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 151;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back("ALTER TABLE dbo.[WorkFlowChangeFile] DROP CONSTRAINT [FK_WorkflowChangeFile_ActionSource]");
		vecQueries.push_back("ALTER TABLE dbo.[WorkFlowChangeFile] DROP CONSTRAINT [FK_WorkflowChangeFile_ActionDestination]"); 
		vecQueries.push_back("ALTER TABLE dbo.[WorkFlowChangeFile] DROP CONSTRAINT [FK_WorkflowChangeFile_WorkflowDest]");
		vecQueries.push_back("ALTER TABLE dbo.[WorkFlowChangeFile] DROP CONSTRAINT [FK_WorkflowChangeFile_WorkflowSource]");
		vecQueries.push_back(gstrCREATE_ACTION_ON_DELETE_TRIGGER);
		vecQueries.push_back(gstrCREATE_WORKFLOW_ON_DELETE_TRIGGER);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));
		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI43448");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion152(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 152;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back("ALTER TABLE dbo.[WorkflowChangeFile] DROP CONSTRAINT [PK_WorkflowChangeFile]");
		vecQueries.push_back("ALTER TABLE dbo.[WorkflowChangeFile] ADD [ID] INT IDENTITY(1,1) NOT NULL"); 
		vecQueries.push_back("ALTER TABLE dbo.[WorkflowChangeFile] ADD CONSTRAINT [PK_WorkflowChangeFile] PRIMARY KEY(ID)");
		vecQueries.push_back("ALTER TABLE dbo.[WorkflowChangeFile] ALTER COLUMN [SourceActionID] INT NULL");
		vecQueries.push_back("ALTER TABLE dbo.[WorkflowChangeFile] ALTER COLUMN [DestActionID] INT NULL");
		vecQueries.push_back(gstrCREATE_WORKFLOWCHANGEFILE_INDEX);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));
		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI43476");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion153(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 153;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;
		vecQueries.push_back("DROP TRIGGER [dbo].[ActionOnDeleteTrigger]");
		vecQueries.push_back("ALTER TABLE [dbo].[FileTaskSession] DROP CONSTRAINT [FK_FileTaskSession_Action] ");
		vecQueries.push_back(gstrCREATE_ACTION_ON_DELETE_TRIGGER);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));
		executeVectorOfSQL(ipConnection, vecQueries);
		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI43550");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion154(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 154;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(gstrCREATE_FILE_TASK_SESSION_ACTION_INDEX);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));
		executeVectorOfSQL(ipConnection, vecQueries);
		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI43664");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion155(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 155;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(gstrSPLIT_MULTI_PAGE_DOCUMENT_TASK_CLASS);
		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI44842");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion156(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 156;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(gstrCREATE_MLMODEL);
		vecQueries.push_back(gstrCREATE_MLDATA);
		vecQueries.push_back(gstrADD_MLDATA_MLMODEL_FK);
		vecQueries.push_back(gstrADD_MLDATA_FAMFILE_FK);
		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI45014");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion157(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 157;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(gstrCREATE_WEB_APP_CONFIG);
		vecQueries.push_back(gstrADD_WEB_APP_CONFIG_WORKFLOW_FK);
		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI45056");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion158(_ConnectionPtr ipConnection,
							 long* pnNumSteps, 
							 IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 158;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 3;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(gstrINSERT_TASKCLASS_WEB_VERIFICATION);
		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI45330");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion159(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 159;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 3;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(gstrCREATE_DATABASE_SERVICE_TABLE_159);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI50065");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion160(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 160;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;
		
		vecQueries.push_back(gstrCREATE_REPORTING_VERIFICATION_RATES_V160);
		vecQueries.push_back(gstrADD_REPORTING_VERIFICATION_RATES_FAMFILE_FK);
		vecQueries.push_back(gstrADD_REPORTING_VERIFICATION_RATES_DATABASE_SERVICE_FK);
		vecQueries.push_back(gstrADD_REPORTING_VERIFICATION_RATES_ACTION_FK);
		vecQueries.push_back(gstrADD_REPORTING_VERIFICATION_RATES_TASK_CLASS_FK);
		vecQueries.push_back(gstrADD_REPORTING_VERIFICATION_RATES_FILE_TASK_SESSION_FK);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI45476");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion161(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 161;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<_CommandPtr> vecCmds;
		map<string, variant_t> params;
		
		vecCmds.push_back(buildCmd(ipConnection, "ALTER TABLE dbo.[FileTaskSession] ADD [ActivityTime] [float] NULL", params));

		auto cmd = buildCmd(ipConnection, gstADD_UPDATE_DBINFO_SETTING,
			{
				{gstrSETTING_NAME.c_str(), "InputActivityTimeout"}
				,{gstrSETTING_VALUE.c_str(), "30"}
				,{"@UserID", 0}
				,{"@MachineID", 0}
				,{gstrSAVE_HISTORY.c_str(), 0 }
			});
		vecCmds.push_back(cmd);

		// Remove EnableInputEventTracking
		vecCmds.push_back(buildCmd(ipConnection, "DELETE FROM [DBINFO] WHERE [Name] = 'EnableInputEventTracking'", params));

		// Remove InputEventHistorSize
		vecCmds.push_back(buildCmd(ipConnection, "DELETE FROM [DBINFO] WHERE [Name] = 'InputEventHistorySize'", params));

		vecCmds.push_back(buildCmd(ipConnection, gstrUPDATE_SCHEMA_VERSION_QUERY, { {"@SchemaVersion", asString(nNewSchemaVersion).c_str()} }));

		executeVectorOfCmd(vecCmds);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI45501");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion162(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 162;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back("sp_rename '[ReportingVerificationRates].ActiveMinutes', 'ActivityTime', 'COLUMN';");

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI45535");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion163(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 163;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(
			"ALTER TABLE dbo.[DatabaseService] ADD [Enabled] BIT NOT NULL "
			"CONSTRAINT [DF_DatabaseServiceEnabled] DEFAULT 1 ");

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI45642");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion164(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 164;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(gstrCREATE_DASHBOARD_TABLE_V164);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI45760");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion165(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 165;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back("ALTER TABLE [dbo].[FAMUser] ADD [FullUserName][nvarchar](128) NULL");
		vecQueries.push_back(gstrCREATE_PAGINATION_DESTFILE_INDEX);
		vecQueries.push_back(gstrCREATE_PAGINATION_SOURCEFILE_INDEX);
		vecQueries.push_back(gstrCREATE_PAGINATED_DEST_FILES_VIEW);
		vecQueries.push_back(gstrCREATE_USERS_WITH_ACTIVE_VIEW);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI45989");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion166(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 166;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(gstrCREATE_INPUT_EVENT_FAMUSER_WITH_TIMESTAMP_INDEX);
		vecQueries.push_back(gstrCREATE_FAMUSER_INPUT_EVENTS_TIME_VIEW_V166);
		vecQueries.push_back(gstrCREATE_PAGINATION_DATA_WITH_RANK_VIEW);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI46054");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion167(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 167;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(gstrCREATE_PROCESSING_DATA_VIEW);

		vecQueries.push_back("ALTER TABLE dbo.[DatabaseService] ADD [LastFileTaskSessionIDProcessed] INT NULL");

		// Add new columns to dashboard table
		vecQueries.push_back("ALTER TABLE dbo.[Dashboard] ADD [FAMUserID] INT NULL");
		vecQueries.push_back("ALTER TABLE dbo.[Dashboard] ADD [LastImportedDate] DATETIME NULL");

		// Get the id of the current user from the database 
		string strUserName = getCurrentUserName();
		long lFAMUserID = getKeyID(ipConnection, "FAMUser", "UserName", strUserName);

		string updateNewColumnsDataQuery =
			"DECLARE @FAMUserName nvarchar(50) = SUBSTRING(SUSER_SNAME(), CHARINDEX('\',SUSER_SNAME()) +1, 50) \r\n" 
			"DECLARE @FAMUserID INT \r\n" 
			"SELECT @FAMUserID = ID FROM FAMUser WHERE UserName = @FAMUserName \r\n" 
			"IF @FAMUserID IS NULL \r\n"
			"BEGIN \r\n" 
			"	INSERT INTO FAMUser(UserName, FullUserName) \r\n" 
			"	VALUES(@FAMUserName, SUSER_SNAME()) \r\n" 
			"\r\n" 
			"	SELECT @FAMUserID = ID FROM FAMUser WHERE UserName = @FAMUserName \r\n " 
			"END \r\n" 
			"UPDATE [Dashboard] SET FAMUserID = @FAMUserID, LastImportedDate = GETDATE()";
		vecQueries.push_back(updateNewColumnsDataQuery);
		
		vecQueries.push_back("ALTER TABLE dbo.[Dashboard] ALTER COLUMN [FAMUserID] INT NOT NULL");
		vecQueries.push_back("ALTER TABLE dbo.[Dashboard] ALTER COLUMN [LastImportedDate] DATETIME NOT NULL");

		vecQueries.push_back(gstrADD_DASHBOARD_FAMUSER_FK);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI50080");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion168(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 168;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;
		vecQueries.push_back("ALTER TABLE dbo.[DatabaseService] ADD	[StartTime] DateTime NULL");
		vecQueries.push_back("ALTER TABLE dbo.[DatabaseService] ADD	[LastWrite] DateTime NULL"); 
		vecQueries.push_back("ALTER TABLE dbo.[DatabaseService] ADD	[EndTime] DateTime NULL");
		vecQueries.push_back("ALTER TABLE dbo.[DatabaseService] ADD	[MachineID] INT NULL");
		vecQueries.push_back("ALTER TABLE dbo.[DatabaseService] ADD	[Exception] NVARCHAR(MAX) NULL");

		vecQueries.push_back(gstrADD_DATABASESERVICE_MACHINE_FK);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI46236");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion169(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 169;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;
		vecQueries.push_back("ALTER TABLE [Workflow] ADD [EditActionID] INT");
		vecQueries.push_back("ALTER TABLE [Workflow] ADD [PostEditActionID] INT");
		vecQueries.push_back(gstrADD_WORKFLOW_EDITACTION_FK);
		vecQueries.push_back(gstrADD_WORKFLOW_POSTEDITACTION_FK);
		vecQueries.push_back("ALTER TABLE [WorkflowFile] ADD [Deleted] BIT NOT NULL DEFAULT(0)");

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI46296");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion170(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 170;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;
		vecQueries.push_back("ALTER TABLE dbo.[DatabaseService] ALTER COLUMN [Description] NVARCHAR(256) NOT NULL");
		vecQueries.push_back(gstrCREATE_DATABASE_SERVICE_DESCRIPTION_INDEX); 
		vecQueries.push_back("ALTER TABLE dbo.[DatabaseService] ADD [ActiveServiceMachineID] INT NULL");
		vecQueries.push_back("ALTER TABLE dbo.[DatabaseService] ADD [NextScheduledRunTime] DateTime NULL");
		vecQueries.push_back("ALTER TABLE dbo.[DatabaseService] ADD [ActiveFAMID] INT NULL");
		vecQueries.push_back(gstrCREATE_DATABASE_SERVICE_UPDATE_TRIGGER);
		vecQueries.push_back(gstrADD_DATABASESERVICE_ACTIVEFAM_FK);
		vecQueries.push_back(gstrADD_DATABASESERVICE_ACTIVE_MACHINE_FK);
		vecQueries.push_back("ALTER TABLE dbo.[FAMFile] ADD [AddedDateTime] [datetime] NOT NULL CONSTRAINT [DF_FAMFile_AddedDateTime] DEFAULT(GETDATE())");
		vecQueries.push_back("ALTER TABLE dbo.[WorkflowFile] ADD [AddedDateTime] [datetime] NOT NULL CONSTRAINT [DF_WorkflowFile_AddedDateTime] DEFAULT(GETDATE())");
		vecQueries.push_back("INSERT INTO DBINFO ([Name], [Value]) VALUES ('ETLRestart', CONVERT(NVARCHAR(MAX), SYSDATETIMEOFFSET(), 126 ))");

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI46287");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion171(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 171;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;
		vecQueries.push_back("ALTER TABLE [SecureCounterValueChange] ALTER COLUMN [LastUpdatedTime] DATETIME NOT NULL");
		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));
		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI46478");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion172(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 172;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;
		vecQueries.push_back("ALTER TABLE [Dashboard] ADD [UseExtractedData] BIT NOT NULL DEFAULT 0");
		vecQueries.push_back("ALTER TABLE [Dashboard] ADD [ExtractedDataDefinition] [xml] NULL");
		vecQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('"
			+ gstrROOT_PATH_FOR_DASHBOARD_EXTRACTED_DATA + "', '')");
		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));
		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI46966");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion173(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 173;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;
		vecQueries.push_back(gstrINSERT_AUTO_PAGINATE_TASK_CLASS);
		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));
		
		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI47062");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion174(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 174;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;
		vecQueries.push_back(gstrCREATE_FAMUSER_INPUT_EVENTS_TIME_VIEW_LEGACY_166);
		vecQueries.push_back(gstrALTER_FAMUSER_INPUT_EVENTS_TIME_VIEW_V174);
		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI48301");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion175(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 175;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;
		vecQueries.push_back(gstrCREATE_FAMSESSION_ID_FAMUSERID_INDEX);
		vecQueries.push_back(gstrCREATE_FILETASKSESSION_DATETIMESTAMP_WITH_INCLUDES_INDEX);
		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI48424");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion176(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 176;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;
		vecQueries.push_back(gstrINSERT_RTF_DIVIDE_BATCHES_TASK_CLASS);
		vecQueries.push_back(gstrINSERT_RTF_UPDATE_BATCHES_TASK_CLASS);
		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));
		
		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI48385");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion177(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 177;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;
		vecQueries.push_back(gstrCREATE_FILE_TASK_SESSION_CACHE);
		vecQueries.push_back(gstrADD_FILE_TASK_SESSION_CACHE_ACTIVEFAM_FK);
		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI48411");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion178(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 178;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;
		// Column names/order changed; 177 was an internal build and data here is not expected, simply re-create.
		vecQueries.push_back("DROP TABLE [FileTaskSessionCache]");
		vecQueries.push_back(gstrCREATE_FILE_TASK_SESSION_CACHE);
		vecQueries.push_back(gstrADD_FILE_TASK_SESSION_CACHE_ACTIVEFAM_FK);
		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI49541");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion179(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 179;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(gstrINSERT_TASKCLASS_DOCUMENT_API);
		vecQueries.push_back("UPDATE [dbo].[TaskClass] SET [Name] = 'Pagination: Verify' "
			"WHERE [GUID] = 'DF414AD2-742A-4ED7-AD20-C1A1C4993175'");
		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI49584");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion180(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 180;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(gstrADD_GUID_COLUMNS);
		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI49710");
}

//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion181(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 181;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(gstrCREATE_DATABASE_MIGRATION_WIZARD_REPORTING_181);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI49742");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion182(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 182;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(gstrALTER_DATABASE_MIGRATION_WIZARD_REPORTING);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI49823");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion183(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 183;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(gstrCREATE_GET_CLUSTER_NAME_PROCEDURE);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI49859");
} 
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion184(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 184;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		// This fixes a missing contraint on the MetadataField table, it is on the table if the table is created,
		// but not if the database is updated from before the MetadataField table was created with the constraint
		vecQueries.push_back(gstrADD_METADATAFIELD_UNIQUE_NAME_CONSTRAINT);

		// This was missing in some DEMO databases, this will create it if it doesn't exist
		vecQueries.push_back(gstrCREATE_PROCESSING_DATA_VIEW);

		vecQueries.push_back( gstrCREATE_FAMUSER_INPUT_EVENTS_TIME_WITH_FILEID_VIEW_V184);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI49875");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion185(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 185;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(gstrALTER_DATABASE_MIGRATION_WIZARD_REPORTING_COLUMN_SIZES);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI49883");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion186(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 186;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(gstrALTER_FAMUSER_REMOVE_GUID);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI49891");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion187(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 187;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(gstrCREATE_USAGE_FOR_SPECIFIC_USER_SPECIFIC_DAY_PROCEDURE);
		vecQueries.push_back(gstrCREATE_TABLE_FROM_COMMA_SEPARATED_LIST_FUNCTION);
		vecQueries.push_back(gstrCREATE_USER_COUNTS_STORED_PROCEDURE_V187_V199);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI50242");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion188(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 188;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back("DROP INDEX [IX_Skipped_File_FAMSession] ON [dbo].[SkippedFile]");
		vecQueries.push_back("DROP INDEX [IX_Skipped_File] ON [dbo].[SkippedFile]");

		vecQueries.push_back(gstrCREATE_SKIPPED_FILE_INDEX);
		vecQueries.push_back(gstrCREATE_GET_FILES_TO_PROCESS_STORED_PROCEDURE);
		vecQueries.push_back("ALTER TABLE [dbo].[FileActionStatus] DROP CONSTRAINT [PK_FileActionStatus]");
		vecQueries.push_back(gstrCREATE_ACTIONSTATUS_ACTIONID_PRIORITY_FILE_INDEX_188);
		vecQueries.push_back("ALTER TABLE [dbo].[FileActionStatus] ADD  CONSTRAINT [PK_FileActionStatus] PRIMARY KEY "
			"([FileID] ASC,	[ActionID] ASC)");
		vecQueries.push_back("INSERT INTO DBInfo (Name, Value) VALUES ('UseGetFilesLegacy', '0')");

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI51468");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion189(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 189;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		// The procedure was updated
		vecQueries.push_back("DROP INDEX [IX_ActionStatusActionIDPriorityFileID] ON [dbo].[FileActionStatus] WITH ( ONLINE = OFF )");
		vecQueries.push_back(gstrCREATE_GET_FILES_TO_PROCESS_STORED_PROCEDURE);
		vecQueries.push_back(gstrCREATE_FILE_TASK_SESSION_TASKCLASSID_WITH_ID_SESSIONID_DATE);
		vecQueries.push_back(gstrCREATE_ACTIONSTATUS_PRIORITY_FILE_ACTIONID_INDEX);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI51491");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion190(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 190;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		// The procedure was updated
		vecQueries.push_back(gstrCREATE_WORKFLOWFILE_FILEID_WORKFLOWID_DELETED_INDEX);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI51553");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion191(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 191;

		if (pnNumSteps != nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		// The procedure was updated
		vecQueries.push_back(gstrCREATE_GET_FILES_TO_PROCESS_STORED_PROCEDURE);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI51559");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion192(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 192;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 3;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		// Change WorkflowFile.Deleted to Invisible
		vecQueries.push_back("DROP INDEX [IX_Workflowfile_FileID_WorkflowID_Deleted] ON [dbo].[WorkflowFile]");
		vecQueries.push_back("EXEC sp_rename '[dbo].[WorkflowFile].[Deleted]', 'Invisible', 'COLUMN';");
		vecQueries.push_back(gstrCREATE_WORKFLOWFILE_FILEID_WORKFLOWID_INVISIBLE_INDEX);

		// Drop ActionStatistics and ActionStatisticsDelta tables so they can be re-created with added Invisible column
		// No need to transfer data; instead, regenerate the stats afterward.
		vecQueries.push_back("DROP Table [ActionStatistics]");
		vecQueries.push_back("DROP Table [ActionStatisticsDelta]");

		// Add ActionStatistics table.
		vecQueries.push_back(gstrCREATE_ACTION_STATISTICS_TABLE);
		vecQueries.push_back(gstrADD_STATISTICS_ACTION_FK);

		// Add ActionStatisticsDelta table.
		vecQueries.push_back(gstrCREATE_ACTION_STATISTICS_DELTA_TABLE);
		vecQueries.push_back(gstrCREATE_ACTION_STATISTICS_DELTA_ACTIONID_ID_INDEX);
		vecQueries.push_back(gstrADD_ACTION_STATISTICS_DELTA_ACTION_FK);

		// Regenerate the action statistics for all actions (empty "where" clause)
		string strCreateActionStatsSQL = gstrRECREATE_ACTION_STATISTICS_FOR_ACTION;
		replaceVariable(strCreateActionStatsSQL, "<ActionIDWhereClause>", "");
		vecQueries.push_back(strCreateActionStatsSQL);

		// This procedure was updated to work with these stats changes.
		vecQueries.push_back(gstrCREATE_GET_FILES_TO_PROCESS_STORED_PROCEDURE);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI51615");
}
// ------------------------------------------------------------------------------------------------ -
int UpdateToSchemaVersion193(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 193;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 3;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;


	
		vecQueries.push_back("DELETE FROM DBInfo WHERE [Name]= 'UseGetFilesLegacy'");

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI51660");
}
// -------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion194(_ConnectionPtr ipConnection, long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 194;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		// IX_ActionStatisticsDeltaActionID_ID has been updated to account for the Invisible column
		// and, thus, limit potential performance regressions related to:
		// https://extract.atlassian.net/browse/ISSUE-15744
		// https://extract.atlassian.net/browse/ISSUE-16044
		vecQueries.push_back("DROP INDEX [ActionStatisticsDelta].[IX_ActionStatisticsDeltaActionID_ID]");
		vecQueries.push_back(gstrCREATE_ACTION_STATISTICS_DELTA_ACTIONID_INDEX);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI51681");
}
// -------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion195(_ConnectionPtr ipConnection, long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 195;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		// Drob the fn_TableFromCommaSeparatedList function then recreate it
		vecQueries.push_back("DROP FUNCTION [dbo].[fn_TableFromCommaSeparatedList]");
		vecQueries.push_back(gstrCREATE_TABLE_FROM_COMMA_SEPARATED_LIST_FUNCTION);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI51741");
}
int UpdateToSchemaVersion196(_ConnectionPtr ipConnection, long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 196;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;
		vecQueries.push_back(gstrDASHBOARD_CHANGEPK_TO_GUID);
		vecQueries.push_back(gstrCREATE_SECURITY_SCHEMA);
		vecQueries.push_back(gstrCREATE_ROLE_TABLE);
		vecQueries.push_back(gstrCREATE_GROUP_TABLE);
		vecQueries.push_back(gstrCREATE_LOGINGROUPMEMBERSHIP_TABLE);
		vecQueries.push_back(gstrCREATE_GROUPACTION_TABLE);
		vecQueries.push_back(gstrCREATE_GROUPDASHBOARD_TABLE);
		vecQueries.push_back(gstrCREATE_GROUPREPORT_TABLE);
		vecQueries.push_back(gstrCREATE_GROUPWORKFLOW_TABLE);
		vecQueries.push_back(gstrCREATE_GROUPROLE_TABLE);
		vecQueries.push_back(gstrLOGIN_ADD_COLUMN_ALTER_FAMUSER);
		vecQueries.push_back(gstrADD_LOGINGROUPMEMBERSHIP_GROUP_ID_FK);
		vecQueries.push_back(gstrADD_LOGINGROUPMEMBERSHIP_LOGIN_ID_FK);
		vecQueries.push_back(gstrADD_GROUPACTION_GROUP_ID_FK);
		vecQueries.push_back(gstrADD_GROUPACTION_Action_ID_FK);
		vecQueries.push_back(gstrADD_GROUPDASHBOARD_GROUP_ID_FK);
		vecQueries.push_back(gstrADD_GROUPDASHBOARD_DASHBOARD_GUID_FK);
		vecQueries.push_back(gstrADD_GROUPREPORT_GROUP_ID_FK);
		vecQueries.push_back(gstrADD_GROUPWORKFLOW_GROUP_ID_FK);
		vecQueries.push_back(gstrADD_GROUPWORKFLOW_WORKFLOW_ID_FK);
		vecQueries.push_back(gstrADD_GROUPROLE_ROLE_ID_FK);
		vecQueries.push_back(gstrADD_GROUPROLE_GROUP_ID_FK);
		vecQueries.push_back(gstrINSERT_ROLE_DEFAULT_ROLES);
		vecQueries.push_back(gstrINSERT_SECURITYGROUP_DEFAULT_GROUPS);
		vecQueries.push_back(gstrADD_FAMUSER_LOGIN_ID_FK);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI51768");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion197(_ConnectionPtr ipConnection, long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 197;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		CppSqlApplicationRole::CreateApplicationRole(ipConnection, "ExtractSecurityRole", "Change2This3Password", CppSqlApplicationRole::AllAccess);
		CppSqlApplicationRole::CreateApplicationRole(ipConnection, "ExtractRole", "Change2This3Password", CppSqlApplicationRole::AllAccess);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI51777");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion198(_ConnectionPtr ipConnection, long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 198;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;
		vecQueries.push_back(gstrDBINFO_ADD_AZURE_VALUES);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI51865");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion199(_ConnectionPtr ipConnection, long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 199;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;
		vecQueries.push_back(gstrCREATE_USER_COUNTS_STORED_PROCEDURE_V187_V199);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI51932");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion200(_ConnectionPtr ipConnection, long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 200;

		if (pnNumSteps != __nullptr)
		{
			// This update requires updating every row in the FileTaskSession table and is therefore
			// O(n) relative to the number of task sessions in the DB.
			*pnNumSteps += 10;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back("ALTER TABLE dbo.FileTaskSession ADD TaskClassGUID UNIQUEIDENTIFIER;");
		vecQueries.push_back(
			"UPDATE dbo.FileTaskSession \r\n"
			"	Set FileTaskSession.TaskClassGUID = TaskClass.GUID \r\n"
			"	FROM FileTaskSession inner join TaskClass ON FileTaskSession.TaskClassID = TaskClass.ID; \r\n");
		vecQueries.push_back("DROP VIEW [dbo].[vFAMUserInputEventsTime]");
		vecQueries.push_back(gstrCREATE_FAMUSER_INPUT_EVENTS_TIME_VIEW);
		vecQueries.push_back("DROP VIEW [dbo].[vFAMUserInputWithFileID]");
		vecQueries.push_back(gstrCREATE_FAMUSER_INPUT_EVENTS_TIME_WITH_FILEID_VIEW);
		vecQueries.push_back(gstrCREATE_USER_COUNTS_STORED_PROCEDURE); 


		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI51957");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion201(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 201;


		if (pnNumSteps != nullptr)
		{
			// This update requires updating every row in the FileTaskSession and
			// ReportingVerificationRates tables and is therefore O(n) relative to the
			// number of task sessions in the DB.
			*pnNumSteps += 10;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		// StartDateTime/DurationMinusTimeout nullable to allow for existing DBs that have rows with NULL DateTimeStamp
		vecQueries.push_back("ALTER TABLE [FileTaskSession] ADD [StartDateTime] [DATETIME] NULL DEFAULT(GETDATE())");
		vecQueries.push_back("ALTER TABLE [FileTaskSession] ADD [TimedOut] [bit] NOT NULL DEFAULT(0)");
		vecQueries.push_back("ALTER TABLE [FileTaskSession] ADD [DurationMinusTimeout] [float] NULL");
		vecQueries.push_back("UPDATE [FileTaskSession] "
			"	SET [StartDateTime] = DATEADD(MILLISECOND, -[Duration]*1000, [DateTimeStamp]), "
			"		[DurationMinusTimeout] = [Duration]");
		vecQueries.push_back("ALTER TABLE [ReportingVerificationRates] "
			"ADD [DurationMinusTimeout] [float] NOT NULL CONSTRAINT [DF_DurationMinusTimeout] DEFAULT(0.0)");
		vecQueries.push_back("UPDATE [ReportingVerificationRates] SET [DurationMinusTimeout] = [Duration]");

		int nDefaultSessionTimeout = getDefaultSessionTimeoutFromWebConfig(ipConnection);
		if (nDefaultSessionTimeout > 0)
		{
			auto ue = UCLIDException("ELI52960",
				"Application trace: Defaulting session timeout per web redaction verification settings.");
			ue.addDebugInfo("SessionTimeout", nDefaultSessionTimeout);
			ue.log();
		}
		vecQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES ('"
			+ gstrVERIFICATION_SESSION_TIMEOUT + "', " + asString(nDefaultSessionTimeout) + ")");

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI51942");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion202(_ConnectionPtr ipConnection, long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 202;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 10; // This will touch every row in the FileActionStatus table
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(gstrFILEACTIONSTATUS_ADD_RANDOM_ID);

		// This procedure was updated to support using RandomID
		vecQueries.push_back(gstrCREATE_GET_FILES_TO_PROCESS_STORED_PROCEDURE);

		vecQueries.push_back(buildUpdateSchemaVersionQuery(nNewSchemaVersion));

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI52968");
}


//-------------------------------------------------------------------------------------------------
// IFileProcessingDB Methods - Internal
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::DefineNewAction_Internal(bool bDBLocked, BSTR strAction, long* pnID)
{
	try
	{
		try
		{
			string strActionName = asString(strAction);

			// Validate the new action name
			validateNewActionName(strActionName);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				if (getActionIDNoThrow(ipConnection, strActionName, "") > 0)
				{
					UCLIDException ue("ELI43555", "Action " + strActionName + " already exists");
					throw ue;
				}

				*pnID = getKeyID(ipConnection, "Action", "ASCName", strActionName);

				// Commit this transaction
				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI30356");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30626");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::DeleteAction_Internal(bool bDBLocked, BSTR strAction)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				string strActionName = asString(strAction);

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Make sure processing is not active of this action
				assertProcessingNotActiveForAction(bDBLocked, ipConnection, strActionName);

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_criticalSection);

				m_mapActionIdsForActiveWorkflow.clear();

				// Delete the action
				string strDeleteActionQuery = "DELETE FROM [Action] WHERE [ASCName] = @ActionName";

				executeCmd(buildCmd(ipConnection, strDeleteActionQuery, { { "@ActionName", strAction} }));

				// Commit this transaction
				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI30358");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30628");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetActions_Internal(bool bDBLocked, IStrToStrMap * * pmapActionNameToID)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Create StrToStrMap to return the list of actions
				IStrToStrMapPtr ipActions = getActions(ipConnection, getActiveWorkflow());
				ASSERT_RESOURCE_ALLOCATION("ELI13529", ipActions != __nullptr);

				// return the StrToStrMap containing all actions
				*pmapActionNameToID = ipActions.Detach();

			END_CONNECTION_RETRY(ipConnection, "ELI23526");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30630");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetAllActions_Internal(bool bDBLocked, IStrToStrMap** pmapActionNameToID)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();

			// Make sure the DB Schema is the expected version
			validateDBSchemaVersion();

			// Create StrToStrMap to return the list of actions
			IStrToStrMapPtr ipActions = getActions(ipConnection, "");
			ASSERT_RESOURCE_ALLOCATION("ELI42095", ipActions != __nullptr);

			// return the StrToStrMap containing all actions
			*pmapActionNameToID = ipActions.Detach();

			END_CONNECTION_RETRY(ipConnection, "ELI42096");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI42097");
	}
	catch (UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::AddFile_Internal(bool bDBLocked, BSTR strFile,  BSTR strAction, long nWorkflowID,
										EFilePriority ePriority,
										VARIANT_BOOL bForceStatusChange, VARIANT_BOOL bFileModified,
										EActionStatus eNewStatus, VARIANT_BOOL bSkipPageCount,
										VARIANT_BOOL * pbAlreadyExists, EActionStatus *pPrevStatus,
										IFileRecord* * ppFileRecord)
{
	INIT_EXCEPTION_AND_TRACING("MLI03278");

	try
	{
		try
		{
			string strFileName = asString(strFile);

			// Create the file record to return
			UCLID_FILEPROCESSINGLib::IFileRecordPtr ipNewFileRecord(CLSID_FileRecord);
			ASSERT_RESOURCE_ALLOCATION("ELI30359", ipNewFileRecord != __nullptr);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;
			
			BEGIN_CONNECTION_RETRY()

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();

				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Do not allow adding of files to all workflows via AddFile.
				if (nWorkflowID <= 0 && m_bRunningAllWorkflows)
				{
					UCLIDException ue("ELI42029", "Workflow has not been set.");
					ue.addDebugInfo("FPS File", m_strFPSFileName, false);
					throw ue;
				}

				if (nWorkflowID <= 0 && m_bUsingWorkflowsForCurrentAction)
				{
					nWorkflowID = getActiveWorkflowID(ipConnection);
				}

				string strActionName = asString(strAction);

				long nActionID = getActionID(ipConnection, strActionName, nWorkflowID);
				
				_lastCodePos = "10";

				// Open a recordset that contain only the record (if it exists) with the given filename
				_CommandPtr cmdGetFile = buildCmd(ipConnection,
					"SELECT * FROM FAMFile WHERE FileName = @FileName",
					{ {"@FileName",  strFile} });

				// Create a pointer to a recordset
				_RecordsetPtr ipFileSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI30360", ipFileSet != __nullptr);

				ipFileSet->Open((IDispatch*)cmdGetFile, vtMissing, adOpenDynamic,
					adLockOptimistic, adCmdText);

				_lastCodePos = "30";

				// Check whether the file already exists in the database
				bool bAlreadyExists = ipFileSet->adoEOF == VARIANT_FALSE;
				// .. and whether it has been used in the workflow (rest of check below)
				bool bAlreadyExistsInWorkflow = bAlreadyExists && (nWorkflowID <= 0);

				// Initialize the id
				long nID = 0;

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_criticalSection);

				_lastCodePos = "45";

				UCLID_FILEPROCESSINGLib::IFileRecordPtr ipOldRecord = __nullptr;
				FieldsPtr ipFields = __nullptr;

				// if the file is in the FAMFile table get the ID
				if (bAlreadyExists)
				{
					// Get the fields from the file set
					ipFields = ipFileSet->Fields;
					ASSERT_RESOURCE_ALLOCATION("ELI30362", ipFields != __nullptr);

					// Get the file record from the fields
					ipOldRecord = getFileRecordFromFields(ipFields);
					ASSERT_RESOURCE_ALLOCATION("ELI30363", ipOldRecord != __nullptr);

					// Set the Current file Records ID
					nID = ipOldRecord->FileID;

					if (nWorkflowID > 0)
					{
						// NOTE: If the workflow/file pair is not yet in [WorkflowFile], it will be
						// added as part of setStatusForFile below.
						// -1 = not in WorkflowFile, 0 = marked Invisible in WorkflowFile
						bAlreadyExistsInWorkflow = isFileInWorkflow(ipConnection, nID, nWorkflowID) >= 0;
					}
				}

				*pbAlreadyExists = asVariantBool(bAlreadyExistsInWorkflow);

				// Get the FileActionStatus recordset with the status of the file for the action
				// NOTE: if nID = 0 or File status is unattempted for the action the recordset will be empty
				_RecordsetPtr ipFileActionStatusSet = getFileActionStatusSet(ipConnection, nID, nActionID);
				*pPrevStatus = (asCppBool(!ipFileActionStatusSet->adoEOF)) ?
					asEActionStatus(getStringField(ipFileActionStatusSet->Fields, "ActionStatus")) : kActionUnattempted;

				// Check if the existing file is currently from an active pagination process
				if (bAlreadyExistsInWorkflow && isFileInPagination(ipConnection, nID))
				{
					// Update QueueEvent table if enabled
					if (m_bUpdateQueueEventTable)
					{
						// add a new QueueEvent record 
						addQueueEventRecord(ipConnection, nID, nActionID, asString(strFile), (bFileModified == VARIANT_TRUE) ? "M":"A");

						// Commit the changes to the database
						tg.CommitTrans();
					}
					*ppFileRecord = (IFileRecord*)ipOldRecord.Detach();
					return true;
				}

				// Only update file size and page count if the previous status is unattempted
				// or force status change is true
				// [FlexIDSCore #3734]
				long long llFileSize = 0;
				long nPages = 0;
				if (*pPrevStatus == kActionUnattempted || bForceStatusChange == VARIANT_TRUE)
				{
					// Get the size of the file
					// [LRCAU #5157] - getSizeOfFile performs a wait for file access call, no need
					// to perform an additional call here.
					llFileSize = (long long)getSizeOfFile(strFileName);

					// [LegacyRCAndUtils:6354]
					// Check page count only if bSkipPageCount == VARIANT_FALSE
					if (asCppBool(bSkipPageCount))
					{
						nPages = 0;
					}
					else
					{
						// get the file type
						EFileType efType = getFileType(strFileName);

						// if it is an image file OR unknown file [p13 #4816] attempt to
						// get the number of pages
						if (efType == kImageFile || efType == kUnknown)
						{
							try
							{
								// Get the number of pages in the file if it is an image file
								nPages = getNumberOfPagesInImage(strFileName);
							}
							catch(...)
							{
								// if there is an error this may not be a valid image file but we
								// still want to put it in the database
								nPages = 0;
							}
						}
					}
				}

				// Update the new file record with the file data
				ipNewFileRecord->SetFileData(-1, nActionID, strFileName.c_str(),
					llFileSize, nPages, (UCLID_FILEPROCESSINGLib::EFilePriority) ePriority, nWorkflowID);

				_lastCodePos = "50";

				string strNewStatus = asStatusString(eNewStatus);

				// If file did not already exist then add a new record to the database
				if (!bAlreadyExists)
				{
					// Add new record
					ipFileSet->AddNew();

					// Get the fields from the file set
					ipFields = ipFileSet->Fields;
					ASSERT_RESOURCE_ALLOCATION("ELI30361", ipFields != __nullptr);

					// Set the fields from the new file record
					setFieldsFromFileRecord(ipFields, ipNewFileRecord);

					long nPriority = getLongField(ipFields, "Priority");

					_lastCodePos = "60";

					// Add the record
					ipFileSet->Update();

					_lastCodePos = "70";

					// Requery the recordset so that we can get the file ID
					ipFileSet->Requery(adOptionUnspecified);

					_lastCodePos = "71";

					// Check to make sure the recordset is not empty
					if (ipFileSet->adoEOF == VARIANT_TRUE)
					{
						UCLIDException ue("ELI31070", "File record not found in database.");	
						ue.addDebugInfo("Filename", strFileName);
						throw ue;
					}
					
					_lastCodePos = "72";

					// Reset the ipFields to the required fields
					ipFields = ipFileSet->Fields;
					ASSERT_RESOURCE_ALLOCATION("ELI31068", ipFields != __nullptr);

					_lastCodePos = "73";

					// get the new records ID to return
					nID = getLongField(ipFields, "ID");
					ASSERT_RESOURCE_ALLOCATION("ELI31069", nID > 0);

					_lastCodePos = "74";

					// Set the new file Record ID to nID;
					ipNewFileRecord->FileID = nID;
					
					_lastCodePos = "80";

					// Create a record in the FileActionStatus table for the status of the new record
					executeCmd(buildCmd(ipConnection,
						"INSERT INTO FileActionStatus (FileID, ActionID, ActionStatus, Priority) "
						" VALUES (@FileID, @ActionID, @Status, @Priority)",
						{
							{ "@FileID", nID },
							{ "@ActionID", nActionID },
							{ "@Status", strNewStatus.c_str() },
							{ "@Priority", nPriority }
						}));

					_lastCodePos = "86";

					// https://extract.atlassian.net/browse/ISSUE-13491
					if (strNewStatus == "S")
					{
						addSkipFileRecord(ipConnection, nID, nActionID);
					}

					// In the case that the file did exist in the DB, but not the workflow, the
					// [WorkflowFile] row will be added as part of the setStatusForFile call.
					if (!bAlreadyExistsInWorkflow && nWorkflowID > 0)
					{
						executeCmd(buildCmd(ipConnection,
							"INSERT INTO [WorkflowFile] ([WorkflowID], [FileID]) VALUES (@WorkflowID, @FileID)",
							{
								{ "@WorkflowID", nWorkflowID},
								{ "@FileID", nID}
							}));
					}

					_lastCodePos = "87";

					// update the statistics
					updateStats(ipConnection, nActionID, *pPrevStatus, eNewStatus, ipNewFileRecord, NULL, false);
					_lastCodePos = "90";
				}
				else
				{
					_lastCodePos = "100";

					// If Force processing is set need to update the status or if the previous status for this action was unattempted
					if (bForceStatusChange == VARIANT_TRUE || *pPrevStatus == kActionUnattempted)
					{
						// Call setStatusForFile to handle updating all tables related to the status
						// change, as appropriate.
						setStatusForFile(ipConnection, nID, strActionName, nWorkflowID, eNewStatus, true, false);

						_lastCodePos = "110";

						// set the fields to the new file Record
						// (only update the priority if force processing)
						setFieldsFromFileRecord(ipFields, ipNewFileRecord, asCppBool(bForceStatusChange));

						_lastCodePos = "120";

						// It can be assumed after the call to setFieldsFromFileRecord that the record
						// is in the FileActionStatus table for this action at least.
						string strPriority = asString(getLongField(ipFields, "Priority"));
						string strUpdatePrioritySQL = "UPDATE FileActionStatus SET Priority = " +
							strPriority + " WHERE FileID = " + asString(nID) +
							" AND Priority <> " + strPriority;

						executeCmdQuery(ipConnection, strUpdatePrioritySQL);

						_lastCodePos = "150";
					}
					else
					{
						// Set the file size and page count for the file record to
						// the file size and page count stored in the database
						ipNewFileRecord->FileSize = ipOldRecord->FileSize;
						ipNewFileRecord->Pages = ipOldRecord->Pages;
						_lastCodePos = "152";
					}
				}

				// Set the new file Record ID to nID;
				ipNewFileRecord->FileID = nID;

				_lastCodePos = "155";

				// Update QueueEvent table if enabled
				if (m_bUpdateQueueEventTable)
				{
					// add a new QueueEvent record 
					addQueueEventRecord(ipConnection, nID, nActionID, asString(strFile), (bFileModified == VARIANT_TRUE) ? "M":"A");
				}

				_lastCodePos = "160";

				if (m_bUsingWorkflowsForCurrentAction && !bAlreadyExistsInWorkflow)
				{
					initOutputFileMetadataFieldValue(ipConnection, nID, asString(strFile), nWorkflowID);
				}

				// Commit the changes to the database
				tg.CommitTrans();

				_lastCodePos = "170";

				// Return the file record
				*ppFileRecord = (IFileRecord*)ipNewFileRecord.Detach();

			END_CONNECTION_RETRY(ipConnection, "ELI30365");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30631");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		ue.addDebugInfo("FileName", asString(strFile));
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::RemoveFile_Internal(bool bDBLocked, BSTR strFile, BSTR strAction )
{
	try
	{
		try
		{

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY()

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Create a pointer to a recordset
				_RecordsetPtr ipFileSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI30366", ipFileSet != __nullptr);

				// Replace any occurrences of ' with '' this is because SQL Server use the ' to
				// indicate the beginning and end of a string
				string strFileName = asString(strFile);
				replaceVariable(strFileName, "'", "''");

				// Open a recordset that contain only the record (if it exists) with the given filename
				string strFileSQL = "SELECT * FROM FAMFile WHERE FileName = '" + strFileName + "'";
				ipFileSet->Open(strFileSQL.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
					adLockOptimistic, adCmdText);

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_criticalSection);

				// Setup action name and action id
				string strActionName = asString(strAction);
				long nActionID = getActionID(ipConnection, strActionName);

				// If file exists this should not be at end of file
				if (ipFileSet->adoEOF == VARIANT_FALSE)
				{
					// Get the fields from the file set
					FieldsPtr ipFields = ipFileSet->Fields;
					ASSERT_RESOURCE_ALLOCATION("ELI30367", ipFields != __nullptr);

					// Get the old Record from the fields
					UCLID_FILEPROCESSINGLib::IFileRecordPtr ipOldRecord;
					ipOldRecord = getFileRecordFromFields(ipFields);

					// Get the file ID
					long nFileID = ipOldRecord->FileID;

					// Get the FileActionStatus record for the file id and action id
					_RecordsetPtr ipFileActionStatusSet = getFileActionStatusSet(ipConnection, nFileID, nActionID);

					// Check for empty ipFileActionStatusSet, since this will indicate that the current state is unattempted
					if (!asCppBool(ipFileActionStatusSet->adoEOF))
					{
						// only need to look at the fields for the action status
						ipFields = ipFileActionStatusSet->Fields;

						// Get the Previous file state
						string strActionState = getStringField(ipFields, "ActionStatus");

						// only change the state if the current state is pending
						if (strActionState == "P")
						{
							// To set to unattempted just need to delete the record from the FileActionStatus table
							string strSQLDeleteFileActionStatus = "DELETE FROM FileActionStatus WHERE FileID = " + asString(nFileID) 
								+ " AND ActionID = " + asString(nActionID);

							// Execute query to delete the FileActionStatus record for the file and action
							executeCmdQuery(ipConnection, strSQLDeleteFileActionStatus);

							// Only update FileActionStateTransition Table if required
							if (m_bUpdateFASTTable)
							{
								// Add a ActionStateTransition record for the state change
								addFileActionStateTransition(ipConnection, nFileID, nActionID, strActionState, "U", "", "Removed");
							}

							bool bIsInvisible = false;
							long nWorkflowID = getWorkflowID(ipConnection, nActionID);
							if (nWorkflowID > 0)
							{
								bIsInvisible = isFileInWorkflow(ipConnection, nFileID, nWorkflowID) == 0; // 0 = Invisible
							}
							// update the statistics
							updateStats(ipConnection, nActionID, asEActionStatus(strActionState), kActionUnattempted, NULL, ipOldRecord, bIsInvisible); 
						}

						// Update QueueEvent table if enabled
						if (m_bUpdateQueueEventTable)
						{
							// add record the QueueEvent table to indicate that the file was deleted
							addQueueEventRecord(ipConnection, nFileID, nActionID, asString(strFile), "D");
						}
					}
				}

				// Commit the changes
				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI30368");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30632");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::NotifyFileProcessed_Internal(bool bDBLocked, long nFileID,  BSTR strAction,
													 long nWorkflowID,
													 VARIANT_BOOL vbAllowQueuedStatusOverride)
{
	try
	{
		try
		{
			try
			{
				// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
				ADODB::_ConnectionPtr ipConnection = __nullptr;

				BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Ensure file gets added to current workflow if it is missing (setFileActionState)
				nWorkflowID = nWorkflowID == -1 ? getActiveWorkflowID(ipConnection) : nWorkflowID;

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_criticalSection);

				// change the given files state to completed unless there is a pending state in the
				// QueuedActionStatusChange table.
				setFileActionState(ipConnection, nFileID, asString(strAction), nWorkflowID, "C",
					"", false, asCppBool(vbAllowQueuedStatusOverride));

				tg.CommitTrans();

				END_CONNECTION_RETRY(ipConnection, "ELI23529");
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30305");
		}
		catch(UCLIDException &uex)
		{
			if (!bDBLocked)
			{
				return false;
			}
			uex.addDebugInfo("File ID", nFileID);
			uex.addDebugInfo("Action Name", asString(strAction));
			throw uex;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30633");

	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::NotifyFileFailed_Internal(bool bDBLocked, long nFileID,  BSTR strAction,  
												  long nWorkflowID, BSTR strException,
												  VARIANT_BOOL vbAllowQueuedStatusOverride)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Ensure file gets added to current workflow if it is missing (setFileActionState)
				nWorkflowID = nWorkflowID == -1 ? getActiveWorkflowID(ipConnection) : nWorkflowID;

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_criticalSection);

				// [LegacyRCAndUtils:6054]
				// Store the full log string which contains additional info which may be useful.
				UCLIDException ue;
				ue.createFromString("ELI32298", asString(strException));
				string strLogString = ue.createLogString();

				// change the given files state to Failed unless there is a pending state in the
				// QueuedActionStatusChange table.
				setFileActionState(ipConnection, nFileID, asString(strAction), nWorkflowID, "F",
					strLogString, false, asCppBool(vbAllowQueuedStatusOverride));

				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI23530");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30636");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::SetFileStatusToPending_Internal(bool bDBLocked, long nFileID, BSTR strAction,
														VARIANT_BOOL vbAllowQueuedStatusOverride)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();
				
				// Ensure file gets added to current workflow if it is missing (setFileActionState)
				long nWorkflowID = getActiveWorkflowID(ipConnection);

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_criticalSection);
				
				// change the given files state to Pending
				setFileActionState(ipConnection, nFileID, asString(strAction), nWorkflowID, "P", "", 
					false, asCppBool(vbAllowQueuedStatusOverride));

				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI23531");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30637");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::SetFileStatusToUnattempted_Internal(bool bDBLocked, long nFileID, BSTR strAction,
															VARIANT_BOOL vbAllowQueuedStatusOverride)
{
	try
	{
		try
		{


			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();;

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_criticalSection);

				// change the given files state to unattempted
				setFileActionState(ipConnection, nFileID, asString(strAction), -1, "U", "",
					false, asCppBool(vbAllowQueuedStatusOverride));

				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI23532");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30638");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::SetFileStatusToSkipped_Internal(bool bDBLocked, long nFileID, BSTR strAction,
													   VARIANT_BOOL bRemovePreviousSkipped,
													   VARIANT_BOOL vbAllowQueuedStatusOverride)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;
		
			BEGIN_CONNECTION_RETRY();
		
			// Get the connection for the thread and save it locally.
			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();

			// Ensure file gets added to current workflow if it is missing (setFileActionState)
			long nWorkflowID = getActiveWorkflowID(ipConnection);

			// Begin a transaction
			TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_criticalSection);

			// Change the given files state to Skipped
			setFileActionState(ipConnection, nFileID, asString(strAction), nWorkflowID, "S", "",
				false, asCppBool(vbAllowQueuedStatusOverride), -1, asCppBool(bRemovePreviousSkipped));

			tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI26938");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30639");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetFileStatus_Internal(bool bDBLocked, long nFileID,  BSTR strAction,
									VARIANT_BOOL vbAttemptRevertIfLocked, EActionStatus * pStatus)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Create a pointer to a recordset
				_RecordsetPtr ipFileSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI30369", ipFileSet != __nullptr);

				// Set the action name from the parameter
				string strActionName = asString(strAction);

				// Get the action ID and update the strActionName to stored value
				long nActionID = getActionID(ipConnection, strActionName);

				// Open Recordset that contains only the record with the given ID
				string strFileSQL = "SELECT FAMFile.ID, COALESCE(ActionStatus, 'U') AS ActionStatus "
					"FROM FAMFile LEFT JOIN FileActionStatus ON FileActionStatus.FileID = FAMFile.ID "
					" AND FileActionStatus.ActionID = " + asString(nActionID) + " WHERE FAMFile.ID = " 
					+ asString (nFileID);
				ipFileSet->Open(strFileSQL.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
					adLockOptimistic, adCmdText);

				// if the file exists should not be at the end of the file
				if (ipFileSet->adoEOF == VARIANT_FALSE)
				{
					// Set return value to the current Action Status
					string strStatus = getStringField(ipFileSet->Fields, "ActionStatus");
					*pStatus = asEActionStatus(strStatus);

					// If the file status is processing and the caller would like to check if it is a
					// locked file from a timed-out instance, try reverting before returning the initial
					// status.
					if (*pStatus == kActionProcessing && asCppBool(vbAttemptRevertIfLocked))
					{
						revertTimedOutProcessingFAMs(bDBLocked, ipConnection);

						// Re-query to see if the status changed as a result of being auto-reverted.
						ipFileSet->Requery(adOptionUnspecified);

						// Get the updated status
						string strStatus = getStringField(ipFileSet->Fields, "ActionStatus");
						*pStatus = asEActionStatus(strStatus);
					}
				}
				else
				{
					// File ID did not exist
					UCLIDException ue("ELI30370", "File ID was not found.");
					ue.addDebugInfo ("File ID", nFileID);
					throw ue;
				}
			END_CONNECTION_RETRY(ipConnection, "ELI30371");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30640");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::SetStatusForAllFiles_Internal(bool bDBLocked, BSTR strAction,  EActionStatus eStatus)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				
				CSingleLock lock(&m_criticalSection, TRUE);

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Begin a transaction
				// The following code depends on the fact that m_strActiveWorkflow won't change in
				// the midst of this call; m_criticalSection guarantees that.
				TransactionGuard tg(ipConnection, adXactIsolated, &m_criticalSection);

				// Set the action name from the parameter
				string strActionName = asString(strAction);

				if (m_strActiveWorkflow.empty() && databaseUsingWorkflows(ipConnection))
				{
					ValueRestorer<string> restorer(m_strActiveWorkflow, "");

					vector<pair<string, string>> vecWorkflowNamesAndIDs = getWorkflowNamesAndIDs(ipConnection);

					for each (pair<string, string> strWorkflow in vecWorkflowNamesAndIDs)
					{
						m_strActiveWorkflow = strWorkflow.first;

						setStatusForAllFiles(ipConnection, strActionName, eStatus);
					}
				}
				else
				{
					setStatusForAllFiles(ipConnection, strActionName, eStatus);
				}

				// Commit the changes
				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI30376");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30642");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::SetStatusForFile_Internal(bool bDBLocked, long nID,  BSTR strAction,
												  long nWorkflowID, EActionStatus eStatus,  
												  VARIANT_BOOL vbQueueChangeIfProcessing,
												  VARIANT_BOOL vbAllowQueuedStatusOverride,
												  EActionStatus * poldStatus)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			*poldStatus = kActionUnattempted;

			BEGIN_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();

			// Ensure file gets added to current workflow if it is missing (setFileActionState)
			nWorkflowID = nWorkflowID == -1 ? getActiveWorkflowID(ipConnection) : nWorkflowID;

			// Begin a transaction
			TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_criticalSection);

			setStatusForFile(ipConnection, nID, asString(strAction), nWorkflowID, eStatus,
				asCppBool(vbQueueChangeIfProcessing), asCppBool(vbAllowQueuedStatusOverride),
				poldStatus);

			tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI23536");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30643");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetFilesToProcess_Internal(bool bDBLocked, const FilesToProcessRequest& request,
	IIUnknownVector** pvecFileRecords)
{
	try
	{
		try
		{
			// If the FAM has lost its registration, re-register before continuing with processing.
			ensureFAMRegistration();

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;
			BEGIN_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();

			// Make sure the DB Schema is the expected version
			validateDBSchemaVersion();

			// Perform all processing related to setting a file as processing.
			// The previous status of the files to process is expected to be either pending or
			// skipped.
			IIUnknownVectorPtr ipFiles = setFilesToProcessing(bDBLocked, ipConnection, request);
			*pvecFileRecords = ipFiles.Detach();

			END_CONNECTION_RETRY(ipConnection, "ELI51471");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30644");
	}
	catch (UCLIDException& ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetFileToProcess_Internal(bool bDBLocked, long nFileID, BSTR strAction, BSTR bstrFromState,
												  IFileRecord** ppFileRecord)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI37460", ppFileRecord != __nullptr);

			string strFromState = asString(bstrFromState);
			ASSERT_ARGUMENT("ELI47261", strFromState != "R");
		
			// Set the action name from the parameter
			string strActionName = asString(strAction);

			// If the FAM has lost its registration, re-register before continuing with processing.
			ensureFAMRegistration();

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();

			// Make sure the DB Schema is the expected version
			validateDBSchemaVersion();

			string strFileID = asString(nFileID);

			// All workflows is not allowed so this will be a singular ID
			string strActionID = getActionIDsForActiveWorkflow(ipConnection, strActionName);
			
			// Only need ot insert the U record if any from or from U
			if (strFromState.empty() || strFromState == "U")
			{
				// Unlike SelectFilesToProcess which will always be selecting files that already exist
				// in the FileActionStatus table, specific file IDs passed into this method should not
				// be assumed to exist in the table for the specified action. This query will insert
				// the row that can be updated by setFilesToProcessing. 
				string strInsertSQL =
					"INSERT INTO [FileActionStatus] ([FileID], [ActionID], [ActionStatus], [Priority]) "
					"SELECT <FileID>, <ActionID>, 'U', [FAMFile].[Priority] "
					"FROM FAMFile LEFT JOIN FileActionStatus ON FileActionStatus.FileID = FAMFile.ID "
					"	AND FileActionStatus.ActionID = <ActionID> "
					"WHERE [FAMFile].[ID] = <FileID> AND ActionStatus IS NULL";

				replaceVariable(strInsertSQL, "<FileID>", strFileID);
				replaceVariable(strInsertSQL, "<ActionID>", strActionID);

				executeCmdQuery(ipConnection, strInsertSQL);
			}

			// Select the required file info from the database based on the file ID and current action.
			string strSelectSQL =
				"SELECT FAMFile.ID, FileName, Pages, FileSize, ActionID, "
				"COALESCE(FileActionStatus.Priority, FAMFile.Priority) AS Priority, "
				"COALESCE(ActionStatus, 'U') AS ActionStatus "
				"FROM FAMFile LEFT JOIN FileActionStatus WITH (ROWLOCK, UPDLOCK, READPAST ) ON FileActionStatus.FileID = FAMFile.ID "
				"	AND FileActionStatus.ActionID = <ActionID> "
				"WHERE [FAMFile].[ID] = <FileID>";

			replaceVariable(strSelectSQL, "<FileID>", strFileID);
			replaceVariable(strSelectSQL, "<ActionID>", strActionID);
			if (!strFromState.empty())
			{
				strSelectSQL = strSelectSQL + " AND ActionStatus = '" + strFromState + "'";
			}

			// Perform all processing related to setting a file as processing.
			IIUnknownVectorPtr ipFiles = setFilesToProcessing(
				bDBLocked, ipConnection, strSelectSQL, strActionName, 1, "");

			if (ipFiles->Size() == 0)
			{
				// The file was not available in the database.
				*ppFileRecord = __nullptr;
			}
			else
			{
				// Return the loaded IFileRecord.
				UCLID_FILEPROCESSINGLib::IFileRecordPtr ipFileRecord(ipFiles->At(0));
				ASSERT_RESOURCE_ALLOCATION("ELI37461", ipFileRecord != __nullptr);

				*ppFileRecord = (IFileRecord *)ipFileRecord.Detach();
			}

			END_CONNECTION_RETRY(ipConnection, "ELI37462");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37463");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::RemoveFolder_Internal(bool bDBLocked, BSTR strFolder, BSTR strAction)
{
	try
	{
		try
		{
			map<string, _variant_t> params;

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();		

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Set the action name from the parameter
				string strActionName = asString(strAction);

				// Get the action ID and update the strActionName to stored value
				long nActionID = getActionID(ipConnection, strActionName);

				// set up the where clause to find the pending records that the filename begins with the folder name
				string strWhere = "WHERE (ActionStatus = 'P') AND (FileName LIKE @FolderName + '%')";
				string strFrom = "FROM FAMFile " + strWhere;
				params["@FolderName"] = strFolder;

				// Set up the SQL to delete the records in the FileActionStatus table 
				string strDeleteSQL = "DELETE FROM FileActionStatus"
					" WHERE ActionID = @ActionID AND FileID IN ("
					" SELECT FAMFile.ID FROM FileActionStatus RIGHT JOIN FAMFile "
					" ON FileActionStatus.FileID = FAMFile.ID " + 
					strWhere + ")";
				params["@ActionID"] = nActionID;

				// This method does not ever seem to get called, but in case it does, it seems reasonable
				// to ignore and pending changes for files in the folder.
				string strUpdateQueuedActionStatusChange =
					"UPDATE [QueuedActionStatusChange] SET [ChangeStatus] = 'I'"
					"WHERE [ChangeStatus] = 'P' AND [ActionID] = @ActionID "
					" AND [FileID] IN"
					"("
					"	SELECT FAMFile.ID FROM FileActionStatus RIGHT JOIN FAMFile "
					"	ON FileActionStatus.FileID = FAMFile.ID " + strWhere +
					")";

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_criticalSection);

				// add transition records to the database
				addASTransFromSelect(ipConnection, params, strActionName, nActionID, "U", "", "", strWhere, "");

				// Only update the QueueEvent table if update is enabled
				if (m_bUpdateQueueEventTable)
				{
					// Set up the SQL to add the queue event records
					string strInsertQueueRecords = "INSERT INTO QueueEvent (FileID, DateTimeStamp, QueueEventCode, FAMUserID, MachineID) ";

					// Add the Select query to get the records to insert 
					strInsertQueueRecords += "SELECT FAMFile.ID, GETDATE(), 'F', "
						+ asString(getFAMUserID(ipConnection)) + ", " + asString(getMachineID(ipConnection)) + " " + strFrom;

					// Add the QueueEvent records to the database
					executeCmdQuery(ipConnection, strInsertQueueRecords);
				}

				// This needs to be called before strDeleteSQL.
				executeCmdQuery(ipConnection, strUpdateQueuedActionStatusChange);

				// Remove the FileActionStatus records for the deleted folder
				executeCmdQuery(ipConnection, strDeleteSQL);

				// Commit the changes to the database
				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI30378");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30647");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetStatsAllWorkflows_Internal(bool bDBLocked, BSTR bstrActionName,
	VARIANT_BOOL vbForceUpdate, EWorkflowVisibility eWorkflowVisibility, IActionStatistics* *pStats)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();

			// Make sure the DB Schema is the expected version
			validateDBSchemaVersion();

			string strActionName = asString(bstrActionName);

			// Create a pointer to a recordset
			_RecordsetPtr ipActionSet(__uuidof(Recordset));
			ASSERT_RESOURCE_ALLOCATION("ELI42086", ipActionSet != __nullptr);

			long nWorkflowActionCount = 0;
			executeCmdQuery(ipConnection, 
				"SELECT COUNT(*) AS [ID] FROM [Action] WHERE [WorkflowID] IS NOT NULL",
				false, &nWorkflowActionCount);

			string strQuery = Util::Format("SELECT * FROM [Action] WHERE [ASCName] = '%s' "
				"AND [WorkFlowID] IS %s",
				strActionName.c_str(),
				(nWorkflowActionCount > 0) ? "NOT NULL" : "NULL");

			// Open the Action table
			ipActionSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenStatic,
				adLockReadOnly, adCmdText);

			UCLID_FILEPROCESSINGLib::IActionStatisticsPtr ipAggregateStats(CLSID_ActionStatistics);
			ASSERT_RESOURCE_ALLOCATION("ELI42087", ipAggregateStats != __nullptr);

			while (ipActionSet->adoEOF == VARIANT_FALSE)
			{
				FieldsPtr ipFields = ipActionSet->Fields;
				ASSERT_RESOURCE_ALLOCATION("ELI42088", ipFields != __nullptr);

				int nActionID = getLongField(ipFields, "ID");

				UCLID_FILEPROCESSINGLib::IActionStatisticsPtr ipActionStats =
					loadStats(ipConnection, nActionID, eWorkflowVisibility, asCppBool(vbForceUpdate), bDBLocked);
				ASSERT_RESOURCE_ALLOCATION("ELI42089", ipActionStats != __nullptr);

				ipAggregateStats->AddStatistics(ipActionStats);

				ipActionSet->MoveNext();
			}

			// Return the value
			*pStats = (IActionStatistics *)ipAggregateStats.Detach();
			
			END_CONNECTION_RETRY(ipConnection, "ELI42090");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI42091");
	}
	catch (UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetStats_Internal(bool bDBLocked, long nActionID,
	VARIANT_BOOL vbForceUpdate, VARIANT_BOOL vbRevertTimedOutFAMs, EWorkflowVisibility eWorkflowVisibility,
	IActionStatistics* *pStats)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// This flag is true when this method is called from the web app
				if (asCppBool(vbRevertTimedOutFAMs))
				{
					// Ping the DB every time so that a document being verified is not closed by another user's session
					// (Starting in 11.7, revertTimedOutProcessingFAMs can short-circuit before doing the ping)
					if (m_dwLastPingTime == 0 || (GetTickCount() - m_dwLastPingTime) > gnPING_TIMEOUT)
					{
						pingDB();
					}
					revertTimedOutProcessingFAMs(bDBLocked, ipConnection);
				}
				
				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				// return a new object with the statistics
				UCLID_FILEPROCESSINGLib::IActionStatisticsPtr ipActionStats =  
					loadStats(ipConnection, nActionID, eWorkflowVisibility, asCppBool(vbForceUpdate), bDBLocked);
				ASSERT_RESOURCE_ALLOCATION("ELI14107", ipActionStats != __nullptr);

				// Commit any changes (could have recreated the stats)
				tg.CommitTrans();

				// Return the value
				*pStats = (IActionStatistics *)ipActionStats.Detach();

			END_CONNECTION_RETRY(ipConnection, "ELI23539");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30648");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::CopyActionStatusFromAction_Internal(bool bDBLocked, long  nFromAction, long nToAction)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				string strFrom = getActionName(ipConnection, nFromAction);
				string strTo = getActionName(ipConnection, nToAction);

				TransactionGuard tg(ipConnection, adXactIsolated, &m_criticalSection);

				// Copy Action status and only update the FAST table if required
				copyActionStatus(ipConnection, strFrom, strTo, m_bUpdateFASTTable, nToAction);

				// update the stats for the to action
				reCalculateStats(ipConnection, nToAction);

				// Commit the transaction
				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI23540");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30649");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::RenameAction_Internal(bool bDBLocked, BSTR bstrOldActionName, BSTR bstrNewActionName)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Convert action names to string
				string strOld = asString(bstrOldActionName);
				string strNew = asString(bstrNewActionName);

				// Make sure processing is not active for this action
				assertProcessingNotActiveForAction(bDBLocked, ipConnection, strOld);

				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				// Change the name of the action in the action table
				string strSQL = "UPDATE [Action] SET [ASCName] = '" + strNew + "' WHERE [ASCName] = '" + strOld + "'";
				executeCmdQuery(ipConnection, strSQL);

				// Commit the transaction
				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI30379");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30650");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::Clear_Internal(bool bDBLocked, VARIANT_BOOL vbRetainUserValues)
{
	try
	{
		try
		{
			// Call the internal clear
			clear(bDBLocked, false, asCppBool(vbRetainUserValues));
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30651");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::ExportFileList_Internal(bool bDBLocked, BSTR strQuery, BSTR strOutputFileName,
	IRandomMathCondition* pRandomCondition, long *pnNumRecordsOutput)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI23522", pnNumRecordsOutput != __nullptr);

			// check for empty query string
			string strSQL = asString(strQuery);
			if (strSQL.empty())
			{
				UCLIDException ue("ELI14724", "Query string is empty.");
				throw ue;
			}
			// Check if output file name is not empty
			string strOutFileName = asString(strOutputFileName);
			if (strOutFileName.empty())
			{
				UCLIDException ue("ELI14727", "Output file name is blank.");
				throw ue;
			}

			// Ensure the output file name is fully qualified
			strOutFileName = buildAbsolutePath(strOutFileName);

			// Create the directory if needed
			createDirectory(getDirectoryFromFullPath(strOutFileName));

			// Wrap the random math condition in smart pointer
			UCLID_FILEPROCESSINGLib::IRandomMathConditionPtr ipRandomCondition(pRandomCondition);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Recordset to contain the files to process
				_RecordsetPtr ipFileSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI14725", ipFileSet != __nullptr);

				// get the recordset with the top nMaxFiles 
				ipFileSet->Open(strSQL.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenForwardOnly, 
					adLockReadOnly, adCmdText);

				// Open the output file
				ofstream ofsOutput(strOutFileName.c_str(), ios::out | ios::trunc);
				if (!ofsOutput.is_open())
				{
					UCLIDException ue("ELI34205", "Output file could not be opened.");
					ue.addDebugInfo("Filename", strOutFileName);
					ue.addWin32ErrorInfo();
					throw ue;
				}

				// Setup the counter for the number of records
				long nNumRecords = 0;

				// Create empty FileRecord object for the random check condition
				UCLID_FILEPROCESSINGLib::IFileRecordPtr ipFileRecord(CLSID_FileRecord);
				ASSERT_RESOURCE_ALLOCATION("ELI31346", ipFileRecord != __nullptr);

				ipFileRecord->Name = "";
				ipFileRecord->FileID = -1;

				// Fill the ipFiles collection
				while (ipFileSet->adoEOF == VARIANT_FALSE)
				{
					if (ipRandomCondition == __nullptr || ipRandomCondition->CheckCondition(ipFileRecord, 0) == VARIANT_TRUE)
					{
						// Get the FileName
						string strFile = getStringField(ipFileSet->Fields, "FileName");
						ofsOutput << strFile << endl;

						// increment the number of records
						nNumRecords++;
					}
					ipFileSet->MoveNext();
				}
				ofsOutput.flush();
				ofsOutput.close();
				waitForFileToBeReadable(strOutFileName);

				// return the number of records
				*pnNumRecordsOutput	= nNumRecords;

			END_CONNECTION_RETRY(ipConnection, "ELI23542");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30652");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetActionID_Internal(bool bDBLocked, BSTR bstrActionName, long* pnActionID)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI24027", pnActionID != __nullptr);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();


				// Get the action ID
				*pnActionID = getActionID(ipConnection, asString(bstrActionName));

			END_CONNECTION_RETRY(ipConnection, "ELI23544");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30654");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::SetDBInfoSetting_Internal(bool bDBLocked, BSTR bstrSettingName, BSTR bstrSettingValue, 
												 VARIANT_BOOL vbSetIfExists, VARIANT_BOOL vbRecordHistory)
{
	try
	{
		try
		{
			// Convert setting name and value to string 
			string strSettingName = asString(bstrSettingName);
			string strSettingValue = asString(bstrSettingValue);		

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				_CommandPtr queryCmd = buildCmd(ipConnection, gstrDBINFO_SETTING_QUERY,
					{ {gstrSETTING_NAME, bstrSettingName} });

				// Make sure the DB Schema is the expected version - actually DO NOT do this here,
				// as we are setting the DB Info NOW, so attempting to validate the schema can cause
				// an error here before the update is finished.
				//validateDBSchemaVersion();

				// Create a pointer to a recordset
				_RecordsetPtr ipDBInfoSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI19792", ipDBInfoSet != __nullptr);

				// Begin Transaction
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				// Open recordset for the DBInfo Settings
				ipDBInfoSet->Open((IDispatch *)queryCmd, vtMissing, adOpenDynamic,
					adLockOptimistic, adCmdText);

				// Check if setting record exists
				bool bExists = ipDBInfoSet->adoEOF == VARIANT_FALSE;

				// Continue if the setting is new or we are changing an existing setting
				if (!bExists || vbSetIfExists == VARIANT_TRUE)
				{
					long nUserId = getFAMUserID(ipConnection);
					long nMachineId = getMachineID(ipConnection);
					auto cmd = buildCmd(ipConnection, gstADD_UPDATE_DBINFO_SETTING,
						{
							{gstrSETTING_NAME.c_str(), bstrSettingName}
							,{gstrSETTING_VALUE.c_str(), bstrSettingValue}
							,{"@UserID", nUserId}
							,{"@MachineID", nMachineId}
							,{gstrSAVE_HISTORY.c_str(), (m_bStoreDBInfoChangeHistory) ? 1 : 0 }
						});
					executeCmd(cmd);
				}

				// Commit transaction
				tg.CommitTrans();

				// While m_ipDBInfoSettings to null is enough to ensure settings accessed via GetDBInfoSetting
				// get's correct values via a lazy call to loadDBInfoSettings, there are class fields that won't
				// be updated if we don't force a re-load here.
				m_ipDBInfoSettings = __nullptr;
				loadDBInfoSettings(ipConnection);

			END_CONNECTION_RETRY(ipConnection, "ELI27328");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30656");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetDBInfoSetting_Internal(bool bDBLocked, const string& strSettingName,
	bool bThrowIfMissing, string& rstrSettingValue)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Get the setting
				rstrSettingValue = getDBInfoSetting(ipConnection, strSettingName,
					bThrowIfMissing);

			END_CONNECTION_RETRY(ipConnection, "ELI27327");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30658");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetResultsForQuery_Internal(bool bDBLocked, BSTR bstrQuery, _Recordset** ppVal)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI31522", ppVal != __nullptr);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Create a pointer to a recordset
				_RecordsetPtr ipResultSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI19876", ipResultSet != __nullptr);

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Open the Action table
				ipResultSet->Open(bstrQuery, _variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
					adLockReadOnly, adCmdText);

				*ppVal = ipResultSet.Detach();

			END_CONNECTION_RETRY(ipConnection, "ELI23547");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30716");
	}
	catch(UCLIDException ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetFileID_Internal(bool bDBLocked, BSTR bstrFileName, long *pnFileID)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI24028", pnFileID != __nullptr);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Get the file ID
				*pnFileID = getFileID(ipConnection, asString(bstrFileName));

			END_CONNECTION_RETRY(ipConnection, "ELI24029");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30657");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetActionName_Internal(bool bDBLocked, long nActionID, BSTR *pbstrActionName)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI26769", pbstrActionName != __nullptr);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Get the action name from the database
				string strActionName = getActionName(ipConnection, nActionID);

				// Return the action name
				*pbstrActionName = _bstr_t(strActionName.c_str()).Detach();

			END_CONNECTION_RETRY(ipConnection, "ELI26770");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30659");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::NotifyFileSkipped_Internal(bool bDBLocked, long nFileID, BSTR bstrAction,
												   long nWorkflowID,
												   VARIANT_BOOL vbAllowQueuedStatusOverride)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();
				
				auto role = getAppRoleConnection();

				ipConnection = role->ADOConnection();

				// Ensure file gets added to current workflow if it is missing (setFileActionState)
				nWorkflowID = nWorkflowID == -1 ? getActiveWorkflowID(ipConnection) : nWorkflowID;

				// Get the action name
				string strActionName = asString(bstrAction);

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_criticalSection);

				// Set the file state to skipped unless there is a pending state in the
				// QueuedActionStatusChange table.
				setFileActionState(ipConnection, nFileID, strActionName, nWorkflowID, "S", "", false,
					asCppBool(vbAllowQueuedStatusOverride));

				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI26778");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30660");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::SetFileActionComment_Internal(bool bDBLocked, long nFileID, long nActionID,
	BSTR bstrComment)
{
	try
	{
		try
		{
			// Get the comment
			string strComment = asString(bstrComment);

			// Get the current user name
			string strUserName = (m_strFAMUserName.empty()) ? getCurrentUserName() : m_strFAMUserName;

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				validateDBSchemaVersion();

				string strCommentSQL = "SELECT * FROM FileActionComment WHERE FileID = "
					+ asString(nFileID) + " AND ActionID = " + asString(nActionID);

				_RecordsetPtr ipCommentSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI26788", ipCommentSet != __nullptr);

				ipCommentSet->Open(strCommentSQL.c_str(), _variant_t((IDispatch*)ipConnection, true), adOpenDynamic,
					adLockOptimistic, adCmdText);

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				// If no records returned then there is no comment for this pair currently
				// add the new comment to the table (do not add empty comments)
				if (ipCommentSet->BOF == VARIANT_TRUE)
				{
					if (!strComment.empty())
					{
						ipCommentSet->AddNew();

						// Get the fields pointer
						FieldsPtr ipFields = ipCommentSet->Fields;
						ASSERT_RESOURCE_ALLOCATION("ELI26789", ipFields != __nullptr);

						// Set the fields from the provided data
						setStringField(ipFields, "UserName", strUserName);
						setLongField(ipFields, "FileID", nFileID);
						setLongField(ipFields, "ActionID", nActionID);
						setStringField(ipFields, "Comment", strComment);
						setStringField(ipFields, "DateTimeStamp", getSQLServerDateTime(ipConnection));

						ipCommentSet->Update();
					}
				}
				// Record already exists, update the comment , date time stamp, and user name
				// (if comment is empty, delete the row)
				else
				{
					if (strComment.empty())
					{
						ipCommentSet->Delete(adAffectCurrent);
					}
					else
					{
						FieldsPtr ipFields = ipCommentSet->Fields;
						ASSERT_RESOURCE_ALLOCATION("ELI26790", ipFields != __nullptr);

						setStringField(ipFields, "UserName", strUserName);
						setStringField(ipFields, "Comment", strComment);
						setStringField(ipFields, "DateTimeStamp", getSQLServerDateTime(ipConnection));
					}

					// Update the table
					ipCommentSet->Update();
				}

				// Commit the transaction
				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI26772");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30661");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetFileActionComment_Internal(bool bDBLocked, long nFileID, long nActionID,
													 BSTR* pbstrComment)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI26792", pbstrComment != __nullptr);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			// Default the comment to empty string
			string strComment = "";

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				validateDBSchemaVersion();

				string strCommentSQL = "SELECT * FROM FileActionComment WHERE FileID = "
					+ asString(nFileID) + " AND ActionID = " + asString(nActionID);

				_RecordsetPtr ipCommentSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI26793", ipCommentSet != __nullptr);

				ipCommentSet->Open(strCommentSQL.c_str(), _variant_t((IDispatch*)ipConnection, true), adOpenDynamic,
					adLockOptimistic, adCmdText);

				// Only need to get the comment if one exists
				if (ipCommentSet->BOF == VARIANT_FALSE)
				{
					strComment = getStringField(ipCommentSet->Fields, "Comment");
				}

			END_CONNECTION_RETRY(ipConnection, "ELI26774");

			// Set the return value
			*pbstrComment = _bstr_t(strComment.c_str()).Detach();
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30665");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::ClearFileActionComment_Internal(bool bDBLocked, long nFileID, long nActionID)
{
	try
	{
		try
		{

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				validateDBSchemaVersion();

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				// Clear the comment
				clearFileActionComment(ipConnection, nFileID, nActionID);

				// Commit the transaction
				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI26776");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30666");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::ModifyActionStatusForSelection_Internal(bool bDBLocked, 
															IFAMFileSelector* pFileSelector,
															BSTR bstrToAction, EActionStatus eaStatus, 
															BSTR bstrFromAction, 
															VARIANT_BOOL vbModifyWhenTargetActionMissingForSomeFiles,
															long* pnNumRecordsModified)
{
	string strTempWorkflow;
	bool bMissingActionsInWorkflows = false;

	try
	{
		try
		{
			// Check that an action name and a FROM clause have been passed in
			UCLID_FILEPROCESSINGLib::IFAMFileSelectorPtr ipFileSelector(pFileSelector);
			ASSERT_ARGUMENT("ELI30380", ipFileSelector != __nullptr);
			string strToAction = asString(bstrToAction);
			ASSERT_ARGUMENT("ELI30381", !strToAction.empty());

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();
				
				validateDBSchemaVersion();

				// Begin a transaction
				// The following code depends on the fact that m_strActiveWorkflow won't change in
				// the midst of this call; m_criticalSection guarantees that
				TransactionGuard tg(ipConnection, adXactIsolated, &m_criticalSection);

				long nNumRecordsModified = 0;
				string strFromAction = asString(bstrFromAction);
				string strStatus = "";
				if (strFromAction.empty())
				{
					strStatus = asStatusString(eaStatus);
				}

				string strOriginalWorkflow = getActiveWorkflow();
				vector<UCLIDException> vecMissingActionUEX;

				if (strOriginalWorkflow.empty() && databaseUsingWorkflows(ipConnection))
				{
					try
					{
						vector<pair<string, string>> vecWorkflowNamesAndIDs = getWorkflowNamesAndIDs(ipConnection);

						for each (pair<string, string> strWorkflow in vecWorkflowNamesAndIDs)
						{
							// modifyActionStatusForSelection will run in the context of m_strActiveWorkflow.
							// Repeat the call for each workflow.
							setActiveWorkflow(strWorkflow.first);
							strTempWorkflow = strWorkflow.first;

							try
							{
								modifyActionStatusForSelection(ipFileSelector, strToAction, strStatus,
									strFromAction, &nNumRecordsModified);
								strTempWorkflow = "";
							}
							catch (UCLIDException& ue)
							{
								if (ue.getTopELI() == "ELI51514")
								{
									if (!asCppBool(vbModifyWhenTargetActionMissingForSomeFiles))
									{
										vecMissingActionUEX.push_back(ue);
									}
								}
								else
								{
									throw ue;
								}
							}
						}

						setActiveWorkflow(strOriginalWorkflow);
					}
					catch (...)
					{
						setActiveWorkflow(strOriginalWorkflow);

						throw;
					}
				}
				else
				{
					modifyActionStatusForSelection(ipFileSelector, strToAction, strStatus,
						strFromAction, &nNumRecordsModified);
				}

				if (vecMissingActionUEX.size() > 0)
				{
					unique_ptr<UCLIDException> upuexMissingActions = __nullptr;

					for each (auto ue in vecMissingActionUEX)
					{
						upuexMissingActions.reset((upuexMissingActions.get() == __nullptr)
							? new UCLIDException(ue)
							: new UCLIDException(ue.getTopELI(), ue.getTopText(), *upuexMissingActions));
					}

					// WARNING: This ELI code is referenced by CSetActionStatusDlg.applyActionStatusChanges. Do not change.
					UCLIDException uexMissingActions("ELI51515",
						Util::Format("The status of some files could not be set because action \"%s\" does not exist in target workflow.",
							strToAction.c_str()), *upuexMissingActions.get());
					uexMissingActions.addDebugInfo("Number able to set", nNumRecordsModified, false);

					bMissingActionsInWorkflows = true;
					throw uexMissingActions;
				}

				// Commit the transaction
				tg.CommitTrans();

				// Set the return value if it is specified
				if (pnNumRecordsModified != __nullptr)
				{
					*pnNumRecordsModified = nNumRecordsModified;
				}

			END_CONNECTION_RETRY(ipConnection, "ELI30384");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30669");
	}
	catch(UCLIDException &ue)
	{
		if (!strTempWorkflow.empty())
		{
			ue.addDebugInfo("Workflow", strTempWorkflow, false);
		}

		if (!bDBLocked && !bMissingActionsInWorkflows)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetTags_Internal(bool bDBLocked, IStrToStrMap **ppTags)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI27329", ppTags != __nullptr);

			// Create a map to hold the return values
			IStrToStrMapPtr ipTagToDesc(CLSID_StrToStrMap);
			ASSERT_RESOURCE_ALLOCATION("ELI27330", ipTagToDesc != __nullptr);

			ipTagToDesc->CaseSensitive = VARIANT_FALSE;

			// Create query to get the tags and descriptions
			string strQuery = "SELECT [TagName], [TagDescription] FROM [Tag]";

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				validateDBSchemaVersion();

				// Create a pointer to a recordset
				_RecordsetPtr ipTagSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI27331", ipTagSet != __nullptr);

				// Open Recordset that contains all the tags and their descriptions
				ipTagSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true),
					adOpenForwardOnly, adLockReadOnly, adCmdText);

				// Add each tag and description to the map
				while (ipTagSet->adoEOF == VARIANT_FALSE)
				{
					FieldsPtr ipFields = ipTagSet->Fields;
					ASSERT_RESOURCE_ALLOCATION("ELI27332", ipFields != __nullptr);

					// Get the tag and description
					string strTagName = getStringField(ipFields, "TagName");
					string strDesc = getStringField(ipFields, "TagDescription");

					// Add the tag and description to the map
					ipTagToDesc->Set(strTagName.c_str(), strDesc.c_str());

					// Move to the next record
					ipTagSet->MoveNext();
				}

			END_CONNECTION_RETRY(ipConnection, "ELI27333");

			// Set the out value
			*ppTags = ipTagToDesc.Detach();
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30668");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetTagNames_Internal(bool bDBLocked, IVariantVector **ppTagNames)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI27335", ppTagNames != __nullptr);

			IVariantVectorPtr ipVecTags(CLSID_VariantVector);
			ASSERT_RESOURCE_ALLOCATION("ELI27336", ipVecTags != __nullptr);

			string strQuery = "SELECT [TagName] FROM [Tag]";

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				validateDBSchemaVersion();

				// Create a pointer to a recordset
				_RecordsetPtr ipTagSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI27337", ipTagSet != __nullptr);

				// Open Recordset that contains the tag names
				ipTagSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true),
					adOpenForwardOnly, adLockReadOnly, adCmdText);

				// Loop through each tag name and add it to the variant vector
				while (ipTagSet->adoEOF == VARIANT_FALSE)
				{
					// Get the tag and add it to the collection
					string strTagName = getStringField(ipTagSet->Fields, "TagName");
					ipVecTags->PushBack(strTagName.c_str());

					// Move to the next tag
					ipTagSet->MoveNext();
				}

			END_CONNECTION_RETRY(ipConnection, "ELI27338");

			// Set the out value
			*ppTagNames = ipVecTags.Detach();
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30667");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::HasTags_Internal(bool bDBLocked, VARIANT_BOOL* pvbVal)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI27340", pvbVal != __nullptr);

			bool bHasTags = false;

			string strQuery = "SELECT TOP 1 [TagName] FROM [Tag]";

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				validateDBSchemaVersion();

				// Create a pointer to a recordset
				_RecordsetPtr ipTagSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI27341", ipTagSet != __nullptr);

				// Open Recordset that contains the tag names
				ipTagSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true),
					adOpenForwardOnly, adLockReadOnly, adCmdText);

				// Check if there is at least 1 tag
				bHasTags = ipTagSet->adoEOF == VARIANT_FALSE;

			END_CONNECTION_RETRY(ipConnection, "ELI27342");

			*pvbVal = asVariantBool(bHasTags);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30670");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::TagFile_Internal(bool bDBLocked, long nFileID, BSTR bstrTagName)
{
	try
	{
		try
		{
			string strTagName = asString(bstrTagName);
			validateTagName(strTagName);

			// Get the query for adding the tag, update with appropriate file ID
			string strQuery = gstrTAG_FILE_QUERY;
			replaceVariable(strQuery, gstrTAG_FILE_ID_VAR, asString(nFileID));

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				validateDBSchemaVersion();

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				// Get the tag ID (this will also validate the ID)
				long nTagID = getTagID(ipConnection, strTagName);

				// Update the query with the tag ID
				replaceVariable(strQuery, gstrTAG_ID_VAR, asString(nTagID));

				if (m_bStoreDocTagHistory)
				{
					// Update history
					replaceVariable(strQuery, gstrUPDATE_DOC_TAG_HISTORY_VAR, "1");

					// Insert user/machine ID for doc history table
					replaceVariable(strQuery, gstrUSER_ID_VAR, asString(getFAMUserID(ipConnection)));
					replaceVariable(strQuery, gstrMACHINE_ID_VAR, asString(getMachineID(ipConnection)));
				}
				else
				{
					// Do not update history
					replaceVariable(strQuery, gstrUPDATE_DOC_TAG_HISTORY_VAR, "0");

					// User/machine ID are not needed.
					replaceVariable(strQuery, gstrUSER_ID_VAR, "0");
					replaceVariable(strQuery, gstrMACHINE_ID_VAR, "0");
				}

				// Execute the query to add the tag
				executeCmdQuery(ipConnection, strQuery);

				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI27346");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30672");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::UntagFile_Internal(bool bDBLocked, long nFileID, BSTR bstrTagName)
{
	try
	{
		try
		{
			string strTagName = asString(bstrTagName);
			validateTagName(strTagName);

			// Get the query for removing the tag, update with appropriate file ID
			string strQuery = gstrUNTAG_FILE_QUERY;
			replaceVariable(strQuery, gstrTAG_FILE_ID_VAR, asString(nFileID));

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				validateDBSchemaVersion();

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				long nTagID = 0;

				try
				{
					// Get the tag ID (this will also validate the ID)
					nTagID = getTagID(ipConnection, strTagName);
				}
				catch(...)
				{
					// If allowing dynamic tag creation, just return true at this point
					// (an exception indicates the tag doesn't exist)
					if(m_bAllowDynamicTagCreation)
					{
						return true;
					}
					else
					{
						throw;
					}
				}

				// Update the query with the tag ID
				replaceVariable(strQuery, gstrTAG_ID_VAR, asString(nTagID));

				if (m_bStoreDocTagHistory)
				{
					// Update history
					replaceVariable(strQuery, gstrUPDATE_DOC_TAG_HISTORY_VAR, "1");

					// Insert user/machine ID for doc history table
					replaceVariable(strQuery, gstrUSER_ID_VAR, asString(getFAMUserID(ipConnection)));
					replaceVariable(strQuery, gstrMACHINE_ID_VAR, asString(getMachineID(ipConnection)));
				}
				else
				{
					// Do not update history
					replaceVariable(strQuery, gstrUPDATE_DOC_TAG_HISTORY_VAR, "0");

					// User/machine ID are not needed.
					replaceVariable(strQuery, gstrUSER_ID_VAR, "0");
					replaceVariable(strQuery, gstrMACHINE_ID_VAR, "0");
				}

				// Execute the query to remove the tag
				executeCmdQuery(ipConnection, strQuery);

				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI27349");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30671");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::ToggleTagOnFile_Internal(bool bDBLocked, long nFileID, BSTR bstrTagName)
{
	try
	{
		try
		{
			string strTagName = asString(bstrTagName);
			validateTagName(strTagName);

			// Get the query for toggling the tag, update with appropriate file ID
			string strQuery = gstrTOGGLE_TAG_FOR_FILE_QUERY;
			replaceVariable(strQuery, gstrTAG_FILE_ID_VAR, asString(nFileID));

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				validateDBSchemaVersion();

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				// Get the tag ID (this will also validate the ID)
				long nTagID = getTagID(ipConnection, strTagName);

				// Update the query with the tag ID
				replaceVariable(strQuery, gstrTAG_ID_VAR, asString(nTagID));

				if (m_bStoreDocTagHistory)
				{
					// Update history
					replaceVariable(strQuery, gstrUPDATE_DOC_TAG_HISTORY_VAR, "1");

					// Insert user/machine ID for doc history table
					replaceVariable(strQuery, gstrUSER_ID_VAR, asString(getFAMUserID(ipConnection)));
					replaceVariable(strQuery, gstrMACHINE_ID_VAR, asString(getMachineID(ipConnection)));
				}
				else
				{
					// Do not update history
					replaceVariable(strQuery, gstrUPDATE_DOC_TAG_HISTORY_VAR, "0");

					// User/machine ID are not needed.
					replaceVariable(strQuery, gstrUSER_ID_VAR, "0");
					replaceVariable(strQuery, gstrMACHINE_ID_VAR, "0");
				}

				// Execute the query to toggle the tag
				executeCmdQuery(ipConnection, strQuery);

				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI27353");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30673");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::AddTag_Internal(bool bDBLocked, const string& strTagName,
	const string& strDescription, bool bFailIfExists)
{
	try
	{
		try
		{
			string strQuery = "SELECT [TagName], [TagDescription] FROM [Tag] WHERE [TagName] = '"
				+ strTagName + "'";

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				validateDBSchemaVersion();

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				// Create a pointer to a recordset
				_RecordsetPtr ipTagSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI27355", ipTagSet != __nullptr);

				// Open Recordset that contains the tag names
				ipTagSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenDynamic, 
					adLockOptimistic, adCmdText);

				if (ipTagSet->adoEOF == VARIANT_FALSE)
				{
					if (!bFailIfExists)
					{
						return true;
					}

					string strCurrentDescription = getStringField(ipTagSet->Fields, "TagDescription");

					UCLIDException ue("ELI27356", "Specified tag already exists!");
					ue.addDebugInfo("Tag Name", strTagName);
					ue.addDebugInfo("Current Description", strCurrentDescription);
					ue.addDebugInfo("New Description", strDescription);
					throw ue;
				}
				else
				{
					ipTagSet->AddNew();

					// Get the fields
					FieldsPtr ipFields = ipTagSet->Fields;
					ASSERT_RESOURCE_ALLOCATION("ELI27357", ipFields != __nullptr);

					// Set the fields
					setStringField(ipFields, "TagName", strTagName);
					setStringField(ipFields, "TagDescription", strDescription);

					// Update the table
					ipTagSet->Update();
				}

				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI27358");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30674");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::DeleteTag_Internal(bool bDBLocked, BSTR bstrTagName)
{
	try
	{
		try
		{
			// Get the tag name
			string strTagName = asString(bstrTagName);
			validateTagName(strTagName);

			// Build the query
			string strQuery = "SELECT * FROM [Tag] WHERE [TagName] = '" + strTagName + "'";


			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				validateDBSchemaVersion();

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				// Create a pointer to a recordset
				_RecordsetPtr ipTagSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI27418", ipTagSet != __nullptr);

				// Open Recordset that contains the tag names
				ipTagSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenDynamic, 
					adLockOptimistic, adCmdText);

				if (ipTagSet->adoEOF == VARIANT_FALSE)
				{
					// Delete the current record
					ipTagSet->Delete(adAffectCurrent);

					// Update the table
					ipTagSet->Update();
				}

				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI27365");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30675");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::ModifyTag_Internal(bool bDBLocked, BSTR bstrOldTagName, BSTR bstrNewTagName,
										  BSTR bstrNewTagDescription)
{
	try
	{
		try
		{
			// Get the old tag name and validate it
			string strOldTagName = asString(bstrOldTagName);
			validateTagName(strOldTagName);

			// Get the new tag name and description
			string strNewTagName = asString(bstrNewTagName);
			string strNewDescription = asString(bstrNewTagDescription);

			// If new tag name is not empty, validate it
			if (!strNewTagName.empty())
			{
				validateTagName(strNewTagName);
			}

			// Check the description length
			if (strNewDescription.length() > 255)
			{
				UCLIDException ue("ELI29350", "Description is longer than 255 characters.");
				ue.addDebugInfo("Description", strNewDescription);
				ue.addDebugInfo("Description Length", strNewDescription.length());
				throw ue;
			}
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				validateDBSchemaVersion();

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				string strQueryBase = "SELECT [TagName], [TagDescription] FROM [Tag] WHERE [TagName] = '";

				// If specifying new tag name, check for new tag name existence
				// [LRCAU #5693] - Only check existence if the tag name is different
				if (!strNewTagName.empty() && !stringCSIS::sEqual(strOldTagName, strNewTagName))
				{
					string strTempQuery = strQueryBase + strNewTagName + "'";
					_RecordsetPtr ipTemp(__uuidof(Recordset));
					ASSERT_RESOURCE_ALLOCATION("ELI29225", ipTemp != __nullptr);

					ipTemp->Open(strTempQuery.c_str(), _variant_t((IDispatch*) ipConnection, true),
						adOpenDynamic, adLockOptimistic, adCmdText);
					if (ipTemp->adoEOF == VARIANT_FALSE)
					{
						UCLIDException ue("ELI29226", "New tag name already exists.");
						ue.addDebugInfo("Old Tag Name", strOldTagName);
						ue.addDebugInfo("New Tag Name", strNewTagName);
						ue.addDebugInfo("New Description", strNewDescription);
						throw ue;
					}
				}

				string strQuery = strQueryBase + strOldTagName + "'";

				// Create a pointer to a recordset
				_RecordsetPtr ipTagSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI27362", ipTagSet != __nullptr);

				// Open Recordset that contains the tag names
				ipTagSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenDynamic, 
					adLockOptimistic, adCmdText);

				// Ensure there is a record for the old tag name
				if (ipTagSet->adoEOF == VARIANT_TRUE)
				{
					UCLIDException ue("ELI27363", "The tag specified does not exist!");
					ue.addDebugInfo("Tag Name", strOldTagName);
					ue.addDebugInfo("New Tag Name", strNewTagName);
					ue.addDebugInfo("New Description", strNewDescription);
					throw ue;
				}

				// Get the fields pointer
				FieldsPtr ipFields = ipTagSet->Fields;
				ASSERT_RESOURCE_ALLOCATION("ELI27364", ipFields != __nullptr);

				// Update the record with the new values
				if (!strNewTagName.empty())
				{
					setStringField(ipFields, "TagName", strNewTagName);
				}

				// Update the description even if it is empty (allows clearing description)
				setStringField(ipFields, "TagDescription", strNewDescription);

				ipTagSet->Update();

				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI27419");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30680");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetFilesWithTags_Internal(bool bDBLocked, IVariantVector* pvecTagNames,
												 VARIANT_BOOL vbAndOperation,
												 IVariantVector** ppvecFileIDs)
{
	try
	{
		try
		{
			// Check arguments
			IVariantVectorPtr ipVecTagNames(pvecTagNames);
			ASSERT_ARGUMENT("ELI27367", ipVecTagNames != __nullptr);
			ASSERT_ARGUMENT("ELI27368", ppvecFileIDs != __nullptr);

			// Create the vector to return the file IDs
			IVariantVectorPtr ipVecFileIDs(CLSID_VariantVector);
			ASSERT_RESOURCE_ALLOCATION("ELI27369", ipVecFileIDs != __nullptr);

			// Get the size of the vector of tag names
			long lSize = ipVecTagNames->Size;

			// If no tags specified return empty collection
			if (lSize == 0)
			{
				// Set the return value
				*ppvecFileIDs = ipVecFileIDs.Detach();

				return true;
			}

			string strConjunction = asCppBool(vbAndOperation) ? "\nINTERSECT\n" : "\nUNION\n";

			// Get the main sql string
			string strMainQuery = "SELECT [FileTag].[FileID] FROM [FileTag] INNER JOIN [Tag] ON "
				"[FileTag].[TagID] = [Tag].[ID] WHERE [Tag].[TagName] = '";
			strMainQuery += gstrTAG_NAME_VALUE + "'";

			// Build the sql string
			string strQuery = strMainQuery;
			replaceVariable(strQuery, gstrTAG_NAME_VALUE, asString(ipVecTagNames->GetItem(0).bstrVal));

			for (long i=1; i < lSize; i++)
			{
				string strTagName = asString(ipVecTagNames->GetItem(i).bstrVal);
				if (!strTagName.empty())
				{
					string strTemp = strMainQuery;
					replaceVariable(strTemp, gstrTAG_NAME_VALUE,
						asString(ipVecTagNames->GetItem(i).bstrVal));
					strQuery += strConjunction + strTemp;
				}
			}

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				validateDBSchemaVersion();

				// Create a pointer to a recordset
				_RecordsetPtr ipTagSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI27370", ipTagSet != __nullptr);

				// Open Recordset that contains the file IDs
				ipTagSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true),
					adOpenForwardOnly, adLockReadOnly, adCmdText);

				// Loop through each file ID and add it to the variant vector
				while (ipTagSet->adoEOF == VARIANT_FALSE)
				{
					// Get the file ID
					_variant_t vtID(getLongField(ipTagSet->Fields, "FileID"));

					// Add the file ID to the collection
					ipVecFileIDs->PushBack(vtID);

					// Move to the next file ID
					ipTagSet->MoveNext();
				}

			END_CONNECTION_RETRY(ipConnection, "ELI27371");

			// Set the out value
			*ppvecFileIDs = ipVecFileIDs.Detach();
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30681");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetTagsOnFile_Internal(bool bDBLocked, long nFileID, IVariantVector** ppvecTagNames)
{
	try
	{
		try
		{
			// Check argument
			ASSERT_ARGUMENT("ELI27373", ppvecTagNames != __nullptr);

			// Build the sql string
			string strQuery = "SELECT DISTINCT [Tag].[TagName] FROM [FileTag] INNER JOIN "
				"[Tag] ON [FileTag].[TagID] = [Tag].[ID] WHERE [FileTag].[FileID] = ";
			strQuery += asString(nFileID) + " ORDER BY [Tag].[TagName]";

			// Create the vector to return the tag names
			IVariantVectorPtr ipVecTagNames(CLSID_VariantVector);
			ASSERT_RESOURCE_ALLOCATION("ELI27374", ipVecTagNames != __nullptr);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				validateDBSchemaVersion();

				// Create a pointer to a recordset
				_RecordsetPtr ipTagSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI27375", ipTagSet != __nullptr);

				// Open Recordset that contains the file IDs
				ipTagSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true),
					adOpenForwardOnly, adLockReadOnly, adCmdText);

				// Loop through each tag name and add it to the vector
				while (ipTagSet->adoEOF == VARIANT_FALSE)
				{
					// Get the tag name
					_variant_t vtTagName(getStringField(ipTagSet->Fields, "TagName").c_str());

					// Add it to the vector
					ipVecTagNames->PushBack(vtTagName);

					// Move to the next tag name
					ipTagSet->MoveNext();
				}

			END_CONNECTION_RETRY(ipConnection, "ELI27376");

			// Set the out value
			*ppvecTagNames = ipVecTagNames.Detach();
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30682");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::ExecuteCommandQuery_Internal(bool bDBLocked, BSTR bstrQuery, long* pnRecordsAffected)
{
	try
	{
		try
		{
			// Get the query string and ensure it is not empty
			string strQuery = asString(bstrQuery);
			ASSERT_ARGUMENT("ELI27684", !strQuery.empty());

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Set the transaction guard
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				// Execute the query
				long nRecordsAffected = executeCmdQuery(ipConnection, strQuery);

				// If user wants a count of affected records, return it
				if (pnRecordsAffected != __nullptr)
				{
					*pnRecordsAffected = nRecordsAffected;
				}

				// Commit the transaction
				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI27685");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30685");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool 
CFileProcessingDB::
ExecuteCommandReturnLongLongResult_Internal( bool bDBLocked, 
											 BSTR bstrQuery, 
											 long* pnRecordsAffected,
											 BSTR bstrResultColumnName,
											 long long* pResult )
{
	try
	{
		try
		{
			// Get the query string and ensure it is not empty
			string strQuery = asString(bstrQuery);
			ASSERT_ARGUMENT("ELI38678", !strQuery.empty());

			std::string resultColumnName = 
				nullptr != bstrResultColumnName ? asString(bstrResultColumnName) : "";

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Set the transaction guard
				TransactionGuard tg(ipConnection, adXactChaos, nullptr);

				// Execute the query
				const bool displayExceptions = false;
				long nRecordsAffected = executeCmdQuery( ipConnection, 
														 strQuery,
														 resultColumnName,
														 displayExceptions,
														 pResult );

				// If user wants a count of affected records, return it
				if (pnRecordsAffected != nullptr)
				{
					*pnRecordsAffected = nRecordsAffected;
				}

				// Commit the transaction
				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI38679");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38680");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}

		throw ue;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::UnregisterActiveFAM_Internal(bool bDBLocked)
{
	try
	{
		try
		{
			if (!m_bCurrentSessionIsWebSession)
			{
				// Stop thread here
				m_eventStopMaintenanceThreads.signal();

				// Wait for the ping and statistics maintenance threads to exit.
				HANDLE handles[2];
				handles[0] = m_eventPingThreadExited.getHandle();
				handles[1] = m_eventStatsThreadExited.getHandle();
				if (WaitForMultipleObjects(2, (HANDLE *)&handles, TRUE, gnPING_TIMEOUT) != WAIT_OBJECT_0)
				{
					UCLIDException ue("ELI27857", "Application Trace: Timed out waiting for thread to exit.");
					ue.log();
				}
			}
			
			// set FAMRegistered flag to false since thread has exited
			m_bFAMRegistered = false;
			
			auto role = getAppRoleConnection();
			
			// Set the transaction guard
			TransactionGuard tg(role->ADOConnection(), adXactRepeatableRead, &m_criticalSection);

			// Make sure there are no linked records in the LockedFile table 
			// and if there are records reset their status to StatusBeforeLock if their current
			// state for the action is processing.
			UCLIDException uex("ELI30304", "Application Trace: Files were reverted to original status.");
			revertLockedFilesToPreviousState(role->ADOConnection(), m_nActiveFAMID,
				"Processing FAM is exiting.", &uex);

			// Reset m_nActiveFAMID to 0 to specify that it is not registered.
			m_nActiveFAMID = 0;

			// Commit the transaction
			tg.CommitTrans();
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30708");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::SetPriorityForFiles_Internal(bool bDBLocked, BSTR bstrSelectQuery, EFilePriority eNewPriority,
													IRandomMathCondition *pRandomCondition,
													long *pnNumRecordsModified)
{
	try
	{
		try
		{
			// Get the query string and ensure it is not empty
			string strQuery = asString(bstrSelectQuery);
			ASSERT_ARGUMENT("ELI27710", !strQuery.empty());

			// Wrap the random condition (if there is one, in a smart pointer)
			UCLID_FILEPROCESSINGLib::IRandomMathConditionPtr ipRandomCondition(pRandomCondition);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Set the transaction guard
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				// Recordset to search for file IDs
				_RecordsetPtr ipFileSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI27711", ipFileSet != __nullptr);

				// Get the recordset for the specified select query
				ipFileSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenForwardOnly, 
					adLockReadOnly, adCmdText);

				// Create empty FileRecord object for the random check condition
				UCLID_FILEPROCESSINGLib::IFileRecordPtr ipFileRecord(CLSID_FileRecord);
				ASSERT_RESOURCE_ALLOCATION("ELI31345", ipFileRecord != __nullptr);

				ipFileRecord->Name = "";
				ipFileRecord->FileID = 0;

				// Build a list of file ID's to set
				stack<string> stackIDs;
				while (ipFileSet->adoEOF == VARIANT_FALSE)
				{
					if (ipRandomCondition == __nullptr || ipRandomCondition->CheckCondition(ipFileRecord, 0) == VARIANT_TRUE)
					{
						// Get the file ID
						stackIDs.push(asString(getLongField(ipFileSet->Fields, "ID")));
					}

					ipFileSet->MoveNext();
				}

				// Store the count of records that will be modified
				long nNumRecords = stackIDs.size();

				// Loop through the IDs setting the file priority
				string strPriority = asString(eNewPriority == kPriorityDefault ? 
					glDEFAULT_FILE_PRIORITY : (long)eNewPriority);
				while(!stackIDs.empty())
				{
					vector<string> vecQueries;
					
					string strIDList("(" + stackIDs.top());
					stackIDs.pop();
					for (int i = 0; !stackIDs.empty() && i < 150; i++)
					{
						strIDList += ", " + stackIDs.top();
						stackIDs.pop();
					}
					strIDList += ")";

					// Build the queries to update both FAMFile and FileActionStatus
					vecQueries.push_back("UPDATE [FAMFile] SET [Priority] = " + strPriority
						+ " WHERE [ID] IN " + strIDList);
					vecQueries.push_back("UPDATE [FileActionStatus] SET [Priority] = " + strPriority
						+ " WHERE [FileID] IN " + strIDList);
					
					// Execute the queries
					executeVectorOfSQL(ipConnection, vecQueries);
				}


				// Commit the transaction
				tg.CommitTrans();

				// If returning the number of modified records, set the return value
				if (pnNumRecordsModified != __nullptr)
				{
					*pnNumRecordsModified = nNumRecords;
				}

			END_CONNECTION_RETRY(ipConnection, "ELI27712");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30684");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::AddUserCounter_Internal(bool bDBLocked, BSTR bstrCounterName, LONGLONG llInitialValue)
{
	try
	{
		try
		{
			// Get the counter name and ensure it is not empty
			string strCounterName = asString(bstrCounterName);
			ASSERT_ARGUMENT("ELI27749", !strCounterName.empty());
			replaceVariable(strCounterName, "'", "''");

			// Build the query for adding the new counter
			string strQuery = "INSERT INTO " + gstrUSER_CREATED_COUNTER
				+ "([CounterName], [Value]) VALUES ('" + strCounterName
				+ "', " + asString(llInitialValue) + ")";

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Set the transaction guard
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				// Build a query to check for the existence of the specified counter
				string strCheckDuplicateCounter = "SELECT [CounterName] FROM "
					+ gstrUSER_CREATED_COUNTER + " WHERE [CounterName] = '"
					+ strCounterName + "'";

				// Create a pointer to a recordset
				_RecordsetPtr ipCounter(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI29235", ipCounter != __nullptr);

				// Open Recordset that contains the counter
				ipCounter->Open(strCheckDuplicateCounter.c_str(), _variant_t((IDispatch *)ipConnection, true),
					adOpenDynamic, adLockOptimistic, adCmdText);

				// Check that the counter does not exist
				if (ipCounter->adoEOF == VARIANT_FALSE)
				{
					UCLIDException ue("ELI29236", "Cannot add counter, the specified counter already exists.");
					ue.addDebugInfo("Counter To Add", asString(bstrCounterName));
					throw ue;
				}

				// Close the recordset
				ipCounter->Close();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Execute the query to add the new value to the table
				if (executeCmdQuery(ipConnection, strQuery) == 0)
				{
					UCLIDException ue("ELI27750", "Failed to add the new user counter.");
					ue.addDebugInfo("User Counter Name", asString(bstrCounterName));
					throw ue;
				}

				// Commit the transaction
				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI27751");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30687");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::RemoveUserCounter_Internal(bool bDBLocked, BSTR bstrCounterName)
{
	try
	{
		try
		{
			// Get the counter name and ensure it is not empty
			string strCounterName = asString(bstrCounterName);
			ASSERT_ARGUMENT("ELI27753", !strCounterName.empty());
			replaceVariable(strCounterName, "'", "''");

			// Build the query for deleting the counter
			string strQuery = "DELETE FROM " + gstrUSER_CREATED_COUNTER
				+ " WHERE [CounterName] = '" + strCounterName + "'";

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Set the transaction guard
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Delete the row from the table
				if (executeCmdQuery(ipConnection, strQuery) == 0)
				{
					UCLIDException ue("ELI27754",
						"Failed to remove the user counter: Specified counter does not exist.");
					ue.addDebugInfo("User Counter Name", asString(bstrCounterName));
					throw ue;
				}

				// Commit the transaction
				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI27755");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30688");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::RenameUserCounter_Internal(bool bDBLocked, BSTR bstrCounterName, BSTR bstrNewCounterName)
{
	try
	{
		try
		{
			// Get the counter name and ensure it is not empty
			string strCounterName = asString(bstrCounterName);
			ASSERT_ARGUMENT("ELI27757", !strCounterName.empty());
			replaceVariable(strCounterName, "'", "''");

			string strNewCounterName = asString(bstrNewCounterName);
			ASSERT_ARGUMENT("ELI27758", !strNewCounterName.empty());
			replaceVariable(strNewCounterName, "'", "''");

			// Build the query for renaming the counter
			string strQuery = "UPDATE " + gstrUSER_CREATED_COUNTER + " SET [CounterName] = '"
				+ strNewCounterName + "' WHERE [CounterName] = '" + strCounterName + "'";

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Set the transaction guard
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Check for the existence of the new name
				string strCounterExistsQuery = "SELECT [CounterName] FROM " + gstrUSER_CREATED_COUNTER
					+ " WHERE [CounterName] = '" + strNewCounterName + "'";

				// Create a pointer to a recordset
				_RecordsetPtr ipCounter(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI29233", ipCounter != __nullptr);

				// Open Recordset that contains the counter
				ipCounter->Open(strCounterExistsQuery.c_str(), _variant_t((IDispatch *)ipConnection, true),
					adOpenDynamic, adLockOptimistic, adCmdText);

				// Check for a record found
				if (ipCounter->adoEOF == VARIANT_FALSE)
				{
					UCLIDException ue("ELI29234",
						"Unable to modify counter name, the specified name is already in use.");
					ue.addDebugInfo("Counter To Modify", asString(bstrCounterName));
					ue.addDebugInfo("New Counter Name", asString(bstrNewCounterName));
					throw ue;
				}

				// Close the recordset
				ipCounter->Close();

				// Update the counter name
				if (executeCmdQuery(ipConnection, strQuery) == 0)
				{
					UCLIDException ue("ELI27759",
						"Failed to rename the user counter: Specified counter does not exist.");
					ue.addDebugInfo("User Counter Name", asString(bstrCounterName));
					ue.addDebugInfo("New User Counter Name", asString(bstrNewCounterName));
					throw ue;
				}

				// Commit the transaction
				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI27760");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30689");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::SetUserCounterValue_Internal(bool bDBLocked, BSTR bstrCounterName, LONGLONG llNewValue)
{
	try
	{
		try
		{
			// Get the counter name and ensure it is not empty
			string strCounterName = asString(bstrCounterName);
			ASSERT_ARGUMENT("ELI27762", !strCounterName.empty());
			replaceVariable(strCounterName, "'", "''");

			// Build the query for setting the counter value
			string strQuery = "UPDATE " + gstrUSER_CREATED_COUNTER + " SET [Value] = "
				+ asString(llNewValue) + " WHERE [CounterName] = '" + strCounterName + "'";

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Set the transaction guard
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Set the counter value
				if (executeCmdQuery(ipConnection, strQuery) == 0)
				{
					UCLIDException ue("ELI27763",
						"Failed to set the user counter value: Specified counter does not exist.");
					ue.addDebugInfo("User Counter Name", asString(bstrCounterName));
					throw ue;
				}

				// Commit the transaction
				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI27764");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30690");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetUserCounterValue_Internal(bool bDBLocked, BSTR bstrCounterName, LONGLONG *pllValue)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI27766", pllValue != __nullptr);

			// Get the counter name and ensure it is not empty
			string strCounterName = asString(bstrCounterName);
			ASSERT_ARGUMENT("ELI27767", !strCounterName.empty());
			replaceVariable(strCounterName, "'", "''");

			// Build the query for getting the counter value
			string strQuery = "SELECT [Value] FROM " + gstrUSER_CREATED_COUNTER
				+ " WHERE [CounterName] = '" + strCounterName + "'";

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Recordset to get the counter value from
				_RecordsetPtr ipCounterSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI27768", ipCounterSet != __nullptr);

				// Get the recordset for the specified select query
				ipCounterSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenForwardOnly, 
					adLockReadOnly, adCmdText);

				// Check for value in the database
				if (ipCounterSet->adoEOF == VARIANT_FALSE)
				{
					// Get the return value
					*pllValue = getLongLongField(ipCounterSet->Fields, "Value");
				}
				else
				{
					UCLIDException uex("ELI27769", "User counter name specified does not exist.");
					uex.addDebugInfo("User Counter Name", asString(bstrCounterName));
					throw uex;
				}

			END_CONNECTION_RETRY(ipConnection, "ELI27770");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30691");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetUserCounterNames_Internal(bool bDBLocked, IVariantVector** ppvecNames)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI27772", ppvecNames != __nullptr);

			// Build the query for getting the counter value
			string strQuery = "SELECT [CounterName] FROM " + gstrUSER_CREATED_COUNTER;

			IVariantVectorPtr ipVecNames(CLSID_VariantVector);
			ASSERT_RESOURCE_ALLOCATION("ELI27774", ipVecNames != __nullptr);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Recordset to get the counters from
				_RecordsetPtr ipCounterSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI27775", ipCounterSet != __nullptr);

				// Get the recordset for the specified select query
				ipCounterSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenForwardOnly, 
					adLockReadOnly, adCmdText);

				// Check for value in the database
				while (ipCounterSet->adoEOF == VARIANT_FALSE)
				{
					ipVecNames->PushBack(
						_variant_t(getStringField(ipCounterSet->Fields, "CounterName").c_str()));

					ipCounterSet->MoveNext();
				}

			END_CONNECTION_RETRY(ipConnection, "ELI27776");

			// Set the return value
			*ppvecNames = ipVecNames.Detach();
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30692");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetUserCounterNamesAndValues_Internal(bool bDBLocked, IStrToStrMap** ppmapUserCounters)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI27778", ppmapUserCounters != __nullptr);

			// Build the query for getting the counter value
			string strQuery = "SELECT * FROM " + gstrUSER_CREATED_COUNTER;

			IStrToStrMapPtr ipmapUserCounters(CLSID_StrToStrMap);
			ASSERT_RESOURCE_ALLOCATION("ELI27780", ipmapUserCounters != __nullptr);

			ipmapUserCounters->CaseSensitive = VARIANT_FALSE;

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Recordset to get the counters and values from
				_RecordsetPtr ipCounterSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI27781", ipCounterSet != __nullptr);

				// Get the recordset for the specified select query
				ipCounterSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenForwardOnly, 
					adLockReadOnly, adCmdText);

				// Check for value in the database
				while (ipCounterSet->adoEOF == VARIANT_FALSE)
				{
					FieldsPtr ipFields = ipCounterSet->Fields;
					ASSERT_RESOURCE_ALLOCATION("ELI27782", ipFields != __nullptr);

					// Get the name and value from the record
					string strCounterName = getStringField(ipFields, "CounterName");
					string strCounterValue = asString(getLongLongField(ipFields, "Value"));

					// Add name and value to the collection
					ipmapUserCounters->Set(strCounterName.c_str(), strCounterValue.c_str());

					ipCounterSet->MoveNext();
				}

			END_CONNECTION_RETRY(ipConnection, "ELI27783");

			// Set the return value
			*ppmapUserCounters = ipmapUserCounters.Detach();
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30693");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::IsUserCounterValid_Internal(bool bDBLocked, BSTR bstrCounterName,
												   VARIANT_BOOL* pbCounterValid)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI27907", pbCounterValid != __nullptr);

			// Get the counter name and ensure it is not empty
			string strCounterName = asString(bstrCounterName);
			ASSERT_ARGUMENT("ELI27908", !strCounterName.empty());
			replaceVariable(strCounterName, "'", "''");

			string strQuery = "SELECT [Value] FROM " + gstrUSER_CREATED_COUNTER
				+ " WHERE [CounterName] = '" + strCounterName + "'";

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Recordset to get
				_RecordsetPtr ipCounterSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI27909", ipCounterSet != __nullptr);

				// Get the recordset for the specified select query
				ipCounterSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenForwardOnly, 
					adLockReadOnly, adCmdText);

				// Set true if there is a record found false otherwise
				*pbCounterValid = asVariantBool(ipCounterSet->adoEOF == VARIANT_FALSE);

			END_CONNECTION_RETRY(ipConnection, "ELI27910");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30694");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::OffsetUserCounter_Internal(bool bDBLocked, BSTR bstrCounterName, LONGLONG llOffsetValue,
												   LONGLONG* pllNewValue)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI27715", pllNewValue != __nullptr);

			// Get the counter name and ensure it is not empty
			string strCounterName = asString(bstrCounterName);
			ASSERT_ARGUMENT("ELI27716", !strCounterName.empty());
			replaceVariable(strCounterName, "'", "''");

			// Build the query
			string strQuery = "SELECT [Value] FROM " + gstrUSER_CREATED_COUNTER
				+ " WHERE [CounterName] = '" + strCounterName + "'";

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Set the transaction guard
				TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_criticalSection);

				// Recordset to get the counters and values from
				_RecordsetPtr ipCounterSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI27717", ipCounterSet != __nullptr);

				// Get the recordset for the specified select query
				ipCounterSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenDynamic, 
					adLockOptimistic, adCmdText);

				if (ipCounterSet->adoEOF == VARIANT_FALSE)
				{
					FieldsPtr ipFields = ipCounterSet->Fields;
					ASSERT_RESOURCE_ALLOCATION("ELI27718", ipFields != __nullptr);

					// Get the counter value
					LONGLONG llValue = getLongLongField(ipFields, "Value");

					// Modify the value
					llValue += llOffsetValue;

					// Update the value
					setLongLongField(ipFields, "Value", llValue);
					ipCounterSet->Update();

					// Commit the transaction
					tg.CommitTrans();

					// Set the return value
					*pllNewValue = llValue;
				}
				else
				{
					UCLIDException uex("ELI27815", "User counter name specified does not exist.");
					uex.addDebugInfo("User Counter Name", asString(bstrCounterName));
					throw uex;
				}

			END_CONNECTION_RETRY(ipConnection, "ELI27816");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30695");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::RecordFAMSessionStart_Internal(bool bDBLocked, BSTR bstrFPSFileName,
								BSTR bstrActionName, VARIANT_BOOL vbQueuing, VARIANT_BOOL vbProcessing)
{
	try
	{
		try
		{
			// Set session type so that maintenance threads will be started
			m_bCurrentSessionIsWebSession = false;

			// Get the FPS File name
			m_strFPSFileName = asString(bstrFPSFileName);

			string strFAMSessionQuery = "INSERT INTO [" + gstrFAM_SESSION + "] ";
			strFAMSessionQuery += "([MachineID], [FAMUserID], [UPI], [FPSFileID], [ActionID], "
				"[Queuing], [Processing]) ";
			strFAMSessionQuery += "OUTPUT INSERTED.ID ";
			strFAMSessionQuery += "VALUES (";

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Set the transaction guard
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				// Get FPSFileID, MachineID, and UserID (this will add records if they don't exist)
				long nFPSFileID = getKeyID(ipConnection, gstrFPS_FILE, "FPSFileName",
					m_strFPSFileName.empty() ? "<Unsaved FPS File>" : m_strFPSFileName);
				long nMachineID = getKeyID(ipConnection, gstrMACHINE, "MachineName", m_strMachineName);
				long nUserID = addOrUpdateFAMUser(ipConnection);

				string strQueuing = (asCppBool(vbQueuing) ? "1" : "0");
				string strProcessing = (asCppBool(vbProcessing) ? "1" : "0");

				// https://extract.atlassian.net/browse/ISSUE-14974
				// ESFAMService will run IDatabaseProcess instances and record the processing
				// under a session without any action.
				string strActionID;
				string strActionName = asString(bstrActionName);
				if (!strActionName.empty())
				{
					setActiveAction(ipConnection, strActionName);
					strActionID = asString(m_nActiveActionID);
				}
				else
				{
					m_nActiveActionID = -1;
					strActionID = "null";
				}

				strFAMSessionQuery += asString(nMachineID) + ", " + asString(nUserID) + ", '"
					+ m_strUPI + "', " + asString(nFPSFileID) + ", " + strActionID +
					", " + strQueuing + ", " + strProcessing + ")";

				// Insert the record into the FAMSession table
				executeCmdQuery(ipConnection, strFAMSessionQuery, false, (long*)&m_nFAMSessionID);

				// Whenever processing is started, re-get the secure counters as a way to force
				// validation that the secure counters are in a good state.
				m_ipSecureCounters = nullptr;

				// Commit the transaction
				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI28903");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30696");
	}
	catch(UCLIDException &ue)
	{
		m_strFPSFileName = "";

		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::RecordWebSessionStart_Internal(bool bDBLocked, VARIANT_BOOL vbForQueuing)
{
	try
	{
		try
		{
			// Set session type so that maintenance threads will not be started
			m_bCurrentSessionIsWebSession = true;

			// Make sure the threads are not running
			m_eventStopMaintenanceThreads.signal();

			// Wait for the ping and statistics maintenance threads to exit.
			HANDLE handles[2];
			handles[0] = m_eventPingThreadExited.getHandle();
			handles[1] = m_eventStatsThreadExited.getHandle();
			if (WaitForMultipleObjects(2, (HANDLE *)&handles, TRUE, gnPING_TIMEOUT) != WAIT_OBJECT_0)
			{
				UCLIDException ue("ELI46664", "Application Trace: Timed out waiting for thread to exit.");
				ue.log();
			}

			string strFAMSessionQuery = "INSERT INTO [" + gstrFAM_SESSION + "] ";
			strFAMSessionQuery += "([MachineID], [FAMUserID], [UPI], [FPSFileID], [ActionID], "
				"[Queuing], [Processing]) ";
			strFAMSessionQuery += "OUTPUT INSERTED.ID ";
			strFAMSessionQuery += "VALUES (";

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();

			// Make sure the DB Schema is the expected version
			validateDBSchemaVersion();

			// Set the transaction guard
			TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

			// Get FPSFileID, MachineID, and UserID (this will add records if they don't exist)
			long nFPSFileID = getKeyID(ipConnection, gstrFPS_FILE, "FPSFileName",
				m_strFPSFileName.empty() ? "<Unsaved FPS File>" : m_strFPSFileName);
			long nMachineID = getKeyID(ipConnection, gstrMACHINE, "MachineName", m_strMachineName);
			long nUserID = getKeyID(ipConnection, gstrFAM_USER, "UserName", m_strFAMUserName);
			
			bool bForQueuing = asCppBool(vbForQueuing);

			string strQueuing = bForQueuing ? "1" : "0";
			string strProcessing = bForQueuing ? "0" : "1";

			UCLID_FILEPROCESSINGLib::IWorkflowDefinitionPtr ipWorkflowDefinition =
				getCachedWorkflowDefinition(ipConnection);
			ASSERT_RESOURCE_ALLOCATION("ELI50004", ipWorkflowDefinition != __nullptr);

			string strActionName = bForQueuing
				? asString(ipWorkflowDefinition->StartAction)
				: asString(ipWorkflowDefinition->EditAction);
			ASSERT_RUNTIME_CONDITION("ELI49568", !strActionName.empty(),
				(bForQueuing
				? "Workflow start action not configured"
				: "Workflow verify/update action not configured"));

			setActiveAction(ipConnection, strActionName);

			strFAMSessionQuery += asString(nMachineID) + ", " + asString(nUserID) + ", '"
				+ m_strUPI + "', " + asString(nFPSFileID) + ", " + asString(m_nActiveActionID) +
				", " + strQueuing + ", " + strProcessing + ")";

			// Insert the record into the FAMSession table
			executeCmdQuery(ipConnection, strFAMSessionQuery, false, (long*)&m_nFAMSessionID);

			// Whenever processing is started, re-get the secure counters as a way to force
			// validation that the secure counters are in a good state.
			m_ipSecureCounters = nullptr;

			// Commit the transaction
			tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI45274");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI45275");
	}
	catch (UCLIDException &ue)
	{
		m_strFPSFileName = "";

		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::RecordFAMSessionStop_Internal(bool bDBLocked)
{
	try
	{
		try
		{
			if (m_nFAMSessionID == 0)
			{
				throw UCLIDException("ELI38465", "Cannot stop FAM session that does not exist.");
			}

			// Build the update query to set stop time
			string strFAMSessionQuery = "UPDATE [" + gstrFAM_SESSION + "] SET [StopTime] = GETDATE() "
				"WHERE [" + gstrFAM_SESSION + "].[ID] = " + asString(m_nFAMSessionID) + " AND [StopTime] IS NULL";

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Set the transaction guard
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				// Execute the update query
				executeCmdQuery(ipConnection, strFAMSessionQuery);

				// Commit the transaction
				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI28905");

			// Reset members set by starting/resuming a web and/or FAM session back to their defaults
			setDefaultSessionMemberValues();
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30697");
	}
	catch(UCLIDException &ue)
	{
		// Reset members set by starting/resuming a web and/or FAM session back to their defaults
		setDefaultSessionMemberValues();

		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::RecordInputEvent_Internal(bool bDBLocked, BSTR bstrTimeStamp, long nActionID,
												 long nEventCount, long nProcessID)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Set the transaction guard
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				string strTimeStamp = asString(bstrTimeStamp);
				string strActionId = asString(nActionID);
				string strUserId = 
					asString(getKeyID(ipConnection, gstrFAM_USER, "UserName", m_strFAMUserName));
				string strMachineId = 
					asString(getKeyID(ipConnection, gstrMACHINE, "MachineName", m_strMachineName));
				string strProcessId = asString(nProcessID);

				string strQuery = 
					"SELECT SecondsWithInputEvents "
					"FROM " + gstrINPUT_EVENT + " "
					"WHERE (TimeStamp = '" + strTimeStamp + "') AND (ActionID = " + strActionId + ") "
					"AND (FAMUserID = " + strUserId + ") AND MachineID = " + strMachineId + 
					"AND (PID = " + strProcessId + ")";

				// Create a pointer to a recordset
				_RecordsetPtr ipSeconds(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI29144", ipSeconds != __nullptr);

				// Check if the record set already exists
				ipSeconds->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenDynamic, 
					adLockOptimistic, adCmdText);

				if (ipSeconds->adoEOF == VARIANT_TRUE)
				{
					// The record doesn't exist, create it
					string strSQL = "INSERT INTO [" + gstrINPUT_EVENT + "] ([TimeStamp], "
						"[ActionID], [FAMUserID], [MachineID], [PID], [SecondsWithInputEvents]) "
						"VALUES (CAST('" + strTimeStamp + "' AS smalldatetime), "
						+ strActionId + ", " + strUserId + ", " + strMachineId + ", "
						+ strProcessId + ", " + asString(nEventCount) + ")";

					// Execute the insert query
					executeCmdQuery(ipConnection, strSQL);
				}
				else
				{
					// The record exists
					FieldsPtr ipFields = ipSeconds->Fields;
					ASSERT_RESOURCE_ALLOCATION("ELI29150", ipFields != __nullptr);

					// Add the new second count
					long lTotalCount = nEventCount + getLongField(ipFields, "SecondsWithInputEvents");
					if (lTotalCount > 60)
					{
						lTotalCount = 60;
					}
					setLongField(ipFields, "SecondsWithInputEvents", lTotalCount);

					// Update the table
					ipSeconds->Update();
				}

				// Commit the transaction
				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI28942");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30698");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetLoginUsers_Internal(bool bDBLocked, IStrToStrMap**  ppUsers)
{
	try
	{
		try
		{
			// Get the users from the database
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Create a pointer to a recordset
				_RecordsetPtr ipLoginSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI29040", ipLoginSet != __nullptr);

				// SQL query to get the login users that are not admin
				string strSQL = "SELECT UserName, Password FROM Login where UserName <> 'admin'";

				// Open the set of login users
				ipLoginSet->Open(strSQL.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
					adLockReadOnly, adCmdText);

				// Create map to return results
				IStrToStrMapPtr ipUsers(CLSID_StrToStrMap);
				ASSERT_RESOURCE_ALLOCATION("ELI29039", ipUsers != __nullptr);

				ipUsers->CaseSensitive = VARIANT_FALSE;

				// Step through all records
				while (ipLoginSet->adoEOF == VARIANT_FALSE)
				{
					// Get the fields from the action set
					FieldsPtr ipFields = ipLoginSet->Fields;
					ASSERT_RESOURCE_ALLOCATION("ELI29041", ipFields != __nullptr);

					// Get the user
					string strUser = getStringField(ipFields, "UserName");

					// Get the password
					string strPasswordset = getStringField(ipFields, "Password").empty() ? "No" : "Yes";

					// Save in the Users map
					ipUsers->Set(strUser.c_str(), strPasswordset.c_str());

					// Go to next user
					ipLoginSet->MoveNext();
				}

				// return the StrToStrMap containing all login users except admin
				*ppUsers = ipUsers.Detach();
			END_CONNECTION_RETRY(ipConnection, "ELI29042");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30699");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::AddLoginUser_Internal(bool bDBLocked, BSTR bstrUserName)
{
	try
	{
		try
		{
			// Convert the user name to add to string for local use
			string strUserName = asString(bstrUserName);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Set the transaction guard
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				// Check to see if the new user already exists in the database
				if (doesLoginUserNameExist(ipConnection, strUserName))
				{
					UCLIDException ue("ELI29065", "Login username already exists.");
					ue.addDebugInfo("Username", strUserName);
					throw ue;
				}

				// Execute the insert query to add the user Password defaults to empty string
				executeCmdQuery(ipConnection, "Insert Into Login (UserName) VALUES ('" + strUserName + "')");

				// Commit the changes
				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI29063");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30700");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::RemoveLoginUser_Internal(bool bDBLocked, BSTR bstrUserName)
{
	try
	{
		try
		{
			// Convert the passed in user to string for local use
			string strUserName = asString(bstrUserName);

			// admin or administrator is not allowed
			if (stringCSIS::sEqual(strUserName, "admin") || stringCSIS::sEqual(strUserName, "administrator"))
			{
				UCLIDException ue("ELI29110", "Not allowed to delete admin or administrator.");
				ue.addDebugInfo("User to delete", strUserName);
				throw ue;
			}

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Set the transaction guard
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				// Delete the specified user from the login table
				executeCmdQuery(ipConnection, "DELETE FROM Login WHERE UserName = '" + strUserName + "'");

				// Commit the changes
				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI29067");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30701");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::RenameLoginUser_Internal(bool bDBLocked, BSTR bstrUserNameToRename, BSTR bstrNewUserName)
{
	try
	{
		try
		{
			// Convert the old user and new user names to string for local use
			string strOldUserName = asString(bstrUserNameToRename);
			string strNewUserName = asString(bstrNewUserName);

			// admin or administrator is not allowed
			if (stringCSIS::sEqual(strOldUserName, "admin") || stringCSIS::sEqual(strOldUserName, "administrator"))
			{
				UCLIDException ue("ELI29109", "Not allowed to rename admin or administrator.");
				ue.addDebugInfo("Rename from", strOldUserName);
				ue.addDebugInfo("Rename to", strNewUserName);
				throw ue;
			}

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Check to see if the new user already exists in the database
				if (doesLoginUserNameExist(ipConnection, strNewUserName))
				{
					UCLIDException ue("ELI29711", "Login username already exists.");
					ue.addDebugInfo("Username", strNewUserName);
					throw ue;
				}

				// Set the transaction guard
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				// Change old username to new user name in the table.
				executeCmdQuery(ipConnection, "UPDATE Login SET UserName = '" + strNewUserName + 
					"' WHERE UserName = '" + strOldUserName + "'");

				// Commit the changes
				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI29112");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30702");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::ClearLoginUserPassword_Internal(bool bDBLocked, BSTR bstrUserName)
{
	try
	{
		try
		{
			// Convert username param to string for local use
			string strUserName = asString(bstrUserName);

			// admin or administrator is not allowed
			if (stringCSIS::sEqual(strUserName, "admin") || stringCSIS::sEqual(strUserName, "administrator"))
			{
				UCLIDException ue("ELI29108", "Not allowed to clear administrator password.");
				ue.addDebugInfo("Username", strUserName);
				throw ue;
			}

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Set the transaction guard
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				// Clear the password for the given user in the login table
				executeCmdQuery(ipConnection, "UPDATE Login SET Password = '' WHERE UserName = '" + strUserName + "'");

				// Commit the changes
				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI29070");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30703");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetAutoCreateActions_Internal(bool bDBLocked, VARIANT_BOOL* pvbValue)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI29118", pvbValue != __nullptr);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Get the setting
				string strSetting =
					getDBInfoSetting(ipConnection, gstrAUTO_CREATE_ACTIONS, true);

				// Set the out value
				*pvbValue = strSetting == "1" ? VARIANT_TRUE : VARIANT_FALSE;

			END_CONNECTION_RETRY(ipConnection, "ELI29119");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30704");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::AutoCreateAction_Internal(bool bDBLocked, BSTR bstrActionName, long* plId)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI29795", plId != __nullptr);

			// Get the action name as a string
			string strActionName = asString(bstrActionName);

			// Validate the new action name
			validateNewActionName(strActionName);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				string strActiveWorkflow = getActiveWorkflow();

				*plId = getActionIDNoThrow(ipConnection, strActionName, strActiveWorkflow);

				// Check if the action is not yet created
				if (*plId <= 0)
				{
					// Action is not created; if AutoCreateActions is set, create the action
					if (getDBInfoSetting(ipConnection, gstrAUTO_CREATE_ACTIONS, true) == "1")
					{
						// If the action was added into a particular workflow, make sure the base
						// action exists as well.
						if (!strActiveWorkflow.empty())
						{
							if (getActionIDNoThrow(ipConnection, strActionName, "") <= 0)
							{
								addAction(ipConnection, strActionName, "");
							}
						}

						*plId = addAction(ipConnection, strActionName, strActiveWorkflow);
					}
					else
					{
						// AutoCreateActions is not set, throw an exception
						UCLIDException ue("ELI29157", "Invalid action name.");
						ue.addDebugInfo("Action name", strActionName);
						throw ue;
					}
				}

				// Commit the transaction
				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI29153");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30705");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetFileRecord_Internal(bool bDBLocked, BSTR bstrFile, BSTR bstrActionName,
											  IFileRecord** ppFileRecord)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI29546",  ppFileRecord != __nullptr);

			// Replace any occurrences of ' with '' this is because SQL Server use the ' to indicate
			// the beginning and end of a string
			string strFileName = asString(bstrFile);
			replaceVariable(strFileName, "'", "''");

			// Open a recordset that contain only the record (if it exists) with the given filename
			string strFileSQL = "SELECT * FROM FAMFile WHERE FileName = '" + strFileName + "'";

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Create a pointer to a recordset
				_RecordsetPtr ipFileSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI29547", ipFileSet != __nullptr);

				// Execute the query to find the file in the database
				ipFileSet->Open(strFileSQL.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
					adLockReadOnly, adCmdText);

				if (!asCppBool(ipFileSet->adoEOF))
				{
					// Get the fields from the file set
					FieldsPtr ipFields = ipFileSet->Fields;
					ASSERT_RESOURCE_ALLOCATION("ELI29548", ipFields != __nullptr);

					// Get the file record from the fields
					UCLID_FILEPROCESSINGLib::IFileRecordPtr ipFileRecord(CLSID_FileRecord);
					ASSERT_RESOURCE_ALLOCATION("ELI29549", ipFileRecord != __nullptr);

					// Get and return the appropriate file record
					ipFileRecord = getFileRecordFromFields(ipFields);
					ASSERT_RESOURCE_ALLOCATION("ELI29550", ipFileRecord != __nullptr);

					ipFileRecord->ActionID = getActionID(ipConnection, asString(bstrActionName));

					*ppFileRecord = (IFileRecord*)ipFileRecord.Detach();
				}
				else
				{
					// If no entry was found in the database, return NULL
					*ppFileRecord = NULL;
				}

			END_CONNECTION_RETRY(ipConnection, "ELI29551");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30706");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::SetFileStatusToProcessing_Internal(bool bDBLocked, long nFileId, long nActionID)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				string strSelectSQL = "SELECT FAMFile.ID, FileName, Pages, FileSize, ActionID,"
					"COALESCE(FileActionStatus.Priority, FAMFile.Priority) AS Priority, "
					"COALESCE(ActionStatus, 'U') AS ActionStatus "
					"FROM FAMFile LEFT JOIN FileActionStatus ON "
					"FAMFile.ID = FileID AND ActionID = " + asString(nActionID) +
					" WHERE FAMFile.ID = " + asString(nFileId);

				// Perform all processing related to setting a file as processing.
				// The previous status of the files to process is expected to be either pending or
				// skipped.
				string strActionName = getActionName(ipConnection, nActionID);
				setFilesToProcessing(bDBLocked, ipConnection, strSelectSQL, strActionName, 1, "PS");

			END_CONNECTION_RETRY(ipConnection, "ELI30389");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30707");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::UpgradeToCurrentSchema_Internal(bool bDBLocked,
														IProgressStatusPtr ipProgressStatus)
{
	try
	{
		try
		{
			ValueRestorer<CppBaseApplicationRoleConnection::AppRoles> UseApplicationRolesRestorer(m_currentRole);

			m_currentRole = CppBaseApplicationRoleConnection::kNoRole;

			// Make sure all Product specific DB managers have been recognized.
			checkForNewDBManagers();

			m_bValidatingOrUpdatingSchema = true;

			// Assume a lock is going to be necessary for a schema update.
			ASSERT_ARGUMENT("ELI31401", bDBLocked == true);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			if (ipProgressStatus != nullptr)
			{
				ipProgressStatus->InitProgressStatus("Inspecting schema...", 0, 0, VARIANT_TRUE);
			}

			BEGIN_CONNECTION_RETRY();

			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();
            ipConnection->CommandTimeout = 0;

			assertNotActiveBeforeSchemaUpdate();

			// If there are any unrecognized schema elements in the database, disallow a schema
			// update.
			vector<string> vecUnrecognizedSchemaElements =
				findUnrecognizedSchemaElements(ipConnection);
			if (vecUnrecognizedSchemaElements.size() > 0)
			{
				UCLIDException ue("ELI31402",
					"Database contains custom or unlicensed elements and cannot be upgraded.");
				for (vector<string>::iterator iter = vecUnrecognizedSchemaElements.begin();
					 iter != vecUnrecognizedSchemaElements.end();
					 iter++)
				{
					ue.addDebugInfo("Database Element", *iter);
				}

				throw ue;
			}

			TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

			// Defines the signature for a function which will upgrade the FAM DB schema from one schema
			// number to the next.
			typedef int (*DB_SCHEMA_UPDATE_FUNC)(_ConnectionPtr, long*, IProgressStatusPtr);

			// First, get a vector of the schema update functions needed to upgrade the core FAM DB
			// components.
			vector<DB_SCHEMA_UPDATE_FUNC> vecUpdateFuncs;

			int nOriginalSchemaVersion = getDBSchemaVersion();
			int nSchemaVersion = getDBSchemaVersion();
			switch (nSchemaVersion)
			{
				case 23:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion101);
				case 101:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion102);
				case 102:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion103);
				case 103:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion104);
				case 104:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion105);
				case 105:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion106);
				case 106:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion107);
				case 107:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion108);
				case 108:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion109);
				case 109:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion110);
				case 110:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion111);
				case 111:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion112);
				case 112:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion113);
				case 113:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion114);
				case 114:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion115);
				case 115:   vecUpdateFuncs.push_back(&UpdateToSchemaVersion116);
				case 116:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion117);
				case 117:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion118);
				case 118:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion119);
				case 119:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion120);
				case 120:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion121);
				case 121:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion122);
				case 122:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion123);
				case 123:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion124);
				case 124:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion125);
				case 125:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion126);
				case 126:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion127);
				case 127:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion128);
				case 128:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion129);
				case 129:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion130);
				case 130:   vecUpdateFuncs.push_back(&UpdateToSchemaVersion131);
				case 131:   vecUpdateFuncs.push_back(&UpdateToSchemaVersion132);
				case 132:   vecUpdateFuncs.push_back(&UpdateToSchemaVersion133);
				case 133:   vecUpdateFuncs.push_back(&UpdateToSchemaVersion134);
				case 134:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion135);
				case 135:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion136);
				case 136:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion137);
				case 137:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion138);
				case 138:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion139);
				case 139:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion140);
				case 140:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion141);
				case 141:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion142);
				case 142:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion143);
				case 143:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion144);
				case 144:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion145);
				case 145:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion146);
				case 146:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion147);
				case 147:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion148);
				case 148:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion149);
				case 149:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion150);
				case 150:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion151);
				case 151:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion152);
				case 152:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion153);
				case 153:   vecUpdateFuncs.push_back(&UpdateToSchemaVersion154);
				case 154:   vecUpdateFuncs.push_back(&UpdateToSchemaVersion155);
				case 155:   vecUpdateFuncs.push_back(&UpdateToSchemaVersion156);
				case 156:   vecUpdateFuncs.push_back(&UpdateToSchemaVersion157);
				case 157:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion158);
				case 158:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion159);
				case 159:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion160);
				case 160:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion161);
				case 161:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion162);
				case 162:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion163);
				case 163:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion164);
				case 164:   vecUpdateFuncs.push_back(&UpdateToSchemaVersion165);
				case 165:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion166);
				case 166:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion167);
				case 167:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion168);
				case 168:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion169);
				case 169:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion170);
				case 170:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion171);
				case 171:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion172);
				case 172:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion173);
				case 173:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion174);
				case 174:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion175);
				case 175:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion176);
				case 176:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion177);
				case 177:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion178);
				case 178:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion179);
				case 179:   vecUpdateFuncs.push_back(&UpdateToSchemaVersion180);
				case 180:   vecUpdateFuncs.push_back(&UpdateToSchemaVersion181);
				case 181:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion182);
				case 182:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion183);
				case 183:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion184);
				case 184:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion185);
				case 185:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion186);
				case 186:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion187);
				case 187:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion188);
				case 188:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion189);
				case 189:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion190);
				case 190:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion191);
				case 191:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion192);
				case 192:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion193);
				case 193:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion194);
				case 194:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion195);
				case 195:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion196);
				case 196:   vecUpdateFuncs.push_back(&UpdateToSchemaVersion197);
				case 197:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion198);
				case 198:	vecUpdateFuncs.push_back(&UpdateToSchemaVersion199);
				case 199:   vecUpdateFuncs.push_back(&UpdateToSchemaVersion200);
				case 200:   vecUpdateFuncs.push_back(&UpdateToSchemaVersion201);
				case 201:   vecUpdateFuncs.push_back(&UpdateToSchemaVersion202);
				case 202:
					break;

				default:
					{
						UCLIDException ue("ELI31403",
							"Automatic updates are not supported for the current schema.");
						ue.addDebugInfo("Schema version", nSchemaVersion, false);
						throw ue;
					}
			}

			// Each "stage" of the schema update progress will correspond to a single FAM DB schema
			// version. Depending upon whether product-specific upgrades are needed for the final
			// FAM DB schema number, this may or may not include the final FAM DB schema number.
			int nStageCount = vecUpdateFuncs.size();

			// Keeps track of the total number of progress status steps to allocate for each stage
			// of the upgrade operation.
			vector<int> vecStepCounts;

			// Keeps track of the total number of progress status steps to allocate to the upgrade
			// operation.
			long nTotalStepCount = 0;
			
			// A working count of the steps for each stage.
			long nStepCount = 0;
			
			IIUnknownVectorPtr ipProdSpecificMgrs = getLicensedProductSpecificMgrs();
			ASSERT_RESOURCE_ALLOCATION("ELI31398", ipProdSpecificMgrs != __nullptr);

			// For each product-specific database, the current schema version being acted upon.
			map<string, long> mapProductSpecificVersions;

			// For each FAM DB schema version in the conversion process, query each product-specific
			// database manager for any schema updates that occurred during the time the FAM DB was
			// at the corresponding schema version.
			typedef vector<DB_SCHEMA_UPDATE_FUNC>::iterator funcIterator;
			funcIterator iterFunc = vecUpdateFuncs.begin();
			while (true)
			{
				// Get a count of the total number of steps required for all product-specific
				// schema update steps corresponding to the FAM DB nSchemaVersion. 
				nStepCount = 0;
				executeProdSpecificSchemaUpdateFuncs(ipConnection, ipProdSpecificMgrs,
					nSchemaVersion, &nStepCount, __nullptr, mapProductSpecificVersions);

				if (iterFunc != vecUpdateFuncs.end())
				{
					// Add the progress steps for the next FAM DB update.
					nSchemaVersion = (*iterFunc)(ipConnection, &nStepCount, NULL);
				}
				else if (nStepCount > 0)
				{
					// For the final FAM schema version, if there are product-specific schema
					// updates, run these as an extra "stage".
					nStageCount++;
				}

				vecStepCounts.push_back(nStepCount);
				nTotalStepCount += nStepCount;

				if (iterFunc == vecUpdateFuncs.end())
				{
					break;
				}

				iterFunc++;
			}

			// After a valid upgrade path has been verified and the number of progress steps has
			// been initialized, loop through the upgrade methods, this time performing upgrade.
			nSchemaVersion = nOriginalSchemaVersion;
			mapProductSpecificVersions.clear();

			if (ipProgressStatus != nullptr)
			{
				ipProgressStatus->InitProgressStatus(
					"Updating database schema...", 0, nTotalStepCount, VARIANT_TRUE);
			}

			int nFuncCount = vecUpdateFuncs.size();
			for (int i = 0; i < nStageCount; i++)
			{
				CString zMessage;
				if (vecStepCounts[i] > 100)
				{
					zMessage.Format(
						"Updating database schema... (Step %i of %i; this may take a while)",
						i + 1, nStageCount);
				}
				else
				{
					zMessage.Format("Updating database schema... (Step %i of %i)", i + 1, nStageCount);
				}

				if (ipProgressStatus != nullptr)
				{
					ipProgressStatus->StartNextItemGroup(zMessage.GetString(), vecStepCounts[i]);
				}

				executeProdSpecificSchemaUpdateFuncs(ipConnection, ipProdSpecificMgrs,
					nSchemaVersion, NULL,
					(ipProgressStatus == nullptr) ? nullptr : ipProgressStatus->SubProgressStatus,
					mapProductSpecificVersions);

				if (i < nFuncCount)
				{
					nSchemaVersion = vecUpdateFuncs[i](ipConnection, NULL, 
						(ipProgressStatus == nullptr) ? nullptr : ipProgressStatus->SubProgressStatus);
				}

				if (ipProgressStatus != nullptr)
				{
					ipProgressStatus->CompleteCurrentItemGroup();
				}
			}

			// Update last DB info change time since any schema update will have needed to update
			// the schema version
			executeCmdQuery(ipConnection, gstrUPDATE_DB_INFO_LAST_CHANGE_TIME);

			tg.CommitTrans();

			if (nOriginalSchemaVersion < 183)
			{
				// Changes to how the database ID and counters are persisted mean they will be
				// corrupted at this point; restore them.
				updateDatabaseIDAndSecureCounterTablesSchema183(ipConnection);
			}

			UCLIDException ue("ELI32551", "Application Trace: Database schema updated.");
			ue.addDebugInfo("Old version", nOriginalSchemaVersion);
			ue.addDebugInfo("New version", nSchemaVersion);
			ue.log();

			// Force the DBInfo values (including schema version) to be reloaded on the next call to
			// validateDBSchemaVersion
			m_iDBSchemaVersion = 0; 
            ipConnection->CommandTimeout = m_iCommandTimeout;

			END_CONNECTION_RETRY(ipConnection, "ELI31404");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI31405");

		m_bValidatingOrUpdatingSchema = false;
	}
	catch(UCLIDException &ue)
	{
		m_bValidatingOrUpdatingSchema = false;

		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::RenameFile_Internal(bool bDBLocked, IFileRecord* pFileRecord, BSTR bstrNewName)
{
	try
	{
		try
		{
			UCLID_FILEPROCESSINGLib::IFileRecordPtr ipFileRecord(pFileRecord);
			ASSERT_ARGUMENT("ELI31464", ipFileRecord != __nullptr);
			string strNewName = asString(bstrNewName);
			ASSERT_ARGUMENT("ELI31465", !strNewName.empty());
			
			// Simplify the path for the new name
			simplifyPathName(strNewName);

			// stNewNameForQuery may be modified to work in SQL Query, strNewName should remain the 
			// same as it is at this point so that the name is set correctly in the file record
			string strNewNameForQuery = strNewName;
			string strCurrFileName = ipFileRecord->Name;
			string strFileID = asString(ipFileRecord->FileID);

			// Make sure any ' are escaped by using '' for both the current file name and the new file name
			replaceVariable(strNewNameForQuery, "'", "''");
			replaceVariable(strCurrFileName, "'", "''");

			string strChangeNameQuery = "UPDATE[FAMFile] SET[FileName] = @NewFileName WHERE[FileName] = @CurrentFileName AND ID = @ID";

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				auto cmd = buildCmd(ipConnection,
					strChangeNameQuery,
					{
						{"@NewFileName", strNewName.c_str()},
						{"@CurrentFileName", strCurrFileName.c_str()},
						{"@ID", ipFileRecord->FileID}
					});	

				// Set the transaction guard
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				long lRecordsAffected = executeCmd(cmd);

				// There should be one record affected if not an exception should be thrown
				if (lRecordsAffected != 1)
				{
					UCLIDException ue("ELI31495", "Unable to change file name in FAM Database.");
					ue.addDebugInfo("Query", strChangeNameQuery);
					ue.addDebugInfo("RecordsAffected", asString(lRecordsAffected));
					throw ue;
				}

				// If storing history need to update the SourceDocChangeHistory table
				if (m_bStoreSourceDocChangeHistory)
				{
					auto cmd = buildCmd(ipConnection,
						"INSERT INTO [SourceDocChangeHistory]  ([FileID], [FromFileName], "
						"[ToFileName], [TimeStamp], [FAMUserID], [MachineID]) VALUES "
						"( @FileID, @CurrentFileName, @NewFileName, GetDate(), @FAMUserID, @MachineID) ",
						{
							{"@NewFileName", strNewName.c_str()},
							{"@CurrentFileName", strCurrFileName.c_str()},
							{"@FileID", ipFileRecord->FileID},
							{"@FAMUserID", getFAMUserID(ipConnection)},
							{"@MachineID", getMachineID(ipConnection)}
						});

					executeCmd(cmd);
				}
				
				// Commit the transaction
				tg.CommitTrans();

				// Since the new name is now in the database update the file record that was passed in
				ipFileRecord->Name = strNewName.c_str();

			END_CONNECTION_RETRY(ipConnection, "ELI31466");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI31467");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::get_DBInfoSettings_Internal(bool bDBLocked, IStrToStrMap** ppSettings)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI31895", ppSettings != __nullptr);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();

			// Make sure the DB Schema is the expected version
			validateDBSchemaVersion();

			loadDBInfoSettings(ipConnection);

			ASSERT_RUNTIME_CONDITION("ELI51656", m_ipDBInfoSettings != __nullptr, "Unable to load DBInfo");

			// AddRef before returning a pointer
			IStrToStrMapPtr ipCopy(m_ipDBInfoSettings);
			*ppSettings = ipCopy.Detach();

			END_CONNECTION_RETRY(ipConnection, "ELI31899");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI31900");
	}
	catch (UCLIDException& ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::SetDBInfoSettings_Internal(bool bDBLocked, vector<_CommandPtr> vecCommands,
	long& rnNumRowsUpdated)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Set the transaction guard
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				for each(auto cmd in vecCommands)
				{
					_variant_t vtRecordsUpdated = 0;

					cmd->Execute(&vtRecordsUpdated.GetVARIANT(), __nullptr, adCmdText);
					rnNumRowsUpdated += vtRecordsUpdated.lVal;
				}

				tg.CommitTrans();

				m_ipDBInfoSettings = __nullptr;

			END_CONNECTION_RETRY(ipConnection, "ELI31901");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI31902");
	}
	catch(UCLIDException& uex)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw uex;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::RecordFTPEvent_Internal(bool bDBLocked, long nFileId, long nActionID,
	VARIANT_BOOL vbQueueing, EFTPAction eFTPAction, BSTR bstrServerAddress, BSTR bstrUserName,
	BSTR bstrArg1, BSTR bstrArg2, long nRetries, BSTR bstrException)
{
	try
	{
		try
		{
			if (!m_bStoreFTPEventHistory)
			{
				return S_OK;
			}

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();

			// Make sure the DB Schema is the expected version
			validateDBSchemaVersion();

			// Set the transaction guard
			TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

			string strMachineId = asString(getMachineID(ipConnection));
			string strUserId = asString(getFAMUserID(ipConnection));
			string strQueueOrProcess = asCppBool(vbQueueing) ? "Q" : "P";
			string strFTPAction;
			switch (eFTPAction)
			{
				case kDownloadFileFromFtpServer:	strFTPAction = "D"; break;
				case kUploadFileToFtpServer:		strFTPAction = "U"; break;
				case kDeleteFileFromFtpServer:		strFTPAction = "X"; break;
				case kRenameFileOnFtpServer:		strFTPAction = "R"; break;
				case kGetDirectoryListing:			strFTPAction = "Q"; break;
			}

			string strQuery = gstrRECORD_FTP_EVENT_QUERY;
			replaceVariable(strQuery, gstrTAG_FTP_SERVERADDRESS_VAR, asString(bstrServerAddress));
			replaceVariable(strQuery, gstrTAG_FTP_USERNAME_VAR, asString(bstrUserName));
			replaceVariable(strQuery, gstrTAG_FTP_FILEID_VAR, (nFileId == -1) ? "NULL" : asString(nFileId));
			replaceVariable(strQuery, gstrTAG_FTP_ACTIONID_VAR, (nActionID == -1) ? "NULL" : asString(nActionID));
			replaceVariable(strQuery, gstrTAG_FTP_FTPACTION_VAR, strFTPAction);
			replaceVariable(strQuery, gstrTAG_FTP_QUEUE_OR_PROCESS_VAR, strQueueOrProcess);
			replaceVariable(strQuery, gstrTAG_FTP_ARG1_VAR, asString(bstrArg1));
			replaceVariable(strQuery, gstrTAG_FTP_ARG2_VAR, (bstrArg2 == __nullptr)
				? "NULL"
				: "'" + asString(bstrArg2) + "'");
			replaceVariable(strQuery, gstrTAG_FTP_MACHINEID_VAR, strMachineId);
			replaceVariable(strQuery, gstrTAG_FTP_USERID_VAR, strUserId);
			replaceVariable(strQuery, gstrTAG_FTP_RETRIES_VAR, (nRetries == -1) ? "NULL" : asString(nRetries));
			replaceVariable(strQuery, gstrTAG_FTP_EXCEPTION_VAR, (bstrException == __nullptr)
				? "NULL"
				: "'" + asString(bstrException) + "'");

			executeCmdQuery(ipConnection, strQuery);

			tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI33962");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33963");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::IsAnyFAMActive_Internal(bool bDBLocked, VARIANT_BOOL* pvbFAMIsActive)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI34334", pvbFAMIsActive != __nullptr);

			*pvbFAMIsActive = VARIANT_FALSE;

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();

			if (isFAMActiveForAnyAction(bDBLocked))
			{
				*pvbFAMIsActive = VARIANT_TRUE;
			}

			END_CONNECTION_RETRY(ipConnection, "ELI34331");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI34332");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetFileCount_Internal(bool bDBLocked, VARIANT_BOOL bUseOracleSyntax,
											  LONGLONG* pnFileCount)
{
	try
	{
		try
		{
			bool bUseOracle = asCppBool(bUseOracleSyntax);
			bool bGotFastCount = false;

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();

			// Create a pointer to a recordset
			_RecordsetPtr ipResultSet(__uuidof(Recordset));
			ASSERT_RESOURCE_ALLOCATION("ELI35761", ipResultSet != __nullptr);

			if (bUseOracle)
			{
				ipResultSet->Open(gstrSTANDARD_TOTAL_FAMFILE_QUERY_ORACLE.c_str(),
					_variant_t((IDispatch *)ipConnection, true), adOpenStatic, adLockReadOnly,
					adCmdText);
			}
			else
			{
				long nWorkflowID = getActiveWorkflowID(ipConnection);

				// Can't use a fast count if workflows are involved; need to use the WorkflowFile table.
				if (nWorkflowID > 0)
				{
                    auto cmd = buildCmd(ipConnection, gstrSTANDARD_TOTAL_WORKFLOW_FILES_QUERY, {{"@WorkflowID", nWorkflowID}});

					ipResultSet->Open((IDispatch*) cmd, vtMissing, adOpenStatic, adLockReadOnly,
						adCmdText);
				}
				else if (!m_bDeniedFastCountPermission)
				{
					try
					{
						try
						{
							// First attempt a fast query that requires permissions to query system
							// views FAMFile table.
							ipResultSet->Open(gstrFAST_TOTAL_FAMFILE_QUERY.c_str(),
								_variant_t((IDispatch *)ipConnection, true), adOpenStatic,
								adLockReadOnly, adCmdText);

							bGotFastCount = true;
						}
						CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI35763");
					}
					catch (UCLIDException &ue)
					{
						// If there was an error unrelated to permissions, log it (don't throw in
						// case there is still a chance it is related to permissions).
						if (ue.getTopText().find("permission") == string::npos)
						{
							ue.log();
						}

						m_bDeniedFastCountPermission = true;
					}
				}

				if (nWorkflowID <= 0 && m_bDeniedFastCountPermission)
				{
					// It is possible the result set is open from a previous attempt to open
					// but it threw an exception due to a permissions problem 
					// so check if the result set is open and close it if it is
					// https://extract.atlassian.net/browse/ISSUE-15680
					if ((ipResultSet->State & adStateOpen) > 0 )
					{
						ipResultSet->Close();
					}
					// If the user had insufficient permission for the fast query, use the standard
					// query that will work for all db readers/writers.
					ipResultSet->Open(gstrSTANDARD_TOTAL_FAMFILE_QUERY.c_str(),
						_variant_t((IDispatch *)ipConnection, true), adOpenStatic, adLockReadOnly,
						adCmdText);
				}
			}

			ASSERT_RESOURCE_ALLOCATION("ELI35764", ipResultSet != __nullptr);

			// there should only be 1 record returned
			// TODO: This was modified because the returned recordset from Oracle has records but the 
			// record count value is -1 
			if (ipResultSet->adoEOF != VARIANT_TRUE)
			{
				// get the file count (value type depends on which file count query executed.
				*pnFileCount = bGotFastCount
					? getLongLongField(ipResultSet->Fields, gstrTOTAL_FILECOUNT_FIELD)
					: (long long)getLongField(ipResultSet->Fields, gstrTOTAL_FILECOUNT_FIELD);
			}
			else
			{
				THROW_LOGIC_ERROR_EXCEPTION("ELI35765");
			}

			END_CONNECTION_RETRY(ipConnection, "ELI35766");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI35767");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::IsFeatureEnabled_Internal(bool bDBLocked, BSTR bstrFeatureName,
												  VARIANT_BOOL* pbFeatureIsEnabled)
{
	try
	{
		try
		{
			// If feature data has not been retrieved from the DB, it needs to be done now.
			if (!m_bCheckedFeatures)
			{
				// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
				ADODB::_ConnectionPtr ipConnection = __nullptr;

				BEGIN_CONNECTION_RETRY();

				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				checkFeatures(ipConnection);

				END_CONNECTION_RETRY(ipConnection, "ELI36076");
			}

			// Determine whether the specified feature is enabled by checking m_mapEnabledFeatures.
			string strFeatureName = asString(bstrFeatureName);
			bool bFeatureEnabled = false;

			if (m_mapEnabledFeatures.find(strFeatureName) != m_mapEnabledFeatures.end())
			{
				bFeatureEnabled = m_bLoggedInAsAdmin || !m_mapEnabledFeatures[strFeatureName];
			}

			*pbFeatureIsEnabled = asVariantBool(bFeatureEnabled);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36077");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetWorkItemsForGroup_Internal(bool bDBLocked, long nWorkItemGroupID, 
	long nStartPos, long nCount, IIUnknownVector **ppWorkItems)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI36869",  ppWorkItems != __nullptr);

    		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

                auto cmd = buildCmd(ipConnection, gstrGET_WORK_ITEM_FOR_GROUP_IN_RANGE,
                {
                    {"@WorkItemGroupID", nWorkItemGroupID},
                    {"@StartSequence", nStartPos},
                    {"@EndSequence", nStartPos + nCount}
                });

				// Create a pointer to a recordset
				_RecordsetPtr ipWorkItemSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI36870", ipWorkItemSet != __nullptr);

				// Execute the query get the set of WorkItems
				ipWorkItemSet->Open((IDispatch *)cmd, vtMissing, adOpenStatic, 
					adLockReadOnly, adCmdText);

				IIUnknownVectorPtr ipWorkItems(CLSID_IUnknownVector);
				ASSERT_RESOURCE_ALLOCATION("ELI36876", ipWorkItems != __nullptr);

				while (!asCppBool(ipWorkItemSet->adoEOF))
				{
					// Get the fields from the file set
					FieldsPtr ipFields = ipWorkItemSet->Fields;
					ASSERT_RESOURCE_ALLOCATION("ELI36871", ipFields != __nullptr);

					// Get the file record from the fields
					UCLID_FILEPROCESSINGLib::IWorkItemRecordPtr ipWorkItem(CLSID_WorkItemRecord);
					ASSERT_RESOURCE_ALLOCATION("ELI36872", ipWorkItem != __nullptr);

					// Get and return the appropriate file record
					ipWorkItem = getWorkItemFromFields(ipFields);
					ASSERT_RESOURCE_ALLOCATION("ELI36873", ipWorkItem != __nullptr);

					ipWorkItems->PushBack(ipWorkItem); 
					
					// Go to next record in recordset
					ipWorkItemSet->MoveNext();
				}

				*ppWorkItems = ipWorkItems.Detach();

			END_CONNECTION_RETRY(ipConnection, "ELI36874");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36875");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetWorkItemGroupStatus_Internal(bool bDBLocked, long nWorkItemGroupID, 
	WorkItemGroupStatus *pWorkGroupStatus, EWorkItemStatus *pStatus)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI36880",  pStatus != __nullptr);

			// Convert the nWorkItemGroupID to a string
			string strWorkItemGroupID = asString(nWorkItemGroupID);

			// Setup query to get the count of each WorkItem status for the given WorkGroupID
			string strWorkItemSQL = "SELECT Status, Count(ID) as Total FROM WorkItem " 
				" GROUP BY WorkItemGroupID, Status HAVING WorkItemGroupID = " + strWorkItemGroupID;

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Create a pointer to a recordset
				_RecordsetPtr ipWorkItemSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI36881", ipWorkItemSet != __nullptr);

				// Execute the query to find the file in the database
				ipWorkItemSet->Open(strWorkItemSQL.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
					adLockReadOnly, adCmdText);
				FieldsPtr ipFields = ipWorkItemSet->Fields;
				ASSERT_RESOURCE_ALLOCATION("ELI36884", ipFields != __nullptr);
				pWorkGroupStatus->WorkGroupID = nWorkItemGroupID;

				// Zero the totals in the Work group status structure
				pWorkGroupStatus->lCompletedCount = 0;
				pWorkGroupStatus->lFailedCount = 0;
				pWorkGroupStatus->lPendingCount = 0;
				pWorkGroupStatus->lProcessingCount = 0;
				pWorkGroupStatus->lTotal = 0;

				// Transfer the count of each status to the WorkGroupStatus structure
				long lTotal = 0;
				while (!asCppBool(ipWorkItemSet->adoEOF))
				{
					string strStatus = getStringField(ipFields, "Status");
					long lCount = getLongField(ipFields, "Total");
					lTotal += lCount;
					switch (strStatus[0])
					{
					case 'P': 
						pWorkGroupStatus->lPendingCount =lCount;
						break;
					case 'R':
						pWorkGroupStatus->lProcessingCount = lCount;
						break;
					case 'F':
						pWorkGroupStatus->lFailedCount = lCount;
						break;
					case 'C':
						pWorkGroupStatus->lCompletedCount = lCount;
						break;
					}

					ipWorkItemSet->MoveNext();
				}

				// Create a pointer to a recordset
				_RecordsetPtr ipWorkItemGroupSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI37102", ipWorkItemGroupSet != __nullptr);

				// Setup query to get the expected number of work items for the given WokrItemGroupID
				string strWorkItemGroupSQL = "SELECT NumberOfWorkItems FROM WorkItemGroup WHERE ID = " + strWorkItemGroupID; 
				
				ipWorkItemGroupSet->Open(strWorkItemGroupSQL.c_str() , _variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
					adLockReadOnly, adCmdText);
				if (ipWorkItemGroupSet->adoEOF == VARIANT_TRUE)
				{
					UCLIDException ue("ELI37103", "Work item group not found.");
					ue.addDebugInfo("WorkItemID", strWorkItemGroupID);
					throw ue;
				}

				// Set the total field of the WorkGroupStatus to the expected number of work items
				ipFields = ipWorkItemGroupSet->Fields;
				pWorkGroupStatus->lTotal = getLongField(ipFields, "NumberOfWorkItems");

				// Set the status based on the remaining items to process
				if (pWorkGroupStatus->lProcessingCount > 0)
				{
					*pStatus = kWorkUnitProcessing;
				}
				else if (pWorkGroupStatus->lPendingCount > 0 ||
					pWorkGroupStatus->lTotal != lTotal)
				{
					*pStatus = kWorkUnitPending;
				}
				else if (pWorkGroupStatus->lFailedCount > 0)
				{
					*pStatus = kWorkUnitFailed;
				}
				else 
				{
					*pStatus = kWorkUnitComplete;
				}

			END_CONNECTION_RETRY(ipConnection, "ELI36882");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36883");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::CreateWorkItemGroup_Internal(bool bDBLocked,  long nFileID, long nActionID, 
	BSTR stringizedTask, long nNumberOfWorkItems, BSTR bstrRunningTaskDescription, long *pnWorkItemGroupID)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI37093",  pnWorkItemGroupID != __nullptr);

			// Initialize strings used in queries
			string strFileID = asString(nFileID);
			string strActionID = asString(nActionID);
			string strStringizedTask = asString(stringizedTask);
			string strNumberOfWorkItems = asString(nNumberOfWorkItems);
			string strRunningTaskDescription = asString(bstrRunningTaskDescription);

			// Escape the ' in the task description since it will be used in a query
			replaceVariable(strRunningTaskDescription, "'", "''");

			// Open a recordset that contain only the record (if it exists) with the given filename
			string strAddWorkItemGroupSQL = gstrADD_WORK_ITEM_GROUP_QUERY;

			// Add the values to the query
			strAddWorkItemGroupSQL += " VALUES(" + strFileID + ", " + strActionID +
				", '" + strStringizedTask + "', " +
				asString(m_nFAMSessionID) + ", " + strNumberOfWorkItems + ", '" + strRunningTaskDescription + "')";

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

                auto cmd = buildCmd(ipConnection, gstrGET_WORK_ITEM_GROUP_ID,
                {
                    {"@FileID", nFileID},
                    {"@ActionID", nActionID},
                    {"@StringizedSettings", strStringizedTask.c_str()},
                    {"@NumberOfWorkItems", nNumberOfWorkItems}
                } );
				
				*pnWorkItemGroupID = -1;
				try
				{
					// see if group already exists
                    getCmdId(cmd, pnWorkItemGroupID);
				}
				catch(...)
				{
					// this is expected if the record does not exist
				}

				// There is an existing record so return with the id
				if ( *pnWorkItemGroupID >=0 )
				{
					return true;
				}

				// Create new WorkItemGroup and WorkItem records
				TransactionGuard tg(ipConnection,adXactRepeatableRead, &m_criticalSection);

				executeCmdQuery(ipConnection, strAddWorkItemGroupSQL, false, pnWorkItemGroupID);

				tg.CommitTrans();
				
			END_CONNECTION_RETRY(ipConnection, "ELI37094");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37095");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::AddWorkItems_Internal(bool bDBLocked, long nWorkItemGroupID, 
	IIUnknownVector *pWorkItems)
{
	try
	{
		try
		{
			IIUnknownVectorPtr ipWorkItems(pWorkItems);
			ASSERT_ARGUMENT("ELI36905",  ipWorkItems != __nullptr);
						
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Get the last sequence number
				string strWorkItemQuery = 
					"SELECT COALESCE(MAX(Sequence), 0) as LastSequence FROM WorkItem WHERE WorkItemGroupID = " + 
					asString(nWorkItemGroupID);

				_RecordsetPtr ipWorkItemCountSet =
					ipConnection->Execute(strWorkItemQuery.c_str(), NULL, adCmdText);
				ASSERT_RESOURCE_ALLOCATION("ELI37109", ipWorkItemCountSet != __nullptr);

				int nNextSequence = 0;
				if (ipWorkItemCountSet->adoEOF != VARIANT_TRUE)
				{
					FieldsPtr ipFields = ipWorkItemCountSet->Fields;
					nNextSequence = getLongField(ipFields, "LastSequence");
				}
	
				// Create new WorkItemGroup and WorkItem records
				TransactionGuard tg(ipConnection,adXactRepeatableRead, &m_criticalSection);

				string strWorkItemGroupID = asString(nWorkItemGroupID);

				string strWorkItemSQL = gstrADD_WORK_ITEM_QUERY;

				int iTotalItems = ipWorkItems->Size();
				for (int i = 0; i < iTotalItems; i++)
				{
					UCLID_FILEPROCESSINGLib::IWorkItemRecordPtr ipWorkItem = ipWorkItems->At(i);
					ASSERT_RESOURCE_ALLOCATION("ELI36909", ipWorkItem != __nullptr);

					// Add the WorkItems - this could be made more efficient by putting multiple
					// records in the VALUES statement
					string strStatus;
					switch (ipWorkItem->Status)
					{
					case kWorkUnitPending:
						strStatus = "P";
						break;
					case kWorkUnitProcessing:
						strStatus = "R";
						break;
					case kWorkUnitFailed:
						strStatus = "F";
						break;
					case kWorkUnitComplete:
						strStatus = "C";
						break;
					}
					string strBinaryInput = "NULL";
					if (ipWorkItem->BinaryInput != __nullptr)
					{
						strBinaryInput = "0x" + asString(m_ipMiscUtils->GetObjectAsStringizedByteStream(ipWorkItem->BinaryInput));
					}
					long nFAMSessionID = ipWorkItem->FAMSessionID;
					string strFAMSessionID = (nFAMSessionID == 0) ? "NULL" : asString(nFAMSessionID);
					strWorkItemSQL += (i==0) ? "": ", ";
					strWorkItemSQL += " (" + strWorkItemGroupID + ", '" + strStatus +
						"', '" + asString(ipWorkItem->Input) + "', " + strBinaryInput + 
						", '" + asString(ipWorkItem->Output) + "', " + strFAMSessionID + ", " + asString(nNextSequence) + ")";

					nNextSequence++;
				}
				executeCmdQuery(ipConnection, strWorkItemSQL);

				tg.CommitTrans();
				
			END_CONNECTION_RETRY(ipConnection, "ELI36907");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36908");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetWorkItemToProcess_Internal(bool bDBLocked, string strActionName,
	VARIANT_BOOL vbRestrictToFAMSession, IWorkItemRecord **ppWorkItem)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI37039",  ppWorkItem != __nullptr);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				UCLID_FILEPROCESSINGLib::IWorkItemRecordPtr ipWorkItem = 
					setWorkItemToProcessing(bDBLocked, strActionName,
					asCppBool(vbRestrictToFAMSession), kPriorityDefault, ipConnection);

				*ppWorkItem = (IWorkItemRecord *)ipWorkItem.Detach();

			END_CONNECTION_RETRY(ipConnection, "ELI37038");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37037");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::NotifyWorkItemFailed_Internal(bool bDBLocked, long nWorkItemID, BSTR stringizedException)
{

	try
	{
		try
		{
			string strUpdateToFailed = "UPDATE WorkItem SET Status = 'F', [stringizedException] = '" +
				asString(stringizedException) + "' WHERE ID = " + asString(nWorkItemID);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				TransactionGuard tg(ipConnection,adXactRepeatableRead, &m_criticalSection);

				executeCmdQuery(ipConnection, strUpdateToFailed);

				tg.CommitTrans();
				
			END_CONNECTION_RETRY(ipConnection, "ELI36913");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36914");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::NotifyWorkItemCompleted_Internal(bool bDBLocked, long nWorkItemID)
{
	try
	{
		try
		{
			string strUpdateToComplete = "UPDATE WorkItem SET Status = 'C' WHERE ID = " + asString(nWorkItemID);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				TransactionGuard tg(ipConnection,adXactRepeatableRead, &m_criticalSection);

				executeCmdQuery(ipConnection, strUpdateToComplete);

				tg.CommitTrans();
				
			END_CONNECTION_RETRY(ipConnection, "ELI36915");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36916");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetWorkGroupData_Internal(bool bDBLocked, long nWorkItemGroupID, 
	long *pnNumberOfWorkItems, BSTR *pstringizedTask)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI36900",  pstringizedTask != __nullptr);

			// Convert the nWorkItemGroupID to a string
			string strWorkItemGroupID = asString(nWorkItemGroupID);

			// Setup query to get the WorkItemGroup record
			string strWorkItemGroupSQL = "SELECT * FROM WorkItemGroup WHERE ID = " + strWorkItemGroupID;

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Create a pointer to a recordset
				_RecordsetPtr ipWorkItemGroupSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI36901", ipWorkItemGroupSet != __nullptr);

				// Execute the query to get the WorkItemGroup record
				ipWorkItemGroupSet->Open(strWorkItemGroupSQL.c_str(), 
					_variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
					adLockReadOnly, adCmdText);

				// Should only be one record
				if (!asCppBool(ipWorkItemGroupSet->adoEOF))
				{
					// Get the fields from the file set
					FieldsPtr ipFields = ipWorkItemGroupSet->Fields;
					ASSERT_RESOURCE_ALLOCATION("ELI36902", ipFields != __nullptr);
					
					*pstringizedTask = _bstr_t(getStringField(ipFields, "StringizedSettings").c_str()).Detach();

					*pnNumberOfWorkItems = getLongField(ipFields, "NumberOfWorkItems");
				}
				else
				{
					*pnNumberOfWorkItems = 0;
				}

			END_CONNECTION_RETRY(ipConnection, "ELI36903");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36904");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::SaveWorkItemOutput_Internal(bool bDBLocked, long WorkItemID, BSTR strWorkItemOutput)
{
	try
	{
		try
		{
			string strUpdateQuery = "UPDATE WorkItem SET [Output] = '" + asString(strWorkItemOutput) + 
				"' WHERE ID = " + asString(WorkItemID);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				TransactionGuard tg(ipConnection,adXactRepeatableRead, &m_criticalSection);

				executeCmdQuery(ipConnection, strUpdateQuery);

				tg.CommitTrans();
				
			END_CONNECTION_RETRY(ipConnection, "ELI36921");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36922");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::FindWorkItemGroup_Internal(bool bDBLocked,  long nFileID, long nActionID, 
	BSTR stringizedTask, long nNumberOfWorkItems, BSTR bstrRunningTaskDescription, long *pnWorkItemGroupID)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI37104",  pnWorkItemGroupID != __nullptr);
			
			// Initialize strings used in queries
			string strFileID = asString(nFileID);
			string strActionID = asString(nActionID);
			string strStringizedTask = asString(stringizedTask);
			string strNumberOfWorkItems = asString(nNumberOfWorkItems);
			string strRunningTaskDescription = asString(bstrRunningTaskDescription);

			// Create the query to get matching work item group id
			string strGetExisting = gstrGET_WORK_ITEM_GROUP_ID;
			replaceVariable(strGetExisting, "<FileID>", strFileID);
			replaceVariable(strGetExisting, "<ActionID>", strActionID);
			replaceVariable(strGetExisting, "<StringizedSettings>", strStringizedTask);
			replaceVariable(strGetExisting, "<NumberOfWorkItems>", strNumberOfWorkItems);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Create a pointer to a recordset
				_RecordsetPtr ipWorkItemGroupSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI37105", ipWorkItemGroupSet != __nullptr);

				// Execute the query to get the WorkItemID if it exists
				ipWorkItemGroupSet->Open(strGetExisting.c_str(), 
					_variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
					adLockReadOnly, adCmdText);

				if (!asCppBool(ipWorkItemGroupSet->adoEOF))
				{
					// Get the fields from the file set
					FieldsPtr ipFields = ipWorkItemGroupSet->Fields;
					ASSERT_RESOURCE_ALLOCATION("ELI37106", ipFields != __nullptr);

					*pnWorkItemGroupID = getLongField(ipFields, "ID");

					TransactionGuard tg(ipConnection,adXactRepeatableRead, &m_criticalSection);
					// need to update the FAMSessionID to the current FAMSessionID
					string setFAMSessionID = 
						"UPDATE WorkItemGroup SET FAMSessionID = " + asString(m_nFAMSessionID) + 
						", RunningTaskDescription = '" + strRunningTaskDescription + 
						"' WHERE ID = " + asString(*pnWorkItemGroupID);
					executeCmdQuery(ipConnection, setFAMSessionID);
					tg.CommitTrans();
				}
				else
				{
					*pnWorkItemGroupID = 0;
				}

			END_CONNECTION_RETRY(ipConnection, "ELI37107");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37108");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::SaveWorkItemBinaryOutput_Internal(bool bDBLocked, long WorkItemID, 
	IUnknown *pBinaryOutput)
{
	try
	{
		try
		{
			// The query to select the work item to be updated
			string strWorkItemQuery = "SELECT * FROM WorkItem WHERE ID = " + asString(WorkItemID);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				TransactionGuard tg(ipConnection,adXactRepeatableRead, &m_criticalSection);
				
				// Create a pointer to a recordset
				_RecordsetPtr ipWorkItemSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI38273", ipWorkItemSet != __nullptr);

				// Execute the query to get the WorkItemID if it exists
				ipWorkItemSet->Open(strWorkItemQuery.c_str(), 
					_variant_t((IDispatch *)ipConnection, true),  adOpenDynamic, 
					adLockOptimistic, adCmdText);

				if (!asCppBool(ipWorkItemSet->adoEOF))
				{
					// Get the fields from the file set
					FieldsPtr ipFields = ipWorkItemSet->Fields;
					ASSERT_RESOURCE_ALLOCATION("ELI38274", ipFields != __nullptr);

					setIPersistObjToField(ipFields, "BinaryOutput", pBinaryOutput);

					ipWorkItemSet->Update();
				}
				else
				{
					UCLIDException ue("ELI37207", "WorkItem is not longer in database.");
					ue.addDebugInfo("WorkItemID", WorkItemID);
					throw ue;
				}

				tg.CommitTrans();
				
			END_CONNECTION_RETRY(ipConnection, "ELI38275");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37172");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetFileSetFileNames_Internal(bool bDBLocked, BSTR bstrFileSetName,
	IVariantVector **ppvecFileNames)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI37332", ppvecFileNames != __nullptr);
		
			IVariantVectorPtr ipvecFileNames(CLSID_VariantVector);
			ASSERT_RESOURCE_ALLOCATION("ELI37333", ipvecFileNames != __nullptr);

			string strFileSetName = asString(bstrFileSetName);

			csis_map<vector<int>>::type::iterator iterFileSet = m_mapFileSets.find(strFileSetName);
			if (iterFileSet == m_mapFileSets.end())
			{
				UCLIDException ue("ELI37334", "File set not found");
				ue.addDebugInfo("Set Name", strFileSetName);
				throw ue;
			}

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;
		
			BEGIN_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();

			// Set up a query to get the filename for every ID in the fileset.
			vector<int>& vecFileIDs = iterFileSet->second;
			long nCount = vecFileIDs.size();
			for (long i = 0; i < nCount;)
			{
				string strQuery = "SELECT [FileName] FROM [FAMFile] WITH (NOLOCK) WHERE [ID] IN (";
			
				// Query in batches of 1000 to avoid any one query taking too much time in the DB
				// for very large file sets.
				do
				{
					if ((i % 1000) > 0)
					{
						strQuery += ",";
					}
					strQuery += asString(vecFileIDs[i]);

					i++;
				}
				while (i < nCount && i % 1000 != 0);

				strQuery += ")";

				_RecordsetPtr ipResultSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI37335", ipResultSet != __nullptr);

				ipResultSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true),
					adOpenStatic, adLockReadOnly, adCmdText);
		
				// Add all filenames to the return vector.
				while (!asCppBool(ipResultSet->adoEOF))
				{
					FieldsPtr ipFields = ipResultSet->Fields;
					ASSERT_RESOURCE_ALLOCATION("ELI37336", ipFields != __nullptr);

					ipvecFileNames->PushBack(getStringField(ipFields, "FileName").c_str());

					ipResultSet->MoveNext();
				}
			}

			END_CONNECTION_RETRY(ipConnection, "ELI37337");

			*ppvecFileNames = ipvecFileNames.Detach();
			
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37338");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::SetFallbackStatus_Internal(bool bDBLocked, IFileRecord* pFileRecord,
												   EActionStatus eaFallbackStatus)
{
	try
	{
		try
		{
			if (m_nFAMSessionID == 0)
			{
				throw UCLIDException("ELI38467",
					"Cannot set fallback status without an active FAM session.");
			}

			UCLID_FILEPROCESSINGLib::IFileRecordPtr ipFileRecord(pFileRecord);
			ASSERT_ARGUMENT("ELI37464", ipFileRecord != __nullptr);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;
		
			long nFileId = ipFileRecord->FileID;

			BEGIN_CONNECTION_RETRY();

			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();

			// Make sure the DB Schema is the expected version
			validateDBSchemaVersion();

			TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_criticalSection);

			// Create a pointer to a recordset
			_RecordsetPtr ipFileSet(__uuidof(Recordset));
			ASSERT_RESOURCE_ALLOCATION("ELI37465", ipFileSet != __nullptr);

			// Query for the existing record in the lock table
			string strLockedFileQuery =
				"SELECT [StatusBeforeLock] FROM [LockedFile] "
				"	WHERE [FileID] = " + asString(nFileId) +
				"	AND [ActiveFAMID] = " + asString(m_nActiveFAMID);

			ipFileSet->Open(strLockedFileQuery.c_str(), _variant_t((IDispatch *)ipConnection, true),
				adOpenDynamic, adLockOptimistic, adCmdText);

			if (ipFileSet->adoEOF == VARIANT_TRUE)
			{
				// Either the file was not locked or was not locked by this process.
				UCLIDException ue("ELI37466", "Unable to update record not locked by this process");
				ue.addDebugInfo("Filename", asString(ipFileRecord->Name));
				throw ue;
			}
				
			FieldsPtr ipFields = ipFileSet->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI37467", ipFields != __nullptr);

			// Update the StatusBeforeLock field in the database so that if this process crashes,
			// the auto-revert logic will set the file to the desired fallback status.
			setStringField(ipFields, "StatusBeforeLock", asStatusString(eaFallbackStatus));

			ipFileSet->Update();

			tg.CommitTrans();

			// Update the FallbackStatus of the IFileRecord itself (which is referenced by the
			// FPRecordManager to determine which status to restore the record to).
			ipFileRecord->FallbackStatus = (UCLID_FILEPROCESSINGLib::EActionStatus)eaFallbackStatus;

			END_CONNECTION_RETRY(ipConnection, "ELI37468");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37469");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetWorkItemsToProcess_Internal(bool bDBLocked, string strActionName,
	VARIANT_BOOL vbRestrictToFAMSessionID, long nMaxWorkItemsToReturn, EFilePriority eMinPriority,
	IIUnknownVector **ppWorkItems)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI37424",  ppWorkItems != __nullptr);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				IIUnknownVectorPtr ipWorkItems = 
					setWorkItemsToProcessing(bDBLocked, strActionName, nMaxWorkItemsToReturn, 
						asCppBool(vbRestrictToFAMSessionID), eMinPriority, ipConnection);

				*ppWorkItems = (IIUnknownVector *)ipWorkItems.Detach();

			END_CONNECTION_RETRY(ipConnection, "ELI37425");	
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37418");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::SetWorkItemToPending_Internal(bool bDBLocked, long nWorkItemID)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			string strQuery = 	"UPDATE dbo.WorkItem SET [Status] = 'P' FROM dbo.WorkItem ";
			strQuery = strQuery + "WHERE [ID] = " + asString(nWorkItemID);

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				TransactionGuard tg(ipConnection,adXactRepeatableRead, &m_criticalSection);

				ipConnection->Execute(strQuery.c_str(), NULL, adCmdText);

				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI37427");	
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37419");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetFailedWorkItemsForGroup_Internal(bool bDBLocked, long nWorkItemGroupID,
	IIUnknownVector **ppWorkItems)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI37542",  ppWorkItems != __nullptr);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				auto cmd = buildCmd(ipConnection, gstrGET_FAILED_WORK_ITEM_FOR_GROUP,
					{
						{"@WorkItemGroupID", nWorkItemGroupID}
					});

				// Create a pointer to a recordset
				_RecordsetPtr ipWorkItemSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI37543", ipWorkItemSet != __nullptr);

				// Execute the query get the set of WorkItems
				ipWorkItemSet->Open((IDispatch *)cmd, vtMissing, adOpenStatic, 
					adLockReadOnly, adCmdText);

				IIUnknownVectorPtr ipWorkItems(CLSID_IUnknownVector);
				ASSERT_RESOURCE_ALLOCATION("ELI37544", ipWorkItems != __nullptr);

				while (!asCppBool(ipWorkItemSet->adoEOF))
				{
					// Get the fields from the file set
					FieldsPtr ipFields = ipWorkItemSet->Fields;
					ASSERT_RESOURCE_ALLOCATION("ELI37545", ipFields != __nullptr);

					// Get the file record from the fields
					UCLID_FILEPROCESSINGLib::IWorkItemRecordPtr ipWorkItem(CLSID_WorkItemRecord);
					ASSERT_RESOURCE_ALLOCATION("ELI37546", ipWorkItem != __nullptr);

					// Get and return the appropriate file record
					ipWorkItem = getWorkItemFromFields(ipFields);
					ASSERT_RESOURCE_ALLOCATION("ELI37547", ipWorkItem != __nullptr);

					ipWorkItems->PushBack(ipWorkItem); 
					
					// Go to next record in recordset
					ipWorkItemSet->MoveNext();
				}

				*ppWorkItems = ipWorkItems.Detach();

			END_CONNECTION_RETRY(ipConnection, "ELI37548");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37549");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::SetMetadataFieldValue_Internal(bool bDBLocked, long nFileID,
													   BSTR bstrMetadataFieldName,
													   BSTR bstrMetadataFieldValue)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_criticalSection);

				setMetadataFieldValue(ipConnection, nFileID,
					asString(bstrMetadataFieldName), asString(bstrMetadataFieldValue));

				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI37558");	
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37559");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetMetadataFieldValue_Internal(bool bDBLocked, long nFileID,
													   BSTR bstrMetadataFieldName,
													   BSTR *pbstrMetadataFieldValue)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;
			
			string strQuery =
				"SELECT [Value] FROM [FileMetadataFieldValue] WITH (NOLOCK) "
					"INNER JOIN [MetadataField] WITH (NOLOCK) ON [MetadataField].[ID] = [FileMetadataFieldValue].[MetadataFieldID] "
					"WHERE [FileID] = @FileID AND [Name] = @MetadataFieldName";


			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();
				
				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				auto cmd = buildCmd(ipConnection, strQuery, 
					{ 
						{"@FileID", nFileID}, 
						{"@MetadataFieldName", bstrMetadataFieldName} 
					});

				// Create a pointer to a recordset
				_RecordsetPtr ipResult(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI37639", ipResult != __nullptr);

				// Execute the query get the set of WorkItems
				ipResult->Open((IDispatch*)cmd, vtMissing, adOpenStatic, 
					adLockReadOnly, adCmdText);

				if (!asCppBool(ipResult->adoEOF))
				{
					// Get the fields from the file set
					FieldsPtr ipFields = ipResult->Fields;
					ASSERT_RESOURCE_ALLOCATION("ELI37640", ipFields != __nullptr);					

					*pbstrMetadataFieldValue = get_bstr_t(getStringField(ipFields, "Value")).Detach();
				}

			END_CONNECTION_RETRY(ipConnection, "ELI37641");	
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37642");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetMetadataFieldNames_Internal(bool bDBLocked, IVariantVector **ppMetadataFieldNames)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI37657", ppMetadataFieldNames != __nullptr);

			IVariantVectorPtr ipVecMetadataFields(CLSID_VariantVector);
			ASSERT_RESOURCE_ALLOCATION("ELI37658", ipVecMetadataFields != __nullptr);

			string strQuery = "SELECT [Name] FROM [MetadataField]";

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				validateDBSchemaVersion();

				// Create a pointer to a recordset
				_RecordsetPtr ipMetadataFieldSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI37659", ipMetadataFieldSet != __nullptr);

				// Open Recordset that contains the metadata field names
				ipMetadataFieldSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true),
					adOpenForwardOnly, adLockReadOnly, adCmdText);

				// Loop through each metadata field name and add it to the variant vector
				while (ipMetadataFieldSet->adoEOF == VARIANT_FALSE)
				{
					// Get the metadata field name and add it to the collection
					string strMetadataFieldName = getStringField(ipMetadataFieldSet->Fields, "Name");
					ipVecMetadataFields->PushBack(strMetadataFieldName.c_str());

					// Move to the next metadata field
					ipMetadataFieldSet->MoveNext();
				}

			END_CONNECTION_RETRY(ipConnection, "ELI37660");

			// Set the out value
			*ppMetadataFieldNames = ipVecMetadataFields.Detach();
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37661");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::AddMetadataField_Internal(bool bDBLocked, const string& strMetadataFieldName)
{
	try
	{
		try
		{
			string strQuery = "SELECT [Name] FROM [MetadataField] WHERE [Name] = '"
				+ strMetadataFieldName + "'";

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				validateDBSchemaVersion();

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				// Create a pointer to a recordset
				_RecordsetPtr ipMetadataFieldSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI38257", ipMetadataFieldSet != __nullptr);

				// Open Recordset that contains the metadata field names
				ipMetadataFieldSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenDynamic, 
					adLockOptimistic, adCmdText);

				if (ipMetadataFieldSet->adoEOF == VARIANT_FALSE)
				{
					UCLIDException ue("ELI38258", "Specified metadata field already exists!");
					ue.addDebugInfo("Metadata Field Name", strMetadataFieldName);
					throw ue;
				}
				else
				{
					ipMetadataFieldSet->AddNew();

					// Get the fields
					FieldsPtr ipFields = ipMetadataFieldSet->Fields;
					ASSERT_RESOURCE_ALLOCATION("ELI38259", ipFields != __nullptr);

					// Set the fields
					setStringField(ipFields, "Name", strMetadataFieldName);

					// Update the table
					ipMetadataFieldSet->Update();
				}

				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI37699");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37700");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::DeleteMetadataField_Internal(bool bDBLocked, BSTR bstrMetadataFieldName)
{
	try
	{
		try
		{
			// Get the metadata field name
			string strMetadataFieldName = asString(bstrMetadataFieldName);
			validateMetadataFieldName(strMetadataFieldName);

			// Build the query
			string strQuery = "SELECT [Name] FROM [MetadataField] WHERE [Name] = '"
				+ strMetadataFieldName + "'";

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				validateDBSchemaVersion();

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				// Create a pointer to a recordset
				_RecordsetPtr ipMetadataFieldSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI37701", ipMetadataFieldSet != __nullptr);

				// Open Recordset that contains the metadata field names
				ipMetadataFieldSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenDynamic, 
					adLockOptimistic, adCmdText);

				if (ipMetadataFieldSet->adoEOF == VARIANT_FALSE)
				{
					// Delete the current record
					ipMetadataFieldSet->Delete(adAffectCurrent);

					// Update the table
					ipMetadataFieldSet->Update();
				}

				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI37702");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38263");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::RenameMetadataField_Internal(bool bDBLocked, BSTR bstrOldMetadataFieldName,
	BSTR bstrNewMetadataFieldName)
{
	try
	{
		try
		{
			// Get the old name and validate it
			string strOldMetadataFieldName = asString(bstrOldMetadataFieldName);
			validateMetadataFieldName(strOldMetadataFieldName);

			// Get the new name and validate it
			string strNewMetadataFieldName = asString(bstrNewMetadataFieldName);
			validateMetadataFieldName(strNewMetadataFieldName);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				validateDBSchemaVersion();

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				string strQueryBase = "SELECT [Name] FROM [MetadataField] WHERE [Name] = '";

				// Check for new name existence
				string strTempQuery = strQueryBase + strNewMetadataFieldName + "'";
				_RecordsetPtr ipTemp(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI37703", ipTemp != __nullptr);

				ipTemp->Open(strTempQuery.c_str(), _variant_t((IDispatch*) ipConnection, true),
					adOpenDynamic, adLockOptimistic, adCmdText);
				if (ipTemp->adoEOF == VARIANT_FALSE)
				{
					UCLIDException ue("ELI37704", "New metadata field name already exists.");
					ue.addDebugInfo("Old Metadata Field Name", strOldMetadataFieldName);
					ue.addDebugInfo("New Metadata Field Name", strNewMetadataFieldName);
					throw ue;
				}

				string strQuery = strQueryBase + strOldMetadataFieldName + "'";

				// Create a pointer to a recordset
				_RecordsetPtr ipMetadataFieldSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI37705", ipMetadataFieldSet != __nullptr);

				// Open Recordset that contains the tag names
				ipMetadataFieldSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenDynamic, 
					adLockOptimistic, adCmdText);

				// Ensure there is a record for the old tag name
				if (ipMetadataFieldSet->adoEOF == VARIANT_TRUE)
				{
					UCLIDException ue("ELI37706", "The specified metadata field does not exist!");
					ue.addDebugInfo("Old Metadata Field Name", strOldMetadataFieldName);
					ue.addDebugInfo("New Metadata Field Name", strNewMetadataFieldName);
					throw ue;
				}

				// Get the fields pointer
				FieldsPtr ipFields = ipMetadataFieldSet->Fields;
				ASSERT_RESOURCE_ALLOCATION("ELI37707", ipFields != __nullptr);

				// Update the record with the new value
				if (!strNewMetadataFieldName.empty())
				{
					setStringField(ipFields, "Name", strNewMetadataFieldName);
				}

				ipMetadataFieldSet->Update();

				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI37708");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37709");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::StartFileTaskSession_Internal(bool bDBLocked, BSTR bstrTaskClassGuid,
	long nFileID, long nActionID, long *pnFileTaskSessionID)
{
	try
	{
		try
		{
			*pnFileTaskSessionID = 0;

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = nullptr;

			BEGIN_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();

			validateDBSchemaVersion();

			getCmdId(buildCmd(ipConnection, gstrSTART_FILETASKSESSION_DATA,
				{
					{ "@FAMSessionID", m_nFAMSessionID },
					{ "@TaskClassGuid", bstrTaskClassGuid },
					{ "@FileID", nFileID },
					{ "@ActionID", nActionID }
				})
				, pnFileTaskSessionID);
			
			END_CONNECTION_RETRY(ipConnection, "ELI38640");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38641");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::EndFileTaskSession_Internal(bool bDBLocked, long nFileTaskSessionID,
	double dOverheadTime, double dActivityTime, bool bSessionTimeOut)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = nullptr;

			BEGIN_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();

			validateDBSchemaVersion();

			map<string, _variant_t> params = {
				{"@FileTaskSessionID", nFileTaskSessionID},
				{"@OverheadTime", dOverheadTime},
				{"@ActivityTime", dActivityTime},
				{"@SessionTimeOut", bSessionTimeOut},
				{"@SessionTimeoutPeriod", m_dVerificationSessionTimeout}
			};

			auto cmd = buildCmd(ipConnection, gstrUPDATE_FILETASKSESSION_DATA, params);

			executeCmd(cmd);

            cmd = buildCmd(ipConnection, 
                "DELETE FROM [FileTaskSessionCache] \r\n"
				"WHERE [AutoDeleteWithActiveFAMID] IS NOT NULL AND [FileTaskSessionID] = @FileTaskSessionID",
                {
                    {"@FileTaskSessionID", nFileTaskSessionID}
                });
			executeCmd(cmd);
			
			END_CONNECTION_RETRY(ipConnection, "ELI39694");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI39695");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetSecureCounters_Internal(bool bDBLocked, VARIANT_BOOL vbRefresh,
												   IIUnknownVector** ppSecureCounters)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();
			
			// Use any already existing m_ipSecureCounters unless refreshing.
			if (!asCppBool(vbRefresh) && m_ipSecureCounters != __nullptr)
			{
				// Need to increment the reference count since passing to a non smart pointer
				m_ipSecureCounters.AddRef();
				*ppSecureCounters = m_ipSecureCounters;
				return S_OK;
			}

			InvalidatePreviousCachedInfoIfNecessary();

			// Get the connection for the thread and save it locally.
			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();
			bool bIsDatabaseIDValid = checkDatabaseIDValid(ipConnection, false);

			// Get the last issued FAMFile id
			executeCmdQuery(ipConnection,"SELECT cast(IDENT_CURRENT('FAMFile') as int) AS ID",
				false, &m_nLastFAMFileID);

			// Create a pointer to a recordset
			_RecordsetPtr ipResultSet(__uuidof(Recordset));
			ASSERT_RESOURCE_ALLOCATION("ELI38940", ipResultSet != __nullptr);

			// Make sure the DB Schema is the expected version
			validateDBSchemaVersion();

			// Get a list of all of the counters from the database
			string strQuery = gstrSELECT_SECURE_COUNTER_WITH_MAX_VALUE_CHANGE;
			ipResultSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
				adLockReadOnly, adCmdText);

			IIUnknownVectorPtr ipSecureCounters(CLSID_IUnknownVector);

			while (!asCppBool(ipResultSet->adoEOF))
			{
				DBCounter dbCounter;
				UCLID_FILEPROCESSINGLib::IFAMDBSecureCounterPtr ipSecureCounter(nullptr);
				SECURE_CREATE_OBJECT("ELI38941", ipSecureCounter,
					"Extract.FileActionManager.Database.FAMDBRuleExecutionCounter");

				try
				{
					try
					{
						dbCounter.LoadFromFields(ipResultSet->Fields);
						bool bValid = bIsDatabaseIDValid &&
							dbCounter.isValid(m_DatabaseIDValues, ipResultSet->Fields);

						ipSecureCounter->Initialize(getThisAsCOMPtr(), dbCounter.m_nID,
							_bstr_t(dbCounter.m_strName.c_str()), dbCounter.m_nAlertLevel,
							dbCounter.m_nAlertMultiple, asVariantBool(bValid));
					}
					CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI41637")
				}
				catch (UCLIDException ue)
				{
					// Log the exception to make it easier to track Counter problems
					ue.log();
					ipSecureCounter->Initialize(getThisAsCOMPtr(), 0, "", 0, 0, false);
				}
				
				ipSecureCounters->PushBack(ipSecureCounter);

				ipResultSet->MoveNext();
			}
			m_ipSecureCounters = ipSecureCounters;
			
			// Need to increment the reference count since passing to a non smart pointer
			m_ipSecureCounters.AddRef();
			*ppSecureCounters = ipSecureCounters;

			END_CONNECTION_RETRY(ipConnection, "ELI39085");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI39086");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetSecureCounterName_Internal(bool bDBLocked, long nCounterID, BSTR *pstrCounterName)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();
			
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				string strQuery = "SELECT CounterName FROM SecureCounter WHERE ID = " + asString(nCounterID);
				
				// Create a pointer to a recordset
				_RecordsetPtr ipResultSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI38901", ipResultSet != __nullptr);

				// Open the Action table
				ipResultSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
					adLockReadOnly, adCmdText);

				string strCounterName = "";
				if (!asCppBool(ipResultSet->adoEOF))
				{
					strCounterName = getStringField(ipResultSet->Fields, "CounterName");
				}
				*pstrCounterName = _bstr_t(strCounterName.c_str()).Detach();
			
			END_CONNECTION_RETRY(ipConnection, "ELI38800");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38782");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}	

	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::ApplySecureCounterUpdateCode_Internal(bool bDBLocked, BSTR strUpdateCode, BSTR *pbstrResult)
{

	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI39030", pbstrResult != __nullptr);

			DBCounterUpdate counterUpdates;

			try
			{
				try
				{
					ByteStream bsPW;
					getFAMPassword(bsPW);

					// Get the bytestream from the update code
					ByteStream bsUpgradeCode = MapLabel::getMapLabelWithS(asString(strUpdateCode), bsPW);
					ByteStreamManipulator bsmUpgradeCode(ByteStreamManipulator::kRead, bsUpgradeCode);

					bsmUpgradeCode >> counterUpdates;

					ASSERT_RUNTIME_CONDITION("ELI38903", counterUpdates.m_nNumberOfUpdates != 0,
						"No counter updates in code.");
				}
				CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI39159");
			}
			catch (UCLIDException &ue)
			{
				throw UCLIDException("ELI39160", "Failed to parse counter update or unlock code.", ue);
			}

			_ConnectionPtr ipConnection;
			BEGIN_CONNECTION_RETRY();

				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();
				bool bValid = checkDatabaseIDValid(ipConnection, false);

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_criticalSection);
				string strApplyResult;

				try
				{
					try
					{
						UCLIDException ueLog("ELI39143", "Counter code applied.");

						if (counterUpdates.m_nNumberOfUpdates < 0 )
						{
							// this is an unlock code
							unlockCounters(ipConnection, counterUpdates, ueLog);
							strApplyResult = "Counters restored to working state.\r\n";
						}
						else
						{
							if (!bValid)
							{
								UCLIDException ueInvalid("ELI39147", "DatabaseID is corrupt.");
								m_DatabaseIDValues.addAsDebugInfo(ueInvalid, "DatabaseID_");
								throw ueInvalid;
							}

							// Validate the guid and the LastUpdated time
							if (m_DatabaseIDValues != counterUpdates.m_DatabaseID)
							{
								UCLIDException ueInvalid("ELI38902", "Code is not valid.");
								counterUpdates.m_DatabaseID.addAsDebugInfo(ueInvalid, "UpdateCode_");
								m_DatabaseIDValues.addAsDebugInfo(ueInvalid, "DatabaseID_");
								throw ueInvalid;
							}

							strApplyResult = updateCounters(ipConnection, counterUpdates, ueLog);
						}
						tg.CommitTrans();

						// The update was successful so log the ueLog exception
						ueLog.log();
						
						// Whenever counter updates/unlocks are applied, re-get the secure counters 
						// to ensure their new states are properly reported.
						m_ipSecureCounters = nullptr;

						*pbstrResult = _bstr_t(strApplyResult.c_str()).Detach();
					}
					CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38904");
				}
				catch (UCLIDException &ue)
				{
					UCLIDException ueBad("ELI38905", "Unable to process counter update code.", ue);
					throw ueBad;
				}

			END_CONNECTION_RETRY(ipConnection, "ELI38906");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38783");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetSecureCounterValue_Internal(bool bDBLocked, long nCounterID, long* pnCounterValue)
{
	try
	{
		try
		{
			_ConnectionPtr ipConnection;
			BEGIN_CONNECTION_RETRY();

				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				string strQuery = "SELECT * FROM [dbo].[SecureCounter] WHERE ID = " +
					 asString(nCounterID);

				if (!m_bDatabaseIDValuesValidated)
				{
					checkDatabaseIDValid(ipConnection, true);
				}

				// Create a pointer to a recordset
				_RecordsetPtr ipResultSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI38924", ipResultSet != __nullptr);
				
				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Open the Action table
				ipResultSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
					adLockReadOnly, adCmdText);

				if (!asCppBool(ipResultSet->adoEOF))
				{
					FieldsPtr fields = ipResultSet->Fields;
					DBCounter dbCounter;
					
					dbCounter.LoadFromFields(ipResultSet->Fields);
					dbCounter.validate(m_DatabaseIDValues);

					*pnCounterValue = dbCounter.m_nValue;
					return true;
				}
				UCLIDException ue("ELI38931", "Counter value could not be determined.");
				ue.addDebugInfo("CounterID", nCounterID);
				throw ue;

			END_CONNECTION_RETRY(ipConnection, "ELI38923");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38784");
	}
	catch(UCLIDException &ue)
	{
		m_bDatabaseIDValuesValidated = false;
		m_strEncryptedDatabaseID = "";
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::DecrementSecureCounter_Internal(bool bDBLocked, long nCounterID, long decrementAmount, long* pnCounterValue)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI39122", pnCounterValue != nullptr);
			ASSERT_RUNTIME_CONDITION("ELI39026", decrementAmount >= 0, "Decrement must be positive.");

			_ConnectionPtr ipConnection;
			BEGIN_CONNECTION_RETRY();

				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				string strQuery = "SELECT * FROM [dbo].[SecureCounter] WHERE ID = " +
						asString(nCounterID);

				if (!m_bDatabaseIDValuesValidated)
				{
					checkDatabaseIDValid(ipConnection, true);
				}
				
				// Create a pointer to a recordset
				_RecordsetPtr ipResultSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI38937", ipResultSet != __nullptr);
				
				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_criticalSection);

				// Open the Action table
				ipResultSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
					adLockReadOnly, adCmdText);

				if (!asCppBool(ipResultSet->adoEOF))
				{
					FieldsPtr fields = ipResultSet->Fields;
					DBCounter dbCounter;
					dbCounter.LoadFromFields(ipResultSet->Fields);
					dbCounter.validate(m_DatabaseIDValues);

					DBCounterChangeValue dbCounterChange(m_DatabaseIDValues);
					dbCounterChange.m_nCounterID = dbCounter.m_nID;
					dbCounterChange.m_nFromValue = dbCounter.m_nValue;
					if (decrementAmount > dbCounter.m_nValue)
					{
						UCLIDException ue("ELI38938", "Counter has insufficient counts.");
						ue.addDebugInfo("CounterName", dbCounter.m_strName);
						ue.addDebugInfo("CounterID", dbCounter.m_nID);
						ue.addDebugInfo("RequiredCounts", decrementAmount);
						ue.addDebugInfo("RemainingCounts", dbCounter.m_nValue);
						throw ue;
					}
					else
					{
						long lOldValue = dbCounter.m_nValue;
						dbCounter.m_nValue -= decrementAmount;

						dbCounterChange.m_nToValue = dbCounter.m_nValue;
						dbCounterChange.m_nLastUpdatedByFAMSessionID = m_nFAMSessionID;

						dbCounterChange.m_llMinFAMFileCount = m_nLastFAMFileID;
										
						dbCounterChange.m_stUpdatedTime = getSQLServerDateTimeAsSystemTime(role->ADOConnection());

						dbCounterChange.CalculateHashValue(dbCounterChange.m_llHashValue);

						// list of queries to run
						vector<string> vecUpdateQueries;
						vecUpdateQueries.push_back("UPDATE [dbo].[SecureCounter] SET SecureCounterValue = '" + 
							dbCounter.getEncrypted(m_DatabaseIDValues) + 
							"' WHERE ID = " + asString(dbCounter.m_nID));

						vecUpdateQueries.push_back(dbCounterChange.GetInsertQuery());

						executeVectorOfSQL(ipConnection, vecUpdateQueries);
						tg.CommitTrans();

						// Check new counter value and possibly add item to exception log
						if (((lOldValue - 1) / gnLOG_FREQUENCY) != ((dbCounter.m_nValue - 1)/ gnLOG_FREQUENCY))
						{
							UCLIDException ue("ELI40383", "Application trace: debug information");
							ue.addDebugInfo("Item 1", dbCounter.m_nID, true );
							ue.addDebugInfo("Item 2", dbCounter.m_strName, true);
							ue.addDebugInfo("Item 3", dbCounter.m_nValue, true );
							ue.log();
						}							
						*pnCounterValue = dbCounterChange.m_nToValue;
					}
				}
			END_CONNECTION_RETRY(ipConnection, "ELI38936");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38785");
	}
	catch(UCLIDException &ue)
	{
		m_bDatabaseIDValuesValidated = false;
		m_strEncryptedDatabaseID = "";
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::SecureCounterConsistencyCheck_Internal(bool bDBLocked, VARIANT_BOOL* pvbValid)
{
	try
	{
		try
		{
			// TODO: do the check
			*pvbValid = VARIANT_TRUE;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38786");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetCounterUpdateRequestCode_Internal(bool bDBLocked, BSTR* pstrUpdateRequestCode)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				bool bValid = checkDatabaseIDValid(ipConnection, false);

				DatabaseIDValues DBIDValue = m_DatabaseIDValues;

				if (!bValid)
				{
                    bool clustered;
					// Modify the DBIdValue to have corrected values (m_GUID and m_stLastUpdated will be the same)
					getDatabaseInfo(ipConnection, m_strDatabaseName, DBIDValue.m_strServer,
						DBIDValue.m_stCreated, DBIDValue.m_stRestored, clustered);
					DBIDValue.m_strName = m_strDatabaseName;
				}

				// Create a pointer to a recordset
				_RecordsetPtr ipResultSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI38907", ipResultSet != __nullptr);

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Open the secure counter table
				ipResultSet->Open(gstrSELECT_SECURE_COUNTER_WITH_MAX_VALUE_CHANGE.c_str(), 
					_variant_t((IDispatch *)ipConnection, true), adOpenStatic, adLockReadOnly,
					adCmdText);

				vector<DBCounter> vecDBCounters;
				while (!asCppBool(ipResultSet->adoEOF))
				{
					DBCounter dbCounter;
					dbCounter.LoadFromFields(ipResultSet->Fields);

					bValid = bValid && dbCounter.isValid(
						m_DatabaseIDValues, ipResultSet->Fields);

					vecDBCounters.push_back(dbCounter);

					ipResultSet->MoveNext();
				}
				long nNumCounters = vecDBCounters.size();
				bool bCreatedNewDatabaseID = false;
				
				// if the number of counters is 0 and the DatabaseID is invalid
				// create a completely new DatabaseID value and save it in DBInfo
				// then bValid will be set to true and this becomes a request code instead
				if (!bValid && nNumCounters == 0)
				{
					// Create a new DatabaseID
					createAndStoreNewDatabaseID(ipConnection);
					
					// Set the DatabaseID that will be in the request to the new DatabaseID Value
					DBIDValue = m_DatabaseIDValues;
					
					// Since no counters were defined the DatabaseID is now valid
					bValid = true;
				}
				else if (!bValid && m_DatabaseIDValues.m_GUID == GUID_NULL)
				{
					// Create a new DatabaseID
					createAndStoreNewDatabaseID(ipConnection);
					bCreatedNewDatabaseID = true;
					DBIDValue = m_DatabaseIDValues;
				}


				ByteStream bsRequestCode;
				ByteStreamManipulator bsmRequest(ByteStreamManipulator::kWrite, bsRequestCode);
				
				// Add the version to the request
				bsmRequest << glSECURE_COUNTER_REQUEST_VERSION;

				bsmRequest << DBIDValue;

				// Send the offset from UTC for the current timezone so that the counter manager
				// can translate times reported here to central time.
				TIME_ZONE_INFORMATION tzi;
				bool daylight = (GetTimeZoneInformation(&tzi) == TIME_ZONE_ID_DAYLIGHT);
				bsmRequest << (daylight
					? tzi.Bias + tzi.DaylightBias
					: tzi.Bias + tzi.StandardBias);

				// Add the current time
				bsmRequest << getSQLServerDateTimeAsSystemTime(role->ADOConnection());

				bsmRequest << (((nNumCounters == 0) || bValid) ? nNumCounters : -nNumCounters);

				for (auto c = vecDBCounters.begin(); c != vecDBCounters.end(); c++)
				{
					bsmRequest << c->m_nID;
					if (c->m_nID >= 100)
					{
						bsmRequest << c->m_strName;
					}						

					if (c->m_bUnrecoverable)
					{
						// Indicate unrecoverable counters with a -1 value.
						bsmRequest << (long)-1;
					}
					else
					{
						bsmRequest << c->m_nValue;
					}

					if (!bValid)
					{
						bsmRequest << c->m_nChangeLogValue;
						bsmRequest << c->m_strValidationError;
					}
				}

				if (!bValid)
				{
					if (!bCreatedNewDatabaseID)
					{
						m_DatabaseIDValues.CheckIfValid(ipConnection, false, true);
						bsmRequest << m_DatabaseIDValues.m_strInvalidReason;
					}
					else
					{
						string strMessage = "DatabaseID was missing and new DatabaseID has been created.";
						bsmRequest << strMessage;
					}
				}

				bsmRequest.flushToByteStream(8);

				// Get the password 'key' based on the 4 hex global variables
				ByteStream pwBS;
				getFAMPassword(pwBS);
				
				// Create the | separated list
				string strCode = MapLabel::setMapLabelWithS(bsRequestCode, pwBS);
				makeUpperCase(strCode);
				
				*pstrUpdateRequestCode = _bstr_t(strCode.c_str()).Detach();
				
			END_CONNECTION_RETRY(ipConnection, "ELI38798");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38790");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::SetSecureCounterAlertLevel_Internal(bool bDBLocked, long nCounterID,
															long nAlertLevel, long nAlertMultiple)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				string strQuery = Util::Format(
					"UPDATE [SecureCounter] SET [AlertLevel] = %d, [AlertMultiple] = %d  "
					"	WHERE [ID] = %d", nAlertLevel, nAlertMultiple, nCounterID);
				
				executeCmdQuery(ipConnection, strQuery);
				
			END_CONNECTION_RETRY(ipConnection, "ELI39126");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI39127");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::AddFileNoQueue_Internal(bool bDBLocked, BSTR bstrFile, long long llFileSize,
												long lPageCount, EFilePriority ePriority, long nWorkflowID, 
												long* pnID)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI39584", pnID != __nullptr);

			// Replace any occurrences of ' with '' this is because SQL Server use the ' to indicate
			// the beginning and end of a string
			string strFileName = asString(bstrFile);
			replaceVariable(strFileName, "'", "''");

			string strSQL = Util::Format(
				"INSERT INTO FAMFile ([FileName], [FileSize], [Pages], [Priority]) "
					"OUTPUT INSERTED.[ID] "
					"VALUES ('%s', %lld, %d, %d)", strFileName.c_str(), llFileSize, lPageCount, ePriority);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Do not allow adding of files to all workflows via AddFile.
				if (nWorkflowID <= 0 && m_bRunningAllWorkflows)
				{
					UCLIDException ue("ELI43539", "Workflow has not been set.");
					ue.addDebugInfo("FPS File", m_strFPSFileName, false);
					throw ue;
				}

				if (nWorkflowID <= 0 && m_bUsingWorkflowsForCurrentAction)
				{
					nWorkflowID = getActiveWorkflowID(ipConnection);
				}

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_criticalSection);

				long nID = -1;
				executeCmdQuery(ipConnection, strSQL, false, &nID);

				// Update QueueEvent table if enabled
				if (m_bUpdateQueueEventTable)
				{
					// add a new QueueEvent record 
					addQueueEventRecord(ipConnection, nID, -1, asString(bstrFile), "P", llFileSize);
				}

				// -1 = not in WorkflowFile, 0 = marked Invisible in WorkflowFile
				if (nWorkflowID > 0 && isFileInWorkflow(ipConnection, nID, nWorkflowID) == -1)
				{
					// In the case that the file did exist in the DB, but not the workflow, the
					// [WorkflowFile] row will be added as part of the setStatusForFile call.
					executeCmdQuery(ipConnection, Util::Format(
						"INSERT INTO [WorkflowFile] ([WorkflowID], [FileID]) VALUES (%d,%d)",
						nWorkflowID, nID));
				}

				// Commit the changes to the database
				tg.CommitTrans();

				*pnID = nID;

			END_CONNECTION_RETRY(ipConnection, "ELI39573");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI39574");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		ue.addDebugInfo("FileName", asString(bstrFile));
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::AddPaginationHistory_Internal(bool bDBLocked, long nOutputFileID,
													  IIUnknownVector* pSourcePageInfo,
													  IIUnknownVector* pDeletedSourcePageInfo,
													  long nFileTaskSessionID)
{
	try
	{
		try
		{
			IIUnknownVectorPtr ipSourcePageInfo(pSourcePageInfo);
			IIUnknownVectorPtr ipDeletedSourcePageInfo(pDeletedSourcePageInfo);
			ASSERT_ARGUMENT("ELI39683", ipSourcePageInfo != __nullptr
										|| ipDeletedSourcePageInfo != __nullptr);

			// Compile selection queries that will produce a result set with the corresponding
			// source and destination pages for all pages in the output document.
			vector<string> vecPageSelections;
			if (ipSourcePageInfo != __nullptr)
			{
				long nCount = ipSourcePageInfo->Size();
				for (long i = 0; i < nCount; i++)
				{
					IStringPairPtr ipPageInfo = ipSourcePageInfo->At(i);
					ASSERT_RESOURCE_ALLOCATION("ELI39688", ipPageInfo != nullptr);

					string strSourceDocName = asString(ipPageInfo->StringKey);
					replaceVariable(strSourceDocName, "'", "''");
					string strPageNum = asString(ipPageInfo->StringValue);
					string strDestPage = (nOutputFileID <= 0) ? "NULL" : asString(i + 1);

					string strPageSelection = gstrSELECT_SINGLE_PAGINATED_PAGE;
					replaceVariable(strPageSelection, "<SourceFileName>", strSourceDocName);
					replaceVariable(strPageSelection, "<SourcePage>", strPageNum);
					replaceVariable(strPageSelection, "<DestPage>", strDestPage);

					vecPageSelections.push_back(strPageSelection);
				}
			}

			if (ipDeletedSourcePageInfo != __nullptr)
			{
				long nCount = ipDeletedSourcePageInfo->Size();
				for (long i = 0; i < nCount; i++)
				{
					IStringPairPtr ipPageInfo = ipDeletedSourcePageInfo->At(i);
					ASSERT_RESOURCE_ALLOCATION("ELI45365", ipPageInfo != nullptr);

					string strSourceDocName = asString(ipPageInfo->StringKey);
					replaceVariable(strSourceDocName, "'", "''");
					string strPageNum = asString(ipPageInfo->StringValue);
					string strDestPage = "NULL";

					string strPageSelection = gstrSELECT_SINGLE_PAGINATED_PAGE;
					replaceVariable(strPageSelection, "<SourceFileName>", strSourceDocName);
					replaceVariable(strPageSelection, "<SourcePage>", strPageNum);
					replaceVariable(strPageSelection, "<DestPage>", strDestPage);

					vecPageSelections.push_back(strPageSelection);
				}
			}

			// Use this data in gstrINSERT_INTO_PAGINATION which will compute the OriginalFileID
			// and OriginalPage columns for all of the new data.
			string strSQL = gstrINSERT_INTO_PAGINATION;
			replaceVariable(strSQL, "<DestFileID>", (nOutputFileID <= 0) ? "NULL" : asString(nOutputFileID));
			replaceVariable(strSQL, "<SelectPaginations>", asString(vecPageSelections, false, "\r\nUNION\r\n"));
			replaceVariable(strSQL, "<FAMSessionID>", asString(nFileTaskSessionID));

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_criticalSection);

				executeCmdQuery(ipConnection, strSQL);

				// Commit the changes to the database
				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI39684");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI39685");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		ue.addDebugInfo("PaginatedFileID", nOutputFileID);
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::AddWorkflow_Internal(bool bDBLocked, BSTR bstrName, EWorkflowType eType, long* pnID)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI41877", pnID != __nullptr);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();
			validateDBSchemaVersion();

			TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_criticalSection);

			m_mapWorkflowDefinitions.clear();

			const char* szWorkflowType = "U";
			switch (eType)
			{
				case kUndefined: szWorkflowType = "U"; break;
				case kRedaction: szWorkflowType = "R"; break;
				case kExtraction: szWorkflowType = "E"; break;
				case kClassification: szWorkflowType = "C"; break;
				default: break;
			}

			executeCmdQuery(ipConnection,
				Util::Format(
					"INSERT INTO [Workflow] ([Name], [WorkflowTypeCode]) "
					"	OUTPUT INSERTED.[ID]"
					"	VALUES('%s', '%s')", asString(bstrName).c_str(), szWorkflowType)
				, false, pnID);

			tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI41878");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI41879");
	}
	catch (UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		ue.addDebugInfo("WorkflowName", asString(bstrName));
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::DeleteWorkflow_Internal(bool bDBLocked, long nID)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();
			validateDBSchemaVersion();

			TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_criticalSection);

			m_mapWorkflowDefinitions.clear();

			long nDeletedCount = executeCmdQuery(ipConnection,
				"DELETE FROM [Workflow] WHERE [ID] = " + asString(nID));

			if (nDeletedCount == 0)
			{
				UCLIDException ue("ELI41892", "Failed to delete workflow");
				ue.addDebugInfo("ID", nID);
				throw ue;
			}

			tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI41880");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI41881");
	}
	catch (UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		ue.addDebugInfo("WorkflowID", asString(nID));
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetWorkflowDefinition_Internal(bool bDBLocked, long nID,
	IWorkflowDefinition** ppWorkflowDefinition)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI41882", ppWorkflowDefinition != __nullptr);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();
			validateDBSchemaVersion();

			UCLID_FILEPROCESSINGLib::IWorkflowDefinitionPtr ipWorkflowDefinition =
				getWorkflowDefinition(ipConnection, nID);

			*ppWorkflowDefinition = (IWorkflowDefinition*)ipWorkflowDefinition.Detach();

			END_CONNECTION_RETRY(ipConnection, "ELI41883");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI41884");
	}
	catch (UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		ue.addDebugInfo("WorkflowID", asString(nID));
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::SetWorkflowDefinition_Internal(bool bDBLocked,
	IWorkflowDefinition* pWorkflowDefinition)
{
	try
	{
		try
		{
			UCLID_FILEPROCESSINGLib::IWorkflowDefinitionPtr ipWorkflowDefinition(pWorkflowDefinition);
			ASSERT_ARGUMENT("ELI41885", ipWorkflowDefinition != __nullptr);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();
			validateDBSchemaVersion();

			TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_criticalSection);

			m_mapWorkflowDefinitions.clear();

			_RecordsetPtr ipWorkflowSet(__uuidof(Recordset));
			ASSERT_RESOURCE_ALLOCATION("ELI41916", ipWorkflowSet != __nullptr);

			string strQuery =
				Util::Format(
					"SELECT [ID] "
					", [Name] "
					", [WorkflowTypeCode] "
					", [Description] "
					", [StartActionID] "
					", [EditActionID] "
					", [PostEditActionID] "
					", [EndActionID] "
					", [PostWorkflowActionID] "
					", [DocumentFolder] "
					", [OutputAttributeSetID] "
					", [OutputFileMetadataFieldID] "
					", [OutputFilePathInitializationFunction] "
					", [LoadBalanceWeight] "
					"	FROM [Workflow]"
					"	WHERE [ID] = %i", ipWorkflowDefinition->ID);

			ipWorkflowSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true),
				adOpenDynamic, adLockOptimistic, adCmdText);

			if (asCppBool(ipWorkflowSet->adoEOF))
			{
				UCLIDException ue("ELI41917", "Failed to get workflow definition.");
				ue.addDebugInfo("ID", ipWorkflowDefinition->ID);
				throw ue;
			}

			FieldsPtr ipFields = ipWorkflowSet->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI41918", ipFields != __nullptr);

			string strOldWorkflowName = getStringField(ipFields, "Name");

			// Update workflow name last so that action ID lookup by workflow name is unambiguous.

			switch (ipWorkflowDefinition->Type)
			{
				case UCLID_FILEPROCESSINGLib::kUndefined:	setStringField(ipFields, "WorkflowTypeCode", "U"); break;
				case UCLID_FILEPROCESSINGLib::kRedaction:	setStringField(ipFields, "WorkflowTypeCode", "R"); break;
				case UCLID_FILEPROCESSINGLib::kExtraction:	setStringField(ipFields, "WorkflowTypeCode", "E"); break;
				case UCLID_FILEPROCESSINGLib::kClassification: setStringField(ipFields, "WorkflowTypeCode", "C"); break;
			}
			
			setStringField(ipFields, "Description", asString(ipWorkflowDefinition->Description));

			if (ipWorkflowDefinition->StartAction.length() == 0)
			{
				setFieldToNull(ipFields, "StartActionID");
			}
			else
			{
				setLongField(ipFields, "StartActionID",
					getActionID(ipConnection, asString(ipWorkflowDefinition->StartAction), strOldWorkflowName));
			}

			if (ipWorkflowDefinition->EditAction.length() == 0)
			{
				setFieldToNull(ipFields, "EditActionID");
			}
			else
			{
				setLongField(ipFields, "EditActionID",
					getActionID(ipConnection, asString(ipWorkflowDefinition->EditAction), strOldWorkflowName));
			}

			if (ipWorkflowDefinition->PostEditAction.length() == 0)
			{
				setFieldToNull(ipFields, "PostEditActionID");
			}
			else
			{
				setLongField(ipFields, "PostEditActionID",
					getActionID(ipConnection, asString(ipWorkflowDefinition->PostEditAction), strOldWorkflowName));
			}

			if (ipWorkflowDefinition->EndAction.length() == 0)
			{
				setFieldToNull(ipFields, "EndActionID");
			}
			else
			{
				setLongField(ipFields, "EndActionID",
					getActionID(ipConnection, asString(ipWorkflowDefinition->EndAction), strOldWorkflowName));
			}

			if (ipWorkflowDefinition->PostWorkflowAction.length() == 0)
			{
				setFieldToNull(ipFields, "PostWorkflowActionID");
			}
			else
			{
				setLongField(ipFields, "PostWorkflowActionID",
					getActionID(ipConnection, asString(ipWorkflowDefinition->PostWorkflowAction), strOldWorkflowName));
			}
			
			setStringField(ipFields, "DocumentFolder", asString(ipWorkflowDefinition->DocumentFolder));
			
			string strOutputAttributeSet = asString(ipWorkflowDefinition->OutputAttributeSet);
			if (strOutputAttributeSet.empty())
			{
				setFieldToNull(ipFields, "OutputAttributeSetID");
			}
			else
			{
				string strQuery = Util::Format("SELECT [ID] FROM [dbo].[AttributeSetName] WHERE [Description]='%s'",
					strOutputAttributeSet.c_str());
				long long llOutputAttributeSetID = 0;
				executeCmdQuery(ipConnection, strQuery, "ID", false, &llOutputAttributeSetID);
				setLongLongField(ipFields, "OutputAttributeSetID", llOutputAttributeSetID);
			}

			string strOutputFileMetadataField = asString(ipWorkflowDefinition->OutputFileMetadataField);
			if (strOutputFileMetadataField.empty())
			{
				setFieldToNull(ipFields, "OutputFileMetadataFieldID");
			}
			else
			{
				string strQuery = Util::Format("SELECT [ID] FROM [dbo].[MetadataField] WHERE [Name]='%s'",
					strOutputFileMetadataField.c_str());
				long lOutputFileMetadataFieldID = 0;
				executeCmdQuery(ipConnection, strQuery, false, &lOutputFileMetadataFieldID);
				setLongField(ipFields, "OutputFileMetadataFieldID", lOutputFileMetadataFieldID);
			}

			setStringField(ipFields, "OutputFilePathInitializationFunction",
				asString(ipWorkflowDefinition->OutputFilePathInitializationFunction));

			setLongField(ipFields, "LoadBalanceWeight", ipWorkflowDefinition->LoadBalanceWeight);

			setStringField(ipFields, "Name", asString(ipWorkflowDefinition->Name));

			ipWorkflowSet->Update();
			
			tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI41886");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI41887");
	}
	catch (UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}

//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetWorkflows_Internal(bool bDBLocked,
	IStrToStrMap ** pmapWorkFlowNameToID)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();

			// Make sure the DB Schema is the expected version
			validateDBSchemaVersion();

			// Create StrToStrMap to return the list of workflows
			IStrToStrMapPtr ipWorkflows(CLSID_StrToStrMap);
			ASSERT_RESOURCE_ALLOCATION("ELI41937", ipWorkflows != __nullptr);
			
			ipWorkflows->CaseSensitive = VARIANT_FALSE;

			for each(pair<string, string> workflow in getWorkflowNamesAndIDs(ipConnection))
			{
				// Put the values in the StrToStrMap
				ipWorkflows->Set(workflow.first.c_str(), workflow.second.c_str());
			}

			// return the StrToStrMap containing all workflows
			*pmapWorkFlowNameToID = ipWorkflows.Detach();

			END_CONNECTION_RETRY(ipConnection, "ELI41939");

		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI41933");
	}
	catch (UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetWorkflowActions_Internal(bool bDBLocked, long nID,
													IIUnknownVector** pvecActions)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI41990", pvecActions != __nullptr);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();
			validateDBSchemaVersion();

			IIUnknownVectorPtr ipActions(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI41991", ipActions != __nullptr);

			vector<tuple<long, string, bool>> vecWorkflowActions = getWorkflowActions(ipConnection, nID);
			for each (tuple<long, string, bool> item in vecWorkflowActions)
			{
				IVariantVectorPtr ipProperties(CLSID_VariantVector);
				ASSERT_RESOURCE_ALLOCATION("ELI43303", ipProperties != __nullptr);

				ipProperties->PushBack(get<0>(item));
				ipProperties->PushBack(get<1>(item).c_str());
				ipProperties->PushBack(asVariantBool(get<2>(item)));

				ipActions->PushBack(ipProperties);
			}

			*pvecActions = ipActions.Detach();

			END_CONNECTION_RETRY(ipConnection, "ELI41994");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI41995");
	}
	catch (UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		ue.addDebugInfo("WorkflowID", asString(nID));
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::SetWorkflowActions_Internal(bool bDBLocked, long nID,
	IIUnknownVector* pActionList)
{
	try
	{
		try
		{
			IIUnknownVectorPtr ipActionList(pActionList);
			ASSERT_ARGUMENT("ELI41996", ipActionList != __nullptr);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();
			validateDBSchemaVersion();

			TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_criticalSection);

			m_mapActionIdsForActiveWorkflow.clear();

			long nActionCount = ipActionList->Size();
			vector<string> vecActionNames;
			vector<string> vecMainSequenceActions;
			for (long i = 0; i < nActionCount; i++)
			{
				IVariantVectorPtr ipActionInfo = ipActionList->At(i);
				ASSERT_RESOURCE_ALLOCATION("ELI43305", ipActionInfo != __nullptr);

				string strName = asString(ipActionInfo->Item[0].bstrVal);
				vecActionNames.push_back(strName);

				if (asCppBool(ipActionInfo->Item[1].boolVal))
				{
					vecMainSequenceActions.push_back(strName);
				}
			}
			string strActionList = asString(vecActionNames, true, "','");

			if (nActionCount > 0)
			{
				string strQueryActionsToAdd =
					Util::Format("SELECT DISTINCT [Action].[ASCName] "
						"FROM dbo.[Action] "
						"LEFT JOIN [Action][T2] ON [Action].[ASCName] = [T2].[ASCName] AND[T2].[WorkflowID] = %i "
						"WHERE [T2].[ASCName] IS NULL "
						"AND [Action].[ASCName] IN ('%s')",
						nID, strActionList.c_str());

				_RecordsetPtr ipActionsToAdd(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI41997", ipActionsToAdd != __nullptr);

				ipActionsToAdd->Open(strQueryActionsToAdd.c_str(),
					_variant_t((IDispatch *)ipConnection, true),
					adOpenStatic, adLockReadOnly, adCmdText);

				while (!asCppBool(ipActionsToAdd->adoEOF))
				{
					FieldsPtr ipFields = ipActionsToAdd->Fields;
					ASSERT_RESOURCE_ALLOCATION("ELI41998", ipFields != __nullptr);

					string strActionToAdd = getStringField(ipFields, "ASCName");

					string strAddActionQuery =
						Util::Format("INSERT INTO dbo.[Action] ([ASCName], [WorkflowID]) "
							"VALUES ('%s', %i)", strActionToAdd .c_str(), nID);

					executeCmdQuery(ipConnection, strAddActionQuery);

					ipActionsToAdd->MoveNext();
				}

				ipActionsToAdd->Close();
			}

			string strQueryActionsToDelete =
				Util::Format("SELECT DISTINCT [ASCName] "
					"FROM dbo.[Action] "
					"WHERE [WorkflowID] = %i "
					"AND [ASCName] NOT IN ('%s')",
					nID, strActionList.c_str());

			_RecordsetPtr ipActionsToDelete(__uuidof(Recordset));
			ASSERT_RESOURCE_ALLOCATION("ELI41999", ipActionsToDelete != __nullptr);

			ipActionsToDelete->Open(strQueryActionsToDelete.c_str(),
				_variant_t((IDispatch *)ipConnection, true),
				adOpenStatic, adLockReadOnly, adCmdText);

			vector<string> vecActionsToDelete;
			while (!asCppBool(ipActionsToDelete->adoEOF))
			{
				FieldsPtr ipFields = ipActionsToDelete->Fields;
				ASSERT_RESOURCE_ALLOCATION("ELI42000", ipFields != __nullptr);

				vecActionsToDelete.push_back(getStringField(ipFields, "ASCName"));

				ipActionsToDelete->MoveNext();
			}

			ipActionsToDelete->Close();

			if (vecActionsToDelete.size() > 0)
			{
				strActionList = asString(vecActionsToDelete, true, "','");

				string strDeleteActionQuery =
					Util::Format("DELETE FROM dbo.[Action] "
						"WHERE [WorkflowID] = %i "
						"AND [ASCName] IN ('%s')",
						nID, strActionList.c_str());

				long nAffectedRecs = executeCmdQuery(ipConnection, strDeleteActionQuery);
				ASSERT_RUNTIME_CONDITION("ELI42001", nAffectedRecs = nActionCount,
					"Error deleting workflow actions.");
			}

			if (nActionCount > 0)
			{
				string strMainSequenceActions = asString(vecMainSequenceActions, true, "','");

				string strUpdateMainSequenceQuery =
					Util::Format("UPDATE dbo.[Action] "
						"SET [MainSequence] = CASE WHEN ([ASCName] IN ('%s')) THEN 1 ELSE 0 END "
						"WHERE [WorkflowID] = %i ",
						strMainSequenceActions.c_str(), nID);

				long nAffectedRecs = executeCmdQuery(ipConnection, strUpdateMainSequenceQuery);
				ASSERT_RUNTIME_CONDITION("ELI43306", nAffectedRecs = nActionCount,
					"Error updating workflow actions.");
			}

			tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI42002");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI42003");
	}
	catch (UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetWorkflowStatus_Internal(bool bDBLocked, long nFileID, EActionStatus* peaStatus)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI42136", peaStatus != __nullptr);

			// When bReturnFileStatuses = false, return vector lists the file count for each file status.
			vector<tuple<long, string>> vecStatuses = getWorkflowStatus(nFileID, false);
			if (vecStatuses.empty())
			{
				*peaStatus = kActionUnattempted;
			}
			else
			{
				switch (get<1>(vecStatuses[0])[0])
				{
					case 'R': *peaStatus = kActionProcessing; break;
					case 'C': *peaStatus = kActionCompleted; break;
					case 'F': *peaStatus = kActionFailed; break;
					
					default: *peaStatus = kActionUnattempted;
				}
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI42143");
	}
	catch (UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetAggregateWorkflowStatus_Internal(bool bDBLocked, long *pnUnattempted,
											long *pnProcessing, long *pnCompleted, long *pnFailed)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI42154", pnUnattempted != __nullptr);
			ASSERT_ARGUMENT("ELI42155", pnProcessing != __nullptr);
			ASSERT_ARGUMENT("ELI42156", pnCompleted != __nullptr);
			ASSERT_ARGUMENT("ELI42157", pnFailed != __nullptr);

			*pnUnattempted = 0;
			*pnProcessing = 0;
			*pnCompleted = 0;
			*pnFailed = 0;

			// When bReturnFileStatuses = false, return vector lists the file count for each file status.
			vector<tuple<long, string>> vecStatuses = getWorkflowStatus(-1, false);
			for each (tuple<long, string> status in vecStatuses)
			{
				switch (get<1>(status)[0])
				{
					case 'U': *pnUnattempted = get<0>(status); break;
					case 'R': *pnProcessing = get<0>(status); break;
					case 'C': *pnCompleted = get<0>(status); break;
					case 'F': *pnFailed = get<0>(status); break;
				}
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI42158");
	}
	catch (UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetWorkflowStatusAllFiles_Internal(bool bDBLocked, BSTR *pbstrStatusListing)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI46409", pbstrStatusListing != __nullptr);

			string strStatusListing;

			// When bReturnFileStatuses = true, return vector lists a file ID and associated status
			vector<tuple<long, string>> vecStatuses = getWorkflowStatus(-1, true);

			for each (tuple<long, string> status in vecStatuses)
			{
				string strStatusString = asString(get<0>(status)) + ":" + get<1>(status)[0] + ",";
				strStatusListing += strStatusString;
			}

			*pbstrStatusListing = get_bstr_t(strStatusListing.c_str()).Detach();
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI46410");
	}
	catch (UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetWorkflowID_Internal(bool bDBLocked, BSTR bstrWorkflowName, long *pnID)
{
	try
	{
		try
		{
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();
				validateDBSchemaVersion();

				*pnID = getWorkflowID(ipConnection, asString(bstrWorkflowName));

			END_CONNECTION_RETRY(ipConnection, "ELI43219");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI43220");
	}
	catch (UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::IsFileInWorkflow_Internal(bool bDBLocked, long nFileID, long nWorkflowID,
	VARIANT_BOOL *pbIsInWorkflow)
{
	try
	{
		try
		{
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();
			validateDBSchemaVersion();

			// -1 = not in WorkflowFile, 0 = marked Invisible in WorkflowFile, 1 = in workflow, not marked Invisible
			*pbIsInWorkflow = asVariantBool(isFileInWorkflow(ipConnection, nFileID, nWorkflowID) == 1);

			END_CONNECTION_RETRY(ipConnection, "ELI43221");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI43222");
	}
	catch (UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetUsingWorkflows_Internal(bool bDBLocked, VARIANT_BOOL *pbUsingWorkflows)
{
	try
	{
		try
		{
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();

			*pbUsingWorkflows = asVariantBool(databaseUsingWorkflows(ipConnection));

			END_CONNECTION_RETRY(ipConnection, "ELI43229");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI43230");
	}
	catch (UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetWorkflowNameFromActionID_Internal(bool bDBLocked, long nActionID, BSTR * pbstrWorkflowName)
{
		try
	{
		try
		{
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();

			string strQuery = "SELECT COALESCE( Workflow.Name, '') AS [Name] ";
			strQuery += "FROM  Action LEFT JOIN ";
			strQuery += "Workflow ON Action.WorkflowID = Workflow.ID ";
			strQuery += "WHERE Action.ID = " + asString(nActionID);

			_RecordsetPtr ipWorkflowNameSet(__uuidof(Recordset));
			ASSERT_RESOURCE_ALLOCATION("ELI43301", ipWorkflowNameSet != __nullptr);

			ipWorkflowNameSet->Open(strQuery.c_str(), _variant_t((IDispatch*)ipConnection, true), adOpenStatic,
				adLockReadOnly, adCmdText);

			// There should be at least one record
			if (!asCppBool(ipWorkflowNameSet->adoEOF))
			{
				string workflowName = getStringField(ipWorkflowNameSet->Fields, "Name");
				*pbstrWorkflowName = _bstr_t(workflowName.c_str()).Detach();
				return true;
			}
			UCLIDException ue("ELI43304", "Unable to obtain workflow name.");
			ue.addDebugInfo("ActionID", asString(nActionID));
			// set bDBLocked to true so that the exception will be thrown instead of a retry with lock
			bDBLocked = true;
			throw ue;

			END_CONNECTION_RETRY(ipConnection, "ELI43297");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI43298");
	}
	catch (UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetActionIDForWorkflow_Internal(bool bDBLocked, BSTR bstrActionName,
														long nWorkflowID, long* pnActionID)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI43312", pnActionID != __nullptr);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();

			// Get the action ID
			*pnActionID = getActionIDNoThrow(ipConnection, asString(bstrActionName), nWorkflowID);

			END_CONNECTION_RETRY(ipConnection, "ELI43313");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI43314");
	}
	catch (UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::MoveFilesToWorkflowFromQuery_Internal(bool bDBLocked, BSTR bstrQuery, 
	long nSourceWorkflowID, long nDestWorkflowID, long *pnCount)
{
	try
	{
		try
		{
			string strQueryFrom = asString(bstrQuery);
			ASSERT_ARGUMENT("ELI43405", !strQueryFrom.empty());
			ASSERT_ARGUMENT("ELI43543", pnCount != __nullptr);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();

			TransactionGuard tg(ipConnection, adXactIsolated, &m_criticalSection);

			// Create the #SelectedFilesToMove temp table;
			createTempTableOfSelectedFiles(ipConnection, strQueryFrom);

			// Query to add the new WorkflowChange record
			string strWorkflowChangeQuery = Util::Format(
				"INSERT INTO[dbo].[WorkflowChange] ([DestWorkflowID]) "
				"OUTPUT INSERTED.ID "
				"VALUES (%li);", nDestWorkflowID);

			long nWorkflowChangeID = 0;
			executeCmdQuery(ipConnection, strWorkflowChangeQuery, false, &nWorkflowChangeID);

			string strSourceWorkflowSelection;
			string strWorkflowFileSelection = "";
			switch (nSourceWorkflowID)
			{
			case -1: // No workflow
				strSourceWorkflowSelection = "[Action].WorkflowID IS NULL ";
				strWorkflowFileSelection = "WHERE WF.WorkflowID IS NULL";
				break;
			case 0: // All workflows
				strSourceWorkflowSelection = 
					Util::Format("[Action].WorkflowID IS NOT NULL AND [Action].WorkflowID <> %li", nDestWorkflowID);
				strWorkflowFileSelection = "WHERE WF.WorkflowID IS NOT NULL";
				break;
			default:
				strSourceWorkflowSelection = Util::Format("[Action].WorkflowID = %li", nSourceWorkflowID);
				strWorkflowFileSelection = Util::Format("WHERE WF.WorkflowID = %li", nSourceWorkflowID);
			}

			string strDestWorkflowSelection = Util::Format("DA.WorkflowID = %li", nDestWorkflowID);

			string strSelectionFrom = Util::Format(
				"FROM #SelectedFilesToMove AS SQ \r\n"
				"CROSS JOIN  (SELECT * FROM [Action] WHERE %s) AS SA \r\n"
				"LEFT JOIN [Action] AS DA ON SA.ASCName = DA.ASCName AND %s \r\n"
				"LEFT JOIN [WorkflowFile] AS WF ON WF.FileID = SQ.ID %s \r\n",
				strSourceWorkflowSelection.c_str(), strDestWorkflowSelection.c_str(), strWorkflowFileSelection.c_str());

			verifyDestinationActions(ipConnection, strSelectionFrom);

			string strSelectedFilesWithSourceAndDest = Util::Format(
				"SELECT DISTINCT \r\n"
				"	SQ.ID as FileID, %li as WorkflowChangeID, SA.ID AS SourceActionID, DA.ID AS DestActionID, \r\n"
				"	WF.WorkflowID AS SourceWorkflowID, %li AS DestWorkflowID \r\n"
				"%s",
				nWorkflowChangeID, nDestWorkflowID, strSelectionFrom.c_str());

			// Add files to the workflowChangeFile table
			string strWorkflowChangeFile = Util::Format(
				"INSERT INTO [dbo].[WorkflowChangeFile] "
				"([FileID] "
				"	, [WorkflowChangeID] "
				"	, [SourceActionID] "
				"	, [DestActionID] "
				"	, [SourceWorkflowID] "
				"	, [DestWorkflowID]) %s ", strSelectedFilesWithSourceAndDest.c_str());

			executeCmdQuery(ipConnection, strWorkflowChangeFile);

			// Get the count of files being moved.
			executeCmdQuery(ipConnection, Util::Format(
				"SELECT COUNT(DISTINCT(FileID)) AS ID FROM [WorkflowChangeFile] WHERE WorkflowChangeID = %li",
				nWorkflowChangeID), false, pnCount);

			vector<string> vecUpdateQueries;
		    
			if (nSourceWorkflowID == -1)
			{
				vecUpdateQueries.push_back(Util::Format(
					"INSERT INTO [dbo].[WorkflowFile] "
					"([WorkflowID] "
					"	, [FileID]) "
					"SELECT DISTINCT DestWorkflowID, FileID "
					"FROM WorkflowChangeFile "
					"WHERE WorkflowChangeID = %li;", nWorkflowChangeID));
			}
			else
			{
				vecUpdateQueries.push_back(Util::Format(
					"UPDATE       [dbo].WorkflowFile "
					"SET                WorkflowID = WorkflowChangeFile.DestWorkflowID "
					"FROM            WorkflowChangeFile INNER JOIN "
					"WorkflowFile "
					"ON WorkflowChangeFile.FileID = WorkflowFile.FileID "
					"AND WorkflowChangeFile.SourceWorkflowID = WorkflowFile.WorkflowID "
					"WHERE (WorkflowChangeFile.WorkflowChangeID = %li); ", nWorkflowChangeID));
			}
			
			// FileActionStatus table
			vecUpdateQueries.push_back(Util::Format(
				"UPDATE [dbo].[FileActionStatus] "
				"	SET [ActionID] = WorkflowChangeFile.DestActionID "
				"FROM WorkflowChangeFile INNER JOIN FileActionStatus "
				"	ON WorkflowChangeFile.SourceActionID = FileActionStatus.ActionID "
				"		AND WorkflowChangeFile.FileID = FileActionStatus.FileID "
				"WHERE (WorkflowChangeFile.WorkflowChangeID = %li);", nWorkflowChangeID));

			// FAMSession - May need special handling 
			// To simplify, just change the FAMSession ActionID to the no assigned workflow action id
			vecUpdateQueries.push_back(Util::Format(
				"UPDATE [dbo].[FAMSession] "
				"SET [ActionID] = A2.[ID] "
				"FROM [FAMSession] "
				"INNER JOIN [FileTaskSession] ON [FAMSession].ID = [FileTaskSession].FAMSessionID "
				"INNER JOIN [WorkflowChangeFile] ON [WorkflowChangeFile].FileID = [FileTaskSession].FileID "
				"AND [WorkflowChangeFile].WorkflowChangeID = %li "
				"AND [FileTaskSession].ActionID = [WorkflowChangeFile].SourceActionID "
				"INNER JOIN [Action] AS A1 "
				"ON A1.[ID] = [FAMSession].ActionID INNER JOIN "
				"[Action] AS A2 ON A1.[ASCName] = A2.[ASCName] AND A2.[WorkflowID] IS NULL ", 
				nWorkflowChangeID));

			// FileTaskSession
			vecUpdateQueries.push_back(Util::Format(
				"UPDATE       FileTaskSession "
				"SET                ActionID = WorkflowChangeFile.DestActionID "
				"FROM            FileTaskSession INNER JOIN "
				"WorkflowChangeFile "
				"ON FileTaskSession.FileID = WorkflowChangeFile.FileID "
				"AND FileTaskSession.ActionID = WorkflowChangeFile.SourceActionID "
				"WHERE (WorkflowChangeFile.WorkflowChangeID = %li);", nWorkflowChangeID));

			// FileActionComment
			vecUpdateQueries.push_back(Util::Format(
				"UPDATE [dbo].[FileActionComment] "
				"	SET [ActionID] = WorkflowChangeFile.DestActionID "
				"FROM WorkflowChangeFile INNER JOIN "
				"	FileActionComment ON WorkflowChangeFile.FileID = FileActionComment.FileID "
				"	AND WorkflowChangeFile.SourceActionID = FileActionComment.ActionID "
				"WHERE (WorkflowChangeFile.WorkflowChangeID = %li);", nWorkflowChangeID));

			// FileActionStateTransistion
			vecUpdateQueries.push_back(Util::Format(
				"UPDATE     [dbo].FileActionStateTransition "
				"	SET                ActionID = WorkflowChangeFile.DestActionID "
				"FROM            WorkflowChangeFile INNER JOIN "
				"	FileActionStateTransition ON WorkflowChangeFile.FileID = FileActionStateTransition.FileID "
				"		AND FileActionStateTransition.ActionID = WorkflowChangeFile.SourceActionID "
				"WHERE (WorkflowChangeFile.WorkflowChangeID = %li);", nWorkflowChangeID));

			// FTPEventHistory
			vecUpdateQueries.push_back(Util::Format(
				"UPDATE       FTPEventHistory "
				"SET                ActionID = WorkflowChangeFile.DestActionID "
				"FROM            FTPEventHistory INNER JOIN "
				"WorkflowChangeFile ON FTPEventHistory.FileID = WorkflowChangeFile.FileID "
				"AND WorkflowChangeFile.SourceActionID = FTPEventHistory.ActionID "
				"WHERE (WorkflowChangeFile.WorkflowChangeID = %li);", nWorkflowChangeID));


			// QueuedActionStatusChange
			vecUpdateQueries.push_back(Util::Format(
				"UPDATE       QueuedActionStatusChange "
				"SET                ActionID = WorkflowChangeFile.DestActionID "
				"FROM            QueuedActionStatusChange INNER JOIN "
				"WorkflowChangeFile "
				"ON QueuedActionStatusChange.FileID = WorkflowChangeFile.FileID "
				"AND QueuedActionStatusChange.ActionID = WorkflowChangeFile.SourceActionID "
				"WHERE (WorkflowChangeFile.WorkflowChangeID = %li);", nWorkflowChangeID));

			// QueueEvent
			vecUpdateQueries.push_back(Util::Format(
				"UPDATE       QueueEvent "
				"SET                ActionID = WorkflowChangeFile.DestActionID "
				"FROM            QueueEvent INNER JOIN "
				"WorkflowChangeFile "
				"ON QueueEvent.FileID = WorkflowChangeFile.FileID "
				"AND QueueEvent.ActionID = WorkflowChangeFile.SourceActionID "
				"WHERE (WorkflowChangeFile.WorkflowChangeID = %li);", nWorkflowChangeID));

			// SkippedFile
			vecUpdateQueries.push_back(Util::Format(
				"UPDATE       SkippedFile "
				"SET                ActionID = WorkflowChangeFile.DestActionID "
				"FROM            SkippedFile INNER JOIN "
				"WorkflowChangeFile "
				"ON SkippedFile.FileID = WorkflowChangeFile.FileID "
				"AND SkippedFile.ActionID = WorkflowChangeFile.SourceActionID "
				"WHERE (WorkflowChangeFile.WorkflowChangeID = %li);", nWorkflowChangeID));

			// WorkItemGroup 
			vecUpdateQueries.push_back(Util::Format(
				"UPDATE       WorkItemGroup "
				"SET                ActionID = WorkflowChangeFile.DestActionID "
				"FROM            WorkItemGroup INNER JOIN "
				"WorkflowChangeFile "
				"ON WorkItemGroup.FileID = WorkflowChangeFile.FileID "
				"AND WorkItemGroup.ActionID = WorkflowChangeFile.SourceActionID "
				"WHERE (WorkflowChangeFile.WorkflowChangeID = %li);", nWorkflowChangeID));

			executeVectorOfSQL(ipConnection, vecUpdateQueries);

			// Recalculate the action statistics - May be able to get a list of all the source and dest actions and 
			// recalculate only those actions.
			getThisAsCOMPtr()->RecalculateStatistics();

			tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI43401");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI43402");
	}
	catch (UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetAttributeValue_Internal(bool bDBLocked, BSTR bstrSourceDocName,
								BSTR bstrAttributeSetName, BSTR bstrAttributePath, BSTR* pbstrValue)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI43518", pbstrValue != __nullptr);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();

			_RecordsetPtr ipResult(__uuidof(Recordset));
			ASSERT_RESOURCE_ALLOCATION("ELI43521", ipResult != __nullptr);

            auto cmd = buildCmd(ipConnection, gstrGET_ATTRIBUTE_VALUE,
            {
                {"@SourceDocName", bstrSourceDocName},
                {"@AttributeSetName", bstrAttributeSetName},
                {"@AttributePath", bstrAttributePath}
            });

			ipResult->Open((IDispatch*)cmd, vtMissing, adOpenStatic,
				adLockReadOnly, adCmdText);

			if (!asCppBool(ipResult->adoEOF))
			{
				string strAttributeValue = getStringField(ipResult->Fields, "Value");
				*pbstrValue = _bstr_t(strAttributeValue.c_str()).Detach();
			}
			else
			{
				*pbstrValue = _bstr_t("").Detach();
			}

			END_CONNECTION_RETRY(ipConnection, "ELI43519");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI43520");
	}
	catch (UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::IsFileNameInWorkflow_Internal(bool bDBLocked, BSTR bstrFileName,
													  long nWorkflowID, VARIANT_BOOL *pbIsInWorkflow)
{
	try
	{
		try
		{
			ADODB::_ConnectionPtr ipConnection = __nullptr;
			
			BEGIN_CONNECTION_RETRY();

			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();
			validateDBSchemaVersion();

			// -1 = not in WorkflowFile, 0 = marked Invisible in WorkflowFile, 1 = in workflow, not marked Invisible
			*pbIsInWorkflow = asVariantBool(
				isFileInWorkflow(ipConnection, asString(bstrFileName), nWorkflowID) == 1);

			END_CONNECTION_RETRY(ipConnection, "ELI44847");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI44848");
	}
	catch (UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::SaveWebAppSettings_Internal(bool bDBLocked, long nWorkflowID, BSTR bstrType,
												    BSTR bstrSettings)
{
	try
	{
		try
		{
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();
			validateDBSchemaVersion();

			TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_criticalSection);

			string strType = asString(bstrType);
			replaceVariable(strType, "'", "''");

			string strSettings = asString(bstrSettings);
			replaceVariable(strSettings, "'", "''");

			string strQuery;
			if (strSettings.empty())
			{
				strQuery = Util::Format(
					"DELETE FROM dbo.[WebAppConfig]"
					"	WHERE [Type] = '%s' "
					"	AND [WorkflowID] = %d",
					strType.c_str(), nWorkflowID);
			}
			else
			{
				strQuery = Util::Format(
					"UPDATE dbo.[WebAppConfig] SET [Settings] = '%s' "
					"	WHERE [Type] = '%s' "
					"	AND [WorkflowID] = %d "
					"	IF @@ROWCOUNT = 0 "
					"	INSERT INTO dbo.[WebAppConfig] ([Type], [WorkflowID], [Settings]) "
					"	VALUES('%s', %d, '%s')",
					strSettings.c_str(), strType.c_str(), nWorkflowID,
					strType.c_str(), nWorkflowID, strSettings.c_str());
			}

			executeCmdQuery(ipConnection, strQuery);

			tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI45060");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI45061");
	}
	catch (UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::LoadWebAppSettings_Internal(bool bDBLocked, long nWorkflowID, BSTR bstrType,
	BSTR *pbstrSettings)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI45070", pbstrSettings != __nullptr); 

			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();
			validateDBSchemaVersion();

			if (nWorkflowID <= 0)
			{
				nWorkflowID = getActiveWorkflowID(ipConnection);
			}

			string strSettings = getWebAppSettings(ipConnection, nWorkflowID, asString(bstrType));

			*pbstrSettings = _bstr_t(strSettings.c_str()).Detach();

			END_CONNECTION_RETRY(ipConnection, "ELI50052");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI50053");
	}
	catch (UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}

	return true;
}

//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::DefineNewMLModel_Internal(bool bDBLocked, BSTR bstrMLModel, long* pnID)
{
	try
	{
		try
		{
			string strMLModelName = asString(bstrMLModel);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Begin a transMLModel
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				*pnID = getKeyID(ipConnection, "MLModel", "Name", strMLModelName);

				// Commit this transMLModel
				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI45044");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI45046");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::DeleteMLModel_Internal(bool bDBLocked, BSTR bstrMLModel)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Begin a transMLModel
				TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_criticalSection);

				// Delete the MLModel
				string strMLModelName = asString(bstrMLModel);
				replaceVariable(strMLModelName, "'", "''");

				string strDeleteMLModelQuery = "DELETE [MLModel] WHERE [Name] = '" + strMLModelName + "'";
				executeCmdQuery(ipConnection, strDeleteMLModelQuery);

				// Commit this transMLModel
				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI45050");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI45066");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetMLModels_Internal(bool bDBLocked, IStrToStrMap * * pmapModelNameToID)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Create a pointer to a recordset
				_RecordsetPtr ipModelSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI45119", ipModelSet != __nullptr);

				string strQuery = "SELECT * FROM [MLModel]";

				ipModelSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenStatic,
					adLockReadOnly, adCmdText);

				// Create StrToStrMap to return the list of models
				IStrToStrMapPtr ipModels(CLSID_StrToStrMap);
				ASSERT_RESOURCE_ALLOCATION("ELI45120", ipModels != __nullptr);

				ipModels->CaseSensitive = VARIANT_FALSE;

				// Step through all records
				while (ipModelSet->adoEOF == VARIANT_FALSE)
				{
					// Get the fields from the record set
					FieldsPtr ipFields = ipModelSet->Fields;
					ASSERT_RESOURCE_ALLOCATION("ELI45121", ipFields != __nullptr);

					// get the model name
					string strModelName = getStringField(ipFields, "Name");

					// get the model ID
					long lID = getLongField(ipFields, "ID");
					string strID = asString(lID);

					// Put the values in the StrToStrMap
					ipModels->Set(strModelName.c_str(), strID.c_str());

					// Move to the next record in the table
					ipModelSet->MoveNext();
				}

				// return the StrToStrMap containing all models
				*pmapModelNameToID = ipModels.Detach();

			END_CONNECTION_RETRY(ipConnection, "ELI45122");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI45123");
	}
	catch(UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetActiveUsers_Internal(bool bDBLocked, BSTR bstrAction,
												IVariantVector** ppvecUserNames)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();

			string strQuery = "SELECT DISTINCT [UserName] "
				"FROM [ActiveFAM] WITH (NOLOCK) "
				"INNER JOIN [FAMSession] WITH (NOLOCK) ON [FAMSessionID] = [FAMSession].[ID] "
				"INNER JOIN [FAMUser] ON [FAMUserID] = [FAMUser].[ID] "
				"WHERE [ActionID] = <ActionID> AND [Processing] = 1";

			// <All workflows> is not valid for a verification task, so this will be a single ID.
			string strActionID = getActionIDsForActiveWorkflow(ipConnection, asString(bstrAction));

			replaceVariable(strQuery, "<ActionID>", strActionID);

			// Recordset to contain the files to process
			_RecordsetPtr ipUserNames(__uuidof(Recordset));
			ASSERT_RESOURCE_ALLOCATION("ELI45528", ipUserNames != __nullptr);

			ipUserNames->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenForwardOnly,
				adLockReadOnly, adCmdText);

			IVariantVectorPtr ipVecUserNames(CLSID_VariantVector);
			ASSERT_RESOURCE_ALLOCATION("ELI45529", ipVecUserNames != __nullptr);

			while (ipUserNames->adoEOF == VARIANT_FALSE)
			{
				FieldsPtr ipFields = ipUserNames->Fields;
				ASSERT_RESOURCE_ALLOCATION("ELI45530", ipFields != __nullptr);

				string strUserName = getStringField(ipFields, "UserName");

				ipVecUserNames->PushBack(strUserName.c_str());

				ipUserNames->MoveNext();
			}

			*ppvecUserNames = ipVecUserNames.Detach();

			END_CONNECTION_RETRY(ipConnection, "ELI45531");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI45532");
	}
	catch (UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::AbortFAMSession_Internal(bool bDBLocked, long nFAMSessionID)
{
	try
	{
		try
		{
			if (m_bFAMRegistered && m_nFAMSessionID == nFAMSessionID)
			{
				UCLIDException ue("ELI46244", "Unexpected session abort for active session.");
				ue.addDebugInfo("FAMSessionID", m_nFAMSessionID);
				throw ue;
			}

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();

			string strQueryActiveFAM = "SELECT [ID] FROM [ActiveFAM] WHERE [FAMSessionID] = " + asString(nFAMSessionID);

			_RecordsetPtr ipActiveFAMSet(__uuidof(Recordset));
			ASSERT_RESOURCE_ALLOCATION("ELI46247", ipActiveFAMSet != __nullptr);

			ipActiveFAMSet->Open(strQueryActiveFAM.c_str(),
				_variant_t((IDispatch *)ipConnection, true), adOpenStatic, adLockReadOnly, adCmdText);

			if (ipActiveFAMSet->adoEOF == VARIANT_FALSE)
			{
				FieldsPtr ipFields = ipActiveFAMSet->Fields;
				long nActiveFAMID = getLongField(ipFields, "ID");

				TransactionGuard tg(role->ADOConnection(), adXactRepeatableRead, &m_criticalSection);

				UCLIDException uex("ELI46249", "Application Trace: Files were reverted to original status.");
				revertLockedFilesToPreviousState(role->ADOConnection(), nActiveFAMID,
					"FAM session is being reactivated.", &uex);

				tg.CommitTrans();

				UCLIDException ue("ELI46242",
					"Application trace: FAM session has been aborted.");
				ue.addDebugInfo("FAMSession ID", m_nFAMSessionID);
				ue.addDebugInfo("ActiveFAM ID", m_nActiveFAMID);
				ue.log();
			}

			END_CONNECTION_RETRY(ipConnection, "ELI46241");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI50067");
	}
	catch (UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::MarkFileDeleted_Internal(bool bDBLocked, long nFileID, long nWorkflowID)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();

			TransactionGuard tg(ipConnection, adXactIsolated, &m_criticalSection);

			string strMarkDeletedQuery = Util::Format(
				"UPDATE WorkflowFile SET Invisible = 1 WHERE Invisible = 0 AND FileID = %d AND WorkflowID = %d",
				nFileID, nWorkflowID);

			long nAffected = executeCmdQuery(ipConnection, strMarkDeletedQuery.c_str(), false);

			// Update the action statistics for this file
			// AppBackendAPI - Number of pending documents/pages should take into account deleted documents/pages
			// https://extract.atlassian.net/browse/ISSUE-16044
			if (nAffected == 1)
			{
				// Get a FileRecord for this file
				UCLID_FILEPROCESSINGLib::IFileRecordPtr ipFileRecord(CLSID_FileRecord);
				ASSERT_RESOURCE_ALLOCATION("ELI51613", ipFileRecord != __nullptr);
				{
					_RecordsetPtr ipFileSet(__uuidof(Recordset));
					ASSERT_RESOURCE_ALLOCATION("ELI51611", ipFileSet != __nullptr);

					string strFileSQL = Util::Format("SELECT * FROM FAMFile WHERE ID = %d", nFileID);
					ipFileSet->Open(strFileSQL.c_str(), _variant_t((IDispatch*)ipConnection, true), adOpenStatic,
						adLockOptimistic, adCmdText);

					FieldsPtr ipFields = ipFileSet->Fields;
					ASSERT_RESOURCE_ALLOCATION("ELI51612", ipFields != __nullptr);

					// Set pages and file size from the results
					// (none of the other data is used by updateStats)
					ipFileRecord->SetFileData(
						0, 0, "",
						getLongLongField(ipFields, "FileSize"),
						getLongField(ipFields, "Pages"),
						UCLID_FILEPROCESSINGLib::kPriorityNormal, 0);
					ipFileSet->Close();
				}

				// Get all the actions for the specified workflow
				vector<long> actions;
				{
					string strActionQuery = Util::Format("SELECT ID FROM Action WHERE WorkflowID = %d", nWorkflowID);

					_RecordsetPtr ipActionSet(__uuidof(Recordset));
					ASSERT_RESOURCE_ALLOCATION("ELI46706", ipActionSet != __nullptr);

					ipActionSet->Open(strActionQuery.c_str(),
						_variant_t((IDispatch*)ipConnection, true), adOpenStatic, adLockReadOnly, adCmdText);

					while (ipActionSet->adoEOF == VARIANT_FALSE)
					{
						FieldsPtr ipFields = ipActionSet->Fields;
						actions.push_back(getLongField(ipFields, "ID"));
						ipActionSet->MoveNext();
					}
					ipActionSet->Close();
				}

				// Update statistics for each action
				for (auto it = actions.begin(); it != actions.end(); ++it)
				{
					long nActionID = *it;

					_RecordsetPtr ipFileActionStatusSet = getFileActionStatusSet(ipConnection, nFileID, nActionID);
					EActionStatus status = ipFileActionStatusSet->adoEOF
						? kActionUnattempted
						: asEActionStatus(getStringField(ipFileActionStatusSet->Fields, "ActionStatus"));
					ipFileActionStatusSet->Close();

					if (status != kActionUnattempted)
					{
						// Subtract stats for visible state
						updateStats(ipConnection, nActionID, status, kActionUnattempted, __nullptr, ipFileRecord, false);

						// Add stats for invisible state
						updateStats(ipConnection, nActionID, kActionUnattempted, status, ipFileRecord, __nullptr, true);

						// Remove skipped record so that the skipped stats used by the IDShield web app stop counting this file
						removeSkipFileRecord(ipConnection, nFileID, nActionID);
					}
				}

				tg.CommitTrans();
			}
			else
			{
				throw UCLIDException("ELI46299", "Failed to mark file deleted.");
			}
			
			END_CONNECTION_RETRY(ipConnection, "ELI46300");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI46301");
	}
	catch (UCLIDException &ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::CacheFileTaskSessionData_Internal(bool bDBLocked, 
	long nFileTaskSessionID, long nPage, SAFEARRAY* parrayImageData, BSTR bstrUssData,
	BSTR bstrWordZoneData, BSTR bstrAttributeData, BSTR bstrException, VARIANT_BOOL vbCrucialUpdate, VARIANT_BOOL* pbWroteData)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI49464", pbWroteData != __nullptr);
			*pbWroteData = VARIANT_FALSE;

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			bool bCrucialUpdate = asCppBool(vbCrucialUpdate);
			if (bCrucialUpdate)
			{
				BEGIN_CONNECTION_RETRY();

				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				CacheFileTaskSessionData_InternalHelper(ipConnection, nFileTaskSessionID,
					nPage, parrayImageData, bstrUssData, bstrWordZoneData, bstrAttributeData,
					bstrException, bCrucialUpdate, pbWroteData);

				END_CONNECTION_RETRY(ipConnection, "ELI49510");
			}
			else
			{
				// BEGIN_CONNECTION_RETRY not used for non-crucial updates. If the DB connection is lost, this
				// data will simply not be cached. In unit tests, this prevents cache threads from hanging
				// around doing retries after the databases have been closed.
				
				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				CacheFileTaskSessionData_InternalHelper(ipConnection, nFileTaskSessionID,
					nPage, parrayImageData, bstrUssData, bstrWordZoneData, bstrAttributeData,
					bstrException, bCrucialUpdate, pbWroteData);
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI49443");
	}
	catch (UCLIDException & ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::CacheFileTaskSessionData_InternalHelper(ADODB::_ConnectionPtr ipConnection,
	long nFileTaskSessionID, long nPage, SAFEARRAY* parrayImageData, BSTR bstrUssData, BSTR bstrWordZoneData, BSTR bstrAttributeData,
	BSTR bstrException, bool bCrucialUpdate, VARIANT_BOOL* pbWroteData)
{
	TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_criticalSection);

	string strCreateCacheRowQuery = gstrGET_OR_CREATE_FILE_TASK_SESSION_CACHE_ROW;
	replaceVariable(strCreateCacheRowQuery, "<FileTaskSessionID>", asString(nFileTaskSessionID));
	replaceVariable(strCreateCacheRowQuery, "<Page>", asString(nPage));
	// bCrucialUpdate indicates it is unexepected for a row not to be available. Before writing
	// attribute data updates, CacheAttributeData should have been used to populate cache for
	// all pages. If not all pages were present in cache, incomplete document data would be
	// available when compiling output document data from the cache.
	replaceVariable(strCreateCacheRowQuery, "<CrucialUpdate>", bCrucialUpdate ? "1" : "0");

	long long llCacheRowID = -1;
	executeCmdQuery(ipConnection, strCreateCacheRowQuery, "ID", false, &llCacheRowID);

	if (llCacheRowID <= 0 && bCrucialUpdate)
	{
		UCLIDException ue("ELI49539",
			"Unable to apply crucial cache update as appropriate cache row does not exist");
		ue.addDebugInfo("SessionID", asString(nFileTaskSessionID), false);
		ue.addDebugInfo("Page", asString(nPage), false);
		throw ue;
	}

	// If the file task session has closed do not cache the data.
	if (llCacheRowID > 0)
	{
		vector<string> vecFields;
		if (parrayImageData != __nullptr)
		{
			vecFields.push_back("[ImageData]");
		}
		if (bstrUssData != __nullptr)
		{
			vecFields.push_back("[USSData]");
		}
		if (bstrWordZoneData != __nullptr)
		{
			vecFields.push_back("[WordZoneData]");
		}
		if (bstrAttributeData != __nullptr)
		{
			vecFields.push_back("[AttributeData]");
			vecFields.push_back("[AutoDeleteWithActiveFAMID]");
		}
		if (bstrException != __nullptr)
		{
			vecFields.push_back("[Exception]");
		}

		ASSERT_RUNTIME_CONDITION("ELI49501", vecFields.size() > 0,
			"No data has been provided for cache");

		string strCursorQuery = gstrGET_FILE_TASK_SESSION_CACHE_DATA_BY_ID;
		replaceVariable(strCursorQuery, "<ID>", asString(llCacheRowID));
		replaceVariable(strCursorQuery, "<FieldList>", asString(vecFields, true, ","));

		_RecordsetPtr ipCachedDataRow(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI48308", ipCachedDataRow != __nullptr);

		// Concurrent behavior tested by CacheSimultaneousOperations
		ipCachedDataRow->Open(strCursorQuery.c_str(),
			_variant_t((IDispatch*)ipConnection, true), adOpenDynamic,
			adLockOptimistic, adCmdText);
		if (ipCachedDataRow->adoEOF == VARIANT_TRUE)
		{
			throw UCLIDException("ELI48309", "Failed to acquire cache");
		}

		if (parrayImageData != __nullptr)
		{
			FieldPtr ipItem = ipCachedDataRow->Fields->Item["ImageData"];
			ASSERT_RESOURCE_ALLOCATION("ELI48336", ipItem != __nullptr);

			CComSafeArray<BYTE> saData(parrayImageData);
			_variant_t variantData;
			variantData.vt = VT_ARRAY | VT_UI1;
			variantData.parray = saData;
			ipItem->Value = variantData;
		}

		if (bstrUssData != __nullptr)
		{
			setStringField(ipCachedDataRow->Fields, "USSData", asString(bstrUssData));
		}

		if (bstrWordZoneData != __nullptr)
		{
			setStringField(ipCachedDataRow->Fields, "WordZoneData", asString(bstrWordZoneData));
		}
			
		if (bstrAttributeData != __nullptr)
		{
			setStringField(ipCachedDataRow->Fields, "AttributeData", asString(bstrAttributeData));
			setFieldToNull(ipCachedDataRow->Fields, "AutoDeleteWithActiveFAMID");
			// AttributeDataModifiedTime needs to be set as well, but if updated via a variant, it loses
			// millisecond granularity; A separate query is used below to update the timestamp
			// using GetDate()
		}

		if (bstrException != __nullptr)
		{
			setStringField(ipCachedDataRow->Fields, "Exception", asString(bstrException));
		}

		ipCachedDataRow->Update();
		ipCachedDataRow->Close();

		if (bstrAttributeData != __nullptr)
		{
			// Use GetDate() to update AttributeDataModifiedTime with millisecond granularity if attribute
			// data was updated.
			executeCmdQuery(ipConnection, "UPDATE [FileTaskSessionCache] SET [AttributeDataModifiedTime] = GetDate() WHERE [ID] = " + asString(llCacheRowID));
		}

		*pbWroteData = VARIANT_TRUE;
	}

	tg.CommitTrans();
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetCachedFileTaskSessionData_Internal(bool bDBLocked, long nFileTaskSessionID,
	long nPage, ECacheDataType eDataType, VARIANT_BOOL vbCrucialData,
	SAFEARRAY** pparrayImageData, BSTR* pbstrUssData, BSTR* pbstrWordZoneData, BSTR* pbstrAttributeData,
	BSTR* pbstrException, VARIANT_BOOL* pbFoundCacheData)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI49545", pbFoundCacheData != __nullptr);
			bool bFoundCacheData = false;

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			bool bCrucialData = asCppBool(vbCrucialData);
			if (bCrucialData)
			{
				BEGIN_CONNECTION_RETRY();

				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				bFoundCacheData = GetCachedFileTaskSessionData_InternalHelper(
					ipConnection, nFileTaskSessionID, nPage,
					eDataType, pparrayImageData, pbstrUssData, pbstrWordZoneData, pbstrAttributeData,
					pbstrException);

				END_CONNECTION_RETRY(ipConnection, "ELI49546");

				if (!bFoundCacheData)
				{
					UCLIDException ue("ELI50091", "Unable to retrieve crucial cached data");
					ue.addDebugInfo("SessionID", asString(nFileTaskSessionID), false);
					ue.addDebugInfo("Page", asString(nPage), false);
					throw ue;
				}
			}
			else
			{
				// BEGIN_CONNECTION_RETRY not used for non-crucial retrieval. If the DB connection is lost, this
				// data will simply not be retrieved. 

				auto role = getAppRoleConnection();
				ipConnection = role->ADOConnection();

				bFoundCacheData = GetCachedFileTaskSessionData_InternalHelper(
					ipConnection, nFileTaskSessionID, nPage,
					eDataType, pparrayImageData, pbstrUssData, pbstrWordZoneData, pbstrAttributeData,
					pbstrException);
			}

			*pbFoundCacheData = asVariantBool(bFoundCacheData);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI49544");
	}
	catch (UCLIDException & ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetCachedFileTaskSessionData_InternalHelper(ADODB::_ConnectionPtr ipConnection,
	long nFileTaskSessionID, long nPage, ECacheDataType eDataType,
	SAFEARRAY** pparrayImageData, BSTR* pbstrUssData, BSTR* pbstrWordZoneData, BSTR* pbstrAttributeData,
	BSTR* pbstrException)
{
	bool bFoundCachedData = false;
	_bstr_t bstrAttributeData("");

	ASSERT_RUNTIME_CONDITION("ELI49509", nPage > 0 || eDataType == kAttributes,
		"Multi-page data valid for attribute data only");

	if (nPage < 0)
	{
		string strCursorQuery = gstrGET_FILE_TASK_SESSION_CACHE_ROWS;
		replaceVariable(strCursorQuery, "<FileTaskSessionID>", asString(nFileTaskSessionID));

		_RecordsetPtr ipCachedDataRows(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI50089", ipCachedDataRows != __nullptr);

		ipCachedDataRows->Open(strCursorQuery.c_str(),
			_variant_t((IDispatch*)ipConnection, true), adOpenForwardOnly,
			adLockReadOnly, adCmdText);

		// Store page data to map to allow page output to be ordered by page.
		long nPageCount = 0;
		map<long, _bstr_t> mapPageAttributes;

		while (ipCachedDataRows->adoEOF == VARIANT_FALSE)
		{
			long nPage = getLongField(ipCachedDataRows->Fields, "Page");
			nPageCount = max(nPageCount, nPage);

			// Some manipulation of bstrPageAttributes is necessary to create a proper JSON representing
			// multiple pages; store page-specific JSON separate for now.
			_bstr_t bstrPageAttributes;
			if (GetCachedFileTaskSessionData_QueryCachedData(ipConnection, nFileTaskSessionID, nPage, eDataType,
				pparrayImageData, pbstrUssData, pbstrWordZoneData, &(bstrPageAttributes.GetBSTR()), pbstrException))
			{
				mapPageAttributes[nPage] = bstrPageAttributes;
			}

			ipCachedDataRows->MoveNext();
		}

		// Consider data found only if no pages between 1 and the max page number found are missing
		bFoundCachedData = (mapPageAttributes.size() == nPageCount);

		if (bFoundCachedData)
		{
			// Merge all page-specific attribute JSON into a proper JSON array representing all pages.
			for (long nPage = 1; nPage <= nPageCount; nPage++)
			{
				_bstr_t bstrPageAttributes = mapPageAttributes[nPage];
				if (bstrPageAttributes.length() > 0)
				{
					if (bstrAttributeData.length() > 0)
					{
						bstrAttributeData += ",";
					}

					bstrAttributeData += bstrPageAttributes;
				}
			}

			bstrAttributeData = _bstr_t("[") + bstrAttributeData + "]";
			*pbstrAttributeData = bstrAttributeData.Detach();
		}
	}
	else // Specific page number specified
	{
		bFoundCachedData = GetCachedFileTaskSessionData_QueryCachedData(ipConnection,
			nFileTaskSessionID, nPage, eDataType, pparrayImageData, pbstrUssData,
			pbstrWordZoneData, pbstrAttributeData, pbstrException);
	}

	if (!bFoundCachedData)
	{
		*pparrayImageData = __nullptr;
		*pbstrUssData = __nullptr;
		*pbstrWordZoneData = __nullptr;
		*pbstrAttributeData = __nullptr;
		*pbstrException = __nullptr;
	}

	return bFoundCachedData;
}
//-------------------------------------------------------------------------------------------------
// Helper function for GetCachedFileTaskSessionData
bool CFileProcessingDB::GetCachedFileTaskSessionData_QueryCachedData(_ConnectionPtr ipConnection, long nFileTaskSessionID, long nPage,
	ECacheDataType eDataType, SAFEARRAY** pparrayImageData, BSTR* pbstrUssData, BSTR* pbstrWordZoneData, BSTR* pbstrAttributeData,
	BSTR* pbstrException)
{
	bool bFoundCacheData = false;
	bool bGetCachedImage = (eDataType & (int)kImage) != 0;
	bool bGetCachedUSS = (eDataType & (int)kUss) != 0;
	bool bGetCachedWordZones = (eDataType & (int)kWordZone) != 0;
	bool bGetCachedAttributes = (eDataType & (int)kAttributes) != 0;
	bool bGetCacheException = (eDataType & (int)kException) != 0;

	vector<string> vecFields;
	if (bGetCachedImage)
	{
		vecFields.push_back("[ImageData]");
	}
	if (bGetCachedUSS)
	{
		vecFields.push_back("[USSData]");
	}
	if (bGetCachedWordZones)
	{
		vecFields.push_back("[WordZoneData]");
	}
	if (bGetCachedAttributes)
	{
		vecFields.push_back("[AttributeData]");
	}

	// Always look for an exception logged while caching data. If found, throw the
	// exception without retrieving any of the cached data.
	vecFields.push_back("[Exception]");

	if (vecFields.size() > 0)
	{
		ASSERT_RUNTIME_CONDITION("ELI48432", nPage > 0, "Cannot query data for multiple pages");

		string strCursorQuery = gstrGET_FILE_TASK_SESSION_CACHE_DATA_BY_PAGE;
		replaceVariable(strCursorQuery, "<FieldList>", asString(vecFields, true, ","));
		replaceVariable(strCursorQuery, "<FileTaskSessionID>", asString(nFileTaskSessionID));
		replaceVariable(strCursorQuery, "<Page>", asString(nPage));

		_RecordsetPtr ipCachedDataRow(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI48362", ipCachedDataRow != __nullptr);

		ipCachedDataRow->Open(strCursorQuery.c_str(),
			_variant_t((IDispatch*)ipConnection, true), adOpenForwardOnly,
			adLockReadOnly, adCmdText);

		if (ipCachedDataRow->adoEOF == VARIANT_FALSE)
		{
			string strException = getStringField(ipCachedDataRow->Fields, "Exception");
			// Unless caller is specifically asking for a recorded exception, throw any recorded
			// exception from the cache operation when trying to retrieve any other data.
			if (!strException.empty() && !bGetCacheException)
			{
				UCLIDException ue;
				ue.createFromString("ELI49456", strException);
				throw ue;
			}

			try
			{
				try
				{
					if (bGetCachedImage)
					{
						FieldPtr ipImageData = ipCachedDataRow->Fields->Item["ImageData"];
						// If parray is null, CComSafeArray will assert in a way that displays a debug dialog.
						// Assert ourselves here to avoid that.
						ASSERT_RESOURCE_ALLOCATION("ELI49458", ipImageData->Value.parray != __nullptr);

						CComSafeArray<BYTE> saData(ipImageData->Value.parray);
						*pparrayImageData = saData.Detach();
					}
					if (bGetCachedUSS)
					{
						*pbstrUssData = get_bstr_t(getStringField(ipCachedDataRow->Fields, "USSData")).Detach();
					}
					if (bGetCachedWordZones)
					{
						*pbstrWordZoneData = get_bstr_t(getStringField(ipCachedDataRow->Fields, "WordZoneData")).Detach();
					}
					if (bGetCachedAttributes)
					{
						string strAttributeList = getStringField(ipCachedDataRow->Fields, "AttributeData");
						// Each page will contain an array of attributes; in case the call will want many pages of attributes,
						// trim off the JSON array brackets so they can be re-added around requested pages.
						strAttributeList = trim(strAttributeList, "[", "]");
						*pbstrAttributeData = get_bstr_t(strAttributeList).Detach();
					}
					if (bGetCacheException)
					{
						*pbstrException = get_bstr_t(strException).Detach();
					}
				}
				CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI49448");
			}
			catch (UCLIDException & ue)
			{
				UCLIDException ueOuter("ELI49447", "Failed to read cached data", ue);
				ueOuter.addDebugInfo("Session ID", nFileTaskSessionID);
				ueOuter.addDebugInfo("Page", nPage);
				throw ueOuter;
			}

			bFoundCacheData = true;
		}

		ipCachedDataRow->Close();
	}

	return bFoundCacheData;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::CacheAttributeData_Internal(bool bDBLocked, long nFileTaskSessionID,
								IStrToStrMap* pmapAttributeData, VARIANT_BOOL bOverwriteModifiedData)
{
	try
	{
		try
		{
			IStrToStrMapPtr ipDataByPage(pmapAttributeData);
			ASSERT_RESOURCE_ALLOCATION("ELI49473", ipDataByPage != __nullptr);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();

			TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_criticalSection);

			long nPageCount = pmapAttributeData->Size;

			string strCreateCacheRowsQuery = gstrCREATE_FILE_TASK_SESSION_CACHE_ROWS;
			replaceVariable(strCreateCacheRowsQuery, "<FileTaskSessionID>", asString(nFileTaskSessionID));
			replaceVariable(strCreateCacheRowsQuery, "<PageCount>", asString(nPageCount));

			long nCacheRowCount = -1;
			executeCmdQuery(ipConnection, strCreateCacheRowsQuery, "CacheRowCount", false, &nCacheRowCount);

			ASSERT_RUNTIME_CONDITION("ELI49474", nPageCount == nCacheRowCount, "Failed to initialize document data.");

			_RecordsetPtr ipCachedDataRow(__uuidof(Recordset));
			ASSERT_RESOURCE_ALLOCATION("ELI49493", ipCachedDataRow != __nullptr);

			string strCursorQuery = gstrGET_FILE_TASK_SESSION_CACHE_DATA;
			replaceVariable(strCursorQuery, "<FileTaskSessionID>", asString(nFileTaskSessionID));
			replaceVariable(strCursorQuery, "<FieldList>", "[Page], [AttributeData], [AttributeDataModifiedTime]");

			ipCachedDataRow->Open(strCursorQuery.c_str(),
				_variant_t((IDispatch*)ipConnection, true), adOpenDynamic,
				adLockOptimistic, adCmdText);
			int nRowCount = 0;
			while (ipCachedDataRow->adoEOF == VARIANT_FALSE)
			{
				long nPage = getLongField(ipCachedDataRow->Fields, "Page");
				_bstr_t bstrPage = _bstr_t(asString(nPage).c_str());
				if (asCppBool(pmapAttributeData->Contains(bstrPage))
					&& (asCppBool(bOverwriteModifiedData)
						|| isNULL(ipCachedDataRow->Fields, "AttributeDataModifiedTime")))
				{
					string btrPageData = asString(pmapAttributeData->GetValue(bstrPage));
					setStringField(ipCachedDataRow->Fields, "AttributeData", btrPageData);
				}

				nRowCount++;
				ipCachedDataRow->MoveNext();
			}

			ASSERT_RUNTIME_CONDITION("ELI49480", nRowCount == nPageCount, "Failed to initialize document data.");

			tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI49481");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI49492");
	}
	catch (UCLIDException & ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::MarkAttributeDataUnmodified_Internal(bool bDBLocked, long nFileTaskSessionID)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();

			string strMarkAttributeUnmodifiedQuery = gstrMARK_TASK_SESSION_ATTRIBUTE_DATA_UNMODIFIED;
			replaceVariable(strMarkAttributeUnmodifiedQuery, "<FileTaskSessionID>", asString(nFileTaskSessionID));

			executeCmdQuery(ipConnection, strMarkAttributeUnmodifiedQuery);

			END_CONNECTION_RETRY(ipConnection, "ELI49515");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI49516");
	}
	catch (UCLIDException & ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::GetUncommittedAttributeData_Internal(bool bDBLocked, long nFileID, long nActionID,
	BSTR bstrExceptIfMoreRecentAttributeSetName, IIUnknownVector** ppUncommittedPagesOfData)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI50000", ppUncommittedPagesOfData != __nullptr);

			IIUnknownVectorPtr ipUncommittedPagesOfData(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI49526", ipUncommittedPagesOfData != __nullptr);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();

			_RecordsetPtr ipCachedDataRow(__uuidof(Recordset));
			ASSERT_RESOURCE_ALLOCATION("ELI49523", ipCachedDataRow != __nullptr);

			string strCursorQuery = gstrGET_UNCOMMITTED_ATTRIBUTE_DATA;
			replaceVariable(strCursorQuery, "<FileID>", asString(nFileID));
			replaceVariable(strCursorQuery, "<ActionID>", asString(nActionID));
			replaceVariable(strCursorQuery, "<ExceptIfMoreRecentAttributeSetName>",
				asString(bstrExceptIfMoreRecentAttributeSetName));

			ipCachedDataRow->Open(strCursorQuery.c_str(),
				_variant_t((IDispatch*)ipConnection, true), adOpenStatic, adLockReadOnly, adCmdText);
			while (ipCachedDataRow->adoEOF == VARIANT_FALSE)
			{
				IVariantVectorPtr ipRowData(CLSID_VariantVector);
				ASSERT_RESOURCE_ALLOCATION("ELI49527", ipRowData != __nullptr);

				FieldsPtr ipFields = ipCachedDataRow->Fields;
				ASSERT_RESOURCE_ALLOCATION("ELI49528", ipFields != __nullptr);

				ipRowData->PushBack(ipFields->Item["FullUserName"]->Value);
				ipRowData->PushBack(ipFields->Item["AttributeDataModifiedTime"]->Value);
				ipRowData->PushBack(ipFields->Item["Page"]->Value);
				ipRowData->PushBack(ipFields->Item["AttributeData"]->Value);

				ipUncommittedPagesOfData->PushBack(ipRowData);

				ipCachedDataRow->MoveNext();
			}

			END_CONNECTION_RETRY(ipConnection, "ELI49524");

			*ppUncommittedPagesOfData = ipUncommittedPagesOfData.Detach();
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI49525");
	}
	catch (UCLIDException & ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::DiscardOldCacheData_Internal(bool bDBLocked, long nFileID, long nActionID,
																 long nExceptFileTaskSessionID)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();

			string strDiscardQuery = gstrDISCARD_OLD_CACHE_DATA;
			replaceVariable(strDiscardQuery, "<FileID>", asString(nFileID));
			replaceVariable(strDiscardQuery, "<ActionID>", asString(nActionID));
			replaceVariable(strDiscardQuery, "<ExceptFileTaskSessionID>", asString(nExceptFileTaskSessionID));

			executeCmdQuery(ipConnection, strDiscardQuery);

			END_CONNECTION_RETRY(ipConnection, "ELI49530");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI49531");
	}
	catch (UCLIDException & ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
