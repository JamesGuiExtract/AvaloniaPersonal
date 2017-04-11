// FileProcessingDB.cpp : Implementation of CFileProcessingDB

#include "stdafx.h"
#include "FileProcessingDB.h"
#include "FAMDB_SQL.h"
#include "SelectDBDialog.h"

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

// add license management function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// Static Members
//-------------------------------------------------------------------------------------------------
std::string CFileProcessingDB::ms_strCurrServerName = "";
std::string CFileProcessingDB::ms_strCurrDBName = "";
std::string CFileProcessingDB::ms_strCurrAdvConnProperties = "";
std::string CFileProcessingDB::ms_strLastUsedAdvConnStr = "";

CMutex CFileProcessingDB::ms_mutexPingDBLock;
CMutex CFileProcessingDB::ms_mutexSpecialLoggingLock;

//-------------------------------------------------------------------------------------------------
// CFileProcessingDB
//-------------------------------------------------------------------------------------------------
CFileProcessingDB::CFileProcessingDB()
: m_iDBSchemaVersion(0),
m_hUIWindow(NULL),
m_strCurrentConnectionStatus(gstrNOT_CONNECTED),
m_strDatabaseServer(""),
m_strDatabaseName(""),
m_strAdvConnStrProperties(""),
m_lFAMUserID(0),
m_lMachineID(0),
m_iCommandTimeout(glDEFAULT_COMMAND_TIMEOUT),
m_bUpdateQueueEventTable(true),
m_bUpdateFASTTable(true),
m_bAutoDeleteFileActionComment(false),
m_iNumberOfRetries(giDEFAULT_RETRY_COUNT),
m_dRetryTimeout(gdDEFAULT_RETRY_TIMEOUT),
m_nActiveFAMID(0),
m_nFAMSessionID(0),
m_strUPI(""),
m_bFAMRegistered(false),
m_nActionStatisticsUpdateFreqInSeconds(5),
m_bValidatingOrUpdatingSchema(false),
m_bProductSpecificDBSchemasAreValid(false),
m_bRevertInProgress(false),
m_bRetryOnTimeout(true),
m_nActiveActionID(-1),
m_bLoggedInAsAdmin(false),
m_bCheckedFeatures(false),
m_bAllowRestartableProcessing(false),
m_bStoreDBInfoChangeHistory(false),
m_bWorkItemRevertInProgress(false),
m_strEncryptedDatabaseID(""),
m_bDatabaseIDValuesValidated(false),
m_ipSecureCounters(__nullptr),
m_strActiveWorkflow(""),
m_bUsingWorkflows(false),
m_bRunningAllWorkflows(false),
m_nLastFAMFileID(0),
m_bDeniedFastCountPermission(false)
{
	try
	{
		// Check if license files have been loaded - this is here to so that
		// the Database config COM object can be used from C#
		if (!LicenseManagement::filesLoadedFromFolder())
		{
			// Load the license files
			LicenseManagement::loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);
		}

		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI29457", m_ipMiscUtils != __nullptr);

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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
			
			GetActions_Internal(true, pmapActionNameToID);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13531");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::AddFile(BSTR strFile,  BSTR strAction, long nWorkflowID, EFilePriority ePriority,
										VARIANT_BOOL bForceStatusChange, VARIANT_BOOL bFileModified,
										EActionStatus eNewStatus, VARIANT_BOOL bSkipPageCount,
										VARIANT_BOOL * pbAlreadyExists, EActionStatus *pPrevStatus,
										IFileRecord* * ppFileRecord)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check License
		validateLicense();

		if (!AddFile_Internal(false, strFile, strAction, nWorkflowID, ePriority, bForceStatusChange, bFileModified,
			eNewStatus, bSkipPageCount, pbAlreadyExists, pPrevStatus, ppFileRecord))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			AddFile_Internal(true, strFile, strAction, nWorkflowID, ePriority, bForceStatusChange, bFileModified,
				eNewStatus, bSkipPageCount, pbAlreadyExists, pPrevStatus, ppFileRecord);
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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			RemoveFile_Internal(true, strFile, strAction);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13538");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::NotifyFileProcessed(long nFileID,  BSTR strAction, LONG nWorkflowID,
													VARIANT_BOOL vbAllowQueuedStatusOverride)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check License
		validateLicense();

		if (!NotifyFileProcessed_Internal(false, nFileID, strAction, nWorkflowID, vbAllowQueuedStatusOverride))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			NotifyFileProcessed_Internal(true, nFileID, strAction, nWorkflowID, vbAllowQueuedStatusOverride);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13541");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::NotifyFileFailed(long nFileID,  BSTR strAction, long nWorkflowID,
												 BSTR strException,
												 VARIANT_BOOL vbAllowQueuedStatusOverride)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check License
		validateLicense();

		if (!NotifyFileFailed_Internal(false, nFileID, strAction, nWorkflowID, strException, vbAllowQueuedStatusOverride))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			NotifyFileFailed_Internal(true, nFileID, strAction, nWorkflowID, strException, vbAllowQueuedStatusOverride);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13544");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetFileStatusToPending(long nFileID,  BSTR strAction,
													   VARIANT_BOOL vbAllowQueuedStatusOverride)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check License
		validateLicense();
		if (!SetFileStatusToPending_Internal(false, nFileID,  strAction, vbAllowQueuedStatusOverride))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			SetFileStatusToPending_Internal(true, nFileID,  strAction, vbAllowQueuedStatusOverride);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13546");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetFileStatusToUnattempted(long nFileID,  BSTR strAction,
														   VARIANT_BOOL vbAllowQueuedStatusOverride)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check License
		validateLicense();

		if (!SetFileStatusToUnattempted_Internal(false, nFileID, strAction, vbAllowQueuedStatusOverride))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			SetFileStatusToUnattempted_Internal(true, nFileID, strAction, vbAllowQueuedStatusOverride);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13548");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetFileStatusToSkipped(long nFileID, BSTR strAction,
													   VARIANT_BOOL bRemovePreviousSkipped,
													   VARIANT_BOOL vbAllowQueuedStatusOverride)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check License
		validateLicense();

		if (!SetFileStatusToSkipped_Internal(false, nFileID, strAction, bRemovePreviousSkipped,
				vbAllowQueuedStatusOverride))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			SetFileStatusToSkipped_Internal(true, nFileID, strAction, bRemovePreviousSkipped,
				vbAllowQueuedStatusOverride);
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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			SetStatusForAllFiles_Internal(true, strAction, eStatus);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13571");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetStatusForFile(long nID, BSTR strAction, long nWorkflowID,
												 EActionStatus eStatus,  
												 VARIANT_BOOL vbOverrideProcessing,
												 VARIANT_BOOL vbAllowQueuedStatusOverride,
												 EActionStatus *poldStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check License
		validateLicense();

		if (!SetStatusForFile_Internal(false, nID, strAction, nWorkflowID, eStatus, vbOverrideProcessing,
			vbAllowQueuedStatusOverride, poldStatus))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			SetStatusForFile_Internal(true, nID, strAction, nWorkflowID, eStatus, vbOverrideProcessing,
				vbAllowQueuedStatusOverride, poldStatus);
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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			RemoveFolder_Internal(true, strFolder, strAction);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13611");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetStats(long nActionID, VARIANT_BOOL vbForceUpdate,
	IActionStatistics* *pStats)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (!GetStats_Internal(false, nActionID, vbForceUpdate, pStats))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			GetStats_Internal(true, nActionID, vbForceUpdate, pStats);
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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			CopyActionStatusFromAction_Internal(true, nFromAction, nToAction);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14097");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::RenameAction(BSTR bstrOldActionName, BSTR bstrNewActionName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (!RenameAction_Internal(false, bstrOldActionName, bstrNewActionName))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			RenameAction_Internal(true, bstrOldActionName, bstrNewActionName);
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
	
		// Always lock the database for Clear()
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
			
		Clear_Internal(true, vbRetainUserValues);

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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
			
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
		ADODB::_ConnectionPtr ipConnection = __nullptr;
		
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
		TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

		// Delete all Lock records
		string strDelete = gstrDELETE_DB_LOCK;
		replaceVariable(strDelete, gstrDB_LOCK_NAME_VAL, gstrMAIN_DB_LOCK);
		executeCmdQuery(ipConnection, strDelete);
		
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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
			
			GetActionID_Internal(true, bstrActionName, pnActionID);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14986")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::ResetDBConnection(VARIANT_BOOL bResetCredentials)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		// Validate the license
		validateLicense();

		// [DotNetRCAndUtils:1113]
		// m_bLoggedInAsAdmin is no longer being reset when a connection is lost. Instead, callers
		// will set bResetCredentials to clear the credentials whenever a new connection is to be
		// obtained (as opposed to restoring a lost/failed connection).
		if (asCppBool(bResetCredentials))
		{
			m_bLoggedInAsAdmin = false;
		}

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
		closeAllDBConnections(false);

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
		ASSERT_ARGUMENT("ELI20471", pbLoginCancelled != __nullptr);
		ASSERT_ARGUMENT("ELI20472", pbLoginValid != __nullptr);

		// Convert bShowAdmin parameter to cpp bool for later use
		bool bUseAdmin  = asCppBool(bShowAdmin);
		string strUser = (bUseAdmin) ? gstrADMIN_USER : m_strFAMUserName;
		
		// Set the user password
		string strCaption = "Set " + strUser + "'s Password";

		// Set login valid and cancelled to false
		*pbLoginValid = VARIANT_FALSE;
		*pbLoginCancelled = VARIANT_FALSE;

		// [LegacyRCAndUtils:6168]
		// Initialize the DB if it is blank
		if (!initializeIfBlankDB(false, ""))
		{
			// If the user chose not to initialize an empty database, treat as a cancelled login.
			*pbLoginValid = VARIANT_FALSE;
			*pbLoginCancelled = VARIANT_TRUE;
			return S_OK;
		}

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

		bool bLoginValid = isPasswordValid(string(dlgLogin.m_zPassword), bUseAdmin);

		// If login was successful, set m_bLoggedInAsAdmin as appropriate.
		if (bLoginValid)
		{
			m_bLoggedInAsAdmin = bUseAdmin;
		}

		// Validate password
		*pbLoginValid = asVariantBool(bLoginValid);
	
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
		ASSERT_ARGUMENT("ELI15149", pVal != __nullptr);

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
		ASSERT_ARGUMENT("ELI17566", pVal != __nullptr);

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
		ASSERT_ARGUMENT("ELI17564", pVal != __nullptr);

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
		ASSERT_ARGUMENT("ELI17565", pVal != __nullptr);

		*pVal = get_bstr_t(m_strDatabaseName).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17623");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::CreateNewDB(BSTR bstrNewDBName, BSTR bstrInitWithPassword)
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

		string strInitWithPassword = asString(bstrInitWithPassword);

		// [LegacyRCAndUtils:6168]
		// Check for an existing, blank database.
		if (isBlankDB())
		{
			if (!strInitWithPassword.empty())
			{
				initializeIfBlankDB(true, strInitWithPassword);
			}

			// If this is a blank database, return without an exception; this will result in
			// ShowLogin being called and, therefore, the prompt to initialize the database.
			return S_OK;
		}
		
		// Create a connection object to the master db to create the database
		ADODB::_ConnectionPtr ipDBConnection(__uuidof(Connection)); 

		// Open a connection to the master database on the database server
		ipDBConnection->Open(createConnectionString(m_strDatabaseServer, "master").c_str(),
			"", "", adConnectUnspecified);

		// Query to create the database
		string strCreateDB = "CREATE DATABASE [" + m_strDatabaseName + "]";

		try
		{
			try
			{
				// Execute the query to create the new database
				ipDBConnection->Execute(strCreateDB.c_str(), NULL, adCmdText | adExecuteNoRecords);
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI41506")
		}
		catch (UCLIDException ex)
		{
			// ISSUE-13625
			// FAMDBAdmin: DB creation initially fails after having tried to create a DB that already exists
			// Remove the connection from the connection map on error, so it isn't re-used, as at this
			// point it is a connection to an existing database, not a new database.
			//
			try
			{
				ipDBConnection->Close();

				DWORD dwThreadID = GetCurrentThreadId();
				CSingleLock lg(&m_mutex, TRUE);
				m_mapThreadIDtoDBConnections.erase(dwThreadID);
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI41507")

			throw ex;
		}

		// Close the connections
		ipDBConnection->Close();

		if (!strInitWithPassword.empty())
		{
			initializeIfBlankDB(true, strInitWithPassword);
		}
		else
		{
			// Clear the new database to set up the tables
			clear(false, true, false);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17469");

}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::CreateNew80DB(BSTR bstrNewDBName)
{	
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Close any existing connection. P13 #4666
		closeDBConnection();

		// Database server needs to be set in order to create a new database
		if (m_strDatabaseServer.empty())
		{
			UCLIDException ue("ELI34233", "Database server must be set!");
			ue.addDebugInfo("New DB name", asString(bstrNewDBName));
			throw ue;
		}

		// Set the database name to the given database name
		m_strDatabaseName = asString(bstrNewDBName);
		
		if (m_strDatabaseName.empty())
		{
			UCLIDException ue("ELI34234", "Database name must not be empty!");
			throw ue;
		}
		
		// Create a connection object to the master db to create the database
		ADODB::_ConnectionPtr ipDBConnection(__uuidof(Connection)); 

		// Open a connection to the master database on the database server
		ipDBConnection->Open(createConnectionString(m_strDatabaseServer, "master").c_str(),
			"", "", adConnectUnspecified);

		// Query to create the database
		string strCreateDB = "CREATE DATABASE [" + m_strDatabaseName + "]";

		// Execute the query to create the new database
		ipDBConnection->Execute(strCreateDB.c_str(), NULL, adCmdText | adExecuteNoRecords);

		// Close the connections
		ipDBConnection->Close();

		// Initialize the new database
		init80DB();

		// The only reason an 80 DB is created is for the purpose of updating the schema. Set
		// this flag so that any calls to validateDBSchemaVersion during this update are ignored.
		m_bValidatingOrUpdatingSchema = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI34263");

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
		m_strAdvConnStrProperties = ms_strCurrAdvConnProperties;

		resetDBConnection();
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17842");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetDBInfoSetting(BSTR bstrSettingName, BSTR bstrSettingValue, 
										VARIANT_BOOL vbSetIfExists, VARIANT_BOOL vbRecordHistory)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		validateLicense();

		if (!SetDBInfoSetting_Internal(false, bstrSettingName, bstrSettingValue, vbSetIfExists, vbRecordHistory))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			SetDBInfoSetting_Internal(true, bstrSettingName, bstrSettingValue, vbSetIfExists, vbRecordHistory);
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

		ASSERT_ARGUMENT("ELI18938", pbstrSettingValue != __nullptr);

		validateLicense();

		bool bThrowIfMissing = asCppBool(vbThrowIfMissing);
		string strSettingName = asString(bstrSettingName);
		string strVal;
		if (!GetDBInfoSetting_Internal(false, strSettingName, bThrowIfMissing, strVal))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			GetDBInfoSetting_Internal(true, strSettingName, bThrowIfMissing, strVal);
		}

		*pbstrSettingValue = _bstr_t(strVal.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18937");

}

//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::LockDB_InternalOnly(BSTR bstrLockName)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		// Post message indicating that we are trying to lock the database
		postStatusUpdateNotification(kWaitingForLock);

		// lock the database
		lockDB(getDBConnection(), asString(bstrLockName));

		// Post message indicating that the database is now busy
		postStatusUpdateNotification(kConnectionBusy);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19084");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::UnlockDB_InternalOnly(BSTR bstrLockName)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		// Need to catch any exceptions and log them because this could be called within a catch
		// and don't want to throw an exception from a catch
		try
		{
			// Unlock the DB
			unlockDB(getDBConnection(), asString(bstrLockName));
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

		ASSERT_ARGUMENT("ELI19881", ppVal != __nullptr);

		validateLicense();


		if (!GetResultsForQuery_Internal(false, bstrQuery, ppVal))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
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

		ASSERT_ARGUMENT("ELI19899", pbstrStatusString != __nullptr);

		*pbstrStatusString = get_bstr_t(asStatusString(eaStatus)).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19897");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::AsStatusName(EActionStatus eaStatus, BSTR *pbstrStatusName)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		validateLicense();

		ASSERT_ARGUMENT("ELI35688", pbstrStatusName != __nullptr);

		*pbstrStatusName = get_bstr_t(asStatusName(eaStatus)).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35689");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::AsEActionStatus(BSTR bstrStatus, EActionStatus *peaStatus)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		validateLicense();

		ASSERT_ARGUMENT("ELI19900", peaStatus != __nullptr);

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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			GetActionName_Internal(true, nActionID, pbstrActionName);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26771");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::NotifyFileSkipped(long nFileID, BSTR bstrAction, long nWorkflowID,
												  VARIANT_BOOL vbAllowQueuedStatusOverride)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		validateLicense();

		if (!NotifyFileSkipped_Internal(false, nFileID, bstrAction, nWorkflowID, vbAllowQueuedStatusOverride))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
			
			NotifyFileSkipped_Internal(true, nFileID, bstrAction, nWorkflowID, vbAllowQueuedStatusOverride);
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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
			
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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
			
			HasTags_Internal(true, pvbVal);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32027");
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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
			
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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
			
			ToggleTagOnFile_Internal(true, nFileID, bstrTagName);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27354");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::AddTag(BSTR bstrTagName, BSTR bstrTagDescription,
	VARIANT_BOOL vbFailIfExists)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		string strTagName = asString(bstrTagName);
		string strDescription = asString(bstrTagDescription);

		// Validate the tag name
		validateTagName(strTagName);

		// Validate the description length
		if (strDescription.length() > 255)
		{
			UCLIDException ue("ELI29349", "Description is longer than 255 characters.");
			ue.addDebugInfo("Description", strDescription);
			ue.addDebugInfo("Description Length", strDescription.length());
			throw ue;
		}

		bool bFailIfExists = asCppBool(vbFailIfExists);
		if (!AddTag_Internal(false, strTagName, strDescription, bFailIfExists))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			AddTag_Internal(true, strTagName, strDescription, bFailIfExists);
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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
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
		ASSERT_ARGUMENT("ELI32002", pvbVal != __nullptr);

		validateLicense();

		*pvbVal = asVariantBool(m_bAllowDynamicTagCreation);

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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
			
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

		ASSERT_ARGUMENT("ELI27596", ppvecPriorities != __nullptr);

		IVariantVectorPtr ipVecPriorities(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI27597", ipVecPriorities != __nullptr);

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

		ASSERT_ARGUMENT("ELI27671", pbstrPriority != __nullptr);

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

		ASSERT_ARGUMENT("ELI27673", pePriority != __nullptr);

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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
			ExecuteCommandQuery_Internal(true, bstrQuery, pnRecordsAffected);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27686");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP 
CFileProcessingDB::ExecuteCommandReturnLongLongResult( BSTR bstrQuery, 
													  BSTR bstrResultColumnName,
													  long long* pResult,
													  long* pnRecordsAffected )
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		if (!ExecuteCommandReturnLongLongResult_Internal( false, 
														  bstrQuery, 
														  pnRecordsAffected, 
														  bstrResultColumnName, 
														  pResult ))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
			ExecuteCommandReturnLongLongResult_Internal( true, 
														 bstrQuery, 
														 pnRecordsAffected, 
														 bstrResultColumnName, 
														 pResult );
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI40318");
}

