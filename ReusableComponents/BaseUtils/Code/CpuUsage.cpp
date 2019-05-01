#include "stdafx.h"
#include <atlbase.h>	// for CRegKey use
#include "CpuUsage.h"
#include "UCLIDException.h"
#include "Win32Util.h"
#include <VersionHelpers.h>

#pragma pack(push,8)
#include "PerfCounters.h"
#pragma pack(pop)

#define PROCESS_OBJECT_INDEX				230		// 'Process' object
#define PROCESSOR_OBJECT_INDEX				238		// 'Processor' object
#define PROCESSOR_TIME_COUNTER_INDEX		6		// '% processor time' counter (for Win2K/XP)

///////////////////////////////////////////////////////////////////
//
//		GetCpuUsage uses the performance counters to retrieve the
//		system cpu usage.
//		The cpu usage counter is of type PERF_100NSEC_TIMER_INV
//		which as the following calculation:
//
//		Element		Value 
//		=======		===========
//		X			CounterData 
//		Y			100NsTime 
//		Data Size	8 Bytes
//		Time base	100Ns
//		Calculation 100*(1-(X1-X0)/(Y1-Y0)) 
//
//      where the denominator (Y) represents the total elapsed time of the 
//      sample interval and the numerator (X) represents the time during 
//      the interval when the monitored components were inactive.
//
//
//		Note:
//		====
//		On windows NT, cpu usage counter is '% Total processor time'
//		under 'System' object. However, in Win2K/XP Microsoft moved
//		that counter to '% processor time' under '_Total' instance
//		of 'Processor' object.
//		Read 'INFO: Percent Total Performance Counter Changes on Windows 2000'
//		Q259390 in MSDN.
//
///////////////////////////////////////////////////////////////////
CCpuUsage::CCpuUsage()
{
	try
	{
		// If the current platform is not Win2K or greater then throw an exception
		if (!IsWindowsVistaOrGreater())
		{
			UCLIDException uex("ELI21589", "Unsupported operating system for CpuUsage!");
			uex.addDebugInfo("Platform", getPlatformAsString());
			throw uex;
		}

		m_bFirstTime = true;
		m_lnOldValue = 0;
		memset(&m_OldPerfTime100nSec, 0, sizeof(m_OldPerfTime100nSec));
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI21590");
}
//--------------------------------------------------------------------------------------------------
CCpuUsage::~CCpuUsage()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16379");
}
//--------------------------------------------------------------------------------------------------
BOOL CCpuUsage::EnablePerformaceCounters(BOOL bEnable)
{
	CRegKey regKey;
	if (regKey.Open(HKEY_LOCAL_MACHINE, 
		"SYSTEM\\CurrentControlSet\\Services\\PerfOS\\Performance") != ERROR_SUCCESS)
	{
		return FALSE;
	}

	// TESTTHIS: was it right to replace SetValue with SetDWORDValue?
	regKey.SetDWORDValue("Disable Performance Counters", !bEnable);
	regKey.Close();

	if (regKey.Open(HKEY_LOCAL_MACHINE, 
		"SYSTEM\\CurrentControlSet\\Services\\PerfProc\\Performance") != ERROR_SUCCESS)
	{
		return FALSE;
	}

	// TESTTHIS: was it right to replace SetValue with SetDWORDValue?
	regKey.SetDWORDValue("Disable Performance Counters", !bEnable);
	regKey.Close();

	return TRUE;
}
//--------------------------------------------------------------------------------------------------
//
//	GetCpuUsage returns the system-wide cpu usage.
//	Since we calculate the cpu usage by two samplings, the first
//	call to GetCpuUsage() returns 0 and keeps the values for the next
//	sampling.
//  Read the comment at the beginning of this file for the formula.
//
int CCpuUsage::GetCpuUsage()
{
	return GetCpuUsage(0, "_Total", true);
}
//--------------------------------------------------------------------------------------------------
int CCpuUsage::GetCpuUsage(LPCTSTR pProcessName)
{
	return GetCpuUsage(0, pProcessName);
}
//--------------------------------------------------------------------------------------------------
int CCpuUsage::GetCpuUsage(DWORD dwProcessID)
{
	return GetCpuUsage(dwProcessID, NULL);
}

//--------------------------------------------------------------------------------------------------
// Private methods
//--------------------------------------------------------------------------------------------------
int CCpuUsage::GetCpuUsage(DWORD dwProcessID, LPCTSTR pProcessName, bool bTotal)
{
	if (m_bFirstTime)
	{
		EnablePerformaceCounters();
	}
	
	// cpu usage counter is 8 byte length.
	CPerfCounters<LONGLONG> PerfCounters;

	// set the object and cpu usage indexes
	DWORD dwObjectIndex = bTotal ? PROCESSOR_OBJECT_INDEX : PROCESS_OBJECT_INDEX;
	DWORD dwCpuUsageIndex = PROCESSOR_TIME_COUNTER_INDEX;

	// initialize variables
	int				CpuUsage = 0;
	LONGLONG		lnNewValue = 0;
	unique_ptr<PERF_DATA_BLOCK> apPerfData(__nullptr);
	LARGE_INTEGER	NewPerfTime100nSec = {0};

	// check process name, if NULL then get data for specified PID
	if (pProcessName != __nullptr)
	{
		// initialize the instance string to empty string
		char szInstance[256] = {0};

		// copy the pProcessName data to the szInstance string
		strcpy_s(szInstance, sizeof(szInstance) * sizeof(char), pProcessName);

		// get the CPU usage counter value
		lnNewValue = PerfCounters.GetCounterValue(apPerfData, dwObjectIndex, 
			dwCpuUsageIndex, szInstance);
	}
	else
	{
		// get the CPU usage counter value
		lnNewValue = PerfCounters.GetCounterValueForProcessID(apPerfData, dwObjectIndex, 
			dwCpuUsageIndex, dwProcessID);
	}

	if (lnNewValue == -1)
	{		
		// If the process is still alive, it indicates in inability to read the performance data
		// counters for the process. Return -1 to indicate failure.
		if (isProcessAlive(dwProcessID))
		{
			return -1;
		}
		else
		{
			// If the process has has exited, zero is a legitimate CPU usage to report.
			lnNewValue = 0;
		}
	}

	NewPerfTime100nSec = apPerfData->PerfTime100nSec;

	// if this is the first iteration, just store the values and return 0
	if (m_bFirstTime)
	{
		m_bFirstTime = false;
		m_lnOldValue = lnNewValue;
		m_OldPerfTime100nSec = NewPerfTime100nSec;
		return 0;
	}

	// compute the change in the counter value
	LONGLONG lnValueDelta = lnNewValue - m_lnOldValue;
	double DeltaPerfTime100nSec = 
		(double)NewPerfTime100nSec.QuadPart - (double)m_OldPerfTime100nSec.QuadPart;

	// update the stored values
	m_lnOldValue = lnNewValue;
	m_OldPerfTime100nSec = NewPerfTime100nSec;

	// Compute the percentage of CPU time
	double a = (double)lnValueDelta / DeltaPerfTime100nSec;

	// if computing the total CPU usage then need to subtract decimal value from 1.0.
	// see comments for GetCpuUsage() for formula.
	if (bTotal)
	{
		a = (1.0 - a);
	}

	// convert the decimal to a percentage and round it
	CpuUsage = (int) ((a*100.0) + 0.5);

	// ensure positive value
	if (CpuUsage < 0)
	{
		return 0;
	}

	// return the cpu usage
	return CpuUsage;
}
//--------------------------------------------------------------------------------------------------
