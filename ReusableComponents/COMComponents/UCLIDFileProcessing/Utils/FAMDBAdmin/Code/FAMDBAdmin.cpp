// FAMDBAdmin.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "FAMDBAdmin.h"
#include "FAMDBAdminDlg.h"
#include "SelectDBDialog.h"
#include "FAMDBAdminDlg.h"

#include <LicenseMgmt.h>
#include <UCLIDException.h>
#include <UCLIDExceptionDlg.h>
#include <ComponentLicenseIDs.h>
#include <cpputil.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
//Local Functions
//-------------------------------------------------------------------------------------------------
void usage()
{
	string strUsage = "This application can be used with two optional arguments,\n";
		strUsage += "Usage:\n";
		strUsage += "FAMDBAdmin.exe [<Server> <Database>]\n";
		AfxMessageBox(strUsage.c_str());
}

//-------------------------------------------------------------------------------------------------
// CFAMDBAdminApp
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CFAMDBAdminApp, CWinApp)
	ON_COMMAND(ID_HELP, &CWinApp::OnHelp)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CFAMDBAdminApp construction
//-------------------------------------------------------------------------------------------------
CFAMDBAdminApp::CFAMDBAdminApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}

//-------------------------------------------------------------------------------------------------
// The one and only CFAMDBAdminApp object
//-------------------------------------------------------------------------------------------------
CFAMDBAdminApp theApp;

//-------------------------------------------------------------------------------------------------
// CFAMDBAdminApp initialization
//-------------------------------------------------------------------------------------------------
BOOL CFAMDBAdminApp::InitInstance()
{
	// COM has to be initialized because the license management code uses
	// COM objects (ExtractTRP)
	CoInitializeEx(NULL, COINIT_MULTITHREADED);

	try
	{
		// Display usage if not a valid number of arguments specified
		if (__argc != 1 && __argc != 3)
		{
			usage();
			return FALSE;
		}

		// set the UCLIDExceptionViewer as the default exception handler
		static UCLIDExceptionDlg exceptionDlg;
		UCLIDException::setExceptionHandler(&exceptionDlg);

		// InitCommonControlsEx() is required on Windows XP if an application
		// manifest specifies use of ComCtl32.dll version 6 or later to enable
		// visual styles.  Otherwise, any window creation will fail.
		INITCOMMONCONTROLSEX InitCtrls;
		InitCtrls.dwSize = sizeof(InitCtrls);
		// Set this to include all the common control classes you want to use
		// in your application.
		InitCtrls.dwICC = ICC_WIN95_CLASSES;
		InitCommonControlsEx(&InitCtrls);

		// Load license file(s)
		LicenseManagement::loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);
		validateLicense();

		CWinApp::InitInstance();

		// Standard initialization
		// If you are not using these features and wish to reduce the size
		// of your final executable, you should remove from the following
		// the specific initialization routines you do not need
		// Change the registry key under which our settings are stored
		// TODO: You should modify this string to be something appropriate
		// such as the name of your company or organization
		//SetRegistryKey(_T("Local AppWizard-Generated Applications"));

		// Create a database object
		IFileProcessingDBPtr ipFAMDB(CLSID_FileProcessingDB);
		ASSERT_RESOURCE_ALLOCATION("ELI17526", ipFAMDB != __nullptr);

		// Check for Server and database selected
		if ( __argc == 3)
		{
			// Catch exceptions and display then just let the select dialog box be opened with the 
			// same data
			try
			{
				// First arg is server
				ipFAMDB->DatabaseServer = __argv[1];

				// Second arg is the database
				ipFAMDB->DatabaseName = __argv[2];
 
				VARIANT_BOOL bLoginCanceled = VARIANT_FALSE;
				VARIANT_BOOL bLoginValid = VARIANT_FALSE;

				// Attempt to login to the database
				bLoginValid = ipFAMDB->ShowLogin(VARIANT_TRUE, &bLoginCanceled);

				if (asCppBool(bLoginValid))
				{
					// Construct and display dialog
					CFAMDBAdminDlg dlg(ipFAMDB);
					if ( dlg.DoModal() == IDCANCEL )
					{
						// Exit the App
						return FALSE;
					}
				}
				else if (!asCppBool(bLoginCanceled))
				{
					::MessageBox(NULL, "Login failed.\r\n\r\nPlease ensure you are using the correct password and try again.", 
						"Login failed", MB_OK | MB_ICONERROR );
				}
			}
			CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17608");
		}

		// Display the initial dialog box
		SelectDBDialog dlgSelectDB(ipFAMDB);
		
		// Display the Select DB dialog
		dlgSelectDB.DoModal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14890")

	// Removing the CoUninitialize call to fix [#LRCAU #5721]
	//CoUninitialize();

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return FALSE;
}

//-------------------------------------------------------------------------------------------------
// Private Functions
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminApp::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFILE_ACTION_MANAGER_OBJECTS;

	VALIDATE_LICENSE( THIS_COMPONENT_ID, "ELI14876", "FAM DB Admin" );
}
//-------------------------------------------------------------------------------------------------
