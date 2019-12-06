#include "stdafx.h"
#include "StringCSIS.h"
#include "cpputil.h"
#include "UCLIDException.h"


//-------------------------------------------------------------------------------------------------
// stringCSIS Contructors
//-------------------------------------------------------------------------------------------------
stringCSIS::stringCSIS()
: m_bCaseSensitive( stringCSIS::isCaseSensitiveByDefault() ),
m_strSource("")
{
}
//-------------------------------------------------------------------------------------------------
stringCSIS::stringCSIS(const stringCSIS& s)
	: m_strSource(static_cast<string>(s))
{ 
	m_bCaseSensitive = s.m_bCaseSensitive;
}
//-------------------------------------------------------------------------------------------------
stringCSIS::stringCSIS(const bool bIsCS )
: m_bCaseSensitive(bIsCS),
m_strSource("")
{
}
//-------------------------------------------------------------------------------------------------
stringCSIS::stringCSIS(const char* s)
: m_bCaseSensitive( stringCSIS::isCaseSensitiveByDefault() ),
m_strSource(s)
{
}
//-------------------------------------------------------------------------------------------------
stringCSIS::stringCSIS(const string &s, bool bIsCS )
: m_strSource(s), m_bCaseSensitive(bIsCS)
{
}

//-------------------------------------------------------------------------------------------------
// Public methods
//-------------------------------------------------------------------------------------------------
bool stringCSIS::isCaseSensitiveByDefault()
{
	return true;
}
//-------------------------------------------------------------------------------------------------
void stringCSIS::setCaseSensitive(const bool bIsCS)
{
	m_bCaseSensitive = bIsCS;
}
//-------------------------------------------------------------------------------------------------
bool stringCSIS::isCaseSensitive() const
{
	return m_bCaseSensitive;
}
//-------------------------------------------------------------------------------------------------
bool stringCSIS::sEqual(const string &strA, const string &strB, bool bCaseSensitive)
{
	if (bCaseSensitive)
	{
		// Perform case sensitive compare
		return strA == strB;
	}
	else
	{
		// Perform case insensitive compare
		return _stricmp(strA.c_str(), strB.c_str()) == 0;
	}
}
//-------------------------------------------------------------------------------------------------
size_t stringCSIS::find( char c, size_t _Off ) const
{
	// If case insensitive search look fro both the upper and lower case of _Ch
	if ( !m_bCaseSensitive )
	{
		string strLowerUpperChars = getLowerUpperChars(c);
		if (!strLowerUpperChars.empty())
		{
			// Find chars in string with string::find_first_of
			return m_strSource.find_first_of(strLowerUpperChars, _Off );
		}
	}

	// Return the base class results ( case sensitive )
	return m_strSource.find(c, _Off);
}
//-------------------------------------------------------------------------------------------------
size_t stringCSIS::find( const char* _Ptr, size_t _Off ) const
{
	// Get the length of the string to find
	size_t nFindStrLen = strnlen(_Ptr, UINT_MAX);

	// Call find using the length of the find string for the count
	return find(_Ptr, _Off, nFindStrLen);
}
//-------------------------------------------------------------------------------------------------
size_t stringCSIS::find( const char* _Ptr, size_t _Off, size_t _Count ) const
{
	// if _Off == string::npos or _Count == string::npos, the return value will be string::npos 
	if ( _Off == string::npos || _Count == string::npos )
	{
		return string::npos;
	}

	// if case sensitive find return the string::find results
	if ( m_bCaseSensitive )
	{
		return m_strSource.find(_Ptr, _Off, _Count );
	}

	// get the length of the string
	size_t nStrLen = m_strSource.length();

	// if the position to start looking plus the Count of chars to compare
	// is greather than the string length the string will not be found
	if ( _Off + _Count > nStrLen )
	{
		// return npos to indicate string not found
		return string::npos;
	}

	// create a temp pointer to the c string
	const char* vtTmpPtr = m_strSource.c_str();

	// Search the string for the subsring
	for ( size_t i = _Off; i <= nStrLen - _Count; i++ )
	{
		if ( _strnicmp(&vtTmpPtr[i], _Ptr, _Count) == 0 )
		{
			// substring was found so return its start position
			return i;
		}
	}

	// substring was not found so return npos
	return string::npos;
}
//-------------------------------------------------------------------------------------------------
size_t stringCSIS::find( const string& _Str, size_t _Off ) const
{
	// Get the length of the string to find
	size_t nFindStrLen = _Str.length();

	// return results of find using c_str(), _Off and the length of the string to find
	return find ( _Str.c_str(), _Off, nFindStrLen );
}
//-------------------------------------------------------------------------------------------------
size_t stringCSIS::find(const stringCSIS& _Str, size_t _Off) const
{
	return find(_Str.c_str(), _Off, _Str.length());
}
//-------------------------------------------------------------------------------------------------
size_t stringCSIS::find_first_not_of( char c, size_t _Off ) const
{
	// If case insensitive search look for both the upper and lower case of c
	if ( !m_bCaseSensitive )
	{
		string strLowerUpperChars = getLowerUpperChars(c);
		if (!strLowerUpperChars.empty())
		{
			// Find chars in string with string::find_first_not_of
			return m_strSource.find_first_not_of(strLowerUpperChars, _Off );
		}
	}

	// if case sensitive, return the string::find_first_not_of results
	return m_strSource.find_first_not_of(c, _Off );
}
//-------------------------------------------------------------------------------------------------
size_t stringCSIS::find_first_not_of( const char* _Ptr, size_t _Off ) const
{
	// Get the length of the string with chars to find
	size_t nFindStrLen = strnlen(_Ptr, UINT_MAX);

	// return results from find_first_not_of with chars to find, starting location and 
	// lenght of find string
	return find_first_not_of(_Ptr, _Off, nFindStrLen);
}
//-------------------------------------------------------------------------------------------------
size_t stringCSIS::find_first_not_of( const char* _Ptr, size_t _Off, size_t _Count) const
{
	// Prevent read access violations
	// https://extract.atlassian.net/browse/ISSUE-16822
	if (_Count == string::npos) // = search for not any character
	{
		return string::npos;
	}

	// Create strFind to contain the char to search
	string strFind;
	unsigned long ulCharsToSearch = _Count;

	// if case insensitive search, will need to have the upper and lower case chars in the search string
	// if count == npos or 0 the strFind value can be _Ptr because the 
	// results of the find_first_not_of will match no chars if _Count == npos or
	// match any char if _Count == 0
	if ( !m_bCaseSensitive && _Count != string::npos && _Count != 0)
	{	
		// Add the upper or lower case values to the string
		strFind = convertCharsToFind(_Ptr, ulCharsToSearch, ulCharsToSearch);
	}
	else
	{
		strFind = _Ptr;
	}
	
	// return results of string::find_first_not_of with the search string
	return m_strSource.find_first_not_of( strFind.c_str(), _Off, ulCharsToSearch);
}
//-------------------------------------------------------------------------------------------------
size_t stringCSIS::find_first_not_of( const string& _Str, size_t _Off) const
{
	// Return the value from call to find_first_not_of with the string, offset and count of chars
	return find_first_not_of(_Str.c_str(), _Off, _Str.length());
}
//-------------------------------------------------------------------------------------------------
size_t stringCSIS::find_first_not_of(const stringCSIS& _Str, size_t _Off) const
{
	return find_first_not_of(_Str.c_str(), _Off, _Str.length());
}
//-------------------------------------------------------------------------------------------------
size_t stringCSIS::find_first_of( char c, size_t _Off) const
{
	// If case insensitive search look fro both the upper and lower case of c
	if ( !m_bCaseSensitive )
	{
		string strLowerUpperChars = getLowerUpperChars(c);
		if (!strLowerUpperChars.empty())
		{
			// Find chars in string with string::find_first_of
			return m_strSource.find_first_of(strLowerUpperChars, _Off );
		}
	}

	// Just need to look for the char case sensitive search
	return m_strSource.find_first_of(c, _Off );
}
//-------------------------------------------------------------------------------------------------
size_t stringCSIS::find_first_of( const char* _Ptr, size_t _Off ) const
{
	// Call find_first_of with the string, offset and lenght of the search string for count
	return find_first_of(_Ptr, _Off, strnlen(_Ptr, UINT_MAX));
}
//-------------------------------------------------------------------------------------------------
size_t stringCSIS::find_first_of( const char* _Ptr, size_t _Off, size_t _Count) const
{
	// Prevent read access violations
	// https://extract.atlassian.net/browse/ISSUE-16822
	if (_Count == string::npos) // = search for any character
	{
		if (m_strSource.empty())
		{
			return string::npos;
		}
		if (_Off < m_strSource.length())
		{
			return _Off;
		}
		return string::npos;
	}

	// Set string find to the string passed in
	string strFind;
	unsigned long ulCharsToSearch = _Count;

	// if case insensitive search need to add upper or lower case values of char
	// if count == npos or 0 the strFind value can be _Ptr because the 
	// results of the find_first_of will match any char if _Count == npos or
	// match no char if _Count == 0
	if ( !m_bCaseSensitive && _Count != string::npos && _Count != 0 )
	{	
		// Add the upper or lower case values to the string
		strFind = convertCharsToFind(_Ptr, ulCharsToSearch, ulCharsToSearch);
	}
	else
	{
		strFind = _Ptr;
	}

	// Use string::find_first_of to find the char in string using strFind
	return m_strSource.find_first_of( strFind.c_str(), _Off, ulCharsToSearch);
}
//-------------------------------------------------------------------------------------------------
size_t stringCSIS::find_first_of( const string& _Str, size_t _Off ) const
{
	// Call find_first_of with the string, offset and length of string to find
	return find_first_of(_Str.c_str(), _Off, _Str.length());
}
//-------------------------------------------------------------------------------------------------
size_t stringCSIS::find_first_of(const stringCSIS& _Str, size_t _Off) const
{
	return find_first_of(_Str.c_str(), _Off, _Str.length());
}
//-------------------------------------------------------------------------------------------------
size_t stringCSIS::find_last_not_of(const char c, size_t _Off) const
{
	// If case insensitive search look fro both the upper and lower case of _Ch
	if ( !m_bCaseSensitive )
	{
		string strLowerUpperChars = getLowerUpperChars(c);
		if (!strLowerUpperChars.empty())
		{
			// Find chars in string with string::find_last_not_of
			return m_strSource.find_last_not_of(strLowerUpperChars, _Off );
		}
	}

	// use string class find_last_not_of for case sensitive search
	return m_strSource.find_last_not_of(c, _Off);
}
//-------------------------------------------------------------------------------------------------
size_t stringCSIS::find_last_not_of( const char* _Ptr, size_t _Off) const
{
	// Call find_last_not_of with the string, offset and the length of the string to find for count
	return find_last_not_of( _Ptr, _Off, strnlen(_Ptr, UINT_MAX));
}
//-------------------------------------------------------------------------------------------------
size_t stringCSIS::find_last_not_of( const char* _Ptr, size_t _Off, size_t _Count) const
{
	// Prevent read access violations
	// https://extract.atlassian.net/browse/ISSUE-16822
	if (_Count == string::npos) // = search for not any character
	{
		return string::npos;
	}

	// Set string find to the string passed in
	string strFind;
	unsigned long ulCharsToSearch = _Count;

	// if case insensitive search need to add upper or lower case values of char
	// if count == npos or 0 the strFind value can be _Ptr because the 
	// results of the find_last_not_of will match no chars if _Count == npos or
	// match any char if _Count == 0
	if ( !m_bCaseSensitive && _Count != string::npos && _Count != 0 )
	{	
		// Add the upper or lower case values to the string
		strFind = convertCharsToFind(_Ptr, ulCharsToSearch, ulCharsToSearch);
	}
	else
	{
		strFind = _Ptr;
	}

	// call string::find_last_not_of to get results
	return m_strSource.find_last_not_of( strFind.c_str(), _Off, ulCharsToSearch);
}
//-------------------------------------------------------------------------------------------------
size_t stringCSIS::find_last_not_of( const string& _Str, size_t _Off ) const
{
	// call find_last_not_of with the string, offset and length of the string to find as count
	return find_last_not_of( _Str.c_str(), _Off, _Str.length());
}
//-------------------------------------------------------------------------------------------------
size_t stringCSIS::find_last_not_of( const stringCSIS& _Str, size_t _Off ) const
{
	// call find_last_not_of with the string, offset and length of the string to find as count
	return find_last_not_of( _Str.c_str(), _Off, _Str.length());
}
//-------------------------------------------------------------------------------------------------
size_t stringCSIS::find_last_of( char c, size_t _Off) const
{
	// If case insensitive search look for both the upper and lower case of c
	if ( !m_bCaseSensitive )
	{
		string strLowerUpperChars = getLowerUpperChars(c);
		if (!strLowerUpperChars.empty())
		{
			// Find chars in string with string::find_last_of
			return m_strSource.find_last_of(strLowerUpperChars, _Off );
		}
	}

	// find_last_of with case sensitive
	return m_strSource.find_last_of(c, _Off );
}
//-------------------------------------------------------------------------------------------------
size_t stringCSIS::find_last_of( const char* _Ptr, size_t _Off) const
{
	// return results from find_last_of with the string, offset and the length of the find string as count
	return find_last_of( _Ptr, _Off, strnlen(_Ptr, UINT_MAX));
}
//-------------------------------------------------------------------------------------------------
size_t stringCSIS::find_last_of( const char* _Ptr, size_t _Off, size_t _Count) const
{
	// Prevent read access violations
	// https://extract.atlassian.net/browse/ISSUE-16822
	if (_Count == string::npos) // = search for any character
	{
		if (m_strSource.empty())
		{
			return string::npos;
		}
		if (_Off < m_strSource.length())
		{
			return _Off;
		}
		return m_strSource.length() - 1;
	}

	// Set string find to the string passed in
	string strFind;
	unsigned long ulCharsToSearch = _Count;

	// if case insensitive search need to add upper or lower case values of char
	// if count == npos or 0 the strFind value can be _Ptr because the 
	// results of the find_last_of will match any char if _Count == npos or
	// match no char if _Count == 0
	if ( !m_bCaseSensitive && _Count != string::npos && _Count != 0 )
	{	
		// Add the upper or lower case values to the string
		strFind = convertCharsToFind(_Ptr, ulCharsToSearch, ulCharsToSearch);
	}
	else
	{
		strFind = _Ptr;
	}

	return m_strSource.find_last_of( strFind.c_str(), _Off, ulCharsToSearch);
}
//-------------------------------------------------------------------------------------------------
size_t stringCSIS::find_last_of( const string& _Str, size_t _Off ) const
{
	// return results from find_last_of using string, offset and length of string to find as count
	return find_last_of( _Str.c_str(), _Off, _Str.length());
}
//-------------------------------------------------------------------------------------------------
size_t stringCSIS::find_last_of( const stringCSIS& _Str, size_t _Off ) const
{
	// return results from find_last_of using string, offset and length of string to find as count
	return find_last_of( _Str.c_str(), _Off, _Str.length());
}
//-------------------------------------------------------------------------------------------------
size_t stringCSIS::rfind( const string& _Str, size_t _Off ) const
{
	// return results from rfind with sting, offset and length of string to find as count
	return rfind(_Str.c_str(), _Off, _Str.length());
}
//-------------------------------------------------------------------------------------------------
size_t stringCSIS::rfind( const stringCSIS& _Str, size_t _Off ) const
{
	// return results from rfind with sting, offset and length of string to find as count
	return rfind(_Str.c_str(), _Off, _Str.length());
}
//-------------------------------------------------------------------------------------------------
size_t stringCSIS::rfind(	char c, size_t _Off ) const
{
	// return results of find_last_of with the char
	return find_last_of( c, _Off);
}
//-------------------------------------------------------------------------------------------------
size_t stringCSIS::rfind(	const char* _Ptr,	size_t _Off ) const
{
	// return results from rfind with sting, offset and length of string to find as count
	return rfind(_Ptr, _Off, strnlen(_Ptr, UINT_MAX));
}
//-------------------------------------------------------------------------------------------------
size_t stringCSIS::rfind(	const char* _Ptr, size_t _Off, size_t _Count) const
{
	// if the _Off == 0 or _Count == string::npos then return string::npos
	if ( _Off == 0 || _Count == string::npos )
	{
		return string::npos;
	}

	// if case sensitive just pass the call to the string class
	if ( m_bCaseSensitive )
	{
		return m_strSource.rfind(_Ptr, _Off, _Count );
	}

	// get length of string
	size_t nFindStrLen = strnlen(_Ptr, UINT_MAX);
	size_t nStrLen = m_strSource.length();

	// if the _Off is string::npos it should be changed to the length of the string to search
	if ( _Off == string::npos )
	{
		_Off = nStrLen;
	}

	// The _Count must be <= length of the find string
	if ( _Count > nFindStrLen )
	{
		// Set to the find string length as a default
		_Count = nFindStrLen;

		// if the _Count is > the the length of find string log exception 
		UCLIDException ue("ELI16004", "Non critical internal error.");
		ue.addDebugInfo("Count", _Count);
		ue.addDebugInfo("Length", nFindStrLen);
		ue.log();
	}

	// The _Off must be <= length of the string
	if ( _Off > nStrLen )
	{
		_Off = nStrLen;

		// if the _Off is > the the length of string to search log exception 
		UCLIDException ue("ELI15981", "Non critical internal error.");
		ue.addDebugInfo("Offset", _Off);
		ue.addDebugInfo("Length", nStrLen);
		ue.log();
	}

	// if the number of chars to search for is larger than the offset
	// the string will not be found in the search string so return string::npos
	if ( _Count > _Off )
	{
		return string::npos;
	}

	// search string in reverse from _Off - _Count
	for ( size_t i = _Off - _Count + 1; i--> 0; )
	{
		if ( _strnicmp(&c_str()[i], _Ptr, _Count) == 0 )
		{
			// string was found so return its location
			return i;
		}
	}

	// string was not found so return npos
	return string::npos;
}

