//============================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	LMData.cpp
//
// PURPOSE:	Implementation of the LicenseManagement data class LMData
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//============================================================================

#include "stdafx.h"
#include "LMData.h"

#include <cpputil.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <UCLIDException.h>
#include <EncryptionEngine.h>

using namespace std;

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

// Define the current (and any past) version string
const string	strCURRENT_VERSION = "1";

//-------------------------------------------------------------------------------------------------
// Non-exported helper functions
//-------------------------------------------------------------------------------------------------
ByteStream getUserLicensePassword()
{
	// Create a 16 digit password.
	ByteStream	passwordBytes(16);
	auto pData = passwordBytes.getData();

	int i;
	for (i = 0; i < 8; i++)
	{
		pData[i] = i * 4 + 23 - i * 2;
	}
	for (int j = 0; j < 8; j++)
	{
		pData[i+j] = (2*i+j) * 7 + 31 - (i+2*j) * 3;
	}

	return passwordBytes;
}

//-------------------------------------------------------------------------------------------------
// LMData
//-------------------------------------------------------------------------------------------------
LMData::LMData(const string& strComputerName, unsigned long ulSerialNumber,
	const string& strMACAddress)
	: m_strUserComputerName(strComputerName),
	m_ulUserSerialNumber(ulSerialNumber),
	m_strUserMACAddress(strMACAddress)
{
	m_strIssuerName = "";
	m_strLicenseeName = "";
	m_strOrganizationName = "";
	m_IssueDate = CTime::GetCurrentTime();

	// User License items
	m_bUseComputerName = !m_strUserComputerName.empty();
	m_bUseSerialNumber = m_ulUserSerialNumber != 0;
	m_strUserString = "";
	m_strActualComputerName = "";
	m_ulActualSerialNumber = 0;
}
//-------------------------------------------------------------------------------------------------
LMData::~LMData()
{
	try
	{
		// Clean up the map
		m_mapCompIDToData.clear();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16432");
}
//-------------------------------------------------------------------------------------------------
void LMData::addLicensedComponent(unsigned long ulComponentID)
{
	// Create a component data object
	ComponentData	CD;

	// Set as licensed
	CD.m_bIsLicensed = true;

	// Default date to today
	CD.m_ExpirationDate = CTime::GetCurrentTime();

	// Add to map
	m_mapCompIDToData[ulComponentID] = CD;
}
//-------------------------------------------------------------------------------------------------
void LMData::addUnlicensedComponent(unsigned long ulComponentID, 
	const CTime& dateExpiration)
{
	// Create a component data object
	ComponentData	CD;

	// Set as unlicensed
	CD.m_bIsLicensed = false;

	// Store date
	CD.m_ExpirationDate = dateExpiration;

	// Add to map
	m_mapCompIDToData[ulComponentID] = CD;
}
//-------------------------------------------------------------------------------------------------
string LMData::compressData(bool bAddDashes)
{
	string strResult;

	/////////////////////////////////////
	// Add license information to the bsm
	/////////////////////////////////////
	ByteStream bytes;
	ByteStreamManipulator bytesManipulator(ByteStreamManipulator::kWrite, bytes);

	////////////////////////////////////////////////
	// TODO: Apply version-specific data compression
	////////////////////////////////////////////////

	// Add issuer name
	bytesManipulator << m_strIssuerName;

	// Add licensee name
	bytesManipulator << m_strLicenseeName;

	// Add organization name
	bytesManipulator << m_strOrganizationName;

	// Add issue date (and time)
	CTime	today;
	today = CTime::GetCurrentTime();
	bytesManipulator << today;

	// Add Use Computer Name flag
	bytesManipulator << m_bUseComputerName;

	// Add Use Serial Number flag
	bytesManipulator << m_bUseSerialNumber;

	// Add Use MAC Address flag
	bytesManipulator << m_bUseMACAddress;

	// Add User License String, if needed
	if (m_bUseComputerName || m_bUseSerialNumber || m_bUseMACAddress)
	{
		bytesManipulator << m_strUserString;
	}

	// Add component count
	unsigned long	ulCount = m_mapCompIDToData.size();
	bytesManipulator << ulCount;

	// Add component data
	map<unsigned long, ComponentData>::iterator iter;
	for (iter = m_mapCompIDToData.begin(); iter != m_mapCompIDToData.end(); 
		iter++)
	{
		// Add the component ID
		bytesManipulator << iter->first;

		// Add the ComponentData object
		bytesManipulator << (iter->second);
	}

	// Convert information to a stream of bytes
	// with length divisible by 8
	bytesManipulator.flushToByteStream( 8 );

	// Encrypt the byte stream
	ByteStream			encryptedByteStream;
	EncryptionEngine	ee;
	ee.encrypt( encryptedByteStream, bytes, getUserLicensePassword() );

	// Convert the encrypted stream of bytes to a string
	strResult = encryptedByteStream.asString();

	// Convert the string to upper case
	makeUpperCase( strResult );

	// Optionally insert dashes after every four characters for easy reading
	if (bAddDashes)
	{
		int iNumDashes = strResult.length() / 4 - 1;
		for (int i = 1; i <= iNumDashes; i++)
		{
			strResult.insert( i*4 + (i-1), "-" );
		}
	}

	// Return string to caller
	return strResult;
}
//-------------------------------------------------------------------------------------------------
string LMData::compressDataToString(unsigned long ulKey1, unsigned long ulKey2, 
									unsigned long ulKey3, unsigned long ulKey4)
{
	string	strResult;
	string	strOutput;

	///////////////////////
	// Parameter validation
	///////////////////////
	if ((ulKey1 == 0) || (ulKey2 == 0) || (ulKey3 == 0) || (ulKey4 == 0))
	{
		// Create and throw exception
		UCLIDException	ue( "ELI03571", 
			"One or more license keys provided are invalid!" );
		ue.addDebugInfo( "License Key 1", ulKey1 ); 
		ue.addDebugInfo( "License Key 2", ulKey2 ); 
		ue.addDebugInfo( "License Key 3", ulKey3 ); 
		ue.addDebugInfo( "License Key 4", ulKey4 ); 
		throw ue;
	}

	////////////////////
	// Compress the data
	////////////////////
	strResult = compressData();

	//////////////////////////////////////////////
	// Create a byte stream from the provided keys
	//////////////////////////////////////////////
	ByteStream bytesKey;
	ByteStreamManipulator bytesManipulatorKey(
		ByteStreamManipulator::kWrite, bytesKey);

	bytesManipulatorKey << ulKey1;
	bytesManipulatorKey << ulKey2;
	bytesManipulatorKey << ulKey3;
	bytesManipulatorKey << ulKey4;

	bytesManipulatorKey.flushToByteStream( 8 );

	///////////////////////////////////
	// Further encrypt the license data
	///////////////////////////////////
	ByteStream bytesLicense;
	ByteStreamManipulator bytesManipulatorLicense(
		ByteStreamManipulator::kWrite, bytesLicense);

	// Add original encrypted string
	bytesManipulatorLicense << strResult;

	// Convert information to a stream of bytes
	// with length divisible by 8
	bytesManipulatorLicense.flushToByteStream( 8 );

	// Encrypt the byte stream
	ByteStream			encryptedByteStream;
	EncryptionEngine	ee;
	ee.encrypt( encryptedByteStream, bytesLicense, bytesKey );

	// Convert the encrypted stream of bytes to a string
	strOutput = encryptedByteStream.asString();

	// Convert the string to upper case
	makeUpperCase( strOutput );

	// Return doubly-encrypted string
	return strOutput;
}
//-------------------------------------------------------------------------------------------------
bool LMData::containsExpiringComponent()
{
	// Get ID of first component
	long lTestID = getFirstComponentID();

	// Check each component for full licensure
	while (lTestID != -1)
	{
		// Get the associated ComponentData
		ComponentData	CD = getComponentData( lTestID );

		// Is this Component fully licensed
		if (!CD.m_bIsLicensed)
		{
			return true;
		}

		// Get the next component
		lTestID = getNextComponentID( lTestID );
	}

	// All components are fully licensed
	return false;
}
//-------------------------------------------------------------------------------------------------
void LMData::extractData(string strLicenseKey)
{
	// Remove any spaces or dashes
	replaceVariable( strLicenseKey, " ", "" );
	replaceVariable( strLicenseKey, "-", "" );

	// Create the bytestream from the hex characters
	try
	{
		ByteStream bytes( strLicenseKey );
		ByteStream decryptedBS;

		// Decrypt the string here
		EncryptionEngine ee;
		ee.decrypt( decryptedBS, bytes, getUserLicensePassword() );

		ByteStreamManipulator bsm( ByteStreamManipulator::kRead, decryptedBS );

		///////////////////////////////////////////////
		// TODO: Apply version-specific data extraction
		///////////////////////////////////////////////

		// Extract issuer name
		bsm >> m_strIssuerName;

		// Extract licensee name
		bsm >> m_strLicenseeName;

		// Extract organization name
		bsm >> m_strOrganizationName;

		// Extract issue date (and time)
		bsm >> m_IssueDate;

		// Extract Use Computer Name flag
		bsm >> m_bUseComputerName;

		// Extract Use Serial Number flag
		bsm >> m_bUseSerialNumber;

		// Extract Use MAC Address flag
		bsm >> m_bUseMACAddress;

		// Extract User License string, if needed
		if (m_bUseComputerName || m_bUseSerialNumber || m_bUseMACAddress)
		{
			// Retrieve string of encrypted user license info
			string	strData;
			bsm >> strData;

			// Store the string and decrypt contents
			setUserString( strData );
		}

		// Extract component count
		unsigned long	ulCount = 0;
		bsm >> ulCount;

		// Extract the component data
		for (unsigned long n = 0; n < ulCount; n++)
		{
			unsigned long	ulID = 0;
			ComponentData	CD;

			// Extract component ID
			bsm >> ulID;

			// Extract component details
			bsm >> CD;

			// Add component data to map
			m_mapCompIDToData[ulID] = CD;
		}

		////////////////////////////////////
		// Determine info from this computer
		////////////////////////////////////
		// Store actual computer name
		m_strActualComputerName = getComputerName();

		// Store actual disk serial number
		m_ulActualSerialNumber = getDiskSerialNumber();

		// Store actual MAC address
		m_strActualMACAddress = getMACAddress();
	}
	catch (UCLIDException& ue)
	{
		UCLIDException uexOuter("ELI02277", 
			"The license code you have provided is not a valid license code!", ue);
		uexOuter.addDebugInfo("License Key", strLicenseKey); 
		throw uexOuter;
	}
}
//-------------------------------------------------------------------------------------------------
void LMData::extractDataFromString(const string& strLicenseString, 
								   unsigned long ulKey1, unsigned long ulKey2, 
								   unsigned long ulKey3, unsigned long ulKey4)
{
	CStdioFile		fileIn;
	CFileException	e;
	CString			zLicenseInfo;
	string			strResult;

	///////////////////////
	// Parameter validation
	///////////////////////
	if ((ulKey1 == 0) || (ulKey2 == 0) || (ulKey3 == 0) || (ulKey4 == 0))
	{
		// Create and throw exception
		UCLIDException	ue( "ELI03573", 
			"One or more license keys provided are invalid!" );
		ue.addDebugInfo( "License Key 1", ulKey1 ); 
		ue.addDebugInfo( "License Key 2", ulKey2 ); 
		ue.addDebugInfo( "License Key 3", ulKey3 ); 
		ue.addDebugInfo( "License Key 4", ulKey4 ); 
		throw ue;
	}

	if (strLicenseString.length() == 0)
	{
		// Create and throw exception
		UCLIDException	ue( "ELI03574", 
			"Empty string provided for license!" );
		throw ue;
	}

	//////////////////////////////////////////////
	// Create a byte stream from the provided keys
	//////////////////////////////////////////////
	ByteStream bytesKey;
	ByteStreamManipulator bytesManipulatorKey(
		ByteStreamManipulator::kWrite, bytesKey);

	bytesManipulatorKey << ulKey1;
	bytesManipulatorKey << ulKey2;
	bytesManipulatorKey << ulKey3;
	bytesManipulatorKey << ulKey4;

	bytesManipulatorKey.flushToByteStream( 8 );

	///////////////////////////
	// Decrypt the license data
	///////////////////////////
	ByteStream bytes( strLicenseString.c_str() );
	ByteStream decryptedBS;

	// Decrypt and convert to string
	EncryptionEngine ee;
	ee.decrypt( decryptedBS, bytes, bytesKey );

	// Extract the original license string
	ByteStreamManipulator bsm( ByteStreamManipulator::kRead, decryptedBS );
	bsm >> strResult;

	///////////////////
	// Extract the data
	///////////////////
	extractData( strResult );
}
//-------------------------------------------------------------------------------------------------
unsigned long LMData::generateOEMPassword(unsigned long ulKey1, 
										  unsigned long ulKey2, 
										  unsigned long ulKey3, 
										  unsigned long ulKey4) 
{
	unsigned long	ulKey5 = 0;

	// Check primary passwords
	if ((ulKey1 == 0) || (ulKey2 == 0) || (ulKey3 == 0) || (ulKey4 == 0))
	{
		// Create and throw exception
		UCLIDException	ue( "ELI03774", 
			"One or more license keys provided are invalid!" );
		ue.addDebugInfo( "License Key 1", ulKey1 ); 
		ue.addDebugInfo( "License Key 2", ulKey2 ); 
		ue.addDebugInfo( "License Key 3", ulKey3 ); 
		ue.addDebugInfo( "License Key 4", ulKey4 ); 
		throw ue;
	}

	// Generate the OEM password
	ulKey5 = ulKey1 % 42579 + ulKey2 % 33125 + ulKey3 % 84172 + ulKey4 % 76182;

	return ulKey5;
}
//-------------------------------------------------------------------------------------------------
unsigned long LMData::getComponentCount()
{
	unsigned long	ulCount = 0;

	ulCount = m_mapCompIDToData.size();

	return ulCount;
}
//-------------------------------------------------------------------------------------------------
long LMData::getFirstComponentID()
{
	long	lID = -1;

	// Locate beginning of map
	map<unsigned long, ComponentData>::iterator mapIter = 
		m_mapCompIDToData.begin();

	// Check that iterator is valid
	if (mapIter != m_mapCompIDToData.end())
	{
		lID = mapIter->first;
	}

	return lID;
}
//-------------------------------------------------------------------------------------------------
long LMData::getNextComponentID(long lID)
{
	long	lNextID = -1;

	// Locate specified ID in map
	map<unsigned long, ComponentData>::iterator mapIter = 
		m_mapCompIDToData.find(lID);

	// Check that ID was found
	if (mapIter != m_mapCompIDToData.end())
	{
		// Move to next entry in map
		mapIter++;

		// Check validity
		if (mapIter != m_mapCompIDToData.end())
		{
			lNextID = mapIter->first;
		}
	}

	return lNextID;
}
//-------------------------------------------------------------------------------------------------
const ComponentData & LMData::getComponentData(unsigned long ulID)
{
	ComponentData	CD;
	CD = m_mapCompIDToData[ulID];

	// Check to see if this object is in the map
	if (m_mapCompIDToData.find( ulID ) != m_mapCompIDToData.end())
	{
		// Item is in the map, return it
		return m_mapCompIDToData[ulID];
	}
	else
	{
		// Throw exception
		UCLIDException	ue( "ELI02625", 
			"Cannot retrieve component unless contained in map" );
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
string LMData::getComputerName()
{
	return m_strUserComputerName;
}
//-------------------------------------------------------------------------------------------------
string LMData::getCurrentVersion()
{
	return strCURRENT_VERSION;
}
//-------------------------------------------------------------------------------------------------
CTime & LMData::getIssueDate()
{
	return m_IssueDate;
}
//-------------------------------------------------------------------------------------------------
string LMData::getIssuerName()
{
	return m_strIssuerName;
}
//-------------------------------------------------------------------------------------------------
string LMData::getLicenseeName()
{
	return m_strLicenseeName;
}
//-------------------------------------------------------------------------------------------------
string LMData::getOrganizationName()
{
	return m_strOrganizationName;
}
//-------------------------------------------------------------------------------------------------
bool LMData::getUseComputerName()
{
	return m_bUseComputerName;
}
//-------------------------------------------------------------------------------------------------
bool LMData::getUseMACAddress()
{
	return m_bUseMACAddress;
}
//-------------------------------------------------------------------------------------------------
bool LMData::getUseSerialNumber()
{
	return m_bUseSerialNumber;
}
//-------------------------------------------------------------------------------------------------
string LMData::getUserComputerName()
{
	return m_strUserComputerName;
}
//-------------------------------------------------------------------------------------------------
string LMData::getUserMACAddress()
{
	return m_strUserMACAddress;
}
//-------------------------------------------------------------------------------------------------
unsigned long LMData::getUserSerialNumber()
{
	return m_ulUserSerialNumber;
}
//-------------------------------------------------------------------------------------------------
string LMData::getVersion()
{
	return m_strVersion;
}
//-------------------------------------------------------------------------------------------------
CTime LMData::getExpirationDate(unsigned long ulID)
{
	ASSERT_ARGUMENT("ELI23089", isLicensed(ulID));
	ASSERT_ARGUMENT("ELI23090", !m_mapCompIDToData[ulID].m_bIsLicensed);

	// Since it is licensed, the component is in the map and since it
	// is not permanently licensed, it has an expiration date, return the date
	return m_mapCompIDToData[ulID].m_ExpirationDate;
}
//-------------------------------------------------------------------------------------------------
bool LMData::isComponentFound(unsigned long ulID)
{
	bool	bFound = false;

	// Check to see if this object is in the map
	if (m_mapCompIDToData.find( ulID ) != m_mapCompIDToData.end())
	{
		// Item is in the map
		bFound = true;
	}

	return bFound;
}
//-------------------------------------------------------------------------------------------------
bool LMData::isLicensed(unsigned long ulComponentID)
{
	/////////////////////////
	// Test User License data
	/////////////////////////
	bool	bUserLicenseMatch = true;

	// Test Computer Name
	if (m_bUseComputerName)
	{
		if (m_strActualComputerName.compare( m_strUserComputerName ) != 0)
		{
			// Names do not match
			bUserLicenseMatch = false;
		}
	}

	// Test Disk Serial Number
	if (m_bUseSerialNumber)
	{
		if (m_ulActualSerialNumber != m_ulUserSerialNumber)
		{
			// Serial Numbers do not match
			bUserLicenseMatch = false;
		}
	}

	// Test MAC address
	if (m_bUseMACAddress)
	{
		if (m_strActualMACAddress.compare( m_strUserMACAddress ) != 0)
		{
			// Names do not match
			bUserLicenseMatch = false;
		}
	}

	// Just return False if a match failed
	if (!bUserLicenseMatch)
	{
		return false;
	}

	///////////////////
	// Check components
	///////////////////
	bool	bResult = false;

	// Locate component ID within map
	map<unsigned long, ComponentData>::iterator mapIter = 
		m_mapCompIDToData.find(ulComponentID);

	// Was this ID found in the map?
	if (mapIter != m_mapCompIDToData.end())
	{
		// Get component data
		ComponentData	CD( mapIter->second );

		// Check if this component has been disabled
		if (!CD.m_bDisabled)
		{
			// Check licensed state
			if (CD.m_bIsLicensed)
			{
				bResult = true;
			}
			// Not licensed, check expiration date
			else
			{
				if (CD.isExpired())
				{
					// Today is past the expiration date
					bResult = false;
				}
				else
				{
					// Component has not expired yet
					bResult = true;
				}
			}
		}
	}

	// Provide result to caller
	return bResult;
}
//-------------------------------------------------------------------------------------------------
bool LMData::isTemporaryLicense(unsigned long ulComponentID)
{
	ASSERT_ARGUMENT("ELI23064", isLicensed(ulComponentID));

	// The component is in the map since it is licensed, return
	// if it is temporary or not
	return !(m_mapCompIDToData[ulComponentID].isPermanent());
}
//-------------------------------------------------------------------------------------------------
bool LMData::isFirstComponentExpired()
{
	auto id = getFirstComponentID();

	return !isLicensed(id);
}
//-------------------------------------------------------------------------------------------------
void LMData::setIssuerName(const string& strName)
{
	m_strIssuerName = strName;
}
//-------------------------------------------------------------------------------------------------
void LMData::setLicenseeName(const string& strName)
{
	m_strLicenseeName = strName;
}
//-------------------------------------------------------------------------------------------------
void LMData::setOrganizationName(const string& strName)
{
	m_strOrganizationName = strName;
}
//-------------------------------------------------------------------------------------------------
void LMData::setUseComputerName(bool bUseName)
{
	m_bUseComputerName = bUseName;
}
//-------------------------------------------------------------------------------------------------
void LMData::setUseMACAddress(bool bUseAddress)
{
	m_bUseMACAddress = bUseAddress;
}
//-------------------------------------------------------------------------------------------------
void LMData::setUserString(const string& strData)
{
	// Store the string
	m_strUserString = strData;

	////////////////////////////////////
	// Decrypt the string and store info
	////////////////////////////////////
	try
	{
		ByteStream bytes( strData );
		ByteStream decryptedBS;

		// Decrypt the string here
		EncryptionEngine ee;
		ee.decrypt( decryptedBS, bytes, getUserLicensePassword() );

		ByteStreamManipulator bsm( ByteStreamManipulator::kRead, decryptedBS );

		// Extract User computer name
		bsm >> m_strUserComputerName;

		// Extract User disk serial number
		bsm >> m_ulUserSerialNumber;

		// Extract User MAC address
		bsm >> m_strUserMACAddress;

	}
	catch (UCLIDException& ue)
	{
		UCLIDException uexOuter("ELI03772", 
			"The User License string you have provided is not a valid license string!", ue);
		uexOuter.addDebugInfo("User License String", strData); 
		throw uexOuter;
	}
}
//-------------------------------------------------------------------------------------------------
string LMData::getUserLicenseString()
{
	ByteStream bytes;
	ByteStreamManipulator bsm(ByteStreamManipulator::kWrite, bytes);
	bsm << m_strUserComputerName;
	bsm << m_ulUserSerialNumber;
	bsm << m_strUserMACAddress;
	bsm.flushToByteStream(8);

	ByteStream encrypted;
	EncryptionEngine ee;
	ee.encrypt(encrypted, bytes, getUserLicensePassword());

	// Convert the encrypted stream of bytes to a string
	string strResult = encrypted.asString();

	// Convert the string to upper case
	makeUpperCase( strResult );

	return strResult;
}
//-------------------------------------------------------------------------------------------------
void LMData::setUseSerialNumber(bool bUseNumber)
{
	m_bUseSerialNumber = bUseNumber;
}
//-------------------------------------------------------------------------------------------------
void LMData::setIssueDate(const CTime& Date)
{
	m_IssueDate = Date;
}
//-------------------------------------------------------------------------------------------------
void LMData::setIssueDateToToday()
{
	// Get current time
	CTime	today;
	today = CTime::GetCurrentTime();

	// Save time as issue day
	m_IssueDate = today;
}
//-------------------------------------------------------------------------------------------------
// The string contained in the license file consists of two interlaced 
// encrypted strings: User String and UCLID String.  Each string contains 
// identical licensing information.  This method retrieves the specified 
// string.
string LMData::unzipStringFromFile(const string& strLicenseFile, bool bUserString)
{
	CStdioFile		fileIn;
	CFileException	e;
	CString			zLicenseInfo;
	string			strResult;

	// Check filename
	if (strLicenseFile.length() == 0)
	{
		// Create and throw exception
		UCLIDException	ue( "ELI03575", 
			"Empty string provided for name of license file!" );
		throw ue;
	}

	// Open the license file and retrieve the license string
	if (!fileIn.Open( strLicenseFile.c_str(), 
		CFile::modeRead | CFile::shareDenyWrite, &e ) )
	{
		// Create and throw exception
		UCLIDException	ue( "ELI03771", 
			"Unable to open license file!" );
		ue.addDebugInfo( "License File", (LPCTSTR)e.m_strFileName );
		ue.addDebugInfo( "m_cause", e.m_cause );
		ue.addDebugInfo( "m_lOsError", e.m_lOsError );
		throw ue;
	}
	else
	{
		// First read the version string
		CString	zTemp;
		fileIn.ReadString( zTemp );

		// Validate the version string
		long lLength = zTemp.GetLength();
		if ((lLength == 0) || (lLength > 2))
		{
			// Create and throw exception
			UCLIDException	ue( "ELI03773", 
				"Invalid version string" );
			ue.addDebugInfo( "License File", strLicenseFile ); 
			ue.addDebugInfo( "Version String", zTemp.operator LPCTSTR() ); 
			throw ue;
		}
		else
		{
			m_strVersion = zTemp.operator LPCTSTR();
		}

		// Successfully opened the file, so read the license string
		fileIn.ReadString( zLicenseInfo );

		// Close the file
		fileIn.Close();
	}

	// Step through string and retrieve every other character
	//    User  String = even characters
	//    UCLID String = odd  characters
	for (int i = bUserString ? 0 : 1; i < zLicenseInfo.GetLength(); i += 2)
	{
		strResult += zLicenseInfo[i];
	}

	// Return the retrieved string
	return strResult;
}
//-------------------------------------------------------------------------------------------------
// The string contained in the license file consists of two interlaced 
// encrypted strings: User String and UCLID String.  Each string contains 
// identical licensing information.  This method interlaces the given strings 
// and writes the result to the specified file.
bool LMData::zipStringsToFile(const string& strLicenseFile, 
							  const string& strVersion, 
							  const string& strData1, 
							  const string& strData2)
{
	bool	bSuccess = false;
	string	strFinal;

	// Check filename
	if (strLicenseFile.length() == 0)
	{
		// Create and throw exception
		UCLIDException	ue( "ELI03572", 
			"Empty string provided for name of license file!" );
		throw ue;
	}

	// Check first string
	if (strData1.length() == 0)
	{
		// Create and throw exception
		UCLIDException	ue( "ELI03769", 
			"Empty string provided for encrypted license data!" );
		throw ue;
	}

	// If second string is empty, just write the first string directly
	if (strData2.length() == 0)
	{
		CStdioFile		fileOut;
		CFileException	e;
		if( fileOut.Open( strLicenseFile.c_str(), 
			CFile::modeCreate | CFile::modeWrite, &e ) )
		{
			// First write the version string
			CString	zTemp = strVersion.c_str();
			zTemp += "\n";
			fileOut.WriteString( zTemp );

			// Successfully opened the file, so write the license string
			fileOut.WriteString( strData1.c_str() );

			// Close the file and set success flag
			fileOut.Close();
			waitForFileToBeReadable(strLicenseFile);
			bSuccess = true;
		}
	}
	else
	{
		// Verify that string lengths are identical
		if (strData1.length() != strData2.length())
		{
			// Create and throw exception
			UCLIDException	ue( "ELI03770", 
				"Encrypted license data strings have unequal length!" );
			ue.addDebugInfo( "String 1 Length: ", strData1.length() );
			ue.addDebugInfo( "String 2 Length: ", strData2.length() );
			throw ue;
		}
		else
		{
			// Step through strings and concatenate the characters
			for (unsigned int i = 0; i < strData1.length(); i++)
			{
				strFinal += strData1[i];
				strFinal += strData2[i];
			}

			// Write the final string to the output file
			CStdioFile		fileOut;
			CFileException	e;
			if( fileOut.Open( strLicenseFile.c_str(), 
				CFile::modeCreate | CFile::modeWrite, &e ) )
			{
				// First write the version string
				CString	zTemp = strVersion.c_str();
				zTemp += "\n";
				fileOut.WriteString( zTemp );

				// Successfully opened the file, so write the license string
				fileOut.WriteString( strFinal.c_str() );

				// Close the file and set success flag
				fileOut.Close();
				waitForFileToBeReadable(strLicenseFile);
				bSuccess = true;
			}
		}
	}

	return bSuccess;
}
//-------------------------------------------------------------------------------------------------
void LMData::enableAll()
{
	// Go through the map of component data objects setting the disabled flag
	// for each component to false
	for (map<unsigned long, ComponentData>::iterator it = m_mapCompIDToData.begin();
		it != m_mapCompIDToData.end(); it++)
	{
		it->second.m_bDisabled = false;
	}
}
//-------------------------------------------------------------------------------------------------
void LMData::enableId(unsigned long ulComponentID)
{
	// Look for the specified component in the map
	map<unsigned long, ComponentData>::iterator it = m_mapCompIDToData.find(ulComponentID);

	// If the component was found, set its disabled flag to false
	if (it != m_mapCompIDToData.end())
	{
		it->second.m_bDisabled = false;
	}
}
//-------------------------------------------------------------------------------------------------
void LMData::disableAll()
{
	// Go through the map of component data objects setting the disabled flag
	// for each component to true
	for (map<unsigned long, ComponentData>::iterator it = m_mapCompIDToData.begin();
		it != m_mapCompIDToData.end(); it++)
	{
		it->second.m_bDisabled = true;
	}
}
//-------------------------------------------------------------------------------------------------
void LMData::disableId(unsigned long ulComponentID)
{
	// Look for the specified component in the map
	map<unsigned long, ComponentData>::iterator it = m_mapCompIDToData.find(ulComponentID);

	// If the component was found, set its disabled flag to true
	if (it != m_mapCompIDToData.end())
	{
		it->second.m_bDisabled = true;
	}
}
