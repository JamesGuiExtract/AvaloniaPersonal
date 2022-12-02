// FAMDBAdminDlg.h : header file
//

#pragma once
#include "FAMDBAdminSummaryDlg.h"
#include "WorkflowManagement.h"

#include <WindowPersistenceMgr.h>
#include <FileProcessingConfigMgr.h>
#include <ResizablePropertySheet.h>
#include <DatabasePage.h>

#include <memory>
#include "afxwin.h"

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
	afx_msg void OnInspectFiles();
	afx_msg void OnExit();
	afx_msg void OnCancel();
	afx_msg void OnOK();
	afx_msg void OnDatabaseClear();
	afx_msg void OnDatabaseImport();
	afx_msg void OnDatabaseExport();
	afx_msg void OnDatabaseResetLock();
	afx_msg void OnDatabaseUpdateSchema();
	afx_msg void OnDatabaseChangePassword();
	afx_msg void OnDatabaseLogout();
	afx_msg void OnDatabaseSetOptions();
	afx_msg void OnActionManuallySetActionStatus();
	afx_msg void OnHelpFileActionManagerHelp();
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnGetMinMaxInfo(MINMAXINFO* lpMMI);
	afx_msg void OnToolsFileActionManager();
	afx_msg void OnToolsReports();
	afx_msg void OnToolsCheckForNewComponents();
	afx_msg void OnManageTags();
	afx_msg void OnManageBatesCounters();
	afx_msg void OnManageLoginUsers();
	afx_msg void OnManageWorkflowActions();
	afx_msg void OnToolsSetPriority();
	afx_msg void OnRecalculateStats();
	afx_msg void OnManageMetadataFields();
	afx_msg void OnManageAttributeSets();
	afx_msg void OnManageRuleCounters();
	afx_msg void OnManageDashboards();
	afx_msg void OnCbnSelchangeWorkflowCombo();
	afx_msg void OnToolsMoveFilesToWorkflow();
	afx_msg void OnManageDatabaseServices();
	afx_msg void OnManageMLModels();
	afx_msg void OnToolsDashboards();
	afx_msg void OnManageWebAPIConfigs();
	DECLARE_MESSAGE_MAP()

	//INotifyDBConfigChanged
public:
	// In the case that the specified connection information has path tags/functions to be expanded,
	// the variable values are changed to represent the literal database upon completion of this
	// call.
	virtual void OnDBConfigChanged(string& rstrServer, string& rstrDatabase,
		string& rstrAdvConnStrProperties);

	// Prompts user to select the context (ContactTags.sdf) to use. Assuming the user chooses a
	// context, rbDBTagsAvailable will indicate whether the specified context has the
	// <DatabaseServer> and <DatabaseName> tags defined.
	bool PromptToSelectContext(bool& rbDBTagsAvailable);

	// Method called to cause the summary tab to refresh its data
	// If nActionID is -1, all actions are refreshed. Otherwise, only the action with the specified
	// action ID is refreshed.
	void UpdateSummaryTab(long nActionID = -1);

private:
	//////////
	//Variables
	/////////

	ResizablePropertySheet m_propSheet;
	bool m_bInitialized;

	DatabasePage m_propDatabasePage;
	CFAMDBAdminSummaryDlg m_propSummaryPage;

	// Combo box for workflow
	CComboBox m_comboBoxWorkflow;
	CStatic m_staticWorkflowLabel;

	// The Database pointer obj to work with
	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr m_ipFAMDB;

	// Flag to indicate that the connection is valid
	bool m_bIsDBGood;

	// Registry Persistence managers
	unique_ptr<FileProcessingConfigMgr> ma_pCfgMgr;

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

	// Indicates the current database schema is known to be out-of-date.
	bool m_bDBSchemaIsNotCurrent;

	// Indicates workflows are in use but there are files in the database unaffiliated with
	// workflows.
	bool m_bUnaffiliatedFiles;

	// Saves/restores window position/size info to/from the registry.
	WindowPersistenceMgr m_windowMgr;

	// Allows inspection of files in the database using the FAMFileInspector utility.
	IFAMFileInspectorPtr m_ipFAMFileInspector;

	// The current workflow
	string m_strCurrentWorkflow;

	// Contains the current workflow ID - used to identifying when a workflow has been renamed
	long m_nCurrentWorkflowID;

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

	// Attempts to upgrade the current database to the latest schema.
	static UINT upgradeToCurrentSchemaThread(LPVOID pParam);

	// Loads the workflow combo
	void loadWorkflowComboBox();

	// Updates UI elements when workflows or their included files may have changed.
	// Unlike refreshDBStatus, this won't clear a currently selected workflow and can account for
	// a workflow name that has changed (even it if is the active workflow).
	bool refreshWorkflowStatus();

	// Positions the workflow combo and label
	void positionWorkflowControls();

	// Shows the dialog that allows files to be migrated to new workflows
	// If bNoWorkflowSource is true, the source workflow selection will be force to <No workflow>
	int showMoveToWorkflowDialog(bool bAreUnaffiliatedFiles);

	// Refreshed the current DB connection status.
	void refreshDBStatus();

	// Add or update the current FAMUser and return FAMUserID
	long addOrUpdateFAMUser(_ConnectionPtr ipConnection);
};
