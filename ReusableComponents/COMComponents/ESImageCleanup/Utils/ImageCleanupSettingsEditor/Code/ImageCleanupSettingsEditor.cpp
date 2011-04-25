//-------------------------------------------------------------------------------------------------
//
// COPYRIGHT (c) 2007 - 2008 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ImageCleanupSettingsEditor.cpp
//
// PURPOSE:	To display the Image Cleanup Settings Editor
//
// NOTES:	
//
// AUTHORS:	Jeff Shergalis
//
//-------------------------------------------------------------------------------------------------

#include "stdafx.h"
#include "ImageCleanupSettingsEditor.h"
#include "ImageCleanupSettingsEditorDlg.h"

#include <LicenseMgmt.h>
#include <UCLIDExceptionDlg.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <Win32Util.h>
#include <ComponentLicenseIDs.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const char *gpszICSFileDescription = "Extract Systems ICS File";
const char *gpszICSFileExtension = ".ics";

//-------------------------------------------------------------------------------------------------
// CImageCleanupSettingsEditorApp

BEGIN_MESSAGE_MAP(CImageCleanupSettingsEditorApp, CWinApp)
	ON_COMMAND(ID_HELP, &CWinApp::OnHelp)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CImageCleanupSettingsEditorApp construction

CImageCleanupSettingsEditorApp::CImageCleanupSettingsEditorApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}

//-------------------------------------------------------------------------------------------------
// The one and only CImageCleanupSettingsEditorApp object

CImageCleanupSettingsEditorApp theApp;

//-------------------------------------------------------------------------------------------------
void validateLicense()
{
	VALIDATE_LICENSE(gnRULE_DEVELOPMENT_TOOLKIT_OBJECTS, "ELI17177", "Image Cleanup Settings Editor" );
}

//-------------------------------------------------------------------------------------------------
// CImageCleanupSettingsEditorApp initialization

BOOL CImageCleanupSettingsEditorApp::InitInstance()
{
	AfxEnableControlContainer();

	try
	{
		CoInitializeEx(NULL, COINIT_APARTMENTTHREADED);
		{
			static UCLIDExceptionDlg exceptionDlg;
			UCLIDException::setExceptionHandler(&exceptionDlg);

			// every time this application starts, re-register the file
			// associations if the file associations don't exist.  if the
			// file associations already exist, then do nothing.
			// this way, registration will happen the very first time
			// the user runs this application (even if the installation
			// program's call to this application with /r argument failed).
			// NOTE: the registration is not forced because we are passing
			// "true" for bSkipIfKeysExist.
			registerFileAssociations(gpszICSFileExtension, gpszICSFileDescription, 
				getAppFullPath(), true);

			string strFileToOpen = "";

			if (__argc == 2 || __argc == 3)
			{
				if (_strcmpi(__argv[1], "/r") == 0)
				{
					// force registration of file associations
					// NOTE: the registration is forced by passing "false" for
					// bSkipIfKeysExist
					registerFileAssociations(gpszICSFileExtension, gpszICSFileDescription, 
						getAppFullPath(), false);
					return FALSE;
				}
				else if (_strcmpi(__argv[1], "/u") == 0)
				{
					// unregister settings and return
					unregisterFileAssociations(gpszICSFileExtension,
						gpszICSFileDescription);
					return FALSE;
				}
				else
				{
					// if it is not a settings flag, then argument is a file to open
					strFileToOpen = __argv[1];

					// check to be sure the file exists
					validateFileOrFolderExistence(strFileToOpen);
				}

				// check if we have set strFileToOpen yet, if not then 
				// check for three arguments, if so then last one is the file name
				if (strFileToOpen == "" && __argc == 3)
				{
					strFileToOpen = __argv[2];

					// check to be sure the file exists
					validateFileOrFolderExistence(strFileToOpen);
				}
			}

			try
			{
				// init license
				LicenseManagement::loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);

				// check license
				validateLicense();

				// create the dialog
				CImageCleanupSettingsEditorDlg dlg(NULL, strFileToOpen);

				// set our main window handle
				m_pMainWnd = &dlg;

				// display the dialog
				dlg.DoModal();
			}
			CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17178");
		}

		CoUninitialize();		
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17179");

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return FALSE;
}
