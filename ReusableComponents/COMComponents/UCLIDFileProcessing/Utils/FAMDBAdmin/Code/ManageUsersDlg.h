//-------------------------------------------------------------------------------------------------
// ManageUsersDlg.h : header file
// CManageUsersDlg dialog used for FAMDBAdmin
//-------------------------------------------------------------------------------------------------

#pragma once

#include "resource.h"
#include "afxwin.h"

#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// CManageUsersDlg dialog
//-------------------------------------------------------------------------------------------------
class CManageUsersDlg : public CDialog
{
	DECLARE_DYNAMIC(CManageUsersDlg)

public:
// Construction
	CManageUsersDlg(const IFileProcessingDBPtr& ipFAMDB, CWnd* pParent = NULL);
	virtual ~CManageUsersDlg();

// Dialog Data
	enum { IDD = IDD_DIALOG_MANAGE_USERS };

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
	afx_msg void OnBtnClearPassword();
	afx_msg void OnBtnClose();
	afx_msg void OnNMDblclkList(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnLvnItemchangedList(NMHDR* pNMHDR, LRESULT* pResult);

	DECLARE_MESSAGE_MAP()

private:
	////////////
	//Variables
	///////////

	CListCtrl m_listUsers;
	CButton m_btnAdd;
	CButton m_btnRemove;
	CButton m_btnRename;
	CButton m_btnRefresh;
	CButton m_btnClearPassword;

	// The file action manager DB pointer
	IFileProcessingDBPtr m_ipFAMDB;

	////////////
	// Methods
	///////////
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To update controls
	void updateControls();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To configure the list of users
	void configureUserList();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To refresh the list of users with the list contained in the database
	void refreshUserList();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To leave all items in the list in an unselected state
	void clearListSelection();
	//---------------------------------------------------------------------------------------------
};
