// DBLockGuard.h : Declaration of the DBLockGuard

#pragma once

#include "FAMUtils.h"

#include <afxmt.h>

#include <string>

using namespace std;

// https://extract.atlassian.net/browse/ISSUE-12328
// A class used to synchronize access amongst all threads in a process to the FileProcessingDB's
// lock and unl
class FAMUTILS_API FAMDBSemaphore
{
public:

	// Gets a reference to a boolean indicating whether the current thread already has strLockName.
	static bool IsLocked(const string &strLockName);
	static bool ThisThreadHasLock(const string &strLockName);
	static bool Lock(const string &strLockName, DWORD dwLockTimeout);
	static void Unlock(const string &strLockName);

private:
	static void getSyncObjects(const string &strLockName, CSemaphore *&pSemaphore,
		DWORD *&pdwLockThreadId);

	// Semaphores indicating that this instance has the specified lock on the DB. The semaphores
	// prevent multiple threads from each instance from sharing the lock.
	// NOTE: If other locks are added, be sure to add the map entry in the constructor for the new
	// lock.
	// [FlexIDSCore:5244]
	// Semaphores must be used instead of mutexes since it cannot be guaranteed that Lock will be
	// called on the same thread Unlock even though both calls originate from the same thread.
	static CSemaphore ms_semaphoreMainLock;
	static CSemaphore ms_semaphoreCounterLock;
	static CSemaphore ms_semaphoreWorkItemLock;
	static CSemaphore ms_semaphoreSecureCounterLock;
	static CSemaphore ms_semaphoreCacheLock;

	static DWORD ms_dwMainLockThread;
	static DWORD ms_dwCounterLockThread;
	static DWORD ms_dwWorkItemLockThread;
	static DWORD ms_dwSecureCounterLockThread;
	static DWORD ms_dwCacheLockThread;
};