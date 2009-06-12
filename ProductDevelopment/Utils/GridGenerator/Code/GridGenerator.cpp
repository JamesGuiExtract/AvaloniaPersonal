// GridGenerator.cpp : Implementation of DLL Exports.


// Note: Proxy/Stub Information
//      To build a separate proxy/stub DLL, 
//      run nmake -f GridGeneratorps.mk in the project directory.

#include "stdafx.h"
#include "resource.h"
#include <initguid.h>
#include "GridGenerator.h"

#include "GridGenerator_i.c"
#include "DrawGrid.h"
#include "EventSink.h"
#include "QueryFeature.h"


CComModule _Module;

// This is declared here to allow the TimeRollbackPreventor.cpp and LicenseMgmt.cpp files to be complied as part of
// the GridGenerator.dll
//AFX_EXTENSION_MODULE COMLMCoreDLL;

BEGIN_OBJECT_MAP(ObjectMap)
OBJECT_ENTRY(CLSID_DrawGrid, CDrawGrid)
OBJECT_ENTRY(CLSID_EventSink, CEventSink)
OBJECT_ENTRY(CLSID_QueryFeature, CQueryFeature)
END_OBJECT_MAP()

class CGridGeneratorApp : public CWinApp
{
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CGridGeneratorApp)
	public:
    virtual BOOL InitInstance();
    virtual int ExitInstance();
	//}}AFX_VIRTUAL

	//{{AFX_MSG(CGridGeneratorApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

BEGIN_MESSAGE_MAP(CGridGeneratorApp, CWinApp)
	//{{AFX_MSG_MAP(CGridGeneratorApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

CGridGeneratorApp theApp;

BOOL CGridGeneratorApp::InitInstance()
{
    _Module.Init(ObjectMap, m_hInstance, &LIBID_UCLID_GRIDGENERATORLib);

	// This is very bad
	// This is done so that TimeRollbackpreventor.cpp and LicenseMgmt.cpp (which use the
	// variable COMLMCoreDLL.hModule as a way to get the folder for the license files) can be 
	// complied as part of the GridGenerator.dll
	//COMLMCoreDLL.hModule = m_hInstance;
    return CWinApp::InitInstance();
}

int CGridGeneratorApp::ExitInstance()
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
    return _Module.RegisterServer(TRUE);
}

/////////////////////////////////////////////////////////////////////////////
// DllUnregisterServer - Removes entries from the system registry

STDAPI DllUnregisterServer(void)
{
    return _Module.UnregisterServer(TRUE);
}


