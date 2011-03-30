// DatabasePage.cpp : implementation file
//

#include "stdafx.h"
#include "FAMUtils.h"
#include "DatabasePage.h"
#include "DialogSelect.h"
#include "DotNetUtils.h"
#include "FAMUtilsConstants.h"

#include <cpputil.h>
#include <misc.h>
#include <UCLIDException.h>
#include <StringTokenizer.h>
#include <LoadFileDlgThread.h>
#include <COMUtils.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const string gstrDBCFG_FILE_EXTENSION = ".dbcfg";
const string gstrDBCFG_FILE_FILTER =  "Database Config Files (*" + 
											gstrDBCFG_FILE_EXTENSION +
											")|*" + 
											gstrDBCFG_FILE_EXTENSION;
const string gstrALL_FILE_FILTER = "All Files (*.*)|*.*||";
const string gstrDBCFG_FILE_OPEN_FILTER = gstrDBCFG_FILE_FILTER + 
											"|" +
											gstrALL_FILE_FILTER;

//-------------------------------------------------------------------------------------------------
// DatabasePage dialog
//-------------------------------------------------------------------------------------------------

IMPLEMENT_DYNAMIC(DatabasePage, CPropertyPage)

//-------------------------------------------------------------------------------------------------
DatabasePage::DatabasePage()
	:  CPropertyPage(DatabasePage::IDD),
	m_zServer(""),
	m_zDBName(""),
	m_bInitialized(false),
	m_pNotifyDBConfigChangedObject(NULL),
	m_bBrowseEnabled(true)
{
	try
	{
		ma_pCfgMgr = unique_ptr<FileProcessingConfigMgr>(new
			FileProcessingConfigMgr());
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI16730");
}
//-------------------------------------------------------------------------------------------------
DatabasePage::~DatabasePage()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16569");
}
//-------------------------------------------------------------------------------------------------
void DatabasePage::DoDataExchange(CDataExchange* pDX)
{
	CPropertyPage::DoDataExchange(pDX);
	DDX_Text(pDX, IDC_EDIT_DB_SERVER, m_zServer);
	DDX_Text(pDX, IDC_EDIT_DB_NAME, m_zDBName);
	DDX_Control(pDX, IDC_EDIT_CONNECT_STATUS, m_editConnectStatus);
	DDX_Control(pDX, IDC_BUTTON_DB_NAME_BROWSE, m_btnBrowseDB);
	DDX_Control(pDX, IDC_EDIT_DB_SERVER, m_editDBServer);
	DDX_Control(pDX, IDC_EDIT_DB_NAME, m_editDBName);
	DDX_Control(pDX, IDC_BUTTON_REFRESH, m_btnRefresh);
	DDX_Control(pDX, IDC_BUTTON_SQL_SERVER_BROWSE, m_btnSqlServerBrowse);
	DDX_Control(pDX, IDC_BUTTON_LAST_USED_DB, m_btnConnectLastUsedDB);
}

//-------------------------------------------------------------------------------------------------
// Message Map
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(DatabasePage, CPropertyPage)
	ON_BN_CLICKED(IDC_BUTTON_SQL_SERVER_BROWSE, &DatabasePage::OnBnClickedButtonBrowseServer)
	ON_BN_CLICKED(IDC_BUTTON_DB_NAME_BROWSE, &DatabasePage::OnBnClickedButtonBrowseDB)
	ON_BN_CLICKED(IDC_BUTTON_REFRESH, &DatabasePage::OnBnClickedButtonRefresh)
	ON_WM_SIZE()
	ON_BN_CLICKED(IDC_BUTTON_LAST_USED_DB, &DatabasePage::OnBnClickedButtonLastUsedDb)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// DatabasePage message handlers
