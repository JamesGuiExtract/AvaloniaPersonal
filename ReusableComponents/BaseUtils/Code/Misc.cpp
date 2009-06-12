
#include "stdafx.h"
#include "Misc.h"
#include "EncryptedFileManager.h"
#include "StringTokenizer.h"
#include "CommentedTextFileReader.h"
#include "RegistryPersistenceMgr.h"
#include "ExtractMFCUtils.h"
#include "cpputil.h"
#include "UCLIDException.h"
#include "stringCSIS.h"

#include <io.h>
#include <fstream>
#include <list>
#include <memory>
#include <set>
#include <string>


//-------------------------------------------------------------------------------------------------
// Global Vars
//-------------------------------------------------------------------------------------------------
const static string gstrImport = "#import";

//-------------------------------------------------------------------------------------------------
// Private Functions
//-------------------------------------------------------------------------------------------------
void convertFileToLines(const string& strInput, bool bIsFilename, vector<string>& rvecLines)
{
	if (bIsFilename)
	{
		// first make sure the file exists
		if (!isValidFile(strInput))
		{
			UCLIDException ue("ELI07158", "Input file doesn't exist.");
			ue.addDebugInfo("Input File", strInput);
			ue.addWin32ErrorInfo();
			throw ue;
		}

		// If input file is an etf file
		if (getExtensionFromFullPath(strInput, true) == ".etf")
		{
			// Open an input file, which is encrypted
			EncryptedFileManager efm;
			// decrypt the file
			rvecLines = efm.decryptTextFile(strInput);
		}
		else
		{
			// treat the input file as ASCII text file
			ifstream ifs(strInput.c_str());
			
			while (!ifs.eof())
			{
				string strLine("");

				getline(ifs, strLine);

				if (strLine.empty())
				{
					continue;
				}

				// save the line in the vector
				rvecLines.push_back(strLine);
			}
		}
	}
	else	// if input is a block of text
	{
		// delimiter is line feed
		StringTokenizer::sGetTokens(strInput, "\r\n", rvecLines);
	}
}
//-------------------------------------------------------------------------------------------------
string getRegExpFromLines(CommentedTextFileReader& ctfr, const string& strRootFile,
						  bool bAutoEncrypt, const string& strAutoEncryptKey)
{
	string strRegExp = "";
	vector<string> vecLineNoComments;
	while ( !ctfr.reachedEndOfStream())
	{
		string strCurrent = ctfr.getLineText();
		// check for #import statements
		unsigned long ulStartImport = strCurrent.find(gstrImport);
		if (ulStartImport != string::npos)
		{
			unsigned long ulStartFileName = strCurrent.find_first_not_of(" \t", 
				ulStartImport + gstrImport.size());
			if (ulStartFileName == string::npos)
			{
				UCLIDException ue("ELI09752", "Invalid #import statememt.");
				ue.addDebugInfo("string", strCurrent);
				throw ue;
			}

			// if no root folder is specified then #import statements are invalid
			// and we throw an exception
			if (strRootFile == "\\")
			{
				UCLIDException ue("ELI10018", "#import specified with no root folder.");
				throw ue;
			}

			string strRelName = strCurrent.substr(ulStartFileName);
			string strFullFileName = getAbsoluteFileName(strRootFile, strRelName, false);
			// AutoEncrypt if necessary
			if (bAutoEncrypt)
			{
				autoEncryptFile(strFullFileName, strAutoEncryptKey);
			}
			string strExp = getRegExpFromFile(strFullFileName);
			// include anything on the line before the #import
			string strLineBegin = strCurrent.substr(0, ulStartImport);
			vecLineNoComments.push_back(strLineBegin + strExp);
		}
		else if ( !strCurrent.empty())
		{
			vecLineNoComments.push_back( strCurrent );
		}
	}
	// Build the regular expression string, removing any whitespace( \f\n\r\t\v)
	strRegExp = asString(vecLineNoComments, true );

	return strRegExp;
}

