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
	CEdit m_editOldAction;
	CEdit m_editNewAction;

	void SetOldActionName(const string &strOld);

// Method
	void GetNewActionName(string & strNew);

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();
	afx_msg void OnBnClickedOK();
	DECLARE_MESSAGE_MAP()

private:
	/////////////
	//Variable
	////////////

	// The old name of the action
	CString m_zOldActionName;

	// the action's new name
	CString m_zNewActionName;

	// The file processing manager DB pointer
	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr m_ipFPMDB;

	///////////
	//Methods
	//////////
};
