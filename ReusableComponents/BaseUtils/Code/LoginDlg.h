#pragma once

#include "afxwin.h"
#include "BaseUtils.h"
#include "Resource.h"

#include <string>

//-------------------------------------------------------------------------------------------------
// CLoginDlg dialog
//-------------------------------------------------------------------------------------------------
class EXPORT_BaseUtils CLoginDlg : public CDialog
{
	DECLARE_DYNAMIC(CLoginDlg)

public:
	CLoginDlg(const std::string strTitle = "Login", std::string strUser = "", 
		bool bReadOnlyUser = false, CWnd* pParent = NULL);
	virtual ~CLoginDlg();
	
	// Override needed to override resource to use BaseUtils.dll
	virtual int DoModal();

// Dialog Data
	enum { IDD = IDD_LOGINDLG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();

	DECLARE_MESSAGE_MAP()
public:
	// Data from controls
	CString m_zUserName;
	CString m_zPassword;

private:
	// Controls
	CEdit m_editUserName;
	CEdit m_editPassword;

	// Set user name edit box to read-only IFF user name also defined
	bool	m_bReadOnlyUser;

	// Caption for the dialog box
	std::string m_strTitle;
};
