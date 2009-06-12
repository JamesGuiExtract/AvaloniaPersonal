//==================================================================================================
//
// COPYRIGHT (c) 2007 - 2008 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ProcessStatisticsManager.h
//
// PURPOSE:	ProcessStatisticsManager will query the system to get a list of running processes and 
//			extract data about those running processes.  The data is placed into a class called
//			IndividualProcessStatistics.  This class could be expanded to include more information,
//			it currently only logs the Physical Memory, Virtual Memory, Thread Count and Handle Count.
//
// NOTES:	This code is adopted and modified from
//			http://msdn2.microsoft.com/en-us/library/aa384724.aspx
//
// AUTHORS:	Jeff Shergalis
//			Arvind Ganesan
//
//==================================================================================================

#pragma once

#include "BaseUtils.h"
#include "cpputil.h"

#include <string>
#include <set>
#include <vector>

#include <atlcomcli.h>
#include <Wbemidl.h>

//==================================================================================================
//
// CLASS:	IndividualProcessStatistics
//
// PURPOSE:	Basically a struct with some methods to encapsulate the data returned from the
//			ProcessStatisticsManager
//
//==================================================================================================
// Modified 06/25/2008 - JDS - as per [LegacyRCAndUtils #4989]
//		Changed the variable names in the structure to better reflect the data that is
//		being collected.  Also removed the getTotal function as the total memory usage
//		is already being reported from the OS
class EXPORT_BaseUtils IndividualProcessStatistics
{
public:
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To create a default instance of an IndividualProcessStatistic.  The IPS serves basically
	//			as a struct to hold the data returned by the ProcessStatisticsManager
	IndividualProcessStatistics();	
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To create an instance of an IndividualProcessStatistic.  The IPS serves basically
	//			as a struct to hold the data returned by the ProcessStatisticsManager
	IndividualProcessStatistics(const string& rstrProcessName, DWORD dwProcessID, 
		time_t tCurrentTime, DWORD dwTotalMemory, DWORD dwAllocatedVirtualMemory, 
		DWORD dwHandleCount, DWORD dwThreadCount);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To return a string that can serve as a key value for storing the IPS in a map or other
	//			similar data structure.  Can also be used to generate a unique file name for IPS if
	//			logging data to a file for a process
	//
	// PROMISE: The returned string will be of the form "processName.processID"
	string getKeyValue();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To provide a simple way to add an IndividualProcessStatistic to the current
	//			IndividualProcessStatistic
	//
	// PROMISE: Adds the following values
	//			DWORD m_dwTotalMemoryBytes;
	//			DWORD m_dwAllocatedVirtualMemoryBytes;
	//			DWORD m_dwHandleCount;
	//			DWORD m_dwThreadCount;
	//  
	// NOTE:	This method does not check the m_ProcessID to see if it is the same process all it does is adds. 
	//			This operator is useful for creating an IndividualProcessStatistics instance that is the summation 
	//			of other IndividualProcessStatistics
	IndividualProcessStatistics& operator += (const IndividualProcessStatistics& ipsNewStats);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To provide an easy way to compare two IndividualProcessStatistics for equality
	// 
	// PROMISE: this operator overload will compare the following values and return true if all of them match
	//			string m_strProcessName
	//			DWORD m_dwProcessID
	//			DWORD m_dwTotalMemoryBytes
	//			DWORD m_dwAllocatedVirtualMemoryBytes
	//			DWORD m_dwHandleCount
	//			DWORD m_dwThreadCount
	bool operator == (const IndividualProcessStatistics& ripsNewStats);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To provide an easy way to compare two IndividualProcessStatistics for inequality
	//
	// PROMISE: This method simply returns the !(this == ripsNewStats)
	bool operator != (const IndividualProcessStatistics& ripsNewStats);
	//----------------------------------------------------------------------------------------------
	// Public member variables
	string m_strProcessName;
	DWORD m_dwProcessID;
	__time64_t m_tCurrentTime;
	DWORD m_dwTotalMemoryBytes;
	DWORD m_dwAllocatedVirtualMemoryBytes;
	DWORD m_dwHandleCount;
	DWORD m_dwThreadCount;
};

//==================================================================================================
//
// CLASS:	ProcessStatisticsManager
//
// PURPOSE:	To query the system to get a list of running processes and data associated with those 
//			running processes 
//
// REQUIRE:	CoInitialize() must have been called within the current thread before this
//			class can be instantiated
//
// EXTENSIONS:
//			An easy extension would be to track more data, see msdn article:
//			http://msdn2.microsoft.com/en-us/library/aa394323.aspx
//			In the event that that link is gone, search for "Win32_PerfRawData_PerfProc_Process"
//
//==================================================================================================
class EXPORT_BaseUtils ProcessStatisticsManager
{
public:
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To create an instance of the ProcessStatisticsManager.
	//
	// REQUIRE: CoInitialize must have been called prior to instantiating this class.
	//
	ProcessStatisticsManager();
	//----------------------------------------------------------------------------------------------
	~ProcessStatisticsManager();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To return to the user a vector of IndividualProcessStatistics.
	//
	// REQUIRE: setPID = a set of longs containing the Process IDs to be logged (may be an empty set)
	//			setPName = a set of strings containing the name of the process to be logged
	//			(may be an empty set)
	//
	// PROMISE: Will return a vector of IndividualProcessStatistics containing one entry for each of the
	//			currently running Process ID's and for each running Process Name (if more than one process
	//			is running with the same name, then each one will be logged individually).
	//			In the case where both the setPID and setPName were empty the returned vector will be empty.
	//			If there are no matching process found to be running, the method will return an empty vector.
	//			Also return the time stamp value associated with the IndividualProcessStatistics vector.
	__time64_t getProcessStatistics(vector<IndividualProcessStatistics>& rvecProcessStats,
		const set<long>& rsetPID, const set<string>& rsetPName);
	//----------------------------------------------------------------------------------------------
private:
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To get the associated handle and property type for the given Property Name
	//
	// REQUIRE: wszPropertyName must be a valid property string from the 
	//			"Win32_PerfRawData_PerfProc_Process" class, see MSDN article:
	//			http://msdn2.microsoft.com/en-us/library/aa394323.aspx
	//
	// PROMISE: Will place the property type and handle in the corresponding reference variables
	void getPropertyHandle(IWbemObjectAccess* pEnumAccess, LPCWSTR rwszPropertyName,
		CIMTYPE& rctPropertyType, long& rlPropertyHandle);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To get the current process data for the specified process and property handle
	//
	// REQUIRE: A valid property handle to get data from (gathered by the getPropertyHandle method)
	//
	// PROMISE: Will place the associated data for the handle into the rdwData variable
	void readWord(IWbemObjectAccess* pEnumAccess, long& rlHandle, DWORD& rdwData);
	//----------------------------------------------------------------------------------------------
	// Private member variables
	IWbemRefresher* m_pRefresher;
	IWbemHiPerfEnum* m_pEnum;
};
//==================================================================================================
