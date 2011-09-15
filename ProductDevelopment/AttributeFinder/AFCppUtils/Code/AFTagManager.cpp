#include "stdafx.h"
#include "AFTagManager.h"
#include <UCLIDException.h>
#include <TextFunctionExpander.h>
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
	
	string strOut = strText;
	// Expand tags in the name
	_bstr_t _bstr = getAFUtility()->ExpandTags(get_bstr_t(strOut), ipAFDoc);
	strOut = _bstr;
	// Expand functions in the name
	TextFunctionExpander tfe;
	strOut = tfe.expandFunctions(strOut);
	return strOut;
}
//-------------------------------------------------------------------------------------------------
void AFTagManager::validateAsExplicitPath(const std::string& eliCode, const std::string& strFilename)
{
	IAFUtilityPtr ipAFUtility(CLSID_AFUtility);
	ASSERT_RESOURCE_ALLOCATION("ELI33646", ipAFUtility != __nullptr);

	// If the filename contains tags, consider it valid.
	if(ipAFUtility->StringContainsTags(strFilename.c_str()) == VARIANT_FALSE)
	{
		// If it doesn't contain tags, confirm this is a valid absolute path.
		// If the name isn't at least 2 characters, it can't be a valid absolute path.
		if (strFilename.length() <= 2)
		{
			UCLIDException ue(eliCode, "Please specify a valid file name!");
			ue.addDebugInfo("File", strFilename);
			ue.addWin32ErrorInfo();
			throw ue;
		}

		string strRoot = strFilename.substr(0, 2);

		// An absolute path must begin with either a drive letter or double-backslash.
		if (strRoot != "\\\\" && (!isalpha(strRoot[0]) || strRoot[1] != ':'))
		{
			UCLIDException ue(eliCode, "Explicit path required. Use a path tag or absolute path.");
			ue.addDebugInfo("File", strFilename);
			ue.addWin32ErrorInfo();
			throw ue;
		}
		// Ensure that the file exists
		else if (!isValidFile(strFilename))
		{
			UCLIDException ue(eliCode, "Specified file does not exist!");
			ue.addDebugInfo("File", strFilename);
			ue.addWin32ErrorInfo();
			throw ue;
		}
	}
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

		AFTagManager::validateAsExplicitPath(eliCode, strFileWithoutHeader);
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

			AFTagManager::validateAsExplicitPath(eliCode, strFileWithoutHeader);
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
