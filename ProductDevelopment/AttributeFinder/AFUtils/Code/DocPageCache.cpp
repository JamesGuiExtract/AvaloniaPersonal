#include <stdafx.h>
#include "DocPageCache.h"
#include <cpputil.h>

//-------------------------------------------------------------------------------------------------
DocPageCache::DocPageCache()
{
}
//-------------------------------------------------------------------------------------------------
void DocPageCache::add(long nStartPage, long nEndPage, ISpatialStringPtr ipSS)
{
	m_mapCache[getKey(nStartPage, nEndPage)] = ipSS;
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr DocPageCache::find(long nStartPage, long nEndPage)
{
	map<string, ISpatialStringPtr>::iterator it = m_mapCache.find(getKey(nStartPage, nEndPage));
	if(it == m_mapCache.end())
	{
		return NULL;
	}
	else
	{
		return it->second;
	}
}
//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
std::string DocPageCache::getKey(long nStartPage, long nEndPage)
{
	return asString(nStartPage) + "." + asString(nEndPage);
}
//-------------------------------------------------------------------------------------------------