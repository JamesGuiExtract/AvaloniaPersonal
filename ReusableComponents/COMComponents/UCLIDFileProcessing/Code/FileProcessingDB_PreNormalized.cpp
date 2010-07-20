// FileProcessingDB.cpp : Implementation of CFileProcessingDB

#include "stdafx.h"
#include "FileProcessingDB.h"
#include "FAMDB_SQL.h"

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

#include <string>
#include <stack>

using namespace std;
using namespace ADODB;

//-------------------------------------------------------------------------------------------------
// PURPOSE:	 The purpose of this macro is to declare and initialize local variables and define the
//			 beginning of a do...while loop that contains a try...catch block to be used to retry
//			 the block of code between the BEGIN_CONNECTION_RETRY macro and the END_CONNECTION_RETRY
//			 macro.  If an exception is thrown within the block of code between the connection retry
//			 macros the connection passed to END_CONNECTION_RETRY macro will be tested to see if it 
//			 is a good connection if it is the caught exception is rethrown, if it is no longer a 
//			 good connection a check is made to see the retry count is equal to maximum retries, if
//			 not, the exception will be logged if this is the first retry and the connection will be
//			 reinitialized.  If the number of retires is exceeded the exception will be rethrown.
// REQUIRES: An ADODB::ConnectionPtr variable to be declared before the BEGIN_CONNECTION_RETRY macro
//			 is used so it can be passed to the END_CONNECTION_RETRY macro.
//-------------------------------------------------------------------------------------------------
#define BEGIN_CONNECTION_RETRY() \
		int nRetryCount = 0; \
		bool bRetryExceptionLogged = false; \
		bool bRetrySuccess = false; \
		do \
		{ \
			CSingleLock retryLock(&m_mutex, TRUE); \
			try \
			{\
				try\
				{\

//-------------------------------------------------------------------------------------------------
// PURPOSE:	 To define the end of the block of code to be retried. (see above)
#define END_CONNECTION_RETRY(ipRetryConnection, strELICode) \
					bRetrySuccess = true; \
				}\
				CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION(strELICode)\
			} \
			catch(UCLIDException ue) \
			{ \
				if (isConnectionAlive(ipRetryConnection) || nRetryCount >= m_iNumberOfRetries) \
				{ \
					throw ue; \
				}\
				if (!bRetryExceptionLogged) \
				{ \
					UCLIDException uex("ELI30337", "Database connection failed. Attempting to reconnect.", ue); \
					uex.log(); \
					bRetryExceptionLogged = true; \
				} \
				reConnectDatabase(); \
				nRetryCount++; \
			} \
		} \
		while (!bRetrySuccess);
//-------------------------------------------------------------------------------------------------

