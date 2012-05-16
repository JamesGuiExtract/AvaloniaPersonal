#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	CurveWizardResetDlg.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//
//==================================================================================================
#include "resource.h"

class CurveWizardResetDlg : public CDialog
{
// Construction
public:
	CurveWizardResetDlg(CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(CurveWizardResetDlg)
	enum { IDD = IDD_DLG_CurveWizardReset };
	CButton	m_btnCurve7;
	CButton	m_btnCurve8;
	CButton	m_btnCurve6;
	CButton	m_btnCurve4;
	CButton	m_btnCurve5;
	CButton	m_btnCurve3;
	CButton	m_btnCurve2;
	CButton	m_btnCurve1;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CurveWizardResetDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CurveWizardResetDlg)
	virtual void OnCancel();
	virtual void OnOK();
	virtual BOOL OnInitDialog();
	afx_msg void OnBTNSelectAll();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

	void configureButtons(void);

};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
