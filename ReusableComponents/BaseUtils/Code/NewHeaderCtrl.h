#if !defined(AFX_NEWHEADERCTRL_H__99EB0481_4FA1_11D1_980A_004095E0DEFA__INCLUDED_)
#define AFX_NEWHEADERCTRL_H__99EB0481_4FA1_11D1_980A_004095E0DEFA__INCLUDED_

// NewHeaderCtrl.h : header file
//
// Demo project found at http://www.codeproject.com/treectrl/treelist.asp
// Originally written by David Lantsman (davidlan@inter.net.il)
// Copyright (c) 1999
//
// Some unneeded functionality has been removed including
//   - Image list
//   - Right-to-left display

#pragma once

#include <afxtempl.h>

/////////////////////////////////////////////////////////////////////////////
// CNewHeaderCtrl window

class CNewHeaderCtrl : public CHeaderCtrl
{
// Construction
public:
	CNewHeaderCtrl();

// Attributes
private:
	BOOL m_bAutofit;
	void Autofit(int nOverrideItemData = -1, int nOverrideWidth = 0);

// Operations
public:
	void DrawItem( LPDRAWITEMSTRUCT lpDrawItemStruct );
	void SetAutofit(bool bAutofit = true) { m_bAutofit = bAutofit; Autofit(); }

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CNewHeaderCtrl)
	//}}AFX_VIRTUAL

// Implementation
public:
	virtual ~CNewHeaderCtrl();

	// Generated message map functions
protected:
	//{{AFX_MSG(CNewHeaderCtrl)
	afx_msg void OnPaint();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Developer Studio will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_NEWHEADERCTRL_H__99EB0481_4FA1_11D1_980A_004095E0DEFA__INCLUDED_)
