// OCRFileProcessor.cpp : Implementation of COCRFileProcessor
#include "stdafx.h"
#include "FileProcessors.h"
#include "OCRFileProcessor.h"
#include "FileProcessorsUtils.h"

#include <UCLIDException.h>
#include <TemporaryFileName.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <TemporaryResourceOverride.h>
#include <misc.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>

#include <io.h>
#include <cmath>
#include <strstream>
#include <fstream>
#include <string>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

// global variables / static variables / externs
extern CComModule _Module;
//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 3;

// string for the MessageBox that will be displayed if opening an Object from a pre 6.0
// release of the software
const string gstrVERSION5_OCR_WARNING = "As of release 6.0 all tasks following the OCR document task "
										"will be\npassed the original source document name, not "
										"<SourceDocName>.uss.\nIf you have $FileNoExtOf(<SourceDocName>) "
										"in one of your tasks to\nstrip the .uss from a file after "
										"OCRing, you must remove this from\nyour task.";

// string for the title of the MessageBox that will be displayed if opening an Object from
// a pre 6.0 release of the software
const string gstrVERSION5_OCR_WARNING_LABEL = "Important Change To OCR Document Task!";

//--------------------------------------------------------------------------------------------------
// COCRFileProcessor
//--------------------------------------------------------------------------------------------------
COCRFileProcessor::COCRFileProcessor()
: m_bDirty(false),
  m_eOCRPageRangeType(UCLID_FILEPROCESSORSLib::kOCRAll),
  m_bUseCleanedImageIfAvailable(true),
  m_strSpecificPages("")
{
	try
	{
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI11043")
}
//--------------------------------------------------------------------------------------------------
COCRFileProcessor::~COCRFileProcessor()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI11044")
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ILicensedComponent,
		&IID_ICategorizedComponent,
		&IID_IFileProcessingTask,
		&IID_ICopyableObject,
		&IID_IPersistStream,
		&IID_IMustBeConfiguredObject,
		&IID_IOCRFileProcessor,
		&IID_IAccessRequired
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (pbValue == NULL)
		return E_POINTER;

	try
	{
		// validate license
		validateLicense();
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// IFileProcessingTask
//--------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::raw_Init(long nActionID, IFAMTagManager* pFAMTM,
	IFileProcessingDB *pDB)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17786");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::raw_ProcessFile(IFileRecord* pFileRecord, long nActionID,
	IFAMTagManager *pTagManager, IFileProcessingDB *pDB, IProgressStatus *pProgressStatus,
	VARIANT_BOOL bCancelRequested, EFileProcessingResult *pResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI17923", pResult != NULL);
		IFileRecordPtr ipFileRecord(pFileRecord);
		ASSERT_ARGUMENT("ELI31332", ipFileRecord != __nullptr);

		string strImageFileName = asString(ipFileRecord->Name);

		// Default to successful completion
		*pResult = kProcessingSuccessful;

		// verify the file existence
		::validateFileOrFolderExistence(strImageFileName);

		// append .uss to the image file name
		string strOutputFileName = strImageFileName + ".uss";

		// get the image to OCR
		string strImageToOcr = m_bUseCleanedImageIfAvailable ?
			getCleanImageNameIfExists(strImageFileName) : strImageFileName;

		ISpatialStringPtr ipSS = NULL;

		// based on the type
		switch (m_eOCRPageRangeType)
		{
		case UCLID_FILEPROCESSORSLib::kOCRAll:
			{
				ipSS = getOCREngine()->RecognizeTextInImage(strImageToOcr.c_str(), 
					1, -1, UCLID_RASTERANDOCRMGMTLib::kNoFilter, "", 
					UCLID_RASTERANDOCRMGMTLib::kRegistry, VARIANT_TRUE, pProgressStatus);
			}
			break;
		case UCLID_FILEPROCESSORSLib::kOCRSpecifiedPages:
			{
				ipSS = getOCREngine()->RecognizeTextInImage2(strImageToOcr.c_str(), 
					get_bstr_t(m_strSpecificPages), VARIANT_TRUE, pProgressStatus);
			}
			break;
		default:
			THROW_LOGIC_ERROR_EXCEPTION("ELI28161");
			break;
		}
		ASSERT_RESOURCE_ALLOCATION("ELI28150", ipSS != NULL);

		// Ensure source doc name is original image file if a clean image was used
		if (m_bUseCleanedImageIfAvailable)
		{
			ipSS->SourceDocName = strImageFileName.c_str();
		}

		// OutputFileName = InputFileName.uss even if cleaned image was used
		ipSS->SaveTo(strOutputFileName.c_str(), VARIANT_TRUE, VARIANT_TRUE);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11047")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::raw_Cancel()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17785");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::raw_Close()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		// Free the OCR engine from memory
		m_ipOCREngine = NULL;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11139");
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IAccessRequired interface implementation
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::raw_RequiresAdminAccess(VARIANT_BOOL* pbResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI31189", pbResult != __nullptr);

		*pbResult = VARIANT_FALSE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31190");
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19614", pstrComponentDescription != NULL);

		*pstrComponentDescription = _bstr_t("Core: OCR document").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11031")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_FILEPROCESSORSLib::IOCRFileProcessorPtr ipCopyThis(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI11032", ipCopyThis != NULL);
		
		// copy data members
		m_eOCRPageRangeType = (UCLID_FILEPROCESSORSLib::EOCRFPPageRangeType)ipCopyThis->OCRPageRangeType;
		m_strSpecificPages = ipCopyThis->SpecificPages;
		m_bUseCleanedImageIfAvailable = asCppBool(ipCopyThis->UseCleanedImage);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12810");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_OCRFileProcessor);
		ASSERT_RESOURCE_ALLOCATION("ELI11033", ipObjCopy != NULL);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11034");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_OCRFileProcessor;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
