//==================================================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	OptionsProcessor.h
//
// PURPOSE:	Provides support for standardized options file processing
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//==================================================================================================

#pragma once
#include "BaseUtils.h"

#include <string>
#include <map>
using namespace std;

class EXPORT_BaseUtils OptionsProcessor
{
public:
	//=======================================================================
	// PURPOSE: Constructor.
	// REQUIRE: Nothing
	// PROMISE: None
	// ARGS:	pszDelimiter - delimiter between option name and option value
	OptionsProcessor(const char *pszDelimiter = "=");
	
	//=======================================================================
	// PURPOSE: Adds an option name and value association to the map.
	// REQUIRE: Nothing
	// PROMISE: None
	// ARGS:	strOptionName - Option key
	//				strDefaultValue - Value associated with strOptionName
	void	addOption(const string& strOptionName, 
		const string& strDefaultValue);
	
	//=======================================================================
	// PURPOSE: Returns Boolean value associated with the specified option.
	// REQUIRE: Nothing
	// PROMISE: Returns false if value is {0, F, false, off}.
	//				Returns true if value is {1, T, true, on}.
	// ARGS:	strOptionName - Option key
	bool	getBooleanOptionValue(const string& strOptionName);
	
	//=======================================================================
	// PURPOSE: Returns the value associated with the specified key.
	// REQUIRE: strOptionName must already be defined in the map.
	// PROMISE: The value is returned
	// ARGS:	strOptionName - Option key
	const string&	getStringOptionValue(const string& strOptionName);
	
	//=======================================================================
	// PURPOSE: Validates an input string
	// REQUIRE: Nothing
	// PROMISE: True is returned if the delimiter is found within strInput 
	//				but not at the beginning.  Otherwise, false is returned.
	// ARGS:	strInput - String checked for name, delimiter, and value
	bool	isValidOptionString(const string& strInput);
	
	//=======================================================================
	// PURPOSE: Parses the input string into option name and value and adds
	//				it to the map.
	// REQUIRE: strInput must be valid.
	// PROMISE: None
	// ARGS:	strInput - String parsed to yield option name and value
	void	processOptionString(const string& strInput);
	
private:

	// Delimiter between option name and option value
	std::string m_strDelimiter;

	// Map associating option names and option values
	std::map<std::string, std::string> m_mapOptionNameToValue;
};
