#pragma once

#include "afxwin.h"
#include "resource.h"

#include <string>

//-------------------------------------------------------------------------------------------------
// PasswordDlg dialog
//-------------------------------------------------------------------------------------------------
class EXPORT_BaseUtils PasswordDlg : public CDialog
{
	DECLARE_DYNAMIC(PasswordDlg)

public:
	PasswordDlg(const std::string strTitle = "Change Password", std::string strComplexityRequirements = "",
		CWnd* pParent = NULL);
	virtual ~PasswordDlg();
	
	// Override needed to override resource to use BaseUtils.dll
	virtual int DoModal();
// Dialog Data
	enum { IDD = IDD_CREATE_PASSWORD };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();

	DECLARE_MESSAGE_MAP()

	// Message Handlers
	afx_msg void OnBnClickedOk();

public:
	CString m_zNewPassword;
	CString m_zRetypePwd;

private:
	// Control variables
	CEdit m_editNewPwd;
	CEdit m_editRetypePwd;

	// Caption for the dialog
	std::string m_strTitle;

	// Encoded complexity requirements
	std::string m_strComplexityRequirements;

	// PROMISE: To check the passwords and thow exception if passwords don't match or are empty string
	void validatePasswords();
};
