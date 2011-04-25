//==================================================================================================
//
// COPYRIGHT (c) 2008 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	GetWordLengthDist.cpp
//
// PURPOSE:	To process a file (or group of files) and return a word distribution
//			for the specified file(s). [p13 #4944]
//
// AUTHORS:	Jeff Shergalis
//
//==================================================================================================

#include "stdafx.h"

#include <UCLIDException.h>
#include <UCLIDExceptionDlg.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>

#include <iostream>
#include <fstream>
#include <string>
#include <map>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

using namespace std;

DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
const string gstrDEFAULT_EXT = ".uss";

const string gstrHEADER_ROW = "Length, Count, Percentage";

const unsigned int guiREAD_BYTES = 32768; // 2^15 bytes

//--------------------------------------------------------------------------------------------------
// Global variables
//--------------------------------------------------------------------------------------------------
string g_strInput = "";
string g_strExtension = "";
bool g_bDirectory = false;
bool g_bTreatAsList = false;
bool g_bOutputCSV = false;
bool g_bRecursive = false;

//--------------------------------------------------------------------------------------------------
// The application object
//--------------------------------------------------------------------------------------------------
CWinApp theApp;

//--------------------------------------------------------------------------------------------------
// Helper functions
//--------------------------------------------------------------------------------------------------
// PURPOSE: To display the application usage message to the user
void displayUsage()
{
	string strMessage = "GetWordLengthDist.exe {<FileName>|<DirectoryName>[<Extension>]} ";
	strMessage += "[/fl] [/oc] [/r] [/ef <UEXFileName>]\n";
	strMessage += "Usage:\n";
	strMessage += "--------------------\n";
	strMessage += "\t<FileName>: The name of a file to process\n";
	strMessage += "\t<DirectoryName>[<Extension>]: The name of a directory to process\n";
	strMessage += "\t    <Extension>: Optional, the extension of files to process ";
	strMessage += "(default extension is *.uss)\n";
	strMessage += "Optional arguments:\n";
	strMessage += "--------------------\n";
	strMessage += "/fl: Treat <FileName> as a text file containing a list of files to ";
	strMessage += "be processed\n";
	strMessage += "/oc: Output one csv file for each input file\n";
	strMessage += "/r: Recursively search all subdirectories (only applies to <DirectoryName>)\n";
	strMessage += "/ef <UEXFileName>: Log exceptions to <UEXFileName> rather than display them\n";
	strMessage += "\nExamples:\n";
	strMessage += "--------------------\n";
	strMessage += "GetWordLengthDist C:\\temp\\*.txt /oc /r /ef C:\\gwldException.uex\n";
	strMessage += "GetWordLengthDist C:\\temp\\testfile.uss\n";
	strMessage += "GetWordLengthDist C:\\temp\\mylist.lst /fl /oc\n";

	MessageBox(NULL, strMessage.c_str(), "Usage", MB_OK | MB_ICONINFORMATION);
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To validate the user license
void validateLicense()
{
	VALIDATE_LICENSE(gnRULE_DEVELOPMENT_TOOLKIT_OBJECTS, "ELI20640", "GetWordLengthDist");
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To get the file or directory name from the command line file argument
//
// PROMISE: To set the bDirectory flag appropriately and to validate the file
//			or folder for existence
void getFileOrDirectory(const string& strFileArgument)
{
	// first look for * in string
	size_t stPos = strFileArgument.find("*");

	// if stPos == npos then this is either a file name, or a directory
	// with no extension specified
	if (stPos == strFileArgument.npos)
	{
		g_strInput = buildAbsolutePath(strFileArgument);
		g_bDirectory = isValidFolder(g_strInput);
	}
	else
	{
		// directory with extension specified
		g_bDirectory = true;

		// get the extension (ignore the * character)
		g_strExtension = strFileArgument.substr(stPos+1);

		// get the directory name (everything before the * character)
		g_strInput = buildAbsolutePath(strFileArgument.substr(0, stPos-1));
	}

	validateFileOrFolderExistence(g_strInput);
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To get the word length distribution for the specified file and to add the
//			results to the passed in map reference and to add the total number of words
//			to the passed in total reference
//
// PROMISE: To also produce a csv file named strFileName.csv if the bOutputCSV file
//			flag is set
void getWordLengthDistForFile(const string& strFileName, map<long, long>& rmapLengthToCount, 
					   long& rlTotalWords)
{
	// get the extension from the file name
	string strExtension = getExtensionFromFullPath(strFileName, true);

	ISpatialStringPtr ipSS(CLSID_SpatialString);
	ASSERT_RESOURCE_ALLOCATION("ELI20641", ipSS != NULL);

	// check for .uss file
	if (strExtension == ".uss")
	{
		// load the spatial string from the .uss file
		ipSS->LoadFrom(strFileName.c_str(), VARIANT_FALSE);
	}
	// treat as .txt file
	else
	{
		// build a string from the file lines
		string strFileString = getTextFileContentsAsString(strFileName);
		ipSS->CreateNonSpatialString(strFileString.c_str(), "");
	}

	
	// if the output to csv flag was present, open a file for output
	// CFile will automatically be closed when the destructor is called
	CFile flOutput;
	if (g_bOutputCSV)
	{
		// create a CFileException to hold error information if the CFile object
		// fails to open properly
		CFileException cex;
		string strCSVFileName = strFileName + ".csv";
		if (!flOutput.Open(strCSVFileName.c_str(), CFile::modeCreate | CFile::modeWrite, &cex))
		{
			// failed to open the file, throw the Exception
			// (Note: this will be deleted by the catch handlers)
			CFileException* pcex = new CFileException(cex.m_cause, 
				cex.m_lOsError, cex.m_strFileName);
			throw pcex;
		}
		else
		{
			// file opened successfully, add the header row
			flOutput.Write((gstrHEADER_ROW + "\n").c_str(), (UINT)(gstrHEADER_ROW.length()+1));
		}
	}

	// get the word length distribution from the spatial string
	long lTotalWords(0);
	ILongToLongMapPtr ipMap = ipSS->GetWordLengthDist(&lTotalWords);
	ASSERT_RESOURCE_ALLOCATION("ELI20648", ipMap != NULL);

	// increment the total word count
	rlTotalWords += lTotalWords;

	// get the size of the LongToLong map and iterate through
	// adding each of the values to the map
	long lSize = ipMap->Size;
	for (long i = 0; i < lSize; i++)
	{
		long lLength(0), lCount(0);
		ipMap->GetKeyValue(i, &lLength, &lCount);
		rmapLengthToCount[lLength] += lCount;

		// if output to csv then add the data to the csv file
		if (g_bOutputCSV)
		{
			double dPercent = (double)lCount / (double)lTotalWords * 100.0;
			CString zOutput;
			zOutput.Format("%d, %d, %.2f%%\n", lLength, lCount, dPercent);
			flOutput.Write(zOutput, zOutput.GetLength());
		}
	}

	if (g_bOutputCSV)
	{
		CString zOutputPath = flOutput.GetFilePath();
		flOutput.Close();
		if (zOutputPath.GetLength() != 0)
		{
			waitForFileAccess((LPCTSTR) zOutputPath, giMODE_READ_ONLY);
		}
	}
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To search through a directory looking for files that meet the specified
//			criteria (as set on the command line) and to process the files
//			that meet the criteria.  If the recursive flag has been set then will
//			also recursively search directories for files that meet the criteria
void getAndProcessAllFilesInDirectory(string strRoot, map<long, long>& rmapLengthToCount,
									  long& rlTotalCount)
{
	try
	{
		// make sure root directory ends in a slash
		if (strRoot[strRoot.length() - 1] != '\\' || strRoot[strRoot.length() - 1] != '/')
		{
			strRoot += '\\';
		}

		WIN32_FIND_DATA fileInfo;
		HANDLE fileHandle;
		BOOL bFindFileResult = TRUE;

		fileHandle = FindFirstFile((strRoot + "*.*").c_str(), &fileInfo);
		if (fileHandle == INVALID_HANDLE_VALUE)
		{
			return;
		}

		// go through all the files/dirs in the current directory
		do
		{
			string strFileName(fileInfo.cFileName);
			if (strFileName != "." && strFileName != "..")
			{
				// if it is a directory and recursive option is true then recurse
				// and process all files in sub-directory
				if ((fileInfo.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) > 0)
				{
					if (g_bRecursive)
					{
						getAndProcessAllFilesInDirectory(strRoot+strFileName, rmapLengthToCount, 
							rlTotalCount);
					}
				}
				// file found, process it
				else
				{
					// add path information to file name
					strFileName = strRoot + strFileName;
					string strExtension = getExtensionFromFullPath(strFileName, true);

					// if no extension specified then only operate on default extension
					// or extension specified operate on matching files
					if ((g_strExtension.empty() && (strExtension == gstrDEFAULT_EXT))
						|| (strExtension == g_strExtension))
					{
						getWordLengthDistForFile(strFileName, rmapLengthToCount, rlTotalCount);
					}
				}
			}

			bFindFileResult = FindNextFile(fileHandle, &fileInfo);
		}
		while (bFindFileResult);

		FindClose(fileHandle);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20678");
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To get the word length distribution for the specified file(s)
void getDistribution(map<long, long>& rmapLengthToCount, long& rlTotalWords)
{
	// treat input as directory
	if (g_bDirectory)
	{
		getAndProcessAllFilesInDirectory(g_strInput, rmapLengthToCount, rlTotalWords);
	}
	// treat the file as a list of files
	else if (g_bTreatAsList)
	{
		ifstream fList(g_strInput.c_str(), ios::in);
		if (!fList)
		{
			UCLIDException ue("ELI20677", "Unable to open list file for processing!");
			ue.addDebugInfo("List FileName", g_strInput);
			throw ue;
		}

		while (!fList.eof())
		{
			// get the file name from the list
			string strFileName;
			getline(fList, strFileName);

			// ignore blank lines
			if (strFileName.length() > 0)
			{
				// build the absolute path to the filename relative to the list file
				strFileName = getAbsoluteFileName(g_strInput, strFileName, true);

				getWordLengthDistForFile(strFileName, rmapLengthToCount, rlTotalWords);
			}
		}
	}
	// single file
	else
	{
		getWordLengthDistForFile(g_strInput, rmapLengthToCount, rlTotalWords);
	}
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To print the total of the results to the command line
void outputResults(const map<long, long>& mapLengthToCount, long lTotalWords)
{
	cout << "Total Words: " << lTotalWords << endl;
	cout << gstrHEADER_ROW << endl;

	for (map<long, long>::const_iterator it = mapLengthToCount.begin();
		it != mapLengthToCount.end(); it++)
	{
		double dPercent = ((double)(it->second) / (double)lTotalWords) * 100.0;
		CString zOutput;
		zOutput.Format("%d, %d, %.2f%%\n", it->first, it->second, dPercent);

		cout << zOutput;
	}
}

//--------------------------------------------------------------------------------------------------
// The main function
//--------------------------------------------------------------------------------------------------
int _tmain(int argc, TCHAR* argv[], TCHAR* envp[])
{
	string strExceptionLog = "";

	CoInitializeEx(NULL, COINIT_APARTMENTTHREADED);

	try
	{

		try
		{
			// initialize MFC and print and error on failure
			if (!AfxWinInit(::GetModuleHandle(NULL), NULL, ::GetCommandLine(), 0))
			{
				// TODO: change error code to suit your needs
				_tprintf(_T("Fatal Error: MFC initialization failed\n"));
				return EXIT_FAILURE;
			}

			// Setup exception handling
			UCLIDExceptionDlg exceptionDlg;
			UCLIDException::setExceptionHandler( &exceptionDlg );

			// init license management 
			LicenseManagement::loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);

			// check license
			validateLicense();

			// process the command line
			if (argc < 2 || argc > 6)
			{
				displayUsage();
				return EXIT_FAILURE;
			}

			// first argument should be either a file name or a directory name
			getFileOrDirectory(string(argv[1]));
			
			// now get the rest of the command line arguments
			for (int i=2; i < argc; i++)
			{
				string strArg(argv[i]);
				makeLowerCase(strArg);

				if (strArg == "/fl")
				{
					g_bTreatAsList = true;
				}
				else if (strArg == "/oc")
				{
					g_bOutputCSV = true;
				}
				else if (strArg == "/r")
				{
					if (g_bDirectory)
					{
						g_bRecursive = true;
					}
				}
				else if (strArg == "/ef")
				{
					if (i+1 < argc)
					{
						strExceptionLog = buildAbsolutePath(argv[++i]);

						// ensure that the exception file ends with .uex
						if (getExtensionFromFullPath(strExceptionLog, true) != ".uex")
						{
							// there was an error with the exception log file name,
							// set it back to empty so that exceptions will be displayed
							strExceptionLog = "";
							UCLIDException ue("ELI20681", 
								"Exception file name must end with \".uex\"!");
							ue.addDebugInfo("Exception FileName", string(argv[i]));
							throw ue;
						}
					}
					else
					{
						cout << "/ef specified without <UEXFileName>!\n";
						displayUsage();
						return EXIT_FAILURE;
					}
				}
				else
				{
					cout << "Unrecognized command line option!\n";
					displayUsage();
					return EXIT_FAILURE;
				}
			}
			
			map<long, long> mapLengthToCount;
			long lTotalWords = 0;

			// get the distribution
			getDistribution(mapLengthToCount, lTotalWords);

			// output the results
			outputResults(mapLengthToCount, lTotalWords);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20639");
	}
	catch(UCLIDException& ue)
	{
		if (!strExceptionLog.empty())
		{
			ue.log(strExceptionLog);
		}
		else
		{
			ue.display();
		}
	}

	CoUninitialize();

	return EXIT_SUCCESS;
}
//--------------------------------------------------------------------------------------------------
