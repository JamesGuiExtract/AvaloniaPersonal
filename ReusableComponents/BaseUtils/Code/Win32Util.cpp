
#include "stdafx.h"
#include "Win32Util.h"
#include "RegistryPersistenceMgr.h"
#include "UCLIDException.h"
#include "cpputil.h"

#include <tlhelp32.h>  // for Windows 95 
#include <winperf.h>   // for Windows NT 

//--------------------------------------------------------------------------------------------------
// Function pointer types for accessing Toolhelp32 functions dynamically. 
// By dynamically accessing these functions, we can use them on Windows 
// 95 and Windows 2000 and still run on Windows NT, which does not have 
// these functions. 
//
typedef BOOL (WINAPI *PROCESSWALK)(HANDLE hSnapshot, LPPROCESSENTRY32 lppe); 
typedef HINSTANCE (WINAPI *CREATESNAPSHOT)(DWORD dwFlags, DWORD th32ProcessID); 

// Function pointer types for accessing platform-specific functions 
const unsigned long gulMAX_TASKS = 256;
typedef DWORD (*LPGetTaskList)(PTASK_LIST, DWORD); 
typedef bool  (*LPEnableDebugPriv)(void); 

// Constants 
#define INITIAL_SIZE        51200 
#define EXTEND_SIZE         25600 
#define REGKEY_PERF         "software\\microsoft\\windows nt\\currentversion\\perflib" 
#define REGSUBKEY_COUNTERS  "Counters" 
#define PROCESS_COUNTER     "process" 
#define PROCESSID_COUNTER   "id process" 
#define UNKNOWN_TASK        "unknown" 

//--------------------------------------------------------------------------------------------------
// PURPOSE: Changes the process's privilege so that kill works properly.
// REQUIRE: None.
// PROMISE: Always returns true.
// ARGS:	None.
// AUTHOR:	Wayne Lenius
// NOTES:	This function is just an internal support function, and is not exported 
//				from the DLL.
bool enableDebugPriv95(void) 
{ 
   return true; 
} 

//--------------------------------------------------------------------------------------------------
// PURPOSE: Changes the process's privilege so that kill works properly.
// REQUIRE: None.
// PROMISE: Returns true if successful, otherwise false.
// ARGS:	None.
// AUTHOR:	Wayne Lenius
// NOTES:	This function is just an internal support function, and is not exported 
//				from the DLL.
bool enableDebugPrivNT(void) 
{ 
	HANDLE				hToken; 
	LUID				DebugValue; 
	TOKEN_PRIVILEGES	tkp; 

	// Retrieve a handle of the access token 
	if (!OpenProcessToken(GetCurrentProcess(), 
			TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, 
			&hToken)) 
	{ 
		return FALSE; 
	} 

	// Enable the SE_DEBUG_NAME privilege 
	if (!LookupPrivilegeValue((LPSTR) NULL, 
			SE_DEBUG_NAME, 
			&DebugValue)) 
	{ 
		return FALSE; 
	} 

	tkp.PrivilegeCount = 1; 
	tkp.Privileges[0].Luid = DebugValue; 
	tkp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED; 

	AdjustTokenPrivileges(hToken, 
		FALSE, 
		&tkp, 
		sizeof(TOKEN_PRIVILEGES), 
		(PTOKEN_PRIVILEGES) NULL, 
		(PDWORD) NULL); 

	// The return value of AdjustTokenPrivileges can't be tested 
	if (GetLastError() != ERROR_SUCCESS) 
	{ 
		return FALSE; 
	} 

	return TRUE; 
} 

