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
const unsigned long gnCurrentVersion = 1;

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
		&IID_ILicensedComponent
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
STDMETHODIMP CAFEngineFileProcessor::raw_Init()
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
STDMETHODIMP CAFEngineFileProcessor::raw_ProcessFile(BSTR strFileFullName, 
	IFAMTagManager *pTagManager, IFileProcessingDB *pDB, IProgressStatus *pProgressStatus,
	VARIANT_BOOL bCancelRequested, VARIANT_BOOL *pbSuccessfulCompletion)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI17934", strFileFullName != NULL);
		ASSERT_ARGUMENT("ELI17935", pbSuccessfulCompletion != NULL);
		
		// Input file for processing
		string strInputFile = asString(strFileFullName);
		ASSERT_ARGUMENT("ELI17936", strInputFile.empty() == false);

		// Default to successful completion
		*pbSuccessfulCompletion = VARIANT_TRUE;

		::validateFileOrFolderExistence(strInputFile);

		// By default, the file to be processed is the input file name
		string strFileToBeProcessed(strInputFile);

		UCLID_AFCORELib::IAFDocumentPtr ipAFDoc(CLSID_AFDocument);
		ASSERT_RESOURCE_ALLOCATION("ELI19455", ipAFDoc != NULL);

		bool bNeedToRunOCR = false;

		// Check for the file type
		EFileType eFileType = getFileType(strInputFile);
		string strUSSFileName = strInputFile + ".uss";
		if (eFileType == kImageFile)
		{
			// Get the USS file name, assume that the USS file is in
			// the same folder as the image file.
			if (::isFileOrFolderValid(strUSSFileName))
			{
				// If input file is an image and if its associated USS file exists
				if (m_bReadUSSFileIfExist)
				{
					// If it's a valid file
					strFileToBeProcessed = strUSSFileName;
				}
			}
			else	// uss file doesn't exist
			{
				// user chooses to create the uss file
				if (m_bCreateUssFileIfNonExist)
				{
					// The spatial string can be directly loaded from AFDocument.
					// No need for the source doc any more.
					strFileToBeProcessed = "";

					bNeedToRunOCR = true;
				}
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

			// Perform the OCR as configured by the user
			switch (m_eOCRPagesType)
			{
			case kOCRAllPages:
				{
					// Recognize the image file and put the spatial string in ipAFDoc
					ipAFDoc->Text = getOCRUtils()->RecognizeTextInImageFile( get_bstr_t(strInputFile), 
						-1, getOCREngine(), ipProgressStatusToUseForSubTasks);
				}
				break;
			case kOCRCertainPages:
				{
					ipAFDoc->Text = getOCREngine()->RecognizeTextInImage2( get_bstr_t(strInputFile), 
						get_bstr_t(m_strSpecificPages), VARIANT_TRUE, ipProgressStatusToUseForSubTasks);
				}
				break;
			}

			// Output the spatial string to a USS file
			ISpatialStringPtr ipText = ipAFDoc->Text;
			ASSERT_RESOURCE_ALLOCATION("ELI15524", ipText != NULL);
			ipText->SaveTo(get_bstr_t(strUSSFileName.c_str()), 
				VARIANT_TRUE, VARIANT_TRUE);
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
		IRuleSetPtr ipRules( NULL );
		ipRules = getRuleSet();

		IUnknownPtr ipUnknown = ipRules;
		_variant_t _varRuleSet = (IUnknown *) ipUnknown;

		// Execute the rule set
		CComQIPtr<IAFDocument> ipDoc(ipAFDoc);
		getAFEngine()->FindAttributes(ipDoc, strFileToBeProcessed.c_str(), -1, 
			_varRuleSet, NULL, ipProgressStatusToUseForSubTasks);

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

		*pVal = get_bstr_t(m_strRuleFileNameForFileProcessing.c_str()).copy();
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

		string strFileName = asString(newVal);
		// file must exist
		::validateFileOrFolderExistence(strFileName);

		m_strRuleFileNameForFileProcessing = strFileName;
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

		*strSpecificPages = _bstr_t(m_strSpecificPages.c_str()).copy();
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
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineFileProcessor::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19540", pstrComponentDescription != NULL)

		*pstrComponentDescription = _bstr_t("Execute rules").Detach();
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
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10990");

	return S_OK;
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

		bool bConfigured = !m_strRuleFileNameForFileProcessing.empty();

		if (m_bCreateUssFileIfNonExist && m_eOCRPagesType == kOCRCertainPages)
		{
			bConfigured = bConfigured && !m_strSpecificPages.empty();
		}

		*pbValue = bConfigured ? VARIANT_TRUE : VARIANT_FALSE;
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
}
//-------------------------------------------------------------------------------------------------
void CAFEngineFileProcessor::validateLicense()
{
	static const unsigned long AF_ENGINE_FP_ID = gnFLEXINDEX_IDSHIELD_SERVER_CORE;

	VALIDATE_LICENSE( AF_ENGINE_FP_ID, "ELI10995", 
					"Attribute Finder Engine File Processor" );
}
//-------------------------------------------------------------------------------------------------
IRuleSetPtr CAFEngineFileProcessor::getRuleSet()
{
	m_ipRuleSet.loadObjectFromFile(m_strRuleFileNameForFileProcessing);

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
