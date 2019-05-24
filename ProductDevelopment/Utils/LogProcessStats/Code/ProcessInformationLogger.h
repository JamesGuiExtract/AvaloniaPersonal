//==================================================================================================
//
// COPYRIGHT (c) 2007 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ProcessInformationLogger.h
//
// PURPOSE:	ProcessInformationLogger makes use of the ProcessStatisticsManager to query the system
//			about running processes.  The ProcessInformationLogger only requests data for a given
//			set of Process ID's and/or Process Names.  It will then output the data for each
//			individual process to an individual file for each process that will be named
//			"ProcessName.ProcessID.csv".  If the user has given us more than one process
//			or a process name we will also output a file called "all.csv" containing a sum total
//			of the information for each process.
//
// NOTE:	If given a process name we will continue to watch even if currently there are no processes
//			with that name running.  The same applies with the Process ID.
// 
// AUTHORS:	Jeff Shergalis
//			Arvind Ganesan
//
//==================================================================================================
#pragma once

#include "stdafx.h"

#include <ProcessStatisticsManager.h>
#include <CpuUsage.h>

#include <set>
#include <map>
#include <string>
#include <ctime>

class ProcessInformationLogger
{
public:
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To construct our ProcessInformationLogger
	//
	// REQUIRE: rstrWorkingDirectory is a string pointing to a valid directory, 
	//			it should not end with \
	//
	// PROMISE: To instantiate the class and get it ready to begin logging data
	ProcessInformationLogger(const set<long>& rsetPIDs, const set<string>& rsetPNames,
		long lRefreshInterval, const string& rstrWorkingDirectory);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To start the data collecting process.
	//
	// REQUIRE: 1 of the sets (either setPIDs or setPNames is not empty)
	// 
	// PROMISE: This method will not return until end() has been called.
	void start();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To end the data collecting process.
	void end();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To get the refresh interval of the object
	inline unsigned long getRefreshBreakInterval() { return m_ulBreakInterval; }
	//----------------------------------------------------------------------------------------------

private:
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To log the data for the IndividualProcessStatistic to a file
	//
	// REQUIRE: A valid IPS
	//
	// PROMISE: To log one line of data to a file in the current working directory. We generate the file name
	//			by calling rIpd.getKeyValue() and tagging on ".csv". In the case where we are logging the 
	//			ipsTotalStatistics count, we look for a rIps.getKeyValue() of "ALL.0", in this case we will log to 
	//			the file "all.csv"
	void log(IndividualProcessStatistics& rIps);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To log the total cpu usage on the system
	//
	// PROMISE: Will create a file cpu.csv in the current working directory.  The file will contain
	//			two columns: the timestamp and the percent cpu usage.
	void logCPUUsage(__time64_t tCurrentTime);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To compare the current time with the base time and return true if gui_CLEAN_INTERVAL
	//			has elapsed
	bool isTimeForCleanup(__time64_t tCurrentTime);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To remove processes from the map that no longer exist
	void cleanMap(vector<IndividualProcessStatistics>& rvecProcesses,
		map<string, IndividualProcessStatistics>& rmapDataPoints);
	//----------------------------------------------------------------------------------------------
	// Private member variables
	const	set<long>& m_setPIDs;
	const	set<string>& m_setPNames;
	const	string& m_strWorkingDirectory;
	unsigned long	m_ulRefreshInterval;

	// break interval for the sleep calls in the start function.  this is used
	// to allow the logger to wake and check for an end call without having
	// to finish sleeping through the entire refresh interval (which could
	// be a very long time)
	unsigned long   m_ulBreakInterval;

	CCpuUsage m_cCpuUsage;
	__time64_t m_tLastCpuTime;
	__time64_t m_tLastCleanTime;

	volatile bool m_bKeepLooping;

	// this map stores the first time stamp for each process, we use this to compute a baseline
	// of run time so that we can graph things that are meaningful in Excel. by storing the
	// initial time stamp we can have a basic row count that starts at 0 and increases by the
	// amount of time the process has been running (as opposed to a simple row count)
	map<string, __time64_t> m_mapFirstTimeStamp;

	// Added as a member variable so the initialization of the CoInitializeSecurity call
	// is at the time ProcessInformationLogger is created.
	ProcessStatisticsManager m_psmMyManager;
};
//--------------------------------------------------------------------------------------------------