//--------------------------------------------------------------------------------------------------
// PURPOSE: Provides an API for getting a list of tasks running at the time of 
//				the API call.  This function uses Toolhelp32 to get the task 
//				list and is therefore straight WIN32 calls that anyone can call.
// REQUIRE: None.
// PROMISE: Returns the number of tasks placed into the pTask array.
// ARGS:	pTask: array of TASK_LIST structures.
//				dwNumTasks: maximum number of tasks in the pTask array.
// AUTHOR:	Wayne Lenius
// NOTES:	This function is just an internal support function, and is not exported 
//				from the DLL.  Toolhelp32 is not supported under Windows NT 4.0.
DWORD getTaskList95(PTASK_LIST pTask, DWORD dwNumTasks)
{
	// TODO: following code commented out because it does not work on WinNT 4.0
	CREATESNAPSHOT	pCreateToolhelp32Snapshot = NULL; 
	PROCESSWALK		pProcess32First           = NULL; 
	PROCESSWALK		pProcess32Next            = NULL; 

	HINSTANCE		hKernel        = NULL; 
	HINSTANCE		hProcessSnap   = NULL; 
	PROCESSENTRY32	pe32           = {0}; 
	DWORD			dwTaskCount    = 0; 

	// Guarantee to the code later on that we'll enum at least one task. 
	if (dwNumTasks == 0) 
	{
		return 0; 
	}

	// Obtain a module handle to KERNEL so that we can get the addresses of 
	// the 32-bit Toolhelp functions we need. 
	hKernel = GetModuleHandle("KERNEL32.DLL"); 

	// Retrieve pointers to desired functions
	if (hKernel) 
	{ 
		pCreateToolhelp32Snapshot = 
			(CREATESNAPSHOT)GetProcAddress(hKernel, "CreateToolhelp32Snapshot" ); 

		pProcess32First = (PROCESSWALK)GetProcAddress(hKernel, 
			"Process32First" ); 

		pProcess32Next  = (PROCESSWALK)GetProcAddress(hKernel, 
			"Process32Next" ); 
	} 
     
	// Check for pointers
	if (!(pProcess32First && pProcess32Next && pCreateToolhelp32Snapshot)) 
	{
		return 0; 
	}
        
	// Take a snapshot of all processes currently in the system. 
	hProcessSnap = pCreateToolhelp32Snapshot( TH32CS_SNAPPROCESS, 0 ); 
	if (hProcessSnap == (HANDLE)-1) 
	{
        return 0; 
	}

	// Walk the snapshot of processes and for each process, get information 
	// to display. 
	dwTaskCount = 0; 
	pe32.dwSize = sizeof(PROCESSENTRY32);   // must be filled out before use 
	if( pProcess32First( hProcessSnap, &pe32 ) ) 
	{ 
		do 
		{
			// Just copy the executable name
			strcpy_s( pTask->ProcessName, pe32.szExeFile );
			
			// Copy the process ID
			pTask->dwProcessId = pe32.th32ProcessID; 

			// Keep track of how many tasks we've got so far 
			++dwTaskCount;   

			// Move to next task info block. 
			++pTask;         
		} 
		while (dwTaskCount < dwNumTasks && pProcess32Next(hProcessSnap, &pe32)); 
	} 
	else 
	{
        dwTaskCount = 0;    // Couldn't walk the list of processes. 
	}

	// Don't forget to clean up the snapshot object... 
	CloseHandle( hProcessSnap ); 

	return dwTaskCount;
}

