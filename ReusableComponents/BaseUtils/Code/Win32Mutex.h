
#pragma once

#include "BaseUtils.h"

#include <windows.h>

class EXPORT_BaseUtils Win32Mutex
{
public:
    Win32Mutex();
    ~Win32Mutex();
    void acquire();
    bool isAcquired(void);
    void release(void);

private:
	HANDLE m_hMutex;
};

class EXPORT_BaseUtils Win32MutexLockGuard
{
public:
    Win32MutexLockGuard(Win32Mutex& mutex);
    ~Win32MutexLockGuard();

private:
    Win32Mutex& rMutex;
};
