
#include "stdafx.h"
#include "StopWatch.h"
#include "UCLIDException.h"
#include "DateUtil.h"

// global / static member variables
bool StopWatch::ms_bHighResSupported = false;
LARGE_INTEGER StopWatch::ms_highResCounterFreq;

//-------------------------------------------------------------------------------------------------
StopWatch::StopWatch()
:
m_bIsRunning(false),
m_fElapsedTime(0),
m_bIsReset(true)
{
	// initialize static variables if not already done so
	static bool bStaticMembersInitialized = false;
	if (!bStaticMembersInitialized)
	{
		// get the highres-counter frequency
		if (QueryPerformanceFrequency(&ms_highResCounterFreq) != 0)
		{
			ms_bHighResSupported = true;
		}

		bStaticMembersInitialized = true;
	}

	// reset the stop watch
	reset();
}
//-------------------------------------------------------------------------------------------------
StopWatch::StopWatch(const StopWatch& watch)
{
	// invoke the assignment operator
	*this = watch;
}
//-------------------------------------------------------------------------------------------------
StopWatch& StopWatch::operator=(const StopWatch& watch)
{
	// copy the member variables
	m_startTime = watch.m_startTime;
	m_endTime = watch.m_endTime;
	m_startCounter = watch.m_startCounter;
	m_endCounter = watch.m_endCounter;
	m_bIsRunning = watch.m_bIsRunning;
	m_fElapsedTime = watch.m_fElapsedTime;
	m_bIsReset = watch.m_bIsReset;
	m_beginTime = watch.m_beginTime;

	return *this;
}
//-------------------------------------------------------------------------------------------------
void StopWatch::reset()
{
	// get the current time
	//getCurrentTime(m_startTime, m_startCounter);
	m_startCounter.QuadPart = 0;
	m_endCounter.QuadPart = 0;
	ZeroMemory(&m_endTime, sizeof(m_endTime));
	ZeroMemory(&m_startTime, sizeof(m_endTime));
	m_fElapsedTime = 0;
	m_bIsReset = true;
	m_bIsRunning = false;
}
//-------------------------------------------------------------------------------------------------
void StopWatch::start()
{
	if(!m_bIsRunning || m_bIsReset)
	{
		// get the current time
		getCurrentTime(m_startTime, m_startCounter);

		m_bIsRunning = true;

		if(m_bIsReset)
		{
			m_beginTime = m_startTime;
			m_bIsReset = false;
		}
	}
}
//-------------------------------------------------------------------------------------------------
double StopWatch::getElapsedTime() const
{
	if (m_bIsRunning && !m_bIsReset)
	{
		SYSTEMTIME endTime;
		LARGE_INTEGER endCounter;
		
		getCurrentTime(endTime, endCounter);

		// return the elapsed time in the highest resolution possible
		if (!ms_bHighResSupported)
		{
			// return the low-res elapsed time
			return m_fElapsedTime + getLowResElapsedTime(endTime);
		}
		else
		{
			// return the high-res elapsed time if possible
			return m_fElapsedTime + getHighResElapsedTime(endCounter);
		}
	}
	else
	{
		return m_fElapsedTime;
	}
}
//-------------------------------------------------------------------------------------------------
void StopWatch::stop()
{
	if(m_bIsRunning && !m_bIsReset)
	{
		// get the current time into the member variables
		getCurrentTime(m_endTime, m_endCounter);

		double fDeltaTime = 0;
		if (!ms_bHighResSupported)
		{
			// return the low-res elapsed time
			fDeltaTime = getLowResElapsedTime(m_endTime);
		}
		else
		{
			// return the high-res elapsed time if possible
			fDeltaTime =  getHighResElapsedTime(m_endCounter);
		}
		m_fElapsedTime += fDeltaTime;

		m_bIsRunning = false;
	}
}
//-------------------------------------------------------------------------------------------------
bool StopWatch::isRunning()
{
	return m_bIsRunning;
}
//-------------------------------------------------------------------------------------------------
bool StopWatch::isReset()
{
	return m_bIsReset;
}
//-------------------------------------------------------------------------------------------------
const SYSTEMTIME& StopWatch::getBeginTime() const
{
	if(m_bIsReset)
	{
		
		throw UCLIDException("ELI10122", "Cannot determine start time while the stop watch is reset!");
	}
	else
	{
		return m_beginTime;
	}
}
//-------------------------------------------------------------------------------------------------
const SYSTEMTIME& StopWatch::getEndTime() const
{
	// ensure that the stop watch is not running
	if (m_bIsRunning)
	{
		throw UCLIDException("ELI09165", "Cannot determine end time when stop watch is still running!");
	}
	else if (m_bIsReset)
	{
		throw UCLIDException("ELI20440", "Cannot determine end time when stop watch has been reset!");
	}
	return m_endTime;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void StopWatch::getCurrentTime(SYSTEMTIME& rendTime, LARGE_INTEGER& rendCounter) const
{
	// get the end time
	GetLocalTime(&rendTime);

	// get the end-counter
	if (QueryPerformanceCounter(&rendCounter) == 0)
	{
		// for whatever reason, if the high resolution counter 
		// was not readable, turn off the high-res support for the
		// rest of this process's life.
		ms_bHighResSupported = false;
	}
}
//-------------------------------------------------------------------------------------------------
double StopWatch::getLowResElapsedTime(const SYSTEMTIME& endTime) const
{
	// if the end time is less than the start time log an exception
	if (asULongLong(endTime) < asULongLong(m_startTime))
	{
		UCLIDException ue("ELI20436", "End time is less than the start time!");
		
		// Log the exception for debug purposes and return 0
		ue.log();
		return 0.0;
	}

	// return the elapsed time in seconds
	ULONGLONG qwSpan = asULongLong(m_endTime) - asULongLong(m_startTime);

	return (double) qwSpan / ST_SECOND;
}
//-------------------------------------------------------------------------------------------------
double StopWatch::getHighResElapsedTime(const LARGE_INTEGER& endCounter) const
{
	// if the end time is less than the start time log an exception
	if (endCounter.QuadPart < m_startCounter.QuadPart)
	{
		UCLIDException ue("ELI20437", "End time is less than the start time!");
		ue.addDebugInfo("Start time", asString(m_startCounter.QuadPart));
		ue.addDebugInfo("End time", asString(endCounter.QuadPart));

		// Log the exception for debug purposes and return 0
		ue.log();
		return 0.0;
	}

	// return the elapsed time
	return (endCounter.QuadPart - m_startCounter.QuadPart) / 
		(ms_highResCounterFreq.QuadPart * 1.0);
}
//-------------------------------------------------------------------------------------------------