//--------------------------------------------------------------------------------------------------
// PURPOSE: Provides an API for getting a list of tasks running at the time of 
//				the API call.  This function uses the registry performance data 
//				to get the task list and is therefore straight WIN32 calls that 
//				anyone can call.
// REQUIRE: None.
// PROMISE: Returns the number of tasks placed into the pTask array.
// ARGS:	pTask: array of TASK_LIST structures.
//				dwNumTasks: maximum number of tasks in the pTask array.
// AUTHOR:	Wayne Lenius
// NOTES:	This function is just an internal support function, and is not exported 
//				from the DLL.  The specific registry keys are only supported under 
//				Windows NT 4.0.
DWORD getTaskListNT(PTASK_LIST pTask, DWORD dwNumTasks) 
{ 
	DWORD						dwKey; 
	HKEY						hKeyNames; 
	DWORD						dwType; 
	DWORD						dwSize; 
	CHAR*						lpcBuff = NULL; 
	CHAR						szSubKey[1024]; 
	LANGID						wLanguageID; 
	LPSTR						pStr; 
	LPSTR						pStr2; 
	PPERF_DATA_BLOCK			pPerf; 
	PPERF_OBJECT_TYPE			pObj; 
	PPERF_INSTANCE_DEFINITION	pInst; 
	PPERF_COUNTER_BLOCK			pCounter; 
	PPERF_COUNTER_DEFINITION	pCounterDef; 
	DWORD						i; 
	DWORD						dwProcessIdTitle; 
	DWORD						dwProcessIdCounter; 
	CHAR						szProcessName[MAX_PATH]; 
	DWORD						dwLimit = dwNumTasks - 1; 

	// Look for the list of counters.  Always use the neutral 
	// English version, regardless of the local language.  We 
	// are looking for some particular keys, and we are always 
	// going to do our looking in English.  We are not going 
	// to show the user the counter names, so there is no need 
	// to go find the corresponding name in the local language. 
	wLanguageID = MAKELANGID( LANG_ENGLISH, SUBLANG_NEUTRAL ); 
	sprintf_s( szSubKey, sizeof(szSubKey) * sizeof(CHAR), "%s\\%03x", REGKEY_PERF, wLanguageID ); 
	dwKey = RegOpenKeyEx( HKEY_LOCAL_MACHINE, 
						szSubKey, 
						0, 
						KEY_READ, 
						&hKeyNames 
						); 

	// Make sure that the key was found
	if (dwKey != ERROR_SUCCESS) 
	{ 
		goto exit; 
	} 

	// Get the buffer size for the counter names 
	dwKey = RegQueryValueEx( hKeyNames, 
							REGSUBKEY_COUNTERS, 
							NULL, 
							&dwType, 
							NULL, 
							&dwSize 
							); 

	// Check result
	if (dwKey != ERROR_SUCCESS) 
	{ 
		goto exit; 
	} 

	// Allocate, check, and clear buffer for counter names
	lpcBuff = (CHAR *)malloc( dwSize ); 
	if (lpcBuff == NULL) 
	{ 
		goto exit; 
	} 
	memset( lpcBuff, 0, dwSize ); 

	// Read counter names from registry 
	dwKey = RegQueryValueEx( hKeyNames, 
							REGSUBKEY_COUNTERS, 
							NULL, 
							&dwType, 
							(unsigned char *)lpcBuff, 
							&dwSize 
							); 

	// Check result
	if (dwKey != ERROR_SUCCESS) 
	{ 
		goto exit; 
	} 

	// Loop thru the counter names looking for the following counters: 
	// 
	//      1.  "Process"           process name 
	//      2.  "ID Process"        process id 
	// 
	// Buffer contains multiple null terminated strings and then 
	// finally null terminated at the end.  Strings are in pairs of 
	// counter number and counter name. 
	pStr = lpcBuff; 
	while (*pStr) 
	{ 
		if (pStr > lpcBuff)
		{ 
			for( pStr2 = pStr - 2; isdigit((unsigned char) *pStr2); pStr2--) ; 
		} 
		
		if (_stricmp( pStr, PROCESS_COUNTER ) == 0) 
		{ 
			// look backwards for the counter number 
			for( pStr2 = pStr - 2; isdigit((unsigned char) *pStr2); pStr2--) ; 
			strcpy_s( szSubKey, sizeof(szSubKey) * sizeof(CHAR), pStr2 + 1 ); 
		} 
		else if (_stricmp(pStr, PROCESSID_COUNTER) == 0) 
		{ 
			// look backwards for the counter number 
			for( pStr2 = pStr - 2; isdigit((unsigned char) *pStr2); pStr2--) ; 
			dwProcessIdTitle = atol( pStr2 + 1 ); 
		} 

		// Next string 
		pStr += (strlen(pStr) + 1); 
	} 

	// Free the counter names buffer 
	free( lpcBuff ); 

	// Allocate initial buffer for performance data 
	dwSize = INITIAL_SIZE; 
	lpcBuff = (CHAR *)malloc( dwSize ); 
	if (lpcBuff == NULL) 
	{ 
		goto exit; 
	} 
	memset( lpcBuff, 0, dwSize ); 

	// Retrieve performance data
	while (TRUE) 
	{ 
		dwKey = RegQueryValueEx( HKEY_PERFORMANCE_DATA, 
								szSubKey, 
								NULL, 
								&dwType, 
								(unsigned char *)lpcBuff, 
								&dwSize 
								); 

		pPerf = (PPERF_DATA_BLOCK)lpcBuff; 

		// Check for success and valid perf data block signature 
		if ((dwKey == ERROR_SUCCESS) && 
			(dwSize > 0) && 
			(pPerf)->Signature[0] == (WCHAR)'P' && 
			(pPerf)->Signature[1] == (WCHAR)'E' && 
			(pPerf)->Signature[2] == (WCHAR)'R' && 
			(pPerf)->Signature[3] == (WCHAR)'F' ) 
		{ 
			break; 
		} 

		// If buffer is not big enough, reallocate and try again 
		if (dwKey == ERROR_MORE_DATA) 
		{ 
			dwSize += EXTEND_SIZE; 
			lpcBuff = (CHAR *)realloc( lpcBuff, dwSize ); 
			memset( lpcBuff, 0, dwSize ); 
		} 
		else 
		{ 
			goto exit; 
		} 
	} 

	// Set the perf_object_type pointer 
	pObj = (PPERF_OBJECT_TYPE) ((DWORD)pPerf + pPerf->HeaderLength); 

	// loop thru the performance counter definition records looking 
	// for the process id counter and then save its offset 
	pCounterDef = (PPERF_COUNTER_DEFINITION) ((DWORD)pObj + pObj->HeaderLength); 
	for (i = 0; i < (DWORD)pObj->NumCounters; i++) 
	{ 
		if (pCounterDef->CounterNameTitleIndex == dwProcessIdTitle) 
		{ 
			dwProcessIdCounter = pCounterDef->CounterOffset; 
			break; 
		} 
		pCounterDef++; 
	} 

	dwNumTasks = min( dwLimit, (DWORD)pObj->NumInstances ); 

	pInst = (PPERF_INSTANCE_DEFINITION) ((DWORD)pObj + pObj->DefinitionLength); 

	// loop thru the performance instance data extracting each process name 
	// and process id 
	for (i = 0; i < dwNumTasks; i++) 
	{ 
		// pointer to the process name 
		pStr = (LPSTR) ((DWORD)pInst + pInst->NameOffset); 

		// Convert it to ascii 
		dwKey = WideCharToMultiByte( CP_ACP, 
									0, 
									(LPCWSTR)pStr, 
									-1, 
									szProcessName, 
									sizeof(szProcessName), 
									NULL, 
									NULL 
									); 

		if (!dwKey) 
		{ 
			// if we cannot convert the string then use default
			strcpy_s( pTask->ProcessName, sizeof(pTask->ProcessName), UNKNOWN_TASK ); 
		} 

		// Perhaps append ".EXE"
		if (strlen(szProcessName)+4 <= sizeof(pTask->ProcessName)) 
		{ 
			strcpy_s( pTask->ProcessName, sizeof(pTask->ProcessName), szProcessName ); 
			strcat_s( pTask->ProcessName, sizeof(pTask->ProcessName), ".exe" ); 
		} 

		// Get process id 
		pCounter = (PPERF_COUNTER_BLOCK) ((DWORD)pInst + pInst->ByteLength); 
//		pTask->flags = 0; 
		pTask->dwProcessId = *((LPDWORD) ((DWORD)pCounter + dwProcessIdCounter)); 
		if (pTask->dwProcessId == 0) 
		{ 
			// Error indication
			pTask->dwProcessId = (DWORD)-2; 
		} 

		// next process 
		pTask++; 
		pInst = (PPERF_INSTANCE_DEFINITION) ((DWORD)pCounter + pCounter->ByteLength); 
	} 

exit: 
	if (lpcBuff) 
	{ 
		// Release allocated memory
		free( lpcBuff ); 
	} 

	RegCloseKey( hKeyNames ); 
	RegCloseKey( HKEY_PERFORMANCE_DATA ); 

	return dwNumTasks; 
} 

