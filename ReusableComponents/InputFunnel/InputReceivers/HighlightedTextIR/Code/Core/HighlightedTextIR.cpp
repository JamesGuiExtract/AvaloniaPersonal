//============================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	HighlightedTextIR.cpp
//
// PURPOSE:	Implementation of DLL Exports
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//============================================================================

// Note: Proxy/Stub Information
//      To build a separate proxy/stub DLL, 
//      run nmake -f HighlightedTextIRps.mk in the project directory.

#include "stdafx.h"
#include "resource.h"
#include <initguid.h>
#include "HighlightedTextIR.h"

#include "HighlightedTextIR_i.c"
#include "HighlightedTextWindow.h"
#include "HTIRUtils.h"

#include "..\..\..\..\IFCore\Code\IFCategories.h"

#include <UCLIDException.h>
#include <comutils.h>

CComModule _Module;

BEGIN_OBJECT_MAP(ObjectMap)
OBJECT_ENTRY(CLSID_HighlightedTextWindow, CHighlightedTextWindow)
OBJECT_ENTRY(CLSID_HTIRUtils, CHTIRUtils)
END_OBJECT_MAP()

class CHighlightedTextIRApp : public CWinApp
{
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CHighlightedTextIRApp)
	public:
    virtual BOOL InitInstance();
    virtual int ExitInstance();
	//}}AFX_VIRTUAL

	//{{AFX_MSG(CHighlightedTextIRApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

BEGIN_MESSAGE_MAP(CHighlightedTextIRApp, CWinApp)
	//{{AFX_MSG_MAP(CHighlightedTextIRApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

CHighlightedTextIRApp theApp;

BOOL CHighlightedTextIRApp::InitInstance()
{
    _Module.Init(ObjectMap, m_hInstance, &LIBID_UCLID_HIGHLIGHTEDTEXTIRLib);
	AfxEnableControlContainer();
    return CWinApp::InitInstance();
}

int CHighlightedTextIRApp::ExitInstance()
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
    // Registers object, typelib and all interfaces in typelib
    HRESULT hr = _Module.RegisterServer(TRUE);

	try
	{
		try
		{
			// Create the category if not already present
			createCOMCategory( CATID_INPUTFUNNEL_INPUT_RECEIVERS, 
				INPUTFUNNEL_IR_CATEGORYNAME );
		}
		CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02666")
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


