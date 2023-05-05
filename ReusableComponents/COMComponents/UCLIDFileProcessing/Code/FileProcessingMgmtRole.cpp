// FileProcessingMgmtRole.cpp : Implementation of CFileProcessingMgmtRole

#include "stdafx.h"
#include "UCLIDFileProcessing.h"
#include "FileProcessingMgmtRole.h"
#include "FP_UI_Notifications.h"
#include "FileProcessingUtils.h"
#include "CommonConstants.h"
#include "HelperFunctions.h"
#include "FPWorkItem.h"
#include "ProcessingContext.h"

#include <LicenseMgmt.h>
#include <COMUtils.h>
#include <cpputil.h>
#include <ComponentLicenseIDs.h>
#include <ThreadSafeLogFile.h>
#include <ValueRestorer.h>
#include <FAMUtilsConstants.h>
#include <FAMHelperFunctions.h>
#include <Win32Semaphore.h>
#include <UPI.h>
#include <ADOUtils.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 7;

// Strings associated with logging errors to a text file
const string gstrERROR_SUMMARY_HEADER = "****Error Summary****";
const string gstrERROR_SUMMARY_FILENAME = "Filename: ";
const string gstrERROR_SUMMARY_TIMESTAMP = "Timestamp: ";
const string gstrERROR_SUMMARY_MACHINENAME = "Machine: ";
const string gstrERROR_SUMMARY_PROCESSID = "ProcessID: ";
const string gstrERROR_SUMMARY_THREADID = "ThreadID: ";
const string gstrERROR_SUMMARY_ERRORTEXT = "Error: ";

const string gstrTASK_INFORMATION_HEADER = "****Task Information****";
const string gstrTASK_INFORMATION_NUMBER = "Current task number: ";
const string gstrTASK_INFORMATION_DESCRIPTION = "Current task description: ";

const string gstrEXCEPTION_DETAILS_HEADER = "****Exception Details****";

//-------------------------------------------------------------------------------------------------
// StandbyThread
//-------------------------------------------------------------------------------------------------
StandbyThread::StandbyThread(Win32Event& eventCancelProcessing,
	const UCLID_FILEPROCESSINGLib::IFileProcessingTaskExecutorPtr& ipTaskExecutor)
