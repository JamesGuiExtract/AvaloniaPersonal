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
	//////////////////////////////////////////////////////////////////////////
	// Get the FAMTagManager Pointer and expand tags in m_strFileName, 
	// If the <FPSFiledir> points to C:\RedactionDemo\FPS
	// e.g. m_strFileName = "<FPSFileDir>\123.dat"
	// after expanding: strFile = "C:\RedactionDemo\FPS\123.dat"
	//////////////////////////////////////////////////////////////////////////

	IFAMTagManagerPtr ipTag = IFAMTagManagerPtr(pFAMTM);
	ASSERT_RESOURCE_ALLOCATION("ELI14415", ipTag != __nullptr);

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