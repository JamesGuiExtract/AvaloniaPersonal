// FAMDBAdminSummaryDlg.cpp 
#include "StdAfx.h"
#include "FAMDBAdminSummaryDlg.h"
#include "ExportFileListDlg.h"
#include "SetActionStatusDlg.h"

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
	"SELECT TOP 1000 MachineName, UserName, DateTimeStamp, Exception FROM FAMFile"
	"	INNER JOIN FileActionStatus ON FAMFile.ID = FileActionStatus.FileID"
	"	INNER JOIN FileActionStateTransition"
	"		ON FileActionStateTransition.ID ="
	"		("
	"			SELECT MAX(ID) FROM FileActionStateTransition"
	"				WHERE FAMFile.ID = FileActionStateTransition.FileID"
	"					AND ActionID = <ActionID>"
	"		)"
	"	INNER JOIN Machine ON FileActionStateTransition.MachineID = Machine.ID"
	"	INNER JOIN FAMUser ON FileActionStateTransition.FAMUserID = FAMUser.ID"
	"	WHERE FileActionStatus.ActionStatus = 'F'"
	"		AND FileActionStatus.ActionID = <ActionID>"
	"		AND FileActionStateTransition.Asc_To = 'F'"
	"		AND FileActionStateTransition.Exception IS NOT NULL"
	"	ORDER BY DateTimeStamp DESC";

const string gstrFAILED_FILES_EXCEPTIONS_QUERY_ORACLE = 
	"SELECT  \"MachineName\", \"UserName\", \"DateTimeStamp\", \"Exception\" FROM"
	"(SELECT  \"MachineName\", \"UserName\", \"DateTimeStamp\", \"Exception\",  "
	"		RANK() OVER (ORDER BY \"DateTimeStamp\" DESC) TimeRank"
	"	FROM \"FAMFile\""
	"	INNER JOIN \"FileActionStatus\" ON \"FAMFile\".\"ID\" = \"FileActionStatus\".\"FileID\""
	"	INNER JOIN \"FileActionStateTransition\""
	"		ON \"FileActionStateTransition\".\"ID\" ="
	"		("
	"			SELECT MAX(\"ID\") FROM \"FileActionStateTransition\""
	"				WHERE \"FAMFile\".\"ID\" = \"FileActionStateTransition\".\"FileID\""
	"					AND \"ActionID\" = <ActionID>"
	"		)"
	"	INNER JOIN \"Machine\" ON \"FileActionStateTransition\".\"MachineID\" = \"Machine\".\"ID\""
	"	INNER JOIN \"FAMUser\" ON \"FileActionStateTransition\".\"FAMUserID\" = \"FAMUser\".\"ID\""
	"	WHERE \"FileActionStatus\".\"ActionStatus\" = 'F'"
	"		AND \"FileActionStatus\".\"ActionID\" = <ActionID>"
	"		AND \"FileActionStateTransition\".\"ASC_To\" = 'F'"
	"		AND \"FileActionStateTransition\".\"Exception\" IS NOT NULL)"
    "  WHERE TimeRank <= 1000"
	"	ORDER BY \"DateTimeStamp\" DESC";


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
m_bUseOracleSyntax(false)
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
}

