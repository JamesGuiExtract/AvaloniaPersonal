#include "stdafx.h"
#include "DBCounterChangeValue.h"

#include <DateUtil.h>
#include <UCLIDException.h>

//-------------------------------------------------------------------------------------------------
// DBCounterChangeValue 
//-------------------------------------------------------------------------------------------------
DBCounterChangeValue::DBCounterChangeValue(DatabaseIDValues databaseID)
	: m_nID(0)
	, m_nCounterID(0)
	, m_nToValue(0)
	, m_nFromValue(0)
	, m_nLastUpdatedByFAMSessionID(0)
	, m_llMinFAMFileCount(0)
	, m_llHashValue(0)
{
	ZeroMemory(&m_stUpdatedTime, sizeof(SYSTEMTIME));

	m_DatabaseID = databaseID;
}
//-------------------------------------------------------------------------------------------------
void DBCounterChangeValue::CalculateHashValue(long long &llHashValue)
{
	hash<long> hashLong;
	hash<long long> hashLonglong;
	hash<string> hashString;

	long updatedTimeHash = hashLonglong(asULongLong(m_stUpdatedTime));

	// Distribute overlapping hashed values over all 64 bits
	llHashValue = 0;
	llHashValue = hashLonglong(m_llMinFAMFileCount);
	llHashValue ^= (long long)hashLong(m_nCounterID) << 4;
	llHashValue ^= (long long)hashLong(m_nToValue) << 8;
	llHashValue ^= (long long)hashLong(m_nFromValue) << 12;
	llHashValue ^= (long long)updatedTimeHash << 16;
	llHashValue ^= (long long)hashLong(m_nLastUpdatedByFAMSessionID) << 24;
	llHashValue ^= (long long)hashLonglong(m_llMinFAMFileCount) << 28;
	if (!m_strComment.empty())
	{
		llHashValue ^= (long long)hashString(m_strComment) << 32;
	}

	// Above hash often leaves many bits unaffected between neighboring rows. Salt the hash using
	// data-dependent psuedo random shifts of DB guid to improve distribution
	long long *plGUIDData = (long long *)&m_DatabaseID.m_GUID;
	llHashValue ^= plGUIDData[0];
	llHashValue ^= plGUIDData[1] << (m_nToValue % 32);
	llHashValue ^= plGUIDData[0] >> (m_nFromValue % 32);
	llHashValue ^= plGUIDData[1] << (updatedTimeHash % 32);
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
		m_stUpdatedTime = getTimeDateField(ipFields, "LastUpdatedTime");

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
			long long nCalculatedHash = 0;
			CalculateHashValue(nCalculatedHash);
			if (nCalculatedHash != m_llHashValue)
			{
				UCLIDException ue("ELI38929", "Counter Value change Hash value is invalid.");
				ue.addDebugInfo("SecureCounterValueChangeID", m_nID);
				ue.addDebugInfo("CalculatedHash", nCalculatedHash, true);
				ue.addDebugInfo("ExpectedHash", m_llHashValue, true);
				ue.addDebugInfo("UpdatedTime", (LPCTSTR)CTime(m_stUpdatedTime).Format("%Y-%m-%d %H:%M:%S"), true);
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
	string strTime = CTime(m_stUpdatedTime).Format("%Y-%m-%d %H:%M:%S");
	
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
