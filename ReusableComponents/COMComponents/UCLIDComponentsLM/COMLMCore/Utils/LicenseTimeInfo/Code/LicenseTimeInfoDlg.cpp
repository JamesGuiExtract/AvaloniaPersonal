// LicenseTimeInfoDlg.cpp : implementation file
//

#include "stdafx.h"
#include "LicenseTimeInfo.h"
#include "LicenseTimeInfoDlg.h"

#include <UCLIDException.hpp>
#include <cpputil.hpp>
#include <RegistryPersistenceMgr.h>
#include <EncryptionEngine.hpp>
#include <LMData.h>
#include <ClipboardManager.h>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Registry keys for Date-Time items
//   NOTE: Each key is under HKEY_CURRENT_USER
const string CLicenseTimeInfoDlg::ITEM_SECTION_NAME1 = "Identities\\{7FEF3749-A8CC-4CD0-9CEB-E6D267FA524E}";
const string CLicenseTimeInfoDlg::ITEM_SECTION_NAME2 = "Identities\\{526988F0-27BE-4451-B741-D8614827B838}";

// Registry keys
const string CLicenseTimeInfoDlg::LAST_TIME_USED = "LTUSWU";

// Define the user-specific subfolder and filename for Date-Time encryption
//   i.e. full path will be $(Documents and Settings\Username\Application Data) + gpszDateTimeSubfolderFile
const char gpszDateTimeSubfolderFile[] = "\\Windows\\tlsuuw_DO_NOT_DELETE.dll";

// Modulo constant for random additions to DT strings
const unsigned short gusMODULO_CONSTANT = 17;

//-------------------------------------------------------------------------------------------------
// CLicenseTimeInfoDlg dialog
//-------------------------------------------------------------------------------------------------
CLicenseTimeInfoDlg::CLicenseTimeInfoDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CLicenseTimeInfoDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(CLicenseTimeInfoDlg)
	m_zFile1 = _T("");
	m_zFile2 = _T("");
	m_zRegistry1 = _T("");
	m_zRegistry2 = _T("");
	//}}AFX_DATA_INIT
	// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}
