//==================================================================================================
//
// COPYRIGHT (c) 2006 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	TSSingleton.h
//
// PURPOSE:	Implementation of the thread-safe Singleton template class
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius, extending original code from Arvind Ganesan
//
//==================================================================================================
#pragma once

#include "Win32CriticalSection.h"

#include <assert.h>

// PURPOSE: The purpose of this template class is to create and enforce (to a small degree) the
// Singleton pattern.
// REQUIRE: Derived class must provide a default constructor.
// NOTES: To make any class into a Singleton class, just derive from this template as shown in
// the sample usage below.  The singleton class will only be created when Instance() is called.
// If an instance of the Singleton class was created, then it will be destroyed when the
// application exits.
template <class T>
class TSSingleton
{   
public:
	static inline T* sGetInstance()
    {
		// We can do our first test without concern for locking.
		if (ms_apInstance == NULL)
		{
			// Doesn't exist yet. Acquire lock before proceeding.
			Win32CriticalSectionLockGuard lg( ms_cs ); 

			// We have the lock, but another thread may have gotten
			// the lock first. See if it's still null before creating.
			if (ms_apInstance == NULL)
			{
				ms_apInstance = new T();
			}
		}

		return ms_apInstance;
    }

	static inline void sDeleteInstance()
    {
		if (ms_apInstance != __nullptr)
		{
			// This will delete the existing pointer and set instance to NULL
			// Calling sDeleteInstance after using the pointer is not required unless
			// a COM object pointer is saved in the derived class in which case 
			// the object will need to deleted before the call to CoUninitialize
			delete ms_apInstance;
		}
    }

protected:
	TSSingleton() {}
	virtual ~TSSingleton() {}

private:

	static Win32CriticalSection ms_cs;

	static T* ms_apInstance;
};

template <class T>
Win32CriticalSection TSSingleton<T>::ms_cs;

template <class T>
T* TSSingleton<T>::ms_apInstance = NULL;

// PURPOSE: This macro defines the TSSingleton template as a friend class so that
// the protected constructor (and destructor if applicable) can be accessed
// by the Instance() method of the TSSingleton template class.  It is up to the
// user of the TSSingleton template class to make sure that their class does not
// expose public constructors, etc.
#define ALLOW_TSSINGLETON_ACCESS(SomeClass) \
		friend class TSSingleton<##SomeClass>;
