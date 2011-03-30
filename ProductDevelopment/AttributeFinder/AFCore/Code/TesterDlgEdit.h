// TesterDlgEdit.h : header file
//

#pragma once

#include "resource.h"

/////////////////////////////////////////////////////////////////////////////
// CTesterDlgEdit dialog

class CTesterDlgEdit : public CDialog
{
// Construction
public:
	CTesterDlgEdit(CString zName, CString zValue, CString zType, CWnd* pParent = __nullptr);

	// Provide modified strings back to caller
	CString	GetName();
	CString	GetValue();
	CString	GetType();

// Dialog Data
	//{{AFX_DATA(CTesterDlgEdit)
	enum { IDD = IDD_DLG_TESTER_EDIT };
	CString	m_zName;
	CString	m_zType;
	CString	m_zValue;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CTesterDlgEdit)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CTesterDlgEdit)
	virtual void OnOK();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
