#pragma once

#include "DatabaseIDValues.h"
#include "DBCounter.h"

#include <ByteStreamManipulator.h>

#include <set>

using namespace ADODB;

// enum for operations
enum ECounterUpdateOperations {
	kNone = 0,
    kCreate = 1,
    kSet = 2,
    kIncrement = 3,
    kDecrement = 4,
    kDelete = 5
};

// Class for specifying an update code operation
class CounterOperation
{
public:
	CounterOperation();
	CounterOperation(const DBCounter &dbCounter);
	long m_nCounterID;
	string m_strCounterName;
	ECounterUpdateOperations m_eOperation;
	long m_nValue;

	// Gets the sql query required to preform the operation
	string GetSQLQuery(const DatabaseIDValues &databaseIDValues);
};

// Methods to stream CounterOperation class to and from a ByteStream
ByteStreamManipulator& operator >> (ByteStreamManipulator & bsm, CounterOperation &counterOperation);

// Class to contain the values that are in an counter update code
class DBCounterUpdate
{
public:
	DatabaseIDValues m_DatabaseID;
	SYSTEMTIME m_stTimeCodeGenerated;
	string m_strUserName;
	string m_strMachineName;
	long m_nNumberOfUpdates;
	vector<CounterOperation> m_vecOperations;
};

// Methods to stream the DBCounterUpdate to and from a ByteStream
ByteStreamManipulator& operator >> (ByteStreamManipulator & bsm, DBCounterUpdate &counterUpdate);
