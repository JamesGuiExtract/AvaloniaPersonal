//==================================================================================================
//
// COPYRIGHT (c) 2008 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	FindString.cpp
//
// PURPOSE:	To search a file (or group of files) for a specified search string
//			and return the match information. (enhanced as per [p13 #4948])
//
// AUTHORS:	
//
// MODIFIED BY:	Jeff Shergalis as per [p13 #4948]
//
//==================================================================================================

#include "stdafx.h"
#include "MatchData.h"

#include <ComponentLicenseIDs.h>
#include <ComUtils.h>
#include <cpputil.h>
#include <FileIterator.h>
#include <LicenseMgmt.h>
#include <StringTokenizer.h>
#include <UCLIDException.h>
#include <UCLIDExceptionDlg.h>

#include <cstdlib>
#include <string>
#include <fstream>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
const string gstrDEFAULT_EXT = ".uss";

const unsigned int guiREAD_BYTES = 32768; // 2^15 bytes

const int giMATCH_CHARS_EST = 80;

//--------------------------------------------------------------------------------------------------
// Global variables
//--------------------------------------------------------------------------------------------------
string g_strInput = "";
string g_strExtension = "";
string g_strVerboseFile = "";

// flags for command line arguments
bool g_bTreatAsList = false;
bool g_bExpInFile = false;
bool g_bUseRegExp = false;
bool g_bTotalPerFile = false;
bool g_bVerbose = false;
bool g_bVerboseToFile = false;
bool g_bRecursive = false;
bool g_bDirectory = false;
bool g_bCaseSensitive = false;
bool g_bPagesSpecified = false;

long g_nStartPage = -1;
long g_nEndPage = -1;

unsigned long g_ulNumMatches = 0;

