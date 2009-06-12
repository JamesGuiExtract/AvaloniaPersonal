// CopyNewerFilesDlg.cpp : implementation file
//

#include "stdafx.h"
#include "CopyNewerFiles.h"
#include "CopyNewerFilesDlg.h"

#include <XBrowseForFolder.h>
#include <io.h>
#include <stdio.h>
#include <stdlib.h>
#include <UCLIDException.h>
#include <cpputil.h>


#include <string>
using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// CCopyNewerFilesDlg
//-------------------------------------------------------------------------------------------------
CCopyNewerFilesDlg::CCopyNewerFilesDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CCopyNewerFilesDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(CCopyNewerFilesDlg)
	m_bPrompt = FALSE;
	m_zDestination = _T("");
	m_zSource = _T("");
	//}}AFX_DATA_INIT
	// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);

	// Clear flags
	m_bNoLogFile = false;
	m_bLogFileOpen = false;
}
//-------------------------------------------------------------------------------------------------
void CCopyNewerFilesDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CCopyNewerFilesDlg)
	DDX_Control(pDX, IDC_EDIT_FOLDER, m_editFolder);
	DDX_Control(pDX, IDC_BUTTON_COPY, m_btnCopy);
	DDX_Check(pDX, IDC_CHECK_PROMPT, m_bPrompt);
	DDX_Text(pDX, IDC_EDIT_DESTINATION, m_zDestination);
	DDX_Text(pDX, IDC_EDIT_SOURCE, m_zSource);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CCopyNewerFilesDlg, CDialog)
	//{{AFX_MSG_MAP(CCopyNewerFilesDlg)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_BN_CLICKED(IDC_BUTTON_COPY, OnButtonCopy)
	ON_BN_CLICKED(IDC_BUTTON_DESTINATION, OnButtonDestination)
	ON_BN_CLICKED(IDC_BUTTON_SOURCE, OnButtonSource)
	ON_BN_CLICKED(IDC_CHECK_PROMPT, OnCheckPrompt)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CCopyNewerFilesDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CCopyNewerFilesDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon

	// Default Copy button to disabled
	m_btnCopy.EnableWindow( FALSE );

	return TRUE;  // return TRUE  unless you set the focus to a control
}
//-------------------------------------------------------------------------------------------------
// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.
void CCopyNewerFilesDlg::OnPaint() 
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
HCURSOR CCopyNewerFilesDlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}
//-------------------------------------------------------------------------------------------------
void CCopyNewerFilesDlg::OnButtonCopy() 
{
	try
	{
		// Check for defined folders
		if (!m_zSource.IsEmpty() && !m_zDestination.IsEmpty())
		{
			// Set wait cursorz
			CWaitCursor	wait;

			// Open the log file
			openLogFile();

			// Reset counters
			m_lCountSameTime = 0;
			m_lCountSourceOlder = 0;
			m_lCountNoSourceRead = 0;
			m_lCountNoSourceWriteTime = 0;
			m_lCountNoDestinationWriteTime = 0;
			m_lCountNoDestinationRead = 0;
			m_lCountDestinationTooLong = 0;
			m_lCountCopyFailure = 0;
			m_lCountCopySuccess = 0;

			// Set the initial folder text
			setFolderText( m_zDestination.operator LPCTSTR() );

			// Add initial status entries
			CString	zStatus;
			updateStatus( "* * * * * * * * * * * * * * * * * * * * * * * * *" );
			zStatus.Format( "Start time = %s", (getTimeAsString()).c_str() );
			updateStatus( zStatus.operator LPCTSTR() );
			zStatus.Format( "Copying from source: \"%s\"", m_zSource );
			updateStatus( zStatus.operator LPCTSTR() );
			zStatus.Format( "Copying to destination: \"%s\"\r\n", m_zDestination );
			updateStatus( zStatus.operator LPCTSTR() );

			// Copy files from source folder to destination folder
			doCopy( m_zSource.operator LPCTSTR(), m_zDestination.operator LPCTSTR() );

			// Prepare user message (and last status message)
			zStatus.Format( "Copying is complete.\r\n\r\n"
				"Files with same time: %d\r\n"
				"Files with older source: %d\r\n"
				"Source files unable to read file: %d\r\n"
				"Source files unable to read last write time: %d\r\n"
				"Destination files unable to read file: %d\r\n"
				"Destination files unable to read last write time: %d\r\n"
				"Destination filenames too long: %d\r\n"
				"Copy file failure: %d\r\n"
				"Copy file success: %d\r\n", 
				m_lCountSameTime, m_lCountSourceOlder, m_lCountNoSourceRead, 
				m_lCountNoSourceWriteTime, m_lCountNoDestinationRead, 
				m_lCountNoDestinationWriteTime, m_lCountDestinationTooLong, 
				m_lCountCopyFailure, m_lCountCopySuccess );

			// Add final entries to log file
			updateStatus( zStatus.operator LPCTSTR() );
			updateStatus( "* * * * * * * * * * * * * * * * * * * * * * * * *\r\n\r\n" );

			// Close the Log file
			m_fLogFile.Close();
			m_bLogFileOpen = false;

			// Display user message
			MessageBox( zStatus, "Copying Completed" );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06857")
}
//-------------------------------------------------------------------------------------------------
void CCopyNewerFilesDlg::OnButtonDestination() 
{
	try
	{
		char pszPath[MAX_PATH + 1];
		if (XBrowseForFolder( m_hWnd, m_zDestination, pszPath, sizeof(pszPath) ))
		{
			// Store folder
			m_zDestination = pszPath;
			UpdateData( FALSE );

			// Check for enable of Copy button
			if (!m_zSource.IsEmpty())
			{
				// Enable Copy button
				m_btnCopy.EnableWindow( TRUE );
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06858")
}
//-------------------------------------------------------------------------------------------------
void CCopyNewerFilesDlg::OnButtonSource() 
{
	try
	{
		char pszPath[MAX_PATH + 1];
		if (XBrowseForFolder( m_hWnd, m_zSource, pszPath, sizeof(pszPath) ))
		{
			// Store folder
			m_zSource = pszPath;
			UpdateData( FALSE );

			// Check for enable of Copy button
			if (!m_zDestination.IsEmpty())
			{
				// Enable Copy button
				m_btnCopy.EnableWindow( TRUE );
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06859")
}
//-------------------------------------------------------------------------------------------------
void CCopyNewerFilesDlg::OnCheckPrompt() 
{
	UpdateData( TRUE );
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CCopyNewerFilesDlg::doCopy(std::string strSourceFolder, std::string strDestinationFolder) 
{
	CString	zStatus;
	bool	bForcedOverwrite = false;

	// Append file information to directory
	string			strPath = strSourceFolder;
	strPath += "\\*.*";

	// Step through ALL files in source folder
	CFileFind	ffSource;
	if (ffSource.FindFile( strPath.c_str() ))
	{
		// Find the next file
		BOOL	bMoreFiles = ffSource.FindNextFile();
		while (true)
		{
			// Check for dots
			if (ffSource.IsDots())
			{
				if (bMoreFiles == 0)
				{
					// No more files
					return;
				}
				else
				{
					// Find the next file
					bMoreFiles = ffSource.FindNextFile();
					continue;
				}
			}

			// Get full path to source file
			string	strSource = strSourceFolder;
			strSource += "\\";
			strSource += (ffSource.GetFileName()).operator LPCTSTR();

			// Construct path to this file in Destination folder
			string		strDestination = strDestinationFolder;
			strDestination += "\\";
			strDestination += (ffSource.GetFileName()).operator LPCTSTR();

			// Check for directory
			if (ffSource.IsDirectory())
			{
				// Check for folder existence in destination
				if (_access( strDestination.c_str(), 0 ) == -1)
				{
					// First create the folder
					if (CreateDirectory( strDestination.c_str(), NULL ) == 0)
					{
						// Unable to create directory
						DWORD dwError = GetLastError();
						zStatus.Format( "Failed to create directory: \"%s\" Error = %d", 
							strDestination.c_str(), dwError );
						updateStatus( zStatus.operator LPCTSTR() );
					}
					else
					{
						zStatus.Format( "Created folder: \"%s\"", strDestination.c_str() );
						updateStatus( zStatus.operator LPCTSTR() );
					}
				}

				// Copy the subdirectory
				doCopy( strSource, strDestination );

				// Update the current folder and move cursor to end of text
				setFolderText( strDestination.c_str() );

				// Move on to the next file/folder
				if (bMoreFiles == 0)
				{
					// No more files
					return;
				}
				else
				{
					// Find the next file
					bMoreFiles = ffSource.FindNextFile();
					continue;
				}
			}

			// Check for filename too long
			int iLength = strDestination.length();
			if (iLength > 255)
			{
				// Add status message
				DWORD dwError = GetLastError();
				zStatus.Format( "Skipping destination filename \"%s\" too long (%d)", 
					strDestination.c_str(), iLength );
				updateStatus( zStatus.operator LPCTSTR() );

				// Update counter
				m_lCountDestinationTooLong++;

				// Source file unavailable, just skip it
				if (bMoreFiles == 0)
				{
					// No more files
					return;
				}
				else
				{
					// Find the next file
					bMoreFiles = ffSource.FindNextFile();
					continue;
				}
			}

			//////////////////////////////////
			// Attempt to read source file and 
			// just skip over any corrupt file
			//////////////////////////////////
			try
			{
			   CFile f( strSource.c_str(), CFile::modeRead );
			}
			catch( ... )
			{
				// Add status message
				DWORD dwError = GetLastError();
				zStatus.Format( "Unable to read source file: \"%s\".  Error = %d", 
					strSource.c_str(), dwError );
				updateStatus( zStatus.operator LPCTSTR() );

				// Update counter
				m_lCountNoSourceRead++;

				// Source file cannot be read, just skip it
				if (bMoreFiles == 0)
				{
					// No more files
					return;
				}
				else
				{
					// Find the next file
					bMoreFiles = ffSource.FindNextFile();
					continue;
				}
			}

			// Get the last write time
			FILETIME	ftSourceWrite;
			if (ffSource.GetLastWriteTime( &ftSourceWrite ) == 0)
			{
				// Unexpected error, add status message
				zStatus.Format( "Unable to get last write time of source file: \"%s\"", 
					strSource.c_str() );
				updateStatus( zStatus.operator LPCTSTR() );

				// Update counter
				m_lCountNoSourceWriteTime++;

				// Problem with source file, just skip it
				if (bMoreFiles == 0)
				{
					// No more files
					return;
				}
				else
				{
					// Find the next file
					bMoreFiles = ffSource.FindNextFile();
					continue;
				}
			}

			// Use FileFind object to locate this file
			CFileFind	ffDestination;
			if (ffDestination.FindFile( strDestination.c_str() ) != 0)
			{
				////////////////////////////////////////////////////////////
				// Check to see if existing destination file can be read and 
				// force an overwrite of the existing file if it is corrupt
				////////////////////////////////////////////////////////////
				try
				{
				   CFile f( strDestination.c_str(), CFile::modeRead );
				}
				catch( ... )
				{
					// Add status message
					DWORD dwError = GetLastError();
					zStatus.Format( "Unable to read destination file: \"%s\".  Error = %d", 
						strDestination.c_str(), dwError );
					updateStatus( zStatus.operator LPCTSTR() );

					// Update counter
					m_lCountNoDestinationRead++;

					// Existing file cannot be read, force an overwrite
					bForcedOverwrite = true;
					goto copyFileAnyway;
				}

				// Retrieve last write time from destination file
				ffDestination.FindNextFile();
				FILETIME	ftDestinationWrite;
				bool		bTimeOK = true;
				if (ffDestination.GetLastWriteTime( &ftDestinationWrite ) == 0)
				{
					// Unexpected error, add status message
					zStatus.Format( "Unable to get last write time of destination file: \"%s\"", 
						strDestination.c_str() );
					updateStatus( zStatus.operator LPCTSTR() );

					// Update counter
					m_lCountNoDestinationWriteTime++;

					// Set flag
					bTimeOK = false;
				}

				// Compare write times
				CTime	tmSource( ftSourceWrite );
				CTime	tmDestination( ftDestinationWrite );
				if ((tmSource > tmDestination) || !bTimeOK)
				{
					// Copy the file from Source to Destination
					try
					{
						if (m_bPrompt)
						{
							// Get time strings
							CString	zTimeSrc;
							CString	zTimeDest;
							zTimeSrc = tmSource.Format( "%B %d, %Y -- %H:%M:%S %p" );
							if (bTimeOK)
							{
								zTimeDest = tmDestination.Format( "%B %d, %Y -- %H:%M:%S %p" );
							}
							else
							{
								zTimeDest = "Unknown time";
							}

							// Provide message box
							CString	zPrompt;
							zPrompt.Format( "Source File: %s\r\nDate: %s\r\n\r\nDestination File: %s\r\nDate: %s", 
								strSource.c_str(), zTimeSrc, 
								strDestination.c_str(), zTimeDest );

							if (MessageBox( zPrompt, "Overwrite this file?", MB_ICONQUESTION | MB_YESNO ) == IDNO)
							{
								// Add status item
								zStatus.Format( "User declined to overwrite file: \"%s\"", 
									strDestination.c_str() );
								updateStatus( zStatus.operator LPCTSTR() );

								if (bMoreFiles == 0)
								{
									// No more files
									return;
								}
								else
								{
									// Find the next file
									bMoreFiles = ffSource.FindNextFile();
									continue;
								}
							}
						}

						// Attempt to overwrite the file
						if (CopyFile( strSource.c_str(), strDestination.c_str(), FALSE ) == 0)
						{
							// Error
							DWORD dwError = GetLastError();
							zStatus.Format( "Failed to overwrite file: \"%s\".  Error = %d", 
								strSource.c_str(), dwError );
							updateStatus( zStatus.operator LPCTSTR() );

							// Update counter
							m_lCountCopyFailure++;
						}
						else
						{
							// Wait for the file to be readable
							waitForFileAccess(strDestination, giMODE_READ_ONLY);

							// Update the status
							zStatus.Format( "Overwrote file: \"%s\"", strSource.c_str() );
							updateStatus( zStatus.operator LPCTSTR() );

							// Update counter
							m_lCountCopySuccess++;
						}
					}
					catch (...)
					{
						// Just ignore any exception and continue
					}
				}			// end if tmSource > tmDestination
				else if (tmSource == tmDestination)
				{
					// Source file has same timestamp
					zStatus.Format( "No overwrite - destination file: \"%s\" has same timestamp", strDestination.c_str() );
					updateStatus( zStatus.operator LPCTSTR() );

					// Update counter
					m_lCountSameTime++;
				}
				else
				{
					// Source file is older
					zStatus.Format( "No overwrite - destination file: \"%s\" is newer", strDestination.c_str() );
					updateStatus( zStatus.operator LPCTSTR() );

					// Update counter
					m_lCountSourceOlder++;
				}
			}
			// File does not exist in Destination folder, just copy it
			else
			{
copyFileAnyway:
				try
				{
					// Attempt to copy the file
					if (CopyFile( strSource.c_str(), strDestination.c_str(), FALSE ) == 0)
					{
						// Error
						DWORD dwError = GetLastError();
						if (bForcedOverwrite)
						{
							zStatus.Format( "Failed to force an overwrite of file: \"%s\".  Error = %d", 
								strSource.c_str(), dwError );

							// Reset flag
							bForcedOverwrite = false;
						}
						else
						{
							zStatus.Format( "Failed to copy file: \"%s\".  Error = %d", 
								strSource.c_str(), dwError );
						}
						updateStatus( zStatus.operator LPCTSTR() );

						// Update counter
						m_lCountCopyFailure++;
					}
					else
					{
						// Wait for the file to be readable
						waitForFileAccess(strDestination, giMODE_READ_ONLY);

						if (bForcedOverwrite)
						{
							zStatus.Format( "Forced overwrite of file: \"%s\"", 
								strDestination.c_str() );

							// Reset flag
							bForcedOverwrite = false;
						}
						else
						{
							zStatus.Format( "Copied file: \"%s\"", strSource.c_str() );
						}
						updateStatus( zStatus.operator LPCTSTR() );

						// Update counter
						m_lCountCopySuccess++;
					}
				}
				catch (...)
				{
					// Just ignore any exception and continue
				}
			}			// end else file not present in Destination folder
						// or forcibly overwrite the existing file

			// Was this the last file
			if (bMoreFiles == 0)
			{
				return;
			}

			// Find the next file
			bMoreFiles = ffSource.FindNextFile();
		}				// end while FindNextFile()
	}					// end if FindFile()
}
//-------------------------------------------------------------------------------------------------
void CCopyNewerFilesDlg::openLogFile()
{
	// Open the file if not already open
	if (!m_bLogFileOpen)
	{
		if (!m_fLogFile.Open( "C:\\CopyLog.txt", 
			CFile::modeCreate | CFile::modeNoTruncate | CFile::modeWrite ))
		{
			// Set error flag
			m_bNoLogFile = true;

			// Tell user
			MessageBox( "Unable to open Log File", "Error" );
		}
		else
		{
			m_bLogFileOpen = true;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CCopyNewerFilesDlg::setFolderText(std::string strFolder)
{
	// Update the text
	m_editFolder.SetWindowText( strFolder.c_str() );

	// Advance to the end of the text
	m_editFolder.SetFocus();
	m_editFolder.SetSel( strFolder.length(), -1, FALSE );

	// Update the display
	UpdateData( FALSE );
	Invalidate();
	UpdateWindow();
}
//-------------------------------------------------------------------------------------------------
void CCopyNewerFilesDlg::updateStatus(std::string strStatus) 
{
	// Just return if Log file could not be opened
	if (m_bNoLogFile)
	{
		return;
	}

	// Append a carriage return
	CString	zLine = strStatus.c_str();
	zLine += "\r\n";

	// Write the line to the end of the output file
	m_fLogFile.SeekToEnd();
	m_fLogFile.WriteString( zLine.operator LPCTSTR() );
}
//-------------------------------------------------------------------------------------------------