//--------------------------------------------------------------------------------------------------
// PURPOSE: Kills the process described by this TASK_LIST array entry.
// REQUIRE: None.
// PROMISE: Returns false if task could not be terminated, otherwise true.
// ARGS:	taskObject: pointer to data structure describing a specific process.
// AUTHOR:	Wayne Lenius
// NOTE:	This function is just an internal support function, and is not exported from the DLL
bool killProcess(PTASK_LIST taskObject)
{
	// Get a handle to this process
	HANDLE hProcess = OpenProcess( PROCESS_ALL_ACCESS, FALSE, 
		taskObject->dwProcessId ); 
    
	// Error, could not get handle
	if (hProcess == NULL) 
	{ 
		return false; 
	} 

	// Check the forced termination
	if (!TerminateProcess( hProcess, 1 )) 
	{ 
		// Termination failed, just close the handle and return
		CloseHandle( hProcess ); 
		return false; 
	} 

	// Termination success, close the handle and return
	CloseHandle( hProcess ); 
	return true; 
}

//--------------------------------------------------------------------------------------------------
bool windowIsAncestorWindowOf(HWND hPotentialChild, HWND hParent)
{
	// keep getting the parent window until either there is no
	// more parent window, or until the parent window's handle
	// matches the specified parent window's handle.
	HWND hTemp;
	do
	{
		hTemp = ::GetParent(hPotentialChild);
		if (hTemp == hParent)
			return true;
		else
			hPotentialChild = hTemp;
	}
	while (hTemp != __nullptr);

	// none of the ancestor windows is the specified parent window
	return false;
}

