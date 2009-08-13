
#pragma once

#include <string>

class StringLoader
{
public:
	StringLoader(); // Constructor
	~StringLoader(); // Destructor
	// Load one string from a file. (Used for Advanced Replacing string)
	void loadObjectFromFile(std::string& strFromFile, const std::string& strFile);
	// Load string list from a file. (Used for Locate image region and replace strings)
	void loadObjectFromFile(IVariantVectorPtr & ipVector, const std::string& strFile);
private:
	// Define a IMiscUtilsPtr object for general use
	IMiscUtilsPtr m_ipMiscUtils;
};