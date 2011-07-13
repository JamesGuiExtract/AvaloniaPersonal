//==================================================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	UEXViewer.cpp
//
// PURPOSE:	Provide a UI for UCLID Exception files.
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//==================================================================================================

#include "stdafx.h"
#include "UEXViewer.h"
#include "UEXViewerDlg.h"

#include <cpputil.h>
#include <UCLIDExceptionDlg.h>
#include <StringTokenizer.h>
#include <Win32Util.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const char *gpszUEXFileDescription = "Extract Systems UEX File";
const char *gpszUEXFileExtension = ".uex";

//-------------------------------------------------------------------------------------------------
// PURPOSE: Creates a version number string for the specified EXE file.
// REQUIRE: Nothing
// PROMISE: None.
// ARGS:	strPath - Full path to EXE file 
//-------------------------------------------------------------------------------------------------
string getVersion(string strPath)
{
	string strVersionNumber;

	// Create and populate structure for version data information
	DWORD	dwHandle;
	UINT	uiDataSize;
	LPVOID	lpData;
	DWORD	dwSize;
	LPVOID	lpBuffer;
	char	Data[80];

	LPTSTR	lpszImageName;
	lpszImageName = (char *)strPath.c_str();

	dwHandle = 0; 
	lpData = (void *)(&Data);

	// Get the version information block size,
	dwSize = ::GetFileVersionInfoSize( lpszImageName, &dwHandle );
	if (dwSize > 0)
	{
		// Allocate a storage buffer.
		lpBuffer = malloc( dwSize );

		// Get the version information block
		GetFileVersionInfo( lpszImageName, 0, dwSize, lpBuffer );

		// Use the version information block to obtain the version number.
		VerQueryValue( lpBuffer,
			TEXT("\\StringFileInfo\\040904B0\\FileVersion"),
			&lpData, &uiDataSize );
		
		// Replace commas with periods
		strVersionNumber = (char *)lpData;
		replaceVariable( strVersionNumber, ", ", "." );

		// Free the buffer
		free( lpBuffer );
	}

	return strVersionNumber;
}
//-------------------------------------------------------------------------------------------------
// PURPOSE: Displays supported command-line options to the user in a message box
// REQUIRE: Nothing
// PROMISE: None
// ARGS:	None
//-------------------------------------------------------------------------------------------------
void usage()
{
	string strUsage =	"Usage: UEXViewer.exe [OPTIONS]\n"
						"OPTIONS:\n"
						"/r - register .uex files to open by default with UEXViewer.exe then exit\n"
						"/u - unregister .uex files to not open with UEXViewer.exe then exit\n"
						"/? - display usage information\n"
						"<filename> [/temp] - open UEXViewer.exe with the specified file\n";
						"	/temp - The .uex file is temporary; delete it when the application is closed";
	AfxMessageBox(strUsage.c_str());
}
//-------------------------------------------------------------------------------------------------
// PURPOSE: To display the UEX Viewer dialog
void openUexDialog(CWnd* pMainWnd, string strFileName = "", bool showFileName = true)
{
	// Create and run the main dialog window
	CUEXViewerDlg dlg(NULL, strFileName, showFileName);
	pMainWnd = &dlg;
	dlg.DoModal();
}

//-------------------------------------------------------------------------------------------------
// CUEXViewerApp
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CUEXViewerApp, CWinApp)
	ON_COMMAND(ID_HELP, CWinApp::OnHelp)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CUEXViewerApp construction
//-------------------------------------------------------------------------------------------------
CUEXViewerApp::CUEXViewerApp()
:	m_strTemporaryFile("")
{
	// Place all significant initialization in InitInstance
}

//-------------------------------------------------------------------------------------------------
// The one and only CUEXViewerApp object
//-------------------------------------------------------------------------------------------------
CUEXViewerApp theApp;

