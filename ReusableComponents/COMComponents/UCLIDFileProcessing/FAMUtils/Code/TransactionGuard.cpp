// TransactionGuard.cpp : Implementation of TransactionGuard

#include "stdafx.h"
#include "FAMUtils.h"
#include "TransactionGuard.h"

#include <UCLIDException.h>

//-------------------------------------------------------------------------------------------------
// TransactionGuard
//-------------------------------------------------------------------------------------------------
TransactionGuard::TransactionGuard(ADODB::_ConnectionPtr ipConnection,
	IsolationLevelEnum isolationLevel, CMutex *pMutex)
: m_ipConnection(ipConnection)
, m_pLock(__nullptr)
{
	ASSERT_ARGUMENT("ELI14624", ipConnection != __nullptr );

	if (pMutex != __nullptr)
	{
		m_pLock = new CSingleLock(pMutex, TRUE);
	}

	ipConnection->IsolationLevel = isolationLevel;
	
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
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI14980");

	try
	{
		// Restore the connection isolation level to the ADO default of adXactChaos.
		m_ipConnection->IsolationLevel = adXactChaos;
		
		m_ipConnection = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI35397");

	try
	{
		if (m_pLock != __nullptr)
		{
			delete m_pLock;
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI35398");
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

		if (m_pLock != __nullptr)
		{
			delete m_pLock;
			m_pLock = __nullptr;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27607")
}
//-------------------------------------------------------------------------------------------------

