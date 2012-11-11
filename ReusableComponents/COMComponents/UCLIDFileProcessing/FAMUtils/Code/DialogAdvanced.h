#pragma once

#include "FAMUtils.h"

#include <string>

using namespace std;

// CDialogAdvanced dialog
class FAMUTILS_API CDialogAdvanced : public CDialog
{
	DECLARE_DYNAMIC(CDialogAdvanced)

public:
	// This constructor causes object to use database names in combo box
	CDialogAdvanced(const string& strServer, const string& strDatabase,
		const string& strAdvConnStrProperties, CWnd*pParent = NULL);
	
	virtual ~CDialogAdvanced();

// Dialog Data
	enum { IDD = IDD_DIALOG_CONN_STR };

	// override DoModal so that the correct resource template
	// is always used.
	virtual INT_PTR DoModal();

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();

	DECLARE_MESSAGE_MAP()

	// Message Handlers
	afx_msg void OnBnClickedButtonDefault();

	// Control value
	CString m_zAdvConnStrProperties;

public:	

	//
	bool getServer(string &rstrServer);

	bool getDatabase(string &rstrDatabase);

	string getAdvConnStrProperties();

private:

	string m_strServer;
	string m_strDatabase;
};
