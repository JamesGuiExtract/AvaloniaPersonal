// OSSpecificInstaller.cpp : Defines the entry point for the application.
//

#include "stdafx.h"

#include <shlobj.h>
#include <io.h>
#include <string>
#include <sstream>

using namespace std;

// Type define the function for IsWow64Process
typedef BOOL (WINAPI *LPFN_ISWOW64PROCESS) (HANDLE, PBOOL);

// Type define the function for both Wow64RevertWow64FsRedirection and Wow64DisableWow64FsRedirection
// This is done instead of using the functions specifiying the function directly because XP 32 bit 
// does not have the 2 functions and there for will not run unless the function address is 
// mapped dynamically in code.
typedef VOID (WINAPI *LPFN_WOW64_FS_REDIRECTION)(VOID *);

// Constants
const int gnBUFFER_SIZE = 1024;

// Define the constant for SM_SERVERR2 this is defined in newer versions of windows SDK
#ifndef SM_SERVERR2
#define SM_SERVERR2 89
#endif

// Define the constant for VER_SUITE_WH_SERVER this is defined in newer versions of the windows SDK
#ifndef VER_SUITE_WH_SERVER
#define VER_SUITE_WH_SERVER 0x00008000
#endif

// Define the fnIsWow64Process function as the IsWow64Process function in kernel32
LPFN_ISWOW64PROCESS 
fnIsWow64Process = (LPFN_ISWOW64PROCESS)GetProcAddress(
	GetModuleHandle("kernel32"),"IsWow64Process");

// Define the address for the Wow64DisableWow64FsRedirection function
LPFN_WOW64_FS_REDIRECTION
fnWow64DisableWow64FsRedirection = (LPFN_WOW64_FS_REDIRECTION)GetProcAddress(
	GetModuleHandle("kernel32"),"Wow64DisableWow64FsRedirection");

// Define the address for the Wow64RevertWow64FsRedirection function
LPFN_WOW64_FS_REDIRECTION
fnWow64RevertWow64FsRedirection = (LPFN_WOW64_FS_REDIRECTION)GetProcAddress(
	GetModuleHandle("kernel32"),"Wow64RevertWow64FsRedirection");

//-------------------------------------------------------------------------------------------------
// IsWow64 - function returns TRUE if the current process is running under Wow64, otherwise 
//			returns FALSE
//-------------------------------------------------------------------------------------------------
BOOL IsWow64()
{
    BOOL bIsWow64 = FALSE;
 
    if (NULL != fnIsWow64Process)
    {
        fnIsWow64Process(GetCurrentProcess(),&bIsWow64);
    }
    return bIsWow64;
}

//-------------------------------------------------------------------------------------------------
// GetOSKey - returns a string value that indicates the OS running
//-------------------------------------------------------------------------------------------------
string GetOSKey()
{
	string strKeyString = "";

	// Need to obtain the OS type
    OSVERSIONINFOEX osvi;
    osvi.dwOSVersionInfoSize = sizeof(OSVERSIONINFOEX);
    ::GetVersionEx( (OSVERSIONINFO *)&osvi );

	// Check if workstation - then OS will be XP_64
	if (osvi.wProductType == VER_NT_WORKSTATION)
	{
		// Windows XP is version 5
		if (osvi.dwMajorVersion == 5)
		{
			strKeyString = "XP";
		}
		// Windows 6 is Vista or Windows 7
		else if (osvi.dwMajorVersion == 6)
		{
			// Minor version of 0 is Vista
			if (osvi.dwMinorVersion == 0)
			{
				strKeyString = "VISTA";
			}
			// Minor version of 1 is Windows 7
			else if (osvi.dwMinorVersion == 1)
			{
				strKeyString = "W7";
			}
		}
	}
	// Check for Windows Server 2003 versions - not interested in Windows Home Server
	else if ((osvi.dwMajorVersion == 5) && (osvi.wSuiteMask != VER_SUITE_WH_SERVER))
	{
		// Determine if Windows 2003 or Windows 2003 R2
		if (GetSystemMetrics(SM_SERVERR2) == 0)
		{
			strKeyString = "WS03";
		}
		else
		{
			strKeyString = "WS03R2";
		}
	}
	// Check for Windows Server 2008 versions
	else if (osvi.dwMajorVersion == 6)
	{
		// If Minor version is 0 then it is Windows Server 2008
		if (osvi.dwMinorVersion == 0)
		{
			strKeyString = "W08";
		}
		// If Minor version is 1 then it is Windows Server 2008 R2
		else if (osvi.dwMinorVersion == 1)
		{
			strKeyString = "W08R2";
		}
	}
	return strKeyString;
}

