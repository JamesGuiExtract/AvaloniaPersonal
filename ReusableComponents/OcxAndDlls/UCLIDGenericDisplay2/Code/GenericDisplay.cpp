//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	GenericDisplay.cpp
//
// PURPOSE:	This is an implementation file for GenericDisplay() class.
//			Where the GenericDisplayApp() class has been derived from COleControlModule().
//			The code written in this file makes it possible to implement the various
//			application methods in the control.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
// GenericDisplay.cpp : Implementation of CGenericDisplayApp and DLL registration.

#include "stdafx.h"
#include "GenericDisplay.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif


CGenericDisplayApp NEAR theApp;

const GUID CDECL BASED_CODE _tlid =
		{ 0x14981573, 0x9117, 0x11d4, { 0x97, 0x25, 0, 0x80, 0x48, 0xfb, 0xc9, 0x6e } };
const WORD _wVerMajor = 1;
const WORD _wVerMinor = 0;


////////////////////////////////////////////////////////////////////////////
// CGenericDisplayApp::InitInstance - DLL initialization
//==========================================================================================
BOOL CGenericDisplayApp::InitInstance()
{
	BOOL bInit = COleControlModule::InitInstance();

	if (bInit)
	{
		// TODO: Add your own module initialization code here.
	}

	AfxEnableControlContainer();

	return bInit;
}
//==========================================================================================

////////////////////////////////////////////////////////////////////////////
// CGenericDisplayApp::ExitInstance - DLL termination

int CGenericDisplayApp::ExitInstance()
{
	return COleControlModule::ExitInstance();
}

//==========================================================================================
/////////////////////////////////////////////////////////////////////////////
// DllRegisterServer - Adds entries to the system registry

STDAPI DllRegisterServer(void)
{
	AFX_MANAGE_STATE(_afxModuleAddrThis);

	if (!AfxOleRegisterTypeLib(AfxGetInstanceHandle(), _tlid))
		return ResultFromScode(SELFREG_E_TYPELIB);

	if (!COleObjectFactoryEx::UpdateRegistryAll(TRUE))
		return ResultFromScode(SELFREG_E_CLASS);

	return NOERROR;
}

//==========================================================================================
/////////////////////////////////////////////////////////////////////////////
// DllUnregisterServer - Removes entries from the system registry

STDAPI DllUnregisterServer(void)
{
	AFX_MANAGE_STATE(_afxModuleAddrThis);

	if (!AfxOleUnregisterTypeLib(_tlid, _wVerMajor, _wVerMinor))
		return ResultFromScode(SELFREG_E_TYPELIB);

	if (!COleObjectFactoryEx::UpdateRegistryAll(FALSE))
		return ResultFromScode(SELFREG_E_CLASS);

	return NOERROR;
}
//==========================================================================================
