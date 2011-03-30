// SSOCR2.cpp : Implementation of WinMain


// Note: Proxy/Stub Information
//      To build a separate proxy/stub DLL, 
//      run nmake -f SSOCR2ps.mk in the project directory.

#include "stdafx.h"
#include "resource.h"
#include "SSOCR2.h"
#include "SSOCR2_i.c"
#include "ScansoftOCR2.h"

#include <UCLIDException.h>

#include <initguid.h>

const DWORD dwTimeOut = 0; // time for EXE to be idle before shutting down
const DWORD dwPause = 0; // time to wait for threads to finish up

//-------------------------------------------------------------------------------------------------
// Passed to CreateThread to monitor the shutdown event
static DWORD WINAPI MonitorProc(void* pv)
{
	try
	{
		CExeModule* p = (CExeModule*)pv;
		p->MonitorShutdown();
		return 0;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI12827");
	return 1;
}
//-------------------------------------------------------------------------------------------------
LONG CExeModule::Unlock()
{
    LONG l = CComModule::Unlock();
	try
	{
		if (l == 0)
		{
			bActivity = true;
			SetEvent(hEventShutdown); // tell monitor that we transitioned to zero
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI12766");
    return l;
}
//-------------------------------------------------------------------------------------------------
//Monitors the shutdown event
void CExeModule::MonitorShutdown()
{
	try
	{
		while (1)
		{
			WaitForSingleObject(hEventShutdown, INFINITE);
			DWORD dwWait=0;
			do
			{
				bActivity = false;
				dwWait = WaitForSingleObject(hEventShutdown, dwTimeOut);
			} while (dwWait == WAIT_OBJECT_0);
			// timed out
			if (!bActivity && m_nLockCnt == 0) // if no activity let's really bail
			{
#if _WIN32_WINNT >= 0x0400 & defined(_ATL_FREE_THREADED)
				CoSuspendClassObjects();
				if (!bActivity && m_nLockCnt == 0)
#endif
					break;
			}
		}
		CloseHandle(hEventShutdown);
		PostThreadMessage(dwThreadID, WM_QUIT, 0, 0);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI12765");
}
//-------------------------------------------------------------------------------------------------
bool CExeModule::StartMonitor()
{
	try
	{
		hEventShutdown = CreateEvent(NULL, false, false, NULL);
		if (hEventShutdown == NULL)
			return false;
		DWORD dwThreadID;
		HANDLE h = CreateThread(NULL, 0, MonitorProc, this, 0, &dwThreadID);
		return (h != __nullptr);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI12764");
	return false;
}
//-------------------------------------------------------------------------------------------------
CExeModule _Module;

BEGIN_OBJECT_MAP(ObjectMap)
OBJECT_ENTRY(CLSID_ScansoftOCR2, CScansoftOCR2)
END_OBJECT_MAP()
//-------------------------------------------------------------------------------------------------
LPCTSTR FindOneOf(LPCTSTR p1, LPCTSTR p2)
{
    while (p1 != __nullptr && *p1 != NULL)
    {
        LPCTSTR p = p2;
        while (p != __nullptr && *p != NULL)
        {
            if (*p1 == *p)
                return CharNext(p1);
            p = CharNext(p);
        }
        p1 = CharNext(p1);
    }
    return NULL;
}
//-------------------------------------------------------------------------------------------------
class CMyApp : public CWinApp
      {
      public:
         virtual BOOL InitInstance();
         virtual int ExitInstance();
      protected:
      BOOL m_bRun;
};

CMyApp theApp;
//-------------------------------------------------------------------------------------------------
BOOL CMyApp::InitInstance()
{
	try
	{
		// Initialize OLE libraries.
		if (!AfxOleInit())
		{
			AfxMessageBox(_T("OLE Initialization Failed!"));
			return FALSE;
		}

		// Initialize CcomModule.
		_Module.Init(ObjectMap, m_hInstance);
		_Module.dwThreadID = GetCurrentThreadId();
		
		// Check command line arguments.
		TCHAR szTokens[] = _T("-/");
		m_bRun = TRUE;
		LPCTSTR lpszToken = FindOneOf(m_lpCmdLine, szTokens);
		while (lpszToken != __nullptr)
		{
			// Register ATL and MFC class factories.
			if (lstrcmpi(lpszToken, _T("Embedding"))==0 ||
				lstrcmpi(lpszToken, _T("Automation"))==0)
			{
				AfxOleSetUserCtrl(FALSE);
				break;
			}
			// Unregister servers.
			// There is no unregistration code for MFC
			// servers. Refer to <WWLINK TYPE="ARTICLE" VALUE="Q186212">Q186212</WWLINK> "How To  Unregister MFC
			// Automation Servers" for adding unregistration
			// code.
			else if (lstrcmpi(lpszToken, _T("UnregServer"))==0)
			{
				VERIFY(SUCCEEDED(_Module.UpdateRegistryFromResource(IDR_SSOCR2, FALSE)));
				VERIFY(SUCCEEDED(_Module.UnregisterServer(TRUE)));
				m_bRun = FALSE;
				break;
			}
			// Register ATL and MFC objects in the registry.
			else if (lstrcmpi(lpszToken, _T("RegServer"))==0)
			{
				VERIFY(SUCCEEDED(_Module.UpdateRegistryFromResource(IDR_SSOCR2, TRUE)));
				VERIFY(SUCCEEDED(_Module.RegisterServer(TRUE)));
				COleObjectFactory::UpdateRegistryAll();
				m_bRun = FALSE;
				break;
			}
			lpszToken = FindOneOf(lpszToken, szTokens);
		}
		if (m_bRun)
		{
			// Comment out the next line if not using VC 6-generated
			// code.
			_Module.StartMonitor();
			
			VERIFY(SUCCEEDED(_Module.RegisterClassObjects(CLSCTX_LOCAL_SERVER, REGCLS_SINGLEUSE)));
			VERIFY(COleObjectFactory::RegisterAll());
			// To run the EXE standalone, you need to create a window
			// and assign the CWnd* to m_pMainWnd.
			LPCTSTR szClass = AfxRegisterWndClass(NULL);
			m_pMainWnd = new CWnd;
			m_pMainWnd->CreateEx(0, szClass, _T("SomeName"), 0, CRect(0, 0, 0, 0), NULL, 1234);
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI12762");
	return TRUE;
}
//-------------------------------------------------------------------------------------------------
int CMyApp::ExitInstance()
{
	try
	{
		// MFC's class factories registration is
		// automatically revoked by MFC itself.
		if (m_bRun)
		{
			_Module.RevokeClassObjects();
			Sleep(dwPause); //wait for any threads to finish
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI12763");

    _Module.Term();
	return 0;
}
//-------------------------------------------------------------------------------------------------