#pragma once

#include "resource.h"
#include "afxwin.h"
#include "QueryCondition.h"

#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// QueryConditionDlg dialog
//-------------------------------------------------------------------------------------------------
class QueryConditionDlg : public CDialog
{
public:
	QueryConditionDlg(const IFileProcessingDBPtr& ipFAMDB);
	QueryConditionDlg(const IFileProcessingDBPtr& ipFAMDB, const QueryCondition& settings,
		const string& strQueryHeader);
	~QueryConditionDlg(void);

	QueryCondition getSettings() { return m_settings; }

	// Dialog Data
	enum { IDD = IDD_DLG_QUERY_CONDITION };
	CStatic m_lblQuery;
	CEdit m_editSelectQuery;

	// Implementation
protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

	// Message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnClose();
	afx_msg void OnOK();
	afx_msg void OnClickedOK();
	afx_msg void OnClickedCancel();

	DECLARE_MESSAGE_MAP()

private:

	////////////
	//Variables
	///////////
	// The file action manager DB pointer
	IFileProcessingDBPtr m_ipFAMDB;

	// The settings chosen in the dialog
	QueryCondition m_settings;

	// The text displayed above the query editor box
	string m_strQueryHeader;

	////////////
	// Methods
	///////////
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To update the controls based on the settings object
	void setControlsFromSettings();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To save the selected dialog settings to the settings object
	bool saveSettings();
};

