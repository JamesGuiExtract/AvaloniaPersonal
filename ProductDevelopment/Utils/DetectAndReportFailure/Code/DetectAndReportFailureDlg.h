// DetectAndReportFailureDlg.h : header file
//

#pragma once

#include <afxmt.h>

#include "ExceptionDlg.h"
#include "NotifyOptions.h"

#include <memory>
#include <ThreadSafeLogFile.h>

#include <map>

#include <time.h>

/////////////////////////////////////////////////////////////////////////////
// CDetectAndReportFailureDlg dialog

class CDetectAndReportFailureDlg : public CDialog
{
// Construction
public:
	CDetectAndReportFailureDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	//{{AFX_DATA(CDetectAndReportFailureDlg)
	enum { IDD = IDD_DETECT_AND_REPORT_FAILURE_DIALOG };
		// NOTE: the ClassWizard will add data members here
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CDetectAndReportFailureDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	virtual LRESULT OnNotifyExceptionLogged(WPARAM wParam, LPARAM lParam);
	virtual LRESULT OnNotifyExceptionDisplayed(WPARAM wParam, LPARAM lParam);
	virtual LRESULT OnNotifyApplicationRunning(WPARAM wParam, LPARAM lParam);
	virtual LRESULT OnNotifyApplicationNormalExit(WPARAM wParam, LPARAM lParam);
	virtual LRESULT OnNotifyApplicationAbnormalExit(WPARAM wParam, LPARAM lParam);
	//}}AFX_VIRTUAL

// Implementation
protected:
	HICON m_hIcon;

	std::unique_ptr<ExceptionDlg> m_apExceptionDlg;

	// exception records vector and a lock for it
	CMutex m_exceptionDataLock;
	std::vector<ExceptionRecord> m_vecExceptions;

	time_t m_lastExceptionFrequencyMetTime;
	unsigned long m_ulCPUUsageBelowThresholdForSeconds;

	NotifyOptions m_options;

	// send an email to the recipients mentioned in the settings with
	// the given message and subject
	void sendNotification(const ENotificationEvent eEventType, const std::string& strMsg);

	// a method to send the email message asynchronously
	void sendAsyncNotification(const ENotificationEvent eEventType, const std::string& strMsg);
	friend UINT asyncNotificationThread(LPVOID); // asynchronous emailing thread function

	// REQUIRE: eEvent must be an event type that supports email notification
	// this method returns a standard subject-line string for the different notification
	// events that support email notification
	const std::string& getEmailSubjectText(const ENotificationEvent eEvent);

	// get a default message object, pre-populated with the email settings,
	// on which further actions can be taken (such as displaying the UI, setting
	// other properties, and sending the email message.
	IESMessagePtr getMessageObject();

	// get the full path to the exception log file
	const std::string& getExceptionLogFile() const;

	// Check if the ini file exists, if not, display the message, post WM_CLOSE message
	// and return false, otherwise return true
	bool checkIniFileExistence();

	// map of the Unique Process Identifier (UPI) to last ping time
	// and an access lock for it
	CMutex m_pingDataLock;
	std::map<string, time_t> m_mapUPIToLastPingTime;

	// method to erase an entry from the ping data, for instance if the
	// application is no longer running
	void eraseEntryFromPingData(const std::string& strUPI);

	// map of event type to last email time
	// This map is used to ensure that we are not sending notification emails for 
	// a particular event more frequently than configured.
	CMutex m_lastEmailTimeDataLock;
	std::map<ENotificationEvent, time_t> m_mapEventTypeToLastEmailTime;
	bool minTimeElapsedBetweenEmails(const ENotificationEvent eEvent);

	void sendApplicationCrashEmail(const std::string& strUPI);

	void checkForExceptionFrequency();
	void checkForCrashedApplications();
	void checkForLowCPUUsage();

	// return the email time stamp string that should be at the top of every email
	std::string getEmailTimeStamp() const;

	// write a line to the log file, in a thread-safe way
	static void writeToLogFile(const std::string& strLine);
	
	// Generated message map functions
	//{{AFX_MSG(CDetectAndReportFailureDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnTimer(UINT nIDEvent);
	afx_msg void OnFileExit();
	afx_msg void OnFileShowExceptions();
	afx_msg void OnFileConfigureAutoNotifications();
	afx_msg void OnFileConfigureEmailSettings();
	afx_msg void OnFileSendExceptionLog();
	afx_msg void OnFileSendCustomMessage();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
