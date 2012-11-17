#include "stdafx.h"
#include "RedactionCCUtils.h"
#include "RedactionCCConstants.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <QuickMenuChooser.h>
#include <ComUtils.h>
#include <VectorOperations.h>

#include <string>
#include <vector>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Public Methods
//-------------------------------------------------------------------------------------------------
CRedactionCustomComponentsUtils::CRedactionCustomComponentsUtils()
{
}
//--------------------------------------------------------------------------------------------------
CRedactionCustomComponentsUtils::~CRedactionCustomComponentsUtils()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16480");
}
//--------------------------------------------------------------------------------------------------
const string CRedactionCustomComponentsUtils::ExpandTagsAndTFE(IFAMTagManagerPtr ipFAMTM, const string& strFile, const string& strSourceDocName)
{
	ITagUtilityPtr ipFAMTagUtility(ipFAMTM);
	ASSERT_RESOURCE_ALLOCATION("ELI35182", ipFAMTagUtility != __nullptr);

	string strExpandedFile = asString(
		ipFAMTagUtility->ExpandTagsAndFunctions(strFile.c_str(),
			_bstr_t(strSourceDocName.c_str()).GetBSTR()));

	return strExpandedFile;
}
//-------------------------------------------------------------------------------------------------
const string CRedactionCustomComponentsUtils::ChooseRedactionTextTag(HWND hwnd, long x, long y)
{
	// Create a vector with the appropriate tags
	vector<string> vecstrTags;
	vecstrTags.push_back(gstrEXEMPTION_CODES_TAG);
	vecstrTags.push_back(gstrFIELD_TYPE_TAG);

	// Let the user choose the appropriate tag
	QuickMenuChooser qmc;
	qmc.setChoices(vecstrTags);
	return qmc.getChoiceString(CWnd::FromHandle(hwnd), x, y);
}
//-------------------------------------------------------------------------------------------------
const string CRedactionCustomComponentsUtils::ExpandRedactionTags(const string& strTagText, 
	const string& strExemptionCodes, const string& strFieldType)
{
	// Replace the tags
	string strResult = strTagText;
	if (!strResult.empty())
	{
		replaceVariable(strResult, gstrEXEMPTION_CODES_TAG, strExemptionCodes);
		replaceVariable(strResult, gstrFIELD_TYPE_TAG, strFieldType);
	}

	// Return the result
	return strResult;
}
