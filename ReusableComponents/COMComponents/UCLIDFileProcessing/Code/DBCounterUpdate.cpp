#include "stdafx.h"
#include "DBCounterUpdate.h"

#include <ADOUtils.h>
#include <UCLIDException.h>
#include <DateUtil.h>

#include <EncryptionEngine.h>

//-------------------------------------------------------------------------------------------------
// CounterOperation 
//-------------------------------------------------------------------------------------------------
CounterOperation::CounterOperation()
	: m_eOperation(kNone)
{
}
//-------------------------------------------------------------------------------------------------
CounterOperation::CounterOperation(const DBCounter &dbCounter)
{
	m_eOperation = kNone;
	m_nCounterID = dbCounter.m_nID;
	m_strCounterName = dbCounter.m_strName;
	m_nValue = dbCounter.m_nValue;
}
//-------------------------------------------------------------------------------------------------
string CounterOperation::GetSQLQuery(const DatabaseIDValues &databaseIDValues)
{
	string strQuery;
	
	DBCounter dbCounter(m_nCounterID, m_strCounterName, m_nValue);

	// Get the new encrypted counter value
	string encryptedCounterValue = dbCounter.getEncrypted(databaseIDValues);

	// Build the Where clause to select the row to update
	string strWhere = " WHERE ID = " + asString(m_nCounterID);
	switch (m_eOperation)
	{
	case kNone:
	case kSet:
		// Only update the SecureCounterValue
		strQuery = "UPDATE [dbo].[SecureCounter] SET SecureCounterValue = '" 
			+ encryptedCounterValue + "'" + strWhere;
		break;
	case kCreate:
		// Insert a new counter record
		strQuery = "INSERT INTO [dbo].[SecureCounter] (ID, CounterName, SecureCounterValue) " 
			" VALUES (" + asString(m_nCounterID) + ", '" + 
			m_strCounterName + "', '" +  encryptedCounterValue + "')";
		break;
	case kDelete:
		// Delete the counter record
		strQuery = "DELETE FROM [dbo].[SecureCounter] " + strWhere;
		break;
	default:
		THROW_LOGIC_ERROR_EXCEPTION("ELI38914");
	}
	return strQuery;
}

//-------------------------------------------------------------------------------------------------
// CounterOperation operators
//-------------------------------------------------------------------------------------------------
ByteStreamManipulator& operator >> (ByteStreamManipulator & bsm, CounterOperation &counterOperation)
{
	// Get the CounterID from stream
	bsm >> counterOperation.m_nCounterID;

	// if the counter >= 100 this is a custom counter and so need to get the name
	if (counterOperation.m_nCounterID >= 100)
	{
		// Get the counter name from the stream
		bsm >> counterOperation.m_strCounterName;
	}
	else
	{
		// Set the CounterName to empty string since it is a 
		counterOperation.m_strCounterName = "";
	}
	long nTmp;
	bsm >> nTmp;
	counterOperation.m_eOperation = (ECounterUpdateOperations) nTmp;
	if (counterOperation.m_eOperation != kDelete)
	{
		bsm >> counterOperation.m_nValue;
	}
	else
	{
		counterOperation.m_nValue = 0;
	}
	return bsm;
}

//-------------------------------------------------------------------------------------------------
// DBCounterUpdate operators
//-------------------------------------------------------------------------------------------------
ByteStreamManipulator& operator >> (ByteStreamManipulator & bsm, DBCounterUpdate &counterUpdate)
{
	bsm >> counterUpdate.m_DatabaseID;
	bsm >> counterUpdate.m_stTimeCodeGenerated;
	bsm >> counterUpdate.m_strUserName;
	bsm >> counterUpdate.m_strMachineName;
	bsm >> counterUpdate.m_nNumberOfUpdates;

	counterUpdate.m_vecOperations.clear();
	if (counterUpdate.m_nNumberOfUpdates >= 0)
	{
		for (int i=0; i < counterUpdate.m_nNumberOfUpdates; i++)
		{
			CounterOperation co;
			bsm >> co;
			counterUpdate.m_vecOperations.push_back(co);
		}
	}
	else
	{
		// This is an unlock code
		long nNumCounters = abs(counterUpdate.m_nNumberOfUpdates);
		for (int i=0; i < nNumCounters; i++)
		{
			CounterOperation co;
			bsm >> co.m_nCounterID;
			bsm >> co.m_nValue;
			co.m_eOperation = kNone;
			counterUpdate.m_vecOperations.push_back(co);
		}
	}

	return bsm;
}
//-------------------------------------------------------------------------------------------------
