// UCLIDExceptionDlg.cpp : implementation file
//
#include "stdafx.h"
#include "resource.h"
#include "UCLIDExceptionDlg.h"
#include "UCLIDExceptionDetailsDlg.h"
#include "cpputil.h"
#include "TemporaryResourceOverride.h"
#include "RegistryPersistenceMgr.h"
#include "RegConstants.h"

extern HINSTANCE gModuleResource;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

using namespace std;

//-------------------------------------------------------------------------------------------------
// Registry key constants
//-------------------------------------------------------------------------------------------------
// Path to BaseUtils settings under HKLM
const string gstrBASE_UTILS = "\\ReusableComponents\\BaseUtils";
// Key name for automatic timeout in seconds
const string gstrUE_AUTO_TIMEOUT = "ExceptionAutoTimeout";

//-------------------------------------------------------------------------------------------------
// Statics
//-------------------------------------------------------------------------------------------------
// Specifies whether display() should call SetForegroundWindow
bool UCLIDExceptionDlg::m_sbDisplayAsForegroundWindow = false;

//-------------------------------------------------------------------------------------------------
// UCLIDExceptionDlg
//-------------------------------------------------------------------------------------------------
UCLIDExceptionDlg::UCLIDExceptionDlg(CWnd* pParent /*=NULL*/)
	: CDialog(IDD_DIALOG_INFO, pParent),
	m_pUclidExceptionCaught(NULL),
	m_bShowDebugInformationDlg(false),
	m_bAutoTimeout(false),
	m_lTimeoutCount(0),
	m_lTimerID(0)
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	//{{AFX_DATA_INIT(UCLIDExceptionDlg)
	m_Information = _T("");
	//}}AFX_DATA_INIT
}
//-------------------------------------------------------------------------------------------------
UCLIDExceptionDlg::~UCLIDExceptionDlg()
{
	try
	{
		// TODO: Do not clear this handler at this time because there is 
		// no way to restore the previous exception handler.
		//
		// This object is always used as the exception handler and so it is
		// safe to clear the handler associated with the UCLIDException as this
		// object is being destructed.
		//	UCLIDException::setExceptionHandler(NULL);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16575");
}
//-------------------------------------------------------------------------------------------------
void UCLIDExceptionDlg::DoDataExchange(CDataExchange* pDX)
{
	try
	{
		CDialog::DoDataExchange(pDX);
		//{{AFX_DATA_MAP(UCLIDExceptionDlg)
		DDX_Text(pDX, IDC_EDIT_INFO_TEXT, m_Information);
		//}}AFX_DATA_MAP
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20274");
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(UCLIDExceptionDlg, CDialog)
	//{{AFX_MSG_MAP(UCLIDExceptionDlg)
	ON_BN_CLICKED(IDC_BUTTON_DETAILS, OnButtonDetails)
	ON_BN_CLICKED(IDC_CHECK_TIMEOUT, OnCheckTimeout)
	ON_WM_TIMER()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// UCLIDExceptionDlg message handlers
//-------------------------------------------------------------------------------------------------
void UCLIDExceptionDlg::OnButtonDetails() 
{
	try
	{
		AFX_MANAGE_STATE(AfxGetModuleState());
		TemporaryResourceOverride resourceOverride(gModuleResource);

		// Turn off auto close if user is present and opens the Details dialog
		if (m_lTimerID > 0)
		{
			// Kill the automatic close timer
			KillTimer( m_lTimerID );

			// Update auto close text
			CButton* pButton = (CButton *)GetDlgItem( IDC_CHECK_TIMEOUT );
			if (pButton != __nullptr)
			{
				string strLabel = makeTimeoutString( 0 );
				pButton->SetWindowText( strLabel.c_str() );

				// Uncheck the button and disable it
				pButton->SetCheck( BST_UNCHECKED );
				pButton->EnableWindow( FALSE );
			}
		}

		// Display the Details dialog
		UCLIDExceptionDetailsDlg dlg(*m_pUclidExceptionCaught,this);
		dlg.DoModal();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20275");
}
//-------------------------------------------------------------------------------------------------
BOOL UCLIDExceptionDlg::OnInitDialog() 
{
	try
	{
		AFX_MANAGE_STATE(AfxGetModuleState());
		TemporaryResourceOverride resourceOverride(gModuleResource);

		CDialog::OnInitDialog();

		// Hide Details button if needed
		if (strDetailsMsg == "")
		{
			GetDlgItem(IDC_BUTTON_DETAILS)->ShowWindow(FALSE);
		}
		else
		{
			GetDlgItem(IDC_BUTTON_DETAILS)->ShowWindow(TRUE);
		}

		if (m_bShowDebugInformationDlg)
		{
			//this dialog was called to display UCLIDException to the user.
			//show the details button
			GetDlgItem(IDC_BUTTON_DETAILS)->ShowWindow(TRUE);

			//change the text of the button to 'Debug Information'.If user user clicks on it
			//it will open a dialog box with complete error information.Default label of 
			//the button is Details.
			SetDlgItemText(IDC_BUTTON_DETAILS,"Details...");

			//set the dialog label as IcoaMap Exception
			SetWindowText("Error");
		}

		// Check registry for automatic timeout value
		RegistryPersistenceMgr machineCfgMgr = RegistryPersistenceMgr( HKEY_LOCAL_MACHINE, 
			gstrREG_ROOT_KEY );

		// Check for existence of automatic timeout
		if (!machineCfgMgr.keyExists( gstrBASE_UTILS, gstrUE_AUTO_TIMEOUT ))
		{
			// Not found, just default to 0
			machineCfgMgr.createKey( gstrBASE_UTILS, gstrUE_AUTO_TIMEOUT, "0" );
			m_lTimeoutCount = 0;
		}
		else
		{
			// Retrieve automatic timeout in seconds
			string strTime = machineCfgMgr.getKeyValue( gstrBASE_UTILS, gstrUE_AUTO_TIMEOUT );
			m_lTimeoutCount = asLong( strTime );

			// Set flag if timeout > 0
			m_bAutoTimeout = (m_lTimeoutCount > 0);
		}

		// Set text for control
		string strLabel = makeTimeoutString( m_lTimeoutCount );
		GetDlgItem( IDC_CHECK_TIMEOUT )->SetWindowText( strLabel.c_str() );

		// Enable checkbox control and start countdown thread if needed
		if (m_bAutoTimeout)
		{
			CButton* pButton = (CButton *)GetDlgItem( IDC_CHECK_TIMEOUT );
			if (pButton != __nullptr)
			{
				pButton->SetCheck( BST_CHECKED );
				pButton->EnableWindow( TRUE );
			}

			// Set 1 second timer for text updates
			m_lTimerID = SetTimer( 1000, 1000, NULL );
		}
		else
		{
			// Disable checkbox
			GetDlgItem( IDC_CHECK_TIMEOUT )->EnableWindow( FALSE );
		}

		// If specified, call SetForegroundWindow to ensure the exception is displayed as the
		// foreground window.
		if (m_sbDisplayAsForegroundWindow)
		{
			SetForegroundWindow();
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20276");

	return TRUE; 
}
//-------------------------------------------------------------------------------------------------
int UCLIDExceptionDlg::DoModal() 
{
	try
	{
		AFX_MANAGE_STATE(AfxGetModuleState());
		TemporaryResourceOverride resourceOverride(gModuleResource);

		return CDialog::DoModal();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20277");

	// If we get here there was an exception
	return IDCANCEL;
}
//-------------------------------------------------------------------------------------------------
BOOL UCLIDExceptionDlg::Create(LPCTSTR lpszClassName, LPCTSTR lpszWindowName, DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID, CCreateContext* pContext) 
{
	try
	{
		AFX_MANAGE_STATE(AfxGetModuleState());
		TemporaryResourceOverride resourceOverride(gModuleResource);

		return CDialog::Create(IDD_DIALOG_INFO, pParentWnd);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20278");

	// There was an exception so return FALSE
	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void UCLIDExceptionDlg::OnCheckTimeout() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		// Retrieve current setting
		CButton* pButton = (CButton *)GetDlgItem( IDC_CHECK_TIMEOUT );
		if (pButton != __nullptr)
		{
			int iResult = pButton->GetCheck();
			if (iResult == BST_UNCHECKED)
			{
				// Reset flag
				m_bAutoTimeout = false;

				// Kill the timer and disable the checkbox
				KillTimer( m_lTimerID );
				pButton->EnableWindow( FALSE );
			}
			else
			{
				// Set flag
				m_bAutoTimeout = true;
			}
		}
	}
	catch (UCLIDException& ue)
	{
		UCLIDException uexOuter("ELI12961", "Error handling timeout checkbox.", ue);
		uexOuter.log("", false);
	}
	catch (...)
	{
		UCLIDException ue("ELI12962", "Error handling timeout checkbox.");
		ue.log("", false);
	}
}
//-------------------------------------------------------------------------------------------------
void UCLIDExceptionDlg::OnTimer(UINT nIDEvent) 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		updateCountdown();
	}
	catch (UCLIDException& ue)
	{
		UCLIDException ueOuter("ELI12989", "Error handling OnTimer().", ue);
		ueOuter.log("", false);
	}
	catch (...)
	{
		UCLIDException ue("ELI12990", "Error handling OnTimer().");
		ue.log("", false);
	}
}

//-------------------------------------------------------------------------------------------------
// Public methods
//-------------------------------------------------------------------------------------------------
void UCLIDExceptionDlg::display(const string& strMsg)
{
	try
	{
		string strText = strMsg;
		string strToFind = "\n";
		string strReplaceText1 = "~~~~~~";
		replaceVariable(strText, strToFind, strReplaceText1);
		string strReplaceText2 = "\r\n";
		// TODO; fix this cluge!
		replaceVariable(strText, strReplaceText1, strReplaceText2);

		m_Information = _T(strText.c_str());

		//set details message string to null
		strDetailsMsg = "";

		//set the boolean value to false so that details button will not be shown
		m_bShowDebugInformationDlg = false;

		// show the dialog
		DoModal();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20279");
}
//-------------------------------------------------------------------------------------------------
void UCLIDExceptionDlg::display(const UCLIDException& uclidException)
{
	try
	{
		// Keep a copy in case user clicks on Debug information Button
		m_pUclidExceptionCaught = &uclidException; 

		//Show the message with the top ELI number in the dialog.
		string strText = uclidException.getTopText();
		m_Information = _T(strText.c_str());

		//let this dialog know that debug information is to show
		m_bShowDebugInformationDlg = true;

		DoModal();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20280");
}
//-------------------------------------------------------------------------------------------------
void UCLIDExceptionDlg::handleException(const UCLIDException& uclidException)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetModuleState());
		TemporaryResourceOverride rcoverride(gModuleResource);

		// multiple threads/windows could try to call this method at the same time
		// so, do not use "this" instance of the dlg to display the exception
		// instead, create a new instance on the stack and use it
		UCLIDExceptionDlg dlg;
		dlg.display(uclidException);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20281");
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
string UCLIDExceptionDlg::makeTimeoutString(long lTimeout)
{
	// Default text if lTimeout = 0
	string strText( "Automatically close window" );

	// Provide count for countdown if lTimeout > 0
	if (lTimeout > 0)
	{
		strText += " in ";
		strText += asString( lTimeout );
		if (lTimeout == 1)
		{
			strText += " second.";
		}
		else
		{
			strText += " seconds.";
		}
	}

	return strText;
}
//-------------------------------------------------------------------------------------------------
void UCLIDExceptionDlg::updateCountdown()
{
	// Decrement counter member and build timeout string
	string strLabel = makeTimeoutString( --m_lTimeoutCount );

	// Update the window
	GetDlgItem( IDC_CHECK_TIMEOUT )->SetWindowText( strLabel.c_str() );

	// Close the window if timer reaches zero
	if (m_lTimeoutCount <= 0)
	{
		// Log an exception before close
		UCLIDException ue( "ELI12965", "Application trace: Automatic close of Exception dialog." );
		ue.log();

		PostMessage( WM_CLOSE );
	}
}
//-------------------------------------------------------------------------------------------------
