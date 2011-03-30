#include "stdafx.h"
#include "AFTagManager.h"
#include <UCLIDException.h>
#include <TextFunctionExpander.h>
#include <QuickMenuChooser.h>
#include <cpputil.h>
#include <ComUtils.h>

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
const std::string AFTagManager::displayTagsForSelection(CWnd* pWnd, long nLeft, long nTop)
{
	//add the functions
	IAFUtilityPtr ipAFUtils(CLSID_AFUtility);
	ASSERT_RESOURCE_ALLOCATION("ELI11999", ipAFUtils != __nullptr);
	
	vector<string> vecChoices;
	int i;
	// Add the built in tags
	IVariantVectorPtr ipVecBuiltIn = ipAFUtils->GetBuiltInTags();
	for (i = 0; i < ipVecBuiltIn->Size; i++)
	{
		_variant_t var = ipVecBuiltIn->GetItem(i);
		string str = asString(var.bstrVal);
		vecChoices.push_back(str);
	}
	// Add a separator
	vecChoices.push_back("");

	// Add the ini file in tags
	IVariantVectorPtr ipVecINI = ipAFUtils->GetINIFileTags();
	for (i = 0; i < ipVecINI->Size; i++)
	{
		_variant_t var = ipVecINI->GetItem(i);
		string str = asString(var.bstrVal);
		vecChoices.push_back(str);
	}
	// Add a separator
	vecChoices.push_back("");

	//add the functions
	TextFunctionExpander tfe;
	vector<string> vecFunctions = tfe.getAvailableFunctions();
	tfe.formatFunctions(vecFunctions);
	addVectors(vecChoices, vecFunctions);

	QuickMenuChooser qmc(vecChoices);
	
	string strChoice = qmc.getChoiceString(pWnd, nLeft, nTop);
	return strChoice;
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
