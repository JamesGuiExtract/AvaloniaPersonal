//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	GenericDisplayFrame.h
//
// PURPOSE:	This is an header file for CGenericDisplayFrame class
//			where these have been derived from the CFrameWnd() 
//			class.  The code written in this file makes it possible for
//			initialize the Frame and its controls.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
#if !defined(AFX_GENERICDISPLAYFRAME_H__1498158A_9117_11D4_9725_008048FBC96E__INCLUDED_)
#define AFX_GENERICDISPLAYFRAME_H__1498158A_9117_11D4_9725_008048FBC96E__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// GenericDisplayFrame.h : header file
//
#include "GenericDisplayCtl.h"
#include "GenericDisplayView.h"
/////////////////////////////////////////////////////////////////////////////
// CGenericDisplayFrame frame

//==================================================================================================
//
// CLASS:	CGenericDisplayFrame
//
// PURPOSE:	This class is used to derive GenericDisplayFrame from MFC class CFrameWnd.
//			This derived control is used the show the controls and acts as 
//			user interface.
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

class CGenericDisplayFrame : public CFrameWnd
{
	DECLARE_DYNCREATE(CGenericDisplayFrame)
protected:
	// protected constructor used by dynamic creation
	CGenericDisplayFrame();           

	//	StatusBar object
	CStatusBar m_wndStatusBar;

// Attributes
public:

// Operations
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CGenericDisplayFrame)
	//}}AFX_VIRTUAL

	//--------------------------------------------------------
	// PURPOSE:	To create Status Bar.
	// REQUIRE: Frame
	// PROMISE: Creates the Status Bar
	BOOL CreateStatusBar();
	//--------------------------------------------------------
	// PURPOSE:	To handle the Overwrite key
	// REQUIRE: None
	// PROMISE: sets whether OVR is on or off
	void SetOVRStatus (UINT nKey);
	//--------------------------------------------------------
	// PURPOSE:	To set the status bar text
	// REQUIRE: None
	// PROMISE: None
	void statusText (int iPane, CString zStText);

	void createToolTips(void);

// Implementation
protected:

	virtual ~CGenericDisplayFrame();

	// Generated message map functions
	//{{AFX_MSG(CGenericDisplayFrame)
	afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);
	afx_msg void OnSize(UINT nType, int cx, int cy);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_GENERICDISPLAYFRAME_H__1498158A_9117_11D4_9725_008048FBC96E__INCLUDED_)
