// UCLIDFeatureMgmt.cpp : Implementation of DLL Exports.


// Note: Proxy/Stub Information
//      To build a separate proxy/stub DLL, 
//      run nmake -f UCLIDFeatureMgmtps.mk in the project directory.

#include "stdafx.h"
#include "resource.h"
#include <initguid.h>
#include "UCLIDFeatureMgmt.h"

#include "UCLIDFeatureMgmt_i.c"
#include "LineSegment.h"
#include "ArcSegment.h"
#include "ParameterTypeValuePair.h"
#include "EnumSegment.h"
#include "Part.h"
#include "EnumPart.h"
#include "Feature.h"
#include "CommaDelimitedFeatureAttributeDataInterpreter.h"
#include "UCLIDFeatureMgmtCat.h"

#include <UCLIDException.h>
#include <COMUtils.h>

CComModule _Module;

BEGIN_OBJECT_MAP(ObjectMap)
OBJECT_ENTRY(CLSID_LineSegment, CLineSegment)
OBJECT_ENTRY(CLSID_ArcSegment, CArcSegment)
OBJECT_ENTRY(CLSID_ParameterTypeValuePair, CParameterTypeValuePair)
OBJECT_ENTRY(CLSID_EnumSegment, CEnumSegment)
OBJECT_ENTRY(CLSID_Part, CPart)
OBJECT_ENTRY(CLSID_EnumPart, CEnumPart)
OBJECT_ENTRY(CLSID_Feature, CFeature)
OBJECT_ENTRY(CLSID_CommaDelimitedFeatureAttributeDataInterpreter, CCommaDelimitedFeatureAttributeDataInterpreter)
END_OBJECT_MAP()

class CUCLIDFeatureMgmtApp : public CWinApp
{
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CUCLIDFeatureMgmtApp)
	public:
    virtual BOOL InitInstance();
    virtual int ExitInstance();
	//}}AFX_VIRTUAL

	//{{AFX_MSG(CUCLIDFeatureMgmtApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

BEGIN_MESSAGE_MAP(CUCLIDFeatureMgmtApp, CWinApp)
	//{{AFX_MSG_MAP(CUCLIDFeatureMgmtApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

CUCLIDFeatureMgmtApp theApp;

BOOL CUCLIDFeatureMgmtApp::InitInstance()
{
    _Module.Init(ObjectMap, m_hInstance, &LIBID_UCLID_FEATUREMGMTLib);
    return CWinApp::InitInstance();
}

int CUCLIDFeatureMgmtApp::ExitInstance()
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

	// register the category information
	try
	{
		createCOMCategory(CATID_IFeatureAttributeDataInterpreter, 
			strIFeatureAttributeDataInterpreterCategoryName);
		registerCOMComponentInCategory(CLSID_CommaDelimitedFeatureAttributeDataInterpreter, 
			CATID_IFeatureAttributeDataInterpreter);
	}
	CATCH_UCLID_EXCEPTION("ELI02158")
	CATCH_UNEXPECTED_EXCEPTION("ELI02159")

	return hr;
}

/////////////////////////////////////////////////////////////////////////////
// DllUnregisterServer - Removes entries from the system registry

STDAPI DllUnregisterServer(void)
{
    return _Module.UnregisterServer(TRUE);
}
