// ClearWarningDlg.cpp : implementation file
//

#include "stdafx.h"
#include "FAMDBAdmin.h"
#include "ClearWarningDlg.h"

#include <UCLIDException.h>

//-------------------------------------------------------------------------------------------------
// ClearWarningDlg dialog
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(ClearWarningDlg, CDialog)
//-------------------------------------------------------------------------------------------------
ClearWarningDlg::ClearWarningDlg(const string& strCaption, const string& strTitle, CWnd* pParent)
	: CDialog(ClearWarningDlg::IDD, pParent),
	  m_strCaption(strCaption),
	  m_strTitle(strTitle),
	  m_bRetainActions(true)
{

}
//-------------------------------------------------------------------------------------------------
ClearWarningDlg::~ClearWarningDlg()
{
}
//-------------------------------------------------------------------------------------------------
bool ClearWarningDlg::getRetainActions()
{
	return m_bRetainActions;
}
//-------------------------------------------------------------------------------------------------
void ClearWarningDlg::setCaption(const string& strCaption)
{
	m_strCaption = strCaption;
}
//-------------------------------------------------------------------------------------------------
void ClearWarningDlg::setTitle(const string& strTitle)
{
	m_strTitle = strTitle;
}
//-------------------------------------------------------------------------------------------------
void ClearWarningDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_CAPTION, m_labelCaption);
	DDX_Control(pDX, IDC_CHECK_RETAIN_ACTIONS, m_checkRetainActions);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(ClearWarningDlg, CDialog)
	ON_BN_CLICKED(IDYES, &ClearWarningDlg::OnBnClickedYes)
	ON_BN_CLICKED(IDNO, &ClearWarningDlg::OnBnClickedNo)
END_MESSAGE_MAP()
//-------------------------------------------------------------------------------------------------
// ClearWarningDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL ClearWarningDlg::OnInitDialog() 
{
	CDialog::OnInitDialog();

	try
	{
		// Set the title and caption text
		SetWindowText(m_strTitle.c_str());
		m_labelCaption.SetWindowText(m_strCaption.c_str());

		// Set the checkbox
		m_checkRetainActions.SetCheck( asBSTChecked(m_bRetainActions) );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI25183")

	return TRUE;
}
//-------------------------------------------------------------------------------------------------
void ClearWarningDlg::OnBnClickedYes()
{
	m_bRetainActions = (m_checkRetainActions.GetCheck() == BST_CHECKED);

	EndDialog(IDYES);
}
//-------------------------------------------------------------------------------------------------
void ClearWarningDlg::OnBnClickedNo()
{
	m_bRetainActions = (m_checkRetainActions.GetCheck() == BST_CHECKED);

	EndDialog(IDNO);
}
//-------------------------------------------------------------------------------------------------
