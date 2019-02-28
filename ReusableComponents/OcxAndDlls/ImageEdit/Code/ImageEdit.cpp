//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ImageEdit.cpp
//
// PURPOSE:	This is an implementation file for CImageEditApp() class.
//			Where the CImageEditApp() class has been derived from COleControlModule() class.
//			The code written in this file makes it possible to implement the various
//			application methods in the user interface.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
// ImageEdit.cpp : Implementation of CImageEditApp and DLL registration.

#include "stdafx.h"
#include "ImageEdit.h"

#include <ltwrappr.h>
#include <LicenseMgmt.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif


CImageEditApp NEAR theApp;

const GUID CDECL BASED_CODE _tlid =
		{ 0x65c0b2ea, 0x166a, 0x4212, { 0x92, 0xc8, 0x42, 0xe5, 0xa3, 0x19, 0xc0, 0x93 } };
const WORD _wVerMajor = 1;
const WORD _wVerMinor = 0;


////////////////////////////////////////////////////////////////////////////
// CImageEditApp::InitInstance - DLL initialization

BOOL CImageEditApp::InitInstance()
{
	BOOL bInit = COleControlModule::InitInstance();

	if (bInit)
	{
		LBase::SetErrorListDepth(20);

		LBase::LoadLibraries(LT_FIL|LT_KRN|LT_DIS|LT_IMG|LT_ANN);

		// Unlock support for Document toolkit for annotations
		if (LicenseManagement::isAnnotationLicensed())
		{
			// Unlock Document/Medical support only if 
			// Annotation package is licensed (P13 #4499)
			LSettings::UnlockSupport(L_SUPPORT_DOCUMENT, L_KEY_DOCUMENT);

			// check if document support was unlocked
			if( L_IsSupportLocked(L_SUPPORT_DOCUMENT) == L_TRUE )
			{
				UCLIDException ue("ELI19819", "Unable to unlock document support.");
				ue.addDebugInfo("Document Key", L_KEY_DOCUMENT, true);
				ue.log();
			}
		}

		LBase::SetErrorListDepth(25);
	}

	return bInit;
}
//==================================================================================================

////////////////////////////////////////////////////////////////////////////
// CImageEditApp::ExitInstance - DLL termination

int CImageEditApp::ExitInstance()
{
	LBase::UnloadLibraries(LT_FIL|LT_KRN|LT_DIS);
	return COleControlModule::ExitInstance();
}

//==================================================================================================
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
//==================================================================================================

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
//==================================================================================================
