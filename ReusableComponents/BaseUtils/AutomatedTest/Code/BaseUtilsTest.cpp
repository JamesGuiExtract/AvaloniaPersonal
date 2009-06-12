// BaseUtilsTest.cpp : Implementation of DLL Exports.


#include "stdafx.h"
#include "resource.h"
#include "BaseUtilsTest.h"


class CBaseUtilsTestModule : public CAtlDllModuleT< CBaseUtilsTestModule >
{
public :
	DECLARE_LIBID(LIBID_BaseUtilsTestLib)
	DECLARE_REGISTRY_APPID_RESOURCEID(IDR_BASEUTILSTEST, "{745BB7D4-6F9C-4990-85D2-83D39E76EF70}")
};

CBaseUtilsTestModule _AtlModule;

class CBaseUtilsTestApp : public CWinApp
{
public:

// Overrides
    virtual BOOL InitInstance();
    virtual int ExitInstance();

    DECLARE_MESSAGE_MAP()
};

BEGIN_MESSAGE_MAP(CBaseUtilsTestApp, CWinApp)
END_MESSAGE_MAP()

CBaseUtilsTestApp theApp;

BOOL CBaseUtilsTestApp::InitInstance()
{
    return CWinApp::InitInstance();
}

int CBaseUtilsTestApp::ExitInstance()
{
    return CWinApp::ExitInstance();
}


// Used to determine whether the DLL can be unloaded by OLE
STDAPI DllCanUnloadNow(void)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());
    return (AfxDllCanUnloadNow()==S_OK && _AtlModule.GetLockCount()==0) ? S_OK : S_FALSE;
}


// Returns a class factory to create an object of the requested type
STDAPI DllGetClassObject(REFCLSID rclsid, REFIID riid, LPVOID* ppv)
{
    return _AtlModule.DllGetClassObject(rclsid, riid, ppv);
}


// DllRegisterServer - Adds entries to the system registry
STDAPI DllRegisterServer(void)
{
    // registers object, typelib and all interfaces in typelib
    HRESULT hr = _AtlModule.DllRegisterServer();
	return hr;
}


// DllUnregisterServer - Removes entries from the system registry
STDAPI DllUnregisterServer(void)
{
	HRESULT hr = _AtlModule.DllUnregisterServer();
	return hr;
}

