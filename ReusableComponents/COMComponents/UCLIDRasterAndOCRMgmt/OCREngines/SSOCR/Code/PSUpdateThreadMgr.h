
#pragma once

#include <COMUtils.h>
#include <KernelAPI.h>
#include <Win32Event.h>

#include <string>
#include <vector>

using std::string;
using std::vector;

//--------------------------------------------------------------------------------------------------
class PSUpdateThreadManager
{
public:

	//---------------------------------------------------------------------------------------------
	// PURPOSE: To create a progress status update thread manager to manage the specified progress 
	//          status object while the specified OCR engine is OCRing the specified number of pages.
	// REQUIRE: (1) pProgressStatus != __nullptr
	//          (2) pOCREngine != __nullptr
	//          (3) lPagesToOCR >= 0
	// PROMISE: (1) A progress status update thread manager will be created and its progress status 
	//          update thread will be initialized and immediately begin polling pOCREngine at 
	//          regular intervals for progress updates.
	//          (2) pProgressStatus will be updated continuously. 
	//          (3) lPagesToOCR will be used to determine the total number of progress items for the 
	//          progress status object.
	PSUpdateThreadManager(IProgressStatus* pProgressStatus, IScansoftOCR2* pOCREngine, long lPagesToOCR);
	~PSUpdateThreadManager();


	//---------------------------------------------------------------------------------------------
	// PURPOSE: To notify the progress status update thread manager that OCR has completed successfully.
	// PROMISE: The progress status object will be updated and the update thread will be stopped.
	void notifyOCRComplete();

private:

	///////////////
	// Methods
	///////////////

	//---------------------------------------------------------------------------------------------
	// PURPOSE: A private contructor used to create a copy of provided PSUpdateThreadManager
	//			instance in a different thread than the original by marshalling the OCR engine and
	//			progress status COM interfaces into the new thread.
	// REQUIRE: (1) pThreadManager != __nullptr
	//          (2) pThreadManager->m_ipInterfaceTablePtr != __nullptr
	// PROMISE: A copy of the supplied PSUpdateThreadManager instance will be created which can be
	//			used in the current thread.
	PSUpdateThreadManager(PSUpdateThreadManager *pThreadManager);

	//---------------------------------------------------------------------------------------------
	// PURPOSE: To regularly poll the OCR engine for progress status updates and update the 
	//          progress status object accordingly.
	// REQUIRE: pData is a pointer to the PSUpdateThreadManager
	// PROMISE: The progress status object associated with the PSUpdateThreadManager will be 
	//			updated at regular intervals until the update thread is signaled to stop executing.
	static UINT progressStatusUpdateLoop(void *pData);

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Retrieves the current progress status from the OCR engine, calculates the number of 
	//          progress items completed, and updates the progress status object accordingly.
	// PROMISE: The number of progress status objects that have completed since the last call to
	//          this function will be added to the progress status object's count of the total 
	//          progress items completed.
	void updateProgressStatus();

	///////////////
	// Variables
	///////////////

	// progress status object to be updated
	IProgressStatusPtr m_ipProgressStatus;
	
	IScansoftOCR2Ptr m_ipOCREngine;

	// Used to marshall the ProgressStatus and OCREngine objects into the progress update thread.
	IGlobalInterfaceTablePtr m_ipInterfaceTablePtr;

	// Token used to retreive the OCREngine interface in the progress update thread.
	DWORD m_dwOCREngineCookie;

	// Token used to retreive the ProgressStatus interface in the progress update thread.
	DWORD m_dwProgressStatusCookie;

	// events for the progress status update thread
	Win32Event m_eventPSThreadStarted; // progress status update thread has started
	Win32Event m_eventPSThreadStop;    // signal progress status update thread to stop
	Win32Event m_eventPSThreadStopped; // progress status update thread has stopped

	// constant defining the milliseconds between progress status update intervals
	static const DWORD ms_dwMILLISECONDS_UPDATE_INTERVAL = 250;

	// an array of strings of OCR process names indexed by RecAPI process ID
	static vector<string> ms_vecstrProcessName;
	static CCriticalSection ms_PSUpdateThreadManagerMutex;

	// constants for determining number of progress items per OCR process
	// NOTE: these values are simply weights which were chosen based on trial and observation
	static const long ms_lPROGRESS_ITEMS_PER_LOAD_IMAGE = 3;
	static const long ms_lPROGRESS_ITEMS_PER_DECOMPOSITION = 2;
	static const long ms_lPROGRESS_ITEMS_PER_RECOGNITION_PASS = 13;
	
	// variables for determining progress items
	long m_lPagesToOCR;
	long m_lProgressItemsPerPage;
	long m_lCompletedProgressItems;
	long m_lTotalProgressItems;

	// set to true if OCR has completed, false otherwise
	volatile bool m_bOCRComplete;
};
//--------------------------------------------------------------------------------------------------
