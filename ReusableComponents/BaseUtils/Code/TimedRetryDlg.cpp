// TimedRetryDlg.cpp : implementation file
//

#include "stdafx.h"
#include "BaseUtils.h"
#include "cpputil.h"
#include "TimedRetryDlg.h"
#include "TemporaryResourceOverride.h"
#include "UCLIDException.h"

extern AFX_EXTENSION_MODULE BaseUtilsDLL;

//-------------------------------------------------------------------------------------------------
// CTimedRetryDlg dialog
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CTimedRetryDlg, CDialog)

CTimedRetryDlg::CTimedRetryDlg( std::string strCaption, std::string strMsg, int nTimeOut, CWnd* pParent /*=NULL*/)
	: CDialog(CTimedRetryDlg::IDD, pParent),
	m_nTimeOut(nTimeOut),
	m_strMsg(strMsg), 
	m_strCaption(strCaption),
	m_zStaticMsg(strMsg.c_str()),
	m_uiTimerID(0)
{
}
//-------------------------------------------------------------------------------------------------
CTimedRetryDlg::~CTimedRetryDlg()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16410");
}
//-------------------------------------------------------------------------------------------------
void CTimedRetryDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Text(pDX, IDC_STATIC_MSG, m_zStaticMsg);
	DDX_Control(pDX, IDOK, m_btnOK);
}
//-------------------------------------------------------------------------------------------------
int CTimedRetryDlg::DoModal()
{
	TemporaryResourceOverride rcOverride(BaseUtilsDLL.hResource);
	
	// call the base class member
	return CDialog::DoModal();
}
//-------------------------------------------------------------------------------------------------
BOOL CTimedRetryDlg::OnInitDialog()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CDialog::OnInitDialog();
		
		// Setup the controls
		UpdateData(TRUE);

		// Set the caption for the dialog
		SetWindowTextA(m_strCaption.c_str());

		// Initialize button string and timer
		m_uiTimerID = 0;
		string strBtnMsg = "&OK";
		if ( m_nTimeOut > 0)
		{
			// Set up time remaining to count down
			m_nTimeRemaining = m_nTimeOut;

			// Build the string for the OK button text
			strBtnMsg += " (" + asString(m_nTimeRemaining) + " secs)";

			// Set 1 second timer for text updates
			m_uiTimerID = SetTimer( 1000, 1000, NULL );
		}

		// Set the Ok button Text
		m_btnOK.SetWindowTextA(strBtnMsg.c_str());
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18598");
	
	return TRUE; 
}
//-------------------------------------------------------------------------------------------------

BEGIN_MESSAGE_MAP(CTimedRetryDlg, CDialog)
	ON_WM_TIMER()
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CTimedRetryDlg message handlers
//-------------------------------------------------------------------------------------------------
void CTimedRetryDlg::OnTimer(UINT nIDEvent) 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride rcOverride(BaseUtilsDLL.hResource);

	// Count down the time remaining
	m_nTimeRemaining--;

	// Check for time expired
	if ( m_nTimeRemaining <= 0)
	{
		// Kill the timer and click OK
		KillTimer(m_uiTimerID);
		OnOK();
	}
	else
	{
		// Update the OK button text
		string strBtnMsg = "&OK (" + asString(m_nTimeRemaining) + " secs)";
		m_btnOK.SetWindowTextA(strBtnMsg.c_str());
	}
}
//-------------------------------------------------------------------------------------------------
