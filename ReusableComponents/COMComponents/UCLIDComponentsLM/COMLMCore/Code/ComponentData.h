//============================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ComponentData.h
//
// PURPOSE:	Definition of the LicenseManagement data class ComponentData
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//============================================================================

#pragma once

#include "stdafx.h"
#include "COMLMCore.h"

#include <ByteStreamManipulator.h>

class ComponentData
{
public:
	
	//=======================================================================
	// PURPOSE: Constructor for Component Data object.  A UCLID component 
	//				will have a unique UCLID-defined ID.  This object will 
	//				store a flag to define whether this use of the component 
	//				is licensed forever and an expiration date if that is 
	//				appropriate for the component.
	// REQUIRE: Nothing
	// PROMISE: The default object is not licensed and has a default 
	//				expiration date.
	// ARGS:	None
	ComponentData();

	//=======================================================================
	// PURPOSE: Constructor for Component Data object.  A UCLID component 
	//				will have a unique UCLID-defined ID.  This object will 
	//				store a flag to define whether this use of the component 
	//				is licensed forever, and an expiration date if that is 
	//				appropriate for the component.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	bIsLicensed - true if licensed forever
	//				ExpirationDate - date that component expires if not 
	//				licensed forever
	ComponentData(bool bIsLicensed, const CTime& ExpirationDate);

	//=======================================================================
	// PURPOSE: Constructor for Component Data object.  A UCLID component 
	//				will have a unique UCLID-defined ID.  This object will 
	//				store a flag to define whether this use of the component 
	//				is licensed forever, and an expiration date if that is 
	//				appropriate for the component.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	Data - the object to be copied
	ComponentData(const ComponentData& Data);

	//=======================================================================
	// PURPOSE: Destructor for object
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	None
	~ComponentData();

	//=======================================================================
	// PURPOSE: Checks to see if this component has expired.  Tests current
	//				system time against the expiration date member.
	// REQUIRE: Nothing
	// PROMISE: Returns true if the current system time is after the 
	//				expiration date AND the component is not licensed, false 
	//				otherwise.
	// ARGS:	None
	bool isExpired();

	//=======================================================================
	// PURPOSE: Checks if this component is permanently licensed.
	// REQUIRE: Nothing
	// PROMISE: Returns true if this component is permanently licensed,
	//			false otherwise.
	// ARGS:	None
	bool isPermanent() { return m_bIsLicensed; }

	//=======================================================================
	// PURPOSE: Assignment operator for ComponentData object.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	Data - the object to be copied
    ComponentData& operator = (const ComponentData& Data);

	//=======================================================================
	// PURPOSE: Provides a mechanism for writing a ComponentData object to a 
	//				ByteStreamManipulator.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	bsm - reference to the manipulator that receives the data
	//				Data - the component data object to be written
	friend ByteStreamManipulator& operator << (ByteStreamManipulator& bsm, 
		ComponentData Data);

	//=======================================================================
	// PURPOSE: Provides a mechanism for reading a ComponentData object from 
	//				a ByteStreamManipulator.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	bsm - reference to the manipulator that provides the data
	//				Data - the component data object to be read
	friend ByteStreamManipulator& operator >> (ByteStreamManipulator& bsm, 
		ComponentData& rData);

///////////////
// DATA MEMBERS
///////////////

	// License state: true if licensed forever, false if expires after a 
	// particular date
	bool	m_bIsLicensed;

	// Whether this license ID has been disabled
	// used for testing purposes to allow turning off
	// the licensing of a specific component
	// Added as per [LegacyRCAndUtils #4993]
	bool	m_bDisabled;
	
	// Date that license expires if not licensed forever
	CTime	m_ExpirationDate;
};
