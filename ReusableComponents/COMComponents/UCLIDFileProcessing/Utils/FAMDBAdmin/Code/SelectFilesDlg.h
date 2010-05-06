//-------------------------------------------------------------------------------------------------
// SelectFilesDlg.h : header file
// CSelectFilesDlg dialog used for FAMDBAdmin
//-------------------------------------------------------------------------------------------------

#pragma once

#include "resource.h"
#include "afxwin.h"
#include "SelectFileSettings.h"

#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// CSelectFilesDlg dialog
//-------------------------------------------------------------------------------------------------
class CSelectFilesDlg : public CDialog
{
public:
// Construction
	CSelectFilesDlg(const IFileProcessingDBPtr& ipFAMDB, const string& strSectionHeader,
		const string& strQueryLabel, const SelectFileSettings& settings);
	~CSelectFilesDlg();

// Methods
	// Return the settings from the dialog
	SelectFileSettings getSettings() { return m_settings; }

// Dialog Data
	enum { IDD = IDD_DLG_SELECT_FILES };
	CStatic m_grpSelectFor;
	CStatic m_lblQuery;
	CButton m_radioAllFiles;
	CButton m_radioFilesForWhich;
	CButton m_radioFilesWithTags;
	CButton m_radioFilesFromQuery;
	CButton m_radioFilesWithPriority;
	CComboBox m_comboFilesUnderAction;
	CComboBox m_comboFilesUnderStatus;
	CComboBox m_comboSkippedUser;
	CComboBox m_comboTagsAnyAll;
	CComboBox m_comboPriority;
	CEdit m_editSelectQuery;
	CListCtrl m_listTags;
	CButton m_checkRandomSubset;
	CEdit m_editRandomAmount;
	CComboBox m_comboRandomSubsetUnits;

// Implementation
protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

	// Message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnClickedRadioAllFiles();
	afx_msg void OnClickedRadioFilesStatus();
	afx_msg void OnClickedRadioFilesFromQuery();
	afx_msg void OnClickedRadioFilesWithTags();
	afx_msg void OnClickedRadioFilesWithPriority();
	afx_msg void OnClickedCheckRandomSubset();
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
	IFileProcessingDBPtr m_ipFAMDB;

	// The header to be displayed in the top section of the dialog
	string m_strSectionHeader;

	// The text displayed above the query editor box
	string m_strQueryHeader;

	// The settings chosen in the dialog
	SelectFileSettings m_settings;

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
	// PURPOSE: To configure the tag list and fill it with the current tags from the database
	void configureAndPopulateTagList();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To fill the priority combo box
	void fillPriorities();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To save the selected dialog settings to the settings object
	bool saveSettings();
};
