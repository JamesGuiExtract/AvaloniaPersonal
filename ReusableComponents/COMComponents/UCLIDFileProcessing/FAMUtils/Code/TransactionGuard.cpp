// TransactionGuard.cpp : Implementation of TransactionGuard

#include "stdafx.h"
#include "FAMUtils.h"
#include "TransactionGuard.h"

#include <UCLIDException.h>

//-------------------------------------------------------------------------------------------------
// TransactionGuard
//-------------------------------------------------------------------------------------------------
TransactionGuard::TransactionGuard(ADODB::_ConnectionPtr ipConnection)
: m_ipConnection(ipConnection)
{
	ASSERT_ARGUMENT("ELI14624", ipConnection != __nullptr );
	
	// Start a transaction
	ipConnection->BeginTrans();
	m_bTransactionStarted = true;
}
//-------------------------------------------------------------------------------------------------
TransactionGuard::~TransactionGuard()
{
	// Need to catch any exceptions and log them because this could be called within a catch
	// and don't want to throw an exception from a catch
	try
	{
		// If a transaction is open roll it back
		if ( m_bTransactionStarted )
		{
			// Rollback the open transaction
			m_ipConnection->RollbackTrans();
		}

		m_ipConnection = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI14980");
}
//-------------------------------------------------------------------------------------------------
void TransactionGuard::CommitTrans()
{
	try
	{
		// Commit open transaction
		m_ipConnection->CommitTrans();
		
		// There is no longer a transaction in progress-- so reset the started flag
		m_bTransactionStarted = false;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27607")
}
//-------------------------------------------------------------------------------------------------

