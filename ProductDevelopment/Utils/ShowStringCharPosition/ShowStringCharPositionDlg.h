// ShowStringCharPositionDlg.h : header file
//

#if !defined(AFX_SHOWSTRINGCHARPOSITIONDLG_H__E988FEE6_4A56_45BA_A922_BE7605D6102A__INCLUDED_)
#define AFX_SHOWSTRINGCHARPOSITIONDLG_H__E988FEE6_4A56_45BA_A922_BE7605D6102A__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

/////////////////////////////////////////////////////////////////////////////
// CShowStringCharPositionDlg dialog

class CShowStringCharPositionDlg : public CDialog
{
// Construction
public:
	CShowStringCharPositionDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	//{{AFX_DATA(CShowStringCharPositionDlg)
	enum { IDD = IDD_SHOWSTRINGCHARPOSITION_DIALOG };
	CString	m_zInput;
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CShowStringCharPositionDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	//{{AFX_MSG(CShowStringCharPositionDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	virtual void OnOK();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_SHOWSTRINGCHARPOSITIONDLG_H__E988FEE6_4A56_45BA_A922_BE7605D6102A__INCLUDED_)
