// SelectDBDialog.cpp : implementation file
//

#include "stdafx.h"
#include "FAMDBAdmin.h"
#include "SelectDBDialog.h"
#include "FAMDBAdminDlg.h"

#include <cpputil.h>
#include <COMUtils.h>
#include <DotNetUtils.h>
#include <ExtractMFCUtils.h>
#include <UCLIDException.h>

//-------------------------------------------------------------------------------------------------
// SelectDBDialog dialog
//-------------------------------------------------------------------------------------------------

IMPLEMENT_DYNAMIC(SelectDBDialog, CDialog)

SelectDBDialog::SelectDBDialog(IFileProcessingDBPtr ipFAMDB, CWnd* pParent /*=NULL*/)
	: CDialog(SelectDBDialog::IDD, pParent),
	m_zServerName(""),
	m_zDBName(""),
	m_eOptionsDatabaseGroup(kLoginExisting),
	m_ipFAMDB(ipFAMDB),
	m_comboServerName(DBInfoCombo::kServerName),
	m_comboDBName(DBInfoCombo::kDatabaseName)
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
	DDX_Radio(pDX, IDC_RADIO_LOGIN_EXISTING, (int &)m_eOptionsDatabaseGroup);
}

//-------------------------------------------------------------------------------------------------
// Message Map
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(SelectDBDialog, CDialog)
	ON_BN_CLICKED(IDCLOSE, &SelectDBDialog::OnBnClickedClose)
	ON_CBN_KILLFOCUS(IDC_COMBO_SELECT_DB_SERVER, &SelectDBDialog::OnCbnKillfocusComboSelectDbServer)
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

		// Load the Server and database from the FAMDB
		m_zServerName = asString(m_ipFAMDB->DatabaseServer).c_str();
		m_zDBName = asString(m_ipFAMDB->DatabaseName).c_str();

		// If the server and database in the FAMDB are empty set them from the registry
		if ( m_zServerName.IsEmpty() && m_zDBName.IsEmpty() )
		{
			string strServer, strDatabase;
			ma_pCfgMgr->getLastGoodDBSettings(strServer, strDatabase);

			// Set the values in the database object
			m_ipFAMDB->DatabaseServer = strServer.c_str();
			m_ipFAMDB->DatabaseName = strDatabase.c_str();

			// Set the member values
			m_zServerName = strServer.c_str();
			m_zDBName = strDatabase.c_str();
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
		m_ipFAMDB->DatabaseServer = m_zServerName.operator LPCSTR();
		m_ipFAMDB->DatabaseName = m_zDBName.operator LPCSTR();

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
				// Reset the database connection to get the connection representing the correct 
				// Server and database
				try
				{
					m_ipFAMDB->ResetDBConnection();
				}
				CATCH_AND_LOG_ALL_EXCEPTIONS("ELI18162");

				// Attempt login
				bLoginValid = m_ipFAMDB->ShowLogin(VARIANT_TRUE, &bLoginCanceled);

				// If login is valid show Admin dialog
				if (asCppBool(bLoginValid))
				{
					// Hide this window
					ShowWindow(SW_HIDE);
					
					// Create admin dialog
					CFAMDBAdminDlg dlg(m_ipFAMDB);
					if ( dlg.DoModal() == IDCANCEL )
					{
						// Exit the FAMDBAdmin app
						__super::OnOK();
					}
					else
					{
						// Reset the Database option to login existing
						m_eOptionsDatabaseGroup = kLoginExisting;

						// Update the UI
						UpdateData(FALSE);

						// Show this window again
						ShowWindow(SW_SHOW);

						// Call this so that this dialog will become the active dialog.
						ActivateTopParent();
					}
				}
				else if (!asCppBool(bLoginCanceled))
				{
					MessageBox("Login failed.\r\n\r\nPlease ensure you are using the correct password and try again.", 
						"Login failed", MB_OK | MB_ICONERROR );
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
