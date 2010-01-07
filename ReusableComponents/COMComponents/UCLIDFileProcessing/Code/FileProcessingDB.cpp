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
m_ipParser(NULL),
m_dRetryTimeout(gdDEFAULT_RETRY_TIMEOUT),
m_nUPIID(0)
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

		// Ensure the parser is released
		m_ipParser = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI14981");
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::FinalRelease()
{
	try
	{
		// Ensure the parser is released
		m_ipParser = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27324");
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
		addActionColumn(ipConnection, strActionName);

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
			removeActionColumn(ipConnection, strActionName);

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
STDMETHODIMP CFileProcessingDB::AddFile( BSTR strFile,  BSTR strAction, EFilePriority ePriority,
										VARIANT_BOOL bForceStatusChange, VARIANT_BOOL bFileModified,
										EActionStatus eNewStatus, VARIANT_BOOL * pbAlreadyExists,
										EActionStatus *pPrevStatus, IFileRecord* * ppFileRecord)
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

		// Set the priority in return record
		ipNewFileRecord->Priority = (UCLID_FILEPROCESSINGLib::EFilePriority) ePriority;

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

			// Set the priority
			setLongField(ipFields, "Priority",
				ePriority == kPriorityDefault ? glDEFAULT_FILE_PRIORITY : (long) ePriority);

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
			UCLID_FILEPROCESSINGLib::IFileRecordPtr ipOldRecord = getFileRecordFromFields(ipFields);
			ASSERT_RESOURCE_ALLOCATION("ELI27657", ipOldRecord != NULL);

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
					throw ue;
				}

				// set the fields to the new file Record
				// (only update the priority if force processing)
				setFieldsFromFileRecord(ipFields, ipNewFileRecord, asCppBool(bForceStatusChange));

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
			addQueueEventRecord( ipConnection, nID, nActionID, asString(strFile), ( bFileModified == VARIANT_TRUE ) ? "M":"A");
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
				addQueueEventRecord( ipConnection, nFileID, nActionID, asString(strFile), "D" );
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
													   VARIANT_BOOL bRemovePreviousSkipped)
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
			asCppBool(bRemovePreviousSkipped));

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
		string strActionID = asString(nActionID);

		string strUPIID = asString(m_nUPIID);

		// Action Column to change
		string strActionCol = "ASC_" + strActionName;

		string strWhere = "";
		string strTop = "TOP (" + asString( nMaxFiles ) + " ) ";
		if (bGetSkippedFiles == VARIANT_TRUE)
		{
			strWhere = " INNER JOIN SkippedFile ON FAMFile.ID = SkippedFile.FileID "
				"WHERE ( SkippedFile.ActionID = " + strActionID
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

			strWhere += " )";
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

		// IUnknownVector to hold the FileRecords to return
		IIUnknownVectorPtr ipFiles( CLSID_IUnknownVector );
		ASSERT_RESOURCE_ALLOCATION("ELI19504", ipFiles != NULL );

		// Begin a transaction
		TransactionGuard tg(ipConnection);

		if (m_bAutoRevertLockedFiles)
		{
			revertTimedOutProcessingFAMs(ipConnection);
		}

		// Get the from state for the queries
		string strFromState = bGetSkippedFiles ? "S" : "P";

		// Get the machine and user ID
		string strMachineID = asString(getMachineID(ipConnection));
		string strUserID = asString(getFAMUserID(ipConnection));

		// Create query to create records in the LockedFile table
		string strLockedTableSQL =
			"INSERT INTO LockedFile (FileID, ActionID, UPIID, StatusBeforeLock) SELECT FAMFile.ID, "
			+ strActionID + ", " + strUPIID + ", '" + strFromState + "' FROM FAMFile WHERE "
			" FAMFile.ID IN (";

		// Recordset to contain the files to process
		_RecordsetPtr ipFileSet(__uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI13573", ipFileSet != NULL );

		// get the recordset with the top nMaxFiles 
		ipFileSet->Open(strSelectSQL.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenDynamic, 
			adLockPessimistic, adCmdText );

		// Fill the ipFiles collection, also update the FAMFile table and build
		// the queries to update both the FAST table and the Locked file table
		string strFileIDIn = "";
		while ( ipFileSet->adoEOF == VARIANT_FALSE )
		{
			FieldsPtr ipFields = ipFileSet->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI28234", ipFields != NULL);

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
			strFileIDIn += asString(ipFileRecord->FileID);

			// move to the next record in the recordset
			ipFileSet->MoveNext();
		}

		// Check whether any file IDs have been added to the string
		if (!strFileIDIn.empty())
		{
			strFileIDIn += ")";

			// Update the FAST table if necessary
			if (m_bUpdateFASTTable)
			{
				// Create query to update the FAST table
				string strFASTSql =
					"INSERT INTO " + gstrFILE_ACTION_STATE_TRANSITION + " (FileID, ActionID, "
					"ASC_From, ASC_To, DateTimeStamp, FAMUserID, MachineID, Exception, "
					"Comment) SELECT FAMFile.ID, " + strActionID + ", '" + strFromState
					+ "', 'R', GETDATE(), " + strUserID + ", " + strMachineID
					+ ", NULL, NULL FROM FAMFile WHERE FAMFile.ID IN (" + strFileIDIn;

				executeCmdQuery(ipConnection, strFASTSql);
			}

			// Update the lock table
			executeCmdQuery(ipConnection, strLockedTableSQL + strFileIDIn);
		}

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
STDMETHODIMP CFileProcessingDB::ExportFileList(BSTR strQuery, BSTR strOutputFileName,
											   IRandomMathCondition* pRandomCondition, long *pnNumRecordsOutput)
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

		// Wrap the random math condition in smart pointer
		UCLID_FILEPROCESSINGLib::IRandomMathConditionPtr ipRandomCondition(pRandomCondition);

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
			if (ipRandomCondition == NULL || ipRandomCondition->CheckCondition("", NULL) == VARIANT_TRUE)
			{
				// Get the FileName
				string strFile = getStringField( ipFileSet->Fields, "FileName" );
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
STDMETHODIMP CFileProcessingDB::ShowLogin(VARIANT_BOOL bShowAdmin, VARIANT_BOOL* pbLoginCancelled, 
											   VARIANT_BOOL* pbLoginValid)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI20471", pbLoginCancelled != NULL);
		ASSERT_ARGUMENT("ELI20472", pbLoginValid != NULL);

		// Convert bShowAdmin parameter to cpp bool for later use
		bool bUseAdmin  = asCppBool(bShowAdmin);
		string strUser = (bUseAdmin) ? gstrADMIN_USER : m_strFAMUserName;
		
		// Set the user password
		string strCaption = "Set " + m_strFAMUserName + "'s Password";

		// Set login valid and cancelled to false
		*pbLoginValid = VARIANT_FALSE;
		*pbLoginCancelled = VARIANT_FALSE;

		// Initialize the DB if it is blank
		initializeIfBlankDB();

		// Get the stored password ( if it exists)
		string strStoredEncryptedCombined;
		bool bUserExists = getEncryptedPWFromDB(strStoredEncryptedCombined, bUseAdmin);

		// if there is no password will need to get the new password
		if ( strStoredEncryptedCombined == "" && bUseAdmin)
		{
			// Set the admin password
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
		}
		else if (!bUserExists)
		{
			// default to using the desktop as the parent for the messagebox below
			HWND hParent = getAppMainWndHandle();

			::MessageBox(hParent, "The system is not configured to allow you to perform this operation.\r\n"
				"Please contact the administrator of this product for further assistance.", "Unable to Login", MB_OK);
			*pbLoginValid = VARIANT_FALSE;
			*pbLoginCancelled = VARIANT_TRUE;
			return S_OK;
		}

		// If no password set then set a password
		if (strStoredEncryptedCombined == "")
		{
			PasswordDlg dlgPW(strCaption);
			if ( dlgPW.DoModal() != IDOK )
			{
				// Did not fill in and ok dlg so there is no login
				// Set Cancelled flag
				*pbLoginCancelled = VARIANT_TRUE;
				return S_OK;
			}

			// Update password in database Login table
			string strPassword = dlgPW.m_zNewPassword;
			string strCombined = strUser + strPassword;
			encryptAndStoreUserNamePassword(strCombined, bUseAdmin);

			// Just added password to the db so it is valid
			*pbLoginValid = VARIANT_TRUE;
			return S_OK;
		}

		// Set read-only user name to "admin" (P13 #4112) or user's name
		CLoginDlg dlgLogin( "Login", (bUseAdmin) ? gstrADMIN_USER : m_strFAMUserName , true );
		if ( dlgLogin.DoModal() != IDOK )
		{
			// The OK button on the login dialog was not pressed so do not login
			// Set Cancelled flag
			*pbLoginCancelled = VARIANT_TRUE;
			return S_OK;
		}

		// Validate password
		*pbLoginValid = asVariantBool(isPasswordValid(string(dlgLogin.m_zPassword), bUseAdmin));
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
STDMETHODIMP CFileProcessingDB::ChangeLogin(VARIANT_BOOL bChangeAdmin, VARIANT_BOOL* pbChangeCancelled, 
												 VARIANT_BOOL* pbChangeValid)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Default the Valid and Cancelled flags to false
		*pbChangeValid = VARIANT_FALSE;
		*pbChangeCancelled = VARIANT_FALSE;

		bool bUseAdmin = asCppBool(bChangeAdmin);

		// Get and check the stored password
		string strStoredEncryptedCombined;
		getEncryptedPWFromDB(strStoredEncryptedCombined, asCppBool(bChangeAdmin));

		if (strStoredEncryptedCombined == "")
		{
			// Create and throw exception
			UCLIDException ue( "ELI15721", "Cannot change password if no password is defined!" );
			throw ue;
		}

		bool bPasswordValid = false;
		string strUser = bUseAdmin ? gstrADMIN_USER : m_strFAMUserName;
		string strPasswordDlgCaption = "Change " + strUser + " Password";
		
		// Display Change Password dialog

		ChangePasswordDlg dlgPW(strPasswordDlgCaption);
		do
		{
			if ( dlgPW.DoModal() != IDOK )
			{
				// Did not fill in and ok dlg so there is no login
				// Set Cancelled flag and return
				*pbChangeCancelled = VARIANT_TRUE;
				return S_OK;
			}
			bPasswordValid = isPasswordValid(string(dlgPW.m_zOldPassword), bUseAdmin);
			
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
		string strCombined = strUser + strPassword;
		encryptAndStoreUserNamePassword(strCombined, bUseAdmin);

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

		// Convert setting name and value to string 
		string strSettingName = asString(bstrSettingName);
		string strSettingValue = asString(bstrSettingValue);

		// Setup Setting Query
		string strSQL = gstrDBINFO_SETTING_QUERY;
		replaceVariable(strSQL, gstrSETTING_NAME, strSettingName);
		
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
		_RecordsetPtr ipDBInfoSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI19792", ipDBInfoSet != NULL );

		// Begin Transaction
		TransactionGuard tg(ipConnection);
		
		// Open recordset for the DBInfo Settings
		ipDBInfoSet->Open(strSQL.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenDynamic, 
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

		END_CONNECTION_RETRY(ipConnection, "ELI27328");
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

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();
		
		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Lock the database for this instance
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		// Make sure the DB Schema is the expected version
		validateDBSchemaVersion();

		// Get the setting
		string strSetting = getDBInfoSetting(ipConnection, asString(bstrSettingName));

		// Set the return value
		*pbstrSettingValue = _bstr_t(strSetting.c_str()).Detach();

		END_CONNECTION_RETRY(ipConnection, "ELI27327");
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
STDMETHODIMP CFileProcessingDB::NotifyFileSkipped(long nFileID, long nActionID)
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
		setFileActionState(ipConnection, nFileID, strActionName, "S", "", nActionID, true);

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
														   EActionStatus eaStatus, BSTR bstrFromAction,
														   IRandomMathCondition* pRandomCondition,
														   long* pnNumRecordsModified)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
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

		// Loop through each record
		long nNumRecordsModified = 0;
		while (ipFileSet->adoEOF == VARIANT_FALSE)
		{
			if (ipRandomCondition == NULL || ipRandomCondition->CheckCondition("", NULL) == VARIANT_TRUE)
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
				setFileActionState(ipConnection, nFileID, strToAction, strStatus, "", -1, false, true);
				nNumRecordsModified++;
			}

			// Move to next record
			ipFileSet->MoveNext();
		}

		// Commit the transaction
		tg.CommitTrans();

		// Set the return value if it is specified
		if (pnNumRecordsModified != NULL)
		{
			*pnNumRecordsModified = nNumRecordsModified;
		}

		END_CONNECTION_RETRY(ipConnection, "ELI27041");

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26982");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetTags(IStrToStrMap **ppTags)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI27329", ppTags != NULL);

		// Create a map to hold the return values
		IStrToStrMapPtr ipTagToDesc(CLSID_StrToStrMap);
		ASSERT_RESOURCE_ALLOCATION("ELI27330", ipTagToDesc != NULL);

		// Create query to get the tags and descriptions
		string strQuery = "SELECT [TagName], [TagDescription] FROM [Tag]";

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();

		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Lock the database
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		validateDBSchemaVersion();

		// Create a pointer to a recordset
		_RecordsetPtr ipTagSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI27331", ipTagSet != NULL );

		// Open Recordset that contains all the tags and their descriptions
		ipTagSet->Open( strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true),
			adOpenForwardOnly, adLockReadOnly, adCmdText );

		// Add each tag and description to the map
		while (ipTagSet->adoEOF == VARIANT_FALSE)
		{
			FieldsPtr ipFields = ipTagSet->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI27332", ipFields != NULL);

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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27334");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetTagNames(IVariantVector **ppTagNames)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI27335", ppTagNames != NULL);

		IVariantVectorPtr ipVecTags(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI27336", ipVecTags != NULL);

		string strQuery = "SELECT [TagName] FROM [Tag]";

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();

		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Lock the database
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		validateDBSchemaVersion();

		// Create a pointer to a recordset
		_RecordsetPtr ipTagSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI27337", ipTagSet != NULL );

		// Open Recordset that contains the tag names
		ipTagSet->Open( strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true),
			adOpenForwardOnly, adLockReadOnly, adCmdText );

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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27339");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::HasTags(VARIANT_BOOL* pvbVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI27340", pvbVal != NULL);

		bool bHasTags = false;

		string strQuery = "SELECT TOP 1 [TagName] FROM [Tag]";

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();

		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Lock the database
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		validateDBSchemaVersion();

		// Create a pointer to a recordset
		_RecordsetPtr ipTagSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI27341", ipTagSet != NULL );

		// Open Recordset that contains the tag names
		ipTagSet->Open( strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true),
			adOpenForwardOnly, adLockReadOnly, adCmdText );

		// Check if there is at least 1 tag
		bHasTags = ipTagSet->adoEOF == VARIANT_FALSE;

		END_CONNECTION_RETRY(ipConnection, "ELI27342");

		*pvbVal = asVariantBool(bHasTags);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27343");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::TagFile(long nFileID, BSTR bstrTagName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		string strTagName = asString(bstrTagName);
		validateTagName(strTagName);

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

		// Validate the file ID
		validateFileID(ipConnection, nFileID);

		// Get the tag ID (this will also validate the ID)
		long nTagID = getTagID(ipConnection, strTagName);

		string strQuery = "SELECT [FileID], [TagID] FROM [FileTag] WHERE [FileID] = "
			+ asString(nFileID) + " AND [TagID] = " + asString(nTagID);

		// Create a pointer to a recordset
		_RecordsetPtr ipTagSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI27344", ipTagSet != NULL );

		// Open Recordset that contains the tag names
		ipTagSet->Open( strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenDynamic, 
			adLockOptimistic, adCmdText );

		// Only need to add a record if one does not already exist
		if(ipTagSet->adoEOF == VARIANT_TRUE)
		{
			// Add a new record
			ipTagSet->AddNew();

			// Get the fields pointer
			FieldsPtr ipFields = ipTagSet->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI27345", ipFields != NULL);

			setLongField(ipFields, "FileID", nFileID);
			setLongField(ipFields, "TagID", nTagID);

			ipTagSet->Update();
		}

		tg.CommitTrans();

		END_CONNECTION_RETRY(ipConnection, "ELI27346");

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27347");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::UntagFile(long nFileID, BSTR bstrTagName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		string strTagName = asString(bstrTagName);
		validateTagName(strTagName);

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

		// Validate the file ID
		validateFileID(ipConnection, nFileID);

		// Get the tag ID (this will also validate the ID)
		long nTagID = getTagID(ipConnection, strTagName);

		string strQuery = "SELECT [FileID], [TagID] FROM [FileTag] WHERE [FileID] = "
			+ asString(nFileID) + " AND [TagID] = " + asString(nTagID);

		// Create a pointer to a recordset
		_RecordsetPtr ipTagSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI27348", ipTagSet != NULL );

		// Open Recordset that contains the tag names
		ipTagSet->Open( strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenDynamic, 
			adLockOptimistic, adCmdText );

		// Only need to remove the record if one exists
		if(ipTagSet->adoEOF == VARIANT_FALSE)
		{
			// Delete this record
			ipTagSet->Delete(adAffectCurrent);

			ipTagSet->Update();
		}

		tg.CommitTrans();

		END_CONNECTION_RETRY(ipConnection, "ELI27349");

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27350");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::ToggleTagOnFile(long nFileID, BSTR bstrTagName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		string strTagName = asString(bstrTagName);
		validateTagName(strTagName);

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

		// Validate the file ID
		validateFileID(ipConnection, nFileID);

		// Get the tag ID (this will also validate the ID)
		long nTagID = getTagID(ipConnection, strTagName);

		string strQuery = "SELECT [FileID], [TagID] FROM [FileTag] WHERE [FileID] = "
			+ asString(nFileID) + " AND [TagID] = " + asString(nTagID);

		// Create a pointer to a recordset
		_RecordsetPtr ipTagSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI27351", ipTagSet != NULL );

		// Open Recordset that contains the tag names
		ipTagSet->Open( strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenDynamic, 
			adLockOptimistic, adCmdText );

		// If record does not exist, add it
		if (ipTagSet->adoEOF == VARIANT_TRUE)
		{
			// Add a new record
			ipTagSet->AddNew();

			// Get the fields pointer
			FieldsPtr ipFields = ipTagSet->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI27352", ipFields != NULL);

			setLongField(ipFields, "FileID", nFileID);
			setLongField(ipFields, "TagID", nTagID);

		}
		// Record does exist, remove it
		else
		{
			ipTagSet->Delete(adAffectCurrent);
		}	

		// Update the table
		ipTagSet->Update();

		tg.CommitTrans();

		END_CONNECTION_RETRY(ipConnection, "ELI27353");

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27354");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::AddTag(BSTR bstrTagName, BSTR bstrTagDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Get the tag name
		string strTagName = asString(bstrTagName);

		// Validate the tag name
		validateTagName(strTagName);

		// Get the description
		string strDescription = asString(bstrTagDescription);

		string strQuery = "SELECT [TagName], [TagDescription] FROM [Tag] WHERE [TagName] = '"
			+ strTagName + "'";

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

		// Create a pointer to a recordset
		_RecordsetPtr ipTagSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI27355", ipTagSet != NULL );

		// Open Recordset that contains the tag names
		ipTagSet->Open( strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenDynamic, 
			adLockOptimistic, adCmdText );

		if (ipTagSet->adoEOF == VARIANT_FALSE)
		{
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
			ASSERT_RESOURCE_ALLOCATION("ELI27357", ipFields != NULL);

			// Set the fields
			setStringField(ipFields, "TagName", strTagName);
			setStringField(ipFields, "TagDescription", strDescription);

			// Update the table
			ipTagSet->Update();
		}

		tg.CommitTrans();

		END_CONNECTION_RETRY(ipConnection, "ELI27358");

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27359");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::DeleteTag(BSTR bstrTagName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Get the tag name
		string strTagName = asString(bstrTagName);
		validateTagName(strTagName);
		
		// Build the query
		string strQuery = "SELECT * FROM [Tag] WHERE [TagName] = '" + strTagName + "'";


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

		// Create a pointer to a recordset
		_RecordsetPtr ipTagSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI27418", ipTagSet != NULL );

		// Open Recordset that contains the tag names
		ipTagSet->Open( strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenDynamic, 
			adLockOptimistic, adCmdText );

		if (ipTagSet->adoEOF == VARIANT_FALSE)
		{
			// Delete the current record
			ipTagSet->Delete(adAffectCurrent);

			// Update the table
			ipTagSet->Update();
		}

		tg.CommitTrans();

		END_CONNECTION_RETRY(ipConnection, "ELI27365");

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27366");

}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::ModifyTag(BSTR bstrOldTagName, BSTR bstrNewTagName,
										  BSTR bstrNewTagDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

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

		string strQuery = "SELECT [TagName], [TagDescription] FROM [Tag] WHERE [TagName] = '"
			+ strOldTagName + "'";

		// Create a pointer to a recordset
		_RecordsetPtr ipTagSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI27362", ipTagSet != NULL );

		// Open Recordset that contains the tag names
		ipTagSet->Open( strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenDynamic, 
			adLockOptimistic, adCmdText );

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
		ASSERT_RESOURCE_ALLOCATION("ELI27364", ipFields != NULL);

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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27420");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetFilesWithTags(IVariantVector* pvecTagNames,
												 VARIANT_BOOL vbAndOperation,
												 IVariantVector** ppvecFileIDs)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Check arguments
		IVariantVectorPtr ipVecTagNames(pvecTagNames);
		ASSERT_ARGUMENT("ELI27367", ipVecTagNames != NULL);
		ASSERT_ARGUMENT("ELI27368", ppvecFileIDs != NULL);

		// Create the vector to return the file IDs
		IVariantVectorPtr ipVecFileIDs(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI27369", ipVecFileIDs != NULL);

		// Get the size of the vector of tag names
		long lSize = ipVecTagNames->Size;

		// If no tags specified return empty collection
		if (lSize == 0)
		{
			// Set the return value
			*ppvecFileIDs = ipVecFileIDs.Detach();

			return S_OK;
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
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();

		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Lock the database
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		validateDBSchemaVersion();

		// Create a pointer to a recordset
		_RecordsetPtr ipTagSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI27370", ipTagSet != NULL );

		// Open Recordset that contains the file IDs
		ipTagSet->Open( strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true),
			adOpenForwardOnly, adLockReadOnly, adCmdText );

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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27372");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetTagsOnFile(long nFileID, IVariantVector** ppvecTagNames)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Check argument
		ASSERT_ARGUMENT("ELI27373", ppvecTagNames != NULL);

		// Build the sql string
		string strQuery = "SELECT DISTINCT [Tag].[TagName] FROM [FileTag] INNER JOIN "
			"[Tag] ON [FileTag].[TagID] = [Tag].[ID] WHERE [FileTag].[FileID] = ";
		strQuery += asString(nFileID) + " ORDER BY [Tag].[TagName]";

		// Create the vector to return the tag names
		IVariantVectorPtr ipVecTagNames(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI27374", ipVecTagNames != NULL);

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();

		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Lock the database
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		validateDBSchemaVersion();

		// Validate the file ID
		validateFileID(ipConnection, nFileID);

		// Create a pointer to a recordset
		_RecordsetPtr ipTagSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI27375", ipTagSet != NULL );

		// Open Recordset that contains the file IDs
		ipTagSet->Open( strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true),
			adOpenForwardOnly, adLockReadOnly, adCmdText );

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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27377");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::AllowDynamicTagCreation(VARIANT_BOOL* pvbVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI27378", pvbVal != NULL);

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();

		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Lock the database
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		// Get the allow dynamic tag creation setting value
		string strSetting = getDBInfoSetting(ipConnection, gstrALLOW_DYNAMIC_TAG_CREATION);

		// Set the out value
		*pvbVal = strSetting == "1" ? VARIANT_TRUE : VARIANT_FALSE;

		END_CONNECTION_RETRY(ipConnection, "ELI27379");

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27380");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetStatusForFilesWithTags(IVariantVector *pvecTagNames,
														  VARIANT_BOOL vbAndOperation,
														  long nToActionID,
														  EActionStatus eaNewStatus,
														  long nFromActionID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		IVariantVectorPtr ipVecTagNames(pvecTagNames);
		ASSERT_ARGUMENT("ELI27427", ipVecTagNames != NULL);

		long lSize = ipVecTagNames->Size;

		// If no tags specified do nothing
		if (lSize == 0)
		{
			return S_OK;
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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27431");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetPriorities(IVariantVector** ppvecPriorities)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI27596", ppvecPriorities != NULL);

		IVariantVectorPtr ipVecPriorities(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI27597", ipVecPriorities != NULL);

		// Add the priority levels to the vector
		ipVecPriorities->PushBack("Low");
		ipVecPriorities->PushBack("Below Normal");
		ipVecPriorities->PushBack("Normal");
		ipVecPriorities->PushBack("Above Normal");
		ipVecPriorities->PushBack("High");

		// Set the return value
		*ppvecPriorities = ipVecPriorities.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27595");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::AsPriorityString(EFilePriority ePriority, BSTR *pbstrPriority)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI27671", pbstrPriority != NULL);

		*pbstrPriority = _bstr_t(
			getPriorityString((UCLID_FILEPROCESSINGLib::EFilePriority)ePriority).c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27672");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::AsEFilePriority(BSTR bstrPriority, EFilePriority *pePriority)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI27673", pePriority != NULL);

		*pePriority = (EFilePriority) getPriorityFromString(asString(bstrPriority));

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27674");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::ExecuteCommandQuery(BSTR bstrQuery, long* pnRecordsAffected)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Get the query string and ensure it is not empty
		string strQuery = asString(bstrQuery);
		ASSERT_ARGUMENT("ELI27684", !strQuery.empty());

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();

		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Lock the database
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		// Set the transaction guard
		TransactionGuard tg(ipConnection);

		// Execute the query
		long nRecordsAffected = executeCmdQuery(ipConnection, strQuery);

		// If user wants a count of affected records, return it
		if (pnRecordsAffected != NULL)
		{
			*pnRecordsAffected = nRecordsAffected;
		}

		// Commit the transaction
		tg.CommitTrans();

		END_CONNECTION_RETRY(ipConnection, "ELI27685");

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27686");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::RegisterProcessingFAM()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// This creates a record in the ProcessingFAM table and the LastPingTime
		// is set to the current time by default.
		m_nUPIID = getKeyID(getDBConnection(), "ProcessingFAM", "UPI", m_strUPI);

		m_eventStopPingThread.reset();
		m_eventPingThreadExited.reset();

		// Start thread here
		AfxBeginThread(maintainLastPingTimeForRevert, this);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27726");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::UnregisterProcessingFAM()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		// Stop thread here
		m_eventStopPingThread.signal();

		// Wait for exit event at least the Ping timeout 
		if (m_eventPingThreadExited.wait(gnPING_TIMEOUT) == WAIT_TIMEOUT)
		{
			UCLIDException ue("ELI27857", "Application Trace: Timed out waiting for thread to exit.");
			ue.log();
		}

		// Set the transaction guard
		TransactionGuard tg(getDBConnection());

		// Make sure there are no linked records in the LockedFile table 
		// and if there are records reset there status to StatusBeforeLock if there current
		// state for the action is processing.
		revertLockedFilesToPreviousState(getDBConnection(), m_nUPIID);

		// Reset m_nUPIID to 0 to specify that it is not registered.
		m_nUPIID = 0;

		// Commit the transaction
		tg.CommitTrans();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27728");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetPriorityForFiles(BSTR bstrSelectQuery, EFilePriority eNewPriority,
													IRandomMathCondition *pRandomCondition,
													long *pnNumRecordsModified)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Get the query string and ensure it is not empty
		string strQuery = asString(bstrSelectQuery);
		ASSERT_ARGUMENT("ELI27710", !strQuery.empty());

		// Wrap the random condition (if there is one, in a smart pointer)
		UCLID_FILEPROCESSINGLib::IRandomMathConditionPtr ipRandomCondition(pRandomCondition);

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();

		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Lock the database
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		// Make sure the DB Schema is the expected version
		validateDBSchemaVersion();

		// Set the transaction guard
		TransactionGuard tg(ipConnection);

		// Recordset to search for file IDs
		_RecordsetPtr ipFileSet(__uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI27711", ipFileSet != NULL );

		// Get the recordset for the specified select query
		ipFileSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenForwardOnly, 
			adLockReadOnly, adCmdText );

		// Setup the counter for the number of records
		long nNumRecords = 0;

		// Build a list of file ID's to set
		stack<string> stackIDs;
		while ( ipFileSet->adoEOF == VARIANT_FALSE )
		{
			if (ipRandomCondition == NULL || ipRandomCondition->CheckCondition("", NULL) == VARIANT_TRUE)
			{
				// Get the file ID
				stackIDs.push(asString(getLongField(ipFileSet->Fields, "ID")));
			}

			ipFileSet->MoveNext();
		}

		// Loop through the IDs setting the file priority
		string strPriority = asString(eNewPriority == kPriorityDefault ?
							glDEFAULT_FILE_PRIORITY : (long)eNewPriority);
		while(!stackIDs.empty())
		{
			// Build the update query
			string strUpdateQuery = "UPDATE FAMFile SET Priority = " + strPriority
				+ " WHERE [FAMFile].[ID] IN (" + stackIDs.top();
			stackIDs.pop();
			for (int i=0; !stackIDs.empty() && i < 150; i++)
			{
				strUpdateQuery += ", " + stackIDs.top();
				stackIDs.pop();
			}
			strUpdateQuery += ")";

			// Execute the update query
			nNumRecords += executeCmdQuery(ipConnection, strUpdateQuery);
		}


		// Commit the transaction
		tg.CommitTrans();

		// If returning the number of modified records, set the return value
		if (pnNumRecordsModified != NULL)
		{
			*pnNumRecordsModified = nNumRecords;
		}

		END_CONNECTION_RETRY(ipConnection, "ELI27712");

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27713");

}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::AddUserCounter(BSTR bstrCounterName, LONGLONG llInitialValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Get the counter name and ensure it is not empty
		string strCounterName = asString(bstrCounterName);
		ASSERT_ARGUMENT("ELI27749", !strCounterName.empty());
		replaceVariable(strCounterName, "'", "''");

		// Build the query for adding the new counter
		string strQuery = "INSERT INTO " + gstrUSER_CREATED_COUNTER
			+ "([CounterName], [Value]) VALUES ('" + strCounterName
			+ "', " + asString(llInitialValue) + ")";

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();

		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Lock the database
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		// Set the transaction guard
		TransactionGuard tg(ipConnection);

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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27752");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::RemoveUserCounter(BSTR bstrCounterName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Get the counter name and ensure it is not empty
		string strCounterName = asString(bstrCounterName);
		ASSERT_ARGUMENT("ELI27753", !strCounterName.empty());
		replaceVariable(strCounterName, "'", "''");

		// Build the query for deleting the counter
		string strQuery = "DELETE FROM " + gstrUSER_CREATED_COUNTER
			+ " WHERE [CounterName] = '" + strCounterName + "'";

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();

		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Lock the database
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		// Set the transaction guard
		TransactionGuard tg(ipConnection);

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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27756");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::RenameUserCounter(BSTR bstrCounterName, BSTR bstrNewCounterName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

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
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();

		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Lock the database
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		// Set the transaction guard
		TransactionGuard tg(ipConnection);

		// Make sure the DB Schema is the expected version
		validateDBSchemaVersion();

		// Make sure the DB Schema is the expected version
		validateDBSchemaVersion();

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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27761");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetUserCounterValue(BSTR bstrCounterName, LONGLONG llNewValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Get the counter name and ensure it is not empty
		string strCounterName = asString(bstrCounterName);
		ASSERT_ARGUMENT("ELI27762", !strCounterName.empty());
		replaceVariable(strCounterName, "'", "''");

		// Build the query for setting the counter value
		string strQuery = "UPDATE " + gstrUSER_CREATED_COUNTER + " SET [Value] = "
			+ asString(llNewValue) + " WHERE [CounterName] = '" + strCounterName + "'";

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();

		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Lock the database
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		// Set the transaction guard
		TransactionGuard tg(ipConnection);

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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27765");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetUserCounterValue(BSTR bstrCounterName, LONGLONG *pllValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI27766", pllValue != NULL);

		// Get the counter name and ensure it is not empty
		string strCounterName = asString(bstrCounterName);
		ASSERT_ARGUMENT("ELI27767", !strCounterName.empty());
		replaceVariable(strCounterName, "'", "''");

		// Build the query for getting the counter value
		string strQuery = "SELECT [Value] FROM " + gstrUSER_CREATED_COUNTER
			+ " WHERE [CounterName] = '" + strCounterName + "'";

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();

		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Lock the database
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		// Make sure the DB Schema is the expected version
		validateDBSchemaVersion();

		// Recordset to get the counter value from
		_RecordsetPtr ipCounterSet(__uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI27768", ipCounterSet != NULL );

		// Get the recordset for the specified select query
		ipCounterSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenForwardOnly, 
			adLockReadOnly, adCmdText );

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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27771");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetUserCounterNames(IVariantVector** ppvecNames)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI27772", ppvecNames != NULL);

		// Build the query for getting the counter value
		string strQuery = "SELECT [CounterName] FROM " + gstrUSER_CREATED_COUNTER;

		IVariantVectorPtr ipVecNames(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI27774", ipVecNames != NULL);

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();

		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Lock the database
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		// Make sure the DB Schema is the expected version
		validateDBSchemaVersion();

		// Recordset to get the counters from
		_RecordsetPtr ipCounterSet(__uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI27775", ipCounterSet != NULL );

		// Get the recordset for the specified select query
		ipCounterSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenForwardOnly, 
			adLockReadOnly, adCmdText );

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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27777");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetUserCounterNamesAndValues(IStrToStrMap** ppmapUserCounters)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI27778", ppmapUserCounters != NULL);

		// Build the query for getting the counter value
		string strQuery = "SELECT * FROM " + gstrUSER_CREATED_COUNTER;

		IStrToStrMapPtr ipmapUserCounters(CLSID_StrToStrMap);
		ASSERT_RESOURCE_ALLOCATION("ELI27780", ipmapUserCounters != NULL);

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();

		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Lock the database
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		// Make sure the DB Schema is the expected version
		validateDBSchemaVersion();

		// Recordset to get the counters and values from
		_RecordsetPtr ipCounterSet(__uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI27781", ipCounterSet != NULL );

		// Get the recordset for the specified select query
		ipCounterSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenForwardOnly, 
			adLockReadOnly, adCmdText );

		// Check for value in the database
		while (ipCounterSet->adoEOF == VARIANT_FALSE)
		{
			FieldsPtr ipFields = ipCounterSet->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI27782", ipFields != NULL);

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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27784");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::IsUserCounterValid(BSTR bstrCounterName,
												   VARIANT_BOOL* pbCounterValid)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI27907", pbCounterValid != NULL);

		// Get the counter name and ensure it is not empty
		string strCounterName = asString(bstrCounterName);
		ASSERT_ARGUMENT("ELI27908", !strCounterName.empty());
		replaceVariable(strCounterName, "'", "''");

		string strQuery = "SELECT [Value] FROM " + gstrUSER_CREATED_COUNTER
			+ " WHERE [CounterName] = '" + strCounterName + "'";

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();

		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Lock the database
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		// Make sure the DB Schema is the expected version
		validateDBSchemaVersion();

		// Recordset to get
		_RecordsetPtr ipCounterSet(__uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI27909", ipCounterSet != NULL );

		// Get the recordset for the specified select query
		ipCounterSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenForwardOnly, 
			adLockReadOnly, adCmdText );

		// Set true if there is a record found false otherwise
		*pbCounterValid = asVariantBool(ipCounterSet->adoEOF == VARIANT_FALSE);

		END_CONNECTION_RETRY(ipConnection, "ELI27910");

		return S_OK;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27911");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::OffsetUserCounter(BSTR bstrCounterName, LONGLONG llOffsetValue,
												   LONGLONG* pllNewValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI27715", pllNewValue != NULL);

		// Get the counter name and ensure it is not empty
		string strCounterName = asString(bstrCounterName);
		ASSERT_ARGUMENT("ELI27716", !strCounterName.empty());
		replaceVariable(strCounterName, "'", "''");

		// Build the query
		string strQuery = "SELECT [Value] FROM " + gstrUSER_CREATED_COUNTER
			+ " WHERE [CounterName] = '" + strCounterName + "'";

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();

		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Lock the database
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		// Make sure the DB Schema is the expected version
		validateDBSchemaVersion();

		// Set the transaction guard
		TransactionGuard tg(ipConnection);

		// Recordset to get the counters and values from
		_RecordsetPtr ipCounterSet(__uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI27717", ipCounterSet != NULL );

		// Get the recordset for the specified select query
		ipCounterSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenDynamic, 
			adLockOptimistic, adCmdText );

		if (ipCounterSet->adoEOF == VARIANT_FALSE)
		{
			FieldsPtr ipFields = ipCounterSet->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI27718", ipFields != NULL);

			// Get the counter value
			LONGLONG llValue = getLongLongField(ipFields, "Value");

			// Modify the value
			llValue += llOffsetValue;

			// Update the value
			setLongLongField(ipFields, "Value", llValue);
			ipCounterSet->Update();

			// Set the return value
			*pllNewValue = llValue;
		}
		else
		{
			UCLIDException uex("ELI27815", "User counter name specified does not exist.");
			uex.addDebugInfo("User Counter Name", asString(bstrCounterName));
			throw uex;
		}

		// Commit the transaction
		tg.CommitTrans();

		END_CONNECTION_RETRY(ipConnection, "ELI27816");

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27817");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::RecordFAMSessionStart(BSTR bstrFPSFileName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Get the FPS File name
		string strFPSFileName = asString(bstrFPSFileName);

		string strFAMSessionQuery = "INSERT INTO [" + gstrFAM_SESSION + "] ([MachineID], ";
		strFAMSessionQuery += "[FAMUserID], [UPI], [FPSFileID]) VALUES (";

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();

		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Lock the database
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		// Make sure the DB Schema is the expected version
		validateDBSchemaVersion();

		// Set the transaction guard
		TransactionGuard tg(ipConnection);

		// Get FPSFileID, MachineID, and UserID (this will add records if they don't exist)
		long nFPSFileID = getKeyID(ipConnection, gstrFPS_FILE, "FPSFileName",
			strFPSFileName.empty() ? "<Unsaved FPS File>" : strFPSFileName);
		long nMachineID = getKeyID(ipConnection, gstrMACHINE, "MachineName", m_strMachineName);
		long nUserID = getKeyID(ipConnection, gstrFAM_USER, "UserName", m_strFAMUserName);

		strFAMSessionQuery += asString(nMachineID) + ", " + asString(nUserID) + ", '"
			+ m_strUPI + "', " + asString(nFPSFileID) + ")";

		// Insert the record into the FAMSession table
		executeCmdQuery(ipConnection, strFAMSessionQuery);

		// Commit the transaction
		tg.CommitTrans();

		END_CONNECTION_RETRY(ipConnection, "ELI28903");

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28904");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::RecordFAMSessionStop()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Build the update query to set stop time
		string strFAMSessionQuery = "UPDATE [" + gstrFAM_SESSION + "] SET [StopTime] = GETDATE() "
			"WHERE [" + gstrFAM_SESSION + "].[UPI] = '" + m_strUPI + "' AND [StopTime] IS NULL";

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();

		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Lock the database
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		// Make sure the DB Schema is the expected version
		validateDBSchemaVersion();

		// Set the transaction guard
		TransactionGuard tg(ipConnection);

		// Execute the update query
		executeCmdQuery(ipConnection, strFAMSessionQuery);

		// Commit the transaction
		tg.CommitTrans();

		END_CONNECTION_RETRY(ipConnection, "ELI28905");

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28906");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::RecordInputEvent(BSTR bstrTimeStamp, long nActionID,
												 long nEventCount, long nProcessID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		string strSQL = "INSERT INTO [" + gstrINPUT_EVENT + "] ([TimeStamp], "
			"[ActionID], [FAMUserID], [MachineID], [PID], [SecondsWithInputEvents]) "
			"VALUES (CAST('" + asString(bstrTimeStamp) + "' AS smalldatetime), "
			+ asString(nActionID) + ", ";

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
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
		TransactionGuard tg(ipConnection);

		long nMachineID = getKeyID(ipConnection, gstrMACHINE, "MachineName", m_strMachineName);
		long nUserID = getKeyID(ipConnection, gstrFAM_USER, "UserName", m_strFAMUserName);

		// Add the remaining 
		strSQL += asString(nUserID) + ", " + asString(nMachineID) + ", "
			+ asString(nProcessID) + ", " + asString(nEventCount) + ")";

		// Execute the insert query
		executeCmdQuery(ipConnection, strSQL);

		// Delete the old input events
		deleteOldInputEvents(ipConnection);

		// Commit the transaction
		tg.CommitTrans();

		END_CONNECTION_RETRY(ipConnection, "ELI28942");

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28943");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetLoginUsers(IStrToStrMap**  ppUsers)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Get the users from the database
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
		_RecordsetPtr ipLoginSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI29040", ipLoginSet != NULL );

		// SQL query to get the login users that are not admin
		string strSQL = "SELECT UserName, Password FROM Login where UserName <> 'admin'";

		// Open the set of login users
		ipLoginSet->Open( strSQL.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
			adLockReadOnly, adCmdText );

		// Create map to return results
		IStrToStrMapPtr ipUsers(CLSID_StrToStrMap);
		ASSERT_RESOURCE_ALLOCATION("ELI29039", ipUsers != NULL);

		// Step through all records
		while ( ipLoginSet->adoEOF == VARIANT_FALSE )
		{
			// Get the fields from the action set
			FieldsPtr ipFields = ipLoginSet->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI29041", ipFields != NULL);

			// Get the user
			string strUser = getStringField(ipFields, "UserName");

			// Get the password
			string strPasswordset = getStringField(ipFields, "Password").empty() ? "No" : "Yes";

			// Save in the Users map
			ipUsers->Set( strUser.c_str(), strPasswordset.c_str());

			// Go to next user
			ipLoginSet->MoveNext();
		}

		// return the StrToStrMap containing all login users except admin
		*ppUsers = ipUsers.Detach();
		END_CONNECTION_RETRY(ipConnection, "ELI29042");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29038");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::AddLoginUser(BSTR bstrUserName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check License
		validateLicense();

		// Convert the user name to add to string for local use
		string strUserName = asString(bstrUserName);

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();
		
		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Lock the database for this instance
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		// Make sure the DB Schema is the expected version
		validateDBSchemaVersion();

		// Set the transaction guard
		TransactionGuard tg(ipConnection);

		// Create a pointer to a recordset
		_RecordsetPtr ipLoginSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI29064", ipLoginSet != NULL );

		// Sql query that should either be empty if the passed in users is not in the table
		// or will return the record with the given username
		string strLoginSelect = "Select Username From Login Where UserName = '" + strUserName + "'";

		// Open the sql query
		ipLoginSet->Open( strLoginSelect.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
			adLockReadOnly, adCmdText );

		// Check to see if action exists
		if (!asCppBool(ipLoginSet->adoEOF))
		{
			UCLIDException ue("ELI29065", "Login username already exists.");
			ue.addDebugInfo("Usename", strUserName);
			throw ue;
		}

		// Execute the insert query to add the user Password defaults to empty string
		executeCmdQuery(ipConnection, "Insert Into Login (UserName) VALUES ('" + strUserName + "')");
		
		// Commit the changes
		tg.CommitTrans();

		END_CONNECTION_RETRY(ipConnection, "ELI29063");

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29043");}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::RemoveLoginUser(BSTR bstrUserName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check License
		validateLicense();

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
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();
		
		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Lock the database for this instance
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		// Make sure the DB Schema is the expected version
		validateDBSchemaVersion();

		// Set the transaction guard
		TransactionGuard tg(ipConnection);

		// Delet the specified user from the login table
		executeCmdQuery(ipConnection, "DELETE FROM Login WHERE UserName = '" + strUserName + "'");
		
		// Commit the changes
		tg.CommitTrans();

		END_CONNECTION_RETRY(ipConnection, "ELI29067");

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29044");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::RenameLoginUser(BSTR bstrUserNameToRename, BSTR bstrNewUserName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check License
		validateLicense();

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
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();
		
		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Lock the database for this instance
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		// Make sure the DB Schema is the expected version
		validateDBSchemaVersion();

		// Set the transaction guard
		TransactionGuard tg(ipConnection);

		// Change old username to new user name in the table.
		executeCmdQuery(ipConnection, "UPDATE Login SET UserName = '" + strNewUserName + 
			"' WHERE UserName = '" + strOldUserName + "'");
		
		// Commit the changes
		tg.CommitTrans();

		END_CONNECTION_RETRY(ipConnection, "ELI29112");

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29045");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::ClearLoginUserPassword(BSTR bstrUserName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check License
		validateLicense();

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
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();
		
		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Lock the database for this instance
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		// Make sure the DB Schema is the expected version
		validateDBSchemaVersion();

		// Set the transaction guard
		TransactionGuard tg(ipConnection);

		// Clear the password for the given user in the login table
		executeCmdQuery(ipConnection, "UPDATE Login SET Password = '' WHERE UserName = '" + strUserName + "'");
		
		// Commit the changes
		tg.CommitTrans();

		END_CONNECTION_RETRY(ipConnection, "ELI29070");

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29069");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetAutoCreateActions(VARIANT_BOOL* pvbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI29118", pvbValue != NULL);

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();

		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Lock the database
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

		// Get the setting
		string strSetting = getDBInfoSetting(ipConnection, gstrAUTO_CREATE_ACTIONS);

		// Set the out value
		*pvbValue = strSetting == "1" ? VARIANT_TRUE : VARIANT_FALSE;

		END_CONNECTION_RETRY(ipConnection, "ELI29119");

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29120");
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
