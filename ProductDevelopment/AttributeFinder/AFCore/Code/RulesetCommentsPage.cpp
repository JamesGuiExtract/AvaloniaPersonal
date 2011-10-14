// RulesetCommentsPage.cpp : implementation file
//

#include "stdafx.h"
#include "RulesetCommentsPage.h"

#include <UCLIDException.h>

//--------------------------------------------------------------------------------------------------
// CRuleSetCommentsPage dialog
//--------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CRuleSetCommentsPage, CPropertyPage)
//--------------------------------------------------------------------------------------------------
CRuleSetCommentsPage::CRuleSetCommentsPage(UCLID_AFCORELib::IRuleSetPtr ipRuleSet)
	: CPropertyPage(CRuleSetCommentsPage::IDD)
, m_ipRuleSet(ipRuleSet)
{

}
//--------------------------------------------------------------------------------------------------
CRuleSetCommentsPage::~CRuleSetCommentsPage()
{
	try
	{
		m_ipRuleSet = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI34021");
}
//--------------------------------------------------------------------------------------------------
void CRuleSetCommentsPage::DoDataExchange(CDataExchange* pDX)
{
	CPropertyPage::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_EDIT_COMMENTS, m_editRulesetComments);
}
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CRuleSetCommentsPage, CPropertyPage)
END_MESSAGE_MAP()
//--------------------------------------------------------------------------------------------------
BOOL CRuleSetCommentsPage::OnInitDialog()
{
	try
	{
		CPropertyPage::OnInitDialog();

		m_editRulesetComments.SetWindowText(m_ipRuleSet->Comments);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI34017")

	return TRUE;  // return TRUE unless you set the focus to a control
	// EXCEPTION: OCX Property Pages should return FALSE
}
//--------------------------------------------------------------------------------------------------
void CRuleSetCommentsPage::Apply()
{
	try
	{
		// [FlexIDSCore:4898]
		// If the comments tab was not clicked on, the edit control will not yet have been created.
		// Don't update the comments in this case.
		if (m_editRulesetComments.m_hWnd != NULL)
		{
			CString zComments;
			m_editRulesetComments.GetWindowText(zComments);
			m_ipRuleSet->Comments = (LPCTSTR)zComments;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI34018")
}