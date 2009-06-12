//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ValueRestorer.h
//
// PURPOSE:	Implementation of a class which can be used to restore the value of any object
//			or primitive type to a certain value at the time of destruction
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan
//
//==================================================================================================
#pragma once

template <class T>
class ValueRestorer
{
public:
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To restore the value of an object.
	// REQUIRE: Class T must have a proper assignment operator and copy constructor defined.
	// PROMISE: When this object is deleted, {rObject}'s value will be restored to the value that was
	//			associated with {rObject} when this object was created.
	ValueRestorer(T& rObject)
	: m_rObject(rObject), m_restoreValue(rObject)
	{
	}
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To restore the value of an object.
	// REQUIRE: Class T must have a proper assignment operator and copy constructor defined.
	// PROMISE: When this object is deleted, {rObject}'s value will be restored to {restoreValue}.
	ValueRestorer(T& rObject, const T& restoreValue)
	: m_rObject(rObject), m_restoreValue(restoreValue)
	{
	}
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To restore the value of the specified object to the specified value.
	~ValueRestorer()
	{
		m_rObject = m_restoreValue;
	}
	//----------------------------------------------------------------------------------------------

protected:
	T& m_rObject;		// the object whose value will be restored
	T m_restoreValue;	// the value that will be restored to m_rObject
};


