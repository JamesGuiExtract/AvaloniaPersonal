
#pragma once

#include "BaseUtils.h"
#include "Win32Mutex.h"
#include "StopWatch.h"

#include <fstream>
#include <vector>

//-------------------------------------------------------------------------------------------------
// TimeInterval struct
//-------------------------------------------------------------------------------------------------
class EXPORT_BaseUtils TimeInterval
{
public:
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Constructor
	// REQUIRE: stopWatch.isRunning() == false
	//			endTime >= startTime
	TimeInterval(const SYSTEMTIME& startTime, const SYSTEMTIME& endTime);
	TimeInterval(const StopWatch& stopWatch);
	//---------------------------------------------------------------------------------------------
	// copy ctor, assignment operator, less than, and equal operator
	TimeInterval(const TimeInterval& objToCopy);
	TimeInterval& operator=(const TimeInterval& objToAssign);
	friend bool operator < (const TimeInterval& a, const TimeInterval& b);
	friend bool operator == (const TimeInterval& a, const TimeInterval& b);
	//---------------------------------------------------------------------------------------------
	// PROMISE: To return the total seconds between m_startTime and m_endTime
	unsigned long getTotalSeconds() const;
	//---------------------------------------------------------------------------------------------
	// ability to output to stream for easy debugging
	friend std::ostream& operator << (std::ostream& rStream, const TimeInterval& interval);
	//---------------------------------------------------------------------------------------------
	// read-only access to the member variables
	inline const SYSTEMTIME& getStartTime() const
	{
		return m_startTime;
	}
	inline const SYSTEMTIME& getEndTime() const
	{
		return m_endTime;
	}
	//---------------------------------------------------------------------------------------------

private:
	// member variables
	SYSTEMTIME m_startTime;
	SYSTEMTIME m_endTime;
};

//-------------------------------------------------------------------------------------------------
// TimeIntervalMerger class
//-------------------------------------------------------------------------------------------------
class EXPORT_BaseUtils TimeIntervalMerger
{
public:
	//---------------------------------------------------------------------------------------------
	// ctor, copy ctor, and assignment operator
	TimeIntervalMerger();
	~TimeIntervalMerger();
	TimeIntervalMerger(TimeIntervalMerger& rObjToCopy);
	TimeIntervalMerger& operator=(TimeIntervalMerger& rObjToAssign);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Delete all TimeInterval objects associated with this
	//			object.
	void clear();
	//---------------------------------------------------------------------------------------------
	// REQUIRE: At least one merge call has been made with a non-zero interval
	//			passed to it.
	// PROMISE: return the total seconds in all the time intervals stored
	unsigned long getTotalSeconds();	
	//---------------------------------------------------------------------------------------------
	// PROMISE: If there is already a time interval associated with this object
	//			that overlaps the time interval associated with the stopwatch,
	//			the interval of the stopwatch will be merged with the interval
	//			associated with this time object.
	void merge(const TimeInterval& stopWatch);
	//---------------------------------------------------------------------------------------------
	// ability to output to stream for easy debugging
	friend std::ostream& operator << (std::ostream& rStream, TimeIntervalMerger& obj);
	//---------------------------------------------------------------------------------------------

private:
	std::vector<TimeInterval> m_vecIntervals;
	Win32Mutex m_lock;  // lock for this object

	static Win32Mutex ms_debugFileLock; // lock for this object's debug file
	static std::ofstream ms_debugFile; // debug file stream

	//---------------------------------------------------------------------------------------------
	// REQUIRE: The caller of these methods must ensure that no other thread is
	//			writing to the debug file for the duration of this method call.
	// PURPOSE: writeThreadIDDebugInfo() writes a line to the debug file with 
	//			the current thread's id, and writeObjectNameAndID() writes a line
	//			to the debug file with the name of this object and a long integer
	//			representing this object's address.
	void writeThreadIDDebugInfo();
	void writeObjectNameAndID();
	//---------------------------------------------------------------------------------------------
};
//-------------------------------------------------------------------------------------------------
