// FileProcessingDlgQueueLogPage.cpp : implementation file
//

#include "stdafx.h"
#include "FileProcessingDlgQueueLogPage.h"
#include "UCLIDFileProcessing.h"
#include "HelperFunctions.h"

#include <cpputil.h>
#include <UCLIDException.h>
#include <ExtractMFCUtils.h>
#include <TemporaryResourceOverride.h>
#include <ClipboardManager.h>

// columns common to all 3 grids
static const int giALL_LISTS_DATE_COLUMN = 0;
static const int giALL_LISTS_TIME_COLUMN = 1;
static const int giALL_LISTS_QUEUE_EVENT_COLUMN = 2;
static const int giALL_LISTS_FILENAME_COLUMN = 3;

// columns used in the attempting-to-queue grid
static const int giLIST1_FILE_SUPPLIER_COLUMN = 4;
static const int giLIST1_FILE_PRIORITY_COLUMN = 5;
static const int giLIST1_FOLDER_COLUMN = 6;

// columns in the queue-log grid
static const int giLIST2_FILE_ID_COLUMN = 4;
static const int giLIST2_NUM_PAGES_COLUMN = 5;
static const int giLIST2_ALREADY_EXISTED_COLUMN = 6;
static const int giLIST2_PREVIOUS_STATUS_COLUMN = 7;
static const int giLIST2_COMMENTS_COLUMN = 8;
static const int giLIST2_FILE_SUPPLIER_COLUMN = 9;
static const int giLIST2_FILE_PRIORITY_COLUMN = 10;
static const int giLIST2_FOLDER_COLUMN = 11;

// columns used in the queuing-errors grid
static const int giLIST3_EXCEPTION_COLUMN = 4;
static const int giLIST3_FILE_SUPPLIER_COLUMN = 5;
static const int giLIST3_FILE_PRIORITY_COLUMN = 6;
static const int giLIST3_FOLDER_COLUMN = 7;

// Default widths for the columns
static const int giQUEUE_EVENT_WIDTH = 85;
static const int giALREADY_EXISTED_WIDTH = 90;
static const int giPREVIOUS_STATUS_WIDTH = 90;
static const int giCOMMENTS_WIDTH = 100;
static const int giFILE_SUPPLIER_WIDTH = 100;
static const int giFILE_PRIORITY_WIDTH = 80;

//-------------------------------------------------------------------------------------------------
// FileProcessingDlgQueueLog Property page
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(FileProcessingDlgQueueLogPage, CPropertyPage)
//-------------------------------------------------------------------------------------------------
FileProcessingDlgQueueLogPage::FileProcessingDlgQueueLogPage()
: CPropertyPage(FileProcessingDlgQueueLogPage::IDD),
m_bInitialized(false),
m_bAutoScroll(false)
{
}
//-------------------------------------------------------------------------------------------------
FileProcessingDlgQueueLogPage::~FileProcessingDlgQueueLogPage()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16525");
}
//-------------------------------------------------------------------------------------------------
BOOL FileProcessingDlgQueueLogPage::PreTranslateMessage(MSG* pMsg) 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
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
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15268")

	return CDialog::PreTranslateMessage(pMsg);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgQueueLogPage::DoDataExchange(CDataExchange* pDX)
{
	CPropertyPage::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_LIST_ATTEMPTING_TO_QUEUE, m_listAttemptingToQueue);
	DDX_Control(pDX, IDC_LIST_QUEUE_LOG, m_listQueueLog);
	DDX_Control(pDX, IDC_LIST_FAILED_QUEING, m_listFailedQueing);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(FileProcessingDlgQueueLogPage, CPropertyPage)
	ON_WM_SIZE()
	ON_NOTIFY(NM_DBLCLK, IDC_LIST_FAILED_QUEING, &FileProcessingDlgQueueLogPage::OnNMDblclkFailedFilesList)
	ON_NOTIFY(LVN_ITEMCHANGED, IDC_LIST_FAILED_QUEING, &FileProcessingDlgQueueLogPage::OnLvnItemchangedListFailedQueing)
	ON_NOTIFY(NM_RCLICK, IDC_LIST_ATTEMPTING_TO_QUEUE, &FileProcessingDlgQueueLogPage::OnNMRclkFileLists)
	ON_NOTIFY(NM_RCLICK, IDC_LIST_QUEUE_LOG, &FileProcessingDlgQueueLogPage::OnNMRclkFileLists)
	ON_NOTIFY(NM_RCLICK, IDC_LIST_FAILED_QUEING, &FileProcessingDlgQueueLogPage::OnNMRclkFileLists)
	ON_BN_CLICKED(IDC_BUTTON_QUEUE_EXCEPTION_DETAILS, &FileProcessingDlgQueueLogPage::OnBtnClickedExceptionDetails)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// FileProcessingDlgQueueLogPage message handlers
