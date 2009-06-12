// CPUHogDlg.cpp : implementation file
//

#include "stdafx.h"
#include "CPUHog.h"
#include "CPUHogDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif


#define WM_THREAD_FINISHED WM_USER + 5281

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
// CCPUHogDlg dialog

CCPUHogDlg::CCPUHogDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CCPUHogDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(CCPUHogDlg)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT
	// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}

void CCPUHogDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CCPUHogDlg)
	DDX_Control(pDX, IDC_RADIO_100, m_radio100);
	DDX_Control(pDX, IDC_RADIO_90, m_radio90);
	DDX_Control(pDX, IDC_RADIO_SPECIFIED, m_radioSpecified);
	DDX_Control(pDX, IDC_BTN_RUN, m_btnRun);
	DDX_Control(pDX, IDC_EDIT_SECONDS, m_editSeconds);
	DDX_Control(pDX, IDC_EDIT_PERCENT, m_editPercent);
	//}}AFX_DATA_MAP
}

BEGIN_MESSAGE_MAP(CCPUHogDlg, CDialog)
	//{{AFX_MSG_MAP(CCPUHogDlg)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_BN_CLICKED(IDC_BTN_RUN, OnBtnRun)
	ON_BN_CLICKED(IDC_RADIO_100, OnRadio100)
	ON_BN_CLICKED(IDC_RADIO_90, OnRadio90)
	ON_BN_CLICKED(IDC_RADIO_SPECIFIED, OnRadioSpecified)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CCPUHogDlg message handlers

BOOL CCPUHogDlg::OnInitDialog()
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

	m_radio100.SetCheck(1);
	OnRadio100();
	
	m_editSeconds.SetWindowText("60");
	m_bRunning = false;

	return TRUE;  // return TRUE  unless you set the focus to a control
}

void CCPUHogDlg::OnSysCommand(UINT nID, LPARAM lParam)
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

void CCPUHogDlg::OnPaint() 
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
HCURSOR CCPUHogDlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}
void CCPUHogDlg::OnCancel()
{

	if(m_bRunning)
	{
		m_params.kill();
		WaitForSingleObject(m_pThread->m_hThread, INFINITE);
	}
	CDialog::OnCancel();
}

BOOL CCPUHogDlg::PreTranslateMessage(MSG *pMsg)
{
	if(pMsg->message == WM_THREAD_FINISHED)
	{
		WaitForSingleObject(m_pThread->m_hThread, INFINITE);
		m_btnRun.SetWindowText("Run");
		m_radio100.EnableWindow(TRUE);
		m_radio90.EnableWindow(TRUE);
		m_radioSpecified.EnableWindow(TRUE);
		m_editPercent.EnableWindow(TRUE);
		m_editSeconds.EnableWindow(TRUE);
		m_bRunning = false;
	}

	return CDialog::PreTranslateMessage(pMsg);
}


void CCPUHogDlg::OnBtnRun() 
{
	// TODO: Add your control notification handler code here

	if(!m_bRunning)
	{
		CString strDuration;
		m_editSeconds.GetWindowText(strDuration);
		m_params.setDuration(atoi(strDuration) * 1000);

		m_params.setThread(AfxGetThread());

		if(m_radio100.GetCheck() == 1)
		{
			m_params.setPercent(100);
		}
		else if(m_radio90.GetCheck() == 1)
		{
			m_params.setPercent(90);
		}
		else 
		{
			CString strPercent;
			m_editPercent.GetWindowText(strPercent);
			m_params.setPercent(atof(strPercent));
		}
		m_btnRun.SetWindowText("Stop");
		m_radio100.EnableWindow(FALSE);
		m_radio90.EnableWindow(FALSE);
		m_radioSpecified.EnableWindow(FALSE);
		m_editPercent.EnableWindow(FALSE);
		m_editSeconds.EnableWindow(FALSE);
		m_pThread = AfxBeginThread(threadFunc, &m_params);
		m_bRunning = true;
	
	}
	else
	{
		m_params.kill();
		WaitForSingleObject(m_pThread->m_hThread, INFINITE);
		m_btnRun.SetWindowText("Run");
		m_radio100.EnableWindow(TRUE);
		m_radio90.EnableWindow(TRUE);
		m_radioSpecified.EnableWindow(TRUE);
		m_editPercent.EnableWindow(TRUE);
		m_editSeconds.EnableWindow(TRUE);
		m_bRunning = false;
	}
}

void CCPUHogDlg::OnRadio100() 
{
	// TODO: Add your control notification handler code here
	m_editPercent.EnableWindow(FALSE);
	
}

