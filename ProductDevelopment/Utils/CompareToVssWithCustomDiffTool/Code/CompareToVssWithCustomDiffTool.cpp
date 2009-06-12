// CompareToVssWithCustomDiffTool.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "CompareToVssWithCustomDiffTool.h"

#include <CppUtil.hpp>
#include <TemporaryFileName.h>
#include <UCLIDException.hpp>
#include <UCLIDExceptionDlg.h>

#include <io.h>
#include <string>
using namespace std;

// TODO: Enhance this application to allow for user-specified (via registry, INI file, command-line, etc)
// values for these variables
const string gstrLOCAL_ROOT = "D:\\";
const string gstrDIFF_EXE = "C:\\Program Files\\KDiff3\\KDiff3.exe";

//--------------------------------------------------------------------------------------------------
// Globals
//--------------------------------------------------------------------------------------------------
CCompareToVssWithCustomDiffToolApp theApp;

//--------------------------------------------------------------------------------------------------
// Functions
//--------------------------------------------------------------------------------------------------
void displayUsage(bool bDisplayError)
{
	string strMsg;

	// If the error string should be displayed, add it to the message.
	if (bDisplayError)
	{
		strMsg += "ERROR!\n";
		strMsg += "Please specify a valid number of arguments! See usage below!\n\n";
	}

	// Add the usage description to the message
	strMsg += "Usage:\n";
	strMsg += "CompareToVssUsingKDiff.exe local_file_path\n\n";
	strMsg += "where local_file_path is the path to the local version of the Vss file,\n";
	strMsg += "such as D:\\Engineering\\ProductDevelopment\\MyApp\\Code\\MyCode.cpp\n\n";
	strMsg += "For this utility to work as expected, the following three conditions must be met:\n";
	strMsg += "1. The \"ss get\" command to get the corresponding SourceSafe file should work.\n";
	strMsg += "2. You have KDiff installed at ";
	strMsg += gstrDIFF_EXE;
	strMsg += "\n";
	strMsg += "3. Your local SourceSafe root directory is ";
	strMsg += gstrLOCAL_ROOT;

	// Display the message box
	AfxMessageBox(strMsg.c_str(), MB_OK | MB_ICONEXCLAMATION);
}
//--------------------------------------------------------------------------------------------------
string getVssFile(string strLocalFile)
{
	// Erase the local root folder from the string
	strLocalFile.erase(0, gstrLOCAL_ROOT.length());
	
	// Change \ to /
	replaceVariable(strLocalFile, "\\", "/");

	// Insert $/ in the front
	strLocalFile.insert(0, "$/");
	
	// Return the computed result
	return strLocalFile;
}
//--------------------------------------------------------------------------------------------------
string getTempFileFromVss(const string& strVssFile)
{
	// Compute the name of the file that Vss would end up getting into the current folder
	// E.g. this would be a string like "CppUtil.hpp", without any path
	size_t positionOfLastSlash = strVssFile.rfind('/');
	if (positionOfLastSlash == string::npos)
	{
		UCLIDException ue("ELI16663", "Unable to find any slashes (/) in the Vss file name!");
		ue.addDebugInfo("strVssFile", strVssFile);
		throw ue;
	}

	string strFile = strVssFile.substr(positionOfLastSlash + 1);
	
	// Attempt to get the file from VSS into the system temporary folder, instead of the current folder
	string strTempDir;
	getTempDir(strTempDir);
	strFile.insert(0, strTempDir);

	// Make sure that a file with the specified name does not exist in the current folder
	// so that when Vss gets a file, it does not overwrite any files.
	if (isFileOrFolderValid(strFile))
	{
		UCLIDException ue("ELI16664", "There is already a local file with the same name as the file to be retrieved from SourceSafe!");
		ue.addDebugInfo("strFile", strFile);
		throw ue;
	}

	// Compute the VSS command to execute to get the specified file
	string strCmd = "ss get \"";
	strCmd += strVssFile;
	strCmd += "\" -W";

	// Switch to the temp folder 
	if (!SetCurrentDirectory(strTempDir.c_str()))
	{
		UCLIDException ue("ELI16665", "Unable to set the current working directory!");
		ue.addDebugInfo("strTempDir", strTempDir);
		throw ue;
	}
	
	// Execute the VSS command
	runEXE(strCmd, "", INFINITE);

	// Make sure the file was successfully retrieved from VSS
	if (!fileExistsAndIsReadable(strFile))
	{
		UCLIDException ue("ELI16666", "Unable to retrieve the specified file from Source Safe!");
		ue.addDebugInfo("strVssFile", strVssFile);
		throw ue;
	}

	return strFile;
}
//--------------------------------------------------------------------------------------------------
void executeDiffTool(const string& strLocalFile, const string& strTempFileFromVss)
{
	// Compute the command string to execute the differencing tool
	string strCmd = gstrDIFF_EXE;
	strCmd += " \"";
	strCmd += strTempFileFromVss;
	strCmd += "\" \"";
	strCmd += strLocalFile;
	strCmd += "\"";

	// Execute the differencing tool
	runEXE(strCmd, "", INFINITE);
}
//--------------------------------------------------------------------------------------------------
void compareLocalFileToVssWithCustomDiffTool(const string& strLocalFile)
{
	// ensure that the LocalFile is inside the engineering tree
	string strLocalFileLowerCase = strLocalFile;
	string strLocalRootLowerCase = gstrLOCAL_ROOT;
	makeLowerCase(strLocalFileLowerCase);
	makeLowerCase(strLocalRootLowerCase);
	if (strLocalFileLowerCase.find(strLocalRootLowerCase) != 0)
	{
		UCLIDException ue("ELI16706", "Specified local file is not part of the engineering tree!");
		ue.addDebugInfo("strLocalFile", strLocalFile);
		throw ue;
	}

	// Get the full path of the file in VSS
	string strVssFile = getVssFile(strLocalFile);
	
	// Get the file from VSS into a local temporary file
	string strTempFileFromVss = getTempFileFromVss(strVssFile);

	// Execute the differencing tool
	executeDiffTool(strLocalFile, strTempFileFromVss);

	// Delete the local copy of the file obtained from VSS
	if (!DeleteFile(strTempFileFromVss.c_str()))
	{
		UCLIDException ue("ELI16667", "The local copy of the file obtained from SourceSafe could not be deleted!");
		ue.addDebugInfo("strLocalFile", strLocalFile);
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
void processArguments()
{
	// Ensure valid count of arguments has been passed in
	if (__argc != 2)
	{
		displayUsage(true);
		return;
	}

	// If the user wanted usage help, display it and exit
	string strArg1 = __argv[1];
	if (strArg1 == "/?")
	{
		displayUsage(false);
		return;
	}

	// At this point, we are expecting the argument to be a valid readable filename
	if (!fileExistsAndIsReadable(strArg1))
	{
		UCLIDException ue("ELI16668", "The file you specified is not readable!");
		ue.addDebugInfo("strArg1", strArg1);
		throw ue;
	}

	// Call the method to compare the specified file to its corresponding version in Vss
	compareLocalFileToVssWithCustomDiffTool(strArg1);
}

//--------------------------------------------------------------------------------------------------
// CCompareToVssWithCustomDiffToolApp class
//--------------------------------------------------------------------------------------------------
CCompareToVssWithCustomDiffToolApp::CCompareToVssWithCustomDiffToolApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}
//--------------------------------------------------------------------------------------------------
// Message map for this CWinApp class
BEGIN_MESSAGE_MAP(CCompareToVssWithCustomDiffToolApp, CWinApp)
	ON_COMMAND(ID_HELP, &CWinApp::OnHelp)
END_MESSAGE_MAP()
//--------------------------------------------------------------------------------------------------
BOOL CCompareToVssWithCustomDiffToolApp::InitInstance()
{
	try
	{
		// InitCommonControlsEx() is required on Windows XP if an application
		// manifest specifies use of ComCtl32.dll version 6 or later to enable
		// visual styles.  Otherwise, any window creation will fail.
		INITCOMMONCONTROLSEX InitCtrls;
		InitCtrls.dwSize = sizeof(InitCtrls);
		// Set this to include all the common control classes you want to use
		// in your application.
		InitCtrls.dwICC = ICC_WIN95_CLASSES;
		InitCommonControlsEx(&InitCtrls);

		CWinApp::InitInstance();

		AfxEnableControlContainer();

		// Show the status window
		m_diffStatusDlg.Create(DiffStatusDlg::IDD);
		m_diffStatusDlg.ShowWindow(SW_SHOW);

		// Set the UCLID Exception Viewer as the default exception handler
		static UCLIDExceptionDlg exceptionDlg;
		UCLIDException::setExceptionHandler(&exceptionDlg);

		// Standard initialization
		// If you are not using these features and wish to reduce the size
		// of your final executable, you should remove from the following
		// the specific initialization routines you do not need
		// Change the registry key under which our settings are stored
		// TODO: You should modify this string to be something appropriate
		// such as the name of your company or organization
		SetRegistryKey(_T("Local AppWizard-Generated Applications"));

		// process the arguments
		processArguments();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16669")

	// Since the dialog has been closed, return FALSE so that we exit the
	// application, rather than start the application's message pump.
	return FALSE;
}
//--------------------------------------------------------------------------------------------------
