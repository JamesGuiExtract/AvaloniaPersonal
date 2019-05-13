// FileProcessingDlg.cpp : implementation file
//

#include "stdafx.h"
#include "resource.h"
#include "FileProcessingDlg.h"
#include "FPCategories.h"
#include "FPAboutDlg.h"
#include "FP_UI_Notifications.h"

#include <IFCategories.h>
#include <UCLIDException.h>
#include <RegistryPersistenceMgr.h>
#include <TemporaryResourceOverride.h>
#include <cpputil.h>
#include <ComUtils.h>
#include <Win32Util.h>
#include <RegConstants.h>
#include <LoadFileDlgThread.h>
#include <FAMUtilsConstants.h>
#include <SuspendWindowUpdates.h>
#include <FAMUtilsConstants.h>
#include <DialogSelect.h>

#include <vector>
#include <map>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const int giMIN_LISTBOX_PLUS_TOOLBAR_HEIGHT = 213;
const int giAUTO_SAVE_TIMERID = 1001;
const int giAUTO_SAVE_FREQUENCY = 60 * 1000;
const string gstrFILE_PROCESSING_SPECIFICATION_EXT = ".fps";

// ID's of status bar indicator panes
const long gnDB_CONNECTION_STATUS_PANE_ID = 7;
const long gnFAILED_COUNTS_STATUS_PANE_ID = 6;
const long gnSKIPPED_COUNTS_STATUS_PANE_ID = 5;
const long gnPENDING_COUNTS_STATUS_PANE_ID = 4;
const long gnPROCESSING_COUNTS_STATUS_PANE_ID = 3;
const long gnCOMPLETED_COUNTS_STATUS_PANE_ID = 2;
const long gnTOTAL_COUNTS_STATUS_PANE_ID = 1;
const long gnSTATUS_TEXT_STATUS_PANE_ID = 0;
const long gnNUM_STATUS_PANES = 8;

// widths of status bar indicator panes
const long gnDB_CONNECTION_STATUS_PANE_WIDTH = 73;
const long gnFAILED_COUNTS_STATUS_PANE_WIDTH = 100;					// NOTE: ALL THESE WIDTHS HAVE
const long gnPENDING_COUNTS_STATUS_PANE_WIDTH = 108;				// BEEN HAND-TWEAKED SO THAT
const long gnPROCESSING_COUNTS_STATUS_PANE_WIDTH = 123;				// THE NUMBER 88,888,888 FITS
const long gnCOMPLETED_COUNTS_STATUS_PANE_WIDTH = 123;				// COMFORTABLY IN THE FIELD TO THE
const long gnSKIPPED_COUNTS_STATUS_PANE_WIDTH = 108;				// RIGHT OF THE LABEL OF THE FIELD.
const long gnTOTAL_COUNTS_STATUS_PANE_WIDTH = 95;

// Use of XP Themes causes a visual artifact updating the connection
// status icon if padding is not added to the right hand side [P13:4707]
const long gnSTATUSBAR_RIGHTHAND_PADDING = 10;

// Default labels for status bar panes
const string gstrCOMPLETED_STATUS_PANE_LABEL = "Completed:";
const string gstrPROCESSING_STATUS_PANE_LABEL = "Processing:";
const string gstrFAILED_STATUS_PANE_LABEL = "Failed:";
const string gstrPENDING_STATUS_PANE_LABEL = "Pending:";
const string gstrSKIPPED_STATUS_PANE_LABEL = "Skipped:";
const string gstrTOTAL_STATUS_PANE_LABEL = "Total:";

// other constants associated with the status bar
static const long gnSTATUS_BAR_HEIGHT = 18;
static const long gnDEFAULT_STATUS_PANE_WIDTH = 20;

// String for DBAdmin file name
const string gstrFLEX_INDEX_BIN_RELATIVE_CC = ".\\..\\FlexIndexComponents\\Bin\\";
const string gstrFAMDBADMIN_FILENAME = "FAMDBAdmin.exe";

// Name of the Cutom tags database file
const string gstrCUSTOMTAGS_DB_FILE = "CustomTags.sdf";

// To indicate counts that are uninitialized.
static const long gnUNINITIALIZED = -1;

//-------------------------------------------------------------------------------------------------
// FileProcessingDlg dialog
//-------------------------------------------------------------------------------------------------
FileProcessingDlg::FileProcessingDlg(UCLID_FILEPROCESSINGLib::IFileProcessingManager* pFileProcMgr, 
									 UCLID_FILEPROCESSINGLib::IFileProcessingDB* pFPMDB, void* pFRM,
									 CWnd* pParent)
