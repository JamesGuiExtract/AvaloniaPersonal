#pragma once
// EmailSettingsDlg.h : header file
//
#include "resource.h"       // main symbols

#include <string>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// EmailSettingsDlg dialog

class EmailSettingsDlg : public CDialog
{
// Construction
public:
	EmailSettingsDlg(ESMESSAGEUTILSLib::IEmailSettingsPtr ipEmailSettings, CWnd* pParent = NULL);   // standard constructor
	virtual ~EmailSettingsDlg();

// Dialog Data
	//{{AFX_DATA(EmailSettingsDlg)
	enum { IDD = IDD_EMAIL_SETTINGS_DLG };
	CEdit	m_editUserName;
	CEdit	m_editSMTPServer;
	CEdit	m_editPassword;
	CButton	m_checkAuthentication;
	CEdit	m_editSenderDisplayName;
	CEdit	m_editSenderEmailAddr;
	CEdit	m_editSignature;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(EmailSettingsDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(EmailSettingsDlg)
	virtual void OnOK();
	virtual BOOL OnInitDialog();
	afx_msg void OnAuthentication();
	afx_msg void OnBnClickedButtonSendTestEmail();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	
	ESMESSAGEUTILSLib::IEmailSettingsPtr m_ipEmailSettings;
	// used to enable and disable controls based on check box value
	void updateControls();

	// Reads the settings from the dialog controls and saves to the passed in ipEmailSettings
	void getSettingsFromDialog(ESMESSAGEUTILSLib::IEmailSettingsPtr ipEmailSettings);

	// Returns a IVariantVector filled with the , or ; separated email addresses
	IVariantVectorPtr parseRecipientAddress(const string &strEmailAddresses);
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

