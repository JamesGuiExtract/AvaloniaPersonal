// ProgressStatusDlg.cpp : implementation file
//

#include "stdafx.h"
// VisualStylesXP.h must be included before ProgressStatusMFCDlg.h
#include <VisualStylesXP.h>
#include "ProgressStatusMFCDlg.h"

#include <UCLIDException.h>
#include <cpputil.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
static const string gstrDEFAULT_NO_PROGRESS_STATUS_MSG = "No progress status information is available at this time.";

//--------------------------------------------------------------------------------------------------
// CProgressStatusMFCDlg dialog
//--------------------------------------------------------------------------------------------------
CProgressStatusMFCDlg::CProgressStatusMFCDlg(CWnd* pParent /*=NULL*/, long nNumProgressLevels, 
											 long nDelayBetweenRefreshes, bool bShowCloseButton,
											 HANDLE hStopEvent)
: CDialog(CProgressStatusMFCDlg::IDD, pParent), m_nNumProgressLevels(nNumProgressLevels),
m_nDelayBetweenRefreshes(nDelayBetweenRefreshes), m_bShowCloseButton(bShowCloseButton), 
m_hStopEvent(hStopEvent), m_bStopped(false)
{
}
//--------------------------------------------------------------------------------------------------
CProgressStatusMFCDlg::~CProgressStatusMFCDlg()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16600")
}

//--------------------------------------------------------------------------------------------------
// Message map and misc MFC stuff
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CProgressStatusMFCDlg, CDialog)
	ON_WM_TIMER()
	ON_WM_CLOSE()
	ON_BN_CLICKED(IDC_STOP, &CProgressStatusMFCDlg::OnBnClickedStop)
END_MESSAGE_MAP()

IMPLEMENT_DYNAMIC(CProgressStatusMFCDlg, CDialog)

//--------------------------------------------------------------------------------------------------
// Overridden MFC methods
//--------------------------------------------------------------------------------------------------
void CProgressStatusMFCDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}

//--------------------------------------------------------------------------------------------------
// Public member functions
//--------------------------------------------------------------------------------------------------
void CProgressStatusMFCDlg::setProgressStatusObject(UCLID_COMUTILSLib::IProgressStatusPtr ipProgressStatus)
{
	// Update the reference to the progress status object
	CSingleLock lock(&m_mutexProgressStatus, TRUE);
	m_ipProgressStatus = ipProgressStatus;
}
//--------------------------------------------------------------------------------------------------
UCLID_COMUTILSLib::IProgressStatusPtr CProgressStatusMFCDlg::getProgressStatusObject()
{
	// Return a reference to the progress status object
	CSingleLock lock(&m_mutexProgressStatus, TRUE);
	return m_ipProgressStatus;
}

