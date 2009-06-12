// ExtractTRPDlg.cpp : implementation file
//

#include "stdafx.h"
#include "ExtractTRP2.h"
#include "ExtractTRPDlg.h"

#include <ExtractTRP2Constants.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// CExtractTRPDlg dialog
//-------------------------------------------------------------------------------------------------
CExtractTRPDlg::CExtractTRPDlg(CWnd* pParent /*=NULL*/)
: CDialog(CExtractTRPDlg::IDD, pParent),
  m_TRP(m_stateIsInvalidEvent)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}
//-------------------------------------------------------------------------------------------------
void CExtractTRPDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CExtractTRPDlg, CDialog)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_MESSAGE(gGET_AUTHENTICATION_CODE_MSG, OnGetAuthenticationCode)
	ON_MESSAGE(gSTATE_IS_VALID_MSG, OnStateIsValid)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CExtractTRPDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CExtractTRPDlg::OnInitDialog()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	
	try
	{
		CDialog::OnInitDialog();

		// Hide the window.
		MoveWindow(0, 0, 0, 0);

		// Set the caption to be the exact string that the LicenseMgmt class
		// will be looking for
		SetWindowText( gstrTRP_WINDOW_TITLE.c_str() );

		// Set the icon for this dialog.  The framework does this automatically
		//  when the application's main window is not a dialog
		SetIcon(m_hIcon, TRUE);			// Set big icon
		SetIcon(m_hIcon, FALSE);		// Set small icon
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI18622");

	return TRUE;  // return TRUE  unless you set the focus to a control
}
//-------------------------------------------------------------------------------------------------
// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.
void CExtractTRPDlg::OnPaint()
{
	if (IsIconic())
	{
		CPaintDC dc(this); // device context for painting

		SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

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
// The system calls this function to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CExtractTRPDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}

//-------------------------------------------------------------------------------------------------
// Custom TRP message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CExtractTRPDlg::OnGetAuthenticationCode(WPARAM wParam, LPARAM lParam)
{
	try
	{
		// Return encrypted day code
		return (LRESULT) asUnsignedLong(LICENSE_MGMT_PASSWORD);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI15472");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CExtractTRPDlg::OnStateIsValid(WPARAM wParam, LPARAM lParam)
{
	try
	{
		// Check the state
		if (stateIsValid())
		{
			// If we have reached here, state is valid
			return (LRESULT) guiVALID_STATE_CODE;
		}
		// else just return the invalid state return code below
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI15473");

	// This is an invalid state return code
	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
bool CExtractTRPDlg::stateIsValid()
{
	// return the state of the licensing depending upon
	// whether the event has been signaled
	return m_stateIsInvalidEvent.isSignaled() ? false : true;
}
//-------------------------------------------------------------------------------------------------
