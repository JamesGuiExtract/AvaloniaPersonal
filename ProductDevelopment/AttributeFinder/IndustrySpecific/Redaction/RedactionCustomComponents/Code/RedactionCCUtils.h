#pragma once

#include <string>

using namespace std;

class CRedactionCustomComponentsUtils
{
public:
	CRedactionCustomComponentsUtils();
	~CRedactionCustomComponentsUtils();

	// Expand tags using FAM Tag manager and expand utility function
	static const string ExpandTagsAndTFE(IFAMTagManagerPtr ipFAMTM, const string& strFile, 
		const string& strSourceDocName);

	// Set tags for redaction text
	static const string ChooseRedactionTextTag(HWND hwnd, long x, long y);

	// Expand redactions tags
	static const string ExpandRedactionTags(const string& strTagText, 
		const string& strExemptionCodes, const string& strFieldType);
};