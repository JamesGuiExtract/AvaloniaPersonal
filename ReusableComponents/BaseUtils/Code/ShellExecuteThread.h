
#pragma once

#ifndef SHELL_EXECUTE_THREAD
#define SHELL_EXECUTE_THREAD

#include "BaseUtils.h"

#include "Win32Event.h"

// Class that wraps a separate thread which will load
// the ShellExecute function. This ShellExecute function has 
// a problem when working with CoInitializeEx(NULL, COINIT_MULTITHREADED)
// Read the following for details:
// http://support.microsoft.com/kb/287087

// thread data struct
class EXPORT_BaseUtils ThreadDataStruct
{
public:
	// Constructor
	ThreadDataStruct(HWND hwnd = NULL, LPCTSTR lpOperation = NULL, LPCTSTR lpFile = "", 
		LPCTSTR lpParams = NULL, LPCTSTR lpDir = "", INT nShowCmd = 0);

	// ShellExecute function params
	HWND m_hwnd;
    LPCTSTR m_lpOperation;
    LPCTSTR m_lpFile;
    LPCTSTR m_lpParameters;
    LPCTSTR m_lpDirectory;
    INT m_nShowCmd;
};

class EXPORT_BaseUtils ShellExecuteThread
{
public:
	// Constructor
	ShellExecuteThread(ThreadDataStruct * pTDS);

private:
	// threadDataStruct object
	ThreadDataStruct* m_pThreadData;

	// Event to signal the ShellExecute has been called
	Win32Event m_threadEndedEvent;

	// Thread to ShellExecute function
	static UINT LoadShellExcuteThread(void* pData);
};
 
#endif // SHELL_EXECUTE_THREAD

