
#pragma once

#include "BaseUtils.h"
#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// class stringCSIS
//-------------------------------------------------------------------------------------------------
// PURPOSE: To have all the functionality of STL string, with the ability to 
//			control the case-sensitivity for methods like find(), find_first_of(),
//			etc.
//
// USAGE:	By default this class acts in a string. The case sensitivity should be set to 
//			false to have compares and find methods search without case sensitivity
//			The case sensitivity can be set in a constructor or with the SetCaseSensitive method
//
class EXPORT_BaseUtils stringCSIS: public string
{
public:	
	stringCSIS();
	stringCSIS(const stringCSIS& s);
	stringCSIS(const bool bIsCS );
	stringCSIS(const char* s);
	stringCSIS(const string &s, bool bIsCS = true);

	stringCSIS & operator = (const stringCSIS &m);
	stringCSIS & operator = (const string &s);
	stringCSIS & operator = (const char* s);

	// Sets case sensitive flag
	void setCaseSensitive(const bool bIsCS);

	// Tests case sensitive flag
	bool isCaseSensitive() const;

	static bool isCaseSensitiveByDefault();

	// Static method that defaults to a case insensitive compare of two strings
	static bool sEqual(const string& strA, const string& strB, bool bCaseSensitive = false);

	// Overloaded find methods
	size_t find( const basic_string& _Str, size_t _Off = 0) const;
	size_t find(	value_type _Ch, size_t _Off = 0) const;
	size_t find(	const value_type* _Ptr,	size_t _Off = 0) const;
	size_t find(	const value_type* _Ptr, size_t _Off, size_t _Count) const;

	// Overloaded find_first_not_of methods
	size_t find_first_not_of( value_type _Ch, size_t _Off = 0) const;
	size_t find_first_not_of( value_type * _Ptr, size_t _Off = 0) const;
	size_t find_first_not_of( const value_type* _Ptr, size_t _Off, size_t _Count) const;
	size_t find_first_not_of( const basic_string& _Str, size_t _Off = 0 ) const;

	// Overloaded find_firt_of methods
	size_t find_first_of( value_type _Ch, size_t _Off = 0) const;
	size_t find_first_of( value_type * _Ptr, size_t _Off = 0) const;
	size_t find_first_of( const value_type* _Ptr, size_t _Off, size_t _Count) const;
	size_t find_first_of( const basic_string& _Str, size_t _Off = 0 ) const;

	// Overloaded find_last_not_of methods
	size_t find_last_not_of( value_type _Ch, size_t _Off = npos) const;
	size_t find_last_not_of( value_type * _Ptr, size_t _Off = npos) const;
	size_t find_last_not_of( const value_type* _Ptr, size_t _Off, size_t _Count) const;
	size_t find_last_not_of( const basic_string& _Str, size_t _Off = npos ) const;

	// Overloaded find_last_of methods
	size_t find_last_of( value_type _Ch, size_t _Off = npos) const;
	size_t find_last_of( value_type * _Ptr, size_t _Off = npos) const;
	size_t find_last_of( const value_type* _Ptr, size_t _Off, size_t _Count) const;
	size_t find_last_of( const basic_string& _Str, size_t _Off = npos ) const;

	// Overloaded rfind methods
	size_t rfind( const basic_string& _Str, size_t _Off = npos) const;
	size_t rfind(	value_type _Ch, size_t _Off = npos) const;
	size_t rfind(	const value_type* _Ptr,	size_t _Off = npos) const;
	size_t rfind(	const value_type* _Ptr, size_t _Off, size_t _Count) const;
	
private:
	
	// returns string with each character replaced with its upper and lower case values if 
	// they are different otherwise keeps the same character
	// e.g. input: "ab.2d" returns "aAbB.2dD"
	//	ulStrCharToFindCount should be set to a value <= length of the strCharFind and is the number
	// of characters in the string that need to be converted
	// in the above example if ulStrCharToFindCount is 2 with the inputs: "ab.2d" return value: "aAbB" 
	// with rulReturnedStringLength = 4
	string convertCharsToFind(  const string &strCharFind, unsigned long ulStrCharToFindCount,
		unsigned long &rulReturnedStringLength ) const;

	// returns empty string if not affected by case otherwise returns string with upper and lower case
	// values
	string getLowerUpperChars(value_type _Ch) const;

	bool m_bCaseSensitive;
};

//-------------------------------------------------------------------------------------------------
inline bool operator !=(const stringCSIS &s1, const string &s2 )
{
	if ( s1.isCaseSensitive() )
	{
		return strcmp(s1.c_str(), s2.c_str()) != 0;
	}
	return _stricmp(s1.c_str(), s2.c_str()) != 0;
}
//-------------------------------------------------------------------------------------------------
inline bool operator ==(const stringCSIS &s1, const string &s2)
{
	if ( s1.isCaseSensitive() )
	{
		return strcmp(s1.c_str(), s2.c_str()) == 0;
	}
	return _stricmp(s1.c_str(), s2.c_str()) == 0;
}
//-------------------------------------------------------------------------------------------------
inline bool operator <(const stringCSIS &s1, const string &s2)
{
	if ( s1.isCaseSensitive() )
	{
		return strcmp(s1.c_str(), s2.c_str()) < 0;
	}
	return _stricmp(s1.c_str(), s2.c_str()) < 0;
}
//-------------------------------------------------------------------------------------------------
inline bool operator <=(const stringCSIS &s1, const string &s2)
{
	if ( s1.isCaseSensitive() )
	{
		return strcmp(s1.c_str(), s2.c_str()) <= 0;
	}
	return _stricmp(s1.c_str(), s2.c_str()) <= 0;
}
//-------------------------------------------------------------------------------------------------
inline bool operator >(const stringCSIS &s1, const string &s2)
{
	if ( s1.isCaseSensitive() )
	{
		return strcmp(s1.c_str(), s2.c_str()) > 0;
	}
	return _stricmp(s1.c_str(), s2.c_str()) > 0;
}
//-------------------------------------------------------------------------------------------------
inline bool operator >=(const stringCSIS &s1, const string &s2)
{
	if ( s1.isCaseSensitive() )
	{
		return strcmp(s1.c_str(), s2.c_str()) >= 0;
	}
	return _stricmp(s1.c_str(), s2.c_str()) >= 0;
}
//-------------------------------------------------------------------------------------------------
