#pragma once


#include <ADOUtils.h>
#include <ByteStreamManipulator.h>

#include <string>

using namespace ADODB;



// Class that represents a counter from the SecureCounter table
class DBCounter
{
public:
	DBCounter();
	DBCounter(long nID, string strName, long nValue);
		
	long m_nID;
	string m_strName;
	long m_nValue;

	// This is expected to be (<DatabaseID Hash> << 10) + m_nID
	long m_nDatabaseIDCounterIDHash;

	// Assumes results in ipFields using gstrSELECT_SECURE_COUNTER_WITH_MAX_VALUE_CHANGE
	// Which has SecureCounter.ID as ID and SecureCounterValueChange.ID as ValueChangedID
	// if bValidate is false the only requires SecureCounter fields
	void LoadFromFields(FieldsPtr ipFields, const long nDatabaseIDHash, bool bValidate);

	// This will only validate that the counterID portion of the m_nDatabaseIDCounterIDHash = m_nID
	// It does not validate the databaseID hash
	void LoadFromFields(FieldsPtr ipFields);

	// Returns a string of the encrypted m_nValue & CounterIDHash
	string getEncrypted(const long nDatabaseIDHash);

	// Returns true if the m_nDatabaseIDCounterIDHash is the expected value given nDatabaseIDHash
	bool isValid(const long nDatabaseIDHash);

	// Map that maps the CounterID to Name for the standard counters
	static map<long, string> ms_mapOfStandardNames;
};
