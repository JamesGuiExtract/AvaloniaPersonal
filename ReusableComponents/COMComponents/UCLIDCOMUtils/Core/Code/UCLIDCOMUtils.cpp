// UCLIDCOMUtils.cpp : Implementation of DLL Exports.


// Note: Proxy/Stub Information
//      To build a separate proxy/stub DLL, 
//      run nmake -f UCLIDCOMUtilsps.mk in the project directory.

#include "stdafx.h"
#include "resource.h"
#include "UCLIDCOMUtils.h"
#include "UCLIDCOMUtils_i.c"
#include "Token.h"
#include "VariantVector.h"
#include "IUnknownVector.h"
#include "ObjectPropertiesUI.h"
#include "ObjectWithDescription.h"
#include "ObjectSelectorUI.h"
#include "CategoryManager.h"
#include "StrToStrMap.h"
#include "StrToObjectMap.h"
#include "StringPair.h"
#include "ObjectPair.h"
#include "ClipboardObjectManager.h"
#include "StringPatternMatcher.h"
#include "LongRectangle.h"
#include "MiscUtils.h"
#include "MultipleObjSelectorPP.h"
#include "LongToObjectMap.h"
#include "LongPoint.h"
#include "LongToLongMap.h"
#include "DoublePoint.h"

#include <initguid.h>

CComModule _Module;

BEGIN_OBJECT_MAP(ObjectMap)
OBJECT_ENTRY(CLSID_Token, CToken)
OBJECT_ENTRY(CLSID_VariantVector, CVariantVector)
OBJECT_ENTRY(CLSID_IUnknownVector, CIUnknownVector)
OBJECT_ENTRY(CLSID_ObjectPropertiesUI, CObjectPropertiesUI)
OBJECT_ENTRY(CLSID_ObjectWithDescription, CObjectWithDescription)
OBJECT_ENTRY(CLSID_ObjectSelectorUI, CObjectSelectorUI)
OBJECT_ENTRY(CLSID_CategoryManager, CCategoryManager)
OBJECT_ENTRY(CLSID_StrToStrMap, CStrToStrMap)
OBJECT_ENTRY(CLSID_StrToObjectMap, CStrToObjectMap)
OBJECT_ENTRY(CLSID_StringPair, CStringPair)
OBJECT_ENTRY(CLSID_ObjectPair, CObjectPair)
OBJECT_ENTRY(CLSID_ClipboardObjectManager, CClipboardObjectManager)
OBJECT_ENTRY(CLSID_StringPatternMatcher, CStringPatternMatcher)
OBJECT_ENTRY(CLSID_LongRectangle, CLongRectangle)
OBJECT_ENTRY(CLSID_MiscUtils, CMiscUtils)
OBJECT_ENTRY(CLSID_MultipleObjSelectorPP, CMultipleObjSelectorPP)
OBJECT_ENTRY(CLSID_LongToObjectMap, CLongToObjectMap)
OBJECT_ENTRY(CLSID_LongPoint, CLongPoint)
OBJECT_ENTRY(CLSID_LongToLongMap, CLongToLongMap)
OBJECT_ENTRY(CLSID_DoublePoint, CDoublePoint)
END_OBJECT_MAP()

class CUCLIDCOMUtilsApp : public CWinApp
{
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CUCLIDCOMUtilsApp)
	public:
    virtual BOOL InitInstance();
    virtual int ExitInstance();
	//}}AFX_VIRTUAL

	//{{AFX_MSG(CUCLIDCOMUtilsApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

BEGIN_MESSAGE_MAP(CUCLIDCOMUtilsApp, CWinApp)
	//{{AFX_MSG_MAP(CUCLIDCOMUtilsApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

CUCLIDCOMUtilsApp theApp;

BOOL CUCLIDCOMUtilsApp::InitInstance()
{
	_Module.Init(ObjectMap, m_hInstance, &LIBID_UCLID_COMUTILSLib);
    return CWinApp::InitInstance();
}

int CUCLIDCOMUtilsApp::ExitInstance()
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


