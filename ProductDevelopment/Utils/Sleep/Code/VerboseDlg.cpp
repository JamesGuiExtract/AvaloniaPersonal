// VerboseDlg.cpp - Implementation of the CVerboseDlg class
#include "stdafx.h"
#include "Resource.h"
#include "VerboseDlg.h"
#include "SleepConstants.h"

#include <UCLIDException.h>

#include <string>
#include <cmath>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

//--------------------------------------------------------------------------------------------------
// CVerboseDlg dialog
//--------------------------------------------------------------------------------------------------
CVerboseDlg::CVerboseDlg(CWnd* pParent /*=NULL*/) : 
CDialog(CVerboseDlg::IDD, pParent)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}

//--------------------------------------------------------------------------------------------------
// DataExchange and MessageMap
//--------------------------------------------------------------------------------------------------
void CVerboseDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CVerboseDlg, CDialog)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//--------------------------------------------------------------------------------------------------
// Message Handlers
//--------------------------------------------------------------------------------------------------
BOOL CVerboseDlg::OnInitDialog()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CDialog::OnInitDialog();

		// Set the icon for this dialog.  The framework does this automatically
		//  when the application's main window is not a dialog
		SetIcon(m_hIcon, TRUE);			// Set big icon
		SetIcon(m_hIcon, FALSE);		// Set small icon
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI21059");

	return TRUE;  // return TRUE  unless you set the focus to a control
}
//--------------------------------------------------------------------------------------------------
void CVerboseDlg::OnOK()
{
	// purpose of having this function here is to prevent the user from closing the
	// dialog by pressing the Enter key
}
//--------------------------------------------------------------------------------------------------
void CVerboseDlg::OnCancel()
{
	// purpose of having this function here is to prevent the user from closing the
	// dialog by pressing the Escape key
}

//--------------------------------------------------------------------------------------------------
// Public helper functions
//--------------------------------------------------------------------------------------------------
void CVerboseDlg::setSleepTime(unsigned long ulSleepTime)
{
	try
	{
		// check which label to apply based on the size of the SleepTime
		string strFormat = "";
		if (ulSleepTime >= glHOURS_MULTIPLIER)
		{
			strFormat = "%H hour(s) %M minute(s) and %S second(s).";
		}
		else if (ulSleepTime >= glMINUTES_MULTIPLIER)
		{
			strFormat = "%M minute(s) and %S second(s).";
		}

		string strLabel = "Sleeping for ";

		// convert ulSleep from ms to s
		double dSeconds = (double) ulSleepTime / 1000.0;

		// if format is empty just convert to seconds (with 2 decimal places)
		if (strFormat.empty())
		{
			strLabel += asString(dSeconds, 2) + " second(s).";
		}
		else
		{
			// format is not blank, pass the seconds value to CTimeSpan
			// and generate a string using the Format command
			dSeconds = floor(dSeconds + 0.5);
			CTimeSpan ctSpan((unsigned long)dSeconds);
			strLabel += ctSpan.Format(strFormat.c_str()).GetString();
		}

		// update the label to show the sleep time
		GetDlgItem(IDC_TEXT_SLEEP)->SetWindowText(strLabel.c_str());

		// ensure the label gets updated
		UpdateData();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI21060");
}
//--------------------------------------------------------------------------------------------------
