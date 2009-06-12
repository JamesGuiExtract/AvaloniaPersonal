
#pragma once

#include "Win32GlobalAtom.h"
#include "Win32Event.h"

#include <string>

// forward declarations
class UCLIDException;

//-------------------------------------------------------------------------------------------------
// FailureDetectionAndReportingMgr class
//-------------------------------------------------------------------------------------------------
class EXPORT_BaseUtils FailureDetectionAndReportingMgr
{
public:
	static void notifyExceptionLogged(const UCLIDException *pEx);

	static void notifyExceptionDisplayed(const UCLIDException *pEx);

	static void notifyApplicationRunning();

	static void notifyApplicationNormallyExited();

	static void notifyApplicationAbnormallyExited();

	static void startFDRSPingThread();

	static void stopFDRSPingThread();

private:
	// returns the HWND of the main window of the Failure Detection and 
	// Reporting System (FDRS).  If the FDRS is not running, NULL is returned.
	static HWND getFRDSWindow();

	// sends a message to the Failure Detection and Reporting System (FDRS)
	// NOTE: wParam is not one of the arguments for this method because the wParam
	// is always set to the Unique Process Identifier (UPI) for messages sent to the
	// FDRS.
	static void sendMessageToFDRS(UINT uiMsgID, LPARAM lParam);
	
	friend UINT FDRSPingThread(LPVOID pData);
	
	// used to tell the FDRSPingThread to stop
	static Win32Event ms_eventPingStopRequest;

	// used to indicate that the FDRSPingThread has stopped
	static Win32Event ms_eventPingThreadStopped;
};
//-------------------------------------------------------------------------------------------------
