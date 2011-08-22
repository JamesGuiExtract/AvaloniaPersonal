// COMLicenseGenerator.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "COMLicenseGenerator.h"
#include "COMLicenseGeneratorDlg.h"
#include "LMData.h"
#include "UCLIDCOMPackages.h"

#include <cpputil.h>
#include <LicenseUtils.h>
#include <UCLIDException.h>

#include <set>
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
	strUsage += "Usage: COMLicenseGenerator [/SDK] | [<filename> <package_names> <eval_days>]\r\n";
	strUsage += "--------------------------\r\n";
	strUsage += "/SDK\t\tSpecifies for the license generator UI to be shown with ";
	strUsage += "random password and check disk serial numbers options enabled.\r\n";
	strUsage += "\t\tIf the /SDK option is not specified, any specified parameters must be:\r\n";
	strUsage += "<filename>\tThe path the license file should be written to. The ";
	strUsage += "directory (if specified) must already exist.\r\n";
	strUsage += "<package_names>\tA comma delimited list of license packages to apply ";
	strUsage += "to the eval.\r\n\t\tThese must be specified using only package variables ";
	strUsage += "defined in packages.dat (variables prefixed with '!').\r\n";
	strUsage += "<eval_days>\tThe number of days the evaluation should last. Must be at ";
	strUsage += "least 1 but no more than 184 days (6 months).\r\n";
	strUsage += "\r\n";
	strUsage += "Examples:\r\n";
	strUsage += "--------------------------\r\n";
	strUsage += "COMLicenseGenerator /SDK\r\n";
	strUsage += "COMLicenseGenerator eval.lic IDShieldOffice,IDShield 16\r\n";
	strUsage += "\r\n";

	AfxMessageBox(strUsage.c_str());
}
//-------------------------------------------------------------------------------------------------
set<unsigned long> getPackageIDs(const string &strPackages, string &rstrInvalidPackages)
{
	// A container to store the required component IDs
	set<unsigned long> setPackageIDs;
	
	// Ensure rstrInvalidPackages is clear initially.
	rstrInvalidPackages.clear();

	// Declare and initialize a COMPackages class to retrieve package information
	COMPackages packageInfo;
	packageInfo.init();

	// Loop through each entry in the strPackages list.
	int nPos = 0;
	while (true)
	{
		// Find the next comma in the list
		int nNextPos = strPackages.find(',', nPos + 1);

		// Assign
		string strPackage;
		if (nNextPos == string::npos)
		{
			strPackage = strPackages.substr(nPos);
		}
		else
		{
			strPackage = strPackages.substr(nPos, nNextPos - nPos);
		}

		// Trim any leading or trailing spaces as well as any leading comma in case the first
		// character of the list is a comma.
		strPackage = trim(strPackage, ", ", " ");

		if (!strPackage.empty())
		{
			// As long as we have something left after trimming, retrive component IDs
			// for this package by prefixing with the "!" char.  This ensures the package
			// is a named variable and prevents specific component IDs from being specified.
			vector<unsigned long> vecIDs = packageInfo.getPackageComponents("!" + strPackage);

			if (vecIDs.size() > 0)
			{
				// If we found corresponding component IDs, add them to the return result.
				setPackageIDs.insert(vecIDs.begin(), vecIDs.end());
			}
			else
			{
				// If we didn't find any component IDs, there is something wrong with the
				// specified package name.
				rstrInvalidPackages += (rstrInvalidPackages.empty() ? "" : ", ") + strPackage;
			}
		}

		if (nNextPos == string::npos || nNextPos == strPackages.length() - 1)
		{
			// If we didn't find another comma or the next comma is the last character in the list,
			// we're done.
			break;
		}
		else
		{
			// The next item in the list starts at the character following the next comma.
			nPos = nNextPos + 1;
		}
	}

	return setPackageIDs;
}
//-------------------------------------------------------------------------------------------------
bool generateEvalLicense(const string &strLicenseFile, const set<unsigned long> &setComponentIDs, 
						 long nEvalDays)
{
	ASSERT_ARGUMENT("ELI23080", setComponentIDs.size() > 0);
	ASSERT_ARGUMENT("ELI23081", nEvalDays >= 1 && nEvalDays <= 184);

	// Create and intialize an LMData object to use in creating the evalutation license.
	LMData lm;
	lm.setIssueDateToToday();
	lm.setUseComputerName(false);
	lm.setUseSerialNumber(false);
	lm.setUseMACAddress(false);

	// Set the expiration date bases on nEvalDays
	CTime timeExpire(CTime::GetCurrentTime() + CTimeSpan(nEvalDays, 0, 0, 0));
	timeExpire = CTime(timeExpire.GetYear(), timeExpire.GetMonth(), timeExpire.GetDay(), 
					   23, 59, 59);

	// Add an eval for each of the specified components.
	for each (unsigned long ulComponentID in setComponentIDs)
	{
		lm.addUnlicensedComponent(ulComponentID, timeExpire);
	}

	// Use basic UCLID passwords to encrypt the license.
	string strData1 = lm.compressDataToString(gulUCLIDKey5, gulUCLIDKey6, gulUCLIDKey7, gulUCLIDKey8);
	string strData2 = lm.compressDataToString(gulUCLIDKey1, gulUCLIDKey2, gulUCLIDKey3, gulUCLIDKey4);
	
	// Write the license to file and return true if successful.
	return lm.zipStringsToFile(strLicenseFile, lm.getCurrentVersion(), strData1, strData2);
}
//-------------------------------------------------------------------------------------------------
void handleArguments(int argc, char **argv, bool &rbShowUI, bool &rbEnableSDK)
{
	try
	{
		try
		{
			// Default to not show the UI so the UI doesn't end up showing in case of an exception.
			rbShowUI = false;
			rbEnableSDK = false;

			if (argc == 1)
			{
				// No arguments; show the UI without the EnableSDK option.
				rbShowUI = true;
				return;
			}
			else if (argc > 4)
			{
				displayUsage("ERROR: Invalid number of arguments!");
				return;
			}

			// There are arguments to handle...
			string strArg1 = argv[1];
			makeUpperCase(strArg1);

			if (strArg1.find('?') != string::npos)
			{
				displayUsage();
			}
			else if (strArg1 == "/SDK" || strArg1 == "-SDK")
			{
				// If the SDK parameter is specified, just make sure there are no other parameters
				// specified.

				if (argc !=  2)
				{
					displayUsage("ERROR: SDK argument not compatible with any other arguments!");
					return;
				}
				else
				{
					// Per command-line argument, show the UI with SDK enabled...
					rbShowUI = true;
					rbEnableSDK = true;
					return;
				}
			}
			else
			{
				// If the SDK parameter is not specified, there must be a total of 4 parameters.
				if (argc != 4)
				{
					displayUsage("ERROR: Invalid number of arguments!");
					return;
				}

				// Retrieve the location to write the license file.
				string strLicenseFile = argv[1];

				// Ensure the specified file is in a valid directory.
				string strDirectory = getDirectoryFromFullPath(strLicenseFile);
				if (!strDirectory.empty() && !isFileOrFolderValid(strDirectory))
				{
					displayUsage("ERROR: Directory not found: " + strDirectory);
					return;
				}

				// Retrieve the list of packages this license file applies to.
				string strPackages = argv[2];

				// Validate the package names and retrieve their corresponding component IDs.
				string strBadPackages;
				set<unsigned long> setComponentIDs = getPackageIDs(strPackages, strBadPackages);

				// Make sure all specified packages were valid
				if (!strBadPackages.empty())
				{
					displayUsage("ERROR: Invalid package name(s): " + strBadPackages + " !");
					return;
				}

				int nEvalDays = 0;

				try
				{
					// Retrieve the number of days the eval period should last.
					nEvalDays = asLong(argv[3]);
				}
				catch (...)
				{
					displayUsage("ERROR: Unable to parse the number of days the evaluation period "
						"should last!");
					return;
				}

				// Ensure a reasonable eval period
				if (nEvalDays < 1 || nEvalDays > 184)
				{
					displayUsage("ERROR: The evalution period must be 1 to 184 days (6 months)!");
					return;
				}

				// Generate the evaluation license
				if (!generateEvalLicense(strLicenseFile, setComponentIDs, nEvalDays))
				{
					throw UCLIDException ("ELI23091", "Failed to generate license file!");
				}
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI23078")
	}
	catch (UCLIDException &ue)
	{
		ue.display();
		
		displayUsage();
	}
}

//-------------------------------------------------------------------------------------------------
// CCOMLicenseGeneratorApp
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CCOMLicenseGeneratorApp, CWinApp)
	//{{AFX_MSG_MAP(CCOMLicenseGeneratorApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG
	ON_COMMAND(ID_HELP, CWinApp::OnHelp)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CCOMLicenseGeneratorApp construction
//-------------------------------------------------------------------------------------------------
CCOMLicenseGeneratorApp::CCOMLicenseGeneratorApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}

