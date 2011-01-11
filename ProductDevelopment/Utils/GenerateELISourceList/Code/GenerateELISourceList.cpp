// GenerateELISourceList.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <windows.h>

// windows.h must be included before cppUtil.h
#include <cppUtil.h>

#include <string>
#include <vector>
#include <map>
#include <set>
#include <algorithm>
#include <fstream>
#include <iostream>
#include <ObjBase.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Local functions
//-------------------------------------------------------------------------------------------------
string getFileExtension(const string& strFileName)
{
	char zDrive[_MAX_DRIVE], zPath[_MAX_PATH], zFileName[_MAX_FNAME], zExt[_MAX_EXT];
	// Break a path name into components.
	_splitpath_s(strFileName.c_str(), zDrive, zPath, zFileName, zExt);

	return string(zExt);
}
//-------------------------------------------------------------------------------------------------
// find files with specified extension within the strRootFolder recursively
vector<string> findFilesRecursive(const string& strRootFolder, 
								  const vector<string>& vecFileNameExtensions)
{
	WIN32_FIND_DATA fileInfo;
	HANDLE fileHandle;
	BOOL bFindFileResult;
	vector<string> vecAllFiles;

	fileHandle = FindFirstFile( (strRootFolder + "*.*").c_str(), &fileInfo);
	
	if (fileHandle == INVALID_HANDLE_VALUE)
	{
		return vecAllFiles; 
	}

	// go through all the files and dirs in the current directory
	do 
	{
		// this file name might be file name or folder name
		string strFileName(fileInfo.cFileName);

		// Check that the file name is not "." or ".."
		if( (strFileName != ".") && (strFileName != ".."))
		{
			// The attribute stored in fileInfo is read to cofirm the filetype: file or directory
			bool bFileIsADirectory = (fileInfo.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) > 0;

			// if the file is actually a folder, then call on this function to get
			// all files out from this folder recursively
			if (bFileIsADirectory)
			{
				string strSubDir = strRootFolder + strFileName + "\\";

				vector<string> vecSubFolderFiles = findFilesRecursive(strSubDir, vecFileNameExtensions);

				// append the vecSubFolerFiles to vecAllFiles
				vecAllFiles.insert(vecAllFiles.end(), vecSubFolderFiles.begin(), vecSubFolderFiles.end());
			}
			else
			{
				string strFoundFile(getFileExtension(strFileName));
				vector<string>::const_iterator it = find(vecFileNameExtensions.begin(), vecFileNameExtensions.end(), strFoundFile);
				if (it != vecFileNameExtensions.end())
				{
					// can't just delete the files now because the handle is still open, so we add them to a local vector of files
					// to be deleted.
					vecAllFiles.push_back(strRootFolder + strFileName);
				}
			}
		}

		// find next
		bFindFileResult = FindNextFile(fileHandle, &fileInfo );
	}
	while (bFindFileResult);
	
	// close the find file handle before trying to delete any files or do any recursion
	if (FindClose(fileHandle) == 0)
	{
		
	}

	return vecAllFiles;
}
//-------------------------------------------------------------------------------------------------
// get a vector of file names inside the specified folder
// Files include: *.cpp, *.h, *.hpp, *.c, *.cls, *.bas, *.frm, *.vbs, *.cs
vector<string> getFileNamesIn(const string& strFullFolderName)
{
	string strRootFolder(strFullFolderName+"\\");
	vector<string> vecFileNameExtensions;
	vecFileNameExtensions.push_back(".cpp");
	vecFileNameExtensions.push_back(".h");
	vecFileNameExtensions.push_back(".hpp");
	vecFileNameExtensions.push_back(".c");
	vecFileNameExtensions.push_back(".cls");
	vecFileNameExtensions.push_back(".bas");
	vecFileNameExtensions.push_back(".frm");
	vecFileNameExtensions.push_back(".vbs");
	vecFileNameExtensions.push_back(".cs");

	vector<string> vecAllFiles = findFilesRecursive(strRootFolder, vecFileNameExtensions);

	return vecAllFiles;
}
//-------------------------------------------------------------------------------------------------
void trimStringLeft(string& strInput, const string& strToBeTrimmed)
{
	int nPos = strInput.find_first_not_of(strToBeTrimmed);
	if (nPos != string::npos)
	{
		strInput = strInput.substr(nPos, strInput.length()-nPos);
	}
}
//-------------------------------------------------------------------------------------------------
// Search through all given files and get the line containing ELI code,
// store them in a map, which is ELI code to a certain format of code
// as following:
// <Fully qualified file name>(line number): <Actual code at that line>
// Also store all duplicate ELI codes in a multimap in the same format
map<string, string> searchLinesWithELICode(const vector<string>& vecFilesToSearch, 
										   multimap<string, string>& mmapDuplicateELI)
{
	map<string, string> mapELIToLine;
	set<string> setSeenDuplicate;

	for (unsigned int i = 0; i < vecFilesToSearch.size(); i++)
	{
		string strCurrentFileToBeSearched(vecFilesToSearch[i]);
		ifstream ifs(strCurrentFileToBeSearched.c_str());
		string strLine("");
		int nLineCount = 1;

		while (ifs)
		{
			getline(ifs, strLine);

			int nFound = strLine.find("ELI");
			// search for ELIXXXXX in the line
			if (nFound != string::npos)
			{
				char cChar = strLine[nFound+3];
				// make sure the following char after ELI is a numeric char
				if (cChar >= '0' && cChar <= '9' )
				{
					// get the ELI code
					string strELICode(strLine.substr(nFound, 8));
					
					// trim off all spaces off the left of the string
					trimStringLeft(strLine, " \t");
					// line number
					char pszDigits[10];
					sprintf_s(pszDigits, "%d", nLineCount);				
					string strLineNum(pszDigits);
					
					// the line with the full file name, 
					// the line number and the actual contents that line 
					string strLineCode = strCurrentFileToBeSearched 
											+ "(" + strLineNum + "): " + strLine;
					
					// store it in the map
					pair<map<string, string>::iterator, bool> ret;
					ret = mapELIToLine.insert(pair<string, string>(strELICode, strLineCode));

					// check the return code from insert, if it is false then this ELI
					// code already existed in the map, store it in the duplicate multimap
					if (!ret.second)
					{
						// check to see if there is already an entry in the multimap
						// for this ELI code, if not then store the first occurrence as
						// well (for each duplicate there will be at least 2 entries in the
						// output file)
						if (setSeenDuplicate.insert(strELICode).second)
						{
							mmapDuplicateELI.insert(
								pair<string, string>(strELICode, mapELIToLine[strELICode]));
						}

						// store the string in the multimap
						mmapDuplicateELI.insert(pair<string, string>(strELICode, strLineCode));
					}
				}
			}

			// increment the line count
			nLineCount++;
		}
	}

	return mapELIToLine;
}

