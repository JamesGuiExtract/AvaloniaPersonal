#pragma once

#include <string>

class CFAMConditionUtils
{
public:
	CFAMConditionUtils();
	~CFAMConditionUtils();

	// Expand tags using FAM Tag manager and expand utility function
	static const std::string ExpandTagsAndTFE(IFAMTagManager * pFAMTM, const std::string& strFile, const std::string& strSourceDocName);
};