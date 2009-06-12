// GetNextELICode.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "GetNextELICode.h"
#include "GetNextELICodeDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CGetNextELICodeApp

BEGIN_MESSAGE_MAP(CGetNextELICodeApp, CWinApp)
	//{{AFX_MSG_MAP(CGetNextELICodeApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG
	ON_COMMAND(ID_HELP, CWinApp::OnHelp)
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CGetNextELICodeApp construction

CGetNextELICodeApp::CGetNextELICodeApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}

/////////////////////////////////////////////////////////////////////////////
// The one and only CGetNextELICodeApp object

CGetNextELICodeApp theApp;

/////////////////////////////////////////////////////////////////////////////
// CGetNextELICodeApp initialization

BOOL CGetNextELICodeApp::InitInstance()
{
	CGetNextELICodeDlg dlg;
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

BOOL CGetNextELICodeApp::OnCmdMsg(UINT nID, int nCode, void* pExtra, AFX_CMDHANDLERINFO* pHandlerInfo) 
{
	// TODO: Add your specialized code here and/or call the base class
	
	return CWinApp::OnCmdMsg(nID, nCode, pExtra, pHandlerInfo);
}
