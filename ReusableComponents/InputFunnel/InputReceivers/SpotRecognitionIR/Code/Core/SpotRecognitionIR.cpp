// SpotRecognitionIR.cpp : Implementation of DLL Exports.


// Note: Proxy/Stub Information
//      To build a separate proxy/stub DLL, 
//      run nmake -f SpotRecognitionIRps.mk in the project directory.

#include "stdafx.h"
#include "resource.h"
#include <initguid.h>
#include "SpotRecognitionIR.h"

#include "SpotRecognitionIR_i.c"
#include "SpotRecognitionWindow.h"

#include <UCLIDException.h>
#include <comutils.h>

#include "..\..\..\..\IFCore\Code\IFCategories.h"
#include "SRIRUtils.h"

CComModule _Module;

BEGIN_OBJECT_MAP(ObjectMap)
OBJECT_ENTRY(CLSID_SpotRecognitionWindow, CSpotRecognitionWindow)
OBJECT_ENTRY(CLSID_SRIRUtils, CSRIRUtils)
END_OBJECT_MAP()

class CSpotRecognitionIRApp : public CWinApp
{
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CSpotRecognitionIRApp)
	public:
    virtual BOOL InitInstance();
    virtual int ExitInstance();
	//}}AFX_VIRTUAL

	//{{AFX_MSG(CSpotRecognitionIRApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

BEGIN_MESSAGE_MAP(CSpotRecognitionIRApp, CWinApp)
	//{{AFX_MSG_MAP(CSpotRecognitionIRApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

CSpotRecognitionIRApp theApp;

BOOL CSpotRecognitionIRApp::InitInstance()
{
    _Module.Init(ObjectMap, m_hInstance, &LIBID_UCLID_SPOTRECOGNITIONIRLib);
	AfxEnableControlContainer();
    return CWinApp::InitInstance();
}

int CSpotRecognitionIRApp::ExitInstance()
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

	// Create the category if not already present
	createCOMCategory( CATID_INPUTFUNNEL_INPUT_RECEIVERS, 
		INPUTFUNNEL_IR_CATEGORYNAME );

	// register the input receiver under the correct
	// UCLID InputReceiver category
	registerCOMComponentInCategory(CLSID_SpotRecognitionWindow,
		CATID_INPUTFUNNEL_INPUT_RECEIVERS);

	return hr;
}

/////////////////////////////////////////////////////////////////////////////
// DllUnregisterServer - Removes entries from the system registry

STDAPI DllUnregisterServer(void)
{
    return _Module.UnregisterServer(TRUE);
}


