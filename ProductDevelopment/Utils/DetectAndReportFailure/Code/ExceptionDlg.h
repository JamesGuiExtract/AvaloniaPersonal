
#pragma once

#include <afxmt.h>

#include <time.h>
#include <string>
#include <vector>

#include <SystemTray.h>
#include <ColorButton.h>

#include <UCLIDException.h>
#include <UPI.h>

#include "ExceptionRecord.h"
#include "NotifyOptions.h"

/////////////////////////////////////////////////////////////////////////////
// ExceptionDlg dialog

class ExceptionDlg : public CDialog
{
// Construction
public:
	ExceptionDlg(std::vector<ExceptionRecord>& rvecExceptionRecords,
		CMutex& m_rExceptionDataLock, NotifyOptions& rOptions,
		CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(ExceptionDlg)
	enum { IDD = IDD_EXCEPTION_DIALOG };
	CColorButton	m_btnClose;
	CColorButton	m_btnPrev;
	CColorButton	m_btnNext;
	CColorButton	m_btnLast;
	CColorButton	m_btnFirst;
	CColorButton	m_btnDetails;
	CColorButton	m_btnClear;
	CColorButton	m_btnClearAll;
	CString	m_zExceptionText;
	CString	m_zExceptionSequence;
	CString	m_zDateTime;
	CString	m_zUPI;
	//}}AFX_DATA

	// notify this dialog of a new logged exception
	// if bForceShowWindow is true, then the popup window will be shown
	// if bForceShowWindow is false, then the popup window will only be shown
	// if m_rOptions.notifyOnScreenWhenExceptionLogged() returns true.
	void notifyLoggedException(const std::string& strStringizedException, 
		const UPI& upi, bool bForceShowWindow);
	
	// methods to show/hide window in any fancy way needed
	void showWindow();
	void hideWindow();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(ExceptionDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	
	// system tray icon manager class to show system tray icon, switch
	// icons, etc
	CSystemTray m_systemTrayIcon;

	// flag indicating the current visible state of the window
	bool m_bWindowVisible;

	// flag to indicate whether we should prevent the auto-hide even
	// though there may have been no activity in the exception dialog
	// for some time
	bool m_bPreventAutoHide;

	bool m_bAllExceptionsSeen;
	long m_nLastBadIconID;

	// brushes to paint the background of window/controls in certain colors
	CBrush m_backgroundColorBrush;
	CBrush m_titleBackgroundBrush;

	// time of last activity in this dialog, and method to update
	// the last activity time
	time_t m_tmLastActivityTime;
	void updateLastActivityTime();

	// th size of the original window as defined in the RC file.
	CRect m_originalWindowSize;
	long m_nStepHeightInPixels; // pixel-height of each "slide" step to show/hide window

	// variables for the exception records and the current index
	CMutex& m_rExceptionDataLock;
	std::vector<ExceptionRecord>& m_rvecExceptionRecords;
	unsigned long m_nCurrentRecordIndex;

	// a reference to the options object passed in by the outer scope
	NotifyOptions& m_rOptions;

	// update the UI
	// see documentation of bForceShowWindow in notifyLoggedException() method
	void refreshUI(bool bForceShowWindow = false);

	// update the enabled/disabled state of the buttons
	void refreshButtonsState();

	// Generated message map functions
	//{{AFX_MSG(ExceptionDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnClose();
	afx_msg HBRUSH OnCtlColor(CDC* pDC, CWnd* pWnd, UINT nCtlColor);
	afx_msg void OnButtonClose();
	afx_msg void OnButtonFirst();
	afx_msg void OnButtonLast();
	afx_msg void OnButtonNext();
	afx_msg void OnButtonPrev();
	afx_msg void OnButtonDetails();
	afx_msg void OnButtonClear();
	afx_msg void OnButtonClearAll();
	afx_msg void OnOK();
	afx_msg void OnCancel();
	afx_msg void OnTimer(UINT nIDEvent);
	virtual LRESULT OnTrayNotification(WPARAM wParam, LPARAM lParam);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
