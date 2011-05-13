// FileProcessingDlgStatusPage.cpp : implementation file
//

#include "stdafx.h"
#include "resource.h"
#include "FilePriorityHelper.h"
#include "FileProcessingDlgStatusPage.h"
#include "TaskInfoDlg.h"
#include "HelperFunctions.h"

#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>
#include <StopWatch.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <ExtractMFCUtils.h>
#include <ClipboardManager.h>
#include <VectorOperations.h>

#include <string>
#include <vector>
#include <list>
using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// columns common to all grids
static const int giALL_LISTS_DATE_COLUMN = 0;
static const int giALL_LISTS_START_TIME_COLUMN = 1;
static const int giALL_LISTS_FILE_ID_COLUMN = 2;
static const int giALL_LISTS_FILE_NAME_COLUMN = 3;
static const int giALL_LISTS_NUM_PAGES_COLUMN = 4;

// columns in the currently being processed files grid
static const int giLIST1_TASK_COLUMN = 5;				// text representing the current task
static const int giLIST1_PROGRESS_STATUS_COLUMN = 6;	// progress status for the current task
static const int giLIST1_FILE_PRIORITY_COLUMN = 7;
static const int giLIST1_FOLDER_COLUMN = 8;

// Column for the completed processing recently and failed grids
static const int giLIST2_TOTAL_TIME_COLUMN = 5;
static const int giLIST2_FILE_PRIORITY_COLUMN = 6;
static const int giLIST2_FOLDER_COLUMN = 7;

// Column for the failed processing recently grid
static const int giLIST3_TOTAL_TIME_COLUMN = 5;
static const int giLIST3_EXCEPTION_COLUMN = 6;
static const int giLIST3_FILE_PRIORITY_COLUMN = 7;
static const int giLIST3_FOLDER_COLUMN = 8;

// Set some column widths
static const int giCURRENT_TASK_COL_WIDTH = 120;
static const int giCURRENT_TASK_PROGRESS_COL_WIDTH = 120;
static const int giTOTAL_TIME_COL_WIDTH = 80;
static const int giFILE_PRIORITY_WIDTH = 80;

// Other constants
static const int giPROGRESS_REFRESH_EVENTID = 1000;
static const int giTIME_BETWEEN_PROGRESS_REFRESHES = 250; // milli-seconds

static const char* gzPROGRESS_STATUS_DIALOG_PROG_ID =
	"Extract.Utilties.Forms.ProgressStatusDialog";

