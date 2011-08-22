#include "stdafx.h"
#include "TimeIntervalMerger.h"
#include "UCLIDException.h"
#include "DateUtil.h"

#include <algorithm>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// leave the following line comment out if debug output should not be created
// #define OUTPUT_DEBUG_FILE

// static/global vars
Win32Mutex TimeIntervalMerger::ms_debugFileLock;
ofstream TimeIntervalMerger::ms_debugFile;

// format string for Long date and time representation
// For example: "Tuesday, March 14, 1995, 12:41:29".
static const string& gstrTIME_DEBUG_INFO_FORMAT = "%#c";

//-------------------------------------------------------------------------------------------------
// TimeInterval class
//-------------------------------------------------------------------------------------------------
TimeInterval::TimeInterval(const SYSTEMTIME& startTime, const SYSTEMTIME& endTime)
:m_startTime(startTime), m_endTime(endTime)
{
	try
	{
		// ensure proper argument
		ASSERT_ARGUMENT("ELI11117", asULongLong(m_endTime) >= asULongLong(m_startTime));
		ASSERT_ARGUMENT("ELI11128", asULongLong(m_endTime) >= 0);
		ASSERT_ARGUMENT("ELI11129", asULongLong(m_startTime) >= 0);
	}
	catch (UCLIDException& ue)
	{
		CString zStartTime = formatSystemTime(m_startTime, gstrTIME_DEBUG_INFO_FORMAT).c_str();
		CString zEndTime = formatSystemTime(m_endTime, gstrTIME_DEBUG_INFO_FORMAT).c_str();
		ue.addDebugInfo("startTime", (LPCTSTR) zStartTime);
		ue.addDebugInfo("endTime", (LPCTSTR) zEndTime);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
TimeInterval::TimeInterval(const StopWatch& stopWatch)
:m_startTime(stopWatch.getBeginTime()), m_endTime(stopWatch.getEndTime())
{
	try
	{
		// ensure proper argument
		ASSERT_ARGUMENT("ELI11124", asULongLong(m_endTime) >= asULongLong(m_startTime));
		ASSERT_ARGUMENT("ELI11130", asULongLong(m_endTime) >= 0);
		ASSERT_ARGUMENT("ELI11131", asULongLong(m_startTime) >= 0);
	}
	catch (UCLIDException& ue)
	{
		CString zStartTime = formatSystemTime(m_startTime, gstrTIME_DEBUG_INFO_FORMAT).c_str();
		CString zEndTime = formatSystemTime(m_endTime, gstrTIME_DEBUG_INFO_FORMAT).c_str();
		ue.addDebugInfo("startTime", (LPCTSTR) zStartTime);
		ue.addDebugInfo("endTime", (LPCTSTR) zEndTime);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
TimeInterval::TimeInterval(const TimeInterval& objToCopy)
{
	// invoke the assignment operator
	*this = objToCopy;
}
//-------------------------------------------------------------------------------------------------
TimeInterval& TimeInterval::operator=(const TimeInterval& objToAssign)
{
	// copy the data members
	m_startTime = objToAssign.m_startTime;
	m_endTime = objToAssign.m_endTime;
	
	return *this;
}
//-------------------------------------------------------------------------------------------------
bool operator < (const TimeInterval& a, const TimeInterval& b)
{
	if (asULongLong(a.m_startTime) < asULongLong(b.m_startTime))
	{
		return true;
	}
	else if (asULongLong(a.m_startTime) == asULongLong(b.m_startTime))
	{
		return (asULongLong(a.m_endTime) < asULongLong(b.m_endTime)) == TRUE;
	}
	else
	{
		return false;
	}
}
//-------------------------------------------------------------------------------------------------
bool operator == (const TimeInterval& a, const TimeInterval& b)
{
	return asULongLong(a.m_startTime) == asULongLong(b.m_startTime) && 
		   asULongLong(a.m_endTime) == asULongLong(b.m_endTime);
}
//-------------------------------------------------------------------------------------------------
ostream& operator << (ostream& rStream, const TimeInterval& interval)
{
	// write the interval object's data members to the output stream
	string strStartTime = formatSystemTime(interval.m_startTime, "%H:%M:%S");
	string strEndTime = formatSystemTime(interval.m_endTime, "%H:%M:%S");
	rStream << "Interval(" << strStartTime << ", " << strEndTime << ")";

	return rStream;
}
//-------------------------------------------------------------------------------------------------
unsigned long TimeInterval::getTotalSeconds() const
{
	// return the elapsed time in seconds
	ULONGLONG qwSpan = asULongLong(m_endTime) - asULongLong(m_startTime);

	return (unsigned long)(qwSpan / ST_SECOND);
}

//-------------------------------------------------------------------------------------------------
// TimeIntervalMerger class
//-------------------------------------------------------------------------------------------------
TimeIntervalMerger::TimeIntervalMerger()
{
#ifdef OUTPUT_DEBUG_FILE
	// only one instance of this object can be constructed
	// at any given time
	static Win32Mutex ls_ctorLock;
	Win32MutexLockGuard ctorGuard(ls_ctorLock);
	
	// create the debug output file if it has not yet been created
	static bool ls_bDebugFileCreated = false;
	if (!ls_bDebugFileCreated)
	{
		const char *pszDEBUG_FILE = "c:\\time_interval_merger.tmp";
		ms_debugFile.open(pszDEBUG_FILE, ios::app);
		if (!ms_debugFile)
		{
			UCLIDException ue("ELI11132", "Unable to create debug output file!");
			ue.addDebugInfo("File", pszDEBUG_FILE);
			throw ue;
		}

		ls_bDebugFileCreated = true;
	}

	// create debug info about object construction
	Win32MutexLockGuard guard(ms_debugFileLock);
	writeThreadIDDebugInfo();
	writeObjectNameAndID();
	ms_debugFile << " constructed." << endl;
#endif
}
//-------------------------------------------------------------------------------------------------
TimeIntervalMerger::~TimeIntervalMerger()
{
	try
	{
#ifdef OUTPUT_DEBUG_FILE
		Win32MutexLockGuard guard(ms_debugFileLock);
		writeThreadIDDebugInfo();
		writeObjectNameAndID();
		ms_debugFile << " destructed." << endl;
#endif
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16411");
}
//-------------------------------------------------------------------------------------------------
TimeIntervalMerger::TimeIntervalMerger(TimeIntervalMerger& rObjToCopy)
{
#ifdef OUTPUT_DEBUG_FILE
	Win32MutexLockGuard guard(ms_debugFileLock);
	writeThreadIDDebugInfo();
	writeObjectNameAndID();
	ms_debugFile << " copy constructed." << endl;
#endif

	// invoke the assignment operator
	*this = rObjToCopy;
}
//-------------------------------------------------------------------------------------------------
TimeIntervalMerger& TimeIntervalMerger::operator=(TimeIntervalMerger& rObjToAssign)
{
	// lock the objects
	Win32MutexLockGuard guard1(m_lock);
	Win32MutexLockGuard guard2(rObjToAssign.m_lock);

	// copy the data members
	m_vecIntervals = rObjToAssign.m_vecIntervals;

	return *this;
}
//-------------------------------------------------------------------------------------------------
ostream& operator << (ostream& rStream, TimeIntervalMerger& obj)
{
	// lock the object
	Win32MutexLockGuard guard(obj.m_lock);

	// write the object's data members to the output stream
	rStream << "INTERVAL_MERGER_DATA_BEGIN " << (long) &obj << endl;

	// write out each of the time intervals
	vector<TimeInterval>::const_iterator iter;
	for (iter = obj.m_vecIntervals.begin(); iter != obj.m_vecIntervals.end(); iter++)
	{	
		rStream << *iter << endl;
	}

	rStream << "INTERVAL_MERGER_DATA_END" << endl;

	return rStream;
}
//-------------------------------------------------------------------------------------------------
void TimeIntervalMerger::writeThreadIDDebugInfo()
{
	ms_debugFile << "--- [" << GetCurrentThreadId() << "] --- " << endl;
}
//-------------------------------------------------------------------------------------------------
void TimeIntervalMerger::writeObjectNameAndID()
{
	ms_debugFile << "TimeIntervalMerger " << (long) this;
}
//-------------------------------------------------------------------------------------------------
void TimeIntervalMerger::clear()
{
	// lock the object
	Win32MutexLockGuard guard(m_lock);

	m_vecIntervals.clear();
}
//-------------------------------------------------------------------------------------------------
unsigned long TimeIntervalMerger::getTotalSeconds()
{
	// lock the object
	Win32MutexLockGuard guard(m_lock);

	unsigned long ulTotal = 0;
	
	// iterate through the time intervals and 
	vector<TimeInterval>::const_iterator iter;
	for (iter = m_vecIntervals.begin(); iter != m_vecIntervals.end(); iter++)
	{
		ulTotal += iter->getTotalSeconds();
	}

	return ulTotal;
}
//-------------------------------------------------------------------------------------------------
void TimeIntervalMerger::merge(const TimeInterval& interval)
{
	// lock the object
	Win32MutexLockGuard objGuard(m_lock);

#ifdef OUTPUT_DEBUG_FILE
	Win32MutexLockGuard guard(ms_debugFileLock);
	writeThreadIDDebugInfo();
	ms_debugFile << "Merge called with " << interval << endl;
	ms_debugFile << "Current interval manager is:" << endl << *this << endl;
#endif

	if (m_vecIntervals.empty())
	{
		// this is the first interval passed to this object - just store it
		m_vecIntervals.push_back(interval);

#ifdef OUTPUT_DEBUG_FILE
		ms_debugFile << interval << " pushed onto vector" << endl;
#endif
	}
	else
	{
		// find the best place in the intervals list to insert the interval
		// (the best place may be anywhere in the list, including at the end)
		vector<TimeInterval>::iterator iter;
		iter = lower_bound(m_vecIntervals.begin(), m_vecIntervals.end(), interval);
		unsigned long nInsertPos = iter - m_vecIntervals.begin();
		
		// if there is an item at the insert position, and if that item is 
		// equal to the new interval we want to merge with, we don't need to do
		// anything more.
		// NOTE: this can happen because the resolution of the TimeInterval
		// class is 1 second, and many operations can be completed within one
		// second.
		if (nInsertPos < m_vecIntervals.size() && 
			m_vecIntervals[nInsertPos] == interval)
		{
#ifdef OUTPUT_DEBUG_FILE
			ms_debugFile << interval << " already in vector - so ignored" << endl;
#endif
			return;
		}

		// there is no exact duplicate to the new interval being merged
		// so add it to our vector
		m_vecIntervals.insert(iter, interval);

#ifdef OUTPUT_DEBUG_FILE
		ms_debugFile << interval << " inserted vector at " << nInsertPos << endl;
		ms_debugFile << "Result after insertion is:" << endl;
		ms_debugFile << *this << endl;
#endif

		// now check the intervals prior to the inserted position to see if
		// there is any overlap
		unsigned long ulLeftIndex = nInsertPos;
		while (ulLeftIndex > 0)
		{
			const TimeInterval& leftInterval = m_vecIntervals[ulLeftIndex - 1];
			if (asULongLong(leftInterval.getEndTime()) >= asULongLong(interval.getStartTime()) ||
				asULongLong(leftInterval.getStartTime()) == asULongLong(interval.getStartTime()))
			{
				ulLeftIndex--;
				continue;
			}

			break;
		}

		// now check the intervals after the inserted position to see if there
		// is any overlap
		unsigned long ulNumIntevals = m_vecIntervals.size();
		unsigned long ulRightIndex = nInsertPos;
		while (ulRightIndex < ulNumIntevals - 1)
		{
			const TimeInterval& rightInterval = m_vecIntervals[ulRightIndex + 1];
			if (asULongLong(rightInterval.getStartTime()) <= asULongLong(interval.getEndTime()) ||
				asULongLong(rightInterval.getEndTime()) == asULongLong(interval.getEndTime()))
			{
				ulRightIndex++;
				continue;
			}

			break;
		}

		// if there is any overlap, then do the merge
		if (ulLeftIndex != nInsertPos || ulRightIndex != nInsertPos)
		{
			// determine the start/end time of the new merged inteval
			SYSTEMTIME startTime = m_vecIntervals[ulLeftIndex].getStartTime();
			SYSTEMTIME endTime = m_vecIntervals[ulRightIndex].getEndTime();

			// erase the overlapping intervals
			// get the iterStart iterator to the correct location
			vector<TimeInterval>::iterator iterStart, iterEnd;
			iterStart = m_vecIntervals.begin();
			iterEnd = m_vecIntervals.begin();
			unsigned long ul;
			for (ul = 0; ul < ulLeftIndex; ul++)
			{
				iterStart++;
			}

			// get the iterEnd iterator to the correct location
			iterEnd = iterStart;
			for (ul = ulLeftIndex; ul <= ulRightIndex; ul++)
			{
				iterEnd++;
			}

			// Protect against iterators pointing to same location
			if (iterEnd == iterStart)
			{
				iterEnd++;
			}

#ifdef OUTPUT_DEBUG_FILE
			ms_debugFile << "About to erase some entries..." << endl;
			ms_debugFile << "LeftIndex = " << nLeftIndex << " RightIndex = " << nRightIndex << endl;
#endif

			// erase the entries that are to be merged
			m_vecIntervals.erase(iterStart, iterEnd);

#ifdef OUTPUT_DEBUG_FILE
			ms_debugFile << "Result after erase is:" << endl;
			ms_debugFile << *this << endl;
#endif

			// insert the new merged interval into the vector
			m_vecIntervals.insert(m_vecIntervals.begin() + ulLeftIndex, 
				TimeInterval(startTime, endTime));
		}
	}

#ifdef OUTPUT_DEBUG_FILE
	ms_debugFile << "Merged result is:" << endl;
	ms_debugFile << *this << endl;
#endif
}
//-------------------------------------------------------------------------------------------------