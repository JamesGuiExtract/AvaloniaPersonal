// DocumentSorter.cpp : Implementation of CDocumentSorter
#include "stdafx.h"
#include "AFUtils.h"
#include "DocumentSorter.h"
#include "SpecialStringDefinitions.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <comutils.h>
#include <ComponentLicenseIDs.h>

#include <fstream>
#include <io.h>

#define CRTDBG_MAP_ALLOC
#include <stdlib.h>
#include <crtdbg.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// CDocumentSorter
//-------------------------------------------------------------------------------------------------
CDocumentSorter::CDocumentSorter()
: m_ipOCRUtils(NULL),
  m_ipOCREngine(NULL),
  m_ipDocClassifier(NULL)
{
}
//-------------------------------------------------------------------------------------------------
CDocumentSorter::~CDocumentSorter()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16332");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocumentSorter::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IDocumentSorter,
		&IID_ILicensedComponent
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IDocumentSorter
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocumentSorter::SortDocuments(BSTR strInputFolder, 
											BSTR strOutputFolder, 
											BSTR strDocIndustryName,
											VARIANT_BOOL bOCRImages)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// make sure the folder exists and can be written to
		string strInputFolderName = asString(strInputFolder);
		if (!isValidFolder(strInputFolderName) ||
			!canCreateFile(strInputFolderName))
		{
			// if the folder doesn't exist or can't be read/write
			UCLIDException ue("ELI06279", "The folder either doesn't exist,"
				" or you don't have the read/write permission to the folder.");
			ue.addDebugInfo("Folder", strInputFolderName);
			throw ue;
		}

		if (bOCRImages == VARIANT_TRUE)
		{
			// do a batch ocr first if required
			batchOCR(strInputFolder);
		}

		// now look into this input folder, run classification for each .txt file
		vector<string> vecTextFiles;
		// Since batch ocr generates text files as OCR output files,
		// get all .uss files in that directory for classification
		::getFilesInDir(vecTextFiles, strInputFolderName, "*.uss", true);
		
		string strCategoryName = asString(strDocIndustryName);
		string strOutputRootFolder = asString(strOutputFolder);
		// process each file
		for (unsigned int ui = 0; ui < vecTextFiles.size(); ui++)
		{
			sortFile(vecTextFiles[ui], strCategoryName, strOutputRootFolder);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06275")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocumentSorter::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pbValue == NULL)
		{
			return E_POINTER;
		}

		// Check license
		validateLicense();

		// If no exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// private / helper methods
//-------------------------------------------------------------------------------------------------
void CDocumentSorter::batchOCR(BSTR strInputFolder)
{
	// use OCRUtils to run the batch OCR first
	if (m_ipOCRUtils == __nullptr)
	{
		m_ipOCRUtils.CreateInstance(CLSID_OCRUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI06277", m_ipOCRUtils != __nullptr);
	}

	if (!m_ipOCREngine)
	{	
		m_ipOCREngine.CreateInstance(CLSID_ScansoftOCR);
		ASSERT_RESOURCE_ALLOCATION("ELI06278", m_ipOCREngine != __nullptr);
		
		_bstr_t _bstrPrivateLicenseCode = LICENSE_MGMT_PASSWORD.c_str();
		IPrivateLicensedComponentPtr ipScansoftEngine(m_ipOCREngine);
		ipScansoftEngine->InitPrivateLicense(_bstrPrivateLicenseCode);
	}
				
	// process first 2 pages for each image, no recursion necessary,
	// go ahead and overwrite any existing TXT files
	m_ipOCRUtils->BatchOCR(strInputFolder, m_ipOCREngine, VARIANT_FALSE, 2, 
		VARIANT_TRUE, VARIANT_FALSE, VARIANT_FALSE, NULL);
}
//-------------------------------------------------------------------------------------------------
void CDocumentSorter::moveFile(const string& strUSSFileName, 
							   const string& strMoveToFolder)
{
	// create the directory if not exists
	if (!::directoryExists(strMoveToFolder))
	{
		::createDirectory(strMoveToFolder);
	}

	vector<string> vecAllSpecifiedFiles;
	string strFolder = ::getDirectoryFromFullPath(strUSSFileName) + "\\";
	string strFileNameWithoutExtension = ::getFileNameWithoutExtension(strUSSFileName);
	string strFilesToFind = strFileNameWithoutExtension + ".*";

	::getFilesInDir(vecAllSpecifiedFiles, strFolder, strFilesToFind, false);

	for (unsigned int ui = 0; ui < vecAllSpecifiedFiles.size(); ui++)
	{
		string strFileToMove = vecAllSpecifiedFiles[ui];
		string strFileNameOnly = ::getFileNameFromFullPath(strFileToMove);
		string strNewFileName = strMoveToFolder + "\\" + strFileNameOnly;

		::MoveFileEx(strFileToMove.c_str(), strNewFileName.c_str(), MOVEFILE_REPLACE_EXISTING);
	}
}
//-------------------------------------------------------------------------------------------------
void CDocumentSorter::sortFile(const string& strUSSFileName, 
							   const string& strCategoryName,
							   const string& strOutputRootFolder)
{
	// create a new AFDocument for holding the value and tags
	IAFDocumentPtr ipAFDoc(CLSID_AFDocument);
	ASSERT_RESOURCE_ALLOCATION("ELI06280", ipAFDoc != __nullptr);

	ISpatialStringPtr ipText = ipAFDoc->Text;
	ipText->LoadFrom(_bstr_t(strUSSFileName.c_str()), VARIANT_FALSE);
	
	if (m_ipDocClassifier == __nullptr)
	{
		m_ipDocClassifier.CreateInstance(CLSID_DocumentClassifier);
		ASSERT_RESOURCE_ALLOCATION("ELI06281", m_ipDocClassifier != __nullptr);
	}

	m_ipDocClassifier->IndustryCategoryName = _bstr_t(strCategoryName.c_str());
	IDocumentPreprocessorPtr ipDocPreprocessor(m_ipDocClassifier);
	// run the AFDoc through the preprocessor to create tags on the doc
	ipDocPreprocessor->Process(ipAFDoc, NULL);

	string strMoveToFolder = strOutputRootFolder + "\\";
	// a flag to indicate whether or not to move current 
	// file to the unclassified folder
	bool bMoveToUnclassified = true;

	// analyze the tag
	IStrToStrMapPtr ipStringTags = ipAFDoc->StringTags;
	if (ipStringTags != __nullptr && ipStringTags->Size > 0)
	{
		// the actual probability created by Document Classifier
		string strActualProbability = asString(ipStringTags->GetValue(_bstr_t(DOC_PROBABILITY.c_str())));
		
		// if actual probaility is zero, then this document shall be
		// moved to "Unclassified" folder
		if (strActualProbability != "0")
		{	
			// the actual document type name
			IStrToObjectMapPtr ipObjTags = ipAFDoc->ObjectTags;
			if (ipObjTags != __nullptr && ipObjTags->Size > 0)
			{
				// get found document types
				IVariantVectorPtr ipDocTypes = ipObjTags->GetValue(_bstr_t(DOC_TYPE.c_str()));
				if (ipDocTypes != __nullptr && ipDocTypes->Size == 1)
				{
					// if the actual document type is of one and only one type
					// then we shall move the file to the folder has the name of
					// the document type
					string strActualDocType = asString(_bstr_t(ipDocTypes->GetItem(0)));
					strMoveToFolder += strActualDocType;
					
					bMoveToUnclassified = false;
				}
			}
		}
	}

	if (bMoveToUnclassified)
	{
		// otherwise, this document shall be moved to the "Unclassified" folder
		strMoveToFolder += "Unclassified";
	}

	moveFile(strUSSFileName, strMoveToFolder);
}
//-------------------------------------------------------------------------------------------------
void CDocumentSorter::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI06274", "Document Sorter");
}
//-------------------------------------------------------------------------------------------------
