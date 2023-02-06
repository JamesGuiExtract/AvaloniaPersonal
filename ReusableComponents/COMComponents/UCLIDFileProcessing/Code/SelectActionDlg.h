#pragma once

#include "resource.h"

#include <ImageButtonWithStyle.h>

#include <string>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// SelectActionDlg dialog
class SelectActionDlg : public CDialog
{
	DECLARE_DYNAMIC(SelectActionDlg)

public:
// constructor
	SelectActionDlg(UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFPMDB,
		string strCaption, string strAction, bool bAllowTags = false, CWnd* pParent = NULL);  
	virtual ~SelectActionDlg();

// Dialog Data
	enum { IDD = IDD_DLG_SELECT_ACTION };

// Method
	// Get the action name and ID that currently Selected
	// e.g. the action that is to be selected, removed or resetted
	string GetSelectedAction();

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();
	afx_msg void OnCbnSelchangeCmbAction();
	afx_msg void OnBnClickOK();
	afx_msg void OnBnClickActionTag();
	afx_msg void OnCbnSelEndCancel();
	DECLARE_MESSAGE_MAP()
private:
	/////////////
	//Variable
	////////////

	// Caption of the dialog
	string m_strCaption;

	// Action that is now used for FPM
	string m_strPrevAction;

	// Whether to allow document tags
	bool m_bAllowTags;
	
	// Action that is selected
	string m_strSelectedActionName;

	// Remembers the selection of action combo box
	DWORD m_dwActionSel;

	// File processing manager DB pointer
	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr m_ipFPMDB;

	// Controls
	CStatic m_staticLabel;
	CComboBox m_cmbAction;
	CImageButtonWithStyle m_btnActionTag;

	///////////
	//Methods
	//////////

	// Retrieves the name of the currently selected action from the combo box
	string getActionName();
	
	// Makes the specified dropdown list into an editable dropdown
	static void makeDropListEditable(CComboBox& cmbDropList);
};
