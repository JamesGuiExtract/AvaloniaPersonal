// ModifyFile.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <windows.h>

// windows.h must be included before cppUtil.h
#include <cppUtil.h>

#include <string>
#include <fstream>
#include <iostream>
using namespace std;

void printUsage()
{
	cout << endl;
	cout << "This application takes two arguments:" << endl;
	cout << "  FileName: this is the name of the file to modify." << endl;
	cout << "  Operator : this should be either \"suffix\" or \"prefix\"." << endl;
	cout << "  Data: this should be the data associated with the modification operator." << endl;
	cout << endl;
}

int modifyFile(const char *pszFileName, bool bPrefixData, const char *pszData)
{
	ifstream infile(pszFileName);
	if (!infile)
	{
		cout << "ERROR: Unable to open input file for reading!" << endl;
		return EXIT_FAILURE;
	}

	const char *pszTempFileName = "ModifyFile.tmp";
	ofstream outfile(pszTempFileName);
	if (!outfile)
	{
		cout << "ERROR: Unable to open temporary output file for writing!" << endl;
		return EXIT_FAILURE;
	}

	string strTemp;
	do
	{
		getline(infile, strTemp);
		
		if (strTemp != "")
		{
			if (bPrefixData)
				strTemp.insert(0, pszData);
			else
				strTemp.append(pszData);
		}

		outfile << strTemp << endl;

	} while (infile);

	// close the two files
	infile.close();
	outfile.close();
	waitForFileAccess(pszTempFileName, giMODE_READ_ONLY);

	if (!CopyFile(pszTempFileName, pszFileName, FALSE))
	{
		cout << "ERROR: Unable to update input file with modifications!" << endl;
		return EXIT_FAILURE;
	}
	waitForFileAccess(pszFileName, giMODE_READ_ONLY);

	try
	{
		deleteFile(pszTempFileName);
	}
	catch (...)
	{
		cout << "WARNING: Unable to delete temporary file!" << endl;
		return EXIT_FAILURE;
	}

	return EXIT_SUCCESS;
}

int main(int argc, char* argv[])
{
	if (argc != 4)
	{
		printUsage();
		return EXIT_FAILURE;
	}
	else
	{
		bool bPrefix = _strcmpi(argv[2], "prefix") == 0;
		bool bSuffix = _strcmpi(argv[2], "suffix") == 0;
		
		if (!bSuffix && !bPrefix)
		{
			cout << "ERROR: Invalid modification operator!" << endl;
			printUsage();
			return EXIT_FAILURE;
		}

		if (bPrefix)
			return modifyFile(argv[1], true, argv[3]);
		else
			return modifyFile(argv[1], false, argv[3]);
	}
}