#include "stdafx.h"
#include "EncryptionEngine.h"
#include "EncryptedFileManager.h"
#include "StringTokenizer.h"
#include "cpputil.h"
#include "UCLIDException.h"
#include "ByteStream.h"
#include "ByteStreamManipulator.h"

#include <io.h>

using namespace std;

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

//-------------------------------------------------------------------------------------------------
// GLOBALS
//-------------------------------------------------------------------------------------------------
// Define four UCLID passwords used for encrypted ETF files
// NOTE: These passwords were not exposed at the header file level because
//		 no user of this class needs to know that these passwords exist
const unsigned long	gulUCLIDKey09 = 0x35026831;
const unsigned long	gulUCLIDKey10 = 0x57D74D90;
const unsigned long	gulUCLIDKey11 = 0x57250A5E;
const unsigned long	gulUCLIDKey12 = 0x0FCD7A67;

// static variables
//unsigned long EncryptedFileManager::COMPONENT_ID = 151;

//-------------------------------------------------------------------------------------------------
// FILE-SCOPE FUNCTIONS
//-------------------------------------------------------------------------------------------------
// NOTE: This function is purposely not exposed at header file level as a class
//		 method, as no user of this class needs to know that such a function
//		 exists.
void getPassword(ByteStream& rPasswordBytes)
{
	ByteStreamManipulator bsm(ByteStreamManipulator::kWrite, rPasswordBytes);
	
	bsm << gulUCLIDKey09;
	bsm << gulUCLIDKey10;
	bsm << gulUCLIDKey11;
	bsm << gulUCLIDKey12;
	bsm.flushToByteStream( 8 );
}

//-------------------------------------------------------------------------------------------------
// MapLabelManager = EncryptedFileManager. The class has been renamed disguise its purpose and make
// hacking our encryption a somewhat more difficult task.
//-------------------------------------------------------------------------------------------------
MapLabelManager::MapLabelManager()
{
}
//-------------------------------------------------------------------------------------------------
void MapLabelManager::setMapLabel(const string& strInputFile, const string& strOutputFile)
{
	// Check for existence of file
	validateFileOrFolderExistence( strInputFile );

	// Create a bytestream to represent the input file
	ByteStream bsInput;
	bsInput.loadFrom( strInputFile );

	// New bytestream padded to 8-byte boundary
	ByteStream bsPaddedInput;
	ByteStreamManipulator bsm( ByteStreamManipulator::kWrite, bsPaddedInput );
	bsm.write( bsInput );
	bsm.flushToByteStream( 8 );

	// Create a bytestream to represent the encryption password 
	ByteStream passwordBytes;
	getPassword( passwordBytes );

	// Create the encryption engine and encrypt the padded bytestream
	MapLabel encryptionEngine;
	ByteStream encryptedBytes;
	encryptionEngine.setMapLabel( encryptedBytes, bsPaddedInput, passwordBytes );

	// Write the encrypted data to the specified output file
	encryptedBytes.saveTo( strOutputFile.c_str() );
}
//-------------------------------------------------------------------------------------------------
unsigned char * MapLabelManager::getMapLabel(const string& strInputFile, 
														unsigned long *pulByteCount)
{
	try
	{
		try
		{
			// Check for existence of file
			validateFileOrFolderExistence( strInputFile );

			// Read the encrypted data from the specified input file
			// Bytestream from input file is assumed to be padded to 8-byte boundary
			ByteStream encryptedBytes;
			encryptedBytes.loadFrom( strInputFile );

			// Create a bytestream to represent the encryption password 
			ByteStream passwordBytes;
			getPassword( passwordBytes );

			// Create the encryption engine and decrypt the bytestream
			MapLabel encryptionEngine;
			ByteStream decryptedBytes;
			encryptionEngine.getMapLabel( decryptedBytes, encryptedBytes, passwordBytes );

			// Retrieve the unpadded ByteStream
			ByteStream bsUnpadded;
			ByteStreamManipulator bsm( ByteStreamManipulator::kRead, decryptedBytes );
			bsm.read( bsUnpadded );

			// Copy data to new block of bytes
			// NOTE: Caller is responsible for deleting pNewData
			unsigned long ulSize = bsUnpadded.getLength();
			unsigned char *pNewData = new unsigned char[ulSize];
			memcpy( pNewData, bsUnpadded.getData(), ulSize );

			// Return the result
			*pulByteCount = ulSize;
			return pNewData;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30108");
	}
	catch(UCLIDException& uex)
	{
		UCLIDException ue("ELI30109", "Unable to read binary file.", uex);
		ue.addDebugInfo("File To Read", strInputFile);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
std::vector<std::string> MapLabelManager::getMapLabel(const string& strInputFile)
{
	try
	{
		try
		{
			// Retrieve binary data from the text file
			unsigned long ulLength = 0;
			unsigned char* pszData = getMapLabel( strInputFile, &ulLength );

			// Get the string from the decrypted bytes
			string strLines( (const char *)pszData, ulLength );
			delete [] pszData;

			// Replace all \r before proceeding
			::replaceVariable( strLines, "\r", "" );

			vector<string> vecTokens;
			// Tokenize the lines using the newline character
			StringTokenizer st( '\n' );
			st.parse( strLines, vecTokens );
			return vecTokens;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30106");
	}
	catch(UCLIDException& uex)
	{
		UCLIDException ue("ELI30107", "Unable to read text file.", uex);
		ue.addDebugInfo("File To Read", strInputFile);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
