// ChangeWindowTitleDlg.h : header file
//

#if !defined(AFX_CHANGEWINDOWTITLEDLG_H__61E18A24_79B2_4447_B39E_86BD1E0E5D24__INCLUDED_)
#define AFX_CHANGEWINDOWTITLEDLG_H__61E18A24_79B2_4447_B39E_86BD1E0E5D24__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

/////////////////////////////////////////////////////////////////////////////
// CChangeWindowTitleDlg dialog

class CChangeWindowTitleDlg : public CDialog
{
// Construction
public:
	CChangeWindowTitleDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	//{{AFX_DATA(CChangeWindowTitleDlg)
	enum { IDD = IDD_CHANGEWINDOWTITLE_DIALOG };
	CString	m_EditTextFrom;
	CString	m_EditTextTo;
	CString	m_EditTextFind;
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CChangeWindowTitleDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	HICON m_hIcon;
	void saveSettings();

	// Generated message map functions
	//{{AFX_MSG(CChangeWindowTitleDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnClose();
	virtual void OnOK();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_CHANGEWINDOWTITLEDLG_H__61E18A24_79B2_4447_B39E_86BD1E0E5D24__INCLUDED_)
