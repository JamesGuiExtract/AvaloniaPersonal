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
#include <ScheduleGrid.h>
#include <ValueRestorer.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 5;

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
: m_pRecordMgr(NULL),
  m_ipRoleNotifyFAM(NULL),
  m_threadDataSemaphore(2,2),
  m_bProcessing(false),
  m_bProcessingSingleFile(false)
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
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// check pre-conditions
		ASSERT_ARGUMENT("ELI14296", m_bEnabled == true);
		ASSERT_ARGUMENT("ELI14301", m_ipFileProcessingTasks != NULL);
		ASSERT_ARGUMENT("ELI14302", m_ipFileProcessingTasks->Size() > 0);
		ASSERT_ARGUMENT("ELI14344", m_pRecordMgr != NULL);

		// Before starting the processManager thread, make sure it is not already started
		if (m_eventProcessManagerStarted.isSignaled() && !m_eventProcessManagerExited.isSignaled())
		{
			UCLIDException ue("ELI28320", "Manager thread is currently running.");
			throw ue;
		}

		// Set the File Action manager pointer
		m_ipRoleNotifyFAM = pRoleNotifyFAM;
		ASSERT_RESOURCE_ALLOCATION("ELI14531", m_ipRoleNotifyFAM != NULL );

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
		m_eventWatcherThreadExited.reset();
		m_eventPause.reset();
		m_eventResume.reset();

		// Initialice the current running state to normal stop.
		m_eCurrentRunningState = kNormalStop;

		// start the processing thread
		AfxBeginThread(processManager, this);	
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

		// Cause processing to wait until this m_eventPause is signaled
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
// Version 5:
//	 Added persistence for the processing schedule
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

		// Load Schedule data
		if (nDataVersion > 4)
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
STDMETHODIMP CFileProcessingMgmtRole::get_ProcessingSchedule(IVariantVector** ppHoursSchedule)
{	
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI28157", ppHoursSchedule != NULL);

		// Create the return variant vector
		IVariantVectorPtr ipHours(CLSID_VariantVector);
		ASSERT_ARGUMENT("ELI28159", ipHours != NULL);

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
		ASSERT_ARGUMENT("ELI28160", ipHoursSchedule != NULL);

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
		ASSERT_ARGUMENT("ELI28177", pbVal != NULL);

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
			throw new UCLIDException("ELI29554", "Cannot process single file when processing "
				"is already in progress!");
		}

		UCLID_FILEPROCESSINGLib::IFileRecordPtr ipFileRecord(pFileRecord);
		ASSERT_ARGUMENT("ELI29536", ipFileRecord != NULL);

		m_pDB = pFPDB;
		ASSERT_ARGUMENT("ELI29552", m_pDB != NULL);

		m_pFAMTagManager = pFAMTagManager;
		ASSERT_ARGUMENT("ELI29553", m_pFAMTagManager != NULL);

		// Ensure m_bProcessingSingleFile will be reset to false.
		ValueRestorer<volatile bool> restorer(m_bProcessingSingleFile, false);

		m_bProcessingSingleFile = true;

		// Register this processing FAM for auto revert
		getFPMDB()->RegisterProcessingFAM();

		// Get the current action status-- allow an attempt to auto-revert locked files if the file
		// is in the processing state.
		string strActionName = getFPMDB()->GetActionName(ipFileRecord->ActionID);
		UCLID_FILEPROCESSINGLib::EActionStatus easCurrent =
			getFPMDB()->GetFileStatus(ipFileRecord->FileID, strActionName.c_str(), VARIANT_TRUE);

		// If file is not in the correct state to process (depending on the skipped file
		// setting), throw an exception.
		while ((m_bProcessSkippedFiles && easCurrent != UCLID_FILEPROCESSINGLib::kActionSkipped) ||
			   (!m_bProcessSkippedFiles && easCurrent != UCLID_FILEPROCESSINGLib::kActionPending))
		{
			UCLIDException ue("ELI29545", string("The file cannot be processed because it ") +
				"is not currently " + (m_bProcessSkippedFiles ? "skipped!" : "pending!"));
			ue.addDebugInfo("Current Status", asString(getFPMDB()->AsStatusString(easCurrent)));
			throw ue;
		}

		// Set action ID to the record manager
		m_pRecordMgr->setActionID(ipFileRecord->ActionID);

		// Clear the record manager to ensure no files except the one specified will be processed.
		m_pRecordMgr->clear(false);

		// Use the configured skipped file settings.
		m_pRecordMgr->setProcessSkippedFiles(m_bProcessSkippedFiles);
		m_pRecordMgr->setSkippedForCurrentUser(!m_bSkippedForAnyUser);

		// Do not keep processing regardless of the configured setting.
		m_pRecordMgr->setKeepProcessingAsAdded(false);

		// Create and initialize a processing thread data struct needed by processTask.
		ProcessingThreadData threadData;
		threadData.m_pFPMgmtRole = this;

		// Initialize the task executor
		UCLID_FILEPROCESSINGLib::IFileProcessingTaskExecutorPtr ipExecutor = 
			threadData.m_ipTaskExecutor;
		ASSERT_RESOURCE_ALLOCATION("ELI29555", ipExecutor != NULL);
		ipExecutor->Init(m_ipFileProcessingTasks, m_pRecordMgr->getActionID(), getFPMDB(),
			getFAMTagManager());

		// Create a FileProcessingRecord for the file and add it to the record manager's queue.
		getFPMDB()->SetFileStatusToProcessing(ipFileRecord->FileID, ipFileRecord->ActionID);
		FileProcessingRecord task(ipFileRecord);

		// Though the record manager is in a sense unnecessary when processing a single file,
		// because of the close way it is tied to processing, exceptions will be raised if the file
		// does not follow the same code-path through push and pop prior to processing.
		m_pRecordMgr->push(task);
		m_pRecordMgr->pop(task);

		// Process the file
		processTask(task, &threadData);

		ipExecutor->Close();

		getFPMDB()->UnregisterProcessingFAM();

		m_bProcessingSingleFile = false;

		// Exceptions that occured while processing a file in processTask will not be thrown out.
		// Throw the exception here if necessary.
		if (!task.m_strException.empty())
		{
			UCLIDException ue;
			ue.createFromString("ELI29557", task.m_strException);
			throw ue;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29533");
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
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI11073")

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

				// Unregister Processing FAM to reset file back to previous state if any remaining
				if (pFPM->m_bProcessing)
				{
					pFPM->getFPMDB()->UnregisterProcessingFAM();
				}

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
		if (!m_bProcessing && !m_bProcessingSingleFile)
		{
			return kProcessingCancelled;
		}

		// Attempt to process the file
		EFileProcessingResult eResult = (EFileProcessingResult) ipExecutor->ProcessFile(
			task.getFileName().c_str(), task.getFileID(), m_pRecordMgr->getActionID(),
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

	// Reset the action name
	m_strAction = "";

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

	// Reset the Processing schedule info
	m_bLimitProcessingToSchedule = false;
	m_vecScheduledHours.clear();

	// Default the schedule to all on
	m_vecScheduledHours.resize(giNUMBER_OF_HOURS_IN_WEEK, true);
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
DWORD CFileProcessingMgmtRole::getActionID(const string & strAct)
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
UINT CFileProcessingMgmtRole::processManager(void *pData)
{
	try
	{
		// Cast the argument to FileProcessingMgmtRole
		CFileProcessingMgmtRole *pFPM = static_cast<CFileProcessingMgmtRole *>(pData);

		// Validate that the FPM is setup properly
		ASSERT_RESOURCE_ALLOCATION("ELI28202", pFPM != NULL);
		ASSERT_RESOURCE_ALLOCATION("ELI28203", pFPM->m_pRecordMgr != NULL);
		ASSERT_RESOURCE_ALLOCATION("ELI28204", pFPM->m_ipRoleNotifyFAM != NULL );

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
			
			ERunningState eNextRunningState;
			
			pFPM->timeTillNextProcessingChange(eNextRunningState);

			bool bExit = false;

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
				CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI28507")

				// Post Schedule Inactive messag to the UI
				if ( eNextRunningState == kScheduleStop && pFPM->m_hWndOfUI != NULL)
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
					// retrurned by the timeTillNextProcessingChange function
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
					// Reset the Pause and resume events
					pFPM->m_eventPause.reset();
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

						// Wait for the watcher thread
						pFPM->m_eventWatcherThreadExited.wait();

						// Reset watcher thread exited event
						pFPM->m_eventWatcherThreadExited.reset();

						// Notify UI that processing is inactive if not a normal stop
						if (pFPM->m_eCurrentRunningState != kNormalStop && pFPM->m_hWndOfUI != NULL)
						{
							::PostMessage( pFPM->m_hWndOfUI, FP_SCHEDULE_INACTIVE, 0, 0);
						}
					}
					break;
				case kScheduleRun:
				case kNormalRun:
					// Start processing
					if (ePreviousState == kPaused || ePreviousState == kScheduleStop)
					{
						pFPM->startProcessing();

						// Notify UI that processing is running
						if (pFPM->m_hWndOfUI != NULL)
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
			if (pFPM->m_hWndOfUI != NULL)
			{
				::PostMessage( pFPM->m_hWndOfUI, FP_SCHEDULE_ACTIVE, 0, 0);
			}

			// Notify the FAM that processing is complete
			UCLID_FILEPROCESSINGLib::IRoleNotifyFAMPtr ipRoleNotifyFAM = pFPM->m_ipRoleNotifyFAM;
			ASSERT_RESOURCE_ALLOCATION("ELI28315", ipRoleNotifyFAM != NULL);
			ipRoleNotifyFAM->NotifyProcessingCompleted();

			pFPM->m_eventProcessManagerExited.signal();
		}
		catch(...)
		{
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
		throw new UCLIDException("ELI29556", "Cannot start processing; currently processing a file "
			"independently!");
	}

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
			m_eventWatcherThreadExited.reset();

			// If not starting off the threads don't start the thread watcher.
			if (!bDontStartThreads)
			{
				// Kick off the thread which will wait for all file processing threads
				// to complete and then update the UI
				// By putting this kick-off this early in this method, we can benefit from 
				// the fact that all the sitations related to processing ending
				// (due to errors or due to successful processing) can be handled in one
				// place (i.e. this thread function).
				AfxBeginThread(fileProcessingThreadsWatcherThread, this);
			}

			// Set action ID to the record manager
			m_pRecordMgr->setActionID(getActionID(m_strAction));

			// Signal Not Paused event so that processing will continue
			m_eventPause.reset();

			// if there is a dialog set it to receive status updates
			if(m_hWndOfUI)
			{
				m_pRecordMgr->setDlg(m_hWndOfUI);
			}

			// clear all the records in the file processing record manager
			// (i.e. clear the queue of files to process)
			m_pRecordMgr->clear(m_eCurrentRunningState != kScheduleRun);

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

			if (!bDontStartThreads)
			{
				// Create all the file processors for the threads and 
				// notify them that processing is about to begin
				for (int i = 0; i < nNumThreads; i++)
				{
					// Create a thread data struct and add it to the vector
					ProcessingThreadData* pThreadData = new ProcessingThreadData();
					ASSERT_RESOURCE_ALLOCATION("ELI17948", pThreadData != NULL);
					m_vecProcessingThreadData.push_back(pThreadData);

					// Update the thread data members
					pThreadData->m_pFPMgmtRole = this;

					// Initialize executor with a new copy of the file processors for the thread
					UCLID_FILEPROCESSINGLib::IFileProcessingTaskExecutorPtr ipExecutor = 
						pThreadData->m_ipTaskExecutor;
					ASSERT_RESOURCE_ALLOCATION("ELI17949", ipExecutor != NULL);
					ipExecutor->Init(copyFileProcessingTasks(m_ipFileProcessingTasks),
						m_pRecordMgr->getActionID(), getFPMDB(), getFAMTagManager());
				}
			}

			// start the processing
			m_bProcessing = true;

			// Register this processing FAM for auto revert
			m_pDB->RegisterProcessingFAM();
			
			if (!bDontStartThreads)
			{
				// begin the threads to process files in parallel
				for (int i = 0; i < nNumThreads; i++)
				{
					ProcessingThreadData* pThreadData = m_vecProcessingThreadData[i];

					// begin the thread
					pThreadData->m_pThread = AfxBeginThread(fileProcessingThreadProc, pThreadData);
					ASSERT_RESOURCE_ALLOCATION("ELI11075", pThreadData->m_pThread != NULL);

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
		ASSERT_ARGUMENT("ELI28208", m_ipFileProcessingTasks != NULL);
		ASSERT_ARGUMENT("ELI28209", m_ipFileProcessingTasks->Size() > 0);
		ASSERT_ARGUMENT("ELI28210", m_pRecordMgr != NULL);

		// Notify the FAM that processing is cancelling
		if (m_eCurrentRunningState == kNormalStop)
		{
			m_ipRoleNotifyFAM->NotifyProcessingCancelling();
		}

		// The Processing needs to be stopped asynchronously
		// to keep the UI from being blocked
		AfxBeginThread(handleStopRequestAsynchronously, this);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28211");
}
//-------------------------------------------------------------------------------------------------
unsigned long CFileProcessingMgmtRole::timeTillNextProcessingChange( ERunningState &eNextRunningState )
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
