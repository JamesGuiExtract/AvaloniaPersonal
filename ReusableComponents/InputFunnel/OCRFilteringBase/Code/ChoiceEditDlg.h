#pragma once

// ChoiceEditDlg.h : header file
//

/////////////////////////////////////////////////////////////////////////////
// ChoiceEditDlg dialog

#include "resource.h"



class ChoiceEditDlg : public CDialog
{
// Construction
public:
	ChoiceEditDlg(CWnd* pParent = NULL);   // standard constructor
	ChoiceEditDlg(CString cstrDes, CString cstrChars, CWnd* pParent = NULL);  

// Dialog Data
	//{{AFX_DATA(ChoiceEditDlg)
	enum { IDD = IDD_DLG_ChoiceEdit };
	CEdit	m_editDescription;
	CString	m_strChars;
	CString	m_strDescription;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(ChoiceEditDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(ChoiceEditDlg)
	virtual BOOL OnInitDialog();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
