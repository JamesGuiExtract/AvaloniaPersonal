// TutorialVC3Dlg.cpp : implementation file
//

#include "stdafx.h"
#include "TutorialVC3.h"
#include "TutorialVC3Dlg.h"

#import "COMLM.dll"
using namespace UCLID_COMLMLib;

#import "UCLIDExceptionMgmt.dll"
using namespace UCLIDEXCEPTIONMGMTLib;

#import "UCLIDCOMUtils.dll"
using namespace UCLIDCOMUTILSLib;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CTutorialVC3Dlg dialog

CTutorialVC3Dlg::CTutorialVC3Dlg(CWnd* pParent /*=NULL*/)
	: CDialog(CTutorialVC3Dlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(CTutorialVC3Dlg)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT
	// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
	
}

void CTutorialVC3Dlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CTutorialVC3Dlg)
		// NOTE: the ClassWizard will add DDX and DDV calls here
	//}}AFX_DATA_MAP
}

BEGIN_MESSAGE_MAP(CTutorialVC3Dlg, CDialog)
	//{{AFX_MSG_MAP(CTutorialVC3Dlg)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_BN_CLICKED(IDC_TEST, OnTest)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CTutorialVC3Dlg message handlers

BOOL CTutorialVC3Dlg::OnInitDialog()
{
	CDialog::OnInitDialog();
	
	try
	{
		// TODO: Add extra initialization here
		// Declare license object
		IUCLIDComponentLMPtr ipLicense( __uuidof(UCLIDComponentLM) );
		
		// TODO: Initialize license object with valid file and 4 passwords
		ipLicense->InitializeFromFile( "Insert filename here!!!", 1, 2, 3, 4 );
	}
	catch (_com_error& ex)
	{
		_bstr_t bstrDescription(ex.Description());

		ICOMUCLIDExceptionPtr	ipEx( __uuidof(COMUCLIDException) );
		ipEx->CreateFromString( "12345", bstrDescription);

		// Display the Exception
		ipEx->Display();
	}
	catch (...)
	{
		AfxMessageBox("Unknown exception caught");
	}

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon	
	
	return TRUE;  // return TRUE  unless you set the focus to a control
}

// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.

void CTutorialVC3Dlg::OnPaint() 
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

// The system calls this to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CTutorialVC3Dlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}

void CTutorialVC3Dlg::OnTest() 
{	
	try
	{
		// Create IUnknownVector object
		IIUnknownVectorPtr	ipVec( __uuidof(IUnknownVector) );

		// Call inappropriate method 
		// This will throw a UCLID Exception
		IUnknownPtr	ipTemp;
		ipTemp = ipVec->At( 0 );

	}
	catch (_com_error& ex)
	{
		// Retrieve the error description
		_bstr_t bstrDescription(ex.Description());

		// Convert the description into a UCLID Exception
		ICOMUCLIDExceptionPtr	ipEx( __uuidof(COMUCLIDException) );
		ipEx->CreateFromString( "ELI45678", bstrDescription );

		// Check UCLID Style radio button
		if (((CButton *)GetDlgItem( IDC_RADIO_UCLID ))->GetCheck() == 1)
		{
			// Just display the UCLID Exception
			ipEx->Display();
		}
		else if (((CButton *)GetDlgItem( IDC_RADIO_MESSAGE ))->GetCheck() == 1)
		{
			// Retrieve topmost exception information
			CString	zCode ( ipEx->GetTopELICode().operator const char *() );
			CString	zText( ipEx->GetTopText().operator const char *() );

			// Provide strings to message box
			CString	zTotal;
			zTotal.Format( "Error code = %s\r\nError description = %s", 
				zCode, zText );
			MessageBox( zTotal.operator LPCTSTR(), "Exception data", 
				MB_ICONEXCLAMATION | MB_OK );
		}
		else
		{
			MessageBox( "Please select a display method", "Error", 
				MB_ICONEXCLAMATION | MB_OK );
		}
	}
}