//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::RegisterActiveFAM()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_nFAMSessionID == 0)
		{
			throw UCLIDException("ELI38466",
				"Cannot register active FAM for session that does not exist.");
		}

		// Always lock the database for this call.
		// Not having a lock here may have had to do with some of the errors reported in
		// [LegacyRCAndUtils:6154]
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

		// This creates a record in the ActiveFAM table and the LastPingTime
		// is set to the current time by default.
		executeCmdQuery(getDBConnection(), 
			string("INSERT INTO ActiveFAM (FAMSessionID) ")
				+ "OUTPUT INSERTED.ID "
				+ "VALUES ('" + asString(m_nFAMSessionID) + "')",
			false, (long*)&m_nActiveFAMID);

		m_eventStopMaintainenceThreads.reset();
		m_eventPingThreadExited.reset();
		m_eventStatsThreadExited.reset();

		// set FAM registered flag
		m_bFAMRegistered = true;
		m_dwLastPingTime = 0;

		// Start thread here
		AfxBeginThread(maintainLastPingTimeForRevert, this);
		AfxBeginThread(maintainActionStatistics, this);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27726");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::UnregisterActiveFAM()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (!UnregisterActiveFAM_Internal(false))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
			UnregisterActiveFAM_Internal(true);
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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
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

		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
			gstrUSER_COUNTER_DB_LOCK);

		AddUserCounter_Internal(true, bstrCounterName, llInitialValue);

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

		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
			gstrUSER_COUNTER_DB_LOCK);

		RemoveUserCounter_Internal(true, bstrCounterName);

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

		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
			gstrUSER_COUNTER_DB_LOCK);

		RenameUserCounter_Internal(true, bstrCounterName, bstrNewCounterName);

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

		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
			gstrUSER_COUNTER_DB_LOCK);

		SetUserCounterValue_Internal(true , bstrCounterName, llNewValue);

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

		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
			gstrUSER_COUNTER_DB_LOCK);

		GetUserCounterValue_Internal(true, bstrCounterName, pllValue);

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
		
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
			gstrUSER_COUNTER_DB_LOCK);

		GetUserCounterNames_Internal(true, ppvecNames);

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

		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
			gstrUSER_COUNTER_DB_LOCK);

		GetUserCounterNamesAndValues_Internal(true, ppmapUserCounters);

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

		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
			gstrUSER_COUNTER_DB_LOCK);

		IsUserCounterValid_Internal(true, bstrCounterName, pbCounterValid);

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

		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
			gstrUSER_COUNTER_DB_LOCK);

		OffsetUserCounter_Internal(true, bstrCounterName, llOffsetValue, pllNewValue);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27817");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::RecordFAMSessionStart(BSTR bstrFPSFileName, BSTR bstrActionName,
												VARIANT_BOOL vbQueuing, VARIANT_BOOL vbProcessing)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Always lock the database for this call.
		// Not having a lock here may have had to do with some of the errors reported in
		// [LegacyRCAndUtils:6154]
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

		RecordFAMSessionStart_Internal(true, bstrFPSFileName, bstrActionName, vbQueuing, vbProcessing);

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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
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

		ASSERT_ARGUMENT("ELI29207", pvbSkipAuthentication != __nullptr);

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = __nullptr;
		
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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
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
		ASSERT_ARGUMENT("ELI29861", pnNumberOfRetries != __nullptr);
		ASSERT_ARGUMENT("ELI29862", pdRetryTimeout != __nullptr);

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

		UpgradeToCurrentSchema_Internal(true, ipProgressStatus);

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
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
			RenameFile_Internal(true, pFileRecord, bstrNewName);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31463");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::get_DBInfoSettings(IStrToStrMap** ppSettings)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		ASSERT_ARGUMENT("ELI31907", ppSettings != __nullptr);

		if (!get_DBInfoSettings_Internal(false, ppSettings))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
			get_DBInfoSettings_Internal(true, ppSettings);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31908");

}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetDBInfoSettings(IStrToStrMap* pSettings, long* pnNumRowsUpdated)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		IStrToStrMapPtr ipSettings(pSettings);
		ASSERT_ARGUMENT("ELI31909", ipSettings != __nullptr);
		ASSERT_ARGUMENT("ELI32173", pnNumRowsUpdated != __nullptr);

		string strBaseQuery = m_bStoreDBInfoChangeHistory ? gstrDBINFO_UPDATE_SETTINGS_QUERY_STORE_HISTORY
			: gstrDBINFO_UPDATE_SETTINGS_QUERY;

		IIUnknownVectorPtr ipPairs = ipSettings->GetAllKeyValuePairs();
		ASSERT_RESOURCE_ALLOCATION("ELI31910", ipPairs != __nullptr);

		// Get the key value pairs from the StrToStrMap and create the update queries
		int nSize = ipPairs->Size();
		vector<string> vecQueries;
		vecQueries.reserve(nSize);
		for(int i=0; i < nSize; i++)
		{
			IStringPairPtr ipPair = ipPairs->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI31911", ipPair != __nullptr);

			_bstr_t bstrKey;
			_bstr_t bstrValue;
			ipPair->GetKeyValuePair(bstrKey.GetAddress(), bstrValue.GetAddress());

			string strQuery = strBaseQuery;
			replaceVariable(strQuery, gstrSETTING_NAME, asString(bstrKey), kReplaceAll);
			replaceVariable(strQuery, gstrSETTING_VALUE, asString(bstrValue), kReplaceAll);
			vecQueries.push_back(strQuery);
		}

		long nNumRowsUpdated = 0;
		if (!SetDBInfoSettings_Internal(false, vecQueries, nNumRowsUpdated))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
			SetDBInfoSettings_Internal(true, vecQueries, nNumRowsUpdated);
		}

		*pnNumRowsUpdated = nNumRowsUpdated;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31912");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::RecordFTPEvent(long nFileId, long nActionID,
	VARIANT_BOOL vbQueueing, EFTPAction eFTPAction, BSTR bstrServerAddress,
	BSTR bstrUserName, BSTR bstrArg1, BSTR bstrArg2, long nRetries, BSTR bstrException)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		if (!RecordFTPEvent_Internal(false, nFileId, nActionID, vbQueueing, eFTPAction,
				bstrServerAddress, bstrUserName, bstrArg1, bstrArg2, nRetries, bstrException))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
			RecordFTPEvent_Internal(true, nFileId, nActionID, vbQueueing, eFTPAction, bstrServerAddress,
				bstrUserName, bstrArg1, bstrArg2, nRetries, bstrException);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33988");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::RecalculateStatistics()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Lock the database
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

		ADODB::_ConnectionPtr ipConnection = __nullptr;
		
		BEGIN_CONNECTION_RETRY();

		assertProcessingNotActiveForAnyAction(true);

		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();
		
		IStrToStrMapPtr ipMapActions = getActions(ipConnection);
		ASSERT_RESOURCE_ALLOCATION("ELI34335", ipMapActions != __nullptr);		

		// Loop through each action to recalculate the statistics.
		long lSize = ipMapActions->Size;
		vector<int> vecActionIDs(lSize);
		for (int i = 0; i < lSize; i++)
		{
			_bstr_t bstrKey;
			_bstr_t bstrValue;
			ipMapActions->GetKeyValue(i, bstrKey.GetAddress(), bstrValue.GetAddress());
		
			int nActionID = asLong(asString(bstrValue));
	
			reCalculateStats(ipConnection, nActionID);
		}

		END_CONNECTION_RETRY(ipConnection, "ELI34329")
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI34328");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::IsAnyFAMActive(VARIANT_BOOL* pvbFAMIsActive)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		ASSERT_ARGUMENT("ELI34330", pvbFAMIsActive != __nullptr);

		if (!IsAnyFAMActive_Internal(false, pvbFAMIsActive))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			IsAnyFAMActive_Internal(true, pvbFAMIsActive);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI34333");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::put_RetryOnTimeout(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		m_bRetryOnTimeout = asCppBool(newVal);
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI34337");

}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::get_RetryOnTimeout(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		ASSERT_ARGUMENT("ELI34338", pVal != __nullptr);

		*pVal = asVariantBool(m_bRetryOnTimeout);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI34339");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::put_AdvancedConnectionStringProperties(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		m_strAdvConnStrProperties = asString(newVal);

		// Set the static advanced connection string properties.
		CSingleLock lock(&m_mutex, TRUE);
		ms_strCurrAdvConnProperties = m_strAdvConnStrProperties;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35141");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::get_AdvancedConnectionStringProperties(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI35139", pVal != __nullptr);

		*pVal = _bstr_t(m_strAdvConnStrProperties.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35140");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::get_IsConnected(VARIANT_BOOL* pbIsConnected)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI35707", pbIsConnected != __nullptr);

		*pbIsConnected = asCppBool(m_strCurrentConnectionStatus == gstrCONNECTION_ESTABLISHED);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35708");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::ShowSelectDB(BSTR bstrPrompt, VARIANT_BOOL bAllowCreation,
									VARIANT_BOOL bRequireAdminLogin, VARIANT_BOOL* pbConnected)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI35754", pbConnected != __nullptr);

		// Display the initial dialog box
		SelectDBDialog dlgSelectDB(getThisAsCOMPtr(), asString(bstrPrompt),
			asCppBool(bAllowCreation), asCppBool(bRequireAdminLogin));

		// Display the Select DB dialog and return whether it resulted in connecting to a selected
		// database.
		*pbConnected = asVariantBool(dlgSelectDB.DoModal() == IDOK);

		return S_OK;

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35755")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetFileCount(VARIANT_BOOL bUseOracleSyntax, LONGLONG* pnFileCount)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI35770", pnFileCount != __nullptr);

		validateLicense();

		if (!GetFileCount_Internal(false, bUseOracleSyntax, pnFileCount))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			GetFileCount_Internal(true, bUseOracleSyntax, pnFileCount);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35760")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::get_ConnectionString(BSTR* pbstrConnectionString)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI35945", pbstrConnectionString != __nullptr);

		// https://extract.atlassian.net/browse/ISSUE-12312
		// Because each thread establishes its own DB connections (which modified the previously
		// existing m_strCurrentConnectionString), m_strCurrentConnectionString could not be used
		// to reliably obtain the connection string across all threads. Instead generate the
		// connection string that would be used whether or not we are currently connected.
		string strConnectionString = createConnectionString(
			m_strDatabaseServer, m_strDatabaseName, m_strAdvConnStrProperties);

		*pbstrConnectionString = _bstr_t(strConnectionString.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35946");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::get_LoggedInAsAdmin(VARIANT_BOOL* pbLoggedInAsAdmin)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI36046", pbLoggedInAsAdmin != __nullptr);

		*pbLoggedInAsAdmin = asVariantBool(m_bLoggedInAsAdmin);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36047");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::IsFeatureEnabled(BSTR bstrFeatureName, VARIANT_BOOL* pbFeatureIsEnabled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI36070", pbFeatureIsEnabled != __nullptr);

		validateLicense();

		if (!IsFeatureEnabled_Internal(false, bstrFeatureName, pbFeatureIsEnabled))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			IsFeatureEnabled_Internal(true, bstrFeatureName, pbFeatureIsEnabled);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36071")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::DuplicateConnection(IFileProcessingDB *pConnectionSource)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipConnectionSource(pConnectionSource);
		ASSERT_ARGUMENT("ELI36072", ipConnectionSource != __nullptr);

		validateLicense();

		// Ensure this instance doesn't have any open connections
		closeAllDBConnections(false);

		// Copy the connection information from ipConnectionSource
		UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipThis = getThisAsCOMPtr();
		ipThis->DatabaseServer = ipConnectionSource->DatabaseServer;
		ipThis->DatabaseName = ipConnectionSource->DatabaseName;
		ipThis->AdvancedConnectionStringProperties =
			ipConnectionSource->AdvancedConnectionStringProperties;

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		_ConnectionPtr ipConnection = __nullptr;
		
		BEGIN_CONNECTION_RETRY();
		
		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		END_CONNECTION_RETRY(ipConnection, "ELI36073");

		// Inherit the credentials of the source connection.
		m_bLoggedInAsAdmin = asCppBool(ipConnectionSource->LoggedInAsAdmin);
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36074")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetWorkItemsForGroup(long nWorkItemGroupID, long nStartPos, long nCount, IIUnknownVector **ppWorkItems)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		
		if (!GetWorkItemsForGroup_Internal(false, nWorkItemGroupID, nStartPos, nCount, ppWorkItems))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrWORKITEM_DB_LOCK);
			GetWorkItemsForGroup_Internal(true, nWorkItemGroupID, nStartPos, nCount, ppWorkItems);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36892");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetWorkItemGroupStatus(long nWorkItemGroupID, WorkItemGroupStatus *pWorkGroupStatus, 
	EWorkItemStatus *pStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		
		if (!GetWorkItemGroupStatus_Internal(false, nWorkItemGroupID, pWorkGroupStatus, pStatus))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrWORKITEM_DB_LOCK);
			GetWorkItemGroupStatus_Internal(true, nWorkItemGroupID, pWorkGroupStatus, pStatus);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36893");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::CreateWorkItemGroup(long nFileID, long nActionID,  
			BSTR stringizedTask, long nNumberOfWorkItems, BSTR bstrRunningTaskDescription, long *pnWorkItemGroupID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		
		if (!CreateWorkItemGroup_Internal(false, nFileID, nActionID, stringizedTask, nNumberOfWorkItems,
			bstrRunningTaskDescription, pnWorkItemGroupID))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrWORKITEM_DB_LOCK);
			CreateWorkItemGroup_Internal(true,  nFileID, nActionID, stringizedTask, nNumberOfWorkItems, 
				bstrRunningTaskDescription, pnWorkItemGroupID);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37092");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::AddWorkItems(long nWorkItemGroupID, IIUnknownVector *pWorkItems)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		
		if (!AddWorkItems_Internal(false, nWorkItemGroupID, pWorkItems))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrWORKITEM_DB_LOCK);
			AddWorkItems_Internal(true, nWorkItemGroupID, pWorkItems);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36894");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetWorkItemToProcess(long nActionID,
								VARIANT_BOOL vbRestrictToFAMSession, IWorkItemRecord **pWorkItem)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		
		if (!GetWorkItemToProcess_Internal(false, nActionID, vbRestrictToFAMSession, pWorkItem))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrWORKITEM_DB_LOCK);
			GetWorkItemToProcess_Internal(true, nActionID, vbRestrictToFAMSession, pWorkItem);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36895");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::NotifyWorkItemFailed(long nWorkItemID, BSTR stringizedException)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		
		if (!NotifyWorkItemFailed_Internal(false, nWorkItemID, stringizedException))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrWORKITEM_DB_LOCK);
			NotifyWorkItemFailed_Internal(true, nWorkItemID, stringizedException);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36896");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::NotifyWorkItemCompleted(long nWorkItemID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		
		if (!NotifyWorkItemCompleted_Internal(false, nWorkItemID))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrWORKITEM_DB_LOCK);
			NotifyWorkItemCompleted_Internal(true, nWorkItemID);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36897");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetWorkGroupData(long WorkItemGroupID, long *pnNumberOfWorkItems,
	BSTR *pstringizedTask)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		
		if (!GetWorkGroupData_Internal(false, WorkItemGroupID, pnNumberOfWorkItems, pstringizedTask))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrWORKITEM_DB_LOCK);
			GetWorkGroupData_Internal(true, WorkItemGroupID, pnNumberOfWorkItems, pstringizedTask);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36898");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SaveWorkItemOutput(long WorkItemID, BSTR strWorkItemOutput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		
		if (!SaveWorkItemOutput_Internal(false, WorkItemID, strWorkItemOutput))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrWORKITEM_DB_LOCK);
			SaveWorkItemOutput_Internal(true, WorkItemID, strWorkItemOutput);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36920");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::FindWorkItemGroup(long nFileID, long nActionID, BSTR stringizedTask, 
	long nNumberOfWorkItems, BSTR bstrRunningTaskDescription, long *pnWorkItemGroupID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		
		if (!FindWorkItemGroup_Internal(false, nFileID, nActionID, stringizedTask, nNumberOfWorkItems, 
			bstrRunningTaskDescription, pnWorkItemGroupID))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrWORKITEM_DB_LOCK);
			FindWorkItemGroup_Internal(true,  nFileID, nActionID, stringizedTask, nNumberOfWorkItems, 
				bstrRunningTaskDescription, pnWorkItemGroupID);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37164");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SaveWorkItemBinaryOutput(long WorkItemID, IUnknown *pBinaryOutput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		
		if (!SaveWorkItemBinaryOutput_Internal(false, WorkItemID, pBinaryOutput))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrWORKITEM_DB_LOCK);
			SaveWorkItemBinaryOutput_Internal(true, WorkItemID, pBinaryOutput);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37171");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetFileSets(IVariantVector **pvecFileSetNames)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI37339", pvecFileSetNames != __nullptr);
		
		IVariantVectorPtr ipFileSetNames(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI37340", ipFileSetNames != __nullptr);

		for (csis_map<vector<int>>::type::iterator iter = m_mapFileSets.begin();
			 iter != m_mapFileSets.end();
			 iter++)
		{
			ipFileSetNames->PushBack(get_bstr_t(iter->first));
		}

		*pvecFileSetNames = ipFileSetNames.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37341");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::AddFileSet(BSTR bstrFileSetName, IVariantVector *pvecIDs)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		
		// Make sure the DB Schema is the expected version
		validateDBSchemaVersion();

		string strFileSetName = asString(bstrFileSetName);

		IVariantVectorPtr ipvecIDs(pvecIDs);
		ASSERT_ARGUMENT("ELI37342", ipvecIDs != __nullptr);

		vector<int> vecFileIDs;
		long nCount = ipvecIDs->Size;
		for (long i = 0; i < nCount; i++)
		{
			vecFileIDs.push_back(ipvecIDs->Item[i].lVal);
		}

		m_mapFileSets[strFileSetName] = vecFileIDs;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37343");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetFileSetFileIDs(BSTR bstrFileSetName,
												  IVariantVector **ppvecFileIDs)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI37344", ppvecFileIDs != __nullptr);
		
		IVariantVectorPtr ipvecFileIDs(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI37345", ipvecFileIDs != __nullptr);

		string strFileSetName = asString(bstrFileSetName);

		csis_map<vector<int>>::type::iterator iterFileSet = m_mapFileSets.find(strFileSetName);
		if (iterFileSet == m_mapFileSets.end())
		{
			UCLIDException ue("ELI37346", "File set not found");
			ue.addDebugInfo("Set Name", strFileSetName);
			throw ue;
		}

		vector<int>& vecFileIDs = iterFileSet->second;

		for each (int nFileID in vecFileIDs)
		{
			ipvecFileIDs->PushBack(nFileID);
		}

		*ppvecFileIDs = ipvecFileIDs.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37347");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetFileSetFileNames(BSTR bstrFileSetName,
													IVariantVector **ppvecFileNames)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		if (!GetFileSetFileNames_Internal(false, bstrFileSetName, ppvecFileNames))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			GetFileSetFileNames_Internal(true, bstrFileSetName, ppvecFileNames);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37348");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetFileToProcess(long nFileID, BSTR strAction,
												 IFileRecord** ppFileRecord)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		if (!GetFileToProcess_Internal(false, nFileID, strAction, ppFileRecord))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			GetFileToProcess_Internal(true, nFileID, strAction, ppFileRecord);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37458");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetFallbackStatus(IFileRecord* pFileRecord,
												  EActionStatus eaFallbackStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		if (!SetFallbackStatus_Internal(false, pFileRecord, eaFallbackStatus))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			SetFallbackStatus_Internal(true, pFileRecord, eaFallbackStatus);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37459");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetWorkItemsToProcess(long nActionID, VARIANT_BOOL vbRestrictToFAMSessionID, 
			long nMaxWorkItemsToReturn, EFilePriority eMinPriority, IIUnknownVector **pWorkItems)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		if (!GetWorkItemsToProcess_Internal(false, nActionID, vbRestrictToFAMSessionID,
			nMaxWorkItemsToReturn, eMinPriority, pWorkItems))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrWORKITEM_DB_LOCK);

			GetWorkItemsToProcess_Internal(true, nActionID, vbRestrictToFAMSessionID,
				nMaxWorkItemsToReturn, eMinPriority, pWorkItems);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37417");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetWorkItemToPending(long nWorkItemID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		if (!SetWorkItemToPending_Internal(false, nWorkItemID))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrWORKITEM_DB_LOCK);

			SetWorkItemToPending_Internal(true, nWorkItemID);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37420");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetFailedWorkItemsForGroup(long nWorkItemGroupID, IIUnknownVector **ppWorkItems)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		if (!GetFailedWorkItemsForGroup_Internal(false, nWorkItemGroupID, ppWorkItems))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrWORKITEM_DB_LOCK);

			GetFailedWorkItemsForGroup_Internal(true, nWorkItemGroupID, ppWorkItems);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37539");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetMetadataFieldValue(long nFileID, BSTR bstrMetadataFieldName,
													  BSTR bstrMetadataFieldValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		if (!SetMetadataFieldValue_Internal(false, nFileID, bstrMetadataFieldName, bstrMetadataFieldValue))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			SetMetadataFieldValue_Internal(true,  nFileID, bstrMetadataFieldName, bstrMetadataFieldValue);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37557");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetMetadataFieldValue(long nFileID, BSTR bstrMetadataFieldName,
													  BSTR *pbstrMetadataFieldValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		if (!GetMetadataFieldValue_Internal(false, nFileID, bstrMetadataFieldName, pbstrMetadataFieldValue))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			GetMetadataFieldValue_Internal(true,  nFileID, bstrMetadataFieldName, pbstrMetadataFieldValue);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37637");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetLastConnectionStringConfiguredThisProcess(BSTR *pbstrConnectionString)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();
		
		ASSERT_ARGUMENT("ELI37874", pbstrConnectionString != __nullptr);

		CSingleLock lock(&m_mutex, TRUE);
		string strConnectionString = createConnectionString(
			ms_strCurrServerName, ms_strCurrDBName, ms_strCurrAdvConnProperties);

		*pbstrConnectionString = _bstr_t(strConnectionString.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37875");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::AddMetadataField(BSTR bstrMetadataFieldName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		string strMetadataFieldName = asString(bstrMetadataFieldName);

		// Validate the tag name
		validateMetadataFieldName(strMetadataFieldName);

		if (!AddMetadataField_Internal(false, strMetadataFieldName))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			AddMetadataField_Internal(true, strMetadataFieldName);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37710");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::DeleteMetadataField(BSTR bstrMetadataFieldName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		if (!DeleteMetadataField_Internal(false, bstrMetadataFieldName))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			DeleteMetadataField_Internal(true, bstrMetadataFieldName);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37711");

}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::RenameMetadataField(BSTR bstrOldMetadataFieldName, BSTR bstrNewMetadataFieldName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		if (!RenameMetadataField_Internal(false, bstrOldMetadataFieldName, bstrNewMetadataFieldName))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
			RenameMetadataField_Internal(true, bstrOldMetadataFieldName, bstrNewMetadataFieldName);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37712");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetMetadataFieldNames(IVariantVector **ppMetadataFieldNames)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		if (!GetMetadataFieldNames_Internal(false, ppMetadataFieldNames))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			GetMetadataFieldNames_Internal(true, ppMetadataFieldNames);
		}
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37656");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::get_ActiveFAMID(long *pnActiveFAMID)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI38461", pnActiveFAMID != __nullptr);

		*pnActiveFAMID = m_nActiveFAMID;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38462");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::get_FAMSessionID(long *pnFAMSessionID)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI38550", pnFAMSessionID != __nullptr);

		*pnFAMSessionID = m_nFAMSessionID;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38551");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::StartFileTaskSession(BSTR bstrTaskClassGuid, long nFileID,
	long *pnFileTaskSessionID)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI38638", pnFileTaskSessionID != nullptr);

		if (!StartFileTaskSession_Internal(false, bstrTaskClassGuid, nFileID, pnFileTaskSessionID))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			StartFileTaskSession_Internal(true, bstrTaskClassGuid, nFileID, pnFileTaskSessionID);
		}
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38639");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::UpdateFileTaskSession(long nFileTaskSessionID,
													  double dDuration, double dOverheadTime)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		if (!UpdateFileTaskSession_Internal(false, nFileTaskSessionID, dDuration, dOverheadTime))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			UpdateFileTaskSession_Internal(true, nFileTaskSessionID, dDuration, dOverheadTime);
		}
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI39696");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetFileNameFromFileID( long fileID, BSTR* pbstrFileName )
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		std::string query = Util::Format( "SELECT [FileName] FROM [dbo].[FAMFile] "
										  "WHERE [ID]=%d;", 
										  fileID );

		ADODB::_RecordsetPtr ipRecords = getThisAsCOMPtr()->GetResultsForQuery( query.c_str() );
		ASSERT_RUNTIME_CONDITION( "ELI38704", 
								  VARIANT_FALSE == ipRecords->adoEOF, 
								  Util::Format("No filename found for fileID: %d", 
											   fileID).c_str() );

		FieldsPtr ipFields = ipRecords->Fields;
		ASSERT_RESOURCE_ALLOCATION("ELI38622", ipFields != nullptr);

		std::string filename = getStringField( ipFields, "FileName" );
		ASSERT_RUNTIME_CONDITION( "ELI38705", 
								  !filename.empty(), 
								  Util::Format("Empty filename found for FileID: %d", 
											   fileID).c_str() );

		*pbstrFileName = get_bstr_t(filename.c_str()).Detach();
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38703");	
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetSecureCounters(VARIANT_BOOL vbRefresh,
	IIUnknownVector** ppSecureCounters)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();
		ASSERT_ARGUMENT("ELI39076", ppSecureCounters != nullptr);

		if (!GetSecureCounters_Internal(false, vbRefresh, ppSecureCounters))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(
				getThisAsCOMPtr(), gstrSECURE_COUNTER_DB_LOCK);

			GetSecureCounters_Internal(true, vbRefresh, ppSecureCounters);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI39077");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetSecureCounterName (long nCounterID, BSTR *pstrCounterName)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();
		ASSERT_ARGUMENT("ELI38772", pstrCounterName != nullptr);

		if (!GetSecureCounterName_Internal(false, nCounterID, pstrCounterName))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), 
				gstrSECURE_COUNTER_DB_LOCK);

			GetSecureCounterName_Internal(true, nCounterID, pstrCounterName);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38773");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::ApplySecureCounterUpdateCode (BSTR strUpdateCode, BSTR *pbstrResult)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();
		
		if (!ApplySecureCounterUpdateCode_Internal(false, strUpdateCode, pbstrResult))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), 
				gstrSECURE_COUNTER_DB_LOCK);

			ApplySecureCounterUpdateCode_Internal(true, strUpdateCode, pbstrResult);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38775");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetSecureCounterValue (long nCounterID, long* pnCounterValue)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();
		ASSERT_ARGUMENT("ELI38776", pnCounterValue != nullptr);

		if (!GetSecureCounterValue_Internal(false, nCounterID, pnCounterValue))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), 
				gstrSECURE_COUNTER_DB_LOCK);

			GetSecureCounterValue_Internal(true, nCounterID, pnCounterValue);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38777");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::DecrementSecureCounter (long nCounterID, long decrementAmount, long* pnCounterValue)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();
		ASSERT_ARGUMENT("ELI38778", pnCounterValue != nullptr);

		if (!DecrementSecureCounter_Internal(false, nCounterID, decrementAmount, pnCounterValue))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), 
				gstrSECURE_COUNTER_DB_LOCK);

			DecrementSecureCounter_Internal(true, nCounterID, decrementAmount, pnCounterValue);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38779");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SecureCounterConsistencyCheck (VARIANT_BOOL* pvbValid)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();
		ASSERT_ARGUMENT("ELI38780", pvbValid != nullptr);
		
		if (!SecureCounterConsistencyCheck_Internal(false, pvbValid))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), 
				gstrSECURE_COUNTER_DB_LOCK);

			SecureCounterConsistencyCheck_Internal(true,  pvbValid);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38781");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetCounterUpdateRequestCode (BSTR* pstrUpdateRequestCode)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();
		ASSERT_ARGUMENT("ELI38788", pstrUpdateRequestCode != nullptr);
		
		if (!GetCounterUpdateRequestCode_Internal(false, pstrUpdateRequestCode))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), 
				gstrSECURE_COUNTER_DB_LOCK);

			GetCounterUpdateRequestCode_Internal(true,  pstrUpdateRequestCode);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38789");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::get_DatabaseID(BSTR* pbstrDatabaseID)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();
		ASSERT_ARGUMENT("ELI39078", pbstrDatabaseID != nullptr);

		checkDatabaseIDValid(getDBConnection(), false);
		string strDatabaseID = asString(m_DatabaseIDValues.m_GUID);
		replaceVariable(strDatabaseID, "{", "");
		replaceVariable(strDatabaseID, "}", "");
		
		*pbstrDatabaseID = get_bstr_t(strDatabaseID.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI39079");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::get_ConnectedDatabaseServer(BSTR* pbstrDatabaseServer)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();
		ASSERT_ARGUMENT("ELI39080", pbstrDatabaseServer != nullptr);

		checkDatabaseIDValid(getDBConnection(), false);
		
		*pbstrDatabaseServer = get_bstr_t(m_DatabaseIDValues.m_strServer.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI39081");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::get_ConnectedDatabaseName(BSTR* pbstrDatabaseName)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();
		ASSERT_ARGUMENT("ELI39082", pbstrDatabaseName != nullptr);

		checkDatabaseIDValid(getDBConnection(), false);
		
		*pbstrDatabaseName = get_bstr_t(m_DatabaseIDValues.m_strName.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI39083");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetSecureCounterAlertLevel(long nCounterID, long nAlertLevel,
														   long nAlertMultiple)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();
		ASSERT_ARGUMENT("ELI39128", nCounterID > 0 && nCounterID < 1000);
		ASSERT_ARGUMENT("ELI39129", nAlertLevel >= 0);
		ASSERT_ARGUMENT("ELI39130", nAlertMultiple >= 0);
		
		if (!SetSecureCounterAlertLevel_Internal(false, nCounterID, nAlertLevel, nAlertMultiple))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), 
				gstrSECURE_COUNTER_DB_LOCK);

			SetSecureCounterAlertLevel_Internal(true, nCounterID, nAlertLevel, nAlertMultiple);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI39131");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::AddFileNoQueue(BSTR bstrFile, long long llFileSize,
											   long lPageCount, EFilePriority ePriority, long* pnID)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();
		
		if (!AddFileNoQueue_Internal(false, bstrFile, llFileSize, lPageCount, ePriority, pnID))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), 
				gstrMAIN_DB_LOCK);

			AddFileNoQueue_Internal(true, bstrFile, llFileSize, lPageCount, ePriority, pnID);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI39575");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::AddPaginationHistory(BSTR bstrOutputFile,
													 IIUnknownVector* pSourcePageInfo,
													 long nFileTaskSessionID)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();
		
		if (!AddPaginationHistory_Internal(false, bstrOutputFile, pSourcePageInfo, nFileTaskSessionID))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), 
				gstrMAIN_DB_LOCK);

			AddPaginationHistory_Internal(true, bstrOutputFile, pSourcePageInfo, nFileTaskSessionID);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI39682");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::AddWorkflow(BSTR bstrName, EWorkflowType eType, long* pnID)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		if (!AddWorkflow_Internal(false, bstrName, eType, pnID))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
				gstrMAIN_DB_LOCK);

			AddWorkflow_Internal(true, bstrName, eType, pnID);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41888");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::DeleteWorkflow(long nID)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		if (!DeleteWorkflow_Internal(false, nID))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
				gstrMAIN_DB_LOCK);

			DeleteWorkflow_Internal(true, nID);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41889");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetWorkflowDefinition(long nID,
		IWorkflowDefinition** ppWorkflowDefinition)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		if (!GetWorkflowDefinition_Internal(false, nID, ppWorkflowDefinition))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
				gstrMAIN_DB_LOCK);

			GetWorkflowDefinition_Internal(true, nID, ppWorkflowDefinition);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41890");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetWorkflowDefinition(IWorkflowDefinition* pWorkflowDefinition)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		if (!SetWorkflowDefinition_Internal(false, pWorkflowDefinition))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
				gstrMAIN_DB_LOCK);

			SetWorkflowDefinition_Internal(true, pWorkflowDefinition);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41891");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetWorkflows(IStrToStrMap ** pmapWorkFlowNameToID)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		if (!GetWorkflows_Internal(false, pmapWorkFlowNameToID))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
				gstrMAIN_DB_LOCK);

			GetWorkflows_Internal(true, pmapWorkFlowNameToID);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41932");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetWorkflowActions(long nID, IStrToStrMap** pmapActionNameToID)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		if (!GetWorkflowActions_Internal(false, nID, pmapActionNameToID))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
				gstrMAIN_DB_LOCK);

			GetWorkflowActions_Internal(true, nID, pmapActionNameToID);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41987");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetWorkflowActions(long nID, IVariantVector* pActionList)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		if (!SetWorkflowActions_Internal(false, nID, pActionList))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
				gstrMAIN_DB_LOCK);

			SetWorkflowActions_Internal(true, nID, pActionList);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41988");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::get_ActiveWorkflow(BSTR* pbstrWorkflowName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI42026", pbstrWorkflowName != __nullptr);

		*pbstrWorkflowName = get_bstr_t(m_strActiveWorkflow).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI42027");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::put_ActiveWorkflow(BSTR bstrWorkflowName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		if (m_nFAMSessionID != 0)
		{
			throw UCLIDException("ELI42030", "Cannot set workflow while a session is open.");
		}

		m_strActiveWorkflow = asString(bstrWorkflowName);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI42025");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::get_ActiveActionID(long* pnActionID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI42083", pnActionID != __nullptr);

		*pnActionID = m_nActiveActionID;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI42084");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetStatsAllWorkflows(BSTR bstrActionName, VARIANT_BOOL vbForceUpdate,
													 IActionStatistics** pStats)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		if (!GetStatsAllWorkflows_Internal(false, bstrActionName, vbForceUpdate, pStats))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
				gstrMAIN_DB_LOCK);

			GetStatsAllWorkflows_Internal(true, bstrActionName, vbForceUpdate, pStats);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI42085");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetAllActions(IStrToStrMap** pmapActionNameToID)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		if (!GetAllActions_Internal(false, pmapActionNameToID))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
				gstrMAIN_DB_LOCK);

			GetAllActions_Internal(true, pmapActionNameToID);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI42085");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetWorkflowStatus(long nFileID, EActionStatus* peaStatus)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		if (!GetWorkflowStatus_Internal(false, nFileID, peaStatus))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
				gstrMAIN_DB_LOCK);

			GetWorkflowStatus_Internal(true, nFileID, peaStatus);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI42135");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetWorkflowStatusAllFiles(long *pnUnattempted, long *pnProcessing,
														  long *pnCompleted, long *pnFailed)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		if (!GetWorkflowStatusAllFiles_Internal(false, pnUnattempted, pnProcessing, pnCompleted, pnFailed))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
				gstrMAIN_DB_LOCK);

			GetWorkflowStatusAllFiles_Internal(true, pnUnattempted, pnProcessing, pnCompleted, pnFailed);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI42153");
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
