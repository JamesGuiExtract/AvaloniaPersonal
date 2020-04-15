// FAMDBAdminAboutDlg.cpp : implementation file
//

#include "stdafx.h"
#include "FAMDBAdminAboutDlg.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <EnvironmentInfo.h>

#include <string>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// CFAMDBAdminAboutDlg dialog
//-------------------------------------------------------------------------------------------------
CFAMDBAdminAboutDlg::CFAMDBAdminAboutDlg()
: CDialog(CFAMDBAdminAboutDlg::IDD)
{
	//{{AFX_DATA_INIT(CFAMDBAdminAboutDlg)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminAboutDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CFAMDBAdminAboutDlg)
	// NOTE: the ClassWizard will add DDX and DDV calls here
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CFAMDBAdminAboutDlg, CDialog)
	//{{AFX_MSG_MAP(CFAMDBAdminAboutDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CFAMDBAdminAboutDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CFAMDBAdminAboutDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		CDialog::OnInitDialog();

		// Set the caption
		string strCaption = "About FAMDBAdmin";
		SetWindowText(strCaption.c_str());

		EnvironmentInfo envInfo;
		string strVersion = "Version " + envInfo.GetExtractVersion();
		SetDlgItemText(IDC_EDIT_VERSION, strVersion.c_str());

		string strLicenses = asString(envInfo.GetLicensedPackages(), false, "\r\n");
		SetDlgItemText(IDC_EDIT_LICENSES, strLicenses.c_str());
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18621");

	return TRUE;
}
//-------------------------------------------------------------------------------------------------
