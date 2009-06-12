// ConvertFAMDBDlg.cpp : implementation file
//

#include "stdafx.h"
#include "ConvertFAMDB.h"
#include "ConvertFAMDBDlg.h"
#include "ConvertSQL.h"

#include <UCLIDException.h>
#include <DialogSelect.h>
#include <ADOUtils.h>
#include <COMUtils.h>

#include <string>
#include <vector>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


static std::string gstrDEFAULT_DATABASE_SUFFIX = "_7_0";
const int gi5_0DB_SCHEMA_VERSION = 7;

//-------------------------------------------------------------------------------------------------
// CAboutDlg dialog used for App About
//-------------------------------------------------------------------------------------------------
class CAboutDlg : public CDialog
{
public:
	CAboutDlg();

// Dialog Data
	enum { IDD = IDD_ABOUTBOX };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

// Implementation
protected:
	DECLARE_MESSAGE_MAP()
};
//-------------------------------------------------------------------------------------------------
CAboutDlg::CAboutDlg() : CDialog(CAboutDlg::IDD)
{
}
//-------------------------------------------------------------------------------------------------
void CAboutDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}

//-------------------------------------------------------------------------------------------------
// CAboutDlg Message Handlers
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CAboutDlg, CDialog)
END_MESSAGE_MAP()


