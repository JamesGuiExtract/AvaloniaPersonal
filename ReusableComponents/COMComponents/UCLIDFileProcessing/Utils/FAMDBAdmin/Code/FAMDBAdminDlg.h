// FAMDBAdminDlg.h : header file
//

#pragma once
#include "FAMDBAdminSummaryDlg.h"

#include <WindowPersistenceMgr.h>
#include <FileProcessingConfigMgr.h>
#include <ResizablePropertySheet.h>
#include <DatabasePage.h>

#include <memory>

using namespace std;

// CFAMDBAdminDlg dialog
class CFAMDBAdminDlg : public CDialog, public IDBConfigNotifications
{
// Construction
public:
	CFAMDBAdminDlg(IFileProcessingDBPtr ipFAMDB, CWnd* pParent = NULL);
	~CFAMDBAdminDlg();
	
// Dialog Data
	enum { IDD = IDD_FAMDBADMIN_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	virtual BOOL PreTranslateMessage(MSG* pMsg);

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnHelpAbout();
	afx_msg void OnExportFileLists();
	afx_msg void OnExit();
	afx_msg void OnCancel();
	afx_msg void OnOK();
	afx_msg void OnDatabaseClear();
	afx_msg void OnDatabaseResetLock();
	afx_msg void OnDatabaseUpdateSchema();
	afx_msg void OnDatabaseChangePassword();
	afx_msg void OnDatabaseLogout();
	afx_msg void OnActionManuallySetActionStatus();
	afx_msg void OnHelpFileActionManagerHelp();
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnGetMinMaxInfo(MINMAXINFO* lpMMI);
	afx_msg void OnToolsFileActionManager();
	afx_msg void OnToolsReports();
	afx_msg void OnToolsCheckForNewComponents();
	afx_msg void OnManageTags();
	afx_msg void OnManageCounters();
	afx_msg void OnManageLoginUsers();
	afx_msg void OnManageActions();
	afx_msg void OnToolsSetPriority();
	DECLARE_MESSAGE_MAP()

	//INotifyDBConfigChanged
public:
	virtual void OnDBConfigChanged(const string& strServer, const string& strDatabase);

	// Method called to cause the summary tab to refresh its data
	void NotifyStatusChanged();

private:
	//////////
	//Variables
	/////////

	ResizablePropertySheet m_propSheet;
	bool m_bInitialized;

	DatabasePage m_propDatabasePage;
	CFAMDBAdminSummaryDlg m_propSummaryPage;

	// The Database pointer obj to work with
	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr m_ipFAMDB;

	// Flag to indicate that the connection is valid
	bool m_bIsDBGood;

	// Registry Persistence managers
	auto_ptr<FileProcessingConfigMgr> ma_pCfgMgr;

	// Misc utils object for calling AllowUserToSelectAndConfigureObject2
	IMiscUtilsPtr m_ipMiscUtils;

	// Category manager to check for registered category implementations
	ICategoryManagerPtr m_ipCategoryManager;

	// An IProgressStatus object with which to display schema update progress
	IProgressStatusPtr m_ipSchemaUpdateProgressStatus;

	// The IProgressStatusDialog for m_ipSchemaUpdateProgressStatus. 
	IProgressStatusDialogPtr m_ipSchemaUpdateProgressStatusDialog;

	// Indicates whether the most recently attempt schema update succeeded.
	bool m_bSchemaUpdateSucceeded;

	// Indicates wether the current database schema is known to be out-of-date.
	bool m_bDBSchemaIsNotCurrent;

	// Saves/restores window position/size info to/from the registry.
	WindowPersistenceMgr m_windowMgr;

	//////////
	//Methods
	/////////

	// Loads menu resource and removes unwanted entries
	void loadMenu();

	// Enables and disables menu items based on the status of the m_bIsAdmin, m_bIsDBGood and m_bOldDB
	void enableMenus();

	// Checks for actions in the DB, if none displays a Message box advising the user and returns true
	// returns false if there are actions in the DB
	bool notifyNoActions();

	// Sets up the Property page size based on the client area of the dialog
	void setPropPageSize();

	// Returns m_ipMiscUtils, after initializing it if necessary
	IMiscUtilsPtr getMiscUtils();

	// Returns m_ipCategoryManager, after initializing it if necessary
	ICategoryManagerPtr getCategoryManager();

	// Obtain and verify licensing of a categorized component with the given ID
	// Returns the pointer if successful, NULL otherwise.  It will display
	// (but not throw) an exception for the first problem instantiating each class.
	ICategorizedComponentPtr getCategorizedComponent(const string& strProgID);

	// Gets the status from the database and updates the status on the Database page
	void setUIDatabaseStatus();

	// Refreshes the statistics on a summary tab
	void updateSummaryTab();

	// Attempts to upgrade the current database to the latest schema.
	static UINT upgradeToCurrentSchemaThread(LPVOID pParam);
};