//-------------------------------------------------------------------------------------------------
// FileProcessingDlgStatusPage property page
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(FileProcessingDlgStatusPage, CPropertyPage)
//-------------------------------------------------------------------------------------------------
FileProcessingDlgStatusPage::FileProcessingDlgStatusPage() 
: 
CPropertyPage(FileProcessingDlgStatusPage::IDD),
m_bAutoScroll(false),
m_bInitialized(false),
m_nTotalBytesProcessed(0),
m_nTotalDocumentsProcessed(0),
m_nTotalPagesProcessed(0)
{
	//{{AFX_DATA_INIT(FileProcessingDlgStatusPage)
	//}}AFX_DATA_INIT
}
//-------------------------------------------------------------------------------------------------
FileProcessingDlgStatusPage::~FileProcessingDlgStatusPage()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20393");
}
//-------------------------------------------------------------------------------------------------
BOOL FileProcessingDlgStatusPage::PreTranslateMessage(MSG* pMsg) 
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15271")

	return CDialog::PreTranslateMessage(pMsg);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgStatusPage::DoDataExchange(CDataExchange* pDX)
{
	CPropertyPage::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_CURRENT_FILES_LIST, m_currentFilesList);
	DDX_Control(pDX, IDC_COMPLETE_FILES_LIST, m_completedFilesList);
	DDX_Control(pDX, IDC_FAILED_FILES_LIST, m_failedFilesList);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(FileProcessingDlgStatusPage, CPropertyPage)
	ON_WM_SIZE()
	ON_WM_TIMER()
	ON_NOTIFY(NM_DBLCLK, IDC_FAILED_FILES_LIST, &FileProcessingDlgStatusPage::OnNMDblclkFailedFilesList)
	ON_NOTIFY(NM_DBLCLK, IDC_CURRENT_FILES_LIST, &FileProcessingDlgStatusPage::OnNMDblclkCurrentFilesList)
	ON_NOTIFY(NM_RCLICK, IDC_CURRENT_FILES_LIST, &FileProcessingDlgStatusPage::OnNMRclkFileLists)
	ON_NOTIFY(NM_RCLICK, IDC_COMPLETE_FILES_LIST, &FileProcessingDlgStatusPage::OnNMRclkFileLists)
	ON_NOTIFY(NM_RCLICK, IDC_FAILED_FILES_LIST, &FileProcessingDlgStatusPage::OnNMRclkFileLists)
	ON_NOTIFY(LVN_ITEMCHANGED, IDC_FAILED_FILES_LIST, &FileProcessingDlgStatusPage::OnItemchangedFailedFilesList)
	ON_BN_CLICKED(IDC_BUTTON_PROGRESS_DETAILS, &FileProcessingDlgStatusPage::OnBtnClickedProgressDetails)
	ON_BN_CLICKED(IDC_BUTTON_EXCEPTION_DETAILS, &FileProcessingDlgStatusPage::OnBtnClickedExceptionDetails)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// Public methods
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgStatusPage::clear()
{
	// Clear lists
	m_currentFilesList.DeleteAllItems();
	m_completedFilesList.DeleteAllItems();
	m_failedFilesList.DeleteAllItems();

	// Clear vectors and the file id map
	m_vecCurrFileIds.clear();
	m_vecCompFileIds.clear();
	m_vecFailFileIds.clear();
	m_vecFailedUEXCodes.clear();
	m_mapFileIdToFileName.clear();


	// Clear the processing time
	m_completedFailedOrCurrentTime.clear();

	// Clear the processed amounts
	m_nTotalBytesProcessed = 0;
	m_nTotalPagesProcessed = 0;
	m_nTotalDocumentsProcessed = 0;

	// Update the enabled state of the exception details button
	long result = 0;
	OnItemchangedFailedFilesList(__nullptr, &result);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgStatusPage::onStatusChange(long nFileId, ERecordStatus eOldStatus, ERecordStatus eNewStatus)
{
	// by default, assume that the transition is valid
	bool bUnexpectedTransition = false;

	// based upon what the old status and the new status is, update the UI
	switch (eOldStatus)
	{
	case kRecordNone:
		switch (eNewStatus)
		{
		case kRecordPending:
		case kRecordNone:
		case kRecordProcessingError:
			// valid transitions but nothing to be done.
			break;

		default:
			// from none, we only expect to transition to pending.
			// all other transitions are invalid
			bUnexpectedTransition = true;
		};
		break;

	case kRecordPending:
		switch (eNewStatus)
		{
		case kRecordPending:
		case kRecordNone:
		case kRecordProcessingError:
			// valid transitions but nothing to be done.
			break;

		case kRecordCurrent:
			moveTaskFromPendingToCurrent(nFileId);
			break;

		default:
			// from pending, we only expect to transition to current.
			// all other transitions are invalid
			bUnexpectedTransition = true;
		};
		break;

	case kRecordCurrent:
		switch (eNewStatus)
		{
		case kRecordCurrent:
			break;

		case kRecordComplete:
			moveTaskFromCurrentToComplete(nFileId);
			break;

		case kRecordFailed:
		case kRecordProcessingError:
			moveTaskFromCurrentToFailed(nFileId);
			break;

		case kRecordPending:
		case kRecordSkipped:
		case kRecordNone:
			removeTaskFromCurrentList(nFileId);
			break;

		default:
			// from current, we only expect to transition to
			// complete or failed (besides from current-to-current in
			// which case the current task was updated)
			// all other transitions are invalid.
			bUnexpectedTransition = true;
		};
		break;

	case kRecordComplete:
		switch(eNewStatus)
		{
		case kRecordNone:
			// This changes is sent by the FPRecordManager when it removes old items from its completed queue
			break;
		case kRecordPending:
			// don't need to do anything because the the completed record should stay in completed list and 
			// when it is completed again there will be another record put in the completed list
			break;
		
		case kRecordProcessingError:
			// Nothing to do 
			break;

		default:
			// From complete we can expect to transition to pending
			// if the file is resupplied with the force processing option
			bUnexpectedTransition = true;
			break;
		}
		break;

	case kRecordFailed:
		switch(eNewStatus)
		{
		case kRecordNone:
			// This changes is sent by the FPRecordManager when it removes old items from its failed queue
			break;

		case kRecordPending:
			// don't need to do anything because the the failed record should stay in completed list and 
			// when it is failed again there will be another record put in the failed or completed list
			break;

		case kRecordProcessingError:
			break;
		
		default:
			// from Failed we can expect to transition to pending
			// if the file is resupplied with the force processing option
			bUnexpectedTransition = true;
			break;
		}
		break;

	case kRecordSkipped:
		switch(eNewStatus)
		{
		case kRecordNone:
			// This change is sent by the FPRecordManager, nothing to do here
			break;

		case kRecordPending:
			// don't need to do anything since there is no list of skipped files
			break;

		case kRecordProcessingError:
			break;

		default:
			// From Skipped we can expect transition to pending
			// if the file is resupplied with the force processing option
			bUnexpectedTransition = true;
			break;
		}
		break;

	default:
		// the previous state is some unknown state
		bUnexpectedTransition = true;
	};

	if (bUnexpectedTransition)
	{
		UCLIDException ue("ELI14916", "Internal logic error!");
		ue.addDebugInfo("OldStatus", FPRecordManager::statusAsString(eOldStatus));
		ue.addDebugInfo("NewStatus", FPRecordManager::statusAsString(eNewStatus));
		ue.addDebugInfo("FileID", nFileId);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgStatusPage::setRecordManager(FPRecordManager* pRecordMgr)
{
	ASSERT_ARGUMENT("ELI10174", pRecordMgr != __nullptr);
	m_pRecordMgr = pRecordMgr;
}

//-------------------------------------------------------------------------------------------------
// FileProcessingDlgStatusPage message handlers
//-------------------------------------------------------------------------------------------------
BOOL FileProcessingDlgStatusPage::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		CPropertyPage::OnInitDialog();

		// Prepare the Files being processed list columns
		m_currentFilesList.SetExtendedStyle(LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT);
		m_currentFilesList.InsertColumn(giALL_LISTS_DATE_COLUMN , "Date", LVCFMT_LEFT, giDATE_COL_WIDTH );
		m_currentFilesList.InsertColumn(giALL_LISTS_START_TIME_COLUMN , "Start Time", LVCFMT_LEFT, giTIME_COL_WIDTH );
		m_currentFilesList.InsertColumn(giALL_LISTS_FILE_ID_COLUMN, "FileID", LVCFMT_LEFT, giFILE_ID_COL_WIDTH);
		m_currentFilesList.InsertColumn(giALL_LISTS_FILE_NAME_COLUMN, "Filename", LVCFMT_LEFT, giFILENAME_COL_WIDTH);
		m_currentFilesList.InsertColumn(giALL_LISTS_NUM_PAGES_COLUMN , "Pages", LVCFMT_LEFT, giNUM_PAGES_COL_WIDTH );
		m_currentFilesList.InsertColumn(giLIST1_TASK_COLUMN, "Current Task", LVCFMT_LEFT, giCURRENT_TASK_COL_WIDTH);
		m_currentFilesList.InsertColumn(giLIST1_PROGRESS_STATUS_COLUMN, "Current Task Progress", LVCFMT_LEFT, giCURRENT_TASK_PROGRESS_COL_WIDTH);
		m_currentFilesList.InsertColumn(giLIST1_FILE_PRIORITY_COLUMN, "Priority", LVCFMT_LEFT, giFILE_PRIORITY_WIDTH);
		m_currentFilesList.InsertColumn(giLIST1_FOLDER_COLUMN, "Folder", LVCFMT_LEFT, giFOLDER_COL_WIDTH );

		// Prepare the Files that completed recently columns
		m_completedFilesList.SetExtendedStyle(LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT);
		m_completedFilesList.InsertColumn(giALL_LISTS_DATE_COLUMN , "Date", LVCFMT_LEFT, giDATE_COL_WIDTH );
		m_completedFilesList.InsertColumn(giALL_LISTS_START_TIME_COLUMN , "Start Time", LVCFMT_LEFT, giTIME_COL_WIDTH );
		m_completedFilesList.InsertColumn(giALL_LISTS_FILE_ID_COLUMN, "FileID", LVCFMT_LEFT, giFILE_ID_COL_WIDTH);
		m_completedFilesList.InsertColumn(giALL_LISTS_FILE_NAME_COLUMN, "Filename", LVCFMT_LEFT, giFILENAME_COL_WIDTH);
		m_completedFilesList.InsertColumn(giALL_LISTS_NUM_PAGES_COLUMN , "Pages", LVCFMT_LEFT, giNUM_PAGES_COL_WIDTH );
		m_completedFilesList.InsertColumn(giLIST2_TOTAL_TIME_COLUMN , "Total Time (s)", LVCFMT_LEFT, giTOTAL_TIME_COL_WIDTH );
		m_completedFilesList.InsertColumn(giLIST2_FILE_PRIORITY_COLUMN, "Priority", LVCFMT_LEFT, giFILE_PRIORITY_WIDTH);
		m_completedFilesList.InsertColumn(giLIST2_FOLDER_COLUMN, "Folder", LVCFMT_LEFT, giFOLDER_COL_WIDTH);

		// Prepare the Files that failed recently columns
		m_failedFilesList.SetExtendedStyle(LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT);
		m_failedFilesList.InsertColumn(giALL_LISTS_DATE_COLUMN , "Date", LVCFMT_LEFT, giDATE_COL_WIDTH );
		m_failedFilesList.InsertColumn(giALL_LISTS_START_TIME_COLUMN , "Start Time", LVCFMT_LEFT, giTIME_COL_WIDTH );
		m_failedFilesList.InsertColumn(giALL_LISTS_FILE_ID_COLUMN, "FileID", LVCFMT_LEFT, giFILE_ID_COL_WIDTH);
		m_failedFilesList.InsertColumn(giALL_LISTS_FILE_NAME_COLUMN, "Filename", LVCFMT_LEFT, giFILENAME_COL_WIDTH);
		m_failedFilesList.InsertColumn(giALL_LISTS_NUM_PAGES_COLUMN , "Pages", LVCFMT_LEFT, giNUM_PAGES_COL_WIDTH );
		m_failedFilesList.InsertColumn(giLIST3_TOTAL_TIME_COLUMN , "Total Time (s)", LVCFMT_LEFT, giTOTAL_TIME_COL_WIDTH );
		m_failedFilesList.InsertColumn(giLIST3_EXCEPTION_COLUMN, "Error", LVCFMT_LEFT, giEXCEPTION_COL_WIDTH);
		m_failedFilesList.InsertColumn(giLIST3_FILE_PRIORITY_COLUMN, "Priority", LVCFMT_LEFT, giFILE_PRIORITY_WIDTH);
		m_failedFilesList.InsertColumn(giLIST3_FOLDER_COLUMN, "Folder", LVCFMT_LEFT, giFOLDER_COL_WIDTH);

		// By default, the exception details button is disabled.  It is only 
		// enabled when there is a selected row in the corresponding list control.
		getWindowAndRectInfo(IDC_BUTTON_EXCEPTION_DETAILS, NULL)->EnableWindow(FALSE);

		// Set m_bInitialized to true so that 
		// next call to OnSize() will not be skipped
		m_bInitialized = true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09026")
	
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgStatusPage::OnSize(UINT nType, int cx, int cy) 
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

		// Get the current coordinates of the progress details button
		CRect rectProgressDetailsButton;
		getWindowAndRectInfo(IDC_BUTTON_PROGRESS_DETAILS, &rectProgressDetailsButton);

		// Get the current coordinates of the exception details button
		CRect rectExceptionDetailsButton;
		getWindowAndRectInfo(IDC_BUTTON_EXCEPTION_DETAILS, &rectExceptionDetailsButton);

		// resize the 3 lists and associated labels using the reusable
		// helper function
		resize3LabelsAndLists(this, m_currentFilesList, m_completedFilesList, 
			m_failedFilesList, IDC_STATIC_CURR_PROC, IDC_STATIC_COMP_PROC, 
			IDC_STATIC_FAIL_PROC, rectProgressDetailsButton.Height(), 0, 
			rectExceptionDetailsButton.Height());

		// Reposition the progress details button
		repositionDetailsButton(IDC_BUTTON_PROGRESS_DETAILS, IDC_STATIC_CURR_PROC,
			m_currentFilesList);

		// Reposition the exception details button
		repositionDetailsButton(IDC_BUTTON_EXCEPTION_DETAILS, IDC_STATIC_FAIL_PROC,
			m_failedFilesList);
		
		// set the folder column of the list control to take up 
		// all the space that's available in the 3 lists
		updateCurrColWidths();
		updateCompColWidths();
		updateFailColWidths();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11115")
}
//----------------------------------------------------------------------------------------------
void FileProcessingDlgStatusPage::OnTimer(UINT nIDEvent) 
{
	try
	{
		switch(nIDEvent)
		{
		// Handle the progress refresh timer event
		case giPROGRESS_REFRESH_EVENTID:
			// Refresh the progress status data in the progress details dialog
			updateProgressDetailsDlg();

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
void FileProcessingDlgStatusPage::OnBtnClickedProgressDetails()
{
	try
	{
		// If the progress details dialog has not yet been created, create the object
		if (m_ipProgressStatusDialog == __nullptr)
		{
			m_ipProgressStatusDialog.CreateInstance(gzPROGRESS_STATUS_DIALOG_PROG_ID);
			ASSERT_RESOURCE_ALLOCATION("ELI16257", m_ipProgressStatusDialog != __nullptr);
		}

		// Show the progress details as a modeless dialog
		const long nNUM_PROGRESS_LEVELS = 3; // TODO: in the future make this a registy setting
		const long nDELAY_BETWEEN_PROGRESS_REFRESHES = 100; // In milliseconds; TODO: in the future make this a registry setting

		HWND hParent = NULL;
		CWnd *pWnd = AfxGetMainWnd();
		if (pWnd)
		{
			hParent = pWnd->GetSafeHwnd();
		}

		m_ipProgressStatusDialog->ShowModelessDialog(hParent, "", NULL, 
			nNUM_PROGRESS_LEVELS, nDELAY_BETWEEN_PROGRESS_REFRESHES, VARIANT_TRUE, NULL);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16603")
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgStatusPage::OnBtnClickedExceptionDetails()
{
	try
	{
		// Figure out which one was clicked
		int iPos = getIndexOfFirstSelectedItem(m_failedFilesList);
		
		// If no item was selected, do nothing. [LegacyRCAndUtils #4879]
		if (iPos < 0)
		{
			return;
		}

		// verify that iPos is a valid index for the exceptions vector 
		if ((unsigned int) iPos >= m_vecFailedUEXCodes.size())
		{
			UCLIDException ue("ELI14930", "Internal logic error!");
			ue.addDebugInfo("iPos", iPos);
			ue.addDebugInfo("m_vecFailedUEXCodes.size()", m_vecFailedUEXCodes.size());
			throw ue;
		}

		// Pull up the UEX vector string
		std::string strUEXCode = m_vecFailedUEXCodes.at(iPos);

		// Translate it into a UE
		UCLIDException ue;
		ue.createFromString("ELI14109", strUEXCode);

		// display the UE (do not log it)
		ue.display(false);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14947")
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgStatusPage::OnNMDblclkFailedFilesList(NMHDR *pNMHDR, LRESULT *pResult)
{
	try
	{
		// Do the same as if the user clicked the exception details button
		OnBtnClickedExceptionDetails();

		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI32032");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgStatusPage::OnNMDblclkCurrentFilesList(NMHDR *pNMHDR, LRESULT *pResult)
{
	try
	{
		// Do the same as if the user clicked the progress details button
		OnBtnClickedProgressDetails();

		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16596")
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgStatusPage::OnNMRclkFileLists(NMHDR* pNMHDR, LRESULT* pResult)
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		ASSERT_ARGUMENT("ELI32033", pNMHDR != __nullptr);

		CListCtrl* pList = __nullptr;
		vector<long>* pvecListItems = __nullptr;
		switch(pNMHDR->idFrom)
		{
		case IDC_CURRENT_FILES_LIST:
			pList = &m_currentFilesList;
			pvecListItems = &m_vecCurrFileIds;
			break;
		case IDC_COMPLETE_FILES_LIST:
			pList = &m_completedFilesList;
			pvecListItems = &m_vecCompFileIds;
			break;
		case IDC_FAILED_FILES_LIST:
			pList = &m_failedFilesList;
			pvecListItems = &m_vecFailFileIds;
			break;

		default:
			THROW_LOGIC_ERROR_EXCEPTION("ELI32083");
		}

		vector<int> vecSelected = getIndexOfAllSelectedItems(*pList);
		if (vecSelected.empty())
		{
			return;
		}

		vector<string> vecFileNames;
		for(vector<int>::iterator it = vecSelected.begin(); it != vecSelected.end(); it++)
		{
			// Get the file names for each of the file id's
			long nFileId = pvecListItems->at(*it);
			string fileName = getFileNameForFileId(nFileId);
			if (!fileName.empty())
			{
				vecFileNames.push_back(fileName);
			}
		}

		CMenu menu;
		menu.LoadMenu(IDR_MENU_FAM_GRID_CONTEXT);
		CMenu* pContextMenu = menu.GetSubMenu(0);

		// enable/disable context menu items properly
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
				for(size_t i=1; i < nSize; i++)
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

				// LRCAU #6042 - explorer.exe appears to be in different locations sometimes
				// depending on the install. Since it is hard to imagine Windows operating
				// without explorer.exe being in the path, we should be able to just run
				// explorer.exe without specifying the full path to it.
				string strFileName = bOpenLocation ?
					getDirectoryFromFullPath(vecFileNames[0]) : vecFileNames[0];
				runEXE("explorer.exe", strFileName);
			}
			break;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI32036");

	if (pResult != __nullptr)
	{
		*pResult = 0;
	}
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgStatusPage::OnItemchangedFailedFilesList(NMHDR* pNMHDR, LRESULT* pResult) 
{
	try
	{
		// Enable the exception details button only if a row is selected in
		// the failed files list
		BOOL bEnable = asMFCBool(m_failedFilesList.GetFirstSelectedItemPosition() != __nullptr);
		getWindowAndRectInfo(IDC_BUTTON_EXCEPTION_DETAILS, NULL)->EnableWindow(bEnable);
		
		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16620")
}

//-------------------------------------------------------------------------------------------------
// Public Methods
//-------------------------------------------------------------------------------------------------
long FileProcessingDlgStatusPage::getCurrentlyProcessingCount()
{
	return static_cast<long>( m_currentFilesList.GetItemCount() );
}
//-------------------------------------------------------------------------------------------------
unsigned long FileProcessingDlgStatusPage::getTotalProcTime()
{
	return 	m_completedFailedOrCurrentTime.getTotalSeconds();
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgStatusPage::getLocalStats( LONGLONG& nTotalBytes, long& nTotalDocs, long& nTotalPages )
{
	try
	{
		nTotalBytes = m_nTotalBytesProcessed;
		nTotalDocs = m_nTotalDocumentsProcessed;
		nTotalPages = m_nTotalPagesProcessed;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS( "ELI14291" );
	return;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgStatusPage::ResetInitialized()
{
	m_bInitialized = false;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgStatusPage::setConfigMgr(FileProcessingConfigMgr* pCfgMgr)
{
	m_pCfgMgr = pCfgMgr;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgStatusPage::setAutoScroll(bool bAutoScroll)
{
	m_bAutoScroll = bAutoScroll;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgStatusPage::updateProgressDetailsDlg()
{
	// There's nothing to do if the progress details dialog is not
	// being displayed
	if (m_ipProgressStatusDialog == __nullptr)
	{
		return;
	}

	// Get the selected record in the currently processing files list
	static int iPreviouslySelectedIndex = -1;
	long nFileID = -1;
	int iPos = getIndexOfFirstSelectedItem(m_currentFilesList);
	int iNumItemsInCurrentFilesList = m_currentFilesList.GetItemCount();
	if (iPos != -1)
	{
		// Get the file ID associated with the currently selected record
		nFileID = asLong((LPCTSTR) m_currentFilesList.GetItemText(iPos, giALL_LISTS_FILE_ID_COLUMN));
		iPreviouslySelectedIndex = iPos;
	}
	else if (iNumItemsInCurrentFilesList > 0)
	{
		int iIndexToSelect = 0; // By default, select the first entry

		// If there is no selection in the current files list, we would prefer to
		// select the index position that was last selected, assuming that
		// that index position is still a valid index
		if (iPreviouslySelectedIndex >= 0 && iPreviouslySelectedIndex < iNumItemsInCurrentFilesList)
		{
			iIndexToSelect = iPreviouslySelectedIndex;
		}
		else if (iPreviouslySelectedIndex >= 0)
		{
			// If there is no selection right now, but there exists a
			// previously selected index, restore the selection to the last
			// entry in the list as that is the next most intuitive choice
			// (as opposed to selecting the first entry)
			iIndexToSelect = iNumItemsInCurrentFilesList - 1;
		}

		// Select the row in the current files list at the computed index.
		m_currentFilesList.SetItemState(iIndexToSelect, LVIS_SELECTED, LVIS_SELECTED);
		
		// Get the FileID associated with the currently selected row in the current files list
		nFileID = asLong((LPCTSTR) m_currentFilesList.GetItemText(iIndexToSelect, giALL_LISTS_FILE_ID_COLUMN));
	}
	else
	{
		// There is no progress status to show
		// By leaving nFileID unchanged, the code below will pass an empty
		// string and NULL progress status object to the ProgressStatusDialog
		// and cause the dialog to show some "no progress information available"
		// type of default message.
	}

	// Determine the top level progress status object that should be 
	// displayed in the detailed progress status window, and the window title
	// for the detailed progress status window.
	IProgressStatusPtr ipProgressStatus = __nullptr;
	string strTitle;
	if (nFileID != -1)
	{
		// Get the task object associated with the selected record
		const FileProcessingRecord& task = m_pRecordMgr->getTask(nFileID);
		
		// Get the progress status object to be displayed
		ipProgressStatus = task.m_ipProgressStatus;

		// Compute the title of the progress status window
		strTitle = "Status for ";
		strTitle += getFileNameFromFullPath(task.getFileName());
		strTitle += " (ID=";
		strTitle += asString(nFileID);
		strTitle += ")";
	}

	// Set the top level progress status object in the progress status window
	m_ipProgressStatusDialog->ProgressStatusObject = ipProgressStatus;

	// Set the title of the progress status window
	m_ipProgressStatusDialog->Title = strTitle.c_str();
}
//-------------------------------------------------------------------------------------------------
CWnd* FileProcessingDlgStatusPage::getWindowAndRectInfo(UINT uiControlID, CRect *pRect)
{
	// Get the window associated with the specified control
	CWnd *pwndControl = GetDlgItem(uiControlID);
	ASSERT_RESOURCE_ALLOCATION("ELI16571", pwndControl != __nullptr);

	// If the caller wanted the window coordinates of the control, get that information
	if (pRect)
	{
		pwndControl->GetWindowRect(pRect);
	}

	// Return the window pointer
	return pwndControl;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgStatusPage::repositionDetailsButton(UINT uiButtonID, UINT uiLabelID, 
														  CListCtrl& rListCtrl)
{
	// Get the current coordinates of the button
	CRect rectDetailsButton;
	CWnd *pwndDetailsButton = GetDlgItem(uiButtonID);
	ASSERT_RESOURCE_ALLOCATION("ELI16607", pwndDetailsButton != __nullptr);
	pwndDetailsButton->GetWindowRect(&rectDetailsButton);

	// Get the position of the list control
	CRect rectList;
	rListCtrl.GetWindowRect(&rectList);
	
	// Get the position of the list label
	CRect rectLabel;
	CWnd *pwndLabel = GetDlgItem(uiLabelID);
	ASSERT_RESOURCE_ALLOCATION("ELI16608", pwndLabel != __nullptr);
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
void FileProcessingDlgStatusPage::refreshProgressStatus()
{
	try
	{
		// Iterate through all the current tasks, and update their progress information
		vector<long>::const_iterator iter;
		for (iter = m_vecCurrFileIds.begin(); iter != m_vecCurrFileIds.end(); iter++)
		{
			// Get the file ID
			long nFileID = *iter;

			// Get the task object associated with the ID
			const FileProcessingRecord& fileRecord = m_pRecordMgr->getTask(nFileID);

			// Get the task's progress status object, to retrieve the text description
			// of the current task
			IProgressStatusPtr ipTopLevelProgressStatus = fileRecord.m_ipProgressStatus;
			
			// Ensure that the progress status object exists. It may just have been created but not 
			// initialized, or it may have just completed and reset. If it doesn't exist, just ignore
			// this refresh progress request and wait for the next one.
			if (ipTopLevelProgressStatus)
			{
				// Update the description of the current task executing on the file
				string strTaskDescription = asString(ipTopLevelProgressStatus->Text);
				int iPos = getTaskPosFromVector(m_vecCurrFileIds, nFileID);
				setItemTextIfDifferent(m_currentFilesList, iPos, giLIST1_TASK_COLUMN, strTaskDescription);

				// Get the progress status information for the current task executing on 
				// the file
				IProgressStatusPtr ipCurrentTaskProgress = ipTopLevelProgressStatus->SubProgressStatus;
				
				// Ensure that the sub progress status object exists. It may just have been created but 
				// not initialized, or it may have just completed and reset. If it doesn't exist, just ignore
				// this refresh progress request and wait for the next one.
				if (ipCurrentTaskProgress)
				{
					// Update the progress information for the current task
					const int iNUM_DECIMALS_IN_PERCENT_COMPLETE = 2;
					double dPercentComplete = (100 * ipCurrentTaskProgress->GetProgressPercent());
					string strProgressText = asString(dPercentComplete, iNUM_DECIMALS_IN_PERCENT_COMPLETE) + "%";
					setItemTextIfDifferent(m_currentFilesList, iPos, giLIST1_PROGRESS_STATUS_COLUMN, strProgressText);
				}
			}
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16077")
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgStatusPage::limitListSizeAndUpdateVars(CListCtrl& rListCtrl)
{
	// if we did not need to remove an entry from this list, just exit
	if (!limitListSizeIfNeeded(rListCtrl, m_pCfgMgr))
	{
		return;
	}

	long lId = 0;

	// delete the associated data in other vectors, depending upon 
	// which list control we are deleting data from
	if (&rListCtrl == &m_currentFilesList)
	{
		auto it = m_vecCurrFileIds.begin();
		lId = *it;
		m_vecCurrFileIds.erase(it);
	}
	else if (&rListCtrl == &m_completedFilesList)
	{
		auto it = m_vecCompFileIds.begin();
		lId = *it;
		m_vecCompFileIds.erase(it);
	}
	else if (&rListCtrl == &m_failedFilesList)
	{
		auto it = m_vecFailFileIds.begin();
		lId = *it;
		m_vecFailFileIds.erase(it);
		m_vecFailedUEXCodes.erase(m_vecFailedUEXCodes.begin());
	}
	else
	{
		// we should never reach here
		THROW_LOGIC_ERROR_EXCEPTION("ELI14919")
	}

	removeFileNameForFileId(lId);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgStatusPage::moveTaskFromPendingToCurrent(long nFileID)
{
	// Get the task based on the ID
	const FileProcessingRecord& task = m_pRecordMgr->getTask(nFileID);

	// append a new record and update the default columns
	int iNewItemIndex = appendNewRecord(m_currentFilesList, task);
	
	// update the folder column
	m_currentFilesList.SetItemText(iNewItemIndex, giLIST1_FOLDER_COLUMN, 
		getDirectoryFromFullPath(task.getFileName()).c_str());

	// update the priority column
	m_currentFilesList.SetItemText(iNewItemIndex, giLIST1_FILE_PRIORITY_COLUMN,
		getPriorityString(task.getPriority()).c_str());
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgStatusPage::moveTaskFromCurrentToComplete(long nFileID)
{
	// remove task from the current list
	removeTaskFromCurrentList(nFileID);

	// Get the task based on the ID
	const FileProcessingRecord& task = m_pRecordMgr->getTask(nFileID);

	// limit the size of the completed list as needed
	limitListSizeAndUpdateVars(m_completedFilesList);

	// append a new record and update the default columns
	int iNewItemIndex = appendNewRecord(m_completedFilesList, task);
		
	// update the total time column
	m_completedFilesList.SetItemText(iNewItemIndex, giLIST2_TOTAL_TIME_COLUMN, 
		commaFormatNumber(task.getTaskDuration(), 2).c_str());

	// update the priority column
	m_completedFilesList.SetItemText(iNewItemIndex, giLIST2_FILE_PRIORITY_COLUMN,
		getPriorityString(task.getPriority()).c_str());

	// update the folder column
	m_completedFilesList.SetItemText(iNewItemIndex, giLIST2_FOLDER_COLUMN, 
		getDirectoryFromFullPath(task.getFileName()).c_str());

	// update internal stats
	updateStatisticsVariablesWithTaskInfo(nFileID);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgStatusPage::moveTaskFromCurrentToFailed(long nFileID)
{
	// remove task from the current list
	removeTaskFromCurrentList(nFileID);

	// Get the task based on the ID
	const FileProcessingRecord& task = m_pRecordMgr->getTask(nFileID);

	// limit the size of the completed list as needed
	limitListSizeAndUpdateVars(m_failedFilesList);

	// append a new record and update the default columns
	int iNewItemIndex = appendNewRecord(m_failedFilesList, task);
	
	// update the total time column
	m_failedFilesList.SetItemText(iNewItemIndex, giLIST3_TOTAL_TIME_COLUMN, 
		commaFormatNumber(task.getTaskDuration(), 2).c_str());

	// update the exception text column
	UCLIDException ue;
	ue.createFromString("ELI14108", task.m_strException);
	m_failedFilesList.SetItemText(iNewItemIndex, giLIST3_EXCEPTION_COLUMN , ue.getTopText().c_str());

	// update the priority column
	m_failedFilesList.SetItemText(iNewItemIndex, giLIST3_FILE_PRIORITY_COLUMN,
		getPriorityString(task.getPriority()).c_str());

	// update the folder column
	m_failedFilesList.SetItemText(iNewItemIndex, giLIST3_FOLDER_COLUMN, 
		getDirectoryFromFullPath(task.getFileName()).c_str());

	// Check for exception while executing error task
	if (!task.m_strErrorTaskException.empty())
	{
		// append a new record and update the default columns
		iNewItemIndex = appendNewRecord(m_failedFilesList, task, true);

		// update the error task time
		m_failedFilesList.SetItemText(iNewItemIndex, giLIST3_TOTAL_TIME_COLUMN, 
			commaFormatNumber(task.getErrorTaskDuration(), 2).c_str());

		// update the exception text column
		UCLIDException ue;
		ue.createFromString("ELI18015", task.m_strErrorTaskException);
		m_failedFilesList.SetItemText(iNewItemIndex, giLIST3_EXCEPTION_COLUMN , ue.getTopText().c_str());

		// update the priority column
		m_failedFilesList.SetItemText(iNewItemIndex, giLIST3_FILE_PRIORITY_COLUMN,
			getPriorityString(task.getPriority()).c_str());

		// update the folder column
		m_failedFilesList.SetItemText(iNewItemIndex, giLIST3_FOLDER_COLUMN, 
			getDirectoryFromFullPath(task.getFileName()).c_str());
	}

	// update internal stats
	updateStatisticsVariablesWithTaskInfo(nFileID);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgStatusPage::removeTaskFromCurrentList(long nFileID)
{
	// Figure out it's location in the currently processing vector
	int iPos = getTaskPosFromVector(m_vecCurrFileIds, nFileID);

	// Remove from the currently processing list
	m_currentFilesList.DeleteItem(iPos);
	eraseFromVector(m_vecCurrFileIds, nFileID);	
}
//-------------------------------------------------------------------------------------------------
long FileProcessingDlgStatusPage::appendNewRecord(CListCtrl& rListCtrl, 
												  const FileProcessingRecord& task,
												  bool bErrorTaskEntry/* = false*/)
{
	// insert a new record in the list, and update the leftmost column
	// with the date
	int iNewItemIndex = rListCtrl.InsertItem(rListCtrl.GetItemCount(),
		getMonthDayDateString().c_str());

	// Add the name data to the file name map
	long fileId = task.getFileID();
	string strFileName = task.getFileName();
	addFileNameToFileIdMap(fileId, strFileName);

	// Get the start time.
	// for the current-files list, the time is the current time.
	// for the other two lists, the time is the start time associated with
	// the task object.  For failed error tasks, the start time is assosiated
	// with the time the error task began execution
	CTime startTime;
	if (&rListCtrl == &m_currentFilesList)
	{
		startTime = CTime::GetCurrentTime();
	}
	else if (bErrorTaskEntry)
	{
		startTime = task.getErrorTaskStartTime();
	}
	else
	{
		startTime = task.getStartTime();
	}

	// update the time column
	CString zStartTime = startTime.Format("%#H:%M:%S");
	rListCtrl.SetItemText(iNewItemIndex, giALL_LISTS_START_TIME_COLUMN , zStartTime);

	// update the ID column
	rListCtrl.SetItemText(iNewItemIndex, giALL_LISTS_FILE_ID_COLUMN, 
		asString(fileId).c_str());

	// update the filename column
	rListCtrl.SetItemText(iNewItemIndex, giALL_LISTS_FILE_NAME_COLUMN, 
		getFileNameFromFullPath(strFileName).c_str());

	// update the pages column
	rListCtrl.SetItemText(iNewItemIndex, giALL_LISTS_NUM_PAGES_COLUMN, 
		asString(task.getNumberOfPages()).c_str());

	// If auto scroll is enabled, ensure that the new item is visible
	if (m_bAutoScroll)
	{
		rListCtrl.EnsureVisible(iNewItemIndex, false);
	}

	// update the associated data in other vectors, depending upon 
	// which list control we are appending data to
	if (&rListCtrl == &m_currentFilesList)
	{
		m_vecCurrFileIds.push_back(fileId);
	}
	else if (&rListCtrl == &m_completedFilesList)
	{
		m_vecCompFileIds.push_back(fileId);
	}
	else if (&rListCtrl == &m_failedFilesList)
	{
		m_vecFailFileIds.push_back(fileId);
		
		if (bErrorTaskEntry)
		{
			// Push back error task exception
			m_vecFailedUEXCodes.push_back(task.m_strErrorTaskException);
		}
		else
		{
			// Push back task exception
			m_vecFailedUEXCodes.push_back(task.m_strException);
		}
	}
	else
	{
		// we should never reach here
		THROW_LOGIC_ERROR_EXCEPTION("ELI14920")
	}

	// return the new item's index
	return iNewItemIndex;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgStatusPage::updateStatisticsVariablesWithTaskInfo(long nFileID)
{
	// Get the task based on the ID
	const FileProcessingRecord& task = m_pRecordMgr->getTask(nFileID);

	// Use the try-catch because getStartTime() for the task can throw a UE if 
	// the stopwatch is somehow reset.
	try
	{
		// Make an interval for this task's processing time
		TimeInterval interval(task.getStartTime(), CTime::GetCurrentTime());

		// Add the processing time of this task to the total processing time
		// The merge prevents overlaps from being counted twice.
		m_completedFailedOrCurrentTime.merge( interval );

		// Add the data to the totals
		m_nTotalBytesProcessed += task.getFileSize();
		m_nTotalPagesProcessed += task.getNumberOfPages();
		m_nTotalDocumentsProcessed++;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS( "ELI14297" );
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgStatusPage::updateFailColWidths()
{
	// get the current width of the list
	CRect listFailedRect;
	m_failedFilesList.GetWindowRect(listFailedRect);
	long nCurrWidth = listFailedRect.Width();

	int scrollwidth = GetSystemMetrics(SM_CXVSCROLL);
	long nLostPixels = 4 + scrollwidth;

	// This starts with the width of the control and removes each column's width.
	// The end result is used to set the variable width column.
	nCurrWidth -= m_failedFilesList.GetColumnWidth(giALL_LISTS_DATE_COLUMN);
	nCurrWidth -= m_failedFilesList.GetColumnWidth(giALL_LISTS_START_TIME_COLUMN);
	nCurrWidth -= m_failedFilesList.GetColumnWidth(giALL_LISTS_FILE_ID_COLUMN);
	nCurrWidth -= m_failedFilesList.GetColumnWidth(giALL_LISTS_FILE_NAME_COLUMN);
	nCurrWidth -= m_failedFilesList.GetColumnWidth(giALL_LISTS_NUM_PAGES_COLUMN);
	nCurrWidth -= m_failedFilesList.GetColumnWidth(giLIST3_TOTAL_TIME_COLUMN);
	nCurrWidth -= m_failedFilesList.GetColumnWidth(giLIST3_EXCEPTION_COLUMN);

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
	
	if (!m_failedFilesList.SetColumnWidth(giLIST3_FOLDER_COLUMN, nCurrWidth))
	{
		UCLIDException ue("ELI11114", "Unable to set column info!");
		ue.addDebugInfo("List: ", "Failed List");
		throw ue;
	}
}
//----------------------------------------------------------------------------------------------
void FileProcessingDlgStatusPage::updateCompColWidths()
{
	// get the current width of the list
	CRect listCompleteRect;
	m_completedFilesList.GetWindowRect(listCompleteRect);
	long nCurrWidth = listCompleteRect.Width();

	int scrollwidth = GetSystemMetrics(SM_CXVSCROLL);
	long nLostPixels = 4 + scrollwidth;

	// This starts with the width of the control and removes each column's width.
	// The end result is used to set the variable width column.
	nCurrWidth -= m_completedFilesList.GetColumnWidth(giALL_LISTS_DATE_COLUMN);
	nCurrWidth -= m_completedFilesList.GetColumnWidth(giALL_LISTS_START_TIME_COLUMN);
	nCurrWidth -= m_completedFilesList.GetColumnWidth(giALL_LISTS_FILE_ID_COLUMN);
	nCurrWidth -= m_completedFilesList.GetColumnWidth(giALL_LISTS_FILE_NAME_COLUMN);
	nCurrWidth -= m_completedFilesList.GetColumnWidth(giALL_LISTS_NUM_PAGES_COLUMN);
	nCurrWidth -= m_completedFilesList.GetColumnWidth(giLIST2_TOTAL_TIME_COLUMN);

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
	if (!m_completedFilesList.SetColumnWidth(giLIST2_FOLDER_COLUMN, nCurrWidth))
	{
		UCLIDException ue("ELI14116", "Unable to set column info!");
		ue.addDebugInfo("List:", "Completed List");
		throw ue;
	}
}
//----------------------------------------------------------------------------------------------
void FileProcessingDlgStatusPage::updateCurrColWidths()
{
	// get the current width of the list
	CRect listCurrentRect;
	m_currentFilesList.GetWindowRect(listCurrentRect);
	long nCurrWidth = listCurrentRect.Width();

	int scrollwidth = GetSystemMetrics(SM_CXVSCROLL);
	long nLostPixels = 4 + scrollwidth;	

	// This starts with the width of the control and removes each column's width.
	// The end result is used to set the variable width column.
	nCurrWidth -= m_currentFilesList.GetColumnWidth(giALL_LISTS_DATE_COLUMN);
	nCurrWidth -= m_currentFilesList.GetColumnWidth(giALL_LISTS_START_TIME_COLUMN);
	nCurrWidth -= m_currentFilesList.GetColumnWidth(giALL_LISTS_FILE_ID_COLUMN);
	nCurrWidth -= m_currentFilesList.GetColumnWidth(giALL_LISTS_FILE_NAME_COLUMN);
	nCurrWidth -= m_currentFilesList.GetColumnWidth(giALL_LISTS_NUM_PAGES_COLUMN);
	nCurrWidth -= m_currentFilesList.GetColumnWidth(giLIST1_TASK_COLUMN);

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
	if (!m_currentFilesList.SetColumnWidth(giLIST1_FOLDER_COLUMN, nCurrWidth))
	{
		UCLIDException ue("ELI14117", "Unable to set column info!");
		ue.addDebugInfo("List:", "Current List");
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
int FileProcessingDlgStatusPage::getTaskPosFromVector(const std::vector<long>& vecToSearch, long nFileID)
{
	int vecSize = vecToSearch.size();
	for( int i = 0; i < vecSize; i++)
	{
		if( vecToSearch.at(i) == nFileID )
			return i;
	}
	return -1;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgStatusPage::startProgressUpdates()
{
	// Initialize the progress update timer
	SetTimer(giPROGRESS_REFRESH_EVENTID, giTIME_BETWEEN_PROGRESS_REFRESHES, NULL);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgStatusPage::stopProgressUpdates()
{
	KillTimer(giPROGRESS_REFRESH_EVENTID);

	// If the progress dialog object has been created, set its progress
	// status object pointer to NULL, as we have finished processing
	if (m_ipProgressStatusDialog)
	{
		m_ipProgressStatusDialog->ProgressStatusObject = NULL;
	}
}
//-------------------------------------------------------------------------------------------------
string FileProcessingDlgStatusPage::getFileNameForFileId(long fileId)
{
	CSingleLock lg(&m_mutexFileNameMap, TRUE);
	auto it = m_mapFileIdToFileName.find(fileId);
	if (it != m_mapFileIdToFileName.end())
	{
		return it->second;
	}

	return "";
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgStatusPage::addFileNameToFileIdMap(long fileId, const string& strFileName)
{
	CSingleLock lg(&m_mutexFileNameMap, TRUE);
	m_mapFileIdToFileName[fileId] = strFileName;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgStatusPage::removeFileNameForFileId(long fileId)
{
	CSingleLock lg(&m_mutexFileNameMap, TRUE);
	auto it = m_mapFileIdToFileName.find(fileId);
	if (it != m_mapFileIdToFileName.end())
	{
		m_mapFileIdToFileName.erase(it);
	}
}
//-------------------------------------------------------------------------------------------------