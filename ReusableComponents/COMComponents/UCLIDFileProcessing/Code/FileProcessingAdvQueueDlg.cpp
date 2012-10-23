// FileProcessingAdvQueueDlg.cpp : implementation file
//

#include "stdafx.h"
#include "resource.h"
#include "FileProcessingAdvQueueDlg.h"
#include "afxdialogex.h"

#include <UCLIDException.h>

IMPLEMENT_DYNAMIC(FileProcessingAdvQueueDlg, CDialogEx)

//-------------------------------------------------------------------------------------------------
// FileProcessingAdvQueueDlg dialog
//-------------------------------------------------------------------------------------------------
FileProcessingAdvQueueDlg::FileProcessingAdvQueueDlg(CWnd* pParent /*=NULL*/)
	: CDialogEx(FileProcessingAdvQueueDlg::IDD, pParent)
	, m_bSkipPageCount(FALSE)
{
	try
	{
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI35086");
}
//-------------------------------------------------------------------------------------------------
FileProcessingAdvQueueDlg::~FileProcessingAdvQueueDlg()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI35087");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingAdvQueueDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);
	DDX_Check(pDX, IDC_CHK_SKIP_PAGE_COUNT, m_bSkipPageCount);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(FileProcessingAdvQueueDlg, CDialogEx)
	ON_BN_CLICKED(IDOK, &FileProcessingAdvQueueDlg::OnBnClickedOk)
	ON_BN_CLICKED(IDCANCEL, &FileProcessingAdvQueueDlg::OnBnClickedCancel)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// FileProcessingAdvQueueDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL FileProcessingAdvQueueDlg::OnInitDialog()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CDialogEx::OnInitDialog();

		UpdateData(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI35088");

	return TRUE;  // return TRUE unless you set the focus to a control
	// EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void FileProcessingAdvQueueDlg::OnOK()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		UpdateData(TRUE);

		CDialogEx::OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI35089");
}

//-------------------------------------------------------------------------------------------------
void FileProcessingAdvQueueDlg::OnBnClickedOk()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CDialogEx::OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI35090");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingAdvQueueDlg::OnBnClickedCancel()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// TODO: Add your control notification handler code here
		CDialogEx::OnCancel();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI35091");
}
//-------------------------------------------------------------------------------------------------