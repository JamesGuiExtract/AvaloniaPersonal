#pragma once

#include <ADOUtils.h>

#include <ByteStreamManipulator.h>

// Class that represents the decrypted Database ID
class DatabaseIDValues
{
public:
	DatabaseIDValues();
	DatabaseIDValues(const string &strEncrypted);
	GUID m_GUID;
	string m_strServer;
	string m_strName;
	CTime m_ctCreated;
	CTime m_ctRestored;
	CTime m_ctLastUpdated;

	// This is not saved but will be calculated when read from a stream
	long m_nHashValue;

	// Checks the contained values against the values returned for the current connection
	// returns true if valid if not valid will return false or throw exception if bThrowIfInvalid
	// is true
	bool CheckIfValid(_ConnectionPtr ipConnection, bool bThrowIfInvalid = false);
	
	// Calculates the hash value for this class (does not include m_nHashValue)
	void CalculateHashValue(long &nHashValue);

	// Comparison operators
	bool operator !=(const DatabaseIDValues &other);
	bool operator ==(const DatabaseIDValues &other);
};

// Methods to stream the DatabaseIDValues to and from a ByteStream
ByteStreamManipulator& operator >> (ByteStreamManipulator & bsm, DatabaseIDValues &databaseIDValues);
ByteStreamManipulator& operator << (ByteStreamManipulator & bsm, const DatabaseIDValues &databaseIDValues);
