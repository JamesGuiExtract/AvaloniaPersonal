
#pragma once

// FileProcessingDlgScopePage.h : header file
//

#include <string>
#include <vector>
#include <QuickMenuChooser.h>

#include "QueueGrid.h"

class FileProcessingConfigMgr;
class FileProcessingManager;

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// FileProcessingDlgScopePage dialog

class FileProcessingDlgScopePage : public CPropertyPage
{
	DECLARE_DYNCREATE(FileProcessingDlgScopePage)

// Construction
public:
	FileProcessingDlgScopePage();
	~FileProcessingDlgScopePage();

	void setConfigMgr(FileProcessingConfigMgr* cfgMgr);

	void setFPMgr(UCLID_FILEPROCESSINGLib::IFileProcessingManager* pFPMgr);

	// Reload page from File Processing Manager items
	void refresh();

	// When enabled is set to false this page cannot be accessed
	void setEnabled(bool bEnabled);

	// Will update status for the specified File Supplier
	// or all Suppliers if ipFS == __nullptr.  Also updates enabled/disabled 
	// state for action buttons
	void updateSupplierStatus(WPARAM wParam, LPARAM lParam);

	// This method is used to reset the m_bInitialized to false
	// when the user hide the tab so that when this tab reappears,
	// it will skip the first call to OnSize()
	void ResetInitialized();

// Override
	// called when the page is to be activated
	BOOL OnSetActive();
// Dialog Data
	//{{AFX_DATA(FileProcessingDlgScopePage)
	enum { IDD = IDD_DLG_SCOPE_PROP };
	CButton	m_btnAdd;
	CButton	m_btnRemove;
	CButton	m_btnConfigure;
	CString	m_zConditionDescription;
	CButton	m_btnSelectCondition;
	CEdit m_editSelectCondition;
	//}}AFX_DATA


// Overrides
	// ClassWizard generate virtual function overrides
	//{{AFX_VIRTUAL(FileProcessingDlgScopePage)
	protected:
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	afx_msg void OnBtnAdd();
	afx_msg void OnBtnRemove();
	afx_msg void OnBtnConfigure();
	virtual BOOL OnInitDialog();
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnBtnSelectCondition();
	afx_msg void OnContextCut();
	afx_msg void OnContextCopy();
	afx_msg void OnContextPaste();
	afx_msg void OnContextDelete();
	afx_msg LRESULT OnCellValueChange(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT OnGridDblClick(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT OnGridSelChange(WPARAM wParam, LPARAM lParam);
	afx_msg void OnLButtonDblClk(UINT nFlags, CPoint point);

	DECLARE_MESSAGE_MAP()

private:
	////////////
	// Variables
	////////////
	//Clipboard manager for the RC context menu
	IClipboardObjectManagerPtr m_ipClipboardMgr;

	FileProcessingConfigMgr* m_cfgMgr;

	// Object allowing user to select and configure plug in objects
	IMiscUtilsPtr m_ipMiscUtils;

	// Pointer to File Processing Manager class
	// not a smart pointer to avoid destruction issues
	UCLID_FILEPROCESSINGLib::IFileProcessingManager*	m_pFPM;

	// True if all the suppliers are in inactive, stop or done status
	// False if one of the suppliers is in active, pause status
	bool m_bEnabled;

	// Grid window with two checkbox and a combo box column
	CQueueGrid m_wndGrid;

	// This variable is initialized to false so that the first call of OnSize()
	// before OnInitDialog() will be skipped, then it will be set to true inside 
	// OnInitDialog()
	bool m_bInitialized;

	//////////
	// Methods
	//////////

	// get the file supplying mgmt role for brief use
	UCLID_FILEPROCESSINGLib::IFileSupplyingMgmtRolePtr getFSMgmtRole();

	// get the file supplier data vector for brief use
	IIUnknownVectorPtr getFileSuppliersData();

	// Get File Supplier from the specified row
	UCLID_FILEPROCESSINGLib::IFileSupplierPtr getFileSupplier(int iRow);

	// Get current status of the specified row
	UCLID_FILEPROCESSINGLib::EFileSupplierStatus getStatus(int iRow);

	//---------------------------------------------------------------------------------------------
	// PURPOSE: returns a valid clipboard manager.
	// PROMISE: if m_ipClipboardMgr is NULL, creates a new clipboard manager
	//          and points m_ipClipboardMgr to it. returns m_ipClipboardMgr.
	IClipboardObjectManagerPtr getClipboardManager();

	//---------------------------------------------------------------------------------------------
	// PURPOSE: returns a valid MiscUtils object
	// PROMISE: if m_ipMiscUtils is NULL, creates a new MiscUtils object
	//          and points m_ipMiscUtils to it. returns m_ipMiscUtils.
	IMiscUtilsPtr getMiscUtils();

	//---------------------------------------------------------------------------------------------
	// PURPOSE: sets rectWindow to the position and dimensions of the dialog item
	//          with the specified resource ID.
	// REQUIRE: uiDlgItemResourceID must be a unique resource ID corresponding to a dialog item.
	//          rectWindow must be non-NULL.
	// PROMISE: sets rectWindow to the window rect of the dialog item with the specified ID.
	void getDlgItemWindowRect(UINT uiDlgItemResourceID, RECT &rectWindow);

	// Enable / disable File Suppliers buttons
	void	setButtonStates();

	// Set status of the specified row
	void	setStatus(int iRow, UCLID_FILEPROCESSINGLib::EFileSupplierStatus eNewStatus);

	// Updates the specified row in the grid with the specified 
	// File Supplier Data
	void updateList(int nRow, UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr ipFSD);

	// Update UI, menu and toolbar items
	void updateUI();

	// Display the context menu
	void displayContextMenu();
};
//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
