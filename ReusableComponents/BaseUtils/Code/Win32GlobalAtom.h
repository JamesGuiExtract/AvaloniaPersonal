#pragma once

#include "BaseUtils.h"
#include <string>

class EXPORT_BaseUtils Win32GlobalAtom
{
public:
	//---------------------------------------------------------------------------------------------
	// PURPOSE: create a new unattached Win32GlobalAtom
	// REQUIRE: none
	// PROMISE: This atom is unattached
	Win32GlobalAtom();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: create a new Win32GlobalAtom and attach it 
	//			to atom
	// REQUIRE: none
	// PROMISE: the reference count on atom will
	//			not be incremented
	Win32GlobalAtom(ATOM atom);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: create a new Win32GlobalAtom
	// REQUIRE: none
	// PROMISE: If an ATOM already exists with strAtomName
	//			its reference count will increment and it will 
	//			be attached to this Win32GlobalAtom.
	//			If there is not currently an ATOM for strAtomName
	//			one will be created and its reference count set to
	//			one
	Win32GlobalAtom(const std::string& strAtomName);	
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To release the ATOM currently attached to
	//			this Win32GlobalAtom
	// REQUIRE: The ATOM to which this object is currently attached must 
	//			be valid and have a reference count > 0
	// PROMISE: To decrement by 1 the reference count to the attached 
	//			ATOM
	~Win32GlobalAtom();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To attach a new ATOM
	// REQUIRE: If an ATOM is currently attached to this object at the time this
	//			method is called it must be valid and have a reference 
	//			count > 0
	// PROMISE: If there is already an ATOM attached to this object its
	//			reference count will be decremented by one
	//			The reference count of the new ATOM, atom will not be incremented
	void attach(ATOM atom);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To detach the currently attached atom
	// REQUIRE: This object must be currently attached to an ATOM
	// PROMISE: The reference count of the currently attached atom will
	//			not be decremented
	//			Will return the newly detached ATOM
	ATOM detach();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To retrieve the currently attached atom handle
	// REQUIRE: The return Atom must not be deleted outside the scope of
	//			this object.
	// PROMISE: Will return the currently attached ATOM handle, which may be
	//			NULL if no ATOM is currently attached to this object.
	ATOM getWin32Atom() const;
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To release the currently attached atom
	// REQUIRE: This object must be currently attached to an ATOM and that 
	//			ATOM must be valid and have a reference count > 0
	// PROMISE: The current ATOM will be detached and its reference 
	//			count will be decremented by 1
	void release();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To return the name of the currently attached ATOM
	// REQUIRE: This object must be currently attached to an ATOM
	// PROMISE: 
	const std::string getName() const;
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To associate a new name with this object
	// REQUIRE: Nothing
	// PROMISE: The currently attached atom, if any, will be detached, and a 
	//			new atom will be attached to this object with the given name.
	const void setName(const std::string& strAtomName);
	//---------------------------------------------------------------------------------------------

private:
	ATOM m_atom;
};