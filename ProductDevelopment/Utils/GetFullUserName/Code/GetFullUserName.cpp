//
// GetFullUserName.cpp : Defines the entry point for the GetFullUserName.exe application
//
//
// This application has been designed to simply check for the display user name via
// the GetUserNameEx function.  If it is found it will be written to the file that
// was specified on the command line.  Usage for this executable is as follows:
// GetFullUserName <FullyQualifiedFileName>
// Example:
// GetFullUserName "c:\Documents and Settings\Users\Admin\Local Data\Temp\1a3bgt.tmp"
//
// This application has been created to deal with an issue on Windows Vista 32 involving
// calling into the Secur32.dll
// Details about the issue can be found at:
// http://support.microsoft.com/kb/942234
// The issue was created when adding the Secur32.dll dependency to BaseUtils and then
// throwing an exception from within an application that was linked to the Nuance libraries
// (ESConvertToPDF and SSOCR2).  When the exception was thrown a call was made to GetUserName
// which caused the application to crash.  This new application will be used to isolate
// the dependency/loading of the Secur32.dll.

#include "stdafx.h"
#include "GetFullUserName.h"

// Needed for the GetUserNameEx function
#define SECURITY_WIN32 
#include <security.h>

#include <string>
#include <fstream>
#include <io.h>

using namespace std;

#define MAX_LOADSTRING 100

// Global Variables:
HINSTANCE hInst;								// current instance
TCHAR szTitle[MAX_LOADSTRING];					// The title bar text
TCHAR szWindowClass[MAX_LOADSTRING];			// the main window class name

// Entry point function
int APIENTRY _tWinMain(HINSTANCE hInstance,
                     HINSTANCE hPrevInstance,
                     LPTSTR    lpCmdLine,
                     int       nCmdShow)
{
	UNREFERENCED_PARAMETER(hPrevInstance);

	// Initialize global strings
	LoadString(hInstance, IDS_APP_TITLE, szTitle, MAX_LOADSTRING);
	LoadString(hInstance, IDC_GETFULLUSERNAME, szWindowClass, MAX_LOADSTRING);

	// Get the file name from the command line
	// (Assume that there is only a file name)
	string strFileName(lpCmdLine);
	
	// Check for a valid file name and that the file exists
	if (strFileName.empty() || _access_s(strFileName.c_str(), 0) != 0)
	{
		return 0;
	}

	// Get the full username and write it to the file
	char zName[1024] = {0};
	unsigned long nLength = 1024;
	if (GetUserNameEx(NameDisplay, zName, &nLength) == TRUE)
	{
		// If the username was retrieved, then open the file for output
		ofstream outFile(strFileName.c_str(), ios::out);
		if (outFile.is_open())
		{
			// File was open so write the name to the file and close the file
			outFile << zName << endl;
			outFile.close();
		}

		// Return 0 to indicate success
		return 0;
	}
	else
	{
		// Failed getting the user name, return the last error
		return GetLastError();
	}
}