//============================================================================
//
// COPYRIGHT (c) 2003 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	AddRuleDlg.h
//
// PURPOSE:	Declaration of CAddRuleDlg class
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//============================================================================

#pragma once

#include "resource.h"

#include <string>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CAddRuleDlg dialog
class CAddRuleDlg : public CDialog
{
	typedef enum EControlSelected
	{
		kNoControl,
		kRulesList,
		kPreprocessor
	}	EControlSelected;

// Construction
public:
	CAddRuleDlg(IClipboardObjectManagerPtr ipClipboardMgr,
		UCLID_AFCORELib::IAttributeRulePtr ipRule, CWnd* pParent = __nullptr);

	// Set the prompt text
	void	SetPromptText(string strPrompt);

// Dialog Data
	//{{AFX_DATA(CAddRuleDlg)
	enum { IDD = IDD_DLG_ADDRULE };
	CStatic	m_lblConfigure;
	CListCtrl	m_listRules;
	CComboBox	m_comboRule;
	CButton	m_btnRuleUp;
	CButton	m_btnRuleDown;
	CButton	m_btnDelRule;
	CButton	m_btnConRule2;
	CButton	m_btnConRule;
	CButton	m_btnAddRule;
	CButton	m_btnSelectPreprocessor;
	BOOL	m_bApplyMod;
	BOOL    m_bDocPP;
	CString	m_zDescription;
	CString	m_zPrompt;
	CString	m_zPPDescription;
	BOOL	m_bIgnoreDocPPErrors;
	BOOL	m_bIgnoreModErrors;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CAddRuleDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CAddRuleDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnBtnConfigureRule2();
	afx_msg void OnBtnAddRule();
	afx_msg void OnBtnDeleteRule();
	afx_msg void OnBtnConfigureRule();
	afx_msg void OnBtnRuleUp();
	afx_msg void OnBtnRuleDown();
	afx_msg void OnBtnSelectPreprocessor();
	virtual void OnOK();
	afx_msg void OnCheckModify();
	afx_msg void OnSelchangeComboRule();
	afx_msg void OnClickListRules(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnDblclkListRules(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg int OnMouseActivate(CWnd* pDesktopWnd, UINT nHitTest, UINT message);
	afx_msg void OnRclickListRules(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnRclickEditPreprocessor(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnEditCut();
	afx_msg void OnEditCopy();
	afx_msg void OnEditPaste();
	afx_msg void OnEditDelete();
	afx_msg BOOL OnSetCursor(CWnd* pWnd, UINT nHitTest, UINT message);
	afx_msg void OnItemchangedListRules(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnChangeEditDesc();
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnGetMinMaxInfo(MINMAXINFO* pMMI);
	afx_msg void OnBnClickedCheckAfruleDocPp();
	//}}AFX_MSG
	afx_msg void OnDoubleClickDocumentPreprocessor();
	DECLARE_MESSAGE_MAP()

private:
	///////////////
	// Data members
	///////////////
	// Encapsulation of description, enabled flag, ValueFindingRule, and 
	// ModifyingRule collection and Document Preprocessor
	UCLID_AFCORELib::IAttributeRulePtr m_ipRule;

	// Pointer to object selected in combo box
	UCLID_AFCORELib::IAttributeFindingRulePtr	m_ipAFRule;

	// Pointer to Document Preprocessor and description
	IObjectWithDescriptionPtr m_ipDocPreprocessor;

	// Stores association between registered Object names and ProgIDs.
	// The collection of names populates the combo box.
	IStrToStrMapPtr	m_ipAFRulesMap;

	// Pointer to collection of Attribute Modifying Rules 
	// each is an IObjectWithDescription (AMRule + Description)
	IIUnknownVectorPtr	m_ipAMRulesVector;

	IClipboardObjectManagerPtr m_ipClipboardMgr;

	// Which control owns the context menu
	EControlSelected	m_eContextMenuCtrl;

	// Have the dialog's controls been instantiated yet - allows for resize
	// and repositioning
	bool			m_bInitialized;

	// utility for handling plug-in object double-click events
	IMiscUtilsPtr m_ipMiscUtils;

	////////////////
	// Local methods
	////////////////
	// clears all selections in the list ctrl
	void clearListSelection();

	// Deletes VM Rules that have been marked for deletion via SetItemData
	void	deleteMarkedRules();

	// Provide object pointer for specified item
	UCLID_AFCORELib::IAttributeFindingRulePtr	getObjectFromName(std::string strName);

	// Uses SetItemData to mark selected AM Rules for later deletion
	void	markSelectedRules();

	// Set up column header
	void	prepareList();

	// Populate the combo box
	void	populateCombo();

	//---------------------------------------------------------------------------------------------
	// PURPOSE: replaces the rule located in the rule list box at iIndex with
	//          the rule given
	// REQUIRE: iIndex is the index of the rule to replace in the rule list box
	//          pNewRule is a non-NULL ObjectWithDescription
	// PROMISE: deletes the old rule at iIndex.
	//          inserts the new rule at iIndex.
	//          updates associated buttons (e.g. insert, modify) appropriately.
	//          refreshes the dialog.
	void replaceRuleInListAt(int iIndex, IObjectWithDescriptionPtr ipNewRule);

	// Enable/disable various buttons
	void	setButtonStates();

	// Set the description
	void	setDescription();

	// Set the Document Preprocessor and the edit box
	void	setPreprocessor();

	// Set the AttributeFindingRule
	void	setAFRule();

	// Set the AttributeModifyingRules and the checkbox
	void	setAMRules();

	// Show or hide the static text that tells the user the selected Attribute 
	// Finding Rule still needs to be configured
	void	showReminder();

	//---------------------------------------------------------------------------------------------
	// PURPOSE: updates the preprocessor check box and edit control
	//          using corresponding values in m_ipDocPreprocessor
	// REQUIRE: m_ipDocPreprocessor must be non-NULL
	// PROMISE: sets the checkbox according to whether m_ipDocPreprocessor is enabled.
	//          puts m_ipDocPreprocessor's description in the edit control box.
	//          refreshes the dialog.
	void updatePreprocessorCheckBoxAndEditControl();

};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
