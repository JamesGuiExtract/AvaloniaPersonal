// FileProcessingDB_Internal.cpp : Implementation of CFileProcessingDB private methods

#include "stdafx.h"
#include "FileProcessingDB.h"
#include "FAMDB_SQL.h"
#include "FPCategories.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <ExtractMFCUtils.h>
#include <LicenseMgmt.h>
#include <EncryptionEngine.h>
#include <ComponentLicenseIDs.h>
#include <FAMUtilsConstants.h>
#include <ADOUtils.h>
#include <StopWatch.h>
#include <StringTokenizer.h>
#include <stringCSIS.h>

#include <string>
#include <memory>
#include <map>

using namespace std;
using namespace ADODB;

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
// Define constant for the current DB schema version
// This must be updated when the DB schema changes
const long glFAMDBSchemaVersion = 23;

// Define four UCLID passwords used for encrypting the password
// NOTE: These passwords were not exposed at the header file level because
//		 no user of this class needs to know that these passwords exist
// These passwords are also uses in the FileProcessingDB.cpp
const unsigned long	gulFAMKey1 = 0x78932517;
const unsigned long	gulFAMKey2 = 0x193E2224;
const unsigned long	gulFAMKey3 = 0x20134253;
const unsigned long	gulFAMKey4 = 0x15990323;

static const string gstrTAG_REGULAR_EXPRESSION = "^[a-zA-Z0-9_][a-zA-Z0-9\\s_]*$";

