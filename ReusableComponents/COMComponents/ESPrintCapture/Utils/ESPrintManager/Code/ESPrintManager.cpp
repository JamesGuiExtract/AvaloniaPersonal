// ESPrintManager.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "ESPrintManager.h"

#include <UCLIDException.h>
#include <UCLIDExceptionDlg.h>
#include <cpputil.h>
#include <INIFilePersistenceMgr.h>
#include <RegistryPersistenceMgr.h>
#include <RegConstants.h>

#include <string>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const string gstrFILE_KEY = "File";
const string gstrORIGINAL_FILE = "{Original}";

const string gstrESPRINTMANAGER_ROOT_FOLDER_PATH = gstrCOM_COMPONENTS_REG_PATH
	+ "\\ESPrintCapture\\Utils\\ESPrintManager";
const string gstrESPRINTMANAGER_SETTINGS_KEY = "ESPMSettings";
const string gstrESPRINTMANAGER_SETTINGS_KEY_PATH = gstrESPRINTMANAGER_ROOT_FOLDER_PATH
	+ "\\" + gstrESPRINTMANAGER_SETTINGS_KEY;
const string gstrESPRINTMANAGER_SETTINGS_APPLICATION_KEY = "Application";
const string gstrESPRINTMANAGER_SETTINGS_APPLICATION_ARGS_KEY = "ApplicationArgs";

//--------------------------------------------------------------------------------------------------
// CESPrintManagerApp
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CESPrintManagerApp, CWinApp)
END_MESSAGE_MAP()

//--------------------------------------------------------------------------------------------------
// CESPrintManagerApp construction
//--------------------------------------------------------------------------------------------------
CESPrintManagerApp::CESPrintManagerApp()
{
	try
	{
		m_apUserCfgMgr = unique_ptr<IConfigurationSettingsPersistenceMgr>(
			new RegistryPersistenceMgr(HKEY_LOCAL_MACHINE, gstrESPRINTMANAGER_SETTINGS_KEY_PATH));
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI22139");
}
//--------------------------------------------------------------------------------------------------
CESPrintManagerApp::~CESPrintManagerApp()
{
	try
	{
		if (!m_strPrintedINIFile.empty())
		{
			deleteFile(m_strPrintedINIFile);
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI22140");
}

//--------------------------------------------------------------------------------------------------
// The one and only CESPrintManagerApp object
//--------------------------------------------------------------------------------------------------
CESPrintManagerApp theApp;

//--------------------------------------------------------------------------------------------------
// Public methods
//--------------------------------------------------------------------------------------------------
// CESPrintManagerApp initialization
BOOL CESPrintManagerApp::InitInstance()
{
	try
	{
		CWinApp::InitInstance();

		AfxEnableControlContainer();

		// Setup exception handling
		UCLIDExceptionDlg exceptionDlg;
		UCLIDException::setExceptionHandler( &exceptionDlg );

		if (__argc != 2)
		{
			AfxMessageBox("Invalid command line\nUsage:\nESPrintManager <INIFileName>");
			return FALSE;
		}

		// Get the INI file from the command line
		m_strPrintedINIFile = buildAbsolutePath(__argv[1]);

		// Ensure the file exists
		validateFileOrFolderExistence(m_strPrintedINIFile);

		// Read the registry settings
		readSettingsFromRegistry();

		// Process the INI file
		PrintedImageResults results = processPrintedINIFile();

		// Launch the application
		launchApplication(results);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI22141");

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return FALSE;
}

//--------------------------------------------------------------------------------------------------
// Private methods
//--------------------------------------------------------------------------------------------------
PrintedImageResults CESPrintManagerApp::processPrintedINIFile()
{
	// Create an INIFilePersistenceMgr from the INI file
	INIFilePersistenceMgr imgrPrintedINI(m_strPrintedINIFile);
	string strFolder = m_strPrintedINIFile + "\\";

	PrintedImageResults results;

	// Get the original document 
	results.OriginalDocument = imgrPrintedINI.getKeyValue(strFolder + "Info", "DocumentName", "");

	// Get the count of output files
	strFolder += "Output";
	long lFileCount = asLong(imgrPrintedINI.getKeyValue(strFolder, "FileCount", ""));

	// Currently only support output of a single multi-page tif file, if more
	// than one file was produced this is an error condition
	if (lFileCount > 1)
	{
		// Get the file names and delete them before throwing exception
		try
		{
			for (long i = 0; i < lFileCount; i++)
			{
				string strNewFile =
					imgrPrintedINI.getKeyValue(strFolder, gstrFILE_KEY + asString(i), "");
				deleteFile(strNewFile);
			}
		}
		CATCH_AND_LOG_ALL_EXCEPTIONS("ELI22142");

		// Build an exception and throw it
		UCLIDException uex("ELI22143", "Too many files created by print driver!");
		uex.addDebugInfo("Original file", results.OriginalDocument);
		uex.addDebugInfo("File count", lFileCount);
		throw uex;
	}

	// Return the generated image file
	results.ImageFile = imgrPrintedINI.getKeyValue(strFolder, gstrFILE_KEY + "0", "");

	return results;
}
//--------------------------------------------------------------------------------------------------
void CESPrintManagerApp::launchApplication(const PrintedImageResults& results)
{
	try
	{
		// Check if the command line args contains {Original}, if so
		// replace it with the original document name
		string strCommandLineArgs = m_strCommandLineArgs;
		if (!strCommandLineArgs.empty()
			&& strCommandLineArgs.find(gstrORIGINAL_FILE) != string::npos)
		{
			replaceVariable(strCommandLineArgs, gstrORIGINAL_FILE,
				"\"" + results.OriginalDocument + "\"");
		}

		// Build command line arguments
		string strCommandLine = "\"" + results.ImageFile + "\"" +
			(strCommandLineArgs.empty() ? "" : (" " + strCommandLineArgs));

		// Create a SHELLEXECUTEINFO struct
		SHELLEXECUTEINFO shellExecuteInfo = {0};

		// Fill the SHELLEXECUTEINFO struct
		shellExecuteInfo.cbSize = sizeof(SHELLEXECUTEINFO);
		shellExecuteInfo.hwnd = __nullptr;
		shellExecuteInfo.lpVerb = "open";
		shellExecuteInfo.lpFile = m_strLaunchApplication.c_str();
		shellExecuteInfo.lpParameters = strCommandLine.c_str();
		shellExecuteInfo.lpDirectory = __nullptr;
		shellExecuteInfo.nShow = SW_SHOWNORMAL;

		// Launch the application
		if (!asCppBool(ShellExecuteEx(&shellExecuteInfo)))
		{
			UCLIDException uex("ELI22148", "Unable to launch printer output handler!");
			uex.addWin32ErrorInfo();
			uex.addDebugInfo("Application Path", m_strLaunchApplication);
			throw uex;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI22149");
}
//--------------------------------------------------------------------------------------------------
void CESPrintManagerApp::readSettingsFromRegistry()
{
	// Get the application to launch from the registry
	m_strLaunchApplication = m_apUserCfgMgr->getKeyValue("",
		gstrESPRINTMANAGER_SETTINGS_APPLICATION_KEY, "");

	if (m_strLaunchApplication.empty())
	{
		throw UCLIDException("ELI22150",
			"An application to handle the printed image must be registered!");
	} 

	// Get any command line options if specified
	m_strCommandLineArgs = m_apUserCfgMgr->getKeyValue("",
		gstrESPRINTMANAGER_SETTINGS_APPLICATION_ARGS_KEY, "");
}
//--------------------------------------------------------------------------------------------------
