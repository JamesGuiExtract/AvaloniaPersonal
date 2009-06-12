// DetectIncorrectPageCount.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#include <cpputil.hpp>
#include <UCLIDException.hpp>
#include <FileDirectorySearcher.hpp>
#include <MiscLeadUtils.h>

#include <string>
#include <iostream>
#include <fstream>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// String constant for format statement for Error File comments lines
const std::string	gstrERROR_FILE_COMMENT_FORMAT = "// File in INPUT_FOLDER contains %d pages; File in OUTPUT_FOLDER contains %d pages.\n";

// String constant for format statement for Error File input file lines
const std::string	gstrERROR_FILE_INPUTFILE_FORMAT = "%s\n";

// String constant for format statement for Final Output of Input Folder
const std::string	gstrOUTPUT_INPUTFOLDER_FORMAT = "Name of input folder: %s\n";

// String constant for format statement for Final Output of Input Folder File Count
const std::string	gstrOUTPUT_INPUTFOLDER_COUNT_FORMAT = "Total number of files in input folder: %d\n";

// String constant for format statement for Final Output of Output Folder
const std::string	gstrOUTPUT_OUTPUTFOLDER_FORMAT = "Name of output folder: %s\n";

// String constant for format statement for Final Output of Output Folder File Count
const std::string	gstrOUTPUT_OUTPUTFOLDER_COUNT_FORMAT = "Total number of files in output folder: %d\n";

// String constant for format statement for Final Output of Error List File
const std::string	gstrOUTPUT_ERRORFILE_FORMAT = "Name of error list file: %s\n";

// String constant for format statement for Final Output of Error List File Count
const std::string	gstrOUTPUT_ERRORLIST_COUNT_FORMAT = "Total number of files with incorrect page count or errors: %d\n";

//-------------------------------------------------------------------------------------------------
// Local functions
//-------------------------------------------------------------------------------------------------
void usage()
{
	string strUsage = "This application takes 3 arguments:\n";
		strUsage += "\tan input folder,\n";
		strUsage += "\tan output folder,\n";
		strUsage += "\tan error list file\n\n";
		strUsage += "Usage:\n";
		strUsage += "DetectIncorrectPageCount.exe <strInFolder> <strOutFolder> <strErrorFile>\n";
		MessageBox( NULL, strUsage.c_str(), "Application Usage", MB_OK | MB_ICONINFORMATION );
}
//-------------------------------------------------------------------------------------------------
int getImagePageCount(string strImageFile)
{
	FILEINFO fileInfo;
	// Initialize fileInfo structure to all zeros
	memset( &fileInfo, 0, sizeof(fileInfo));	

	// Get Number of Pages
	int nRet = L_FileInfo( (char *)strImageFile.c_str(), &fileInfo, sizeof(FILEINFO), 
		FILEINFO_TOTALPAGES, NULL);

	// Check for failure
	if (nRet < 0)
	{
		// Create and throw exception
		UCLIDException ue( "ELI15360", "Unable to retrieve image page count!" );
		ue.addDebugInfo( "Image File", strImageFile );
		ue.addDebugInfo( "Error Code", nRet );
		throw ue;
	}

	return fileInfo.TotalPages;
}

