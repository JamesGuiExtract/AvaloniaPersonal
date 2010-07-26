#pragma once

/////////////////////////////////////////////////////////////////////////////
// FileProcessingDlgReportPage dialog

#include "FileProcessingRecord.h"
#include "TaskInfoDlg.h"
#include "FPRecordManager.h"
#include "FileProcessingDB.h"
#include "TaskEvent.h"

#include <TimeIntervalMerger.h>
#include <XInfoTip.h>

#include <vector>
#include <string>

class FileProcessingConfigMgr;

//////////////
// Enums
//////////////
enum EReportType
{
	kReportProcessed = 0,
	kReportFailed,
	kReportPending,
	kReportSkipped,
	kReportAll,
	kNumReportTypes,
	kReportNone
};

//////////////////
// Snapshot class
//////////////////
class Snapshot
{
public:
	Snapshot();
	Snapshot(ATL::CTime currentTime, 
			UCLID_FILEPROCESSINGLib::IActionStatisticsPtr ipNewActionStats);
	~Snapshot();
	
	/////////////////////
	// Public Methods:
	/////////////////////
	CTime getTime();
	UCLID_FILEPROCESSINGLib::IActionStatisticsPtr getActionStatistic();

private:
	// Time that the snapshot was created
	CTime m_curTime;

	// Action statistics for this snapshot
	UCLID_FILEPROCESSINGLib::IActionStatisticsPtr m_ipActionStats;
};

class FileProcessingDlgReportPage : public CPropertyPage
{
	DECLARE_DYNCREATE(FileProcessingDlgReportPage)

// Construction
public:
	FileProcessingDlgReportPage();
	~FileProcessingDlgReportPage();

	void setConfigMgr(FileProcessingConfigMgr* cfgMgr);
	void clear();
	void setRecordManager(FPRecordManager* pRecordMgr);
	void setFPM( UCLID_FILEPROCESSINGLib::IFileProcessingManager* pFPM);

	// Promise: Update the list with fresh statistics
	// Args:	sWatch - Total Run time from the Dialog
	//			ipActionStatsNew - current snapshot of the DB
	//			nTotalBytes		- Total number of bytes completed / failed
	//			nTotalDocs		- Total number of documents completed / failed
	//			nTotalPages		- Total number of pages completed / failed
	//			nTotalProcTime	- Processing time from the status page's timeIntervalMerger. If the
	//								ProcessingLog page is disabled, this will be 0, and then the local
	//								stats ListCtrl should be disabled.
	void populateStats(StopWatch sWatch, 
				UCLID_FILEPROCESSINGLib::IActionStatisticsPtr ipActionStatsNew,
				const LONGLONG nTotalBytes, const long nTotalDocs, const long nTotalPages, 
				const unsigned long nTotalProcTime);

	bool getInit();

// Dialog Data
	//{{AFX_DATA(FileProcessingDlgReportPage)
	enum { IDD = IDD_DLG_REPORT_PROP };
	CListCtrl m_listGlobalStats;
	CListCtrl m_listLocalStats;
	CButton m_btnExport;
	CEdit m_editCautiously;
	//}}AFX_DATA

// Methods
	// This method is used to reset the m_bInitialized to false
	// when the user hide the tab so that when this tab reappears,
	// it will skip the first call to OnSize()
	void ResetInitialized();

// Overrides
	// ClassWizard generate virtual function overrides
	//{{AFX_VIRTUAL(FileProcessingDlgReportPage)
	protected:
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL
// Implementation
protected:
	// Generated message map functions
	//{{AFX_MSG(FileProcessingDlgReportPage)
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnBnClickedBtnExport();
	afx_msg void OnStnClickedInterpretCautiouslyHelp();
	virtual BOOL OnInitDialog();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:

	//////////////
	// Variables
	//////////////
	// Record Manager
	FPRecordManager* m_pRecordMgr;

	// Pointer to File Processing Manager class
	UCLID_FILEPROCESSINGLib::IFileProcessingManager* m_pFPM;

	// Config Manager is used to get the number of snapshots before Interpret Cautiously
	FileProcessingConfigMgr* m_pCfgMgr;

	// This variable is initialized to false so that the first call of OnSize()
	// before OnInitDialog() will be skipped, then it will be set to true inside 
	// OnInitDialog()
	bool m_bInitialized;

	// The action stats from the last Status update
	UCLID_FILEPROCESSINGLib::IActionStatisticsPtr m_ipActionStatsOld;

	// The time that processing last took place
	CTime m_TimeOfLastProcess;

