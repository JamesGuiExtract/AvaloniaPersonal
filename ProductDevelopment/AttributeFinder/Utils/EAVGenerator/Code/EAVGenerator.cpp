// EAVGenerator.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "EAVGenerator.h"
#include "EAVGeneratorDlg.h"

#include <LicenseMgmt.h>
#include <UCLIDExceptionDlg.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <Win32Util.h>
#include <ComponentLicenseIDs.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

CComModule _Module;
const char *gpszEAVFileDescription = "Extract Systems EAV File";
const char *gpszEAVFileExtension = ".eav";
const char *gpszVOAFileDescription = "Extract Systems VOA File";
const char *gpszVOAFileExtension = ".voa";
const char *gpszEVOAFileDescription = "Extract Systems Expected VOA File";
const char *gpszEVOAFileExtension = ".evoa";

// add license management function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

/////////////////////////////////////////////////////////////////////////////
// CEAVGeneratorApp

BEGIN_MESSAGE_MAP(CEAVGeneratorApp, CWinApp)
	//{{AFX_MSG_MAP(CEAVGeneratorApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG
	ON_COMMAND(ID_HELP, CWinApp::OnHelp)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CEAVGeneratorApp construction

CEAVGeneratorApp::CEAVGeneratorApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}

//-------------------------------------------------------------------------------------------------
// The one and only CEAVGeneratorApp object

CEAVGeneratorApp theApp;

//-------------------------------------------------------------------------------------------------
void validateLicense()
{
	static const unsigned long THIS_APP_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_APP_ID, "ELI07384", "EAV Generator" );
}

//-------------------------------------------------------------------------------------------------
// CEAVGeneratorApp initialization

BOOL CEAVGeneratorApp::InitInstance()
{
	AfxEnableControlContainer();

	try
	{
		//CoInitializeEx(NULL, COINIT_MULTITHREADED);
		// This is being used instead of multithreaded version because
		// This app uses the Spot Recognition Window that uses an OCX
		// that will not work with the multithreaded option
		CoInitializeEx(NULL, COINIT_APARTMENTTHREADED);
		{
			// Set up the exception handling aspect.
			static UCLIDExceptionDlg exceptionDlg;
			UCLIDException::setExceptionHandler( &exceptionDlg );			
			
			// Every time this application starts, re-register the file
			// associations if the file associations don't exist.  If the
			// file associations already exist, then do nothing.
			// This way, registration will happen the very first time
			// the user runs this application (even if the installation
			// program's call to this application with /r argument failed).
			// NOTE: the registration is not forced because we are passing
			// "true" for bSkipIfKeysExist.
			registerFileAssociations(gpszEAVFileExtension, gpszEAVFileDescription, 
				getAppFullPath(), true);
			registerFileAssociations(gpszVOAFileExtension, gpszVOAFileDescription, 
				getAppFullPath(), true);
			registerFileAssociations(gpszEVOAFileExtension, gpszEVOAFileDescription, 
				getAppFullPath(), true);
			
			// if appropriate command line arguments have been provided
			// register or unregister RSD file related settings
			// as appropriate, and return
			if (__argc == 2)
			{
				if (_strcmpi(__argv[1], "/r") == 0)
				{
					// force registration of file associations because
					// the /r argument was specifically provided
					// NOTE: the registration is forced by passing "false" for
					// bSkipIfKeysExist
					registerFileAssociations(gpszEAVFileExtension, 
						gpszEAVFileDescription, getAppFullPath(), false);
					registerFileAssociations(gpszVOAFileExtension, 
						gpszVOAFileDescription, getAppFullPath(), false);
					registerFileAssociations(gpszEVOAFileExtension,
						gpszEVOAFileDescription, getAppFullPath(), false);
					return FALSE;
				}
				else if (_strcmpi(__argv[1], "/u") == 0)
				{
					// unregister settings and return.
					unregisterFileAssociations(gpszEAVFileExtension,
						gpszEAVFileDescription);
					unregisterFileAssociations(gpszVOAFileExtension,
						gpszVOAFileDescription);
					unregisterFileAssociations(gpszEVOAFileExtension,
						gpszEVOAFileDescription);
					return FALSE;
				}
			}
			
			try
			{
				// init license
				LicenseManagement::loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);

				// Check license
				validateLicense();

				// Construct and display the dialog
				CEAVGeneratorDlg dlg;
				m_pMainWnd = &dlg;
				dlg.DoModal();
			}
			CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15787")

			// delete the singleton input manager instance, which
			// may be in existence
			IInputManagerSingletonPtr ipInputMgrSingleton(CLSID_InputManagerSingleton);
			ipInputMgrSingleton->DeleteInstance();
		}
		// removed CoUninitialize because it causes an unhandled exception when the program
		// exits.  (Unhandled exception at 0x782cdc68 in mfc80d.dll)
		// it will break in the appcore.cpp CWinApp::ExitInstance function at the line:
		// if (!afxContextIsDLL)
		//CoUninitialize();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07385")

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return FALSE;
}
