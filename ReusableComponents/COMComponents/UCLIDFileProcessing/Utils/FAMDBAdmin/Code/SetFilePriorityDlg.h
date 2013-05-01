//-------------------------------------------------------------------------------------------------
// CSetFilePriorityDlg dialog used for FAMDBAdmin
//-------------------------------------------------------------------------------------------------

#pragma once

#include "resource.h"
#include "afxwin.h"

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

	// Allows the selection of files whose priority is to be changed.
	UCLID_FILEPROCESSINGLib::IFAMFileSelectorPtr m_ipFileSelector;

	////////////
	//Methods
	///////////
	// Populate the priority combo box
	void fillPriorityCombo();
};
