#pragma once

// ProgressDlg.h : header file
//
#include "resource.h"
//#include "Win32Semaphore.h"
#include "Win32Event.h"

#include <afxcmn.h>
#include <afxmt.h>

#include <string>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CProgressDlg dialog

class EXPORT_BaseUtils CProgressDlg : public CDialog
{
// Construction
public:
	CProgressDlg(CWnd* pParent = NULL);
	CProgressDlg(CWnd* pParent, bool bModeless);   // standard constructor

// Dialog Data
	//{{AFX_DATA(CProgressDlg)
	enum { IDD = IDD_PROGRESS_DIALOG };
	CButton	m_ctrlCancel;
	CProgressCtrl	m_ctrlProgressBar;
	CString	m_szText;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CProgressDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CProgressDlg)
	virtual void OnCancel();
	virtual BOOL OnInitDialog();
	afx_msg void OnTimer(UINT nIDEvent);
	afx_msg LRESULT OnKillDlg(WPARAM wParam, LPARAM lParam);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()


public:

	// This displays the progress dialog
	void show();

	// This hides the progress dialog
	void hide();

	// This turns the cancel button on or off and resizes the window accordingly
	void showCancel(bool bShowCancel);

	// Resets the percent complete to zero
	void reset();

	// tell the window how much of the progressing task has been completed
	void setPercentComplete(double fPercent);

	// set the caption that will be displayed above the progress dialog
	void setText(string strText);

	// set the text the will appear in the dialog title bar
	void setTitle(string strText);
	
	// returns true if the user has canceled
	// the window
	bool userCanceled();

	// Tell the dialog that it should End itself with the specified 
	// termination code
	void kill(int retCode = 0);

	void waitForInit();

private:
	CStatic m_txtCaption;

	// This semaphore basically protects the private member data
	// of this class
	CMutex m_mutexAll;

	// this will be signaled once the dlg is initialized
	Win32Event m_eventInit;

	// set to true when the dialog is canceled by the user
	bool m_bUserCanceled;

	// the max amount allowed 
	double m_fMaxNoRefresh;

	// the percentage that was used for the last update
	double m_fPrevPercent;

	// the percentage that will be used for the next update
	double m_fCurrPercent;

	// this specifies that the information in the progress dialog
	// needs to be updated
	bool m_bUpdateWindow;

	// When this flag is set to true the thread will kill itself
	bool m_bKill;

	// true if the progress bar should be shown
	bool m_bShow;

	// the text that will be displayed above the progress bar
	std::string m_strText;

	// the text that will appear in the dialog title bar
	std::string m_strTitle;

	// is the cancel button shown or not
	bool m_bShowCancel;

	// true if this is the first time the dialog has been created with DoModal
	bool m_bFirstCreate;
	// The last position the dialog was displayed at
	RECT m_rectLastPos;

	int m_nRetCode;

	UINT_PTR m_nTimer;

	// configures the dialog to show/hide the cancel
	void setShowCancel(bool bShowCancel);

};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
