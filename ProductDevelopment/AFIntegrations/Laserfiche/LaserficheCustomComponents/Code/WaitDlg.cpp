// WaitDlg.cpp : implementation file
//

#include "stdafx.h"
#include "LaserficheCustomComponents.h"
#include "WaitDlg.h"

#include <UCLIDException.h>

const int WM_CLOSE_DIALOG	= WM_USER + 100;
const int WM_UPDATE_MESSAGE	= WM_USER + 101;

//--------------------------------------------------------------------------------------------------
// CWaitDlg
//--------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CWaitDlg, CDialog)
//--------------------------------------------------------------------------------------------------
CWaitDlg::CWaitDlg(const string &strMessage, CWnd* pParent/* =NULL*/)
	: CDialog(CWaitDlg::IDD, pParent)
{
	// Caller's thread //
	try
	{
		m_eventIsInitialized.reset();
		m_eventUIThreadFinished.reset();

		showMessage(strMessage);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20855");
}
//--------------------------------------------------------------------------------------------------
CWaitDlg::CWaitDlg(CWnd* pParent/* = NULL*/)
	: CDialog(CWaitDlg::IDD, pParent)
{
	try
	{
		m_eventIsInitialized.reset();
		m_eventUIThreadFinished.reset();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI21040");
}
//--------------------------------------------------------------------------------------------------
CWaitDlg::~CWaitDlg()
{
	// Caller's thread //
	try
	{
		close();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20853");
}
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CWaitDlg, CDialog)
	ON_WM_DESTROY()
	ON_MESSAGE(WM_CLOSE_DIALOG, OnCloseDialog)
	ON_MESSAGE(WM_UPDATE_MESSAGE, OnUpdateMessage)
END_MESSAGE_MAP()
//--------------------------------------------------------------------------------------------------
void CWaitDlg::showMessage(const string &strMessage)
{	
	// Caller's thread //

	m_zMessage = strMessage.c_str();

	if (m_eventIsInitialized.isSignaled())
	{
		// Ensure the wait dialog is visible.  (Currently CIDShieldLF::ConnectPrompt is hiding it
		// to solve a window layering issue).
		::ShowWindow(m_hWnd, SW_SHOW);

		PostMessage(WM_UPDATE_MESSAGE, 0, 0);
	}
	else // Need to initialize message box
	{
		m_eventUIThreadFinished.reset();

		// Display the message box in a separate thread so the box isn't unresponsive (locked-up)
		// while processing is going on.
		AfxBeginThread(showDialog, this);

		// Wait for dialog to initialize to ensure destructor can't be called on an invalid window.
		m_eventIsInitialized.wait();
	}
}
//--------------------------------------------------------------------------------------------------
void CWaitDlg::show()
{
	// Caller's thread //

	if (m_hWnd == NULL || !asCppBool(::IsWindow(m_hWnd)))
	{
		throw UCLIDException("ELI21923", "Dialog not initialized!");
	}
	
	::ShowWindow(m_hWnd, SW_SHOW);
}
//--------------------------------------------------------------------------------------------------
void CWaitDlg::hide()
{
	// Caller's thread //

	if (m_hWnd != NULL && asCppBool(::IsWindow(m_hWnd)))
	{
		::ShowWindow(m_hWnd, SW_HIDE);
	}
}
//--------------------------------------------------------------------------------------------------
void CWaitDlg::close()
{
	// Caller's thread //

	// The dialog needs to close to end the UI thread.
	if (m_eventIsInitialized.isSignaled())
	{
		PostMessage(WM_CLOSE_DIALOG, 0, 0);
		
		// Wait for the dialog to close
		m_eventUIThreadFinished.wait();

		m_eventIsInitialized.reset();
	}
}

//--------------------------------------------------------------------------------------------------
// Overrides
//--------------------------------------------------------------------------------------------------
void CWaitDlg::DoDataExchange(CDataExchange* pDX)
{
	// UI thread //
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CDialog::DoDataExchange(pDX);
		DDX_Text(pDX, IDC_STATIC_MESSAGE, m_zMessage);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20919");
}
//--------------------------------------------------------------------------------------------------
BOOL CWaitDlg::OnInitDialog()
{
	// UI thread //
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CDialog::OnInitDialog();

		// Signal caller thread that the dialog is displayed
		m_eventIsInitialized.signal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20918");

	return TRUE;
}

//--------------------------------------------------------------------------------------------------
// Message Handlers
//--------------------------------------------------------------------------------------------------
LRESULT CWaitDlg::OnCloseDialog(WPARAM wParam, LPARAM lParam)
{
	// UI thread //
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20923");
	
	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CWaitDlg::OnUpdateMessage(WPARAM wParam, LPARAM lParam)
{
	// UI thread //
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UpdateData(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI21039");
	
	return 0;
}

//--------------------------------------------------------------------------------------------------
// Private
//--------------------------------------------------------------------------------------------------
UINT CWaitDlg::showDialog(LPVOID pData)
{
	// UI thread //
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CWaitDlg *pTheDlg = (CWaitDlg *)pData;
		pTheDlg->DoModal();
		// Signal the caller thread that the UI thread is now finished
		pTheDlg->m_eventUIThreadFinished.signal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20917");

	return 0;
}
//--------------------------------------------------------------------------------------------------