//-------------------------------------------------------------------------------------------------
// Main application
//-------------------------------------------------------------------------------------------------
int _tmain(int argc, _TCHAR* argv[])
{
	try
	{
		// Check arguments count
		// This also handles /? on the command line
		if (argc != 4)
		{
			usage();
			return EXIT_FAILURE;
		}

		// Initialize license
//		LicenseManagement::sGetInstance().loadLicenseFilesFromFolder();
//		validateLicense();

		// Retrieve command-line parameters
		string strInputFolder = argv[1];
		string strOutputFolder = argv[2];
		string strErrorFile = argv[3];
		
		// Make sure folders exists
		validateFileOrFolderExistence( strInputFolder );
		validateFileOrFolderExistence( strOutputFolder );

		// Open the error list file for append
		ofstream fileError(strErrorFile.c_str(), ios::app);
		if (!fileError)
		{
			// Create and throw exception
			UCLIDException ue( "ELI15262", "Unable to write to error list file in append mode!" );
			ue.addDebugInfo( "Error File", strErrorFile );
			throw ue;
		}

		// Initialize input and error counters
		long lInputFileCount = 0;
		long lErrorCount = 0;

		// Find all files in the output folder, without recursion
		//   These are expected to be redacted image files with 
		//   unredacted original images in strInputFolder
		string strFileSpec = strOutputFolder + "\\*.*";
		FileDirectorySearcher fds;
		vector<string> vecOutputFiles;
		vecOutputFiles = fds.searchFiles( strFileSpec, false );

		// Check each file found in the output folder
		long lOutputFileCount = (long)vecOutputFiles.size();
		for (int i = 0; i < lOutputFileCount; i++ )
		{
			// Ignore this file if it is a hidden file (i.e. Thumbs.db)
			CFileStatus status;
			CFile::GetStatus( vecOutputFiles[i].c_str(), status );
			if ( status.m_attribute & 0x02  )
			{
				continue;
			}

			// Retrieve the filename portion of the path
			string strName = getFileNameFromFullPath( vecOutputFiles[i] );

			// Build the filename expected in the input folder
			string strExpectedInput = strInputFolder;
			strExpectedInput += "\\";
			strExpectedInput += strName;

			// Provide filename to console
			cout << strName.c_str() << endl;

			// Check to see if this file is present in the input folder
			if (!isValidFile( strExpectedInput ))
			{
				UCLIDException ue("ELI15254", "Cannot find expected input file!");
				ue.addDebugInfo("Expected File", strExpectedInput );
				throw ue;
			}

			// Initialize page counts and increment file counter
			long lOutFilePageCount = 0;
			long lInFilePageCount = 0;
			lInputFileCount++;

			// Get page counts for the file in the input and output folder
			lInFilePageCount = getImagePageCount( strExpectedInput );
			lOutFilePageCount = getImagePageCount( vecOutputFiles[i] );

			// Compare page counts
			if (lInFilePageCount != lOutFilePageCount)
			{
				// Build comment line and add to Error List file
				CString	zComment;
				zComment.Format( gstrERROR_FILE_COMMENT_FORMAT.c_str(), 
					lInFilePageCount, lOutFilePageCount );
				fileError << string((LPCTSTR) zComment);

				// Build input file line and add to Error List file
				CString	zInputFile;
				zInputFile.Format( gstrERROR_FILE_INPUTFILE_FORMAT.c_str(), 
					strExpectedInput.c_str() );
				fileError << string((LPCTSTR) zInputFile);

				// Increment error counter
				lErrorCount++;
			}
		}		// end for each file

		/////////////////////////////////////////
		// Output final statistics to the console
		/////////////////////////////////////////
		CString zOut;
		cout << endl;

		// Input Folder Name
		zOut.Format( gstrOUTPUT_INPUTFOLDER_FORMAT.c_str(), strInputFolder.c_str() );
		cout << zOut;

		// Input File Count
		zOut.Format( gstrOUTPUT_INPUTFOLDER_COUNT_FORMAT.c_str(), lInputFileCount );
		cout << zOut;

		// Output Folder Name
		zOut.Format( gstrOUTPUT_OUTPUTFOLDER_FORMAT.c_str(), strOutputFolder.c_str() );
		cout << zOut;

		// Output Folder Count
		zOut.Format( gstrOUTPUT_OUTPUTFOLDER_COUNT_FORMAT.c_str(), lOutputFileCount );
		cout << zOut;

		// Error File Name
		zOut.Format( gstrOUTPUT_ERRORFILE_FORMAT.c_str(), strErrorFile.c_str() );
		cout << zOut;

		// Error File Count
		zOut.Format( gstrOUTPUT_ERRORLIST_COUNT_FORMAT.c_str(), lErrorCount );
		cout << zOut;

		// Wait for user action before closing the console
		cout << endl << "Press [ENTER] to exit..." << endl;
		cin.unsetf(ios::skipws);
		char c;
		cin >> c;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15259");

	return EXIT_SUCCESS;
}
//-------------------------------------------------------------------------------------------------
