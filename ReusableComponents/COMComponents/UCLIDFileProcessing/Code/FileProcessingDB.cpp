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
m_dRetryTimeout(gdDEFAULT_RETRY_TIMEOUT),
m_nUPIID(0),
m_bFAMRegistered(false),
m_nActionStatisticsUpdateFreqInSeconds(5),
m_bValidatingOrUpdatingSchema(false),
m_bProductSpecificDBSchemasAreValid(false)
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

		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI29457", m_ipMiscUtils != NULL);

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
void CFileProcessingDB::FinalRelease()
{
	try
	{
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
STDMETHODIMP CFileProcessingDB::DefineNewAction(BSTR strAction, long* pnID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check License
		validateLicense();

		if (!DefineNewAction_Internal(false, strAction, pnID))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

			DefineNewAction_Internal(true, strAction, pnID);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13524");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::DeleteAction(BSTR strAction)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check License
		validateLicense();

		if (!DeleteAction_Internal(false, strAction))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

			DeleteAction_Internal(true, strAction);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13527");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetActions(IStrToStrMap * * pmapActionNameToID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check License
		validateLicense();

		if (!GetActions_Internal(false, pmapActionNameToID))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			
			GetActions_Internal(true, pmapActionNameToID);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13531");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::AddFile(BSTR strFile,  BSTR strAction, EFilePriority ePriority,
										VARIANT_BOOL bForceStatusChange, VARIANT_BOOL bFileModified,
										EActionStatus eNewStatus, VARIANT_BOOL * pbAlreadyExists,
										EActionStatus *pPrevStatus, IFileRecord* * ppFileRecord)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check License
		validateLicense();

		if (!AddFile_Internal(false, strFile, strAction, ePriority, bForceStatusChange, bFileModified,
			eNewStatus, pbAlreadyExists, pPrevStatus, ppFileRecord))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

			AddFile_Internal(true, strFile, strAction, ePriority, bForceStatusChange, bFileModified,
				eNewStatus, pbAlreadyExists, pPrevStatus, ppFileRecord);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13536");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::RemoveFile(BSTR strFile, BSTR strAction)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check License
		validateLicense();

		if (!RemoveFile_Internal(false, strFile, strAction))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

			RemoveFile_Internal(true, strFile, strAction);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13538");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::NotifyFileProcessed(long nFileID,  BSTR strAction)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check License
		validateLicense();

		if (!NotifyFileProcessed_Internal(false, nFileID, strAction))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

			NotifyFileProcessed_Internal(true, nFileID, strAction);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13541");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::NotifyFileFailed(long nFileID,  BSTR strAction,  BSTR strException)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check License
		validateLicense();

		if (!NotifyFileFailed_Internal(false, nFileID, strAction, strException))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

			NotifyFileFailed_Internal(true, nFileID, strAction, strException);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13544");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetFileStatusToPending(long nFileID,  BSTR strAction)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check License
		validateLicense();
		if (!SetFileStatusToPending_Internal(false, nFileID,  strAction))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

			SetFileStatusToPending_Internal(true, nFileID,  strAction);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13546");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetFileStatusToUnattempted(long nFileID,  BSTR strAction)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check License
		validateLicense();

		if (!SetFileStatusToUnattempted_Internal(false, nFileID, strAction))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

			SetFileStatusToUnattempted_Internal(true, nFileID, strAction);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13548");
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

		if (!SetFileStatusToSkipped_Internal(false, nFileID, strAction, bRemovePreviousSkipped))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

			SetFileStatusToSkipped_Internal(true, nFileID, strAction, bRemovePreviousSkipped);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26939");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetFileStatus(long nFileID,  BSTR strAction,
									VARIANT_BOOL vbAttemptRevertIfLocked, EActionStatus * pStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check License
		validateLicense();

		if (!GetFileStatus_Internal(false, nFileID,  strAction, vbAttemptRevertIfLocked, pStatus))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

			GetFileStatus_Internal(true, nFileID,  strAction, vbAttemptRevertIfLocked, pStatus);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13550");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SearchAndModifyFileStatus(long nWhereActionID,  EActionStatus eWhereStatus,  
														  long nToActionID, EActionStatus eToStatus,
														  BSTR bstrSkippedFromUserName, 
														  long nFromActionID, long * pnNumRecordsModified)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check License
		validateLicense();

		if (!SearchAndModifyFileStatus_Internal(false, nWhereActionID, eWhereStatus, nToActionID, eToStatus,
			bstrSkippedFromUserName, nFromActionID, pnNumRecordsModified))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

			SearchAndModifyFileStatus_Internal(true, nWhereActionID, eWhereStatus, nToActionID, eToStatus,
				bstrSkippedFromUserName, nFromActionID, pnNumRecordsModified);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13565");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetStatusForAllFiles(BSTR strAction,  EActionStatus eStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check License
		validateLicense();

		if (!SetStatusForAllFiles_Internal(false, strAction,  eStatus))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

			SetStatusForAllFiles_Internal(true, strAction, eStatus);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13571");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetStatusForFile(long nID,  BSTR strAction,  EActionStatus eStatus,  
												 EActionStatus * poldStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check License
		validateLicense();

		if (!SetStatusForFile_Internal(false, nID, strAction, eStatus, poldStatus))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

			SetStatusForFile_Internal(true, nID, strAction, eStatus, poldStatus);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13572");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetFilesToProcess(BSTR strAction,  long nMaxFiles, 
												  VARIANT_BOOL bGetSkippedFiles,
												  BSTR bstrSkippedForUserName,
												  IIUnknownVector * * pvecFileRecords)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check License
		validateLicense();

		if (!GetFilesToProcess_Internal(false, strAction, nMaxFiles, bGetSkippedFiles, 
				bstrSkippedForUserName, pvecFileRecords))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			GetFilesToProcess_Internal(true, strAction, nMaxFiles, bGetSkippedFiles, 
				bstrSkippedForUserName, pvecFileRecords);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13574");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::RemoveFolder(BSTR strFolder, BSTR strAction)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check License
		validateLicense();

		if (!RemoveFolder_Internal(false, strFolder, strAction))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

			RemoveFolder_Internal(true, strFolder, strAction);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13611");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetStats(/*[in]*/ long nActionID, /*[out, retval]*/ IActionStatistics* *pStats)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (!GetStats_Internal(false, nActionID, pStats))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

			GetStats_Internal(true, nActionID, pStats);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14045")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::CopyActionStatusFromAction(long  nFromAction, long nToAction)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (!CopyActionStatusFromAction_Internal(false, nFromAction, nToAction))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

			CopyActionStatusFromAction_Internal(true, nFromAction, nToAction);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14097");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::RenameAction(long nActionID, BSTR strNewActionName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (!RenameAction_Internal(false, nActionID, strNewActionName))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

			RenameAction_Internal(true, nActionID, strNewActionName);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19505");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::Clear(VARIANT_BOOL vbRetainUserValues)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();
	
		if (!Clear_Internal(false, vbRetainUserValues))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			
			Clear_Internal(true, vbRetainUserValues);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14088");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::ExportFileList(BSTR strQuery, BSTR strOutputFileName,
											   IRandomMathCondition* pRandomCondition, long *pnNumRecordsOutput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();

		if (!ExportFileList_Internal(false, strQuery, strOutputFileName, pRandomCondition, 
			pnNumRecordsOutput))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			
			ExportFileList_Internal(true, strQuery, strOutputFileName, pRandomCondition, 
			pnNumRecordsOutput);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14726");
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
		CSingleLock lock(&m_mutex, TRUE);

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
STDMETHODIMP CFileProcessingDB::GetActionID(BSTR bstrActionName, long* pnActionID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		if (!GetActionID_Internal(false, bstrActionName, pnActionID))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			
			GetActionID_Internal(true, bstrActionName, pnActionID);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14986")
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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19507")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::CloseAllDBConnections()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		// Validate the license
		validateLicense();

		// Call the internal close all DB connections
		closeAllDBConnections();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29883")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetNotificationUIWndHandle(long nHandle)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// update the internal window handle to send UI notifications to
		m_hUIWindow = (HWND) nHandle;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14989")
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
		string strCaption = "Set " + strUser + "'s Password";

		// Set login valid and cancelled to false
		*pbLoginValid = VARIANT_FALSE;
		*pbLoginCancelled = VARIANT_FALSE;

		// Initialize the DB if it is blank
		initializeIfBlankDB();

		// Get the stored password (if it exists)
		string strStoredEncryptedCombined;
		bool bUserExists = getEncryptedPWFromDB(strStoredEncryptedCombined, bUseAdmin);

		// if there is no password will need to get the new password
		if (strStoredEncryptedCombined == "" && bUseAdmin)
		{
			// Set the admin password
			// default to using the desktop as the parent for the messagebox below
			HWND hParent = getAppMainWndHandle();

			::MessageBox(hParent, "This is the first time you are logging into this File Action Manager database.\r\n\r\n"
				"You will be prompted to set the admin password in the next screen.  The admin password "
				"will be required to login into the database before any actions can be performed on the "
				"database from this application.\r\n\r\n"
				"Please keep your admin password in a safe location and share it only with people capable "
				"of administering the File Action Manager database.  Please note that anyone with access "
				"to the admin password will be able to use this application to execute data-deleting "
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
			if (dlgPW.DoModal() != IDOK)
			{
				// Did not fill in and ok dlg so there is no login
				// Set Cancelled flag
				*pbLoginCancelled = VARIANT_TRUE;
				return S_OK;
			}

			// Update password in database Login table (fail if not the admin user
			// and the user doesn't exist)
			string strPassword = dlgPW.m_zNewPassword;
			string strCombined = strUser + strPassword;
			encryptAndStoreUserNamePassword(strCombined, bUseAdmin, !bUseAdmin);

			// Just added password to the db so it is valid
			*pbLoginValid = VARIANT_TRUE;
			return S_OK;
		}

		// Set read-only user name to "admin" (P13 #4112) or user's name
		CLoginDlg dlgLogin("Login", (bUseAdmin) ? gstrADMIN_USER : m_strFAMUserName , true);
		if (dlgLogin.DoModal() != IDOK)
		{
			// The OK button on the login dialog was not pressed so do not login
			// Set Cancelled flag
			*pbLoginCancelled = VARIANT_TRUE;
			return S_OK;
		}

		// Validate password
		*pbLoginValid = asVariantBool(isPasswordValid(string(dlgLogin.m_zPassword), bUseAdmin));
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15099");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::get_DBSchemaVersion(LONG* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI15149", pVal != NULL);

		*pVal = getDBSchemaVersion();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15148");
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
			UCLIDException ue("ELI15721", "Cannot change password if no password is defined!");
			throw ue;
		}

		bool bPasswordValid = false;
		string strUser = bUseAdmin ? gstrADMIN_USER : m_strFAMUserName;
		string strPasswordDlgCaption = "Change " + strUser + " Password";
		
		// Display Change Password dialog

		ChangePasswordDlg dlgPW(strPasswordDlgCaption);
		do
		{
			if (dlgPW.DoModal() != IDOK)
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
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16167");

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
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17621");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::get_DatabaseServer(BSTR* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		ASSERT_ARGUMENT("ELI17564", pVal != NULL);

		*pVal = get_bstr_t(m_strDatabaseServer).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17468");
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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17622");

}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::get_DatabaseName(BSTR* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		ASSERT_ARGUMENT("ELI17565", pVal != NULL);

		*pVal = get_bstr_t(m_strDatabaseName).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17623");
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
		if (m_strDatabaseServer.empty())
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
		ADODB::_ConnectionPtr ipDBConnection(__uuidof(Connection)); 

		// Open a connection to the the master database on the database server
		ipDBConnection->Open(createConnectionString(m_strDatabaseServer, "master").c_str(),
			"", "", adConnectUnspecified);

		// Query to create the database
		string strCreateDB = "CREATE DATABASE [" + m_strDatabaseName + "]";

		// Execute the query to create the new database
		ipDBConnection->Execute(strCreateDB.c_str(), NULL, adCmdText | adExecuteNoRecords);

		// Close the connections
		ipDBConnection->Close();

		// Clear the new database to set up the tables
		clear();
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17469");

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
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17842");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetDBInfoSetting(BSTR bstrSettingName, BSTR bstrSettingValue, 
												 VARIANT_BOOL vbSetIfExists)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		validateLicense();

		if (!SetDBInfoSetting_Internal(false, bstrSettingName, bstrSettingValue, vbSetIfExists))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

			SetDBInfoSetting_Internal(true, bstrSettingName, bstrSettingValue, vbSetIfExists);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18936");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetDBInfoSetting(BSTR bstrSettingName, VARIANT_BOOL vbThrowIfMissing,
	BSTR* pbstrSettingValue)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		validateLicense();

		if (!GetDBInfoSetting_Internal(false, bstrSettingName, vbThrowIfMissing, pbstrSettingValue))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

			GetDBInfoSetting_Internal(true, bstrSettingName, vbThrowIfMissing, pbstrSettingValue);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18937");

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


		if (!GetResultsForQuery_Internal(false, bstrQuery, ppVal))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			GetResultsForQuery_Internal(true, bstrQuery, ppVal);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19875");
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

		if (!GetFileID_Internal(false, bstrFileName, pnFileID))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

			GetFileID_Internal(true, bstrFileName, pnFileID);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24030");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetActionName(long nActionID, BSTR *pbstrActionName)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		validateLicense();

		if (!GetActionName_Internal(false, nActionID, pbstrActionName))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

			GetActionName_Internal(true, nActionID, pbstrActionName);
		}

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

		if (!NotifyFileSkipped_Internal(false, nFileID, nActionID))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			
			NotifyFileSkipped_Internal(true, nFileID, nActionID);
		}
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

		if (!SetFileActionComment_Internal(false, nFileID, nActionID, bstrComment))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			
			SetFileActionComment_Internal(true, nFileID, nActionID, bstrComment);
		}

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

		if (!GetFileActionComment_Internal(false, nFileID, nActionID, pbstrComment))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			GetFileActionComment_Internal(true, nFileID, nActionID, pbstrComment);
		}

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

		if (!ClearFileActionComment_Internal(false, nFileID, nActionID))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

			ClearFileActionComment_Internal(true, nFileID, nActionID);
		}
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
		validateLicense();

		if (!ModifyActionStatusForQuery_Internal(false, bstrQueryFrom, bstrToAction, eaStatus, 
			bstrFromAction, pRandomCondition, pnNumRecordsModified))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			ModifyActionStatusForQuery_Internal(true, bstrQueryFrom, bstrToAction, eaStatus, 
				bstrFromAction, pRandomCondition, pnNumRecordsModified);
		}
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

		if (!GetTags_Internal(false, ppTags))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

			GetTags_Internal(true, ppTags);
		}
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

		if (!GetTagNames_Internal(false, ppTagNames))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

			GetTagNames_Internal(true, ppTagNames);
		}
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27343");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::HasTags(VARIANT_BOOL* pvbVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		if (!HasTags_Internal(false, pvbVal))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			
			HasTags_Internal(true, pvbVal);
		}
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

		if (!TagFile_Internal(false, nFileID, bstrTagName))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			
			TagFile_Internal(true, nFileID, bstrTagName);
		}
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

		if (!UntagFile_Internal(false, nFileID, bstrTagName))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			UntagFile_Internal(true, nFileID, bstrTagName);
		}

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

		if (!ToggleTagOnFile_Internal(false, nFileID, bstrTagName))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			
			ToggleTagOnFile_Internal(true, nFileID, bstrTagName);
		}
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

		if (!AddTag_Internal(false, bstrTagName, bstrTagDescription))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

			AddTag_Internal(true, bstrTagName, bstrTagDescription);
		}
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

		if (!DeleteTag_Internal(false, bstrTagName))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());

			DeleteTag_Internal(true, bstrTagName);
		}
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

		if (!ModifyTag_Internal(false, bstrOldTagName, bstrNewTagName,bstrNewTagDescription))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			ModifyTag_Internal(true, bstrOldTagName, bstrNewTagName,bstrNewTagDescription);
		}
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

		if (!GetFilesWithTags_Internal(false, pvecTagNames, vbAndOperation, ppvecFileIDs))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			GetFilesWithTags_Internal(true, pvecTagNames, vbAndOperation, ppvecFileIDs);
		}
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

		if (!GetTagsOnFile_Internal(false, nFileID, ppvecTagNames))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			GetTagsOnFile_Internal(true, nFileID, ppvecTagNames);
		}

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

		if (!AllowDynamicTagCreation_Internal(false, pvbVal))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			AllowDynamicTagCreation_Internal(true, pvbVal);
		}
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

		if (!SetStatusForFilesWithTags_Internal(false, pvecTagNames, vbAndOperation, 
			nToActionID, eaNewStatus, nFromActionID))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			
			SetStatusForFilesWithTags_Internal(true, pvecTagNames, vbAndOperation, 
			nToActionID, eaNewStatus, nFromActionID);
		} 
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


		if (!ExecuteCommandQuery_Internal(false, bstrQuery, pnRecordsAffected))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			ExecuteCommandQuery_Internal(true, bstrQuery, pnRecordsAffected);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27686");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::RegisterProcessingFAM(long lActionID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// This creates a record in the ProcessingFAM table and the LastPingTime
		// is set to the current time by default.
		executeCmdQuery(getDBConnection(), "INSERT INTO ProcessingFAM (UPI, ActionID) "
			"VALUES ('" + m_strUPI + "', " + asString(lActionID) + ")");
		// get the new records ID to return
		m_nUPIID = getLastTableID(getDBConnection(), "ProcessingFAM");

		m_eventStopPingThread.reset();
		m_eventPingThreadExited.reset();

		// set FAM registered flag
		m_bFAMRegistered = true;

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

		if (!UnregisterProcessingFAM_Internal(false))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			UnregisterProcessingFAM_Internal(true);
		}
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


		if (!SetPriorityForFiles_Internal(false, bstrSelectQuery, eNewPriority, pRandomCondition, 
			pnNumRecordsModified))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			SetPriorityForFiles_Internal(true, bstrSelectQuery, eNewPriority,
				pRandomCondition, pnNumRecordsModified);
		}
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


		if (!AddUserCounter_Internal(false, bstrCounterName, llInitialValue))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			AddUserCounter_Internal(true, bstrCounterName, llInitialValue);
		}
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


		if (!RemoveUserCounter_Internal(false, bstrCounterName))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			RemoveUserCounter_Internal(true, bstrCounterName);
		}
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

		if (!RenameUserCounter_Internal(false, bstrCounterName, bstrNewCounterName))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			RenameUserCounter_Internal(true, bstrCounterName, bstrNewCounterName);
		}

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

		if (!SetUserCounterValue_Internal(false, bstrCounterName, llNewValue))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			SetUserCounterValue_Internal(true , bstrCounterName, llNewValue);
		}

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

		if (!GetUserCounterValue_Internal(false, bstrCounterName, pllValue))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			GetUserCounterValue_Internal(true, bstrCounterName, pllValue);
		}
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
		
		if (!GetUserCounterNames_Internal(false , ppvecNames))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			GetUserCounterNames_Internal(true, ppvecNames);
		}
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

		if (!GetUserCounterNamesAndValues_Internal(false, ppmapUserCounters))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			GetUserCounterNamesAndValues_Internal(true, ppmapUserCounters);
		}

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

		if (!IsUserCounterValid_Internal(false, bstrCounterName, pbCounterValid))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			IsUserCounterValid_Internal(true, bstrCounterName, pbCounterValid);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27911");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::OffsetUserCounter(BSTR bstrCounterName, LONGLONG llOffsetValue,
												   LONGLONG* pllNewValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		if (!OffsetUserCounter_Internal(false, bstrCounterName, llOffsetValue, pllNewValue))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			OffsetUserCounter_Internal(true, bstrCounterName, llOffsetValue, pllNewValue);
		}
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
		if (!RecordFAMSessionStart_Internal(false, bstrFPSFileName))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			RecordFAMSessionStart_Internal(true, bstrFPSFileName);
		}

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
		if (!RecordFAMSessionStop_Internal(false))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			RecordFAMSessionStop_Internal(true);
		}

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

		if (!RecordInputEvent_Internal(false, bstrTimeStamp, nActionID, nEventCount, nProcessID))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			RecordInputEvent_Internal(true, bstrTimeStamp, nActionID, nEventCount, nProcessID);
		}
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

		if (!GetLoginUsers_Internal(false, ppUsers))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			GetLoginUsers_Internal(true, ppUsers);
		}
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

		if (!AddLoginUser_Internal(false, bstrUserName))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			AddLoginUser_Internal(true, bstrUserName);
		}
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

		if (!RemoveLoginUser_Internal(false, bstrUserName))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			RemoveLoginUser_Internal(true, bstrUserName);
		}
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

		if (!RenameLoginUser_Internal(false, bstrUserNameToRename, bstrNewUserName))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			RenameLoginUser_Internal(true, bstrUserNameToRename, bstrNewUserName);
		}
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

		if (!ClearLoginUserPassword_Internal(false, bstrUserName))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			ClearLoginUserPassword_Internal(true, bstrUserName);
		}
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

		if (!GetAutoCreateActions_Internal(false, pvbValue))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			GetAutoCreateActions_Internal(true, pvbValue);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29120");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::AutoCreateAction(BSTR bstrActionName, long* plId)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		if (!AutoCreateAction_Internal(false, bstrActionName, plId))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			AutoCreateAction_Internal(true, bstrActionName, plId);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29154");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::CanSkipAuthenticationOnThisMachine(VARIANT_BOOL* pvbSkipAuthentication)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Make sure the DB Schema is the expected version
		validateDBSchemaVersion();

		ASSERT_ARGUMENT("ELI29207", pvbSkipAuthentication != NULL);

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = NULL;
		
		BEGIN_CONNECTION_RETRY();

		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Check whether the current machine is in the list of machines to skip user
		// authentication when running as a service
		*pvbSkipAuthentication = asVariantBool(
			isMachineInListOfMachinesToSkipUserAuthentication(ipConnection));

		END_CONNECTION_RETRY(ipConnection, "ELI29237");

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29238");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetFileRecord(BSTR bstrFile, BSTR bstrActionName,
											  IFileRecord** ppFileRecord)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		
		if (!GetFileRecord_Internal(false, bstrFile, bstrActionName, ppFileRecord))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			GetFileRecord_Internal(true, bstrFile, bstrActionName, ppFileRecord);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29705");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetFileStatusToProcessing(long nFileId, long nActionID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		if (!SetFileStatusToProcessing_Internal(false, nFileId, nActionID))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			SetFileStatusToProcessing_Internal(true, nFileId, nActionID);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29620");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetConnectionRetrySettings(long* pnNumberOfRetries,
														   double* pdRetryTimeout)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI29861", pnNumberOfRetries != NULL);
		ASSERT_ARGUMENT("ELI29862", pdRetryTimeout != NULL);

		validateLicense();

		*pnNumberOfRetries = m_iNumberOfRetries;
		*pdRetryTimeout = m_dRetryTimeout;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29863");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::UpgradeToCurrentSchema(IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		IProgressStatusPtr ipProgressStatus(pProgressStatus);

		if (!UpgradeToCurrentSchema_Internal(false, ipProgressStatus))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			UpgradeToCurrentSchema_Internal(true, ipProgressStatus);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31390");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::RenameFile(IFileRecord* pFileRecord, BSTR bstrNewName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();


		if (!RenameFile_Internal(false, pFileRecord, bstrNewName))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr());
			RenameFile_Internal(true, pFileRecord, bstrNewName);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31463");
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
