//============================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	COMLicenseEvalDlg.h
//
// PURPOSE:	Declaration of COMLicenseGeneratorDlg class
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//============================================================================

#pragma once

class LMData;

#ifdef _DEBUG
#include <IConfigurationSettingsPersistenceMgr.h>
#endif

#include <string>
#include <memory>

/////////////////////////////////////////////////////////////////////////////
// CCOMLicenseEvalDlg dialog

class CCOMLicenseEvalDlg : public CDialog
{
// Construction
public:
	CCOMLicenseEvalDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	//{{AFX_DATA(CCOMLicenseEvalDlg)
	enum { IDD = IDD_COMLICENSEEVAL_DIALOG };
	CListCtrl	m_list;
	CButton	m_Paste;
	CButton	m_Evaluate;
	CString	m_zCode;
	CString	m_zDate;
	CString	m_zLicensee;
	CString	m_zOrganization;
	CString	m_zIssuer;
	CString	m_zIssueDate;
	CString	m_zFile;
	BOOL	m_bUseComputerName;
	BOOL	m_bUseSerialNumber;
	CString	m_zComputerName;
	CString	m_zSerialNumber;
	CString	m_zMACAddress;
	CString	m_zVersion;
	BOOL	m_bUseMACAddress;
	BOOL	m_bUseSpecialPasswords;
	int		m_iType;
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CCOMLicenseEvalDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	void doExtract();

	void doComponents(LMData* pData);

	void evaluateUnlockFile();

	//=======================================================================
	// PURPOSE: Provides default folder for license or unlock file.  If 
	//			SBL folder is not writable, returns local CommonComponents
	//			folder, if present.  In Debug mode, uses persistent folder 
	//			from the registry.
	// REQUIRE: Nothing
	// PROMISE: A non-empty string will have a trailing backslash
	// ARGS:	None
	std::string	getTargetFolder();

	// Generated message map functions
	//{{AFX_MSG(CCOMLicenseEvalDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnChangeEditCode();
	afx_msg void OnButtonEvaluate();
	afx_msg void OnButtonPaste();
	afx_msg void OnBrowse();
	afx_msg void OnRadioUclid();
	afx_msg void OnRadioUser();
	afx_msg void OnCheckSpecial();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

#ifdef _DEBUG
	// Handles Registry items
	std::unique_ptr<IConfigurationSettingsPersistenceMgr> ma_pSettingsCfgMgr;
#endif

private:
	HICON	m_hIcon;
	bool	m_bUserString;
	bool	m_bLicenseFile;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
