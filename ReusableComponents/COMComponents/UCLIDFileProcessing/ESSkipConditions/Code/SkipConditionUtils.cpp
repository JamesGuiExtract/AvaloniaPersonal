#include "stdafx.h"
#include "SkipConditionUtils.h"
#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <TextFunctionExpander.h>
#include <cpputil.h>
#include <QuickMenuChooser.h>
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
const std::string CFAMConditionUtils::ChooseDocTag(HWND hwnd, long x, long y)
{
	std::vector<std::string> vecChoices;

	// Add the built in tags
	IVariantVectorPtr ipVecBuiltInTags = getFAMTagManager()->GetBuiltInTags();
	long lBuiltInSize = ipVecBuiltInTags->Size;
	for (long i = 0; i < lBuiltInSize; i++)
	{
		_variant_t var = ipVecBuiltInTags->Item[i];
		std::string str = asString(var.bstrVal);
		vecChoices.push_back(str);
	}
	// Add a separator if there is at
	// least one build in tags
	if (lBuiltInSize > 0)
	{
		vecChoices.push_back(""); // Separator
	}

	// Add tags in specified ini file
	IVariantVectorPtr ipVecIniTags = getFAMTagManager()->GetINIFileTags();
	long lIniSize = ipVecIniTags->Size;
	for (long i = 0; i < lIniSize; i++)
	{
		_variant_t var = ipVecIniTags->Item[i];
		std::string str = asString(var.bstrVal);
		vecChoices.push_back(str);
	}
	// Add a separator if there is
	// at least one tags from INI file
	if (lIniSize > 0)
	{
		vecChoices.push_back(""); // Separator
	}

	// Add utility functions
	TextFunctionExpander tfe;
	std::vector<std::string> vecFunctions = tfe.getAvailableFunctions();
	tfe.formatFunctions(vecFunctions);
	addVectors(vecChoices, vecFunctions); // add the functions

	QuickMenuChooser qmc;
	qmc.setChoices(vecChoices);

	return qmc.getChoiceString(CWnd::FromHandle(hwnd), x, y);
}
//--------------------------------------------------------------------------------------------------
const std::string CFAMConditionUtils::ExpandTagsAndTFE(IFAMTagManager *pFAMTM, const string& strFile, const std::string& strSourceDocName)
{
	//////////////////////////////////////////////////////////////////////////
	// Get the FAMTagManager Pointer and expand tags in strFile, 
	// e.g. if source file name is C:\123.tif,
	// strFile = "$DirOf(<SourceDocName>)\$FileOf(<SourceDocName>).voa"
	// after expanding: strExpandedFile = "$DirOf(C:\123.tif)\$FileOf(C:\123.tif).voa"
	//////////////////////////////////////////////////////////////////////////

	IFAMTagManagerPtr ipTag = IFAMTagManagerPtr(pFAMTM);
	ASSERT_RESOURCE_ALLOCATION("ELI14403", ipTag != __nullptr);

	// Pass the file name with the tags(strFile) and the source doc name(strSourceDocName) as parameters to Expandtags
	// If file name contains <SourceDocName>, ExpandTags() will use strSourceDocName to expand it [P13: 3901]
	_bstr_t bstrFile = ipTag->ExpandTags(_bstr_t(strFile.c_str()), _bstr_t(strSourceDocName.c_str()));
	string strExpandedFile = asString(bstrFile);

	////////////////////////////////////////////////////////////////////////////////////
	// Expand function in strExpandedFile
	// e.g. Before expanding: strExpandedFile = "$DirOf(C:\123.tif)\$FileOf(C:\123.tif).voa"
	// After expanding: strExpandedFile = "C:\123.tif.voa"
	////////////////////////////////////////////////////////////////////////////////////

	TextFunctionExpander tfe;
	strExpandedFile = tfe.expandFunctions(strExpandedFile); 

	return strExpandedFile;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
IFAMTagManagerPtr CFAMConditionUtils::getFAMTagManager()
{
	IFAMTagManagerPtr ipFAMTagManager(CLSID_FAMTagManager);
	ASSERT_RESOURCE_ALLOCATION("ELI14404", ipFAMTagManager != __nullptr);
	return ipFAMTagManager;
}
//--------------------------------------------------------------------------------------------------