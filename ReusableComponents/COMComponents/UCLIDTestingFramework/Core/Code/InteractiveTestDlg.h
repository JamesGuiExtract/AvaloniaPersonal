//{{AFX_INCLUDES()
#include "webbrowser2.h"
//}}AFX_INCLUDES
#pragma once
// InteractiveTestDlg.h : header file
//
#pragma warning (disable : 4786)


#include "TestCaseInfo.h"
#include <vector>

/////////////////////////////////////////////////////////////////////////////
// InteractiveTestDlg dialog

class InteractiveTestDlg : public CDialog
{
// Construction
public:
	InteractiveTestDlg(ITestResultLoggerPtr ipResultLogger, const char *pszITCFile, 
		CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(InteractiveTestDlg)
	enum { IDD = IDD_INTERACTIVE_TEST_DLG };
	CString	m_zDescription;
	CString	m_zTestCaseID;
	CWebBrowser2	m_browser;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(InteractiveTestDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(InteractiveTestDlg)
	afx_msg void OnAddException();
	afx_msg void OnAddNote();
	afx_msg void OnFail();
	afx_msg void OnPass();
	virtual BOOL OnInitDialog();
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnGetMinMaxInfo(MINMAXINFO* pMMI);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	/////////////
	// Variables
	/////////////
	ITestResultLoggerPtr m_ipResultLogger;
	std::string m_strITCFile;
	std::vector<TestCaseInfo> m_vecTestCaseInfo;
	unsigned long m_ulCurrentTestCase;

	// This is only used in OnSize to skip very first OnSize call when the dialog 
	// just created.
	bool m_bOnSizeInit;
	//////////
	// Methods
	//////////
	void processTestCaseCompletion(bool bResult);
	void setCurrentTestCase(unsigned long ulIndex);
	void processInputFile(const char *pszITCFile);

	// move button as the dialog is resizing
	void moveButtonControl(UINT nControlID, int nMoveXUnits, int nMoveYUnits);
	// resize edit box as the dialog is resizing
	// nX -- strentch or compress nX units horizontally
	void resizeEditControl(UINT nControlID, int nXUnits);
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

