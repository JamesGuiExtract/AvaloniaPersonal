// CleanupImage.cpp : Implementation of CCleanupImageFileProcessor

#include "stdafx.h"
#include "CleanupImageFileProcessor.h"
#include "FileProcessorsUtils.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>
#include <..\..\..\..\..\ProductDevelopment\AttributeFinder\AFCore\Code\Common.h>

#include <string>

using namespace std;

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
unsigned long gnCurrentVersion = 1;

//--------------------------------------------------------------------------------------------------
// CCleanupImageFileProcessor
//--------------------------------------------------------------------------------------------------
CCleanupImageFileProcessor::CCleanupImageFileProcessor() :
m_ipImageCleanupEngine(NULL),
m_ipMiscUtils(NULL),
m_strImageCleanupSettingsFileName(""),
m_bDirty(false)
{
	try
	{
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI17267");
}
//--------------------------------------------------------------------------------------------------
CCleanupImageFileProcessor::~CCleanupImageFileProcessor()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI17268");
}

//--------------------------------------------------------------------------------------------------
// ISupportErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CCleanupImageFileProcessor::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ILicensedComponent,
		&IID_ICategorizedComponent,
		&IID_IFileProcessingTask,
		&IID_ICopyableObject,
		&IID_IPersistStream,
		&IID_IMustBeConfiguredObject,
		&IID_ICleanupImageFileProcessor,
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
STDMETHODIMP CCleanupImageFileProcessor::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CCleanupImageFileProcessor::raw_Init(long nActionID, IFAMTagManager* pFAMTM,
	IFileProcessingDB *pDB)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17379");
	
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CCleanupImageFileProcessor::raw_ProcessFile(IFileRecord* pFileRecord,
	long nActionID, IFAMTagManager *pTagManager, IFileProcessingDB *pDB, IProgressStatus *pProgressStatus,
	VARIANT_BOOL bCancelRequested, EFileProcessingResult *pResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI17911", pTagManager != NULL);
		ASSERT_ARGUMENT("ELI17902", pResult != NULL);
		IFileRecordPtr ipFileRecord(pFileRecord);
		ASSERT_ARGUMENT("ELI31336", ipFileRecord != __nullptr);

		// get the input file name as a string
		string strImageFileName = asString(ipFileRecord->Name);
		ASSERT_ARGUMENT("ELI17913", strImageFileName.empty() == false);

		// verify the image file existence
		validateFileOrFolderExistence(strImageFileName);

		// expand the tags for the ImageCleanupSettingsFileName
		string strSettingsFileName = CFileProcessorsUtils::ExpandTagsAndTFE(pTagManager,
			m_strImageCleanupSettingsFileName, strImageFileName);

		// perform any appropriate auto-encrypt actions on the settings file
		getMiscUtils()->AutoEncryptFile(strSettingsFileName.c_str(),
			gstrAF_AUTO_ENCRYPT_KEY_PATH.c_str());

		// validate the existence of the settings file
		validateFileOrFolderExistence(strSettingsFileName);

		// get output file name
		string strOutputFileName = getCleanImageName(strImageFileName);

		// get a pointer to an ImageCleanupEngine Object
		IImageCleanupEnginePtr ipICEngine = getImageCleanupEngine();
		ASSERT_RESOURCE_ALLOCATION("ELI17290", ipICEngine != NULL);

		// call the CleanupImage operation on the image file 
		ipICEngine->CleanupImage(strImageFileName.c_str(), strOutputFileName.c_str(), strSettingsFileName.c_str());

		// successful completion
		*pResult = kProcessingSuccessful;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17385");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CCleanupImageFileProcessor::raw_Cancel()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17295");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCleanupImageFileProcessor::raw_Close()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17380");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IAccessRequired interface implementation
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCleanupImageFileProcessor::raw_RequiresAdminAccess(VARIANT_BOOL* pbResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI31177", pbResult != __nullptr);

		*pbResult = VARIANT_FALSE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31178");
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCleanupImageFileProcessor::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19610", pstrComponentDescription != NULL);

		*pstrComponentDescription = _bstr_t("Core: Cleanup image").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17296")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCleanupImageFileProcessor::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_FILEPROCESSORSLib::ICleanupImageFileProcessorPtr ipCopyThis(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI17297", ipCopyThis != NULL);

		// get the settings file name
		m_strImageCleanupSettingsFileName = asString(ipCopyThis->ImageCleanupSettingsFileName);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17299");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCleanupImageFileProcessor::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19911", pObject != NULL);

		// create an instance of the CleanupImageFileProcessor Object
		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_CleanupImageFileProcessor);
		ASSERT_RESOURCE_ALLOCATION("ELI17300", ipObjCopy != NULL);

		// copy from this to the new CleanupImageFileProcessor
		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17301");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCleanupImageFileProcessor::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_CleanupImageFileProcessor;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCleanupImageFileProcessor::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCleanupImageFileProcessor::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	INIT_EXCEPTION_AND_TRACING("MLI00012");

	try
	{
		// reset member variables
		m_strImageCleanupSettingsFileName = "";
		m_ipImageCleanupEngine = NULL;
		_lastCodePos = "10";

		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read(&nDataLength, sizeof(nDataLength), NULL);
		ByteStream data(nDataLength);
		pStream->Read(data.getData(), nDataLength, NULL);
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);
		_lastCodePos = "20";

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;
		_lastCodePos = "30";

		// Check for newer version
		if (nDataVersion > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue("ELI17302", "Unable to load newer OCRFileProcessor component.");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}
		
		// read the image cleanup settings file name from the bytestream
		dataReader >> m_strImageCleanupSettingsFileName;
		_lastCodePos = "40";

		// clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
		_lastCodePos = "50";
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17303");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCleanupImageFileProcessor::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	INIT_EXCEPTION_AND_TRACING("MLI00011");

	try
	{
		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);
		_lastCodePos = "10";

		// write version number
		dataWriter << gnCurrentVersion;
		_lastCodePos = "20";

		// write the settings file name
		dataWriter << m_strImageCleanupSettingsFileName;
		_lastCodePos = "30";

		dataWriter.flushToByteStream();
		_lastCodePos = "40";

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		_lastCodePos = "50";
		pStream->Write(data.getData(), nDataLength, NULL);
		_lastCodePos = "60";

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
		_lastCodePos = "70";
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17304");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCleanupImageFileProcessor::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCleanupImageFileProcessor::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		bool bConfigured = true;

		// if the image cleanup settings file name is "" then the user has not selected an
		// image cleanup settings file name
		if (m_strImageCleanupSettingsFileName == "")
		{
			bConfigured = false;
		}

		*pbValue = asVariantBool(bConfigured);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17305");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IImageCleanupFileProcessor
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCleanupImageFileProcessor::get_ImageCleanupSettingsFileName(BSTR* strFileName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		*strFileName = get_bstr_t(m_strImageCleanupSettingsFileName).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17357");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCleanupImageFileProcessor::put_ImageCleanupSettingsFileName(BSTR strFileName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_strImageCleanupSettingsFileName = asString(strFileName);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17350");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CCleanupImageFileProcessor::validateLicense()
{
	VALIDATE_LICENSE( gnIMAGE_CLEANUP_ENGINE_FEATURE, "ELI17306", "Clean Image File Processor" );
}
//-------------------------------------------------------------------------------------------------
IImageCleanupEnginePtr CCleanupImageFileProcessor::getImageCleanupEngine()
{
	if (m_ipImageCleanupEngine == NULL)
	{
		m_ipImageCleanupEngine.CreateInstance(CLSID_ImageCleanupEngine);
		ASSERT_RESOURCE_ALLOCATION("ELI17282", m_ipImageCleanupEngine != NULL);
	}

	return m_ipImageCleanupEngine;
}
//-------------------------------------------------------------------------------------------------
IMiscUtilsPtr CCleanupImageFileProcessor::getMiscUtils()
{
	if (m_ipMiscUtils == NULL)
	{
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI17384", m_ipMiscUtils != NULL );
	}

	return m_ipMiscUtils;
}
//-------------------------------------------------------------------------------------------------
