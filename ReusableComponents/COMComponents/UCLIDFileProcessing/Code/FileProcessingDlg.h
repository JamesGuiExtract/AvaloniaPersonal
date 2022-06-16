
#pragma once

// FileProcessingDlg.h : header file
//

#include "FileProcessingDlgTaskPage.h"
#include "FileProcessingDlgScopePage.h"
#include "FileProcessingDlgProcessingPage.h"
#include "FileProcessingDlgActionPage.h"
#include "FileProcessingDlgQueueLogPage.h"
#include "FileProcessingDlgReportPage.h"
#include "FileProcessingOptionsDlg.h"
#include "FileProcessingRecord.h"
#include "FPWorkItem.h"
#include "TaskEvent.h"
#include "FPRecordManager.h"
#include "DatabaseStatusIconUpdater.h"

#include <memory>
#include <SplitterControl.h>
#include <ResizablePropertySheet.h>
#include <TimeIntervalMerger.h>
#include <FileRecoveryManager.h>
#include <DatabasePage.h>
#include <FileProcessingConfigMgr.h>
#include <MRUList.h>

#include <set>
#include "afxwin.h"

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// FileProcessingDlg dialog

class FileProcessingDlg : public CDialog, public IDBConfigNotifications
{
// Construction
public:
	//---------------------------------------------------------------------------------------------
	FileProcessingDlg(UCLID_FILEPROCESSINGLib::IFileProcessingManager* pFileProcMgr, 
					  UCLID_FILEPROCESSINGLib::IFileProcessingDB* pFPMDB, void* pFRM,
					  CWnd* pParent = NULL);   // standard constructor
	//---------------------------------------------------------------------------------------------
	~FileProcessingDlg();
	//---------------------------------------------------------------------------------------------
	void setRecordManager(FPRecordManager* pRecordMgr);
	//---------------------------------------------------------------------------------------------
	void setRunOnInit(bool bRunOnInit);
	//---------------------------------------------------------------------------------------------
	void setCloseOnComplete(bool bCloseOnExit);
	//---------------------------------------------------------------------------------------------
	void setForceCloseOnComplete( bool bForceClose);
	//---------------------------------------------------------------------------------------------
	// REQUIRE: lNumberOfDocsToExecute >= 0
	void setNumberOfDocsToExecute(long lNumberOfDocsToExecute);
	//---------------------------------------------------------------------------------------------
	
	// Enum contains value for each page 
	enum EDlgTabPage:int
	{
		kDatabasePage = 0,
		kActionPage = 1,
		kQueueSetupPage = 2,
		kQueueLogPage = 3,
		kProcessingSetupPage = 4,
		kProcessingLogPage = 5,
		kStatisticsPage = 6	
	} ETabPage;

	//---------------------------------------------------------------------------------------------
	// PURPOSE: To update the tabs on the property sheet to have only the tabs in the setPages set
	// NOTE:	The DatabasePage will always be displayed.
	void updateTabs(const set<EDlgTabPage>& setPages);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: update the enabled/disabled/pushed status of the run/pause/stop buttons & menu
	//			items
	void updateMenuAndToolbar();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: This method will update UI
	void updateUI();
	//---------------------------------------------------------------------------------------------
	// Updates the status bar (current status + stats). This will not query for the latest stats;
	// it will use the previously retrieved stats.
	void updateStatusBar(bool bForceUpdate);
	//---------------------------------------------------------------------------------------------	
	// Updates the database tab based on the current database status and updates the UI
	void updateUIForCurrentDBStatus();
	//---------------------------------------------------------------------------------------------
	// Updates the stats using the supplied ipActionStatistics. This will call updateStatusBar to 
	// display the new stats.
	void updateStats(UCLID_FILEPROCESSINGLib::IActionStatisticsPtr ipActionStats);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To return true if the FAM is "Ready" (i.e. FAM is able to Run) and false otherwise
	bool isFAMReady();
	
// Dialog Data
	enum { IDD = IDD_DLG_PROCESS_FILE };

// INotifyDBConfigChanged class
public:
	// In the case that the specified connection information has path tags/functions to be expanded,
	// the variable values are changed to represent the literal database upon completion of this
	// call.
	void OnDBConfigChanged(
		string& rstrServer, string& rstrDatabase, string& rstrAdvConnStrProperties);

	// Prompts user to select the context (ContactTags.sdf) to use. Assuming the user chooses a
	// context, rbDBTagsAvailable will indicate whether the specified context has the
	// <DatabaseServer> and <DatabaseName> tags defined.
	bool PromptToSelectContext(bool& rbDBTagsAvailable);

protected:
// Overrides
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

// Implementation

