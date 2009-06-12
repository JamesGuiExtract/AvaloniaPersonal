// USBLicenseKeyManagerDlg.cpp : implementation file
//

#include "stdafx.h"
#include "USBLicenseKeyManager.h"
#include "USBLicenseKeyManagerDlg.h"
#include "SafeNetLicenseMgr.h"

#include <LicenseMgmt.h>
#include <UCLIDException.h>
#include <RegConstants.h>
#include <cpputil.h>
#include <ShellExecuteThread.h>
#include <ComponentLicenseIDs.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//--------------------------------------------------------------------------------------------------
// Globals
//--------------------------------------------------------------------------------------------------
const int giCOUNTER_NAME_COLUMN = 1;
const int giCOUNTER_VALUE = 2;
const int giALERT_LEVEL = 3;
const int giALERT_MULTIPLE = 4;

const WORD gwINDEXING_COUNTER = 1;
const WORD gwPAGINATION_COUNTER = 2;
const WORD gwREDACTION_COUNTER = 3;

const double gdWAIT_SECONDS_RESET = 30;

//--------------------------------------------------------------------------------------------------
// CUSBLicenseKeyManagerDlg dialog
//--------------------------------------------------------------------------------------------------
CUSBLicenseKeyManagerDlg::CUSBLicenseKeyManagerDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CUSBLicenseKeyManagerDlg::IDD, pParent),
	m_bIsKeyServerValid(false),
	m_ipEmailSettings(CLSID_EmailSettings)
{
	try
	{
	ASSERT_RESOURCE_ALLOCATION("ELI12318", m_ipEmailSettings != NULL );
	
	IObjectSettingsPtr ipSettings = m_ipEmailSettings;
	ASSERT_RESOURCE_ALLOCATION("ELI12324", ipSettings != NULL );
	
	ipSettings->LoadFromRegistry( gstrEMAIL_REG_PATH.c_str() );

	//{{AFX_DATA_INIT(CUSBLicenseKeyManagerDlg)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT
	// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
	m_hIcon = AfxGetApp()->LoadIcon(IDR_USBKEY_ICON);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12327");
	
}
//--------------------------------------------------------------------------------------------------
void CUSBLicenseKeyManagerDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CUSBLicenseKeyManagerDlg)
	DDX_Control(pDX, IDC_ALERT_EXTRACT, m_checkAlertExtract);
	DDX_Control(pDX, IDC_TO_LIST, m_editToList);
	DDX_Control(pDX, IDC_EMAIL_ALERT, m_checkEmailAlert);
	DDX_Control(pDX, IDC_COUNTER_SN, m_editCounterSerial);
	DDX_Control(pDX, IDC_REMOTE_MACHINE_NAME, m_editRemoteMachineName);
	DDX_Control(pDX, IDC_REMOTE_MACHINE, m_btnRemoteMachine);
	DDX_Control(pDX, IDC_LOCAL_MACHINE, m_btnLocalMachine);
	DDX_Control(pDX, IDC_HARD_LIMIT, m_editHardLimit);
	DDX_Control(pDX, IDC_SOFT_LIMIT, m_editSoftLimit);
	//}}AFX_DATA_MAP
}
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CUSBLicenseKeyManagerDlg, CDialog)
	//{{AFX_MSG_MAP(CUSBLicenseKeyManagerDlg)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_BN_CLICKED(IDC_REFRESH, OnRefresh)
	ON_BN_CLICKED(IDC_RESET_LOCK, OnResetLock)
	ON_BN_CLICKED(IDC_LOCAL_MACHINE, OnMachineChanged)
	ON_BN_CLICKED(IDC_APPLY, OnApply)
	ON_BN_CLICKED(IDC_SERVER_STATUS, OnServerStatus)
	ON_BN_CLICKED(IDC_REMOTE_MACHINE, OnMachineChanged)
	ON_BN_CLICKED(IDC_EMAILSETTINGS, OnEmailsettings)
	ON_BN_CLICKED(IDC_EMAIL_ALERT, OnEmailAlert)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
