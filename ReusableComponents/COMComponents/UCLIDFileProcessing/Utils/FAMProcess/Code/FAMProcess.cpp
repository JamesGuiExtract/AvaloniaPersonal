// FAMProcess.cpp : Implementation of WinMain


#include "stdafx.h"
#include "resource.h"
#include "FAMProcess.h"

// This is basically the code from the CAtlExeModuleT definition in
// atlbase.h  The code is inaccessible in atlbase.h since we have included
// afxwin.h in our project.  I am not sure what the major issue is
// but this appears to work (we pulled a similar trick inside of SSOCR2
// so I believe this is the "workaround" for the issue).
class CFAMProcessModule : public CAtlModuleT< CFAMProcessModule >
{
public :
	DWORD m_dwMainThreadID;
	HANDLE m_hEventShutdown;
	DWORD m_dwTimeOut;
	DWORD m_dwPause;
	bool m_bDelayShutdown;
	bool m_bActivity;
	bool m_bComInitialized;

	CFAMProcessModule() throw()
		: m_dwMainThreadID(::GetCurrentThreadId()),
		m_dwTimeOut(5000),
		m_dwPause(1000),
		m_hEventShutdown(NULL),
		m_bDelayShutdown(true),
		m_bComInitialized(false)
	{
		HRESULT hr = CFAMProcessModule::InitializeCom();
		if (FAILED(hr))
		{
			// Ignore RPC_E_CHANGED_MODE if CLR is loaded. Error is due to CLR initializing
			// COM and InitializeCOM trying to initialize COM with different flags.
			if (hr != RPC_E_CHANGED_MODE || GetModuleHandle(_T("Mscoree.dll")) == NULL)
			{
				ATLASSERT(0);
				CAtlBaseModule::m_bInitFailed =	 true;
				return;
			}
		}
		else
		{
			m_bComInitialized = true;
		}


		_AtlComModule.ExecuteObjectMain(true);
	}

	~CFAMProcessModule() throw()
	{
		_AtlComModule.ExecuteObjectMain(false);

		// Call term functions before COM is uninitialized
		Term();

		// Clean up AtlComModule before COM is uninitialized
		_AtlComModule.Term();

		if (m_bComInitialized)
			CFAMProcessModule::UninitializeCom();
	}

	static HRESULT InitializeCom() throw()
	{
		return CoInitialize(NULL);
	}

	static void UninitializeCom() throw()
	{
		CoUninitialize();
	}

	LONG Unlock() throw()
	{
		LONG lRet = CComGlobalsThreadModel::Decrement(&m_nLockCnt);
		if (lRet == 0)
		{
			if (m_bDelayShutdown)
			{
				m_bActivity = true;
				::SetEvent(m_hEventShutdown); // tell monitor that we transitioned to zero
			}
			else
			{
				::PostThreadMessage(m_dwMainThreadID, WM_QUIT, 0, 0);
			}
		}

		return lRet;
	}

	void MonitorShutdown() throw()
	{
		while (1)
		{
			::WaitForSingleObject(m_hEventShutdown, INFINITE);
			DWORD dwWait = 0;
			do
			{
				m_bActivity = false;
				dwWait = ::WaitForSingleObject(m_hEventShutdown, m_dwTimeOut);
			} while (dwWait == WAIT_OBJECT_0);
			// timed out
			if (!m_bActivity && m_nLockCnt == 0) // if no activity let's really bail
			{
					break;
			}
		}
		::CloseHandle(m_hEventShutdown);
		::PostThreadMessage(m_dwMainThreadID, WM_QUIT, 0, 0);
	}

	HANDLE StartMonitor() throw()
	{
		m_hEventShutdown = ::CreateEvent(NULL, false, false, NULL);
		if (m_hEventShutdown == NULL)
        {
			return NULL;
        }
		DWORD dwThreadID;
		HANDLE hThread = ::CreateThread(NULL, 0, MonitorProc, this, 0, &dwThreadID);
        if(hThread==NULL)
        {
    		::CloseHandle(m_hEventShutdown);
        }
		return hThread;
	}

	static DWORD WINAPI MonitorProc(void* pv) throw()
	{
		CFAMProcessModule* p = static_cast<CFAMProcessModule*>(pv);
		p->MonitorShutdown();
		return 0;
	}

	int WinMain(int nShowCmd) throw()
	{
		if (CAtlBaseModule::m_bInitFailed)
		{
			ATLASSERT(0);
			return -1;
		}
		CFAMProcessModule* pT = static_cast<CFAMProcessModule*>(this);
		HRESULT hr = S_OK;

		LPTSTR lpCmdLine = GetCommandLine(); //this line necessary for _ATL_MIN_CRT
		if (pT->ParseCommandLine(lpCmdLine, &hr) == true)
			hr = pT->Run(nShowCmd);

#ifdef _DEBUG
		// Prevent false memory leak reporting. ~CAtlWinModule may be too late.
		_AtlWinModule.Term();		
#endif	// _DEBUG
		return hr;
	}

