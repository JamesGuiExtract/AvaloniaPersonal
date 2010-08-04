// SelectTargetFileUIDlg.cpp : Implementation of CSelectTargetFileUIDlg

#include "stdafx.h"
#include "SelectTargetFileUIDlg.h"
#include "RedactionCCUtils.h"

#include <UCLIDException.h>
#include <LoadFileDlgThread.h>
#include <ComUtils.h>

//--------------------------------------------------------------------------------------------------
// CSelectTargetFileUIDlg
//--------------------------------------------------------------------------------------------------
CSelectTargetFileUIDlg::CSelectTargetFileUIDlg() :
	m_strFileName(""),
	m_strFileTypes("All Files (*.*)|*.*||"),
	m_strDefaultExtension(""),
	m_strDefaultFileName(""),
	m_strTitle("Select File"),
	m_strInstructions("Select target file")
{
}
//--------------------------------------------------------------------------------------------------
CSelectTargetFileUIDlg::~CSelectTargetFileUIDlg()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI17485");
}
//-------------------------------------------------------------------------------------------------
// CSelectTargetFileUIDlg message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CSelectTargetFileUIDlg::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CAxDialogImpl<CSelectTargetFileUIDlg>::OnInitDialog(uMsg, wParam, lParam, bHandled);

		// Initialize controls
		m_editFileName = GetDlgItem(IDC_EDIT_FILENAME);
		m_btnBrowse = GetDlgItem(IDC_BTN_BROWSE_FILE);
		m_btnSelectDocTag.SubclassDlgItem(IDC_BTN_SELECT_DOC_TAG, CWnd::FromHandle(m_hWnd));
		m_btnSelectDocTag.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));
		m_btnReset = GetDlgItem(IDC_RESET);

		// Initialize dlg text
		SetWindowText(m_strTitle.c_str());
		GetDlgItem(IDC_STATIC_INSTRUCTIONS).SetWindowText(m_strInstructions.c_str());
		m_editFileName.SetWindowText(m_strFileName.c_str());

		// If no default value is specified, hide reset button since no action is possible
		if (m_strDefaultFileName == "")
		{
			m_btnReset.ShowWindow(SW_HIDE);
		}

		// set focus to the editbox
		m_editFileName.SetSel(0, -1);
		m_editFileName.SetFocus();

		bHandled = TRUE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17486");

	return 1; // Let the system set the focus
}
//--------------------------------------------------------------------------------------------------
LRESULT CSelectTargetFileUIDlg::OnClickedOK(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Retrieve contents of edit box
		CComBSTR bstrNewFileName;
		m_editFileName.GetWindowText(&bstrNewFileName);
		m_strFileName = asString(bstrNewFileName);

		EndDialog(wID);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17487");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CSelectTargetFileUIDlg::OnClickedCancel(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		EndDialog(wID);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17488");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CSelectTargetFileUIDlg::OnBnClickedBtnSelectDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Display doc tags menu
		RECT rect;
		m_btnSelectDocTag.GetWindowRect(&rect);
		string strChoice = CRedactionCustomComponentsUtils::ChooseDocTag(m_hWnd, rect.right, rect.top);
		if (strChoice != "")
		{
			m_editFileName.ReplaceSel(strChoice.c_str(), TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17506");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CSelectTargetFileUIDlg::OnBnClickedBtnBrowseFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Initialize open file dialog
		CFileDialog fileDlg(TRUE, m_strDefaultExtension.c_str(), NULL, OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST,
			m_strFileTypes.c_str(), NULL);
		
		// Pass the pointer of dialog to create ThreadDataStruct object
		ThreadFileDlg tfd(&fileDlg);

		// if the user clicked on OK, then update the filename in the editbox
		if (fileDlg.DoModal() == IDOK)
		{
			// get the file name
			m_editFileName.SetWindowText(fileDlg.GetPathName());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17512");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CSelectTargetFileUIDlg::OnBnClickedBtnReset(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Reset file name to default value
		m_editFileName.SetWindowText(m_strDefaultFileName.c_str());
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17517");

	return 0;
}
//--------------------------------------------------------------------------------------------------
