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

#include <string>

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
			CSingleLock retryLock(&m_mutex, TRUE ); \
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
					UCLIDException uex("ELI23631", "Database connection failed. Attempting to reconnect.", ue); \
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
// Constants
//-------------------------------------------------------------------------------------------------
// moved to header file so that it is accessible by 
// multiple files [p13 #4920]
// User name for FAM DB Admin access
//const string gstrADMIN_USER = "admin"; 

//-------------------------------------------------------------------------------------------------
// Static Members
//-------------------------------------------------------------------------------------------------
std::string CFileProcessingDB::ms_strCurrServerName = "";
std::string CFileProcessingDB::ms_strCurrDBName = "";

//-------------------------------------------------------------------------------------------------
// CFileProcessingDB
//-------------------------------------------------------------------------------------------------
CFileProcessingDB::CFileProcessingDB()
: m_iDBSchemaVersion(0),
m_bDBLocked(false),
m_regUserCfgMgr(HKEY_CURRENT_USER, ""),
m_regFPCfgMgr(&m_regUserCfgMgr, "\\FileProcessingDB"),
m_hUIWindow(NULL),
m_strCurrentConnectionStatus(gstrNOT_CONNECTED),
m_strDatabaseServer(""),
m_strDatabaseName(""),
m_lFAMUserID(0),
m_lMachineID(0),
m_iCommandTimeout(glDEFAULT_COMMAND_TIMEOUT),
m_bUpdateQueueEventTable(true),
m_bUpdateFASTTable(true),
m_bAutoDeleteFileActionComment(false),
m_iNumberOfRetries(giDEFAULT_RETRY_COUNT),
m_dRetryTimeout(gdDEFAULT_RETRY_TIMEOUT)
{
	try
	{
		// Check if license files have been loaded - this is here to so that
		// the Database config COM object can be used from C#
		if (!LicenseManagement::sGetInstance().filesLoadedFromFolder())
		{
			// Load the license files
			LicenseManagement::sGetInstance().loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);
		}

		// set the Unique Process Identifier string to be used to in locking the database
		m_strUPI = UPI::getCurrentProcessUPI().getUPI();
		m_strMachineName = getComputerName();
		m_strFAMUserName = getCurrentUserName();
		m_lDBLockTimeout = m_regFPCfgMgr.getDBLockTimeout();

		// If PDF support is licensed initialize support
		// NOTE: no exception is thrown or logged if PDF support is not licensed.
		initPDFSupport();

		// Post message indicating that the database's connection is not yet established
		postStatusUpdateNotification(kConnectionNotEstablished);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI15341")
}
//-------------------------------------------------------------------------------------------------
CFileProcessingDB::~CFileProcessingDB()
{
	// Need to catch any exceptions and log them because this could be called within a catch
	// and don't want to throw an exception from a catch
	try
	{
		// Clean up the map of connections
		m_mapThreadIDtoDBConnections.clear();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI14981");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IFileProcessingDB,
		&IID_ILicensedComponent
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IFileProcessingDB Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::DefineNewAction( BSTR strAction,  long * pnID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();
		
		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Check License
		validateLicense();

		// Lock the database for this instance
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		// Make sure the DB Schema is the expected version
		validateDBSchemaVersion();

		string strActionName = asString(strAction);

		// Begin a trasaction
		TransactionGuard tg(ipConnection);

		// Create a pointer to a recordset
		_RecordsetPtr ipActionSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI13517", ipActionSet != NULL );

		// Setup select statement to open Action Table
		string strActionSelect = "Select ASCName From Action Where ASCName = '" + strActionName + "'";

		// Open the Action table in the database
		ipActionSet->Open( strActionSelect.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenDynamic, 
			adLockOptimistic, adCmdText );

		// Check to see if action exists
		if ( ipActionSet->adoEOF == VARIANT_FALSE )
		{
			// Build error string (P13 #3931)
			CString zText;
			zText.Format( "The action '%s' already exists, and therefore cannot be added again.", 
				strActionName.c_str() );
			UCLIDException ue("ELI13946", zText.operator LPCTSTR());
			throw ue;
		}

		// Add a new record
		ipActionSet->AddNew();

		// Set the values of the ASCName field
		setStringField(ipActionSet->Fields,"ASCName", strActionName);

		// Add the record to the Action Table
		ipActionSet->Update();

		// Get and return the ID of the new Action
		*pnID = getLastTableID(ipConnection, "Action");

		// Add column to the FAMFile table
		addActionColumn(strActionName);

		// Commit this transaction
		tg.CommitTrans();

		END_CONNECTION_RETRY(ipConnection, "ELI23524");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13524");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::DeleteAction( BSTR strAction)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
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
		_RecordsetPtr ipActionSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI13528", ipActionSet != NULL );

		// Open the Action table
		ipActionSet->Open( "Action", _variant_t((IDispatch *)ipConnection, true), adOpenDynamic, 
			adLockOptimistic, adCmdTableDirect );

		// Begin a transaction
		TransactionGuard tg(ipConnection);

		// Setup find criteria to find the action to delete
		string strFind = "ASCName = '" + asString(strAction) + "'";

		// Search for the action to delete
		ipActionSet->Find(strFind.c_str(), 0, adSearchForward);

		// if action was found
		if ( ipActionSet->adoEOF == VARIANT_FALSE )
		{
			// Get the action name from the database
			strActionName = getStringField(ipActionSet->Fields, "ASCName");

			// Delete the record 
			ipActionSet->Delete( adAffectCurrent );

			// Remove column from FAMFile
			removeActionColumn(strActionName);

			// Commit the change to the database
			tg.CommitTrans();
		}
		END_CONNECTION_RETRY(ipConnection, "ELI23525");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13527");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetActions( IStrToStrMap * * pmapActionNameToID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
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
		_RecordsetPtr ipActionSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI13530", ipActionSet != NULL );

		// Open the Action table
		ipActionSet->Open( "Action", _variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
			adLockReadOnly, adCmdTableDirect );

		// Create StrToStrMap to return the list of actions
		IStrToStrMapPtr ipActions( CLSID_StrToStrMap );
		ASSERT_RESOURCE_ALLOCATION("ELI13529", ipActions != NULL );

		// Step through all records
		while ( ipActionSet->adoEOF == VARIANT_FALSE )
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
			ipActions->Set( strActionName.c_str(), strID.c_str() );

			// Move to the next record in the table
			ipActionSet->MoveNext();
		}

		// return the StrToStrMap containing all actions
		*pmapActionNameToID = ipActions.Detach();
		END_CONNECTION_RETRY(ipConnection, "ELI23526");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13531");

	return S_OK;

}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::AddFile( BSTR strFile,  BSTR strAction, VARIANT_BOOL bForceStatusChange, 
										VARIANT_BOOL bFileModified, EActionStatus eNewStatus,
										VARIANT_BOOL * pbAlreadyExists, EActionStatus *pPrevStatus, IFileRecord* * ppFileRecord)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	INIT_EXCEPTION_AND_TRACING("MLI00006");

	try
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
		
		_lastCodePos = "10";
		
		// Replace any occurences of ' with '' this is because SQL Server use the ' to indicate the beginning and end of a string
		string strFileName = asString(strFile);
		replaceVariable(strFileName, "'", "''" );

		// Open a recordset that contain only the record (if it exists ) with the given filename
		string strFileSQL = "SELECT * FROM FAMFile WHERE FileName = '" + strFileName + "'";

		// Create a pointer to a recordset
		_RecordsetPtr ipFileSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI13535", ipFileSet != NULL );

		_lastCodePos = "20";

		ipFileSet->Open( strFileSQL.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenDynamic, 
			adLockOptimistic, adCmdText );

		_lastCodePos = "30";

		// put the unaltered file name back in the strFileName variable
		strFileName = asString(strFile);

		// Create the file record to return
		UCLID_FILEPROCESSINGLib::IFileRecordPtr ipNewFileRecord(CLSID_FileRecord);
		ASSERT_RESOURCE_ALLOCATION("ELI14203", ipNewFileRecord != NULL );

		// Initialize the id
		long nID = 0;

		// Begin a transaction
		TransactionGuard tg(ipConnection);

		_lastCodePos = "40";

		// Set the action name from the parameter
		string strActionName = asString(strAction);
		
		// Get the action ID and update the strActionName to stored value
		long nActionID = getActionID(ipConnection, strActionName);

		// Action Column to update
		string strActionCol = "ASC_" + strActionName;

		// Get the size of the file
		// [LRCAU #5157] - getSizeOfFile performs a wait for file access call, no need
		// to perform an additional call here.
		long long llFileSize;
		llFileSize = (long long )getSizeOfFile( strFileName );

		// get the file type
		EFileType efType = getFileType(strFileName);
		long nPages = 0;

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
		// Set the number of pages in return record
		ipNewFileRecord->Pages = nPages;

		// Set the Name in return record
		ipNewFileRecord->Name = strFileName.c_str();

		// Set the FileSize in return record
		ipNewFileRecord->FileSize = llFileSize;

		_lastCodePos = "50";

		string strNewStatus = asStatusString(eNewStatus);

		// If no records were returned a new record should be added to the FAMFile
		if ( ipFileSet->BOF == VARIANT_TRUE )
		{
			// The filename is not in the table
			*pbAlreadyExists = VARIANT_FALSE;

			// Add new record
			ipFileSet->AddNew();

			// Get the fields from the file set
			FieldsPtr ipFields = ipFileSet->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI26872", ipFields != NULL);

			// Set the fields from the new file record
			setFieldsFromFileRecord(ipFields, ipNewFileRecord);

			// set the initial Action state to pending
			setStringField( ipFields, strActionCol, strNewStatus );

			_lastCodePos = "60";

			// Add the record
			ipFileSet->Update();
			
			_lastCodePos = "70";

			// get the new records ID to return
			nID = getLastTableID( ipConnection, "FAMFile" );

			// return the previous state as Unattempted
			*pPrevStatus = kActionUnattempted;

			_lastCodePos = "80";

			// update the statistics
			updateStats( ipConnection, nActionID, *pPrevStatus, eNewStatus, ipNewFileRecord, NULL);
			_lastCodePos = "90";
		}
		else
		{
			// The file name is in the database
			*pbAlreadyExists = VARIANT_TRUE;

			// Get the fields from the file set
			FieldsPtr ipFields = ipFileSet->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI26873", ipFields != NULL);

			// Get the file record from the fields
			UCLID_FILEPROCESSINGLib::IFileRecordPtr ipOldRecord;
			ipOldRecord = getFileRecordFromFields(ipFields);

			// Set the Current file Records ID
			nID = ipOldRecord->FileID;

			_lastCodePos = "100";

			// Get the last action status to return
			string strStatus = getStringField(ipFields, strActionCol );

			// Set the Previous status return var
			*pPrevStatus = asEActionStatus( strStatus );

			_lastCodePos = "100.1";

			// if Force processing is set need to update the status or if the previous status for this action was unattempted
			if ( bForceStatusChange == VARIANT_TRUE || *pPrevStatus == kActionUnattempted )
			{
				_lastCodePos = "100.2";

				// if the previous state is "R" it should not be changed
				// TODO: Handle the "R" case so that they will be marked as pending after the processing has completed
				if ( *pPrevStatus == kActionProcessing )
				{
					UCLIDException ue("ELI15043", "Cannot force status from Processing.");
					ue.addDebugInfo("File", strFileName );
					ue.addDebugInfo("Action Name", strActionName);
					ue.log();
					throw ue;
				}

				// set the fields to the new file Record
				setFieldsFromFileRecord(ipFields, ipNewFileRecord);

				// set the Action state to the new status
				setStringField(ipFields, strActionCol, strNewStatus );

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
				// same as the new status
				if ( *pPrevStatus != kActionUnattempted && *pPrevStatus != eNewStatus  )
				{
					_lastCodePos = "130";

					// Only update FileActionStateTransition table if required
					if (m_bUpdateFASTTable)
					{
						addFileActionStateTransition(ipConnection, nID, nActionID, strStatus.c_str(), 
							strNewStatus, "", "" );
					}

					_lastCodePos = "140";
				}
				// update the statistics
				updateStats( ipConnection, nActionID, *pPrevStatus, eNewStatus, ipNewFileRecord, ipOldRecord );

				_lastCodePos = "150";
			}
		}

		// Set the new file Record ID to nID;
		ipNewFileRecord->FileID = nID;

		_lastCodePos = "150.1";

		// Update QueueEvent table if enabled
		if (m_bUpdateQueueEventTable)
		{
			// add a new QueueEvent record 
			addQueueEventRecord( ipConnection, nID, asString(strFile), ( bFileModified == VARIANT_TRUE ) ? "M":"A");
		}

		_lastCodePos = "160";

		// Commit the changes to the database
		tg.CommitTrans();

		_lastCodePos = "170";

		// Return the file record
		*ppFileRecord = (IFileRecord*)ipNewFileRecord.Detach();

		END_CONNECTION_RETRY(ipConnection, "ELI23527");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13536");
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::RemoveFile( BSTR strFile, BSTR strAction )
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
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
		_RecordsetPtr ipFileSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI13537", ipFileSet != NULL );

		// Replace any occurances of ' with '' this is because SQL Server use the ' to indicate the beginning and end of a string
		string strFileName = asString(strFile);
		replaceVariable(strFileName, "'", "''" );

		// Open a recordset that contain only the record (if it exists ) with the given filename
		string strFileSQL = "SELECT * FROM FAMFile WHERE FileName = '" + strFileName + "'";
		ipFileSet->Open( strFileSQL.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenDynamic, 
			adLockOptimistic, adCmdText );

		// Begin a transaction
		TransactionGuard tg(ipConnection);

		// Setup action name and action id
		string strActionName = asString(strAction);
		long nActionID = getActionID(ipConnection, strActionName);

		// If file exists this should not be at end of file
		if ( ipFileSet->adoEOF == VARIANT_FALSE )
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
			string strActionState = getStringField( ipFields, strActionCol );

			// only change the state if the current state is pending
			if ( strActionState == "P" )
			{
				// change state to unattempted
				setStringField( ipFields, strActionCol, "U" );

				ipFileSet->Update();

				// Only update FileActionStateTransition Table if required
				if (m_bUpdateFASTTable)
				{
					// Add a ActionStateTransition record for the state change
					addFileActionStateTransition( ipConnection, nFileID, nActionID, strActionState, "U", "", "Removed" );
				}

				// update the statistics
				updateStats( ipConnection, nActionID, asEActionStatus(strActionState), kActionUnattempted, NULL, ipOldRecord); 
			}

			// Update QueueEvent table if enabled
			if (m_bUpdateQueueEventTable)
			{
				// add record the QueueEvent table to indicate that the file was deleted
				addQueueEventRecord( ipConnection, nFileID, asString(strFile), "D" );
			}
		}

		// Commit the changes
		tg.CommitTrans();

		END_CONNECTION_RETRY(ipConnection, "ELI23528");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13538");
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::NotifyFileProcessed( long nFileID,  BSTR strAction)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check License
		validateLicense();

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();
		
		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// change the given files state to completed
		setFileActionState( ipConnection, nFileID, asString(strAction), "C", "" );

		END_CONNECTION_RETRY(ipConnection, "ELI23529");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13541");

	return S_OK;}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::NotifyFileFailed( long nFileID,  BSTR strAction,  BSTR strException)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check License
		validateLicense();

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();
		
		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// change the given files state to Failed
		setFileActionState( ipConnection, nFileID, asString(strAction), "F", asString(strException) );

		END_CONNECTION_RETRY(ipConnection, "ELI23530");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13544");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetFileStatusToPending( long nFileID,  BSTR strAction)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check License
		validateLicense();

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();
		
		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// change the given files state to Pending
		setFileActionState( ipConnection, nFileID, asString(strAction), "P", "" );

		END_CONNECTION_RETRY(ipConnection, "ELI23531");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13546");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetFileStatusToUnattempted( long nFileID,  BSTR strAction)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check License
		validateLicense();

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();
		
		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// change the given files state to unattempted
		setFileActionState( ipConnection, nFileID, asString(strAction), "U", "" );

		END_CONNECTION_RETRY(ipConnection, "ELI23532");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13548");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetFileStatusToSkipped(long nFileID, BSTR strAction,
													   BSTR bstrUniqueProcessID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check License
		validateLicense();

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();
		
		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Change the given files state to Skipped
		setFileActionState(ipConnection, nFileID, asString(strAction), "S", "", -1, true,
			asString(bstrUniqueProcessID));

		END_CONNECTION_RETRY(ipConnection, "ELI26938");

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26939");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetFileStatus( long nFileID,  BSTR strAction,  EActionStatus * pStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
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
		_RecordsetPtr ipFileSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI13551", ipFileSet != NULL );

		// Open Recordset that contains only the record with the given ID
		string strFileSQL = "SELECT * FROM FAMFile WHERE ID = " + asString (nFileID);
		ipFileSet->Open( strFileSQL.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
			adLockOptimistic, adCmdText );

		// Set the action name from the parameter
		string strActionName = asString(strAction);
		
		// Get the action ID and update the strActionName to stored value
		long nActionID = getActionID(ipConnection, strActionName);

		// Action Column to update
		string strActionCol = "ASC_" + strActionName;
		// if the file exists should not be at the end of the file
		if ( ipFileSet->adoEOF == VARIANT_FALSE )
		{
			// Set return value to the current Action Status
			string strStatus = getStringField( ipFileSet->Fields, strActionCol );
			*pStatus = asEActionStatus( strStatus );
		}
		else
		{
			// File ID did not exist
			UCLIDException ue("ELI13553", "File ID was not found." );
			ue.addDebugInfo ( "File ID", nFileID );
			throw ue;
		}
		END_CONNECTION_RETRY(ipConnection, "ELI23533");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13550");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SearchAndModifyFileStatus( long nWhereActionID,  EActionStatus eWhereStatus,  
														  long nToActionID, EActionStatus eToStatus,
														  BSTR bstrSkippedFromUserName, 
														  long nFromActionID, long * pnNumRecordsModified)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check License
		validateLicense();

		// Changing an Action status to failed should only be done on an individual file bases
		if ( eToStatus == kActionFailed )
		{
			UCLIDException ue ("ELI13603", "Cannot change status Failed.");
			throw ue;
		}

		// If the to status is not skipped, the from status is the same as the to status
		// and the Action ids are the same, there is nothing to do
		// If setting skipped status the skipped file table needs to be updated
		if ( eToStatus != kActionSkipped
			&& eWhereStatus == eToStatus
			&& nToActionID == nWhereActionID)
		{
			// nothing to do
			return S_OK;
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

		// Get a vector of FileIDS to modify
		vector<long> vecFileIDs;

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
			adOpenForwardOnly, adLockReadOnly, adCmdText );

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
				nToActionID, false);

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13565");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetStatusForAllFiles( BSTR strAction,  EActionStatus eStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
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

		if ( eStatus == kActionFailed )
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
		addASTransFromSelect( ipConnection, strActionName, nActionID, asStatusString( eStatus ),
			"", "", strWhere, "" );

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13571");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetStatusForFile( long nID,  BSTR strAction,  EActionStatus eStatus,  
												 EActionStatus * poldStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check License
		validateLicense();

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();
		
		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// change the status for the given file and return the previous state
		*poldStatus = setFileActionState( ipConnection, nID, asString(strAction), asStatusString( eStatus ), "" );

		END_CONNECTION_RETRY(ipConnection, "ELI23536");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13572");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetFilesToProcess( BSTR strAction,  long nMaxFiles, 
												  VARIANT_BOOL bGetSkippedFiles,
												  BSTR bstrSkippedForUserName,
												  BSTR bstrUniqueProcessID,
												  IIUnknownVector * * pvecFileRecords)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
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

		string strWhere = "";
		string strTop = "TOP (" + asString( nMaxFiles ) + " ) ";
		if (bGetSkippedFiles == VARIANT_TRUE)
		{
			strWhere = " INNER JOIN SkippedFile ON FAMFile.ID = SkippedFile.FileID "
				"WHERE ( SkippedFile.ActionID = " + asString(nActionID)
				+ " AND FAMFile." + strActionCol + " = 'S'";

			string strUserName = asString(bstrSkippedForUserName);
			if(!strUserName.empty())
			{
				replaceVariable(strUserName, "'", "''");
				string strUserAnd = " AND SkippedFile.UserName = '" + strUserName + "'";
				strWhere += strUserAnd;
			}

			string strUniqueProcessID = asString(bstrUniqueProcessID);
			if (!strUniqueProcessID.empty())
			{
				replaceVariable(strUniqueProcessID, "'", "''");
				strWhere += " AND SkippedFile.UniqueFAMID <> '";
				strWhere += strUniqueProcessID;
				strWhere += "'";
			}

			strWhere += " )";
		}
		else
		{
			strWhere = "WHERE (" + strActionCol + " = 'P')";
		}
		string strFrom = "FROM FAMFile " + strWhere;
			
		// create query to select top records;
		string strSelectSQL = "SELECT " + strTop + " FAMFile.ID, FileName, Pages, FileSize " + strFrom;

		// Create the query to update the status to processing
		string strUpdateSQL = "UPDATE " + strTop + " FAMFile SET " + strActionCol + " = 'R' " + strFrom;

		// IUnknownVector to hold the FileRecords to return
		IIUnknownVectorPtr ipFiles( CLSID_IUnknownVector );
		ASSERT_RESOURCE_ALLOCATION("ELI19504", ipFiles != NULL );

		// Begin a transaction
		TransactionGuard tg(ipConnection);

		// Recordset to contain the files to process
		_RecordsetPtr ipFileSet(__uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI13573", ipFileSet != NULL );

		// get the recordset with the top nMaxFiles 
		ipFileSet->Open(strSelectSQL.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenForwardOnly, 
			adLockReadOnly, adCmdText );

		// Fill the ipFiles collection
		while ( ipFileSet->adoEOF == VARIANT_FALSE )
		{
			UCLID_FILEPROCESSINGLib::IFileRecordPtr ipFileRecord;

			// Get the file Record from the fields
			ipFileRecord = getFileRecordFromFields(ipFileSet->Fields);

			// Put record in list of records to return
			ipFiles->PushBack(ipFileRecord);

			// move to the next record in the recordset
			ipFileSet->MoveNext();
		}
		// Add transition records for the state change to Processing
		addASTransFromSelect( ipConnection, strActionName, nActionID, "R", "", "", strWhere, strTop );

		// Update the status of the selected FAMFiles records
		executeCmdQuery(ipConnection, strUpdateSQL);

		// Commit the changes to the database
		tg.CommitTrans();

		// return the vector of file records
		*pvecFileRecords = ipFiles.Detach();

		END_CONNECTION_RETRY(ipConnection, "ELI23537");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13574");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::RemoveFolder( BSTR strFolder, BSTR strAction )
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
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
		replaceVariable(strFolderName, "'", "''" );

		// set up the where clause to find the pending records that the filename begins with the folder name
		string strWhere = "WHERE (" + strActionCol + " = 'P') AND ( FileName LIKE '" + strFolderName + "%')";
		string strFrom = "FROM FAMFile " + strWhere;

		// Set up the SQL to update the FAMFile
		string strUpdateSQL = "UPDATE FAMFile SET " + strActionCol + " = 'U' " + strFrom;

		// Begin a transaction
		TransactionGuard tg(ipConnection);

		// add transition records to the databse
		addASTransFromSelect( ipConnection, strActionName, nActionID, "U", "", "", strWhere, "" );

		// Only update the QueueEvent table if update is enabled
		if (m_bUpdateQueueEventTable)
		{
			// Set up the SQL to add the queue event records
			string strInsertQueueRecords = "INSERT INTO QueueEvent ( FileID, DateTimeStamp, QueueEventCode, FAMUserID, MachineID ) ";

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13611");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetStats(/*[in]*/ long nActionID, /*[out, retval]*/ IActionStatistics* *pStats)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
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

		// Begin a transaction
		TransactionGuard tg(ipConnection);
	
		// Load stats from DB
		loadStats(ipConnection, nActionID);

		// Commit any changes (could have recreated the stats)
		tg.CommitTrans();
	
		// Create an ActionStatistics pointer to return the values
		ICopyableObjectPtr ipCopyObj = m_mapActionIDtoStats[nActionID];
		ASSERT_RESOURCE_ALLOCATION("ELI14633", ipCopyObj != NULL );

		// return a new object with the statistics
		UCLID_FILEPROCESSINGLib::IActionStatisticsPtr ipActionStats =  ipCopyObj->Clone();
		ASSERT_RESOURCE_ALLOCATION("ELI14107", ipActionStats != NULL );

		// Return the value
		*pStats = (IActionStatistics *)ipActionStats.Detach();

		END_CONNECTION_RETRY(ipConnection, "ELI23539");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14045")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::CopyActionStatusFromAction( long  nFromAction, long nToAction )
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
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

		string strFrom = getActionName(ipConnection, nFromAction);
		string strTo = getActionName(ipConnection, nToAction);

		TransactionGuard tg(ipConnection);

		// Copy Action status and only update the FAST table if required
		copyActionStatus(ipConnection, strFrom, strTo, m_bUpdateFASTTable, nToAction);

		// update the stats for the to action
		reCalculateStats(ipConnection, nToAction);

		// Commit the transaction
		tg.CommitTrans();

		END_CONNECTION_RETRY(ipConnection, "ELI23540");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14097");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::RenameAction( long nActionID, BSTR strNewActionName )
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
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
		addActionColumn(strNew);

		// Copy status from the old column without transition records (and without update skipped table)
		copyActionStatus(ipConnection, strOld, strNew, false);

		// Change the name of the action in the action table
		string strSQL = "UPDATE Action SET ASCName = '" + strNew + "' WHERE ID = " + asString(nActionID);
		executeCmdQuery(ipConnection, strSQL);

		// Remove the old action column from FMPFile table
		removeActionColumn(strOld);

		// Commit the transaction
		tg.CommitTrans();

		END_CONNECTION_RETRY(ipConnection, "ELI23541");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19505");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::Clear()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();
	
		// Call the internal clear
		clear();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14088");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::ExportFileList(BSTR strQuery, BSTR strOutputFileName, long *pnNumRecordsOutput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI23522", pnNumRecordsOutput != NULL);

		// check for empty query string
		string strSQL = asString(strQuery);
		if ( strSQL.empty() )
		{
			UCLIDException ue("ELI14724", "Query string is empty.");
			throw ue;
		}
		// Check if output file name is not empty
		string strOutFileName = asString(strOutputFileName);
		if ( strOutFileName.empty())
		{
			UCLIDException ue("ELI14727", "Output file name is blank.");
			throw ue;
		}

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

		// Recordset to contain the files to process
		_RecordsetPtr ipFileSet(__uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI14725", ipFileSet != NULL );

		// get the recordset with the top nMaxFiles 
		ipFileSet->Open(strSQL.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenForwardOnly, 
			adLockReadOnly, adCmdText );
		// Open the output file
		ofstream ofsOutput(strOutFileName.c_str(), ios::out | ios::trunc);

		// Setup the counter for the number of records
		long nNumRecords = 0;

		// Fill the ipFiles collection
		while ( ipFileSet->adoEOF == VARIANT_FALSE )
		{
			// Get the FileName
			string strFile = getStringField( ipFileSet->Fields, "FileName" );
			ofsOutput << strFile << endl;

			// increment the number of records
			nNumRecords++;
			ipFileSet->MoveNext();
		}
		ofsOutput.flush();
		ofsOutput.close();
		waitForFileToBeReadable(strOutFileName);
		
		// return the number of records
		*pnNumRecordsOutput	= nNumRecords;

		END_CONNECTION_RETRY(ipConnection, "ELI23542");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14726");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::ResetDBLock(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();
		
		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// The mutex only needs to be locked while the data is being obtained
		CSingleLock lock(&m_mutex, TRUE );

		// Check License
		validateLicense();

		// Make sure the DB Schema is the expected version
		validateDBSchemaVersion();

		// Begin Transaction
		TransactionGuard tg(ipConnection);

		// Delete all Lock records
		executeCmdQuery(ipConnection, gstrDELETE_DB_LOCK);
		
		// Commit the changes
		tg.CommitTrans();
		
		END_CONNECTION_RETRY(ipConnection, "ELI23543");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14799")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetActionID( BSTR bstrActionName, long* pnActionID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI24027", pnActionID != NULL);

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();
		
		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Lock the database
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		// Get the action ID
		*pnActionID = getActionID(ipConnection, asString(bstrActionName));

		END_CONNECTION_RETRY(ipConnection, "ELI23544");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14986")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::ResetDBConnection()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		// Validate the license
		validateLicense();

		// Call the internal reset db connection
		resetDBConnection();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19507")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetNotificationUIWndHandle(long nHandle)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// update the internal window handle to send UI notifications to
		m_hUIWindow = (HWND) nHandle;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14989")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::ShowAdminLogin(VARIANT_BOOL* pbLoginCancelled, 
											   VARIANT_BOOL* pbLoginValid)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI20471", pbLoginCancelled != NULL);
		ASSERT_ARGUMENT("ELI20472", pbLoginValid != NULL);

		// Initialize to no new password
		bool bNewPassword = false;

		// Set login valid and cancelled to false
		*pbLoginValid = VARIANT_FALSE;
		*pbLoginCancelled = VARIANT_FALSE;

		// Initialize the DB if it is blank
		initializeIfBlankDB();

		// Get the stored password ( if it exists)
		string strStoredEncryptedCombined = getEncryptedAdminPWFromDB();

		// if there is no password will need to get the new password
		if ( strStoredEncryptedCombined == "" )
		{
			// default to using the desktop as the parent for the messagebox below
			HWND hParent = getAppMainWndHandle();

			::MessageBox(hParent, "This is the first time you are logging into this File Action Manager database.\r\n\r\n"
				"You will be prompted to set the admin password in the next screen.  The admin password\r\n"
				"will be required to login into the database before any actions can be performed on the\r\n"
				"database from this application.\r\n\r\n"
				"Please keep your admin password in a safe location and share it only with people capable\r\n"
				"of administering the File Action Manager database.  Please note that anyone with access\r\n"
				"to the admin password will be able to use this application to execute data-deleting\r\n"
				"commands such as removing rows in tables, or emptying out the entire database.\r\n\r\n"
				"Click OK to continue to the next screen where you will be prompted to set the "
				"admin password.", "Set Admin Password", MB_ICONINFORMATION | MB_APPLMODAL);

			PasswordDlg dlgPW("Set Admin Password");
			if ( dlgPW.DoModal() != IDOK )
			{
				// Did not fill in and ok dlg so there is no login
				// Set Cancelled flag
				*pbLoginCancelled = VARIANT_TRUE;
				return S_OK;
			}

			// Update password in database Login table
			bNewPassword = true;
			string strPassword = dlgPW.m_zNewPassword;
			string strCombined = gstrADMIN_USER + strPassword;
			encryptAndStoreUserNamePassword( strCombined );

			// Just added password to the db so it is valid
			*pbLoginValid = VARIANT_TRUE;
			return S_OK;
		}

		// Set read-only user name to "admin" (P13 #4112)
		CLoginDlg dlgLogin( "Login", gstrADMIN_USER, true );
		if ( dlgLogin.DoModal() != IDOK )
		{
			// The OK button on the login dialog was not pressed so do not login
			// Set Cancelled flag
			*pbLoginCancelled = VARIANT_TRUE;
			return S_OK;
		}

		// Validate password
		*pbLoginValid = asVariantBool(isAdminPasswordValid(string(dlgLogin.m_zPassword)));
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15099");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::get_DBSchemaVersion(LONG* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI15149", pVal != NULL );

		*pVal = getDBSchemaVersion();

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15148");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::ChangeAdminLogin(VARIANT_BOOL* pbChangeCancelled, 
												 VARIANT_BOOL* pbChangeValid)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Default the Valid and Cancelled flags to false
		*pbChangeValid = VARIANT_FALSE;
		*pbChangeCancelled = VARIANT_FALSE;

		// Get and check the stored password
		string strStoredEncryptedCombined = getEncryptedAdminPWFromDB();
		if (strStoredEncryptedCombined == "")
		{
			// Create and throw exception
			UCLIDException ue( "ELI15721", "Cannot change password if no password is defined!" );
			throw ue;
		}

		bool bPasswordValid = false;
		// Display Change Password dialog
		ChangePasswordDlg dlgPW("Change Admin Password");
		do
		{
			if ( dlgPW.DoModal() != IDOK )
			{
				// Did not fill in and ok dlg so there is no login
				// Set Cancelled flag and return
				*pbChangeCancelled = VARIANT_TRUE;
				return S_OK;
			}
			bPasswordValid = isAdminPasswordValid(string(dlgPW.m_zOldPassword));
			
			// If the password is not valid display a dialog
			if (!bPasswordValid)
			{
				// default to using the desktop as the parent for the messagebox below
				HWND hParent = getAppMainWndHandle();
				dlgPW.m_zOldPassword = "";
				::MessageBox(hParent, "Old password is not correct. Please try again.", "Login failed!", MB_ICONINFORMATION | MB_APPLMODAL);
			}
		}
		while (!bPasswordValid);

		// Encrypt and store the user name and password in the Login table
		string strPassword = dlgPW.m_zNewPassword;
		string strCombined = gstrADMIN_USER + strPassword;
		encryptAndStoreUserNamePassword( strCombined );

		// Just added the new password to the db so it is valid
		*pbChangeValid = VARIANT_TRUE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15720");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetCurrentConnectionStatus(BSTR* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17566", pVal != NULL);

		// Don't return the current connection in the middle of a reset event [FlexIDSCore #3463]
		string strResult = m_strCurrentConnectionStatus;
		if (strResult == gstrNOT_CONNECTED)
		{
			CSingleLock lg(&m_mutex, TRUE);
			strResult = m_strCurrentConnectionStatus;
		}

		*pVal = get_bstr_t(strResult).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16167");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::put_DatabaseServer(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		m_strDatabaseServer = asString(newVal);

		// Set the static server name
		CSingleLock lock(&m_mutex, TRUE);
		ms_strCurrServerName = m_strDatabaseServer;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17621");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::get_DatabaseServer(BSTR* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		ASSERT_ARGUMENT("ELI17564", pVal != NULL);

		*pVal = get_bstr_t(m_strDatabaseServer).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17468");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::put_DatabaseName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		m_strDatabaseName = asString(newVal);

		// Set the  static Database name
		CSingleLock lock(&m_mutex, TRUE);
		ms_strCurrDBName = m_strDatabaseName;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17622");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::get_DatabaseName(BSTR* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		ASSERT_ARGUMENT("ELI17565", pVal != NULL);

		*pVal = get_bstr_t(m_strDatabaseName).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17623");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::CreateNewDB(BSTR bstrNewDBName)
{	
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Close any existing connection. P13 #4666
		closeDBConnection();

		// Database server needs to be set in order to create a new database
		if ( m_strDatabaseServer.empty())
		{
			UCLIDException ue("ELI17470", "Database server must be set!");
			ue.addDebugInfo("New DB name", asString(bstrNewDBName));
			throw ue;
		}

		// Set the database name to the given database name
		m_strDatabaseName = asString(bstrNewDBName);
		
		if (m_strDatabaseName.empty())
		{
			UCLIDException ue("ELI18327", "Database name must not be empty!");
			throw ue;
		}
		
		// Create a connection object to the master db to create the database
		ADODB::_ConnectionPtr ipDBConnection( __uuidof( Connection ) ); 

		// Open a connection to the the master database on the database server
		ipDBConnection->Open(createConnectionString(m_strDatabaseServer, "master").c_str(),
			"", "", adConnectUnspecified);

		// Query to create the database
		string strCreateDB = "CREATE DATABASE [" + m_strDatabaseName + "]";

		// Execute the query to create the new database
		ipDBConnection->Execute(strCreateDB.c_str(), NULL, adCmdText | adExecuteNoRecords );

		// Close the connections
		ipDBConnection->Close();

		// Clear the new database to set up the tables
		clear();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17469");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::ConnectLastUsedDBThisProcess()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		CSingleLock lock(&m_mutex, TRUE);

		// Set the active settings to the saved static settings
		m_strDatabaseServer = ms_strCurrServerName;
		m_strDatabaseName  = ms_strCurrDBName;

		resetDBConnection();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17842");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetDBInfoSetting(BSTR bstrSettingName, BSTR bstrSettingValue )
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		validateLicense();

		// Lock the database for this instance
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		// Make sure the DB Schema is the expected version
		validateDBSchemaVersion();

		// Create a pointer to a recordset
		_RecordsetPtr ipDBInfoSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI19792", ipDBInfoSet != NULL );

		// Convert setting name and value to string 
		string strSettingName = asString(bstrSettingName);
		string strSettingValue = asString(bstrSettingValue);

		// Setup Setting Query
		string strSQL = gstrDBINFO_SETTING_QUERY;
		replaceVariable(strSQL, gstrSETTING_NAME, strSettingName);
		
		// Begin Transaction
		TransactionGuard tg(getDBConnection());
		
		// Open recordset for the DBInfo Settings
		ipDBInfoSet->Open(strSQL.c_str(), _variant_t((IDispatch *)getDBConnection(), true), adOpenDynamic, 
			adLockOptimistic, adCmdText ); 

		// Check if setting record exist
		if (ipDBInfoSet->adoEOF == VARIANT_TRUE)
		{
			// Setting does not exist so add it
			ipDBInfoSet->AddNew();
			setStringField(ipDBInfoSet->Fields, "Name", strSettingName, true);
		}

		// Set the value field to the new value
		setStringField(ipDBInfoSet->Fields, "Value", strSettingValue);

		// Update the database
		ipDBInfoSet->Update();

		// Commit transaction
		tg.CommitTrans();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18936");
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetDBInfoSetting(BSTR bstrSettingName, BSTR* pbstrSettingValue )
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		ASSERT_ARGUMENT("ELI18938", pbstrSettingValue != NULL);

		validateLicense();

		// Lock the database for this instance
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		// Make sure the DB Schema is the expected version
		validateDBSchemaVersion();

		// Create a pointer to a recordset
		_RecordsetPtr ipDBInfoSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI19793", ipDBInfoSet != NULL );

		// Convert Setting name to string
		string strSettingName = asString(bstrSettingName);
		
		// Setup Setting Query
		string strSQL = gstrDBINFO_SETTING_QUERY;
		replaceVariable(strSQL, gstrSETTING_NAME, strSettingName);
		
		// Open the record set using the Setting Query		
		ipDBInfoSet->Open(strSQL.c_str(), _variant_t((IDispatch *)getDBConnection(), true), adOpenStatic, 
			adLockReadOnly, adCmdText ); 

		// Check if any data returned
		if (ipDBInfoSet->adoEOF == VARIANT_FALSE)
		{
			// Return the setting value
			*pbstrSettingValue = get_bstr_t(getStringField(ipDBInfoSet->Fields, "Value")).Detach();
		}
		else
		{
			UCLIDException ue("ELI18940", "DBInfo setting does not exist!");
			ue.addDebugInfo("Setting", strSettingName);
			throw  ue;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18937");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::LockDB()
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		// Post message indicating that we are trying to lock the database
		postStatusUpdateNotification(kWaitingForLock);

		// lock the database
		lockDB(getDBConnection());

		// Post message indicating that the database is now busy
		postStatusUpdateNotification(kConnectionBusy);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19084");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::UnlockDB()
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		CSingleLock lg(&m_mutex, TRUE);

		// Need to catch any exceptions and log them because this could be called within a catch
		// and don't want to throw an exception from a catch
		try
		{
			// Unlock the DB
			unlockDB(getDBConnection());
		}
		catch(...)
		{
			// Post message indicating that the database is back to connection-established status
			postStatusUpdateNotification(kConnectionEstablished);
			throw;
		}

		// Post message indicating that the database is back to connection-established status
		postStatusUpdateNotification(kConnectionEstablished);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19095");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetResultsForQuery(BSTR bstrQuery, _Recordset** ppVal)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		ASSERT_ARGUMENT("ELI19881", ppVal != NULL);

		validateLicense();

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();
		
		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Create a pointer to a recordset
		_RecordsetPtr ipResultSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI19876", ipResultSet != NULL );

		// Make sure the DB Schema is the expected version
		validateDBSchemaVersion();

		// Open the Action table
		ipResultSet->Open( bstrQuery, _variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
			adLockReadOnly, adCmdText );

		*ppVal = ipResultSet.Detach();
		
		END_CONNECTION_RETRY(ipConnection, "ELI23547");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19875");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::AsStatusString(EActionStatus eaStatus, BSTR *pbstrStatusString)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		validateLicense();

		ASSERT_ARGUMENT("ELI19899", pbstrStatusString != NULL);

		*pbstrStatusString = get_bstr_t(asStatusString(eaStatus)).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19897");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::AsEActionStatus(BSTR bstrStatus, EActionStatus *peaStatus)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		validateLicense();

		ASSERT_ARGUMENT("ELI19900", peaStatus != NULL);

		*peaStatus = asEActionStatus(asString(bstrStatus));
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19898");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetFileID(BSTR bstrFileName, long *pnFileID)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		validateLicense();

		ASSERT_ARGUMENT("ELI24028", pnFileID != NULL);

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();
		
		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Lock the database
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		// Get the file ID
		*pnFileID = getFileID(ipConnection, asString(bstrFileName));

		END_CONNECTION_RETRY(ipConnection, "ELI24029");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24030");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetActionName(long nActionID, BSTR *pbstrActionName)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		validateLicense();

		ASSERT_ARGUMENT("ELI26769", pbstrActionName != NULL);

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();

		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Lock the database
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		// Get the action name from the database
		string strActionName = getActionName(ipConnection, nActionID);

		// Return the action name
		*pbstrActionName = _bstr_t(strActionName.c_str()).Detach();

		END_CONNECTION_RETRY(ipConnection, "ELI26770");

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26771");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::NotifyFileSkipped(long nFileID, long nActionID,
												  BSTR bstrUniqueProcessID)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		validateLicense();

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();

		ipConnection = getDBConnection();

		// Get the action name
		string strActionName = getActionName(ipConnection, nActionID);

		// Set the file state to skipped
		setFileActionState(ipConnection, nFileID, strActionName, "S", "", nActionID, true,
			asString(bstrUniqueProcessID));

		END_CONNECTION_RETRY(ipConnection, "ELI26778");

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26779");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetFileActionComment(long nFileID, long nActionID, BSTR bstrComment)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		validateLicense();

		// Get the comment
		string strComment = asString(bstrComment);
		replaceVariable(strComment, "'", "''");

		// Get the current user name
		string strUserName = getCurrentUserName();

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();

		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Lock the database
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		validateDBSchemaVersion();

		string strCommentSQL = "SELECT * FROM FileActionComment WHERE FileID = "
			+ asString(nFileID) + " AND ActionID = " + asString(nActionID);

		_RecordsetPtr ipCommentSet(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI26788", ipCommentSet != NULL);

		ipCommentSet->Open(strCommentSQL.c_str(), _variant_t((IDispatch*)ipConnection, true), adOpenDynamic,
			adLockOptimistic, adCmdText);

		// Begin a transaction
		TransactionGuard tg(ipConnection);

		// If no records returned then there is no comment for this pair currently
		// add the new comment to the table (do not add empty comments)
		if (ipCommentSet->BOF == VARIANT_TRUE)
		{
			if (!strComment.empty())
			{
				ipCommentSet->AddNew();

				// Get the fields pointer
				FieldsPtr ipFields = ipCommentSet->Fields;
				ASSERT_RESOURCE_ALLOCATION("ELI26789", ipFields != NULL);

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
				ASSERT_RESOURCE_ALLOCATION("ELI26790", ipFields != NULL);

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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26773");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetFileActionComment(long nFileID, long nActionID,
													 BSTR* pbstrComment)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		validateLicense();

		ASSERT_ARGUMENT("ELI26792", pbstrComment != NULL);

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		// Default the comment to empty string
		string strComment = "";

		BEGIN_CONNECTION_RETRY();

		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Lock the database
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		validateDBSchemaVersion();

		string strCommentSQL = "SELECT * FROM FileActionComment WHERE FileID = "
			+ asString(nFileID) + " AND ActionID = " + asString(nActionID);

		_RecordsetPtr ipCommentSet(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI26793", ipCommentSet != NULL);

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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26775");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::ClearFileActionComment(long nFileID, long nActionID)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		validateLicense();

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

		// Clear the comment
		clearFileActionComment(ipConnection, nFileID, nActionID);

		// Commit the transaction
		tg.CommitTrans();

		END_CONNECTION_RETRY(ipConnection, "ELI26776");

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26777");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::ModifyActionStatusForQuery(BSTR bstrQueryFrom, BSTR bstrToAction,
														   EActionStatus eaStatus, BSTR bstrFromAction)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check that an action name and a FROM clause have been passed in
		string strQueryFrom = asString(bstrQueryFrom);
		ASSERT_ARGUMENT("ELI27037", !strQueryFrom.empty());
		string strToAction = asString(bstrToAction);
		ASSERT_ARGUMENT("ELI27038", !strToAction.empty());

		string strFromAction = asString(bstrFromAction);
		bool bFromSpecified = !strFromAction.empty();

		validateLicense();

		// Build the file set query
		string strFileQuery = "SELECT FAMFile.ID";
		string strStatus = "";
		if (bFromSpecified)
		{
			strFromAction = "FAMFile.ASC_" + strFromAction;
			strFileQuery += ", " + strFromAction;
		}
		else
		{
			// Get the new status as a string
			strStatus = asStatusString(eaStatus);
		}
		strFileQuery += " FROM " + strQueryFrom;


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
		ipFileSet->Open(strFileQuery.c_str(), _variant_t((IDispatch*)ipConnection, true),
			adOpenForwardOnly, adLockReadOnly, adCmdText);

		// Loop through each record
		while (ipFileSet->adoEOF == VARIANT_FALSE)
		{
			FieldsPtr ipFields = ipFileSet->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI27040", ipFields != NULL);

			// Get the file ID
			long nFileID = getLongField(ipFields, "ID");

			// If copying from an action, get the status for the action
			if (bFromSpecified)
			{
				strStatus = getStringField(ipFields, strFromAction);
			}

			// Set the file action state
			setFileActionState(ipConnection, nFileID, strToAction, strStatus, "", -1, false);

			// Move to next record
			ipFileSet->MoveNext();
		}

		// Commit the transaction
		tg.CommitTrans();

		END_CONNECTION_RETRY(ipConnection, "ELI27041");

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26982");
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pbValue == NULL)
		{
			return E_POINTER;
		}

		// Check license
		validateLicense();

		// If no exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
