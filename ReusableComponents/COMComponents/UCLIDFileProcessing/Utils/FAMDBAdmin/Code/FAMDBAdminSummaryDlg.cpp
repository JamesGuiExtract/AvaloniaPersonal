// FAMDBAdminSummaryDlg.cpp 
#include "StdAfx.h"
#include "FAMDBAdminSummaryDlg.h"
#include "ExportFileListDlg.h"
#include "SetActionStatusDlg.h"
#include "FAMUtilsConstants.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <ComUtils.h>
#include <ADOUtils.h>
#include <TemporaryFileName.h>
#include <ClipboardManager.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
// constants for referring to the columns in the summary list control
const int giACTION_COLUMN = 0;
const int giUNATTEMPTED_COLUMN = 1;
const int giPENDING_COLUMN = 2;
const int giPROCESSING_COLUMN = 3;
const int giCOMPLETED_COLUMN = 4;
const int giSKIPPED_COLUMN = 5;
const int giFAILED_COLUMN = 6;
const int giTOTALS_COLUMN = 7;

// constants for the summary list control setup
const int giNUMBER_OF_COLUMNS = 8;
const int giMIN_SIZE_COLUMN = 80;

// action status codes and action status names for each column of the list control
const string gmapCOLUMN_STATUS[][2] =
{
	{ "",  "Action" },
	{ "U", "Unattempted"},
	{ "P", "Pending" },
	{ "R", "Processing" },
	{ "C", "Complete" },
	{ "S", "Skipped" },
	{ "F", "Failed" },
	{ "",  "Total" }
};

// Query to retrieve the last 1000 exceptions for failed files on the specified action.
const string gstrFAILED_FILES_EXCEPTIONS_QUERY =
	";WITH FASTFailureRows AS \r\n"
	"(\r\n"
	"	SELECT TOP 1000 FileActionStateTransition.ID, \r\n"
	"			ROW_NUMBER() OVER(PARTITION BY FileActionStateTransition.FileID ORDER BY DateTimeStamp DESC) AS Instance \r\n"
	"		FROM FileActionStateTransition \r\n"
	"		INNER JOIN FileActionStatus ON FileActionStateTransition.FileID = FileActionStatus.FileID \r\n"
	"			AND FileActionStateTransition.ActionID = FileActionStatus.ActionID \r\n"
	"			AND ASC_To = ActionStatus \r\n"
	"		INNER JOIN Action ON FileActionStateTransition.ActionID = Action.ID \r\n"
	"		LEFT JOIN Workflow ON WorkflowID = Workflow.ID \r\n"
	"		WHERE ActionStatus = 'F' \r\n"
	"			AND ASCName = '<Action>' \r\n"
	"			AND('<WorkflowID>' <= 0 OR '<WorkflowID>' = Workflow.ID) \r\n"
	") \r\n"
	"SELECT TOP 1000 MachineName, UserName, DateTimeStamp, Exception \r\n"
	"	FROM FileActionStateTransition \r\n"
	"	INNER JOIN FASTFailureRows ON FASTFailureRows.ID = FileActionStateTransition.ID AND Instance = 1 \r\n"
	"	INNER JOIN Machine ON FileActionStateTransition.MachineID = Machine.ID \r\n"
	"	INNER JOIN FAMUser ON FileActionStateTransition.FAMUserID = FAMUser.ID \r\n"
	"	WHERE FileActionStateTransition.Exception IS NOT NULL \r\n"
	"	ORDER BY DateTimeStamp DESC";

//--------------------------------------------------------------------------------------------------
// FAMDBAdminSummary dialog
//--------------------------------------------------------------------------------------------------

IMPLEMENT_DYNAMIC(CFAMDBAdminSummaryDlg, CPropertyPage)

