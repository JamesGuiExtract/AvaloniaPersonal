
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
#include "TemporaryFileName.h"

#include <io.h>
#include <fstream>
#include <list>
#include <memory>
#include <set>
#include <string>

//-------------------------------------------------------------------------------------------------
// Private Functions
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
// updates the vector with new page numbers
void updatePageNumbers(vector<int>& rvecPageNubmers, 
					   int nTotalNumberOfPages, 
					   int nStartPage, 
					   int nEndPage,
					   bool bThrowExceptionOnPageOutOfRange)
{
	bool bEndOutOfRange = nEndPage > nTotalNumberOfPages;
	if (bEndOutOfRange && bThrowExceptionOnPageOutOfRange)
	{
		UCLIDException ue("ELI29980", "Specified end page number is out of range.");
		ue.addDebugInfo("End Page Number", nEndPage);
		ue.addDebugInfo("Total Number Of Pages", nTotalNumberOfPages);
		throw ue;
	}

	int nLastPageNumber = (!bEndOutOfRange && nEndPage > 0)? nEndPage : nTotalNumberOfPages;
	for (int n=nStartPage; n<=nLastPageNumber; n++)
	{
		::vectorPushBackIfNotContained(rvecPageNubmers, n);
	}
}
//-------------------------------------------------------------------------------------------------
// updates the vector with new page numbers. nPageNumber > 0
// nPageNumber - this argument could be a single page number if bLastPagesDefined == false,
//				 it could be last X number of pages if bLastPagesDefined == true
void updatePageNumbers(vector<int>& rvecPageNubmers, 
					   int nTotalNumberOfPages, 
					   int nPageNumber,
					   bool bThrowExceptionOnPageOutOfRange,
					   bool bLastPagesDefined = false)
{
	// Check if the page number is valid
	bool bPageOutOfRange = nPageNumber > nTotalNumberOfPages;
	if (bPageOutOfRange)
	{
		// Throw exception if specified
		if (bThrowExceptionOnPageOutOfRange)
		{
			UCLIDException ue("ELI29981", "Specified page number is out of range.");
			ue.addDebugInfo("Page Number", nPageNumber);
			ue.addDebugInfo("Total Number Of Pages", nTotalNumberOfPages);
			throw ue;
		}
		// Check if not last page defined just return
		else if (!bLastPagesDefined)
		{
			return;
		}
	}
	if (bLastPagesDefined)
	{
		int n = !bPageOutOfRange ? (nTotalNumberOfPages - nPageNumber) + 1 : 1;
		for (; n<=nTotalNumberOfPages; n++)
		{
			::vectorPushBackIfNotContained(rvecPageNubmers, n);
		}
	}
	else
	{
		::vectorPushBackIfNotContained(rvecPageNubmers, nPageNumber);
	}
}
//-------------------------------------------------------------------------------------------------
void fillPageNumberVector(vector<int>& rvecPageNumbers,
					int nTotalNumberOfPages, const string strSpecifiedPageNumbers,
					bool bThrowExceptionOnPageOutOfRange)
{
	// Assume before this methods is called, the caller has already called validatePageNumbers()
	vector<string> vecTokens;

	// parse string into tokens
	StringTokenizer::sGetTokens(strSpecifiedPageNumbers, ",", vecTokens);
	size_t nLength = vecTokens.size();
	for (size_t n=0; n < nLength; n++)
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
				updatePageNumbers(rvecPageNumbers, nTotalNumberOfPages, nStartPage, nEndPage,
					bThrowExceptionOnPageOutOfRange);
			}
			else
			{
				// last X number of pages
				updatePageNumbers(rvecPageNumbers, nTotalNumberOfPages, nEndPage,
					bThrowExceptionOnPageOutOfRange, true);
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
			updatePageNumbers(rvecPageNumbers, nTotalNumberOfPages, nPageNumber,
				bThrowExceptionOnPageOutOfRange);
		}
	}
}

