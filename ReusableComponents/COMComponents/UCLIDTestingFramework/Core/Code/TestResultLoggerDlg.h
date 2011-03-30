#if !defined(AFX_TESTRESULTLOGGERDLG_H__7582634F_2862_4696_8159_933EB9C84360__INCLUDED_)
#define AFX_TESTRESULTLOGGERDLG_H__7582634F_2862_4696_8159_933EB9C84360__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// TestResultLoggerDlg.h : header file
//

/////////////////////////////////////////////////////////////////////////////
// TestResultLoggerDlg dialog

#include <string>
#include <map>
#include <fstream>
#include <memory>
#include <RegistryPersistenceMgr.h>

using namespace std;

class TestResultLoggerDlg : public CDialog
{
// Construction
public:
	// PROMISE: if pOutputFile != __nullptr, then component and testharness
	// level statistics will be written to the specified file using
	// appropriate XML tags.
	TestResultLoggerDlg(CWnd* pParent);
	~TestResultLoggerDlg();

// Dialog Data
	//{{AFX_DATA(TestResultLoggerDlg)
	enum { IDD = IDD_TEST_RESULT_LOGGER_DLG };
	CEdit	m_editNote;
	CEdit	m_editNoteRight;
	CTreeCtrl m_tree;
	CString	m_zNote;
	CString m_zNoteRight;
	//}}AFX_DATA

	void startTestHarness(const string& strHarnessDescription);
	// strOutputFileName - fully qualified output file name for recording 
	// current component test
	void startComponentTest(const string& strComponentDescription);
	void startTestCase(const string& strTestCaseID, 
		const string& strTestCaseDescription, ETestCaseType eTestCaseType);
	void addTestCaseNote(const string& strTestCaseNote);
	void addTestCaseDetailNote(const string& strTitle, const string& strTestDetailCaseNote);
	void addTestCaseMemo(const string& strTitle, const string& strTestCaseMemo);
	void addTestCaseFile(const string& strFileName);
	void addTestCaseException(const string& strTestCaseException);

	//=======================================================================
	// PURPOSE: Outputs expected and found data to the appropriate editboxes
	// REQUIRE: 5 strings
	// PROMISE: Populates the edit boxes with the strings' contents.
	// ARGS:	strTitle:  The title of the Item in the output tree.
	//			strLabel1: The label put in the left side edit box
	//			strInput1: The input following the label in the left edit box
	//			strLabel2: The label put in the right side edit box
	//			strInput2: The input following the label in the right edit box
	void TestResultLoggerDlg::addTestCaseCompareData(const string& strTitle, const string& strLabel1, 
			const string& strInput1, const string& strLabel2, const string& strInput2);

	void endTestCase(bool bResult);
	void endComponentTest(bool bShowComponentLevelSummary = true);
	void endTestHarness();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(TestResultLoggerDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(TestResultLoggerDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnDblclkTree(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnGetMinMaxInfo(MINMAXINFO* pMMI);
	afx_msg void OnSelchangedTree(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnClose();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:

	////////////
	// Variables
	////////////
	// bitmap objects for the various tree view icons
	unique_ptr<CBitmap> m_apBmpHarness, m_apBmpComponent, m_apBmpTestCaseDoneGood,
		m_apBmpTestCaseDoneBad, m_apBmpTestCaseDoneOtherGood,
		m_apBmpTestCaseDoneOtherBad, m_apBmpTestCaseInProgress, 
		m_apBmpNote, m_apBmpDetailNote, m_apBmpMemo, m_apBmpFile, 
		m_apBmpException;

	// the image list and indexes of the various images
	unique_ptr<CImageList> m_apImageList;
	int m_iBitmapTestCaseDoneGood, m_iBitmapTestCaseDoneBad, 
		m_iBitmapTestCaseDoneOtherGood, m_iBitmapTestCaseDoneOtherBad,
		m_iBitmapTestCaseInProgress, m_iBitmapComponent, m_iBitmapHarness, 
		m_iBitmapNote, m_iBitmapDetailNote, m_iBitmapMemo, 
		m_iBitmapFile, m_iBitmapException;

	// handles to various main tree items
	HTREEITEM m_hCurrentComponent;
	HTREEITEM m_hCurrentTestCase;
	HTREEITEM m_hCurrentHarness;

	// map of tree items (of exception type) to exception string
	map<HTREEITEM, string> m_mapHandleToExceptionData;

	// map of tree items (of detail-note type) to detail string
	map<HTREEITEM, string> m_mapHandleToDetailNote;

	// map of tree items (of memo type) to memo string
	map<HTREEITEM, string> m_mapHandleToMemo;

	// map of tree items (of file type) to filename string
	map<HTREEITEM, string> m_mapHandleToFile;

	// map of tree items (of comparedata type) to compare data string
	map<HTREEITEM, string> m_mapHandleToCompareData;

	// booleans to keep track of which kind of test is currently running
	bool m_bTestHarnessActive;
	bool m_bComponentTestActive;
	bool m_bTestCaseActive;
	ETestCaseType m_eCurrentTestCaseType;

	// For checking Scoll Logger flag	
	unique_ptr<IConfigurationSettingsPersistenceMgr> m_apCfgMgr;

	// Used to indicate the failed case should be collapsed when endTestCase is called
	//		set to true in addTestCaseFile if file is .nte and exists
	//		set to false in endTestCase
	bool m_bCollapseOnFail;

	//////////
	// Methods
	//////////

	// return true if the registry setting is configured to auto-scroll
	bool autoScrollEnabled() const;

	// whether or not to retain each test case node to be displayed in the logger
	bool retainTestCaseNodes() const;

	// Get the string for the command line to use to diff the files from the registry. The user can specify
	// any program they want to use via RDTConfig.
	string getDiffString();

	// Compares the given string via some user specified command line diff utility
	// Default is to use kdiff3 installed in the default location.
	// arg: One string, with 2 values delimited with ^|&$
	void openCompareDiff(const string& strSource);
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_TESTRESULTLOGGERDLG_H__7582634F_2862_4696_8159_933EB9C84360__INCLUDED_)
