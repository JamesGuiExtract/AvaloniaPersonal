// LandRecordsIV.cpp : Implementation of DLL Exports.


// Note: Proxy/Stub Information
//      To build a separate proxy/stub DLL, 
//      run nmake -f LandRecordsIVps.mk in the project directory.

#include "stdafx.h"
#include "resource.h"
#include <initguid.h>
#include "LandRecordsIV.h"

#include "LandRecordsIV_i.c"
#include "Direction.h"
#include "Bearing.h"
#include "Angle.h"
#include "Distance.h"
#include "DirectionInputValidator.h"
#include "BearingInputValidator.h"
#include "AngleInputValidator.h"
#include "DistanceInputValidator.h"
#include "CartographicPointInputValidator.h"

#include "..\..\..\IFCore\Code\IFCategories.h"
#include <UCLIDException.h>
#include <comutils.h>


CComModule _Module;

BEGIN_OBJECT_MAP(ObjectMap)
OBJECT_ENTRY(CLSID_Bearing, CBearing)
OBJECT_ENTRY(CLSID_Angle, CAngle)
OBJECT_ENTRY(CLSID_Distance, CDistance)
OBJECT_ENTRY(CLSID_Direction, CDirection)
OBJECT_ENTRY(CLSID_BearingInputValidator, CBearingInputValidator)
OBJECT_ENTRY(CLSID_AngleInputValidator, CAngleInputValidator)
OBJECT_ENTRY(CLSID_DistanceInputValidator, CDistanceInputValidator)
OBJECT_ENTRY(CLSID_DirectionInputValidator, CDirectionInputValidator)
OBJECT_ENTRY(CLSID_CartographicPointInputValidator, CCartographicPointInputValidator)
END_OBJECT_MAP()

class CLandRecordsIVApp : public CWinApp
{
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CLandRecordsIVApp)
	public:
    virtual BOOL InitInstance();
    virtual int ExitInstance();
	//}}AFX_VIRTUAL

	//{{AFX_MSG(CLandRecordsIVApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

BEGIN_MESSAGE_MAP(CLandRecordsIVApp, CWinApp)
	//{{AFX_MSG_MAP(CLandRecordsIVApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

CLandRecordsIVApp theApp;

BOOL CLandRecordsIVApp::InitInstance()
{
    _Module.Init(ObjectMap, m_hInstance, &LIBID_UCLID_LANDRECORDSIVLib);
    return CWinApp::InitInstance();
}

int CLandRecordsIVApp::ExitInstance()
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
    HRESULT hr = _Module.RegisterServer(TRUE);

	try
	{
		createCOMCategory(CATID_INPUTFUNNEL_INPUT_VALIDATORS, 
			INPUTFUNNEL_IV_CATEGORYNAME);
		// register input validators under uclid input validator category
		registerCOMComponentInCategory(CLSID_AngleInputValidator,
			CATID_INPUTFUNNEL_INPUT_VALIDATORS);
		registerCOMComponentInCategory(CLSID_BearingInputValidator,
			CATID_INPUTFUNNEL_INPUT_VALIDATORS);
		registerCOMComponentInCategory(CLSID_DistanceInputValidator,
			CATID_INPUTFUNNEL_INPUT_VALIDATORS);
		registerCOMComponentInCategory(CLSID_DirectionInputValidator,
			CATID_INPUTFUNNEL_INPUT_VALIDATORS);
		// register input validators under uclid input validator category
		registerCOMComponentInCategory(CLSID_CartographicPointInputValidator,
			CATID_INPUTFUNNEL_INPUT_VALIDATORS);
	}
	CATCH_UCLID_EXCEPTION("ELI02384")
	CATCH_UNEXPECTED_EXCEPTION("ELI02385")
    
	// registers object, typelib and all interfaces in typelib
    return hr;
}

/////////////////////////////////////////////////////////////////////////////
// DllUnregisterServer - Removes entries from the system registry

STDAPI DllUnregisterServer(void)
{
    return _Module.UnregisterServer(TRUE);
}