//-------------------------------------------------------------------------------------------------
// Public Functions
//-------------------------------------------------------------------------------------------------
string getRegExpFromFile(const string& strFilename, bool bAutoEncrypt,
						 const string& strAutoEncryptKey)
{
	// String to contain the return value
	string strRegExp = "";

	vector<string> vecLines;
	if ( bAutoEncrypt )
	{
		autoEncryptFile( strFilename, strAutoEncryptKey );
	}
	convertFileToLines(strFilename, true, vecLines);
	
	// Extract comments from the loaded vector of lines
	CommentedTextFileReader ctfr( vecLines, "//");
	strRegExp = getRegExpFromLines(ctfr, strFilename, bAutoEncrypt, strAutoEncryptKey);

	return strRegExp;
}
//-------------------------------------------------------------------------------------------------
string getRegExpFromText(const string& strText, const string& strRootFolder,
						 bool bAutoEncrypt, const string& strAutoEncryptKey)
{
	// String to contain the return value
	string strRegExp = "";

	vector<string> vecLines;
	convertFileToLines(strText, false, vecLines);
	
	// Extract comments from the loaded vector of lines
	CommentedTextFileReader ctfr( vecLines, "//");
	strRegExp = getRegExpFromLines(ctfr, strRootFolder + "\\", bAutoEncrypt, strAutoEncryptKey);

	return strRegExp;
}
//-------------------------------------------------------------------------------------------------
void autoEncryptFile(const string& strFile, const string& strRegistryKey)
{
	string strRegFullKey = strRegistryKey;
	
	static auto_ptr<IConfigurationSettingsPersistenceMgr> pSettings(NULL);
	if (pSettings.get() == NULL)
	{
		pSettings = auto_ptr<IConfigurationSettingsPersistenceMgr>(
			new RegistryPersistenceMgr(HKEY_CURRENT_USER, ""));
		ASSERT_RESOURCE_ALLOCATION("ELI08827", pSettings.get() != NULL);
	}

	// compute the folder and keyname from the registry key
	// for use with the RegistryPersistenceMgr class
	unsigned long ulLastPos = strRegFullKey.find_last_of('\\');
	if (ulLastPos == string::npos)
	{
		UCLIDException ue("ELI08826", "Invalid registry key!");
		ue.addDebugInfo("RegKey", strRegFullKey);
		throw ue;
	}
	string strRegFolder(strRegFullKey, 0, ulLastPos);
	string strRegKey(strRegFullKey, ulLastPos + 1, 
		strRegFullKey.length() - ulLastPos - 1);

	// if the extension is not .etf, then just return
	string strExt = getExtensionFromFullPath(strFile, true);
	if (strExt != ".etf")
	{
		return;
	}

	// Get name for the base file,
	// for instance, if strFile = "XYZ.dcc.etf" 
	// then the base file will be "XYZ.dcc"
	string strBaseFile = getPathAndFileNameWithoutExtension(strFile);

	// File must exist to continue
	if (!isFileOrFolderValid(strBaseFile.c_str()))
	{
		return;
	}

	bool bAutoEncryptOn = false;

	// check if the registry key for auto-encrypt exists.
	// if it does not, create the key with a default value of "0"
	if (!pSettings->keyExists(strRegFolder, strRegKey))
	{
		pSettings->createKey(strRegFolder, strRegKey, "0");
	}
	else
	{
		// get the key value. If it is "1", then auto-encrypt 
		// setting is on
		if (pSettings->getKeyValue(strRegFolder, strRegKey) == "1")
		{
			bAutoEncryptOn = true;
		}
	}

	// AutoEncrypt must be ON in registry to continue
	if (!bAutoEncryptOn)
	{
		return;
	}

	// If ETF already exists, compare the last modification
	// on both the base file and the etf file
	if (::isFileOrFolderValid(strFile.c_str()))
	{
		// Compare timestamps
		CTime tmBaseFile(getFileModificationTimeStamp(strBaseFile));
		CTime tmETFFile(getFileModificationTimeStamp(strFile));
		if (tmBaseFile <= tmETFFile)
		{
			// no need to encrypt the file again
			return;
		}
	}

	// Encrypt the base file
	static EncryptedFileManager efm;
	efm.encrypt(strBaseFile, strFile);
}
//-------------------------------------------------------------------------------------------------
vector<string> convertFileToLines(const string& strFilename)
{
	vector<string> vecLines;
	convertFileToLines( strFilename, true, vecLines );
	return vecLines;
}
//-------------------------------------------------------------------------------------------------
void writeLinesToFile(const vector<string>& vecLines, const string& strFileName, bool bAppend)
{
	// Open the output file (open for append mode if bAppend is true)
	ofstream fOut(strFileName.c_str(), (bAppend ? ios::app : ios::out));

	// Ensure the file was opened
	if (fOut.fail())
	{
		UCLIDException uex("ELI24708", "Unable to open file for output!");
		uex.addDebugInfo("File To Output", strFileName);
		throw uex;
	}

	// For each string in the vector, write it to the file
	for (vector<string>::const_iterator it = vecLines.begin(); it != vecLines.end(); it++)
	{
		fOut << (*it) << endl;
	}
	
	// Close the file
	fOut.close();

	// Wait until the file is readable
	waitForFileToBeReadable(strFileName);
}
//-------------------------------------------------------------------------------------------------
// Parse strPageRange, returns start and end page. nStartPage could be 0 (which means it's empty). 
// nEndPage must be greater than 0 if start page is empty
// Require: strPageRange must have one and only one dash (-)
// 
void getStartAndEndPage(const string& strPageRange, int& nStartPage, int& nEndPage)
{
	// assume this is a range of page numbers, or last X number of pages
	// Further parse the string with delimiter as '-'
	vector<string> vecTokens;
	StringTokenizer::sGetTokens(strPageRange, "-", vecTokens);
	if (vecTokens.size() != 2)
	{
		UCLIDException ue("ELI10262", "Invalid format for page range or last X number of pages.");
		ue.addDebugInfo("String", strPageRange);
		throw ue;
	}
	
	string strStartPage = ::trim(vecTokens[0], " \t", " \t");
	// start page could be empty
	nStartPage = 0;
	if (!strStartPage.empty())
	{
		if(strStartPage == "0")
		{
			UCLIDException ue("ELI12950", "Starting page can not be zero.");
			ue.addDebugInfo("String", strPageRange);
			throw ue;
		}
		// make sure the start page is a number

		nStartPage = ::asLong(strStartPage);
	}
	
	string strEndPage = ::trim(vecTokens[1], " \t", " \t");
	// end page must not be empty if start page is empty
	if (strStartPage.empty() && strEndPage.empty())
	{
		UCLIDException ue("ELI10263", "Starting and ending page can't be both empty.");
		ue.addDebugInfo("Page range", strPageRange);
		throw ue;
	}
	else if (!strStartPage.empty() && strEndPage.empty())
	{
		// if start page is not empty, but end page is empty, for instance, 2-,
		// then the user wants to get all pages from the starting page till the end
		// Set ending page as 0
		nEndPage = 0;
		return;
	}
	
	nEndPage = ::asLong(strEndPage);
	
	// make sure the start page number is less than the end page number
	if (nStartPage >= nEndPage)
	{
		UCLIDException ue("ELI10264", "Start page number must be less than the end page nubmer.");
		ue.addDebugInfo("Page range", strPageRange);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void validatePageNumbers(const string& strSpecifiedPageNumbers)
{
	if (strSpecifiedPageNumbers.empty())
	{
		throw UCLIDException("ELI10260", "Specified page number string is empty.");
	}

	vector<string> vecTokens;
	// parse string into tokens
	StringTokenizer::sGetTokens(strSpecifiedPageNumbers, ",", vecTokens);
	for (unsigned int n = 0; n<vecTokens.size(); n++)
	{
		// trim any leading/trailing white spaces
		string strToken = ::trim(vecTokens[n], " \t", " \t");

		// if the token contains a dash
		if (strToken.find("-") != string::npos)
		{
			// start page could be empty
			int nStartPage = 0, nEndPage = 0;
			getStartAndEndPage(strToken, nStartPage, nEndPage);
		}
		else
		{
			// assume this is a page number
			int nPageNumber = ::asLong(strToken);
			if (nPageNumber <= 0)
			{
				UCLIDException ue("ELI10261", "Invalid page number.");
				ue.addDebugInfo("Page Number", nPageNumber);
				throw ue;
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
// updates the vector with new page numbers
void updatePageNumbers(vector<int>& rvecPageNubmers, 
					   int nTotalNumberOfPages, 
					   int nStartPage, 
					   int nEndPage)
{
	int nLastPageNumber = (nEndPage < nTotalNumberOfPages && nEndPage > 0)? 
							nEndPage : nTotalNumberOfPages;
	for (int n=nStartPage; n<=nLastPageNumber; n++)
	{
		::vectorPushBackIfNotContained(rvecPageNubmers, n);
	}
}
//-------------------------------------------------------------------------------------------------
// updates the vector with new page numbers. nPageNubmer > 0
// nPageNubmer - this argument could be a single page number if bLastPagesDefined == false,
//				 it could be last X number of pages if bLastPagesDefined == true
void updatePageNumbers(vector<int>& rvecPageNubmers, 
					   int nTotalNumberOfPages, 
					   int nPageNubmer,
					   bool bLastPagesDefined = false)
{
	if (bLastPagesDefined)
	{
		int n = nPageNubmer < nTotalNumberOfPages ? nTotalNumberOfPages-nPageNubmer+1 : 1;
		for (; n<=nTotalNumberOfPages; n++)
		{
			::vectorPushBackIfNotContained(rvecPageNubmers, n);
		}
	}
	else
	{
		if (nPageNubmer > nTotalNumberOfPages)
		{
			return;
		}
		
		::vectorPushBackIfNotContained(rvecPageNubmers, nPageNubmer);
	}
}
//-------------------------------------------------------------------------------------------------
vector<int> getPageNumbers(int nTotalNumberOfPages, const string& strSpecifiedPageNumbers)
{
	// vector of page numbers in ascending order, no duplicates allowed
	vector<int> vecSortedPageNumbers;

	// Assume before this methods is called, the caller has already called validatePageNumbers()
	vector<string> vecTokens;
	// parse string into tokens
	StringTokenizer::sGetTokens(strSpecifiedPageNumbers, ",", vecTokens);
	for (unsigned int n=0; n<vecTokens.size(); n++)
	{
		// trim any leading/trailing white spaces
		string strToken = ::trim(vecTokens[n], " \t", " \t");

		// if the token contains a dash
		if (strToken.find("-") != string::npos)
		{
			// start page could be empty
			int nStartPage = 0, nEndPage = 0;
			getStartAndEndPage(strToken, nStartPage, nEndPage);

			if (nStartPage > 0 && 
				(nEndPage > nStartPage || nEndPage <= 0))
			{
				// range of pages
				updatePageNumbers(vecSortedPageNumbers, nTotalNumberOfPages, nStartPage, nEndPage);
			}
			else
			{
				// last X number of pages
				updatePageNumbers(vecSortedPageNumbers, nTotalNumberOfPages, nEndPage, true);
			}
		}
		else
		{
			// assume this is a page number
			int nPageNumber = ::asLong(strToken);
			if (nPageNumber <= 0)
			{
				UCLIDException ue("ELI19385", "Invalid page number.");
				ue.addDebugInfo("Page Number", nPageNumber);
				throw ue;
			}

			// single page number
			updatePageNumbers(vecSortedPageNumbers, nTotalNumberOfPages, nPageNumber);
		}
	}

	// sort the vector in ascending order
	sort(vecSortedPageNumbers.begin(), vecSortedPageNumbers.end());

	return vecSortedPageNumbers;
}
//-------------------------------------------------------------------------------------------------
void getFileListFromFile(const string &strFileName, vector<string> &rvecFiles, bool bUnique)
{
	if ( !isValidFile( strFileName ))
	{
		UCLIDException ue("ELI13414", "Not a valid file." );
		ue.addDebugInfo( "FileName", strFileName );
		throw ue;
	}
	set<stringCSIS> setFiles;

	// add the files that are already in the vector to the set 
	// this is done to remove the duplicates
	if ( bUnique && rvecFiles.size() > 0 )
	{
		for each ( string s in rvecFiles )
		{
			setFiles.insert( stringCSIS(s, false));
		}
	}

	// Set up File list 
	ifstream ifs(strFileName.c_str());
	CommentedTextFileReader ctfrFiles( ifs );

	if ( ifs.good() )
	{
		// load the list of files
		list<string> listFiles;
		convertFileToListOfStrings(ifs, listFiles);
		ctfrFiles.sGetUncommentedFileContents(listFiles);
		for each ( string str in listFiles )
		{
			if ( bUnique )
			{
				pair< set<stringCSIS>::iterator, bool > pr;
				pr = setFiles.insert( stringCSIS( str, false ));
				if  ( pr.second == true )
				{
					rvecFiles.push_back( str );
				}
			}
			else
			{
				rvecFiles.push_back( str );
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
