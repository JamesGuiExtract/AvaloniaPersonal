// ChangeWindowTitle.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "ChangeWindowTitle.h"
#include "ChangeWindowTitleDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CChangeWindowTitleApp

BEGIN_MESSAGE_MAP(CChangeWindowTitleApp, CWinApp)
	//{{AFX_MSG_MAP(CChangeWindowTitleApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG
	ON_COMMAND(ID_HELP, CWinApp::OnHelp)
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CChangeWindowTitleApp construction

CChangeWindowTitleApp::CChangeWindowTitleApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}

/////////////////////////////////////////////////////////////////////////////
// The one and only CChangeWindowTitleApp object

CChangeWindowTitleApp theApp;

/////////////////////////////////////////////////////////////////////////////
// CChangeWindowTitleApp initialization

BOOL CChangeWindowTitleApp::InitInstance()
{
	AfxEnableControlContainer();

	CChangeWindowTitleDlg dlg;
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
