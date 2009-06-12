//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	MCRView.h
//
// PURPOSE:	This is an header file for CMCRView class
//			where this has been derived from the CRichEditView()
//			class.  The code written in this file makes it possible for
//			initialize to set the view.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
#if !defined(AFX_MCRVIEW_H__7F4F91B3_62B9_11D4_96EE_008048FBC96E__INCLUDED_)
#define AFX_MCRVIEW_H__7F4F91B3_62B9_11D4_96EE_008048FBC96E__INCLUDED_

// MCRView.h : header file
//

/////////////////////////////////////////////////////////////////////////////
// CMCRView view
//===========================================================================
//
// CLASS:	CMCRView
//
// PURPOSE:	This class is used to derive a View from MFC class CRichEditView.
//			This derived view is attached to Frame.  Object of this class are  
//			created in MCRTextViewer.cpp.  It has implementation of functions 
//			to the text files.  This class is used to set link between the 
//			user and the control.
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
class CUCLIDMCRTextViewerCtrl;

class CMCRView : public CRichEditView 
{
protected:
	CMCRView();           // protected constructor used by dynamic creation
	DECLARE_DYNCREATE(CMCRView)

// Attributes
public:
	//	Handle to the view
	HWND m_hwndView;

// Operations
public:
	virtual ~CMCRView();

	//=============================================================================
	// PURPOSE: To set current related MCRTextViewerCtrl object
	// REQUIRE: Nothing
	// PROMISE: None
	// ARGS:	pMCRTextViewerCtrl : pointer to control
	void setMCRTextViewerCtrl(CUCLIDMCRTextViewerCtrl* pMCRTextViewerCtrl) {m_pMCRTextViewerCtrl = pMCRTextViewerCtrl;}

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CMCRView)
	protected:
	virtual BOOL PreCreateWindow(CREATESTRUCT& cs);
    virtual void OnInitialUpdate();
	//}}AFX_VIRTUAL

// Implementation
protected:
#ifdef _DEBUG
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif

	// Generated message map functions
protected:
	//{{AFX_MSG(CMCRView)
	afx_msg void OnMouseMove(UINT nFlags, CPoint point);
	afx_msg void OnLButtonDown(UINT nFlags, CPoint point);
	afx_msg BOOL OnSetCursor(CWnd* pWnd, UINT nHitTest, UINT message);
	afx_msg void OnRButtonDown(UINT nFlags, CPoint point);
	afx_msg void OnSendSelected();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

	bool	bHandCursor;

private:
	CUCLIDMCRTextViewerCtrl *m_pMCRTextViewerCtrl;
};

/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_MCRVIEW_H__7F4F91B3_62B9_11D4_96EE_008048FBC96E__INCLUDED_)
