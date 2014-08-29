#pragma once

#include "resource.h"
#include "SpecifiedFilesCondition.h"
#include "afxwin.h"

//-------------------------------------------------------------------------------------------------
// SpecifiedFilesConditionDlg
//-------------------------------------------------------------------------------------------------
class SpecifiedFilesConditionDlg : public CDialog
{
public:
	SpecifiedFilesConditionDlg(const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr& ipFAMDB);
	SpecifiedFilesConditionDlg(const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr& ipFAMDB,
						 const SpecifiedFilesCondition& settings);
	~SpecifiedFilesConditionDlg(void);

	SpecifiedFilesCondition getSettings() { return m_settings; }

// Dialog Data
	enum { IDD = IDD_DLG_SPECIFIED_FILES_CONDITION };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

	// Message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnClose();
	afx_msg void OnOK();
	afx_msg void OnClickedOK();
	afx_msg void OnClickedCancel();
	afx_msg void OnClickedRadio();
	afx_msg void OnSetFocusSpecifiedFileNames(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnClickListFileNames(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnKeyDownListFileNames(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnItemChangedListFileNames(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnBeginLabelEditListFileNames(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnEndLabelEditListFileNames(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnClickedBtnAddFileName();
	afx_msg void OnClickedBtnModifyFileName();
	afx_msg void OnClickedBtnDeleteFileName();
	afx_msg void OnClickedBtnBrowseFileName();
	afx_msg void OnClickedBtnBrowseListFile();

	DECLARE_MESSAGE_MAP()

private:

	/////////////
	// Variables
	/////////////

	// Mapped control variables
	CButton m_btnStaticList;	
	CButton m_btnListFile;
	CListCtrl m_listFileNames;
	CButton m_btnAddFileName;
	CButton m_btnModifyFileName;
	CButton m_btnDeleteFileName;
	CButton m_btnBrowseFileName;
	CEdit m_editListFileName;
	CButton m_btnListFileBrowse;

	// The file action manager DB pointer
	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr m_ipFAMDB;

	// The settings chosen in the dialog
	SpecifiedFilesCondition m_settings;

	// The index of the last item selected in the file list control (whether or not it is still
	// selected).
	int m_nLastSelectedFile;

	// In order to prevent having to click on the file list control multiple times, it will enter
	// edit mode as soon as it gains focus via a click except in special cases where this value is
	// set to true.
	bool m_bIgnoreNextFocus;

	/////////////
	// Methods
	/////////////

	// Update the controls based on the m_settings.
	void setControlsFromSettings();

	// Save the selected dialog settings to m_settings.
	bool saveSettings();

	// Updates the states of controls based on current selections
	void updateControlStates();

	// Ensure the file list control's only column is sized properly to the width of the control.
	void updateGridWidth();

	// Gets the currently select item in the file list control.
	int getSelectedItem();
};