: m_eventCancelProcessing(eventCancelProcessing)
, m_ipTaskExecutor(ipTaskExecutor)
{
	try
	{
		ASSERT_RESOURCE_ALLOCATION("ELI33940", m_ipTaskExecutor != __nullptr);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33941");
}
//-------------------------------------------------------------------------------------------------
StandbyThread::~StandbyThread()
{
	try
	{
		m_ipTaskExecutor = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI33934")
}
//-------------------------------------------------------------------------------------------------
BOOL StandbyThread::InitInstance()
{
	// Return TRUE so Run is called.
	return TRUE;
}
//-------------------------------------------------------------------------------------------------
int StandbyThread::Run()
{
	try
	{
		// The Standby call may block if there is a cancellable task configured.
		if (!asCppBool(m_ipTaskExecutor->Standby()) && !m_eventStandbyEnded.isSignaled())
		{
			// If the return value of Standby is false, and the Standby state has not yet ended,
			// cancel processing.
			m_eventCancelProcessing.signal();
		}

		// This thread needs to remain alive until Standby has ended so that endStandby may be
		// called.
		m_eventStandbyEnded.wait();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI33935")

	return 0;
}
//-------------------------------------------------------------------------------------------------
void StandbyThread::endStandby()
{
	try
	{
		// Notify the task executor that standby has ended so that it will stop waiting on replies
		// from any Standby calls that are blocking.
		m_ipTaskExecutor->EndStandby();

		m_eventStandbyEnded.signal();

		// Do not attempt the same code here that was added to
		// CFileProcessingTaskExecutor::StandbyThread::endStandby() to address LegacyRCAndUtils:6211.
		// The race condition is much less likely to occur in this class (the mgmt role would need
		// to be destroyed in the time before m_eventCancelProcessing.signal is called) and doing
		// so here would cause a deadlock. In the extremely unlikely case that the race condition
		// occurs here, the only negative side-effect is that an exception will be logged (ELI33935).
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI33936")
}

//-------------------------------------------------------------------------------------------------
// ProcessingThreadData
//-------------------------------------------------------------------------------------------------
ProcessingThreadData::ProcessingThreadData()
:	m_pThread(__nullptr), m_pFPMgmtRole(__nullptr)
{
	try
	{
		m_ipTaskExecutor.CreateInstance(CLSID_FileProcessingTaskExecutor);
		ASSERT_RESOURCE_ALLOCATION("ELI17845", m_ipTaskExecutor != __nullptr);

		m_ipErrorTaskExecutor.CreateInstance(CLSID_FileProcessingTaskExecutor);
		ASSERT_RESOURCE_ALLOCATION("ELI18006", m_ipErrorTaskExecutor != __nullptr);
	}
	catch (...)
	{
		throw uex::fromCurrent("ELI17942");
	};
}
//-------------------------------------------------------------------------------------------------
ProcessingThreadData::~ProcessingThreadData()
{
	try
	{
		// Release the file processing tasks
		m_ipTaskExecutor = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI12958");
}

//-------------------------------------------------------------------------------------------------
// WorkUnitThreadData
//-------------------------------------------------------------------------------------------------
Win32Event WorkItemThreadData::ms_threadStopProcessing;

WorkItemThreadData::WorkItemThreadData(CFileProcessingMgmtRole* pFPMgmtRole, long nActionID, 
	Win32Semaphore &rSemaphore, IFileProcessingDB *pDB) :
m_pFPMgmtRole(pFPMgmtRole),
m_nActionID(nActionID),
m_rSemaphore(rSemaphore),
m_pDB(pDB)
{
}
//-------------------------------------------------------------------------------------------------
WorkItemThreadData::~WorkItemThreadData()
{
}

//-------------------------------------------------------------------------------------------------
// CFileProcessingMgmtRole
//-------------------------------------------------------------------------------------------------
CFileProcessingMgmtRole::CFileProcessingMgmtRole()
	: m_pRecordMgr(NULL),
	m_ipRoleNotifyFAM(NULL),
	m_threadDataSemaphore(2, 2),
	m_bProcessing(false),
	m_bHasProcessingCompleted(false),
	m_bProcessingSingleFile(false),
	m_ipProcessingSingleFileRecord(__nullptr),
	m_upProcessingSingleFileTask(__nullptr),
	m_upParallelSemaphore(__nullptr),
	m_eQueueMode(kPendingAnyUserOrNoUser)
{
	try
	{
		// clear internal data
		clear();
	}
	catch (...)
	{
		throw uex::fromCurrent("ELI14299");
	}
}
//-------------------------------------------------------------------------------------------------
CFileProcessingMgmtRole::~CFileProcessingMgmtRole()
{
	try
	{
		// clear internal data
		clear();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI14300")
}
//-------------------------------------------------------------------------------------------------
HRESULT CFileProcessingMgmtRole::FinalConstruct()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingMgmtRole::FinalRelease()
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IFileActionMgmtRole,
		&IID_ILicensedComponent,
		&IID_IFileProcessingMgmtRole,
		&IID_IAccessRequired,
		&IID_IFileRequestHandler,
		&IID_IPersistStream
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (pbValue == NULL)
	{
		return E_POINTER;
	}

	try
	{
		// validate license
		validateLicense();

		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IFileActionMgmtRole interface implementation
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::Start(IFileProcessingDB* pDB, long lActionId, 
	BSTR bstrAction, long hWndOfUI, IFAMTagManager* pTagManager, IRoleNotifyFAM* pRoleNotifyFAM,
	BSTR bstrFpsFileName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// check pre-conditions
		ASSERT_ARGUMENT("ELI14296", m_bEnabled == true);
		ASSERT_ARGUMENT("ELI14301", m_ipFileProcessingTasks != __nullptr);
		ASSERT_ARGUMENT("ELI14302", m_ipFileProcessingTasks->Size() > 0);
		ASSERT_ARGUMENT("ELI14344", m_pRecordMgr != __nullptr);

		m_bHasProcessingCompleted = false;

		UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipDB(pDB);

		// Before starting the processManager thread, make sure it is not already started
		if (m_eventProcessManagerStarted.isSignaled() && !m_eventProcessManagerExited.isSignaled())
		{
			UCLIDException ue("ELI28320", "Manager thread is currently running.");
			throw ue;
		}

		// Set the File Action manager pointer
		m_ipRoleNotifyFAM = pRoleNotifyFAM;
		ASSERT_RESOURCE_ALLOCATION("ELI14531", m_ipRoleNotifyFAM != __nullptr );

		// store the pointer to the DB so that subsequent calls to getFPDB() will work correctly
		m_pDB = pDB;

		// store the pointer to the TagManager so that subsequent calls to getFPMTagManager() will work
		m_pFAMTagManager = pTagManager;

		// remember the handle of the UI so that messages can be sent to it
		m_hWndOfUI = (HWND) hWndOfUI;

		// remember the action name
		m_strAction = asString(bstrAction);

		// Reset all of the events required managing the processing
		m_eventManualStopProcessing.reset();
		m_eventProcessManagerExited.reset();
		m_eventProcessManagerStarted.reset();
		m_eventProcessManagerActive.reset();
		m_eventWatcherThreadExited.reset();
		m_eventPause.reset();
		m_eventResume.reset();

		// Initialize the current running state to normal stop.
		m_eCurrentRunningState = kNormalStop;

		// start the processing thread
		AfxBeginThread(processManager, this);	

		// Wait for up to a second for the processing to actually start
		// This is so that the method does not return until processing
		// has started and thus if a user calls Pause the processing will
		// be able to pause - [LRCAU #5835]
		int nCount = 0;
		while (!m_bProcessing && nCount < 10)
		{
			Sleep(100);
			nCount++;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09825");
	
	return S_OK;  
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::Stop(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		
		// Signal to the manager thread that processing should stop
		m_eventManualStopProcessing.signal();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14308");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::Pause(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// Reset the resume event
		m_eventResume.reset();

		// Cause processing to wait until the m_eventResume is signaled
		m_eventPause.signal();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14322");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::Resume(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// Reset the pause event
		m_eventPause.reset();

		// Cause processing to resume
		m_eventResume.signal();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14327");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::get_Enabled(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		*pVal = (m_bEnabled ? VARIANT_TRUE : VARIANT_FALSE);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14328")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::put_Enabled(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		m_bEnabled = (newVal == VARIANT_TRUE);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14329")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::Clear(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// call the internal method
		clear();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14309")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::ValidateStatus(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_bEnabled)
		{
			// If "Process files..." checkbox is checked, at least one file processor must be defined
			if (!m_ipFileProcessingTasks || (m_ipFileProcessingTasks && m_ipFileProcessingTasks->Size() == 0))
			{
				UCLIDException ue("ELI14361", "At least one task should be specified!");
				throw ue;
			}

			// At least one file processor must be enabled
			bool bEnabled = false;
			long nCountFileProcessingTasks = m_ipFileProcessingTasks->Size();
			for (int i = 0; i < nCountFileProcessingTasks; i++)
			{
				// Retrieve this Object With Description
				IObjectWithDescriptionPtr ipOWD = m_ipFileProcessingTasks->At( i );
				ASSERT_RESOURCE_ALLOCATION("ELI16007", ipOWD != __nullptr);

				// Check the Enabled flag
				if (asCppBool( ipOWD->Enabled ))
				{
					// This task is enabled
					bEnabled = true;
					break;
				}
			}

			if (!bEnabled)
			{
				UCLIDException ue("ELI16006", "At least one task must be enabled!");
				throw ue;
			}

			// Check for defined error log
			if (m_bLogErrorDetails)
			{
				if (m_strErrorLogFile.length() == 0)
				{
					UCLIDException ue("ELI16090", "An error log must be specified!");
					throw ue;
				}
				
				// Ensure log file name ends in uex
				string strExt = getExtensionFromFullPath(m_strErrorLogFile, true);
				if (strExt != ".uex")
				{
					UCLIDException ue("ELI18005", "Error log must end with extension \".uex\"!");
					throw ue;
				}
			}

			// Check for defined error email task
			if (m_bSendErrorEmail)
			{
				IErrorEmailTaskPtr ipErrorEmailTask = getErrorEmailTask();
				if (ipErrorEmailTask == __nullptr)
				{
					UCLIDException ue("ELI36166", "Error validating error email settings!");
					throw ue;
				}

				ipErrorEmailTask->ValidateErrorEmailConfiguration();
			}

			// Check for defined error task
			if (asCppBool(getErrorHandlingTask()->Enabled))
			{
				UCLID_FILEPROCESSINGLib::IFileProcessingTaskPtr ipErrorTask(getErrorHandlingTask()->Object);
				if (ipErrorTask == __nullptr)
				{
					UCLIDException ue("ELI18059", "An error task must be specified!");
					throw ue;
				}
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14350")
}

//-------------------------------------------------------------------------------------------------
// IAccessRequired interface implementation
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::raw_RequiresAdminAccess(VARIANT_BOOL* pbResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI31246", pbResult != __nullptr);

		// Admin access is required if 
		// 1. File processing is enabled [LRCAU #5478]
		// 2. Skipped files are being processed
		// 3. Processing skipped files for any user
		// 4. DBInfo setting requires password to process skipped files for any user
		// 5. Any of the tasks being processed requires admin access
		// 5. The error task requires admin access

		bool bResult = m_bEnabled && (m_eQueueMode == kSkippedAnyUserOrNoUser &&
			(asString(getFPMDB()->GetDBInfoSetting(
				gstrREQUIRE_PASSWORD_TO_PROCESS_SKIPPED.c_str(), VARIANT_TRUE)) == "1")
			|| checkForRequiresAdminAccess(m_ipFileProcessingTasks)  
			|| checkForRequiresAdminAccess(getErrorHandlingTask()));
		
		*pbResult = asVariantBool(bResult);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31247");
}

//-------------------------------------------------------------------------------------------------
// IFileRequestHandler
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::CheckoutNextFile(VARIANT_BOOL vbAllowQueuedStatusOverride,
													   long* pnFileID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI40153", pnFileID != __nullptr);

		EActionStatus prevStatus;
		// No process currently has the field; request the FPRecordManger to lock it for
		// processing and add it to the internal queue.
		*pnFileID = -1;
		m_pRecordMgr->checkoutForProcessing(*pnFileID,
			asCppBool(vbAllowQueuedStatusOverride), &prevStatus);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI40152");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::GetNextCheckedOutFile(long nAfterFileID, long* pnFileID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI40308", pnFileID != __nullptr);

		*pnFileID = m_pRecordMgr->peekNext(nAfterFileID);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI40309");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::CheckoutForProcessing(long nFileID,
	VARIANT_BOOL vbAllowQueuedStatusOverride, EActionStatus* pPrevStatus, VARIANT_BOOL* pSucceeded)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI37470", pSucceeded != __nullptr);

		bool bResult = false;

		_RecordsetPtr ipRecords(nullptr);

		if (nFileID != gnGET_NEXT_QUEUED_FILE)
		{
			// Query for an existing record in the lock table.
			string strLockedFileQuery =
			"SELECT [ActiveFAMID] FROM [LockedFile] WHERE [FileID] = " + asString(nFileID);

			ipRecords = getFPMDB()->GetResultsForQuery(strLockedFileQuery.c_str());
			ASSERT_RESOURCE_ALLOCATION("ELI37471", ipRecords != __nullptr);
		}

		if (ipRecords != nullptr && ipRecords->adoEOF == VARIANT_FALSE)
		{
			// The file is already locked. But is it locked by this process?
			ipRecords->MoveFirst();

			FieldsPtr ipFields = ipRecords->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI37472", ipFields != __nullptr);

			if (getFPMDB()->ActiveFAMID == getLongField(ipFields, "ActiveFAMID"))
			{
				// Already locked (processing) by this process.
				*pPrevStatus = kActionProcessing;
				bResult = true;
			}
			else
			{
				// Another process already has the file.
				bResult = false;
			}
		}
		else
		{
			// No process currently has the file; request the FPRecordManger to lock it for
			// processing and add it to the internal queue.
			bResult = m_pRecordMgr->checkoutForProcessing(nFileID,
				asCppBool(vbAllowQueuedStatusOverride), pPrevStatus);
		}
		
		*pSucceeded = asVariantBool(bResult);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37473");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::MoveToFrontOfProcessingQueue(long nFileID, VARIANT_BOOL* pSucceeded)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI37474", pSucceeded != __nullptr);

		bool bResult = m_pRecordMgr->moveToFrontOfQueue(nFileID);
		
		*pSucceeded = asVariantBool(bResult);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37475");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::ReleaseFile(long nFileID, VARIANT_BOOL* pSucceeded)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI37476", pSucceeded != __nullptr);

		bool bResult = m_pRecordMgr->remove(nFileID);
		
		*pSucceeded = asVariantBool(bResult);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37477");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::SetFallbackStatus(long nFileID, 
	EActionStatus eaFallbackStatus, VARIANT_BOOL* pSucceeded)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI37478", pSucceeded != __nullptr);

		bool bResult = false;

		FileProcessingRecord task;
		if (m_pRecordMgr->getTask(nFileID, task))
		{
			getFPMDB()->SetFallbackStatus(task.getFileRecord(),
				(UCLID_FILEPROCESSINGLib::EActionStatus)eaFallbackStatus);

			bResult = true;
		}
		
		*pSucceeded = asVariantBool(bResult);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37479");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::PauseProcessingQueue()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_eventProcessManagerActive.reset();
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37536");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::ResumeProcessingQueue()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_eventProcessManagerActive.signal();
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37537");
}

//-------------------------------------------------------------------------------------------------
// IFileProcessingMgmtRole interface implementation
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::get_FileProcessors(IIUnknownVector ** pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_ipFileProcessingTasks == __nullptr)
		{
			m_ipFileProcessingTasks.CreateInstance(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI08936", m_ipFileProcessingTasks != __nullptr);
		}

		IIUnknownVectorPtr ipShallowCopy = m_ipFileProcessingTasks;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08830")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::put_FileProcessors(IIUnknownVector * newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_ipFileProcessingTasks = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08831")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::get_NumThreads(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		*pVal = m_nNumThreads;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14330")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::put_NumThreads(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_nNumThreads = newVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14331")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::SetDirty(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		m_bDirty = (newVal == VARIANT_TRUE);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14332")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::SetRecordMgr(void *pRecordMgr)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		// verify args
		ASSERT_ARGUMENT("ELI14286", pRecordMgr != __nullptr);

		// update internal ptr
		m_pRecordMgr = static_cast<FPRecordManager *> (pRecordMgr);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14287")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::get_KeepProcessingAsAdded(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		// verify args
		ASSERT_ARGUMENT("ELI14518", pVal != __nullptr );
		
		*pVal = (m_bKeepProcessingAsAdded) ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14516")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::put_KeepProcessingAsAdded(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		m_bKeepProcessingAsAdded = newVal == VARIANT_TRUE;

		// Set Dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14517")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::put_OkToStopWhenQueueIsEmpty(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		m_bOkToStopWhenQueueIsEmpty = newVal == VARIANT_TRUE;

		if (m_pRecordMgr != __nullptr )
		{
			if ( m_bOkToStopWhenQueueIsEmpty )
			{
				m_pRecordMgr->setKeepProcessingAsAdded(m_bKeepProcessingAsAdded);
			}
			else
			{
				m_pRecordMgr->setKeepProcessingAsAdded(true);
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14520")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::get_OkToStopWhenQueueIsEmpty(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		// verify args
		ASSERT_ARGUMENT("ELI14521", pVal != __nullptr );
		
		*pVal = (m_bOkToStopWhenQueueIsEmpty) ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14522")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::get_LogErrorDetails(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license and verify argument
		validateLicense();
		ASSERT_ARGUMENT("ELI16060", pVal != __nullptr );

		*pVal = asVariantBool( m_bLogErrorDetails );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16061")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::put_LogErrorDetails(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bLogErrorDetails = asCppBool( newVal );

		// Set Dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16062")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::get_ErrorLogName(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = get_bstr_t( m_strErrorLogFile.c_str() ).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16069");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::put_ErrorLogName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Convert parameter to STL string
		string strErrorLogName = asString(newVal);

		m_strErrorLogFile = strErrorLogName;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16071");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::get_ExecuteErrorTask(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license and verify argument
		validateLicense();
		ASSERT_ARGUMENT("ELI16063", pVal != __nullptr );

		*pVal = asVariantBool( isErrorHandlingTaskEnabled() );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16064")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::put_ExecuteErrorTask(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Retrieve existing value
		VARIANT_BOOL vbExisting = getErrorHandlingTask()->Enabled;

		// Update the setting and set Dirty flag if changed
		if (newVal != vbExisting)
		{
			getErrorHandlingTask()->Enabled = newVal;
			m_bDirty = true;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16065")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::get_ErrorTask(IObjectWithDescription **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// Provide shallow copy to caller
		IObjectWithDescriptionPtr ipShallowCopy = getErrorHandlingTask();
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16067")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::put_ErrorTask(IObjectWithDescription *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_ipErrorTask = newVal;

		// Set dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16068")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::get_QueueMode(EQueueType* pVal)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI53295", pVal != __nullptr);

		*pVal = m_eQueueMode;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI53296");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::put_QueueMode(EQueueType newVal)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		if (m_eQueueMode != newVal)
		{
			m_bDirty = true;
			m_eQueueMode = newVal;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI53297");
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_FileProcessingMgmtRole;
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	try
	{
		// if the directly held data is dirty, then indicate to the caller that
		// this object is dirty
		if (m_bDirty)
		{
			return S_OK;
		}

		// check if the file processors vector object is dirty
		if (m_ipFileProcessingTasks != __nullptr)
		{
			IPersistStreamPtr ipFPStream = m_ipFileProcessingTasks;
			ASSERT_RESOURCE_ALLOCATION("ELI14333", ipFPStream != __nullptr);
			if (ipFPStream->IsDirty() == S_OK)
			{
				return S_OK;
			}
		}

		// check if the error task is dirty
		if (m_ipErrorTask != __nullptr)
		{
			IPersistStreamPtr ipFPStream = m_ipErrorTask;
			ASSERT_RESOURCE_ALLOCATION("ELI18064", ipFPStream != __nullptr);
			if (ipFPStream->IsDirty() == S_OK)
			{
				return S_OK;
			}
		}

		// check if the error task is dirty
		if (m_ipErrorEmailTask != __nullptr)
		{
			IPersistStreamPtr ipFPStream = m_ipErrorEmailTask;
			ASSERT_RESOURCE_ALLOCATION("ELI36133", ipFPStream != __nullptr);
			if (ipFPStream->IsDirty() == S_OK)
			{
				return S_OK;
			}
		}

		// if we reached here, it means that the object is not dirty
		// indicate to the caller that this object is not dirty
		return S_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI30415");
}
//-------------------------------------------------------------------------------------------------
// Version 3:
//   Added persistence for error logging items and error handling items
// Version 4:
//	 Added persistence for skipped file processing
// Version 5:
//	 Added persistence for the processing schedule
// Version 6:
//	 Added persistence for error condition emails
// Version 7:
//	 Added ability to process files queued for a specific user (m_bLimitToUserQueue, m_bIncludeFilesQueuedForOthers)
STDMETHODIMP CFileProcessingMgmtRole::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Reset all the member variables
		clear();

		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read(&nDataLength, sizeof(nDataLength), NULL);
		ByteStream data(nDataLength);
		pStream->Read(data.getData(), nDataLength, NULL);
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// Check for newer version
		if (nDataVersion > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue( "ELI14453", "Unable to load newer File Processing Management Role." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		// Read Enabled status
		dataReader >> m_bEnabled;

		// Number of threads for processing
		dataReader >> m_nNumThreads;

		// Continuous processing
		if (nDataVersion >= 2)
		{
			dataReader >> m_bKeepProcessingAsAdded;
		}

		// Logging of error details
		if (nDataVersion >= 3)
		{
			dataReader >> m_bLogErrorDetails;

			if (m_bLogErrorDetails)
			{
				dataReader >> m_strErrorLogFile;
			}
		}

		// Skipped file processing data
		bool bProcessSkippedFiles = false;
		bool bSkippedForAnyUser = false;
		if (nDataVersion >= 4)
		{
			// Read in the process skipped files data
			dataReader >> bProcessSkippedFiles;

			// Read in skipped for any user value
			dataReader >> bSkippedForAnyUser;
		}

		// Load Schedule data
		if (nDataVersion >= 5)
		{
			dataReader >> m_bLimitProcessingToSchedule;
			if (m_bLimitProcessingToSchedule)
			{
				// Read the schedule from the stream
				long nSize;
				dataReader >> nSize;
				for (int i = 0; i < nSize; i++)
				{
					bool bTemp;
					dataReader >> bTemp;
					m_vecScheduledHours[i] = bTemp;
				}
			}
		}

		// Load error handling email task
		if (nDataVersion >= 6)
		{
			dataReader >> m_bSendErrorEmail;
			
			bool bLoadErrorEmailTask = true;
			dataReader >> bLoadErrorEmailTask;

			// Read in the task object only if it was saved.
			if (bLoadErrorEmailTask)
			{
				IPersistStreamPtr ipErrorEmailTaskObj;
				readObjectFromStream(ipErrorEmailTaskObj, pStream, "ELI36134");
				m_ipErrorEmailTask = ipErrorEmailTaskObj;
			}
		}

		bool bLimitToUserQueue = false;
		bool bIncludeFilesQueuedForOthers = false;
		if (nDataVersion >= 7)
		{
			dataReader >> bLimitToUserQueue;
			dataReader >> bIncludeFilesQueuedForOthers;
		}

		// Convert booleans into EQueueType enum
		if (bProcessSkippedFiles)
		{
			if (bSkippedForAnyUser)
			{
				m_eQueueMode = kSkippedAnyUserOrNoUser;
			}
			else
			{
				m_eQueueMode = kSkippedSpecifiedUser;
			}
		}
		else
		{
			if (bIncludeFilesQueuedForOthers)
			{
				m_eQueueMode = kPendingAnyUserOrNoUser;
			}
			else if (bLimitToUserQueue)
			{
				m_eQueueMode = kPendingSpecifiedUser;
			}
			else
			{
				m_eQueueMode = kPendingSpecifiedUserOrNoUser;
			}
		}

		// Error handling task
		if (nDataVersion >= 3)
		{
			// Read in the Error Task
			IPersistStreamPtr ipErrorTaskObj;
			readObjectFromStream( ipErrorTaskObj, pStream, "ELI16052" );
			m_ipErrorTask = ipErrorTaskObj;

			// Between the time of introduction of code to store the error task
			// and the completion of implementation, it was possible to store the
			// ObjectWithDescription as enabled, yet not be associated with a 
			// FileProcessing task object. ValidateStatus code that now prevents 
			// such a configuration causes annoyances when loading fps files created 
			// during this time period due to the validation that occurs upon loading 
			// the fps. Assume on load that if there is no object assigned, it should 
			// not be enabled.
			if (asCppBool(m_ipErrorTask->Enabled) && m_ipErrorTask->Object == NULL)
			{
				m_ipErrorTask->Enabled = VARIANT_FALSE;
			}
		}

		// Read in the collected File Processors
		IPersistStreamPtr ipFPObj;
		readObjectFromStream( ipFPObj, pStream, "ELI14454" );
		m_ipFileProcessingTasks = ipFPObj;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14334");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter( ByteStreamManipulator::kWrite, data );
		dataWriter << gnCurrentVersion;

		// Save the enabled flag
		dataWriter << m_bEnabled;

		// Save the number of threads
		dataWriter << m_nNumThreads;

		// Save the CompleteOnNoRcdFromDB flag
		dataWriter << m_bKeepProcessingAsAdded;

		// Save error logging items
		dataWriter << m_bLogErrorDetails;
		if (m_bLogErrorDetails)
		{
			dataWriter << m_strErrorLogFile;
		}

		// Write the processing scope (pending files or skipped files)
		bool bProcessSkippedFiles = m_eQueueMode & kSkippedFlag;
		bool bSkippedForAnyUser = m_eQueueMode & kAnyUserFlag;
		dataWriter << bProcessSkippedFiles;
		dataWriter << bSkippedForAnyUser;

		// Write the processing schedule info
		dataWriter << m_bLimitProcessingToSchedule;
		if (m_bLimitProcessingToSchedule)
		{
			// Write the schedule to the stream
			long nSize = m_vecScheduledHours.size();
			dataWriter << nSize;
			for (int i = 0; i < nSize; i++)
			{
				dataWriter << m_vecScheduledHours[i];
			}
		}

		dataWriter << m_bSendErrorEmail;
		bool bSaveErrorEmailTask = (m_ipErrorEmailTask != __nullptr);
		dataWriter << bSaveErrorEmailTask;

		bool bLimitToUserQueue = m_eQueueMode == kPendingSpecifiedUser;
		bool bIncludeFilesQueuedForOthers = m_eQueueMode & kAnyUserFlag;
		dataWriter << bLimitToUserQueue;
		dataWriter << bIncludeFilesQueuedForOthers;

		// Write these items to the byte stream
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), NULL );
		pStream->Write( data.getData(), nDataLength, NULL );

		// Save the error email task if it exists. (Don't create an email task until needed.)
		if (bSaveErrorEmailTask)
		{
			IPersistStreamPtr ipErrorEmailTaskObj = getErrorEmailTask();
			ASSERT_RESOURCE_ALLOCATION("ELI36135", ipErrorEmailTaskObj != __nullptr);
			writeObjectToStream(ipErrorEmailTaskObj, pStream, "ELI36136", fClearDirty);
		}

		// Save the error handling task
		IPersistStreamPtr ipErrorTaskObj = getErrorHandlingTask();
		ASSERT_RESOURCE_ALLOCATION( "ELI16055", ipErrorTaskObj != __nullptr );
		writeObjectToStream( ipErrorTaskObj, pStream, "ELI16056", fClearDirty );

		// Make sure the FileProcessingTasks vector has been created
		if ( m_ipFileProcessingTasks == __nullptr )
		{
			m_ipFileProcessingTasks.CreateInstance(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI14586", m_ipFileProcessingTasks != __nullptr );
		}
		
		// Save the File Processors
		IPersistStreamPtr ipFPObj = m_ipFileProcessingTasks;
		ASSERT_RESOURCE_ALLOCATION( "ELI14455", ipFPObj != __nullptr );
		writeObjectToStream( ipFPObj, pStream, "ELI14456", fClearDirty );

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14335");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::get_ProcessingSchedule(IVariantVector** ppHoursSchedule)
{	
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI28157", ppHoursSchedule != __nullptr);

		// Create the return variant vector
		IVariantVectorPtr ipHours(CLSID_VariantVector);
		ASSERT_ARGUMENT("ELI28159", ipHours != __nullptr);

		// Copy data from the STL vector to the variant vector
		int nSize = m_vecScheduledHours.size();
		for ( int i = 0; i < nSize; i++)
		{
			variant_t v(m_vecScheduledHours[i]);
			ipHours->PushBack(v);
		}

		// Return the hours
		*ppHoursSchedule = ipHours.Detach();
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28158");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::put_ProcessingSchedule(IVariantVector* pHoursSchedule)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		IVariantVectorPtr ipHoursSchedule(pHoursSchedule);
		ASSERT_ARGUMENT("ELI28160", ipHoursSchedule != __nullptr);

		// Clear the scheduled hours vector
		m_vecScheduledHours.clear();
		m_vecScheduledHours.reserve(giNUMBER_OF_HOURS_IN_WEEK);

		// Copy values for the schedule
		int nSize = ipHoursSchedule->Size;
		for ( int i = 0; i < nSize; i++ )
		{
			variant_t v(ipHoursSchedule->Item[i]);
			if (v.vt == VT_BOOL)
			{
				m_vecScheduledHours.push_back(asCppBool(v.boolVal));
			}
			else
			{
				UCLIDException ue("ELI28184", "Unexpected variant type.");
				ue.addDebugInfo("VariantType", v.vt);
				throw ue;
			}
		}

		// Set dirty flag
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28601");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::get_LimitProcessingToSchedule(VARIANT_BOOL *pbVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI28177", pbVal != __nullptr);

		*pbVal = asVariantBool(m_bLimitProcessingToSchedule);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28175");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::put_LimitProcessingToSchedule(VARIANT_BOOL bVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Don't set the dirty flag if the state does not change
		bool bNewValue = asCppBool(bVal);
		if ( m_bLimitProcessingToSchedule == bNewValue)
		{
			return S_OK;
		}

		// Set the new value
		m_bLimitProcessingToSchedule = bNewValue;

		// If the schedule is turned on, initialize the vector of scheduled hours
		if (!m_bLimitProcessingToSchedule)
		{
			m_vecScheduledHours.clear();
			m_vecScheduledHours.resize(giNUMBER_OF_HOURS_IN_WEEK, true);
		}

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28176");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::ProcessSingleFile(IFileRecord* pFileRecord,
										IFileProcessingDB* pFPDB, IFAMTagManager* pFAMTagManager)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		if (m_bProcessing || m_bProcessingSingleFile)
		{
			throw UCLIDException("ELI29554", 
				"Cannot process single file when processing is already in progress.");
		}

		// Ensure m_bProcessingSingleFile will be reset to false.
		ValueRestorer<volatile bool> restorer(m_bProcessingSingleFile, false);
		m_bProcessingSingleFile = true;

		m_ipProcessingSingleFileRecord = pFileRecord;
		ASSERT_ARGUMENT("ELI29536", m_ipProcessingSingleFileRecord != __nullptr);

		m_pDB = pFPDB;
		ASSERT_ARGUMENT("ELI29552", m_pDB != __nullptr);

		m_pFAMTagManager = pFAMTagManager;
		ASSERT_ARGUMENT("ELI29553", m_pFAMTagManager != __nullptr);

		// Get the action id
		long lActionID = m_ipProcessingSingleFileRecord->ActionID;

		// Get the current action status-- allow an attempt to auto-revert locked files if the file
		// is in the processing state.
		string strActionName = getFPMDB()->GetActionName(lActionID);
		UCLID_FILEPROCESSINGLib::EActionStatus easCurrent = getFPMDB()->GetFileStatus(
			m_ipProcessingSingleFileRecord->FileID, strActionName.c_str(), VARIANT_TRUE);

		// If file is not in the correct state to process (depending on the skipped file
		// setting), throw an exception.
		bool bProcessSkippedFiles = m_eQueueMode & kSkippedFlag;
		while ((bProcessSkippedFiles && easCurrent != UCLID_FILEPROCESSINGLib::kActionSkipped) ||
			   (!bProcessSkippedFiles && easCurrent != UCLID_FILEPROCESSINGLib::kActionPending))
		{
			UCLIDException ue("ELI29545", string("The file cannot be processed because it ") +
				"is not currently " + (bProcessSkippedFiles ? "skipped!" : "pending!"));
			ue.addDebugInfo("Current Status", asString(getFPMDB()->AsStatusString(easCurrent)));
			throw ue;
		}

		// Set action ID to the record manager
		m_pRecordMgr->setActionID(m_ipProcessingSingleFileRecord->ActionID);

		// Clear the record manager to ensure no files except the one specified will be processed.
		m_pRecordMgr->clear(false);

		// Use the configured skipped file settings.
		m_pRecordMgr->setQueueMode(m_eQueueMode);

		// Do not keep processing regardless of the configured setting.
		m_pRecordMgr->setKeepProcessingAsAdded(false);

		// Create and initialize a processing thread data struct needed by processTask.
		ProcessingThreadData threadData;
		threadData.m_pFPMgmtRole = this;

		m_upProcessingSingleFileTask.reset(new FileProcessingRecord(m_ipProcessingSingleFileRecord));
		ASSERT_RESOURCE_ALLOCATION("ELI34999", m_upProcessingSingleFileTask.get());

		unsigned long ulStackSize = getProcessingThreadStackSize();
		
		// a value of 0 means use one thread per processor
		long nNumThreads = m_nNumThreads;
		if (nNumThreads == 0)
		{
			nNumThreads = getNumLogicalProcessors();
		}
		createProcessingSemaphore(nNumThreads);

		// Since this is for single file all threads should only process workItems generated by this instance.
		m_pRecordMgr->setRestrictToFAMSessionID(true);

		// Setup the work item threads if any tasks are set to parallelize
		bool bParallelize = setupWorkItemThreadData(nNumThreads, lActionID);
		if (bParallelize)
		{
			// Start the work item threads
			startWorkItemThreads(ulStackSize);
		}

		threadData.m_pThread = AfxBeginThread(processSingleFileThread, &threadData, 0, ulStackSize);
		ASSERT_RESOURCE_ALLOCATION("ELI35000", threadData.m_pThread != __nullptr);

		// wait for the thread to end
		threadData.m_threadEndedEvent.wait();

		if (bParallelize)
		{
			// processing is done so signal the work item threads to stop and wait until
			// they exit
			signalWorkItemThreadsToStopAndWait();

			// clean up the thread data
			releaseWorkItemThreadDataObjects();
		}

		// Retrieve any exception thrown and caught in the thread.
		string strException = m_upProcessingSingleFileTask->m_strException;

		m_upProcessingSingleFileTask.reset(__nullptr);
		m_ipProcessingSingleFileRecord = __nullptr;
		m_bProcessingSingleFile = false;

		// Exceptions that occurred while processing a file in processTask will not be thrown out.
		// Throw the exception here if necessary.
		if (!strException.empty())
		{
			UCLIDException ue;
			ue.createFromString("ELI29557", strException);
			throw ue;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29533");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::get_FPDB(IFileProcessingDB** ppFPDB)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI34343", ppFPDB != __nullptr);

		// Increment the ref count because the object will be managed by a smart pointer
		// that assumes this has been done already
		// https://extract.atlassian.net/browse/ISSUE-19155
		m_pDB->AddRef();

		*ppFPDB = m_pDB;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI34344");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::put_FPDB(IFileProcessingDB* pFPDB)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_pDB = pFPDB;

		// Don't set the dirty flag here; A database needs to be assigned for processing, but it is
		// not part of the configuration.

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI34345");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::get_SendErrorEmail(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		// Check license and verify argument
		validateLicense();
		ASSERT_ARGUMENT("ELI36137", pVal != __nullptr );

		*pVal = asVariantBool(m_bSendErrorEmail);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36138")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::put_SendErrorEmail(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		bool bSendErrorEmail = asCppBool(newVal);

		// Update the setting and set dirty flag if changed
		if (bSendErrorEmail != m_bSendErrorEmail)
		{
			m_bSendErrorEmail = bSendErrorEmail;
			
			// Ensure the error email task is created when the user enables the option.
			if (m_bSendErrorEmail)
			{
				getErrorEmailTask();
			}

			m_bDirty = true;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36139")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::get_ErrorEmailTask(IErrorEmailTask **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_ipErrorEmailTask == __nullptr)
		{
			// Don't create an email task until needed.
			*pVal = __nullptr;
		}
		else
		{
			// Provide shallow copy to caller
			IErrorEmailTaskPtr ipShallowCopy = getErrorEmailTask();
			*pVal = ipShallowCopy.Detach();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36140")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::put_ErrorEmailTask(IErrorEmailTask *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_ipErrorEmailTask = newVal;

		// Set dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36141")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::get_HasProcessingCompleted(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI42134", pVal != __nullptr)

		// Don't create an email task until needed.
		*pVal = asVariantBool(m_bHasProcessingCompleted);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI42133")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::get_ProcessingDisplaysUI(VARIANT_BOOL * pProcessingDisplaysUI)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI44995", pProcessingDisplaysUI != __nullptr);

		bool bDisplaysUI = false;

		unsigned long ulMinStackSize = 0;
		int nTaskCount = m_ipFileProcessingTasks->Size();
		for (int i = 0; (i < nTaskCount) && !bDisplaysUI; i++)
		{
			// Get the current object as ObjectWithDescription
			IObjectWithDescriptionPtr ipOWD = m_ipFileProcessingTasks->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI44997", ipOWD != __nullptr);

			UCLID_FILEPROCESSINGLib::IFileProcessingTaskPtr ipFileProcessor(ipOWD->Object);
			ASSERT_RESOURCE_ALLOCATION("ELI44998", ipFileProcessor != __nullptr);

			if (asCppBool(ipOWD->Enabled))
			{
				bDisplaysUI = asCppBool(ipFileProcessor->DisplaysUI);
			}
		}

		*pProcessingDisplaysUI = asVariantBool(bDisplaysUI);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI44996")
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileActionMgmtRolePtr CFileProcessingMgmtRole::getThisAsCOMPtr()
{
	UCLID_FILEPROCESSINGLib::IFileActionMgmtRolePtr ipThis(this);
	ASSERT_RESOURCE_ALLOCATION("ELI16976", ipThis != __nullptr);

	return ipThis;
}
//-------------------------------------------------------------------------------------------------
long CFileProcessingMgmtRole::getEnabledFileProcessingTasksCount(IIUnknownVectorPtr ipFileProcessingTasks) const
{
	long nCount = 0;

	// Iterate through the IIUnknownVector of IObjectWithDescription of IFileProcessingTask objects
	// and determine the count of enabled file processors
	if (ipFileProcessingTasks != __nullptr)
	{
		for (int i = 0; i < ipFileProcessingTasks->Size(); i++)
		{
			// Get the file processor at the current index
			IObjectWithDescriptionPtr ipObjWithDesc = ipFileProcessingTasks->At(i);

			// Increment the count if the object is defined and is enabled for use
			if (ipObjWithDesc->Enabled == VARIANT_TRUE && ipObjWithDesc->Object != __nullptr)
			{
				nCount++;
			}
		}
	}

	return nCount;
}
//-------------------------------------------------------------------------------------------------
UINT CFileProcessingMgmtRole::fileProcessingThreadProc(void *pData)
{
	// cast argument into thread data structure pointer
	ProcessingThreadData *pThreadData = static_cast<ProcessingThreadData *>(pData);
	ASSERT_ARGUMENT("ELI11107", pThreadData != __nullptr);

	// notify interested parties that the thread has started
	pThreadData->m_threadStartedEvent.signal();

	try
	{
		CoInitializeEx(NULL, COINIT_MULTITHREADED);
		CFileProcessingMgmtRole *pFPMgmtRole = pThreadData->m_pFPMgmtRole;
		ASSERT_RESOURCE_ALLOCATION("ELI11108", pFPMgmtRole != __nullptr);
		pFPMgmtRole->processFiles2(pThreadData);
		CoUninitialize();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI11073")

	// notify interested parties that the thread has ended
	pThreadData->m_threadEndedEvent.signal();

	return 0;
}
//-------------------------------------------------------------------------------------------------
UINT CFileProcessingMgmtRole::workItemProcessingThreadProc(void *pData)
{
	// cast argument into Work item thread data structure pointer
	WorkItemThreadData *pThreadData = static_cast<WorkItemThreadData *>(pData);
	ASSERT_ARGUMENT("ELI36866", pThreadData != __nullptr);

	// Signal that the thread has started
	pThreadData->m_threadStartedEvent.signal();

	try
	{
		CoInitializeEx(NULL, COINIT_MULTITHREADED);

		// Setup the database pointer
		UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipDB(pThreadData->m_pDB);

		// Setup pointer to the file processing management role
		CFileProcessingMgmtRole *pFPMgmtRole = pThreadData->m_pFPMgmtRole;
		ASSERT_RESOURCE_ALLOCATION("ELI37138", pFPMgmtRole != __nullptr);

		FPRecordManager* fprm = pFPMgmtRole->m_pRecordMgr;
		ASSERT_RESOURCE_ALLOCATION("ELI37374", fprm != __nullptr);
		
		UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipFAMTag(pFPMgmtRole->m_pFAMTagManager);

		long nWorkItemGroupID = -1;
		long nWorkItemID = -1;
		long nActionID = pThreadData->m_nActionID;
		UCLID_FILEPROCESSINGLib::IParallelizableTaskPtr ipTask = __nullptr;

		IMiscUtilsPtr ipMiscUtils(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI37036", ipMiscUtils != __nullptr);
		bool bDone = false;
		DWORD dwWaitTime = gnMIN_ALLOWED_SLEEP_TIME_BETWEEN_DB_CHECK;
		FPWorkItem workItem;

		do
		{
			try
			{
				try
				{
					nWorkItemID = -1;

					pFPMgmtRole->m_eventProcessManagerActive.wait();

					// Get next work item to process
					bool bWorkItemReturned = fprm->getWorkItemToProcess(workItem, pThreadData->ms_threadStopProcessing);
					
					if (!bWorkItemReturned)
					{
						// no work items to process
						break;
					}
					UCLID_FILEPROCESSINGLib::IWorkItemRecordPtr ipWorkItem = workItem.getWorkItemRecord();

					if (bWorkItemReturned && ipWorkItem != __nullptr)
					{
						// if the work item group is the same as the last one don't have to 
						// recreate the task just use the one from the last
						if (nWorkItemGroupID != ipWorkItem->WorkItemGroupID)
						{
							nWorkItemGroupID = ipWorkItem->WorkItemGroupID;
							ipTask = __nullptr;
							
							long nNumberOfWorkItems;
							_bstr_t bstrStringizedTask = ipDB->GetWorkGroupData(nWorkItemGroupID, &nNumberOfWorkItems);
							
							ipTask = ipMiscUtils->GetObjectFromStringizedByteStream(bstrStringizedTask);
							ASSERT_RESOURCE_ALLOCATION("ELI36868", ipTask != __nullptr);
						}
						nWorkItemID = ipWorkItem->WorkItemID;
						Win32SemaphoreLockGuard lg(pThreadData->m_rSemaphore);
						workItem.markAsStarted();
						fprm->updateWorkItem(workItem);

						_bstr_t bstrCurrentWorkflow("");
						if (ipDB != __nullptr)
						{
							bstrCurrentWorkflow = ipDB->GetWorkflowNameFromActionID(nActionID);
						}
						// Replace tag manager with new instance that has current workflow
						ipFAMTag = ipFAMTag->GetFAMTagManagerWithWorkflow(bstrCurrentWorkflow);

						ipTask->ProcessWorkItem(ipWorkItem, nActionID, ipFAMTag, ipDB, workItem.m_ipProgressStatus);
						ipDB->NotifyWorkItemCompleted(nWorkItemID);
						workItem.markAsCompleted();
						fprm->updateWorkItem(workItem);
					}
				}
				CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36918");
			}
			catch(UCLIDException &ue)
			{
				if (nWorkItemID > 0)
				{
					string stringizedException = ue.asStringizedByteStream();
					ipDB->NotifyWorkItemFailed(nWorkItemID,stringizedException.c_str());
					workItem.markAsFailed(stringizedException);
					fprm->updateWorkItem(workItem);
				}
				else
				{
					ue.log();
				}
			}
		}
		while (!pThreadData->ms_threadStopProcessing.isSignaled());
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI36867");
	
	CoUninitialize();
	
	pThreadData->m_threadEndedEvent.signal();

	return 0;
}
//-------------------------------------------------------------------------------------------------
UINT CFileProcessingMgmtRole::fileProcessingThreadsWatcherThread(void *pData)
{
	CFileProcessingMgmtRole* pFPM;
	try
	{
		pFPM = static_cast<CFileProcessingMgmtRole*>(pData);
		ASSERT_ARGUMENT("ELI13893", pFPM != __nullptr);
		ASSERT_ARGUMENT("ELI14348", pFPM->m_pRecordMgr != __nullptr);
		ASSERT_ARGUMENT("ELI14532", pFPM->m_ipRoleNotifyFAM != __nullptr );

		try
		{
			// This thread needs to be blocked until all thread data has been created
			// so it needs to wait until there is a semaphore available
			CSingleLock guard( &pFPM->m_threadDataSemaphore, TRUE );

			CoInitializeEx(NULL, COINIT_MULTITHREADED);
			try
			{
				try
				{
					// wait for each of the threads to complete their work
					vector<ProcessingThreadData *>& rvecProcessingThreadData = pFPM->m_vecProcessingThreadData;
					unsigned long ulNumThreads = rvecProcessingThreadData.size();
					for (unsigned long i = 0; i < ulNumThreads; i++)
					{
						// Before waiting for the thread to complete need to make sure it started
						if ( !rvecProcessingThreadData[i]->m_threadStartedEvent.isSignaled() )
						{
							// thread was not started 
							UCLIDException ue("ELI16215", "Processing thread was not started!");
							ue.addDebugInfo("Thread #", i);
							// Log the exceptions and continue for remaining threads
							ue.log();
						}
						else
						{
							// Wait for processing to finish for that thread
							rvecProcessingThreadData[i]->m_threadEndedEvent.wait();
						}
					}

					// Inform file processing task executor that processing has ended
					for (unsigned long i = 0; i < ulNumThreads; i++)
					{
						ProcessingThreadData* pThreadData = rvecProcessingThreadData[i];
						ASSERT_RESOURCE_ALLOCATION("ELI17950", pThreadData != __nullptr);

						UCLID_FILEPROCESSINGLib::IFileProcessingTaskExecutorPtr ipExecutor = 
							pThreadData->m_ipTaskExecutor;
						ASSERT_RESOURCE_ALLOCATION("ELI17951", ipExecutor != __nullptr);

						ipExecutor->Close();
					}

					pFPM->signalWorkItemThreadsToStopAndWait();
				}
				catch (...)
				{
					uex::logOrDisplayCurrent("ELI14527", pFPM->m_hWndOfUI != __nullptr);
				}

				// Semaphore needs to be released so that the releaseProcessingThreadDataObjects will be
				// able to get the semaphore counts to delete the data
				guard.Unlock();

				// Release the memory for the Thread objects since they will not be needed
				// This should be done before the notification that processing is complete
				pFPM->releaseProcessingThreadDataObjects();

				// Release the memory for the Work Item thread objects since they are no longer needed
				pFPM->releaseWorkItemThreadDataObjects();

				CoUninitialize();
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28323")
		}
		catch(UCLIDException &ue)
		{
			pFPM->m_eventWatcherThreadExited.signal();
			throw ue;
		}
		pFPM->m_eventWatcherThreadExited.signal();
	}
	catch (...)
	{
		uex::logOrDisplayCurrent("ELI13892", pFPM && pFPM->m_hWndOfUI);
	}

	return 0;
}
//-------------------------------------------------------------------------------------------------
UINT CFileProcessingMgmtRole::handleStopRequestAsynchronously(void *pData)
{
	CFileProcessingMgmtRole* pFPM;
	try
	{
		pFPM = static_cast<CFileProcessingMgmtRole*>(pData);
		ASSERT_RESOURCE_ALLOCATION("ELI19422", pFPM != __nullptr);

		FPRecordManager* pRecordManager = pFPM->m_pRecordMgr;
		ASSERT_RESOURCE_ALLOCATION("ELI19431", pRecordManager != __nullptr);
		ASSERT_ARGUMENT("ELI19433", pFPM->m_ipRoleNotifyFAM != __nullptr );
		
		// Notify the queue that processing is stopping so it will no longer satisfy file
		// requests
		pRecordManager->stopProcessingQueue();

		// Notify the FAM that processing is cancelling
		pFPM->notifyFileProcessingTasksOfStopRequest();
		
		// The user (or OEM application) wants to stop the processing of files as soon as possible.
		// Indicate to the record manager that the pending files in the queue are to be discarded
		pRecordManager->discardProcessingQueue();
	}
	catch (...)
	{
		uex::logOrDisplayCurrent("ELI16258", pFPM && pFPM->m_hWndOfUI);
	}
	
	return 0;
}

//-------------------------------------------------------------------------------------------------
void CFileProcessingMgmtRole::processFiles2(ProcessingThreadData *pThreadData)
{
	// Keep processing tasks as long as tasks are available (or expect to be available)
	// The record manager's pop() method will return when a task is available (in which
	// case, it will return true), or return when processing should be stopped (in which
	// case it will return false).
	while (true)
	{
		// when false is returned by the pop() method, that means we should stop
		// processing...so just return
		FileProcessingRecord task;

		try
		{
			// If PauseProcessingQueue has been called, don't allow the next file to be grabbed
			// until ResumeProcessingQueue is called.
			m_eventProcessManagerActive.wait();

			// if parallelizable tasks being used manage the semaphore
			UPI upi = UPI::getCurrentProcessUPI();
			Win32Semaphore parallelSemaphore(upi.getProcessSemaphoreName());
		
			// Check if there are any files available right now.
			bool bProcessingActive;
			if (!m_pRecordMgr->pop(task, false, parallelSemaphore, &bProcessingActive))
			{
				// TODO: The assigned value seems like it ought to be negated...
				// (m_bHasProcessingCompleted is only used for unit tests so not a big deal
				// but there are comments that state it is does not work correctly for multi-threaded FAMs)
				m_bHasProcessingCompleted = m_pRecordMgr->areAnyFilesActive();

				// If not, and processing is no longer active, end the processing loop.
				if (!bProcessingActive)
				{
					return;
				}

				UCLID_FILEPROCESSINGLib::IFileProcessingTaskExecutorPtr ipExecutor =
					pThreadData->m_ipTaskExecutor;
				ASSERT_RESOURCE_ALLOCATION("ELI33925", ipExecutor != __nullptr);

				StandbyThread *pStandbyThread =
					new StandbyThread(m_eventManualStopProcessing, ipExecutor);

				// If waiting for the next file to be queued, initialize a standby thread to notify
				// the file processing tasks to standby.
				bool bGotFile = false;

				try
				{
					pStandbyThread->CreateThread();

					bGotFile = m_pRecordMgr->pop(task, true, parallelSemaphore);

					// Call endStandby so that the standby the thread will exit when all tasks have
					// responded to the standby call.
					pStandbyThread->endStandby();
				}
				catch (...)
				{
					// Ensure endStandby is called even in the case of an exception.
					pStandbyThread->endStandby();
					throw;
				}

				// If we didn't get any files after waiting, processing has ended.
				if (!bGotFile)
				{
					return;
				}
			}

			m_bHasProcessingCompleted = false;

			// we now have a valid task that we need to process
			try
			{
				// The semaphore has already been acquired
				Win32SemaphoreLockGuard lg(parallelSemaphore, false);

				// process the task
				processTask(task, pThreadData);
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI14336");
		}
		catch (UCLIDException ue)
		{
			UCLIDException ueThread("ELI23868", "Thread exited with exception.", ue);

			// Do not want to throw an exception here
			try
			{
				// Mark the current task as a processing error
				task.markAsProcessingError(ueThread.asStringizedByteStream());
				m_pRecordMgr->updateTask(task);
			}
			CATCH_AND_LOG_ALL_EXCEPTIONS("ELI23867");

			throw ueThread;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingMgmtRole::processTask(FileProcessingRecord& task, 
										  ProcessingThreadData* pThreadData)
{
	UCLID_FILEPROCESSINGLib::IFileProcessingMgmtRolePtr ipFPManagement = __nullptr;

	UCLIDException::SetFileContextForExceptions("ELI14340", task.getFileID(),
		[&]() -> void
		{
			ASSERT_ARGUMENT("ELI17943", pThreadData != __nullptr);

			ipFPManagement = UCLID_FILEPROCESSINGLib::IFileProcessingMgmtRolePtr(pThreadData->m_pFPMgmtRole);

			task.markAsStarted();
			m_pRecordMgr->updateTask(task);

			// Attempt executing the tasks on the current file and mark
			// the current file as either completed or pending.
			EFileProcessingResult eResult = startFileProcessingChain(task, pThreadData);
			switch (eResult)
			{
			case kProcessingSuccessful:
				task.markAsCompleted();
				break;

			case kProcessingSkipped:
				task.markAsSkipped();
				break;

				// Delayed indicates that the file did not complete processing and should be
				// returned to the front of the queue, while the FAM instance should continue
				// processing.
			case kProcessingDelayed:
				task.markAsPending();
				m_pRecordMgr->delay(task);
				break;

			case kProcessingCancelled:
				if (m_eCurrentRunningState != kScheduleStop && m_eCurrentRunningState != kPaused)
				{
					Stop();
				}
				task.markAsNone();
				break;
			}
		},
		[&](UCLIDException &ue) -> void 
		{
			// add the thread ID to the debug info
			ue.addDebugInfo("ThreadId", GetCurrentThreadId());
			ue.addDebugInfo("Top level File", task.getFileName());

			// Mark task as failed prior to running any error tasks so that we don't overwrite
			// a change that may be made by the error task
			task.markAsFailed(ue.asStringizedByteStream());

			// handleProcessingError will log the exception and execute error task as required
			handleProcessingError(task, pThreadData, ue);
		}
	);

	// Update the task even if an exception was caught indicating task failure (P13 #4398)
	m_pRecordMgr->updateTask(task);
}
//-------------------------------------------------------------------------------------------------
EFileProcessingResult CFileProcessingMgmtRole::startFileProcessingChain(FileProcessingRecord& task, 
													   ProcessingThreadData* pThreadData)
{
	try
	{
		ASSERT_ARGUMENT("ELI17944", pThreadData != __nullptr);

		UCLID_FILEPROCESSINGLib::IFileProcessingTaskExecutorPtr ipExecutor = 
			pThreadData->m_ipTaskExecutor;
		ASSERT_RESOURCE_ALLOCATION("ELI17945", ipExecutor != __nullptr);

		// If m_bProcessing is false it means processing has been stopped.
		if (!m_bProcessing && !m_bProcessingSingleFile)
		{
			return kProcessingCancelled;
		}

		// Attempt to process the file
		EFileProcessingResult eResult = (EFileProcessingResult) ipExecutor->ProcessFile(
			task.getFileRecord(), task.getFileRecord()->ActionID,
			task.m_ipProgressStatus, VARIANT_FALSE);

		return eResult;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI10909");
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingMgmtRole::handleProcessingError(FileProcessingRecord &task,
													const ProcessingThreadData* pThreadData,
													const UCLIDException &rUE)
{
	// This method is called from within a catch block, 
	// do not allow it to throw an exception
	try
	{
		// Always log the exception
		rUE.log();

		// Should error details be logged
		if (m_bLogErrorDetails)
		{
			// Resolve document tags in path to log file
			string strLogFile = CFileProcessingUtils::ExpandTagsAndTFE(getFAMTagManager(), m_strErrorLogFile, task.getFileName());

			// Check log file extension
			string strExt = getExtensionFromFullPath( strLogFile, true );
			if (strExt == ".uex")
			{
				// Log the exception to specified file
				rUE.log( strLogFile );
			}
//			else if (strExt == ".txt")
//			{
//				writeErrorDetailsText( strLogFile, strSourceDocument, rUE );
//			}
			// Unsupported file extension
			else
			{
				THROW_LOGIC_ERROR_EXCEPTION("ELI16057");
			}
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI18003");

	try
	{
		if (m_bSendErrorEmail)
		{
			IObjectWithDescriptionPtr ipErrorEmailTaskWrapper(CLSID_ObjectWithDescription);
			ASSERT_RESOURCE_ALLOCATION("ELI36142", ipErrorEmailTaskWrapper != __nullptr );

			IErrorEmailTaskPtr ipErrorEmailTask = getErrorEmailTask();
			ASSERT_RESOURCE_ALLOCATION("ELI36143", ipErrorEmailTask != __nullptr );

			ipErrorEmailTask->StringizedException = rUE.asStringizedByteStream().c_str();
			ipErrorEmailTaskWrapper->Object = ipErrorEmailTask;

			executeErrorTask(task, pThreadData, ipErrorEmailTaskWrapper);
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI36144");

	// Run task in a separate try block so that an exception while logging does not
	// prevent the error task from executing
	try
	{
		try
		{	
			// Should error task be executed
			if (isErrorHandlingTaskEnabled())
			{
				executeErrorTask(task, pThreadData, getErrorHandlingTask());
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI18013")
	}
	catch (UCLIDException &ue)
	{
		UCLIDException uexOuter("ELI18014", "Unable to execute error task!", ue);

		task.notifyErrorTaskFailed(ue.asStringizedByteStream());

		uexOuter.log();
	}
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingMgmtRole::executeErrorTask(FileProcessingRecord &task,
											   const ProcessingThreadData* pThreadData,
											   IObjectWithDescriptionPtr fileProcessingTask)
{
	ASSERT_ARGUMENT("ELI18007", pThreadData != __nullptr);

	UCLID_FILEPROCESSINGLib::IFileProcessingTaskExecutorPtr ipExecutor = 
		pThreadData->m_ipErrorTaskExecutor;
	ASSERT_RESOURCE_ALLOCATION("ELI18008", ipExecutor != __nullptr);

	task.notifyRunningErrorTask();

	// Create an error task "list" that contains the error task to run
	IIUnknownVectorPtr ipErrorTaskList(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI17993", ipErrorTaskList != __nullptr);

	ipErrorTaskList->PushBack(fileProcessingTask);

	// Run the task
	UCLID_FILEPROCESSINGLib::EFileProcessingResult eResult = ipExecutor->InitProcessClose(
		task.getFileRecord(), ipErrorTaskList, 
		task.getFileRecord()->ActionID, getFPMDB(), getFAMTagManager(), getFileRequestHandler(),
		task.m_ipProgressStatus, VARIANT_FALSE);

	// Log a cancellation during error task execution
	if (eResult == kProcessingCancelled)
	{
		UCLIDException ue("ELI18060","Application trace: Error task cancelled.");
		ue.addDebugInfo("File", task.getFileName());
		ue.addDebugInfo("Task", asString(getErrorHandlingTask()->Description));
		ue.log();
	}
	else if (eResult == kProcessingDelayed)
	{
		UCLIDException ue("ELI37480","Application trace: Error task delayed.");
		ue.addDebugInfo("File", task.getFileName());
		ue.addDebugInfo("Task", asString(getErrorHandlingTask()->Description));
		ue.log();
	}

	task.notifyErrorTaskCompleted();
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingMgmtRole::writeErrorDetailsText(const string& strLogFile, 
													const string& strSourceDocument, 
													const UCLIDException &rUE)
{
	// Open log file in thread-safe manner
	ThreadSafeLogFile tslf( strLogFile );

	// TODO: Append Error Summary section
	// - Header
	// - Filename
	// - Timestamp
	// - Machine, ProcessID, ThreadID
	// - UCLIDException.getTopText()

	// TODO: Append Task Information section
	// - Header
	// - Task number
	// - Task description

	// Append Exception Details section
	// - Header
	// - UCLIDException.asString()
	// - two blank lines
	tslf.writeLine( gstrEXCEPTION_DETAILS_HEADER );
	string strCompleteText;
	rUE.asString( strCompleteText );
	tslf.writeLine( strCompleteText );
	tslf.writeLine( "" );
	tslf.writeLine( "" );
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingMgmtRole::clear()
{
	// Release the database pointer
	m_pDB = NULL;

	// Release the processing tasks
	if (m_ipFileProcessingTasks != __nullptr)
	{
		m_ipFileProcessingTasks->Clear();
		m_ipFileProcessingTasks = __nullptr;
	}

	// Reset the number of threads
	m_nNumThreads = 0;

	// Reset the action name
	m_strAction = "";

	// Clear the continuous processing flags
	m_bKeepProcessingAsAdded = true;
	m_bOkToStopWhenQueueIsEmpty = false;

	// Clear the error log items
	m_bLogErrorDetails = false;
	m_strErrorLogFile.clear();

	// Clear the error email task
	m_bSendErrorEmail = false;
	m_ipErrorEmailTask = __nullptr;

	// Clear the error task
	m_ipErrorTask = __nullptr;

	// Clear the checkbox for the Processing tabs and the dirty flag
	m_bEnabled = false;
	m_bDirty = false;

	m_eQueueMode = kPendingAnyUserOrNoUser;

	// Reset the Processing schedule info
	m_bLimitProcessingToSchedule = false;
	m_vecScheduledHours.clear();

	// Default the schedule to all on
	m_vecScheduledHours.resize(giNUMBER_OF_HOURS_IN_WEEK, true);

	m_strFpsFile = "";

	// Clear the Parallel semaphore
	m_upParallelSemaphore.reset(__nullptr);
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr CFileProcessingMgmtRole::getFPMDB()
{
	// ensure that the db pointer is not NULL
	if (m_pDB == NULL)
	{
		throw UCLIDException("ELI14341", "No database available!");
	}

	return m_pDB;
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr CFileProcessingMgmtRole::getFAMTagManager()
{
	UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipTagManager = m_pFAMTagManager;
	ASSERT_RESOURCE_ALLOCATION("ELI14401", ipTagManager != __nullptr);

	// While the tag manager would initialize its own FAMDB instance if necessary, it is more
	// efficient to give it access to the instance we already have.
	ipTagManager->FAMDB = getFPMDB();

	return ipTagManager;
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileRequestHandlerPtr CFileProcessingMgmtRole::getFileRequestHandler()
{
	UCLID_FILEPROCESSINGLib::IFileRequestHandlerPtr ipFileRequestHandler = this;
	ASSERT_RESOURCE_ALLOCATION("ELI37481", ipFileRequestHandler != __nullptr);
	return ipFileRequestHandler;
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingMgmtRole::releaseProcessingThreadDataObjects()
{
	// Obtain this lock since both semaphores are needed
	CSingleLock lockThread(&m_threadLock, TRUE );

	// Need both semaphore counts since this method is deleting the
	// thread data
	CSingleLock lock1(&m_threadDataSemaphore, TRUE);
	CSingleLock lock2(&m_threadDataSemaphore, TRUE);
	
	// release memory allocated for FPThreadData objects referenced by the pointers 
	// in m_vecProcessingThreadData
	vector<ProcessingThreadData *>::const_iterator iter;
	for (iter = m_vecProcessingThreadData.begin(); iter != m_vecProcessingThreadData.end(); iter++)
	{
		// get the thread data object
		ProcessingThreadData *pFPThreadData = *iter;
		
		// if the thread was started and not yet ended, there's an internal logic error
		if ( pFPThreadData->m_threadStartedEvent.isSignaled() && !pFPThreadData->m_threadEndedEvent.isSignaled())
		{
			UCLIDException ue("ELI14342", "Internal error: File processing thread active, but associated thread data object is being deleted!");
			ue.log();
		}

		// release the memory
		delete pFPThreadData;
	}

	// clear the vector
	m_vecProcessingThreadData.clear();
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingMgmtRole::releaseWorkItemThreadDataObjects()
{
	// Obtain this lock since both semaphores are needed
	CSingleLock lockThread(&m_threadLock, TRUE );

	// Need both semaphore counts since this method is deleting the
	// thread data
	CSingleLock lock1(&m_threadDataSemaphore, TRUE);
	CSingleLock lock2(&m_threadDataSemaphore, TRUE);
	
	// release memory allocated for FPThreadData objects referenced by the pointers 
	// in m_vecWorkItemThreads
	vector<WorkItemThreadData *>::const_iterator iter;
	for (iter = m_vecWorkItemThreads.begin(); iter != m_vecWorkItemThreads.end(); iter++)
	{
		// get the thread data object
		WorkItemThreadData *pWIThreadData = *iter;
		
		// if the thread was started and not yet ended, there's an internal logic error
		if ( pWIThreadData->m_threadStartedEvent.isSignaled() && !pWIThreadData->m_threadEndedEvent.isSignaled())
		{
			UCLIDException ue("ELI36865", "Internal error: Work Item processing thread active, but associated thread data object is being deleted!");
			ue.log();
		}

		// release the memory
		delete pWIThreadData;
	}

	// clear the vector
	m_vecWorkItemThreads.clear();
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CFileProcessingMgmtRole::copyFileProcessingTasks(IIUnknownVectorPtr ipFileProcessingTasks)
{
	IIUnknownVectorPtr ipNewProcessors(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI11151", ipNewProcessors != __nullptr);

	long nSize = ipFileProcessingTasks->Size();
	int i;
	for(i = 0; i < nSize; i++)
	{
		// Retrieve the Object With Description
		IObjectWithDescriptionPtr ipObject(ipFileProcessingTasks->At(i));
		ASSERT_RESOURCE_ALLOCATION("ELI11148", ipObject != __nullptr );

		// Retrieve the associated file processor
		UCLID_FILEPROCESSINGLib::IFileProcessingTaskPtr ipFileProc(ipObject->Object);
		ASSERT_RESOURCE_ALLOCATION("ELI11149", ipFileProc != __nullptr );

		// Make separate copy of this file processor for the thread.
		// If the task cannot be run multithreaded, the task is required to 
		// control processing via mutexes as necessary to make it thread-safe
		// The copy needs to be made by saving and loading from a stream
		// so that the instanceGUID will be the same for each task so that 
		// restartable processing will work correctly.
		IPersistStreamPtr ipPersistObj(ipObject);

		// Create the stream
		IStreamPtr ipStream;
		if (FAILED(CreateStreamOnHGlobal(NULL, TRUE, &ipStream)))
		{
			throw UCLIDException("ELI37116", "Unable to create stream object to duplicate tasks!");
		}

		// stream the object into the IStream
		writeObjectToStream(ipPersistObj, ipStream, "ELI37117", FALSE);	

		// reset the stream current position to the beginning of the stream
		LARGE_INTEGER zeroOffset;
		zeroOffset.QuadPart = 0;
		ipStream->Seek(zeroOffset, STREAM_SEEK_SET, NULL);

		// stream the object out of the IStream
		readObjectFromStream(ipPersistObj, ipStream, "ELI37118");
		ASSERT_RESOURCE_ALLOCATION("ELI37119", ipPersistObj != __nullptr);

		ipNewProcessors->PushBack(ipPersistObj);
	}

	// Provide collection of file processors
	return ipNewProcessors;
}
//--------------------------------------------------------------------------------------------------
DWORD CFileProcessingMgmtRole::getActionID(const string & strAct)
{
	// Initialize action ID
	DWORD dwActionID = 0;

	try
	{
		return getFPMDB()->GetActionID(strAct.c_str());
	}
	catch (...)
	{
		uex::logOrDisplayCurrent("ELI14934", m_hWndOfUI != __nullptr);
	}
	
	return dwActionID;
}
//-------------------------------------------------------------------------------------------------
IErrorEmailTaskPtr CFileProcessingMgmtRole::getErrorEmailTask()
{
	if (m_ipErrorEmailTask == __nullptr)
	{
		m_ipErrorEmailTask.CreateInstance("Extract.FileActionManager.FileProcessors.SendEmailTask");
		ASSERT_RESOURCE_ALLOCATION("ELI36145", m_ipErrorEmailTask != __nullptr );

		m_ipErrorEmailTask->ApplyDefaultErrorEmailSettings();
	}

	return m_ipErrorEmailTask;
}
//-------------------------------------------------------------------------------------------------
IObjectWithDescriptionPtr CFileProcessingMgmtRole::getErrorHandlingTask()
{
	// Make sure the Error Task ObjectWithDescription has been created
	if (m_ipErrorTask == __nullptr)
	{
		m_ipErrorTask.CreateInstance(CLSID_ObjectWithDescription);
		ASSERT_RESOURCE_ALLOCATION("ELI16106", m_ipErrorTask != __nullptr );

		// By default, an error task should not be enabled.
		m_ipErrorTask->Enabled = VARIANT_FALSE;

		// ensure the dirty flag is cleared [P13 #4766]
		clearDirtyFlag( (IPersistStreamPtr) m_ipErrorTask);
	}

	return m_ipErrorTask;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingMgmtRole::isErrorHandlingTaskEnabled()
{
	// Check to see if an actual task is defined
	if (getErrorHandlingTask()->Object != __nullptr)
	{
		// Provide actual Enabled setting
		return asCppBool( getErrorHandlingTask()->Enabled );
	}
	else
	{
		// No defined task, therefore error task execution is not enabled
		return false;
	}
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingMgmtRole::validateLicense()
{
	// use the same ID as the file action manager.  If the FAM is licensed, so is this.
	static const unsigned long THIS_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI14343", "File Processing Management Role");
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingMgmtRole::notifyFileProcessingTasksOfStopRequest()
{
	// Need to obtain a semaphore since this method uses the m_vecProcessingThreadData vector
	CSingleLock lock(&m_threadDataSemaphore, TRUE);

	for (unsigned int i = 0; i < m_vecProcessingThreadData.size(); i++)
	{
		// Need to log any exceptions for stopping a processor but finish stopping the rest of them
		try
		{
			// Get the file processor that is currently running
			ProcessingThreadData* pThreadData = m_vecProcessingThreadData[i];
			ASSERT_RESOURCE_ALLOCATION("ELI17946", pThreadData != __nullptr);

			UCLID_FILEPROCESSINGLib::IFileProcessingTaskExecutorPtr ipTaskExecutor = 
				pThreadData->m_ipTaskExecutor;
			ASSERT_RESOURCE_ALLOCATION("ELI17947", ipTaskExecutor != __nullptr);

			UCLID_FILEPROCESSINGLib::IFileProcessingTaskExecutorPtr ipErrorExecutor = 
				pThreadData->m_ipErrorTaskExecutor;
			ASSERT_RESOURCE_ALLOCATION("ELI18009", ipErrorExecutor != __nullptr);

			ipTaskExecutor->Cancel();
			ipErrorExecutor->Cancel();
		}
		CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16209")
	}
}
//-------------------------------------------------------------------------------------------------
UINT CFileProcessingMgmtRole::processManager(void *pData)
{
	CFileProcessingMgmtRole* pFPM;
	try
	{
		// Cast the argument to FileProcessingMgmtRole
		pFPM = static_cast<CFileProcessingMgmtRole*>(pData);

		// Validate that the FPM is setup properly
		ASSERT_RESOURCE_ALLOCATION("ELI28202", pFPM != __nullptr);
		ASSERT_RESOURCE_ALLOCATION("ELI28203", pFPM->m_pRecordMgr != __nullptr);
		ASSERT_RESOURCE_ALLOCATION("ELI28204", pFPM->m_ipRoleNotifyFAM != __nullptr );

		CoInitializeEx(NULL, COINIT_MULTITHREADED);

		// Set up array of handles to be used for waiting that control the states
		HANDLE lpHandles[4];
		lpHandles[0] = pFPM->m_eventManualStopProcessing.getHandle();
		lpHandles[1] = pFPM->m_eventWatcherThreadExited.getHandle();
		lpHandles[2] = pFPM->m_eventPause.getHandle();
		lpHandles[3] = pFPM->m_eventResume.getHandle();

		try
		{
			// Signal that the process manager has started.
			pFPM->m_eventProcessManagerStarted.signal();
			pFPM->m_eventProcessManagerActive.signal();
			
			ERunningState eNextRunningState;
			
			pFPM->timeTillNextProcessingChange(eNextRunningState);

			bool bExit = false;

			// Wrap the FAMDB as a smart pointer
			UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFamDB(pFPM->m_pDB);
			ASSERT_RESOURCE_ALLOCATION("ELI29886", ipFamDB != __nullptr);

			// Set up what the current running state should be
			if (pFPM->m_bLimitProcessingToSchedule)
			{
				switch (eNextRunningState)
				{
				case kNormalStop: 
					// no processing at all
					bExit = true;
					break;
				case kNormalRun:
					// always process
					break;
				case kScheduleRun:
					eNextRunningState = kScheduleStop;
					break;
				case kScheduleStop:
					eNextRunningState = kScheduleRun;
					break;
				}
			}
				
			pFPM->m_eCurrentRunningState = eNextRunningState;

			if (!bExit)
			{
				// A failure in startProcessing signals the processing to stop. The signal is 
				// handled later in this method. If an exception occurs, display it and continue.
				// [FIDSC #3742]
				try
				{
					// Start processing, if should not be processing this will put the UI in the state
					// that will look like it is not processing because it is not scheduled to process
					pFPM->startProcessing(eNextRunningState == kScheduleStop);
				}
				catch (...)
				{
					uex::logOrDisplayCurrent("ELI28507", pFPM && pFPM->m_hWndOfUI);
				}

				// Post Schedule Inactive message to the UI
				if ( eNextRunningState == kScheduleStop && pFPM->m_hWndOfUI != __nullptr)
				{
					::PostMessage( pFPM->m_hWndOfUI, FP_SCHEDULE_INACTIVE, 0, 0);
				}
			}

			while(!bExit)
			{
				// Wait on all the events that cause a change in the processing state
				DWORD rtnValue = WaitForMultipleObjects(4, lpHandles, FALSE, 
					pFPM->timeTillNextProcessingChange(eNextRunningState));
				
				// Save the previous state
				ERunningState ePreviousState = pFPM->m_eCurrentRunningState;

				// Determine the next state
				switch ( rtnValue )
				{
				case WAIT_TIMEOUT:
					// If the wait timed out need to change to the next running state
					// returned by the timeTillNextProcessingChange function
					// Only change the state if it is not paused
					if (ePreviousState != kPaused)
					{
						pFPM->m_eCurrentRunningState = eNextRunningState;
					}
					break;
				case WAIT_OBJECT_0:
				case WAIT_OBJECT_0 + 1:
					// Manual stop or stop due to watcher thread exit after no further files to 
					// process
					pFPM->m_eCurrentRunningState = kNormalStop;
					break;
				case WAIT_OBJECT_0 + 2:
					// Processing should be paused
					pFPM->m_eCurrentRunningState = kPaused;
					pFPM->m_eventPause.reset();
					break;
				case WAIT_OBJECT_0 + 3:
					// Resume processing after a pause
					// If the processing is being limited to a schedule need to set it to the
					// correct processing state
					if (pFPM->m_bLimitProcessingToSchedule)
					{
						// eNextRunningState is the state that will be changed to if the wait
						// for the events timed out so if eNextRunningState is ScheduleRun then 
						// processing should be stopped otherwise it should be running
						pFPM->m_eCurrentRunningState = (eNextRunningState == kScheduleRun) ? 
							kScheduleStop : kScheduleRun;
					}
					else
					{
						// Processing should be running
						pFPM->m_eCurrentRunningState = kNormalRun;
					}
					// Reset resume event
					pFPM->m_eventResume.reset();
					break;
				default:
					THROW_LOGIC_ERROR_EXCEPTION("ELI28347");
					break;
				}
					
				// Change the processing state
				switch (pFPM->m_eCurrentRunningState)
				{
				case kPaused:
				case kScheduleStop:
				case kNormalStop:
					// Need to stop the processing if previously running 
					if (ePreviousState == kScheduleRun || ePreviousState == kNormalRun)
					{
						// Stop the processing
						pFPM->stopProcessing();

						// Wait for event watcher thread to exit or a manual stop
						rtnValue = WaitForMultipleObjects(2, lpHandles, FALSE, INFINITE);
						switch( rtnValue)
						{
						case WAIT_OBJECT_0:
							// Set the current running state to normal stop
							pFPM->m_eCurrentRunningState = kNormalStop;

							// Stop the processing
							pFPM->stopProcessing();

							// Need to wait for the watcher thread to exit
							pFPM->m_eventWatcherThreadExited.wait();

							// Close the database connections
							ipFamDB->CloseAllDBConnections();
							break;
						case WAIT_OBJECT_0 + 1:
							// Reset watcher thread exited event
							pFPM->m_eventWatcherThreadExited.reset();

							// Close the database connections
							ipFamDB->CloseAllDBConnections();

							// If not a normal stop, log processing inactive message
							if (pFPM->m_eCurrentRunningState != kNormalStop)
							{
								string strTraceMessage =
									string("Application trace: File Action Manager processing ") +
									((pFPM->m_eCurrentRunningState == kScheduleStop)
										? "now inactive per schedule."
										: "paused.");
										
								UCLIDException ue("ELI30308", strTraceMessage);
								ue.addDebugInfo("FPS File",
									pFPM->m_strFpsFile.empty() ? "<Not Saved>" : pFPM->m_strFpsFile);
								ue.log();

								// Notify the UI of processing inactive
								if (pFPM->m_hWndOfUI != __nullptr)
								{
									::PostMessage( pFPM->m_hWndOfUI, FP_SCHEDULE_INACTIVE, 0, 0);
								}
							}
							break;
						default:
							UCLIDException ue("ELI29806", "Stop events are in a bad state.");
							throw ue;
						}
					}
					break;
				case kScheduleRun:
				case kNormalRun:
					// Start processing
					if (ePreviousState == kPaused || ePreviousState == kScheduleStop)
					{
						pFPM->startProcessing();

						string strTraceMessage =
							string("Application trace: File Action Manager processing ") +
							((pFPM->m_eCurrentRunningState == kScheduleRun)
								? "now active per schedule."
								: "unpaused.");

						UCLIDException ue("ELI30309", strTraceMessage);
						ue.addDebugInfo("FPS File",
							pFPM->m_strFpsFile.empty() ? "<Not Saved>" : pFPM->m_strFpsFile);
						ue.log();

						// Notify UI that processing is running
						if (pFPM->m_hWndOfUI != __nullptr)
						{
							::PostMessage( pFPM->m_hWndOfUI, FP_SCHEDULE_ACTIVE, 0, 0);

						}
					}
					break;
				}

				// If this a normal stop exit loop
				if (pFPM->m_eCurrentRunningState == kNormalStop)
				{
					bExit = true;
				}
			}

			// Set the processing flag to false
			pFPM->m_bProcessing = false;

			// Post message to UI that the schedule is active so the Processing Inactive 
			// message is removed
			if (pFPM->m_hWndOfUI != __nullptr)
			{
				// Do not log the schedule active trace here as processing has stopped
				// this is just to ensure the inactive message is cleared out
				::PostMessage( pFPM->m_hWndOfUI, FP_SCHEDULE_ACTIVE, 0, 0);
			}

			// Notify the FAM that processing is complete
			UCLID_FILEPROCESSINGLib::IRoleNotifyFAMPtr ipRoleNotifyFAM = pFPM->m_ipRoleNotifyFAM;
			ASSERT_RESOURCE_ALLOCATION("ELI28315", ipRoleNotifyFAM != __nullptr);
			ipRoleNotifyFAM->NotifyProcessingCompleted();

			pFPM->m_eventProcessManagerActive.reset();
			pFPM->m_eventProcessManagerExited.signal();
		}
		catch(...)
		{
			pFPM->m_eventProcessManagerActive.reset();
			pFPM->m_eventProcessManagerExited.signal();
			throw;
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI28205");
	return 0;
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingMgmtRole::startProcessing(bool bDontStartThreads)
{
	if (m_bProcessingSingleFile)
	{
		throw UCLIDException("ELI29556", 
			"Cannot start processing; currently processing a file independently.");
	}

	// Obtain this lock since both semaphores are needed
	CSingleLock lockThread(&m_threadLock, TRUE );

	// Acquire both semaphore counts since the thread data will be 
	// created in this method
	CSingleLock guard( &m_threadDataSemaphore, TRUE );
	CSingleLock guard2( &m_threadDataSemaphore, TRUE );

	try
	{
		try
		{
			m_eventWatcherThreadExited.reset();

			// If not starting off the threads don't start the thread watcher.
			if (!bDontStartThreads)
			{
				// Kick off the thread which will wait for all file processing threads
				// to complete and then update the UI
				// By putting this kick-off this early in this method, we can benefit from 
				// the fact that all the situations related to processing ending
				// (due to errors or due to successful processing) can be handled in one
				// place (i.e. this thread function).
				AfxBeginThread(fileProcessingThreadsWatcherThread, this);
			}

			// Get the Action ID
			long lActionID = getActionID(m_strAction);

			// Set action ID to the record manager
			m_pRecordMgr->setActionID(lActionID);

			// Signal Not Paused event so that processing will continue
			m_eventPause.reset();

			// if there is a dialog set it to receive status updates
			if(m_hWndOfUI)
			{
				m_pRecordMgr->setDlg(m_hWndOfUI);
			}

			// clear all the records in the file processing record manager
			// (i.e. clear the queue of files to process)
			m_pRecordMgr->clear(false);

			// Set whether processing skipped files, etc
			m_pRecordMgr->setQueueMode(m_eQueueMode);

			// Set the KeepProcssingAsAdded for the record manager
			// but only if it is ok to stop when queue is empty
			if ( m_bOkToStopWhenQueueIsEmpty )
			{
				m_pRecordMgr->setKeepProcessingAsAdded(m_bKeepProcessingAsAdded);
			}
			else
			{
				// This tells the record manager to keep trying to get
				// files to process from the database
				m_pRecordMgr->setKeepProcessingAsAdded(true);
			}

			// create the data structs for each of the threads
			// Note: that an unique_ptr cannot be used here because an
			// array must be deleted specially with delete []
			// Note: a vector cannot be used here because all the members of
			// ProcessingThreadData do not have correct copy -constructors and
			// assignments operators namely Win32Event

			// a value of 0 means use one thread per processor
			long nNumThreads = m_nNumThreads;
			if (nNumThreads == 0)
			{
				nNumThreads = getNumLogicalProcessors();
			}

			if (!bDontStartThreads)
			{
				// Create the processing semaphore needed for any processing
				createProcessingSemaphore(nNumThreads);

				// if there are parallelizable tasks setup the thread data
				bool bParallelize = setupWorkItemThreadData(nNumThreads, lActionID);			

				// Create all the file processors for the threads and 
				// notify them that processing is about to begin
				for (int i = 0; i < nNumThreads; i++)
				{
					// Create a thread data struct and add it to the vector
					ProcessingThreadData* pThreadData = new ProcessingThreadData();
					ASSERT_RESOURCE_ALLOCATION("ELI17948", pThreadData != __nullptr);
					m_vecProcessingThreadData.push_back(pThreadData);

					// Update the thread data members
					pThreadData->m_pFPMgmtRole = this;

					// Initialize executor with a new copy of the file processors for the thread
					UCLID_FILEPROCESSINGLib::IFileProcessingTaskExecutorPtr ipExecutor = 
						pThreadData->m_ipTaskExecutor;
					ASSERT_RESOURCE_ALLOCATION("ELI17949", ipExecutor != __nullptr);
					ipExecutor->Init(copyFileProcessingTasks(m_ipFileProcessingTasks),
						m_pRecordMgr->getActionID(), getFPMDB(), getFAMTagManager(),
						getFileRequestHandler());
				}
			}

			// start the processing
			m_bProcessing = true;
			
			if (!bDontStartThreads)
			{
				unsigned long ulStackSize = getProcessingThreadStackSize();
				
				// Start work item threads first so they will start processing existing work items
				// before processing thread get a file to  process
				startWorkItemThreads(ulStackSize);
				
				// begin the threads to process files in parallel
				for (int i = 0; i < nNumThreads; i++)
				{
					ProcessingThreadData* pThreadData = m_vecProcessingThreadData[i];

					// begin the thread
					pThreadData->m_pThread =
						AfxBeginThread(fileProcessingThreadProc, pThreadData, 0, ulStackSize);
					ASSERT_RESOURCE_ALLOCATION("ELI11075", pThreadData->m_pThread != __nullptr);

					// wait for the thread to start
					pThreadData->m_threadStartedEvent.wait();
				}
			}
		}
		catch (...)
		{
			// Don't want to throw any exceptions here but should log any from these methods
			try
			{
				// do the stop
				getThisAsCOMPtr()->Stop();
			}
			CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16212");

			// The exception should be rethrown
			// if any threads had started processing there should be a ProcessingCompleted message sent when
			// they stop
			throw;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28206");
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingMgmtRole::stopProcessing()
{
	try
	{
		// check pre-conditions
		ASSERT_ARGUMENT("ELI28207", m_bEnabled == true);
		ASSERT_ARGUMENT("ELI28208", m_ipFileProcessingTasks != __nullptr);
		ASSERT_ARGUMENT("ELI28209", m_ipFileProcessingTasks->Size() > 0);
		ASSERT_ARGUMENT("ELI28210", m_pRecordMgr != __nullptr);

		// The Processing needs to be stopped asynchronously
		// to keep the UI from being blocked
		AfxBeginThread(handleStopRequestAsynchronously, this);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28211");
}
//-------------------------------------------------------------------------------------------------
unsigned long CFileProcessingMgmtRole::timeTillNextProcessingChange( ERunningState &eNextRunningState)
{
	// If not limiting to a schedule running state should always be kNormalRun
	if (!m_bLimitProcessingToSchedule)
	{
		eNextRunningState = kNormalRun; // should be running now and never change to not running
		return INFINITE;
	}

	// Get the current time
	CTime timeCurrent = CTime::GetCurrentTime();

	// Determine the current hour of the week to be an index into the vector
	long nHourOfWeek = ((long) timeCurrent.GetDayOfWeek() - 1) * 24 + timeCurrent.GetHour();

	// Make sure nHourOfWeek is in the correct range
	if (nHourOfWeek < 0 || nHourOfWeek >= giNUMBER_OF_HOURS_IN_WEEK)
	{
		UCLIDException ue("ELI28316", "Hour of week is out of range.");
		ue.addDebugInfo("HourOfWeek", nHourOfWeek);
		throw ue;
	}

	// The next state should be the opposite of the state for the current hour
	eNextRunningState = (m_vecScheduledHours[nHourOfWeek]) ? kScheduleStop : kScheduleRun;

	// Calculate the time that the state will change

	// Set to dwTimeTilNextChange to the number of milliseconds left in the current hour
	DWORD dwTimeTilNextChange = ((59 - timeCurrent.GetMinute()) * 60 + 60 - timeCurrent.GetSecond()) * 1000;
	
	long n;

	// The next change state to look for
	bool bNextChangeRun = eNextRunningState == kScheduleRun;

	// To calculate the time till next change, step through vector starting from the next hour
	// until either the expected transition or until all of the Hours have been checked
	for ( n = nHourOfWeek + 1; n < nHourOfWeek + giNUMBER_OF_HOURS_IN_WEEK; n++)
	{
		// If the run state is the one we are looking for break out of the loop
		if (m_vecScheduledHours[n % giNUMBER_OF_HOURS_IN_WEEK] == bNextChangeRun)
		{
			break;
		}

		// Add the number of milliseconds in an hour
		dwTimeTilNextChange += 3600000;
	}
	
	// Check to see if there was no transition
	if ( n >= nHourOfWeek + giNUMBER_OF_HOURS_IN_WEEK )
	{
		// If the next running state is Run then the schedule is all stop
		if (eNextRunningState == kScheduleRun)
		{
			eNextRunningState = kNormalStop;
			return 0;
		}
		else if ( eNextRunningState == kScheduleStop)
		{
			// The schedule is all run so return kNormalRun with INFINITE wait
			eNextRunningState = kNormalRun;
			return INFINITE;
		}
	}
	return dwTimeTilNextChange;
}
//-------------------------------------------------------------------------------------------------
UINT CFileProcessingMgmtRole::processSingleFileThread(void *pData)
{
	try
	{
		CoInitializeEx(NULL, COINIT_MULTITHREADED);
		
		// Initialize thread data, management role and file record from pData.
		ProcessingThreadData *pThreadData = static_cast<ProcessingThreadData *>(pData);
		ASSERT_ARGUMENT("ELI34993", pThreadData != __nullptr);

		CFileProcessingMgmtRole *pMgmtRole = pThreadData->m_pFPMgmtRole;
		ASSERT_RESOURCE_ALLOCATION("ELI34994", pMgmtRole != __nullptr);

		ASSERT_RESOURCE_ALLOCATION("ELI34995", pMgmtRole->m_upProcessingSingleFileTask.get() != __nullptr);
		FileProcessingRecord &task(*(pMgmtRole->m_upProcessingSingleFileTask.get()));

		try
		{
			try
			{
				UCLID_FILEPROCESSINGLib::IFileProcessingTaskExecutorPtr ipExecutor = 
					pThreadData->m_ipTaskExecutor;
				ASSERT_RESOURCE_ALLOCATION("ELI34996", ipExecutor != __nullptr);

				pThreadData->m_ipTaskExecutor->Init(pMgmtRole->m_ipFileProcessingTasks,
					pMgmtRole->m_pRecordMgr->getActionID(),
					pMgmtRole->getFPMDB(), pMgmtRole->getFAMTagManager(),
					pMgmtRole->getFileRequestHandler());

				auto& fileRecord = pMgmtRole->m_ipProcessingSingleFileRecord;

				// Expect the same status that was previously calculated as the fallback status to be the current status
				// https://extract.atlassian.net/browse/ISSUE-18826
				_bstr_t fromStatus = pMgmtRole->getFPMDB()->AsStatusString(fileRecord->FallbackStatus);

				// Create a FileProcessingRecord for the file and add it to the record manager's queue.
				auto& retrievedFileRecord = pMgmtRole->getFPMDB()->GetFileToProcess(
					fileRecord->FileID,
					pMgmtRole->getFPMDB()->GetActionName(fileRecord->ActionID),
					fromStatus);

				ASSERT_RUNTIME_CONDITION("ELI53792", retrievedFileRecord != __nullptr, "File is not available for processing!")

				// Though the record manager is in a sense unnecessary when processing a single file,
				// because of the close way it is tied to processing, exceptions will be raised if the file
				// does not follow the same code-path through push and pop prior to processing.
				pMgmtRole->m_pRecordMgr->push(task);

				// if parallelizable tasks being used manage the semaphore
				UPI upi = UPI::getCurrentProcessUPI();
				Win32Semaphore parallelSemaphore(upi.getProcessSemaphoreName());

				// Need to signal that the processManager is active (for single file this is the 
				// process manager
				pMgmtRole->m_eventProcessManagerActive.signal();

				pMgmtRole->m_pRecordMgr->pop(task, false, parallelSemaphore);
				
				// The semaphore has already been acquired
				Win32SemaphoreLockGuard lg(parallelSemaphore, false);
				
				// Process the file
				pMgmtRole->processTask(task, pThreadData);

				ipExecutor->Close();
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI34997")
		}
		catch (UCLIDException &ue)
		{
			// Use the task's exception member to pass out any exception to the calling thread.
			task.m_strException = ue.asStringizedByteStream();
		}

		// notify interested parties that the thread has ended
		pThreadData->m_threadEndedEvent.signal();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI34998");

	CoUninitialize();

	return 0;
}
//-------------------------------------------------------------------------------------------------
unsigned long CFileProcessingMgmtRole::getProcessingThreadStackSize()
{
	unsigned long ulMinStackSize = 0;
	int nTaskCount = m_ipFileProcessingTasks->Size();
	for (int i = 0; i < nTaskCount ; i++)
	{
		// Get the current object as ObjectWithDescription
		IObjectWithDescriptionPtr ipOWD = m_ipFileProcessingTasks->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI35031", ipOWD != __nullptr);

		UCLID_FILEPROCESSINGLib::IFileProcessingTaskPtr ipFileProcessor(ipOWD->Object);
		ASSERT_RESOURCE_ALLOCATION("ELI35032", ipFileProcessor != __nullptr);

		// Check the object for requires admin access
		ulMinStackSize = max(ulMinStackSize, ipFileProcessor->MinStackSize);
	}

	return ulMinStackSize;
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingMgmtRole::createProcessingSemaphore(long nNumberOfCounts)
{
	// if going to be using parallelizable tasks create the semaphore
	UPI upi = UPI::getCurrentProcessUPI();

	// make sure if the semaphore exists that it is destroyed first
	m_upParallelSemaphore.reset(__nullptr);
	m_upParallelSemaphore.reset(new Win32Semaphore(nNumberOfCounts, nNumberOfCounts, upi.getProcessSemaphoreName()));
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingMgmtRole::setupWorkItemThreadData(long nNumberOfThreads, long lActionID)
{
	// check for parallelizable tasks in the list if found set the semaphore
	int numberOfTasks = m_ipFileProcessingTasks->Size();
	bool bParallelize = false;
	for (int i = 0; i < numberOfTasks; i++)
	{
		// Retrieve the Object With Description
		IObjectWithDescriptionPtr ipObject(m_ipFileProcessingTasks->At(i));
		ASSERT_RESOURCE_ALLOCATION("ELI36864", ipObject != __nullptr );

		if (asCppBool(ipObject->Enabled))
		{
			UCLID_FILEPROCESSINGLib::IParallelizableTaskPtr ipParallelTask(ipObject->Object);
			if (ipParallelTask != NULL && ipParallelTask->Parallelize == VARIANT_TRUE)
			{
				bParallelize = true;
				break;
			}
		}
	}
	if(bParallelize)
	{
		// Create the workitem processing threads
		for (int i = 0; i < nNumberOfThreads; i++ )
		{
			WorkItemThreadData *threadData = 
				new WorkItemThreadData(this, lActionID, *m_upParallelSemaphore.get(), m_pDB);
					
			m_vecWorkItemThreads.push_back(threadData);

		}
		// if the record manager is set, enable parallelizable
		if (m_pRecordMgr != __nullptr)
		{
			m_pRecordMgr->enableParallelizable(true);
		}
	}
	return bParallelize;
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingMgmtRole::startWorkItemThreads(unsigned long ulStackSize)
{
	long nSize = m_vecWorkItemThreads.size();
	for (int i = 0; i < nSize; i++)
	{
		WorkItemThreadData *pThreadData = m_vecWorkItemThreads[i];
		AfxBeginThread(workItemProcessingThreadProc, pThreadData, 0, ulStackSize);
		pThreadData->m_threadStartedEvent.wait();
	}
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingMgmtRole::signalWorkItemThreadsToStopAndWait()
{
	// workitem threads to exit if parallelizable tasks are being used
	long nSize = m_vecWorkItemThreads.size();
	if (nSize > 0)
	{
		// Signal threads to exit
		WorkItemThreadData::ms_threadStopProcessing.signal();
					
		// wait for each to stop
		for (long i = 0; i < nSize; i++)
		{
			WorkItemThreadData *p = m_vecWorkItemThreads[i];

			// only wait if the thread started
			if (p->m_threadStartedEvent.isSignaled())
			{
				// 
				p->m_threadEndedEvent.wait();
			}
		}
		WorkItemThreadData::ms_threadStopProcessing.reset();
	}
}
