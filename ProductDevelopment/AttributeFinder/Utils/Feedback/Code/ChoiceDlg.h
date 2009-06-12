// ChoiceDlg.h : header file
//

#pragma once

/////////////////////////////////////////////////////////////////////////////
// CChoiceDlg dialog

class CChoiceDlg : public CDialog
{
// Construction
public:
	CChoiceDlg(IFeedbackMgrInternalsPtr ipFBMgr, CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(CChoiceDlg)
	enum { IDD = IDD_DLG_CHOICE };
		// NOTE: the ClassWizard will add data members here
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CChoiceDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CChoiceDlg)
	afx_msg void OnBtnConfigure();
	afx_msg void OnBtnPackage();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	IFeedbackMgrInternalsPtr m_ipFBMgr;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
