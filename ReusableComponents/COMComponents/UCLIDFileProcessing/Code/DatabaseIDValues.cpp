
#include "stdafx.h"
#include "DatabaseIDValues.h"

#include <FAMUtilsConstants.h>
#include <EncryptionEngine.h>
#include <cpputil.h>
#include <UCLIDException.h>
#include <DateUtil.h>

#include <functional>

using namespace std;

// Defined in FileProcessingDB_Internal.cpp
void getFAMPassword(ByteStream &bsPW);


//-------------------------------------------------------------------------------------------------
// DatabaseIDValues Constructors
//-------------------------------------------------------------------------------------------------
DatabaseIDValues::DatabaseIDValues()
	:m_strServer(""), m_strName(""), m_nHashValue(0), m_strInvalidReason("")
{
	m_GUID.Data1 = 0;
	m_GUID.Data2 = 0;
	m_GUID.Data3 = 0;
	memset(m_GUID.Data4,0, sizeof(m_GUID.Data4));
	ZeroMemory(&m_stCreated, sizeof(SYSTEMTIME));
	ZeroMemory(&m_stRestored, sizeof(SYSTEMTIME));
	ZeroMemory(&m_stLastUpdated, sizeof(SYSTEMTIME));
}
//-------------------------------------------------------------------------------------------------
DatabaseIDValues::DatabaseIDValues(const string &strEncrypted)
{
	try
	{
		ByteStream bsPassword;
		getFAMPassword(bsPassword);

		// Get the decrypted ByteSteam
		ByteStream bsDatabaseID = MapLabel::getMapLabelWithS(strEncrypted, bsPassword);
		ByteStreamManipulator bsm(ByteStreamManipulator::kRead, bsDatabaseID);

		bsm >> *this;
	}
	catch (...)
	{
		throw  uex::fromCurrent("ELI53103");
	}
}

