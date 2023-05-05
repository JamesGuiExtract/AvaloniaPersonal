// FAMDBAdminDlg.cpp : implementation file
//

#include "stdafx.h"
#include "FAMDBAdmin.h"
#include "FAMDBAdminDlg.h"
#include "FAMDBAdminAboutDlg.h"
#include "FileProcessingUtils.h"
#include "ClearWarningDlg.h"
#include "ExportFileListDlg.h"
#include "ManageUserCountersDlg.h"
#include "ManageTagsDlg.h"
#include "SetActionStatusDlg.h"
#include "SetFilePriorityDlg.h"
#include "..\..\..\code\FPCategories.h"
#include "..\..\..\..\InputFunnel\IFCore\Code\IFCategories.h"
#include "ManageUsersDlg.h"
#include "ManageMetadataFieldsDlg.h"
#include "ManageAttributeSets.h"
#include "MoveToWorkflowForm.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <ComUtils.h>
#include <RegistryPersistenceMgr.h>
#include <FAMUtilsConstants.h>
#include <RegConstants.h>
#include <ClipboardManager.h>

#include <set>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

using namespace Extract::FAMDBAdmin;
using namespace Extract::ETL::Management;
using namespace System::Collections::Generic;
using namespace Extract::FileActionManager::Forms;
using namespace Extract::Utilities;
using namespace Extract::Dashboard::Forms;
using namespace System::Threading;
using namespace Extract::Web::ApiConfiguration;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const int giMIN_WINDOW_WIDTH = 725;
const int giMIN_WINDOW_HEIGHT = 340;
const string gstrFILE_ACTION_MANAGER_FILENAME = "ProcessFiles.exe";
const string gstrREPORT_VIEWER_EXE = "ReportViewer.exe";
const string gstrTITLE = "File Action Manager Database Administration";
const string gstrFAMDB_REG_KEY = gstrCOM_COMPONENTS_REG_PATH + "\\UCLIDFileProcessing\\FAMDBAdmin";

// Guid for the COM class that displays the DB info configuration dialog
const string gstrDB_OPTIONS_GUID = "{F86BB12C-EB1C-44EA-B5EA-9A428A601608}";
const string gstrDB_SECURE_COUNTER_MANAGER_GUID = "{D22193BD-EC71-4494-B185-F673EEA548D2}";

