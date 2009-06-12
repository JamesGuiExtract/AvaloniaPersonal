// TestHighlightWindowDlg.h : header file
//

#if !defined(AFX_TESTHIGHLIGHTWINDOWDLG_H__EA922EBA_B58A_470D_BD70_E5CB86DEC1E1__INCLUDED_)
#define AFX_TESTHIGHLIGHTWINDOWDLG_H__EA922EBA_B58A_470D_BD70_E5CB86DEC1E1__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

/////////////////////////////////////////////////////////////////////////////
// CTestHighlightWindowDlg dialog

class CTestHighlightWindowDlg : public CDialog
{
// Construction
public:
	CTestHighlightWindowDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	//{{AFX_DATA(CTestHighlightWindowDlg)
	enum { IDD = IDD_TESTHIGHLIGHTWINDOW_DIALOG };
	int		m_iShowTransparentWindow;
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CTestHighlightWindowDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	//{{AFX_MSG(CTestHighlightWindowDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnButton1();
	afx_msg void OnButton2();
	afx_msg void OnButton3();
	afx_msg void OnButton4();
	afx_msg void OnRadioNo();
	afx_msg void OnRadioYes();
	afx_msg void OnButton5();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_TESTHIGHLIGHTWINDOWDLG_H__EA922EBA_B58A_470D_BD70_E5CB86DEC1E1__INCLUDED_)
