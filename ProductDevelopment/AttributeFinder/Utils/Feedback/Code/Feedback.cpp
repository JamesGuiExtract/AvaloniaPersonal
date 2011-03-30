// Feedback.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "Feedback.h"
#include "ConfigDlg.h"
#include "PackageDlg.h"
#include "ChoiceDlg.h"

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

CFeedbackApp theApp;
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CFeedbackApp, CWinApp)
	//{{AFX_MSG_MAP(CFeedbackApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG
	ON_COMMAND(ID_HELP, CWinApp::OnHelp)
END_MESSAGE_MAP()
//-------------------------------------------------------------------------------------------------
CFeedbackApp::CFeedbackApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}
//-------------------------------------------------------------------------------------------------
void validateLicense()
{
	static const unsigned long THIS_APP_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( THIS_APP_ID, "ELI11109", "Feedback Manager" );
}
//-------------------------------------------------------------------------------------------------
BOOL CFeedbackApp::InitInstance()
{
	AfxEnableControlContainer();

	try
	{
		CoInitializeEx(NULL, COINIT_MULTITHREADED);
		{
			// Set up the exception handling aspect.
			static UCLIDExceptionDlg exceptionDlg;
			UCLIDException::setExceptionHandler( &exceptionDlg );

			// Load license file(s)
			LicenseManagement::sGetInstance().loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);

			try
			{
				validateLicense();
				
				// Which dialog should be shown
				EFeedbackDialogSelected	eDialogChoice = kNoDialog;

				// Check for command line argument
				// to specify dialog to show
				if (__argc == 2)
				{
					if (_strcmpi(__argv[1], "/c") == 0)
					{
						// Set dialog choice as Configure
						eDialogChoice = kConfigure;
					}
					else if (_strcmpi(__argv[1], "/p") == 0)
					{
						// Set dialog choice as Package
						eDialogChoice = kPackage;
					}
					else if (_strcmpi(__argv[1], "/u") == 0)
					{
						// Set dialog choice as Unpackage
						eDialogChoice = kUnpackage;
					}
				}
				else
				{
					// Default to Choice dialog
					eDialogChoice = kChoice;
				}

				// Create the IFeedbackMgrInternals object needed by the dialogs
				IFeedbackMgrInternalsPtr	ipManager( CLSID_FeedbackMgr );
				ASSERT_RESOURCE_ALLOCATION( "ELI09152", ipManager != __nullptr );

				// Show the appropriate UI
				switch (eDialogChoice)
				{

				case kChoice:
				case kNoDialog:
					{
						// Construct and show the dialog
						CChoiceDlg dlg( ipManager );
						dlg.DoModal();
					}
					break;

				case kConfigure:
					{
						// Construct and show the dialog
						CConfigDlg dlg( ipManager );
						dlg.DoModal();
					}
					break;

				case kPackage:
					{
						// Construct and show the dialog
						CPackageDlg dlg( ipManager );
						dlg.DoModal();
					}
					break;

				default:
					// Throw exception
					UCLIDException ue( "ELI08086", "Invalid Feedback Manager option." );
					ue.addDebugInfo( "eDialogChoice: ", eDialogChoice );
					throw ue;
					break;
				}
			}
			CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15788");
		}

		CoUninitialize();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07975");

	// Since the dialog has been closed, return FALSE so that we exit the
	// application, rather than start the application's message pump.
	return FALSE;
}
//-------------------------------------------------------------------------------------------------