//-------------------------------------------------------------------------------------------------
// CFAMDBAdminDlg dialog
//-------------------------------------------------------------------------------------------------
CFAMDBAdminDlg::CFAMDBAdminDlg(IFileProcessingDBPtr ipFAMDB,CWnd* pParent /*=NULL*/)
: CDialog(CFAMDBAdminDlg::IDD, pParent),
m_windowMgr(this, gstrFAMDB_REG_KEY),
m_ipFAMDB(ipFAMDB),
m_ipFAMFileInspector(CLSID_FAMFileInspectorComLibrary),
m_bIsDBGood(false),
m_ipMiscUtils(NULL),
m_ipCategoryManager(NULL),
m_ipSchemaUpdateProgressStatus(NULL),
m_ipSchemaUpdateProgressStatusDialog(NULL),
m_bSchemaUpdateSucceeded(false),
m_bDBSchemaIsNotCurrent(false),
m_bUnaffiliatedFiles(false),
m_bInitialized(false)
{
	try
	{
		ASSERT_ARGUMENT("ELI17610", m_ipFAMDB != __nullptr);
		ASSERT_ARGUMENT("ELI35802", m_ipFAMFileInspector != __nullptr);

		m_hIcon = AfxGetApp()->LoadIcon(IDI_ICON_FAMDBADMIN);

		ma_pCfgMgr = unique_ptr<FileProcessingConfigMgr>(new FileProcessingConfigMgr());

		// Set the Database Page notify to this object ( so the status can be updated )
		m_propDatabasePage.setNotifyDBConfigChanged(this);

		// set the database and FAMFileInspector for the summary page
		m_propSummaryPage.setFAMDatabase(m_ipFAMDB);
		m_propSummaryPage.setFAMFileInspector(m_ipFAMFileInspector);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI17609");
}
//-------------------------------------------------------------------------------------------------
CFAMDBAdminDlg::~CFAMDBAdminDlg()
{
	try
	{
		m_ipFAMDB = (IFileProcessingDBPtr)__nullptr;
		m_ipMiscUtils = (IMiscUtilsPtr)__nullptr;
		m_ipCategoryManager = (ICategoryManagerPtr)__nullptr;
		m_ipFAMFileInspector = (IFAMFileInspectorPtr)__nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI18124")
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_WORKFLOW_COMBO, m_comboBoxWorkflow);
	DDX_Control(pDX, IDC_STATIC_WORKFLOW, m_staticWorkflowLabel);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CFAMDBAdminDlg, CDialog)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_COMMAND(ID_HELP_ABOUTTHISAPPLICATION, &CFAMDBAdminDlg::OnHelpAbout)
	ON_COMMAND(ID_TOOLS_EXPORTFILELISTS, &CFAMDBAdminDlg::OnExportFileLists)
	ON_COMMAND(ID_TOOLS_INSPECT_FILES, &CFAMDBAdminDlg::OnInspectFiles)
	ON_COMMAND(ID_TOOLS_FILEACTIONMANAGER, &CFAMDBAdminDlg::OnToolsFileActionManager)
	ON_COMMAND(ID_DATABASE_EXIT, &CFAMDBAdminDlg::OnExit)
	ON_COMMAND(ID_DATABASE_CLEAR, &CFAMDBAdminDlg::OnDatabaseClear)
	ON_COMMAND(ID_DATABASE_IMPORT, &CFAMDBAdminDlg::OnDatabaseImport)
	ON_COMMAND(ID_DATABASE_EXPORT, &CFAMDBAdminDlg::OnDatabaseExport)
	ON_COMMAND(ID_DATABASE_RESETLOCK, &CFAMDBAdminDlg::OnDatabaseResetLock)
	ON_COMMAND(ID_DATABASE_UPDATE_SCHEMA, &CFAMDBAdminDlg::OnDatabaseUpdateSchema)
	ON_COMMAND(ID_DATABASE_SET_OPTIONS, &CFAMDBAdminDlg::OnDatabaseSetOptions)
	ON_COMMAND(ID_DATABASE_CHANGEPASSWORD, &CFAMDBAdminDlg::OnDatabaseChangePassword)
	ON_COMMAND(ID_DATABASE_LOGOUT, &CFAMDBAdminDlg::OnDatabaseLogout)
	ON_COMMAND(ID_TOOLS_MANUALLYSETACTIONSTATUS, &CFAMDBAdminDlg::OnActionManuallySetActionStatus)
	ON_COMMAND(ID_HELP_FILEACTIONMANAGERHELP, &CFAMDBAdminDlg::OnHelpFileActionManagerHelp)
	ON_WM_SIZE()
	ON_WM_GETMINMAXINFO()
	ON_COMMAND(ID_TOOLS_CHECKFORNEWCOMPONENTS, &CFAMDBAdminDlg::OnToolsCheckForNewComponents)
	ON_COMMAND(ID_MANAGE_TAGS, &CFAMDBAdminDlg::OnManageTags)
	ON_COMMAND(ID_MANAGE_BATES_COUNTERS, &CFAMDBAdminDlg::OnManageBatesCounters)
	ON_COMMAND(ID_MANAGE_USERS, &CFAMDBAdminDlg::OnManageLoginUsers)
	ON_COMMAND(ID_MANAGE_ACTIONS, &CFAMDBAdminDlg::OnManageWorkflowActions)
	ON_COMMAND(ID_TOOLS_SETPRIORITY, &CFAMDBAdminDlg::OnToolsSetPriority)
	ON_COMMAND(ID_TOOLS_REPORTS, &CFAMDBAdminDlg::OnToolsReports)
	ON_COMMAND(ID_TOOLS_RECALCULATE_STATS, &CFAMDBAdminDlg::OnRecalculateStats)
	ON_COMMAND(ID_MANAGE_METADATA, &CFAMDBAdminDlg::OnManageMetadataFields)
	ON_COMMAND(ID_MANAGE_ATTRIBUTESETS, &CFAMDBAdminDlg::OnManageAttributeSets)
	ON_COMMAND(ID_MANAGE_RULE_COUNTERS, &CFAMDBAdminDlg::OnManageRuleCounters)
	ON_COMMAND(ID_MANAGE_DATABASESERVICES, &CFAMDBAdminDlg::OnManageDatabaseServices)
	ON_COMMAND(ID_MANAGE_DASHBOARDS, &CFAMDBAdminDlg::OnManageDashboards)
	//}}AFX_MSG_MAP
	ON_CBN_SELCHANGE(IDC_WORKFLOW_COMBO, &CFAMDBAdminDlg::OnCbnSelchangeWorkflowCombo)
	ON_COMMAND(ID_TOOLS_MOVE_FILES_TO_WORKFLOW, &CFAMDBAdminDlg::OnToolsMoveFilesToWorkflow)
	ON_COMMAND(ID_MANAGE_MLMODELS, &CFAMDBAdminDlg::OnManageMLModels)
	ON_COMMAND(ID_TOOLS_DASHBOARDS, &CFAMDBAdminDlg::OnToolsDashboards)
	ON_COMMAND(ID_MANAGE_WEBCONFIGS, &CFAMDBAdminDlg::OnManageWebAPIConfigs)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CFAMDBAdminDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CFAMDBAdminDlg::OnInitDialog()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		CDialog::OnInitDialog();

		// Set the icon for this dialog.  The framework does this automatically
		// when the application's main window is not a dialog
		SetIcon(m_hIcon, TRUE);			// Set big icon
		SetIcon(m_hIcon, FALSE);		// Set small icon
		
		// Restore the dialog to the position it was in last time it was open.
		m_windowMgr.RestoreWindowPosition();

		positionWorkflowControls();

		// Load and update the menu
		loadMenu();

		// Enable the menus based on the new flag settings
		enableMenus();
		
		m_propSheet.AddPage(&m_propDatabasePage);
		m_propSheet.AddPage(&m_propSummaryPage);
		m_propSheet.Create(this, WS_CHILD | WS_VISIBLE, 0);
		m_propSheet.ModifyStyleEx(0, WS_EX_CONTROLPARENT);
		m_propSheet.SetActivePage(&m_propDatabasePage);
		
		m_bInitialized = true;
		
		setPropPageSize();

		// Set the BrowseEnabled flag to false so that the Server and Database are not selectable
		m_propDatabasePage.setBrowseEnabled(false);

		// Get the server and database from the FAMDB
		string strServer = asString(m_ipFAMDB->DatabaseServer);
		string strDatabase = asString(m_ipFAMDB->DatabaseName);
		string strAdvConnStrProperties = asString(m_ipFAMDB->AdvancedConnectionStringProperties);

		ProcessingContext context(strServer, strDatabase, "", 0);
		UCLIDException::SetCurrentProcessingContext(context);

		string strCaption = strDatabase + " on " + strServer + " - " + gstrTITLE;
		SetWindowText(strCaption.c_str());

		// Save the good settings to the registry
		ma_pCfgMgr->setLastGoodDBSettings(strServer, strDatabase, strAdvConnStrProperties);

		// Set the Server and DB names
		m_propDatabasePage.setConnectionInfo(strServer, strDatabase, strAdvConnStrProperties);

		// If the DB is not connected and valid, if it is because the schema is out of date, prompt
		// to upgrade now.
		if (m_bIsDBGood)
		{
			if (m_ipFAMDB->HasCounterCorruption == VARIANT_TRUE)
			{
				::MessageBox(NULL, "Corrupted rule execution counters detected. Please use \r\n"
					"\"Rule execution counters\" from the \"Manage\" menu to repair.",
					"Counter corruption", MB_ICONINFORMATION);
			}
		}
		else
		{
			string strCurDBStatus = asString(m_ipFAMDB->GetCurrentConnectionStatus());
			if (strCurDBStatus == gstrWRONG_SCHEMA)
			{
				int iResult = MessageBox("This database was created with a different version of "
					"the software.\r\n\r\nDo you wish to update the database to be compatible with "
					"the current software version?", "Update Database Schema?",
					MB_YESNO);
				if (iResult == IDYES)
				{
					OnDatabaseUpdateSchema();
				}
			}
			else if (strCurDBStatus == gstrUNAFFILIATED_FILES)
			{
				int iResult = MessageBox("This database has defined workflow(s) but files that exist "
					"outside those workflows.\r\n\r\nThese files need to be migrated to workflows before "
					"processing.\r\n\r\nDo you wish to migrate the files now?",
					"Migrate files to workflows?",
					MB_YESNO);
				if (iResult == IDYES)
				{
					showMoveToWorkflowDialog(true);
				}
			}
		}
			

		m_strCurrentWorkflow = gstrALL_WORKFLOWS;
		m_nCurrentWorkflowID = -1;
	}
	// This is needed because .net exception causes crash if not handled
	catch (Exception ^ex)
	{
		Extract::ExceptionExtensionMethods::ExtractDisplay(ex, "ELI45606");
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14870");

	return TRUE;  // return TRUE  unless you set the focus to a control
}

