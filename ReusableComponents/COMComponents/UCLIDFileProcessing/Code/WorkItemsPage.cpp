// WorkItemsPage.cpp : implementation file
//

#include "stdafx.h"
#include "WorkItemsPage.h"
#include "afxdialogex.h"
#include "HelperFunctions.h"

#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>
#include <VectorOperations.h>
#include <ExtractMFCUtils.h>


static const int giALL_LISTS_DATE_COLUMN = 0;
static const int giALL_LISTS_START_TIME_COLUMN = 1;
static const int giALL_LISTS_FILE_ID_COLUMN = 2;
static const int giALL_LISTS_FILE_NAME_COLUMN = 3;

// columns in the currently being processed files grid
static const int giLIST_WORK_ITEM_COLUMN = 4;				// text representing the current work item
static const int giLIST_PROGRESS_STATUS_COLUMN = 5;	// progress status for the current work item
static const int giLIST_FOLDER_COLUMN = 6;

// Set some column widths
static const int giCURRENT_WORK_ITEM_COL_WIDTH = 120;
static const int giCURRENT_TASK_PROGRESS_COL_WIDTH = 120;

static const int giPROGRESS_REFRESH_EVENTID = 1001;
static const int giTIME_BETWEEN_PROGRESS_REFRESHES = 250; // milli-seconds


IMPLEMENT_DYNCREATE(WorkItemsPage, CPropertyPage);

//-------------------------------------------------------------------------------------------------
// WorkItemsPage constructors, destructors
//-------------------------------------------------------------------------------------------------
WorkItemsPage::WorkItemsPage()
	: CPropertyPage(WorkItemsPage::IDD),
	m_bInitialized(false),
	m_bAutoScroll(false)
{

}
//-------------------------------------------------------------------------------------------------
WorkItemsPage::~WorkItemsPage()
{
}

//-------------------------------------------------------------------------------------------------
// Message map
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(WorkItemsPage, CPropertyPage)
	ON_WM_SIZE()
	ON_WM_TIMER()
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
void WorkItemsPage::DoDataExchange(CDataExchange* pDX)
{
	CPropertyPage::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_CURRENT_WORKITEM_LIST, m_currentWorkItemsList);
}

