
#pragma once

#include "BaseUtils.h"

class EXPORT_BaseUtils Win32Event
{
public:
	// create and destroy this event class
	// This class wraps the Win32 Events
	// NOTE: This wrapper class creates manual-reset events, 
	// (which means that a signaled event stays in that state until
	// reset() is called), or auto-reset events (which means
	// that a signaled event returns to a non-signaled state as soon
	// as one of the waiting threads have been released).
	Win32Event(bool bManualReset = true);
	Win32Event(const Win32Event& event);
	~Win32Event();

	Win32Event& operator=(const Win32Event& event);

	// set the state of this event to "signaled"
	void signal();

	// returns true if the event is currently in a signaled state.
	// NOTE: This method should generally not be called for
	// auto-reset events, because in auto-reset events, the 
	// event usually stays in a signaled state for a very short
	// duration.
	bool isSignaled() const;

	// wait for the state of this event to become "signaled".
	// For documentation of the return code, see the return code
	// documentation of WaitForSingleObject()
	DWORD wait(DWORD dwMilliseconds = INFINITE);

	// Dispatches windows message while waiting for an object.
	// TODO, add a param as the waiting time so it will return
	// the result from MsgWaitForMultipleObjects
	DWORD messageWait();

	// reset the state of this event to "unsignaled"
	void reset();

	// returns the handle of this event
	HANDLE getHandle();

private:
	HANDLE m_hEvent;
//	bool m_bIsSignaled;
//	bool m_bManualReset;
};
