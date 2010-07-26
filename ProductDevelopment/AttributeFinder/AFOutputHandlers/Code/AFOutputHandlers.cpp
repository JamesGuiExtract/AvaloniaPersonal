// AFOutputHandlers.cpp : Implementation of DLL Exports.


// Note: Proxy/Stub Information
//      To build a separate proxy/stub DLL, 
//      run nmake -f AFOutputHandlersps.mk in the project directory.

#include "stdafx.h"
#include "resource.h"
#include <initguid.h>
#include "AFOutputHandlers.h"

#include "AFOutputHandlers_i.c"
#include "EliminateDuplicates.h"
#include "RemoveInvalidEntries.h"
#include "SelectOnlyUniqueValues.h"
#include "SelectUsingMajority.h"
#include "OutputHandlerSequence.h"
#include "RemoveEntriesFromList.h"
#include "KeepAttributesInMemory.h"
#include "RemoveEntriesFromListPP.h"

#include "OutputToXML.h"
#include "OutputToXMLPP.h"
#include "ModifyAttributeValueOH.h"
#include "ModifyAttributeValuePP.h"
#include "OutputToVOA.h"
#include "OutputToVOAPP.h"
#include "MoveAndModifyAttributes.h"
#include "MoveAndModifyAttributesPP.h"
#include "RemoveSubAttributes.h"
#include "RemoveSubAttributesPP.h"
#include "ReformatPersonNames.h"
#include "ReformatPersonNamesPP.h"
#include "RunObjectOnQuery.h"
#include "RunObjectOnQueryPP.h"
#include "ConditionalOutputHandler.h"
CComModule _Module;

BEGIN_OBJECT_MAP(ObjectMap)
OBJECT_ENTRY(CLSID_EliminateDuplicates, CEliminateDuplicates)
OBJECT_ENTRY(CLSID_RemoveInvalidEntries, CRemoveInvalidEntries)
OBJECT_ENTRY(CLSID_SelectOnlyUniqueValues, CSelectOnlyUniqueValues)
OBJECT_ENTRY(CLSID_SelectUsingMajority, CSelectUsingMajority)
OBJECT_ENTRY(CLSID_OutputHandlerSequence, COutputHandlerSequence)
OBJECT_ENTRY(CLSID_RemoveEntriesFromList, CRemoveEntriesFromList)
OBJECT_ENTRY(CLSID_KeepAttributesInMemory, CKeepAttributesInMemory)
OBJECT_ENTRY(CLSID_RemoveEntriesFromListPP, CRemoveEntriesFromListPP)
OBJECT_ENTRY(CLSID_OutputToXML, COutputToXML)
OBJECT_ENTRY(CLSID_OutputToXMLPP, COutputToXMLPP)
OBJECT_ENTRY(CLSID_ModifyAttributeValueOH, CModifyAttributeValueOH)
OBJECT_ENTRY(CLSID_ModifyAttributeValuePP, CModifyAttributeValuePP)
OBJECT_ENTRY(CLSID_OutputToVOA, COutputToVOA)
OBJECT_ENTRY(CLSID_OutputToVOAPP, COutputToVOAPP)
OBJECT_ENTRY(CLSID_MoveAndModifyAttributes, CMoveAndModifyAttributes)
OBJECT_ENTRY(CLSID_MoveAndModifyAttributesPP, CMoveAndModifyAttributesPP)
OBJECT_ENTRY(CLSID_RemoveSubAttributes, CRemoveSubAttributes)
OBJECT_ENTRY(CLSID_RemoveSubAttributesPP, CRemoveSubAttributesPP)
OBJECT_ENTRY(CLSID_ReformatPersonNames, CReformatPersonNames)
OBJECT_ENTRY(CLSID_ReformatPersonNamesPP, CReformatPersonNamesPP)
OBJECT_ENTRY(CLSID_RunObjectOnQuery, CRunObjectOnQuery)
OBJECT_ENTRY(CLSID_RunObjectOnQueryPP, CRunObjectOnQueryPP)
OBJECT_ENTRY(CLSID_ConditionalOutputHandler, CConditionalOutputHandler)
END_OBJECT_MAP()

class CAFOutputHandlersApp : public CWinApp
{
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CAFOutputHandlersApp)
	public:
    virtual BOOL InitInstance();
    virtual int ExitInstance();
	//}}AFX_VIRTUAL

	//{{AFX_MSG(CAFOutputHandlersApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

BEGIN_MESSAGE_MAP(CAFOutputHandlersApp, CWinApp)
	//{{AFX_MSG_MAP(CAFOutputHandlersApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

CAFOutputHandlersApp theApp;

BOOL CAFOutputHandlersApp::InitInstance()
{
    _Module.Init(ObjectMap, m_hInstance, &LIBID_UCLID_AFOUTPUTHANDLERSLib);

    return CWinApp::InitInstance();
}

int CAFOutputHandlersApp::ExitInstance()
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


#include "RemoveSubAttributes.h"
#include "RemoveSubAttributesPP.h"
#include "ReformatPersonNames.h"
#include "ReformatPersonNamesPP.h"
#include "RunObjectOnQuery.h"
#include "RunObjectOnQueryPP.h"
#include "ConditionalOutputHandler.h"
