// TransactionGuard.cpp : Implementation of TransactionGuard

#include "stdafx.h"
#include "FAMUtils.h"
#include "TransactionGuard.h"

#include <UCLIDException.h>

std::map<_ConnectionPtr, DWORD> TransactionGuard::m_mapExistingTransactions;

CCriticalSection TransactionGuard::m_sCSExistingTrans;

//-------------------------------------------------------------------------------------------------
// TransactionGuard
//-------------------------------------------------------------------------------------------------
TransactionGuard::TransactionGuard(ADODB::_ConnectionPtr ipConnection,
	IsolationLevelEnum isolationLevel, CCriticalSection *pCriticalSection)
: m_ipConnection(ipConnection)
, m_bTransactionStarted(false)
, m_bNestedTransaction(false)
, m_upLock(__nullptr)
{
	ASSERT_ARGUMENT("ELI14624", ipConnection != __nullptr );

	try
	{
		DWORD dwThreadId = GetCurrentThreadId();

		// Scope the critical section lock for using the m_mapExistingTransaction
		{
			CSingleLock csLock(&m_sCSExistingTrans, TRUE);

			if (m_mapExistingTransactions.find(ipConnection) != m_mapExistingTransactions.end())
			{
				if (dwThreadId != m_mapExistingTransactions[ipConnection])
				{
					THROW_LOGIC_ERROR_EXCEPTION("ELI39572");
				}
				else
				{
					// There is already a transaction open on this connection; trying to start another
					// would fail.
					m_bNestedTransaction = true;
					return;
				}
			}
		}

		if (pCriticalSection != __nullptr)
		{
			m_upLock.reset(new CSingleLock(pCriticalSection, TRUE));
		}

		ipConnection->IsolationLevel = isolationLevel;

		// Start a transaction
		ipConnection->BeginTrans();
		m_bTransactionStarted = true;

		// Scope the critical section lock for using the m_mapExistingTransaction
		{
			CSingleLock csLock(&m_sCSExistingTrans, TRUE);

			m_mapExistingTransactions[ipConnection] = dwThreadId;

		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38458");
}
//-------------------------------------------------------------------------------------------------
TransactionGuard::~TransactionGuard()
{
	// If this was a nested transaction, no action was performed that needs to be undone.
	if (m_bNestedTransaction)
	{
		return;
	}
	try
	{
		CSingleLock csLock(&m_sCSExistingTrans, TRUE);

		m_mapExistingTransactions.erase(m_ipConnection);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI40190");

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
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI14980");

	try
	{
		// Restore the connection isolation level to the ADO default of adXactReadCommitted.
		m_ipConnection->IsolationLevel = adXactReadCommitted;
		
		m_ipConnection = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI35397");

	try
	{
		m_upLock.reset(__nullptr);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI35398");
}
//-------------------------------------------------------------------------------------------------
void TransactionGuard::CommitTrans()
{
	try
	{
		if (!m_bNestedTransaction)
		{
			// Commit open transaction
			m_ipConnection->CommitTrans();
		
			// There is no longer a transaction in progress-- so reset the started flag
			m_bTransactionStarted = false;

			m_upLock.reset(__nullptr);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27607")
}
//-------------------------------------------------------------------------------------------------

