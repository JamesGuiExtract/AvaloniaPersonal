//-------------------------------------------------------------------------------------------------
// ManageTagsDlg.h : header file
// CManageTagsDlg dialog used for FAMDBAdmin
//-------------------------------------------------------------------------------------------------

#pragma once

#include "resource.h"
#include "afxwin.h"

#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// CManageTagsDlg dialog
//-------------------------------------------------------------------------------------------------
class CManageTagsDlg : public CDialog
{
	DECLARE_DYNAMIC(CManageTagsDlg)

public:
// Construction
	CManageTagsDlg(const IFileProcessingDBPtr& ipFAMDB, CWnd* pParent = NULL);
	virtual ~CManageTagsDlg();

// Dialog Data
	enum { IDD = IDD_DIALOG_MANAGE_TAGS };

// Implementation
protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL PreTranslateMessage(MSG* pMsg); 

	// Message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnBtnAdd();
	afx_msg void OnBtnModify();
	afx_msg void OnBtnDelete();
	afx_msg void OnBtnRefresh();
	afx_msg void OnBtnClose();
	afx_msg void OnNMDblclkList(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnLvnItemchangedList(NMHDR* pNMHDR, LRESULT* pResult);

	DECLARE_MESSAGE_MAP()

private:

	// Internal class used for adding/modifying tags
	class CAddModifyTagsDlg : public CDialog
	{
	public:
		enum { IDD = IDD_DIALOG_ADDMODIFY_TAG };

		CAddModifyTagsDlg(const string& strTagToModify = "",
			const string& strDescriptionToModify = "", CWnd* pParent = NULL);
		virtual ~CAddModifyTagsDlg();

		string getTagName() { return m_strTagName; }
		void setTagName(const string& strTagName) { m_strTagName = strTagName; }
		string getDescription() { return m_strDescription; }
		void setDescription(const string& strDescription) { m_strDescription = strDescription; }

	protected:
		virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
		virtual BOOL OnInitDialog();
		afx_msg void OnOK();
		afx_msg void OnBtnOK();
		afx_msg void OnBtnCancel();
		DECLARE_MESSAGE_MAP()

	private:
		string m_strTagName;
		string m_strDescription;

		CEdit m_editTagName;
		CEdit m_editDescription;
	};


	////////////
	//Variables
	///////////

	CListCtrl m_listTags;
	CButton m_btnAdd;
	CButton m_btnModify;
	CButton m_btnDelete;
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
	void configureTagList();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To refresh the list of tags with the list contained in the database
	void refreshTagList();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To leave all items in the list in an unselected state
	void clearListSelection();
	//---------------------------------------------------------------------------------------------
};
