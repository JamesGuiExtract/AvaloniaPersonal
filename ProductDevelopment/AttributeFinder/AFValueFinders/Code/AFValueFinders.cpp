// AFValueFinders.cpp : Implementation of DLL Exports.


// Note: Proxy/Stub Information
//      To build a separate proxy/stub DLL, 
//      run nmake -f AFValueFindersps.mk in the project directory.

#include "stdafx.h"
#include "resource.h"
#include <initguid.h>
#include "AFValueFinders.h"

#include "AFValueFinders_i.c"
#include "ValueAfterClue.h"
#include "ValueBeforeClue.h"
#include "ValueFromList.h"
#include "..\..\AFCore\Code\AFCategories.h"
#include "RegExprRule.h"
#include "RegExprRulePP.h"

#include <ComUtils.h>
#include "ValueAfterCluePP.h"
#include "ValueBeforeCluePP.h"
#include "ValueFromListPP.h"
#include "ExtractLine.h"
#include "ExtractLinePP.h"
#include "LegalDescriptionFinder.h"
#include "BlockFinder.h"
#include "BlockFinderPP.h"
#include "SPMFinder.h"
#include "SPMFinderPP.h"
#include "REPMFinder.h"
#include "REPMFinderPP.h"
#include "ReturnAddrFinder.h"
#include "LocateImageRegion.h"
#include "LocateImageRegionPP.h"
#include "AddressFinder.h"
#include "ReturnAddrFinderPP.h"
#include "CreateValue.h"
#include "CreateValuePP.h"
#include "FindFromRSD.h"
#include "FindFromRSDPP.h"
#include "ConditionalValueFinder.h"

CComModule _Module;

BEGIN_OBJECT_MAP(ObjectMap)
OBJECT_ENTRY(CLSID_ValueAfterClue, CValueAfterClue)
OBJECT_ENTRY(CLSID_ValueBeforeClue, CValueBeforeClue)
OBJECT_ENTRY(CLSID_ValueFromList, CValueFromList)
OBJECT_ENTRY(CLSID_RegExprRule, CRegExprRule)
OBJECT_ENTRY(CLSID_RegExprRulePP, CRegExprRulePP)
OBJECT_ENTRY(CLSID_ValueAfterCluePP, CValueAfterCluePP)
OBJECT_ENTRY(CLSID_ValueBeforeCluePP, CValueBeforeCluePP)
OBJECT_ENTRY(CLSID_ValueFromListPP, CValueFromListPP)
OBJECT_ENTRY(CLSID_ExtractLine, CExtractLine)
OBJECT_ENTRY(CLSID_ExtractLinePP, CExtractLinePP)
OBJECT_ENTRY(CLSID_LegalDescriptionFinder, CLegalDescriptionFinder)
OBJECT_ENTRY(CLSID_BlockFinder, CBlockFinder)
OBJECT_ENTRY(CLSID_BlockFinderPP, CBlockFinderPP)
OBJECT_ENTRY(CLSID_SPMFinder, CSPMFinder)
OBJECT_ENTRY(CLSID_SPMFinderPP, CSPMFinderPP)
OBJECT_ENTRY(CLSID_REPMFinder, CREPMFinder)
OBJECT_ENTRY(CLSID_REPMFinderPP, CREPMFinderPP)
OBJECT_ENTRY(CLSID_ReturnAddrFinder, CReturnAddrFinder)
OBJECT_ENTRY(CLSID_LocateImageRegion, CLocateImageRegion)
OBJECT_ENTRY(CLSID_LocateImageRegionPP, CLocateImageRegionPP)
OBJECT_ENTRY(CLSID_AddressFinder, CAddressFinder)
OBJECT_ENTRY(CLSID_ReturnAddrFinderPP, CReturnAddrFinderPP)
OBJECT_ENTRY(CLSID_CreateValue, CCreateValue)
OBJECT_ENTRY(CLSID_CreateValuePP, CCreateValuePP)
OBJECT_ENTRY(CLSID_FindFromRSD, CFindFromRSD)
OBJECT_ENTRY(CLSID_FindFromRSDPP, CFindFromRSDPP)
OBJECT_ENTRY(CLSID_ConditionalValueFinder, CConditionalValueFinder)
END_OBJECT_MAP()

class CAFValueFindersApp : public CWinApp
{
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CAFValueFindersApp)
	public:
    virtual BOOL InitInstance();
    virtual int ExitInstance();
	//}}AFX_VIRTUAL

	//{{AFX_MSG(CAFValueFindersApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

BEGIN_MESSAGE_MAP(CAFValueFindersApp, CWinApp)
	//{{AFX_MSG_MAP(CAFValueFindersApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

CAFValueFindersApp theApp;

BOOL CAFValueFindersApp::InitInstance()
{
    _Module.Init(ObjectMap, m_hInstance, &LIBID_UCLID_AFVALUEFINDERSLib);
    return CWinApp::InitInstance();
}

int CAFValueFindersApp::ExitInstance()
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
	// TODO: Why do the following lines not register the class with the specified
	// categories?  As a workaround until we find the reason for these two lines
	// not working as expected, the 'Implemented Categories' key is set from the
	// RegExprRule.rgs file.  We need to debug this problem, get the following code
	// to work, and remove the 'Implemented Categories' key from the .rgs file.

	// register the RegExprRule object under both the finders and modifiers categories
	// registerCOMComponentInCategory(CLSID_RegExprRule, CATID_AFAPI_VALUE_FINDERS);
	// registerCOMComponentInCategory(CLSID_RegExprRule, CATID_AFAPI_VALUE_MODIFIERS);

    // registers object, typelib and all interfaces in typelib
    return _Module.RegisterServer(TRUE);
}

/////////////////////////////////////////////////////////////////////////////
// DllUnregisterServer - Removes entries from the system registry

STDAPI DllUnregisterServer(void)
{
    return _Module.UnregisterServer(TRUE);
}
