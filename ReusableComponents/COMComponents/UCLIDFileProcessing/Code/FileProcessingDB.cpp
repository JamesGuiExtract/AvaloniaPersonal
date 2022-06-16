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
#include <StringTokenizer.h>
#include <SqlApplicationRole.h>
#include <ValueRestorer.h>

#include <atlsafe.h>

#include <string>
#include <stack>
#include <map>

using namespace ADODB;
using namespace FAMUtils;
using namespace std;

// add license management function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// Static Members
//-------------------------------------------------------------------------------------------------
std::string CFileProcessingDB::ms_strCurrServerName = "";
std::string CFileProcessingDB::ms_strCurrDBName = "";
std::string CFileProcessingDB::ms_strCurrAdvConnProperties = "";
std::string CFileProcessingDB::ms_strLastUsedAdvConnStr = "";
std::string CFileProcessingDB::ms_strLastWorkflow = "";

// TODO: Seems like this should be tracked per database
// (since this is a static and there could be more than one DB in use for process)
DWORD CFileProcessingDB::ms_dwLastRevertTime;

CMutex CFileProcessingDB::ms_mutexPingDBLock;
CMutex CFileProcessingDB::ms_mutexAutoRevertLock;
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
	m_bLoadBalance(true),
	m_iNumberOfRetries(giDEFAULT_RETRY_COUNT),
	m_bNumberOfRetriesOverridden(false),
	m_dRetryTimeout(gdDEFAULT_RETRY_TIMEOUT),
	m_bRetryTimeoutOverridden(false),
	m_nActiveFAMID(0),
	m_nFAMSessionID(0),
	m_strFPSFileName(""),
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
	m_nActiveWorkflowID(0),
	m_bUsingWorkflowsForCurrentAction(false),
	m_bRunningAllWorkflows(false),
	m_nLastFAMFileID(0),
	m_bDeniedFastCountPermission(false),
	m_ipFAMTagManager(__nullptr),
	m_bCurrentSessionIsWebSession(false),
	m_dwLastPingTime(0),
	m_ipDBInfoSettings(__nullptr),
	m_currentRole(AppRole::kExtractRole),
	m_roleUtility()
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

		m_ipFAMTagManager.CreateInstance(CLSID_FAMTagManager);
		ASSERT_RESOURCE_ALLOCATION("ELI43201", m_ipFAMTagManager != __nullptr);

		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI29457", m_ipMiscUtils != __nullptr);

		// set the Unique Process Identifier string to be used to in locking the database
		m_strUPI = UPI::getCurrentProcessUPI().getUPI();
		m_strMachineName = getComputerName();
		m_strFAMUserName = getCurrentUserName();
		m_lDBLockTimeout = m_regFPCfgMgr.getDBLockTimeout();
		
		// https://extract.atlassian.net/browse/ISSUE-15247
		// Until these threads are started signal them to indicate they are not running.
		// Otherwise, this can cause ensureFAMRegistration to hang waiting for threads
		// that aren't running.
		m_eventPingThreadExited.signal();
		m_eventStatsThreadExited.signal();

		// If PDF support is licensed initialize support
		// NOTE: no exception is thrown or logged if PDF support is not licensed.
		//initPDFSupport();
		// This initializes all licensed leadtool items
		InitLeadToolsLicense();

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
		m_mapWorkflowDefinitions.clear();
		if (m_ipSecureCounters != __nullptr) m_ipSecureCounters->Clear();
		m_ipSecureCounters = __nullptr;
		m_ipFAMTagManager = __nullptr;
		if (m_ipDBInfoSettings != __nullptr) m_ipDBInfoSettings->Clear();
		m_ipDBInfoSettings = __nullptr;

		// Clean up the map of connections
		closeAllDBConnections(false);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI14981");
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::FinalRelease()
{
	try
	{
		// Clean up the map of connections
		closeAllDBConnections(false);
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
STDMETHODIMP CFileProcessingDB::SetStatusForAllFiles(BSTR strAction,  EActionStatus eStatus, long nUserID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check License
		validateLicense();

		if (eStatus == kActionFailed)
		{
			UCLIDException ue("ELI30375", "Transition to Failed state is not allowed.");
			throw ue;
		}

		RetryWithDBLockAndConnection("ELI53384", gstrMAIN_DB_LOCK, [&](_ConnectionPtr ipConnection) -> void
		{
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

					setStatusForAllFiles(ipConnection, strActionName, eStatus, nUserID);
				}
			}
			else
			{
				setStatusForAllFiles(ipConnection, strActionName, eStatus, nUserID);
			}

			// Commit the changes
			tg.CommitTrans();
		});

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
STDMETHODIMP CFileProcessingDB::SetStatusForFileForUser(long nID, BSTR strAction, long nWorkflowID, 
														BSTR strForUser,
														EActionStatus eStatus,
														VARIANT_BOOL vbOverrideProcessing,
														VARIANT_BOOL vbAllowQueuedStatusOverride,
														EActionStatus* poldStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check License
		validateLicense();

		RetryWithDBLockAndConnection("ELI53278", gstrMAIN_DB_LOCK, [&](_ConnectionPtr ipConnection) -> void
		{
			*poldStatus = kActionUnattempted;

			// Ensure file gets added to current workflow if it is missing (setFileActionState)
			nWorkflowID = nWorkflowID == -1 ? getActiveWorkflowID(ipConnection) : nWorkflowID;

			string forUser = asString(strForUser);
			long nForUserID = forUser.empty() ? 0 : getKeyID(ipConnection, gstrFAM_USER, "UserName", forUser, true);

			// Begin a transaction
			TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_criticalSection);

			setStatusForFile(ipConnection, nID, asString(strAction), nWorkflowID, nForUserID, eStatus,
				asCppBool(vbOverrideProcessing), asCppBool(vbAllowQueuedStatusOverride),
				poldStatus);

			tg.CommitTrans();
		});

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI53277");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetFileInformationForFile(int fileID, long long fileSize, int pageCount)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		ASSERT_ARGUMENT("ELI51572", fileSize != -1 || pageCount != -1);
		// Check License
		validateLicense();

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = __nullptr;

		// Get the connection for the thread and save it locally.
		auto role = getAppRoleConnection();
		ipConnection = role->ADOConnection();

		// Begin a transaction
		TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_criticalSection);

		string updateFamFileSQL;

		if (fileSize != -1 && pageCount != -1)
		{
			updateFamFileSQL = " UPDATE "
				" dbo.FAMFile "
				" SET "
				" FileSize = " + asString(fileSize) +
				", Pages = " + asString(pageCount) +
				" WHERE "
				" dbo.FAMFile.ID = " + asString(fileID);
		}
		else if (fileSize != -1)
		{
			updateFamFileSQL = " UPDATE "
				" dbo.FAMFile "
				" SET "
				" FileSize = " + asString(fileSize) +
				" WHERE "
				" dbo.FAMFile.ID = " + asString(fileID);

		}
		else if (pageCount != -1)
		{
			updateFamFileSQL = " UPDATE "
				" dbo.FAMFile "
				" SET "
				" Pages = " + asString(pageCount) +
				" WHERE "
				" dbo.FAMFile.ID = " + asString(fileID);
		}

		executeCmdQuery(ipConnection, updateFamFileSQL);

		tg.CommitTrans();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI51583");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetFilesToProcess(BSTR strAction, long nMaxFiles,
	VARIANT_BOOL bGetSkippedFiles, BSTR bstrSkippedForUserName,
	IIUnknownVector** pvecFileRecords)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check License
		validateLicense();

		EQueueType queueMode = kPendingAnyUserOrNoUser;
		string userName = asString(bstrSkippedForUserName);
		if (bGetSkippedFiles)
		{
			if (userName.empty())
			{
				queueMode = kSkippedAnyUserOrNoUser;
			}
			else
			{
				queueMode = kSkippedSpecifiedUser;
			}
		}

		FilesToProcessRequest request{
			asString(strAction), //.actionName
			queueMode, //.queueMode
			userName, //.userName
			nMaxFiles, //.maxFiles
			false, //.useRandomIDForQueueOrder
		};

		if (!GetFilesToProcess_Internal(false, request, pvecFileRecords))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			GetFilesToProcess_Internal(true, request, pvecFileRecords);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13574");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetFilesToProcessAdvanced(
		BSTR strAction,
		long nMaxFiles,
		EQueueType eQueueMode,
		BSTR bstrUserName,
		VARIANT_BOOL bUseRandomIDForQueueOrder,
		IIUnknownVector** pvecFileRecords)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check License
		validateLicense();

		FilesToProcessRequest request{
			asString(strAction), //.actionName
			eQueueMode, // .queueMode
			asString(bstrUserName), //.userName
			nMaxFiles, //.maxFiles
			asCppBool(bUseRandomIDForQueueOrder), //.useRandomIDForQueueOrder
		};

		if (!GetFilesToProcess_Internal(false, request, pvecFileRecords))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			GetFilesToProcess_Internal(true, request, pvecFileRecords);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI52962");
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
STDMETHODIMP CFileProcessingDB::GetStats(long nActionID, VARIANT_BOOL vbForceUpdate, IActionStatistics* *pStats)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		EWorkflowVisibility eWorkflowVisibility = EWorkflowVisibility::All;
		if (!GetStats_Internal(false, nActionID, vbForceUpdate, VARIANT_FALSE, eWorkflowVisibility, pStats))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			GetStats_Internal(true, nActionID, vbForceUpdate, VARIANT_FALSE, eWorkflowVisibility, pStats);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14045")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetVisibleFileStats(long nActionID, VARIANT_BOOL vbForceUpdate, VARIANT_BOOL vbRevertTimedOutFAMs,
	IActionStatistics* *pStats)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		EWorkflowVisibility eWorkflowVisibility = EWorkflowVisibility::Visible;
		if (!GetStats_Internal(false, nActionID, vbForceUpdate, vbRevertTimedOutFAMs, eWorkflowVisibility, pStats))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			GetStats_Internal(true, nActionID, vbForceUpdate, vbRevertTimedOutFAMs, eWorkflowVisibility, pStats);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI51616")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetInvisibleFileStats(long nActionID, VARIANT_BOOL vbForceUpdate,
	IActionStatistics* *pStats)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		VARIANT_BOOL revertTimedOutFAMs = VARIANT_FALSE;
		EWorkflowVisibility eWorkflowVisibility = EWorkflowVisibility::Invisible;
		if (!GetStats_Internal(false, nActionID, vbForceUpdate, revertTimedOutFAMs, eWorkflowVisibility, pStats))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			GetStats_Internal(true, nActionID, vbForceUpdate, revertTimedOutFAMs, eWorkflowVisibility, pStats);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI51654")
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

		// This is a temporary close (don't reset m_bLoggedInAsAdmin)
		// https://extract.atlassian.net/browse/ISSUE-17800
		closeAllDBConnections(true);

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
		auto role = getAppRoleConnection();
		ipConnection = role->ADOConnection();

		// The mutex only needs to be locked while the data is being obtained
		CSingleLock lock(&m_criticalSection, TRUE);

		// Check License
		validateLicense();

		// Make sure the DB Schema is the expected version
		validateDBSchemaVersion();

		// Begin Transaction
		TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

		// Delete all Lock records
		executeCmd(
			buildCmd(ipConnection, gstrDELETE_DB_LOCK,
				{ { gstrDB_LOCK_NAME_VAL, gstrMAIN_DB_LOCK.c_str() } }));
		
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
STDMETHODIMP CFileProcessingDB::ResetDBConnection(VARIANT_BOOL bResetCredentials,
												  VARIANT_BOOL vbCheckForUnnaffiliatedFiles)
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
		resetDBConnection(asCppBool(vbCheckForUnnaffiliatedFiles));

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
STDMETHODIMP CFileProcessingDB::ChangePassword(BSTR userName, BSTR oldPassword, BSTR newPassword)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		bool LoginValid = isPasswordValid(asString(oldPassword), false);

		if (!LoginValid)
		{
			UCLIDException uex("ELI48433",
				"The provided password was invalid.");
			uex.addDebugInfo("User Name", userName);
			throw uex;
		}

		string pwdReq = getThisAsCOMPtr()->GetDBInfoSetting(gstrPASSWORD_COMPLEXITY_REQUIREMENTS.c_str(), VARIANT_FALSE);
		Util::checkPasswordComplexity(asString(newPassword), pwdReq);
		
		encryptAndStoreUserNamePassword(asString(userName), asString(newPassword), false);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI48434");
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
		
		// Set login valid and cancelled to false
		*pbLoginValid = VARIANT_FALSE;
		*pbLoginCancelled = VARIANT_FALSE;
		bool bUseAdmin = asCppBool(bShowAdmin);

		// Initialize the DB if it is blank
		if (isBlankDB() && !initializeDB(false, ""))
		{
			// If the user chose not to initialize an empty database, treat as a cancelled login.
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
			*pbLoginCancelled = VARIANT_TRUE;
			return S_OK;
		}

		// Get the complexity requirements from the database unless the schema needs updating first
		string strComplexityReq = "";
		bool isSchemaCurrent = false;
		try
		{
			validateDBSchemaVersion();
			isSchemaCurrent = true;
		}
		catch (...) {}

		if (isSchemaCurrent)
		{
			strComplexityReq = getThisAsCOMPtr()->GetDBInfoSetting(gstrPASSWORD_COMPLEXITY_REQUIREMENTS.c_str(), VARIANT_FALSE);
		}

		// If no password set then set a password
		if (strStoredEncryptedCombined == "")
		{
			promptForNewPassword(bShowAdmin, strComplexityReq, pbLoginCancelled, pbLoginValid);
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
		bool bLoginValid = isPasswordValid(string(dlgLogin.m_zPassword), bUseAdmin);

		m_bLoggedInAsAdmin = bLoginValid ? bUseAdmin : m_bLoggedInAsAdmin;

		if (bLoginValid)
		{
			try
			{
				std::string newPassword = dlgLogin.m_zPassword;

				Util::checkPasswordComplexity(newPassword, strComplexityReq);

				*pbLoginValid = asVariantBool(bLoginValid);
			}
			catch (UCLIDException ue)
			{
				ue.display();
				promptForNewPassword(bShowAdmin, strComplexityReq, pbLoginCancelled, pbLoginValid);
			}
		}
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15099");
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::promptForNewPassword(VARIANT_BOOL bShowAdmin, const string& strComplexityRequirements,
	VARIANT_BOOL* pbLoginCancelled, VARIANT_BOOL* pbLoginValid)
{
	bool bUseAdmin = asCppBool(bShowAdmin);
	string strUser = (bUseAdmin) ? gstrADMIN_USER : m_strFAMUserName;
	string strCaption = "Set " + strUser + "'s Password";

	PasswordDlg dlgPW(strCaption, strComplexityRequirements);
	if (dlgPW.DoModal() != IDOK)
	{
		// The user hit cancel on the password dialog. Do NOT update the pw.
		// Set Cancelled flag
		*pbLoginCancelled = VARIANT_TRUE;
		*pbLoginValid = VARIANT_FALSE;
	}
	else
	{
		// Update password in database Login table (fail if not the admin user
		// and the user doesn't exist)
		string strPassword = dlgPW.m_zNewPassword;
		encryptAndStoreUserNamePassword(strUser, strPassword, !bUseAdmin);

		// Consider the user now logged-in as strUser.
		m_strFAMUserName = strUser;
		if (bUseAdmin)
		{
			m_bLoggedInAsAdmin = true;
		}

		// Just added password to the db so it is valid
		*pbLoginValid = VARIANT_TRUE;
	}
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::get_HasCounterCorruption(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI53078", pVal != __nullptr);

		auto role = getAppRoleConnection();
		auto ipConnection = role->ADOConnection();

		bool bIdValid = checkDatabaseIDValid(ipConnection, false, false);
		vector<DBCounter> vecDBCounters;
		bool bCountersValid = checkCountersValid(ipConnection, &vecDBCounters);

		// If there are no counters, but the database ID is invalid, try to repair the ID without prompting
		if (!bIdValid && vecDBCounters.size() == 0)
		{
			try
			{
				try
				{
					// Direct authentication of user is needed to update the database ID (as opposed to using app roles)
					ValueRestorer<AppRole> applicationRoleRestorer(m_currentRole);
					m_currentRole = AppRole::kNoRole;

					auto noAppRole = confirmNoRoleConnection("ELI53114", getAppRoleConnection());

					createAndStoreNewDatabaseID(*noAppRole);
					bIdValid = true;
				}
				CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI53080")
			}
			// Unless there are corrupted counters, an invalid ID is not a showstopper; log and continue
			catch (UCLIDException& ue)
			{
				ue.log();
			}
		}

		*pVal = asVariantBool(vecDBCounters.size() > 0 && (!bIdValid || !bCountersValid));

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI53079");
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
STDMETHODIMP CFileProcessingDB::get_CurrentDBSchemaVersion(LONG* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI49890", pVal != __nullptr);

		*pVal = CFileProcessingDB::ms_lFAMDBSchemaVersion;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI49897");
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
		string strComplexityReq = getThisAsCOMPtr()->GetDBInfoSetting(gstrPASSWORD_COMPLEXITY_REQUIREMENTS.c_str(), VARIANT_FALSE);
		
		// Display Change Password dialog

		ChangePasswordDlg dlgPW(strPasswordDlgCaption, strComplexityReq);
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
		encryptAndStoreUserNamePassword(strUser, strPassword);

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
			CSingleLock lg(&m_criticalSection, TRUE);
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
		CSingleLock lock(&m_criticalSection, TRUE);
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
		CSingleLock lock(&m_criticalSection, TRUE);
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

		ValueRestorer<AppRole> applicationRoleRestorer(m_currentRole);
		m_currentRole = AppRole::kNoRole;

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
		if (isExistingDB() && isBlankDB())
		{
			if (!strInitWithPassword.empty())
			{
				initializeDB(true, strInitWithPassword);
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

				ipDBConnection->Close();
				ipDBConnection->Open(createConnectionString(m_strDatabaseServer, m_strDatabaseName).c_str(),
					"", "", adConnectUnspecified);
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
				CSingleLock lg(&m_criticalSection, TRUE);
				m_mapThreadIDtoDBConnections.erase(dwThreadID);
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI41507");

			throw ex;
		}

		ipDBConnection->Close();

		if (!strInitWithPassword.empty())
		{
			initializeDB(true, strInitWithPassword);
		}
		else
		{
			// Clear the new database to set up the tables
			clear(false, true, false);
		}

		getThisAsCOMPtr()->CloseAllDBConnections();

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

		ValueRestorer<AppRole> applicationRolesRestorer(m_currentRole);

		m_currentRole = AppRole::kNoRole;

		validateServerAndDatabase();

		// Set the database name to the given database name
		m_strDatabaseName = asString(bstrNewDBName);

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
		getThisAsCOMPtr()->CloseAllDBConnections();

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
		CSingleLock lock(&m_criticalSection, TRUE);

		// Set the active settings to the saved static settings0
		m_strDatabaseServer = ms_strCurrServerName;
		m_strDatabaseName  = ms_strCurrDBName;
		m_strAdvConnStrProperties = ms_strCurrAdvConnProperties;
		m_strActiveWorkflow = ms_strLastWorkflow;

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
		auto role = getAppRoleConnection();
		lockDB(role->ADOConnection(), asString(bstrLockName));

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
			auto role = getAppRoleConnection();
			unlockDB(role->ADOConnection(), asString(bstrLockName));
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
STDMETHODIMP CFileProcessingDB::ModifyActionStatusForSelection(
	IFAMFileSelector* pFileSelector, BSTR bstrToAction, EActionStatus eaStatus, 
	VARIANT_BOOL vbModifyWhenTargetActionMissingForSomeFiles, long nUserIdToSet, long* pnNumRecordsModified)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		if (!ModifyActionStatusForSelection_Internal(false, pFileSelector, bstrToAction, eaStatus, 
			vbModifyWhenTargetActionMissingForSomeFiles, nUserIdToSet, pnNumRecordsModified))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
			ModifyActionStatusForSelection_Internal(true, pFileSelector, bstrToAction, eaStatus,
				vbModifyWhenTargetActionMissingForSomeFiles, nUserIdToSet, pnNumRecordsModified);
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
		auto role = getAppRoleConnection();
		getCmdId(
			buildCmd(role->ADOConnection(),
				"INSERT INTO ActiveFAM (FAMSessionID) "
				"	OUTPUT INSERTED.ID "
				"	VALUES (@FAMSessionID)",
				{ { "@FAMSessionID", m_nFAMSessionID } })
			, (long *)&m_nActiveFAMID);

		// set FAM registered flag
		m_bFAMRegistered = true;
		m_dwLastPingTime = 0;

		if (!m_bCurrentSessionIsWebSession)
		{
			m_eventStopMaintenanceThreads.reset();
			m_eventPingThreadExited.reset();
			m_eventStatsThreadExited.reset();

			// Start thread here
			AfxBeginThread(maintainLastPingTimeForRevert, this);
			AfxBeginThread(maintainActionStatistics, this);
		}
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

		ipConnection = getDBConnectionRegardlessOfRole();

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

		IIUnknownVectorPtr ipPairs = ipSettings->GetAllKeyValuePairs();
		ASSERT_RESOURCE_ALLOCATION("ELI31910", ipPairs != __nullptr);

		// Get the key value pairs from the StrToStrMap and create the update queries
		int nSize = ipPairs->Size();
		vector<_CommandPtr> vecCommands;
		vecCommands.reserve(nSize);
		auto role = getAppRoleConnection();
		auto ipConnection = role->ADOConnection();
		int famUserID = getFAMUserID(ipConnection);
		int machineID = getMachineID(ipConnection);

		for(int i=0; i < nSize; i++)
		{
			IStringPairPtr ipPair = ipPairs->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI31911", ipPair != __nullptr);

			_bstr_t bstrKey;
			_bstr_t bstrValue;
			ipPair->GetKeyValuePair(bstrKey.GetAddress(), bstrValue.GetAddress());
			vecCommands.push_back(buildCmd(ipConnection, gstADD_UPDATE_DBINFO_SETTING,
				{
					{gstrSETTING_NAME.c_str(), bstrKey}
					,{gstrSETTING_VALUE.c_str(), bstrValue}
					,{"@UserID", famUserID}
					,{"@MachineID",machineID}
					,{gstrSAVE_HISTORY.c_str(), (m_bStoreDBInfoChangeHistory) ? 1 : 0 }
				}));
		}

		long nNumRowsUpdated = 0;
		if (!SetDBInfoSettings_Internal(false, vecCommands, nNumRowsUpdated))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
			SetDBInfoSettings_Internal(true, vecCommands, nNumRowsUpdated);
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
		auto role = getAppRoleConnection();
		ipConnection = role->ADOConnection();
		
		// Create a pointer to a recordset
		_RecordsetPtr ipActionSet(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI43422", ipActionSet != __nullptr);

		string strQuery = "SELECT * FROM [Action]";

		// Open the Action table
		ipActionSet->Open("Action", _variant_t((IDispatch *)ipConnection, true), adOpenStatic,
			adLockReadOnly, adCmdTable);

		// Step through all records
		while (ipActionSet->adoEOF == VARIANT_FALSE)
		{
			long nActionID = getLongField(ipActionSet->Fields, "ID");
	
			reCalculateStats(ipConnection, nActionID);

			ipActionSet->MoveNext();
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
		CSingleLock lock(&m_criticalSection, TRUE);
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
		ipThis->ActiveWorkflow = ipConnectionSource->ActiveWorkflow;
		ipThis->ConnectionRetryTimeout = ipConnectionSource->ConnectionRetryTimeout;
		ipThis->NumberOfConnectionRetries = ipConnectionSource->NumberOfConnectionRetries;

		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		_ConnectionPtr ipConnection = __nullptr;
		
		BEGIN_CONNECTION_RETRY();
		
		// Get the connection for the thread and save it locally.
		auto role = getAppRoleConnection();
		ipConnection = role->ADOConnection();

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
STDMETHODIMP CFileProcessingDB::GetWorkItemToProcess(BSTR bstrActionName,
								VARIANT_BOOL vbRestrictToFAMSession, IWorkItemRecord **pWorkItem)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		string strActionName = asString(bstrActionName);
		
		if (!GetWorkItemToProcess_Internal(false, strActionName, vbRestrictToFAMSession, pWorkItem))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrWORKITEM_DB_LOCK);
			GetWorkItemToProcess_Internal(true, strActionName, vbRestrictToFAMSession, pWorkItem);
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
STDMETHODIMP CFileProcessingDB::GetFileToProcess(long nFileID, BSTR strAction, BSTR bstrFromStatus,
												 IFileRecord** ppFileRecord)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_RUNTIME_CONDITION("ELI43393", !m_bRunningAllWorkflows,
			"GetFileToProcess is not valid when processing <All workflows>");

		if (!GetFileToProcess_Internal(false, nFileID, strAction, bstrFromStatus, ppFileRecord))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			GetFileToProcess_Internal(true, nFileID, strAction, bstrFromStatus, ppFileRecord);
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
STDMETHODIMP CFileProcessingDB::GetWorkItemsToProcess(BSTR bstrActionName, VARIANT_BOOL vbRestrictToFAMSessionID, 
			long nMaxWorkItemsToReturn, EFilePriority eMinPriority, IIUnknownVector **pWorkItems)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		string strActionName = asString(bstrActionName);

		if (!GetWorkItemsToProcess_Internal(false, strActionName, vbRestrictToFAMSessionID,
			nMaxWorkItemsToReturn, eMinPriority, pWorkItems))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrWORKITEM_DB_LOCK);

			GetWorkItemsToProcess_Internal(true, strActionName, vbRestrictToFAMSessionID,
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

		CSingleLock lock(&m_criticalSection, TRUE);
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

		// Check if the maintain ping thread is still active
		if (m_eventPingThreadExited.isSignaled())
		{
			// if not active set to 0
			m_nActiveFAMID = 0;
		}
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
	long nActionID, long *pnFileTaskSessionID)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI38638", pnFileTaskSessionID != nullptr);

		if (!StartFileTaskSession_Internal(false, bstrTaskClassGuid, nFileID, nActionID, pnFileTaskSessionID))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			StartFileTaskSession_Internal(true, bstrTaskClassGuid, nFileID, nActionID, pnFileTaskSessionID);
		}
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38639");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::EndFileTaskSession(long nFileTaskSessionID,
	double dOverheadTime, double dActivityTime, VARIANT_BOOL vbSessionTimeOut)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		bool bSessionTimeOut = asCppBool(vbSessionTimeOut);

		if (!EndFileTaskSession_Internal(false, 
			nFileTaskSessionID, dOverheadTime, dActivityTime, bSessionTimeOut))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			EndFileTaskSession_Internal(true, 
				nFileTaskSessionID, dOverheadTime, dActivityTime, bSessionTimeOut);
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

		_ConnectionPtr ipConnection = getDBConnectionRegardlessOfRole();
		checkDatabaseIDValid(ipConnection, false, false);
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

		_ConnectionPtr ipConnection = getDBConnectionRegardlessOfRole();
		checkDatabaseIDValid(ipConnection, false, false);
		
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

		_ConnectionPtr ipConnection = getDBConnectionRegardlessOfRole();
		checkDatabaseIDValid(ipConnection, false, false);
		
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
STDMETHODIMP CFileProcessingDB::AddFileNoQueue(BSTR bstrFile, long long llFileSize, long lPageCount,
											   EFilePriority ePriority, long nWorkflowID, long* pnID)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();
		
		if (!AddFileNoQueue_Internal(false, bstrFile, llFileSize, lPageCount, ePriority, nWorkflowID, pnID))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), 
				gstrMAIN_DB_LOCK);

			AddFileNoQueue_Internal(true, bstrFile, llFileSize, lPageCount, ePriority, nWorkflowID, pnID);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI39575");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::AddPaginationHistory(long nOutputFileID,
													 IIUnknownVector* pSourcePageInfo,
													 IIUnknownVector* pDeletedSourcePageInfo,
													 long nFileTaskSessionID)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();
		
		if (!AddPaginationHistory_Internal(false, nOutputFileID, pSourcePageInfo, pDeletedSourcePageInfo, nFileTaskSessionID))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), 
				gstrMAIN_DB_LOCK);

			AddPaginationHistory_Internal(true, nOutputFileID, pSourcePageInfo, pDeletedSourcePageInfo, nFileTaskSessionID);
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
STDMETHODIMP CFileProcessingDB::GetWorkflowActions(long nID, IIUnknownVector** pvecActions)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		if (!GetWorkflowActions_Internal(false, nID, pvecActions))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
				gstrMAIN_DB_LOCK);

			GetWorkflowActions_Internal(true, nID, pvecActions);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41987");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetWorkflowActions(long nID, IIUnknownVector* pActionList)
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

		string strActiveWorkflow = getActiveWorkflow();
		*pbstrWorkflowName = get_bstr_t(strActiveWorkflow).Detach();

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
		setActiveWorkflow(asString(bstrWorkflowName));

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

		EWorkflowVisibility eWorkflowVisibility = EWorkflowVisibility::All;
		if (!GetStatsAllWorkflows_Internal(false, bstrActionName, vbForceUpdate, eWorkflowVisibility, pStats))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
				gstrMAIN_DB_LOCK);

			GetStatsAllWorkflows_Internal(true, bstrActionName, vbForceUpdate, eWorkflowVisibility, pStats);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI42085");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetVisibleFileStatsAllWorkflows(BSTR bstrActionName,
	VARIANT_BOOL vbForceUpdate, IActionStatistics** pStats)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		EWorkflowVisibility eWorkflowVisibility = EWorkflowVisibility::Visible;
		if (!GetStatsAllWorkflows_Internal(false, bstrActionName, vbForceUpdate, eWorkflowVisibility, pStats))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
				gstrMAIN_DB_LOCK);

			GetStatsAllWorkflows_Internal(true, bstrActionName, vbForceUpdate, eWorkflowVisibility, pStats);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI51618");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetInvisibleFileStatsAllWorkflows(BSTR bstrActionName,
	VARIANT_BOOL vbForceUpdate, IActionStatistics** pStats)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		EWorkflowVisibility eWorkflowVisibility = EWorkflowVisibility::Invisible;
		if (!GetStatsAllWorkflows_Internal(false, bstrActionName, vbForceUpdate, eWorkflowVisibility, pStats))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
				gstrMAIN_DB_LOCK);

			GetStatsAllWorkflows_Internal(true, bstrActionName, vbForceUpdate, eWorkflowVisibility, pStats);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI51653");
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI50002");
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
STDMETHODIMP CFileProcessingDB::GetAggregateWorkflowStatus(long *pnUnattempted, long *pnProcessing,
														  long *pnCompleted, long *pnFailed)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		if (!GetAggregateWorkflowStatus_Internal(false, pnUnattempted, pnProcessing, pnCompleted, pnFailed))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
				gstrMAIN_DB_LOCK);

			GetAggregateWorkflowStatus_Internal(true, pnUnattempted, pnProcessing, pnCompleted, pnFailed);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI42153");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetWorkflowStatusAllFiles(BSTR *pbstrStatusListing)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		if (!GetWorkflowStatusAllFiles_Internal(false, pbstrStatusListing))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
				gstrMAIN_DB_LOCK);

			GetWorkflowStatusAllFiles_Internal(true, pbstrStatusListing);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI46408");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::LoginUser(BSTR bstrUserName, BSTR bstrPassword)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	string strPreviousUserName = m_strFAMUserName;

	try
	{
		validateLicense();

		try
		{
			if (asString(bstrUserName) == gstrONE_TIME_ADMIN_USER)
			{
				authenticateOneTimePassword(asString(bstrPassword));
				m_strFAMUserName = gstrADMIN_USER;
			}
			else
			{
				m_strFAMUserName = asString(bstrUserName);

				string strStoredPW;
				bool bUserExists = getEncryptedPWFromDB(strStoredPW, false);

				if (!bUserExists || strStoredPW.empty() || !isPasswordValid(asString(bstrPassword), false))
				{
					UCLIDException ue("ELI42171", "Invalid username or password");
					ue.addDebugInfo("Username", m_strFAMUserName, true);
					throw ue;
				}

				if (m_strFAMUserName == gstrADMIN_USER)
				{
					m_bLoggedInAsAdmin = true;
				}
			}
		}
		catch (...)
		{
			m_strFAMUserName = strPreviousUserName;
			throw;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI42170");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::get_RunningAllWorkflows(VARIANT_BOOL *pRunningAllWorkflows)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI43206", pRunningAllWorkflows != __nullptr);

		*pRunningAllWorkflows = asVariantBool(m_bRunningAllWorkflows);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI43207");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetWorkflowID(BSTR bstrWorkflowName, long *pnID)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());
	
	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI43214", pnID != __nullptr);

		if (!GetWorkflowID_Internal(false, bstrWorkflowName, pnID))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
				gstrMAIN_DB_LOCK);

			GetWorkflowID_Internal(true, bstrWorkflowName, pnID);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI43215");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::IsFileInWorkflow(long nFileID, long nWorkflowID, VARIANT_BOOL *pbIsInWorkflow)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI43216", pbIsInWorkflow != __nullptr);

		if (!IsFileInWorkflow_Internal(false, nFileID, nWorkflowID, pbIsInWorkflow))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
				gstrMAIN_DB_LOCK);

			IsFileInWorkflow_Internal(true, nFileID, nWorkflowID, pbIsInWorkflow);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI43217");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::get_UsingWorkflows(VARIANT_BOOL *pbUsingWorkflows)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI43227", pbUsingWorkflows != __nullptr);

		if (!GetUsingWorkflows_Internal(false, pbUsingWorkflows))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
				gstrMAIN_DB_LOCK);

			GetUsingWorkflows_Internal(true, pbUsingWorkflows);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI43228");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetWorkflowNameFromActionID(long nActionID, BSTR * pbstrWorkflowName)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI43299", pbstrWorkflowName != __nullptr);

		if (!GetWorkflowNameFromActionID_Internal(false, nActionID, pbstrWorkflowName))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
				gstrMAIN_DB_LOCK);

			GetWorkflowNameFromActionID_Internal(true, nActionID, pbstrWorkflowName);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI43300");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetActionIDForWorkflow(BSTR bstrActionName, long nWorkflowID, long* pnActionID)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI43310", pnActionID != __nullptr);

		if (!GetActionIDForWorkflow_Internal(false, bstrActionName, nWorkflowID, pnActionID))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
				gstrMAIN_DB_LOCK);

			GetActionIDForWorkflow_Internal(true, bstrActionName, nWorkflowID, pnActionID);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI43311");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::put_NumberOfConnectionRetries(long nNewVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		m_iNumberOfRetries = nNewVal;
		m_bNumberOfRetriesOverridden = true;
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI43358");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::get_NumberOfConnectionRetries(long * pnVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI43354", pnVal != __nullptr);

		*pnVal = m_iNumberOfRetries;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI43355");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::put_ConnectionRetryTimeout(long nNewVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		m_dRetryTimeout = nNewVal;
		m_bRetryTimeoutOverridden = true;
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI43359");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::get_ConnectionRetryTimeout(long * pnVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI43360", pnVal != __nullptr);

		*pnVal = (long)m_dRetryTimeout;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI43361");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetNewPassword(BSTR bstrUserName, VARIANT_BOOL* pbSuccess)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		*pbSuccess = VARIANT_FALSE;
		
		ASSERT_RUNTIME_CONDITION("ELI43406", m_bLoggedInAsAdmin,
			"SetNewPassword cannot be used if not logged in as admin.");

		string strUser = asString(bstrUserName);
		string strCaption = "Set new password for " + strUser;
		string strComplexityReq = getThisAsCOMPtr()->GetDBInfoSetting(gstrPASSWORD_COMPLEXITY_REQUIREMENTS.c_str(), VARIANT_FALSE);

		// Display Change Password dialog
		PasswordDlg dlgPW(strCaption, strComplexityReq);
		if (dlgPW.DoModal() != IDOK)
		{
			return S_OK;
		}

		// Update password in database Login table (fail if the doesn't exist)
		string strPassword = dlgPW.m_zNewPassword;
		encryptAndStoreUserNamePassword(strUser, strPassword, true);

		*pbSuccess = VARIANT_TRUE;
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI43407");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::MoveFilesToWorkflowFromQuery(BSTR bstrQuery, long nSourceWorkflowID, 
	long nDestWorkflowID, long *pnCount)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		if (!MoveFilesToWorkflowFromQuery_Internal(false, bstrQuery, nSourceWorkflowID, nDestWorkflowID, pnCount))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
				gstrMAIN_DB_LOCK);

			MoveFilesToWorkflowFromQuery_Internal(true, bstrQuery, nSourceWorkflowID, nDestWorkflowID, pnCount);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI43404");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetAttributeValue(BSTR bstrSourceDocName, BSTR bstrAttributeSetName,
												  BSTR bstrAttributePath, BSTR* pbstrValue)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		if (!GetAttributeValue_Internal(false, bstrSourceDocName, bstrAttributeSetName, bstrAttributePath, pbstrValue))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
				gstrMAIN_DB_LOCK);

			GetAttributeValue_Internal(true, bstrSourceDocName, bstrAttributeSetName, bstrAttributePath, pbstrValue);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI43529");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::IsFileNameInWorkflow(BSTR bstrFileName, long nWorkflowID,
													 VARIANT_BOOL *pbIsInWorkflow)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();
		
		ASSERT_ARGUMENT("ELI44845", pbIsInWorkflow != __nullptr);

		if (!IsFileNameInWorkflow_Internal(false, bstrFileName, nWorkflowID, pbIsInWorkflow))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
				gstrMAIN_DB_LOCK);

			IsFileNameInWorkflow_Internal(true, bstrFileName, nWorkflowID, pbIsInWorkflow);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI44846");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SaveWebAppSettings(long nWorkflowID, BSTR bstrType, BSTR bstrSettings)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI45057", bstrType != __nullptr);
		ASSERT_ARGUMENT("ELI45058", bstrSettings != __nullptr);

		if (!SaveWebAppSettings_Internal(false, nWorkflowID, bstrType, bstrSettings))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
				gstrMAIN_DB_LOCK);

			SaveWebAppSettings_Internal(true, nWorkflowID, bstrType, bstrSettings);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI45059");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::LoadWebAppSettings(long nWorkflowID, BSTR bstrType, BSTR *pbstrSettings)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI45067", bstrType != __nullptr);
		ASSERT_ARGUMENT("ELI45068", pbstrSettings != __nullptr);

		if (!LoadWebAppSettings_Internal(false, nWorkflowID, bstrType, pbstrSettings))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
				gstrMAIN_DB_LOCK);

			LoadWebAppSettings_Internal(true, nWorkflowID, bstrType, pbstrSettings);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI45069");
}

