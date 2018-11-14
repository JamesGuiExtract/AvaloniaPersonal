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
: m_nID(0)
, m_nValue(0)
, m_nAlertLevel(0)
, m_nAlertMultiple(0)
, m_bUnrecoverable(false)
, m_nChangeLogValue(-1)
{
}
//-------------------------------------------------------------------------------------------------
DBCounter::DBCounter(long nID, string strName, long nValue)
: m_nID(nID)
, m_strName(strName)
, m_nValue(nValue)
, m_nAlertLevel(0)
, m_nAlertMultiple(0)
, m_bUnrecoverable(false)
, m_nChangeLogValue(-1)
{
}
//-------------------------------------------------------------------------------------------------
// DBCounter operators
//-------------------------------------------------------------------------------------------------
void DBCounter::LoadFromFields(FieldsPtr ipFields)
{
	try
	{
		try
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
				m_strValidationError = "Hash ID discrepancy; unrecoverable.";
				UCLIDException ue("ELI38970", "Counter is invalid.");
				ue.addDebugInfo("Expected ID", m_nID, true);
				ue.addDebugInfo("Hashed ID", nHashedID, true);
				throw ue;
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI39156")
	}
	catch (UCLIDException &ue)
	{
		m_bUnrecoverable = true;
		if (m_strValidationError.empty())
		{
			m_strValidationError = ue.getTopText();
		}

		UCLIDException uexOuter("ELI39157", "Failed to read secure counter.", ue);
		uexOuter.addDebugInfo("Name", m_strName, false);
		uexOuter.log();
	}
}
//-------------------------------------------------------------------------------------------------
string DBCounter::getEncrypted(const DatabaseIDValues databaseID)
{
	ByteStream bsPW;
	getFAMPassword(bsPW);
	
	// Add the counter id to the rightmost 10 bits after shifting the DatabaseIDHash
	m_nDatabaseIDCounterIDHash = (databaseID.m_nHashValue << 10) + m_nID;
	
	ByteStream bsCounter;
	ByteStreamManipulator bsm(ByteStreamManipulator::kWrite, bsCounter);
	bsm << m_nValue;
	bsm << m_nDatabaseIDCounterIDHash;
	bsm.flushToByteStream(8);

	return MapLabel::setMapLabelWithS(bsCounter, bsPW);
}
//-------------------------------------------------------------------------------------------------
void DBCounter::validate(const DatabaseIDValues databaseID, FieldsPtr ipFields/* = nullptr */)
{
	if (m_bUnrecoverable)
	{
		if (m_strValidationError.empty())
		{
			m_strValidationError = "Invalid/unrecoverable";
		}

		throw UCLIDException("ELI39158", "Failed to read secure counter.");
	}

	if (ipFields != nullptr)
	{
		DBCounterChangeValue counterChange(databaseID);
		try
		{
			try
			{
				// If ipFields are specified, check the change log before any other checks to
				// populate m_nChangeLogValue.
				counterChange.LoadFromFields(ipFields, true, m_nChangeLogValue);
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

	if (m_nDatabaseIDCounterIDHash != ((databaseID.m_nHashValue << 10) + m_nID))
	{
		m_strValidationError = "Counter hash is invalid";
		throw UCLIDException("ELI38927", "Counter has been corrupted.");
	}

	// Clear any validation errors from previous attempts.
	m_strValidationError = "";
}
//-------------------------------------------------------------------------------------------------
bool DBCounter::isValid(const DatabaseIDValues databaseID, FieldsPtr ipFields/* = nullptr */)
{
	try
	{
		validate(databaseID, ipFields);
		return true;
	}
	catch (UCLIDException ue)
	{
		// Log the exception to help track down what caused the counter to be invalid
		ue.log();

		return false;
	}
}