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
#include <cpputil.h>

#include <string>
#include <vector>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


static std::string gstrDEFAULT_DATABASE_SUFFIX = "_9_0";
const int gi5_0DB_SCHEMA_VERSION = 7;
const int gi7_0DB_SCHEMA_VERSION = 8;

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
		DDX_Control(pDX, IDC_CHECK_RETAIN_HISTORY_DATA, m_checkRetainHistoryData);
		DDX_Control(pDX, IDC_STATIC_CURRENT_STEP, m_staticCurrentStep);
		DDX_Control(pDX, IDC_STATIC_CURRENT_RECORD, m_staticCurrentRecord);
		DDX_Control(pDX, IDC_PROGRESS_CURRENT, m_progressCurrent);
		DDX_Control(pDX, IDOK, m_buttonStart);
		DDX_Control(pDX, IDCANCEL, m_buttonClose);
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
		if (pSysMenu != __nullptr)
		{
			CString strAboutMenu;
			strAboutMenu.LoadString(IDS_ABOUTBOX);
			if (!strAboutMenu.IsEmpty())
			{
				pSysMenu->AppendMenu(MF_SEPARATOR);
				pSysMenu->AppendMenu(MF_STRING, IDM_ABOUTBOX, strAboutMenu);
			}
		}

		// check the retain history check
		m_checkRetainHistoryData.SetCheck(BST_CHECKED);
		
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
		// if the conversion process is running display message and return (don't process the close)
		if (m_eventConvertStarted.isSignaled() && !m_eventConvertComplete.isSignaled() && nID == SC_CLOSE)
		{
			AfxMessageBox("The database is still in the process of being converted. Please wait.");
			return;
		}

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
			// Reset the Convert Events
			m_eventConvertStarted.reset();
			m_eventConvertComplete.reset();

			// Start the conversion thread
			AfxBeginThread(convertDatabaseInThread, this);
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
		// Disable the controls so they cannot be modified until after the conversion is done
		enableControls(false);

		// Signal that the conversion has started
		m_eventConvertStarted.signal();

		// Log exception to indicate that a database is being converted
		UCLIDException ue("ELI28525", "Application Trace: Converting DB to 9.0");
		ue.addDebugInfo("From Server", (LPCSTR) m_zFromServer);
		ue.addDebugInfo("From DB", (LPCSTR) m_zFromDB);
		ue.addDebugInfo("To Server", (LPCSTR) m_zToServer);
		ue.addDebugInfo("To DB", (LPCSTR) m_zToDB);
		ue.log();

		m_staticCurrentStep.SetWindowText("Creating database");
		
		// Create a FAMDB object for creating the new database and adding the actions
		IFileProcessingDBPtr ipFAMDB(CLSID_FileProcessingDB);
		ASSERT_RESOURCE_ALLOCATION("ELI19889", ipFAMDB != __nullptr);

		// Set the Database Server
		ipFAMDB->DatabaseServer = (LPCSTR) m_zToServer;
		_lastCodePos = "10";

		// Create the new database
		ipFAMDB->CreateNew80DB((LPCSTR) m_zToDB);
		_lastCodePos = "20";

		// Create the connection object for new database
		_ConnectionPtr ipNewDB = getConnection ( (LPCSTR) m_zToServer, (LPCSTR) m_zToDB);
		ASSERT_RESOURCE_ALLOCATION("ELI19961", ipNewDB != __nullptr );
		_lastCodePos = "30";

		// Create the connection object for the old database
		_ConnectionPtr ipOldDB = getConnection ( (LPCSTR) m_zFromServer, (LPCSTR) m_zFromDB);
		ASSERT_RESOURCE_ALLOCATION("ELI20014", ipOldDB != __nullptr );
		_lastCodePos = "40";

		// Need to determine the number of steps for the progress
		bool bConvertIDShieldTable = doesTableExist(ipOldDB, "IDShieldData") && doesTableExist(ipNewDB, "IDShieldData");
		bool bRetainHistory = m_checkRetainHistoryData.GetCheck() == BST_CHECKED;

		// There are 8 steps + 2 for converting the IDShield table if the table exists + 
		// 2 for retaining history (QueueEvent and FAST tables)
		long nNumberOfSteps = 8 + (bConvertIDShieldTable ? 2 : 0) + (bRetainHistory ? 2 : 0); 
		// Add 5 more steps to account for converting from 8.0 to 9.0. We won't be able to show
		// progress through these steps, but it will give a better indication of time remaining than
		// if this phase had only 1 assigned step.
		nNumberOfSteps += 5;
		long nCurrentStep = 2;

		updateCurrentStep("Creating actions ", nCurrentStep, nNumberOfSteps);
		
		// Copy the actions from the old DB to the new DB
		addActionsToNewDB80(ipOldDB, ipNewDB);
		_lastCodePos = "50";

		updateCurrentStep("Updating DBInfo settings", nCurrentStep, nNumberOfSteps);

		// Copy existing settings
		copyDBInfoSettings(ipFAMDB, ipOldDB);
		_lastCodePos = "55";

		// Done with the FAMDB object so set to NULL
		ipFAMDB = __nullptr;

		updateCurrentStep("Copying FAMUser table", nCurrentStep, nNumberOfSteps);

		// Copy the FAMUser table preserving the ID
		copyRecords(ipOldDB, ipNewDB, "FAMUser", "FAMUser", true);
		_lastCodePos = "60";

		updateCurrentStep("Copying Machine table", nCurrentStep, nNumberOfSteps);

		// Copy the Machine table preserving the ID
		copyRecords(ipOldDB, ipNewDB, "Machine", "Machine", true);
		_lastCodePos = "70";

		updateCurrentStep("Copying FAMFile table", nCurrentStep, nNumberOfSteps);
		
		// Copy the FAMFile records preserving the ID field since it is used in links to 
		// other tables
		copyRecords	(ipOldDB, ipNewDB, "FAMFile", "FAMFile", true);
		_lastCodePos = "80";

		updateCurrentStep("Copying Login table", nCurrentStep, nNumberOfSteps);
		
		// Copy the Login table without preserving the ID
		copyRecords(ipOldDB, ipNewDB, "Login", "Login");
		_lastCodePos = "90";
		
		updateCurrentStep("Copying ActionStatistics table", nCurrentStep, nNumberOfSteps);
				
		// Copy the ActionStatistics table
		copyRecords(ipOldDB, ipNewDB, gstrSELECT_ACTIONSTATISTICS_FOR_TRANSFER_FROM_7_0, "ActionStatistics");
		_lastCodePos = "100";

		// If there is an IDShieldData table copy it.
		if (bConvertIDShieldTable)
		{
			updateCurrentStep("Copying IDShieldData table", nCurrentStep, nNumberOfSteps);
					
			// Need to check first if there is a IDShieldData file
			copyRecords(ipOldDB, ipNewDB, gstrSELECT_IDSHIELD_DATA_FOR_TRANSFER_FROM_7_0, "IDShieldData");
			_lastCodePos = "130";

			updateCurrentStep("Updating IDShieldData table", nCurrentStep, nNumberOfSteps);
			m_staticCurrentRecord.SetWindowText("Please wait...");

			// Fix up the duration totals
			executeCmdQuery(ipNewDB, gstrUPDATE_IDSHIELD_DATA_DURATION_FOR_8_0);
		}
	
		// Only copy the FAST and QueueEvent records if retaining history data
		if (bRetainHistory)
		{
			updateCurrentStep("Copying FileActionStateTransition table", nCurrentStep, nNumberOfSteps);
					
			// Copy the FileActionStateTransition
			copyRecords(ipOldDB, ipNewDB, gstrSELECT_FAST_RECORDS_FOR_TRANSFER_FROM_7_0, "FileActionStateTransition");
			_lastCodePos = "110";

			updateCurrentStep("Copying QueueEvent table", nCurrentStep, nNumberOfSteps);
			
			// Copy the QueueEventRecords table 
			copyRecords(ipOldDB, ipNewDB, "QueueEvent", "QueueEvent");
			_lastCodePos = "120";
		}

		updateCurrentStep("Finalizing conversion...", nCurrentStep, nNumberOfSteps);

		// Create a new FAMDB object for converting from 8.0 to 9.0
		ipFAMDB.CreateInstance(CLSID_FileProcessingDB);
		ASSERT_RESOURCE_ALLOCATION("ELI34258", ipFAMDB != __nullptr);

		IProgressStatusPtr ipProgressStatus(CLSID_ProgressStatus);
		ASSERT_RESOURCE_ALLOCATION("ELI34259", ipProgressStatus != __nullptr);

		ipProgressStatus->InitProgressStatus("", 0, 100, VARIANT_FALSE);

		ipFAMDB->DatabaseServer = (LPCSTR) m_zToServer;
		ipFAMDB->DatabaseName = (LPCSTR) m_zToDB;
		ipFAMDB->UpgradeToCurrentSchema(ipProgressStatus);

		_lastCodePos = "130";

		m_staticCurrentStep.SetWindowText("");

		// Log exception to indicate that a database convertion is complete
		UCLIDException ueCompleted("ELI28526", "Application Trace: DB has been converted to 9.0");
		ueCompleted.addDebugInfo("From Server", (LPCSTR) m_zFromServer);
		ueCompleted.addDebugInfo("From DB", (LPCSTR) m_zFromDB);
		ueCompleted.addDebugInfo("To Server", (LPCSTR) m_zToServer);
		ueCompleted.addDebugInfo("To DB", (LPCSTR) m_zToDB);
		ueCompleted.log();

		// Tell the user the database has been converted
		AfxMessageBox("The database has been successfully converted.", MB_OK | MB_ICONINFORMATION);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20148");

	try
	{
		// Enable the controls
		enableControls(true);

		// Signal that the conversion is complete
		m_eventConvertComplete.signal();

		// Clear the text in the status controls
		m_staticCurrentRecord.SetWindowText("");
		m_staticCurrentStep.SetWindowText("");
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI28620");
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
		ASSERT_RESOURCE_ALLOCATION("ELI20015", ipDBConnection != __nullptr);

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
void CConvertFAMDBDlg::addActionsToNewDB80(_ConnectionPtr ipSourceDBConnection,
										 _ConnectionPtr ipDestDBConnection)
{
	INIT_EXCEPTION_AND_TRACING("MLI00028");

	try
	{
		ASSERT_ARGUMENT("ELI34244", ipSourceDBConnection != __nullptr);
		ASSERT_ARGUMENT("ELI34245", ipDestDBConnection != __nullptr);

		// Create the source action set
		_RecordsetPtr ipSourceActionSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI20013", ipSourceActionSet != __nullptr );

		// Open the Action set table in the source DB database
		ipSourceActionSet->Open( "Action", _variant_t((IDispatch *)ipSourceDBConnection, true), adOpenStatic, 
			adLockReadOnly, adCmdTable );
		_lastCodePos = "10";

		long nRecordCount = ipSourceActionSet->GetRecordCount();
		string strRcdCount = asString(nRecordCount);
		long nCurrentRecord = 1;
		_lastCodePos = "20";

		// Setup the progress bar
		m_progressCurrent.SetRange32(1, nRecordCount + 1);
		_lastCodePos = "30";

		// While there are Actions in the Source table
		while (ipSourceActionSet->adoEOF == VARIANT_FALSE)
		{
			// Get the action name
			string strActionName = getStringField(ipSourceActionSet->Fields, "ASCName");
			_lastCodePos = "40_" + strActionName;

			string strRcdString = "Creating action " + asString(nCurrentRecord) + " of " + strRcdCount;
			m_staticCurrentRecord.SetWindowText(strRcdString.c_str());
			_lastCodePos = "50_" + strActionName;

			// Set progress position
			m_progressCurrent.SetPos(nCurrentRecord);
			_lastCodePos = "60_" + strActionName;

			// Create the action in the new database
			defineNewAction80(ipDestDBConnection, strActionName);
			_lastCodePos = "70_" + strActionName;

			// Move to the next action
			ipSourceActionSet->MoveNext();
			_lastCodePos = "80_" + strActionName;
			
			nCurrentRecord++;
		}
		_lastCodePos = "90";
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20150");
}
//-------------------------------------------------------------------------------------------------
void CConvertFAMDBDlg::addFKData(_ConnectionPtr ipDestDBConnection, FieldsPtr ipSourceFields, FieldsPtr ipDestFields) 
{
	INIT_EXCEPTION_AND_TRACING("MLI00029");

	try
	{
		ASSERT_ARGUMENT("ELI20036", ipDestDBConnection != __nullptr);
		ASSERT_ARGUMENT("ELI20037", ipSourceFields != __nullptr);
		ASSERT_ARGUMENT("ELI20147", ipDestFields != __nullptr);

		// Check for ASCName Field
		FieldPtr ipField = getNamedField(ipSourceFields, "ASCName");
		_lastCodePos = "10";

		// If the ASCName field was found set the ActionID in the Destination fields
		if ( ipField != __nullptr )
		{
			// Set the ActionID value
			copyIDValue(ipDestDBConnection, ipDestFields, "Action", "ASCName", 
				asString(ipField->Value.bstrVal), false);
		}
		_lastCodePos = "20";

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
		m_staticCurrentRecord.SetWindowText("Calculating...");
		_lastCodePos = "10";

		ASSERT_ARGUMENT("ELI20020", ipSourceDBConnection != __nullptr);
		ASSERT_ARGUMENT("ELI20021", ipDestDBConnection != __nullptr);

		// Create the source ActionStatistics recordset
		_RecordsetPtr ipSourceSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI20022", ipSourceSet != __nullptr );
		_lastCodePos = "20";

		// Source could be a Select statement
		string strSelect = strSource.substr(0, strlen("SELECT"));
		makeUpperCase(strSelect);
		_lastCodePos = "30";

		bool bSourceIsQuery = strSelect == "SELECT";

		// Open the Source set
		ipSourceSet->Open( strSource.c_str(), _variant_t((IDispatch *)ipSourceDBConnection, true), 
			adOpenStatic, adLockReadOnly, bSourceIsQuery ? adCmdText : adCmdTable );
		_lastCodePos = "40";

		// Create the destination ActionStatistics recordset
		_RecordsetPtr ipDestSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI20023", ipDestSet != __nullptr );
		
		// Turn IDENTITY_INSERT option on if required
		if (bCopyID && !bSourceIsQuery)
		{
			// Turn on IDENTITY_INSERT to allow copying of ID
			identityInsert(ipDestDBConnection, strSource, true); 
			_lastCodePos = "50";
		}

		// Open the ActionStatistics set table in the database
		ipDestSet->Open( strDest.c_str(), _variant_t((IDispatch *)ipDestDBConnection, true), adOpenDynamic, 
			adLockOptimistic, adCmdTable );
		_lastCodePos = "60";

		long nRecordCount = ipSourceSet->GetRecordCount();
		string strRcdCount = asString(nRecordCount);
		long nCurrentRecord = 1;
		_lastCodePos = "70";
		
		// Setup the progress bar
		m_progressCurrent.SetRange32(1, nRecordCount + 1);
		_lastCodePos = "80";

		// While the source table is not at EOF
		while (ipSourceSet->adoEOF == VARIANT_FALSE)
		{
			string strCurrRec = asString(nCurrentRecord);

			string strRcdString = "Copying record " + strCurrRec + " of " + strRcdCount;
			m_staticCurrentRecord.SetWindowText(strRcdString.c_str());
			_lastCodePos = "90-" + strCurrRec;

			m_progressCurrent.SetPos(nCurrentRecord);
			_lastCodePos = "100" + strCurrRec;

			// Create a new recod
			ipDestSet->AddNew();
			_lastCodePos = "110" + strCurrRec;
	
			// copy fields that have the same name except the ID field
			copyExistingFields(ipSourceSet->Fields, ipDestSet->Fields, bCopyID);
			_lastCodePos = "120" + strCurrRec;

			// Add the foreign key data
			addFKData(ipDestDBConnection, ipSourceSet->Fields, ipDestSet->Fields);
			_lastCodePos = "130-" + strCurrRec;

			// Update the new record
			ipDestSet->Update();
			_lastCodePos = "140-" + strCurrRec;

			// move to next source record
			ipSourceSet->MoveNext();	
			_lastCodePos = "150-" + strCurrRec;
			
			nCurrentRecord++;
		}
		_lastCodePos = "160";

		// Turn IDENTITY_INSERT option off as required
		if (bCopyID && !bSourceIsQuery)
		{
			// Turn off Identity insert so any identity fields will be copied
			identityInsert(ipDestDBConnection, strSource, false);
			_lastCodePos = "170";
		}
		m_staticCurrentRecord.SetWindowText("");
		_lastCodePos = "180";

		m_progressCurrent.SetPos(0);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20152");
}
//-------------------------------------------------------------------------------------------------
void CConvertFAMDBDlg::copyDBInfoSettings(IFileProcessingDBPtr ipFAMDB, _ConnectionPtr ipSourceDBConnection)
{
	try
	{
		ASSERT_ARGUMENT("ELI28607", ipFAMDB != __nullptr);
		ASSERT_ARGUMENT("ELI28608", ipSourceDBConnection);

		// Create the source DBInfo set
		_RecordsetPtr ipSourceDBInfoSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI28609", ipSourceDBInfoSet != __nullptr );

		// Open the DBInfo set table in the source DB database
		ipSourceDBInfoSet->Open( "DBInfo", _variant_t((IDispatch *)ipSourceDBConnection, true), adOpenStatic, 
			adLockReadOnly, adCmdTable );

		// While there are records in the Source table
		while (ipSourceDBInfoSet->adoEOF == VARIANT_FALSE)
		{
			// Get the setting name
			string strSetting = getStringField(ipSourceDBInfoSet->Fields, "Name");
			
			// Check if this is a version setting and if it is do not transfer the setting
			if (strSetting.find("Version") == string::npos)
			{
				// Get the value for the setting
				string strValue = getStringField(ipSourceDBInfoSet->Fields, "Value");

				// Save the setting in the new database
				ipFAMDB->SetDBInfoSetting(
					strSetting.c_str(), strValue.c_str(), VARIANT_TRUE, VARIANT_FALSE);
			}

			// Move to the next record
			ipSourceDBInfoSet->MoveNext();
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28606");
}
//--------------------------------------------------------------------------------------------------
long CConvertFAMDBDlg::defineNewAction80(_ConnectionPtr ipConnection, const string& strActionName)
{
	try
	{
		// Create a pointer to a recordset containing the action
		_RecordsetPtr ipActionSet = getActionSet80(ipConnection, strActionName);
		ASSERT_RESOURCE_ALLOCATION("ELI34246", ipActionSet != NULL);

		// Check to see if action exists
		if (ipActionSet->adoEOF == VARIANT_FALSE)
		{
			// Build error string (P13 #3931)
			CString zText;
			zText.Format("The action '%s' already exists, and therefore cannot be added again.", 
				strActionName.c_str());
			UCLIDException ue("ELI34247", LPCTSTR(zText));
			throw ue;
		}

		// Create a new action and return its ID
		return addActionToRecordset80(ipConnection, ipActionSet, strActionName);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI34248");
}
//--------------------------------------------------------------------------------------------------
_RecordsetPtr CConvertFAMDBDlg::getActionSet80(_ConnectionPtr ipConnection, const string &strAction)
{
	// Create a pointer to a recordset
	_RecordsetPtr ipActionSet(__uuidof(Recordset));
	ASSERT_RESOURCE_ALLOCATION("ELI34249", ipActionSet != NULL);

	// Setup select statement to open Action Table
	string strActionSelect = "SELECT ID, ASCName FROM Action WHERE ASCName = '" + strAction + "'";

	// Open the Action table in the database
	ipActionSet->Open(strActionSelect.c_str(), _variant_t((IDispatch *)ipConnection, true), 
		adOpenDynamic, adLockOptimistic, adCmdText);

	return ipActionSet;
}
//--------------------------------------------------------------------------------------------------
long CConvertFAMDBDlg::addActionToRecordset80(_ConnectionPtr ipConnection, 
											 _RecordsetPtr ipRecordset, const string &strAction)
{
	try
	{
		// Add a new record
		ipRecordset->AddNew();

		// Set the values of the ASCName field
		setStringField(ipRecordset->Fields, "ASCName", strAction);

		// Add the record to the Action Table
		ipRecordset->Update();

		// Get the ID of the new Action
		long lActionId = getLastTableID80(ipConnection, "Action");

		// Add column to the FAMFile table
		addActionColumn80(ipConnection, strAction);

		return lActionId;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI34250")
}
//-------------------------------------------------------------------------------------------------
long CConvertFAMDBDlg::getLastTableID80(const _ConnectionPtr& ipDBConnection, string strTableName)
{
	ASSERT_ARGUMENT("ELI34251", ipDBConnection != NULL);

	_RecordsetPtr ipRSet;
	// Build SQL string to get the last ID for the given table
	string strGetIDSQL = "SELECT IDENT_CURRENT ('" + strTableName + "') AS CurrentID";

	// Execute the command 
	ipRSet = ipDBConnection->Execute( strGetIDSQL.c_str(), NULL, adCmdUnknown );
	ASSERT_RESOURCE_ALLOCATION("ELI34252", ipRSet != NULL );

	// The ID field in all of the tables is a 32 bit int so
	// the value returned by this function can be changed to type long
	return (long) getLongLongField( ipRSet->Fields, "CurrentID" );
}
//--------------------------------------------------------------------------------------------------
void CConvertFAMDBDlg::addActionColumn80(const _ConnectionPtr& ipConnection, const string& strAction)
{

	// Add new Column to FAMFile table
	// Create SQL statement to add the column to FAMFile
	string strAddColSQL = "Alter Table FAMFile Add ASC_" + strAction + " nvarchar(1)";

	// Run the SQL to add column to FAMFile
	executeCmdQuery(ipConnection, strAddColSQL);

	// Create the query and update the file status for all files to unattempted
	string strUpdateSQL = "UPDATE FAMFile SET ASC_" + strAction + " = 'U' FROM FAMFile";
	executeCmdQuery(ipConnection, strUpdateSQL);

	// Create index on the new column
	string strCreateIDX = "Create Index IX_ASC_" + strAction + " on FAMFile (ASC_" 
		+ strAction + ")";
	executeCmdQuery(ipConnection, strCreateIDX);

	// Add foreign key contraint for the new column to reference the ActionState table
	string strAddContraint = "ALTER TABLE FAMFile WITH CHECK ADD CONSTRAINT FK_ASC_" 
		+ strAction + " FOREIGN KEY(ASC_" + 
		strAction + ") REFERENCES ActionState(Code)";

	// Create the foreign key
	executeCmdQuery(ipConnection, strAddContraint);

	// Add the default contraint for the column
	string strDefault = "ALTER TABLE FAMFile ADD CONSTRAINT DF_ASC_" 
		+ strAction + " DEFAULT 'U' FOR ASC_" + strAction;
	executeCmdQuery(ipConnection, strDefault);
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
	ASSERT_RESOURCE_ALLOCATION("ELI20386", ipFAMDB != __nullptr);

	// Set to the from database
	ipFAMDB->DatabaseServer = (LPCSTR) m_zFromServer;
	ipFAMDB->DatabaseName = (LPCSTR) m_zFromDB;

	// Check the schema version
	if (ipFAMDB->DBSchemaVersion != gi7_0DB_SCHEMA_VERSION )
	{
		// Set focus to the FromDB control
		m_cbFromDB.SetFocus();
		AfxMessageBox(
			"The selected database to convert data from is not a 6.0 or 7.0 database.\r\n"
			"Please select a different database.", MB_OK | MB_ICONEXCLAMATION);
		return false;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
void CConvertFAMDBDlg::identityInsert(_ConnectionPtr ipDestDBConnection, const string& strTable, bool bState)
{
	string strIdentityInsertSetting = bState ? gstrSET_IDENTITY_INSERT_ON : 
		gstrSET_IDENTITY_INSERT_OFF;

	replaceVariable(strIdentityInsertSetting, "<TableName>", strTable);

	// Turn on Identity insert so any identity fields will be copied
	executeCmdQuery(ipDestDBConnection, strIdentityInsertSetting);
}
//-------------------------------------------------------------------------------------------------
void CConvertFAMDBDlg::updateCurrentStep(string strStepDescription, long &rnStepNumber, long nTotalSteps)
{
	strStepDescription += " (Step " + asString(rnStepNumber) + " of " + asString(nTotalSteps) + ")";
	m_staticCurrentStep.SetWindowText(strStepDescription.c_str());
	rnStepNumber++;
}
//-------------------------------------------------------------------------------------------------
UINT CConvertFAMDBDlg::convertDatabaseInThread(void *pData)
{
	CConvertFAMDBDlg * pDlg = static_cast<CConvertFAMDBDlg *>(pData);
	CoInitializeEx(NULL, COINIT_MULTITHREADED);
	try
	{
		ASSERT_ARGUMENT("ELI28617" ,pDlg != __nullptr);

		// Convert the database
		pDlg->convertDatabase();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI28616");

	CoUninitialize();

	return 0;
}
//-------------------------------------------------------------------------------------------------
void CConvertFAMDBDlg::enableControls(bool bEnable)
{
	m_cbFromServer.EnableWindow(asMFCBool(bEnable));
	m_cbFromDB.EnableWindow(asMFCBool(bEnable));
	m_cbToServer.EnableWindow(asMFCBool(bEnable));
	m_cbToDB.EnableWindow(asMFCBool(bEnable));
	m_checkRetainHistoryData.EnableWindow(asMFCBool(bEnable));
	m_buttonStart.EnableWindow(asMFCBool(bEnable));
	m_buttonClose.EnableWindow(asMFCBool(bEnable));
}
//-------------------------------------------------------------------------------------------------
