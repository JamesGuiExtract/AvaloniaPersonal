// TestLicensing.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "TestLicensing.h"
#include "TestLicensingDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CTestLicensingApp

BEGIN_MESSAGE_MAP(CTestLicensingApp, CWinApp)
	//{{AFX_MSG_MAP(CTestLicensingApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG
	ON_COMMAND(ID_HELP, CWinApp::OnHelp)
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CTestLicensingApp construction

CTestLicensingApp::CTestLicensingApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}

/////////////////////////////////////////////////////////////////////////////
// The one and only CTestLicensingApp object

CTestLicensingApp theApp;

/////////////////////////////////////////////////////////////////////////////
// CTestLicensingApp initialization

BOOL CTestLicensingApp::InitInstance()
{
	// Standard initialization
	// If you are not using these features and wish to reduce the size
	//  of your final executable, you should remove from the following
	//  the specific initialization routines you do not need.

#ifdef _AFXDLL
	Enable3dControls();			// Call this when using MFC in a shared DLL
#else
	Enable3dControlsStatic();	// Call this when linking to MFC statically
#endif

	// Check for command line argument to allow 
	// MessageBoxes to further display error information
	bool	bShowMessages = false;
	if (__argc == 2)
	{
		if (strcmpi(__argv[1], "/v") == 0)
		{
			// Verbose option
			bShowMessages = true;
		}
	}

	CTestLicensingDlg dlg( bShowMessages );
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

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return FALSE;
}
