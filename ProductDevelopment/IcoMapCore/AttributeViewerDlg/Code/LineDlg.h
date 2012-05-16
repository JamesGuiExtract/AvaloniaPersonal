//===========================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	LineDlg.h
//
// PURPOSE:	To provide user interface allowing View/Edit of a new or an 
//				existing Line segment.
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//===========================================================================

#pragma once
// LineDlg.h : header file
//

#include "resource.h"

/////////////////////////////////////////////////////////////////////////////
// CLineDlg dialog

class CLineDlg : public CDialog
{
// Construction
public:
	//=============================================================================
	// PURPOSE: Creates a modal Line dialog
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	pszBearing - Input Bearing string.
	//				pszDistance - Input Distance string.
	//				bReadOnly - true if user only allowed to View data.
	//				bNew - true if caption should indicate this is a new line.
	CLineDlg(LPCTSTR pszBearing, LPCTSTR pszDistance, bool bReadOnly, 
		bool bNew, CWnd* pParent = NULL);   // standard constructor

	//=============================================================================
	// PURPOSE: Provides validated output Bearing string
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	LPCTSTR getBearing();

	//=============================================================================
	// PURPOSE: Provides validated output Distance string
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	LPCTSTR getDistance();

// Dialog Data
	//{{AFX_DATA(CLineDlg)
	enum { IDD = IDD_VIEWEDIT_LINE };
	CString	m_zBearing;
	CString	m_zDistance;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CLineDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	// Is this dialog providing read-only data (View)
	bool	m_bReadOnly;

	// Is this dialog representing a new line?
	bool	m_bNew;

	// Generated message map functions
	//{{AFX_MSG(CLineDlg)
	virtual BOOL OnInitDialog();
	virtual void OnOK();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

