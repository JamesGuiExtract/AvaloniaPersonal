#include "stdafx.h"
#include "RedactionCCUtils.h"
#include "RedactionCCConstants.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <TextFunctionExpander.h>
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
const string CRedactionCustomComponentsUtils::ChooseDocTag(HWND hwnd, long x, long y)
{
	vector<string> vecChoices;

	// Add the built in tags
	IVariantVectorPtr ipVecBuiltInTags = getFAMTagManager()->GetBuiltInTags();
	long lBuiltInSize = ipVecBuiltInTags->Size;
	for (long i = 0; i < lBuiltInSize; i++)
	{
		_variant_t var = ipVecBuiltInTags->Item[i];
		string str = asString(var.bstrVal);
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
		string str = asString(var.bstrVal);
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
	vector<string> vecFunctions = tfe.getAvailableFunctions();
	tfe.formatFunctions(vecFunctions);
	addVectors(vecChoices, vecFunctions); // add the functions

	QuickMenuChooser qmc;
	qmc.setChoices(vecChoices);

	return qmc.getChoiceString(CWnd::FromHandle(hwnd), x, y);
}
//--------------------------------------------------------------------------------------------------
const string CRedactionCustomComponentsUtils::ExpandTagsAndTFE(IFAMTagManagerPtr ipFAMTM, const string& strFile, const string& strSourceDocName)
{
	// verify valid arguments
	ASSERT_ARGUMENT("ELI15152", ipFAMTM != __nullptr);

	//////////////////////////////////////////////////////////////////////////
	// Get the FAMTagManager Pointer and expand tags in m_strFileName, 
	// If the <FPSFiledir> points to C:\RedactionDemo\FPS
	// e.g. m_strFileName = "<FPSFileDir>\123.dat"
	// after expanding: strFile = "C:\RedactionDemo\FPS\123.dat"
	//////////////////////////////////////////////////////////////////////////

	// Pass the file name with the tags(strFile) and the source doc name(strSourceDocName) as parameters to Expandtags
	// If file name contains <SourceDocName>, ExpandTags() will use strSourceDocName to expand it [P13: 3901]
	_bstr_t bstrFile = ipFAMTM->ExpandTags( _bstr_t(strFile.c_str()), _bstr_t(strSourceDocName.c_str()) );
	string strExpandedFile = asString(bstrFile);

	////////////////////////////////////////////////////////////////////////////
	// Expand function in strFile
	// e.g. Before expanding: strFile = "$DirOf(C:\123.dat)\$FileOf(C:\123.dat)"
	// After expanding: strFileName = "C:\123.dat"
	////////////////////////////////////////////////////////////////////////////

	TextFunctionExpander tfe;
	strExpandedFile = tfe.expandFunctions(strExpandedFile); 

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

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
IFAMTagManagerPtr CRedactionCustomComponentsUtils::getFAMTagManager()
{
	IFAMTagManagerPtr ipFAMTagManager(CLSID_FAMTagManager);
	ASSERT_RESOURCE_ALLOCATION("ELI15010", ipFAMTagManager != __nullptr);
	return ipFAMTagManager;
}
//-------------------------------------------------------------------------------------------------
