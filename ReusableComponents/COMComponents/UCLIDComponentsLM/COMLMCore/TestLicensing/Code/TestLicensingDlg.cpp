// TestLicensingDlg.cpp : implementation file
//

#include "stdafx.h"
#include "TestLicensing.h"
#include "TestLicensingDlg.h"

#include <io.h>
#include <fstream.h>
#include <SYS/UTIME.H>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Error string constants
//-------------------------------------------------------------------------------------------------
// Standard string indicating nothing (error condition)
const char gpszUnknown[] = "Unknown";

// Standard string indicating that item is not found
const char gpszItemNotFound[] = "Object not found!";

// Standard string indicating that item is not found and creation failed
const char gpszItemNotFoundNoCreate[] = "Object not found and failed creation!";

// Standard string indicating that item cannot be created
const char gpszCannotCreate[] = "Object cannot be created!";

// Standard string indicating that item cannot be read
const char gpszCannotRead[] = "Object exists but cannot be read!";

// Standard string indicating that item cannot be written
const char gpszCannotWrite[] = "Object can be read but cannot be written!";

//-------------------------------------------------------------------------------------------------
// Success string constants
//-------------------------------------------------------------------------------------------------
// Standard string indicating object existence
const char gpszExistence[] = "PASS: Object exists!";

// Standard string indicating that item can be read
const char gpszCanRead[] = "PASS: Object can be read!";

// Standard string indicating that item can be read
const char gpszCanWrite[] = "PASS: Object can be written!";

// Standard string indicating success
const char gpszSuccess[] = "PASS: Object access succeeded!";

//-------------------------------------------------------------------------------------------------
// Registry key constants
//-------------------------------------------------------------------------------------------------
// Locations for Date-Time items - Version 1
// under HKEY_LOCAL_MACHINE
const char gpszDATE_TIME_KEY1[] = "Software\\Microsoft\\Windows\\System32";
const char gpszCOUNT_KEY1[] = "Software\\Microsoft\\Command Processor";
const char gpszUNLOCK_KEY1[] = "Software\\Microsoft\\DirectInput\\PUBDC";

// Locations for Date-Time items - Version 2
// under HKEY_CURRENT_USER
const char gpszDATE_TIME_KEY2[] = "Identities\\{7FEF3749-A8CC-4CD0-9CEB-E6D267FA524E}";
//const char gpszDATE_TIME_KEY3[] = "Software\\Microsoft\\CurrentVersion\\DateTime";
const char gpszCOUNT_KEY2[] = "Software\\Windows";
const char gpszUNLOCK_KEY2[] = "Software\\Classes\\Code";

// Registry keys
const char gpszLAST_TIME_USED[] = "LTUSWU";
const char gpszCOUNT[] = "Count";

//-------------------------------------------------------------------------------------------------
// File constants
//-------------------------------------------------------------------------------------------------
// Define the Windows subfolder and filename for Date-Time encryption
//   i.e. full path will be $(Windows) + gpszDateTimeSubfolderFile1
// NOTE: This applies only to Version 1
const char gpszDateTimeSubfolderFile1[] = 
	"\\system32\\spool\\prtprocs\\w32x86\\tlsuuw.dll";

// Define the user-specific subfolder and filename for Date-Time encryption
//   i.e. full path will be $(Documents and Settings\Username\Application Data) + gpszDateTimeSubfolderFile
// NOTE 1: This applies only to Version 2
// NOTE 2: The file will have the hidden attribute set
const char gpszDateTimeSubfolderFile2[] = 
	"\\Windows\\tlsuuw_DO_NOT_DELETE.dll";

