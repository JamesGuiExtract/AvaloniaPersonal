#pragma once

#include "resource.h"
#include "afxcmn.h"
#include "FPRecordManager.h"
#include "FPWorkItem.h"

// WorkItemsPage dialog

class WorkItemsPage : public CPropertyPage
{
	DECLARE_DYNCREATE(WorkItemsPage)

public:
	WorkItemsPage();
	virtual ~WorkItemsPage();

// Dialog Data
	enum { IDD = IDD_DLG_WORKITEMS_PROP };

	// Method to process status change messages sent from the FPRecordManager
	void onStatusChange(const FPWorkItem *pWorkItem, EWorkItemStatus eOldStatus);

	// Sets the m_pRecordManager member
	void setRecordManager(FPRecordManager* pRecordMgr);
	
	// Returns a count of the number of files in the currently processing list
	long getCurrentlyProcessingCount();

	// Resets the m_bInitialized flag
	void ResetInitialized();

	// Enables the timer used for progress updates
	void startProgressUpdates();

	// Disable the timer used for progress updates
	void stopProgressUpdates();

	// Clears all data on the form.
	void clear();
	
	// Sets the auto scroll flag to the bAutoScroll value
	void setAutoScroll(bool bAutoScroll);

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();

	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnTimer(UINT nIDEvent);
	
	DECLARE_MESSAGE_MAP()

private:

	// Variable for the current work item list
	CListCtrl m_currentWorkItemsList;

	// Flag to indicate if the page has be initialized
	bool m_bInitialized;

	// The FPRecord manager set with the setRecordManager
	FPRecordManager* m_pRecordManager;

	// Vector containig the current work items being displayed 
	// the order is the same as the order of the items in the m_currentWorkItemsList
	vector<long> m_vecCurrentWorkItemIDs;
	
	// Mutex to synchronize access to the file ID map and lists.
	CMutex m_mutex;

	// Flag to indicate if the list should be scrolled automatically
	bool m_bAutoScroll;

	// Private methods

	// Adjusts the widths of the columns
	void updateCurrColWidths();

	// Adds a new record to the current work item list returns the index of the added record
	long appendNewRecord(CListCtrl& rListCtrl, const FPWorkItem& workItem);

	// Removes a record from the current work item list
	void removeWorkItemFromCurrentList(long nWorkItemID);

	// Returns the index of the row for the given nWorkItemID
	int getWorkItemPosFromVector(const std::vector<long>& vecToSearch, long nWorkItemID);	

	// Updates the progress displayed for the current workitems
	void refreshProgressStatus();
};
