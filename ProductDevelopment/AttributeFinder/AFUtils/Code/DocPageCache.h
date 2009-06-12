#pragma once

#include <map>
#include <string>

class DocPageCache
{
public:
	DocPageCache();
	
	// Add the ipSS which contains pages nStartPage through nEndPage to the cache
	void add(long nStartPage, long nEndPage, ISpatialStringPtr ipSS);

	// return the cache entry for the specifies page range if one exists
	// otherwise NULL is returned
	ISpatialStringPtr find(long nStartPage, long nEndPage);

private:
	//////////////
	// Variables
	//////////////
	std::map<std::string, ISpatialStringPtr> m_mapCache;
	
	//////////////
	// Methods
	//////////////
	std::string getKey(long nStartPage, long nEndPage);
};