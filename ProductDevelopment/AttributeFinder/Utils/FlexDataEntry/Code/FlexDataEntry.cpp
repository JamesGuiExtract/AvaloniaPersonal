// FlexDataEntry.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "FlexDataEntry.h"
#include "FlexDataEntryDlg.h"

#include <RWUtils.h>
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

// Defines message used to help application close gracefully
// Used only for preliminary version with Build 1.3.0.11
//const char *gpszMsg = "[Extract Systems Message]";

CComModule _Module;

//-------------------------------------------------------------------------------------------------
// CFlexDataEntryApp
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CFlexDataEntryApp, CWinApp)
	//{{AFX_MSG_MAP(CFlexDataEntryApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG
	ON_COMMAND(ID_HELP, CWinApp::OnHelp)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CFlexDataEntryApp construction
//-------------------------------------------------------------------------------------------------
CFlexDataEntryApp::CFlexDataEntryApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}

//-------------------------------------------------------------------------------------------------
// The one and only CFlexDataEntryApp object
//-------------------------------------------------------------------------------------------------
CFlexDataEntryApp theApp;

//-------------------------------------------------------------------------------------------------
// Local functions
//-------------------------------------------------------------------------------------------------
void validateLicense()
{
	static const unsigned long THIS_APP_ID = gnFLEXINDEX_SERVER_OBJECTS;

	VALIDATE_LICENSE( THIS_APP_ID, "ELI10812", "FLEX Index Data Entry" );
}/*
//-------------------------------------------------------------------------------------------------
// Used only for preliminary version with Build 1.3.0.11
UINT closeWnd(LPVOID)
{
	HWND hWnd = NULL;
	while (!hWnd)
	{
		hWnd = FindWindow("#32770", gpszMsg);
	}

	// Programmatically close the window
	SendMessage(hWnd, WM_COMMAND, MAKEWPARAM(IDCANCEL, BN_CLICKED), NULL);

	return 0;
}*/

//-------------------------------------------------------------------------------------------------
// MyDummyDlg
//-------------------------------------------------------------------------------------------------
class MyDummyDlg : public CDialog
{
// Construction
public:
	MyDummyDlg(bool bShowDlg, std::string strImageFile, CWnd* pParent = NULL)
	: CDialog(CFlexDataEntryDlg::IDD, pParent)
	{
		if (bShowDlg)
		{
			CFlexDataEntryDlg dlg( strImageFile );
			dlg.DoModal();
		}
	}

// Dialog Data
	enum { IDD = IDD_FLEXDATAENTRY_DIALOG };
};

//-------------------------------------------------------------------------------------------------
// CFlexDataEntryApp initialization
//-------------------------------------------------------------------------------------------------
BOOL CFlexDataEntryApp::InitInstance()
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
			// Initialize the Rogue Wave Utils library
			RWInitializer	rwInit;

			// Set up the exception handling aspect.
			static UCLIDExceptionDlg exceptionDlg;
			UCLIDException::setExceptionHandler( &exceptionDlg );			

			// Initialize license
			LicenseManagement::sGetInstance().loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);
			validateLicense();

			// Handle Command-line argument for INI file
			string strCommand;
			if (__argc == 2)
			{
				strCommand = __argv[1];
			}

			// Construct and display the dialog
			MyDummyDlg dlg( true, strCommand );
		}

		// Calling CoUninitialize here causes an unexpected Win32 error on exit [P16 #2336].
		// This appears related to using the apartment threading model (see note on CoInitializeEx).
		//CoUninitialize();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10803")

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return FALSE;
}
//-------------------------------------------------------------------------------------------------
int CFlexDataEntryApp::ExitInstance()
{
	try
	{
		// Cleanup the Rogue Wave Utils library using the updated termination function
		RWCleanup cleanup;

		//////////////////
		// Remove 1.3.0.11 special code - WEL 05/09/05
		//////////////////

		// Start the thread that will close the following MessageBox before
		// it becomes visible.  It was discovered that the MessageBox was required 
		// to avoid memory errors at exit with Build 1.3.0.11.
//		AfxBeginThread(closeWnd, NULL);
//		MessageBox(NULL, "", gpszMsg, MB_OK);

		// It was discovered that without forcing the app to die at this point, some
		// "memory could not be read" error messages show up at exit with Build 1.3.0.11.  
		// In theory, killing the app at this time should not be harmful because all of 
		// UCLID's objects should have been unloaded from memory by now anyway.
//		_exit(0);
	}
	catch(...)
	{
	}

	return CWinApp::ExitInstance();
}
//-------------------------------------------------------------------------------------------------
