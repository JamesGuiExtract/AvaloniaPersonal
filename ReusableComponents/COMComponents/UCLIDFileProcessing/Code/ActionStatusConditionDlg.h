#pragma once

#include "resource.h"
#include "afxwin.h"
#include "ActionStatusCondition.h"

#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// ActionStatusConditionDlg dialog
//-------------------------------------------------------------------------------------------------
class ActionStatusConditionDlg : public CDialog
{
public:
	ActionStatusConditionDlg(const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr& ipFAMDB);
	ActionStatusConditionDlg(const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr& ipFAMDB,
		const ActionStatusCondition& settings);
	~ActionStatusConditionDlg(void);

	ActionStatusCondition getSettings() { return m_settings; }

	// Dialog Data
	enum { IDD = IDD_DLG_ACTION_STATUS_CONDITION };
	CComboBox m_comboFilesUnderAction;
	CComboBox m_comboFilesUnderStatus;
	CComboBox m_comboUser;

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
	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr m_ipFAMDB;

	// The settings chosen in the dialog
	ActionStatusCondition m_settings;

	////////////
	// Methods
	///////////
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To fill in the by user combo box using the query and the field name
	void fillComboBoxFromDB(CComboBox &rCombo, string strQuery, string fieldName);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To fill the given combo box with data from the map the key will be shown and the value
	//			will be set ast the item data
	void fillComboBoxFromMap(CComboBox& rCombo, IStrToStrMapPtr ipMapData);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: to fill the by user combo box for selecting files in a user queue
	void fillByUserWithFAMUsers();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To update the controls based on the settings object
	void setControlsFromSettings();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To save the selected dialog settings to the settings object
	bool saveSettings();
};