//-------------------------------------------------------------------------------------------------
// Public Functions
//-------------------------------------------------------------------------------------------------
void autoEncryptFile(const string& strFile, const string& strRegistryKey)
{
	try
	{
		try
		{
			string strRegFullKey = strRegistryKey;

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

			// Protect access to the IConfigurationSettingsPersistenceMgr
			{
				static CMutex mutex;
				CSingleLock lg(&mutex, TRUE );

				static unique_ptr<IConfigurationSettingsPersistenceMgr> pSettings(__nullptr);
				if (pSettings.get() == __nullptr)
				{
					pSettings = unique_ptr<IConfigurationSettingsPersistenceMgr>(
						new RegistryPersistenceMgr(HKEY_CURRENT_USER, ""));
					ASSERT_RESOURCE_ALLOCATION("ELI08827", pSettings.get() != __nullptr);
				}

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
			}

			// AutoEncrypt must be ON in registry to continue
			if (!bAutoEncryptOn)
			{
				return;
			}

			// Get the base file time stamp
			CTime tmBaseFile(getFileModificationTimeStamp(strBaseFile));

			// If ETF already exists, compare the last modification
			// on both the base file and the etf file
			if (isFileOrFolderValid(strFile.c_str()))
			{
				// Compare timestamps
				CTime tmETFFile(getFileModificationTimeStamp(strFile));
				if (tmBaseFile <= tmETFFile)
				{
					// no need to encrypt the file again
					return;
				}
			}

			// Use a temporary file alongside the target etf file to use a flag that the file is currently
			// being encrypted.
			{
				unique_ptr<TemporaryFileName> upTempFile(__nullptr);

				// If there is a temporary encryption file it means the file is currently being encrypted.
				// Wait up to 20 seconds for the file to go away.
				string strTempEncryptionFile = strFile + ".encryption.tmp";
				int nWaitTime = 0;
				while (true)
				{
					if (!isValidFile(strTempEncryptionFile))
					{
						try
						{
							// Create the temporary file
							upTempFile.reset(new TemporaryFileName(strTempEncryptionFile));

							// Temporary file was created, break from the loop
							// (this skips the sleep statement below)
							break;
						}
						catch (...)
						{
							upTempFile.reset(__nullptr);
						}
					}

					if (nWaitTime > 20000)
					{
						UCLIDException ue("ELI07701", "Timeout waiting for file to be encyrypted!");
						ue.addDebugInfo("Filename", strFile);
						ue.addDebugInfo("Temp filename", strTempEncryptionFile);
						throw ue;
					}

					Sleep(100);
					nWaitTime += 100;
				}

				// If ETF already exists, compare the last modification
				// on both the base file and the etf file
				if (::isFileOrFolderValid(strFile.c_str()))
				{
					// Compare timestamps
					CTime tmETFFile(getFileModificationTimeStamp(strFile));
					if (tmBaseFile <= tmETFFile)
					{
						// no need to encrypt the file again
						return;
					}
				}

				// Encrypt the base file to the temporary filename
				static EncryptedFileManager efm;
				efm.encrypt(strBaseFile, strTempEncryptionFile);

				// Once the encryption process is complete, copy it into the real destination.
				copyFile(strTempEncryptionFile, strFile);

				// strTempEncryptionFile goes out of scope here, allowing auto-encryption from other threads
				// and processes access here.
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30118");
	}
	catch(UCLIDException& uex)
	{
		UCLIDException ue("ELI30119", "Unable to update encrypted file.", uex);
		ue.addDebugInfo("File To Encrypt", strFile);
		throw ue;
	}
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
vector<int> getPageNumbers(int nTotalNumberOfPages, const string& strSpecifiedPageNumbers,
						   bool bThrowExceptionPageOutOfRange)
{
	// vector of page numbers in ascending order, no duplicates allowed
	vector<int> vecSortedPageNumbers;
	fillPageNumberVector(vecSortedPageNumbers, nTotalNumberOfPages, strSpecifiedPageNumbers,
		bThrowExceptionPageOutOfRange);

	// sort the vector in ascending order
	sort(vecSortedPageNumbers.begin(), vecSortedPageNumbers.end());

	return vecSortedPageNumbers;
}
//-------------------------------------------------------------------------------------------------
set<int> getPageNumbersAsSet(int nTotalNumberOfPages, const string& strSpecifiedPageNumbers,
						   bool bThrowExceptionPageOutOfRange)
{
	// vector of page numbers in ascending order, no duplicates allowed
	vector<int> vecPageNumbers;
	fillPageNumberVector(vecPageNumbers, nTotalNumberOfPages, strSpecifiedPageNumbers,
		bThrowExceptionPageOutOfRange);

	set<int> setPageNumbers;
	setPageNumbers.insert(vecPageNumbers.begin(), vecPageNumbers.end());

	return setPageNumbers;
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