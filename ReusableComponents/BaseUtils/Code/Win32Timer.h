//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	Win32Timer.h
//
// PURPOSE:	Win32Timer is responsible for notifying registered Observers when the timer expires
//			that the specified event has occured. 
//
// NOTES:		
//
// AUTHORS:	John Hurd
//
//==================================================================================================

#pragma once

#include "BaseUtils.h"
#include "EventID.h"
#include "ObservableSubject.h"

#include <map>

//==================================================================================================
//
// CLASS:	Win32Timer
//
// PURPOSE:	Win32Timer is responsible for notifying registered Observers when the timer expires
//			that the specified event has occured. Win32Timer is an ObservableSubject in the Observer
//			pattern.
//
// REQUIRE:	Clients can specify the type of EventID that they want to be notified about when creating
//			an instance of the Win32Timer class.  Clients should call Start() to begin the timer 
//			countdown.  Clients must call Stop() before reusing a Win32Timer object.  If clients
//			have a requirement to refresh the timer before it has expired, then they should call
//			Reset().  Clients can decide whether or not a timer has expired by calling GetActive().
// 
// INVARIANTS:	
//
// EXTENSIONS:
//
// NOTES:	
//
//==================================================================================================
class EXPORT_BaseUtils Win32Timer : public ObservableSubject  
{
public:
	static EventID TIMER_EVENT;		// default EventID for event notification when timer expires

	//----------------------------------------------------------------------------------------------
	// PURPOSE: Default constructor takes a default argument, Win32Timer::TIMER_EVENT
	//
	// REQUIRE: 
	//
	// PROMISE: 
	//
	// ARGS:	
	//			rEventID: identifies the type of EventID used to notify the client
	//
	Win32Timer(const EventID& rEventID=TIMER_EVENT);

	//----------------------------------------------------------------------------------------------
	// PURPOSE: Destructor
	//
	// REQUIRE: 
	//
	// PROMISE:	Stop() will be called if the timer is active. 
	//
	// ARGS:	
	//
	virtual ~Win32Timer();

	//----------------------------------------------------------------------------------------------
	// PURPOSE: Start the timer countdown.
	//
	// REQUIRE:	Clients are responsible for calling Stop() after starting the timer. 
	//
	// PROMISE:	Returns success.  The client will be notified when the timer countdown expires. 
	//
	// ARGS:	
	//			nMilliseconds: number of milliseconds before timer expires; nMilliseconds >=0
	//
	bool Start(int nMilliseconds);

	//----------------------------------------------------------------------------------------------
	// PURPOSE: Stop the timer countdown.  Clients are not notified of the event.
	//
	// REQUIRE:	Clients should have first called Start() but it is OK if they don't. 
	//
	// PROMISE:	Returns success.
	//
	// ARGS:	
	//			nMilliseconds: number of milliseconds before timer expires; nMilliseconds >=0
	//
	bool Stop(void);

	//----------------------------------------------------------------------------------------------
	// PURPOSE: Query the EventID used to notify any Observers when the timer expires.
	//
	// REQUIRE:	
	//
	// PROMISE:	Returns the EventID used to notify any Observers when the timer expires.
	//
	// ARGS:	
	//
	const EventID& GetEventID(void) const {return m_eventID;}

	//----------------------------------------------------------------------------------------------
	// PURPOSE: Query whether the timer has expired or is still running.
	//
	// REQUIRE:	
	//
	// PROMISE:	Returns whether the timer has expired or not.
	//
	// ARGS:	
	//
	bool Expired(void) const {return (m_nTimerID == 0);}

private:
	//----------------------------------------------------------------------------------------------
	// PURPOSE: Callback routine used by the Win32 system timer
	//
	// REQUIRE:	
	//
	// PROMISE:	
	//
	// ARGS:	
	//		hwnd:		[in] Handle to the window associated with the timer. 
	//		uMsg:		[in] Specifies the WM_TIMER message. 
	//		idEvent:	[in] Specifies the timer's identifier. 
	//		dwTime:		[in] Specifies the number of milliseconds that have elapsed since the system was started. 
	//				This is the value returned by the GetTickCount function
	static void CALLBACK TimerProc(HWND hwnd, UINT uMsg, UINT idEvent, DWORD dwTime); 

	EventID m_eventID;		// identifies the type of event to trigger when the timer expires
	UINT m_nTimerID;		// identifies the Win32 timer system resource
	static std::map<UINT,Win32Timer*> m_mapTimers;	// collection of Win32Timer objects identified by an idEvent
};