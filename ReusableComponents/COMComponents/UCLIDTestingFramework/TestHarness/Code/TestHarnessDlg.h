// TestHarnessDlg.h : header file
//

#pragma once

#include <memory>
class IConfigurationSettingsPersistenceMgr;

/////////////////////////////////////////////////////////////////////////////
// CTestHarnessDlg dialog

class CTestHarnessDlg : public CDialog
{
// Construction
public:
	CTestHarnessDlg(CWnd* pParent = NULL);	// standard constructor
	~CTestHarnessDlg();

// Dialog Data
	//{{AFX_DATA(CTestHarnessDlg)
	enum { IDD = IDD_TESTHARNESS_DIALOG };
	CButton	m_btnBrowse;
	CButton	m_radioTCL;
	CButton	m_radioITC;
	CButton	m_btnNotepad;
	CButton	m_btnInteractiveTests;
	CButton	m_btnAutomatedTests;
	CButton	m_btnAllTests;
	CString	m_zFilename;
	int		m_nFileType;
	CString	m_zTestFolder;
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CTestHarnessDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support

	virtual LRESULT DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam);
	//}}AFX_VIRTUAL

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	//{{AFX_MSG(CTestHarnessDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnBtnAllTests();
	afx_msg void OnBtnAutomatedTests();
	afx_msg void OnBtnBrowse();
	afx_msg void OnBtnInteractiveTests();
	afx_msg void OnChangeEditFilename();
	afx_msg void OnRadioTcl();
	afx_msg void OnRadioItc();
	afx_msg void OnBtnOpenNotepad();
	afx_msg void OnOK();
	afx_msg void OnCancel();
	afx_msg void OnClose();
	afx_msg void OnTimer(UINT nIDEvent);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()


private:
	ITestHarnessPtr m_ipTestHarness;
	std::unique_ptr<IConfigurationSettingsPersistenceMgr> m_apCfgMgr;

	CWinThread* m_pThread;

	bool m_bRunning;

	// member function that does the thread work
	void runAutoTests();
	// Thread Function, just calls the member function
	static UINT runAutoTests(LPVOID pParam);

	// member function that does the thread work
	void runInteractiveTests();
	// Thread Function, just calls the member function
	static UINT runInteractiveTests(LPVOID pParam);

	// member function that does the thread work
	void runAllTests();
	// Thread Function, just calls the member function
	static UINT runAllTests(LPVOID pParam);

	// Methods
	void runAutomatedTests();

	void disableUI();
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
