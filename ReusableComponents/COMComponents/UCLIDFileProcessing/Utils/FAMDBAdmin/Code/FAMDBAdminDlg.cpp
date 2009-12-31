// FAMDBAdminDlg.cpp : implementation file
//

#include "stdafx.h"
#include "FAMDBAdmin.h"
#include "FAMDBAdminDlg.h"
#include "FAMDBAdminAboutDlg.h"
#include "ClearWarningDlg.h"
#include "ExportFileListDlg.h"
#include "FileProcessingAddActionDlg.h"
#include "ManageUserCountersDlg.h"
#include "ManageTagsDlg.h"
#include "RenameActionDlg.h"
#include "SetActionStatusDlg.h"
#include "SetFilePriorityDlg.h"
#include "..\..\..\code\FPCategories.h"
#include "..\..\..\..\InputFunnel\IFCore\Code\IFCategories.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <ComUtils.h>
#include <RegistryPersistenceMgr.h>
#include <FAMUtilsConstants.h>

#include <set>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const int giMIN_WINDOW_WIDTH = 700;
const int giMIN_WINDOW_HEIGHT = 320;
const string gstrFILE_ACTION_MANAGER_FILENAME = "ProcessFiles.exe";
const string gstrREPORT_VIEWER_EXE = "ReportViewer.exe";

