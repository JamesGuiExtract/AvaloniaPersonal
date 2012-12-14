#include "stdafx.h"
#include "SkipConditionUtils.h"
#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>

//-------------------------------------------------------------------------------------------------
// Public Methods
//-------------------------------------------------------------------------------------------------
CFAMConditionUtils::CFAMConditionUtils()
{
}
//--------------------------------------------------------------------------------------------------
CFAMConditionUtils::~CFAMConditionUtils()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16567");
}
//--------------------------------------------------------------------------------------------------
const std::string CFAMConditionUtils::ExpandTagsAndTFE(IFAMTagManager *pFAMTM, const string& strFile, const std::string& strSourceDocName)
{
	ITagUtilityPtr ipFAMTagUtility(pFAMTM);
	ASSERT_RESOURCE_ALLOCATION("ELI35244", ipFAMTagUtility != __nullptr);

	string strExpandedFile = asString(
		ipFAMTagUtility->ExpandTagsAndFunctions(strFile.c_str(), _bstr_t(strSourceDocName.c_str()).GetBSTR(), __nullptr));

	return strExpandedFile;
}