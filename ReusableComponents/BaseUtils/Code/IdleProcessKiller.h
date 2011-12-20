#pragma once

#include "CpuUsage.h"
#include "Win32Event.h"

#include <string>

using namespace std;

class EXPORT_BaseUtils IdleProcessKiller
{
public:

	IdleProcessKiller(unsigned long ulProcessId, int iTimeOut=120000, int iInterval=2000);
	~IdleProcessKiller();

	// Returns true if the idle process killer killed the process and false otherwise
	bool killedProcess() { return m_bKilledProcess; }

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
};
