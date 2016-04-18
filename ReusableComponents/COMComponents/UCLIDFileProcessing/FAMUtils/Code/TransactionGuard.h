// TransactionGuard.h  : Declaration of the TransactionGuard

#pragma once

#include "FAMUtils.h"

#include <afxmt.h>
#include <memory>
#include <map>

using namespace ADODB;

class FAMUTILS_API TransactionGuard
{
public:
	// PROMISE: To begin a transaction on the given DB connection
	//			The connection pointer passed must be for an open connection
	// ARGS:	ipConnection- The database connection;
	//			isolationLevel- The isolation level to use for the transaction. adXactChaos is the
	//			ADO default.
	//			pMutex- A mutex that should be locked for the duration of the transaction (if non-null)
	//			[LegacyRCAndUtils:6350]
	//			Use at least adXactRepeatableRead for any calls that may involve changing file status,
	//			or any operations that involve updating multiple tables related to the same record. It
	//			is important that the status of the related table rows don't change via other threads/
	//			processes in the midst of such calls, otherwise the records may end up in an
	//			inconsistent state. For example, a file could end up complete in the FileActionStatus
	//			table, but still be in the LockedFile table. adXactRepeatableRead will ensure the
	//			relevant records do not change via other threads/processes before this operation
	//			is complete.
	//			NOTE: Take care when using elevated isolation levels, however, as doing so can create
	//			deadlock situations.
	//			[FlexIDSCore:5244], [DataEntry:1212]
	//			Specify pMutex for any transactions that are repeatable read or isolated when that
	//			mutex that has the potential to be locked during the transaction. Otherwise, if the
	//			transaction is started, then another thread locks the mutex before the one in the
	//			transaction, it can lead to a deadlock as the database causes the other thread to
	//			wait on the active repeatable read transaction.
	TransactionGuard(ADODB::_ConnectionPtr ipConnection, IsolationLevelEnum isolationLevel,
		CMutex *pMutex);

	// PROMISE: To Rollback a started transaction if it has not been committed
	~TransactionGuard();

	// PROMISE: To commit currently open transaction
	void CommitTrans();
private:
	// Variables

	// Indicates if this instance is currently nested within another TransactionGuard for the same
	// connection. In this case a DB transaction (which would fail) will not be initiated.
	bool m_bNestedTransaction;
	
	// Flag that is true while a transaction has been started
	// this will be set to false if Commit has been called
	bool m_bTransactionStarted;

	// Mutex that, if non-null, should be locked for the duration of the transaction.
	
	// Use a unique_ptr for the CSingleLock object that is created so that 
	// if an exception is generated the lock gets released
	// https://extract.atlassian.net/browse/ISSUE-12694
	std::unique_ptr <CSingleLock> m_upLock;

	// Keeps track of any existing TransactionGuards for each DB connection.
	static std::map<_ConnectionPtr, DWORD> m_mapExistingTransactions;

	// Connection Pointer
	ADODB::_ConnectionPtr m_ipConnection;
};
