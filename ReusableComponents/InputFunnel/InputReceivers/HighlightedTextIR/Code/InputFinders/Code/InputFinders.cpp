// InputFinders.cpp : Implementation of DLL Exports.


// Note: Proxy/Stub Information
//      To build a separate proxy/stub DLL, 
//      run nmake -f InputFindersps.mk in the project directory.

#include "stdafx.h"
#include "resource.h"
#include <initguid.h>
#include "InputFinders.h"

#include "InputFinders_i.c"
#include "WordInputFinder.h"
#include "MCRTextInputFinder.h"

#include "..\..\Core\HTCategories.h"
#include <UCLIDException.h>
#include <comutils.h>
#include "RegExprFinder.h"
#include "NothingInputFinder.h"

CComModule _Module;

BEGIN_OBJECT_MAP(ObjectMap)
OBJECT_ENTRY(CLSID_WordInputFinder, CWordInputFinder)
OBJECT_ENTRY(CLSID_MCRTextInputFinder, CMCRTextInputFinder)
OBJECT_ENTRY(CLSID_RegExprFinder, CRegExprFinder)
OBJECT_ENTRY(CLSID_NothingInputFinder, CNothingInputFinder)
END_OBJECT_MAP()

class CInputFindersApp : public CWinApp
{
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CInputFindersApp)
	public:
    virtual BOOL InitInstance();
    virtual int ExitInstance();
	//}}AFX_VIRTUAL

	//{{AFX_MSG(CInputFindersApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

BEGIN_MESSAGE_MAP(CInputFindersApp, CWinApp)
	//{{AFX_MSG_MAP(CInputFindersApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

CInputFindersApp theApp;

//-------------------------------------------------------------------------------------------------
BOOL CInputFindersApp::InitInstance()
{
    _Module.Init(ObjectMap, m_hInstance, &LIBID_UCLID_INPUTFINDERSLib);
    return CWinApp::InitInstance();
}
//-------------------------------------------------------------------------------------------------
int CInputFindersApp::ExitInstance()
{
    _Module.Term();
    return CWinApp::ExitInstance();
}

//-------------------------------------------------------------------------------------------------
// Used to determine whether the DLL can be unloaded by OLE
STDAPI DllCanUnloadNow(void)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());
    return (AfxDllCanUnloadNow()==S_OK && _Module.GetLockCount()==0) ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
// Returns a class factory to create an object of the requested type
STDAPI DllGetClassObject(REFCLSID rclsid, REFIID riid, LPVOID* ppv)
{
    return _Module.GetClassObject(rclsid, riid, ppv);
}
//-------------------------------------------------------------------------------------------------
// DllRegisterServer - Adds entries to the system registry
STDAPI DllRegisterServer(void)
{
    // registers object, typelib and all interfaces in typelib
    HRESULT hr = _Module.RegisterServer(TRUE);

	try
	{
		// Create the category if not already present
		createCOMCategory(CATID_HT_INPUT_FINDERS, 
			HT_IF_CATEGORYNAME);

		// register input finders under UCLID Input Finders category
		registerCOMComponentInCategory(CLSID_MCRTextInputFinder,
			CATID_HT_INPUT_FINDERS);
		registerCOMComponentInCategory(CLSID_WordInputFinder,
			CATID_HT_INPUT_FINDERS);
		registerCOMComponentInCategory(CLSID_NothingInputFinder,
			CATID_HT_INPUT_FINDERS);
	}
	CATCH_UCLID_EXCEPTION("ELI02741")
	CATCH_UNEXPECTED_EXCEPTION("ELI02742")
    
	// registers object, typelib and all interfaces in typelib
    return hr;
}
//-------------------------------------------------------------------------------------------------
// DllUnregisterServer - Removes entries from the system registry
STDAPI DllUnregisterServer(void)
{
    return _Module.UnregisterServer(TRUE);
}
//-------------------------------------------------------------------------------------------------
