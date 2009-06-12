// ESSkipConditions.cpp : Implementation of DLL Exports.


#include "stdafx.h"
#include "resource.h"
#include <initguid.h>

#include "ESSkipConditions.h"
#include "ESSkipConditions_i.c"
#include "FileExistence.h"
#include "FileExistencePP.h"
#include "FileNamePattern.h"
#include "FileNamePatternPP.h"
#include "GenericMultiSkipCondition.h"
#include "MultiSkipConditionAND.h"
#include "MultiSkipConditionOR.h"
#include "MultiSkipConditionEXACTONE.h"
#include "MultiSkipConditionNONE.h"

CComModule _Module;

BEGIN_OBJECT_MAP(ObjectMap)
OBJECT_ENTRY(CLSID_FileExistence, CFileExistence)
OBJECT_ENTRY(CLSID_FileExistencePP, CFileExistencePP)
OBJECT_ENTRY(CLSID_FileNamePattern, CFileNamePattern)
OBJECT_ENTRY(CLSID_FileNamePatternPP, CFileNamePatternPP)
OBJECT_ENTRY(CLSID_GenericMultiFAMCondition, CGenericMultiFAMCondition)
OBJECT_ENTRY(CLSID_MultiFAMConditionAND, CMultiFAMConditionAND)
OBJECT_ENTRY(CLSID_MultiFAMConditionOR, CMultiFAMConditionOR)
OBJECT_ENTRY(CLSID_MultiFAMConditionEXACTONE, CMultiFAMConditionEXACTONE)
OBJECT_ENTRY(CLSID_MultiFAMConditionNONE, CMultiFAMConditionNONE)
END_OBJECT_MAP()

class CESFAMConditionsApp : public CWinApp
{
public:

// Overrides
    virtual BOOL InitInstance();
    virtual int ExitInstance();

    DECLARE_MESSAGE_MAP()
};

BEGIN_MESSAGE_MAP(CESFAMConditionsApp, CWinApp)
END_MESSAGE_MAP()

CESFAMConditionsApp theApp;

BOOL CESFAMConditionsApp::InitInstance()
{
	_Module.Init(ObjectMap, m_hInstance, &LIBID_EXTRACT_FAMCONDITIONSLib);
    return CWinApp::InitInstance();
}

int CESFAMConditionsApp::ExitInstance()
{
    _Module.Term();
    return CWinApp::ExitInstance();
}


// Used to determine whether the DLL can be unloaded by OLE
STDAPI DllCanUnloadNow(void)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());
    return (AfxDllCanUnloadNow()==S_OK && _Module.GetLockCount()==0) ? S_OK : S_FALSE;
}


// Returns a class factory to create an object of the requested type
STDAPI DllGetClassObject(REFCLSID rclsid, REFIID riid, LPVOID* ppv)
{
    return _Module.GetClassObject(rclsid, riid, ppv);
}


// DllRegisterServer - Adds entries to the system registry
STDAPI DllRegisterServer(void)
{
    // registers object, typelib and all interfaces in typelib
    return _Module.RegisterServer(TRUE);
}


// DllUnregisterServer - Removes entries from the system registry
STDAPI DllUnregisterServer(void)
{
	return _Module.UnregisterServer(TRUE);
}

