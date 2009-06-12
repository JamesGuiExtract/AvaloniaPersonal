//===========================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	MCRTextViewer.cpp
//
// PURPOSE:	This is an implementation file for CMCRTextViewerApp() class.
//			Where the CMCRTextViewerApp() class has been derived from COleControlModule().
//			The code written in this file makes it possible to implement the various
//			application methods in the control.
// NOTES:	
//
// AUTHORS:	
//
//===========================================================================
// MCRTextViewer.cpp : Implementation of CMCRTextViewerApp and DLL registration.

#include "stdafx.h"
#include "MCRTextViewer.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif


CMCRTextViewerApp NEAR theApp;

const GUID CDECL BASED_CODE _tlid =
		{ 0x8e5f747c, 0x4028, 0x4237, { 0x9a, 0x1f, 0xa, 0x1f, 0xb, 0x5c, 0xde, 0x8 } };
const WORD _wVerMajor = 1;
const WORD _wVerMinor = 0;


////////////////////////////////////////////////////////////////////////////
// CMCRTextViewerApp::InitInstance - DLL initialization

BOOL CMCRTextViewerApp::InitInstance()
{
	BOOL bInit = COleControlModule::InitInstance();

	if (bInit)
	{
		// TODO: Add your own module initialization code here.
	}

	// Check to see if OLE has already been initialized
	_AFX_THREAD_STATE* pState = AfxGetThreadState();
	if (!pState->m_bNeedTerm)
	{
		if (!AfxOleInit())
		{
			AfxMessageBox("OLE load failed");
			return -1;
		}
	}

	// Allow loading of Text Finder control
	AfxEnableControlContainer();

	// Verify that rich edit control functionality is available
	if (LoadLibraryA( _T("RICHED20.DLL")) == NULL)
	{
		AfxMessageBox( "RICHED20.DLL Load Failed", MB_OK | MB_ICONEXCLAMATION );
		return -1;
	}

	return bInit;
}


////////////////////////////////////////////////////////////////////////////
// CMCRTextViewerApp::ExitInstance - DLL termination

int CMCRTextViewerApp::ExitInstance()
{
	// TODO: Add your own module termination code here.

	return COleControlModule::ExitInstance();
}


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
