// SelectDBDialog.cpp : implementation file
//

#include "stdafx.h"
#include "SelectDBDialog.h"

#include <cpputil.h>
#include <COMUtils.h>
#include <DotNetUtils.h>
#include <ExtractMFCUtils.h>
#include <UCLIDException.h>
#include <DialogAdvanced.h>
#include <ADOUtils.h>

//-------------------------------------------------------------------------------------------------
// SelectDBDialog dialog
//-------------------------------------------------------------------------------------------------

IMPLEMENT_DYNAMIC(SelectDBDialog, CDialog)

SelectDBDialog::SelectDBDialog(UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFAMDB,
		string strPrompt, bool bAllowCreation, bool bRequireAdminLogin, CWnd* pParent /*=NULL*/)
	: CDialog(SelectDBDialog::IDD, pParent),
	m_zServerName(""),
	m_zDBName(""),
	m_zAdvConnStrProperties(""),
	m_eOptionsDatabaseGroup(kLoginExisting),
	m_ipFAMDB(ipFAMDB),
	m_comboServerName(DBInfoCombo::kServerName),
	m_comboDBName(DBInfoCombo::kDatabaseName),
	m_zPrompt((LPCTSTR)strPrompt.c_str()),
	m_bAllowCreation(bAllowCreation),
	m_bRequireAdminLogin(bRequireAdminLogin)
{
	try
	{
		// ipFAMDB must exist
		ASSERT_ARGUMENT("ELI17557", ipFAMDB != __nullptr);

		// Load the Icon
		m_hIcon = AfxGetApp()->LoadIcon(IDI_ICON_FAMDBADMIN);

		ma_pCfgMgr = unique_ptr<FileProcessingConfigMgr>(new
			FileProcessingConfigMgr());
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI17538");
}
//-------------------------------------------------------------------------------------------------
SelectDBDialog::~SelectDBDialog()
{
	try
	{
		m_ipFAMDB = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI17561");
}
//-------------------------------------------------------------------------------------------------
void SelectDBDialog::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_COMBO_SELECT_DB_SERVER, m_comboServerName);
	DDX_Control(pDX, IDC_COMBO_SELECT_DB_NAME, m_comboDBName);
	DDX_CBString(pDX, IDC_COMBO_SELECT_DB_SERVER, m_zServerName);
	DDX_CBString(pDX, IDC_COMBO_SELECT_DB_NAME, m_zDBName);
	DDX_Text(pDX, IDC_EDIT_CONN_STR, m_zAdvConnStrProperties);
	DDX_Radio(pDX, IDC_RADIO_LOGIN_EXISTING, (int &)m_eOptionsDatabaseGroup);
}