//-------------------------------------------------------------------------------------------------
// The one and only CCOMLicenseGeneratorApp object
CCOMLicenseGeneratorApp theApp;

//-------------------------------------------------------------------------------------------------
// CCOMLicenseGeneratorApp initialization
//-------------------------------------------------------------------------------------------------
BOOL CCOMLicenseGeneratorApp::InitInstance()
{
	AfxEnableControlContainer();

#ifndef _DEBUG
	if (!isInternalToolsLicensed())
	{
		// File does not exist
		MessageBox( NULL, "Unable to run License Generator Utility", 
			"Error", MB_ICONSTOP );

		return FALSE;
	}
#endif

	CoInitializeEx(NULL, COINIT_MULTITHREADED);

	// force loading of the COMLMCore.Dll right now so that a following call to
	// GetModuleHandle() to determine the COMLMCore.DLL's location does not fail
	HMODULE hCOMLMCore = LoadLibrary("COMLMCore.Dll");

	// Set by handleArguments to indicate what needs to be done after the arguments are processed.
	bool bShowUI;
	bool bEnableSDK;
	
	// Validate and handle any command-line parameters.
	handleArguments(__argc, __argv, bShowUI, bEnableSDK);

	if (bShowUI)
	{
		// Check for presence of Calendar control on system
		HKEY	hKey = NULL;
		LONG lResult = ::RegOpenKeyEx( HKEY_CLASSES_ROOT, 
			"CLSID\\{8E27C92B-1264-101C-8A2F-040224009C02}\\TypeLib",
			0, KEY_EXECUTE, &hKey );
		
		if (lResult != ERROR_SUCCESS || hKey == NULL)
		{
			// Control not found
			MessageBox( NULL, "The calendar control (MSCAL.OCX) must be registered before "
				"License Generator Utility will run!", "Error", MB_ICONSTOP );
		}
		else
		{
			// Create and run the main dialog window
			CCOMLicenseGeneratorDlg dlg(bEnableSDK);
			m_pMainWnd = &dlg;
			dlg.DoModal();
		}
	}

	FreeLibrary(hCOMLMCore);

	CoUninitialize();

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return FALSE;
}
//-------------------------------------------------------------------------------------------------