#include "stdafx.h"
#include "IdleProcessKiller.h"
#include "UCLIDException.h"

#include <Psapi.h>

//-------------------------------------------------------------------------------------------------
IdleProcessKiller::IdleProcessKiller(unsigned long ulProcessId, int iTimeOut, int iInterval)
	: m_ulProcessId(ulProcessId),
	  m_iInterval(iInterval),
	  m_iZeroCpuCount(0),
	  m_iLowCpuCount(0),
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
		ASSERT_RESOURCE_ALLOCATION("ELI25209", pIdleProcessKiller != __nullptr);

		INIT_EXCEPTION_AND_TRACING("MLI03273");
		try
		{
			_lastCodePos = "10";

			// Wait the specified interval for the stop event
			while (pIdleProcessKiller->m_eventStopping.wait(pIdleProcessKiller->m_iInterval) == WAIT_TIMEOUT)
			{
				_lastCodePos = "20";
				// We haven't stopped yet, monitor the process
				pIdleProcessKiller->monitorProcess();
				_lastCodePos = "30";
			}
		}
		CATCH_AND_LOG_ALL_EXCEPTIONS("ELI25210");

	
		// Signal that this thread has stopped
		pIdleProcessKiller->m_eventStopped.signal();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI25212");

	// Temporary code to investigate LegacyRCAndUtils:5945
	UCLIDException("ELI34104", "Idle process killer monitoring thread exited.").log();

	return 0;
}
//-------------------------------------------------------------------------------------------------
void IdleProcessKiller::monitorProcess()
{
	INIT_EXCEPTION_AND_TRACING("MLI03274");
	try
	{
		int nCpuUsage = m_cpuUsage.GetCpuUsage(m_ulProcessId);

		// Temporary code to investigate LegacyRCAndUtils:5945
		if (nCpuUsage > 2)
		{
			m_iLowCpuCount = 0;
		}
		else
		{
			m_iLowCpuCount++;

			if (m_iLowCpuCount == m_iMaxZeroCpuCount)
			{
				UCLIDException("ELI34102", "Low (but not zero CPU usage) for an extended period.").log();
			}
		}

		// Check if there is zero cpu usage
		if (nCpuUsage > 0)
		{
			// Temporary code to investigate LegacyRCAndUtils:5945
			if (m_iZeroCpuCount > 3) 
			{
				UCLIDException ue("ELI34103", "CPU usage had been (but is no longer) zero");
				ue.addDebugInfo("Time at zero", m_iZeroCpuCount);
				ue.addDebugInfo("Current usage", nCpuUsage);
				ue.log();
			}

			// Process isn't idle. Reset the zero cpu count.
			m_iZeroCpuCount = 0;

			return;
		}
		_lastCodePos = "10";

		// Process may be idle. Increment count.
		m_iZeroCpuCount++;

		// Check if this the process is considered idle
		if (m_iZeroCpuCount >= m_iMaxZeroCpuCount)
		{
			// Kill the process
			HANDLE hProcess = OpenProcess(PROCESS_ALL_ACCESS, FALSE, m_ulProcessId);
			_lastCodePos = "20";
			if (hProcess != NULL)
			{
				// Get the name of the file that is being terminated
				string strFileName = getProcessFileName(hProcess);
				_lastCodePos = "30";

				// Terminate the process
				TerminateProcess(hProcess, 0);
				_lastCodePos = "40";

				CloseHandle(hProcess);
				_lastCodePos = "50";

				m_bKilledProcess = true;

				// Log an application trace
				UCLIDException ue("ELI29833", "Application trace: Idle process terminated.");
				ue.addDebugInfo("Process", strFileName);
				ue.addDebugInfo("Process ID", m_ulProcessId);
				ue.log();
				_lastCodePos = "60";
			}
			else
			{
				// Temporary code to investigate LegacyRCAndUtils:5945
				UCLIDException("ELI34103", "Application trace: Could not get handle to close process.").log();
			}

			// Stop monitoring the process
			m_eventStopping.signal();
			_lastCodePos = "70";
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29834");
}
//-------------------------------------------------------------------------------------------------
string IdleProcessKiller::getProcessFileName(HANDLE hProcess)
{
	char buffer[MAX_PATH] = {0};
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