//-------------------------------------------------------------------------------------------------
// Main function
//-------------------------------------------------------------------------------------------------
int main(int argc, char* argv[])
{
	if (argc == 3)
	{
		// get the root folder path, which is the first parameter
		// Note: argv[0] is the program name
		string strRootFolder = buildAbsolutePath(argv[1]);
        string strSharePointFolder = buildAbsolutePath(strRootFolder + "\\..\\SharePoint");
        
		// get the output file name
		string strOutputFile(argv[2]);
		
		// find all qualified files inside specified root folder recursively
		vector<string> vecFiles = getFileNamesIn(strRootFolder);
        if (isValidFolder(strSharePointFolder))
        {
            vector<string> vecTemp = getFileNamesIn(strSharePointFolder);
            vecFiles.insert(vecFiles.end(), vecTemp.begin(), vecTemp.end());
        }

		if (!vecFiles.empty())
		{
			multimap<string,string> mmapDuplicateELICodes;
			map<string, string> mapELIToLine = searchLinesWithELICode(vecFiles, mmapDuplicateELICodes);
			if (!mapELIToLine.empty())
			{
				// write to the output file (overwrite always)
				ofstream ofs(strOutputFile.c_str(), ios::out | ios::trunc); 
				map<string, string>::iterator itMap = mapELIToLine.begin();
				for (; itMap!=mapELIToLine.end(); itMap++)
				{
					ofs << itMap->first << ";" << itMap->second << endl;
				}

				ofs.close();
				waitForFileAccess(strOutputFile, giMODE_READ_ONLY);
			}

			// if there are duplicate ELI codes
			if (!mmapDuplicateELICodes.empty())
			{
				// write to duplicate file (overwrite always)
				int nFound = strOutputFile.find_last_of(".");

				// get extension
				string strExt = strOutputFile.substr(nFound);

				// build the duplicate file name
				string strErrorFile = strOutputFile.substr(0, nFound) + ".duplicates"
					+ strExt;

				ofstream ofs(strErrorFile.c_str(), ios::out | ios::trunc);
				for (multimap<string,string>::iterator it = mmapDuplicateELICodes.begin();
					it != mmapDuplicateELICodes.end(); it++)
				{
					ofs << it->first << ";" << it->second << endl;
				}
				ofs.close();
				waitForFileAccess(strErrorFile, giMODE_READ_ONLY);
			}
		}
	}
	else
	{
		cout << "\nExpecting 2 parameters";
		cout << "\nUsage:";
		cout << "\n\targ1 - fully-qualified root folder to be searched";
		cout << "\n\targ2 - fully-qualified output file name";
		cout << endl;
		return 1;
	}

	return 0;
}
//-------------------------------------------------------------------------------------------------
