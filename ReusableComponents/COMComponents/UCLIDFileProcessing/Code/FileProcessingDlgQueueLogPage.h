#pragma once

// FileProcessingDlgQueueLogPage.h : header file

#include "FPRecordManager.h"
#include "resource.h"
#include "FP_UI_Notifications.h"
#include "FileSupplyingRecord.h"

#include <vector>

#include <UCLIDException.h>
#include <FileProcessingConfigMgr.h>

//-------------------------------------------------------------------------------------------------
// FileProcessingDlgQueueLogPage dialog
//-------------------------------------------------------------------------------------------------
class FileProcessingDlgQueueLogPage : public CPropertyPage
{
	DECLARE_DYNAMIC(FileProcessingDlgQueueLogPage)

public:
	FileProcessingDlgQueueLogPage();
	virtual ~FileProcessingDlgQueueLogPage();

// Dialog Data
	enum { IDD = IDD_DLG_QUEUELOG_PROP };
	CListCtrl	m_listAttemptingToQueue;
	CListCtrl	m_listQueueLog;
	CListCtrl	m_listFailedQueing;

	//////////////////
	// Methods
	//////////////////
	// This method is used to reset the m_bInitialized to false
	// when the user hide the tab so that when this tab reappears,
	// it will skip the first call to OnSize()
	void ResetInitialized();

	// Promise: Adds an event to the event log
	// Params - Pointer to a FileSupplyingRecord with all the necessary info, this parameter must
	//			come from a call to new, and it will be destroyed at the end of this function by
	//			this function since this function handles ::PostMessage calls.
	// Note   - This in turn calls one of the private helper functions for the event
	//			specific handling of the record.
	void addEvent(FileSupplyingRecord* ptrFileSupRec);

	// Promise: Set the m_bAutoScroll member variable to the argument's value
	void setAutoScroll(bool bAutoScroll);

	// Clear the Queue log tab
	void clear();

	// Set the configuration manager
	void setConfigMgr( FileProcessingConfigMgr* cfgMgr );

protected:
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnNMDblclkFailedFilesList(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnLvnItemchangedListFailedQueing(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnBtnClickedExceptionDetails();
	DECLARE_MESSAGE_MAP()

private:
	///////////////////
	// Variables
	///////////////////
	// This variable is initialized to false so that the first call of OnSize()
	// before OnInitDialog() will be skipped, then it will be set to true inside 
	// OnInitDialog()
	bool m_bInitialized;

	// Variable to control whether or not the list automatically scrolls to
	// focus on the newly added item.
	bool m_bAutoScroll;

	// Config Manager is used to get the max number of rows to display
	FileProcessingConfigMgr* m_pCfgMgr;

	// vector of exceptions associated with the rows in the exception list
	std::vector<UCLIDException> m_vecExceptions;

	///////////////////
	// Methods
	///////////////////
	// Event adding helper methods: 
	void fileAddedEvent(FileSupplyingRecord* ptrFileSupRec, int iNewItem);
	void fileRemovedEvent(FileSupplyingRecord* ptrFileSupRec, int iNewItem);
	void fileRenamedEvent(FileSupplyingRecord* ptrFileSupRec, int iNewItem);
	void fileModifiedEvent(FileSupplyingRecord* ptrFileSupRec, int iNewItem);
	void folderRemovedEvent(FileSupplyingRecord* ptrFileSupRec, int iNewItem);
	void folderRenamedEvent(FileSupplyingRecord* ptrFileSupRec, int iNewItem);

	// Helper method to get a suitable string based on the Action Status
	std::string getStatus(UCLID_FILEPROCESSINGLib::EActionStatus eActionStatus);

	// Enable or disable the vertical scrollbar based upon the number of items in the 
	// list and the size of the list itself.
	void FileProcessingDlgQueueLogPage::enableVertScrollBar();

	// Resizes the columns as necessary. "Folder" column is dynamically sized.
	void updateAttemptingToQueueListColumnWidths();
	void updateQueueLogColumnWidths();
	void updateFailedQueueEventsListColumnWidths();

	// methods to handle the different types of queue event status notifications
	void onQueueEventReceived(FileSupplyingRecord* pFileSupRec);
	void onQueueEventHandled(FileSupplyingRecord* pFileSupRec);
	void onQueueEventFailed(FileSupplyingRecord* pFileSupRec);

	//---------------------------------------------------------------------------------------------
	// PROMISE:	To add a new record to the given list control and also add the
	//			columns that are common to all lists on this page.
	//			This method will also auto-scroll the list control to display the most
	//			recently added row if that setting is enabled
	long appendNewRecord(CListCtrl& rListCtrl, FileSupplyingRecord* pFileSupRec);
	//---------------------------------------------------------------------------------------------
	// PROMISE:	To return a user-friendly string representation for the eFSRecordType
	//			file supplying record type.
	std::string getQueueEventString(EFileSupplyingRecordType eFSRecordType);
	//---------------------------------------------------------------------------------------------
	// PROMISE:	To return the column ID of the file supplier column for the specified
	//			list control.
	long getFileSupplierColumnID(CListCtrl& rListCtrl);
	//---------------------------------------------------------------------------------------------
	// PROMISE:	To return the column ID of the folder column for the specified
	//			list control.
	long getFolderColumnID(CListCtrl& rListCtrl);
	//---------------------------------------------------------------------------------------------
	// PROMISE:	To return the column ID of the file priority column for the specified
	//			list control.
	long getFilePriorityColumnID(CListCtrl& rListCtrl);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To return the CWnd pointer to the specified dialog control, and optionally
	//			also return the control's current window coordinates.
	// REQUIRE:	InitDialog() has already been called and the control represented by uiControlID
	//			is one of the child controls of this property page.
	// PROMISE:	The returned CWnd will be a pointer to the control represented by uiControlID.
	//			If pRect != NULL, the window coordinates of the control will be returned
	//			via pRect.
	CWnd* getWindowAndRectInfo(UINT uiControlID, CRect *pRect);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To position a given button correctly when the Log page is resized
	// REQUIRE:	This method relies on the positions of a related list control and static label
	//			to determine the position of the button.  Those related controls must
	//			already be in their correct positions for this method to work as expected.
	// PROMISE:	The button represened by uiButtonID will be repositioned.  This method does not
	//			change the size of the button.  The button's size will continue to be what it was
	//			designed to be in the RC file.
	// ARGUMENTS:
	//			uiButtonID - this is the ID of the button control to re-position.  
	//			uiLabelID - this is the ID of the static label, whose bottom coordinates will
	//				match the bottom coordinates of the re-positioned button after this method
	//				executes.
	//			rListCtrl - this reference should be set to the list control whose right coordinates
	//				will match the right coordinates of the re-positioned button after this method
	//				executes.
	void repositionButton(UINT uiButtonID, UINT uiLabelID, CListCtrl& rListCtrl);
	//---------------------------------------------------------------------------------------------
};
