// InstallPCE.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "InstallPCE.h"

#include <UCLIDException.h>
#include <UCLIDExceptionDlg.h>
#include <cpputil.h>

#include <string>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

//--------------------------------------------------------------------------------------------------
// Statics
//--------------------------------------------------------------------------------------------------
Win32Event CInstallPCEApp::ms_eventPrinterInstalled;

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
// constant for use with the displayUsage message - adjust size according to message length
const long glUSAGE_STRING_SIZE = 500;

const string gstrEXAMPLE_PROGID = "\"ESActMaskPCE.ActMaskTIFPrintCaptureEngine.1\"";
const string gstrEXAMPLE_APPNAME = 
	"\"C:\\Program Files\\Extract Systems\\CommonComponents\\IDShieldDesktop.exe\"";

//--------------------------------------------------------------------------------------------------
// CInstallPCEApp
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CInstallPCEApp, CWinApp)
END_MESSAGE_MAP()

//--------------------------------------------------------------------------------------------------
// CInstallPCEApp construction
//--------------------------------------------------------------------------------------------------
CInstallPCEApp::CInstallPCEApp()
{
}

//--------------------------------------------------------------------------------------------------
// Helper methods
//--------------------------------------------------------------------------------------------------
void displayUsage()
{
	// reserve enough space in the string to hold the usage message
	string strUsage;
	strUsage.reserve(glUSAGE_STRING_SIZE);

	strUsage = "InstallPCE <PCEProgID> <HandlerApp | /u>\n\n";
	strUsage += "Usage:\n";
	strUsage += "----------------------\n";
	strUsage += "\tPCEProgID - the progID of the print capture engine to install.\n";
	strUsage += "\tHandlerApp - the full path to the application that will receive\n";
	strUsage += "\t    the captured print output as an image.\n";
	strUsage += "\t/u - Call the uninstall method for the specified progID.\n";
	strUsage += "Example:\n";
	strUsage += "----------------------\n";
	strUsage += "InstallPCE " + gstrEXAMPLE_PROGID + " " + gstrEXAMPLE_APPNAME + "\n\n";

	MessageBox(NULL, strUsage.c_str(), "Usage", MB_OK | MB_ICONINFORMATION);
}

//--------------------------------------------------------------------------------------------------
// The one and only CInstallPCEApp object
//--------------------------------------------------------------------------------------------------
CInstallPCEApp theApp;

//--------------------------------------------------------------------------------------------------
// CInstallPCEApp initialization
//--------------------------------------------------------------------------------------------------
BOOL CInstallPCEApp::InitInstance()
{
	CWinApp::InitInstance();

	INIT_EXCEPTION_AND_TRACING("MLI00164");

	try
	{
		if (__argc != 3)
		{
			displayUsage();
			return FALSE;
		}

		// Setup exception handling
		UCLIDExceptionDlg exceptionDlg;
		UCLIDException::setExceptionHandler( &exceptionDlg );
		_lastCodePos = "10";

		CoInitializeEx(NULL, COINIT_MULTITHREADED);
		
		// scope for COM pointers
		try
		{
			// progID should be first argument
			string strProgID(__argv[1]);
			_lastCodePos = "20";

			// attempt to instantiate the printer capture engine specified
			IPrintCaptureEnginePtr ipPCEngine = __nullptr;
			ipPCEngine.CreateInstance(strProgID.c_str());
			if (ipPCEngine == __nullptr)
			{
				UCLIDException ue("ELI20738", "PCEProgID is not a valid ESPrintCaptureEngine!");
				ue.addDebugInfo("PCEProgID", strProgID);
				throw ue;
			}
			_lastCodePos = "25";

			// second argument is either handler app or /u for uninstall
			string strHandlerApp(__argv[2]);
			makeLowerCase(strHandlerApp);
			_lastCodePos = "30";

			if (strHandlerApp == "/u")
			{
				// call the printer capture engine's uninstall method
				ipPCEngine->Uninstall();
				_lastCodePos = "35";
			}
			else
			{
				// validate the handler app's existence
				validateFileOrFolderExistence(strHandlerApp);
				_lastCodePos = "40";

				// [IDSD:301] (This is a temporary fix)
				// Kick off a thread to watch for the default printer prompt and close it by 
				// answering "no".
				AfxBeginThread(closeDefaultPrinterPrompt, NULL);
				_lastCodePos = "50";
			
				// call the printer capture engine's install method
				ipPCEngine->Install(strHandlerApp.c_str());
				_lastCodePos = "70";

				// Signal the closeDefaultPrinterPrompt thread that the install is complete and if
				// it hasn't found the prompt by now, it can stop looking.
				ms_eventPrinterInstalled.signal();
			}
		}
		catch (...)
		{
			// In the case of an exception, signal ms_eventPrinterInstalled to ensure the
			// closeDefaultPrinterPrompt thread ends if it is running.
			ms_eventPrinterInstalled.signal();

			throw;
		}

		CoUninitialize();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20740");


	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return FALSE;
}
//--------------------------------------------------------------------------------------------------
UINT CInstallPCEApp::closeDefaultPrinterPrompt(LPVOID pData)
{
	// Create a buffer for checking the text of windows.
	char szWindowTextBuffer[256];

	// Search for the prompt as long the printer install process has not completed.
	while (!ms_eventPrinterInstalled.isSignaled())
	{
		// Search for a window with the title "Setup"
		HWND hwnd = ::FindWindow(NULL, "Setup");
		if (hwnd != __nullptr)
		{
			// If we found such a window, search for two immediate children-- one that contains
			// "default printer" (the message box's prompt) and another whose text is "No" (the 
			// button we need to press.
			HWND hwndPrompt = NULL;
			HWND hwndNoButton = NULL;

			for (HWND hwndChild = ::GetWindow(hwnd, GW_CHILD); 
				hwndChild != __nullptr;  
				hwndChild = ::GetWindow(hwndChild, GW_HWNDNEXT))
			{
				// Retrieve the text of the child window (control).
				ZeroMemory(szWindowTextBuffer, sizeof(szWindowTextBuffer));
				::GetWindowText(hwndChild, szWindowTextBuffer, sizeof(szWindowTextBuffer) - 1);
				string strWindowText(szWindowTextBuffer);
				
				// Test for a match.
				if (strWindowText.find("default printer") != string::npos)
				{
					hwndPrompt = hwndChild;
				}
				else if (strWindowText == "&No")
				{
					hwndNoButton = hwndChild;
				}
			}

			// If we found both the child controls we were looking for, simulate the pressing of
			// the no button and exit the thread.
			if (hwndPrompt != __nullptr && hwndNoButton != __nullptr)
			{
				::PostMessage(hwndNoButton, WM_LBUTTONDOWN, 0, 0);
				::PostMessage(hwndNoButton, WM_LBUTTONUP, 0, 0);
				break;
			}
		}

		// Loop about 10 times per sec.
		Sleep(100);
	}

	return 0;
}
//--------------------------------------------------------------------------------------------------