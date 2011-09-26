#pragma once

#include "resource.h"
#include "afxwin.h"
#include "FileTagCondition.h"

#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// FileTagConditionDlg dialog
//-------------------------------------------------------------------------------------------------
class FileTagConditionDlg : public CDialog
{
public:
	FileTagConditionDlg(const IFileProcessingDBPtr& ipFAMDB);
	FileTagConditionDlg(const IFileProcessingDBPtr& ipFAMDB, const FileTagCondition& settings);
	~FileTagConditionDlg(void);

	FileTagCondition getSettings() { return m_settings; }

	// Dialog Data
	enum { IDD = IDD_DLG_TAG_CONDITION };
	CComboBox m_comboTagsAnyAll;
	CListCtrl m_listTags;

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
	FileTagCondition m_settings;

	////////////
	// Methods
	///////////
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To update the controls based on the settings object
	void setControlsFromSettings();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To save the selected dialog settings to the settings object
	bool saveSettings();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To configure the tag list and fill it with the current tags from the database
	void configureAndPopulateTagList();
};

