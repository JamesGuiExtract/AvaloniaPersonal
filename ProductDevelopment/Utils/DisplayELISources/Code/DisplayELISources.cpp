// DisplayELISources.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#include <UCLIDException.h>
#include <StringTokenizer.h>
#include <cpputil.h>
#include <LicenseUtils.h>
#include <EncryptionEngine.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>


#include <string>
#include <vector>
#include <algorithm>
#include <iostream>
#include <fstream>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Define four UCLID passwords used for encrypted Debug information
// NOTE: These passwords were not exposed at the header file level because
//		 no user of this class needs to know that these passwords exist
const unsigned long	gulUCLIDKey13 = 0x6B772ADF;
const unsigned long	gulUCLIDKey14 = 0x498075C5;
const unsigned long	gulUCLIDKey15 = 0x3C6D1A3E;
const unsigned long	gulUCLIDKey16 = 0x6EDC5C7D;

//-------------------------------------------------------------------------------------------------
// Global Variables
//-------------------------------------------------------------------------------------------------
string gstrEngineeringFolder;

//-------------------------------------------------------------------------------------------------
// NOTE: This function is purposely not exposed at header file level as a class
//		 method, as no user of this class needs to know that such a function
//		 exists.
void getUEPassword(ByteStream& rPasswordBytes)
{
	ByteStreamManipulator bsm(ByteStreamManipulator::kWrite, rPasswordBytes);
	
	bsm << gulUCLIDKey13;
	bsm << gulUCLIDKey14;
	bsm << gulUCLIDKey15;
	bsm << gulUCLIDKey16;
	bsm.flushToByteStream( 8 );
}

//--------------------------------------------------------------------------------------------------
// print ELI Source list according to the specified ELI codes
void printELISourceList(const string& strSourceFile, const vector<string>& vecELICodes)
{
	if (vecELICodes.size() > 0)
	{
		ifstream ifs(strSourceFile.c_str());
		string strLine("");
		while (ifs)
		{
			getline(ifs, strLine);
			if (!strLine.empty())
			{
				vector<string> vecTokens;
				StringTokenizer::sGetTokens(strLine, ';', vecTokens);
				if (vecTokens.size() >= 2)
				{
					// if one of the ELI codes can be found on this line
					if (find(vecELICodes.begin(), vecELICodes.end(), vecTokens[0]) != vecELICodes.end())
					{
						cout << vecTokens[1] << endl;
					}
				}
			}
		}
	}
}
//--------------------------------------------------------------------------------------------------
std::string getDataValue(const std::string strEncrypted)
{
	// Make a working copy so if it fails we can return the original
	string strValue = strEncrypted;

	// Check Value for Encryption
	if (strValue.find( gstrENCRYPTED_PREFIX.c_str(), 0 ) == 0)
	{
		// Check Internal Tools license
		if (isInternalToolsLicensed())
		{
			// Decrypt the value
			string s = strEncrypted;
			try
			{
				try
				{
					///////////////////////////////////
					// Licensed, provide decrypted text
					///////////////////////////////////
					// Remove encryption indicator prefix
					strValue.erase( 0, gstrENCRYPTED_PREFIX.length() );

					// Create encrypted ByteStream from the hex string
					ByteStream bsInput(strValue);

					// Decrypt the ByteStream
					ByteStream decryptedBytes;
					MapLabel encryptionEngine;
					ByteStream passwordBytes;

					getUEPassword( passwordBytes );

					encryptionEngine.getMapLabel( decryptedBytes, bsInput, passwordBytes );

					// Retrieve the final string
					ByteStreamManipulator bsmFinal(ByteStreamManipulator::kRead, decryptedBytes);
					bsmFinal >> strValue;
				}
				CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION_NO_MFC("ELI21958")
			}
			catch(UCLIDException ue)
			{
				UCLIDException uex("ELI21957", "Unable to decrypt value", ue);
				uex.log();
				strValue = strEncrypted;
			}
		}
		else
		{
			// Not licensed, replace text with Encrypted indicator
			strValue = gstrENCRYPTED_INDICATOR;
		}
	}
	return strValue;
}
//--------------------------------------------------------------------------------------------------
void getELICodesAndStackTraceFromClipBoard( vector<string>& vecELICodes, vector<string>& vecStackTrace)
{
	vecELICodes.clear();
	vecStackTrace.clear();
	// Check to see is clipboard has the text format available
	BOOL bIsText = ::IsClipboardFormatAvailable(CF_TEXT);
	if (bIsText)
	{
		// open the clipboard
		BOOL bOpen = ::OpenClipboard(NULL);
		if (bOpen)
		{
			HANDLE handle = ::GetClipboardData(CF_TEXT);
			char* sData = (char*)::GlobalLock(handle);
			::GlobalUnlock(handle);
			::CloseClipboard();
			
			// get the byte stream from the clip board
			string strByteStream(sData);
			// create a uclid exception from the byte stream
			string strDummyCode("DummyCode");
			UCLIDException uclidException;
			uclidException.createFromString(strDummyCode, strByteStream);
			// get all ELI codes for this exception, then divide and
			// store them into the vecELICodes
			string strELICodes(uclidException.getAllELIs());
			
			StringTokenizer::sGetTokens(strELICodes, ',', vecELICodes);		
			vector<string>::iterator iter = find(vecELICodes.begin(), vecELICodes.end(), strDummyCode);
			// remove the "DummyCode"
			if (iter != vecELICodes.end())
			{
				vecELICodes.erase(iter);
			}

			// Get all of the stack trace entries
			
			// Set current exception to the one obtained from the clipboard.
			const UCLIDException *ueCurr = &uclidException;
			while (ueCurr != NULL)
			{
				// Add Stack trace to the list of the eli codes.
				const vector<string> & rvecStackTrace = ueCurr->getStackTrace();
				for (size_t i = 0; i < rvecStackTrace.size(); i++)
				{
					// Get the decrypted stack trace line
					string strStackTraceLine = getDataValue(rvecStackTrace[i]);

					// Update the path given in the stack trace to be relative to to
					// engineering folder passed on the command line.

					// Find \Engineering\ in the path
					string strSearchCopy = strStackTraceLine;
					makeLowerCase(strSearchCopy);
					int nEngPos = strSearchCopy.find("\\engineering\\");

					// Find the " in " that preceeds the path.
					int nInPos = strSearchCopy.find(" in ");

					// If both were found update the path.
					if ( nEngPos != string::npos && nInPos != string::npos)
					{
						// Replace from the end of In to the end of Engineering with the engineering folder.
						strStackTraceLine.replace(nInPos + 4, nEngPos - (nInPos +4) + 13, gstrEngineeringFolder + "\\");
					}
					
					// Save the stack trace line in the vecStackTrace vector.
					vecStackTrace.push_back(strStackTraceLine);
				}

				// Get the next inner exception.
				ueCurr = ueCurr->getInnerException();
			}
		}
	}
}

