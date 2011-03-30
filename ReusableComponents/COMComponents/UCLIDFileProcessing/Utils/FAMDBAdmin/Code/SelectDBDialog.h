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
	SelectDBDialog(IFileProcessingDBPtr ipFAMDB, CWnd* pParent = NULL);   // standard constructor
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
	EDBOptions m_eOptionsDatabaseGroup;

	afx_msg void OnBnClickedClose();

private:
	HICON m_hIcon;

	// FAMDB pointer to perform operations on the database
	IFileProcessingDBPtr m_ipFAMDB;

	unique_ptr<FileProcessingConfigMgr> ma_pCfgMgr;
public:
	afx_msg void OnCbnKillfocusComboSelectDbServer();
};
