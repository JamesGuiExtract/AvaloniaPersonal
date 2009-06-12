// ProgressDlg.cpp : implementation file

#include "stdafx.h"
#include "BaseUtils.h"
#include "ProgressDialog.h"
#include "TemporaryResourceOverride.h"
#include "Math.h"
#include "UCLIDException.h"

extern AFX_EXTENSION_MODULE BaseUtilsDLL;

#define  IDT_TIMER_0  WM_USER + 200 

#define  PD_KILL_DLG  WM_USER + 2000

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// CProgressDlg dialog
//-------------------------------------------------------------------------------------------------
CProgressDlg::CProgressDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CProgressDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(CProgressDlg)
	m_szText = _T("");
	//}}AFX_DATA_INIT
	TemporaryResourceOverride rcOverride(BaseUtilsDLL.hResource);

	Create(IDD_PROGRESS_DIALOG);

	reset();
	m_bFirstCreate = true;
}
//-------------------------------------------------------------------------------------------------
CProgressDlg::CProgressDlg(CWnd* pParent /*=NULL*/, bool /*bModeless*/)
	: CDialog(CProgressDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(CProgressDlg)
	m_szText = _T("");
	//}}AFX_DATA_INIT
	TemporaryResourceOverride rcOverride(BaseUtilsDLL.hResource);

	reset();
	m_bFirstCreate = true;
}
//-------------------------------------------------------------------------------------------------
void CProgressDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CProgressDlg)
	DDX_Control(pDX, IDCANCEL, m_ctrlCancel);
	DDX_Control(pDX, IDC_PROGRESS_BAR, m_ctrlProgressBar);
	DDX_Text(pDX, IDC_STRING, m_szText);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CProgressDlg, CDialog)
	//{{AFX_MSG_MAP(CProgressDlg)
	ON_WM_TIMER()
	ON_MESSAGE(PD_KILL_DLG, OnKillDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
//-------------------------------------------------------------------------------------------------
void CProgressDlg::show()
{
	TemporaryResourceOverride rcOverride(BaseUtilsDLL.hResource);
	{
		CSingleLock lg( &m_mutexAll, TRUE );
		m_bUpdateWindow = true;
		m_bShow = true;
	}
}
//-------------------------------------------------------------------------------------------------
void CProgressDlg::hide()
{
	TemporaryResourceOverride rcOverride(BaseUtilsDLL.hResource);
	{
		CSingleLock lg( &m_mutexAll, TRUE );
		m_bUpdateWindow = true;
		m_bShow = false;
	}
}
//-------------------------------------------------------------------------------------------------
void CProgressDlg::showCancel(bool bShowCancel)
{
	TemporaryResourceOverride rcOverride(BaseUtilsDLL.hResource);
	{
		CSingleLock lg( &m_mutexAll, TRUE );
		m_bUpdateWindow = true;
		m_bShowCancel = bShowCancel;
	}
}
//-------------------------------------------------------------------------------------------------
void CProgressDlg::reset()
{
	TemporaryResourceOverride rcOverride(BaseUtilsDLL.hResource);
	{
		CSingleLock lg( &m_mutexAll, TRUE );
		m_fCurrPercent = 0;
		m_fPrevPercent = 0;
		m_bUserCanceled = false;
		m_bUpdateWindow = true;
		m_fMaxNoRefresh = 1;
		m_bKill = false;
		m_nRetCode = 0;
		m_strTitle = "";
		m_strText = "";
	}
}
//-------------------------------------------------------------------------------------------------
void CProgressDlg::setPercentComplete(double fPercent)
{
	TemporaryResourceOverride rcOverride(BaseUtilsDLL.hResource);
	CSingleLock lg( &m_mutexAll, TRUE );
	m_fCurrPercent = fPercent;
	if (fabs(m_fCurrPercent - m_fPrevPercent) > m_fMaxNoRefresh)
	{
		m_fPrevPercent = m_fCurrPercent;
		m_bUpdateWindow = true;
	}
}
//-------------------------------------------------------------------------------------------------
void CProgressDlg::setText(string strText)
{
	TemporaryResourceOverride rcOverride(BaseUtilsDLL.hResource);
	CSingleLock lg( &m_mutexAll, TRUE );
	m_strText = strText;
	m_bUpdateWindow = true;
}
//-------------------------------------------------------------------------------------------------
void CProgressDlg::setTitle(string strText)
{
	TemporaryResourceOverride rcOverride(BaseUtilsDLL.hResource);
	CSingleLock lg( &m_mutexAll, TRUE );
	m_strTitle = strText;
	m_bUpdateWindow = true;
}
//-------------------------------------------------------------------------------------------------
bool CProgressDlg::userCanceled()
{
	TemporaryResourceOverride rcOverride(BaseUtilsDLL.hResource);
	CSingleLock lg( &m_mutexAll, TRUE );
	if (m_bUserCanceled)
	{
		// now that someone has been alerted to the fact that
		// the user pressed cancel reset the flag in case they want to resume
		m_bUserCanceled = false;
		return true;
	}
	else
	{
		return false;
	}
}
//-------------------------------------------------------------------------------------------------
LRESULT CProgressDlg::OnKillDlg(WPARAM /*wParam*/, LPARAM /*lParam*/)
{
	KillTimer(m_nTimer);
	// Save the position of the dialog
	GetWindowRect(&m_rectLastPos);
	EndDialog(m_nRetCode);
	return 0;
}
//-------------------------------------------------------------------------------------------------
void CProgressDlg::kill(int retCode)
{
	TemporaryResourceOverride rcOverride(BaseUtilsDLL.hResource);
	CSingleLock lg( &m_mutexAll, TRUE );
	m_nRetCode = retCode;
	::PostMessage(m_hWnd, PD_KILL_DLG, 0, 0);
}
//-------------------------------------------------------------------------------------------------
void CProgressDlg::waitForInit()
{
	TemporaryResourceOverride rcOverride(BaseUtilsDLL.hResource);
	CSingleLock lg( &m_mutexAll, TRUE );
	m_eventInit.wait();
	m_eventInit.reset();
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CProgressDlg::setShowCancel(bool bShowCancel)
{
	
	TemporaryResourceOverride rcOverride(BaseUtilsDLL.hResource);
	if (bShowCancel && m_ctrlCancel.IsWindowVisible() == FALSE)
	{
		
		CRect winRect;
		GetWindowRect(&winRect);

		CRect cancelRect;
		m_ctrlCancel.GetWindowRect(&cancelRect);
		winRect.bottom = winRect.bottom + cancelRect.Height();

		MoveWindow(winRect);

		m_ctrlCancel.ShowWindow(SW_SHOW);
	}
	else if (!bShowCancel && m_ctrlCancel.IsWindowVisible() == TRUE)
	{
		m_ctrlCancel.ShowWindow(SW_HIDE);
		CRect winRect;
		GetWindowRect(&winRect);

		CRect cancelRect;
		m_ctrlCancel.GetWindowRect(&cancelRect);
		winRect.bottom = winRect.bottom - cancelRect.Height();

		MoveWindow(winRect);	
	}
}

//-------------------------------------------------------------------------------------------------
// CProgressDlg message handlers
//-------------------------------------------------------------------------------------------------
void CProgressDlg::OnCancel() 
{
	TemporaryResourceOverride rcOverride(BaseUtilsDLL.hResource);

	CSingleLock lg( &m_mutexAll, TRUE );
	m_bUserCanceled = true;
}
//-------------------------------------------------------------------------------------------------
BOOL CProgressDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride rcOverride(BaseUtilsDLL.hResource);

	try
	{
		CDialog::OnInitDialog();

		m_bUserCanceled = false;
		m_bUpdateWindow = true;
		m_bKill = false;
		m_nRetCode = 0;
		reset();

		m_ctrlProgressBar.SetRange32(0, 100000);
		m_nTimer = SetTimer(IDT_TIMER_0, 20, 0);

		// If the dialog was in a previous location we should again diplay it in that
		// same location
		if (m_bFirstCreate)
		{
			m_bFirstCreate = false;
		}
		else
		{
			SetWindowPos(NULL, m_rectLastPos.left, m_rectLastPos.top, 0, 0, SWP_NOSIZE|SWP_NOZORDER);
		}

		m_eventInit.signal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18595");

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void CProgressDlg::OnTimer(UINT /*nIDEvent*/) 
{
	TemporaryResourceOverride rcOverride(BaseUtilsDLL.hResource);
	CSingleLock lg( &m_mutexAll, TRUE );
	
	if (/*nIDEvent == m_nTimer && */m_bUpdateWindow)
	{
		ShowWindow(m_bShow ? SW_SHOW : SW_HIDE);
		m_szText = m_strText.c_str();
		SetWindowText(m_strTitle.c_str());
		m_ctrlProgressBar.SetPos((int) m_fCurrPercent * 1000);
		setShowCancel(m_bShowCancel);

		UpdateData(FALSE);
		UpdateWindow();
	
		// reset the update
		m_bUpdateWindow = false;
	} 
}