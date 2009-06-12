// TLFrame.h : header file
//
// Demo project found at http://www.codeproject.com/treectrl/treelist.asp
// Originally written by David Lantsman (davidlan@inter.net.il)
// Copyright (c) 1999
//
// Some unneeded functionality has been removed including
//   - Sorting

#pragma once

#include "NewTreeListCtrl.h"

/////////////////////////////////////////////////////////////////////////////
// CTLFrame window

class EXPORT_BaseUtils CTLFrame : public CWnd
{
// Construction
public:
	CTLFrame();

// Attributes
public:
	CFont				m_headerFont;
	CNewTreeListCtrl	m_tree;
	CScrollBar			m_horScrollBar;

// Operations
private:
	static LONG FAR PASCAL DummyWndProc(HWND, UINT, WPARAM, LPARAM);
	void Initialize();

	bool	m_bInitialized;

public:
	static void RegisterClass();
	BOOL SubclassDlgItem(UINT nID, CWnd* parent); // use in CDialog/CFormView

	void ResetScrollBar();

	BOOL VerticalScrollVisible();
	BOOL HorizontalScrollVisible();

	int StretchWidth(int m_nWidth, int m_nMeasure);

	// Added these methods to facilitate resizing of dialog
	void	DoResize();
	void	SetStretchColumn(int iColumn);

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CTLFrame)
	protected:
	virtual BOOL OnNotify(WPARAM wParam, LPARAM lParam, LRESULT* pResult);
	//}}AFX_VIRTUAL

// Implementation
public:
	virtual ~CTLFrame();

	// Generated message map functions
public:
	afx_msg void OnHScroll(UINT nSBCode, UINT nPos, CScrollBar* pScrollBar);
protected:
	//{{AFX_MSG(CTLFrame)
	afx_msg void OnContextMenu(CWnd* pWnd, CPoint point);
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnMove(int x, int y);
	afx_msg void OnLButtonDblClk(UINT nFlags, CPoint point);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	int		m_iStretchColumn;
	static bool ms_bIsRegistered;
};

/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Developer Studio will insert additional declarations immediately before the previous line.
