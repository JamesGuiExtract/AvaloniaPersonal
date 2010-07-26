// UCLIDFileProcessing.cpp : Implementation of DLL Exports.


// Note: Proxy/Stub Information
//      To build a separate proxy/stub DLL, 
//      run nmake -f UCLIDFileProcessingps.mk in the project directory.

#include "stdafx.h"
#include "resource.h"
#include <initguid.h>
#include "UCLIDFileProcessing.h"

#include "UCLIDFileProcessing_i.c"
#include "FileProcessingManager.h"
#include "FileSupplierData.h"
#include "FPCategories.h"
#include "FAMTagManager.h"
#include "FileProcessingTaskExecutor.h"

#include <ComUtils.h>

CComModule _Module;

BEGIN_OBJECT_MAP(ObjectMap)
OBJECT_ENTRY(CLSID_FileProcessingManager, CFileProcessingManager)
OBJECT_ENTRY(CLSID_FileSupplierData, CFileSupplierData)
OBJECT_ENTRY(CLSID_FAMTagManager, CFAMTagManager)
OBJECT_ENTRY(CLSID_FileProcessingTaskExecutor, CFileProcessingTaskExecutor)
END_OBJECT_MAP()

class CUCLIDFileProcessingApp : public CWinApp
{
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CUCLIDFileProcessingApp)
	public:
    virtual BOOL InitInstance();
    virtual int ExitInstance();
	//}}AFX_VIRTUAL

	//{{AFX_MSG(CUCLIDFileProcessingApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

BEGIN_MESSAGE_MAP(CUCLIDFileProcessingApp, CWinApp)
	//{{AFX_MSG_MAP(CUCLIDFileProcessingApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

CUCLIDFileProcessingApp theApp;

BOOL CUCLIDFileProcessingApp::InitInstance()
{
    _Module.Init(ObjectMap, m_hInstance, &LIBID_UCLID_FILEPROCESSINGLib);

    return CWinApp::InitInstance();
}

int CUCLIDFileProcessingApp::ExitInstance()
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
	createCOMCategory(CATID_FP_FILE_PROCESSORS, FP_FILE_PROC_CATEGORYNAME);
	createCOMCategory(CATID_FP_FILE_SUPPLIERS, FP_FILE_SUPP_CATEGORYNAME);
	createCOMCategory(CATID_FP_FAM_CONDITIONS, FP_FAM_CONDITIONS_CATEGORYNAME);
	createCOMCategory(CATID_FP_FAM_REPORTS, FP_FAM_REPORTS_CATEGORYNAME);
	createCOMCategory(CATID_FP_FAM_PRODUCT_SPECIFIC_DB_MGRS, FP_FAM_PRODUCT_SPECIFIC_DB_MGRS);
    // registers object, typelib and all interfaces in typelib
    return _Module.RegisterServer(TRUE);
}

/////////////////////////////////////////////////////////////////////////////
// DllUnregisterServer - Removes entries from the system registry

STDAPI DllUnregisterServer(void)
{
    return _Module.UnregisterServer(TRUE);
}


