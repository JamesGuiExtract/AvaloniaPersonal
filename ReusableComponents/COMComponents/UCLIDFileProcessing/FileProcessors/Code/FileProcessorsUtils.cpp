#include "stdafx.h"
#include "FileProcessorsUtils.h"
#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <cpputil.h>

//-------------------------------------------------------------------------------------------------
// Public Methods
//-------------------------------------------------------------------------------------------------
CFileProcessorsUtils::CFileProcessorsUtils()
{
}
//--------------------------------------------------------------------------------------------------
CFileProcessorsUtils::~CFileProcessorsUtils()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16439");
}
//--------------------------------------------------------------------------------------------------
const string CFileProcessorsUtils::ExpandTagsAndTFE(IFAMTagManager *pFAMTM, const string& strFile, const string& strSourceDocName)
{
	ITagUtilityPtr ipFAMTagUtility(pFAMTM);
	ASSERT_RESOURCE_ALLOCATION("ELI35245", ipFAMTagUtility != __nullptr);

	string strExpandedFile = asString(
		ipFAMTagUtility->ExpandTagsAndFunctions(strFile.c_str(), _bstr_t(strSourceDocName.c_str()).GetBSTR(), __nullptr));

	return strExpandedFile;
}