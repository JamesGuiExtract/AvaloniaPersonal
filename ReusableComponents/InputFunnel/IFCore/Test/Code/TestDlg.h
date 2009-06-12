// TestDlg.h : header file
//
//{{AFX_INCLUDES()
#include "inputmanager.h"
//}}AFX_INCLUDES

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include <map>

/////////////////////////////////////////////////////////////////////////////
// CTestDlg dialog

class CTestDlg : public CDialog
{
// Construction
public:
	CTestDlg(CWnd* pParent = NULL);	// standard constructor
	~CTestDlg();

// Dialog Data
	//{{AFX_DATA(CTestDlg)
	enum { IDD = IDD_TEST_DIALOG };
	CInputManager	m_ctrlInputManager;
	CString	m_editOutput;
	int		m_nValidator;
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CTestDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	//{{AFX_MSG(CTestDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnBtnShow();
	afx_msg void OnOnInputReceivedInputmanager1(LPDISPATCH pTextInput);
	afx_msg void OnClose();
	afx_msg void OnBtnCloseAllir();
	afx_msg void OnRadioNumberValidator();
	afx_msg void OnRadioTextValidator();
	afx_msg void OnBtnAppCreate();
	afx_msg void OnBtnAppDestroy();
	afx_msg void OnDestroy();
	DECLARE_EVENTSINK_MAP()
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	IInputValidatorPtr m_ipTextInputValidator;
	std::map<long, IInputReceiver*> m_mapIDToNumInputReceiver;

	// enable input
	void enableInput(bool bEnable = true);
	IInputValidatorPtr getTextInputValidator();

};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

