// FAMDBAdminSummaryDlg.cpp 
#include "StdAfx.h"
#include "FAMDBAdminSummaryDlg.h"
#include "ExportFileListDlg.h"
#include "SetActionStatusDlg.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <ComUtils.h>
#include <ADOUtils.h>

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

// constants for the query to get the total number of files referenced in the database
const string gstrTOTAL_FILECOUNT_FIELD = "FileCount";
const string gstrTOTAL_FAMFILE_QUERY = "SELECT COUNT(ID) as " +
	gstrTOTAL_FILECOUNT_FIELD + " FROM FAMFile";

//--------------------------------------------------------------------------------------------------
// FAMDBAdminSummary dialog
//--------------------------------------------------------------------------------------------------

IMPLEMENT_DYNAMIC(CFAMDBAdminSummaryDlg, CPropertyPage)

//--------------------------------------------------------------------------------------------------
CFAMDBAdminSummaryDlg::CFAMDBAdminSummaryDlg(void) :
CPropertyPage(CFAMDBAdminSummaryDlg::IDD),
m_ipFAMDB(NULL),
m_bInitialized(false)
{
	try
	{
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI19593");
}
//--------------------------------------------------------------------------------------------------
CFAMDBAdminSummaryDlg::~CFAMDBAdminSummaryDlg(void)
{
	try
	{
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
	ON_COMMAND(ID_SUMMARY_MENU_SET_ACTION_STATUS,  &OnContextSetFileActionStatus)
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

		// If there is a valid selection...
		if (pNMItemActivate->iItem >= 0 && pNMItemActivate->iSubItem >= 0)
		{
			// If the selected column has an associated action status code... 
			string strActionStatus = gmapCOLUMN_STATUS[pNMItemActivate->iSubItem][0];
			if (!strActionStatus.empty())
			{
				// Prepare file selection info based on the selected action and actions status
				string strContextActionStatusName = gmapCOLUMN_STATUS[pNMItemActivate->iSubItem][1];
				string strContextActionName = m_listActions.GetItemText(pNMItemActivate->iItem, 0);

				m_contextMenuFileSelection.setScope(eAllFilesForWhich);
				m_contextMenuFileSelection.setStatus(m_ipFAMDB->AsEActionStatus(strActionStatus.c_str()));
				m_contextMenuFileSelection.setStatusString(strContextActionStatusName);		
				m_contextMenuFileSelection.setActionID(m_ipFAMDB->GetActionID(strContextActionName.c_str()));		
				m_contextMenuFileSelection.setAction(strContextActionName);
				m_contextMenuFileSelection.setUser(gstrANY_USER);

				CMenu menu;
				menu.LoadMenu(IDR_MENU_SUMMARY_CONTEXT);
				CMenu *pContextMenu = menu.GetSubMenu(0);
			
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
		CExportFileListDlg dlgExportFiles(m_ipFAMDB, m_contextMenuFileSelection);
		dlgExportFiles.DoModal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI31253");
}
//--------------------------------------------------------------------------------------------------
void CFAMDBAdminSummaryDlg::OnContextSetFileActionStatus()
{
	try
	{
		// Display the set file action status dialog
		CFAMDBAdminDlg *pFAMDBAdminDlg = (CFAMDBAdminDlg*)AfxGetMainWnd();
		CSetActionStatusDlg dlgSetActionStatus(m_ipFAMDB, pFAMDBAdminDlg, m_contextMenuFileSelection);
		dlgSetActionStatus.DoModal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI31254");
}

//--------------------------------------------------------------------------------------------------
// Public methods
//--------------------------------------------------------------------------------------------------
void CFAMDBAdminSummaryDlg::setFAMDatabase(IFileProcessingDBPtr ipFAMDB)
{
	ASSERT_ARGUMENT("ELI19805", ipFAMDB != NULL);
	m_ipFAMDB = ipFAMDB;
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
void CFAMDBAdminSummaryDlg::populatePage()
{
	try
	{
		// ensure we have a database object
		ASSERT_ARGUMENT("ELI30520", m_ipFAMDB != NULL);

		// Set the wait cursor
		CWaitCursor wait;

		// clear the list control first
		m_listActions.DeleteAllItems();

		long long llFileCount = 0;

		// query database for total number of files in the FAMFile table
		_RecordsetPtr ipRecordSet = m_ipFAMDB->GetResultsForQuery(gstrTOTAL_FAMFILE_QUERY.c_str());
		ASSERT_RESOURCE_ALLOCATION("ELI30525", ipRecordSet != NULL);

		// there should only be 1 record returned
		if (ipRecordSet->RecordCount == 1)
		{
			// get the file count
			llFileCount = (long long)getLongField(ipRecordSet->Fields, gstrTOTAL_FILECOUNT_FIELD);

			m_editFileTotal.SetWindowText(commaFormatNumber(llFileCount).c_str());
		}
		else
		{
			THROW_LOGIC_ERROR_EXCEPTION("ELI30526");
		}

		// get the list of actions from the database
		IStrToStrMapPtr ipMapActions = m_ipFAMDB->GetActions();
		ASSERT_RESOURCE_ALLOCATION("ELI30521", ipMapActions != NULL);

		// loop through each action and add the data to the list control
		long lNumActions = ipMapActions->Size;
		for (long i=0; i < lNumActions; i++)
		{
			// Get one action's name and ID inside the database
			_bstr_t bstrKey, bstrValue;
			ipMapActions->GetKeyValue(i, bstrKey.GetAddress(), bstrValue.GetAddress());
			string strActionName = asString(bstrKey);
			long nActionID = asLong(asString(bstrValue));

			// insert the action name into the list control
			int nItem = m_listActions.InsertItem(i, strActionName.c_str());

			IActionStatisticsPtr ipActionStats = m_ipFAMDB->GetStats(nActionID);

			long lPending, lCompleted, lSkipped, lFailed, lTotal;

			ipActionStats->GetAllStatistics(&lTotal, &lPending, &lCompleted, &lFailed, &lSkipped,
				NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

			// Calculate the processing and unattempted totals
			long lProcessing = lTotal - (lPending + lCompleted + lSkipped + lFailed);
			long lUnattempted = (long) llFileCount - lTotal;

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
		}

	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI30527");
}
//--------------------------------------------------------------------------------------------------
