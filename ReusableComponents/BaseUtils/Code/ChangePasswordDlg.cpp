// ChangePasswordDlg.cpp : implementation file

#include "stdafx.h"
#include "BaseUtils.h"
#include "ChangePasswordDlg.h"
#include "TemporaryResourceOverride.h"
#include "UCLIDException.h"

extern AFX_EXTENSION_MODULE BaseUtilsDLL;

//-------------------------------------------------------------------------------------------------
// ChangePasswordDlg dialog
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(ChangePasswordDlg, CDialog)

ChangePasswordDlg::ChangePasswordDlg(const std::string strTitle, const std::string strComplexityRequirements,
	CWnd* pParent /*=NULL*/)
	: CDialog(ChangePasswordDlg::IDD, pParent)
	, m_zNewPassword(_T(""))
	, m_zRetypePwd(_T(""))
	, m_strTitle(strTitle)
	, m_strComplexityRequirements(strComplexityRequirements)
{
}
//-------------------------------------------------------------------------------------------------
ChangePasswordDlg::~ChangePasswordDlg()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI17710");
}
//-------------------------------------------------------------------------------------------------
void ChangePasswordDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Text(pDX, IDC_EDIT_NEW_PWD, m_zNewPassword);
	DDX_Text(pDX, IDC_EDIT_RETYPE_PWD, m_zRetypePwd);
	DDX_Text(pDX, IDC_EDIT_OLD_PWD, m_zOldPassword);
	DDX_Control(pDX, IDC_EDIT_NEW_PWD, m_editNewPwd);
	DDX_Control(pDX, IDC_EDIT_RETYPE_PWD, m_editRetypePwd);
	DDX_Control(pDX, IDC_EDIT_OLD_PWD, m_editOldPwd);
}
//-------------------------------------------------------------------------------------------------
int ChangePasswordDlg::DoModal()
{
	try
	{
		TemporaryResourceOverride rcOverride(BaseUtilsDLL.hResource);
	
		// call the base class member
		return CDialog::DoModal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17762");

	// This will only be reached if there was an error 
	return IDCANCEL;
}
//-------------------------------------------------------------------------------------------------
BOOL ChangePasswordDlg::OnInitDialog() 
{
	try
	{
		CDialog::OnInitDialog();

		// update the data for the prompt, input data, and title
		SetWindowText(m_strTitle.c_str());

		// Set focus to the old password field
		m_editOldPwd.SetFocus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17761");

	return FALSE;  // return TRUE unless you set the focus to a control
	// EXCEPTION: OCX Property Pages should return FALSE
}

//-------------------------------------------------------------------------------------------------
// Message Map 
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(ChangePasswordDlg, CDialog)
	ON_BN_CLICKED(IDOK, &ChangePasswordDlg::OnBnClickedOk)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// ChangePasswordDlg message handlers
//-------------------------------------------------------------------------------------------------
void ChangePasswordDlg::OnBnClickedOk()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Update the data in the member vars
		UpdateData();

		try
		{
			// Make sure the passwords are valid
			validatePasswords();
		}
		catch(...)
		{
			// An exception here means there was a problem with the new passwords
			// so set focus to the new password edit box.
			m_editNewPwd.SetFocus();

			// update the data members
			UpdateData();
			
			// rethrow exception
			throw;
		}
		
		// Password is valid
		OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17711");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void ChangePasswordDlg::validatePasswords()
{
	// Update the data in the member vars
	UpdateData();

	if (m_zNewPassword.IsEmpty() )
	{
		throw UCLIDException("ELI17712", "Password must not be blank!" );
	}
	else if (m_zNewPassword == m_zOldPassword)
	{
		// new password cannot be the same as the old password
		throw UCLIDException("ELI17713", "New password must be different from old password!");
	}

	// if passwords don't match, clear and throw an exception
	if (strcmp( m_zNewPassword, m_zRetypePwd ) != 0 )
	{
		m_editNewPwd.SetWindowTextA("");
		m_editRetypePwd.SetWindowTextA("");
		throw UCLIDException("ELI17714", "Passwords must match!" );
	}

	// Throw exception if the password doesn't meet defined requirements
	Util::checkPasswordComplexity(string(m_zNewPassword), m_strComplexityRequirements);
}
//-------------------------------------------------------------------------------------------------
