#pragma once

#include <ADOUtils.h>

using namespace ADODB;

// Class that represents a record in the SecureCounterValueChange table
class DBCounterChangeValue
{
public:
	DBCounterChangeValue();
	long m_nID;
	long m_nCounterID;
	long m_nToValue;
	long m_nFromValue;
	SYSTEMTIME m_stUpdatedTime;
	long m_nLastUpdatedByFAMSessionID;
	long long m_llMinFAMFileCount;
	long long m_llHashValue;
	string m_strComment;

	// Calculates a hash value for the recored (does not include m_llHashValue
	void CalculateHashValue(long long &llHashValue);

	// Assumes results in ipFields using gstrSELECT_SECURE_COUNTER_WITH_MAX_VALUE_CHANGE
	// Which has SecureCounter.ID as ID and SecureCounterValueChange.ID as ValueChangedID
	// rnToValue returns the "ToValue" for the counter from the SecureCounterValueChange table.
	void LoadFromFields(FieldsPtr ipFields, bool bValidateHash, long& rnToValue);

	// Returns the query to insert the record into the SecureCounterValueChange table
	string GetInsertQuery();
};
