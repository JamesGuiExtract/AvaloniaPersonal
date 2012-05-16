//===========================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	TransferDlg.h
//
// PURPOSE:	To provide user interface allowing Transfer of Feature Attributes.
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//===========================================================================

#if !defined(AFX_TRANSFERDLG_H__D901FD16_B1D9_4433_ABDB_C12119D415CB__INCLUDED_)
#define AFX_TRANSFERDLG_H__D901FD16_B1D9_4433_ABDB_C12119D415CB__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// TransferDlg.h : header file
//

#include "resource.h"

/////////////////////////////////////////////////////////////////////////////
// CTransferDlg dialog

class CTransferDlg : public CDialog
{
// Construction
public:
	//=============================================================================
	// PURPOSE: Creates a modal Transfer dialog
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	bOriginalDefined - false if no Original Attributes exist, implying 
	//				that user cannot transfer to Current.
	//				bCanReplaceOriginal - true if user allowed to transfer to 
	//				Original
	//				bCanReplaceCurrent - true if user allowed to transfer to 
	//				Current
	CTransferDlg(bool bOriginalDefined, bool bCanReplaceOriginal, 
		bool bCanReplaceCurrent, CWnd* pParent = NULL);   // standard constructor

	//=============================================================================
	// PURPOSE: Indicates if user chose to transfer from Current to Original
	// REQUIRE: Nothing
	// PROMISE: true if transfer to Original, false otherwise.
	// ARGS:	None.
	bool	isTransferToOriginal();

// Dialog Data
	//{{AFX_DATA(CTransferDlg)
	enum { IDD = IDD_TRANSFER };
		// NOTE: the ClassWizard will add data members here
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CTransferDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CTransferDlg)
	afx_msg void OnRadioCurrentasorig();
	afx_msg void OnRadioOrigascurrent();
	virtual BOOL OnInitDialog();
	virtual void OnOK();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	// Is an Original Feature defined?
	bool	m_bOriginalDefined;

	// Is user choosing to transfer from Current to Original (default = true)
	bool	m_bTransferToOriginal;

	// Is user allowed to replace Original Attributes with Current?
	bool	m_bCanReplaceOriginal;

	// Is user allowed to replace Current Attributes with Original?
	bool	m_bCanReplaceCurrent;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_TRANSFERDLG_H__D901FD16_B1D9_4433_ABDB_C12119D415CB__INCLUDED_)
