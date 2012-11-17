#include "stdafx.h"
#include "FileSupplierUtils.h"
#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>

//-------------------------------------------------------------------------------------------------
// Public Methods
//-------------------------------------------------------------------------------------------------
CFileSupplierUtils::CFileSupplierUtils()
{
}
//--------------------------------------------------------------------------------------------------
CFileSupplierUtils::~CFileSupplierUtils()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16558");
}
//--------------------------------------------------------------------------------------------------
const std::string CFileSupplierUtils::ExpandTagsAndTFE(IFAMTagManager *pFAMTM, const string& strFile, const std::string& strSourceDocName)
{
	ITagUtilityPtr ipFAMTagUtility(pFAMTM);
	ASSERT_RESOURCE_ALLOCATION("ELI35243", ipFAMTagUtility != __nullptr);

	string strExpandedFile = asString(
		ipFAMTagUtility->ExpandTagsAndFunctions(strFile.c_str(),
			_bstr_t(strSourceDocName.c_str()).GetBSTR()));

	return strExpandedFile;
}