
#pragma once

#include "FileProcessingRecord.h"
#include "TaskInfoDlg.h"
#include "FPRecordManager.h"
#include "TaskEvent.h"

#include <TimeIntervalMerger.h>
#include <FileProcessingConfigMgr.h>

#include <vector>
#include <deque>
#include <set>
#include <string>

/////////////////////////////////////////////////////////////////////////////
// FileProcessingDlgStatusPage dialog

class FileProcessingDlgStatusPage : public CPropertyPage
{
	DECLARE_DYNCREATE(FileProcessingDlgStatusPage)

// Construction
public:
	//---------------------------------------------------------------------------------------------
	FileProcessingDlgStatusPage();
	//---------------------------------------------------------------------------------------------
	~FileProcessingDlgStatusPage();
	//---------------------------------------------------------------------------------------------
	void clear();
	//---------------------------------------------------------------------------------------------
	void onStatusChange(long nTaskID, ERecordStatus eOldStatus, ERecordStatus eNewStatus);
	//---------------------------------------------------------------------------------------------
	void setRecordManager(FPRecordManager* pRecordMgr);
	//---------------------------------------------------------------------------------------------
	// Returns total processing time via the timeIntervalMerger's getTotalSeconds().
	unsigned long getTotalProcTime();
	//---------------------------------------------------------------------------------------------
	void setConfigMgr( FileProcessingConfigMgr* cfgMgr );
	//---------------------------------------------------------------------------------------------
	void setAutoScroll(bool bAutoScroll);
	//---------------------------------------------------------------------------------------------
	// Returns a count of the number of files in the currently processing list
	long getCurrentlyProcessingCount();
	//---------------------------------------------------------------------------------------------
	// Promise: Returns the total amount of each argument by reference. 
	//			Used to get local statistics for the statistics page
	// Args:    - nTotalBytes : total number of bytes completed or failed
	//			- nTotalDocs  : total number of documents completed or failed
	//			- nTotalPages : total number of pages completed or failed
	void getLocalStats( LONGLONG& nTotalBytes, long& nTotalDocs, long& nTotalPages );
	//---------------------------------------------------------------------------------------------
	// PURPOSE:	Methods to start/stop progress status updates
	void startProgressUpdates();
	void stopProgressUpdates();
	//---------------------------------------------------------------------------------------------


// Dialog Data
	//{{AFX_DATA(FileProcessingDlgStatusPage)
	enum { IDD = IDD_DLG_STATUS_PROP };
	CListCtrl	m_currentFilesList;
	CListCtrl	m_completedFilesList;
	CListCtrl	m_failedFilesList;
	//}}AFX_DATA

// Methods
	// This method is used to reset the m_bInitialized to false
	// when the user hide the tab so that when this tab reappears,
	// it will skip the first call to OnSize()
	void ResetInitialized();

// Overrides
	protected:
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

// Implementation
protected:
	virtual BOOL OnInitDialog();
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnTimer(UINT nIDEvent);
	afx_msg void OnNMDblclkFailedFilesList(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnNMDblclkCurrentFilesList(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnNMRclkFileLists(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnBtnClickedProgressDetails();
	afx_msg void OnItemchangedFailedFilesList(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnBtnClickedExceptionDetails();
	DECLARE_MESSAGE_MAP()

private:
	struct StatusUpdateInfo
	{
		long FileId;
		ERecordStatus OldStatus;
		ERecordStatus NewStatus;
	};

	//////////////
	// Variables
	//////////////
	std::vector<long> m_vecCurrFileIds;
	std::vector<long> m_vecCompFileIds;
	std::vector<long> m_vecFailFileIds;

	// vector of strings for UEX codes to go with failed files
	std::vector<std::string> m_vecFailedUEXCodes;

	// Automatically scroll to the end of the list on insert based on this bool
	bool m_bAutoScroll;

	// Used to keep track of the local statistics for the stats page
	LONGLONG m_nTotalBytesProcessed;
	long m_nTotalPagesProcessed;
	long m_nTotalDocumentsProcessed;

	FPRecordManager* m_pRecordMgr;
	FileProcessingConfigMgr* m_pCfgMgr;

	// Time Interval Merger to keep track of processing time
	TimeIntervalMerger m_completedFailedOrCurrentTime;

	// This variable is initialized to false so that the first call of OnSize()
	// before OnInitDialog() will be skipped, then it will be set to true inside 
	// OnInitDialog(). This prevents assert errors from null objects.
	bool m_bInitialized;

	// The detailed progress status dialog that can be launched from 
	// this property page.
	IProgressStatusDialogPtr m_ipProgressStatusDialog;

	//////////
	// Methods
	//////////
	// Promise:  Will return the position of the fileID in the specified vector
	int getTaskPosFromVector(const std::vector<long>& vecToSearch, long nFileID);

	// Resizes the columns as necessary. "Folder" column is dynamically sized.
	void updateCurrColWidths();
	void updateCompColWidths();
	void updateFailColWidths();

	//---------------------------------------------------------------------------------------------
	// PROMISE:	To remove the earliest entry in rListCtrl if the list is already at its maximum 
	//			size.  Associated internal member variables are updated accordingly.
	void limitListSizeAndUpdateVars(CListCtrl& rListCtrl);
	//---------------------------------------------------------------------------------------------
	// PROMISE:	To a task from pending to current state (which makes the task be displayed in the
	//			current list) and to update all member variables accordingly.
	void moveTaskFromPendingToCurrent(long nFileID);
	//---------------------------------------------------------------------------------------------
	// PROMISE:	To move the record from the current list to the complete list and update
	//			all member variables accordingly.
	void moveTaskFromCurrentToComplete(long nFileID);
	//---------------------------------------------------------------------------------------------
	// PROMISE:	To move the record from the current list to the failed list and update all
	//			member variables accordingly.
	void moveTaskFromCurrentToFailed(long nFileID);
	//---------------------------------------------------------------------------------------------
	// PROMISE:	To remove the record in the current list associated with nFileID and to
	//			update all internal member variables accordingly.
	void removeTaskFromCurrentList(long nFileID);
	//---------------------------------------------------------------------------------------------
	// PROMISE:	To add a new record to the given list control and also add the
	//			leftmost four columns of data to that list.  Also, internal member
	//			variables (such as vectors) associated with rListCtrl will be updated.
	//			This method will also auto-scroll the list control to display the most
	//			recently added row if that setting is enabled
	// ARGS:	rListCtrl- The list control to which the record should be appended
	//			task- The source record for the entry
	//			bErrorTaskEntry- true if entry is in regards to a failed error task
	long appendNewRecord(CListCtrl& rListCtrl, const FileProcessingRecord& task, bool bErrorTaskEntry = false);
	//---------------------------------------------------------------------------------------------
	// PURPOSE:	To update internal variables that keep track of statistics with the latest
	//			information from the task object associated with nFileID.
	void updateStatisticsVariablesWithTaskInfo(long nFileID);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To notify the detailed progress status dialog of the top level progress status
	//			object it should display (along with the # of levels of sub-progress the dialog
	//			is configured to show).  This method will set the progress status object of
	//			the detailed progress status window to that of the selected entry in the 
	//			"files currently being processed" list.  If no entry on that list is selected,
	//			and the list has at least 1 entry, the detailed progress status window will be 
	//			configured to display the progress of the topmost entry in the list.
	void updateProgressDetailsDlg();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To return the CWnd pointer to the specified dialog control, and optionally
	//			also return the control's current window coordinates.
	// REQUIRE:	InitDialog() has already been called and the control represented by uiControlID
	//			is one of the child controls of this property page.
	// PROMISE:	The returned CWnd will be a pointer to the control represented by uiControlID.
	//			If pRect != __nullptr, the window coordinates of the control will be returned
	//			via pRect.
	CWnd* getWindowAndRectInfo(UINT uiControlID, CRect *pRect);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To update the progress status information for all currently executing files, based
	//			upon the progress status information associated with each FileProcessingRecord
	//			object.
	void refreshProgressStatus();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To position the progress or exception details button correctly when the
	//			property page is resized.
	// REQUIRE:	This method relies on the positions of a related list control and static label
	//			to determine the position of the details button.  Those related controls must
	//			already be in their correct positions for this method to work as expected.
	// PROMISE:	The button represened by uiButtonID will be repositioned.  This method does not
	//			change the size of the button.  The button's size will continue to be what it was
	//			designed to be in the RC file.
	// ARGUMENTS:
	//			uiButtonID - this is the ID of the button control to re-position.  Pass in 
	//				the ID of the exception details button or the progress details button.
	//			uiLabelID - this is the ID of the static label, whose bottom coordinates will
	//				match the bottom coordinates of the re-positioned button after this method
	//				executes.
	//			rListCtrl - this reference should be set to the list control whose right coordinates
	//				will match the right coordinates of the re-positioned button after this method
	//				executes.
	void repositionDetailsButton(UINT uiButtonID, UINT uiLabelID, CListCtrl& rListCtrl);
	//---------------------------------------------------------------------------------------------
};