//-------------------------------------------------------------------------------------------------
BOOL DatabasePage::OnInitDialog()
{
	try
	{
		CPropertyPage::OnInitDialog();

		// set flag to indicate OnInitDialog has been called
		m_bInitialized = true;

		// Call the setBrowsEnabled using the flag
		setBrowseEnabled(m_bBrowseEnabled);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16161");
	
	return TRUE;
}
//-------------------------------------------------------------------------------------------------
void DatabasePage::OnBnClickedButtonBrowseServer()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CDialogSelect dlgSelect;
		if (dlgSelect.DoModal() == IDOK)
		{
			// If the server has changed need to clear the selected DB
			if (m_zServer != dlgSelect.m_zComboValue)
			{
				// Clear the database value
				m_zDBName = "";
				
				// Set the server value
				m_zServer = dlgSelect.m_zComboValue;

				// Update the UI
				UpdateData(FALSE);
			}
			
			// change the focus to the browse db button
			m_btnBrowseDB.SetFocus();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17504");

}
//-------------------------------------------------------------------------------------------------
void DatabasePage::OnBnClickedButtonBrowseDB()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		string strServer = m_zServer;
		if (strServer.empty())
		{
			UCLIDException ue("ELI17507", "Server must be defined!" );
			throw ue;
		}

		CDialogSelect dlgSelectDB(strServer);
		if (dlgSelectDB.DoModal() == IDOK)
		{
			m_zDBName = dlgSelectDB.m_zComboValue;
			UpdateData(FALSE);
		}

		// Notify the objects of config change
		notifyObjects();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17505");
}
//-------------------------------------------------------------------------------------------------
void DatabasePage::OnBnClickedButtonRefresh()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Update control member variables
		UpdateData();

		// Notify the objects of config change
		notifyObjects();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16110");
}
//-------------------------------------------------------------------------------------------------
void DatabasePage::OnSize(UINT nType, int cx, int cy)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CPropertyPage::OnSize(nType, cx, cy);

		// Dialog has not been initialized return
		if ( !m_bInitialized )
		{
			return;
		}

		int iLRMargin, iDistBetween, iBrowseBtnWidth, iRefreshBtnWidth;
		CRect rectDlg, rectServer, rectBrowseBtn, rectRefreshBtn, rectConnectLastDBBtn;

		// Get the Pages client window
		GetClientRect(&rectDlg);

		// Resize the DB server edit control
		m_editDBServer.GetWindowRect(&rectServer);
		ScreenToClient(&rectServer);

		// Get the Browse button control's windows rect
		m_btnSqlServerBrowse.GetWindowRect(&rectBrowseBtn);
		ScreenToClient(&rectBrowseBtn);

		// Get the Refresh button control's windows rect
		m_btnRefresh.GetWindowRect(&rectRefreshBtn);
		ScreenToClient(&rectRefreshBtn);

		// Get the ConnectLastDB buttons constrol's window rect
		m_btnConnectLastUsedDB.GetWindowRect(&rectConnectLastDBBtn);
		ScreenToClient(&rectConnectLastDBBtn);

		// Calculate the space to leave to the right and left
		iLRMargin = rectServer.left - rectDlg.left;

		// Calculate the control spacing
		iDistBetween = rectBrowseBtn.left - rectServer.right;

		// Get the width of the browse button
		iBrowseBtnWidth = rectBrowseBtn.Width();

		// Get the width of the refresh button
		iRefreshBtnWidth = rectRefreshBtn.Width();

		// resize Server edit control
		rectServer.right = rectDlg.right - iLRMargin - iBrowseBtnWidth - iDistBetween;
		m_editDBServer.MoveWindow(&rectServer);

		// Move the browse button
		rectBrowseBtn.left = rectServer.right + iDistBetween;
		rectBrowseBtn.right = rectDlg.right - iLRMargin;
		m_btnSqlServerBrowse.MoveWindow(&rectBrowseBtn);

		// Rect to use for the other buttons
		CRect rectResize;

		// Resize the DB name edit control
		m_editDBName.GetWindowRect(&rectResize);
		ScreenToClient(&rectResize);
		rectResize.left = rectServer.left;
		rectResize.right = rectServer.right; 
		m_editDBName.MoveWindow(&rectResize);

		// resize the db browse button
		rectBrowseBtn.top = rectResize.top;
		rectBrowseBtn.bottom = rectResize.bottom;
		m_btnBrowseDB.MoveWindow(&rectBrowseBtn);

		// Resize the connect status edit control
		m_editConnectStatus.GetWindowRect(&rectResize);
		ScreenToClient(&rectResize);
		rectResize.right = rectDlg.right - iLRMargin - iRefreshBtnWidth - iDistBetween;
		m_editConnectStatus.MoveWindow(&rectResize);

		// Move the refresh button
		rectRefreshBtn.left = rectResize.right + iDistBetween;
		rectRefreshBtn.right = rectDlg.right - iLRMargin;
		m_btnRefresh.MoveWindow(&rectRefreshBtn);

		// Move the ConnectLastDB button
		rectConnectLastDBBtn.left = rectRefreshBtn.right - rectConnectLastDBBtn.Width();
		rectConnectLastDBBtn.right = rectRefreshBtn.right;
		rectConnectLastDBBtn.top = rectRefreshBtn.bottom + iDistBetween;
		rectConnectLastDBBtn.bottom = rectConnectLastDBBtn.top + rectRefreshBtn.Height();
		m_btnConnectLastUsedDB.MoveWindow(rectConnectLastDBBtn);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16160");
}
//-------------------------------------------------------------------------------------------------
BOOL DatabasePage::PreTranslateMessage(MSG* pMsg) 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Want any accelerator keys handled by the parent of the property sheet that 
		// contains this page.
		if ( pMsg->message == WM_KEYDOWN)
		{
			// Get the parent
			CWnd *pWnd = GetParent();
			if (pWnd)
			{
				// Get the grandparent
				CWnd *pGrandParent = pWnd->GetParent();
				if (pGrandParent)
				{
					// Pass the message on to the grand parent
					return pGrandParent->PreTranslateMessage(pMsg);
				}
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17645")
	
	return CDialog::PreTranslateMessage(pMsg);
}
//-------------------------------------------------------------------------------------------------
void DatabasePage::OnBnClickedButtonLastUsedDb()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	// need try catch block for [p13 #4856]
	// since this function is handled by the property page,
	// if it throws an exception the application will
	// crash due to an unhandled exception.
	try
	{
		// refresh the button state
		// (this prevents the button from staying
		// in the clicked state while it processes
		// the rest of the function)
		m_btnConnectLastUsedDB.UpdateWindow();

		// show the wait cursor
		CWaitCursor cWait;

		// Get the last used db info from the registry
		string strServer, strDatabase;
		ma_pCfgMgr->getLastGoodDBSettings(strServer, strDatabase);

		// set the server and database
		setServerAndDBName(strServer, strDatabase, true);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20381");
}	

//-------------------------------------------------------------------------------------------------
// Public methods
//-------------------------------------------------------------------------------------------------
void DatabasePage::setServerAndDBName(const std::string& strSQLServer, const std::string& strDBName, bool bNotifyObjects)
{
	m_zServer = strSQLServer.c_str();
	m_zDBName = strDBName.c_str();

	UpdateData(FALSE);

	if (bNotifyObjects)
	{
		notifyObjects();
	}
}
//-------------------------------------------------------------------------------------------------
void DatabasePage::setDBConnectionStatus( const string& strStatusString )
{
	// Set the status string
	m_editConnectStatus.SetWindowTextA( strStatusString.c_str() );

	// Save the settings if the connection was established
	if (strStatusString == gstrCONNECTION_ESTABLISHED)
	{
		ma_pCfgMgr->setLastGoodDBSettings(m_zServer.operator LPCSTR(), m_zDBName.operator LPCSTR());
	}
}
//-------------------------------------------------------------------------------------------------
void DatabasePage::setNotifyDBConfigChanged( IDBConfigNotifications* pNotifyObject )
{
	m_pNotifyDBConfigChangedObject = pNotifyObject;
}
//-------------------------------------------------------------------------------------------------
void DatabasePage::clear()
{
	m_zServer = "";
	m_zDBName = "";
	
	// Clear the connection Status
	setDBConnectionStatus("");

	// Update the fields on the page
	UpdateData(FALSE);

	// Notify the object of change
	notifyObjects();
}
//-------------------------------------------------------------------------------------------------
void DatabasePage::setBrowseEnabled(bool bBrowseEnabled)
{
	m_bBrowseEnabled = bBrowseEnabled;

	// Only call the EnableWindow method if the form has been initialized
	if (m_bInitialized)
	{
		m_btnBrowseDB.EnableWindow(asMFCBool(m_bBrowseEnabled));
		m_btnSqlServerBrowse.EnableWindow(asMFCBool(m_bBrowseEnabled));
		m_btnConnectLastUsedDB.ShowWindow(m_bBrowseEnabled ? SW_SHOW : SW_HIDE);
	}
}
//-------------------------------------------------------------------------------------------------
void DatabasePage::updateLastUsedDBButton()
{
	// Get the last used db info from the registry
	string strServer(""), strDatabase("");
	ma_pCfgMgr->getLastGoodDBSettings(strServer, strDatabase);

	// enable the button if there is a last used setting in the registry
	m_btnConnectLastUsedDB.EnableWindow(asMFCBool(!strServer.empty() && !strDatabase.empty()));
}
//-------------------------------------------------------------------------------------------------
void DatabasePage::enableAllControls(bool bEnableAll)
{
	// Get the enable value as an MFC bool
	BOOL bEnable = asMFCBool(bEnableAll);

	// Enable/disable the browse buttons
	setBrowseEnabled(bEnableAll);

	// Enable/disable the refresh button
	m_btnRefresh.EnableWindow(bEnable);

	// Enable/disable the last used DB button
	if (bEnableAll)
	{
		// Enable the button
		updateLastUsedDBButton();
	}
	else
	{
		// Disable the button
		m_btnConnectLastUsedDB.EnableWindow(FALSE);
	}

	// Enable/disable the edit controls
	m_editDBName.EnableWindow(bEnable);
	m_editDBServer.EnableWindow(bEnable);
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void DatabasePage::notifyObjects()
{
	// Notify the dbConfigChange object
	if (m_pNotifyDBConfigChangedObject != __nullptr)
	{
		m_pNotifyDBConfigChangedObject->OnDBConfigChanged(string(m_zServer), string(m_zDBName));
	}
}
//-------------------------------------------------------------------------------------------------
