#include "stdafx.h"
#include "Win32Semaphore.h"
#include "UCLIDException.h"

// TODO: remove criticalSection class attribute from hpp file!

Win32Semaphore::Win32Semaphore(unsigned long ulInitialCount, 
							   unsigned long ulMaxCount,
							   const string& strSemaphoreName)
:NamedObject(strSemaphoreName), ulCurrentCount(ulInitialCount), ulMaxCount(ulMaxCount)
{
	const char *pszName = strSemaphoreName == "" ? NULL : strSemaphoreName.c_str();

	hSemaphore = CreateSemaphore(NULL, ulInitialCount, ulMaxCount, pszName);
	ASSERT_RESOURCE_ALLOCATION("ELI00326", hSemaphore != __nullptr);
}

Win32Semaphore::Win32Semaphore(const string& strSemaphoreName)
:NamedObject(strSemaphoreName)
{
	const char *pszName = strSemaphoreName == "" ? NULL : strSemaphoreName.c_str();

	hSemaphore = OpenSemaphore(SEMAPHORE_ALL_ACCESS, TRUE, pszName);
	ASSERT_RESOURCE_ALLOCATION("ELI00938", hSemaphore != __nullptr);
	
	// TODO: when we open a semaphore by name, how do we know its current
	// count and maximum count?
	ulCurrentCount = ulMaxCount = 1;
}

Win32Semaphore::~Win32Semaphore()
{
	try
	{
		if (!CloseHandle(hSemaphore))
		{
			// TODO: not good to throw exception in destructor
			UCLIDException uclidException("ELI00327", "Unable to close handle!");
			addDebugInfoTo(uclidException);

			throw uclidException;
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16419");
}

void Win32Semaphore::acquire(DWORD dwMilliSeconds)
{
	DWORD dwResult = WaitForSingleObject(hSemaphore, dwMilliSeconds);
	if (dwResult == WAIT_TIMEOUT)
	{
		throw AcquireSemaphoreTimedOut(*this);
	}
	else if (dwResult != WAIT_OBJECT_0)
	{
		UCLIDException uclidException("ELI00329", "Unable to acquire semaphore!");
		addDebugInfoTo(uclidException);

		throw uclidException;
	}
	else
	{
		ulCurrentCount--;
	}
}

void Win32Semaphore::release()
{
	if (!ReleaseSemaphore(hSemaphore, 1, NULL))
	{
		UCLIDException uclidException("ELI00330", "Unable to release semaphore!");
		addDebugInfoTo(uclidException);

		throw uclidException;
	}
	else
	{
		ulCurrentCount++;
	}
}

void Win32Semaphore::addDebugInfoTo(UCLIDException& uclidException)
{
	// call base class method first
	NamedObject::addDebugInfoTo(uclidException);
	
	// add debug info from this object
	uclidException.addDebugInfo("ulCurrentCount", ulCurrentCount);
	uclidException.addDebugInfo("ulMaxCount", ulMaxCount);
}

bool Win32Semaphore::isAcquired()
{
	bool bReturnValue = ulCurrentCount != ulMaxCount;

	return bReturnValue;
}

Win32SemaphoreLockGuard::Win32SemaphoreLockGuard(Win32Semaphore& rSemaphore, bool bAcquire, 
												 DWORD dwMilliSeconds)
:rSemaphore(rSemaphore)
{
	if (bAcquire)
		rSemaphore.acquire(dwMilliSeconds);
}

Win32SemaphoreLockGuard::~Win32SemaphoreLockGuard()
{
	try
	{
		rSemaphore.release();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16420");
}

Win32Semaphore::AcquireSemaphoreTimedOut::AcquireSemaphoreTimedOut(	Win32Semaphore& rSemaphore)
:UCLIDException("ELI00947", "Semaphore acquisition timed out!")
{
	addDebugInfo("Semaphore.ObjectName", rSemaphore.getObjectName());
	addDebugInfo("Semaphore.handle", (unsigned long) rSemaphore.hSemaphore);
	addDebugInfo("Semaphore.ulCurrentCount", rSemaphore.ulCurrentCount);
	addDebugInfo("Semaphore.ulMaxCount", rSemaphore.ulMaxCount);
}
