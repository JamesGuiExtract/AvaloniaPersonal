// DatabasePage.cpp : implementation file
//

#include "stdafx.h"
#include "FAMUtils.h"
#include "DatabasePage.h"
#include "DialogAdvanced.h"
#include "DialogSelect.h"
#include "DotNetUtils.h"
#include "FAMUtilsConstants.h"
#include "ADOUtils.h"
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
// Statics
//-------------------------------------------------------------------------------------------------
HCURSOR DatabasePage::g_hHandCursor = LoadCursor(NULL, IDC_HAND);

//-------------------------------------------------------------------------------------------------
// DatabasePage dialog
//-------------------------------------------------------------------------------------------------

IMPLEMENT_DYNAMIC(DatabasePage, CPropertyPage)

//-------------------------------------------------------------------------------------------------
DatabasePage::DatabasePage()
	:  CPropertyPage(DatabasePage::IDD),
	m_zServer(""),
	m_zDBName(""),
	m_zAdvConnStrProperties(""),
	m_bInitialized(false),
	m_pNotifyDBConfigChangedObject(NULL),
	m_bBrowseEnabled(true),
	m_bShowDBServerTag(false),
	m_bShowDBNameTag(false),
	m_crWorkflowWarningTextColor(RGB(255,0,0) /* Red */)
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
	DDX_Text(pDX, IDC_EDIT_CONN_STR, m_zAdvConnStrProperties);
	DDX_Control(pDX, IDC_EDIT_CONNECT_STATUS, m_editConnectStatus);
	DDX_Control(pDX, IDC_BUTTON_DB_NAME_BROWSE, m_btnBrowseDB);
	DDX_Control(pDX, IDC_EDIT_DB_SERVER, m_editDBServer);
	DDX_Control(pDX, IDC_EDIT_DB_NAME, m_editDBName);
	DDX_Control(pDX, IDC_EDIT_CONN_STR, m_editAdvConnStrProperties);
	DDX_Control(pDX, IDC_STATIC_WORKFLOW_WARNING, m_labelWorkflowWarning);
	DDX_Control(pDX, IDC_BUTTON_REFRESH, m_btnRefresh);
	DDX_Control(pDX, IDC_BUTTON_SQL_SERVER_BROWSE, m_btnSqlServerBrowse);
	DDX_Control(pDX, IDC_BUTTON_LAST_USED_DB, m_btnConnectLastUsedDB);
	DDX_Control(pDX, IDC_BUTTON_CONN_STR, m_btnAdvConnStrProperties);
	DDX_Control(pDX, IDC_BUTTON_USE_CURRENT_CONTEXT, m_btnUseCurrentContextDatabase);
	DDX_Control(pDX, IDC_BUTTON_SELECT_CONTEXT, m_btnSelectContext);
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
	ON_BN_CLICKED(IDC_BUTTON_CONN_STR, &DatabasePage::OnBnClickedButtonAdvConnStrProperties)
	ON_BN_CLICKED(IDC_BUTTON_USE_CURRENT_CONTEXT, &DatabasePage::OnBnClickedButtonUseCurrentContextDatabase)
	ON_BN_CLICKED(IDC_BUTTON_SELECT_CONTEXT, &DatabasePage::OnBnClickedButtonSelectContext)
	ON_WM_CTLCOLOR()
	ON_WM_SETCURSOR()
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
		
		CFont *pFont = m_labelWorkflowWarning.GetFont();
		LOGFONT lf;
		pFont->GetLogFont(&lf);
		lf.lfHeight = lf.lfHeight + lf.lfHeight / 2;
		m_fontWorkflowWarning.CreateFontIndirect(&lf);
		m_labelWorkflowWarning.SetFont(&m_fontWorkflowWarning);
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
		dlgSelect.showDBServerTag(m_bShowDBServerTag);

		if (dlgSelect.DoModal() == IDOK)
		{
			// If the server has changed need to clear the selected DB
			if (m_zServer != dlgSelect.m_zComboValue)
			{
				// Clear the database value
				setDatabase("");

				// Set the server value
				setServer((LPCTSTR)dlgSelect.m_zComboValue);

				// Update the UI
				UpdateData(FALSE);
			}
			
			// change the focus to the browse db button
			m_btnBrowseDB.SetFocus();

			// Notify the objects of config change
			notifyObjects();
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
		dlgSelectDB.showDBNameTag(m_bShowDBNameTag);

		if (dlgSelectDB.DoModal() == IDOK)
		{
			setDatabase((LPCTSTR)dlgSelectDB.m_zComboValue);
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
		refreshConnection();
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
		CRect rectDlg, rectServer, rectBrowseBtn, rectRefreshBtn, rectConnectLastDBBtn,
			rectCurrContextDBBtn, rectSelectContextBtn;

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

		// Get the ConnectLastDB buttons control's window rect
		m_btnConnectLastUsedDB.GetWindowRect(&rectConnectLastDBBtn);
		ScreenToClient(&rectConnectLastDBBtn);

		m_btnConnectLastUsedDB.GetWindowRect(&rectCurrContextDBBtn);
		ScreenToClient(&rectCurrContextDBBtn);

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

		// Resize the db browse button
		rectBrowseBtn.top = rectResize.top;
		rectBrowseBtn.bottom = rectResize.bottom;
		m_btnBrowseDB.MoveWindow(&rectBrowseBtn);

		// Resize the advanced connection properties edit control
		m_editAdvConnStrProperties.GetWindowRect(&rectResize);
		ScreenToClient(&rectResize);
		rectResize.left = rectServer.left;
		rectResize.right = rectServer.right; 
		m_editAdvConnStrProperties.MoveWindow(&rectResize);

		// Resize the advanced connection properties edit button
		rectBrowseBtn.top = rectResize.top;
		rectBrowseBtn.bottom = rectResize.bottom;
		m_btnAdvConnStrProperties.MoveWindow(&rectBrowseBtn);

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
		rectConnectLastDBBtn.top = rectResize.bottom + (iDistBetween * 2);
		rectConnectLastDBBtn.bottom = rectConnectLastDBBtn.top + rectRefreshBtn.Height();
		m_btnConnectLastUsedDB.MoveWindow(rectConnectLastDBBtn);

		// Move the use current context database button
		rectCurrContextDBBtn.left = rectConnectLastDBBtn.left - iDistBetween - rectConnectLastDBBtn.Width();
		rectCurrContextDBBtn.right = rectConnectLastDBBtn.left - iDistBetween;
		rectCurrContextDBBtn.top = rectConnectLastDBBtn.top;
		rectCurrContextDBBtn.bottom = rectConnectLastDBBtn.bottom;
		m_btnUseCurrentContextDatabase.MoveWindow(rectCurrContextDBBtn);

		// Move the select context button
		rectSelectContextBtn.left = rectCurrContextDBBtn.left - rectCurrContextDBBtn.Width();
		rectSelectContextBtn.right = rectCurrContextDBBtn.left - iDistBetween;
		rectSelectContextBtn.top = rectCurrContextDBBtn.top;
		rectSelectContextBtn.bottom = rectCurrContextDBBtn.bottom;
		m_btnSelectContext.MoveWindow(rectSelectContextBtn);

		// Size the workflow warning
		positionWorkflowWarningLabel();
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
		string strServer, strDatabase, strAdvConnStrProperties;
		ma_pCfgMgr->getLastGoodDBSettings(strServer, strDatabase, strAdvConnStrProperties);

		// set the server and database
		setConnectionInfo(strServer, strDatabase, strAdvConnStrProperties, true);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20381");
}	
//-------------------------------------------------------------------------------------------------
void DatabasePage::OnBnClickedButtonAdvConnStrProperties()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CDialogAdvanced dlgAdvanced((LPCTSTR)m_zServer, (LPCTSTR)m_zDBName,
			(LPCSTR)m_zAdvConnStrProperties);
		if (dlgAdvanced.DoModal() == IDOK)
		{
			m_zAdvConnStrProperties = dlgAdvanced.getAdvConnStrProperties().c_str();
			
			string strServer;
			if (dlgAdvanced.getServer(strServer))
			{
				m_zServer = strServer.c_str();
			}
			string strDatabase;
			if (dlgAdvanced.getDatabase(strDatabase))
			{
				m_zDBName = strDatabase.c_str();
			}

			UpdateData(FALSE);

			// Notify the objects of config change
			notifyObjects();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI35134");
}
//-------------------------------------------------------------------------------------------------
void DatabasePage::OnBnClickedButtonUseCurrentContextDatabase()
{
	try
	{
		m_zServer = gstrDATABASE_SERVER_TAG.c_str();
		m_zDBName = gstrDATABASE_NAME_TAG.c_str();
		setServer(gstrDATABASE_SERVER_TAG);
		setDatabase(gstrDATABASE_NAME_TAG);

		UpdateData(FALSE);

		notifyObjects();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI38064");
}
//-------------------------------------------------------------------------------------------------
void DatabasePage::OnBnClickedButtonSelectContext()
{
	try
	{
		bool bDBTagsAvailable = false;
		if (m_pNotifyDBConfigChangedObject != __nullptr)
		{
			if (!m_pNotifyDBConfigChangedObject->PromptToSelectContext(bDBTagsAvailable))
			{
				return;
			}
		}

		if (bDBTagsAvailable)
		{
			m_zServer = gstrDATABASE_SERVER_TAG.c_str();
			m_zDBName = gstrDATABASE_NAME_TAG.c_str();
			setServer(gstrDATABASE_SERVER_TAG);
			setDatabase(gstrDATABASE_NAME_TAG);
		}

		UpdateData(FALSE);

		notifyObjects();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI39299");
}
//-------------------------------------------------------------------------------------------------
HBRUSH DatabasePage::OnCtlColor(CDC* pDC, CWnd* pWnd, UINT nCtlColor)
{
	HBRUSH hbr = __nullptr;

	try
	{
		// Call the base
		hbr = CDialog::OnCtlColor(pDC, pWnd, nCtlColor);

		// if this is the workflow warning control set the color
		if (pWnd->GetDlgCtrlID() == m_labelWorkflowWarning.GetDlgCtrlID())
		{
			// Set the text color to the workflow warning control
			pDC->SetTextColor(m_crWorkflowWarningTextColor);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI38095");

	return hbr;
}
//-------------------------------------------------------------------------------------------------
// Public methods
//-------------------------------------------------------------------------------------------------
void DatabasePage::setConnectionInfo(const std::string& strSQLServer, const std::string& strDBName,
	const string& strAdvConnStrProperties, bool bNotifyObjects)
{
	// Assign m_zAdvConnStrProperties so that it can be overridden by strSQLServer or strDBName
	// if necessary.
	m_zAdvConnStrProperties = strAdvConnStrProperties.c_str();
	setServer(strSQLServer);
	setDatabase(strDBName);

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
		ma_pCfgMgr->setLastGoodDBSettings(
			m_strCurrServer, m_strCurrDBName, m_strCurrAdvConnStrProperties);
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
	m_zAdvConnStrProperties = "";
	
	// Clear the connection Status
	setDBConnectionStatus("");

	// Update the fields on the page
	UpdateData(FALSE);

	// Notify the object of change
	notifyObjects();
}
//-------------------------------------------------------------------------------------------------
void DatabasePage::refreshConnection()
{
	// Update control member variables
	UpdateData();

	// Notify the objects of config change
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
		m_btnAdvConnStrProperties.EnableWindow(asMFCBool(m_bBrowseEnabled));
		m_btnConnectLastUsedDB.ShowWindow(m_bBrowseEnabled ? SW_SHOW : SW_HIDE);
		m_btnUseCurrentContextDatabase.ShowWindow(m_bBrowseEnabled ? SW_SHOW : SW_HIDE);
		m_btnSelectContext.ShowWindow(m_bBrowseEnabled ? SW_SHOW : SW_HIDE);
	}
}
//-------------------------------------------------------------------------------------------------
void DatabasePage::showDBServerTag(bool bShowDBServerTag)
{
	m_bShowDBServerTag = bShowDBServerTag;
}
//-------------------------------------------------------------------------------------------------
void DatabasePage::showDBNameTag(bool bShowDBNameTag)
{
	m_bShowDBNameTag = bShowDBNameTag;
}
//-------------------------------------------------------------------------------------------------
void DatabasePage::updateLastUsedDBButton()
{
	// Get the last used db info from the registry
	string strServer(""), strDatabase(""), strAdvConnStrProperties("");
	ma_pCfgMgr->getLastGoodDBSettings(strServer, strDatabase, strAdvConnStrProperties);

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
	m_editAdvConnStrProperties.EnableWindow(bEnable);
}
//-------------------------------------------------------------------------------------------------
void DatabasePage::setCurrentContextState(bool bFPSSaved, bool bValidContext, 
										  const string& strContextName)
{
	if (m_pNotifyDBConfigChangedObject != nullptr)
	{
		m_btnUseCurrentContextDatabase.EnableWindow(asMFCBool(bValidContext));

		m_btnSelectContext.EnableWindow(!bValidContext || !bFPSSaved);
		m_btnSelectContext.SetWindowText((!bValidContext && bFPSSaved)
			? "Create context..."
			: "Select context...");
	}
	else
	{
		m_btnUseCurrentContextDatabase.EnableWindow(FALSE);
		m_btnSelectContext.EnableWindow(FALSE);
	}
}
//-------------------------------------------------------------------------------------------------
void DatabasePage::positionWorkflowWarningLabel()
{
	CString zText;
	m_labelWorkflowWarning.GetWindowText(zText);

	CClientDC dc(this);
	CFont *pFont = dc.SelectObject(m_labelWorkflowWarning.GetFont());
	dc.SelectObject(&pFont);
	CSize textSize = dc.GetTextExtent(zText, zText.GetLength());

	CRect rectPage, rectLabel;
	GetClientRect(&rectPage);
	m_labelWorkflowWarning.GetWindowRect(&rectLabel);
	ScreenToClient(&rectLabel);
	
	rectLabel.top = rectPage.bottom - 5 - textSize.cy;
	rectLabel.bottom = rectPage.bottom - 5;
	rectLabel.left = (rectPage.Width() - textSize.cx) / 2 - 1;
	rectLabel.right = (rectPage.Width() + textSize.cx) / 2;

	m_labelWorkflowWarning.MoveWindow(&rectLabel);
}
//-------------------------------------------------------------------------------------------------
void DatabasePage::showWorkflowWarning(bool bShow)
{
	m_labelWorkflowWarning.ShowWindow((bShow) ? SW_SHOW : SW_HIDE);
}


//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void DatabasePage::notifyObjects()
{
	// Notify the dbConfigChange object
	if (m_pNotifyDBConfigChangedObject != __nullptr)
	{
		m_strCurrServer = string(m_zServer);
		m_strCurrDBName = string(m_zDBName);
		m_strCurrAdvConnStrProperties = string(m_zAdvConnStrProperties);

		m_pNotifyDBConfigChangedObject->OnDBConfigChanged(
			m_strCurrServer, m_strCurrDBName, m_strCurrAdvConnStrProperties);
	}
}
//-------------------------------------------------------------------------------------------------
void DatabasePage::setServer(const string& strServer)
{
	m_zServer = strServer.c_str();
	string strAdvConnStrProperties = (LPCTSTR)m_zAdvConnStrProperties;

	if (findConnectionStringProperty(strAdvConnStrProperties, gstrSERVER))
	{
		updateConnectionStringProperties(strAdvConnStrProperties,
			gstrSERVER + "=" + strServer);
	}
	if (findConnectionStringProperty(strAdvConnStrProperties, gstrDATA_SOURCE))
	{
		updateConnectionStringProperties(strAdvConnStrProperties,
			gstrDATA_SOURCE + "=" + strServer);
	}

	m_zAdvConnStrProperties = strAdvConnStrProperties.c_str();
}
//-------------------------------------------------------------------------------------------------
void DatabasePage::setDatabase(const string& strDatabase)
{
	m_zDBName = strDatabase.c_str();
	string strAdvConnStrProperties = (LPCTSTR)m_zAdvConnStrProperties;

	if (findConnectionStringProperty(strAdvConnStrProperties, gstrDATABASE))
	{
		updateConnectionStringProperties(strAdvConnStrProperties,
			gstrDATABASE + "=" + strDatabase);
	}
	if (findConnectionStringProperty(strAdvConnStrProperties, gstrINITIAL_CATALOG))
	{
		updateConnectionStringProperties(strAdvConnStrProperties,
			gstrINITIAL_CATALOG + "=" + strDatabase);
	}

	m_zAdvConnStrProperties = strAdvConnStrProperties.c_str();
}