//-------------------------------------------------------------------------------------------------
// CTestLicensingDlg dialog
//-------------------------------------------------------------------------------------------------
CTestLicensingDlg::CTestLicensingDlg(bool bShowMessages, CWnd* pParent /*=NULL*/)
	: CDialog(CTestLicensingDlg::IDD, pParent),
	m_bShowMessages(bShowMessages),
	m_bCreatedDirectory(false),
	m_bCreatedDTFile(false),
	m_bCreatedDTKey1(false),
	m_bCreatedDTKey2(false),
	m_bCreatedCountKey1(false),
	m_bCreatedCountKey2(false),
	m_iCreatedCodeKey1(-1),
	m_iCreatedCodeKey2(-1),
	m_iTotalErrorCount(0),
	m_bVersion1(true)
{
	//{{AFX_DATA_INIT(CTestLicensingDlg)
	m_zUser = _T("");
	m_zComputer = _T("");
	m_zCode = _T("");
	m_zCount = _T("");
	m_zFile = _T("");
	m_zKey = _T("");
	m_zCountValue = _T("");
	m_zCountFound = _T("");
	m_zSummary = _T("");
	//}}AFX_DATA_INIT
	// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}
//-------------------------------------------------------------------------------------------------
void CTestLicensingDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CTestLicensingDlg)
	DDX_Control(pDX, ID_BTN_TEST_READ, m_btnRead);
	DDX_Control(pDX, ID_BTN_TEST_WRITE, m_btnWrite);
	DDX_Text(pDX, IDC_EDIT_USER, m_zUser);
	DDX_Text(pDX, IDC_EDIT_COMPUTER, m_zComputer);
	DDX_Text(pDX, IDC_EDIT_CODE, m_zCode);
	DDX_Text(pDX, IDC_EDIT_COUNT, m_zCount);
	DDX_Text(pDX, IDC_EDIT_FILE, m_zFile);
	DDX_Text(pDX, IDC_EDIT_KEY, m_zKey);
	DDX_Text(pDX, IDC_EDIT_COUNT_VALUE, m_zCountValue);
	DDX_Text(pDX, IDC_EDIT_COUNT_FOUND, m_zCountFound);
	DDX_Text(pDX, IDC_EDIT_SUMMARY, m_zSummary);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CTestLicensingDlg, CDialog)
	//{{AFX_MSG_MAP(CTestLicensingDlg)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_BN_CLICKED(ID_BTN_TEST_PRESENCE, OnBtnTestPresence)
	ON_BN_CLICKED(ID_BTN_TEST_READ, OnBtnTestRead)
	ON_BN_CLICKED(ID_BTN_TEST_WRITE, OnBtnTestWrite)
	ON_BN_CLICKED(IDC_RADIO_V1, OnRadioV1)
	ON_BN_CLICKED(IDC_RADIO_V2, OnRadioV2)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CTestLicensingDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CTestLicensingDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon
	
	// Get and apply user name
	m_zUser = getCurrentUserName().c_str();

	// Get and apply computer name
	m_zComputer = getComputerName().c_str();

	// Disable Read and Write buttons
	m_btnRead.EnableWindow( FALSE );
	m_btnWrite.EnableWindow( FALSE );

	// Update version buttons
	if (m_bVersion1)
	{
		((CButton *)GetDlgItem( IDC_RADIO_V1 ))->SetCheck( 1 );
	}
	else
	{
		((CButton *)GetDlgItem( IDC_RADIO_V2 ))->SetCheck( 1 );
	}

	// Refresh display
	UpdateData( FALSE );

	return TRUE;  // return TRUE  unless you set the focus to a control
}
//-------------------------------------------------------------------------------------------------
void CTestLicensingDlg::OnCancel() 
{
	// Cleaning up registry keys created during testing

	////////////////////////
	// Directory and/or file
	////////////////////////
	// Get full path to Date-Time file
	std::string	strPath = getDateTimeFilePath();

	if (m_bCreatedDirectory)
	{
		// Delete the file
		if (!DeleteFile( strPath.c_str() ))
		{
			if (m_bShowMessages)
			{
				MessageBox( "Failed to delete file", "Update Error", MB_OK );
			}
		}

		// Get the directory and remove it
		std::string	strDir = getDirectoryFromFullPath( strPath );
		if (!RemoveDirectory( strDir.c_str() ))
		{
			if (m_bShowMessages)
			{
				DWORD dwError = GetLastError();
				CString	zError;
				zError.Format( "Failed (Error = %ld) to remove directory", dwError );
				MessageBox( zError, "Update Error", MB_OK );
			}
		}
	}
	else if (m_bCreatedDTFile)
	{
		// Delete the file
		if (!DeleteFile( strPath.c_str() ))
		{
			if (m_bShowMessages)
			{
				MessageBox( "Failed to delete file", "Update Error", MB_OK );
			}
		}
	}

	////////////////
	// Date-Time key
	////////////////
	if (m_bCreatedDTKey1)
	{
		// Do not delete the whole HKLM key
		deleteKey( gpszDATE_TIME_KEY1, gpszLAST_TIME_USED, false );
	}

	if (m_bCreatedDTKey2)
	{
		deleteKey( gpszDATE_TIME_KEY2, gpszLAST_TIME_USED, true );
	}

	///////////////
	// Unlock count
	///////////////
	if (m_bCreatedCountKey1)
	{
		// Do not delete the whole HKLM key
		deleteKey( gpszCOUNT_KEY1, gpszCOUNT, false );
	}

	if (m_bCreatedCountKey2)
	{
		deleteKey( gpszCOUNT_KEY2, gpszCOUNT, true );
	}

	//////////////////
	// Unlock code key
	//////////////////
	if (m_iCreatedCodeKey1 > -1)
	{
		CString	zTemp;
		zTemp.Format( "%d", m_iCreatedCodeKey1 );

		deleteKey( gpszUNLOCK_KEY1, zTemp.operator LPCTSTR(), 
			(m_iCreatedCodeKey1 > 0) ? false : true );
	}

	if (m_iCreatedCodeKey2 > -1)
	{
		CString	zTemp;
		zTemp.Format( "%d", m_iCreatedCodeKey2 );

		deleteKey( gpszUNLOCK_KEY2, zTemp.operator LPCTSTR(), 
			(m_iCreatedCodeKey2 > 0) ? false : true );
	}

	CDialog::OnCancel();
}
//-------------------------------------------------------------------------------------------------
// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.
void CTestLicensingDlg::OnPaint() 
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
//-------------------------------------------------------------------------------------------------
// The system calls this to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CTestLicensingDlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}
//-------------------------------------------------------------------------------------------------
void CTestLicensingDlg::OnBtnTestPresence() 
{
	// Reset error count
	m_iTotalErrorCount = 0;

	// Reset edit boxes
	clearFields();

	// Test each component and provide results
	m_zFile = testFilePresence().c_str();

	// Date-Time registry key
	if (m_bVersion1 && testRegistryPresence( gpszDATE_TIME_KEY1, gpszLAST_TIME_USED ))
	{
		m_zKey = gpszExistence;
	}
	else if (!m_bVersion1 && testRegistryPresence( gpszDATE_TIME_KEY2, gpszLAST_TIME_USED ))
	{
		m_zKey = gpszExistence;
	}
	else
	{
		m_zKey = gpszItemNotFound;

		// Update error count
		m_iTotalErrorCount++;
		if (m_bShowMessages)
		{
			MessageBox( "DT item not found", "Update Error", MB_OK );
		}

		// Attempt creation of key
		if (m_bVersion1 && createKey( gpszDATE_TIME_KEY1, gpszLAST_TIME_USED, "test" ))
		{
			// Set flag
			m_bCreatedDTKey1 = true;
		}
		else if (!m_bVersion1 && createKey( gpszDATE_TIME_KEY2, gpszLAST_TIME_USED, "test" ))
		{
			// Set flag
			m_bCreatedDTKey2 = true;
		}
	}

	// Unlock Count registry key
	bool	bCountFound = false;
	if (m_bVersion1 && testRegistryPresence( gpszCOUNT_KEY1, gpszCOUNT ))
	{
		m_zCount = gpszExistence;
		bCountFound = true;
	}
	else if (!m_bVersion1 && testRegistryPresence( gpszCOUNT_KEY2, gpszCOUNT ))
	{
		m_zCount = gpszExistence;
		bCountFound = true;
	}
	else
	{
		// May or may not be an error
		m_zCount = gpszItemNotFound;

		// Attempt creation of key
		if (m_bVersion1 && createKey( gpszCOUNT_KEY1, gpszCOUNT, "0" ))
		{
			// Set flag
			m_bCreatedCountKey1 = true;
		}
		else if (!m_bVersion1 && createKey( gpszCOUNT_KEY2, gpszCOUNT, "0" ))
		{
			// Set flag
			m_bCreatedCountKey2 = true;
		}
		else
		{
			// Error if Count item cannot even be created
			m_zCount = gpszItemNotFoundNoCreate;
			m_iTotalErrorCount++;
			if (m_bShowMessages)
			{
				MessageBox( "Cannot create Count item", "Update Error", MB_OK );
			}
		}
	}

	// First Unlock Code registry key --> only expected if Count > 0
	if (m_bVersion1 && testRegistryPresence( gpszUNLOCK_KEY1, "1" ))
	{
		m_zCode = gpszExistence;
	}
	else if (!m_bVersion1 && testRegistryPresence( gpszUNLOCK_KEY2, "1" ))
	{
		m_zCode = gpszExistence;
	}
	else
	{
		m_zCode = gpszItemNotFound;

		// May or may not be an error
		if (bCountFound)
		{
			// Update error count
			m_iTotalErrorCount++;
			if (m_bShowMessages)
			{
				MessageBox( "Code item(s) not found but Count found", "Update Error", MB_OK );
			}
		}

		// Attempt creation of key
		if (m_bVersion1 && createKey( gpszUNLOCK_KEY1, "0", "0" ))
		{
			// Set item indicator --> to be deleted at exit
			m_iCreatedCodeKey1 = 0;
		}
		else if (!m_bVersion1 && createKey( gpszUNLOCK_KEY2, "0", "0" ))
		{
			// Set item indicator --> to be deleted at exit
			m_iCreatedCodeKey2 = 0;
		}
		else
		{
			// Error if Code item cannot even be created
			m_zCode = gpszItemNotFoundNoCreate;
			m_iTotalErrorCount++;
			if (m_bShowMessages)
			{
				MessageBox( "Cannot create Code item", "Update Error", MB_OK );
			}
		}
	}

	// Just ignore the actual Count and actual codes at this time

	// Update the summary
	m_zSummary.Format( "%d errors found in Presence test", m_iTotalErrorCount );

	// Enable the Read button
	m_btnRead.EnableWindow( TRUE );

	// Disable the Write button
	m_btnWrite.EnableWindow( FALSE );

	// Refresh display
	UpdateData( FALSE );
}
//-------------------------------------------------------------------------------------------------
void CTestLicensingDlg::OnBtnTestRead() 
{
	// Reset edit boxes
	clearFields();

	// Test each component and provide results
	m_zFile = testFileRead().c_str();

	// Date-Time registry key
	if (m_bVersion1 && testRegistryReadAccess( gpszDATE_TIME_KEY1, gpszLAST_TIME_USED ))
	{
		m_zKey = gpszCanRead;
	}
	else if (!m_bVersion1 && testRegistryReadAccess( gpszDATE_TIME_KEY2, gpszLAST_TIME_USED ))
	{
		m_zKey = gpszCanRead;
	}
	else
	{
		m_zKey = gpszCannotRead;

		// Update error count
		m_iTotalErrorCount++;
		if (m_bShowMessages)
		{
			MessageBox( "Cannot read DT item", "Update Error", MB_OK );
		}
	}

	// Unlock Count registry key
	if (m_bVersion1 && testRegistryReadAccess( gpszCOUNT_KEY1, gpszCOUNT ))
	{
		m_zCount = gpszCanRead;
	}
	else if (!m_bVersion1 && testRegistryReadAccess( gpszCOUNT_KEY2, gpszCOUNT ))
	{
		m_zCount = gpszCanRead;
	}
	else
	{
		m_zCount = gpszCannotRead;

		// Update error count
		m_iTotalErrorCount++;
		if (m_bShowMessages)
		{
			MessageBox( "Cannot read Count item", "Update Error", MB_OK );
		}
	}

	// Unlock Code registry key
	long lCount = atol( m_zCountValue.operator LPCTSTR() );
	if (lCount > 0)
	{
		if ((m_bVersion1 && testRegistryReadAccess( gpszUNLOCK_KEY1, "1" )) || 
			testRegistryReadAccess( gpszUNLOCK_KEY2, "1" ))
		{
			m_zCode = gpszCanRead;

			// Test presence of each of the expected Unlock code keys
			std::string	strTest;
			bool	bErrorFound = false;
			for (int i = 1; i <= lCount; i++)
			{
				CString zItem;
				zItem.Format( "%d", i );
				if ((m_bVersion1 && testRegistryPresence( gpszUNLOCK_KEY1, 
					zItem.operator LPCTSTR() )) || 
					testRegistryPresence( gpszUNLOCK_KEY2, zItem.operator LPCTSTR() ))
				{
					// Update count found string
					m_zCountFound = zItem;
				}
				else
				{
					// Increment error count
					m_iTotalErrorCount++;
					if (m_bShowMessages)
					{
						MessageBox( "Missing expected Code", "Update Error", MB_OK );
					}

					// Quit checking any remaining items
					bErrorFound = true;
					break;
				}
			}

			// Check for extra Unlock codes
			if (!bErrorFound)
			{
				CString zItem;
				zItem.Format( "%d", lCount + 1 );
				if ((m_bVersion1 && testRegistryPresence( gpszUNLOCK_KEY1, 
					zItem.operator LPCTSTR() )) || 
					testRegistryPresence( gpszUNLOCK_KEY2, zItem.operator LPCTSTR() ))
				{
					// Update count found string
					m_zCountFound = zItem;

					// Increment error count
					m_iTotalErrorCount++;
					if (m_bShowMessages)
					{
						MessageBox( "Found extra Code", "Update Error", MB_OK );
					}
				}
			}
		}
		else
		{
			m_zCode = gpszCannotRead;

			// Update error count
			m_iTotalErrorCount++;
			if (m_bShowMessages)
			{
				MessageBox( "Cannot read Code item '1'", "Update Error", MB_OK );
			}
		}
	}
	else
	{
		// Test for created "0" subkey
		if (m_bVersion1 && testRegistryReadAccess( gpszUNLOCK_KEY1, "0" ))
		{
			m_zCode = gpszCanRead;
		}
		else if (!m_bVersion1 && testRegistryReadAccess( gpszUNLOCK_KEY2, "0" ))
		{
			m_zCode = gpszCanRead;
		}
		else
		{
			m_zCode = gpszCannotRead;

			// Update error count
			m_iTotalErrorCount++;
			if (m_bShowMessages)
			{
				MessageBox( "Cannot read Code item '0'", "Update Error", MB_OK );
			}
		}

		// Provide count of zero
		m_zCountFound = "0";
	}

	// Update the summary
	m_zSummary.Format( "%d errors found in Presence + Read tests", m_iTotalErrorCount );

	// Enable the Write button
	m_btnWrite.EnableWindow( TRUE );

	// Refresh display
	UpdateData( FALSE );
}
//-------------------------------------------------------------------------------------------------
void CTestLicensingDlg::OnBtnTestWrite() 
{
	// Reset edit boxes
	clearFields();

	// Test each component and provide results
	m_zFile = testFileWrite().c_str();

	// Date-Time registry key
	if (m_bVersion1 && testRegistryWriteAccess( gpszDATE_TIME_KEY1, gpszLAST_TIME_USED ))
	{
		m_zKey = gpszCanWrite;
	}
	else if (!m_bVersion1 && testRegistryWriteAccess( gpszDATE_TIME_KEY2, gpszLAST_TIME_USED ))
	{
		m_zKey = gpszCanWrite;
	}
	else
	{
		m_zKey = gpszCannotWrite;

		// Update error count
		m_iTotalErrorCount++;
		if (m_bShowMessages)
		{
			MessageBox( "Cannot write DT item in Write()", "Update Error", MB_OK );
		}
	}

	// Unlock Count registry key
	if (m_bVersion1 && testRegistryWriteAccess( gpszCOUNT_KEY1, gpszCOUNT ))
	{
		m_zCount = gpszCanWrite;
	}
	else if (!m_bVersion1 && testRegistryWriteAccess( gpszCOUNT_KEY2, gpszCOUNT ))
	{
		m_zCount = gpszCanWrite;
	}
	else
	{
		m_zCount = gpszCannotWrite;

		// Update error count
		m_iTotalErrorCount++;
		if (m_bShowMessages)
		{
			MessageBox( "Cannot write Count item", "Update Error", MB_OK );
		}
	}

	// Unlock Code registry key
	long lCount = atol( m_zCountFound.operator LPCTSTR() );
	CString	zTestCount;
	zTestCount.Format( "%d", lCount );
	if (lCount > -1)
	{
		// Test writing to an existing code item
		if (m_bVersion1 && testRegistryWriteAccess( gpszUNLOCK_KEY1, zTestCount.operator LPCTSTR() ))
		{
			m_zCode = gpszCanWrite;
		}
		else if (!m_bVersion1 && testRegistryWriteAccess( gpszUNLOCK_KEY2, zTestCount.operator LPCTSTR() ))
		{
			m_zCode = gpszCanWrite;
		}
		else
		{
			m_zCode = gpszCannotWrite;

			// Update error count
			m_iTotalErrorCount++;
			if (m_bShowMessages)
			{
				MessageBox( "Cannot write existing Code item", "Update Error", MB_OK );
			}
		}
	}
	else
	{
		// First create a new Code key
		if (m_bVersion1)
		{
			createKey( gpszUNLOCK_KEY1, zTestCount.operator LPCTSTR(), "0" );
			m_iCreatedCodeKey1 = lCount;
		}
		else
		{
			createKey( gpszUNLOCK_KEY2, zTestCount.operator LPCTSTR(), "0" );
			m_iCreatedCodeKey2 = lCount;
		}

		// Test writing to the new code item
		if (m_bVersion1 && testRegistryWriteAccess( gpszUNLOCK_KEY1, zTestCount.operator LPCTSTR() ))
		{
			m_zCode = gpszCanWrite;
		}
		else if (!m_bVersion1 && testRegistryWriteAccess( gpszUNLOCK_KEY2, zTestCount.operator LPCTSTR() ))
		{
			m_zCode = gpszCanWrite;
		}
		else
		{
			m_zCode = gpszCannotWrite;

			// Update error count
			m_iTotalErrorCount++;
			if (m_bShowMessages)
			{
				MessageBox( "Cannot write new Code item '0'", "Update Error", MB_OK );
			}
		}
	}

	// Just ignore the actual Count and actual codes at this time

	// Update the summary
	m_zSummary.Format( "%d errors found in Presence + Read + Write tests", m_iTotalErrorCount );

	// Refresh display
	UpdateData( FALSE );
}
//-------------------------------------------------------------------------------------------------
void CTestLicensingDlg::OnRadioV1() 
{
	// Update data member
	m_bVersion1 = true;

	// Reset error counter
	m_iTotalErrorCount = 0;

	// Enable only Presence button
	m_btnRead.EnableWindow( FALSE );
	m_btnWrite.EnableWindow( FALSE );

	// Clear all fields
	clearFields();
	m_zCountValue = "";
	m_zCountFound = "";
	m_zSummary = "";
	UpdateData( FALSE );
}
//-------------------------------------------------------------------------------------------------
void CTestLicensingDlg::OnRadioV2() 
{
	// Update data member
	m_bVersion1 = false;

	// Reset error counter
	m_iTotalErrorCount = 0;

	// Enable only Presence button
	m_btnRead.EnableWindow( FALSE );
	m_btnWrite.EnableWindow( FALSE );

	// Clear all fields
	clearFields();
	m_zCountValue = "";
	m_zCountFound = "";
	m_zSummary = "";
	UpdateData( FALSE );
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CTestLicensingDlg::clearFields()
{
	// Clear the tested strings
	m_zFile = _T("");
	m_zKey = _T("");
	m_zCount = _T("");
	m_zCode = _T("");

	// Refresh display
	UpdateData( FALSE );
}
//-------------------------------------------------------------------------------------------------
bool CTestLicensingDlg::createKey(std::string strKey, std::string strSubKey, std::string strValue)
{
	bool	bResult = false;

	// Create the registry key
	HKEY hKey;
	DWORD dwDisposition = 0;
	LONG lReturn = ::RegCreateKeyEx( m_bVersion1 ? HKEY_LOCAL_MACHINE : HKEY_CURRENT_USER, 
								TEXT( strKey.c_str() ),
								0,
								NULL,
								REG_OPTION_NON_VOLATILE,
//								KEY_ALL_ACCESS,
								KEY_EXECUTE | KEY_WRITE,
								NULL,
								&hKey,
								&dwDisposition );

	// Check key existence
	if (lReturn == ERROR_SUCCESS)
	{
		DWORD dwSize = strValue.size();

		// Assign the value to the key
		lReturn = ::RegSetValueEx( hKey, 
								   TEXT( strSubKey.c_str()), 
								   0, 
								   REG_SZ, 
								   (LPBYTE)strValue.c_str(), 
								   dwSize );

		if (lReturn == ERROR_SUCCESS)
		{
			// Set flag
			bResult = true;
		}
		else if (m_bShowMessages)
		{
			CString zTemp;
			zTemp.Format( "Failed to set value in createKey() with error = %d", lReturn );
			MessageBox( zTemp, "Error", MB_OK | MB_ICONINFORMATION );
		}
	}
	else if (m_bShowMessages)
	{
		CString zTemp;
		zTemp.Format( "Failed to create key (subkey = %s) with error = %d", 
			strSubKey.c_str(), lReturn );
		MessageBox( zTemp, "Error", MB_OK | MB_ICONINFORMATION );
	}

	// Close the key when finished
	::RegCloseKey( hKey );

	return bResult;
}
//-------------------------------------------------------------------------------------------------
void CTestLicensingDlg::deleteKey(std::string strKey, std::string strSubKey, bool bDeleteWholeKey)
{
	HKEY hKey;

	// Open the registry key
	LONG lReturn = ::RegOpenKeyEx( m_bVersion1 ? HKEY_LOCAL_MACHINE : HKEY_CURRENT_USER, 
		TEXT( strKey.c_str() ),		// key name
		0,							// reserved
		KEY_EXECUTE | KEY_WRITE,	// security access mask (Read & Write permission)
		&hKey );					// handle to opened key

	// Check key existence
	if (lReturn == ERROR_SUCCESS)
	{
		// Now delete the specified subkey
		lReturn = ::RegDeleteValue( hKey, TEXT( strSubKey.c_str() ) );

		if (lReturn != ERROR_SUCCESS)
		{
			// Tell user about error
			if (m_bShowMessages)
			{
				MessageBox( "Unable to remove value created during testing", 
				"Error", MB_OK | MB_ICONINFORMATION );
			}
		}

		// Check for deleting whole key
		if (bDeleteWholeKey)
		{
			lReturn = ::RegDeleteKey( m_bVersion1 ? HKEY_LOCAL_MACHINE : HKEY_CURRENT_USER, 
				TEXT( strKey.c_str() ) );

			if (lReturn != ERROR_SUCCESS)
			{
				// Tell user about error
				if (m_bShowMessages)
				{
					MessageBox( "Unable to remove item created during testing", 
					"Error", MB_OK | MB_ICONINFORMATION );
				}
			}
		}
	}
	else
	{
		// Tell user about error
		if (m_bShowMessages)
		{
			MessageBox( "Unable to open item created during testing", 
			"Error", MB_OK | MB_ICONINFORMATION );
		}
	}

	// Close the key when finished
	::RegCloseKey( hKey );
}
//-------------------------------------------------------------------------------------------------
std::string CTestLicensingDlg::getCurrentUserName()
{
	std::string strUserName = "NOT AVAILABLE";

	// initialize variables
	char pszUserName[512];
	unsigned long ulBufferSize = sizeof(pszUserName);

	// get the current logged-in user's name
	if (GetUserName(pszUserName, &ulBufferSize))
	{
		strUserName = pszUserName;
	}

	return strUserName;
}
//-------------------------------------------------------------------------------------------------
std::string CTestLicensingDlg::getComputerName()
{
	std::string strComputerName = "NOT AVAILABLE";

	// initialize variables
	char pszComputerName[512];
	unsigned long ulBufferSize = sizeof(pszComputerName);

	// get this computer's name
	if (GetComputerName(pszComputerName, &ulBufferSize))
	{
		strComputerName = pszComputerName;
	}

	return strComputerName;
}
//-------------------------------------------------------------------------------------------------
std::string CTestLicensingDlg::getDateTimeFilePath()
{
	std::string	strDTFile;
	char pszDir[MAX_PATH];

	if (m_bVersion1)
	{
		// Get path to Windows folder
		UINT	uiResult = GetWindowsDirectory( pszDir, MAX_PATH );
		if (uiResult != 0)
		{
			// Get path and filename for DT file
			strDTFile = pszDir;
			strDTFile += gpszDateTimeSubfolderFile1;
		}
	}
	else
	{
		// Get path to special (Application Data) folder
		if (SUCCEEDED (SHGetSpecialFolderPath( NULL, pszDir, CSIDL_APPDATA, 0 ))) 
		{
			// Append the filename
			strDTFile = pszDir;
			strDTFile += gpszDateTimeSubfolderFile2;
		}
	}

	// Return results
	return strDTFile;
}
//-------------------------------------------------------------------------------------------------
std::string CTestLicensingDlg::getDirectoryFromFullPath(std::string strFullFileName)
{
	char zDrive[_MAX_DRIVE], zPath[_MAX_PATH], zFileName[_MAX_FNAME], zExt[_MAX_EXT];
	// Break a path name into components.
	_splitpath(strFullFileName.c_str(), zDrive, zPath, zFileName, zExt);

	// this directory has a trailing "\"
	std::string strRet = std::string(zDrive) + std::string(zPath);
	
	// remove the trailing slash
	long lSize = strRet.length();
	if (strRet[lSize - 1] == '\\')
	{
		strRet = strRet.substr( 0, lSize - 1 );
	}

	return strRet;
}
//--------------------------------------------------------------------------------------------------
std::string CTestLicensingDlg::testFilePresence()
{
	// Default to object not found
	std::string strResult = gpszItemNotFound;

	// Get path to file
	std::string	strPath = getDateTimeFilePath();

	// Check file presence
	if (_access( strPath.c_str(), 00 ) == 0)
	{
		// File exists
		strResult = gpszExistence;
	}
	else
	{
		// Increment error count
		m_iTotalErrorCount++;
		if (m_bShowMessages)
		{
			MessageBox( "DT 1 item not found", "Update Error", MB_OK );
		}

		// Check for desired directory
		std::string	strDir = getDirectoryFromFullPath( strPath );
		if (_access( strDir.c_str(), 00 ) != 0)
		{
			// Need to create the directory before file can be created
			if (!::CreateDirectory( strDir.c_str(), NULL ))
			{
				if (m_bShowMessages)
				{
					DWORD dwErr = GetLastError();
					CString zTemp;
					zTemp.Format( "Unable to create DT 1 item folder error = %ld.", dwErr );

					MessageBox( zTemp, "Update Error", MB_OK );
				}

				// Just return now
				strResult = gpszItemNotFoundNoCreate;
				return strResult;
			}
			else
			{
				// Set flag for directory to be deleted at exit
				m_bCreatedDirectory = true;
			}
		}

		// Create an empty file for later testing
		HANDLE hFile = ::CreateFile( strPath.c_str(), GENERIC_READ | GENERIC_WRITE, 0, 
			NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_HIDDEN, NULL );

		if (hFile == INVALID_HANDLE_VALUE)
		{
			if (m_bShowMessages)
			{
				MessageBox( "Unable to create DT 1 item", "Update Error", MB_OK );
			}
		}
		else
		{
			CFile	file( (int) hFile );

			// Write random test data to the file
			char pBuffer[100];
			file.Write( pBuffer, 100 );
			file.Flush();
			file.Close();

			// Set flag for file to be deleted at exit
			m_bCreatedDTFile = true;
		}
	}

	// Return results
	return strResult;
}
//-------------------------------------------------------------------------------------------------
std::string CTestLicensingDlg::testFileRead()
{
	// Default to object not found
	std::string strResult = gpszItemNotFound;

	// Get path to file
	std::string	strPath = getDateTimeFilePath();

	// Check file presence
	if (_access( strPath.c_str(), 00 ) == 0)
	{
		// File exists, now check read access
		strResult = gpszCannotRead;

		if (_access( strPath.c_str(), 02 ) == 0)
		{
			// File has read access
			strResult = gpszCanRead;
		}
		else
		{
			// Increment error count
			m_iTotalErrorCount++;
			if (m_bShowMessages)
			{
				MessageBox( "Cannot read DT 1 item", "Update Error", MB_OK );
			}
		}
	}
	else
	{
		// Increment error count
		m_iTotalErrorCount++;
		if (m_bShowMessages)
		{
			MessageBox( "DT 1 item not found in Read", "Update Error", MB_OK );
		}
	}

	// Return results
	return strResult;
}
//-------------------------------------------------------------------------------------------------
std::string CTestLicensingDlg::testFileWrite()
{
	// Default to object not found
	std::string strResult = gpszItemNotFound;

	// Get path to file
	std::string	strPath = getDateTimeFilePath();

	// Check file presence
	if (_access( strPath.c_str(), 00 ) == 0)
	{
		// File exists, now check read access
		strResult = gpszCannotRead;

		if (_access( strPath.c_str(), 02 ) == 0)
		{
			// File has read access, now check write access
			strResult = gpszCanRead;

			// Save original time settings for file
			FILETIME	ftCreationTime;
			FILETIME	ftLastAccessTime;
			FILETIME	ftLastWriteTime;
			CFileFind	ffSource;
			bool		bFoundTimes = false;
			if (ffSource.FindFile( strPath.c_str() ))
			{
				// Find the "next" file and update source 
				// information for this one
				BOOL	bMoreFiles = ffSource.FindNextFile();

				// Get file times
				ffSource.GetCreationTime( &ftCreationTime );
				ffSource.GetLastAccessTime( &ftLastAccessTime );
				ffSource.GetLastWriteTime( &ftLastWriteTime );

				// Set flag
				bFoundTimes = true;
			}

			// Open the text file
			CStdioFile	file;
			if (file.Open( strPath.c_str(), CFile::modeReadWrite ))
			{
				// Read the original string
				CString	zOriginal;
				file.ReadString( zOriginal );
				file.SeekToBegin();

				// Write a test string
				file.WriteString( "test string" );

				// Close the file
				file.Close();

				// Reopen the file and replace the original string
				try
				{
					CStdioFile	fileSame( strPath.c_str(), CFile::modeWrite );

					// Write the data to the output file stream
					fileSame.WriteString( zOriginal.operator LPCTSTR() );
					fileSame.Flush();
					fileSame.Close();

					// File has write access
					strResult = gpszCanWrite;
				}
				catch (...)
				{
					// Set error string
					strResult = gpszCannotWrite;

					// Increment error count
					m_iTotalErrorCount++;
					if (m_bShowMessages)
					{
						MessageBox( "Unable to rewrite DT 1 item", "Update Error", MB_OK );
					}
				}

				// Restore original file times
				if (bFoundTimes)
				{
					// Convert original time settings into time_t objects
					CTime	ctmAccess( ftLastAccessTime );
					CTime	ctmWrite( ftLastWriteTime );
					time_t	tmAccess = ctmAccess.GetTime();
					time_t	tmWrite = ctmWrite.GetTime();

					// Set file access and modification times
					struct _utimbuf	tmSettings;
					tmSettings.actime = tmAccess;
					tmSettings.modtime = tmWrite;

					// Reset file access and modification times
					_utime( strPath.c_str(), &tmSettings );
				}
			}
			else
			{
				// Set error string
				strResult = gpszCannotWrite;

				// Increment error count
				m_iTotalErrorCount++;
				if (m_bShowMessages)
				{
					MessageBox( "Cannot open DT 1 item for ReadWrite in testWrite", 
						"Update Error", MB_OK );
				}
			}
		}
		else
		{
			// Increment error count
			m_iTotalErrorCount++;
			if (m_bShowMessages)
			{
				MessageBox( "Cannot read DT 1 item in write()", "Update Error", MB_OK );
			}
		}
	}
	else
	{
		// Increment error count
		m_iTotalErrorCount++;
		if (m_bShowMessages)
		{
			MessageBox( "DT 1 item not found in Write", "Update Error", MB_OK );
		}
	}

	// Return results
	return strResult;
}
//-------------------------------------------------------------------------------------------------
bool CTestLicensingDlg::testRegistryPresence(std::string strKey, std::string strSubKey)
{
	bool bResult = false;

	HKEY hKey;

	// Open the registry key
	LONG lReturn = ::RegOpenKeyEx( m_bVersion1 ? HKEY_LOCAL_MACHINE : HKEY_CURRENT_USER, 
		TEXT( strKey.c_str() ),				// key name
		0,									// reserved
		KEY_EXECUTE | KEY_QUERY_VALUE,		// security access mask (Read & Query subkey permission)
		&hKey );							// handle to opened key

	// Check key existence
	if (lReturn == ERROR_SUCCESS)
	{
		// Now look for subkey
		if (!strSubKey.empty())
		{
			// set max length to be 500
			TCHAR szValue[500];
			DWORD dwBufLen = 500;

			lReturn = ::RegQueryValueEx( hKey,
									TEXT( strSubKey.c_str() ),
									NULL,
									NULL,
									(LPBYTE)szValue,
									&dwBufLen );

			// Check results
			if (lReturn == ERROR_SUCCESS)
			{
				// Now we know the key exists
				bResult = true;
			}
		}
		else
		{
			// Now we know the key exists and we don't care about the subkey
			bResult = true;
		}
	}

	// Close the key when finished
	::RegCloseKey( hKey );

	// Return results
	return bResult;
}
//-------------------------------------------------------------------------------------------------
bool CTestLicensingDlg::testRegistryReadAccess(std::string strKey, std::string strSubKey)
{
	bool bResult = false;

	HKEY hKey;

	// Open the registry key
	LONG lReturn = ::RegOpenKeyEx( m_bVersion1 ? HKEY_LOCAL_MACHINE : HKEY_CURRENT_USER, 
		TEXT( strKey.c_str() ),		// key name
		0,							// reserved
		KEY_EXECUTE,				// security access mask (Read permission)
		&hKey );					// handle to opened key

	// Check key existence
	if (lReturn == ERROR_SUCCESS)
	{
		// Set max length to be 500
		TCHAR szValue[500];					// original text
		DWORD dwBufLen = 500;

		// Now check specific subkey
		lReturn = ::RegQueryValueEx( hKey,
								TEXT( strSubKey.c_str() ),
								NULL,
								NULL,
								(LPBYTE)szValue,
								&dwBufLen );

		if (lReturn == ERROR_SUCCESS)
		{
			// Now we know the key exists and has been read
			bResult = true;

			// Special treatment for Unlock Count
			if (strSubKey.compare( gpszCOUNT ) == 0)
			{
				m_zCountValue = szValue;
			}
		}
	}

	// Close the key when finished
	::RegCloseKey( hKey );

	// Return results
	return bResult;
}
//-------------------------------------------------------------------------------------------------
bool CTestLicensingDlg::testRegistryWriteAccess(std::string strKey, std::string strSubKey)
{
	bool bResult = false;

	HKEY hKey;

	// Open the registry key
	LONG lReturn = ::RegOpenKeyEx( m_bVersion1 ? HKEY_LOCAL_MACHINE : HKEY_CURRENT_USER, 
		TEXT( strKey.c_str() ),		// key name
		0,							// reserved
		KEY_EXECUTE | KEY_WRITE,	// security access mask (Read & Write permission)
		&hKey );					// handle to opened key

	// Check key existence
	if (lReturn == ERROR_SUCCESS)
	{
		// Set max length to be 500
		TCHAR szValue[500];					// original text
		DWORD dwBufLen = 500;

		// Now check specific subkey
		lReturn = ::RegQueryValueEx( hKey,
								TEXT( strSubKey.c_str() ),
								NULL,
								NULL,
								(LPBYTE)szValue,
								&dwBufLen );

		if (lReturn == ERROR_SUCCESS)
		{
			TCHAR szNewValue[5] = "test";		// new text

			// Now test writing to the subkey
			lReturn = ::RegSetValueEx( hKey, 
								  TEXT( strSubKey.c_str() ),
								  0,		// reserved
								  REG_SZ,	// value type
								  (LPBYTE)szNewValue,			// value data
								  5 );		// size of new value

			// Check result
			if (lReturn == ERROR_SUCCESS)
			{
				// Now replace the original string
				lReturn = ::RegSetValueEx( hKey, 
									  TEXT( strSubKey.c_str() ),
									  0,		// reserved
									  REG_SZ,	// value type
									  (LPBYTE)szValue,			// original text
									  dwBufLen );		// size of original text

				// Now we know the key exists and has been written
				bResult = true;
			}
		}
	}

	// Close the key when finished
	::RegCloseKey( hKey );

	// Return result
	return bResult;
}
//-------------------------------------------------------------------------------------------------