//-------------------------------------------------------------------------------------------------
// CConvertFAMDBDlg dialog
//-------------------------------------------------------------------------------------------------
CConvertFAMDBDlg::CConvertFAMDBDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CConvertFAMDBDlg::IDD, pParent)
	, m_zFromServer("")
	, m_zFromDB("")
	, m_zToServer("")
	, m_zToDB("")
	, m_cbFromServer(DBInfoCombo::kServerName)
	, m_cbFromDB(DBInfoCombo::kDatabaseName)
	, m_cbToServer(DBInfoCombo::kServerName)
	, m_cbToDB(DBInfoCombo::kDatabaseName)
	, m_iLastControlID(0)
{
	try
	{
		m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI18066");
}
//-------------------------------------------------------------------------------------------------
void CConvertFAMDBDlg::DoDataExchange(CDataExchange* pDX)
{
	try
	{
		CDialog::DoDataExchange(pDX);
		DDX_Text(pDX, IDC_COMBO_FROM_SERVER, m_zFromServer);
		DDX_Text(pDX, IDC_COMBO_FROM_DB, m_zFromDB);
		DDX_Text(pDX, IDC_COMBO_TO_SERVER, m_zToServer);
		DDX_Text(pDX, IDC_COMBO_TO_DB, m_zToDB);
		DDX_Control(pDX, IDC_COMBO_FROM_SERVER, m_cbFromServer);
		DDX_Control(pDX, IDC_COMBO_FROM_DB, m_cbFromDB);
		DDX_Control(pDX, IDC_COMBO_TO_SERVER, m_cbToServer);
		DDX_Control(pDX, IDC_COMBO_TO_DB, m_cbToDB);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18067");
}

//-------------------------------------------------------------------------------------------------
// CConvertFAMDBDlg Message Map
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CConvertFAMDBDlg, CDialog)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_CBN_SETFOCUS(IDC_COMBO_FROM_DB, &CConvertFAMDBDlg::OnSetfocusControl)
	ON_CBN_SETFOCUS(IDC_COMBO_FROM_SERVER, &CConvertFAMDBDlg::OnSetfocusControl)
	ON_CBN_SETFOCUS(IDC_COMBO_TO_DB, &CConvertFAMDBDlg::OnSetfocusControl)
	ON_CBN_SETFOCUS(IDC_COMBO_TO_SERVER, &CConvertFAMDBDlg::OnSetfocusControl)
	ON_BN_CLICKED(IDOK, &CConvertFAMDBDlg::OnBnClickedOk)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


//-------------------------------------------------------------------------------------------------
// CConvertFAMDBDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CConvertFAMDBDlg::OnInitDialog()
{
	try
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
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18068");

	return TRUE;  // return TRUE  unless you set the focus to a control
}
//-------------------------------------------------------------------------------------------------
void CConvertFAMDBDlg::OnSysCommand(UINT nID, LPARAM lParam)
{
	try
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18069");
}

// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.
//-------------------------------------------------------------------------------------------------
void CConvertFAMDBDlg::OnPaint()
{
	try
	{
		if (IsIconic())
		{
			CPaintDC dc(this); // device context for painting

			SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18070");
}
//-------------------------------------------------------------------------------------------------
// The system calls this function to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CConvertFAMDBDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}
//-------------------------------------------------------------------------------------------------
void CConvertFAMDBDlg::OnSetfocusControl()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Update the member variables
		UpdateData(TRUE);

		// Set the server for the FromDB
		m_cbFromDB.setSQLServer((LPCSTR)m_zFromServer);

		// If the ToServer is not specified default to the FromServer
		if (m_zToServer.IsEmpty())
		{
			// Set To server to the From server
			m_zToServer = m_zFromServer;

			// Set the SQL server for the ToDB combo box
			m_cbToDB.setSQLServer((LPCSTR)m_zToServer);

			// Update the UI
			UpdateData(FALSE);
		}

		// If the ToDB is not set and the FromDB is default to FromDB with default suffix
		if (m_zToDB.IsEmpty() && !m_zFromDB.IsEmpty())
		{
			// Set the ToDB to FromDB with suffix
			m_zToDB = m_zFromDB + gstrDEFAULT_DATABASE_SUFFIX.c_str();

			// Update the UI
			UpdateData(FALSE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18072");
}
//-------------------------------------------------------------------------------------------------
void CConvertFAMDBDlg::OnBnClickedOk()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Display the wait cursor
		CWaitCursor cw;

		// update the server and database info from the UI
		UpdateData(TRUE);
		
		// Check if input data is valid
		if (!isInputDataValid())
		{
			return;
		}

		// Display message that the process may take a while
		int iResult = AfxMessageBox(
			"The database conversion process may take anywhere from a few "
			"minutes to a few hours, depending upon the size of the database.\r\n\r\n"
			"As a datapoint, it's pretty normal for a database that references "
			"about 5 million files to take about 18 hours to convert.\r\n\r\n"
			"If you choose to continue with the conversion process, please "
			"be patient and do not terminate this conversion utility.\r\n\r\n"
			"Would you like to proceed with the database conversion at this time?", 
			MB_YESNO | MB_ICONQUESTION);

		// If the user answers yes to the question convert the database
		if ( iResult == IDYES )
		{
			// Convert the database
			convertDatabase();

			// Tell the user the database has been converted
			AfxMessageBox("The database has been successfully converted.", MB_OK | MB_ICONINFORMATION);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18073");
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CConvertFAMDBDlg::convertDatabase()
{
	INIT_EXCEPTION_AND_TRACING("MLI00026");

	try
	{
		// Create a FAMDB object for creating the new database and adding the actions
		IFileProcessingDBPtr ipFAMDB(CLSID_FileProcessingDB);
		ASSERT_RESOURCE_ALLOCATION("ELI19889", ipFAMDB != NULL);

		// Set the Database Server
		ipFAMDB->DatabaseServer = m_zToServer.operator LPCSTR();
		_lastCodePos = "10";

		// Create the new database
		ipFAMDB->CreateNewDB(m_zToDB.operator LPCSTR());
		_lastCodePos = "20";

		// Create the connection object for new database
		_ConnectionPtr ipNewDB = getConnection ( m_zToServer.operator LPCSTR(), m_zToDB.operator LPCSTR() );
		ASSERT_RESOURCE_ALLOCATION("ELI19961", ipNewDB != NULL );

		// Create the connection object for the old database
		_ConnectionPtr ipOldDB = getConnection ( m_zFromServer.operator LPCSTR(), m_zFromDB.operator LPCSTR() );
		ASSERT_RESOURCE_ALLOCATION("ELI20014", ipOldDB != NULL );

		// Add the actions from the old DB to the new DB using the FAMDB object
		addActionsToNewDB(ipFAMDB, ipOldDB);
		_lastCodePos = "30";

		// Done with the FAMDB object so set to NULL
		ipFAMDB = NULL;
		_lastCodePos = "40";

		// Copy the FAMFile records preserving the ID field since it is used in links to 
		// other tables
		copyRecords	(ipOldDB, ipNewDB, "FAMFile", "FAMFile", true);
		_lastCodePos = "50";

		// Copy the Login table without preserving the ID
		copyRecords(ipOldDB, ipNewDB, "Login", "Login");
		_lastCodePos = "50";

		// Copy the ActionStatistics table
		copyRecords(ipOldDB, ipNewDB, gstrSELECT_ACTIONSTATISTICS_FOR_TRANSFER_FROM_5_0, "ActionStatistics");
		_lastCodePos = "50";

		// Copy the FileActionStateTransition
		copyRecords(ipOldDB, ipNewDB, gstrSELECT_FAST_RECORDS_FOR_TRANSFER_FROM_5_0, "FileActionStateTransition");
		_lastCodePos = "50";

		// Copy the QueueEventRecords table 
		copyRecords(ipOldDB, ipNewDB, "QueueEvent", "QueueEvent");
		_lastCodePos = "50";
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20148");
}
//-------------------------------------------------------------------------------------------------
_ConnectionPtr CConvertFAMDBDlg::getConnection(const string& strServer, const string& strDatabase)
{
	INIT_EXCEPTION_AND_TRACING("MLI00027");

	try
	{
		ASSERT_ARGUMENT("ELI20032", !strServer.empty());
		ASSERT_ARGUMENT("ELI20033", !strDatabase.empty());

		// Create the connection object
		ADODB::_ConnectionPtr ipDBConnection(__uuidof( Connection ));
		ASSERT_RESOURCE_ALLOCATION("ELI20015", ipDBConnection != NULL);

		// create the connection string
		string strConnectionString = createConnectionString(strServer, strDatabase);
		_lastCodePos = "10";

		// Open the connection
		ipDBConnection->Open( strConnectionString.c_str(), "", "", adConnectUnspecified );
		_lastCodePos = "20";

		// Set the Command timeout to INFINITE
		ipDBConnection->CommandTimeout = 0;
		return ipDBConnection;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20149");

}
//-------------------------------------------------------------------------------------------------
void CConvertFAMDBDlg::addActionsToNewDB(IFileProcessingDBPtr ipFAMDB, _ConnectionPtr ipSourceDBConnection)
{
	INIT_EXCEPTION_AND_TRACING("MLI00028");

	try
	{
		ASSERT_ARGUMENT("ELI20034", ipFAMDB != NULL);
		ASSERT_ARGUMENT("ELI20035", ipSourceDBConnection != NULL);

		// Create the source action set
		_RecordsetPtr ipSourceActionSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI20013", ipSourceActionSet != NULL );

		// Open the Action set table in the source DB database
		ipSourceActionSet->Open( "Action", _variant_t((IDispatch *)ipSourceDBConnection, true), adOpenStatic, 
			adLockReadOnly, adCmdTable );
		_lastCodePos = "10";

		// While there are Actions in the Source table
		while (!ipSourceActionSet->adoEOF)
		{
			// Get the action name
			string strActionName = getStringField(ipSourceActionSet->Fields, "ASCName");
			_lastCodePos = "20";

			// Create the action in the new database
			ipFAMDB->DefineNewAction(strActionName.c_str());
			_lastCodePos = "30_" + strActionName;

			// Move to the next action
			ipSourceActionSet->MoveNext();
			_lastCodePos = "40";
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20150");
}
//-------------------------------------------------------------------------------------------------
void CConvertFAMDBDlg::addFKData(_ConnectionPtr ipDestDBConnection, FieldsPtr ipSourceFields, FieldsPtr ipDestFields) 
{
	INIT_EXCEPTION_AND_TRACING("MLI00029");

	try
	{
		ASSERT_ARGUMENT("ELI20036", ipDestDBConnection != NULL);
		ASSERT_ARGUMENT("ELI20037", ipSourceFields != NULL);
		ASSERT_ARGUMENT("ELI20147", ipDestFields != NULL);

		// Check for ASCName Field
		FieldPtr ipField = getNamedField(ipSourceFields, "ASCName");
		_lastCodePos = "10";

		// If the ASCName field was found set the ActionID in the Destination fields
		if ( ipField != NULL )
		{
			// Set the ActionID value
			copyIDValue(ipDestDBConnection, ipDestFields, "Action", "ASCName", 
				asString(ipField->Value.bstrVal), false);
		}
		_lastCodePos = "20";

		// Check for MachineName field
		ipField = getNamedField(ipSourceFields, "MachineName");
		_lastCodePos = "30";

		// If the MachineName field was found set the MachineID in the Destination fields
		if (ipField != NULL)
		{
			// Set the MachineID
			copyIDValue(ipDestDBConnection, ipDestFields, "Machine", "MachineName", 
				asString(ipField->Value.bstrVal), true);
		}
		_lastCodePos = "40";

		// Check for the UserName Field
		ipField = getNamedField(ipSourceFields, "UserName");
		_lastCodePos = "50";

		// If the UserName field was found set the ActionID in the Destination fields
		if (ipField != NULL)
		{
			// Set the FAMUserID
			copyIDValue(ipDestDBConnection, ipDestFields, "FAMUser", "UserName", 
				asString(ipField->Value.bstrVal), true);
		}
		_lastCodePos = "60";
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20151");
}
//-------------------------------------------------------------------------------------------------
void CConvertFAMDBDlg::copyRecords(_ConnectionPtr ipSourceDBConnection, _ConnectionPtr ipDestDBConnection, 
		const string& strSource, const string& strDest, bool bCopyID)
{
	INIT_EXCEPTION_AND_TRACING("MLI00030");

	try
	{
		ASSERT_ARGUMENT("ELI20020", ipSourceDBConnection != NULL);
		ASSERT_ARGUMENT("ELI20021", ipDestDBConnection != NULL);

		// Create the source ActionStatistics recordset
		_RecordsetPtr ipSourceSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI20022", ipSourceSet != NULL );

		// Source could be a Select statement
		string strSelect = strSource.substr(0, strlen("SELECT"));
		makeUpperCase(strSelect);
		_lastCodePos = "10";

		bool bSourceIsQuery = strSelect == "SELECT";

		// Open the Source set
		ipSourceSet->Open( strSource.c_str(), _variant_t((IDispatch *)ipSourceDBConnection, true), 
			adOpenStatic, adLockReadOnly, bSourceIsQuery ? adCmdText : adCmdTable );
		_lastCodePos = "20";

		// Create the destination ActionStatistics recordset
		_RecordsetPtr ipDestSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI20023", ipDestSet != NULL );

		// Turn on Identity insert so any identity fields will be copied
		executeCmdQuery(ipDestDBConnection, gstrSET_IDENTITY_INSERT_ON);
		_lastCodePos = "30";

		// Open the ActionStatistics set table in the database
		ipDestSet->Open( strDest.c_str(), _variant_t((IDispatch *)ipDestDBConnection, true), adOpenDynamic, 
			adLockOptimistic, adCmdTable );
		_lastCodePos = "40";

		// Counter to count the number of times through the loop
		long nCount = 0;

		// While the source table is not at EOF
		while (!ipSourceSet->adoEOF)
		{
			// Create a new recod
			ipDestSet->AddNew();
			_lastCodePos = "50-" + asString(nCount);;
	
			// copy fields that have the same name except the ID field
			copyExistingFields(ipSourceSet->Fields, ipDestSet->Fields, bCopyID);
			_lastCodePos = "60-" + asString(nCount);;

			// Copy any time fields found
			copyTimeFields(ipSourceSet->Fields, ipDestSet->Fields);
			_lastCodePos = "70-" + asString(nCount);;

			// Add the foreign key data
			addFKData(ipDestDBConnection, ipSourceSet->Fields, ipDestSet->Fields);
			_lastCodePos = "80-" + asString(nCount);;

			// Update the new record
			ipDestSet->Update();
			_lastCodePos = "90-" + asString(nCount);;

			// move to next source record
			ipSourceSet->MoveNext();	
			_lastCodePos = "100-" + asString(nCount);;
			
			nCount++;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20152");
}
//-------------------------------------------------------------------------------------------------
void CConvertFAMDBDlg::copyTimeFields(FieldsPtr ipSource, FieldsPtr ipDest)
{
	try
	{
		ASSERT_ARGUMENT("ELI20127", ipSource != NULL);
		ASSERT_ARGUMENT("ELI20128", ipDest != NULL);

		// Check for TS_Transition Field
		FieldPtr ipField = getNamedField(ipSource, "TS_Transition");

		if (ipField == NULL )
		{
			ipField = getNamedField(ipSource, "TimeStamp");
		}

		// if either TS_Transition or TimeStamp field was found transfer the value
		if ( ipField != NULL )
		{
			// Get the DestField
			FieldPtr ipDateTimeStamp = ipDest->Item["DateTimeStamp"];
			ASSERT_RESOURCE_ALLOCATION("ELI20056", ipDateTimeStamp != NULL);

			// Set the dest field value to the found source
			ipDateTimeStamp->Value = ipField->Value;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20153");
}
//-------------------------------------------------------------------------------------------------
bool CConvertFAMDBDlg::isInputDataValid()
{
	bool bDataValid = true;
	string strMsg;

	// Make sure all the values are not empty
	if ( m_zFromServer.IsEmpty() )
	{
		// Set focus to From Server control
		m_cbFromServer.SetFocus();
		strMsg = "The server to convert from cannot be empty.";
		bDataValid = false;
	}
	else if ( m_zFromDB.IsEmpty())
	{
		// Set focus to From Database control
		m_cbFromDB.SetFocus();
		strMsg = "The database to convert from cannot be empty.";
		bDataValid = false;
	}
	else if ( m_zToServer.IsEmpty())
	{
		// Set focus to To Database control
		m_cbToServer.SetFocus();
		strMsg = "The server to convert to cannot be empty.";
		bDataValid = false;
	}
	else if ( m_zToDB.IsEmpty())
	{
		// Set focus to To Database control
		m_cbToDB.SetFocus();
		strMsg = "The database to convert to cannot be empty.";
		bDataValid = false;
	}

	// The DataValid flag is false display the appropriate message
	if ( !bDataValid )
	{
		AfxMessageBox(strMsg.c_str(), MB_OK | MB_ICONEXCLAMATION);
		return false;
	}

	// Create a FAMDB object to check the schema version of the database to convert from
	IFileProcessingDBPtr ipFAMDB(CLSID_FileProcessingDB);
	ASSERT_RESOURCE_ALLOCATION("ELI20386", ipFAMDB != NULL);

	// Set to the from database
	ipFAMDB->DatabaseServer = (LPCSTR) m_zFromServer;
	ipFAMDB->DatabaseName = (LPCSTR) m_zFromDB;

	// Check the schema version
	if (ipFAMDB->DBSchemaVersion != gi5_0DB_SCHEMA_VERSION )
	{
		// Set focus to the FromDB control
		m_cbFromDB.SetFocus();
		AfxMessageBox(
			"The selected database to convert data from is not a 5.0 database.\r\n"
			"Please select a different database.", MB_OK | MB_ICONEXCLAMATION);
		return false;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
