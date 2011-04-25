// CleanupImage.cpp : Defines the class behaviors for the application.
//
#include "stdafx.h"
#include "CleanupImage.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <UCLIDExceptionDlg.h>
#include <cpputil.h>
#include <ComponentLicenseIDs.h>

#include <string>
#include <vector>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// CCleanupImageApp
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CCleanupImageApp, CWinApp)
	ON_COMMAND(ID_HELP, &CWinApp::OnHelp)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CCleanupImageApp construction
//-------------------------------------------------------------------------------------------------
CCleanupImageApp::CCleanupImageApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}

//-------------------------------------------------------------------------------------------------
// The one and only CCleanupImageApp object
//-------------------------------------------------------------------------------------------------
CCleanupImageApp theApp;

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void usage()
{
	string strUsage = "This application has 3 required arguments and 1 optional argument:\n";
	strUsage += "  - An input image file to clean\n"
				"  - An output image file name for the cleaned image\n"
				"  - An image cleanup settings file (.ics | .ics.etf)\n"
				"  - The optional argument (/ef <filename>) fully specifies the location \n"
				"    of an exception log that will store any thrown exception.  Without \n"
				"    an exception log, any thrown exception will be displayed.\n\n";
	strUsage += "Usage:\n";
	strUsage += "CleanupImage.exe <strInputFile> <strOutputFile> <strSettings> [/ef <filename>]\n"
				"where:\n"
				" - <strSettings> is an image cleanup settings file (.ics | .ics.etf)\n"
				" - <filename> is the fully qualified path to an exception log\n\n";
	AfxMessageBox(strUsage.c_str());
}
//-------------------------------------------------------------------------------------------------
void validateLicense()
{
	VALIDATE_LICENSE(gnIMAGE_CLEANUP_ENGINE_FEATURE, "ELI17834", "Image Cleanup Settings Editor" );
}

//-------------------------------------------------------------------------------------------------
// CleanupImage Main
//-------------------------------------------------------------------------------------------------
BOOL CCleanupImageApp::InitInstance()
{
	// define empty string for local exception log
	string strLocalExceptionLog = "";

	try
	{
		try
		{
			// InitCommonControlsEx() is required on Windows XP if an application
			// manifest specifies use of ComCtl32.dll version 6 or later to enable
			// visual styles.  Otherwise, any window creation will fail.
			INITCOMMONCONTROLSEX InitCtrls;
			InitCtrls.dwSize = sizeof(InitCtrls);
			// Set this to include all the common control classes you want to use
			// in your application.
			InitCtrls.dwICC = ICC_WIN95_CLASSES;
			InitCommonControlsEx(&InitCtrls);

			CWinApp::InitInstance();

			CoInitializeEx(NULL, COINIT_APARTMENTTHREADED);

			// scope so that COM objects will be destroyed before call to CoUninitialize
			{
				// Setup exception handling
				UCLIDExceptionDlg exceptionDlg;
				UCLIDException::setExceptionHandler( &exceptionDlg );

				// check for proper number of arguments
				// CleanupImage.exe <in> <out> <settings> [/ef <log>]
				if (__argc != 4 && __argc != 6)
				{
					usage();
					return FALSE;
				}

				// get input file from command line
				string strInputFile = buildAbsolutePath(string(__argv[1]));

				// get output file from command line
				string strOutputFile = buildAbsolutePath(string(__argv[2]));

				// get settings file from command line
				string strSettingsFile = buildAbsolutePath(string(__argv[3]));

				// check for /ef (exception logging)
				if (__argc == 6)
				{
					// retrieve the flag
					string strFlag(__argv[4]);
					makeLowerCase(strFlag);

					if (strFlag != "/ef")
					{
						usage();
						return FALSE;
					}

					// get the exception log file from command line
					strLocalExceptionLog = __argv[5];
				}

				// check to make sure the input image and settings files exist
				validateFileOrFolderExistence(strInputFile);
				validateFileOrFolderExistence(strSettingsFile);

				// init license management
				LicenseManagement::loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);

				// check license
				validateLicense();

				// create an ImageCleanupEngine
				ESImageCleanupLib::IImageCleanupEnginePtr 
					ipCleanupEngine(CLSID_ImageCleanupEngine);
				ASSERT_RESOURCE_ALLOCATION("ELI17837", ipCleanupEngine != __nullptr);

				// create an ImageCleanupSettings object
				ESImageCleanupLib::IImageCleanupSettingsPtr
					ipSettings(CLSID_ImageCleanupSettings);
				ASSERT_RESOURCE_ALLOCATION("ELI17838", ipSettings != __nullptr);

				// load the image cleanup settings
				ipSettings->LoadFrom(strSettingsFile.c_str(), VARIANT_FALSE);

				// perform the image cleanup
				ipCleanupEngine->CleanupImageInternalUseOnly(strInputFile.c_str(),
					strOutputFile.c_str(), ipSettings);
			}

			CoUninitialize();
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI17839");
	}
	catch(UCLIDException ue)
	{
		// handle the exception log
		if (strLocalExceptionLog.empty())
		{
			// no log file so display the exception
			ue.display();
		}
		else
		{
			// log the exception without displaying it and notify FDRS
			ue.log(strLocalExceptionLog, true);
		}
	}
	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return FALSE;
}
//-------------------------------------------------------------------------------------------------