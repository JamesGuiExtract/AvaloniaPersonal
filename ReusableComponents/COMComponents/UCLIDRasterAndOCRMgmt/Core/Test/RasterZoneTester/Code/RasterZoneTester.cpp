// RasterZoneTester.cpp : Implementation of DLL Exports.

#include "stdafx.h"
#include "resource.h"
#include "RasterZoneTester.h"
#include "RasterZoneTester_i.c"
#include "TestIntersection.h"

CComModule _Module;

//--------------------------------------------------------------------------------------------------
BEGIN_OBJECT_MAP(ObjectMap)
OBJECT_ENTRY(CLSID_TestIntersection, CTestIntersection)
END_OBJECT_MAP()
//--------------------------------------------------------------------------------------------------

class CRasterZoneTesterApp : public CWinApp
{
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CRasterZoneTesterApp)
	public:
    virtual BOOL InitInstance();
    virtual int ExitInstance();
	//}}AFX_VIRTUAL

	//{{AFX_MSG(CRasterZoneTesterApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CRasterZoneTesterApp, CWinApp)
	//{{AFX_MSG_MAP(CRasterZoneTesterApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
//--------------------------------------------------------------------------------------------------

CRasterZoneTesterApp theApp;
//--------------------------------------------------------------------------------------------------
BOOL CRasterZoneTesterApp::InitInstance()
{
    _Module.Init(ObjectMap, m_hInstance, &LIBID_UCLID_RASTERZONETESTERLib);
    return CWinApp::InitInstance();
}
//--------------------------------------------------------------------------------------------------
int CRasterZoneTesterApp::ExitInstance()
{
    _Module.Term();
    return CWinApp::ExitInstance();
}
//--------------------------------------------------------------------------------------------------
/////////////////////////////////////////////////////////////////////////////
// Used to determine whether the DLL can be unloaded by OLE
STDAPI DllCanUnloadNow(void)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());
    return (AfxDllCanUnloadNow()==S_OK && _Module.GetLockCount()==0) ? S_OK : S_FALSE;
}
//--------------------------------------------------------------------------------------------------
/////////////////////////////////////////////////////////////////////////////
// Returns a class factory to create an object of the requested type
STDAPI DllGetClassObject(REFCLSID rclsid, REFIID riid, LPVOID* ppv)
{
    return _Module.GetClassObject(rclsid, riid, ppv);
}
//--------------------------------------------------------------------------------------------------
/////////////////////////////////////////////////////////////////////////////
// DllRegisterServer - Adds entries to the system registry
STDAPI DllRegisterServer(void)
{
    // registers object, typelib and all interfaces in typelib
    return _Module.RegisterServer(TRUE);
}
//--------------------------------------------------------------------------------------------------
/////////////////////////////////////////////////////////////////////////////
// DllUnregisterServer - Removes entries from the system registry
STDAPI DllUnregisterServer(void)
{
    return _Module.UnregisterServer(TRUE);
}
//--------------------------------------------------------------------------------------------------