//-------------------------------------------------------------------------------------------------
// WorkItemsPage message handlers
//-------------------------------------------------------------------------------------------------
BOOL WorkItemsPage::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		CPropertyPage::OnInitDialog();

		// Prepare the Files being processed list columns
		m_currentWorkItemsList.SetExtendedStyle(LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT);
		m_currentWorkItemsList.InsertColumn(giALL_LISTS_DATE_COLUMN , "Date", LVCFMT_LEFT, giDATE_COL_WIDTH );
		m_currentWorkItemsList.InsertColumn(giALL_LISTS_START_TIME_COLUMN , "Start Time", LVCFMT_LEFT, giTIME_COL_WIDTH );
		m_currentWorkItemsList.InsertColumn(giALL_LISTS_FILE_ID_COLUMN, "FileID", LVCFMT_LEFT, giFILE_ID_COL_WIDTH);
		m_currentWorkItemsList.InsertColumn(giALL_LISTS_FILE_NAME_COLUMN, "Filename", LVCFMT_LEFT, giFILENAME_COL_WIDTH);
		m_currentWorkItemsList.InsertColumn(giLIST_WORK_ITEM_COLUMN, "Current Work Item", LVCFMT_LEFT, giCURRENT_WORK_ITEM_COL_WIDTH);
		m_currentWorkItemsList.InsertColumn(giLIST_PROGRESS_STATUS_COLUMN, "Work Item Progress", LVCFMT_LEFT, giCURRENT_TASK_PROGRESS_COL_WIDTH);
		m_currentWorkItemsList.InsertColumn(giLIST_FOLDER_COLUMN, "Folder", LVCFMT_LEFT, giFOLDER_COL_WIDTH );

		m_bInitialized = true;
	}
	
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37237");

	return TRUE;
}
//-------------------------------------------------------------------------------------------------
void WorkItemsPage::OnSize(UINT nType, int cx, int cy) 
{
	try
	{
		AFX_MANAGE_STATE(AfxGetModuleState());
		TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

		CPropertyPage::OnSize(nType, cx, cy);

		// first call to this function shall be ignored
		if (!m_bInitialized) 
		{
			return;
		}
		resizeLabelAndList(this, m_currentWorkItemsList, IDC_STATIC_CURR_WORKITEMS);
		updateCurrColWidths();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37243");
}
//-------------------------------------------------------------------------------------------------
void WorkItemsPage::OnTimer(UINT nIDEvent) 
{
	try
	{
		switch(nIDEvent)
		{
		// Handle the progress refresh timer event
		case giPROGRESS_REFRESH_EVENTID:
			// Refresh the progress status displayed directly in the processing log tab
			refreshProgressStatus();
			break;

		// Default timer event handler
		default:
			CPropertyPage::OnTimer(nIDEvent);
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI11636");
}

//-------------------------------------------------------------------------------------------------
// Public methods
//-------------------------------------------------------------------------------------------------
void WorkItemsPage::ResetInitialized()
{
	m_bInitialized = false;
}
//-------------------------------------------------------------------------------------------------
void WorkItemsPage::startProgressUpdates()
{
	// Initialize the progress update timer
	SetTimer(giPROGRESS_REFRESH_EVENTID, giTIME_BETWEEN_PROGRESS_REFRESHES, NULL);
}
//-------------------------------------------------------------------------------------------------
void WorkItemsPage::stopProgressUpdates()
{
	KillTimer(giPROGRESS_REFRESH_EVENTID);
}
//-------------------------------------------------------------------------------------------------
long WorkItemsPage::getCurrentlyProcessingCount()
{
	return m_vecCurrentWorkItemIDs.size();
}
//-------------------------------------------------------------------------------------------------
void WorkItemsPage::onStatusChange(const FPWorkItem *pWorkItem, EWorkItemStatus eOldStatus)
{
	EWorkItemStatus eNewStatus = pWorkItem->m_status;

	if (eOldStatus == kWorkUnitPending && eNewStatus == kWorkUnitProcessing)
	{
		// add to list
		appendNewRecord(m_currentWorkItemsList, *pWorkItem);
	}
	else if (eNewStatus == kWorkUnitComplete ||	eNewStatus == kWorkUnitFailed)
	{
		// remove from list
		removeWorkItemFromCurrentList(pWorkItem->getWorkItemID());
	}
	// not interested in anything else
}
//-------------------------------------------------------------------------------------------------
void WorkItemsPage::setRecordManager(FPRecordManager* pRecordMgr)
{
	m_pRecordManager = pRecordMgr;
}
//-------------------------------------------------------------------------------------------------
void WorkItemsPage::clear()
{
	m_currentWorkItemsList.DeleteAllItems();
	m_vecCurrentWorkItemIDs.clear();	
}
//-------------------------------------------------------------------------------------------------
void WorkItemsPage::setAutoScroll(bool bAutoScroll)
{
	m_bAutoScroll = bAutoScroll;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void WorkItemsPage::updateCurrColWidths()
{
	// get the current width of the list
	CRect listCurrentRect;
	m_currentWorkItemsList.GetWindowRect(listCurrentRect);
	long nCurrWidth = listCurrentRect.Width();

	int scrollwidth = GetSystemMetrics(SM_CXVSCROLL);
	long nLostPixels = 4 + scrollwidth;	

	// This starts with the width of the control and removes each column's width.
	// The end result is used to set the variable width column.
	nCurrWidth -= m_currentWorkItemsList.GetColumnWidth(giALL_LISTS_DATE_COLUMN);
	nCurrWidth -= m_currentWorkItemsList.GetColumnWidth(giALL_LISTS_START_TIME_COLUMN);
	nCurrWidth -= m_currentWorkItemsList.GetColumnWidth(giALL_LISTS_FILE_ID_COLUMN);
	nCurrWidth -= m_currentWorkItemsList.GetColumnWidth(giALL_LISTS_FILE_NAME_COLUMN);
	nCurrWidth -= m_currentWorkItemsList.GetColumnWidth(giLIST_WORK_ITEM_COLUMN);

	if( nCurrWidth < giFOLDER_COL_WIDTH)
	{
		// If left-over size is less than min size, set folder column to min size
		nCurrWidth = giFOLDER_COL_WIDTH - nLostPixels;
	}
	else
	{
		// Otherwise set the folder column to the remaining width
		nCurrWidth = nCurrWidth - nLostPixels;
	}
	if (!m_currentWorkItemsList.SetColumnWidth(giLIST_FOLDER_COLUMN, nCurrWidth))
	{
		UCLIDException ue("ELI37251", "Unable to set column info!");
		ue.addDebugInfo("List:", "Current Work Item List");
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void WorkItemsPage::removeWorkItemFromCurrentList(long nWorkItemID)
{
	CSingleLock lg(&m_mutex, TRUE);
	// Figure out it's location in the currently processing vector
	int iPos = getWorkItemPosFromVector(m_vecCurrentWorkItemIDs, nWorkItemID);
	if (iPos < 0)
	{
		UCLIDException ue("ELI37300", "Work item not found in current list");
		ue.addDebugInfo("WorkItemID", nWorkItemID);

		// This is not a critical error so just log it - it shouldn't happen and if it does 
		// there is nothing the user can do about it
		ue.log();
		return;
	}
	// Remove from the currently processing list
	m_currentWorkItemsList.DeleteItem(iPos);
	eraseFromVector(m_vecCurrentWorkItemIDs, nWorkItemID);	
}
//-------------------------------------------------------------------------------------------------
int WorkItemsPage::getWorkItemPosFromVector(const vector<long>& vecToSearch, long nWorkItemID)
{
	CSingleLock lg(&m_mutex, TRUE);
	int vecSize = vecToSearch.size();
	for( int i = 0; i < vecSize; i++)
	{
		if( vecToSearch.at(i) == nWorkItemID )
			return i;
	}
	return -1;
}
//-------------------------------------------------------------------------------------------------
long WorkItemsPage::appendNewRecord(CListCtrl& rListCtrl, 
									const FPWorkItem& workItem)
{
	CSingleLock lg(&m_mutex, TRUE);

	// insert a new record in the list, and update the leftmost column
	// with the date
	int iNewItemIndex = rListCtrl.InsertItem(rListCtrl.GetItemCount(),
		getMonthDayDateString().c_str());

	// Add the name data to the file name map
	long workItemID = workItem.getWorkItemID();
	string strFileName = workItem.getFileName();
	
	// Get the start time.
	CTime startTime;
	if (&rListCtrl == &m_currentWorkItemsList)
	{
		startTime = CTime::GetCurrentTime();
	}
	else
	{
		startTime = workItem.getStartTime();
	}

	// update the time column
	CString zStartTime = startTime.Format("%#H:%M:%S");
	rListCtrl.SetItemText(iNewItemIndex, giALL_LISTS_START_TIME_COLUMN , zStartTime);

	// update the ID column
	rListCtrl.SetItemText(iNewItemIndex, giALL_LISTS_FILE_ID_COLUMN, 
		asString(workItem.getFileID()).c_str());

	// update the filename column
	rListCtrl.SetItemText(iNewItemIndex, giALL_LISTS_FILE_NAME_COLUMN, 
		getFileNameFromFullPath(strFileName).c_str());

	rListCtrl.SetItemText(iNewItemIndex, giLIST_FOLDER_COLUMN,
		getDirectoryFromFullPath(strFileName).c_str());

	// If auto scroll is enabled, ensure that the new item is visible
	if (m_bAutoScroll)
	{
		rListCtrl.EnsureVisible(iNewItemIndex, false);
	}

	// update the associated data in other vectors, depending upon 
	// which list control we are appending data to
	if (&rListCtrl == &m_currentWorkItemsList)
	{
		m_vecCurrentWorkItemIDs.push_back(workItemID);
	}
	else
	{
		// we should never reach here
		THROW_LOGIC_ERROR_EXCEPTION("ELI37265")
	}

	// return the new item's index
	return iNewItemIndex;
}
//-------------------------------------------------------------------------------------------------
void WorkItemsPage::refreshProgressStatus()
{
	try
	{
		CSingleLock lg(&m_mutex, TRUE);

		// Iterate through all the current tasks, and update their progress information
		vector<long>::const_iterator iter;
		for (iter = m_vecCurrentWorkItemIDs.begin(); iter != m_vecCurrentWorkItemIDs.end(); iter++)
		{
			// Get the Work item ID
			long nWorkItemID = *iter;

			// Get the work item's progress status object
			IProgressStatusPtr ipTopLevelProgressStatus = m_pRecordManager->getWorkItemProgressStatus(nWorkItemID);
			
			// Ensure that the progress status object exists. It may just have been created but not 
			// initialized, or it may have just completed and reset. If it doesn't exist, just ignore
			// this refresh progress request and wait for the next one.
			if (ipTopLevelProgressStatus)
			{
				// Update the description of the current work item executing on the file
				string strTaskDescription = asString(ipTopLevelProgressStatus->Text);
				int iPos = getWorkItemPosFromVector(m_vecCurrentWorkItemIDs, nWorkItemID);
				setItemTextIfDifferent(m_currentWorkItemsList, iPos, giLIST_WORK_ITEM_COLUMN, strTaskDescription);

				// Get the progress status information for the current work item executing on 
				// the file
				IProgressStatusPtr ipCurrentTaskProgress = ipTopLevelProgressStatus;
				
				// Ensure that the progress status object exists. It may just have been created but 
				// not initialized, or it may have just completed and reset. If it doesn't exist, just ignore
				// this refresh progress request and wait for the next one.
				if (ipCurrentTaskProgress)
				{
					// Update the progress information for the current work item
					const int iNUM_DECIMALS_IN_PERCENT_COMPLETE = 2;
					double dPercentComplete = (100 * ipCurrentTaskProgress->GetProgressPercent());
					string strProgressText = asString(dPercentComplete, iNUM_DECIMALS_IN_PERCENT_COMPLETE) + "%";
					setItemTextIfDifferent(m_currentWorkItemsList, iPos, giLIST_PROGRESS_STATUS_COLUMN, strProgressText);
				}
			}
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI37276")
}
//-------------------------------------------------------------------------------------------------
