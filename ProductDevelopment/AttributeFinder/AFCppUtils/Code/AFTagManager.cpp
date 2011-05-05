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
// Private
//-------------------------------------------------------------------------------------------------
IAFUtilityPtr AFTagManager::getAFUtility()
{
	IAFUtilityPtr ipAFUtility(CLSID_AFUtility);
	ASSERT_RESOURCE_ALLOCATION("ELI12004", ipAFUtility != __nullptr);
	return ipAFUtility;
	
}