//--------------------------------------------------------------------------------------------------
void displayUsage(const string& strError = "")
{
	string strMessage = "";
	if (!strError.empty())
	{
		strMessage += "ERROR! - " + strError + "\n\n";
	}
	else
	{
		strMessage += "FindString.exe - (c) 2021 - Extract Systems, LLC\n\n";
	}

	strMessage += "--------------------\n";
	strMessage += "Usage: FindString <filename> <search text> [OPTIONS]\n";
	strMessage += "OPTIONS:\n";
	strMessage += "\t/fl - Treat <filename> as a file containing a list of files to search\n";
	strMessage += "\t/el - Treat <search text> as a file containing a regular expression\n";
	strMessage += "\t/e - Treat search text as a regular expression\n";
	strMessage += "\t/c - Perform a case sensitive search\n";
	strMessage += "\t/t - Display total matches for each file (cannot be combined with /v)\n";
	strMessage += "\t/v - Provide verbose output\n";
	strMessage += "\t/vf <MatchResultsFile> - Write verbose output to <MatchResultsFile>\n";
	strMessage += "\t/r - Recursively search subdirectories\n";
	strMessage += "\t     only has an effect when <filename> is a directory\n";
	strMessage += "\t/p StartPage-Endpage - search a range of pages\n";
	strMessage += "\t	    /p 1 searches only page 1\n";
	strMessage += "\t	    /p 1-5 searches only pages 1 through 5\n";
	strMessage += "\t	    /p 1- searches pages 1 through the end of the document\n";
	strMessage += "\t/ef <UEXFile> - Log exceptions to <UEXFile> rather than display them\n";

	AfxMessageBox(strMessage.c_str());
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To validate the user license
void validateLicense()
{
	VALIDATE_LICENSE(gnRULE_DEVELOPMENT_TOOLKIT_OBJECTS, "ELI20693", "FindString");
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
// PURPOSE: To search the given string for the given expression and fill in the
//			vector of matches with the match information
bool FindVariable(const string& strStringToSearch, const string& strSearchString, 
				  const string& strSourceDoc, vector<MatchData>& rvecMatches)
{
	size_t findpos;
	bool bReturnType;

	// copy arguments to local vars (we may need to modify strings for case insensitive search)
	string s = strStringToSearch;
	string t1 = strSearchString;

	// store the length of the search string
	size_t nSearchLength = strSearchString.length();

	// check for case sensitive search
	if (!g_bCaseSensitive)
	{
		makeLowerCase(s);
		makeLowerCase(t1);
	}

	// find the first occurrence
	findpos = s.find(t1);
	if(findpos == string::npos)
	{
		// no match found
		bReturnType = false;
	}
	else
	{
		// at least one match found
		bReturnType = true;

		do
		{
			// increment the number of found matches
			g_ulNumMatches++;

			// push the match data into the vector
			rvecMatches.push_back(MatchData(strSourceDoc, strSearchString, 
				(unsigned long) findpos, (unsigned long) (findpos + nSearchLength)));

			// find the next occurrence
			findpos = s.find(t1, findpos + nSearchLength);
		}
		// loop while there is still data found
		while (findpos != string::npos);
	}
	return bReturnType;
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To find the search expression in the given file (either by regular expression
//			or plain text searching)
void FindVariableInFile(const string& strFileName, const string& strSearchString,
					   IRegularExprParserPtr ipRegExParser)
{
	// string to hold the file contents
	string strFileContents;

	// vector to hold the matches
	vector<MatchData> vecMatches;

	string ext = getExtensionFromFullPath(strFileName, true);
	// .uss file, load into spatial string
	if(ext == ".uss")
	{
		try
		{
			// create a new spatial string
			ISpatialStringPtr ipSS(CLSID_SpatialString);
			ASSERT_RESOURCE_ALLOCATION("ELI20685", ipSS != __nullptr);

			// now load the spatial string from the file
			ipSS->LoadFrom(strFileName.c_str(), VARIANT_FALSE);

			// if select pages specified, get only specific pages
			if(g_bPagesSpecified)
			{
				ipSS = ipSS->GetSpecifiedPages(g_nStartPage, g_nEndPage);
			}

			// get the string from the spatial string
			strFileContents = asString(ipSS->String);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20686");
	}
	// other type of file - treat as plain text
	else
	{
		strFileContents = getTextFileContentsAsString(strFileName);
	}

	// search in the fileContents
	if(ipRegExParser)
	{
		IIUnknownVectorPtr ipMatches = 
			ipRegExParser->Find(strFileContents.c_str(), VARIANT_FALSE, VARIANT_FALSE, VARIANT_FALSE);
		ASSERT_RESOURCE_ALLOCATION("ELI20687", ipMatches != __nullptr);

		long lSize = ipMatches->Size();
		for(long i = 0; i < lSize; i++)
		{
			// get the object pair from the match
			IObjectPairPtr ipObjPair = ipMatches->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI08576", ipObjPair != __nullptr);

			ITokenPtr ipMatch = ipObjPair->Object1;
			ASSERT_RESOURCE_ALLOCATION("ELI08577", ipMatch != __nullptr);
			vecMatches.push_back(MatchData(strFileName, asString(ipMatch->Value),
				ipMatch->StartPosition, ipMatch->EndPosition));
			g_ulNumMatches++;
		}
	}
	else
	{
		FindVariable(strFileContents, strSearchString, strFileName, vecMatches);
	}

	// prepare and write verbose output to console, file, or both
	if ((g_bVerbose || g_bVerboseToFile) && (vecMatches.size() > 0))
	{
		string strOutput = "";

		// reserve a chunk of room in the string for our data
		strOutput.reserve(vecMatches.size() * giMATCH_CHARS_EST);

		// build the verbose output
		vector<MatchData>::iterator it = vecMatches.begin();
		strOutput += it->toCSVString();
		for (; it != vecMatches.end(); it++)
		{
			strOutput += "\n" + it->toCSVString();
		}

		// output to console
		if (g_bVerbose)
		{
			cout << strOutput << endl;
		}

		// output to file
		if (g_bVerboseToFile)
		{
			ofstream fOut(g_strVerboseFile.c_str(), ios::app);
			if (!fOut)
			{
				UCLIDException ue("ELI20691", "Unable to open verbose file for writing!");
				ue.addDebugInfo("Verbose FileName", g_strVerboseFile);
				throw ue;
			}
			else
			{
				fOut << strOutput << endl;
			}

			fOut.close();
			waitForFileToBeReadable(g_strVerboseFile);
		}
	}
	
	// output totals
	if (g_bTotalPerFile)
	{
		cout << strFileName << "," << vecMatches.size() << endl;
	}
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To search through a directory looking for files that meet the specified
//			criteria (as set on the command line) and to search the files
//			that meet the criteria.  If the recursive flag has been set then will
//			also recursively search directories for files that meet the criteria
void getAndSearchAllFilesInDirectory(string strRoot, const string& strSearchString, 
									 long& rlTotalFiles, IRegularExprParserPtr ipRegExParser)
{
	try
	{
		// make sure root directory ends in a slash
		if (strRoot.find_first_of("/\\", strRoot.length()-1) == strRoot.npos)
		{
			strRoot += '\\';
		}


		FileIterator iter(strRoot + "*.*"); 
		// go through all the files/dirs in the current directory
		while (iter.moveNext())
		{
			string strFileName = iter.getFileName();
			if (strFileName != "." && strFileName != "..")
			{
				// if it is a directory and recursive option is true then recurse
				// and process all files in sub-directory
				if (iter.isDirectory())
				{
					if (g_bRecursive)
					{
						getAndSearchAllFilesInDirectory(strRoot+strFileName, 
							strSearchString, rlTotalFiles, ipRegExParser);
					}
				}
				// file found, process it
				else
				{
					// add path information to file name
					strFileName = strRoot + strFileName;
					string strExtension = getExtensionFromFullPath(strFileName, true);

					// if no extension specified then only operate on default extension
					// or if extension specified operate on matching files
					if ((g_strExtension.empty() && (strExtension == gstrDEFAULT_EXT))
						|| (strExtension == g_strExtension))
					{
						rlTotalFiles++;
						FindVariableInFile(strFileName, strSearchString, ipRegExParser);
					}
				}
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20689");
}
//--------------------------------------------------------------------------------------------------
long performSearch(string& rstrSearchString)
{
	// if the search expression is in a file, load the expression from the file
	if (g_bExpInFile)
	{
		rstrSearchString = getTextFileContentsAsString(buildAbsolutePath(rstrSearchString));
		
		// need to trim the blank lines from the beginning and ending of the
		// expression (any lines in the middle we assume are meant to be there)
		rstrSearchString = trim(rstrSearchString, "\r\n", "\r\n");
	}

	IRegularExprParserPtr ipRegExParser = __nullptr;
	if(g_bUseRegExp)
	{
		// Get a regular expression parser.
		IMiscUtilsPtr ipMiscUtils(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI22292", ipMiscUtils != __nullptr);
		ipRegExParser = ipMiscUtils->GetNewRegExpParserInstance("FindStringApp");
		ASSERT_RESOURCE_ALLOCATION("ELI20688", ipRegExParser != __nullptr);

		ipRegExParser->Pattern = rstrSearchString.c_str();
		ipRegExParser->IgnoreCase = asVariantBool(!g_bCaseSensitive);
	}

	long lTotalFilesSearched = 0;

	// treat input as directory
	if (g_bDirectory)
	{
		getAndSearchAllFilesInDirectory(g_strInput, rstrSearchString, 
			lTotalFilesSearched, ipRegExParser);
	}
	// treat the file as a list of files
	else if (g_bTreatAsList)
	{
		ifstream fList(g_strInput.c_str(), ios::in);
		if (!fList)
		{
			UCLIDException ue("ELI20690", "Unable to open list file for processing!");
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

				FindVariableInFile(strFileName, rstrSearchString, ipRegExParser);

				lTotalFilesSearched++;
			}
		}
	}
	// single file
	else
	{
		FindVariableInFile(g_strInput, rstrSearchString, ipRegExParser);

		lTotalFilesSearched++;
	}

	return lTotalFilesSearched;
}

//--------------------------------------------------------------------------------------------------
// The application object
//--------------------------------------------------------------------------------------------------
CWinApp theApp;

//--------------------------------------------------------------------------------------------------
// Main function
//--------------------------------------------------------------------------------------------------
int _tmain(int argc, TCHAR* argv[], TCHAR* envp[])
{
	string strExceptionLog = "";

	CoInitializeEx(NULL, COINIT_MULTITHREADED);
				
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
			validateLicense();

			if (argc <= 2)
			{
				displayUsage();
				return EXIT_FAILURE;
			}
			else if (argc < 3 || argc > 10)
			{
				displayUsage("Invalid number of arguments on the command line!\n");
				return EXIT_FAILURE;
			}

			// process the input file/directory argument
			getFileOrDirectory(argv[1]);

			// the search expression is always the second argument
			string strToFind = argv[2];

			int i = 3;
			for(i = 3; i < argc; i++)
			{
				string arg(argv[i]);
				makeLowerCase(arg);

				// treat file as list of files
				if (arg == "/fl")
				{
					g_bTreatAsList = true;
				}
				// treat expression as file containing expression
				else if (arg == "/el")
				{
					g_bExpInFile = true;
				}
				// treat expression as regular expression
				else if(arg == "/e")
				{
					g_bUseRegExp = true;
				}
				// output total matches for each file
				else if (arg == "/t")
				{
					// /v and /t are mutually exclusive
					if (g_bVerbose)
					{
						displayUsage("Invalid command line specification:\n/t and /v "
							"cannot be used together!");
						return EXIT_FAILURE;
					}

					g_bTotalPerFile = true;
				}
				// print verbose output to console
				else if(arg == "/v")
				{
					// /v and /t are mutually exclusive
					if (g_bTotalPerFile)
					{
						displayUsage("Invalid command line specification:\n/t and /v "
							"cannot be used together!");
						return EXIT_FAILURE;
					}
					g_bVerbose = true;
				}
				// print verbose output to file
				else if(arg == "/vf")
				{
					if (i+1 < argc)
					{
						// build path to file
						g_strVerboseFile = buildAbsolutePath(argv[++i]);
						if (getExtensionFromFullPath(g_strVerboseFile) == "")
						{
							UCLIDException ue("ELI20682", 
								"Invalid file name provided with /vf option!");
							ue.addDebugInfo("Verbose FileName", g_strVerboseFile);
							throw ue;
						}
					}
					else
					{
						displayUsage("/vf with no file argument!");
						return EXIT_FAILURE;
					}

					// if the output file already exists, delete it
					if (isValidFile(g_strVerboseFile))
					{
						deleteFile(g_strVerboseFile);
					}

					g_bVerboseToFile = true;
				}
				// recursively search directories
				else if(arg == "/r")
				{
					g_bRecursive = true;
				}
				// perform search in a case sensitive manner
				else if (arg == "/c")
				{
					g_bCaseSensitive = true;
				}
				// only search specific pages
				else if(arg == "/p")
				{
					if(i+1 >= argc)
					{
						displayUsage("/p requires a page range to be specified!");
						return EXIT_FAILURE;
					}

					g_bPagesSpecified = true;

					// get the page range
					string strPages = argv[i+1];
					vector<string> vecTokens;
					StringTokenizer::sGetTokens(strPages, "-", vecTokens);
					if(vecTokens.size() == 1)
					{
						if(vecTokens[0] == "")
						{
							displayUsage("/p - Page range specified incorrectly!");
							return EXIT_FAILURE;
						}
						g_nStartPage = asLong(vecTokens[0]);
						g_nEndPage = g_nStartPage;
					}
					else if(vecTokens.size() == 2)
					{
						if(vecTokens[0] == "")
						{
							displayUsage("/p - Page range specified incorrectly!");
							return EXIT_FAILURE;
						}
						g_nStartPage = asLong(vecTokens[0]);
						if(vecTokens[1] == "")
						{
							g_nEndPage = -1;
						}
						else
						{
							g_nEndPage = asLong(vecTokens[1]);
						}
					}
					i++;
				}
				// log exceptions to specified file rather than display them
				else if (arg == "/ef")
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
							UCLIDException ue("ELI20692", 
								"Exception file name must end with \".uex\"!");
							ue.addDebugInfo("Exception FileName", string(argv[i]));
							throw ue;
						}
					}
					else
					{
						displayUsage("/ef specified without <UEXFileName>!");
						return EXIT_FAILURE;
					}
				}
				// unrecognized argument
				else
				{
					displayUsage("Unrecognized command line argument!");
					return EXIT_FAILURE;
				}
			}
			
			// perform the search (returns the number of files searched)
			long numFilesSearched = performSearch(strToFind);
			
			// print totals to console
			cout << endl << "Total Files Searched: " << numFilesSearched << endl;
			cout << endl << "Total matches found: " << g_ulNumMatches << endl;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20683");
	}
	catch(UCLIDException& ue)
	{
		if (strExceptionLog.empty())
		{
			ue.display();
		}
		else
		{
			ue.log(strExceptionLog);
		}
	}

	CoUninitialize();

	return EXIT_SUCCESS;
}
