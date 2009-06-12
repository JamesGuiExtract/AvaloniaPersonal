// FileProcessingTaskExecutor.cpp : Implementation of CFileProcessingTaskExecutor

#include "stdafx.h"
#include "FileProcessingTaskExecutor.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>
#include <ValueRestorer.h>

//--------------------------------------------------------------------------------------------------
// CFileProcessingTaskExecutor
//--------------------------------------------------------------------------------------------------
CFileProcessingTaskExecutor::CFileProcessingTaskExecutor() :
	m_ipFileProcessingTasks(NULL),
	m_ipCurrentTask(NULL),
	m_bInitialized(false),
	m_ipDB(NULL),
	m_ipFAMTagManager(NULL),
	m_ipMiscUtils(NULL)
{
}
//--------------------------------------------------------------------------------------------------
CFileProcessingTaskExecutor::~CFileProcessingTaskExecutor()
{
	try
	{
		// Release member interface pointers (just a precaution)
		m_ipFileProcessingTasks = NULL;
		m_ipCurrentTask = NULL;
		m_ipDB = NULL;
		m_ipFAMTagManager = NULL;
		m_ipMiscUtils = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI17687");
}
//--------------------------------------------------------------------------------------------------
HRESULT CFileProcessingTaskExecutor::FinalConstruct()
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingTaskExecutor::FinalRelease()
{
}

//--------------------------------------------------------------------------------------------------
// IFileProcessingTaskExecutor
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingTaskExecutor::Init(IIUnknownVector *pFileProcessingTasks, IFileProcessingDB *pDB, 
	IFAMTagManager *pFAMTagManager)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		CSingleLock lg(&m_mutex,TRUE);

		// Check license
		validateLicense();

		// Ensure we are not already initialized
		if (m_bInitialized)
		{
			UCLIDException ue("ELI17879", "Internal error: Task executor has already been initialized!");
			throw ue;
		}

		// Clear any previous cancel request
		m_eventCancelRequested.reset();

		m_ipFileProcessingTasks = pFileProcessingTasks;
		ASSERT_ARGUMENT("ELI17698", m_ipFileProcessingTasks != NULL);

		m_ipDB = pDB;
		ASSERT_ARGUMENT("ELI17700", m_ipDB != NULL);

		m_ipFAMTagManager = pFAMTagManager;
		ASSERT_ARGUMENT("ELI17702", m_ipFAMTagManager != NULL);

		int nTaskCount = m_ipFileProcessingTasks->Size();

		// Initialize each enabled FileProcessingTask
		for (int i = 0; i < nTaskCount ; i++)
		{
			// Retrieve specified file processor Object With Description
			IObjectWithDescriptionPtr ipObject(m_ipFileProcessingTasks->At(i));
			ASSERT_RESOURCE_ALLOCATION("ELI17848", ipObject != NULL);

			// Retrieve this file processor
			UCLID_FILEPROCESSINGLib::IFileProcessingTaskPtr ipFileProc(ipObject->Object);
			ASSERT_RESOURCE_ALLOCATION("ELI17849", ipFileProc != NULL);

			// Only initialize the processing task if it is enabled
			if (asCppBool(ipObject->Enabled))
			{
				ipFileProc->Init();
			}
		}

		m_bInitialized = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17844");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingTaskExecutor::ProcessFile(BSTR bstrSourceDocName,  
	IProgressStatus *pProgressStatus, VARIANT_BOOL vbCancelRequested, 
	VARIANT_BOOL *pbSuccessfulCompletion)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		CSingleLock lg(&m_mutex,TRUE);

		// Check license
		validateLicense();

		verifyInitialization();

		// Assert required arguments
		ASSERT_ARGUMENT("ELI17706", bstrSourceDocName != NULL);
		ASSERT_ARGUMENT("ELI17877", !asString(bstrSourceDocName).empty());
		ASSERT_ARGUMENT("ELI17704", pbSuccessfulCompletion != NULL);

		// Initialize to true... set false once we encounter a file processing task that doesn't complete
		*pbSuccessfulCompletion = VARIANT_TRUE;

		// ProgressStatus not required... don't use if NULL
		IProgressStatusPtr ipProgressStatus(pProgressStatus);

		if (ipProgressStatus)
		{
			// Initialize the progress bar appropriately
			int nEnabledTaskCount = getMiscUtils()->CountEnabledObjectsIn(m_ipFileProcessingTasks);
			ipProgressStatus->InitProgressStatus("Initializing tasks...", 0, nEnabledTaskCount, VARIANT_TRUE);
		}

		int nTaskCount = m_ipFileProcessingTasks->Size();

		// Exercise each File processor
		for (int nCurrentTask = 0; nCurrentTask < nTaskCount; nCurrentTask++)
		{
			// Retrieve specified file processor Object With Description
			IObjectWithDescriptionPtr ipObject(m_ipFileProcessingTasks->At(nCurrentTask));
			ASSERT_RESOURCE_ALLOCATION("ELI17691", ipObject != NULL);

			// Only process the file if the task is enabled
			if (asCppBool(ipObject->Enabled))
			{
				// Get the task number and task description
				string strCurrentTaskName = "task #";
				strCurrentTaskName += asString(nCurrentTask + 1);
				strCurrentTaskName += " (";
				strCurrentTaskName += asString(ipObject->Description);
				strCurrentTaskName += ")";

				try
				{
					try
					{
						// Update the text associated with the ProgressStatus to use 
						// the current task's description
						if (ipProgressStatus)
						{
							string strStatusMessage = "Executing " + strCurrentTaskName;
							ipProgressStatus->StartNextItemGroup(strStatusMessage.c_str(), 1);
						}

						// Create a pointer to the Sub-ProgressStatus object, depending 
						// upon whether the caller requested progress information
						IProgressStatusPtr ipSubProgressStatus = (ipProgressStatus == NULL) ? 
							NULL : ipProgressStatus->SubProgressStatus;

						// Ensure that in the case of an error that m_ipCurrentTask will be restored to NULL
						ValueRestorer<UCLID_FILEPROCESSINGLib::IFileProcessingTaskPtr> restorer(m_ipCurrentTask, NULL);

						// Retrieve this file processor; use separate scope to limit m_mutexCurrentTask lock to this call
						{
							// Lock access to m_ipCurrentTask to ensure it can't be read/written at the same time
							CSingleLock lock(&m_mutexCurrentTask, TRUE);
							m_ipCurrentTask = ipObject->Object;
							ASSERT_RESOURCE_ALLOCATION("ELI17692", m_ipCurrentTask != NULL);
						}

						// Was cancel request either passed in or received via a call to Cancel?
						bool bCancelRequested = (asCppBool(vbCancelRequested) || m_eventCancelRequested.isSignaled());

						VARIANT_BOOL vbSuccessfulCompletion = m_ipCurrentTask->ProcessFile(
								bstrSourceDocName, 
								(UCLID_FILEPROCESSINGLib::IFAMTagManager*) m_ipFAMTagManager, 
								(UCLID_FILEPROCESSINGLib::IFileProcessingDB*) m_ipDB,
								ipSubProgressStatus, asVariantBool(bCancelRequested));

						// Task is no longer running; indicate such
						{
							// Lock access to m_ipCurrentTask to ensure it can't be read/written at the same time
							CSingleLock lock(&m_mutexCurrentTask, TRUE);
							m_ipCurrentTask = NULL;
						}

						// Check success flag
						if (!asCppBool(vbSuccessfulCompletion))
						{
							// Processing didn't complete.
							// Log the fact that processing was cancelled
							string strMsg = "Processing cancelled while performing ";
							if (strCurrentTaskName.empty())
							{
								// Use the task # as part of the error string.
								strMsg += "task #";
								strMsg += asString(nCurrentTask + 1); // use 1-based index for the user
							}
							else
							{
								// Use the task name as part of the error string.
								strMsg += strCurrentTaskName;
							}
							strMsg += "!";

							// Add the history record and debug information, then log the exception
							UCLIDException ue("ELI17862",strMsg);
							ue.addDebugInfo("File", asString(bstrSourceDocName));
							ue.addDebugInfo("Task", strCurrentTaskName);
							ue.log();

							// Return false to indicate that the file was not processed.
							*pbSuccessfulCompletion = VARIANT_FALSE;
							
							return S_OK;
						}
					}
					CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI17694");
				}
				catch ( UCLIDException &ue )
				{
					// Rethrow the exception as com error
					string strMsg = "Unable to execute ";
					if (strCurrentTaskName.empty())
					{
						// Use the task # as part of the error string.
						strMsg += "task #";
						strMsg += asString(nCurrentTask + 1); // use 1-based index for the user
					}
					else
					{
						// Use the task name as part of the error string.
						strMsg += strCurrentTaskName;
					}
					strMsg += "!";

					// Add the history record and debug information before rethrowing the exception.
					UCLIDException uexOuter("ELI17697",strMsg, ue);
					uexOuter.addDebugInfo("File", asString(bstrSourceDocName));
					uexOuter.addDebugInfo("Task", strCurrentTaskName);

					throw uexOuter;
				}
			}
		}
		
		// Completed task list successfully; return true
		*pbSuccessfulCompletion = VARIANT_TRUE;

		// Update progress bar to indicate completion
		if (ipProgressStatus)
		{
			ipProgressStatus->CompleteCurrentItemGroup();
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17689");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingTaskExecutor::InitProcessClose(BSTR bstrSourceDocName, 
	IIUnknownVector *pFileProcessingTasks, IFileProcessingDB *pDB, IFAMTagManager *pFAMTagManager, 
	IProgressStatus *pProgressStatus, VARIANT_BOOL bCancelRequested, 
	VARIANT_BOOL *pbSuccessfulCompletion)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		CSingleLock lg(&m_mutex,TRUE);

		// Check license
		validateLicense();

		// Verify required arguments
		ASSERT_ARGUMENT("ELI17863", pFileProcessingTasks != NULL);
		ASSERT_ARGUMENT("ELI17864", pDB != NULL);
		ASSERT_ARGUMENT("ELI17865", pFAMTagManager != NULL);
		ASSERT_ARGUMENT("ELI17866", pbSuccessfulCompletion != NULL);

		// ProgressStatus not required.  Won't be used if NULL
		IProgressStatusPtr ipProgressStatus(pProgressStatus);

		// Initialize the processing tasks
		getThisAsCOMPtr()->Init(pFileProcessingTasks, (UCLID_FILEPROCESSINGLib::IFileProcessingDB *) pDB, 
			(UCLID_FILEPROCESSINGLib::IFAMTagManager *) pFAMTagManager);
		
		// Process the tasks
		try
		{
			*pbSuccessfulCompletion = getThisAsCOMPtr()->ProcessFile(bstrSourceDocName, 
				ipProgressStatus, bCancelRequested);
		}
		catch (...)
		{
			// Guarantee that Close is called if Init has been called
			getThisAsCOMPtr()->Close();
			throw;
		}

		// Close the processing tasks
		getThisAsCOMPtr()->Close();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19447");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingTaskExecutor::Cancel()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		m_eventCancelRequested.signal();

		// Call Cancel for the currently executing task (if it exists)
		UCLID_FILEPROCESSINGLib::IFileProcessingTaskPtr ipCurrentTask = getThisAsCOMPtr()->GetCurrentTask();
		
		// If there is no current task, no need to call cancel (and not an exception condition)
		if (ipCurrentTask)
		{
			ipCurrentTask->Cancel();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17847");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingTaskExecutor::Close()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		CSingleLock lg(&m_mutex,TRUE);

		// Check license
		validateLicense();

		verifyInitialization();

		// Set initialized flag to false immediately; cannot guarantee initialization beyond this point
		m_bInitialized = false;

		int nTaskCount = m_ipFileProcessingTasks->Size();

		// Close each FileProcessingTask
		for (int i = 0; i < nTaskCount ; i++)
		{
			// Retrieve specified file processor Object With Description
			IObjectWithDescriptionPtr ipObject(m_ipFileProcessingTasks->At(i));
			ASSERT_RESOURCE_ALLOCATION("ELI19449", ipObject != NULL);

			// Retrieve this file processor
			UCLID_FILEPROCESSINGLib::IFileProcessingTaskPtr ipFileProc(ipObject->Object);
			ASSERT_RESOURCE_ALLOCATION("ELI19450", ipFileProc != NULL);

			// Only close the processing task if it is enabled
			if (asCppBool(ipObject->Enabled))
			{
				ipFileProc->Close();
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19448");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingTaskExecutor::GetCurrentTask(IFileProcessingTask **ppCurrentTask)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI17859", ppCurrentTask != NULL);

		// Lock access to m_ipCurrentTask to ensure it can't be read/written at the same time
		CSingleLock lock(&m_mutexCurrentTask, TRUE);

		// Provide the currently executing task (if there is one)
		if (m_ipCurrentTask)
		{
			UCLID_FILEPROCESSINGLib::IFileProcessingTaskPtr ipShallowCopy = m_ipCurrentTask;
			*ppCurrentTask = (IFileProcessingTask *) ipShallowCopy.Detach();
		}
		else
		{
			*ppCurrentTask = NULL;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17716");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingTaskExecutor::get_IsInitialized(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		// ensure and initialize the return value
		ASSERT_ARGUMENT("ELI17898", pVal != NULL);
		
		*pVal = asVariantBool(m_bInitialized);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17899");

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingTaskExecutor::InterfaceSupportsErrorInfo(REFIID riid)
{
	try
	{
		static const IID* arr[] = 
		{
			&IID_IFileProcessingTaskExecutor,
			&IID_ILicensedComponent
		};

		for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
		{
			if (InlineIsEqualGUID(*arr[i],riid))
				return S_OK;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17688")

	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingTaskExecutor::raw_IsLicensed(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17875", pbValue != NULL);

		try
		{
			// check the license
			validateLicense();

			// If no exception, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17876");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CFileProcessingTaskExecutor::verifyInitialization()
{
	// Verify the task array is valid
	if (!m_ipFileProcessingTasks)
	{
		UCLIDException ue("ELI17856", "Internal error: Task executor has invalid task list!");
		throw ue;
	}

	// Verify the tasks have been initialized
	if (m_bInitialized == false)
	{
		UCLIDException ue("ELI17857", "Internal error: Processing tasks have not been initialized!");
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
IMiscUtilsPtr CFileProcessingTaskExecutor::getMiscUtils()
{
	if (m_ipMiscUtils == NULL)
	{
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI16570", m_ipMiscUtils != NULL );
	}
	
	return m_ipMiscUtils;
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileProcessingTaskExecutorPtr CFileProcessingTaskExecutor::getThisAsCOMPtr()
{
	UCLID_FILEPROCESSINGLib::IFileProcessingTaskExecutorPtr ipThis = this;
	ASSERT_RESOURCE_ALLOCATION("ELI17851", ipThis != NULL);
	return ipThis;
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingTaskExecutor::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI17690", "File Processing Task Executor");
}
//-------------------------------------------------------------------------------------------------