//--------------------------------------------------------------------------------------------------
// CUSBLicenseKeyManagerDlg message handlers
//--------------------------------------------------------------------------------------------------
BOOL CUSBLicenseKeyManagerDlg::OnInitDialog()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	
	try
	{
		CDialog::OnInitDialog();
		m_listCounterValues.SubclassDlgItem(IDC_COUNTER_VALUES, this);
		
		m_listCounterValues.Initialize();

		// Set the icon for this dialog.  The framework does this automatically
		//  when the application's main window is not a dialog
		SetIcon(m_hIcon, TRUE);			// Set big icon
		SetIcon(m_hIcon, FALSE);		// Set small icon
		
		// Set the option for the license server
		string strContactServerName = m_snlcSafeNetCfg.getContactServerName();
		makeUpperCase(strContactServerName);
		string strComputerName = getComputerName();
		makeUpperCase(strComputerName);
		if ( strContactServerName == strComputerName)
		{
			m_btnRemoteMachine.SetCheck(BST_UNCHECKED);
			m_btnLocalMachine.SetCheck(BST_CHECKED);
			m_editRemoteMachineName.EnableWindow(FALSE);
		}
		else 
		{
			m_btnRemoteMachine.SetCheck(BST_CHECKED);
			m_btnLocalMachine.SetCheck(BST_UNCHECKED);
			m_editRemoteMachineName.EnableWindow(TRUE);
			m_editRemoteMachineName.SetWindowText( strContactServerName.c_str());
		}		
		if ( m_snlcSafeNetCfg.getSendAlert() )
		{
			m_checkEmailAlert.SetCheck( BST_CHECKED );
		}
		else
		{
			m_checkEmailAlert.SetCheck( BST_UNCHECKED );
		}
		if ( m_snlcSafeNetCfg.getSendToExtract() )
		{
			m_checkAlertExtract.SetCheck( BST_CHECKED );
		}
		else
		{
			m_checkAlertExtract.SetCheck( BST_UNCHECKED );
		}
		m_editToList.SetWindowText(m_snlcSafeNetCfg.getAlertToList().c_str());
		updateControls();	
		prepareCounterList();

		LicenseManagement::sGetInstance().loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);

		ShowWindow(SW_SHOW);
		try
		{
			// Get license manager and obtain license with no retry
			SafeNetLicenseMgr snlmLicense( gusblFlexIndex, true, false );
			loadCounterValuesList(snlmLicense);
			m_bIsKeyServerValid = true;
		}
		catch ( ... )
		{
			m_bIsKeyServerValid = false;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11535");

	return TRUE;  // return TRUE  unless you set the focus to a control
}
//--------------------------------------------------------------------------------------------------
// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.

