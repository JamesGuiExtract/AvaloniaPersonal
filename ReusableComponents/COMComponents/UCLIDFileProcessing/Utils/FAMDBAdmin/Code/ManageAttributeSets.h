#pragma once

#include "resource.h"
#include "afxdialogex.h"
#include "afxcmn.h"
#include "PromptDlg.h"
#include <string>
using namespace std;

//-------------------------------------------------------------------------------------------------
// CManageAttributeSets dialog
//-------------------------------------------------------------------------------------------------
class CManageAttributeSets : public CDialogEx
{
	DECLARE_DYNAMIC(CManageAttributeSets)

public:
	CManageAttributeSets(const IFileProcessingDBPtr& ipFAMDB, CWnd* pParent = NULL);   // standard constructor
	virtual ~CManageAttributeSets();

// Dialog Data
	enum { IDD = IDD_DIALOG_MANAGE_ATTRIBUTE_SETS };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

	// Message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnBtnClose();
	afx_msg void OnBnClickedBtnAddAttributeSet();
	afx_msg void OnBnClickedBtnRenameAttributeSet();
	afx_msg void OnBnClickedBtnRefreshAttributeSets();
	afx_msg void OnBnClickedBtnRemoveAttributeSet();
	afx_msg void OnBnClickedBtnHistoryAttributeSets();
	afx_msg void OnLvnItemchangedListAttributeSetsToManage(NMHDR *pNMHDR, LRESULT *pResult);
	DECLARE_MESSAGE_MAP()

private:
	////////////
	//Variables
	///////////

	CListCtrl m_listAttributeSets;
	CButton m_btnAdd;
	CButton m_btnRename;
	CButton m_btnRemove;
	CButton m_btnHistory;
	CButton m_btnRefresh;
	
	// The file action manager DB pointer
	IAttributeDBMgrPtr m_ipAttributeDBMgr;

	////////////
	//Methods
	///////////
	void configureAttributeSetsList();

	void updateControls();

	void refreshAttributeSetsList();

	void clearListSelection();

	bool attributeSetExists(const string &attributeName);
};