//-------------------------------------------------------------------------------------------------
// Operator =
//-------------------------------------------------------------------------------------------------
stringCSIS& stringCSIS::operator =(const stringCSIS &m)
{
	// call string = operator to set string properties
	m_strSource = m.m_strSource;

	// set the case sensitive flag
	m_bCaseSensitive = m.m_bCaseSensitive;
	
	// return reference to itself
	return *this;
}
//-------------------------------------------------------------------------------------------------
stringCSIS & stringCSIS::operator = (const string &s)
{
	// just set the string properties
	m_strSource = s;
	return *this;
}
//-------------------------------------------------------------------------------------------------
stringCSIS & stringCSIS::operator = (const char* s)
{
	// set the string properties
	m_strSource = s;
	return *this;
}

//-------------------------------------------------------------------------------------------------
// Operator +=
//-------------------------------------------------------------------------------------------------
stringCSIS& stringCSIS::operator += (const string& strSource)
{
	m_strSource += strSource;

	return *this;
}

//-------------------------------------------------------------------------------------------------
// Operator <<
//-------------------------------------------------------------------------------------------------
ostream& operator<<(ostream& output, const stringCSIS& strText)
{
	output << strText.m_strSource;
	return output;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
string stringCSIS::convertCharsToFind( const string &strCharFind, unsigned long ulStrCharToFindCount,
									  unsigned long &rulReturnedStringLength ) const
{
	// if the count is greater than the number of chars in the string
	// set the count to the length of the string
	if ( ulStrCharToFindCount > strCharFind.length() )
	{
		// if the count is > the the length of string to find log exception 
		UCLIDException ue("ELI15927", "Non critical internal error.");
		ue.addDebugInfo("Count", ulStrCharToFindCount);
		ue.addDebugInfo("Length", strCharFind.length());
		ue.log();

		// Default to the length of the strCharFind string
		ulStrCharToFindCount = strCharFind.length();
	}

	// Build the string to find by adding the lower or uppercase chars
	string strRtn = "";
	rulReturnedStringLength = ulStrCharToFindCount;

	// Only need to do the first _Count chars
	for ( unsigned long i = 0; i < ulStrCharToFindCount; i++ )
	{
		// Get the upper and lower case values
		int iUpper = toupper(strCharFind[i]);
		int iLower = tolower(strCharFind[i]);

		// if equal just add char to the find string
		if ( iUpper == iLower )
		{
			strRtn += strCharFind[i];
		}
		else
		{
			// TODO: This can cause letters to be included in string more than once 
			//			since a character can be in the string already could
			//			check the string for the char before adding it, but that
			//			may not be as fast as just adding the char again
			// add both upper and lower case values to find string
			strRtn += iLower;
			strRtn += iUpper;

			// update count to include the extra value
			rulReturnedStringLength++;
		}
	}

	// return the adjusted string
	return strRtn;
}
//-------------------------------------------------------------------------------------------------
string stringCSIS::getLowerUpperChars(char c) const
{
	// get the upper and lower case values
	int iUpper = toupper(c);
	int iLower = tolower(c);

	// if not equal search for both
	if ( iUpper != iLower )
	{
		// Build string to find with upper and lower case value
		string strFind = "";
		strFind += iUpper;
		strFind += iLower;
		return strFind;
	}
	return "";
}
//-------------------------------------------------------------------------------------------------
