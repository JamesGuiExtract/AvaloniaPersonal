// AlertLevelDlg.cpp : implementation file
//

#include "stdafx.h"
#include "USBLicenseKeyManager.h"
#include "AlertLevelDlg.h"

#include <UCLIDException.h>

//--------------------------------------------------------------------------------------------------
// CAlertLevelDlg dialog
//--------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CAlertLevelDlg, CDialog)
//--------------------------------------------------------------------------------------------------
CAlertLevelDlg::CAlertLevelDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CAlertLevelDlg::IDD, pParent)
	, m_dwAlertLevel(0)
	, m_dwAlertMultiple(0)
{
	try
	{
		
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI30409");
}
//--------------------------------------------------------------------------------------------------
CAlertLevelDlg::~CAlertLevelDlg()
{
}
//--------------------------------------------------------------------------------------------------
void CAlertLevelDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Text(pDX, IDC_EDIT_ALERT_LEVEL, m_dwAlertLevel);
	DDV_MinMaxDWord(pDX, m_dwAlertLevel, 0, 4294967295);
	DDX_Text(pDX, IDC_EDIT_ALERT_MULTIPLE, m_dwAlertMultiple);
	DDV_MinMaxDWord(pDX, m_dwAlertMultiple, 0, 4294967295);
}
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CAlertLevelDlg, CDialog)
END_MESSAGE_MAP()
//--------------------------------------------------------------------------------------------------
bool CAlertLevelDlg::GetAlertLevel(CWnd *pParent, CString zTitle,
								   DWORD &rdwAlertLevel, DWORD &rdwAlertMultiple)
{
	try
	{
		CAlertLevelDlg dlg(pParent);
		dlg.m_dwAlertLevel = rdwAlertLevel;
		dlg.m_dwAlertMultiple = rdwAlertMultiple;
		dlg.m_zTitle = zTitle;

		if (dlg.DoModal() == IDOK)
		{
			rdwAlertLevel = dlg.m_dwAlertLevel;
			rdwAlertMultiple = dlg.m_dwAlertMultiple;

			return true;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI30410");

	return false;
}
//--------------------------------------------------------------------------------------------------
BOOL CAlertLevelDlg::OnInitDialog()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		if (!m_zTitle.IsEmpty())
		{
			SetWindowText(m_zTitle);
		}

		UpdateData(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI30408")

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//--------------------------------------------------------------------------------------------------
