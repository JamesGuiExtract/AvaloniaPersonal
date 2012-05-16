//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	HistoryEdit.h
//
// PURPOSE:	A CEdit subclass that allows you to display a scrolling history 
//				of text entries.
//
// NOTES:	
//
// AUTHORS:	Duan Wang
//
//==================================================================================================

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

/////////////////////////////////////////////////////////////////////////////
// HistoryEdit window

class HistoryEdit : public CEdit
{
// Construction
public:
	HistoryEdit();

// Attributes
public:

// Operations
public:
  void  AppendString (CString str);
  BOOL  IsSelectable() { return m_bSelectable; }
  void  AllowSelection (BOOL bAllowSelect) { m_bSelectable = bAllowSelect; }

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(HistoryEdit)
	//}}AFX_VIRTUAL

// Implementation
public:
	virtual ~HistoryEdit();

	// Generated message map functions
protected:
	//{{AFX_MSG(HistoryEdit)
	afx_msg void OnSetFocus(CWnd* pOldWnd);
	afx_msg void OnContextMenu(CWnd* pWnd, CPoint point);
	afx_msg void OnCopy();
	afx_msg void OnClearAll();
	afx_msg void OnSelectAll();
	afx_msg void OnDeleteSelected();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

protected:
  BOOL  m_bSelectable;                          // flag: user can select text in control
};

/////////////////////////////////////////////////////////////////////////////
