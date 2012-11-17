#include "stdafx.h"
#include "AFFileProcessorsUtils.h"
#include <LicenseMgmt.h>
#include <ComUtils.h>

//-------------------------------------------------------------------------------------------------
// Public Methods
//-------------------------------------------------------------------------------------------------
CAFFileProcessorsUtils::CAFFileProcessorsUtils()
{
}
//--------------------------------------------------------------------------------------------------
CAFFileProcessorsUtils::~CAFFileProcessorsUtils()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16309");
}
//--------------------------------------------------------------------------------------------------
const std::string CAFFileProcessorsUtils::ExpandTagsAndTFE(IFAMTagManagerPtr ipFAMTM, const string& strFile, const std::string& strSourceDocName)
{
	ITagUtilityPtr ipFAMTagUtility(ipFAMTM);
	ASSERT_RESOURCE_ALLOCATION("ELI35163", ipFAMTagUtility != __nullptr);

	string strExpandedFile = asString(
		ipFAMTagUtility->ExpandTagsAndFunctions(strFile.c_str(), _bstr_t(strSourceDocName.c_str()).GetBSTR()));

	return strExpandedFile;
}