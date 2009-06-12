// LicenseTimeInfoDlg.h : header file
//
#pragma once

/////////////////////////////////////////////////////////////////////////////
// CLicenseTimeInfoDlg dialog

#include <ByteStreamManipulator.hpp>
//#include <Win32Semaphore.hpp>
#include <IConfigurationSettingsPersistenceMgr.h>
#include <auto_ptr2.h>

#include <string>

class CLicenseTimeInfoDlg : public CDialog
{
// Construction
public:
	CLicenseTimeInfoDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	//{{AFX_DATA(CLicenseTimeInfoDlg)
	enum { IDD = IDD_LICENSETIMEINFO_DIALOG };
	CString	m_zFile1;
	CString	m_zFile2;
	CString	m_zRegistry1;
	CString	m_zRegistry2;
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CLicenseTimeInfoDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	//{{AFX_MSG(CLicenseTimeInfoDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnButtonRead();
	afx_msg void OnButtonClipboard();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	// Decrypts specified Date-Time string using specified byte stream password
	// Results are provided in specified CTime object
	// Returns true if decryption was sucessful, false otherwise
	bool decryptDateTimeString(std::string strEncryptedDT, const ByteStream& bsPassword, 
		CTime* ptmResult);

	// Retrieve fully qualified path to Date-Time file.
	// Presence or absence of file is not tested
	std::string getDateTimeFilePath() const;

	// Retrieve encrypted string from Date-Time file and registry items.
	// Returns empty string if Date-Time item is not found.
	std::string getLocalDateTimeString(std::string strFileName) const;
	std::string getRemoteDateTimeString(std::string strPath, std::string strKey) const;

	// Retrieves encryption/decryption passwords
	// 1: for Date-Time file
	// 2: for Date-Time registry key
	const ByteStream& getPassword1() const;
	const ByteStream& getPassword2() const;

	// Semaphore to protect reading and writing of licensing items
	CSemaphore m_semReadWrite;

	// Handles Date-Time Registry items
	std::auto_ptr<IConfigurationSettingsPersistenceMgr> ma_pRollbackCfgMgr;

	// Registry keys for information persistence
	static const std::string ITEM_SECTION_NAME1;
	static const std::string ITEM_SECTION_NAME2;
	static const std::string LAST_TIME_USED;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
