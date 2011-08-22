
#pragma once

#include "BaseUtils.h"

class EXPORT_BaseUtils StopWatch
{
public:
	// PROMISE: To reset the stop watch
	StopWatch();

	// Copy Ctor and assignment operator
	StopWatch(const StopWatch& watch);
	StopWatch& operator=(const StopWatch& watch);

	// PROMISE: To reset the time to 0;
	void reset();

	// PROMISE: To start the watch.
	// Subsequent calls to isRunning() will return true;
	void start();

	// PROMISE: To stop the stop watch so that any subsequent call
	// to getElapsedTime() will return the same results as a call that
	// would have taken place right now.
	// Also, subsequent calls to isRunning() will return false
	void stop();

	// PROMISE: If the watch is running, the number of seconds that
	// have elapsed since the stop watch was reset will be returned.
	// If the watch has been stopped, the number of seconds that elapsed
	// between the calls to reset() and stop() will be returned.
	double getElapsedTime() const;

	// PROMISE: To return true if the stop watch is currently running.
	bool isRunning();

	// PROMISE: To return true if the stop watch is currently reset.
	bool isReset();

	// REQUIRE: isReset() == false
	// PROMISE: To return the time at which the watch was started from a reset state
	const SYSTEMTIME& getBeginTime() const;

	// REQUIRE: isRunning() == false
	// PROMISE: To return the end time of this watch.
	const SYSTEMTIME& getEndTime() const;

private:
	// keep track of start time in SYSTEMTIME (in case the hardware does not
	// support high resolution counters)
	SYSTEMTIME m_startTime, m_endTime;

	// if the hardware supports high resolution counters, then we keep
	// track of the start counter here.
	LARGE_INTEGER m_startCounter, m_endCounter;

	// boolean to keep track of whether the hardware supports 
	// high resolution counters
	static bool ms_bHighResSupported;

	double m_fElapsedTime;

	// the value of the high resolution counter frequency is stored in
	// this static variable if the hardware has support for the high res counter
	static LARGE_INTEGER ms_highResCounterFreq;

	// bool to keep track of whether the watch is running
	bool m_bIsRunning;

	// true after the watch is reset until the watch is started
	bool m_bIsReset;

	// This is the time that the watch was started with the clock at 0
	// i.e. the time of the first start after a reset
	SYSTEMTIME m_beginTime;

	// PURPOSE: To get the current low-res and high-res time/counter
	// values into the provided variables
	void getCurrentTime(SYSTEMTIME& rendTime, LARGE_INTEGER& rendCounter) const;
	
	// get the low-resolution elapsed time.  Resolution of returned
	// value is 1 second.
	double getLowResElapsedTime(const SYSTEMTIME& endTime) const;

	// get the high-resolution elapsed time
	double getHighResElapsedTime(const LARGE_INTEGER& endCounter) const;
};