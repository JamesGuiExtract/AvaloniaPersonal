#include "stdafx.h"
#include "UCLIDFileProcessing.h"
#include "FileProcessingUtils.h"

#include <LicenseMgmt.h>
#include <cpputil.h>
#include <ComUtils.h>

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
const string CFileProcessingUtils::ExpandTagsAndTFE(UCLID_FILEPROCESSINGLib::IFAMTagManager *pFAMTM,
	const string &strFile, const string &strSourceDocName)
{
	//////////////////////////////////////////////////////////////////////////
	// Get the FAMTagManager Pointer and expand tags in m_strFileName, 
	// If the <FPSFiledir> points to C:\RedactionDemo\FPS
	// e.g. m_strFileName = "<FPSFileDir>\123.dat"
	// after expanding: strFile = "C:\RedactionDemo\FPS\123.dat"
	//////////////////////////////////////////////////////////////////////////

	IMiscUtilsPtr ipMiscUtils(CLSID_MiscUtils);
	ASSERT_RESOURCE_ALLOCATION("ELI35241", ipMiscUtils != __nullptr);

	ITagUtilityPtr ipTag(pFAMTM);
	ASSERT_RESOURCE_ALLOCATION("ELI35242", ipTag != __nullptr);

	// Pass the file name with the tags(strFile) and the source doc name(strSourceDocName) as parameters to Expandtags
	// If file name contains <SourceDocName>, ExpandTags() will use strSourceDocName to expand it [P13: 3901]
	_bstr_t bstrFile = ipMiscUtils->ExpandTagsAndFunctions(_bstr_t(strFile.c_str()), ipTag,
		_bstr_t(strSourceDocName.c_str()).Detach(), __nullptr);
	string strExpandedFile = asString(bstrFile);

	return strExpandedFile;
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingUtils::addStatusInComboBox(CComboBox& comboStatus)
{
	// Insert the action status to the ComboBox
	// The items are inserted the same order as the EActionStatus in FAM
	comboStatus.InsertString(0, "Unattempted");
	comboStatus.InsertString(1, "Pending");
	comboStatus.InsertString(2, "Processing");
	comboStatus.InsertString(3, "Completed");
	comboStatus.InsertString(4, "Failed");
	comboStatus.InsertString(5, "Skipped");
}