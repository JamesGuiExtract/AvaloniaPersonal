
#pragma once

// FileProcessingDlgTaskPage.h : header file

#include <vector>
#include <stack>
#include <string>
#include "afxwin.h"
#include <ImageButtonWithStyle.h>

#include "TaskGrid.h"

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// FileProcessingDlgTaskPage dialog

class FileProcessingDlgTaskPage : public CPropertyPage
{
	DECLARE_DYNCREATE(FileProcessingDlgTaskPage)

// Construction
public:
	FileProcessingDlgTaskPage();
	~FileProcessingDlgTaskPage();

// Methods

	void setFPMgr(UCLID_FILEPROCESSINGLib::IFileProcessingManager* pFPMgr);

	// When enabled is set to false this page cannot be accessed
	void setEnabled(bool bEnabled);

	// Retrieve current count from UI
	int getNumThreads();

	// Clear and refresh any UI elements
	void refresh();

	// This method is used to reset the m_bInitialized to false
	// when the user hide the tab so that when this tab reappears,
	// it will skip the first call to OnSize()
	void ResetInitialized();


// Dialog Data
	enum { IDD = IDD_DLG_TASK_PROP };
	CButton	m_btnAdd;
	CButton	m_btnModify;
	CButton	m_btnRemove;
	CImageButtonWithStyle m_btnUp;
	CImageButtonWithStyle m_btnDown;
	CTaskGrid m_fileProcessorList;
	CButton	m_btnLogErrorDetails;
	BOOL	m_bLogErrorDetails;
	CEdit m_editErrorLog;
	CString	m_zErrorLog;
	CImageButtonWithStyle	m_btnErrorSelectTag;
	CButton	m_btnBrowseErrorLog;
	CButton	m_btnExecuteErrorTask;
	BOOL	m_bExecuteErrorTask;
	CString	m_zErrorTaskDescription;
	CButton	m_btnSelectErrorTask;
	CButton m_radioProcessAll;
	CButton m_radioProcessSkipped;
	CComboBox m_comboSkipped;
	CStatic m_staticSkipped;
	CButton m_btnAdvancedSettings;
	CEdit m_editExecuteTask;

// Overrides
	// ClassWizard generate virtual function overrides
	protected:
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

// Implementation
protected:
	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnBtnAdd();
	afx_msg void OnBtnRemove();
	afx_msg void OnBtnModify();
	afx_msg void OnBtnDown();
	afx_msg void OnBtnUp();
	afx_msg LRESULT OnGridSelChange(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT OnGridDblClick(WPARAM wParam, LPARAM lParam);
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg LRESULT OnGridRightClick(WPARAM wParam, LPARAM lParam);
	afx_msg void OnContextCut();
	afx_msg void OnContextCopy();
	afx_msg void OnContextPaste();
	afx_msg void OnContextDelete();
	afx_msg LRESULT OnCellValueChange(WPARAM wParam, LPARAM lParam);
	afx_msg void OnCheckLogErrorDetails();
	afx_msg void OnEnChangeEditErrorLog();
	afx_msg void OnBtnErrorSelectDocTag();
	afx_msg void OnBtnBrowseErrorLog();
	afx_msg void OnCheckExecuteErrorTask();
	afx_msg void OnBtnAddErrorTask();
	afx_msg void OnBtnProcessAllOrSkipped();
	afx_msg void OnComboSkippedChange();
	afx_msg void OnLButtonDblClk(UINT nFlags, CPoint point);
	afx_msg void OnBtnAdvancedSettings();
	
	DECLARE_MESSAGE_MAP()

private:
	////////////
	// Methods
	////////////
	// to show SelectObjectWithDescription dialog for picking 
	// an file processor with unique description
	bool selectFileProcessor(IObjectWithDescriptionPtr ipObjWithDesc);
	
	//This is mainly called when loading a file. Other adds are done
	// via the onBtnAdd function.
	void addFileProcessor(IObjectWithDescriptionPtr ipObj);

	// Enable/disable various buttons based upon which items are selected
	void	setButtonStates();

	// Deletes the currently selected tasks
	void	deleteSelectedTasks();	

	//---------------------------------------------------------------------------------------------
	// PURPOSE: sets rectWindow to the position and dimensions of the dialog item
	//          with the specified resource ID.
	// REQUIRE: uiDlgItemResourceID must be a unique resource ID corresponding to a dialog item.
	//          rectWindow must be non-NULL.
	// PROMISE: sets rectWindow to the window rect of the dialog item with the specified ID.
	void getDlgItemWindowRect(UINT uiDlgItemResourceID, RECT &rectWindow);

	//---------------------------------------------------------------------------------------------
	// PURPOSE: replaces the file processor in the list box at iIndex with ipNewFP.
	// REQUIRE: iIndex is the index of the file processor to replace in the file processor list box.
	//          ipNewFP is a non-NULL ObjectWithDescription.
	// PROMISE: replaces the old file processor at iIndex with ipNewFP.
	//          retains the checkbox state.
	//          refreshes the dialog.
	void	replaceFileProcessorAt(int iIndex, IObjectWithDescriptionPtr ipNewFP);

	//---------------------------------------------------------------------------------------------
	// PURPOSE: returns a valid clipboard manager.
	// PROMISE: if m_ipClipboardMgr is NULL, creates a new clipboard manager
	//          and points m_ipClipboardMgr to it. returns m_ipClipboardMgr.
	IMiscUtilsPtr getMiscUtils();

	// get the file processing mgmt role for brief use
	UCLID_FILEPROCESSINGLib::IFileProcessingMgmtRolePtr getFPMgmtRole();

	// get the file processor data vector for brief use
	IIUnknownVectorPtr getFileProcessorsData();

	// Update UI, menu and toolbar items
	void updateUI();

	/////////////
	// Variables
	/////////////

	//Clipboard manager for the RC context menu
	IClipboardObjectManagerPtr m_ipClipboardMgr;
	
	// Pointer to File Processing Manager class
	// not a smart pointer to avoid destruction issues
	UCLID_FILEPROCESSINGLib::IFileProcessingManager*	m_pFPM;

	// object for handling plug in object selection and configuration
	IMiscUtilsPtr m_ipMiscUtils;

	// Used to hold the start and end of the current selection in the Error Log edit box
	DWORD m_dwSel;

	// true if this tab should be accessible
	bool m_bEnabled;

	// This variable is initialized to false so that the first call of OnSize()
	// before OnInitDialog() will be skipped, then it will be set to true inside 
	// OnInitDialog()
	bool m_bInitialized;
};