//--------------------------------------------------------------------------------------------------
HWND windowHasAncestorOfClass(HWND hChild, const char *pszWindowClassName)
{
	// keep getting the parent window until either there is no
	// more parent window, or until the parent window's class is the specified classname
	HWND hTemp;
	do
	{
		hTemp = ::GetParent(hChild);

		char pszTemp[128];

		// if we could not determine the class name, then assume that
		// no ancestor window exists of the type that we are looking for.
		if (!GetClassName(hTemp, pszTemp, sizeof(pszTemp)))
			return NULL;

		// if the current window's class name is the same as what we are looking for,
		// return its handle, otherwise keep searching higher
		if (_strcmpi(pszTemp, pszWindowClassName) == 0)
			return hTemp;
		else
			hChild = hTemp;
	}
	while (hTemp != __nullptr);

	// none of the ancestor windows was of the given class name, so return NULL.
	return NULL;
}

//--------------------------------------------------------------------------------------------------
const char *getDefaultWindowsDialogClassName()
{
	static const char *pszDefaultWindowsDialogClassName = "#32770";
	return pszDefaultWindowsDialogClassName;
}

//--------------------------------------------------------------------------------------------------
bool windowIsOfDefaultWindowsDialogClass(HWND hWnd)
{
	// determine the class name of the specified window
	char pszTemp[128];
	if (!GetClassName(hWnd, pszTemp, sizeof(pszTemp)))
		return false;

	// if the class name is the name of the default windows dialog class, then
	// return true, otherwise return false.
	if (_strcmpi(pszTemp, getDefaultWindowsDialogClassName()) == 0)
		return true;
	else
		return false;
}

