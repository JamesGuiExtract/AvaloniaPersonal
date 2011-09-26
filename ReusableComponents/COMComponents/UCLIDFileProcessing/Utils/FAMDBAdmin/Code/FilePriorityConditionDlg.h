#pragma once

#include "resource.h"
#include "afxwin.h"
#include "FilePriorityCondition.h"

#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// FilePriorityConditionDlg dialog
//-------------------------------------------------------------------------------------------------
class FilePriorityConditionDlg : public CDialog
{
public:
	FilePriorityConditionDlg(const IFileProcessingDBPtr& ipFAMDB);
	FilePriorityConditionDlg(const IFileProcessingDBPtr& ipFAMDB, const FilePriorityCondition& settings);
	~FilePriorityConditionDlg(void);

	FilePriorityCondition getSettings() { return m_settings; }

	// Dialog Data
	enum { IDD = IDD_DLG_PRIORITY_CONDITION };
	CComboBox m_comboPriority;

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
	FilePriorityCondition m_settings;

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

