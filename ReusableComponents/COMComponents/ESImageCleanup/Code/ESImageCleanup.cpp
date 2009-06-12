// ESImageCleanup.cpp : Implementation of DLL Exports.

#include "stdafx.h"
#include "resource.h"
#include "ESImageCleanup.h"
#include "ICCategories.h"

CComModule _Module;

#include <COMUtils.h>

class CESImageCleanupModule : public CAtlDllModuleT< CESImageCleanupModule >
{
public :
	DECLARE_LIBID(LIBID_ESImageCleanupLib)
	DECLARE_REGISTRY_APPID_RESOURCEID(IDR_ESIMAGECLEANUP, "{E03A978D-1FC5-4155-A281-A91585AB157D}")
};
//-------------------------------------------------------------------------------------------------
//CESImageCleanupModule _Module;
//-------------------------------------------------------------------------------------------------
class CESImageCleanupApp : public CWinApp
{
public:

// Overrides
    virtual BOOL InitInstance();
    virtual int ExitInstance();

    DECLARE_MESSAGE_MAP()
};
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CESImageCleanupApp, CWinApp)
END_MESSAGE_MAP()
//-------------------------------------------------------------------------------------------------
CESImageCleanupApp theApp;
//-------------------------------------------------------------------------------------------------
BOOL CESImageCleanupApp::InitInstance()
{
#ifdef _MERGE_PROXYSTUB
    if (!PrxDllMain(m_hInstance, DLL_PROCESS_ATTACH, NULL))
		return FALSE;
#endif
    return CWinApp::InitInstance();
}
//-------------------------------------------------------------------------------------------------
int CESImageCleanupApp::ExitInstance()
{
    return CWinApp::ExitInstance();
}
//-------------------------------------------------------------------------------------------------
// Used to determine whether the DLL can be unloaded by OLE
STDAPI DllCanUnloadNow(void)
{
#ifdef _MERGE_PROXYSTUB
    HRESULT hr = PrxDllCanUnloadNow();
    if (hr != S_OK)
        return hr;
#endif
    AFX_MANAGE_STATE(AfxGetStaticModuleState());
    return (AfxDllCanUnloadNow()==S_OK && _Module.GetLockCount()==0) ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
// Returns a class factory to create an object of the requested type
STDAPI DllGetClassObject(REFCLSID rclsid, REFIID riid, LPVOID* ppv)
{
#ifdef _MERGE_PROXYSTUB
    if (PrxDllGetClassObject(rclsid, riid, ppv) == S_OK)
        return S_OK;
#endif
    return _Module.DllGetClassObject(rclsid, riid, ppv);
}
//-------------------------------------------------------------------------------------------------
// DllRegisterServer - Adds entries to the system registry
STDAPI DllRegisterServer(void)
{
	// create the COM categories for this interface
	createCOMCategory(CATID_ICO_CLEANING_OPERATIONS, ICO_CLEANING_OPERATIONS_CATEGORYNAME);

    // registers object, typelib and all interfaces in typelib
    HRESULT hr = _Module.DllRegisterServer();
#ifdef _MERGE_PROXYSTUB
    if (FAILED(hr))
        return hr;
    hr = PrxDllRegisterServer();
#endif
	return hr;
}
//-------------------------------------------------------------------------------------------------
// DllUnregisterServer - Removes entries from the system registry
STDAPI DllUnregisterServer(void)
{
	HRESULT hr = _Module.DllUnregisterServer();
#ifdef _MERGE_PROXYSTUB
    if (FAILED(hr))
        return hr;
    hr = PrxDllRegisterServer();
    if (FAILED(hr))
        return hr;
    hr = PrxDllUnregisterServer();
#endif
	return hr;
}
//-------------------------------------------------------------------------------------------------