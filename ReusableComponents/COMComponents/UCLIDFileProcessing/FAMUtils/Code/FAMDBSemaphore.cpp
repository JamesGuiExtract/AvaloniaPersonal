#include "stdafx.h"
#include "FAMDBSemaphore.h"
#include "ADOUtils.h"
#include "FAMUtilsConstants.h"

#include <UCLIDException.h>

#include <afxmt.h>

DWORD FAMDBSemaphore::ms_dwMainLockThread = 0;
DWORD FAMDBSemaphore::ms_dwCounterLockThread = 0;
DWORD FAMDBSemaphore::ms_dwWorkItemLockThread = 0;
DWORD FAMDBSemaphore::ms_dwSecureCounterLockThread = 0;
DWORD FAMDBSemaphore::ms_dwCacheLockThread = 0;

CSemaphore FAMDBSemaphore::ms_semaphoreMainLock(1);
CSemaphore FAMDBSemaphore::ms_semaphoreCounterLock(1);
CSemaphore FAMDBSemaphore::ms_semaphoreWorkItemLock(1);
CSemaphore FAMDBSemaphore::ms_semaphoreSecureCounterLock(1);
CSemaphore FAMDBSemaphore::ms_semaphoreCacheLock(1);

//--------------------------------------------------------------------------------------------------
// FAMDBSemaphore
//--------------------------------------------------------------------------------------------------
bool FAMDBSemaphore::IsLocked(const string &strLockName)
{
	CSemaphore *pSemaphore = __nullptr;
	DWORD *pdwLockThreadId = __nullptr;
	getSyncObjects(strLockName, pSemaphore, pdwLockThreadId);

	return (*pdwLockThreadId != 0);
}
//--------------------------------------------------------------------------------------------------
bool FAMDBSemaphore::ThisThreadHasLock(const string &strLockName)
{
	CSemaphore *pSemaphore = __nullptr;
	DWORD *pdwLockThreadId = __nullptr;
	getSyncObjects(strLockName, pSemaphore, pdwLockThreadId);

	return (*pdwLockThreadId == GetCurrentThreadId());
}
//--------------------------------------------------------------------------------------------------
bool FAMDBSemaphore::Lock(const string &strLockName, DWORD dwLockTimeout)
{
	CSemaphore *pSemaphore = __nullptr;
	DWORD *pdwLockThreadId = __nullptr;
	getSyncObjects(strLockName, pSemaphore, pdwLockThreadId);

	if (!pSemaphore->Lock(dwLockTimeout))
	{
		return false;
	}

	if (*pdwLockThreadId != 0)
	{
		// It is an unexpected condition to have gotten the semaphore lock while the pdwLockThreadId
		// is set. Reset the lock before throwing and exception so that the next call can succeed.
		pSemaphore->Unlock();
		*pdwLockThreadId = 0;

		THROW_LOGIC_ERROR_EXCEPTION("ELI37215");
	}

	*pdwLockThreadId = GetCurrentThreadId();

	return true;
}
//--------------------------------------------------------------------------------------------------
void FAMDBSemaphore::Unlock(const string &strLockName)
{
	CSemaphore *pSemaphore = __nullptr;
	DWORD *pdwLockThreadId = __nullptr;
	getSyncObjects(strLockName, pSemaphore, pdwLockThreadId);

	*pdwLockThreadId = 0;
	pSemaphore->Unlock();
}
//--------------------------------------------------------------------------------------------------
void FAMDBSemaphore::getSyncObjects(const string &strLockName, CSemaphore *&pSemaphore, 
								DWORD *&pdwLockThreadId)
{
	if (strLockName == gstrMAIN_DB_LOCK)
	{
		pSemaphore = &ms_semaphoreMainLock;
		pdwLockThreadId = &ms_dwMainLockThread;
	}
	else if (strLockName == gstrUSER_COUNTER_DB_LOCK)
	{
		pSemaphore = &ms_semaphoreCounterLock;
		pdwLockThreadId = &ms_dwCounterLockThread;
	}
	else if (strLockName == gstrWORKITEM_DB_LOCK)
	{
		pSemaphore = &ms_semaphoreWorkItemLock;
		pdwLockThreadId = &ms_dwWorkItemLockThread;
	}
	else if (strLockName == gstrSECURE_COUNTER_DB_LOCK)
	{
		pSemaphore = &ms_semaphoreSecureCounterLock;
		pdwLockThreadId = &ms_dwSecureCounterLockThread;
	}
	else if (strLockName == gstrCACHE_LOCK)
	{
		pSemaphore = &ms_semaphoreCacheLock;
		pdwLockThreadId = &ms_dwCacheLockThread;
	}
	else 
	{
		THROW_LOGIC_ERROR_EXCEPTION("ELI37216");
	}
}

