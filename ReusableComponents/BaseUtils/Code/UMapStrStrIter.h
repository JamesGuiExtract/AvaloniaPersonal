//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	UMapStrStrIter.h
//
// NOTES:		
//
// AUTHORS:	John Hurd
//
//==================================================================================================

#pragma once

#include "BaseUtils.h"

#include <string>
#include <map>

class EXPORT_BaseUtils UMapStringString;

//==================================================================================================
//
// CLASS:	UMapStrStrIter
//
// PURPOSE:	UMapStrStrIter is responsible for iterating through a UMapStringString collection.
//			The UMapStrStrIter encapsulates an stl map<string,string>::iterator but can safely be
//			used across module boundaries (i.e. from one DLL to another) unlike the stl map.
//			For example, a client in Some.DLL can use a UMapStrStrIter object created in the 
//			BaseUtils.DLL to fetch each key-value pair in a UMapStringString.
//
// REQUIRE:	Create an instance of this class by calling UMapStringString::CreateIter().
// 
// INVARIANTS:	
//
// EXTENSIONS:
//
// NOTES:	
//
//==================================================================================================
class UMapStrStrIter  
{
public:
	//----------------------------------------------------------------------------------------------
	// PURPOSE: Constructor associates the object with the UMapStringString that it is to iterate.
	//
	UMapStrStrIter(std::map<std::string,std::string>& mapStringString);

	//----------------------------------------------------------------------------------------------
	// PURPOSE: Set the iterator to point to the first item in the collection.  Note that
	//			the first item is also the end of the collection whenever the collection is empty.
	//
	// REQUIRE:	Clients should load the map before expecting to iterate through it.
	//
	void Reset(void);

	//----------------------------------------------------------------------------------------------
	// PURPOSE: Retrieve the key-value pair to which the iterator currently points.
	//			The iterator is automatically incremented to point to the next available
	//			key-value pair in the collection.  Whenever the object is first created or
	//			Reset, the iterator points to the first item in the collection.  Note that
	//			the first item is also the end of the collection whenever the collection is empty.
	//
	// REQUIRE:	Clients should load the map before expecting to iterate through it.
	//
	// RETURN:	whether or not a key-value pair was retrieved
	//			true	-  the key-value pair was retrieved successfully
	//			false	-  failed to retrieve the key-value pair, most likey because iterated to the end
	//
	// ARGS:	
	//		key:		[out] the key used to retrieve a value. 
	//		value:		[out] the value to retrieve when using the associated key
	//
	bool FetchValuePair(std::string* pKey,std::string* pValue=NULL);

private:
	//----------------------------------------------------------------------------------------------
	// PURPOSE: Default Constructor cannot be used.
	//
	UMapStrStrIter();

	std::map<std::string,std::string> m_map;						// the map over which this class iterates
	std::map<std::string,std::string>::const_iterator m_iter;		// const iterator for stl map<string,string>
};