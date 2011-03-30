// DBLockGuard.h : Declaration of the DBLockGuard

#pragma once
#include <UCLIDException.h>

// Template for use with a database smartpointer that has a LockDB() and UnlockDB() method
template<class T> class LockGuard 
{
public:
	// PROMISE: To lock the database. This will call ipDB->LockDB() 
	LockGuard(T ipDB)
	{
		ASSERT_ARGUMENT("ELI19755", ipDB != __nullptr );
		m_ipDB = ipDB;

		// lock the database
		m_ipDB->LockDB();
	}

	// PROMISE: To unlock the database. This will call m_ipDB->unlockDB() and unlock the m_ipDB->m_mutex
	//			The mutex will be unlocked even if m_ipDB->unlockDB() throws and exception.
	~LockGuard()
	{
		// Need to catch any exceptions and log them because this could be called within a catch
		// and don't want to throw an exception from a catch
		try
		{
			// Unlock the DB
			m_ipDB->UnlockDB();
		}
		CATCH_AND_LOG_ALL_EXCEPTIONS("ELI19754");
	}

private:
	T m_ipDB;	
};
