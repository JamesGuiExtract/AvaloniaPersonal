#pragma once

#include "resource.h"

#include <grid\gxall.h>

//--------------------------------------------------------------------------------------------------
// CProgressStatusMFCDlg dialog
//--------------------------------------------------------------------------------------------------
class CProgressStatusMFCDlg : public CDialog
{
public:
	//----------------------------------------------------------------------------------------------
	// Constructor/destructor
	// See documentation for IProgressStatusDialog.ShowModelessDialog() in the IDL file.  The
	// arguments passed to the constructor are just passed through from that IDL method 
	// implementation.
	CProgressStatusMFCDlg(CWnd* pParent, long nNumProgressLevels, long nDelayBetweenRefreshes,
		bool bShowCloseButton, HANDLE hStopEvent);   
	virtual ~CProgressStatusMFCDlg();
	//----------------------------------------------------------------------------------------------
	// Methods to get/set the progress status object in a thread safe way
	void setProgressStatusObject(UCLID_COMUTILSLib::IProgressStatusPtr ipProgressStatus);
	UCLID_COMUTILSLib::IProgressStatusPtr getProgressStatusObject();
	//----------------------------------------------------------------------------------------------

	// Dialog Data
	enum { IDD = IDD_DIALOG_PROGRESS_STATUS };

protected:

	// Overridden MFC methods
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnTimer(UINT_PTR nIDEvent);
	afx_msg void OnClose();
	afx_msg void OnBnClickedStop();
	DECLARE_MESSAGE_MAP()

private:
	DECLARE_DYNAMIC(CProgressStatusMFCDlg)

	// Constants
	static const long ms_nREFRESH_TIMER_ID = 1001;

	// The progress status object and a mutex to protect access to it
	CMutex m_mutexProgressStatus;
	UCLID_COMUTILSLib::IProgressStatusPtr m_ipProgressStatus;

	// Controls on the dialog
	CGXGridWnd gridWnd;

	// The number of progress levels, the delay between refreshes, and whether the close
	// button should be shown
	long m_nNumProgressLevels;
	long m_nDelayBetweenRefreshes; // in milli-seconds
	bool m_bShowCloseButton;

	// Used to keep track of whether the stop button has been pressed and to signal the caller.
	volatile bool m_bStopped;
	HANDLE m_hStopEvent;

	// Method to refresh the progress status levels in the RogueWave grid
	void refreshProgressStatus();
};
//--------------------------------------------------------------------------------------------------
