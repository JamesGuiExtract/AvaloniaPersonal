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
	CSelectFilesDlg(UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFAMDB,
		const string& strSectionHeader, const string& strQueryLabel,
		const SelectFileSettings& settings);
	~CSelectFilesDlg();

// Methods
	// Return the settings from the dialog
	SelectFileSettings getSettings() { return m_settings; }

// Dialog Data
	enum { IDD = IDD_DLG_SELECT_FILES };
	CStatic m_grpSelectFor;
	CComboBox m_cmbConditionType;
	CListCtrl m_listConditions;
	CButton m_btnModifyCondition;
	CButton m_btnDeleteCondition;
	CButton m_checkSubset;
	CEdit m_editSubsetSize;
	CComboBox m_comboSubsetMethod;
	CComboBox m_comboSubsetUnits;
	CButton m_cmbAnd;
	CButton m_cmbOr;

// Implementation
protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

	// Message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnClickedCheckSubset();
	afx_msg void OnBnClickedBtnAddCondition();
	afx_msg void OnBnClickedBtnModifyCondition();
	afx_msg void OnBnClickedBtnDeleteCondition();
	afx_msg void OnNMDblclkListConditions(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnLvnItemChangedListConditions(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnBnClickedConjunction();
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
	// PURPOSE: Displays configuration for a new condition of type T on the set of files to be
	// selected.
	template <class T>
	void addCondition(T* pCondition);
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
