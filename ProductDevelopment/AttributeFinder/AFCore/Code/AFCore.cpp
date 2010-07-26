// AFCore.cpp : Implementation of DLL Exports.


// Note: Proxy/Stub Information
//      To build a separate proxy/stub DLL, 
//      run nmake -f AFCoreps.mk in the project directory.

#include "stdafx.h"
#include "resource.h"
#include <initguid.h>
#include "AFCore.h"

#include "AFCore_i.c"
#include "AttributeRule.h"
#include "Attribute.h"
#include "RuleSet.h"
#include "AttributeFinderEngine.h"
#include "AttributeFindInfo.h"

#include "AFAboutDlg.h"
#include "AFCategories.h"
#include <ComUtils.h>
#include "AFDocument.h"
#include "RuleExecutionEnv.h"
#include "RuleExecutionSession.h"
#include "RuleTesterUI.h"
#include "TesterDlgRulesetPage.h"
#include <TLFrame.h>
#include <UCLIDException.h>
#include <UCLIDExceptionDlg.h>
#include "ConditionalRulePP.h"
#include "SpatiallyCompareAttributes.h"

CComModule _Module;

BEGIN_OBJECT_MAP(ObjectMap)
OBJECT_ENTRY(CLSID_AttributeRule, CAttributeRule)
OBJECT_ENTRY(CLSID_RuleSet, CRuleSet)
OBJECT_ENTRY(CLSID_Attribute, CAttribute)
OBJECT_ENTRY(CLSID_AttributeFinderEngine, CAttributeFinderEngine)
OBJECT_ENTRY(CLSID_AttributeFindInfo, CAttributeFindInfo)
OBJECT_ENTRY(CLSID_AFDocument, CAFDocument)
OBJECT_ENTRY(CLSID_RuleExecutionEnv, CRuleExecutionEnv)
OBJECT_ENTRY(CLSID_RuleExecutionSession, CRuleExecutionSession)
OBJECT_ENTRY(CLSID_RuleTesterUI, CRuleTesterUI)
OBJECT_ENTRY(CLSID_ConditionalRulePP, CConditionalRulePP)
OBJECT_ENTRY(CLSID_SpatiallyCompareAttributes, CSpatiallyCompareAttributes)
END_OBJECT_MAP()

class CAFCoreApp : public CWinApp
{
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CAFCoreApp)
	public:
    virtual BOOL InitInstance();
    virtual int ExitInstance();
	//}}AFX_VIRTUAL

	//{{AFX_MSG(CAFCoreApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

BEGIN_MESSAGE_MAP(CAFCoreApp, CWinApp)
	//{{AFX_MSG_MAP(CAFCoreApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

CAFCoreApp theApp;

BOOL CAFCoreApp::InitInstance()
{
    _Module.Init(ObjectMap, m_hInstance, &LIBID_UCLID_AFCORELib);

	try
	{
		// setup the global uclid exception related settings
		static UCLIDExceptionDlg exceptionDlg;
		UCLIDException::setExceptionHandler(&exceptionDlg);
		UCLIDException::setApplication( getAttributeFinderEngineVersion( 
			kFlexIndexHelpAbout ) );
	}
	catch(...)
	{
	}

	// Register the TLFrame class
	CTLFrame::RegisterClass();

    return CWinApp::InitInstance();
}

int CAFCoreApp::ExitInstance()
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
	// create various attribute-finder related COM categories
	createCOMCategory(CATID_AFAPI_OUTPUT_HANDLERS, AFAPI_OUTPUT_HANDLERS_CATEGORYNAME);
	createCOMCategory(CATID_AFAPI_VALUE_MODIFIERS, AFAPI_VALUE_MODIFIERS_CATEGORYNAME);
	createCOMCategory(CATID_AFAPI_VALUE_FINDERS, AFAPI_VALUE_FINDERS_CATEGORYNAME);
	createCOMCategory(CATID_AFAPI_ATTRIBUTE_SPLITTERS, AFAPI_ATTRIBUTE_SPLITTERS_CATEGORYNAME);
	createCOMCategory(CATID_AFAPI_DOCUMENT_PREPROCESSORS, AFAPI_DOCUMENT_PREPROCESSORS_CATEGORYNAME);
	createCOMCategory(CATID_AFAPI_DATA_SCORERS, AFAPI_DATA_SCORERS_CATEGORYNAME);
	createCOMCategory(CATID_AFAPI_CONDITIONS, AFAPI_CONDITIONS_CATEGORYNAME);
	createCOMCategory(CATID_AFAPI_ATTRIBUTE_SELECTORS, AFAPI_ATTRIBUTE_SELECTORS_CATEGORYNAME);

    // registers object, typelib and all interfaces in typelib
    return _Module.RegisterServer(TRUE);
}

/////////////////////////////////////////////////////////////////////////////
// DllUnregisterServer - Removes entries from the system registry

STDAPI DllUnregisterServer(void)
{
    return _Module.UnregisterServer(TRUE);
}
