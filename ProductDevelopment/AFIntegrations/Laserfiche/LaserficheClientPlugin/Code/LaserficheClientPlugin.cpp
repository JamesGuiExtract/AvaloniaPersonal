// LaserficheClientPlugin.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "LaserficheClientPlugin.h"

#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <UCLIDException.h>
#include <UCLIDExceptionDlg.h>
#include <ComUtils.h>

#include <string>
using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// add license management function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
static string gstrADMIN_CONSOLE_SWITCH		= "/admin";
static string gstrREDACT_SELECTED_SWITCH	= "/redact";
static string gstrSUBMIT_SELECTED_SWITCH	= "/submit";
static string gstrVERIFY_SELECTED_SWITCH	= "/verify";
static string gstrSERVICE_CONSOLE_SWITCH	= "/service";

//--------------------------------------------------------------------------------------------------
// The one and only CLaserficheClientPluginApp object
CLaserficheClientPluginApp theApp;

//--------------------------------------------------------------------------------------------------
// CLaserficheClientPluginApp
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CLaserficheClientPluginApp, CWinApp)
	ON_COMMAND(ID_HELP, &CWinApp::OnHelp)
END_MESSAGE_MAP()
//--------------------------------------------------------------------------------------------------
CLaserficheClientPluginApp::CLaserficheClientPluginApp() :
	m_ipIDShieldLFPtr(NULL)
{
}
//--------------------------------------------------------------------------------------------------
CLaserficheClientPluginApp::~CLaserficheClientPluginApp()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20711");
}
//--------------------------------------------------------------------------------------------------
BOOL CLaserficheClientPluginApp::InitInstance()
{
	try
	{
		// Initialize the application & COM
		CWinApp::InitInstance();
		CoInitializeEx(NULL, COINIT_MULTITHREADED);

		// Create the IDShieldLF instance
		m_ipIDShieldLFPtr.CreateInstance(CLSID_IDShieldLF);
		ASSERT_RESOURCE_ALLOCATION("ELI20709", m_ipIDShieldLFPtr != NULL);

		// Initialize license
		LicenseManagement::sGetInstance().loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);
		// License validation to be handled by m_ipIDShieldLFPtr.

		// [FlexIDSIntegrations:75] This call will ensure exceptions are displayed as the
		// foreground window.
		UCLIDExceptionDlg::setDisplayAsForegroundWindow(true);

		// Set up the exception handling aspect.
		static UCLIDExceptionDlg exceptionDlg;
		UCLIDException::setExceptionHandler(&exceptionDlg);

		// Default command to admin console
		string strCommand = gstrADMIN_CONSOLE_SWITCH;

		// Retrieve first switch (and ignore any further switches)
		if (__argc >= 2)
		{
			strCommand = __argv[1];
			makeLowerCase(strCommand);
		}
	
		// Administrator Console (repository configuration)
		if (strCommand == gstrADMIN_CONSOLE_SWITCH)
		{
			if (asCppBool(m_ipIDShieldLFPtr->ConnectPrompt(kAdministrator)))
			{
				m_ipIDShieldLFPtr->ShowAdminConsole();
			}
		}
		
		// Redact currently selected documents in the open Laserfiche Client
		if (strCommand == gstrREDACT_SELECTED_SWITCH)
		{
			if (m_ipIDShieldLFPtr->ConnectToActiveClient(kProcess) == VARIANT_TRUE)
			{
				m_ipIDShieldLFPtr->RedactSelected();
			}
		}

		// Submit currently selected documents in the open client for background redaction.
		if (strCommand == gstrSUBMIT_SELECTED_SWITCH)
		{
			if (m_ipIDShieldLFPtr->ConnectToActiveClient(kSubmit) == VARIANT_TRUE)
			{
				m_ipIDShieldLFPtr->SubmitSelectedForRedaction();
			}
		}

		// Verify currently selected documents in the open client.
		if (strCommand == gstrVERIFY_SELECTED_SWITCH)
		{
			if (m_ipIDShieldLFPtr->ConnectToActiveClient(kVerify) == VARIANT_TRUE)
			{
				m_ipIDShieldLFPtr->VerifySelected();
			}
		}

		// Service Console (background service configuration)
		if (strCommand == gstrSERVICE_CONSOLE_SWITCH)
		{
			m_ipIDShieldLFPtr->ShowServiceConsole();
		}

		// Free the IDShieldLF instantiation.
		m_ipIDShieldLFPtr = NULL;

		// 5/20/08 SNK Calling CoUninitialize worked until I added and used RWUtils in 
		// LaserficheCustomComponents.cpp.  Now it causes a crash on exit if it is included.
		//CoUninitialize();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20518");

	return FALSE;
}
//--------------------------------------------------------------------------------------------------
BOOL CLaserficheClientPluginApp::ExitInstance(void)
{
	return CWinApp::ExitInstance();
}
//--------------------------------------------------------------------------------------------------