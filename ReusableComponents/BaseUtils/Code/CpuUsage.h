// This class was downloaded from CodeGuru on 12/16/2005
// Article name: How to get CPU usage by performance counters (without PDH)
// Author: By Dudi Avramov 
// URL: http://www.codeproject.com/system/cpuusage.asp
// Files downloaded as part of this class:
//   CpuUsage.h
//   CpuUsage.cpp
//   PerfCounters.h

#pragma once

#include "BaseUtils.h"

#include <windows.h>

class EXPORT_BaseUtils CCpuUsage
{
public:
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To construct a new instance of the CCpuUsage class.
	CCpuUsage();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To cleanup an instance of the CCpuUsage class.
	virtual ~CCpuUsage();

	//----------------------------------------------------------------------------------------------
	// Methods
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To return the total CPU usage of the current system.
	int GetCpuUsage();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To return the total CPU usage of all processes with the specified name.
	int GetCpuUsage(LPCTSTR pProcessName);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To return the CPU usage for the specified process ID.
	int GetCpuUsage(DWORD dwProcessID);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To enable the performance counters so that performance data can be gathered.
	BOOL EnablePerformaceCounters(BOOL bEnable = TRUE);

private:
	//----------------------------------------------------------------------------------------------
	// Attributes
	//----------------------------------------------------------------------------------------------
	bool			m_bFirstTime;
	LONGLONG		m_lnOldValue ;
	LARGE_INTEGER	m_OldPerfTime100nSec;

	//----------------------------------------------------------------------------------------------
	// Methods
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To gather the CPU usage for either the specified PID, specified process names
	//			or if "_Total" is passed in for pProcessName gathers the total CPU usage
	//			on the current system.
	int GetCpuUsage(DWORD dwProcessID, LPCTSTR pProcessName, bool bTotal = false);
};