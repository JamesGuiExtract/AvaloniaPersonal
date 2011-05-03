//==================================================================================================
// COPYRIGHT UCLID SOFTWARE, LLC. 2000
//
// FILE:	FileDirectorySearcher.h
//
// PURPOSE:	Based on the user input file name(with directory), find all the files that under the 
//			same root and have the same name, extension, etc. (there might be wild cards in the file
//			name). This class can do a recursive search till reaches the end of the directory.
//
// NOTES:
//
// AUTHOR:	Duan Wang
//
//==================================================================================================
//
//==================================================================================================

#include "stdafx.h"
#include "FileDirectorySearcher.h"
#include "cpputil.h"
#include "FileIterator.h"
#include "UCLIDException.h"
#include "VectorOperations.h"

using namespace std;

//--------------------------------------------------------------------------------------------------
// Public member functions
//--------------------------------------------------------------------------------------------------
void FileDirectorySearcherBase::findFiles(const string& strFileSpec, bool bRecursive)
{
	//strings to hold directory part and filename as part of the input string
	string strPath = "";
	string strFileName = "";
	//strPath now has the valid dir, and strFileName has the file name only
	parseFileName(strFileSpec, strPath, strFileName);

	getFilesFromCurDirectory( strFileSpec, strPath );

	if( !shouldStop() && bRecursive)
	{
		vector<string> vecDirectories = getSubDirectories(strPath, false);
		vector<string>::iterator iter = vecDirectories.begin();
		while(iter != vecDirectories.end())
		{
			findFiles((*iter)+"\\"+strFileName, true);
			iter++;
		}
	}

	return;

}
//--------------------------------------------------------------------------------------------------
vector<string> FileDirectorySearcherBase::getSubDirectories(const string& strCurDir, bool bRecursive)
{
	vector<string> vecDirectories;
	string strPath;

	unsigned int uiPos = strCurDir.find_last_not_of(" \t");
	if (uiPos != string::npos)
	{
		if (strCurDir[uiPos] == '\\')
		{
			//search for all posible subdirectories under current dir.
			// strCurDir end with "\\"
			strPath = strCurDir;
		}
		else
		{
			//strCurDir doesnot have "\\" at the end
			strPath = strCurDir + "\\";
		}
	}
	else
	{
		UCLIDException ue("ELI00331", "Invalid Directory.");
		ue.addDebugInfo("Path", strPath);
		throw ue;
	}

	FileIterator iter(strPath + "*");

	// true if one or more files and/or paths are found
	bool bFindFile = iter.moveNext();
	if (!bFindFile)
	{
		// No more files or directories exist under the directory
		UCLIDException ue("ELI00332", "Invalid Directory.");
		ue.addDebugInfo("Path", strPath);
		throw ue;
	}

	//put all found files(and valid directories into the vector)
	while (!shouldStop() && bFindFile)
	{
		// Check that the file name is not "." or ".."
		// Must be a non-system directory
		string strFileName = iter.getFileName();
		if (strFileName != "." && strFileName != ".." && iter.isDirectory() && !iter.isSystemFile())
		{
				//put the string in the struct(including the whole path)
				string strTempDir = strPath + strFileName;
				vecDirectories.push_back(strTempDir);	
				if (bRecursive)
				{
					addVectors(vecDirectories, getSubDirectories(strTempDir, bRecursive));
				}
		}
		bFindFile = iter.moveNext();
	}
	//remember to put a try..catch in the caller's (or main)body
	iter.reset();

	return vecDirectories;
}

//--------------------------------------------------------------------------------------------------
// Private member functions
//--------------------------------------------------------------------------------------------------
void FileDirectorySearcherBase::parseFileName(const string& strFile, 
										  string& strPath, 
										  string& strFileName)
{
	//strings to hold directory part and filename part of the input string
	strPath = "";
	strFileName = "";
	//parse out the file name only
	//look for the start char pos of the filename from the whole input string
	unsigned int uiPrevPos = strFile.find_last_of("\\");
	if (uiPrevPos != string::npos) 
	{
		//look for the dot sign as part of the filename
		unsigned int uiNextPos = strFile.find_first_of( ".", uiPrevPos + 1 );
		if (uiNextPos == string::npos)
		{
			uiNextPos = strFile.find_first_of( "*", uiPrevPos + 1 );
		}

		if (uiNextPos != string::npos)
		{
			uiNextPos = strFile.find_last_not_of(" \t");
			if (uiNextPos != string::npos)
			{
				//filename to store the file caller is searching for
				//start from pos next to "\\"(excluded) till the end of the filename
				strFileName = strFile.substr( uiPrevPos + 1, uiNextPos - uiPrevPos + 2 );
			}
		}
		//string start pos till the the last "\\"(included)
		strPath = strFile.substr( 0, uiPrevPos + 1 );
	}
}
//--------------------------------------------------------------------------------------------------
void FileDirectorySearcherBase::getFilesFromCurDirectory(const string& strFileSpec,
														 const string& strPath)
{
	FileIterator iter(strFileSpec);

	//put all found files(and valid directories into the vector)
	while(!shouldStop() && iter.moveNext())
	{
		// Check that the file name is not "." or ".." and no directories included
		string strFileName = iter.getFileName();
		if (strFileName != "." && strFileName != ".." && !iter.isDirectory())
		{
			//put the string in the struct(including the whole path)
			addFile(strPath + strFileName);
		}
	}
	//remember to put a try..catch in the caller's (or main)body
	iter.reset();

	return;
}

//--------------------------------------------------------------------------------------------------
// FileDirectorySearcher
//--------------------------------------------------------------------------------------------------
std::vector<std::string> FileDirectorySearcher::searchFiles(const std::string& strFileSpec, bool bRecursive)
{
	// Clear out the contents of the current vector
	m_vecFiles.clear();
	findFiles( strFileSpec, bRecursive );
	// return the vector of files
	return m_vecFiles;
}
//--------------------------------------------------------------------------------------------------
void FileDirectorySearcher::addFile(const string& strFile)
{
	// put the file on the vector of files
	m_vecFiles.push_back(strFile);
}
//--------------------------------------------------------------------------------------------------