	HICON m_hIcon;

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnGetMinMaxInfo(MINMAXINFO* pMMI);
	afx_msg void OnPaint();
	afx_msg void OnClose();
	afx_msg void OnBtnRun();
	afx_msg void OnBtnStop();
	afx_msg void OnBtnAutoScroll();
	afx_msg void OnTimer(UINT nIDEvent);
	afx_msg LRESULT OnClearUI(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT OnProcessingComplete(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT OnProcessingCancelling(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT OnStatusChange(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT OnWorkItemStatusChange(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT OnSupplierStatusChange(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT OnQueueEvent(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT OnStatsUpdateMessage(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT OnScheduleInactive(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT OnScheduleActive(WPARAM wParam, LPARAM lParam);
	afx_msg void OnFileExit();
	afx_msg void OnFileNew();
	afx_msg void OnFileOpen();
	afx_msg void OnFileSave();
	afx_msg void OnFileSaveas();
	afx_msg void OnFileRequireAdminEdit();
	afx_msg void OnFileLoginAsAdmin();
	afx_msg void OnToolsCheckfornewcomponents();
	afx_msg void OnToolsOptions();
	afx_msg void OnDropFiles( HDROP hDropInfo );
	afx_msg void OnProcessStartprocessing();
	afx_msg void OnProcessStopprocessing();
	afx_msg void OnToolsAutoscroll();
	afx_msg void OnBtnPause();
	afx_msg void OnProcessPauseProcessing();
	afx_msg void OnHelpAboutfileprocessingmanager();
	afx_msg void OnHelpFileprocessingmanagerhelp();
	afx_msg void OnToolsFAMDBAdmin();
	afx_msg void OnToolsEditCustomTags();
	afx_msg void OnToolbarDropDown(NMHDR* pNMHDR, LRESULT *plr);
	afx_msg void OnSelectFAMMRUPopupMenu(UINT nID);
	afx_msg void OnCbnSelchangeWorkflowCombo();
	afx_msg void OnBnClickedContextEdit();
	BOOL OnToolTipNotify(UINT nID, NMHDR* pNMHDR, LRESULT* pResult);
	DECLARE_MESSAGE_MAP()

private:

	/////////////
	// Variables
	/////////////
	CToolBar m_toolBar;
	ResizablePropertySheet m_propSheet;

	FileProcessingDlgTaskPage m_propProcessSetupPage;
	FileProcessingDlgScopePage m_propQueueSetupPage;
	FileProcessingDlgProcessingPage m_propProcessingPage;
	FileProcessingDlgReportPage m_propStatisticsPage;
	FileProcessingDlgActionPage m_propActionPage;
	FileProcessingDlgQueueLogPage m_propQueueLogPage;
	DatabasePage m_propDatabasePage;

	FileProcessingOptionsDlg m_dlgOptions;

	// The status bar that will display processing information
	CStatusBarCtrl m_statusBar;

	// ComboBox for the workflows in selected database
	CComboBox m_comboBoxWorkflow;

	// Flag to indicate that workflows are defined in the database
	bool m_bWorkflowsDefined;
	
	// Static labels for workflow and context
	CStatic m_staticWorkflowLabel;
	CStatic m_staticContextLabel;
	
	// Button that will display current context and allow you to edit it
	CButton m_buttonContext;

	// an object that helps keep the database status icon updated
	unique_ptr<DatabaseStatusIconUpdater> m_apDatabaseStatusIconUpdater;

	CSize m_sizeMinimumPropPage;
	long m_nCurrentBottomOfPropPage;

	unique_ptr<FileProcessingConfigMgr> ma_pCfgMgr;
	
	CToolTipCtrl m_ToolTipCtrl;

	bool m_bInitialized;

	// true if we are currently processing
	bool m_bRunning;

	// This flag is set if only the statistics option is checked when the start button
	// is pressed
	bool m_bStatsOnlyRunning;

	// this is used so that stats can be stopped and started when the pause button
	// is pressed
	bool m_bPaused;

	EQueueType m_eQueueMode;

	// The file processing manager does the actual file processing
	UCLID_FILEPROCESSINGLib::IFileProcessingManager* m_pFileProcMgr;

	// The file processing manager DB pointer
	UCLID_FILEPROCESSINGLib::IFileProcessingDB* m_pFPMDB;

	// Some variables the that will be tracked for display in the status bar
	long m_nNumInitialCompleted;
	long m_nNumInitialFailed;
	long m_nNumCompletedProcessing;
	long m_nNumFailed;
	long m_nNumPending;
	long m_nNumSkipped;
	long m_nNumCurrentlyProcessing;
	long m_nNumTotalDocs;

	// The number of documents to execute when processing
	long m_nNumberOfDocsToExecute;

	// Used to retrieve information about specific file processing records
	FPRecordManager* m_pRecordMgr;

	// Stores the name of the currently loaded(or saved) fps file
	string m_strCurrFPSFilename;

	// Indicates that the database connection is currently being established based on the currently
	// specified database parameters. Used to prevent displaying the edit custom tags UI at
	// inappropriate times.
	bool m_bUpdatingConnection;

	// This flag is set to true when the user stop processing using the stop button or stop processing
	// menu option.  If the user interacts with the system by stopping processing the system will not
	// close on completion even if that option was specified.
	bool m_bStoppedManually;

	// this flag when set before the dialog is displayed will automatically begin running 
	// a loaded fps file on startup.  This option requires that there be a valid fps file loaded
	// before the dialog is displayed.
	bool m_bRunOnInit;

	// This flag specifies that the dialog will automatically terminate when a processing batch finishes.
	// If the processing is stopped manually or some of the files have failed the dialog will not auto-close.
	// In other words setting this flag does not guarantee that the dialog will auto close
	bool m_bCloseOnComplete;

	// This flag specifies that the dialog will automatically terminate when a processing batch finishes.
	// If the processing is stopped manually will not auto-close. However it there are errors the dialog will close
	// In other words setting this flag does not guarantee that the dialog will auto close
	bool m_bForceCloseOnComplete;

	// Flag to indicate that the stop process has been initiated
	bool m_bExceededExecCount;

	// Time interval for stats calculations
	TimeIntervalMerger m_completedOrFailedTime;

	// Timer to keep track of processing duration
	StopWatch m_stopWatch;
	
	// Used to set and reset the timer
	long m_nTimerID;

	// object that manages file recovery related functionality
	// This can be NULL
	FileRecoveryManager *m_pFRM;

	// Is Database connection ready
	bool m_bDBConnectionReady;

	// The ID of the currently configured action
	long m_nCurrActionID;

	// A textual description of the current database connection status.
	string m_strCurrDBStatus;

	// Statistics Thread Related Events
	Win32Event m_eventStatsThreadStarted;
	Win32Event m_eventStatsThreadStop;
	Win32Event m_eventStatsThreadExited;

	// This flag is set if running as a service
	bool m_bRunningAsService;

	// String to contain processing state string 
	// if processing active it will be empty
	// if processing is inactive by the schedule it will contain "Processing Inactive"
	string m_strProcessingStateString;

	// MRU List objects
	std::unique_ptr<IConfigurationSettingsPersistenceMgr> m_upUserConfig;
	std::unique_ptr<MRUList> m_upMRUList;

	// MRU for contexts
	unique_ptr<IConfigurationSettingsPersistenceMgr> m_upUserConfig2;
	unique_ptr<MRUList> m_upContextMRUList;

	// Used to determine if <DatabaseServer> and <DatabaseName> environment specific path tags are
	// available for use in m_propDatabasePage.
	UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr m_ipFAMTagManager;

	// Flag indicating if the FPS requires an admin login to allow editing
	bool m_bRequireAdminEdit;

	// Indicates whether the FAM is currently locked from editing.
	bool m_bFPSLocked;

	////////////
	// Methods
	///////////

	//---------------------------------------------------------------------------------------------
	void createStatusBar();
	//---------------------------------------------------------------------------------------------
	void createPropertyPages();
	//---------------------------------------------------------------------------------------------
	void createToolBar();
	//---------------------------------------------------------------------------------------------
	void doResize();
	//---------------------------------------------------------------------------------------------
	// this will be called when a new Task has been added 
	void addTask(const FileProcessingRecord& rTask);
	//---------------------------------------------------------------------------------------------
	// this will be called when a tasks status has changed
	void updateTask(const FileProcessingRecord& rOldTask, const FileProcessingRecord& rNewTask);
	//---------------------------------------------------------------------------------------------
	// this will be called when a task is to be removed
	void removeTask(const FileProcessingRecord& rTask);
	//---------------------------------------------------------------------------------------------
	// This method will load the specified .fps file into m_ipFileProcMgr and then load the settings
	// from m_ipFileProcMgr into the dialog.
	// If the current configuration of the dialog has not been saved it will prompt the user to save.
	// If strFileName == "" this method will open a dialog from which the user can select a .fps file
	// to open
	// If bPreserveConnection is true, key connection parameters will be restored after loading
	// (most notably admin login status). Do not use bPreserveConnection unless it is certain the loaded
	// database will match the current database.
	void openFile(string strFileName, bool bPreserveConnection);
		//---------------------------------------------------------------------------------------------
	// This method will flush the current dialog settings to the m_ipFileProcMgr and then saves them
	// to an fps file. bShowConfigurationWarnings indicates whether a prompt should be displayed
	// about any potential configuration issues before allowing the save.
	bool saveFile(string strFileName, bool bShowConfigurationWarnings);
	//---------------------------------------------------------------------------------------------
	// This method will check whether any settings in the editor have changed since the last 
	// load/save if so it will prompt the user to Save, Continue without saving or cancel.
	// If the user chooses cancel this method will return false.  
	// If the user chooses save this method will save and return true
	// If the user chooses not to save this method will just return true
	bool checkForSave();
	//---------------------------------------------------------------------------------------------
	// This method populates all of the settings in the dialog (primarily the scope and task pages)
	// with the settings retrieved from the current IFileProcessingManager m_ipFileProcMgr
	void loadSettingsFromManager();
	//---------------------------------------------------------------------------------------------
	// This method will retrieve all the settings from the dialog and set them on the 
	// IFileProcessingManager
	bool flushSettingsToManager();
	//---------------------------------------------------------------------------------------------
	// This method sets the m_strCurrFPSFilename to strFileName and updates the Dialog title
	// appropriately
	void setCurrFPSFile(const string& strFileName);
	//---------------------------------------------------------------------------------------------
	void updateDBConnectionStatus();
	//---------------------------------------------------------------------------------------------
	void saveWindowSettings();
	//---------------------------------------------------------------------------------------------
	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr getDBPointer();
	//---------------------------------------------------------------------------------------------
	// Get File Processing Manager smart pointer for brief use
	UCLID_FILEPROCESSINGLib::IFileProcessingManagerPtr getFPM();
	//---------------------------------------------------------------------------------------------
	void startRunTimer();
	//---------------------------------------------------------------------------------------------
	void stopRunTimer();
	//---------------------------------------------------------------------------------------------
	void resetRunTimer();
	//---------------------------------------------------------------------------------------------
	// Check if the FAM is in valid status so it can be saved or run
	void validateFAMStatus();
	//---------------------------------------------------------------------------------------------
	// Get the status for the current FAM setting
	CString getFAMStatus();
	//---------------------------------------------------------------------------------------------
	// Promise: Set up and launch the Statistics thread
	//			- The stats thread will update itself from the DB and post a message when the stats
	//			  need to be updated.
	void setupAndLaunchStatsThread();
	//---------------------------------------------------------------------------------------------
	// Promise: To update the UI to the appropriate state when processing has completed
	void updateUIForProcessingComplete();
	//---------------------------------------------------------------------------------------------
	void addFileToMRUList(const string& strFileToAdd);
	//---------------------------------------------------------------------------------------------
	void removeFileFromMRUList(const string& strFileToRemove);
	//---------------------------------------------------------------------------------------------

	// Stops the statistics thread
	void stopStatsThread();

	// Thread function pParam must be a StatisticsMgr * (this pointer)
	static UINT __cdecl StatisticsMgrThreadFunct( LPVOID pParam );

	// Returns a pointer to the property page that is specified by ePage, this should always return a valid pointer
	CPropertyPage *getPropertyPage(EDlgTabPage ePage);

	// Returns true if the property page specified by ePage is displayed, otherwise false
	bool isPageDisplayed(EDlgTabPage ePage);

	// Removes the page from the property sheet m_propSheet
	void removePage(EDlgTabPage ePage);

	// Displays the given page if it is not already displayed
	void displayPage(EDlgTabPage ePage);

	string getDefaultFileName();

	// Clears the settings that indicate a context
	void clearContext();

	// Refreshes context tag info based on current FPSFileDir from ContextTags.sdf.
	void refreshContext();

	// Displays custom tags editor
	void editCustomTags();

	// Displays dialog allowing used to select from recently used contexts or to browse to one
	// manually.
	bool displayRecentContextSelection();

	// Indicates whether an active context has defined the <DatabaseServer> and <DatabaseName> tags.
	bool areDatabaseTagsDefined();

	// Checks that strContext is populated and not a special string indicating a missing context.
	bool isValidContext(string strContext = "");

	// Retrieves get an ITagUtility interface pointer to m_ipFAMTagManager.
	ITagUtilityPtr getTagUtility();

	// Loads the workflow combo box from the database
	void loadWorkflowComboBox();

	// Positions the Workflow combo, labels and context button
	void positionWorkflowContextControls();

	// Check the version of the context tags database and prompt for update if it isn't current
	void checkAndUpdateContextTagsDatabaseIfNeeded(std::string &strFileName);

	// Enable or Disable all tabs based on bEnable
	void setPagesEnable(bool bEnable);
};
