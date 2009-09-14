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

#include <string>
#include <memory>

using namespace std;
using namespace ADODB;

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
// Define constant for the current DB schema version
// This must be updated when the DB schema changes
const long glFAMDBSchemaVersion = 12;

// Table names
static const string gstrACTION = "Action";
static const string gstrACTION_STATE = "ActionState";
static const string gstrACTION_STATISTICS = "ActionStatistics";
static const string gstrDB_INFO = "DBInfo";
static const string gstrFAM_FILE = "FAMFile";
static const string gstrFILE_ACTION_STATE_TRANSITION = "FileActionStateTransition";
static const string gstrLOCK_TABLE = "LockTable";
static const string gstrLOGIN = "Login";
static const string gstrQUEUE_EVENT = "QueueEvent";
static const string gstrQUEUE_EVENT_CODE = "QueueEventCode";
static const string gstrMACHINE = "Machine";
static const string gstrFAM_USER = "FAMUser";
static const string gstrFAM_FILE_ACTION_COMMENT = "FileActionComment";
static const string gstrFAM_SKIPPED_FILE = "SkippedFile";
static const string gstrFAM_TAG = "Tag";
static const string gstrFAM_FILE_TAG = "FileTag";

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
	bsm.flushToByteStream( 8 );
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

	map<DWORD, ADODB::_ConnectionPtr>::iterator it;
	it = m_mapThreadIDtoDBConnections.find(dwThreadID);

	if (it != m_mapThreadIDtoDBConnections.end() )
	{
		// close the connection if it is open
		ADODB::_ConnectionPtr ipConnection = it->second;
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
EActionStatus CFileProcessingDB::setFileActionState( ADODB::_ConnectionPtr ipConnection, long nFileID, 
													string strAction, const string& strState,
													const string& strException,
													long nActionID, bool bLockDB,
													const string& strUniqueProcessID)
{
	auto_ptr<LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr>> apDBlg;
	auto_ptr<TransactionGuard> apTG;
	try
	{
		ASSERT_ARGUMENT("ELI26796", ipConnection != NULL);
		ASSERT_ARGUMENT("ELI26795", !strAction.empty() || nActionID != -1);

		EActionStatus easRtn = kActionUnattempted;

		// Set up the select query to select the file to change
		string strFileSQL = "SELECT * FROM FAMFile WHERE ID = " + asString (nFileID);

		if (bLockDB)
		{
			// Lock the database for this instance
			apDBlg.reset(
				new LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr>(getThisAsCOMPtr()));
		}

		// Make sure the DB Schema is the expected version
		validateDBSchemaVersion();

		_RecordsetPtr ipFileSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI13542", ipFileSet != NULL );
		ipFileSet->Open( strFileSQL.c_str(), _variant_t((IDispatch *)ipConnection, true), 
			adOpenDynamic, adLockOptimistic, adCmdText );

		if (bLockDB)
		{
			// Begin a transaction
			apTG.reset(new TransactionGuard(ipConnection));
		}

		// Update action ID/Action name
		if (!strAction.empty() && nActionID == -1)
		{
			nActionID = getActionID(ipConnection, strAction);
		}
		else if (strAction.empty() && nActionID != -1)
		{
			strAction = getActionName(ipConnection, nActionID);
		}

		// Action Column to update
		string strActionCol = "ASC_" + strAction;

		// Find the file if it exists
		if ( ipFileSet->adoEOF == VARIANT_FALSE )
		{
			FieldsPtr ipFileSetFields = ipFileSet->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI26867", ipFileSetFields != NULL);

			// Get the previous state
			string strPrevStatus = getStringField(ipFileSetFields, strActionCol ); 
			easRtn = asEActionStatus ( strPrevStatus );

			// Get the current record
			UCLID_FILEPROCESSINGLib::IFileRecordPtr ipCurrRecord;
			ipCurrRecord = getFileRecordFromFields(ipFileSetFields);

			// set the new state
			setStringField( ipFileSetFields, strActionCol, strState );

			// If transition to complete and AutoDeleteFileActionComment == true
			// then clear the file action comment for this file
			if (strState == "C" && m_bAutoDeleteFileActionComment)
			{
				clearFileActionComment(ipConnection, nFileID, nActionID);
			}

			// Update the file record
			ipFileSet->Update();

			// if the old status does not equal the new status add transition records
			if ( strPrevStatus != strState )
			{
				// update the statistics
				EActionStatus easStatsFrom = easRtn;
				if (easRtn == kActionProcessing)
				{
					// If moving from processing, check for record in skipped file table
					string strTempSQL = "SELECT [ID] FROM [SkippedFile] WHERE ([FileID] = "
						+ asString(nFileID) + " AND [ActionID] = " + asString(nActionID) + ")";

					// Get the record set
					_RecordsetPtr ipSkippedSet(__uuidof(Recordset));
					ASSERT_RESOURCE_ALLOCATION("ELI26949", ipSkippedSet != NULL);
					ipSkippedSet->Open(strTempSQL.c_str(), _variant_t((IDispatch *)ipConnection, true),
						adOpenForwardOnly, adLockReadOnly, adCmdText );

					// If there is a record in the skipped table, call update stats with
					// Skipped as the previous state
					if (ipSkippedSet->adoEOF == VARIANT_FALSE)
					{
						easStatsFrom = kActionSkipped;
					}
				}
				updateStats(ipConnection, nActionID, easStatsFrom, asEActionStatus(strState),
					ipCurrRecord, ipCurrRecord);

				// Only update FileActionStateTransition table if required
				if (m_bUpdateFASTTable)
				{
					addFileActionStateTransition( ipConnection, nFileID, nActionID, strPrevStatus, 
						strState, strException, "" );
				}

				if (!strUniqueProcessID.empty())
				{
					// These calls are order dependent.
					// Remove the skipped record (if any) and add a new
					// skipped file record if the new state is skipped
					removeSkipFileRecord(ipConnection, nFileID, nActionID);
					if (strState == "S")
					{
						// Add a record to the skipped table
						addSkipFileRecord(ipConnection, nFileID, nActionID, strUniqueProcessID);
					}
				}
			}

			// If there is a transaction guard then commit the transaction
			if (apTG.get() != NULL)
			{
				apTG->CommitTrans();
			}
		}
		else
		{
			// No file with the given id
			UCLIDException ue("ELI13543", "File ID was not found." );
			ue.addDebugInfo ( "File ID", nFileID );
			throw ue;
		}

		return easRtn;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26912");
}
//--------------------------------------------------------------------------------------------------
EActionStatus CFileProcessingDB::asEActionStatus  ( const string& strStatus )
{
	EActionStatus easRtn;

	if ( strStatus == "P" )
	{
		easRtn = kActionPending;
	}
	else if ( strStatus == "R" )
	{
		easRtn = kActionProcessing;
	}
	else if ( strStatus == "F" )
	{
		easRtn = kActionFailed;
	}
	else if ( strStatus == "C" )
	{
		easRtn = kActionCompleted;
	}
	else if ( strStatus == "U" )
	{
		easRtn = kActionUnattempted;
	}
	else if ( strStatus == "S" )
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
string CFileProcessingDB::asStatusString ( EActionStatus eStatus )
{
	switch ( eStatus )
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
void CFileProcessingDB::addQueueEventRecord( ADODB::_ConnectionPtr ipConnection, long nFileID, 
											string strFileName, string strQueueEventCode )
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

			_RecordsetPtr ipQueueEventSet( __uuidof( Recordset ));
			ASSERT_RESOURCE_ALLOCATION("ELI13591", ipQueueEventSet != NULL );

			// Open the QueueEvent table
			ipQueueEventSet->Open( "QueueEvent", _variant_t((IDispatch *)ipConnection, true), 
				adOpenDynamic, adLockOptimistic, adCmdTableDirect );
			_lastCodePos = "20";

			// Add a new record
			ipQueueEventSet->AddNew();
			_lastCodePos = "30";

			FieldsPtr ipFields = ipQueueEventSet->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI26875", ipFields != NULL);

			//  add the field values to the new record
			setLongField( ipFields, "FileID",  nFileID );
			_lastCodePos = "40";

			setStringField( ipFields, "DateTimeStamp", 
				getSQLServerDateTime(ipConnection) );
			_lastCodePos = "50";
			
			setStringField( ipFields, "QueueEventCode", strQueueEventCode );
			_lastCodePos = "60";

			setLongField( ipFields, "FAMUserID", getFAMUserID(ipConnection));
			_lastCodePos = "70";

			setLongField( ipFields, "MachineID", getMachineID(ipConnection));
			_lastCodePos = "80";

			// File should exist for these options
			if ( strQueueEventCode == "A" || strQueueEventCode == "M" )
			{
				_lastCodePos = "80_10";

				// if adding or modifing the file add the file modified and file size fields
				CTime fileTime;
				fileTime = getFileModificationTimeStamp( strFileName );
				_lastCodePos = "80_20";

				string strFileModifyTime = fileTime.Format("%m/%d/%y %I:%M:%S %p");
				setStringField(	ipFields, "FileModifyTime", strFileModifyTime );
				_lastCodePos = "80_30";

				// Get the file size
				long long llFileSize;
				llFileSize = getSizeOfFile( strFileName );
				_lastCodePos = "80_40";

				// Set the file size in the table
				setLongLongField( ipFields, "FileSizeInBytes", llFileSize );
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
void CFileProcessingDB::addFileActionStateTransition ( ADODB::_ConnectionPtr ipConnection,
													  long nFileID, long nActionID, 
													  const string &strFromState, 
													  const string &strToState, 
													  const string &strException, 
													  const string &strComment )
{
	// check if updates to FileActionStateTransition table are required
	if ( !m_bUpdateFASTTable || strToState == strFromState )
	{
		// nothing to do
		return;
	}
	_RecordsetPtr ipActionTransitionSet( __uuidof( Recordset ));
	ASSERT_RESOURCE_ALLOCATION("ELI13593", ipActionTransitionSet != NULL );

	// Open the transition table
	ipActionTransitionSet->Open( "FileActionStateTransition", 
		_variant_t((IDispatch *)ipConnection, true), adOpenDynamic, 
		adLockOptimistic, adCmdTableDirect );

	// Add a new record
	ipActionTransitionSet->AddNew();

	// Get the fields pointer
	FieldsPtr ipFields = ipActionTransitionSet->Fields;
	ASSERT_RESOURCE_ALLOCATION("ELI26868", ipFields != NULL);

	// set the records fields
	setLongField( ipFields, "FileID", nFileID );
	setLongField( ipFields, "ActionID", nActionID );
	setStringField( ipFields, "ASC_From", strFromState );
	setStringField( ipFields, "ASC_To", strToState );
	setStringField( ipFields, "DateTimeStamp", getSQLServerDateTime(ipConnection));
	setLongField( ipFields, "FAMUserID", getFAMUserID(ipConnection));
	setLongField( ipFields, "MachineID", getMachineID(ipConnection));

	// if a transition to failed add the exception
	if ( strToState == "F" )
	{
		setStringField( ipFields, "Exception", strException, true );
	}
	
	// save Comment
	setStringField( ipFields, "Comment", strComment, true );

	// update the table
	ipActionTransitionSet->Update();
}
//--------------------------------------------------------------------------------------------------
long CFileProcessingDB::getFileID(ADODB::_ConnectionPtr ipConnection, string& rstrFileName)
{
	try
	{
		return getKeyID(ipConnection, gstrFAM_FILE, "FileName", rstrFileName, false);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26720");
}
//--------------------------------------------------------------------------------------------------
long CFileProcessingDB::getActionID( ADODB::_ConnectionPtr ipConnection, string& rstrActionName )
{
	try
	{
		return getKeyID(ipConnection, gstrACTION, "ASCName", rstrActionName, false);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26721");
}
//--------------------------------------------------------------------------------------------------
string CFileProcessingDB::getActionName( ADODB::_ConnectionPtr ipConnection, long nActionID )
{
	try
	{
		_RecordsetPtr ipAction( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI14046", ipAction != NULL );

		// Oepn Action table
		ipAction->Open( "Action", _variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
			adLockReadOnly, adCmdTableDirect );

		// Setup criteria to find
		string strCriteria = "ID = " + asString(nActionID);

		// search for the given action ID
		ipAction->Find( strCriteria.c_str(), 0, adSearchForward );
		if ( ipAction->adoEOF == VARIANT_TRUE )
		{
			// Action ID was not found
			UCLIDException ue ("ELI14047", "Action ID was not found." );
			ue.addDebugInfo( "Action ID", nActionID);
			throw ue;
		}

		// return the found Action name
		return getStringField( ipAction->Fields, "ASCName" );
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26722");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::addASTransFromSelect ( ADODB::_ConnectionPtr ipConnection,
											  const string &strAction, long nActionID,
											  const string &strToState, const string &strException,
											  const string &strComment, const string &strWhereClause, 
											  const string &strTopClause )
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
		string strInsertTrans = "INSERT INTO FileActionStateTransition ( FileID, ActionID, ASC_From, "
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
ADODB::_ConnectionPtr  CFileProcessingDB::getDBConnection()
{
	INIT_EXCEPTION_AND_TRACING("MLI00018");
	try
	{
		try
		{
			// Get the current threads ID
			DWORD dwThreadID = GetCurrentThreadId();

			ADODB::_ConnectionPtr ipConnection = NULL;

			// Lock mutex to keep other instances from running code that may cause the
			// connection to be reset
			CSingleLock lg(&m_mutex, TRUE);

			map<DWORD, ADODB::_ConnectionPtr>::iterator it;
			it = m_mapThreadIDtoDBConnections.find(dwThreadID);
			_lastCodePos = "5";

			if (it != m_mapThreadIDtoDBConnections.end() )
			{
				ipConnection = it->second;
			}

			// check to see if the DB connection has been allocated
			if ( ipConnection == NULL )
			{
				_lastCodePos = "10";
				ipConnection.CreateInstance( __uuidof( Connection ) );
				ASSERT_RESOURCE_ALLOCATION("ELI13650",  ipConnection != NULL );

				// Reset the schema version to indicate that it needs to be read from DB
				m_iDBSchemaVersion = 0;
			}

			_lastCodePos = "20";

			// if closed and Database server and database name are defined,  open the database connection
			if ( ipConnection->State == adStateClosed && !m_strDatabaseServer.empty() 
				&& !m_strDatabaseName.empty() )
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
void CFileProcessingDB::reCalculateStats( ADODB::_ConnectionPtr ipConnection, long nActionID )
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
	_RecordsetPtr ipCalcStatsSet( __uuidof( Recordset ));
	ASSERT_RESOURCE_ALLOCATION("ELI14048", ipCalcStatsSet != NULL );

	// Open the Calc set table in the database
	ipCalcStatsSet->Open( strCalcSQL.c_str(), _variant_t((IDispatch *)ipConnection, true), 
		adOpenDynamic, adLockOptimistic, adCmdText );

	// Create a pointer to a recordset
	_RecordsetPtr ipActionStats( __uuidof( Recordset ));
	ASSERT_RESOURCE_ALLOCATION("ELI14049", ipActionStats != NULL );

	// Select the existing Statistics record if it exists
	string strSelectStat = "SELECT * FROM ActionStatistics WHERE ActionID = " + asString(nActionID);

	// Open the recordse to for the statisics with the record for ActionID if it exists
	ipActionStats->Open( strSelectStat.c_str(), _variant_t((IDispatch *)ipConnection, true), 
		adOpenDynamic, adLockOptimistic, adCmdText );

	FieldsPtr ipActionFields = NULL;

	// If no records in the data set then will need create a new record
	if ( ipActionStats->adoEOF == VARIANT_TRUE )
	{
		// Create new record
		ipActionStats->AddNew();

		// Get the fields from the new record
		ipActionFields = ipActionStats->Fields;
		ASSERT_RESOURCE_ALLOCATION("ELI26865", ipActionFields != NULL);

		//  Set Action ID
		setLongField( ipActionFields, "ActionID", nActionID );
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
		string strActionState = getStringField( ipCalcFields, strActionColName ); 

		if (strActionState != "U")
		{
			long lNumDocs = getLongField( ipCalcFields, "NumDocs" );
			long lNumPages = getLongField( ipCalcFields, "SumOfPages" );
			long long llNumBytes = getLongLongField( ipCalcFields, "SumOfFileSize" );

			// Set the sums to the appropriate statistics property
			if ( strActionState == "F" )
			{
				// Set Failed totals
				lNumDocsFailed = lNumDocs;
				lNumPagesFailed = lNumPages;
				llNumBytesFailed = llNumBytes;
			}
			else if ( strActionState == "C" )
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
	setLongField( ipActionFields, "NumDocumentsFailed", lNumDocsFailed );
	setLongField( ipActionFields, "NumPagesFailed", lNumPagesFailed );
	setLongLongField( ipActionFields, "NumBytesFailed", llNumBytesFailed );

	// Set Completed totals
	setLongField( ipActionFields, "NumDocumentsComplete", lNumDocsCompleted );
	setLongField( ipActionFields, "NumPagesComplete", lNumPagesCompleted );
	setLongLongField( ipActionFields, "NumBytesComplete", llNumBytesCompleted );

	// Set Skipped totals
	setLongField( ipActionFields, "NumDocumentsSkipped", lNumDocsSkipped );
	setLongField( ipActionFields, "NumPagesSkipped", lNumPagesSkipped );
	setLongLongField( ipActionFields, "NumBytesSkipped", llNumBytesSkipped );

	// Save totals in the ActionStatistics table
	setLongField( ipActionFields, "NumDocuments", lTotalDocs );
	setLongField( ipActionFields, "NumPages", lTotalPages );
	setLongLongField( ipActionFields, "NumBytes", llTotalBytes );

	// Update the record
	ipActionStats->Update();
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::dropTables()
{
	// First remove all Product Specific stuff
	removeProductSpecificDB();

	// Get the list of tables
	vector<string> vecTables; 
	getExpectedTables(vecTables);

	// Remove the login table from the list
	eraseFromVector(vecTables, gstrLOGIN);

	// Drop the tables in the vector
	dropTablesInVector( getDBConnection(), vecTables );
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::addTables()
{
	try
	{
		vector<string> vecQueries;

		// Add queries to create tables to the vector
		vecQueries.push_back(gstrCREATE_ACTION_TABLE);
		vecQueries.push_back(gstrCREATE_LOCK_TABLE);
		vecQueries.push_back(gstrCREATE_DB_INFO_TABLE);
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
		vecQueries.push_back(gstrCREATE_FAM_TAG_TABLE);
		vecQueries.push_back(gstrCREATE_FAM_FILE_TAG_TABLE);
		vecQueries.push_back(gstrCREATE_FILE_TAG_INDEX);

		// Only create the login table if it does not already exist
		if ( !doesTableExist( getDBConnection(), "Login"))
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
		vecQueries.push_back(gstrADD_FILE_ACTION_COMMENT_ACTION_FK);
		vecQueries.push_back(gstrADD_FILE_ACTION_COMMENT_FAM_FILE_FK);
		vecQueries.push_back(gstrADD_SKIPPED_FILE_FAM_FILE_FK);
		vecQueries.push_back(gstrADD_SKIPPED_FILE_ACTION_FK);
		vecQueries.push_back(gstrADD_FILE_TAG_FAM_FILE_FK);
		vecQueries.push_back(gstrADD_FILE_TAG_TAG_ID_FK);

		// Execute all of the queries
		executeVectorOfSQL(getDBConnection(), vecQueries);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI18011");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::initializeTableValues()
{
	vector<string> vecQueries;

	// Add valid action states to the Action State table
	vecQueries.push_back( "INSERT INTO [ActionState] ([Code], [Meaning]) "
		"VALUES('C', 'Complete')");

	vecQueries.push_back( "INSERT INTO [ActionState] ([Code], [Meaning]) "
		"VALUES('F', 'Failed')");

	vecQueries.push_back( "INSERT INTO [ActionState] ([Code], [Meaning]) "
		"VALUES('P', 'Pending')");

	vecQueries.push_back( "INSERT INTO [ActionState] ([Code], [Meaning]) "
		"VALUES('R', 'Processing')");

	vecQueries.push_back( "INSERT INTO [ActionState] ([Code], [Meaning]) "
		"VALUES('U', 'Unattempted')");

	vecQueries.push_back( "INSERT INTO [ActionState] ([Code], [Meaning]) "
		"VALUES('S', 'Skipped')");

	// Add Valid Queue event codes the QueueEventCode table
	vecQueries.push_back( "INSERT INTO [QueueEventCode] ([Code], [Description]) "
		"VALUES('A', 'File added to queue')");
	
	vecQueries.push_back( "INSERT INTO [QueueEventCode] ([Code], [Description]) "
		"VALUES('D', 'File deleted from queue')");

	vecQueries.push_back( "INSERT INTO [QueueEventCode] ([Code], [Description]) "
		"VALUES('F', 'Folder was deleted')");

	vecQueries.push_back( "INSERT INTO [QueueEventCode] ([Code], [Description]) "
		"VALUES('M', 'File was modified')");

	vecQueries.push_back( "INSERT INTO [QueueEventCode] ([Code], [Description]) "
		"VALUES('R', 'File was renamed')");

	// Add the schema version to the DBInfo table
	string strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrFAMDB_SCHEMA_VERSION +
		"', '" + asString(glFAMDBSchemaVersion) + "')";
	vecQueries.push_back( strSQL);

	// Add Command Timeout setting
	strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrCOMMAND_TIMEOUT +
		"', '" + asString(glDEFAULT_COMMAND_TIMEOUT) + "')";
	vecQueries.push_back( strSQL);

	// Add Update Queue Event Table setting
	strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrUPDATE_QUEUE_EVENT_TABLE 
		+ "', '1')";
	vecQueries.push_back( strSQL);

	// Add Update Queue Event Table setting
	strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrUPDATE_FAST_TABLE + "', '1')";
	vecQueries.push_back( strSQL);

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

	// Execute all of the queries
	executeVectorOfSQL( getDBConnection(), vecQueries);
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::copyActionStatus( ADODB::_ConnectionPtr ipConnection, string strFrom, 
										 string strTo, bool bAddTransRecords, long nToActionID)
{
	try
	{
		if ( bAddTransRecords )
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
			string strAddSkipped = "INSERT INTO SkippedFile (FileID, ActionID, UserName) SELECT "
				" FAMFile.ID, " + strToActionID + " AS NewActionID, '" + getCurrentUserName()
				+ "' AS NewUserName FROM FAMFile WHERE ASC_" + strFrom + " = 'S'";

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
void CFileProcessingDB::addActionColumn(string strAction)
{

	// Add new Column to FAMFile table
	// Create SQL statement to add the column to FAMFile
	string strAddColSQL = "Alter Table FAMFile Add ASC_" + strAction + " nvarchar(1)";

	// Run the SQL to add column to FAMFile
	executeCmdQuery(getDBConnection(), strAddColSQL);

	// Set the default value to Unattempted
	// Set the from statement
	string strFrom = "FROM FAMFile";

	// Create the query and update the file status for all files to unattempted
	string strUpdateSQL = "UPDATE FAMFile SET ASC_" + strAction + " = 'U' " + strFrom;
	executeCmdQuery(getDBConnection(), strUpdateSQL);

	// Create index on the new column
	string strCreateIDX = "Create Index IX_ASC_" + strAction + " on FAMFile ( ASC_" 
		+ strAction + ")";
	executeCmdQuery(getDBConnection(), strCreateIDX);

	// Add foreign key contraint for the new column to reference the ActionState table
	string strAddContraint = "ALTER TABLE FAMFile WITH CHECK ADD CONSTRAINT FK_ASC_" 
		+ strAction + " FOREIGN KEY(ASC_" + 
		strAction + ") REFERENCES ActionState(Code)";

	// Create the foreign key
	executeCmdQuery(getDBConnection(), strAddContraint);

	// Add the default contraint for the column
	string strDefault = "ALTER TABLE FAMFile ADD CONSTRAINT DF_ASC_" 
		+ strAction + " DEFAULT 'U' FOR ASC_" + strAction;
	executeCmdQuery(getDBConnection(), strDefault);
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::removeActionColumn(string strAction)
{
	// Remove the Foreign key relationship
	dropConstraint(getDBConnection(), gstrFAM_FILE, "FK_ASC_" + strAction);

	// Drop index on the action column
	string strSQL = "Drop Index IX_ASC_" + strAction + " ON FAMFile";
	executeCmdQuery(getDBConnection(), strSQL);

	// Remove the default contraint
	dropConstraint(getDBConnection(), gstrFAM_FILE, "DF_ASC_" + strAction);

	// Drop the column
	strSQL = "ALTER TABLE FAMFile DROP COLUMN ASC_" + strAction;
	executeCmdQuery(getDBConnection(), strSQL);
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::updateStats( ADODB::_ConnectionPtr ipConnection, long nActionID, 
									EActionStatus eFromStatus, EActionStatus eToStatus, 
									UCLID_FILEPROCESSINGLib::IFileRecordPtr ipNewRecord, 
									UCLID_FILEPROCESSINGLib::IFileRecordPtr ipOldRecord)
{
	// Only time a ipOldRecord can be NULL is if the from status is kActionUnattempted
	if (eFromStatus != kActionUnattempted && ipOldRecord == NULL )
	{
		UCLIDException ue("ELI17029", "Must have an old record");
		ue.addDebugInfo("FromStatus", eFromStatus);
		ue.addDebugInfo("ToStatus", eToStatus);
		ue.addDebugInfo("ActionID", nActionID);
		throw ue;
	}

	// Only time a ipNewRecord can be NULL is if the to status is kActionUnattempted
	if (eToStatus != kActionUnattempted && ipNewRecord == NULL )
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
	_bstr_t bstrTemp;
	if (ipOldRecord != NULL)
	{
		ipOldRecord->GetFileData(&lTempFileID, &lTempActionID, bstrTemp.GetAddress(),
			&llOldFileSize, &lOldPages);
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
				&llNewFileSize, &lNewPages);
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
	bool bRecalculated = loadStats(ipConnection, nActionID);
	if ( bRecalculated )
	{
		// if the stats were recalculated the all added on changed records have been included
		return;
	}
	// Create an ActionStatistics pointer to return the values
	UCLID_FILEPROCESSINGLib::IActionStatisticsPtr ipActionStats;
	ipActionStats = m_mapActionIDtoStats[nActionID];

	// Transfer the data from the recordset to the ActionStatisticsPtr
	switch (eToStatus )
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
	if ( eToStatus != kActionUnattempted )
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
	if ( eFromStatus != kActionUnattempted )
	{
		// Make sure the ipOldRecord is not NULL
		ASSERT_ARGUMENT("ELI17055", ipOldRecord != NULL);
		long lNumDocsTotal(0), lNumPagesTotal(0);
		LONGLONG llNumBytesTotal(0);
		ipActionStats->GetTotals(&lNumDocsTotal, &lNumPagesTotal, &llNumBytesTotal);
		ipActionStats->SetTotals(lNumDocsTotal-1, lNumPagesTotal - lOldPages,
			llNumBytesTotal - llOldFileSize);
	}

	// Save the stats
	saveStats(ipConnection, nActionID);
}
//--------------------------------------------------------------------------------------------------
bool CFileProcessingDB::loadStats( ADODB::_ConnectionPtr ipConnection, long nActionID )
{
	// Create a pointer to a recordset
	_RecordsetPtr ipActionStatSet( __uuidof( Recordset ));
	ASSERT_RESOURCE_ALLOCATION("ELI14099", ipActionStatSet != NULL );

	// Select the existing Statistics record if it exists
	string strSelectStat = "SELECT * FROM ActionStatistics WHERE ActionID = " + asString(nActionID);

	// Open the recordse to for the statisics with the record for ActionID if it exists
	ipActionStatSet->Open( strSelectStat.c_str(), 
		_variant_t((IDispatch *)ipConnection, true), adOpenDynamic, 
		adLockOptimistic, adCmdText );

	bool bRecalculated = false;
	if ( ipActionStatSet->adoEOF == VARIANT_TRUE )
	{
		reCalculateStats(ipConnection, nActionID);
		bRecalculated = true;
		ipActionStatSet->Requery(adOptionUnspecified);
	}
	if ( ipActionStatSet->adoEOF == VARIANT_TRUE )
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
	ASSERT_RESOURCE_ALLOCATION("ELI14101", ipActionStats != NULL );

	// Get all the data from the recordset
	long lNumDocsFailed =  getLongField(ipFields, "NumDocumentsFailed" );
	long lNumPagesFailed = getLongField(ipFields, "NumPagesFailed" );
	LONGLONG llNumBytesFailed = getLongLongField(ipFields, "NumBytesFailed" );
	long lNumDocsSkipped =  getLongField(ipFields, "NumDocumentsSkipped" );
	long lNumPagesSkipped = getLongField( ipFields, "NumPagesSkipped" );
	LONGLONG llNumBytesSkipped = getLongLongField( ipFields, "NumBytesSkipped" );
	long lNumDocsComplete = getLongField( ipFields, "NumDocumentsComplete" );
	long lNumPagesComplete = getLongField( ipFields, "NumPagesComplete" );
	LONGLONG llNumBytesComplete = getLongLongField( ipFields, "NumBytesComplete" );
	long lNumDocs = getLongField( ipFields, "NumDocuments" );
	long lNumPages = getLongField( ipFields, "NumPages" );
	LONGLONG llNumBytes = getLongLongField( ipFields, "NumBytes" );

	// Transfer the data from the recordset to the ActionStatisticsPtr
	ipActionStats->SetAllStatistics(lNumDocs, lNumDocsComplete, lNumDocsFailed, lNumDocsSkipped,
		lNumPages, lNumPagesComplete, lNumPagesFailed, lNumPagesSkipped, llNumBytes,
		llNumBytesComplete, llNumBytesFailed, llNumBytesSkipped);

	// save the stats back to the map
	m_mapActionIDtoStats[nActionID] = ipActionStats;
	return bRecalculated;
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::saveStats( ADODB::_ConnectionPtr ipConnection, long nActionID )
{
	// Create a pointer to a recordset
	_RecordsetPtr ipActionStatSet( __uuidof( Recordset ));
	ASSERT_RESOURCE_ALLOCATION("ELI19506", ipActionStatSet != NULL );

	// Select the existing Statistics record if it exists
	string strSelectStat = "SELECT * FROM ActionStatistics WHERE ActionID = " + asString(nActionID);

	// Open the recordse to for the statisics with the record for ActionID if it exists
	ipActionStatSet->Open( strSelectStat.c_str(), _variant_t((IDispatch *)ipConnection, true), 
		adOpenDynamic, adLockOptimistic, adCmdText );

	// Find the stats for the given action
	if ( m_mapActionIDtoStats.find(nActionID) == m_mapActionIDtoStats.end() )
	{
		UCLIDException ue("ELI14104", "No Statistics to save.");
		ue.addDebugInfo("ActionID", nActionID);
		throw ue;
	}

	if (ipActionStatSet->adoEOF == VARIANT_FALSE)
	{
		// Create an ActionStatistics pointer to return the values
		UCLID_FILEPROCESSINGLib::IActionStatisticsPtr ipActionStats;
		ipActionStats = m_mapActionIDtoStats[nActionID];

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
		setLongField( ipFields, "NumPagesFailed", lNumPagesFailed );
		setLongLongField( ipFields, "NumBytesFailed", llNumBytesFailed );
		setLongField( ipFields, "NumDocumentsComplete", lNumDocsComplete );
		setLongField( ipFields, "NumPagesComplete", lNumPagesComplete );
		setLongLongField(ipFields, "NumBytesComplete", llNumBytesComplete);
		setLongField(ipFields, "NumDocumentsSkipped", lNumDocsSkipped);
		setLongField( ipFields, "NumPagesSkipped", lNumPagesSkipped );
		setLongLongField( ipFields, "NumBytesSkipped", llNumBytesSkipped );
		setLongField( ipFields, "NumDocuments", lNumDocs );
		setLongField( ipFields, "NumPages", lNumPages );
		setLongLongField( ipFields, "NumBytes", llNumBytes );

		// Update the action statistics
		ipActionStatSet->Update();

		m_mapActionIDtoStats[nActionID] = ipActionStats;
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
	if ( m_iDBSchemaVersion != 0 )
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
	if ( iDBSchemaVersion != glFAMDBSchemaVersion )
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
void CFileProcessingDB::lockDB(ADODB::_ConnectionPtr ipConnection)
{
	CSingleLock lock(&m_mutex, TRUE );

	// If DB is already locked return
	if ( m_bDBLocked )
	{
		return;
	}

	// Keep trying to lock the DB until it is locked
	while ( !m_bDBLocked )
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
				TransactionGuard tg(ipConnection);

				// Create a pointer to a recordset
				_RecordsetPtr ipLockTable( __uuidof( Recordset ));
				ASSERT_RESOURCE_ALLOCATION("ELI14550", ipLockTable != NULL );

				// Open recordset with the locktime 
				ipLockTable->Open( gstrDB_LOCK_QUERY.c_str(), 
					_variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
					adLockReadOnly, adCmdText );

				// If we get to here the db connection should be valid
				bConnectionGood = true;

				// If there is an existing record check to see if the time to see if the lock
				// has been on for more than the lock timeout
				if ( ipLockTable->adoEOF == VARIANT_FALSE )
				{
					// Get the time locked value from the record 
					long nSecondsLocked = getLongField(ipLockTable->Fields, "TimeLocked"); 
					if ( nSecondsLocked > m_lDBLockTimeout )
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
						ue.addDebugInfo ( "Lock Timeout", m_lDBLockTimeout);
						ue.addDebugInfo ( "Actual Lock Time", asString(nSecondsLocked));
						ue.log();

						// Restart the loop since we don't want to assume this instance will 
						// get the lock
						continue;
					}
				}

				string strAddLockSQL = "INSERT INTO LockTable (LockID, UPI ) VALUES ( 1, '" 
					+  m_strUPI + "')";
				
				// Add the lock
				executeCmdQuery(ipConnection, strAddLockSQL);

				// Commit the changes
				// If a DB lock is in the table for another process this will throw an exception
				tg.CommitTrans();

				// Update the lock flag to indicate the DB is locked
				m_bDBLocked = true;
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI14973");
		}
		catch(UCLIDException &ue)
		{
			// if the bConnectionGood flag is false the exception should be thrown
			if ( !bConnectionGood ) 
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
void CFileProcessingDB::unlockDB(ADODB::_ConnectionPtr ipConnection)
{
	CSingleLock lock(&m_mutex, TRUE );

	// if DB is already unlocked return
	if ( !m_bDBLocked )
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
string CFileProcessingDB::getEncryptedAdminPWFromDB()
{
	// Open the Login Table
	// Lock the mutex for this instance
	CSingleLock lock(&m_mutex, TRUE );

	// Create a pointer to a recordset
	_RecordsetPtr ipLoginSet( __uuidof( Recordset ));
	ASSERT_RESOURCE_ALLOCATION("ELI15103", ipLoginSet != NULL );

	// setup the SQL Query.  Currently the only user that is allowed is 'admin'
	string strSQL = "SELECT * FROM LOGIN WHERE UserName = '" + gstrADMIN_USER + "'";

	// Open the Action set for Action name 
	ipLoginSet->Open( strSQL.c_str(), _variant_t((IDispatch *)getDBConnection(), true), 
		adOpenStatic, adLockReadOnly, adCmdText );

	// admin user was in the DB if not at the end of file
	if ( ipLoginSet->adoEOF == VARIANT_FALSE )
	{
		// Return the encrypted password that is stored in the DB
		string strEncryptedPW = getStringField( ipLoginSet->Fields, "Password" );
		return strEncryptedPW;
	}

	// Was not in the DB
	return "";
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::encryptAndStoreUserNamePassword(const string strUserNameAndPassword)
{
	// Get the encrypted version of the combined string
	string strEncryptedCombined = getEncryptedString( strUserNameAndPassword );

	// Lock the mutex for this instance
	CSingleLock lock( &m_mutex, TRUE );

	// Create a pointer to a recordset
	_RecordsetPtr ipLoginSet( __uuidof( Recordset ));
	ASSERT_RESOURCE_ALLOCATION("ELI15722", ipLoginSet != NULL );

	// Begin Transaction
	TransactionGuard tg( getDBConnection() );

	// Retrieve records from Login table.  Currently the only user that is allowed is 'admin'
	string strSQL = "SELECT * FROM LOGIN WHERE UserName = '" + gstrADMIN_USER + "'";
	ipLoginSet->Open( strSQL.c_str(), _variant_t((IDispatch *)getDBConnection(), true), 
		adOpenDynamic, adLockPessimistic, adCmdText );

	// User not in DB if at the end of file
	if (ipLoginSet->adoEOF == VARIANT_TRUE)
	{
		// Insert a new record
		ipLoginSet->AddNew();

		// Set the UserName field
		setStringField( ipLoginSet->Fields, "UserName", gstrADMIN_USER );
	}

	// Update the password field
	setStringField( ipLoginSet->Fields, "Password", strEncryptedCombined );
	ipLoginSet->Update();

	// Commit the changes
	tg.CommitTrans();
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
	bytesManipulator.flushToByteStream( 8 );

	// Get the password 'key' based on the 4 hex global variables
	ByteStream pwBS;
	getFAMPassword( pwBS );

	// Do the encryption
	ByteStream encryptedBS;
	EncryptionEngine ee;
	ee.encrypt( encryptedBS, bytes, pwBS );

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
	ADODB::FieldsPtr ipFields)
{
	// Make sure the ipFields argument is not NULL
	ASSERT_ARGUMENT("ELI17028", ipFields != NULL);

	UCLID_FILEPROCESSINGLib::IFileRecordPtr ipFileRecord(CLSID_FileRecord);
	ASSERT_RESOURCE_ALLOCATION("ELI17027", ipFileRecord != NULL );
	
	// Set the file data from the fields collection (set ActionID to 0)
	ipFileRecord->SetFileData(getLongField(ipFields, "ID"), 0,
		getStringField(ipFields, "FileName").c_str(), getLongLongField(ipFields, "FileSize"),
		getLongField(ipFields, "Pages"));

	return ipFileRecord;
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::setFieldsFromFileRecord(ADODB::FieldsPtr ipFields, 
											UCLID_FILEPROCESSINGLib::IFileRecordPtr ipFileRecord)
{
	// Make sure the ipFields argument is not NULL
	ASSERT_ARGUMENT("ELI17031", ipFields != NULL);

	// Make sure the ipFileRecord object is not NULL
	ASSERT_ARGUMENT("ELI17032", ipFileRecord != NULL);
	
	// Get the file data
	long lFileID(-1), lActionID(-1), lNumPages(-1);
	LONGLONG llFileSize(-1);
	_bstr_t bstrFileName;
	ipFileRecord->GetFileData(&lFileID, &lActionID, bstrFileName.GetAddress(),
		&llFileSize, &lNumPages);

	// set the file name field
	setStringField(ipFields, "FileName", asString(bstrFileName));

	// Set the file Size
	setLongLongField(ipFields, "FileSize", llFileSize);

	// Set the number of pages
	setLongField(ipFields, "Pages", lNumPages);
}
//--------------------------------------------------------------------------------------------------
bool  CFileProcessingDB::isAdminPasswordValid(const string& strPassword)
{
	// Make combined string for comparison
	string strCombined = gstrADMIN_USER + strPassword;

	// Get the stored password ( if it exists)
	string strStoredEncryptedCombined = getEncryptedAdminPWFromDB();
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
	ByteStreamManipulator bsm( ByteStreamManipulator::kRead, decryptedPW );

	// Get the decrypted combined username and password from byte stream
	string strDecryptedCombined = "";
	bsm >> strDecryptedCombined;

	// Successful login if decrypted matches the entered
	if( strDecryptedCombined == strCombined )
	{
		return true;
	}

	// Password is not valid
	return false;
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
		string strType = getStringField( ipTables->Fields, "TABLE_TYPE" );

		// Only need to look at the tables ( no system tables or views)
		if ( strType == "TABLE")
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
long CFileProcessingDB::getMachineID(ADODB::_ConnectionPtr ipConnection)
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
long CFileProcessingDB::getFAMUserID(ADODB::_ConnectionPtr ipConnection)
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
void CFileProcessingDB::loadDBInfoSettings(ADODB::_ConnectionPtr ipConnection)
{
	INIT_EXCEPTION_AND_TRACING("MLI00019");

	try
	{
		// Create a pointer to a recordset
		_RecordsetPtr ipDBInfoSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI18171", ipDBInfoSet != NULL );

		// Initialize settings to default values
		m_iDBSchemaVersion = 0;
		m_iCommandTimeout = glDEFAULT_COMMAND_TIMEOUT;
		m_bUpdateQueueEventTable = true;
		m_bUpdateFASTTable = true;
		m_iNumberOfRetries = giDEFAULT_RETRY_COUNT;
		m_dRetryTimeout = gdDEFAULT_RETRY_TIMEOUT;

		_lastCodePos = "10";

		ipDBInfoSet->Open("DBInfo", _variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
			adLockReadOnly, adCmdTable ); 

		_lastCodePos = "20";

		// Loop through all of the records in the DBInfo table
		while (!asCppBool(ipDBInfoSet->adoEOF))
		{
			FieldsPtr ipFields = ipDBInfoSet->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI18172", ipFields != NULL );

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
				if ( ipField->Name == _bstr_t("Name") )
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
					else if (strValue == gstrCONNECTION_RETRY_TIMEOUT )
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
	for ( long n = 0; n < nSize; n++ )
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
	// Get vector of all license product specific managers
	IIUnknownVectorPtr ipProdSpecMgrs = getLicensedProductSpecificMgrs();
	ASSERT_RESOURCE_ALLOCATION("ELI18951", ipProdSpecMgrs != NULL);

	// Loop through all of the objects and call the RemoveProductSpecificSchema 
	long nSize = ipProdSpecMgrs->Size();
	for ( long n = 0; n < nSize; n++ )
	{
		UCLID_FILEPROCESSINGLib::IProductSpecificDBMgrPtr ipMgr = ipProdSpecMgrs->At(n);
		ASSERT_RESOURCE_ALLOCATION("ELI18952", ipMgr != NULL);

		// Remove the schema for the product specific manager
		ipMgr->RemoveProductSpecificSchema(getThisAsCOMPtr());
	}
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::addProductSpecificDB()
{
	// Get vector of all license product specific managers
	IIUnknownVectorPtr ipProdSpecMgrs = getLicensedProductSpecificMgrs();
	ASSERT_RESOURCE_ALLOCATION("ELI19790", ipProdSpecMgrs != NULL);

	// Loop through all of the objects and call the AddProductSpecificSchema
	long nSize = ipProdSpecMgrs->Size();
	for ( long n = 0; n < nSize; n++ )
	{
		UCLID_FILEPROCESSINGLib::IProductSpecificDBMgrPtr ipMgr = ipProdSpecMgrs->At(n);
		ASSERT_RESOURCE_ALLOCATION("ELI19791", ipMgr != NULL);

		// Add the schema from the product specific db manager
		ipMgr->AddProductSpecificSchema(getThisAsCOMPtr());
	}
}
//--------------------------------------------------------------------------------------------------
bool CFileProcessingDB::isConnectionAlive(ADODB::_ConnectionPtr ipConnection)
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
		catch( UCLIDException ue)
		{
			// Check to see if the timeout has been reached.
			if ( sw.getElapsedTime() > m_dRetryTimeout )
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
	while ( !bNoMoreRetries );

	return false;
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::addSkipFileRecord(const ADODB::_ConnectionPtr &ipConnection,
										  long nFileID, long nActionID,
										  const string& strUniqueProcessID)
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
		if (ipSkippedSet->BOF == VARIANT_FALSE)
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
			setStringField(ipFields, "UniqueFAMID", strUniqueProcessID);

			// Update the row
			ipSkippedSet->Update();
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26804");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::removeSkipFileRecord(const ADODB::_ConnectionPtr &ipConnection,
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

		CSingleLock lock(&m_mutex, TRUE );
		
		_lastCodePos = "20";
		
		// Initilize count for MLI Code iteration count
		long nCount = 0;
		map<DWORD, ADODB::_ConnectionPtr>::iterator it;
		for ( it = m_mapThreadIDtoDBConnections.begin(); it != m_mapThreadIDtoDBConnections.end(); it++)
		{

			// Do the close within a try catch because an exception on the close could just mean the connection is in a bad state and
			// recreating and opening will put it in a good state
			try
			{
				ADODB::_ConnectionPtr ipDBConnection = it->second;
				_lastCodePos = "25-" + asString(nCount);

				// This will close the existing connection if not already closed
				if ( ipDBConnection != NULL && ipDBConnection->State != adStateClosed )
				{
					_lastCodePos = "30";

					ipDBConnection->Close();
				}
			}
			CATCH_AND_LOG_ALL_EXCEPTIONS("ELI15000")
		}

		// Clear all of the connections in all of the threads
		m_mapThreadIDtoDBConnections.clear();
		_lastCodePos = "35";

		// Reset the Current connection status to not connected
		m_strCurrentConnectionStatus = gstrNOT_CONNECTED;

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
void CFileProcessingDB::clear()
{
	try
	{
		CSingleLock lock(&m_mutex, TRUE );

		// Begin a transaction
		TransactionGuard tg(getDBConnection());

		// Drop the tables
		dropTables();

		// Add the tables back
		addTables();

		// Setup the tables that require initial values
		initializeTableValues();

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
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::getFilesSkippedByUser(vector<long>& rvecSkippedFileIDs, long nActionID,
													  string strUserName,
													  const ADODB::_ConnectionPtr& ipConnection)
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
		_RecordsetPtr ipFileIDSet(__uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI26909", ipFileIDSet != NULL );

		// get the recordset with skipped file ID's
		ipFileIDSet->Open(strSQL.c_str(), _variant_t((IDispatch *)ipConnection, true),
			adOpenForwardOnly, adLockReadOnly, adCmdText );

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
		// If the parser has not been created yet, create it
		if (m_ipParser == NULL)
		{
			IMiscUtilsPtr ipMisc(CLSID_MiscUtils);
			ASSERT_RESOURCE_ALLOCATION("ELI27381", ipMisc != NULL);

			m_ipParser = ipMisc->GetNewRegExpParserInstance("");
			ASSERT_RESOURCE_ALLOCATION("ELI27382", m_ipParser != NULL);

			// Set the pattern
			m_ipParser->Pattern = gstrTAG_REGULAR_EXPRESSION.c_str();
		}

		if (strTagName.empty() ||
			m_ipParser->StringMatchesPattern(strTagName.c_str()) == VARIANT_FALSE)
		{
			UCLIDException ue("ELI27383", "Invalid tag name!");
			ue.addDebugInfo("Tag", strTagName);
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

		ipRecord->Open( strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
			adLockOptimistic, adCmdText );

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
		_RecordsetPtr ipDBInfoSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI19793", ipDBInfoSet != NULL );

		// Setup Setting Query
		string strSQL = gstrDBINFO_SETTING_QUERY;
		replaceVariable(strSQL, gstrSETTING_NAME, strSettingName);
		
		// Open the record set using the Setting Query		
		ipDBInfoSet->Open(strSQL.c_str(), _variant_t((IDispatch *)ipConnection, true),
			adOpenForwardOnly, adLockReadOnly, adCmdText ); 

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