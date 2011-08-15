
#include "stdafx.h"

#include <EncryptedFileManager.h>
#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

#include <stdlib.h>
#include <fstream>
#include <vector>
#include <string>
#include <iostream>

using namespace std;

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void initialize()
{
	// Removed this requirement per P13 #3494 - WEL 09/16/05
	// Replaced this requirement per P13 #4243 - WEL 01/30/07
	// Read license files
	LicenseManagement::loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);
}
//-------------------------------------------------------------------------------------------------
void printUsage()
{
	cout << "USAGE:" << endl;
	cout << "This application requires 2 arguments:" << endl;
	cout << " arg1: The name of the text file to encrypt." << endl;
	cout << " arg2 (optional): The name of the encrypted file to be created."<< endl;
	cout << "   If the second argument is not provided, the encrypted file" << endl;
	cout << "   will be created in the same location as the input file, but" << endl;
	cout << "   with a .etf extension instead of the original extension." << endl;
	cout << endl;
}
//-------------------------------------------------------------------------------------------------
void validateLicense()
{
	// RDT license is required to run this utility
	VALIDATE_LICENSE( gnRULE_DEVELOPMENT_TOOLKIT_OBJECTS, "ELI07071", "Encrypt File" );
}
//-------------------------------------------------------------------------------------------------
bool verifyArguments(int argc, char* argv[])
{
	// ensure that there is at least one argument, and no more than 
	// 2 arguments provided to this application from the command prompt
	if (argc != 2 && argc != 3)
	{
		cout << "ERROR: Invalid number of arguments!" << endl;
		return false;
	}

	if (strcmp(argv[1], "/?") == 0)
	{
		return false;
	}

	return true;
}

//-------------------------------------------------------------------------------------------------
// Main function
//-------------------------------------------------------------------------------------------------
int main(int argc, char* argv[])
{
	try
	{
		// verify arguments, and print usage if appropriate
		if (!verifyArguments(argc, argv))
		{
			printUsage();
			return EXIT_FAILURE;
		}

		// get the names of the input and output files
		string strInputFile = argv[1];
		
		// if the second argument was not provided, compute the name of
		// the output file by appending the .etf extension
		// NOTE: ETF = Encrypted Text File
		string strOutputFile = strInputFile;
		if (argc == 2)
		{
			// append the new extension
			strOutputFile += string(".etf");
		}
		else if (argc == 3)
		{
			strOutputFile = argv[2];
		}

		// Prepare licensing
		initialize();

		// Check license state
		validateLicense();

		// display message indicating that encrypting will start...
		cout << endl;
		cout << "Encrypting text file \"" << strInputFile << "\" ..." << endl;

		// compile the input file and save compiled results 
		// as the specified output file
		EncryptedFileManager efm;
		efm.encrypt(strInputFile, strOutputFile);

		// display message indicating that encrypting is over
		cout << "Encryption successfully completed." << endl;
		cout << endl;

		/*
		// ORIGINAL
		// DEBUGGING CODE TO TEST IF DECOMPILATION IS WORKING CORRECTLY
		vector<string> vecResult = efm.decryptTextFile("c:\\a.out");
		ofstream outfile("c:\\a2.txt");
		vector<string>::const_iterator iter;
		bool bWriteNewline = false;
		for (iter = vecResult.begin(); iter != vecResult.end(); iter++)
		{
			if (bWriteNewline)
			{
				outfile << string("\n");
			}

			string strTemp = iter->c_str();
			outfile << strTemp;
			bWriteNewline = true;
		}
		*/
/*
		// MORE DEBUGGING CODE FOR BINARY FILES
		unsigned long ulSize = 0;
		unsigned char *pData = efm.decryptBinaryFile( "C:\\BlockTest03A.rsd.etf", &ulSize );
		CFile fileOut( "C:\\BlockTest03B.rsd", CFile::modeCreate | CFile::modeWrite );
		fileOut.Write( (const void *)pData, ulSize );
		fileOut.Close();
		*/
/*		
		// MORE DEBUGGING CODE FOR TEXT FILES
		vector<string> vecResult = efm.decryptTextFile("c:\\CopyLog-Test.txt.etf");
		ofstream outfile("c:\\b2.txt");
		vector<string>::const_iterator iter;
		bool bWriteNewline = false;
		for (iter = vecResult.begin(); iter != vecResult.end(); iter++)
		{
			if (bWriteNewline)
			{
				outfile << string("\n");
			}

			string strTemp = iter->c_str();
			outfile << strTemp;
			bWriteNewline = true;
		}
*/
		// if we reached here, all went well
		return EXIT_SUCCESS;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06603")

	// if we reached here, it's because an exception was caught
	return EXIT_FAILURE;
}
//-------------------------------------------------------------------------------------------------
