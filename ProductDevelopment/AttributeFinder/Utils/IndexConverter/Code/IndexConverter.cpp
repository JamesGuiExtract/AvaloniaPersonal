// IndexConverter.cpp : Defines the class behaviors for the application.
//
#include "stdafx.h"
#include "IndexConverter.h"
#include "IndexConverterDlg.h"

#include <UCLIDExceptionDlg.h>
#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// CIndexConverterApp
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CIndexConverterApp, CWinApp)
	//{{AFX_MSG_MAP(CIndexConverterApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG
	ON_COMMAND(ID_HELP, CWinApp::OnHelp)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CIndexConverterApp construction
//-------------------------------------------------------------------------------------------------
CIndexConverterApp::CIndexConverterApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}

//-------------------------------------------------------------------------------------------------
// The one and only CIndexConverterApp object
//-------------------------------------------------------------------------------------------------
CIndexConverterApp theApp;

//-------------------------------------------------------------------------------------------------
// CIndexConverterApp initialization
//-------------------------------------------------------------------------------------------------
BOOL CIndexConverterApp::InitInstance()
{
	AfxEnableControlContainer();

	CoInitializeEx(NULL, COINIT_MULTITHREADED);
	try
	{
		// Set up the exception handling aspect.
		static UCLIDExceptionDlg exceptionDlg;
		UCLIDException::setExceptionHandler(&exceptionDlg);

		// init license
		LicenseManagement::loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);
		
		// Check license
		validateLicense();

		// Create and run the dialog
		CIndexConverterDlg dlg;
		m_pMainWnd = &dlg;
		dlg.DoModal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06953");

	CoUninitialize();
	// Since the dialog has been closed, return FALSE so that we exit the
	// application, rather than start the application's message pump.
	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterApp::validateLicense()
{
	static const unsigned long INDEXCONVERTER_ID = gnRULE_DEVELOPMENT_TOOLKIT_OBJECTS;

	VALIDATE_LICENSE( INDEXCONVERTER_ID, "ELI07100", "Index Converter" );
}
//-------------------------------------------------------------------------------------------------
