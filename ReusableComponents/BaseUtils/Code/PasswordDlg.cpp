// PasswordDlg.cpp : implementation file

#include "stdafx.h"
#include "BaseUtils.h"
#include "PasswordDlg.h"
#include "TemporaryResourceOverride.h"
#include "UCLIDException.h"
#include <regex>

extern AFX_EXTENSION_MODULE BaseUtilsDLL;

//-------------------------------------------------------------------------------------------------
// PasswordDlg dialog
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(PasswordDlg, CDialog)

PasswordDlg::PasswordDlg(const std::string strTitle, const std::string strComplexityRequirements,
	CWnd* pParent /*=NULL*/)
	: CDialog(PasswordDlg::IDD, pParent)
	, m_zNewPassword(_T(""))
	, m_zRetypePwd(_T(""))
	, m_strTitle(strTitle)
	, m_strComplexityRequirements(strComplexityRequirements)
{
}
//-------------------------------------------------------------------------------------------------
PasswordDlg::~PasswordDlg()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16392");
}
//-------------------------------------------------------------------------------------------------
void PasswordDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Text(pDX, IDC_EDIT_NEW_PWD, m_zNewPassword);
	DDX_Text(pDX, IDC_EDIT_RETYPE_PWD, m_zRetypePwd);
	DDX_Control(pDX, IDC_EDIT_NEW_PWD, m_editNewPwd);
	DDX_Control(pDX, IDC_EDIT_RETYPE_PWD, m_editRetypePwd);
}
//-------------------------------------------------------------------------------------------------
int PasswordDlg::DoModal()
{
	try
	{
		TemporaryResourceOverride rcOverride(BaseUtilsDLL.hResource);
	
		// call the base class member
		return CDialog::DoModal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17756");

	// This will only be reached if there was an error 
	return IDCANCEL;
}
//-------------------------------------------------------------------------------------------------
BOOL PasswordDlg::OnInitDialog() 
{
	try
	{
		CDialog::OnInitDialog();

		// update the data for the prompt, input data, and title
		SetWindowText(m_strTitle.c_str());

		// Set focus to the new password edit box
		m_editNewPwd.SetFocus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17758");

	return FALSE;  // return TRUE unless you set the focus to a control
	// EXCEPTION: OCX Property Pages should return FALSE
}

//-------------------------------------------------------------------------------------------------
// Message Map 
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(PasswordDlg, CDialog)
	ON_BN_CLICKED(IDOK, &PasswordDlg::OnBnClickedOk)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// PasswordDlg message handlers
//-------------------------------------------------------------------------------------------------
void PasswordDlg::OnBnClickedOk()
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
			m_editNewPwd.SetFocus();

			// update the data members
			UpdateData();
			
			// rethrow exception
			throw;
		}
		
		// Password is valid
		OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15190");
}
//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void PasswordDlg::validatePasswords()
{
	// Update the data in the member vars
	UpdateData();

	// if passwords don't match clear and throw an exception
	if (strcmp( m_zNewPassword, m_zRetypePwd ) != 0 )
	{
		m_editNewPwd.SetWindowTextA("");
		m_editRetypePwd.SetWindowTextA("");
		throw UCLIDException("ELI15193", "Passwords must match!" );
	}

	// Throw exception if the password doesn't meet defined requirements
	Util::checkPasswordComplexity(string(m_zNewPassword), m_strComplexityRequirements);
}