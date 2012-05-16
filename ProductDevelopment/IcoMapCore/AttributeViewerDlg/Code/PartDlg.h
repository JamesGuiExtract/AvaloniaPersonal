//===========================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	PartDlg.h
//
// PURPOSE:	To provide user interface allowing View/Edit of a new or an 
//				existing Starting Point (Part).
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//===========================================================================

#if !defined(AFX_PARTDLG_H__FFA518E1_AC50_486B_B312_F5413BAC2084__INCLUDED_)
#define AFX_PARTDLG_H__FFA518E1_AC50_486B_B312_F5413BAC2084__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// PartDlg.h : header file
//

#include "resource.h"

/////////////////////////////////////////////////////////////////////////////
// CPartDlg dialog

class CPartDlg : public CDialog
{
// Construction
public:
	//=============================================================================
	// PURPOSE: Creates a modal Line dialog
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	pszX - Input X-value string.
	//				pszY - Input Y-value string.
	//				bReadOnly - true if user only allowed to View data.
	//				bNew - true if caption should indicate this is a new part.
	CPartDlg(LPCTSTR pszX, LPCTSTR pszY, bool bReadOnly, bool bNew, 
		CWnd* pParent = NULL);   // standard constructor

	//=============================================================================
	// PURPOSE: Provides validated output X-value string
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	LPCTSTR	getX();

	//=============================================================================
	// PURPOSE: Provides validated output Y-value string
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	LPCTSTR	getY();

// Dialog Data
	//{{AFX_DATA(CPartDlg)
	enum { IDD = IDD_VIEWEDIT_PART };
	CString	m_zXString;
	CString	m_zYString;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CPartDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	// Is this dialog providing read-only data (View)
	bool	m_bReadOnly;

	// Is this dialog representing a new part?
	bool	m_bNew;

	// Generated message map functions
	//{{AFX_MSG(CPartDlg)
	virtual void OnOK();
	virtual BOOL OnInitDialog();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_PARTDLG_H__FFA518E1_AC50_486B_B312_F5413BAC2084__INCLUDED_)
