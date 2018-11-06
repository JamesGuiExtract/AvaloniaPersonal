
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
	:m_strServer(""), m_strName(""), m_ctCreated(0), m_ctRestored(0), m_ctLastUpdated(0), m_nHashValue(0),
	m_strInvalidReason("")
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
bool DatabaseIDValues::CheckIfValid(_ConnectionPtr ipConnection, bool bThrowIfInvalid, bool bGenerateInvalidReason,
	bool bNoTZConversion)
{
	// Reset the invalid reason
	m_strInvalidReason = "";

	// Get the expected values
	string strServer;
	CTime ctCreationDate, ctRestoreDate;
	string strDatabaseName = ipConnection->DefaultDatabase;
	
	getDatabaseInfo(ipConnection, strDatabaseName, strServer, ctCreationDate, ctRestoreDate, bNoTZConversion);

	makeLowerCase(strDatabaseName);
	makeLowerCase(strServer);
	
	DatabaseIDValues tmp = *this;

	makeLowerCase(tmp.m_strServer);
	makeLowerCase(tmp.m_strName);

	// Expand the check of the times by making sure they are in a 12 hour window
	// https://extract.atlassian.net/browse/ISSUE-15325
	CTimeSpan TwelveHours(0, 12, 0, 0);
	if (tmp.m_strServer == strServer && tmp.m_strName == strDatabaseName
		&& m_ctCreated >= (ctCreationDate - TwelveHours) && m_ctCreated <= (ctCreationDate + TwelveHours)
		&& m_ctRestored >=(ctRestoreDate - TwelveHours) && m_ctRestored <= (ctRestoreDate + TwelveHours))
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
		if (tmp.m_ctCreated != ctCreationDate)
		{
			vecReasons.push_back("Creation date has changed");
		}
		if (tmp.m_ctRestored != ctRestoreDate)
		{
			vecReasons.push_back("Restored date has changed");
		}
		if (vecReasons.size() > 0)
		{
			m_strInvalidReason = asString(vecReasons, false, ", ");
		}
	}

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
	nHashValue ^= hashLongLong((long long)m_ctCreated.GetTime()) << 3;
	nHashValue ^= hashLongLong((long long)m_ctRestored.GetTime()) << 4;
	nHashValue ^= hashLongLong((long long)m_ctLastUpdated.GetTime());
}
//-------------------------------------------------------------------------------------------------
void DatabaseIDValues::addAsDebugInfo(UCLIDException &ue, string strPrefix)
{
	ue.addDebugInfo(strPrefix + "Server", m_strServer);
	ue.addDebugInfo(strPrefix + "DBName", m_strName);
	ue.addDebugInfo(strPrefix + "DBGUID", asString(m_GUID), true);
	ue.addDebugInfo(strPrefix + "Created",(LPCSTR) m_ctCreated.Format(gstrDATE_TIME_FORMAT.c_str()), true);
	ue.addDebugInfo(strPrefix + "Restored",(LPCSTR) m_ctRestored.Format(gstrDATE_TIME_FORMAT.c_str()), true);
	ue.addDebugInfo(strPrefix + "LastUpdated",(LPCSTR) m_ctLastUpdated.Format(gstrDATE_TIME_FORMAT.c_str()), true);
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
