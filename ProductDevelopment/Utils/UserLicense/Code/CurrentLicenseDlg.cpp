#include "stdafx.h"
#include "UserLicense.h"
#include "CurrentLicenseDlg.h"

#include <UCLIDException.h>
#include <EnvironmentInfo.h>

//-------------------------------------------------------------------------------------------------
// CCurrentLicenseDlg
//-------------------------------------------------------------------------------------------------
CCurrentLicenseDlg::CCurrentLicenseDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CCurrentLicenseDlg::IDD, pParent)
{
	try
	{
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI49755");
}
//-------------------------------------------------------------------------------------------------
CCurrentLicenseDlg::~CCurrentLicenseDlg()
{
}
//-------------------------------------------------------------------------------------------------
void CCurrentLicenseDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CAboutDlg)
	//}}AFX_DATA_MAP
}//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CCurrentLicenseDlg, CDialog)
END_MESSAGE_MAP()
//--------------------------------------------------------------------------------------------------
BOOL CCurrentLicenseDlg::OnInitDialog()
{
	try
	{
		CDialog::OnInitDialog();

		EnvironmentInfo envInfo;

		SetDlgItemText(IDC_EDIT_VERSION, ("Version " + envInfo.GetExtractVersion()).c_str());

		SetDlgItemText(IDC_EDIT_LICENSE, asString(envInfo.GetLicensedPackages(), false, "\r\n").c_str());
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI49756")

	return TRUE;  // return TRUE unless you set the focus to a control
				  // EXCEPTION: OCX Property Pages should return FALSE
}
