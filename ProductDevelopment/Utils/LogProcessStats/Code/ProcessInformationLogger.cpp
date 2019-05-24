#include "stdafx.h"
#include "ProcessInformationLogger.h"

#include <ProcessStatisticsManager.h>
#include <UCLIDException.h>
#include <cpputil.h>

#include <vector>
#include <set>
#include <map>
#include <ctime>
#include <fstream>

using namespace std;
//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
const string gstr_SEPARATOR = ",";

// clean interval in seconds.  used to check if time to clean process map
const __int64 gi64_CLEAN_INTERVAL = 3600;

// default sleep break interval in milliseconds. used in the constructor when 
// setting m_ulBreakInterval.
const unsigned long gulBREAK_INTERVAL = 1000;

// size of the arrays for formatted time strings
const int giFORMATTED_DATE_LENGTH = 12;
const int giFORMATTED_TIME_LENGTH = 10;

//--------------------------------------------------------------------------------------------------
ProcessInformationLogger::ProcessInformationLogger(const set<long> &rsetPID, 
									 const set<string> &rsetPName, 
									 long lRefreshInterval, const std::string &rstrWorkingDirectory) :
		m_setPIDs(rsetPID),
		m_setPNames(rsetPName),
		m_ulRefreshInterval(lRefreshInterval),
		m_strWorkingDirectory(rstrWorkingDirectory),
		m_cCpuUsage(),
		m_bKeepLooping(true),
		m_tLastCpuTime(0),
		m_psmMyManager()
{
	m_ulBreakInterval = min(m_ulRefreshInterval, gulBREAK_INTERVAL);
	_time64(&m_tLastCleanTime);
}

//--------------------------------------------------------------------------------------------------
// Public Methods
//--------------------------------------------------------------------------------------------------
void ProcessInformationLogger::start()
{
	// if both sets are empty then we will not log any stats
	if ( (m_setPIDs.size() == 0) && (m_setPNames.size() == 0) )
	{
		UCLIDException ue("ELI16735", 
			"Error: Empty sets passed to ProcessInformationLogger::start()!");
		throw ue;
	}

	// check if we need to output an all.csv file (PNames > 0 or PIDs > 1)
	bool bAllCsv = false;
	if ( (m_setPNames.size() > 0) || (m_setPIDs.size() > 1) )
	{
		bAllCsv = true;
	}


	// the map contains a list of all of the data points that we have seen.
	// the key is based on the IPS.getKeyValue() method.  we use the
	// map to store the last data point and then compare the last point
	// to the current point to see if we need to log it
	map<string, IndividualProcessStatistics> mapLastDataPoint;

	// begin gathering data points and logging
	while (m_bKeepLooping)
	{
		// vector to hold all of the stats that we get back from the ProcessStatisticsManager
		vector<IndividualProcessStatistics> vecProcStats;
		__time64_t tCurrentTime = m_psmMyManager.getProcessStatistics(
										vecProcStats, m_setPIDs, m_setPNames);
		// log the cpu usage
		logCPUUsage(tCurrentTime);
		
		// check to see if its time to perform map cleanup
		if (isTimeForCleanup(tCurrentTime))
		{
			cleanMap(vecProcStats, mapLastDataPoint);
			m_tLastCleanTime = tCurrentTime;
			
		}

		// ipsTotalStatistics holds the data for our all.csv output, needs to be set to 0 to start
		IndividualProcessStatistics ipsTotalStatistics("ALL",0,0,0,0,0,0);

		// loop through each of the process statistics and log them
		// if any of the data changed we need to output a line in the all.csv file
		bool bWroteData = false; 
		for ( unsigned int i = 0; i < vecProcStats.size(); i++ )
		{
			string strKeyValue = vecProcStats[i].getKeyValue();

			IndividualProcessStatistics lastStats = mapLastDataPoint[strKeyValue];
			mapLastDataPoint[strKeyValue] = vecProcStats[i];
			if (lastStats != vecProcStats[i])
			{
				log(vecProcStats[i]);
				bWroteData = true;
			}

			ipsTotalStatistics += vecProcStats[i];
		}			
		
		// check to see if we need to output a line in the all.csv file
		if ( bWroteData && bAllCsv )
		{
			// set the timestamp
			ipsTotalStatistics.m_tCurrentTime = tCurrentTime;
			log ( ipsTotalStatistics );			
		}

		// clear the process stats vector
		vecProcStats.clear();

		// sleep in intervals, waking to check if end has been called
		unsigned long ulSleepTime = m_ulRefreshInterval;
		while (ulSleepTime > 0 && m_bKeepLooping)
		{
			// check if the sleep time remaining is < the break interval
			if (ulSleepTime < m_ulBreakInterval)
			{
				// sleep the remaining time and set remaining time to 0
				Sleep(ulSleepTime);
				ulSleepTime = 0;
			}
			else
			{
				// sleep for the break interval and deduct the time from our
				// sleep time remaining
				Sleep(m_ulBreakInterval);
				ulSleepTime -= m_ulBreakInterval;
			}
		}
	}
}
//--------------------------------------------------------------------------------------------------
void ProcessInformationLogger::end()
{
	m_bKeepLooping = false;
}

