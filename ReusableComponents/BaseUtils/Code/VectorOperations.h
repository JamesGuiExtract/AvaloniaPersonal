#pragma once

#include "BaseUtils.h"
#include "UCLIDException.h"

#include <string>
#include <vector>

using namespace std;

template <class T>
bool vectorContainsElement(const vector<T>& vec, 
						   const T& elemToFind)
{
	//return find(vec.begin(), vec.end(), elemToFind) != vec.end();
	for (unsigned int i = 0; i < vec.size(); i++)
	{
		T t1 = vec[i];
		T t2 = elemToFind;

		if(t1 == t2)
		{
			return true;
		}
	}
	return false;
}
//-------------------------------------------------------------------------------------------------
template <class T>
void vectorPushBackIfNotContained(vector<T>& vec, 
								  const T& elemToPush)
{
	if(!vectorContainsElement(vec, elemToPush))
	{
		vec.push_back(elemToPush);
	}
}
//-------------------------------------------------------------------------------------------------
template <class T>
void eraseFromVector(vector<T>& rVector, 
					 const T& item)
{
	// try to find the item in the vector
	vector<T>::iterator iter = find(rVector.begin(), rVector.end(), item);
	
	// if the item was found, erase all instances of it from the vector
	if (iter != rVector.end())
	{
		rVector.erase(iter);
	}
}
//-------------------------------------------------------------------------------------------------
template <class T>
void addVectors(vector<T>& rvecSource, 
		   const vector<T>& vecToAdd)
{
	vector<T>::const_iterator iter;

	for (iter = vecToAdd.begin(); iter != vecToAdd.end(); iter++)
	{
		rvecSource.push_back(*iter);
	}
}
//-------------------------------------------------------------------------------------------------
// Adds a specified range of numbers to a vector of the specified type (will be done by explicitly
// casting the unsigned long representation of the string to type T). The range can be a comma
// delimited list, a range of numbers by using a hyphen, or a combination of both.
//  
template <typename T>
void addRangeToVector(vector<T>& rvecSource, const string& str)
{
	try
	{
		try
		{
			vector<string> vecTokens;
			StringTokenizer::sGetTokens(str, ',', vecTokens);

			// Loop through all comma delimited tokens first.
			for (vector<string>::iterator iter = vecTokens.begin(); iter != vecTokens.end(); iter++)
			{
				unsigned long nDashIndex = iter->find_first_of('-');

				// If there are no dashes, treat this as a specific number to be added to the vector.
				if (nDashIndex == string::npos)
				{
					rvecSource.push_back((T)asUnsignedLong(*iter));
				}
				// If there is a dash, treat this token as a contiguous range of numbers
				else
				{
					if (nDashIndex != iter->find_last_of('-'))
					{
						throw new UCLIDException("ELI32500", "Multiple dashes where one is expected");
					}

					long nStartIndex = asUnsignedLong(iter->substr(0, nDashIndex));
					long nEndIndex = asUnsignedLong(iter->substr(nDashIndex + 1));

					if (nEndIndex < nStartIndex)
					{
						UCLIDException ue("ELI32503", "Invalid range.");
						ue.addDebugInfo("Range", *iter);
						throw ue;
					}
					
					rvecSource.reserve(rvecSource.size() + nEndIndex - nStartIndex + 1);
					for (long i = nStartIndex; i <= nEndIndex; i++)
					{
						rvecSource.push_back((T)i);
					}
				}
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32501");
	}
	catch (UCLIDException& ue)
	{
		UCLIDException uex2("ELI32502", "Failed to convert range of numbers", ue);
		uex2.addDebugInfo("Range", str);
		throw uex2;
	}
}