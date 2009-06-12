#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// InputCorrectionDlg.h : header file
//

/////////////////////////////////////////////////////////////////////////////
// InputCorrectionDlg dialog

class InputCorrectionDlg : public CDialog
{
// Construction
public:
	InputCorrectionDlg(CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(InputCorrectionDlg)
	enum { IDD = IDD_DLG_CORRECT };
	CString	m_editCorrection;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(InputCorrectionDlg)
	public:
	virtual int DoModal();
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(InputCorrectionDlg)
		// NOTE: the ClassWizard will add member functions here
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
