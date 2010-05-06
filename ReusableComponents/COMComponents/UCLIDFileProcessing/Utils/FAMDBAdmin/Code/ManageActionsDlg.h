//-------------------------------------------------------------------------------------------------
// ManageActionsDlg.h : header file
// CManageActionsDlg dialog used for FAMDBAdmin
//-------------------------------------------------------------------------------------------------

#pragma once

#include "resource.h"
#include "afxwin.h"

#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// CManageActionsDlgdialog
//-------------------------------------------------------------------------------------------------
class CManageActionsDlg: public CDialog
{
	DECLARE_DYNAMIC(CManageActionsDlg)

public:
// Construction
	CManageActionsDlg(const IFileProcessingDBPtr& ipFAMDB, CWnd* pParent = NULL);
	virtual ~CManageActionsDlg();

// Dialog Data
	enum { IDD = IDD_DIALOG_MANAGE_ACTIONS };

// Implementation
protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL PreTranslateMessage(MSG* pMsg); 

	// Message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnBtnAdd();
	afx_msg void OnBtnRemove();
	afx_msg void OnBtnRename();
	afx_msg void OnBtnRefresh();
	afx_msg void OnBtnClose();
	afx_msg void OnNMDblclkList(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnLvnItemchangedList(NMHDR* pNMHDR, LRESULT* pResult);

	DECLARE_MESSAGE_MAP()

private:
	////////////
	//Variables
	///////////

	CListCtrl m_listActions;
	CButton m_btnAdd;
	CButton m_btnRemove;
	CButton m_btnRename;
	CButton m_btnRefresh;

	// The file action manager DB pointer
	IFileProcessingDBPtr m_ipFAMDB;

	////////////
	// Methods
	///////////
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To update controls
	void updateControls();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To configure the list of actions
	void configureActionList();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To refresh the list of actions with the list contained in the database
	void refreshActionList();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To leave all items in the list in an unselected state
	void clearListSelection();
	//---------------------------------------------------------------------------------------------
};
