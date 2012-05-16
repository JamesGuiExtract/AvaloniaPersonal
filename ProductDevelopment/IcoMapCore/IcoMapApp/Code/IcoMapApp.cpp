// IcoMapApp.cpp : Implementation of DLL Exports.


// Note: Proxy/Stub Information
//      To merge the proxy/stub code into the object DLL, add the file 
//      dlldatax.c to the project.  Make sure precompiled headers 
//      are turned off for this file, and add _MERGE_PROXYSTUB to the 
//      defines for the project.  
//
//      If you are not running WinNT4.0 or Win95 with DCOM, then you
//      need to remove the following define from dlldatax.c
//      #define _WIN32_WINNT 0x0400
//
//      Further, if you are running MIDL without /Oicf switch, you also 
//      need to remove the following define from dlldatax.c.
//      #define USE_STUBLESS_PROXY
//
//      Modify the custom build rule for IcoMapApp.idl by adding the following 
//      files to the Outputs.
//          IcoMapApp_p.c
//          dlldata.c
//      To build a separate proxy/stub DLL, 
//      run nmake -f IcoMapAppps.mk in the project directory.

#include "stdafx.h"
#include "resource.h"
#include <initguid.h>
#include "IcoMapApp.h"

#include "IcoMapApp_i.c"
#include "IcoMapCtl.h"
#include "IcoMapInputContext.h"
#include "IcoMapCommandRecognizer.h"

#include "..\..\..\InputFunnel\IFCore\Code\IFCategories.h"

#include <UCLIDException.h>
#include <comutils.h>
#include <RWUtils.h>
#include "NothingInputValidator.h"

HINSTANCE gModuleResource = NULL;
CComModule _Module;

BEGIN_OBJECT_MAP(ObjectMap)
OBJECT_ENTRY(CLSID_IcoMap, CIcoMapCtl)
OBJECT_ENTRY(CLSID_IcoMapInputContext, CIcoMapInputContext)
OBJECT_ENTRY(CLSID_IcoMapCommandRecognizer, CIcoMapCommandRecognizer)
OBJECT_ENTRY(CLSID_NothingInputValidator, CNothingInputValidator)
END_OBJECT_MAP()

class CIcoMapAppApp : public CWinApp
{
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CIcoMapAppApp)
	public:
    virtual BOOL InitInstance();
    virtual int ExitInstance();
	//}}AFX_VIRTUAL

	//{{AFX_MSG(CIcoMapAppApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

BEGIN_MESSAGE_MAP(CIcoMapAppApp, CWinApp)
	//{{AFX_MSG_MAP(CIcoMapAppApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

CIcoMapAppApp theApp;

BOOL CIcoMapAppApp::InitInstance()
{
    _Module.Init(ObjectMap, m_hInstance, &LIBID_ICOMAPAPPLib);
	gModuleResource = m_hInstance;
	AfxEnableControlContainer();

	try
	{
		// Initialize the Rogue Wave Utils library
		RWInitializer rwInit;
	}
	catch (...)
	{
	}

    return CWinApp::InitInstance();
}

int CIcoMapAppApp::ExitInstance()
{
    _Module.Term();

	try
	{
		// Cleanup the Rogue Wave Utils library
		RWCleanup	rwClean;
	}
	catch(...)
	{
	}

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
		// create input receivers category
		createCOMCategory(CATID_INPUTFUNNEL_INPUT_RECEIVERS, 
			INPUTFUNNEL_IR_CATEGORYNAME);
		// register input receivers under uclid input receiver category
		registerCOMComponentInCategory(CLSID_IcoMap,
			CATID_INPUTFUNNEL_INPUT_RECEIVERS);
	}
	CATCH_UCLID_EXCEPTION("ELI02785")
	CATCH_COM_EXCEPTION("ELI02786")
	CATCH_UNEXPECTED_EXCEPTION("ELI02787")

    return hr;
}

/////////////////////////////////////////////////////////////////////////////
// DllUnregisterServer - Removes entries from the system registry

STDAPI DllUnregisterServer(void)
{
    return _Module.UnregisterServer(TRUE);
}
