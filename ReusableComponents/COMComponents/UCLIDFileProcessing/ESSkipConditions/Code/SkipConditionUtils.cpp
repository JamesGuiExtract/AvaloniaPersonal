#include "stdafx.h"
#include "SkipConditionUtils.h"
#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <TextFunctionExpander.h>
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