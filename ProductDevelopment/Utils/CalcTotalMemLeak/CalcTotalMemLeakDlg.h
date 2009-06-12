// CalcTotalMemLeakDlg.h : header file
//

#if !defined(AFX_CALCTOTALMEMLEAKDLG_H__4E2DB424_1A6E_43B0_AD4D_89741C6DE04E__INCLUDED_)
#define AFX_CALCTOTALMEMLEAKDLG_H__4E2DB424_1A6E_43B0_AD4D_89741C6DE04E__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

/////////////////////////////////////////////////////////////////////////////
// CCalcTotalMemLeakDlg dialog

class CCalcTotalMemLeakDlg : public CDialog
{
// Construction
public:
	CCalcTotalMemLeakDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	//{{AFX_DATA(CCalcTotalMemLeakDlg)
	enum { IDD = IDD_CALCTOTALMEMLEAK_DIALOG };
	CEdit	m_editInput;
	CString	m_zInput;
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CCalcTotalMemLeakDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	//{{AFX_MSG(CCalcTotalMemLeakDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	virtual void OnOK();
	afx_msg void OnButton2();
	afx_msg void OnButton1();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_CALCTOTALMEMLEAKDLG_H__4E2DB424_1A6E_43B0_AD4D_89741C6DE04E__INCLUDED_)