//--------------------------------------------------------------------------------------------------
// RunCommand - Runs the command line passed in strCommand
//				This method is a modified version of the RunEXE command in BaseUtils
//				if the Command ran without error return 0
//				if there was an error display a dialog with the strDescription and the error code
//				and then return -1
//--------------------------------------------------------------------------------------------------
int RunCommand(const string &strCommand, const string &strDescription)
{
	// Prepare STARTUPINFO data structure
	STARTUPINFO si;
	memset(&si,0,sizeof(STARTUPINFO));
	si.cb = sizeof(STARTUPINFO);
	si.wShowWindow = SW_SHOW;
	PROCESS_INFORMATION pi;

	// Exit code default to 0 to indicate no errors
	DWORD dwExitCode = 0;
	char zMessageBuffer[gnBUFFER_SIZE];
	int iReturnValue = 0;

	try
	{
		// Attempt to start the new process
		if (!CreateProcess(NULL,(char *) strCommand.c_str(), NULL, NULL, TRUE, NULL, 
			NULL, NULL, &si, &pi))
		{
			dwExitCode = GetLastError();
			sprintf_s(zMessageBuffer, gnBUFFER_SIZE,"Unable to run command: %s \r\nErrorCode: 0x%1X",
				strCommand.c_str(), dwExitCode);
			MessageBox(NULL, zMessageBuffer, "Error", 
				MB_OK | MB_TOPMOST | MB_SYSTEMMODAL | MB_ICONEXCLAMATION);
			return -1;
		}

		// Wait for the EXE to finish.  
		WaitForSingleObject( pi.hProcess, INFINITE );

		// Get the exit code of the process that was executed
		GetExitCodeProcess(pi.hProcess, &dwExitCode);
		
		bool bSuccess = dwExitCode == ERROR_SUCCESS 
			|| dwExitCode == ERROR_SUCCESS_REBOOT_INITIATED
			|| dwExitCode == ERROR_SUCCESS_REBOOT_REQUIRED
			|| dwExitCode == ERROR_SUCCESS_RESTART_REQUIRED;

		// If the exit code is not 0 there was an error
		if (!bSuccess)
		{
			// Setup the message to display indicating an error
			sprintf_s(zMessageBuffer, gnBUFFER_SIZE, "The %s install failed with exit code: 0x%lX", 
				strDescription.c_str(), dwExitCode);
			
			// Display message to indicate the install failed
			MessageBox (NULL, zMessageBuffer, "Install failed", 
				MB_OK | MB_TOPMOST | MB_SYSTEMMODAL | MB_ICONEXCLAMATION);
		}

		// Set the return code to 0 if no error and -1 if there was an error
		iReturnValue = (bSuccess) ? 0 : -1;
	}
	catch(...)
	{
		// Set return value to -1 to indicate and error processing the command
		iReturnValue = -1;

		// Display message that there was an unspecified error.
		sprintf_s(zMessageBuffer, gnBUFFER_SIZE, "There was an unspecified error running the %s install.", 
			strDescription.c_str());
		MessageBox(NULL, zMessageBuffer, "Install failed", 
			MB_OK | MB_TOPMOST | MB_SYSTEMMODAL | MB_ICONEXCLAMATION);
	}

	// Close the handles for the process and process thread
	CloseHandle( pi.hProcess );
	CloseHandle( pi.hThread);
	
	return iReturnValue;
}
//--------------------------------------------------------------------------------------------------
// replaceVariable - Replaces each occurance of string t1 in string s with string t2
//					 This is exactly the same as the replaceVariable method in BaseUtils
//--------------------------------------------------------------------------------------------------
bool replaceVariable(string& s, const string& t1, const string& t2)
{
	// this function replaces all occurrences of t1 in S by t2
	size_t findpos;
	bool bReturnType;

	findpos = s.find(t1);
	if (findpos == string::npos)
	{
		bReturnType = 0;
	}
	else
	{
		bReturnType = 1;
		while (findpos != string::npos)
		{
			s.replace(findpos, t1.length(), t2);
			findpos = s.find(t1, findpos + t2.length());
		}
	}

	return bReturnType;
}

