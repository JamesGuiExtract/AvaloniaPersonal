// UMapStrStrIter.cpp: implementation of the UMapStrStrIter class.
#include "stdafx.h"
#include "UMapStrStrIter.h"

using namespace std;

//--------------------------------------------------------------------------------------------------
UMapStrStrIter::UMapStrStrIter(map<string,string>& mapStringString) :
	m_map(mapStringString),
	m_iter(m_map.begin())
{
}

//--------------------------------------------------------------------------------------------------
void UMapStrStrIter::Reset(void)
{
	m_iter = m_map.begin();
}

//--------------------------------------------------------------------------------------------------
bool UMapStrStrIter::FetchValuePair(std::string* pKey,std::string* pValue)
{
	bool bSuccess(false);

	if (m_iter != m_map.end())
	{
		bSuccess = true;
		*pKey = (*m_iter).first;
		if (pValue != __nullptr)
		{
			*pValue = (*m_iter).second;
		}
		++m_iter;
	}

	return bSuccess;
}
