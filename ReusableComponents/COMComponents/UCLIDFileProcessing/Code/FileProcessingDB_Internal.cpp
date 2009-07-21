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

using namespace std;
using namespace ADODB;

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
// Define constant for the current DB schema version
// This must be updated when the DB schema changes
const long glFAMDBSchemaVersion = 8;

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

// Define four UCLID passwords used for encrypting the password
// NOTE: These passwords were not exposed at the header file level because
//		 no user of this class needs to know that these passwords exist
// These passwords are also uses in the FileProcessingDB.cpp
const unsigned long	gulFAMKey1 = 0x78932517;
const unsigned long	gulFAMKey2 = 0x193E2224;
const unsigned long	gulFAMKey3 = 0x20134253;
const unsigned long	gulFAMKey4 = 0x15990323;

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
													string strAction, string strState, string strException )
{
	EActionStatus easRtn = kActionUnattempted;

	// Set up the select query to selec the file to change
	string strFileSQL = "SELECT * FROM FAMFile WHERE ID = " + asString (nFileID);

	// Lock the database for this instance
	LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

	// Make sure the DB Schema is the expected version
	validateDBSchemaVersion();

	_RecordsetPtr ipFileSet( __uuidof( Recordset ));
	ASSERT_RESOURCE_ALLOCATION("ELI13542", ipFileSet != NULL );
	ipFileSet->Open( strFileSQL.c_str(), _variant_t((IDispatch *)ipConnection, true), 
		adOpenDynamic, adLockOptimistic, adCmdText );
	
	// begin a transaction
	TransactionGuard tg(ipConnection);

	// Get the action ID and update the strAction to stored value
	long nActionID = getActionID(ipConnection, strAction);

	// Action Column to update
	string strActionCol = "ASC_" + strAction;

	// Find the file if it exists
	if ( !ipFileSet->adoEOF )
	{
		// Get the previous state
		string strPrevStatus = getStringField(ipFileSet->Fields, strActionCol ); 
		easRtn = asEActionStatus ( strPrevStatus );

		// Get the current record
		UCLID_FILEPROCESSINGLib::IFileRecordPtr ipCurrRecord;
		ipCurrRecord = getFileRecordFromFields(ipFileSet->Fields);

		// set the new state
		setStringField( ipFileSet->Fields, strActionCol, strState );

		// Update the file record
		ipFileSet->Update();

		// if the old status does not equal the new status add transition records
		if ( easRtn != asEActionStatus(strState) )
		{
			// update the statistics
			updateStats(ipConnection, nActionID, easRtn, asEActionStatus(strState), ipCurrRecord, ipCurrRecord);

			// Only update FileActionStateTransition table if required
			if (m_bUpdateFASTTable)
			{
				addFileActionStateTransition( ipConnection, nFileID, nActionID, asStatusString(easRtn), 
					strState, strException, "" );
			}
		}
		// commit the changes to the database
		tg.CommitTrans();
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

			//  add the field values to the new record
			setLongField( ipQueueEventSet->Fields, "FileID",  nFileID );
			_lastCodePos = "40";

			setStringField( ipQueueEventSet->Fields, "DateTimeStamp", 
				getSQLServerDateTime(ipConnection) );
			_lastCodePos = "50";
			
			setStringField( ipQueueEventSet->Fields, "QueueEventCode", strQueueEventCode );
			_lastCodePos = "60";

			setLongField( ipQueueEventSet->Fields, "FAMUserID", getFAMUserID(ipConnection));
			_lastCodePos = "70";

			setLongField( ipQueueEventSet->Fields, "MachineID", getMachineID(ipConnection));
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
				setStringField(	ipQueueEventSet->Fields, "FileModifyTime", strFileModifyTime );
				_lastCodePos = "80_30";

				// Get the file size
				long long llFileSize;
				llFileSize = getSizeOfFile( strFileName );
				_lastCodePos = "80_40";

				// Set the file size in the table
				setLongLongField( ipQueueEventSet->Fields, "FileSizeInBytes", llFileSize );
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

	// set the records fields
	setLongField( ipActionTransitionSet->Fields, "FileID", nFileID );
	setLongField( ipActionTransitionSet->Fields, "ActionID", nActionID );
	setStringField( ipActionTransitionSet->Fields, "ASC_From", strFromState );
	setStringField( ipActionTransitionSet->Fields, "ASC_To", strToState );
	setStringField( ipActionTransitionSet->Fields, "DateTimeStamp", 
		getSQLServerDateTime(ipConnection));
	setLongField( ipActionTransitionSet->Fields, "FAMUserID", getFAMUserID(ipConnection));
	setLongField( ipActionTransitionSet->Fields, "MachineID", getMachineID(ipConnection));

	// if a transition to failed add the exception
	if ( strToState == "F" )
	{
		setStringField( ipActionTransitionSet->Fields, "Exception", strException, true );
	}
	
	// save Comment
	setStringField( ipActionTransitionSet->Fields, "Comment", strComment, true );

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
		if ( ipAction->adoEOF )
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
											  string &rstrAction, const string &strToState, 
											  const string &strException, const string &strComment, 
											  const string &strWhereClause, 
											  const string &strTopClause )
{
	if (!m_bUpdateFASTTable)
	{
		return;
	}

	// Get the action ID and update the strActionName to stored value
	long nActionID = getActionID(ipConnection, rstrAction);

	// Action Column to change
	string strActionCol = "ASC_" + rstrAction;

	// Create the from string
	string strFrom = " FROM FAMFile " + strWhereClause;

	// if the strException string is empty NULL should be added to the db
	string strNewException = (strException.empty()) ? "NULL": "'" + strException + "'";

	// if the strComment is empty the NULL should be added to the database
	string strNewComment = (strComment.empty()) ? "NULL": "'" + strComment + "'";

	// create the insert string
	string strInsertTrans = "INSERT INTO FileActionStateTransition ( FileID, ActionID, ASC_From, "
		"ASC_To, DateTimeStamp, Exception, Comment, FAMUserID, MachineID) ";
	strInsertTrans += "SELECT " + strTopClause + " ID, " + 
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

	// If no records in the data set then will need create a new record
	if ( ipActionStats->adoEOF )
	{
		// Create new record
		ipActionStats->AddNew();

		//  Set Action ID
		setLongField( ipActionStats->Fields, "ActionID", nActionID );
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

	// Go thru each of the records in the Calculation set
	while (!ipCalcStatsSet->adoEOF)
	{
		// Get the action state
		string strActionState = getStringField( ipCalcStatsSet->Fields, strActionColName ); 

		// Set the sums to the appropriate statistics property
		if ( strActionState == "F" )
		{
			// Set Failed totals
			lNumDocsFailed = getLongField( ipCalcStatsSet->Fields, "NumDocs" );
			lNumPagesFailed = getLongField( ipCalcStatsSet->Fields, "SumOfPages" );
			llNumBytesFailed = getLongLongField( ipCalcStatsSet->Fields, "SumOfFileSize" );
		}
		else if ( strActionState == "C" )
		{
			// Set Completed totals
			lNumDocsCompleted = getLongField( ipCalcStatsSet->Fields, "NumDocs" );
			lNumPagesCompleted = getLongField( ipCalcStatsSet->Fields, "SumOfPages" );
			llNumBytesCompleted = getLongLongField( ipCalcStatsSet->Fields, "SumOfFileSize" );
		}
		if ( strActionState != "U" )
		{
			// All values are added to the Totals
			lTotalDocs += getLongField( ipCalcStatsSet->Fields, "NumDocs" );
			lTotalPages += getLongField( ipCalcStatsSet->Fields, "SumOfPages" );
			llTotalBytes += getLongLongField( ipCalcStatsSet->Fields, "SumOfFileSize" );
		}

		// Move to next record
		ipCalcStatsSet->MoveNext();
	}

	// Set Failed totals
	setLongField( ipActionStats->Fields, "NumDocumentsFailed", lNumDocsFailed );
	setLongField( ipActionStats->Fields, "NumPagesFailed", lNumPagesFailed );
	setLongLongField( ipActionStats->Fields, "NumBytesFailed", llNumBytesFailed );

	// Set Completed totals
	setLongField( ipActionStats->Fields, "NumDocumentsComplete", lNumDocsCompleted );
	setLongField( ipActionStats->Fields, "NumPagesComplete", lNumPagesCompleted );
	setLongLongField( ipActionStats->Fields, "NumBytesComplete", llNumBytesCompleted );

	// Save totals in the ActionStatistics table
	setLongField( ipActionStats->Fields, "NumDocuments", lTotalDocs );
	setLongField( ipActionStats->Fields, "NumPages", lTotalPages );
	setLongLongField( ipActionStats->Fields, "NumBytes", llTotalBytes );

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
		vecQueries.push_back(gstrCREATE_FAM_FILE_INDEX);
		vecQueries.push_back(gstrCREATE_QUEUE_EVENT_CODE_TABLE);
		vecQueries.push_back(gstrCREATE_ACTION_STATISTICS_TABLE);
		vecQueries.push_back(gstrCREATE_FILE_ACTION_STATE_TRANSITION_TABLE);
		vecQueries.push_back(gstrCREATE_QUEUE_EVENT_TABLE);
		vecQueries.push_back(gstrCREATE_QUEUE_EVENT_INDEX);
		vecQueries.push_back(gstrCREATE_MACHINE_TABLE);
		vecQueries.push_back(gstrCREATE_FAM_USER_TABLE);

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

	// Execute all of the queries
	executeVectorOfSQL( getDBConnection(), vecQueries);
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::copyActionStatus( ADODB::_ConnectionPtr ipConnection, string strFrom, 
										 string strTo, bool bAddTransRecords)
{
	if ( bAddTransRecords )
	{
		string strTransition = "INSERT INTO FileActionStateTransition "
			"(FileID, ActionID, ASC_From, ASC_To, DateTimeStamp, Comment, FAMUserID, MachineID) "
			"SELECT ID, " + asString(getActionID(ipConnection, strTo)) + " AS ActionID, ASC_" + 
			strTo + ", ASC_" + strFrom + " , GETDATE() AS TS_Trans, 'Copy status from " + 
			strFrom +" to " + strTo + "' AS Comment, " + asString(getFAMUserID(ipConnection)) + 
			", " + asString(getMachineID(ipConnection)) + " FROM FAMFile";

		executeCmdQuery(ipConnection, strTransition);
	}
	string strCopy = "UPDATE FAMFile SET ASC_" + strTo + " = ASC_" + strFrom;
	executeCmdQuery(ipConnection, strCopy);
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
	// Nothing to do if the "from" status == the "to" status
	if (eFromStatus == eToStatus)
	{
		// If the to and from status is unattempted there is nothing to do
		// Otherwise if the FileSize and the number Pages are the same there is nothing to do
		if (eFromStatus == kActionUnattempted ||
			(ipNewRecord != NULL && ipOldRecord != NULL &&
			ipNewRecord->FileSize == ipOldRecord->FileSize && 
			ipNewRecord->Pages == ipOldRecord->Pages))
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
		// Make sure the ipNewRecord is not NULL
		ASSERT_ARGUMENT("ELI17046", ipNewRecord != NULL);
		ipActionStats->NumDocumentsFailed = ipActionStats->NumDocumentsFailed + 1;
		ipActionStats->NumPagesFailed = ipActionStats->NumPagesFailed + ipNewRecord->Pages;
		ipActionStats->NumBytesFailed = ipActionStats->NumBytesFailed + ipNewRecord->FileSize;
		break;
	case kActionCompleted:
		// Make sure the ipNewRecord is not NULL
		ASSERT_ARGUMENT("ELI17048", ipNewRecord != NULL);
		ipActionStats->NumDocumentsComplete = ipActionStats->NumDocumentsComplete + 1;
		ipActionStats->NumPagesComplete = ipActionStats->NumPagesComplete + ipNewRecord->Pages;
		ipActionStats->NumBytesComplete = ipActionStats->NumBytesComplete + ipNewRecord->FileSize;
		break;
	}
	// Add the new counts to the totals if the to status is not unattempted
	if ( eToStatus != kActionUnattempted )
	{
		// Make sure the ipNewRecord is not NULL
		ASSERT_ARGUMENT("ELI17050", ipNewRecord != NULL);
		ipActionStats->NumDocuments = ipActionStats->NumDocuments + 1;
		ipActionStats->NumPages = ipActionStats->NumPages + ipNewRecord->Pages;
		ipActionStats->NumBytes = ipActionStats->NumBytes + ipNewRecord->FileSize;
	}

	switch (eFromStatus)
	{
	case kActionFailed:
		// Make sure the ipOldRecord is not NULL
		ASSERT_ARGUMENT("ELI17052", ipOldRecord != NULL);
		ipActionStats->NumDocumentsFailed = ipActionStats->NumDocumentsFailed - 1;
		ipActionStats->NumPagesFailed = ipActionStats->NumPagesFailed - ipOldRecord->Pages;
		ipActionStats->NumBytesFailed = ipActionStats->NumBytesFailed - ipOldRecord->FileSize;
		break;
	case kActionCompleted:
		// Make sure the ipOldRecord is not NULL
		ASSERT_ARGUMENT("ELI17053", ipOldRecord != NULL);
		ipActionStats->NumDocumentsComplete = ipActionStats->NumDocumentsComplete - 1;
		ipActionStats->NumPagesComplete = ipActionStats->NumPagesComplete - ipOldRecord->Pages;
		ipActionStats->NumBytesComplete = ipActionStats->NumBytesComplete - ipOldRecord->FileSize;
		break;
	}

	// Remove the counts form the totals if the from status is not unattempted
	if ( eFromStatus != kActionUnattempted )
	{
		// Make sure the ipOldRecord is not NULL
		ASSERT_ARGUMENT("ELI17055", ipOldRecord != NULL);
		ipActionStats->NumDocuments = ipActionStats->NumDocuments - 1;
		ipActionStats->NumPages = ipActionStats->NumPages - ipOldRecord->Pages;
		ipActionStats->NumBytes = ipActionStats->NumBytes - ipOldRecord->FileSize;
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
	if ( ipActionStatSet->adoEOF )
	{
		reCalculateStats(ipConnection, nActionID);
		bRecalculated = true;
		ipActionStatSet->Requery(adOptionUnspecified);
	}
	if ( ipActionStatSet->adoEOF )
	{
		UCLIDException ue("ELI14100", "Unable to load statistics.");
		ue.addDebugInfo("ActionID", nActionID);
		throw ue;
	}

	// Create an ActionStatistics pointer to return the values
	UCLID_FILEPROCESSINGLib::IActionStatisticsPtr ipActionStats(CLSID_ActionStatistics);
	ASSERT_RESOURCE_ALLOCATION("ELI14101", ipActionStats != NULL );

	// Transfer the data from the recordset to the ActionStatisticsPtr
	ipActionStats->NumDocumentsFailed =  getLongField(ipActionStatSet->Fields, 
		"NumDocumentsFailed" );
	ipActionStats->NumPagesFailed = getLongField( ipActionStatSet->Fields, "NumPagesFailed" );
	ipActionStats->NumBytesFailed = getLongLongField( ipActionStatSet->Fields, "NumBytesFailed" );
	ipActionStats->NumDocumentsComplete = getLongField( ipActionStatSet->Fields, 
		"NumDocumentsComplete" );
	ipActionStats->NumPagesComplete = getLongField( ipActionStatSet->Fields, "NumPagesComplete" );
	ipActionStats->NumBytesComplete = getLongLongField( ipActionStatSet->Fields, 
		"NumBytesComplete" );
	ipActionStats->NumDocuments = getLongField( ipActionStatSet->Fields, "NumDocuments" );
	ipActionStats->NumPages = getLongField( ipActionStatSet->Fields, "NumPages" );
	ipActionStats->NumBytes = getLongLongField( ipActionStatSet->Fields, "NumBytes" );

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

	// Create an ActionStatistics pointer to return the values
	UCLID_FILEPROCESSINGLib::IActionStatisticsPtr ipActionStats;
	ipActionStats = m_mapActionIDtoStats[nActionID];

	variant_t vtllVal;

	// Transfer the data from the recordset to the ActionStatisticsPtr
	setLongField(ipActionStatSet->Fields, "NumDocumentsFailed", ipActionStats->NumDocumentsFailed);
	setLongField( ipActionStatSet->Fields, "NumPagesFailed", ipActionStats->NumPagesFailed );
	setLongLongField( ipActionStatSet->Fields, "NumBytesFailed", ipActionStats->NumBytesFailed );
	setLongField( ipActionStatSet->Fields, "NumDocumentsComplete", 
		ipActionStats->NumDocumentsComplete );
	setLongField( ipActionStatSet->Fields, "NumPagesComplete", ipActionStats->NumPagesComplete );
	setLongLongField(ipActionStatSet->Fields, "NumBytesComplete", ipActionStats->NumBytesComplete);
	setLongField( ipActionStatSet->Fields, "NumDocuments", ipActionStats->NumDocuments );
	setLongField( ipActionStatSet->Fields, "NumPages", ipActionStats->NumPages );
	setLongLongField( ipActionStatSet->Fields, "NumBytes", ipActionStats->NumBytes );
	ipActionStatSet->Update();

	m_mapActionIDtoStats[nActionID] = ipActionStats;
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
				if ( !ipLockTable->adoEOF )
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
	if ( !ipLoginSet->adoEOF )
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
	if (ipLoginSet->adoEOF)
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
	
	// Get the fileID to return
	ipFileRecord->FileID = getLongField(ipFields, "ID" );

	// Get the file size saved in the file record
	ipFileRecord->FileSize = getLongLongField(ipFields, "FileSize");

	// Get the number of pages saved in the file record
	ipFileRecord->Pages = getLongField(ipFields, "Pages");

	// Set the Name 
	ipFileRecord->Name = getStringField(ipFields, "FileName" ).c_str();

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
	
	// set the file name field
	setStringField(ipFields, "FileName", asString(ipFileRecord->Name));

	// Set the file Size
	setLongLongField(ipFields, "FileSize", ipFileRecord->FileSize);

	// Set the number of pages
	setLongField(ipFields, "Pages", ipFileRecord->Pages);
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
		getThisAsCOMPtr()->Clear();
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
				getThisAsCOMPtr()->ResetDBConnection();

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
