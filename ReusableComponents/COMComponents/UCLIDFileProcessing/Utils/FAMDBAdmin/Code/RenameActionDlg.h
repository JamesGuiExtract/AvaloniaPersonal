#pragma once

#include "resource.h"
#include <string>
using namespace std;

/////////////////////////////////////////////////////////////////////////////
// RenameActionDlg dialog

class RenameActionDlg : public CDialog
{
	DECLARE_DYNAMIC(RenameActionDlg)

public:
// constructor
	RenameActionDlg(UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFPMDB,
					CWnd* pParent = NULL);   // standard constructor
	virtual ~RenameActionDlg();

// Dialog Data
	enum { IDD = IDD_DLG_RENAME_ACTION };
	CComboBox m_CMBAction;
	CEdit m_EditNewAction;

// Method
	DWORD GetOldNameAndNewName( string&  strOld, string& strNew);

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();
	afx_msg void OnCbnSelchangeCmbActionNames();
	afx_msg void OnBnClickedOK();
	DECLARE_MESSAGE_MAP()

private:
	/////////////
	//Variable
	////////////

	// The selected action name from database (old name)
	CString m_zSelectedActionName;

	// The selected action ID
	DWORD m_dwSelectedActionID;

	// the action's new name
	CString m_zNewActionName;

	// The file processing manager DB pointer
	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr m_ipFPMDB;

	///////////
	//Methods
	//////////
};
