//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	GenericDisplay.h
//
// PURPOSE:	This is an header file for CGenericDisplayApp class
//			where this has been derived from the COleControlModule()
//			class.  The code written in this file makes it possible for
//			initialize the Frame and useful to interact between the controls.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
#if !defined(AFX_GENERICDISPLAY_H__1498157C_9117_11D4_9725_008048FBC96E__INCLUDED_)
#define AFX_GENERICDISPLAY_H__1498157C_9117_11D4_9725_008048FBC96E__INCLUDED_

// GenericDisplay.h : main header file for GENERICDISPLAY.DLL

#if !defined( __AFXCTL_H__ )
	#error include 'afxctl.h' before including this file
#endif

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CGenericDisplayApp : See GenericDisplay.cpp for implementation.
//==================================================================================================
//
// CLASS:	CGenericDisplayApp
//
// PURPOSE:	This class is used to derive GenericDisplayApp from MFC class CGenericDisplayApp.
//			This class is created by the software itself when created the Frame.
// REQUIRE:	Nothing
// 
// INVARIANTS:
//			
// EXTENSIONS:	Nothing
//			
//
// NOTES:	
//
//==================================================================================================

class CGenericDisplayApp : public COleControlModule
{
public:
	BOOL InitInstance();
	int ExitInstance();
};

extern const GUID CDECL _tlid;
extern const WORD _wVerMajor;
extern const WORD _wVerMinor;

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_GENERICDISPLAY_H__1498157C_9117_11D4_9725_008048FBC96E__INCLUDED)
