#include "stdafx.h"
#include "Win32Mutex.h"
#include "UCLIDException.h"

#include <string>
using namespace std;

//-------------------------------------------------------------------------------------------------
// Win32Mutex class
//-------------------------------------------------------------------------------------------------
Win32Mutex::Win32Mutex()
: m_hMutex(NULL)
{
	// create the mutex object
	m_hMutex = CreateMutex(NULL, FALSE, NULL);
	if (m_hMutex == NULL)
	{
		throw UCLIDException("ELI13010", "Unable to create mutex!");
	}
}
//-------------------------------------------------------------------------------------------------
Win32Mutex::~Win32Mutex()
{
	try
	{
		// release the mutex object if it has been created
		if (m_hMutex)
		{
			ReleaseMutex(m_hMutex);

			// The handle must be closed or there will be a handle leak. P13 #3886
			CloseHandle(m_hMutex);
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16429");
}
//-------------------------------------------------------------------------------------------------
void Win32Mutex::acquire(void)
{
	// acquire ownership of the mutex
	DWORD dwResult = WaitForSingleObject(m_hMutex, INFINITE);
	if (dwResult != WAIT_OBJECT_0)
	{
		UCLIDException ue("ELI13011", "Unable to acquire mutex!");
		ue.addDebugInfo("Result", dwResult);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void Win32Mutex::release(void)
{
	// release ownership of the mutex
	if (ReleaseMutex(m_hMutex) == 0)
	{
		throw UCLIDException("ELI13012", "Unable to release mutex!");
	}
}
//-------------------------------------------------------------------------------------------------
bool Win32Mutex::isAcquired(void)
{
	// check if the mutex is acquired
	return WaitForSingleObject(m_hMutex, 0) == WAIT_OBJECT_0;
}

//-------------------------------------------------------------------------------------------------
// Win32MutexLockGuard class
//-------------------------------------------------------------------------------------------------
Win32MutexLockGuard::Win32MutexLockGuard(Win32Mutex& mutex)
:rMutex(mutex)
{
	rMutex.acquire();
}
//-------------------------------------------------------------------------------------------------
Win32MutexLockGuard::~Win32MutexLockGuard()
{
	try
	{
		rMutex.release();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16418");
}
//-------------------------------------------------------------------------------------------------
