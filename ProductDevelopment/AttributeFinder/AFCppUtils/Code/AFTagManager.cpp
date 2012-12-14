#include "stdafx.h"
#include "AFTagManager.h"
#include <UCLIDException.h>
#include <QuickMenuChooser.h>
#include <cpputil.h>
#include <ComUtils.h>
#include <VectorOperations.h>

//-------------------------------------------------------------------------------------------------
AFTagManager::AFTagManager()
{
}
//-------------------------------------------------------------------------------------------------
AFTagManager::~AFTagManager()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16308");
}
//-------------------------------------------------------------------------------------------------
const std::string AFTagManager::expandTagsAndFunctions(const std::string& strText, IAFDocumentPtr ipAFDoc)
{
	ITagUtilityPtr ipTagExpander(getAFUtility());
	ASSERT_RESOURCE_ALLOCATION("ELI35162", ipTagExpander != __nullptr);

	// Expand tags and functions in strText
	string strOut =
		asString(ipTagExpander->ExpandTagsAndFunctions(get_bstr_t(strText), "", ipAFDoc));

	return strOut;
}
//-------------------------------------------------------------------------------------------------
void AFTagManager::validateDynamicFilePath(const std::string& eliCode, std::string strValue)
{
	IMiscUtilsPtr ipMiscUtils(CLSID_MiscUtils);
	ASSERT_RESOURCE_ALLOCATION("ELI33662", ipMiscUtils != __nullptr);

	// This is the prefix for dynamic files.
	string strFileHeader = asString(ipMiscUtils->GetFileHeader());

	// If the value has a dynamic file header, ensure the path is an explicit (not relative) path.
	if (strValue.find(strFileHeader) == 0)
	{
		string strFileWithoutHeader =
			asString(ipMiscUtils->GetFileNameWithoutHeader(_bstr_t(strValue.c_str())));

		IAFUtilityPtr ipAFUtility(CLSID_AFUtility);
		ASSERT_RESOURCE_ALLOCATION("ELI33846", ipAFUtility != __nullptr);

		ipAFUtility->ValidateAsExplicitPath(eliCode.c_str(), strFileWithoutHeader.c_str());
	}
}
//-------------------------------------------------------------------------------------------------
void AFTagManager::validateDynamicFilePath(const std::string& eliCode, IVariantVectorPtr ipList)
{
	ASSERT_ARGUMENT("ELI33652", ipList != __nullptr);

	IMiscUtilsPtr ipMiscUtils(CLSID_MiscUtils);
	ASSERT_RESOURCE_ALLOCATION("ELI33651", ipMiscUtils != __nullptr);

	// This is the prefix for dynamic files.
	string strFileHeader = asString(ipMiscUtils->GetFileHeader());

	int nSize = ipList->Size;
	for (int i = 0; i < nSize; i++)
	{
		_bstr_t bstrValue = ipList->GetItem(i);
		string strValue = asString(bstrValue);

		// If the value has a dynamic file header, ensure the path is an explicit (not relative) path.
		if (strValue.find(strFileHeader) == 0)
		{
			string strFileWithoutHeader =
				asString(ipMiscUtils->GetFileNameWithoutHeader(_bstr_t(strValue.c_str())));

			IAFUtilityPtr ipAFUtility(CLSID_AFUtility);
			ASSERT_RESOURCE_ALLOCATION("ELI33852", ipAFUtility != __nullptr);

			ipAFUtility->ValidateAsExplicitPath(eliCode.c_str(), strFileWithoutHeader.c_str());
		}
	}
}

//-------------------------------------------------------------------------------------------------
// Private
//-------------------------------------------------------------------------------------------------
IAFUtilityPtr AFTagManager::getAFUtility()
{
	IAFUtilityPtr ipAFUtility(CLSID_AFUtility);
	ASSERT_RESOURCE_ALLOCATION("ELI12004", ipAFUtility != __nullptr);
	return ipAFUtility;
	
}
