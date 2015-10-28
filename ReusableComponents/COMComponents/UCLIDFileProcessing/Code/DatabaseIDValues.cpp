
#include "stdafx.h"
#include "DatabaseIDValues.h"

#include <FAMUtilsConstants.h>
#include <EncryptionEngine.h>
#include <cpputil.h>
#include <UCLIDException.h>

#include <functional>

using namespace std;

// Defined in FileProcessingDB_Internal.cpp
void getFAMPassword(ByteStream &bsPW);


//-------------------------------------------------------------------------------------------------
// DatabaseIDValues Constructors
//-------------------------------------------------------------------------------------------------
DatabaseIDValues::DatabaseIDValues()
	:m_strServer(""), m_strName(""), m_ctCreated(0), m_ctRestored(0), m_ctLastUpdated(0), m_nHashValue(0)
{
	m_GUID.Data1 = 0;
	m_GUID.Data2 = 0;
	m_GUID.Data3 = 0;
	memset(m_GUID.Data4,0, sizeof(m_GUID.Data4));
}
//-------------------------------------------------------------------------------------------------
DatabaseIDValues::DatabaseIDValues(const string &strEncrypted)
{
	ByteStream bsPassword;
	getFAMPassword(bsPassword);

	// Get the decrypted ByteSteam
	ByteStream bsDatabaseID = MapLabel::getMapLabelWithS(strEncrypted, bsPassword);
	ByteStreamManipulator bsm(ByteStreamManipulator::kRead, bsDatabaseID);

	bsm >> *this;
}

//-------------------------------------------------------------------------------------------------
// DatabaseIDValues operators
//-------------------------------------------------------------------------------------------------
ByteStreamManipulator& operator >> (ByteStreamManipulator & bsm, DatabaseIDValues &databaseIDValues)
{
	bsm >> databaseIDValues.m_GUID;
	bsm >> databaseIDValues.m_strServer;
	bsm >> databaseIDValues.m_strName;
	bsm >> databaseIDValues.m_ctCreated;
	bsm >> databaseIDValues.m_ctRestored;
	bsm >> databaseIDValues.m_ctLastUpdated;
	databaseIDValues.CalculateHashValue(databaseIDValues.m_nHashValue);
	return bsm;
}
//-------------------------------------------------------------------------------------------------
ByteStreamManipulator& operator << (ByteStreamManipulator & bsm, const DatabaseIDValues &databaseIDValues)
{
	bsm << databaseIDValues.m_GUID;
	bsm << databaseIDValues.m_strServer;
	bsm << databaseIDValues.m_strName;
	bsm << databaseIDValues.m_ctCreated;
	bsm << databaseIDValues.m_ctRestored;
	bsm << databaseIDValues.m_ctLastUpdated;
	return bsm;
}
//-------------------------------------------------------------------------------------------------
bool DatabaseIDValues::CheckIfValid(_ConnectionPtr ipConnection, bool bThrowIfInvalid)
{
	// Get the expected values
	string strServer;
	CTime ctCreationDate, ctRestoreDate;
	string strDatabaseName = ipConnection->DefaultDatabase;
	
	getDatabaseCreationDateAndRestoreDate(ipConnection, strDatabaseName, strServer, ctCreationDate, ctRestoreDate);

	makeLowerCase(strDatabaseName);
	makeLowerCase(strServer);
	
	DatabaseIDValues tmp = *this;

	makeLowerCase(tmp.m_strServer);
	makeLowerCase(tmp.m_strName);

	if (tmp.m_strServer == strServer && tmp.m_strName == strDatabaseName
		&& m_ctCreated == ctCreationDate && m_ctRestored == ctRestoreDate)
	{
		return true;
	}

	if (bThrowIfInvalid)
	{
		UCLIDException ue("ELI38796", "DatabaseID is invalid.");
		ue.addDebugInfo("SavedServer", m_strServer, true);
		ue.addDebugInfo("SavedDatabase", m_strName, true);
		ue.addDebugInfo("SavedCreation", 
			(LPCSTR)m_ctCreated.Format(gstrDATE_TIME_FORMAT.c_str()), true);
		ue.addDebugInfo("SavedRestored", 
			(LPCSTR)m_ctRestored.Format(gstrDATE_TIME_FORMAT.c_str()), true);
		ue.addDebugInfo("ExpectedServer", strServer, true);
		ue.addDebugInfo("ExpectedDatabase", strDatabaseName, true);
		ue.addDebugInfo("ExpectedCreationDate", 
			(LPCSTR)ctCreationDate.Format(gstrDATE_TIME_FORMAT.c_str()), true);
		ue.addDebugInfo("ExpectedRestoreDate",
			(LPCSTR)ctRestoreDate.Format(gstrDATE_TIME_FORMAT.c_str()), true);
		throw ue;
	}
	return false;
}
//-------------------------------------------------------------------------------------------------
void DatabaseIDValues::CalculateHashValue(long &nHashValue)
{
	hash<DWORD> hashDWORD;
	hash<string> hashString;
	hash<long long> hashLongLong;
	
	DWORD *pdwGUIDData = (DWORD *)&m_GUID;
	nHashValue = hashDWORD(pdwGUIDData[0]);
	nHashValue ^= hashDWORD(pdwGUIDData[1]);
	nHashValue ^= hashDWORD(pdwGUIDData[2]);
	nHashValue ^= hashDWORD(pdwGUIDData[3]);
	nHashValue ^= hashString(m_strServer) << 1;
	nHashValue ^= hashString(m_strName) << 2;
	nHashValue ^= hashLongLong((long long)m_ctCreated.GetTime()) << 3;
	nHashValue ^= hashLongLong((long long)m_ctRestored.GetTime()) << 4;
	nHashValue ^= hashLongLong((long long)m_ctLastUpdated.GetTime());
}
//-------------------------------------------------------------------------------------------------
bool DatabaseIDValues::operator !=(const DatabaseIDValues &other)
{
	return m_GUID != other.m_GUID ||
		m_strServer != other.m_strServer ||
		m_strName != other.m_strName ||
		m_ctCreated != other.m_ctCreated ||
		m_ctRestored != other.m_ctRestored ||
		m_ctLastUpdated != other.m_ctLastUpdated;
}
//-------------------------------------------------------------------------------------------------
bool DatabaseIDValues::operator ==(const DatabaseIDValues &other)
{
	return m_GUID == other.m_GUID &&
		m_strServer == other.m_strServer &&
		m_strName == other.m_strName &&
		m_ctCreated == other.m_ctCreated &&
		m_ctRestored == other.m_ctRestored &&
		m_ctLastUpdated == other.m_ctLastUpdated;
}
