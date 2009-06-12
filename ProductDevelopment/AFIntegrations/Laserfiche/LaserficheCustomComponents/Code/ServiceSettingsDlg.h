//==================================================================================================
//
// COPYRIGHT (c) 2008 EXTRACT SYSTEMS LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ServiceSettingsDlg.h
//
// PURPOSE:	A CIDShieldLF helper class.  Displays the configuration screen for the Laserfiche
//			background service.
//
// NOTES:	
//
// AUTHORS:	Steve Kurth
//
//==================================================================================================
#pragma once
#include "stdafx.h"
#include "resource.h"

#include "IDShieldLFHelper.h"

#include <string>
#include <vector>
#include <map>
using namespace std;

class CIDShieldLF;

//--------------------------------------------------------------------------------------------------
// CServiceSettingsDlg
//--------------------------------------------------------------------------------------------------
class CServiceSettingsDlg : public CDialog, private CIDShieldLFHelper
{
	DECLARE_DYNAMIC(CServiceSettingsDlg)

public:
	CServiceSettingsDlg(CIDShieldLF *pIDShieldLF, CWnd* pParent = NULL);   // standard constructor
	virtual ~CServiceSettingsDlg();

	enum { IDD = IDD_SERVICE };

protected:

	//////////////////////
	// Overrides
	//////////////////////

	CString m_zDirectory;
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();
	virtual void OnOK();

	//////////////////////
	// Message Handlers
	//////////////////////
	afx_msg void OnTimer(UINT_PTR nIDEvent);
	afx_msg void OnBnClickedBtnStart();
	afx_msg void OnBnClickedBtnStop();
	afx_msg void OnUpdateThreads();
	afx_msg void OnDeltaposSpinThreads(NMHDR *pNMHDR, LRESULT *pResult);

	//////////////////////
	// Control variables
	//////////////////////
	CComboBox m_cmbRepository;
	CString m_zRepository;
	CString m_zUser;
	CString m_zPassword;
	CString m_zThreads;
	CString m_zStatus;
	BOOL m_bAutoStart;

private:

	//////////////////
	// Variables
	//////////////////

	// Repository list
	vector<string> m_vecRepositoryList;
	map<string, string> m_mapServers;

	// Service instance & status
	SC_HANDLE m_hServiceManager;
	SC_HANDLE m_hService;
	DWORD m_dwServiceStatus;

	//////////////////
	// Methods
	//////////////////

	// Opens handles to the service manager and ID Shield service
	void initServiceHandles();

	// Checks the status of the service and updates the UI.
	void updateStatus();

	// Retrives the service status.  If bThrowOnError is true, it will throw an exception when
	// there is an error retrieving the service status.  If bThrowOnError is false it will 
	// return gnSERVICE_ERROR if there is an error retrieving the service
	DWORD getServiceStatus(bool bThrowOnError = false);

	// Returns true if the service is set to start automatically
	bool isAutoStart();

	// If bEnable == true, the startup type is set to automatic. If false, it is set to manual.
	void setAutoStart(bool bEnable);

	// Updates the UI to reflect the specified server, repository, user, password and number of
	// threads to use
	void setUIValues(const string &strServer, const string &strRepository, 
					 const string &strUser, const string &strPassword, const string &strThreads);
	// Retrieves the user selected values from the UI.
	void getUIValues(string &rstrServer, string &rstrRepository, string &rstrUser, 
					 string &rstrPassword, string &rstrThreads);

	// Stores currently selected settings to the registry
	void saveSettings();

	DECLARE_MESSAGE_MAP()
};