//-------------------------------------------------------------------------------------------------
// WinMain - Main appliction function.
//-------------------------------------------------------------------------------------------------
int APIENTRY WinMain(HINSTANCE hInstance,
                     HINSTANCE hPrevInstance,
                     LPSTR     lpCmdLine,
                     int       nCmdShow)
{
	try
	{
		// Get the current directory
		char zCurrDir[gnBUFFER_SIZE];
		GetCurrentDirectory(gnBUFFER_SIZE, zCurrDir);
		string strCurrDir = zCurrDir;
		char zBuffer[gnBUFFER_SIZE];

		// Make sure the OSSI.ini file exists
		if (_access_s( "OSSI.INI", 0) != 0)
		{
			sprintf_s(zBuffer, gnBUFFER_SIZE, "Could not find file %s\\%s.", zCurrDir, "OSSI.INI");
			MessageBox(NULL, zBuffer, "Missing file", 
				MB_OK | MB_TOPMOST | MB_SYSTEMMODAL | MB_ICONEXCLAMATION);
			return -1;
		}

		// Get the current OS Key
		string strKeyString = GetOSKey();

		// Get the SetupDescription - this is not OS dependent
		GetPrivateProfileString("SetupApps", "SetupDescription", NULL, zBuffer, gnBUFFER_SIZE, 
			".\\OSSI.ini");
		string strDescription = zBuffer;

		// if the OS was recognized - run the correct install
		if (strKeyString != "" )
		{
			// Initialize flag to turn off 64bit file system redirection.
			bool bTurnOf64FSRedirection = false;

			if (IsWow64() == TRUE)
			{
				strKeyString += "_64";

				// Get the TurnOff64BitFSRedirection flag
				GetPrivateProfileString("SetupApps", "TurnOff64BitFSRedirection", NULL, zBuffer,
					gnBUFFER_SIZE, ".\\OSSI.ini");

				// Set to true if value equals 1.
				bTurnOf64FSRedirection = zBuffer[0] == '1';
			}
			else
			{
				strKeyString += "_32";
			}

			// Get the install string
			GetPrivateProfileString("SetupApps", strKeyString.c_str(), NULL, zBuffer, gnBUFFER_SIZE,
				".\\OSSI.ini");

			// if there was an entry execute the command
			if (strlen(zBuffer) > 0)
			{
				// Replace occurrences of <CurrDir> in the command with the current directory
				string strCommand = zBuffer;
				replaceVariable(strCommand, "<CurrentDir>", strCurrDir);

				// Pointer used for disable and reverting file system redirection
				PVOID pRedirection;

				// If OS is 64 bit disable file system redirection
				if (bTurnOf64FSRedirection && fnWow64DisableWow64FsRedirection != NULL)
				{
					fnWow64DisableWow64FsRedirection(&pRedirection);
				}

				// Run the command
				int nReturn = RunCommand(strCommand, strDescription);

				// If OS is 64 bit revert file system redirection
				if (bTurnOf64FSRedirection && fnWow64RevertWow64FsRedirection != NULL)
				{
					fnWow64RevertWow64FsRedirection(&pRedirection);
				}
				return nReturn;
			}
		}
		else
		{
			// Display message that there was an unspecified error.
			sprintf_s(zBuffer, gnBUFFER_SIZE, "Did not recognize the Operating System for %s install.", 
				strDescription.c_str());
			MessageBox(NULL, zBuffer, "Install failed", 
				MB_OK | MB_TOPMOST | MB_SYSTEMMODAL | MB_ICONEXCLAMATION);

			// Return -1 since the the OS could not be determined.
			return -1;
		}		
	}
	catch(...)
	{
		// Display message that there was an unspecified error.
		MessageBox(NULL, "There was an unspecified error running the install.", "Install failed", 
			MB_OK | MB_TOPMOST | MB_SYSTEMMODAL | MB_ICONEXCLAMATION);
		return -1;
	};

	return 0;
}
//-------------------------------------------------------------------------------------------------
