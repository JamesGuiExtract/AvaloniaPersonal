// ArcGISUtils.cpp : Implementation of DLL Exports.


// Note: Proxy/Stub Information
//      To build a separate proxy/stub DLL, 
//      run nmake -f ArcGISUtilsps.mk in the project directory.

#include "stdafx.h"
#include "resource.h"
#include <initguid.h>
#include "ArcGISUtils.h"

#include "ArcGISUtils_i.c"
#include "ArcMapToolbar.h"
#include "HTIRCommand.h"
#include "SRIRCommand.h"

#include "UCLIDArcMapToolbarCATID.h"
#include <COMUtils.h>
#include <UCLIDException.h>
#include "ArcGISDisplayAdapter.h"
#include "ArcGISAttributeManager.h"

CComModule _Module;

BEGIN_OBJECT_MAP(ObjectMap)
OBJECT_ENTRY(CLSID_ArcMapToolbar, CArcMapToolbar)
OBJECT_ENTRY(CLSID_HTIRCommand, CHTIRCommand)
OBJECT_ENTRY(CLSID_SRIRCommand, CSRIRCommand)
OBJECT_ENTRY(CLSID_ArcGISDisplayAdapter, CArcGISDisplayAdapter)
OBJECT_ENTRY(CLSID_ArcGISAttributeManager, CArcGISAttributeManager)
END_OBJECT_MAP()

class CArcGISUtilsApp : public CWinApp
{
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CArcGISUtilsApp)
	public:
    virtual BOOL InitInstance();
    virtual int ExitInstance();
	//}}AFX_VIRTUAL

	//{{AFX_MSG(CArcGISUtilsApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

BEGIN_MESSAGE_MAP(CArcGISUtilsApp, CWinApp)
	//{{AFX_MSG_MAP(CArcGISUtilsApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

CArcGISUtilsApp theApp;

BOOL CArcGISUtilsApp::InitInstance()
{
    _Module.Init(ObjectMap, m_hInstance, &LIBID_UCLID_ARCGISUTILSLib);
    return CWinApp::InitInstance();
}

int CArcGISUtilsApp::ExitInstance()
{
    _Module.Term();
    return CWinApp::ExitInstance();
}

/////////////////////////////////////////////////////////////////////////////
// Used to determine whether the DLL can be unloaded by OLE

STDAPI DllCanUnloadNow(void)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());
    return (AfxDllCanUnloadNow()==S_OK && _Module.GetLockCount()==0) ? S_OK : S_FALSE;
}

/////////////////////////////////////////////////////////////////////////////
// Returns a class factory to create an object of the requested type

STDAPI DllGetClassObject(REFCLSID rclsid, REFIID riid, LPVOID* ppv)
{
    return _Module.GetClassObject(rclsid, riid, ppv);
}

/////////////////////////////////////////////////////////////////////////////
// DllRegisterServer - Adds entries to the system registry

STDAPI DllRegisterServer(void)
{
    // registers object, typelib and all interfaces in typelib
    HRESULT hr = _Module.RegisterServer(TRUE);
	try
	{
		// create the uclid toolbar category
		createCOMCategory(CATID_UCLIDArcMapToolbar, UCLID_ARCMAP_TOOLBAR);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04105")
    
	// registers object, typelib and all interfaces in typelib
    return hr;
}

/////////////////////////////////////////////////////////////////////////////
// DllUnregisterServer - Removes entries from the system registry

STDAPI DllUnregisterServer(void)
{
    return _Module.UnregisterServer(TRUE);
}


