// UMapStrStr.cpp: implementation of the UMapStrStr class.
#include "stdafx.h"
#include "UMapStrStr.h"
#include "UMapStrStrIter.h"

using namespace std;

//--------------------------------------------------------------------------------------------------
UMapStrStrIter* UMapStrStr::CreateIter(void)
{
	return new UMapStrStrIter(m_map);
}

//--------------------------------------------------------------------------------------------------
std::string UMapStrStr::Find(const std::string& key) 
{
	return m_map[key];
}

//--------------------------------------------------------------------------------------------------
bool UMapStrStr::Insert(const std::string& key, const std::string& value)
{
	return m_map.insert(map<string,string>::value_type(key,value)).second;
}

//--------------------------------------------------------------------------------------------------
void UMapStrStr::Add(const std::string& key, const std::string& value)
{
	m_map[key] = value;
}