#include "stdafx.h"
#include "Win32CriticalSection.h"
#include "UCLIDException.h"

//--------------------------------------------------------------------------------------------------
// Win32CriticalSection
//--------------------------------------------------------------------------------------------------
Win32CriticalSection::Win32CriticalSection()
{
	InitializeCriticalSection(&m_cs);
}
//--------------------------------------------------------------------------------------------------
Win32CriticalSection::~Win32CriticalSection()
{
	try
	{
		DeleteCriticalSection(&m_cs);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16427");
}
//--------------------------------------------------------------------------------------------------
void Win32CriticalSection::enter()
{
	EnterCriticalSection(&m_cs);
}
//--------------------------------------------------------------------------------------------------
void Win32CriticalSection::leave()
{
	LeaveCriticalSection(&m_cs);
}
//--------------------------------------------------------------------------------------------------
// Win32CriticalSectionLockGuard
//--------------------------------------------------------------------------------------------------
Win32CriticalSectionLockGuard::Win32CriticalSectionLockGuard(Win32CriticalSection& rCS, bool bEnter)
: m_rCS(rCS)
{
	if(bEnter)
	{
		m_rCS.enter();
	}
}
//--------------------------------------------------------------------------------------------------
Win32CriticalSectionLockGuard::~Win32CriticalSectionLockGuard()
{
	try
	{
		m_rCS.leave();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16428");
}