//--------------------------------------------------------------------------------------------------
CFAMDBAdminSummaryDlg::CFAMDBAdminSummaryDlg(void) :
CPropertyPage(CFAMDBAdminSummaryDlg::IDD),
m_ipFAMDB(__nullptr),
m_ipContextMenuFileSelector(CLSID_FAMFileSelector),
m_ipFAMFileInspector(__nullptr),
m_bInitialized(false),
m_bUseOracleSyntax(false),
m_eWorkflowVisibilityMode(EWorkflowVisibility::All)
{
	try
	{
		ASSERT_RESOURCE_ALLOCATION("ELI35684", m_ipContextMenuFileSelector != __nullptr);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI19593");
}
//--------------------------------------------------------------------------------------------------
CFAMDBAdminSummaryDlg::~CFAMDBAdminSummaryDlg(void)
{
	try
	{
		m_ipContextMenuFileSelector = __nullptr;
		m_ipFAMFileInspector = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI19594");
}
//--------------------------------------------------------------------------------------------------
void CFAMDBAdminSummaryDlg::DoDataExchange(CDataExchange *pDX)
{
	CPropertyPage::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_LIST_ACTIONS, m_listActions);
	DDX_Control(pDX, IDC_EDIT_FILE_TOTAL, m_editFileTotal);
	DDX_Control(pDX, IDC_BUTTON_REFRESH_SUMMARY, m_btnRefreshSummary);
	DDX_Control(pDX, IDC_STATIC_TOTAL_LABEL, m_lblTotals);
	DDX_Control(pDX, IDC_STATIC_UPDATED, m_staticLastUpdated);
	DDX_Control(pDX, IDC_SHOW_STATS_TYPE, m_staticStatisticsType);
	DDX_Control(pDX, IDC_RADIO_SHOW_VISIBLE_STATS, m_btnShowNonDeletedFileStats);
	DDX_Control(pDX, IDC_RADIO_SHOW_INVISIBLE_STATS, m_btnShowDeletedFileStats);
	DDX_Control(pDX, IDC_RADIO_SHOW_ALL_STATS, m_btnShowAllStats);
}

//--------------------------------------------------------------------------------------------------
// Message Map
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CFAMDBAdminSummaryDlg, CPropertyPage)
	ON_BN_CLICKED(IDC_BUTTON_REFRESH_SUMMARY, &OnBnClickedRefreshSummary)
	ON_BN_CLICKED(IDC_RADIO_SHOW_VISIBLE_STATS, &OnBnClickedShowDeletedFileStats)
	ON_BN_CLICKED(IDC_RADIO_SHOW_INVISIBLE_STATS, &OnBnClickedShowDeletedFileStats)
	ON_BN_CLICKED(IDC_RADIO_SHOW_ALL_STATS, &OnBnClickedShowDeletedFileStats)
	ON_WM_SIZE()
	ON_NOTIFY(NM_RCLICK, IDC_LIST_ACTIONS, &CFAMDBAdminSummaryDlg::OnNMRClickListActions)
	ON_COMMAND(ID_SUMMARY_MENU_EXPORT_LIST, &OnContextExportFileList)
	ON_COMMAND(ID_SUMMARY_MENU_SET_ACTION_STATUS, &OnContextSetFileActionStatus)
	ON_COMMAND(ID_SUMMARY_MENU_VIEW_FAILED, &OnContextViewFailed)
	ON_COMMAND(ID_SUMMARY_MENU_INSPECT_FILES, &OnContextInspectFiles)
	ON_COMMAND(ID_SUMMARY_MENU_ROW_HEADER_COPY, &OnContextCopyActionName)
	ON_COMMAND(ID_SUMMARY_MENU_COPY_COUNT, &OnContextCopyCount)
END_MESSAGE_MAP()
//--------------------------------------------------------------------------------------------------

