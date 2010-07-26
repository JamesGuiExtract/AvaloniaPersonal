//==================================================================================================
//
// COPYRIGHT (c) 2000 - 2008 EXTRACT SYSTEMS LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	cpputil.cpp
//
// PURPOSE:	Various utility functions
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan (August 2000 - present)
//
//==================================================================================================

#include "stdafx.h"
#include "cpputil.h"
#include "UCLIDException.h"
#include "TemporaryFileName.h"
#include "StringTokenizer.h"
#include "IdleProcessKiller.h"

#include <ComDef.h>
#include <ObjBase.h>
#include <stdlib.h>
#include <memory>
#include <time.h>
#include <wincon.h>
#include <nb30.h>
#include <math.h>
#include <iostream>
#include <afxmt.h>

// FIXTHIS: confirm that it is OK to comment these lines
// #include <d3dtypes.h>

using namespace std;

// Adapter status structure - used for MAC Address
typedef struct _ASTAT_
{
	ADAPTER_STATUS adapt;
	NAME_BUFFER    NameBuff [30];
}	ASTAT, *PASTAT;

//-------------------------------------------------------------------------------------------------
unsigned char getValueOfHexChar(unsigned char ucChar)
{
	// verify that the character is a hex char
	if (!isHexChar(ucChar))
	{
		UCLIDException ue("ELI01685", "Invalid hex character!");
		ue.addDebugInfo("ucChar", ucChar);
		throw ue;
	}

	if (ucChar >= '0' && ucChar <= '9')
	{
		return ucChar - '0';
	}
	else
	{
		return ucChar - 'A' + 10;
	}
}
//-------------------------------------------------------------------------------------------------
string getDateAsString()
{
	string strTemp;
	CTime currentTime = CTime::GetCurrentTime();
	strTemp = currentTime.Format("%m/%d/%Y").operator LPCTSTR();
	return strTemp;
}
//-------------------------------------------------------------------------------------------------
string getHumanTimeAsString(CTime tmInput)
{
	string strTemp;
	// Build human-readable time as "March 25, 2008 14:28:32"
	strTemp = tmInput.Format("%B %d, %Y %H:%M:%S").operator LPCTSTR();
	return strTemp;
}
//-------------------------------------------------------------------------------------------------
string getEnvironmentVariableValue(const string& strVarName)
{
	string strValue = "";
	char *pszValue = NULL;

	// TESTTHIS: usage of dupenv
	errno_t err = _dupenv_s(&pszValue, NULL, strVarName.c_str());
	if (err != 0)
	{
		// Ensure the memory is cleared if an error occurred
		// (it is safe to call free even if the pointer is NULL)
		free(pszValue);
		UCLIDException ue("ELI12928", "Unable to determine environment variable value!");
		ue.addWin32ErrorInfo(err);
		ue.addDebugInfo("Var", strVarName);
		throw ue;
	}

	// return the value after freeing the memory
	if (pszValue != NULL)
	{
		strValue = pszValue;
		free (pszValue);
	}

	return strValue;
}
//-------------------------------------------------------------------------------------------------
// This code was derived from Microsoft Knowledge Base Article Q118623
string getMACAddress()
{
	string strTemp = "";

	ASTAT Adapter;
	NCB Ncb;
	UCHAR uRetCode;
	LANA_ENUM   lenum;

	memset( &Ncb, 0, sizeof(Ncb) );
	Ncb.ncb_command = NCBENUM;
	Ncb.ncb_buffer = (UCHAR *)&lenum;
	Ncb.ncb_length = sizeof(lenum);
	uRetCode = Netbios( &Ncb );

	// Just retrieve the first address
	if (lenum.length > 0)
	{
		memset( &Ncb, 0, sizeof(Ncb) );
		Ncb.ncb_command = NCBRESET;
		Ncb.ncb_lana_num = lenum.lana[0];

		uRetCode = Netbios( &Ncb );

		memset( &Ncb, 0, sizeof (Ncb) );
		Ncb.ncb_command = NCBASTAT;
		Ncb.ncb_lana_num = lenum.lana[0];

		_mbscpy_s( Ncb.ncb_callname,  (unsigned char *)("*              ") );
		Ncb.ncb_buffer = (unsigned char *) &Adapter;
		Ncb.ncb_length = sizeof(Adapter);

		uRetCode = Netbios( &Ncb );

		if ( uRetCode == 0 )
		{
			char	pszAddress[30] = {0};
			if (sprintf_s( pszAddress, sizeof(pszAddress), "%02x%02x%02x%02x%02x%02x",
				Adapter.adapt.adapter_address[0],
				Adapter.adapt.adapter_address[1],
				Adapter.adapt.adapter_address[2],
				Adapter.adapt.adapter_address[3],
				Adapter.adapt.adapter_address[4],
				Adapter.adapt.adapter_address[5] ) == -1)
			{
				UCLIDException uex("ELI28716", "Unable to format MAC address.");
				uex.addWin32ErrorInfo(errno);
				throw uex;
			}

			strTemp = pszAddress;
		}
	}

	// If no address was found, set the value to Address N/A
	// [LRCAU #5475]
	if (strTemp.length() == 0)
	{
		strTemp = "Address N/A";
	}

	return strTemp;
}
//-------------------------------------------------------------------------------------------------
string getTimeAsString()
{
	string strTemp;
	CTime currentTime = CTime::GetCurrentTime();
	strTemp = currentTime.Format("%H:%M:%S").operator LPCTSTR();
	return strTemp;
}
//-------------------------------------------------------------------------------------------------
string getTimeStamp()
{
	CString zTimeStamp;
	__time64_t curTime;
	time( &curTime );
	tm _tm;
	if (_localtime64_s( &_tm, &curTime ) != 0)
	{
		throw UCLIDException("ELI15735", "Unable to get local time!");
	}
	
	// create a time stamp to be added to the file's name
	zTimeStamp.Format("%02d-%02d-%04d - %02d.%02d.%02d", 
		_tm.tm_mon + 1, 
		_tm.tm_mday, 
		_tm.tm_year + 1900, 
		_tm.tm_hour, 
		_tm.tm_min, 
		_tm.tm_sec);

	return (LPCTSTR)zTimeStamp;
}
//-------------------------------------------------------------------------------------------------
string getMillisecondTimeAsString()
{
	// Get the current time
	SYSTEMTIME	st;
	GetLocalTime( &st );

	// Format time as "HH:MM:SS.mmm"
	CString zTemp;
	zTemp.Format( "%02d:%02d:%02d.%03d", st.wHour, st.wMinute, st.wSecond, st.wMilliseconds );
	string strTemp = LPCTSTR(zTemp);
	return strTemp;
}
//-------------------------------------------------------------------------------------------------
string getCurrentUserName()
{
	string strUserName = "NOT AVAILABLE";

	// get the current logged-in user's name
	// initialize variables
	char pszUserName[512] = {0};
	unsigned long ulBufferSize = sizeof(pszUserName);

	// get the user name
	if (GetUserName(pszUserName, &ulBufferSize))
	{
		strUserName = pszUserName;
	}

	return strUserName;
}
//-------------------------------------------------------------------------------------------------
string getFullUserName(bool bThrowExceptionIfNoFound)
{
	// Get the path to the GetFullUserName executable
	static string strGetFullUserNamePath = getModuleDirectory("BaseUtils.dll")
		+ "\\GetFullUserName.exe";

	// Create a temporary file for the username to be written to
	TemporaryFileName tempFile;

	// Run the GetFullUserName application passing the temp file in as the argument
	DWORD dwExitCode = runExeWithProcessKiller(strGetFullUserNamePath, false, tempFile.getName());

	// Check the exit code of the application and ensure the file size is not 0
	if (dwExitCode == 0 && getSizeOfFile(tempFile.getName()) > 0)
	{
		// Pointer to an ifstream
		ifstream* pinFile = NULL;

		// Wait for the file to be readable (pass in reference to ifstream object
		// so that the file will be opened upon exit if it was successful
		waitForFileToBeReadable(tempFile.getName(), true, &pinFile);

		// Create an auto pointer to manage the pinFile
		auto_ptr<ifstream> apInFile(pinFile);

		// If pointer is null then file was not opened, make one more attempt to open the file
		if (apInFile.get() == NULL)
		{
			// Open the file
			apInFile.reset(new ifstream(tempFile.getName().c_str(), ios::in));
		}

		// Check if the file is open
		if (apInFile->is_open())
		{
			// Get the user name from the file
			string strFullUserName;
			getline(*apInFile, strFullUserName);
			apInFile->close();

			// Return the full user name
			return trim(strFullUserName, " \t", " \t");
		}
	}

	// Application failed, file size is 0, or the temp file could not be opened
	if (!bThrowExceptionIfNoFound)
	{
		// Not throwing an exception, return the username
		return getCurrentUserName();
	}
	else
	{
		// Throw an exception (add the exit code as error info since GetFullUserName
		// returns the result of GetLastError when it fails
		UCLIDException ue("ELI29183", "Unable to retrieve full user name.");
		if (dwExitCode != 0)
		{
			ue.addWin32ErrorInfo(dwExitCode);
		}
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
string getComputerName()
{
	string strComputerName = "NOT AVAILABLE";

	// get this computer's name
	// initialize variables
	char pszComputerName[512] = {0};
	unsigned long ulBufferSize = sizeof(pszComputerName);

	// get the computer name
	if (GetComputerName(pszComputerName, &ulBufferSize))
	{
		strComputerName = pszComputerName;
	}

	return strComputerName;
}
//-------------------------------------------------------------------------------------------------
string getCurrentProcessID()
{
	return asString(GetCurrentProcessId());
}
//-------------------------------------------------------------------------------------------------
string getPrivateProfileString(const string& strAppName, 
							   const string& strKeyName, 
							   const string& strDefault, 
							   const string& strFileName)
{
	char pszTemp[2048] = {0};
	GetPrivateProfileString(strAppName.c_str(),
							strKeyName.c_str(), 
							strDefault.c_str(), 
							pszTemp, 
							sizeof(pszTemp), 
							strFileName.c_str());
	return string(pszTemp);
}
//-------------------------------------------------------------------------------------------------
void emptyWindowsMessageQueue()
{
	// manually dispatch any windows messages that are waiting in this application's queue.  
	// This allows AutoCAD to redraw while big tasks like closure are being processed. 
	MSG msg;
	BOOL bResult;

	do
	{
		bResult = PeekMessage(&msg, NULL, 0, 0, PM_REMOVE);

		if (bResult) // if we got a message off the queue
		{
			DispatchMessage(&msg); // dispatch it
		}
	}
	while (bResult); // while there are still messages to remove
}
//-------------------------------------------------------------------------------------------------
void pumpMessageQueue()
{
	// Pump message queue
	MSG msg;
	while (::PeekMessage(&msg, NULL, 0, 0, PM_NOREMOVE))
	{
		if (!AfxGetApp()->PumpMessage())
		{
			::PostQuitMessage(0);
		}
	}

	// Allow chance for Idle messages
	LONG lIdle = 0;
	while (AfxGetApp()->OnIdle(lIdle++));
}
//-------------------------------------------------------------------------------------------------
/*  Unused function with memory leak
bool truncateUntilSeparator(string& strOrigString, string strSeparator)
{
	size_t findpos;
	
	// FIXTHIS: why 100? make it generic
	char* pszTemp = new char[100];

	findpos = strOrigString.find(strSeparator);
	
	if (findpos == string::npos)
	{
		return 0;
	}

	strncpy_s(temp, sizeof(temp), strOrigString.c_str(), findpos + strSeparator.length());
	temp[findpos + strSeparator.length()] = '\0';
	
	if (!(replaceVariable(strOrigString, string(temp), string(""), kReplaceFirst)))
	{
		return 0;
	}
	else
	{
		return 1;
	}
}
*/
//-------------------------------------------------------------------------------------------------
bool isValidCommaFormat(const string &strValue)
{
	bool bIsValid = false;
	try
	{
		// Check the length of string
		if (strValue.length() > 0)
		{
			// Whether the string has a "+" or "-" at the begining
			bool bHasSign = false;
			if (strValue[0] == '-' || strValue[0] == '+')
			{
				bHasSign = true;
			}
			
			// Handle special cases that ',' is at the begining of a string
			if (strValue[0] == ',' || (bHasSign == true && strValue[1] == ','))
			{
				return bIsValid;
			}
			
			int strLength = strValue.length();
			for (int i = strLength - 1; i >= 0; --i)
			{
				// "strLength - i" is the position of current character,
				// starting from the end of the string with the initial 
				// value equal to 1, not zero
				// If the remaind of the position devided by 4 is zero, 
				// it should be a position for comma
				if ((strLength - i)%4 == 0)
				{
					if (strValue[i] == ',')
					{
						bIsValid = true;
					}
					// For the first character, special handler needed
					else if (i == 0 && bHasSign)
					{
						bIsValid = true;
					}
					else
					{
						bIsValid = false;
						break;
					}
				}
				else
				{
					if (strValue[i] == ',')
					{
						bIsValid = false;
						break;
					}
				}
			}
		}
	}
	catch (...)
	{
		bIsValid = false;
		// Trap and ignore exception
	}
	return bIsValid;
}
//-------------------------------------------------------------------------------------------------
void validateRemoveCommaInteger(string& str)
{
	if (isValidCommaFormat(str))
	{
		string::iterator newEnd;
		newEnd = remove(str.begin(), str.end(), ',');
		str.erase( newEnd, str.end() );
	}
	else
	{
		UCLIDException ue( "ELI13118", "Invalid comma format inside a number!" );
		ue.addDebugInfo( "Input string", str );
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void validateRemoveCommaDouble(string& str)
{
	basic_string <char>::size_type index;

	// Find the first position of '.' or 'E' or 'e' inside the string
	index = str.find_first_of(".Ee");

	// No '.' or 'E' inside the string, treat it as an integer
	if (index == string::npos)
	{
		validateRemoveCommaInteger(str);
	}
	else
	{
		// Get the sub-string after '.' or 'E'
		string subDigits = str.substr(index,str.length() - index);
		if (subDigits.find(',') == string::npos)
		{
			// put the integer part of a double number to verification
			str = str.substr(0, index);
			validateRemoveCommaInteger(str);
			str = str + subDigits;
		}
		else
		{
			UCLIDException ue( "ELI13119", "Invalid comma format inside a number!" );
			ue.addDebugInfo( "Input string", str );
			throw ue;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void runEXE(const string& strExeFullFileName, const string& strParameters, 
			const DWORD dwTimeoutInMilliseconds, ProcessInformationWrapper* pPIW,
			const string& strWorkingDir, DWORD dwCreationFlags/* = DETACHED_PROCESS*/)
{
	try
	{
		try
		{
			// Prepare complete command-line string
			string strCommand = strExeFullFileName;

			if (!strParameters.empty())
			{
				strCommand += " " + strParameters;
			}

			// Prepare STARTUPINFO data structure
			STARTUPINFO si;
			memset(&si,0,sizeof(STARTUPINFO));
			si.cb = sizeof(STARTUPINFO);
			si.wShowWindow = SW_SHOW;

			// Create local ProcessInformationWrapper object
			ProcessInformationWrapper piw;
			if (pPIW == NULL)
			{
				// Object not passed in, use the local object
				pPIW = &piw;
			}

			// Attempt to start the new process
			if (!CreateProcess(NULL,(char *) strCommand.data(), NULL, NULL, TRUE, dwCreationFlags, 
				NULL, (strWorkingDir.empty() ? NULL : strWorkingDir.c_str()), &si, &(pPIW->pi)))
			{
				// Create and throw an exception, including Win32 error information
				UCLIDException uclidException("ELI02396", "Unable to run the executable file!");
				uclidException.addWin32ErrorInfo();
				throw uclidException;
			}

			// Just return if no timeout has been defined
			if (dwTimeoutInMilliseconds == 0)
			{
				return;
			}

			// Wait for the EXE to finish.  If the EXE does not finish within
			// the desired time, throw an exception
			DWORD dwResult = WaitForSingleObject( pPIW->pi.hProcess, dwTimeoutInMilliseconds );

			switch (dwResult)
			{
				// EXE successfully completed before the timeout
			case WAIT_OBJECT_0:
				break;

				// EXE timed out, throw exception
			case WAIT_TIMEOUT:
				{
					UCLIDException ue("ELI15930", "Unexpected timeout while running EXE!");
					throw ue;
				}
				break;

				// A mutex object was not released before the owning thread terminated, 
				// see WaitForSingleObject()
			case WAIT_ABANDONED:
				{
					UCLIDException ue("ELI15933", "Unexpected WAIT_ABANDONED return from runEXE()!");
					throw ue;
				}
				break;

			default:
				{
					UCLIDException ue("ELI15934", "Unexpected return code from runEXE()!");
					ue.addDebugInfo( "Return code", (unsigned long)dwResult );
					throw ue;
				}
				break;
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25457");
	}
	catch(UCLIDException& uex)
	{
		// Add additional debug information
		uex.addDebugInfo("EXE Name", strExeFullFileName);
		uex.addDebugInfo("Working Directory", strWorkingDir);
		uex.addDebugInfo("Timeout_ms", dwTimeoutInMilliseconds);
		if (!strParameters.empty())
		{
			uex.addDebugInfo("Parameters", strParameters);
		}

		throw uex;
	}
}
//-------------------------------------------------------------------------------------------------
void runExtractEXE(const string& strExeFullFileName, const string& strParameters, 
				   const DWORD dwTimeoutInMilliseconds, ProcessInformationWrapper* pPIW,
				   const string& strWorkingDir, DWORD dwCreationFlags/* = DETACHED_PROCESS*/)
{
	// Create temporary UEX file to hold exception thrown by outside utility
	TemporaryFileName tfn( "", ".uex", true );

	// Append /ef <filename> to provided parameters
	string strNewParameters = strParameters;
	strNewParameters += " /ef \"";
	strNewParameters += tfn.getName();
	strNewParameters += "\"";

	// Run the executable
	runEXE( strExeFullFileName, strNewParameters, dwTimeoutInMilliseconds, pPIW, strWorkingDir,
		    dwCreationFlags);

	// Check size of temporary UEX file
	if (getSizeOfFile( tfn.getName() ) > 0)
	{
		// Exception file is non-empty, get contents as string
		string strError = getTextFileContentsAsString( tfn.getName() );

		// Parse the data
		vector<string> vecTokens;
		StringTokenizer	s;
		s.parse( strError, vecTokens );

		// Check the number of tokens
		if (vecTokens.size() == 7)
		{
			// Retrieve the exception's stringized data
			string strData = trim( vecTokens[6], " \r\n", " \r\n" );

			// Create the Exception object from the string
			UCLIDException ue;
			ue.createFromString( "ELI16271", strData );

			// Throw the exception to the outer scope
			throw ue;
		}
	}
}
//-------------------------------------------------------------------------------------------------
DWORD runExeWithProcessKiller(const string& strExeFullFileName, bool bIsExtractExe,
							  string strParameters, const string& strWorkingDirectory,
							  int iIdleTimeout, int iIdleCheckInterval)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI28886", iIdleCheckInterval > 0);
			ASSERT_ARGUMENT("ELI28887", iIdleTimeout >= iIdleCheckInterval);

			auto_ptr<TemporaryFileName> apTempFile;

			// If running an extract exe, add the /ef <ExceptionFile> argument
			if (bIsExtractExe)
			{
				apTempFile.reset(new TemporaryFileName( "", ".uex", true));
				ASSERT_RESOURCE_ALLOCATION("ELI29399", apTempFile.get() != NULL);

				strParameters += " /ef \"";
				strParameters += apTempFile->getName();
				strParameters += "\"";
			}

			// Launch the executable
			ProcessInformationWrapper piw;
			runEXE(strExeFullFileName, strParameters, 0, &piw, strWorkingDirectory);

			// Start an idle process killer
			IdleProcessKiller idleKiller(piw.pi.dwProcessId, iIdleTimeout, iIdleCheckInterval);

			// Wait for the process to end
			WaitForSingleObject( piw.pi.hProcess, INFINITE );

			// Get the process exit code
			DWORD dwExitCode = 0;
			GetExitCodeProcess(piw.pi.hProcess, &dwExitCode);

			// Check if process was killed by the idle process killer
			if (idleKiller.killedProcess())
			{
				UCLIDException uex("ELI28888", "Process killed by idle process killer.");
				uex.addDebugInfo("Idle Timeout", iIdleTimeout);
				uex.addDebugInfo("Timeout Interval", iIdleCheckInterval);
				throw uex;
			}

			// If there was a temp exception file, check if an exception was logged
			if (apTempFile.get() != NULL && getSizeOfFile(apTempFile->getName()) > 0)
			{
				// Exception file is non-empty, get contents as string
				string strError = getTextFileContentsAsString(apTempFile->getName());

				// Parse the data
				vector<string> vecTokens;
				StringTokenizer	s;
				s.parse( strError, vecTokens );

				// Check the number of tokens
				if (vecTokens.size() == 7)
				{
					// Retrieve the exception's stringized data
					string strData = trim( vecTokens[6], " \r\n", " \r\n" );

					// Create the Exception object from the string
					UCLIDException ue;
					ue.createFromString( "ELI29400", strData );

					// Throw the exception to the outer scope
					throw ue;
				}
			}

			// Return the exit code
			return dwExitCode;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28889");
	}
	catch(UCLIDException& uex)
	{
		// Add the executable name, parameters, and working directory
		uex.addDebugInfo("Executable Name", strExeFullFileName);
		uex.addDebugInfo("Parameters", strParameters.empty() ? "<No Parameters>" : strParameters);
		uex.addDebugInfo("Working Directory", strWorkingDirectory.empty()
			? "<No Directory>" : strWorkingDirectory);
		throw uex;
	}
}
//-------------------------------------------------------------------------------------------------
bool isVirtKeyCurrentlyPressed(int iVirtKey)
{
	short iStatus = GetAsyncKeyState(iVirtKey);

	if (iStatus & 0x8000)
	{
		return true;
	}
	else
	{
		return false;
	}
}
//-------------------------------------------------------------------------------------------------
bool windowTransparencyIsSupported()
{
	// First check number of system colors
	int	iNumColors = GetDeviceCaps( GetDC( NULL ), NUMCOLORS );
	if (iNumColors != -1)
	{
		// Color depth is no more than 8 bits per pixel,
		// not enough for transparency
		return false;
	}

	typedef BOOL (WINAPI *lpfnSetLayeredWindowAttributes)(HWND hwnd, COLORREF crKey, BYTE bAlpha, DWORD dwFlags); 
	typedef UINT (WINAPI *lpfnRealGetWindowClass)(HWND  hwnd, LPTSTR pszType, UINT  cchType); 
	
	lpfnSetLayeredWindowAttributes m_pSetLayeredWindowAttributes; 
	lpfnRealGetWindowClass m_pRealGetWindowClass; 
	
	// get access to the User32.Dll and some necessary functions exported from
	// that dll
	HMODULE hUser32 = GetModuleHandle("USER32.DLL");
	if (hUser32 == NULL)
	{
		throw UCLIDException("ELI03980", "Unable to get module handle for User32.Dll!");
	}
	
	m_pSetLayeredWindowAttributes = (lpfnSetLayeredWindowAttributes)
		GetProcAddress(hUser32, "SetLayeredWindowAttributes"); 
	m_pRealGetWindowClass = (lpfnRealGetWindowClass)
		GetProcAddress(hUser32, "RealGetWindowClass"); 
	
	if (m_pSetLayeredWindowAttributes == NULL || m_pRealGetWindowClass == NULL)
	{
		// the particular operating system on this machine does not support
		// translucency...return false to indicate that the method failed.
		return false;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
bool makeWindowTransparent(HWND hWnd, bool bTransparent, BYTE byteTransparency)
{
	// make sure that the window handle given is valid
	if (!::IsWindow(hWnd))
	{
		throw UCLIDException("ELI03988", "Window handle cannot be NULL!");
	}

	// if the user wants to make the window opague, just do it and return
	if (bTransparent == false)
	{
		SetWindowLong(hWnd, GWL_EXSTYLE, GetWindowLong(hWnd, GWL_EXSTYLE) &
			~WS_EX_LAYERED & ~WS_EX_TRANSPARENT);
		
		return false;
	}

	// First check number of system colors
	int	iNumColors = GetDeviceCaps( GetDC( NULL ), NUMCOLORS );
	if (iNumColors != -1)
	{
		// Color depth is no more than 8 bits per pixel,
		// not enough for transparency
		return false;
	}

	typedef BOOL (WINAPI *lpfnSetLayeredWindowAttributes)(HWND hwnd, COLORREF crKey, BYTE bAlpha, DWORD dwFlags); 
	typedef UINT (WINAPI *lpfnRealGetWindowClass)(HWND  hwnd, LPTSTR pszType, UINT  cchType); 
	
	lpfnSetLayeredWindowAttributes m_pSetLayeredWindowAttributes; 
	lpfnRealGetWindowClass m_pRealGetWindowClass; 
	
	// get access to the User32.Dll and some necessary functions exported from
	// that dll
	HMODULE hUser32 = GetModuleHandle("USER32.DLL");
	if (hUser32 == NULL)
	{
		throw UCLIDException("ELI03958", "Unable to get module handle for User32.Dll!");
	}
	
	m_pSetLayeredWindowAttributes = (lpfnSetLayeredWindowAttributes)
		GetProcAddress(hUser32, "SetLayeredWindowAttributes"); 
	m_pRealGetWindowClass = (lpfnRealGetWindowClass)
		GetProcAddress(hUser32, "RealGetWindowClass"); 
	
	if (m_pSetLayeredWindowAttributes == NULL || m_pRealGetWindowClass == NULL)
	{
		// the particular operating system on this machine does not support
		// translucency...return false to indicate that the method failed.
		return false;
	}

	// Make it a layered window.
	SetWindowLong(hWnd, GWL_EXSTYLE,
		GetWindowLong(hWnd, GWL_EXSTYLE) | WS_EX_LAYERED | WS_EX_TRANSPARENT);
	
	// Redraw contents NOW - no flickering since the window's not visible
	RedrawWindow(hWnd, NULL, NULL, RDW_UPDATENOW); 
	
	// Set the window translucency
	m_pSetLayeredWindowAttributes(hWnd, 0, byteTransparency, LWA_ALPHA);

	return true;
}
//-------------------------------------------------------------------------------------------------
vector<string> getArgumentsAsVector(int argc, char *argv[], bool bMakeUpperCase)
{
	vector<string> vecResult;

	for (int i = 1; i < argc; i++)
	{
		// get the argument
		string strArg = argv[i];
		
		// make it uppercase if requested
		if (bMakeUpperCase)
		{
			makeUpperCase(strArg);
		}

		// add argument to result vector
		vecResult.push_back(strArg);
	}

	return vecResult;
}
//-------------------------------------------------------------------------------------------------
bool vectorContainsStringWithPrefix(const vector<string>& vecStrings, 
									const string& strTextToFind,
									long& rnIndex)
{
	vector<string>::const_iterator iter;
	rnIndex = 0;
	for (iter = vecStrings.begin(); iter != vecStrings.end(); iter++)
	{
		if (iter->find(strTextToFind) == 0)
		{
			return true;
		}
		else
		{
			rnIndex++;
		}
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
long round(double dNum)
{
	if (dNum >= 0)
	{
		// FIXTHIS: casting
		return (long) floor(dNum + 0.5);
	}
	else
	{
		// FIXTHIS: casting
		return (long) ceil(dNum - 0.5);
	}
}
//-------------------------------------------------------------------------------------------------
bool isValidIdentifier(const string& strName)
{
	try
	{
		validateIdentifier( strName );
	}
	catch (...)
	{
		return false;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
void validateIdentifier(const string& strName)
{
	// Identifier cannot be empty
	if (strName.empty())
	{
		throw UCLIDException("ELI09530", "Identifier cannot be empty.");
	}

	// Identifier cannot start with a digit
	if (isDigitChar(strName[0]))
	{
		// Create and throw exception
		UCLIDException ue("ELI28221", "First character of identifier cannot be a digit.");
		ue.addDebugInfo("Identifier", strName);
		ue.addDebugInfo("Valid initial characters", 
			"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_");
		throw ue;
	}

	// Identifier must consist of letters, numbers, or underscores
	size_t iFirstNot = strName.find_first_not_of(gstrVALID_IDENTIFIER_CHARS);
	if (iFirstNot != string::npos)
	{
		// Create and throw exception
		UCLIDException ue("ELI09529", "Invalid character for identifier.");
		ue.addDebugInfo("Identifier", strName);
		ue.addDebugInfo("Valid characters", gstrVALID_IDENTIFIER_CHARS);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
long getIdentifierEndPos(const string& strText, long nStartPos)
{
	// Check if start position is out of range or initial character is a digit
	if (nStartPos < 0 || ((ULONG)nStartPos) >= strText.length() || isDigitChar(strText[nStartPos]))
	{
		return string::npos;
	}

	// Return the first invalid identifier character
	size_t iFirstNot = strText.find_first_not_of(gstrVALID_IDENTIFIER_CHARS, nStartPos);
	if (iFirstNot == nStartPos)
	{
		// The first character is invalid
		return string::npos;
	}
	else if (iFirstNot == string::npos)
	{
		// The rest of the string are valid identifier characters
		return strText.length();
	}

	return iFirstNot;
}
//-------------------------------------------------------------------------------------------------
long getCloseScopePos(const string& strText, long nStartPos, char cScopeOpen, char cScopeClose)
{
	string strOpen;
	strOpen += cScopeOpen;

	string strClose;
	strClose += cScopeClose;

	return getCloseScopePos(strText, nStartPos, strOpen, strClose);
}
//-------------------------------------------------------------------------------------------------
long getCloseScopePos(const string& strText, long nStartPos, string strScopeOpen, string strScopeClose)
{
	// find the initial scope operator
	unsigned long ulCurrPos = strText.find(strScopeOpen, nStartPos);
	if (ulCurrPos == string::npos)
	{
		return string::npos;
	}

	ulCurrPos += strScopeOpen.length();

	long nOpen = 1;
	while (nOpen > 0)
	{
		unsigned long ulOpenPos = strText.find( strScopeOpen, ulCurrPos );
		unsigned long ulClosePos = strText.find( strScopeClose, ulCurrPos );

		if (ulClosePos == string::npos)
		{
			return string::npos;
		}
		else if (ulOpenPos == string::npos ||
				ulOpenPos > ulClosePos)
		{
			nOpen--;
			ulCurrPos = ulClosePos + strScopeClose.length();
		}
		else if (ulOpenPos < ulClosePos)
		{
			nOpen++;
			ulCurrPos = ulOpenPos + strScopeOpen.length();
		}
		else // they are equal (one is a substring of the other)
		{
			if (strScopeOpen.length() > strScopeClose.length())
			{
				nOpen++;
				ulCurrPos = ulOpenPos + strScopeOpen.length();
			}
			else
			{
				nOpen--;
				ulCurrPos = ulClosePos + strScopeClose.length();
			}
		}
	}

	return (long)ulCurrPos;
}
//-------------------------------------------------------------------------------------------------
long getRGBFromString(const string& strInput, char cSeparator)
{
	long lResult = -1;
	int r, g, b;

	// Find separators
	unsigned long ulLength = strInput.length();
	if (ulLength == 0)
	{
		// Throw exception
		UCLIDException	ue("ELI11410", "Empty R G B string!");
		throw ue;
	}

	unsigned long ulPos1 = strInput.find( cSeparator, 0 );
	unsigned long ulPos2 = string::npos;
	if ((ulPos1 != string::npos) && (ulPos1 < ulLength - 1))
	{
		ulPos2 = strInput.find( cSeparator, ulPos1 + 1 );
	}
	else
	{
		// Throw exception
		UCLIDException	ue("ELI11411", "Invalid R G B string!");
		ue.addDebugInfo("Input String", strInput );
		throw ue;
	}

	// Extract R, G, B values
	if ((ulPos2 != string::npos) && (ulPos2 < ulLength - 1))
	{
		r = asLong( strInput.substr( 0, ulPos1 ) );
		g = asLong( strInput.substr( ulPos1 + 1, ulPos2 - ulPos1 - 1 ) );
		b = asLong( strInput.substr( ulPos2 + 1, ulLength - ulPos2 - 1 ) );

		// Validate { 0 - 255 }
		if ((r < 0) || (r > 255) ||
			(g < 0) || (g > 255) ||
			(b < 0) || (b > 255))
		{
			// Throw exception
			UCLIDException	ue("ELI11223", "Invalid R G B triple!");
			ue.addDebugInfo( "Red Value", r );
			ue.addDebugInfo( "Green Value", g );
			ue.addDebugInfo( "Blue Value", b );
			throw ue;
		}

		// Convert to long
		lResult = RGB( r, g, b );
	}
	else
	{
		// Throw exception
		UCLIDException	ue("ELI11412", "Invalid R G B string!");
		ue.addDebugInfo("Input String", strInput );
		throw ue;
	}

	return lResult;
}
//-------------------------------------------------------------------------------------------------
COLORREF invertColor(COLORREF crColor)
{
	return RGB(~GetRValue(crColor),~GetGValue(crColor),~GetBValue(crColor));
}
//-------------------------------------------------------------------------------------------------
long getNumLogicalProcessors()
{
	HANDLE hProcess = OpenProcess(PROCESS_ALL_ACCESS, TRUE, GetCurrentProcessId());
	DWORD dwProcessMask, dwSystemMask;
	if (GetProcessAffinityMask(hProcess, &dwProcessMask, &dwSystemMask) == 0)
	{
		UCLIDException ue("ELI11534", "Unable To get the number of processors.");
		throw ue;
	}
	CloseHandle(hProcess);
	
	long nNumProcessors = 0;
	DWORD dwMask = 1;
	// 8 bits in a byte
	long nSize = sizeof(DWORD) * 8;
	int i;
	for(i = 0; i < nSize; i++)
	{
		if (dwMask & dwSystemMask)
		{
			nNumProcessors++;
		}
		dwMask = dwMask << 1;
	}
	return nNumProcessors;
}
//-------------------------------------------------------------------------------------------------
int formatMessageBox(const char* szText, ...)
{
	va_list args;
	va_start(args, szText);
	char buf[16384];
	// TESTTHIS
	int nRet = _vsnprintf_s(buf, sizeof(buf), sizeof(buf)/sizeof(char), szText, args);
	if (nRet == -1)
	{
		UCLIDException ue("ELI11899", "Maximum text byte count exceeded.");
		ue.addDebugInfo("Text", szText);
		ue.addDebugInfo("Bytes", 16384);
		throw ue;
	}
	va_end(args);

	return AfxMessageBox(buf);
}
//-------------------------------------------------------------------------------------------------
const std::string getWindowsErrorString(DWORD dwError)
{
	LPVOID lpMsgBuf;
    FormatMessage(
        FORMAT_MESSAGE_ALLOCATE_BUFFER | 
        FORMAT_MESSAGE_FROM_SYSTEM,
        NULL,
        dwError,
        MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
        (LPTSTR) &lpMsgBuf,
        0, NULL );

	string strError = getFormattedString("Error %d: %s", dwError, lpMsgBuf);

	// for some reason the returned string has \r\n at the end. Trim it off.
	strError = trim(strError, "\r\n", "\r\n");

	// This free should proabably be automatic
    LocalFree(lpMsgBuf);

	return strError;
}
//-------------------------------------------------------------------------------------------------
const std::string getFormattedString(const char* szText, ...)
{
	va_list args;
	va_start(args, szText);
	char buf[512];
	// TESTTHIS
	int nRet = _vsnprintf_s(buf, sizeof(buf), sizeof(buf)/sizeof(char), szText, args);
	if (nRet == -1)
	{
		UCLIDException ue("ELI19402", "Maximum text byte count exceeded.");
		ue.addDebugInfo("Text", szText);
		ue.addDebugInfo("Bytes", 16384);
		throw ue;
	}
	va_end(args);

	std::string str = buf;
	return str;
}
//-------------------------------------------------------------------------------------------------
 bool asCppBool( string strBool)
 {
	// convert to lower case
	makeLowerCase( strBool );

	// Check for "true" or "false" and return appropriate value
	// 4/15/08 SNK Added check for "0" and "1" as well
	if ( strBool == "true" || strBool == "1" )
	{
		return true;
	}
	else if ( strBool == "false" || strBool == "0" )
	{
		return false;
	}

	// The string was not "true" or "false" so throw exception
	UCLIDException ue("ELI16008", "String can not be converted to boolean.");
	ue.addDebugInfo("String", strBool );
	throw ue;
 }
//-------------------------------------------------------------------------------------------------
// PURPOSE: A helper function for shellOpenDocument.
// PROMISE: Adds ShellExecute error information to the provided UCLIDException
// ARGS:	rue- The UCLIDException to which the error info should be added
//			hInstance- The return value from a ShellExecute call
void addShellOpenDocumentErrorInfo(UCLIDException& rue, HINSTANCE hInstance)
{
	switch ((long)hInstance)
	{
		case 0:
			{
				rue.addDebugInfo("ShellExecute ErrorCode", "0");
				rue.addDebugInfo("ShellExecute Error", "The operating system is out of memory or resources.");
				break;
			}
		case ERROR_FILE_NOT_FOUND:
			{
				rue.addDebugInfo("ShellExecute ErrorCode", "ERROR_FILE_NOT_FOUND");
				rue.addDebugInfo("ShellExecute Error", "The specified file was not found.");
				break;
			}
		case ERROR_PATH_NOT_FOUND:
			{
				rue.addDebugInfo("ShellExecute ErrorCode", "ERROR_PATH_NOT_FOUND");
				rue.addDebugInfo("ShellExecute Error", "The specified path was not found.");
				break;
			}
		case ERROR_BAD_FORMAT:
			{
				rue.addDebugInfo("ShellExecute ErrorCode", "ERROR_BAD_FORMAT");
				rue.addDebugInfo("ShellExecute Error", "The .exe file is invalid (non-Microsoft Win32 .exe or error in .exe image).");
				break;
			}
		case SE_ERR_ACCESSDENIED:
			{
				rue.addDebugInfo("ShellExecute ErrorCode", "SE_ERR_ACCESSDENIED");
				rue.addDebugInfo("ShellExecute Error", "The operating system denied access to the specified file.");
				break;
			}
		case SE_ERR_ASSOCINCOMPLETE:
			{
				rue.addDebugInfo("ShellExecute ErrorCode", "SE_ERR_ASSOCINCOMPLETE");
				rue.addDebugInfo("ShellExecute Error", "The file name association is incomplete or invalid.");
				break;
			}
		case SE_ERR_DDEBUSY:
			{
				rue.addDebugInfo("ShellExecute ErrorCode", "SE_ERR_DDEBUSY");
				rue.addDebugInfo("ShellExecute Error", "The Dynamic Data Exchange (DDE) transaction could not be completed because other DDE transactions were being processed.");
				break;
			}
		case SE_ERR_DDEFAIL:
			{
				rue.addDebugInfo("ShellExecute ErrorCode", "SE_ERR_DDEFAIL");
				rue.addDebugInfo("ShellExecute Error", "The DDE transaction failed.");
				break;
			}
		case SE_ERR_DDETIMEOUT: 
			{
				rue.addDebugInfo("ShellExecute ErrorCode", "SE_ERR_DDETIMEOUT");
				rue.addDebugInfo("ShellExecute Error", "The DDE transaction could not be completed because the request timed out.");
				break;
			}
		case SE_ERR_DLLNOTFOUND:
			{
				rue.addDebugInfo("ShellExecute ErrorCode", "SE_ERR_DLLNOTFOUND");
				rue.addDebugInfo("ShellExecute Error", "The specified dynamic-link library (DLL) was not found.");
				break;
			}
		// SE_ERR_FNF is the same as ERROR_FILE_NOT_FOUND
		case SE_ERR_NOASSOC:
			{
				rue.addDebugInfo("ShellExecute ErrorCode", "SE_ERR_NOASSOC");
				rue.addDebugInfo("ShellExecute Error", "There is no application associated with the given file name extension. This error will also be returned if you attempt to print a file that is not printable.");
				break;
			}
		case SE_ERR_OOM:
			{
				rue.addDebugInfo("ShellExecute ErrorCode", "SE_ERR_OOM");
				rue.addDebugInfo("ShellExecute Error", "There was not enough memory to complete the operation.");
				break;
			}
		// SE_ERR_PNF is the same as ERROR_PATH_NOT_FOUND
		case SE_ERR_SHARE:
			{
				rue.addDebugInfo("ShellExecute ErrorCode", "SE_ERR_SHARE");
				rue.addDebugInfo("ShellExecute Error", "A sharing violation occurred.");
				break;
			}
		default:
			{
				rue.addDebugInfo("ShellExecute ErrorCode", (long)hInstance);
				rue.addDebugInfo("ShellExecute Error", "Unknown Error");
				break;
			}
	}
}
//-------------------------------------------------------------------------------------------------
void shellOpenDocument(const string& strFilename)
{
	// Request windows shell to open the specified document
	HINSTANCE hRes = ShellExecute(NULL, "open", strFilename.c_str(), NULL, 
		getDirectoryFromFullPath(strFilename.c_str()).c_str(), SW_SHOW);

	// Per MDSN for ShellExecute: 0-32 represent error values
	if ((int)hRes >= 0 && (int)hRes <= 32)
	{
		UCLIDException ue("ELI18144", "Failed to open document!");
		// Note: Be sure to addWin32ErrorInfo prior to addDebugInfo; addDebugInfo clears the last error
		ue.addWin32ErrorInfo();
		ue.addDebugInfo("Filename", strFilename);
		addShellOpenDocumentErrorInfo(ue, hRes);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
