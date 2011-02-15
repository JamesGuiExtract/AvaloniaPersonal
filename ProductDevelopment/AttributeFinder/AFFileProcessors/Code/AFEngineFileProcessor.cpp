// AFEngineFileProcessor.cpp : Implementation of CAFEngineFileProcessor
#include "stdafx.h"
#include "AFFileProcessors.h"
#include "AFEngineFileProcessor.h"

#include <UCLIDException.h>
#include <TemporaryResourceOverride.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <RegistryPersistenceMgr.h>
#include <ComUtils.h>
#include <Misc.h>
#include <ComponentLicenseIDs.h>

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Version history:
// 1 - First version of this object
// 2 - Added m_bUseCleanedImage option to object
//     Also added support for ocr type: kNoOCR
const unsigned long gnCurrentVersion = 2;

//-------------------------------------------------------------------------------------------------
// CAFEngineFileProcessor
//-------------------------------------------------------------------------------------------------
CAFEngineFileProcessor::CAFEngineFileProcessor()
:	m_bDirty(false)
{
	clear();

	m_ipRuleSet.m_obj.CreateInstance( CLSID_RuleSet );
	ASSERT_RESOURCE_ALLOCATION( "ELI11558", m_ipRuleSet.m_obj != NULL );
}
//-------------------------------------------------------------------------------------------------
CAFEngineFileProcessor::~CAFEngineFileProcessor()
{
	try
	{
		// Release COM objects
		m_ipOCRUtils = NULL;
		m_ipOCREngine = NULL;
		m_ipAFEngine = NULL;

		// Clear the rule set cache (releases the internal COM object)
		m_ipRuleSet.Clear();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20388");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineFileProcessor::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IAFEngineFileProcessor,
		&IID_IFileProcessingTask,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_IMustBeConfiguredObject,
		&IID_ILicensedComponent,
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
// IFileProcessingTask
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineFileProcessor::raw_Init(long nActionID, IFAMTagManager* pFAMTM,
	IFileProcessingDB *pDB)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17792");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineFileProcessor::raw_ProcessFile(IFileRecord* pFileRecord, long nActionID,
	IFAMTagManager *pTagManager, IFileProcessingDB *pDB, IProgressStatus *pProgressStatus,
	VARIANT_BOOL bCancelRequested, EFileProcessingResult *pResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		IFAMTagManagerPtr ipTagManager(pTagManager);
		ASSERT_ARGUMENT("ELI26658", ipTagManager != NULL);
		ASSERT_ARGUMENT("ELI17935", pResult != NULL);
		
		IFileRecordPtr ipFileRecord(pFileRecord);
		ASSERT_ARGUMENT("ELI31334", ipFileRecord != __nullptr);

		// Input file for processing
		string strInputFile = asString(ipFileRecord->Name);
		ASSERT_ARGUMENT("ELI17936", strInputFile.empty() == false);

		// Expand the tags for the rules file
		string strRulesFile = asString(
			ipTagManager->ExpandTags(m_strRuleFileNameForFileProcessing.c_str(),
			strInputFile.c_str()));

		// Default to successful completion
		*pResult = kProcessingSuccessful;

		// Validate the input file and the rules file
		validateFileOrFolderExistence(strInputFile, "ELI26659");
		validateFileOrFolderExistence(strRulesFile, "ELI26660");

		// Create a new AFDoc pointer
		IAFDocumentPtr ipAFDoc(CLSID_AFDocument);
		ASSERT_RESOURCE_ALLOCATION("ELI19455", ipAFDoc != NULL);

		// If reading the USS file from disk, check for USS file
		// default the need to OCR to true
		bool bNeedToRunOCR = true;
		if (m_bReadUSSFileIfExist)
		{
			EFileType eFileType = getFileType(strInputFile);
			string strSpatialStringFile = (eFileType == kUSSFile)
				? strInputFile : strInputFile + ".uss";

			if (!strSpatialStringFile.empty() && isValidFile(strSpatialStringFile))
			{
				ISpatialStringPtr ipText = ipAFDoc->Text;
				ASSERT_RESOURCE_ALLOCATION("ELI28088", ipText != NULL);

				// Load the spatial string from the file
				ipText->LoadFrom(strSpatialStringFile.c_str(), VARIANT_FALSE);

				// Set the need to OCR flag to false
				bNeedToRunOCR = false;
			}
		}

		// Based upon whether OCR will need to be run, determine the total number
		// of items to use for progress status updates
		const long nNUM_PROGRESS_ITEMS_OCR = 1;
		const long nNUM_PROGRESS_ITEMS_EXECUTE_RULES = 1;
		const long nMAX_PROGRESS_ITEMS = nNUM_PROGRESS_ITEMS_OCR + nNUM_PROGRESS_ITEMS_EXECUTE_RULES;
		const long nTOTAL_PROGRESS_ITEMS = bNeedToRunOCR ? nMAX_PROGRESS_ITEMS : nNUM_PROGRESS_ITEMS_EXECUTE_RULES;

		// Wrap the progress status object in a smart pointer
		IProgressStatusPtr ipProgressStatus(pProgressStatus);

		// Since this object at runtime may only execute one sub task or more than one
		// sub task, determine which progress status object should be passed to the sub tasks
		IProgressStatusPtr ipProgressStatusToUseForSubTasks = NULL;
		if (ipProgressStatus)
		{
			if (nTOTAL_PROGRESS_ITEMS > 1)
			{
				// There are multiple progress items associated with this task.  Pass
				// the sub progress status object to each sub task.
				ipProgressStatus->InitProgressStatus("Initializing rules execution...", 0, 
					nTOTAL_PROGRESS_ITEMS, VARIANT_TRUE);
				ipProgressStatusToUseForSubTasks = ipProgressStatus->SubProgressStatus;
			}
			else if (nTOTAL_PROGRESS_ITEMS == 1)
			{
				// There is only one progress item for this task.  Just pass this
				// Progress status object to the sub task
				ipProgressStatusToUseForSubTasks = ipProgressStatus;
			}
			else
			{
				// We should never reach here.  Log an exception
				UCLIDException ue("ELI16259", "Internal logic error state recorded.");
				ue.addDebugInfo("nTOTAL_PROGRESS_ITEMS", nTOTAL_PROGRESS_ITEMS);
				ue.log();
			}
		}

		// Perform OCR on the document if necessary
		if (bNeedToRunOCR)
		{
			// Update the progress status
			if (ipProgressStatus && nTOTAL_PROGRESS_ITEMS > 1)
			{
				ipProgressStatus->StartNextItemGroup("Performing OCR...", nNUM_PROGRESS_ITEMS_OCR);
			}

			EFileType eFileType = getFileType(strInputFile);
			if (eFileType == kImageFile)
			{
				// Get the name of the file to OCR
				string strFileToOCR =
					m_bUseCleanedImage ? getCleanImageNameIfExists(strInputFile) : strInputFile;

				// Perform the OCR as configured by the user
				switch (m_eOCRPagesType)
				{
				case kOCRAllPages:
					{
						// Recognize the image file and put the spatial string in ipAFDoc
						ipAFDoc->Text = getOCRUtils()->RecognizeTextInImageFile( strFileToOCR.c_str(), 
							-1, getOCREngine(), ipProgressStatusToUseForSubTasks);
					}
					break;
				case kOCRCertainPages:
					{
						ipAFDoc->Text = getOCREngine()->RecognizeTextInImage2( strFileToOCR.c_str(), 
							m_strSpecificPages.c_str(), VARIANT_TRUE, ipProgressStatusToUseForSubTasks);
					}
					break;
				}
			}
			// Text file
			else
			{
				ISpatialStringPtr ipText = ipAFDoc->Text;
				ASSERT_RESOURCE_ALLOCATION("ELI31688", ipText != __nullptr);

				// Load a spatial string as indexed text
				ipText->LoadFrom(strInputFile.c_str(), VARIANT_FALSE);
			}

			// Ensure the appropriate source doc name is in the spatial string
			if (m_bUseCleanedImage || m_eOCRPagesType == kNoOCR)
			{
				ISpatialStringPtr ipText = ipAFDoc->Text;
				ASSERT_RESOURCE_ALLOCATION("ELI28151", ipText != NULL);

				ipText->SourceDocName = strInputFile.c_str();
			}

			if (m_bCreateUssFileIfNonExist && m_eOCRPagesType != kNoOCR)
			{
				// Output the spatial string to a USS file
				ISpatialStringPtr ipText = ipAFDoc->Text;
				ASSERT_RESOURCE_ALLOCATION("ELI15524", ipText != NULL);
				ipText->SaveTo(get_bstr_t(strInputFile + ".uss"), VARIANT_TRUE, VARIANT_TRUE);
			}
		}

		// Update the progress status to indicate that the next item group
		// (associated with executing rules) is about to start.
		if (ipProgressStatus && nTOTAL_PROGRESS_ITEMS > 1)
		{
			ipProgressStatus->StartNextItemGroup("Executing rules...", nNUM_PROGRESS_ITEMS_EXECUTE_RULES);
		}

		// Make sure the rule set file exists.
		// The rule set is saved in this object so that it can be passed in to AFEngine
		// and so that it need not be loaded each time this method is called.
		IRuleSetPtr ipRules = getRuleSet(strRulesFile);

		IUnknownPtr ipUnknown = ipRules;
		_variant_t _varRuleSet = (IUnknown *) ipUnknown;

		// Execute the rule set
		getAFEngine()->FindAttributes(ipAFDoc, strInputFile.c_str(), 0, 
			_varRuleSet, NULL, VARIANT_TRUE, ipProgressStatusToUseForSubTasks);

		// Update the progress status to indicate that rule running is complete.
		if (ipProgressStatus && nTOTAL_PROGRESS_ITEMS > 1)
		{
			ipProgressStatus->CompleteCurrentItemGroup();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08998")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineFileProcessor::raw_Cancel()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17791");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineFileProcessor::raw_Close()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17790");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IAccessRequired interface implementation
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineFileProcessor::raw_RequiresAdminAccess(VARIANT_BOOL* pbResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI31171", pbResult != __nullptr);

		*pbResult = VARIANT_FALSE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31172");
}

//-------------------------------------------------------------------------------------------------
// IAFEngineFileProcessor
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineFileProcessor::get_ReadUSSFile(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bReadUSSFileIfExist ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09006")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineFileProcessor::put_ReadUSSFile(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bReadUSSFileIfExist = newVal == VARIANT_TRUE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09007")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineFileProcessor::get_CreateUSSFile(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bCreateUssFileIfNonExist ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09008")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineFileProcessor::put_CreateUSSFile(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bCreateUssFileIfNonExist = newVal == VARIANT_TRUE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09009")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineFileProcessor::get_RuleSetFileName(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = _bstr_t(m_strRuleFileNameForFileProcessing.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09010")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineFileProcessor::put_RuleSetFileName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// Set the file name (only validate file name when processing [FlexIDSCore #3575])
		m_strRuleFileNameForFileProcessing = asString(newVal);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09011")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineFileProcessor::get_OCRPagesType(EOCRPagesType *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_eOCRPagesType;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10292")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineFileProcessor::put_OCRPagesType(EOCRPagesType newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_eOCRPagesType = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10293")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineFileProcessor::get_OCRCertainPages(BSTR *strSpecificPages)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*strSpecificPages = _bstr_t(m_strSpecificPages.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10294")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineFileProcessor::put_OCRCertainPages(BSTR strSpecificPages)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		string strPages = asString(strSpecificPages);
		::validatePageNumbers(strPages);

		m_strSpecificPages = strPages;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10295")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineFileProcessor::get_UseCleanedImage(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI28070", pVal != NULL);

		*pVal = asVariantBool(m_bUseCleanedImage);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28071")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineFileProcessor::put_UseCleanedImage(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Get the new value
		bool bNewVal = asCppBool(newVal);

		// If the new value is not the same as the original value then
		// set the new value and set the dirty flag
		if (bNewVal != m_bUseCleanedImage)
		{
			m_bUseCleanedImage = bNewVal;
			m_bDirty = true;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28072")
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineFileProcessor::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19540", pstrComponentDescription != NULL);

		*pstrComponentDescription = _bstr_t("Core: Execute rules").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12809");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineFileProcessor::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		UCLID_AFFILEPROCESSORSLib::IAFEngineFileProcessorPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI10991", ipSource != NULL);

		m_strRuleFileNameForFileProcessing = ipSource->RuleSetFileName;
		m_bReadUSSFileIfExist = ipSource->ReadUSSFile == VARIANT_TRUE;
		m_bCreateUssFileIfNonExist = ipSource->CreateUSSFile == VARIANT_TRUE;
		m_eOCRPagesType = (EOCRPagesType)ipSource->OCRPagesType;
		m_strSpecificPages = ipSource->OCRCertainPages;
		m_bUseCleanedImage = ipSource->UseCleanedImage == VARIANT_TRUE;

		// Set dirty flag since this object has changed
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10990");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineFileProcessor::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		// create another instance of this object
		ICopyableObjectPtr ipObjCopy(CLSID_AFEngineFileProcessor);
		ASSERT_RESOURCE_ALLOCATION("ELI10993", ipObjCopy != NULL);

		IUnknownPtr ipUnk(this);
		ipObjCopy->CopyFrom(ipUnk);
	
		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10992");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineFileProcessor::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Object is configured if:
		// 1. The rules file name is not empty
		// 2. The ocr type is not Certain pages OR the specific pages string is not empty
		bool bConfigured = !m_strRuleFileNameForFileProcessing.empty()
			&& (m_eOCRPagesType != kOCRCertainPages || !m_strSpecificPages.empty());

		*pbValue = asVariantBool(bConfigured);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10994");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineFileProcessor::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineFileProcessor::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_AFEngineFileProcessor;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineFileProcessor::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineFileProcessor::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Reset member variables
		clear();

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
			UCLIDException ue( "ELI10996", 
				"Unable to load newer Run Object On Query Output Handler!" );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		dataReader >> m_strRuleFileNameForFileProcessing;

		// read the uss file if exist
		dataReader >> m_bReadUSSFileIfExist;

		// create uss file if it doesn't exist
		dataReader >> m_bCreateUssFileIfNonExist;

		long nTmp;
		dataReader >> nTmp;
		m_eOCRPagesType = (EOCRPagesType)nTmp;

		dataReader >> m_strSpecificPages;

		if (nDataVersion >= 2)
		{
			dataReader >> m_bUseCleanedImage;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10997");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineFileProcessor::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter( ByteStreamManipulator::kWrite, data );
		dataWriter << gnCurrentVersion;

		dataWriter << m_strRuleFileNameForFileProcessing;

		// read the uss file if exist
		dataWriter << m_bReadUSSFileIfExist;

		// create uss file if it doesn't exist
		dataWriter << m_bCreateUssFileIfNonExist;

		dataWriter << (long)m_eOCRPagesType;

		dataWriter << m_strSpecificPages;

		dataWriter << m_bUseCleanedImage;
		
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), NULL );
		pStream->Write( data.getData(), nDataLength, NULL );

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10998");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineFileProcessor::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CAFEngineFileProcessor::clear()
{
	m_strRuleFileNameForFileProcessing = "";
	m_bReadUSSFileIfExist = false;
	m_bCreateUssFileIfNonExist = false;
	m_eOCRPagesType = kOCRAllPages;
	m_strSpecificPages = "";
	m_bUseCleanedImage = true;
}
//-------------------------------------------------------------------------------------------------
void CAFEngineFileProcessor::validateLicense()
{
	static const unsigned long AF_ENGINE_FP_ID = gnFLEXINDEX_IDSHIELD_SERVER_CORE;

	VALIDATE_LICENSE( AF_ENGINE_FP_ID, "ELI10995", 
					"Attribute Finder Engine File Processor" );
}
//-------------------------------------------------------------------------------------------------
IRuleSetPtr CAFEngineFileProcessor::getRuleSet(const string& strRulesFile)
{
	m_ipRuleSet.loadObjectFromFile(strRulesFile);

	return m_ipRuleSet.m_obj;
}
//-------------------------------------------------------------------------------------------------
IOCREnginePtr CAFEngineFileProcessor::getOCREngine()
{
	if (m_ipOCREngine == NULL)
	{
		m_ipOCREngine.CreateInstance( CLSID_ScansoftOCR );
		ASSERT_RESOURCE_ALLOCATION( "ELI19134", m_ipOCREngine != NULL );
		
		_bstr_t _bstrPrivateLicenseCode = get_bstr_t(LICENSE_MGMT_PASSWORD.c_str());
		IPrivateLicensedComponentPtr ipScansoftEngine(m_ipOCREngine);
		ASSERT_RESOURCE_ALLOCATION("ELI20468", ipScansoftEngine != NULL);
		ipScansoftEngine->InitPrivateLicense( _bstrPrivateLicenseCode );
	}

	return m_ipOCREngine;
}
//-------------------------------------------------------------------------------------------------
IOCRUtilsPtr CAFEngineFileProcessor::getOCRUtils()
{
	if (m_ipOCRUtils == NULL)
	{
		m_ipOCRUtils.CreateInstance(CLSID_OCRUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI19135", m_ipOCRUtils != NULL);
	}

	return m_ipOCRUtils;
}
//-------------------------------------------------------------------------------------------------
IAttributeFinderEnginePtr CAFEngineFileProcessor::getAFEngine()
{
	if (m_ipAFEngine == NULL)
	{
		m_ipAFEngine.CreateInstance(CLSID_AttributeFinderEngine);
		ASSERT_RESOURCE_ALLOCATION("ELI11112", m_ipAFEngine != NULL);
	}
	return m_ipAFEngine;
}
//-------------------------------------------------------------------------------------------------
