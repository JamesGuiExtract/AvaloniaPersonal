// DetectAndReportFailure.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "DetectAndReportFailure.h"
#include "DetectAndReportFailureDlg.h"

#include <LicenseMgmt.h>
#include <UCLIDExceptionDlg.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

/////////////////////////////////////////////////////////////////////////////
// CDetectAndReportFailureApp

BEGIN_MESSAGE_MAP(CDetectAndReportFailureApp, CWinApp)
	//{{AFX_MSG_MAP(CDetectAndReportFailureApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG
	ON_COMMAND(ID_HELP, CWinApp::OnHelp)
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CDetectAndReportFailureApp construction

CDetectAndReportFailureApp::CDetectAndReportFailureApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}

/////////////////////////////////////////////////////////////////////////////
// The one and only CDetectAndReportFailureApp object

CDetectAndReportFailureApp theApp;

/////////////////////////////////////////////////////////////////////////////
// CDetectAndReportFailureApp initialization

BOOL CDetectAndReportFailureApp::InitInstance()
{
	AfxEnableControlContainer();

	CoInitializeEx(NULL, COINIT_MULTITHREADED);

	try
	{
		// load license files
		LicenseManagement::loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);

		static UCLIDExceptionDlg exceptionDlg;
		UCLIDException::setExceptionHandler(&exceptionDlg);

		CDetectAndReportFailureDlg dlg;
		m_pMainWnd = &dlg;
		dlg.DoModal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15684")

	CoUninitialize();

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return FALSE;
}
