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

// Disk Serial Number expected for Extract Network drive
const unsigned long gulDOMAIN_PATH_SERIAL_NUMBER_FNP = 1558792743;		// for FNP
const unsigned long gulDOMAIN_PATH_SERIAL_NUMBER_ESITDC01 = 15240731;		// for es-it-dc-01  - Jake's replacement

// Associated Network path
const string gstrDOMAIN_PATH_FNP = "\\fnp2\\internal";		// for FNP
const string gstrDOMAIN_PATH_ESITDC01 = "\\es-it-dc-01\\internal";		// for es-it-dc-01  - Jake's replacement
const string gstrDOMAIN_PATH_ESITDC01_JAKE = "\\es-it-dc-01\\Jake-Internal"; // 2nd share that is the same as Internal

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
	// Check FNP
	string strDrive = getMappedDrive( gstrDOMAIN_PATH_FNP );
	if (strDrive.length() > 0)
	{
		// Append backslash character
		strDrive += "\\";

		// Get FNP's disk serial number
		unsigned long	ulTemp = 0;
		GetVolumeInformation( strDrive.c_str(), NULL, NULL, &ulTemp, NULL, NULL, NULL, NULL );

		// Compare serial numbers
		if (ulTemp == gulDOMAIN_PATH_SERIAL_NUMBER_FNP)
		{
			return true;
		}
	}

	// Check ES-IT-DC-01 if FNP check failed
	strDrive = getMappedDrive(gstrDOMAIN_PATH_ESITDC01);
	if ( strDrive == "" )
	{
		// There are 2 possible drive mappings that go to the same drive
		strDrive = getMappedDrive(gstrDOMAIN_PATH_ESITDC01_JAKE);
	}
	if (strDrive.length() > 0)
	{
		// Append a backslash character
		strDrive += "\\";

		// Get ES-IT-DC-01's disk serial number
		unsigned long ulTemp = 0;
		GetVolumeInformation(strDrive.c_str(), NULL, NULL, &ulTemp, NULL, NULL, NULL, NULL);

		// Compare serial numbers
		if (ulTemp == gulDOMAIN_PATH_SERIAL_NUMBER_ESITDC01)
		{
			return true;
		}
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
