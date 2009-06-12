// TestLicensingDlg.h : header file
//

#pragma once

#include <string>

/////////////////////////////////////////////////////////////////////////////
// CTestLicensingDlg dialog

class CTestLicensingDlg : public CDialog
{
// Construction
public:
	CTestLicensingDlg(bool bShowMessages, CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	//{{AFX_DATA(CTestLicensingDlg)
	enum { IDD = IDD_TESTLICENSING_DIALOG };
	CButton	m_btnRead;
	CButton	m_btnWrite;
	CString	m_zUser;
	CString	m_zComputer;
	CString	m_zCode;
	CString	m_zCount;
	CString	m_zFile;
	CString	m_zKey;
	CString	m_zCountValue;
	CString	m_zCountFound;
	CString	m_zSummary;
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CTestLicensingDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	//{{AFX_MSG(CTestLicensingDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnBtnTest();
	afx_msg void OnBtnTestPresence();
	afx_msg void OnBtnTestRead();
	afx_msg void OnBtnTestWrite();
	virtual void OnCancel();
	afx_msg void OnRadioV1();
	afx_msg void OnRadioV2();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	///////
	// Data
	///////

	// Indicates that Message Boxes should be displayed to the user when 
	// the error count is incremented
	bool		m_bShowMessages;

	// Indicates that directory or file to hold Date-Time file was created during testing 
	// and therefore should be deleted at exit
	bool		m_bCreatedDirectory;
	bool		m_bCreatedDTFile;

	// Indicates that specified registry keys were created during testing 
	// and therefore should be deleted at exit
	bool		m_bCreatedDTKey1;
	bool		m_bCreatedDTKey2;
	bool		m_bCreatedCountKey1;
	bool		m_bCreatedCountKey2;

	// Indicates which Unlock registry key was created during testing 
	// and therefore should be deleted at exit (0 --> none)
	int			m_iCreatedCodeKey1;
	int			m_iCreatedCodeKey2;

	// Indicates how many unexpected results have been seen so far (0 --> none)
	int			m_iTotalErrorCount;

	// Indicates that initial registry and file locations should be tested
	// otherwise test the alternate registry locations
	bool		m_bVersion1;

	//////////
	// Methods
	//////////

	// Clears test results except for User and Computer
	void		clearFields();

	// Creates the specified key for testing
	bool		createKey(std::string strKey, std::string strSubKey, std::string strValue);

	// Deletes the specified key
	//    Used for removing a subkey that was created during testing
	//    Also will delete an entire key created during testing if bDeleteWholeKey == true
	void		deleteKey(std::string strKey, std::string strSubKey, bool bDeleteWholeKey);

	// Retrieves name of current user
	std::string	getCurrentUserName();

	// Retrieves name of computer
	std::string	getComputerName();

	// Presence or absence of file is not tested
	std::string getDateTimeFilePath();

	// Returns directory from fully qualified path
	std::string getDirectoryFromFullPath(std::string strFullFileName);

	// Tests presence/absence and read/write permissions of Date-Time file
	std::string	testFilePresence();
	std::string	testFileRead();
	std::string	testFileWrite();

	// Tests access permissions of registry items
	bool	testRegistryPresence(std::string strKey, std::string strSubKey);
	bool	testRegistryReadAccess(std::string strKey, std::string strSubKey);
	bool	testRegistryWriteAccess(std::string strKey, std::string strSubKey);
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
