
#include "stdafx.h"
#include "DatabaseStatusIconUpdater.h"
#include "resource.h"

#include <UCLIDException.h>

//-------------------------------------------------------------------------------------------------
// constants
//-------------------------------------------------------------------------------------------------
static const int giDB_STATUS_INDICATOR_UPDATE_FREQ = 100; // milli-seconds
static const int giDB_STATUS_INDICATOR_UPDATE_TIMER_ID = 1002;
static const int giDATABASE_STATUS_ICON_UPDATER_WINDOW_ID = 21234;

//-------------------------------------------------------------------------------------------------
// DatabaseStatusIconUpdater class
//-------------------------------------------------------------------------------------------------
DatabaseStatusIconUpdater::DatabaseStatusIconUpdater(CStatusBarCtrl& rCtrl, long nPaneID,
													 CWnd *pParentWnd)
:m_rStatusBarCtrl(rCtrl), m_nDatabaseIconPaneID(nPaneID), m_hConnectionEstablished(0),
 m_hConnectionNotEstablished(0), m_hConnectionBusy(0), m_hWaitingForLock(0), 
 m_pParentWnd(pParentWnd), m_nDBStatusUpdatesSinceLastUIUpdate(0),
 m_eDBWrapperObjectStatus(kConnectionNotEstablished)
{
	try
	{
		// verify parent window is valid
		ASSERT_ARGUMENT("ELI15018", m_pParentWnd != __nullptr);

		// create this window object
		CRect rect(0, 0, 0, 0);
		Create(NULL, "DatabaseStatusIconUpdater", WS_CHILD, rect, m_pParentWnd, 
			giDATABASE_STATUS_ICON_UPDATER_WINDOW_ID);

		// load the bitmaps associated with the various database wrapper status'es.
		// NOTE: the 4 IDI_ICON_* identifiers used below must have the exact same numeric value
		// in the resource.h files of the UCLIDFileProcessing project as well as the FAMDBAdmin project
		m_hConnectionEstablished = ::LoadIcon(AfxGetInstanceHandle(), MAKEINTRESOURCE(IDI_ICON_CONNECTION_ESTABLISHED));
		ASSERT_RESOURCE_ALLOCATION("ELI14995", m_hConnectionEstablished != __nullptr);
		m_hConnectionNotEstablished = ::LoadIcon(AfxGetInstanceHandle(), MAKEINTRESOURCE(IDI_ICON_CONNECTION_NOT_ESTABLISHED));
		ASSERT_RESOURCE_ALLOCATION("ELI14996", m_hConnectionNotEstablished != __nullptr);
		m_hConnectionBusy = ::LoadIcon(AfxGetInstanceHandle(), MAKEINTRESOURCE(IDI_ICON_CONNECTION_BUSY));
		ASSERT_RESOURCE_ALLOCATION("ELI14997", m_hConnectionBusy != __nullptr);
		m_hWaitingForLock = ::LoadIcon(AfxGetInstanceHandle(), MAKEINTRESOURCE(IDI_ICON_WAITING_FOR_LOCK));
		ASSERT_RESOURCE_ALLOCATION("ELI14998", m_hWaitingForLock != __nullptr);

		// initiate the timer for the db status/access indicator
		SetTimer(giDB_STATUS_INDICATOR_UPDATE_TIMER_ID, 
			giDB_STATUS_INDICATOR_UPDATE_FREQ, NULL);

		// update the icon
		updateDBStatusIcon();
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI15015")
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(DatabaseStatusIconUpdater, CWnd)
	ON_WM_TIMER()
	ON_MESSAGE(FP_DB_STATUS_UPDATE, OnDatabaseStatusUpdateEvent)
END_MESSAGE_MAP()
//-------------------------------------------------------------------------------------------------
DatabaseStatusIconUpdater::~DatabaseStatusIconUpdater()
{
	try
	{
		// delete the handles to the bitmaps associated with database status
		// if these bitmaps were allocated successfully
		if (m_hConnectionEstablished)
		{
			::DeleteObject(m_hConnectionEstablished);
		}
		if (m_hConnectionNotEstablished)
		{
			::DeleteObject(m_hConnectionNotEstablished);
		}
		if (m_hConnectionBusy)
		{
			::DeleteObject(m_hConnectionBusy);
		}
		if (m_hWaitingForLock)
		{
			::DeleteObject(m_hWaitingForLock);
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI15016")
}

//-------------------------------------------------------------------------------------------------
// Message handlers
//-------------------------------------------------------------------------------------------------
LRESULT DatabaseStatusIconUpdater::OnDatabaseStatusUpdateEvent(WPARAM wParam, LPARAM lParam)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// update the database wrapper object status
		m_eDBWrapperObjectStatus = static_cast<EDatabaseWrapperObjectStatus>(wParam);

		// increment the count that keeps track of # of updates since last UI update
		InterlockedIncrement(&m_nDBStatusUpdatesSinceLastUIUpdate);

		// NOTE: the status icon will be updated when the
		// giDB_STATUS_INDICATOR_UPDATE_TIMER_ID timer event fires next
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI14991");

	return 0;
}
//-------------------------------------------------------------------------------------------------
void DatabaseStatusIconUpdater::OnTimer(UINT nIDEvent)
{
	try
	{
		switch (nIDEvent)
		{
		case giDB_STATUS_INDICATOR_UPDATE_TIMER_ID:
			// update the database status / access icon
			updateDBStatusIcon();
			break;
		
		default:
			// for all other timers, just call the base class method
			CWnd::OnTimer(nIDEvent);
			break;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15017")
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void DatabaseStatusIconUpdater::updateDBStatusIcon()
{
	// reset the counts that keeps track of db status updates since last UI update
	LONG nNumStatusUpdatesSinceLastUIUpdate = InterlockedExchange(&m_nDBStatusUpdatesSinceLastUIUpdate, 0);

	// determine the icon that matches the current status
	HICON hIconForCurrentStatus = NULL;	
	switch (m_eDBWrapperObjectStatus)
	{
	case kConnectionNotEstablished:
		hIconForCurrentStatus = m_hConnectionNotEstablished;
		break;
	case kConnectionEstablished:
		hIconForCurrentStatus = m_hConnectionEstablished;
		break;
	case kWaitingForLock:
		hIconForCurrentStatus = m_hWaitingForLock;
		break;
	case kConnectionBusy:
		hIconForCurrentStatus = m_hConnectionBusy;
		break;
	default:
		{
			UCLIDException ue("ELI14993", "Internal logic error!");
			ue.addDebugInfo("Status", (int) m_eDBWrapperObjectStatus);
			throw ue;
		}
	}

	// determine the icon to display, which may not necessarily be matching the current status
	static HICON hLastDisplayedIcon = NULL;
	HICON hIconToDisplay = NULL;

	// by default, we want the UI to display the current status
	hIconToDisplay = hIconForCurrentStatus;

	// if there have been updates to the status recently, and the
	// current status is either waiting for lock / connection busy states, and the
	// previous status was either waiting for lock / connection busy states, then we want the
	// UI to just switch to the opposite of the last waiting for lock / connection busy state.
	// This will ensure a smooth "flip/flop" between the two states so that the UI is not unnecessarily
	// refreshing.  Also, this code below ensures that if constant activity is taking place, and the
	// timer actually happened to fire each time exactly when the status is waiting for lock, the UI
	// will not just display the waiting for lock icon.  It will actually flip/flop between the two states
	// as long as there is database status updates taking place.
	if ((nNumStatusUpdatesSinceLastUIUpdate >= 0) &&
		(hLastDisplayedIcon == m_hWaitingForLock || hLastDisplayedIcon == m_hConnectionBusy) &&
		(hIconForCurrentStatus == m_hWaitingForLock || hIconForCurrentStatus == m_hConnectionBusy))
	{
		hIconToDisplay = (hLastDisplayedIcon == m_hWaitingForLock) ? m_hConnectionBusy  : m_hWaitingForLock;
	}

	// NOTE: the above code guarantees that the UI status is no more than giDB_STATUS_INDICATOR_UPDATE_FREQ
	// milli-seconds behind the actual status update.  This "guarantee" is based upon the assumption that
	// that no other message handling function will "block" the queue and prevent timer messages from being received.

	// update the icon in the status pane if the status bar control is still a valid window
	if (::IsWindow(m_rStatusBarCtrl.m_hWnd))
	{
		m_rStatusBarCtrl.SetIcon(m_nDatabaseIconPaneID, hIconToDisplay);
		hLastDisplayedIcon = hIconToDisplay;
	}
}
//-------------------------------------------------------------------------------------------------