//--------------------------------------------------------------------------------------------------
// CProgressStatusMFCDlg message handlers
//--------------------------------------------------------------------------------------------------
BOOL CProgressStatusMFCDlg::OnInitDialog()
{
	try
	{
		CDialog::OnInitDialog();

		// If a parent window was specified, display the dialog centered against its parent
		CWnd *pParent = GetParent();
		if (pParent != NULL)
		{
			CenterWindow();
		}

		// Initialize the grid control
		gridWnd.SubclassDlgItem(IDC_PROGRESS_GRID, this);

		// Using XP Themes with gridWnd causes some unwanted visual issues.  The simplest solution
		// seems to be to disable themes for the gridWnd. [P13:4665]
		g_xpStyle.SetWindowTheme(gridWnd.m_hWnd, L"", L"");

		gridWnd.Initialize();

		// Disable the grid control so that the user cannot interact with it 
		// by changing current cell selection, etc. We can still use the grid
		// to show progress information
		gridWnd.EnableWindow(FALSE);

		// Set the row and column counts, and hide the row/column headers
		gridWnd.SetRowCount(m_nNumProgressLevels * 3);
		gridWnd.SetColCount(1);
		gridWnd.HideCols(0, 0, TRUE);
		gridWnd.HideRows(0, 0, TRUE);

		// Determine the height of the default row, and use that to set the 
		// height of the window
		int iDefaultRowHeight = gridWnd.GetRowHeight(1);

		// Determine the height of the cell that is used to separate progress levels
		int iLevelSeparatorRowHeight = iDefaultRowHeight / 2;

		// Determine the height of each progress level.  Each progress level has one
		// standard-height row for the label, one standard-height row for the 
		// progress bar, and a third row whose height is iLevelSeparatorRowHeight
		int iProgressLevelHeight = iDefaultRowHeight * 2 + iLevelSeparatorRowHeight;
		
		// Size this window to exactly fit showing the required number
		// of progress status levels
		CRect rect;
		GetClientRect(&rect);
		rect.bottom = rect.top + iProgressLevelHeight * m_nNumProgressLevels;
		gridWnd.MoveWindow(&rect);
		CRect rectWindow, rectClient;
		GetWindowRect(&rectWindow);
		GetClientRect(&rectClient);
		rectWindow.bottom = rectWindow.top + rect.Height() + 
			                (rectWindow.Height() - rectClient.Height());
		
		// If a handle for a stop event has been provided, adjust the UI to show the stop
		// button.
		if (m_hStopEvent != NULL)
		{
			static const int nBUTTON_PADDING = 6;

			CButton *pStopButton = (CButton *)GetDlgItem(IDC_STOP);
			CRect rectStopBtn;
			pStopButton->GetWindowRect(&rectStopBtn);
			ScreenToClient(&rectStopBtn);
			rectStopBtn.MoveToY(rectClient.top + rect.Height() + nBUTTON_PADDING);
			pStopButton->MoveWindow(&rectStopBtn);
			pStopButton->ShowWindow(SW_SHOW);

			rectWindow.bottom += rectStopBtn.Height() + (2 * nBUTTON_PADDING);
		}

		MoveWindow(&rectWindow);

		// Configure the single column of the grid to consume the entire width of the window
		gridWnd.GetClientRect(&rect);
		gridWnd.SetColWidth(1, 1, rect.Width() - 1);

		// Remove borders on all cells by painting the border in white
		CGXPen whitePen(PS_SOLID, 1, RGB(255, 255, 255));
		gridWnd.SetStyleRange(CGXRange(1, 1, m_nNumProgressLevels * 3, 1), 
			CGXStyle().SetBorders(gxBorderAll, whitePen));
		
		// For each level, there are 3 rows in the grid.  Setup the correct styles for
		// each of the three rows.
		for (int iLevel = 0; iLevel < m_nNumProgressLevels; iLevel++)
		{
			// Set the first row to be a static label
			int iFirstRowIndex = iLevel * 3 + 1;
			gridWnd.SetStyleRange(CGXRange(iFirstRowIndex, 1), CGXStyle().SetControl(GX_IDS_CTRL_STATIC));
			
			// Set the second row to be a progress status control
			int iSecondRowIndex = iFirstRowIndex + 1;
			gridWnd.SetStyleRange(CGXRange(iSecondRowIndex, 1), CGXStyle().SetControl(GX_IDS_CTRL_PROGRESS));
			gridWnd.SetStyleRange(CGXRange(iSecondRowIndex, 1), CGXStyle().SetUserAttribute(GX_IDS_UA_PROGRESS_MIN, (LONG) 0));
			gridWnd.SetStyleRange(CGXRange(iSecondRowIndex, 1), CGXStyle().SetUserAttribute(GX_IDS_UA_PROGRESS_MAX, (LONG) 100));

			// Set the third row to be a static label
			int iThirdRowIndex = iSecondRowIndex + 1;
			gridWnd.SetStyleRange(CGXRange(iThirdRowIndex, 1), CGXStyle().SetControl(GX_IDS_CTRL_STATIC));

			// The third row is used as a "level separator".  It does not need to be as 
			// tall as the other cells.  Set the row height to be a quarter of the other
			// default row height
			gridWnd.SetRowHeight(iThirdRowIndex, iThirdRowIndex, iLevelSeparatorRowHeight);
		}
		
		// Show the close button on the dialog only if necessary.  The resource has been
		// setup by default to NOT show the close button.  This code counts on the dialog
		// being defined in the resource without the close button.
		if (m_bShowCloseButton)
		{
			LONG nStyle = GetWindowLong(this->m_hWnd, GWL_STYLE);
			nStyle |= WS_SYSMENU;
			SetWindowLong(this->m_hWnd, GWL_STYLE, nStyle);
		}

		// Start the progress status update timer
		SetTimer(ms_nREFRESH_TIMER_ID, m_nDelayBetweenRefreshes, NULL);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16594")

	return TRUE;  // return TRUE  unless you set the focus to a control
}
//--------------------------------------------------------------------------------------------------
void CProgressStatusMFCDlg::OnClose()
{
	try
	{
		// When the close button is clicked, just hide the window
		ShowWindow(SW_HIDE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16604")
}
//--------------------------------------------------------------------------------------------------
void CProgressStatusMFCDlg::OnTimer(UINT_PTR nIDEvent)
{
	try
	{
		switch (nIDEvent)
		{
		case ms_nREFRESH_TIMER_ID:
			// Refresh the progress status
			refreshProgressStatus();
			break;

		default:
			// Call the default timer event handler
			CDialog::OnTimer(nIDEvent);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16593")
}
//--------------------------------------------------------------------------------------------------
void CProgressStatusMFCDlg::OnBnClickedStop()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Stop button can only be pressed once.  Disable it.
		GetDlgItem(IDC_STOP)->EnableWindow(FALSE);

		// Signal the caller's event that the user has pressed stop.
		if (SetEvent(m_hStopEvent) == NULL)
		{
			UCLIDException ue("ELI21188", "Unable to signal stop event!");
			ue.addDebugInfo("Handle", (unsigned long) m_hStopEvent);
			throw ue;
		}

		// Keep track within this class that the stop button has been pressed.
		m_bStopped = true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI21189");
}

//--------------------------------------------------------------------------------------------------
// Private member functions
//--------------------------------------------------------------------------------------------------
void CProgressStatusMFCDlg::refreshProgressStatus()
{
	// If this window is not visible, no need to refresh progress.  Just return
	if (!asCppBool(IsWindowVisible()))
	{
		return;
	}

	// Lock access to the progress status object so that it can't be modified during the
	// refresh operation
	CSingleLock lock(&m_mutexProgressStatus, TRUE);

	UCLID_COMUTILSLib::IProgressStatusPtr ipCurrentLevelPS = m_ipProgressStatus;
	for (int iLevel = 0; iLevel < m_nNumProgressLevels; iLevel++)
	{
		// By default we don't want to show the progress bar for a particular level
		// which is the same effect as setting the % complete to zero.
		int iPercent = 0;
		int iRow = iLevel * 3 + 1;
		int iCol = 1;

		// If stopping remove all progress information and just display "Stopping..." until
		// the progress dialog is closed.
		if (m_bStopped)
		{
			if (iLevel == 0)
			{
				gridWnd.SetValueRange(CGXRange(iRow, iCol), "Stopping...");
			}
			else
			{
				gridWnd.SetValueRange(CGXRange(iRow, iCol), "");
			}

			CGXProgressCtrl::SetProgressValue(&gridWnd, iRow + 1, 1, 0);
			gridWnd.SetStyleRange(CGXRange(iRow + 1, iCol), 
				CGXStyle().SetUserAttribute(GX_IDS_UA_PROGRESS_CAPTIONMASK, ""));

			continue;
		}

		// By default, we don't want to show any label for the progress level.
		// However, for the topmost level, if there is no progress status object
		// we should show some default text indicating that there is no progress
		// status information.
		string strText = (ipCurrentLevelPS == NULL && iLevel == 0) ? 
			gstrDEFAULT_NO_PROGRESS_STATUS_MSG : "";

		// Get the text and percent complete for the current progress level
		if (ipCurrentLevelPS)
		{
			// Determine the percent complete at the current level
			iPercent = (int) (ipCurrentLevelPS->GetProgressPercent() * 100);

			// Determine the text value of the level
			strText = ipCurrentLevelPS->Text;
		}

		// Update the the text value of the level, if it is different than the current value
		CString zCurrentValue = gridWnd.GetValueRowCol(iRow, iCol);
		if (strText != (LPCTSTR) zCurrentValue)
		{
			gridWnd.SetValueRange(CGXRange(iRow, iCol), strText.c_str());
		}

		// Update the progress value of the level
		iRow = iLevel * 3 + 2;
		zCurrentValue = gridWnd.GetValueRowCol(iRow, iCol);
		if (zCurrentValue.IsEmpty() || iPercent != asLong((LPCTSTR) zCurrentValue))
		{
			// Update the progress bar value
			CGXProgressCtrl::SetProgressValue(&gridWnd, iRow, 1, iPercent);

			// Update the text of the progress bar
			// We don't want a "0%" to displayed for Level X when there are less
			// than X levels of progress status objects.  So, set the caption
			// mask to an empty string if the current level is not a valid progress
			// status level, which can be determined by examining the emptyness of strText
			string strCaptionMask = (ipCurrentLevelPS == NULL || strText.empty()) ? "" : "%d%%";
			gridWnd.SetStyleRange(CGXRange(iRow, 1), 
				CGXStyle().SetUserAttribute(GX_IDS_UA_PROGRESS_CAPTIONMASK, strCaptionMask.c_str()));
		}

		// Set the current level progress status object to the 
		// next level progress status object
		if (ipCurrentLevelPS)
		{
			ipCurrentLevelPS = ipCurrentLevelPS->SubProgressStatus;
		}
	}
}
//--------------------------------------------------------------------------------------------------
