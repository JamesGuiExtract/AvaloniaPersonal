// SpatialStringViewer.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "SpatialStringViewer.h"
#include "SpatialStringViewerDlg.h"

#include <UCLIDException.h>
#include <UCLIDExceptionDlg.h>
#include <Win32Util.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

const char *gpszUSSFileDescription = "Extract Systems USS File";
const char *gpszUSSFileExtension = ".uss";

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// CSpatialStringViewerApp
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CSpatialStringViewerApp, CWinApp)
	//{{AFX_MSG_MAP(CSpatialStringViewerApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG
	ON_COMMAND(ID_HELP, CWinApp::OnHelp)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CSpatialStringViewerApp construction
//-------------------------------------------------------------------------------------------------
CSpatialStringViewerApp::CSpatialStringViewerApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}

//-------------------------------------------------------------------------------------------------
// The one and only CSpatialStringViewerApp object
//-------------------------------------------------------------------------------------------------
CSpatialStringViewerApp theApp;

//-------------------------------------------------------------------------------------------------
// Local function
//-------------------------------------------------------------------------------------------------
void validateLicense()
{
	// Just re-use the ID for the Spatial String object
	static const unsigned long THIS_APP_ID = gnEXTRACT_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_APP_ID, "ELI12511", "Spatial String Viewer" );
}
//-------------------------------------------------------------------------------------------------
void usage()
{
	string strUsage = "This application can takes zero or one argument:\n";
		strUsage += "Usage:\n";
		strUsage += "USSFileViewer.exe [USSFileName]\n";
		AfxMessageBox(strUsage.c_str());
}

//-------------------------------------------------------------------------------------------------
// CSpatialStringViewerApp initialization
//-------------------------------------------------------------------------------------------------
BOOL CSpatialStringViewerApp::InitInstance()
{
	try
	{
		AfxEnableControlContainer();

		// set the UCLID exception viewer
		static UCLIDExceptionDlg dlg;
		UCLIDException::setExceptionHandler(&dlg);

		// Every time this application starts, re-register the file
		// associations if the file associations don't exist.  If the
		// file associations already exist, then do nothing.
		// This way, registration will happen the very first time
		// the user runs this application (even if the installation
		// program's call to this application with /r argument failed).
		// NOTE: the registration is not forced because we are passing
		// "true" for bSkipIfKeysExist.
		registerFileAssociations(gpszUSSFileExtension, gpszUSSFileDescription, 
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
				registerFileAssociations(gpszUSSFileExtension, 
					gpszUSSFileDescription, getAppFullPath(), false);
				return FALSE;
			}
			else if (_strcmpi(__argv[1], "/u") == 0)
			{
				// unregister settings and return.
				unregisterFileAssociations(gpszUSSFileExtension,
					gpszUSSFileDescription);
				return FALSE;
			}
		}
		else if (__argc > 2)
		{
			// Display the usage
			usage();
			return FALSE;
		}

		// initialize COM and bring up the dialog
		CoInitializeEx(NULL, COINIT_MULTITHREADED);

		// Initialize and check licensing for UCLID components
		LicenseManagement::loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);
		validateLicense();
				
		{
			m_hAccel=LoadAccelerators(AfxGetInstanceHandle(), MAKEINTRESOURCE(IDR_ACCELERATORS));

			// Construct and display the dialog
			CSpatialStringViewerDlg dlg;
			m_pMainWnd = &dlg;
			dlg.DoModal();
		}

		CoUninitialize();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06789")

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return FALSE;
}
//-------------------------------------------------------------------------------------------------
BOOL CSpatialStringViewerApp::ProcessMessageFilter(int code, LPMSG lpMsg)
{
	if(m_hAccel && m_pMainWnd)
    {
        if (::TranslateAccelerator(m_pMainWnd->m_hWnd, m_hAccel, lpMsg)) 
            return(TRUE);
    }
	
    return CWinApp::ProcessMessageFilter(code, lpMsg);
}
//-------------------------------------------------------------------------------------------------
