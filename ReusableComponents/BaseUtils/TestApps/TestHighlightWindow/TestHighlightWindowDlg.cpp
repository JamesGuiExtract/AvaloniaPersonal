// TestHighlightWindowDlg.cpp : implementation file
//

#include "stdafx.h"
#include "TestHighlightWindow.h"
#include "TestHighlightWindowDlg.h"
#include "Dlg1.h"
#include "Dlg2.h"
#include "Dlg3.h"
#include "Dlg4.h"
#include <HighlightWindow.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

COLORREF DISABLED_COLOR = RGB(255, 128, 128);

/////////////////////////////////////////////////////////////////////////////
// CTestHighlightWindowDlg dialog

CTestHighlightWindowDlg::CTestHighlightWindowDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CTestHighlightWindowDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(CTestHighlightWindowDlg)
	m_iShowTransparentWindow = -1;
	//}}AFX_DATA_INIT
	// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}

void CTestHighlightWindowDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CTestHighlightWindowDlg)
	DDX_Radio(pDX, IDC_RADIO_NO, m_iShowTransparentWindow);
	//}}AFX_DATA_MAP
}

BEGIN_MESSAGE_MAP(CTestHighlightWindowDlg, CDialog)
	//{{AFX_MSG_MAP(CTestHighlightWindowDlg)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_BN_CLICKED(IDC_BUTTON1, OnButton1)
	ON_BN_CLICKED(IDC_BUTTON2, OnButton2)
	ON_BN_CLICKED(IDC_BUTTON3, OnButton3)
	ON_BN_CLICKED(IDC_BUTTON4, OnButton4)
	ON_BN_CLICKED(IDC_RADIO_NO, OnRadioNo)
	ON_BN_CLICKED(IDC_RADIO_YES, OnRadioYes)
	ON_BN_CLICKED(IDC_BUTTON5, OnButton5)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CTestHighlightWindowDlg message handlers

BOOL CTestHighlightWindowDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon
	
	// if window transparency is not supported, then make
	// the transparency-override related controls invisible
	int iShowMode = HighlightWindow::sGetInstance().
		windowTransparencyIsSupported() ? SW_SHOW : SW_HIDE;
	GetDlgItem(IDC_RADIO_YES)->ShowWindow(iShowMode);
	GetDlgItem(IDC_RADIO_NO)->ShowWindow(iShowMode);
	GetDlgItem(IDC_TRANSPARENCY)->ShowWindow(iShowMode);

	m_iShowTransparentWindow = 1;
	UpdateData(FALSE);

	return TRUE;  // return TRUE  unless you set the focus to a control
}

// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.

void CTestHighlightWindowDlg::OnPaint() 
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
HCURSOR CTestHighlightWindowDlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}

void CTestHighlightWindowDlg::OnButton1() 
{
	Dlg1 *pDlg = new Dlg1();
	pDlg->Create(Dlg1::IDD);
	pDlg->ShowWindow(SW_SHOW);
}

void CTestHighlightWindowDlg::OnButton2() 
{
	Dlg2 *pDlg = new Dlg2();
	pDlg->Create(Dlg2::IDD);
	pDlg->ShowWindow(SW_SHOW);
}

void CTestHighlightWindowDlg::OnButton3() 
{
	Dlg3 *pDlg = new Dlg3();
	pDlg->Create(Dlg3::IDD);
	pDlg->ShowWindow(SW_SHOW);
}

void CTestHighlightWindowDlg::OnButton4() 
{
	Dlg4 *pDlg = new Dlg4();
	pDlg->Create(Dlg4::IDD);
	pDlg->ShowWindow(SW_SHOW);
}

void CTestHighlightWindowDlg::OnRadioNo() 
{
	HighlightWindow::sGetInstance().setHighlightType(
		HighlightWindow::kHighlightUsingBorder);
}

void CTestHighlightWindowDlg::OnRadioYes() 
{
	HighlightWindow::sGetInstance().setHighlightType(
		HighlightWindow::kHighlightUsingTransparency);
}

void CTestHighlightWindowDlg::OnButton5() 
{
	HighlightWindow::sGetInstance().hide();
}