//--------------------------------------------------------------------------------------------------
// FILE-SCOPE FUNCTIONS
//--------------------------------------------------------------------------------------------------
// NOTE: This function is purposely not exposed at header file level as a class
//		 method, as no user of this class needs to know that such a function
//		 exists.
void getFAMPassword(ByteStream& rPasswordBytes)
{
	ByteStreamManipulator bsm(ByteStreamManipulator::kWrite, rPasswordBytes);
	
	bsm << gulFAMKey1;
	bsm << gulFAMKey2;
	bsm << gulFAMKey3;
	bsm << gulFAMKey4;
	bsm.flushToByteStream(8);
}
//--------------------------------------------------------------------------------------------------
// Private Methods
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::closeDBConnection()
{
	// Lock mutex to keep other instances from running code that may cause the
	// connection to be reset
	CSingleLock lg(&m_mutex, TRUE);

	// Get the current thread ID
	DWORD dwThreadID = GetCurrentThreadId();

	map<DWORD, _ConnectionPtr>::iterator it;
	it = m_mapThreadIDtoDBConnections.find(dwThreadID);

	if (it != m_mapThreadIDtoDBConnections.end())
	{
		// close the connection if it is open
		_ConnectionPtr ipConnection = it->second;
		if (ipConnection != NULL && ipConnection->State != adStateClosed)
		{
			// close the database connection
			ipConnection->Close();
		}
		// Post message indicating that the database's connection is no longer established
		postStatusUpdateNotification(kConnectionNotEstablished);
	}
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::postStatusUpdateNotification(EDatabaseWrapperObjectStatus eStatus)
{
	if (m_hUIWindow)
	{
		::PostMessage(m_hUIWindow, FP_DB_STATUS_UPDATE, (WPARAM) eStatus, NULL);
	}
}
//--------------------------------------------------------------------------------------------------
set<long> CFileProcessingDB::getSkippedFilesForAction(const _ConnectionPtr& ipConnection,
													  long nActionId)
{
	try
	{
		string strQuery = "SELECT FileID FROM SkippedFile WHERE ActionID = " + asString(nActionId);

		_RecordsetPtr ipFileSet(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI30293", ipFileSet != NULL);

		// Open the file set
		ipFileSet->Open(strQuery.c_str(), _variant_t((IDispatch*)ipConnection, true),
			adOpenForwardOnly, adLockReadOnly, adCmdText);

		set<long> setFileIds;
		while (ipFileSet->adoEOF == VARIANT_FALSE)
		{
			setFileIds.insert(getLongField(ipFileSet->Fields, "FileID"));
			ipFileSet->MoveNext();
		}

		return setFileIds;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30294");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::setFileActionState(_ConnectionPtr ipConnection,
										   const vector<SetFileActionData>& vecSetData,
										   string strAction, const string& strState)
{
	try
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
					&& setSkippedIds.find(data.FileID) != setSkippedIds.end() ?
					kActionSkipped : data.FromStatus,
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
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30295");
}
//--------------------------------------------------------------------------------------------------
EActionStatus CFileProcessingDB::setFileActionState(_ConnectionPtr ipConnection, long nFileID, 
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
						// Update the UPIID so to current process so it will be not be selected
						// again as a skipped file for the current process
						executeCmdQuery(ipConnection, "Update SkippedFile Set UPIID = " + 
							asString(m_nUPIID) + ", DateTimeStamp = GETDATE() WHERE FileID = " + asString(nFileID));
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
EActionStatus CFileProcessingDB::asEActionStatus  (const string& strStatus)
{
	EActionStatus easRtn;

	if (strStatus == "P")
	{
		easRtn = kActionPending;
	}
	else if (strStatus == "R")
	{
		easRtn = kActionProcessing;
	}
	else if (strStatus == "F")
	{
		easRtn = kActionFailed;
	}
	else if (strStatus == "C")
	{
		easRtn = kActionCompleted;
	}
	else if (strStatus == "U")
	{
		easRtn = kActionUnattempted;
	}
	else if (strStatus == "S")
	{
		easRtn = kActionSkipped;
	}
	else
	{
		THROW_LOGIC_ERROR_EXCEPTION("ELI13552");
	}
	return easRtn;
}
//--------------------------------------------------------------------------------------------------
string CFileProcessingDB::asStatusString(EActionStatus eStatus)
{
	switch (eStatus)
	{
	case kActionUnattempted:
		return "U";
	case kActionPending:
		return "P";
	case kActionProcessing:
		return "R";
	case kActionCompleted:
		return "C";
	case kActionFailed:
		return "F";
	case kActionSkipped:
		return "S";
	default:
		THROW_LOGIC_ERROR_EXCEPTION("ELI13562");
	}
	return "U";
}
//--------------------------------------------------------------------------------------------------
string CFileProcessingDB::asStatusName(const string& strStatus)
{
	if (strStatus.length() != 1)
	{
		THROW_LOGIC_ERROR_EXCEPTION("ELI29623");
	}

	switch (strStatus[0])
	{
		case 'U':	return "Unattempted";
		case 'P':	return "Pending";
		case 'R':	return "Processing";
		case 'C':	return "Completed";
		case 'F':	return "Failed";
		case 'S':	return "Skipped";

		default: THROW_LOGIC_ERROR_EXCEPTION("ELI29625");
	}
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::addQueueEventRecord(_ConnectionPtr ipConnection, long nFileID, 
											long nActionID, string strFileName, 
											string strQueueEventCode)
{
	try
	{
		INIT_EXCEPTION_AND_TRACING("MLI02855");
		try
		{
			// Check if QueueEvent Table should be updated
			if (!m_bUpdateQueueEventTable)
			{
				return;
			}
			_lastCodePos = "10";

			_RecordsetPtr ipQueueEventSet(__uuidof(Recordset));
			ASSERT_RESOURCE_ALLOCATION("ELI13591", ipQueueEventSet != NULL);

			// Open the QueueEvent table
			ipQueueEventSet->Open("QueueEvent", _variant_t((IDispatch *)ipConnection, true), 
				adOpenDynamic, adLockOptimistic, adCmdTableDirect);
			_lastCodePos = "20";

			// Add a new record
			ipQueueEventSet->AddNew();
			_lastCodePos = "30";

			FieldsPtr ipFields = ipQueueEventSet->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI26875", ipFields != NULL);

			//  add the field values to the new record
			setLongField(ipFields, "FileID",  nFileID);
			_lastCodePos = "40";

			setLongField(ipFields, "ActionID", nActionID);
			_lastCodePos = "45";

			setStringField(ipFields, "DateTimeStamp", 
				getSQLServerDateTime(ipConnection));
			_lastCodePos = "50";
			
			setStringField(ipFields, "QueueEventCode", strQueueEventCode);
			_lastCodePos = "60";

			setLongField(ipFields, "FAMUserID", getFAMUserID(ipConnection));
			_lastCodePos = "70";

			setLongField(ipFields, "MachineID", getMachineID(ipConnection));
			_lastCodePos = "80";

			// File should exist for these options
			if (strQueueEventCode == "A" || strQueueEventCode == "M")
			{
				_lastCodePos = "80_10";

				// if adding or modifing the file add the file modified and file size fields
				CTime fileTime;
				fileTime = getFileModificationTimeStamp(strFileName);
				_lastCodePos = "80_20";

				string strFileModifyTime = fileTime.Format("%m/%d/%y %I:%M:%S %p");
				setStringField(	ipFields, "FileModifyTime", strFileModifyTime);
				_lastCodePos = "80_30";

				// Get the file size
				long long llFileSize;
				llFileSize = getSizeOfFile(strFileName);
				_lastCodePos = "80_40";

				// Set the file size in the table
				setLongLongField(ipFields, "FileSizeInBytes", llFileSize);
				_lastCodePos = "80_50";
			}
			// Update the QueueEvent table
			ipQueueEventSet->Update();
			_lastCodePos = "90";
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25376");
	}
	catch(UCLIDException& uex)
	{
		uex.addDebugInfo("File ID", nFileID);
		uex.addDebugInfo("File To Add", strFileName);
		uex.addDebugInfo("Queue Event Code", strQueueEventCode);
		if (ipConnection == NULL)
		{
			uex.addDebugInfo("ConnectionValue", "NULL");
		}

		throw uex;
	}
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::addFileActionStateTransition (_ConnectionPtr ipConnection,
													  long nFileID, long nActionID, 
													  const string &strFromState, 
													  const string &strToState, 
													  const string &strException, 
													  const string &strComment)
{
	string strInsertQuery = "";
	try
	{
		try
		{
			// check if updates to FileActionStateTransition table are required
			if (!m_bUpdateFASTTable || strToState == strFromState)
			{
				// nothing to do
				return;
			}

			// Build the insert query for adding the new row
			strInsertQuery = "INSERT INTO " + gstrFILE_ACTION_STATE_TRANSITION
				+ " (FileID, ActionID, ASC_From, ASC_To, DateTimeStamp, FAMUserID, MachineID, "
				"Exception, Comment) VALUES (" + asString(nFileID) + ", " + asString(nActionID)
				+ ", '" + strFromState + "', '" + strToState + "', GETDATE(), "
				+ asString(getFAMUserID(ipConnection)) + ", " + asString(getMachineID(ipConnection)) + ", "
				+ (strException.empty() ? "NULL" : ("'" + strException + "'")) + ", "
				+ (strComment.empty() ? "NULL" : ("'" + strComment + "'")) + ")";

			// Run the query
			executeCmdQuery(ipConnection, strInsertQuery);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28236");
	}
	catch(UCLIDException& ue)
	{
		ue.addDebugInfo("SQL Query", strInsertQuery);
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
long CFileProcessingDB::getFileID(_ConnectionPtr ipConnection, string& rstrFileName)
{
	try
	{
		return getKeyID(ipConnection, gstrFAM_FILE, "FileName", rstrFileName, false);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26720");
}
//--------------------------------------------------------------------------------------------------
long CFileProcessingDB::getActionID(_ConnectionPtr ipConnection, string& rstrActionName)
{
	try
	{
		return getKeyID(ipConnection, gstrACTION, "ASCName", rstrActionName, false);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26721");
}
//--------------------------------------------------------------------------------------------------
string CFileProcessingDB::getActionName(_ConnectionPtr ipConnection, long nActionID)
{
	try
	{
		_RecordsetPtr ipAction(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI14046", ipAction != NULL);

		// Oepn Action table
		ipAction->Open("Action", _variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
			adLockReadOnly, adCmdTableDirect);

		// Setup criteria to find
		string strCriteria = "ID = " + asString(nActionID);

		// search for the given action ID
		ipAction->Find(strCriteria.c_str(), 0, adSearchForward);
		if (ipAction->adoEOF == VARIANT_TRUE)
		{
			// Action ID was not found
			UCLIDException ue ("ELI14047", "Action ID was not found.");
			ue.addDebugInfo("Action ID", nActionID);
			throw ue;
		}

		// return the found Action name
		return getStringField(ipAction->Fields, "ASCName");
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26722");
}
//--------------------------------------------------------------------------------------------------
_RecordsetPtr CFileProcessingDB::getActionSet(_ConnectionPtr ipConnection, const string &strAction)
{
	// Create a pointer to a recordset
	_RecordsetPtr ipActionSet(__uuidof(Recordset));
	ASSERT_RESOURCE_ALLOCATION("ELI29155", ipActionSet != NULL);

	// Setup select statement to open Action Table
	string strActionSelect = "SELECT ID, ASCName FROM Action WHERE ASCName = '" + strAction + "'";

	// Open the Action table in the database
	ipActionSet->Open(strActionSelect.c_str(), _variant_t((IDispatch *)ipConnection, true), 
		adOpenDynamic, adLockOptimistic, adCmdText);

	return ipActionSet;
}
//--------------------------------------------------------------------------------------------------
long CFileProcessingDB::addActionToRecordset(_ConnectionPtr ipConnection, 
											 _RecordsetPtr ipRecordset, const string &strAction)
{
	try
	{
		// Add a new record
		ipRecordset->AddNew();

		// Set the values of the ASCName field
		setStringField(ipRecordset->Fields, "ASCName", strAction);

		// Add the record to the Action Table
		ipRecordset->Update();

		// Get the ID of the new Action
		long lActionId = getLastTableID(ipConnection, "Action");

		// Add column to the FAMFile table
		addActionColumn(ipConnection, strAction);

		return lActionId;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29158")
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::addASTransFromSelect(_ConnectionPtr ipConnection,
											  const string &strAction, long nActionID,
											  const string &strToState, const string &strException,
											  const string &strComment, const string &strWhereClause, 
											  const string &strTopClause)
{
	try
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
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26937");
}
//--------------------------------------------------------------------------------------------------
_ConnectionPtr  CFileProcessingDB::getDBConnection()
{
	INIT_EXCEPTION_AND_TRACING("MLI00018");
	try
	{
		try
		{
			// Get the current threads ID
			DWORD dwThreadID = GetCurrentThreadId();

			_ConnectionPtr ipConnection = NULL;

			// Lock mutex to keep other instances from running code that may cause the
			// connection to be reset
			CSingleLock lg(&m_mutex, TRUE);

			map<DWORD, _ConnectionPtr>::iterator it;
			it = m_mapThreadIDtoDBConnections.find(dwThreadID);
			_lastCodePos = "5";

			if (it != m_mapThreadIDtoDBConnections.end())
			{
				ipConnection = it->second;
			}

			// check to see if the DB connection has been allocated
			if (ipConnection == NULL)
			{
				_lastCodePos = "10";
				ipConnection.CreateInstance(__uuidof(Connection));
				ASSERT_RESOURCE_ALLOCATION("ELI13650",  ipConnection != NULL);

				// Reset the schema version to indicate that it needs to be read from DB
				m_iDBSchemaVersion = 0;
			}

			_lastCodePos = "20";

			// if closed and Database server and database name are defined,  open the database connection
			if (ipConnection->State == adStateClosed && !m_strDatabaseServer.empty() 
				&& !m_strDatabaseName.empty())
			{
				_lastCodePos = "30";

				// Since the database is being opened reset the m_lFAMUserID and m_lMachineID
				m_lFAMUserID = 0;
				m_lMachineID = 0;

				// Set the status of the connection to not connected
				m_strCurrentConnectionStatus = gstrNOT_CONNECTED;

				// Create the connection string with the current server and database
				string strConnectionString = createConnectionString(m_strDatabaseServer, 
					m_strDatabaseName);

				_lastCodePos = "40";

				// Open the database
				ipConnection->Open (strConnectionString.c_str(), "", "", adConnectUnspecified);

				_lastCodePos = "50";

				// Reset the schema version to indicate that it needs to be read from DB
				m_iDBSchemaVersion = 0;

				// Add the connection to the map
				m_mapThreadIDtoDBConnections[dwThreadID] = ipConnection;

				// Load the database settings
				loadDBInfoSettings(ipConnection);

				_lastCodePos = "60";

				// Set the command timeout
				ipConnection->CommandTimeout = m_iCommandTimeout;

				_lastCodePos = "70";

				// Connection has been established 
				m_strCurrentConnectionStatus = gstrCONNECTION_ESTABLISHED;

				_lastCodePos = "80";

				// Post message indicating that the database's connection is now established
				postStatusUpdateNotification(kConnectionEstablished);
			}

			// return the open connection
			return ipConnection;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI18320");
	}

	catch (UCLIDException ue)
	{
		// if we catch any exception, that means that we could not
		// establish a connection successfully
		// Post message indicating that the database's connection is no longer established
		postStatusUpdateNotification(kConnectionNotEstablished);

		// Update the connection Status string 
		// TODO:  may want to get more detail as to what is the problem
		m_strCurrentConnectionStatus = gstrUNABLE_TO_CONNECT_TO_SERVER;

		// throw the exception to the outer scope
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI13668", "File Processing DB");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::reCalculateStats(_ConnectionPtr ipConnection, long nActionID)
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
void CFileProcessingDB::dropTables(bool bRetainUserTables)
{
	try
	{
		// First remove all Product Specific stuff
		removeProductSpecificDB();

		// Get the list of tables
		vector<string> vecTables; 
		getExpectedTables(vecTables);

		// Retain the user tables if necessary
		if (bRetainUserTables)
		{
			eraseFromVector(vecTables, gstrDB_INFO);
			eraseFromVector(vecTables, gstrFAM_TAG);
			eraseFromVector(vecTables, gstrUSER_CREATED_COUNTER);
			eraseFromVector(vecTables, gstrLOGIN);
		}

		// Drop the tables in the vector
		dropTablesInVector(getDBConnection(), vecTables);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27605")
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::addTables(bool bAddUserTables)
{
	try
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
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI18011");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::initializeTableValues(bool bInitializeUserTables)
{
	try
	{
		vector<string> vecQueries;

		// Add valid action states to the Action State table
		vecQueries.push_back("INSERT INTO [ActionState] ([Code], [Meaning]) "
			"VALUES('C', 'Complete')");

		vecQueries.push_back("INSERT INTO [ActionState] ([Code], [Meaning]) "
			"VALUES('F', 'Failed')");

		vecQueries.push_back("INSERT INTO [ActionState] ([Code], [Meaning]) "
			"VALUES('P', 'Pending')");

		vecQueries.push_back("INSERT INTO [ActionState] ([Code], [Meaning]) "
			"VALUES('R', 'Processing')");

		vecQueries.push_back("INSERT INTO [ActionState] ([Code], [Meaning]) "
			"VALUES('U', 'Unattempted')");

		vecQueries.push_back("INSERT INTO [ActionState] ([Code], [Meaning]) "
			"VALUES('S', 'Skipped')");

		// Add Valid Queue event codes the QueueEventCode table
		vecQueries.push_back("INSERT INTO [QueueEventCode] ([Code], [Description]) "
			"VALUES('A', 'File added to queue')");
		
		vecQueries.push_back("INSERT INTO [QueueEventCode] ([Code], [Description]) "
			"VALUES('D', 'File deleted from queue')");

		vecQueries.push_back("INSERT INTO [QueueEventCode] ([Code], [Description]) "
			"VALUES('F', 'Folder was deleted')");

		vecQueries.push_back("INSERT INTO [QueueEventCode] ([Code], [Description]) "
			"VALUES('M', 'File was modified')");

		vecQueries.push_back("INSERT INTO [QueueEventCode] ([Code], [Description]) "
			"VALUES('R', 'File was renamed')");

		// Initialize the DB Info settings if necessary
		if (bInitializeUserTables)
		{
			// Add the schema version to the DBInfo table
			string strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrFAMDB_SCHEMA_VERSION +
				"', '" + asString(glFAMDBSchemaVersion) + "')";
			vecQueries.push_back(strSQL);

			// Add Command Timeout setting
			strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrCOMMAND_TIMEOUT +
				"', '" + asString(glDEFAULT_COMMAND_TIMEOUT) + "')";
			vecQueries.push_back(strSQL);

			// Add Update Queue Event Table setting
			strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrUPDATE_QUEUE_EVENT_TABLE 
				+ "', '1')";
			vecQueries.push_back(strSQL);

			// Add Update Queue Event Table setting
			strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrUPDATE_FAST_TABLE + "', '1')";
			vecQueries.push_back(strSQL);

			// Add Auto Delete File Action Comment On Complete setting
			strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrAUTO_DELETE_FILE_ACTION_COMMENT
				+ "', '0')";
			vecQueries.push_back(strSQL);

			// Add Require Password To Process All Skipped Files setting (default to true)
			strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrREQUIRE_PASSWORD_TO_PROCESS_SKIPPED
				+ "', '1')";
			vecQueries.push_back(strSQL);

			// Add Allow Dynamic Tag Creation setting (default to false)
			strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrALLOW_DYNAMIC_TAG_CREATION
				+ "', '0')";
			vecQueries.push_back(strSQL);

			// Add AutoRevertLockedFiles setting (default to true)
			strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrAUTO_REVERT_LOCKED_FILES
				+ "', '1')";
			vecQueries.push_back(strSQL);

			// Add AutoRevertTimeOutInMinutes setting (default to 60 minutes)
			strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrAUTO_REVERT_TIME_OUT_IN_MINUTES
				+ "', '60')";
			vecQueries.push_back(strSQL);
			
			// Add AutoRevertNotifyEmailList setting (default to empy string)
			strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrAUTO_REVERT_NOTIFY_EMAIL_LIST
				+ "', '')";
			vecQueries.push_back(strSQL);

			// Add NumberOfConnectionRetries setting (default to empy string)
			strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrNUMBER_CONNECTION_RETRIES
				+ "', '10')";
			vecQueries.push_back(strSQL);
			
			// Add ConnectionRetryTimeout setting (default to empy string)
			strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrCONNECTION_RETRY_TIMEOUT
				+ "', '120')";
			vecQueries.push_back(strSQL);

			// Add StoreFAMSessionHistory setting (default to true)
			strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrSTORE_FAM_SESSION_HISTORY
				+ "', '1')";
			vecQueries.push_back(strSQL);

			// Add the EnableInputEventTracking setting (default to false)
			strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrENABLE_INPUT_EVENT_TRACKING
				+ "', '0')";
			vecQueries.push_back(strSQL);

			// Add the InputEventHistorySize setting (default to 30 days)
			strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrINPUT_EVENT_HISTORY_SIZE
				+ "', '30')";
			vecQueries.push_back(strSQL);

			// Add RequireAuthenticationBeforeRun setting (default to false)
			strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrREQUIRE_AUTHENTICATION_BEFORE_RUN
				+ "', '0')";
			vecQueries.push_back(strSQL);

			// Add Auto Create Actions setting (default to false)
			strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrAUTO_CREATE_ACTIONS
				+ "', '0')";
			vecQueries.push_back(strSQL);

			// Add Skip Authentication For Services On Machines
			strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('"
				+ gstrSKIP_AUTHENTICATION_ON_MACHINES + "', '')";
			vecQueries.push_back(strSQL);
		}

		// Execute all of the queries
		executeVectorOfSQL(getDBConnection(), vecQueries);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27606")
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::copyActionStatus(const _ConnectionPtr& ipConnection, const string& strFrom, 
										 string strTo, bool bAddTransRecords, long nToActionID)
{
	try
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
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27054");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::addActionColumn(const _ConnectionPtr& ipConnection, const string& strAction)
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
void CFileProcessingDB::removeActionColumn(const _ConnectionPtr& ipConnection,
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
void CFileProcessingDB::updateStats(_ConnectionPtr ipConnection, long nActionID, 
									EActionStatus eFromStatus, EActionStatus eToStatus, 
									UCLID_FILEPROCESSINGLib::IFileRecordPtr ipNewRecord, 
									UCLID_FILEPROCESSINGLib::IFileRecordPtr ipOldRecord,
									bool bUpdateAndSaveStats)
{
	// Only time a ipOldRecord can be NULL is if the from status is kActionUnattempted
	if (eFromStatus != kActionUnattempted && ipOldRecord == NULL)
	{
		UCLIDException ue("ELI17029", "Must have an old record");
		ue.addDebugInfo("FromStatus", eFromStatus);
		ue.addDebugInfo("ToStatus", eToStatus);
		ue.addDebugInfo("ActionID", nActionID);
		throw ue;
	}

	// Only time a ipNewRecord can be NULL is if the to status is kActionUnattempted
	if (eToStatus != kActionUnattempted && ipNewRecord == NULL)
	{
		UCLIDException ue("ELI17030", "Must have a new record");
		ue.addDebugInfo("FromStatus", eFromStatus);
		ue.addDebugInfo("ToStatus", eToStatus);
		ue.addDebugInfo("ActionID", nActionID);
		throw ue;
	}

	// Attempt to get the file data
	LONGLONG llOldFileSize(-1), llNewFileSize(-1);
	long lOldPages(-1), lNewPages(-1), lTempFileID(-1), lTempActionID(-1);
	UCLID_FILEPROCESSINGLib::EFilePriority ePriority(
		(UCLID_FILEPROCESSINGLib::EFilePriority)kPriorityDefault);
	_bstr_t bstrTemp;
	if (ipOldRecord != NULL)
	{
		ipOldRecord->GetFileData(&lTempFileID, &lTempActionID, bstrTemp.GetAddress(),
			&llOldFileSize, &lOldPages, &ePriority);
	}
	if (ipNewRecord != NULL)
	{
		// If the records are the same, just copy the data that was already retrieved
		if (ipNewRecord == ipOldRecord)
		{
			llNewFileSize = llOldFileSize;
			lNewPages = lOldPages;
		}
		else
		{
			ipNewRecord->GetFileData(&lTempFileID, &lTempActionID, bstrTemp.GetAddress(),
				&llNewFileSize, &lNewPages, &ePriority);
		}
	}

	// Nothing to do if the "from" status == the "to" status
	if (eFromStatus == eToStatus)
	{
		// If the to and from status is unattempted there is nothing to do
		// Otherwise if the FileSize and the number Pages are the same there is nothing to do
		if (eFromStatus == kActionUnattempted ||
			(ipNewRecord != NULL && ipOldRecord != NULL &&
			llNewFileSize == llOldFileSize && 
			lNewPages == lOldPages))
		{
			return;
		}
	}

	// load the record from the ActionStatistics table
	if (bUpdateAndSaveStats)
	{
		if (loadStats(ipConnection, nActionID))
		{
			// if the stats were recalculated the all added on changed records have been included
			return;
		}
	}

	// Create an ActionStatistics pointer to return the values
	UCLID_FILEPROCESSINGLib::IActionStatisticsPtr ipActionStats;
	ipActionStats = m_mapActionIDtoStats[nActionID];

	// Transfer the data from the recordset to the ActionStatisticsPtr
	switch (eToStatus)
	{
	case kActionFailed:
		{
			// Make sure the ipNewRecord is not NULL
			ASSERT_ARGUMENT("ELI17046", ipNewRecord != NULL);
			long lNumDocsFailed(0), lNumPagesFailed(0);
			LONGLONG llNumBytesFailed(0);
			ipActionStats->GetFailed(&lNumDocsFailed, &lNumPagesFailed, &llNumBytesFailed);
			ipActionStats->SetFailed(lNumDocsFailed+1, lNumPagesFailed + lNewPages,
				llNumBytesFailed + llNewFileSize);
			break;
		}

	case kActionCompleted:
		{
			// Make sure the ipNewRecord is not NULL
			ASSERT_ARGUMENT("ELI17048", ipNewRecord != NULL);
			long lNumDocsComplete(0), lNumPagesComplete(0);
			LONGLONG llNumBytesComplete(0);
			ipActionStats->GetComplete(&lNumDocsComplete, &lNumPagesComplete, &llNumBytesComplete);
			ipActionStats->SetComplete(lNumDocsComplete+1, lNumPagesComplete + lNewPages,
				llNumBytesComplete + llNewFileSize);
			break;
		}

	case kActionSkipped:
		{
			// Make sure the ipNewRecord is not NULL
			ASSERT_ARGUMENT("ELI26803", ipNewRecord != NULL);
			long lNumDocsSkipped(0), lNumPagesSkipped(0);
			LONGLONG llNumBytesSkipped(0);
			ipActionStats->GetSkipped(&lNumDocsSkipped, &lNumPagesSkipped, &llNumBytesSkipped);
			ipActionStats->SetSkipped(lNumDocsSkipped+1, lNumPagesSkipped + lNewPages,
				llNumBytesSkipped + llNewFileSize);
			break;
		}
	}
	// Add the new counts to the totals if the to status is not unattempted
	if (eToStatus != kActionUnattempted)
	{
		// Make sure the ipNewRecord is not NULL
		ASSERT_ARGUMENT("ELI17050", ipNewRecord != NULL);
		long lNumDocsTotal(0), lNumPagesTotal(0);
		LONGLONG llNumBytesTotal(0);
		ipActionStats->GetTotals(&lNumDocsTotal, &lNumPagesTotal, &llNumBytesTotal);
		ipActionStats->SetTotals(lNumDocsTotal+1, lNumPagesTotal + lNewPages,
			llNumBytesTotal + llNewFileSize);
	}

	switch (eFromStatus)
	{
	case kActionFailed:
		{
			// Make sure the ipOldRecord is not NULL
			ASSERT_ARGUMENT("ELI17052", ipOldRecord != NULL);
			long lNumDocsFailed(0), lNumPagesFailed(0);
			LONGLONG llNumBytesFailed(0);
			ipActionStats->GetFailed(&lNumDocsFailed, &lNumPagesFailed, &llNumBytesFailed);
			ipActionStats->SetFailed(lNumDocsFailed-1, lNumPagesFailed - lOldPages,
				llNumBytesFailed - llOldFileSize);
			break;
		}

	case kActionCompleted:
		{
			// Make sure the ipOldRecord is not NULL
			long lNumDocsComplete(0), lNumPagesComplete(0);
			LONGLONG llNumBytesComplete(0);
			ipActionStats->GetComplete(&lNumDocsComplete, &lNumPagesComplete, &llNumBytesComplete);
			ipActionStats->SetComplete(lNumDocsComplete-1, lNumPagesComplete - lOldPages,
				llNumBytesComplete - llOldFileSize);
			break;
		}

	case kActionSkipped:
		{
			// Make sure the ipOldRecord is not NULL
			ASSERT_ARGUMENT("ELI17053", ipOldRecord != NULL);
			long lNumDocsSkipped(0), lNumPagesSkipped(0);
			LONGLONG llNumBytesSkipped(0);
			ipActionStats->GetSkipped(&lNumDocsSkipped, &lNumPagesSkipped, &llNumBytesSkipped);
			ipActionStats->SetSkipped(lNumDocsSkipped-1, lNumPagesSkipped - lOldPages,
				llNumBytesSkipped - llOldFileSize);
			break;
		}
	}

	// Remove the counts form the totals if the from status is not unattempted
	if (eFromStatus != kActionUnattempted)
	{
		// Make sure the ipOldRecord is not NULL
		ASSERT_ARGUMENT("ELI17055", ipOldRecord != NULL);
		long lNumDocsTotal(0), lNumPagesTotal(0);
		LONGLONG llNumBytesTotal(0);
		ipActionStats->GetTotals(&lNumDocsTotal, &lNumPagesTotal, &llNumBytesTotal);
		ipActionStats->SetTotals(lNumDocsTotal-1, lNumPagesTotal - lOldPages,
			llNumBytesTotal - llOldFileSize);
	}

	if (bUpdateAndSaveStats)
	{
		saveStats(ipConnection, nActionID);
	}
}
//--------------------------------------------------------------------------------------------------
bool CFileProcessingDB::loadStats(_ConnectionPtr ipConnection, long nActionID)
{
	// Create a pointer to a recordset
	_RecordsetPtr ipActionStatSet(__uuidof(Recordset));
	ASSERT_RESOURCE_ALLOCATION("ELI14099", ipActionStatSet != NULL);

	// Select the existing Statistics record if it exists
	string strSelectStat = "SELECT * FROM ActionStatistics WHERE ActionID = " + asString(nActionID);

	// Open the recordse to for the statisics with the record for ActionID if it exists
	ipActionStatSet->Open(strSelectStat.c_str(), 
		_variant_t((IDispatch *)ipConnection, true), adOpenDynamic, 
		adLockOptimistic, adCmdText);

	bool bRecalculated = false;
	if (ipActionStatSet->adoEOF == VARIANT_TRUE)
	{
		reCalculateStats(ipConnection, nActionID);
		bRecalculated = true;
		ipActionStatSet->Requery(adOptionUnspecified);
	}
	if (ipActionStatSet->adoEOF == VARIANT_TRUE)
	{
		UCLIDException ue("ELI14100", "Unable to load statistics.");
		ue.addDebugInfo("ActionID", nActionID);
		throw ue;
	}

	// Get the fields from the action stat set
	FieldsPtr ipFields = ipActionStatSet->Fields;
	ASSERT_RESOURCE_ALLOCATION("ELI26863", ipFields != NULL);

	// Create an ActionStatistics pointer to return the values
	UCLID_FILEPROCESSINGLib::IActionStatisticsPtr ipActionStats(CLSID_ActionStatistics);
	ASSERT_RESOURCE_ALLOCATION("ELI14101", ipActionStats != NULL);

	// Get all the data from the recordset
	long lNumDocsFailed =  getLongField(ipFields, "NumDocumentsFailed");
	long lNumPagesFailed = getLongField(ipFields, "NumPagesFailed");
	LONGLONG llNumBytesFailed = getLongLongField(ipFields, "NumBytesFailed");
	long lNumDocsSkipped =  getLongField(ipFields, "NumDocumentsSkipped");
	long lNumPagesSkipped = getLongField(ipFields, "NumPagesSkipped");
	LONGLONG llNumBytesSkipped = getLongLongField(ipFields, "NumBytesSkipped");
	long lNumDocsComplete = getLongField(ipFields, "NumDocumentsComplete");
	long lNumPagesComplete = getLongField(ipFields, "NumPagesComplete");
	LONGLONG llNumBytesComplete = getLongLongField(ipFields, "NumBytesComplete");
	long lNumDocs = getLongField(ipFields, "NumDocuments");
	long lNumPages = getLongField(ipFields, "NumPages");
	LONGLONG llNumBytes = getLongLongField(ipFields, "NumBytes");

	// Transfer the data from the recordset to the ActionStatisticsPtr
	ipActionStats->SetAllStatistics(lNumDocs, lNumDocsComplete, lNumDocsFailed, lNumDocsSkipped,
		lNumPages, lNumPagesComplete, lNumPagesFailed, lNumPagesSkipped, llNumBytes,
		llNumBytesComplete, llNumBytesFailed, llNumBytesSkipped);

	// save the stats back to the map
	m_mapActionIDtoStats[nActionID] = ipActionStats;
	return bRecalculated;
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::saveStats(_ConnectionPtr ipConnection, long nActionID)
{
	// Find the stats for the given action
	map<long, UCLID_FILEPROCESSINGLib::IActionStatisticsPtr>::iterator it =
		m_mapActionIDtoStats.find(nActionID);
	if (it == m_mapActionIDtoStats.end())
	{
		UCLIDException ue("ELI14104", "No Statistics to save.");
		ue.addDebugInfo("ActionID", nActionID);
		throw ue;
	}

	// Create a pointer to a recordset
	_RecordsetPtr ipActionStatSet(__uuidof(Recordset));
	ASSERT_RESOURCE_ALLOCATION("ELI19506", ipActionStatSet != NULL);

	// Select the existing Statistics record if it exists
	string strSelectStat = "SELECT * FROM ActionStatistics WHERE ActionID = " + asString(nActionID);

	// Open the recordse to for the statisics with the record for ActionID if it exists
	ipActionStatSet->Open(strSelectStat.c_str(), _variant_t((IDispatch *)ipConnection, true), 
		adOpenDynamic, adLockOptimistic, adCmdText);

	if (ipActionStatSet->adoEOF == VARIANT_FALSE)
	{
		// Create an ActionStatistics pointer to return the values
		UCLID_FILEPROCESSINGLib::IActionStatisticsPtr ipActionStats = it->second;

		// Get all the statistics
		long lNumDocs(-1), lNumDocsComplete(-1), lNumDocsFailed(-1), lNumDocsSkipped(-1),
			lNumPages(-1), lNumPagesComplete(-1), lNumPagesFailed(-1), lNumPagesSkipped(-1);
		LONGLONG llNumBytes(-1), llNumBytesComplete(-1), llNumBytesFailed(-1), llNumBytesSkipped(-1);
		ipActionStats->GetAllStatistics(&lNumDocs, &lNumDocsComplete, &lNumDocsFailed,
			&lNumDocsSkipped, &lNumPages, &lNumPagesComplete, &lNumPagesFailed, &lNumPagesSkipped,
			&llNumBytes, &llNumBytesComplete, &llNumBytesFailed, &llNumBytesSkipped);

		// Get the fields from the action stat set
		FieldsPtr ipFields = ipActionStatSet->Fields;
		ASSERT_RESOURCE_ALLOCATION("ELI26877", ipFields != NULL);

		// Transfer the data from the ActionStatisticsPtr to the recordset 
		setLongField(ipFields, "NumDocumentsFailed", lNumDocsFailed);
		setLongField(ipFields, "NumPagesFailed", lNumPagesFailed);
		setLongLongField(ipFields, "NumBytesFailed", llNumBytesFailed);
		setLongField(ipFields, "NumDocumentsComplete", lNumDocsComplete);
		setLongField(ipFields, "NumPagesComplete", lNumPagesComplete);
		setLongLongField(ipFields, "NumBytesComplete", llNumBytesComplete);
		setLongField(ipFields, "NumDocumentsSkipped", lNumDocsSkipped);
		setLongField(ipFields, "NumPagesSkipped", lNumPagesSkipped);
		setLongLongField(ipFields, "NumBytesSkipped", llNumBytesSkipped);
		setLongField(ipFields, "NumDocuments", lNumDocs);
		setLongField(ipFields, "NumPages", lNumPages);
		setLongLongField(ipFields, "NumBytes", llNumBytes);

		// Update the action statistics
		ipActionStatSet->Update();
	}
	else
	{
		UCLIDException uex("ELI26876", "Action statistics do not exist for this action!");
		uex.addDebugInfo("Action ID", nActionID);
		throw uex;
	}
}
//--------------------------------------------------------------------------------------------------
int CFileProcessingDB::getDBSchemaVersion()
{
	// if the value of the SchemaVersion is not 0 then it has already been read from the db
	if (m_iDBSchemaVersion != 0)
	{
		return m_iDBSchemaVersion;
	}
	
	// Get all of the settings from the DBInfo
	loadDBInfoSettings(getDBConnection());

	// if the Schema version is still 0 there is a problem
	if (m_iDBSchemaVersion == 0)
	{
		UCLIDException ue("ELI14775", "Unable to obtain DB Schema Version.");
		throw ue;
	}

	// return the Schema version
	return m_iDBSchemaVersion;
}

//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::validateDBSchemaVersion()
{
	// Get the Schema Version from the database
	int iDBSchemaVersion = getDBSchemaVersion();
	if (iDBSchemaVersion != glFAMDBSchemaVersion)
	{
		// Update the current connection status string
		m_strCurrentConnectionStatus = gstrWRONG_SCHEMA;

		UCLIDException ue("ELI14380", "DB Schema version does not match.");
		ue.addDebugInfo("SchemaVer in Database", iDBSchemaVersion);
		ue.addDebugInfo("SchemaVer expected", glFAMDBSchemaVersion);
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::lockDB(_ConnectionPtr ipConnection)
{
	// If DB is already locked return
	if (m_bDBLocked)
	{
		return;
	}

	// Lock insertion string for this process
	string strAddLockSQL = "INSERT INTO LockTable (LockID, UPI) VALUES (1, '" 
		+  m_strUPI + "')";
				
	// Keep trying to lock the DB until it is locked
	while (!m_bDBLocked)
	{
		// Flag to indicate if the connection is in a good state
		// this will be determined if the TransactionGuard does not throw an exception
		// this needs to be initialized each time through the loop
		bool bConnectionGood = false;

		// put this in a try catch block to catch the possiblity that another 
		// instance is trying to lock the DB at exactly the same time
		try
		{
			try
			{
				// Lock while updating the lock table and m_bDBLocked variable
				CSingleLock lock(&m_mutex, TRUE);

				// Check again to ensure no one else set the m_bDBLocked to true
				if (m_bDBLocked)
				{
					break;
				}

				TransactionGuard tg(ipConnection);

				// Create a pointer to a recordset
				_RecordsetPtr ipLockTable(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI14550", ipLockTable != NULL);

				// Open recordset with the locktime 
				ipLockTable->Open(gstrDB_LOCK_QUERY.c_str(), 
					_variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
					adLockReadOnly, adCmdText);

				// If we get to here the db connection should be valid
				bConnectionGood = true;

				// If there is an existing record check to see if the time to see if the lock
				// has been on for more than the lock timeout
				if (ipLockTable->adoEOF == VARIANT_FALSE)
				{
					// Get the time locked value from the record 
					long nSecondsLocked = getLongField(ipLockTable->Fields, "TimeLocked"); 
					if (nSecondsLocked > m_lDBLockTimeout)
					{
						// Delete the lock record since it has been in the db for
						// more than the lock period
						executeCmdQuery(ipConnection, gstrDELETE_DB_LOCK);

						// commit the changes
						// this may throw an exception if another instance gets here
						// at the same time
						tg.CommitTrans();

						// log an exception that the lock has been reset
						UCLIDException ue("ELI15406", "Lock timed out. Lock has been reset.");
						ue.addDebugInfo ("Lock Timeout", m_lDBLockTimeout);
						ue.addDebugInfo ("Actual Lock Time", asString(nSecondsLocked));
						ue.log();

						// Restart the loop since we don't want to assume this instance will 
						// get the lock
						continue;
					}
				}

				// Add the lock
				executeCmdQuery(ipConnection, strAddLockSQL);

				// Commit the changes
				// If a DB lock is in the table for another process this will throw an exception
				tg.CommitTrans();

				// Update the lock flag to indicate the DB is locked
				m_bDBLocked = true;

				// Lock obtained, break from the loop to avoid sleep call below
				break;
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI14973");
		}
		catch(UCLIDException &ue)
		{
			// if the bConnectionGood flag is false the exception should be thrown
			if (!bConnectionGood) 
			{
				UCLIDException uexOuter("ELI15459", "Connection is no longer good", ue);
				postStatusUpdateNotification(kConnectionNotEstablished);
				throw uexOuter;
			}
		};
		
		// wait a random time from 0 to 50 ms
		unsigned int iRandom;
		rand_s(&iRandom);
		Sleep(iRandom % 50);
	}
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::unlockDB(_ConnectionPtr ipConnection)
{
	CSingleLock lock(&m_mutex, TRUE);

	// if DB is already unlocked return
	if (!m_bDBLocked)
	{
		return;
	}

	// Delete the Lock record
	string strDeleteSQL = gstrDELETE_DB_LOCK + " WHERE UPI = '" + m_strUPI + "'";
	executeCmdQuery(ipConnection, strDeleteSQL);

	// Mark DB as unlocked
	m_bDBLocked = false;
}
//--------------------------------------------------------------------------------------------------
bool CFileProcessingDB::getEncryptedPWFromDB(string &rstrEncryptedPW, bool bUseAdmin)
{
	try
	{
		// Open the Login Table
		// Lock the mutex for this instance
		CSingleLock lock(&m_mutex, TRUE);

		// Create a pointer to a recordset
		_RecordsetPtr ipLoginSet(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI15103", ipLoginSet != NULL);

		// setup the SQL Query to get the encrypted combo for admin or user
		string strSQL = "SELECT * FROM LOGIN WHERE UserName = '" + 
			((bUseAdmin) ? gstrADMIN_USER : m_strFAMUserName) + "'";

		// Open the set for the user being logged in
		ipLoginSet->Open(strSQL.c_str(), _variant_t((IDispatch *)getDBConnection(), true), 
			adOpenStatic, adLockReadOnly, adCmdText);

		// user was in the DB if not at the end of file
		if (ipLoginSet->adoEOF == VARIANT_FALSE)
		{
			// Return the encrypted password that is stored in the DB
			rstrEncryptedPW = getStringField(ipLoginSet->Fields, "Password");
			return true;
		}

		// record not found in DB for user or admin
		rstrEncryptedPW = "";
		return false;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29897");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::encryptAndStoreUserNamePassword(const string strUserNameAndPassword,
														bool bUseAdmin, bool bFailIfUserDoesNotExist)
{
	// Get the encrypted version of the combined string
	string strEncryptedCombined = getEncryptedString(strUserNameAndPassword);

	storeEncryptedPasswordAndUserName(strEncryptedCombined, bUseAdmin, bFailIfUserDoesNotExist);
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::storeEncryptedPasswordAndUserName(const string& strEncryptedPW,
														  bool bUseAdmin,
														  bool bFailIfUserDoesNotExist,
														  bool bCreateTransactionGuard)
{
	string strUser = bUseAdmin ? gstrADMIN_USER : m_strFAMUserName;

	// Lock the mutex for this instance
	CSingleLock lock(&m_mutex, TRUE);

	// Create a pointer to a recordset
	_RecordsetPtr ipLoginSet(__uuidof(Recordset));
	ASSERT_RESOURCE_ALLOCATION("ELI15722", ipLoginSet != NULL);


	// Begin Transaction if needed
	auto_ptr<TransactionGuard> apTg;
	if (bCreateTransactionGuard)
	{
		apTg.reset(new TransactionGuard(getDBConnection()));
		ASSERT_RESOURCE_ALLOCATION("ELI29896", apTg.get() != NULL);
	}

	// Retrieve records from Login table for the admin or current user
	string strSQL = "SELECT * FROM LOGIN WHERE UserName = '" + strUser + "'";
	ipLoginSet->Open(strSQL.c_str(), _variant_t((IDispatch *)getDBConnection(), true), 
		adOpenDynamic, adLockPessimistic, adCmdText);

	// User not in DB if at the end of file
	if (ipLoginSet->adoEOF == VARIANT_TRUE)
	{
		if (bFailIfUserDoesNotExist)
		{
			UCLIDException ue("ELI30012", "The specified user was not found, cannot set password.");
			ue.addDebugInfo("User Name", strUser);
			throw ue;
		}

		// Insert a new record
		ipLoginSet->AddNew();

		// Set the UserName field
		setStringField(ipLoginSet->Fields, "UserName", strUser);
	}

	// Update the password field
	setStringField(ipLoginSet->Fields, "Password", strEncryptedPW);
	ipLoginSet->Update();

	// Commit the changes
	if (apTg.get() != NULL)
	{
		apTg->CommitTrans();
	}
}
//--------------------------------------------------------------------------------------------------
string CFileProcessingDB::getEncryptedString(const string strInput)
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
	getFAMPassword(pwBS);

	// Do the encryption
	ByteStream encryptedBS;
	EncryptionEngine ee;
	ee.encrypt(encryptedBS, bytes, pwBS);

	// Return the encrypted value
	return encryptedBS.asString();
}
//--------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr CFileProcessingDB::getThisAsCOMPtr()
{
	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipThis;
	ipThis = this;
	ASSERT_RESOURCE_ALLOCATION("ELI17015", ipThis != NULL);
	return ipThis;
}
//--------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileRecordPtr CFileProcessingDB::getFileRecordFromFields(
	const FieldsPtr& ipFields, bool bGetPriority)
{
	// Make sure the ipFields argument is not NULL
	ASSERT_ARGUMENT("ELI17028", ipFields != NULL);

	UCLID_FILEPROCESSINGLib::IFileRecordPtr ipFileRecord(CLSID_FileRecord);
	ASSERT_RESOURCE_ALLOCATION("ELI17027", ipFileRecord != NULL);
	
	// Set the file data from the fields collection (set ActionID to 0)
	ipFileRecord->SetFileData(getLongField(ipFields, "ID"), 0,
		getStringField(ipFields, "FileName").c_str(), getLongLongField(ipFields, "FileSize"),
		getLongField(ipFields, "Pages"), (UCLID_FILEPROCESSINGLib::EFilePriority)
		(bGetPriority ? getLongField(ipFields, "Priority") : 0));

	return ipFileRecord;
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::setFieldsFromFileRecord(const FieldsPtr& ipFields, 
		const UCLID_FILEPROCESSINGLib::IFileRecordPtr& ipFileRecord, bool bSetPriority)
{
	// Make sure the ipFields argument is not NULL
	ASSERT_ARGUMENT("ELI17031", ipFields != NULL);

	// Make sure the ipFileRecord object is not NULL
	ASSERT_ARGUMENT("ELI17032", ipFileRecord != NULL);
	
	// Get the file data
	long lFileID(-1), lActionID(-1), lNumPages(-1);
	LONGLONG llFileSize(-1);
	UCLID_FILEPROCESSINGLib::EFilePriority ePriority(
		(UCLID_FILEPROCESSINGLib::EFilePriority)kPriorityDefault);
	_bstr_t bstrFileName;
	ipFileRecord->GetFileData(&lFileID, &lActionID, bstrFileName.GetAddress(),
		&llFileSize, &lNumPages, &ePriority);

	// set the file name field
	setStringField(ipFields, "FileName", asString(bstrFileName));

	// Set the file Size
	setLongLongField(ipFields, "FileSize", llFileSize);

	// Set the number of pages
	setLongField(ipFields, "Pages", lNumPages);

	if (bSetPriority)
	{
		// Set the priority
		setLongField(ipFields, "Priority",
			(ePriority == kPriorityDefault ? glDEFAULT_FILE_PRIORITY : (long) ePriority));
	}
}
//--------------------------------------------------------------------------------------------------
bool  CFileProcessingDB::isPasswordValid(const string& strPassword, bool bUseAdmin)
{
	// Set the user to validate
	string  strUser = bUseAdmin ? gstrADMIN_USER : m_strFAMUserName;
	
	// Make combined string for comparison
	string strCombined = strUser + strPassword;
	
	// Get the stored password (if it exists)
	string strStoredEncryptedCombined;
	if (!getEncryptedPWFromDB(strStoredEncryptedCombined, bUseAdmin))
	{
		UCLIDException uex("ELI30013",
			"The specified user was not found, cannot authenticate password.");
		uex.addDebugInfo("User Name", strUser);
		throw uex;
	}

	// Check for no password
	if (strStoredEncryptedCombined.empty())
	{
		// if there is no stored password then strPassword should be emtpy
		return strPassword.empty();
	}

	// Get the password 'key' based on the 4 hex global variables
	ByteStream pwBS;
	getFAMPassword(pwBS);

	// Stream to hold the decrypted PW
	ByteStream decryptedPW;

	// Decrypt the stored, encrypted PW
	EncryptionEngine ee;
	ee.decrypt(decryptedPW, strStoredEncryptedCombined, pwBS);
	ByteStreamManipulator bsm(ByteStreamManipulator::kRead, decryptedPW);

	// Get the decrypted combined username and password from byte stream
	string strDecryptedCombined = "";
	bsm >> strDecryptedCombined;

	// Since the username is not case sensitive and the password is, will need to separate them

	// Extract the user from the decrypted username password combination using the expected
	// username length
	int iUserNameSize = strUser.length();

	// Successful login if decrypted user matches the expected and the decrypted password matches 
	// the expected password
	return (stringCSIS::sEqual(strUser, strDecryptedCombined.substr(0, iUserNameSize)) &&
		strDecryptedCombined.substr(iUserNameSize) == strCombined.substr(iUserNameSize));
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::initializeIfBlankDB()
{
	// Get the tables that exist in the database
	_ConnectionPtr ipConnection =  getDBConnection();
	_RecordsetPtr ipTables = ipConnection->OpenSchema(adSchemaTables);

	// Set blank flag to true
	bool bBlank = true;

	// Go thru all of the tables
	while (!asCppBool(ipTables->adoEOF))
	{
		// Get the Table Type
		string strType = getStringField(ipTables->Fields, "TABLE_TYPE");

		// Only need to look at the tables (no system tables or views)
		if (strType == "TABLE")
		{
			// There is at least one non system table
			bBlank = false;
			break;
		}

		// Get next table
		ipTables->MoveNext();
	}
	
	// If blank flag is set clear the database
	if (bBlank)
	{
		clear();
	}
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::getExpectedTables(std::vector<string>& vecTables)
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
bool CFileProcessingDB::isExtractTable(const string& strTable)
{
	// Get the list of tables
	vector<string> vecTables;
	getExpectedTables(vecTables);

	return vectorContainsElement(vecTables, strTable);
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::dropAllFKConstraints()
{
	// Get the tables for removing the foreign key constraints
	vector<string> vecTables;
	getExpectedTables(vecTables);

	// Drop the constraints
	dropFKContraintsOnTables(getDBConnection(), vecTables);
}
//--------------------------------------------------------------------------------------------------
long CFileProcessingDB::getMachineID(_ConnectionPtr ipConnection)
{
	// if the m_lMachineID == 0, get the id from the database
	if (m_lMachineID == 0)
	{
		CSingleLock lg(&m_mutex, TRUE);
		m_lMachineID = getKeyID(ipConnection, gstrMACHINE, "MachineName", m_strMachineName);
	}
	return m_lMachineID;
}
//--------------------------------------------------------------------------------------------------
long CFileProcessingDB::getFAMUserID(_ConnectionPtr ipConnection)
{
	// if the m_lFAMUserID == 0, get the id from the database
	if (m_lFAMUserID == 0)
	{
		CSingleLock lg(&m_mutex, TRUE);
		m_lFAMUserID = getKeyID(ipConnection, gstrFAM_USER, "UserName", m_strFAMUserName);
	}
	return m_lFAMUserID;
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::loadDBInfoSettings(_ConnectionPtr ipConnection)
{
	INIT_EXCEPTION_AND_TRACING("MLI00019");

	try
	{
		// Initialize settings to default values
		m_iDBSchemaVersion = 0;
		m_iCommandTimeout = glDEFAULT_COMMAND_TIMEOUT;
		m_bUpdateQueueEventTable = true;
		m_bUpdateFASTTable = true;
		m_iNumberOfRetries = giDEFAULT_RETRY_COUNT;
		m_dRetryTimeout = gdDEFAULT_RETRY_TIMEOUT;

		// Only load the settings if the table exists
		if (doesTableExist(getDBConnection(), "DBInfo"))
		{
			// Create a pointer to a recordset
			_RecordsetPtr ipDBInfoSet(__uuidof(Recordset));
			ASSERT_RESOURCE_ALLOCATION("ELI18171", ipDBInfoSet != NULL);

			_lastCodePos = "10";

			ipDBInfoSet->Open("DBInfo", _variant_t((IDispatch *)ipConnection, true), adOpenDynamic, 
				adLockOptimistic, adCmdTable); 

			_lastCodePos = "20";

			// Loop through all of the records in the DBInfo table
			while (!asCppBool(ipDBInfoSet->adoEOF))
			{
				FieldsPtr ipFields = ipDBInfoSet->Fields;
				ASSERT_RESOURCE_ALLOCATION("ELI18172", ipFields != NULL);

				_lastCodePos = "30";

				// Check all of the fields
				int nFields = ipFields->Count;
				for (long l = 0; l < nFields; l++)
				{
					// Setup the variant with the current loop count
					variant_t vt = l;

					_lastCodePos = "40";

					// Get field with that index
					FieldPtr ipField = ipFields->Item[vt];

					_lastCodePos = "50";

					// If the "Name" field exist
					if (ipField->Name == _bstr_t("Name"))
					{
						_lastCodePos = "60";

						// Get the Setting name
						string strValue = getStringField(ipFields, "Name");
						if (strValue == gstrFAMDB_SCHEMA_VERSION)
						{
							_lastCodePos = "70";

							// Get the schema version
							m_iDBSchemaVersion = asLong(getStringField(ipFields, "Value"));
						}
						else if (strValue == gstrCOMMAND_TIMEOUT)
						{
							_lastCodePos = "80";

							// Get the commmand timeout
							m_iCommandTimeout =  asLong(getStringField(ipFields, "Value"));
						}
						else if (strValue == gstrUPDATE_QUEUE_EVENT_TABLE)
						{
							_lastCodePos = "90";

							// Get the Update Queue flag
							m_bUpdateQueueEventTable = getStringField(ipFields, "Value") == "1";
						}
						else if (strValue == gstrUPDATE_FAST_TABLE)
						{
							_lastCodePos = "100";

							// get the Update FAST flag
							m_bUpdateFASTTable = getStringField(ipFields, "Value") == "1";
						}
						else if (strValue == gstrNUMBER_CONNECTION_RETRIES)
						{
							_lastCodePos = "150";

							// Get the Connection retry count
							m_iNumberOfRetries = asLong(getStringField(ipFields, "Value"));
						}
						else if (strValue == gstrCONNECTION_RETRY_TIMEOUT)
						{
							_lastCodePos = "160";

							// Get the connection retry timeout
							m_dRetryTimeout =  asDouble(getStringField(ipFields, "Value"));
						}
						else if (strValue == gstrAUTO_DELETE_FILE_ACTION_COMMENT)
						{
							_lastCodePos = "170";

							m_bAutoDeleteFileActionComment = getStringField(ipFields, "Value") == "1";
						}
						else if (strValue == gstrAUTO_REVERT_LOCKED_FILES)
						{
							_lastCodePos = "180";

							m_bAutoRevertLockedFiles = getStringField(ipFields, "Value") == "1";
						}
						else if (strValue == gstrAUTO_REVERT_TIME_OUT_IN_MINUTES)
						{
							_lastCodePos = "190";

							m_nAutoRevertTimeOutInMinutes =  asLong(getStringField(ipFields, "Value"));

							// if less that a minimum value this should be reset to the minimum value
							if (m_nAutoRevertTimeOutInMinutes < gnMINIMUM_AUTO_REVERT_TIME_OUT_IN_MINUTES)
							{
								try
								{
									string strNewValue = asString(gnMINIMUM_AUTO_REVERT_TIME_OUT_IN_MINUTES);
									// Log application trace exception 
									UCLIDException ue("ELI29826", "Application trace: AutoRevertTimeOutInMinutes changed to " + 
										strNewValue + " minutes.");
									ue.addDebugInfo("Old value", m_nAutoRevertTimeOutInMinutes);
									ue.addDebugInfo("New value", gnMINIMUM_AUTO_REVERT_TIME_OUT_IN_MINUTES);
									ue.log();

									// Not sure if this is actually safe here.
									// if updating this field fails it will be thrown out and rolled back in 
									// a transaction in the outer scope.  A transaction cannot be created for
									// this here because there will most likely be a transaction in the
									// outer scope.
									setStringField(ipFields, "Value", strNewValue);
									ipDBInfoSet->Update();
								}
								CATCH_AND_LOG_ALL_EXCEPTIONS("ELI29832");

								m_nAutoRevertTimeOutInMinutes = gnMINIMUM_AUTO_REVERT_TIME_OUT_IN_MINUTES;
							}
						}
						else if (strValue == gstrAUTO_REVERT_NOTIFY_EMAIL_LIST)
						{
							_lastCodePos = "200";

							m_strAutoRevertNotifyEmailList = getStringField(ipFields, "Value");
						}
					}
					else if (ipField->Name == _bstr_t("FAMDBSchemaVersion"))
					{
						_lastCodePos = "110";

						// Get the schema version for the previous Database version
						m_iDBSchemaVersion = getLongField(ipFields, "FAMDBSchemaVersion");
					}
					else if (ipField->Name == _bstr_t("FPMDBSchemaVersion"))
					{
						_lastCodePos = "120";

						// This is for an even older schema version
						m_iDBSchemaVersion = getLongField(ipFields, "FPMDBSchemaVersion");
					}
				}

				_lastCodePos = "130";

				// Move the next record
				ipDBInfoSet->MoveNext();

				_lastCodePos = "140";
			}
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI18146");
}
//--------------------------------------------------------------------------------------------------
HWND CFileProcessingDB::getAppMainWndHandle()
{
	// try to use this application's main window as the parent for the messagebox below
	// get the main window
	CWnd *pWnd = AfxGetMainWnd();//pApp->GetMainWnd();
	if (pWnd)
	{
		return pWnd->m_hWnd;
	}
	return NULL;
}
//--------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CFileProcessingDB::getLicensedProductSpecificMgrs()
{
	// Create the category manager instance
	ICategoryManagerPtr ipCategoryMgr(CLSID_CategoryManager);
	ASSERT_RESOURCE_ALLOCATION("ELI18948", ipCategoryMgr != NULL);

	// Get map of licensed prog ids that belong to the product specific db managers category
	IStrToStrMapPtr ipProductSpecMgrProgIDs = 
		ipCategoryMgr->GetDescriptionToProgIDMap1(FP_FAM_PRODUCT_SPECIFIC_DB_MGRS.c_str());
	ASSERT_RESOURCE_ALLOCATION("ELI18947", ipProductSpecMgrProgIDs != NULL);

	// Create a vector to contain instances of DB managers to return
	IIUnknownVectorPtr ipProdSpecMgrs(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI18949", ipProdSpecMgrs != NULL);

	// Get the number of licensed product specific db managers
	long nSize = ipProductSpecMgrProgIDs->Size;
	for (long n = 0; n < nSize; n++)
	{
		// get the prog id
		CComBSTR bstrKey, bstrValue;
		ipProductSpecMgrProgIDs->GetKeyValue(n, &bstrKey, &bstrValue);
		
		// Create the object
		ICategorizedComponentPtr ipComponent(asString(bstrValue).c_str());
		if (ipComponent == NULL)
		{
			UCLIDException ue("ELI18950", "Unable to create Product Specific DB Manager!");
			ue.addDebugInfo("ObjectName", asString(bstrValue));
			throw ue;
		}
		
		// Put instance on the return vector
		ipProdSpecMgrs->PushBack(ipComponent);
	}

	// Return the vector of product specific managers
	return ipProdSpecMgrs;
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::removeProductSpecificDB()
{
	try
	{
		// Get vector of all license product specific managers
		IIUnknownVectorPtr ipProdSpecMgrs = getLicensedProductSpecificMgrs();
		ASSERT_RESOURCE_ALLOCATION("ELI18951", ipProdSpecMgrs != NULL);

		// Loop through all of the objects and call the RemoveProductSpecificSchema 
		long nSize = ipProdSpecMgrs->Size();
		for (long n = 0; n < nSize; n++)
		{
			UCLID_FILEPROCESSINGLib::IProductSpecificDBMgrPtr ipMgr = ipProdSpecMgrs->At(n);
			ASSERT_RESOURCE_ALLOCATION("ELI18952", ipMgr != NULL);

			// Remove the schema for the product specific manager
			ipMgr->RemoveProductSpecificSchema(getThisAsCOMPtr());
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27610")
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::addProductSpecificDB()
{
	try
	{
		// Get vector of all license product specific managers
		IIUnknownVectorPtr ipProdSpecMgrs = getLicensedProductSpecificMgrs();
		ASSERT_RESOURCE_ALLOCATION("ELI19790", ipProdSpecMgrs != NULL);

		// Loop through all of the objects and call the AddProductSpecificSchema
		long nSize = ipProdSpecMgrs->Size();
		for (long n = 0; n < nSize; n++)
		{
			UCLID_FILEPROCESSINGLib::IProductSpecificDBMgrPtr ipMgr = ipProdSpecMgrs->At(n);
			ASSERT_RESOURCE_ALLOCATION("ELI19791", ipMgr != NULL);

			// Add the schema from the product specific db manager
			ipMgr->AddProductSpecificSchema(getThisAsCOMPtr());
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27608")
}
//--------------------------------------------------------------------------------------------------
bool CFileProcessingDB::isConnectionAlive(_ConnectionPtr ipConnection)
{
	try
	{
		if (ipConnection != NULL)
		{
			getSQLServerDateTime(ipConnection);
			return true;
		}
	}
	catch(...){};
	
	return false;
}
//--------------------------------------------------------------------------------------------------
bool CFileProcessingDB::reConnectDatabase()
{
	StopWatch sw;
	sw.start();
	bool bNoMoreRetries = false;
	DWORD dwThreadID = GetCurrentThreadId();
	do
	{
		try
		{
			try
			{
				// Reset all database connections since if one is in a bad state all will be.				
				resetDBConnection();

				// Exception logged to indicate the retry was successful.
				UCLIDException ueConnected("ELI23614", "Connection retry successful.");
				ueConnected.log();

				return true;
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI23503");
		}
		catch(UCLIDException ue)
		{
			// Check to see if the timeout has been reached.
			if (sw.getElapsedTime() > m_dRetryTimeout)
			{
				// Set the flag for no more retries to exit the loop.
				bNoMoreRetries = true;

				// Create exception to indicate retry timed out
				UCLIDException uex("ELI23612", "Database connection retry timed out!", ue);

				// Log the caught exception.
				uex.log();
			}
			else
			{
				// Sleep to reduce the number of retries/second
				Sleep(100);
			}
		}
	}
	while (!bNoMoreRetries);

	return false;
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::addSkipFileRecord(const _ConnectionPtr &ipConnection,
										  long nFileID, long nActionID)
{
	try
	{
		string strSkippedSQL = "SELECT * FROM SkippedFile WHERE FileID = "
			+ asString(nFileID) + " AND ActionID = " + asString(nActionID);

		_RecordsetPtr ipSkippedSet(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI26884", ipSkippedSet != NULL);

		ipSkippedSet->Open(strSkippedSQL.c_str(), _variant_t((IDispatch*)ipConnection, true),
			adOpenDynamic, adLockOptimistic, adCmdText);

		// Ensure no records returned
		if (ipSkippedSet->adoEOF == VARIANT_FALSE)
		{
			UCLIDException uex("ELI26806", "File has already been skipped for this action!");
			uex.addDebugInfo("Action ID", nActionID);
			uex.addDebugInfo("File ID", nFileID);
			throw uex;
		}
		else
		{
			string strUserName = getCurrentUserName();

			// Add a new row
			ipSkippedSet->AddNew();

			// Get the fields pointer
			FieldsPtr ipFields = ipSkippedSet->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI26807", ipFields != NULL);

			// Set the fields from the provided data
			setStringField(ipFields, "UserName", strUserName);
			setLongField(ipFields, "FileID", nFileID);
			setLongField(ipFields, "ActionID", nActionID);
			setLongField(ipFields, "UPIID", m_nUPIID);

			// Update the row
			ipSkippedSet->Update();
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26804");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::removeSkipFileRecord(const _ConnectionPtr &ipConnection,
											 long nFileID, long nActionID)
{
	try
	{
		string strSkippedSQL = "SELECT * FROM SkippedFile WHERE FileID = "
			+ asString(nFileID) + " AND ActionID = " + asString(nActionID);

		_RecordsetPtr ipSkippedSet(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI26885", ipSkippedSet != NULL);

		ipSkippedSet->Open(strSkippedSQL.c_str(), _variant_t((IDispatch*)ipConnection, true),
			adOpenDynamic, adLockOptimistic, adCmdText);

		// Only delete the record if it is found
		if (ipSkippedSet->BOF == VARIANT_FALSE)
		{
			// Delete the row
			ipSkippedSet->Delete(adAffectCurrent);
			ipSkippedSet->Update();
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26805");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::resetDBConnection()
{
	INIT_EXCEPTION_AND_TRACING("MLI03268");
	try
	{
		_lastCodePos = "10";

		CSingleLock lock(&m_mutex, TRUE);
		
		// Close all the DB connections and clear the map [LRCAU# 5659]
		closeAllDBConnections();

		_lastCodePos = "40";

		// If there is a non empty server and database name get a connection and validate
		if (!m_strDatabaseServer.empty() && !m_strDatabaseName.empty())
		{
			// This will create a new connection for this thread and initialize the schema
			getDBConnection();

			_lastCodePos = "50";

			// Validate the schema
			validateDBSchemaVersion();
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26869");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::closeAllDBConnections()
{
	INIT_EXCEPTION_AND_TRACING("MLI03275");
	try
	{
		CSingleLock lock(&m_mutex, TRUE);
		
		_lastCodePos = "20";
		
		// Initilize count for MLI Code iteration count
		long nCount = 0;
		map<DWORD, _ConnectionPtr>::iterator it;
		for (it = m_mapThreadIDtoDBConnections.begin(); it != m_mapThreadIDtoDBConnections.end(); it++)
		{

			// Do the close within a try catch because an exception on the close could just mean the connection is in a bad state and
			// recreating and opening will put it in a good state
			try
			{
				_ConnectionPtr ipDBConnection = it->second;
				_lastCodePos = "25-" + asString(nCount);

				// This will close the existing connection if not already closed
				if (ipDBConnection != NULL && ipDBConnection->State != adStateClosed)
				{
					_lastCodePos = "30";

					ipDBConnection->Close();
				}
			}
			CATCH_AND_LOG_ALL_EXCEPTIONS("ELI29884")
		}

		// Clear all of the connections in all of the threads
		m_mapThreadIDtoDBConnections.clear();
		_lastCodePos = "35";

		// Reset the Current connection status to not connected
		m_strCurrentConnectionStatus = gstrNOT_CONNECTED;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29885");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::clear(bool retainUserValues)
{
	try
	{
		CSingleLock lock(&m_mutex, TRUE);

		// Begin a transaction
		TransactionGuard tg(getDBConnection());

		// Get a list of the action names to preserve
		vector<string> vecActionNames;
		if (retainUserValues)
		{
			// Read all actions from the DB
			IStrToStrMapPtr ipMapActions = getActions(getDBConnection());
			ASSERT_RESOURCE_ALLOCATION("ELI25184", ipMapActions != NULL);
			IVariantVectorPtr ipActions = ipMapActions->GetKeys();
			ASSERT_RESOURCE_ALLOCATION("ELI25185", ipActions != NULL);

			// Iterate over the actions
			long lSize = ipActions->Size;
			vecActionNames.reserve(lSize);
			for (int i = 0; i < lSize; i++)
			{
				// Get each action name and add it to the vector
				_variant_t action = ipActions->Item[i];
				vecActionNames.push_back( asString(action.bstrVal) );
			}
		}

		string strAdminPW;

		// Only get the admin password if we are not retaining user values and the
		// Login table already exists [LRCAU #5780]
		if (!retainUserValues && doesTableExist(getDBConnection(), "Login"))
		{
			// Need to store the admin login and add it back after re-adding the table
			getEncryptedPWFromDB(strAdminPW, true);
		}

		// Drop the tables
		dropTables(retainUserValues);

		// Add the tables back
		addTables(!retainUserValues);

		// Setup the tables that require initial values
		initializeTableValues(!retainUserValues);

		// Add any retained actions
		for (unsigned int i = 0; i < vecActionNames.size(); i++)
		{
			defineNewAction(getDBConnection(), vecActionNames[i]);
		}

		// Add the admin user back with admin PW
		if (!strAdminPW.empty())
		{
			storeEncryptedPasswordAndUserName(strAdminPW, true, false);
		}

		tg.CommitTrans();

		// Add the Product specific db after the base tables have been committed
		addProductSpecificDB();

		// Reset the database connection
		resetDBConnection();

		// Shrink the database
		executeCmdQuery(getDBConnection(), gstrSHRINK_DATABASE);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26870");
}
//-------------------------------------------------------------------------------------------------
IStrToStrMapPtr CFileProcessingDB::getActions(_ConnectionPtr ipConnection)
{
	try
	{
		// Create a pointer to a recordset
		_RecordsetPtr ipActionSet(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI13530", ipActionSet != NULL);

		// Open the Action table
		ipActionSet->Open("Action", _variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
			adLockReadOnly, adCmdTableDirect);

		// Create StrToStrMap to return the list of actions
		IStrToStrMapPtr ipActions(CLSID_StrToStrMap);
		ASSERT_RESOURCE_ALLOCATION("ELI29687", ipActions != NULL);

		// Step through all records
		while (ipActionSet->adoEOF == VARIANT_FALSE)
		{
			// Get the fields from the action set
			FieldsPtr ipFields = ipActionSet->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI26871", ipFields != NULL);

			// get the action name
			string strActionName = getStringField(ipFields, "ASCName");

			// get the action ID
			long lID = getLongField(ipFields, "ID");
			string strID = asString(lID);

			// Put the values in the StrToStrMap
			ipActions->Set(strActionName.c_str(), strID.c_str());

			// Move to the next record in the table
			ipActionSet->MoveNext();
		}

		return ipActions;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29686");
}
//-------------------------------------------------------------------------------------------------
long CFileProcessingDB::defineNewAction(_ConnectionPtr ipConnection, const string& strActionName)
{
	try
	{
		// Create a pointer to a recordset containing the action
		_RecordsetPtr ipActionSet = getActionSet(ipConnection, strActionName);
		ASSERT_RESOURCE_ALLOCATION("ELI13517", ipActionSet != NULL);

		// Check to see if action exists
		if (ipActionSet->adoEOF == VARIANT_FALSE)
		{
			// Build error string (P13 #3931)
			CString zText;
			zText.Format("The action '%s' already exists, and therefore cannot be added again.", 
				strActionName.c_str());
			UCLIDException ue("ELI13946", LPCTSTR(zText));
			throw ue;
		}

		// Create a new action and return its ID
		return addActionToRecordset(ipConnection, ipActionSet, strActionName);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29681");
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::getFilesSkippedByUser(vector<long>& rvecSkippedFileIDs, long nActionID,
													  string strUserName,
													  const _ConnectionPtr& ipConnection)
{
	try
	{
		// Clear the vector
		rvecSkippedFileIDs.clear();

		string strSQL = "SELECT [FileID] FROM [SkippedFile] WHERE [ActionID] = "
			+ asString(nActionID);
		if (!strUserName.empty())
		{
			// Escape any single quotes
			replaceVariable(strUserName, "'", "''");
			strSQL += " AND [UserName] = '" + strUserName + "'";
		}

		// Make sure the DB Schema is the expected version
		validateDBSchemaVersion();

		// Recordset to contain the files to process
		_RecordsetPtr ipFileIDSet(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI26909", ipFileIDSet != NULL);

		// get the recordset with skipped file ID's
		ipFileIDSet->Open(strSQL.c_str(), _variant_t((IDispatch *)ipConnection, true),
			adOpenForwardOnly, adLockReadOnly, adCmdText);

		// Loop through the result set adding the file ID's to the vector
		while (ipFileIDSet->adoEOF == VARIANT_FALSE)
		{
			// Get the file ID and add it to the vector
			long nFileID = getLongField(ipFileIDSet->Fields, "FileID");
			rvecSkippedFileIDs.push_back(nFileID);

			// Move to the next record
			ipFileIDSet->MoveNext();
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26907");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::clearFileActionComment(const _ConnectionPtr& ipConnection, long nFileID,
											   long nActionID)
{
	try
	{
		// Query for deleting the comment
		string strCommentSQL = "DELETE FROM FileActionComment ";
		string strWhere = "";
		if (nFileID != -1)
		{
			strWhere += "FileID = " + asString(nFileID);
		}

		// If nActionID == -1 then delete all comments for the FileID
		if (nActionID != -1)
		{
			if (nFileID != -1)
			{
				strWhere += " AND ";
			}
			strWhere += "ActionID = " + asString(nActionID);
		}

		if (!strWhere.empty())
		{
			strCommentSQL += "WHERE " + strWhere;
		}

		// Perform the deletion
		executeCmdQuery(ipConnection, strCommentSQL);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27109");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::validateTagName(const string& strTagName)
{
	try
	{
		// Tag name is invalid if either:
		// 1. Empty
		// 2. Longer than 100 characters
		// 3. Does not match the TAG_REGULAR_EXPRESSION
		if (strTagName.empty() || strTagName.length() > 100
			|| getParser()->StringMatchesPattern(strTagName.c_str()) == VARIANT_FALSE)
		{
			UCLIDException ue("ELI27383", "Invalid tag name!");
			ue.addDebugInfo("Tag", strTagName);
			ue.addDebugInfo("Tag Length", strTagName.length());
			ue.addDebugInfo("Valid Tag Name", gstrTAG_REGULAR_EXPRESSION);
			throw ue;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27384");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::validateFileID(const _ConnectionPtr& ipConnection, long nFileID)
{
	try
	{
		string strQuery = "SELECT [FileName] FROM [" + gstrFAM_FILE + "] WHERE [ID] = "
			+ asString(nFileID);

		_RecordsetPtr ipRecord(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI27385", ipRecord != NULL);

		ipRecord->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
			adLockOptimistic, adCmdText);

		if (ipRecord->adoEOF == VARIANT_TRUE)
		{
			UCLIDException ue("ELI27386", "Invalid File ID: File ID does not exist in database!");
			ue.addDebugInfo("File ID", nFileID);
			ue.addDebugInfo("Database Name", m_strDatabaseName);
			ue.addDebugInfo("Database Server", m_strDatabaseServer);
			throw ue;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27387");
}
//--------------------------------------------------------------------------------------------------
string CFileProcessingDB::getDBInfoSetting(const _ConnectionPtr& ipConnection,
										   const string& strSettingName)
{
	try
	{
		// Create a pointer to a recordset
		_RecordsetPtr ipDBInfoSet(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI19793", ipDBInfoSet != NULL);

		// Setup Setting Query
		string strSQL = gstrDBINFO_SETTING_QUERY;
		replaceVariable(strSQL, gstrSETTING_NAME, strSettingName);
		
		// Open the record set using the Setting Query		
		ipDBInfoSet->Open(strSQL.c_str(), _variant_t((IDispatch *)ipConnection, true),
			adOpenForwardOnly, adLockReadOnly, adCmdText); 

		// Check if any data returned
		if (ipDBInfoSet->adoEOF == VARIANT_FALSE)
		{
			// Return the setting value
			return getStringField(ipDBInfoSet->Fields, "Value");
		}
		else
		{
			UCLIDException ue("ELI18940", "DBInfo setting does not exist!");
			ue.addDebugInfo("Setting", strSettingName);
			throw  ue;
		}

	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27388");
}
//--------------------------------------------------------------------------------------------------
long CFileProcessingDB::getTagID(const _ConnectionPtr &ipConnection, string &rstrTagName)
{
	try
	{
		return getKeyID(ipConnection, gstrFAM_TAG, "TagName", rstrTagName, false);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27389");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::revertLockedFilesToPreviousState(const _ConnectionPtr& ipConnection, 
														 long nUPIID, const string& strFASTComment, 
														 UCLIDException *pUE)
{
	try
	{
		// Setup Setting Query
		string strSQL = "SELECT FileID, ActionID, UPI, StatusBeforeLock, ASCName " 
			" FROM LockedFile INNER JOIN ProcessingFAM ON LockedFile.UPIID = ProcessingFAM.ID"
			" INNER JOIN Action ON LockedFile.ActionID = Action.ID"
			" WHERE LockedFile.UPIID = " + asString(nUPIID);

		// Open a recordset that has the action names that need to have files reset
		_RecordsetPtr ipFileSet(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI27737", ipFileSet != NULL);

		ipFileSet->Open(strSQL.c_str(), _variant_t((IDispatch *)ipConnection, true), 
			adOpenForwardOnly, adLockReadOnly, adCmdText);

		// Map to track the number of files for each action that are being reset
		map<string, map<string, int>> map_StatusCounts;
		map_StatusCounts.clear();

		// Step through all of the file records in the LockedFile table for the dead UPI
		while(ipFileSet->adoEOF == VARIANT_FALSE)
		{
			FieldsPtr ipFields = ipFileSet->Fields;

			// Get the action name and previous status
			string strActionName = getStringField(ipFields, "ASCName");
			string strRevertToStatus = getStringField(ipFields, "StatusBeforeLock");

			// Add to the count
			map_StatusCounts[strActionName][strRevertToStatus] = 
				map_StatusCounts[strActionName][strRevertToStatus] + 1;

			setFileActionState(ipConnection, getLongField(ipFields, "FileID"), 
				strActionName, strRevertToStatus, 
				"", getLongField(ipFields, "ActionID"), false, false,
				strFASTComment);

			ipFileSet->MoveNext();
		}

		// Delete the UPI record from the ProcessingFAM table
		string strQuery = "DELETE FROM ProcessingFAM WHERE ID = " + asString(nUPIID); 
		executeCmdQuery(getDBConnection(), strQuery);

		// Set up the logged exception if it is not null
		if (pUE != NULL)
		{
			bool bAtLeastOneReset = false;
			string strEmailMessage = "";

			map<string, map<string,int>>::iterator itMap = map_StatusCounts.begin();
			for(; itMap != map_StatusCounts.end(); itMap++)
			{
				map<string,int>::iterator itCounts = itMap->second.begin();
				for (; itCounts != itMap->second.end(); itCounts++)
				{
					string strAction = itMap->first;
					string strDebugInfo = "CountOf_" + strAction + "_RevertedTo_" + itCounts->first;
					if (!bAtLeastOneReset)
					{
						// If this is the first item added to the message, add the FAST comment
						strEmailMessage = strFASTComment;
					}
					pUE->addDebugInfo(strDebugInfo, itCounts->second);
					strEmailMessage += "\r\n    " + strDebugInfo + ": " + asString(itCounts->second);
					bAtLeastOneReset = true;
				}
			}
			
			// Only log the reset exception if one or more files were reset
			if (bAtLeastOneReset)
			{
				pUE->log();
				
				// Send the email message if list is setup and message to send
				if (!m_strAutoRevertNotifyEmailList.empty() && !strEmailMessage.empty())
				{
					emailMessage(strEmailMessage);
				}
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27738");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::pingDB()
{
	// if m_bFAMRegistered is false there is nothing to do
	if ( !m_bFAMRegistered )
	{
		return;
	}

	// Lock mutex so that a call from the connection retry functionality will not collide
	CSingleLock lock(&m_mutex, TRUE);

	// Always call the getKeyID so that if the record was removed by another
	// instance because this instance lost the DB for a while
	long nUPIID = getKeyID(getDBConnection(), "ProcessingFAM", "UPI", m_strUPI);
	if (nUPIID != m_nUPIID)
	{
		// The only time m_nUPIID is 0 is when there was no previous instance 
		if (m_nUPIID > 0 )
		{
			UCLIDException ue("ELI27785", "Application Trace: UPIID has changed.");
			ue.addDebugInfo("Expected", m_nUPIID);
			ue.addDebugInfo("New UPIID", nUPIID);
			ue.log();
		}

		// Update the UPIID
		m_nUPIID = nUPIID;
	}
	else
	{
		// Update the ping record. 
		executeCmdQuery(getDBConnection(), 
			"UPDATE ProcessingFAM SET LastPingTime=GETDATE() WHERE ID = " + asString(nUPIID));
	}
}
//--------------------------------------------------------------------------------------------------
UINT CFileProcessingDB::maintainLastPingTimeForRevert(void *pData)
{
	try
	{
		CoInitializeEx(NULL, COINIT_MULTITHREADED);

		CFileProcessingDB *pDB = static_cast<CFileProcessingDB *>(pData);
		ASSERT_ARGUMENT("ELI27746", pDB != NULL);

		// Enclose so that the exited event can always be signaled if it can be.
		try
		{
			while (pDB->m_eventStopPingThread.wait(gnPING_TIMEOUT) == WAIT_TIMEOUT)
			{
				try
				{
					pDB->pingDB();
				}
				CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27747");
			}
		}
		CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27858");

		// Signal that the thread has exited
		pDB->m_eventPingThreadExited.signal();

		CoUninitialize();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27745");

	return 0;
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::revertTimedOutProcessingFAMs(const _ConnectionPtr& ipConnection)
{
	// Make sure the LastPingTime is up to date to keep before reverting so that the
	// current session doesn't get auto reverted
	pingDB();

	// Query to show the elapsed time since last ping for all ProcessingFAM records
	string strElapsedSQL = "SELECT [ID], DATEDIFF(minute,[LastPingTime],GetDate()) as Elapsed "
			"FROM [ProcessingFAM]";

	_RecordsetPtr ipFileSet(__uuidof(Recordset));
	ASSERT_RESOURCE_ALLOCATION("ELI27813", ipFileSet != NULL);

	ipFileSet->Open(strElapsedSQL.c_str(), _variant_t((IDispatch *)ipConnection, true), 
		adOpenForwardOnly, adLockReadOnly, adCmdText);

	// Step through all of the ProcessingFAM records to find dead FAM's
	while(ipFileSet->adoEOF == VARIANT_FALSE)
	{
		FieldsPtr ipFields = ipFileSet->Fields;
		
		// Get the Elapsed time since the last ping
		long nElapsed = getLongField(ipFields,"Elapsed");
		
		// Check for a dead FAM
		if (nElapsed > m_nAutoRevertTimeOutInMinutes)
		{
			long nUPIID = getLongField(ipFields, "ID");
			long nMinutesSinceLastPing = getLongField(ipFields, "Elapsed");

			UCLIDException ue("ELI27814", "Application Trace: Files were reverted to original status.");
			ue.addDebugInfo("Minutes files locked", nMinutesSinceLastPing);

			// Build the comment for the FAST table
			string strRevertComment = "Auto reverted after " + asString(nMinutesSinceLastPing) + " minutes.";

			// Revert the files for this dead FAM to there previous status
			revertLockedFilesToPreviousState(ipConnection, nUPIID, strRevertComment, &ue);
		}
		// move to next Processing FAM record
		ipFileSet->MoveNext();
	}
}
//--------------------------------------------------------------------------------------------------
bool CFileProcessingDB::isInputEventTrackingEnabled(const _ConnectionPtr& ipConnection)
{
	try
	{
		// Check the DB setting (only check once per session)
		static bool bInputTrackingEnabled =
			getDBInfoSetting(ipConnection, gstrENABLE_INPUT_EVENT_TRACKING) == "1";

		return bInputTrackingEnabled;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28967");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::deleteOldInputEvents(const _ConnectionPtr& ipConnection)
{
	static CTime lastTime(1970, 1,1, 0, 0, 0);

	try
	{
		// Get the current time and compute the time span between the current and the last
		// delete operation
		CTime currentTime = CTime::GetCurrentTime();
		CTimeSpan span = currentTime - lastTime;

		// Only execute the query if it hasn't been run in the past day
		if (span.GetDays() > 1)
		{
			// Set the last time to the current time
			lastTime = currentTime;

			// Execute the delete old input event records query
			executeCmdQuery(ipConnection, gstrDELETE_OLD_INPUT_EVENT_RECORDS);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28941");
}
//--------------------------------------------------------------------------------------------------
bool CFileProcessingDB::isMachineInListOfMachinesToSkipUserAuthentication(
	const _ConnectionPtr& ipConnection)
{
	try
	{
		// Get the list of machine names
		string strMachines = getDBInfoSetting(ipConnection, gstrSKIP_AUTHENTICATION_ON_MACHINES);

		// Tokenize by either comma, semicolon, or pipe
		vector<string> vecTokens;
		StringTokenizer::sGetTokens(strMachines, ",;|", vecTokens, true);

		string strMachineName = m_strMachineName;
		makeLowerCase(strMachineName);

		// Look through the list of strings for the machine name
		for(vector<string>::iterator it = vecTokens.begin(); it != vecTokens.end(); it++)
		{
			// Trim whitespace and make the string lower case
			string strTemp = trim(*it, " \t", " \t");
			makeLowerCase(strTemp);

			// Check if it matches the machine name
			if (strTemp == strMachineName)
			{
				return true;
			}
		}

		// No match found, return false
		return false;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29188");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::validateNewActionName(const string& strActionName)
{
	// Check if the action name is valid
	if (strActionName.length() > 50 || !isValidIdentifier(strActionName))
	{
		// Throw an exception
		UCLIDException ue("ELI29706", "Specified action name is invalid.");
		ue.addDebugInfo("Action Name", strActionName);
		ue.addDebugInfo("Valid Pattern", "[_a-zA-Z][_a-zA-Z0-9]*" );
		ue.addDebugInfo("Action Name Length", strActionName.length());
		ue.addDebugInfo("Maximum Length", "50");
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
IRegularExprParserPtr CFileProcessingDB::getParser()
{
	try
	{
		IRegularExprParserPtr ipParser = m_ipMiscUtils->GetNewRegExpParserInstance("");
		ASSERT_RESOURCE_ALLOCATION("ELI27382", ipParser != NULL);

		// Set the pattern
		ipParser->Pattern = gstrTAG_REGULAR_EXPRESSION.c_str();

		return ipParser;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29458");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::emailMessage(const string & strMessage)
{
	AfxBeginThread(emailMessageThread, 
		new EmailThreadData(m_strAutoRevertNotifyEmailList, strMessage)); 
}
//--------------------------------------------------------------------------------------------------
UINT CFileProcessingDB::emailMessageThread(void *pData)
{
	try
	{
		// Put the emailThreadData pointer passed in to an auto pointer.
		auto_ptr<EmailThreadData> apEmailThreadData(static_cast<EmailThreadData *>(pData));
		ASSERT_RESOURCE_ALLOCATION("ELI27999", apEmailThreadData.get() != NULL);

		CoInitializeEx(NULL, COINIT_MULTITHREADED);

		try
		{
			try
			{
				// Email Settings
				IEmailSettingsPtr ipEmailSettings(CLSID_EmailSettings);
				ASSERT_RESOURCE_ALLOCATION("ELI27962", ipEmailSettings != NULL);

				IObjectSettingsPtr ipSettings = ipEmailSettings;
				ASSERT_RESOURCE_ALLOCATION("ELI27963", ipSettings != NULL);

				ipSettings->LoadFromRegistry(gstrEMAIL_REG_PATH.c_str());

				// If there is no SMTP server set the UI needs to be displayed to enter that information
				string strServer = ipEmailSettings->SMTPServer;
				if (strServer.empty())
				{
					UCLIDException ue("ELI27969", 
						"Email settings have not been specified. Unable to send auto-revert message.");
					ue.log();
					return 0;
				}

				// Email Message 
				IESMessagePtr ipMessage(CLSID_ESMessage);
				ASSERT_RESOURCE_ALLOCATION("ELI27964", ipMessage != NULL);

				ipMessage->EmailSettings = ipEmailSettings;

				vector<string> vecRecipients;
				StringTokenizer::sGetTokens(apEmailThreadData->m_strRecipients, ",;", vecRecipients, true);

				IVariantVectorPtr ipRecipients(CLSID_VariantVector);
				ASSERT_RESOURCE_ALLOCATION("ELI27966", ipRecipients != NULL);

				for (unsigned int i = 0; i < vecRecipients.size(); i++)
				{
					ipRecipients->PushBack(vecRecipients[i].c_str());
				}

				// Add Recipients list to email message
				ipMessage->Recipients = ipRecipients;

				ipMessage->Subject = "Files were reverted to previous status."; 

				ipMessage->BodyText = apEmailThreadData->m_strMessage.c_str();

				// Send  the message
				ipMessage->Send();
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27971")
		}
		catch(UCLIDException ue)
		{
			UCLIDException uex("ELI27970", "Unable to send email.", ue);
			uex.addDebugInfo("Recipients", apEmailThreadData->m_strRecipients);
			uex.addDebugInfo("Message", apEmailThreadData->m_strMessage);
			uex.log();
		}		

		CoUninitialize();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27998");

	return 0;
}
//--------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CFileProcessingDB::setFilesToProcessing(const _ConnectionPtr &ipConnection,
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
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::doesLoginUserNameExist(const _ConnectionPtr& ipConnection, const string &strUserName)
{
	// Create a pointer to a recordset
	_RecordsetPtr ipLoginSet(__uuidof(Recordset));
	ASSERT_RESOURCE_ALLOCATION("ELI29710", ipLoginSet != NULL);

	// Sql query that should either be empty if the passed in users is not in the table
	// or will return the record with the given username
	string strLoginSelect = "Select Username From Login Where UserName = '" + strUserName + "'";

	// Open the sql query
	ipLoginSet->Open(strLoginSelect.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
		adLockReadOnly, adCmdText);

	// if not at the end of file then there is a user by that name.
	if (!asCppBool(ipLoginSet->adoEOF))
	{
		return true;
	}

	// No user by that name was found
	return false;
}
//-------------------------------------------------------------------------------------------------
