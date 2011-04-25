// ConvertFPSFile.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "ConvertFPSFile.h"
#include "ConvertFPSFileDlg.h"

#include <LicenseMgmt.h>
#include <UCLIDExceptionDlg.h>
#include <UCLIDException.h>
#include <ComponentLicenseIDs.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// CConvertFPSFileApp
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CConvertFPSFileApp, CWinApp)
	ON_COMMAND(ID_HELP, &CWinApp::OnHelp)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CConvertFPSFileApp construction
//-------------------------------------------------------------------------------------------------
CConvertFPSFileApp::CConvertFPSFileApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}

//-------------------------------------------------------------------------------------------------
// The one and only CConvertFPSFileApp object
//-------------------------------------------------------------------------------------------------
CConvertFPSFileApp theApp;

//-------------------------------------------------------------------------------------------------
void validateLicense()
{
	static const unsigned long THIS_APP_ID = gnFILE_ACTION_MANAGER_OBJECTS;

	VALIDATE_LICENSE(THIS_APP_ID, "ELI14164", "ConvertFPSFile" );
}

//-------------------------------------------------------------------------------------------------
// CConvertFPSFileApp initialization
//-------------------------------------------------------------------------------------------------
BOOL CConvertFPSFileApp::InitInstance()
{
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

	try
	{
		CoInitializeEx(NULL, COINIT_MULTITHREADED);
		{
			// Set up the exception handling aspect.
			static UCLIDExceptionDlg exceptionDlg;
			UCLIDException::setExceptionHandler( &exceptionDlg );			

			// Initialize the license
			LicenseManagement::loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);

			try
			{
				// Make sure that the application is licensed
				validateLicense();
				
				// Create and display the dialog
				CConvertFPSFileDlg dlg;
				m_pMainWnd = &dlg;
				dlg.DoModal();
			}
			CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15792")
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14165")

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return FALSE;
}
//-------------------------------------------------------------------------------------------------
