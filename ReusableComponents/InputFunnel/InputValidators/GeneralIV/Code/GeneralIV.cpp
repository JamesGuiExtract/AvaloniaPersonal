// GeneralIV.cpp : Implementation of DLL Exports.


// Note: Proxy/Stub Information
//      To build a separate proxy/stub DLL, 
//      run nmake -f GeneralIVps.mk in the project directory.

#include "stdafx.h"
#include "resource.h"
#include <initguid.h>
#include "GeneralIV.h"

#include <UCLIDException.h>
#include <comutils.h>
#include "..\..\..\IFCore\Code\IFCategories.h"

#include "GeneralIV_i.c"
#include "IntegerInputValidator.h"
#include "DoubleInputValidator.h"
#include "FloatInputValidator.h"
#include "ShortInputValidator.h"
#include "DateInputValidator.h"
#include "IntegerInputValidatorPP.h"
#include "DoubleInputValidatorPP.h"


CComModule _Module;

BEGIN_OBJECT_MAP(ObjectMap)
OBJECT_ENTRY(CLSID_IntegerInputValidator, CIntegerInputValidator)
OBJECT_ENTRY(CLSID_DoubleInputValidator, CDoubleInputValidator)
OBJECT_ENTRY(CLSID_FloatInputValidator, CFloatInputValidator)
OBJECT_ENTRY(CLSID_ShortInputValidator, CShortInputValidator)
OBJECT_ENTRY(CLSID_DateInputValidator, CDateInputValidator)
OBJECT_ENTRY(CLSID_IntegerInputValidatorPP, CIntegerInputValidatorPP)
OBJECT_ENTRY(CLSID_DoubleInputValidatorPP, CDoubleInputValidatorPP)
END_OBJECT_MAP()

class CGeneralIVApp : public CWinApp
{
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CGeneralIVApp)
	public:
    virtual BOOL InitInstance();
    virtual int ExitInstance();
	//}}AFX_VIRTUAL

	//{{AFX_MSG(CGeneralIVApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

BEGIN_MESSAGE_MAP(CGeneralIVApp, CWinApp)
	//{{AFX_MSG_MAP(CGeneralIVApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

CGeneralIVApp theApp;

BOOL CGeneralIVApp::InitInstance()
{
    _Module.Init(ObjectMap, m_hInstance, &LIBID_UCLID_GENERALIVLib);
    return CWinApp::InitInstance();
}

int CGeneralIVApp::ExitInstance()
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
		try
		{
			createCOMCategory(CATID_INPUTFUNNEL_INPUT_VALIDATORS, 
				INPUTFUNNEL_IV_CATEGORYNAME);
			registerCOMComponentInCategory(CLSID_IntegerInputValidator,
				CATID_INPUTFUNNEL_INPUT_VALIDATORS);
			registerCOMComponentInCategory(CLSID_ShortInputValidator,
				CATID_INPUTFUNNEL_INPUT_VALIDATORS);
			registerCOMComponentInCategory(CLSID_DoubleInputValidator,
				CATID_INPUTFUNNEL_INPUT_VALIDATORS);
			registerCOMComponentInCategory(CLSID_FloatInputValidator,
				CATID_INPUTFUNNEL_INPUT_VALIDATORS);
			registerCOMComponentInCategory(CLSID_DateInputValidator,
				CATID_INPUTFUNNEL_INPUT_VALIDATORS);
		}
		CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03828");
	}
	catch (...)
	{
		// We should never reach here - nested try/catch blocks to avoid warning C4297
	}

	return hr;
}

/////////////////////////////////////////////////////////////////////////////
// DllUnregisterServer - Removes entries from the system registry

STDAPI DllUnregisterServer(void)
{
    return _Module.UnregisterServer(TRUE);
}


