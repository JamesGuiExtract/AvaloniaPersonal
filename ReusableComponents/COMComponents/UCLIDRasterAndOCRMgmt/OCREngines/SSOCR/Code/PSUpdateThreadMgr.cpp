
#include "stdafx.h"
#include "PSUpdateThreadMgr.h"

#include <cpputil.h>
#include <UCLIDException.h>

// Static and global variables
vector<string> PSUpdateThreadManager::ms_vecstrProcessName;

//-------------------------------------------------------------------------------------------------
PSUpdateThreadManager::PSUpdateThreadManager(IProgressStatus* pProgressStatus, IScansoftOCR2* pOCREngine, 
									 long lPagesToOCR)
 : m_ipProgressStatus(pProgressStatus),
   m_ipOCREngine(pOCREngine),
   m_lPagesToOCR(lPagesToOCR),
   m_lProgressItemsPerPage(0),
   m_lCompletedProgressItems(0),
   m_lTotalProgressItems(1),
   m_bOCRComplete(false),
   m_dwOCREngineCookie(0),
   m_dwProgressStatusCookie(0),
   m_ipInterfaceTablePtr(NULL)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// ensure the progress status object and OCR Engine are non-NULL
		ASSERT_ARGUMENT("ELI16163", m_ipProgressStatus != NULL);
		ASSERT_ARGUMENT("ELI16201", m_ipOCREngine != NULL);

		// determine the number of recognition passes
		// Two recognition passes are always made.  The third one is optional (and
		// is controlled by a registry key)
		long lRecognitionPasses = 2;
		if (asCppBool(m_ipOCREngine->WillPerformThirdRecognitionPass()))
		{
			lRecognitionPasses++;
		}

		// calculate the count of progress status items
		m_lProgressItemsPerPage = ms_lPROGRESS_ITEMS_PER_LOAD_IMAGE + 
			ms_lPROGRESS_ITEMS_PER_DECOMPOSITION + 
			lRecognitionPasses * ms_lPROGRESS_ITEMS_PER_RECOGNITION_PASS;
		m_lTotalProgressItems = m_lPagesToOCR * m_lProgressItemsPerPage;

		// initialize the progress status object
		m_ipProgressStatus->InitProgressStatus("Initializing OCR...", 
			m_lCompletedProgressItems, m_lTotalProgressItems, VARIANT_FALSE);

		// initialize the static array of process names
		static bool sbInitialized = false;
		if (!sbInitialized)
		{
			static CMutex localMutex;
			CSingleLock lock(&localMutex, TRUE);

			if (!sbInitialized)
			{
				ms_vecstrProcessName.resize(PID_SCANNER_WARMUP + 1);
				ms_vecstrProcessName[PID_IMGINPUT] =       "Loading Image";
				ms_vecstrProcessName[PID_IMGSAVE] =        "Saving Image";
				ms_vecstrProcessName[PID_IMGPREPROCESS] =  "Pre-Processing Image";
				ms_vecstrProcessName[PID_DECOMPOSITION] =  "Performing Page Decomposition";
				ms_vecstrProcessName[PID_RECOGNITION1] =   "Performing First Recognition Pass";
				ms_vecstrProcessName[PID_RECOGNITION2] =   "Performing Second Recognition Pass";
				ms_vecstrProcessName[PID_RECOGNITION3] =   "Performing Third Recognition Pass";
				ms_vecstrProcessName[PID_SPELLING] =       "Spell Checking";
				ms_vecstrProcessName[PID_FORMATTING] =     "Formatting Image";
				ms_vecstrProcessName[PID_WRITEFOUTDOC] =   "Writing Recognized Text";
				ms_vecstrProcessName[PID_CONVERTIMG] =     "Writing Graphical Zones";
				ms_vecstrProcessName[PID_SCANNER_WARMUP] = "Scanner Warming Up";

				sbInitialized = true;
			}
		}
		
		// In order to marshall the OCR engine and progress status interface pointers into the status
		// update thread, add entries into a global interface table.
		m_ipInterfaceTablePtr.CreateInstance(CLSID_StdGlobalInterfaceTable);
		m_ipInterfaceTablePtr->RegisterInterfaceInGlobal(m_ipOCREngine, IID_IScansoftOCR2, 
			&m_dwOCREngineCookie);
		m_ipInterfaceTablePtr->RegisterInterfaceInGlobal(m_ipProgressStatus, IID_IProgressStatus, 
			&m_dwProgressStatusCookie);

		// create thread to poll SSOCR2 engine for progress status updates
		if( !AfxBeginThread(progressStatusUpdateLoop, this) )
		{
			throw UCLIDException("ELI16203", "Unable to initialize progress status update thread.");
		}

		// wait for the progress status update thread to start
		m_eventPSThreadStarted.wait();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI16159");
}
//-------------------------------------------------------------------------------------------------
PSUpdateThreadManager::PSUpdateThreadManager(PSUpdateThreadManager *pThreadManager)
: m_dwOCREngineCookie(0)
, m_dwProgressStatusCookie(0)
, m_ipInterfaceTablePtr(NULL)
{
	try
	{
		// m_ipInterfaceTablePtr will remain NULL using this constructor-- it is used by the
		// destructor as a flag for whether the instance was the original or a marshalled copy for
		// the update status thread.

		// For a private copy marshalled into a status update thread, the source instance must have
		// a global interface table and cookies for the OCR manager and progress status objects.
		ASSERT_ARGUMENT("ELI25289", pThreadManager != NULL);
		ASSERT_ARGUMENT("ELI25298", pThreadManager->m_ipInterfaceTablePtr != NULL);
		ASSERT_ARGUMENT("ELI25296", pThreadManager->m_dwOCREngineCookie != 0);
		ASSERT_ARGUMENT("ELI25297", pThreadManager->m_dwProgressStatusCookie != 0);

		// Copy the settings
		m_lPagesToOCR = pThreadManager->m_lPagesToOCR;
		m_lProgressItemsPerPage = pThreadManager->m_lProgressItemsPerPage;
		m_lCompletedProgressItems = pThreadManager->m_lCompletedProgressItems;
		m_lTotalProgressItems = pThreadManager->m_lTotalProgressItems;
		m_bOCRComplete = pThreadManager->m_bOCRComplete;
		
		// Copy the events
		m_eventPSThreadStarted = pThreadManager->m_eventPSThreadStarted;
		m_eventPSThreadStop = pThreadManager->m_eventPSThreadStop;
		m_eventPSThreadStopped  = pThreadManager->m_eventPSThreadStopped;

		// Marshall OCR engine interface into this thread from the interface table.
		pThreadManager->m_ipInterfaceTablePtr->GetInterfaceFromGlobal(
			pThreadManager->m_dwOCREngineCookie, IID_IScansoftOCR2, 
			(void **)&m_ipOCREngine);

		// Marshall progress status interface into this thread from the interface table.
		pThreadManager->m_ipInterfaceTablePtr->GetInterfaceFromGlobal(
			pThreadManager->m_dwProgressStatusCookie, IID_IProgressStatus, 
			(void **)&m_ipProgressStatus);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25290");
}
//-------------------------------------------------------------------------------------------------
PSUpdateThreadManager::~PSUpdateThreadManager()
{
	try
	{
		// Only if this instance has a m_ipInterfaceTablePtr defined (meaning it is the original
		// copy and not a copy for the status update thread), does final cleanup need to occur.
		if (m_ipInterfaceTablePtr != NULL)
		{
			// kill the progress status update thread
			m_eventPSThreadStop.signal();

			// wait for the progress status update thread to die
			m_eventPSThreadStopped.wait();

			// Remove the OCR Engine from the interface table.
			if (m_dwOCREngineCookie != 0)
			{
				m_ipInterfaceTablePtr->RevokeInterfaceFromGlobal(m_dwOCREngineCookie);
			}

			// Remove the progress status from the interface table.
			if (m_dwProgressStatusCookie != 0)
			{
				m_ipInterfaceTablePtr->RevokeInterfaceFromGlobal(m_dwProgressStatusCookie);
			}

			m_ipInterfaceTablePtr = NULL;
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16164");
}
//-------------------------------------------------------------------------------------------------
void PSUpdateThreadManager::notifyOCRComplete()
{
	try
	{
		// flag the OCR as completed
		m_bOCRComplete = true;

		// signal the thread to stop
		m_eventPSThreadStop.signal();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI16208");
}

//-------------------------------------------------------------------------------------------------
// Helper methods
//-------------------------------------------------------------------------------------------------
UINT PSUpdateThreadManager::progressStatusUpdateLoop(void *pData)
{
	CoInitializeEx(NULL, COINIT_MULTITHREADED);

	// create pointer for progress status update thread manager
	PSUpdateThreadManager* pThreadManager = NULL;

	try
	{
		// Get a copy of the progress status thread manager that is marshalled into this thread.
		pThreadManager = new PSUpdateThreadManager((PSUpdateThreadManager*) pData);
		ASSERT_RESOURCE_ALLOCATION("ELI25291", pThreadManager != NULL);

		// signal that the progress status thread has started
		pThreadManager->m_eventPSThreadStarted.signal();

		// begin periodic updates of the progress status object
		while (pThreadManager->m_eventPSThreadStop.wait(ms_dwMILLISECONDS_UPDATE_INTERVAL) == WAIT_TIMEOUT)
		{
			pThreadManager->updateProgressStatus();
		}

		// check if the OCR has finished
		if (pThreadManager->m_bOCRComplete)
		{
			// mark the remaining progress status items as completed
			IProgressStatusPtr ipProgressStatus = pThreadManager->m_ipProgressStatus;
			ASSERT_RESOURCE_ALLOCATION("ELI25245", ipProgressStatus != NULL);

			ipProgressStatus->CompleteProgressItems("Finished OCR", 
				pThreadManager->m_lTotalProgressItems - pThreadManager->m_lCompletedProgressItems);
		}	
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16204");

	try
	{
		// signal that the progress status thread has stopped
		if (pThreadManager)
		{
			pThreadManager->m_eventPSThreadStopped.signal();

			// Deleting the mashalled copy won't effect the copy of the m_eventPSThreadStopped event
			// that the source instance has.
			delete pThreadManager;
		}
		else
		{
			// pThreadManager should never be null!
			THROW_LOGIC_ERROR_EXCEPTION("ELI16242")
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16237");

	CoUninitialize();
	return 0;
}
//-------------------------------------------------------------------------------------------------
void PSUpdateThreadManager::updateProgressStatus()
{
	try
	{
		// progress status update parameters for calls to SSOCR2 engine
		long lProcessID;
		long lPercentComplete;
		long lPageIndex;
		long lPageNumber;

		// retrieve progress data from SSOCR2
		m_ipOCREngine->GetProgress(&lProcessID, &lPercentComplete, &lPageIndex, &lPageNumber);
	
		// instantiate the current completed progress items
		long lCurrentCompletedProgressItems = 0;

		// calculate the progress items for all the previously completed processes.
		// NOTE: notice the lack of break statements means all previous processes will be summed.
		switch (lProcessID)
		{
		case PID_RECOGNITION3:

			// has completed second recognition pass
			lCurrentCompletedProgressItems += ms_lPROGRESS_ITEMS_PER_RECOGNITION_PASS;
		
		case PID_RECOGNITION2:
		
			// has completed first recognition pass
			lCurrentCompletedProgressItems += ms_lPROGRESS_ITEMS_PER_RECOGNITION_PASS;

		case PID_RECOGNITION1:

			// has completed decomposition
			lCurrentCompletedProgressItems += ms_lPROGRESS_ITEMS_PER_DECOMPOSITION;
		
		case PID_DECOMPOSITION:

			// has loaded the image
			lCurrentCompletedProgressItems += ms_lPROGRESS_ITEMS_PER_LOAD_IMAGE;

		case PID_IMGINPUT:

			// add previous OCRed pages if applicable (ie. lPageIndex > 0)
			lCurrentCompletedProgressItems += lPageIndex * m_lProgressItemsPerPage;

			break;
		}

		// calculate the remaining progress items based on 
		// the percentage complete of the current process
		switch (lProcessID)
		{
		case PID_RECOGNITION3:
		case PID_RECOGNITION2:
		case PID_RECOGNITION1:
			lCurrentCompletedProgressItems += lPercentComplete * ms_lPROGRESS_ITEMS_PER_RECOGNITION_PASS / 100;
			break;

		case PID_DECOMPOSITION:
			lCurrentCompletedProgressItems += lPercentComplete * ms_lPROGRESS_ITEMS_PER_DECOMPOSITION / 100;
			break;
		
		case PID_IMGINPUT:
			lCurrentCompletedProgressItems += lPercentComplete * ms_lPROGRESS_ITEMS_PER_LOAD_IMAGE / 100;
			break;
		}

		// check if any progress items have completed since the last notification
		if (m_lCompletedProgressItems < lCurrentCompletedProgressItems)
		{
			// update the progress status object
			m_ipProgressStatus->CompleteProgressItems(
				get_bstr_t( ms_vecstrProcessName[lProcessID] + " on page " + asString(lPageNumber) ), 
				lCurrentCompletedProgressItems - m_lCompletedProgressItems);

			// store the new number of completed progress items
			m_lCompletedProgressItems = lCurrentCompletedProgressItems;
		}	
	}
	catch(...)
	{
		// check if OCR engine was killed
		if(!m_ipOCREngine)
		{
			// there is no need for further progress status updates.
			// NOTE: the outer scope is responsible for handling the OCR engine, 
			// so there is no need to log an error about it here.
			m_eventPSThreadStop.signal();
		}
		else
		{
			throw UCLIDException("ELI16648", 
				"Application trace: Unable to update OCR progress status.");
		}
	}
}
//-------------------------------------------------------------------------------------------------
