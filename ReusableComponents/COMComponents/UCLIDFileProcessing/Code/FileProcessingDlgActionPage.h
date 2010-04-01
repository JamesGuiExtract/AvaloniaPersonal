#pragma once

// FileProcessingDlgActionPage.h : header file
//

#include "FPRecordManager.h"

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// FileProcessingDlgActionPage dialog

class FileProcessingDlgActionPage : public CPropertyPage
{
	DECLARE_DYNAMIC(FileProcessingDlgActionPage)

// Construction
public:
	FileProcessingDlgActionPage();
	virtual ~FileProcessingDlgActionPage();

// Dialog Data
	enum { IDD = IDD_DLG_ACTION_PROP };
	CButton	m_btnQueue;
	CButton	m_btnProcess;
	CButton	m_btnDisplay;
	CButton	m_btnSelectAction;

// Methods
	// This method is used to reset the m_bInitialized to false
	// when the user hide the tab so that when this tab reappears,
	// it will skip the first call to OnSize()
	void ResetInitialized();

	// This method return the Name of the current action
	string GetCurrentActionName();
	
	// Reload page from File Processing Manager items
	// bWarnIfActionNotFound - If true then a warning will be displayed and an exception will
	// be logged if the action is not found, if false no warning will be displayed.
	void refresh(bool bWarnIfActionNotFound = false);
	
	// Set the File Processing Manager pointer
	void setFPMgr(UCLID_FILEPROCESSINGLib::IFileProcessingManager* pFPMgr);
	
	// Set the File Processing DB pointer
	void setFPMDB(UCLID_FILEPROCESSINGLib::IFileProcessingDB* pFPMDB);

	// When enabled is set to false this page cannot be changed
	void setEnabled(bool bEnabled);

protected:
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnBnClickedChkQueue();
	afx_msg void OnBnClickedBtnSelAction();
	afx_msg void OnBnClickedChkProc();
	afx_msg void OnBnClickedChkDisplay();
	virtual BOOL OnInitDialog();
	DECLARE_MESSAGE_MAP()

private:
	//////////
	//Variables
	/////////

	// Action name
	CString	m_zActionName;

	// This variable is initialized to false so that the first call of OnSize()
	// before OnInitDialog() will be skipped, then it will be set to true inside 
	// OnInitDialog()
	bool m_bInitialized;

	// The file processing manager DB pointer
	UCLID_FILEPROCESSINGLib::IFileProcessingDB * m_pFPMDB;

	// Pointer to File Processing Manager class
	// not a smart pointer to avoid destruction issues
	UCLID_FILEPROCESSINGLib::IFileProcessingManager*	m_pFPM;

	/////////
	//Methods
	////////

	// Get File Processing Manager smart pointer for brief use
	UCLID_FILEPROCESSINGLib::IFileProcessingManagerPtr getFPM();

	// get the file supplying mgmt role for brief use
	UCLID_FILEPROCESSINGLib::IFileSupplyingMgmtRolePtr getFSMgmtRole();

	// get the action mgmt role for brief use
	UCLID_FILEPROCESSINGLib::IFileActionMgmtRolePtr getSupplyingActionMgmtRole();

	// get the file processing mgmt role for brief use
	UCLID_FILEPROCESSINGLib::IFileProcessingMgmtRolePtr getFPMgmtRole();

	// get the action mgmt role for brief use
	UCLID_FILEPROCESSINGLib::IFileActionMgmtRolePtr getProcessingActionMgmtRole();

	// This method is used to add and remove property
	// pages according to the status of three check boxes
	void addRemovePages();
	
	// Get a smart pointer to the DB
	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr getDBPointer();

	// Update the check boxes and tabs when load the action tab
	void updateChecksAndTabs();

	// Update UI, menu and toolbar items
	void updateUI();
};