// Version 3: added ability to use cleaned image if available
STDMETHODIMP COCRFileProcessor::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// reset member variables
		m_eOCRPageRangeType = UCLID_FILEPROCESSORSLib::kOCRAll;
		m_strSpecificPages = "";
		m_bUseCleanedImageIfAvailable = false;

		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read(&nDataLength, sizeof(nDataLength), NULL);
		ByteStream data(nDataLength);
		pStream->Read(data.getData(), nDataLength, NULL);
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// Check for newer version
		if (nDataVersion > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue("ELI11035", "Unable to load newer OCRFileProcessor component.");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}
		
		// check to see if the object we are processing was created with a 
		// pre 6.0 release of the software. if it is, we set bNewDirtyFlag
		// to true
		bool bNewDirtyFlag = false;
		if (nDataVersion == 1)
		{
			// display a message box with our warning message and title and the icon set
			// to warning
			MessageBox(NULL, gstrVERSION5_OCR_WARNING.c_str(), gstrVERSION5_OCR_WARNING_LABEL.c_str(),
				MB_ICONWARNING | MB_OK);
			
			// change the bNewDirtyFlag to true because we are processing an old version of
			// the object
			bNewDirtyFlag = true;
		}

		// load data members from stream
		long tmp;
		dataReader >> tmp;
		m_eOCRPageRangeType = (UCLID_FILEPROCESSORSLib::EOCRFPPageRangeType)tmp;
		dataReader >> m_strSpecificPages;
		
		// if version 3 and higher then read m_bUseCleanedImageIfAvailable
		if (nDataVersion >= 3)
		{
			dataReader >> m_bUseCleanedImageIfAvailable;
		}

		// set the dirty flag
		m_bDirty = bNewDirtyFlag;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11036");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		// write data members to stream
		dataWriter << gnCurrentVersion;
		dataWriter << (long)m_eOCRPageRangeType;
		dataWriter << m_strSpecificPages;
		dataWriter << m_bUseCleanedImageIfAvailable;

		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10250");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		bool bConfigured = true;
		if (m_eOCRPageRangeType == UCLID_FILEPROCESSORSLib::kOCRSpecifiedPages && m_strSpecificPages.empty())
		{
			bConfigured = false;
		}

		*pbValue = asVariantBool(bConfigured);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11037");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IOCRFileProcessor
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::get_OCRPageRangeType(EOCRFPPageRangeType *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		*pVal = (EOCRFPPageRangeType)m_eOCRPageRangeType;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11038");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::put_OCRPageRangeType(EOCRFPPageRangeType newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		m_eOCRPageRangeType = (UCLID_FILEPROCESSORSLib::EOCRFPPageRangeType)newVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11039");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::get_SpecificPages(BSTR *strSpecificPages)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		*strSpecificPages = _bstr_t(m_strSpecificPages.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11040");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::put_SpecificPages(BSTR strSpecificPages)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		// validate the string first
		string strDefinedPageNums = asString(strSpecificPages);
		::validatePageNumbers(strDefinedPageNums);

		m_strSpecificPages = strDefinedPageNums;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11041");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::get_UseCleanedImage(VARIANT_BOOL *pbUseCleaned)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		*pbUseCleaned = asVariantBool(m_bUseCleanedImageIfAvailable);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17458");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::put_UseCleanedImage(VARIANT_BOOL bUseCleaned)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_bUseCleanedImageIfAvailable = asCppBool(bUseCleaned);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17459");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void COCRFileProcessor::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnOCR_ON_SERVER_FEATURE;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI11042", "OCR File Processor");
}
//-------------------------------------------------------------------------------------------------
IOCREnginePtr COCRFileProcessor::getOCREngine()
{
	if (m_ipOCREngine == NULL)
	{
		m_ipOCREngine.CreateInstance(CLSID_ScansoftOCR);
		ASSERT_RESOURCE_ALLOCATION("ELI11048", m_ipOCREngine != NULL);

		_bstr_t _bstrPrivateLicenseCode = get_bstr_t(LICENSE_MGMT_PASSWORD.c_str());
		IPrivateLicensedComponentPtr ipScansoftEngine(m_ipOCREngine);
		ipScansoftEngine->InitPrivateLicense( _bstrPrivateLicenseCode );
	}
	return m_ipOCREngine;
}
//-------------------------------------------------------------------------------------------------

