//-------------------------------------------------------------------------------------------------
// CSetFilePriorityDlg dialog used for FAMDBAdmin
//-------------------------------------------------------------------------------------------------

#pragma once

#include "resource.h"
#include "afxwin.h"
#include "SelectFileSettings.h"

//-------------------------------------------------------------------------------------------------
// CSetFilePriorityDlg dialog
//-------------------------------------------------------------------------------------------------
class CSetFilePriorityDlg : public CDialog
{
public:
// Construction
	CSetFilePriorityDlg(IFileProcessingDBPtr pFAMDB);
	~CSetFilePriorityDlg();

// Dialog Data
	enum { IDD = IDD_DIALOG_SET_PROCESSING_PRIORITY };
	CEdit m_editSummary;
	CButton m_btnOk;
	CComboBox m_comboPriority;

// Overrides
	// ClassWizard generated virtual function overrides
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

// Implementation
protected:

	// Message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnClickedSelectFiles();
	afx_msg void OnClickedOK();
	DECLARE_MESSAGE_MAP()

private:
	////////////
	//Variables
	///////////
	// The file action manager DB pointer
	IFileProcessingDBPtr m_ipFAMDB;

	// Settings for the priorities to modify
	SelectFileSettings m_settings;

	////////////
	//Methods
	///////////
	// Populate the priority combo box
	void fillPriorityCombo();
};
