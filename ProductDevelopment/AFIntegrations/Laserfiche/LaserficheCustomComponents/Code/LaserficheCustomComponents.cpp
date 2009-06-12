// LaserficheCustomComponents.cpp : Implementation of DLL Exports.


#include "stdafx.h"
#include "resource.h"
#include "LaserficheCustomComponents.h"
#include <RWUtils.h>

class CLaserficheCustomComponentsModule : public CAtlDllModuleT< CLaserficheCustomComponentsModule >
{
public :
	DECLARE_LIBID(LIBID_UCLID_LASERFICHECCLib)
	DECLARE_REGISTRY_APPID_RESOURCEID(IDR_LASERFICHECUSTOMCOMPONENTS, "{55A7C69C-F814-490A-8B8C-E8E157967B21}")
};

CLaserficheCustomComponentsModule _AtlModule;

class CLaserficheCustomComponentsApp : public CWinApp
{
public:

// Overrides
    virtual BOOL InitInstance();
    virtual int ExitInstance();

    DECLARE_MESSAGE_MAP()
};

BEGIN_MESSAGE_MAP(CLaserficheCustomComponentsApp, CWinApp)
END_MESSAGE_MAP()

CLaserficheCustomComponentsApp theApp;

BOOL CLaserficheCustomComponentsApp::InitInstance()
{
	// Initialize the Rogue Wave Utils library
	RWInitializer	rwInit;

    return CWinApp::InitInstance();
}

int CLaserficheCustomComponentsApp::ExitInstance()
{
	// Cleanup the Rogue Wave Utils library
	RWCleanup	rwClean;

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