//-------------------------------------------------------------------------------------------------
// CUEXViewerApp initialization
//-------------------------------------------------------------------------------------------------
BOOL CUEXViewerApp::InitInstance()
{
	try
	{
		AfxEnableControlContainer();

		// Get version number as a string
		string strVersion = getVersion(__argv[0]);

		// Set application name and version information
		string	strApp = AfxGetAppName();
		strApp += " ";
		strApp += strVersion;
		UCLIDException::setApplication( strApp );

		// Set default exception handler
		static UCLIDExceptionDlg ueDlg;
		UCLIDException::setExceptionHandler( &ueDlg );

		// Every time this application starts, re-register the file
		// associations if the file associations don't exist.  If the
		// file associations already exist, then do nothing.
		// This way, registration will happen the very first time
		// the user runs this application (even if the installation
		// program's call to this application with /r argument failed).
		// NOTE: the registration is not forced because we are passing
		// "true" for bSkipIfKeysExist.
		registerFileAssociations( gpszUEXFileExtension, gpszUEXFileDescription, 
			getAppFullPath(), true );

		// Check for a command line item
		if (__argc > 1)
		{
			/////////////////////////////////
			// Check for command-line options
			/////////////////////////////////

			// Register file associations and return
			if (_stricmp(__argv[1], "/r") == 0)
			{
				// Force registration since the /r argument was specifically provided
				// NOTE: Force by passing "false" for bSkipIfKeysExist
				registerFileAssociations( gpszUEXFileExtension, 
					gpszUEXFileDescription, getAppFullPath(), false );
				return FALSE;
			}

			// Unregister file associations and return
			else if (_stricmp(__argv[1], "/u") == 0)
			{
				unregisterFileAssociations( gpszUEXFileExtension, 
					gpszUEXFileDescription );
				return FALSE;
			}

			// Display usage and return.
			else if (_stricmp(__argv[1], "/?") == 0)
			{
				usage();
				return FALSE;
			}

			////////////////////
			// Read the UEX file
			////////////////////
			CStdioFile	file;
			CString		zLine;
			CString		zLine2;

			// Remove quotes from path
			string strFileName = __argv[1];
			replaceVariable(strFileName, "\"", "");

			// Check for file existence
			if (isValidFile(strFileName))
			{
				// Open the file
				CFileException	e;
				if (file.Open(strFileName.c_str(), CFile::modeRead, &e))
				{
					// Read the first line
					file.ReadString( zLine );

					// Read the second line, if present
					file.ReadString( zLine2 );

					// Close the file
					file.Close();

					if (__argc > 2 && _stricmp(__argv[2], "/temp") == 0)
					{
						m_strTemporaryFile = strFileName;
					}

					/////////////////////////////////////////////////
					// If only one line in the input file, 
					// then just display the exception in normal form
					/////////////////////////////////////////////////
					if (zLine2.IsEmpty())
					{
						// Parse the data
						vector<string> vecTokens;
						StringTokenizer	s;
						string strText((LPCTSTR)zLine);
						s.parse(strText, vecTokens);

						// Check the number of tokens
						if (vecTokens.size() == 7)
						{
							////////////////////////////////////////////////
							// Create and display the UCLID Exception object
							////////////////////////////////////////////////
							UCLIDException ue;
							string	strData = vecTokens[6];
							ue.createFromString("ELI14359", strData);
							// Do not add this exception to the log
							ue.display(false);

						}	// end if right number of tokens
						else
						{
							// Format error message
							CString zError;
							zError.Format("Parsing error (only %d tokens) on first line of file \"%s\"", 
								vecTokens.size(), strFileName);

							// Display error message
							::MessageBox(NULL, (LPCTSTR)zError, "Error", MB_ICONSTOP | MB_OK);

						}	// end else wrong number of tokens
					}		// end if single-line input file
					else
					{
						openUexDialog(m_pMainWnd, strFileName, m_strTemporaryFile.empty());
					}		// end else multiple-line input file
				}			// end if file open
				else
				{
					// Format error message
					CString zError;
					zError.Format("Error: %d, Could not open file \"%s\"", e.m_cause, strFileName);

					// Display error message
					::MessageBox(NULL, (LPCTSTR)zError, "Error", MB_ICONSTOP | MB_OK);

				}			// end else could not open file
			}			// end if file on command line exists
			else
			{
				openUexDialog(m_pMainWnd);
			}
		}				// end if file on command line
		else
		{
			openUexDialog(m_pMainWnd);
		}				// end else empty command line
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13415");

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return FALSE;
}
//-------------------------------------------------------------------------------------------------
int CUEXViewerApp::ExitInstance() 
{
	try
	{
		if (!m_strTemporaryFile.empty())
		{
			deleteFile(m_strTemporaryFile, false, false);
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI32289");

   return CWinApp::ExitInstance();
}