//-------------------------------------------------------------------------------------------------
// Message Map
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(SelectDBDialog, CDialog)
	ON_BN_CLICKED(IDCLOSE, &SelectDBDialog::OnBnClickedClose)
	ON_CBN_KILLFOCUS(IDC_COMBO_SELECT_DB_SERVER, &SelectDBDialog::OnCbnKillfocusComboSelectDbServer)
	ON_BN_CLICKED(IDC_BUTTON_CONN_STR, &SelectDBDialog::OnBnClickedButtonAdvanced)
	ON_CBN_EDITCHANGE(IDC_COMBO_SELECT_DB_SERVER, OnChangeServerName)
	ON_CBN_EDITCHANGE(IDC_COMBO_SELECT_DB_NAME, OnChangeDBName)
	ON_CBN_SELCHANGE(IDC_COMBO_SELECT_DB_SERVER, OnSelChangeServerName)
	ON_CBN_SELCHANGE(IDC_COMBO_SELECT_DB_NAME, OnSelChangeDBName)
	ON_WM_CLOSE()
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// SelectDBDialog message handlers
//-------------------------------------------------------------------------------------------------
BOOL SelectDBDialog::OnInitDialog()
{
	try
	{
		// Set the icon for this dialog.  The framework does this automatically
		// when the application's main window is not a dialog
		SetIcon(m_hIcon, TRUE);			// Set big icon
		SetIcon(m_hIcon, FALSE);		// Set small icon

		// Set the window title.
		SetWindowText(m_zPrompt);

		if (!m_bAllowCreation)
		{
			// Hide the radio buttons to choose between logging into to an existing DB or creating
			// a new one.
			hideCreationOption();
		}

		if (!m_bRequireAdminLogin)
		{
			// If no login is required, change "Next" to "OK"
			GetDlgItem(IDOK)->SetWindowTextA("OK");
		}

		// Load the Server and database from the FAMDB
		m_zServerName = asString(m_ipFAMDB->DatabaseServer).c_str();
		m_zDBName = asString(m_ipFAMDB->DatabaseName).c_str();
		m_zAdvConnStrProperties = asString(m_ipFAMDB->AdvancedConnectionStringProperties).c_str();

		// If the server and database in the FAMDB are empty set them from the registry
		if ( m_zServerName.IsEmpty() && m_zDBName.IsEmpty() )
		{
			string strServer, strDatabase, strAdvConnStrProperties;
			ma_pCfgMgr->getLastGoodDBSettings(strServer, strDatabase, strAdvConnStrProperties);

			// Set the values in the database object
			m_ipFAMDB->DatabaseServer = strServer.c_str();
			m_ipFAMDB->DatabaseName = strDatabase.c_str();
			m_ipFAMDB->AdvancedConnectionStringProperties = strAdvConnStrProperties.c_str();

			// Set the member values
			m_zServerName = strServer.c_str();
			m_zDBName = strDatabase.c_str();
			m_zAdvConnStrProperties = strAdvConnStrProperties.c_str();
		}

		// Set the server for the DB name combo
		m_comboDBName.setSQLServer((LPCSTR)m_zServerName);

		CDialog::OnInitDialog();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17460");
	
	return TRUE;
}
//-------------------------------------------------------------------------------------------------
void SelectDBDialog::OnBnClickedClose()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		EndDialog(IDCLOSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17464");
}
//-------------------------------------------------------------------------------------------------
void SelectDBDialog::OnCancel()
{
	// Stubbed in to prevent dialog closing on esc pressed
}
//-------------------------------------------------------------------------------------------------
void SelectDBDialog::OnClose()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		EndDialog(IDCLOSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI26653");
}
//-------------------------------------------------------------------------------------------------
void SelectDBDialog::OnOK()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Show the wait cursor
		CWaitCursor waitCursor;

		// Preset the Login variables
		VARIANT_BOOL bLoginCanceled = VARIANT_FALSE;
		VARIANT_BOOL bLoginValid = VARIANT_FALSE;	

		// Get the data from the Dialog
		UpdateData();

		// Set the Database server and name
		m_ipFAMDB->DatabaseServer = (LPCSTR)m_zServerName;
		m_ipFAMDB->DatabaseName = (LPCSTR)m_zDBName;
		m_ipFAMDB->AdvancedConnectionStringProperties = (LPCSTR)m_zAdvConnStrProperties;

		//  Check for create database
		if ( m_eOptionsDatabaseGroup == kCreateNew )
		{
			// Create the database
			m_ipFAMDB->CreateNewDB(m_zDBName.operator LPCSTR());
		}

		// Try to login to the database and display Admin window
		try
		{
			try
			{
				// Attempt login if m_bRequireAdminLogin
				if (m_bRequireAdminLogin)
				{
					// Reset the database connection to get the connection representing the correct 
					// Server and database
					try
					{
						m_ipFAMDB->ResetDBConnection();
					}
					CATCH_AND_LOG_ALL_EXCEPTIONS("ELI18162");

					bLoginValid = m_ipFAMDB->ShowLogin(VARIANT_TRUE, &bLoginCanceled);

					// If login is valid, return IDOK
					if (asCppBool(bLoginValid))
					{
						__super::OnOK();
					}
					else if (!asCppBool(bLoginCanceled))
					{
						MessageBox("Login failed.\r\n\r\nPlease ensure you are using the correct password and try again.", 
							"Login failed", MB_OK | MB_ICONERROR );
					}
				}
				else
				{
					// Attempt to connect to the specified database (with no login prompt)
					try
					{
						m_ipFAMDB->ResetDBConnection();
					}
					CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI35758");

					__super::OnOK(); 
				}
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI17535");
		}
		catch(UCLIDException ue)
		{
			UCLIDException uexOuter("ELI17537", 
				"The database may not exist. Verify the server and database and try again.", ue);
			uexOuter.display();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17465");
}
//-------------------------------------------------------------------------------------------------
void SelectDBDialog::OnCbnKillfocusComboSelectDbServer()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Get data from the UI
		UpdateData();

		// Set the server for the m_comboDBName
		m_comboDBName.setSQLServer((LPCSTR)m_zServerName);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18119");
}
//-------------------------------------------------------------------------------------------------
void SelectDBDialog::OnBnClickedButtonAdvanced()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CDialogAdvanced dlgAdvanced((LPCTSTR)m_zServerName, (LPCTSTR)m_zDBName, 
			(LPCSTR)m_zAdvConnStrProperties);
		if (dlgAdvanced.DoModal() == IDOK)
		{
			m_zAdvConnStrProperties = dlgAdvanced.getAdvConnStrProperties().c_str();
			
			string strServer;
			if (dlgAdvanced.getServer(strServer))
			{
				m_zServerName = strServer.c_str();
			}
			string strDatabase;
			if (dlgAdvanced.getDatabase(strDatabase))
			{
				m_zDBName = strDatabase.c_str();
			}

			UpdateData(FALSE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18119");
}
//--------------------------------------------------------------------------------------------------
void SelectDBDialog::OnChangeServerName()
{
	try
	{
		UpdateData(TRUE);

		updateAdvConnStrProperties();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI35143");
}
//--------------------------------------------------------------------------------------------------
void SelectDBDialog::OnChangeDBName()
{
	try
	{
		UpdateData(TRUE);

		updateAdvConnStrProperties();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI35144");
}
//--------------------------------------------------------------------------------------------------
void SelectDBDialog::OnSelChangeServerName()
{
	try
	{
		// Get index of selection
		int iIndex = m_comboServerName.GetCurSel();

		if (iIndex >= 0)
		{
			// Retrieve name of the newly selected value.
			m_comboServerName.GetLBText(iIndex, m_zServerName);

			updateAdvConnStrProperties();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI35145");
}
//--------------------------------------------------------------------------------------------------
void SelectDBDialog::OnSelChangeDBName()
{
	try
	{
		// Get index of selection
		int iIndex = m_comboDBName.GetCurSel();

		if (iIndex >= 0)
		{
			// Retrieve name of the newly selected value.
			m_comboDBName.GetLBText(iIndex, m_zDBName);

			updateAdvConnStrProperties();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI35146");
}

//-------------------------------------------------------------------------------------------------
// Private Members
//-------------------------------------------------------------------------------------------------
void SelectDBDialog::updateAdvConnStrProperties()
{
	string strAdvConnStrProperties = (LPCTSTR)m_zAdvConnStrProperties;

	if (findConnectionStringProperty(strAdvConnStrProperties, gstrSERVER))
	{
		updateConnectionStringProperties(strAdvConnStrProperties,
			gstrSERVER + "=" + (LPCTSTR)m_zServerName);
	}
	if (findConnectionStringProperty(strAdvConnStrProperties, gstrDATA_SOURCE))
	{
		updateConnectionStringProperties(strAdvConnStrProperties,
			gstrDATA_SOURCE + "=" + (LPCTSTR)m_zServerName);
	}
	if (findConnectionStringProperty(strAdvConnStrProperties, gstrDATABASE))
	{
		updateConnectionStringProperties(strAdvConnStrProperties,
			gstrDATABASE + "=" + (LPCTSTR)m_zDBName);
	}
	if (findConnectionStringProperty(strAdvConnStrProperties, gstrINITIAL_CATALOG))
	{
		updateConnectionStringProperties(strAdvConnStrProperties,
			gstrINITIAL_CATALOG + "=" + (LPCTSTR)m_zDBName);
	}

	m_zAdvConnStrProperties = strAdvConnStrProperties.c_str();

	// Update only the text of the connection string control to avoid resetting selection in the
	// database or server controls.
	GetDlgItem(IDC_EDIT_CONN_STR)->SetWindowText(m_zAdvConnStrProperties);
}
//-------------------------------------------------------------------------------------------------
void SelectDBDialog::hideCreationOption()
{
	CWnd *pExistingRadioButton = GetDlgItem(IDC_RADIO_LOGIN_EXISTING);
	CWnd *pNewRadioButton = GetDlgItem(IDC_RADIO_CREATE_NEW_DB);
	CWnd *pDBNameLabel = GetDlgItem(IDC_DB_NAME_LABEL);
	CWnd *pDBNameCombo = GetDlgItem(IDC_COMBO_SELECT_DB_NAME);
	CWnd *pAdvPropLabel = GetDlgItem(IDC_ADV_PROP_LABEL);
	CWnd *pConnStrEditBox = GetDlgItem(IDC_EDIT_CONN_STR);
	CWnd *pConnStrBrowseBtn = GetDlgItem(IDC_BUTTON_CONN_STR);
	CWnd *pDBGroupBox = GetDlgItem(IDC_DB_GROUP_BOX);
	CWnd *pOKButton = GetDlgItem(IDOK);
	CWnd *pCloseButton = GetDlgItem(IDCLOSE);

	CRect rect;
	pExistingRadioButton->GetWindowRect(&rect);
	ScreenToClient(&rect);
	int nTop = rect.top;
			
	pExistingRadioButton->ShowWindow(SW_HIDE);
	pNewRadioButton->ShowWindow(SW_HIDE);
	
	pDBNameLabel->GetWindowRect(&rect);
	ScreenToClient(&rect);

	int nOffset = nTop - rect.top;
	rect.OffsetRect(0, nOffset);
	pDBNameLabel->MoveWindow(rect);

	pDBNameCombo->GetWindowRect(&rect);
	ScreenToClient(&rect);
	rect.OffsetRect(0, nOffset);
	pDBNameCombo->MoveWindow(rect);

	pAdvPropLabel->GetWindowRect(&rect);
	ScreenToClient(&rect);
	rect.OffsetRect(0, nOffset);
	pAdvPropLabel->MoveWindow(rect);

	pConnStrEditBox->GetWindowRect(&rect);
	ScreenToClient(&rect);
	rect.OffsetRect(0, nOffset);
	pConnStrEditBox->MoveWindow(rect);

	pConnStrBrowseBtn->GetWindowRect(&rect);
	ScreenToClient(&rect);
	rect.OffsetRect(0, nOffset);
	pConnStrBrowseBtn->MoveWindow(rect);

	pDBGroupBox->GetWindowRect(&rect);
	ScreenToClient(&rect);
	rect.bottom += nOffset;
	pDBGroupBox->MoveWindow(rect);

	pOKButton->GetWindowRect(&rect);
	ScreenToClient(&rect);
	rect.OffsetRect(0, nOffset);
	pOKButton->MoveWindow(rect);

	pCloseButton->GetWindowRect(&rect);
	ScreenToClient(&rect);
	rect.OffsetRect(0, nOffset);
	pCloseButton->MoveWindow(rect);

	GetWindowRect(&rect);
	rect.bottom += nOffset;
	MoveWindow(rect);
}