void CCPUHogDlg::OnRadio90() 
{
	// TODO: Add your control notification handler code here
	m_editPercent.EnableWindow(FALSE);
}

void CCPUHogDlg::OnRadioSpecified() 
{
	// TODO: Add your control notification handler code here
	m_editPercent.EnableWindow(TRUE);
}


UINT CCPUHogDlg::threadFunc( LPVOID pParam )
{
	ThreadParams* pParams = (ThreadParams*) pParam;

	LARGE_INTEGER nFrequency;
	QueryPerformanceFrequency(&nFrequency);

	LARGE_INTEGER nCounter;
	QueryPerformanceCounter(&nCounter);

	//All times are in in milliseconds
	LONGLONG nStartTime = (nCounter.QuadPart*1000) / nFrequency.QuadPart;
	LONGLONG nEndTime = nStartTime + pParams->getDuration();

	LONGLONG nCurrTime = nStartTime;

	long nSleepLength = 200;
	double fPercent = pParams->getPercent() / 100.0;

	LONGLONG nWakeLength;
	if(fPercent >= 1)
	{
		nSleepLength = 0;
		nWakeLength = 1000;
	}
	else
	{
		nWakeLength = (LONGLONG)((nSleepLength * fPercent) / (1.0-fPercent));
	}
	// This needs to change
	LONGLONG nNextSleep = nStartTime + nWakeLength;

	while(nCurrTime < nEndTime)
	{
		if(pParams->killed())
		{
			return 0;
		}

		if(nCurrTime > nNextSleep)
		{
			Sleep(nSleepLength);
			QueryPerformanceFrequency(&nFrequency);
			QueryPerformanceCounter(&nCounter);
			nCurrTime = (nCounter.QuadPart*1000) / nFrequency.QuadPart;
			// This needs to change
			nNextSleep = nCurrTime + nWakeLength;
		}

		QueryPerformanceFrequency(&nFrequency);
		QueryPerformanceCounter(&nCounter);
		nCurrTime = (nCounter.QuadPart*1000) / nFrequency.QuadPart;
	}

	PostThreadMessage(pParams->getThread()->m_nThreadID, WM_THREAD_FINISHED, 0, 0);

	return 0;
}

CCPUHogDlg::ThreadParams::ThreadParams() : m_nDuration(0), m_fPercent(0), m_bKill(false)
{
}
long CCPUHogDlg::ThreadParams::getDuration()
{
	CSingleLock lock(&m_semAll);
	lock.Lock();

	if(lock.IsLocked())
	{
		return m_nDuration;
	}
	else 
	{
		return 0;
	}
}
void CCPUHogDlg::ThreadParams::setDuration(long nDuration)
{
	CSingleLock lock(&m_semAll);
	lock.Lock();

	if(lock.IsLocked())
	{
		m_nDuration = nDuration;
	}
}
double CCPUHogDlg::ThreadParams::getPercent()
{
	CSingleLock lock(&m_semAll);
	lock.Lock();

	if(lock.IsLocked())
	{
		return m_fPercent;
	}
	else
	{
		return 0;
	}
}
void CCPUHogDlg::ThreadParams::setPercent(double fPercent)
{
	CSingleLock lock(&m_semAll);
	lock.Lock();
	if(lock.IsLocked())
	{
		m_fPercent = fPercent;
	}
}
void CCPUHogDlg::ThreadParams::kill()
{
	CSingleLock lock(&m_semAll);
	lock.Lock();
	if(lock.IsLocked())
	{
		m_bKill = true;
	}
}
bool CCPUHogDlg::ThreadParams::killed()
{
	CSingleLock lock(&m_semAll);
	lock.Lock();

	if(lock.IsLocked())
	{
		return m_bKill;
	}
	else
	{
		return false;
	}
}
void CCPUHogDlg::ThreadParams::reset()
{
	CSingleLock lock(&m_semAll);
	lock.Lock();

	if(lock.IsLocked())
	{
		m_bKill = false;
	}
}
CWinThread* CCPUHogDlg::ThreadParams::getThread()
{
	CSingleLock lock(&m_semAll);
	lock.Lock();

	if(lock.IsLocked())
	{
		return m_pThread;
	}
	else
	{
		return NULL;
	}
}
void CCPUHogDlg::ThreadParams::setThread(CWinThread* pThread)
{
	CSingleLock lock(&m_semAll);
	lock.Lock();
	if(lock.IsLocked())
	{
		m_pThread = pThread;
	}
}