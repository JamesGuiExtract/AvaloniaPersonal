// RedactionCustomComponents.cpp : Implementation of DLL Exports.


// Note: Proxy/Stub Information
//      To build a separate proxy/stub DLL, 
//      run nmake -f RedactionCustomComponentsps.mk in the project directory.

#include "stdafx.h"
#include "resource.h"
#include <initguid.h>
#include "RedactionCustomComponents.h"
#include "RedactionCustomComponents_i.c"

#include "RedactionVerificationUI.h"
#include "RedactionVerificationUIPP.h"
#include "RedactFileProcessor.h"
#include "RedactFileProcessorPP.h"
#include "RedactionTask.h"
#include "RedactionTaskPP.h"
#include "IDShieldVOAFileContentsCondition.h"
#include "IDShieldVOAFileContentsConditionPP.h"
#include "SelectTargetFileUI.h"
#include "SSNFinder.h"
#include "SSNFinderPP.h"
#include "FilterIDShieldDataFileTask.h"
#include "FilterIDShieldDataFileTaskPP.h"

#include <AFAboutDlg.h>
#include <UCLIDException.h>
#include <UCLIDExceptionDlg.h>
#include <cpputil.h>

CComModule _Module;

BEGIN_OBJECT_MAP(ObjectMap)
OBJECT_ENTRY(CLSID_RedactionTask, CRedactionTask)
OBJECT_ENTRY(CLSID_RedactionTaskPP, CRedactionTaskPP)
OBJECT_ENTRY(CLSID_IDShieldVOAFileContentsCondition, CIDShieldVOAFileContentsCondition)
OBJECT_ENTRY(CLSID_IDShieldVOAFileContentsConditionPP, CIDShieldVOAFileContentsConditionPP)
OBJECT_ENTRY(CLSID_SelectTargetFileUI, CSelectTargetFileUI)
OBJECT_ENTRY(CLSID_SSNFinder, CSSNFinder)
OBJECT_ENTRY(CLSID_SSNFinderPP, CSSNFinderPP)
OBJECT_ENTRY(CLSID_FilterIDShieldDataFileTask, CFilterIDShieldDataFileTask)
OBJECT_ENTRY(CLSID_FilterIDShieldDataFileTaskPP, CFilterIDShieldDataFileTaskPP)
END_OBJECT_MAP()

//-------------------------------------------------------------------------------------------------
std::string getProductName(EHelpAboutType eType)
{
	// Get Product Name
	CString zProductName;
	switch (eType)
	{
	case UCLID_AFCORELib::kFlexIndexHelpAbout:
		zProductName.LoadString( IDS_FLEXINDEX_PRODUCT );
		break;

	case UCLID_AFCORELib::kIDShieldHelpAbout:
		zProductName.LoadString( IDS_IDSHIELD_PRODUCT );
		break;

	default:
		zProductName.LoadString( IDS_UNKNOWN_PRODUCT );
		break;
	}

	return zProductName.operator LPCTSTR();
}

//-------------------------------------------------------------------------------------------------
std::string getAttributeFinderEngineVersion(EHelpAboutType eType)
{
	// Get module path and filename
	char zFileName[MAX_PATH];
	int ret = ::GetModuleFileName( _Module.m_hInst, zFileName, MAX_PATH );
	if (ret == 0)
	{
		throw UCLIDException( "ELI11652", "Unable to retrieve module file name!" );
	}

	// TODO : Set application-specific icons

	// Retrieve version information from this module
	string strVersion = getProductName( eType );
	strVersion += " Version ";
	strVersion += ::getFileVersion( string( zFileName ) );

	return strVersion;
}

class CRedactionCustomComponentsApp : public CWinApp
{
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CRedactionCustomComponentsApp)
	public:
    virtual BOOL InitInstance();
    virtual int ExitInstance();
	//}}AFX_VIRTUAL

	//{{AFX_MSG(CRedactionCustomComponentsApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CRedactionCustomComponentsApp, CWinApp)
	//{{AFX_MSG_MAP(CRedactionCustomComponentsApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
//-------------------------------------------------------------------------------------------------
CRedactionCustomComponentsApp theApp;

BOOL CRedactionCustomComponentsApp::InitInstance()
{
    _Module.Init(ObjectMap, m_hInstance, &LIBID_UCLID_REDACTIONCUSTOMCOMPONENTSLib);

	try
	{
		// setup the global uclid exception related settings
		static UCLIDExceptionDlg exceptionDlg;
		UCLIDException::setExceptionHandler(&exceptionDlg);
		UCLIDException::setApplication( getAttributeFinderEngineVersion( kIDShieldHelpAbout ) );
	}
	catch(...)
	{
	}

    return CWinApp::InitInstance();
}
//-------------------------------------------------------------------------------------------------
int CRedactionCustomComponentsApp::ExitInstance()
{
    _Module.Term();

    return CWinApp::ExitInstance();
}

//-------------------------------------------------------------------------------------------------
// Used to determine whether the DLL can be unloaded by OLE
//-------------------------------------------------------------------------------------------------
STDAPI DllCanUnloadNow(void)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());
    return (AfxDllCanUnloadNow()==S_OK && _Module.GetLockCount()==0) ? S_OK : S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// Returns a class factory to create an object of the requested type
//-------------------------------------------------------------------------------------------------
STDAPI DllGetClassObject(REFCLSID rclsid, REFIID riid, LPVOID* ppv)
{
    return _Module.GetClassObject(rclsid, riid, ppv);
}

//-------------------------------------------------------------------------------------------------
// DllRegisterServer - Adds entries to the system registry
//-------------------------------------------------------------------------------------------------
STDAPI DllRegisterServer(void)
{
    // registers object, typelib and all interfaces in typelib
    return _Module.RegisterServer(TRUE);
}

//-------------------------------------------------------------------------------------------------
// DllUnregisterServer - Removes entries from the system registry
//-------------------------------------------------------------------------------------------------
STDAPI DllUnregisterServer(void)
{
    return _Module.UnregisterServer(TRUE);
}
//-------------------------------------------------------------------------------------------------
