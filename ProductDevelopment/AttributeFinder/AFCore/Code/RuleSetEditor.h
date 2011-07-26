//============================================================================
//
// COPYRIGHT (c) 2003 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	RuleSetEditor.h
//
// PURPOSE:	Declaration of CRuleSetEditor class
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//============================================================================

#pragma once

#include "resource.h"
#include "afxwin.h"
#include "RuleGrid.h"

#include <FileRecoveryManager.h>

#include <memory>
#include <string>
#include <stack>

using namespace std;

// forward declarations
class RuleTesterDlg;
class PromptDlg;
class IConfigurationSettingsPersistenceMgr;
class MRUList;

/////////////////////////////////////////////////////////////////////////////
// CRuleSetEditor dialog

class CRuleSetEditor : public CDialog
{
	typedef enum EEditorControlSelected
	{
		kNoControl,
		kAttributes,
		kRulesList,
//		kIgnoreList,
		kValidator,
		kSplitter,
		kPreprocessor,
		kOutputHandler
	}	EEditorControlSelected;

// Construction
public:
	CRuleSetEditor(const string& strFileName = "", const string& strProductRootDir = "", CWnd* pParent = __nullptr);

// Dialog Data
	//{{AFX_DATA(CRuleSetEditor)
	enum { IDD = IDD_DLG_EDITOR };
	CButton m_btnSelectAttributeSplitter;
	CButton	m_btnSelectPreprocessor;
	CButton	m_btnSelectIV;
	CButton	m_btnSelectOH;
	CButton	m_btnAddRule;
	CButton	m_btnRuleUp;
	CButton	m_btnRuleDown;
	CButton	m_btnConRule;
	CButton	m_btnDelRule;
	CRuleGrid m_listRules;
	CButton	m_btnDelAttr;
	CButton	m_btnRenAttr;
	CComboBox	m_comboAttr;
	BOOL	m_bStopWhenValueFound;
	BOOL	m_bDocumentPP;
	BOOL	m_bIgnoreDocumentPPErrors;
	BOOL	m_bOutputHandler;
	BOOL	m_bIgnoreOutputHandlerErrors;
	BOOL	m_bInputValidator;
	BOOL	m_bAttSplitter;
	BOOL	m_bIgnoreAttSplitterErrors;
	CString	m_zPPDescription;
	CString	m_zIVDescription;
	CString	m_zAttributeSplitterDescription;
	CString	m_zOutputHandlerDescription;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CRuleSetEditor)
	public:
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	//{{AFX_MSG(CRuleSetEditor)
	afx_msg void OnFileNew();
	afx_msg void OnFileOpen();
	afx_msg void OnFileSave();
	afx_msg void OnFileSaveas();
	afx_msg void OnFileExit();
	afx_msg void OnHelpAbout();
	afx_msg void OnHelpHelp();
	afx_msg void OnToolsCheck();
	afx_msg void OnToolsTest();
	afx_msg void OnToolsHarness();
	afx_msg void OnBtnAddAttribute();
	afx_msg void OnBtnDeleteAttribute();
	afx_msg void OnBtnRenameAttribute();
	virtual BOOL OnInitDialog();
	afx_msg void OnBtnAddRule();
	afx_msg void OnBtnDeleteRule();
	afx_msg void OnBtnConfigureRule();
	afx_msg void OnBtnRuleUp();
	afx_msg void OnBtnRuleDown();
	afx_msg void OnBtnSelectIV();
	afx_msg LRESULT OnDblclkListRules(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT OnCellValueChange(WPARAM wParam, LPARAM lParam);
	afx_msg void OnSelchangeComboAttributes();
	afx_msg void OnCheckStop();
	afx_msg void OnClose();
	afx_msg void OnOK();
	afx_msg void OnCancel();
	afx_msg void OnDropFiles( HDROP hDropInfo );
	afx_msg void OnBtnSelectAttributeSplitter();
	afx_msg LRESULT OnRclickListRules(WPARAM wParam, LPARAM lParam);
	afx_msg void OnEditCut();
	afx_msg void OnEditCopy();
	afx_msg void OnEditPaste();
	afx_msg void OnEditDelete();
	afx_msg void OnRclickComboAttributes(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg int OnMouseActivate(CWnd* pDesktopWnd, UINT nHitTest, UINT message);
	afx_msg BOOL OnSetCursor(CWnd* pWnd, UINT nHitTest, UINT message);
	afx_msg void OnRclickValidatorText(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnRclickSplitterText(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnRclickPreprocessorText(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnTimer(UINT nIDEvent);
	afx_msg void OnEditDuplicate();
	afx_msg void OnBtnSelectPreprocessor();
	afx_msg LRESULT OnItemchangedListRules(WPARAM wParam, LPARAM lParam);
	afx_msg void OnBtnSelectOutputHandler();
	afx_msg void OnRclickHandlerText(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnFileProperties();
	afx_msg void OnBnClickedCheckDocumentPp();
	afx_msg void OnBnClickedIgnorePpErrors();
	afx_msg void OnBnClickedCheckInputValidator();
	afx_msg void OnBnClickedCheckAttSplitter();
	afx_msg void OnBnClickedIgnoreAttSplitterErrors();
	afx_msg void OnBnClickedCheckOutputHandler();
	afx_msg void OnBnClickedIgnoreOutputHandlerErrors();
	//}}AFX_MSG
	afx_msg void OnSelectMRUMenu(UINT nID);
	afx_msg void OnDoubleClickDocumentPreprocessor();
	afx_msg void OnDoubleClickInputValidator();
	afx_msg void OnDoubleClickAttributeSplitter();
	afx_msg void OnDoubleClickOutputHandler();
	DECLARE_MESSAGE_MAP()

private:
	////////////
	// Variables
	////////////
	// the current ruleset being edited by this UI
	UCLID_AFCORELib::IRuleSetPtr m_ipRuleSet;

	// utility for auto-encrypting feature
	IMiscUtilsPtr m_ipMiscUtils;

	// The IPersistStream interface on the RuleSet object
	IPersistStreamPtr	m_ipRuleSetStream;

	// a pointer to the attribute-name to attribute-find-info map that is
	// a member variable of the ruleset object
	IStrToObjectMapPtr m_ipAttributeNameToInfoMap;

	// AttributeFindInfo object for currently selected Attribute
	UCLID_AFCORELib::IAttributeFindInfoPtr m_ipInfo;

	// the name of the last file opened
	string m_strLastFileOpened;

	// the file name for currently opened file
	string m_strCurrentFileName;

	IClipboardObjectManagerPtr m_ipClipboardMgr;

	// object that manages file recovery related functionality
	FileRecoveryManager m_FRM;

	unique_ptr<RuleTesterDlg> m_apRuleTesterDlg;

	// Which control owns the context menu
	EEditorControlSelected	m_eContextMenuCtrl;

	// Expected path to Test Harness executable
	string	m_strTestHarnessPath;

	// Expected path to SDK Root
	string	m_strBinFolder;

	// parent that creates rule set editor
	CWnd *m_pParentWnd;

	// the menu pointer to the main menu -> "File" menu -> Recent Files sub menu
	CMenu* m_pMRUFilesMenu;

	CStatusBar m_statusBar;

	// persistent manager
	unique_ptr<IConfigurationSettingsPersistenceMgr> ma_pUserCfgMgr;
	unique_ptr<MRUList> ma_pMRUList;

	//////////
	// Methods
	//////////

	// check for modification
	// Return : true -- checking succeeded and can continue with next step
	//			false -- checking succeeded and cannot continue with next step
	bool checkModification();

	// clears all selections in the list ctrl
	void clearListSelection();

	// return true if the current contents of the clipboard is an attribute
	// object
	bool clipboardObjectIsAttribute();

	// Enables/disables menu and UI items if Rule Set is or is not encrypted
	void enableEditFeatures(bool bEnable);

	// call this method to get attribute name
	bool promptForAttributeName(PromptDlg& promptDlg);

	// Refresh the UI from the selected Attribute
	void refreshUIFromAttribute();

	// Refresh the UI from the member variable ruleset object 
	void refreshUIFromRuleSet();

	// Enable/disable various buttons
	void setButtonStates();

	// a method to update the window caption depending upon the currently
	// loaded file (if any)
	void updateWindowCaption();

	// PURPOSE: open a .RSD file
	// REQUIRE: strFileName == "", or is a valid filename of a .RSD file
	// PROMISE: Will open the specified file if strFileName != "".  Will
	//			prompt user to save changes to current ruleset if appropriate.
	//			If strFileName == "", will prompt user to select filename with
	//			the standard file-open dialog box.
	void openFile(string strFileName);

	// Deletes Attribute rules that have been marked for deletion via SetItemData
	void deleteSelectedRules();

	// Adds Test Harness item to Tools menu
	// ONLY IF TestHarness object is licensed
	void updateToolsMenu();

	// process dropped files
	void processDroppedFile(char *pszFile);

	// add a file to the top of the MRU list
	void addFileToMRUList(const string& strFileToBeAdded);

	// remove the file from MRU list
	void removeFileFromMRUList(const string& strFileToBeRemoved);

	// Refresh the menu with the current MRU list
	void refreshFileMRU();

	// Updates the status bar text
	void setStatusBarText();

	//---------------------------------------------------------------------------------------------
	// PURPOSE: updates check boxes and an edit control based on the given object
	// REQUIRE: non-NULL arguments and a valid Check Box ID
	// PROMISE: sets the check box 1 state to correspond with whether the Object is enabled.
	//          disables the check box if the Object has description, otherwise enables.
	//          sets the edit control text to match the Object's description.
	//          refreshes the display of the check box and edit control.
	//			If uiCheckBoxID2 is provided, it is enabled/disabled based on the check state of
	//			uiCheckBoxID1.
	void updateCheckBoxAndEditControlBasedOnObject(IObjectWithDescription* pObject, 
		BOOL &bCheckBoxState, CString &zEditControlText, UINT uiCheckBoxID1, UINT uiCheckBoxID2 = 0);	

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Throws an exception if the rule set is not licensed or is encrypted and cannot be saved
	void validateRuleSetCanBeSaved();
};