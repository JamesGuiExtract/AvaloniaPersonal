#include "stdafx.h"
#include "ShellExecuteThread.h"
#include "UCLIDException.h"

using namespace std;

//-------------------------------------------------------------------------------------------------
// ThreadDataStruct class
//-------------------------------------------------------------------------------------------------
ThreadDataStruct::ThreadDataStruct(HWND hwnd, LPCTSTR lpOperation, LPCTSTR lpFile, 
								   LPCTSTR lpParams, LPCTSTR lpDir, INT nShowCmd)
: m_hwnd(hwnd),
m_lpOperation(lpOperation),
m_lpFile(lpFile),
m_lpParameters(lpParams),
m_lpDirectory(lpDir),
m_nShowCmd(nShowCmd)
{
}

//-------------------------------------------------------------------------------------------------
// ShellExecuteThread class
//-------------------------------------------------------------------------------------------------
ShellExecuteThread::ShellExecuteThread(ThreadDataStruct * pTDS)
:m_pThreadData(pTDS)
{
	// Start loading ShellExcute function
	AfxBeginThread(LoadShellExcuteThread, this);

	// Wait for the LoadShellExcuteThread to finish
	m_threadEndedEvent.messageWait();
}

//-------------------------------------------------------------------------------------------------
// thread proc
//-------------------------------------------------------------------------------------------------
UINT ShellExecuteThread::LoadShellExcuteThread(void* pData)
{
	try
	{
		// Cast back to ThreadDataStruct pointer
		ShellExecuteThread* pSET = (ShellExecuteThread *) pData;
		ASSERT_ARGUMENT("ELI15691", pSET != __nullptr);

		ThreadDataStruct* pTDS = pSET->m_pThreadData;
		ASSERT_ARGUMENT("ELI15693", pTDS != __nullptr);

		// call ShellExecute function
		ShellExecute(pTDS->m_hwnd, pTDS->m_lpOperation, pTDS->m_lpFile, 
			pTDS->m_lpParameters, pTDS->m_lpDirectory, pTDS->m_nShowCmd);

		// Signal the event object
		pSET->m_threadEndedEvent.signal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15692");

	return 0;
}
//-------------------------------------------------------------------------------------------------
