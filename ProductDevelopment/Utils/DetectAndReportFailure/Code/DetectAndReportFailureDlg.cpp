// DetectAndReportFailure.cpp : implementation file
//

#include "stdafx.h"
#include "DetectAndReportFailure.h"
#include "DetectAndReportFailureDlg.h"

#include <FailureDetectionAndReportingConstants.h>
#include <Win32GlobalAtom.h>
#include <UCLIDExceptionDlg.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <Zipper.h>
#include <RegConstants.h>
#include <CpuUsage.h>
#include <MutexUtils.h>

#include <fstream>
using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Globals / statics
//-------------------------------------------------------------------------------------------------
const UINT gONE_HERTZ_TIMER_ID = 1001;

const UINT gMAX_MISSED_PINGS_FOR_ALIVE_APPLICATION = 1;

//-------------------------------------------------------------------------------------------------
// CDetectAndReportFailureDlg dialog
//-------------------------------------------------------------------------------------------------
CDetectAndReportFailureDlg::CDetectAndReportFailureDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CDetectAndReportFailureDlg::IDD, pParent),
	m_lastExceptionFrequencyMetTime(time(NULL)),
	m_ulCPUUsageBelowThresholdForSeconds(0)
{
	try
	{
		// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
		m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);

		// Validate the ini file existence
		validateFileOrFolderExistence(m_options.getINIFileName());

		// create an instance of the exception dialog
		m_apExceptionDlg.reset(new ExceptionDlg(m_vecExceptions, m_exceptionDataLock, m_options));
		m_upLogMutex.reset(getGlobalNamedMutex(gstrLOG_FILE_MUTEX));
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32292");
}
//-------------------------------------------------------------------------------------------------
void CDetectAndReportFailureDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CDetectAndReportFailureDlg)
		// NOTE: the ClassWizard will add DDX and DDV calls here
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CDetectAndReportFailureDlg, CDialog)
	//{{AFX_MSG_MAP(CDetectAndReportFailureDlg)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_WM_TIMER()
	ON_COMMAND(ID_FILE_EXIT, OnFileExit)
	ON_COMMAND(ID_FILE_SHOW_EXCEPTIONS, OnFileShowExceptions)
	ON_COMMAND(ID_FILE_CONFIGURE_AUTO_NOTIFICATIONS, OnFileConfigureAutoNotifications)
	ON_COMMAND(ID_FILE_CONFIGURE_EMAIL_SETTINGS, OnFileConfigureEmailSettings)
	ON_MESSAGE(gNOTIFY_EXCEPTION_LOGGED_MSG, OnNotifyExceptionLogged)
	ON_MESSAGE(gNOTIFY_EXCEPTION_DISPLAYED_MSG, OnNotifyExceptionDisplayed)
	ON_MESSAGE(gNOTIFY_APPLICATION_RUNNING_MSG, OnNotifyApplicationRunning)
	ON_MESSAGE(gNOTIFY_APPLICATION_NORMAL_EXIT_MSG, OnNotifyApplicationNormalExit)
	ON_MESSAGE(gNOTIFY_APPLICATION_ABNORMAL_EXIT_MSG, OnNotifyApplicationAbnormalExit)
	ON_COMMAND(ID_FILE_SEND_EXCEPTION_LOG, OnFileSendExceptionLog)
	ON_COMMAND(ID_FILE_SEND_CUSTOM_MESSAGE, OnFileSendCustomMessage)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CDetectAndReportFailureDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CDetectAndReportFailureDlg::OnInitDialog()
{
		CDialog::OnInitDialog();

		// Set the icon for this dialog.  The framework does this automatically
		//  when the application's main window is not a dialog
		SetIcon(m_hIcon, TRUE);			// Set big icon
		SetIcon(m_hIcon, FALSE);		// Set small icon
		
		// set the window caption
		SetWindowText(gpszFDRS_WINDOW_TITLE);

		// initialize the environment for displaying UCLIDExceptions
		static UCLIDExceptionDlg dlg;
		UCLIDException::setExceptionHandler(&dlg);

		char pszFileName[MAX_PATH + 1];
		if (!GetModuleFileName(AfxGetApp()->m_hInstance, pszFileName, MAX_PATH))
		{
			string strFileVersion = getFileVersion(pszFileName);
			string strApp = "Failure Detection and Reporting System Ver. ";
			strApp += strFileVersion;
			UCLIDException::setApplication(strApp);
		}

		// create the exception dlg
		m_apExceptionDlg->Create(ExceptionDlg::IDD, this);

		 // set timer to hide the dialog box
		// NOTE: the timer frequency is set to 1000ms for a particular reason.
		//		 Don't change the frequency unless you know what you're doing.
		SetTimer(gONE_HERTZ_TIMER_ID, 1000, NULL);
	
	return TRUE;  // return TRUE  unless you set the focus to a control
}
//-------------------------------------------------------------------------------------------------
// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.
void CDetectAndReportFailureDlg::OnPaint() 
{
	if (IsIconic())
	{
		CPaintDC dc(this); // device context for painting

		SendMessage(WM_ICONERASEBKGND, (WPARAM) dc.GetSafeHdc(), 0);

		// Center icon in client rectangle
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// Draw the icon
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CDialog::OnPaint();
	}
}
//-------------------------------------------------------------------------------------------------
// The system calls this to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CDetectAndReportFailureDlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}
//-------------------------------------------------------------------------------------------------
LRESULT CDetectAndReportFailureDlg::OnNotifyExceptionLogged(WPARAM wParam, LPARAM lParam)
{
	INIT_EXCEPTION_AND_TRACING("MLI00021");

	try
	{
		_lastCodePos = "10";

		// get the atom associated with the UPI
		ATOM hUPIAtom = static_cast<ATOM>(wParam);
		Win32GlobalAtom upiAtom;
		upiAtom.attach(hUPIAtom);
		_lastCodePos = "20";

		string strUPI = upiAtom.getName();
		_lastCodePos = "30";

		// get the atom associated with the exception file name
		ATOM hExceptionFileNameAtom = static_cast<ATOM>(lParam);
		Win32GlobalAtom exceptionFileNameAtom;
		exceptionFileNameAtom.attach(hExceptionFileNameAtom);
		_lastCodePos = "40";

		// open the file represented by the atom name and read the stringized exception
		ifstream infile(exceptionFileNameAtom.getName().c_str());
		_lastCodePos = "50";
		if (infile)
		{
			string strException;
			infile >> strException;
			_lastCodePos = "60";

			infile.close();
			_lastCodePos = "70";

			// delete the temporary file containing the stringized exception
			DeleteFile(exceptionFileNameAtom.getName().c_str());
			_lastCodePos = "80";

			// notify the exception dialog of this new exception
			m_apExceptionDlg->notifyLoggedException(strException, UPI(strUPI), false);
			_lastCodePos = "90";

			// check to see if the alert conditions for exception frequency
			// were met 
			checkForExceptionFrequency();
			_lastCodePos = "100";
		}

		_lastCodePos = "110";
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11874")

	return TRUE;
}
//-------------------------------------------------------------------------------------------------
LRESULT CDetectAndReportFailureDlg::OnNotifyExceptionDisplayed(WPARAM wParam, LPARAM lParam)
{
	try
	{
		// get the atom associated with the UPI
		ATOM hUPIAtom = static_cast<ATOM>(wParam);
		Win32GlobalAtom upiAtom;
		upiAtom.attach(hUPIAtom);

		// nothing more to do for now.
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12345")

	return TRUE;
}
//-------------------------------------------------------------------------------------------------
LRESULT CDetectAndReportFailureDlg::OnNotifyApplicationRunning(WPARAM wParam, LPARAM lParam)
{
	try
	{
		// get the atom associated with the UPI
		ATOM hUPIAtom = static_cast<ATOM>(wParam);
		Win32GlobalAtom upiAtom;
		upiAtom.attach(hUPIAtom);
		string strUPI = upiAtom.getName();
	
		// log the last ping time for this application
		CSingleLock lock(&m_pingDataLock, TRUE);
		m_mapUPIToLastPingTime[strUPI] = time(NULL);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12338")

	return TRUE;
}
//-------------------------------------------------------------------------------------------------
LRESULT CDetectAndReportFailureDlg::OnNotifyApplicationNormalExit(WPARAM wParam, LPARAM lParam)
{
	try
	{
		// get the atom associated with the UPI
		ATOM hUPIAtom = static_cast<ATOM>(wParam);
		Win32GlobalAtom upiAtom;
		upiAtom.attach(hUPIAtom);
		string strUPI = upiAtom.getName();

		// delete the item from the map if it exists
		eraseEntryFromPingData(strUPI);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12343")

	return TRUE;
}
//-------------------------------------------------------------------------------------------------
LRESULT CDetectAndReportFailureDlg::OnNotifyApplicationAbnormalExit(WPARAM wParam, LPARAM lParam)
{
	try
	{
		// get the atom associated with the UPI
		ATOM hUPIAtom = static_cast<ATOM>(wParam);
		Win32GlobalAtom upiAtom;
		upiAtom.attach(hUPIAtom);
		string strUPI = upiAtom.getName();

		// create and log an exception noting the abnormal application exit
		UCLIDException ue("ELI12403", "Application abnormally exited!");
		ue.addDebugInfo("UPI", strUPI);
		ue.log("", false);

		// notify the exception dialog of this new exception
		// we always want the popup message to be displayed for this event
		bool bForcePopupNotify = m_options.notificationIsEnabled(
			kPopupNotification, kApplicationCrashed);
		m_apExceptionDlg->notifyLoggedException(ue.asStringizedByteStream(),
			UPI(strUPI), bForcePopupNotify);

		// delete the UPI for the process so that we are no longer checking for
		// missed pings from this app
		eraseEntryFromPingData(strUPI);

		// process any email notifications that are applicable for
		// this application crash
		if (m_options.notificationIsEnabled(kEmailNotification, kApplicationCrashed) && 
			minTimeElapsedBetweenEmails(kApplicationCrashed))
		{
			sendApplicationCrashEmail(strUPI);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12344")

	return TRUE;
}
//-------------------------------------------------------------------------------------------------
void CDetectAndReportFailureDlg::OnTimer(UINT nIDEvent) 
{
	try
	{
		// Check the existence of ini file
		if (!checkIniFileExistence())
		{
			return;
		}

		// the timer event is used to hide this window after it has been
		// displayed for a short period when the application starts.
		static bool bWindowHidden = false;
		if (!bWindowHidden)
		{
			bWindowHidden = true;

			// hide the dialog after an initial pause
			Sleep(1000); // cosmetic
			ShowWindow(SW_HIDE);
		}

		// check to see if any of the applications have crashed
		checkForCrashedApplications();

		// perform CPU usage check
		checkForLowCPUUsage();
		
		CDialog::OnTimer(nIDEvent);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12416")
}
//-------------------------------------------------------------------------------------------------
void CDetectAndReportFailureDlg::OnFileExit() 
{
	try
	{
		// close the application after prompting the user
		if (MessageBox("Are you sure you want to terminate the\n"
			"Extract Systems Failure Detection and Reporting System (FDRS)?", 
			"Confirmation", MB_YESNO) == IDYES)
		{
			m_apExceptionDlg.reset(__nullptr);
			SendMessage(WM_CLOSE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12436");
}
//-------------------------------------------------------------------------------------------------
void CDetectAndReportFailureDlg::OnFileShowExceptions() 
{
	try
	{
		m_apExceptionDlg->showWindow();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12437");
}
//-------------------------------------------------------------------------------------------------
void CDetectAndReportFailureDlg::OnFileConfigureAutoNotifications() 
{
	try
	{
		m_options.showUserInterface();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12434");
}
//-------------------------------------------------------------------------------------------------
void CDetectAndReportFailureDlg::OnFileConfigureEmailSettings() 
{
	try
	{
		// create the email settings object
		ISmtpEmailSettingsPtr ipEmailSettings(CLSID_SmtpEmailSettings);
		ASSERT_RESOURCE_ALLOCATION("ELI12429", ipEmailSettings != __nullptr);

		// Load the current settings and display the configuration UI
		ipEmailSettings->LoadSettings(VARIANT_FALSE);

		IConfigurableObjectPtr ipConfigure = ipEmailSettings;
		ASSERT_RESOURCE_ALLOCATION("ELI32287", ipConfigure != __nullptr);
		ipConfigure->RunConfiguration();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12432")
}
//-------------------------------------------------------------------------------------------------
void CDetectAndReportFailureDlg::OnFileSendExceptionLog() 
{
	try
	{
		CWaitCursor waitCursor;

		// get the default message object
		IExtractEmailMessagePtr ipMsg = getMessageObject();
		ASSERT_RESOURCE_ALLOCATION("ELI12440", ipMsg != __nullptr);

		// populate the message object
		ipMsg->Body= "[Please enter your custom message here if applicable...]";
		ipMsg->Subject = "[FDRS] Exception log";

		// set the FDRS email as the default recipient
		IVariantVectorPtr ipRecipients(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI12442", ipRecipients != __nullptr);
		ipRecipients->PushBack(_bstr_t("fdrs@ExtractSystems.com"));
		ipMsg->Recipients = ipRecipients;

		// set the attachments list
		IVariantVectorPtr ipAttachments(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI12443", ipAttachments != __nullptr);
		ipAttachments->PushBack(_bstr_t(getExceptionLogFile().c_str()));
		ipMsg->Attachments = ipAttachments;

		waitCursor.Restore();

		// Show in client (pass false to indicate no zipping of attachment)
		ipMsg->ShowInClient(VARIANT_FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12438")
}
//-------------------------------------------------------------------------------------------------
void CDetectAndReportFailureDlg::OnFileSendCustomMessage() 
{
	try
	{
		CWaitCursor waitCursor;

		// get the default message object
		IExtractEmailMessagePtr ipMsg = getMessageObject();
		ASSERT_RESOURCE_ALLOCATION("ELI19412", ipMsg != __nullptr);

		// populate the message object
		ipMsg->Body = "[Please enter your custom message here...]";
		ipMsg->Subject = "[FDRS] Custom message";

		// set the FDRS email as the default recipient
		IVariantVectorPtr ipRecipients(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI12441", ipRecipients != __nullptr);
		ipRecipients->PushBack(_bstr_t("fdrs@ExtractSystems.com"));
		ipMsg->Recipients = ipRecipients;

		waitCursor.Restore();
		
		// Show in client (pass false to indicate no zipping of attachment)
		ipMsg->ShowInClient(VARIANT_FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12439")
}

//-------------------------------------------------------------------------------------------------
// Private / helper methods
//-------------------------------------------------------------------------------------------------
void CDetectAndReportFailureDlg::sendApplicationCrashEmail(const string& strUPI)
{
	// create the message string
	string strMsg = "The following application crashed:\n";
	strMsg += strUPI;

	// send the email
	sendAsyncNotification(kApplicationCrashed, strMsg);
}
//-------------------------------------------------------------------------------------------------
void CDetectAndReportFailureDlg::checkForLowCPUUsage()
{
	// object to query CPU usage
	static CCpuUsage m_cpuQueryObj;

	unsigned int uiCPUUsage = m_cpuQueryObj.GetCpuUsage(); // get CPU usage

	const unsigned long ulCPU_THRESHOLD = m_options.getCPUThreshold();
	if (uiCPUUsage < ulCPU_THRESHOLD)
	{
		// CPU usage is below specified threshold.  Increment the
		// counter which keeps track of the number of seconds for which
		// the CPU usage has been below the threshold
		m_ulCPUUsageBelowThresholdForSeconds++;
	}
	else
	{
		// if the CPU usage has crossed the threshold, then reset the
		// counter which keeps track of the number of seconds for which
		// the CPU usage has been below the threshold
		m_ulCPUUsageBelowThresholdForSeconds = 0;
	}

	// if the CPU usage has been below the specified threshold for the specified time
	// duration, AND if CPU usage reporting is enabled, then send the notification
	if (m_ulCPUUsageBelowThresholdForSeconds >= m_options.getCPUCheckDurationInSeconds() &&
		m_options.notificationIsEnabled(kEmailNotification, kCPUUsageIsLow) && 
		minTimeElapsedBetweenEmails(kCPUUsageIsLow))
	{
		// create the message
		string strMsg = "CPU usage has been below the ";
		strMsg += asString(ulCPU_THRESHOLD);
		strMsg += "% threshold for the last ";
		strMsg += asString(m_ulCPUUsageBelowThresholdForSeconds);
		strMsg += " seconds!";

		// reset the counter
		m_ulCPUUsageBelowThresholdForSeconds = 0;

		// send the email
		sendAsyncNotification(kCPUUsageIsLow, strMsg);
	}
}
//-------------------------------------------------------------------------------------------------
void CDetectAndReportFailureDlg::checkForCrashedApplications()
{
	vector<string> vecCrashedAppUPI;

	// perform the application-crash analysis
	{
		// iterate through all the apps that we know are supposed to be alive
		// and check to see if any of them have crashed
		CSingleLock lock(&m_pingDataLock, TRUE);
		map<string, time_t>::iterator iter;
		for (iter = m_mapUPIToLastPingTime.begin(); iter != m_mapUPIToLastPingTime.end(); iter++)
		{
			// get the last ping time for the application
			time_t lastPingTime = iter->second;

			// if we have missed more than the acceptable # of pings, then
			// assume that the application has crashed
			long nMaxTimeElaspseInSeconds = 0;
			nMaxTimeElaspseInSeconds = (gMAX_MISSED_PINGS_FOR_ALIVE_APPLICATION + 1) *
				uiPING_FREQUENCY_IN_SECONDS;
			if (time(NULL) - lastPingTime > nMaxTimeElaspseInSeconds)
			{
				vecCrashedAppUPI.push_back(iter->first);
			}
		}

		// delete entries for crashed apps from the map
		vector<string>::iterator iterCrashedAppUPI;
		if (!vecCrashedAppUPI.empty())
		{
			for (iterCrashedAppUPI = vecCrashedAppUPI.begin(); 
				 iterCrashedAppUPI != vecCrashedAppUPI.end(); iterCrashedAppUPI++)
			{
				m_mapUPIToLastPingTime.erase(
					m_mapUPIToLastPingTime.find(*iterCrashedAppUPI));
			}
		}
	}

	// perform application-crash notifications
	vector<string>::iterator iterCrashedAppUPI;
	for (iterCrashedAppUPI = vecCrashedAppUPI.begin(); 
		 iterCrashedAppUPI != vecCrashedAppUPI.end(); iterCrashedAppUPI++)
	{
		// create and log an exception noting the application was detected as having crashed
		const string& strUPI = *iterCrashedAppUPI;
		UCLIDException ue("ELI12404", "Application detected as having crashed!");
		ue.addDebugInfo("UPI", strUPI);
		ue.log("",false);

		// display the exception in the exception dialog
		bool bForcePopupNotify = m_options.notificationIsEnabled(
			kPopupNotification, kApplicationCrashed);
		m_apExceptionDlg->notifyLoggedException(ue.asStringizedByteStream(),
			UPI(strUPI), bForcePopupNotify);

		// process any email notifications that are applicable for
		// this application crash
		if (m_options.notificationIsEnabled(kEmailNotification, kApplicationCrashed) &&
			minTimeElapsedBetweenEmails(kApplicationCrashed))
		{
			sendApplicationCrashEmail(strUPI);
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CDetectAndReportFailureDlg::checkForExceptionFrequency()
{
	INIT_EXCEPTION_AND_TRACING("MLI00041");

	try
	{
		bool bNotifyByEmail = m_options.notificationIsEnabled(kEmailNotification,
			kExceptionsLoggedFrequently);
		bool bNotifyByPopup = m_options.notificationIsEnabled(kPopupNotification,
			kExceptionsLoggedFrequently);
		_lastCodePos = "10";
		
		// if neither popup or email notification is turned on, then just exit
		if (!bNotifyByEmail && !bNotifyByPopup)
		{
			return;
		}
		_lastCodePos = "20";

		// continue with checking for exception frequency
		unsigned long ulNumExceptionsToCheck = m_options.getNumExceptions();
		unsigned long ulActualTimePeriod = 0;
		time_t endRecordTime = NULL;
		_lastCodePos = "30";

		// check if we have enough exceptions to even continue with this check
		// NOTE: a new scope has been inserted here to ensure that the vecExceptions
		// vector is locked for the smallest duration possible
		{
			CSingleLock lock(&m_exceptionDataLock, TRUE);
			_lastCodePos = "40";
			unsigned long ulNumTotalExceptions = m_vecExceptions.size();
			_lastCodePos = "50";
			if (ulNumTotalExceptions < ulNumExceptionsToCheck)
			{
				return;
			}
			_lastCodePos = "60";

			// get the start exception record that marks the range
			// and ensure that it is later than the last time at which the 
			// exception frequency was met
			const ExceptionRecord& startRecord = 
				m_vecExceptions[ulNumTotalExceptions - ulNumExceptionsToCheck];
			_lastCodePos = "70";
			if (startRecord.getTime() <= m_lastExceptionFrequencyMetTime)
			{
				return;
			}
			_lastCodePos = "80";

			// get the end exception record that marks the range
			const ExceptionRecord& endRecord = m_vecExceptions[ulNumTotalExceptions - 1];
			_lastCodePos = "90";

			// compute the actual time delay between the start and end records
			ulActualTimePeriod = (unsigned long) (endRecord.getTime() - startRecord.getTime());
			_lastCodePos = "100";
			endRecordTime = endRecord.getTime();
			_lastCodePos = "110";
		}

		// get the duration in seconds between the start and end records
		// if that duration is lesser than what is acceptable, then
		// log an exception and send an alert
		unsigned long ulMaxTimePeriod = m_options.getExceptionCheckDurationInSeconds();
		_lastCodePos = "120";
		if (ulActualTimePeriod < ulMaxTimePeriod)
		{
			// update the last time at which the exception frequency was met
			m_lastExceptionFrequencyMetTime = endRecordTime;
			_lastCodePos = "130";

			// log an exception
			UCLIDException ue("ELI12413", "High frequency of exceptions logged recently!");
			ue.addDebugInfo("# exceptions", ulNumExceptionsToCheck);
			ue.addDebugInfo("Actual time period", ulActualTimePeriod);
			ue.addDebugInfo("Max time period", ulMaxTimePeriod);
			ue.log("", false);
			_lastCodePos = "140";

			// display the exception in the exception dialog
			// NOTE: we always want the popup message to be displayed for this event
			m_apExceptionDlg->notifyLoggedException(ue.asStringizedByteStream(),
				UPI::getCurrentProcessUPI(), bNotifyByPopup);
			_lastCodePos = "150";

			// send the email notification if enabled
			if (bNotifyByEmail && minTimeElapsedBetweenEmails(kExceptionsLoggedFrequently))
			{
				// create the email message text
				string strMsg = "High frequency of exceptions logged!\n";
				strMsg += "# exceptions: ";
				strMsg += asString(ulNumExceptionsToCheck);
				strMsg += "\n";
				strMsg += "Actual time period: ";
				strMsg += asString(ulActualTimePeriod);
				strMsg += "\n";
				strMsg += "Max time period: ";
				strMsg += asString(ulMaxTimePeriod);
				strMsg += "\n";
				strMsg += "DetectingProcessUPI: ";
				strMsg += UPI::getCurrentProcessUPI().getUPI();
				_lastCodePos = "160";

				// send notification
				sendAsyncNotification(kExceptionsLoggedFrequently, strMsg);
				_lastCodePos = "170";
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20375");
}
//-------------------------------------------------------------------------------------------------
// data structure to store the data required by the asynchronous emailing-thread
struct AsyncNotificationThreadData
{
public:
	AsyncNotificationThreadData(const ENotificationEvent eEvent, const string& strMsg, 
		CDetectAndReportFailureDlg *pDlg)
		: m_eEvent(eEvent), m_strMsg(strMsg), m_pDlg(pDlg)
	{
		ASSERT_ARGUMENT("ELI12460", pDlg != __nullptr);
	}

private:
	// make the thread function a friend so that the data can be accessed
	friend UINT asyncNotificationThread(LPVOID);

	ENotificationEvent m_eEvent;
	string m_strMsg;
	CDetectAndReportFailureDlg *m_pDlg;
};
//-------------------------------------------------------------------------------------------------
// the asynchronous email thread function 
UINT asyncNotificationThread(LPVOID pData)
{
	try
	{
		// get access to the thread data structure
		AsyncNotificationThreadData *pThreadData = 
			static_cast<AsyncNotificationThreadData *> (pData);
		unique_ptr<AsyncNotificationThreadData> apThreadData(pThreadData);

		CoInitializeEx(NULL, COINIT_MULTITHREADED);

		// ensure that only one thread can send email at a time
		static CMutex ls_lock;
		CSingleLock guard(&ls_lock, TRUE);

		// send the notification synchronously
		apThreadData->m_pDlg->sendNotification(apThreadData->m_eEvent, apThreadData->m_strMsg);

		CoUninitialize();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI12459")

	return 0;
}
//-------------------------------------------------------------------------------------------------
void CDetectAndReportFailureDlg::sendAsyncNotification(const ENotificationEvent eEvent, 
													   const string& strMsg)
{
	// create the thread data structure to perform the asynchronous emailing, 
	// and start the thread
	AsyncNotificationThreadData *pData = new AsyncNotificationThreadData(eEvent, strMsg, this);
	AfxBeginThread(asyncNotificationThread, pData);
}
//-------------------------------------------------------------------------------------------------
const string& CDetectAndReportFailureDlg::getEmailSubjectText(const ENotificationEvent eEvent)
{
	// constants for the various strings
	static const string APPLICATION_CRASHED_EVENT_SUBJECT = "[FDRS] Notification of application crash";
	static const string CPU_USAGE_IS_LOW_EVENT_SUBJECT = "[FDRS] Notification of low CPU usage";
	static const string EXCEPTIONS_LOGGED_FREQUENTLY_EVENT_SUBJECT = "[FDRS] Notification of high frequency of exceptions";

	switch (eEvent)
	{
	case kApplicationCrashed:
		return APPLICATION_CRASHED_EVENT_SUBJECT;

	case kExceptionsLoggedFrequently:
		return EXCEPTIONS_LOGGED_FREQUENTLY_EVENT_SUBJECT;

	case kCPUUsageIsLow:
		return CPU_USAGE_IS_LOW_EVENT_SUBJECT;

	default:
		// we should never reach here!
		THROW_LOGIC_ERROR_EXCEPTION("ELI12673");
	}
}
//-------------------------------------------------------------------------------------------------
string CDetectAndReportFailureDlg::getEmailTimeStamp() const
{
	string strText = "*** ";
	strText += getComputerName();
	strText += " ";
	strText += getDateAsString();
	strText += " ";
	strText += getTimeAsString();
	strText += " ***\n\n";

	return strText;
}
//-------------------------------------------------------------------------------------------------
void CDetectAndReportFailureDlg::sendNotification(const ENotificationEvent eEvent,
												  const string& strMsg)
{
	// get the default message object
	IExtractEmailMessagePtr ipMsg = getMessageObject();
	ASSERT_RESOURCE_ALLOCATION("ELI19413", ipMsg != __nullptr);

	// prepend a computer/date/time stamp to the message
	string strNewMsg = getEmailTimeStamp() + strMsg;

	ipMsg->Body = strNewMsg.c_str();
	ipMsg->Subject = getEmailSubjectText(eEvent).c_str();
	ipMsg->Recipients = m_options.getEmailRecipients();

	// send the message
	ipMsg->Send();

	// lock the event-type-to-last-email-time map for safe thread access
	CSingleLock lock(&m_lastEmailTimeDataLock, TRUE);

	// update the last email time for this particular event type.
	m_mapEventTypeToLastEmailTime[eEvent] = time(NULL);
}
//-------------------------------------------------------------------------------------------------
IExtractEmailMessagePtr CDetectAndReportFailureDlg::getMessageObject()
{
	// create the message object
	IExtractEmailMessagePtr ipMsg(CLSID_ExtractEmailMessage);
	ASSERT_RESOURCE_ALLOCATION("ELI12417", ipMsg != __nullptr);

	// create the email settings object
	ISmtpEmailSettingsPtr ipEmailSettings(CLSID_SmtpEmailSettings);
	ASSERT_RESOURCE_ALLOCATION("ELI12418", ipEmailSettings != __nullptr);

	// load the email settings
	ipEmailSettings->LoadSettings(VARIANT_FALSE);

	// populate the message object
	ipMsg->EmailSettings = ipEmailSettings;

	return ipMsg;
}
//-------------------------------------------------------------------------------------------------
const string& CDetectAndReportFailureDlg::getExceptionLogFile() const
{
	// the location of the exception log is relative to the location of this EXE
	static string strLogFile;
	if (strLogFile.empty())
	{
		// Get the default log file path
		strLogFile = UCLIDException::getDefaultLogFileFullPath();
	}

	// compute the name of the zip file
	static string strZipFile = strLogFile + ".zip";

	// zip the exception log file and return the name of the zip file
	CZipper z(strZipFile.c_str());

	// Mutex around access to the log file
	{
		CSingleLock lg(m_upLogMutex.get(), TRUE);
		z.AddFileToZip(strLogFile.c_str());
	}

	z.CloseZip();

	return strZipFile;
}
//-------------------------------------------------------------------------------------------------
bool CDetectAndReportFailureDlg::minTimeElapsedBetweenEmails(const ENotificationEvent eEvent)
{
	// lock the event-type-to-last-email-time map for safe thread access
	CSingleLock lock(&m_lastEmailTimeDataLock, TRUE);

	map<ENotificationEvent, time_t>::const_iterator iter;
	iter = m_mapEventTypeToLastEmailTime.find(eEvent);
	if (iter == m_mapEventTypeToLastEmailTime.end())
	{
		// no email has been sent for this particualar event type
		// so it's OK to send one.
		return true;
	}
	else
	{
		// get the last email time and the current time
		time_t lastEmailTime = iter->second;
		time_t currentTime = time(NULL);

		// return true only if enough time has elapsed since the last email
		return (currentTime - lastEmailTime >= m_options.getMinSecondsBetweenEmails(eEvent));
	}
}
//-------------------------------------------------------------------------------------------------
void CDetectAndReportFailureDlg::eraseEntryFromPingData(const string& strUPI)
{
	CSingleLock lock(&m_pingDataLock, TRUE);
	map<string, time_t>::iterator iter = m_mapUPIToLastPingTime.find(strUPI);
	if (iter != m_mapUPIToLastPingTime.end())
	{
		m_mapUPIToLastPingTime.erase(iter);
	}
}
//-------------------------------------------------------------------------------------------------
bool CDetectAndReportFailureDlg::checkIniFileExistence()
{
	try
	{
		// Validate the ini file existence
		validateFileOrFolderExistence(m_options.getINIFileName());
	}
	catch(UCLIDException ue)
	{
		KillTimer(gONE_HERTZ_TIMER_ID);

		// Set m_apExceptionDlg as the active window
		// to display the below exception and message box
		// because sometimes when m_apExceptionDlg is not 
		// the active window, exceptions can not be displayed
		m_apExceptionDlg->SetActiveWindow();

		// Display the exception and the message box to let the
		// user know the application will exit becuse of missing file
		ue.display();
		MessageBox("The application will close because DetectAndReportFailure.ini is missing!", 
			"File Not Found", MB_OK|MB_ICONERROR);

		// Close the dialog and return false
		m_apExceptionDlg.reset(__nullptr);
		PostMessage(WM_CLOSE);
		return false;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------