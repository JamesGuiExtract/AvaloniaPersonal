// ArcGISIcoMap.cpp : Implementation of DLL Exports.


// Note: Proxy/Stub Information
//      To build a separate proxy/stub DLL, 
//      run nmake -f ArcGISIcoMapps.mk in the project directory.

#include "stdafx.h"
#include "resource.h"
#include <initguid.h>
#include "ArcGISIcoMap.h"

#include "ArcGISIcoMap_i.c"
#include "IcoMapDrawingCtrl.h"
#include "EditEventsSink.h"
#include <cpputil.h>
#include <UCLIDException.h>
#include <COMUtils.h>

HINSTANCE gModuleResource = NULL;
CComModule _Module;

BEGIN_OBJECT_MAP(ObjectMap)
OBJECT_ENTRY(CLSID_IcoMapDrawingCtrl, CIcoMapDrawingCtrl)
OBJECT_ENTRY(CLSID_EditEventsSink, CEditEventsSink)
END_OBJECT_MAP()

//-------------------------------------------------------------------------------------------------
// CArcGISIcoMapApp Declaration
//-------------------------------------------------------------------------------------------------
class CArcGISIcoMapApp : public CWinApp
{
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CArcGISIcoMapApp)
	public:
    virtual BOOL InitInstance();
    virtual int ExitInstance();
	//}}AFX_VIRTUAL

	//{{AFX_MSG(CArcGISIcoMapApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//-------------------------------------------------------------------------------------------------
//  CArcGISIcoMapApp message map
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CArcGISIcoMapApp, CWinApp)
	//{{AFX_MSG_MAP(CArcGISIcoMapApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
//-------------------------------------------------------------------------------------------------

//-------------------------------------------------------------------------------------------------
// Define instance of CArcGISIcoMapApp
//-------------------------------------------------------------------------------------------------
CArcGISIcoMapApp theApp;

//-------------------------------------------------------------------------------------------------
// CArcGISIcoMapApp implementation
//-------------------------------------------------------------------------------------------------
BOOL CArcGISIcoMapApp::InitInstance()
{
    _Module.Init(ObjectMap, m_hInstance, &LIBID_ARCGISICOMAPLib);
	gModuleResource = m_hInstance;

	AfxEnableControlContainer();
    return CWinApp::InitInstance();
}
//-------------------------------------------------------------------------------------------------
int CArcGISIcoMapApp::ExitInstance()
{
    _Module.Term();
    return CWinApp::ExitInstance();
}

//-------------------------------------------------------------------------------------------------
// Dll functions
//-------------------------------------------------------------------------------------------------
// registerIcoMapExtension()
//		private function for registering and unregistering the IcoMap extension
//		if bRegister is true IcoMap extension is registered in the ArcGIS extension category 
//		if bRegister is false coMap extension is unregistered in the ArcGIS extension category 
void registerIcoMapExtension(bool bRegister )
{
	try
	{
		try
		{
			CoInitialize(NULL);

			// Create instance of ArgGIS Component manaager
			CComPtr<IComponentCategoryManager> ipCatMgr;
			ipCatMgr.CoCreateInstance(CLSID_ComponentCategoryManager);
			ASSERT_RESOURCE_ALLOCATION("ELI15900", ipCatMgr != NULL );
			
			// Create instance of ArgGIS UID for IcoMapDrawingControl GUID
			CComPtr<IUID> ipIcoMapDrawingCtrlID;
			ipIcoMapDrawingCtrlID.CoCreateInstance(CLSID_UID);
			ASSERT_RESOURCE_ALLOCATION("ELI15911", ipIcoMapDrawingCtrlID != NULL );

			// Set to IcoMapDrawingControl GUID
			ipIcoMapDrawingCtrlID->Value = _variant_t("{D80801D0-7AC8-11D5-817F-0050DAD4FF55}");
	
			// Create Instance of ArgGIS UID for ArcGIS MxExtension Category GUID
			CComPtr<IUID> ipMxExtensionCatID;
			ipMxExtensionCatID.CoCreateInstance(CLSID_UID);
			ASSERT_RESOURCE_ALLOCATION("ELI15910", ipMxExtensionCatID != NULL );

			// Set to ArcGIS MxExtension Cateory GUID
			ipMxExtensionCatID->Value = _variant_t("{B56A7C45-83D4-11D2-A2E9-080009B6F22B}");

			// Get path to this dll
			string strDLLPath = ::getModuleDirectory(_Module.m_hInst) + "\\ArcGISIcoMap.DLL";

			// Register the IcomapDrawingControl with the MxExtension Category
			ipCatMgr->SetupObject(_bstr_t(strDLLPath.c_str()), ipIcoMapDrawingCtrlID,
				ipMxExtensionCatID, (bRegister) ? VARIANT_TRUE : VARIANT_FALSE);
		}
		CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15909");
		CoUninitialize();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI15908");
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

	registerIcoMapExtension(true);

	return hr;
}

//-------------------------------------------------------------------------------------------------
// DllUnregisterServer - Removes entries from the system registry
STDAPI DllUnregisterServer(void)
{

	registerIcoMapExtension(false);	   
	
	// unregisters object, typelib and all interfaces in typelib
    HRESULT hr = _Module.UnregisterServer(TRUE);

	return hr;
}
//-------------------------------------------------------------------------------------------------