//--------------------------------------------------------------------------------------------------
// Private Methods
//--------------------------------------------------------------------------------------------------
void ProcessInformationLogger::log(IndividualProcessStatistics& rIps)
{
	// build the file name
	string strFileName = m_strWorkingDirectory + "\\";
	string strKeyValue = rIps.getKeyValue();
	strFileName += (strKeyValue == "ALL.0") ? "all" : strKeyValue;
	strFileName += ".csv";

	// here we check to see if we have recorded a baseline time stamp for this process
	// if we haven't then we need to set the baseline stamp
	__time64_t tMapTimeValue = m_mapFirstTimeStamp[strKeyValue];
	if (tMapTimeValue == 0)
	{
		tMapTimeValue = rIps.m_tCurrentTime;
		m_mapFirstTimeStamp[strKeyValue] = tMapTimeValue;
	}

	// here we need to get the time into a pretty format so it is easily readable by user
	tm stTimeInfo;
	errno_t err = localtime_s(&stTimeInfo, &rIps.m_tCurrentTime);
	if (err)
	{
		UCLIDException ue("ELI16697", "Error processing current time.");
		ue.addDebugInfo("Error No.", err);
		throw ue;
	}

	// format the date and time for output
	char caDate[giFORMATTED_DATE_LENGTH] = {0};
	char caTime[giFORMATTED_TIME_LENGTH] = {0};
	strftime(caDate, giFORMATTED_DATE_LENGTH, "%m/%d/%Y", &stTimeInfo);
	strftime(caTime, giFORMATTED_TIME_LENGTH, "%H:%M:%S", &stTimeInfo);
	
	// open our file for output
	ofstream f_out(strFileName.c_str(), ios::app);	
	if (!f_out)
	{
		UCLIDException ue("ELI16672", "Cannot open file for output!");
		ue.addDebugInfo("File Name", strFileName);
		throw ue;
	}	

	// write data to the file
	// Modified 06/25/2008 - JDS - as per [LegacyRCAndUtils #4989] - updated the output
	// based on the changes to IndividualProcessStatistics
	f_out << string(caDate) << gstr_SEPARATOR << string(caTime) << gstr_SEPARATOR;
	f_out << rIps.m_tCurrentTime << gstr_SEPARATOR << rIps.m_strProcessName << gstr_SEPARATOR;
	f_out << rIps.m_dwProcessID << gstr_SEPARATOR << rIps.m_dwTotalMemoryBytes;
	f_out << gstr_SEPARATOR << rIps.m_dwAllocatedVirtualMemoryBytes << gstr_SEPARATOR;
	f_out << rIps.m_dwHandleCount << gstr_SEPARATOR << rIps.m_dwThreadCount;
	f_out << gstr_SEPARATOR << rIps.m_tCurrentTime - tMapTimeValue << endl;
	f_out.close();
	waitForFileAccess(strFileName, giMODE_READ_ONLY);
}
//--------------------------------------------------------------------------------------------------
void ProcessInformationLogger::logCPUUsage(__time64_t tCurrentTime)
{
	// check if baseline time stamp has been set
	if (m_tLastCpuTime == 0)
	{
		// set baseline time stamp
		m_tLastCpuTime = tCurrentTime;
	}

	// build output file name
	string strFileName = m_strWorkingDirectory + "\\cpu.csv";

	// open our file for output
	ofstream f_out(strFileName.c_str(), ios::app);	
	if (!f_out)
	{
		UCLIDException ue("ELI17017", "Cannot open file for output!");
		ue.addDebugInfo("File Name", strFileName);
		throw ue;
	}	

	// write data to the file
	f_out << tCurrentTime - m_tLastCpuTime << gstr_SEPARATOR << m_cCpuUsage.GetCpuUsage() << endl;
	f_out.close();
	waitForFileAccess(strFileName, giMODE_READ_ONLY);
}
//--------------------------------------------------------------------------------------------------
void ProcessInformationLogger::cleanMap(vector<IndividualProcessStatistics>& rvecProcesses,
										map<string, IndividualProcessStatistics>& rmapDataPoints)
{

	// create set of strings containing the key values and fill it
	set<string> setProcessKeys;
	for (vector<IndividualProcessStatistics>::iterator it = rvecProcesses.begin();
		it != rvecProcesses.end(); it++)
	{
		setProcessKeys.insert(it->getKeyValue());
	}

	// vector of map iterators used to store the iterators of map entries no longer needed
	vector<map<string,IndividualProcessStatistics>::iterator> vecMapRemovalIterators; 

	// iterate over the map comparing it with the entries in the set.  if it exists
	// in the set than keep the map entry, if not store the iterator in the removal
	// vector
	for (map<string, IndividualProcessStatistics>::iterator it = rmapDataPoints.begin();
		it != rmapDataPoints.end(); it++)
	{
		set<string>::iterator setIt = setProcessKeys.find(it->first);
		if (setIt != setProcessKeys.end())
		{
			// remove this set entry, this reduces the search space.  The keys
			// are unique, there will not be multiple occurrences of the keys in
			// the map
			setProcessKeys.erase(setIt);
		}
		else
		{
			// if not found add this iterator to the removal vector
			vecMapRemovalIterators.push_back(it);

			// Also clean up the baseline time stamp map
			m_mapFirstTimeStamp.erase(it->first);
		}
	}

	// loop through the removal vector removing entries from the map
	for (size_t i = 0; i < vecMapRemovalIterators.size(); i++)
	{
		rmapDataPoints.erase(vecMapRemovalIterators[i]);
	}
}
//--------------------------------------------------------------------------------------------------
bool ProcessInformationLogger::isTimeForCleanup(__time64_t tCurrentTime)
{
	bool bReturn = (tCurrentTime - m_tLastCleanTime) > gi64_CLEAN_INTERVAL;

	return bReturn;
}
//--------------------------------------------------------------------------------------------------