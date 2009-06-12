// LoginDlg.cpp : implementation file

#include "stdafx.h"
#include "BaseUtils.h"
#include "LoginDlg.h"
#include "TemporaryResourceOverride.h"
#include "UCLIDException.h"

extern AFX_EXTENSION_MODULE BaseUtilsDLL;

IMPLEMENT_DYNAMIC(CLoginDlg, CDialog)

//-------------------------------------------------------------------------------------------------
// CLoginDlg dialog
//-------------------------------------------------------------------------------------------------
CLoginDlg::CLoginDlg(const std::string strTitle, std::string strUser, bool bReadOnlyUser, 
					 CWnd* pParent /*=NULL*/)
	: CDialog(CLoginDlg::IDD, pParent)
	, m_zUserName(_T(strUser.c_str()))
	, m_zPassword(_T(""))
	, m_bReadOnlyUser(bReadOnlyUser)
	, m_strTitle(strTitle)
{
}
//-------------------------------------------------------------------------------------------------
CLoginDlg::~CLoginDlg()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16388");
}
//-------------------------------------------------------------------------------------------------
void CLoginDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_EDIT_LOGIN_USER_NAME, m_editUserName);
	DDX_Control(pDX, IDC_EDIT_LOGIN_PASSWORD, m_editPassword);
	DDX_Text(pDX, IDC_EDIT_LOGIN_USER_NAME, m_zUserName);
	DDX_Text(pDX, IDC_EDIT_LOGIN_PASSWORD, m_zPassword);
}

//-------------------------------------------------------------------------------------------------
// Message handlers
//-------------------------------------------------------------------------------------------------
int CLoginDlg::DoModal()
{
	// Override resource so that the Utils resources will be used
	TemporaryResourceOverride rcOverride(BaseUtilsDLL.hResource);
	
	// call the base class member
	return CDialog::DoModal();
}
//-------------------------------------------------------------------------------------------------
BOOL CLoginDlg::OnInitDialog() 
{
	CDialog::OnInitDialog();
		
	// update the data for the prompt, input data, and title
	SetWindowText(m_strTitle.c_str());

	// Check for defined user name and read-only edit box
	if (!m_zUserName.IsEmpty() && m_bReadOnlyUser)
	{
		// Make the user name edit box read-only
		m_editUserName.SetReadOnly( TRUE );

		// Set focus to the password edit box
		m_editPassword.SetFocus();
	}
	else
	{
		// Set focus to the Username edit box
		m_editUserName.SetFocus();
	}

	return FALSE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

//-------------------------------------------------------------------------------------------------
// Message Map
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CLoginDlg, CDialog)
END_MESSAGE_MAP()


