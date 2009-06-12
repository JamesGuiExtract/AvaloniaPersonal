// ESFileSuppliers.cpp : Implementation of DLL Exports.


#include "stdafx.h"
#include "resource.h"

#include "ESFileSuppliers.h"
#include "ESFileSuppliers_i.c"
#include "StaticFileListFS.h"
#include "StaticFileListFSPP.h"
#include "FolderFS.h"
#include "FolderFSPP.h"
#include "DynamicFileListFS.h"
#include "DynamicFileListFSPP.h"

CComModule _Module;

BEGIN_OBJECT_MAP(ObjectMap)
OBJECT_ENTRY(CLSID_StaticFileListFS, CStaticFileListFS)
OBJECT_ENTRY(CLSID_StaticFileListFSPP, CStaticFileListFSPP)
OBJECT_ENTRY(CLSID_FolderFS, CFolderFS)
OBJECT_ENTRY(CLSID_FolderFSPP, CFolderFSPP)
OBJECT_ENTRY(CLSID_DynamicFileListFS, CDynamicFileListFS)
OBJECT_ENTRY(CLSID_DynamicFileListFSPP, CDynamicFileListFSPP)
END_OBJECT_MAP()



/*class CESFileSuppliersModule : public CAtlDllModuleT< CESFileSuppliersModule >
{
public :
	DECLARE_LIBID(LIBID_EXTRACT_FILESUPPLIERSLib)
	DECLARE_REGISTRY_APPID_RESOURCEID(IDR_ESFILESUPPLIERS, "{44ABCF77-5919-4788-A0F4-A78EC9E01264}")
};
*/

//CESFileSuppliersModule _AtlModule;

class CESFileSuppliersApp : public CWinApp
{
public:

// Overrides
    virtual BOOL InitInstance();
    virtual int ExitInstance();

    DECLARE_MESSAGE_MAP()
};

BEGIN_MESSAGE_MAP(CESFileSuppliersApp, CWinApp)
END_MESSAGE_MAP()

CESFileSuppliersApp theApp;

BOOL CESFileSuppliersApp::InitInstance()
{
    return CWinApp::InitInstance();
}

int CESFileSuppliersApp::ExitInstance()
{
    return CWinApp::ExitInstance();
}


// Used to determine whether the DLL can be unloaded by OLE
STDAPI DllCanUnloadNow(void)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());
    return (AfxDllCanUnloadNow()==S_OK && _Module.GetLockCount()==0) ? S_OK : S_FALSE;
}


// Returns a class factory to create an object of the requested type
STDAPI DllGetClassObject(REFCLSID rclsid, REFIID riid, LPVOID* ppv)
{
    return _Module.DllGetClassObject(rclsid, riid, ppv);
}


// DllRegisterServer - Adds entries to the system registry
STDAPI DllRegisterServer(void)
{
    // registers object, typelib and all interfaces in typelib
    HRESULT hr = _Module.DllRegisterServer();
	return hr;
}


// DllUnregisterServer - Removes entries from the system registry
STDAPI DllUnregisterServer(void)
{
	HRESULT hr = _Module.DllUnregisterServer();
	return hr;
}