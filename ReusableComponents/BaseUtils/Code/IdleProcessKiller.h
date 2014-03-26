#pragma once

#include "CpuUsage.h"
#include "Win32Event.h"
#include "RegistryPersistenceMgr.h"

#include <string>
#include <afxmt.h>

using namespace std;

class EXPORT_BaseUtils IdleProcessKiller
{
public:

	IdleProcessKiller(unsigned long ulProcessId, int iTimeOut = 0, int iInterval = 0);
	~IdleProcessKiller();

	// Returns true if the idle process killer killed the process and false otherwise
	bool killedProcess() { return m_bKilledProcess; }
	int getInterval() { return m_iInterval; }
	int getTimeOut() { return m_iTimeOut; }

private:

	//---------------------------------------------------------------------------------------------
	// Methods
	//---------------------------------------------------------------------------------------------

	// Monitors the status of the process and kills the process if it is idle
	static UINT monitorProcessLoop(void* pData);
	void monitorProcess();

	// Get the full path of the specified process
	string getProcessFileName(HANDLE hProcess);

	//---------------------------------------------------------------------------------------------
	// Data
	//---------------------------------------------------------------------------------------------

	// The process id of the process being monitored
	unsigned long m_ulProcessId;

	// The number of milliseconds elapsed between checking the cpu usage
	int m_iInterval;

	// The number of milliseconds of zero CPU usage that should trigger the process to be killed.
	int m_iTimeOut;

	// The number of consecutive times the process has been at zero cpu usage
	int m_iZeroCpuCount;

	// The number of consecutive zero cpu usage checks before a process is considered idle
	int m_iMaxZeroCpuCount;

	// Used to compute the cpu usage of the monitored process
	CCpuUsage m_cpuUsage;

	// Signaled when the thread should stop processing
	Win32Event m_eventStopping;

	// Signaled when the thread has stopped processing
	Win32Event m_eventStopped;

	bool m_bKilledProcess;

	// Used to retrieve registry-defined interval and time-out values.
	RegistryPersistenceMgr m_registryManager;

	// Number of retry attempts that have been made to read the CPU usage for the process. Three
	// attempts will be allowed.
	int m_nRetryCount;

	// Indicates whether a log message has been written indicating idle processes will not be able
	// to be detected/killed.
	static bool ms_bLoggedCpuInfoError;

	// Protects access to ms_bLoggedCpuInfoError
	static CMutex ms_Mutex;
};
