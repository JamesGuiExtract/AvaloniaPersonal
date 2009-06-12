// RunRulesDlg.h : header file
//

#if !defined(AFX_RUNRULESDLG_H__5C3CC0EC_656A_40EE_899D_BF2907D7524B__INCLUDED_)
#define AFX_RUNRULESDLG_H__5C3CC0EC_656A_40EE_899D_BF2907D7524B__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

/////////////////////////////////////////////////////////////////////////////
// CRunRulesDlg dialog

class CRunRulesDlg : public CDialog
{
// Construction
public:
	CRunRulesDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	//{{AFX_DATA(CRunRulesDlg)
	enum { IDD = IDD_RUNRULES_DIALOG };
		// NOTE: the ClassWizard will add data members here
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CRunRulesDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	//{{AFX_MSG(CRunRulesDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_RUNRULESDLG_H__5C3CC0EC_656A_40EE_899D_BF2907D7524B__INCLUDED_)
