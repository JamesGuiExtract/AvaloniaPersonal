#pragma once

#include <string>

class CFAMConditionUtils
{
public:
	CFAMConditionUtils();
	~CFAMConditionUtils();
	// Set tags for FAM condition dialog
	static const std::string ChooseDocTag(HWND hwnd, long x, long y);
	// Expand tags using FAM Tag manager and expand utility function
	static const std::string ExpandTagsAndTFE(IFAMTagManager * pFAMTM, const std::string& strFile, const std::string& strSourceDocName);
private:
	// return IFAMTagManager pointer
	static IFAMTagManagerPtr getFAMTagManager();
};