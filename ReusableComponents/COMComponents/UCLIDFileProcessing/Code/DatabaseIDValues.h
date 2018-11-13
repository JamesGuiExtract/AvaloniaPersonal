#pragma once

#include <ADOUtils.h>

#include <ByteStreamManipulator.h>
#include <UCLIDException.h>

// Class that represents the decrypted Database ID
class DatabaseIDValues
{
public:
	DatabaseIDValues();
	DatabaseIDValues(const string &strEncrypted);
	GUID m_GUID;
	string m_strServer;
	string m_strName;
	SYSTEMTIME m_stCreated;
	SYSTEMTIME m_stRestored;
	SYSTEMTIME m_stLastUpdated;

	// This will only be filled in when CheckIfValid is called with bGenerateInvalidReason = true
	string m_strInvalidReason;

	// This is not saved but will be calculated when read from a stream
	long m_nHashValue;

	// Checks the contained values against the values returned for the current connection
	// returns true if valid if not valid will return false or throw exception if bThrowIfInvalid
	// is true
	// Fills in the m_strInvalidReason if bGenerateInvalidReason is true otherwise m_strInvalidReason will 
	// be set to ""
	bool CheckIfValid(_ConnectionPtr ipConnection, bool bThrowIfInvalid = false, bool bGenerateInvalidReason = false);

	// This will return a string that identifies what parts of the Database id are invalid
	// if the database id is valid the return string will be ""
	string ReasonInvalid(_ConnectionPtr ipConnection);
	
	// Calculates the hash value for this class (does not include m_nHashValue)
	void CalculateHashValue(long &nHashValue);

	// Adds the values to the debug data of the exception using strPrefix in front of the member values names
	// the Server and Database Names are the only items add not encrypted
	void addAsDebugInfo(UCLIDException &ue, string strPrefix);

	// Comparison operators
	bool operator !=(const DatabaseIDValues &other);
	bool operator ==(const DatabaseIDValues &other);
};

// Methods to stream the DatabaseIDValues to and from a ByteStream
ByteStreamManipulator& operator >> (ByteStreamManipulator & bsm, DatabaseIDValues &databaseIDValues);
ByteStreamManipulator& operator << (ByteStreamManipulator & bsm, const DatabaseIDValues &databaseIDValues);
