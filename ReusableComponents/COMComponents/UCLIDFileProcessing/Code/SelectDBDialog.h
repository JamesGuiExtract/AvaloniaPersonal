#pragma once

#include "afxwin.h"
#include "resource.h"

#include <RegistryPersistenceMgr.h>
#include <FileProcessingConfigMgr.h>
#include <DBInfoCombo.h>

#include <memory>

using namespace std;

// SelectDBDialog dialog

class SelectDBDialog : public CDialog
{
	DECLARE_DYNAMIC(SelectDBDialog)

public:
	SelectDBDialog(UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFAMDB, string strPrompt, 
		bool bAllowCreation, bool bRequireAdminLogin, CWnd* pParent = NULL);
	virtual ~SelectDBDialog();

// Dialog Data
	enum { IDD = IDD_DIALOG_SELECT_DB_TO_ADMINISTER };

	// Enum for the radio buttons for existing or create new database
	enum EDBOptions : int 
	{
		kLoginExisting	= 0,
		kCreateNew		= 1
	};

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();
	virtual void OnOK();
	virtual void OnCancel(); // Stubbed in to prevent dialog from closing on esc
	virtual void OnClose();

	DECLARE_MESSAGE_MAP()
public:
	DBInfoCombo m_comboServerName;
	DBInfoCombo m_comboDBName;
	CString m_zServerName;
	CString m_zDBName;
	CString m_zAdvConnStrProperties;
	EDBOptions m_eOptionsDatabaseGroup;
	CString m_zPrompt;
	bool m_bAllowCreation;
	bool m_bRequireAdminLogin;

	afx_msg void OnCbnKillfocusComboSelectDbServer();
	afx_msg void OnBnClickedClose();
	afx_msg void OnBnClickedButtonAdvanced();
	afx_msg void OnChangeServerName();
	afx_msg void OnChangeDBName();
	afx_msg void OnSelChangeServerName();
	afx_msg void OnSelChangeDBName();

private:
	HICON m_hIcon;

	// FAMDB pointer to perform operations on the database
	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr m_ipFAMDB;

	unique_ptr<FileProcessingConfigMgr> ma_pCfgMgr;

	// If advance connection properties are specified and they reference the server or database,
	// update it with the current server and database name.
	void updateAdvConnStrProperties();

	// Hides the radio buttons to choose between loggin into an existing DB or creating a new one.
	// (logging into an existing one will be the only option)
	void hideCreationOption();
};
