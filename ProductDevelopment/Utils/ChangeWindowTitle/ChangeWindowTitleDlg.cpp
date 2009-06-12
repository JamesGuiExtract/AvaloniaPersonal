// ChangeWindowTitleDlg.cpp : implementation file
//

#include "stdafx.h"
#include "ChangeWindowTitle.h"
#include "ChangeWindowTitleDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CAboutDlg dialog used for App About

class CAboutDlg : public CDialog
{
public:
	CAboutDlg();

// Dialog Data
	//{{AFX_DATA(CAboutDlg)
	enum { IDD = IDD_ABOUTBOX };
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CAboutDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	//{{AFX_MSG(CAboutDlg)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

CAboutDlg::CAboutDlg() : CDialog(CAboutDlg::IDD)
{
	//{{AFX_DATA_INIT(CAboutDlg)
	//}}AFX_DATA_INIT
}

void CAboutDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CAboutDlg)
	//}}AFX_DATA_MAP
}

BEGIN_MESSAGE_MAP(CAboutDlg, CDialog)
	//{{AFX_MSG_MAP(CAboutDlg)
		// No message handlers
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CChangeWindowTitleDlg dialog

CChangeWindowTitleDlg::CChangeWindowTitleDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CChangeWindowTitleDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(CChangeWindowTitleDlg)
	m_EditTextFrom = _T("");
	m_EditTextTo = _T("");
	m_EditTextFind = _T("");
	//}}AFX_DATA_INIT
	// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}

void CChangeWindowTitleDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CChangeWindowTitleDlg)
	DDX_Text(pDX, IDC_EDIT_TEXT_FROM, m_EditTextFrom);
	DDX_Text(pDX, IDC_EDIT_TEXT_TO, m_EditTextTo);
	DDX_Text(pDX, IDC_EDIT_FIND_TEXT, m_EditTextFind);
	//}}AFX_DATA_MAP
}

BEGIN_MESSAGE_MAP(CChangeWindowTitleDlg, CDialog)
	//{{AFX_MSG_MAP(CChangeWindowTitleDlg)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_WM_CLOSE()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CChangeWindowTitleDlg message handlers

BOOL CChangeWindowTitleDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	// Add "About..." menu item to system menu.

	// IDM_ABOUTBOX must be in the system command range.
	ASSERT((IDM_ABOUTBOX & 0xFFF0) == IDM_ABOUTBOX);
	ASSERT(IDM_ABOUTBOX < 0xF000);

	CMenu* pSysMenu = GetSystemMenu(FALSE);
	if (pSysMenu != NULL)
	{
		CString strAboutMenu;
		strAboutMenu.LoadString(IDS_ABOUTBOX);
		if (!strAboutMenu.IsEmpty())
		{
			pSysMenu->AppendMenu(MF_SEPARATOR);
			pSysMenu->AppendMenu(MF_STRING, IDM_ABOUTBOX, strAboutMenu);
		}
	}

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon
	
	// TODO: Add extra initialization here
	char pszTemp[128];
	GetPrivateProfileString("Defaults", "TitleFind", "", pszTemp, sizeof(pszTemp), "ChangeWindowTitle.Ini");
	m_EditTextFind = _T(pszTemp);
	GetPrivateProfileString("Defaults", "TitleFrom", "", pszTemp, sizeof(pszTemp), "ChangeWindowTitle.Ini");
	m_EditTextFrom = _T(pszTemp);
	GetPrivateProfileString("Defaults", "TitleTo", "", pszTemp, sizeof(pszTemp), "ChangeWindowTitle.Ini");
	m_EditTextTo = _T(pszTemp);
	UpdateData(FALSE);

	return TRUE;  // return TRUE  unless you set the focus to a control
}

void CChangeWindowTitleDlg::OnSysCommand(UINT nID, LPARAM lParam)
{
	if ((nID & 0xFFF0) == IDM_ABOUTBOX)
	{
		CAboutDlg dlgAbout;
		dlgAbout.DoModal();
	}
	else
	{
		CDialog::OnSysCommand(nID, lParam);
	}
}

// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.

void CChangeWindowTitleDlg::OnPaint() 
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
HCURSOR CChangeWindowTitleDlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}

void CChangeWindowTitleDlg::saveSettings() 
{
	UpdateData(TRUE);
	WritePrivateProfileString("Defaults", "TitleFind", m_EditTextFind, "ChangeWindowTitle.Ini");
	WritePrivateProfileString("Defaults", "TitleFrom", m_EditTextFrom, "ChangeWindowTitle.Ini");
	WritePrivateProfileString("Defaults", "TitleTo", m_EditTextTo, "ChangeWindowTitle.Ini");
}

void CChangeWindowTitleDlg::OnClose() 
{
	// TODO: Add your message handler code here and/or call default
	saveSettings();

	CDialog::OnClose();
}

bool modifyWindowTitles(const char *pszWindowTitle, const char *pszSubstFrom, 
						const char *pszSubstTo)
{
	HWND hWnd = NULL;
	bool bSuccessful = false;

	hWnd = FindWindowEx(NULL, hWnd, NULL, NULL);
	while (hWnd != NULL)
	{
		char pszTemp[512];
		::GetWindowText(hWnd, pszTemp, sizeof(pszTemp));
		if (strstr(pszTemp, pszWindowTitle) != NULL)
		{
			CString zWindowTitle = pszTemp;
			bSuccessful = zWindowTitle.Replace(pszSubstFrom, pszSubstTo) != 0;
			::SetWindowText(hWnd, zWindowTitle);
		}

		hWnd = FindWindowEx(NULL, hWnd, NULL, NULL);
	}

	return bSuccessful;
}

void CChangeWindowTitleDlg::OnOK() 
{
	UpdateData(TRUE);
	if (m_EditTextFind == "")
	{
		MessageBox("Invalid input! Please specify text to find!");
		return;
	}

	if (m_EditTextFrom == "")
	{
		MessageBox("Invalid input! Please specify text to change from!");
		return;
	}

	if (m_EditTextTo == "")
	{
		MessageBox("Invalid input! Please specify text to change to!");
		return;
	}

	if (!modifyWindowTitles(m_EditTextFind, m_EditTextFrom, m_EditTextTo))
		Beep(1000, 100);
	else
		saveSettings();
}
