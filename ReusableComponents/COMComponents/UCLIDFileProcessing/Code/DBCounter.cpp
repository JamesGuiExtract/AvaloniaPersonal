#include "stdafx.h"
#include "DBCounter.h"
#include "DBCounterChangeValue.h"


#include <UCLIDException.h>
#include <EncryptionEngine.h>
#include <MapInitializationTemplate.h>


// Defined in FileProcessingDB_Internal.cpp
void getFAMPassword(ByteStream &bsPW);

//-------------------------------------------------------------------------------------------------
// DBCounter Static variables
//-------------------------------------------------------------------------------------------------
map<long, string> DBCounter::ms_mapOfStandardNames = create_map<long, string>
	(1, "FLEX Index - Indexing (By Document)")
	(2, "FLEX Index - Pagination (By Document)")
	(3, "ID Shield - Redaction (By Page)")
	(4, "ID Shield - Redaction (By Document)")
	(5, "FLEX Index - Indexing (By Page)");

//-------------------------------------------------------------------------------------------------
// DBCounter
//-------------------------------------------------------------------------------------------------
DBCounter::DBCounter()
{
}
//-------------------------------------------------------------------------------------------------
DBCounter::DBCounter(long nID, string strName, long nValue)
	: m_nID(nID), m_strName(strName), m_nValue(nValue), m_nAlertLevel(0), m_nAlertMultiple(0)
{
}
//-------------------------------------------------------------------------------------------------
// DBCounter operators
//-------------------------------------------------------------------------------------------------
void DBCounter::LoadFromFields(FieldsPtr ipFields)
{
	m_nID = getLongField(ipFields, "ID");
	m_strName = getStringField(ipFields, "CounterName");
	m_nAlertLevel = getLongField(ipFields, "AlertLevel");
	m_nAlertMultiple = getLongField(ipFields, "AlertMultiple");
	m_strValidationError = "";
	
	string encryptedCounter = getStringField(ipFields, "SecureCounterValue");
	ByteStream bsCounterPW;
	getFAMPassword(bsCounterPW); 

	ByteStream bsDecrypted = MapLabel::getMapLabelWithS(encryptedCounter, bsCounterPW);
	ByteStreamManipulator bsm(ByteStreamManipulator::kRead, bsDecrypted);

	bsm >> m_nValue;
	bsm >> m_nDatabaseIDCounterIDHash;

	// The counterID is the lower 10 bits or 0x3FF
	long nHashedID = m_nDatabaseIDCounterIDHash & 0x3FF;

	// Check the hashed counter id against the expected id
	if (nHashedID != m_nID)
	{
		UCLIDException ue("ELI38970", "Counter is invalid.");
		ue.addDebugInfo("Expected ID", m_nID, true);
		ue.addDebugInfo("Hashed ID", nHashedID, true);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
string DBCounter::getEncrypted(const long nDatabaseIDHash)
{
	ByteStream bsPW;
	getFAMPassword(bsPW);
	
	// Add the counter id to the rightmost 10 bits after shifting the DatabaseIDHash
	m_nDatabaseIDCounterIDHash = (nDatabaseIDHash << 10) + m_nID;
	
	ByteStream bsCounter;
	ByteStreamManipulator bsm(ByteStreamManipulator::kWrite, bsCounter);
	bsm << m_nValue;
	bsm << m_nDatabaseIDCounterIDHash;
	bsm.flushToByteStream(8);

	return MapLabel::setMapLabelWithS(bsCounter, bsPW);
}
//-------------------------------------------------------------------------------------------------
void DBCounter::validate(const long nDatabaseIDHash, FieldsPtr ipFields/* = nullptr */)
{
	long nHashedID = m_nDatabaseIDCounterIDHash & 0x3FF;

	if (m_nDatabaseIDCounterIDHash != ((nDatabaseIDHash << 10) + m_nID))
	{
		m_strValidationError = "Counter hash is invalid";
		throw UCLIDException("ELI38927", "Counter has been corrupted.");
	}

	if (ipFields != nullptr)
	{
		DBCounterChangeValue counterChange;
		try
		{
			try
			{
				counterChange.LoadFromFields(ipFields, true);
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI39102");
		}
		catch (UCLIDException &ue)
		{
			m_strValidationError = "Counter history corrupted";
			throw ue;
		}

		if (m_nValue != counterChange.m_nToValue)
		{
			m_strValidationError = "Counter history discrepancy";
			UCLIDException ue("ELI38930", "Counter has been corrupted.");
			ue.addDebugInfo("CounterID", m_nID);
			ue.addDebugInfo("CounterName", m_strName);
			throw ue;
		}
	}

	// Clear any validation errors from previous attempts.
	m_strValidationError = "";
}
//-------------------------------------------------------------------------------------------------
bool DBCounter::isValid(const long nDatabaseIDHash, FieldsPtr ipFields/* = nullptr */)
{
	try
	{
		validate(nDatabaseIDHash, ipFields);
		return true;
	}
	catch (...)
	{
		return false;
	}
}