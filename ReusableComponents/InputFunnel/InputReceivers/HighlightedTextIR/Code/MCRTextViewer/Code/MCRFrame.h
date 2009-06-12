//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	MCRFrame.h
//
// PURPOSE:	This is an header file for CMCRFrame and CMCRTextToolBar classes
//			where these have been derived from the CFrameWnd() and 
//			CToolBar() classes.  The code written in this file makes it possible for
//			initialize the Frame and ToolBar and their controls.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
#if !defined(AFX_MCRFRAME_H__7F4F91B5_62B9_11D4_96EE_008048FBC96E__INCLUDED_)
#define AFX_MCRFRAME_H__7F4F91B5_62B9_11D4_96EE_008048FBC96E__INCLUDED_

// MCRFrame.h : header file
//
/////////////////////////////////////////////////////////////////////////////
// CMCRFrame frame
//==================================================================================================
//
// CLASS:	CMCRFrame
//
// PURPOSE:	This class is used to derive MCRFrame from MFC class CFrameWnd.
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
class CMCRFrame : public CFrameWnd
{
	DECLARE_DYNCREATE(CMCRFrame)
protected:
	CMCRFrame();           // protected constructor used by dynamic creation


// Attributes
public:

// Operations
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CMCRFrame)
	public:
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CMCRFrame)
	afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_MCRFRAME_H__7F4F91B5_62B9_11D4_96EE_008048FBC96E__INCLUDED_)
