// TransactionGuard.h  : Declaration of the TransactionGuard

#pragma once

#include "FAMUtils.h"

class FAMUTILS_API TransactionGuard
{
public:
	// PROMISE: To begin a transaction on the given DB connection
	//			The connection pointer passed must be for an open connection
	TransactionGuard(ADODB::_ConnectionPtr ipConnection);

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
