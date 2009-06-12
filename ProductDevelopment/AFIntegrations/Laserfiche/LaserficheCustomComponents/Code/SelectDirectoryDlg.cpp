// SelectDirectoryDlg.cpp : implementation file
//
#include "stdafx.h"
#include "LaserficheCustomComponents.h"
#include "SelectDirectoryDlg.h"

#include <UCLIDException.h>

//--------------------------------------------------------------------------------------------------
// CSelectDirectoryDlg
//--------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CSelectDirectoryDlg, CDialog)
//--------------------------------------------------------------------------------------------------
CSelectDirectoryDlg::CSelectDirectoryDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CSelectDirectoryDlg::IDD, pParent)
	, m_zDirectory(_T(""))
{
}
//--------------------------------------------------------------------------------------------------
CSelectDirectoryDlg::~CSelectDirectoryDlg()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20812");
}
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CSelectDirectoryDlg, CDialog)
END_MESSAGE_MAP()
//--------------------------------------------------------------------------------------------------
int CSelectDirectoryDlg::prompt(string &rstrDirectory)
{
	try
	{
		m_zDirectory = rstrDirectory.c_str();

		int nRes = (int) DoModal();
		if (nRes == IDOK)
		{
			if (rstrDirectory == m_zDirectory.GetString())
			{
				// If the directory hasn't changed, treat it the same as pressing cancel.
				return IDCANCEL;
			}
			else
			{
				// Update the directory with the entered value.
				rstrDirectory = m_zDirectory.GetString();
			}
		}

		return nRes;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20813");
}

//--------------------------------------------------------------------------------------------------
// Overrides
//--------------------------------------------------------------------------------------------------
void CSelectDirectoryDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Text(pDX, IDC_EDIT_DIRECTORY, m_zDirectory);
}
//--------------------------------------------------------------------------------------------------