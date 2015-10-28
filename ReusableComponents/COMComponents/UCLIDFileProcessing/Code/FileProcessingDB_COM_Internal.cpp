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

#include <string>
#include <stack>

using namespace std;
using namespace ADODB;

//-------------------------------------------------------------------------------------------------
// Define constant for the current DB schema version
// This must be updated when the DB schema changes
// !!!ATTENTION!!!
// An UpdateToSchemaVersion method must be added when checking in a new schema version.
const long CFileProcessingDB::ms_lFAMDBSchemaVersion = 132;
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

inline string getEncryptedString(string strInput)
{
	// Put the input string into the byte manipulator
	ByteStream bytes;
	ByteStreamManipulator bytesManipulator(ByteStreamManipulator::kWrite, bytes);

	bytesManipulator << strInput;

	// Convert information to a stream of bytes
	// with length divisible by 8 (in variable called 'bytes')
	bytesManipulator.flushToByteStream(8);

	// Get the password 'key' based on the 4 hex global variables
	ByteStream pwBS;
	ByteStreamManipulator bsm(ByteStreamManipulator::kWrite, pwBS);

	bsm << gulFAMKey1;
	bsm << gulFAMKey2;
	bsm << gulFAMKey3;
	bsm << gulFAMKey4;
	bsm.flushToByteStream(8);
	

	// Do the encryption
	ByteStream encryptedBS;
	MapLabel encryptionEngine;
	encryptionEngine.setMapLabel(encryptedBS, bytes, pwBS);

	// Return the encrypted value
	return encryptedBS.asString();
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
		vecQueries.push_back(gstrCREATE_ACTION_STATISTICS_TABLE);

		// https://extract.atlassian.net/browse/ISSUE-12916
		// Was added in 10.1 should have been added at the time the table was changed.
		vecQueries.push_back(gstrADD_STATISTICS_ACTION_FK);

		// Add new ActionStatisticsDelta table.
		vecQueries.push_back(gstrCREATE_ACTION_STATISTICS_DELTA_TABLE);
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
		vecQueries.push_back(gstrCREATE_FILE_ACTION_STATUS);
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
		vecQueries.push_back(gstrADD_FAM_SESSION_ACTION_FK);
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
		vecQueries.push_back(gstrCREATE_SKIPPED_FILE_FAM_SESSION_INDEX);
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
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33184");
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
		vecQueries.push_back(gstrCREATE_FILE_TASK_SESSION);
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

		vecQueries.push_back(gstrCREATE_SECURE_COUNTER);
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
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38716");
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

		vecQueries.push_back(gstrCREATE_SECURE_COUNTER);
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
				ipConnection = getDBConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

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
				ipConnection = getDBConnection();

				string strActionName = asString(strAction);

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Get the action ID and update the strActionName to stored value
				long nActionID = getActionID(ipConnection, strActionName);

				// Make sure processing is not active of this action
				assertProcessingNotActiveForAction(bDBLocked, ipConnection, nActionID);

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				// Delete the action
				string strDeleteActionQuery = "DELETE FROM [Action] WHERE [ASCName] = '" + asString(strAction) + "'";
				executeCmdQuery(ipConnection, strDeleteActionQuery);

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
				ipConnection = getDBConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Create StrToStrMap to return the list of actions
				IStrToStrMapPtr ipActions = getActions(ipConnection);
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
bool CFileProcessingDB::AddFile_Internal(bool bDBLocked, BSTR strFile,  BSTR strAction, EFilePriority ePriority,
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
			// Replace any occurrences of ' with '' this is because SQL Server use the ' to indicate
			// the beginning and end of a string
			string strFileName = asString(strFile);
			replaceVariable(strFileName, "'", "''");

			// Open a recordset that contain only the record (if it exists) with the given filename
			string strFileSQL = "SELECT * FROM FAMFile WHERE FileName = '" + strFileName + "'";

			// put the unaltered file name back in the strFileName variable
			strFileName = asString(strFile);

			// Create the file record to return
			UCLID_FILEPROCESSINGLib::IFileRecordPtr ipNewFileRecord(CLSID_FileRecord);
			ASSERT_RESOURCE_ALLOCATION("ELI30359", ipNewFileRecord != __nullptr);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				ipConnection = getDBConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				_lastCodePos = "10";

				// Create a pointer to a recordset
				_RecordsetPtr ipFileSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI30360", ipFileSet != __nullptr);

				ipFileSet->Open(strFileSQL.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenDynamic, 
					adLockOptimistic, adCmdText);

				_lastCodePos = "30";

				// Check whether the file already exists in the database
				*pbAlreadyExists = asVariantBool(ipFileSet->adoEOF == VARIANT_FALSE);

				// Initialize the id
				long nID = 0;

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_mutex);

				// Set the action name from the parameter
				string strActionName = asString(strAction);

				// Get the action ID and update the strActionName to stored value
				long nActionID = getActionID(ipConnection, strActionName);
				_lastCodePos = "45";

				// if the file is in the FAMFile table get the ID
				if (asCppBool(*pbAlreadyExists))
				{
					nID = getLongField(ipFileSet->Fields, "ID");
				}

				// Get the FileActionStatus recordset with the status of the file for the action
				// NOTE: if nID = 0 or File status is unattempted for the action the recordset will be empty
				_RecordsetPtr ipFileActionStatusSet = getFileActionStatusSet(ipConnection, nID, nActionID);
				*pPrevStatus = (asCppBool(!ipFileActionStatusSet->adoEOF)) ?
					asEActionStatus(getStringField(ipFileActionStatusSet->Fields, "ActionStatus")) : kActionUnattempted;

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
					llFileSize, nPages, (UCLID_FILEPROCESSINGLib::EFilePriority) ePriority);

				_lastCodePos = "50";

				string strNewStatus = asStatusString(eNewStatus);

				// If file did not already exist then add a new record to the database
				if (*pbAlreadyExists == VARIANT_FALSE)
				{
					// Add new record
					ipFileSet->AddNew();

					// Get the fields from the file set
					FieldsPtr ipFields = ipFileSet->Fields;
					ASSERT_RESOURCE_ALLOCATION("ELI30361", ipFields != __nullptr);

					// Set the fields from the new file record
					setFieldsFromFileRecord(ipFields, ipNewFileRecord);

					string strPriority = asString(getLongField(ipFields, "Priority"));

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
					string strStatusSQL = "INSERT INTO FileActionStatus "
						"(FileID, ActionID, ActionStatus, Priority) "
						"VALUES( " + asString(nID) + ", " + asString(nActionID) + ", '" + strNewStatus +
						"', " + strPriority + ")";

					_lastCodePos = "85";

					// Execute query to insert the new FileActionStatus record
					executeCmdQuery(ipConnection, strStatusSQL);

					_lastCodePos = "87";

					// update the statistics
					updateStats(ipConnection, nActionID, *pPrevStatus, eNewStatus, ipNewFileRecord, NULL);
					_lastCodePos = "90";
				}
				else
				{
					// Get the fields from the file set
					FieldsPtr ipFields = ipFileSet->Fields;
					ASSERT_RESOURCE_ALLOCATION("ELI30362", ipFields != __nullptr);

					// Get the file record from the fields
					UCLID_FILEPROCESSINGLib::IFileRecordPtr ipOldRecord = getFileRecordFromFields(ipFields);
					ASSERT_RESOURCE_ALLOCATION("ELI30363", ipOldRecord != __nullptr);

					// Set the Current file Records ID
					nID = ipOldRecord->FileID;

					_lastCodePos = "100";

					// If Force processing is set need to update the status or if the previous status for this action was unattempted
					if (bForceStatusChange == VARIANT_TRUE || *pPrevStatus == kActionUnattempted)
					{
						// Call setStatusForFile to handle updating all tables related to the status
						// change, as appropriate.
						setStatusForFile(ipConnection, nID, strActionName, kActionPending, true, false);

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

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				ipConnection = getDBConnection();

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
				TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_mutex);

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

							// update the statistics
							updateStats(ipConnection, nActionID, asEActionStatus(strActionState), kActionUnattempted, NULL, ipOldRecord); 
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
bool CFileProcessingDB::NotifyFileProcessed_Internal(bool bDBLocked, long nFileID,  BSTR strAction )
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
				ipConnection = getDBConnection();

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_mutex);

				// change the given files state to completed unless there is a pending state in the
				// QueuedActionStatusChange table.
				setFileActionState(ipConnection, nFileID, asString(strAction), "C", "", false, true);

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
bool CFileProcessingDB::NotifyFileFailed_Internal(bool bDBLocked,long nFileID,  BSTR strAction,  
	BSTR strException)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				ipConnection = getDBConnection();

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_mutex);

				// [LegacyRCAndUtils:6054]
				// Store the full log string which contains additional info which may be useful.
				UCLIDException ue;
				ue.createFromString("ELI32298", asString(strException));
				string strLogString = ue.createLogString();

				// change the given files state to Failed unless there is a pending state in the
				// QueuedActionStatusChange table.
				setFileActionState(ipConnection, nFileID, asString(strAction), "F", strLogString, false, true);

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
				ipConnection = getDBConnection();
				
				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_mutex);
				
				// change the given files state to Pending
				setFileActionState(ipConnection, nFileID, asString(strAction), "P", "", 
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
				ipConnection = getDBConnection();;

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_mutex);

				// change the given files state to unattempted
				setFileActionState(ipConnection, nFileID, asString(strAction), "U", "",
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
			ipConnection = getDBConnection();

			// Begin a transaction
			TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_mutex);

			// Change the given files state to Skipped
			setFileActionState(ipConnection, nFileID, asString(strAction), "S", "",
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
				ipConnection = getDBConnection();

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
						// Begin a transaction
						TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_mutex);

						revertTimedOutProcessingFAMs(bDBLocked, ipConnection);

						// Commit the changes to the database
						tg.CommitTrans();

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
bool CFileProcessingDB::SearchAndModifyFileStatus_Internal(bool bDBLocked,
	long nWhereActionID,  EActionStatus eWhereStatus,  
	long nToActionID, EActionStatus eToStatus,
	BSTR bstrSkippedFromUserName, 
	long nFromActionID, long * pnNumRecordsModified)
{
	try
	{
		try
		{
			// Changing an Action status to failed should only be done on an individual file bases
			if (eToStatus == kActionFailed)
			{
				UCLIDException ue ("ELI30372", "Cannot change status Failed.");
				throw ue;
			}

			// If the to status is not skipped, the from status is the same as the to status
			// and the Action ids are the same, there is nothing to do
			// If setting skipped status the skipped file table needs to be updated
			if (eToStatus != kActionSkipped
				&& eWhereStatus == eToStatus
				&& nToActionID == nWhereActionID)
			{
				// nothing to do
				return true;
			}

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				ipConnection = getDBConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				string strToAction = getActionName(ipConnection, nToActionID);

				// Create string to use for Where action ID
				string strWhereActionID = asString(nWhereActionID);

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactIsolated, &m_mutex);

				string strSQL = "SELECT FAMFile.ID AS FAMFileID, COALESCE(ActionStatus, 'U') AS WhereActionStatus ";

				strSQL += " FROM FAMFile LEFT JOIN FileActionStatus ON FAMFile.ID = FileActionStatus.FileID"
					" AND ActionID = " + strWhereActionID;

				string strWhere = " WHERE (WhereActionStatus = '"
					+ asStatusString(eWhereStatus) + "'";

				// Where status is skipped, need to add inner join to skip file table
				if (eWhereStatus == kActionSkipped)
				{
					strSQL += " INNER JOIN SkippedFile ON FAMFile.ID = SkippedFile.FileID ";
					strWhere += " AND SkippedFile.ActionID = " + strWhereActionID;
					string strUser = asString(bstrSkippedFromUserName);
					if (!strUser.empty())
					{
						strWhere += " AND SkippedFile.UserName = '" + strUser + "'";
					}
				}

				// Close the where clause and add it to the SQL statement
				strWhere += ")";
				strSQL += strWhere;

				// Get the to status as a string
				string strToStatus = asStatusString(eToStatus);

				// Get a recordset to fill with the File IDs
				_RecordsetPtr ipFileSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI30373", ipFileSet != __nullptr);

				// Open the recordset
				ipFileSet->Open(strSQL.c_str(), _variant_t((IDispatch *)ipConnection, true),
					adOpenForwardOnly, adLockReadOnly, adCmdText);

				// Modify each files status (count the number of records modified)
				long nRecordsModified = 0;
				while(ipFileSet->adoEOF == VARIANT_FALSE)
				{
					FieldsPtr ipFields = ipFileSet->Fields;
					long nFileID = getLongField(ipFields, "FAMFileID");
					if (nFromActionID > 0)
					{
						strToStatus = getStringField(ipFields, "WhereActionStatus");
					}

					setFileActionState(ipConnection, nFileID, strToAction, strToStatus, "", false,
						false, nToActionID, true);

					// Update modified records count
					nRecordsModified++;

					// Move to next record
					ipFileSet->MoveNext();
				}

				// Set the return value
				*pnNumRecordsModified = nRecordsModified;

				// Commit the changes
				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI30374");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30641");
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

				// Get the connection for the thread and save it locally.
				ipConnection = getDBConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				if (eStatus == kActionFailed)
				{
					UCLIDException ue ("ELI30375", "Transition to Failed state is not allowed.");
					throw ue;
				}

				// Set the action name from the parameter
				string strActionName = asString(strAction);

				// Get the action ID and update the strActionName to stored value
				long nActionID = getActionID(ipConnection, strActionName);

				string strActionStatus = asStatusString(eStatus);

				// Only want to change the status that is different from status that is being changed to
				string strWhere = " WHERE ActionStatus  <> '" + strActionStatus + "'";

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactIsolated, &m_mutex);

				// Get the action ID as a string
				string strActionID = asString(nActionID);

				// Remove any records from the skipped file table that where skipped for this action
				string strDeleteSkippedSQL = "DELETE FROM [SkippedFile] WHERE ActionID = " + strActionID;
				executeCmdQuery(ipConnection, strDeleteSkippedSQL);

				// Remove any records in the LockedFile table for status changing from Processing to pending
				string strDeleteLockedFiles = "DELETE FROM [LockedFile] WHERE ActionID = " + strActionID;
				executeCmdQuery(ipConnection, strDeleteLockedFiles);

				// There are no cases where this method should not just ignore all pending entries in
				// [QueuedActionStatusChange] for the selected files.
				string strUpdateQueuedActionStatusChange =
					"UPDATE [QueuedActionStatusChange] SET [ChangeStatus] = 'I'"
					"WHERE [ChangeStatus] = 'P' AND [ActionID] = " + strActionID;
				executeCmdQuery(ipConnection, strUpdateQueuedActionStatusChange);

				// If setting files to skipped, need to add skipped record for each file
				if (eStatus == kActionSkipped)
				{
					// Get the current user name
					string strUserName = getCurrentUserName();

					// Add all files to the skipped table for this action
					string strSQL = "INSERT INTO [SkippedFile] ([FileID], [ActionID], [UserName]) "
						"(SELECT [ID], " + strActionID + " AS ActionID, '" + strUserName
						+ "' AS UserName FROM [FAMFile])";
					executeCmdQuery(ipConnection, strSQL);
				}

				// Add the transition records
				addASTransFromSelect(ipConnection, strActionName, nActionID, strActionStatus,
					"", "", strWhere, "");

				// if the new status is Unattempted
				if (eStatus == kActionUnattempted)
				{
					string strDeleteStatus = "DELETE FROM FileActionStatus "
						" WHERE ActionID = " + strActionID;
					executeCmdQuery(ipConnection, strDeleteStatus);
				}
				else
				{
					// Update status of existing records
					string strUpdateStatus = "UPDATE FileActionStatus SET ActionStatus = '" + 
						strActionStatus + "' "
						"FROM FileActionStatus INNER JOIN FAMFile ON FileID = FAMFile.ID AND "
						"FileActionStatus.ActionID = " + strActionID;
					executeCmdQuery(ipConnection, strUpdateStatus);

					// Insert new records where previous status was 'U'
					string strInsertStatus = "INSERT INTO FileActionStatus "
						"(FileID, ActionID, ActionStatus, Priority) "
						" SELECT FAMFile.ID, " + strActionID + " as ActionID, '" +
						strActionStatus + "' AS ActionStatus, "
						"COALESCE(FileActionStatus.Priority, FAMFile.Priority) AS Priority "
						"FROM FAMFile LEFT JOIN FileActionStatus ON "
						"FAMFile.ID = FileActionStatus.FileID AND "
						"FileActionStatus.ActionID = " + strActionID +
						" WHERE FileActionStatus.ActionID IS NULL ";
					executeCmdQuery(ipConnection, strInsertStatus);
				}

				// If going to complete status and AutoDeleteFileActionComments == true then
				// clear the file action comments
				if (eStatus == kActionCompleted && m_bAutoDeleteFileActionComment)
				{
					clearFileActionComment(ipConnection, -1, nActionID);
				}

				// update the stats
				reCalculateStats(ipConnection, nActionID);

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
												  EActionStatus eStatus,  
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
			ipConnection = getDBConnection();

			// Begin a transaction
			TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_mutex);

			setStatusForFile(ipConnection, nID, asString(strAction), eStatus,
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
bool CFileProcessingDB::GetFilesToProcess_Internal(bool bDBLocked, BSTR strAction,  long nMaxFiles, 
												  VARIANT_BOOL bGetSkippedFiles,
												  BSTR bstrSkippedForUserName,
												  IIUnknownVector * * pvecFileRecords)
{
	try
	{
		try
		{
			// Set the action name from the parameter
			string strActionName = asString(strAction);

			// If the FAM has lost its registration, re-register before continuing with processing.
			ensureFAMRegistration(strActionName);

			static const string strActionIDPlaceHolder = "<ActionIDPlaceHolder>";

			string strWhere = "";
			string strTop = "TOP (" + asString(nMaxFiles) + ") ";
			if (bGetSkippedFiles == VARIANT_TRUE)
			{
				strWhere = "INNER JOIN SkippedFile ON FileActionStatus.FileID = SkippedFile.FileID "
					"AND SkippedFile.ActionID = <ActionIDPlaceHolder> WHERE (ActionStatus = 'S'";

				string strUserName = asString(bstrSkippedForUserName);
				if(!strUserName.empty())
				{
					replaceVariable(strUserName, "'", "''");
					string strUserAnd = " AND SkippedFile.UserName = '" + strUserName + "'";
					strWhere += strUserAnd;
				}

				// Only get files that have not been skipped by the current session.
				strWhere += " AND COALESCE(SkippedFile.FAMSessionID, 0) <> " + asString(m_nFAMSessionID);
			}
			else
			{
				strWhere = "WHERE (ActionStatus = 'P'";
			}

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			long nActionID;

			{
				BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				ipConnection = getDBConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Get the action ID 
				nActionID = getActionID(ipConnection, strActionName);

				// [LegacyRCAndUtils:6233]
				// Since the query run by setFilesToProcessing is expensive (even when there are no
				// pending records available), before calling setFilesToProcessing do a quick and
				// simple check to see if there are any files available.
				string strGateKeeperQuery =
					"IF EXISTS ("
					"	SELECT * FROM [FileActionStatus] " + strWhere +
					"		AND [FileActionStatus].[ActionID] = <ActionIDPlaceHolder>)"
					"		OR ([ActionStatus] = 'R' "
					"		AND [FileActionStatus].[ActionID] = <ActionIDPlaceHolder>)"
					") SELECT 1 AS ID ELSE SELECT 0 AS ID";

				// Update the select statement with the action ID
				replaceVariable(strGateKeeperQuery, strActionIDPlaceHolder, asString(nActionID));

				// The "ID" column for executeCmdQuery will actually be 1 if there are potential
				// files to process of 0 if there are not.
				long nFilesToProcess = 0;
				executeCmdQuery(ipConnection, strGateKeeperQuery, false, &nFilesToProcess);

				// If there are no files available, don't bother calling setFilesToProcessing.
				if (nFilesToProcess == 0)
				{
					IIUnknownVectorPtr ipFiles(CLSID_IUnknownVector);
					ASSERT_RESOURCE_ALLOCATION("ELI34145", ipFiles != __nullptr);

					*pvecFileRecords = ipFiles.Detach();
					
					return true;
				}

				END_CONNECTION_RETRY(ipConnection, "ELI34143");
			}

			// Order by priority [LRCAU #5438]
			strWhere += ") ORDER BY [FileActionStatus].[Priority] DESC, [FileActionStatus].[FileID] ASC ";

			// Build the from clause
			string strFrom = "FROM FAMFile INNER JOIN FileActionStatus "
				"ON FileActionStatus.FileID = FAMFile.ID AND FileActionStatus.ActionID = <ActionIDPlaceHolder> "
				+ strWhere;

			// create query to select top records;
			string strSelectSQL = "SELECT " + strTop
				+ " FAMFile.ID, FileName, Pages, FileSize, FileActionStatus.Priority, ActionStatus " + strFrom;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				ipConnection = getDBConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Update the select statement with the action ID
				replaceVariable(strSelectSQL, strActionIDPlaceHolder, asString(nActionID));

				// Perform all processing related to setting a file as processing.
				// The previous status of the files to process is expected to be either pending or
				// skipped.
				IIUnknownVectorPtr ipFiles = setFilesToProcessing(
					bDBLocked, ipConnection, strSelectSQL, nActionID, "PS");
				*pvecFileRecords = ipFiles.Detach();
			END_CONNECTION_RETRY(ipConnection, "ELI30377");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30644");
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
bool CFileProcessingDB::GetFileToProcess_Internal(bool bDBLocked, long nFileID, BSTR strAction,
												  IFileRecord** ppFileRecord)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI37460", ppFileRecord != __nullptr);
		
			// Set the action name from the parameter
			string strActionName = asString(strAction);

			// If the FAM has lost its registration, re-register before continuing with processing.
			ensureFAMRegistration(strActionName);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			ipConnection = getDBConnection();

			// Make sure the DB Schema is the expected version
			validateDBSchemaVersion();

			string strFileID = asString(nFileID);
			long nActionID = getActionID(ipConnection, strActionName);
			string strActionID = asString(nActionID);
			
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

			// Select the required file info from the database based on the file ID and current action.
			string strSelectSQL =
				"SELECT FAMFile.ID, FileName, Pages, FileSize, "
				"COALESCE(FileActionStatus.Priority, FAMFile.Priority) AS Priority, "
				"COALESCE(ActionStatus, 'U') AS ActionStatus "
				"FROM FAMFile LEFT JOIN FileActionStatus ON FileActionStatus.FileID = FAMFile.ID "
				"	AND FileActionStatus.ActionID = <ActionID> "
				"WHERE [FAMFile].[ID] = <FileID>";

			replaceVariable(strSelectSQL, "<FileID>", strFileID);
			replaceVariable(strSelectSQL, "<ActionID>", strActionID);

			// Perform all processing related to setting a file as processing.
			IIUnknownVectorPtr ipFiles = setFilesToProcessing(
				bDBLocked, ipConnection, strSelectSQL, nActionID, "");

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
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				ipConnection = getDBConnection();		

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Set the action name from the parameter
				string strActionName = asString(strAction);

				// Get the action ID and update the strActionName to stored value
				long nActionID = getActionID(ipConnection, strActionName);

				// Replace any occurrences of ' with '' this is because SQL Server use the ' to
				// indicate the beginning and end of a string
				string strFolderName = asString(strFolder);
				replaceVariable(strFolderName, "'", "''");

				// set up the where clause to find the pending records that the filename begins with the folder name
				string strWhere = "WHERE (ActionStatus = 'P') AND (FileName LIKE '" + strFolderName + "%')";
				string strFrom = "FROM FAMFile " + strWhere;

				// Set up the SQL to delete the records in the FileActionStatus table 
				string strDeleteSQL = "DELETE FROM FileActionStatus"
					" WHERE ActionID = " + asString(nActionID) + " AND FileID IN ("
					" SELECT FAMFile.ID FROM FileActionStatus RIGHT JOIN FAMFile "
					" ON FileActionStatus.FileID = FAMFile.ID " + 
					strWhere + ")";

				// This method does not ever seem to get called, but in case it does, it seems reasonable
				// to ignore and pending changes for files in the folder.
				string strUpdateQueuedActionStatusChange =
					"UPDATE [QueuedActionStatusChange] SET [ChangeStatus] = 'I'"
					"WHERE [ChangeStatus] = 'P' AND [ActionID] = " + asString(nActionID) +
					" AND [FileID] IN"
					"("
					"	SELECT FAMFile.ID FROM FileActionStatus RIGHT JOIN FAMFile "
					"	ON FileActionStatus.FileID = FAMFile.ID " + strWhere +
					")";

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_mutex);

				// add transition records to the database
				addASTransFromSelect(ipConnection, strActionName, nActionID, "U", "", "", strWhere, "");

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
bool CFileProcessingDB::GetStats_Internal(bool bDBLocked, long nActionID,
	VARIANT_BOOL vbForceUpdate, IActionStatistics* *pStats)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				ipConnection = getDBConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();
				
				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				// return a new object with the statistics
				UCLID_FILEPROCESSINGLib::IActionStatisticsPtr ipActionStats =  
					loadStats(ipConnection, nActionID, asCppBool(vbForceUpdate), bDBLocked);
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
				ipConnection = getDBConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				string strFrom = getActionName(ipConnection, nFromAction);
				string strTo = getActionName(ipConnection, nToAction);

				TransactionGuard tg(ipConnection, adXactIsolated, &m_mutex);

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
bool CFileProcessingDB::RenameAction_Internal(bool bDBLocked, long nActionID, BSTR strNewActionName)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				ipConnection = getDBConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Make sure processing is not active of this action
				assertProcessingNotActiveForAction(bDBLocked, ipConnection, nActionID);

				// Convert action names to string
				string strOld = getActionName(ipConnection, nActionID);
				string strNew = asString(strNewActionName);

				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				// Change the name of the action in the action table
				string strSQL = "UPDATE Action SET ASCName = '" + strNew + "' WHERE ID = " + asString(nActionID);
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
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();


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
												 VARIANT_BOOL vbSetIfExists)
{
	try
	{
		try
		{
			// Convert setting name and value to string 
			string strSettingName = asString(bstrSettingName);
			string strSettingValue = asString(bstrSettingValue);

			// Setup Setting Query
			string strSQL = gstrDBINFO_SETTING_QUERY;
			replaceVariable(strSQL, gstrSETTING_NAME, strSettingName);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				ipConnection = getDBConnection();

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
				ipDBInfoSet->Open(strSQL.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenDynamic, 
					adLockOptimistic, adCmdText); 

				// Check if setting record exists
				bool bExists = ipDBInfoSet->adoEOF == VARIANT_FALSE;

				// Continue if the setting is new or we are changing an existing setting
				if (!bExists || vbSetIfExists == VARIANT_TRUE)
				{
					if (!bExists)
					{
						// Setting does not exist so add it
						ipDBInfoSet->AddNew();

						setStringField(ipDBInfoSet->Fields, "Name", strSettingName, true);
					}

					// Set the value field to the new value
					setStringField(ipDBInfoSet->Fields, "Value", strSettingValue);

					// Update the database
					ipDBInfoSet->Update();

					executeCmdQuery(ipConnection, gstrUPDATE_DB_INFO_LAST_CHANGE_TIME);
				}

				// Commit transaction
				tg.CommitTrans();

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
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

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
bool CFileProcessingDB::NotifyFileSkipped_Internal(bool bDBLocked, long nFileID, long nActionID)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				ipConnection = getDBConnection();

				// Get the action name
				string strActionName = getActionName(ipConnection, nActionID);

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_mutex);

				// Set the file state to skipped unless there is a pending state in the
				// QueuedActionStatusChange table.
				setFileActionState(ipConnection, nFileID, strActionName, "S", "", false, true, nActionID);

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
			string strUserName = getCurrentUserName();

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

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
bool CFileProcessingDB::ModifyActionStatusForQuery_Internal(bool bDBLocked, BSTR bstrQueryFrom, 
															BSTR bstrToAction, EActionStatus eaStatus, 
															BSTR bstrFromAction, 
															IRandomMathCondition* pRandomCondition,
															long* pnNumRecordsModified)
{
	try
	{
		try
		{
			// Check that an action name and a FROM clause have been passed in
			string strQueryFrom = asString(bstrQueryFrom);
			ASSERT_ARGUMENT("ELI30380", !strQueryFrom.empty());
			string strToAction = asString(bstrToAction);
			ASSERT_ARGUMENT("ELI30381", !strToAction.empty());

			// Wrap the random condition (if there is one, in a smart pointer)
			UCLID_FILEPROCESSINGLib::IRandomMathConditionPtr ipRandomCondition(pRandomCondition);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				ipConnection = getDBConnection();

				// Determine the source of the new status
				string strFromAction = asString(bstrFromAction);
				bool bFromSpecified = !strFromAction.empty();
				string strStatus = "";
				long nFromActionID = 0;
				if (bFromSpecified)
				{
					nFromActionID = getActionID(ipConnection, strFromAction);
				}
				else
				{
					// Get the new status as a string
					strStatus = asStatusString(eaStatus);
				}

				validateDBSchemaVersion();

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactIsolated, &m_mutex);

				_RecordsetPtr ipFileSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI30382", ipFileSet != __nullptr);

				// Open the file set
				ipFileSet->Open(strQueryFrom.c_str(), _variant_t((IDispatch*)ipConnection, true),
					adOpenForwardOnly, adLockReadOnly, adCmdText);

				// Create an empty file record object for the random condition.
				UCLID_FILEPROCESSINGLib::IFileRecordPtr ipFileRecord(CLSID_FileRecord);
				ipFileRecord->Name = "";
				ipFileRecord->FileID = 0;

				// Get the list of file ID's to modify
				long nNumRecordsModified = 0;
				vector<long> vecFileIds;
				while (ipFileSet->adoEOF == VARIANT_FALSE)
				{
					if (ipRandomCondition == __nullptr || ipRandomCondition->CheckCondition(ipFileRecord, 0) == VARIANT_TRUE)
					{
						// Get the file ID
						vecFileIds.push_back(getLongField(ipFileSet->Fields, "ID"));

						nNumRecordsModified++;
					}

					// Move to next record
					ipFileSet->MoveNext();
				}
				ipFileSet->Close();

				// Action id to change
				long nToActionID = getActionID(ipConnection, strToAction);
				string strToActionID = asString(nToActionID);

				// Loop through the file Ids to change in groups of 10000 populating the SetFileActionData
				size_t count = vecFileIds.size();
				size_t i = 0;
				string strSelectQuery;
				if (!bFromSpecified)
				{
					strSelectQuery = "SELECT FAMFile.ID, FileName, FileSize, Pages, "
						"COALESCE (ToFAS.Priority, FAMFile.Priority) AS Priority, "
						"COALESCE (ToFAS.ActionStatus, 'U') AS ToActionStatus "
						"FROM FAMFile LEFT JOIN FileActionStatus as ToFAS "
						"ON FAMFile.ID = ToFAS.FileID AND ToFAS.ActionID = " + strToActionID;
				}
				else
				{
					strSelectQuery = "SELECT FAMFile.ID, FileName, FileSize, Pages, "
						"COALESCE(ToFAS.Priority, FAMFile.Priority) AS Priority, "
						"COALESCE(ToFAS.ActionStatus, 'U') AS ToActionStatus, "
						"COALESCE(FromFAS.ActionStatus, 'U') AS FromActionStatus "
						"FROM FAMFile LEFT JOIN FileActionStatus as ToFAS "
						"ON FAMFile.ID = ToFAS.FileID AND ToFAS.ActionID = " + strToActionID +
						" LEFT JOIN FileActionStatus as FromFAS ON FAMFile.ID = FromFAS.FileID AND "
						"FromFAS.ActionID = " + asString(nFromActionID);
				}
				while (i < count)
				{
					map<string, vector<SetFileActionData>> mapFromStatusToId;

					string strQuery = strSelectQuery + " WHERE FAMFile.ID IN (";
					string strFileIds = asString(vecFileIds[i++]);
					for (int j=1; i < count && j < 10000; j++)
					{
						strFileIds += ", " + asString(vecFileIds[i++]);
					}
					strQuery += strFileIds;
					strQuery += ")";
					ipFileSet->Open(strQuery.c_str(), _variant_t((IDispatch*)ipConnection, true),
						adOpenForwardOnly, adLockReadOnly, adCmdText);

					// Loop through each record
					while (ipFileSet->adoEOF == VARIANT_FALSE)
					{
						FieldsPtr ipFields = ipFileSet->Fields;
						ASSERT_RESOURCE_ALLOCATION("ELI30383", ipFields != __nullptr);

						long nFileID = getLongField(ipFields, "ID");
						EActionStatus oldStatus = asEActionStatus(getStringField(ipFields, "ToActionStatus"));

						// If copying from an action, get the status for the action
						if (bFromSpecified)
						{
							strStatus = getStringField(ipFields, "FromActionStatus");
						}

						mapFromStatusToId[strStatus].push_back(SetFileActionData(nFileID,
							getFileRecordFromFields(ipFields, false), oldStatus));

						ipFileSet->MoveNext();
					}
					ipFileSet->Close();

					// Set the file action state for each vector of file data
					for(map<string, vector<SetFileActionData>>::iterator it = mapFromStatusToId.begin();
						it != mapFromStatusToId.end(); it++)
					{
						setFileActionState(ipConnection, it->second, strToAction, it->first); 
					}
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
		if (!bDBLocked)
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

			// Create query to get the tags and descriptions
			string strQuery = "SELECT [TagName], [TagDescription] FROM [Tag]";

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

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
bool CFileProcessingDB::SetStatusForFilesWithTags_Internal(bool bDBLocked, IVariantVector *pvecTagNames,
														  VARIANT_BOOL vbAndOperation,
														  long nToActionID,
														  EActionStatus eaNewStatus,
														  long nFromActionID)
{
	try
	{
		try
		{
			IVariantVectorPtr ipVecTagNames(pvecTagNames);
			ASSERT_ARGUMENT("ELI30385", ipVecTagNames != __nullptr);

			long lSize = ipVecTagNames->Size;

			// If no tags specified do nothing
			if (lSize == 0)
			{
				return true;
			}

			string strConjunction = asCppBool(vbAndOperation) ? "\nINTERSECT\n" : "\nUNION\n";

			string strQuery = gstrQUERY_FILES_WITH_TAGS;
			replaceVariable(strQuery, gstrTAG_NAME_VALUE, asString(ipVecTagNames->GetItem(0).bstrVal));

			for (long i=1; i < lSize; i++)
			{
				string strTagName = asString(ipVecTagNames->GetItem(i).bstrVal);
				if (!strTagName.empty())
				{
					string strTemp = gstrQUERY_FILES_WITH_TAGS;
					replaceVariable(strTemp, gstrTAG_NAME_VALUE,
						asString(ipVecTagNames->GetItem(i).bstrVal));
					strQuery += strConjunction + strTemp;
				}
			}

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				ipConnection = getDBConnection();

				bool bFromAction = nFromActionID != -1;
				string strStatus = "";
				string strFromAction = "";
				if (bFromAction)
				{
					replaceVariable(strQuery, gstrTAG_QUERY_SELECT,
						"[FAMFile].[ID], COALESCE(ActionStatus, 'U' AS ActionStatus");
					replaceVariable(strQuery, "<ActionStatusJoin>", 
						"LEFT JOIN FileActionStatus ON FileTag.FileID = FileActionStatus.FileID "
						"AND FileActionStatus.ActionID = " + asString(nFromActionID));
				}
				else
				{
					replaceVariable(strQuery, gstrTAG_QUERY_SELECT, "[FAMFile].[ID]");
					replaceVariable(strQuery, "<ActionStatusJoin>", "");

					// Get the new status as a string
					strStatus = asStatusString(eaNewStatus);
				}

				// Get the action name 
				string strToAction = getActionName(ipConnection, nToActionID);

				// Set the transaction guard
				TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_mutex);

				_RecordsetPtr ipFileSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI30386", ipFileSet != __nullptr);

				// Open the file set
				ipFileSet->Open(strQuery.c_str(), _variant_t((IDispatch*)ipConnection, true),
					adOpenForwardOnly, adLockReadOnly, adCmdText);

				// Loop through each record
				while (ipFileSet->adoEOF == VARIANT_FALSE)
				{
					FieldsPtr ipFields = ipFileSet->Fields;
					ASSERT_RESOURCE_ALLOCATION("ELI30387", ipFields != __nullptr);

					// Get the file ID
					long nFileID = getLongField(ipFields, "ID");

					// If copying from an action, get the status for the action
					if (bFromAction)
					{
						strStatus = getStringField(ipFields, "ActionStatus");
					}

					// Set the file action state
					// This call should not be made from file processing and the new state should
					// overrule any pending state for the file in the QueuedActionStatusChange table.
					setFileActionState(ipConnection, nFileID, strToAction, strStatus, "", false,
						false, nToActionID);

					// Move to next record
					ipFileSet->MoveNext();
				}

				// Commit the transaction
				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI30388");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30686");
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
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

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
			// Stop thread here
			m_eventStopMaintainenceThreads.signal();

			// Wait for the ping and statistics maintenance threads to exit.
			HANDLE handles[2];
			handles[0] = m_eventPingThreadExited.getHandle();
			handles[1] = m_eventStatsThreadExited.getHandle();
			if (WaitForMultipleObjects(2, (HANDLE *)&handles, TRUE, gnPING_TIMEOUT) != WAIT_OBJECT_0)
			{
				UCLIDException ue("ELI27857", "Application Trace: Timed out waiting for thread to exit.");
				ue.log();
			}
			
			// set FAMRegistered flag to false since thread has exited
			m_bFAMRegistered = false;
			m_nActiveActionID = -1;

			// Set the transaction guard
			TransactionGuard tg(getDBConnection(), adXactRepeatableRead, &m_mutex);

			// Make sure there are no linked records in the LockedFile table 
			// and if there are records reset there status to StatusBeforeLock if there current
			// state for the action is processing.
			UCLIDException uex("ELI30304", "Application Trace: Files were reverted to original status.");
			revertLockedFilesToPreviousState(getDBConnection(), m_nActiveFAMID,
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
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

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

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Set the transaction guard
				TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_mutex);

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
								long lActionID, VARIANT_BOOL vbQueuing, VARIANT_BOOL vbProcessing)
{
	try
	{
		try
		{
			// Get the FPS File name
			string strFPSFileName = asString(bstrFPSFileName);

			string strFAMSessionQuery = "INSERT INTO [" + gstrFAM_SESSION + "] ";
			strFAMSessionQuery += "([MachineID], [FAMUserID], [UPI], [FPSFileID], [ActionID], "
				"[Queuing], [Processing]) ";
			strFAMSessionQuery += "OUTPUT INSERTED.ID ";
			strFAMSessionQuery += "VALUES (";

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				ipConnection = getDBConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Set the transaction guard
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				// Get FPSFileID, MachineID, and UserID (this will add records if they don't exist)
				long nFPSFileID = getKeyID(ipConnection, gstrFPS_FILE, "FPSFileName",
					strFPSFileName.empty() ? "<Unsaved FPS File>" : strFPSFileName);
				long nMachineID = getKeyID(ipConnection, gstrMACHINE, "MachineName", m_strMachineName);
				long nUserID = getKeyID(ipConnection, gstrFAM_USER, "UserName", m_strFAMUserName);
				string strQueuing = (asCppBool(vbQueuing) ? "1" : "0");
				string strProcessing = (asCppBool(vbProcessing) ? "1" : "0");

				strFAMSessionQuery += asString(nMachineID) + ", " + asString(nUserID) + ", '"
					+ m_strUPI + "', " + asString(nFPSFileID) + ", " + asString(lActionID) +
					", " + strQueuing + ", " + strProcessing + ")";

				// Insert the record into the FAMSession table
				executeCmdQuery(ipConnection, strFAMSessionQuery, false, (long*)&m_nFAMSessionID);
				m_nActiveActionID = lActionID;

				// Commit the transaction
				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI28903");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30696");
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
				ipConnection = getDBConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Set the transaction guard
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				// Execute the update query
				executeCmdQuery(ipConnection, strFAMSessionQuery);

				// Commit the transaction
				tg.CommitTrans();

			END_CONNECTION_RETRY(ipConnection, "ELI28905");

			m_nFAMSessionID = 0;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30697");
	}
	catch(UCLIDException &ue)
	{
		m_nFAMSessionID = 0;

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
				ipConnection = getDBConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				if (!isInputEventTrackingEnabled(ipConnection))
				{
					throw UCLIDException("ELI28966", "Input event tracking is not currently enabled.");
				}

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

				// Delete the old input events
				deleteOldInputEvents(ipConnection);

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
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				// Create a pointer to a recordset containing the action
				_RecordsetPtr ipActionSet = getActionSet(ipConnection, strActionName);
				ASSERT_RESOURCE_ALLOCATION("ELI29177", ipActionSet != __nullptr);

				// Check if the action is not yet created
				if (ipActionSet->adoEOF == VARIANT_TRUE)
				{
					// Action is not created
					if (getDBInfoSetting(ipConnection, gstrAUTO_CREATE_ACTIONS, true) == "1")
					{
						// AutoCreateActions is set, create the action
						*plId = addActionToRecordset(ipConnection, ipActionSet, strActionName);
					}
					else
					{
						// AutoCreateActions is not set, throw an exception
						UCLIDException ue("ELI29157", "Invalid action name.");
						ue.addDebugInfo("Action name", strActionName);
						throw ue;
					}
				}
				else
				{
					// Action is already created, get its ID
					*plId = getLongField(ipActionSet->Fields, "ID");
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
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				string strSelectSQL = "SELECT FAMFile.ID, FileName, Pages, FileSize, "
					"COALESCE(FileActionStatus.Priority, FAMFile.Priority) AS Priority, "
					"COALESCE(ActionStatus, 'U') AS ActionStatus "
					"FROM FAMFile LEFT JOIN FileActionStatus ON "
					"FAMFile.ID = FileID AND ActionID = " + asString(nActionID) +
					" WHERE FAMFile.ID = " + asString(nFileId);

				// Perform all processing related to setting a file as processing.
				// The previous status of the files to process is expected to be either pending or
				// skipped.
				setFilesToProcessing(bDBLocked, ipConnection, strSelectSQL, nActionID, "PS");

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
			// Make sure all Product specific DB managers have been recognized.
			checkForNewDBManagers();

			m_bValidatingOrUpdatingSchema = true;

			// Assume a lock is going to be necessary for a schema update.
			ASSERT_ARGUMENT("ELI31401", bDBLocked == true);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			ipProgressStatus->InitProgressStatus("Inspecting schema...", 0, 0, VARIANT_TRUE);

			BEGIN_CONNECTION_RETRY();

			ipConnection = getDBConnection();

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
				case 132:	break;

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

			ipProgressStatus->InitProgressStatus(
				"Updating database schema...", 0, nTotalStepCount, VARIANT_TRUE);

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
				ipProgressStatus->StartNextItemGroup(zMessage.GetString(), vecStepCounts[i]);


				executeProdSpecificSchemaUpdateFuncs(ipConnection, ipProdSpecificMgrs,
					nSchemaVersion, NULL, ipProgressStatus->SubProgressStatus,
					mapProductSpecificVersions);

				if (i < nFuncCount)
				{
					nSchemaVersion = vecUpdateFuncs[i](ipConnection, NULL, ipProgressStatus->SubProgressStatus);
				}

				ipProgressStatus->CompleteCurrentItemGroup();
			}

			// Update last DB info change time since any schema update will have needed to update
			// the schema version
			executeCmdQuery(ipConnection, gstrUPDATE_DB_INFO_LAST_CHANGE_TIME);

			tg.CommitTrans();

			UCLIDException ue("ELI32551", "Application Trace: Database schema updated.");
			ue.addDebugInfo("Old version", nOriginalSchemaVersion);
			ue.addDebugInfo("New version", nSchemaVersion);
			ue.log();

			// Force the DBInfo values (including schema version) to be reloaded on the next call to
			// validateDBSchemaVersion
			m_iDBSchemaVersion = 0; 

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

			string strChangeNameQuery = "UPDATE [FAMFile]   SET [FileName] = '" + strNewNameForQuery + 
				"' WHERE FileName = '" + strCurrFileName + "' AND ID = " + strFileID;

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				ipConnection = getDBConnection();

				// Set the transaction guard
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				long lRecordsAffected = executeCmdQuery(ipConnection, strChangeNameQuery);

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
					string strChangeHistoryQuery = "INSERT INTO [SourceDocChangeHistory]  ([FileID], [FromFileName], "
						"[ToFileName], [TimeStamp], [FAMUserID], [MachineID]) VALUES "
						"(" + strFileID + ", '" + strCurrFileName + "', '" + strNewNameForQuery + "', GetDate(), " 
						+ asString(getFAMUserID(ipConnection)) + ", " + asString(getMachineID(ipConnection)) + ")";

					executeCmdQuery(ipConnection, strChangeHistoryQuery);
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

			IStrToStrMapPtr ipSettings(CLSID_StrToStrMap);
			ASSERT_RESOURCE_ALLOCATION("ELI31896", ipSettings != __nullptr);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				ipConnection = getDBConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Create a pointer to a recordset
				_RecordsetPtr ipDBInfoSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI31897", ipDBInfoSet != __nullptr);

				// Open the record set using the Setting Query		
				ipDBInfoSet->Open(gstrDBINFO_GET_SETTINGS_QUERY.c_str(),
					_variant_t((IDispatch *)ipConnection, true), adOpenForwardOnly,
					adLockReadOnly, adCmdText); 

				while (ipDBInfoSet->adoEOF == VARIANT_FALSE)
				{
					FieldsPtr ipFields = ipDBInfoSet->Fields;
					ASSERT_RESOURCE_ALLOCATION("ELI31898", ipFields != __nullptr);

					string strKey = getStringField(ipFields, "Name");
					string strValue = getStringField(ipFields, "Value");
					ipSettings->Set(strKey.c_str(), strValue.c_str());

					ipDBInfoSet->MoveNext();
				}

				*ppSettings = ipSettings.Detach();

			END_CONNECTION_RETRY(ipConnection, "ELI31899");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI31900");
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
bool CFileProcessingDB::SetDBInfoSettings_Internal(bool bDBLocked, bool bUpdateHistory,
	vector<string> vecQueries, long& rnNumRowsUpdated)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				ipConnection = getDBConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Set the transaction guard
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				if (bUpdateHistory)
				{
					// If updating history, need to get user and machine id
					string strUserId = asString(getFAMUserID(ipConnection));
					string strMachineId = asString(getMachineID(ipConnection));

					// Update the queries with the user and machine id
					for(vector<string>::iterator it = vecQueries.begin();
						it != vecQueries.end(); it++)
					{
						replaceVariable(*it, gstrUSER_ID_VAR, strUserId, kReplaceAll);
						replaceVariable(*it, gstrMACHINE_ID_VAR, strMachineId, kReplaceAll);
					}
				}

				// Execute query and get count of updated rows
				rnNumRowsUpdated = executeVectorOfSQL(ipConnection, vecQueries);

				// If at least 1 row was updated, update the last DB info changed value
				if (rnNumRowsUpdated > 0)
				{
					executeCmdQuery(ipConnection, gstrUPDATE_DB_INFO_LAST_CHANGE_TIME);
				}

				tg.CommitTrans();

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
			ipConnection = getDBConnection();

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
			ipConnection = getDBConnection();

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

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			ipConnection = getDBConnection();

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
				if (!m_bDeniedFastCountPermission)
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
							ASSERT_RESOURCE_ALLOCATION("ELI35762", ipResultSet != __nullptr);
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

				if (m_bDeniedFastCountPermission)
				{
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
				*pnFileCount = !bUseOracle && m_bDeniedFastCountPermission
					? (long long)getLongField(ipResultSet->Fields, gstrTOTAL_FILECOUNT_FIELD)
					: getLongLongField(ipResultSet->Fields, gstrTOTAL_FILECOUNT_FIELD);
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

				ipConnection = getDBConnection();

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

			// Convert the nWorkItemGroupID to a string
			string strWorkItemGroupID = asString(nWorkItemGroupID);

			// Setup query of workItems for the given work group id and sequence number in the range
			// nStartPos to nStartPos + 1
			string strWorkItemSQL = gstrGET_WORK_ITEM_FOR_GROUP_IN_RANGE;
			replaceVariable(strWorkItemSQL, "<WorkItemGroupID>", strWorkItemGroupID);
			replaceVariable(strWorkItemSQL, "<StartSequence>", asString(nStartPos));
			replaceVariable(strWorkItemSQL, "<EndSequence>", asString(nStartPos + nCount));

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				ipConnection = getDBConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Create a pointer to a recordset
				_RecordsetPtr ipWorkItemSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI36870", ipWorkItemSet != __nullptr);

				// Execute the query get the set of WorkItems
				ipWorkItemSet->Open(strWorkItemSQL.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
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
				ipConnection = getDBConnection();

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

			// Create the query to get matching work item group id
			string strGetExisting = gstrGET_WORK_ITEM_GROUP_ID;
			replaceVariable(strGetExisting, "<FileID>", strFileID);
			replaceVariable(strGetExisting, "<ActionID>", strActionID);
			replaceVariable(strGetExisting, "<StringizedSettings>", strStringizedTask);
			replaceVariable(strGetExisting, "<NumberOfWorkItems>", strNumberOfWorkItems);

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
				ipConnection = getDBConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();
				
				*pnWorkItemGroupID = -1;
				try
				{
					// see if group already exists
					executeCmdQuery(ipConnection, strGetExisting, false, pnWorkItemGroupID);

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
				TransactionGuard tg(ipConnection,adXactRepeatableRead, &m_mutex);

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
				ipConnection = getDBConnection();

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
				TransactionGuard tg(ipConnection,adXactRepeatableRead, &m_mutex);

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
bool CFileProcessingDB::GetWorkItemToProcess_Internal(bool bDBLocked, long nActionID,
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
				ipConnection = getDBConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				UCLID_FILEPROCESSINGLib::IWorkItemRecordPtr ipWorkItem = 
					setWorkItemToProcessing(bDBLocked, nActionID,
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
				ipConnection = getDBConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				TransactionGuard tg(ipConnection,adXactRepeatableRead, &m_mutex);

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
				ipConnection = getDBConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				TransactionGuard tg(ipConnection,adXactRepeatableRead, &m_mutex);

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
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				TransactionGuard tg(ipConnection,adXactRepeatableRead, &m_mutex);

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
				ipConnection = getDBConnection();

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

					TransactionGuard tg(ipConnection,adXactRepeatableRead, &m_mutex);
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
				ipConnection = getDBConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				TransactionGuard tg(ipConnection,adXactRepeatableRead, &m_mutex);
				
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
			ipConnection = getDBConnection();

			// Set up a query to get the filename for every ID in the fileset.
			vector<int>& vecFileIDs = iterFileSet->second;
			long nCount = vecFileIDs.size();
			for (long i = 0; i < nCount;)
			{
				string strQuery = "SELECT [FileName] FROM [FAMFile] WHERE [ID] IN (";
			
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

			ipConnection = getDBConnection();

			// Make sure the DB Schema is the expected version
			validateDBSchemaVersion();

			TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_mutex);

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
bool CFileProcessingDB::GetWorkItemsToProcess_Internal(bool bDBLocked, long nActionID,
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
				ipConnection = getDBConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				IIUnknownVectorPtr ipWorkItems = 
					setWorkItemsToProcessing(bDBLocked, nActionID, nMaxWorkItemsToReturn, 
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
				ipConnection = getDBConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				TransactionGuard tg(ipConnection,adXactRepeatableRead, &m_mutex);

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

			// Convert the nWorkItemGroupID to a string
			string strWorkItemGroupID = asString(nWorkItemGroupID);

			// Setup query of failed work items for the given work group id
			string strWorkItemSQL = gstrGET_FAILED_WORK_ITEM_FOR_GROUP;
			replaceVariable(strWorkItemSQL, "<WorkItemGroupID>", strWorkItemGroupID);

			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				ipConnection = getDBConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Create a pointer to a recordset
				_RecordsetPtr ipWorkItemSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI37543", ipWorkItemSet != __nullptr);

				// Execute the query get the set of WorkItems
				ipWorkItemSet->Open(strWorkItemSQL.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
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

			string strQuery =
				"DECLARE @fieldID INT "
				"SELECT @fieldID = [ID] FROM [MetadataField] "
				"	WHERE [Name] = '<MetadataFieldName>' "

				"IF EXISTS (SELECT * FROM [FileMetadataFieldValue] WHERE [FileID] = <FileID> AND [MetadataFieldID] = @fieldID) "
				"	UPDATE [FileMetadataFieldValue] SET [Value] = '<MetadataFieldValue>' "
				"		WHERE [FileID] = <FileID> AND [MetadataFieldID] = @fieldID "
				"ELSE "
				"	INSERT INTO [FileMetadataFieldValue] ([FileID], [MetadataFieldID], [Value]) "
				"		VALUES (<FileID>, @fieldID, '<MetadataFieldValue>')";

			replaceVariable(strQuery, "<FileID>", asString(nFileID));
			replaceVariable(strQuery, "<MetadataFieldName>", asString(bstrMetadataFieldName));
			replaceVariable(strQuery, "<MetadataFieldValue>", asString(bstrMetadataFieldValue));

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				ipConnection = getDBConnection();

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_mutex);

				ipConnection->Execute(strQuery.c_str(), NULL, adCmdText);

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
				"SELECT [Value] FROM [FileMetadataFieldValue] "
					"INNER JOIN [MetadataField] ON [MetadataField].[ID] = [FileMetadataFieldValue].[MetadataFieldID] "
					"WHERE [FileID] = <FileID> AND [Name] = '<MetadataFieldName>'";

			replaceVariable(strQuery, "<FileID>", asString(nFileID));
			replaceVariable(strQuery, "<MetadataFieldName>", asString(bstrMetadataFieldName));

			BEGIN_CONNECTION_RETRY();

				// Get the connection for the thread and save it locally.
				ipConnection = getDBConnection();
				
				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Create a pointer to a recordset
				_RecordsetPtr ipResult(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI37639", ipResult != __nullptr);

				// Execute the query get the set of WorkItems
				ipResult->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
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
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

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
				ipConnection = getDBConnection();

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
bool CFileProcessingDB::RecordFileTaskSession_Internal(bool bDBLocked, BSTR bstrTaskClassGuid, 
					long nFileID, double dDuration, double dOverheadTime, long *pnFileTaskSessionID)
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
			ipConnection = getDBConnection();

			validateDBSchemaVersion();

			string strInsertSQL = gstrINSERT_FILETASKSESSION_DATA;
			replaceVariable(strInsertSQL, "<FAMSessionID>", asString(m_nFAMSessionID));
			replaceVariable(strInsertSQL, "<TaskClassGuid>", asString(bstrTaskClassGuid));
			replaceVariable(strInsertSQL, "<FileID>", asString(nFileID));
			replaceVariable(strInsertSQL, "<Duration>", asString(dDuration));
			replaceVariable(strInsertSQL, "<OverheadTime>", asString(dOverheadTime));

			long nFileTaskSessionID = 0;
			executeCmdQuery(ipConnection, strInsertSQL, false, pnFileTaskSessionID);
			
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
bool CFileProcessingDB::GetSecureCounterName_Internal(bool bDBLocked, long nCounterID, BSTR *pstrCounterName)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();
			
				ipConnection = getDBConnection();

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
bool CFileProcessingDB::ApplySecureCounterUpdateCode_Internal(bool bDBLocked, BSTR strUpdateCode)
{

	try
	{
		try
		{
			_ConnectionPtr ipConnection;
			BEGIN_CONNECTION_RETRY();

				ipConnection = getDBConnection();
				bool bValid = checkDatabaseIDValid(ipConnection, false);

				ByteStream bsPW;
				getFAMPassword(bsPW);

				// Get the bytestream from the update code
				ByteStream bsUpgradeCode = MapLabel::getMapLabelWithS(asString(strUpdateCode), bsPW);
				ByteStreamManipulator bsmUpgradeCode(ByteStreamManipulator::kRead, bsUpgradeCode);

				DBCounterUpdate counterUpdates;

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_mutex);

				try
				{
					try
					{
						bsmUpgradeCode >> counterUpdates;

						ASSERT_RUNTIME_CONDITION("ELI38903", counterUpdates.m_nNumberOfUpdates != 0,
							"No counter updates in code.");

						if (counterUpdates.m_nNumberOfUpdates < 0 )
						{
							// this is an unlock code
							unlockCounters(ipConnection, counterUpdates);
						}
						else
						{
							ASSERT_RUNTIME_CONDITION("ELI38976", bValid, "DatabaseID is corrupt.")

							// Validate the guid and the LastUpdated time
							ASSERT_RUNTIME_CONDITION("ELI38902", m_DatabaseIDValues == counterUpdates.m_DatabaseID, 
								"Code is not valid.");

							updateCounters(ipConnection, counterUpdates);
						}
						tg.CommitTrans();
					}
					CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38904");
				}
				catch (UCLIDException &ue)
				{
					UCLIDException ueBad("ELI38905", "Unable to process counter upgrade code.", ue);
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

				ipConnection = getDBConnection();

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
					
					dbCounter.LoadFromFields(ipResultSet->Fields, m_DatabaseIDValues.m_nHashValue, false);

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
			ASSERT_RUNTIME_CONDITION("ELI39026", decrementAmount >= 0, "Decrement must be positive.");

			bool bCounterDecremented = false;
			long nDecrementRetries = 0;
			long nMillisecondsToWait = 30000;
			do 
			{
				_ConnectionPtr ipConnection;
				BEGIN_CONNECTION_RETRY();

					ipConnection = getDBConnection();

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
					TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_mutex);

					// Open the Action table
					ipResultSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
						adLockReadOnly, adCmdText);

					if (!asCppBool(ipResultSet->adoEOF))
					{
						FieldsPtr fields = ipResultSet->Fields;
						DBCounter dbCounter;
						dbCounter.LoadFromFields(ipResultSet->Fields, 
							m_DatabaseIDValues.m_nHashValue, false);

						DBCounterChangeValue dbCounterChange;
						dbCounterChange.m_nCounterID = dbCounter.m_nID;
						dbCounterChange.m_nFromValue = dbCounter.m_nValue;
						if (decrementAmount > dbCounter.m_nValue)
						{
							if (nDecrementRetries % 100 == 0)
							{
								UCLIDException ue("ELI38938", "Counter has insufficient counts.");
								ue.addDebugInfo("CounterName", dbCounter.m_strName);
								ue.addDebugInfo("CounterID", dbCounter.m_nID);
								ue.addDebugInfo("RequiredCounts", decrementAmount);
								ue.addDebugInfo("RemainingCountes", dbCounter.m_nValue);
								ue.addDebugInfo("nNumberOfRetries", nDecrementRetries);
								ue.log();
							}
							if (nDecrementRetries < 1000)
							{
								Sleep(nMillisecondsToWait);
								nMillisecondsToWait += 500;
								nDecrementRetries++;
							}
						}
						else
						{
							dbCounter.m_nValue -= decrementAmount;
							nDecrementRetries = 0;
							bCounterDecremented = true;

							dbCounterChange.m_nToValue = dbCounter.m_nValue;
							dbCounterChange.m_nLastUpdatedByFAMSessionID = m_nFAMSessionID;

							dbCounterChange.m_llMinFAMFileCount = m_nLastFAMFileID;
										
							dbCounterChange.m_ctUpdatedTime = getSQLServerDateTimeAsCTime(getDBConnection());

							dbCounterChange.CalculateHashValue(dbCounterChange.m_llHashValue);

							// list of queries to run
							vector<string> vecUpdateQueries;
							vecUpdateQueries.push_back("UPDATE [dbo].[SecureCounter] SET SecureCounterValue = '" + 
								dbCounter.getEncrypted(m_DatabaseIDValues.m_nHashValue) + 
								"' WHERE ID = " + asString(dbCounter.m_nID));

							vecUpdateQueries.push_back(dbCounterChange.GetInsertQuery());

							executeVectorOfSQL(ipConnection, vecUpdateQueries);
							tg.CommitTrans();
						}
					}

				END_CONNECTION_RETRY(ipConnection, "ELI38936");
			} while (!bCounterDecremented  && nDecrementRetries < 1000);
			
			if (nDecrementRetries >= 1000)
			{
				UCLIDException ue("ELI38985", "Could not decrement counter.");
				ue.addDebugInfo("NumberOfRetires", nDecrementRetries);
				throw ue;
			}
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

bool CFileProcessingDB::GetCounterUpdateRequestCode_Internal(bool bDBLocked, BSTR* pstrUpdateRequestCode)
{
	try
	{
		try
		{
			// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
			ADODB::_ConnectionPtr ipConnection = __nullptr;

			BEGIN_CONNECTION_RETRY();

				ipConnection = getDBConnection();
				bool bValid = checkDatabaseIDValid(ipConnection, false);

				DatabaseIDValues DBIDValue = m_DatabaseIDValues;

				if (!bValid)
				{
					// Modify the DBIdValue to have corrected values (m_GUID and m_ctLastUpdated will be the same)
					getDatabaseCreationDateAndRestoreDate(ipConnection, m_strDatabaseName, 
						DBIDValue.m_strServer, DBIDValue.m_ctCreated, DBIDValue.m_ctRestored);
					DBIDValue.m_strName = m_strDatabaseName;
				}

				ByteStream bsRequestCode;
				ByteStreamManipulator bsmRequest(ByteStreamManipulator::kWrite, bsRequestCode);
				
				bsmRequest << DBIDValue;

				// Add the current time
				bsmRequest << getSQLServerDateTimeAsCTime(getDBConnection());

				// Add code to get counters info
				// 
				string strQuery = "SELECT * FROM [dbo].[SecureCounter]";
  
				// Create a pointer to a recordset
				_RecordsetPtr ipResultSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI38907", ipResultSet != __nullptr);

				// Make sure the DB Schema is the expected version
				validateDBSchemaVersion();

				// Open the Action table
				ipResultSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
					adLockReadOnly, adCmdText);
				
				vector<DBCounter> vecDBCounters;
				while (!asCppBool(ipResultSet->adoEOF))
				{
					DBCounter dbCounter;
					dbCounter.LoadFromFields(ipResultSet->Fields);

					bValid = bValid && dbCounter.isValid(m_DatabaseIDValues.m_nHashValue);

					vecDBCounters.push_back(dbCounter);

					ipResultSet->MoveNext();
				}
				long nNumCounters = vecDBCounters.size();
				// if the number of counters is 0 and the DatabaseID is invalid
				// create a completely new DatabaseID value and save it in DBInfo
				// then bValid will be set to true and this becomes a request code instead
				if (!bValid && nNumCounters == 0)
				{
					// Create a new DatabaseID and encrypt it
					ByteStream bsDatabaseID;
					createDatabaseID(ipConnection, bsDatabaseID);

					ByteStream bsPW;
					getFAMPassword(bsPW);
					m_strEncryptedDatabaseID = MapLabel::setMapLabelWithS(bsDatabaseID,bsPW);
					string strUpdateQuery = gstrDBINFO_UPDATE_SETTINGS_QUERY;
					replaceVariable(strUpdateQuery, gstrSETTING_NAME, gstrDATABASEID);
					replaceVariable(strUpdateQuery, gstrSETTING_VALUE, m_strEncryptedDatabaseID);
					executeCmdQuery(ipConnection,gstrDBINFO_UPDATE_SETTINGS_QUERY);
					bValid = true;
				}


				bsmRequest << (((nNumCounters == 0) || bValid) ? nNumCounters : -nNumCounters);

				for (auto c = vecDBCounters.begin(); c != vecDBCounters.end(); c++)
				{
					bsmRequest << c->m_nID;
					if (c->m_nID >= 100)
					{
						bsmRequest << c->m_strName;
					}						

					bsmRequest << c->m_nValue;
				}

				bsmRequest.flushToByteStream(8);

				// Get the password 'key' based on the 4 hex global variables
				ByteStream pwBS;
				getFAMPassword(pwBS);
				
				// Create the | separated list
				string strCode = MapLabel::setMapLabelWithS(bsRequestCode, pwBS);
				
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