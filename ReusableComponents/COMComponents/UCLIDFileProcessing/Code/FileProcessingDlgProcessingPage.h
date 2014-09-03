#pragma once

#include "FileProcessingRecord.h"
//#include "TaskInfoDlg.h"
#include "FPRecordManager.h"
//#include "TaskEvent.h"
#include "FileProcessingDlgStatusPage.h"
#include "WorkItemsPage.h"

#include <TimeIntervalMerger.h>
#include <FileProcessingConfigMgr.h>
#include <ResizablePropertySheet.h>

#include <vector>
#include <deque>
#include <set>
#include <map>
#include <string>

using namespace std;


// FileProcessingDlgProcessingPage dialog

class FileProcessingDlgProcessingPage : public CPropertyPage
{
	DECLARE_DYNCREATE(FileProcessingDlgProcessingPage)

public:
	FileProcessingDlgProcessingPage();
	virtual ~FileProcessingDlgProcessingPage();

	//---------------------------------------------------------------------------------------------
	void clear();
	//---------------------------------------------------------------------------------------------
	void onStatusChange(const FileProcessingRecord *pTask, ERecordStatus eOldStatus);
	//---------------------------------------------------------------------------------------------
	void onWorkItemStatusChange(const FPWorkItem *pWorkItem, EWorkItemStatus eOldStatus);
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
	void refresh();

	// Enum for the page indexes of the embeded property pages
	typedef enum EProcessingDlgTabPage:int
	{
		kFilesPage = 0,
		kWorkItemsPage = 1
	} EProcessingTabPage;


	// Methods
	// This method is used to reset the m_bInitialized to false
	// when the user hide the tab so that when this tab reappears,
	// it will skip the first call to OnSize()
	void ResetInitialized();
// Dialog Data
	enum { IDD = IDD_DLG_PROCESSING_LOG_PROP };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();
	afx_msg void OnSize(UINT nType, int cx, int cy);

	DECLARE_MESSAGE_MAP()
	
private:
	// Flag to idicate if the page has been initialized
	bool m_bInitialized;

	// Number of Currently processing files or work items
	long m_nNumCurrentProcessingFiles;
	long m_nNumCurrentProcessingWorkItems;

	// The property sheet that contains the processing page for files and work items
	ResizablePropertySheet m_propSheet;

	// The pages that are contained in the property sheet
	FileProcessingDlgStatusPage m_propProcessLogPage;
	WorkItemsPage m_propWorkItemsPage;

	// Creates the tabs
	void createPropertyPages();

	// Sets the text for the indicated tab to the new text
	void changeTabTextCount(int nTabIndex, string strTitleText, long nCount);

	// updates the tabs that are displayed based on the pages in the set
	void updateTabs(const set <EProcessingTabPage>& setPages);

	// Returns a pointer to the property page that is specified by ePage, this should always return a valid pointer
	CPropertyPage *getPropertyPage(EProcessingTabPage ePage);

	// Returns true if the property page specified by ePage is displayed, otherwise false
	bool isPageDisplayed(EProcessingTabPage ePage);

	// Removes the page from the property sheet m_propSheet
	void removePage(EProcessingTabPage ePage);

	// Displays the given page if it is not already displayed
	void displayPage(EProcessingTabPage ePage);
};
