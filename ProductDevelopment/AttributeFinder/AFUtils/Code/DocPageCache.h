#pragma once

#include <map>
#include <string>

using namespace std;

class DocPageCache
{
public:
	DocPageCache();
	~DocPageCache();
	
	// Add the ipSS which contains pages nStartPage through nEndPage to the cache
	void add(long nStartPage, long nEndPage, const ISpatialStringPtr& ipSS);

	// return the cache entry for the specifies page range if one exists
	// otherwise NULL is returned
	ISpatialStringPtr find(long nStartPage, long nEndPage);

private:
	//////////////
	// Variables
	//////////////
	map<string, ISpatialStringPtr> m_mapCache;
	
	//////////////
	// Methods
	//////////////
	string getKey(long nStartPage, long nEndPage);
};