//-------------------------------------------------------------------------------------------------
// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.
void CFAMDBAdminDlg::OnPaint()
{
	if (IsIconic())
	{
		CPaintDC dc(this); // device context for painting

		SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

		// Center icon in client rectangle
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// Draw the icon
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CDialog::OnPaint();
	}
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnHelpAbout()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Display the About box with version information
		CFAMDBAdminAboutDlg dlgAbout;
		dlgAbout.DoModal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14884");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnExportFileLists()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	
	try
	{
		// Display the exporting file list dialog
		CExportFileListDlg dlgExportFiles(m_ipFAMDB);
		dlgExportFiles.DoModal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14874");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnInspectFiles()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	
	try
	{
		m_ipFAMFileInspector->OpenFAMFileInspector(m_ipFAMDB, __nullptr, false, "", __nullptr, 0, m_ipFAMDB->GetOneTimePassword());
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI35799");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnExit()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		OnCancel();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13986");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnCancel()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Save the dialog's current size/position to the registry.
		m_windowMgr.SaveWindowPosition();

		CDialog::OnCancel();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI31578");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnOK()
{
	// Override the OnOK() to ignore the message if you press "Enter" key [P13: 4135]
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnDatabaseClear()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Build prompt string
		string strPrompt = "Are you sure you want to delete all information in the database?\r\n\r\n"
			"If you click \"Yes\" below, you will be deleting all information in the database, "
			"including all filenames, file processing history, and file queueing history. "
			"Actions defined in the database will also be deleted, unless the checkbox below "
			"is checked.\r\n\r\n"
			"Click \"No\" below if you do not want to clear the database at this time.";
		ClearWarningDlg prompt(strPrompt, "Confirmation");
		if (prompt.DoModal() != IDYES)
		{
			return;
		}
		
		// Build second prompt string
		prompt.setCaption("Are you absolutely sure you want to delete all information in the database?\r\n\r\n"
			"Once the database has been cleared, the data in the database will be deleted and "
			"can no longer be retrieved, unless you have made a backup of the data outside "
			"the scope of Extract's applications.\r\n\r\n"
			"Click \"Yes\" below, if you want to clear the database and lose all information "
			"contained in it.\r\n\r\n"
			"Click \"No\" below if you do not want to clear the database at this time.");
		prompt.setTitle("Final Confirmation");
		if (prompt.DoModal() != IDYES)
		{
			return;
		}

		// Display wait cursor
		CWaitCursor wait;

		bool bRetainValues = prompt.getRetainActions();

		// Set the database is good flag to false
		m_bIsDBGood = false;
		m_bDBSchemaIsNotCurrent = false;
		m_bUnaffiliatedFiles = false;
		try
		{
			// Clear the database
			m_ipFAMDB->Clear(asVariantBool(bRetainValues));
		}
		catch (...)
		{
			// Determine if the reason the clear failed is because the schema is out-of-date.
			string strCurDBStatus = asString(m_ipFAMDB->GetCurrentConnectionStatus());
			if (strCurDBStatus == gstrWRONG_SCHEMA)
			{
				m_bDBSchemaIsNotCurrent = true;
			}
			else if (strCurDBStatus == gstrUNAFFILIATED_FILES)
			{
				m_bUnaffiliatedFiles = true;
			}
			else if (strCurDBStatus == gstrCONNECTION_ESTABLISHED)
			{
				m_bIsDBGood = true;
			}

			// Enable menus
			enableMenus();

			// Update the summary tab
			UpdateSummaryTab();
			throw;
		}

		// Set the database is good flag to true
		m_bIsDBGood = true;

		// Add application trace whenever a database modification is made
		// [LRCAU #5052 - JDS - 12/18/2008]
		UCLIDException uex("ELI23598", "Application trace: Database change");
		uex.addDebugInfo("Change", "Clear database");
		uex.addDebugInfo("User Name", getCurrentUserName());
		uex.addDebugInfo("Server Name", asString(m_ipFAMDB->DatabaseServer));
		uex.addDebugInfo("Database", asString(m_ipFAMDB->DatabaseName));
		uex.addDebugInfo("Settings", bRetainValues ? "Retained" : "Not retained");
		uex.log();

		// Set the database status
		setUIDatabaseStatus();

		MessageBox("Current database has been cleared.", "Success", MB_ICONINFORMATION);

		// Enable the menus based on the new flag settings
		enableMenus();

		// Update the summary tab
		UpdateSummaryTab();

		if (!bRetainValues)
		{
			loadWorkflowComboBox();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14861");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnDatabaseImport()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		runEXE("DatabaseMigrationWizard.exe",
			"/DatabaseServer " + asString(m_ipFAMDB->DatabaseServer)
			+ " /DatabaseName " + asString(m_ipFAMDB->DatabaseName)
			+ " /Password " + asString(m_ipFAMDB->GetOneTimePassword())
			+ " /Import");
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI49819");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnDatabaseExport()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		runEXE("DatabaseMigrationWizard.exe",
			+"/DatabaseServer " + asString(m_ipFAMDB->DatabaseServer)
			+ " /DatabaseName " + asString(m_ipFAMDB->DatabaseName)
			+ " /Password " + asString(m_ipFAMDB->GetOneTimePassword())
			+ " /Export");
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI49820");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnDatabaseResetLock()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		CWaitCursor wait;

		// Call ResetDBLock() to reset the database lock
		m_ipFAMDB->ResetDBLock();

		// Add application trace whenever a database modification is made
		// [LRCAU #5052 - JDS - 12/18/2008]
		UCLIDException uex("ELI23603", "Application trace: Database change");
		uex.addDebugInfo("Change", "Reset database lock");
		uex.addDebugInfo("User Name", getCurrentUserName());
		uex.addDebugInfo("Server Name", asString(m_ipFAMDB->DatabaseServer));
		uex.addDebugInfo("Database", asString(m_ipFAMDB->DatabaseName));
		uex.log();

		MessageBox("Current database lock has been reset.", "Success", MB_ICONINFORMATION);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14862");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnDatabaseUpdateSchema()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		try
		{
			// Until it succeeds, the update has failed.
			m_bSchemaUpdateSucceeded = false;

			// Initialize the progress status.
			m_ipSchemaUpdateProgressStatusDialog.CreateInstance(
				"Extract.Utilties.Forms.ProgressStatusDialog");
			ASSERT_RESOURCE_ALLOCATION("ELI31385", m_ipSchemaUpdateProgressStatusDialog != __nullptr);

			m_ipSchemaUpdateProgressStatus = CFileProcessingUtils::createMTAProgressStatus();
			ASSERT_RESOURCE_ALLOCATION("ELI31386", m_ipSchemaUpdateProgressStatus != __nullptr);

			m_ipSchemaUpdateProgressStatusDialog->Initialize(get_bstr_t("Schema Update"),
				m_ipSchemaUpdateProgressStatus, 2, 100, VARIANT_FALSE, NULL);

			// Start background thread to perform the upgrade
			AfxBeginThread(upgradeToCurrentSchemaThread, this);

			// Show a modal progress status dialog that cannot be closed by the user. The background
			// thread will close the dialog once the update has completed (whether successfully or not).
			m_ipSchemaUpdateProgressStatusDialog->ShowModalDialog(m_hWnd);

			if (m_bSchemaUpdateSucceeded)
			{
				refreshDBStatus();

				// Set the database status
				setUIDatabaseStatus();

				MessageBox("Database schema has been updated.", "Success", MB_ICONINFORMATION);

				// Update menu & summary info
				enableMenus();
				UpdateSummaryTab();

				if (m_ipFAMDB->HasCounterCorruption == VARIANT_TRUE)
				{
					MessageBox("Corrupted rule execution counters detected. Please use \r\n"
						"\"Rule execution counters\" from the \"Manage\" menu to repair.",
						"Counter corruption", MB_ICONINFORMATION);
				}
			}
			else
			{
				MessageBox("Failed to update database schema.", "Failure", MB_ICONINFORMATION);
			}
		}
		CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI31388");

		if (m_ipSchemaUpdateProgressStatusDialog != __nullptr)
		{
			m_ipSchemaUpdateProgressStatusDialog.Release();
			m_ipSchemaUpdateProgressStatusDialog = (IProgressStatusDialogPtr)__nullptr;
		}

		if (m_ipSchemaUpdateProgressStatus != __nullptr)
		{
			m_ipSchemaUpdateProgressStatus.Release();
			m_ipSchemaUpdateProgressStatus = (IProgressStatusPtr)__nullptr;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI31521");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnDatabaseSetOptions()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		IConfigureDBInfoSettingsPtr ipSettings(gstrDB_OPTIONS_GUID.c_str());
		ASSERT_RESOURCE_ALLOCATION("ELI31930", ipSettings != __nullptr);
		if (ipSettings->PromptForSettings(m_ipFAMDB) == VARIANT_TRUE)
		{
			UCLIDException uex("ELI32165", "Application trace: Database settings changed");
			uex.addDebugInfo("User Name", getCurrentUserName());
			uex.addDebugInfo("Server Name", asString(m_ipFAMDB->DatabaseServer));
			uex.addDebugInfo("Database", asString(m_ipFAMDB->DatabaseName));
			uex.log();

			// Reset the connection to update the cached settings in the FAMDB pointer
			m_ipFAMDB->ResetDBConnection(VARIANT_FALSE, VARIANT_FALSE);

			MessageBox("Database settings have been updated.", "Settings Updated");
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI31931");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnDatabaseChangePassword()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Display the Change Login dialog - passing in a 'cancelled' parameter
		VARIANT_BOOL vbCancelled = VARIANT_FALSE;
		if ( m_ipFAMDB->ChangeLogin(VARIANT_TRUE, &vbCancelled ) == VARIANT_TRUE )
		{
			MessageBox("Changed password successfully.", "Success", MB_OK | MB_ICONINFORMATION);

			// Add application trace whenever a database modification is made
			// [LRCAU #5052 - JDS - 12/18/2008]
			UCLIDException uex("ELI23599", "Application trace: Database change");
			uex.addDebugInfo("Change", "Change password");
			uex.addDebugInfo("User Name", getCurrentUserName());
			uex.addDebugInfo("Server Name", asString(m_ipFAMDB->DatabaseServer));
			uex.addDebugInfo("Database", asString(m_ipFAMDB->DatabaseName));
			uex.log();

		}
		else if (vbCancelled == VARIANT_FALSE)
		{
			MessageBox("Change of password failed.\r\n\r\nPlease ensure you are using the correct password and try again.", 
				"Change failed", MB_OK | MB_ICONERROR );
		}
		// else user did Cancel and no Change Failure message is needed
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15718");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnDatabaseLogout()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Exit dialog and return logout id
		EndDialog(IDOK);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14864");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnActionManuallySetActionStatus()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Check if there is no action inside the database;
		if ( notifyNoActions() )
		{
			return;
		}

		// Display the reset action dialog
		CSetActionStatusDlg dlgSetActionStatus(m_ipFAMDB, this);
		dlgSetActionStatus.DoModal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14868");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnHelpFileActionManagerHelp()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		string documentationLink = "https://extract.atlassian.net/wiki/spaces/KB/pages/164921381/DB+Administration";
	
		int returnValue = (int) ShellExecute(NULL, "open", documentationLink.c_str(), NULL, NULL, SW_NORMAL);
		

		// Per MDSN for ShellExecute: 0-32 represent error values
		if (returnValue >= 0 && returnValue <= 32)
		{
			UCLIDException ue("ELI53814", "Failed to open document!");
			ue.addDebugInfo("Filename", documentationLink);
			addShellOpenDocumentErrorInfo(ue, returnValue);
			throw ue;
		}

		

	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14869");
}
//-------------------------------------------------------------------------------------------------
// The system calls this function to obtain the cursor to display while the user drags
// the minimized window.
HCURSOR CFAMDBAdminDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}
//-------------------------------------------------------------------------------------------------
BOOL CFAMDBAdminDlg::PreTranslateMessage(MSG* pMsg) 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

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

			// Find and eat an Escape character (P13 #4325)
			if (pMsg->wParam == VK_ESCAPE)
			{
				return TRUE;
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15237")

	return CDialog::PreTranslateMessage(pMsg);
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnSize(UINT nType, int cx, int cy)
{
	try
	{
		CDialog::OnSize(nType, cx, cy);

		if (IsIconic())
		{
			return;
		}

		// Ensure that the dlg's controls are realized before moving them.
		if (m_bInitialized)		
		{
			setPropPageSize();
			positionWorkflowControls();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16638");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnGetMinMaxInfo(MINMAXINFO* lpMMI)
{
	try
	{
		// Set the min width and height
		lpMMI->ptMinTrackSize.x = giMIN_WINDOW_WIDTH;
		lpMMI->ptMinTrackSize.y = giMIN_WINDOW_HEIGHT;

		__super::OnGetMinMaxInfo(lpMMI);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17034");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnToolsFileActionManager()
{
	try
	{
		// Get the path of the FAMDBAdmin app
		string strEXEPath = getModuleDirectory(theApp.m_hInstance);

		// Set up the executable path for FAM
		strEXEPath += "\\";
		strEXEPath += gstrFILE_ACTION_MANAGER_FILENAME;

		// Set the parameters to load the server and database in the FAM
		// Add quotes around the database name [LegacyRC #5124]
		string strParameters = "/sd " + asString(m_ipFAMDB->DatabaseServer) + 
			" \"" + asString(m_ipFAMDB->DatabaseName) + "\"";

		string strAdvConnStrProperties = asString(m_ipFAMDB->AdvancedConnectionStringProperties);
		if (!strAdvConnStrProperties.empty())
		{
			strParameters += " /a \"" + strAdvConnStrProperties + "\"";
		}

		// Start the File Action Manager with the /sd switch
		runEXE(strEXEPath, strParameters);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17612");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnToolsReports()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Get the path of the FAMDBAdmin app
		string strEXEPath = getModuleDirectory(theApp.m_hInstance);

		// Set up the executable path for the ReportViewer application
		strEXEPath += "\\";
		strEXEPath += gstrREPORT_VIEWER_EXE;

		// Put quotes around server and database [LegacyRC #5091]
		string strParameters = "\"" + asString(m_ipFAMDB->DatabaseServer) + "\"" + " "
			+ "\"" + asString(m_ipFAMDB->DatabaseName) + "\""
			+ " \"" + asString(m_ipFAMDB->ActiveWorkflow) + "\"";

		// Start the ReportViewer application
		runEXE(strEXEPath, strParameters, 0, NULL, getDirectoryFromFullPath(strEXEPath));
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18033");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnToolsCheckForNewComponents()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// create a vector of all categories we care about.
		IVariantVectorPtr ipCategoryNames(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI18165", ipCategoryNames != __nullptr);

		ipCategoryNames->PushBack(get_bstr_t(FP_FILE_PROC_CATEGORYNAME.c_str()));
		ipCategoryNames->PushBack(get_bstr_t(FP_FILE_SUPP_CATEGORYNAME.c_str()));
		ipCategoryNames->PushBack(get_bstr_t(FP_FAM_CONDITIONS_CATEGORYNAME.c_str()));
		ipCategoryNames->PushBack(get_bstr_t(FP_FAM_REPORTS_CATEGORYNAME.c_str()));
		ipCategoryNames->PushBack(get_bstr_t(INPUTFUNNEL_IR_CATEGORYNAME.c_str()));

		// Also update the product-specific database items [P13 #4962]
		ipCategoryNames->PushBack(get_bstr_t(FP_FAM_PRODUCT_SPECIFIC_DB_MGRS.c_str()));

		getCategoryManager()->CheckForNewComponents(ipCategoryNames);

		// Create a new ExtractCategories.json 
		Extract::Utilities::UtilityMethods::GetExtractCategoriesJson(true);

		// Update the menus to reflect any components that were found
		enableMenus();
	}
	// This is needed because .net exception causes crash if not handled
	catch (Exception ^ex)
	{
		Extract::ExceptionExtensionMethods::ExtractDisplay(ex, "ELI45603");
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18159");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnManageTags()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Create a new tag manager dialog
		CManageTagsDlg dlg(m_ipFAMDB);

		// Display the dialog
		dlg.DoModal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27416");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnManageBatesCounters()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Create a new counter manager dialog
		CManageUserCountersDlg dlg(m_ipFAMDB);

		// Display the dialog
		dlg.DoModal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27787");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnManageLoginUsers()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Create a new user manager dialog
		CManageUsersDlg dlg(m_ipFAMDB);

		// Display the dialog
		dlg.DoModal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29061");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnManageWorkflowActions()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Display the WorkflowManagement dialog
		WorkflowManagement workFlow(m_ipFAMDB, marshal_as<String^>(m_strCurrentWorkflow));
		NativeWindow ^currentWindow = __nullptr;

		IntPtr managedHWND(this->GetSafeHwnd());
		currentWindow = NativeWindow::FromHandle(managedHWND);
		workFlow.ShowDialog(currentWindow);

		try
		{
			refreshWorkflowStatus();

			while (m_bUnaffiliatedFiles)
			{
				int iResult = MessageBox("This database has defined workflow(s) but files that exist "
					"outside those workflows.\r\n\r\nThese files need to be migrated to workflows before "
					"processing.\r\n\r\nDo you wish to migrate the files now?",
					"Migrate files to workflows?",
					MB_YESNO);
				if (iResult == IDYES)
				{
					if (showMoveToWorkflowDialog(true) != IDOK)
					{
						break;
					}
				}
				else
				{
					break;
				}
			}
		}
		finally
		{
			if (currentWindow)
				currentWindow->ReleaseHandle();
		}
	}
	// This is needed because .net exception causes crash if not handled
	catch (Exception ^ex)
	{
		Extract::ExceptionExtensionMethods::ExtractDisplay(ex, "ELI45604");
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29104");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnManageMetadataFields()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{	
		// Create a new Metadata fields manager dialog
		CManageMetadataFieldsDlg dlg(m_ipFAMDB);

		// Display the dialog
		dlg.DoModal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37649");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnToolsSetPriority()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{	
		// Create a new set file priority dialog
		CSetFilePriorityDlg dlg(m_ipFAMDB);

		// Display the dialog
		dlg.DoModal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27698");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnRecalculateStats()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{	
		// Set the wait cursor
		CWaitCursor wait;

		// Create a new set file priority dialog
		m_ipFAMDB->RecalculateStatistics();

		m_propSheet.SetActivePage(&m_propSummaryPage);

		UpdateSummaryTab();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI34327");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::UpdateSummaryTab(long nActionID /*= -1*/)
{
	try
	{
		// Update the summary tab
		if (m_propSummaryPage.m_hWnd != __nullptr)
		{
			m_propSummaryPage.populatePage(nActionID);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27696");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnManageAttributeSets()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{	
		// Create a new Metadata fields manager dialog
		CManageAttributeSets dlg(m_ipFAMDB);

		// Display the dialog
		dlg.DoModal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI38633");
}
//--------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnManageRuleCounters()
{
	try
	{
		ISecureCounterManagementPtr ipCounterManager(gstrDB_SECURE_COUNTER_MANAGER_GUID.c_str());
		ASSERT_RESOURCE_ALLOCATION("ELI39087", ipCounterManager != __nullptr);

		ipCounterManager->ShowUI(m_ipFAMDB, m_hWnd);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI39088");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnCbnSelchangeWorkflowCombo()
{
	try
	{
		CString cSelectedItem;
		int index = m_comboBoxWorkflow.GetCurSel();
		if (index >= 0)
		{
			m_comboBoxWorkflow.GetLBText(index, cSelectedItem);
			m_strCurrentWorkflow = cSelectedItem;
			m_nCurrentWorkflowID = m_comboBoxWorkflow.GetItemData(index);

			if (m_nCurrentWorkflowID == -1)
			{
				m_ipFAMDB->ActiveWorkflow = "";
			}
			else
			{
				// Set the Active workflow for the database
				m_ipFAMDB->ActiveWorkflow = get_bstr_t(m_strCurrentWorkflow);
			}
			UpdateSummaryTab();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI42073");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnToolsMoveFilesToWorkflow()
{
	try
	{
		showMoveToWorkflowDialog(m_bUnaffiliatedFiles);
	}
	// This is needed because .net exception causes crash if not handled
	catch (Exception ^ex)
	{
		Extract::ExceptionExtensionMethods::ExtractDisplay(ex, "ELI50069");
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI43362");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnManageDatabaseServices()
{
	try
	{
		ManageDatabaseServicesForm manageDatabaseServices(
			marshal_as<String^>(m_ipFAMDB->DatabaseServer),
			marshal_as<String^>(m_ipFAMDB->DatabaseName));

		NativeWindow ^currentWindow = __nullptr;
		try
		{
			IntPtr managedHWND(this->GetSafeHwnd());
			currentWindow = NativeWindow::FromHandle(managedHWND);
			manageDatabaseServices.ShowDialog(currentWindow);
		}
		finally
		{
			if (currentWindow)
				currentWindow->ReleaseHandle();
		}
	}
	// This is needed because .net exception causes crash if not handled
	catch (Exception ^ex)
	{
		Extract::ExceptionExtensionMethods::ExtractDisplay(ex, "ELI45605");
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI45585");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnManageMLModels()
{
	try
	{
		EditTableData editMLModel(marshal_as<String^>(m_ipFAMDB->DatabaseServer),
				marshal_as<String^>(m_ipFAMDB->DatabaseName), "MLModel");

		NativeWindow ^currentWindow = __nullptr;
		try
		{
			IntPtr managedHWND(this->GetSafeHwnd());
			currentWindow = NativeWindow::FromHandle(managedHWND);
			editMLModel.ShowDialog(currentWindow);
		}
		finally
		{
			if (currentWindow)
				currentWindow->ReleaseHandle();
		}
	}
	// This is needed because .net exception causes crash if not handled
	catch (Exception ^ex)
	{
		Extract::ExceptionExtensionMethods::ExtractDisplay(ex, "ELI45741");
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI45742");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnManageDashboards()
{
	try
	{
		ManageDashboardsForm manage(
			marshal_as<String^>(m_ipFAMDB->DatabaseServer), 
			marshal_as<String^>(m_ipFAMDB->DatabaseName));

		NativeWindow ^currentWindow = __nullptr;
		try
		{
			IntPtr managedHWND(this->GetSafeHwnd());
			currentWindow = NativeWindow::FromHandle(managedHWND);
			manage.ShowDialog(currentWindow);
		}
		finally
		{
			if (currentWindow)
				currentWindow->ReleaseHandle();
		}
	}
	// This is needed because .net exception causes crash if not handled
	catch (Exception ^ex)
	{
		Extract::ExceptionExtensionMethods::ExtractDisplay(ex, "ELI45766");
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI45767");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnToolsDashboards()
{
	try
	{
		String^ pathEXE = marshal_as<String^>( getCurrentProcessEXEDirectory() + "\\DashboardViewer.exe");

		String^ parameters = String::Format(CultureInfo::InvariantCulture,
			"/s \"{0}\" /d \"{1}\"", marshal_as<String^>(m_ipFAMDB->DatabaseServer), marshal_as<String^>(m_ipFAMDB->DatabaseName));
		CancellationToken noCancel(false);
		SystemMethods::RunExecutable(pathEXE, parameters, 0, false, noCancel, true);
	}
	// This is needed because .net exception causes crash if not handled
	catch (Exception^ ex)
	{
		Extract::ExceptionExtensionMethods::ExtractDisplay(ex, "ELI49924");
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI49925");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnManageWebAPIConfigs()
{
	try
	{
		ApiConfigMgmtForm configurationForm(
			marshal_as<String^>(m_ipFAMDB->DatabaseServer),
			marshal_as<String^>(m_ipFAMDB->DatabaseName));

		NativeWindow ^currentWindow = __nullptr;
		try
		{
			IntPtr managedHWND(this->GetSafeHwnd());
			currentWindow = NativeWindow::FromHandle(managedHWND);
			configurationForm.ShowDialog(currentWindow);
		}
		finally
		{
			if (currentWindow)
				currentWindow->ReleaseHandle();
		}
	}
	// This is needed because .net exception causes crash if not handled
	catch (Exception ^ex)
	{
		Extract::ExceptionExtensionMethods::ExtractDisplay(ex, "ELI53802");
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI53803");
}

//-------------------------------------------------------------------------------------------------
//INotifyDBConfigChanged
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnDBConfigChanged(string& rstrServer, string& rstrDatabase,
	string& rstrAdvConnStrProperties)
{
	// Must set flags if this fails but will still want the exception info
	try
	{
		// Set the database status on the database page
		m_propDatabasePage.setDBConnectionStatus(gstrCONNECTING);
		emptyWindowsMessageQueue();
		CWaitCursor cWait;

		// Preset the DB good flag to false
		m_bIsDBGood = false;
		m_bDBSchemaIsNotCurrent = false;
		m_bUnaffiliatedFiles = false;

		// Set the server and database
		m_ipFAMDB->DatabaseServer = rstrServer.c_str();
		m_ipFAMDB->DatabaseName = rstrDatabase.c_str();

		// Attempt to connect with the new settings
		m_ipFAMDB->ResetDBConnection(VARIANT_FALSE, VARIANT_TRUE);

		// In case path tags were expanded, return the literal database connection properties we
		// actually connected to.
		rstrServer = asString(m_ipFAMDB->DatabaseServer);
		rstrDatabase = asString(m_ipFAMDB->DatabaseName);

		// Set the connection is good to true
		m_bIsDBGood = true;
		m_nCurrentWorkflowID = -1;
		m_strCurrentWorkflow = gstrALL_WORKFLOWS;
		loadWorkflowComboBox();
	}
	// Log exceptions so exception dialog doesn't come up before the FAMDBAdminDlg
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI18173");

	if (!m_bIsDBGood)
	{
		// Determine if the reason the reset failed is because the schema is out-of-date.
		string strCurDBStatus = asString(m_ipFAMDB->GetCurrentConnectionStatus());
		if (strCurDBStatus == gstrWRONG_SCHEMA)
		{
			m_bDBSchemaIsNotCurrent = true;
		}
		else if (strCurDBStatus == gstrUNAFFILIATED_FILES)
		{
			m_bUnaffiliatedFiles = true;
		}
		m_comboBoxWorkflow.ResetContent();
	}

	// Get and set the status on the Database page
	setUIDatabaseStatus();

	// Enable the menus
	enableMenus();
}
//-------------------------------------------------------------------------------------------------
bool CFAMDBAdminDlg::PromptToSelectContext(bool& rbDBTagsAvailable)
{
	// FAMDBAdmin does not support context management.

	THROW_LOGIC_ERROR_EXCEPTION("ELI39303");
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::loadMenu()
{
	// Load the menu and retrieve the File submenu
	CMenu menu;
	menu.LoadMenu( IDR_MENU_FAMDBADMIN );

	// Associate the modified menu with the window
	SetMenu( &menu );
	menu.Detach();

	// Enable the menus based on the new flag settings
	enableMenus();
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::enableMenus()
{
	CMenu *pMenu = GetMenu();
	ASSERT_ARGUMENT("ELI18626", pMenu != __nullptr);

	UINT nEnable = (MF_BYCOMMAND | MF_ENABLED);
	UINT nDisable = (MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);

	// Enable all of the menus
	pMenu->EnableMenuItem(ID_DATABASE_CLEAR, nEnable);
	pMenu->EnableMenuItem(ID_DATABASE_LOGOUT, nEnable); 

	// Only enable other menu items if the db connection is good
	pMenu->EnableMenuItem(ID_DATABASE_CLEAR, m_bIsDBGood ? nEnable : nDisable);
	pMenu->EnableMenuItem(ID_DATABASE_RESETLOCK, m_bIsDBGood ? nEnable : nDisable);
	pMenu->EnableMenuItem(ID_DATABASE_UPDATE_SCHEMA, m_bDBSchemaIsNotCurrent ? nEnable : nDisable);
	pMenu->EnableMenuItem(ID_DATABASE_CHANGEPASSWORD, m_bIsDBGood ? nEnable : nDisable);
	pMenu->EnableMenuItem(ID_DATABASE_SET_OPTIONS, m_bIsDBGood ? nEnable : nDisable);
	pMenu->EnableMenuItem(ID_MANAGE_TAGS, m_bIsDBGood ? nEnable : nDisable);
	pMenu->EnableMenuItem(ID_MANAGE_BATES_COUNTERS, m_bIsDBGood ? nEnable : nDisable);
	pMenu->EnableMenuItem(ID_MANAGE_ACTIONS, (m_bIsDBGood || m_bUnaffiliatedFiles) ? nEnable : nDisable);
	pMenu->EnableMenuItem(ID_MANAGE_USERS, m_bIsDBGood ? nEnable : nDisable);
	pMenu->EnableMenuItem(ID_MANAGE_METADATA, m_bIsDBGood ? nEnable : nDisable);
	pMenu->EnableMenuItem(ID_MANAGE_ATTRIBUTESETS, m_bIsDBGood ? nEnable : nDisable);
	pMenu->EnableMenuItem(ID_MANAGE_RULE_COUNTERS, m_bIsDBGood ? nEnable : nDisable);
	pMenu->EnableMenuItem(ID_MANAGE_MLMODELS, m_bIsDBGood ? nEnable : nDisable);
	pMenu->EnableMenuItem(ID_MANAGE_DATABASESERVICES, m_bIsDBGood ? nEnable : nDisable);
	pMenu->EnableMenuItem(ID_TOOLS_MANUALLYSETACTIONSTATUS, m_bIsDBGood ? nEnable : nDisable);
	pMenu->EnableMenuItem(ID_TOOLS_SETPRIORITY, m_bIsDBGood ? nEnable : nDisable);
	pMenu->EnableMenuItem(ID_TOOLS_EXPORTFILELISTS, m_bIsDBGood ? nEnable : nDisable);
	pMenu->EnableMenuItem(ID_TOOLS_INSPECT_FILES, m_bIsDBGood ? nEnable : nDisable);
	pMenu->EnableMenuItem(ID_TOOLS_REPORTS, m_bIsDBGood ? nEnable : nDisable);
	pMenu->EnableMenuItem(ID_TOOLS_RECALCULATE_STATS, m_bIsDBGood ? nEnable : nDisable);
	pMenu->EnableMenuItem(ID_TOOLS_DASHBOARDS, m_bIsDBGood ? nEnable : nDisable); 
	pMenu->EnableMenuItem(ID_TOOLS_MOVE_FILES_TO_WORKFLOW, (m_bIsDBGood || m_bUnaffiliatedFiles) ? nEnable : nDisable);
	pMenu->EnableMenuItem(ID_DATABASE_IMPORT, m_bIsDBGood ? nEnable : nDisable); 
	pMenu->EnableMenuItem(ID_DATABASE_EXPORT, m_bIsDBGood ? nEnable : nDisable); 
	pMenu->EnableMenuItem(ID_MANAGE_DASHBOARDS, m_bIsDBGood ? nEnable : nDisable); 
	pMenu->EnableMenuItem(ID_MANAGE_WEBCONFIGS, m_bIsDBGood ? nEnable : nDisable); 
}
//-------------------------------------------------------------------------------------------------
bool CFAMDBAdminDlg::notifyNoActions()
{
	// Check if there is no action inside the database;
	IStrToStrMapPtr ipMapActions = m_ipFAMDB->GetActions();
	ASSERT_RESOURCE_ALLOCATION("ELI15255", ipMapActions != __nullptr );

	// If there is no action inside database
	if (ipMapActions->GetSize() == 0)
	{
		string strPrompt = "There are no actions inside the current database!";
		MessageBox(strPrompt.c_str(), "No action", MB_ICONINFORMATION);
		return true;
	}
	return false;
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::setPropPageSize()
{
	CRect rectDlg;
	GetClientRect(&rectDlg);
	
	rectDlg.bottom -= 30;

	// Set the property sheet size to the size of the dialog client area
	m_propSheet.resize(rectDlg);
}
//-------------------------------------------------------------------------------------------------
IMiscUtilsPtr CFAMDBAdminDlg::getMiscUtils()
{
	// check if a MiscUtils object has all ready been created
	if (!m_ipMiscUtils)
	{
		// create MiscUtils object
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI18035", m_ipMiscUtils != __nullptr);
	}

	return m_ipMiscUtils;
}
//-------------------------------------------------------------------------------------------------
ICategoryManagerPtr CFAMDBAdminDlg::getCategoryManager()
{
	if (!m_ipCategoryManager)
	{
		// create category manager object
		m_ipCategoryManager.CreateInstance(CLSID_CategoryManager);
		ASSERT_RESOURCE_ALLOCATION("ELI18038", m_ipCategoryManager != __nullptr);
	}

	return m_ipCategoryManager;
}
//--------------------------------------------------------------------------------------------------
ICategorizedComponentPtr CFAMDBAdminDlg::getCategorizedComponent(const std::string& strProgID)
{
	try
	{
		try
		{
			// instantiate the object via the ProgID value
			ICategorizedComponentPtr ipObject(strProgID.c_str());
			ASSERT_RESOURCE_ALLOCATION("ELI18157", ipObject != __nullptr);

			return ipObject;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI18149")
	}
	catch (UCLIDException &ue)
	{
		// Keep track of which classes for which we have notified the user.
		static std::set<std::string> setFailedIDs;

		if (setFailedIDs.find(strProgID) == setFailedIDs.end())
		{
			setFailedIDs.insert(strProgID);

			// Provide details about the plug-in object we failed to instantiate
			UCLIDException uexOuter("ELI18150", "Invalid component!", ue);
			uexOuter.addDebugInfo("ProgID", strProgID);

			uexOuter.display();
		}
	}

	return NULL;
}
//--------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::setUIDatabaseStatus()
{
	// Set the default status
	string strCurrDBStatus = gstrUNABLE_TO_CONNECT_TO_SERVER;
	try
	{
		// Try to get the status from the FAMDB
		strCurrDBStatus = asString(m_ipFAMDB->GetCurrentConnectionStatus());
	}
	catch(...)
	{
		// Don't need to do anything
	}

	// Set the status default status
	m_propDatabasePage.setDBConnectionStatus ( strCurrDBStatus );
}
//--------------------------------------------------------------------------------------------------
UINT CFAMDBAdminDlg::upgradeToCurrentSchemaThread(LPVOID pParam)
{
	try
	{
		ASSERT_ARGUMENT("ELI31384", pParam != __nullptr);
		CFAMDBAdminDlg *pFAMDBAdminDlg = (CFAMDBAdminDlg *)pParam;

		ASSERT_ARGUMENT("ELI31448", pFAMDBAdminDlg->m_ipFAMDB != __nullptr);
		
		try
		{
			try
			{
				// Perform the schema update.
				pFAMDBAdminDlg->m_ipFAMDB->UpgradeToCurrentSchema(
					pFAMDBAdminDlg->m_ipSchemaUpdateProgressStatus);

				// If we got here, the upgrade succeeded
				pFAMDBAdminDlg->m_bSchemaUpdateSucceeded = true;		
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI31389")
		}
		catch (UCLIDException &ue)
		{
			ue.display();
		}
			
		// Close the progress status dialog so the UI thread can continue.
		pFAMDBAdminDlg->m_ipSchemaUpdateProgressStatusDialog->Close();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI31387");

	return 0;
}
//--------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::loadWorkflowComboBox()
{
	// If the database connection is not in a good state there is nothing to do
	if ((!m_bIsDBGood && !m_bUnaffiliatedFiles) || m_ipFAMDB == __nullptr)
	{
		return;
	}
	m_comboBoxWorkflow.ResetContent();

	IStrToStrMapPtr ipWorkflows = m_ipFAMDB->GetWorkflows();
	ASSERT_RESOURCE_ALLOCATION("ELI50001", ipWorkflows != __nullptr);

	IIUnknownVectorPtr ipItemPairs = ipWorkflows->GetAllKeyValuePairs();

	int numItems = ipItemPairs->Size();
	for (int i = 0; i < numItems; i++)
	{
		IStringPairPtr ipCurrentActionPair = (IStringPairPtr)ipItemPairs->At(i);
		int index = m_comboBoxWorkflow.AddString(ipCurrentActionPair->StringKey);
		long lValue = asLong(ipCurrentActionPair->StringValue);
		m_comboBoxWorkflow.SetItemData(index, lValue);

		// Check for change current workflow id so if text has changed it will be selected correctly
		if (m_nCurrentWorkflowID >= 0 && m_nCurrentWorkflowID == lValue)
		{
			m_strCurrentWorkflow = asString(ipCurrentActionPair->StringKey);
		}
	}
	m_comboBoxWorkflow.InsertString(0, gstrALL_WORKFLOWS.c_str());
	m_comboBoxWorkflow.SetItemData(0, -1);
	int index = m_comboBoxWorkflow.FindStringExact(0, m_strCurrentWorkflow.c_str());
	if (index < 0)
	{ 
		m_strCurrentWorkflow = gstrALL_WORKFLOWS;
		m_nCurrentWorkflowID = -1;
		index = 0;
		m_ipFAMDB->ActiveWorkflow = "";
	}
	m_comboBoxWorkflow.SetCurSel(index);
	if (index >= 0)
	{
		m_nCurrentWorkflowID = m_comboBoxWorkflow.GetItemData(index);
	}
}
//-------------------------------------------------------------------------------------------------
bool CFAMDBAdminDlg::refreshWorkflowStatus()
{
	// Update the workflow combo box to reflect any changes made to the available workflows.
	// (add/delete/rename)
	loadWorkflowComboBox();

	UCLIDException &uex = UCLIDException();

	if (m_nCurrentWorkflowID >= 0)
	{
		// If the current workflow was renamed, m_strCurrentWorkflow will now represent the
		// new name; apply it to m_ipFAMDB to avoid an error running ResetDBConnection below.
		// https://extract.atlassian.net/browse/ISSUE-14779
		m_ipFAMDB->ActiveWorkflow = m_strCurrentWorkflow.c_str();
	}

	try
	{
		// Force re-check for unaffiliated files.
		m_ipFAMDB->ResetDBConnection(VARIANT_FALSE, VARIANT_TRUE);

		m_bIsDBGood = true;
		m_bUnaffiliatedFiles = false;
	}
	catch (...)
	{
		refreshDBStatus();
	}

	// Get and set the status on the Database page
	setUIDatabaseStatus();

	// Enable the menus
	enableMenus();

	//Update the summary tab
	UpdateSummaryTab();

	return m_bIsDBGood;
}
//--------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::positionWorkflowControls()
{
	CRect rectDlg, labelRect, controlRect;
	GetClientRect(rectDlg);

	m_staticWorkflowLabel.GetWindowRect(labelRect);
	ScreenToClient(labelRect);

	labelRect.MoveToY(rectDlg.bottom - labelRect.Height() - 14);
	m_staticWorkflowLabel.MoveWindow(labelRect);

	m_comboBoxWorkflow.GetWindowRect(controlRect);
	ScreenToClient(controlRect);
	controlRect.MoveToY(rectDlg.bottom - controlRect.Height() - 9);
	controlRect.right = rectDlg.right - 6;
	m_comboBoxWorkflow.MoveWindow(controlRect);

}
//--------------------------------------------------------------------------------------------------
int CFAMDBAdminDlg::showMoveToWorkflowDialog(bool bAreUnaffiliatedFiles)
{
	MoveToWorkflowForm moveToWorkflow(m_ipFAMDB, bAreUnaffiliatedFiles);
	NativeWindow ^currentWindow = __nullptr;
	try
	{
		IntPtr managedHWND(this->GetSafeHwnd());
		currentWindow = NativeWindow::FromHandle(managedHWND);
		DialogResult result = moveToWorkflow.ShowDialog(currentWindow);

		refreshWorkflowStatus();

		return (int)result;
	}
	finally
	{
		if (currentWindow)
			currentWindow->ReleaseHandle();
	}
}
//--------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::refreshDBStatus()
{
	string strServer = asString(m_ipFAMDB->DatabaseServer);
	string strDatabaseName = asString(m_ipFAMDB->DatabaseName);
	string strAdvConnStringProperties =
		asString(m_ipFAMDB->AdvancedConnectionStringProperties);
	OnDBConfigChanged(strServer, strDatabaseName, strAdvConnStringProperties);
}
//--------------------------------------------------------------------------------------------------

