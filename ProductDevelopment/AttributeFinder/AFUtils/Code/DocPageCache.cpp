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
		// Release COM objects
		for(map<string, ISpatialStringPtr>::iterator it = m_mapCache.begin();
			it != m_mapCache.end(); it++)
		{
			it->second = NULL;
		}
		m_mapCache.clear();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI28216");
}
//-------------------------------------------------------------------------------------------------
void DocPageCache::add(long nStartPage, long nEndPage, const ISpatialStringPtr& ipSS)
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
string DocPageCache::getKey(long nStartPage, long nEndPage)
{
	return asString(nStartPage) + "." + asString(nEndPage);
}
//-------------------------------------------------------------------------------------------------