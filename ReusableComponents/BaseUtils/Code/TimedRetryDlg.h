#pragma once

#include "BaseUtils.h"
#include "resource.h"

#include <string>

// CTimedRetryDlg dialog

class EXPORT_BaseUtils CTimedRetryDlg : public CDialog
{
	DECLARE_DYNAMIC(CTimedRetryDlg)

public:
	// Constructs a dialog object that will display with 
	// Caption of strCaption, 
	// Message of strMsg,
	// if nTimeOut == 0 will wait until user presses OK or Cancel
	// if nTimeOut > 0 will count down the number of seconds and press the OK button
	CTimedRetryDlg( std::string strCaption, std::string strMsg, int nTimeOut, CWnd* pParent = NULL);   // standard constructor
	virtual ~CTimedRetryDlg();
	virtual int DoModal();

// Dialog Data
	enum { IDD = IDD_TIMEDRETRYDLG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();
	afx_msg void OnTimer(UINT nIDEvent);
	DECLARE_MESSAGE_MAP()

private:
	// ID of timer that counts down the seconds
	UINT m_uiTimerID;

	// Number of seconds to count down to
	int m_nTimeOut;
	
	// Number of seconds that remain
	int m_nTimeRemaining;
	
	// Message displayed in Dialog
	std::string m_strMsg;

	// Caption displayed in the title bar
	std::string m_strCaption;

	// Static control that contains the message
	CString m_zStaticMsg;

	// OK button
	CButton m_btnOK;
};
