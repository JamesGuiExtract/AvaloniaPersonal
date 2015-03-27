//-------------------------------------------------------------------------------------------------
// ManageMetadataFieldsDlg.h : header file
// CManageMetadataFieldsDlg dialog used for FAMDBAdmin
//-------------------------------------------------------------------------------------------------

#pragma once

#include "resource.h"
#include "afxwin.h"

#include <string>
#include "afxcmn.h"

using namespace std;

//-------------------------------------------------------------------------------------------------
// CManageMetadataFieldsDlg dialog
//-------------------------------------------------------------------------------------------------
class CManageMetadataFieldsDlg : public CDialog
{
	DECLARE_DYNAMIC(CManageMetadataFieldsDlg)

public:
// Construction
	CManageMetadataFieldsDlg(const IFileProcessingDBPtr& ipFAMDB, CWnd* pParent = NULL);
	virtual ~CManageMetadataFieldsDlg();

// Dialog Data
	enum { IDD = IDD_DIALOG_MANAGE_METADATA_FIELDS };

// Implementation
protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
		
	// Message map functions
	afx_msg void OnBtnAdd();
	afx_msg void OnBtnRemove();
	afx_msg void OnBtnRename();
	afx_msg void OnBtnRefresh();
	afx_msg void OnBtnClose();

	DECLARE_MESSAGE_MAP()

private:
	
	////////////
	//Variables
	///////////

	CButton m_btnAdd;
	CButton m_btnRemove;
	CButton m_btnRename;
	CButton m_btnRefresh;
	CButton m_btnClose;
	CListCtrl m_listMetadataFields;

	// The file action manager DB pointer
	IFileProcessingDBPtr m_ipFAMDB;

	////////////
	// Methods
	///////////
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To update controls
	void updateControls();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To configure the list of metadata fields
	void configureMetadataFieldList();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To refresh the list of metadata fields with the list contained in the database
	void refreshMetadataFieldList();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To leave all items in the list in an unselected state
	void clearListSelection();
	//---------------------------------------------------------------------------------------------
public:
	afx_msg void OnLvnItemchangedList(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnNMRDblclkList(NMHDR *pNMHDR, LRESULT *pResult);
	virtual BOOL OnInitDialog();
};
