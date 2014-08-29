#pragma once

#include "resource.h"
#include "FileSetCondition.h"

//-------------------------------------------------------------------------------------------------
// FileSetConditionDlg
//-------------------------------------------------------------------------------------------------
class FileSetConditionDlg : public CDialog
{
public:
	FileSetConditionDlg(const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr& ipFAMDB);
	FileSetConditionDlg(const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr& ipFAMDB,
						const FileSetCondition& settings);
	~FileSetConditionDlg(void);

	FileSetCondition getSettings() { return m_settings; }

// Dialog Data
	enum { IDD = IDD_DLG_FILE_SET_CONDITION };

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

	/////////////
	// Variables
	/////////////

	CComboBox m_cmbFileSet;

	// The file action manager DB pointer
	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr m_ipFAMDB;

	// The settings chosen in the dialog
	FileSetCondition m_settings;

	/////////////
	// Methods
	/////////////

	// Update the controls based on the m_settings.
	void setControlsFromSettings();

	// Save the selected dialog settings to m_settings.
	bool saveSettings();
};