//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::DefineNewMLModel(BSTR strModelName, long* pnID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check License
		validateLicense();

		if (!DefineNewMLModel_Internal(false, strModelName, pnID))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			DefineNewMLModel_Internal(true, strModelName, pnID);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI45062");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::DeleteMLModel(BSTR strModelName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check License
		validateLicense();

		if (!DeleteMLModel_Internal(false, strModelName))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			DeleteMLModel_Internal(true, strModelName);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI45063");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetMLModels(IStrToStrMap * * pmapModelNameToID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check License
		validateLicense();

		if (!GetMLModels_Internal(false, pmapModelNameToID))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);
			
			GetMLModels_Internal(true, pmapModelNameToID);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI45124");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::RecordWebSessionStart(BSTR bstrType, VARIANT_BOOL vbForQueuing,
												BSTR bstrLoginId, BSTR bstrIpAddress, BSTR bstrUser)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Always lock the database for this call.
		// Not having a lock here may have had to do with some of the errors reported in
		// [LegacyRCAndUtils:6154]
		// m_strUPI will be used by LockGuard to lock the DB; set session variables before locking the DB.
		m_strFPSFileName = asString(bstrType);
		m_strUPI = asString(bstrLoginId);
		m_strMachineName = asString(bstrIpAddress);
		m_strFAMUserName = asString(bstrUser);
		
		LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

		RecordWebSessionStart_Internal(true, vbForQueuing);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI45220");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::ResumeWebSession(long nFAMSessionID, long* pnFileTaskSessionID, long* pnOpenFileID, VARIANT_BOOL* pbIsFileOpen)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_nFAMSessionID = nFAMSessionID;
		string strFAMSessionID = asString(nFAMSessionID);
		m_bCurrentSessionIsWebSession = true;

		string strSessionQuery =
			"SELECT [MachineName], [UPI], [FPSFileName], [UserName]"
			" FROM dbo.[FAMSession]"
			" JOIN dbo.[FAMUser] ON [FAMUserID] = [FAMUser].[ID]"
			" JOIN dbo.[FPSFile] ON [FPSFileID] = [FPSFile].[ID]"
			" JOIN dbo.[Machine] ON [MachineID] = [Machine].[ID]"
			" WHERE [StopTime] IS NULL"
			" AND [FAMSession].[ID] = " + strFAMSessionID;

		_RecordsetPtr ipRecords = getThisAsCOMPtr()->GetResultsForQuery(strSessionQuery.c_str());
		ASSERT_RUNTIME_CONDITION("ELI46659", ipRecords->adoEOF == VARIANT_FALSE,
			"Can't resume session that isn't open");

		FieldsPtr ipFields = ipRecords->Fields;
		ASSERT_RESOURCE_ALLOCATION("ELI46660", ipFields != nullptr);

		m_strMachineName = getStringField(ipFields, "MachineName");
		m_strUPI = getStringField(ipFields, "UPI");
		m_strFPSFileName = getStringField(ipFields, "FPSFileName");
		m_strFAMUserName = getStringField(ipFields, "UserName");

		string strActiveFAMQuery = "SELECT [ActiveFAM].[ID] FROM dbo.[ActiveFAM] WHERE [FAMSessionID] = " + asString(nFAMSessionID);

		// Set active FAM, if there was one for this session
		m_bFAMRegistered = false;
		m_nActiveFAMID = 0;
		ipRecords = getThisAsCOMPtr()->GetResultsForQuery(strActiveFAMQuery.c_str());
		if (ipRecords->adoEOF == VARIANT_FALSE)
		{
			FieldsPtr ipFields = ipRecords->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI46658", ipFields != nullptr);

			long nActiveFAMID = getLongField(ipFields, "ID");
			if (nActiveFAMID > 0)
			{
				m_nActiveFAMID = nActiveFAMID;
				m_bFAMRegistered = true;
			}
		}

		// Get open FileID if there is one
		string strGetOpenFileFromFAMSession =
			"SELECT TOP(1) [FileTaskSession].[ID] AS [FileTaskSessionID], [FileTaskSession].[FileID]"
			"	FROM [dbo].[FileTaskSession]"
			"	JOIN [dbo].[LockedFile] ON [ActiveFAMID] = " + asString(m_nActiveFAMID) +
			"		AND [FileTaskSession].[FileID] = [LockedFile].[FileID]"
			"		AND [FileTaskSession].[ActionID] = [LockedFile].[ActionID]"
			"	WHERE [FAMSessionID] = " + strFAMSessionID +
			"	ORDER BY [FileTaskSession].[ID] DESC";

		ipRecords = getThisAsCOMPtr()->GetResultsForQuery(strGetOpenFileFromFAMSession.c_str());
		if (ipRecords->adoEOF == VARIANT_FALSE)
		{
			FieldsPtr ipFields = ipRecords->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI46663", ipFields != nullptr);

			*pnFileTaskSessionID = getLongField(ipFields, "FileTaskSessionID");
			*pnOpenFileID = getLongField(ipFields, "FileID");
			*pbIsFileOpen = VARIANT_TRUE;
		}
		else
		{
			*pbIsFileOpen = VARIANT_FALSE;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI46657");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SuspendWebSession()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Reset members set by starting/resuming a web and/or FAM session back to their defaults
		setDefaultSessionMemberValues();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI46696");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetActiveUsers(BSTR bstrAction, IVariantVector** ppvecUserNames)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		if (!GetActiveUsers_Internal(false, bstrAction, ppvecUserNames))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			GetActiveUsers_Internal(true, bstrAction, ppvecUserNames);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI45523");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::AbortFAMSession(long nFAMSessionID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		if (!AbortFAMSession_Internal(false, nFAMSessionID))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			AbortFAMSession_Internal(true, nFAMSessionID);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI46252");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::MarkFileDeleted(long nFileID, long nWorkflowID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		if (!MarkFileDeleted_Internal(false, nFileID, nWorkflowID))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrMAIN_DB_LOCK);

			MarkFileDeleted_Internal(true, nFileID, nWorkflowID);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI46298");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::IsFAMSessionOpen(long nFAMSessionID, VARIANT_BOOL* pbIsFAMSessionOpen)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_nFAMSessionID = nFAMSessionID;
		string strFAMSessionID = asString(nFAMSessionID);

		string strSessionQuery =
			"SELECT 1"
			" FROM dbo.[FAMSession]"
			" WHERE [StopTime] IS NULL"
			" AND [FAMSession].[ID] = " + strFAMSessionID;

		_RecordsetPtr ipRecords = getThisAsCOMPtr()->GetResultsForQuery(strSessionQuery.c_str());

		*pbIsFAMSessionOpen = asVariantBool(!ipRecords->adoEOF);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI46724");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetNumberSkippedForUser(BSTR bstrUserName, long nActionID, VARIANT_BOOL bRevertTimedOutFAMs, long* pnFilesSkipped)
{
	try
	{
		string strQuery =
			"SELECT COUNT(ID) as NumberSkippedForUser "
			"FROM SkippedFile "
			"WHERE UserName = '<UserName>' AND ActionID = " + asString(nActionID);
		
		string strUser = asString(bstrUserName);

		replaceVariable(strQuery, "<UserName>", strUser);
		long lNumberSkipped;
		auto role = getAppRoleConnection();
		executeCmdQuery(role->ADOConnection(), strQuery, "NumberSkippedForUser", false, &lNumberSkipped);

		*pnFilesSkipped = lNumberSkipped;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI46754");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::CacheFileTaskSessionData(long nFileTaskSessionID, long nPage,
	SAFEARRAY *parrayImageData, BSTR bstrUssData, BSTR bstrWordZoneData, BSTR bstrAttributeData, BSTR bstrException,
	VARIANT_BOOL vbCrucialUpdate, VARIANT_BOOL* pbWroteData)
{
	try
	{
		validateLicense();

		if (!CacheFileTaskSessionData_Internal(false, nFileTaskSessionID, nPage,
				parrayImageData, bstrUssData, bstrWordZoneData, bstrAttributeData, bstrException, vbCrucialUpdate, pbWroteData)
			&& asCppBool(vbCrucialUpdate))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrCACHE_LOCK);

			CacheFileTaskSessionData_Internal(true, nFileTaskSessionID, nPage,
				parrayImageData, bstrUssData, bstrWordZoneData, bstrAttributeData, bstrException, vbCrucialUpdate, pbWroteData);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI48311");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetCachedFileTaskSessionData(long nFileTaskSessionID, long nPage,
	ECacheDataType eDataType, VARIANT_BOOL vbCrucialData,
	SAFEARRAY** pparrayImageData, BSTR* pbstrUssData, BSTR* pbstrWordZoneData, BSTR* pbstrAttributeData,
	BSTR* pbstrException, VARIANT_BOOL* pbFoundCacheData)
{
	try
	{
		validateLicense();

		if (!GetCachedFileTaskSessionData_Internal(false, nFileTaskSessionID, nPage, eDataType, vbCrucialData,
			pparrayImageData, pbstrUssData, pbstrWordZoneData, pbstrAttributeData, pbstrException,
			pbFoundCacheData)
			&& asCppBool(vbCrucialData))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrCACHE_LOCK);

			GetCachedFileTaskSessionData_Internal(true, nFileTaskSessionID, nPage, eDataType, vbCrucialData,
				pparrayImageData, pbstrUssData, pbstrWordZoneData, pbstrAttributeData, pbstrException,
				pbFoundCacheData);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI48422");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetCachedPageNumbers(long nFileTaskSessionID, ECacheDataType eCacheDataType,
	SAFEARRAY** pparrayCachedPages)
{
	try
	{
		// NOTE: There are not retries with locks here since caching should not be critical to application
		// behavior and retries with locking may erase performance gains from caching; if existing
		// cache data is not found, at worst the data will be re-cached.

		validateLicense();

		ASSERT_ARGUMENT("ELI49486", pparrayCachedPages != __nullptr);
		ASSERT_ARGUMENT("ELI49487", eCacheDataType != 0);

		ADODB::_ConnectionPtr ipConnection = __nullptr;

		// Get the connection for the thread and save it locally.
		auto role = getAppRoleConnection();
		ipConnection = role->ADOConnection();

		vector<string> vecFieldRestrictions;
		if (eCacheDataType & ECacheDataType::kImage)
		{
			vecFieldRestrictions.push_back("ImageData");
		}
		if (eCacheDataType & ECacheDataType::kUss)
		{
			vecFieldRestrictions.push_back("USSData");
		}
		if (eCacheDataType & ECacheDataType::kWordZone)
		{
			vecFieldRestrictions.push_back("WordZoneData");
		}
		if (eCacheDataType & ECacheDataType::kAttributes)
		{
			vecFieldRestrictions.push_back("AttributeData");
		}
		if (eCacheDataType & ECacheDataType::kException)
		{
			vecFieldRestrictions.push_back("Exception");
		}

		// Retrieve pages all pages as a single comma-delimited scaler value to avoid having to iterate all cache rows with a cursor.
		string strQuery =
			"SELECT\r\n"
			"(\r\n"
			"	SELECT(CAST([Page] AS NVARCHAR) + ',')\r\n"
			"		FROM [FileTaskSessionCache] WITH (NOLOCK) \r\n"
			"		WHERE [FileTaskSessionID] = <FileTaskSessionID>\r\n"
			"		AND <TargetFields>\r\n"
			"		FOR XML PATH('')\r\n"
			"	) AS [Pages]";
		string strFieldClause = "["
			+ asString(vecFieldRestrictions, false, "] IS NOT NULL\r\n		AND [")
			+ "] IS NOT NULL";
		replaceVariable(strQuery, "<FileTaskSessionID>", asString(nFileTaskSessionID));
		replaceVariable(strQuery, "<TargetFields>", strFieldClause);

		_RecordsetPtr ipCachedPages(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI49496", ipCachedPages != __nullptr);

		ipCachedPages->Open(strQuery.c_str(),
			_variant_t((IDispatch*)ipConnection, true), adOpenStatic,
			adLockReadOnly, adCmdText);

		// Parse out the string representation of the pages into a vector
		vector<string> vecPages;
		if (!isNULL(ipCachedPages->Fields, "Pages"))
		{
			string strPageList = getStringField(ipCachedPages->Fields, "Pages");
			strPageList = trim(strPageList, "", ",");
			StringTokenizer::sGetTokens(strPageList, ',', vecPages);
		}

		// Convert to an integer and store in a map in order to sort pages (which otherwise may appear
		// in the the FileTaskSessionCache table out of order).
		map<long, void*> mapPages;
		for each (string strPage in vecPages)
		{
			mapPages[asLong(strPage)] = __nullptr;
		}

		CComSafeArray<long> saCachedPages((ULONG)0);
		for (auto iter = mapPages.begin(); iter != mapPages.end(); iter++)
		{
			saCachedPages.Add(iter->first);
		}

		*pparrayCachedPages = (LPSAFEARRAY)saCachedPages.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI49495");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::CacheAttributeData(long nFileTaskSessionID, 
								IStrToStrMap* pmapAttributeData, VARIANT_BOOL bOverwriteModifiedData)
{
	try
	{
		validateLicense();

		if (!CacheAttributeData_Internal(false, nFileTaskSessionID, pmapAttributeData, bOverwriteModifiedData))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrCACHE_LOCK);

			CacheAttributeData_Internal(true, nFileTaskSessionID, pmapAttributeData, bOverwriteModifiedData);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI49472");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::MarkAttributeDataUnmodified(long nFileTaskSessionID)
{
	try
	{
		validateLicense();

		if (!MarkAttributeDataUnmodified_Internal(false, nFileTaskSessionID))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrCACHE_LOCK);

			MarkAttributeDataUnmodified_Internal(true, nFileTaskSessionID);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI49514");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetUncommittedAttributeData(long nFileID, long nActionID,
	BSTR bstrExceptIfMoreRecentAttributeSetName, IIUnknownVector** ppUncommittedPagesOfData)
{
	try
	{
		validateLicense();

		if (!GetUncommittedAttributeData_Internal(false, nFileID, nActionID,
				bstrExceptIfMoreRecentAttributeSetName, ppUncommittedPagesOfData))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrCACHE_LOCK);

			GetUncommittedAttributeData_Internal(true, nFileID, nActionID,
				bstrExceptIfMoreRecentAttributeSetName, ppUncommittedPagesOfData);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI49522");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::DiscardOldCacheData(long nFileID, long nActionID, long nExceptFileTaskSessionID)
{
	try
	{
		validateLicense();

		if (!DiscardOldCacheData_Internal(false, nFileID, nActionID, nExceptFileTaskSessionID))
		{
			// Lock the database for this instance
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(), gstrCACHE_LOCK);

			DiscardOldCacheData_Internal(true, nFileID, nActionID, nExceptFileTaskSessionID);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI49517");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetOneTimePassword(BSTR* pVal)
{
	try
	{
		validateLicense();

		// One-time admin password can only be generated if currently authenticated as admin.
		// there should not be an exising FAM session.
		ASSERT_RUNTIME_CONDITION("ELI49836", m_bLoggedInAsAdmin && m_nFAMSessionID == 0,
			"Not authorized to generate password");

		try
		{
			// Leverage a FAMSession as a component of one-time passwords in order to limit the
			// password this database, prevent it from being used multiple times and to ensure it
			// was created within the past minute.
			getThisAsCOMPtr()->RecordFAMSessionStart(
				gstrONE_TIME_ADMIN_USER.c_str(), "", VARIANT_FALSE, VARIANT_FALSE);

			auto role = getAppRoleConnection();
			string strPassword = getOneTimePassword(role->ADOConnection());
			
			// Once the password is generated, the session ID in use for this instance should be reset.
			m_nFAMSessionID = 0;

			*pVal = get_bstr_t(strPassword).Detach();
		}
		catch (...)
		{
			m_nFAMSessionID = 0;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI49832");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetFamUsers(IStrToStrMap** pmapFamUserNameToID)
{
	try
	{
		RetryWithDBLockAndConnection("ELI53290", gstrMAIN_DB_LOCK, [&](_ConnectionPtr ipConnection) -> void
		{
			IStrToStrMapPtr ipmapUsers(CLSID_StrToStrMap);
			ASSERT_RESOURCE_ALLOCATION("ELI53293", ipmapUsers != __nullptr);

			// Create a pointer to a recordset
			_RecordsetPtr ipFAMUserSet(__uuidof(Recordset));
			ASSERT_RESOURCE_ALLOCATION("ELI53291", ipFAMUserSet != __nullptr);

			ipFAMUserSet->Open("SELECT ID, UserName FROM FAMUser", _variant_t((IDispatch*)ipConnection, true),
				adOpenForwardOnly, adLockReadOnly, adCmdText);

			while (ipFAMUserSet->adoEOF == VARIANT_FALSE)
			{
				FieldsPtr ipFields = ipFAMUserSet->Fields;
				ASSERT_RESOURCE_ALLOCATION("ELI53292", ipFields != __nullptr);

				string strUserName = getStringField(ipFields, "UserName");
				string strID =  asString(getLongField(ipFields, "ID"));

				ipmapUsers->Set(strUserName.c_str(), strID.c_str());
				ipFAMUserSet->MoveNext();
			}

			*pmapFamUserNameToID = ipmapUsers.Detach();
		});

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI53287");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetExternalLogin(BSTR bstrDescription, BSTR* pbstrUserName, BSTR* pbstrPassword)
{
	try
	{
		validateLicense();

		ASSERT_RUNTIME_CONDITION("ELI53307", m_bLoggedInAsAdmin, "Not authorized to get login information");

		RetryWithDBLockAndConnection("ELI53308", gstrMAIN_DB_LOCK, [&](_ConnectionPtr ipConnection) -> void
		{
			string userName, password;
			getExternalLogin(ipConnection, bstrDescription, userName, password);

			*pbstrUserName = get_bstr_t(userName.data()).Detach();
			*pbstrPassword = get_bstr_t(password.data()).Detach();
		});

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI53309");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::SetExternalLogin(BSTR bstrDescription, BSTR bstrUserName, BSTR bstrPassword)
{
	try
	{
		validateLicense();

		ASSERT_RUNTIME_CONDITION("ELI53310", m_bLoggedInAsAdmin, "Not authorized to set login information");

		RetryWithDBLockAndConnection("ELI53311", gstrMAIN_DB_LOCK, [&](_ConnectionPtr ipConnection) -> void
		{
			string encryptedPassword = getEncryptedString(1, asString(bstrPassword));
			string query =
				"UPDATE [dbo].[ExternalLogin] SET UserName = @UserName, Password = @Password WHERE Description = @Description "
				"IF @@ROWCOUNT = 0 "
				"INSERT INTO [dbo].[ExternalLogin] (Description, UserName, Password) VALUES (@Description, @UserName, @Password)";

			executeCmd(buildCmd(ipConnection, query,
				{
					{"@Description", get_bstr_t(bstrDescription)},
					{"@UserName", get_bstr_t(bstrUserName)},
					{"@Password", encryptedPassword.data()}
				}));
		});

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI53312");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingDB::GetAzureAccessToken(BSTR bstrExternalLoginDescription, BSTR* pbstrAccessToken)
{
	try
	{
		string userName, password;

		RetryWithDBLockAndConnection("ELI53313", gstrMAIN_DB_LOCK, [&](_ConnectionPtr ipConnection) -> void
		{
			getExternalLogin(ipConnection, bstrExternalLoginDescription, userName, password);
		});

		UCLID_FILEPROCESSINGLib::IAuthenticationProviderPtr authenticationProvider;
		SECURE_CREATE_OBJECT("ELI53314", authenticationProvider, "Extract.Utilities.AuthenticationProvider");

		string accessToken = authenticationProvider->
			GetAccessToken(getThisAsCOMPtr(), get_bstr_t(userName.data()), get_bstr_t(password.data()));

		*pbstrAccessToken = get_bstr_t(accessToken.data()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI53315");
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
