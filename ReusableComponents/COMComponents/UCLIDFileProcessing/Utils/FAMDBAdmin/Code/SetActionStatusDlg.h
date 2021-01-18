//-------------------------------------------------------------------------------------------------
// CSetActionStatusDlg dialog used for FAMDBAdmin
//-------------------------------------------------------------------------------------------------

#pragma once

#include "resource.h"
#include "afxwin.h"
#include "FAMDBAdminDlg.h"

// SetActionStatusDlg.h : header file
//

//-------------------------------------------------------------------------------------------------
// CSetActionStatusDlg dialog
//-------------------------------------------------------------------------------------------------
class CSetActionStatusDlg : public CDialog
{
public:
// Construction
	CSetActionStatusDlg(UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFAMDB,
		CFAMDBAdminDlg* pFAMDBAdmin);
	CSetActionStatusDlg(UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFAMDB,
		CFAMDBAdminDlg* pFAMDBAdmin, UCLID_FILEPROCESSINGLib::IFAMFileSelectorPtr ipFileSelector);
	~CSetActionStatusDlg();

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
	CComboBox m_comboNewStatus;
	CComboBox m_comboStatusFromAction;

	// Used to notify the db admin to update the summary tab
	CFAMDBAdminDlg* m_pFAMDBAdmin;

	// The file action manager DB pointer
	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr m_ipFAMDB;

	// Allows the selection of files whose file action status is to be changed.
	UCLID_FILEPROCESSINGLib::IFAMFileSelectorPtr m_ipFileSelector;

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

	// In the case that applyActionStatusChanges errored because the target action did not exists
	// in the workflow of all affected files, prompt user for what to do.
	// RETURNS: true if user selected to move files that could be move, false if no files should be moved.
	bool CSetActionStatusDlg::handleCantMoveFilesForAllWorkflows(UCLIDException& ueModifyError,
		CString& zToActionName, EActionStatus eNewStatus, CString& zFromAction);

	//---------------------------------------------------------------------------------------------
	// Update controls
	void updateControls();
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
