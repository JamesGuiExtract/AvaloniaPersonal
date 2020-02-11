#ifndef CSIS_UTILS_H
#define CSIS_UTILS_H

#include "BaseUtils.h"

#include <map>
#include <set>
#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// This struct provides case-insensitive comparison of two strings for the purposes of use in STL
// classes.
// Returns true if the first argument is less than the second.
struct csis_less : binary_function<string, string, bool>
{
	struct compare : public binary_function<unsigned char,unsigned char,bool> 
    {
		bool operator() (const unsigned char& c1, const unsigned char& c2) const 
		{
			return tolower(c1) < tolower(c2);
		}
	};

	bool operator() (const string & s1, const string & s2) const
    {
		return _stricmp(s1.c_str(), s2.c_str()) < 0;
    }
};
//-------------------------------------------------------------------------------------------------
// Defines an STL map where the key is a string that will be compared case-insensitively and the
// value is of type T.
// Example use:
// csis_map<string>::type mapTest;
// mapTest["one"] = "Test";
template<typename T>
struct csis_map
{
	typedef map<string, T, csis_less> type;
};

//-------------------------------------------------------------------------------------------------
// Defines an STL set where the values are strings that will be compared case-insensitively
// Example use:
// csis_set::type setTest;
// setTest["one"] = "Test";
struct csis_set
{
	typedef set<string, csis_less> type;
};

#endif // CSIS_UTILS_H