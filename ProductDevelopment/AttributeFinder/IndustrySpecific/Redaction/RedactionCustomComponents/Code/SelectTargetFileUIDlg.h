// SelectTargetFileUIDlg.h : Declaration of the CSelectTargetFileUIDlg

#pragma once

#include "resource.h"       // main symbols

#include <atlhost.h>
#include <string>
#include <ImageButtonWithStyle.h>

// CSelectTargetFileUIDlg

class CSelectTargetFileUIDlg : 
	public CAxDialogImpl<CSelectTargetFileUIDlg>
{
	friend class CSelectTargetFileUI;

public:
	CSelectTargetFileUIDlg();
	~CSelectTargetFileUIDlg();

	enum { IDD = IDD_SELECTTARGETFILEUIDLG };

BEGIN_MSG_MAP(CSelectTargetFileUIDlg)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDOK, BN_CLICKED, OnClickedOK)
	COMMAND_HANDLER(IDCANCEL, BN_CLICKED, OnClickedCancel)
	COMMAND_HANDLER(IDC_BTN_SELECT_DOC_TAG, BN_CLICKED, OnBnClickedBtnSelectDocTag)
	COMMAND_HANDLER(IDC_BTN_BROWSE_FILE, BN_CLICKED, OnBnClickedBtnBrowseFile)
	COMMAND_HANDLER(IDC_RESET, BN_CLICKED, OnBnClickedBtnReset)
	CHAIN_MSG_MAP(CAxDialogImpl<CSelectTargetFileUIDlg>)
	// REFLECT_NOTIFICATIONS needed by ImageButtonWithSytle
	REFLECT_NOTIFICATIONS()
END_MSG_MAP()

// CSelectTargetFileUIDlg message handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedOK(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedCancel(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedBtnSelectDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedBtnBrowseFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedBtnReset(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	//////////////
	// Variables
	//////////////
	ATLControls::CEdit m_editFileName;
	ATLControls::CButton m_btnBrowse;
	CImageButtonWithStyle m_btnSelectDocTag;
	ATLControls::CButton m_btnReset;
		
	std::string m_strFileName;
	std::string m_strFileTypes;
	std::string m_strDefaultExtension;
	std::string m_strDefaultFileName;
	std::string m_strTitle;
	std::string m_strInstructions;
};


