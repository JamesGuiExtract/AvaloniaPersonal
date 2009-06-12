// CalcTotalMemLeakDlg.cpp : implementation file
//

#include "stdafx.h"
#include "CalcTotalMemLeak.h"
#include "CalcTotalMemLeakDlg.h"

#include <string>
using namespace std;

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
// CCalcTotalMemLeakDlg dialog

CCalcTotalMemLeakDlg::CCalcTotalMemLeakDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CCalcTotalMemLeakDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(CCalcTotalMemLeakDlg)
	m_zInput = _T("");
	//}}AFX_DATA_INIT
	// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}

void CCalcTotalMemLeakDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CCalcTotalMemLeakDlg)
	DDX_Control(pDX, IDC_EDIT1, m_editInput);
	DDX_Text(pDX, IDC_EDIT1, m_zInput);
	//}}AFX_DATA_MAP
}

BEGIN_MESSAGE_MAP(CCalcTotalMemLeakDlg, CDialog)
	//{{AFX_MSG_MAP(CCalcTotalMemLeakDlg)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_BN_CLICKED(IDC_BUTTON2, OnButton2)
	ON_BN_CLICKED(IDC_BUTTON1, OnButton1)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CCalcTotalMemLeakDlg message handlers

BOOL CCalcTotalMemLeakDlg::OnInitDialog()
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
	
	return TRUE;  // return TRUE  unless you set the focus to a control
}

void CCalcTotalMemLeakDlg::OnSysCommand(UINT nID, LPARAM lParam)
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

void CCalcTotalMemLeakDlg::OnPaint() 
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
HCURSOR CCalcTotalMemLeakDlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}

void CCalcTotalMemLeakDlg::OnOK() 
{

	UpdateData(TRUE);

	unsigned long ulTotalBytes = 0;
	int iPos = 0;
	do
	{
		iPos = m_zInput.Find(" bytes long", iPos);
		if (iPos != -1)
		{
			int iStartPos = iPos-1;
			const char *pszTemp = (LPCTSTR) m_zInput;
			while (pszTemp[iStartPos-1] >= '0' && pszTemp[iStartPos-1] <= '9')
				iStartPos--;
			
			CString zBytes = m_zInput.Mid(iStartPos, iPos - iStartPos);
			iPos++;

			long lBytes = atol((LPCTSTR) zBytes);
			if (lBytes == 0)
				MessageBox("Error!");
			else
				ulTotalBytes += lBytes;
		}

	} while (iPos != -1);

	char pszTemp[128];
	sprintf_s(pszTemp, "%d", ulTotalBytes);

	string strMsg = "Total bytes leaked:\n";
	strMsg += pszTemp;

	MessageBox(strMsg.c_str());
}

void CCalcTotalMemLeakDlg::OnButton2() 
{
	m_zInput = "";
	UpdateData(FALSE);
}

void CCalcTotalMemLeakDlg::OnButton1() 
{
	m_zInput = "";
	UpdateData(FALSE);
	m_editInput.Paste();
}
