//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	MCRTextViewer.h
//
// PURPOSE:	This is an header file for CMCRTextViewerApp class
//			where this has been derived from the COleControlModule()
//			class.  The code written in this file makes it possible for
//			initialize the Frame and useful to interact between the controls.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
#if !defined(AFX_UCLIDMCRTEXTVIEWER_H__248E93C1_1371_4E02_A06E_F1E15CFA9BCB__INCLUDED_)
#define AFX_UCLIDMCRTEXTVIEWER_H__248E93C1_1371_4E02_A06E_F1E15CFA9BCB__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

// MCRTextViewer.h : main header file for MCRTEXTVIEWER.DLL

#if !defined( __AFXCTL_H__ )
	#error include 'afxctl.h' before including this file
#endif

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CMCRTextViewerApp : See MCRTextViewer.cpp for implementation.

//===========================================================================
//
// CLASS:	CMCRTextViewerApp
//
// PURPOSE:	This class is used to derive MCRTextViewerApp from MFC class 
//			COleControlModule.  This class is created by the software itself 
//			when creating the Frame.
// REQUIRE:	Nothing
// 
// INVARIANTS:
//			
// EXTENSIONS:	Nothing
//			
//
// NOTES:	
//
//===========================================================================
class CMCRTextViewerApp : public COleControlModule
{
public:

	//-----------------------------------------------------------------------
	// PURPOSE: To handle initiating the instance of the control
	// REQUIRE: Nothing
	// PROMISE: Nothing
	BOOL InitInstance();

	//-----------------------------------------------------------------------
	// PURPOSE: To handle exiting the instance of the control
	// REQUIRE: Nothing
	// PROMISE: Nothing
	int ExitInstance();
};

extern const GUID CDECL _tlid;
extern const WORD _wVerMajor;
extern const WORD _wVerMinor;

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_UCLIDMCRTEXTVIEWER_H__248E93C1_1371_4E02_A06E_F1E15CFA9BCB__INCLUDED)
