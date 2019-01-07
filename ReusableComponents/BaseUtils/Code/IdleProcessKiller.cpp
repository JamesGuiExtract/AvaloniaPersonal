#include "stdafx.h"
#include "IdleProcessKiller.h"
#include "UCLIDException.h"
#include "cpputil.h"

#include <Psapi.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const string gstrTIME_OUT_VALUE_NAME = "IdleProcessTimeout";
const string gstrDEFAULT_TIME_OUT = "120000";
const string gstrINTERVAL_VALUE_NAME = "IdleProcessInterval";
const string gstrDEFAULT_INTERVAL = "2000";
const string gstrREG_KEY = "SOFTWARE\\Extract Systems\\ReusableComponents\\BaseUtils";

//-------------------------------------------------------------------------------------------------
// Static members
//-------------------------------------------------------------------------------------------------
bool IdleProcessKiller::ms_bLoggedCpuInfoError = false;
CMutex IdleProcessKiller::ms_Mutex;

//-------------------------------------------------------------------------------------------------
IdleProcessKiller::IdleProcessKiller(unsigned long ulProcessId,
									 int iTimeOut/* = 0*/, int iInterval/* = 0*/)
	: m_ulProcessId(ulProcessId),
	  m_iInterval(iInterval),
	  m_iTimeOut(iTimeOut),
	  m_iZeroCpuCount(0),
	  m_iMaxZeroCpuCount(0),
	  m_bKilledProcess(false),
	  m_nRetryCount(0),
	  m_registryManager(HKEY_LOCAL_MACHINE, gstrREG_KEY)
{
	try
	{
		if (m_iTimeOut == 0)
		{
			string strTimeOut =
				m_registryManager.getKeyValue("", gstrTIME_OUT_VALUE_NAME, gstrDEFAULT_TIME_OUT);
			if (strTimeOut.empty())
			{
				strTimeOut = gstrDEFAULT_TIME_OUT;
			}

			m_iTimeOut = asLong(strTimeOut);
		}

		if (m_iInterval == 0)
		{
			string strInterval =
				m_registryManager.getKeyValue("", gstrINTERVAL_VALUE_NAME, gstrDEFAULT_INTERVAL);
			if (strInterval.empty())
			{
				strInterval = gstrDEFAULT_INTERVAL;
			}

			m_iInterval = asLong(strInterval);
		}
			
		ASSERT_ARGUMENT("ELI25213", m_iInterval > 0);
		ASSERT_ARGUMENT("ELI25207", m_iTimeOut >= m_iInterval);

		// Round up to the nearest integer
		m_iMaxZeroCpuCount = (m_iTimeOut + m_iInterval - 1) / m_iInterval;

		// Create a thread to monitor the process
		if( !AfxBeginThread(monitorProcessLoop, this) )
		{
			UCLIDException ue("ELI25208", "Unable to initialize idle process monitoring thread.");

			// Add error info to the exception
			char errmsg[80]; 
			strerror_s(errmsg, 80, errno);
			ue.addDebugInfo("ErrorCode", errno);
			ue.addDebugInfo("Error", errmsg);

			throw ue;
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

	return 0;
}
//-------------------------------------------------------------------------------------------------
void IdleProcessKiller::monitorProcess()
{
	INIT_EXCEPTION_AND_TRACING("MLI03274");
	try
	{
		_lastCodePos = "1";
		// Check if there is zero cpu usage
		int nCpuUsage = m_cpuUsage.GetCpuUsage(m_ulProcessId);

		_lastCodePos = "5";

		// -1 indicates that GetCpuUsage failed to obtain data, likely either due to lack of
		// necessary permissions or the "Disable Performance Counters" registry values being
		// set.
		if (nCpuUsage < 0)
		{
			// Periodically there can be failures to read CPU usage that are not indicative of
			// a permanent issue, especially as a process is starting. Make 3 attempts before
			// determining that the CPU usage cannot be read.
			if (m_nRetryCount < 3)
			{
				m_nRetryCount++;
				return;
			}

			if (!ms_bLoggedCpuInfoError)
			{
				CSingleLock lg(&ms_Mutex, TRUE);
				// Recheck ms_bLoggedCpuInfoError after getting the lock.
				if (!ms_bLoggedCpuInfoError)
				{
					UCLIDException("ELI36742", "Application trace: Unable to obatin CPU usage data; "
						"hung processes will not be detected.").log();
					ms_bLoggedCpuInfoError = true;
				}
			}

			// No need to let monitorProcessLoop continue if we can't read the CPU usage.
			m_eventStopping.signal();
			return;
		}
		
		// We've succeeded in reading the CPU usage; reset the retry count.
		m_nRetryCount = 0;

		_lastCodePos = "8";

		if (nCpuUsage > 0)
		{
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

