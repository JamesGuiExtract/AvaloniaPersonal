#pragma once
#include <string>
#include <vector>
#include "resource.h"
using namespace std;

/////////////////////////////////////////////////////////////////////////////
// SelectActionDlg dialog

class SelectActionDlg : public CDialog
{
	DECLARE_DYNAMIC(SelectActionDlg)

public:
// constructor
	SelectActionDlg(UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFPMDB,
						string strCaption, string strAction, CWnd* pParent = NULL);  
	virtual ~SelectActionDlg();

// Dialog Data
	enum { IDD = IDD_DLG_SELECT_ACTION };
	CComboBox m_CMBAction;
	CStatic m_StaticLabel;

// Method
	// Get the action name and ID that currently Selected
	// e.g. the action that is to be selected, removed or resetted
	void GetSelectedAction(string& strAction, DWORD& iID);

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();
	afx_msg void OnCbnSelchangeCmbAction();
	afx_msg void OnBnClickOK();
	DECLARE_MESSAGE_MAP()
private:
	/////////////
	//Variable
	////////////

	// Caption of the dialog
	std::string m_strCaption;

	// The action that is now used for FPM
	std::string m_strPrevAction;
	
	// The action that is used for selecting, removing or reset
	string m_strSelectedActionName;

	// The action ID that is used for selecting, removing or reset
	DWORD m_dwSelectedActionID;

	// The file processing manager DB pointer
	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr m_ipFPMDB;

	///////////
	//Methods
	//////////
};
