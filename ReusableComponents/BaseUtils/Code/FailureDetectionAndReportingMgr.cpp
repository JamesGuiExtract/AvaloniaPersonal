
#include "stdafx.h"
#include "FailureDetectionAndReportingMgr.h"
#include "FailureDetectionAndReportingConstants.h"
#include "UCLIDException.h"
#include "TemporaryFileName.h"
#include "cpputil.h"
#include "UPI.h"

#include <fstream>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Static 
//-------------------------------------------------------------------------------------------------
Win32Event FailureDetectionAndReportingMgr::ms_eventPingStopRequest;
Win32Event FailureDetectionAndReportingMgr::ms_eventPingThreadStopped;

//-------------------------------------------------------------------------------------------------
// FailureDetectionAndReportingMgr class
//-------------------------------------------------------------------------------------------------
void FailureDetectionAndReportingMgr::notifyExceptionLogged(const UCLIDException *pEx)
{
	string strTempFileName;

	try
	{
		// if the Failure Detection and Reporting System (FDRS) is available, 
		// notify it of this logged exception
		if (getFRDSWindow() != __nullptr)
		{
			// write the exception to a temporary file
			string strEx = pEx->asStringizedByteStream();

			TemporaryFileName tmpFile(NULL, ".tmp", false);
			strTempFileName = tmpFile.getName();
			ofstream outfile(strTempFileName.c_str());
			outfile << strEx;
			outfile.close();
			waitForFileToBeReadable(strTempFileName);

			// create a global atom with the name of the temporary file
			// and send it along with the notify-exception-logged message
			Win32GlobalAtom atom(strTempFileName);
			sendMessageToFDRS(gNOTIFY_EXCEPTION_LOGGED_MSG, (LPARAM) atom.detach());
		}
	}
	catch (UCLIDException& ue)
	{
		UCLIDException uexOuter("ELI11879", 
			"Unable to notify Failure Detection and Reporting System (FDRS) of logged exception!", ue);
		uexOuter.addDebugInfo("strTempFileName", strTempFileName);
		uexOuter.log("", false);
	}
	catch (...)
	{
		UCLIDException ue("ELI11878", 
			"Unable to notify Failure Detection and Reporting System (FDRS) of logged exception!");
		ue.addDebugInfo("strTempFileName", strTempFileName);
		ue.log("", false);
	}
}
//-------------------------------------------------------------------------------------------------
void FailureDetectionAndReportingMgr::notifyExceptionDisplayed(const UCLIDException *pEx)
{
	string strTempFileName;

	try
	{
		// if the Failure Detection and Reporting System (FDRS) is available, 
		// notify it of this logged exception
		if (getFRDSWindow() != __nullptr)
		{
			// write the exception to a temporary file
			string strEx = pEx->asStringizedByteStream();

			TemporaryFileName tmpFile(NULL, ".tmp", false);
			strTempFileName = tmpFile.getName();
			ofstream outfile(strTempFileName.c_str());
			outfile << strEx;
			outfile.close();
			waitForFileToBeReadable(strTempFileName);

			// create a global atom with the name of the temporary file
			// and send it along with the notify-exception-logged message
			Win32GlobalAtom atom(strTempFileName);
			sendMessageToFDRS(gNOTIFY_EXCEPTION_DISPLAYED_MSG, (LPARAM) atom.detach());
		}
	}
	catch (UCLIDException& ue)
	{
		UCLIDException uexOuter("ELI12350", 
			"Unable to notify Failure Detection and Reporting System (FDRS) of logged exception!", ue);
		uexOuter.addDebugInfo("strTempFileName", strTempFileName);
		uexOuter.log("", false);
	}
	catch (...)
	{
		UCLIDException ue("ELI12351", 
			"Unable to notify Failure Detection and Reporting System (FDRS) of logged exception!");
		ue.addDebugInfo("strTempFileName", strTempFileName);
		ue.log("", false);
	}
}
//-------------------------------------------------------------------------------------------------
UINT FDRSPingThread(LPVOID pData)
{
	pData;	// unused parameter
	try
	{
		while ( FailureDetectionAndReportingMgr::ms_eventPingStopRequest.wait(uiPING_FREQUENCY_IN_SECONDS * 1000) == WAIT_TIMEOUT )
		{
			FailureDetectionAndReportingMgr::notifyApplicationRunning();
		}
	}
	catch (UCLIDException& ue)
	{
		ue.addDebugInfo("ELI12340", "Unknown exception caught in FDRS ping thread!");
		ue.log("", false);
	}
	catch (...)
	{
		UCLIDException ue("ELI12339", "Unknown exception caught in FDRS ping thread!");
		ue.log("", false);
	}
	FailureDetectionAndReportingMgr::ms_eventPingThreadStopped.signal();
	return 0;
}
//-------------------------------------------------------------------------------------------------
void FailureDetectionAndReportingMgr::startFDRSPingThread()
{
	// this function should only be called once
	static bool bAlreadyCalled = false;
	if (!bAlreadyCalled)
	{
		// begin the thread that will keep pinging the FDRS to let it know
		// that this application is still running
		AfxBeginThread(FDRSPingThread, NULL);

		// prevent this method's code from being executed again
		bAlreadyCalled = true;
	}
}
//-------------------------------------------------------------------------------------------------
void FailureDetectionAndReportingMgr::notifyApplicationRunning()
{
	try
	{
		// if the Failure Detection and Reporting System (FDRS) is available, 
		// notify it of this logged exception
		if (getFRDSWindow() != __nullptr)
		{
			sendMessageToFDRS(gNOTIFY_APPLICATION_RUNNING_MSG, 0);
		}
	}
	catch (UCLIDException& ue)
	{
		UCLIDException uexOuter("ELI12335", 
			"Unable to notify Failure Detection and Reporting System (FDRS) of application start event!", ue);
		uexOuter.log("", false);
	}
	catch (...)
	{
		UCLIDException ue("ELI12336", 
			"Unable to notify Failure Detection and Reporting System (FDRS) of application start event!");
		ue.log("", false);
	}
}
//-------------------------------------------------------------------------------------------------
void FailureDetectionAndReportingMgr::notifyApplicationNormallyExited()
{
	try
	{
		// if the Failure Detection and Reporting System (FDRS) is available, 
		// notify it of this event
		if (getFRDSWindow() != __nullptr)
		{
			sendMessageToFDRS(gNOTIFY_APPLICATION_NORMAL_EXIT_MSG, 0);
			stopFDRSPingThread();
		}
	}
	catch (UCLIDException& ue)
	{
		UCLIDException uexOuter("ELI12346", 
			"Unable to notify Failure Detection and Reporting System (FDRS) of application exit event!", ue);
		uexOuter.log("", false);
	}
	catch (...)
	{
		UCLIDException ue("ELI12347", 
			"Unable to notify Failure Detection and Reporting System (FDRS) of application exit event!");
		ue.log("", false);
	}
}
//-------------------------------------------------------------------------------------------------
void FailureDetectionAndReportingMgr::notifyApplicationAbnormallyExited()
{
	try
	{
		// if the Failure Detection and Reporting System (FDRS) is available, 
		// notify it of this event
		if (getFRDSWindow() != __nullptr)
		{
			sendMessageToFDRS(gNOTIFY_APPLICATION_ABNORMAL_EXIT_MSG, 0);
			stopFDRSPingThread();
		}
	}
	catch (UCLIDException& ue)
	{
		UCLIDException uexOuter("ELI12348", 
			"Unable to notify Failure Detection and Reporting System (FDRS) of application abnormal exit event!", ue);
		uexOuter.log("", false);
	}
	catch (...)
	{
		UCLIDException ue("ELI12349", 
			"Unable to notify Failure Detection and Reporting System (FDRS) of application abnormal exit event!");
		ue.log("", false);
	}
}
//-------------------------------------------------------------------------------------------------
void FailureDetectionAndReportingMgr::stopFDRSPingThread()
{
	if ( !ms_eventPingStopRequest.isSignaled() || !ms_eventPingThreadStopped.isSignaled() )
	{
		ms_eventPingStopRequest.signal();
		ms_eventPingThreadStopped.wait( uiPING_FREQUENCY_IN_SECONDS * 1000 );
	}
}
//-------------------------------------------------------------------------------------------------
// Private/Helper functions
//-------------------------------------------------------------------------------------------------
HWND FailureDetectionAndReportingMgr::getFRDSWindow()
{
	// "#32770" is the WNDCLASS name of a dialog and that is what we 
	// are looking for
	HWND hWnd = ::FindWindow(MAKEINTATOM(32770), gpszFDRS_WINDOW_TITLE);

	return hWnd;
}
//-------------------------------------------------------------------------------------------------
void FailureDetectionAndReportingMgr::sendMessageToFDRS(UINT uiMsgID, LPARAM lParam)
{
	// NOTE: for WPARAM, we are sending the atom associated with the Unique
	// Process Identifier (UPI)...the receiver of this message is required to NOT
	// release the global atom as we need to reuse that atom for subsequent messages

	// The receiver of this message IS required to release any resources that may
	// be associated with lParam.
	static string strUPI = UPI::getCurrentProcessUPI().getUPI();

	Win32GlobalAtom upiAtom(strUPI);

	::PostMessage(getFRDSWindow(), uiMsgID, 
		(WPARAM) upiAtom.detach(), lParam);
}
//-------------------------------------------------------------------------------------------------