void CUSBLicenseKeyManagerDlg::OnPaint() 
{
	try
	{
		if (IsIconic())
		{
			CPaintDC dc(this); // device context for painting
			
			SendMessage(WM_ICONERASEBKGND, (WPARAM) dc.GetSafeHdc(), 0);
			
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11536");

}

//--------------------------------------------------------------------------------------------------
// The system calls this to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CUSBLicenseKeyManagerDlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}
//--------------------------------------------------------------------------------------------------
void CUSBLicenseKeyManagerDlg::OnRefresh() 
{
	try
	{
		CWaitCursor cwCursor;
		OnApply();
		
		// Clear previous values
		clearCounterValuesList();
		
		// This operation rereads the counter values from the key but will first check for a pending
		// counter increment
		
		// Get license manager and obtain license with no retry
		SafeNetLicenseMgr snlmLicense( gusblFlexIndex, true, false );
		
		// Check for a counter update
		SP_DWORD dwCounterToUpdate;
		
		// NOTE: Not sure if this is the best way to do.  
		//		if the value is read before the counter is completely updated
		//		the increment for the counter could be lost
		dwCounterToUpdate = snlmLicense.setCellValue(gdcellCounterToIncrement, 0);
		if ( dwCounterToUpdate != 0 )
		{
			SP_DWORD dwAmount = snlmLicense.setCellValue(gdcellCounterIncrementAmount, 0 );
			switch ( dwCounterToUpdate )
			{
			case gwINDEXING_COUNTER:
				snlmLicense.increaseCellValue ( gdcellFlexIndexingCounter, dwAmount );
				break;
			case gwPAGINATION_COUNTER:
				snlmLicense.increaseCellValue ( gdcellFlexPaginationCounter, dwAmount );
				break;
			case gwREDACTION_COUNTER:
				snlmLicense.increaseCellValue ( gdcellIDShieldRedactionCounter, dwAmount );
				break;
			default:
				THROW_LOGIC_ERROR_EXCEPTION("ELI11326");
			}
		}
		loadCounterValuesList(snlmLicense);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11537");
}
//--------------------------------------------------------------------------------------------------
void CUSBLicenseKeyManagerDlg::OnResetLock() 
{
	try
	{
		// Get license manager and obtain license with no retry
		SafeNetLicenseMgr snlmLicense( gusblFlexIndex, true, false );
		
		// Reset the lock
		snlmLicense.resetLock( gdcellFlexIndexingCounter, 30);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11538");
}
//--------------------------------------------------------------------------------------------------
void CUSBLicenseKeyManagerDlg::OnOK() 
{
	try
	{
		CWaitCursor cwCursor;
		applyNewValues();
		
		CDialog::OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11539");
}
//--------------------------------------------------------------------------------------------------
void CUSBLicenseKeyManagerDlg::OnCancel() 
{
	try
	{
		
		CDialog::OnCancel();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11540");
}
//--------------------------------------------------------------------------------------------------
void CUSBLicenseKeyManagerDlg::OnMachineChanged() 
{
	try
	{
		CWaitCursor cwCursor;
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11541");
}
//--------------------------------------------------------------------------------------------------
void CUSBLicenseKeyManagerDlg::OnApply() 
{
	try
	{
		CWaitCursor cwCursor;
		applyNewValues();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11542");
}
//--------------------------------------------------------------------------------------------------
void CUSBLicenseKeyManagerDlg::OnServerStatus() 
{
	try
	{
		string strServerName = m_snlcSafeNetCfg.getContactServerName();
		if ( strServerName == "SP_LOCAL_MODE" )
		{
			strServerName = getComputerName();
		}
		// Sentinel server Monitor http Address
		string strMonitorAddress = "http://" + strServerName + ":6002";

		// Create the ThreadDataStruct obj
		ThreadDataStruct ThreadData(this->GetSafeHwnd(), "open", strMonitorAddress.c_str(), 
			NULL, "", SW_SHOWNORMAL);

		// Call the ShellExecuteThread to execute in a separate thread 
		ShellExecuteThread shellExecute(&ThreadData);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12322");
}
//--------------------------------------------------------------------------------------------------
void CUSBLicenseKeyManagerDlg::OnEmailsettings() 
{
	try
	{
		IObjectUserInterfacePtr ipSettingsUI = m_ipEmailSettings;
		ASSERT_RESOURCE_ALLOCATION("ELI12325", ipSettingsUI != NULL);
		
		ipSettingsUI->DisplayReadWrite();

		// Save settings to registry
		IObjectSettingsPtr ipSettings = m_ipEmailSettings;
		ASSERT_RESOURCE_ALLOCATION("ELI25261", ipSettings != NULL);

		ipSettings->SaveToRegistry(gstrEMAIL_REG_PATH.c_str());
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12319");
	
}
//--------------------------------------------------------------------------------------------------
void CUSBLicenseKeyManagerDlg::OnEmailAlert() 
{
	try
	{
		updateControls	();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12320");
}
//--------------------------------------------------------------------------------------------------
// Private Methods
//--------------------------------------------------------------------------------------------------
void CUSBLicenseKeyManagerDlg::updateControls()
{
	int nRemoteState = m_btnRemoteMachine.GetCheck();
	if ( nRemoteState == BST_CHECKED )
	{
		m_editRemoteMachineName.EnableWindow(TRUE);
	}
	else
	{
		m_editRemoteMachineName.EnableWindow(FALSE);
	}		
	if ( m_checkEmailAlert.GetCheck() == BST_CHECKED)
	{
		m_editToList.EnableWindow( TRUE );
	}
	else
	{
		m_editToList.EnableWindow( FALSE );
	}
}
//--------------------------------------------------------------------------------------------------
void CUSBLicenseKeyManagerDlg::loadCounterValuesList(SafeNetLicenseMgr &snLM)
{
	CWaitCursor cwCursor;
	
	// Set grid to read only
	m_listCounterValues.GetParam()->SetLockReadOnly( FALSE );	
	try
	{
		// Add the key serial number
		m_editCounterSerial.SetWindowText (asString(snLM.getKeySN()).c_str());

		// Add the key hard limit
		m_editHardLimit.SetWindowText(asString(snLM.getHardLimit()).c_str());

		// Add the key soft limit
		m_editSoftLimit.SetWindowText(asString(snLM.getCellValue(gdcellFlexIndexUserLimit)).c_str());
	
		setGridCounterValues( snLM, gwINDEXING_COUNTER, gdcellFlexIndexingCounter );
		setGridCounterValues( snLM, gwPAGINATION_COUNTER, gdcellFlexPaginationCounter );
		setGridCounterValues( snLM, gwREDACTION_COUNTER, gdcellIDShieldRedactionCounter );
	}
	catch (UCLIDException ue )
	{
		m_listCounterValues.GetParam()->SetLockReadOnly( TRUE );	
		throw ue;
	}
	m_listCounterValues.GetParam()->SetLockReadOnly( TRUE );	
}
//--------------------------------------------------------------------------------------------------
void CUSBLicenseKeyManagerDlg::prepareCounterList()
{
	CWaitCursor cwCursor;

	//////////////////
	// Prepare headers
	//////////////////
	// Get dimensions of control
	CRect	rect;
	//m_listCounterValues.GetClientRect( &rect );
	
	// Compute column widths
	CRect	rectList;
	m_listCounterValues.GetClientRect( rectList );
	long	lValueWidth = rectList.Width() / 5;
	long	lNameWidth = lValueWidth * 2;

	// Set up colum headers
	vector<string> vecCol;
	vecCol.push_back("Counter");
	vecCol.push_back("Value");
	vecCol.push_back("Alert Level");
	vecCol.push_back("Alert Multiple");

	// Set up Column widths;
	vector<int> vecColWidths;
	vecColWidths.push_back(lNameWidth);
	vecColWidths.push_back(lValueWidth);
	vecColWidths.push_back(lValueWidth);
	vecColWidths.push_back(lValueWidth);

	vector<string> vecRow;
	
	m_listCounterValues.PrepareGrid(vecCol, vecColWidths, vecRow, 0,true, false);

	m_listCounterValues.InsertRows(1, 3 );
	// Unlock the read-only cells
	m_listCounterValues.GetParam()->SetLockReadOnly( FALSE );

	CGXStyle cgxStyle;
	// Set Styles attributes that are the same for all 
	cgxStyle.SetReadOnly(TRUE);
	cgxStyle.SetEnabled(TRUE);
	cgxStyle.SetControl(GX_IDS_CTRL_STATIC);

	// Set FLEX Index - Indexing
	cgxStyle.SetValue( "FLEX Index - Indexing" );
	m_listCounterValues.SetStyleRange(CGXRange( gwINDEXING_COUNTER, giCOUNTER_NAME_COLUMN), cgxStyle );

	// Set FLEX Index - Pagination
	cgxStyle.SetValue( "FLEX Index - Pagination" );
	m_listCounterValues.SetStyleRange(CGXRange( gwPAGINATION_COUNTER, giCOUNTER_NAME_COLUMN), cgxStyle ); 

	// Set ID Shield - Redaction
	cgxStyle.SetValue( "ID Shield - Redaction" );
	m_listCounterValues.SetStyleRange(CGXRange( gwREDACTION_COUNTER, giCOUNTER_NAME_COLUMN), cgxStyle );

	m_listCounterValues.GetParam()->SetLockReadOnly( TRUE );
	loadAlertValues(gwINDEXING_COUNTER, gdcellFlexIndexingCounter);
	loadAlertValues(gwPAGINATION_COUNTER, gdcellFlexPaginationCounter);
	loadAlertValues(gwREDACTION_COUNTER, gdcellIDShieldRedactionCounter);
	clearCounterValuesList();
}
//--------------------------------------------------------------------------------------------------
void CUSBLicenseKeyManagerDlg::applyNewValues()
{
	//string strServerName = "SP_LOCAL_MODE";
	string strServerName = getComputerName();
	if ( m_btnRemoteMachine.GetCheck() == BST_CHECKED )
	{
		CString czServerName;
		m_editRemoteMachineName.GetWindowText(czServerName);
		strServerName = czServerName;
	}
	m_snlcSafeNetCfg.setServerName(strServerName);
	string strSMTPServer = m_ipEmailSettings->SMTPServer;
	if ( m_checkEmailAlert.GetCheck() == BST_CHECKED)
	{
		if ( strSMTPServer.empty())
		{
			UCLIDException ue("ELI11887", "SMTP Server not set in Email Settings.");
			throw ue;
		}
		CString czToList;
		m_editToList.GetWindowText(czToList);
		if ( czToList.IsEmpty() == TRUE )
		{
			m_editToList.SetFocus();
			UCLIDException ue("ELI11888", "No Address list set." );
			throw ue;
		}
		m_snlcSafeNetCfg.setAlertToList(czToList.operator LPCTSTR());
		m_snlcSafeNetCfg.setSendAlert(true);
	}
	else
	{
		m_snlcSafeNetCfg.setSendAlert(false);
	}
	if ( m_checkAlertExtract.GetCheck() == BST_CHECKED)
	{
		m_snlcSafeNetCfg.setSendToExtract(true);
		if ( strSMTPServer.empty())
		{
			UCLIDException ue("ELI11889", "SMTP Server not set in Email Settings.");
			throw ue;
		}
	}
	else
	{
		m_snlcSafeNetCfg.setSendToExtract(false);
	}
	IObjectSettingsPtr ipSettings = m_ipEmailSettings;
	ASSERT_RESOURCE_ALLOCATION("ELI12326", ipSettings != NULL );
	
	ipSettings->SaveToRegistry( gstrEMAIL_REG_PATH.c_str() );
	
	applyAlertValues( gwINDEXING_COUNTER, gdcellFlexIndexingCounter );
	applyAlertValues( gwPAGINATION_COUNTER, gdcellFlexPaginationCounter );
	applyAlertValues( gwREDACTION_COUNTER, gdcellIDShieldRedactionCounter );

}
//--------------------------------------------------------------------------------------------------
void CUSBLicenseKeyManagerDlg::clearCounterValuesList()
{

	// Clear serial number field
	m_editCounterSerial.SetWindowText("");

	// Clear Hard limit field
	m_editHardLimit.SetWindowText("");

	// Clear Soft limit field
	m_editSoftLimit.SetWindowText("");

	m_listCounterValues.GetParam()->SetLockReadOnly( FALSE );	

	// only clear the counter values not the alert values
	m_listCounterValues.SetStyleRange(CGXRange( gwINDEXING_COUNTER,  giCOUNTER_VALUE ,  gwREDACTION_COUNTER, giCOUNTER_VALUE ),
		CGXStyle()
			.SetHorizontalAlignment(DT_CENTER)
			.SetValue( "0" )
			);
	m_listCounterValues.RedrawWindow();
	m_listCounterValues.GetParam()->SetLockReadOnly( TRUE );	
}

//--------------------------------------------------------------------------------------------------
void CUSBLicenseKeyManagerDlg::setGridCounterValues ( SafeNetLicenseMgr &snLM, int nCounterRow, DataCell &dcCell)
{
	CGXStyle gxNonEdit;

	// Set the settings that will be the same for all non Editable
	gxNonEdit.SetHorizontalAlignment(DT_CENTER);
	gxNonEdit.SetReadOnly(TRUE);
	gxNonEdit.SetEnabled(TRUE);
	gxNonEdit.SetAutoSize(FALSE);
	gxNonEdit.SetIncludeAutoSize(TRUE);
	gxNonEdit.SetControl(GX_IDS_CTRL_STATIC);

	SP_DWORD dwValue = snLM.getCellValue(dcCell);	

	// Format cell value as D,DDD,DDD (P13 #3375)
	string strValue = commaFormatNumber( (LONGLONG)dwValue );
	gxNonEdit.SetValue( strValue.c_str() );

	// Add this string and the appropriate Level setting
	m_listCounterValues.SetStyleRange(CGXRange( nCounterRow, giCOUNTER_VALUE ), gxNonEdit);
}
//--------------------------------------------------------------------------------------------------
void CUSBLicenseKeyManagerDlg::loadAlertValues (int nCounterRow, DataCell &dcCell)
{
	CGXStyle gxEdit;
	// Set the settings that will be the same for all editable
	gxEdit.SetValueType(GX_VT_NUMERIC);
	gxEdit.SetHorizontalAlignment(DT_CENTER);
	gxEdit.SetAutoSize(FALSE);
	gxEdit.SetIncludeAutoSize(TRUE);
	gxEdit.SetAllowEnter(FALSE);

	SP_DWORD dwAlertLevel;
	dwAlertLevel = m_snlcSafeNetCfg.getCounterAlertLevel(dcCell.getCellName());

	// Format cell value as D,DDD,DDD (P13 #3672)
	string strValue = commaFormatNumber( (LONGLONG)dwAlertLevel );
	gxEdit.SetValue( strValue.c_str() );

	// Add this string and the appropriate Level setting
	m_listCounterValues.SetStyleRange(CGXRange( nCounterRow, giALERT_LEVEL), gxEdit );

	SP_DWORD dwAlertMultiple;
	dwAlertMultiple = m_snlcSafeNetCfg.getCounterAlertMultiple(dcCell.getCellName());

	// Format cell value as D,DDD,DDD (P13 #3672)
	strValue = commaFormatNumber( (LONGLONG)dwAlertMultiple );
	gxEdit.SetValue( strValue.c_str() );
	m_listCounterValues.SetStyleRange(CGXRange( nCounterRow, giALERT_MULTIPLE), gxEdit );
}
//--------------------------------------------------------------------------------------------------
void CUSBLicenseKeyManagerDlg::applyAlertValues (int nCounterRow, DataCell &dcCell)
{
	CGXStyle cgxStyle;
	DWORD dwAlert;
	// Get new Alert Level for the indexing counter
	m_listCounterValues.GetStyleRowCol(nCounterRow, giALERT_LEVEL, cgxStyle, gxCopy, 0 );
	string strAlert = cgxStyle.GetValue();
	try
	{
		dwAlert = asUnsignedLong(strAlert);
	}
	catch ( ... ) 
	{
		m_listCounterValues.SelectRow(nCounterRow);
		m_listCounterValues.SetCurrentCell(nCounterRow, giALERT_LEVEL );
		m_listCounterValues.SetFocus();
		UCLIDException ue ("ELI11890", "Invalid Alert Level." );
		throw ue;
	}
	//dwAlert = cgxStyle.GetDWordValue();
	// Save Alert level in registry for Indexing Counter
	m_snlcSafeNetCfg.setCounterAlertLevel(dcCell.getCellName(), dwAlert);

	// Get new Alert Level for the indexing counter
	m_listCounterValues.GetStyleRowCol(nCounterRow, giALERT_MULTIPLE, cgxStyle, gxCopy, 0 );
	strAlert = cgxStyle.GetValue();
	try
	{
		dwAlert = asUnsignedLong(strAlert);
	}
	catch ( ... ) 
	{
		m_listCounterValues.SelectRow(nCounterRow);
		m_listCounterValues.SetCurrentCell(nCounterRow, giALERT_MULTIPLE );
		m_listCounterValues.SetFocus();
		UCLIDException ue ("ELI11894", "Invalid Alert Multiple." );
		throw ue;
	}

	//dwAlert = cgxStyle.GetDWordValue();
	// Save Alert level in registry for Indexing Counter
	m_snlcSafeNetCfg.setCounterAlertMultiple(dcCell.getCellName(), dwAlert);
}
//--------------------------------------------------------------------------------------------------