	// Scan command line and perform registration
	// Return value specifies if server should run

	// Parses the command line and registers/unregisters the rgs file if necessary
	bool ParseCommandLine(LPCTSTR lpCmdLine, HRESULT* pnRetCode) throw()
	{
		*pnRetCode = S_OK;

		TCHAR szTokens[] = _T("-/");

		CFAMProcessModule* pT = static_cast<CFAMProcessModule*>(this);
		LPCTSTR lpszToken = FindOneOf(lpCmdLine, szTokens);
		while (lpszToken != __nullptr)
		{
			if (WordCmpI(lpszToken, _T("UnregServer"))==0)
			{
				*pnRetCode = pT->UnregisterServer(TRUE);
				if (SUCCEEDED(*pnRetCode))
					*pnRetCode = pT->UnregisterAppId();
				return false;
			}

			// Register as Local Server
			if (WordCmpI(lpszToken, _T("RegServer"))==0)
			{
				*pnRetCode = pT->RegisterAppId();
				if (SUCCEEDED(*pnRetCode))
					*pnRetCode = pT->RegisterServer(TRUE);
				return false;
			}

			lpszToken = FindOneOf(lpszToken, szTokens);
		}

		return true;
	}

	HRESULT PreMessageLoop(int /*nShowCmd*/) throw()
	{
		HRESULT hr = S_OK;
		CFAMProcessModule* pT = static_cast<CFAMProcessModule*>(this);
		pT;

		// In order to have a single exe per object instance, set the flag to REGCLS_SINGLEUSE
		// http://www.tech-archive.net/Archive/VC/microsoft.public.vc.atl/2004-12/0352.html
		hr = pT->RegisterClassObjects(CLSCTX_LOCAL_SERVER, 
			REGCLS_SINGLEUSE);
		if (hr == S_OK)
		{
			if (m_bDelayShutdown && !pT->StartMonitor())
			{
				hr = E_FAIL;
			}
		}
		else
		{
			m_bDelayShutdown = false;
		}

		ATLASSERT(SUCCEEDED(hr));
		return hr;
	}

	HRESULT PostMessageLoop() throw()
	{
		HRESULT hr = S_OK;

		CFAMProcessModule* pT = static_cast<CFAMProcessModule*>(this);
		hr = pT->RevokeClassObjects();
		if (m_bDelayShutdown)
			Sleep(m_dwPause); //wait for any threads to finish

		return hr;
	}

	void RunMessageLoop() throw()
	{
		MSG msg;
		while (GetMessage(&msg, 0, 0, 0) > 0)
		{
			TranslateMessage(&msg);
			DispatchMessage(&msg);
		}
	}

	HRESULT Run(int nShowCmd = SW_HIDE) throw()
	{
		HRESULT hr = S_OK;

		CFAMProcessModule* pT = static_cast<CFAMProcessModule*>(this);
		hr = pT->PreMessageLoop(nShowCmd);

		// Call RunMessageLoop only if PreMessageLoop returns S_OK.
		if (hr == S_OK)
		{
			pT->RunMessageLoop();
		}

		// Call PostMessageLoop if PreMessageLoop returns success.
		if (SUCCEEDED(hr))
		{
			hr = pT->PostMessageLoop();
		}

		ATLASSERT(SUCCEEDED(hr));
		return hr;
	}

	// Register/Revoke All Class Factories with the OS (EXE only)
	HRESULT RegisterClassObjects(DWORD dwClsContext, DWORD dwFlags) throw()
	{
		return AtlComModuleRegisterClassObjects(&_AtlComModule, dwClsContext, dwFlags);
	}
	HRESULT RevokeClassObjects() throw()
	{
		return AtlComModuleRevokeClassObjects(&_AtlComModule);
	}

public :

	DECLARE_LIBID(LIBID_FAMProcessLib)
	DECLARE_REGISTRY_APPID_RESOURCEID(IDR_FAMPROCESS, "{08463A92-A444-48AF-8822-693C4F6E1F08}")
};

CFAMProcessModule _AtlModule;

//
extern "C" int WINAPI _tWinMain(HINSTANCE /*hInstance*/, HINSTANCE /*hPrevInstance*/, 
                                LPTSTR /*lpCmdLine*/, int nShowCmd)
{
    return _AtlModule.WinMain(nShowCmd);
}

