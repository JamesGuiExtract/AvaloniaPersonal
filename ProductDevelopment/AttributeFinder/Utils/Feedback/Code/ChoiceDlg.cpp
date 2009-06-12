// ChoiceDlg.cpp : implementation file
//

#include "stdafx.h"
#include "resource.h"
#include "ChoiceDlg.h"
#include "ConfigDlg.h"
#include "PackageDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// CChoiceDlg dialog
//-------------------------------------------------------------------------------------------------
CChoiceDlg::CChoiceDlg(IFeedbackMgrInternalsPtr ipFBMgr, CWnd* pParent /*=NULL*/)
	: CDialog(CChoiceDlg::IDD, pParent),
	m_ipFBMgr(ipFBMgr)
{
	//{{AFX_DATA_INIT(CChoiceDlg)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT
}
//-------------------------------------------------------------------------------------------------
void CChoiceDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CChoiceDlg)
		// NOTE: the ClassWizard will add DDX and DDV calls here
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CChoiceDlg, CDialog)
	//{{AFX_MSG_MAP(CChoiceDlg)
	ON_BN_CLICKED(IDC_BTN_CONFIGURE, OnBtnConfigure)
	ON_BN_CLICKED(IDC_BTN_PACKAGE, OnBtnPackage)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CChoiceDlg message handlers
//-------------------------------------------------------------------------------------------------
void CChoiceDlg::OnBtnConfigure() 
{
	// Display the Configuration dialog
	CConfigDlg	dlg( m_ipFBMgr );
	dlg.DoModal();
}
//-------------------------------------------------------------------------------------------------
void CChoiceDlg::OnBtnPackage() 
{
	// Display the Packaging dialog
	CPackageDlg	dlg( m_ipFBMgr );
	dlg.DoModal();
}
//-------------------------------------------------------------------------------------------------
