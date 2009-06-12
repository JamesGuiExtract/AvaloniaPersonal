//============================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	OCRFilterSettingsDlg.h
//
// PURPOSE:	Dialog for OCR Filtering settings
//
// NOTES:	Current implementation is based on the assumption that all list 
//			boxes are not sorted. All items displayed are in the
//			sequence of how they are added.
//
// AUTHORS:	Duan Wang
//
//============================================================================
#pragma once
// OCRFilterSettingsDlg.h : header file
//

#include "resource.h"
#include "FilterSchemeDefinition.h"

#include <string>
#include <vector>
#include <map>

class OCRFilterSchemesDlg;

/////////////////////////////////////////////////////////////////////////////
// OCRFilterSettingsDlg dialog

class OCRFilterSettingsDlg : public CDialog
{
// Construction
public:
	//==============================================================================================
	// Constructor implies that the dialog is now editable for
	// creating/modifying FOD files
	OCRFilterSettingsDlg(FilterOptionsDefinitions* pFODs, CWnd* pParent = NULL); 
	//==============================================================================================
	// Constructor implies that the dialog is not editable for 
	// creating/modifying FSD files
	OCRFilterSettingsDlg(FilterOptionsDefinitions* pFODs, 
						 FilterSchemeDefinitions* pFSDs,
						 const std::string& strCurrentSchemeName,
						 CWnd* pParent = NULL);

// Dialog Data
	//{{AFX_DATA(OCRFilterSettingsDlg)
	enum { IDD = IDD_DLG_Settings };
	CButton	m_btnClearChoices;
	CButton	m_btnRemoveAffectedInputType;
	CButton	m_btnAddAffectedInputType;
	CListBox	m_listAffectedInputTypes;
	CListBox	m_listInputCategories;
	CButton	m_btnRemoveInputCategories;
	CButton	m_btnAddInputCategories;
	CButton	m_chkExactCase;
	CButton	m_chkAllUpper;
	CButton	m_chkAllLower;
	CButton	m_btnRemoveChoice;
	CButton	m_btnAddChoice;
	CEdit	m_editCharsAlwaysOn;
	CCheckListBox	m_chklistChoices;
	CString	m_strCharsAlwaysOn;
	BOOL	m_chkDisableFiltering;
	BOOL	m_bAllLower;
	BOOL	m_bAllUpper;
	BOOL	m_bExactCase;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(OCRFilterSettingsDlg)
	public:
	virtual int DoModal();
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(OCRFilterSettingsDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnBTNAddChoice();
	afx_msg void OnBTNRemoveChoice();
	afx_msg void OnCHKAllLower();
	afx_msg void OnCHKAllUpper();
	afx_msg void OnCHKEnableFiltering();
	afx_msg void OnCHKExact();
	afx_msg void OnChangeEDITCharsAlwaysOn();
	afx_msg void OnSelchangeLISTCategories();
	afx_msg void OnDblclkLISTChoices();
	afx_msg void OnSelchangeLISTInputTypes();
	afx_msg void OnCancel();
	virtual void OnOK();
	afx_msg void OnKillfocusLISTChoices();
	afx_msg void OnDblclkLISTInputTypes();
	afx_msg void OnDblclkLISTCategories();
	afx_msg void OnBTNAddAffectedInputType();
	afx_msg void OnBTNAddInputCategories();
	afx_msg void OnBTNClearChoices();
	afx_msg void OnBTNClose();
	afx_msg void OnBTNNew();
	afx_msg void OnBTNOpen();
	afx_msg void OnBTNRemoveAffectedInputType();
	afx_msg void OnBTNRemoveInputCategories();
	afx_msg void OnBTNSave();
	afx_msg void OnBTNSaveAs();
	afx_msg void OnSelchangeLISTInputCategories();
	afx_msg void OnDblclkLISTInputCategories();
	afx_msg void OnDblclkLISTAffectedInputTypes();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	friend class OCRFilterSchemesDlg;
	// **************************
	// Methods

	//==============================================================================================
	// Creates a new FOD and put it in the map
	// bCreateFile -- Whether or not to create the specified file
	void createFOD(const std::string& strFODFileName, bool bCreateFile = false);

	//==============================================================================================
	// Prompts user for new FOD name and then creates a new FOD using user provided FOD name
	void createNewFOD();

	//==============================================================================================
	// Get current module full path
	std::string getCurrentModuleFullPath();

	//==============================================================================================
	// Returns sub string choice id according the item index (0-based)
	// IMPORTANT!!!
	// The choice check list box is not sorted to any order.
	// All items displayed in the check list box of the choices
	// are in the order of the squence they are added.
	std::string getItemChoiceID(unsigned int nItemIndex);

	//==============================================================================================
	// Initialize dialog state, i.e. whether the dlg is
	// opened as editable or not
	void initDialogState();

	//==============================================================================================
	// Inspects whether or not current choice check list is changed or not
	void inspectCurrentChoiceCheckList();

	//==============================================================================================
	// Modifies existing or creates new choice info
	void modifyChoiceInfo(bool bCreateNew = false);

	//==============================================================================================
	// Uses PromptDlg for entering a string
	// strTitle -- title for the prompt dlg
	// strPrompt -- prompt text for the dlg
	// strEditString -- what shall fill the edit box when the dlg is brought up
	std::string promptForData(const std::string& strTitle, 
							  const std::string& strPrompt,
							  const std::string& strEditString = "");

	//==============================================================================================
	// Remove the specified input category (FOD) from the map and delete its
	// associated file from hard disk permanently
	// if success, return true
	bool removeFOD(const std::string& strInputCategory);

	//==============================================================================================
	// Saves the choice info in the FOD
	void saveChoiceInfo(const std::string& strChoiceID, 
						const std::string& strChoiceDescription,
						const std::string& strChoiceChars);

	//==============================================================================================
	// Whether or not FSD/FOD needs to be saved before close the dialog
	void saveBeforeClose();

	//==============================================================================================
	// Saves all FOD files
	void saveFODFiles();

	//==============================================================================================
	// Saves a specific FSD file
	void saveFSDFile(const std::string& strFSDFileName);

	//==============================================================================================
	// Set current selection to specified input type list item index, 
	// and refreshes all contents in the dialog according to the selected input type
	void selectInputCategoriesItem(int nItemIndex);

	//==============================================================================================
	// set window title bar text
	void setWindowTitle(const std::string& strFileName);

	//==============================================================================================
	// Store all case sensitive check boxes' value in current FSD
	void storeCaseSensitivities();

	//==============================================================================================
	// Update afftected input type list according to current FOD
	void updateAffectedInputTypeList();

	//==============================================================================================
	// Updates all case sensitive check boxes' state with respect to 
	// enable/disable
	void updateCaseCheckBoxStates();

	//==============================================================================================
	// Updates all case sensitive check boxes' value according to the current FSD
	void updateCaseSensitivities();
	
	//==============================================================================================
	// Update choice check list according to current selected category name
	void updateChoiceCheckList(const std::string& strInputCategory);

	//==============================================================================================
	// Updates the current FOD
	void updateCurrentFOD();

	// Updates Disable filteirng check box state and other control according to the
	// Disable filtering check box. i.e. if current scheme is disabled, then other controls
	// need to be disabled as well
	void updateDisableFilteringState();

	//==============================================================================================
	// populate input type list boxes with FODs
	void updateInputCategoriesList();

	// Save the without prompting for a name unless
	// file does not have a name 
	// if saved return true if not saved return false
	bool doSave();

	// Save the file prompting for a name
	//	if file saved return true if not saved return false
	bool doSaveAs();

	// **************************
	// Member variables
	// vector of currently loaded FODs
	FilterOptionsDefinitions* m_pFODs;

	// vector of currently loaded FSDs
	FilterSchemeDefinitions* m_pFSDs;

	// a new FSD
	FilterSchemeDefinition m_NewFSD;

	// Currently selected FOD
	FilterOptionsDefinition *m_pCurrentFOD;

	// whoever is the current FSD that determines which choice shall
	// be selected
	FilterSchemeDefinition* m_pCurrentFSD;

	std::string m_strPrevFSDName;

	std::string m_strCurrentFSDName;

	// Whether or not this dialog is created in an editable mode for
	// creating/modifying FOD file
	bool m_bEditFOD;

	// current selected input category (FOD) in the input categories list box
	std::string m_strSelectedInputCategory;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

