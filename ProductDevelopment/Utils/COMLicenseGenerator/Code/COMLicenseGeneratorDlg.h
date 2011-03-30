//============================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	COMLicenseGeneratorDlg.h
//
// PURPOSE:	Declaration of COMLicenseGeneratorDlg class
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//============================================================================
//{{AFX_INCLUDES()
#include "calendar.h"
//}}AFX_INCLUDES

#pragma once

#ifdef _DEBUG
#include <IConfigurationSettingsPersistenceMgr.h>
#endif

#include <string>
#include <vector>

class COMPackages;

/////////////////////////////////////////////////////////////////////////////
// CCOMLicenseGeneratorDlg dialog

class CCOMLicenseGeneratorDlg : public CDialog
{
// Construction
public:

	//=======================================================================
	// PURPOSE: Constructor for modal dialog.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	pParent - parent window
	CCOMLicenseGeneratorDlg(bool bEnableSDK = false, CWnd* pParent = NULL);

// Dialog Data
	//{{AFX_DATA(CCOMLicenseGeneratorDlg)
	enum { IDD = IDD_COMLICENSEGENERATOR_DIALOG };
	CComboBox	m_cboType;
	CButton	m_ctlPaste;
	CButton	m_ctlCopy;
	CButton	m_ctlGenerate;
	CListBox	m_list;
	CString	m_zCode;
	CString	m_zDate;
	CString	m_zLicensee;
	CString	m_zOrganization;
	CCalendar	m_calendar;
	CString	m_zFile;
	CString	m_zUser;
	CString	m_zComputer;
	BOOL	m_bUseComputerName;
	BOOL	m_bUseSerialNumber;
	BOOL	m_bUseMACAddress;
	CString	m_zOEM;
	CString	m_zFolder;
	CButton	m_ctlPastePassword;
	CButton	m_ctlUnlock;
	BOOL	m_bUseSpecial;
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CCOMLicenseGeneratorDlg)
	public:
	virtual BOOL DestroyWindow();
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	//=======================================================================
	// PURPOSE: Adds password types to combo box.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	None
	void	populateCombo();

	//=======================================================================
	// PURPOSE: Adds package names to list control.  Package names and 
	//				associated component IDs are contained in the 
	//				Packages.dat file managed by the COMPackages class.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	None
	void	populateList();

	//=======================================================================
	// PURPOSE: Creates 4 random unsigned longs to be used as a password to 
	//				encrypt the license file.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	None
	void	generatePassword();

	//=======================================================================
	// PURPOSE: Provides default folder for license or unlock file.  If 
	//			SBL folder is not writable, returns local CommonComponents
	//			folder, if present.  In Debug mode, uses persistent folder 
	//			from the registry.
	// REQUIRE: Nothing
	// PROMISE: A non-empty string will have a trailing backslash
	// ARGS:	None
	std::string	getTargetFolder();

	//=======================================================================
	// PURPOSE: Sets up license mode and default expiration date.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	None
	void	initLicensing();

	//=======================================================================
	// PURPOSE: Checks to see if all required items have been set before 
	//				allowing creation and display of a license string.
	// REQUIRE: Nothing
	// PROMISE: Returns true if all items are present and valid, false 
	//				otherwise.
	// ARGS:	None
	bool	isDataValid();

	//=======================================================================
	// PURPOSE: Checks to see if all required items have been set before 
	//				allowing creation of an unlock license file.
	// REQUIRE: Nothing
	// PROMISE: Returns true if all items are present and valid, false 
	//				otherwise.
	// ARGS:	None
	bool	isUnlockDataValid();

	//=======================================================================
	// PURPOSE: Saves default folder for license or unlock file to registry.
	// REQUIRE: strFolder shall have a trailing backslash
	// PROMISE: Does nothing in Release mode.
	// ARGS:	None
	void	saveTargetFolder(std::string strFolder);

	//=======================================================================
	// PURPOSE: Enables and disables Generate button based on data validity.
	//				Enables and disables Copy button based on presence of 
	//				license key string.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	None
	void	updateButtons();

	//=======================================================================
	// PURPOSE: Adds IDs to the master collection from a sub-collection that 
	//				is associated with a particular package.
	// REQUIRE: Nothing
	// PROMISE: The master ID collection will have only one copy of each 
	//				licensed component ID.
	// ARGS:	vecIDs - collected IDs to be added to the master set
	void	updateComponentCollection(std::vector<unsigned long> vecIDs);

	// Generated message map functions
	//{{AFX_MSG(CCOMLicenseGeneratorDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnButtonCopy();
	afx_msg void OnButtonGenerate();
	afx_msg void OnRadioEval();
	afx_msg void OnRadioFull();
	afx_msg void OnSelchangeListPackages();
	afx_msg void OnChangeEditLicensee();
	afx_msg void OnChangeEditOrganization();
	afx_msg void OnAfterUpdateCalendar();
	afx_msg void OnAfterNewMonthCalendar();
	afx_msg void OnAfterNewYearCalendar();
	afx_msg void OnButtonPaste();
	afx_msg void OnCheckComputer();
	afx_msg void OnCheckDisk();
	afx_msg void OnCheckAddress();
	afx_msg void OnRadioRandom();
	afx_msg void OnRadioUCLID();
	afx_msg void OnRadioSpecified();
	afx_msg void OnButtonPastePassword();
	afx_msg void OnButtonUnlock();
	afx_msg void OnButtonUserlicense();
	DECLARE_EVENTSINK_MAP()
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

	// Icon displayed on title bar
	HICON			m_hIcon;

	// Data object responsible for managing COM component IDs and 
	// associations between package names and required components
	COMPackages*	m_pPackageInfo;

	// License state
	bool			m_bFullyLicensed;

	// Password state
	bool			m_bRandomPassword;
	bool			m_bSpecifiedPassword;

	// Will Random password be enabled
	bool			m_bEnableSDK;

	// Has dialog been initialized yet?
	bool			m_bInitialized;

	// Name of person to whom license is being given
	std::string		m_strLicensee;

	// Name of organization to whom license is being given
	std::string		m_strOrganization;

	// Current version number string
	std::string		m_strVersion;

	// System time that license string is created/issued
	CTime			m_timeIssue;

	// Date and time that components with evaluation licenses will expire
	CTime			m_timeExpire;

	// Password to decrypt license file
	unsigned long	m_ulKey1;
	unsigned long	m_ulKey2;
	unsigned long	m_ulKey3;
	unsigned long	m_ulKey4;

#ifdef _DEBUG
	// Handles Registry items
	std::unique_ptr<IConfigurationSettingsPersistenceMgr> ma_pSettingsCfgMgr;
#endif

	// Final product of license creation
	std::string		m_strLicenseKey;

	// Master collection of COM component IDs for whom a license will be 
	// generated
	std::vector<unsigned long>	m_vecLicensedComponents;

private:
#ifdef _DEBUG
	// Registry keys for information persistence
	static const std::string LICENSE_SECTION;
	static const std::string TARGETFOLDER_KEY;
#endif
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
