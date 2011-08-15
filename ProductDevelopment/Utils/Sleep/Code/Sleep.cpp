//==================================================================================================
//
// COPYRIGHT (c) 2008 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	Sleep.cpp
//
// PURPOSE:	To search a file (or group of files) for a specified search string
//			and return the match information. (enhanced as per [p13 #4948])
//
// AUTHORS:	
//
// MODIFIED BY:	Jeff Shergalis as per [p13 #4969]
//
//==================================================================================================
#include "stdafx.h"
#include "Sleep.h"
#include "VerboseDlg.h"
#include "SleepConstants.h"

#include <UCLIDException.h>
#include <UCLIDExceptionDlg.h>
#include <cpputil.h>

#include <windows.h>
#include <string>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

//--------------------------------------------------------------------------------------------------
// Helper functions
//--------------------------------------------------------------------------------------------------
void displayUsage()
{
	string strMessage = "Usage: Sleep <SleepTime>|</?> [OPTIONS]\n";
	strMessage += "SleepTime - The amount of time to sleep. The SleepTime is assumed\n";
	strMessage += "\tto be in milliseconds unless ms, s, m, or h is specified.\n";
	strMessage += "\t(Example: Sleep 5s would be treated as sleep for 5 seconds).\n";
	strMessage += "/? - Display this usage message\n";
	strMessage += "OPTIONS:\n";
	strMessage += "/r - sleep for a random amount of time between 0 and SleepTime\n";
	strMessage += "/v - verbose output (displays modeless dialog with the sleep time)\n\n";
	strMessage += "EXAMPLE:\n";
	strMessage += "Sleep 2m /v /r would specify to sleep for a random time between\n";
	strMessage += "\t0 and 120,000 milliseconds and will display a dialog\n";
	strMessage += "\tshowing how long it will sleep.\n\n";

	AfxMessageBox(strMessage.c_str(), MB_OK | MB_ICONINFORMATION);
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To process the sleep time argument looking for unit specification and to convert
//			the specified time and units to milliseconds [LegacyRCAndUtils #4973] - JDS - 05/05/08
DWORD computeSleepTime(const string& strSleepTime)
{
	try
	{
		// find the first position that is not a number
		size_t pos = strSleepTime.find_first_not_of("0123456789");

		// if pos == npos then no unit specifier, default to milliseconds
		if (pos == strSleepTime.npos)
		{
			return asUnsignedLong(strSleepTime);
		}

		// get the number part from the string
		string strNumberPart = strSleepTime.substr(0, pos);

		// get the units from the string
		string strUnitPart = strSleepTime.substr(pos);
		makeLowerCase(strUnitPart);

		unsigned long ulTimeMultiplier = 0;

		// SleepTime needs to be in milliseconds - check units and pick
		// appropriate multiplier
		if (strUnitPart == "ms")
		{
			ulTimeMultiplier = glMILLISECONDS_MULTIPLIER;
		}
		else if (strUnitPart == "s")
		{
			ulTimeMultiplier = glSECONDS_MULTIPLIER;
		}
		else if (strUnitPart == "m")
		{
			ulTimeMultiplier = glMINUTES_MULTIPLIER;
		}
		else if (strUnitPart == "h")
		{
			ulTimeMultiplier = glHOURS_MULTIPLIER;
		}
		else
		{
			// unrecognized units, throw exception

			UCLIDException ue("ELI21043", 
				"Sleep time contained an unrecognized unit specifier.");
			ue.addDebugInfo("Unit specifier", strUnitPart);
			ue.addDebugInfo("SleepTime", strSleepTime);
			throw ue;
		}

		return (ulTimeMultiplier * asUnsignedLong(strNumberPart));
	}
	catch (UCLIDException& ue)
	{
		UCLIDException uexOuter("ELI21044", "Unable to determine sleep time!", ue);
		uexOuter.addDebugInfo("SleepTime", strSleepTime);
		throw uexOuter;
	}
}

//--------------------------------------------------------------------------------------------------
// CSleepApp
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CSleepApp, CWinApp)
	ON_COMMAND(ID_HELP, &CWinApp::OnHelp)
END_MESSAGE_MAP()

//--------------------------------------------------------------------------------------------------
// CSleepApp construction
//--------------------------------------------------------------------------------------------------
CSleepApp::CSleepApp()
{
}

//--------------------------------------------------------------------------------------------------
// The one and only CSleepApp object
//--------------------------------------------------------------------------------------------------
CSleepApp theApp;

//--------------------------------------------------------------------------------------------------
// CSleepApp initialization
//--------------------------------------------------------------------------------------------------
BOOL CSleepApp::InitInstance()
{
	CWinApp::InitInstance();

	CVerboseDlg dlgVerbose;

	try
	{
		try
		{
			bool bVerbose = false;

			if (__argc < 2 || __argc > 4)
			{
				displayUsage();
				return FALSE;
			}

			// Setup exception handling
			UCLIDExceptionDlg exceptionDlg;
			UCLIDException::setExceptionHandler( &exceptionDlg );

			// get the first argument, it should be either /?
			// or the sleep time 
			string strArg1(__argv[1]);

			// if /? display usage and exit [LegacyRCAndUtils #4973]
			if (strArg1 == "/?")
			{
				displayUsage();
				return FALSE;
			}

			// not /? treat as time specification [LegacyRCAndUtils #4973]
			unsigned long ulSleep = computeSleepTime(strArg1);

			// look for other options
			for (int i = 2; i < __argc; i++)
			{
				string arg(__argv[i]);
				makeLowerCase(arg);

				// random sleep time
				if (arg == "/r")
				{
					srand(GetTickCount());
					long nRand = rand();
					ulSleep = (long)(((double)nRand / double(RAND_MAX)) * (double)ulSleep);
				}
				// verbose mode
				else if (arg == "/v")
				{
					bVerbose = true;
				}
				// unrecognized option
				else
				{
					displayUsage();
					return FALSE;
				}
			}

			if(bVerbose)
			{
				// finish creating the dialog to display verbose information
				dlgVerbose.Create(IDD_SLEEP_DIALOG);

				// show the window
				dlgVerbose.ShowWindow(SW_SHOW);

				// set the sleep time label
				dlgVerbose.setSleepTime(ulSleep);

				// ensure the label gets updated
				dlgVerbose.UpdateData();
			}

			// sleep for specified length of time
			Sleep(ulSleep);

			// destroy the verbose window if it has been created
			if (dlgVerbose.m_hWnd)
			{
				dlgVerbose.DestroyWindow();
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI21033");
	}
	catch(UCLIDException& ue)
	{
		// ensure the dialog gets cleaned up if necessary
		if (dlgVerbose.m_hWnd != NULL)
		{
			dlgVerbose.DestroyWindow();
		}

		ue.display();
	}

	// return FALSE so that we exit the
	// application, rather than start the application's message pump.
	return FALSE;
}
//--------------------------------------------------------------------------------------------------
