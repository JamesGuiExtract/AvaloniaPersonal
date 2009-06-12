// ESActMaskPCE.cpp : Implementation of DLL Exports.


#include "stdafx.h"
#include "resource.h"
#include "ESActMaskPCE.h"

//--------------------------------------------------------------------------------------------------
class CESActMaskPCEModule : public CAtlDllModuleT< CESActMaskPCEModule >
{
public :
	DECLARE_LIBID(LIBID_ESActMaskPCELib)
	DECLARE_REGISTRY_APPID_RESOURCEID(IDR_ESACTMASKPCE, "{621DD94C-7CE7-46A2-A1E7-5814CA67CC9A}")
};

CESActMaskPCEModule _AtlModule;

//--------------------------------------------------------------------------------------------------
class CESActMaskPCEApp : public CWinApp
{
public:

// Overrides
    virtual BOOL InitInstance();
    virtual int ExitInstance();

    DECLARE_MESSAGE_MAP()
};

//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CESActMaskPCEApp, CWinApp)
END_MESSAGE_MAP()

//--------------------------------------------------------------------------------------------------
CESActMaskPCEApp theApp;

//--------------------------------------------------------------------------------------------------
BOOL CESActMaskPCEApp::InitInstance()
{
    return CWinApp::InitInstance();
}
//--------------------------------------------------------------------------------------------------
int CESActMaskPCEApp::ExitInstance()
{
    return CWinApp::ExitInstance();
}

//--------------------------------------------------------------------------------------------------
// Used to determine whether the DLL can be unloaded by OLE
STDAPI DllCanUnloadNow(void)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());
    return (AfxDllCanUnloadNow()==S_OK && _AtlModule.GetLockCount()==0) ? S_OK : S_FALSE;
}
//--------------------------------------------------------------------------------------------------
// Returns a class factory to create an object of the requested type
STDAPI DllGetClassObject(REFCLSID rclsid, REFIID riid, LPVOID* ppv)
{
    return _AtlModule.DllGetClassObject(rclsid, riid, ppv);
}
//--------------------------------------------------------------------------------------------------
// DllRegisterServer - Adds entries to the system registry
STDAPI DllRegisterServer(void)
{
    // registers object, typelib and all interfaces in typelib
    HRESULT hr = _AtlModule.DllRegisterServer();
	return hr;
}
//--------------------------------------------------------------------------------------------------
// DllUnregisterServer - Removes entries from the system registry
STDAPI DllUnregisterServer(void)
{
	HRESULT hr = _AtlModule.DllUnregisterServer();
	return hr;
}
//--------------------------------------------------------------------------------------------------
