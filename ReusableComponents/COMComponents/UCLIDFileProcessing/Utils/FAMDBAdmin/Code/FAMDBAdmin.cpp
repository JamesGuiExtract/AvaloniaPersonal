// FAMDBAdmin.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "FAMDBAdmin.h"
#include "FAMDBAdminDlg.h"
#include "FAMDBAdminDlg.h"

#include <LicenseMgmt.h>
#include <UCLIDException.h>
#include <UCLIDExceptionDlg.h>
#include <ComponentLicenseIDs.h>
#include <COMUtils.h>
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
	string strUsage = "This application can be used with two or three optional arguments,\n";
		strUsage += "Usage:\n";
		strUsage += "FAMDBAdmin.exe [<Server> <Database> [<AdvancedConnectionStringProperties]\n";
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
	// [LegacyRCAndUtils:6341]
	// This was changed to initialize as apartment threaded in April 2012 to solve a long delay
	// when initializing an Oracle DB via the entity framework. However, this change breaks the
	// schema update process which passes a progress status COM object to another thread.
	// With the /clr switch this is not required as it will be applied with the setting in the project properties
	//CoInitializeEx(NULL, COINIT_MULTITHREADED);

	try
	{
		// Display usage if not a valid number of arguments specified
		if (__argc != 1 && __argc != 3 && __argc != 4)
		{
			usage();
			return FALSE;
		}

		// set the UCLIDExceptionViewer as the default exception handler
		static UCLIDExceptionDlg exceptionDlg;
		UCLIDException::setExceptionHandler(&exceptionDlg);

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
		IFAMDBUtilsPtr ipFAMDBUtils(CLSID_FAMDBUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI34528", ipFAMDBUtils != __nullptr);

		string strProgID = ipFAMDBUtils->GetFAMDBProgId();
	
		IFileProcessingDBPtr ipFAMDB(strProgID.c_str());
		ASSERT_RESOURCE_ALLOCATION("ELI17526", ipFAMDB != __nullptr);

		// [LegacyRCAndUtils:6261]
		// The database will now retry on query timeout errors by default. However, the
		// FAMDBAdmin should not retry-- it may perform database queries that take a
		// long time to run, and retrying would mean the FAMDBAdmin could end up locked
		// for a long period of time when there is no chance a retry will every succeed.
		ipFAMDB->RetryOnTimeout = VARIANT_FALSE;

		// Check for Server and database selected
		if ( __argc >= 3)
		{
			// Catch exceptions and display then just let the select dialog box be opened with the 
			// same data
			try
			{
				// First arg is server
				ipFAMDB->DatabaseServer = __argv[1];

				// Second arg is the database
				ipFAMDB->DatabaseName = __argv[2];

				ProcessingContext context(asString(ipFAMDB->DatabaseServer), 
					asString(ipFAMDB->DatabaseName), "", 0);
				UCLIDException::SetCurrentProcessingContext(context);

				// Optional third parameter is advanced connection string properties.
				if (__argc == 4)
				{
					ipFAMDB->AdvancedConnectionStringProperties = __argv[3];
				}
 
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
					
					// Create a new FAMDB pointer so that there are not settings carried over from previous open
					ipFAMDB = __nullptr;
					ipFAMDB.CreateInstance(strProgID.c_str());
					ASSERT_RESOURCE_ALLOCATION("ELI43479", ipFAMDB != __nullptr);
				}
				else if (!asCppBool(bLoginCanceled))
				{
					::MessageBox(NULL, "Login failed.\r\n\r\nPlease ensure you are using the correct password and try again.", 
						"Login failed", MB_OK | MB_ICONERROR );
				}
			}
			CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17608");
		}

		// Display the initial database selection dialog box
		while (asCppBool(ipFAMDB->ShowSelectDB(
			"Select database to administer", VARIANT_TRUE, VARIANT_TRUE)))
		{
			// Create admin dialog
			CFAMDBAdminDlg dlg(ipFAMDB);
			if (dlg.DoModal() == IDCANCEL)
			{
				// Exit the App
				return FALSE;
			}

			// Create a new FAMDB pointer so that there are not settings carried over from previous open
			ipFAMDB = __nullptr;
			ipFAMDB.CreateInstance(strProgID.c_str());
			ASSERT_RESOURCE_ALLOCATION("ELI43478", ipFAMDB != __nullptr);

			// If CFAMDBAdminDlg's result was IDOK, the user has chosen to logout without closing;
			// the login prompt should be re-displayed.
		}
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
