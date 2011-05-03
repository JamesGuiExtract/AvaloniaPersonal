#include "stdafx.h"
#include "UCLIDFileProcessing.h"
#include "FileProcessingUtils.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <TextFunctionExpander.h>
#include <cpputil.h>
#include <QuickMenuChooser.h>
#include <ComUtils.h>
#include <VectorOperations.h>

//-------------------------------------------------------------------------------------------------
// Public Methods
//-------------------------------------------------------------------------------------------------
CFileProcessingUtils::CFileProcessingUtils()
{
}
//--------------------------------------------------------------------------------------------------
CFileProcessingUtils::~CFileProcessingUtils()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI18000");
}
//--------------------------------------------------------------------------------------------------
const string CFileProcessingUtils::ChooseDocTag(HWND hwnd, long x, long y, 
													 bool bIncludeSourceDocName)
{
	vector<string> vecChoices;

	// Add the built in tags
	IVariantVectorPtr ipVecBuiltInTags = getFAMTagManager()->GetBuiltInTags();
	long lBuiltInSize = ipVecBuiltInTags->Size;
	for (long i = 0; i < lBuiltInSize; i++)
	{
		_variant_t var = ipVecBuiltInTags->Item[i];
		string str = asString(var.bstrVal);
		if (bIncludeSourceDocName || str != "<SourceDocName>")
		{
			vecChoices.push_back(str);
		}
	}
	// Add a separator if there is at
	// least one built in tags
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
	// at least one tag from INI file
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
const string CFileProcessingUtils::ExpandTagsAndTFE(UCLID_FILEPROCESSINGLib::IFAMTagManager *pFAMTM, const string &strFile, const string &strSourceDocName)
{
	//////////////////////////////////////////////////////////////////////////
	// Get the FAMTagManager Pointer and expand tags in m_strFileName, 
	// If the <FPSFiledir> points to C:\RedactionDemo\FPS
	// e.g. m_strFileName = "<FPSFileDir>\123.dat"
	// after expanding: strFile = "C:\RedactionDemo\FPS\123.dat"
	//////////////////////////////////////////////////////////////////////////

	UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipTag = UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr(pFAMTM);
	ASSERT_RESOURCE_ALLOCATION("ELI18001", ipTag != __nullptr);

	// Pass the file name with the tags(strFile) and the source doc name(strSourceDocName) as parameters to Expandtags
	// If file name contains <SourceDocName>, ExpandTags() will use strSourceDocName to expand it [P13: 3901]
	_bstr_t bstrFile = ipTag->ExpandTags( _bstr_t(strFile.c_str()), _bstr_t(strSourceDocName.c_str()) );
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
// Private Methods
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr CFileProcessingUtils::getFAMTagManager()
{
	UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipFAMTagManager(CLSID_FAMTagManager);
	ASSERT_RESOURCE_ALLOCATION("ELI18002", ipFAMTagManager != __nullptr);
	return ipFAMTagManager;
}
//--------------------------------------------------------------------------------------------------