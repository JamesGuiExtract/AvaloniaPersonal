// AFValueModifiers.cpp : Implementation of DLL Exports.


// Note: Proxy/Stub Information
//      To build a separate proxy/stub DLL, 
//      run nmake -f AFValueModifiersps.mk in the project directory.

#include "stdafx.h"
#include "resource.h"
#include <initguid.h>
#include "AFValueModifiers.h"

#include "AFValueModifiers_i.c"
#include "LimitAsLeftPart.h"
#include "LimitAsRightPart.h"
#include "LimitAsMidPart.h"
#include "LimitAsLeftPartPP.h"
#include "TranslateValue.h"
#include "RemoveCharacters.h"
#include "ReplaceStrings.h"
#include "TranslateToClosestValueInList.h"
#include "LimitAsMidPartPP.h"
#include "LimitAsRightPartPP.h"
#include "RemoveCharactersPP.h"
#include "ReplaceStringsPP.h"
#include "TranslateValuePP.h"
#include "TranslateToClosestValueInListPP.h"
#include "AdvancedReplaceString.h"
#include "AdvancedReplaceStringPP.h"
#include "InsertCharacters.h"
#include "InsertCharactersPP.h"
#include "StringTokenizerModifier.h"
#include "StringTokenizerModifierPP.h"
#include "ChangeCase.h"
#include "ChangeCasePP.h"
#include "PadValue.h"
#include "PadValuePP.h"
#include "ConditionalAttributeModifier.h"
#include "OCRArea.h"
#include "OCRAreaPP.h"

CComModule _Module;

BEGIN_OBJECT_MAP(ObjectMap)
OBJECT_ENTRY(CLSID_LimitAsLeftPart, CLimitAsLeftPart)
OBJECT_ENTRY(CLSID_LimitAsRightPart, CLimitAsRightPart)
OBJECT_ENTRY(CLSID_LimitAsMidPart, CLimitAsMidPart)
OBJECT_ENTRY(CLSID_LimitAsLeftPartPP, CLimitAsLeftPartPP)
OBJECT_ENTRY(CLSID_TranslateValue, CTranslateValue)
OBJECT_ENTRY(CLSID_RemoveCharacters, CRemoveCharacters)
OBJECT_ENTRY(CLSID_ReplaceStrings, CReplaceStrings)
OBJECT_ENTRY(CLSID_TranslateToClosestValueInList, CTranslateToClosestValueInList)
OBJECT_ENTRY(CLSID_LimitAsMidPartPP, CLimitAsMidPartPP)
OBJECT_ENTRY(CLSID_LimitAsRightPartPP, CLimitAsRightPartPP)
OBJECT_ENTRY(CLSID_RemoveCharactersPP, CRemoveCharactersPP)
OBJECT_ENTRY(CLSID_ReplaceStringsPP, CReplaceStringsPP)
OBJECT_ENTRY(CLSID_TranslateValuePP, CTranslateValuePP)
OBJECT_ENTRY(CLSID_TranslateToClosestValueInListPP, CTranslateToClosestValueInListPP)
OBJECT_ENTRY(CLSID_AdvancedReplaceString, CAdvancedReplaceString)
OBJECT_ENTRY(CLSID_AdvancedReplaceStringPP, CAdvancedReplaceStringPP)
OBJECT_ENTRY(CLSID_InsertCharacters, CInsertCharacters)
OBJECT_ENTRY(CLSID_InsertCharactersPP, CInsertCharactersPP)
OBJECT_ENTRY(CLSID_StringTokenizerModifier, CStringTokenizerModifier)
OBJECT_ENTRY(CLSID_StringTokenizerModifierPP, CStringTokenizerModifierPP)
OBJECT_ENTRY(CLSID_ChangeCase, CChangeCase)
OBJECT_ENTRY(CLSID_ChangeCasePP, CChangeCasePP)
OBJECT_ENTRY(CLSID_PadValue, CPadValue)
OBJECT_ENTRY(CLSID_PadValuePP, CPadValuePP)
OBJECT_ENTRY(CLSID_ConditionalAttributeModifier, CConditionalAttributeModifier)
OBJECT_ENTRY(CLSID_OCRArea, COCRArea)
OBJECT_ENTRY(CLSID_OCRAreaPP, COCRAreaPP)
END_OBJECT_MAP()

class CAFValueModifiersApp : public CWinApp
{
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CAFValueModifiersApp)
	public:
    virtual BOOL InitInstance();
    virtual int ExitInstance();
	//}}AFX_VIRTUAL

	//{{AFX_MSG(CAFValueModifiersApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

BEGIN_MESSAGE_MAP(CAFValueModifiersApp, CWinApp)
	//{{AFX_MSG_MAP(CAFValueModifiersApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

CAFValueModifiersApp theApp;

BOOL CAFValueModifiersApp::InitInstance()
{
    _Module.Init(ObjectMap, m_hInstance, &LIBID_UCLID_AFVALUEMODIFIERSLib);
    return CWinApp::InitInstance();
}

int CAFValueModifiersApp::ExitInstance()
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


