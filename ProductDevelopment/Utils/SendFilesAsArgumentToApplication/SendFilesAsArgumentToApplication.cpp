// SendFilesAsArgumentToApplication.cpp : Defines the entry point for the console application.
//

#pragma warning(disable:4786)

#include "stdafx.h"
#include <afxwin.h>

#include <iostream>
#include <string>
#include <vector>
using namespace std;

// TODO: this function contains 100% reusable code!  put this in CPP util or one of the
// other core reusable files
void getFilesMeetingSpec(vector<string>& rvecFiles, string strDirAndFileSpec, bool bRecurse, bool bFilesOnly)
{
	WIN32_FIND_DATA data;
	
	// if no directory was specified, use the default directory
	if (strDirAndFileSpec.find('\\') == string::npos)
		strDirAndFileSpec.insert(0, ".\\");

	// determine the directory specfication and the file specification
	// as seperate strings
	// e.g: if strDirAndFileSpec = "c:\\temp\\*.txt", then
	// strDirSpec = "c:\\temp" and strFileSpec = "*.txt"
	const char *pszFileSpec = strrchr(strDirAndFileSpec.c_str(), '\\') + 1;
	string strFileSpec = pszFileSpec;
	string strDirSpec = strDirAndFileSpec;
	strDirSpec.erase(strDirSpec.rfind('\\'));
		
	// find all files in the current directory that meet the file specification
	HANDLE hFind = FindFirstFile(strDirAndFileSpec.c_str(), &data);
	if (hFind != INVALID_HANDLE_VALUE)
	{
		while (true)
		{
			if (((data.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) == 0 && bFilesOnly) ||
				((data.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) != 0 && !bFilesOnly &&
				  strcmp(data.cFileName, ".") != 0 && strcmp(data.cFileName, "..") != 0))
			{
				// we found a file that meets the specs.
				string strFileMeetingSpec = strDirSpec;
				strFileMeetingSpec += "\\";
				strFileMeetingSpec += data.cFileName;
				rvecFiles.push_back(strFileMeetingSpec);
			}

			if (!FindNextFile(hFind, &data))
				break;
		}
	}

	FindClose(hFind);

	// if recursion was desired by the caller, and if there exist directories under
	// the current directory, then recyrse through all the sub-directories and 
	// search for files with the same file specification
	if (bRecurse)
	{
		vector<string> vecDirectories;

		// find all the sub directories
		strDirAndFileSpec = strDirSpec + string("\\*.*");
		hFind = FindFirstFile(strDirAndFileSpec.c_str(), &data);
		if (hFind != INVALID_HANDLE_VALUE)
		{
			while (true)
			{
				if ((data.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) != 0 &&
					strcmp(data.cFileName, ".") != 0 && strcmp(data.cFileName, "..") != 0)
				{
					// we found a subdirectory
					string strFileMeetingSpec = strDirSpec;
					strFileMeetingSpec.append("\\");
					strFileMeetingSpec += data.cFileName;
					vecDirectories.push_back(strFileMeetingSpec);
				}

				if (!FindNextFile(hFind, &data))
					break;
			}
		}
		FindClose(hFind);

		if (!vecDirectories.empty())
		{
			vector<string>::const_iterator iter;
			for (iter = vecDirectories.begin(); iter != vecDirectories.end(); iter++)
			{
				string strRecurseDirAndFileSpec = *iter;
				strRecurseDirAndFileSpec += "\\";
				strRecurseDirAndFileSpec += strFileSpec;
				getFilesMeetingSpec(rvecFiles, strRecurseDirAndFileSpec, bRecurse, bFilesOnly);
			}
		}
	}
}

void printUsage()
{
	cout << "This application requires four arguments:" << endl;
	cout << " arg1: file specification (e.g. \"*.dat\")" << endl;
	cout << " arg2: should be 1 if recursion is to be done, or 0 otherwise" << endl;
	cout << " arg3: should be 0 if result should contain directories" << endl;
	cout << "       should be 1 if result should contain files" << endl;
	cout << " arg4: the name of the application to run when matching files" << endl;
	cout << "       or directories are found" << endl;
	cout << " argn: any additional provided arguments will be passed to the" << endl;
	cout << "       {arg4} application as the 2nd, 3rd, 4th, and so on arguments" << endl;
	cout << "       (Note that the first argument to the {arg4} application will" << endl;
	cout << "       always be a file/directory that meets the search specification" << endl;
	cout << endl;
}

int main(int argc, char* argv[])
{
	cout << "SendFilesAsArgumentToApplication Utility" << endl;
	cout << "Copyright 2001, UCLID Software, LLC." << endl;
	cout << "All rights reserved." << endl;
	cout << endl;
	// ensure correct number of parameters.
	if (argc < 5)
	{
		printUsage();
		return 0;
	}

	// ensure valid correct second parameter
	bool bRecurse;
	if (strcmp(argv[3], "0") == 0)
		bRecurse = false;
	else if (strcmp(argv[3], "1") == 0)
		bRecurse = true;
	else
	{
		cout << "Invalid 2nd argument!" << endl;
		cout << endl;
		printUsage();
	}

	bool bFilesOnly;
	if (strcmp(argv[3], "0") == 0)
		bFilesOnly = false;
	else if (strcmp(argv[3], "1") == 0)
		bFilesOnly = true;
	else
	{
		cout << "Invalid 3rd argument!" << endl;
		cout << endl;
		printUsage();
	}

	string strAdditionalArguments;
	for (int i = 0; i < argc-5; i++)
	{
		strAdditionalArguments += " ";
		char *pszArgument = argv[i+5];
		
		// if the argument contains spaces, enclose it in quotes, 
		// otherwise, leave it the way it is
		if (strchr(pszArgument, ' ') != NULL)
		{
			strAdditionalArguments += "\"";
			strAdditionalArguments += string(pszArgument);
			strAdditionalArguments += "\"";
		}
		else
			strAdditionalArguments += string(pszArgument);
	}

	// process files
	vector<string> vecFilesToProcess;
	getFilesMeetingSpec(vecFilesToProcess, argv[1], bRecurse, bFilesOnly);
	if (!vecFilesToProcess.empty())
	{
		vector<string>::const_iterator iter;
		for (iter = vecFilesToProcess.begin(); iter != vecFilesToProcess.end(); iter++)
		{
			string strCommand = argv[4];
			strCommand += " \"";
			strCommand += *iter;
			strCommand += "\"";
			strCommand += strAdditionalArguments;
			cout << "SYSTEM CALL: " << strCommand << endl;
			if (system(strCommand.c_str()) == -1)
			{
				cout << "Error executing the above command!" << endl;
				cout << endl;
				return EXIT_FAILURE;
			}
		}
	}
	else
	{
		cout << "No files found that meet the specification " << argv[1] << endl;
	}
	return 0;
}