//--------------------------------------------------------------------------------------------------
DWORD getTaskList(TASK_LIST arrTaskList[], DWORD dwMaxTasks)
{
	OSVERSIONINFO		verInfo = {0}; 
	LPGetTaskList		GetTaskListProc; 
	LPEnableDebugPriv	EnableDebugPrivProc; 

	// Determine operating system version
	verInfo.dwOSVersionInfoSize = sizeof( verInfo ); 
	GetVersionEx( &verInfo ); 

	// Point to appropriate function for determining task list
	// and enabling debug privileges
	switch (verInfo.dwPlatformId) 
	{ 
	case VER_PLATFORM_WIN32_NT: 
		// Check for Windows 2000
		if (verInfo.dwMajorVersion >= 5)
		{
			GetTaskListProc     = getTaskList95; 
			EnableDebugPrivProc = enableDebugPrivNT; 
		}
		// Check for Windows NT 4.0
		else if (verInfo.dwMajorVersion == 4)
		{
			GetTaskListProc     = getTaskListNT; 
			EnableDebugPrivProc = enableDebugPrivNT; 
		}
		else
		{
			// Not supported
			return 0;
		}
		break; 

	case VER_PLATFORM_WIN32_WINDOWS: 
		GetTaskListProc     = getTaskList95; 
		EnableDebugPrivProc = enableDebugPriv95; 
		break; 

	default: 
		return 0; 
	} 

    /////////////////
    // Set privileges
    /////////////////
    EnableDebugPrivProc(); 

	///////////////////////
	// Create the task list
	///////////////////////
	return GetTaskListProc(arrTaskList, dwMaxTasks); 
	
}
//--------------------------------------------------------------------------------------------------
unsigned long killNamedProcess(const char *pszProcessName)
{ 
	unsigned long	ulNumProcessesKilled = 0;
	TASK_LIST		arrTaskList[gulMAX_TASKS]; 
	int				iNumTasks = 0;

	// get the current task list
	iNumTasks = getTaskList(arrTaskList, gulMAX_TASKS);

	//////////////
	// Check tasks
	//////////////
	for (int i = 0; i < iNumTasks; i++)
	{
		// Compare task name against specified process name
		if (_stricmp( pszProcessName, arrTaskList[i].ProcessName) == 0)
		{
			// Kill the process
			killProcess( &(arrTaskList[i]) );
			ulNumProcessesKilled++;
		}
	}

	return ulNumProcessesKilled;

//	return 0;
}
//--------------------------------------------------------------------------------------------------
// example: strFileExtension = ".rsd"
void registerFileAssociations(const string& strFileExtension,
							  const string& strFileTypeDescription,
							  const string& strFullPathToEXE,
							  bool bSkipIfKeysExist,
							  unsigned long nDefaultIconIndex)
{
	// add registry entries pertaining to the specified file extension
	RegistryPersistenceMgr reg(HKEY_CLASSES_ROOT, "");

	// if the caller has requested this operation to be skipped if
	// the two main keys exist, then do so
	if (bSkipIfKeysExist && reg.folderExists(strFileExtension) &&
		reg.folderExists(strFileTypeDescription))
	{
		return;
	}

	// add the entries for the extension
	reg.createFolder(strFileExtension);
	reg.createKey(strFileExtension, "", strFileTypeDescription);

	// add the entries in the registry that associate the specified file extension
	// with the specified icon in the specified EXE
	string strDefaultIcon = strFullPathToEXE + string(",") + asString(nDefaultIconIndex);
	reg.createFolder(strFileTypeDescription);
	string strTemp;
	strTemp = strFileTypeDescription + string("\\DefaultIcon");
	reg.createFolder(strTemp.c_str());
	reg.createKey(strTemp.c_str(), "", strDefaultIcon.c_str());

	// add the registry key which specifies a textual description for
	// the specified extension when the mouse is hovered on top of a file
	// Windows explorer
	reg.createKey(strFileTypeDescription, "", strFileTypeDescription);

	// set the RuleSetEditor.exe (assumed to be in the current directory)
	// as the default application to open .RSD files
	strTemp = strFileTypeDescription + string("\\shell");
	reg.createFolder(strTemp.c_str());
	strTemp += "\\open";
	reg.createFolder(strTemp.c_str());
	strTemp += "\\command";
	reg.createFolder(strTemp.c_str());
	string strEXEFullPath = strFullPathToEXE + string(" \"%1\"");
	reg.createKey(strTemp.c_str(), "", strEXEFullPath.c_str());
}
//--------------------------------------------------------------------------------------------------
// example: strFileExtension = ".rsd"
void unregisterFileAssociations(const string& strFileExtension,
											  const string& strFileTypeDescription)
{
	// when deleting these folders, we are passing "false" as the argument
	// because multiple unregistrations in sequence will otherwise cause
	// an error.
	RegistryPersistenceMgr reg(HKEY_CLASSES_ROOT, "");	
	reg.deleteFolder(strFileExtension, false);
	reg.deleteFolder(strFileTypeDescription, false);
}
//-------------------------------------------------------------------------------------------------
string getAppFullPath()
{
	// NOTE: argv[0] does not give the full path to the EXE.  It just
	// gives what was typed at the commandline, which may be just the
	// name of the exe (without the path).
	char pszEXEFullPath[MAX_PATH + 1];
	if (::GetModuleFileName(AfxGetApp()->m_hInstance, 
		pszEXEFullPath, MAX_PATH) == 0)
	{
		UCLIDException ue("ELI06791", "Unable to retrieve module file name!");
		throw ue;
	}

	return string(pszEXEFullPath);
}
//--------------------------------------------------------------------------------------------------
void flashWindow(HWND hWnd, bool bSetFocus)
{
	// set the focus on the window that has the image already open
	// in it.
	if (bSetFocus)
	{
		SetFocus(hWnd);
	}

	// flash the window with the open image
	for (int i = 0; i < 10; i++)
	{
		FlashWindow(hWnd, TRUE);
		Sleep(100);
	}
}
//--------------------------------------------------------------------------------------------------
string getPlatformAsString(PLATFORM platform)
{
	string strReturnString = "";

	switch(platform)
	{
	case WIN9X:
		strReturnString = "Windows 9X";
		break;
		
	case WINNT:
		strReturnString = "Windows NT";
		break;

	case WIN2KSERVER:
		strReturnString = "Windows 2000 Server";
		break;

	case WINXP:
		strReturnString = "Windows XP";
		break;

	case WIN2003SERVER:
		strReturnString = "Windows 2003 Server";
		break;

	case WINVISTA:
		strReturnString = "Windows Vista";
		break;

	case WIN2008SERVER:
		strReturnString = "Windows 2008 Server";
		break;

	default:
		strReturnString = "Unknown";
		break;
	}

	return strReturnString;
}
//--------------------------------------------------------------------------------------------------
PLATFORM GetPlatform()
{
	// Create an OSVERSIONINFOEX struct and zero it out
	OSVERSIONINFOEX osvi;
	ZeroMemory(&osvi, sizeof(OSVERSIONINFOEX));

	// Set the sizeof flag
	osvi.dwOSVersionInfoSize = sizeof(OSVERSIONINFOEX);

	// Attempt to get the version info
	if (!GetVersionEx((OSVERSIONINFO*) &osvi))
	{
		// If failed to get the version info, return UNKNOWN
		return UNKNOWN;
	}

	// Value for return information
	PLATFORM platReturnVal = UNKNOWN;

	// Check the platform ID first
	switch (osvi.dwPlatformId)
	{
	case VER_PLATFORM_WIN32_WINDOWS:
		platReturnVal = WIN9X;
		break;

	// WinNT platforms, need to check the major versions first
	case VER_PLATFORM_WIN32_NT:
		{
			switch(osvi.dwMajorVersion)
			{
			case 4:
				platReturnVal = WINNT;
				break;

			case 5:
				{
					// Win2K, WinXP, Win2003
					// Check minor versions to distinguish
					switch(osvi.dwMinorVersion)
					{
					case 0:
						platReturnVal = WIN2KSERVER;
						break;

					case 1:
						platReturnVal = WINXP;
						break;

					case 2:
						platReturnVal = WIN2003SERVER;
						break;
					}
				}
				break;

			case 6:
				{
					// Version 6 - WinVista and Win2008
					// Check product type to distinguish
					if (osvi.wProductType == VER_NT_SERVER)
					{
						platReturnVal = WIN2008SERVER;
					}
					else
					{
						platReturnVal = WINVISTA;
					}
				}
				break;
			}
		}
		break;
	}

	return platReturnVal;
}
//--------------------------------------------------------------------------------------------------
bool isPlatformWin2KOrGreater(PLATFORM platform)
{
	switch(platform)
	{
	case WIN2KSERVER:
	case WINXP:
	case WIN2003SERVER:
	case WINVISTA:
	case WIN2008SERVER:
		return true;
		break;

	default:
		return false;
		break;
	}
}

