
#pragma once

// FileProcessingDlg.h : header file
//

#include "FileProcessingDlgTaskPage.h"
#include "FileProcessingDlgScopePage.h"
#include "FileProcessingDlgStatusPage.h"
#include "FileProcessingDlgActionPage.h"
#include "FileProcessingDlgQueueLogPage.h"
#include "FileProcessingDlgReportPage.h"
#include "FileProcessingOptionsDlg.h"
#include "FileProcessingRecord.h"
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

#include <set>

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
	//---------------------------------------------------------------------------------------------
	void updateUI();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To return true if the FAM is "Ready" (i.e. FAM is able to Run) and false otherwise
	bool isFAMReady();
	
// Dialog Data
	enum { IDD = IDD_DLG_PROCESS_FILE };

// INotifyDBConfigChanged class
public:
	void OnDBConfigChanged(const string& strServer, const string& strDatabase);

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
	FileProcessingDlgStatusPage m_propProcessLogPage;
	FileProcessingDlgReportPage m_propStatisticsPage;
	FileProcessingDlgActionPage m_propActionPage;
	FileProcessingDlgQueueLogPage m_propQueueLogPage;
	DatabasePage m_propDatabasePage;

	FileProcessingOptionsDlg m_dlgOptions;

	// The status bar that will display processing information
	CStatusBarCtrl m_statusBar;

	// an object that helps keep the database status icon updated
	auto_ptr<DatabaseStatusIconUpdater> m_apDatabaseStatusIconUpdater;

	CSize m_sizeMinimumPropPage;
	long m_nCurrentBottomOfPropPage;

	auto_ptr<IConfigurationSettingsPersistenceMgr> ma_pUserCfgMgr;
	auto_ptr<FileProcessingConfigMgr> ma_pCfgMgr;
	
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

	// Indicates whether processing is currently processing skipped files or not
	bool m_bProcessingSkippedFiles;

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
	void openFile(string strFileName);
	//---------------------------------------------------------------------------------------------
	// This method will flush the current dialog settings to the m_ipFileProcMgr and then saves them
	// to an fps file
	bool saveFile(string strFileName);
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

	// Stops the statistics thread
	void stopStatsThread();

	// Thread function pParam must be a StatisticsMgr * (this pointer)
	static UINT __cdecl StatisticsMgrThreadFunct( LPVOID pParam );

	// Updates the database tab based on the current database status and updates the UI
	void updateUIForCurrentDBStatus();

	// Returns a pointer to the property page that is specified by ePage, this should always return a valid pointer
	CPropertyPage *getPropertyPage(EDlgTabPage ePage);

	// Returns true if the property page specified by ePage is displayed, otherwise false
	bool isPageDisplayed(EDlgTabPage ePage);

	// Removes the page from the property sheet m_propSheet
	void removePage(EDlgTabPage ePage);

	// Displays the given page if it is not already displayed
	void displayPage(EDlgTabPage ePage);
};
