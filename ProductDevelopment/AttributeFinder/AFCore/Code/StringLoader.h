
#pragma once

#include "CachedObjectFromFile.h"

#include <string>
using namespace std;

class StringLoader : public FileObjectLoaderBase
{
public:
	StringLoader(); // Constructor
	~StringLoader(); // Destructor
	// Load one string from a file. (Used for Advanced Replacing string)
	void loadObjectFromFile(string& strFromFile, const string& strFile);
	// Load string list from a file. (Used for Locate image region and replace strings)
	void loadObjectFromFile(IVariantVectorPtr & ipVector, const string& strFile);
private:
	// Define a IMiscUtilsPtr object for general use
	IMiscUtilsPtr m_ipMiscUtils;
};