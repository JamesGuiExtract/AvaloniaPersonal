// TaskInfoDlg.cpp : implementation file
//

#include "stdafx.h"
#include "resource.h"
#include "TaskInfoDlg.h"

#include <UCLIDException.h>
#include <TemporaryResourceOverride.h>
#include <cpputil.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

static const int STATRTTIME_ROW = 0;
static const int DURATION_ROW = 1;
static const int EXCEPTION_ROW = 2;
static const int FIRST_COLUMN_WIDTH = 70;

//-------------------------------------------------------------------------------------------------
// TaskInfoDlg dialog
//-------------------------------------------------------------------------------------------------
TaskInfoDlg::TaskInfoDlg(CWnd* pParent /*=NULL*/)
: CDialog(TaskInfoDlg::IDD, pParent),
  m_apUCLIDException(__nullptr),
  m_bInitialized(false)
{
	// this is a modeless dialog box;
	Create(IDD_DLG_TASK_INFO);
}
//-------------------------------------------------------------------------------------------------
void TaskInfoDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(TaskInfoDlg)
	DDX_Control(pDX, IDC_LIST_DETAIL, m_listDetails);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(TaskInfoDlg, CDialog)
	//{{AFX_MSG_MAP(TaskInfoDlg)
	ON_NOTIFY(NM_DBLCLK, IDC_LIST_DETAIL, OnDblclkListDetail)
	ON_WM_SIZE()
	ON_WM_GETMINMAXINFO()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
//-------------------------------------------------------------------------------------------------
void TaskInfoDlg::setTask(const FileProcessingRecord& task)
{
	m_taskFileProcessing = task;

	try
	{
		if (!m_taskFileProcessing.m_strException.empty())
		{
			m_apUCLIDException.reset(new UCLIDException());
			m_apUCLIDException->createFromString("ELI09055", m_taskFileProcessing.m_strException);
		}
		else
		{
			m_apUCLIDException.reset(__nullptr);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09058")

	// translate to local time
	string strStartTime;
	if(m_taskFileProcessing.m_stopWatch.isReset())
	{
		strStartTime = "Task not started";
	}
	else
	{
		// TESTTHIS: was the code change here during the VS2005 port valid
		tm _tm;
		m_taskFileProcessing.m_stopWatch.getBeginTime().GetLocalTm(&_tm);
		// the display string
		char pszTemp[256];
		if (asctime_s(pszTemp, sizeof(pszTemp), &_tm) != 0)
		{
			throw UCLIDException("ELI12930", "Unable to get ASCII time!");
		}
		strStartTime = pszTemp;
		strStartTime = ::trim(strStartTime, "", "\r\n");
	}
	m_listDetails.SetItemText(STATRTTIME_ROW, 1, strStartTime.c_str());

	// display duration
	double dTotalTime = m_taskFileProcessing.m_stopWatch.getElapsedTime();
	if (dTotalTime >= 0.0)
	{
		string strTotalElapsedSeconds = ::asString(dTotalTime);
		CString zElapsedTime = strTotalElapsedSeconds.c_str();
		zElapsedTime += " seconds";
		m_listDetails.SetItemText(DURATION_ROW, 1, zElapsedTime);
	}

	if (m_apUCLIDException.get())
	{
		string strTopText = m_apUCLIDException->getTopText();
		// display the top text
		m_listDetails.SetItemText(EXCEPTION_ROW, 1, strTopText.c_str());
	}
	else
	{
		m_listDetails.SetItemText(EXCEPTION_ROW, 1, "");
	}


	CRect rectDlg;
	GetWindowRect(&rectDlg);

	m_nDlgMinWidth = rectDlg.Width();
	m_nDlgFixedHeight = rectDlg.Height();

	UpdateWindow();
}
//-------------------------------------------------------------------------------------------------
// TaskInfoDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL TaskInfoDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	CDialog::OnInitDialog();
		
	try
	{
		// Enable full row selection plus grid lines and checkboxes
		m_listDetails.SetExtendedStyle(LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT);

		// Compute width for Description column
		CRect rectList;
		m_listDetails.GetClientRect(rectList);
		long nWidth = rectList.Width();
		// Add 2 columns
		m_listDetails.InsertColumn(0, "Item", LVCFMT_LEFT, FIRST_COLUMN_WIDTH, 0);
		m_listDetails.InsertColumn(1, "Description", LVCFMT_LEFT, nWidth - FIRST_COLUMN_WIDTH, 1);

		// **********
		// start time 
		m_listDetails.InsertItem(STATRTTIME_ROW, "");
		m_listDetails.SetItemText(STATRTTIME_ROW, 0, "Start Time");

		// *********
		// duration
		m_listDetails.InsertItem(DURATION_ROW, "");
		m_listDetails.SetItemText(DURATION_ROW, 0, "Duration");
		
		// *********
		// Exception
		m_listDetails.InsertItem(EXCEPTION_ROW, "");
		m_listDetails.SetItemText(EXCEPTION_ROW, 0, "Exception");

		m_bInitialized = true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09049")
	
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void TaskInfoDlg::OnDblclkListDetail(NMHDR* pNMHDR, LRESULT* pResult) 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// get current selected item
		int nIndex = -1;
		POSITION pos = m_listDetails.GetFirstSelectedItemPosition();
		if (pos != __nullptr)
		{
			// Get index of first selection
			nIndex = m_listDetails.GetNextSelectedItem(pos);
		}
		
		if (nIndex == EXCEPTION_ROW && m_apUCLIDException.get() != __nullptr)
		{
			// display the exception if any
			m_apUCLIDException->display();
		}
		
		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09057")
}
//-------------------------------------------------------------------------------------------------
void TaskInfoDlg::OnSize(UINT nType, int cx, int cy) 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		CDialog::OnSize(nType, cx, cy);
	
		if (!m_bInitialized)
		{
			return;
		}

		CRect rectDlg;
		GetWindowRect(&rectDlg);
		// make the list second column longer/shorter
		int nNewListWidth = rectDlg.Width() - 2;
		CRect rectList;
		m_listDetails.GetClientRect(&rectList);
		rectList.right = rectList.left + nNewListWidth;
		m_listDetails.MoveWindow(&rectList);

		int nSecondColumnWidth = nNewListWidth - FIRST_COLUMN_WIDTH;
		m_listDetails.SetColumnWidth(0, FIRST_COLUMN_WIDTH);
		m_listDetails.SetColumnWidth(1, nSecondColumnWidth);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09063")
}
//-------------------------------------------------------------------------------------------------
void TaskInfoDlg::OnGetMinMaxInfo(MINMAXINFO* lpMMI)
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		if (!m_bInitialized)
		{
			return;
		}

		// Minimum width to allow display of list
		lpMMI->ptMinTrackSize.x = m_nDlgMinWidth;

		// Minimum height
		lpMMI->ptMinTrackSize.y = m_nDlgFixedHeight;
		// Maximum height
		lpMMI->ptMaxTrackSize.y = m_nDlgFixedHeight;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09064")
}