	// Flag to control whether or not the export outputs the local listbox data to the DB
	bool m_bExportLocal;

	// Vector of database 'snapshots.' This vector will be used to track the state of the DB
	// across a certain amount of time. 
	std::vector<Snapshot> m_vecSnapshots;

	// Number of snapshots needed to determine if statistics may need to be interpreted cautiously
	// This should be the number of snapshots taken within giINTERPRET_STATS_DELAY
	long m_lCautiousSnapshotCount;

	// Vector of completion rates (Bytes/second). This is used to calculate the  
	// average completion rate that is used to artificially smooth the 
	// estimated completion time. m_dTotalCompletionRate is the sum of all rates
	// in the vector
	std::vector<double> m_vecCompletionRates;
	double m_dTotalCompletionRate;

	// Tooltip for the bubble help
	CXInfoTip m_infoTip;

	//////////
	// Methods
	//////////
	// Promise: Sets the global Bytes Processed Row in the stats tab
	// Args:	nBytes		 - total number of bytes
	//			nBytesCom	 - number of completed bytes
	//			nBytesFailed - number of failed bytes
	void setGlobalBytesProcRow(const LONGLONG nBytes, const LONGLONG nBytesCom, 
						 const LONGLONG nBytesFailed);

	// Promise: Sets the global Docs or Pages processed Row in the stats tab
	// Args:	nTotal			 - total number of items
	//			nCompleted		 - number of completed items
	//			nFailed			 - number of failed items
	//			ROWNUM			 - row number to put results in
	void setGlobalProcessedRow(const long nTotal, const long nCompleted, 
						const long nFailed,	const int ROWNUM);

	// Promise:	Set the "Estimated Time Remaining" and "Estimated Completion Time" rows
	// Return:	-false if the first and last entries in the snapshot vector are the same. If this method
	//			returns false, do NOT update the global stats, but still update the local stats.
	//			-true if the first and last entries in the snapshot vector are different. 
	//			This means that the DB has been changed (more files have been processed, etc) 
	//			and the global stats need to be updated.
	bool setTimeRemainingRows();

	// Promise: Set the "Total Run Time" row
	// Args:	sWatch	- Stopwatch from the FileProcessingDlg
	void setTotalRunTimeRow(StopWatch sWatch );
	
	// Promise: Set the "Total Processing Time" row
	// Args:	unTotalProcTime	- Sum of the completed and failed time rows
	void setTotalProcTimeRow( const unsigned long unTotalProcTime );

	// Promise: Set the local "Bytes Processed" row to the current data
	// Args:	- nTotalBytes - the total number of bytes completed or failed
	//			- nTotalProcTime - the number of seconds active processing has been going
	void setLocalBytesProcRow(const LONGLONG nTotalBytes, const unsigned long nTotalProcTime );

	// Promise: Set the local "Pages Processed" row to current data
	void setLocalPagesProcessedRow( const long nTotalPages, const unsigned long nTotalProcTime );

	// Promise: Set the local "Documents Processed" row to current data
	void setLocalDocsProcessedRow( const long nTotalDocs, const unsigned long nTotalProcTime );

	// Time formatting utils
	CString getFormattedTime(const long nDays, const long nHours, const long nMins, 
							const long nSecs,const bool bDisplayMinRes);
	void splitTime(long nTime, long& nDays, long& nHours, long& nMins, long& nSecs);

	// Put the labels in the left-most column
	void fillLabelColumn();

	// Get File Processing Manager smart pointer for brief use
	UCLID_FILEPROCESSINGLib::IFileProcessingManagerPtr getFPM();

	// Resize the lists and align the export button
	void sizeList();

	// Add the latest snapshot to the vector of snapshots using the current time. 
	// If  m_vecSnapshots.size > g_iMAX_SNAPSHOTS_IN_VECTOR, 
	// remove the snapshot on the front of the vector
	void addSnapshotToVector(UCLID_FILEPROCESSINGLib::IActionStatisticsPtr ipActionStatsNew);

	// Add the latest completion rate to the vector. If the size of the vector becomes greater
	// than g_iMAX_COMPLETION_RATES_IN_VECTOR, remove the first item in the vector. 
	// Also updates the m_nTotalCompletionRate appropriately
	void addNewCompletionRateToVector(double dNewCompetionRate);

	// Calculate the log file path. This will create a local static variable and only calculate the
	// log file path if it has not yet been calculated.
	const std::string& getLogFileFullPath();

	// used to resize the list controls to accomodate the header row
	void resizeListForHeader();
};
