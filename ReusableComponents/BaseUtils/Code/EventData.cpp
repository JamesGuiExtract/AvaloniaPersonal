
#include "stdafx.h"
#include "EventData.h"

#include "UCLIDException.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

ValueTypePair& EventData::operator[] (const string& strKey)
{
	return map<string, ValueTypePair>::operator[](strKey);
}

void EventData::add(const string& strKey, const ValueTypePair& vtpData)
{
	map<string, ValueTypePair>::operator[](strKey) = vtpData;
}

const ValueTypePair& EventData::get(const string& strKey) const
{
	const_iterator iter = find(strKey);

	if (iter == end())
	{
		// the key was not found, throw an exception
		throw UCLIDException("ELI00834", "Internal error: EventData key not found!");
	}

	const ValueTypePair p = iter->second;

	// return the underlying data
	return iter->second;
}

// a method to determine if a certain key exists as part of the data
bool EventData::has(const string& strKey) const
{
	const_iterator iter = find(strKey);

	// return true only if the item was found.
	return iter != end();
}

// need a way to determine all the event data key's
vector<string> EventData::getKeys(void) const
{
	vector<string> vecResult;

	const_iterator iter;
	for (iter = begin(); iter != end(); iter++)
		vecResult.push_back(iter->first);

	return vecResult;
}
