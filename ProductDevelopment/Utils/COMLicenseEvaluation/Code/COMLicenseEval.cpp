// COMLicenseEval.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "COMLicenseEval.h"
#include "COMLicenseEvalDlg.h"
#include "LMData.h"
#include "ComponentData.h"
#include "UCLIDCOMPackages.h"

#include <UCLIDExceptionDlg.h>
#include <UCLIDException.h>

#include <string>
#include <map>
using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void displayUsage(string strError = "")
{
	string strUsage = (!strError.empty()) ? strError + "\r\n\r\n" : "";
	strUsage += "Usage: COMLicenseEval [filename]\r\n";
	strUsage += "----------------------------------------\r\n";
	strUsage += "[filename]\tDisplays information about the license state and expiration date\r\n";
	strUsage += "\t\tof this license file as well as the components it contains.  Only\r\n";
	strUsage += "\t\tworks for license files encrypted with standard passwords.";

	AfxMessageBox(strUsage.c_str());
}
//-------------------------------------------------------------------------------------------------
void evaluateLicenseFile(const string &strFilename)
{
	try
	{
		try
		{
			// Load the license package information from packages.dat
			COMPackages packageInfo;
			packageInfo.init();
			map<unsigned long, string> mapComponents = packageInfo.getComponents();

			// Read and unencrypt the license file.
			LMData	lm;
			string	strData = lm.unzipStringFromFile(strFilename, false);
			lm.extractDataFromString(strData, 
				gulUCLIDKey1, gulUCLIDKey2, gulUCLIDKey3, gulUCLIDKey4);

			// Retrieve the first component and its license state information
			unsigned long ulID = lm.getFirstComponentID();
			ComponentData componentData = lm.getComponentData(ulID);
			string strComponents = mapComponents[ulID] + " [" + asString(ulID) + "]\r\n";
			bool bIsLicensed = componentData.m_bIsLicensed;
			CTime timeExpiration = componentData.m_ExpirationDate;

			// Keep track of the earliest and latest expiration date so we can be sure the
			// expiration dates of the components are never more than a minute apart.
			CTime timeEarliestExpiration = timeExpiration;
			CTime timeLatestExpiration = timeExpiration;
			
			// Glean information from each of the components.  
			for (ulID = lm.getNextComponentID(ulID); ulID != -1; ulID = lm.getNextComponentID(ulID))
			{
				// Build a display-able string of all the components in the file.
				strComponents += mapComponents[ulID] + " [" + asString(ulID) + "]\r\n";

				// Obtain the license information from the component data.
				componentData = lm.getComponentData(ulID);
				
				// Keep track of the earliest and latest expiration date/time found.
				CTime timeExpiration2 = componentData.m_ExpirationDate;
				if (timeExpiration2 < timeEarliestExpiration)
				{
					timeEarliestExpiration = timeExpiration;
				}
				else if (timeExpiration2 > timeLatestExpiration)
				{
					timeLatestExpiration = timeExpiration;
				}
				
				// If the license information from this component does not match that of the 
				// previous components, throw an exception-- command line mode is not able
				// to process this file.
				if (bIsLicensed != componentData.m_bIsLicensed ||
					timeLatestExpiration - timeEarliestExpiration > CTimeSpan(0, 0, 1, 0))
				{
					displayUsage("ERROR: Cannot evaluate license files whose components are "
						"licensed differently!");
					return;
				}
			}

			// Generate and display string with the information that has been compiled.
			string strLicenseInfo = 
				"License info for " + getFileNameFromFullPath(strFilename) + "\r\n";
			strLicenseInfo += "--------------------------------------------\r\n";
			strLicenseInfo += "Fully licensed? " + string(bIsLicensed ? "Yes" : "No");
			if (!bIsLicensed)
			{
				strLicenseInfo += "\r\n";
				string strExpiration = timeEarliestExpiration.Format("%m/%d/%Y %H:%M:%S").GetString();
				strLicenseInfo += "Eval expiration: " + string(bIsLicensed ? "N/A" : strExpiration);
			}
			strLicenseInfo += "\r\n\r\n";
			strLicenseInfo += "Components:\r\n";
			strLicenseInfo += strComponents;

			AfxMessageBox(strLicenseInfo.c_str());
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI23087");
	}
	catch (UCLIDException &ue)
	{
		UCLIDException ueOuter("ELI23088", "Unable to evaluate license file!", ue);
		ueOuter.addDebugInfo("filename", strFilename);
		ueOuter.display();
	}
}

/////////////////////////////////////////////////////////////////////////////
// CCOMLicenseEvalApp

BEGIN_MESSAGE_MAP(CCOMLicenseEvalApp, CWinApp)
	//{{AFX_MSG_MAP(CCOMLicenseEvalApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG
	ON_COMMAND(ID_HELP, CWinApp::OnHelp)
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CCOMLicenseEvalApp construction

CCOMLicenseEvalApp::CCOMLicenseEvalApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}

/////////////////////////////////////////////////////////////////////////////
// The one and only CCOMLicenseEvalApp object

CCOMLicenseEvalApp theApp;

/////////////////////////////////////////////////////////////////////////////
// CCOMLicenseEvalApp initialization

BOOL CCOMLicenseEvalApp::InitInstance()
{
	CoInitializeEx(NULL, COINIT_MULTITHREADED);

	AfxEnableControlContainer();

	try
	{
		// Set up the exception handling aspect.
		static UCLIDExceptionDlg exceptionDlg;
		UCLIDException::setExceptionHandler( &exceptionDlg );			

		// force loading of the COMLMCore.Dll right now so that a following call to
		// GetModuleHandle() to determine the COMLMCore.DLL's location does not fail
		HMODULE hCOMLMCore = LoadLibrary("COMLMCore.Dll");
		
		if (__argc == 2)
		{
			string strFilename(__argv[1]);

			if (strFilename.find("?") != string::npos)
			{
				// Display usage information if the one and only parameter contains a questionmark.
				displayUsage();
			}
			else if (fileExistsAndIsReadable(strFilename))
			{
				// If the parameter describes a filename that is read-able, evaluate it.
				evaluateLicenseFile(strFilename);
			}
			else
			{
				// The parameter does not describe a reabable file.
				displayUsage("ERROR: Missing or un-readable license file!");
			}
		}
		else if (__argc > 2)
		{
			displayUsage("ERROR: Invalid number of arguments!");
		}
		else
		{
			// If there were no command-line arguments, display the license evaluator dialog.
			CCOMLicenseEvalDlg dlg;
			m_pMainWnd = &dlg;
			dlg.DoModal();
		}
		
		FreeLibrary(hCOMLMCore);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12142")

	CoUninitialize();

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return FALSE;
}