//--------------------------------------------------------------------------------------------------
// Message Map
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CFAMDBAdminSummaryDlg, CPropertyPage)
	ON_BN_CLICKED(IDC_BUTTON_REFRESH_SUMMARY, &OnBnClickedRefreshSummary)
	ON_WM_SIZE()
	ON_NOTIFY(NM_RCLICK, IDC_LIST_ACTIONS, &CFAMDBAdminSummaryDlg::OnNMRClickListActions)
	ON_COMMAND(ID_SUMMARY_MENU_EXPORT_LIST, &OnContextExportFileList)
	ON_COMMAND(ID_SUMMARY_MENU_SET_ACTION_STATUS, &OnContextSetFileActionStatus)
	ON_COMMAND(ID_SUMMARY_MENU_VIEW_FAILED, &OnContextViewFailed)
	ON_COMMAND(ID_SUMMARY_MENU_INSPECT_FILES, &OnContextInspectFiles)
	ON_COMMAND(ID_SUMMARY_MENU_ROW_HEADER_COPY, &OnContextCopyActionName)
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

		CRect recDlg, recListCtrl, recRefresh, recTotalText, recLabel;

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

		// compute margin
		int iMargin = recListCtrl.left - recDlg.left;

		// compute the new refresh button position
		int iHeight = recRefresh.Height();
		recRefresh.bottom = recDlg.bottom - iMargin;
		recRefresh.top = recRefresh.bottom - iHeight;

		// compute the new file totals edit box position
		iHeight = recTotalText.Height();
		recTotalText.top = recRefresh.top;
		recTotalText.bottom = recTotalText.top + iHeight;

		// compute the new file totals label position
		iHeight = recLabel.Height();
		recLabel.top = recRefresh.top + 2; // the file label is offset by 2 pixels to center it
		recLabel.bottom = recLabel.top + iHeight;

		// compute the new list position
		recListCtrl.right = recDlg.right - iMargin;
		recListCtrl.bottom = recRefresh.top - iMargin;

		// move the controls
		m_lblTotals.MoveWindow(&recLabel);
		m_editFileTotal.MoveWindow(&recTotalText);
		m_btnRefreshSummary.MoveWindow(&recRefresh);
		m_listActions.MoveWindow(&recListCtrl);

		// resize the columns in the list control
		resizeListColumns();
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
void CFAMDBAdminSummaryDlg::OnNMRClickListActions(NMHDR *pNMHDR, LRESULT *pResult)
{
	try
	{
		*pResult = 0;

		// Retrieve information about the active cell.
		LPNMITEMACTIVATE pNMItemActivate = reinterpret_cast<LPNMITEMACTIVATE>(pNMHDR);
		
		CMenu menu;
		CMenu *pContextMenu = __nullptr;
		string strActionStatus;

		// If there is a valid selection...
		if (pNMItemActivate->iItem >= 0 && pNMItemActivate->iSubItem >= 0)
		{

			// If the click occured in the row header...
			if (pNMItemActivate->iSubItem == 0)
			{
				string strContextActionName = m_listActions.GetItemText(pNMItemActivate->iItem, 0);
				m_nContextMenuActionID = m_ipFAMDB->GetActionID(strContextActionName.c_str());

				menu.LoadMenu(IDR_MENU_SUMMARY_ROW_HEADER);
				pContextMenu = menu.GetSubMenu(0);
			}
			// Otherwise, see if the selected column has an associated action status code... 
			else
			{
				strActionStatus = gmapCOLUMN_STATUS[pNMItemActivate->iSubItem][0];
			}

			// If there is an associated action status code... 
			if (!strActionStatus.empty())
			{
				// Prepare file selection info based on the selected action and actions status
				string strContextActionStatusName = gmapCOLUMN_STATUS[pNMItemActivate->iSubItem][1];
				string strContextActionName = m_listActions.GetItemText(pNMItemActivate->iItem, 0);

				m_nContextMenuActionID = m_ipFAMDB->GetActionID(strContextActionName.c_str());
				EActionStatus esStatus = m_ipFAMDB->AsEActionStatus(strActionStatus.c_str());
				m_ipContextMenuFileSelector->Reset();
				m_ipContextMenuFileSelector->AddActionStatusCondition(m_ipFAMDB,
					m_nContextMenuActionID, esStatus);

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
		IActionStatisticsPtr ipActionStats = m_ipFAMDB->GetStats(
			m_nContextMenuActionID, VARIANT_TRUE);

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
		if (m_bUseOracleSyntax)
		{
			strQuery = gstrFAILED_FILES_EXCEPTIONS_QUERY_ORACLE;
		}
		replaceVariable(strQuery, "<ActionID>", asString(m_nContextMenuActionID));

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
				sprintf_s(pszTime, sizeof(pszTime), "%ld", timeStamp.GetTime());

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
		string strActionName = asString(m_ipFAMDB->GetActionName(m_nContextMenuActionID));

		ClipboardManager clipboardManager(this);
		clipboardManager.writeText(strActionName);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37286");
}

//--------------------------------------------------------------------------------------------------
// Public methods
//--------------------------------------------------------------------------------------------------
void CFAMDBAdminSummaryDlg::setFAMDatabase(IFileProcessingDBPtr ipFAMDB)
{
	ASSERT_ARGUMENT("ELI19805", ipFAMDB != __nullptr);
	m_ipFAMDB = ipFAMDB;

	// Need to determine the datatabase type being worked with
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

		// Clear the list control if refreshing all actions
		if (nActionIDToRefresh < 0)
		{
			m_listActions.DeleteAllItems();
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

			IActionStatisticsPtr ipActionStats = m_ipFAMDB->GetStats(nActionID, VARIANT_TRUE);

			long lPending, lCompleted, lSkipped, lFailed, lTotal;

			ipActionStats->GetAllStatistics(&lTotal, &lPending, &lCompleted, &lFailed, &lSkipped,
				NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

			long long llFileCount = m_ipFAMDB->GetFileCount(asVariantBool(m_bUseOracleSyntax));
			m_editFileTotal.SetWindowText(commaFormatNumber(llFileCount).c_str());

			// Calculate the processing and unattempted totals
			long lProcessing = lTotal - (lPending + lCompleted + lSkipped + lFailed);
			long lUnattempted = (long) llFileCount - lTotal;

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
			m_listActions.SetItemText(nItem, giUNATTEMPTED_COLUMN, 
				commaFormatNumber((long long) lUnattempted).c_str());
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
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI30527");
}
//--------------------------------------------------------------------------------------------------
