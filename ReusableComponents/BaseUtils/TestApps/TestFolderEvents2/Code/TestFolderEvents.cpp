// TestFolderEvents.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "TestFolderEvents.h"
#include "TestFolderEventsDlg.h"

#include <UCLIDExceptionDlg.h>
#include <UCLIDException.hpp>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CTestFolderEventsApp

BEGIN_MESSAGE_MAP(CTestFolderEventsApp, CWinApp)
	//{{AFX_MSG_MAP(CTestFolderEventsApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG
	ON_COMMAND(ID_HELP, CWinApp::OnHelp)
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CTestFolderEventsApp construction

CTestFolderEventsApp::CTestFolderEventsApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}

/////////////////////////////////////////////////////////////////////////////
// The one and only CTestFolderEventsApp object

CTestFolderEventsApp theApp;

/////////////////////////////////////////////////////////////////////////////
// CTestFolderEventsApp initialization

BOOL CTestFolderEventsApp::InitInstance()
{
	try
	{
		// Set up the exception handling aspect.
		static UCLIDExceptionDlg exceptionDlg;
		UCLIDException::setExceptionHandler( &exceptionDlg );			
			
		CTestFolderEventsDlg dlg;
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELIWL012")

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return FALSE;
}
