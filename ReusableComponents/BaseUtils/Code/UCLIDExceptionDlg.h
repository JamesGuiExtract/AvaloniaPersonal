#pragma once

#include "BaseUtils.h"
#include "UCLIDException.h"

#include <string>

#pragma warning(disable: 4275)	// CDialog is exported

class EXPORT_BaseUtils UCLIDExceptionDlg : public CDialog,
										public UCLIDExceptionHandler
{
// Construction
public:
	UCLIDExceptionDlg(CWnd* pParent = NULL);   // standard constructor
	~UCLIDExceptionDlg();

	void display(const std::string& strMsg);
	void display(const UCLIDException& uclidException);

	//----------------------------------------------------------------------------------------------
	// PURPOSE: To display information associated with an UCLIDException object
	//
	// REQUIRE: Nothing
	//
	// PROMISE: To display all information associated with the specified UCLIDException
	//			object in a Windows GUI.
	//
	// ARGS:	uclidException: the UCLIDException object who's associated infomation needs
	//			to be displayed to the user.
	//		
	void handleException(const UCLIDException& uclidException);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: Specifies whether display() should call SetForegroundWindow.
	// ARGS:	bDisplayAsForegroundWindow: If true, SetForegroundWindow will be called.
	static void inline setDisplayAsForegroundWindow(bool bDisplayAsForegroundWindow)
		{ m_sbDisplayAsForegroundWindow = bDisplayAsForegroundWindow; }
	//----------------------------------------------------------------------------------------------

// Dialog Data
	//{{AFX_DATA(UCLIDExceptionDlg)
	CString	m_Information;
	//}}AFX_DATA

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(UCLIDExceptionDlg)
	public:
	virtual int DoModal();
	virtual BOOL Create(LPCTSTR lpszClassName, LPCTSTR lpszWindowName, DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID, CCreateContext* pContext = NULL);
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	
	// this string will contain the detail information when an exception
	// is being shown.
	std::string strDetailsMsg;

	// Generated message map functions
	//{{AFX_MSG(UCLIDExceptionDlg)
	afx_msg void OnButtonDetails();
	virtual BOOL OnInitDialog();
	afx_msg void OnCheckTimeout();
	afx_msg void OnTimer(UINT nIDEvent);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	///////
	// Data
	///////
	const UCLIDException* m_pUclidExceptionCaught;
	bool	m_bShowDebugInformationDlg;
	// Allow dialog to close automatically
	bool	m_bAutoTimeout;
	// Number of seconds remaining until auto close
	long	m_lTimeoutCount;
	// ID of timer that counts down the seconds
	long	m_lTimerID;
	
	// Whether SetForegroundWindow should be called when the dialog is displayed.
	// This seems to be necessary in the Laserfiche integration to get the exceptions
	// to appear on top, but per discussion with Arvind, we want to be careful not
	// to affect existing behavior.
	static bool m_sbDisplayAsForegroundWindow;

	//////////
	// Methods
	//////////

	// Builds label for checkbox control - showing a countdown
	string	makeTimeoutString(long lTimeout);
	// Does the actual update of the label and closes dialog at zero
	void	updateCountdown();
};
