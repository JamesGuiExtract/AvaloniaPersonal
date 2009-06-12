#pragma once

#include "BaseUtils.h"

class EXPORT_BaseUtils Win32CriticalSection
{
public:
	Win32CriticalSection();
	~Win32CriticalSection();

	// Enter the critical section
	// this method will wait if another thread is already inside the section
	void enter();
	// Leave the critical section
	void leave();

private:
	CRITICAL_SECTION m_cs;
};

class EXPORT_BaseUtils Win32CriticalSectionLockGuard
{
public:
	Win32CriticalSectionLockGuard(Win32CriticalSection& rCS, bool bEnter = true);
	~Win32CriticalSectionLockGuard();

private:
	Win32CriticalSection& m_rCS;
};