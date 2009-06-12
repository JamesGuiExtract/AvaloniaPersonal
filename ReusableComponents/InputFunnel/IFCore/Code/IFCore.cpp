// IFCore.cpp : Implementation of DLL Exports.


// Note: Proxy/Stub Information
//      To build a separate proxy/stub DLL, 
//      run nmake -f IFCoreps.mk in the project directory.

#include "stdafx.h"
#include "resource.h"
#include <initguid.h>
#include "IFCore.h"

#include "IFCore_i.c"

#include "InputEntity.h"
#include "TextInput.h"
#include "TextInputValidator.h"
#include "InputManager.h"
#include "IFCategories.h"
#include "OCRFilterMgr.h"

#include <UCLIDException.h>

#include <comutils.h>
#include "InputCorrectionUI.h"
#include "InputManagerSingleton.h"
#include "IRUIDisabler.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

CComModule _Module;

BEGIN_OBJECT_MAP(ObjectMap)
OBJECT_ENTRY(CLSID_InputEntity, CInputEntity)
OBJECT_ENTRY(CLSID_TextInput, CTextInput)
OBJECT_ENTRY(CLSID_TextInputValidator, CTextInputValidator)
OBJECT_ENTRY(CLSID_InputManager, CInputManager)
OBJECT_ENTRY(CLSID_InputCorrectionUI, CInputCorrectionUI)
OBJECT_ENTRY(CLSID_OCRFilterMgr, COCRFilterMgr)
OBJECT_ENTRY(CLSID_InputManagerSingleton, CInputManagerSingleton)
OBJECT_ENTRY(CLSID_IRUIDisabler, CIRUIDisabler)
END_OBJECT_MAP()

class CIFCoreApp : public CWinApp
{
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CIFCoreApp)
	public:
    virtual BOOL InitInstance();
    virtual int ExitInstance();
	//}}AFX_VIRTUAL

	//{{AFX_MSG(CIFCoreApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

BEGIN_MESSAGE_MAP(CIFCoreApp, CWinApp)
	//{{AFX_MSG_MAP(CIFCoreApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

CIFCoreApp theApp;

BOOL CIFCoreApp::InitInstance()
{
    _Module.Init(ObjectMap, m_hInstance, &LIBID_UCLID_INPUTFUNNELLib);
	
	return CWinApp::InitInstance();
}

int CIFCoreApp::ExitInstance()
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
    // register object, typelib and all interfaces in typelib
    HRESULT hr = _Module.RegisterServer(TRUE);

	try
	{
		createCOMCategory(CATID_INPUTFUNNEL_INPUT_RECEIVERS, 
			INPUTFUNNEL_IR_CATEGORYNAME);
		createCOMCategory(CATID_INPUTFUNNEL_INPUT_VALIDATORS, 
			INPUTFUNNEL_IV_CATEGORYNAME);
		registerCOMComponentInCategory(CLSID_TextInputValidator,
			CATID_INPUTFUNNEL_INPUT_VALIDATORS);
	}
	CATCH_UCLID_EXCEPTION("ELI02234")
	CATCH_UNEXPECTED_EXCEPTION("ELI02235")
    
	// registers object, typelib and all interfaces in typelib
    return hr;
}

/////////////////////////////////////////////////////////////////////////////
// DllUnregisterServer - Removes entries from the system registry

STDAPI DllUnregisterServer(void)
{
    return _Module.UnregisterServer(TRUE);
}
