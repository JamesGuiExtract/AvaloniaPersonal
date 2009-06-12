// ESPrintCaptureCore.cpp : Implementation of DLL Exports.


#include "stdafx.h"
#include "resource.h"
#include "ESPrintCaptureCore.h"

//--------------------------------------------------------------------------------------------------
class CESPrintCaptureCoreModule : public CAtlDllModuleT< CESPrintCaptureCoreModule >
{
public :
	DECLARE_LIBID(LIBID_ESPrintCaptureCoreLib)
	DECLARE_REGISTRY_APPID_RESOURCEID(IDR_ESPRINTCAPTURECORE, 
	"{3C923AB5-D61D-48E3-AC58-532D5DED02C2}")
};

CESPrintCaptureCoreModule _AtlModule;

//--------------------------------------------------------------------------------------------------
class CESPrintCaptureCoreApp : public CWinApp
{
public:

// Overrides
    virtual BOOL InitInstance();
    virtual int ExitInstance();

    DECLARE_MESSAGE_MAP()
};

//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CESPrintCaptureCoreApp, CWinApp)
END_MESSAGE_MAP()
//--------------------------------------------------------------------------------------------------

CESPrintCaptureCoreApp theApp;

//--------------------------------------------------------------------------------------------------
BOOL CESPrintCaptureCoreApp::InitInstance()
{
    return CWinApp::InitInstance();
}
//--------------------------------------------------------------------------------------------------
int CESPrintCaptureCoreApp::ExitInstance()
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
