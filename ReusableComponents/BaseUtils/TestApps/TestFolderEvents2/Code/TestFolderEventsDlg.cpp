// TestFolderEventsDlg.cpp : implementation file
//

#include "stdafx.h"
#include "TestFolderEvents.h"
#include "TestFolderEventsDlg.h"

#include <UCLIDException.hpp>
#include <XBrowseForFolder.h>
#include <cpputil.hpp>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Global
//-------------------------------------------------------------------------------------------------
const unsigned long gulFOLDER_LISTENER_BUF_SIZE	 = 65536;

//-------------------------------------------------------------------------------------------------
// Thread that listens for file changes
//-------------------------------------------------------------------------------------------------
UINT asyncListeningThread(LPVOID pData)
{
	try
	{
		// Create a local dialog object from the data parameter
		CTestFolderEventsDlg *pDlg = (CTestFolderEventsDlg *) pData;

		// Begin listening
		pDlg->startListening();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI12772")

	return 0;
}

//-------------------------------------------------------------------------------------------------
// CTestFolderEventsDlg dialog
//-------------------------------------------------------------------------------------------------
CTestFolderEventsDlg::CTestFolderEventsDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CTestFolderEventsDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(CTestFolderEventsDlg)
	m_zFolder = _T("");
	m_bRecursive = TRUE;
	//}}AFX_DATA_INIT
	// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);

	m_pListenerThread = NULL;
}
//-------------------------------------------------------------------------------------------------
CTestFolderEventsDlg::~CTestFolderEventsDlg()
{
	// Just wait for logging thread to finish writing
	Sleep(1000);
}
//-------------------------------------------------------------------------------------------------
void CTestFolderEventsDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CTestFolderEventsDlg)
	DDX_Text(pDX, IDC_EDIT_FOLDER, m_zFolder);
	DDX_Check(pDX, IDC_CHECK_RECURSIVE, m_bRecursive);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CTestFolderEventsDlg, CDialog)
	//{{AFX_MSG_MAP(CTestFolderEventsDlg)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_BN_CLICKED(IDC_BUTTON_BROWSE, OnButtonBrowse)
	ON_BN_CLICKED(IDC_BUTTON_START, OnButtonStart)
	ON_BN_CLICKED(IDC_CHECK_RECURSIVE, OnCheckRecursive)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CTestFolderEventsDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CTestFolderEventsDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon
	
	// TODO: Add extra initialization here
	
	return TRUE;  // return TRUE  unless you set the focus to a control
}
//-------------------------------------------------------------------------------------------------
// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.
void CTestFolderEventsDlg::OnPaint() 
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
HCURSOR CTestFolderEventsDlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}
//-------------------------------------------------------------------------------------------------
void CTestFolderEventsDlg::OnCancel() 
{
	try
	{
		// Stop listening for files
		if (m_pListenerThread != NULL)
		{
		}

		// Log the finish
		getLogFile()->writeLine( "Dlg::OnCancel() - Stopped listening for files" );

		CDialog::OnCancel();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14805");
}
//-------------------------------------------------------------------------------------------------
void CTestFolderEventsDlg::OnButtonBrowse() 
{
	try
	{
		CString zFolder;
		char pszPath[MAX_PATH + 1];
		if (XBrowseForFolder( m_hWnd, zFolder, pszPath, sizeof(pszPath) ))
		{
			// Retain the setting
			m_zFolder = pszPath;
			UpdateData( FALSE );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14806");
}
//-------------------------------------------------------------------------------------------------
void CTestFolderEventsDlg::OnButtonStart() 
{
	try
	{
		// Confirm that a folder has been specified
		UpdateData( TRUE );
		if (m_zFolder.IsEmpty())
		{
			UCLIDException ue( "ELI14807", "A folder must be specified before listening can be started!" );
			throw ue;
		}

		// Get list of files currently found in the specified folder
		vector<string>	vecFiles;
		getFilesInDir( vecFiles, m_zFolder.operator LPCTSTR() );

		// Log the folder name
		CString zFolder;
		zFolder.Format( "Dlg::OnButtonStart(): Folder = %s", m_zFolder );
		getLogFile()->writeLine( zFolder.operator LPCTSTR() );

		// Log the recursion setting
		if (m_bRecursive)
		{
			zFolder.Format( "Dlg::OnButtonStart(): Recursion is ON" );
		}
		else
		{
			zFolder.Format( "Dlg::OnButtonStart(): Recursion is OFF" );
		}
		getLogFile()->writeLine( zFolder.operator LPCTSTR() );

		// Log the file count
		unsigned int uiCount = vecFiles.size();
		
		CString zText;
		zText.Format( "Dlg::OnButtonStart(): %d files present in main folder", uiCount );
		getLogFile()->writeLine( zText.operator LPCTSTR() );

		if (uiCount < 100)
		{
			// Log each filename
			for (unsigned int ui = 0; ui < uiCount; ui++)
			{
				CString zText;
				zText.Format( "Dlg::OnButtonStart(): File = %s", vecFiles[ui].c_str() );
				getLogFile()->writeLine( zText.operator LPCTSTR() );
			}
		}

		// Spawn separate thread to do the listening
		m_pListenerThread = AfxBeginThread( asyncListeningThread, this );

		/*
		// Get a handle to the listening folder
		HANDLE hDir = CreateFile( m_zFolder.operator LPCTSTR(), 
			FILE_LIST_DIRECTORY,
			FILE_SHARE_READ | FILE_SHARE_DELETE | FILE_SHARE_WRITE,
			NULL,
			OPEN_EXISTING,
			FILE_FLAG_BACKUP_SEMANTICS,
			NULL );

		// Create buffer for returned filenames
		DWORD dwBytesReturned;
		LPBYTE lpBuffer = new BYTE[gulFOLDER_LISTENER_BUF_SIZE];

		// Listen for directory changes and process each item
		DWORD ret = 0;
		while( ::ReadDirectoryChangesW( hDir, 
			(LPVOID)lpBuffer, 
			gulFOLDER_LISTENER_BUF_SIZE, 
			m_bRecursive, 
			FILE_NOTIFY_CHANGE_FILE_NAME,
			&dwBytesReturned, NULL, NULL) )
		{
			// Start processing at beginning of buffer
			LPBYTE pCurrByte = lpBuffer;
			PFILE_NOTIFY_INFORMATION pFileInfo = NULL;

			// Protect access
//			Win32SemaphoreLockGuard lg(pTD->m_semFolderListen);

			// Rename events happen in two pieces, rename old and rename new
			// when a rename old event happens we keep track of it so that we 
			// can have one event handler for rename that takes the old and new
			// names
//			string strOldFilename = "";

			// Iterate through the buffer extracting the information that we need
			do
			{
				// Extract info for this file
				pFileInfo = (PFILE_NOTIFY_INFORMATION)pCurrByte;

				// Get the name of the affected file
				int len = WideCharToMultiByte( CP_ACP, 0, pFileInfo->FileName, 
					pFileInfo->FileNameLength / sizeof(WCHAR), 0, 0, 0, 0 );
				LPSTR result = new char[len+1];
				WideCharToMultiByte( CP_ACP, 0, pFileInfo->FileName, 
					pFileInfo->FileNameLength / sizeof(WCHAR), result, len, 0, 0 );
				result[len] = '\0';

				// Create full path to file
				string strFilename = removeLastSlashFromPath( m_zFolder.operator LPCTSTR() ) + 
					"\\" + result;

				// Handle each event by adding statement to logfile
				CString	zText;
				switch (pFileInfo->Action)
				{
				case FILE_ACTION_ADDED:
					{
						zText.Format( "Dlg - FILE_ACTION_ADDED: %s", result );
						getLogFile()->writeLine( zText.operator LPCTSTR() );
					}
					break;
				case FILE_ACTION_REMOVED:
					{
						zText.Format( "Dlg - FILE_ACTION_REMOVED: %s", result );
						getLogFile()->writeLine( zText.operator LPCTSTR() );
					}
					break;
				case FILE_ACTION_MODIFIED:
					{
						zText.Format( "Dlg - FILE_ACTION_MODIFIED: %s", result );
						getLogFile()->writeLine( zText.operator LPCTSTR() );
					}
					break;
				case FILE_ACTION_RENAMED_OLD_NAME:
					{
//						strOldFilename = strFilename;
						zText.Format( "Dlg - FILE_ACTION_RENAMED_OLD_NAME: %s", result );
						getLogFile()->writeLine( zText.operator LPCTSTR() );
					}
					break;
				case FILE_ACTION_RENAMED_NEW_NAME:
					{
						zText.Format( "Dlg - FILE_ACTION_RENAMED_NEW_NAME: %s", result );
						getLogFile()->writeLine( zText.operator LPCTSTR() );
					}
					break;
				default:
					break;
				}

				// Clean up local filename buffer
				delete result;

				// Advance to the next file returned
				pCurrByte += pFileInfo->NextEntryOffset;

			}	// end while each file
			while(pFileInfo->NextEntryOffset != 0);

			// Brief pause to let the UI catch up
			Invalidate();
			UpdateWindow();
//			Sleep( 10 );

		}		// end while listening for directory changes
		*/
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14808");
}
//-------------------------------------------------------------------------------------------------
void CTestFolderEventsDlg::OnCheckRecursive() 
{
	try
	{
		UpdateData( TRUE );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14809");
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
ThreadSafeLogFile* CTestFolderEventsDlg::getLogFile()
{
	if (m_apLogFile.get() == NULL)
	{
		m_apLogFile = auto_ptr<ThreadSafeLogFile>( new ThreadSafeLogFile(getLogFileName()) );
		ASSERT_RESOURCE_ALLOCATION("ELI14810", m_apLogFile.get() != NULL);
	}

	return m_apLogFile.get();
}
//-------------------------------------------------------------------------------------------------
const string& CTestFolderEventsDlg::getLogFileName() const
{
	// Location of the log is relative to the location of this EXE
	static string strLogFile;
	if (strLogFile.empty())
	{
		// Ensure access to the CWinApp object
		CWinApp *pApp = AfxGetApp();
		ASSERT_ARGUMENT("ELI14811", pApp != NULL);

		// Get folder of this application
		string strModuleDirectory = getModuleDirectory(pApp->m_hInstance);

		// Compute the name of the log file
		strLogFile = strModuleDirectory + string("\\LogFiles\\TestListener2.log");
	}

	return strLogFile;
}
//-------------------------------------------------------------------------------------------------
void CTestFolderEventsDlg::startListening()
{
	bool bContinue = true;
	bool bCleanupThreadRelatedObjects = false;

	// Get a handle to the listening folder
	HANDLE hDir = CreateFile( m_zFolder.operator LPCTSTR(), 
		FILE_LIST_DIRECTORY,
		FILE_SHARE_READ | FILE_SHARE_DELETE | FILE_SHARE_WRITE,
		NULL,
		OPEN_EXISTING,
		FILE_FLAG_BACKUP_SEMANTICS,
		NULL );

	// Create buffer for returned filenames
	DWORD dwBytesReturned;
	LPBYTE lpBuffer = new BYTE[gulFOLDER_LISTENER_BUF_SIZE];

	// Listen for directory changes and process each item
	DWORD ret = 0;
	while( ::ReadDirectoryChangesW( hDir, 
		(LPVOID)lpBuffer, 
		gulFOLDER_LISTENER_BUF_SIZE, 
		m_bRecursive, 
		FILE_NOTIFY_CHANGE_FILE_NAME,
		&dwBytesReturned, NULL, NULL) )
	{
		// Start processing at beginning of buffer
		LPBYTE pCurrByte = lpBuffer;
		PFILE_NOTIFY_INFORMATION pFileInfo = NULL;

		// Protect access
//		Win32SemaphoreLockGuard lg(pTD->m_semFolderListen);

		// Rename events happen in two pieces, rename old and rename new
		// when a rename old event happens we keep track of it so that we 
		// can have one event handler for rename that takes the old and new
		// names
//		string strOldFilename = "";

		// Iterate through the buffer extracting the information that we need
		do
		{
			// Extract info for this file
			pFileInfo = (PFILE_NOTIFY_INFORMATION)pCurrByte;

			// Get the name of the affected file
			int len = WideCharToMultiByte( CP_ACP, 0, pFileInfo->FileName, 
				pFileInfo->FileNameLength / sizeof(WCHAR), 0, 0, 0, 0 );
			LPSTR result = new char[len+1];
			WideCharToMultiByte( CP_ACP, 0, pFileInfo->FileName, 
				pFileInfo->FileNameLength / sizeof(WCHAR), result, len, 0, 0 );
			result[len] = '\0';

			// Create full path to file
			string strFilename = removeLastSlashFromPath( m_zFolder.operator LPCTSTR() ) + 
				"\\" + result;

			// Handle each event by adding statement to logfile
			CString	zText;
			switch (pFileInfo->Action)
			{
			case FILE_ACTION_ADDED:
				{
					zText.Format( "Dlg - FILE_ACTION_ADDED: %s", result );
					getLogFile()->writeLine( zText.operator LPCTSTR() );
				}
				break;
			case FILE_ACTION_REMOVED:
				{
					zText.Format( "Dlg - FILE_ACTION_REMOVED: %s", result );
					getLogFile()->writeLine( zText.operator LPCTSTR() );
				}
				break;
			case FILE_ACTION_MODIFIED:
				{
					zText.Format( "Dlg - FILE_ACTION_MODIFIED: %s", result );
					getLogFile()->writeLine( zText.operator LPCTSTR() );
				}
				break;
			case FILE_ACTION_RENAMED_OLD_NAME:
				{
//					strOldFilename = strFilename;
					zText.Format( "Dlg - FILE_ACTION_RENAMED_OLD_NAME: %s", result );
					getLogFile()->writeLine( zText.operator LPCTSTR() );
				}
				break;
			case FILE_ACTION_RENAMED_NEW_NAME:
				{
					zText.Format( "Dlg - FILE_ACTION_RENAMED_NEW_NAME: %s", result );
					getLogFile()->writeLine( zText.operator LPCTSTR() );
				}
				break;
			default:
				break;
			}

			// Clean up local filename buffer
			delete result;

			// Advance to the next file returned
			pCurrByte += pFileInfo->NextEntryOffset;

		}	// end while each file
		while(pFileInfo->NextEntryOffset != 0);

	}		// end while listening for directory changes

	DWORD dwError = GetLastError();
	if (dwError != 0)
	{
		// Convert the error code to an error string
		long nError = dwError;
		string strError = getWindowsErrorString( nError );
		strError += string( " from ReadDirectoryChanges()!" );

		// Convert carriage-return & linefeed into characters
		convertCppStringToNormalString( strError );

		// Log the error
		getLogFile()->writeLine( strError.c_str() );
	}
}
//-------------------------------------------------------------------------------------------------
