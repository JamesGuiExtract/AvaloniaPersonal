// RuleSetEditor.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "RuleSetEditor.h"

#include <LicenseMgmt.h>
#include <UCLIDExceptionDlg.h>
#include <UCLIDException.h>
#include <RegistryPersistenceMgr.h>
#include <cpputil.h>
#include <Win32Util.h>
#include <ComponentLicenseIDs.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

CRuleSetEditorApp theApp;
const char *gpszRSDFileDescription = "Extract Systems RSD File";
const char *gpszRSDFileExtension = ".rsd";

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CRuleSetEditorApp, CWinApp)
	//{{AFX_MSG_MAP(CRuleSetEditorApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG
	ON_COMMAND(ID_HELP, CWinApp::OnHelp)
END_MESSAGE_MAP()
//-------------------------------------------------------------------------------------------------
CRuleSetEditorApp::CRuleSetEditorApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}
//-------------------------------------------------------------------------------------------------
BOOL CRuleSetEditorApp::InitInstance()
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

		// Every time this application starts, re-register the file
		// associations if the file associations don't exist.  If the
		// file associations already exist, then do nothing.
		// This way, registration will happen the very first time
		// the user runs this application (even if the installation
		// program's call to this application with /r argument failed).
		// NOTE: the registration is not forced because we are passing
		// "true" for bSkipIfKeysExist.
		registerFileAssociations(gpszRSDFileExtension, gpszRSDFileDescription, 
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
				registerFileAssociations(gpszRSDFileExtension, 
					gpszRSDFileDescription, getAppFullPath(), false);
				return FALSE;
			}
			else if (_strcmpi(__argv[1], "/u") == 0)
			{
				// unregister settings and return.
				unregisterFileAssociations(gpszRSDFileExtension,
					gpszRSDFileDescription);
				return FALSE;
			}
		}

		// Load license file(s) using Special Simple Rule Writing passwords
		LicenseManagement::sGetInstance().loadLicenseFilesFromFolder( LICENSE_MGMT_PASSWORD,
			gnSIMPLE_RULE_WRITING_PASSWORDS );

		// Check to see if a license file was loaded that licenses the Rule Set Editor
		if (!LicenseManagement::sGetInstance().isLicensed( gnRULESET_EDITOR_UI_OBJECT ))
		{
			// Try again with default passwords
			LicenseManagement::sGetInstance().loadLicenseFilesFromFolder( LICENSE_MGMT_PASSWORD,
				gnDEFAULT_PASSWORDS );
		}

		// Create the IRuleSet object
		IRuleSetUIPtr	ipRuleSetUI(CLSID_RuleSet);
		ASSERT_RESOURCE_ALLOCATION( "ELI15789", ipRuleSetUI != __nullptr );

		// get the command line and see if there was an argument provided.
		string strFileName = "";
		if (__argc >= 2)
		{
			strFileName = __argv[1];
		}

		string strBinDir = getCurrentProcessEXEDirectory();
		// Show the UI
		ipRuleSetUI->ShowUIForEditing(_bstr_t(strFileName.c_str()), _bstr_t(strBinDir.c_str()));

		// delete the singleton input manager instance, which
		// may be in existence
		IInputManagerSingletonPtr ipInputMgrSingleton(CLSID_InputManagerSingleton);
		ASSERT_RESOURCE_ALLOCATION( "ELI15790", ipInputMgrSingleton != __nullptr );
		ipInputMgrSingleton->DeleteInstance();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04921");

	// Calling CoUninitialize here causes an unexpected Win32 error on exit [P16 #2452].
	// This appears related to using the apartment threading model (see note on CoInitializeEx).
	//CoUninitialize();

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return FALSE;
}
//-------------------------------------------------------------------------------------------------
int CRuleSetEditorApp::ExitInstance()
{
	return CWinApp::ExitInstance();
}
//-------------------------------------------------------------------------------------------------
