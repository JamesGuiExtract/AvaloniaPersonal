//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ImageEdit.h
//
// PURPOSE:	This is an header file for CImageEditApp class
//			where this has been derived from the COleControlModule()
//			class.  The code written in this file makes it possible for
//			initialize the Frame and useful to interact between the controls.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
#if !defined(AFX_IMAGEEDIT_H__B5F48103_50C4_42C4_8216_F726872B94A6__INCLUDED_)
#define AFX_IMAGEEDIT_H__B5F48103_50C4_42C4_8216_F726872B94A6__INCLUDED_

// ImageEdit.h : main header file for IMAGEEDIT.DLL

#if !defined( __AFXCTL_H__ )
	#error include 'afxctl.h' before including this file
#endif

#include "resource.h"       // main symbols
/////////////////////////////////////////////////////////////////////////////
// CImageEditApp : See ImageEdit.cpp for implementation.
//==================================================================================================
//
// CLASS:	CImageEditApp
//
// PURPOSE:	This class is used to derive ImageEditApp from MFC class COleControlModule.
//			This class is created by the software itself when created the application.
// REQUIRE:	Nothing
// 
// INVARIANTS:
//			
// EXTENSIONS:	Nothing
//			
//
// NOTES:	
//
class CImageEditApp : public COleControlModule
{
public:
	BOOL InitInstance();
	int ExitInstance();
};
//==================================================================================================
extern const GUID CDECL _tlid;
extern const WORD _wVerMajor;
extern const WORD _wVerMinor;

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_IMAGEEDIT_H__B5F48103_50C4_42C4_8216_F726872B94A6__INCLUDED)