//--------------------------------------------------------------------------------------------------
// HandleCloser class
//--------------------------------------------------------------------------------------------------
HandleCloser::HandleCloser(HANDLE hHandle)
:m_hHandle(hHandle)
{
}
//--------------------------------------------------------------------------------------------------
HandleCloser::~HandleCloser()
{
	try
	{
		close(); 
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16422");
}
//--------------------------------------------------------------------------------------------------
void HandleCloser::close()
{
	if (m_hHandle)
	{
		if (!CloseHandle(m_hHandle))
		{
			UCLIDException ue("ELI05555", "Unable to close handle!");
			ue.addDebugInfo("Handle", (long) m_hHandle);
			throw ue;
		}

		// reset the handle
		m_hHandle = NULL;
	}
}
//--------------------------------------------------------------------------------------------------
ClipboardOpenerCloser::ClipboardOpenerCloser(CWnd *pWnd)
:m_pWnd(pWnd)
{
	if (!m_pWnd->OpenClipboard())
	{
		throw UCLIDException("ELI05560", "Unable to open the clipboard!");
	}
}
//--------------------------------------------------------------------------------------------------
ClipboardOpenerCloser::~ClipboardOpenerCloser()
{
	try
	{
		close();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16576");
}
//--------------------------------------------------------------------------------------------------
void ClipboardOpenerCloser::close()
{
	if (m_pWnd)
	{
		m_pWnd = NULL;

		if (!CloseClipboard())
		{
			UCLIDException ue("ELI05559", "Unable to close clipboard!");
			ue.addDebugInfo("GetLastError()", GetLastError());
			throw ue;
		}
	}
}

//--------------------------------------------------------------------------------------------------
// GlobalMemoryHandler class
//--------------------------------------------------------------------------------------------------
GlobalMemoryHandler::GlobalMemoryHandler(HGLOBAL hData)
:m_pData(NULL), m_hData(hData)
{
	lock(m_hData);
}
//--------------------------------------------------------------------------------------------------
GlobalMemoryHandler::~GlobalMemoryHandler()
{
	try
	{
		unlock();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16423");
}
//--------------------------------------------------------------------------------------------------
void GlobalMemoryHandler::lock()
{
	lock(m_hData);
}
//--------------------------------------------------------------------------------------------------
void GlobalMemoryHandler::lock(HGLOBAL hData)
{
	// verify non-null handle
	if (hData == NULL)
	{
		throw UCLIDException("ELI05561", "Invalid global memory handle!");
	}

	// unlock current global memory object, if any
	unlock();

	// lock the specified global memory object
	m_hData = hData;
	m_pData = GlobalLock(m_hData);
	if (m_pData == NULL)
	{
		m_hData = NULL;
		UCLIDException ue("ELI05557", "Unable to lock global memory!");
		ue.addDebugInfo("GetLastError()", (long) GetLastError());
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
void GlobalMemoryHandler::unlock()
{
	if (m_pData)
	{
		BOOL bResult = GlobalUnlock(m_hData);
		m_pData = NULL;

		// GlobalUnlock has a bit of an odd return scheme
		// if the data stays locked because the lock count 
		// on the data is greater than one than it returns
		// the lock count on the data.  It return 0 if there is
		// an error or the has become unlocked (the lockcount is 0)
		// the way to differentiate these two cases is to call 
		// GetLastError.  If it returns NO_ERROR than the data
		// was unlocked otherwise it returns the error info
		if (bResult == 0)
		{
			DWORD dwErr = GetLastError();
			if (dwErr != NO_ERROR)
			{
				UCLIDException ue("ELI05558", "Unable to unlock global memory!");
				ue.addDebugInfo("GetLastError()", (long)dwErr );
				throw ue;
			}
		}
	}
}
//--------------------------------------------------------------------------------------------------
void* GlobalMemoryHandler::getData()
{
	if (m_pData)
	{
		return m_pData;
	}
	else
	{
		throw UCLIDException("ELI05556", "No object has been locked yet!");
	}
}
//--------------------------------------------------------------------------------------------------
GlobalMemoryHandler& GlobalMemoryHandler::operator=(HGLOBAL hData)
{
	// lock the specified global memory object
	lock(hData);

	return *this;
}
//--------------------------------------------------------------------------------------------------
GlobalMemoryHandler::operator HGLOBAL()
{
	return m_hData;
}

//--------------------------------------------------------------------------------------------------
// ForegroundWindowRestorer class
//--------------------------------------------------------------------------------------------------
ForegroundWindowRestorer::ForegroundWindowRestorer()
{
	m_hwndActive = ::GetForegroundWindow();
}
//--------------------------------------------------------------------------------------------------
ForegroundWindowRestorer::~ForegroundWindowRestorer()
{
	try
	{
		restore();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16424");
}
//--------------------------------------------------------------------------------------------------
void ForegroundWindowRestorer::restore()
{
	::SetForegroundWindow(m_hwndActive);
}

//--------------------------------------------------------------------------------------------------
// DropFinisher class
//--------------------------------------------------------------------------------------------------
DragDropFinisher::DragDropFinisher(HDROP hDrop)
:m_hDrop(hDrop)
{
}
//--------------------------------------------------------------------------------------------------
DragDropFinisher::~DragDropFinisher()
{
	try
	{
		finish();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16425");
}
//--------------------------------------------------------------------------------------------------
void DragDropFinisher::finish()
{
	if (m_hDrop)
	{
		DragFinish(m_hDrop);
		m_hDrop = NULL;
	}
}

//--------------------------------------------------------------------------------------------------
// WindowDisabler class
//--------------------------------------------------------------------------------------------------
WindowDisabler::WindowDisabler(HWND hWnd)
: m_hWnd(hWnd)
{
	if (m_hWnd != __nullptr)
	{
		::EnableWindow(m_hWnd, FALSE);
	}
}
//--------------------------------------------------------------------------------------------------
WindowDisabler::~WindowDisabler()
{
	try
	{
		if (m_hWnd != __nullptr)
		{
			::EnableWindow(m_hWnd, TRUE);
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI25305");
}
//--------------------------------------------------------------------------------------------------