//--------------------------------------------------------------------------------------------------
// CFAMDBAdminSummaryDlg message handlers
//--------------------------------------------------------------------------------------------------
BOOL CFAMDBAdminSummaryDlg::OnInitDialog()
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	try
	{
		CPropertyPage::OnInitDialog();

		// set the wait cursor
		CWaitCursor wait;

		m_btnShowDeletedFileStats.SetCheck(asBSTChecked(m_eWorkflowVisibilityMode == EWorkflowVisibility::Invisible));
		m_btnShowNonDeletedFileStats.SetCheck(asBSTChecked(m_eWorkflowVisibilityMode == EWorkflowVisibility::Visible));
		m_btnShowAllStats.SetCheck(asBSTChecked(m_eWorkflowVisibilityMode == EWorkflowVisibility::All));

		prepareListControl();
		resizeListColumns();
		populatePage();
		
		m_bInitialized = true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19595");

	return TRUE;  // return TRUE  unless you set the focus to a control
}
//--------------------------------------------------------------------------------------------------
void CFAMDBAdminSummaryDlg::OnSize(UINT nType, int cx, int cy)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// dialog has not been initialized, return
		if (!m_bInitialized)
		{
			return;
		}

		CRect recDlg, recListCtrl, recRefresh, recTotalText, recLabel, recLastUpdated,
			recStatisticsType, recShowNonDeletedFiles, recShowDeletedFiles, recShowAll;

		// Get current positions of the controls and then move them.
		// Start at the bottom and work up so that the controls stick to the bottom of the dialog

		// get the summary page rectangle
		GetClientRect(&recDlg);

		// get the list control rectangle
		m_listActions.GetWindowRect(&recListCtrl);
		ScreenToClient(&recListCtrl);

		// get the total label rectangle
		m_lblTotals.GetWindowRect(&recLabel);
		ScreenToClient(&recLabel);

		// get the total text box
		m_editFileTotal.GetWindowRect(&recTotalText);
		ScreenToClient(&recTotalText);

		// get the refresh button rectangle
		m_btnRefreshSummary.GetWindowRect(&recRefresh);
		ScreenToClient(&recRefresh);

		// get the last updated label rectangle
		m_staticLastUpdated.GetWindowRect(&recLastUpdated);
		ScreenToClient(&recLastUpdated);

		// get the statistics type groupbox rectangle
		m_staticStatisticsType.GetWindowRect(&recStatisticsType);
		ScreenToClient(&recStatisticsType);

		// get the show NonDeleted file stats button rectangle
		m_btnShowNonDeletedFileStats.GetWindowRect(&recShowNonDeletedFiles);
		ScreenToClient(&recShowNonDeletedFiles);

		// get the show NonDeleted file stats button rectangle
		m_btnShowDeletedFileStats.GetWindowRect(&recShowDeletedFiles);
		ScreenToClient(&recShowDeletedFiles);

		// get the show All stats button rectangle
		m_btnShowAllStats.GetWindowRect(&recShowAll);
		ScreenToClient(&recShowAll);

		// compute margin
		int iMargin = recListCtrl.left - recDlg.left;

		// compute the new statistics type group box position so that it fills the width
		int iHeight = recStatisticsType.Height();
		recStatisticsType.bottom = recDlg.bottom - iMargin;
		recStatisticsType.top = recStatisticsType.bottom - iHeight;
		recStatisticsType.left = recDlg.left + iMargin;
		recStatisticsType.right = recDlg.right - iMargin;

		// compute radio button positions
		recShowAll.MoveToXY(recStatisticsType.left + iMargin, recStatisticsType.top + 20);
		recShowNonDeletedFiles.MoveToXY(recShowAll.right + iMargin, recShowAll.top);
		recShowDeletedFiles.MoveToXY(recShowNonDeletedFiles.right + iMargin, recShowAll.top);

		// compute the new file totals label position
		iHeight = recLabel.Height();
		recLabel.MoveToY(recStatisticsType.top - iHeight - iMargin + 5);

		// compute the new file totals edit box position
		recTotalText.MoveToXY(recLabel.right + iMargin, recLabel.top - 3);

		// compute the new refresh button position
		recRefresh.MoveToXY(recTotalText.right + iMargin, recLabel.top - 5);

		// compute the last updated text position
		recLastUpdated.MoveToXY(recRefresh.right + iMargin, recLabel.top);

		// compute the new list position
		recListCtrl.right = recDlg.right - iMargin;
		recListCtrl.bottom = recRefresh.top - iMargin;

		// move the controls
		m_lblTotals.MoveWindow(&recLabel);
		m_editFileTotal.MoveWindow(&recTotalText);
		m_btnRefreshSummary.MoveWindow(&recRefresh);
		m_listActions.MoveWindow(&recListCtrl);
		m_staticLastUpdated.MoveWindow(&recLastUpdated);
		m_staticStatisticsType.MoveWindow(&recStatisticsType);
		m_btnShowAllStats.MoveWindow(&recShowAll);
		m_btnShowNonDeletedFileStats.MoveWindow(&recShowNonDeletedFiles);
		m_btnShowDeletedFileStats.MoveWindow(&recShowDeletedFiles);

		// resize the columns in the list control
		resizeListColumns();
		
		Invalidate();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19796");
}
//--------------------------------------------------------------------------------------------------
BOOL CFAMDBAdminSummaryDlg::PreTranslateMessage(MSG* pMsg)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// If this is an accelerator key, check for F5 first, otherwise
		// pass the message off to the parent form to handle the keypress
		if ( pMsg->message == WM_KEYDOWN)
		{
			// If the key is the F5 key then refresh the summary grid
			if (pMsg->wParam == VK_F5)
			{
				// Refresh the grid
				populatePage();

				// Need to "Set" the mouse position to clear the wait cursor
				POINT point;
				GetCursorPos(&point);
				SetCursorPos(point.x, point.y);

				// Return TRUE to indicate the message was handled
				return TRUE;
			}

			// Get the parent
			CWnd *pWnd = GetParent();
			if (pWnd)
			{
				// Get the grandparent
				CWnd *pGrandParent = pWnd->GetParent();
				if (pGrandParent)
				{
					// Pass the message on to the grand parent
					return pGrandParent->PreTranslateMessage(pMsg);
				}
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19905")
	
	return CDialog::PreTranslateMessage(pMsg);
}
//--------------------------------------------------------------------------------------------------
void CFAMDBAdminSummaryDlg::OnBnClickedRefreshSummary()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		populatePage();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19797");
}
//--------------------------------------------------------------------------------------------------
void CFAMDBAdminSummaryDlg::OnBnClickedShowDeletedFileStats()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		EWorkflowVisibility eVis = EWorkflowVisibility::All;
		if (m_btnShowDeletedFileStats.GetCheck() == BST_CHECKED)
		{
			eVis = EWorkflowVisibility::Invisible;
		}
		else if (m_btnShowNonDeletedFileStats.GetCheck() == BST_CHECKED)
		{
			eVis = EWorkflowVisibility::Visible;
		}
		if (m_eWorkflowVisibilityMode != eVis)
		{
			m_eWorkflowVisibilityMode = eVis;
			populatePage();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI51617");
}
//--------------------------------------------------------------------------------------------------
void CFAMDBAdminSummaryDlg::OnNMRClickListActions(NMHDR *pNMHDR, LRESULT *pResult)
{
	try
	{
		*pResult = 0;

		// Retrieve information about the active cell.
		LPNMITEMACTIVATE pNMItemActivate = reinterpret_cast<LPNMITEMACTIVATE>(pNMHDR);
		
		CMenu menu;
		CMenu *pContextMenu = __nullptr;

		// If there is a valid selection...
		if (pNMItemActivate->iItem >= 0 && pNMItemActivate->iSubItem >= 0)
		{
			m_strContextMenuAction = m_listActions.GetItemText(pNMItemActivate->iItem, 0);
			string strActionStatus = gmapCOLUMN_STATUS[pNMItemActivate->iSubItem][0];

			if (pNMItemActivate->iSubItem == giUNATTEMPTED_COLUMN &&
				asCppBool(m_ipFAMDB->UsingWorkflows) && 
				m_ipFAMDB->ActiveWorkflow.length() == 0)
			{ 
				return;
			}

			// If there is not an associated action status code, the click occured in the row header or
			// totals column.
			if (strActionStatus.empty())
			{
				if (pNMItemActivate->iSubItem == giACTION_COLUMN)
				{
					menu.LoadMenu(IDR_MENU_SUMMARY_ROW_HEADER);
				}
				else if (pNMItemActivate->iSubItem == giTOTALS_COLUMN)
				{
					m_strContextMenuCount =
						m_listActions.GetItemText(pNMItemActivate->iItem, pNMItemActivate->iSubItem);

					menu.LoadMenu(IDR_MENU_SUMMARY_TOTAL_COLUMN);
				}
				pContextMenu = menu.GetSubMenu(0);
			}
			// If there is an associated action status code, the click corresponds to a specific
			// action status.
			else 
			{
				m_strContextMenuCount =
					m_listActions.GetItemText(pNMItemActivate->iItem, pNMItemActivate->iSubItem);

				// Prepare file selection info based on the selected action and actions status
				EActionStatus esStatus = m_ipFAMDB->AsEActionStatus(strActionStatus.c_str());
				m_ipContextMenuFileSelector->Reset();
				m_ipContextMenuFileSelector->AddActionStatusCondition(m_ipFAMDB,
					m_strContextMenuAction.c_str(), esStatus);

				menu.LoadMenu(IDR_MENU_SUMMARY_CONTEXT);
				pContextMenu = menu.GetSubMenu(0);
			}

			if (pContextMenu != __nullptr)
			{
				string strFailedFileCount =
					(LPCTSTR)m_listActions.GetItemText(pNMItemActivate->iItem, giFAILED_COLUMN);

				// Enable "View exceptions for failed files" only when there are failed files for
				// the action.
				UINT nEnable = (MF_BYCOMMAND | MF_ENABLED);
				UINT nDisable = (MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
				pContextMenu->EnableMenuItem(ID_SUMMARY_MENU_VIEW_FAILED,
					(strFailedFileCount != "0") ? nEnable : nDisable);

				// Get the cursor position
				CPoint point;
				GetCursorPos(&point);

				// Display the context menu
				pContextMenu->TrackPopupMenu(TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_RIGHTBUTTON, 
					point.x, point.y, this);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI31252");
}
//--------------------------------------------------------------------------------------------------
void CFAMDBAdminSummaryDlg::OnContextExportFileList()
{
	try
	{
		// Display the exporting file list dialog
		CExportFileListDlg dlgExportFiles(m_ipFAMDB, m_ipContextMenuFileSelector);
		dlgExportFiles.DoModal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI31253");
}
//--------------------------------------------------------------------------------------------------
void CFAMDBAdminSummaryDlg::OnContextInspectFiles()
{
	try
	{
		if (m_ipFAMFileInspector == __nullptr)
		{
			throw UCLIDException("ELI35801", "FAMFileInspector instance not found.");
		}

		m_ipFAMFileInspector->OpenFAMFileInspector(
			m_ipFAMDB, m_ipContextMenuFileSelector, false, "", __nullptr, 0);
		
		// The OpenFAMFileInspector will use and modify the m_ipContextMenuFileSelector passed in.
		// Since we don't want such changes being reflected in this window, create a new instance to
		// use for the next use of the context menu.
		m_ipContextMenuFileSelector = __nullptr;
		m_ipContextMenuFileSelector.CreateInstance(CLSID_FAMFileSelector);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI35788");
}
//--------------------------------------------------------------------------------------------------
void CFAMDBAdminSummaryDlg::OnContextSetFileActionStatus()
{
	try
	{
		// Display the set file action status dialog
		CFAMDBAdminDlg *pFAMDBAdminDlg = (CFAMDBAdminDlg*)AfxGetMainWnd();
		CSetActionStatusDlg dlgSetActionStatus(m_ipFAMDB, pFAMDBAdminDlg, m_ipContextMenuFileSelector);
		dlgSetActionStatus.DoModal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI31254");
}
//--------------------------------------------------------------------------------------------------
void CFAMDBAdminSummaryDlg::OnContextViewFailed()
{
	try
	{
		CWaitCursor wait;

		// Get the stats for the current action both to trigger a stats update to ensure the most
		// recent failures will be displayed, and also to warn the user if there are too many
		// exceptions to display them all.
		IActionStatisticsPtr ipActionStats;
		string currentWorkflow = m_ipFAMDB->ActiveWorkflow;
		if (currentWorkflow.empty())
		{
			switch (m_eWorkflowVisibilityMode)
			{
			case EWorkflowVisibility::All:
				ipActionStats = m_ipFAMDB->GetStatsAllWorkflows(m_strContextMenuAction.c_str(), VARIANT_TRUE);
				break;
			case EWorkflowVisibility::Visible:
				ipActionStats = m_ipFAMDB->GetVisibleFileStatsAllWorkflows(m_strContextMenuAction.c_str(), VARIANT_TRUE);
				break;
			case EWorkflowVisibility::Invisible:
				ipActionStats = m_ipFAMDB->GetInvisibleFileStatsAllWorkflows(m_strContextMenuAction.c_str(), VARIANT_TRUE);
				break;
			}
		}
		else
		{
			long nActionID = m_ipFAMDB->GetActionID(m_strContextMenuAction.c_str());
			switch (m_eWorkflowVisibilityMode)
			{
			case EWorkflowVisibility::All:
				ipActionStats = m_ipFAMDB->GetStats(nActionID, VARIANT_TRUE);
				break;
			case EWorkflowVisibility::Visible:
				ipActionStats = m_ipFAMDB->GetVisibleFileStats(nActionID, VARIANT_TRUE, VARIANT_FALSE);
				break;
			case EWorkflowVisibility::Invisible:
				ipActionStats = m_ipFAMDB->GetInvisibleFileStats(nActionID, VARIANT_TRUE);
				break;
			}
		}

		long nFailedCount = ipActionStats->GetNumDocumentsFailed();
		if (nFailedCount > 1000)
		{
			int nResult = MessageBox("Since there are a very large number of failures for this "
				"action, only the last 1000 will be displayed.",
				"View exceptions for failed files", MB_OKCANCEL | MB_ICONINFORMATION );
			if (nResult == IDCANCEL)
			{
				return;
			}
		}

		// Build and execute the query to retrieve the exceptions
		string strQuery = gstrFAILED_FILES_EXCEPTIONS_QUERY;
		replaceVariable(strQuery, "<Action>", m_strContextMenuAction);
		long nWorkflowID = m_ipFAMDB->GetWorkflowID(m_ipFAMDB->ActiveWorkflow);
		replaceVariable(strQuery, "<WorkflowID>", asString(nWorkflowID));

		_RecordsetPtr ipRecordSet = m_ipFAMDB->GetResultsForQuery(strQuery.c_str());
		ASSERT_RESOURCE_ALLOCATION("ELI32291", ipRecordSet != __nullptr);

		// The results are ordered newest to oldest. To have the UEX viewer display them in that
		// order, the temp .uex file needs to be in the opposite order. Compile the exceptions
		// in a stack, then output from the stack to reverse the order.
		stack<string> outputLines;
		while (ipRecordSet->adoEOF == VARIANT_FALSE)
		{
			string strException = getStringField(ipRecordSet->Fields, "Exception");
			if (strException.find(',') != string::npos)
			{
				// The full log string was logged; no further info is needed.
				outputLines.push(strException);
			}
			else
			{
				// The exception was not stored with the log string data.
				// We can add the machine, user and timestamp from the DB.
				// (There is not info in the DB regarding serial #, process ID or application)
				string strMachine = getStringField(ipRecordSet->Fields, "MachineName");
				string strUser = getStringField(ipRecordSet->Fields, "UserName");
				CTime timeStamp = getTimeDateField(ipRecordSet->Fields, "DateTimeStamp");
				char pszTime[20];
				sprintf_s(pszTime, sizeof(pszTime), "%lld", static_cast<long long>(timeStamp.GetTime()));

				outputLines.push(
					",," + strMachine + "," + strUser + ",," + string(pszTime) +"," + strException);
			}

			ipRecordSet->MoveNext();
		}

		// Generate the temporary .uex file and open it with the UEX viewer.
		if (outputLines.size() > 0)
		{
			TemporaryFileName tempFile(false, __nullptr, ".uex", false);
			ofstream outputStream(tempFile.getName(), ios::out | ios::trunc);
			if (!outputStream.is_open())
			{
				UCLIDException ue("ELI34227", "Output file could not be opened.");
				ue.addDebugInfo("Filename", tempFile.getName());
				ue.addWin32ErrorInfo();
				throw ue;
			}

			bool firstLine = true;
			while (outputLines.size() > 0)
			{
				if (firstLine)
				{
					firstLine = false;
				}
				else
				{
					outputStream << endl;
				}

				outputStream << outputLines.top();

				outputLines.pop();
			}

			outputStream.close();

			runEXE("UEXViewer.exe", "\"" + tempFile.getName() + "\" /temp");
		}
		else
		{
			MessageBox("There were no exceptions logged for any of the failed files.",
				"No exceptions", MB_OK | MB_ICONINFORMATION);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI32305");
}
//--------------------------------------------------------------------------------------------------
void CFAMDBAdminSummaryDlg::OnContextCopyActionName()
{
	try
	{
		ClipboardManager clipboardManager(this);
		clipboardManager.writeText(m_strContextMenuAction);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37286");
}
//--------------------------------------------------------------------------------------------------
void CFAMDBAdminSummaryDlg::OnContextCopyCount()
{
	try
	{
		ClipboardManager clipboardManager(this);
		clipboardManager.writeText(m_strContextMenuCount);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37596");
}

//--------------------------------------------------------------------------------------------------
// Public methods
//--------------------------------------------------------------------------------------------------
void CFAMDBAdminSummaryDlg::setFAMDatabase(IFileProcessingDBPtr ipFAMDB)
{
	ASSERT_ARGUMENT("ELI19805", ipFAMDB != __nullptr);
	m_ipFAMDB = ipFAMDB;

	// Need to determine the database type being worked with
	IFAMDBUtilsPtr ipFAMDBUtils(CLSID_FAMDBUtils);
	ASSERT_RESOURCE_ALLOCATION("ELI34545", ipFAMDBUtils != __nullptr);

	m_bUseOracleSyntax = asString(ipFAMDBUtils->GetFAMDBProgId()) == 
		"Extract.FileActionManager.Database.FAMDatabaseManager";
}
//--------------------------------------------------------------------------------------------------
void CFAMDBAdminSummaryDlg::setFAMFileInspector(IFAMFileInspectorPtr ipFAMFileInspector)
{
	ASSERT_ARGUMENT("ELI35800", ipFAMFileInspector != __nullptr);
	m_ipFAMFileInspector = ipFAMFileInspector;
}

//--------------------------------------------------------------------------------------------------
// Private helper methods
//--------------------------------------------------------------------------------------------------
void CFAMDBAdminSummaryDlg::prepareListControl()
{
	// insert all of the columns with their associated headers
	// also, insert a temporary width, this will get reset in the OnInitDialog which will
	// call resizeListColumns to size the columns based on the list control's dimensions
	m_listActions.SetExtendedStyle(LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT);
	m_listActions.InsertColumn(giACTION_COLUMN, gmapCOLUMN_STATUS[giACTION_COLUMN][1].c_str(), LVCFMT_LEFT, 50);
	m_listActions.InsertColumn(giUNATTEMPTED_COLUMN, 
		gmapCOLUMN_STATUS[giUNATTEMPTED_COLUMN][1].c_str(), LVCFMT_LEFT, 25);
	m_listActions.InsertColumn(giPENDING_COLUMN, gmapCOLUMN_STATUS[giPENDING_COLUMN][1].c_str(), LVCFMT_LEFT, 25);
	m_listActions.InsertColumn(giPROCESSING_COLUMN, gmapCOLUMN_STATUS[giPROCESSING_COLUMN][1].c_str(), LVCFMT_LEFT, 25);
	m_listActions.InsertColumn(giCOMPLETED_COLUMN, gmapCOLUMN_STATUS[giCOMPLETED_COLUMN][1].c_str(), LVCFMT_LEFT, 25);
	m_listActions.InsertColumn(giSKIPPED_COLUMN, gmapCOLUMN_STATUS[giSKIPPED_COLUMN][1].c_str(), LVCFMT_LEFT, 25);
	m_listActions.InsertColumn(giFAILED_COLUMN, gmapCOLUMN_STATUS[giFAILED_COLUMN][1].c_str(), LVCFMT_LEFT, 25);
	m_listActions.InsertColumn(giTOTALS_COLUMN, gmapCOLUMN_STATUS[giTOTALS_COLUMN][1].c_str(), LVCFMT_LEFT, 25);
}
//--------------------------------------------------------------------------------------------------
void CFAMDBAdminSummaryDlg::resizeListColumns()
{
	// get the rectangle for the list control
	CRect recListCtrl;
	m_listActions.GetWindowRect(&recListCtrl);
	
	// get the width of the list control
	int iWidth = recListCtrl.Width();

	// divide by #columns+1 so that the action column can have extra width
	int iColWidth = iWidth / (giNUMBER_OF_COLUMNS+1);
	if (iColWidth < giMIN_SIZE_COLUMN)
	{
		iColWidth = giMIN_SIZE_COLUMN;
	}

	// set the column widths
	for (int i=0; i < giNUMBER_OF_COLUMNS; i++)
	{
		if (i == giACTION_COLUMN)
		{
			// compute width of action column to be all remaining pixels - 4
			// so that under XP themes the horizontal scroll bar does not appear
			int iActionWidth = (iWidth - (iColWidth * (giNUMBER_OF_COLUMNS-1))) - 4;

			// set the action column width
			m_listActions.SetColumnWidth(giACTION_COLUMN, iActionWidth);
		}
		else
		{
			m_listActions.SetColumnWidth(i, iColWidth);
		}
	}
}
//--------------------------------------------------------------------------------------------------
void CFAMDBAdminSummaryDlg::populatePage(long nActionIDToRefresh /*= -1*/)
{
	try
	{
		// ensure we have a database object
		ASSERT_ARGUMENT("ELI30520", m_ipFAMDB != __nullptr);

		// Set the wait cursor
		CWaitCursor wait;

		// Flag for negative processing number
		bool bNegativeProcessingCalculation = false;

		// Clear the list control if refreshing all actions
		if (nActionIDToRefresh < 0)
		{
			m_listActions.DeleteAllItems();

			if (asString(m_ipFAMDB->GetCurrentConnectionStatus()) != gstrCONNECTION_ESTABLISHED)
			{
				return;
			}
		}
		// Need to refresh all actions if the table has not yet been populated.
		else if (m_listActions.GetItemCount() == 0)
		{
			nActionIDToRefresh = -1;
		}

		long long llFileCount = m_ipFAMDB->GetFileCount(asVariantBool(m_bUseOracleSyntax));

		// get the list of actions from the database
		IStrToStrMapPtr ipMapActions = m_ipFAMDB->GetActions();
		ASSERT_RESOURCE_ALLOCATION("ELI30521", ipMapActions != __nullptr);

		// loop through each action and add the data to the list control
		long lNumActions = ipMapActions->Size;
		for (long i=0; i < lNumActions; i++)
		{
			// Get one action's name and ID inside the database
			_bstr_t bstrKey, bstrValue;
			ipMapActions->GetKeyValue(i, bstrKey.GetAddress(), bstrValue.GetAddress());
			string strActionName = asString(bstrKey);
			long nActionID = asLong(asString(bstrValue));

			if (nActionIDToRefresh >= 0)
			{
				// If refreshing only one action, and this is that action, delete the previous row
				// for that action
				if (nActionIDToRefresh == nActionID)
				{
					m_listActions.DeleteItem(i);
				}
				// Otherwise, there is nothing to do for this action.
				else
				{
					continue;
				}
			}

			// insert the action name into the list control
			int nItem = m_listActions.InsertItem(i, strActionName.c_str());

			IActionStatisticsPtr ipActionStats;
			string currentWorkflow = m_ipFAMDB->ActiveWorkflow;
			if (currentWorkflow.empty())
			{
				switch (m_eWorkflowVisibilityMode)
				{
				case EWorkflowVisibility::All:
					ipActionStats = m_ipFAMDB->GetStatsAllWorkflows(strActionName.c_str(), VARIANT_TRUE);
					break;
				case EWorkflowVisibility::Visible:
					ipActionStats = m_ipFAMDB->GetVisibleFileStatsAllWorkflows(strActionName.c_str(), VARIANT_TRUE);
					break;
				case EWorkflowVisibility::Invisible:
					ipActionStats = m_ipFAMDB->GetInvisibleFileStatsAllWorkflows(strActionName.c_str(), VARIANT_TRUE);
					break;
				}
			}
			else
			{
				switch (m_eWorkflowVisibilityMode)
				{
				case EWorkflowVisibility::All:
					ipActionStats = m_ipFAMDB->GetStats(nActionID, VARIANT_TRUE);
					break;
				case EWorkflowVisibility::Visible:
					ipActionStats = m_ipFAMDB->GetVisibleFileStats(nActionID, VARIANT_TRUE, VARIANT_FALSE);
					break;
				case EWorkflowVisibility::Invisible:
					ipActionStats = m_ipFAMDB->GetInvisibleFileStats(nActionID, VARIANT_TRUE);
					break;
				}
			}

			long lPending, lCompleted, lSkipped, lFailed, lTotal;

			ipActionStats->GetAllStatistics(&lTotal, &lPending, &lCompleted, &lFailed, &lSkipped,
				NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

			long long llFileCount = m_ipFAMDB->GetFileCount(asVariantBool(m_bUseOracleSyntax));
			m_editFileTotal.SetWindowText(commaFormatNumber(llFileCount).c_str());

			// Calculate the processing and unattempted totals
			long lProcessing = lTotal - (lPending + lCompleted + lSkipped + lFailed);
			long lUnattempted = (long) llFileCount - lTotal;

			// Set flag if the calculated processing number is negative
			if (lProcessing < 0)
			{
				bNegativeProcessingCalculation = true;
			}

			// [LegacyRCAndUtils:6106]
			// Since the file count is obtained in a separate call that the rest of the statistics,
			// the FileTotal may include files queued since the time when the rest of the stats
			// were accurate. This can cause a negative number in the unattempted column when
			// queuing. Per discussion with Arvind, to avoid a more complex change late in the
			// release cycle, for now simply change any negative unattempted number to zero
			if (lUnattempted < 0)
			{
				lUnattempted = 0;
			}

			// fill in the grid row
			if (asCppBool(m_ipFAMDB->UsingWorkflows) && m_ipFAMDB->ActiveWorkflow.length() == 0)
			{
				m_listActions.SetItemText(nItem, giUNATTEMPTED_COLUMN, "n/a");
			}
			else
			{
				m_listActions.SetItemText(nItem, giUNATTEMPTED_COLUMN,
					commaFormatNumber((long long)lUnattempted).c_str());
			}
			m_listActions.SetItemText(nItem, giPENDING_COLUMN,
				commaFormatNumber((long long) lPending).c_str());
			m_listActions.SetItemText(nItem, giPROCESSING_COLUMN,
				commaFormatNumber((long long) lProcessing).c_str());
			m_listActions.SetItemText(nItem, giCOMPLETED_COLUMN,
				commaFormatNumber((long long) lCompleted).c_str());
			m_listActions.SetItemText(nItem, giSKIPPED_COLUMN,
				commaFormatNumber((long long) lSkipped).c_str());
			m_listActions.SetItemText(nItem, giFAILED_COLUMN,
				commaFormatNumber((long long) lFailed).c_str());

			// fill in the total column (sum of all files except Unattempted)
			m_listActions.SetItemText(nItem, giTOTALS_COLUMN,
				commaFormatNumber((long long) (lTotal)).c_str());

			// If we are to refresh only this action, there is no point in looping through the rest
			// of the actions.
			if (nActionIDToRefresh >= 0)
			{
				break;
			}
		}

		SYSTEMTIME st;
		GetLocalTime(&st);
		CString cstrMessage;
	    cstrMessage.Format( "Last Updated: %02d/%02d/%d %02d:%02d:%02d", 
							st.wMonth,
							st.wDay,
							st.wYear,
							st.wHour,
							st.wMinute,
							st.wSecond );
		CWnd* pWnd = GetDlgItem(IDC_STATIC_UPDATED);
		pWnd->SetWindowTextA(cstrMessage);

		if (bNegativeProcessingCalculation)
		{
			MessageBox(
			"Summary statistics are in a bad state for at least one action."
			"\r\n\r\n"
			"In order to recalculate the statistics, stop all active processing "
			"and select \"Recalculate summary statistics\" from the \"Tools\" menu.",
			"Invalid summary statistics generated", MB_ICONERROR | MB_OK);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI30527");
}
//--------------------------------------------------------------------------------------------------