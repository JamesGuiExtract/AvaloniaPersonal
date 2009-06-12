// AFUtils.cpp : Implementation of DLL Exports.


// Note: Proxy/Stub Information
//      To build a separate proxy/stub DLL, 
//      run nmake -f AFUtilsps.mk in the project directory.

#include "stdafx.h"
#include "resource.h"
#include <initguid.h>
#include "AFUtils.h"

#include "AFUtils_i.c"

#include <ComUtils.h>
#include "MERSHandler.h"
#include "EntityFinder.h"
#include "DocumentClassifier.h"
#include "EntityKeywords.h"
#include "DocumentSorter.h"
#include "AFUtility.h"
#include "DocumentClassifierPP.h"

CComModule _Module;

BEGIN_OBJECT_MAP(ObjectMap)
OBJECT_ENTRY(CLSID_MERSHandler, CMERSHandler)
OBJECT_ENTRY(CLSID_DocumentClassifier, CDocumentClassifier)
OBJECT_ENTRY(CLSID_EntityFinder, CEntityFinder)
OBJECT_ENTRY(CLSID_EntityKeywords, CEntityKeywords)
OBJECT_ENTRY(CLSID_DocumentSorter, CDocumentSorter)
OBJECT_ENTRY(CLSID_AFUtility, CAFUtility)
OBJECT_ENTRY(CLSID_DocumentClassifierPP, CDocumentClassifierPP)
//OBJECT_ENTRY(CLSID_DocumentClassificationUtils, CDocumentClassificationUtils)
END_OBJECT_MAP()

class CAFUtilsApp : public CWinApp
{
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CAFUtilsApp)
	public:
    virtual BOOL InitInstance();
    virtual int ExitInstance();
	//}}AFX_VIRTUAL

	//{{AFX_MSG(CAFUtilsApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

BEGIN_MESSAGE_MAP(CAFUtilsApp, CWinApp)
	//{{AFX_MSG_MAP(CAFUtilsApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

CAFUtilsApp theApp;

BOOL CAFUtilsApp::InitInstance()
{
    _Module.Init(ObjectMap, m_hInstance, &LIBID_UCLID_AFUTILSLib);
    return CWinApp::InitInstance();
}

int CAFUtilsApp::ExitInstance()
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