//-------------------------------------------------------------------------------------------------
// CFAMDBAdminDlg dialog
//-------------------------------------------------------------------------------------------------
CFAMDBAdminDlg::CFAMDBAdminDlg(IFileProcessingDBPtr ipFAMDB,CWnd* pParent /*=NULL*/)
: CDialog(CFAMDBAdminDlg::IDD, pParent),
m_ipFAMDB(ipFAMDB),
m_bIsDBGood(false),
m_ipMiscUtils(NULL),
m_ipCategoryManager(NULL),
m_bInitialized(false)
{
	try
	{
		// Make sure there is a FAMDB object
		ASSERT_ARGUMENT("ELI17610", m_ipFAMDB != NULL);

		m_hIcon = AfxGetApp()->LoadIcon(IDI_ICON_FAMDBADMIN);

		ma_pUserCfgMgr = auto_ptr<IConfigurationSettingsPersistenceMgr>(new
			RegistryPersistenceMgr(HKEY_CURRENT_USER, ""));
		
		ma_pCfgMgr = auto_ptr<FileProcessingConfigMgr>(new
			FileProcessingConfigMgr(ma_pUserCfgMgr.get(), "\\FAMDBAdmin"));

		// Set the Database Page notify to this object ( so the status can be updated )
		m_propDatabasePage.setNotifyDBConfigChanged(this);

		// set the database pointer for the summary page
		m_propSummaryPage.setFAMDatabase(m_ipFAMDB);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI17609");
}
//-------------------------------------------------------------------------------------------------
CFAMDBAdminDlg::~CFAMDBAdminDlg()
{
	try
	{
		m_ipFAMDB = NULL;
		m_ipMiscUtils = NULL;
		m_ipCategoryManager = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI18124")
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CFAMDBAdminDlg, CDialog)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_COMMAND(ID_HELP_ABOUTTHISAPPLICATION, &CFAMDBAdminDlg::OnHelpAbout)
	ON_COMMAND(ID_TOOLS_EXPORTFILELISTS, &CFAMDBAdminDlg::OnExportFileLists)
	ON_COMMAND(ID_TOOLS_FILEACTIONMANAGER, &CFAMDBAdminDlg::OnToolsFileActionManager)
	ON_COMMAND(ID_DATABASE_EXIT, &CFAMDBAdminDlg::OnExit)
	ON_COMMAND(ID_DATABASE_CLEAR, &CFAMDBAdminDlg::OnDatabaseClear)
	ON_COMMAND(ID_DATABASE_RESETLOCK, &CFAMDBAdminDlg::OnDatabaseResetLock)
	ON_COMMAND(ID_DATABASE_CHANGEPASSWORD, &CFAMDBAdminDlg::OnDatabaseChangePassword)
	ON_COMMAND(ID_DATABASE_LOGOUT, &CFAMDBAdminDlg::OnDatabaseLogout)
	ON_COMMAND(ID_ACTION_ADD, &CFAMDBAdminDlg::OnActionAdd)
	ON_COMMAND(ID_ACTION_REMOVE, &CFAMDBAdminDlg::OnActionRemove)
	ON_COMMAND(ID_ACTION_RENAME, &CFAMDBAdminDlg::OnActionRename)
	ON_COMMAND(ID_ACTION_MANUALLYSETACTIONSTATUS, &CFAMDBAdminDlg::OnActionManuallySetActionStatus)
	ON_COMMAND(ID_HELP_FILEACTIONMANAGERHELP, &CFAMDBAdminDlg::OnHelpFileActionManagerHelp)
	ON_WM_SIZE()
	ON_WM_GETMINMAXINFO()
	ON_COMMAND(ID_TOOLS_REPORTS, &CFAMDBAdminDlg::OnToolsReports)
	ON_COMMAND(ID_TOOLS_CHECKFORNEWCOMPONENTS, &CFAMDBAdminDlg::OnToolsCheckForNewComponents)
	ON_COMMAND(ID_TOOLS_MANAGE_TAGS, &CFAMDBAdminDlg::OnToolsManageTags)
	ON_COMMAND(ID_TOOLS_MANAGE_COUNTERS, &CFAMDBAdminDlg::OnToolsManageCounters)
	ON_COMMAND(ID_TOOLS_SETPRIORITY, &CFAMDBAdminDlg::OnToolsSetPriority)
	//}}AFX_MSG_MAP
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

		// Get the server and datatabase from the FAMDB
		string strServer = asString(m_ipFAMDB->DatabaseServer);
		string strDatabase = asString(m_ipFAMDB->DatabaseName);

		// Save the good settings to the registry
		ma_pCfgMgr->setLastGoodDBSettings(strServer, strDatabase);

		// Set the Server and DB names
		m_propDatabasePage.setServerAndDBName(strServer, strDatabase);
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
void CFAMDBAdminDlg::OnExit()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		m_ipFAMDB = NULL;
		CDialog::OnCancel();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13986");
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

		// Get a list of the action names to preserve
		vector<string> vecActionNames;
		bool bRetainActions = prompt.getRetainActions();
		if (bRetainActions)
		{
			// Read all actions from the DB
			IStrToStrMapPtr ipMapActions = m_ipFAMDB->GetActions();
			ASSERT_RESOURCE_ALLOCATION("ELI25184", ipMapActions != NULL);
			IVariantVectorPtr ipActions = ipMapActions->GetKeys();
			ASSERT_RESOURCE_ALLOCATION("ELI25185", ipActions != NULL);

			// Iterate over the actions
			long lSize = ipActions->Size;
			vecActionNames.reserve(lSize);
			for (int i = 0; i < lSize; i++)
			{
				// Get each action name and add it to the vector
				_variant_t action = ipActions->Item[i];
				vecActionNames.push_back( asString(action.bstrVal) );
			}
		}

		// Set the database is good flag to false
		m_bIsDBGood = false;
		try
		{
			// Clear the database
			m_ipFAMDB->Clear();

			// Add any retained actions
			for (unsigned int i = 0; i < vecActionNames.size(); i++)
			{
				m_ipFAMDB->DefineNewAction(vecActionNames[i].c_str());
			}
		}
		catch(...)
		{
			// Enable menus
			enableMenus();

			// Update the summary tab
			updateSummaryTab();
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
		uex.addDebugInfo("Actions", bRetainActions ? "Retained" : "Not retained");
		uex.log();

		// Set the database status
		setUIDatabaseStatus();

		MessageBox("Current database has been cleared.", "Success", MB_ICONINFORMATION);

		// Enable the menus based on the new flag settings
		enableMenus();

		// Update the summary tab
		updateSummaryTab();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14861");
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
void CFAMDBAdminDlg::OnActionAdd()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Create an add action dialog
		FileProcessingAddActionDlg dlgAddAction(m_ipFAMDB);

		if (dlgAddAction.DoModal() == IDOK)
		{
			// Add application trace whenever a database modification is made
			// [LRCAU #5052 - JDS - 12/18/2008]
			UCLIDException uex("ELI23600", "Application trace: Database change");
			uex.addDebugInfo("Change", "Add new action");
			uex.addDebugInfo("User Name", getCurrentUserName());
			uex.addDebugInfo("Server Name", asString(m_ipFAMDB->DatabaseServer));
			uex.addDebugInfo("Database", asString(m_ipFAMDB->DatabaseName));

			// Get the new action name
			std::string strNewAction = std::string ((LPCTSTR) dlgAddAction.GetActionName());
			if (strNewAction != "")
			{
				// Get the default status for the new action
				int iStatus = dlgAddAction.GetDefaultStatus();

				// Add the new action to the database
				DWORD dwNewActionID;

				// Display a wait-cursor because we are adding an action to DB
				CWaitCursor wait;
				
				dwNewActionID = m_ipFAMDB->DefineNewAction(_bstr_t(strNewAction.c_str()));
				uex.addDebugInfo("New Action Name", strNewAction);
				uex.addDebugInfo("New Action ID", dwNewActionID);

				// If the default status equal to -1,
				// We should copy the status from one action to another
				if (iStatus == -1)
				{
					// Get the action ID that the status of which will be copied from
					DWORD dwCopyActionID = dlgAddAction.GetCopyStatusActionID();

					// Copy action status from another action to the new action
					m_ipFAMDB->CopyActionStatusFromAction(dwCopyActionID, dwNewActionID);
					uex.addDebugInfo("Action ID Copied From", dwCopyActionID);
				}
				// If the default status is not -1
				else
				{
					// Cast the default status to an EActionStatus type
					UCLID_FILEPROCESSINGLib::EActionStatus eStatus;
					eStatus = (UCLID_FILEPROCESSINGLib::EActionStatus)(iStatus);

					// Call SetStatusForAllFiles() to set the default status for
					// newly added action
					m_ipFAMDB->SetStatusForAllFiles(_bstr_t(strNewAction.c_str()), eStatus);
					uex.addDebugInfo("Default Status", asString(m_ipFAMDB->AsStatusString(eStatus)));
				} // Inner else block

				// Restore the wait cursor because we have finished adding an action to DB
				wait.Restore();

				// Log application trace [LRCAU #5052 - JDS - 12/18/2008]
				uex.log();

				// Update the summary tab
				updateSummaryTab();
			} // Out if block

			// Prompt to the user that the action has been added
			string strPrompt = "'" + strNewAction + "' has been added to the database.";
			MessageBox(strPrompt.c_str(), "Add Action", MB_ICONINFORMATION);
		} // Dialog block
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14865");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnActionRemove()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Check if there is no action inside the datebase;
		if ( notifyNoActions() )
		{
			return;
		}

		// Create IFAMDBUtilsPtr used to bring up select action dialog
		UCLID_FILEPROCESSINGLib::IFAMDBUtilsPtr ipFAMDBUtils(CLSID_FAMDBUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI14914", ipFAMDBUtils != NULL );

		// Get the action's name from the dialog
		_bstr_t bstrActionName = ipFAMDBUtils->PromptForActionSelection(m_ipFAMDB, "Remove Action", "");

		// Remove the action selected if strActionName is not empty
		if (bstrActionName.length() > 0)
		{
			// Display the wait cursor wheil action is deleted
			CWaitCursor cursor;

			m_ipFAMDB->DeleteAction(bstrActionName);

			// Add application trace whenever a database modification is made
			// [LRCAU #5052 - JDS - 12/18/2008]
			UCLIDException uex("ELI23601", "Application trace: Database change");
			uex.addDebugInfo("Change", "Remove action");
			uex.addDebugInfo("User Name", getCurrentUserName());
			uex.addDebugInfo("Server Name", asString(m_ipFAMDB->DatabaseServer));
			uex.addDebugInfo("Database", asString(m_ipFAMDB->DatabaseName));
			uex.addDebugInfo("Action To Remove", asString(bstrActionName));
			uex.log();

			// Update the summary tab
			updateSummaryTab();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14866");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnActionRename()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Check if there is no action inside the datebase;
		if ( notifyNoActions() )
		{
			return;
		}

		// Create an add action dialog
		RenameActionDlg dlgRenameAction(m_ipFAMDB);

		if (dlgRenameAction.DoModal() == IDOK)
		{
			// Add application trace whenever a database modification is made
			// [LRCAU #5052 - JDS - 12/18/2008]
			UCLIDException uex("ELI23602", "Application trace: Database change");
			uex.addDebugInfo("Change", "Rename action");
			uex.addDebugInfo("User Name", getCurrentUserName());
			uex.addDebugInfo("Server Name", asString(m_ipFAMDB->DatabaseServer));
			uex.addDebugInfo("Database", asString(m_ipFAMDB->DatabaseName));

			// Display wait cursor
			CWaitCursor wait;

			// Init the action ID, old name and new name
			DWORD dwID;
			string strOldName = "";
			string strNewName = "";

			// Get the old name and the new name
			dwID = dlgRenameAction.GetOldNameAndNewName(strOldName, strNewName);

			if (dwID > 0)
			{
				if (strOldName != strNewName)
				{
					// Call RenameAction to rename the action
					m_ipFAMDB->RenameAction(dwID, _bstr_t(strNewName.c_str()));
					uex.addDebugInfo("Old Action Name", strOldName);
					uex.addDebugInfo("New Action Name", strNewName);

					// Log application trace [LRCAU #5052 - JDS - 12/18/2008]
					uex.log();

					// Update the summary tab
					updateSummaryTab();

					// Display the message that the action is renamed
					string strPrompt = "The action '" + strOldName + "' has been renamed to '"
										+ strNewName + "'.";
					MessageBox( strPrompt.c_str(), "Success", MB_OK|MB_ICONINFORMATION );
				}
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14867");
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
#ifdef _DEBUG
		MessageBox("Please launch Help file manually from the network location.", "Help File", MB_OK);
		return;
#endif
		
		// go up Two level, from "..\Extract Systems\FlexIndexComponents\Bin\" 
		// to "..\Extract Systems\", and then go to
		// "..\Extract Systems\FLEX Index\Help\"
		string strExtractFolder = getCurrentProcessEXEDirectory();
		string strExtractSystemFolderName("\\Extract Systems");
		int nBinPos = strExtractFolder.rfind(strExtractSystemFolderName);
		if (nBinPos == string::npos )
		{
			UCLIDException ue("ELI14871", "Can't find Help file.");
			ue.addDebugInfo("Extract Systems Folder", strExtractFolder);
			throw ue;
		}

		// remove the Bin folder and FlexIndexComponents folder
		string strHelpPath = strExtractFolder.substr(0, nBinPos + strExtractSystemFolderName.length() );

		// Initialize the paths to possible help files
		string strFlexHelpPath = strHelpPath + "\\FlexIndex\\Help\\FLEXIndex.chm";
		string strIDShieldHelpPath = strHelpPath + "\\IDShield\\Help\\IDShield.chm";
		string strLabDEHelpPath = strHelpPath + "\\LabDE\\Help\\LabDE.chm";

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
			UCLIDException ue( "ELI15220", "Unable to find Help file." );
			ue.addDebugInfo( "Flex Help Path", strFlexHelpPath );
			ue.addDebugInfo( "ID Shield Help Path", strIDShieldHelpPath );
			ue.addDebugInfo( "LabDE Help Path", strLabDEHelpPath );
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

		// Set the the parameters to load the server and database in the FAM
		// Add quotes around the database name [LegacyRC #5124]
		string strParameters = "/sd " + asString(m_ipFAMDB->DatabaseServer) + 
			" \"" + asString(m_ipFAMDB->DatabaseName) + "\"";

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
			+ "\"" + asString(m_ipFAMDB->DatabaseName) + "\"";

		// Start the ReportViewer application
		runEXE(strEXEPath, strParameters, 0, NULL, getDirectoryFromFullPath(strEXEPath));
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18033");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnToolsCheckForNewComponents() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{	
		// create a vector of all categories we care about.
		IVariantVectorPtr ipCategoryNames(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI18165", ipCategoryNames != NULL);

		ipCategoryNames->PushBack(get_bstr_t(FP_FILE_PROC_CATEGORYNAME.c_str()));
		ipCategoryNames->PushBack(get_bstr_t(FP_FILE_SUPP_CATEGORYNAME.c_str()));
		ipCategoryNames->PushBack(get_bstr_t(FP_FAM_CONDITIONS_CATEGORYNAME.c_str()));
		ipCategoryNames->PushBack(get_bstr_t(FP_FAM_REPORTS_CATEGORYNAME.c_str()));
		ipCategoryNames->PushBack(get_bstr_t(INPUTFUNNEL_IR_CATEGORYNAME.c_str()));

		// Also update the product-specific database items [P13 #4962]
		ipCategoryNames->PushBack(get_bstr_t(FP_FAM_PRODUCT_SPECIFIC_DB_MGRS.c_str()));

		getCategoryManager()->CheckForNewComponents(ipCategoryNames);

		// Update the menus to reflect any components that were found
		enableMenus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18159");
}
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnToolsManageTags()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

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
void CFAMDBAdminDlg::OnToolsManageCounters()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

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
void CFAMDBAdminDlg::NotifyStatusChanged()
{
	try
	{
		// Update the summary tab
		updateSummaryTab();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27696");
}

//-------------------------------------------------------------------------------------------------
//INotifyDBConfigChanged
//-------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::OnDBConfigChanged(const std::string& strServer, const std::string& strDatabase)
{
	// Must set flags if this fails but will still want the exception info
	try
	{
		// Preset the DB good flag to false
		m_bIsDBGood = false;

		// Set the server and database
		m_ipFAMDB->DatabaseServer = strServer.c_str();
		m_ipFAMDB->DatabaseName = strDatabase.c_str();

		// Attempt to connect with the new settings
		m_ipFAMDB->ResetDBConnection();

		// Set the connection is good to true
		m_bIsDBGood = true;
	}
	// Log exceptions so exception dialog doesn't come up before the FAMDBAdminDlg
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI18173"); 

	// Get and set the status on the Database page
	setUIDatabaseStatus();

	// Enable the menus
	enableMenus();
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
	ASSERT_ARGUMENT("ELI18626", pMenu != NULL);

	UINT nEnable = (MF_BYCOMMAND | MF_ENABLED);
	UINT nDisable = (MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);

	// Enable all of the menus
	pMenu->EnableMenuItem(ID_DATABASE_CLEAR, nEnable);
	pMenu->EnableMenuItem(ID_DATABASE_LOGOUT, nEnable);

	// Only enable other menu items if the db connection is good
	pMenu->EnableMenuItem(ID_DATABASE_RESETLOCK, m_bIsDBGood ? nEnable : nDisable);
	pMenu->EnableMenuItem(ID_DATABASE_CHANGEPASSWORD, m_bIsDBGood ? nEnable : nDisable);
	pMenu->EnableMenuItem(ID_ACTION_ADD, m_bIsDBGood ? nEnable : nDisable);
	pMenu->EnableMenuItem(ID_ACTION_REMOVE, m_bIsDBGood ? nEnable : nDisable);
	pMenu->EnableMenuItem(ID_ACTION_RENAME, m_bIsDBGood ? nEnable : nDisable);
	pMenu->EnableMenuItem(ID_ACTION_MANUALLYSETACTIONSTATUS, m_bIsDBGood ? nEnable : nDisable);
	pMenu->EnableMenuItem(ID_TOOLS_EXPORTFILELISTS, m_bIsDBGood ? nEnable : nDisable);
	pMenu->EnableMenuItem(ID_TOOLS_REPORTS, m_bIsDBGood ? nEnable : nDisable);
}
//-------------------------------------------------------------------------------------------------
bool CFAMDBAdminDlg::notifyNoActions()
{
	// Check if there is no action inside the datebase;
	IStrToStrMapPtr ipMapActions = m_ipFAMDB->GetActions();
	ASSERT_RESOURCE_ALLOCATION("ELI15255", ipMapActions != NULL );

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
		ASSERT_RESOURCE_ALLOCATION("ELI18035", m_ipMiscUtils != NULL);
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
		ASSERT_RESOURCE_ALLOCATION("ELI18038", m_ipCategoryManager != NULL);
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
			ASSERT_RESOURCE_ALLOCATION("ELI18157", ipObject != NULL);

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

			// Provide datails about the plug-in object we failed to instantiate
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
		// Dont need to do anything
	}

	// Set the status default status
	m_propDatabasePage.setDBConnectionStatus ( strCurrDBStatus );
}
//--------------------------------------------------------------------------------------------------
void CFAMDBAdminDlg::updateSummaryTab()
{
	if (m_propSummaryPage.m_hWnd != NULL)
	{
		m_propSummaryPage.populatePage();
	}
}
//--------------------------------------------------------------------------------------------------
