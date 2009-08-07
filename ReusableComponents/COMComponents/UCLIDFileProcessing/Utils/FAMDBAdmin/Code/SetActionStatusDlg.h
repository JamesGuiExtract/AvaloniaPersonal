//-------------------------------------------------------------------------------------------------
// CSetActionStatusDlg dialog used for FAMDBAdmin
//-------------------------------------------------------------------------------------------------

#pragma once

#include "resource.h"
#include "afxwin.h"
#include "SelectFileSettings.h"

// SetActionStatusDlg.h : header file
//

//-------------------------------------------------------------------------------------------------
// CSetActionStatusDlg dialog
//-------------------------------------------------------------------------------------------------
class CSetActionStatusDlg : public CDialog
{
public:
// Construction
	CSetActionStatusDlg(UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFAMDB);

// Dialog Data
	//{{AFX_DATA(CSetActionStatusDlg)
	enum { IDD = IDD_DLG_SET_ACTION_STATUS };
	//}}AFX_DATA

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CSetActionStatusDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CSetActionStatusDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnClickedRadioNewStatus();
	afx_msg void OnClickedRadioStatusOfAction();
	afx_msg void OnClickedOK();
	afx_msg void OnClickedApply();
	afx_msg void OnClickedSelectFiles();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	////////////
	//Variables
	///////////
	CComboBox m_comboActions;
	CEdit m_editSummary;
	CButton m_radioNewStatus;
	CButton m_radioStatusFromAction;
	CButton m_btnOK;
	CButton m_btnApply;
	CComboBox m_comboNewStatus;
	CComboBox m_comboStatusFromAction;

	// The file action manager DB pointer
	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr m_ipFAMDB;

	SelectFileSettings m_settings;

	////////////
	//Methods
	///////////

	//---------------------------------------------------------------------------------------------
	// PROMISE: Applies user-specified changes to the action status. 
	//          Closes the dialog if bCloseDialog is true.
	// PROMISE: Changes the user-specified action status for the user-specified files to the 
	//          user-specified value. Displays a modal dialog, confirming the user's changes.
	//          Closes the dialog if bCloseDialog is true, otherwise the dialog remains open.
	void applyActionStatusChanges(bool bCloseDialog);

	//---------------------------------------------------------------------------------------------
	// Update controls
	void updateControls();
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
