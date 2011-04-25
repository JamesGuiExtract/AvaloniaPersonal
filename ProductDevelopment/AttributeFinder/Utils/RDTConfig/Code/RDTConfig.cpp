// RDTConfig.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "RDTConfig.h"
#include "RDTConfigDlg.h"

#include <LicenseMgmt.h>
#include <UCLIDExceptionDlg.h>
#include <UCLIDException.h>
#include <ComponentLicenseIDs.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// Local function
//-------------------------------------------------------------------------------------------------
void validateLicense()
{
	static const unsigned long THIS_APP_ID = gnRULE_DEVELOPMENT_TOOLKIT_OBJECTS;

	VALIDATE_LICENSE(THIS_APP_ID, "ELI08820", "RDTConfig EXE" );
}

//-------------------------------------------------------------------------------------------------
// CRDTConfigApp
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CRDTConfigApp, CWinApp)
	//{{AFX_MSG_MAP(CRDTConfigApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG
	ON_COMMAND(ID_HELP, CWinApp::OnHelp)
END_MESSAGE_MAP()
//-------------------------------------------------------------------------------------------------
CRDTConfigApp::CRDTConfigApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}

//-------------------------------------------------------------------------------------------------
// The one and only CRDTConfigApp object
//-------------------------------------------------------------------------------------------------
CRDTConfigApp theApp;

//-------------------------------------------------------------------------------------------------
// CRDTConfigApp initialization
//-------------------------------------------------------------------------------------------------
BOOL CRDTConfigApp::InitInstance()
{
	AfxEnableControlContainer();

	try
	{
		CoInitializeEx(NULL, COINIT_MULTITHREADED);
		{
			// Set up the exception handling aspect
			static UCLIDExceptionDlg exceptionDlg;
			UCLIDException::setExceptionHandler( &exceptionDlg );			

			// Initialize and validate license
			LicenseManagement::loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);
			validateLicense();

			// Construct and display the dialog
			CRDTConfigDlg dlg;
			m_pMainWnd = &dlg;
			dlg.DoModal();
		}
		
		// This is commented out to fix FlexIDSCore #2256 - so that RDTConfig does not
		// cause an MFC exception when it exits
		//CoUninitialize();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07697")

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return FALSE;
}
//-------------------------------------------------------------------------------------------------
