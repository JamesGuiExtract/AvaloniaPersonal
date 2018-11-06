#include "stdafx.h"
#include "DBCounterChangeValue.h"

#include <UCLIDException.h>

//-------------------------------------------------------------------------------------------------
// DBCounterChangeValue 
//-------------------------------------------------------------------------------------------------
DBCounterChangeValue::DBCounterChangeValue()
	: m_nID(0)
	, m_nCounterID(0)
	, m_nToValue(0)
	, m_nFromValue(0)
	, m_ctUpdatedTime(0)
	, m_nLastUpdatedByFAMSessionID(0)
	, m_llMinFAMFileCount(0)
	, m_llHashValue(0)
{
}
//-------------------------------------------------------------------------------------------------
void DBCounterChangeValue::CalculateHashValue(long long &llHashValue)
{
	hash<long> hashLong;
	hash<long long> hashLonglong;
	hash<string> hashString;

	llHashValue = hashLonglong(m_llMinFAMFileCount);
	llHashValue ^= hashLong(m_nCounterID) << 1;
	llHashValue ^= hashLong(m_nToValue) << 2;
	llHashValue ^= hashLong(m_nFromValue) << 3;
	llHashValue ^= hashLonglong((long long)m_ctUpdatedTime.GetTime()) << 4;
	llHashValue ^= hashLong(m_nLastUpdatedByFAMSessionID) << 5;
	llHashValue ^= hashLonglong(m_llMinFAMFileCount) << 6;
	if (!m_strComment.empty())
	{
		llHashValue ^= hashString(m_strComment);
	}
}
//-------------------------------------------------------------------------------------------------
void DBCounterChangeValue::LoadFromFields(FieldsPtr ipFields, bool bValidateHash, long& rnToValue)
{
	try
	{
		m_nID = getLongField(ipFields, "ValueChangedID");
		m_nCounterID = getLongField(ipFields, "ID");
		m_nToValue = getLongField(ipFields, "ToValue");
		rnToValue = m_nToValue; 
		m_nFromValue = getLongField(ipFields, "FromValue");
		string tempDate = getStringField(ipFields, "LastUpdatedTime");
		m_ctUpdatedTime = getTimeDateField(ipFields, "LastUpdatedTime", true);

		// This field could be null 
		if (isNULL(ipFields, "LastUpdatedByFAMSessionID"))
		{
			m_nLastUpdatedByFAMSessionID = 0;
		}
		else
		{
			m_nLastUpdatedByFAMSessionID = getLongField(ipFields, "LastUpdatedByFAMSessionID");
		}
		m_llMinFAMFileCount = getLongLongField(ipFields, "MinFAMFileCount");
		m_llHashValue = getLongLongField(ipFields, "HashValue");

		m_strComment = getStringField(ipFields, "Comment");

		// Validate the HASH
		if (bValidateHash)
		{
			long long nCalculatedHash;
			CalculateHashValue(nCalculatedHash);
			if (nCalculatedHash != m_llHashValue)
			{
				UCLIDException ue("ELI38929", "Counter Value change Hash value is invalid.");
				ue.addDebugInfo("SecureCounterValueChangeID", m_nID);
				ue.addDebugInfo("CalculatedHash", nCalculatedHash, true);
				ue.addDebugInfo("ExpectedHash", m_llHashValue, true);
				ue.addDebugInfo("UpdatedTime", m_ctUpdatedTime.FormatGmt("%Y-%m-%d %H:%M:%S %z").operator LPCSTR(), true);
				ue.addDebugInfo("UpdatedTimeAsStringFromDB", tempDate, true);
				throw ue;
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38928"); 
}
//-------------------------------------------------------------------------------------------------
string DBCounterChangeValue::GetInsertQuery()
{
	string strInsertQuery = "INSERT INTO [dbo].[SecureCounterValueChange] "
		" (CounterID, FromValue, ToValue, LastUpdatedTime, LastUpdatedByFAMSessionID, MinFamFileCount, HashValue, Comment) "
		"VALUES (";

	// Format the time as UTC 
	string strTime = m_ctUpdatedTime.FormatGmt("%Y-%m-%d %H:%M:%S +00:00");
	
	strInsertQuery += asString(m_nCounterID) + ", ";
	strInsertQuery += asString(m_nFromValue) + ", ";
	strInsertQuery += asString(m_nToValue) + ", ";
	strInsertQuery += "'" + strTime + "', ";
	strInsertQuery += (m_nLastUpdatedByFAMSessionID == 0) ? "NULL, " : asString(m_nLastUpdatedByFAMSessionID) + ", ";
	strInsertQuery += asString(m_llMinFAMFileCount) + ", ";
	strInsertQuery += asString(m_llHashValue) + ", ";
	strInsertQuery += "'" + m_strComment + "')";
	return strInsertQuery;
}
