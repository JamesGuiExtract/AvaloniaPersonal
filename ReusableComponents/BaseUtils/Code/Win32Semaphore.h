#ifndef WIN32_SEMAPHORE_HPP
#define WIN32_SEMAPHORE_HPP

#include "BaseUtils.h"
#include "NamedObject.h"
#include "UCLIDException.h"

#include <string>
using namespace std;

class EXPORT_BaseUtils Win32Semaphore : public NamedObject
{
public:
	// An object of the following class is thrown when a timed-acquire call
	// fails
	class AcquireSemaphoreTimedOut : public UCLIDException
	{
	public:
		AcquireSemaphoreTimedOut(Win32Semaphore& rSemaphore);
	};

	// constructors & destructor
	Win32Semaphore(unsigned long ulInitialCount = 1, unsigned long ulMaxCount = 1, const string& strSemaphoreName = "");
	Win32Semaphore(const string& strSemaphoreName);
	~Win32Semaphore();

	// public methods
	void acquire(DWORD dwMilliSeconds = INFINITE);
	void release();
	bool isAcquired();
	virtual void addDebugInfoTo(UCLIDException& uclidException);

	// friend classes
	EXPORT_BaseUtils friend class AcquireSemaphoreTimedOut;

private:
	HANDLE hSemaphore;
	unsigned long ulCurrentCount, ulMaxCount;
};

class EXPORT_BaseUtils Win32SemaphoreLockGuard
{
public:
	Win32SemaphoreLockGuard(Win32Semaphore& rSemaphore, bool bAcquire = true, DWORD dwMilliSeconds = INFINITE);
	~Win32SemaphoreLockGuard();

private:
	Win32Semaphore& rSemaphore;
};

#endif // WIN32_SEMAPHORE_HPP