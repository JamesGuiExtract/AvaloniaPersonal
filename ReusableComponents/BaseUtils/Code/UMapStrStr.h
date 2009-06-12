//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	UMapStrStr.h
//
// PURPOSE:	UMapStrStr is responsible for implementing a map-like data structure that can 
//			safely be used across module boundaries (i.e. from one DLL to another) unlike the stl map.
//			The UMapStrStr encapsulates an stl map<string,string>
//			For example, a client in Some.DLL can use a UMapStrStr object created in the 
//			BaseUtils.DLL to both create and use a map in the BaseUtils.DLL
//
// NOTES:		
//
// AUTHORS:	John Hurd
//
//==================================================================================================

#pragma once

#include "BaseUtils.h"

#include <map>
#include <string>

class UMapStrStrIter;

//==================================================================================================
//
// CLASS:	UMapStrStr
//
// PURPOSE:	UMapStrStr is responsible for implementing a map-like data structure that can 
//			safely be used across module boundaries (i.e. from one DLL to another) unlike the stl map.
//			The UMapStrStr encapsulates an stl map<string,string>
//			For example, a client in Some.DLL can use a UMapStrStr object created in the 
//			BaseUtils.DLL to both create and use a map in the BaseUtils.DLL
//
// REQUIRE:	Clients should load the map with the desired key-value pairs before use.
//			If a client calls CreateIter, the client is responsible for deleting the UMapStrStrIter object.
// 
// INVARIANTS:	
//
// EXTENSIONS:
//
// NOTES:	
//
//==================================================================================================
class EXPORT_BaseUtils UMapStrStr  
{
public:
	//----------------------------------------------------------------------------------------------
	// PURPOSE: Create an iterator to be used with the UMapStrStr object.
	//			The iterator will enumerate through the entire map collection.
	//
	// REQUIRE:	Clients are RESPONSIBLE for deleting the the newly create UMapStrStrIter object
	//			when they are finished using it.   Otherwise memory leaks will occur!
	//
	// RETURN:	the newly create UMapStrStrIter object
	//
	// ARGS:	
	//
	UMapStrStrIter* CreateIter(void);

	//----------------------------------------------------------------------------------------------
	// PURPOSE: Finds the value associated with the key.
	//
	// REQUIRE:	Clients should load the map before expecting to find anything in it
	//
	// RETURN:	the value associated with the key
	//
	// ARGS:	
	//		key:		[in] the key used to retrieve a value. 
	//
	std::string Find(const std::string& key);

	//----------------------------------------------------------------------------------------------
	// PURPOSE: Load the map with a key-value pair.  Insert() will NOT overwrite
	//			an existing key's value with a new value if the key already exists in the map.
	//
	// REQUIRE:	Clients should load the map before expecting to find anything in it
	//
	// RETURN:	whether or not the key-value pair was loaded
	//			true	-  the key-value pair was loaded successfully
	//			false	-  the key already exists so the new value was NOT loaded
	//
	// ARGS:	
	//		key:		[in] the key used to retrieve a value. 
	//		value:		[in] the value to retrieve when using the associated key
	//
	bool Insert(const std::string& key, const std::string& value);

	//----------------------------------------------------------------------------------------------
	// PURPOSE: Load the map with a key-value pair.  ADD() will overwrite
	//			an existing key's value with a new value if the key already exists in the map.
	//
	// REQUIRE:	Clients should load the map before expecting to find anything in it
	//
	// ARGS:	
	//		key:		[in] the key used to retrieve a value. 
	//		value:		[in] the value to retrieve when using the associated key
	//
	void Add(const std::string& key, const std::string& value);

private:
	std::map<std::string,std::string> m_map;	// STL map wrapped by UMapStrStr

};
