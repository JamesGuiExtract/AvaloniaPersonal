#pragma once

#include "FAMUtils.h"
#include "DBInfoCombo.h"

#include <string>
#include <vector>

using namespace std;

// CDialogSelect dialog
class FAMUTILS_API CDialogSelect : public CDialog
{
	DECLARE_DYNAMIC(CDialogSelect)

public:
	// This constructor causes object to use database names in combo box
	CDialogSelect(const string& strServer, CWnd*pParent = NULL);
	
	// This constructor causes object to use Server names in combo box
	CDialogSelect(CWnd* pParent = NULL);   // standard constructor
	
	virtual ~CDialogSelect();

	// Specifies whether gstrDATABASE_SERVER_TAG should be show in the dropdown when the combo is
	// of type kServerName.
	void showDBServerTag(bool bShowDBServerTag);

	// Indicates whether gstrDATABASE_NAME_TAG should be show in the dropdown when the combo is
	// of type kDatabaseName.
	void showDBNameTag(bool bShowDBNameTag);

// Dialog Data
	enum { IDD = IDD_DIALOG_SELECT };

	// override DoModal so that the correct resource template
	// is always used.
	virtual INT_PTR DoModal();

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();

	DECLARE_MESSAGE_MAP()

private:
	// Control objects
	CStatic m_staticComboLabel;
	DBInfoCombo m_comboData;

	// This holds the server to find the db names
	string m_strServer;

	// Whether gstrDATABASE_SERVER_TAG should be show in the dropdown when the combo is of type
	// kServerName.
	bool m_bShowDBServerTag;

	// Whether gstrDATABASE_SERVER_TAG should be show in the dropdown when the combo is of type
	// kServerName.
	bool m_bShowDBNameTag;

public:
	// Control value
	CString m_zComboValue;
};
