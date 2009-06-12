// CopyNumberedFiles.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "CopyNumberedFiles.h"

#include <cpputil.h>
#include <UCLIDException.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// The one and only application object
CWinApp theApp;

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const long glMILLISECONDS_PER_SECOND	= 1000;
const long glSECONDS_PER_MINUTE			= 60;
const long glSECONDS_PER_HOUR			= 3600;
const long glSECONDS_PER_DAY			= 86400;

//-------------------------------------------------------------------------------------------------
// Local functions
//-------------------------------------------------------------------------------------------------
void usage()
{
	string strUsage = "This application takes 4 required arguments:\n";
		strUsage += "\tan input file,\n";
		strUsage += "\tan output folder,\n";
		strUsage += "\ta delay time in seconds or milliseconds ( 10 = 10s = 10000ms ), and\n";
		strUsage += "\t\tone of 4 options to define either how many files to create or\n";
		strUsage += "\t\thow long to create files (in minutes, hours, or days)\n\n";
		strUsage += "Usage:\n";
		strUsage += "CopyNumberedFiles.exe <strInFile> <strOutFolder> <strDelay> [-n<Count>|-m<Minutes>|-h<Hours>|-d<Days>]\n";
		MessageBox( NULL, strUsage.c_str(), "Application Usage", MB_OK | MB_ICONINFORMATION );
}
//-------------------------------------------------------------------------------------------------
int _tmain(int argc, TCHAR* argv[], TCHAR* envp[])
{
	try
	{
		// Initialize MFC
		if (!AfxWinInit(::GetModuleHandle(NULL), NULL, ::GetCommandLine(), 0))
		{
			UCLIDException ue("ELI17039", "Fatal Error: MFC initialization failed!");
			throw ue;
		}

		// Check arguments count
		// This also handles /? on the command line
		if (argc != 5)
		{
			usage();
			return EXIT_FAILURE;
		}

		// Retrieve command-line parameters
		string strInputFile = argv[1];
		string strOutputFolder = argv[2];
		string strDelay = argv[3];
		string strDuration = argv[4];

		// Convert input file path from relative to absolute if necessary
		string strCWD = getCurrentDirectory();
		if (!isAbsolutePath( strInputFile ))
		{
			// Build complete string and simplify it
			string strIn = strCWD;
			strIn += "\\";
			strIn += strInputFile.c_str();
			simplifyPathName( strIn );
			strInputFile = strIn;
		}

		// Convert output folder path from relative to absolute if necessary
		if (!isAbsolutePath( strOutputFolder ))
		{
			// Build complete string and simplify it
			string strOut = strCWD;
			strOut += "\\";
			strOut += strOutputFolder.c_str();
			simplifyPathName( strOut );
			strOutputFolder = strOut;
		}

		// Make sure file exists
		validateFileOrFolderExistence( strInputFile );

		// Create output folder if needed
		if (!isFileOrFolderValid( strOutputFolder ))
		{
			createDirectory( strOutputFolder );
		}

		// Extract separate filename and extension strings
		string strFile = getFileNameWithoutExtension( strInputFile, false );
		string strExt = getExtensionFromFullPath( strInputFile, false );

		// Check delay string for units
		bool bUseMilliSeconds = false;
		makeLowerCase( strDelay );
		size_t nPos = strDelay.find( "ms" );
		if (nPos != string::npos)
		{
			// Found millisecond units
			bUseMilliSeconds = true;
			strDelay = strDelay.substr( 0, nPos );
		}
		else
		{
			nPos = strDelay.find( "s" );
			if (nPos != string::npos)
			{
				// Found second units
				strDelay = strDelay.substr( 0, nPos );
			}
		}
		// else no units found, default to seconds

		// Convert delay into count
		int nDelay = asLong( strDelay );

		// Extract duration type
		string strType = strDuration.substr( 0, 2 );
		makeLowerCase( strType );

		// Extract duration value
		string strValue = strDuration.substr( 2 );
		int nValue = asLong( strValue );

		// Interpret duration parameter
		int nLastFileCount = 0;
		if (strType == "-n")
		{
			// Use an absolute count
			nLastFileCount = nValue;
		}
		else if (strType == "-m")
		{
			// Determine final count based on number of minutes and the delay in seconds
			nLastFileCount = nValue * glSECONDS_PER_MINUTE;
			if (bUseMilliSeconds)
			{
				nLastFileCount *= glMILLISECONDS_PER_SECOND;
			}
			nLastFileCount /= nDelay;
		}
		else if (strType == "-h")
		{
			// Determine final count based on number of hours and the delay in seconds
			nLastFileCount = nValue * glSECONDS_PER_HOUR;
			if (bUseMilliSeconds)
			{
				nLastFileCount *= glMILLISECONDS_PER_SECOND;
			}
			nLastFileCount /= nDelay;
		}
		else if (strType == "-d")
		{
			// Determine final count based on number of days and the delay in seconds
			nLastFileCount = nValue * glSECONDS_PER_DAY;
			if (bUseMilliSeconds)
			{
				nLastFileCount *= glMILLISECONDS_PER_SECOND;
			}
			nLastFileCount /= nDelay;
		}
		else 
		{
			// Unsupported duration type
			THROW_LOGIC_ERROR_EXCEPTION("ELI17036");
		}

		// Create desired number of files
		long lIndex = 1;
		for (lIndex = 1; lIndex <= nLastFileCount; lIndex++)
		{
			// Create new filename
			string strDestination = strOutputFolder;
			strDestination += "\\";
			strDestination += strFile.c_str();
			strDestination += "_";
			strDestination += asString(lIndex).c_str();
			strDestination += strExt.c_str();

			// Only copy the file if not already present
			if (!isFileOrFolderValid( strDestination ))
			{
				// Use the old timestamp
				copyFile( strInputFile, strDestination, false );
			}

			// Apply appropriate delay
			Sleep( bUseMilliSeconds ? nDelay : nDelay * glMILLISECONDS_PER_SECOND );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17035");

	return EXIT_SUCCESS;
}
//-------------------------------------------------------------------------------------------------
