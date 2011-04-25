// RuleTester.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "RuleTester.h"

#include <LicenseMgmt.h>
#include <BaseUtils.h>
#include <UCLIDExceptionDlg.h>
#include <UCLIDException.h>
#include <cpputil.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

/////////////////////////////////////////////////////////////////////////////
// CRuleTesterApp

BEGIN_MESSAGE_MAP(CRuleTesterApp, CWinApp)
	//{{AFX_MSG_MAP(CRuleTesterApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG
	ON_COMMAND(ID_HELP, CWinApp::OnHelp)
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CRuleTesterApp construction

CRuleTesterApp::CRuleTesterApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}

/////////////////////////////////////////////////////////////////////////////
// The one and only CRuleTesterApp object

CRuleTesterApp theApp;

/////////////////////////////////////////////////////////////////////////////
// CRuleTesterApp initialization

BOOL CRuleTesterApp::InitInstance()
{
	//CoInitializeEx(NULL, COINIT_MULTITHREADED);
	// This is being used instead of multithreaded version because
	// This app uses the Spot Recognition Window that uses an OCX
	// that will not work with the multithreaded option
	CoInitializeEx(NULL, COINIT_APARTMENTTHREADED);

	try
	{
		// Set up the exception handling aspect.
		static UCLIDExceptionDlg exceptionDlg;
		UCLIDException::setExceptionHandler( &exceptionDlg );

		// Load license file(s)
		LicenseManagement::loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);

		// Create the IRuleTesterUI object
		IRuleTesterUIPtr	ipRuleTesterUI(CLSID_RuleTesterUI);
		ASSERT_RESOURCE_ALLOCATION("ELI08814", ipRuleTesterUI != __nullptr );

		// get the command line and see if there was an argument provided.
		string strFileName = "";
		if (__argc >= 2)
		{
			strFileName = __argv[1];
			// Get the full path with file name
			strFileName = getAbsoluteFileName( strFileName, strFileName, true);
		}

		// Show the UI
		ipRuleTesterUI->ShowUI(_bstr_t(strFileName.c_str()));

		// delete the singleton input manager instance, which
		// may be in existence
		IInputManagerSingletonPtr ipInputMgrSingleton(CLSID_InputManagerSingleton);
		ASSERT_RESOURCE_ALLOCATION("ELI15791", ipInputMgrSingleton != __nullptr );
		ipInputMgrSingleton->DeleteInstance();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08802");

	// Calling CoUninitialize here causes an unexpected win32 error on exit. [P16 #2469]
	// Commenting it out is a temporary fix. 
	//CoUninitialize();

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return FALSE;
}
