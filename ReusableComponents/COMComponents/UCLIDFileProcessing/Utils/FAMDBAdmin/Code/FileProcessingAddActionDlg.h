#pragma once

#include "resource.h"

/////////////////////////////////////////////////////////////////////////////
// FileProcessingAddActionDlg dialog

class FileProcessingAddActionDlg : public CDialog
{
	DECLARE_DYNAMIC(FileProcessingAddActionDlg)

public:
	FileProcessingAddActionDlg(UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFPMDB,
							   CWnd* pParent = NULL);   // standard constructor
	virtual ~FileProcessingAddActionDlg();

// Dialog Data
	enum { IDD = IDD_DLG_ADD_ACTION };
	CButton m_btnSetToStatus;
	CButton m_btnCopyFromStatus;
	CEdit m_edActionName;
	CComboBox m_cmbStatus;
	CComboBox m_cmbCopyStatus;

// Methods
	// Get the action name that need to be added to database
	CString GetActionName();
	// Get the default status for the new action 
	// if return -1, we need to copy status from another action
	int GetDefaultStatus();
	// Get the action ID from which the new action will copy the status
	DWORD GetCopyStatusActionID();

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();
	afx_msg void OnBnClickOK();
	afx_msg void OnBnClickedRdsetto();
	afx_msg void OnBnClickedRdcopyfrom();
	afx_msg void OnCbnSelchangeCmbStatus();
	afx_msg void OnCbnSelchangeCmbCopyFrom();
	DECLARE_MESSAGE_MAP()

private:

	////////////
	//Variables
	///////////

	// New Action's name
	CString m_zActionName;
	// Default status
	int m_iDefaultStatus;
	// the action ID from which the new action will copy status
	DWORD m_iActionID;

	// The file processing manager DB pointer
	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr m_ipFPMDB;

	///////////
	//Methods
	///////////
};
