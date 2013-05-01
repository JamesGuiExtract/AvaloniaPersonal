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
	CComboBox m_comboSkippedUser;

	// Implementation
protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

	// Message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnFilesUnderActionChange();
	afx_msg void OnFilesUnderStatusChange();
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
	// PURPOSE: To fill in the by user combo box for selecting files skipped by a particular user
	void fillSkippedUsers();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To update the controls based on the settings object
	void setControlsFromSettings();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To update controls
	void updateControls();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To save the selected dialog settings to the settings object
	bool saveSettings();
};

