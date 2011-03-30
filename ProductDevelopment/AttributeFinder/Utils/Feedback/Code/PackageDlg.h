// PackageDlg.h : header file
//

#pragma once

#include "..\..\FeedbackManager\Code\PersistenceMgr.h"

/////////////////////////////////////////////////////////////////////////////
// CPackageDlg dialog

class CPackageDlg : public CDialog
{
// Construction
public:
	CPackageDlg(IFeedbackMgrInternalsPtr ipFBMgr, CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(CPackageDlg)
	enum { IDD = IDD_DLG_PACKAGE };
	BOOL	m_bClear;
	CString	m_zSize;
	CString	m_zFile;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CPackageDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CPackageDlg)
	afx_msg void OnBtnBrowse();
	virtual void OnOK();
	virtual BOOL OnInitDialog();
	afx_msg void OnBtnReadindex();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	///////
	// Data
	///////

	// Handles configuration persistence
	std::unique_ptr<IConfigurationSettingsPersistenceMgr> ma_pUserCfgMgr;
	std::unique_ptr<PersistenceMgr> ma_pCfgFeedbackMgr;

	// Read database only once
	bool	m_bReadDatabase;

	// Collection of files from Feedback database
	std::vector<std::string>	m_vecFBFiles;

	IFeedbackMgrInternalsPtr m_ipFBMgr;

	//////////
	// Methods
	//////////
	void	estimatePackageSize();

	// Reads Feedback database and builds vector of files to be packaged
	void	readDatabase();

	// Reads registry settings
	void	readRegistrySettings();

	// Writes registry settings
	void	writeRegistrySettings();

	// Zips specified files into Feedback package
	void	zipCollectedFiles();
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
