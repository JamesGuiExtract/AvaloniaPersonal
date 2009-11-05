#include <stdafx.h>
#include "DocPageCache.h"

#include <UCLIDException.h>
#include <cpputil.h>

//-------------------------------------------------------------------------------------------------
DocPageCache::DocPageCache()
{
}
//-------------------------------------------------------------------------------------------------
DocPageCache::~DocPageCache()
{
	try
	{
		m_mapCache.clear();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI28216");
}
//-------------------------------------------------------------------------------------------------
void DocPageCache::add(long nStartPage, long nEndPage, const string& strText)
{
	m_mapCache[getKey(nStartPage, nEndPage)] = strText;
}
//-------------------------------------------------------------------------------------------------
bool DocPageCache::find(long nStartPage, long nEndPage, string& rstrText)
{
	map<string, string>::iterator it = m_mapCache.find(getKey(nStartPage, nEndPage));
	if(it != m_mapCache.end())
	{
		rstrText = it->second;
		return true;
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
string DocPageCache::getKey(long nStartPage, long nEndPage)
{
	return asString(nStartPage) + "." + asString(nEndPage);
}
//-------------------------------------------------------------------------------------------------