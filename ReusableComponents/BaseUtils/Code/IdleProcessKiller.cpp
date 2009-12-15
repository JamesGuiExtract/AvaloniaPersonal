#include "stdafx.h"
#include "IdleProcessKiller.h"
#include "UCLIDException.h"

#include <Psapi.h>

//-------------------------------------------------------------------------------------------------
IdleProcessKiller::IdleProcessKiller(unsigned long ulProcessId, int iTimeOut, int iInterval)
	: m_ulProcessId(ulProcessId),
	  m_iInterval(iInterval),
	  m_iZeroCpuCount(0),
	  m_iMaxZeroCpuCount(0),
	  m_bKilledProcess(false)
{
	try
	{
		// Ensure the arguments are valid
		ASSERT_ARGUMENT("ELI25213", iInterval > 0);
		ASSERT_ARGUMENT("ELI25207", iTimeOut >= iInterval);

		// Round up to the nearest integer
		m_iMaxZeroCpuCount = (iTimeOut + iInterval - 1) / iInterval;

		// Create a thread to monitor the process
		if( !AfxBeginThread(monitorProcessLoop, this) )
		{
			throw UCLIDException("ELI25208", "Unable to initialize idle process monitoring thread.");
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25215")
}
//-------------------------------------------------------------------------------------------------
IdleProcessKiller::~IdleProcessKiller()
{
	try
	{
		// Signal the monitor thread to stop
		m_eventStopping.signal();

		// Wait for the monitor thread to stop
		m_eventStopped.wait();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI25214")
}
//-------------------------------------------------------------------------------------------------
UINT IdleProcessKiller::monitorProcessLoop(void* pData)
{
	try
	{
		IdleProcessKiller* pIdleProcessKiller = (IdleProcessKiller*) pData;
		ASSERT_RESOURCE_ALLOCATION("ELI25209", pIdleProcessKiller != NULL);

		try
		{
			// Wait the specified interval for the stop event
			while (pIdleProcessKiller->m_eventStopping.wait(pIdleProcessKiller->m_iInterval) == WAIT_TIMEOUT)
			{
				// We haven't stopped yet, monitor the process
				pIdleProcessKiller->monitorProcess();
			}
		}
		CATCH_AND_LOG_ALL_EXCEPTIONS("ELI25210");

	
		// Signal that this thread has stopped
		pIdleProcessKiller->m_eventStopped.signal();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI25212");

	return 0;
}
//-------------------------------------------------------------------------------------------------
void IdleProcessKiller::monitorProcess()
{
	// Check if there is zero cpu usage
	if (m_cpuUsage.GetCpuUsage(m_ulProcessId) > 0)
	{
		// Process isn't idle. Reset the zero cpu count.
		m_iZeroCpuCount = 0;

		return;
	}

	// Process may be idle. Increment count.
	m_iZeroCpuCount++;

	// Check if this the process is considered idle
	if (m_iZeroCpuCount >= m_iMaxZeroCpuCount)
	{
		// Kill the process
		HANDLE hProcess = OpenProcess(PROCESS_ALL_ACCESS, FALSE, m_ulProcessId);
		if (hProcess != NULL)
		{
			// Get the name of the file that is being terminated
			string strFileName = getProcessFileName(hProcess);

			// Terminate the process
			TerminateProcess(hProcess, 0);
			CloseHandle(hProcess);
			
			m_bKilledProcess = true;

			// Log an application trace
			UCLIDException ue("ELI25219", "Application trace: Idle process terminated.");
			ue.addDebugInfo("Process", strFileName);
			ue.log();
		}

		// Stop monitoring the process
		m_eventStopping.signal();
	}
}
//-------------------------------------------------------------------------------------------------
string IdleProcessKiller::getProcessFileName(HANDLE hProcess)
{
	char buffer[MAX_PATH];
	if (GetModuleFileNameEx(hProcess, 0, buffer, MAX_PATH) != 0)
	{
		// Return just the file name
		return getFileNameFromFullPath(buffer);
	}
	else
	{
		// Could not get file name, return process id instead
		return "Process #" + asString(m_ulProcessId);
	}
}
//-------------------------------------------------------------------------------------------------

