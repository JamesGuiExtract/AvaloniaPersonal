//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	Singleton.h
//
// PURPOSE:	Implementation of the Singleton template class
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan
//
//==================================================================================================
#pragma once

#include <memory>

// PURPOSE: The purpose of this template class is to create and enforce (to a small degree) the
// Singleton pattern.
// REQUIRE: Derived class must provide a default constructor.
// NOTES: To make any class into a Singleton class, just derive from this template as shown in
// the sample usage below.  The singleton class will only be created when Instance() is called.
// If an instance of the Singleton class was created, then it will be destroyed when the
// application exits.
template <class T>
class Singleton
{   
public:
	// PURPOSE: to allow proper destruction of the derived class
	virtual ~Singleton() {}

	static inline T& sGetInstance()
    {
		return *sGetOrDeleteInstance();
    }

	static inline T* sGetInstancePtr()
    {
		return sGetOrDeleteInstance();
    }

	static inline void sDeleteInstance()
    {
		sGetOrDeleteInstance(false);
    }

private:
	// NOTE: we're not making pInstance into a member variable
	// because then the member variable has to be defined somewhere
	// (in a .cpp file usually)...and this has created problems with
	// exporting symbols from DLLS and correctly importing them etc.
	// Having the static variable be local to this method, and allowing
	// this method to both create and delete the static variable gives
	// us all the functionality we need to implement the above
	// public static methods.
	static T* sGetOrDeleteInstance(bool bGet = true)
	{
		// This is an unique_ptr so that it will always be deleted
		static std::unique_ptr<T> sapInstance(__nullptr);

		if (sapInstance.get() == NULL && bGet)
		{
			sapInstance = std::unique_ptr<T>(new T());
		}
		else if (sapInstance.get() && !bGet)
		{
			// This will delete the existing pointer and set instance to NULL
			// Calling sDeleteInstance after using the pointer is not required unless
			// a COM object pointer is saved in the derived class in which case 
			// the object will need to deleted before the call to CoUninitialize
			sapInstance.reset(__nullptr);
		}

		return sapInstance.get();
	}
};

// PURPOSE: This macro defines the Singleton template as a friend class so that
// the protected constructor (and destructor if applicable) can be accessed
// by the Instance() method of the Singleton template class.  It is up to the
// user of the Singleton template class to make sure that their class does not
// expose public constructors, etc.
#define ALLOW_SINGLETON_ACCESS(SomeClass) \
		friend class Singleton<##SomeClass>; \
		friend class std::unique_ptr<##SomeClass>;

