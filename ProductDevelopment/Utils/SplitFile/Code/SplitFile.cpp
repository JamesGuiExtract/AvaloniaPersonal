// SplitFile.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#include <cpputil.h>
#include <FileDirectorySearcher.h>
#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

#include <string>
#include <iostream>
#include <fstream>

using namespace std;

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// Local functions
//-------------------------------------------------------------------------------------------------
void validateLicense()
{
	static const unsigned long THIS_APP_ID = gnFLEXINDEX_IDSHIELD_SERVER_CORE;

	VALIDATE_LICENSE( THIS_APP_ID, "ELI15250", "Split File" );
}
//-------------------------------------------------------------------------------------------------
void printUsage()
{
	cout << endl;
	cout << "SplitFile <File with file list> <# lines per file>" << endl;
}

//-------------------------------------------------------------------------------------------------
// Main application
//-------------------------------------------------------------------------------------------------
int main(int argc, char* argv[])
{
	CoInitializeEx(NULL, COINIT_MULTITHREADED);
	try
	{
		if (argc != 3)
		{
			printUsage();
			return EXIT_FAILURE;
		}

		// Initialize license
		LicenseManagement::loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);
		validateLicense();

		string strInputFile = argv[1];
		int iNumLines = asLong(argv[2]);
		string strFilePrefix = getPathAndFileNameWithoutExtension( strInputFile );

		// Make sure input file exists
		validateFileOrFolderExistence(strInputFile);

		// Check for files with names as the output
		string strFileSpec = strFilePrefix + ".*";
		FileDirectorySearcher fds;
		vector<string> vecExistingSplitFiles;
		vecExistingSplitFiles = fds.searchFiles( strFileSpec, false );

		// Verify that there are no files with the given prefix
		for ( unsigned int u = 0; u < vecExistingSplitFiles.size(); u++ )
		{
			long lExtValue = -1;
			try
			{
				string strExt = getExtensionFromFullPath( vecExistingSplitFiles[u] );
				lExtValue = asLong(strExt.substr(1));
			}
			catch(...)
			{
				// a caught exception here means the extension either did not exist or was non numeric which is ok
				lExtValue = -1;
			}

			if ( lExtValue >= 0 )
			{
				UCLIDException ue("ELI13237", "File with numeric extension already exists." );
				ue.addDebugInfo("File", vecExistingSplitFiles[u] );
				throw ue;
			}
		}

		// Output the lines to separate files
		ifstream inputFile(strInputFile.c_str());
		int iFileNumber = 0;
		int iTotalLines = 0;
		while ( !inputFile.bad() && !inputFile.eof())
		{
			string strLine;
			iFileNumber++;
			string strOutputFileName = strFilePrefix + '.' + asString(iFileNumber);

			ofstream outputFile(strOutputFileName.c_str());
			cout << "Creating: " << strOutputFileName << endl;

			for ( int i = 0; i < iNumLines && !inputFile.eof(); i++ )
			{
				strLine = "";
				getline( inputFile, strLine );
				outputFile << strLine << endl;
				iTotalLines++;
			}

			// Close the file and wait for it to be readable
			outputFile.close();
			waitForFileToBeReadable(strOutputFileName);
		}

		// Output number of lines processed and number of files
		cout << endl;
		cout << "Total lines from " << strInputFile << " : " << asString(iTotalLines) << endl;
		cout << "Split into " << asString(iFileNumber) << " file(s)" << endl;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13238");
	CoUninitialize();

	return EXIT_SUCCESS;
}
//-------------------------------------------------------------------------------------------------
