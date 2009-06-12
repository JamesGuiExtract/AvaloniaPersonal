// ExceptionDlg.cpp : implementation file
//

#include "stdafx.h"
#include "DetectAndReportFailure.h"
#include "ExceptionDlg.h"
#include "NotifyOptions.h"

#include <UCLIDException.h>
#include <UCLIDExceptionDetailsDlg.h>
#include <ValueRestorer.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

using namespace std;

//-------------------------------------------------------------------------------------------------
// Globals / statics
//-------------------------------------------------------------------------------------------------
const UINT gICON_NOTIFY_MSG = WM_APP + 1348;

const COLORREF gBACKGROUND_COLOR = RGB(153,204,255);
const COLORREF gBUTTON_TEXT_COLOR = RGB(0, 0, 0);
const COLORREF gBUTTON_BK_COLOR(gBACKGROUND_COLOR);
const COLORREF gBUTTON_BK_DISABLED_COLOR = RGB(210, 233, 255);
const COLORREF gTITLE_TEXT_COLOR = gBACKGROUND_COLOR;
const COLORREF gTITLE_BK_COLOR = RGB(0, 0, 0);

const unsigned long gulMAX_EXCEPTION_RECORDS = 500;

const long gnNUM_WINDOW_SLIDE_STEPS = 10;
const long gnSLIDE_SLEEP_TIME = 20; // milli seconds
const long gnDEFAULT_HIDE_TIME = 10; // seconds

