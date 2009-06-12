// TagInfoDlg.cpp : implementation file
//
#include "stdafx.h"
#include "LaserficheCustomComponents.h"
#include "TagInfoDlg.h"

#include <UCLIDException.h>

//--------------------------------------------------------------------------------------------------
// CTagInfoDlg
//--------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CTagInfoDlg, CDialog)
//--------------------------------------------------------------------------------------------------
CTagInfoDlg::CTagInfoDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CTagInfoDlg::IDD, pParent)
{
}
//--------------------------------------------------------------------------------------------------
CTagInfoDlg::~CTagInfoDlg()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI21031");
}
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CTagInfoDlg, CDialog)
END_MESSAGE_MAP()

//--------------------------------------------------------------------------------------------------
// Overrides
//--------------------------------------------------------------------------------------------------
void CTagInfoDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_PIC_INFOICON, m_Icon);
}

//--------------------------------------------------------------------------------------------------
// Message Handlers
//--------------------------------------------------------------------------------------------------
BOOL CTagInfoDlg::OnInitDialog()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CDialog::OnInitDialog();

		m_Icon.SetIcon(LoadIcon(NULL, MAKEINTRESOURCE(IDI_INFORMATION)));
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI21351");

	return TRUE;
}
//--------------------------------------------------------------------------------------------------