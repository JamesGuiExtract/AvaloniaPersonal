// USBLicenseKeyManager.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "USBLicenseKeyManager.h"
#include "USBLicenseKeyManagerDlg.h"


#include <UCLIDExceptionDlg.h>
#include <UCLIDException.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CUSBLicenseKeyManagerApp

BEGIN_MESSAGE_MAP(CUSBLicenseKeyManagerApp, CWinApp)
	//{{AFX_MSG_MAP(CUSBLicenseKeyManagerApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG
	ON_COMMAND(ID_HELP, CWinApp::OnHelp)
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CUSBLicenseKeyManagerApp construction

CUSBLicenseKeyManagerApp::CUSBLicenseKeyManagerApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}

/////////////////////////////////////////////////////////////////////////////
// The one and only CUSBLicenseKeyManagerApp object

CUSBLicenseKeyManagerApp theApp;

/////////////////////////////////////////////////////////////////////////////
// CUSBLicenseKeyManagerApp initialization

BOOL CUSBLicenseKeyManagerApp::InitInstance()
{
	AfxEnableControlContainer();

	CoInitializeEx(NULL, COINIT_MULTITHREADED);
	// Set up the exception handling aspect.
	static UCLIDExceptionDlg exceptionDlg;
	UCLIDException::setExceptionHandler( &exceptionDlg );
	
	try
	{
		CUSBLicenseKeyManagerDlg dlg;
		m_pMainWnd = &dlg;
		int nResponse = dlg.DoModal();
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11321");

	// Work around for problem of error being displayed after exit.
	//CoUninitialize();
		
	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return FALSE;
}
