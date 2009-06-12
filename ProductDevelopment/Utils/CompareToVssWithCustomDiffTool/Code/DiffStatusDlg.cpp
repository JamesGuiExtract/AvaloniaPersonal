// DiffStatusDlg.cpp : implementation file
//

#include "stdafx.h"
#include "CompareToVssWithCustomDiffTool.h"
#include "DiffStatusDlg.h"

//--------------------------------------------------------------------------------------------------
// DiffStatusDlg dialog
//--------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(DiffStatusDlg, CDialog)
DiffStatusDlg::DiffStatusDlg(CWnd* pParent /*=NULL*/)
	: CDialog(DiffStatusDlg::IDD, pParent)
{
}
//--------------------------------------------------------------------------------------------------
// Message map for DiffStatusDlg
BEGIN_MESSAGE_MAP(DiffStatusDlg, CDialog)
END_MESSAGE_MAP()
//--------------------------------------------------------------------------------------------------
DiffStatusDlg::~DiffStatusDlg()
{
}
//--------------------------------------------------------------------------------------------------
void DiffStatusDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}

//--------------------------------------------------------------------------------------------------
// DiffStatusDlg message handlers
//--------------------------------------------------------------------------------------------------
