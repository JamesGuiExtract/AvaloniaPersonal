
#include "stdafx.h"
#include "Win32Event.h"
#include "UCLIDException.h"

//-------------------------------------------------------------------------------------------------
Win32Event::Win32Event(bool bManualReset)
{
	m_hEvent = CreateEvent(NULL, bManualReset ? TRUE : FALSE, FALSE, NULL);
	if (m_hEvent == NULL)
	{
		throw UCLIDException("ELI09128", "Unable to create Event object!");
	}
}
//-------------------------------------------------------------------------------------------------
Win32Event::Win32Event(const Win32Event& event)
: m_hEvent(NULL)
{
	*this = event;
}
//-------------------------------------------------------------------------------------------------
Win32Event::~Win32Event()
{
	try
	{
		if (m_hEvent != __nullptr && CloseHandle(m_hEvent) == FALSE)
		{
			UCLIDException ue("ELI09130", "Unable to close event handle!");
			ue.addDebugInfo("Handle", (unsigned long) m_hEvent);
			throw ue;
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16416");
}
//-------------------------------------------------------------------------------------------------
void Win32Event::signal()
{
	if (SetEvent(m_hEvent) == NULL)
	{
		UCLIDException ue("ELI09131", "Unable to signal event!");
		ue.addDebugInfo("Handle", (unsigned long) m_hEvent);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
bool Win32Event::isSignaled() const
{
	return WaitForSingleObject(m_hEvent, 0 ) == WAIT_OBJECT_0;
}
//-------------------------------------------------------------------------------------------------
DWORD Win32Event::wait(DWORD dwMilliseconds)
{
	// wait for the event
	DWORD result = WaitForSingleObject(m_hEvent, dwMilliseconds);

	return result;
}
//-------------------------------------------------------------------------------------------------
DWORD Win32Event::messageWait()
{
	// collection of one handle to be waited on
	HANDLE hObjects[1];  
	hObjects[0] = m_hEvent;
	
	// number of handles to wait on
	int cObjects = 1;     

	// Return value from MsgWaitForMultipleObjects
	DWORD dwResult;

	// The message loop lasts until we get a WM_QUIT message,
    // after which we shall return from the function.
    do
    {
        // block-local variable 
        MSG msg; 

        // Read all of the messages in this next loop, 
        // removing each message as we read it.
        while (PeekMessage(&msg, NULL, 0, 0, PM_REMOVE)) 
        { 
            // dispatch the message.
            DispatchMessage(&msg); 
        } // End of PeekMessage while loop.

		dwResult = MsgWaitForMultipleObjects(cObjects, hObjects, 
                 FALSE, INFINITE, QS_ALLINPUT);

	} while (dwResult != WAIT_OBJECT_0 );

	return dwResult;
}
//-------------------------------------------------------------------------------------------------
void Win32Event::reset()
{
	if (ResetEvent(m_hEvent) == NULL)
	{
		UCLIDException ue("ELI09129", "Unable to reset event!");
		ue.addDebugInfo("Handle", (unsigned long) m_hEvent);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
HANDLE Win32Event::getHandle()
{
	return m_hEvent;
}
//-------------------------------------------------------------------------------------------------
Win32Event& Win32Event::operator=(const Win32Event& event)
{
	// [LegacyRCAndUtils:5257] Close the existing handle before duplicating the source handle.
	if (m_hEvent != __nullptr && CloseHandle(m_hEvent) == FALSE)
	{
		UCLIDException ue("ELI25340", "Unable to close event handle!");
		ue.addDebugInfo("Handle", (unsigned long) m_hEvent);
		ue.log();
	}

	HANDLE hProcess = GetCurrentProcess();
	DuplicateHandle(hProcess, event.m_hEvent, hProcess, &m_hEvent, 0, true, DUPLICATE_SAME_ACCESS);

	return *this;
}
//-------------------------------------------------------------------------------------------------