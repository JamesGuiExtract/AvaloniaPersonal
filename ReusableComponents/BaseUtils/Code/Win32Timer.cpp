// Win32Timer.cpp: implementation of the Win32Timer class.

#include "stdafx.h"
#include "Win32Timer.h"
#include "UCLIDException.h"

#include <Windows.h>

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

EventID Win32Timer::TIMER_EVENT;
std::map<UINT,Win32Timer*> Win32Timer::m_mapTimers;	// collection of Win32Timer objects identified by an idEvent

//-------------------------------------------------------------------------------------------------
Win32Timer::Win32Timer(
	const EventID& rEventID)		// notify observers that this event occurred when the timer expires
	:
	m_eventID(rEventID),
	m_nTimerID(0)
{
}
//-------------------------------------------------------------------------------------------------
Win32Timer::~Win32Timer()
{
	try
	{
		Stop();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16421");
}
//-------------------------------------------------------------------------------------------------
bool Win32Timer::Start(
	int nMilliseconds)	// number of milliseconds before timer expires
{
	ASSERT(nMilliseconds > 0);
	bool bSuccess(true);

	if (m_nTimerID)
	{
		bSuccess = Stop();
	}

	if (bSuccess)
	{
		UINT nTimerID = ::SetTimer(0,0,nMilliseconds,TimerProc);
		if (nTimerID == 0)
		{
			UCLIDException ue("ELI00980", "Unable to create new timer.");
			ue.addWin32ErrorInfo();
			throw ue;
		}
		m_nTimerID = nTimerID;
		m_mapTimers[nTimerID] = this;
	}

	return bSuccess;
}
//-------------------------------------------------------------------------------------------------
bool Win32Timer::Stop()
{
	bool bSuccess(false);

	if (m_nTimerID)
	{
		BOOL bTimerKilled = ::KillTimer(0,m_nTimerID);
		ASSERT_RESOURCE_ALLOCATION("ELI00981", bTimerKilled == TRUE);

		if (!m_mapTimers.empty())
		{
			if (m_mapTimers.find(m_nTimerID) != m_mapTimers.end())
			{
				m_mapTimers.erase(m_nTimerID);
				bSuccess = true;
			}
			else
			{
				THROW_LOGIC_ERROR_EXCEPTION("ELI00978");
			}
		}
		else
		{
			bSuccess = true;
		}

		m_nTimerID = 0;
	}
	else
	{
		bSuccess = true;
	}

	return bSuccess;
}
//-------------------------------------------------------------------------------------------------
// see MFC documentation
void CALLBACK Win32Timer::TimerProc(HWND /*hwnd*/, UINT /*uMsg*/, UINT idEvent, DWORD /*dwTime*/)
{
	Win32Timer* pTimer = m_mapTimers[idEvent];
	pTimer->notifyObservers(ObservableEvent(pTimer->GetEventID()));
}
//-------------------------------------------------------------------------------------------------
