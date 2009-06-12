#include "StdAfx.h"
#include "ProgressDlg.h"
#include "IDShieldLF.h"

#include <COMUtils.h>
#include <UCLIDException.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
static const int nPROGRESS_LEVELS = 2;
static const int nREFRESH_DELAY = 100;

//--------------------------------------------------------------------------------------------------
// CProgressDlg
//--------------------------------------------------------------------------------------------------
CProgressDlg::CProgressDlg(IProgressStatusPtr ipProgressStatus, HWND hwndParent/* = NULL*/,
						   HANDLE hStopEvent/* = NULL*/)
: m_hwndParent(hwndParent)
, m_ipProgressStatus(ipProgressStatus)
, m_hStopEvent(hStopEvent)
{
	// Worker Thread //
	try
	{
		ASSERT_ARGUMENT("ELI21186", ipProgressStatus != NULL);

		// Launch the UI in a separate thread.
		AfxBeginThread(showProgressStatus, this);

		// Wait until the UI has been initialized before returning.
		m_eventInitialized.wait();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI21185");
}
//--------------------------------------------------------------------------------------------------
CProgressDlg::~CProgressDlg(void)
{
	// Worker Thread //
	try
	{
		// Signal the UI thread to end.
		m_eventDestroy.signal();

		m_eventUIThreadFinished.wait();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI21181");
}
//--------------------------------------------------------------------------------------------------
void CProgressDlg::Close()
{
	// Worker Thread //
	if (m_ipProgressStatusDlg)
	{
		m_ipProgressStatusDlg->Close();
	}
}
//--------------------------------------------------------------------------------------------------
UINT CProgressDlg::showProgressStatus(LPVOID pParam)
{
	// UI Thread //
	
	// Used to enable/disable the parent window; Outside the scope of the try block in case of exception.
	HWND hwndParent = NULL;
	CProgressDlg *pProgressDlg = (CProgressDlg *)pParam;

	try
	{
		ASSERT_ARGUMENT("ELI21187", pProgressDlg != NULL);

		// Create and show the progress status dialog
		IProgressStatusDialogPtr ipProgressStatusDialog(CLSID_ProgressStatusDialog);
		ASSERT_RESOURCE_ALLOCATION("ELI21184", ipProgressStatusDialog != NULL);

		pProgressDlg->m_ipProgressStatusDlg = ipProgressStatusDialog;

		ipProgressStatusDialog->ShowModelessDialog(pProgressDlg->m_hwndParent, 
			gstrPRODUCT_NAME.c_str(), pProgressDlg->m_ipProgressStatus, nPROGRESS_LEVELS, 
			nREFRESH_DELAY, VARIANT_FALSE, pProgressDlg->m_hStopEvent);

		hwndParent = pProgressDlg->m_hwndParent;
		if (hwndParent != NULL)
		{
			// If a parent is supplied, simulate application modality by disabling the parent window.
			::EnableWindow(hwndParent, FALSE);
		}

		// Signal the constructor that the UI is now prepared.
		pProgressDlg->m_eventInitialized.signal();

		// Need to use the messageWait instead of wait because without a message loop the progress
		// status dialog will not receive the events it needs to be a reactive UI element.
		pProgressDlg->m_eventDestroy.messageWait();

		// Close the progress status dialog
		ipProgressStatusDialog->Close();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI21226");

	// If the parent window handle was specified, attempt to re-enable the parent window.
	if (hwndParent != NULL)
	{
		try
		{
			::EnableWindow(hwndParent, TRUE);

			// The way the code is currently, it leaves the Client without focus when the progress
			// window is closed.  Set focus back to the Client.
			::SetFocus(hwndParent);
		}
		CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI21230"); 
	}

	// Signal worker thread that the UI thread is finished. This must come after the EnableWindow
	// call to ensure the calling thread doesn't end before the parent window is enabled.
	try
	{
		if (pProgressDlg != NULL)
		{
			pProgressDlg->m_eventUIThreadFinished.signal();
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI21231");

	return 0;
}
//--------------------------------------------------------------------------------------------------