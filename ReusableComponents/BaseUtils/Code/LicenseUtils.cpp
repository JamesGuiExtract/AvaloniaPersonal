//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	cpputil.cpp
//
// PURPOSE:	Various utility functions
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan (August 2000 - present)
//
//==================================================================================================

#include "stdafx.h"
#include "LicenseUtils.h"
#include "UCLIDException.h"
#include "ByteStreamManipulator.h"

#include <string>
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


// Since the DFS shares can be provided by different domain server, currently DC1 and Megatron, the both volume serial number are needed
// If another domain server is added -- hex value of Volume Serial number is displayed when dir \\<domain server>\All is ran from cmd prompt
const unsigned long gulDOMAIN_PATH_ALL_MEGATRON = 0xA4D4BD53; // For extract.local\all
const unsigned long gulDOMAIN_PATH_ALL_DC1 = 0xF4582397; // For extract.local\all
const unsigned long gulDOMAIN_PATH_ALL_DC4 = 0x7857D186; // For extract.oocal\All

// Associated Network path
const string gstrDOMAIN_PATH_ALL = "\\extract.local\\all"; // DFS share that contains the license utility

//-------------------------------------------------------------------------------------------------
// Local functions
//-------------------------------------------------------------------------------------------------
string getMappedDrive(string strNetworkPath)
{
	// Make the string lower case
	makeLowerCase(strNetworkPath);

	// Step through all possible drive letters
    char cDrive = 'A';
    while (cDrive <= 'Z')
    {
		char szDeviceName[3] = {cDrive, ':', '\0'};
        char szTarget[512] = {0};

		// Check for this drive letter
        if (QueryDosDevice( szDeviceName, szTarget, 511 ) != 0)
		{
			// Drive letter exists, search for semicolon
			string strPath = szTarget;
		
			// Make the string lower case
			makeLowerCase(strPath);

			size_t pos = strPath.find( ';' );

			// Continue if this is not a local device
			if (pos != string::npos)
			{
				// Retain the text after the semicolon
				strPath = strPath.substr( pos + 1 );

				// Find the next backslash character
				pos = strPath.find( '\\' );
				if (pos != string::npos)
				{
					// Retain the backslash and remaining characters
					strPath = strPath.substr( pos );

					// Compare against desired network path
					if (strPath.compare( strNetworkPath.c_str() ) == 0)
					{
						return szDeviceName;
					}
				}
			}
		}

		// Check the next drive letter
        cDrive++;
    }

	// Not found
	return "";
}
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

//-------------------------------------------------------------------------------------------------
// Exported functions
//-------------------------------------------------------------------------------------------------
bool isInternalToolsLicensed()
{
	// Check Extract.local\All 
	string strDrive = getMappedDrive(gstrDOMAIN_PATH_ALL);
	if (strDrive.length() > 0)
	{
		// Append a backslash character
		strDrive += "\\";

		// Get  disk serial number
		unsigned long ulTemp = 0;
		GetVolumeInformation(strDrive.c_str(), NULL, NULL, &ulTemp, NULL, NULL, NULL, NULL);

		// Compare serial numbers
		if (ulTemp == gulDOMAIN_PATH_ALL_DC1 || ulTemp == gulDOMAIN_PATH_ALL_MEGATRON
			|| ulTemp == gulDOMAIN_PATH_ALL_DC4)
		{
			return true;
		}
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