:CDialog(FileProcessingDlg::IDD, pParent),
 m_bInitialized(false), m_bRunning(false),
 m_pFileProcMgr(pFileProcMgr),
 m_pFPMDB(pFPMDB),
 m_nNumInitialCompleted(gnUNINITIALIZED),
 m_nNumInitialFailed(gnUNINITIALIZED),
 m_nNumCompletedProcessing(0),
 m_nNumFailed(0),
 m_nNumPending(0),
 m_nNumSkipped(0),
 m_nNumCurrentlyProcessing(0),
 m_nNumTotalDocs(0),
 m_bStoppedManually(false),
 m_bRunOnInit(false),
 m_bCloseOnComplete(false),
 m_bForceCloseOnComplete(false),
 m_bExceededExecCount(false),
 m_bDBConnectionReady(true),
 m_nCurrActionID(-1),
 m_apDatabaseStatusIconUpdater(__nullptr),
 m_bStatsOnlyRunning(false),
 m_bProcessingSkippedFiles(false),
 m_bPaused(false),
 m_nNumberOfDocsToExecute(0),
 m_bUpdatingConnection(false),
 m_bWorkflowsDefined(false),
 m_bRequireAdminEdit(false),
 m_bFPSLocked(false)
{
	try
	{
		//{{AFX_DATA_INIT(FileProcessingDlg)
			// NOTE: the ClassWizard will add member initialization here
		//}}AFX_DATA_INIT

		// check pre-conditions
		ASSERT_ARGUMENT("ELI15053", pFPMDB != __nullptr );
		ASSERT_RESOURCE_ALLOCATION("ELI08925", m_pFileProcMgr != __nullptr);

		m_hIcon = AfxGetApp()->LoadIcon(IDI_ICON_PROCESS);
		ASSERT_RESOURCE_ALLOCATION("ELI14999", m_hIcon != __nullptr);

		m_ipFAMTagManager.CreateInstance(CLSID_FAMTagManager);
		ASSERT_RESOURCE_ALLOCATION("ELI37906", m_ipFAMTagManager != __nullptr);

		ma_pCfgMgr = unique_ptr<FileProcessingConfigMgr>(new
			FileProcessingConfigMgr());

		// create a registry config mgr for the MRU list settings
		m_upUserConfig.reset(new RegistryPersistenceMgr(HKEY_CURRENT_USER,
			gstrREG_ROOT_KEY + "\\UCLIDFileProcessing\\FileProcessingDlg"));
		m_upMRUList.reset(new MRUList(m_upUserConfig.get(), "\\MRUList", "File_%d", 8));

		// https://extract.atlassian.net/browse/ISSUE-13528
		// It seems to me a mistake that m_upUserConfig uses gstrREG_ROOT_KEY instead of
		// gstrCOM_COMPONENTS_REG_PATH, but rather than fix it (which would either lose MRU data for
		// customers as a result of upgrading, or would require code to port the list to the new
		// location) I am just creating a different RegistryPersistenceMgr instance.
		m_upUserConfig2.reset(new RegistryPersistenceMgr(HKEY_CURRENT_USER,
			gstrCOM_COMPONENTS_REG_PATH + "\\UCLIDFileProcessing"));
		m_upContextMRUList.reset(
			new MRUList(m_upUserConfig2.get(), "\\ContextsMRUList", "File_%d", 12));

		m_dlgOptions.setConfigManager(ma_pCfgMgr.get());

		// Set the NotifyDBConfigFileChanged pointer for the DatabasePage
		m_propDatabasePage.setNotifyDBConfigChanged(this);
		
		// Cast to an FileRecoveryManager pointer
		m_pFRM = (FileRecoveryManager *)pFRM;

		// This is running as a service if the FileRecoveryManager pointer is NULL
		m_bRunningAsService = ( m_pFRM == NULL );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08885")
}
//-------------------------------------------------------------------------------------------------
FileProcessingDlg::~FileProcessingDlg() 
{
	try
	{
		// delete icon updater
		m_apDatabaseStatusIconUpdater.reset(__nullptr);

		// delete other allocated handles
		if (m_hIcon)
		{
			::DeleteObject(m_hIcon);
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI14992")
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(FileProcessingDlg)
	//}}AFX_DATA_MAP
	DDX_Control(pDX, IDC_WORKFLOW_COMBO, m_comboBoxWorkflow);
	DDX_Control(pDX, IDC_CONTEXT_EDIT, m_buttonContext);
	DDX_Control(pDX, IDC_STATIC_WORKFLOW, m_staticWorkflowLabel);
	DDX_Control(pDX, IDC_STATIC_CONTEXT, m_staticContextLabel);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(FileProcessingDlg, CDialog)
	ON_WM_SIZE()
	ON_WM_GETMINMAXINFO()
	ON_WM_PAINT()
	ON_WM_CLOSE()
	ON_COMMAND(IDC_BTN_RUN, OnBtnRun)
	ON_COMMAND(IDC_BTN_STOP, OnBtnStop)
	ON_COMMAND(IDC_BTN_AUTO_SCROLL, OnBtnAutoScroll)
	ON_WM_TIMER()
	ON_MESSAGE(FP_CLEAR_UI, OnClearUI)
	ON_MESSAGE(FP_PROCESSING_COMPLETE, OnProcessingComplete)
	ON_MESSAGE(FP_PROCESSING_CANCELLING, OnProcessingCancelling)
	ON_MESSAGE(FP_STATUS_CHANGE, OnStatusChange)
	ON_MESSAGE(FP_WORK_ITEM_STATUS_CHANGE, OnWorkItemStatusChange)
	ON_MESSAGE(FP_SUPPLIER_STATUS_CHANGE, OnSupplierStatusChange)
	ON_MESSAGE(FP_QUEUE_EVENT, OnQueueEvent)
	ON_MESSAGE(FP_STATISTICS_UPDATE, OnStatsUpdateMessage)
	ON_MESSAGE(FP_SCHEDULE_INACTIVE, OnScheduleInactive)
	ON_MESSAGE(FP_SCHEDULE_ACTIVE, OnScheduleActive)
	ON_COMMAND(ID_FILE_EXIT, OnFileExit)
	ON_COMMAND(ID_FILE_NEW, OnFileNew)
	ON_COMMAND(ID_FILE_OPEN, OnFileOpen)
	ON_COMMAND(ID_FILE_SAVE, OnFileSave)
	ON_COMMAND(ID_FILE_SAVEAS, OnFileSaveas)
	ON_COMMAND(ID_FILE_REQUIREADMINEDIT, OnFileRequireAdminEdit)
	ON_COMMAND(ID_FILE_LOGINASADMIN, OnFileLoginAsAdmin)
	ON_COMMAND(ID_TOOLS_CHECKFORNEWCOMPONENTS, OnToolsCheckfornewcomponents)
	ON_COMMAND(ID_TOOLS_OPTIONS, OnToolsOptions)
	ON_WM_DROPFILES()
	ON_COMMAND(ID_PROCESS_STARTPROCESSING, OnProcessStartprocessing)
	ON_COMMAND(ID_PROCESS_STOPPROCESSING, OnProcessStopprocessing)
	ON_COMMAND(ID_TOOLS_AUTOSCROLL, OnToolsAutoscroll)
	ON_COMMAND(IDC_BTN_PAUSE, OnBtnPause)
	ON_COMMAND(ID_PROCESS_PAUSEPROCESSING, OnProcessPauseProcessing)
	ON_COMMAND(ID_HELP_ABOUTFILEPROCESSINGMANAGER, OnHelpAboutfileprocessingmanager)
	ON_COMMAND(ID_HELP_FILEPROCESSINGMANAGERHELP, &FileProcessingDlg::OnHelpFileprocessingmanagerhelp)
	ON_COMMAND(ID_BTN_FAM_OPEN, &FileProcessingDlg::OnFileOpen)
	ON_COMMAND(ID_BTN_FAM_SAVE, &FileProcessingDlg::OnFileSave)
	ON_NOTIFY_EX_RANGE(TTN_NEEDTEXT,0x0000,0xFFFF,OnToolTipNotify)
	ON_COMMAND(ID_TOOLS_DATABASEADMINISTRATIONUTILITY, &FileProcessingDlg::OnToolsFAMDBAdmin)
	ON_COMMAND(ID_TOOLS_EDITCUSTOMTAGS, &FileProcessingDlg::OnToolsEditCustomTags)
	ON_COMMAND_RANGE(ID_FAM_MRU_FILE1, ID_FAM_MRU_FILE8, &FileProcessingDlg::OnSelectFAMMRUPopupMenu)
	ON_NOTIFY(TBN_DROPDOWN, AFX_IDW_TOOLBAR, &FileProcessingDlg::OnToolbarDropDown)
	ON_CBN_SELCHANGE(IDC_WORKFLOW_COMBO, &FileProcessingDlg::OnCbnSelchangeWorkflowCombo)
	ON_BN_CLICKED(IDC_CONTEXT_EDIT, &FileProcessingDlg::OnBnClickedContextEdit)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// FileProcessingDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL FileProcessingDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		try
		{
			CDialog::OnInitDialog();

			// initiate the timer event to do the auto-saving
			// don't need to set the timer for auto save if this instance is running as a service
			if ( !m_bRunningAsService)
			{
				// User also needs write permission in the file recovery folder (P13 #4327)
				if (m_pFRM != __nullptr && m_pFRM->isRecoveryFolderWritable())
				{
					SetTimer(giAUTO_SAVE_TIMERID, giAUTO_SAVE_FREQUENCY, NULL);
				}
			}

			SetIcon(m_hIcon, TRUE);			// Set big icon
			SetIcon(m_hIcon, FALSE);			// Set small icon

			// create the toolbar associated with this window
			createToolBar();
			
			// create the status bar
			createStatusBar();

			// [P13:4710] Update the menu and toolbar right away;  Otherwise if later code
			// (such as initializing m_propDatabasePage and the database connection) throws
			// an exception, the menu & toolbar states will not have been initialized.
			updateMenuAndToolbar();
			
			// add all property pages - this will cause the status bar to be updated
			// so this must happen after the status bar is created
			createPropertyPages();

			// create the database status icon updater object
			m_apDatabaseStatusIconUpdater = unique_ptr<DatabaseStatusIconUpdater> 
				(new DatabaseStatusIconUpdater(m_statusBar, gnDB_CONNECTION_STATUS_PANE_ID, this));

			// set the database status icon updater window as the window to receive database status
			// notifications
			getDBPointer()->SetNotificationUIWndHandle((long) m_apDatabaseStatusIconUpdater->m_hWnd);

			// Set flag indicating that controls exist
			m_bInitialized = true;

			// Check for window maximized
			WINDOWPLACEMENT wp;
			wp.length = sizeof( WINDOWPLACEMENT );
			wp.showCmd = SW_SHOWNORMAL;
			if (ma_pCfgMgr->getWindowMaximized())
			{
				// Set flag for window to be maximized
				wp.showCmd = SW_SHOWMAXIMIZED;
			}

			// Retrieve previous ( normal ) window position
			long	lLeft = 0;
			long	lTop = 0;
			long	lWidth = 0;
			long	lHeight = 0;
			ma_pCfgMgr->getWindowPos(lLeft, lTop);
			ma_pCfgMgr->getWindowSize(lWidth, lHeight);

			// Set normal rectangle
			wp.rcNormalPosition = CRect( lLeft, lTop, lLeft + lWidth, lTop + lHeight );

			// Adjust window position based on retrieved settings
			SetWindowPlacement( &wp );

			// Refresh the window
			doResize();

			m_ToolTipCtrl.Create(this, TTS_ALWAYSTIP);

			// Initializes the DatabasePage
			m_propSheet.SetActivePage(&m_propDatabasePage);
			m_propDatabasePage.enableAllControls(true);

			// Clear the UI
			OnClearUI(0, 0);

			DragAcceptFiles();

			// We will initialize the UI with settings from the attached manager
			if (m_pFileProcMgr != __nullptr)
			{
				loadSettingsFromManager();
			}

			loadWorkflowComboBox();

			updateUI();

			// start the processing if requested
			if (m_bRunOnInit)
			{
				OnBtnRun();
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI08866");
	}
	catch (UCLIDException& ue)
	{
		if (m_bRunningAsService)
		{
			ue.log();
		}
		else
		{
			ue.display();
		}

		// check to be sure essential controls have been created
		// and if they have not then close the application
		if (!m_bInitialized)
		{
			CDialog::OnCancel();
		}
	}

	return TRUE;  // return TRUE unless you set the focus to a control
				  // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::OnBtnRun() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Make sure the database settings are using the latest value in customTags if tags
		// are being used
		try
		{
			getFPM()->RefreshDBSettings();
		}
		catch(...)
		{
			try
			{
				updateUIForCurrentDBStatus();
			}
			CATCH_AND_LOG_ALL_EXCEPTIONS("ELI38308");

			throw;
		}

		// set all parameters for file processing manager
		// if saving a setting fails we will not run
		if (!flushSettingsToManager())
		{
			return;
		}

		UCLID_FILEPROCESSINGLib::IFileProcessingManagerPtr ipFPM = getFPM();

		// If auto-saving FPS file is enabled, save the FPS file
		if (m_dlgOptions.getAutoSaveFPSFile())
		{
			if (!saveFile(m_strCurrFPSFilename, true))
			{
				// Do not run if the save failed/was cancelled
				return;
			}
		}
		else if (m_pFRM != __nullptr)
		{
			// sometimes, the processing may cause the program the crash.  This
			// can happen if one or more of the components are poorly written.
			// The user may not have saved the FAM...save the FAM settings in
			// a temporary file so that it can be recovered in case of a crash
			// NOTE: we are passing VARIANT_FALSE as the second argument
			// here because we don't want the internal dirty flag to be
			// effected by this SaveTo() call.
			IPersistStreamPtr ipPersist = ipFPM;
			if (!m_bFPSLocked && ipPersist->IsDirty() == S_OK)
			{
				// Only perform the from save if the file is dirty
				ipFPM->SaveTo(m_pFRM->getRecoveryFileName().c_str(), 
					VARIANT_FALSE);
			}
		}
	
		// Prompt for a password if one is needed. If one was needed but not supplied, processing
		// is not allowed.
		if (!asCppBool(ipFPM->AuthenticateForProcessing()))
		{
			return;
		}

		// Get the index of the Queue log, process log and statistics page
		int iQueueLogIndex = m_propSheet.GetPageIndex(&m_propQueueLogPage);
		int iProcLogIndex = m_propSheet.GetPageIndex(&m_propProcessingPage);
		int iStatisticsIndex = m_propSheet.GetPageIndex(&m_propStatisticsPage);

		// Get the index of the current page
		int iActiveIndex = m_propSheet.GetActiveIndex();

		// Check whether we are processing skipped files or not
		UCLID_FILEPROCESSINGLib::IFileProcessingMgmtRolePtr ipRole = ipFPM->FileProcessingMgmtRole;
		ASSERT_RESOURCE_ALLOCATION("ELI26941", ipRole != __nullptr);
		m_bProcessingSkippedFiles = asCppBool(ipRole->ProcessSkippedFiles);

		// If the current tab is not one of the log tab or the statistics tab
		if (iActiveIndex != iQueueLogIndex && iActiveIndex != iProcLogIndex
			&& iActiveIndex != iStatisticsIndex)
		{
			// Set leftmost visible tab among these tabs: queue log, processing log, and statistics
			// as the current tab 
			if (iQueueLogIndex > 0)
			{
				m_propSheet.SetActivePage(iQueueLogIndex);
			}
			else if (iProcLogIndex > 0)
			{
				m_propSheet.SetActivePage(iProcLogIndex);
			}
			else if (iStatisticsIndex > 0)
			{
				m_propSheet.SetActivePage(iStatisticsIndex);
			}
		}

		// If the processing log page is shown, then enable the progress updates
		if (iProcLogIndex > 0)
		{
			m_propProcessingPage.startProgressUpdates();
		}

		setPagesEnable(false);

		// set the running flag
		m_bRunning = true;

		// Reset the paused flag
		m_bPaused = false;

		// reset the stopped manually flag and start processing
		m_bStoppedManually = false;

		// reset the exceeded exec count flag
		m_bExceededExecCount = false;

		// Reset the initial completed/failed count
		m_nNumInitialCompleted = gnUNINITIALIZED;
		m_nNumInitialFailed = gnUNINITIALIZED;

		// Get a statistics thread going
		setupAndLaunchStatsThread();

		// Set the Stats only running if the Queue log page and processing page are not displayed
		// and the Stats page is
		m_bStatsOnlyRunning =  (iQueueLogIndex  <= 0 ) && (iProcLogIndex <= 0 ) && (iStatisticsIndex > 0);
		
		// Reset the run time timer
		resetRunTimer();

		// Start the run time timer
		startRunTimer();

		// If any exceptions are thrown when the processing is started
		// the menus should still be updated and the exception rethrown
		try
		{
			ipFPM->StartProcessing();
		}
		catch(...)
		{
			try
			{
				updateUIForProcessingComplete();
			}
			CATCH_AND_LOG_ALL_EXCEPTIONS("ELI29348");

			throw;
		}

		updateUI();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08926");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::OnBtnPause() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// if the stats are not the only thing running pause or resume the FPM
		if ( !m_bStatsOnlyRunning )
		{
			m_bPaused = asCppBool(getFPM()->ProcessingPaused);
			if (m_bPaused)
			{
				// resume the processing
				getFPM()->StartProcessing();
			}
			else
			{
				// pause the processing
				getFPM()->PauseProcessing();
			}
		}

		// Pause or resume the stats thread. Pause just stops thread, resume restarts the thread
		if ( m_bPaused )
		{
			setupAndLaunchStatsThread();
			m_bPaused = false;
		}
		else
		{
			stopStatsThread();
			m_bPaused = true;
		}

		// update the UI
		updateMenuAndToolbar();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12732")
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::OnBtnStop() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		try
		{
			CWaitCursor wait;

			// Do not auto-terminate the dialog
			m_bStoppedManually = true;

			// Log a message indicating that the user is stopping the FAM
			UCLIDException ue("ELI15679", "Application trace: The user has stopped File Action Manager processing.");
			ue.addDebugInfo("FPS File",
				m_strCurrFPSFilename.empty() ? "<Not Saved>" : m_strCurrFPSFilename);
			ue.log();

			// If only the stats thread is running need to stop it here as well as update
			// the menus and running flags
			if ( m_bStatsOnlyRunning )
			{
				// Notify that the processing has been cancelled
				OnProcessingCancelling(0, 0);

				// Stop the statistics thread
				stopStatsThread();

				// Update the status for stats only to non running
				m_bStatsOnlyRunning = false;
			
				// This needs to be reset here because there will not be a processing completed message 
				m_bRunning = false;
			
				// Stop run timer
				stopRunTimer();

				// Enable the action page after processing
				if (isPageDisplayed(kActionPage))
				{
					m_propActionPage.setEnabled(true);
				}

				// If the processing log page is shown, notify it to stop progress updates
				if (isPageDisplayed(kProcessingLogPage))
				{
					m_propProcessingPage.stopProgressUpdates();
				}

				updateUI();

				// Re-enable the process menu since no complete message will be sent
				CMenu* pMenu = GetMenu();
				pMenu->EnableMenuItem(1, MF_BYPOSITION | MF_ENABLED);

				// Redraw the menu bar
				DrawMenuBar();

				// Log a FAM has stopped processing message [LRCAU #5302]
				UCLID_FILEPROCESSINGLib::IRoleNotifyFAMPtr ipRole(getFPM());
				ASSERT_RESOURCE_ALLOCATION("ELI25564", ipRole != __nullptr);
				ipRole->NotifyProcessingCompleted();
			}
			else
			{
				// Disable the buttons so they will not be pressed while the FAM is stopping
				m_toolBar.GetToolBarCtrl().EnableButton(IDC_BTN_RUN, FALSE);
				m_toolBar.GetToolBarCtrl().EnableButton(IDC_BTN_PAUSE, FALSE);
				m_toolBar.GetToolBarCtrl().EnableButton(IDC_BTN_STOP, FALSE);

				// update the menu items
				CMenu* pMenu = GetMenu();
			
				// Disable the menus so they will not be pressed while the FAM is stopping
				pMenu->EnableMenuItem(ID_PROCESS_STARTPROCESSING, MF_BYCOMMAND | MF_GRAYED);
				pMenu->EnableMenuItem(ID_PROCESS_PAUSEPROCESSING, MF_BYCOMMAND | MF_GRAYED);
				pMenu->EnableMenuItem(ID_PROCESS_STOPPROCESSING, MF_BYCOMMAND | MF_GRAYED);

				// Add a status message that the FAM is stopping.
				m_statusBar.SetText("Stopping", gnSTATUS_TEXT_STATUS_PANE_ID, 0);
			
				// Files are processing. Stop them.
				getFPM()->StopProcessing();
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI08927")
	}
	catch (UCLIDException &ue)
	{
		// [FlexIDSCore:5003]
		// If someone clicks the stop button before a stop initiated by closing the verification
		// window has been completed, there's no need to display an exception to the user; log
		// instead.
		if (ue.getTopELI() == "ELI12734")
		{
			ue.log();
		}
		else
		{
			ue.display();
		}
	}
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::OnBtnAutoScroll() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Get index of toolbar button
		int nIndex = m_toolBar.CommandToIndex(IDC_BTN_AUTO_SCROLL);
		if (nIndex < 0)
		{
			return;
		}

		// Get the status of auto-scrolling and set to the log
		// pages and registry
		TBBUTTON button;
		m_toolBar.GetToolBarCtrl().GetButton(nIndex, &button);
		if (button.fsState & TBSTATE_CHECKED)
		{
			m_propProcessingPage.setAutoScroll(true);
			m_propQueueLogPage.setAutoScroll(true);
			ma_pCfgMgr->setAutoScrolling(true);
		}
		else
		{
			m_propProcessingPage.setAutoScroll(false);
			m_propQueueLogPage.setAutoScroll(false);
			ma_pCfgMgr->setAutoScrolling(false);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09316")
}
//-------------------------------------------------------------------------------------------------
BOOL FileProcessingDlg::PreTranslateMessage(MSG* pMsg) 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		if (pMsg->message == WM_KEYDOWN)
		{
			// translate accelerators
			static HACCEL hAccel = LoadAccelerators(AfxGetApp()->m_hInstance, 
				MAKEINTRESOURCE(IDR_ACCELERATORS));
			if (TranslateAccelerator(m_hWnd, hAccel, pMsg))
			{
				// since the message has been handled, no further dispatch is needed
				return TRUE;
			}
		}

		// make sure the tool tip control is a valid window before passing messages to it
		if (asCppBool(::IsWindow(m_ToolTipCtrl.m_hWnd)))
		{
			// show tooltips
			m_ToolTipCtrl.RelayEvent(pMsg);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08896")
	
	return CDialog::PreTranslateMessage(pMsg);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::OnPaint() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		CPaintDC dc(this); // device context for painting
		
		// get the toolbar height and the dialog width
		CRect rectDlg;
		GetWindowRect(&rectDlg);
		CRect rectToolBar;
		m_toolBar.GetWindowRect(&rectToolBar);
		int iToolBarHeight = rectToolBar.Height();
		int iDialogWidth = rectDlg.Width();
		
		// with gray and white pens, draw horizontal lines that span the entire width
		// of the dialog, and that are just below the toolbar buttons
		CPen penGray;
		CPen penWhite;
		penGray.CreatePen(PS_SOLID, 0, RGB(128, 128, 128));
		penWhite.CreatePen(PS_SOLID, 0, RGB(255, 255, 255));

		// First the gray line
		dc.SelectObject(&penGray);
		dc.MoveTo(0, iToolBarHeight);
		dc.LineTo(iDialogWidth, iToolBarHeight);

		// Next the white line, one pixel below the gray
		dc.SelectObject(&penWhite);
		dc.MoveTo(0, iToolBarHeight + 1);
		dc.LineTo(iDialogWidth, iToolBarHeight + 1);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08897")
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::OnSize(UINT nType, int cx, int cy) 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		CDialog::OnSize(nType, cx, cy);
		
		doResize();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08898")
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::OnGetMinMaxInfo(MINMAXINFO* lpMMI)
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Minimum width to allow display of buttons
		lpMMI->ptMinTrackSize.x = m_sizeMinimumPropPage.cx + 2 * 
			GetSystemMetrics(SM_CYSIZEFRAME);

		// Minimum height to allow display of list
		lpMMI->ptMinTrackSize.y = m_sizeMinimumPropPage.cy + 
			giMIN_LISTBOX_PLUS_TOOLBAR_HEIGHT + 
			GetSystemMetrics(SM_CXSIZEFRAME) + GetSystemMetrics(SM_CYCAPTION) + 1 +
			5;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08899")
}
//-------------------------------------------------------------------------------------------------
BOOL FileProcessingDlg::OnToolTipNotify(UINT id, NMHDR * pNMHDR, LRESULT *pResult)
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	BOOL retCode = FALSE;
	
	TOOLTIPTEXT* pTTT = (TOOLTIPTEXT*)pNMHDR;
	UINT nID = pNMHDR->idFrom;
	if (pNMHDR->code == TTN_NEEDTEXT && (pTTT->uFlags & TTF_IDISHWND))
	{
		// idFrom is actually the HWND of the tool, ex. button control, edit control, etc.
		nID = ::GetDlgCtrlID((HWND)nID);
	}

	if (nID)
	{
		retCode = TRUE;
		pTTT->hinst = AfxGetResourceHandle();
		pTTT->lpszText = MAKEINTRESOURCE(nID);
	}

	return retCode;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::OnClose() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// The app cannot be closed while processing is occurring
		// If the user attempts this we will alert them that it is not allowed
		if (m_bRunning)
		{
			if ( m_bRunningAsService )
			{
				OnBtnStop();
			}
			else
			{

				MessageBox( "Files are currently being processed.\nProcessing must be stopped before closing the application.", 
					"Error", MB_OK | MB_ICONINFORMATION );
			}
			return;
		}

		// If the settings have been modified prompt the user for a save
		if (!checkForSave())
		{
			return;
		}

		saveWindowSettings();

		// we are exiting the application at the user's request, regardless
		// of whether they are saving the current FAM or not. 
		// it is OK to delete the recovery file
		if ( m_pFRM != __nullptr )
		{
			m_pFRM->deleteRecoveryFile();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08906")
	
	CDialog::OnClose();
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::OnTimer(UINT nIDEvent)
{
	try
	{
		switch (nIDEvent)
		{
		case giAUTO_SAVE_TIMERID:
			// Since we received a timer event to perform an auto-save, do the auto-save
			// NOTE: we are passing VARIANT_FALSE as the second argument
			// here because we don't want the internal dirty flag to be
			// effected by this SaveTo() call.
			if (!m_bRunning)
			{
				// Only do auto-save if processing / supplying / statistics has 
				// not started (P13 #4168)
				if ( m_pFRM != __nullptr)
				{
					// Only save if the current file is dirty [LRCAU #6012]
					UCLID_FILEPROCESSINGLib::IFileProcessingManagerPtr ipFPM = getFPM();
					IPersistStreamPtr ipPersist = ipFPM;
					if (!m_bFPSLocked && ipPersist->IsDirty() == S_OK)
					{
						ipFPM->SaveTo(m_pFRM->getRecoveryFileName().c_str(), 
							VARIANT_FALSE);
					}
				}
			}
			break;

		default:
			// for all other timers, just call the base class method
			CWnd::OnTimer(nIDEvent);
			break;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14121")
}
//-------------------------------------------------------------------------------------------------
LRESULT FileProcessingDlg::OnSupplierStatusChange(WPARAM wParam, LPARAM lParam)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// Make sure the page is initialized before using it
		if (isPageDisplayed(kQueueSetupPage))
		{
			// Pass the notification on to the Scope tab
			m_propQueueSetupPage.updateSupplierStatus( wParam, lParam );
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI13943");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT FileProcessingDlg::OnQueueEvent(WPARAM wParam, LPARAM lParam)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// Make sure the page is initialized before using it
		if (isPageDisplayed(kQueueLogPage))
		{
			// Add the event to the Queue Log Page
			m_propQueueLogPage.addEvent( (FileSupplyingRecord*)wParam );
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI14281");
	
	return 0;
}
//-------------------------------------------------------------------------------------------------
// wParam = ActionStatistics * with the new stats
// lParam = 0
LRESULT FileProcessingDlg::OnStatsUpdateMessage(WPARAM wParam, LPARAM lParam)
{
	try
	{
		// The statistics page sometimes needs one last update; such as when the processing is 
		// done, but the stats are still showing 99.9% complete. 
		if(m_bRunning == false)
		{
			// If the FAM is not running, stop the timer.
			stopRunTimer();
		}

		// Variables for processing data
		LONGLONG nTotalBytes = 0;
		long nTotalDocs = 0;
		long nTotalPages = 0;
		unsigned long unTotalProcTime = 0;

		// Get the total Processing time in order to update the time statistics
		if (isPageDisplayed(kProcessingLogPage))
		{
			// If the processing log page doesn't exist, these will remain 0 and be used
			// as a flag for the enabling or disabling the local page.
			unTotalProcTime = m_propProcessingPage.getTotalProcTime();
			m_propProcessingPage.getLocalStats( nTotalBytes, nTotalDocs, nTotalPages );
		}

		// Cast the wParam objects
		UCLID_FILEPROCESSINGLib::IActionStatisticsPtr ipActionStatsNew;
		ipActionStatsNew.Attach((UCLID_FILEPROCESSINGLib::IActionStatistics *) wParam);
		ASSERT_RESOURCE_ALLOCATION("ELI15050", ipActionStatsNew != __nullptr );

		// Make sure the Stats page is initialized and enabled
		if (isPageDisplayed(kStatisticsPage) && m_propStatisticsPage.getInit() )
		{
			// If meaningful stats are present, use them to update all stats
			m_propStatisticsPage.populateStats(m_stopWatch, 
				ipActionStatsNew, nTotalBytes, nTotalDocs, 
				nTotalPages, unTotalProcTime );
		}

		updateStats(ipActionStatsNew);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS( "ELI14508" )

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT FileProcessingDlg::OnStatusChange(WPARAM wParam, LPARAM lParam)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		const FileProcessingRecord *pTask = (const FileProcessingRecord*)lParam;
		ERecordStatus eOldStatus = (ERecordStatus)wParam;
		ERecordStatus eNewStatus = pTask->m_eStatus;

		// Make sure the page is initialized before using it
		if (isPageDisplayed(kProcessingLogPage))
		{
			m_propProcessingPage.onStatusChange(pTask, eOldStatus);
		}		

		updateStatusBar(false);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI11610");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT FileProcessingDlg::OnClearUI(WPARAM wParam, LPARAM lParam)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// Make sure the page is initialized before using it
		if (isPageDisplayed(kProcessingLogPage))
		{
			m_propProcessingPage.clear();
		}
		// Make sure the page is initialized before using it
		if (isPageDisplayed(kStatisticsPage))
		{
			m_propStatisticsPage.clear();
		}
		// Make sure the page is initialized before using it
		if (isPageDisplayed(kQueueLogPage))
		{
			m_propQueueLogPage.clear();
		}

		// Reset counts
		m_nNumTotalDocs = 0;
		m_nNumCompletedProcessing = 0;
		m_nNumFailed = 0;
		m_nNumPending = 0;
		m_nNumSkipped = 0;
		m_nNumCurrentlyProcessing = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11611");
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT FileProcessingDlg::OnProcessingComplete(WPARAM wParam, LPARAM lParam)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		updateUIForProcessingComplete();

		// If any files failed we will automatically show the user the failed list on the report page.
		if (m_nNumFailed > 0)
		{
			m_propSheet.SetActivePage(&m_propProcessingPage);
		}

		// The dialog will only auto-terminate on completion of processing if...
		// 1) The user has instructed this to happen by setting CloseOnComplete
		// 2) Processing was not manually stopped
		// 3) No files failed during processing
		// 4) Always terminates if running as a service
		int numFailed = m_nNumFailed - m_nNumInitialFailed;
 
		if (m_bRunningAsService || (!m_bStoppedManually 
				&& ((m_bCloseOnComplete && numFailed <= 0) || m_bForceCloseOnComplete )))
		{
			// Since we are exiting the application automatically
			// it is OK to delete the recovery file
			if ( m_pFRM != __nullptr )
			{
				m_pFRM->deleteRecoveryFile();
			}
			PostQuitMessage(0);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11612");
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT FileProcessingDlg::OnProcessingCancelling(WPARAM wParam, LPARAM lParam)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// Do not auto-terminate the dialog
		m_bStoppedManually = true;

		// When processing has been stopped, this dialog
		// will be notified. Until then, disable the stop button.
		m_toolBar.GetToolBarCtrl().EnableButton(IDC_BTN_STOP, FALSE);

		// If the FAM is currently paused do not disable the pause button [FlexIDSCore #5316]
		if (!m_bPaused)
		{
			m_toolBar.GetToolBarCtrl().EnableButton(IDC_BTN_PAUSE, FALSE);
		}
		
		// Disable the "process" menu
		CMenu* pMenu = GetMenu();
		pMenu->EnableMenuItem(1, MF_BYPOSITION | MF_GRAYED);

		// Redraw the menu bar
		DrawMenuBar();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI23468");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT FileProcessingDlg::OnScheduleInactive(WPARAM wParam, LPARAM lParam)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		m_strProcessingStateString = "Processing Inactive";
		setCurrFPSFile(m_strCurrFPSFilename);

		updateUI();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI28343");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT FileProcessingDlg::OnScheduleActive(WPARAM wParam, LPARAM lParam)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		m_strProcessingStateString = "";
		setCurrFPSFile(m_strCurrFPSFilename);

		updateUI();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI28342");

	return 0;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::OnFileNew() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// Test if the FAM is running
		if (m_bRunning)
		{
			return;
		}

		// If the settings have been modified prompt the user for a save
		if (!checkForSave())
		{
			return;
		}

		// Clear the FPM
		getFPM()->Clear();

		// Clear the current context
		clearContext();

		// Update the database page Server and DBName
		m_propDatabasePage.setConnectionInfo("", "", "");
		
		// With all of the data cleared a new database config file should be selected before the action tab is displayed
		// so if it is displayed it should be removed
		set<EDlgTabPage> setPages;
		setPages.insert(kDatabasePage);
		updateTabs(setPages);

		updateUI();

		// Clear the Report and Status tabs, update UI
		setCurrFPSFile( "" );

		// Delete recovery file - user has started a new file
		if ( m_pFRM != __nullptr )
		{
			m_pFRM->deleteRecoveryFile();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12740");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::OnFileOpen() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// Test if the FAM is running
		if (!m_bRunning)
		{
			openFile(string(""), false);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10959");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::OnDropFiles(HDROP hDropInfo)
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// setup hDropInfo to automatically be released when we go out of scope
		DragDropFinisher finisher(hDropInfo);

		// Only handle drag and drop of files if not currently processing
		// [FlexIDSCore #3275]
		if (!m_bRunning)
		{
			unsigned int iNumFiles = DragQueryFile(hDropInfo, 0xFFFFFFFF, NULL, NULL);

			// Do not allow drag and drop of multiple files [LRCAU #5135]
			if (iNumFiles > 1)
			{
				MessageBox("Cannot open more than one file!", "Cannot Open Multiple Files",
					MB_ICONERROR | MB_OK);
				return;
			}

			// get the full path to the dragged filename
			char pszFile[MAX_PATH + 1] = {0};
				DragQueryFile(hDropInfo, 0, pszFile, MAX_PATH);

			string strFile = pszFile;

			string strExtension = getExtensionFromFullPath(strFile, true);
			// if this is an FPS file
			if (strExtension == gstrFILE_PROCESSING_SPECIFICATION_EXT)
			{
				// Attempt to open the file
				openFile(strFile, false);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10966")
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::OnFileSave() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// Test if the FAM is running 
		// Save should be allowed even when settings are not runnable  
		// https://extract.atlassian.net/browse/ISSUE-12798
		if (!m_bRunning)
		{
			saveFile(m_strCurrFPSFilename, true);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10960");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::OnFileSaveas() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		saveFile(string(""), true);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10961");
	
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::OnFileExit() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// If the settings have been modified prompt the user for a save
		if (!checkForSave())
		{
			return;
		}

		saveWindowSettings();

		// we are exiting the application at the user's request, regardless
		// of whether they are saving the current FAM or not. 
		// it is OK to delete the recovery file
		if ( m_pFRM != __nullptr )
		{
			m_pFRM->deleteRecoveryFile();
		}
		
		CDialog::OnCancel();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10962");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::OnFileLoginAsAdmin()
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		VARIANT_BOOL vbCanceled;
		// Must be logged in as admin
		if (asCppBool(getDBPointer()->IsConnected) && !asCppBool(getDBPointer()->LoggedInAsAdmin))
		{
			getDBPointer()->ShowLogin(VARIANT_TRUE, &vbCanceled);
			updateMenuAndToolbar();
		}
			
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI43517");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::OnFileRequireAdminEdit()
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		VARIANT_BOOL vbCanceled;
		// Must be logged in as admin -- if the connection isn't good this the value can't be changed
		if (asCppBool(getDBPointer()->IsConnected) && 
			(asCppBool(getDBPointer()->LoggedInAsAdmin) || asCppBool(getDBPointer()->ShowLogin(VARIANT_TRUE, &vbCanceled))))
		{
			m_bRequireAdminEdit = !m_bRequireAdminEdit;
			updateMenuAndToolbar();
		}

	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI43531");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::OnProcessStartprocessing() 
{
	OnBtnRun();
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::OnProcessStopprocessing() 
{
	OnBtnStop();	
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::OnProcessPauseProcessing() 
{
	OnBtnPause();
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::OnToolsCheckfornewcomponents() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// create instance of the category manager
		ICategoryManagerPtr ipCatMgr(CLSID_CategoryManager);
		ASSERT_RESOURCE_ALLOCATION("ELI09397", ipCatMgr != __nullptr);

		// create a vector of all categories we care about.
		IVariantVectorPtr ipCategoryNames(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI18166", ipCategoryNames != __nullptr);

		ipCategoryNames->PushBack(get_bstr_t(FP_FILE_PROC_CATEGORYNAME.c_str()));
		ipCategoryNames->PushBack(get_bstr_t(FP_FILE_SUPP_CATEGORYNAME.c_str()));
		ipCategoryNames->PushBack(get_bstr_t(FP_FAM_CONDITIONS_CATEGORYNAME.c_str()));
		ipCategoryNames->PushBack(get_bstr_t(FP_FAM_REPORTS_CATEGORYNAME.c_str()));
		ipCategoryNames->PushBack(get_bstr_t(INPUTFUNNEL_IR_CATEGORYNAME.c_str()));

		// Also update the product-specific database items [P13 #4962]
		ipCategoryNames->PushBack(get_bstr_t(FP_FAM_PRODUCT_SPECIFIC_DB_MGRS.c_str()));

		ipCatMgr->CheckForNewComponents(ipCategoryNames);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10963");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::OnToolsOptions()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		m_dlgOptions.DoModal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12453");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::OnToolsAutoscroll() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		////////////////////////////////
		// Treat this as a toolbar press
		////////////////////////////////

		// Get index of toolbar button
		int nIndex = m_toolBar.CommandToIndex( IDC_BTN_AUTO_SCROLL );
		if (nIndex < 0)
		{
			return;
		}

		// Get current button state
		TBBUTTON button;
		m_toolBar.GetToolBarCtrl().GetButton( nIndex, &button );

		// Toggle the button
		if (button.fsState & TBSTATE_CHECKED)
		{
			// "Release" the button
			m_toolBar.GetToolBarCtrl().CheckButton( IDC_BTN_AUTO_SCROLL, FALSE );
		}
		else
		{
			// "Press" the button
			m_toolBar.GetToolBarCtrl().CheckButton( IDC_BTN_AUTO_SCROLL, TRUE );
		}

		// Handle the toolbar press
		OnBtnAutoScroll();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12584");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::OnHelpAboutfileprocessingmanager()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{		
		// Display the About box with version information
		CFPAboutDlg dlgAbout;
		dlgAbout.DoModal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13154")
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::OnHelpFileprocessingmanagerhelp()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
#ifdef _DEBUG
		MessageBox("Please launch Help file manually from the network location.", "Help File", MB_OK);
		return;
#endif
		
		
		// All help files should be in Extract Systems\Help
		// Binaries are in Extract Systems\CommonComponents
		string strExtractFolder = getCurrentProcessEXEDirectory();
		string strExtractSystemFolderName("\\Extract Systems");
		int nBinPos = strExtractFolder.rfind(strExtractSystemFolderName);
		if (nBinPos == string::npos )
		{
			UCLIDException ue("ELI13155", "Can't find Help file.");
			ue.addDebugInfo("Extract Systems Folder", strExtractFolder);
			throw ue;
		}

		// remove the Bin folder and FlexIndexComponents folder
		string strHelpPath = strExtractFolder.substr(0, 
			nBinPos + strExtractSystemFolderName.length() );

		// Initialize the paths to possible help files
		string strFlexHelpPath = strHelpPath + "\\Help\\FLEXIndex.chm";
		string strIDShieldHelpPath = strHelpPath + "\\Help\\IDShield.chm";
		string strLabDEHelpPath = strHelpPath + "\\Help\\LabDE.chm";

		// Check for FLEXIndex Help file
		if (isFileOrFolderValid( strFlexHelpPath ))
		{
			::runEXE("hh.exe", strFlexHelpPath );
		}
		// Look for IDShield Help file
		else if (isFileOrFolderValid( strIDShieldHelpPath )) 
		{
			::runEXE("hh.exe", strIDShieldHelpPath );
		}
		else if (isFileOrFolderValid( strLabDEHelpPath )) 
		{
			::runEXE("hh.exe", strLabDEHelpPath );
		}
		else
		{
			// Create and throw exception
			UCLIDException ue( "ELI15653", "Unable to find Help file." );
			ue.addDebugInfo( "Flex Help Path", strFlexHelpPath );
			ue.addDebugInfo( "ID Shield Help Path", strIDShieldHelpPath );
			ue.addDebugInfo( "LabDE Help Path", strLabDEHelpPath );
			throw ue;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13156")
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::OnToolsFAMDBAdmin()
{
	try
	{
		// Get the path of the FAMDBAdmin app
		string strModulePath = getModuleDirectory(_Module.m_hInst);
		
		// Check for FAMDBAdmin.exe in the install folder
		string strEXEPath = strModulePath + "\\" + gstrFLEX_INDEX_BIN_RELATIVE_CC + gstrFAMDBADMIN_FILENAME;

		// Resolve the relative path
		simplifyPathName(strEXEPath);

		// If path does not exist 
		if (!isFileOrFolderValid(strEXEPath))
		{
			// Check in the same folder as UCLIDFileProcessing.dll
			string strEXEPath2 = strModulePath + "\\" + gstrFAMDBADMIN_FILENAME;
			if (!isFileOrFolderValid(strEXEPath2))
			{
				string strMsg = "Could not find " + gstrFAMDBADMIN_FILENAME;
				UCLIDException ue("ELI17613", strMsg);
				ue.addDebugInfo("Path 1", strEXEPath);
				ue.addDebugInfo("Path 2", strEXEPath2);
				throw ue;
			}
			
			// Set the exe path
			strEXEPath = strEXEPath2;
		}

		string strParameters = "";

		// Get the Server and database name
		string strServer = asString(getDBPointer()->DatabaseServer);
		if (!strServer.empty())
		{
			strParameters = "\"" + strServer + "\" \""
				+ asString(getDBPointer()->DatabaseName) + "\"";

			string strAdvConnStrProperties =
				asString(getDBPointer()->AdvancedConnectionStringProperties);
			if (!strAdvConnStrProperties.empty())
			{
				strParameters += " \"" + strAdvConnStrProperties + "\"";
			}
		}

		// Start the File Action Manager
		runEXE(strEXEPath, strParameters);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17611");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::OnToolsEditCustomTags()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		editCustomTags();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI38104");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::OnToolbarDropDown(NMHDR* pNMHDR, LRESULT *plr)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{	
		NMTOOLBAR *pTB = (NMTOOLBAR *)pNMHDR;
		UINT nID = pTB->iItem;
		
		// Switch on button command id's.
		if (nID == ID_BTN_FAM_OPEN)
		{
			// load the popup MRU file list menu
			CMenu menuLoader;
			if (!menuLoader.LoadMenu(IDR_MENU_FAM_MRU))
			{
				throw UCLIDException("ELI32003", "Failed to load Most Recent Used File list.");
			}
			
			CMenu* pPopup = menuLoader.GetSubMenu(0);
			if (pPopup != __nullptr)
			{
				m_upMRUList->readFromPersistentStore();
				long nSize = m_upMRUList->getCurrentListSize();
				if (nSize > 0)
				{
					// remove the "No File" item from the menu
					pPopup->RemoveMenu(ID_MNU_FAM_MRU, MF_BYCOMMAND);
				}

				for(long i = nSize-1; i >= 0; i--)
				{
					CString pszFile(m_upMRUList->at(i).c_str());
					if (!pszFile.IsEmpty())
					{
						pPopup->InsertMenu(0, MF_BYPOSITION, ID_FAM_MRU_FILE1+i, pszFile);
					}
				}

				CRect rc;
				m_toolBar.SendMessage(TB_GETRECT, pTB->iItem, (LPARAM)&rc);
				m_toolBar.ClientToScreen(&rc);
				
				pPopup->TrackPopupMenu( TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_VERTICAL,
					rc.left, rc.bottom, this, &rc);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI32004");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::OnSelectFAMMRUPopupMenu(UINT nID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{	
		if (nID >= ID_FAM_MRU_FILE1 && nID <= ID_FAM_MRU_FILE8)
		{
			// get the current selected file index of MRU list
			int nCurrentSelectedFileIndex = nID - ID_FAM_MRU_FILE1;

			openFile(m_upMRUList->at(nCurrentSelectedFileIndex), false);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI32006");
}
//-------------------------------------------------------------------------------------------------
LRESULT FileProcessingDlg::OnWorkItemStatusChange(WPARAM wParam, LPARAM lParam)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		const FPWorkItem *pWorkItem = (const FPWorkItem*)lParam;
		EWorkItemStatus eOldStatus = (EWorkItemStatus)wParam;
		EWorkItemStatus eNewStatus = pWorkItem->m_status;

		// Make sure the page is initialized before using it
		if (isPageDisplayed(kProcessingLogPage))
		{
			m_propProcessingPage.onWorkItemStatusChange(pWorkItem, eOldStatus);
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI37264");

	return 0;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::OnCbnSelchangeWorkflowCombo()
{
	try
	{
		CString cSelectedItem;
		int index = m_comboBoxWorkflow.GetCurSel();

		// If there is no selection set workflow to blank
		if (index < 0)
		{
			getFPM()->ActiveWorkflow = "";
		}
		else
		{
			m_comboBoxWorkflow.GetLBText(index, cSelectedItem);
			getFPM()->ActiveWorkflow = get_bstr_t(cSelectedItem);
		}
		updateUIForCurrentDBStatus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI42075");
}
//--------------------------------------------------------------------------------------------------
void FileProcessingDlg::OnBnClickedContextEdit()
{
	try
	{
		editCustomTags();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI42076");
}

//--------------------------------------------------------------------------------------------------
// INotifyDBConfigChanged class
//--------------------------------------------------------------------------------------------------
void FileProcessingDlg::OnDBConfigChanged(string& rstrServer, string& rstrDatabase,
										  string& rstrAdvConnStrProperties)
{
	try
	{
		// Set the database status on the database page
		m_propDatabasePage.setDBConnectionStatus(gstrCONNECTING);
		emptyWindowsMessageQueue();
		CWaitCursor cWait;

		// The connection parameters only need to be change on the FPM since the FPM will set these
		// values of the DB.
		// [p13 #4581 && #4580]
		// NOTE: do this before the call to ResetDBConnection
		//		 because ResetDBConnection will throw exception if
		//		 the Server or DBName do not exist
		getFPM()->DatabaseServer = rstrServer.c_str();
		getFPM()->DatabaseName = rstrDatabase.c_str();
		getFPM()->AdvancedConnectionStringProperties = rstrAdvConnStrProperties.c_str();

		// Determine if tags need to be expanded
		bool bTagsToExpand = asCppBool(m_ipFAMTagManager->StringContainsTags(rstrServer.c_str())) ||
			asCppBool(m_ipFAMTagManager->StringContainsTags(rstrDatabase.c_str())) ||
			asCppBool(m_ipFAMTagManager->StringContainsTags(rstrAdvConnStrProperties.c_str()));

		if (bTagsToExpand)
		{
			// The FPS file needs to have been saved
			if (m_strCurrFPSFilename.empty() && m_ipFAMTagManager->FPSFileDir.length() == 0)
			{
				// Inform the users that the configuration needs to be saved
				if (MessageBox("The configuration needs to be saved for the context to be identified.\r\nSave now?",
					"Save Configuration?", MB_ICONEXCLAMATION | MB_YESNO) == IDYES)
				{
					// Prompt for save
					bTagsToExpand = saveFile(string(""), false);
				}
			}
			
			if (bTagsToExpand)
			{
				refreshContext();

				// Expand tags
				string strExpandedServerName = m_ipFAMTagManager->ExpandTags(rstrServer.c_str(), "");
				string strExpandedDBName = m_ipFAMTagManager->ExpandTags(rstrDatabase.c_str(), "");
				string strExpandedAdvConnStrProperties =
					m_ipFAMTagManager->ExpandTags(rstrAdvConnStrProperties.c_str(), "");

				// Determine if there are still tags that need to be expanded
				bTagsToExpand = asCppBool(m_ipFAMTagManager->StringContainsTags(strExpandedServerName.c_str())) || 
					asCppBool(m_ipFAMTagManager->StringContainsTags(strExpandedDBName.c_str())) ||
					asCppBool(m_ipFAMTagManager->StringContainsTags(strExpandedAdvConnStrProperties.c_str()));

				// If tags still need to be expanded open the edit context tags UI
				if (bTagsToExpand && !m_bUpdatingConnection)
				{
					getTagUtility()->EditCustomTags((long)GetSafeHwnd());

					refreshContext();
					strExpandedServerName = m_ipFAMTagManager->ExpandTags(rstrServer.c_str(), "");
					strExpandedDBName = m_ipFAMTagManager->ExpandTags(rstrDatabase.c_str(), "");
					strExpandedAdvConnStrProperties =
						m_ipFAMTagManager->ExpandTags(rstrAdvConnStrProperties.c_str(), "");
				
					bTagsToExpand = asCppBool(m_ipFAMTagManager->StringContainsTags(strExpandedServerName.c_str())) || 
						asCppBool(m_ipFAMTagManager->StringContainsTags(strExpandedDBName.c_str())) ||
						asCppBool(m_ipFAMTagManager->StringContainsTags(strExpandedAdvConnStrProperties.c_str()));

					if (!bTagsToExpand)
					{
						m_bUpdatingConnection = true;
						m_propDatabasePage.refreshConnection();
					}
				}
			}
		}

		// Only try to connect if no tags need to be expanded
		if (!bTagsToExpand)
		{
			// Reset the database connection with current tags refreshed
			getFPM()->RefreshDBSettings();

			// In case path tags were expanded, return the literal database connection properties we
			// actually connected to.
			rstrServer = getDBPointer()->DatabaseServer;
			rstrDatabase = getDBPointer()->DatabaseName;
			rstrAdvConnStrProperties = getDBPointer()->AdvancedConnectionStringProperties;
		}
		else 
		{
			getDBPointer()->CloseAllDBConnections();
		}

		m_propDatabasePage.showDBServerTag(true);
		m_propDatabasePage.showDBNameTag(true);
		m_bUpdatingConnection = false;
	}
	catch ( ... )
	{
		m_bUpdatingConnection = false;

		// Set the status string
		try
		{
			// Still need to update the UI if there was an exception
			updateUIForCurrentDBStatus();
		}
		CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16642");

		// Rethrow the original exception
		throw;
	}

	// Update the status and UI
	updateUIForCurrentDBStatus();
	updateMenuAndToolbar();
}
//-------------------------------------------------------------------------------------------------
bool FileProcessingDlg::PromptToSelectContext(bool& rbDBTagsAvailable)
{
	bool bResult = false;

	if (m_strCurrFPSFilename.empty())
	{
		bResult = displayRecentContextSelection(); 
	}
	else if (!isValidContext())
	{
		editCustomTags();

		bResult = isValidContext();
	}

	if (bResult)
	{
		rbDBTagsAvailable = areDatabaseTagsDefined();
	}

	return bResult;
}

//-------------------------------------------------------------------------------------------------
// Public Functions
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::setRecordManager(FPRecordManager* pRecordMgr)
{
	ASSERT_ARGUMENT("ELI10146", pRecordMgr != __nullptr);
	m_pRecordMgr = pRecordMgr;

	m_propStatisticsPage.setRecordManager(pRecordMgr);
	m_propProcessingPage.setRecordManager(pRecordMgr);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::setRunOnInit(bool bRunOnInit)
{
	m_bRunOnInit = bRunOnInit;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::setCloseOnComplete(bool bCloseOnComplete)
{
	m_bCloseOnComplete = bCloseOnComplete;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::setForceCloseOnComplete( bool bForceClose)
{
	m_bForceCloseOnComplete = bForceClose;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::setNumberOfDocsToExecute(long lNumberOfDocToExecute)
{
	// Number must be positive or 0
	ASSERT_ARGUMENT("ELI29185", lNumberOfDocToExecute >= 0);

	m_nNumberOfDocsToExecute = lNumberOfDocToExecute;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::updateTabs(const set<EDlgTabPage>& setPages)
{
	// Save the active page so it can be restored later if possible
	CPropertyPage *pActivePage = m_propSheet.GetActivePage();
	
	// Lock Window Updates
	SuspendWindowUpdates wndUpdatelocker(m_propSheet);
	
	// set remove flag to false
	bool bRemove = false;
	
	// Remove only the pages that are not to be displayed or after the first page that should be
	// displayed but is currently not displayed - this is so it this is called while processing,
	// the pages don't lose the data currently displayed. When processing the only page that will
	// be added or removed is the statistics page which is the last page.
	for ( EDlgTabPage ePage = kActionPage; ePage <= kStatisticsPage; ePage = (EDlgTabPage)(ePage + 1))
	{
		bool bDisplayPage =  setPages.find(ePage) != setPages.end();

		bool bPageDisplayed = isPageDisplayed(ePage);

		if (!bRemove && bDisplayPage && !bPageDisplayed)
		{
			bRemove = true;
		}
		else if (bRemove || !bDisplayPage)
		{
			// remove the page
			removePage(ePage);
		}
	}

	// Add pages that should be displayed
	for ( EDlgTabPage ePage = kActionPage; ePage <= kStatisticsPage; ePage = (EDlgTabPage)(ePage + 1))
	{
		// Check if the current page is in the set of pages to display
		if (setPages.find(ePage) != setPages.end())
		{
			// Display the page
			displayPage(ePage);
		}
	}

	// If there was an active page 
	if ( m_propSheet.GetPageIndex(pActivePage) >= 0 )
	{
		// set back to the active page
		m_propSheet.SetActivePage(pActivePage);
	}
	else if (isPageDisplayed(kActionPage))
	{
		// Then set it back to the Action Page
		m_propSheet.SetActivePage(&m_propActionPage);
	}
	else
	{
		// if the action page is not displayed set the database page as the active page
		m_propSheet.SetActivePage(&m_propDatabasePage);
	}
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::updateMenuAndToolbar()
{
	bool bEnableRun = false;
	bool bEnablePause = false;
	bool bEnableStop = false;
	bool bProcessingStarted = (getFPM()->ProcessingStarted == VARIANT_TRUE);
	bool bProcessingPaused = false;
	bool bContextSelected = (m_ipFAMTagManager->ActiveContext.length() > 0);
	m_bFPSLocked = m_bRequireAdminEdit && asCppBool(getDBPointer()->IsConnected) && !asCppBool(getDBPointer()->LoggedInAsAdmin);

	// update the menu items
	CMenu* pMenu = GetMenu();

	pMenu->CheckMenuItem(ID_FILE_REQUIREADMINEDIT, MF_BYCOMMAND | (m_bRequireAdminEdit) ? MF_CHECKED : MF_UNCHECKED);
	pMenu->EnableMenuItem(ID_FILE_LOGINASADMIN, MF_BYCOMMAND | (m_bFPSLocked) ? MF_ENABLED : MF_GRAYED);
		 
	// A local flag indicate FAM is processing or running only with statistics tab
	bool bRunningStatus = bProcessingStarted || (m_bStatsOnlyRunning && m_bRunning);

	// if bRunningStatus is true - set buttons in running or paused state
	if (bRunningStatus)
	{
		// set flag to indicate if stats processing are paused
		bProcessingPaused = m_bPaused || (getFPM()->ProcessingPaused == VARIANT_TRUE);

		if (bProcessingPaused)
		{
			bEnablePause = true;
		}
		else
		{
			bEnablePause = true;
			bEnableStop = true;
		}
	}
	else
	{
		if (m_toolBar)
		{
			string strCurrWorkflow = getFPM()->ActiveWorkflow;
			CString zStatusText = getFAMStatus();
			if (zStatusText == "Ready" && (!m_bWorkflowsDefined || !strCurrWorkflow.empty()))
			{
				bEnableRun = true;
			}
			m_buttonContext.EnableWindow(bContextSelected);
		}
	}

	// update the toolbar buttons
	m_toolBar.GetToolBarCtrl().EnableButton(ID_BTN_FAM_OPEN, asMFCBool(!bRunningStatus));
	m_toolBar.GetToolBarCtrl().EnableButton(ID_BTN_FAM_SAVE, asMFCBool(!bRunningStatus && !m_bFPSLocked));
	m_toolBar.GetToolBarCtrl().EnableButton(IDC_BTN_RUN, asMFCBool(bEnableRun) );
	m_toolBar.GetToolBarCtrl().EnableButton(IDC_BTN_PAUSE, asMFCBool(bEnablePause) );
	m_toolBar.GetToolBarCtrl().EnableButton(IDC_BTN_STOP, asMFCBool(bEnableStop) );
	m_toolBar.GetToolBarCtrl().PressButton(IDC_BTN_PAUSE, asMFCBool(bProcessingPaused) );

	// update workflow combo
	m_comboBoxWorkflow.EnableWindow(asMFCBool(!bRunningStatus && !m_bFPSLocked && m_bWorkflowsDefined));
	m_buttonContext.EnableWindow(asMFCBool(!bRunningStatus && !m_bFPSLocked && bContextSelected));

	// Get the auto scrolling setting from registry
	bool bAutoScroll = ma_pCfgMgr->getAutoScrolling();
	// Set the auto scrolling status to the buttons and log pages
	m_toolBar.GetToolBarCtrl().CheckButton(IDC_BTN_AUTO_SCROLL, asMFCBool(bAutoScroll) );
	m_propProcessingPage.setAutoScroll(bAutoScroll);
	m_propQueueLogPage.setAutoScroll(bAutoScroll);
	m_toolBar.Invalidate();

	// change the menu item label to either "Pause" or "Resume" processing depending upon
	// the current situation
	pMenu->CheckMenuItem(ID_PROCESS_PAUSEPROCESSING, MF_BYCOMMAND | (bProcessingPaused ? MF_CHECKED : MF_UNCHECKED) );
	
	// enable/disable menu items as appropriate.
	pMenu->EnableMenuItem(0, MF_BYPOSITION | (bRunningStatus ? MF_GRAYED : MF_ENABLED) );
	pMenu->EnableMenuItem(ID_FILE_SAVE, MF_BYCOMMAND | (!m_bFPSLocked) ? MF_ENABLED : MF_GRAYED);
	pMenu->EnableMenuItem(ID_FILE_SAVEAS, MF_BYCOMMAND | (!m_bFPSLocked) ? MF_ENABLED : MF_GRAYED);
	pMenu->EnableMenuItem(ID_PROCESS_STARTPROCESSING, MF_BYCOMMAND | (bEnableRun ? MF_ENABLED: MF_GRAYED) );
	pMenu->EnableMenuItem(ID_PROCESS_PAUSEPROCESSING, MF_BYCOMMAND | (bEnablePause ? MF_ENABLED: MF_GRAYED) );
	pMenu->EnableMenuItem(ID_PROCESS_STOPPROCESSING, MF_BYCOMMAND | (bEnableStop ? MF_ENABLED: MF_GRAYED) );
	pMenu->EnableMenuItem(2, MF_BYPOSITION | (bRunningStatus ? MF_GRAYED : MF_ENABLED) );
	pMenu->EnableMenuItem(ID_TOOLS_EDITCUSTOMTAGS, MF_BYCOMMAND | (bContextSelected && !m_bFPSLocked ? MF_ENABLED : MF_GRAYED) );

	// Enable/disable the controls on the database page based on whether
	// we are currently processing or not
	if (m_propDatabasePage.m_hWnd != __nullptr)
	{
		m_propDatabasePage.enableAllControls(!m_bRunning && !m_bFPSLocked);
	}

	setPagesEnable(!m_bRunning && !m_bFPSLocked);

	// redraw the menu bar
	DrawMenuBar();
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::createStatusBar()
{
	// Create the status bar
	CRect rectStatusBar;
	this->GetWindowRect(&rectStatusBar);
	rectStatusBar.top = rectStatusBar.bottom - gnSTATUS_BAR_HEIGHT;
	m_statusBar.Create(WS_CHILD | WS_BORDER | WS_VISIBLE, rectStatusBar, this, IDC_STATUSBAR);

	// initialize a widths array to default pane widths
	// and set the number of parts in the status bar
	int statusPaneWidths[gnNUM_STATUS_PANES];
	for (int i = 0; i < gnNUM_STATUS_PANES; i++)
	{
		statusPaneWidths[i] = gnDEFAULT_STATUS_PANE_WIDTH;
	}
	m_statusBar.SetParts(gnNUM_STATUS_PANES, statusPaneWidths);
	
	// initialize all the text fields to empty text
	m_statusBar.SetText("", gnSTATUS_TEXT_STATUS_PANE_ID, 0);
	m_statusBar.SetText("", gnTOTAL_COUNTS_STATUS_PANE_ID, 0);
	m_statusBar.SetText("", gnCOMPLETED_COUNTS_STATUS_PANE_ID, 0);
	m_statusBar.SetText("", gnPROCESSING_COUNTS_STATUS_PANE_ID, 0);
	m_statusBar.SetText("", gnPENDING_COUNTS_STATUS_PANE_ID, 0);
	m_statusBar.SetText("", gnFAILED_COUNTS_STATUS_PANE_ID, 0);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::createPropertyPages()
{
	// Create the Database page others will be created as they are required
	m_propSheet.AddPage(&m_propDatabasePage);

	m_propSheet.Create(this, WS_CHILD | WS_VISIBLE, 0);
	m_propSheet.ModifyStyleEx(0, WS_EX_CONTROLPARENT);

	CRect rectPropSheet;
	m_propSheet.GetWindowRect(&rectPropSheet);
	ScreenToClient(&rectPropSheet);

	// Set minimum size (this is the minimum size for the processing tab
	// to fit all of the settings on the screen)
	m_sizeMinimumPropPage.SetSize(660, 350);
	
	// Set the FPM pointer for the prop pages
	m_propActionPage.setFPMgr(m_pFileProcMgr);
	m_propProcessSetupPage.setFPMgr(m_pFileProcMgr);
	m_propQueueSetupPage.setFPMgr(m_pFileProcMgr);
	m_propStatisticsPage.setFPM( m_pFileProcMgr );

	// Set the Database pointer to the action tab
	m_propActionPage.setFPMDB(getDBPointer());

	// Set the config manager pointer for the pages that need it
	m_propStatisticsPage.setConfigMgr(ma_pCfgMgr.get());
	m_propProcessingPage.setConfigMgr(ma_pCfgMgr.get());
	m_propQueueLogPage.setConfigMgr(ma_pCfgMgr.get());
	
	// set active page back to the Database tab
	m_propSheet.SetActivePage(&m_propDatabasePage);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::createToolBar()
{
	if (m_toolBar.CreateEx(this, TBSTYLE_FLAT, WS_CHILD | WS_VISIBLE | CBRS_TOP
								| CBRS_TOOLTIPS | CBRS_FLYBY | CBRS_SIZE_DYNAMIC) )
	{
		m_toolBar.LoadToolBar(IDR_TOOLBAR_PROCESS_FILE);
	}

	m_toolBar.SetBarStyle(m_toolBar.GetBarStyle() |
		CBRS_TOOLTIPS | CBRS_FLYBY | CBRS_SIZE_DYNAMIC);

	// must set TBSTYLE_TOOLTIPS here in order to get tool tips
	m_toolBar.ModifyStyle(0, TBSTYLE_TOOLTIPS);

	// Set the button style for the auto-scroll button
	int nIndex = m_toolBar.CommandToIndex(IDC_BTN_AUTO_SCROLL);
	if (nIndex != -1)
	{
		m_toolBar.SetButtonStyle(nIndex, TBBS_CHECKBOX);
	}

	// We need to resize the dialog to make room for control bars.
	// First, figure out how big the control bars are.
	CRect rcClientStart;
	CRect rcClientNow;
	GetClientRect(rcClientStart);
	RepositionBars(AFX_IDW_CONTROLBAR_FIRST, AFX_IDW_CONTROLBAR_LAST,
				   0, reposQuery, rcClientNow);

	// Now move all the controls so they are in the same relative
	// position within the remaining client area as they would be
	// with no control bars.
	CPoint ptOffset(rcClientNow.left - rcClientStart.left,
					rcClientNow.top - rcClientStart.top);

	CRect  rcChild;
	CWnd* pwndChild = GetWindow(GW_CHILD);
	while (pwndChild)
	{
		pwndChild->GetWindowRect(rcChild);
		ScreenToClient(rcChild);
		rcChild.OffsetRect(ptOffset);
		pwndChild->MoveWindow(rcChild, FALSE);
		pwndChild = pwndChild->GetNextWindow();
	}

	// Adjust the dialog window dimensions
	CRect rcWindow;
	GetWindowRect(rcWindow);
	rcWindow.right += rcClientStart.Width() - rcClientNow.Width();
	rcWindow.bottom += rcClientStart.Height() - rcClientNow.Height();
	MoveWindow(rcWindow, FALSE);

	// And position the control bars
	RepositionBars(AFX_IDW_CONTROLBAR_FIRST, AFX_IDW_CONTROLBAR_LAST, 0);

	// add a drop down arrow next to the Open button
	m_toolBar.GetToolBarCtrl().SetExtendedStyle(TBSTYLE_EX_DRAWDDARROWS);
	DWORD dwStyle = m_toolBar.GetButtonStyle(m_toolBar.CommandToIndex(ID_BTN_FAM_OPEN));
	dwStyle |= TBBS_DROPDOWN; 
	m_toolBar.SetButtonStyle(m_toolBar.CommandToIndex(ID_BTN_FAM_OPEN), dwStyle);

	UpdateWindow();
	Invalidate();
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::doResize()
{
	// if it's minimized, do nothing
	if (IsIconic())
	{
		return;
	}

	// Ensure that the dlg's controls are realized before moving them.
	if (m_bInitialized)		
	{
		// Get total dialog size
		CRect rectDlg;
		GetClientRect(&rectDlg);
		int i = GetSystemMetrics(SM_CYCAPTION);
		int k = GetSystemMetrics(SM_CXSIZEFRAME);

		// get the size of the toolbar control
		CRect rectToolBar;
		m_toolBar.GetWindowRect(&rectToolBar);
		ScreenToClient(&rectToolBar);

		// Resize the property sheet
		CRect rectPropSheet;
		rectPropSheet.left = 5;

		// leave space below toolbar for lines from OnPaint()
		rectPropSheet.top = rectToolBar.bottom + 2;
		rectPropSheet.right = rectDlg.right - 5;
		rectPropSheet.bottom = rectDlg.bottom - 8 - gnSTATUS_BAR_HEIGHT - 24;
		m_propSheet.resize(rectPropSheet);
		m_nCurrentBottomOfPropPage = rectPropSheet.bottom;

		CRect rectStatusBar;
		this->GetWindowRect(&rectStatusBar);
		rectStatusBar.top = rectStatusBar.bottom - gnSTATUS_BAR_HEIGHT;
		GetDlgItem(IDC_STATUSBAR)->MoveWindow(&rectStatusBar);
		
		int statusPaneWidths[gnNUM_STATUS_PANES];
		statusPaneWidths[gnDB_CONNECTION_STATUS_PANE_ID] = rectStatusBar.Width() - gnSTATUSBAR_RIGHTHAND_PADDING;
		statusPaneWidths[gnFAILED_COUNTS_STATUS_PANE_ID] = statusPaneWidths[gnDB_CONNECTION_STATUS_PANE_ID] - gnDB_CONNECTION_STATUS_PANE_WIDTH;
		statusPaneWidths[gnSKIPPED_COUNTS_STATUS_PANE_ID] = statusPaneWidths[gnFAILED_COUNTS_STATUS_PANE_ID] - gnFAILED_COUNTS_STATUS_PANE_WIDTH;
		statusPaneWidths[gnPENDING_COUNTS_STATUS_PANE_ID] = statusPaneWidths[gnSKIPPED_COUNTS_STATUS_PANE_ID] - gnSKIPPED_COUNTS_STATUS_PANE_WIDTH;
		statusPaneWidths[gnPROCESSING_COUNTS_STATUS_PANE_ID] = statusPaneWidths[gnPENDING_COUNTS_STATUS_PANE_ID] - gnPENDING_COUNTS_STATUS_PANE_WIDTH;
		statusPaneWidths[gnCOMPLETED_COUNTS_STATUS_PANE_ID] = statusPaneWidths[gnPROCESSING_COUNTS_STATUS_PANE_ID] - gnPROCESSING_COUNTS_STATUS_PANE_WIDTH;
		statusPaneWidths[gnTOTAL_COUNTS_STATUS_PANE_ID] = statusPaneWidths[gnCOMPLETED_COUNTS_STATUS_PANE_ID] - gnCOMPLETED_COUNTS_STATUS_PANE_WIDTH;
		statusPaneWidths[gnSTATUS_TEXT_STATUS_PANE_ID] = statusPaneWidths[gnTOTAL_COUNTS_STATUS_PANE_ID] - gnTOTAL_COUNTS_STATUS_PANE_WIDTH;
		m_statusBar.SetParts(gnNUM_STATUS_PANES, statusPaneWidths);

		// Position the toolbar in the dialog
		RepositionBars(AFX_IDW_CONTROLBAR_FIRST, AFX_IDW_CONTROLBAR_LAST, 0);

		positionWorkflowContextControls();
	}

	Invalidate();
	UpdateWindow();
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::updateStats(UCLID_FILEPROCESSINGLib::IActionStatisticsPtr ipActionStats)
{
	if(ipActionStats == __nullptr)
	{
		m_nNumTotalDocs = 0;
		m_nNumCompletedProcessing = 0;
		m_nNumFailed = 0;
		m_nNumPending = 0;
		m_nNumSkipped = 0;
		m_nNumCurrentlyProcessing = 0;
	}
	else
	{
		ipActionStats->GetAllStatistics(&m_nNumTotalDocs, &m_nNumPending, 
			&m_nNumCompletedProcessing, &m_nNumFailed, &m_nNumSkipped,
			NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

		m_nNumCurrentlyProcessing = m_nNumTotalDocs -
			(m_nNumPending + m_nNumCompletedProcessing + m_nNumFailed + m_nNumSkipped);

		if (m_nNumInitialCompleted == gnUNINITIALIZED || m_nNumInitialFailed == gnUNINITIALIZED)
		{
			m_nNumInitialCompleted = m_nNumCompletedProcessing;
			m_nNumInitialFailed = m_nNumFailed;
		}

		// There is a slight lag on the initial polling while the DB is updating an existing action. 
		// This can result in -1 for number pending.
		if( m_nNumPending < 0 )
		{
			// prevent negative number of documents
			m_nNumPending = 0;
		}
	}

	updateStatusBar(false);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::updateUI()
{
	// update the connect to last database button [p13 #4855]
	m_propDatabasePage.updateLastUsedDBButton();

	updateDBConnectionStatus();
	
	updateMenuAndToolbar();

	updateStatusBar(true);

	CString szTitle = "File Action Manager";
	string strActiveContext = asString(m_ipFAMTagManager->ActiveContext);
	string strFile = getFileNameFromFullPath(m_strCurrFPSFilename);
	bool bFileSaved = !strFile.empty();
	if (!bFileSaved)
	{
		strFile = "[Unsaved file]";
	}

	vector<string> strTitleComponents;
	if (m_bDBConnectionReady || !strActiveContext.empty())
	{
		strTitleComponents.push_back(strFile);
	}
	if (m_bDBConnectionReady)
	{
		strTitleComponents.push_back(
			Util::Format("%s on %s",
				asString(getDBPointer()->DatabaseName).c_str(),
				asString(getDBPointer()->DatabaseServer).c_str()));
	}
	if (!strActiveContext.empty())
	{
		strTitleComponents.push_back(strActiveContext);
	}
	strTitleComponents.push_back("File Action Manager");

	szTitle = asString(strTitleComponents, false, " - ").c_str();

	if (!m_strProcessingStateString.empty())
	{
		string strTitle = szTitle;
		szTitle.Format("%s (%s)", strTitle.c_str(), m_strProcessingStateString.c_str());
	}
	SetWindowText(szTitle);

	// Update context related controls on the database tab.
	m_propDatabasePage.setCurrentContextState(bFileSaved, isValidContext(), strActiveContext);

	m_buttonContext.SetWindowText(strActiveContext.c_str());
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::updateStatusBar(bool bForceUpdate)
{
	CString zOldStatusText, zOldCompletedProcessing, zOldCurrentlyProcessing, zOldFailed, zOldPending, zOldSkipped, zOldTotal;
	CString zNewStatusText, zNewCompletedProcessing, zNewCurrentlyProcessing, zNewFailed, zNewPending, zNewSkipped, zNewTotal;

	if (m_bRunning)
	{
		if (m_strProcessingStateString.empty())
		{
			if (isPageDisplayed(kProcessingLogPage))
			{
				long nNumCurrentlyProcessing = m_propProcessingPage.getCurrentlyProcessingCount();
				if (nNumCurrentlyProcessing <= 0)
				{
					zNewStatusText = "Waiting";
				}
				else
				{
					zNewStatusText = "Processing";
				}
			}
			else if (isPageDisplayed(kQueueLogPage))
			{
				zNewStatusText = "Queueing";
			}
		}
		else
		{
			zNewStatusText = m_strProcessingStateString.c_str();
		}
	}
	else
	{
		zNewStatusText = getFAMStatus();
	}

	// Display comma-separated numbers in status bar only if an Action has been selected (P13 #4355)
	if (m_bDBConnectionReady && m_nCurrActionID != -1)
	{
		zNewCompletedProcessing.Format("%s %s", gstrCOMPLETED_STATUS_PANE_LABEL.c_str(),
			commaFormatNumber((LONGLONG)m_nNumCompletedProcessing).c_str());
		zNewCurrentlyProcessing.Format("%s %s", gstrPROCESSING_STATUS_PANE_LABEL.c_str(),
			commaFormatNumber((LONGLONG)m_nNumCurrentlyProcessing).c_str());
		zNewFailed.Format("%s %s", gstrFAILED_STATUS_PANE_LABEL.c_str(),
			commaFormatNumber((LONGLONG)m_nNumFailed).c_str());
		zNewSkipped.Format("%s %s", gstrSKIPPED_STATUS_PANE_LABEL.c_str(),
			commaFormatNumber((LONGLONG)m_nNumSkipped).c_str());
		zNewPending.Format("%s %s", gstrPENDING_STATUS_PANE_LABEL.c_str(),
			commaFormatNumber((LONGLONG)m_nNumPending).c_str());
		zNewTotal.Format("%s %s", gstrTOTAL_STATUS_PANE_LABEL.c_str(),
			commaFormatNumber((LONGLONG)m_nNumTotalDocs).c_str());
	}
	else
	{
		// Just display the labels in the cells
		zNewCompletedProcessing.Format("%s", gstrCOMPLETED_STATUS_PANE_LABEL.c_str());
		zNewCurrentlyProcessing.Format("%s", gstrPROCESSING_STATUS_PANE_LABEL.c_str());
		zNewFailed.Format("%s", gstrFAILED_STATUS_PANE_LABEL.c_str());
		zNewPending.Format("%s", gstrPENDING_STATUS_PANE_LABEL.c_str());
		zNewSkipped.Format("%s", gstrSKIPPED_STATUS_PANE_LABEL.c_str());
		zNewTotal.Format("%s", gstrTOTAL_STATUS_PANE_LABEL.c_str());
	}

	if (!bForceUpdate)
	{
		zOldStatusText = m_statusBar.GetText(gnSTATUS_TEXT_STATUS_PANE_ID);
		zOldCompletedProcessing = m_statusBar.GetText(gnCOMPLETED_COUNTS_STATUS_PANE_ID);
		zOldCurrentlyProcessing = m_statusBar.GetText(gnPROCESSING_COUNTS_STATUS_PANE_ID);
		zOldPending = m_statusBar.GetText(gnPENDING_COUNTS_STATUS_PANE_ID);
		zOldSkipped = m_statusBar.GetText(gnSKIPPED_COUNTS_STATUS_PANE_ID);
		zOldFailed = m_statusBar.GetText(gnFAILED_COUNTS_STATUS_PANE_ID);
		zOldTotal = m_statusBar.GetText(gnTOTAL_COUNTS_STATUS_PANE_ID);
	}
	
	if (zOldStatusText != zNewStatusText)
	{
		m_statusBar.SetText(zNewStatusText, gnSTATUS_TEXT_STATUS_PANE_ID, 0);
	}
	if (zOldCompletedProcessing != zNewCompletedProcessing)
	{
		m_statusBar.SetText(zNewCompletedProcessing, gnCOMPLETED_COUNTS_STATUS_PANE_ID, 0);
	}
	if (zOldCurrentlyProcessing != zNewCurrentlyProcessing)
	{
		m_statusBar.SetText(zNewCurrentlyProcessing, gnPROCESSING_COUNTS_STATUS_PANE_ID, 0);
	}
	if (zOldPending != zNewPending)
	{
		m_statusBar.SetText(zNewPending, gnPENDING_COUNTS_STATUS_PANE_ID, 0);
	}
	if (zOldSkipped != zNewSkipped)
	{
		m_statusBar.SetText(zNewSkipped, gnSKIPPED_COUNTS_STATUS_PANE_ID, 0);
	}
	if (zOldFailed != zNewFailed)
	{
		m_statusBar.SetText(zNewFailed, gnFAILED_COUNTS_STATUS_PANE_ID, 0);
	}
	if (zOldTotal != zNewTotal)
	{
		m_statusBar.SetText(zNewTotal, gnTOTAL_COUNTS_STATUS_PANE_ID, 0);
	}
}
//-------------------------------------------------------------------------------------------------
bool FileProcessingDlg::isFAMReady()
{
	try
	{
		// Validate if the FAM is ready
		validateFAMStatus();
		return true;
	}
	// Exception indicates FAM is not ready
	catch(...)
	{
		// Fam is not ready, return false
		return false;
	}
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::openFile(string strFileName, bool bPreserveConnection) 
{
	try
	{
		// If necessary prompt the user to save changes
		// they may also cancel the open
		if (!checkForSave())
		{
			return;
		}

		if (strFileName == "")
		{
			const static string strFilter = "File Processing Specifications (*.fps)|*.fps|"
											"All Files (*.*)|*.*||";
			// ask user to select file to load
			CFileDialog fileDlg(TRUE, gstrFILE_PROCESSING_SPECIFICATION_EXT.c_str(), NULL, 
				OFN_ENABLESIZING | OFN_EXPLORER | OFN_FILEMUSTEXIST | OFN_HIDEREADONLY | OFN_PATHMUSTEXIST,
				strFilter.c_str(), this);
			
			// Pass the pointer of dialog to create ThreadDataStruct object
			ThreadFileDlg tfd(&fileDlg);

			// If Cancel is clicked
			if (tfd.doModal() != IDOK)
			{
				return;
			}

			strFileName = (LPCTSTR) fileDlg.GetPathName();
		}

		CWaitCursor wait;
		
		// Clear the current context
		clearContext();

		// make sure the file exists
		::validateFileOrFolderExistence(strFileName);

		// verify extension is FPS
		string strExt = getExtensionFromFullPath( strFileName, true );
		if ( strExt != gstrFILE_PROCESSING_SPECIFICATION_EXT)
		{
			throw UCLIDException("ELI10964", "File is not an FPS file.");
		}
		
		checkAndUpdateContextTagsDatabaseIfNeeded(strFileName);

		// If bPreserveConnection, create a duplicate copy to use to restore key connection state
		// variables back to the newly loaded connection.
		UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipSavedConnection(__nullptr);
		if (bPreserveConnection && m_bDBConnectionReady)
		{
			ipSavedConnection.CreateInstance(CLSID_FileProcessingDB);
			ipSavedConnection->DuplicateConnection(getDBPointer());
		}
		
		getFPM()->LoadFrom(get_bstr_t(strFileName), VARIANT_FALSE);

		// https://extract.atlassian.net/browse/ISSUE-14793
		// If the target DB has changed due to being saved into a different context or we didn't
		// have a good connection in the first place, don't preserve current login.
		// NOTE: Compare connections strings not DB server/name as they may resolve to
		// different DBs based on context.
		if (bPreserveConnection && ipSavedConnection != __nullptr &&
			ipSavedConnection->ConnectionString == getDBPointer()->ConnectionString)
		{
			{
				getDBPointer()->DuplicateConnection(ipSavedConnection);
			}
		}

		loadSettingsFromManager();

		updateUI();

		// Delete recovery file - user has opened a new file
		if ( m_pFRM != __nullptr )
		{
			m_pFRM->deleteRecoveryFile();
		}

		// add the file to MRU list
		addFileToMRUList(strFileName);
	}
	catch (...)
	{
		removeFileFromMRUList(strFileName);
		throw;
	}
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::checkAndUpdateContextTagsDatabaseIfNeeded(std::string &strFileName)
{
	// Before loading if there is a ContextTags database defined make sure it is the correct version
	IContextTagProviderPtr ipContextTags;
	ipContextTags.CreateInstance("Extract.Utilities.ContextTags.ContextTagProvider");
	ASSERT_RESOURCE_ALLOCATION("ELI43335", ipContextTags != __nullptr);
	string strContextPath = getDirectoryFromFullPath(strFileName);
	string strCustomTagsDBFile = strContextPath + "\\" + gstrCUSTOMTAGS_DB_FILE;
	if (isFileOrFolderValid(strCustomTagsDBFile) && ipContextTags->IsUpdateRequired(strContextPath.c_str()))
	{
		if (MessageBox("The context tags database needs to be updated to a newer version. Update?",
			"Context tags database update", MB_YESNO | MB_ICONQUESTION) == IDYES)
		{
			ipContextTags->UpdateContextTagsDB(strContextPath.c_str());
		}
		else
		{
			UCLIDException ue("ELI43336", "Context tags database needs to be updated.");
			ue.addDebugInfo("ContextPath", strContextPath, false);
			throw ue;
		}
	}

	ipContextTags->CloseDatabase();
}
//-------------------------------------------------------------------------------------------------
bool FileProcessingDlg::saveFile(std::string strFileName, bool bShowConfigurationWarnings)
{
	if (bShowConfigurationWarnings)
	{
		string strWarning = asString(getFPM()->GetConfigurationWarnings());

		if (!strWarning.empty())
		{
			strWarning += "\r\n\r\nContinue with save?";

			int nResponse = MessageBox(strWarning.c_str(), "Configuration warning",
				MB_YESNO| MB_ICONEXCLAMATION);
			if (nResponse != IDYES)
			{
				return false;
			}
		}
	}

	if (strFileName == "")
	{
		// Get the default file name based on action 
		strFileName = (m_strCurrFPSFilename.empty()) ? getDefaultFileName(): m_strCurrFPSFilename;

		// Use the currently selected context as the default save location.
		if (m_ipFAMTagManager->FPSFileDir.length() > 0)
		{
			strFileName = removeLastSlashFromPath(asString(m_ipFAMTagManager->FPSFileDir))
				+ "\\" + strFileName;
		}

		const static string strFilter = "File Processing Specifications (*.fps)|*.fps|"
											"All Files (*.*)|*.*||";
		
		// ask user to select file to save
		CFileDialog fileDlg(FALSE, gstrFILE_PROCESSING_SPECIFICATION_EXT.c_str(), strFileName.c_str(), 
			OFN_ENABLESIZING | OFN_EXPLORER | OFN_NOREADONLYRETURN | OFN_PATHMUSTEXIST,
			strFilter.c_str(), this);
		
		// Pass the pointer of dialog to create ThreadDataStruct object
		ThreadFileDlg tfd(&fileDlg);

		if (tfd.doModal() != IDOK)
		{
			return false;
		}

		strFileName = (LPCTSTR) fileDlg.GetPathName();

		// if the file already exists ask the user if they want to overwrite it
		if (isFileOrFolderValid(strFileName))
		{
			int nRet = MessageBox( "The specified file already exists.\nDo you want to overwrite the saved file?", 
				"Confirm Overwrite", MB_YESNO );
			if (nRet == IDNO)
			{
				return false;
			}
		}

		if (m_ipFAMTagManager->ActiveContext.length() > 0)
		{
			string strOldFPSFileDir = asString(m_ipFAMTagManager->FPSFileDir).c_str();
			string strNewFPSFileDir = getDirectoryFromFullPath(strFileName).c_str();
			if (_stricmp(strOldFPSFileDir.c_str(), strNewFPSFileDir.c_str()) != 0)
			{
				int nRet = MessageBox("The active context will not apply in this FPS file location.\r\n"
					"Proper context tag values will need to be ensured.\r\n\r\n"
					"Are you sure you want to save to this location?", 
					"Changing Context", MB_YESNO );
				if (nRet == IDNO)
				{
					return false;
				}
			}
		}
	}

	if (!flushSettingsToManager())
	{
		return false;
	}

	checkAndUpdateContextTagsDatabaseIfNeeded(strFileName);

	getFPM()->SaveTo(get_bstr_t(strFileName), VARIANT_TRUE);

	// Delete recovery file - user has saved the file
	if ( m_pFRM != __nullptr )
	{
		m_pFRM->deleteRecoveryFile();
	}

	// If the file name changed, update MRU list
	if (!stringCSIS::sEqual(strFileName, m_strCurrFPSFilename))
	{
		addFileToMRUList(strFileName);
	}
	setCurrFPSFile(strFileName);

	// Reopen the file - this will reset the context so that tags will be interpreted correctly
	openFile(m_strCurrFPSFilename, true);
	
	// The open file call above sometimes triggers the side-effect of a task description
	// "disappearing" when on the processing setup tag. Invalidate to ensure the current tab is
	// refreshed appropriately.
	Invalidate(TRUE);

	return true;
}
//-------------------------------------------------------------------------------------------------
bool FileProcessingDlg::checkForSave()
{
	// get access to the IPersistStream interface on the FPM
	IPersistStreamPtr ipStream = getFPM();
	ASSERT_RESOURCE_ALLOCATION("ELI14154", ipStream != __nullptr);

	// Check if there is any change in the FPM
	if (!m_bFPSLocked && ipStream->IsDirty() == S_OK)
	{
		// Provide MessageBox to user
		int nRet = MessageBox("Current settings have been modified.\nDo you wish to save the changes?", 
			"Save Changes?", MB_YESNOCANCEL );
		if (nRet == IDYES)
		{
			// Save should be allowed even when settings are not runnable  
			// https://extract.atlassian.net/browse/ISSUE-12798

			// Check for read-only FPS file (P13 #4181)
			if (isFileReadOnly( m_strCurrFPSFilename ))
			{
				// Display a SaveAs dialog
				return saveFile("", true);
			}

			// if the file fails to save we treat it as if the user wishes to cancel
			if (!saveFile(m_strCurrFPSFilename, true))
			{
				return false;
			}
		}
		else if(nRet == IDCANCEL)
		{
			return false;
		}
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr FileProcessingDlg::getDBPointer()
{
	ASSERT_RESOURCE_ALLOCATION("ELI15055", m_pFPMDB != __nullptr );
	return UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr( m_pFPMDB );
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileProcessingManagerPtr FileProcessingDlg::getFPM()
{
	ASSERT_RESOURCE_ALLOCATION("ELI15056", m_pFileProcMgr != __nullptr );
	return UCLID_FILEPROCESSINGLib::IFileProcessingManagerPtr( m_pFileProcMgr );
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::setCurrFPSFile(const string& strFileName)
{
	m_strCurrFPSFilename = strFileName;

	updateUI();
}
//---------------------------------------------------------------------------------------------
void FileProcessingDlg::updateDBConnectionStatus()
{
	m_nCurrActionID = -1;
	m_bDBConnectionReady = false;
	m_strCurrDBStatus = gstrNOT_CONNECTED;

	m_strCurrDBStatus = asString(getDBPointer()->GetCurrentConnectionStatus());
	m_bDBConnectionReady = (m_strCurrDBStatus == gstrCONNECTION_ESTABLISHED);

	if (m_bDBConnectionReady)
	{
		try
		{
			m_nCurrActionID = getDBPointer()->GetActionID(getFPM()->GetExpandedActionName());
		}
		catch (_com_error&) {} 
		catch (UCLIDException&) {}
	}
	loadWorkflowComboBox();
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::loadSettingsFromManager()
{
	m_bUpdatingConnection = true;

	try
	{
		m_bRequireAdminEdit = asCppBool(getFPM()->RequireAdminEdit);

		// get the .FPS file first so that it will still be set even
		// if an exception is thrown below here [p13 #4580]
		// Get the .FPS filename and set it to the current FPS file
		string strFPSFile = asString( getFPM()->FPSFileName );
		setCurrFPSFile(strFPSFile);
		if (!strFPSFile.empty())
		{
			// [LRCAU #6008] - Files opened on command line should be added the MRU list
			addFileToMRUList(strFPSFile);
		}

		// set the database file for the database page
		if (isPageDisplayed(kDatabasePage))
		{
			m_propDatabasePage.setConnectionInfo(asString(getFPM()->DatabaseServer), 
											asString(getFPM()->DatabaseName),
											asString(getFPM()->AdvancedConnectionStringProperties));
		}

		// If the action page is displayed refresh the data 
		if (isPageDisplayed(kActionPage))
		{
			// Warn the user if the action name no longer exists [LRCAU #4825]
			m_propActionPage.refresh(true);
		}

		// Set the File processors & number of threads
		if (isPageDisplayed(kProcessingSetupPage))
		{
			m_propProcessSetupPage.refresh();
		}

		// Set the File Suppliers
		if (isPageDisplayed(kQueueSetupPage))
		{
			m_propQueueSetupPage.refresh();
		}

		m_bUpdatingConnection = false;
	}
	catch (...)
	{
		// If the database settings are invalid that exception would be caught here and
		// the UI needs to be updated based on the current DB status
		// https://extract.atlassian.net/browse/ISSUE-13100
		try
		{
			updateUI();
		}
		CATCH_AND_LOG_ALL_EXCEPTIONS("ELI38315")
		m_bUpdatingConnection = false;
		throw;
	}
}
//-------------------------------------------------------------------------------------------------
bool FileProcessingDlg::flushSettingsToManager()
{
	getFPM()->RestrictNumStoredRecords = VARIANT_TRUE;
	getFPM()->MaxStoredRecords = m_dlgOptions.getMaxDisplayRecords();
	getFPM()->RequireAdminEdit = asVariantBool(m_bRequireAdminEdit);

	return true;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::saveWindowSettings()
{
	//////////////////
	// Remember window positions
	//////////////////
	WINDOWPLACEMENT windowPlacement;
	windowPlacement.length = sizeof( WINDOWPLACEMENT );
	if (GetWindowPlacement(&windowPlacement) != 0)
	{
		// Check for window in maximized state
		if (windowPlacement.showCmd == SW_SHOWMAXIMIZED)
		{
			ma_pCfgMgr->setWindowMaximized( true );
		}
		// Window not maximized, store normal position info
		else
		{
			RECT	rect;
			rect = windowPlacement.rcNormalPosition;

			ma_pCfgMgr->setWindowPos(rect.left, rect.top);
			ma_pCfgMgr->setWindowSize(rect.right - rect.left, rect.bottom - rect.top);

			ma_pCfgMgr->setWindowMaximized( false );
		}
	}	
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::startRunTimer()
{
	// If its not already running, reset it and start it
	if(! m_stopWatch.isRunning() )
	{
		m_stopWatch.reset();
		m_stopWatch.start();
	}
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::stopRunTimer()
{
	// Stop the stopwatch, but do not reset it. 
	// Used in the case of the pause button being clicked.
	m_stopWatch.stop();
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::resetRunTimer()
{
	// Reset the stopwatch
	m_stopWatch.reset();
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::validateFAMStatus()
{
	// Call ValidateStatus(), if there is any exception,
	// rethrow it
	try
	{
		getFPM()->ValidateStatus();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI14364");
}
//-------------------------------------------------------------------------------------------------
CString FileProcessingDlg::getFAMStatus()
{
	// String for the status bar, which will display
	// an exception text when got one.
	CString zStatusText = "Ready";

	// If FPM status is invalid, disable save menu and run button
	try
	{
		validateFAMStatus();
	}
	catch (UCLIDException& ue)
	{
		// Set the exception text
		zStatusText = ue.getTopText().c_str();
	}

	return zStatusText;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::setupAndLaunchStatsThread()
{
	try
	{
		// Make sure there is no running stats thread
		stopStatsThread();

		// Reset the thread events
		m_eventStatsThreadStarted.reset();
		m_eventStatsThreadStop.reset();
		m_eventStatsThreadExited.reset();

		// start the thread
		AfxBeginThread(FileProcessingDlg::StatisticsMgrThreadFunct, this);
		
		// Wait for the StatsThread started event
		m_eventStatsThreadStarted.wait();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS( "ELI14500" )
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::stopStatsThread()
{
	// If the statistics page is displayed, clear it before the final update is made to the page.
	// This will force stats to be recomputed on the final update and ensure that estimated time
	// remaining statistics don't linger after processing is complete. [P13:4628]
	if (isPageDisplayed(kStatisticsPage))
	{
		m_propStatisticsPage.clear();
	}

	// Stop the statistics thread
	if ( m_eventStatsThreadStarted.isSignaled())
	{
		// Signal thread to stop
		m_eventStatsThreadStop.signal();

		// Wait for the thread to exit
		m_eventStatsThreadExited.wait();
	}

	// Reset the started event
	m_eventStatsThreadStarted.reset();
}
//-------------------------------------------------------------------------------------------------
UINT __cdecl FileProcessingDlg::StatisticsMgrThreadFunct( LPVOID pParam )
{

	INIT_EXCEPTION_AND_TRACING("MLI00020");

	CoInitializeEx(NULL, COINIT_MULTITHREADED);

	FileProcessingDlg *pFPDlg = NULL;

	try
	{
		ASSERT_ARGUMENT("ELI15045", pParam != __nullptr );
	
		// Set the StatisticsMgr *
		pFPDlg = (static_cast<FileProcessingDlg *> (pParam));
		ASSERT_RESOURCE_ALLOCATION("ELI25246", pFPDlg != __nullptr);

		_lastCodePos = "10";

		// Signal that the thread has started
		pFPDlg->m_eventStatsThreadStarted.signal();
		
		_lastCodePos = "20";

		FileProcessingConfigMgr* pCfgMgr = pFPDlg->ma_pCfgMgr.get();
		ASSERT_RESOURCE_ALLOCATION("ELI25247", pCfgMgr != __nullptr);
		unsigned int uiTickSpeed = pCfgMgr->getTimerTickSpeed();

		_lastCodePos = "30";

		// Get the DB pointer
		UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFPMDB = pFPDlg->getDBPointer();
		ASSERT_RESOURCE_ALLOCATION("ELI14509", ipFPMDB != __nullptr);

		_lastCodePos = "40";

		// Get action must be done in the thread so that the UI doesn't get blocked if DB is locked
		UCLID_FILEPROCESSINGLib::IFileProcessingManagerPtr ipFPMgr = pFPDlg->getFPM();
		ASSERT_RESOURCE_ALLOCATION("ELI15638", ipFPMgr != __nullptr);

		_lastCodePos = "50";

		// Get the current workflow and action name
		string currentWorkflow = ipFPMDB->ActiveWorkflow;
		string currentActionName = ipFPMgr->GetExpandedActionName();

		_lastCodePos = "55";

		// Get the action ID
		long nActionID = -1;
		try
		{
			nActionID = ipFPMDB->GetActionID(currentActionName.c_str());
		}
		catch (...)
		{
			// If the action name doesn't exist the main thread will handle it
		}

		UCLID_FILEPROCESSINGLib::IActionStatisticsPtr ipNewActionStats;

		if (nActionID != -1)
		{
			_lastCodePos = "60";

			// Loop with call to check the stats - while the stop event is not signaled
			do
			{
				// Display any exceptions that occur, but allow the thread to
				// keep attempting to update the statistics
				try
				{
					if (currentWorkflow.empty())
					{
						_lastCodePos = "62";
						ipNewActionStats = ipFPMDB->GetStatsAllWorkflows(currentActionName.c_str(), VARIANT_FALSE);
						ASSERT_RESOURCE_ALLOCATION("ELI43480", ipNewActionStats != __nullptr);
					}
					else
					{
						_lastCodePos = "66";
						// Get the stats from the db in a temp object so that the UI will not be blocked
						ipNewActionStats = ipFPMDB->GetStats(nActionID, VARIANT_FALSE, VARIANT_FALSE);
						ASSERT_RESOURCE_ALLOCATION("ELI20383", ipNewActionStats != __nullptr);
					}
				
					_lastCodePos = "70";

					pFPDlg->PostMessageA(FP_STATISTICS_UPDATE, 
						(WPARAM) ipNewActionStats.Detach(), (LPARAM)0 );

					_lastCodePos = "80";
				}
				// [LegacyRCAndUtils:6311]
				// Per discussion with Arvind, since stats are not a critical part of the FAM
				// functionality, just log.
				CATCH_AND_LOG_ALL_EXCEPTIONS("ELI29166");
			}
			while ( pFPDlg->m_eventStatsThreadStop.wait( uiTickSpeed ) == WAIT_TIMEOUT );

			// Call one last time to get the final stats.  This is so that if processing or supplying of all of the files
			// finishes the stats that are displayed will be current at the time of the thread stop event being signaled.
			// If this is the only instance and it is processing files the final stats will show that the processing is 
			// finished instead of showing the stats as of the last timeout
			if (currentWorkflow.empty())
			{
				_lastCodePos = "90";
				ipNewActionStats = ipFPMDB->GetStatsAllWorkflows(currentActionName.c_str(), VARIANT_TRUE);
				ASSERT_RESOURCE_ALLOCATION("ELI43481", ipNewActionStats != __nullptr);
			}
			else
			{
				_lastCodePos = "100";
				// Get the stats from the db in a temp object so that the UI will not be blocked
				ipNewActionStats = ipFPMDB->GetStats(nActionID, VARIANT_TRUE, VARIANT_FALSE);
				ASSERT_RESOURCE_ALLOCATION("ELI43482", ipNewActionStats != __nullptr);
			}
			
			_lastCodePos = "110";
			
			pFPDlg->PostMessageA(FP_STATISTICS_UPDATE, (WPARAM) ipNewActionStats.Detach(), (LPARAM)0 );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15001");

	// Signal that the thread has exited
	if ( pFPDlg != __nullptr )
	{
		pFPDlg->m_eventStatsThreadExited.signal();
	}

	_lastCodePos = "100";

	CoUninitialize();
	
	return 0;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::updateUIForCurrentDBStatus()
{
	updateDBConnectionStatus();

	// Set the database status on the database page
	m_propDatabasePage.setDBConnectionStatus(m_strCurrDBStatus);

	// Initialize page set
	set<EDlgTabPage> setPages;
	
	// Always display the database page
	setPages.insert(kDatabasePage);

	// If the new database file connects to the database successfully the action tab should be displayed
	if (m_bDBConnectionReady)
	{
		string strCurrWorkflow = getFPM()->ActiveWorkflow;
		if (!m_bWorkflowsDefined || !strCurrWorkflow.empty())
		{
			if (isPageDisplayed(kActionPage))
			{
				// https://extract.atlassian.net/browse/ISSUE-12936
				// If the action page was already displayed, refresh it rather than resetting the tabs
				// entirely (which would cause the current tab to be reset to the action tab rather than
				// the tab it is currently on).
				m_propActionPage.refresh();
			}
			else
			{
				setPages.insert(kActionPage);
				updateTabs(setPages);
			}
		}
		else
		{
			updateTabs(setPages);
		}

		// If there is an action set, show the stats for the current action.
		try
		{
			UCLID_FILEPROCESSINGLib::IActionStatisticsPtr ipActionStats;
			string currentWorkflow = asString( getDBPointer()->ActiveWorkflow);
			if (m_nCurrActionID != -1)
			{
				if (currentWorkflow.empty())
				{
					string currentActionName = getDBPointer()->GetActionName(m_nCurrActionID);
					ipActionStats = getDBPointer()->GetStatsAllWorkflows(currentActionName.c_str(), VARIANT_FALSE);
					ASSERT_RESOURCE_ALLOCATION("ELI43480", ipActionStats != __nullptr);
				}
				else
				{
					ipActionStats =
						getDBPointer()->GetStats(m_nCurrActionID, VARIANT_FALSE, VARIANT_FALSE);
					ASSERT_RESOURCE_ALLOCATION("ELI38476", ipActionStats != __nullptr);
				}
				updateStats(ipActionStats);
			}
		}
		// If the action name doesn't exist don't show an error; simply don't update the counts
		catch (_com_error&)
		{
			updateStats(__nullptr);
		}
		catch (UCLIDException&)
		{
			updateStats(__nullptr);
		}
	}
	else
	{
		// Hide the Action page
		updateTabs(setPages);

		updateStats(__nullptr);
	}

	updateUI();
}
//-------------------------------------------------------------------------------------------------
CPropertyPage * FileProcessingDlg::getPropertyPage(EDlgTabPage ePage)
{
	switch(ePage)
	{
	case kDatabasePage:
		return &m_propDatabasePage;
	case kActionPage:
		return &m_propActionPage;
	case kQueueSetupPage:
		return &m_propQueueSetupPage;
	case kQueueLogPage:
		return &m_propQueueLogPage;
	case kProcessingSetupPage:
		return &m_propProcessSetupPage;
	case kProcessingLogPage:
		return &m_propProcessingPage;
	case kStatisticsPage:
		return &m_propStatisticsPage;
	default:
		THROW_LOGIC_ERROR_EXCEPTION("ELI17058");
		break;
	}
	THROW_LOGIC_ERROR_EXCEPTION("ELI16872");
}
//-------------------------------------------------------------------------------------------------
bool FileProcessingDlg::isPageDisplayed(EDlgTabPage ePage)
{
	long lPageIndex = m_propSheet.GetPageIndex(getPropertyPage(ePage));
	return lPageIndex >= 0;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::removePage(EDlgTabPage ePage)
{
	// if the page is not displayed just return
	if (!isPageDisplayed(ePage))
	{
		return;
	}

	// Remove the page
	m_propSheet.RemovePage(getPropertyPage(ePage));

	// Call ResetInitialized for all pages the need it called
	switch(ePage)
	{
	case kDatabasePage:
		break;
	case kActionPage:
		m_propActionPage.ResetInitialized();
		break;
	case kQueueSetupPage:
		m_propQueueSetupPage.ResetInitialized();
		break;
	case kQueueLogPage:
		m_propQueueLogPage.ResetInitialized();
		break;
	case kProcessingSetupPage:
		m_propProcessSetupPage.ResetInitialized();
		break;
	case kProcessingLogPage:
		m_propProcessingPage.ResetInitialized();
		break;
	case kStatisticsPage:
		m_propStatisticsPage.ResetInitialized();
		break;
	default:
		THROW_LOGIC_ERROR_EXCEPTION("ELI17059");
		break;
	}
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::displayPage(EDlgTabPage ePage)
{
	if (isPageDisplayed(ePage))
	{
		// page is already displayed
		return;
	}

	// Add the page
	CPropertyPage *pPage = getPropertyPage(ePage);
	m_propSheet.AddPage(pPage);

	// Set the page to active so that it gets created.
	m_propSheet.SetActivePage(pPage);

	// Call refresh method for pages that have that defined
	switch(ePage)
	{
	case kDatabasePage:
		break;
	case kActionPage:
		m_propActionPage.refresh();
		break;
	case kQueueSetupPage:
		m_propQueueSetupPage.refresh();
		break;
	case kQueueLogPage:
		break;
	case kProcessingSetupPage:
		m_propProcessSetupPage.refresh();
		break;
	case kProcessingLogPage:
		m_propProcessingPage.refresh();
		break;
	case kStatisticsPage:
		break;
	default:
		THROW_LOGIC_ERROR_EXCEPTION("ELI17057");
		break;
	}
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::updateUIForProcessingComplete()
{
	try
	{
		// Stop the statistics thread
		stopStatsThread();

		// Enable the process setup page after processing
		if (isPageDisplayed(kProcessingSetupPage))
		{
			m_propProcessSetupPage.setEnabled(true);
		}

		// Enable the queue setup page after processing
		if (isPageDisplayed(kQueueSetupPage))
		{
			m_propQueueSetupPage.setEnabled(true);
		}

		// Enable the action page after processing
		if (isPageDisplayed(kActionPage))
		{
			m_propActionPage.setEnabled(true);
		}

		// If the processing log page is shown, notify it to stop
		// progress updates
		if (isPageDisplayed(kProcessingLogPage))
		{
			m_propProcessingPage.stopProgressUpdates();
		}

		// This takes care of stopping the timer on the next timer tick. 
		m_bRunning = false;

		// enable the "process" menu
		CMenu* pMenu = GetMenu();
		pMenu->EnableMenuItem(1, MF_BYPOSITION | MF_ENABLED);

		updateUI();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29347");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::addFileToMRUList(const string& strFileToAdd)
{
	// Update the list, add the new item, write the list back out
	m_upMRUList->readFromPersistentStore();
	m_upMRUList->addItem(strFileToAdd);
	m_upMRUList->writeToPersistentStore();	
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::removeFileFromMRUList(const string& strFileToRemove)
{
	// Update the list, remove the item, write the list back out
	m_upMRUList->readFromPersistentStore();
	m_upMRUList->removeItem(strFileToRemove);
	m_upMRUList->writeToPersistentStore();	
}
//-------------------------------------------------------------------------------------------------
string FileProcessingDlg::getDefaultFileName()
{
	// Make sure the action patch has been initialized
	if (m_propActionPage.m_hWnd == __nullptr)
	{
		return "";
	}
	
	// Get the configured action name
	string strActionName = m_propActionPage.GetCurrentActionName();
	if (strActionName.empty())
	{
		return "";
	}

	// Get the state of the check boxes on the Action page
	bool bQueuing = m_propActionPage.m_btnQueue.GetCheck() == BST_CHECKED;
	bool bProcess = m_propActionPage.m_btnProcess.GetCheck() == BST_CHECKED;
	bool bStats = m_propActionPage.m_btnDisplay.GetCheck() == BST_CHECKED;

	// If nothing is checked there is no default;
	if (!bQueuing && !bProcess && !bStats)
	{
		return "";
	}

	// Process is checked and Queuing is not
	if (bProcess && !bQueuing)
	{
		return strActionName + ".fps";
	}

	// Both Process and Queuing are checked
	if (bProcess & bQueuing)
	{
		return strActionName + "_QueueAndProcess.fps";
	}

	return strActionName + ((bQueuing) ? "_Queue.fps" : "_Stats.fps" );
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::clearContext()
{
	m_ipFAMTagManager->FPSFileDir = "";
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::refreshContext()
{
	if (m_ipFAMTagManager->FPSFileDir.length() == 0)
	{
		m_ipFAMTagManager->FPSFileName = m_strCurrFPSFilename.c_str();
		m_ipFAMTagManager->FPSFileDir = getDirectoryFromFullPath(m_strCurrFPSFilename).c_str();
	}
	else
	{
		m_ipFAMTagManager->RefreshContextTags();
	}
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlg::editCustomTags()
{
	try
	{
		getTagUtility()->EditCustomTags((long)m_hWnd);

		m_bUpdatingConnection = true;
		m_propDatabasePage.refreshConnection();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI39297");

	m_bUpdatingConnection = false;
}
//-------------------------------------------------------------------------------------------------
bool FileProcessingDlg::displayRecentContextSelection()
{
	m_upContextMRUList->readFromPersistentStore();
	vector<string> vecContexts;

	long nSize = m_upContextMRUList->getCurrentListSize();
	IContextTagProviderPtr ipContextTags("Extract.Utilities.ContextTags.ContextTagProvider");
	ASSERT_RESOURCE_ALLOCATION("ELI39300", ipContextTags != nullptr);

	map<string, string> mapContextOptions;

	for(long i = 0; i < nSize; i++)
	{
		string strPath;
		try
		{
			try
			{
				strPath = m_upContextMRUList->at(i);
				ipContextTags->ContextPath = strPath.c_str();
				string strContext = asString(ipContextTags->ActiveContext);
				if (isValidContext(strContext))
				{
					string strContextOption = Util::Format("%s (%s)",
						strContext.c_str(), strPath.c_str());
					mapContextOptions[strContextOption] = strPath;
					vecContexts.push_back(strContextOption);
				}
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI43271")
		}
		catch (UCLIDException &ue)
		{
			UCLIDException newUE("ELI43272", "Unable to load context for display.", ue);
			newUE.addDebugInfo("ContextPath", strPath, false);
			newUE.addPossibleResolution("This may have been logged when attempting to load an older version of the ContextTags.sdf file.");
			newUE.log();
		}
	}

	ipContextTags->CloseDatabase();

	CDialogSelect dlgSelect("Select the context to use:", "Select Context", vecContexts,
		nSize > 0 ? vecContexts.front() : "");

	if (dlgSelect.DoModal() == IDOK)
	{
		string strLastFPSFileDir = asString(m_ipFAMTagManager->FPSFileDir);

		string strSelectedValue = (LPCTSTR)dlgSelect.m_zComboValue;
		auto selectedOption = mapContextOptions.find(strSelectedValue);
		m_ipFAMTagManager->FPSFileDir = (selectedOption == mapContextOptions.end())
			? strSelectedValue.c_str()
			: selectedOption->second.c_str();
		// Prompt not only for a directory without a ContextTags.sdf file, but also for where an
		// existing ContextTags.sdf does not have a context for FPSFileDir.
		if (!isValidContext())
		{
			CString zWarning;
			zWarning.Format("A context does not yet exist for the path:\r\n"
				"\"%s\"\r\n\r\n"
				"Do you wish to create one now?", dlgSelect.m_zComboValue);

			int nResponse = MessageBox(zWarning, "Create context?",
				MB_YESNO| MB_ICONEXCLAMATION);
			if (nResponse != IDYES)
			{
				// If user decided no to create new context, switch back to the old one.
				m_ipFAMTagManager->FPSFileDir = strLastFPSFileDir.c_str();
				return false;
			}
		}

		return true;
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
bool FileProcessingDlg::areDatabaseTagsDefined()
{
	IVariantVectorPtr ipContextTags = getTagUtility()->GetCustomFileTags();
	ASSERT_RESOURCE_ALLOCATION("ELI39298", ipContextTags != nullptr);

	long nSize = ipContextTags->Size;
	int nCountDBTags = 0;		
	for (long i = 0; i < nSize; i++)
	{
		string strTag = asString(ipContextTags->Item[i].bstrVal);
		if (strTag == gstrDATABASE_SERVER_TAG ||
			strTag == gstrDATABASE_NAME_TAG)
		{
			nCountDBTags++;
		}
	}

	return (nCountDBTags == 2);
}
//-------------------------------------------------------------------------------------------------
bool FileProcessingDlg::isValidContext(string strContext/* = ""*/)
{
	if (strContext.empty())
	{
		strContext = asString(m_ipFAMTagManager->ActiveContext);
	}

	return (!strContext.empty() && strContext != "No context defined!");
}
//-------------------------------------------------------------------------------------------------
ITagUtilityPtr FileProcessingDlg::getTagUtility()
{
	ITagUtilityPtr ipTagUtility = m_ipFAMTagManager;
	ASSERT_RESOURCE_ALLOCATION("ELI39274", ipTagUtility != nullptr);

	return ipTagUtility;
}
//--------------------------------------------------------------------------------------------------
void FileProcessingDlg::loadWorkflowComboBox()
{
	m_comboBoxWorkflow.ResetContent();
	
	int index = -1;
	string strCurrWorkflow = asString(getFPM()->ActiveWorkflow);

	// If the database connection is not in a good state there is nothing to do
	if (!m_bDBConnectionReady || m_pFPMDB == __nullptr)
	{
		m_comboBoxWorkflow.SetCurSel(index);
		m_bWorkflowsDefined = false;
		m_propDatabasePage.showWorkflowWarning((index == -1) && m_bWorkflowsDefined);
		return;
	}

	IStrToStrMapPtr ipWorkflows = getDBPointer()->GetWorkflows();
	ASSERT_RESOURCE_ALLOCATION("ELI42072", ipWorkflows != __nullptr);

	IIUnknownVectorPtr ipItemPairs = ipWorkflows->GetAllKeyValuePairs();

	int numItems = ipItemPairs->Size();

	// Only add the all workflows option if there are workflows defined
	if (numItems == 0)
	{
		m_bWorkflowsDefined = false;
	}
	else
	{
		m_bWorkflowsDefined = true;

		// get the file supplying mgmt role
		UCLID_FILEPROCESSINGLib::IFileActionMgmtRolePtr ipMgmtRole = getFPM()->FileSupplyingMgmtRole;
		ASSERT_RESOURCE_ALLOCATION("ELI42120", ipMgmtRole != __nullptr);

		// Do not add the all workflow option if the file supplying role
		// is enabled
		if (!asCppBool(ipMgmtRole->Enabled))
		{
			// Add the all workflows option
			m_comboBoxWorkflow.InsertString(0, gstrALL_WORKFLOWS.c_str());
			m_comboBoxWorkflow.SetItemData(0, -1);
		}

		for (int i = 0; i < numItems; i++)
		{
			IStringPairPtr ipCurrentActionPair = (IStringPairPtr)ipItemPairs->At(i);
			index = m_comboBoxWorkflow.AddString(ipCurrentActionPair->StringKey);
			long lValue = asLong(ipCurrentActionPair->StringValue);
			m_comboBoxWorkflow.SetItemData(index, lValue);
		}
	}
	if (strCurrWorkflow.empty())
	{
		index = -1;
	}
	else
	{
		index = m_comboBoxWorkflow.FindStringExact(0, strCurrWorkflow.c_str());
		if (index < 0)
		{
			string strMsg = Util::Format("The workflow '%s' does not exist in the database.", strCurrWorkflow.c_str());

			MessageBox(strMsg.c_str(), "Workflow does not exist.", MB_OK || MB_ICONEXCLAMATION);

			getFPM()->ActiveWorkflow = "";
		}
	}
	m_propDatabasePage.showWorkflowWarning((index == -1) && m_bWorkflowsDefined);
	m_comboBoxWorkflow.SetCurSel(index);
}
//--------------------------------------------------------------------------------------------------
void FileProcessingDlg::positionWorkflowContextControls()
{
	CRect rectDlg, labelWorkflowRect, workflowRect, labelContextRect, contextButtonRect;
	GetClientRect(rectDlg);

	// Get the rects for the workflow and context controls
	m_staticWorkflowLabel.GetWindowRect(labelWorkflowRect);
	ScreenToClient(labelWorkflowRect);
	m_staticContextLabel.GetWindowRect(labelContextRect);
	ScreenToClient(labelContextRect);
	m_comboBoxWorkflow.GetWindowRect(workflowRect);
	ScreenToClient(workflowRect);
	m_buttonContext.GetWindowRect(contextButtonRect);
	ScreenToClient(contextButtonRect);

	// Position the workflow label
	labelWorkflowRect.MoveToY(rectDlg.bottom - labelWorkflowRect.Height() - gnSTATUS_BAR_HEIGHT - 15);
	m_staticWorkflowLabel.MoveWindow(labelWorkflowRect);

	// Position the context buttons
	contextButtonRect.MoveToXY(rectDlg.right - 6 - contextButtonRect.Width(),
		rectDlg.bottom - contextButtonRect.Height() - gnSTATUS_BAR_HEIGHT - 10);
	m_buttonContext.MoveWindow(contextButtonRect);

	// Position the context label
	labelContextRect.top = labelWorkflowRect.top;
	labelContextRect.bottom = labelWorkflowRect.bottom;
	labelContextRect.MoveToX(contextButtonRect.left - 1 - labelContextRect.Width());
	m_staticContextLabel.MoveWindow(labelContextRect);

	// position the workflow combo
	workflowRect.left = labelWorkflowRect.right + 6;
	workflowRect.right = labelContextRect.left - 6;
	workflowRect.MoveToY(rectDlg.bottom - workflowRect.Height() - gnSTATUS_BAR_HEIGHT - 10);
	m_comboBoxWorkflow.MoveWindow(workflowRect);
}
//--------------------------------------------------------------------------------------------------
void FileProcessingDlg::setPagesEnable(bool bEnable)
{
	// Disable the Process setup page while running
	if (isPageDisplayed(kProcessingSetupPage))
	{
		m_propProcessSetupPage.setEnabled(bEnable);
	}

	// Disable the Queue setup page
	if (isPageDisplayed(kQueueSetupPage))
	{
		m_propQueueSetupPage.setEnabled(bEnable);
	}

	// Disable the action page
	if (isPageDisplayed(kActionPage))
	{
		m_propActionPage.setEnabled(bEnable);
	}
}