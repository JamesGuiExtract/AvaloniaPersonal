// FileProcessingTaskExecutor.cpp : Implementation of CFileProcessingTaskExecutor

#include "stdafx.h"
#include "FileProcessingTaskExecutor.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>
#include <ValueRestorer.h>
#include <Win32Semaphore.h>
#include <UPI.h>

//-------------------------------------------------------------------------------------------------
// StandbyThread
//-------------------------------------------------------------------------------------------------
CFileProcessingTaskExecutor::StandbyThread::StandbyThread(Win32Event& eventCancelProcessing,
	UCLID_FILEPROCESSINGLib::IFileProcessingTaskPtr ipFileProcessingTask)
: m_eventCancelProcessing(eventCancelProcessing)
, m_ipFileProcessingTask(ipFileProcessingTask)
{
	try
	{
		ASSERT_RESOURCE_ALLOCATION("ELI33938", m_ipFileProcessingTask != __nullptr);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33939");
}
//-------------------------------------------------------------------------------------------------
CFileProcessingTaskExecutor::StandbyThread::~StandbyThread()
{
	try
	{
		m_ipFileProcessingTask = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI33942")
}
//-------------------------------------------------------------------------------------------------
BOOL CFileProcessingTaskExecutor::StandbyThread::InitInstance()
{
	// Return TRUE so Run is called.
	return TRUE;
}
//-------------------------------------------------------------------------------------------------
int CFileProcessingTaskExecutor::StandbyThread::Run()
{
	try
	{
		// The Standby call may block if this is a cancellable task.
		if (!asCppBool(m_ipFileProcessingTask->Standby()) && !m_eventStandbyEnding.isSignaled())
		{
			m_eventCancelProcessing.signal();
		}

		// This thread needs to remain alive until Standby has ended so that endStandby may be
		// called.
		m_eventStandbyEnding.wait();

		// Notify endStandby that the thread is done.
		m_eventStandbyEnded.signal();

		// Ensure endStandby has gotten past m_eventStandbyEnded.wait() before going out of scope.
		m_eventEndStandbyEnded.wait();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI33943")

	return 0;
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingTaskExecutor::StandbyThread::endStandby()
{
	try
	{
		m_eventStandbyEnding.signal();

		// [LegacyRCAndUtils:6211]
		// The existence of m_eventCancelProcessing cannot be guaranteed after returning from here.
		// To prevent a race condition in which m_eventCancelProcessing goes out of scope in the
		// calling function before m_eventCancelProcessing.signal() is called, ensure the standby
		// thread is done processing before returning from this call.
		m_eventStandbyEnded.wait();

		// Let main thread know it can now exit.
		m_eventEndStandbyEnded.signal();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI33944")
}

//--------------------------------------------------------------------------------------------------
// CFileProcessingTaskExecutor
//--------------------------------------------------------------------------------------------------
CFileProcessingTaskExecutor::CFileProcessingTaskExecutor() :
	m_ipCurrentTask(__nullptr),
	m_bInitialized(false),
	m_ipDB(__nullptr),
	m_ipFAMTagManager(__nullptr),
	m_ipFileRequestHandler(__nullptr),
	m_eventStandbyRunning(false)
{
}
//--------------------------------------------------------------------------------------------------
CFileProcessingTaskExecutor::~CFileProcessingTaskExecutor()
{
	try
	{
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
	try
	{
		// Release member interface pointers (just a precaution)
		m_vecProcessingTasks.clear(); // This vector holds interface pointers
		m_ipCurrentTask = __nullptr;
		m_ipDB = __nullptr;
		m_ipFAMTagManager = __nullptr;
		m_ipFileRequestHandler = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI26736");
}

//--------------------------------------------------------------------------------------------------
// IFileProcessingTaskExecutor
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingTaskExecutor::Init(IIUnknownVector *pFileProcessingTasks, long nActionID,
	IFileProcessingDB *pDB, IFAMTagManager *pFAMTagManager, IFileRequestHandler* pFileRequestHandler)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		CSingleLock lg(&m_mutex,TRUE);

		// Check license
		validateLicense();

		IIUnknownVectorPtr ipFileProcessingTasks(pFileProcessingTasks);
		ASSERT_ARGUMENT("ELI26780", ipFileProcessingTasks != __nullptr);

		// Database is allowed to be NULL since this may be running from 
		// RunFPSFile with /ignoreDB
		// https://extract.atlassian.net/browse/ISSUE-12746
		UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipDB(pDB);

		UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipFAMTagManager(pFAMTagManager);
		ASSERT_ARGUMENT("ELI26782", ipFAMTagManager != __nullptr);

		// FileRequestHandler is allowed to be null since this may be running 
		// from RunFPSFile with /ignoreDB
		// https://extract.atlassian.net/browse/ISSUE-12746
		UCLID_FILEPROCESSINGLib::IFileRequestHandlerPtr ipFileRequestHandler(pFileRequestHandler);

		// Call the init method
		init(ipFileProcessingTasks, nActionID, ipDB, ipFAMTagManager, ipFileRequestHandler);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17844");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingTaskExecutor::ProcessFile(IFileRecord* pFileRecord,
	long nActionID, IProgressStatus *pProgressStatus, VARIANT_BOOL vbCancelRequested, 
	EFileProcessingResult *pResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		CSingleLock lg(&m_mutex,TRUE);

		// Check license
		validateLicense();

		verifyInitialization();

		// Assert required arguments
		ASSERT_ARGUMENT("ELI31323", pFileRecord != __nullptr);
		ASSERT_ARGUMENT("ELI17704", pResult != __nullptr);

		// ProgressStatus not required... don't use if NULL
		IProgressStatusPtr ipProgressStatus(pProgressStatus);

		// Process the file and set the return value
		*pResult = processFile(pFileRecord, nActionID, ipProgressStatus,
			asCppBool(vbCancelRequested));

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17689");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingTaskExecutor::InitProcessClose(IFileRecord* pFileRecord, 
	IIUnknownVector *pFileProcessingTasks, long nActionID, IFileProcessingDB *pDB,
	IFAMTagManager *pFAMTagManager, IFileRequestHandler* pFileRequestHandler,
	IProgressStatus *pProgressStatus, VARIANT_BOOL bCancelRequested, 
	EFileProcessingResult *pResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		CSingleLock lg(&m_mutex,TRUE);

		// Check license
		validateLicense();

		// Verify required arguments
		IIUnknownVectorPtr ipFileProcessingTasks(pFileProcessingTasks);
		ASSERT_ARGUMENT("ELI17863", ipFileProcessingTasks !=  __nullptr);
		UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipFAMTagManager(pFAMTagManager);
		ASSERT_ARGUMENT("ELI17865", ipFAMTagManager !=  __nullptr);
		UCLID_FILEPROCESSINGLib::IFileRequestHandlerPtr ipFileRequestHandler(pFileRequestHandler);
		// https://extract.atlassian.net/browse/ISSUE-12509
		// pFileRequestHandler is not currently guaranteed to be available in all execution contexts.
		// The task is responsible for asserting its existence if needed.
		ASSERT_ARGUMENT("ELI17866", pResult !=  __nullptr);
		ASSERT_ARGUMENT("ELI31324", pFileRecord != __nullptr);

		// ProgressStatus and database not required.  Won't be used if NULL
		IProgressStatusPtr ipProgressStatus(pProgressStatus);
		UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipDB(pDB);

		// Initialize the processing tasks
		init(ipFileProcessingTasks, nActionID, ipDB, ipFAMTagManager, ipFileRequestHandler);
		
		// Process the tasks
		try
		{
			*pResult = processFile(pFileRecord, nActionID, ipProgressStatus,
				asCppBool(bCancelRequested));
		}
		catch (...)
		{
			// Guarantee that Close is called if Init has been called
			close();
			throw;
		}

		// Close the processing tasks
		close();
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
		UCLID_FILEPROCESSINGLib::IFileProcessingTaskPtr ipCurrentTask = getCurrentTask();
		
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

		// Call the close method
		close();
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

		ASSERT_ARGUMENT("ELI17859", ppCurrentTask != __nullptr);

		*ppCurrentTask = (IFileProcessingTask*) getCurrentTask().Detach();
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
		ASSERT_ARGUMENT("ELI17898", pVal != __nullptr);
		
		*pVal = asVariantBool(m_bInitialized);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17899");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingTaskExecutor::Standby(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();
		
		ASSERT_ARGUMENT("ELI33920", pVal != __nullptr);

		vector< StandbyThread* > vecStandbyThreads;
		Win32Event eventCancelProcessing;
		DWORD dwWaitResult;

		try
		{
			// Since Standby occurs on a different thread than Init & Close, we need to be sure that the
			// initialization state remains constant until we are done accessing m_vecProcessingTasks.
			{
				CSingleLock lg(&m_mutex, TRUE);

				// If we are no longer initialized, no need to initialize standby.
				if (!m_bInitialized)
				{
					return S_OK;
				}

				m_eventEndStandby.reset();

				// For each task, spawn a thread to call Standby.
				for (vector< unique_ptr<ProcessingTask> >::iterator iterTask = m_vecProcessingTasks.begin();
					 iterTask != m_vecProcessingTasks.end();
					 iterTask++)
				{
					if (iterTask->get()->Enabled)
					{
						UCLID_FILEPROCESSINGLib::IFileProcessingTaskPtr ipTask = (*iterTask)->Task;
						ASSERT_RESOURCE_ALLOCATION("ELI34264", ipTask != __nullptr);

						StandbyThread *pStandbyThread = new StandbyThread(eventCancelProcessing, ipTask);
						vecStandbyThreads.push_back(pStandbyThread);

						pStandbyThread->CreateThread();
					}
				}
			}
		
			HANDLE pEventHandles[] = { eventCancelProcessing.getHandle(), m_eventEndStandby.getHandle() };

			m_eventStandbyRunning.signal();
		
			// Wait for either processing to be cancelled or standby mode to end.
			dwWaitResult = WaitForMultipleObjects(2, pEventHandles, FALSE, INFINITE);
		}
		catch (...)
		{
			// The standby threads will live forever if endStandby is not called.
			for (vector<StandbyThread*>::iterator iterThread = vecStandbyThreads.begin();
				 iterThread != vecStandbyThreads.end();
				 iterThread++)
			{
				(*iterThread)->endStandby();
			}

			throw;
		}

		// The standby threads will live forever if endStandby is not called.
		for (vector<StandbyThread*>::iterator iterThread = vecStandbyThreads.begin();
				iterThread != vecStandbyThreads.end();
				iterThread++)
		{
			(*iterThread)->endStandby();
		}
		
		if (dwWaitResult - WAIT_OBJECT_0 == 0)
		{
			// Processing has been canceled.
			*pVal = VARIANT_FALSE;
		}
		else
		{
			// Standby mode has ended.
			*pVal = VARIANT_TRUE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33921");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingTaskExecutor::EndStandby()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Ensure standby has started before we try to stop it to prevent a race condition.
		m_eventStandbyRunning.wait();

		m_eventEndStandby.signal();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33928");

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
		ASSERT_ARGUMENT("ELI17875", pbValue != __nullptr);

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
	// Verify the tasks have been initialized
	if (m_bInitialized == false)
	{
		UCLIDException ue("ELI17857", "Internal error: Processing tasks have not been initialized!");
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
EFileProcessingResult CFileProcessingTaskExecutor::processFile(
	IFileRecord* pFileRecord, long nActionID,
	const IProgressStatusPtr& ipProgressStatus, bool bCancelRequested)
{
	try
	{
		UCLID_FILEPROCESSINGLib::IFileRecordPtr ipFileRecord(pFileRecord);
		ASSERT_RESOURCE_ALLOCATION("ELI31322", ipFileRecord != __nullptr);

		// Get the source doc name from the file record
		string strSourceDocName = asString(ipFileRecord->Name);
		ASSERT_RESOURCE_ALLOCATION("ELI17877", !strSourceDocName.empty());

		long nFileID = ipFileRecord->FileID;

		// Progress status can be null, initialize if it exists
		if (ipProgressStatus)
		{
			// Initialize the progress bar appropriately
			long nEnabledTaskCount = countEnabledTasks();
			ipProgressStatus->InitProgressStatus("Initializing tasks...", 0, nEnabledTaskCount, VARIANT_TRUE);
		}

		// Get task list count
		long nTaskCount = m_vecProcessingTasks.size();

		// Exercise each File processor
		for (long nCurrentTask = 0; nCurrentTask < nTaskCount; nCurrentTask++)
		{
			unique_ptr<ProcessingTask>& upTask = m_vecProcessingTasks[nCurrentTask];

			// Only process the file if the task is enabled
			if (upTask->Enabled)
			{
				// Get the task number and task description
				string strCurrentTaskName = "task #";
				strCurrentTaskName += asString(nCurrentTask + 1);
				strCurrentTaskName += " (";
				strCurrentTaskName += upTask->Description;
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
						IProgressStatusPtr ipSubProgressStatus = (ipProgressStatus == __nullptr) ? 
							__nullptr : ipProgressStatus->SubProgressStatus;

						// Ensure that in the case of an error that m_ipCurrentTask will be restored to NULL
						ValueRestorer<UCLID_FILEPROCESSINGLib::IFileProcessingTaskPtr> restorer(m_ipCurrentTask, NULL);

						// Retrieve this file processor; use separate scope to limit m_mutexCurrentTask lock to this call
						{
							// Lock access to m_ipCurrentTask to ensure it can't be read/written at the same time
							CSingleLock lock(&m_mutexCurrentTask, TRUE);
							m_ipCurrentTask = upTask->Task;
							ASSERT_RESOURCE_ALLOCATION("ELI17692", m_ipCurrentTask != __nullptr);
						}

						// Was cancel request either passed in or received via a call to Cancel?
						bool bCancel = (bCancelRequested || m_eventCancelRequested.isSignaled());

						UCLID_FILEPROCESSINGLib::EFileProcessingResult eResult =
							m_ipCurrentTask->ProcessFile(ipFileRecord,
							nActionID, m_ipFAMTagManager, m_ipDB, ipSubProgressStatus,
							asVariantBool(bCancel));

						// Task is no longer running; indicate such
						{
							// Lock access to m_ipCurrentTask to ensure it can't be read/written at the same time
							CSingleLock lock(&m_mutexCurrentTask, TRUE);
							m_ipCurrentTask = __nullptr;
						}

						// Check success flag
						if (eResult == kProcessingCancelled ||
							eResult == kProcessingSkipped ||
							eResult == kProcessingDelayed)
						{
							// Processing didn't complete.
							// Log the fact that processing was cancelled or skipped
							string strMsg = "Application Trace: Processing ";
							switch (eResult)
							{
								case kProcessingCancelled:	strMsg += "cancelled";	break;
								case kProcessingSkipped:	strMsg += "skipped";	break;
								case kProcessingDelayed:	strMsg += "delayed";	break;
							}
							strMsg += " while performing ";
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
							strMsg += ".";

							// Add the history record and debug information, then log the exception
							UCLIDException ue("ELI17862",strMsg);
							ue.addDebugInfo("File", strSourceDocName);
							ue.addDebugInfo("Task", strCurrentTaskName);
							ue.addDebugInfo("User Name", getCurrentUserName());
							ue.log();

							return (EFileProcessingResult)eResult;
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

					// Add the history record and debug information before re-throwing the exception.
					UCLIDException uexOuter("ELI17697",strMsg, ue);
					uexOuter.addDebugInfo("File", strSourceDocName);
					uexOuter.addDebugInfo("Task", strCurrentTaskName);
					uexOuter.addDebugInfo("FPS", asString(m_ipFAMTagManager->FPSFileName));

					throw uexOuter;
				}
			}
		}
		
		// Update progress bar to indicate completion
		if (ipProgressStatus)
		{
			ipProgressStatus->CompleteCurrentItemGroup();
		}

		// Return successful completion
		return kProcessingSuccessful;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26724");
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingTaskExecutor::init(const IIUnknownVectorPtr& ipFileProcessingTasks,
									   long actionID,
									   const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr& ipDB,
									   const UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr& ipFAMTagManager,
									   const UCLID_FILEPROCESSINGLib::IFileRequestHandlerPtr& ipFileRequestHandler)
{
	try
	{
		// Ensure we are not already initialized
		if (m_bInitialized)
		{
			UCLIDException ue("ELI17879", "Internal error: Task executor has already been initialized!");
			throw ue;
		}

		// Clear any previous cancel request
		m_eventCancelRequested.reset();

		// Check arguments
		ASSERT_ARGUMENT("ELI17698", ipFileProcessingTasks != __nullptr);
		m_ipFAMTagManager = ipFAMTagManager;
		ASSERT_ARGUMENT("ELI17702", m_ipFAMTagManager != __nullptr);

		m_ipDB = ipDB;
		m_ipFileRequestHandler = ipFileRequestHandler;

		// Build the collection of tasks and initialize each enabled FileProcessingTask
		int nTaskCount = ipFileProcessingTasks->Size();
		m_vecProcessingTasks.reserve(nTaskCount);
		for (int i = 0; i < nTaskCount ; i++)
		{
			// Retrieve specified file processor Object With Description
			IObjectWithDescriptionPtr ipObject(ipFileProcessingTasks->At(i));
			ASSERT_RESOURCE_ALLOCATION("ELI17848", ipObject != __nullptr);

			// Retrieve this file processor
			UCLID_FILEPROCESSINGLib::IFileProcessingTaskPtr ipFileProc(ipObject->Object);
			ASSERT_RESOURCE_ALLOCATION("ELI17849", ipFileProc != __nullptr);

			m_vecProcessingTasks.push_back(unique_ptr<ProcessingTask>(
				new ProcessingTask(ipFileProc, asString(ipObject->Description),
					asCppBool(ipObject->Enabled))));

			// Only initialize the processing task if it is enabled
			if (m_vecProcessingTasks[i]->Enabled)
			{
				ipFileProc->Init(actionID, ipFAMTagManager, ipDB, m_ipFileRequestHandler);
			}
		}

		m_bInitialized = true;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26725");
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingTaskExecutor::close()
{
	try
	{
		// Set initialized flag to false immediately; cannot guarantee initialization beyond this point
		m_bInitialized = false;

		// Close each FileProcessingTask
		for (vector< unique_ptr<ProcessingTask> >::iterator it = m_vecProcessingTasks.begin();
			it != m_vecProcessingTasks.end(); it++)
		{
			// Only close the processing task if it is enabled
			if (it->get()->Enabled)
			{
				it->get()->Task->Close();
			}
		}
		
		// Clear the vector
		m_vecProcessingTasks.clear();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26783");
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileProcessingTaskPtr CFileProcessingTaskExecutor::getCurrentTask()
{
	try
	{
		// Lock access to m_ipCurrentTask to ensure it can't be read/written at the same time
		CSingleLock lock(&m_mutexCurrentTask, TRUE);

		return m_ipCurrentTask;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26737");
}
//-------------------------------------------------------------------------------------------------
long CFileProcessingTaskExecutor::countEnabledTasks()
{
	long nCount = 0;
	for(vector< unique_ptr<ProcessingTask> >::iterator it = m_vecProcessingTasks.begin();
		it != m_vecProcessingTasks.end(); it++)
	{
		if (it->get()->Enabled)
		{
			nCount++;
		}
	}

	return nCount;
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingTaskExecutor::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI17690", "File Processing Task Executor");
}
//-------------------------------------------------------------------------------------------------
