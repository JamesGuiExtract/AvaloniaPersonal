// UserLicense.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "UserLicense.h"
#include "LicenseWizard.h"

#include <UCLIDExceptionDlg.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

/////////////////////////////////////////////////////////////////////////////
// CUserLicenseApp

BEGIN_MESSAGE_MAP(CUserLicenseApp, CWinApp)
	//{{AFX_MSG_MAP(CUserLicenseApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG
	ON_COMMAND(ID_HELP, CWinApp::OnHelp)
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CUserLicenseApp construction

CUserLicenseApp::CUserLicenseApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}

/////////////////////////////////////////////////////////////////////////////
// The one and only CUserLicenseApp object

CUserLicenseApp theApp;

/////////////////////////////////////////////////////////////////////////////
// CUserLicenseApp initialization

BOOL CUserLicenseApp::InitInstance()
{
	try
	{
		// Set up the exception handling aspect.
		static UCLIDExceptionDlg exceptionDlg;
		UCLIDException::setExceptionHandler( &exceptionDlg );

		// If appropriate command line argument has been provided,
		// initialize the Date-Time file and registry items and return
		if (__argc == 2)
		{
			if (_stricmp(__argv[1], "/init") == 0)
			{
				// Get file creation time of UserLicense EXE
				CFileFind	ffSource;
				FILETIME	ftCreationTime;
				string		strPath = getCurrentProcessEXEFullPath();
				if (ffSource.FindFile( strPath.c_str() ))
				{
					// Find the "next" file and update source 
					// information for this one
					BOOL	bMoreFiles = ffSource.FindNextFile();

					// Get file creation time
					if (ffSource.GetCreationTime( &ftCreationTime ) == 0)
					{
						// Unexpected error, create and throw exception
						UCLIDException ue( "ELI07376", "Unable to get file creation time" );
						throw ue;
					}
				}

				// Get current system time
				CTime tmCurrent = CTime::GetCurrentTime();

				// Compare file creation and system time
				// and continue only if system time is newer than EXE time
				CTime tmEXE( ftCreationTime );
				if (tmCurrent > tmEXE)
				{
					// Create file and registry items only if neither are present
					try
					{
						LicenseManagement::initTrpData(LICENSE_MGMT_PASSWORD);
					}
					catch(UCLIDException& ue)
					{
						UCLIDException uexOuter( "ELI07449", 
							"Unable to initialize licensing scheme!", ue );
						throw uexOuter;
					}
				}
				else
				{
					// Create and throw exception
					UCLIDException ue( "ELI07480", "Unable to initialize licensing scheme!" );
					throw ue;
				}

				return FALSE;
			}
		}

		// Run the user license wizard
		CLicenseWizard wizard("User License");
		wizard.DoModal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07450")

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return FALSE;
}
