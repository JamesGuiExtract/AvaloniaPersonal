
#pragma once

#include "FP_UI_Notifications.h"

//-------------------------------------------------------------------------------------------------
// DatabaseStatusIconUpdater class
//-------------------------------------------------------------------------------------------------
class DatabaseStatusIconUpdater : public CWnd
{
public:
	DatabaseStatusIconUpdater(CStatusBarCtrl& rCtrl, long nPaneID, CWnd *pParentWnd);
	~DatabaseStatusIconUpdater();

protected:
	// Message handlers
	afx_msg void OnTimer(UINT nIDEvent);
	afx_msg LRESULT OnDatabaseStatusUpdateEvent(WPARAM wParam, LPARAM lParam);
	DECLARE_MESSAGE_MAP()

private:
	// reference to the status bar control in which the icon should be updated
	CStatusBarCtrl& m_rStatusBarCtrl;
	long m_nDatabaseIconPaneID;
	
	// pointer to the parent window owning this window
	CWnd* m_pParentWnd;

	// handles to bitmaps that are loaded one-time for displaying database
	// wrapper object status
	HICON m_hConnectionEstablished;
	HICON m_hConnectionNotEstablished;
	HICON m_hConnectionBusy;
	HICON m_hWaitingForLock;

	// current mode of database wrapper and a method to update
	// the status icon in the status bar
	EDatabaseWrapperObjectStatus m_eDBWrapperObjectStatus;
	void updateDBStatusIcon();

	// The database status updates are not always shown to the UI right away.
	// Sometimes, the UI is just updated with the current status and at other
	// times, it is made to flip-flop between two states.  This depends upon the
	// number of database status update notifications received since the last
	// UI update, which is what is kept track of in this variable.
	LONG m_nDBStatusUpdatesSinceLastUIUpdate;
};
//-------------------------------------------------------------------------------------------------

