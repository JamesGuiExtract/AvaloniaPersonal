// UCLIDUtilAppsDlg.h : header file
//

#if !defined(AFX_UCLIDUTILAPPSDLG_H__A951EF60_40EE_43AE_ACCE_52AA1EA5369A__INCLUDED_)
#define AFX_UCLIDUTILAPPSDLG_H__A951EF60_40EE_43AE_ACCE_52AA1EA5369A__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

/////////////////////////////////////////////////////////////////////////////
// CUCLIDUtilAppsDlg dialog

class CUCLIDUtilAppsDlg : public CDialog
{
// Construction
public:
	CUCLIDUtilAppsDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	//{{AFX_DATA(CUCLIDUtilAppsDlg)
	enum { IDD = IDD_UCLIDUTILAPPS_DIALOG };
		// NOTE: the ClassWizard will add data members here
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CUCLIDUtilAppsDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	//{{AFX_MSG(CUCLIDUtilAppsDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_UCLIDUTILAPPSDLG_H__A951EF60_40EE_43AE_ACCE_52AA1EA5369A__INCLUDED_)
