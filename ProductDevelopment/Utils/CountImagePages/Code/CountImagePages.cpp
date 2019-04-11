// CountImagePages.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#pragma warning(disable:4786)

#include <l_bitmap.h>
#include <cpputil.h>
#include <io.h>
#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <MiscLeadUtils.h>
#include <LeadToolsLicenseRestrictor.h>

#include <vector>
#include <string>
#include <iostream>
using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

unsigned long gulNumDocs = 0;
unsigned long gulNumPages = 0;
unsigned long gulNumErrors = 0;

//-------------------------------------------------------------------------------------------------
void printUsage()
{
	cout << "This utility counts the number of pages in multi-page images." << endl;
	cout << endl;
	cout << "This application uses 2 arguments:" << endl;
	cout << " arg1: Root directory for searching (e.g. \"C:\\ImageFiles\")" << endl;
	cout << " arg2: <Optional> Use /s to indicate recursive search." << endl;
	cout << "       By default, the searches are not recursive." << endl;
	cout << endl;
}
//-------------------------------------------------------------------------------------------------
void process(const string& strRootDir, bool bRecursive)
{
	vector<string> vecFiles;
	getFilesInDir(vecFiles, strRootDir, 
		"*.tif", bRecursive);

	vector<string>::const_iterator iter;
	string strLastDir;
	unsigned long ulNumFolderDocs = 0;
	unsigned long ulNumFolderPages = 0;
	for (iter = vecFiles.begin(); iter != vecFiles.end(); iter++)
	{
		const string& strFile = *iter;
		string strFileNameWithoutPath = getFileNameFromFullPath(strFile);

		FILEINFO fileInfo;
		memset(&fileInfo, 0, sizeof(FILEINFO));

		// get the page count
		{
			LeadToolsLicenseRestrictor leadToolsLicenseGuard;
			int iReturnCode = L_FileInfo((char *)strFile.c_str(), &fileInfo, sizeof(FILEINFO),
				FILEINFO_TOTALPAGES, NULL);
			if (iReturnCode != SUCCESS)
			{
				cout << strFileNameWithoutPath << " - ERROR! Unable to determine page count! (ErrorCode = " << iReturnCode << ")" << endl;
				gulNumErrors++;
				continue;
			}
		}

		// increment the counters
		gulNumDocs++;
		unsigned long ulImagePages = fileInfo.TotalPages;
		gulNumPages += ulImagePages;

		// update status
		string strThisDir = getDirectoryFromFullPath(strFile);
		if (strThisDir != strLastDir || iter == vecFiles.end())
		{
			// summarize results for the previous folder if applicable
			if (!strLastDir.empty())
			{
				cout << "Total " << ulNumFolderPages << " pages in ";
				cout << ulNumFolderDocs << " documents." << endl << endl;
				ulNumFolderDocs = 0;
				ulNumFolderPages = 0;
			}

			// output the new folder name
			strLastDir = strThisDir;
			cout << "Folder: " << strThisDir << endl;
		}

		ulNumFolderDocs++;
		ulNumFolderPages += ulImagePages;
		cout << strFileNameWithoutPath << " - " << ulImagePages << " pages." << endl;
	}
}
//-------------------------------------------------------------------------------------------------
void validateLicense()
{
	// Requires Flex Index/ID Shield core license
	static const unsigned long THIS_APP_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_APP_ID, "ELI46650", "CountImagePages");
}

//-------------------------------------------------------------------------------------------------
int main(int argc, char *argv[])
{
	try
	{
		// print banner
		cout << "CountImagePages Utility" << endl;
		cout << "Copyright 2004, UCLID Software, LLC." << endl;
		cout << "All rights reserved." << endl;
		cout << endl;

		// Load license files ( this is need for IVariantVector )
		LicenseManagement::loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);
		validateLicense();

		InitLeadToolsLicense();

		// check for correct # of arguments
		if (argc < 2 || argc > 3)
		{
			cout << "ERROR: Incorrect usage!" << endl;
			printUsage();
			return EXIT_FAILURE;
		}

		// get the root directory
		string strRootDir = argv[1];
		
		// get the correct state of the recursive flag
		bool bRecursive = false;
		if (argc == 3)
		{
			if (_strcmpi(argv[2], "/s") == 0)
			{
				bRecursive = true;
			}
			else
			{
				cout << "ERROR: Invalid second argument!" << endl;
				printUsage();
				return EXIT_FAILURE;
			}
		}

		// process the files per the user's specifications
		CTime start = CTime::GetCurrentTime();
		process(strRootDir, bRecursive);
		CTime end = CTime::GetCurrentTime();
		CTimeSpan duration = end - start;
		unsigned long ulTotalSeconds = (unsigned long)duration.GetTotalSeconds();

		cout << endl;
		cout << endl;
		cout << "Final summary: " << endl;
		cout << "Total " << gulNumPages << " pages in ";
		cout << gulNumDocs << " documents." << endl;
		double dAverage = gulNumPages / (double ) gulNumDocs;
		cout << "Average pages per document: " << dAverage << endl;
		cout << "Total errors: " << gulNumErrors << endl;	
		cout << "Total processing time: ";
		cout << ulTotalSeconds << " seconds." << endl;
		cout << endl;

		return EXIT_SUCCESS;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06410")

	// if we reached here, it's because an exception was caught.
	cout << endl;
	cout << "NOTE: Process terminated prematurely because of error condition!" << endl;
	cout << endl;
	return EXIT_FAILURE;
}
//-------------------------------------------------------------------------------------------------
