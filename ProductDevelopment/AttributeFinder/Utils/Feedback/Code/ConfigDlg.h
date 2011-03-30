#pragma once

// ConfigDlg.h : header file
//

#include "resource.h"
#include "..\..\FeedbackManager\Code\PersistenceMgr.h"

/////////////////////////////////////////////////////////////////////////////
// CConfigDlg dialog

class CConfigDlg : public CDialog
{
// Construction
public:
	CConfigDlg(IFeedbackMgrInternalsPtr ipFBMgr, CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(CConfigDlg)
	enum { IDD = IDD_DLG_CONFIG };
	CListCtrl	m_list;
	CButton	m_btnDelete;
	CButton	m_btnModify;
	CButton	m_btnAdd;
	BOOL	m_bEnabled;
	BOOL	m_bConvertToText;
	BOOL	m_bTurnOff;
	CString	m_zDate;
	CString	m_zCount;
	CString	m_zFolder;
	int		m_bAllAttributes;
	int		m_bNoCollect;
	int		m_bOnDate;
	CString	m_zSkipCount;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CConfigDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CConfigDlg)
	afx_msg void OnBtnBrowse();
	afx_msg void OnBtnClear();
	virtual void OnOK();
	afx_msg void OnCheckTurnoff();
	afx_msg void OnCheckEnable();
	virtual BOOL OnInitDialog();
	afx_msg void OnRadioOndate();
	afx_msg void OnRadioAfter();
	afx_msg void OnRadioNocollect();
	afx_msg void OnRadioAtexecution();
	afx_msg void OnRadioAtpackaging();
	afx_msg void OnBtnAdd();
	afx_msg void OnBtnDelete();
	afx_msg void OnBtnModify();
	afx_msg void OnRadioAll();
	afx_msg void OnRadioSome();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	///////
	// Data
	///////

	// Handles configuration persistence
	std::unique_ptr<IConfigurationSettingsPersistenceMgr> ma_pUserCfgMgr;
	std::unique_ptr<PersistenceMgr> ma_pCfgFeedbackMgr;

	IFeedbackMgrInternalsPtr m_ipFBMgr;

	//////////
	// Methods
	//////////
	// Enables and disables controls
	void	enableButtonStates();

	// Reads registry settings
	void	readRegistrySettings();

	// Writes registry settings
	void	writeRegistrySettings();
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