//-------------------------------------------------------------------------------------------------
void CLicenseTimeInfoDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CLicenseTimeInfoDlg)
	DDX_Text(pDX, IDC_EDIT_F1, m_zFile1);
	DDX_Text(pDX, IDC_EDIT_F2, m_zFile2);
	DDX_Text(pDX, IDC_EDIT_R1, m_zRegistry1);
	DDX_Text(pDX, IDC_EDIT_R2, m_zRegistry2);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CLicenseTimeInfoDlg, CDialog)
	//{{AFX_MSG_MAP(CLicenseTimeInfoDlg)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_BN_CLICKED(IDC_BUTTON_READ, OnButtonRead)
	ON_BN_CLICKED(IDC_BUTTON_CLIPBOARD, OnButtonClipboard)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CLicenseTimeInfoDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CLicenseTimeInfoDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon
	
	// Setup Registry items
	ma_pRollbackCfgMgr = auto_ptr<IConfigurationSettingsPersistenceMgr>(
		new RegistryPersistenceMgr( HKEY_CURRENT_USER, "" ) );

	// Disable Copy to Clipboard at startup
	GetDlgItem( IDC_BUTTON_CLIPBOARD )->EnableWindow( FALSE );
	
	return TRUE;  // return TRUE  unless you set the focus to a control
}
//-------------------------------------------------------------------------------------------------
// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.
void CLicenseTimeInfoDlg::OnPaint() 
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
HCURSOR CLicenseTimeInfoDlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}
//-------------------------------------------------------------------------------------------------
void CLicenseTimeInfoDlg::OnButtonRead() 
{
	// Get the encrypted data from (local) DT file and registry
	bool	bLocalData1;
	bool	bRemoteData1;
	bool	bLocalData2;
	bool	bRemoteData2;
	string	strLocalData1;
	string	strRemoteData1;
	string	strLocalData2;
	string	strRemoteData2;

	// Protect the read accesses
	{
		CSingleLock lock( &m_semReadWrite, TRUE );

		strLocalData1 = getLocalDateTimeString(getDateTimeFilePath());
		strRemoteData1 = getRemoteDateTimeString(ITEM_SECTION_NAME1, LAST_TIME_USED);

		strLocalData2 = getLocalDateTimeString(getDateTimeFilePath() + ".old");
		strRemoteData2 = getRemoteDateTimeString(ITEM_SECTION_NAME2, LAST_TIME_USED);
	}

	// Check results of string retrieval
	bLocalData1 = (strLocalData1 == "") ? false : true;
	bRemoteData1 = (strRemoteData1 == "") ? false : true;
	bLocalData2 = (strLocalData2 == "") ? false : true;
	bRemoteData2 = (strRemoteData2 == "") ? false : true;

	///////////////////////////////////
	// Display primary values or errors
	///////////////////////////////////
	// Convert the encrypted Date-Time strings into CTime objects
	CString zLocal1;
	CString zRemote1;
	CString zLocal2;
	CString zRemote2;
	CTime	tmLocal;
	CTime	tmRemote;
	bool	bLocal = decryptDateTimeString( strLocalData1, getPassword1(), &tmLocal );
	bool	bRemote = decryptDateTimeString( strRemoteData1, getPassword2(), &tmRemote );

	// Display primary values or errors
	zLocal1.Format( "%ld", tmLocal );
	zRemote1.Format( "%ld", tmRemote );

	m_zFile1 = bLocal ? zLocal1 : 
		(bLocalData1 ? "<String Decryption Error>" : "<String Not Found or Read Error>");
	m_zRegistry1 = bRemote ? zRemote1 : 
		(bRemoteData1 ? "<String Decryption Error>" : "<String Not Found or Read Error>");

	///////////////////////////////////
	// Display backup values or errors
	///////////////////////////////////
	// Convert the encrypted Date-Time strings into CTime objects
	bLocal = decryptDateTimeString( strLocalData2, getPassword1(), &tmLocal );
	bRemote = decryptDateTimeString( strRemoteData2, getPassword2(), &tmRemote );

	zLocal2.Format( "%ld", tmLocal );
	zRemote2.Format( "%ld", tmRemote );

	m_zFile2 = bLocal ? zLocal2 : 
		(bLocalData2 ? "<String Decryption Error>" : "<String Not Found or Read Error>");
	m_zRegistry2 = bRemote ? zRemote2 : 
		(bRemoteData2 ? "<String Decryption Error>" : "<String Not Found or Read Error>");

	// Enable Copy to Clipboard after Read
	GetDlgItem( IDC_BUTTON_CLIPBOARD )->EnableWindow( TRUE );

	UpdateData( FALSE );
}
//-------------------------------------------------------------------------------------------------
void CLicenseTimeInfoDlg::OnButtonClipboard() 
{
	// Create string containing displayed results
	std::string strResults = string( "F1 = " );
	strResults += m_zFile1.operator LPCTSTR();
	strResults += string( "\r\n" );

	strResults += string( "F2 = " );
	strResults += m_zFile2.operator LPCTSTR();
	strResults += string( "\r\n" );

	strResults += string( "R1 = " );
	strResults += m_zRegistry1.operator LPCTSTR();
	strResults += string( "\r\n" );

	strResults += string( "R2 = " );
	strResults += m_zRegistry2.operator LPCTSTR();
	strResults += string( "\r\n" );

	// Create Clipboard Manager and send text to Clipboard
	ClipboardManager	cm( this );
	cm.writeText( strResults );
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
bool CLicenseTimeInfoDlg::decryptDateTimeString(std::string strEncryptedDT, 
												  const ByteStream& bsPassword, 
												  CTime* ptmResult)
{
	bool bReturn = true;

	try
	{
		try
		{
			// Decrypt the bytes
			ByteStream bsEncrypted( strEncryptedDT );
			ByteStream bsUnencrypted;
			EncryptionEngine ee;
			ee.decrypt( bsUnencrypted, bsEncrypted, bsPassword );

			// Extract CTime data from the bytes
			ByteStreamManipulator bsm( ByteStreamManipulator::kRead, bsUnencrypted );

			// Get first random unsigned short
			unsigned short usTemp1;
			bsm >> usTemp1;

			// Confirm divisibility by modulo constant
			unsigned short usExtra = usTemp1 % gusMODULO_CONSTANT;
			if (usExtra != 0)
			{
				bReturn = false;
			}

			// Retrieve CTime data
			CTime	tmTemp;
			bsm >> tmTemp;

			// Get second random unsigned short
			unsigned short usTemp2;
			bsm >> usTemp2;

			// Confirm divisibility by modulo constant
			usExtra = usTemp2 % gusMODULO_CONSTANT;
			if (usExtra != 0)
			{
				bReturn = false;
			}

			// Provide CTime to caller
			*ptmResult = tmTemp;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI12504")
	}
	catch(UCLIDException ue)
	{
#ifdef _DEBUG
		ue.log();
#endif
		bReturn = false;
	}

	return bReturn;
}
//-------------------------------------------------------------------------------------------------
std::string CLicenseTimeInfoDlg::getDateTimeFilePath() const
{
	string	strDTFile;

	// This try catch is just to give more trace information
	try
	{
		// Get path to special user-specific folder
		char pszDir[MAX_PATH];
		if (SUCCEEDED (SHGetSpecialFolderPath( NULL, pszDir, CSIDL_APPDATA, 0 ))) 
		{
			// Add path and filename for DT file
			strDTFile = string( pszDir ) + string( gpszDateTimeSubfolderFile );
		}
		else
		{
			// Create and throw exception
			UCLIDException ue( "ELI12506", "Unable to get path to Special folder" );
			ue.addDebugInfo( "Last Error", GetLastError() );
			throw ue;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI12505")

	// Return results
	return strDTFile;
}
//-------------------------------------------------------------------------------------------------
std::string CLicenseTimeInfoDlg::getLocalDateTimeString(std::string strFileName) const
{
	string	strData;

	// This try catch is just to give more trace information
	try
	{
		// Get path and filename for DT file
		string	strDTFile = strFileName;
		if (isFileOrFolderValid( strDTFile ))
		{
			// Read the string
			strData = getTextFileContentsAsString( strDTFile );
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI12507")

	// Return results
	return strData;
}
//-------------------------------------------------------------------------------------------------
const ByteStream& CLicenseTimeInfoDlg::getPassword1() const
{
	static ByteStream passwordBytes1;
	static bool bAlreadyInitialized1 = false;
	// This try catch is just to give more trace information
	try
	{
		if (!bAlreadyInitialized1)
		{
			// Create a 16 byte password from LMData constants
			passwordBytes1.setSize( 16 );
			unsigned char* pData = passwordBytes1.getData();
			pData[0]  = (unsigned char)(LOBYTE(LOWORD(gulUCLIDDateTimeKey1)));
			pData[1]  = (unsigned char)(HIBYTE(LOWORD(gulUCLIDDateTimeKey1)));
			pData[2]  = (unsigned char)(LOBYTE(HIWORD(gulUCLIDDateTimeKey1)));
			pData[3]  = (unsigned char)(HIBYTE(HIWORD(gulUCLIDDateTimeKey1)));
			
			pData[4]  = (unsigned char)(LOBYTE(LOWORD(gulUCLIDDateTimeKey2)));
			pData[5]  = (unsigned char)(HIBYTE(LOWORD(gulUCLIDDateTimeKey2)));
			pData[6]  = (unsigned char)(LOBYTE(HIWORD(gulUCLIDDateTimeKey2)));
			pData[7]  = (unsigned char)(HIBYTE(HIWORD(gulUCLIDDateTimeKey2)));
			
			pData[8]  = (unsigned char)(LOBYTE(LOWORD(gulUCLIDDateTimeKey3)));
			pData[9]  = (unsigned char)(HIBYTE(LOWORD(gulUCLIDDateTimeKey3)));
			pData[10]  = (unsigned char)(LOBYTE(HIWORD(gulUCLIDDateTimeKey3)));
			pData[11]  = (unsigned char)(HIBYTE(HIWORD(gulUCLIDDateTimeKey3)));
			
			pData[12]  = (unsigned char)(LOBYTE(LOWORD(gulUCLIDDateTimeKey4)));
			pData[13]  = (unsigned char)(HIBYTE(LOWORD(gulUCLIDDateTimeKey4)));
			pData[14]  = (unsigned char)(LOBYTE(HIWORD(gulUCLIDDateTimeKey4)));
			pData[15]  = (unsigned char)(HIBYTE(HIWORD(gulUCLIDDateTimeKey4)));

			// Set flag
			bAlreadyInitialized1 = true;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI12508")

	return passwordBytes1;
}
//--------------------------------------------------------------------------------------------------
const ByteStream& CLicenseTimeInfoDlg::getPassword2() const
{
	static ByteStream passwordBytes2;
	static bool bAlreadyInitialized2 = false;
	// This try catch is just to give more trace information
	try
	{
		if (!bAlreadyInitialized2)
		{
			// Create a 16 byte password from LMData constants
			passwordBytes2.setSize( 16 );
			unsigned char* pData = passwordBytes2.getData();
			pData[0]  = (unsigned char)(LOBYTE(LOWORD(gulUCLIDDateTimeKey5)));
			pData[1]  = (unsigned char)(HIBYTE(LOWORD(gulUCLIDDateTimeKey5)));
			pData[2]  = (unsigned char)(LOBYTE(HIWORD(gulUCLIDDateTimeKey5)));
			pData[3]  = (unsigned char)(HIBYTE(HIWORD(gulUCLIDDateTimeKey5)));

			pData[4]  = (unsigned char)(LOBYTE(LOWORD(gulUCLIDDateTimeKey6)));
			pData[5]  = (unsigned char)(HIBYTE(LOWORD(gulUCLIDDateTimeKey6)));
			pData[6]  = (unsigned char)(LOBYTE(HIWORD(gulUCLIDDateTimeKey6)));
			pData[7]  = (unsigned char)(HIBYTE(HIWORD(gulUCLIDDateTimeKey6)));
			
			pData[8]  = (unsigned char)(LOBYTE(LOWORD(gulUCLIDDateTimeKey7)));
			pData[9]  = (unsigned char)(HIBYTE(LOWORD(gulUCLIDDateTimeKey7)));
			pData[10]  = (unsigned char)(LOBYTE(HIWORD(gulUCLIDDateTimeKey7)));
			pData[11]  = (unsigned char)(HIBYTE(HIWORD(gulUCLIDDateTimeKey7)));
			
			pData[12]  = (unsigned char)(LOBYTE(LOWORD(gulUCLIDDateTimeKey8)));
			pData[13]  = (unsigned char)(HIBYTE(LOWORD(gulUCLIDDateTimeKey8)));
			pData[14]  = (unsigned char)(LOBYTE(HIWORD(gulUCLIDDateTimeKey8)));
			pData[15]  = (unsigned char)(HIBYTE(HIWORD(gulUCLIDDateTimeKey8)));

			// Set flag
			bAlreadyInitialized2 = true;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI12509")

	return passwordBytes2;
}
//-------------------------------------------------------------------------------------------------
std::string CLicenseTimeInfoDlg::getRemoteDateTimeString(std::string strPath, std::string strKey) const
{
	string	strData;

	// This try catch is just to give more trace information
	try
	{
		if (!ma_pRollbackCfgMgr->keyExists( strPath, strKey ))
		{
			ma_pRollbackCfgMgr->createKey( strPath, strKey, "" );
			return "";
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI12510")

	return ma_pRollbackCfgMgr->getKeyValue( strPath, strKey, "" );
}
//-------------------------------------------------------------------------------------------------
