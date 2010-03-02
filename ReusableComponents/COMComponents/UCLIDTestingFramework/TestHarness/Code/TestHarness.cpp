// TestHarness.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "TestHarness.h"
#include "TestHarnessDlg.h"

#include <LicenseMgmt.h>
#include <UCLIDExceptionDlg.h>
#include <UCLIDException.h>
#include <ComponentLicenseIDs.h>
#include <cpputil.h>

#include <io.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// CTestHarnessApp
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CTestHarnessApp, CWinApp)
	//{{AFX_MSG_MAP(CTestHarnessApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG
	ON_COMMAND(ID_HELP, CWinApp::OnHelp)
END_MESSAGE_MAP()
//-------------------------------------------------------------------------------------------------
CTestHarnessApp::CTestHarnessApp()
{
}
//-------------------------------------------------------------------------------------------------
CTestHarnessApp::~CTestHarnessApp()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16504");
}

//-------------------------------------------------------------------------------------------------
// The one and only CTestHarnessApp object
//-------------------------------------------------------------------------------------------------
CTestHarnessApp theApp;

//-------------------------------------------------------------------------------------------------
// Local functions
//-------------------------------------------------------------------------------------------------
void printUsage()
{
	string strMsg = "This application takes 0, 1 or 2 arguments (not necessarily in the order as displayed) :\r\n"
					"- /f<TCL File Name>. eg. /f\"C:\\Program Files\\ABC\\TCL\\MyFile.tcl.\"";

	throw UCLIDException("ELI07354", strMsg);
}
//-------------------------------------------------------------------------------------------------
void checkLicensing()
{
	// Load license file(s) with default passwords
	LicenseManagement::sGetInstance().loadLicenseFilesFromFolder( LICENSE_MGMT_PASSWORD,
		gnDEFAULT_PASSWORDS );

	// Check the license one last time
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI21334", "Test Harness" );
}

//-------------------------------------------------------------------------------------------------
// CTestHarnessApp initialization
//-------------------------------------------------------------------------------------------------
BOOL CTestHarnessApp::InitInstance()
{
	AfxEnableControlContainer();

	// Standard initialization
	// If you are not using these features and wish to reduce the size
	//  of your final executable, you should remove from the following
	//  the specific initialization routines you do not need.

#ifdef _AFXDLL
	// TESTTHIS commenting out enable3d
//	Enable3dControls();			// Call this when using MFC in a shared DLL
#else
	Enable3dControlsStatic();	// Call this when linking to MFC statically
#endif

	CoInitializeEx(NULL, COINIT_MULTITHREADED);

	{	
		try
		{
			// Set up UCLID exception handling
			static UCLIDExceptionDlg exceptionDlg;
			UCLIDException::setExceptionHandler( &exceptionDlg );

			// Initialize licenses
			checkLicensing();

			int nNumOfArguments = __argc;
			if (nNumOfArguments != 1 
				&& nNumOfArguments != 2 
				&& nNumOfArguments != 3)
			{
				printUsage();
			}

			string strTCLFileName("");
			string strTestFileRootFolder("");
			for (int n=1; n<nNumOfArguments; n++)
			{
				string strArgument = __argv[n];
				if (strArgument.find("/f") == 0)
				{
					strTCLFileName = strArgument.substr(2);
					// trim off any leading or trailing quotes
					strTCLFileName = ::trim(strTCLFileName, "\"", "\"");
					// make sure it exists
					if (!::isFileOrFolderValid(strTCLFileName))
					{
						strTCLFileName = "";
					}
				}
				else
				{
					printUsage();
				}
			}
				
			// Display the Test Harness dialog
			CTestHarnessDlg dlg;
			if (!strTCLFileName.empty())
			{
				dlg.m_zFilename = strTCLFileName.c_str();
			}
				
			m_pMainWnd = &dlg;
			dlg.DoModal();
		}
		CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07298")
	}
	
	CoUninitialize();

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return FALSE;
}
//-------------------------------------------------------------------------------------------------
