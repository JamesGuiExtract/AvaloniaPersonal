// ConvertFAMDB.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "ConvertFAMDB.h"
#include "ConvertFAMDBDlg.h"

#include <LicenseMgmt.h>
#include <UCLIDException.h>
#include <UCLIDExceptionDlg.h>
#include <ComponentLicenseIDs.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// CConvertFAMDBApp Message Map
//-------------------------------------------------------------------------------------------------

BEGIN_MESSAGE_MAP(CConvertFAMDBApp, CWinApp)
	ON_COMMAND(ID_HELP, &CWinApp::OnHelp)
END_MESSAGE_MAP()


//-------------------------------------------------------------------------------------------------
// CConvertFAMDBApp
//-------------------------------------------------------------------------------------------------
CConvertFAMDBApp::CConvertFAMDBApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}

//-------------------------------------------------------------------------------------------------
// The one and only CConvertFAMDBApp object
//-------------------------------------------------------------------------------------------------
CConvertFAMDBApp theApp;

//-------------------------------------------------------------------------------------------------
// CConvertFAMDBApp initialization
//-------------------------------------------------------------------------------------------------
BOOL CConvertFAMDBApp::InitInstance()
{
	CoInitializeEx(NULL, COINIT_MULTITHREADED);

	try
	{
		// Set up the exception handling aspect.
		static UCLIDExceptionDlg exceptionDlg;
		UCLIDException::setExceptionHandler(&exceptionDlg);

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

		// Standard initialization
		// If you are not using these features and wish to reduce the size
		// of your final executable, you should remove from the following
		// the specific initialization routines you do not need
		// Change the registry key under which our settings are stored
		// TODO: You should modify this string to be something appropriate
		// such as the name of your company or organization
		//SetRegistryKey(_T("Local AppWizard-Generated Applications"));

		// Load license file(s)
		LicenseManagement::sGetInstance().loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);

		// Validate the license [LRCAU #5277]
		validateLicense();

		CConvertFAMDBDlg dlg;
		m_pMainWnd = &dlg;
		INT_PTR nResponse = dlg.DoModal();
		if (nResponse == IDOK)
		{
			// TODO: Place code here to handle when the dialog is
			//  dismissed with OK
		}
		else if (nResponse == IDCANCEL)
		{
			// TODO: Place code here to handle when the dialog is
			//  dismissed with Cancel
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19887");

	// This is commented out because there is a problem with active COM objects
	// that cause this to display an error message on exit
	//CoUninitialize();

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return FALSE;
}
//-------------------------------------------------------------------------------------------------
// Private Functions
//-------------------------------------------------------------------------------------------------
void CConvertFAMDBApp::validateLicense()
{
	// [LRCAU #5783] - Require server license
	VALIDATE_LICENSE( gnFLEXINDEX_IDSHIELD_SERVER_CORE, "ELI20158", "Convert FAM Database" );
}
//-------------------------------------------------------------------------------------------------
