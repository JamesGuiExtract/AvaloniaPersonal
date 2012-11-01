// TransactionGuard.h  : Declaration of the TransactionGuard

#pragma once

#include "FAMUtils.h"

using namespace ADODB;

class FAMUTILS_API TransactionGuard
{
public:
	// PROMISE: To begin a transaction on the given DB connection
	//			The connection pointer passed must be for an open connection
	// ARGS:	ipConnection- The database connection;
	//			isolationLevel- The isolation level to use for the transaction. adXactChaos is the
	//			ADO default.
	//			[LegacyRCAndUtils:6350]
	//			Use at least adXactRepeatableRead for any calls that may involve changing file status,
	//			or any operations that involve updating multiple tables related to the same record. It
	//			is important that the status of the related table rows don't change via other threads/
	//			processes in the midst of such calls, otherwise the records may end up in an
	//			inconsisent state. For example, a file could end up complete in the FileActionStatus
	//			table, but still be in the LockedFile table. adXactRepeatableRead will ensure the
	//			relevant records do not change beforevia other threads/processes before this operation
	//			is complete.
	//			NOTE: Take care when using elevated isolation levels, however, as doing so can create
	//			deadlock situations.
	TransactionGuard(ADODB::_ConnectionPtr ipConnection, IsolationLevelEnum isolationLevel);

	// PROMISE: To Rollback a started transaction if it has not been commited
	~TransactionGuard();

	// PROMISE: To commit currently open transaction
	void CommitTrans();
private:
	// Variables
	
	// Flag that is true while a transaction has been started
	// this will be set to false if Commit has been called
	bool m_bTransactionStarted;

	// Connection Pointer
	ADODB::_ConnectionPtr m_ipConnection;
};
