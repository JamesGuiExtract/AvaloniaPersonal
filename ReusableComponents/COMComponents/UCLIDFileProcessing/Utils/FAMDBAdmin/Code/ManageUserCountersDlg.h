//-------------------------------------------------------------------------------------------------
// ManageUserCountersDlg.h : header file
// CManageUserCountersDlg dialog used for FAMDBAdmin
//-------------------------------------------------------------------------------------------------

#pragma once

#include "resource.h"
#include "afxwin.h"

#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// CManageUserCountersDlg dialog
//-------------------------------------------------------------------------------------------------
class CManageUserCountersDlg : public CDialog
{
	DECLARE_DYNAMIC(CManageUserCountersDlg)

public:
// Construction
	CManageUserCountersDlg(const IFileProcessingDBPtr& ipFAMDB, CWnd* pParent = NULL);
	virtual ~CManageUserCountersDlg();

// Dialog Data
	enum { IDD = IDD_DIALOG_MANAGE_USER_COUNTERS };

// Implementation
protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL PreTranslateMessage(MSG* pMsg); 

	// Message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnBtnAdd();
	afx_msg void OnBtnDelete();
	afx_msg void OnBtnRename();
	afx_msg void OnBtnSetValue();
	afx_msg void OnBtnRefresh();
	afx_msg void OnBtnClose();
	afx_msg void OnNMDblclkList(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnLvnItemchangedList(NMHDR* pNMHDR, LRESULT* pResult);

	DECLARE_MESSAGE_MAP()

private:

	// Internal class used for adding/modifying counters
	class CAddModifyUserCountersDlg : public CDialog
	{
	public:
		enum { IDD = IDD_DIALOG_ADDMODIFY_COUNTER };

		CAddModifyUserCountersDlg(const string& strCounterToModify,
			LONGLONG llValueToModify, bool bAllowModifyName, bool bAllowModifyValue,
			CWnd* pParent = NULL);
		virtual ~CAddModifyUserCountersDlg();

		string getCounterName() { return m_strCounterName; }
		LONGLONG getValue() { return m_llValue; }

	protected:
		virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
		virtual BOOL OnInitDialog();
		afx_msg void OnOK();
		afx_msg void OnBtnOK();
		afx_msg void OnBtnCancel();
		DECLARE_MESSAGE_MAP()

	private:
		string m_strCounterName;
		LONGLONG m_llValue;
		bool m_bEnableCounterName;
		bool m_bEnableCounterValue;

		CEdit m_editCounterName;
		CEdit m_editCounterValue;
	};


	////////////
	//Variables
	///////////

	CListCtrl m_listCounters;
	CButton m_btnAdd;
	CButton m_btnDelete;
	CButton m_btnRename;
	CButton m_btnSetValue;
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
	// PURPOSE: To configure the list of tags
	void configureCounterList();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To refresh the list of tags with the list contained in the database
	void refreshCounterList(const string& strNameToSelect = "");
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To leave all items in the list in an unselected state
	void clearListSelection();
	//---------------------------------------------------------------------------------------------
};
