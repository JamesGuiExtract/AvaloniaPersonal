#pragma once

#include "DatabaseIDValues.h"

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
	long m_nAlertLevel;
	long m_nAlertMultiple;
	bool m_bUnrecoverable;
	long m_nChangeLogValue;

	// Whenever either validate or isValid are called and find the counter to be invalid, the reason
	// for the validation error is maintained here. This reason is intended for Extract support
	// purposes (via unlock codes) and not for display to the customer.
	string m_strValidationError;

	// This is expected to be (<DatabaseID Hash> << 10) + m_nID
	long m_nDatabaseIDCounterIDHash;

	// This will only validate that the counterID portion of the m_nDatabaseIDCounterIDHash = m_nID
	// It does not validate the databaseID hash
	void LoadFromFields(FieldsPtr ipFields);

	// Returns a string of the encrypted m_nValue & CounterIDHash
	string getEncrypted(const DatabaseIDValues databaseID);

	// Throws an exception if checks of the counter validity fail. If ipFields is specified, the
	// counter's value with be cross checked with the SecureCounterValueChange table. Use of
	// ipFields assumes ipFields was generated with gstrSELECT_SECURE_COUNTER_WITH_MAX_VALUE_CHANGE.
	void validate(const DatabaseIDValues databaseID, FieldsPtr ipFields = nullptr);

	// Returns true if checks of the counter validity succeed.
	// Assumes ipFields generated with gstrSELECT_SECURE_COUNTER_WITH_MAX_VALUE_CHANGE.
	bool isValid(const DatabaseIDValues databaseID, FieldsPtr ipFields = nullptr);

	// Map that maps the CounterID to Name for the standard counters
	static map<long, string> ms_mapOfStandardNames;
};
