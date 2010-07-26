
#pragma once

#include "BaseUtils.h"
#include <string>
#include <ostream>

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
class EXPORT_BaseUtils stringCSIS
{
	friend EXPORT_BaseUtils ostream& operator<<(ostream& output, const stringCSIS& strText);
public:	
	stringCSIS();
	stringCSIS(const stringCSIS& s);
	stringCSIS(const bool bIsCS );
	stringCSIS(const char* s);
	stringCSIS(const string &s, bool bIsCS = true);

	stringCSIS & operator = (const stringCSIS &m);
	stringCSIS & operator = (const string &s);
	stringCSIS & operator = (const char* s);

	stringCSIS& operator += (const string& strSource);

	operator string() { return m_strSource; } 
	operator string() const { return m_strSource; }
	char& operator[](size_t pos) { return m_strSource[pos]; }
	const char& operator[](size_t pos) const { return m_strSource[pos]; }

	inline const char* c_str() const { return m_strSource.c_str(); }

	inline size_t length() const { return m_strSource.length(); }

	inline bool empty() const { return m_strSource.empty(); }

	// Erases characters from the encapsulated string
	inline void erase(size_t pos = 0, size_t n = string::npos) { m_strSource.erase(pos, n); }

	// Assigns new content to the encapsulated string
	inline void assign(const string& str) { m_strSource.assign(str); }
	inline void assign(const string& str, size_t pos, size_t n) { m_strSource.assign(str, pos, n); }
	inline void assign(const stringCSIS& str) { m_strSource.assign(str.m_strSource); }
	inline void assign(const stringCSIS& str, size_t pos, size_t n) { m_strSource.assign(str.m_strSource, pos, n); }
	inline void assign(const char* psz, size_t n) { m_strSource.assign(psz, n); }
	inline void assign(const char* psz) { m_strSource.assign(psz); }
	inline void assign(size_t n, char c){ m_strSource.assign(n, c); }

	// Sets case sensitive flag
	void setCaseSensitive(const bool bIsCS);

	// Tests case sensitive flag
	bool isCaseSensitive() const;

	static bool isCaseSensitiveByDefault();

	// Static method that defaults to a case insensitive compare of two strings
	static bool sEqual(const string& strA, const string& strB, bool bCaseSensitive = false);

	// Overloaded find methods
	size_t find( const stringCSIS& _Str, size_t _Off = 0) const;
	size_t find( const string& _Str, size_t _Off = 0) const;
	size_t find( const char* _Ptr, size_t _Off = 0) const;
	size_t find( const char* _Ptr, size_t _Off, size_t _Count) const;
	size_t find( char c, size_t _Off = 0) const;

	// Overloaded find_first_not_of methods
	size_t find_first_not_of( const stringCSIS& _Str, size_t _Off = 0 ) const;
	size_t find_first_not_of( const string& _Str, size_t _Off = 0 ) const;
	size_t find_first_not_of( const char* _Ptr, size_t _Off = 0 ) const;
	size_t find_first_not_of( const char* _Ptr, size_t _Off, size_t _Count ) const;
	size_t find_first_not_of( char c, size_t _Off = 0 ) const;

	// Overloaded find_firt_of methods
	size_t find_first_of( const stringCSIS& _Str, size_t _Off = 0 ) const;
	size_t find_first_of( const string& _Str, size_t _Off = 0 ) const;
	size_t find_first_of( const char* _Ptr, size_t _Off = 0 ) const;
	size_t find_first_of( const char* _Ptr, size_t _Off, size_t _Count ) const;
	size_t find_first_of( char c, size_t _Off = 0 ) const;

	// Overloaded find_last_not_of methods
	size_t find_last_not_of( const stringCSIS& _Str, size_t _Off = string::npos ) const;
	size_t find_last_not_of( const string& _Str, size_t _Off = string::npos ) const;
	size_t find_last_not_of( const char* _Ptr, size_t _Off = string::npos ) const;
	size_t find_last_not_of( const char* _Ptr, size_t _Off, size_t _Count ) const;
	size_t find_last_not_of( char c, size_t _Off = string::npos ) const;

	// Overloaded find_last_of methods
	size_t find_last_of( const stringCSIS& _Str, size_t _Off = string::npos ) const;
	size_t find_last_of( const string& _Str, size_t _Off = string::npos ) const;
	size_t find_last_of( const char* _Ptr, size_t _Off = string::npos ) const;
	size_t find_last_of( const char* _Ptr, size_t _Off, size_t _Count ) const;
	size_t find_last_of( char c, size_t _Off = string::npos ) const;

	// Overloaded rfind methods
	size_t rfind( const stringCSIS& _Str, size_t _Off = string::npos) const;
	size_t rfind( const string& _Str, size_t _Off = string::npos) const;
	size_t rfind( const char* _Ptr, size_t _Off = string::npos) const;
	size_t rfind( const char* _Ptr, size_t _Off, size_t _Count) const;
	size_t rfind( char c, size_t _Off = string::npos) const;
	
private:
	string m_strSource;

	// returns string with each character replaced with its upper and lower case values if 
	// they are different otherwise keeps the same character
	// e.g. input: "ab.2d" returns "aAbB.2dD"
	//	ulStrCharToFindCount should be set to a value <= length of the strCharFind and is the number
	// of characters in the string that need to be converted
	// in the above example if ulStrCharToFindCount is 2 with the inputs: "ab.2d" return value: "aAbB" 
	// with rulReturnedStringLength = 4
	string convertCharsToFind(  const string &strCharFind, unsigned long ulStrCharToFindCount,
		unsigned long &rulReturnedStringLength ) const;

	string getLowerUpperChars(char c) const;

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