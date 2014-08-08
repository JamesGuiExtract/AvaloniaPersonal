// DBLockGuard.h : Declaration of the DBLockGuard

#pragma once
#include "FAMDBSemaphore.h"

#include <UCLIDException.h>


// Template for use with a database smartpointer that has a LockDB() and UnlockDB() method
template<class T> class LockGuard 
{
public:
	// PROMISE: To lock the database. This will call ipDB->LockDB()
	// NOTE: If LockGuard is used while the database is already locked, the instance will not lock
	// the database or unlock it when destroyed (and will not throw any errors).
	LockGuard(T ipDB, const string& strLockName)
		: m_strLockName(strLockName)
		, m_bLockedByThisInstance(false)
	{
		ASSERT_ARGUMENT("ELI19755", ipDB != __nullptr );
		m_ipDB = ipDB;

		// https://extract.atlassian.net/browse/ISSUE-12328
		// Don't attempt to lock the DB if a higher scope has already locked the DB on this thread.
		if (!FAMDBSemaphore::ThisThreadHasLock(strLockName))
		{
			// lock the database
			m_ipDB->LockDB(m_strLockName.c_str());
			m_bLockedByThisInstance = true;
		}
		else
		{
			m_bLockedByThisInstance = false;
		}
	}

	// PROMISE: To unlock the database. This will call m_ipDB->unlockDB() and unlock the m_ipDB->m_mutex
	//			The mutex will be unlocked even if m_ipDB->unlockDB() throws and exception.
	~LockGuard()
	{
		// Need to catch any exceptions and log them because this could be called within a catch
		// and don't want to throw an exception from a catch
		try
		{
			// Unlock the DB only if this instance locked it.
			if (m_bLockedByThisInstance)
			{
				m_ipDB->UnlockDB(m_strLockName.c_str());
			}
		}
		CATCH_AND_LOG_ALL_EXCEPTIONS("ELI19754");
	}

private:
	T m_ipDB;	
	string m_strLockName;
	bool m_bLockedByThisInstance;
};