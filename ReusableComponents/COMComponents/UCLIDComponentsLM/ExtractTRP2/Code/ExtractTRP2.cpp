// ExtractTRP2.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "ExtractTRP2.h"
#include "ExtractTRPDlg.h"

#include <MutexUtils.h>
#include <UCLIDException.h>
#include <UCLIDExceptionDlg.h>
#include <cpputil.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

//-------------------------------------------------------------------------------------------------
// CExtractTRPApp
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CExtractTRPApp, CWinApp)
	ON_COMMAND(ID_HELP, &CWinApp::OnHelp)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CExtractTRPApp construction
//-------------------------------------------------------------------------------------------------
CExtractTRPApp::CExtractTRPApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}

//-------------------------------------------------------------------------------------------------
// The one and only CExtractTRPApp object
//-------------------------------------------------------------------------------------------------
CExtractTRPApp theApp;

//-------------------------------------------------------------------------------------------------
// Local function
//-------------------------------------------------------------------------------------------------
void usage()
{
	string strUsage = "This application does not require arguments:\n";
		strUsage += "ExtractTRP2.exe [OPTIONS]\n";
		strUsage += "OPTIONS:\n";
		strUsage += "/exit	Exit - closes an open instance of the EXE\n";
		strUsage += "/?	Help - displays this usage message\n";
		AfxMessageBox(strUsage.c_str());
}

//-------------------------------------------------------------------------------------------------
// CExtractTRPApp initialization
//-------------------------------------------------------------------------------------------------
BOOL CExtractTRPApp::InitInstance()
{
	try
	{
		// Retrieve command-line settings
		string strArg = m_lpCmdLine;
		makeLowerCase(strArg);

		if (strArg != "" && strArg != "/exit")
		{
			usage();
			return FALSE;
		}

		// reset the last error code before calling the create mutex function
		// to ensure that the error code from GetLastError came from the call
		// to create mutex
		SetLastError(0);

		// Check for auto-exit option (P13 #4414)
		if (strArg == "/exit" )
		{
			// only need to get the window for the running instance if we are trying
			// to close it
			// Locate the currently running instance, if any
			HWND hwndCurrent = ::FindWindow( MAKEINTATOM(32770), gstrTRP_WINDOW_TITLE.c_str() );

			// if we didn't find the window handle on the first attempt it may be that the
			// instance is still initializing, sleep for 3 seconds and try again
			int sleepCount = 0;
			while (hwndCurrent == NULL && sleepCount < 3000)
			{
				Sleep(500);
				sleepCount += 500;
				hwndCurrent = ::FindWindow( MAKEINTATOM(32770), gstrTRP_WINDOW_TITLE.c_str() );
			}

			// Ask current instance to close (P13 #4354)
			if (hwndCurrent != NULL)
			{
				PostMessage( hwndCurrent, WM_CLOSE, 0, 0 );
			}

			// Exit this instance every time for the /exit option
			return FALSE;
		}

		// create a new named mutex (P13 #4690)
		unique_ptr<CMutex> pRunning(getGlobalNamedMutex(gpszTrpRunning));
		ASSERT_RESOURCE_ALLOCATION("ELI32539", pRunning.get() != __nullptr);
		CSingleLock lRunning(pRunning.get(), FALSE);

		// Only allow one instance of ExtractTRP2 (P13 #4353) and (P13 #4690)
		if (lRunning.Lock(1000) == TRUE)
		{
			// Add exception handling
			static UCLIDExceptionDlg exceptionDlg;
			UCLIDException::setExceptionHandler( &exceptionDlg );

			// InitCommonControlsEx() is required on Windows XP if an application
			// manifest specifies use of ComCtl32.dll version 6 or later to enable
			// visual styles.  Otherwise, any window creation will fail.
			INITCOMMONCONTROLSEX InitCtrls;
			InitCtrls.dwSize = sizeof(InitCtrls);
			// Set this to include all the common control classes you want to use
			// in your application.
			InitCtrls.dwICC = ICC_WIN95_CLASSES;
			InitCommonControlsEx(&InitCtrls);

			CWinApp::InitInstance();

			AfxEnableControlContainer();

			// Standard initialization
			// If you are not using these features and wish to reduce the size
			// of your final executable, you should remove from the following
			// the specific initialization routines you do not need.
			// Change the registry key under which our settings are stored.
			SetRegistryKey(_T("Extract Systems TRP Application"));

			CExtractTRPDlg dlg;
			m_pMainWnd = &dlg;
			dlg.DoModal();

			// Unlock as soon as the dialog exits
			lRunning.Unlock();
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI15480");

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return FALSE;
}
//-------------------------------------------------------------------------------------------------
