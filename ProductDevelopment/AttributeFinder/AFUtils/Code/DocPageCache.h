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
	void add(long nStartPage, long nEndPage, const string& strText);

	// Find the cache entry for the specified range and place it in rstrText if
	// it exists.  Return true if found, false if not found
	// otherwise NULL is returned
	bool find(long nStartPage, long nEndPage, string& rstrText);

private:
	//////////////
	// Variables
	//////////////
	map<string, string> m_mapCache;
	
	//////////////
	// Methods
	//////////////
	string getKey(long nStartPage, long nEndPage);
};