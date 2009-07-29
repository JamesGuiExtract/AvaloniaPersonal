// FileProcessingMgmtRole.cpp : Implementation of CFileProcessingMgmtRole

#include "stdafx.h"
#include "UCLIDFileProcessing.h"
#include "FileProcessingMgmtRole.h"
#include "FP_UI_Notifications.h"
#include "FileProcessingUtils.h"

#include <LicenseMgmt.h>
#include <COMUtils.h>
#include <cpputil.h>
#include <ComponentLicenseIDs.h>
#include <ThreadSafeLogFile.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 4;

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
// ProcessingThreadData
//-------------------------------------------------------------------------------------------------
ProcessingThreadData::ProcessingThreadData()
:	m_pThread(NULL), m_pFPMgmtRole(NULL)
{
	try
	{
		m_ipTaskExecutor.CreateInstance(CLSID_FileProcessingTaskExecutor);
		ASSERT_RESOURCE_ALLOCATION("ELI17845", m_ipTaskExecutor != NULL);

		m_ipErrorTaskExecutor.CreateInstance(CLSID_FileProcessingTaskExecutor);
		ASSERT_RESOURCE_ALLOCATION("ELI18006", m_ipErrorTaskExecutor != NULL);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI17942");
}
//-------------------------------------------------------------------------------------------------
ProcessingThreadData::~ProcessingThreadData()
{
	try
	{
		// Release the file processing tasks
		m_ipTaskExecutor = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI12958");
}

//-------------------------------------------------------------------------------------------------
// CFileProcessingMgmtRole
//-------------------------------------------------------------------------------------------------
CFileProcessingMgmtRole::CFileProcessingMgmtRole()
:m_pRecordMgr(NULL),
	m_ipRoleNotifyFAM(NULL),
	m_threadDataSemaphore(2,2)
{
	try
	{
		// clear internal data
		clear();
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI14299")
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
		&IID_IFileProcessingMgmtRole
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
STDMETHODIMP CFileProcessingMgmtRole::Start(IFileProcessingDB *pDB, BSTR bstrAction, long hWndOfUI, 
											IFAMTagManager *pTagManager, IRoleNotifyFAM *pRoleNotifyFAM)
{
	// Obtain this lock since both semaphores are needed
	CSingleLock lockThread(&m_threadLock, TRUE );

	// Aquire both semaphore counts since the thread data will be 
	// created in this method
	CSingleLock guard( &m_threadDataSemaphore, TRUE );
	CSingleLock guard2( &m_threadDataSemaphore, TRUE );

	try
	{
		try
		{
			validateLicense();

			// check pre-conditions
			ASSERT_ARGUMENT("ELI14296", m_bEnabled == true);
			ASSERT_ARGUMENT("ELI14301", m_ipFileProcessingTasks != NULL);
			ASSERT_ARGUMENT("ELI14302", m_ipFileProcessingTasks->Size() > 0);
			ASSERT_ARGUMENT("ELI14344", m_pRecordMgr != NULL);

			// Set the File Action manager pointer
			m_ipRoleNotifyFAM = pRoleNotifyFAM;
			ASSERT_RESOURCE_ALLOCATION("ELI14531", m_ipRoleNotifyFAM != NULL );

			// Kick off the thread which will wait for all file processing threads
			// to complete and then update the UI
			// By putting this kick-off this early in this method, we can benefit from 
			// the fact that all the sitations related to processing ending
			// (due to errors or due to successful processing) can be handled in one
			// place (i.e. this thread function).
			AfxBeginThread(fileProcessingThreadsWatcherThread, this);

			// store the pointer to the DB so that subsequent calls to getFPDB() will work correctly
			m_pDB = pDB;

			// Get the action's ID from name
			DWORD dwActionID = getActionID(asString(bstrAction));

			// Set action ID to the record manager
			m_pRecordMgr->setActionID(dwActionID);

			// Signal Not Paused event so that processing will continue
			m_eventResume.signal();

			// store the pointer to the TagManager so that subsequent calls to getFPMTagManager() will work
			m_pFAMTagManager = pTagManager;

			// remember the action name
			m_strAction = asString(bstrAction);

			// remember the handle of the UI so that messages can be sent to it
			m_hWndOfUI = (HWND) hWndOfUI;

			// if there is a dialog set it to receive status updates
			if(m_hWndOfUI)
			{
				m_pRecordMgr->setDlg(m_hWndOfUI);
			}

			// clear all the records in the file processing record manager
			// (i.e. clear the queue of files to process)
			m_pRecordMgr->clear(true);

			// Set whether processing skipped files or not
			m_pRecordMgr->setProcessSkippedFiles(m_bProcessSkippedFiles);
			m_pRecordMgr->setSkippedForCurrentUser(!m_bSkippedForAnyUser);

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
			// Note: that an auto_ptr cannot be used here because an
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

			// Create all the file processors for the threads and 
			// notify them that processing is about to begin
			for (int i = 0; i < nNumThreads; i++)
			{
				m_vecProcessingThreadData.push_back(new ProcessingThreadData());
				// We'll work with rThreadData here instead of tmp
				// because rThreadData is the actual ProcessingThreadData stored in
				// the array which is a copy of tmp;
				// get a reference to the thread data struct and update data members
				ProcessingThreadData* pThreadData = m_vecProcessingThreadData[i];
				ASSERT_RESOURCE_ALLOCATION("ELI17948", pThreadData != NULL);

				UCLID_FILEPROCESSINGLib::IFileProcessingTaskExecutorPtr ipExecutor = 
					pThreadData->m_ipTaskExecutor;
				ASSERT_RESOURCE_ALLOCATION("ELI17949", ipExecutor != NULL);

				pThreadData->m_pFPMgmtRole = this;
				
				// Initialize executor with a new copy of the file processors for the thread
				ipExecutor->Init(copyFileProcessingTasks(m_ipFileProcessingTasks),
					getFPMDB(), getFAMTagManager());
			}

			// start the processing
			m_bProcessing = true;
			
			// begin the threads to process files in parallel
			for (int i = 0; i < nNumThreads; i++)
			{
				ProcessingThreadData* pThreadData = m_vecProcessingThreadData[i];

				// begin the thread
				pThreadData->m_pThread = AfxBeginThread(fileProcessingThreadProc, m_vecProcessingThreadData[i]);
				ASSERT_RESOURCE_ALLOCATION("ELI11075", pThreadData->m_pThread != NULL);

				// wait for the thread to start
				pThreadData->m_threadStartedEvent.wait();
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

		// check pre-conditions
		ASSERT_ARGUMENT("ELI14303", m_bEnabled == true);
		ASSERT_ARGUMENT("ELI14316", m_ipFileProcessingTasks != NULL);
		ASSERT_ARGUMENT("ELI14317", m_ipFileProcessingTasks->Size() > 0);
		ASSERT_ARGUMENT("ELI14345", m_pRecordMgr != NULL);

		// Notify the FAM that processing is cancelling
		m_ipRoleNotifyFAM->NotifyProcessingCancelling();

		// The Processing needs to be stopped asynchronously
		// to keep the UI from being blocked
		AfxBeginThread(handleStopRequestAsynchronously, this);
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

		// Cause processing to wait until this m_eventResume is signaled
		m_eventResume.reset();
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
				ASSERT_RESOURCE_ALLOCATION("ELI16007", ipOWD != NULL);

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

			// Check for defined error task
			if (asCppBool(getErrorHandlingTask()->Enabled))
			{
				UCLID_FILEPROCESSINGLib::IFileProcessingTaskPtr ipErrorTask(getErrorHandlingTask()->Object);
				if (ipErrorTask == NULL)
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
// IFileProcessingMgmtRole interface implementation
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::get_FileProcessors(IIUnknownVector ** pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_ipFileProcessingTasks == NULL)
		{
			m_ipFileProcessingTasks.CreateInstance(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI08936", m_ipFileProcessingTasks != NULL);
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
		ASSERT_ARGUMENT("ELI14286", pRecordMgr != NULL);

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
		ASSERT_ARGUMENT("ELI14518", pVal != NULL );
		
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

		if (m_pRecordMgr != NULL )
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
		ASSERT_ARGUMENT("ELI14521", pVal != NULL );
		
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
		ASSERT_ARGUMENT("ELI16060", pVal != NULL );

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
		ASSERT_ARGUMENT("ELI16063", pVal != NULL );

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
STDMETHODIMP CFileProcessingMgmtRole::get_ProcessSkippedFiles(VARIANT_BOOL *pbVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI26916", pbVal != NULL);

		// Get the skipped files value
		*pbVal = asVariantBool(m_bProcessSkippedFiles);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26917");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::put_ProcessSkippedFiles(VARIANT_BOOL bNewVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Update skipped files value
		m_bProcessSkippedFiles = asCppBool(bNewVal);

		// Set dirty flag
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26918");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::get_SkippedForAnyUser(VARIANT_BOOL* pbVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI26919", pbVal != NULL);

		// Get the skipped user name
		*pbVal = asVariantBool(m_bSkippedForAnyUser);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26920");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingMgmtRole::put_SkippedForAnyUser(VARIANT_BOOL bNewVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Update the skipped user name
		m_bSkippedForAnyUser = asCppBool(bNewVal);

		// Set dirty flag
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26921");
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
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	// if the directly held data is dirty, then indicate to the caller that
	// this object is dirty
	if (m_bDirty)
	{
		return S_OK;
	}

	// check if the file processors vector object is dirty
	if (m_ipFileProcessingTasks != NULL)
	{
		IPersistStreamPtr ipFPStream = m_ipFileProcessingTasks;
		ASSERT_RESOURCE_ALLOCATION("ELI14333", ipFPStream != NULL);
		if (ipFPStream->IsDirty() == S_OK)
		{
			return S_OK;
		}
	}

	// check if the error task is dirty
	if (m_ipErrorTask != NULL)
	{
		IPersistStreamPtr ipFPStream = m_ipErrorTask;
		ASSERT_RESOURCE_ALLOCATION("ELI18064", ipFPStream != NULL);
		if (ipFPStream->IsDirty() == S_OK)
		{
			return S_OK;
		}
	}

	// if we reached here, it means that the object is not dirty
	// indicate to the caller that this object is not dirty
	return S_FALSE;
}
//-------------------------------------------------------------------------------------------------
// Version 3:
//   Added persistence for error logging items and error handling items
// Version 4:
//	 Added persistence for skipped file processing
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
		if (nDataVersion > 1)
		{
			dataReader >> m_bKeepProcessingAsAdded;
		}

		// Logging of error details
		if (nDataVersion > 2)
		{
			dataReader >> m_bLogErrorDetails;

			if (m_bLogErrorDetails)
			{
				dataReader >> m_strErrorLogFile;
			}
		}

		// Skipped file processing data
		if (nDataVersion > 3)
		{
			// Read in the process skipped files data
			dataReader >> m_bProcessSkippedFiles;

			// Read in skipped for any user value
			dataReader >> m_bSkippedForAnyUser;
		}

		// Error handling task
		if (nDataVersion > 2)
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
		dataWriter << m_bProcessSkippedFiles;
		dataWriter << m_bSkippedForAnyUser;

		// Write these items to the byte stream
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), NULL );
		pStream->Write( data.getData(), nDataLength, NULL );

		// Save the error handling task
		IPersistStreamPtr ipErrorTaskObj = getErrorHandlingTask();
		ASSERT_RESOURCE_ALLOCATION( "ELI16055", ipErrorTaskObj != NULL );
		writeObjectToStream( ipErrorTaskObj, pStream, "ELI16056", fClearDirty );

		// Make sure the FileProcessingTasks vector has been created
		if ( m_ipFileProcessingTasks == NULL )
		{
			m_ipFileProcessingTasks.CreateInstance(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI14586", m_ipFileProcessingTasks != NULL );
		}
		
		// Save the File Processors
		IPersistStreamPtr ipFPObj = m_ipFileProcessingTasks;
		ASSERT_RESOURCE_ALLOCATION( "ELI14455", ipFPObj != NULL );
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
// Private methods
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileActionMgmtRolePtr CFileProcessingMgmtRole::getThisAsCOMPtr()
{
	UCLID_FILEPROCESSINGLib::IFileActionMgmtRolePtr ipThis(this);
	ASSERT_RESOURCE_ALLOCATION("ELI16976", ipThis != NULL);

	return ipThis;
}
//-------------------------------------------------------------------------------------------------
long CFileProcessingMgmtRole::getEnabledFileProcessingTasksCount(IIUnknownVectorPtr ipFileProcessingTasks) const
{
	long nCount = 0;

	// Iterate through the IIUnknownVector of IObjectWithDescription of IFileProcessingTask objects
	// and determine the count of enabled file processors
	if (ipFileProcessingTasks != NULL)
	{
		for (int i = 0; i < ipFileProcessingTasks->Size(); i++)
		{
			// Get the file processor at the current index
			IObjectWithDescriptionPtr ipObjWithDesc = ipFileProcessingTasks->At(i);

			// Increment the count if the object is defined and is enabled for use
			if (ipObjWithDesc->Enabled == VARIANT_TRUE && ipObjWithDesc->Object != NULL)
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
	ASSERT_ARGUMENT("ELI11107", pThreadData != NULL);

	// notify interested parties that the thread has started
	pThreadData->m_threadStartedEvent.signal();

	try
	{
		CoInitializeEx(NULL, COINIT_MULTITHREADED);
		CFileProcessingMgmtRole *pFPMgmtRole = pThreadData->m_pFPMgmtRole;
		ASSERT_RESOURCE_ALLOCATION("ELI11108", pFPMgmtRole != NULL);
		pFPMgmtRole->processFiles2(pThreadData);
		CoUninitialize();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11073")

	// notify interested parties that the thread has ended
	pThreadData->m_threadEndedEvent.signal();

	return 0;
}
//-------------------------------------------------------------------------------------------------
UINT CFileProcessingMgmtRole::fileProcessingThreadsWatcherThread(void *pData)
{
	try
	{
		CFileProcessingMgmtRole *pFPM = static_cast<CFileProcessingMgmtRole *>(pData);
		ASSERT_ARGUMENT("ELI13893", pFPM != NULL);
		ASSERT_ARGUMENT("ELI14348", pFPM->m_pRecordMgr != NULL);
		ASSERT_ARGUMENT("ELI14532", pFPM->m_ipRoleNotifyFAM != NULL );

		// This thread needs to be blocked until all thread data has been created
		// so it needs to wait until there is a semaphore available
		CSingleLock guard( &pFPM->m_threadDataSemaphore, TRUE );

		CoInitializeEx(NULL, COINIT_MULTITHREADED);
		
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
				ASSERT_RESOURCE_ALLOCATION("ELI17950", pThreadData != NULL);

				UCLID_FILEPROCESSINGLib::IFileProcessingTaskExecutorPtr ipExecutor = 
					pThreadData->m_ipTaskExecutor;
				ASSERT_RESOURCE_ALLOCATION("ELI17951", ipExecutor != NULL);

				ipExecutor->Close();
			}
		}
		CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14527");

		// Semaphore needs to be released so that the releaseProcessingThreadDataObjects will be
		// able to get the semaphore counts to delete the data
		guard.Unlock();

		// Release the memory for the Thread objects since they will not be needed
		// This should be done before the notification that processing is complete
		pFPM->releaseProcessingThreadDataObjects();

		// Set the processing flag to false
		pFPM->m_bProcessing = false;

		// Notify the FAM that processing is complete
		UCLID_FILEPROCESSINGLib::IRoleNotifyFAMPtr ipRoleNotifyFAM = pFPM->m_ipRoleNotifyFAM;
		ASSERT_RESOURCE_ALLOCATION("ELI25249", ipRoleNotifyFAM != NULL);
		ipRoleNotifyFAM->NotifyProcessingCompleted();

		CoUninitialize();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13892")

	return 0;
}
//-------------------------------------------------------------------------------------------------
UINT CFileProcessingMgmtRole::handleStopRequestAsynchronously(void *pData)
{
	try
	{
		CFileProcessingMgmtRole *pFPM = static_cast<CFileProcessingMgmtRole *>(pData);
		ASSERT_RESOURCE_ALLOCATION("ELI19422", pFPM != NULL);

		FPRecordManager* pRecordManager = pFPM->m_pRecordMgr;
		ASSERT_RESOURCE_ALLOCATION("ELI19431", pRecordManager != NULL);
		ASSERT_ARGUMENT("ELI19433", pFPM->m_ipRoleNotifyFAM != NULL );
		
		// Notify all of the processors of the stop request
		pFPM->notifyFileProcessingTasksOfStopRequest();
		
		// The user (or OEM application) wants to stop the processing of files as soon as possible.
		// Indicate to the record manager that the pending files in the queue are to be discarded
		pRecordManager->discardProcessingQueue();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16258");
	
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
			// If Paused this will be signaled so wait until it is not signaled
			m_eventResume.wait();

			if (!m_pRecordMgr->pop(task))
			{
				return;
			}

			// we now have a valid task that we need to process
			try
			{
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
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI17943", pThreadData != NULL);

			task.markAsStarted();
			m_pRecordMgr->updateTask(task);

			// Attempt executing the tasks on the current file and mark
			// the current file as either completed or pending.
			EFileProcessingResult eResult = startFileProcessingChain(task, pThreadData);
			if (eResult == kProcessingSuccessful)
			{
				task.markAsCompleted();
			}
			else if (eResult == kProcessingSkipped)
			{
				task.markAsSkipped();
			}
			else
			{
				Stop();
				task.markAsNone();
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI14340");
	}
	catch (UCLIDException& ue)
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

	// Update the task even if an exception was caught indicating task failure (P13 #4398)
	m_pRecordMgr->updateTask(task);
}
//-------------------------------------------------------------------------------------------------
EFileProcessingResult CFileProcessingMgmtRole::startFileProcessingChain(FileProcessingRecord& task, 
													   ProcessingThreadData* pThreadData)
{
	try
	{
		ASSERT_ARGUMENT("ELI17944", pThreadData != NULL);

		UCLID_FILEPROCESSINGLib::IFileProcessingTaskExecutorPtr ipExecutor = 
			pThreadData->m_ipTaskExecutor;
		ASSERT_RESOURCE_ALLOCATION("ELI17945", ipExecutor != NULL);

		// If m_bProcessing is false it means processing has been stopped.
		if (!m_bProcessing)
		{
			return kProcessingCancelled;
		}

		// Attempt to process the file
		EFileProcessingResult eResult = (EFileProcessingResult) ipExecutor->ProcessFile(
			task.getFileName().c_str(), task.getFileID(), task.getActionID(),
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

	// Run task in a separate try block so that an exception while logging does not
	// prevent the error task from executing
	try
	{
		try
		{	
			// Should error task be executed
			if (isErrorHandlingTaskEnabled())
			{
				ASSERT_ARGUMENT("ELI18007", pThreadData != NULL);

				UCLID_FILEPROCESSINGLib::IFileProcessingTaskExecutorPtr ipExecutor = 
					pThreadData->m_ipErrorTaskExecutor;
				ASSERT_RESOURCE_ALLOCATION("ELI18008", ipExecutor != NULL);

				task.notifyRunningErrorTask();

				// Create an error task "list" that contains the error task to run
				IIUnknownVectorPtr ipErrorTaskList(CLSID_IUnknownVector);
				ASSERT_RESOURCE_ALLOCATION("ELI17993", ipErrorTaskList != NULL);

				ipErrorTaskList->PushBack(getErrorHandlingTask());

				// Run the task
				UCLID_FILEPROCESSINGLib::EFileProcessingResult eResult = ipExecutor->InitProcessClose(
					get_bstr_t(task.getFileName()), ipErrorTaskList, task.getFileID(),
					task.getActionID(), getFPMDB(), getFAMTagManager(), 
					task.m_ipProgressStatus, VARIANT_FALSE);

				// Log a cancellation during error task execution
				if (eResult == kProcessingCancelled)
				{
					UCLIDException ue("ELI18060","Processing cancelled while executing error task!");
					ue.addDebugInfo("File", task.getFileName());
					ue.addDebugInfo("Task", asString(getErrorHandlingTask()->Description));
					ue.log();
				}

				task.notifyErrorTaskCompleted();
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
	if (m_ipFileProcessingTasks != NULL)
	{
		m_ipFileProcessingTasks->Clear();
		m_ipFileProcessingTasks = NULL;
	}

	// Reset the number of threads
	m_nNumThreads = 0;

	// Clear the continuous processing flags
	m_bKeepProcessingAsAdded = false;
	m_bOkToStopWhenQueueIsEmpty = false;

	// Clear the error log items
	m_bLogErrorDetails = false;
	m_strErrorLogFile.clear();

	// Clear the error task
	m_ipErrorTask = NULL;

	// Clear the checkbox for the Processing tabs and the dirty flag
	m_bEnabled = false;
	m_bDirty = false;

	m_bProcessSkippedFiles = false;
	m_bSkippedForAnyUser = false;
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
	ASSERT_RESOURCE_ALLOCATION("ELI14401", ipTagManager != NULL);
	return ipTagManager;
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
IIUnknownVectorPtr CFileProcessingMgmtRole::copyFileProcessingTasks(IIUnknownVectorPtr ipFileProcessingTasks)
{
	IIUnknownVectorPtr ipNewProcessors(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI11151", ipNewProcessors != NULL);

	long nSize = ipFileProcessingTasks->Size();
	int i;
	for(i = 0; i < nSize; i++)
	{
		// Retrieve the Object With Description
		IObjectWithDescriptionPtr ipObject(ipFileProcessingTasks->At(i));
		ASSERT_RESOURCE_ALLOCATION("ELI11148", ipObject != NULL );

		// Retrieve the associated file processor
		UCLID_FILEPROCESSINGLib::IFileProcessingTaskPtr ipFileProc(ipObject->Object);
		ASSERT_RESOURCE_ALLOCATION("ELI11149", ipFileProc != NULL );

		// Make separate copy of this file processor for the thread.
		// If the task cannot be run multithreaded, the task is required to 
		// control processing via mutexs as necessary to make it thread-safe
		ICopyableObjectPtr ipCopyObj = ipObject;
		ASSERT_RESOURCE_ALLOCATION("ELI11150", ipObject != NULL );
		ipNewProcessors->PushBack(ipCopyObj->Clone());
	}

	// Provide collection of file processors
	return ipNewProcessors;
}
//--------------------------------------------------------------------------------------------------
DWORD CFileProcessingMgmtRole::getActionID(const std::string & strAct)
{
	// Initialize action ID
	DWORD dwActionID = 0;

	try
	{
		return getFPMDB()->GetActionID(strAct.c_str());
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14934");
	
	return dwActionID;
}
//-------------------------------------------------------------------------------------------------
IObjectWithDescriptionPtr CFileProcessingMgmtRole::getErrorHandlingTask()
{
	// Make sure the Error Task ObjectWithDescription has been created
	if (m_ipErrorTask == NULL)
	{
		m_ipErrorTask.CreateInstance(CLSID_ObjectWithDescription);
		ASSERT_RESOURCE_ALLOCATION("ELI16106", m_ipErrorTask != NULL );

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
	if (getErrorHandlingTask()->Object != NULL)
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
	// Need to obtain a semaphore since this method uses th em_vecProcessingThreadData vector
	CSingleLock lock(&m_threadDataSemaphore, TRUE);

	for (unsigned int i = 0; i < m_vecProcessingThreadData.size(); i++)
	{
		// Need to log any exceptions for stopping a processor but finish stopping the rest of them
		try
		{
			// Get the file processor that is currently running
			ProcessingThreadData* pThreadData = m_vecProcessingThreadData[i];
			ASSERT_RESOURCE_ALLOCATION("ELI17946", pThreadData != NULL);

			UCLID_FILEPROCESSINGLib::IFileProcessingTaskExecutorPtr ipTaskExecutor = 
				pThreadData->m_ipTaskExecutor;
			ASSERT_RESOURCE_ALLOCATION("ELI17947", ipTaskExecutor != NULL);

			UCLID_FILEPROCESSINGLib::IFileProcessingTaskExecutorPtr ipErrorExecutor = 
				pThreadData->m_ipErrorTaskExecutor;
			ASSERT_RESOURCE_ALLOCATION("ELI18009", ipErrorExecutor != NULL);

			ipTaskExecutor->Cancel();
			ipErrorExecutor->Cancel();
		}
		CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16209")
	}
}
//-------------------------------------------------------------------------------------------------