// add license management function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// Interface fuctions for IFileProcessingDB
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::DefineNewAction2(BSTR strAction, long* pnID)
{
	string strActionName = asString(strAction);

	// Validate the new action name
	validateNewActionName(strActionName);

	// Check License
	validateLicense();

	// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
	ADODB::_ConnectionPtr ipConnection = NULL;

	BEGIN_CONNECTION_RETRY();

	// Get the connection for the thread and save it locally.
	ipConnection = getDBConnection();

	// Lock the database for this instance
	LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

	// Make sure the DB Schema is the expected version
	validateDBSchemaVersion();

	// Begin a transaction
	TransactionGuard tg(ipConnection);

	*pnID = defineNewAction(ipConnection, strActionName);

	// Commit this transaction
	tg.CommitTrans();

	END_CONNECTION_RETRY(ipConnection, "ELI23524");
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::DeleteAction2(BSTR strAction)
{

	// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
	ADODB::_ConnectionPtr ipConnection = NULL;

	BEGIN_CONNECTION_RETRY();

	// Get the connection for the thread and save it locally.
	ipConnection = getDBConnection();

	string strActionName = asString(strAction);

	// Check License
	validateLicense();

	// Lock the database for this instance
	LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

	// Make sure the DB Schema is the expected version
	validateDBSchemaVersion();

	// Create a pointer to a recordset
	_RecordsetPtr ipActionSet(__uuidof(Recordset));
	ASSERT_RESOURCE_ALLOCATION("ELI13528", ipActionSet != NULL);

	// Open the Action table
	ipActionSet->Open("Action", _variant_t((IDispatch *)ipConnection, true), adOpenDynamic, 
		adLockOptimistic, adCmdTableDirect);

	// Begin a transaction
	TransactionGuard tg(ipConnection);

	// Setup find criteria to find the action to delete
	string strFind = "ASCName = '" + asString(strAction) + "'";

	// Search for the action to delete
	ipActionSet->Find(strFind.c_str(), 0, adSearchForward);

	// if action was found
	if (ipActionSet->adoEOF == VARIANT_FALSE)
	{
		// Get the action name from the database
		strActionName = getStringField(ipActionSet->Fields, "ASCName");

		// Delete the record 
		ipActionSet->Delete(adAffectCurrent);

		// Remove column from FAMFile
		removeActionColumn(ipConnection, strActionName);

		// Commit the change to the database
		tg.CommitTrans();
	}
	END_CONNECTION_RETRY(ipConnection, "ELI23525");
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::AddFile2(BSTR strFile,  BSTR strAction, EFilePriority ePriority,
										VARIANT_BOOL bForceStatusChange, VARIANT_BOOL bFileModified,
										EActionStatus eNewStatus, VARIANT_BOOL * pbAlreadyExists,
										EActionStatus *pPrevStatus, IFileRecord* * ppFileRecord)
{
	INIT_EXCEPTION_AND_TRACING("MLI00006");
	try
	{
		// Check License
		validateLicense();

		// Replace any occurences of ' with '' this is because SQL Server use the ' to indicate the beginning and end of a string
		string strFileName = asString(strFile);
		replaceVariable(strFileName, "'", "''");

		// Open a recordset that contain only the record (if it exists) with the given filename
		string strFileSQL = "SELECT * FROM FAMFile WHERE FileName = '" + strFileName + "'";

		// put the unaltered file name back in the strFileName variable
		strFileName = asString(strFile);

		// Create the file record to return
		UCLID_FILEPROCESSINGLib::IFileRecordPtr ipNewFileRecord(CLSID_FileRecord);
		ASSERT_RESOURCE_ALLOCATION("ELI14203", ipNewFileRecord != NULL);

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;

		BEGIN_CONNECTION_RETRY();

		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Lock the database for this instance
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		// Make sure the DB Schema is the expected version
		validateDBSchemaVersion();

		_lastCodePos = "10";

		// Create a pointer to a recordset
		_RecordsetPtr ipFileSet(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI13535", ipFileSet != NULL);

		ipFileSet->Open(strFileSQL.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenDynamic, 
			adLockOptimistic, adCmdText);

		_lastCodePos = "30";

		// Check whether the file already exists in the database
		*pbAlreadyExists = asVariantBool(ipFileSet->adoEOF == VARIANT_FALSE);

		// Initialize the id
		long nID = 0;

		// Begin a transaction
		TransactionGuard tg(ipConnection);

		// Set the action name from the parameter
		string strActionName = asString(strAction);

		// Get the action ID and update the strActionName to stored value
		long nActionID = getActionID(ipConnection, strActionName);
		_lastCodePos = "45";

		// Action Column to update
		string strActionCol = "ASC_" + strActionName;

		// Get the previous status (if there was no previous record then the previous status
		// is always unattempted
		*pPrevStatus = *pbAlreadyExists == VARIANT_TRUE ?
			asEActionStatus(getStringField(ipFileSet->Fields, strActionCol)) : kActionUnattempted;
		_lastCodePos = "48";

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
					// if there is an error this may not be a valid image file but we still want
					// to put it in the database
					nPages = 0;
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
			ASSERT_RESOURCE_ALLOCATION("ELI26872", ipFields != NULL);

			// Set the fields from the new file record
			setFieldsFromFileRecord(ipFields, ipNewFileRecord);

			// set the initial Action state to pending
			setStringField(ipFields, strActionCol, strNewStatus);

			_lastCodePos = "60";

			// Add the record
			ipFileSet->Update();

			_lastCodePos = "70";

			// get the new records ID to return
			nID = getLastTableID(ipConnection, "FAMFile");

			// update the statistics
			updateStats(ipConnection, nActionID, *pPrevStatus, eNewStatus, ipNewFileRecord, NULL);
			_lastCodePos = "90";
		}
		else
		{
			// Get the fields from the file set
			FieldsPtr ipFields = ipFileSet->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI26873", ipFields != NULL);

			// Get the file record from the fields
			UCLID_FILEPROCESSINGLib::IFileRecordPtr ipOldRecord = getFileRecordFromFields(ipFields);
			ASSERT_RESOURCE_ALLOCATION("ELI27657", ipOldRecord != NULL);

			// Set the Current file Records ID
			nID = ipOldRecord->FileID;

			_lastCodePos = "100";

			// if Force processing is set need to update the status or if the previous status for this action was unattempted
			if (bForceStatusChange == VARIANT_TRUE || *pPrevStatus == kActionUnattempted)
			{
				_lastCodePos = "100.2";

				// If the previous state is "R" it should not be changed unless the FAM that was
				// processing it has timed out.
				bool bAttemptedRevert = false;
				while (*pPrevStatus == kActionProcessing)
				{
					// If auto-revert is to be attempted, but we have not attempted it yet.
					if (m_bAutoRevertLockedFiles && !bAttemptedRevert)
					{
						revertTimedOutProcessingFAMs(ipConnection);
						
						// Requery to see if the attempt had an effect on the file in question.
						ipFileSet->Requery(adOptionUnspecified);

						// Update the action status to reflect the attempt.
						*pPrevStatus = asEActionStatus(getStringField(ipFields, strActionCol));

						// Re-test to see if the record is still marked as processing.
						bAttemptedRevert = true;
						continue;
					}

					UCLIDException ue("ELI15043", "Cannot force status from Processing.");
					ue.addDebugInfo("File", strFileName);
					ue.addDebugInfo("Action Name", strActionName);
					throw ue;
				}

				// set the fields to the new file Record
				// (only update the priority if force processing)
				setFieldsFromFileRecord(ipFields, ipNewFileRecord, asCppBool(bForceStatusChange));

				// set the Action state to the new status
				setStringField(ipFields, strActionCol, strNewStatus);

				_lastCodePos = "110";

				// Update the record
				ipFileSet->Update();

				_lastCodePos = "120";

				// If the previous status was skipped, remove the record from the skipped file table
				if (*pPrevStatus == kActionSkipped)
				{
					removeSkipFileRecord(ipConnection, nID, nActionID);
				}

				// add an Action State Transition if the previous state was not unattempted or was not the
				// same as the new status and the FAST table should be updated
				if (*pPrevStatus != kActionUnattempted && *pPrevStatus != eNewStatus
					&& m_bUpdateFASTTable)
				{
					addFileActionStateTransition(ipConnection, nID, nActionID,
						asStatusString(*pPrevStatus), strNewStatus, "", "");
				}
				_lastCodePos = "140";

				// update the statistics
				updateStats(ipConnection, nActionID, *pPrevStatus, eNewStatus, ipNewFileRecord, ipOldRecord);

				_lastCodePos = "150";
			}
			else
			{
				// Set the file size and and page count for the file record to
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

		END_CONNECTION_RETRY(ipConnection, "ELI23527");
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30338")
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB:: RemoveFile2(BSTR strFile, BSTR strAction)
{
	// Check License
	validateLicense();

	// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
	ADODB::_ConnectionPtr ipConnection = NULL;

	BEGIN_CONNECTION_RETRY();

	// Get the connection for the thread and save it locally.
	ipConnection = getDBConnection();

	// Lock the database for this instance
	LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

	// Make sure the DB Schema is the expected version
	validateDBSchemaVersion();

	// Create a pointer to a recordset
	_RecordsetPtr ipFileSet(__uuidof(Recordset));
	ASSERT_RESOURCE_ALLOCATION("ELI13537", ipFileSet != NULL);

	// Replace any occurances of ' with '' this is because SQL Server use the ' to indicate the beginning and end of a string
	string strFileName = asString(strFile);
	replaceVariable(strFileName, "'", "''");

	// Open a recordset that contain only the record (if it exists) with the given filename
	string strFileSQL = "SELECT * FROM FAMFile WHERE FileName = '" + strFileName + "'";
	ipFileSet->Open(strFileSQL.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenDynamic, 
		adLockOptimistic, adCmdText);

	// Begin a transaction
	TransactionGuard tg(ipConnection);

	// Setup action name and action id
	string strActionName = asString(strAction);
	long nActionID = getActionID(ipConnection, strActionName);

	// If file exists this should not be at end of file
	if (ipFileSet->adoEOF == VARIANT_FALSE)
	{
		// Get the fields from the file set
		FieldsPtr ipFields = ipFileSet->Fields;
		ASSERT_RESOURCE_ALLOCATION("ELI26874", ipFields != NULL);

		// Get the old Record from the fields
		UCLID_FILEPROCESSINGLib::IFileRecordPtr ipOldRecord;
		ipOldRecord = getFileRecordFromFields(ipFields);

		// Action Column to change
		string strActionCol = "ASC_" + strActionName;

		// Get the file ID
		long nFileID = ipOldRecord->FileID;

		// Get the Previous file state
		string strActionState = getStringField(ipFields, strActionCol);

		// only change the state if the current state is pending
		if (strActionState == "P")
		{
			// change state to unattempted
			setStringField(ipFields, strActionCol, "U");

			ipFileSet->Update();

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

	// Commit the changes
	tg.CommitTrans();

	END_CONNECTION_RETRY(ipConnection, "ELI23528");
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::GetFileStatus2(long nFileID,  BSTR strAction,
									VARIANT_BOOL vbAttemptRevertIfLocked, EActionStatus * pStatus)
{

	// Check License
	validateLicense();

	// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
	ADODB::_ConnectionPtr ipConnection = NULL;

	BEGIN_CONNECTION_RETRY();

	// Get the connection for the thread and save it locally.
	ipConnection = getDBConnection();

	// Lock the database for this instance
	LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

	// Make sure the DB Schema is the expected version
	validateDBSchemaVersion();

	// Create a pointer to a recordset
	_RecordsetPtr ipFileSet(__uuidof(Recordset));
	ASSERT_RESOURCE_ALLOCATION("ELI13551", ipFileSet != NULL);

	// Open Recordset that contains only the record with the given ID
	string strFileSQL = "SELECT * FROM FAMFile WHERE ID = " + asString (nFileID);
	ipFileSet->Open(strFileSQL.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
		adLockOptimistic, adCmdText);

	// Set the action name from the parameter
	string strActionName = asString(strAction);

	// Get the action ID and update the strActionName to stored value
	long nActionID = getActionID(ipConnection, strActionName);

	// Action Column to update
	string strActionCol = "ASC_" + strActionName;
	// if the file exists should not be at the end of the file
	if (ipFileSet->adoEOF == VARIANT_FALSE)
	{
		// Set return value to the current Action Status
		string strStatus = getStringField(ipFileSet->Fields, strActionCol);
		*pStatus = asEActionStatus(strStatus);

		// If the file status is processing and the caller would like to check if it is a
		// locked file from a timed-out instance, try reverting before returning the initial
		// status.
		if (m_bAutoRevertLockedFiles && *pStatus == kActionProcessing &&
			asCppBool(vbAttemptRevertIfLocked))
		{
			// Begin a transaction
			TransactionGuard tg(ipConnection);

			revertTimedOutProcessingFAMs(ipConnection);

			// Commit the changes to the database
			tg.CommitTrans();

			// Re-query to see if the status changed as a result of being auto-revereted.
			ipFileSet->Requery(adOptionUnspecified);

			// Get the updated status
			string strStatus = getStringField(ipFileSet->Fields, strActionCol);
			*pStatus = asEActionStatus(strStatus);
		}
	}
	else
	{
		// File ID did not exist
		UCLIDException ue("ELI13553", "File ID was not found.");
		ue.addDebugInfo ("File ID", nFileID);
		throw ue;
	}
	END_CONNECTION_RETRY(ipConnection, "ELI23533");
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::SearchAndModifyFileStatus2(long nWhereActionID,  EActionStatus eWhereStatus,  
														  long nToActionID, EActionStatus eToStatus,
														  BSTR bstrSkippedFromUserName, 
														  long nFromActionID, long * pnNumRecordsModified)
{
	// Check License
	validateLicense();

	// Changing an Action status to failed should only be done on an individual file bases
	if (eToStatus == kActionFailed)
	{
		UCLIDException ue ("ELI13603", "Cannot change status Failed.");
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
		return;
	}

	// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
	ADODB::_ConnectionPtr ipConnection = NULL;

	BEGIN_CONNECTION_RETRY();

	// Get the connection for the thread and save it locally.
	ipConnection = getDBConnection();

	// Lock the database for this instance
	LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

	// Make sure the DB Schema is the expected version
	validateDBSchemaVersion();

	string strToAction = getActionName(ipConnection, nToActionID);
	string strWhereAction = getActionName(ipConnection, nWhereActionID);
	string strFromActionCol;
	if (nFromActionID > 0)
	{
		strFromActionCol = "ASC_" + getActionName(ipConnection, nFromActionID);
	}

	// Action column to search
	string strWhereActionCol = "ASC_" + strWhereAction;

	// Action Column to change
	string strToActionCol = "ASC_" + strToAction;

	// Begin a transaction
	TransactionGuard tg(ipConnection);

	string strSQL = "SELECT FAMFile.ID AS FAMFileID";
	if (!strFromActionCol.empty())
	{
		strSQL += ", FAMFile." + strFromActionCol;
	}

	strSQL += " FROM FAMFile";

	string strWhere = " WHERE (FAMFile." + strWhereActionCol + " = '"
		+ asStatusString(eWhereStatus) + "'";

	// Where status is skipped, need to add inner join to skip file table
	if (eWhereStatus == kActionSkipped)
	{
		strSQL += " INNER JOIN SkippedFile ON FAMFile.ID = SkippedFile.FileID ";
		strWhere += " AND SkippedFile.ActionID = " + asString(nWhereActionID);
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
	ASSERT_RESOURCE_ALLOCATION("ELI26913", ipFileSet != NULL);

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
			strToStatus = getStringField(ipFields, strFromActionCol);
		}

		setFileActionState(ipConnection, nFileID, strToAction, strToStatus, "",
			nToActionID, false, true);

		// Update modified records count
		nRecordsModified++;

		// Move to next record
		ipFileSet->MoveNext();
	}

	// Set the return value
	*pnNumRecordsModified = nRecordsModified;

	// update the stats
	reCalculateStats(ipConnection, nToActionID);

	// Commit the changes
	tg.CommitTrans();

	END_CONNECTION_RETRY(ipConnection, "ELI23534");
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::SetStatusForAllFiles2(BSTR strAction,  EActionStatus eStatus)
{
	// Check License
	validateLicense();

	// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
	ADODB::_ConnectionPtr ipConnection = NULL;

	BEGIN_CONNECTION_RETRY();

	// Get the connection for the thread and save it locally.
	ipConnection = getDBConnection();

	// Lock the database for this instance
	LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

	// Make sure the DB Schema is the expected version
	validateDBSchemaVersion();

	if (eStatus == kActionFailed)
	{
		UCLIDException ue ("ELI13604", "Transition to Failed state is not allowed.");
		throw ue;
	}

	// Set the action name from the parameter
	string strActionName = asString(strAction);

	// Get the action ID and update the strActionName to stored value
	long nActionID = getActionID(ipConnection, strActionName);

	// Action Column to change
	string strActionCol = "ASC_" + strActionName;

	// Only want to change the status that is different from status that is being changed to
	string strWhere = " WHERE " + strActionCol + " <> '" + asStatusString(eStatus) + "'";

	// Set the from statement
	string strFrom = "FROM FAMFile " + strWhere;

	// Create the query to update the file status for all files
	string strUpdateSQL = "UPDATE FAMFile SET " + strActionCol + " = '" + asStatusString(eStatus) + "' " + strFrom;

	// Begin a transaction
	TransactionGuard tg(ipConnection);

	// Get the action ID as as string
	string strActionID = asString(nActionID);

	// Remove any records from the skipped file table that where skipped for this action
	string strDeleteSkippedSQL = "DELETE FROM [SkippedFile] WHERE ActionID = " + strActionID;
	executeCmdQuery(ipConnection, strDeleteSkippedSQL);

	// Remove any records in the LockedFile table for status changing from Processing to pending
	string strDeleteLockedFiles = "DELETE FROM [LockedFile] WHERE ActionID = " + strActionID;
	executeCmdQuery(ipConnection, strDeleteLockedFiles);

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
	addASTransFromSelect(ipConnection, strActionName, nActionID, asStatusString(eStatus),
		"", "", strWhere, "");

	// Update the FAMFiles table
	executeCmdQuery(ipConnection, strUpdateSQL);

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

	END_CONNECTION_RETRY(ipConnection, "ELI23535");
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::GetFilesToProcess2(BSTR strAction,  long nMaxFiles, 
												  VARIANT_BOOL bGetSkippedFiles,
												  BSTR bstrSkippedForUserName,
												  IIUnknownVector * * pvecFileRecords)
{
	static const string strActionIDPlaceHolder = "<ActionIDPlaceHolder>";

	// Check License
	validateLicense();

	// Set the action name from the parameter
	string strActionName = asString(strAction);

	string strUPIID = asString(m_nUPIID);

	// Action Column to change
	string strActionCol = "ASC_" + strActionName;

	string strWhere = "";
	string strTop = "TOP (" + asString(nMaxFiles) + ") ";
	if (bGetSkippedFiles == VARIANT_TRUE)
	{
		strWhere = " INNER JOIN SkippedFile ON FAMFile.ID = SkippedFile.FileID "
			"WHERE (SkippedFile.ActionID = " + strActionIDPlaceHolder
			+ " AND FAMFile." + strActionCol + " = 'S'";

		string strUserName = asString(bstrSkippedForUserName);
		if(!strUserName.empty())
		{
			replaceVariable(strUserName, "'", "''");
			string strUserAnd = " AND SkippedFile.UserName = '" + strUserName + "'";
			strWhere += strUserAnd;
		}

		// Only get files that have not been skipped by the current process
		strWhere += " AND SkippedFile.UPIID <> " + strUPIID;

		strWhere += ")";
	}
	else
	{
		strWhere = "WHERE (" + strActionCol + " = 'P')";
	}

	// Order by priority [LRCAU #5438]
	strWhere += " ORDER BY [FAMFile].[Priority] DESC, [FAMFile].[ID] ASC ";

	// Build the from clause
	string strFrom = "FROM FAMFile " + strWhere;

	// create query to select top records;
	string strSelectSQL = "SELECT " + strTop
		+ " FAMFile.ID, FileName, Pages, FileSize, Priority, " + strActionCol + " " + strFrom;

	// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
	ADODB::_ConnectionPtr ipConnection = NULL;

	BEGIN_CONNECTION_RETRY();

	// Get the connection for the thread and save it locally.
	ipConnection = getDBConnection();

	// Lock the database for this instance
	LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

	// Make sure the DB Schema is the expected version
	validateDBSchemaVersion();

	// Get the action ID 
	long nActionID = getActionID(ipConnection, strActionName);

	// Update the select statement with the action ID
	replaceVariable(strSelectSQL, strActionIDPlaceHolder, asString(nActionID));

	// return the vector of file records
	IIUnknownVectorPtr ipFiles = setFilesToProcessing(ipConnection, strSelectSQL, nActionID);
	*pvecFileRecords = ipFiles.Detach();

	END_CONNECTION_RETRY(ipConnection, "ELI23537");
}
//-------------------------------------------------------------------------------------------------

void CFileProcessingDB::RemoveFolder2(BSTR strFolder, BSTR strAction)
{
	// Check License
	validateLicense();

	// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
	ADODB::_ConnectionPtr ipConnection = NULL;

	BEGIN_CONNECTION_RETRY();

	// Get the connection for the thread and save it locally.
	ipConnection = getDBConnection();		

	// Lock the database for this instance
	LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

	// Make sure the DB Schema is the expected version
	validateDBSchemaVersion();

	// Set the action name from the parameter
	string strActionName = asString(strAction);

	// Get the action ID and update the strActionName to stored value
	long nActionID = getActionID(ipConnection, strActionName);

	// Action Column to change
	string strActionCol = "ASC_" + strActionName;

	// Replace any occurences of ' with '' this is because SQL Server use the ' to indicate the beginning and end of a string
	string strFolderName = asString(strFolder);
	replaceVariable(strFolderName, "'", "''");

	// set up the where clause to find the pending records that the filename begins with the folder name
	string strWhere = "WHERE (" + strActionCol + " = 'P') AND (FileName LIKE '" + strFolderName + "%')";
	string strFrom = "FROM FAMFile " + strWhere;

	// Set up the SQL to update the FAMFile
	string strUpdateSQL = "UPDATE FAMFile SET " + strActionCol + " = 'U' " + strFrom;

	// Begin a transaction
	TransactionGuard tg(ipConnection);

	// add transition records to the databse
	addASTransFromSelect(ipConnection, strActionName, nActionID, "U", "", "", strWhere, "");

	// Only update the QueueEvent table if update is enabled
	if (m_bUpdateQueueEventTable)
	{
		// Set up the SQL to add the queue event records
		string strInsertQueueRecords = "INSERT INTO QueueEvent (FileID, DateTimeStamp, QueueEventCode, FAMUserID, MachineID) ";

		// Add the Select query to get the records to insert 
		strInsertQueueRecords += "SELECT ID, GETDATE(), 'F', "
			+ asString(getFAMUserID(ipConnection)) + ", " + asString(getMachineID(ipConnection)) + " " + strFrom;

		// Add the QueueEvent records to the database
		executeCmdQuery(ipConnection, strInsertQueueRecords);
	}

	// Update the status in the FAMFile table
	executeCmdQuery(ipConnection, strUpdateSQL);

	// Commit the changes to the database
	tg.CommitTrans();

	END_CONNECTION_RETRY(ipConnection, "ELI23538");
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::RenameAction2(long nActionID, BSTR strNewActionName)
{
	validateLicense();

	// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
	ADODB::_ConnectionPtr ipConnection = NULL;

	BEGIN_CONNECTION_RETRY();

	// Get the connection for the thread and save it locally.
	ipConnection = getDBConnection();

	// Lock the database for this instance
	LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

	// Make sure the DB Schema is the expected version
	validateDBSchemaVersion();

	// Convert action names to string
	string strOld = getActionName(ipConnection, nActionID);
	string strNew = asString(strNewActionName);

	TransactionGuard tg(ipConnection);

	// Add a new column to the FMPFile table
	addActionColumn(ipConnection, strNew);

	// Copy status from the old column without transition records (and without update skipped table)
	copyActionStatus(ipConnection, strOld, strNew, false);

	// Change the name of the action in the action table
	string strSQL = "UPDATE Action SET ASCName = '" + strNew + "' WHERE ID = " + asString(nActionID);
	executeCmdQuery(ipConnection, strSQL);

	// Remove the old action column from FMPFile table
	removeActionColumn(ipConnection, strOld);

	// Commit the transaction
	tg.CommitTrans();

	END_CONNECTION_RETRY(ipConnection, "ELI23541");
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::ModifyActionStatusForQuery2(BSTR bstrQueryFrom, BSTR bstrToAction,
														   EActionStatus eaStatus, BSTR bstrFromAction,
														   IRandomMathCondition* pRandomCondition,
														   long* pnNumRecordsModified)
{
	// Check that an action name and a FROM clause have been passed in
	string strQueryFrom = asString(bstrQueryFrom);
	ASSERT_ARGUMENT("ELI27037", !strQueryFrom.empty());
	string strToAction = asString(bstrToAction);
	ASSERT_ARGUMENT("ELI27038", !strToAction.empty());

	validateLicense();

	// Determine the source of the new status
	string strFromAction = asString(bstrFromAction);
	bool bFromSpecified = !strFromAction.empty();
	string strStatus = "";
	if (bFromSpecified)
	{
		strFromAction = "ASC_" + strFromAction;
	}
	else
	{
		// Get the new status as a string
		strStatus = asStatusString(eaStatus);
	}

	// Wrap the random condition (if there is one, in a smart pointer)
	UCLID_FILEPROCESSINGLib::IRandomMathConditionPtr ipRandomCondition(pRandomCondition);

	// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
	ADODB::_ConnectionPtr ipConnection = NULL;

	BEGIN_CONNECTION_RETRY();

	// Get the connection for the thread and save it locally.
	ipConnection = getDBConnection();

	// Lock the database
	LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

	validateDBSchemaVersion();

	// Begin a transaction
	TransactionGuard tg(ipConnection);

	_RecordsetPtr ipFileSet(__uuidof(Recordset));
	ASSERT_RESOURCE_ALLOCATION("ELI27039", ipFileSet != NULL);

	// Open the file set
	ipFileSet->Open(strQueryFrom.c_str(), _variant_t((IDispatch*)ipConnection, true),
		adOpenForwardOnly, adLockReadOnly, adCmdText);

	// Get the list of file ID's to modify
	long nNumRecordsModified = 0;
	vector<long> vecFileIds;
	while (ipFileSet->adoEOF == VARIANT_FALSE)
	{
		if (ipRandomCondition == NULL || ipRandomCondition->CheckCondition("", 0, 0) == VARIANT_TRUE)
		{
			// Get the file ID
			vecFileIds.push_back(getLongField(ipFileSet->Fields, "ID"));

			nNumRecordsModified++;
		}

		// Move to next record
		ipFileSet->MoveNext();
	}
	ipFileSet->Close();

	// The action column to change
	string strToActionCol = "ASC_" + strToAction;

	// Loop through the file Ids to change in groups of 10000 populating the SetFileActionData
	map<string, vector<SetFileActionData>> mapFromStatusToId;
	size_t count = vecFileIds.size();
	size_t i = 0;
	while (i < count)
	{
		string strQuery = "SELECT * FROM FAMFile WHERE FAMFile.ID IN (";
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
			ASSERT_RESOURCE_ALLOCATION("ELI27040", ipFields != NULL);

			long nFileID = getLongField(ipFields, "ID");
			EActionStatus fromStatus = asEActionStatus(getStringField(ipFields, strToActionCol));

			// If copying from an action, get the status for the action
			if (bFromSpecified)
			{
				strStatus = getStringField(ipFields, strFromAction);
			}

			mapFromStatusToId[strStatus].push_back(SetFileActionData(nFileID,
				getFileRecordFromFields(ipFields, false), fromStatus));

			ipFileSet->MoveNext();
		}
		ipFileSet->Close();
	}

	// Set the file action state for each vector of file data
	for(map<string, vector<SetFileActionData>>::iterator it = mapFromStatusToId.begin();
		it != mapFromStatusToId.end(); it++)
	{
		setFileActionState(ipConnection, it->second, strToAction, it->first); 
	}

	// Commit the transaction
	tg.CommitTrans();

	// Set the return value if it is specified
	if (pnNumRecordsModified != NULL)
	{
		*pnNumRecordsModified = nNumRecordsModified;
	}

	END_CONNECTION_RETRY(ipConnection, "ELI27041");
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::SetStatusForFilesWithTags2(IVariantVector *pvecTagNames,
														  VARIANT_BOOL vbAndOperation,
														  long nToActionID,
														  EActionStatus eaNewStatus,
														  long nFromActionID)
{
	validateLicense();

	IVariantVectorPtr ipVecTagNames(pvecTagNames);
	ASSERT_ARGUMENT("ELI27427", ipVecTagNames != NULL);

	long lSize = ipVecTagNames->Size;

	// If no tags specified do nothing
	if (lSize == 0)
	{
		return;
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
	ADODB::_ConnectionPtr ipConnection = NULL;

	BEGIN_CONNECTION_RETRY();

	// Get the connection for the thread and save it locally.
	ipConnection = getDBConnection();

	// Lock the database
	LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

	bool bFromAction = nFromActionID != -1;
	string strStatus = "";
	string strFromAction = "";
	if (bFromAction)
	{
		strFromAction = "ASC_" + getActionName(ipConnection, nFromActionID);
		replaceVariable(strQuery, gstrTAG_QUERY_SELECT,
			"[FAMFile].[ID], [FAMFile]." + strFromAction);
	}
	else
	{
		replaceVariable(strQuery, gstrTAG_QUERY_SELECT, "[FAMFile].[ID]");

		// Get the new status as a string
		strStatus = asStatusString(eaNewStatus);
	}

	// Get the action name 
	string strToAction = getActionName(ipConnection, nToActionID);

	// Set the transaction guard
	TransactionGuard tg(ipConnection);

	_RecordsetPtr ipFileSet(__uuidof(Recordset));
	ASSERT_RESOURCE_ALLOCATION("ELI27428", ipFileSet != NULL);

	// Open the file set
	ipFileSet->Open(strQuery.c_str(), _variant_t((IDispatch*)ipConnection, true),
		adOpenForwardOnly, adLockReadOnly, adCmdText);

	// Loop through each record
	while (ipFileSet->adoEOF == VARIANT_FALSE)
	{
		FieldsPtr ipFields = ipFileSet->Fields;
		ASSERT_RESOURCE_ALLOCATION("ELI27429", ipFields != NULL);

		// Get the file ID
		long nFileID = getLongField(ipFields, "ID");

		// If copying from an action, get the status for the action
		if (bFromAction)
		{
			strStatus = getStringField(ipFields, strFromAction);
		}

		// Set the file action state
		setFileActionState(ipConnection, nFileID, strToAction, strStatus, "", nToActionID, 
			false, true);

		// Move to next record
		ipFileSet->MoveNext();
	}

	// Commit the transaction
	tg.CommitTrans();

	END_CONNECTION_RETRY(ipConnection, "ELI27430");
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::SetFileStatusToProcessing2(long nFileId, long nActionID)
{
	validateLicense();

	// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
	ADODB::_ConnectionPtr ipConnection = NULL;

	BEGIN_CONNECTION_RETRY();

	// Get the connection for the thread and save it locally.
	ipConnection = getDBConnection();

	// Lock the database for this instance
	LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

	// Make sure the DB Schema is the expected version
	validateDBSchemaVersion();

	// Action column to update
	string strActionCol = "ASC_" + getActionName(ipConnection, nActionID);

	string strSelectSQL = "SELECT ID, FileName, Pages, FileSize, Priority, " +
		strActionCol + " FROM FAMFile WHERE ID = " + asString(nFileId);

	// Perform all processing related to setting a file as processing.
	setFilesToProcessing(ipConnection, strSelectSQL, nActionID);

	END_CONNECTION_RETRY(ipConnection, "ELI29619");
}
//-------------------------------------------------------------------------------------------------
// Private
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::setFileActionState2(_ConnectionPtr ipConnection,
										   const vector<SetFileActionData>& vecSetData,
										   string strAction, const string& strState)
{
	// Get the ActionID
	long nActionID = getActionID(ipConnection, strAction);
	string strActionID = asString(nActionID);
	string strActionCol = "ASC_" + strAction;
	string strFAMUser = asString(getFAMUserID(ipConnection));
	string strMachine = asString(getMachineID(ipConnection));
	EActionStatus eaTo = asEActionStatus(strState);

	// Get the set of all skipped files for the specified action
	set<long> setSkippedIds = getSkippedFilesForAction(ipConnection, nActionID);

	// Build main queries
	string strUpdateFamFile = "Update FAMFile Set " + strActionCol + " = '" + 
		strState + "' WHERE ID IN (";
	string strDeleteLockedFile = "DELETE FROM LockedFile WHERE ActionID = "
		+ strActionID + " AND UPIID = " + asString(m_nUPIID)
		+ " AND FileID IN (";
	string strRemoveSkippedFile = "DELETE FROM SkippedFile WHERE ActionID = "
		+ strActionID + " AND FileID IN (";
	string strFastQuery = "INSERT INTO " + gstrFILE_ACTION_STATE_TRANSITION
		+ " (FileID, ActionID, ASC_From, ASC_To, DateTimeStamp, FAMUserID, MachineID"
		+ ") SELECT FAMFile.ID, " + strActionID + " AS ActionID, FAMFile."
		+ strActionCol + " AS ASC_From, '" + strState + "' AS ASC_To, "
		+ "GETDATE() AS DateTimeStamp, " + strFAMUser + " AS FAMUserID, " + strMachine
		+ " AS MachineID FROM FAMFile WHERE FAMFile.ID IN (";
	string strClearComments = m_bAutoDeleteFileActionComment && strState == "C" ?
		"DELETE FROM FileActionComment WHERE ActionID = " + strActionID + " AND FileID IN("
		: "";
	string strAddSkipRecord = strState == "S" ?
		"INSERT INTO SkippedFile (UserName, FileID, ActionID) SELECT '"
		+ getCurrentUserName() + "' AS UserName, FAMFile.ID, "
		+ strActionID + " AS ActionID FROM FAMFile WHERE FAMFile.ID IN (" : "";

	// Reload the action statistics from the database
	loadStats(ipConnection, nActionID);

	// Execute the queries in groups of 10000 File IDs
	size_t i=0;
	size_t count = vecSetData.size();
	while (i < count)
	{
		string strFileIdList;
		for(int j=0; i < count && j < 10000; j++)
		{
			const SetFileActionData& data = vecSetData[i++];
			if (!strFileIdList.empty())
			{
				strFileIdList += ", ";
			}
			strFileIdList += asString(data.FileID);

			// Update the stats
			// If the current status for the file is processing but it is listed
			// in the skipped table, then set the from action status to skipped so
			// that skipped file counts are correctly computed
			// do not push to the database yet
			updateStats(ipConnection, nActionID,
				data.FromStatus == kActionProcessing
				&& setSkippedIds.find(data.FileID) != 
					setSkippedIds.end() ? kActionSkipped : data.FromStatus,
				 eaTo, data.FileRecord, data.FileRecord, false);
		}
		strFileIdList += ")";

		// Execute the queries (execute the FAMFile update last)
		executeCmdQuery(ipConnection, strFastQuery + strFileIdList);
		executeCmdQuery(ipConnection, strDeleteLockedFile + strFileIdList);
		executeCmdQuery(ipConnection, strRemoveSkippedFile + strFileIdList);
		if (!strClearComments.empty())
		{
			executeCmdQuery(ipConnection, strClearComments + strFileIdList);
		}
		if (!strAddSkipRecord.empty())
		{
			executeCmdQuery(ipConnection, strAddSkipRecord + strFileIdList);
		}
		executeCmdQuery(ipConnection, strUpdateFamFile + strFileIdList);
	}

	// Done setting all file states and updating statistics, push to the database now
	saveStats(ipConnection, nActionID);
}
//--------------------------------------------------------------------------------------------------
EActionStatus CFileProcessingDB::setFileActionState2(_ConnectionPtr ipConnection, long nFileID, 
													string strAction, const string& strState,
													const string& strException,
													long nActionID, bool bLockDB,
													bool bRemovePreviousSkipped,
													const string& strFASTComment)
{
	INIT_EXCEPTION_AND_TRACING("MLI03270");

	auto_ptr<LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr>> apDBlg;
	auto_ptr<TransactionGuard> apTG;
	try
	{
		ASSERT_ARGUMENT("ELI26796", ipConnection != NULL);
		ASSERT_ARGUMENT("ELI26795", !strAction.empty() || nActionID != -1);

		_lastCodePos = "10";
		EActionStatus easRtn = kActionUnattempted;

		if (bLockDB)
		{
			_lastCodePos = "20";
			// Lock the database for this instance
			apDBlg.reset(
				new LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr>(getThisAsCOMPtr()));
		}

		// Update action ID/Action name
		if (!strAction.empty() && nActionID == -1)
		{	
			_lastCodePos = "30";
			nActionID = getActionID(ipConnection, strAction);
		}
		else if (strAction.empty() && nActionID != -1)
		{
			_lastCodePos = "40";
			strAction = getActionName(ipConnection, nActionID);
		}
		_lastCodePos = "50";

		// Action Column to update
		string strActionCol = "ASC_" + strAction;

		// Set up the select query to select the file to change and include and skipped file data
		// If there is no skipped file record the SkippedActionID will be -1
		string strFileSQL = "SELECT FAMFile.ID as ID, FileName, FileSize, Pages, Priority, " + 
			strActionCol + ", COALESCE(SkippedFile.ActionID, -1) AS SkippedActionID " +
			"FROM SkippedFile RIGHT OUTER JOIN FAMFile ON SkippedFile.FileID = FAMFile.ID AND " +
			"SkippedFile.ActionID = " + asString(nActionID) + " WHERE FAMFile.ID = " + asString (nFileID);
		
		_lastCodePos = "60";

		// Make sure the DB Schema is the expected version
		validateDBSchemaVersion();
		_lastCodePos = "70";

		_RecordsetPtr ipFileSet(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI13542", ipFileSet != NULL);
		_lastCodePos = "80";

		ipFileSet->Open(strFileSQL.c_str(), _variant_t((IDispatch *)ipConnection, true), 
			adOpenStatic, adLockReadOnly, adCmdText);
		
		_lastCodePos = "90";
		if (bLockDB)
		{
			_lastCodePos = "100";
			// Begin a transaction
			apTG.reset(new TransactionGuard(ipConnection));
		}
		_lastCodePos = "110";

		// Find the file if it exists
		if (ipFileSet->adoEOF == VARIANT_FALSE)
		{
			_lastCodePos = "120";
			FieldsPtr ipFileSetFields = ipFileSet->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI26867", ipFileSetFields != NULL);

			_lastCodePos = "130";
			// Get the previous state
			string strPrevStatus = getStringField(ipFileSetFields, strActionCol); 
			_lastCodePos = "140";

			easRtn = asEActionStatus (strPrevStatus);
			_lastCodePos = "150";

			// Get the current record
			UCLID_FILEPROCESSINGLib::IFileRecordPtr ipCurrRecord;
			ipCurrRecord = getFileRecordFromFields(ipFileSetFields);
			_lastCodePos = "160";

			// Get the skipped ActionID
			long nSkippedActionID = getLongField(ipFileSetFields, "SkippedActionID");
			_lastCodePos = "170";

			// Update the state of the file in the FAMFile table
			executeCmdQuery(ipConnection, "Update FAMFile Set " + strActionCol + " = '" + 
				strState + "' WHERE ID = " + asString(nFileID));
			_lastCodePos = "180";

			// If transition to complete and AutoDeleteFileActionComment == true
			// then clear the file action comment for this file
			if (strState == "C" && m_bAutoDeleteFileActionComment)
			{
				_lastCodePos = "190";
				clearFileActionComment(ipConnection, nFileID, nActionID);
			}
			_lastCodePos = "200";

			// if the old status does not equal the new status add transition records
			if (strPrevStatus != strState || bRemovePreviousSkipped)
			{
				_lastCodePos = "210";
				// update the statistics
				EActionStatus easStatsFrom = easRtn;
				if (easRtn == kActionProcessing)
				{
					_lastCodePos = "220";
					// If there is a record in the skipped table, call update stats with
					// Skipped as the previous state
					if (nSkippedActionID != -1)
					{
						_lastCodePos = "230";
						easStatsFrom = kActionSkipped;
					}
					_lastCodePos = "240";

					// Remove record from the LockedFileTable
					executeCmdQuery(ipConnection, "DELETE FROM LockedFile WHERE FileID = " + 
						asString(nFileID) + " AND ActionID = " + asString(nActionID) + 
						" AND UPIID = " + asString(m_nUPIID));
				}
				_lastCodePos = "250";
				updateStats(ipConnection, nActionID, easStatsFrom, asEActionStatus(strState),
					ipCurrRecord, ipCurrRecord);

				_lastCodePos = "260";
				// Only update FileActionStateTransition table if required
				if (m_bUpdateFASTTable)
				{
					_lastCodePos = "270";
					addFileActionStateTransition(ipConnection, nFileID, nActionID, strPrevStatus, 
						strState, strException, strFASTComment);
				}
				_lastCodePos = "280";

				// Determine if existing skipped record should be removed
				bool bSkippedRemoved = nSkippedActionID != -1 && (bRemovePreviousSkipped || strState != "S");

				// These calls are order dependent.
				// Remove the skipped record (if any) and add a new
				// skipped file record if the new state is skipped
				if (bSkippedRemoved)
				{
					_lastCodePos = "290";
					removeSkipFileRecord(ipConnection, nFileID, nActionID);
				}
				_lastCodePos = "300";

				if (strState == "S")
				{
					if (nSkippedActionID == -1 || bSkippedRemoved)
					{
						_lastCodePos = "310";

						// Add a record to the skipped table
						addSkipFileRecord(ipConnection, nFileID, nActionID);
					}
					else 
					{
						_lastCodePos = "320";
						// Update the UPIID to current process so it will be not be selected
						// again as a skipped file for the current process
						// Also update the time stamp and the UserName (since the user
						// could be processing all files skipped by any user)
						// [LRCAU #5853]
						executeCmdQuery(ipConnection, "Update SkippedFile Set UPIID = " + 
							asString(m_nUPIID) + ", DateTimeStamp = GETDATE(), UserName = '"
							+ getCurrentUserName() + "' WHERE FileID = " + asString(nFileID));
					}
				}
			}
			_lastCodePos = "330";

			// If there is a transaction guard then commit the transaction
			if (apTG.get() != NULL)
			{
				_lastCodePos = "340";
				apTG->CommitTrans();
			}
			_lastCodePos = "350";
		}
		else
		{
			_lastCodePos = "360";

			// No file with the given id
			UCLIDException ue("ELI13543", "File ID was not found.");
			ue.addDebugInfo ("File ID", nFileID);
			throw ue;
		}
		_lastCodePos = "370";

		return easRtn;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26912");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::addASTransFromSelect2(_ConnectionPtr ipConnection,
											  const string &strAction, long nActionID,
											  const string &strToState, const string &strException,
											  const string &strComment, const string &strWhereClause, 
											  const string &strTopClause)
{
	if (!m_bUpdateFASTTable)
	{
		return;
	}

	// Action Column to change
	string strActionCol = "ASC_" + strAction;

	// Create the from string
	string strFrom = " FROM FAMFile " + strWhereClause;

	// if the strException string is empty NULL should be added to the db
	string strNewException = (strException.empty()) ? "NULL": "'" + strException + "'";

	// if the strComment is empty the NULL should be added to the database
	string strNewComment = (strComment.empty()) ? "NULL": "'" + strComment + "'";

	// create the insert string
	string strInsertTrans = "INSERT INTO FileActionStateTransition (FileID, ActionID, ASC_From, "
		"ASC_To, DateTimeStamp, Exception, Comment, FAMUserID, MachineID) ";
	strInsertTrans += "SELECT " + strTopClause + " FAMFile.ID, " + 
		asString(nActionID) + " as ActionID, " + 
		strActionCol + " as ASC_From, '" + 
		strToState + 
		"' as ASC_To, GetDate() as DateTimeStamp, " + 
		strNewException + ", " +
		strNewComment + ", " +
		asString(getFAMUserID(ipConnection)) + ", " +
		asString(getMachineID(ipConnection)) + " " + 		
		strFrom;

	// Insert the records
	executeCmdQuery(ipConnection, strInsertTrans);
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::reCalculateStats2(_ConnectionPtr ipConnection, long nActionID)
{
	// Get the name of the action for the ID
	string strActionName = getActionName(ipConnection, nActionID);	

	// Set up string the column name in FAMFiles
	string strActionColName = "ASC_" + strActionName;

	// Setup SQL string to calculate totals for each of the Action Types
	string strCalcSQL = "SELECT COUNT(ID) AS NumDocs, " + strActionColName  + 
		", SUM(FileSize) AS SumOfFileSize, SUM(Pages) AS SumOfPages FROM FAMFile " +
		" WHERE (NOT (" + strActionColName + "  = 'U')) " +
		"GROUP BY " + strActionColName;

	// Create a pointer to a recordset
	_RecordsetPtr ipCalcStatsSet(__uuidof(Recordset));
	ASSERT_RESOURCE_ALLOCATION("ELI14048", ipCalcStatsSet != NULL);

	// Open the Calc set table in the database
	ipCalcStatsSet->Open(strCalcSQL.c_str(), _variant_t((IDispatch *)ipConnection, true), 
		adOpenDynamic, adLockOptimistic, adCmdText);

	// Create a pointer to a recordset
	_RecordsetPtr ipActionStats(__uuidof(Recordset));
	ASSERT_RESOURCE_ALLOCATION("ELI14049", ipActionStats != NULL);

	// Select the existing Statistics record if it exists
	string strSelectStat = "SELECT * FROM ActionStatistics WHERE ActionID = " + asString(nActionID);

	// Open the recordse to for the statisics with the record for ActionID if it exists
	ipActionStats->Open(strSelectStat.c_str(), _variant_t((IDispatch *)ipConnection, true), 
		adOpenDynamic, adLockOptimistic, adCmdText);

	FieldsPtr ipActionFields = NULL;

	// If no records in the data set then will need create a new record
	if (ipActionStats->adoEOF == VARIANT_TRUE)
	{
		// Create new record
		ipActionStats->AddNew();

		// Get the fields from the new record
		ipActionFields = ipActionStats->Fields;
		ASSERT_RESOURCE_ALLOCATION("ELI26865", ipActionFields != NULL);

		//  Set Action ID
		setLongField(ipActionFields, "ActionID", nActionID);
	}
	else
	{
		// Get the action fields
		ipActionFields = ipActionStats->Fields;
		ASSERT_RESOURCE_ALLOCATION("ELI26866", ipActionFields != NULL);
	}

	// Initialize totals
	long lTotalDocs = 0;
	long lTotalPages = 0;
	long long llTotalBytes = 0;

	long lNumDocsFailed = 0;
	long lNumPagesFailed = 0;
	long long llNumBytesFailed = 0;

	long lNumDocsCompleted = 0;
	long lNumPagesCompleted = 0;
	long long llNumBytesCompleted = 0;

	long lNumDocsSkipped = 0;
	long lNumPagesSkipped = 0;
	long long llNumBytesSkipped = 0;

	// Go thru each of the records in the Calculation set
	while (ipCalcStatsSet->adoEOF == VARIANT_FALSE)
	{
		// Get the fields from the calc stat set
		FieldsPtr ipCalcFields = ipCalcStatsSet->Fields;
		ASSERT_RESOURCE_ALLOCATION("ELI26864", ipCalcFields != NULL);

		// Get the action state
		string strActionState = getStringField(ipCalcFields, strActionColName); 

		if (strActionState != "U")
		{
			long lNumDocs = getLongField(ipCalcFields, "NumDocs");
			long lNumPages = getLongField(ipCalcFields, "SumOfPages");
			long long llNumBytes = getLongLongField(ipCalcFields, "SumOfFileSize");

			// Set the sums to the appropriate statistics property
			if (strActionState == "F")
			{
				// Set Failed totals
				lNumDocsFailed = lNumDocs;
				lNumPagesFailed = lNumPages;
				llNumBytesFailed = llNumBytes;
			}
			else if (strActionState == "C")
			{
				// Set Completed totals
				lNumDocsCompleted = lNumDocs;
				lNumPagesCompleted = lNumPages;
				llNumBytesCompleted = llNumBytes;
			}
			else if (strActionState == "S")
			{
				// Set Skipped totals
				lNumDocsSkipped = lNumDocs;
				lNumPagesSkipped = lNumPages;
				llNumBytesSkipped = llNumBytes;
			}

			// All values are added to the Totals
			lTotalDocs += lNumDocs;
			lTotalPages += lNumPages;
			llTotalBytes += llNumBytes;
		}

		// Move to next record
		ipCalcStatsSet->MoveNext();
	}

	// Set Failed totals
	setLongField(ipActionFields, "NumDocumentsFailed", lNumDocsFailed);
	setLongField(ipActionFields, "NumPagesFailed", lNumPagesFailed);
	setLongLongField(ipActionFields, "NumBytesFailed", llNumBytesFailed);

	// Set Completed totals
	setLongField(ipActionFields, "NumDocumentsComplete", lNumDocsCompleted);
	setLongField(ipActionFields, "NumPagesComplete", lNumPagesCompleted);
	setLongLongField(ipActionFields, "NumBytesComplete", llNumBytesCompleted);

	// Set Skipped totals
	setLongField(ipActionFields, "NumDocumentsSkipped", lNumDocsSkipped);
	setLongField(ipActionFields, "NumPagesSkipped", lNumPagesSkipped);
	setLongLongField(ipActionFields, "NumBytesSkipped", llNumBytesSkipped);

	// Save totals in the ActionStatistics table
	setLongField(ipActionFields, "NumDocuments", lTotalDocs);
	setLongField(ipActionFields, "NumPages", lTotalPages);
	setLongLongField(ipActionFields, "NumBytes", llTotalBytes);

	// Update the record
	ipActionStats->Update();
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::addTables2(bool bAddUserTables)
{
	vector<string> vecQueries;

	// Add the user tables if necessary
	if (bAddUserTables)
	{
		vecQueries.push_back(gstrCREATE_DB_INFO_TABLE);
		vecQueries.push_back(gstrCREATE_FAM_TAG_TABLE);
		vecQueries.push_back(gstrCREATE_USER_CREATED_COUNTER_TABLE);
		vecQueries.push_back(gstrCREATE_USER_CREATED_COUNTER_VALUE_INDEX);
	}

	// Add queries to create tables to the vector
	vecQueries.push_back(gstrCREATE_ACTION_TABLE);
	vecQueries.push_back(gstrCREATE_LOCK_TABLE);
	vecQueries.push_back(gstrCREATE_ACTION_STATE_TABLE);
	vecQueries.push_back(gstrCREATE_FAM_FILE_TABLE);
	vecQueries.push_back(gstrCREATE_FAM_FILE_ID_PRIORITY_INDEX);
	vecQueries.push_back(gstrCREATE_FAM_FILE_INDEX);
	vecQueries.push_back(gstrCREATE_QUEUE_EVENT_CODE_TABLE);
	vecQueries.push_back(gstrCREATE_ACTION_STATISTICS_TABLE);
	vecQueries.push_back(gstrCREATE_FILE_ACTION_STATE_TRANSITION_TABLE);
	vecQueries.push_back(gstrCREATE_QUEUE_EVENT_TABLE);
	vecQueries.push_back(gstrCREATE_QUEUE_EVENT_INDEX);
	vecQueries.push_back(gstrCREATE_MACHINE_TABLE);
	vecQueries.push_back(gstrCREATE_FAM_USER_TABLE);
	vecQueries.push_back(gstrCREATE_FAM_FILE_ACTION_COMMENT_TABLE);
	vecQueries.push_back(gstrCREATE_FILE_ACTION_COMMENT_INDEX);
	vecQueries.push_back(gstrCREATE_FAM_SKIPPED_FILE_TABLE);
	vecQueries.push_back(gstrCREATE_SKIPPED_FILE_INDEX);
	vecQueries.push_back(gstrCREATE_SKIPPED_FILE_UPI_INDEX);
	vecQueries.push_back(gstrCREATE_FAM_FILE_TAG_TABLE);
	vecQueries.push_back(gstrCREATE_FILE_TAG_INDEX);
	vecQueries.push_back(gstrCREATE_PROCESSING_FAM_TABLE);
	vecQueries.push_back(gstrCREATE_PROCESSING_FAM_UPI_INDEX);
	vecQueries.push_back(gstrCREATE_LOCKED_FILE_TABLE);
	vecQueries.push_back(gstrCREATE_FPS_FILE_TABLE);
	vecQueries.push_back(gstrCREATE_FPS_FILE_NAME_INDEX);
	vecQueries.push_back(gstrCREATE_FAM_SESSION);
	vecQueries.push_back(gstrCREATE_INPUT_EVENT);
	vecQueries.push_back(gstrCREATE_INPUT_EVENT_INDEX);

	// Only create the login table if it does not already exist
	if (!doesTableExist(getDBConnection(), "Login"))
	{
		vecQueries.push_back(gstrCREATE_LOGIN_TABLE);
	}

	// Add Foreign keys 
	vecQueries.push_back(gstrADD_STATISTICS_ACTION_FK);
	vecQueries.push_back(gstrADD_FILE_ACTION_STATE_TRANSITION_ACTION_FK);
	vecQueries.push_back(gstrADD_FILE_ACTION_STATE_TRANSITION_FAM_FILE_FK);
	vecQueries.push_back(gstrADD_QUEUE_EVENT_FAM_FILE_FK);
	vecQueries.push_back(gstrADD_QUEUE_EVENT_QUEUE_EVENT_CODE_FK);
	vecQueries.push_back(gstrADD_FILE_ACTION_STATE_TRANSITION_MACHINE_FK);
	vecQueries.push_back(gstrADD_FILE_ACTION_STATE_TRANSITION_FAM_USER_FK);
	vecQueries.push_back(gstrADD_FILE_ACTION_STATE_TRANSITION_ACTION_STATE_TO_FK);
	vecQueries.push_back(gstrADD_FILE_ACTION_STATE_TRANSITION_ACTION_STATE_FROM_FK);
	vecQueries.push_back(gstrADD_QUEUE_EVENT_MACHINE_FK);
	vecQueries.push_back(gstrADD_QUEUE_EVENT_FAM_USER_FK);
	vecQueries.push_back(gstrADD_QUEUE_EVENT_ACTION_FK);
	vecQueries.push_back(gstrADD_FILE_ACTION_COMMENT_ACTION_FK);
	vecQueries.push_back(gstrADD_FILE_ACTION_COMMENT_FAM_FILE_FK);
	vecQueries.push_back(gstrADD_SKIPPED_FILE_FAM_FILE_FK);
	vecQueries.push_back(gstrADD_SKIPPED_FILE_ACTION_FK);
	vecQueries.push_back(gstrADD_FILE_TAG_FAM_FILE_FK);
	vecQueries.push_back(gstrADD_FILE_TAG_TAG_ID_FK);
	vecQueries.push_back(gstrADD_LOCKED_FILE_ACTION_FK);
	vecQueries.push_back(gstrADD_LOCKED_FILE_ACTION_STATE_FK);
	vecQueries.push_back(gstrADD_LOCKED_FILE_FAMFILE_FK);
	vecQueries.push_back(gstrADD_LOCKED_FILE_PROCESSINGFAM_FK);
	vecQueries.push_back(gstrADD_FAM_SESSION_MACHINE_FK);
	vecQueries.push_back(gstrADD_FAM_SESSION_FAMUSER_FK);
	vecQueries.push_back(gstrADD_FAM_SESSION_FPSFILE_FK);
	vecQueries.push_back(gstrADD_INPUT_EVENT_ACTION_FK);
	vecQueries.push_back(gstrADD_INPUT_EVENT_MACHINE_FK);
	vecQueries.push_back(gstrADD_INPUT_EVENT_FAMUSER_FK);

	// Execute all of the queries
	executeVectorOfSQL(getDBConnection(), vecQueries);
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::copyActionStatus2(const _ConnectionPtr& ipConnection, const string& strFrom, 
										 string strTo, bool bAddTransRecords, long nToActionID)
{
	if (bAddTransRecords)
	{
		string strToActionID = asString(nToActionID == -1 ? getActionID(ipConnection, strTo) : nToActionID);
		string strTransition = "INSERT INTO FileActionStateTransition "
			"(FileID, ActionID, ASC_From, ASC_To, DateTimeStamp, Comment, FAMUserID, MachineID) "
			"SELECT ID, " + strToActionID + " AS ActionID, ASC_" + 
			strTo + ", ASC_" + strFrom + " , GETDATE() AS TS_Trans, 'Copy status from " + 
			strFrom +" to " + strTo + "' AS Comment, " + asString(getFAMUserID(ipConnection)) + 
			", " + asString(getMachineID(ipConnection)) + " FROM FAMFile";

		executeCmdQuery(ipConnection, strTransition);
	}

	// Check if the skipped table needs to be updated
	if (nToActionID != -1)
	{
		// Get the to action ID as a string
		string strToActionID = asString(nToActionID);

		// Delete any existing skipped records (files may be leaving skipped status)
		string strDeleteSkipped = "DELETE FROM SkippedFile WHERE ActionID = " + strToActionID;

		// Need to add any new skipped records (files may be entering skipped status)
		string strAddSkipped = "INSERT INTO SkippedFile (FileID, ActionID, UserName, UPIID) SELECT "
			" FAMFile.ID, " + strToActionID + " AS NewActionID, '" + getCurrentUserName()
			+ "' AS NewUserName, " + asString(m_nUPIID) + " AS UPIID FROM FAMFile WHERE ASC_" 
			+ strFrom + " = 'S'";

		// Delete the existing skipped records for this action and insert any new ones
		executeCmdQuery(ipConnection, strDeleteSkipped);
		executeCmdQuery(ipConnection, strAddSkipped);
	}

	string strCopy = "UPDATE FAMFile SET ASC_" + strTo + " = ASC_" + strFrom;
	executeCmdQuery(ipConnection, strCopy);
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::addActionColumn2(const _ConnectionPtr& ipConnection, const string& strAction)
{

	// Add new Column to FAMFile table
	// Create SQL statement to add the column to FAMFile
	string strAddColSQL = "Alter Table FAMFile Add ASC_" + strAction + " nvarchar(1)";

	// Run the SQL to add column to FAMFile
	executeCmdQuery(ipConnection, strAddColSQL);

	// Create the query and update the file status for all files to unattempted
	string strUpdateSQL = "UPDATE FAMFile SET ASC_" + strAction + " = 'U' FROM FAMFile";
	executeCmdQuery(ipConnection, strUpdateSQL);

	// Create index on the new column
	string strCreateIDX = "Create Index IX_ASC_" + strAction + " on FAMFile (ASC_" 
		+ strAction + ")";
	executeCmdQuery(ipConnection, strCreateIDX);

	// Add foreign key contraint for the new column to reference the ActionState table
	string strAddContraint = "ALTER TABLE FAMFile WITH CHECK ADD CONSTRAINT FK_ASC_" 
		+ strAction + " FOREIGN KEY(ASC_" + 
		strAction + ") REFERENCES ActionState(Code)";

	// Create the foreign key
	executeCmdQuery(ipConnection, strAddContraint);

	// Add the default contraint for the column
	string strDefault = "ALTER TABLE FAMFile ADD CONSTRAINT DF_ASC_" 
		+ strAction + " DEFAULT 'U' FOR ASC_" + strAction;
	executeCmdQuery(ipConnection, strDefault);
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::removeActionColumn2(const _ConnectionPtr& ipConnection,
										   const string& strAction)
{
	// Remove the Foreign key relationship
	dropConstraint(ipConnection, gstrFAM_FILE, "FK_ASC_" + strAction);

	// Drop index on the action column
	string strSQL = "Drop Index IX_ASC_" + strAction + " ON FAMFile";
	executeCmdQuery(ipConnection, strSQL);

	// Remove the default contraint
	dropConstraint(ipConnection, gstrFAM_FILE, "DF_ASC_" + strAction);

	// Drop the column
	strSQL = "ALTER TABLE FAMFile DROP COLUMN ASC_" + strAction;
	executeCmdQuery(ipConnection, strSQL);
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::getExpectedTables2(std::vector<string>& vecTables)
{
	// Add Tables
	vecTables.push_back(gstrACTION);
	vecTables.push_back(gstrACTION_STATE);
	vecTables.push_back(gstrACTION_STATISTICS);
	vecTables.push_back(gstrDB_INFO);
	vecTables.push_back(gstrFAM_FILE);
	vecTables.push_back(gstrFILE_ACTION_STATE_TRANSITION);
	vecTables.push_back(gstrLOCK_TABLE);
	vecTables.push_back(gstrLOGIN);
	vecTables.push_back(gstrQUEUE_EVENT);
	vecTables.push_back(gstrQUEUE_EVENT_CODE);
	vecTables.push_back(gstrMACHINE);
	vecTables.push_back(gstrFAM_USER);
	vecTables.push_back(gstrFAM_FILE_ACTION_COMMENT);
	vecTables.push_back(gstrFAM_SKIPPED_FILE);
	vecTables.push_back(gstrFAM_FILE_TAG);
	vecTables.push_back(gstrFAM_TAG);
	vecTables.push_back(gstrPROCESSING_FAM);
	vecTables.push_back(gstrLOCKED_FILE);
	vecTables.push_back(gstrUSER_CREATED_COUNTER);
	vecTables.push_back(gstrFPS_FILE);
	vecTables.push_back(gstrFAM_SESSION);
	vecTables.push_back(gstrINPUT_EVENT);
}
//--------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CFileProcessingDB::setFilesToProcessing2(const _ConnectionPtr &ipConnection,
														   const string& strSelectSQL,
														   long nActionID)
{
	try
	{
		try
		{
			// IUnknownVector to hold the FileRecords to return
			IIUnknownVectorPtr ipFiles(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI19504", ipFiles != NULL);

			// Begin a transaction
			TransactionGuard tg(ipConnection);

			if (m_bAutoRevertLockedFiles)
			{
				revertTimedOutProcessingFAMs(ipConnection);
			}

			// Action Column to change
			string strActionName = getActionName(ipConnection, nActionID);
			string strActionCol = "ASC_" + strActionName;

			// Recordset to contain the files to process
			_RecordsetPtr ipFileSet(__uuidof(Recordset));
			ASSERT_RESOURCE_ALLOCATION("ELI13573", ipFileSet != NULL);

			// Get recordset of files to be set to processing.
			ipFileSet->Open(strSelectSQL.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenDynamic, 
				adLockPessimistic, adCmdText);

			// The state the records were in previous to being marked processing.
			string strFromState;

			// Fill the ipFiles collection, also update the FAMFile table and build
			// the queries to update both the FAST table and the Locked file table
			string strFileIDIn = "";
			while (ipFileSet->adoEOF == VARIANT_FALSE)
			{
				FieldsPtr ipFields = ipFileSet->Fields;
				ASSERT_RESOURCE_ALLOCATION("ELI28234", ipFields != NULL);

				string strFileFromState = getStringField(ipFields, strActionCol);

				// Set the record to processing and update the recordset
				setStringField(ipFields, strActionCol, "R");
				ipFileSet->Update();

				// Get the file Record from the fields
				UCLID_FILEPROCESSINGLib::IFileRecordPtr ipFileRecord =
					getFileRecordFromFields(ipFields);
				ASSERT_RESOURCE_ALLOCATION("ELI28235", ipFileRecord != NULL);

				// Put record in list of records to return
				ipFiles->PushBack(ipFileRecord);

				// Add the file ID to the list of ID's
				if (!strFileIDIn.empty())
				{
					strFileIDIn += ", ";
				}

				string strFileID = asString(ipFileRecord->FileID);

				if (strFileFromState != "P" && strFileFromState != "S")
				{
					UCLIDException ue("ELI29629", "Invalid File State Transition!");
					ue.addDebugInfo("Old Status", asStatusName(strFileFromState));
					ue.addDebugInfo("New Status", "Processing");
					ue.addDebugInfo("Action Name", strActionName);
					ue.addDebugInfo("File ID", strFileID);
					throw ue;
				}

				strFileIDIn += strFileID;

				if (strFromState.empty())
				{
					strFromState = strFileFromState;
				}
				else if (strFromState != strFileFromState)
				{
					UCLIDException ue("ELI29622", "Unable to simultaneously set a batch of records "
						"in multiple action states to processing!");
					ue.addDebugInfo("Action Name", strActionName);
					ue.addDebugInfo("File IDs", strFileIDIn);
					ue.addDebugInfo("Action State A", strFromState);
					ue.addDebugInfo("Action State B", strFileFromState);
					throw ue;
				}

				// move to the next record in the recordset
				ipFileSet->MoveNext();
			}

			// Check whether any file IDs have been added to the string
			if (!strFileIDIn.empty())
			{
				strFileIDIn += ")";

				// Get the from state for the queries
				string strActionID = asString(nActionID);
				string strUPIID = asString(m_nUPIID);

				// Update the FAST table if necessary
				if (m_bUpdateFASTTable)
				{
					// Get the machine and user ID
					string strMachineID = asString(getMachineID(ipConnection));
					string strUserID = asString(getFAMUserID(ipConnection));

					// Create query to update the FAST table
					string strFASTSql =
						"INSERT INTO " + gstrFILE_ACTION_STATE_TRANSITION + " (FileID, ActionID, "
						"ASC_From, ASC_To, DateTimeStamp, FAMUserID, MachineID, Exception, "
						"Comment) SELECT FAMFile.ID, " + strActionID + ", '" + strFromState
						+ "', 'R', GETDATE(), " + strUserID + ", " + strMachineID
						+ ", NULL, NULL FROM FAMFile WHERE FAMFile.ID IN (" + strFileIDIn;

					executeCmdQuery(ipConnection, strFASTSql);
				}

				// Create query to create records in the LockedFile table
				string strLockedTableSQL =
					"INSERT INTO LockedFile (FileID, ActionID, UPIID, StatusBeforeLock) SELECT FAMFile.ID, "
					+ strActionID + ", " + strUPIID + ", '" + strFromState + "' FROM FAMFile WHERE "
					" FAMFile.ID IN (";

				// Update the lock table
				executeCmdQuery(ipConnection, strLockedTableSQL + strFileIDIn);
			}

			// Commit the changes to the database
			tg.CommitTrans();

			return ipFiles;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29630");
	}
	catch (UCLIDException &ue)
	{
		ue.addDebugInfo("Record Query", strSelectSQL, true);
		throw ue;
	}
}