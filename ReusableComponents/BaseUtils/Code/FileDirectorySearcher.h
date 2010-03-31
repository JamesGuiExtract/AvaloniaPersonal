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

#pragma once

#include "BaseUtils.h"

#include <vector>
#include <string>

using namespace std;

class EXPORT_BaseUtils FileDirectorySearcherBase
{
public:
	//==============================================================================================
	// PURPOSE:	Find all files and pass them to the addFile method
	//
	// REQUIRE:	Recursive search if bRecursive is true
	//
	// PROMISE: To call addFile method with all of the found files 
	//
	// PARAMETERS:  
	//			strFileSpec: Holds the name of the input string of file directory
	//			bRecursive:	If true, do recursive search deep into the directory from input till 
	//						reaches the end
	//
	void findFiles(const string& strFileSpec, bool bRecursive);

	//==============================================================================================
	// PURPOSE:	Find all subdirectories under the current directory
	//
	// REQUIRE:	No recursion required, only one level
	//
	// PROMISE:	All directories under current dir will be listed in the vec
	//
	// PARAMETERS:  
	//			strCurDir: Current directory
	//			bRecursive: if false -- get all subfolders one level down
	//						if true --  get all subfolders recursively till exhausted
	//
	vector<string> getSubDirectories(const string& strCurDir, bool bRecursive);

protected:
	// This function is to be overridden to place the file in the collection being used
	virtual void addFile ( const string &strFile ) = 0;
	
	// This function is called within the file search to determine if the search should be stopped
	// This should be overridden if the search should be stoppable
	virtual bool shouldStop()
	{
		return false;
	};

private:

	//==============================================================================================
	// PURPOSE:	Parse out the valid directory and file name from the input strFileSpec
	//
	// REQUIRE:  
	//
	// PROMISE:  
	//
	// PARAMETER:
	//			strFile: File (includes path and filename) for parsing
	//			strPath: Returns valid file path
	//			strFileName: Return file name
	//
	void parseFileName(const string& strFile, string& strPath, string& strFileName);
	//==============================================================================================
	// PURPOSE:	Find all files(and directories) from current dir and put them in a vector
	//
	// REQUIRE:	No recursion, require files type meet user criteria
	//
	// PROMISE: Return vector contains all qualified files in current directory
	//
	// PARAMETERS:  
	//			strFileSpec: Holds the name of the input string of file directory
	//			strPath:	Holds the path parsed from the strFileSpec value
	//
	void getFilesFromCurDirectory(const string& strFileSpec, const string& strPath);
	//==============================================================================================
};

//==================================================================================================
// CLASS:	FileDirectorySearcher
//
// PURPOSE: Search for file(s) (and directories) which meet the user's searching criteria
//
// REQUIRE:
// 
// INVARIANTS:
//
// EXTENSIONS:
//
// NOTES:	
//
//==================================================================================================
class EXPORT_BaseUtils FileDirectorySearcher : public FileDirectorySearcherBase
{
public:
	//==============================================================================================
	// PURPOSE:	Find all files and put them in a vector
	//
	// REQUIRE:	Recursive search if bRecursive is true
	//
	// PROMISE: Return vector containing all 
	//
	// PARAMETERS:  
	//			strFileSpec: Holds the name of the input string of file directory
	//			bRecursive:	If true, do recursive search deep into the directory from input till 
	//						reaches the end
	//
	vector<string> searchFiles(const string& strFileSpec, bool bRecursive);

protected:
	// Override addFile to put the file in the m_vecFiles vector
	virtual void addFile( const string &strFile );

private:
	vector<string> m_vecFiles;

};
