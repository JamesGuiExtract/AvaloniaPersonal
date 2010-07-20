// USBLicenseKeyManagerDlg.h : header file
//

#pragma once

#include "SafeNetLicenseCfg.h"
#include "SafeNetLicenseMgr.h"

/////////////////////////////////////////////////////////////////////////////
// CUSBLicenseKeyManagerDlg dialog

class CUSBLicenseKeyManagerDlg : public CDialog
{
// Construction
public:
	CUSBLicenseKeyManagerDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	//{{AFX_DATA(CUSBLicenseKeyManagerDlg)
	enum { IDD = IDD_USBLICENSEKEYMANAGER_DIALOG };
	CButton	m_checkAlertExtract;
	CEdit	m_editToList;
	CButton	m_checkEmailAlert;
	CEdit	m_editCounterSerial;
	CEdit	m_editRemoteMachineName;
	CButton	m_btnRemoteMachine;
	CButton	m_btnLocalMachine;
	CEdit	m_editHardLimit;
	CEdit	m_editSoftLimit;
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CUSBLicenseKeyManagerDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	//{{AFX_MSG(CUSBLicenseKeyManagerDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnRefresh();
	afx_msg void OnResetLock();
	virtual void OnOK();
	afx_msg void OnMachineChanged();
	afx_msg void OnApply();
	afx_msg void OnServerStatus();
	afx_msg void OnEmailsettings();
	afx_msg void OnEmailAlert();
	afx_msg void OnNMDblclkCounterValues(NMHDR *pNMHDR, LRESULT *pResult);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	void updateControls();

	void clearCounterValuesList();
	void loadCounterValuesList(SafeNetLicenseMgr &snLM);

	// Loads the Alert level and alert Multiple for each of the counters
	void loadAlertValues(int nCounterRow, DataCell &dcCell);
	// Saves the alert values in the grid to the registry
	void applyAlertValues(int nCounterRow, DataCell &dcCell);

	void prepareCounterList();
	
	void setGridCounterValues ( SafeNetLicenseMgr &snLM, int nCounterRow, DataCell &dcCell);
	
	void applyNewValues();

	// Variables

	// will be true if the specified server has a obtainable key on it
	bool m_bIsKeyServerValid;
	SafeNetLicenseCfg m_snlcSafeNetCfg;
	// For the Email Settings
	IEmailSettingsPtr m_ipEmailSettings;
	
	CListCtrl m_listCounterValues;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

