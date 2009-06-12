// AFSplitters.cpp : Implementation of DLL Exports.


// Note: Proxy/Stub Information
//      To build a separate proxy/stub DLL, 
//      run nmake -f AFSplittersps.mk in the project directory.

#include "stdafx.h"
#include "resource.h"
#include <initguid.h>
#include "AFSplitters.h"

#include "AFSplitters_i.c"
#include "StringTokenizerSplitter.h"
#include "StringTokenizerSplitterPP.h"
#include "EntityNameSplitter.h"
#include "EntityNameSplitterPP.h"
#include "PersonNameSplitter.h"
#include "RSDSplitter.h"
#include "RSDSplitterPP.h"
#include "AddressSplitter.h"
#include "LegalDescSplitter.h"
#include "AddressSplitterPP.h"
#include "DateTimeSplitter.h"
#include "DateTimeSplitterPP.h"


CComModule _Module;

BEGIN_OBJECT_MAP(ObjectMap)
OBJECT_ENTRY(CLSID_StringTokenizerSplitter, CStringTokenizerSplitter)
OBJECT_ENTRY(CLSID_StringTokenizerSplitterPP, CStringTokenizerSplitterPP)
OBJECT_ENTRY(CLSID_EntityNameSplitter, CEntityNameSplitter)
OBJECT_ENTRY(CLSID_EntityNameSplitterPP, CEntityNameSplitterPP)
OBJECT_ENTRY(CLSID_PersonNameSplitter, CPersonNameSplitter)
OBJECT_ENTRY(CLSID_RSDSplitter, CRSDSplitter)
OBJECT_ENTRY(CLSID_RSDSplitterPP, CRSDSplitterPP)
OBJECT_ENTRY(CLSID_AddressSplitter, CAddressSplitter)
OBJECT_ENTRY(CLSID_LegalDescSplitter, CLegalDescSplitter)
OBJECT_ENTRY(CLSID_AddressSplitterPP, CAddressSplitterPP)
OBJECT_ENTRY(CLSID_DateTimeSplitter, CDateTimeSplitter)
OBJECT_ENTRY(CLSID_DateTimeSplitterPP, CDateTimeSplitterPP)
END_OBJECT_MAP()

class CAFSplittersApp : public CWinApp
{
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CAFSplittersApp)
	public:
    virtual BOOL InitInstance();
    virtual int ExitInstance();
	//}}AFX_VIRTUAL

	//{{AFX_MSG(CAFSplittersApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

BEGIN_MESSAGE_MAP(CAFSplittersApp, CWinApp)
	//{{AFX_MSG_MAP(CAFSplittersApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

CAFSplittersApp theApp;

BOOL CAFSplittersApp::InitInstance()
{
    _Module.Init(ObjectMap, m_hInstance, &LIBID_UCLID_AFSPLITTERSLib);
    return CWinApp::InitInstance();
}

int CAFSplittersApp::ExitInstance()
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


