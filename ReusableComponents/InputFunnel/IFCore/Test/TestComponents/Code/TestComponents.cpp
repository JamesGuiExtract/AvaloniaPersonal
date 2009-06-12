// TestComponents.cpp : Implementation of DLL Exports.


// Note: Proxy/Stub Information
//      To build a separate proxy/stub DLL, 
//      run nmake -f TestComponentsps.mk in the project directory.

#include "stdafx.h"
#include "resource.h"
#include <initguid.h>
#include "TestComponents.h"

#include "TestComponents_i.c"
#include "NumberInputReceiver.h"

#include <UCLIDException.hpp>
#include <comutils.h>

#include "..\..\..\Code\IFCategories.h"
#include "NumberInputValidator.h"

CComModule _Module;

BEGIN_OBJECT_MAP(ObjectMap)
OBJECT_ENTRY(CLSID_NumberInputReceiver, CNumberInputReceiver)
OBJECT_ENTRY(CLSID_NumberInputValidator, CNumberInputValidator)
END_OBJECT_MAP()

HINSTANCE g_Resource = NULL;

class CTestComponentsApp : public CWinApp
{
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CTestComponentsApp)
	public:
    virtual BOOL InitInstance();
    virtual int ExitInstance();
	//}}AFX_VIRTUAL

	//{{AFX_MSG(CTestComponentsApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

BEGIN_MESSAGE_MAP(CTestComponentsApp, CWinApp)
	//{{AFX_MSG_MAP(CTestComponentsApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

CTestComponentsApp theApp;

BOOL CTestComponentsApp::InitInstance()
{
    _Module.Init(ObjectMap, m_hInstance, &LIBID_TESTCOMPONENTSLib);
	g_Resource = m_hInstance;

    return CWinApp::InitInstance();
}

int CTestComponentsApp::ExitInstance()
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
		// register this number input receiver under input funnel input
		// receiver category in Registry
		registerCOMComponentInCategory(CLSID_NumberInputReceiver,
			CATID_INPUTFUNNEL_INPUT_RECEIVERS);
		// register this number input validator under input funnel input
		// validator category in Registry
		registerCOMComponentInCategory(CLSID_NumberInputValidator,
			CATID_INPUTFUNNEL_INPUT_VALIDATORS);
	}
	CATCH_UCLID_EXCEPTION("ELI02290")
	CATCH_UNEXPECTED_EXCEPTION("ELI02291")
    
	// registers object, typelib and all interfaces in typelib
    return hr;
}

/////////////////////////////////////////////////////////////////////////////
// DllUnregisterServer - Removes entries from the system registry

STDAPI DllUnregisterServer(void)
{
    return _Module.UnregisterServer(TRUE);
}