//-------------------------------------------------------------------------------------------------
BOOL FileProcessingDlgQueueLogPage::OnInitDialog()
{
	CPropertyPage::OnInitDialog();
	
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Set m_bInitialized to true so that 
		// next call to OnSize() will not be skipped
 		m_bInitialized = true;

		// Prepare the top list control
		m_listAttemptingToQueue.SetExtendedStyle( LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT );
		m_listAttemptingToQueue.InsertColumn(giALL_LISTS_DATE_COLUMN , "Date", LVCFMT_LEFT, giDATE_COL_WIDTH );
		m_listAttemptingToQueue.InsertColumn(giALL_LISTS_TIME_COLUMN , "Time", LVCFMT_LEFT, giTIME_COL_WIDTH );
		m_listAttemptingToQueue.InsertColumn(giALL_LISTS_QUEUE_EVENT_COLUMN , "Queue Event", LVCFMT_LEFT, giQUEUE_EVENT_WIDTH );
		m_listAttemptingToQueue.InsertColumn(giALL_LISTS_FILENAME_COLUMN , "Name", LVCFMT_LEFT, giFILENAME_COL_WIDTH );
		m_listAttemptingToQueue.InsertColumn(giLIST1_FILE_SUPPLIER_COLUMN , "File Supplier", LVCFMT_LEFT, giFILE_SUPPLIER_WIDTH  );
		m_listAttemptingToQueue.InsertColumn(giLIST1_FILE_PRIORITY_COLUMN, "Priority", LVCFMT_LEFT, giFILE_PRIORITY_WIDTH);
		m_listAttemptingToQueue.InsertColumn(giLIST1_FOLDER_COLUMN , "Folder", LVCFMT_LEFT, giFOLDER_COL_WIDTH  );

		// Prepare the middle list control
		m_listQueueLog.SetExtendedStyle( LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT );
		m_listQueueLog.InsertColumn(giALL_LISTS_DATE_COLUMN , "Date", LVCFMT_LEFT, giDATE_COL_WIDTH );
		m_listQueueLog.InsertColumn(giALL_LISTS_TIME_COLUMN , "Time", LVCFMT_LEFT, giTIME_COL_WIDTH );
		m_listQueueLog.InsertColumn(giALL_LISTS_QUEUE_EVENT_COLUMN , "Queue Event", LVCFMT_LEFT, giQUEUE_EVENT_WIDTH );
		m_listQueueLog.InsertColumn(giALL_LISTS_FILENAME_COLUMN , "Name", LVCFMT_LEFT, giFILENAME_COL_WIDTH );
		m_listQueueLog.InsertColumn(giLIST2_FILE_ID_COLUMN , "FileID", LVCFMT_LEFT, giFILE_ID_COL_WIDTH );
		m_listQueueLog.InsertColumn(giLIST2_NUM_PAGES_COLUMN , "Pages", LVCFMT_LEFT, giNUM_PAGES_COL_WIDTH );
		m_listQueueLog.InsertColumn(giLIST2_ALREADY_EXISTED_COLUMN , "Already Existed", LVCFMT_LEFT, giALREADY_EXISTED_WIDTH );
		m_listQueueLog.InsertColumn(giLIST2_PREVIOUS_STATUS_COLUMN , "Previous Status", LVCFMT_LEFT, giPREVIOUS_STATUS_WIDTH );
		m_listQueueLog.InsertColumn(giLIST2_COMMENTS_COLUMN, "Comments", LVCFMT_LEFT, giCOMMENTS_WIDTH );
		m_listQueueLog.InsertColumn(giLIST2_FILE_SUPPLIER_COLUMN , "File Supplier", LVCFMT_LEFT, giFILE_SUPPLIER_WIDTH  );
		m_listQueueLog.InsertColumn(giLIST2_FILE_PRIORITY_COLUMN, "Priority", LVCFMT_LEFT, giFILE_PRIORITY_WIDTH);
		m_listQueueLog.InsertColumn(giLIST2_FOLDER_COLUMN , "Folder", LVCFMT_LEFT, giFOLDER_COL_WIDTH  );

		// Prepare the bottom list control
		m_listFailedQueing.SetExtendedStyle( LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT );
		m_listFailedQueing.InsertColumn(giALL_LISTS_DATE_COLUMN , "Date", LVCFMT_LEFT, giDATE_COL_WIDTH );
		m_listFailedQueing.InsertColumn(giALL_LISTS_TIME_COLUMN , "Time", LVCFMT_LEFT, giTIME_COL_WIDTH );
		m_listFailedQueing.InsertColumn(giALL_LISTS_QUEUE_EVENT_COLUMN , "Queue Event", LVCFMT_LEFT, giQUEUE_EVENT_WIDTH );
		m_listFailedQueing.InsertColumn(giALL_LISTS_FILENAME_COLUMN , "Name", LVCFMT_LEFT, giFILENAME_COL_WIDTH );
		m_listFailedQueing.InsertColumn(giLIST3_EXCEPTION_COLUMN , "Error", LVCFMT_LEFT, giEXCEPTION_COL_WIDTH );
		m_listFailedQueing.InsertColumn(giLIST3_FILE_SUPPLIER_COLUMN, "File Supplier", LVCFMT_LEFT, giFILE_SUPPLIER_WIDTH  );
		m_listFailedQueing.InsertColumn(giLIST3_FILE_PRIORITY_COLUMN, "Priority", LVCFMT_LEFT, giFILE_PRIORITY_WIDTH);
		m_listFailedQueing.InsertColumn(giLIST3_FOLDER_COLUMN , "Folder", LVCFMT_LEFT, giFOLDER_COL_WIDTH  );

		// By default, the exception details button is disabled.  It is only 
		// enabled when there is a selected row in the corresponding list control.
		getWindowAndRectInfo(IDC_BUTTON_QUEUE_EXCEPTION_DETAILS, NULL)->EnableWindow(FALSE);
		
		// Verify that the config manager is initialized
		ASSERT_RESOURCE_ALLOCATION( "ELI14278", m_pCfgMgr != __nullptr );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14039")
	
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgQueueLogPage::OnSize(UINT nType, int cx, int cy)
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

		// get size of the exception detail button, then call the
		// reusable helper function to resize the 3 labels and the
		// 3 lists, taking the size of the button into account.  
		// we then call the reposition button function
		// to move the button (repositionButton requires that all
		// other controls be resized and repositioned first.
		CRect rectExceptionDetailsButton;
		getWindowAndRectInfo(IDC_BUTTON_QUEUE_EXCEPTION_DETAILS, &rectExceptionDetailsButton);
		resize3LabelsAndLists(this, m_listAttemptingToQueue, m_listQueueLog, m_listFailedQueing,
			IDC_STATIC_ATTEMPTING_TO_QUEUE, IDC_STATIC_QUEUE_LOG, IDC_STATIC_FAILED_QUEING,
			0, 0, rectExceptionDetailsButton.Height());
		repositionButton(IDC_BUTTON_QUEUE_EXCEPTION_DETAILS, IDC_STATIC_FAILED_QUEING,
			m_listFailedQueing);
		
		// set the folder column of the list control to take up 
		// all the space that's available in the 3 lists
		updateAttemptingToQueueListColumnWidths();
		updateQueueLogColumnWidths();
		updateFailedQueueEventsListColumnWidths();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14040")
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgQueueLogPage::OnBtnClickedExceptionDetails()
{
	try
	{
		// Figure out which one was clicked
		int iPos = getIndexOfFirstSelectedItem(m_listFailedQueing);

		// If no item was selected, do nothing. [LegacyRCAndUtils #4880]
		if (iPos < 0)
		{
			return;
		}
		
		// verify that iPos is a valid index for the exceptions vector 
		if ((unsigned int) iPos >= m_vecExceptions.size())
		{
			UCLIDException ue("ELI16803", "Internal logic error!");
			ue.addDebugInfo("iPos", iPos);
			ue.addDebugInfo("m_vecExceptions.size()", m_vecExceptions.size());
			throw ue;
		}
		
		// Pull up the UEX vector string
		const UCLIDException& ue = m_vecExceptions.at(iPos);
		
		// display the UE (do not log it)
		ue.display(false);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14946")
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgQueueLogPage::OnNMDblclkFailedFilesList(NMHDR *pNMHDR, LRESULT *pResult)
{
	try
	{
		// Do the same as if the user clicked the exception details button
		OnBtnClickedExceptionDetails();

		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI32031");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgQueueLogPage::OnLvnItemchangedListFailedQueing(NMHDR *pNMHDR, LRESULT *pResult)
{
	try
	{
		// Enable the exception details button only if a row is selected in
		// the failed files list
		BOOL bEnable = asMFCBool(m_listFailedQueing.GetFirstSelectedItemPosition() != __nullptr);
		getWindowAndRectInfo(IDC_BUTTON_QUEUE_EXCEPTION_DETAILS, NULL)->EnableWindow(bEnable);
		
		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16808")
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgQueueLogPage::OnNMRclkFileLists(NMHDR* pNMHDR, LRESULT* pResult)
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		ASSERT_ARGUMENT("ELI32065", pNMHDR != __nullptr);

		CListCtrl* pList = __nullptr;
		int iFolderColumn = 0;
		switch(pNMHDR->idFrom)
		{
		case IDC_LIST_ATTEMPTING_TO_QUEUE:
			pList = &m_listAttemptingToQueue;
			iFolderColumn = giLIST1_FOLDER_COLUMN;
			break;
		case IDC_LIST_QUEUE_LOG:
			pList = &m_listQueueLog;
			iFolderColumn = giLIST2_FOLDER_COLUMN;
			break;
		case IDC_LIST_FAILED_QUEING:
			pList = &m_listFailedQueing;
			iFolderColumn = giLIST3_FOLDER_COLUMN;
			break;

		default:
			THROW_LOGIC_ERROR_EXCEPTION("ELI32084");
		}

		vector<string> vecFileNames;
		getFileNamesFromAllSelectedRows(*pList, iFolderColumn, vecFileNames);
		if (vecFileNames.empty())
		{
			return;
		}

		CMenu menu;
		menu.LoadMenu(IDR_MENU_FAM_GRID_CONTEXT);
		CMenu* pContextMenu = menu.GetSubMenu(0);

		size_t nSize = vecFileNames.size();
		UINT nEnable = (MF_BYCOMMAND | MF_ENABLED);
		UINT nDisable = (MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
		pContextMenu->EnableMenuItem(ID_GRID_CONTEXT_COPY_FILENAME, nSize > 0 ? nEnable : nDisable);
		pContextMenu->EnableMenuItem(ID_GRID_CONTEXT_OPEN_FILE, nSize == 1 ? nEnable : nDisable );
		pContextMenu->EnableMenuItem(ID_GRID_CONTEXT_OPEN_FILE_LOCATION, nSize == 1 ? nEnable : nDisable );

		CPoint point;
		GetCursorPos(&point);
		int val = pContextMenu->TrackPopupMenu(
			TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_RIGHTBUTTON | TPM_RETURNCMD,
			point.x, point.y, this);
		switch(val)
		{
		case ID_GRID_CONTEXT_COPY_FILENAME:
			{
				string strFileNames = vecFileNames[0];
				for(size_t i=1; i < vecFileNames.size(); i++)
				{
					strFileNames += "\r\n";
					strFileNames += vecFileNames[i];
				}
				ClipboardManager clippy(this);
				clippy.writeText(strFileNames);
			}
			break;

		case ID_GRID_CONTEXT_OPEN_FILE:
		case ID_GRID_CONTEXT_OPEN_FILE_LOCATION:
			{
				bool bOpenLocation = val == ID_GRID_CONTEXT_OPEN_FILE_LOCATION;
				string strExe;
				getSpecialFolderPath(CSIDL_SYSTEM, strExe);
				strExe += "\\explorer.exe";
				string strFileName = bOpenLocation ?
					getDirectoryFromFullPath(vecFileNames[0]) : vecFileNames[0];
				runEXE(strExe, strFileName);
			}
			break;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI32066");

	if (pResult != __nullptr)
	{
		*pResult = 0;
	}
}

//-------------------------------------------------------------------------------------------------
// Public Methods
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgQueueLogPage::ResetInitialized()
{
	m_bInitialized = false;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgQueueLogPage::setAutoScroll(bool bAutoScroll)
{
	m_bAutoScroll = bAutoScroll;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgQueueLogPage::addEvent(FileSupplyingRecord* pFileSupRec)
{
	try
	{
		// verify arguments
		ASSERT_ARGUMENT("ELI14923", pFileSupRec != __nullptr);

		// we are responsible for deleting the memory associated with pFileSupRec.
		// So, attach it to an auto-pointer.
		unique_ptr<FileSupplyingRecord> apFileSupRec(pFileSupRec);

		// depending upon what type of event was received, call 
		// appropriate methods to handle the event
		switch (apFileSupRec->m_eQueueEventStatus)
		{
		case kQueueEventReceived:
			onQueueEventReceived(pFileSupRec);
			break;

		case kQueueEventHandled:
			onQueueEventHandled(pFileSupRec);
			break;

		case kQueueEventFailed:
			onQueueEventFailed(pFileSupRec);
			break;

		default:
			// we should never reach here
			{
				UCLIDException ue("ELI14921", "Internal logic error!");
				ue.addDebugInfo("EventStatus", (int) apFileSupRec->m_eQueueEventStatus);
				throw ue;
			}
		};

		// TODO
		// EnableVertScrollbar();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14172");	
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgQueueLogPage::onQueueEventReceived(FileSupplyingRecord* pFileSupRec)
{
	// limit the list size
	limitListSizeIfNeeded(m_listAttemptingToQueue, m_pCfgMgr);

	// Insert the new item and populate common columns
	appendNewRecord(m_listAttemptingToQueue, pFileSupRec);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgQueueLogPage::onQueueEventHandled(FileSupplyingRecord* pFileSupRec)
{
	ASSERT_ARGUMENT("ELI27640", pFileSupRec != __nullptr);

	// limit the list size
	limitListSizeIfNeeded(m_listQueueLog, m_pCfgMgr);

	// Insert the new item and populate common columns
	int iNewItemIndex = appendNewRecord(m_listQueueLog, pFileSupRec);

	// update the columns of data specific to this list
	// starting with the file ID column
	if (pFileSupRec->m_ulFileID != 0)
	{
		m_listQueueLog.SetItemText(iNewItemIndex, giLIST2_FILE_ID_COLUMN, 
			asString(pFileSupRec->m_ulFileID).c_str());
	}

	// Update the pages column
	if (pFileSupRec->m_ulNumPages != 0)
	{
		m_listQueueLog.SetItemText(iNewItemIndex, giLIST2_NUM_PAGES_COLUMN, 
			asString(pFileSupRec->m_ulNumPages).c_str());
	}

	// Set the already existed column if the event is a file event (as opposed to a folder event)
	EFileSupplyingRecordType eFSRecordType = pFileSupRec->m_eFSRecordType;
	if (eFSRecordType == kFileAdded || /* eFSRecordType == kFileRemoved || */
		eFSRecordType == kFileRenamed || eFSRecordType == kFileModified)
	{
		string strAlreadyExisted = pFileSupRec->m_bAlreadyExisted ? "Yes" : "No";
		m_listQueueLog.SetItemText(iNewItemIndex, giLIST2_ALREADY_EXISTED_COLUMN, strAlreadyExisted.c_str());
	}

	// update the comments column
	if (eFSRecordType == kFileRenamed)
	{
		CString zNewFileName = getFileNameFromFullPath(pFileSupRec->m_strNewFileName).c_str();
		m_listQueueLog.SetItemText(iNewItemIndex, giLIST2_COMMENTS_COLUMN, "File renamed to " + zNewFileName);
	}
	else if (eFSRecordType == kFolderRenamed)
	{
		CString zNewFolderName = getDirectoryFromFullPath(pFileSupRec->m_strNewFileName).c_str();
		m_listQueueLog.SetItemText(iNewItemIndex, giLIST2_COMMENTS_COLUMN, "Folder Renamed to " + zNewFolderName);
	}

	// Set the previous status column if the event is a file event (as opposed to a folder event) and
	// if the file already existed in the database.
	if (pFileSupRec->m_bAlreadyExisted && (eFSRecordType == kFileAdded || /* eFSRecordType == kFileRemoved || */
		eFSRecordType == kFileRenamed || eFSRecordType == kFileModified))
	{
		m_listQueueLog.SetItemText(iNewItemIndex, giLIST2_PREVIOUS_STATUS_COLUMN, 
			getStatus(pFileSupRec->m_ePreviousActionStatus).c_str());
	}
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgQueueLogPage::onQueueEventFailed(FileSupplyingRecord* pFileSupRec)
{
	ASSERT_ARGUMENT("ELI27641", pFileSupRec != __nullptr);

	// limit the list size.  If a record was deleted, delete the corresponding record
	// from the internal member variable also
	if (limitListSizeIfNeeded(m_listFailedQueing, m_pCfgMgr))
	{
		m_vecExceptions.erase(m_vecExceptions.begin());
	}

	// Insert the new item and populate common columns
	long nNewItemIndex = appendNewRecord(m_listFailedQueing, pFileSupRec);
	m_vecExceptions.push_back(pFileSupRec->m_ueException);

	// update the exception column
	m_listFailedQueing.SetItemText(nNewItemIndex, giLIST3_EXCEPTION_COLUMN, 
		pFileSupRec->m_ueException.getTopText().c_str());
}
//-------------------------------------------------------------------------------------------------
long FileProcessingDlgQueueLogPage::appendNewRecord(CListCtrl& rListCtrl, 
													FileSupplyingRecord* pFileSupRec)
{
	ASSERT_ARGUMENT("ELI27639", pFileSupRec != __nullptr);

	// create a new record in the list control and set the leftmost column to the current date
	long nNewItemIndex = rListCtrl.GetItemCount();
	rListCtrl.InsertItem(nNewItemIndex, getMonthDayDateString().c_str());

	// update the time column
	rListCtrl.SetItemText(nNewItemIndex, giALL_LISTS_TIME_COLUMN, getTimeAsString().c_str());

	// update the event column with the file supplying record type
	rListCtrl.SetItemText(nNewItemIndex, giALL_LISTS_QUEUE_EVENT_COLUMN, 
		getQueueEventString(pFileSupRec->m_eFSRecordType).c_str());

	// update the filename column if the event type is associated with a file
	EFileSupplyingRecordType eFSRecordType = pFileSupRec->m_eFSRecordType;
	if (eFSRecordType == kFileAdded || eFSRecordType == kFileRemoved ||
		eFSRecordType == kFileRenamed || eFSRecordType == kFileModified)
	{
		rListCtrl.SetItemText(nNewItemIndex, giALL_LISTS_FILENAME_COLUMN,  
			getFileNameFromFullPath(pFileSupRec->m_strOriginalFileName).c_str());
	}

	// update the file supplier column
	rListCtrl.SetItemText(nNewItemIndex, getFileSupplierColumnID(rListCtrl), 
		pFileSupRec->m_strFSDescription.c_str());

	// update the file priority column
	rListCtrl.SetItemText(nNewItemIndex, getFilePriorityColumnID(rListCtrl),
		pFileSupRec->m_strPriority.c_str());

	// update the folder column
	string strFolder;
	if (eFSRecordType == kFileAdded || eFSRecordType == kFileRemoved ||
		eFSRecordType == kFileRenamed || eFSRecordType == kFileModified)
	{
		strFolder = getDirectoryFromFullPath(pFileSupRec->m_strOriginalFileName);
	}
	else if (eFSRecordType == kFolderRemoved || eFSRecordType == kFolderRenamed)
	{
		strFolder = pFileSupRec->m_strOriginalFileName;
	}

	// Simplify the path name
	simplifyPathName(strFolder);
	rListCtrl.SetItemText(nNewItemIndex, getFolderColumnID(rListCtrl), strFolder.c_str());

	// If Autoscroll is enabled, focus on the new item
	if (m_bAutoScroll)
	{
		rListCtrl.EnsureVisible(nNewItemIndex, false);
	}

	// return the index of the newly created row
	return nNewItemIndex;
}
//-------------------------------------------------------------------------------------------------
string FileProcessingDlgQueueLogPage::getQueueEventString(EFileSupplyingRecordType eFSRecordType)
{
	switch (eFSRecordType)
	{
		case kNoAction:
			return string("");

		case kFileAdded:
			return string("Add File");

		case kFileRemoved:
			return string("Remove File");

		case kFileRenamed:
			return string("File Renamed");

		case kFileModified:
			return string("File Modified");

		case kFolderRemoved:
			return string("Folder Removed");

		case kFolderRenamed:
			return string("Folder Renamed");
	}

	// we should never reach here
	UCLIDException ue("ELI14924", "Internal logic error!");
	ue.addDebugInfo("RecordType", (int) eFSRecordType);
	throw ue;
}
//-------------------------------------------------------------------------------------------------
long FileProcessingDlgQueueLogPage::getFileSupplierColumnID(CListCtrl& rListCtrl)
{
	if (&rListCtrl == &m_listAttemptingToQueue)
	{
		return giLIST1_FILE_SUPPLIER_COLUMN;
	}
	else if (&rListCtrl == &m_listQueueLog)
	{
		return giLIST2_FILE_SUPPLIER_COLUMN;
	}
	else if (&rListCtrl == &m_listFailedQueing)
	{
		return giLIST3_FILE_SUPPLIER_COLUMN;
	}
	else
	{
		// we should never reach here
		THROW_LOGIC_ERROR_EXCEPTION("ELI14925")
	}
}
//-------------------------------------------------------------------------------------------------
long FileProcessingDlgQueueLogPage::getFolderColumnID(CListCtrl& rListCtrl)
{
	if (&rListCtrl == &m_listAttemptingToQueue)
	{
		return giLIST1_FOLDER_COLUMN;
	}
	else if (&rListCtrl == &m_listQueueLog)
	{
		return giLIST2_FOLDER_COLUMN;
	}
	else if (&rListCtrl == &m_listFailedQueing)
	{
		return giLIST3_FOLDER_COLUMN;
	}
	else
	{
		// we should never reach here
		THROW_LOGIC_ERROR_EXCEPTION("ELI14926")
	}
}
//-------------------------------------------------------------------------------------------------
long FileProcessingDlgQueueLogPage::getFilePriorityColumnID(CListCtrl& rListCtrl)
{
	if (&rListCtrl == &m_listAttemptingToQueue)
	{
		return giLIST1_FILE_PRIORITY_COLUMN;
	}
	else if (&rListCtrl == &m_listQueueLog)
	{
		return giLIST2_FILE_PRIORITY_COLUMN;
	}
	else if (&rListCtrl == &m_listFailedQueing)
	{
		return giLIST3_FILE_PRIORITY_COLUMN;
	}
	else
	{
		// we should never reach here
		THROW_LOGIC_ERROR_EXCEPTION("ELI27638")
	}
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgQueueLogPage::clear()
{
	m_listAttemptingToQueue.DeleteAllItems();
	m_listQueueLog.DeleteAllItems();
	m_listFailedQueing.DeleteAllItems();
	m_vecExceptions.clear();

	// Update the enabled state of the exception details button
	long result = 0;
	OnLvnItemchangedListFailedQueing(__nullptr, &result);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgQueueLogPage::setConfigMgr(FileProcessingConfigMgr *pCfgMgr)
{
	m_pCfgMgr = pCfgMgr;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgQueueLogPage::updateAttemptingToQueueListColumnWidths()
{
	// get the current width of the list
	CRect listRect;
	m_listAttemptingToQueue.GetWindowRect(listRect);
	long nCurrWidth = listRect.Width();

	int scrollwidth = GetSystemMetrics(SM_CXVSCROLL);
	long nLostPixels = 4 + scrollwidth;	

	// This starts with the width of the control and removes each column's width.
	// The end result is used to set the variable width column.
	nCurrWidth -= m_listAttemptingToQueue.GetColumnWidth(giALL_LISTS_DATE_COLUMN);
	nCurrWidth -= m_listAttemptingToQueue.GetColumnWidth(giALL_LISTS_TIME_COLUMN);
	nCurrWidth -= m_listAttemptingToQueue.GetColumnWidth(giALL_LISTS_QUEUE_EVENT_COLUMN);
	nCurrWidth -= m_listAttemptingToQueue.GetColumnWidth(giALL_LISTS_FILENAME_COLUMN);
	nCurrWidth -= m_listAttemptingToQueue.GetColumnWidth(giLIST1_FILE_SUPPLIER_COLUMN);
	nCurrWidth -= m_listAttemptingToQueue.GetColumnWidth(giLIST1_FILE_PRIORITY_COLUMN);

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

	if (!m_listAttemptingToQueue.SetColumnWidth(giLIST1_FOLDER_COLUMN, nCurrWidth))
	{
		UCLIDException ue("ELI14912", "Unable to set column info!");
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgQueueLogPage::updateQueueLogColumnWidths()
{
	// get the current width of the list
	CRect listRect;
	m_listQueueLog.GetWindowRect(listRect);
	long nCurrWidth = listRect.Width();

	int scrollwidth = GetSystemMetrics(SM_CXVSCROLL);
	long nLostPixels = 4 + scrollwidth;

	// This starts with the width of the control and removes each column's width.
	// The end result is used to set the variable width column.
	nCurrWidth -= m_listQueueLog.GetColumnWidth(giALL_LISTS_DATE_COLUMN);
	nCurrWidth -= m_listQueueLog.GetColumnWidth(giALL_LISTS_TIME_COLUMN);
	nCurrWidth -= m_listQueueLog.GetColumnWidth(giALL_LISTS_QUEUE_EVENT_COLUMN);
	nCurrWidth -= m_listQueueLog.GetColumnWidth(giALL_LISTS_FILENAME_COLUMN);
	nCurrWidth -= m_listQueueLog.GetColumnWidth(giLIST2_FILE_ID_COLUMN);
	nCurrWidth -= m_listQueueLog.GetColumnWidth(giLIST2_NUM_PAGES_COLUMN);
	nCurrWidth -= m_listQueueLog.GetColumnWidth(giLIST2_ALREADY_EXISTED_COLUMN);
	nCurrWidth -= m_listQueueLog.GetColumnWidth(giLIST2_PREVIOUS_STATUS_COLUMN);
	nCurrWidth -= m_listQueueLog.GetColumnWidth(giLIST2_COMMENTS_COLUMN);
	nCurrWidth -= m_listQueueLog.GetColumnWidth(giLIST2_FILE_SUPPLIER_COLUMN);
	nCurrWidth -= m_listQueueLog.GetColumnWidth(giLIST2_FILE_PRIORITY_COLUMN);

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

	if (!m_listQueueLog.SetColumnWidth(giLIST2_FOLDER_COLUMN, nCurrWidth))
	{
		UCLIDException ue("ELI14910", "Unable to set column info!");
		ue.addDebugInfo("List:", "Completed List");
		throw ue;
	}
}
//----------------------------------------------------------------------------------------------
void FileProcessingDlgQueueLogPage::updateFailedQueueEventsListColumnWidths()
{
	// get the current width of the list
	CRect listRect;
	m_listFailedQueing.GetWindowRect(listRect);
	long nCurrWidth = listRect.Width();

	int scrollwidth = GetSystemMetrics(SM_CXVSCROLL);
	long nLostPixels = 4 + scrollwidth;

	// This starts with the width of the control and removes each column's width.
	// The end result is used to set the variable width column.
	nCurrWidth -= m_listFailedQueing.GetColumnWidth(giALL_LISTS_DATE_COLUMN);
	nCurrWidth -= m_listFailedQueing.GetColumnWidth(giALL_LISTS_TIME_COLUMN);
	nCurrWidth -= m_listFailedQueing.GetColumnWidth(giALL_LISTS_QUEUE_EVENT_COLUMN);
	nCurrWidth -= m_listFailedQueing.GetColumnWidth(giALL_LISTS_FILENAME_COLUMN);
	nCurrWidth -= m_listFailedQueing.GetColumnWidth(giLIST3_EXCEPTION_COLUMN);
	nCurrWidth -= m_listFailedQueing.GetColumnWidth(giLIST3_FILE_SUPPLIER_COLUMN);
	nCurrWidth -= m_listFailedQueing.GetColumnWidth(giLIST3_FILE_PRIORITY_COLUMN);

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
	
	if (!m_listFailedQueing.SetColumnWidth(giLIST3_FOLDER_COLUMN, nCurrWidth))
	{
		UCLIDException ue("ELI14911", "Unable to set column info!");
		ue.addDebugInfo("List: ", "Failed List");
		throw ue;
	}
}
//----------------------------------------------------------------------------------------------
std::string FileProcessingDlgQueueLogPage::getStatus( UCLID_FILEPROCESSINGLib::EActionStatus eActionStatus )
{
	switch (eActionStatus)
	{
		case kActionUnattempted:
			return "Unattempted";

		case kActionPending:
			return "Pending";

		case kActionProcessing:
			return "Processing";

		case kActionCompleted:
			return "Completed";

		case kActionFailed:
			return "Failed";

		case kActionSkipped:
			return "Skipped";
	}

	// we should never reach here
	UCLIDException ue("ELI14915", "Internal logic error!");
	ue.addDebugInfo("ActionStatus", (int) eActionStatus);
	throw ue;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgQueueLogPage::enableVertScrollBar()
{
	//TODO: update code to work with other two lists also

	//TODO: Maximizing the window displays and enables the scrollbar. Clicking on the bar portion
	//		of the scrollbar twice disables it. Clicking the arrow makes it disappear.
	// get the number of items in the list
 	int iListCount = m_listQueueLog.GetItemCount();

	// Get the number of items that can be displayed by the list's current size
	int iListSize = m_listQueueLog.GetCountPerPage();

	// By comparing the count and size, we can see whether the scrollbar should be enabled or not.
	if( iListCount < iListSize )
	{
		// Disable the arrows on the vertical scrollbar
		m_listQueueLog.EnableScrollBar( SB_VERT, ESB_DISABLE_BOTH );
	}
	else
	{
		// Enable the vertical scrollbar
		m_listQueueLog.EnableScrollBar( SB_VERT, ESB_ENABLE_BOTH );
	}
}
//-------------------------------------------------------------------------------------------------
CWnd* FileProcessingDlgQueueLogPage::getWindowAndRectInfo(UINT uiControlID, CRect *pRect)
{
	// Get the window associated with the specified control
	CWnd *pwndControl = GetDlgItem(uiControlID);
	ASSERT_RESOURCE_ALLOCATION("ELI16801", pwndControl != __nullptr);

	// If the caller wanted the window coordinates of the control, get that information
	if (pRect)
	{
		pwndControl->GetWindowRect(pRect);
	}

	// Return the window pointer
	return pwndControl;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgQueueLogPage::repositionButton(UINT uiButtonID, UINT uiLabelID, 
														  CListCtrl& rListCtrl)
{
	// Get the current coordinates of the button
	CRect rectDetailsButton;
	CWnd *pwndDetailsButton = GetDlgItem(uiButtonID);
	ASSERT_RESOURCE_ALLOCATION("ELI16809", pwndDetailsButton != __nullptr);
	pwndDetailsButton->GetWindowRect(&rectDetailsButton);

	// Get the position of the list control
	CRect rectList;
	rListCtrl.GetWindowRect(&rectList);
	
	// Get the position of the list label
	CRect rectLabel;
	CWnd *pwndLabel = GetDlgItem(uiLabelID);
	ASSERT_RESOURCE_ALLOCATION("ELI16802", pwndLabel != __nullptr);
	pwndLabel->GetWindowRect(&rectLabel);

	// Compute the new position of the details button
	int iWidth = rectDetailsButton.Width();
	int iHeight = rectDetailsButton.Height();
	rectDetailsButton = CRect(rectList.right - iWidth,
		rectLabel.bottom - iHeight, rectList.right, 
		rectLabel.bottom);

	// Move the details button to the newly computed position
	ScreenToClient(rectDetailsButton);
	pwndDetailsButton->MoveWindow(&rectDetailsButton);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgQueueLogPage::getFileNamesFromAllSelectedRows(CListCtrl& rListCtrl,
	int iFolderColumn, vector<string>& rvecFileNames)
{
	rvecFileNames.clear();
	POSITION pos = rListCtrl.GetFirstSelectedItemPosition();
	while (pos != __nullptr)
	{
		int iPos = rListCtrl.GetNextSelectedItem(pos);
		CString zFolderName = rListCtrl.GetItemText(iPos, iFolderColumn);
		CString zFileName = rListCtrl.GetItemText(iPos, giALL_LISTS_FILENAME_COLUMN);
		rvecFileNames.push_back((LPCTSTR)(zFolderName + "\\" + zFileName));
	}
}
//-------------------------------------------------------------------------------------------------
