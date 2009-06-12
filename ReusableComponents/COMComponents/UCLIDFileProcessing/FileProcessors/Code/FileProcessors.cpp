// FileProcessors.cpp : Implementation of DLL Exports.


// Note: Proxy/Stub Information
//      To build a separate proxy/stub DLL, 
//      run nmake -f FileProcessorsps.mk in the project directory.

#include "stdafx.h"
#include "resource.h"
#include <initguid.h>
#include "FileProcessors.h"

#include "FileProcessors_i.c"
#include "OCRFileProcessor.h"
#include "OCRFileProcessorPP.h"
#include "CopyMoveDeleteFileProcessor.h"
#include "CopyMoveDeleteFileProcessorPP.h"
#include "LaunchAppFileProcessor.h"
#include "LaunchAppFileProcessorPP.h"
#include "CleanupImageFileProcessor.h"
#include "CleanupImageFileProcessorPP.h"

CComModule _Module;

BEGIN_OBJECT_MAP(ObjectMap)
OBJECT_ENTRY(CLSID_OCRFileProcessor, COCRFileProcessor)
OBJECT_ENTRY(CLSID_OCRFileProcessorPP, COCRFileProcessorPP)
OBJECT_ENTRY(CLSID_CopyMoveDeleteFileProcessor, CCopyMoveDeleteFileProcessor)
OBJECT_ENTRY(CLSID_CopyMoveDeleteFileProcessorPP, CCopyMoveDeleteFileProcessorPP)
OBJECT_ENTRY(CLSID_LaunchAppFileProcessor, CLaunchAppFileProcessor)
OBJECT_ENTRY(CLSID_LaunchAppFileProcessorPP, CLaunchAppFileProcessorPP)
OBJECT_ENTRY(CLSID_CleanupImageFileProcessor, CCleanupImageFileProcessor)
OBJECT_ENTRY(CLSID_CleanupImageFileProcessorPP, CCleanupImageFileProcessorPP)
END_OBJECT_MAP()

class CFileProcessorsApp : public CWinApp
{
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CFileProcessorsApp)
	public:
    virtual BOOL InitInstance();
    virtual int ExitInstance();
	//}}AFX_VIRTUAL

	//{{AFX_MSG(CFileProcessorsApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

BEGIN_MESSAGE_MAP(CFileProcessorsApp, CWinApp)
	//{{AFX_MSG_MAP(CFileProcessorsApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

CFileProcessorsApp theApp;

BOOL CFileProcessorsApp::InitInstance()
{
    _Module.Init(ObjectMap, m_hInstance, &LIBID_UCLID_FILEPROCESSORSLib);
    return CWinApp::InitInstance();
}

int CFileProcessorsApp::ExitInstance()
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