//-------------------------------------------------------------------------------------------------
// ExceptionDlg dialog
//-------------------------------------------------------------------------------------------------
ExceptionDlg::ExceptionDlg(vector<ExceptionRecord>& rvecExceptionRecords, 
						   CMutex& rExceptionDataLock, 
						   NotifyOptions& rOptions, CWnd* pParent)
	: CDialog(ExceptionDlg::IDD, pParent),
	m_rvecExceptionRecords(rvecExceptionRecords),
	m_rExceptionDataLock(rExceptionDataLock),
	m_rOptions(rOptions),
	m_bWindowVisible(false), m_nCurrentRecordIndex(0), m_tmLastActivityTime(0),
	m_nLastBadIconID(0), m_bAllExceptionsSeen(false), m_bPreventAutoHide(false)
{
	//{{AFX_DATA_INIT(ExceptionDlg)
	m_zExceptionText = _T("");
	m_zExceptionSequence = _T("");
	m_zDateTime = _T("");
	m_zUPI = _T("");
	//}}AFX_DATA_INIT

	// update the last activity time
	updateLastActivityTime();
}
//-------------------------------------------------------------------------------------------------
void ExceptionDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(ExceptionDlg)
	DDX_Control(pDX, IDC_BUTTON_CLOSE, m_btnClose);
	DDX_Control(pDX, IDC_BUTTON_PREV, m_btnPrev);
	DDX_Control(pDX, IDC_BUTTON_NEXT, m_btnNext);
	DDX_Control(pDX, IDC_BUTTON_LAST, m_btnLast);
	DDX_Control(pDX, IDC_BUTTON_FIRST, m_btnFirst);
	DDX_Control(pDX, IDC_BUTTON_EXCEPTION_DETAILS, m_btnDetails);
	DDX_Control(pDX, IDC_BUTTON_CLEAR, m_btnClear);
	DDX_Control(pDX, IDC_BUTTON_CLEAR_ALL, m_btnClearAll);
	DDX_Text(pDX, IDC_EDIT_EXCEPTION_TEXT, m_zExceptionText);
	DDX_Text(pDX, IDC_STATIC_EXCEPTION_SEQ, m_zExceptionSequence);
	DDX_Text(pDX, IDC_STATIC_DATE_TIME, m_zDateTime);
	DDX_Text(pDX, IDC_STATIC_UPI, m_zUPI);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(ExceptionDlg, CDialog)
	//{{AFX_MSG_MAP(ExceptionDlg)
	ON_WM_CLOSE()
	ON_WM_CTLCOLOR()
	ON_BN_CLICKED(IDC_BUTTON_CLOSE, OnButtonClose)
	ON_BN_CLICKED(IDC_BUTTON_FIRST, OnButtonFirst)
	ON_BN_CLICKED(IDC_BUTTON_LAST, OnButtonLast)
	ON_BN_CLICKED(IDC_BUTTON_NEXT, OnButtonNext)
	ON_BN_CLICKED(IDC_BUTTON_PREV, OnButtonPrev)
	ON_BN_CLICKED(IDC_BUTTON_EXCEPTION_DETAILS, OnButtonDetails)
	ON_WM_TIMER()
	ON_WM_LBUTTONDOWN()
	ON_WM_MOVE()
	ON_WM_RBUTTONDOWN()
	ON_MESSAGE(gICON_NOTIFY_MSG, OnTrayNotification)
	ON_BN_CLICKED(IDC_BUTTON_CLEAR, OnButtonClear)
	ON_BN_CLICKED(IDC_BUTTON_CLEAR_ALL, OnButtonClearAll)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// ExceptionDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL ExceptionDlg::OnInitDialog() 
{
	try
	{
		CDialog::OnInitDialog();
		
		// create the system tray icon
		if (!m_systemTrayIcon.Create(this, gICON_NOTIFY_MSG, 
			"ExtractSystems Failure Detection And Reporting System", NULL, 
			IDR_DETECT_AND_REPORT_FAILURE_MENU))
		{
			// this application may be run as a service - in which
			// case we want the functionality of reporting and logging to
			// still be taking place even if we can't create the system tray
			// icon....so just log an exception and move on
			UCLIDException ue("ELI12484", "Unable to create system tray icon!");
			ue.log("", false);
		}

		// initialize the color of the window background painting brush
		m_backgroundColorBrush.CreateSolidBrush(gBACKGROUND_COLOR);
		m_titleBackgroundBrush.CreateSolidBrush(gTITLE_BK_COLOR);

		// initialize the colors of the buttons
		m_btnPrev.SetColor(gBUTTON_TEXT_COLOR, gBUTTON_BK_COLOR, gBUTTON_BK_DISABLED_COLOR);
		m_btnNext.SetColor(gBUTTON_TEXT_COLOR, gBUTTON_BK_COLOR, gBUTTON_BK_DISABLED_COLOR);
		m_btnLast.SetColor(gBUTTON_TEXT_COLOR, gBUTTON_BK_COLOR, gBUTTON_BK_DISABLED_COLOR);
		m_btnFirst.SetColor(gBUTTON_TEXT_COLOR, gBUTTON_BK_COLOR, gBUTTON_BK_DISABLED_COLOR);
		m_btnDetails.SetColor(gBUTTON_TEXT_COLOR, gBUTTON_BK_COLOR, gBUTTON_BK_DISABLED_COLOR);
		m_btnClear.SetColor(gBUTTON_TEXT_COLOR, gBUTTON_BK_COLOR, gBUTTON_BK_DISABLED_COLOR);
		m_btnClearAll.SetColor(gBUTTON_TEXT_COLOR, gBUTTON_BK_COLOR, gBUTTON_BK_DISABLED_COLOR);
		m_btnClose.SetColor(gBUTTON_TEXT_COLOR, gBUTTON_BK_COLOR, gBUTTON_BK_DISABLED_COLOR);

		// store the original window size and calculate the size of the slide step
		// for each "step" that is used to slide the window into/out of view.
		GetWindowRect(&m_originalWindowSize);		
		m_nStepHeightInPixels =  m_originalWindowSize.Height() / 
			gnNUM_WINDOW_SLIDE_STEPS;
		
		// set the window position to be in the bottom right side of the desktop
		RECT workAreaRect;
		if (SystemParametersInfo(SPI_GETWORKAREA, 0, &workAreaRect, 0))
		{
			CRect workArea(workAreaRect);
			CRect newRect;
			newRect.right = workArea.right;
			newRect.left = newRect.right - m_originalWindowSize.Width();
			newRect.bottom = workArea.bottom;
			newRect.top = newRect.bottom;
			MoveWindow(&newRect);
		}

		// set window to be always on top
		SetWindowPos(&wndTopMost, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);

		// refresh the UI
		refreshUI();

		// initialize the timer that would detect that the window has
		// been up for some amount of time and automaticall hide it
		SetTimer(1, 500, NULL);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11901")

	return TRUE;  // return TRUE unless you set the focus to a control
		          // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void ExceptionDlg::OnCancel() 
{
	// ignore
}
//-------------------------------------------------------------------------------------------------
void ExceptionDlg::OnOK() 
{
	// ignore
}
//-------------------------------------------------------------------------------------------------
void ExceptionDlg::OnClose() 
{
	try
	{
		hideWindow();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11902")
}
//-------------------------------------------------------------------------------------------------
void ExceptionDlg::OnButtonClose() 
{
	try
	{
		// if the user clicked the close button, that means they are
		// saying "I have seen all exceptions.  Hide the window".
		m_bAllExceptionsSeen = true;

		CSingleLock lock(&m_rExceptionDataLock, TRUE);
		if (!m_rvecExceptionRecords.empty())
		{
			m_systemTrayIcon.SetIcon(IDI_ICON_EXCEPTIONS_SEEN);
		}

		OnClose();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11903")
}
//-------------------------------------------------------------------------------------------------
LRESULT ExceptionDlg::OnTrayNotification(WPARAM wParam, LPARAM lParam)
{
	try
	{
		return m_systemTrayIcon.OnTrayNotification(wParam, lParam);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11904")

	THROW_LOGIC_ERROR_EXCEPTION("ELI11915")
	return NULL; // prevent warning C4715
}
//-------------------------------------------------------------------------------------------------
void ExceptionDlg::OnTimer(UINT nIDEvent)
{
	try
	{
		// get the current time
		time_t now;
		time(&now);

		// if it is time to hide the window, do so
		if (now - m_tmLastActivityTime > gnDEFAULT_HIDE_TIME &&
			!m_bPreventAutoHide)
		{
			hideWindow();
		}

		// check if the exceptions vector is empty
		bool bVectorEmpty = false;
		{
			CSingleLock lock(&m_rExceptionDataLock, TRUE);
			bVectorEmpty = m_rvecExceptionRecords.empty();
		}

		// blink the system tray icon if new exceptions exist
		if (!bVectorEmpty && !m_bAllExceptionsSeen)
		{
			switch (m_nLastBadIconID)
			{
			case IDI_ICON_NONE:
				m_nLastBadIconID = IDI_ICON_NEW_EXCEPTIONS;
				break;
			case IDI_ICON_NEW_EXCEPTIONS:
				m_nLastBadIconID = IDI_ICON_NONE;
				break;
			default:
				{
					UCLIDException ue("ELI11896", "Unexpected switch default encountered!");
					ue.addDebugInfo("m_nLastBadIconID", m_nLastBadIconID);
					ue.log();
				}
				m_nLastBadIconID = IDI_ICON_NEW_EXCEPTIONS;
			}

			m_systemTrayIcon.SetIcon(m_nLastBadIconID);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11905")
}
//-------------------------------------------------------------------------------------------------
HBRUSH ExceptionDlg::OnCtlColor(CDC* pDC, CWnd* pWnd, UINT nCtlColor) 
{
	try
	{
		if (pWnd->GetDlgCtrlID() == IDC_STATIC_TITLE)
		{
			pDC->SetBkColor(gTITLE_BK_COLOR);
			pDC->SetTextColor(gTITLE_TEXT_COLOR);
			return m_titleBackgroundBrush;
		}
		else
		{
			pDC->SetBkColor(gBACKGROUND_COLOR);
			return m_backgroundColorBrush;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11906")

	// we should never reach here
	THROW_LOGIC_ERROR_EXCEPTION("ELI11914")
	return NULL; // prevent warning C4715
}
//-------------------------------------------------------------------------------------------------
void ExceptionDlg::OnButtonFirst() 
{
	try
	{
		CSingleLock lock(&m_rExceptionDataLock, TRUE);

		// show the first exception
		if (m_rvecExceptionRecords.size() > 0 && m_nCurrentRecordIndex != 0)
		{
			m_nCurrentRecordIndex = 0;
			refreshUI();
		}

		// update the last activity time
		updateLastActivityTime();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11907")
}
//-------------------------------------------------------------------------------------------------
void ExceptionDlg::OnButtonLast() 
{
	try
	{
		CSingleLock lock(&m_rExceptionDataLock, TRUE);

		// show the last exception
		long nNumExceptions = m_rvecExceptionRecords.size();
		if (nNumExceptions > 0 && m_nCurrentRecordIndex != nNumExceptions - 1)
		{
			m_nCurrentRecordIndex = nNumExceptions - 1;
			refreshUI();
		}

		// update the last activity time
		updateLastActivityTime();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11908")
}
//-------------------------------------------------------------------------------------------------
void ExceptionDlg::OnButtonNext() 
{
	try
	{
		CSingleLock lock(&m_rExceptionDataLock, TRUE);

		// show the next exception
		unsigned long ulNumExceptions = m_rvecExceptionRecords.size();
		if (ulNumExceptions > 0 && m_nCurrentRecordIndex < ulNumExceptions - 1)
		{
			m_nCurrentRecordIndex++;
			refreshUI();
		}

		// update the last activity time
		updateLastActivityTime();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11909")
}
//-------------------------------------------------------------------------------------------------
void ExceptionDlg::OnButtonPrev() 
{
	try
	{
		CSingleLock lock(&m_rExceptionDataLock, TRUE);

		// show the previous exception
		long nNumExceptions = m_rvecExceptionRecords.size();
		if (nNumExceptions > 0 && m_nCurrentRecordIndex > 0)
		{
			m_nCurrentRecordIndex--;
			refreshUI();
		}

		// update the last activity time
		updateLastActivityTime();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11910")
}
//-------------------------------------------------------------------------------------------------
void ExceptionDlg::OnButtonDetails() 
{
	try
	{
		UCLIDException ue;
		bool bVectorEmpty = false;

		// access the current exception record
		{
			CSingleLock lock(&m_rExceptionDataLock, TRUE);

			bVectorEmpty = m_rvecExceptionRecords.empty();
			if (!bVectorEmpty)
			{
				ue = m_rvecExceptionRecords[m_nCurrentRecordIndex].getException();
			}
		}
		
		if (!bVectorEmpty)
		{
			// we want to prevent the auto-hide for the duration 
			// that the exception details are displayed
			ValueRestorer<bool> restoreValue(m_bPreventAutoHide);
			m_bPreventAutoHide = true;

			// display the current exception
			UCLIDExceptionDetailsDlg dlg(ue, GetDesktopWindow());
			dlg.DoModal();
		}

		// update the last activity time
		updateLastActivityTime();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11911")
}
//-------------------------------------------------------------------------------------------------
void ExceptionDlg::OnButtonClear() 
{
	try
	{
		CSingleLock lock(&m_rExceptionDataLock, TRUE);

		if (!m_rvecExceptionRecords.empty())
		{
			// erase the entry at the current location
			m_rvecExceptionRecords.erase(m_rvecExceptionRecords.begin() + m_nCurrentRecordIndex);

			// if the vector does not contain enough entries for the current 
			// position to be valid, then adjust the current position
			if (m_nCurrentRecordIndex > m_rvecExceptionRecords.size() - 1)
			{
				m_nCurrentRecordIndex--;
			}

			// refresh the UI
			refreshUI();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11912")
}
//-------------------------------------------------------------------------------------------------
void ExceptionDlg::OnButtonClearAll() 
{
	try
	{
		CSingleLock lock(&m_rExceptionDataLock, TRUE);

		if (!m_rvecExceptionRecords.empty())
		{
			// clear the vector and hide the window
			m_rvecExceptionRecords.clear();
			
			// refresh the UI
			refreshUI();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11913")
}

//-------------------------------------------------------------------------------------------------
// ExceptionDlg other methods
//-------------------------------------------------------------------------------------------------
void ExceptionDlg::notifyLoggedException(const string& strStringizedException, 
										 const UPI& upi, bool bForceShowWindow)
{
	CSingleLock lock(&m_rExceptionDataLock, TRUE);

	// create the new exception record
	ExceptionRecord record(strStringizedException, upi);
	m_rvecExceptionRecords.push_back(record);

	// if we have more than the desired maximum number of records,
	// remove the oldest one
	if (m_rvecExceptionRecords.size() > gulMAX_EXCEPTION_RECORDS)
	{
		// remove the first entry
		m_rvecExceptionRecords.erase(m_rvecExceptionRecords.begin());
	}

	if (!m_bWindowVisible)
	{
		// if the window is not visible, then by default show the most
		// recently logged exception.
		m_nCurrentRecordIndex = m_rvecExceptionRecords.size() - 1;
	}

	// update the time of the last activity
	updateLastActivityTime();

	// a new exception has come in, and there is no guarantee that it
	// was seen by the user (because the user may not be at the workstation)
	// So, update m_bAllExceptionsSeen accordingly.
	m_bAllExceptionsSeen = false;

	// refresh the text entities in the UI
	refreshUI(bForceShowWindow);
}
//-------------------------------------------------------------------------------------------------
void ExceptionDlg::refreshUI(bool bForceShowWindow)
{
	if (m_rvecExceptionRecords.empty())
	{
		// hide the window if there are no more exception records.
		hideWindow();

		// empty out the fields
		m_zExceptionSequence = "No exceptions";
		m_zDateTime = m_zExceptionText = "";
		m_zUPI = "";
		UpdateData(FALSE);

		// indicate "good" status in the system tray icon
		m_systemTrayIcon.SetIcon(IDI_ICON_NO_EXCEPTIONS);
	}
	else
	{
		// show the window if appropriate, as it may be hidden
		bool bShowPopupWhenExceptionLogged = m_rOptions.notificationIsEnabled(
			kPopupNotification, kExceptionLogged);
		if (bShowPopupWhenExceptionLogged || bForceShowWindow)
		{
			showWindow();
		}

		// indicate "bad" status in the system tray icon
		if (!m_bAllExceptionsSeen)
		{
			m_systemTrayIcon.SetIcon(IDI_ICON_NEW_EXCEPTIONS);
			m_nLastBadIconID = IDI_ICON_NEW_EXCEPTIONS;
		}

		// get the current exception record
		const ExceptionRecord& record = m_rvecExceptionRecords[m_nCurrentRecordIndex];

		m_zExceptionSequence.Format("Exception %d of %d", m_nCurrentRecordIndex + 1, 
			m_rvecExceptionRecords.size());

		CTime tempTime(record.getTime());
		m_zDateTime = tempTime.Format("%m/%d/%Y %H:%M:%S");
		m_zExceptionText = record.getException().getTopText().c_str();
		m_zUPI = record.getUPI().getUPI().c_str();

		UpdateData(FALSE);
	}

	// refresh the enabled/disabled state of the buttons
	refreshButtonsState();
}
//-------------------------------------------------------------------------------------------------
void ExceptionDlg::showWindow()
{
	if (!m_bWindowVisible)
	{
		// show the window so that the slide effect can be seen
		ShowWindow(SW_SHOW);

		// if the window is being shown, it's because of a user-activity
		// so update the last activity time
		updateLastActivityTime();

		// get the desktop work area window
		RECT workAreaRect;
		if (SystemParametersInfo(SPI_GETWORKAREA, 0, &workAreaRect, 0))
		{
			CRect workArea(workAreaRect);
			CRect newRect;
			newRect.right = workArea.right;
			newRect.left = newRect.right - m_originalWindowSize.Width();
			newRect.bottom = workArea.bottom;

			// slide the window into view
			for (int i = 1; i < gnNUM_WINDOW_SLIDE_STEPS - 1; i++)
			{
				newRect.top = newRect.bottom - i * m_nStepHeightInPixels;
				MoveWindow(&newRect);
				Sleep(gnSLIDE_SLEEP_TIME);
			}

			// perform the last step to slide the window completely into view
			if (gnNUM_WINDOW_SLIDE_STEPS > 1)
			{
				newRect.top = newRect.bottom - m_originalWindowSize.Height();
				MoveWindow(&newRect);
				Sleep(gnSLIDE_SLEEP_TIME);
			}
		}

		// mark this window as visible
		m_bWindowVisible = true;
	}
}
//-------------------------------------------------------------------------------------------------
void ExceptionDlg::hideWindow()
{
	if (m_bWindowVisible)
	{
		// get the desktop work area window
		RECT workAreaRect;
		if (SystemParametersInfo(SPI_GETWORKAREA, 0, &workAreaRect, 0))
		{
			CRect workArea(workAreaRect);
			CRect newRect;
			newRect.right = workArea.right;
			newRect.left = newRect.right - m_originalWindowSize.Width();
			newRect.bottom = workArea.bottom;

			// slide the window into view
			for (int i = gnNUM_WINDOW_SLIDE_STEPS - 1; i >=0; i--)
			{
				newRect.top = newRect.bottom - i * m_nStepHeightInPixels;
				MoveWindow(&newRect);
				Sleep(gnSLIDE_SLEEP_TIME);
			}
		}

		// simple show/hide functionality for now.
		ShowWindow(SW_HIDE);

		m_bWindowVisible = false;
	}
}
//-------------------------------------------------------------------------------------------------
void ExceptionDlg::refreshButtonsState()
{	
	if (!m_rvecExceptionRecords.empty())
	{
		// if there are exception records, that means that an exception
		// is being displayed.  Enable the exception related buttons
		GetDlgItem(IDC_BUTTON_EXCEPTION_DETAILS)->EnableWindow(TRUE);
		GetDlgItem(IDC_BUTTON_CLEAR)->EnableWindow(TRUE);
		GetDlgItem(IDC_BUTTON_CLEAR_ALL)->EnableWindow(TRUE);

		// if we are not currently displaying the first item, then enable 
		// the prev/first navigation buttons
		if (m_nCurrentRecordIndex > 0)
		{
			GetDlgItem(IDC_BUTTON_FIRST)->EnableWindow(TRUE);
			GetDlgItem(IDC_BUTTON_PREV)->EnableWindow(TRUE);
		}
		else
		{
			GetDlgItem(IDC_BUTTON_FIRST)->EnableWindow(FALSE);
			GetDlgItem(IDC_BUTTON_PREV)->EnableWindow(FALSE);
		}

		// if we are not currently displaying the last item, then enable
		// the next/last navigation buttons
		if (m_nCurrentRecordIndex < m_rvecExceptionRecords.size() - 1)
		{
			GetDlgItem(IDC_BUTTON_NEXT)->EnableWindow(TRUE);
			GetDlgItem(IDC_BUTTON_LAST)->EnableWindow(TRUE);
		}
		else
		{
			GetDlgItem(IDC_BUTTON_NEXT)->EnableWindow(FALSE);
			GetDlgItem(IDC_BUTTON_LAST)->EnableWindow(FALSE);
		}
	}
	else
	{
		// disable all navigation and exception related buttons
		GetDlgItem(IDC_BUTTON_FIRST)->EnableWindow(FALSE);
		GetDlgItem(IDC_BUTTON_PREV)->EnableWindow(FALSE);
		GetDlgItem(IDC_BUTTON_NEXT)->EnableWindow(FALSE);
		GetDlgItem(IDC_BUTTON_LAST)->EnableWindow(FALSE);
		GetDlgItem(IDC_BUTTON_EXCEPTION_DETAILS)->EnableWindow(FALSE);
		GetDlgItem(IDC_BUTTON_CLEAR)->EnableWindow(FALSE);
		GetDlgItem(IDC_BUTTON_CLEAR_ALL)->EnableWindow(FALSE);
	}
}
//-------------------------------------------------------------------------------------------------
void ExceptionDlg::updateLastActivityTime()
{
	// set the current time to be the time of the last activity.
	time( &m_tmLastActivityTime );
}
//-------------------------------------------------------------------------------------------------