//--------------------------------------------------------------------------------------------------
int main(int argc, char* argv[])
{
	// at least one parameter
	if (argc >= 3)
	{
		// first parameter is reserved for the ELI source list file
		string strELISourceListFile(argv[1]);

		vector<string> vecELICodes;
		vector<string> vecStackTrace;

		// whether it's an ELI code or the /c, which needs to copy
		// text from the clip board
		if (_strcmpi(argv[2], "/c") == 0)
		{
			// Must get the Engineering folder
			if (argc != 4 )
			{
				gstrEngineeringFolder = "";
			}
			else
			{
				gstrEngineeringFolder = argv[3];
			}
			getELICodesAndStackTraceFromClipBoard(vecELICodes, vecStackTrace);
		}
		else
		{
			for (int i=2; i<argc; i++)
			{
				string strELICode(argv[i]);
				::makeUpperCase(strELICode);
				vecELICodes.push_back(strELICode);
			}
		}

		printELISourceList(strELISourceListFile, vecELICodes);

		for each ( string s in vecStackTrace)
		{
			int linepos = s.find(":line");
			if ( linepos != string::npos)
			{
				s.replace(linepos, 6, "(");
				s += "):";
			}
			string strFile;
			string strMethod;
			string strLineNumber;
			int pos = s.find (" in ");
			if ( pos != string::npos)
			{
				cout << s.substr(pos+4) <<  s.substr(0,pos) <<endl;
			}
			else
			{
				cout << s << endl;
			}
		}
	}
	else
	{
		cout << "\nExpecting at lease 2 paramenter";
		cout << "\nUsage:";
		cout << "\n\targ1 - Fully qualified ELI source list file name";
		cout << "\n\targ2 - ELI Code OR /c";
		cout << "\n\t\targ3 - if /c specified  -Path for Engineering folder - without trailing \\";
		cout << "\n\t\t\te.g. D:\\Engineering";
		cout << "\n\tOptional arg3...n - ELI Code";
		cout << endl;

		return 1;
	}

	return 0;
}
