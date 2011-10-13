// RulesetPropertiesDlg.cpp : implementation file
//
#include "stdafx.h"
#include "RulesetPropertiesDlg.h"

#include <UCLIDException.h>

//--------------------------------------------------------------------------------------------------
// CRuleSetPropertiesDlg dialog
//--------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CRuleSetPropertiesDlg, CDialog)
//--------------------------------------------------------------------------------------------------
CRuleSetPropertiesDlg::CRuleSetPropertiesDlg(UCLID_AFCORELib::IRuleSetPtr ipRuleSet, bool bReadOnly,
	CWnd* pParent /*=NULL*/)
: CDialog(CRuleSetPropertiesDlg::IDD, pParent)
, m_ruleSetPropertiesPage(ipRuleSet, bReadOnly)
, m_ruleSetCommentsPage(ipRuleSet)
, m_bReadOnly(bReadOnly)
{

}
//--------------------------------------------------------------------------------------------------
CRuleSetPropertiesDlg::~CRuleSetPropertiesDlg()
{
}
//--------------------------------------------------------------------------------------------------
void CRuleSetPropertiesDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CRuleSetPropertiesDlg, CDialog)
END_MESSAGE_MAP()

//--------------------------------------------------------------------------------------------------
// CRuleSetPropertiesDlg message handlers
//--------------------------------------------------------------------------------------------------
BOOL CRuleSetPropertiesDlg::OnInitDialog()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CDialog::OnInitDialog();

		m_propSheet.AddPage(&m_ruleSetPropertiesPage);

		m_propSheet.AddPage(&m_ruleSetCommentsPage);
		m_propSheet.Create(this, WS_CHILD | WS_VISIBLE, 0);
		m_propSheet.ModifyStyleEx (0, WS_EX_CONTROLPARENT);

		CRect rectDlg;
		GetClientRect(&rectDlg);
		int i = GetSystemMetrics(SM_CYCAPTION);
		int k = GetSystemMetrics(SM_CXSIZEFRAME);

		CRect rectOKButton;
		GetDlgItem(IDOK)->GetWindowRect(&rectOKButton);
		CWnd::ScreenToClient(&rectOKButton);

		// Resize the property sheet
		CRect rectPropSheet;
		rectPropSheet.left = 0;
		// leave space below toolbar for lines from OnPaint()
		rectPropSheet.top = rectDlg.top + 2;
		rectPropSheet.right = rectDlg.right;
		rectPropSheet.bottom = rectOKButton.top - 2;
		m_propSheet.resize(rectPropSheet);

		if (m_bReadOnly)
		{
			// In order to disable the tabs they need to have been created and they are not created
			// until they become active.
			m_propSheet.SetActivePage(1);
			m_propSheet.SetActivePage(0);

			m_ruleSetPropertiesPage.EnableWindow(FALSE);
			m_ruleSetCommentsPage.EnableWindow(FALSE);

			GetDlgItem(IDOK)->ShowWindow(SW_HIDE);
			GetDlgItem(IDCANCEL)->ShowWindow(SW_HIDE);

			int nAmountToShrink = rectDlg.bottom - rectOKButton.top;

			GetWindowRect(&rectDlg);
			rectDlg.bottom -= nAmountToShrink;
			MoveWindow(rectDlg);
		}
		else
		{
			m_propSheet.SetActivePage(0);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI34015")

	return TRUE;  // return TRUE unless you set the focus to a control
	// EXCEPTION: OCX Property Pages should return FALSE
}
//--------------------------------------------------------------------------------------------------
void CRuleSetPropertiesDlg::OnOK()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		if (!m_bReadOnly)
		{
			m_ruleSetPropertiesPage.Apply();
			m_ruleSetCommentsPage.Apply();
		}

		CDialog::OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI34016")                                   
}