//-------------------------------------------------------------------------------------------------
// DatabaseIDValues operators
//-------------------------------------------------------------------------------------------------
ByteStreamManipulator& operator >> (ByteStreamManipulator & bsm, DatabaseIDValues &databaseIDValues)
{
	try
	{
		bsm >> databaseIDValues.m_GUID;
		bsm >> databaseIDValues.m_strServer;
		bsm >> databaseIDValues.m_strName;
		bsm >> databaseIDValues.m_stCreated;
		bsm >> databaseIDValues.m_stRestored;
		bsm >> databaseIDValues.m_stLastUpdated;

		databaseIDValues.CalculateHashValue(databaseIDValues.m_nHashValue);

		return bsm;
	}
	catch (...)
	{
		throw UCLIDException("ELI53102", "Failure parsing database ID", uex::fromCurrent("ELI53101"));
	}
}
//-------------------------------------------------------------------------------------------------
ByteStreamManipulator& operator << (ByteStreamManipulator & bsm, const DatabaseIDValues &databaseIDValues)
{
	bsm << databaseIDValues.m_GUID;
	bsm << databaseIDValues.m_strServer;
	bsm << databaseIDValues.m_strName;
	bsm << databaseIDValues.m_stCreated;
	bsm << databaseIDValues.m_stRestored;
	bsm << databaseIDValues.m_stLastUpdated;

	return bsm;
}
//-------------------------------------------------------------------------------------------------
bool DatabaseIDValues::CheckIfValid(_ConnectionPtr ipConnection, string strServer, 
									bool bThrowIfInvalid, bool bGenerateInvalidReason)
{
	// Reset the invalid reason
	m_strInvalidReason = "";

	// Get the expected values
	SYSTEMTIME stCreationDate, stRestoreDate;
	string strDatabaseName = ipConnection->DefaultDatabase;
	
	getDatabaseInfo(ipConnection, strDatabaseName, strServer, stCreationDate, stRestoreDate);

	makeLowerCase(strDatabaseName);
	makeLowerCase(strServer);
	
	DatabaseIDValues tmp = *this;

	makeLowerCase(tmp.m_strServer);
	makeLowerCase(tmp.m_strName);

	if (tmp.m_strServer == strServer && tmp.m_strName == strDatabaseName
		&& asULongLong(m_stLastUpdated) > asULongLong(stCreationDate)
		&& asULongLong(m_stLastUpdated) > asULongLong(stRestoreDate))
	{
		return true;
	}

	if (bGenerateInvalidReason)
	{
		// Determine the reasons the code is invalid
		vector<string> vecReasons;
		if (tmp.m_strServer != strServer)
		{
			vecReasons.push_back("Server has changed");
		}
		if (tmp.m_strName != strDatabaseName)
		{
			vecReasons.push_back("Database has changed");
		}
		if (asULongLong(m_stLastUpdated) <= asULongLong(stCreationDate))
		{
			vecReasons.push_back("Unexpected creation date");
		}
		if (asULongLong(m_stLastUpdated) <= asULongLong(stRestoreDate))
		{
			vecReasons.push_back("Unexpected restore date");
		}
		if (vecReasons.size() > 0)
		{
			m_strInvalidReason = asString(vecReasons, false, ", ");
		}
	}

	UCLIDException ue("ELI38796", "DatabaseID is invalid.");
	ue.addDebugInfo("SavedServer", m_strServer, true);
	ue.addDebugInfo("SavedDatabase", m_strName, true);
	ue.addDebugInfo("ExpectedServer", strServer, true);
	ue.addDebugInfo("ExpectedDatabase", strDatabaseName, true);
	try
	{
		ue.addDebugInfo("SavedCreation",
			(LPCSTR)CTime(m_stCreated).Format(gstrDATE_TIME_FORMAT.c_str()), true);
		ue.addDebugInfo("SavedRestored",
			(LPCSTR)CTime(m_stRestored).Format(gstrDATE_TIME_FORMAT.c_str()), true);
		ue.addDebugInfo("ExpectedCreationDate",
			(LPCSTR)CTime(stCreationDate).Format(gstrDATE_TIME_FORMAT.c_str()), true);
		ue.addDebugInfo("ExpectedRestoreDate",
			(LPCSTR)CTime(stRestoreDate).Format(gstrDATE_TIME_FORMAT.c_str()), true);
	}
	catch (...) {}

	(bThrowIfInvalid) ? throw ue : ue.log();
	
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
	nHashValue ^= hashLongLong(asULongLong(m_stCreated)) << 3;
	nHashValue ^= hashLongLong(asULongLong(m_stRestored)) << 4;
	nHashValue ^= hashLongLong(asULongLong(m_stLastUpdated));
}
//-------------------------------------------------------------------------------------------------
void DatabaseIDValues::addAsDebugInfo(UCLIDException &ue, string strPrefix)
{
	ue.addDebugInfo(strPrefix + "Server", m_strServer);
	ue.addDebugInfo(strPrefix + "DBName", m_strName);
	ue.addDebugInfo(strPrefix + "DBGUID", asString(m_GUID), true);
	try
	{
		ue.addDebugInfo(strPrefix + "Created", (LPCSTR)CTime(m_stCreated).Format(gstrDATE_TIME_FORMAT.c_str()), true);
		ue.addDebugInfo(strPrefix + "Restored", (LPCSTR)CTime(m_stRestored).Format(gstrDATE_TIME_FORMAT.c_str()), true);
		ue.addDebugInfo(strPrefix + "LastUpdated", (LPCSTR)CTime(m_stLastUpdated).Format(gstrDATE_TIME_FORMAT.c_str()), true);
	}
	catch (...) {}
}
//-------------------------------------------------------------------------------------------------
bool DatabaseIDValues::operator !=(const DatabaseIDValues &other)
{
	return m_GUID != other.m_GUID ||
		m_strServer != other.m_strServer ||
		m_strName != other.m_strName ||
		asULongLong(m_stCreated) != asULongLong(other.m_stCreated) ||
		asULongLong(m_stRestored) != asULongLong(other.m_stRestored) ||
		asULongLong(m_stLastUpdated) != asULongLong(other.m_stLastUpdated);
}
//-------------------------------------------------------------------------------------------------
bool DatabaseIDValues::operator ==(const DatabaseIDValues &other)
{
	return m_GUID == other.m_GUID &&
		m_strServer == other.m_strServer &&
		m_strName == other.m_strName &&
		asULongLong(m_stCreated) == asULongLong(other.m_stCreated) &&
		asULongLong(m_stRestored) == asULongLong(other.m_stRestored) &&
		asULongLong(m_stLastUpdated) == asULongLong(other.m_stLastUpdated);
}
